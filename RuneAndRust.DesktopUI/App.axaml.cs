using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.DesktopUI.Views;
using RuneAndRust.Engine;
using Serilog;
using System;
using System.IO;
using System.Linq;

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

        Log.Information("Rune & Rust Desktop UI v0.43.20 starting...");

        try
        {
            // Build service container
            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            Log.Information("Service container configured successfully");

            // Load sprites
            LoadSprites(Services);

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

        // UI Services (v0.43.1, v0.43.3)
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IKeyboardShortcutService, KeyboardShortcutService>();

        // Sprite Services (v0.43.2, v0.43.6, v0.43.7)
        services.AddSingleton<ISpriteService, SpriteService>();
        services.AddSingleton<IStatusEffectIconService, StatusEffectIconService>();
        services.AddSingleton<IHazardVisualizationService, HazardVisualizationService>();

        // Animation Services (v0.43.8)
        services.AddSingleton<IAnimationService, AnimationService>();

        // Meta-Progression Services (v0.43.15)
        services.AddSingleton<IMetaProgressionService, MetaProgressionService>();

        // Endgame Services (v0.43.16)
        services.AddSingleton<IEndgameService, EndgameService>();

        // Boss Display Services (v0.43.17)
        services.AddSingleton<IBossDisplayService, BossDisplayService>();

        // Audio Services (v0.43.18)
        services.AddSingleton<IAudioService, AudioService>();

        // Save/Load Services (v0.43.19)
        services.AddSingleton<ISaveGameService, SaveGameService>();

        // Tooltip & Help Services (v0.43.20)
        services.AddSingleton<ITooltipService, TooltipService>();

        // Engine Services (v0.43.5)
        services.AddSingleton<DiceService>();
        services.AddSingleton<SagaService>();
        services.AddSingleton<LootService>();
        services.AddSingleton<EquipmentService>();
        services.AddSingleton<HazardService>();
        services.AddSingleton<CurrencyService>();
        services.AddSingleton<AdvancedStatusEffectService>();
        services.AddSingleton<CombatEngine>();
        services.AddSingleton<EnemyAI>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<SpriteDemoViewModel>();
        services.AddTransient<CombatViewModel>();
        services.AddTransient<CharacterSheetViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<DungeonExplorationViewModel>();
        services.AddTransient<SpecializationTreeViewModel>();
        services.AddTransient<MetaProgressionViewModel>();
        services.AddTransient<EndgameModeViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SaveLoadViewModel>();
        services.AddTransient<HelpViewModel>();

        // Note: Additional engine services will be registered as needed in future specs.

        Log.Debug("Registered {ServiceCount} service types", services.Count);
    }

    /// <summary>
    /// Loads sprites from the Assets/Sprites directory.
    /// </summary>
    private void LoadSprites(IServiceProvider services)
    {
        var spriteService = services.GetRequiredService<ISpriteService>();
        var spritePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Sprites");

        Log.Information("Loading sprites from {Path}", spritePath);
        spriteService.LoadSpritesFromDirectory(spritePath);

        var spriteCount = spriteService.GetAvailableSprites().Count();
        Log.Information("Loaded {Count} sprites", spriteCount);
    }
}
