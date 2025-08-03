using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Core.Services;
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
    /// Whether the view is in narrow (mobile) mode
    /// </summary>
    [ObservableProperty]
    private bool _isNarrowView = false;
    
    /// <summary>
    /// Whether an image is currently loading
    /// </summary>
    [ObservableProperty]
    private bool _isLoadingImage = true;
    
    /// <summary>
    /// The building image
    /// </summary>
    [ObservableProperty]
    private Bitmap? _buildingImage;
    
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
    /// Whether the color picker is visible
    /// </summary>
    [ObservableProperty]
    private bool _showColorPicker = false;
    
    /// <summary>
    /// Color picker view model
    /// </summary>
    [ObservableProperty]
    private ColorPickerViewModel? _colorPickerViewModel;
    
    /// <summary>
    /// Current building image data for color picking
    /// </summary>
    private byte[]? _currentImageData;
    
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
        
        // Initialize color picker
        var colorPickerService = new ColorPickerService();
        ColorPickerViewModel = new ColorPickerViewModel(colorPickerService);
        ColorPickerViewModel.ColorConfirmed += OnColorConfirmed;
        ColorPickerViewModel.ColorPickingCancelled += OnColorPickingCancelled;
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
    /// Reject the current building and open color picker
    /// </summary>
    [RelayCommand]
    private async Task RejectBuilding()
    {
        // Show the color picker for manual color selection
        await ShowColorPicker();
    }
    
    /// <summary>
    /// Show the color picker for manual color selection
    /// </summary>
    [RelayCommand]
    private async Task ShowColorPicker()
    {
        if (BuildingImage != null && _currentImageData != null && ColorPickerViewModel != null)
        {
            ColorPickerViewModel.SetImage(_currentImageData, BuildingImage);
            ShowColorPicker = true;
        }
        else
        {
            Console.WriteLine("Cannot show color picker: image data not available");
        }
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
        
        // Indicate that we're loading
        IsLoadingImage = true;
        
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
            },
            Location = new GeoLocation
            {
                Lat = 40.7128 + (random.NextDouble() * 0.1 - 0.05),
                Lon = -74.0060 + (random.NextDouble() * 0.1 - 0.05)
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
        
        // Load the building image
        await LoadBuildingImage();
    }
    
    /// <summary>
    /// Load the building image
    /// </summary>
    private async Task LoadBuildingImage()
    {
        try
        {
            IsLoadingImage = true;
            
            // Dispose of the previous image if it exists
            BuildingImage?.Dispose();
            BuildingImage = null;
            
            // For demo purposes, load a placeholder image from a URL
            // In a real app, this would load from your satellite imagery provider
            string imageUrl = GetSampleImageUrl();
            
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(imageUrl);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var memoryStream = new MemoryStream();
            
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            // Store the image data for color picking
            _currentImageData = memoryStream.ToArray();
            memoryStream.Position = 0;
            
            // Create a bitmap from the stream on the UI thread
            BuildingImage = new Bitmap(memoryStream);
        }
        catch (Exception ex)
        {
            // In a real app, you would log this error and show a message to the user
            Console.WriteLine($"Error loading image: {ex.Message}");
            
            // In a real app, you would load a fallback image
            // For now, just leave the image null
            BuildingImage = null;
        }
        finally
        {
            IsLoadingImage = false;
        }
    }
    
    /// <summary>
    /// Get a sample image URL for demonstration purposes
    /// </summary>
    private string GetSampleImageUrl()
    {
        // For demo purposes, use a random placeholder image
        // In a real app, this would be the URL to your satellite imagery
        var random = new Random();
        int width = 600;
        int height = 400;
        int imageId = random.Next(1, 1000);
        
        return $"https://picsum.photos/seed/{imageId}/{width}/{height}";
    }
    
    /// <summary>
    /// Handle window size changes to adjust the layout
    /// </summary>
    /// <param name="width">Window width</param>
    public void HandleWindowSizeChanged(double width)
    {
        // Consider the view narrow if it's less than 800 pixels wide
        IsNarrowView = width < 800;
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
    
    /// <summary>
    /// Handle color confirmed from color picker
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event args</param>
    private void OnColorConfirmed(object? sender, ColorConfirmedEventArgs e)
    {
        // Update the building with the manually selected color
        if (CurrentBuilding.ProposedColor != null)
        {
            CurrentBuilding.ProposedColor.Value = e.StandardColor;
            CurrentBuilding.ProposedColor.Source = "manual";
            CurrentBuilding.ProposedColor.Confidence = e.PickedColor.MappingConfidence;
            CurrentBuilding.ProposedColor.Explanation = $"Manually selected from satellite imagery at pixel ({e.PickedColor.PixelX}, {e.PickedColor.PixelY})";
        }
        
        // Update UI
        ProposedColorValue = e.StandardColor;
        ProposedColorExplanation = $"Manually selected: {e.StandardColor} (RGB: {e.PickedColor.Rgb.ToHex()})";
        
        // Update confidence indicator
        ConfidenceIndicator = new ConfidenceIndicator(e.PickedColor.MappingConfidence);
        
        // Hide color picker
        ShowColorPicker = false;
        
        Console.WriteLine($"Color manually selected: {e.StandardColor} for building {CurrentBuilding.OsmId}");
    }
    
    /// <summary>
    /// Handle color picking cancelled
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event args</param>
    private void OnColorPickingCancelled(object? sender, EventArgs e)
    {
        // Hide color picker and load next building
        ShowColorPicker = false;
        
        // Load the next building since user rejected and didn't pick a color
        _ = LoadNextBuilding();
        
        Console.WriteLine($"Color picking cancelled for building {CurrentBuilding.OsmId}");
    }
}