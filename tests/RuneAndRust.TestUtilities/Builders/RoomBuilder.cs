using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.TestUtilities.Builders;

/// <summary>
/// Fluent builder for creating test Room instances.
/// </summary>
public class RoomBuilder
{
    private Guid? _id;
    private string _name = "Test Room";
    private string _description = "A room for testing.";
    private Position3D _position = Position3D.Origin;
    private readonly Dictionary<Direction, Guid> _exits = new();
    private readonly List<Monster> _monsters = [];
    private readonly List<Item> _items = [];

    /// <summary>
    /// Creates a new RoomBuilder with default values.
    /// </summary>
    public static RoomBuilder Create() => new();

    /// <summary>
    /// Sets the room ID.
    /// </summary>
    public RoomBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the room name.
    /// </summary>
    public RoomBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the room description.
    /// </summary>
    public RoomBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the room position on the dungeon grid (Z defaults to 0).
    /// </summary>
    public RoomBuilder AtPosition(int x, int y, int z = 0)
    {
        _position = new Position3D(x, y, z);
        return this;
    }

    /// <summary>
    /// Adds an exit in the specified direction.
    /// </summary>
    public RoomBuilder WithExit(Direction direction, Guid targetRoomId)
    {
        _exits[direction] = targetRoomId;
        return this;
    }

    /// <summary>
    /// Adds a monster to the room.
    /// </summary>
    public RoomBuilder WithMonster(Monster monster)
    {
        _monsters.Add(monster);
        return this;
    }

    /// <summary>
    /// Adds a monster using a MonsterBuilder.
    /// </summary>
    public RoomBuilder WithMonster(MonsterBuilder builder)
    {
        _monsters.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Adds an item to the room.
    /// </summary>
    public RoomBuilder WithItem(Item item)
    {
        _items.Add(item);
        return this;
    }

    /// <summary>
    /// Builds the Room instance.
    /// </summary>
    public Room Build()
    {
        var room = new Room(_name, _description, _position);

        // Note: Room generates its own ID in constructor, so we can't set a custom ID
        // The _id field is kept for potential future use but currently unused

        foreach (var (direction, targetRoomId) in _exits)
        {
            room.AddExit(direction, targetRoomId);
        }

        foreach (var monster in _monsters)
        {
            room.AddMonster(monster);
        }

        foreach (var item in _items)
        {
            room.AddItem(item);
        }

        return room;
    }
}
