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
    /// Retrieves a room by its grid position.
    /// </summary>
    /// <param name="position">The position to search for.</param>
    /// <returns>The room at the specified position if found; otherwise, <c>null</c>.</returns>
    public Room? GetRoomByPosition(Position position) =>
        _rooms.Values.FirstOrDefault(r => r.Position.Equals(position));

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
    /// Gets the opposite cardinal direction.
    /// </summary>
    /// <param name="direction">The direction to reverse.</param>
    /// <returns>The opposite direction (North↔South, East↔West).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when direction is not a valid cardinal direction.</exception>
    private static Direction GetOppositeDirection(Direction direction) => direction switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };

    /// <summary>
    /// Factory method that creates a pre-configured starter dungeon for new games.
    /// </summary>
    /// <returns>A new <see cref="Dungeon"/> with five interconnected rooms, items, and a monster.</returns>
    /// <remarks>
    /// The starter dungeon "The Forgotten Depths" contains:
    /// <list type="bullet">
    /// <item>Entrance Hall (starting room) at position (0, 0)</item>
    /// <item>Armory with a sword at position (0, 1)</item>
    /// <item>Ancient Library with a scroll at position (1, 0)</item>
    /// <item>Storage Room with a health potion at position (-1, 0)</item>
    /// <item>Dungeon Passage with a goblin at position (0, 2)</item>
    /// </list>
    /// </remarks>
    public static Dungeon CreateStarterDungeon()
    {
        var dungeon = new Dungeon("The Forgotten Depths");

        // Create rooms
        var entrance = new Room(
            "Entrance Hall",
            "A dimly lit hall with ancient stone walls. Cobwebs hang from the ceiling, and the air smells of dust and decay. Faded torches line the walls, barely providing enough light to see.",
            new Position(0, 0)
        );

        var armory = new Room(
            "Armory",
            "Weapon racks line the walls, most of them empty. A few rusty weapons remain scattered on the floor. An old armor stand stands in the corner, its occupant long gone.",
            new Position(0, 1)
        );
        armory.AddItem(Item.CreateSword());

        var library = new Room(
            "Ancient Library",
            "Towering bookshelves reach toward the shadowy ceiling. Most books have crumbled to dust, but a few ancient tomes remain. A reading desk sits in the center, covered in dust.",
            new Position(1, 0)
        );
        library.AddItem(Item.CreateScroll());

        var storage = new Room(
            "Storage Room",
            "Crates and barrels fill this cluttered room. Most contain nothing but rotten supplies, but some treasures might remain hidden among the debris.",
            new Position(-1, 0)
        );
        storage.AddItem(Item.CreateHealthPotion());

        var passage = new Room(
            "Dungeon Passage",
            "A narrow corridor stretches into darkness. Strange scratching sounds echo from deeper within. The walls are covered in mysterious runes that seem to pulse with faint light.",
            new Position(0, 2)
        );
        // TODO: Refactor to use IMonsterService when DI is available in factory methods
#pragma warning disable CS0618 // Type or member is obsolete
        passage.AddMonster(Monster.CreateGoblin());
#pragma warning restore CS0618

        // Add rooms to dungeon
        dungeon.AddRoom(entrance, isStartingRoom: true);
        dungeon.AddRoom(armory);
        dungeon.AddRoom(library);
        dungeon.AddRoom(storage);
        dungeon.AddRoom(passage);

        // Connect rooms
        dungeon.ConnectRooms(entrance.Id, Direction.North, armory.Id);
        dungeon.ConnectRooms(entrance.Id, Direction.East, library.Id);
        dungeon.ConnectRooms(entrance.Id, Direction.West, storage.Id);
        dungeon.ConnectRooms(armory.Id, Direction.North, passage.Id);

        return dungeon;
    }

    /// <summary>
    /// Returns a string representation of this dungeon.
    /// </summary>
    /// <returns>The dungeon's name.</returns>
    public override string ToString() => Name;
}
