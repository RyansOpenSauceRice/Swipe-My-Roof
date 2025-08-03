namespace SwipeMyRoof.Core.Models;

/// <summary>
/// Represents the confidence level of an AI suggestion
/// </summary>
public enum ConfidenceLevel
{
    /// <summary>
    /// Very low confidence (0.0-0.2)
    /// </summary>
    VeryLow,
    
    /// <summary>
    /// Low confidence (0.2-0.4)
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium confidence (0.4-0.6)
    /// </summary>
    Medium,
    
    /// <summary>
    /// High confidence (0.6-0.8)
    /// </summary>
    High,
    
    /// <summary>
    /// Very high confidence (0.8-1.0)
    /// </summary>
    VeryHigh
}

/// <summary>
/// Extension methods for confidence levels
/// </summary>
public static class ConfidenceLevelExtensions
{
    /// <summary>
    /// Convert a numeric confidence value to a confidence level
    /// </summary>
    /// <param name="confidence">Confidence value (0.0-1.0)</param>
    /// <returns>Confidence level</returns>
    public static ConfidenceLevel ToConfidenceLevel(this double confidence)
    {
        return confidence switch
        {
            < 0.2 => ConfidenceLevel.VeryLow,
            < 0.4 => ConfidenceLevel.Low,
            < 0.6 => ConfidenceLevel.Medium,
            < 0.8 => ConfidenceLevel.High,
            _ => ConfidenceLevel.VeryHigh
        };
    }
    
    /// <summary>
    /// Get a description of the confidence level
    /// </summary>
    /// <param name="level">Confidence level</param>
    /// <returns>Description</returns>
    public static string GetDescription(this ConfidenceLevel level)
    {
        return level switch
        {
            ConfidenceLevel.VeryLow => "Very uncertain, please verify carefully",
            ConfidenceLevel.Low => "Uncertain, please verify",
            ConfidenceLevel.Medium => "Moderately confident",
            ConfidenceLevel.High => "Confident",
            ConfidenceLevel.VeryHigh => "Very confident",
            _ => "Unknown confidence level"
        };
    }
    
    /// <summary>
    /// Get a color for the confidence level
    /// </summary>
    /// <param name="level">Confidence level</param>
    /// <returns>Color in hex format (#RRGGBB)</returns>
    public static string GetColor(this ConfidenceLevel level)
    {
        return level switch
        {
            ConfidenceLevel.VeryLow => "#FF0000", // Red
            ConfidenceLevel.Low => "#FF8800", // Orange
            ConfidenceLevel.Medium => "#FFFF00", // Yellow
            ConfidenceLevel.High => "#88FF00", // Light green
            ConfidenceLevel.VeryHigh => "#00FF00", // Green
            _ => "#CCCCCC" // Gray
        };
    }
    
    /// <summary>
    /// Get an icon name for the confidence level
    /// </summary>
    /// <param name="level">Confidence level</param>
    /// <returns>Icon name</returns>
    public static string GetIconName(this ConfidenceLevel level)
    {
        return level switch
        {
            ConfidenceLevel.VeryLow => "error",
            ConfidenceLevel.Low => "warning",
            ConfidenceLevel.Medium => "info",
            ConfidenceLevel.High => "check_circle",
            ConfidenceLevel.VeryHigh => "verified",
            _ => "help"
        };
    }
}