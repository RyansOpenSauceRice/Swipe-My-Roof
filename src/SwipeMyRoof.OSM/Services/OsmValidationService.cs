using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Implementation of OSM validation service
/// </summary>
public class OsmValidationService : IOsmValidationService
{
    private static readonly Regex HexColorRegex = new(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);
    
    /// <inheritdoc />
    public async Task<OsmValidationResult> ValidateRoofColorChangeAsync(OsmBuilding building, string newRoofColor)
    {
        var result = new OsmValidationResult
        {
            ValidatedBuilding = building
        };
        
        // Validate the new color value
        var colorValidation = ValidateRoofColorValue(newRoofColor);
        if (!colorValidation.IsValid)
        {
            result.IsValid = false;
            result.Errors.Add($"Invalid roof color value: {colorValidation.ErrorMessage}");
            return result;
        }
        
        // Get current roof color
        var currentRoofColor = GetCurrentRoofColor(building);
        result.PreviousRoofColor = currentRoofColor;
        result.IsNewRoofColor = string.IsNullOrEmpty(currentRoofColor);
        
        // Check if building already has this color
        if (BuildingHasRoofColor(building, colorValidation.NormalizedColor!))
        {
            result.IsValid = false;
            result.Errors.Add($"Building already has roof color {colorValidation.NormalizedColor}");
            return result;
        }
        
        // Validate building data
        if (building.OsmId <= 0)
        {
            result.IsValid = false;
            result.Errors.Add("Invalid OSM ID");
            return result;
        }
        
        if (string.IsNullOrEmpty(building.OsmType) || (building.OsmType != "way" && building.OsmType != "relation"))
        {
            result.IsValid = false;
            result.Errors.Add("OSM element must be a way or relation");
            return result;
        }
        
        // Check if this is actually a building
        if (building.Tags == null || !building.Tags.ContainsKey("building"))
        {
            result.Warnings.Add("Element does not have a 'building' tag - ensure this is actually a building");
        }
        
        // Warn about overwriting existing roof color
        if (!result.IsNewRoofColor)
        {
            result.Warnings.Add($"This will overwrite existing roof:colour value '{currentRoofColor}'");
        }
        
        result.IsValid = true;
        return result;
    }
    
    /// <inheritdoc />
    public async Task<string> CreateRoofColorChangesetAsync(List<ValidatedBuilding> validatedBuildings, string changesetComment)
    {
        var sb = new StringBuilder();
        
        // XML header
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<osmChange version=\"0.6\" generator=\"SwipeMyRoof\">");
        sb.AppendLine("  <modify>");
        
        foreach (var building in validatedBuildings)
        {
            // Only include buildings that haven't been uploaded yet
            if (building.UploadedToOsm)
                continue;
            
            sb.AppendLine($"    <{building.OsmType} id=\"{building.OsmId}\" version=\"1\">");
            
            // Add existing tags (if we have them)
            if (!string.IsNullOrEmpty(building.OriginalOsmTags))
            {
                try
                {
                    var tags = JsonSerializer.Deserialize<Dictionary<string, string>>(building.OriginalOsmTags);
                    if (tags != null)
                    {
                        foreach (var tag in tags)
                        {
                            // Skip the roof:colour tag - we'll add the new one
                            if (tag.Key == "roof:colour")
                                continue;
                            
                            sb.AppendLine($"      <tag k=\"{EscapeXml(tag.Key)}\" v=\"{EscapeXml(tag.Value)}\"/>");
                        }
                    }
                }
                catch (JsonException)
                {
                    // If we can't parse the original tags, just add the building tag
                    sb.AppendLine("      <tag k=\"building\" v=\"yes\"/>");
                }
            }
            else
            {
                // Minimal building tag if we don't have original data
                sb.AppendLine("      <tag k=\"building\" v=\"yes\"/>");
            }
            
            // Add the new roof:colour tag
            sb.AppendLine($"      <tag k=\"roof:colour\" v=\"{EscapeXml(building.RoofColorHex)}\"/>");
            
            sb.AppendLine($"    </{building.OsmType}>");
        }
        
        sb.AppendLine("  </modify>");
        sb.AppendLine("</osmChange>");
        
        return sb.ToString();
    }
    
    /// <inheritdoc />
    public OsmColorValidationResult ValidateRoofColorValue(string hexColor)
    {
        var result = new OsmColorValidationResult();
        
        if (string.IsNullOrWhiteSpace(hexColor))
        {
            result.IsValid = false;
            result.ErrorMessage = "Color value cannot be empty";
            return result;
        }
        
        // Normalize the color (add # if missing, convert to lowercase)
        var normalizedColor = hexColor.Trim();
        if (!normalizedColor.StartsWith("#"))
        {
            normalizedColor = "#" + normalizedColor;
        }
        normalizedColor = normalizedColor.ToLowerInvariant();
        
        // Validate HEX format
        if (!HexColorRegex.IsMatch(normalizedColor))
        {
            result.IsValid = false;
            result.ErrorMessage = "Color must be in HEX format (#RRGGBB)";
            
            // Try to suggest a correction
            if (normalizedColor.Length == 4 && normalizedColor.StartsWith("#"))
            {
                // Convert #RGB to #RRGGBB
                var shortHex = normalizedColor.Substring(1);
                var expandedHex = "#" + string.Join("", shortHex.Select(c => $"{c}{c}"));
                result.SuggestedColor = expandedHex;
            }
            
            return result;
        }
        
        result.IsValid = true;
        result.NormalizedColor = normalizedColor;
        return result;
    }
    
    /// <inheritdoc />
    public string? GetCurrentRoofColor(OsmBuilding building)
    {
        return building.Tags?.GetValueOrDefault("roof:colour");
    }
    
    /// <inheritdoc />
    public bool BuildingHasRoofColor(OsmBuilding building, string hexColor)
    {
        var currentColor = GetCurrentRoofColor(building);
        if (string.IsNullOrEmpty(currentColor))
            return false;
        
        // Normalize both colors for comparison
        var normalizedCurrent = ValidateRoofColorValue(currentColor).NormalizedColor;
        var normalizedNew = ValidateRoofColorValue(hexColor).NormalizedColor;
        
        return string.Equals(normalizedCurrent, normalizedNew, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Escape XML special characters
    /// </summary>
    /// <param name="text">Text to escape</param>
    /// <returns>Escaped text</returns>
    private static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}