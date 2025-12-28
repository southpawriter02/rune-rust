using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Constants;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Settings;
using RuneAndRust.Engine.Algorithms;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Engine.Performance;
using RuneAndRust.Engine.Repositories;
using RuneAndRust.Engine.Services;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using RuneAndRust.Terminal.Controllers;
using RuneAndRust.Terminal.Rendering;
using RuneAndRust.Terminal.Services;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using Character = RuneAndRust.Core.Entities.Character;

class Program
{
    static void Main(string[] args)
    {
        // 1. Configure Serilog (v0.3.24b: Environment-aware configuration)
#if DEBUG
        // Debug: Verbose logging to console and file
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
            .WriteTo.File("logs/runeandrust.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Debug)
            .CreateLogger();
#else
        // Release: Minimal console output, info+ to file
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Warning)
            .WriteTo.File("logs/runeandrust.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();
#endif

        Log.Information("[Startup] Rune & Rust {Version} ({Config}) starting...",
            BuildInfo.FullVersionString, BuildInfo.Configuration);

        // Declare host outside try block for crash handler access (v0.3.16b)
        IHost? host = null;

        try
        {
            // Database connection string - use environment variable in production
            // Default uses port 5433 for Docker container (see docker-compose.yml)
            var connectionString = Environment.GetEnvironmentVariable("RUNEANDRUST_CONNECTION_STRING")
                ?? "Host=localhost;Port=5433;Database=RuneAndRust;Username=postgres;Password=password";

            // 2. Build Host
            host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register Database Context
                    services.AddDbContext<RuneAndRustDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    // Register Repositories
                    services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
                    services.AddScoped<ISaveGameRepository, SaveGameRepository>();
                    services.AddScoped<IRoomRepository, RoomRepository>();
                    services.AddScoped<ICharacterRepository, CharacterRepository>();
                    services.AddScoped<IInteractableObjectRepository, InteractableObjectRepository>();
                    services.AddScoped<ICodexEntryRepository, CodexEntryRepository>();
                    services.AddScoped<IDataCaptureRepository, DataCaptureRepository>();
                    services.AddScoped<IActiveAbilityRepository, ActiveAbilityRepository>();
                    services.AddScoped<IItemRepository, ItemRepository>();

                    // Register Dynamic Room Engine Repositories (v0.4.0)
                    services.AddScoped<IRoomTemplateRepository, RoomTemplateRepository>();
                    services.AddScoped<IBiomeDefinitionRepository, BiomeDefinitionRepository>();
                    services.AddScoped<IBiomeElementRepository, BiomeElementRepository>();

                    // Register Capture Template Repository (v0.3.25b: JSON-based templates)
                    services.AddSingleton<ICaptureTemplateRepository>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<JsonCaptureTemplateRepository>>();
                        var basePath = Path.Combine(AppContext.BaseDirectory, "data", "capture-templates", "categories");
                        return new JsonCaptureTemplateRepository(logger, basePath);
                    });

                    // Register Core State (Singleton to persist across game loop)
                    services.AddSingleton<GameState>();

                    // Register Input/Output Handler
                    services.AddSingleton<IInputHandler, TerminalInputHandler>();

                    // Register Engine Services
                    services.AddSingleton<CommandParser>();
                    services.AddSingleton<IGameService, GameService>();
                    services.AddSingleton<IDiceService, DiceService>();
                    services.AddSingleton<IStatCalculationService, StatCalculationService>();
                    services.AddScoped<ISagaService, SagaService>();
                    services.AddScoped<IProgressionService, ProgressionService>();
                    services.AddScoped<SaveManager>();

                    // Register Spatial Services
                    services.AddScoped<DungeonGenerator>();
                    services.AddScoped<INavigationService, NavigationService>();

                    // Register Interaction Services
                    services.AddScoped<IDescriptorEngine, DescriptorEngine>();
                    services.AddScoped<IInteractionService, InteractionService>();
                    services.AddScoped<IDataCaptureService, DataCaptureService>();
                    services.AddScoped<ObjectSpawner>();

                    // Register Library Service (v0.3.11a - Dynamic Knowledge Engine)
                    services.AddSingleton<ILibraryService, LibraryService>();

                    // Register DocGen Service (v0.3.11b - Developer's Handbook)
                    services.AddScoped<IDocGenService, DocGenService>();

                    // Register Loot Audit Service (v0.3.13a - The Loot Audit)
                    services.AddScoped<ILootAuditService, LootAuditService>();

                    // Register Combat Audit Service (v0.3.13b - The Combat Simulator)
                    services.AddScoped<ICombatAuditService, CombatAuditService>();

                    // Register Journal Services
                    services.AddScoped<IJournalService, JournalService>();

                    // Register Combat Services
                    services.AddSingleton<IInitiativeService, InitiativeService>();
                    services.AddSingleton<IStatusEffectService, StatusEffectService>();
                    services.AddSingleton<ITraumaService, TraumaService>();
                    services.AddSingleton<IAttackResolutionService, AttackResolutionService>();
                    services.AddSingleton<IEnemyAIService, EnemyAIService>();
                    services.AddSingleton<ICreatureTraitService, CreatureTraitService>();
                    services.AddSingleton<IResourceService, ResourceService>();
                    services.AddSingleton<IAbilityService, AbilityService>();
                    services.AddScoped<ILootService, LootService>();
                    services.AddScoped<ICombatService, CombatService>();
                    services.AddSingleton<ICombatScreenRenderer, CombatScreenRenderer>();
                    services.AddSingleton<IVictoryScreenRenderer, VictoryScreenRenderer>();

                    // Register Character Creation Services
                    services.AddScoped<CharacterFactory>();
                    services.AddScoped<CharacterCreationController>();

                    // Register Wizard Services (v0.3.4b)
                    services.AddScoped<IWizardService, WizardService>();
                    services.AddScoped<CreationWizard>();

                    // Register Narrative Services (v0.3.4c)
                    services.AddScoped<INarrativeService, NarrativeService>();
                    services.AddScoped<ITypewriterRenderer, TypewriterRenderer>();

                    // Register Exploration HUD (v0.3.5a)
                    services.AddSingleton<IExplorationScreenRenderer, ExplorationScreenRenderer>();

                    // Register Saga UI (v0.4.0c - The Shrine)
                    services.AddSingleton<ISagaScreenRenderer, SagaScreenRenderer>();
                    services.AddScoped<SagaController>();

                    // Register Enemy Factory
                    services.AddScoped<IEnemyFactory, EnemyFactory>();

                    // Register Inventory Services
                    services.AddScoped<IInventoryRepository, InventoryRepository>();
                    services.AddScoped<IInventoryService, InventoryService>();

                    // Register Inventory Screen Renderer (v0.3.7a)
                    services.AddSingleton<IInventoryScreenRenderer, InventoryScreenRenderer>();

                    // Register Ambush Services (v0.3.2b)
                    services.AddScoped<IAmbushService, AmbushService>();

                    // Register Rest Services (v0.3.2a)
                    services.AddScoped<IRestService, RestService>();

                    // Register Rest Screen Renderer (v0.3.2c)
                    services.AddSingleton<IRestScreenRenderer, RestScreenRenderer>();

                    // Register Crafting Services (v0.3.1a)
                    services.AddScoped<ICraftingService, CraftingService>();

                    // Register Crafting Screen Renderer (v0.3.7b)
                    services.AddSingleton<ICraftingScreenRenderer, CraftingScreenRenderer>();

                    // Register Journal Screen Renderer (v0.3.7c)
                    services.AddSingleton<IJournalScreenRenderer, JournalScreenRenderer>();

                    // Register Visual Effect Service (v0.3.9a)
                    services.AddSingleton<IVisualEffectService, VisualEffectService>();

                    // Register Theme Service (v0.3.9b)
                    services.AddSingleton<IThemeService, ThemeService>();

                    // Register Screen Transition Service (v0.3.14b)
                    services.AddSingleton<IScreenTransitionService, ScreenTransitionService>();

                    // Register Localization Service (v0.3.15a - The Lexicon)
                    services.AddSingleton<ILocalizationService, LocalizationService>();

                    // Register Main Menu Controller (v0.3.15a)
                    services.AddSingleton<MainMenuController>();

                    // Register Options View Helper Service (v0.3.15a)
                    services.AddSingleton<OptionsViewHelperService>();

                    // Register Input Configuration Service (v0.3.9c)
                    services.AddSingleton<IInputConfigurationService, InputConfigurationService>();

                    // Register Input Service (v0.3.23a - The Gatekeeper)
                    services.AddSingleton<IInputService, InputService>();

                    // Register Terminal Service (v0.3.23c - The Mouse)
                    services.AddSingleton<ITerminalService, TerminalService>();

                    // Register Hit Test Service (v0.3.23c - The Mouse)
                    services.AddSingleton<IHitTestService, HitTestService>();

                    // Register Context Help Service (v0.3.9c)
                    services.AddSingleton<IContextHelpService, ContextHelpService>();

                    // Register Settings Service (v0.3.10a)
                    services.AddSingleton<ISettingsService, SettingsService>();

                    // Register Options Screen (v0.3.10b)
                    services.AddSingleton<IOptionsScreenRenderer, OptionsScreenRenderer>();
                    services.AddScoped<OptionsController>();

                    // Register Bodging Services (v0.3.1b)
                    services.AddScoped<IBodgingService, BodgingService>();

                    // Register Hazard Services (v0.3.3a)
                    services.AddSingleton<EffectScriptExecutor>();
                    services.AddScoped<IHazardService, HazardService>();

                    // Register Condition Services (v0.3.3b)
                    services.AddScoped<IConditionService, ConditionService>();

                    // Register Environment Ecosystem Services (v0.3.3c)
                    services.AddScoped<IEnvironmentPopulator, EnvironmentPopulator>();

                    // Register Dynamic Room Engine Services (v0.4.0)
                    services.AddScoped<ITemplateLoaderService, TemplateLoaderService>();
                    services.AddScoped<ITemplateRendererService, TemplateRendererService>();
                    services.AddScoped<IElementSpawnEvaluator, ElementSpawnEvaluator>();

                    // Register Title Screen Service (v0.3.4a)
                    services.AddScoped<ITitleScreenService, TitleScreenService>();

#if DEBUG
                    // Register Debug Console Services (v0.3.17a)
                    // v0.3.24a: Wrapped in #if DEBUG to exclude from Release builds
                    services.AddSingleton<IDebugConsoleService, DebugConsoleService>();
                    services.AddScoped<IDebugConsoleRenderer, DebugConsoleRenderer>();

                    // Register Cheat Service (v0.3.17b)
                    // v0.3.24a: Wrapped in #if DEBUG to exclude from Release builds
                    services.AddScoped<ICheatService, CheatService>();
#endif

                    // Register Performance Pools (v0.3.18a - The Garbage Collector)
                    services.AddSingleton<ITextBufferPool, TextBufferPool>();

                    // Register Pathfinding Infrastructure (v0.3.18b - The Hot Path)
                    services.AddSingleton<ISpatialHashGrid, SpatialHashGrid>();
                    services.AddSingleton<IPathfindingService, AStarPathfinder>();

                    // Register Audio Infrastructure (v0.3.19a - The Instrument)
                    services.AddSingleton<IAudioProvider, ConsoleAudioProvider>();
                    services.AddSingleton<IAudioService, AudioService>();

                    // Register Event Bus and Audio Listener (v0.3.19b - The Score)
                    services.AddSingleton<IEventBus, EventBus>();
                    services.AddSingleton<AudioEventListener>();

                    // Register Ambience Service (v0.3.19c - The Soundscape)
                    services.AddSingleton<IAmbienceService, AmbienceService>();

                    // Register Map Export Service (v0.3.20b - The Atlas)
                    services.AddSingleton<IMapExportService, MapExportService>();

                    // Register Fast Travel Services (v0.3.20c - The Pathfinder)
                    services.AddScoped<IRoomPathfinderService, RoomPathfinderService>();
                    services.AddScoped<IAutoTravelService, AutoTravelService>();
                })
                .UseSerilog() // Wire Serilog into ILogger
                .Build();

            // 3. Load user settings (v0.3.10a)
            var settingsService = host.Services.GetRequiredService<ISettingsService>();
            settingsService.LoadAsync().GetAwaiter().GetResult();

            // 3b. Load locale (v0.3.15b - The Translator: uses GameSettings.Language)
            var locService = host.Services.GetRequiredService<ILocalizationService>();
            locService.LoadLocaleAsync(GameSettings.Language).GetAwaiter().GetResult();

            // 3c. Initialize audio event listener (v0.3.19b - The Score)
            var audioListener = host.Services.GetRequiredService<AudioEventListener>();
            audioListener.SubscribeAll();

            // 3d. Initialize terminal service for mouse mode (v0.3.23c)
            var terminalService = host.Services.GetRequiredService<ITerminalService>();
            if (terminalService.IsMouseSupported())
            {
                terminalService.EnableMouseMode();
                Log.Information("[Startup] Mouse mode enabled (SGR Extended)");
            }
            else
            {
                Log.Debug("[Startup] Mouse mode not supported in this terminal");
            }

            // 4. Seed data
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RuneAndRustDbContext>();
                var templateLoader = scope.ServiceProvider.GetRequiredService<ITemplateLoaderService>();
                var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();

                AbilitySeeder.SeedAsync(context).GetAwaiter().GetResult();
                ConditionSeeder.SeedAsync(context).GetAwaiter().GetResult();
                HazardTemplateSeeder.SeedAsync(context).GetAwaiter().GetResult();

                // Seed Room Templates and Biome Definitions (v0.4.0)
                RoomTemplateSeeder.SeedAsync(context, templateLoader, logger).GetAwaiter().GetResult();
            }

            // 5. CLI Argument Handling (v0.3.11b - DocGen)
            if (args.Contains("--docgen"))
            {
                AnsiConsole.MarkupLine("[yellow]Generating documentation from [GameDocument] attributes...[/]");

                using (var scope = host.Services.CreateScope())
                {
                    var docGen = scope.ServiceProvider.GetRequiredService<IDocGenService>();
                    docGen.GenerateDocsAsync("docs/generated").GetAwaiter().GetResult();
                }

                AnsiConsole.MarkupLine("[green]Documentation generation complete. See docs/generated/[/]");
                return;
            }

            // 5b. CLI Argument Handling (v0.3.13a - Loot Audit)
            if (args.Any(a => a.StartsWith("--audit-loot")))
            {
                // Parse CLI arguments with defaults
                var iterations = ParseIntArg(args, "iterations", 10000);
                var biome = ParseEnumArg(args, "biome", BiomeType.Ruin);
                var danger = ParseEnumArg(args, "danger", DangerLevel.Safe);
                var witsBonus = ParseIntArg(args, "wits", 0);

                AnsiConsole.MarkupLine($"[yellow]Running Loot Audit: {iterations:N0} iterations in {biome} ({danger})...[/]");

                using (var scope = host.Services.CreateScope())
                {
                    var auditService = scope.ServiceProvider.GetRequiredService<ILootAuditService>();
                    var config = new LootAuditConfiguration(iterations, biome, danger, witsBonus);
                    var report = auditService.RunAuditAsync(config).GetAwaiter().GetResult();

                    // Ensure output directory exists
                    var outputDir = "docs/audits";
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    // Write report
                    var outputPath = Path.Combine(outputDir, $"loot_audit_{DateTime.Now:yyyyMMdd_HHmmss}.md");
                    File.WriteAllText(outputPath, report.MarkdownReport);

                    // Summary output
                    var criticals = report.Flags.Count(f => f.Severity == VarianceSeverity.Critical);
                    var warnings = report.Flags.Count(f => f.Severity == VarianceSeverity.Warning);

                    if (criticals > 0)
                        AnsiConsole.MarkupLine($"[red]Audit complete with {criticals} critical variance(s).[/]");
                    else if (warnings > 0)
                        AnsiConsole.MarkupLine($"[yellow]Audit complete with {warnings} warning(s).[/]");
                    else
                        AnsiConsole.MarkupLine("[green]Audit complete. All distributions within acceptable bounds.[/]");

                    AnsiConsole.MarkupLine($"[grey]Report: {outputPath}[/]");
                }

                return;
            }

            // 5c. CLI Argument Handling (v0.3.13b - Combat Audit)
            if (args.Any(a => a.StartsWith("--audit-combat")))
            {
                // Parse CLI arguments with defaults
                var iterations = ParseIntArg(args, "iterations", 1000);
                var archetype = ParseEnumArg(args, "archetype", ArchetypeType.Warrior);
                var enemy = ParseStringArg(args, "enemy", "und_draugr_01");
                var level = ParseIntArg(args, "level", 1);

                AnsiConsole.MarkupLine($"[yellow]Running Combat Audit: {iterations:N0} matches, {archetype} vs {enemy} (Level {level})...[/]");

                using (var scope = host.Services.CreateScope())
                {
                    var auditService = scope.ServiceProvider.GetRequiredService<ICombatAuditService>();
                    var config = new CombatAuditConfiguration(iterations, archetype, enemy, level);
                    var report = auditService.RunAuditAsync(config).GetAwaiter().GetResult();

                    // Ensure output directory exists
                    var outputDir = "docs/audits";
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    // Write report
                    var outputPath = Path.Combine(outputDir, $"combat_audit_{DateTime.Now:yyyyMMdd_HHmmss}.md");
                    File.WriteAllText(outputPath, report.MarkdownReport);

                    // Summary output
                    var criticals = report.Flags.Count(f => f.Severity == VarianceSeverity.Critical);
                    var warnings = report.Flags.Count(f => f.Severity == VarianceSeverity.Warning);

                    AnsiConsole.MarkupLine($"[green]Win Rate: {report.Statistics.WinRate:F1}%[/]");
                    AnsiConsole.MarkupLine($"[green]Avg Rounds: {report.Statistics.AvgRoundsPerEncounter:F1}[/]");

                    if (criticals > 0)
                        AnsiConsole.MarkupLine($"[red]Audit complete with {criticals} critical deviation(s).[/]");
                    else if (warnings > 0)
                        AnsiConsole.MarkupLine($"[yellow]Audit complete with {warnings} warning(s).[/]");
                    else
                        AnsiConsole.MarkupLine("[green]Audit complete. All metrics within acceptable bounds.[/]");

                    AnsiConsole.MarkupLine($"[grey]Report: {outputPath}[/]");
                }

                return;
            }

            // 6. UI Handover (v0.3.24b: Uses BuildInfo for version)
            AnsiConsole.MarkupLine($"[green]Rune & Rust {BuildInfo.FullVersionString} Booting...[/]");
            AnsiConsole.WriteLine();

            // Show title screen and get menu selection (v0.3.4a)
            TitleScreenResult menuResult;
            using (var scope = host.Services.CreateScope())
            {
                var titleScreen = scope.ServiceProvider.GetRequiredService<ITitleScreenService>();
                menuResult = titleScreen.ShowAsync().GetAwaiter().GetResult();
            }

            // Route based on menu selection
            switch (menuResult.SelectedOption)
            {
                case MainMenuOption.Quit:
                    AnsiConsole.MarkupLine("[grey]Farewell, Traveler...[/]");
                    return;

                case MainMenuOption.NewGame:
                    // Run character creation wizard (v0.3.4b)
                    Character? character = null;
                    using (var scope = host.Services.CreateScope())
                    {
                        var wizard = scope.ServiceProvider.GetRequiredService<CreationWizard>();
                        character = wizard.RunAsync().GetAwaiter().GetResult();
                    }

                    if (character != null)
                    {
                        // CRITICAL FIX (v0.3.4c): Bridge character to GameState
                        var gameState = host.Services.GetRequiredService<GameState>();
                        gameState.CurrentCharacter = character;
                        gameState.Phase = GamePhase.Exploration;
                        Log.Debug("[Program] Character {Name} set to GameState", character.Name);

                        // Generate starting dungeon (v0.3.24a: Uses template-based generation)
                        using (var scope = host.Services.CreateScope())
                        {
                            var dungeonGen = scope.ServiceProvider.GetRequiredService<DungeonGenerator>();
                            var startRoomId = dungeonGen.GenerateDungeonAsync("the_roots").GetAwaiter().GetResult();
                            gameState.CurrentRoomId = startRoomId;
                            gameState.VisitedRoomIds.Add(startRoomId);  // Mark starting room as visited (v0.3.5b)
                            Log.Information("[Program] World initialized: Starting room {RoomId}", startRoomId);
                        }

                        // Play prologue (v0.3.4c)
                        using (var scope = host.Services.CreateScope())
                        {
                            var narrative = scope.ServiceProvider.GetRequiredService<INarrativeService>();
                            var typewriter = scope.ServiceProvider.GetRequiredService<ITypewriterRenderer>();
                            var prologueText = narrative.GetPrologueText(character);
                            typewriter.PlaySequenceAsync(prologueText).GetAwaiter().GetResult();
                        }
                    }
                    else
                    {
                        // Character creation was cancelled, exit
                        AnsiConsole.MarkupLine("[grey]Character creation cancelled.[/]");
                        return;
                    }
                    break;

                case MainMenuOption.LoadGame:
                    if (menuResult.SaveSlotNumber.HasValue)
                    {
                        using (var scope = host.Services.CreateScope())
                        {
                            var saveManager = scope.ServiceProvider.GetRequiredService<SaveManager>();
                            var gameState = scope.ServiceProvider.GetRequiredService<GameState>();
                            var loadedState = saveManager.LoadGameAsync(menuResult.SaveSlotNumber.Value).GetAwaiter().GetResult();
                            if (loadedState != null)
                            {
                                // Copy loaded state to current game state
                                gameState.CurrentCharacter = loadedState.CurrentCharacter;
                                gameState.CurrentRoomId = loadedState.CurrentRoomId;
                                gameState.Phase = loadedState.Phase;
                                gameState.TurnCount = loadedState.TurnCount;
                                gameState.VisitedRoomIds = loadedState.VisitedRoomIds;  // Restore fog of war (v0.3.5b)
                            }
                        }
                    }
                    break;
            }

            // v0.3.23b: Cancellation token for graceful shutdown
            using var cts = new CancellationTokenSource();

            // Handle Ctrl+C gracefully
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent immediate termination
                cts.Cancel();

                // v0.3.23c: Disable mouse mode before exit
                terminalService.DisableMouseMode();

                Console.WriteLine("\n[System] Shutting down gracefully...");
            };

            // Handle SIGTERM (for containerized environments)
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                cts.Cancel();

                // v0.3.23c: Disable mouse mode before exit
                terminalService.DisableMouseMode();
            };

            // Start the game loop with cancellation support
            var game = host.Services.GetRequiredService<IGameService>();
            game.StartAsync(cts.Token).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            // v0.3.23b: Clean exit from cancellation
            Console.WriteLine("Goodbye!");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // v0.3.16a: The Safety Net - Global Exception Handler
            // v0.3.23c: Attempt to disable mouse mode before crash screen
            try
            {
                if (host != null)
                {
                    var terminalSvc = host.Services.GetService<ITerminalService>();
                    terminalSvc?.DisableMouseMode();
                }
            }
            catch
            {
                // Swallow - don't crash the crash handler
            }

            // 1. Attempt to log crash details to file
            string? logPath = null;
            try
            {
                // Manually instantiate CrashService since DI container may not be available
                var crashService = new CrashService(
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<CrashService>.Instance);
                logPath = crashService.LogCrash(ex);
            }
            catch
            {
                // Swallow - don't crash the crash handler
            }

            // v0.3.16b: The Black Box - Attempt emergency save
            // 2. Try to preserve game state before displaying crash screen
            bool? backupSaved = null;
            try
            {
                if (host != null)
                {
                    var gameState = host.Services.GetService<GameState>();
                    if (gameState?.CurrentCharacter != null && gameState.IsSessionActive)
                    {
                        var emergencySaveService = new EmergencySaveService();
                        backupSaved = emergencySaveService.TryEmergencySave(gameState);
                    }
                }
            }
            catch
            {
                backupSaved = false;
            }

            // 3. Render user-friendly crash screen with backup status
            CrashScreenRenderer.Render(ex, logPath, backupSaved);

            // 4. Log to Serilog if still available
            Log.Fatal(ex, "System Crash");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    #region CLI Argument Helpers

    /// <summary>
    /// Parses an integer argument from CLI args (e.g., --iterations=10000).
    /// </summary>
    private static int ParseIntArg(string[] args, string name, int defaultValue)
    {
        var prefix = $"--{name}=";
        var arg = args.FirstOrDefault(a => a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (arg == null)
            return defaultValue;

        var valueStr = arg.Substring(prefix.Length);
        return int.TryParse(valueStr, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Parses an enum argument from CLI args (e.g., --biome=Industrial).
    /// </summary>
    private static T ParseEnumArg<T>(string[] args, string name, T defaultValue) where T : struct, Enum
    {
        var prefix = $"--{name}=";
        var arg = args.FirstOrDefault(a => a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (arg == null)
            return defaultValue;

        var valueStr = arg.Substring(prefix.Length);
        return Enum.TryParse<T>(valueStr, ignoreCase: true, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Parses a string argument from CLI args (e.g., --enemy=und_draugr_01).
    /// </summary>
    private static string ParseStringArg(string[] args, string name, string defaultValue)
    {
        var prefix = $"--{name}=";
        var arg = args.FirstOrDefault(a => a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (arg == null)
            return defaultValue;

        return arg.Substring(prefix.Length);
    }

    #endregion
}
