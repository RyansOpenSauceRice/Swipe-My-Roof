using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Implementation of the OpenStreetMap service
/// </summary>
public class OsmService : IOsmService
{
    private readonly HttpClient _httpClient;
    private string? _authToken;
    
    private const string BaseUrl = "https://api.openstreetmap.org/api/0.6";
    private const string OverpassUrl = "https://overpass-api.de/api/interpreter";
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClient">HTTP client</param>
    public OsmService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <inheritdoc />
    public async Task<bool> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would use OAuth 1.0a
            // For now, we'll simulate authentication with basic auth
            var authBytes = Encoding.ASCII.GetBytes($"{username}:{password}");
            var authHeader = Convert.ToBase64String(authBytes);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            
            var response = await _httpClient.GetAsync($"{BaseUrl}/user/details", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            // Store the auth token for future requests
            _authToken = authHeader;
            
            return true;
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
            if (string.IsNullOrEmpty(_authToken))
            {
                throw new InvalidOperationException("Not authenticated. Call AuthenticateAsync first.");
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
            var response = await _httpClient.GetAsync($"{BaseUrl}/way/{building.Id}", cancellationToken);
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
        // In a real implementation, this would create a changeset
        // For now, we'll return a dummy changeset ID
        await Task.Delay(100, cancellationToken);
        return 12345;
    }
    
    private async Task<bool> UpdateBuildingAsync(OsmBuilding building, string roofColor, long changesetId, CancellationToken cancellationToken)
    {
        // In a real implementation, this would update the building
        // For now, we'll return success
        await Task.Delay(100, cancellationToken);
        return true;
    }
    
    private async Task CloseChangesetAsync(long changesetId, CancellationToken cancellationToken)
    {
        // In a real implementation, this would close the changeset
        await Task.Delay(100, cancellationToken);
    }
    
    #endregion
}