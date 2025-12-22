using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Generates dungeon layouts by creating connected room graphs.
/// v0.4.0: Implements template-based generation from BiomeDefinitions and RoomTemplates.
/// Uses Dynamic Room Engine with variable substitution and element spawning.
/// </summary>
public class DungeonGenerator
{
    private readonly IRoomRepository _roomRepository;
    private readonly IEnvironmentPopulator _environmentPopulator;
    private readonly IRoomTemplateRepository _roomTemplateRepository;
    private readonly IBiomeDefinitionRepository _biomeDefinitionRepository;
    private readonly ITemplateRendererService _templateRendererService;
    private readonly IDiceService _diceService;
    private readonly ILogger<DungeonGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DungeonGenerator"/> class.
    /// </summary>
    /// <param name="roomRepository">The room repository for persistence.</param>
    /// <param name="environmentPopulator">The environment populator for hazards/conditions.</param>
    /// <param name="roomTemplateRepository">Repository for room templates.</param>
    /// <param name="biomeDefinitionRepository">Repository for biome definitions.</param>
    /// <param name="templateRendererService">Service for rendering room names/descriptions from templates.</param>
    /// <param name="diceService">Service for deterministic random number generation.</param>
    /// <param name="logger">The logger instance.</param>
    public DungeonGenerator(
        IRoomRepository roomRepository,
        IEnvironmentPopulator environmentPopulator,
        IRoomTemplateRepository roomTemplateRepository,
        IBiomeDefinitionRepository biomeDefinitionRepository,
        ITemplateRendererService templateRendererService,
        IDiceService diceService,
        ILogger<DungeonGenerator> logger)
    {
        _roomRepository = roomRepository;
        _environmentPopulator = environmentPopulator;
        _roomTemplateRepository = roomTemplateRepository;
        _biomeDefinitionRepository = biomeDefinitionRepository;
        _templateRendererService = templateRendererService;
        _diceService = diceService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a template-based dungeon from a biome definition (v0.4.0).
    /// Implements Phase 1 linear layout: EntryHall → alternating Corridors/Chambers → BossArena.
    /// Renders room names/descriptions using variable substitution and populates with biome elements.
    /// </summary>
    /// <param name="biomeId">The biome ID (e.g., "the_roots").</param>
    /// <returns>The ID of the starting room (EntryHall).</returns>
    public async Task<Guid> GenerateDungeonAsync(string biomeId)
    {
        _logger.LogInformation("[DungeonGenerator] Generating dungeon for biome: {BiomeId}", biomeId);

        // 1. Load biome definition
        var biome = await _biomeDefinitionRepository.GetByBiomeIdAsync(biomeId);

        if (biome == null)
        {
            throw new InvalidOperationException($"Biome '{biomeId}' not found in database. Run RoomTemplateSeeder first.");
        }

        _logger.LogDebug("[DungeonGenerator] Loaded biome: {Name} with {TemplateCount} available templates",
            biome.Name, biome.AvailableTemplates.Count);

        // 2. Determine room count
        var roomCount = _diceService.RollSingle(biome.MaxRoomCount - biome.MinRoomCount + 1, "room count") + biome.MinRoomCount - 1;
        _logger.LogDebug("[DungeonGenerator] Generating {RoomCount} rooms (range: {Min}-{Max})",
            roomCount, biome.MinRoomCount, biome.MaxRoomCount);

        // 3. Clear existing rooms for a fresh start
        await _roomRepository.ClearAllRoomsAsync();

        // 4. Generate room layout graph (positions + template assignments)
        var roomLayouts = await GenerateLinearLayoutAsync(roomCount, biome);

        // 5. Instantiate Room entities from templates
        var rooms = new List<Room>();
        foreach (var layout in roomLayouts)
        {
            var room = await InstantiateRoomFromTemplateAsync(layout.Template, layout.Position, layout.IsStartingRoom, biomeId);
            rooms.Add(room);
        }

        // 6. Link rooms bidirectionally
        LinkRoomsInSequence(rooms);

        // 7. Populate each room with elements
        foreach (var room in rooms)
        {
            var template = roomLayouts.First(l => l.Position == room.Position).Template;
            await _environmentPopulator.PopulateRoomAsync(room, template, biomeId);
        }

        // 8. Persist to database
        await _roomRepository.AddRangeAsync(rooms);
        await _roomRepository.SaveChangesAsync();

        var startingRoom = rooms.First(r => r.IsStartingRoom);
        _logger.LogInformation("[DungeonGenerator] Generated dungeon with {Count} rooms. Starting room: {RoomName} ({RoomId})",
            rooms.Count, startingRoom.Name, startingRoom.Id);

        return startingRoom.Id;
    }

    /// <summary>
    /// Generates and persists a simple test map with connected rooms.
    /// Backwards compatibility wrapper that calls GenerateDungeonAsync with "the_roots" biome.
    /// </summary>
    /// <returns>The ID of the starting room.</returns>
    [Obsolete("Use GenerateDungeonAsync(biomeId) instead. This method is deprecated in v0.4.0.")]
    public async Task<Guid> GenerateTestMapAsync()
    {
        _logger.LogInformation("[DungeonGenerator] GenerateTestMapAsync called - forwarding to GenerateDungeonAsync('the_roots')");
        return await GenerateDungeonAsync("the_roots");
    }

    /// <summary>
    /// Generates a linear room layout: EntryHall → alternating Corridors/Chambers → BossArena.
    /// Phase 1 implementation uses simple northward progression.
    /// </summary>
    private async Task<List<RoomLayout>> GenerateLinearLayoutAsync(int roomCount, BiomeDefinition biome)
    {
        _logger.LogDebug("[DungeonGenerator] Generating linear layout with {RoomCount} rooms", roomCount);

        var layouts = new List<RoomLayout>();
        var position = new Coordinate(0, 0, 0);

        // 1. Place EntryHall at origin
        var entryHallTemplate = await SelectTemplateByArchetypeAsync("EntryHall", biome);
        layouts.Add(new RoomLayout
        {
            Template = entryHallTemplate,
            Position = position,
            IsStartingRoom = true
        });

        _logger.LogDebug("[DungeonGenerator] Placed EntryHall at {Position} using template {TemplateId}",
            position, entryHallTemplate.TemplateId);

        // 2. Generate main path northward (alternating Corridor and Chamber)
        for (int i = 1; i < roomCount - 1; i++)
        {
            position = new Coordinate(0, i, 0); // Move north

            // Alternate between Corridor (~33%) and Chamber (~67%)
            var archetype = _diceService.RollSingle(3, "archetype selection") == 1 ? "Corridor" : "Chamber";
            var template = await SelectTemplateByArchetypeAsync(archetype, biome);

            layouts.Add(new RoomLayout
            {
                Template = template,
                Position = position,
                IsStartingRoom = false
            });

            _logger.LogDebug("[DungeonGenerator] Placed {Archetype} at {Position} using template {TemplateId}",
                archetype, position, template.TemplateId);
        }

        // 3. Place BossArena at the end
        position = new Coordinate(0, roomCount - 1, 0);
        var bossArenaTemplate = await SelectTemplateByArchetypeAsync("BossArena", biome);
        layouts.Add(new RoomLayout
        {
            Template = bossArenaTemplate,
            Position = position,
            IsStartingRoom = false
        });

        _logger.LogDebug("[DungeonGenerator] Placed BossArena at {Position} using template {TemplateId}",
            position, bossArenaTemplate.TemplateId);

        return layouts;
    }

    /// <summary>
    /// Selects a random template by archetype from the biome's available templates.
    /// </summary>
    private async Task<RoomTemplate> SelectTemplateByArchetypeAsync(string archetype, BiomeDefinition biome)
    {
        var allTemplates = await _roomTemplateRepository.GetAllAsync();
        var candidates = allTemplates
            .Where(t => t.Archetype.Equals(archetype, StringComparison.OrdinalIgnoreCase)
                     && biome.AvailableTemplates.Contains(t.TemplateId))
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException($"No templates found for archetype '{archetype}' in biome '{biome.BiomeId}'. " +
                                              $"Available templates: {string.Join(", ", biome.AvailableTemplates)}");
        }

        var index = _diceService.RollSingle(candidates.Count, $"template selection for {archetype}") - 1;
        var selected = candidates[index];

        _logger.LogDebug("[DungeonGenerator] Selected template {TemplateId} for archetype {Archetype} ({Index}/{Count})",
            selected.TemplateId, archetype, index + 1, candidates.Count);

        return selected;
    }

