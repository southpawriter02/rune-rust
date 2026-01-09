using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a room in the dungeon that can contain items, monsters, and exits to other rooms.
/// </summary>
/// <remarks>
/// Rooms are the fundamental building blocks of the dungeon. Each room has a unique position,
/// can be connected to other rooms via directional exits, and may contain items and monsters
/// for the player to interact with.
/// </remarks>
public class Room : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this room.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the display name of this room.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the narrative description of this room shown to the player.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the position of this room in the dungeon grid.
    /// </summary>
    public Position Position { get; private set; }

    /// <summary>
    /// Dictionary mapping directions to connected room IDs.
    /// </summary>
    private readonly Dictionary<Direction, Guid> _exits = [];

    /// <summary>
    /// List of items present in this room.
    /// </summary>
    private readonly List<Item> _items = [];

    /// <summary>
    /// List of monsters present in this room.
    /// </summary>
    private readonly List<Monster> _monsters = [];

    // ===== Dropped Loot Fields (v0.0.9d) =====

    /// <summary>
    /// List of items dropped as loot in this room.
    /// </summary>
    private readonly List<DroppedItem> _droppedItems = [];

    /// <summary>
    /// Dictionary of currency dropped in this room.
    /// </summary>
    private readonly Dictionary<string, int> _droppedCurrency = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a read-only dictionary of exits from this room.
    /// </summary>
    public IReadOnlyDictionary<Direction, Guid> Exits => _exits.AsReadOnly();

    /// <summary>
    /// Gets a read-only list of items in this room.
    /// </summary>
    public IReadOnlyList<Item> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets a read-only list of monsters in this room.
    /// </summary>
    public IReadOnlyList<Monster> Monsters => _monsters.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this room has any living monsters.
    /// </summary>
    public bool HasMonsters => _monsters.Any(m => m.IsAlive);

    /// <summary>
    /// Gets a value indicating whether this room has any items.
    /// </summary>
    public bool HasItems => _items.Count > 0;

    // ===== Dropped Loot Properties (v0.0.9d) =====

    /// <summary>
    /// Gets a read-only list of dropped items in this room.
    /// </summary>
    public IReadOnlyList<DroppedItem> DroppedItems => _droppedItems.AsReadOnly();

    /// <summary>
    /// Gets a read-only dictionary of dropped currency in this room.
    /// </summary>
    public IReadOnlyDictionary<string, int> DroppedCurrency => _droppedCurrency.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this room has any dropped loot.
    /// </summary>
    public bool HasDroppedLoot => _droppedItems.Count > 0 || _droppedCurrency.Count > 0;

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Room()
    {
        Name = null!;
        Description = null!;
    }

    /// <summary>
    /// Creates a new room with the specified name, description, and position.
    /// </summary>
    /// <param name="name">The display name of the room.</param>
    /// <param name="description">The narrative description shown to players.</param>
    /// <param name="position">The position of this room in the dungeon grid.</param>
    /// <exception cref="ArgumentNullException">Thrown when name or description is null.</exception>
    public Room(string name, string description, Position position)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Position = position;
    }

    /// <summary>
    /// Adds or updates an exit from this room in the specified direction.
    /// </summary>
    /// <param name="direction">The direction of the exit.</param>
    /// <param name="roomId">The ID of the room this exit leads to.</param>
    public void AddExit(Direction direction, Guid roomId)
    {
        _exits[direction] = roomId;
    }

    /// <summary>
    /// Checks if there is an exit in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns><c>true</c> if an exit exists in that direction; otherwise, <c>false</c>.</returns>
    public bool HasExit(Direction direction) => _exits.ContainsKey(direction);

    /// <summary>
    /// Gets the room ID that the exit in the specified direction leads to.
    /// </summary>
    /// <param name="direction">The direction of the exit.</param>
    /// <returns>The room ID if an exit exists; otherwise, <c>null</c>.</returns>
    public Guid? GetExit(Direction direction) =>
        _exits.TryGetValue(direction, out var roomId) ? roomId : null;

    /// <summary>
    /// Adds an item to this room.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public void AddItem(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    /// <summary>
    /// Removes an item from this room.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the item was found and removed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public bool RemoveItem(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        return _items.Remove(item);
    }

    /// <summary>
    /// Finds an item in this room by name (case-insensitive).
    /// </summary>
    /// <param name="name">The name of the item to find.</param>
    /// <returns>The item if found; otherwise, <c>null</c>.</returns>
    public Item? GetItemByName(string name) =>
        _items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Adds a monster to this room.
    /// </summary>
    /// <param name="monster">The monster to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when monster is null.</exception>
    public void AddMonster(Monster monster)
    {
        if (monster == null)
            throw new ArgumentNullException(nameof(monster));
        _monsters.Add(monster);
    }

    /// <summary>
    /// Gets all living monsters in this room.
    /// </summary>
    /// <returns>An enumerable of monsters that are still alive.</returns>
    public IEnumerable<Monster> GetAliveMonsters() => _monsters.Where(m => m.IsAlive);

    /// <summary>
    /// Gets a human-readable description of the exits from this room.
    /// </summary>
    /// <returns>A string describing available exits, or a message if no exits exist.</returns>
    public string GetExitsDescription()
    {
        if (_exits.Count == 0)
            return "There are no visible exits.";

        var directions = _exits.Keys.Select(d => d.ToString().ToLower());
        return $"Exits: {string.Join(", ", directions)}";
    }

    // ===== Dropped Loot Methods (v0.0.9d) =====

    /// <summary>
    /// Adds loot to this room from a loot drop.
    /// </summary>
    /// <param name="loot">The loot drop to add.</param>
    public void AddLoot(LootDrop loot)
    {
        if (loot.IsEmpty) return;

        if (loot.HasItems)
        {
            foreach (var item in loot.Items)
            {
                _droppedItems.Add(item);
            }
        }

        if (loot.HasCurrency)
        {
            foreach (var kvp in loot.Currency)
            {
                if (_droppedCurrency.TryGetValue(kvp.Key, out var existing))
                {
                    _droppedCurrency[kvp.Key] = existing + kvp.Value;
                }
                else
                {
                    _droppedCurrency[kvp.Key] = kvp.Value;
                }
            }
        }
    }

    /// <summary>
    /// Collects all dropped loot from this room.
    /// </summary>
    /// <returns>A LootDrop containing all collected loot.</returns>
    public LootDrop CollectAllLoot()
    {
        if (!HasDroppedLoot)
        {
            return LootDrop.Empty;
        }

        var items = _droppedItems.ToList();
        var currency = new Dictionary<string, int>(_droppedCurrency, StringComparer.OrdinalIgnoreCase);

        ClearDroppedLoot();

        return LootDrop.Create(items, currency);
    }

    /// <summary>
    /// Clears all dropped loot from this room.
    /// </summary>
    public void ClearDroppedLoot()
    {
        _droppedItems.Clear();
        _droppedCurrency.Clear();
    }

    /// <summary>
    /// Returns the name of this room.
    /// </summary>
    /// <returns>The room name.</returns>
    public override string ToString() => Name;
}
