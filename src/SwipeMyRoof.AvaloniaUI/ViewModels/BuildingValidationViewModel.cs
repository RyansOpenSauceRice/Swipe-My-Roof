using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.UI.Controls;

namespace SwipeMyRoof.AvaloniaUI.ViewModels;

/// <summary>
/// View model for the building validation screen
/// </summary>
public partial class BuildingValidationViewModel : ViewModelBase
{
    /// <summary>
    /// Current building being validated
    /// </summary>
    [ObservableProperty]
    private BuildingCandidate _currentBuilding;
    
    /// <summary>
    /// Confidence indicator for the current building
    /// </summary>
    [ObservableProperty]
    private ConfidenceIndicator _confidenceIndicator;
    
    /// <summary>
    /// Whether to show the confidence indicator
    /// </summary>
    [ObservableProperty]
    private bool _showConfidenceIndicator = true;
    
    /// <summary>
    /// Whether the application is in practice mode
    /// </summary>
    [ObservableProperty]
    private bool _isPracticeMode = false;
    
    /// <summary>
    /// Whether there is a warning message to display
    /// </summary>
    [ObservableProperty]
    private bool _hasWarning = false;
    
    /// <summary>
    /// Warning message to display
    /// </summary>
    [ObservableProperty]
    private string _warningMessage = string.Empty;
    
    /// <summary>
    /// Proposed color value
    /// </summary>
    [ObservableProperty]
    private string _proposedColorValue = string.Empty;
    
    /// <summary>
    /// Proposed color explanation
    /// </summary>
    [ObservableProperty]
    private string _proposedColorExplanation = string.Empty;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public BuildingValidationViewModel()
    {
        // Initialize with a sample building
        CurrentBuilding = new BuildingCandidate
        {
            OsmId = 123456,
            ProposedColor = new ProposedColor
            {
                Value = "Red",
                Confidence = 0.85,
                Explanation = "The building is clearly visible in the image with good lighting conditions."
            }
        };
        
        // Initialize confidence indicator
        ConfidenceIndicator = new ConfidenceIndicator(CurrentBuilding.ProposedColor?.Confidence ?? 0.5);
        
        // Set sample values
        ProposedColorValue = "Red";
        ProposedColorExplanation = "The building appears to have a red roof based on the satellite imagery.";
    }
    
    /// <summary>
    /// Get the practice mode banner text
    /// </summary>
    public string GetPracticeModeBanner => "PRACTICE MODE - Your submissions will not be uploaded to OpenStreetMap";
    
    /// <summary>
    /// Get the warning message
    /// </summary>
    public string GetWarningMessage => WarningMessage;
    
    /// <summary>
    /// Accept the current building
    /// </summary>
    [RelayCommand]
    private async Task AcceptBuilding()
    {
        // TODO: Implement building acceptance logic
        
        // For now, just show a message
        Console.WriteLine($"Building {CurrentBuilding.OsmId} accepted");
        
        // Load the next building
        await LoadNextBuilding();
    }
    
    /// <summary>
    /// Reject the current building
    /// </summary>
    [RelayCommand]
    private async Task RejectBuilding()
    {
        // TODO: Implement building rejection logic
        
        // For now, just show a message
        Console.WriteLine($"Building {CurrentBuilding.OsmId} rejected");
        
        // Load the next building
        await LoadNextBuilding();
    }
    
    /// <summary>
    /// Skip the current building
    /// </summary>
    [RelayCommand]
    private async Task SkipBuilding()
    {
        // TODO: Implement building skip logic
        
        // For now, just show a message
        Console.WriteLine($"Building {CurrentBuilding.OsmId} skipped");
        
        // Load the next building
        await LoadNextBuilding();
    }
    
    /// <summary>
    /// Load the next building
    /// </summary>
    private async Task LoadNextBuilding()
    {
        // TODO: Implement loading the next building from a queue
        
        // For now, just create a new sample building
        var random = new Random();
        CurrentBuilding = new BuildingCandidate
        {
            OsmId = random.Next(100000, 999999),
            ProposedColor = new ProposedColor
            {
                Value = GetRandomColor(),
                Confidence = random.NextDouble(),
                Explanation = GetRandomConfidenceExplanation()
            }
        };
        
        // Update confidence indicator
        ConfidenceIndicator = new ConfidenceIndicator(CurrentBuilding.ProposedColor?.Confidence ?? 0.5);
        
        // Set sample values
        ProposedColorValue = GetRandomColor();
        ProposedColorExplanation = $"The building appears to have a {ProposedColorValue.ToLower()} roof based on the satellite imagery.";
        
        // Randomly show a warning
        HasWarning = random.Next(0, 5) == 0;
        if (HasWarning)
        {
            WarningMessage = "This building may be difficult to classify due to shadows or poor image quality.";
        }
    }
    
    /// <summary>
    /// Get a random color
    /// </summary>
    private string GetRandomColor()
    {
        var colors = new[] { "Red", "Blue", "Green", "Brown", "Black", "Gray", "White" };
        return colors[new Random().Next(0, colors.Length)];
    }
    
    /// <summary>
    /// Get a random confidence explanation
    /// </summary>
    private string GetRandomConfidenceExplanation()
    {
        var explanations = new[]
        {
            "The building is clearly visible in the image with good lighting conditions.",
            "The building is partially obscured by trees, making classification challenging.",
            "The image quality is good, but there are some shadows that may affect accuracy.",
            "The building is small in the image, which may reduce confidence.",
            "The image has excellent clarity and the building is clearly visible."
        };
        
        return explanations[new Random().Next(0, explanations.Length)];
    }
}