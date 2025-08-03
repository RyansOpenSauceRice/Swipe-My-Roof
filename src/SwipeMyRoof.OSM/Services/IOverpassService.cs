using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Interface for Overpass API operations
/// </summary>
public interface IOverpassService
{
    /// <summary>
    /// Get buildings in a circular area around a center point
    /// </summary>
    /// <param name="center">Center point</param>
    /// <param name="radiusMeters">Radius in meters</param>
    /// <param name="limit">Maximum number of buildings to return</param>
    /// <param name="skipExistingRoofColors">Whether to skip buildings with existing roof colors</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of buildings</returns>
    Task<List<OsmBuilding>> GetBuildingsInRadiusAsync(GeoLocation center, double radiusMeters, int limit = 50, bool skipExistingRoofColors = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get buildings in a bounding box
    /// </summary>
    /// <param name="boundingBox">Bounding box</param>
    /// <param name="limit">Maximum number of buildings to return</param>
    /// <param name="skipExistingRoofColors">Whether to skip buildings with existing roof colors</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of buildings</returns>
    Task<List<OsmBuilding>> GetBuildingsInBoundingBoxAsync(BoundingBox boundingBox, int limit = 50, bool skipExistingRoofColors = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a specific building by its OSM ID
    /// </summary>
    /// <param name="osmId">OSM ID of the building</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Building details or null if not found</returns>
    Task<OsmBuilding?> GetBuildingByIdAsync(long osmId, CancellationToken cancellationToken = default);
}