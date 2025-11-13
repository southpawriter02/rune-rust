using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;
using System.Diagnostics;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.11 Population Pipeline
/// Coordinates all population spawners to fill rooms with enemies, hazards, terrain, loot, and conditions
/// Implements the v2.0 data-driven population system
/// </summary>
public class PopulationPipeline
{
    private static readonly ILogger _log = Log.ForContext<PopulationPipeline>();

    private readonly DormantProcessSpawner _processSpawner;
    private readonly HazardSpawner _hazardSpawner;
    private readonly TerrainSpawner _terrainSpawner;
    private readonly LootSpawner _lootSpawner;
    private readonly ConditionApplier _conditionApplier;

    public PopulationPipeline(
        DormantProcessSpawner processSpawner,
        HazardSpawner hazardSpawner,
        TerrainSpawner terrainSpawner,
        LootSpawner lootSpawner,
        ConditionApplier conditionApplier)
    {
        _processSpawner = processSpawner;
        _hazardSpawner = hazardSpawner;
        _terrainSpawner = terrainSpawner;
        _lootSpawner = lootSpawner;
        _conditionApplier = conditionApplier;
    }

    /// <summary>
    /// Populates all rooms in a dungeon with enemies, hazards, terrain, loot, and conditions
    /// Pipeline order: Conditions → Hazards → Terrain → Enemies → Loot
    /// (Conditions first so they can affect spawn weights via Coherent Glitch rules)
    /// </summary>
    public void PopulateDungeon(Dungeon dungeon, BiomeDefinition biome, Random rng)
    {
        var stopwatch = Stopwatch.StartNew();
        _log.Information("Starting population pipeline for dungeon {DungeonId}, biome {BiomeName}",
            dungeon.DungeonId, biome.Name);

        int populatedRoomCount = 0;
        int skippedHandcraftedCount = 0;

        foreach (var room in dungeon.Rooms.Values)
        {
            if (room.IsHandcrafted)
            {
                _log.Debug("Skipping handcrafted room {RoomId} (Quest Anchor)", room.RoomId);
                skippedHandcraftedCount++;
                continue;
            }

            PopulateRoom(room, biome, rng);
            populatedRoomCount++;
        }

        stopwatch.Stop();
        _log.Information(
            "Population pipeline complete: {PopulatedRooms} rooms populated, {SkippedRooms} handcrafted rooms skipped, duration: {Duration}ms",
            populatedRoomCount, skippedHandcraftedCount, stopwatch.ElapsedMilliseconds);

        // Validation
        ValidatePopulation(dungeon);
    }

    /// <summary>
    /// Populates a single room following the correct pipeline order
    /// </summary>
    public void PopulateRoom(Room room, BiomeDefinition biome, Random rng)
    {
        _log.Debug("Populating room {RoomId} ({RoomName})", room.RoomId, room.Name);

        try
        {
            // Step 1: Apply ambient conditions (first, so they affect spawn weights)
            _conditionApplier.ApplyConditions(room, biome, rng);

            // Step 2: Spawn dynamic hazards
            _hazardSpawner.PopulateRoom(room, biome, rng);

            // Step 3: Place static terrain (may depend on hazards via Coherent Glitch rules)
            _terrainSpawner.PopulateRoom(room, biome, rng);

            // Step 4: Spawn enemies
            _processSpawner.PopulateRoom(room, biome, rng);

            // Step 5: Place loot nodes
            _lootSpawner.PopulateRoom(room, biome, rng);

            _log.Debug("Room {RoomId} population complete: {Enemies} enemies, {Hazards} hazards, {Terrain} terrain, {Loot} loot, {Conditions} conditions",
                room.RoomId,
                room.Enemies.Count,
                room.DynamicHazards.Count,
                room.StaticTerrain.Count,
                room.LootNodes.Count,
                room.AmbientConditions.Count);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error populating room {RoomId}", room.RoomId);
            throw;
        }
    }

