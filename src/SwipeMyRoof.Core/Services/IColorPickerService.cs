using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.Core.Services;

/// <summary>
/// Interface for color picking operations on images
/// </summary>
public interface IColorPickerService
{
    /// <summary>
    /// Pick a color from an image at the specified coordinates
    /// </summary>
    /// <param name="imageData">Image data (JPEG, PNG, etc.)</param>
    /// <param name="x">X coordinate (pixel)</param>
    /// <param name="y">Y coordinate (pixel)</param>
    /// <returns>Picked color information</returns>
    Task<PickedColor?> PickColorAsync(byte[] imageData, int x, int y);
    
    /// <summary>
    /// Pick a color from an image at the specified coordinates with sampling
    /// </summary>
    /// <param name="imageData">Image data (JPEG, PNG, etc.)</param>
    /// <param name="x">X coordinate (pixel)</param>
    /// <param name="y">Y coordinate (pixel)</param>
    /// <param name="sampleRadius">Radius in pixels to sample around the point</param>
    /// <returns>Picked color information (averaged from sample area)</returns>
    Task<PickedColor?> PickColorWithSamplingAsync(byte[] imageData, int x, int y, int sampleRadius = 3);
    
    /// <summary>
    /// Get the dominant colors from an image
    /// </summary>
    /// <param name="imageData">Image data (JPEG, PNG, etc.)</param>
    /// <param name="maxColors">Maximum number of colors to return</param>
    /// <returns>List of dominant colors</returns>
    Task<List<RgbColor>> GetDominantColorsAsync(byte[] imageData, int maxColors = 5);
    
    /// <summary>
    /// Get a human-readable description of an RGB color
    /// </summary>
    /// <param name="rgb">RGB color to describe</param>
    /// <returns>Color description</returns>
    string GetColorDescription(RgbColor rgb);
}