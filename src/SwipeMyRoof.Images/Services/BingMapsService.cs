using SwipeMyRoof.Images.Models;

namespace SwipeMyRoof.Images.Services;

/// <summary>
/// Service for integrating with Bing Maps imagery (following GoMap's approach)
/// </summary>
public interface IBingMapsService
{
    /// <summary>
    /// Get Bing Maps tile URL for given coordinates and zoom level
    /// </summary>
    /// <param name="x">Tile X coordinate</param>
    /// <param name="y">Tile Y coordinate</param>
    /// <param name="zoom">Zoom level (1-21)</param>
    /// <param name="imageType">Type of imagery (Aerial, Road, etc.)</param>
    /// <returns>Tile URL</returns>
    string GetTileUrl(int x, int y, int zoom, BingImageryType imageType = BingImageryType.Aerial);
    
    /// <summary>
    /// Get Bing Maps tile URL using QuadKey (Bing's tile naming system)
    /// </summary>
    /// <param name="quadKey">Bing Maps QuadKey</param>
    /// <param name="imageType">Type of imagery</param>
    /// <returns>Tile URL</returns>
    string GetTileUrlByQuadKey(string quadKey, BingImageryType imageType = BingImageryType.Aerial);
    
    /// <summary>
    /// Convert tile coordinates to QuadKey (Bing's system)
    /// </summary>
    /// <param name="x">Tile X coordinate</param>
    /// <param name="y">Tile Y coordinate</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>QuadKey string</returns>
    string TileToQuadKey(int x, int y, int zoom);
    
    /// <summary>
    /// Get building imagery for specific building bounds
    /// </summary>
    /// <param name="bounds">Building bounding box</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Building imagery data</returns>
    Task<BuildingImagery> GetBuildingImageryAsync(
        BoundingBox bounds, 
        int width = 512, 
        int height = 512, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get attribution text for Bing Maps (required by license)
    /// </summary>
    /// <returns>Attribution text</returns>
    string GetAttributionText();
    
    /// <summary>
    /// Initialize Bing Maps service with API key
    /// </summary>
    /// <param name="apiKey">Bing Maps API key</param>
    Task InitializeAsync(string apiKey);
}

/// <summary>
/// Bing Maps imagery types
/// </summary>
public enum BingImageryType
{
    /// <summary>
    /// Aerial satellite imagery
    /// </summary>
    Aerial,
    
    /// <summary>
    /// Aerial imagery with road labels
    /// </summary>
    AerialWithLabels,
    
    /// <summary>
    /// Road map view
    /// </summary>
    Road,
    
    /// <summary>
    /// Canvas (light map style)
    /// </summary>
    CanvasLight,
    
