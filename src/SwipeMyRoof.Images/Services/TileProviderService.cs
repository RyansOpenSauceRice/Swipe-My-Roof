using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Images.Models;
using SwipeMyRoof.Storage.Services;

namespace SwipeMyRoof.Images.Services;

/// <summary>
/// Service for managing tile providers
/// </summary>
public class TileProviderService : ITileProviderService
{
    private readonly HttpClient _httpClient;
    private readonly IStorageService _storageService;
    private readonly string _cacheDirectory;
    private readonly Dictionary<string, TileProvider> _providers = new();
    private TileProvider? _currentProvider;
    
    private const string STORAGE_KEY_CURRENT_PROVIDER = "CurrentTileProvider";
    private const string STORAGE_KEY_PROVIDERS = "TileProviders";
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClient">HTTP client</param>
    /// <param name="storageService">Storage service</param>
    public TileProviderService(HttpClient httpClient, IStorageService storageService)
    {
        _httpClient = httpClient;
        _storageService = storageService;
        
        // Set up cache directory
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _cacheDirectory = Path.Combine(appDataDir, "SwipeMyRoof", "TileCache");
        Directory.CreateDirectory(_cacheDirectory);
        
        // Load providers from storage
        LoadProviders();
    }
    
    /// <inheritdoc />
    public IReadOnlyList<TileProvider> GetAllProviders()
    {
        return new List<TileProvider>(_providers.Values);
    }
    
    /// <inheritdoc />
    public TileProvider? GetCurrentProvider()
    {
        return _currentProvider;
    }
    
    /// <inheritdoc />
    public void SetCurrentProvider(string providerId)
    {
        if (_providers.TryGetValue(providerId, out var provider))
        {
            _currentProvider = provider;
            _storageService.Set(STORAGE_KEY_CURRENT_PROVIDER, providerId);
        }
    }
    
    /// <inheritdoc />
    public void AddProvider(TileProvider provider)
    {
        _providers[provider.Id] = provider;
        SaveProviders();
    }
    
    /// <inheritdoc />
    public void RemoveProvider(string providerId)
    {
        if (_providers.Remove(providerId))
        {
            if (_currentProvider?.Id == providerId)
            {
                _currentProvider = _providers.Count > 0 ? _providers.Values.First() : null;
                _storageService.Set(STORAGE_KEY_CURRENT_PROVIDER, _currentProvider?.Id);
            }
            
            SaveProviders();
        }
    }
    
