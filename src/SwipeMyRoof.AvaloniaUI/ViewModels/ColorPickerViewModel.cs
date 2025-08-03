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
    private string _manualHexInput = string.Empty;
    
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
        ApplyManualColorCommand = ReactiveCommand.Create(ApplyManualColor, this.WhenAnyValue(x => x.ManualHexInput).Select(hex => ColorUtils.IsValidHexColor(hex)));
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
    public string SelectedColorHex => SelectedColor?.HexColor ?? "#000000";
    
    /// <summary>
    /// Selected color description
    /// </summary>
    public string SelectedColorDescription => SelectedColor?.ColorName ?? "none";
    
    /// <summary>
    /// RGB values text
    /// </summary>
    public string RgbValuesText => SelectedColor != null 
        ? $"R:{SelectedColor.Rgb.R} G:{SelectedColor.Rgb.G} B:{SelectedColor.Rgb.B}"
        : "";
    
    /// <summary>
    /// Manual HEX color input
    /// </summary>
    public string ManualHexInput
    {
        get => _manualHexInput;
        set => this.RaiseAndSetIfChanged(ref _manualHexInput, value);
    }
    
    /// <summary>
    /// Command to pick a color at coordinates
    /// </summary>
    public ReactiveCommand<(int x, int y), Unit> PickColorCommand { get; }
    
    /// <summary>
    /// Command to confirm the selected color
    /// </summary>
    public ReactiveCommand<Unit, Unit> ConfirmColorCommand { get; }
    
    /// <summary>
    /// Command to apply manual HEX color
    /// </summary>
    public ReactiveCommand<Unit, Unit> ApplyManualColorCommand { get; }
    
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
                Instructions = $"Selected: {pickedColor.ColorName} ({pickedColor.HexColor})";
                
                // Raise property changed for computed properties
                this.RaisePropertyChanged(nameof(SelectedColorBrush));
                this.RaisePropertyChanged(nameof(SelectedColorHex));
                this.RaisePropertyChanged(nameof(SelectedColorDescription));
                this.RaisePropertyChanged(nameof(RgbValuesText));
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
            HexColor = SelectedColor.HexColor,
            ColorDescription = SelectedColor.ColorName ?? "Unknown"
        };
        
        ColorConfirmed?.Invoke(this, args);
        
        // Reset state
        IsPickingColor = false;
        ShowColorPreview = false;
        Instructions = "Color confirmed!";
    }
    
    /// <summary>
    /// Apply manually entered HEX color
    /// </summary>
    private void ApplyManualColor()
    {
        if (!ColorUtils.IsValidHexColor(ManualHexInput))
            return;
        
        // Parse HEX to RGB
        var hex = ManualHexInput.TrimStart('#');
        var r = Convert.ToByte(hex.Substring(0, 2), 16);
        var g = Convert.ToByte(hex.Substring(2, 2), 16);
        var b = Convert.ToByte(hex.Substring(4, 2), 16);
        
        var rgb = new RgbColor { R = r, G = g, B = b, A = 255 };
        var pickedColor = new PickedColor
        {
            Rgb = rgb,
            PixelX = -1, // Manual entry
            PixelY = -1, // Manual entry
            ColorName = ColorUtils.GetColorDescription(rgb)
        };
        
        SelectedColor = pickedColor;
        ShowColorPreview = true;
        Instructions = $"Manual entry: {pickedColor.ColorName} ({pickedColor.HexColor})";
        
        // Raise property changed for computed properties
        this.RaisePropertyChanged(nameof(SelectedColorBrush));
        this.RaisePropertyChanged(nameof(SelectedColorHex));
        this.RaisePropertyChanged(nameof(SelectedColorDescription));
        this.RaisePropertyChanged(nameof(RgbValuesText));
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
    public string HexColor { get; set; } = string.Empty;
    public string ColorDescription { get; set; } = string.Empty;
}