using System;

namespace SwipeMyRoof.OSM.Models;

/// <summary>
/// Represents an OAuth token for OpenStreetMap authentication
/// </summary>
public class OsmAuthToken
{
    /// <summary>
    /// OAuth token
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// OAuth token secret
    /// </summary>
    public string TokenSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// When the token was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the token expires (null for non-expiring tokens)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Whether the token is a request token
    /// </summary>
    public bool IsRequestToken { get; set; }
    
    /// <summary>
    /// Whether the token is an access token
    /// </summary>
    public bool IsAccessToken { get; set; }
    
    /// <summary>
    /// The username associated with the token (for access tokens)
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Check if the token is valid (not expired)
    /// </summary>
    public bool IsValid => !ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow;
}