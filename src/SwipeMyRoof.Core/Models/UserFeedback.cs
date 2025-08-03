namespace SwipeMyRoof.Core.Models;

/// <summary>
/// User feedback on a building validation
/// </summary>
public class UserFeedback
{
    /// <summary>
    /// OSM ID of the building
    /// </summary>
    public long BuildingId { get; set; }
    
    /// <summary>
    /// User's action
    /// </summary>
    public UserAction Action { get; set; }
    
    /// <summary>
    /// Final color value as HEX (if accepted or manually corrected)
    /// </summary>
    public string? FinalHexColor { get; set; }
    
    /// <summary>
    /// Whether the color was manually selected
    /// </summary>
    public bool WasManuallySelected { get; set; }
    
    /// <summary>
    /// RGB color picked by user (if manually selected)
    /// </summary>
    public RgbColor? PickedRgbColor { get; set; }
    
    /// <summary>
    /// Pixel coordinates where color was picked (if manually selected)
    /// </summary>
    public (int x, int y)? PickedPixelCoordinates { get; set; }
    
    /// <summary>
    /// Color description (if manually selected)
    /// </summary>
    public string? ColorDescription { get; set; }
    
    /// <summary>
    /// Timestamp when feedback was provided
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Session ID for tracking
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional user comment
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// User action types
/// </summary>
public enum UserAction
{
    /// <summary>
    /// User accepted the AI-proposed color
    /// </summary>
    Accepted,
    
    /// <summary>
    /// User rejected the AI-proposed color and manually selected a different one
    /// </summary>
    ManuallySelected,
    
    /// <summary>
    /// User skipped this building
    /// </summary>
    Skipped,
    
    /// <summary>
    /// User reported an issue with the building/image
    /// </summary>
    Reported
}