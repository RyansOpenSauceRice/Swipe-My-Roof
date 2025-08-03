using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SwipeMyRoof.AvaloniaUI.Views;

/// <summary>
/// View for OSM authentication
/// </summary>
public partial class OsmAuthView : UserControl
{
    /// <summary>
    /// Constructor
    /// </summary>
    public OsmAuthView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}