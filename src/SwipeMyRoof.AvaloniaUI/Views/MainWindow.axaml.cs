using System;
using Avalonia;
using Avalonia.Controls;
using SwipeMyRoof.AvaloniaUI.ViewModels;

namespace SwipeMyRoof.AvaloniaUI.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Handle window size changes
        PropertyChanged += MainWindow_PropertyChanged;
    }
    
    private void MainWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
        {
            // Update the view model with the new window width
            if (ViewModel?.BuildingValidationViewModel != null)
            {
                ViewModel.BuildingValidationViewModel.HandleWindowSizeChanged(Bounds.Width);
            }
        }
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        // Set initial window size state
        if (ViewModel?.BuildingValidationViewModel != null)
        {
            ViewModel.BuildingValidationViewModel.HandleWindowSizeChanged(Bounds.Width);
        }
    }
}