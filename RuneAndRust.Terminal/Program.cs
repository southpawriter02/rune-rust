using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Engine.Services;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using RuneAndRust.Terminal.Services;
using Serilog;
using Spectre.Console;

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
            var connectionString = Environment.GetEnvironmentVariable("RUNEANDRUST_CONNECTION_STRING")
                ?? "Host=localhost;Database=RuneAndRust;Username=postgres;Password=password";

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

                    // Register Enemy Factory
                    services.AddScoped<IEnemyFactory, EnemyFactory>();

                    // Register Inventory Services
                    services.AddScoped<IInventoryService, InventoryService>();

                    // Register Crafting Services (v0.3.1a)
                    services.AddScoped<ICraftingService, CraftingService>();

                    // Register Bodging Services (v0.3.1b)
                    services.AddScoped<IBodgingService, BodgingService>();
                })
                .UseSerilog() // Wire Serilog into ILogger
                .Build();

            // 3. Seed data
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RuneAndRustDbContext>();
                AbilitySeeder.SeedAsync(context).GetAwaiter().GetResult();
            }

            // 4. UI Handover
            AnsiConsole.MarkupLine("[green]Rune & Rust v0.3.1b Booting...[/]");
            AnsiConsole.WriteLine();

            // Resolve the entry point from DI
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
