using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.OSM.Models;

/// <summary>
/// Represents a location search result from Nominatim
/// </summary>
public class LocationSearchResult
{
    /// <summary>
    /// Display name of the location
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Geographic location
    /// </summary>
    public GeoLocation Location { get; set; } = new();
    
    /// <summary>
    /// Bounding box of the location
    /// </summary>
    public BoundingBox? BoundingBox { get; set; }
    
    /// <summary>
    /// Type of location (city, state, country, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Importance score (0.0-1.0)
    /// </summary>
    public double Importance { get; set; }
    
    /// <summary>
    /// Place ID from Nominatim
    /// </summary>
    public long PlaceId { get; set; }
}