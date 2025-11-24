using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.DesktopUI.Views;
using Serilog;
using System;

namespace RuneAndRust.DesktopUI;

/// <summary>
/// Main application class for Rune & Rust Desktop UI.
/// Configures dependency injection and initializes the application lifecycle.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Gets the application service provider.
    /// </summary>
    public IServiceProvider? Services { get; private set; }

    /// <summary>
    /// Initializes the application and loads XAML.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Called when the framework initialization is completed.
    /// Sets up dependency injection and creates the main window.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/desktopui-.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();

        Log.Information("Rune & Rust Desktop UI v0.43.1 starting...");

        try
        {
            // Build service container
            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            Log.Information("Service container configured successfully");

            // Create and show main window
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel
                };

                Log.Information("Main window created");
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during application initialization");
            throw;
        }
    }

    /// <summary>
    /// Configures all services for dependency injection.
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddSingleton<ILogger>(Log.Logger);

        // UI Services (new in v0.43.1)
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IDialogService, DialogService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MenuViewModel>();

        // Note: Engine services (CombatEngine, DungeonGenerator, etc.) will be registered
        // in later specs when they are actually used by the UI.
        // For v0.43.1, we're establishing the foundation only.

        Log.Debug("Registered {ServiceCount} service types", services.Count);
    }
}
