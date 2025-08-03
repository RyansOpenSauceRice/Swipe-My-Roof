using System.Text.Json.Serialization;

namespace SwipeMyRoof.Core.Models;

/// <summary>
/// Represents a proposed roof material from LLM or other source
/// </summary>
public class ProposedRoofMaterial
{
    /// <summary>
    /// The material value (e.g., "metal", "tile", "shingle", etc.)
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Source of the material suggestion (ai, heuristic, etc.)
    /// </summary>
    public string Source { get; set; } = "ai";
    
    /// <summary>
    /// Confidence level (0.0-1.0)
    /// </summary>
    public double Confidence { get; set; }
    
    /// <summary>
    /// Timestamp of when the material was proposed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Whether this is a decoy material (for training/reliability)
    /// </summary>
    public bool IsDecoy { get; set; }
    
    /// <summary>
    /// Brief explanation of the material choice
    /// </summary>
    public string? Explanation { get; set; }
}

/// <summary>
/// Standard roof material types recognized by OSM
/// </summary>
public static class RoofMaterialTypes
{
    /// <summary>
    /// Standard palette of roof materials
    /// </summary>
    public static readonly string[] StandardPalette = new[]
    {
        "metal",
        "concrete",
        "tiles",
        "slate",
        "shingle",
        "thatch",
        "asbestos",
        "glass",
        "grass",
        "tar_paper",
        "copper",
        "stone",
        "other"
    };
    
    /// <summary>
    /// Check if a material is in the standard palette
    /// </summary>
    /// <param name="material">Material to check</param>
    /// <returns>True if in standard palette</returns>
    public static bool IsStandardMaterial(string material)
    {
        return Array.IndexOf(StandardPalette, material.ToLowerInvariant()) >= 0;
    }
}

/// <summary>
/// Extension to BuildingCandidate to include roof material
/// </summary>
public static class BuildingCandidateExtensions
{
    /// <summary>
    /// Get the existing roof material from tags
    /// </summary>
    /// <param name="candidate">Building candidate</param>
    /// <returns>Roof material or null if not present</returns>
    public static string? GetExistingRoofMaterial(this BuildingCandidate candidate)
    {
        // In a real implementation, this would extract from OSM tags
        // For now, we'll return null to indicate no existing material
        return null;
    }
    
    /// <summary>
    /// Set the proposed roof material
    /// </summary>
    /// <param name="candidate">Building candidate</param>
    /// <param name="material">Proposed material</param>
    public static void SetProposedRoofMaterial(this BuildingCandidate candidate, ProposedRoofMaterial material)
    {
        // In a real implementation, this would be stored in the candidate
        // For now, we'll just store it in a property we'll add to BuildingCandidate
    }
}