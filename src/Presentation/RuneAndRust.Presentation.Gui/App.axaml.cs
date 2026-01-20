using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RuneAndRust.Presentation.Gui.Services;
using RuneAndRust.Presentation.Gui.ViewModels;
using RuneAndRust.Presentation.Gui.Views;
using Serilog;

namespace RuneAndRust.Presentation.Gui;

/// <summary>
/// Main application class for the Avalonia GUI.
/// </summary>
/// <remarks>
/// Handles application initialization, dependency injection setup,
/// and logging configuration.
/// </remarks>
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
    /// <remarks>
    /// Configures logging, sets up DI container, and creates the main window.
    /// </remarks>
    public override void OnFrameworkInitializationCompleted()
    {
        ConfigureLogging();
        ConfigureServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Log.Information("Starting Rune and Rust GUI v0.7.0c");
            
            // Resolve MainMenuViewModel from DI container
            var viewModel = Services?.GetService<MainMenuViewModel>();
            desktop.MainWindow = new MainMenuWindow
            {
                DataContext = viewModel ?? new MainMenuViewModel()
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
        
        // Register services (singletons for state management)
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IWindowLayoutService, WindowLayoutService>();
        
        // Register view models
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<GameWindowViewModel>();
        
        Services = services.BuildServiceProvider();
        Log.Debug("Services configured: NavigationService, WindowLayoutService, ViewModels");
    }
}
