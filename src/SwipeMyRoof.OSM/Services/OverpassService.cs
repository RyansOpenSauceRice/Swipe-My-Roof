using System.Text;
using System.Text.Json;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Overpass API implementation for querying OSM data
/// </summary>
public class OverpassService : IOverpassService
{
    private readonly HttpClient _httpClient;
    private const string OverpassApiUrl = "https://overpass-api.de/api/interpreter";
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClient">HTTP client</param>
    public OverpassService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // Set user agent for Overpass API
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SwipeMyRoof/1.0 (https://github.com/RyansOpenSauceRice/Swipe-My-Roof)");
    }
    
    /// <inheritdoc />
    public async Task<List<OsmBuilding>> GetBuildingsInRadiusAsync(GeoLocation center, double radiusMeters, int limit = 50, bool skipExistingRoofColors = true, CancellationToken cancellationToken = default)
    {
        var roofColorFilter = skipExistingRoofColors ? "[!\"roof:colour\"]" : "";
        
        var query = $@"
[out:json][timeout:25];
(
  way[""building""{roofColorFilter}](around:{radiusMeters},{center.Lat},{center.Lon});
  relation[""building""{roofColorFilter}](around:{radiusMeters},{center.Lat},{center.Lon});
);
out geom meta {limit};
";
        
        return await ExecuteOverpassQuery(query, cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<List<OsmBuilding>> GetBuildingsInBoundingBoxAsync(BoundingBox boundingBox, int limit = 50, bool skipExistingRoofColors = true, CancellationToken cancellationToken = default)
    {
        var roofColorFilter = skipExistingRoofColors ? "[!\"roof:colour\"]" : "";
        
        var query = $@"
[out:json][timeout:25];
(
  way[""building""{roofColorFilter}]({boundingBox.MinY},{boundingBox.MinX},{boundingBox.MaxY},{boundingBox.MaxX});
  relation[""building""{roofColorFilter}]({boundingBox.MinY},{boundingBox.MinX},{boundingBox.MaxY},{boundingBox.MaxX});
);
out geom meta {limit};
";
        
        return await ExecuteOverpassQuery(query, cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<OsmBuilding?> GetBuildingByIdAsync(long osmId, CancellationToken cancellationToken = default)
    {
        var query = $@"
[out:json][timeout:25];
(
  way({osmId});
  relation({osmId});
);
out geom meta;
";
        
        var buildings = await ExecuteOverpassQuery(query, cancellationToken);
        return buildings.FirstOrDefault();
    }
    
    private async Task<List<OsmBuilding>> ExecuteOverpassQuery(string query, CancellationToken cancellationToken)
    {
        try
        {
            var content = new StringContent(query, Encoding.UTF8, "text/plain");
            var response = await _httpClient.PostAsync(OverpassApiUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var overpassResult = JsonSerializer.Deserialize<OverpassResult>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (overpassResult?.Elements == null)
                return new List<OsmBuilding>();
            
            return overpassResult.Elements
                .Where(e => e.Type == "way" || e.Type == "relation")
                .Where(e => e.Tags?.ContainsKey("building") == true)
                .Select(ConvertToOsmBuilding)
                .Where(b => b != null)
                .Cast<OsmBuilding>()
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing Overpass query: {ex.Message}");
            return new List<OsmBuilding>();
        }
    }
    
    private static OsmBuilding? ConvertToOsmBuilding(OverpassElement element)
    {
        if (element.Tags == null || !element.Tags.ContainsKey("building"))
            return null;
        
        var building = new OsmBuilding
        {
            Id = element.Id,
            BuildingType = element.Tags.GetValueOrDefault("building"),
            RoofColor = element.Tags.GetValueOrDefault("roof:colour"),
            Version = element.Version ?? 1,
            LastModified = element.Timestamp ?? DateTime.UtcNow
        };
        
        // Calculate center point and bounding box from geometry
        if (element.Geometry != null && element.Geometry.Length > 0)
        {
            var lats = element.Geometry.Select(g => g.Lat).ToArray();
            var lons = element.Geometry.Select(g => g.Lon).ToArray();
            
            building.Location = new GeoLocation
            {
                Lat = lats.Average(),
                Lon = lons.Average()
            };
            
            building.BoundingBox = new BoundingBox
            {
                MinY = lats.Min(),
                MaxY = lats.Max(),
                MinX = lons.Min(),
                MaxX = lons.Max()
            };
            
            // Store node IDs for ways
            if (element.Type == "way" && element.Nodes != null)
            {
                building.NodeIds = element.Nodes.ToList();
            }
        }
        
        return building;
    }
}

/// <summary>
/// Overpass API response structure
/// </summary>
internal class OverpassResult
{
    public OverpassElement[]? Elements { get; set; }
}

/// <summary>
/// Overpass API element structure
/// </summary>
internal class OverpassElement
{
    public long Id { get; set; }
    public string? Type { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
    public long[]? Nodes { get; set; }
    public OverpassGeometry[]? Geometry { get; set; }
    public int? Version { get; set; }
    public DateTime? Timestamp { get; set; }
}

/// <summary>
/// Overpass API geometry structure
/// </summary>
internal class OverpassGeometry
{
    public double Lat { get; set; }
    public double Lon { get; set; }
}