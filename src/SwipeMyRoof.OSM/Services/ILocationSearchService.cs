using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Interface for location search operations using Nominatim
/// </summary>
public interface ILocationSearchService
{
    /// <summary>
    /// Search for locations by name (city, state, country, street, etc.)
    /// </summary>
    /// <param name="query">Search query (e.g., "Seattle", "Washington", "Main Street")</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of location search results</returns>
    Task<List<LocationSearchResult>> SearchLocationsAsync(string query, int limit = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get detailed information about a specific location
    /// </summary>
    /// <param name="placeId">Place ID from search results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed location information</returns>
    Task<LocationSearchResult?> GetLocationDetailsAsync(long placeId, CancellationToken cancellationToken = default);
}