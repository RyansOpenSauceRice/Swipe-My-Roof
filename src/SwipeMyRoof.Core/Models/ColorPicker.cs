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
    /// HEX color value (e.g., "#FF5733")
    /// </summary>
    public string HexColor => Rgb.ToHex();
    
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
    
    /// <summary>
    /// Optional color name/description provided by user
    /// </summary>
    public string? ColorName { get; set; }
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
/// Color utility methods
/// </summary>
public static class ColorUtils
{
    /// <summary>
    /// Get a human-readable description of a color based on its RGB values
    /// </summary>
    /// <param name="rgb">RGB color</param>
    /// <returns>Color description</returns>
    public static string GetColorDescription(RgbColor rgb)
    {
        // Calculate brightness
        var brightness = (rgb.R * 0.299 + rgb.G * 0.587 + rgb.B * 0.114) / 255.0;
        
        // Determine dominant color channel
        var max = Math.Max(rgb.R, Math.Max(rgb.G, rgb.B));
        var min = Math.Min(rgb.R, Math.Min(rgb.G, rgb.B));
        var diff = max - min;
        
        // If very low saturation, it's a shade of gray
        if (diff < 30)
        {
            return brightness switch
            {
                < 0.2 => "Very Dark",
                < 0.4 => "Dark",
                < 0.6 => "Medium",
                < 0.8 => "Light",
                _ => "Very Light"
            };
        }
        
        // Determine hue-based description
        string hueDescription;
        if (rgb.R == max)
        {
            if (rgb.G > rgb.B)
                hueDescription = rgb.G > rgb.R * 0.7 ? "Orange-Red" : "Red";
            else
                hueDescription = rgb.B > rgb.R * 0.7 ? "Purple-Red" : "Red";
        }
        else if (rgb.G == max)
        {
            if (rgb.R > rgb.B)
                hueDescription = rgb.R > rgb.G * 0.7 ? "Yellow-Green" : "Green";
            else
                hueDescription = rgb.B > rgb.G * 0.7 ? "Blue-Green" : "Green";
        }
        else // rgb.B == max
        {
            if (rgb.R > rgb.G)
                hueDescription = rgb.R > rgb.B * 0.7 ? "Purple-Blue" : "Blue";
            else
                hueDescription = rgb.G > rgb.B * 0.7 ? "Cyan-Blue" : "Blue";
        }
        
        // Add brightness modifier
        var brightnessModifier = brightness switch
        {
            < 0.3 => "Dark ",
            > 0.7 => "Light ",
            _ => ""
        };
        
        return brightnessModifier + hueDescription;
    }
    
    /// <summary>
    /// Validate if a hex color string is valid
    /// </summary>
    /// <param name="hex">Hex color string</param>
    /// <returns>True if valid</returns>
    public static bool IsValidHexColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return false;
        
        hex = hex.TrimStart('#');
        return hex.Length == 6 && hex.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }
}