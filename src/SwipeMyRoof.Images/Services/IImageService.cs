using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Images.Models;

namespace SwipeMyRoof.Images.Services;

/// <summary>
/// Interface for image service operations
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Get a satellite image for a building
    /// </summary>
    /// <param name="building">Building to get image for</param>
    /// <param name="settings">Image quality settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Building image</returns>
    Task<BuildingImage> GetBuildingImageAsync(BuildingCandidate building, ImageQualitySettings? settings = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a base64-encoded thumbnail of a building image
    /// </summary>
    /// <param name="building">Building to get thumbnail for</param>
    /// <param name="width">Thumbnail width</param>
    /// <param name="height">Thumbnail height</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Base64-encoded thumbnail</returns>
    Task<string> GetBuildingThumbnailBase64Async(BuildingCandidate building, int width = 64, int height = 64, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Extract dominant colors from a building image
    /// </summary>
    /// <param name="image">Building image</param>
    /// <param name="maxColors">Maximum number of colors to extract</param>
    /// <returns>List of dominant colors</returns>
    List<string> ExtractDominantColors(BuildingImage image, int maxColors = 3);
    
    /// <summary>
    /// Calculate the building-to-background ratio for an image
    /// </summary>
    /// <param name="image">Building image</param>
    /// <returns>Building-to-background ratio</returns>
    double CalculateBuildingRatio(BuildingImage image);
}