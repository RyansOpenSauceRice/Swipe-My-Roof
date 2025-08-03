using Avalonia.Media.Imaging;
using ReactiveUI;
using SwipeMyRoof.Images.Models;
using SwipeMyRoof.Images.Services;
using SwipeMyRoof.OSM.Models;
using SwipeMyRoof.AvaloniaUI.Controls;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace SwipeMyRoof.AvaloniaUI.ViewModels;

/// <summary>
/// ViewModel for building map control with satellite imagery and overlays
/// </summary>
public class BuildingMapViewModel : ViewModelBase
{
    private readonly IBingMapsService _bingMapsService;
    private readonly IBuildingOverlayService _overlayService;
    
    // Map state
    private Bitmap? _satelliteImageSource;
    private BuildingOverlay _currentOverlay = new();
    private BoundingBox _currentBounds = new();
    private int _currentZoomLevel = 18;
    private BingImageryType _currentImageryType = BingImageryType.Aerial;
    
    // UI state
    private bool _isLoadingMap;
    private string _loadingMessage = "Loading map...";
    private bool _hasError;
    private string _errorMessage = string.Empty;
    private bool _enableSwipeGestures = true;
    private bool _showBuildingInfo = true;
    
    // Building info
    private string _buildingId = string.Empty;
    private string _buildingArea = string.Empty;
    private string _proposedColor = string.Empty;
    private string _attributionText = string.Empty;
    
    public BuildingMapViewModel(IBingMapsService bingMapsService, IBuildingOverlayService overlayService)
    {
        _bingMapsService = bingMapsService;
        _overlayService = overlayService;
        
        // Initialize commands
        ZoomInCommand = ReactiveCommand.CreateFromTask(ZoomInAsync);
        ZoomOutCommand = ReactiveCommand.CreateFromTask(ZoomOutAsync);
        CenterOnBuildingCommand = ReactiveCommand.CreateFromTask(CenterOnBuildingAsync);
        ToggleImageryTypeCommand = ReactiveCommand.Create(ToggleImageryType);
        RetryLoadMapCommand = ReactiveCommand.CreateFromTask(LoadMapAsync);
        
        // Set attribution
        _attributionText = _bingMapsService.GetAttributionText();
    }
    
    #region Properties
    
    public Bitmap? SatelliteImageSource
    {
        get => _satelliteImageSource;
        set => this.RaiseAndSetIfChanged(ref _satelliteImageSource, value);
    }
    
    public bool HasSatelliteImage => SatelliteImageSource != null;
    
    public bool HasBuildingOverlay => _currentOverlay.OutlinePixels.Count > 0;
    
    public bool IsLoadingMap
    {
        get => _isLoadingMap;
        set => this.RaiseAndSetIfChanged(ref _isLoadingMap, value);
    }
    
    public string LoadingMessage
    {
        get => _loadingMessage;
        set => this.RaiseAndSetIfChanged(ref _loadingMessage, value);
    }
    
