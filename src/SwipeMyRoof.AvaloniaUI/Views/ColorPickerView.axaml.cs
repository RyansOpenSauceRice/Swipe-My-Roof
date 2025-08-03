using Avalonia.Controls;
using Avalonia.Input;
using SwipeMyRoof.AvaloniaUI.ViewModels;

namespace SwipeMyRoof.AvaloniaUI.Views;

/// <summary>
/// Color picker view with eyedropper functionality
/// </summary>
public partial class ColorPickerView : UserControl
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ColorPickerView()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Handle pointer press on image for color picking
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event args</param>
    private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not ColorPickerViewModel viewModel || !viewModel.IsPickingColor)
            return;
        
        if (sender is not Control control)
            return;
        
        var position = e.GetPosition(control);
        
        // Convert position to image coordinates
        // Note: This is a simplified version - in a real implementation,
        // you'd need to account for image scaling and positioning within the control
        var x = (int)position.X;
        var y = (int)position.Y;
        
        // Execute the pick color command
        viewModel.PickColorCommand.Execute((x, y));
        
        e.Handled = true;
    }
}