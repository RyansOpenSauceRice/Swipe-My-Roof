using SwipeMyRoof.Core.Models;
using SwipeMyRoof.UI.Controls;
using SwipeMyRoof.UI.ViewModels;

namespace SwipeMyRoof.ConsoleApp;

/// <summary>
/// Demo for the confidence indicator feature
/// </summary>
public class ConfidenceDemo
{
    /// <summary>
    /// Run the demo
    /// </summary>
    public static void Run()
    {
        Console.WriteLine("=== Confidence Indicator Demo ===");
        Console.WriteLine();
        
        // Create a sample building with different confidence levels
        var buildings = new List<BuildingCandidate>
        {
            CreateSampleBuilding(0.15, "Very uncertain due to shadows"),
            CreateSampleBuilding(0.35, "Low quality image"),
            CreateSampleBuilding(0.55, "Mixed tones"),
            CreateSampleBuilding(0.75, "Clear pattern"),
            CreateSampleBuilding(0.95, "Uniform color")
        };
        
        // Create a view model
        var viewModel = new BuildingValidationViewModel
        {
            IsPracticeMode = true
        };
        
        // Show each building
        foreach (var building in buildings)
        {
            viewModel.SetCurrentBuilding(building);
            
            Console.WriteLine($"Building ID: {building.OsmId}");
            Console.WriteLine($"Proposed Color: {viewModel.ProposedColorValue}");
            Console.WriteLine($"Explanation: {viewModel.ProposedColorExplanation}");
            
            if (viewModel.ConfidenceIndicator != null)
            {
                Console.WriteLine($"Confidence Level: {viewModel.ConfidenceIndicator.ConfidenceLevel}");
                Console.WriteLine($"Description: {viewModel.ConfidenceIndicator.GetDescription()}");
                Console.WriteLine($"Color: {viewModel.ConfidenceIndicator.GetColor()}");
                Console.WriteLine($"Icon: {viewModel.ConfidenceIndicator.GetIconName()}");
                Console.WriteLine($"Tooltip: {viewModel.ConfidenceIndicator.GetTooltip()}");
            }
            
            var warning = viewModel.GetWarningMessage();
            if (!string.IsNullOrEmpty(warning))
            {
                Console.WriteLine($"Warning: {warning}");
            }
            
            Console.WriteLine();
        }
        
        // Show practice mode banner
        Console.WriteLine($"Practice Mode Banner: {viewModel.GetPracticeModeBanner()}");
        Console.WriteLine($"Theme Color: {viewModel.GetThemeColor()}");
        
        // Switch to real mode
        viewModel.IsPracticeMode = false;
        Console.WriteLine($"Real Mode Banner: {viewModel.GetPracticeModeBanner()}");
        Console.WriteLine($"Theme Color: {viewModel.GetThemeColor()}");
    }
    
    /// <summary>
    /// Create a sample building with a given confidence level
    /// </summary>
    /// <param name="confidence">Confidence level (0.0-1.0)</param>
    /// <param name="explanation">Explanation text</param>
    /// <returns>Building candidate</returns>
    private static BuildingCandidate CreateSampleBuilding(double confidence, string explanation)
    {
        return new BuildingCandidate
        {
            OsmId = Random.Shared.Next(100000, 999999),
            Location = new GeoLocation
            {
                Lat = 46.12345,
                Lon = -98.54321
            },
            BoundingBox = new BoundingBox
            {
                MinX = -98.544,
                MinY = 46.122,
                MaxX = -98.542,
                MaxY = 46.124
            },
            ProposedColor = new ProposedColor
            {
                Value = GetColorForConfidence(confidence),
                Confidence = confidence,
                Explanation = explanation,
                IsDecoy = false
            }
        };
    }
    
    /// <summary>
    /// Get a color name based on confidence level
    /// </summary>
    /// <param name="confidence">Confidence level (0.0-1.0)</param>
    /// <returns>Color name</returns>
    private static string GetColorForConfidence(double confidence)
    {
        return confidence switch
        {
            < 0.2 => "other",
            < 0.4 => "light gray",
            < 0.6 => "dark gray",
            < 0.8 => "red",
            _ => "brown"
        };
    }
}