    /// <summary>
    /// Canvas (dark map style)
    /// </summary>
    CanvasDark
}

/// <summary>
/// Building imagery data with satellite background and building overlay
/// </summary>
public class BuildingImagery
{
    /// <summary>
    /// Satellite imagery as base layer
    /// </summary>
    public byte[] SatelliteImageData { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Building outline overlay data
    /// </summary>
    public BuildingOverlay BuildingOverlay { get; set; } = new();
    
    /// <summary>
    /// Combined image with building highlighted
    /// </summary>
    public byte[] CompositeImageData { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Image bounds in geographic coordinates
    /// </summary>
    public BoundingBox Bounds { get; set; } = new();
    
    /// <summary>
    /// Image dimensions
    /// </summary>
    public ImageDimensions Dimensions { get; set; } = new();
    
    /// <summary>
    /// Attribution text for the imagery
    /// </summary>
    public string Attribution { get; set; } = string.Empty;
}

/// <summary>
/// Building overlay information (following GoMap's vector overlay approach)
/// </summary>
public class BuildingOverlay
{
    /// <summary>
    /// Building outline coordinates in image pixel space
    /// </summary>
    public List<PixelCoordinate> OutlinePixels { get; set; } = new();
    
    /// <summary>
    /// Building outline coordinates in geographic space
    /// </summary>
    public List<GeographicCoordinate> OutlineCoordinates { get; set; } = new();
    
    /// <summary>
    /// Roof area polygon (if different from building outline)
    /// </summary>
    public List<PixelCoordinate> RoofAreaPixels { get; set; } = new();
    
    /// <summary>
    /// Building center point in pixels
    /// </summary>
    public PixelCoordinate CenterPixel { get; set; } = new();
    
    /// <summary>
    /// Suggested highlight color for the building
    /// </summary>
    public string HighlightColor { get; set; } = "#FF0000";
    
    /// <summary>
    /// Building area in square meters
    /// </summary>
    public double AreaSquareMeters { get; set; }
}

/// <summary>
/// Image dimensions
/// </summary>
public class ImageDimensions
{
    public int Width { get; set; }
    public int Height { get; set; }
    public double MetersPerPixel { get; set; }
}

/// <summary>
/// Pixel coordinate in image space
/// </summary>
public class PixelCoordinate
{
    public double X { get; set; }
    public double Y { get; set; }
}

/// <summary>
/// Geographic coordinate
/// </summary>
public class GeographicCoordinate
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

/// <summary>
/// Implementation of Bing Maps service (following GoMap's approach)
/// </summary>
public class BingMapsService : IBingMapsService
{
    private string? _apiKey;
    private readonly HttpClient _httpClient;
    private readonly IBuildingOverlayService _overlayService;
    
    // Bing Maps tile server URLs (similar to GoMap's configuration)
    private const string BingTileUrlTemplate = "https://ecn.t{subdomain}.tiles.virtualearth.net/tiles/{imagerySet}{quadkey}.jpeg?g=13515&mkt=en-US&key={apiKey}";
    private const string BingStaticMapUrl = "https://dev.virtualearth.net/REST/v1/Imagery/Map/{imagerySet}/{centerPoint}/{zoomLevel}?mapSize={width},{height}&format=jpeg&key={apiKey}";
    
    public BingMapsService(HttpClient httpClient, IBuildingOverlayService overlayService)
    {
        _httpClient = httpClient;
        _overlayService = overlayService;
    }
    
    /// <inheritdoc />
    public async Task InitializeAsync(string apiKey)
    {
        _apiKey = apiKey;
        
        // Validate API key by making a test request
        try
        {
            var testUrl = GetTileUrl(0, 0, 1);
            var response = await _httpClient.GetAsync(testUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Invalid Bing Maps API key. Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize Bing Maps service: {ex.Message}", ex);
        }
    }
    
    /// <inheritdoc />
    public string GetTileUrl(int x, int y, int zoom, BingImageryType imageType = BingImageryType.Aerial)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("Bing Maps API key not initialized");
        
        var quadKey = TileToQuadKey(x, y, zoom);
        return GetTileUrlByQuadKey(quadKey, imageType);
    }
    
    /// <inheritdoc />
    public string GetTileUrlByQuadKey(string quadKey, BingImageryType imageType = BingImageryType.Aerial)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("Bing Maps API key not initialized");
        
        var imagerySet = GetImagerySetName(imageType);
        var subdomain = Math.Abs(quadKey.GetHashCode()) % 4; // Load balance across subdomains
        
        return BingTileUrlTemplate
            .Replace("{subdomain}", subdomain.ToString())
            .Replace("{imagerySet}", imagerySet)
            .Replace("{quadkey}", quadKey)
            .Replace("{apiKey}", _apiKey);
    }
    
    /// <inheritdoc />
    public string TileToQuadKey(int x, int y, int zoom)
    {
        var quadKey = string.Empty;
        
        for (int i = zoom; i > 0; i--)
        {
            char digit = '0';
            int mask = 1 << (i - 1);
            
            if ((x & mask) != 0)
                digit++;
            
            if ((y & mask) != 0)
                digit += 2;
            
            quadKey += digit;
        }
        
        return quadKey;
    }
    
    /// <inheritdoc />
    public async Task<BuildingImagery> GetBuildingImageryAsync(
        BoundingBox bounds, 
        int width = 512, 
        int height = 512, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("Bing Maps API key not initialized");
        
        try
        {
            // Calculate center point and zoom level for the building
            var centerLat = (bounds.North + bounds.South) / 2;
            var centerLon = (bounds.East + bounds.West) / 2;
            var zoomLevel = CalculateOptimalZoomLevel(bounds, width, height);
            
            // Get satellite imagery from Bing Maps
            var imageUrl = BingStaticMapUrl
                .Replace("{imagerySet}", GetImagerySetName(BingImageryType.Aerial))
                .Replace("{centerPoint}", $"{centerLat},{centerLon}")
                .Replace("{zoomLevel}", zoomLevel.ToString())
                .Replace("{width}", width.ToString())
                .Replace("{height}", height.ToString())
                .Replace("{apiKey}", _apiKey);
            
            var imageData = await _httpClient.GetByteArrayAsync(imageUrl, cancellationToken);
            
            // Generate building overlay (following GoMap's vector overlay approach)
            var overlay = await _overlayService.CreateBuildingOverlayAsync(bounds, width, height, zoomLevel);
            
            // Create composite image with building highlighted
            var compositeImage = await _overlayService.CreateCompositeImageAsync(imageData, overlay);
            
            return new BuildingImagery
            {
                SatelliteImageData = imageData,
                BuildingOverlay = overlay,
                CompositeImageData = compositeImage,
                Bounds = bounds,
                Dimensions = new ImageDimensions
                {
                    Width = width,
                    Height = height,
                    MetersPerPixel = CalculateMetersPerPixel(zoomLevel, centerLat)
                },
                Attribution = GetAttributionText()
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get building imagery: {ex.Message}", ex);
        }
    }
    
    /// <inheritdoc />
    public string GetAttributionText()
    {
        return "© Microsoft Corporation, © DigitalGlobe, © CNES/Airbus DS";
    }
    
    private static string GetImagerySetName(BingImageryType imageType)
    {
        return imageType switch
        {
            BingImageryType.Aerial => "a",
            BingImageryType.AerialWithLabels => "h",
            BingImageryType.Road => "r",
            BingImageryType.CanvasLight => "c",
            BingImageryType.CanvasDark => "cd",
            _ => "a"
        };
    }
    
    private static int CalculateOptimalZoomLevel(BoundingBox bounds, int width, int height)
    {
        // Calculate the span of the bounding box
        var latSpan = Math.Abs(bounds.North - bounds.South);
        var lonSpan = Math.Abs(bounds.East - bounds.West);
        
        // Calculate zoom level based on desired resolution
        // This is a simplified calculation - GoMap uses more sophisticated logic
        var maxSpan = Math.Max(latSpan, lonSpan);
        
        // Zoom levels: each level doubles the resolution
        // Level 20 = ~1 meter per pixel, Level 15 = ~32 meters per pixel
        if (maxSpan < 0.0001) return 20; // Very close zoom for small buildings
        if (maxSpan < 0.0005) return 19;
        if (maxSpan < 0.001) return 18;
        if (maxSpan < 0.002) return 17;
        if (maxSpan < 0.005) return 16;
        
        return 15; // Default zoom level
    }
    
    private static double CalculateMetersPerPixel(int zoomLevel, double latitude)
    {
        // Earth's circumference at equator in meters
        const double earthCircumference = 40075016.686;
        
        // Calculate meters per pixel at given zoom level and latitude
        var metersPerPixelAtEquator = earthCircumference / Math.Pow(2, zoomLevel + 8);
        
        // Adjust for latitude (Mercator projection)
        var metersPerPixel = metersPerPixelAtEquator * Math.Cos(latitude * Math.PI / 180);
        
        return metersPerPixel;
    }
}