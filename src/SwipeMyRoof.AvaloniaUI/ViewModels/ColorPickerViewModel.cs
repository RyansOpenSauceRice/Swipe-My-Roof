using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Core.Services;
using Avalonia.Media.Imaging;
using Avalonia.Media;

namespace SwipeMyRoof.AvaloniaUI.ViewModels;

/// <summary>
/// View model for the color picker/eyedropper tool
/// </summary>
public class ColorPickerViewModel : ViewModelBase
{
    private readonly IColorPickerService _colorPickerService;
    private byte[]? _imageData;
    private Bitmap? _image;
    private PickedColor? _selectedColor;
    private bool _isPickingColor;
    private string _instructions = "Click on the roof to select its color";
    private bool _showColorPreview;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="colorPickerService">Color picker service</param>
    public ColorPickerViewModel(IColorPickerService colorPickerService)
    {
        _colorPickerService = colorPickerService;
        
        // Commands
        PickColorCommand = ReactiveCommand.CreateFromTask<(int x, int y)>(PickColorAsync);
        ConfirmColorCommand = ReactiveCommand.Create(ConfirmColor, this.WhenAnyValue(x => x.SelectedColor).Select(c => c != null));
        CancelCommand = ReactiveCommand.Create(Cancel);
        StartPickingCommand = ReactiveCommand.Create(StartPicking);
    }
    
    /// <summary>
    /// Image to pick colors from
    /// </summary>
    public Bitmap? Image
    {
        get => _image;
        set => this.RaiseAndSetIfChanged(ref _image, value);
    }
    
    /// <summary>
    /// Currently selected color
    /// </summary>
    public PickedColor? SelectedColor
    {
        get => _selectedColor;
        set => this.RaiseAndSetIfChanged(ref _selectedColor, value);
    }
    
    /// <summary>
    /// Whether the user is currently picking a color
    /// </summary>
    public bool IsPickingColor
    {
        get => _isPickingColor;
        set => this.RaiseAndSetIfChanged(ref _isPickingColor, value);
    }
    
    /// <summary>
    /// Instructions for the user
    /// </summary>
    public string Instructions
    {
        get => _instructions;
        set => this.RaiseAndSetIfChanged(ref _instructions, value);
    }
    
    /// <summary>
    /// Whether to show the color preview
    /// </summary>
    public bool ShowColorPreview
    {
        get => _showColorPreview;
        set => this.RaiseAndSetIfChanged(ref _showColorPreview, value);
    }
    
    /// <summary>
    /// Selected color as Avalonia brush for preview
    /// </summary>
    public IBrush? SelectedColorBrush => SelectedColor != null 
        ? new SolidColorBrush(Color.FromRgb(SelectedColor.Rgb.R, SelectedColor.Rgb.G, SelectedColor.Rgb.B))
        : null;
    
    /// <summary>
    /// Selected color hex string
    /// </summary>
    public string SelectedColorHex => SelectedColor?.Rgb.ToHex() ?? "#000000";
    
    /// <summary>
    /// Selected standard color name
    /// </summary>
    public string SelectedStandardColor => SelectedColor?.StandardColor ?? "none";
    
    /// <summary>
    /// Mapping confidence percentage
    /// </summary>
    public string MappingConfidenceText => SelectedColor != null 
        ? $"{SelectedColor.MappingConfidence:P0} confidence"
        : "";
    
    /// <summary>
    /// Command to pick a color at coordinates
    /// </summary>
    public ReactiveCommand<(int x, int y), Unit> PickColorCommand { get; }
    
    /// <summary>
    /// Command to confirm the selected color
    /// </summary>
    public ReactiveCommand<Unit, Unit> ConfirmColorCommand { get; }
    
    /// <summary>
    /// Command to cancel color picking
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    
    /// <summary>
    /// Command to start picking mode
    /// </summary>
    public ReactiveCommand<Unit, Unit> StartPickingCommand { get; }
    
    /// <summary>
    /// Event fired when a color is confirmed
    /// </summary>
    public event EventHandler<ColorConfirmedEventArgs>? ColorConfirmed;
    
    /// <summary>
    /// Event fired when color picking is cancelled
    /// </summary>
    public event EventHandler? ColorPickingCancelled;
    
    /// <summary>
    /// Set the image for color picking
    /// </summary>
    /// <param name="imageData">Image data</param>
    /// <param name="bitmap">Bitmap for display</param>
    public void SetImage(byte[] imageData, Bitmap bitmap)
    {
        _imageData = imageData;
        Image = bitmap;
        SelectedColor = null;
        ShowColorPreview = false;
    }
    
    private async Task PickColorAsync((int x, int y) coordinates)
    {
        if (_imageData == null || !IsPickingColor)
            return;
        
        try
        {
            Instructions = "Analyzing color...";
            
            // Use sampling for more accurate color picking
            var pickedColor = await _colorPickerService.PickColorWithSamplingAsync(_imageData, coordinates.x, coordinates.y, 2);
            
            if (pickedColor != null)
            {
                SelectedColor = pickedColor;
                ShowColorPreview = true;
                Instructions = $"Selected: {pickedColor.StandardColor} ({pickedColor.Rgb.ToHex()})";
                
                // Raise property changed for computed properties
                this.RaisePropertyChanged(nameof(SelectedColorBrush));
                this.RaisePropertyChanged(nameof(SelectedColorHex));
                this.RaisePropertyChanged(nameof(SelectedStandardColor));
                this.RaisePropertyChanged(nameof(MappingConfidenceText));
            }
            else
            {
                Instructions = "Could not pick color at that location. Try again.";
            }
        }
        catch (Exception ex)
        {
            Instructions = $"Error picking color: {ex.Message}";
        }
    }
    
    private void ConfirmColor()
    {
        if (SelectedColor == null)
            return;
        
        var args = new ColorConfirmedEventArgs
        {
            PickedColor = SelectedColor,
            StandardColor = SelectedColor.StandardColor
        };
        
        ColorConfirmed?.Invoke(this, args);
        
        // Reset state
        IsPickingColor = false;
        ShowColorPreview = false;
        Instructions = "Color confirmed!";
    }
    
    private void Cancel()
    {
        SelectedColor = null;
        IsPickingColor = false;
        ShowColorPreview = false;
        Instructions = "Color picking cancelled";
        
        ColorPickingCancelled?.Invoke(this, EventArgs.Empty);
    }
    
    private void StartPicking()
    {
        IsPickingColor = true;
        SelectedColor = null;
        ShowColorPreview = false;
        Instructions = "Click on the roof to select its color";
    }
}

/// <summary>
/// Event arguments for color confirmed event
/// </summary>
public class ColorConfirmedEventArgs : EventArgs
{
    public PickedColor PickedColor { get; set; } = null!;
    public string StandardColor { get; set; } = string.Empty;
}