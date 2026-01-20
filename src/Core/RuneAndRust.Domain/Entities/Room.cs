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

    // ===== Interactive Objects (v0.4.0a) =====

    /// <summary>
    /// List of interactive objects in this room.
    /// </summary>
    private readonly List<InteractiveObject> _interactables = [];

    // ===== Hazard Zones (v0.4.1c) =====

    /// <summary>
    /// List of environmental hazard zones in this room.
    /// </summary>
    private readonly List<HazardZone> _hazardZones = [];

    // ===== Puzzles (v0.4.2a) =====

    /// <summary>
    /// List of puzzles in this room.
    /// </summary>
    private readonly List<Puzzle> _puzzles = [];

    // ===== Riddle NPCs (v0.4.2c) =====

    /// <summary>
    /// List of riddle NPCs in this room.
    /// </summary>
    private readonly List<RiddleNpc> _riddleNpcs = [];

    // ===== Light Sources (v0.4.3b) =====

    /// <summary>
    /// List of active light sources in this room.
    /// </summary>
    private readonly List<LightSource> _lightSources = [];

    // ===== Harvestable Features (v0.11.0b) =====

    /// <summary>
    /// List of harvestable features in this room.
    /// </summary>
    private readonly List<HarvestableFeature> _harvestableFeatures = [];

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

    // ===== Interactive Object Properties (v0.4.0a) =====

    /// <summary>
    /// Gets a read-only list of interactive objects in this room.
    /// </summary>
    public IReadOnlyList<InteractiveObject> Interactables => _interactables.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this room has any interactive objects.
    /// </summary>
    public bool HasInteractables => _interactables.Count > 0;

    // ===== Hazard Zone Properties (v0.4.1c) =====

    /// <summary>
    /// Gets a read-only list of hazard zones in this room.
    /// </summary>
    public IReadOnlyList<HazardZone> HazardZones => _hazardZones.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this room has any hazard zones.
    /// </summary>
    public bool HasHazards => _hazardZones.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this room has any active hazard zones.
    /// </summary>
    public bool HasActiveHazards => _hazardZones.Any(h => h.IsActive);

    // ===== Puzzle Properties (v0.4.2a) =====

    /// <summary>
    /// Gets a read-only list of puzzles in this room.
    /// </summary>
    public IReadOnlyList<Puzzle> Puzzles => _puzzles.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this room has any puzzles.
    /// </summary>
    public bool HasPuzzles => _puzzles.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this room has any unsolved puzzles.
    /// </summary>
    public bool HasUnsolvedPuzzles => _puzzles.Any(p => p.IsSolvable);

    // ===== Riddle NPC Properties (v0.4.2c) =====

    /// <summary>
    /// Gets a read-only list of riddle NPCs in this room.
    /// </summary>
    public IReadOnlyList<RiddleNpc> RiddleNpcs => _riddleNpcs.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this room has any riddle NPCs.
    /// </summary>
    public bool HasRiddleNpcs => _riddleNpcs.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this room has any unsolved riddle NPCs.
    /// </summary>
    public bool HasUnsolvedRiddleNpcs => _riddleNpcs.Any(n => !n.RiddleSolved);

    // ===== Light Level Properties (v0.4.3a) =====

    /// <summary>
    /// Gets the base light level of this room (from configuration).
    /// </summary>
    /// <remarks>
    /// This is the room's natural light level without active light sources.
    /// Active light sources (v0.4.3b) can raise this level.
    /// </remarks>
    public LightLevel BaseLightLevel { get; private set; } = LightLevel.Dim;

    /// <summary>
    /// Gets the current effective light level.
    /// </summary>
    /// <remarks>
    /// Computed from base level and any active light sources.
    /// In v0.4.3a, this returns BaseLightLevel.
    /// In v0.4.3b, this considers active light sources.
    /// </remarks>
    public LightLevel CurrentLightLevel => CalculateCurrentLightLevel();

    /// <summary>
    /// Gets whether this room is outdoors.
    /// </summary>
    /// <remarks>
    /// Outdoor rooms are affected by the day/night cycle (v0.4.3d).
    /// Indoor rooms use only their configured BaseLightLevel.
    /// </remarks>
    public bool IsOutdoor { get; private set; }

    /// <summary>
    /// Gets a read-only list of light sources in this room.
    /// </summary>
    public IReadOnlyList<LightSource> LightSources => _lightSources.AsReadOnly();

    /// <summary>
    /// Gets whether there are active light sources in this room.
    /// </summary>
    public bool HasActiveLightSources => _lightSources.Any(ls => ls.IsActive);

    // ===== Harvestable Feature Properties (v0.11.0b) =====

    /// <summary>
    /// Gets a read-only list of harvestable features in this room.
    /// </summary>
    public IReadOnlyList<HarvestableFeature> HarvestableFeatures => _harvestableFeatures.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this room has any harvestable features.
    /// </summary>
    public bool HasHarvestableFeatures => _harvestableFeatures.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this room has any non-depleted harvestable features.
    /// </summary>
    public bool HasAvailableHarvestableFeatures => _harvestableFeatures.Any(f => !f.IsDepleted);

    /// <summary>
    /// Gets the type of this room.
    /// </summary>
    /// <remarks>
    /// Room type affects monster spawning, loot, and available interactions.
    /// </remarks>
    public RoomType RoomType { get; private set; } = RoomType.Standard;

    // ===== Exploration State (v0.1.0d) =====

    /// <summary>
    /// Gets the current exploration state of this room.
    /// </summary>
    /// <remarks>
    /// Exploration state tracks player progress through the dungeon
    /// and affects how the room appears on the map.
    /// </remarks>
    public ExplorationState ExplorationState { get; private set; } = ExplorationState.Unexplored;

    /// <summary>
    /// Gets whether this room has been visited by the player.
    /// </summary>
    public bool IsVisited => ExplorationState >= ExplorationState.Visited;

    /// <summary>
    /// Gets whether this room has been fully cleared.
    /// </summary>
    public bool IsCleared => ExplorationState == ExplorationState.Cleared;

    /// <summary>
    /// Gets whether this room can be marked as cleared.
    /// </summary>
    /// <remarks>
    /// A room can be cleared when:
    /// - It has been visited
    /// - No living monsters remain
    /// - All hidden content has been discovered (or none exists)
    /// </remarks>
    public bool CanBeCleared =>
        IsVisited &&
        !HasMonsters &&
        GetHiddenExits().Count == 0 &&
        GetUndiscoveredHiddenItems().Count == 0;

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

    // ===== Exploration State Methods (v0.1.0d) =====

    /// <summary>
    /// Marks this room as visited by the player.
    /// </summary>
    /// <returns>True if state changed; false if already visited or cleared.</returns>
    public bool MarkVisited()
    {
        if (ExplorationState >= ExplorationState.Visited)
            return false;

        ExplorationState = ExplorationState.Visited;
        return true;
    }

    /// <summary>
    /// Marks this room as fully cleared.
    /// </summary>
    /// <returns>True if state changed; false if already cleared or cannot be cleared.</returns>
    public bool MarkCleared()
    {
        if (ExplorationState == ExplorationState.Cleared)
            return false;

        if (!CanBeCleared)
            return false;

        ExplorationState = ExplorationState.Cleared;
        return true;
    }

    /// <summary>
    /// Checks if clearing conditions are met and auto-clears if possible.
    /// </summary>
    /// <returns>True if the room was auto-cleared.</returns>
    public bool TryAutoCleared()
    {
        if (CanBeCleared && ExplorationState == ExplorationState.Visited)
        {
            ExplorationState = ExplorationState.Cleared;
            return true;
        }
        return false;
    }

    // ===== Exit Methods =====

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

    // ===== Interactive Object Methods (v0.4.0a) =====

    /// <summary>
    /// Adds an interactive object to this room.
    /// </summary>
    /// <param name="interactable">The object to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when interactable is null.</exception>
    public void AddInteractable(InteractiveObject interactable)
    {
        ArgumentNullException.ThrowIfNull(interactable);
        _interactables.Add(interactable);
    }

    /// <summary>
    /// Removes an interactive object from this room.
    /// </summary>
    /// <param name="interactable">The object to remove.</param>
    /// <returns>True if the object was removed; false if not found.</returns>
    public bool RemoveInteractable(InteractiveObject interactable)
    {
        return _interactables.Remove(interactable);
    }

    /// <summary>
    /// Gets an interactive object by keyword.
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <returns>The matching object, or null if not found.</returns>
    public InteractiveObject? GetInteractableByKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return null;
        return _interactables.FirstOrDefault(i => i.MatchesKeyword(keyword));
    }

    /// <summary>
    /// Gets the object blocking a specific direction.
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns>The blocking object, or null if no object is blocking.</returns>
    public InteractiveObject? GetBlockingObject(Direction direction)
    {
        return _interactables.FirstOrDefault(i =>
            i.IsCurrentlyBlocking &&
            i.BlockedDirection == direction);
    }

    /// <summary>
    /// Gets all visible interactive objects in this room.
    /// </summary>
    /// <returns>Enumerable of visible interactive objects.</returns>
    public IEnumerable<InteractiveObject> GetVisibleInteractables()
    {
        return _interactables.Where(i => i.IsVisible);
    }

    /// <summary>
    /// Checks if movement in a direction is blocked by an interactive object.
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns>True if an object is blocking that direction.</returns>
    public bool IsDirectionBlocked(Direction direction)
    {
        return GetBlockingObject(direction) != null;
    }

    // ===== Hazard Zone Methods (v0.4.1c) =====

    /// <summary>
    /// Adds a hazard zone to this room.
    /// </summary>
    /// <param name="hazard">The hazard zone to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when hazard is null.</exception>
    public void AddHazardZone(HazardZone hazard)
    {
        ArgumentNullException.ThrowIfNull(hazard);
        _hazardZones.Add(hazard);
    }

    /// <summary>
    /// Removes a hazard zone from this room.
    /// </summary>
    /// <param name="hazard">The hazard zone to remove.</param>
    /// <returns>True if the hazard was removed; false if not found.</returns>
    public bool RemoveHazardZone(HazardZone hazard)
    {
        return _hazardZones.Remove(hazard);
    }

    /// <summary>
    /// Gets all active hazard zones in this room.
    /// </summary>
    /// <returns>An enumerable of active hazard zones.</returns>
    public IEnumerable<HazardZone> GetActiveHazards() =>
        _hazardZones.Where(h => h.IsActive);

    /// <summary>
    /// Gets a hazard zone by keyword (case-insensitive).
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <returns>The matching hazard zone, or null if not found.</returns>
    public HazardZone? GetHazardByKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return null;
        return _hazardZones.FirstOrDefault(h => h.IsActive && h.MatchesKeyword(keyword));
    }

    /// <summary>
    /// Gets hazard zones by type.
    /// </summary>
    /// <param name="hazardType">The type of hazard to filter by.</param>
    /// <returns>An enumerable of matching hazard zones.</returns>
    public IEnumerable<HazardZone> GetHazardsByType(Enums.HazardType hazardType) =>
        _hazardZones.Where(h => h.IsActive && h.HazardType == hazardType);

    /// <summary>
    /// Removes all expired hazard zones from this room.
    /// </summary>
    /// <returns>The number of hazards removed.</returns>
    public int RemoveExpiredHazards() =>
        _hazardZones.RemoveAll(h => !h.IsActive && h.IsExpired);

    // ===== Puzzle Methods (v0.4.2a) =====

    /// <summary>
    /// Adds a puzzle to this room.
    /// </summary>
    /// <param name="puzzle">The puzzle to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when puzzle is null.</exception>
    public void AddPuzzle(Puzzle puzzle)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        _puzzles.Add(puzzle);
    }

    /// <summary>
    /// Removes a puzzle from this room.
    /// </summary>
    /// <param name="puzzle">The puzzle to remove.</param>
    /// <returns>True if the puzzle was removed; false if not found.</returns>
    public bool RemovePuzzle(Puzzle puzzle)
    {
        return _puzzles.Remove(puzzle);
    }

    /// <summary>
    /// Gets a puzzle by keyword (case-insensitive).
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <returns>The matching puzzle, or null if not found.</returns>
    public Puzzle? GetPuzzleByKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return null;
        return _puzzles.FirstOrDefault(p => p.MatchesKeyword(keyword));
    }

    /// <summary>
    /// Gets all unsolved puzzles in this room.
    /// </summary>
    /// <returns>An enumerable of unsolved puzzles.</returns>
    public IEnumerable<Puzzle> GetUnsolvedPuzzles() =>
        _puzzles.Where(p => p.IsSolvable);

    /// <summary>
    /// Gets puzzles by type.
    /// </summary>
    /// <param name="puzzleType">The type of puzzle to filter by.</param>
    /// <returns>An enumerable of matching puzzles.</returns>
    public IEnumerable<Puzzle> GetPuzzlesByType(Enums.PuzzleType puzzleType) =>
        _puzzles.Where(p => p.Type == puzzleType);

    /// <summary>
    /// Processes puzzle reset ticks for all failed puzzles.
    /// </summary>
    /// <returns>The number of puzzles that were reset.</returns>
    public int ProcessPuzzleResetTicks()
    {
        var resetCount = 0;
        foreach (var puzzle in _puzzles.Where(p => p.IsFailed && p.TurnsUntilReset.HasValue))
        {
            if (puzzle.TickReset())
                resetCount++;
        }
        return resetCount;
    }

    // ===== Riddle NPC Methods (v0.4.2c) =====

    /// <summary>
    /// Adds a riddle NPC to this room.
    /// </summary>
    /// <param name="npc">The riddle NPC to add.</param>
    public void AddRiddleNpc(RiddleNpc npc)
    {
        ArgumentNullException.ThrowIfNull(npc);
        if (!_riddleNpcs.Any(n => n.Id == npc.Id))
        {
            _riddleNpcs.Add(npc);
        }
    }

    /// <summary>
    /// Removes a riddle NPC from this room.
    /// </summary>
    /// <param name="npc">The riddle NPC to remove.</param>
    /// <returns>True if removed, false if not found.</returns>
    public bool RemoveRiddleNpc(RiddleNpc npc)
    {
        ArgumentNullException.ThrowIfNull(npc);
        return _riddleNpcs.Remove(npc);
    }

    /// <summary>
    /// Gets a riddle NPC by keyword (name match).
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <returns>The matching RiddleNpc, or null.</returns>
    public RiddleNpc? GetRiddleNpcByKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return null;

        return _riddleNpcs.FirstOrDefault(n =>
            n.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if any riddle NPC blocks passage in a direction.
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns>True if blocked by an unsolved riddle NPC.</returns>
    public bool IsDirectionBlockedByNpc(Direction direction)
    {
        return _riddleNpcs.Any(n => n.IsPassageBlocked(direction));
    }

    /// <summary>
    /// Gets the NPC blocking a specific direction.
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns>The blocking RiddleNpc, or null.</returns>
    public RiddleNpc? GetBlockingNpc(Direction direction)
    {
        return _riddleNpcs.FirstOrDefault(n => n.IsPassageBlocked(direction));
    }

    // ===== Light Level Methods (v0.4.3a) =====

    /// <summary>
    /// Sets the base light level for this room.
    /// </summary>
    /// <param name="level">The new base light level.</param>
    public void SetBaseLightLevel(LightLevel level)
    {
        BaseLightLevel = level;
    }

    /// <summary>
    /// Sets whether this room is outdoors.
    /// </summary>
    /// <param name="isOutdoor">True if the room is outdoors.</param>
    public void SetIsOutdoor(bool isOutdoor)
    {
        IsOutdoor = isOutdoor;
    }

    /// <summary>
    /// Calculates the current effective light level.
    /// </summary>
    /// <returns>The effective light level.</returns>
    /// <remarks>
    /// Considers active light sources. Light sources can only
    /// make rooms brighter, never darker.
    /// </remarks>
    public LightLevel CalculateCurrentLightLevel()
    {
        if (!HasActiveLightSources)
            return BaseLightLevel;

        // Get the brightest active light source (lower enum = brighter)
        var brightestSource = _lightSources
            .Where(ls => ls.IsActive)
            .OrderBy(ls => ls.ProvidedLight)
            .FirstOrDefault();

        if (brightestSource == null)
            return BaseLightLevel;

        // Light can only make rooms brighter, not darker
        return brightestSource.ProvidedLight < BaseLightLevel
            ? brightestSource.ProvidedLight
            : BaseLightLevel;
    }

    // ===== Light Source Methods (v0.4.3b) =====

    /// <summary>
    /// Adds a light source to this room.
    /// </summary>
    /// <param name="lightSource">The light source to add.</param>
    public void AddLightSource(LightSource lightSource)
    {
        ArgumentNullException.ThrowIfNull(lightSource);
        _lightSources.Add(lightSource);
    }

    /// <summary>
    /// Removes a light source from this room.
    /// </summary>
    /// <param name="lightSource">The light source to remove.</param>
    /// <returns>True if removed, false if not found.</returns>
    public bool RemoveLightSource(LightSource lightSource)
    {
        return _lightSources.Remove(lightSource);
    }

    /// <summary>
    /// Gets all active light sources in this room.
    /// </summary>
    /// <returns>Enumerable of active light sources.</returns>
    public IEnumerable<LightSource> GetActiveLightSources()
    {
        return _lightSources.Where(ls => ls.IsActive);
    }

    // ===== Harvestable Feature Methods (v0.11.0b) =====

    /// <summary>
    /// Adds a harvestable feature to this room.
    /// </summary>
    /// <param name="feature">The harvestable feature to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when feature is null.</exception>
    /// <remarks>
    /// <para>
    /// Harvestable features are environmental objects that yield resources
    /// when gathered by the player (e.g., ore veins, herb patches).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var feature = featureProvider.CreateFeatureInstance("iron-ore-vein");
    /// room.AddHarvestableFeature(feature);
    /// </code>
    /// </example>
    public void AddHarvestableFeature(HarvestableFeature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);
        _harvestableFeatures.Add(feature);
    }

    /// <summary>
    /// Removes a harvestable feature from this room.
    /// </summary>
    /// <param name="feature">The harvestable feature to remove.</param>
    /// <returns>True if removed, false if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when feature is null.</exception>
    public bool RemoveHarvestableFeature(HarvestableFeature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);
        return _harvestableFeatures.Remove(feature);
    }

    /// <summary>
    /// Gets all harvestable features in this room.
    /// </summary>
    /// <returns>A read-only list of harvestable features.</returns>
    /// <remarks>
    /// <para>
    /// Returns all features regardless of depletion state.
    /// Use <see cref="GetAvailableHarvestableFeatures"/> to get only
    /// non-depleted features that can be harvested.
    /// </para>
    /// </remarks>
    public IReadOnlyList<HarvestableFeature> GetHarvestableFeatures()
    {
        return _harvestableFeatures.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all non-depleted harvestable features in this room.
    /// </summary>
    /// <returns>A read-only list of available harvestable features.</returns>
    /// <remarks>
    /// <para>
    /// Returns only features where <see cref="HarvestableFeature.IsDepleted"/>
    /// is false. These are features that still have resources to harvest.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var available = room.GetAvailableHarvestableFeatures();
    /// foreach (var feature in available)
    /// {
    ///     Console.WriteLine($"- {feature.Name} ({feature.RemainingQuantity} remaining)");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyList<HarvestableFeature> GetAvailableHarvestableFeatures()
    {
        return _harvestableFeatures
            .Where(f => !f.IsDepleted)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets a harvestable feature by its definition ID.
    /// </summary>
    /// <param name="definitionId">The definition ID to search for (case-insensitive).</param>
    /// <returns>The matching feature, or null if not found.</returns>
    /// <remarks>
    /// <para>
    /// Searches for a feature with a matching <see cref="HarvestableFeature.DefinitionId"/>.
    /// If multiple features of the same type exist, returns the first one found.
    /// </para>
    /// </remarks>
    public HarvestableFeature? GetHarvestableFeatureByDefinitionId(string definitionId)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
            return null;

        return _harvestableFeatures.FirstOrDefault(f =>
            f.DefinitionId.Equals(definitionId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a harvestable feature by keyword (name match).
    /// </summary>
    /// <param name="keyword">The keyword to search for (case-insensitive).</param>
    /// <returns>The matching feature, or null if not found.</returns>
    /// <remarks>
    /// <para>
    /// Searches feature names for a match containing the keyword.
    /// Useful for player commands like "gather ore" where "ore" is the keyword.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var feature = room.GetHarvestableFeatureByKeyword("ore");
    /// if (feature is not null &amp;&amp; !feature.IsDepleted)
    /// {
    ///     // Player can gather from this feature
    /// }
    /// </code>
    /// </example>
    public HarvestableFeature? GetHarvestableFeatureByKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return null;

        return _harvestableFeatures.FirstOrDefault(f =>
            f.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Processes replenishment for all harvestable features in this room.
    /// </summary>
    /// <param name="currentTurn">The current game turn.</param>
    /// <returns>A list of features that were replenished.</returns>
    /// <remarks>
    /// <para>
    /// Checks all harvestable features and calls <see cref="HarvestableFeature.Replenish()"/>
    /// on any that have reached their replenishment turn.
    /// </para>
    /// <para>
    /// Call this method during turn processing to enable renewable resources.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During turn processing
    /// var replenished = room.ProcessFeatureReplenishment(gameState.CurrentTurn);
    /// foreach (var feature in replenished)
    /// {
    ///     Console.WriteLine($"{feature.Name} has regrown!");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyList<HarvestableFeature> ProcessFeatureReplenishment(int currentTurn)
    {
        var replenished = new List<HarvestableFeature>();

        foreach (var feature in _harvestableFeatures)
        {
            if (feature.ShouldReplenish(currentTurn))
            {
                feature.Replenish();
                replenished.Add(feature);
            }
        }

        return replenished.AsReadOnly();
    }

    /// <summary>
    /// Returns the name of this room.
    /// </summary>
    /// <returns>The room name.</returns>
    public override string ToString() => Name;
}
