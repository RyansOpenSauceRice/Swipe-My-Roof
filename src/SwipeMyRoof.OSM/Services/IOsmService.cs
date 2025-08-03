using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Interface for OpenStreetMap service operations
/// </summary>
public interface IOsmService
{
    /// <summary>
    /// Authenticate with OpenStreetMap
    /// </summary>
    /// <param name="username">OSM username</param>
    /// <param name="password">OSM password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if authentication successful</returns>
    Task<bool> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get buildings in the specified area
    /// </summary>
    /// <param name="area">Area selection</param>
    /// <param name="limit">Maximum number of buildings to return</param>
    /// <param name="skipExistingRoofColors">Whether to skip buildings with existing roof colors</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of buildings</returns>
    Task<List<OsmBuilding>> GetBuildingsInAreaAsync(AreaSelection area, int limit = 50, bool skipExistingRoofColors = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a random building in the specified area
    /// </summary>
    /// <param name="area">Area selection</param>
    /// <param name="skipExistingRoofColors">Whether to skip buildings with existing roof colors</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A random building, or null if none found</returns>
    Task<OsmBuilding?> GetRandomBuildingInAreaAsync(AreaSelection area, bool skipExistingRoofColors = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Upload a roof color edit to OpenStreetMap
    /// </summary>
    /// <param name="building">Building to update</param>
    /// <param name="roofColor">New roof color</param>
    /// <param name="changesetComment">Comment for the changeset</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if upload successful</returns>
    Task<bool> UploadRoofColorEditAsync(OsmBuilding building, string roofColor, string changesetComment, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a building has been modified since it was retrieved
    /// </summary>
    /// <param name="building">Building to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if building has been modified</returns>
    Task<bool> HasBuildingBeenModifiedAsync(OsmBuilding building, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Convert a building to a building candidate for the app
    /// </summary>
    /// <param name="building">OSM building</param>
    /// <returns>Building candidate</returns>
    BuildingCandidate ConvertToBuildingCandidate(OsmBuilding building);
}