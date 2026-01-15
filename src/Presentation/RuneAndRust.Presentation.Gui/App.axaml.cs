using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RuneAndRust.Presentation.Gui.ViewModels;
using RuneAndRust.Presentation.Gui.Views;
using Serilog;

namespace RuneAndRust.Presentation.Gui;

/// <summary>
/// Main application class for the Avalonia GUI.
/// </summary>
public partial class App : Avalonia.Application
{
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public static IServiceProvider? Services { get; private set; }

    /// <summary>
    /// Initializes the application and loads XAML resources.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Called when the framework initialization is completed.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        ConfigureLogging();
        ConfigureServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Log.Information("Starting Rune and Rust GUI v0.7.0");
            desktop.MainWindow = new MainMenuWindow
            {
                DataContext = new MainMenuViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Register logging
        services.AddLogging(builder => builder.AddSerilog());
        
        // Register view models
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<SettingsViewModel>();
        
        Services = services.BuildServiceProvider();
    }
}
