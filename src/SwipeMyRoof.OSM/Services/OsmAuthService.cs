using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using OAuth.DotNetCore;
using SwipeMyRoof.OSM.Models;
using SwipeMyRoof.Storage.Services;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Implementation of the OpenStreetMap OAuth authentication service
/// </summary>
public class OsmAuthService : IOsmAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IStorageService _storageService;
    private OsmAuthToken? _currentToken;
    
    private const string RequestTokenUrl = "https://www.openstreetmap.org/oauth/request_token";
    private const string AuthorizeUrl = "https://www.openstreetmap.org/oauth/authorize";
    private const string AccessTokenUrl = "https://www.openstreetmap.org/oauth/access_token";
    private const string UserDetailsUrl = "https://api.openstreetmap.org/api/0.6/user/details";
    
    private const string ConsumerKey = "YOUR_CONSUMER_KEY"; // Replace with your actual consumer key
    private const string ConsumerSecret = "YOUR_CONSUMER_SECRET"; // Replace with your actual consumer secret
    
    private const string TokenStorageKey = "osm_auth_token";
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClient">HTTP client</param>
    /// <param name="storageService">Storage service for persisting tokens</param>
    public OsmAuthService(HttpClient httpClient, IStorageService storageService)
    {
        _httpClient = httpClient;
        _storageService = storageService;
        
        // Try to load the token from storage
        LoadTokenFromStorage();
    }
    
    /// <inheritdoc />
    public OsmAuthToken? GetCurrentAccessToken()
    {
        if (_currentToken?.IsAccessToken == true && _currentToken.IsValid)
        {
            return _currentToken;
        }
        
        return null;
    }
    
    /// <inheritdoc />
    public bool IsAuthenticated()
    {
        return GetCurrentAccessToken() != null;
    }
    
    /// <inheritdoc />
    public async Task<string> StartAuthenticationAsync(string callbackUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create OAuth consumer
            var consumer = new OAuthConsumer(ConsumerKey, ConsumerSecret);
            
            // Create request for request token
            var requestTokenRequest = new OAuthRequest
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Method = "POST",
                Type = OAuthRequestType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                RequestUrl = RequestTokenUrl,
                Version = "1.0",
                Realm = "OpenStreetMap API",
                CallbackUrl = callbackUrl
            };
            
            // Sign the request
            var requestTokenSignature = OAuthUtility.GetSignature(requestTokenRequest);
            
            // Create the authorization header
            var authHeader = OAuthUtility.GetAuthorizationHeader(requestTokenRequest, requestTokenSignature);
            
            // Set up the request
            var request = new HttpRequestMessage(HttpMethod.Post, RequestTokenUrl);
            request.Headers.Add("Authorization", authHeader);
            
            // Send the request
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            // Parse the response
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseParams = ParseQueryString(responseContent);
            
            if (!responseParams.TryGetValue("oauth_token", out var requestToken) || 
                !responseParams.TryGetValue("oauth_token_secret", out var requestTokenSecret))
            {
                throw new InvalidOperationException("Failed to get request token");
            }
            
            // Store the request token
            _currentToken = new OsmAuthToken
            {
                Token = requestToken,
                TokenSecret = requestTokenSecret,
                IsRequestToken = true,
                IsAccessToken = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30) // Request tokens typically expire after a short time
            };
            
            // Save the token to storage
            SaveTokenToStorage();
            
            // Return the authorization URL
            return $"{AuthorizeUrl}?oauth_token={requestToken}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting authentication: {ex.Message}");
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> CompleteAuthenticationAsync(string verifier, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentToken == null || !_currentToken.IsRequestToken)
            {
                throw new InvalidOperationException("No request token available");
            }
            
            // Create OAuth consumer
            var consumer = new OAuthConsumer(ConsumerKey, ConsumerSecret);
            
            // Create request for access token
            var accessTokenRequest = new OAuthRequest
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Method = "POST",
                Type = OAuthRequestType.AccessToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                RequestUrl = AccessTokenUrl,
                Version = "1.0",
                Realm = "OpenStreetMap API",
                Token = _currentToken.Token,
                TokenSecret = _currentToken.TokenSecret,
                Verifier = verifier
            };
            
            // Sign the request
            var accessTokenSignature = OAuthUtility.GetSignature(accessTokenRequest);
            
            // Create the authorization header
            var authHeader = OAuthUtility.GetAuthorizationHeader(accessTokenRequest, accessTokenSignature);
            
            // Set up the request
            var request = new HttpRequestMessage(HttpMethod.Post, AccessTokenUrl);
            request.Headers.Add("Authorization", authHeader);
            
            // Send the request
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            // Parse the response
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseParams = ParseQueryString(responseContent);
            
            if (!responseParams.TryGetValue("oauth_token", out var accessToken) || 
                !responseParams.TryGetValue("oauth_token_secret", out var accessTokenSecret))
            {
                throw new InvalidOperationException("Failed to get access token");
            }
            
            // Store the access token
            _currentToken = new OsmAuthToken
            {
                Token = accessToken,
                TokenSecret = accessTokenSecret,
                IsRequestToken = false,
                IsAccessToken = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = null // Access tokens typically don't expire
            };
            
            // Get the username
            await GetUserDetailsAsync(cancellationToken);
            
            // Save the token to storage
            SaveTokenToStorage();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error completing authentication: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public void SignOut()
    {
        _currentToken = null;
        _storageService.Delete(TokenStorageKey);
    }
    
    /// <inheritdoc />
    public string? GetUsername()
    {
        return _currentToken?.IsAccessToken == true ? _currentToken.Username : null;
    }
    
    #region Private Methods
    
    private async Task GetUserDetailsAsync(CancellationToken cancellationToken)
    {
        if (_currentToken?.IsAccessToken != true)
        {
            return;
        }
        
        try
        {
            // Create OAuth request for user details
            var userDetailsRequest = new OAuthRequest
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Method = "GET",
                Type = OAuthRequestType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                RequestUrl = UserDetailsUrl,
                Version = "1.0",
                Realm = "OpenStreetMap API",
                Token = _currentToken.Token,
                TokenSecret = _currentToken.TokenSecret
            };
            
            // Sign the request
            var userDetailsSignature = OAuthUtility.GetSignature(userDetailsRequest);
            
            // Create the authorization header
            var authHeader = OAuthUtility.GetAuthorizationHeader(userDetailsRequest, userDetailsSignature);
            
            // Set up the request
            var request = new HttpRequestMessage(HttpMethod.Get, UserDetailsUrl);
            request.Headers.Add("Authorization", authHeader);
            
            // Send the request
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            // Parse the response
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var xml = XDocument.Parse(responseContent);
            
            // Extract the username
            var userElement = xml.Root?.Element("user");
            if (userElement != null)
            {
                var displayNameAttribute = userElement.Attribute("display_name");
                if (displayNameAttribute != null)
                {
                    _currentToken.Username = displayNameAttribute.Value;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user details: {ex.Message}");
        }
    }
    
    private void LoadTokenFromStorage()
    {
        try
        {
            var tokenJson = _storageService.Get<string>(TokenStorageKey);
            if (!string.IsNullOrEmpty(tokenJson))
            {
                _currentToken = System.Text.Json.JsonSerializer.Deserialize<OsmAuthToken>(tokenJson);
                
                // Check if the token is valid
                if (_currentToken != null && !_currentToken.IsValid)
                {
                    _currentToken = null;
                    _storageService.Delete(TokenStorageKey);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading token from storage: {ex.Message}");
            _currentToken = null;
        }
    }
    
    private void SaveTokenToStorage()
    {
        try
        {
            if (_currentToken != null)
            {
                var tokenJson = System.Text.Json.JsonSerializer.Serialize(_currentToken);
                _storageService.Set(TokenStorageKey, tokenJson);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving token to storage: {ex.Message}");
        }
    }
    
    private Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>();
        
        foreach (var pair in queryString.Split('&'))
        {
            var parts = pair.Split('=');
            if (parts.Length == 2)
            {
                result[parts[0]] = parts[1];
            }
        }
        
        return result;
    }
    
    #endregion
}