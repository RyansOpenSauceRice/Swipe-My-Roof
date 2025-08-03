using System;
using Avalonia;

namespace SwipeMyRoof.AvaloniaUI.Linux;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // For Linux testing, we can enable some additional logging
        Environment.SetEnvironmentVariable("AVALONIA_DEBUG_LOGS", "1");
        
        // Start the application
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => SwipeMyRoof.AvaloniaUI.Program.BuildAvaloniaApp();
}