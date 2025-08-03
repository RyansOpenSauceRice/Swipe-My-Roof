using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.Images.Models;

/// <summary>
/// Represents a satellite image of a building
/// </summary>
public class BuildingImage
{
    /// <summary>
    /// OpenStreetMap ID of the building
    /// </summary>
    public long OsmId { get; set; }
    
    /// <summary>
    /// Raw image data
    /// </summary>
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Image width in pixels
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// Image height in pixels
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// Image format (e.g., "jpeg", "png")
    /// </summary>
    public string Format { get; set; } = "jpeg";
    
    /// <summary>
    /// Building-to-background ratio
    /// </summary>
    public double BuildingRatio { get; set; }
    
    /// <summary>
    /// Whether the image is complete (vs. partial)
    /// </summary>
    public bool IsComplete { get; set; } = true;
    
    /// <summary>
    /// Dominant colors in the image
    /// </summary>
    public List<string> DominantColors { get; set; } = new();
    
    /// <summary>
    /// Timestamp when the image was fetched
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Image quality settings
/// </summary>
public class ImageQualitySettings
{
    /// <summary>
    /// Target image width in pixels
    /// </summary>
    public int TargetWidth { get; set; } = 64;
    
    /// <summary>
    /// Target image height in pixels
    /// </summary>
    public int TargetHeight { get; set; } = 64;
    
    /// <summary>
    /// Building buffer ratio (e.g., 0.3 = 30% buffer around building)
    /// </summary>
    public double BuildingBufferRatio { get; set; } = 0.3;
    
    /// <summary>
    /// Maximum image size in bytes
    /// </summary>
    public int MaxImageSizeBytes { get; set; } = 1024 * 1024; // 1 MB
    
    /// <summary>
    /// JPEG quality (0-100)
    /// </summary>
    public int JpegQuality { get; set; } = 80;
    
    /// <summary>
    /// Whether to use high-resolution images
    /// </summary>
    public bool UseHighResolution { get; set; } = false;
}
