using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Images.Models;

namespace SwipeMyRoof.Images.Services;

/// <summary>
/// Interface for tile provider service operations
/// </summary>
public interface ITileProviderService
{
    /// <summary>
    /// Get all available tile providers
    /// </summary>
    /// <returns>List of tile providers</returns>
    IReadOnlyList<TileProvider> GetAllProviders();
    
    /// <summary>
    /// Get the current tile provider
    /// </summary>
    /// <returns>Current tile provider, or null if none selected</returns>
    TileProvider? GetCurrentProvider();
    
    /// <summary>
    /// Set the current tile provider
    /// </summary>
    /// <param name="providerId">Provider ID</param>
    void SetCurrentProvider(string providerId);
    
    /// <summary>
    /// Add a new tile provider
    /// </summary>
    /// <param name="provider">Tile provider</param>
    void AddProvider(TileProvider provider);
    
    /// <summary>
    /// Remove a tile provider
    /// </summary>
    /// <param name="providerId">Provider ID</param>
    void RemoveProvider(string providerId);
    
    /// <summary>
    /// Get a tile for the given coordinates using the current provider
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <param name="zoom">Zoom level</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tile image data</returns>
    Task<byte[]> GetTileAsync(double latitude, double longitude, int zoom, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a tile for the given coordinates using the specified provider
    /// </summary>
    /// <param name="provider">Tile provider</param>
    /// <param name="x">Tile X coordinate</param>
    /// <param name="y">Tile Y coordinate</param>
    /// <param name="zoom">Zoom level</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tile image data</returns>
    Task<byte[]> GetTileAsync(TileProvider provider, int x, int y, int zoom, CancellationToken cancellationToken = default);
    
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