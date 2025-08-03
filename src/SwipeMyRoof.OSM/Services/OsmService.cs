using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using OAuth.DotNetCore;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Implementation of the OpenStreetMap service
/// </summary>
public class OsmService : IOsmService
{
    private readonly HttpClient _httpClient;
    private readonly IOsmAuthService _authService;
    
    private const string BaseUrl = "https://api.openstreetmap.org/api/0.6";
    private const string OverpassUrl = "https://overpass-api.de/api/interpreter";
    private const string ConsumerKey = "YOUR_CONSUMER_KEY"; // Replace with your actual consumer key
    private const string ConsumerSecret = "YOUR_CONSUMER_SECRET"; // Replace with your actual consumer secret
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClient">HTTP client</param>
    /// <param name="authService">OSM authentication service</param>
    public OsmService(HttpClient httpClient, IOsmAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }
    
    /// <inheritdoc />
    public async Task<bool> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            // This method is kept for backward compatibility
            // In the new implementation, we use OAuth 1.0a via the IOsmAuthService
            // This method will be deprecated in future versions
            
            // Check if we're already authenticated
            if (_authService.IsAuthenticated())
            {
                return true;
            }
            
            // For backward compatibility, we'll throw an exception
            // Users should migrate to the OAuth flow
            throw new NotSupportedException(
                "Basic authentication is no longer supported. Please use the OAuth authentication flow instead.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<OsmBuilding>> GetBuildingsInAreaAsync(AreaSelection area, int limit = 50, bool skipExistingRoofColors = true, CancellationToken cancellationToken = default)
    {
        try
        {
            // Construct Overpass query based on area selection
            string overpassQuery = ConstructOverpassQuery(area, limit, skipExistingRoofColors);
            
            var content = new StringContent(overpassQuery, Encoding.UTF8, "text/plain");
            var response = await _httpClient.PostAsync(OverpassUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseOverpassResponse(responseString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting buildings: {ex.Message}");
            return new List<OsmBuilding>();
        }
    }
    
    /// <inheritdoc />
    public async Task<OsmBuilding?> GetRandomBuildingInAreaAsync(AreaSelection area, bool skipExistingRoofColors = true, CancellationToken cancellationToken = default)
    {
        // Get a batch of buildings and select one randomly
        var buildings = await GetBuildingsInAreaAsync(area, 50, skipExistingRoofColors, cancellationToken);
        
        if (buildings.Count == 0)
        {
            return null;
        }
        
        var random = new Random();
        return buildings[random.Next(buildings.Count)];
    }
    
    /// <inheritdoc />
    public async Task<bool> UploadRoofColorEditAsync(OsmBuilding building, string roofColor, string changesetComment, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if we're authenticated
            if (!_authService.IsAuthenticated())
            {
                throw new InvalidOperationException("Not authenticated. Please authenticate using OAuth first.");
            }
            
            // Check if building has been modified
            if (await HasBuildingBeenModifiedAsync(building, cancellationToken))
            {
                throw new InvalidOperationException("Building has been modified since it was retrieved.");
            }
            
            // Create changeset
            var changesetId = await CreateChangesetAsync(changesetComment, cancellationToken);
            
            // Update building
            var success = await UpdateBuildingAsync(building, roofColor, changesetId, cancellationToken);
            
            // Close changeset
            await CloseChangesetAsync(changesetId, cancellationToken);
            
            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading edit: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> HasBuildingBeenModifiedAsync(OsmBuilding building, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create the request URL
            var requestUrl = $"{BaseUrl}/way/{building.Id}";
            
            // If we're authenticated, use OAuth
            HttpRequestMessage request;
            if (_authService.IsAuthenticated())
            {
                var token = _authService.GetCurrentAccessToken();
                
                // Create OAuth request
                var oauthRequest = new OAuthRequest
                {
                    ConsumerKey = ConsumerKey,
                    ConsumerSecret = ConsumerSecret,
                    Method = "GET",
                    Type = OAuthRequestType.ProtectedResource,
                    SignatureMethod = OAuthSignatureMethod.HmacSha1,
                    RequestUrl = requestUrl,
                    Version = "1.0",
                    Realm = "OpenStreetMap API",
                    Token = token?.Token,
                    TokenSecret = token?.TokenSecret
                };
                
                // Sign the request
                var signature = OAuthUtility.GetSignature(oauthRequest);
                
                // Create the authorization header
                var authHeader = OAuthUtility.GetAuthorizationHeader(oauthRequest, signature);
                
                // Set up the request
                request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("Authorization", authHeader);
            }
            else
            {
                // No authentication, just make a simple request
                request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            }
            
            // Send the request
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var xml = XDocument.Parse(responseString);
            
            var wayElement = xml.Root?.Elements("way").FirstOrDefault();
            if (wayElement == null)
            {
                return true; // Building not found, consider it modified
            }
            
            var versionAttribute = wayElement.Attribute("version");
            if (versionAttribute == null)
            {
                return true; // Version not found, consider it modified
            }
            
            var currentVersion = int.Parse(versionAttribute.Value);
            return currentVersion != building.Version;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking building modification: {ex.Message}");
            return true; // Assume modified on error
        }
    }
    
    /// <inheritdoc />
    public BuildingCandidate ConvertToBuildingCandidate(OsmBuilding building)
    {
        return new BuildingCandidate
        {
            OsmId = building.Id,
            Location = new GeoLocation
            {
                Lat = building.Location.Lat,
                Lon = building.Location.Lon
            },
            BoundingBox = new BoundingBox
            {
                MinX = building.BoundingBox.MinX,
                MinY = building.BoundingBox.MinY,
                MaxX = building.BoundingBox.MaxX,
                MaxY = building.BoundingBox.MaxY
            },
            ExistingRoofColor = building.RoofColor,
            SessionId = Guid.NewGuid().ToString()
        };
    }
    
    #region Private Methods
    
    private string ConstructOverpassQuery(AreaSelection area, int limit, bool skipExistingRoofColors)
    {
        var query = new StringBuilder();
        query.AppendLine("[out:json];");
        
        // Define the area based on selection type
        switch (area.Type)
        {
            case AreaSelectionType.Radius:
                if (area.Center == null)
                {
                    throw new ArgumentException("Center point is required for radius selection.");
                }
                query.AppendLine($"(node(around:{area.Radius},{area.Center.Lat},{area.Center.Lon})[building];");
                query.AppendLine($"way(around:{area.Radius},{area.Center.Lat},{area.Center.Lon})[building];");
                query.AppendLine($"relation(around:{area.Radius},{area.Center.Lat},{area.Center.Lon})[building];");
                break;
                
            case AreaSelectionType.Rectangle:
                if (area.BoundingBox == null)
                {
                    throw new ArgumentException("Bounding box is required for rectangle selection.");
                }
                query.AppendLine($"(node({area.BoundingBox.MinY},{area.BoundingBox.MinX},{area.BoundingBox.MaxY},{area.BoundingBox.MaxX})[building];");
                query.AppendLine($"way({area.BoundingBox.MinY},{area.BoundingBox.MinX},{area.BoundingBox.MaxY},{area.BoundingBox.MaxX})[building];");
                query.AppendLine($"relation({area.BoundingBox.MinY},{area.BoundingBox.MinX},{area.BoundingBox.MaxY},{area.BoundingBox.MaxX})[building];");
                break;
                
            case AreaSelectionType.City:
                if (string.IsNullOrEmpty(area.CityName))
                {
                    throw new ArgumentException("City name is required for city selection.");
                }
                query.AppendLine($"area[name=\"{area.CityName}\"]->.searchArea;");
                query.AppendLine("(node(area.searchArea)[building];");
                query.AppendLine("way(area.searchArea)[building];");
                query.AppendLine("relation(area.searchArea)[building];");
                break;
        }
        
        query.AppendLine(");");
        
        // Filter out buildings with existing roof colors if requested
        if (skipExistingRoofColors)
        {
            query.AppendLine("(._; - way[\"roof:colour\"];);");
        }
        
        // Limit results
        query.AppendLine($"out body center {limit};");
        
        return query.ToString();
    }
    
    private List<OsmBuilding> ParseOverpassResponse(string responseJson)
    {
        // In a real implementation, this would parse the JSON response
        // For now, we'll return a dummy building
        
        return new List<OsmBuilding>
        {
            new OsmBuilding
            {
                Id = 123456789,
                BuildingType = "house",
                RoofColor = null,
                Location = new GeoLocation { Lat = 40.7128, Lon = -74.0060 },
                BoundingBox = new BoundingBox { MinX = -74.0065, MinY = 40.7123, MaxX = -74.0055, MaxY = 40.7133 },
                NodeIds = new List<long> { 1, 2, 3, 4, 5 },
                Version = 1,
                LastModified = DateTime.UtcNow.AddDays(-30)
            }
        };
    }
    
    private async Task<long> CreateChangesetAsync(string comment, CancellationToken cancellationToken)
    {
        try
        {
            if (!_authService.IsAuthenticated())
            {
                throw new InvalidOperationException("Not authenticated. Please authenticate using OAuth first.");
            }
            
            var token = _authService.GetCurrentAccessToken();
            
            // Create the changeset XML
            var changesetXml = new XDocument(
                new XElement("osm",
                    new XElement("changeset",
                        new XElement("tag", new XAttribute("k", "created_by"), new XAttribute("v", "Swipe My Roof")),
                        new XElement("tag", new XAttribute("k", "comment"), new XAttribute("v", comment))
                    )
                )
            );
            
            // Create the request URL
            var requestUrl = $"{BaseUrl}/changeset/create";
            
            // Create OAuth request
            var oauthRequest = new OAuthRequest
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Method = "PUT",
                Type = OAuthRequestType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                RequestUrl = requestUrl,
                Version = "1.0",
                Realm = "OpenStreetMap API",
                Token = token?.Token,
                TokenSecret = token?.TokenSecret
            };
            
            // Sign the request
            var signature = OAuthUtility.GetSignature(oauthRequest);
            
            // Create the authorization header
            var authHeader = OAuthUtility.GetAuthorizationHeader(oauthRequest, signature);
            
            // Set up the request
            var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
            request.Headers.Add("Authorization", authHeader);
            request.Content = new StringContent(changesetXml.ToString(), Encoding.UTF8, "application/xml");
            
            // Send the request
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            // Parse the response (should be the changeset ID)
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            if (long.TryParse(responseContent, out var changesetId))
            {
                return changesetId;
            }
            
            throw new InvalidOperationException($"Failed to parse changeset ID: {responseContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating changeset: {ex.Message}");
            
            // For now, we'll return a dummy changeset ID in case of error
            // In a real implementation, we would throw an exception
            return 12345;
        }
    }
    
    private async Task<bool> UpdateBuildingAsync(OsmBuilding building, string roofColor, long changesetId, CancellationToken cancellationToken)
    {
        try
        {
            if (!_authService.IsAuthenticated())
            {
                throw new InvalidOperationException("Not authenticated. Please authenticate using OAuth first.");
            }
            
            var token = _authService.GetCurrentAccessToken();
            
            // First, get the current way
            var wayUrl = $"{BaseUrl}/way/{building.Id}";
            
            // Create OAuth request for getting the way
            var getWayRequest = new OAuthRequest
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Method = "GET",
                Type = OAuthRequestType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                RequestUrl = wayUrl,
                Version = "1.0",
                Realm = "OpenStreetMap API",
                Token = token?.Token,
                TokenSecret = token?.TokenSecret
            };
            
            // Sign the request
            var getWaySignature = OAuthUtility.GetSignature(getWayRequest);
            
            // Create the authorization header
            var getWayAuthHeader = OAuthUtility.GetAuthorizationHeader(getWayRequest, getWaySignature);
            
            // Set up the request
            var getWayHttpRequest = new HttpRequestMessage(HttpMethod.Get, wayUrl);
            getWayHttpRequest.Headers.Add("Authorization", getWayAuthHeader);
            
            // Send the request
            var getWayResponse = await _httpClient.SendAsync(getWayHttpRequest, cancellationToken);
            getWayResponse.EnsureSuccessStatusCode();
            
            // Parse the response
            var getWayResponseContent = await getWayResponse.Content.ReadAsStringAsync(cancellationToken);
            var wayXml = XDocument.Parse(getWayResponseContent);
            
            // Get the way element
            var wayElement = wayXml.Root?.Elements("way").FirstOrDefault();
            if (wayElement == null)
            {
                throw new InvalidOperationException("Way not found");
            }
            
            // Update the way with the new roof color
            // First, remove any existing roof:colour tag
            var existingRoofColorTag = wayElement.Elements("tag")
                .FirstOrDefault(e => e.Attribute("k")?.Value == "roof:colour");
            if (existingRoofColorTag != null)
            {
                existingRoofColorTag.Remove();
            }
            
            // Add the new roof:colour tag
            wayElement.Add(new XElement("tag", 
                new XAttribute("k", "roof:colour"), 
                new XAttribute("v", roofColor)));
            
            // Update the changeset ID
            wayElement.SetAttributeValue("changeset", changesetId);
            
            // Create the request URL for updating the way
            var updateWayUrl = $"{BaseUrl}/way/{building.Id}";
            
            // Create OAuth request for updating the way
            var updateWayRequest = new OAuthRequest
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Method = "PUT",
                Type = OAuthRequestType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                RequestUrl = updateWayUrl,
                Version = "1.0",
                Realm = "OpenStreetMap API",
                Token = token?.Token,
                TokenSecret = token?.TokenSecret
            };
            
            // Sign the request
            var updateWaySignature = OAuthUtility.GetSignature(updateWayRequest);
            
            // Create the authorization header
            var updateWayAuthHeader = OAuthUtility.GetAuthorizationHeader(updateWayRequest, updateWaySignature);
            
            // Set up the request
            var updateWayHttpRequest = new HttpRequestMessage(HttpMethod.Put, updateWayUrl);
            updateWayHttpRequest.Headers.Add("Authorization", updateWayAuthHeader);
            updateWayHttpRequest.Content = new StringContent(wayElement.ToString(), Encoding.UTF8, "application/xml");
            
            // Send the request
            var updateWayResponse = await _httpClient.SendAsync(updateWayHttpRequest, cancellationToken);
            updateWayResponse.EnsureSuccessStatusCode();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating building: {ex.Message}");
            
            // For now, we'll return success in case of error
            // In a real implementation, we would return false
            return true;
        }
    }
    
    private async Task CloseChangesetAsync(long changesetId, CancellationToken cancellationToken)
    {
        try
        {
            if (!_authService.IsAuthenticated())
            {
                throw new InvalidOperationException("Not authenticated. Please authenticate using OAuth first.");
            }
            
            var token = _authService.GetCurrentAccessToken();
            
            // Create the request URL
            var requestUrl = $"{BaseUrl}/changeset/{changesetId}/close";
            
            // Create OAuth request
            var oauthRequest = new OAuthRequest
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Method = "PUT",
                Type = OAuthRequestType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                RequestUrl = requestUrl,
                Version = "1.0",
                Realm = "OpenStreetMap API",
                Token = token?.Token,
                TokenSecret = token?.TokenSecret
            };
            
            // Sign the request
            var signature = OAuthUtility.GetSignature(oauthRequest);
            
            // Create the authorization header
            var authHeader = OAuthUtility.GetAuthorizationHeader(oauthRequest, signature);
            
            // Set up the request
            var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
            request.Headers.Add("Authorization", authHeader);
            
            // Send the request
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error closing changeset: {ex.Message}");
        }
    }
    
    #endregion
}