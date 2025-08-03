using System.Text.Json.Serialization;

namespace SwipeMyRoof.LLM.Models;

/// <summary>
/// Standard request to the LLM for roof color inference
/// </summary>
public class RoofColorRequest
{
    /// <summary>
    /// OpenStreetMap ID of the building
    /// </summary>
    public long BuildingId { get; set; }
    
    /// <summary>
    /// Geographic location of the building
    /// </summary>
    public LocationInfo Location { get; set; } = new();
    
    /// <summary>
    /// Bounding box of the building
    /// </summary>
    public BoundingBoxInfo BoundingBox { get; set; } = new();
    
    /// <summary>
    /// Existing roof color tag from OSM, if any
    /// </summary>
    public string? ExistingRoofColour { get; set; }
    
    /// <summary>
    /// Ratio of building size to background (e.g., 1.3)
    /// </summary>
    public double BuildingRatio { get; set; }
    
    /// <summary>
    /// Image summary information
    /// </summary>
    public ImageSummary ImageSummary { get; set; } = new();
    
    /// <summary>
    /// List of allowed color values
    /// </summary>
    public List<string> AllowedColors { get; set; } = new()
    {
        "black", "dark gray", "light gray", "red",
        "brown", "tan", "green", "blue", "white", "other"
    };
}

/// <summary>
/// Location information for LLM request
/// </summary>
public class LocationInfo
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
/// Bounding box information for LLM request
/// </summary>
public class BoundingBoxInfo
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
/// Image summary information for LLM request
/// </summary>
public class ImageSummary
{
    /// <summary>
    /// Base64-encoded thumbnail image (typically 64x64)
    /// </summary>
    public string? ThumbnailBase64 { get; set; }
    
    /// <summary>
    /// Base64-encoded full image (optional, for high-res mode)
    /// </summary>
    public string? FullImageBase64 { get; set; }
    
    /// <summary>
    /// List of dominant colors in the image
    /// </summary>
    public List<string> DominantColors { get; set; } = new();
    
    /// <summary>
    /// Image quality indicator ("full" or "partial")
    /// </summary>
    public string Quality { get; set; } = "full";
}

/// <summary>
/// Response from the LLM for roof color inference
/// </summary>
public class RoofColorResponse
{
    /// <summary>
    /// The inferred roof color
    /// </summary>
    public string Color { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence level (0.0-1.0)
    /// </summary>
    public double Confidence { get; set; }
    
    /// <summary>
    /// Brief explanation of the color choice
    /// </summary>
    public string? Explanation { get; set; }
    
    /// <summary>
    /// Method used for inference (llm, local_model, fallback)
    /// </summary>
    public string Method { get; set; } = "llm";
}

/// <summary>
/// Request for AI re-suggestion after user rejection
/// </summary>
public class RoofColorReSuggestionRequest
{
    /// <summary>
    /// OpenStreetMap ID of the building
    /// </summary>
    public long BuildingId { get; set; }
    
    /// <summary>
    /// Previously suggested color that was rejected
    /// </summary>
    public string PreviousColor { get; set; } = string.Empty;
    
    /// <summary>
    /// Geographic location of the building
    /// </summary>
    public LocationInfo Location { get; set; } = new();
    
    /// <summary>
    /// Image summary information
    /// </summary>
    public ImageSummary ImageSummary { get; set; } = new();
    
    /// <summary>
    /// List of allowed color values
    /// </summary>
    public List<string> AllowedColors { get; set; } = new()
    {
        "black", "dark gray", "light gray", "red",
        "brown", "tan", "green", "blue", "white", "other"
    };
}
