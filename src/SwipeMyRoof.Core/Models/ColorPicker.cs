namespace SwipeMyRoof.Core.Models;

/// <summary>
/// Represents a color picked from an image
/// </summary>
public class PickedColor
{
    /// <summary>
    /// RGB color values (0-255)
    /// </summary>
    public RgbColor Rgb { get; set; } = new();
    
    /// <summary>
    /// Closest standard palette color
    /// </summary>
    public string StandardColor { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence of the palette mapping (0.0-1.0)
    /// </summary>
    public double MappingConfidence { get; set; }
    
    /// <summary>
    /// X coordinate where color was picked
    /// </summary>
    public int PixelX { get; set; }
    
    /// <summary>
    /// Y coordinate where color was picked
    /// </summary>
    public int PixelY { get; set; }
    
    /// <summary>
    /// Timestamp when color was picked
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// RGB color representation
/// </summary>
public class RgbColor
{
    /// <summary>
    /// Red component (0-255)
    /// </summary>
    public byte R { get; set; }
    
    /// <summary>
    /// Green component (0-255)
    /// </summary>
    public byte G { get; set; }
    
    /// <summary>
    /// Blue component (0-255)
    /// </summary>
    public byte B { get; set; }
    
    /// <summary>
    /// Alpha component (0-255)
    /// </summary>
    public byte A { get; set; } = 255;
    
    /// <summary>
    /// Convert to hex string
    /// </summary>
    /// <returns>Hex color string (e.g., "#FF0000")</returns>
    public string ToHex()
    {
        return $"#{R:X2}{G:X2}{B:X2}";
    }
    
    /// <summary>
    /// Create from hex string
    /// </summary>
    /// <param name="hex">Hex color string (e.g., "#FF0000" or "FF0000")</param>
    /// <returns>RGB color</returns>
    public static RgbColor FromHex(string hex)
    {
        hex = hex.TrimStart('#');
        
        if (hex.Length != 6)
            throw new ArgumentException("Invalid hex color format");
        
        return new RgbColor
        {
            R = Convert.ToByte(hex.Substring(0, 2), 16),
            G = Convert.ToByte(hex.Substring(2, 2), 16),
            B = Convert.ToByte(hex.Substring(4, 2), 16)
        };
    }
}

/// <summary>
/// Standard roof color palette with RGB mappings
/// </summary>
public static class RoofColorPalette
{
    /// <summary>
    /// Standard roof colors with their typical RGB values
    /// </summary>
    public static readonly Dictionary<string, RgbColor> StandardColors = new()
    {
        { "black", new RgbColor { R = 30, G = 30, B = 30 } },
        { "dark gray", new RgbColor { R = 80, G = 80, B = 80 } },
        { "light gray", new RgbColor { R = 180, G = 180, B = 180 } },
        { "red", new RgbColor { R = 180, G = 50, B = 50 } },
        { "brown", new RgbColor { R = 120, G = 80, B = 50 } },
        { "tan", new RgbColor { R = 210, G = 180, B = 140 } },
        { "green", new RgbColor { R = 80, G = 120, B = 60 } },
        { "blue", new RgbColor { R = 60, G = 100, B = 150 } },
        { "white", new RgbColor { R = 240, G = 240, B = 240 } },
        { "other", new RgbColor { R = 128, G = 128, B = 128 } }
    };
    
    /// <summary>
    /// Map an RGB color to the closest standard palette color
    /// </summary>
    /// <param name="rgb">RGB color to map</param>
    /// <returns>Closest standard color name and confidence</returns>
    public static (string colorName, double confidence) MapToStandardColor(RgbColor rgb)
    {
        var minDistance = double.MaxValue;
        var closestColor = "other";
        
        foreach (var (colorName, standardRgb) in StandardColors)
        {
            var distance = CalculateColorDistance(rgb, standardRgb);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = colorName;
            }
        }
        
        // Calculate confidence based on distance (closer = higher confidence)
        // Max distance in RGB space is ~441 (sqrt(255^2 + 255^2 + 255^2))
        var confidence = Math.Max(0.0, 1.0 - (minDistance / 441.0));
        
        return (closestColor, confidence);
    }
    
    /// <summary>
    /// Calculate Euclidean distance between two RGB colors
    /// </summary>
    /// <param name="color1">First color</param>
    /// <param name="color2">Second color</param>
    /// <returns>Distance value</returns>
    private static double CalculateColorDistance(RgbColor color1, RgbColor color2)
    {
        var deltaR = color1.R - color2.R;
        var deltaG = color1.G - color2.G;
        var deltaB = color1.B - color2.B;
        
        return Math.Sqrt(deltaR * deltaR + deltaG * deltaG + deltaB * deltaB);
    }
}