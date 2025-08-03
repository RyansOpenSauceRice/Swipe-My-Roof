using SwipeMyRoof.Core.Models;
using SwipeMyRoof.UI.Controls;

namespace SwipeMyRoof.UI.ViewModels;

/// <summary>
/// View model for the building validation screen
/// </summary>
public class BuildingValidationViewModel
{
    /// <summary>
    /// The current building candidate
    /// </summary>
    public BuildingCandidate? CurrentBuilding { get; private set; }
    
    /// <summary>
    /// Whether the screen is in practice mode
    /// </summary>
    public bool IsPracticeMode { get; set; }
    
    /// <summary>
    /// The confidence indicator for the current building
    /// </summary>
    public ConfidenceIndicator? ConfidenceIndicator { get; private set; }
    
    /// <summary>
    /// Whether the confidence indicator should be shown
    /// </summary>
    public bool ShowConfidenceIndicator { get; set; } = true;
    
    /// <summary>
    /// Whether the current building has a proposed color
    /// </summary>
    public bool HasProposedColor => CurrentBuilding?.ProposedColor != null;
    
    /// <summary>
    /// The proposed color value
    /// </summary>
    public string ProposedColorValue => CurrentBuilding?.ProposedColor?.Value ?? string.Empty;
    
    /// <summary>
    /// The proposed color explanation
    /// </summary>
    public string ProposedColorExplanation => CurrentBuilding?.ProposedColor?.Explanation ?? string.Empty;
    
    /// <summary>
    /// Whether the current building is a decoy
    /// </summary>
    public bool IsDecoy => CurrentBuilding?.ProposedColor?.IsDecoy ?? false;
    
    /// <summary>
    /// Set the current building
    /// </summary>
    /// <param name="building">Building candidate</param>
    public void SetCurrentBuilding(BuildingCandidate building)
    {
        CurrentBuilding = building;
        
        if (building.ProposedColor != null)
        {
            ConfidenceIndicator = new ConfidenceIndicator(building.ProposedColor.Confidence);
        }
        else
        {
            ConfidenceIndicator = null;
        }
    }
    
    /// <summary>
    /// Accept the current building
    /// </summary>
    public void AcceptBuilding()
    {
        if (CurrentBuilding == null)
        {
            return;
        }
        
        CurrentBuilding.UserFeedback = UserFeedback.Accepted;
    }
    
    /// <summary>
    /// Reject the current building
    /// </summary>
    public void RejectBuilding()
    {
        if (CurrentBuilding == null)
        {
            return;
        }
        
        CurrentBuilding.UserFeedback = UserFeedback.Rejected;
    }
    
    /// <summary>
    /// Skip the current building
    /// </summary>
    public void SkipBuilding()
    {
        if (CurrentBuilding == null)
        {
            return;
        }
        
        CurrentBuilding.UserFeedback = UserFeedback.Skipped;
    }
    
    /// <summary>
    /// Get a warning message based on confidence level
    /// </summary>
    /// <returns>Warning message or null if no warning</returns>
    public string? GetWarningMessage()
    {
        if (CurrentBuilding?.ProposedColor == null || ConfidenceIndicator == null)
        {
            return null;
        }
        
        return ConfidenceIndicator.ConfidenceLevel switch
        {
            ConfidenceLevel.VeryLow => "Very low confidence! Please verify carefully.",
            ConfidenceLevel.Low => "Low confidence. Please verify.",
            _ => null
        };
    }
    
    /// <summary>
    /// Get the practice mode banner text
    /// </summary>
    /// <returns>Banner text</returns>
    public string GetPracticeModeBanner()
    {
        return IsPracticeMode ? "PRACTICE MODE - No edits will be uploaded to OSM" : string.Empty;
    }
    
    /// <summary>
    /// Get the theme color for the current mode
    /// </summary>
    /// <returns>Theme color in hex format (#RRGGBB)</returns>
    public string GetThemeColor()
    {
        return IsPracticeMode ? "#0077FF" : "#FFFFFF";
    }
}