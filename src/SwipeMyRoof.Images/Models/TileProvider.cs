using System;
using System.Collections.Generic;

namespace SwipeMyRoof.Images.Models;

/// <summary>
/// Type of tile provider
/// </summary>
public enum TileProviderType
{
    /// <summary>
    /// XYZ tile format (e.g., OpenStreetMap)
    /// </summary>
    XYZ,
    
    /// <summary>
    /// Bing Maps
    /// </summary>
    Bing,
    
    /// <summary>
    /// Mapbox
    /// </summary>
    Mapbox,
    
    /// <summary>
    /// Web Map Service (WMS)
    /// </summary>
    WMS,
    
    /// <summary>
    /// Web Map Tile Service (WMTS)
    /// </summary>
    WMTS,
    
    /// <summary>
    /// TMS (Tile Map Service)
    /// </summary>
    TMS
}

/// <summary>
/// Represents a tile provider for satellite imagery
/// </summary>
public class TileProvider
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// URL template for fetching tiles
    /// </summary>
    /// <remarks>
    /// Placeholders:
    /// - {x}, {y}, {z}: Tile coordinates and zoom level
    /// - {apikey}: API key
    /// - {quadkey}: Quadkey (for Bing Maps)
    /// - {width}, {height}: Image dimensions
    /// - {latitude}, {longitude}: Coordinates (for Bing Maps)
    /// </remarks>
    public string UrlTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// API key (if required)
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// Attribution text
    /// </summary>
    public string Attribution { get; set; } = string.Empty;
    
    /// <summary>
    /// Attribution URL
    /// </summary>
    public string AttributionUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum zoom level
    /// </summary>
    public int MaxZoom { get; set; } = 19;
    
    /// <summary>
    /// Minimum zoom level
    /// </summary>
    public int MinZoom { get; set; } = 1;
    
    /// <summary>
    /// Number of days to cache tiles
    /// </summary>
    public int CacheDays { get; set; } = 30;
    
    /// <summary>
    /// Type of tile provider
    /// </summary>
    public TileProviderType Type { get; set; } = TileProviderType.XYZ;
    
    /// <summary>
    /// Whether this provider is user-defined (vs. built-in)
    /// </summary>
    public bool IsUserDefined { get; set; } = false;
    
    /// <summary>
    /// Geographic bounds where this provider is available
    /// </summary>
    public BoundingBox? Bounds { get; set; }
    
    /// <summary>
    /// Start date of imagery (if applicable)
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// End date of imagery (if applicable)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Additional headers to send with requests
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();
}