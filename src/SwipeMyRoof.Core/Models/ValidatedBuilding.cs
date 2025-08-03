using System.ComponentModel.DataAnnotations;

namespace SwipeMyRoof.Core.Models;

/// <summary>
/// Represents a building that has been validated with roof color information
/// </summary>
public class ValidatedBuilding
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// OpenStreetMap building ID
    /// </summary>
    [Required]
    public long OsmId { get; set; }
    
    /// <summary>
    /// OSM element type (way, relation)
    /// </summary>
    [Required]
    public string OsmType { get; set; } = "way";
    
    /// <summary>
    /// Validated roof color as HEX value
    /// </summary>
    [Required]
    public string RoofColorHex { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable color description
    /// </summary>
    public string? ColorDescription { get; set; }
    
    /// <summary>
    /// How the color was determined (ai, manual, eyedropper)
    /// </summary>
    [Required]
    public string ValidationMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// AI confidence score (if applicable)
    /// </summary>
    public double? AiConfidence { get; set; }
    
    /// <summary>
    /// Pixel coordinates where color was picked (if manual)
    /// </summary>
    public string? PickedPixelCoordinates { get; set; }
    
    /// <summary>
    /// Building latitude
    /// </summary>
    public double Latitude { get; set; }
    
    /// <summary>
    /// Building longitude
    /// </summary>
    public double Longitude { get; set; }
    
    /// <summary>
    /// Building type from OSM tags (if available)
    /// </summary>
    public string? BuildingType { get; set; }
    
    /// <summary>
    /// When the validation was performed
    /// </summary>
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User who performed the validation (if user system implemented)
    /// </summary>
    public string? ValidatedBy { get; set; }
    
    /// <summary>
    /// Whether this has been uploaded to OSM
    /// </summary>
    public bool UploadedToOsm { get; set; } = false;
    
    /// <summary>
    /// When it was uploaded to OSM (if applicable)
    /// </summary>
    public DateTime? UploadedAt { get; set; }
    
    /// <summary>
    /// OSM changeset ID (if uploaded)
    /// </summary>
    public long? OsmChangesetId { get; set; }
    
    /// <summary>
    /// Any notes or comments about the validation
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Original OSM tags (JSON) for reference
    /// </summary>
    public string? OriginalOsmTags { get; set; }
    
    /// <summary>
    /// Whether this building had an existing roof:colour tag
    /// </summary>
    public bool HadExistingRoofColor { get; set; } = false;
    
    /// <summary>
    /// Previous roof color value (if updating existing)
    /// </summary>
    public string? PreviousRoofColor { get; set; }
}