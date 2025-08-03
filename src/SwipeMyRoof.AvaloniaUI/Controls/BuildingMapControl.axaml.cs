using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SwipeMyRoof.AvaloniaUI.ViewModels;
using SwipeMyRoof.Images.Models;
using SkiaSharp;
using System.IO;

namespace SwipeMyRoof.AvaloniaUI.Controls;

/// <summary>
/// Building map control with satellite imagery and building overlays (following GoMap's approach)
/// </summary>
public partial class BuildingMapControl : UserControl
{
    private BuildingMapViewModel? _viewModel;
    private Canvas? _overlayCanvas;
    private Image? _satelliteImage;
    private Border? _gestureOverlay;
    
    // Gesture tracking
    private Point _lastPointerPosition;
    private bool _isPointerPressed;
    private DateTime _pointerPressTime;
    private const double SwipeThreshold = 100; // Minimum distance for swipe
    private const double SwipeTimeThreshold = 500; // Maximum time for swipe (ms)
    
    public BuildingMapControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Get references to named controls
        _overlayCanvas = this.FindControl<Canvas>("BuildingOverlayCanvas");
        _satelliteImage = this.FindControl<Image>("SatelliteImage");
        _gestureOverlay = this.FindControl<Border>("GestureOverlay");
        
        // Set up gesture handling
        if (_gestureOverlay != null)
        {
            _gestureOverlay.PointerPressed += OnPointerPressed;
            _gestureOverlay.PointerMoved += OnPointerMoved;
            _gestureOverlay.PointerReleased += OnPointerReleased;
        }
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.BuildingOverlayChanged -= OnBuildingOverlayChanged;
        }
        
        _viewModel = DataContext as BuildingMapViewModel;
        
        if (_viewModel != null)
        {
            _viewModel.BuildingOverlayChanged += OnBuildingOverlayChanged;
        }
    }
    
    private void OnBuildingOverlayChanged(object? sender, BuildingOverlay overlay)
    {
        // For roof color analysis, keep the overlay canvas clean
        // Any visual overlays would interfere with accurate color perception
        if (_overlayCanvas == null) return;
        
        _overlayCanvas.Children.Clear();
        
        // Building outline data is still available for programmatic use
        // but not visually rendered to avoid color interference
    }
    
    #region Gesture Handling (Swipe Detection)
    
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_viewModel?.EnableSwipeGestures != true) return;
        
        _isPointerPressed = true;
        _lastPointerPosition = e.GetPosition(this);
        _pointerPressTime = DateTime.Now;
        
        // Capture pointer for gesture tracking
        if (sender is Control control)
        {
            control.CapturePointer(e.Pointer);
        }
    }
    
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPointerPressed || _viewModel?.EnableSwipeGestures != true) return;
        
        // Track movement for potential swipe gesture
        var currentPosition = e.GetPosition(this);
        var deltaX = currentPosition.X - _lastPointerPosition.X;
        var deltaY = currentPosition.Y - _lastPointerPosition.Y;
        
        // Provide visual feedback for swipe direction
        if (Math.Abs(deltaX) > 20) // Minimum movement for feedback
        {
            _viewModel?.OnSwipePreview(deltaX > 0 ? SwipeDirection.Right : SwipeDirection.Left);
        }
    }
    
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPointerPressed || _viewModel?.EnableSwipeGestures != true) return;
        
        _isPointerPressed = false;
        
        var currentPosition = e.GetPosition(this);
        var deltaX = currentPosition.X - _lastPointerPosition.X;
        var deltaY = currentPosition.Y - _lastPointerPosition.Y;
        var deltaTime = (DateTime.Now - _pointerPressTime).TotalMilliseconds;
        
        // Check if this qualifies as a swipe gesture
        var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        
        if (distance >= SwipeThreshold && 
            deltaTime <= SwipeTimeThreshold && 
            Math.Abs(deltaX) > Math.Abs(deltaY)) // Horizontal swipe
        {
            var direction = deltaX > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            _viewModel?.OnSwipeGesture(direction);
        }
        
        // Release pointer capture
        if (sender is Control control)
        {
            control.ReleasePointerCapture(e.Pointer);
        }
        
        // Clear any preview feedback
        _viewModel?.OnSwipePreview(SwipeDirection.None);
    }
    
    #endregion
}

/// <summary>
/// Swipe direction enumeration
/// </summary>
public enum SwipeDirection
{
    None,
    Left,
    Right,
    Up,
    Down
}