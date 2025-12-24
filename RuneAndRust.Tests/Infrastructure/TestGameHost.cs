using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Settings;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Engine.Services;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using RuneAndRust.Terminal.Rendering;
using RuneAndRust.Terminal.Services;
using Serilog;
using Serilog.Extensions.Logging;

namespace RuneAndRust.Tests.Infrastructure;

/// <summary>
/// A self-contained test host for running E2E integration tests.
/// Provides isolated DI container with ScriptedInputHandler and seeded RNG.
/// </summary>
public class TestGameHost : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private bool _disposed;

    /// <summary>
    /// The service provider for resolving services.
    /// </summary>
    public IServiceProvider Services => _serviceProvider;

    /// <summary>
    /// The game state singleton from this host's container.
    /// </summary>
    public GameState GameState => _serviceProvider.GetRequiredService<GameState>();

    /// <summary>
    /// The scripted input handler for this test run.
    /// </summary>
    public ScriptedInputHandler InputHandler { get; }

    /// <summary>
    /// The seed used for deterministic dice rolls.
    /// </summary>
    public int? Seed { get; }

    private TestGameHost(ServiceProvider provider, ScriptedInputHandler inputHandler, int? seed)
    {
        _serviceProvider = provider;
        InputHandler = inputHandler;
        Seed = seed;
    }

    /// <summary>
    /// Creates a new TestGameHost with the specified configuration.
    /// </summary>
    /// <param name="seed">Optional RNG seed for deterministic dice rolls.</param>
    /// <param name="script">The sequence of commands to execute.</param>
    /// <param name="databaseName">Optional database name for isolation. Defaults to unique GUID.</param>
    /// <returns>A configured TestGameHost ready for test execution.</returns>
    public static TestGameHost Create(int? seed, IEnumerable<string> script, string? databaseName = null)
    {
        var dbName = databaseName ?? $"TestDb_{Guid.NewGuid()}";
        var commands = script.ToList();

        // Configure Serilog for test output
        var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var services = new ServiceCollection();

        // Create ScriptedInputHandler with logger
        var loggerFactory = new SerilogLoggerFactory(serilogLogger);
        var inputHandlerLogger = loggerFactory.CreateLogger<ScriptedInputHandler>();
        var scriptedHandler = new ScriptedInputHandler(commands, inputHandlerLogger);

        // Register In-Memory Database
        services.AddDbContext<RuneAndRustDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

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
        services.AddScoped<IRoomTemplateRepository, RoomTemplateRepository>();
        services.AddScoped<IBiomeDefinitionRepository, BiomeDefinitionRepository>();
        services.AddScoped<IBiomeElementRepository, BiomeElementRepository>();

        // Register Core State (Singleton)
        services.AddSingleton<GameState>();

        // Register Scripted Input Handler (test implementation)
        services.AddSingleton<IInputHandler>(scriptedHandler);

        // Register Serilog Logger Factory
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        // Register DiceService with optional seed
        if (seed.HasValue)
        {
            services.AddSingleton<IDiceService>(sp =>
                new DiceService(sp.GetRequiredService<ILogger<DiceService>>(), seed));
        }
        else
        {
            services.AddSingleton<IDiceService, DiceService>();
        }

        // Register Engine Services
        services.AddSingleton<CommandParser>();
        services.AddSingleton<IGameService, GameService>();
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

        // Register Library Service
        services.AddSingleton<ILibraryService, LibraryService>();

        // Register DocGen Service
        services.AddScoped<IDocGenService, DocGenService>();

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

        // Skip TUI renderers for testing (they require Spectre.Console)
        // GameService accepts null renderers

        // Register Character Creation Services
        services.AddScoped<CharacterFactory>();
        services.AddScoped<CharacterCreationController>();

        // Register Enemy Factory
        services.AddScoped<IEnemyFactory, EnemyFactory>();

        // Register Inventory Services
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryService, InventoryService>();

        // Register Ambush Services
        services.AddScoped<IAmbushService, AmbushService>();

        // Register Rest Services
        services.AddScoped<IRestService, RestService>();

        // Register Crafting Services
        services.AddScoped<ICraftingService, CraftingService>();

        // Register Settings Service (with no-op implementation for tests)
        services.AddSingleton<ISettingsService, TestSettingsService>();

        // Register Visual Effect Service
        services.AddSingleton<IVisualEffectService, VisualEffectService>();

        // Register Theme Service
        services.AddSingleton<IThemeService, ThemeService>();

        // Register Input Configuration Service
        services.AddSingleton<IInputConfigurationService, InputConfigurationService>();

        // Register Context Help Service
        services.AddSingleton<IContextHelpService, ContextHelpService>();

        // Register Bodging Services
        services.AddScoped<IBodgingService, BodgingService>();

        // Register Hazard Services
        services.AddSingleton<EffectScriptExecutor>();
        services.AddScoped<IHazardService, HazardService>();

        // Register Condition Services
        services.AddScoped<IConditionService, ConditionService>();

        // Register Environment Ecosystem Services
        services.AddScoped<IEnvironmentPopulator, EnvironmentPopulator>();

        // Register Dynamic Room Engine Services
        services.AddScoped<ITemplateLoaderService, TemplateLoaderService>();
        services.AddScoped<ITemplateRendererService, TemplateRendererService>();
        services.AddScoped<IElementSpawnEvaluator, ElementSpawnEvaluator>();

        var provider = services.BuildServiceProvider();

        return new TestGameHost(provider, scriptedHandler, seed);
    }

    /// <summary>
    /// Runs the game loop until completion or script exhaustion.
    /// </summary>
    public async Task RunAsync()
    {
        var gameService = _serviceProvider.GetRequiredService<IGameService>();
        await gameService.StartAsync();
    }

    /// <summary>
    /// Sets up the game state for exploration testing.
    /// Creates a test character and a simple test room (bypasses DungeonGenerator seeder dependency).
    /// </summary>
    /// <param name="characterName">Name for the test character.</param>
    public async Task SetupExplorationAsync(string characterName = "TestRunner")
    {
        var gameState = GameState;
        var charFactory = _serviceProvider.GetRequiredService<CharacterFactory>();

        // Create a test character with default attributes
        var character = charFactory.CreateSimple(characterName, LineageType.Human, ArchetypeType.Warrior);
        gameState.CurrentCharacter = character;
        gameState.Phase = GamePhase.Exploration;

        // Create a simple test room directly (avoids biome seeder dependency)
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RuneAndRustDbContext>();

        var startRoom = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Test Chamber",
            Description = "A featureless chamber used for testing. The walls are smooth and unremarkable.",
            Position = Coordinate.Origin,
            IsStartingRoom = true,
            BiomeType = BiomeType.Ruin,
            DangerLevel = DangerLevel.Safe,
            Exits = new Dictionary<Direction, Guid>()
        };

        // Create a connected room to the north
        var northRoom = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Northern Corridor",
            Description = "A narrow passage leading further into the ruins.",
            Position = new Coordinate(0, 1, 0),
            BiomeType = BiomeType.Ruin,
            DangerLevel = DangerLevel.Unstable,
            Exits = new Dictionary<Direction, Guid> { { Direction.South, startRoom.Id } }
        };

        // Link start room to north room
        startRoom.Exits[Direction.North] = northRoom.Id;

        await context.Rooms.AddAsync(startRoom);
        await context.Rooms.AddAsync(northRoom);
        await context.SaveChangesAsync();

        gameState.CurrentRoomId = startRoom.Id;
        gameState.VisitedRoomIds.Add(startRoom.Id);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _serviceProvider.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// A minimal settings service implementation for tests.
/// Does not persist settings to disk.
/// </summary>
internal class TestSettingsService : ISettingsService
{
    public Task LoadAsync()
    {
        return Task.CompletedTask;
    }

    public Task SaveAsync()
    {
        return Task.CompletedTask;
    }

    public Task ResetToDefaultsAsync()
    {
        return Task.CompletedTask;
    }
}
