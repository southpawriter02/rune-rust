using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Engine.Services;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using RuneAndRust.Terminal.Rendering;
using RuneAndRust.Terminal.Services;
using Serilog;
using Spectre.Console;
using Character = RuneAndRust.Core.Entities.Character;

class Program
{
    static void Main(string[] args)
    {
        // 1. Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/runeandrust.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            // Database connection string - use environment variable in production
            // Default uses port 5433 for Docker container (see docker-compose.yml)
            var connectionString = Environment.GetEnvironmentVariable("RUNEANDRUST_CONNECTION_STRING")
                ?? "Host=localhost;Port=5433;Database=RuneAndRust;Username=postgres;Password=password";

            // 2. Build Host
            var host = Host.CreateDefaultBuilder(args)
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

                    // Register Core State (Singleton to persist across game loop)
                    services.AddSingleton<GameState>();

                    // Register Input/Output Handler
                    services.AddSingleton<IInputHandler, TerminalInputHandler>();

                    // Register Engine Services
                    services.AddSingleton<CommandParser>();
                    services.AddSingleton<IGameService, GameService>();
                    services.AddSingleton<IDiceService, DiceService>();
                    services.AddSingleton<IStatCalculationService, StatCalculationService>();
                    services.AddScoped<SaveManager>();

                    // Register Spatial Services
                    services.AddScoped<DungeonGenerator>();
                    services.AddScoped<INavigationService, NavigationService>();

                    // Register Interaction Services
                    services.AddScoped<IDescriptorEngine, DescriptorEngine>();
                    services.AddScoped<IInteractionService, InteractionService>();
                    services.AddScoped<IDataCaptureService, DataCaptureService>();
                    services.AddScoped<ObjectSpawner>();

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

                    // Register Enemy Factory
                    services.AddScoped<IEnemyFactory, EnemyFactory>();

                    // Register Inventory Services
                    services.AddScoped<IInventoryRepository, InventoryRepository>();
                    services.AddScoped<IInventoryService, InventoryService>();

                    // Register Ambush Services (v0.3.2b)
                    services.AddScoped<IAmbushService, AmbushService>();

                    // Register Rest Services (v0.3.2a)
                    services.AddScoped<IRestService, RestService>();

                    // Register Rest Screen Renderer (v0.3.2c)
                    services.AddSingleton<IRestScreenRenderer, RestScreenRenderer>();

                    // Register Crafting Services (v0.3.1a)
                    services.AddScoped<ICraftingService, CraftingService>();

                    // Register Bodging Services (v0.3.1b)
                    services.AddScoped<IBodgingService, BodgingService>();

                    // Register Hazard Services (v0.3.3a)
                    services.AddSingleton<EffectScriptExecutor>();
                    services.AddScoped<IHazardService, HazardService>();

                    // Register Condition Services (v0.3.3b)
                    services.AddScoped<IConditionService, ConditionService>();

                    // Register Environment Ecosystem Services (v0.3.3c)
                    services.AddScoped<IEnvironmentPopulator, EnvironmentPopulator>();

                    // Register Title Screen Service (v0.3.4a)
                    services.AddScoped<ITitleScreenService, TitleScreenService>();
                })
                .UseSerilog() // Wire Serilog into ILogger
                .Build();

            // 3. Seed data
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RuneAndRustDbContext>();
                AbilitySeeder.SeedAsync(context).GetAwaiter().GetResult();
                ConditionSeeder.SeedAsync(context).GetAwaiter().GetResult();
                HazardTemplateSeeder.SeedAsync(context).GetAwaiter().GetResult();
            }

            // 4. UI Handover
            AnsiConsole.MarkupLine("[green]Rune & Rust v0.3.4c Booting...[/]");
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

                        // Generate starting dungeon
                        using (var scope = host.Services.CreateScope())
                        {
                            var dungeonGen = scope.ServiceProvider.GetRequiredService<DungeonGenerator>();
                            var startRoomId = dungeonGen.GenerateTestMapAsync().GetAwaiter().GetResult();
                            gameState.CurrentRoomId = startRoomId;
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
                            }
                        }
                    }
                    break;
            }

            // Start the game loop
            var game = host.Services.GetRequiredService<IGameService>();
            game.StartAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "System Crash");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