    /// <inheritdoc />
    public async Task<byte[]> GetTileAsync(double latitude, double longitude, int zoom, CancellationToken cancellationToken = default)
    {
        if (_currentProvider == null)
        {
            throw new InvalidOperationException("No tile provider selected");
        }
        
        // Convert lat/lon to tile coordinates
        var (x, y) = LatLonToTile(latitude, longitude, zoom);
        
        return await GetTileAsync(_currentProvider, x, y, zoom, cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<byte[]> GetTileAsync(TileProvider provider, int x, int y, int zoom, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"{provider.Id}_{zoom}_{x}_{y}";
        var cachePath = Path.Combine(_cacheDirectory, cacheKey);
        
        if (File.Exists(cachePath))
        {
            try
            {
                var cacheAge = DateTime.UtcNow - File.GetLastWriteTimeUtc(cachePath);
                if (cacheAge.TotalDays < provider.CacheDays)
                {
                    return await File.ReadAllBytesAsync(cachePath, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading tile from cache: {ex.Message}");
            }
        }
        
        // Cache miss or expired, fetch from server
        try
        {
            var url = provider.UrlTemplate
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString())
                .Replace("{z}", zoom.ToString())
                .Replace("{apikey}", provider.ApiKey ?? string.Empty)
                .Replace("{quadkey}", TileToQuadKey(x, y, zoom));
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var tileData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            // Save to cache
            try
            {
                await File.WriteAllBytesAsync(cachePath, tileData, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing tile to cache: {ex.Message}");
            }
            
            return tileData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching tile: {ex.Message}");
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<BuildingImage> GetBuildingImageAsync(BuildingCandidate building, ImageQualitySettings? settings = null, CancellationToken cancellationToken = default)
    {
        settings ??= new ImageQualitySettings();
        
        try
        {
            if (_currentProvider == null)
            {
                throw new InvalidOperationException("No tile provider selected");
            }
            
            // Calculate the bounding box with buffer
            var bbox = CalculateBoundingBoxWithBuffer(building.BoundingBox, settings.BuildingBufferRatio);
            
            // Determine image dimensions
            int width = settings.UseHighResolution ? 256 : settings.TargetWidth;
            int height = settings.UseHighResolution ? 256 : settings.TargetHeight;
            
            // Determine zoom level based on bounding box size
            int zoom = CalculateOptimalZoom(bbox, width, height);
            if (zoom > _currentProvider.MaxZoom)
            {
                zoom = _currentProvider.MaxZoom;
            }
            
            // Get center coordinates
            double centerLat = (bbox.MinY + bbox.MaxY) / 2;
            double centerLon = (bbox.MinX + bbox.MaxX) / 2;
            
            // Get the tile
            var tileData = await GetTileAsync(centerLat, centerLon, zoom, cancellationToken);
            
            // Create the building image
            var buildingImage = new BuildingImage
            {
                OsmId = building.OsmId,
                ImageData = tileData,
                Width = width,
                Height = height,
                Format = "jpeg",
                BuildingRatio = CalculateBuildingRatio(bbox),
                IsComplete = true,
                Timestamp = DateTime.UtcNow
            };
            
            // Extract dominant colors (in a real implementation, this would analyze the image)
            buildingImage.DominantColors = new List<string>
            {
                "dark gray",
                "light gray",
                "brown"
            };
            
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
    
    private void LoadProviders()
    {
        // Load providers from storage
        var providers = _storageService.Get<List<TileProvider>>(STORAGE_KEY_PROVIDERS);
        if (providers != null)
        {
            foreach (var provider in providers)
            {
                _providers[provider.Id] = provider;
            }
        }
        
        // If no providers, add default ones
        if (_providers.Count == 0)
        {
            AddDefaultProviders();
        }
        
        // Load current provider
        var currentProviderId = _storageService.Get<string>(STORAGE_KEY_CURRENT_PROVIDER);
        if (currentProviderId != null && _providers.TryGetValue(currentProviderId, out var currentProvider))
        {
            _currentProvider = currentProvider;
        }
        else if (_providers.Count > 0)
        {
            _currentProvider = _providers.Values.First();
        }
    }
    
    private void SaveProviders()
    {
        _storageService.Set(STORAGE_KEY_PROVIDERS, _providers.Values.ToList());
    }
    
    private void AddDefaultProviders()
    {
        // Add Bing Maps
        var bingProvider = new TileProvider
        {
            Id = "bing",
            Name = "Bing Maps Aerial",
            UrlTemplate = "https://dev.virtualearth.net/REST/v1/Imagery/Map/Aerial/{latitude},{longitude}/{zoom}?mapSize={width},{height}&key={apikey}",
            ApiKey = "YOUR_BING_MAPS_KEY", // Replace with your actual key
            Attribution = "© Microsoft Bing",
            AttributionUrl = "https://www.bing.com/maps/",
            MaxZoom = 19,
            MinZoom = 1,
            CacheDays = 30,
            Type = TileProviderType.Bing
        };
        _providers[bingProvider.Id] = bingProvider;
        
        // Add OpenStreetMap
        var osmProvider = new TileProvider
        {
            Id = "osm",
            Name = "OpenStreetMap",
            UrlTemplate = "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
            Attribution = "© OpenStreetMap contributors",
            AttributionUrl = "https://www.openstreetmap.org/copyright",
            MaxZoom = 19,
            MinZoom = 1,
            CacheDays = 7,
            Type = TileProviderType.XYZ
        };
        _providers[osmProvider.Id] = osmProvider;
        
        // Add Mapbox Satellite
        var mapboxProvider = new TileProvider
        {
            Id = "mapbox-satellite",
            Name = "Mapbox Satellite",
            UrlTemplate = "https://api.mapbox.com/v4/mapbox.satellite/{z}/{x}/{y}.jpg90?access_token={apikey}",
            ApiKey = "YOUR_MAPBOX_KEY", // Replace with your actual key
            Attribution = "© Mapbox",
            AttributionUrl = "https://www.mapbox.com/about/maps/",
            MaxZoom = 22,
            MinZoom = 1,
            CacheDays = 30,
            Type = TileProviderType.XYZ
        };
        _providers[mapboxProvider.Id] = mapboxProvider;
        
        // Add ESRI World Imagery
        var esriProvider = new TileProvider
        {
            Id = "esri-world-imagery",
            Name = "ESRI World Imagery",
            UrlTemplate = "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}",
            Attribution = "© Esri",
            AttributionUrl = "https://www.esri.com",
            MaxZoom = 19,
            MinZoom = 1,
            CacheDays = 30,
            Type = TileProviderType.XYZ
        };
        _providers[esriProvider.Id] = esriProvider;
    }
    
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
    
    private int CalculateOptimalZoom(BoundingBox bbox, int width, int height)
    {
        // Calculate the optimal zoom level based on the bounding box size and target image dimensions
        double latDiff = bbox.MaxY - bbox.MinY;
        double lonDiff = bbox.MaxX - bbox.MinX;
        
        // This is a simplified calculation; in a real implementation, you would need to consider
        // the actual projection and tile size
        double zoomLat = Math.Log(360 * height / (256 * latDiff)) / Math.Log(2);
        double zoomLon = Math.Log(360 * width / (256 * lonDiff)) / Math.Log(2);
        
        int zoom = (int)Math.Min(zoomLat, zoomLon);
        
        // Ensure zoom is within valid range
        zoom = Math.Max(1, Math.Min(zoom, 19));
        
        return zoom;
    }
    
    private double CalculateBuildingRatio(BoundingBox bbox)
    {
        // In a real implementation, this would calculate the ratio based on the bounding box
        // For now, we'll return a dummy value
        
        return 1.3;
    }
    
    private (int x, int y) LatLonToTile(double lat, double lon, int zoom)
    {
        // Convert latitude and longitude to tile coordinates
        double n = Math.Pow(2, zoom);
        double latRad = lat * Math.PI / 180;
        
        int x = (int)((lon + 180) / 360 * n);
        int y = (int)((1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * n);
        
        return (x, y);
    }
    
    private string TileToQuadKey(int x, int y, int zoom)
    {
        // Convert tile coordinates to a quadkey (used by Bing Maps)
        var quadKey = new char[zoom];
        
        for (int i = zoom - 1; i >= 0; i--)
        {
            char digit = '0';
            int mask = 1 << i;
            
            if ((x & mask) != 0)
            {
                digit++;
            }
            
            if ((y & mask) != 0)
            {
                digit += (char)2;
            }
            
            quadKey[zoom - 1 - i] = digit;
        }
        
        return new string(quadKey);
    }
    
    #endregion
}