    /// <summary>
    /// Validates the populated dungeon for common issues
    /// </summary>
    private void ValidatePopulation(Dungeon dungeon)
    {
        var issues = new List<string>();

        // Check for empty rooms (excluding entry halls and secret rooms)
        var emptyNormalRooms = dungeon.Rooms.Values
            .Where(r => !r.IsHandcrafted &&
                       !r.IsStartRoom &&
                       r.GeneratedNodeType != NodeType.Secret &&
                       r.Enemies.Count == 0 &&
                       r.DynamicHazards.Count == 0 &&
                       r.StaticTerrain.Count == 0 &&
                       r.LootNodes.Count == 0)
            .ToList();

        if (emptyNormalRooms.Count > 0)
        {
            _log.Warning("Found {Count} empty rooms (excluding entry/secret): {RoomIds}",
                emptyNormalRooms.Count,
                string.Join(", ", emptyNormalRooms.Select(r => r.RoomId)));
        }

        // Check boss room has a boss
        var bossRoom = dungeon.GetBossRoom();
        if (bossRoom != null && bossRoom.Enemies.Count == 0)
        {
            issues.Add("Boss room has no enemies");
        }

        // Check for overpopulation
        var overpopulatedRooms = dungeon.Rooms.Values
            .Where(r => r.Enemies.Count > 8)
            .ToList();

        if (overpopulatedRooms.Count > 0)
        {
            _log.Warning("Found {Count} overpopulated rooms (>8 enemies): {RoomIds}",
                overpopulatedRooms.Count,
                string.Join(", ", overpopulatedRooms.Select(r => r.RoomId)));
        }

        // Check for Coherent Glitch rule violations
        foreach (var room in dungeon.Rooms.Values)
        {
            // Unstable Ceiling should have Rubble Pile
            bool hasUnstableCeiling = room.DynamicHazards.Cast<DynamicHazard>().Any(h => h.Type == DynamicHazardType.UnstableCeiling);
            bool hasRubblePile = room.StaticTerrain.Cast<StaticTerrain>().Any(t => t.Type == StaticTerrainType.RubblePile);

            if (hasUnstableCeiling && !hasRubblePile)
            {
                issues.Add($"Room {room.RoomId} has Unstable Ceiling but no Rubble Pile (Coherent Glitch violation)");
            }
        }

        if (issues.Count > 0)
        {
            _log.Warning("Population validation found {Count} issues: {Issues}",
                issues.Count, string.Join("; ", issues));
        }
        else
        {
            _log.Information("Population validation passed");
        }
    }

    /// <summary>
    /// Gets population statistics for a dungeon
    /// </summary>
    public Dictionary<string, int> GetStatistics(Dungeon dungeon)
    {
        return new Dictionary<string, int>
        {
            ["TotalEnemies"] = dungeon.Rooms.Values.Sum(r => r.Enemies.Count),
            ["TotalHazards"] = dungeon.Rooms.Values.Sum(r => r.DynamicHazards.Count),
            ["TotalTerrain"] = dungeon.Rooms.Values.Sum(r => r.StaticTerrain.Count),
            ["TotalLoot"] = dungeon.Rooms.Values.Sum(r => r.LootNodes.Count),
            ["TotalConditions"] = dungeon.Rooms.Values.Sum(r => r.AmbientConditions.Count),
            ["RoomsWithEnemies"] = dungeon.Rooms.Values.Count(r => r.Enemies.Count > 0),
            ["RoomsWithHazards"] = dungeon.Rooms.Values.Count(r => r.DynamicHazards.Count > 0),
            ["RoomsWithLoot"] = dungeon.Rooms.Values.Count(r => r.LootNodes.Count > 0),
            ["EmptyRooms"] = dungeon.Rooms.Values.Count(r =>
                r.Enemies.Count == 0 &&
                r.DynamicHazards.Count == 0 &&
                r.StaticTerrain.Count == 0 &&
                r.LootNodes.Count == 0 &&
                r.AmbientConditions.Count == 0)
        };
    }
}
