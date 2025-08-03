using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SwipeMyRoof.AvaloniaUI.Views;

/// <summary>
/// View for managing tile providers
/// </summary>
public partial class TileProviderView : UserControl
{
    /// <summary>
    /// Constructor
    /// </summary>
    public TileProviderView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}