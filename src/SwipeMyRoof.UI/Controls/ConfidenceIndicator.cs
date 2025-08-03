using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.UI.Controls;

/// <summary>
/// A control that displays the confidence level of an AI suggestion
/// </summary>
public class ConfidenceIndicator
{
    /// <summary>
    /// The confidence level to display
    /// </summary>
    public ConfidenceLevel ConfidenceLevel { get; set; }
    
    /// <summary>
    /// The confidence percentage (0-100)
    /// </summary>
    public double ConfidencePercentage => ConfidenceLevel.ToConfidenceValue() * 100;
    
    /// <summary>
    /// The description of the confidence level
    /// </summary>
    public string Description => ConfidenceLevel.GetDescription();
    
    /// <summary>
    /// Whether to show the description text
    /// </summary>
    public bool ShowDescription { get; set; } = true;
    
    /// <summary>
    /// Whether to show the icon
    /// </summary>
    public bool ShowIcon { get; set; } = true;
    
    /// <summary>
    /// Whether to show the color
    /// </summary>
    public bool ShowColor { get; set; } = true;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="confidenceLevel">Confidence level</param>
    public ConfidenceIndicator(ConfidenceLevel confidenceLevel)
    {
        ConfidenceLevel = confidenceLevel;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="confidence">Confidence value (0.0-1.0)</param>
    public ConfidenceIndicator(double confidence)
    {
        ConfidenceLevel = confidence.ToConfidenceLevel();
    }
    
    /// <summary>
    /// Get the description text
    /// </summary>
    /// <returns>Description text</returns>
    public string GetDescription()
    {
        return ConfidenceLevel.GetDescription();
    }
    
    /// <summary>
    /// Get the color
    /// </summary>
    /// <returns>Color in hex format (#RRGGBB)</returns>
    public string GetColor()
    {
        return ConfidenceLevel.GetColor();
    }
    
    /// <summary>
    /// Get the icon name
    /// </summary>
    /// <returns>Icon name</returns>
    public string GetIconName()
    {
        return ConfidenceLevel.GetIconName();
    }
    
    /// <summary>
    /// Get a tooltip text for the confidence level
    /// </summary>
    /// <returns>Tooltip text</returns>
    public string GetTooltip()
    {
        return $"AI confidence: {ConfidenceLevel.GetDescription()} ({(int)(ConfidenceLevel.ToConfidenceValue() * 100)}%)";
    }
}

/// <summary>
/// Extension methods for confidence levels
/// </summary>
public static class ConfidenceLevelValueExtensions
{
    /// <summary>
    /// Convert a confidence level to a numeric value
    /// </summary>
    /// <param name="level">Confidence level</param>
    /// <returns>Confidence value (0.0-1.0)</returns>
    public static double ToConfidenceValue(this ConfidenceLevel level)
    {
        return level switch
        {
            ConfidenceLevel.VeryLow => 0.1,
            ConfidenceLevel.Low => 0.3,
            ConfidenceLevel.Medium => 0.5,
            ConfidenceLevel.High => 0.7,
            ConfidenceLevel.VeryHigh => 0.9,
            _ => 0.0
        };
    }
}