using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.OSM.Models;

/// <summary>
/// Represents a building from OpenStreetMap data
/// </summary>
public class OsmBuilding
{
    /// <summary>
    /// OSM ID of the building
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Geographic location (center point)
    /// </summary>
    public GeoLocation Location { get; set; } = new();
    
    /// <summary>
    /// Bounding box of the building
    /// </summary>
    public BoundingBox? BoundingBox { get; set; }
    
    /// <summary>
    /// Type of building (e.g., "house", "commercial", "yes")
    /// </summary>
    public string? BuildingType { get; set; }
    
    /// <summary>
    /// Current roof color tag value (if any)
    /// </summary>
    public string? RoofColor { get; set; }
    
    /// <summary>
    /// OSM version number
    /// </summary>
    public int Version { get; set; }
    
    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime LastModified { get; set; }
    
    /// <summary>
    /// Node IDs that make up this building (for ways)
    /// </summary>
    public List<long> NodeIds { get; set; } = new();
    
    /// <summary>
    /// Additional OSM tags
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new();
    
    /// <summary>
    /// Whether this building already has a roof color
    /// </summary>
    public bool HasRoofColor => !string.IsNullOrEmpty(RoofColor);
    
    /// <summary>
    /// Convert to BuildingCandidate for validation
    /// </summary>
    /// <returns>Building candidate</returns>
    public BuildingCandidate ToBuildingCandidate()
    {
        return new BuildingCandidate
        {
            OsmId = Id,
            Location = Location,
            BoundingBox = BoundingBox,
            ExistingRoofColor = RoofColor,
            SessionId = Guid.NewGuid().ToString(),
            UploadStatus = UploadStatus.NotStaged
        };
    }
}