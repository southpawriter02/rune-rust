using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Generates dungeon layouts by creating connected room graphs.
/// For v0.0.5, provides a simple test map. Future versions will implement
/// procedural generation algorithms (e.g., Wave Function Collapse).
/// </summary>
public class DungeonGenerator
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<DungeonGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DungeonGenerator"/> class.
    /// </summary>
    /// <param name="roomRepository">The room repository for persistence.</param>
    /// <param name="logger">The logger instance.</param>
    public DungeonGenerator(IRoomRepository roomRepository, ILogger<DungeonGenerator> logger)
    {
        _roomRepository = roomRepository;
        _logger = logger;
    }

    /// <summary>
    /// Generates and persists a simple test map with connected rooms.
    /// Used to verify navigation logic before implementing procedural generation.
    /// </summary>
    /// <returns>The ID of the starting room.</returns>
    public async Task<Guid> GenerateTestMapAsync()
    {
        _logger.LogInformation("Generating test dungeon map...");

        // Clear existing rooms for a fresh start
        await _roomRepository.ClearAllRoomsAsync();

        // Create the test rooms
        var rooms = CreateTestRooms();

        // Link the rooms together
        LinkRooms(rooms);

        // Persist to database
        await _roomRepository.AddRangeAsync(rooms.Values);
        await _roomRepository.SaveChangesAsync();

        var startingRoom = rooms.Values.First(r => r.IsStartingRoom);
        _logger.LogInformation("Generated dungeon with {Count} rooms. Starting room: {RoomName} ({RoomId})",
            rooms.Count, startingRoom.Name, startingRoom.Id);

        return startingRoom.Id;
    }

    /// <summary>
    /// Creates the individual room entities for the test map.
    /// </summary>
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
}
