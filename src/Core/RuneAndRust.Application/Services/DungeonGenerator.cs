using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Orchestrates the complete dungeon generation pipeline:
/// 1. Topology Generation (Sprouting Vine algorithm)
/// 2. Room Instantiation (template selection)
/// 3. Entity Population (threat budget)
/// 4. Coherence Validation (tag conflicts, path validation)
/// </summary>
public class DungeonGenerator : IDungeonGenerator
{
    private readonly ITopologyGenerator _topologyGenerator;
    private readonly IRoomInstantiator _roomInstantiator;
    private readonly IEntityPopulator _entityPopulator;
    private readonly ICoherenceValidator _coherenceValidator;

    public DungeonGenerator(
        ITopologyGenerator topologyGenerator,
        IRoomInstantiator roomInstantiator,
        IEntityPopulator entityPopulator,
        ICoherenceValidator coherenceValidator)
    {
        _topologyGenerator = topologyGenerator ?? throw new ArgumentNullException(nameof(topologyGenerator));
        _roomInstantiator = roomInstantiator ?? throw new ArgumentNullException(nameof(roomInstantiator));
        _entityPopulator = entityPopulator ?? throw new ArgumentNullException(nameof(entityPopulator));
        _coherenceValidator = coherenceValidator ?? throw new ArgumentNullException(nameof(coherenceValidator));
    }

    /// <summary>
    /// Creates a DungeonGenerator with default service implementations.
    /// </summary>
    /// <param name="roomTemplateProvider">Provider for room templates.</param>
    /// <param name="entityTemplateProvider">Provider for entity templates.</param>
    public static DungeonGenerator CreateDefault(
        IRoomTemplateProvider roomTemplateProvider,
        IEntityTemplateProvider entityTemplateProvider)
    {
        return new DungeonGenerator(
            new SproutingVineTopologyGenerator(),
            new RoomInstantiator(roomTemplateProvider),
            new ThreatBudgetPopulator(entityTemplateProvider),
            new CoherenceValidator());
    }

    public Task<Dungeon> GenerateDungeonAsync(
        string name,
        Biome biome,
        DifficultyTier difficulty,
        int roomCount = 15,
        int? seed = null,
        CancellationToken ct = default)
    {
        // Generation is CPU-bound, run synchronously but wrapped for async interface
        return Task.Run(() => GenerateDungeon(name, biome, difficulty, roomCount, seed), ct);
    }

    public Dungeon GenerateDungeon(
        string name,
        Biome biome,
        DifficultyTier difficulty,
        int roomCount = 15,
        int? seed = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Dungeon name cannot be empty", nameof(name));
        if (roomCount < 3)
            throw new ArgumentOutOfRangeException(nameof(roomCount), "Minimum room count is 3");

        var actualSeed = seed ?? Environment.TickCount;
        var random = new Random(actualSeed);

        // Phase 1: Generate topology
        var sector = _topologyGenerator.GenerateSector(biome, roomCount, depth: 1, actualSeed);
        sector.CalculateThreatBudget(difficulty);

        // Phase 2: Instantiate rooms from templates
        var nodeToRoom = _roomInstantiator.InstantiateSector(sector, actualSeed);

        // Phase 3: Populate rooms with entities
        _entityPopulator.PopulateSector(sector, nodeToRoom, random);

        // Phase 4: Validate and fix coherence
        _coherenceValidator.ValidateAndFix(sector, nodeToRoom, random);

        // Phase 5: Build Dungeon entity
        var dungeon = BuildDungeon(name, sector, nodeToRoom);

        return dungeon;
    }

    private static Dungeon BuildDungeon(
        string name,
        Sector sector,
        IReadOnlyDictionary<Guid, Room> nodeToRoom)
    {
        var dungeon = new Dungeon(name);

        // Add all rooms
        foreach (var room in nodeToRoom.Values)
        {
            dungeon.AddRoom(room);
        }

        // Set starting room
        if (sector.StartNodeId.HasValue && nodeToRoom.TryGetValue(sector.StartNodeId.Value, out var startRoom))
        {
            dungeon.SetStartingRoom(startRoom.Id);
        }

        return dungeon;
    }
}
