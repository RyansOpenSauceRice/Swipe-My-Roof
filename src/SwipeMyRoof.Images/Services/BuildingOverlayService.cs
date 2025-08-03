using SwipeMyRoof.Images.Models;
using SwipeMyRoof.OSM.Models;
using SkiaSharp;

namespace SwipeMyRoof.Images.Services;

/// <summary>
/// Service for creating building overlays on satellite imagery (following GoMap's vector overlay approach)
/// </summary>
public interface IBuildingOverlayService
{
    /// <summary>
    /// Create building overlay for satellite imagery
    /// </summary>
    /// <param name="bounds">Image bounds</param>
    /// <param name="width">Image width</param>
    /// <param name="height">Image height</param>
    /// <param name="zoomLevel">Map zoom level</param>
    /// <returns>Building overlay data</returns>
    Task<BuildingOverlay> CreateBuildingOverlayAsync(BoundingBox bounds, int width, int height, int zoomLevel);
    
    /// <summary>
    /// Create composite image with building highlighted on satellite imagery
    /// </summary>
    /// <param name="satelliteImageData">Base satellite image</param>
    /// <param name="overlay">Building overlay data</param>
    /// <returns>Composite image data</returns>
    Task<byte[]> CreateCompositeImageAsync(byte[] satelliteImageData, BuildingOverlay overlay);
    
    /// <summary>
    /// Convert geographic coordinates to pixel coordinates
    /// </summary>
    /// <param name="lat">Latitude</param>
    /// <param name="lon">Longitude</param>
    /// <param name="bounds">Image bounds</param>
    /// <param name="width">Image width</param>
    /// <param name="height">Image height</param>
    /// <returns>Pixel coordinate</returns>
    PixelCoordinate GeographicToPixel(double lat, double lon, BoundingBox bounds, int width, int height);
    
    /// <summary>
    /// Convert building geometry to pixel coordinates
    /// </summary>
    /// <param name="building">OSM building data</param>
    /// <param name="bounds">Image bounds</param>
    /// <param name="width">Image width</param>
    /// <param name="height">Image height</param>
    /// <returns>Building overlay</returns>
    BuildingOverlay ConvertBuildingToOverlay(OsmBuilding building, BoundingBox bounds, int width, int height);
}

/// <summary>
/// Implementation of building overlay service (following GoMap's approach)
/// </summary>
public class BuildingOverlayService : IBuildingOverlayService
{
    private readonly IOverpassService _overpassService;
    
    public BuildingOverlayService(IOverpassService overpassService)
    {
        _overpassService = overpassService;
    }
    
    /// <inheritdoc />
    public async Task<BuildingOverlay> CreateBuildingOverlayAsync(BoundingBox bounds, int width, int height, int zoomLevel)
    {
        try
        {
            // Query buildings in the area (similar to how GoMap loads OSM data)
            var buildings = await _overpassService.GetBuildingsInAreaAsync(
                bounds.South, bounds.West, bounds.North, bounds.East);
            
            if (!buildings.Any())
            {
                return new BuildingOverlay();
            }
            
            // For now, use the first building (in the actual app, this would be the target building)
            var targetBuilding = buildings.First();
            
            return ConvertBuildingToOverlay(targetBuilding, bounds, width, height);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create building overlay: {ex.Message}", ex);
        }
    }
    
    /// <inheritdoc />
    public BuildingOverlay ConvertBuildingToOverlay(OsmBuilding building, BoundingBox bounds, int width, int height)
    {
        var overlay = new BuildingOverlay();
        
        // Convert building outline to pixel coordinates
        if (building.Geometry?.Coordinates?.Any() == true)
        {
            foreach (var coordinate in building.Geometry.Coordinates)
            {
                var pixelCoord = GeographicToPixel(coordinate.Lat, coordinate.Lon, bounds, width, height);
                overlay.OutlinePixels.Add(pixelCoord);
                overlay.OutlineCoordinates.Add(new GeographicCoordinate 
                { 
                    Latitude = coordinate.Lat, 
                    Longitude = coordinate.Lon 
                });
            }
        }
        
        // Calculate building center
        if (overlay.OutlinePixels.Any())
        {
            var centerX = overlay.OutlinePixels.Average(p => p.X);
            var centerY = overlay.OutlinePixels.Average(p => p.Y);
            overlay.CenterPixel = new PixelCoordinate { X = centerX, Y = centerY };
        }
        
        // For roof area, use the same outline (could be refined with roof-specific geometry)
        overlay.RoofAreaPixels = new List<PixelCoordinate>(overlay.OutlinePixels);
        
        // Calculate area (simplified calculation)
        overlay.AreaSquareMeters = CalculateBuildingArea(overlay.OutlineCoordinates);
        
        // Set highlight color (could be based on proposed roof color)
        overlay.HighlightColor = "#FF4444"; // Red highlight like GoMap
        
        return overlay;
    }
    
