using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

public class Dungeon : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private readonly Dictionary<Guid, Room> _rooms = [];
    public Guid StartingRoomId { get; private set; }

    public IReadOnlyDictionary<Guid, Room> Rooms => _rooms.AsReadOnly();
    public int RoomCount => _rooms.Count;

    private Dungeon() { } // For EF Core

    public Dungeon(string name)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void AddRoom(Room room, bool isStartingRoom = false)
    {
        if (room == null)
            throw new ArgumentNullException(nameof(room));

        _rooms[room.Id] = room;

        if (isStartingRoom || _rooms.Count == 1)
            StartingRoomId = room.Id;
    }

    public Room? GetRoom(Guid roomId) =>
        _rooms.TryGetValue(roomId, out var room) ? room : null;

    public Room? GetStartingRoom() => GetRoom(StartingRoomId);

    public Room? GetRoomByPosition(Position position) =>
        _rooms.Values.FirstOrDefault(r => r.Position.Equals(position));

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

    private static Direction GetOppositeDirection(Direction direction) => direction switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };

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
        passage.AddMonster(Monster.CreateGoblin());

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

    public override string ToString() => Name;
}
