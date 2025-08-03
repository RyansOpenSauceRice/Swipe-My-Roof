using System.Text;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Images.Models;

namespace SwipeMyRoof.Images.Services;

/// <summary>
/// Bing Maps implementation of the image service
/// </summary>
public class BingImageService : IImageService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClient">HTTP client</param>
    /// <param name="apiKey">Bing Maps API key</param>
    public BingImageService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }
    
    /// <inheritdoc />
    public async Task<BuildingImage> GetBuildingImageAsync(BuildingCandidate building, ImageQualitySettings? settings = null, CancellationToken cancellationToken = default)
    {
        settings ??= new ImageQualitySettings();
        
        try
        {
            // Calculate the bounding box with buffer
            var bbox = CalculateBoundingBoxWithBuffer(building.BoundingBox, settings.BuildingBufferRatio);
            
            // Determine image dimensions
            int width = settings.UseHighResolution ? 128 : settings.TargetWidth;
            int height = settings.UseHighResolution ? 128 : settings.TargetHeight;
            
            // Construct the Bing Maps Static Image API URL
            var url = ConstructBingMapsUrl(bbox, width, height);
            
            // Fetch the image
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var imageData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            // Create the building image
            var buildingImage = new BuildingImage
            {
                OsmId = building.OsmId,
                ImageData = imageData,
                Width = width,
                Height = height,
                Format = "jpeg",
                BuildingRatio = CalculateBuildingRatio(bbox),
                IsComplete = true,
                Timestamp = DateTime.UtcNow
            };
            
            // Extract dominant colors
            buildingImage.DominantColors = ExtractDominantColors(buildingImage);
            
            return buildingImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting building image: {ex.Message}");
            
            // Return a placeholder image
            return new BuildingImage
            {
                OsmId = building.OsmId,
                ImageData = new byte[0],
                Width = 0,
                Height = 0,
                Format = "jpeg",
                BuildingRatio = 1.0,
                IsComplete = false,
                Timestamp = DateTime.UtcNow
            };
        }
    }
    
    /// <inheritdoc />
    public async Task<string> GetBuildingThumbnailBase64Async(BuildingCandidate building, int width = 64, int height = 64, CancellationToken cancellationToken = default)
    {
        var settings = new ImageQualitySettings
        {
            TargetWidth = width,
            TargetHeight = height,
            UseHighResolution = false
        };
        
        var image = await GetBuildingImageAsync(building, settings, cancellationToken);
        
        if (image.ImageData.Length == 0)
        {
            return string.Empty;
        }
        
        return Convert.ToBase64String(image.ImageData);
    }
    
    /// <inheritdoc />
    public List<string> ExtractDominantColors(BuildingImage image, int maxColors = 3)
    {
        // In a real implementation, this would analyze the image to extract dominant colors
        // For now, we'll return dummy colors
        
        return new List<string>
        {
            "dark gray",
            "light gray",
            "brown"
        };
    }
    
    /// <inheritdoc />
    public double CalculateBuildingRatio(BuildingImage image)
    {
        // In a real implementation, this would calculate the ratio of building to background
        // For now, we'll return a dummy value
        
        return 1.3;
    }
    
    #region Private Methods
    
    private BoundingBox CalculateBoundingBoxWithBuffer(BoundingBox bbox, double bufferRatio)
    {
        double width = bbox.MaxX - bbox.MinX;
        double height = bbox.MaxY - bbox.MinY;
        
        double bufferX = width * bufferRatio / 2;
        double bufferY = height * bufferRatio / 2;
        
        return new BoundingBox
        {
            MinX = bbox.MinX - bufferX,
            MinY = bbox.MinY - bufferY,
            MaxX = bbox.MaxX + bufferX,
            MaxY = bbox.MaxY + bufferY
        };
    }
    
    private string ConstructBingMapsUrl(BoundingBox bbox, int width, int height)
    {
        var sb = new StringBuilder("https://dev.virtualearth.net/REST/v1/Imagery/Map/Aerial/?");
        
        // Add bounding box
        sb.Append($"mapArea={bbox.MinY},{bbox.MinX},{bbox.MaxY},{bbox.MaxX}");
        
        // Add dimensions
        sb.Append($"&mapSize={width},{height}");
        
        // Add API key
        sb.Append($"&key={_apiKey}");
        
        return sb.ToString();
    }
    
    private double CalculateBuildingRatio(BoundingBox bbox)
    {
        // In a real implementation, this would calculate the ratio based on the bounding box
        // For now, we'll return a dummy value
        
        return 1.3;
    }
    
    #endregion
}