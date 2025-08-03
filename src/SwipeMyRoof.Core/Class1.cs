using System.Text.Json.Serialization;

namespace SwipeMyRoof.Core.Models;

/// <summary>
/// Represents a building candidate for roof color validation
/// </summary>
public class BuildingCandidate
{
    /// <summary>
    /// OpenStreetMap ID of the building
    /// </summary>
    public long OsmId { get; set; }
    
    /// <summary>
    /// Geographic location of the building
    /// </summary>
    public GeoLocation Location { get; set; } = new();
    
    /// <summary>
    /// Bounding box of the building
    /// </summary>
    public BoundingBox BoundingBox { get; set; } = new();
    
    /// <summary>
    /// Existing roof color tag from OSM, if any
    /// </summary>
    public string? ExistingRoofColor { get; set; }
    
    /// <summary>
    /// Proposed roof color from LLM or other source
    /// </summary>
    public ProposedColor? ProposedColor { get; set; }
    
    /// <summary>
    /// User feedback on the proposed color
    /// </summary>
    public UserFeedback UserFeedback { get; set; } = UserFeedback.None;
    
    /// <summary>
    /// Session ID for tracking
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Upload status of the building edit
    /// </summary>
    public UploadStatus UploadStatus { get; set; } = UploadStatus.NotStaged;
}

/// <summary>
/// Geographic location (latitude/longitude)
/// </summary>
public class GeoLocation
{
    /// <summary>
    /// Latitude in decimal degrees
    /// </summary>
    public double Lat { get; set; }
    
    /// <summary>
    /// Longitude in decimal degrees
    /// </summary>
    public double Lon { get; set; }
}

/// <summary>
/// Bounding box for a geographic area
/// </summary>
public class BoundingBox
{
    /// <summary>
    /// Minimum X coordinate (longitude)
    /// </summary>
    public double MinX { get; set; }
    
    /// <summary>
    /// Minimum Y coordinate (latitude)
    /// </summary>
    public double MinY { get; set; }
    
    /// <summary>
    /// Maximum X coordinate (longitude)
    /// </summary>
    public double MaxX { get; set; }
    
    /// <summary>
    /// Maximum Y coordinate (latitude)
    /// </summary>
    public double MaxY { get; set; }
}

/// <summary>
/// Proposed roof color from LLM or other source
/// </summary>
public class ProposedColor
{
    /// <summary>
    /// The color value (e.g., "dark gray", "red", etc.)
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Source of the color suggestion (ai, heuristic, etc.)
    /// </summary>
    public string Source { get; set; } = "ai";
    
    /// <summary>
    /// Confidence level (0.0-1.0)
    /// </summary>
    public double Confidence { get; set; }
    
    /// <summary>
    /// Timestamp of when the color was proposed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Whether this is a decoy color (for training/reliability)
    /// </summary>
    public bool IsDecoy { get; set; }
    
    /// <summary>
    /// Brief explanation of the color choice
    /// </summary>
    public string? Explanation { get; set; }
}

/// <summary>
/// User feedback on a proposed roof color
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserFeedback
{
    None,
    Accepted,
    Rejected,
    Skipped,
    Corrected
}

/// <summary>
/// Upload status of a building edit
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UploadStatus
{
    NotStaged,
    Staged,
    Uploaded,
    Failed
}
