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
    /// Gets the 3D position of this room in the dungeon grid.
    /// </summary>
    public Position3D Position { get; private set; }

    /// <summary>
    /// Dictionary mapping directions to Exit value objects.
    /// </summary>
    private readonly Dictionary<Direction, Exit> _exits = [];

    /// <summary>
    /// List of hidden items that require discovery.
    /// </summary>
    private readonly List<HiddenItem> _hiddenItems = [];

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
    /// Gets a read-only dictionary of all exits from this room.
    /// </summary>
    /// <remarks>
    /// Includes both visible and hidden exits. Use GetVisibleExits()
    /// to get only exits the player can currently see.
    /// </remarks>
    public IReadOnlyDictionary<Direction, Exit> Exits => _exits.AsReadOnly();

    /// <summary>
    /// Gets a read-only list of hidden items in this room.
    /// </summary>
    public IReadOnlyList<HiddenItem> HiddenItems => _hiddenItems.AsReadOnly();

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

    // ===== Room Type (v0.1.0c) =====

    /// <summary>
    /// Gets the type of this room.
    /// </summary>
    /// <remarks>
    /// Room type affects monster spawning, loot, and available interactions.
    /// </remarks>
    public RoomType RoomType { get; private set; } = RoomType.Standard;

    // ===== Environment Context (v0.0.11a) =====

    /// <summary>
    /// Gets the environment context for this room.
    /// </summary>
    /// <remarks>
    /// Environment context controls which descriptors are selected for
    /// atmosphere generation and ensures environmental coherence.
    /// </remarks>
    public EnvironmentContext? Environment { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Room()
    {
        Name = null!;
        Description = null!;
    }

    /// <summary>
    /// Creates a new room with the specified name, description, and 3D position.
    /// </summary>
    /// <param name="name">The display name of the room.</param>
    /// <param name="description">The narrative description shown to players.</param>
    /// <param name="position">The 3D position of this room in the dungeon grid.</param>
    /// <exception cref="ArgumentNullException">Thrown when name or description is null.</exception>
    public Room(string name, string description, Position3D position)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Position = position;
    }

    /// <summary>
    /// Creates a new room with the specified name, description, and 2D position (Z defaults to 0).
    /// </summary>
    /// <remarks>
    /// This constructor is provided for backwards compatibility with existing code
    /// that uses 2D positions. The Z coordinate defaults to 0 (surface level).
    /// </remarks>
    [Obsolete("Use the Position3D constructor for new code. This exists for backwards compatibility.")]
    public Room(string name, string description, Position position)
        : this(name, description, Position3D.FromPosition2D(position))
    {
    }

    /// <summary>
    /// Sets the room type.
    /// </summary>
    /// <param name="roomType">The type to set.</param>
    public void SetRoomType(RoomType roomType)
    {
        RoomType = roomType;
    }

    /// <summary>
    /// Adds or updates an exit from this room.
    /// </summary>
    /// <param name="direction">The direction of the exit.</param>
    /// <param name="exit">The exit value object.</param>
    public void AddExit(Direction direction, Exit exit)
    {
        _exits[direction] = exit;
    }

    /// <summary>
    /// Adds a standard (non-hidden) exit to this room.
    /// </summary>
    /// <param name="direction">The direction of the exit.</param>
    /// <param name="targetRoomId">The ID of the target room.</param>
    public void AddExit(Direction direction, Guid targetRoomId)
    {
        _exits[direction] = Exit.Standard(targetRoomId);
    }

    /// <summary>
    /// Adds a hidden exit that requires discovery.
    /// </summary>
    /// <param name="direction">The direction of the exit.</param>
    /// <param name="targetRoomId">The ID of the target room.</param>
    /// <param name="discoveryDC">The difficulty class for discovery.</param>
    /// <param name="hint">Optional hint for the hidden passage.</param>
    public void AddHiddenExit(Direction direction, Guid targetRoomId, int discoveryDC, string? hint = null)
    {
        _exits[direction] = Exit.Hidden(targetRoomId, discoveryDC, hint);
    }

    /// <summary>
    /// Checks if there is any exit in the specified direction (visible or hidden).
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns><c>true</c> if an exit exists in that direction; otherwise, <c>false</c>.</returns>
    public bool HasExit(Direction direction) => _exits.ContainsKey(direction);

    /// <summary>
    /// Checks if there is a visible exit in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns>True if a visible exit exists in that direction.</returns>
    public bool HasVisibleExit(Direction direction)
    {
        return _exits.TryGetValue(direction, out var exit) && exit.IsVisible;
    }

    /// <summary>
    /// Adds a potential exit that may lead to a generated or existing room.
    /// </summary>
    /// <param name="direction">The direction of the potential exit.</param>
    /// <remarks>
    /// Potential exits use Guid.Empty as a placeholder indicating
    /// the exit exists but leads to an unexplored (not yet generated) room.
    /// </remarks>
    public void AddPotentialExit(Direction direction)
    {
        if (!_exits.ContainsKey(direction))
        {
            _exits[direction] = Exit.Standard(Guid.Empty);
        }
    }

    /// <summary>
    /// Checks if an exit leads to an unexplored (not yet generated) room.
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns>True if the exit is unexplored (Guid.Empty target); false otherwise.</returns>
    public bool IsExitUnexplored(Direction direction) =>
        _exits.TryGetValue(direction, out var exit) && exit.TargetRoomId == Guid.Empty;


    /// <summary>
    /// Gets the room ID for a visible exit in the specified direction.
    /// </summary>
    /// <param name="direction">The direction of the exit.</param>
    /// <returns>The target room ID if a visible exit exists; otherwise, null.</returns>
    public Guid? GetExit(Direction direction)
    {
        if (_exits.TryGetValue(direction, out var exit) && exit.IsVisible)
            return exit.TargetRoomId;
        return null;
    }

    /// <summary>
    /// Gets the raw exit in the specified direction (visible or hidden).
    /// </summary>
    /// <param name="direction">The direction of the exit.</param>
    /// <returns>The exit if one exists; otherwise, null.</returns>
    public Exit? GetExitRaw(Direction direction) =>
        _exits.TryGetValue(direction, out var exit) ? exit : null;

    /// <summary>
    /// Gets only exits that are visible to the player.
    /// </summary>
    /// <returns>Dictionary of visible exits (not hidden or already discovered).</returns>
    public IReadOnlyDictionary<Direction, Exit> GetVisibleExits()
    {
        return _exits
            .Where(kvp => kvp.Value.IsVisible)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .AsReadOnly();
    }

    /// <summary>
    /// Gets hidden exits that have not yet been discovered.
    /// </summary>
    /// <returns>Dictionary of undiscovered hidden exits.</returns>
    public IReadOnlyDictionary<Direction, Exit> GetHiddenExits()
    {
        return _exits
            .Where(kvp => kvp.Value.IsHidden && !kvp.Value.IsDiscovered)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .AsReadOnly();
    }

    /// <summary>
    /// Reveals a hidden exit, marking it as discovered.
    /// </summary>
    /// <param name="direction">The direction of the exit to reveal.</param>
    /// <returns>True if the exit was revealed; false if not found or already visible.</returns>
    public bool RevealExit(Direction direction)
    {
        if (!_exits.TryGetValue(direction, out var exit))
            return false;

        if (!exit.IsHidden || exit.IsDiscovered)
            return false;

        _exits[direction] = exit.AsDiscovered();
        return true;
    }

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
    /// Gets a human-readable description of visible exits from this room.
    /// </summary>
    /// <returns>A string describing available exits, including vertical directions.</returns>
    public string GetExitsDescription()
    {
        var visibleExits = GetVisibleExits();

        if (visibleExits.Count == 0)
            return "There are no visible exits.";

        var horizontalExits = visibleExits.Keys
            .Where(d => d is Direction.North or Direction.South or Direction.East or Direction.West)
            .Select(d => d.ToString().ToLower());

        var verticalExits = visibleExits.Keys
            .Where(d => d is Direction.Up or Direction.Down)
            .Select(d => d == Direction.Up ? "up" : "down");

        var allExits = horizontalExits.Concat(verticalExits).ToList();
        return $"Exits: {string.Join(", ", allExits)}";
    }

    /// <summary>
    /// Gets a room type indicator for display.
    /// </summary>
    /// <returns>A short string indicating room type, or empty for Standard.</returns>
    public string GetRoomTypeIndicator() => RoomType switch
    {
        RoomType.Treasure => "[Treasure Room]",
        RoomType.Trap => "[Trap Room]",
        RoomType.Boss => "[Boss Chamber]",
        RoomType.Safe => "[Safe Haven]",
        RoomType.Shrine => "[Shrine]",
        _ => string.Empty
    };

    // ===== Hidden Item Methods (v0.1.0c) =====

    /// <summary>
    /// Adds a hidden item to this room.
    /// </summary>
    /// <param name="hiddenItem">The hidden item to add.</param>
    public void AddHiddenItem(HiddenItem hiddenItem)
    {
        ArgumentNullException.ThrowIfNull(hiddenItem);
        _hiddenItems.Add(hiddenItem);
    }

    /// <summary>
    /// Reveals a hidden item, moving it to the regular items list.
    /// </summary>
    /// <param name="hiddenItemId">The ID of the hidden item to reveal.</param>
    /// <returns>The revealed item, or null if not found.</returns>
    public Item? RevealHiddenItem(Guid hiddenItemId)
    {
        var hiddenItem = _hiddenItems.FirstOrDefault(h => h.Id == hiddenItemId);
        if (hiddenItem == null) return null;

        _hiddenItems.Remove(hiddenItem);
        _items.Add(hiddenItem.Item);
        return hiddenItem.Item;
    }

    /// <summary>
    /// Gets undiscovered hidden items.
    /// </summary>
    public IReadOnlyList<HiddenItem> GetUndiscoveredHiddenItems() =>
        _hiddenItems.Where(h => !h.IsDiscovered).ToList().AsReadOnly();

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

    // ===== Environment Context Methods (v0.0.11a) =====

    /// <summary>
    /// Sets the environment context for this room.
    /// </summary>
    /// <param name="environment">The environment context to set.</param>
    public void SetEnvironment(EnvironmentContext environment)
    {
        Environment = environment;
    }

    /// <summary>
    /// Clears the environment context for this room.
    /// </summary>
    public void ClearEnvironment()
    {
        Environment = null;
    }

    /// <summary>
    /// Returns the name of this room.
    /// </summary>
    /// <returns>The room name.</returns>
    public override string ToString() => Name;
}
