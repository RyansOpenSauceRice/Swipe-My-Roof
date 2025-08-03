using System.Text.Json;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Nominatim implementation of location search service
/// </summary>
public class NominatimLocationSearchService : ILocationSearchService
{
    private readonly HttpClient _httpClient;
    private const string NominatimBaseUrl = "https://nominatim.openstreetmap.org";
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClient">HTTP client</param>
    public NominatimLocationSearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // Set user agent as required by Nominatim usage policy
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SwipeMyRoof/1.0 (https://github.com/RyansOpenSauceRice/Swipe-My-Roof)");
    }
    
    /// <inheritdoc />
    public async Task<List<LocationSearchResult>> SearchLocationsAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{NominatimBaseUrl}/search?q={Uri.EscapeDataString(query)}&format=json&limit={limit}&addressdetails=1&extratags=1&namedetails=1";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var nominatimResults = JsonSerializer.Deserialize<NominatimSearchResult[]>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (nominatimResults == null)
                return new List<LocationSearchResult>();
            
            return nominatimResults.Select(ConvertToLocationSearchResult).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching locations: {ex.Message}");
            return new List<LocationSearchResult>();
        }
    }
    
    /// <inheritdoc />
    public async Task<LocationSearchResult?> GetLocationDetailsAsync(long placeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{NominatimBaseUrl}/details?place_id={placeId}&format=json&addressdetails=1&extratags=1&namedetails=1";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var nominatimResult = JsonSerializer.Deserialize<NominatimSearchResult>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return nominatimResult != null ? ConvertToLocationSearchResult(nominatimResult) : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting location details: {ex.Message}");
            return null;
        }
    }
    
    private static LocationSearchResult ConvertToLocationSearchResult(NominatimSearchResult nominatimResult)
    {
        var result = new LocationSearchResult
        {
            DisplayName = nominatimResult.DisplayName ?? string.Empty,
            Location = new GeoLocation
            {
                Lat = double.Parse(nominatimResult.Lat ?? "0"),
                Lon = double.Parse(nominatimResult.Lon ?? "0")
            },
            Type = nominatimResult.Type ?? string.Empty,
            Importance = nominatimResult.Importance ?? 0.0,
            PlaceId = nominatimResult.PlaceId ?? 0
        };
        
        // Parse bounding box if available
        if (nominatimResult.BoundingBox != null && nominatimResult.BoundingBox.Length == 4)
        {
            result.BoundingBox = new BoundingBox
            {
                MinY = double.Parse(nominatimResult.BoundingBox[0]), // South
                MaxY = double.Parse(nominatimResult.BoundingBox[1]), // North
                MinX = double.Parse(nominatimResult.BoundingBox[2]), // West
                MaxX = double.Parse(nominatimResult.BoundingBox[3])  // East
            };
        }
        
        return result;
    }
}

/// <summary>
/// Nominatim API response structure
/// </summary>
internal class NominatimSearchResult
{
    public long? PlaceId { get; set; }
    public string? DisplayName { get; set; }
    public string? Type { get; set; }
    public double? Importance { get; set; }
    public string? Lat { get; set; }
    public string? Lon { get; set; }
    public string[]? BoundingBox { get; set; }
}