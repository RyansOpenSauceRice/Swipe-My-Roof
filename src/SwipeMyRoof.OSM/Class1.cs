using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.OSM.Models;

/// <summary>
/// Represents an OpenStreetMap building
/// </summary>
public class OsmBuilding
{
    /// <summary>
    /// OpenStreetMap ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Building type
    /// </summary>
    public string? BuildingType { get; set; }
    
    /// <summary>
    /// Existing roof color tag, if any
    /// </summary>
    public string? RoofColor { get; set; }
    
    /// <summary>
    /// Geographic location
    /// </summary>
    public GeoLocation Location { get; set; } = new();
    
    /// <summary>
    /// Bounding box
    /// </summary>
    public BoundingBox BoundingBox { get; set; } = new();
    
    /// <summary>
    /// List of node IDs that make up the building outline
    /// </summary>
    public List<long> NodeIds { get; set; } = new();
    
    /// <summary>
    /// OSM version (for conflict detection)
    /// </summary>
    public int Version { get; set; }
    
    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Represents an area selection for finding buildings
/// </summary>
public class AreaSelection
{
    /// <summary>
    /// Selection type
    /// </summary>
    public AreaSelectionType Type { get; set; }
    
    /// <summary>
    /// Center point (for radius selection)
    /// </summary>
    public GeoLocation? Center { get; set; }
    
    /// <summary>
    /// Radius in meters (for radius selection)
    /// </summary>
    public double Radius { get; set; }
    
    /// <summary>
    /// Bounding box (for rectangle selection)
    /// </summary>
    public BoundingBox? BoundingBox { get; set; }
    
    /// <summary>
    /// City name (for city selection)
    /// </summary>
    public string? CityName { get; set; }
}

/// <summary>
/// Type of area selection
/// </summary>
public enum AreaSelectionType
{
    /// <summary>
    /// Radius around a center point
    /// </summary>
    Radius,
    
    /// <summary>
    /// Rectangular bounding box
    /// </summary>
    Rectangle,
    
    /// <summary>
    /// Named city
    /// </summary>
    City
}
