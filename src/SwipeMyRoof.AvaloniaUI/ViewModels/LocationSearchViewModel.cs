using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;
using SwipeMyRoof.OSM.Services;

namespace SwipeMyRoof.AvaloniaUI.ViewModels;

/// <summary>
/// View model for location search functionality
/// </summary>
public class LocationSearchViewModel : ViewModelBase
{
    private readonly ILocationSearchService _locationSearchService;
    private string _searchQuery = string.Empty;
    private bool _isSearching = false;
    private LocationSearchResult? _selectedLocation;
    private string _statusMessage = "Enter a location to search";
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="locationSearchService">Location search service</param>
    public LocationSearchViewModel(ILocationSearchService locationSearchService)
    {
        _locationSearchService = locationSearchService;
        
        SearchResults = new ObservableCollection<LocationSearchResult>();
        
        // Commands
        SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync, this.WhenAnyValue(x => x.SearchQuery).Select(q => !string.IsNullOrWhiteSpace(q)));
        SelectLocationCommand = ReactiveCommand.Create<LocationSearchResult>(SelectLocation);
        ClearSearchCommand = ReactiveCommand.Create(ClearSearch);
    }
    
    /// <summary>
    /// Search query text
    /// </summary>
    public string SearchQuery
    {
        get => _searchQuery;
        set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
    }
    
    /// <summary>
    /// Whether a search is currently in progress
    /// </summary>
    public bool IsSearching
    {
        get => _isSearching;
        set => this.RaiseAndSetIfChanged(ref _isSearching, value);
    }
    
    /// <summary>
    /// Currently selected location
    /// </summary>
    public LocationSearchResult? SelectedLocation
    {
        get => _selectedLocation;
        set => this.RaiseAndSetIfChanged(ref _selectedLocation, value);
    }
    
    /// <summary>
    /// Status message to display
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    
    /// <summary>
    /// Search results
    /// </summary>
    public ObservableCollection<LocationSearchResult> SearchResults { get; }
    
    /// <summary>
    /// Command to perform search
    /// </summary>
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    
    /// <summary>
    /// Command to select a location
    /// </summary>
    public ReactiveCommand<LocationSearchResult, Unit> SelectLocationCommand { get; }
    
    /// <summary>
    /// Command to clear search results
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClearSearchCommand { get; }
    
    /// <summary>
    /// Event fired when a location is selected
    /// </summary>
    public event EventHandler<LocationSelectedEventArgs>? LocationSelected;
    
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
            return;
        
        try
        {
            IsSearching = true;
            StatusMessage = "Searching...";
            SearchResults.Clear();
            
            var results = await _locationSearchService.SearchLocationsAsync(SearchQuery, 10);
            
            if (results.Count > 0)
            {
                foreach (var result in results)
                {
                    SearchResults.Add(result);
                }
                StatusMessage = $"Found {results.Count} location(s)";
            }
            else
            {
                StatusMessage = "No locations found. Try a different search term.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }
    
    private void SelectLocation(LocationSearchResult location)
    {
        SelectedLocation = location;
        
        var args = new LocationSelectedEventArgs
        {
            Location = location,
            AreaSelection = CreateAreaSelectionFromLocation(location)
        };
        
        LocationSelected?.Invoke(this, args);
        
        StatusMessage = $"Selected: {location.DisplayName}";
    }
    
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        SearchResults.Clear();
        SelectedLocation = null;
        StatusMessage = "Enter a location to search";
    }
    
    private static AreaSelection CreateAreaSelectionFromLocation(LocationSearchResult location)
    {
        // Create a 5km radius area around the selected location
        return new AreaSelection
        {
            Type = AreaSelectionType.Radius,
            Center = location.Location,
            Radius = 5000 // 5km in meters
        };
    }
}

/// <summary>
/// Event arguments for location selected event
/// </summary>
public class LocationSelectedEventArgs : EventArgs
{
    public LocationSearchResult Location { get; set; } = null!;
    public AreaSelection AreaSelection { get; set; } = null!;
}