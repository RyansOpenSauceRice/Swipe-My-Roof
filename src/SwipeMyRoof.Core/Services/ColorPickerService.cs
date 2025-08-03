using SwipeMyRoof.Core.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace SwipeMyRoof.Core.Services;

/// <summary>
/// Service for picking colors from images
/// </summary>
public class ColorPickerService : IColorPickerService
{
    /// <inheritdoc />
    public async Task<PickedColor?> PickColorAsync(byte[] imageData, int x, int y)
    {
        try
        {
            using var stream = new MemoryStream(imageData);
            using var bitmap = new Bitmap(stream);
            
            // Ensure coordinates are within bounds
            if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height)
                return null;
            
            var pixel = bitmap.GetPixel(x, y);
            var rgb = new RgbColor
            {
                R = pixel.R,
                G = pixel.G,
                B = pixel.B,
                A = pixel.A
            };
            
            return new PickedColor
            {
                Rgb = rgb,
                PixelX = x,
                PixelY = y,
                ColorName = ColorUtils.GetColorDescription(rgb)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error picking color: {ex.Message}");
            return null;
        }
    }
    
    /// <inheritdoc />
    public async Task<PickedColor?> PickColorWithSamplingAsync(byte[] imageData, int x, int y, int sampleRadius = 3)
    {
        try
        {
            using var stream = new MemoryStream(imageData);
            using var bitmap = new Bitmap(stream);
            
            // Ensure center coordinates are within bounds
            if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height)
                return null;
            
            var samples = new List<Color>();
            
            // Sample pixels in a square around the center point
            for (int dx = -sampleRadius; dx <= sampleRadius; dx++)
            {
                for (int dy = -sampleRadius; dy <= sampleRadius; dy++)
                {
                    int sampleX = x + dx;
                    int sampleY = y + dy;
                    
                    // Skip pixels outside image bounds
                    if (sampleX < 0 || sampleX >= bitmap.Width || sampleY < 0 || sampleY >= bitmap.Height)
                        continue;
                    
                    samples.Add(bitmap.GetPixel(sampleX, sampleY));
                }
            }
            
            if (samples.Count == 0)
                return null;
            
            // Calculate average color
            var avgR = (byte)samples.Average(c => c.R);
            var avgG = (byte)samples.Average(c => c.G);
            var avgB = (byte)samples.Average(c => c.B);
            var avgA = (byte)samples.Average(c => c.A);
            
            var rgb = new RgbColor
            {
                R = avgR,
                G = avgG,
                B = avgB,
                A = avgA
            };
            
            return new PickedColor
            {
                Rgb = rgb,
                PixelX = x,
                PixelY = y,
                ColorName = ColorUtils.GetColorDescription(rgb)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error picking color with sampling: {ex.Message}");
            return null;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<RgbColor>> GetDominantColorsAsync(byte[] imageData, int maxColors = 5)
    {
        try
        {
            using var stream = new MemoryStream(imageData);
            using var bitmap = new Bitmap(stream);
            
            var colorCounts = new Dictionary<int, int>();
            
            // Sample every 4th pixel to improve performance
            for (int x = 0; x < bitmap.Width; x += 4)
            {
                for (int y = 0; y < bitmap.Height; y += 4)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    
                    // Quantize color to reduce noise (round to nearest 16)
                    var quantizedR = (pixel.R / 16) * 16;
                    var quantizedG = (pixel.G / 16) * 16;
                    var quantizedB = (pixel.B / 16) * 16;
                    
                    var colorKey = (quantizedR << 16) | (quantizedG << 8) | quantizedB;
                    
                    colorCounts[colorKey] = colorCounts.GetValueOrDefault(colorKey, 0) + 1;
                }
            }
            
            // Get the most frequent colors
            var dominantColors = colorCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(maxColors)
                .Select(kvp => new RgbColor
                {
                    R = (byte)((kvp.Key >> 16) & 0xFF),
                    G = (byte)((kvp.Key >> 8) & 0xFF),
                    B = (byte)(kvp.Key & 0xFF)
                })
                .ToList();
            
            return dominantColors;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting dominant colors: {ex.Message}");
            return new List<RgbColor>();
        }
    }
    
    /// <inheritdoc />
    public string GetColorDescription(RgbColor rgb)
    {
        return ColorUtils.GetColorDescription(rgb);
    }
}