    /// <summary>
    /// Instantiates a Room entity from a template with rendered name/description.
    /// </summary>
    private async Task<Room> InstantiateRoomFromTemplateAsync(RoomTemplate template, Coordinate position, bool isStartingRoom, string biomeId)
    {
        // Load biome definition for description rendering
        var biome = await _biomeDefinitionRepository.GetByBiomeIdAsync(biomeId);

        if (biome == null)
        {
            throw new InvalidOperationException($"Biome '{biomeId}' not found during room instantiation.");
        }

        // Render name and description
        var name = _templateRendererService.RenderRoomName(template);
        var description = _templateRendererService.RenderRoomDescription(template, biome);

        // Map biomeId to BiomeType enum
        var biomeType = MapBiomeIdToBiomeType(biomeId);

        // Map template Difficulty to DangerLevel enum
        var dangerLevel = MapDifficultyToDangerLevel(template.Difficulty);

        var room = new Room
        {
            Name = name,
            Description = description,
            Position = position,
            IsStartingRoom = isStartingRoom,
            BiomeType = biomeType,
            DangerLevel = dangerLevel
        };

        _logger.LogDebug("[DungeonGenerator] Instantiated room: {Name} at {Position} (BiomeType: {BiomeType}, DangerLevel: {DangerLevel})",
            name, position, biomeType, dangerLevel);

        return room;
    }

