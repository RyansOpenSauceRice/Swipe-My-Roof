using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SwipeMyRoof.AvaloniaUI.Views;

public partial class BuildingValidationView : UserControl
{
    public BuildingValidationView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}