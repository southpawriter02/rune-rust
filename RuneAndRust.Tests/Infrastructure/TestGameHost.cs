using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Settings;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Algorithms;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Engine.Performance;
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
/// <remarks>See: SPEC-JOURNEY-001 for E2E Journey Testing Framework design.</remarks>
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

    /// <summary>
    /// The database name used by this test host.
    /// Can be passed to another TestGameHost.Create() to share the same database (for persistence tests).
    /// </summary>
    public string DatabaseName { get; }

    private TestGameHost(ServiceProvider provider, ScriptedInputHandler inputHandler, int? seed, string databaseName)
    {
        _serviceProvider = provider;
        InputHandler = inputHandler;
        Seed = seed;
        DatabaseName = databaseName;
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
        services.AddScoped<ISpecializationRepository, SpecializationRepository>();

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
        services.AddScoped<ISagaService, SagaService>();
        services.AddScoped<IProgressionService, ProgressionService>();
        services.AddScoped<ISpecializationService, SpecializationService>();
        services.AddScoped<SaveManager>();

        // Register Spatial Services
        services.AddScoped<DungeonGenerator>();
        services.AddScoped<INavigationService, NavigationService>();

        // Register Pathfinding Services
        services.AddSingleton<ISpatialHashGrid, SpatialHashGrid>();
        services.AddSingleton<IPathfindingService, AStarPathfinder>();

        // Register Event Bus (v0.3.19b)
        services.AddSingleton<IEventBus, EventBus>();

        // Register Interaction Services
        services.AddScoped<IDescriptorEngine, DescriptorEngine>();
        services.AddScoped<IInteractionService, InteractionService>();
        services.AddSingleton<ICaptureTemplateRepository, TestCaptureTemplateRepository>(); // v0.3.25c
        services.AddScoped<IDataCaptureService, DataCaptureService>();
        services.AddScoped<ObjectSpawner>();

        // Register Library Service
        services.AddSingleton<ILibraryService, LibraryService>();

        // Register DocGen Service
        services.AddScoped<IDocGenService, DocGenService>();

        // Register Loot Audit Service
        services.AddScoped<ILootAuditService, LootAuditService>();

        // Register Combat Audit Service
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
        services.AddSingleton<IAetherService, AetherService>();
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

        return new TestGameHost(provider, scriptedHandler, seed, dbName);
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

    /// <summary>
    /// Sets up the game state for combat testing.
    /// Creates a test character, a test room, and initiates combat with a weak enemy.
    /// </summary>
    /// <param name="characterName">Name for the test character.</param>
    /// <param name="enemyHp">HP for the test enemy. Lower values allow faster combat tests.</param>
    public async Task SetupCombatAsync(string characterName = "TestWarrior", int enemyHp = 15)
    {
        // First set up exploration (creates character and room)
        await SetupExplorationAsync(characterName);

        // Create a weak test enemy for quick combat resolution
        var testEnemy = new Enemy
        {
            Id = Guid.NewGuid(),
            Name = "Training Dummy",
            MaxHp = enemyHp,
            CurrentHp = enemyHp,
            MaxStamina = 50,
            CurrentStamina = 50,
            WeaponDamageDie = 4,
            WeaponAccuracyBonus = 0,
            ArmorSoak = 0,
            WeaponName = "Wooden Arms",
            Archetype = EnemyArchetype.DPS,
            Tags = new List<string> { "Training" },
            Attributes = new Dictionary<RuneAndRust.Core.Enums.Attribute, int>
            {
                { RuneAndRust.Core.Enums.Attribute.Sturdiness, 3 },
                { RuneAndRust.Core.Enums.Attribute.Might, 3 },
                { RuneAndRust.Core.Enums.Attribute.Wits, 1 },
                { RuneAndRust.Core.Enums.Attribute.Will, 1 },
                { RuneAndRust.Core.Enums.Attribute.Finesse, 2 }
            }
        };

        // Start combat with the test enemy
        var combatService = _serviceProvider.GetRequiredService<ICombatService>();
        combatService.StartCombat(new List<Enemy> { testEnemy });
    }

    /// <summary>
    /// Gets the combat service for direct assertions on combat state.
    /// </summary>
    public ICombatService GetCombatService() => _serviceProvider.GetRequiredService<ICombatService>();

    /// <summary>
    /// Gets the save manager for persistence operations.
    /// Creates a new scope to ensure proper repository lifecycle.
    /// </summary>
    /// <returns>A tuple containing the scope (for disposal) and the SaveManager instance.</returns>
    public (IServiceScope Scope, SaveManager Manager) GetSaveManager()
    {
        var scope = _serviceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<SaveManager>();
        return (scope, manager);
    }

    /// <summary>
    /// Saves the current game state to a slot using SaveManager.
    /// </summary>
    /// <param name="slot">The save slot number (1-3).</param>
    /// <returns>True if save succeeded; otherwise, false.</returns>
    public async Task<bool> SaveGameAsync(int slot)
    {
        using var scope = _serviceProvider.CreateScope();
        var saveManager = scope.ServiceProvider.GetRequiredService<SaveManager>();
        return await saveManager.SaveGameAsync(slot, GameState);
    }

    /// <summary>
    /// Loads game state from a slot and applies it to this host's GameState.
    /// </summary>
    /// <param name="slot">The save slot number to load.</param>
    /// <returns>True if load succeeded; otherwise, false.</returns>
    public async Task<bool> LoadGameAsync(int slot)
    {
        using var scope = _serviceProvider.CreateScope();
        var saveManager = scope.ServiceProvider.GetRequiredService<SaveManager>();
        var loadedState = await saveManager.LoadGameAsync(slot);

        if (loadedState == null)
            return false;

        // Apply loaded state to this host's GameState singleton
        var gameState = GameState;
        gameState.CurrentCharacter = loadedState.CurrentCharacter;
        gameState.CurrentRoomId = loadedState.CurrentRoomId;
        gameState.Phase = loadedState.Phase;
        gameState.TurnCount = loadedState.TurnCount;
        gameState.VisitedRoomIds = loadedState.VisitedRoomIds;
        gameState.IsSessionActive = loadedState.IsSessionActive;

        return true;
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

/// <summary>
/// A minimal capture template repository implementation for tests.
/// Returns test templates for each category without loading from disk.
/// v0.3.25c: Supports data-driven template system in tests.
/// </summary>
internal class TestCaptureTemplateRepository : ICaptureTemplateRepository
{
    private readonly Dictionary<string, List<CaptureTemplateDto>> _templates;
    private readonly Random _random = new();

    public TestCaptureTemplateRepository()
    {
        // Initialize with test templates for each category
        _templates = new Dictionary<string, List<CaptureTemplateDto>>
        {
            ["generic-container"] = new()
            {
                new CaptureTemplateDto
                {
                    Id = "test-generic-1",
                    Type = CaptureType.TextFragment,
                    FragmentContent = "A weathered document found within.",
                    Source = "Container Search",
                    MatchKeywords = new[] { "container", "salvage" },
                    Category = "generic-container"
                }
            },
            ["rusted-servitor"] = new()
            {
                new CaptureTemplateDto
                {
                    Id = "test-servitor-1",
                    Type = CaptureType.Specimen,
                    FragmentContent = "Corroded metal fragments from an ancient automaton.",
                    Source = "Servitor Remains",
                    MatchKeywords = new[] { "servitor", "automaton", "machine" },
                    Category = "rusted-servitor"
                }
            },
            ["blighted-creature"] = new()
            {
                new CaptureTemplateDto
                {
                    Id = "test-blighted-1",
                    Type = CaptureType.Specimen,
                    FragmentContent = "Tissue sample showing signs of corruption.",
                    Source = "Blighted Remains",
                    MatchKeywords = new[] { "blight", "corruption", "infected" },
                    Category = "blighted-creature"
                }
            },
            ["industrial-site"] = new()
            {
                new CaptureTemplateDto
                {
                    Id = "test-industrial-1",
                    Type = CaptureType.TextFragment,
                    FragmentContent = "Schematics for forgotten machinery.",
                    Source = "Industrial Site",
                    MatchKeywords = new[] { "forge", "mechanism", "industrial" },
                    Category = "industrial-site"
                }
            },
            ["ancient-ruin"] = new()
            {
                new CaptureTemplateDto
                {
                    Id = "test-ruin-1",
                    Type = CaptureType.RunicTrace,
                    FragmentContent = "Faded inscriptions on crumbling stone.",
                    Source = "Ancient Inscription",
                    MatchKeywords = new[] { "ancient", "ruin", "inscription" },
                    Category = "ancient-ruin"
                }
            }
        };
    }

    public int TotalTemplateCount => _templates.Values.Sum(list => list.Count);

    public Task<IReadOnlyList<CaptureTemplateDto>> GetByCategoryAsync(string category)
    {
        if (_templates.TryGetValue(category, out var templates))
            return Task.FromResult<IReadOnlyList<CaptureTemplateDto>>(templates.AsReadOnly());
        return Task.FromResult<IReadOnlyList<CaptureTemplateDto>>(Array.Empty<CaptureTemplateDto>());
    }

    public Task<CaptureTemplateDto?> GetRandomAsync(string category)
    {
        if (_templates.TryGetValue(category, out var templates) && templates.Count > 0)
        {
            var index = _random.Next(templates.Count);
            return Task.FromResult<CaptureTemplateDto?>(templates[index]);
        }
        return Task.FromResult<CaptureTemplateDto?>(null);
    }

    public Task<IReadOnlyList<string>> GetCategoriesAsync()
    {
        return Task.FromResult<IReadOnlyList<string>>(_templates.Keys.ToList().AsReadOnly());
    }

    public Task<CaptureTemplateDto?> GetByIdAsync(string templateId)
    {
        foreach (var templates in _templates.Values)
        {
            var template = templates.FirstOrDefault(t => t.Id == templateId);
            if (template != null)
                return Task.FromResult<CaptureTemplateDto?>(template);
        }
        return Task.FromResult<CaptureTemplateDto?>(null);
    }

    public Task ReloadAsync()
    {
        // No-op for tests
        return Task.CompletedTask;
    }
}
