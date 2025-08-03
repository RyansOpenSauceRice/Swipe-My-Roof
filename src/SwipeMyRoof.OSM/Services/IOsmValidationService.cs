using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.OSM.Services;

/// <summary>
/// Service for validating OSM data changes before upload
/// </summary>
public interface IOsmValidationService
{
    /// <summary>
    /// Validate that a building change only modifies the roof:colour tag
    /// </summary>
    /// <param name="building">Building to validate</param>
    /// <param name="newRoofColor">New roof color value</param>
    /// <returns>Validation result</returns>
    Task<OsmValidationResult> ValidateRoofColorChangeAsync(OsmBuilding building, string newRoofColor);
    
    /// <summary>
    /// Create an OSM changeset for roof color updates
    /// </summary>
    /// <param name="validatedBuildings">Buildings with validated roof colors</param>
    /// <param name="changesetComment">Comment for the changeset</param>
    /// <returns>OSM changeset XML</returns>
    Task<string> CreateRoofColorChangesetAsync(List<ValidatedBuilding> validatedBuildings, string changesetComment);
    
    /// <summary>
    /// Validate a HEX color value for OSM roof:colour tag
    /// </summary>
    /// <param name="hexColor">HEX color to validate</param>
    /// <returns>Validation result</returns>
    OsmColorValidationResult ValidateRoofColorValue(string hexColor);
    
    /// <summary>
    /// Get the current roof:colour value from OSM building data
    /// </summary>
    /// <param name="building">OSM building</param>
    /// <returns>Current roof color or null if not set</returns>
    string? GetCurrentRoofColor(OsmBuilding building);
    
    /// <summary>
    /// Check if a building already has the specified roof color
    /// </summary>
    /// <param name="building">OSM building</param>
    /// <param name="hexColor">HEX color to check</param>
    /// <returns>True if building already has this color</returns>
    bool BuildingHasRoofColor(OsmBuilding building, string hexColor);
}

/// <summary>
/// Result of OSM data validation
/// </summary>
public class OsmValidationResult
{
    /// <summary>
    /// Whether the validation passed
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Validation error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Validation warnings (non-blocking)
    /// </summary>
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>
    /// The validated building data
    /// </summary>
    public OsmBuilding? ValidatedBuilding { get; set; }
    
    /// <summary>
    /// Previous roof color value (if any)
    /// </summary>
    public string? PreviousRoofColor { get; set; }
    
    /// <summary>
    /// Whether this is a new roof color (vs updating existing)
    /// </summary>
    public bool IsNewRoofColor { get; set; }
}

/// <summary>
/// Result of roof color value validation
/// </summary>
public class OsmColorValidationResult
{
    /// <summary>
    /// Whether the color value is valid
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Normalized color value (e.g., lowercase, with #)
    /// </summary>
    public string? NormalizedColor { get; set; }
    
    /// <summary>
    /// Validation error message
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Suggested alternative color (if applicable)
    /// </summary>
    public string? SuggestedColor { get; set; }
}