    public bool HasError
    {
        get => _hasError;
        set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }
    
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }
    
    public bool EnableSwipeGestures
    {
        get => _enableSwipeGestures;
        set => this.RaiseAndSetIfChanged(ref _enableSwipeGestures, value);
    }
    
    public bool ShowBuildingInfo
    {
        get => _showBuildingInfo;
        set => this.RaiseAndSetIfChanged(ref _showBuildingInfo, value);
    }
    
    public string BuildingId
    {
        get => _buildingId;
        set => this.RaiseAndSetIfChanged(ref _buildingId, value);
    }
    
    public string BuildingArea
    {
        get => _buildingArea;
        set => this.RaiseAndSetIfChanged(ref _buildingArea, value);
    }
    
    public string ProposedColor
    {
        get => _proposedColor;
        set => this.RaiseAndSetIfChanged(ref _proposedColor, value);
    }
    
    public string AttributionText
    {
        get => _attributionText;
        set => this.RaiseAndSetIfChanged(ref _attributionText, value);
    }
    
    public bool HasAttribution => !string.IsNullOrEmpty(AttributionText);
    
    #endregion
    
    #region Commands
    
    public ReactiveCommand<Unit, Unit> ZoomInCommand { get; }
    public ReactiveCommand<Unit, Unit> ZoomOutCommand { get; }
    public ReactiveCommand<Unit, Unit> CenterOnBuildingCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleImageryTypeCommand { get; }
    public ReactiveCommand<Unit, Unit> RetryLoadMapCommand { get; }
    
    #endregion
    
    #region Events
    
    public event EventHandler<BuildingOverlay>? BuildingOverlayChanged;
    public event EventHandler<SwipeDirection>? SwipeGestureDetected;
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Load building imagery and overlay
    /// </summary>
    /// <param name="building">Building to display</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task LoadBuildingAsync(OsmBuilding building, CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoadingMap = true;
            HasError = false;
            LoadingMessage = "Loading building imagery...";
            
            // Calculate building bounds
            _currentBounds = CalculateBuildingBounds(building);
            
            // Load building imagery from Bing Maps
            var imagery = await _bingMapsService.GetBuildingImageryAsync(_currentBounds, 512, 512, cancellationToken);
            
            // Convert to Avalonia bitmap
            using var stream = new MemoryStream(imagery.CompositeImageData);
            SatelliteImageSource = new Bitmap(stream);
            
            // Update overlay
            _currentOverlay = imagery.BuildingOverlay;
            BuildingOverlayChanged?.Invoke(this, _currentOverlay);
            
            // Update building info
            BuildingId = $"Building ID: {building.OsmId}";
            BuildingArea = $"Area: {_currentOverlay.AreaSquareMeters:F0} m²";
            ProposedColor = $"Proposed: {building.ProposedRoofColor ?? "Unknown"}";
            
            // Notify property changes
            this.RaisePropertyChanged(nameof(HasSatelliteImage));
            this.RaisePropertyChanged(nameof(HasBuildingOverlay));
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load building imagery: {ex.Message}";
        }
        finally
        {
            IsLoadingMap = false;
        }
    }
    
    /// <summary>
    /// Handle swipe gesture
    /// </summary>
    /// <param name="direction">Swipe direction</param>
    public void OnSwipeGesture(SwipeDirection direction)
    {
        SwipeGestureDetected?.Invoke(this, direction);
    }
    
    /// <summary>
    /// Handle swipe preview (for visual feedback)
    /// </summary>
    /// <param name="direction">Preview direction</param>
    public void OnSwipePreview(SwipeDirection direction)
    {
        // Could add visual feedback here (e.g., highlight accept/reject areas)
    }
    
    #endregion
    
    #region Private Methods
    
    private async Task LoadMapAsync()
    {
        if (_currentBounds.IsEmpty()) return;
        
        try
        {
            IsLoadingMap = true;
            HasError = false;
            LoadingMessage = "Reloading map...";
            
            // Reload imagery
            var imagery = await _bingMapsService.GetBuildingImageryAsync(_currentBounds, 512, 512);
            
            using var stream = new MemoryStream(imagery.CompositeImageData);
            SatelliteImageSource = new Bitmap(stream);
            
            this.RaisePropertyChanged(nameof(HasSatelliteImage));
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to reload map: {ex.Message}";
        }
        finally
        {
            IsLoadingMap = false;
        }
    }
    
    private async Task ZoomInAsync()
    {
        if (_currentZoomLevel >= 21) return; // Max zoom level
        
        _currentZoomLevel++;
        await LoadMapAsync();
    }
    
    private async Task ZoomOutAsync()
    {
        if (_currentZoomLevel <= 10) return; // Min zoom level
        
        _currentZoomLevel--;
        await LoadMapAsync();
    }
    
    private async Task CenterOnBuildingAsync()
    {
        // Re-center the map on the building
        await LoadMapAsync();
    }
    
    private void ToggleImageryType()
    {
        _currentImageryType = _currentImageryType switch
        {
            BingImageryType.Aerial => BingImageryType.Road,  // Aerial → Road for area selection
            BingImageryType.Road => BingImageryType.Aerial,  // Road → Aerial for color analysis
            _ => BingImageryType.Aerial
        };
        
        // Reload with new imagery type
        _ = LoadMapAsync();
    }
    
    private static BoundingBox CalculateBuildingBounds(OsmBuilding building)
    {
        if (building.Geometry?.Coordinates?.Any() != true)
        {
            return new BoundingBox();
        }
        
        var coordinates = building.Geometry.Coordinates;
        var minLat = coordinates.Min(c => c.Lat);
        var maxLat = coordinates.Max(c => c.Lat);
        var minLon = coordinates.Min(c => c.Lon);
        var maxLon = coordinates.Max(c => c.Lon);
        
        // Add padding around the building
        var latPadding = (maxLat - minLat) * 0.5; // 50% padding
        var lonPadding = (maxLon - minLon) * 0.5;
        
        return new BoundingBox
        {
            North = maxLat + latPadding,
            South = minLat - latPadding,
            East = maxLon + lonPadding,
            West = minLon - lonPadding
        };
    }
    
    #endregion
}

/// <summary>
/// Extension methods for BoundingBox
/// </summary>
public static class BoundingBoxExtensions
{
    public static bool IsEmpty(this BoundingBox bounds)
    {
        return bounds.North == 0 && bounds.South == 0 && bounds.East == 0 && bounds.West == 0;
    }
}