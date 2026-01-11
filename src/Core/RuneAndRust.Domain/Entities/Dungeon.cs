using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a dungeon containing interconnected rooms that the player can explore.
/// </summary>
/// <remarks>
/// The Dungeon class serves as a container for rooms and manages their connections.
/// It provides methods for adding rooms, connecting them via exits, and querying
/// rooms by various criteria. Each dungeon has a designated starting room where
/// new game sessions begin.
/// </remarks>
public class Dungeon : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this dungeon.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the display name of this dungeon.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The internal dictionary mapping room IDs to room instances.
    /// </summary>
    private readonly Dictionary<Guid, Room> _rooms = [];

    /// <summary>
    /// Tracks vertical connection stair types between room pairs.
    /// Key is (upperRoomId, lowerRoomId) tuple.
    /// </summary>
    private readonly Dictionary<(Guid, Guid), StairType> _verticalConnections = [];

    /// <summary>
    /// Gets the unique identifier of the room where players start their adventure.
    /// </summary>
    public Guid StartingRoomId { get; private set; }

    /// <summary>
    /// Gets a read-only dictionary of all rooms in this dungeon, keyed by their IDs.
    /// </summary>
    public IReadOnlyDictionary<Guid, Room> Rooms => _rooms.AsReadOnly();

    /// <summary>
    /// Gets the total number of rooms in this dungeon.
    /// </summary>
    public int RoomCount => _rooms.Count;

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Dungeon()
    {
        Name = null!;
    }

    /// <summary>
    /// Creates a new dungeon with the specified name.
    /// </summary>
    /// <param name="name">The display name for the dungeon.</param>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    public Dungeon(string name)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Adds a room to the dungeon.
    /// </summary>
    /// <param name="room">The room to add.</param>
    /// <param name="isStartingRoom">If true, this room becomes the starting room for new sessions.</param>
    /// <exception cref="ArgumentNullException">Thrown when room is null.</exception>
    /// <remarks>
    /// If this is the first room added to the dungeon, it automatically becomes
    /// the starting room regardless of the <paramref name="isStartingRoom"/> parameter.
    /// </remarks>
    public void AddRoom(Room room, bool isStartingRoom = false)
    {
        if (room == null)
            throw new ArgumentNullException(nameof(room));

        _rooms[room.Id] = room;

        if (isStartingRoom || _rooms.Count == 1)
            StartingRoomId = room.Id;
    }

    /// <summary>
    /// Retrieves a room by its unique identifier.
    /// </summary>
    /// <param name="roomId">The ID of the room to retrieve.</param>
    /// <returns>The room if found; otherwise, <c>null</c>.</returns>
    public Room? GetRoom(Guid roomId) =>
        _rooms.TryGetValue(roomId, out var room) ? room : null;

    /// <summary>
    /// Retrieves the starting room for this dungeon.
    /// </summary>
    /// <returns>The starting room if it exists; otherwise, <c>null</c>.</returns>
    public Room? GetStartingRoom() => GetRoom(StartingRoomId);

    /// <summary>
    /// Retrieves a room by its 2D grid position (any Z level).
    /// </summary>
    /// <param name="position">The 2D position to search for.</param>
    /// <returns>The first room at the specified X,Y position if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// This method ignores the Z coordinate and returns the first matching room.
    /// Use <see cref="GetRoomByPosition(Position3D)"/> for exact 3D position matching.
    /// </remarks>
    [Obsolete("Use GetRoomByPosition(Position3D) for exact 3D position matching.")]
    public Room? GetRoomByPosition(Position position) =>
        _rooms.Values.FirstOrDefault(r => r.Position.X == position.X && r.Position.Y == position.Y);

    /// <summary>
    /// Retrieves a room by its exact 3D grid position.
    /// </summary>
    /// <param name="position">The 3D position to search for.</param>
    /// <returns>The room at the specified position if found; otherwise, <c>null</c>.</returns>
    public Room? GetRoomByPosition(Position3D position) =>
        _rooms.Values.FirstOrDefault(r => r.Position.Equals(position));

    /// <summary>
    /// Checks if a room exists at the specified 3D position.
    /// </summary>
    /// <param name="position">The 3D position to check.</param>
    /// <returns><c>true</c> if a room exists at the position; otherwise, <c>false</c>.</returns>
    public bool HasRoomAt(Position3D position) =>
        _rooms.Values.Any(r => r.Position.Equals(position));

    /// <summary>
    /// Maximum rooms allowed per dungeon level (Z).
    /// Used by procedural generation to limit room count.
    /// </summary>
    public int MaxRoomsPerLevel { get; set; } = 50;

    /// <summary>
    /// Checks if generation is allowed at the specified position.
    /// </summary>
    /// <param name="position">The target position.</param>
    /// <returns>True if room generation is allowed.</returns>
    /// <remarks>
    /// Returns false if a room already exists at the position or if
    /// the room limit for that level has been reached.
    /// </remarks>
    public bool CanGenerateAt(Position3D position)
    {
        // Check if room already exists
        if (HasRoomAt(position)) return false;

        // Check max rooms per level
        var roomsOnLevel = GetRoomCountAtLevel(position.Z);
        return roomsOnLevel < MaxRoomsPerLevel;
    }

    /// <summary>
    /// Gets or creates a room at the specified position.
    /// </summary>
    /// <param name="position">The 3D position for the room.</param>
    /// <param name="roomFactory">Factory function to create the room if it doesn't exist.</param>
    /// <returns>The existing or newly created room, or null if generation not allowed.</returns>
    /// <remarks>
    /// This method is called by the RoomGeneratorService during on-demand generation.
    /// </remarks>
    public Room? GetOrAddRoom(Position3D position, Func<Room> roomFactory)
    {
        ArgumentNullException.ThrowIfNull(roomFactory);

        // Return existing room if present
        var existingRoom = GetRoomByPosition(position);
        if (existingRoom != null)
        {
            return existingRoom;
        }

        // Check if generation is allowed
        if (!CanGenerateAt(position))
        {
            return null;
        }

        // Create and add the new room
        var room = roomFactory();
        AddRoom(room);
        return room;
    }

    /// <summary>
    /// Gets the count of rooms at a specific Z level.
    /// </summary>
    /// <param name="level">The Z-level to count rooms for.</param>
    /// <returns>The number of rooms at the specified level.</returns>
    public int GetRoomCountAtLevel(int level) =>
        _rooms.Values.Count(r => r.Position.Z == level);


    /// <summary>
    /// Creates bidirectional connections between two rooms.
    /// </summary>
    /// <param name="fromRoomId">The ID of the first room.</param>
    /// <param name="direction">The direction from the first room to the second.</param>
    /// <param name="toRoomId">The ID of the second room.</param>
    /// <exception cref="ArgumentException">Thrown when either room ID is not found in the dungeon.</exception>
    /// <remarks>
    /// This method creates exits in both directions: an exit from the first room
    /// to the second in the specified direction, and a reverse exit from the
    /// second room back to the first.
    /// </remarks>
    public void ConnectRooms(Guid fromRoomId, Direction direction, Guid toRoomId)
    {
        if (!_rooms.ContainsKey(fromRoomId))
            throw new ArgumentException($"Room {fromRoomId} not found", nameof(fromRoomId));
        if (!_rooms.ContainsKey(toRoomId))
            throw new ArgumentException($"Room {toRoomId} not found", nameof(toRoomId));

        _rooms[fromRoomId].AddExit(direction, toRoomId);

        // Add reverse connection
        var reverseDirection = GetOppositeDirection(direction);
        _rooms[toRoomId].AddExit(reverseDirection, fromRoomId);
    }

    /// <summary>
    /// Gets the opposite direction.
    /// </summary>
    /// <param name="direction">The direction to reverse.</param>
    /// <returns>The opposite direction (North↔South, East↔West, Up↔Down).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when direction is not valid.</exception>
    public static Direction GetOppositeDirection(Direction direction) => direction switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction")
    };

    /// <summary>
    /// Creates a vertical connection between two rooms on different levels.
    /// </summary>
    /// <param name="upperRoomId">The room at the higher level (lower Z value).</param>
    /// <param name="lowerRoomId">The room at the lower level (higher Z value).</param>
    /// <param name="stairType">The type of vertical connection.</param>
    /// <exception cref="ArgumentException">Thrown when rooms are not found or have invalid Z relationship.</exception>
    /// <remarks>
    /// Creates a Down exit from the upper room to the lower room. For bidirectional
    /// stair types, also creates an Up exit from the lower room to the upper room.
    /// Pit connections are one-way (down only).
    /// </remarks>
    public void ConnectRoomsVertically(Guid upperRoomId, Guid lowerRoomId, StairType stairType)
    {
        if (!_rooms.TryGetValue(upperRoomId, out var upperRoom))
            throw new ArgumentException($"Upper room {upperRoomId} not found", nameof(upperRoomId));
        if (!_rooms.TryGetValue(lowerRoomId, out var lowerRoom))
            throw new ArgumentException($"Lower room {lowerRoomId} not found", nameof(lowerRoomId));

        // Validate Z relationship (upper should have lower Z value)
        if (upperRoom.Position.Z >= lowerRoom.Position.Z)
            throw new ArgumentException(
                $"Upper room Z ({upperRoom.Position.Z}) must be less than lower room Z ({lowerRoom.Position.Z})");

        // Add down exit from upper to lower
        upperRoom.AddExit(Direction.Down, lowerRoomId);

        // Add up exit from lower to upper (unless it's a pit - one way down)
        if (stairType != StairType.Pit)
        {
            lowerRoom.AddExit(Direction.Up, upperRoomId);
        }

        // Store the stair type for flavor text
        _verticalConnections[(upperRoomId, lowerRoomId)] = stairType;
    }

    /// <summary>
    /// Gets the stair type for a vertical connection between two rooms.
    /// </summary>
    /// <param name="fromRoomId">The starting room ID.</param>
    /// <param name="toRoomId">The destination room ID.</param>
    /// <returns>The stair type if a vertical connection exists; otherwise, <c>null</c>.</returns>
    public StairType? GetStairType(Guid fromRoomId, Guid toRoomId)
    {
        // Check both orderings since connection could be up or down
        if (_verticalConnections.TryGetValue((fromRoomId, toRoomId), out var type1))
            return type1;
        if (_verticalConnections.TryGetValue((toRoomId, fromRoomId), out var type2))
            return type2;
        return null;
    }

    /// <summary>
    /// Factory method that creates a pre-configured starter dungeon for new games.
    /// </summary>
    /// <returns>A new <see cref="Dungeon"/> with rooms across two levels, items, and a monster.</returns>
    /// <remarks>
    /// The starter dungeon "The Forgotten Depths" contains:
    /// <para><b>Level 0 (Surface):</b></para>
    /// <list type="bullet">
    /// <item>Entrance Hall (starting room) at position (0, 0, 0)</item>
    /// <item>Armory with a sword at position (0, 1, 0)</item>
    /// <item>Ancient Library with a scroll at position (1, 0, 0)</item>
    /// <item>Storage Room with a health potion at position (-1, 0, 0)</item>
    /// <item>Dungeon Passage with a goblin at position (0, 2, 0)</item>
    /// </list>
    /// <para><b>Level 1 (Underground):</b></para>
    /// <list type="bullet">
    /// <item>Forgotten Crypt at position (0, 2, 1) - connected via stairs from Dungeon Passage</item>
    /// </list>
    /// </remarks>
    public static Dungeon CreateStarterDungeon()
    {
        var dungeon = new Dungeon("The Forgotten Depths");

        // ===== Level 0 (Surface) =====
        var entrance = new Room(
            "Entrance Hall",
            "A dimly lit hall with ancient stone walls. Cobwebs hang from the ceiling, and the air smells of dust and decay. Faded torches line the walls, barely providing enough light to see.",
            new Position3D(0, 0, 0)
        );

        var armory = new Room(
            "Armory",
            "Weapon racks line the walls, most of them empty. A few rusty weapons remain scattered on the floor. An old armor stand stands in the corner, its occupant long gone.",
            new Position3D(0, 1, 0)
        );
        armory.AddItem(Item.CreateSword());

        var library = new Room(
            "Ancient Library",
            "Towering bookshelves reach toward the shadowy ceiling. Most books have crumbled to dust, but a few ancient tomes remain. A reading desk sits in the center, covered in dust.",
            new Position3D(1, 0, 0)
        );
        library.AddItem(Item.CreateScroll());

        var storage = new Room(
            "Storage Room",
            "Crates and barrels fill this cluttered room. Most contain nothing but rotten supplies, but some treasures might remain hidden among the debris.",
            new Position3D(-1, 0, 0)
        );
        storage.AddItem(Item.CreateHealthPotion());

        var passage = new Room(
            "Dungeon Passage",
            "A narrow corridor stretches into darkness. Strange scratching sounds echo from deeper within. The walls are covered in mysterious runes that seem to pulse with faint light. A worn stone staircase spirals downward into the depths.",
            new Position3D(0, 2, 0)
        );
        // TODO: Refactor to use IMonsterService when DI is available in factory methods
#pragma warning disable CS0618 // Type or member is obsolete
        passage.AddMonster(Monster.CreateGoblin());
#pragma warning restore CS0618

        // ===== Level 1 (Underground) =====
        var crypt = new Room(
            "Forgotten Crypt",
            "Ancient sarcophagi line the walls of this cold, damp chamber. The air is thick with the musty scent of centuries-old decay. Faded inscriptions on the stone walls speak of warriors long forgotten.",
            new Position3D(0, 2, 1)
        );

        // Add rooms to dungeon
        dungeon.AddRoom(entrance, isStartingRoom: true);
        dungeon.AddRoom(armory);
        dungeon.AddRoom(library);
        dungeon.AddRoom(storage);
        dungeon.AddRoom(passage);
        dungeon.AddRoom(crypt);

        // Connect Level 0 rooms horizontally
        dungeon.ConnectRooms(entrance.Id, Direction.North, armory.Id);
        dungeon.ConnectRooms(entrance.Id, Direction.East, library.Id);
        dungeon.ConnectRooms(entrance.Id, Direction.West, storage.Id);
        dungeon.ConnectRooms(armory.Id, Direction.North, passage.Id);

        // Connect Level 0 to Level 1 vertically
        dungeon.ConnectRoomsVertically(passage.Id, crypt.Id, StairType.SpiralStairs);

        return dungeon;
    }

    /// <summary>
    /// Returns a string representation of this dungeon.
    /// </summary>
    /// <returns>The dungeon's name.</returns>
    public override string ToString() => Name;
}
