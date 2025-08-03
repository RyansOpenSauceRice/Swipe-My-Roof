using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Interface for OpenStreetMap OAuth authentication service
/// </summary>
public interface IOsmAuthService
{
    /// <summary>
    /// Get the current access token, if any
    /// </summary>
    /// <returns>The current access token, or null if not authenticated</returns>
    OsmAuthToken? GetCurrentAccessToken();
    
    /// <summary>
    /// Check if the user is authenticated
    /// </summary>
    /// <returns>True if authenticated with a valid access token</returns>
    bool IsAuthenticated();
    
    /// <summary>
    /// Start the OAuth authentication process
    /// </summary>
    /// <param name="callbackUrl">URL to redirect to after authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The authorization URL to redirect the user to</returns>
    Task<string> StartAuthenticationAsync(string callbackUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Complete the OAuth authentication process
    /// </summary>
    /// <param name="verifier">OAuth verifier from the callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if authentication was successful</returns>
    Task<bool> CompleteAuthenticationAsync(string verifier, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sign out the current user
    /// </summary>
    void SignOut();
    
    /// <summary>
    /// Get the username of the authenticated user
    /// </summary>
    /// <returns>The username, or null if not authenticated</returns>
    string? GetUsername();
}