using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// High-level service for managing procedural dungeon generation (v0.10)
/// Coordinates TemplateLibrary, BiomeLibrary, SeedManager, and DungeonGenerator
/// </summary>
public class DungeonService
{
    private static readonly ILogger _log = Log.ForContext<DungeonService>();

    private readonly TemplateLibrary _templateLibrary;
    private readonly BiomeLibrary _biomeLibrary;
    private readonly SeedManager _seedManager;
    private readonly DungeonGenerator _generator;

    public DungeonService(
        TemplateLibrary templateLibrary,
        BiomeLibrary biomeLibrary,
        SeedManager seedManager)
    {
        _templateLibrary = templateLibrary;
        _biomeLibrary = biomeLibrary;
        _seedManager = seedManager;
        _generator = new DungeonGenerator(templateLibrary);
    }

    /// <summary>
    /// Creates a new procedurally generated GameWorld
    /// </summary>
    public GameWorld CreateProceduralWorld(int? seed = null, string? biomeId = null, int dungeonId = 1)
    {
        // Generate or use provided seed
        int actualSeed = seed ?? _seedManager.GenerateSeed();

        // Get biome (use default if not specified)
        BiomeDefinition? biome = null;
        if (!string.IsNullOrEmpty(biomeId))
        {
            biome = _biomeLibrary.GetBiome(biomeId);
            if (biome == null)
            {
                _log.Warning("Biome {BiomeId} not found, using default", biomeId);
                biome = _biomeLibrary.GetDefaultBiome();
            }
        }
        else
        {
            biome = _biomeLibrary.GetDefaultBiome();
        }

        _log.Information("Creating procedural world: Seed={Seed}, Biome={Biome}",
            actualSeed, biome?.Name ?? "None");

        // Generate complete dungeon
        var dungeon = _generator.GenerateComplete(actualSeed, dungeonId, biome: biome);

        // Create GameWorld from dungeon
        var world = new GameWorld(dungeon);

        _log.Information("Procedural world created: {RoomCount} rooms, Start={StartRoom}",
            world.Rooms.Count, world.StartRoomName);

        return world;
    }

    /// <summary>
    /// Creates a procedurally generated GameWorld with a specific seed string
    /// </summary>
    public GameWorld CreateProceduralWorldFromSeedString(string seedString, string? biomeId = null, int dungeonId = 1)
    {
        int seed = _seedManager.ParseSeed(seedString);
        return CreateProceduralWorld(seed, biomeId, dungeonId);
    }

    /// <summary>
    /// Regenerates a dungeon from a seed (for save/load)
    /// </summary>
    public GameWorld RegenerateDungeon(int seed, string biomeId, int dungeonId)
    {
        _log.Information("Regenerating dungeon: Seed={Seed}, Biome={Biome}, DungeonId={DungeonId}",
            seed, biomeId, dungeonId);

        var biome = _biomeLibrary.GetBiome(biomeId);
        if (biome == null)
        {
            _log.Warning("Biome {BiomeId} not found during regeneration, using default", biomeId);
            biome = _biomeLibrary.GetDefaultBiome();
        }

        var dungeon = _generator.GenerateComplete(seed, dungeonId, biome: biome);
        return new GameWorld(dungeon);
    }

    /// <summary>
    /// Gets information about loaded libraries
    /// </summary>
    public Dictionary<string, int> GetLibraryStatistics()
    {
        return new Dictionary<string, int>
        {
            ["Templates"] = _templateLibrary.GetTemplateCount(),
            ["Biomes"] = _biomeLibrary.GetBiomeCount()
        };
    }
}
