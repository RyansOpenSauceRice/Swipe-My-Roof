using Avalonia.Controls;
using Avalonia.Input;
using SwipeMyRoof.AvaloniaUI.ViewModels;

namespace SwipeMyRoof.AvaloniaUI.Controls;

/// <summary>
/// Location search control for finding cities and setting search radius
/// </summary>
public partial class LocationSearchControl : UserControl
{
    public LocationSearchControl()
    {
        InitializeComponent();
    }
    
    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LocationSearchViewModel viewModel)
        {
            // Trigger search when Enter is pressed
            if (viewModel.SearchLocationCommand.CanExecute(null))
            {
                viewModel.SearchLocationCommand.Execute(null);
            }
        }
    }
}