    /// <inheritdoc />
    public PixelCoordinate GeographicToPixel(double lat, double lon, BoundingBox bounds, int width, int height)
    {
        // Convert geographic coordinates to pixel coordinates
        // This uses a simple linear projection - GoMap uses more sophisticated Mercator projection
        
        var x = (lon - bounds.West) / (bounds.East - bounds.West) * width;
        var y = (bounds.North - lat) / (bounds.North - bounds.South) * height; // Y is inverted
        
        return new PixelCoordinate { X = x, Y = y };
    }
    
    /// <inheritdoc />
    public async Task<byte[]> CreateCompositeImageAsync(byte[] satelliteImageData, BuildingOverlay overlay)
    {
        try
        {
            // Use SkiaSharp to create composite image (similar to GoMap's rendering)
            using var originalBitmap = SKBitmap.Decode(satelliteImageData);
            using var surface = SKSurface.Create(new SKImageInfo(originalBitmap.Width, originalBitmap.Height));
            using var canvas = surface.Canvas;
            
            // Draw the satellite image as base layer
            canvas.DrawBitmap(originalBitmap, 0, 0);
            
            // Draw building outline overlay (following GoMap's style)
            if (overlay.OutlinePixels.Count > 2)
            {
                using var outlinePaint = new SKPaint
                {
                    Color = SKColor.Parse("#FF4444"), // Red outline like GoMap
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3,
                    IsAntialias = true
                };
                
                using var fillPaint = new SKPaint
                {
                    Color = SKColor.Parse("#44FF4444"), // Semi-transparent red fill
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                
                // Create path from building outline
                using var path = new SKPath();
                var firstPoint = overlay.OutlinePixels.First();
                path.MoveTo((float)firstPoint.X, (float)firstPoint.Y);
                
                foreach (var point in overlay.OutlinePixels.Skip(1))
                {
                    path.LineTo((float)point.X, (float)point.Y);
                }
                path.Close();
                
                // Draw filled area first, then outline
                canvas.DrawPath(path, fillPaint);
                canvas.DrawPath(path, outlinePaint);
                
                // Draw center point
                using var centerPaint = new SKPaint
                {
                    Color = SKColors.Yellow,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                
                canvas.DrawCircle(
                    (float)overlay.CenterPixel.X, 
                    (float)overlay.CenterPixel.Y, 
                    5, 
                    centerPaint);
            }
            
            // Convert to byte array
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
            return data.ToArray();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create composite image: {ex.Message}", ex);
        }
    }
    
    private static double CalculateBuildingArea(List<GeographicCoordinate> coordinates)
    {
        if (coordinates.Count < 3)
            return 0;
        
        // Simplified area calculation using shoelace formula
        // This is approximate - GoMap uses more precise geodesic calculations
        double area = 0;
        
        for (int i = 0; i < coordinates.Count; i++)
        {
            var j = (i + 1) % coordinates.Count;
            area += coordinates[i].Longitude * coordinates[j].Latitude;
            area -= coordinates[j].Longitude * coordinates[i].Latitude;
        }
        
        area = Math.Abs(area) / 2.0;
        
        // Convert from degrees to approximate square meters
        // This is very rough - proper calculation requires geodesic math
        const double degreesToMeters = 111320; // Approximate meters per degree at equator
        return area * degreesToMeters * degreesToMeters;
    }
}