    /// <summary>
    /// Links rooms in sequence with bidirectional North/South connections.
    /// </summary>
    private void LinkRoomsInSequence(List<Room> rooms)
    {
        _logger.LogDebug("[DungeonGenerator] Linking {RoomCount} rooms in sequence", rooms.Count);

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            var currentRoom = rooms[i];
            var nextRoom = rooms[i + 1];

            currentRoom.Exits[Direction.North] = nextRoom.Id;
            nextRoom.Exits[Direction.South] = currentRoom.Id;

            _logger.LogDebug("[DungeonGenerator] Linked {Room1} <-> {Room2} (North/South)",
                currentRoom.Name, nextRoom.Name);
        }
    }

    /// <summary>
    /// Maps biomeId string to BiomeType enum.
    /// </summary>
    private BiomeType MapBiomeIdToBiomeType(string biomeId)
    {
        return biomeId.ToLowerInvariant() switch
        {
            "the_roots" => BiomeType.Industrial,
            _ => BiomeType.Industrial // Default fallback
        };
    }

    /// <summary>
    /// Maps template Difficulty string to DangerLevel enum.
    /// </summary>
    private DangerLevel MapDifficultyToDangerLevel(string difficulty)
    {
        return difficulty.ToLowerInvariant() switch
        {
            "easy" => DangerLevel.Safe,
            "medium" => DangerLevel.Moderate,
            "hard" => DangerLevel.Dangerous,
            "veryhard" => DangerLevel.Lethal,
            _ => DangerLevel.Moderate // Default fallback
        };
    }

    /// <summary>
    /// Helper class for room layout planning.
    /// </summary>
    private class RoomLayout
    {
        public required RoomTemplate Template { get; init; }
        public required Coordinate Position { get; init; }
        public required bool IsStartingRoom { get; init; }
    }

    #region Legacy Test Map Methods (Deprecated in v0.4.0)

    /// <summary>
    /// Creates the individual room entities for the test map.
    /// </summary>
    [Obsolete("Legacy method for hardcoded test map. Use GenerateDungeonAsync instead.")]
    private Dictionary<Coordinate, Room> CreateTestRooms()
    {
        var rooms = new Dictionary<Coordinate, Room>();

        // 1. Origin (0,0,0): Entry Hall - Starting Room
        var entry = new Room
        {
            Name = "Entry Hall",
            Description = "A cold, metallic chamber. The air smells of ozone and ancient dust. " +
                         "Faded runes pulse weakly along the walls, their meaning lost to time. " +
                         "Passages lead in several directions.",
            Position = new Coordinate(0, 0, 0),
            IsStartingRoom = true
        };
        rooms[entry.Position] = entry;

        // 2. North (0,1,0): Rusted Corridor
        var corridor = new Room
        {
            Name = "Rusted Corridor",
            Description = "Corroded pipes line the walls of this narrow passage. " +
                         "Water drips from unseen sources, leaving rust-red stains on the floor. " +
                         "The air grows colder here.",
            Position = new Coordinate(0, 1, 0)
        };
        rooms[corridor.Position] = corridor;

        // 3. East (1,0,0): Storage Chamber
        var storage = new Room
        {
            Name = "Storage Chamber",
            Description = "Broken crates and shattered containers litter this abandoned storeroom. " +
                         "Whatever was kept here was either looted long ago or claimed by decay. " +
                         "Dust motes drift in the pale light.",
            Position = new Coordinate(1, 0, 0)
        };
        rooms[storage.Position] = storage;

        // 4. West (-1,0,0): Collapsed Tunnel
        var collapsed = new Room
        {
            Name = "Collapsed Tunnel",
            Description = "Rubble partially blocks this passage. The ceiling groans ominously overhead. " +
                         "Cracks in the walls reveal glimpses of darkness beyond. " +
                         "This area seems unstable.",
            Position = new Coordinate(-1, 0, 0)
        };
        rooms[collapsed.Position] = collapsed;

        // 5. Down (0,0,-1): The Pit
        var pit = new Room
        {
            Name = "The Pit",
            Description = "A deep shaft descends into absolute darkness. " +
                         "Ancient machinery clings to the walls, silent and still. " +
                         "The echoes of your footsteps seem to go on forever.",
            Position = new Coordinate(0, 0, -1)
        };
        rooms[pit.Position] = pit;

        _logger.LogDebug("Created {Count} room entities", rooms.Count);

        return rooms;
    }

    /// <summary>
    /// Links rooms together by populating their Exits dictionaries.
    /// All connections are bidirectional.
    /// </summary>
    [Obsolete("Legacy method for hardcoded test map. Use LinkRoomsInSequence instead.")]
    private void LinkRooms(Dictionary<Coordinate, Room> rooms)
    {
        _logger.LogDebug("Linking rooms together...");

        // Entry Hall connections
        var entry = rooms[new Coordinate(0, 0, 0)];
        var corridor = rooms[new Coordinate(0, 1, 0)];
        var storage = rooms[new Coordinate(1, 0, 0)];
        var collapsed = rooms[new Coordinate(-1, 0, 0)];
        var pit = rooms[new Coordinate(0, 0, -1)];

        // Entry <-> Corridor (North/South)
        entry.Exits[Direction.North] = corridor.Id;
        corridor.Exits[Direction.South] = entry.Id;

        // Entry <-> Storage (East/West)
        entry.Exits[Direction.East] = storage.Id;
        storage.Exits[Direction.West] = entry.Id;

        // Entry <-> Collapsed (West/East)
        entry.Exits[Direction.West] = collapsed.Id;
        collapsed.Exits[Direction.East] = entry.Id;

        // Entry <-> Pit (Down/Up)
        entry.Exits[Direction.Down] = pit.Id;
        pit.Exits[Direction.Up] = entry.Id;

        _logger.LogDebug("Room linking complete. Entry hall has {ExitCount} exits.", entry.Exits.Count);
    }

    /// <summary>
    /// Gets the opposite direction for bidirectional linking.
    /// </summary>
    /// <param name="direction">The original direction.</param>
    /// <returns>The opposite direction.</returns>
    public static Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }

    #endregion
}
