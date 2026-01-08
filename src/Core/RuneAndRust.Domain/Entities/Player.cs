using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a player character in the game.
/// </summary>
/// <remarks>
/// The Player entity encapsulates all character state including health, stats,
/// position in the dungeon, and inventory. It handles combat damage, healing,
/// and item management.
/// </remarks>
public class Player : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this player.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the player's display name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the player's race definition ID.
    /// </summary>
    public string RaceId { get; private set; } = "human";

    /// <summary>
    /// Gets the player's background definition ID.
    /// </summary>
    public string BackgroundId { get; private set; } = "soldier";

    /// <summary>
    /// Gets the player's core attributes (Might, Fortitude, Will, Wits, Finesse).
    /// </summary>
    public PlayerAttributes Attributes { get; private set; } = PlayerAttributes.Default;

    /// <summary>
    /// Gets the player's optional description/backstory.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the ID of the player's archetype.
    /// </summary>
    /// <remarks>
    /// Set during character creation when a class is selected.
    /// </remarks>
    public string? ArchetypeId { get; private set; }

    /// <summary>
    /// Gets the ID of the player's class.
    /// </summary>
    /// <remarks>
    /// Set during character creation. Determines resource type and abilities.
    /// </remarks>
    public string? ClassId { get; private set; }

    /// <summary>
    /// Gets whether the player has selected a class.
    /// </summary>
    public bool HasClass => !string.IsNullOrEmpty(ClassId);

    /// <summary>
    /// Gets the player's resource pools keyed by resource type ID.
    /// </summary>
    public Dictionary<string, ResourcePool> Resources { get; private set; } = new();

    /// <summary>
    /// Gets the player's abilities keyed by ability definition ID.
    /// </summary>
    public Dictionary<string, PlayerAbility> Abilities { get; private set; } = new();

    /// <summary>
    /// Gets the player's current level.
    /// </summary>
    public int Level { get; private set; } = 1;

    /// <summary>
    /// Gets a specific resource pool by type ID.
    /// </summary>
    /// <param name="resourceTypeId">The resource type ID (e.g., "mana").</param>
    /// <returns>The resource pool, or null if the player doesn't have it.</returns>
    public ResourcePool? GetResource(string resourceTypeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceTypeId);
        return Resources.TryGetValue(resourceTypeId.ToLowerInvariant(), out var pool)
            ? pool
            : null;
    }

    /// <summary>
    /// Checks whether the player has a specific resource type.
    /// </summary>
    public bool HasResource(string resourceTypeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceTypeId);
        return Resources.ContainsKey(resourceTypeId.ToLowerInvariant());
    }

    /// <summary>
    /// Initializes a resource pool for the player.
    /// </summary>
    /// <param name="resourceTypeId">The resource type ID.</param>
    /// <param name="maximum">The maximum value.</param>
    /// <param name="startAtZero">Whether to start at zero instead of max.</param>
    public void InitializeResource(string resourceTypeId, int maximum, bool startAtZero = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceTypeId);
        var id = resourceTypeId.ToLowerInvariant();
        Resources[id] = new ResourcePool(id, maximum, startAtZero);
    }

    /// <summary>
    /// Gets a specific ability by its definition ID.
    /// </summary>
    /// <param name="abilityId">The ability definition ID.</param>
    /// <returns>The player ability, or null if the player doesn't have it.</returns>
    public PlayerAbility? GetAbility(string abilityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId);
        return Abilities.TryGetValue(abilityId.ToLowerInvariant(), out var ability)
            ? ability
            : null;
    }

    /// <summary>
    /// Checks whether the player has a specific ability.
    /// </summary>
    /// <param name="abilityId">The ability definition ID.</param>
    /// <returns>True if the player has this ability.</returns>
    public bool HasAbility(string abilityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId);
        return Abilities.ContainsKey(abilityId.ToLowerInvariant());
    }

    /// <summary>
    /// Adds an ability to the player's ability collection.
    /// </summary>
    /// <param name="ability">The ability to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when ability is null.</exception>
    public void AddAbility(PlayerAbility ability)
    {
        ArgumentNullException.ThrowIfNull(ability);
        Abilities[ability.AbilityDefinitionId] = ability;
    }

    /// <summary>
    /// Removes an ability from the player's collection.
    /// </summary>
    /// <param name="abilityId">The ability definition ID.</param>
    /// <returns>True if the ability was removed.</returns>
    public bool RemoveAbility(string abilityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId);
        return Abilities.Remove(abilityId.ToLowerInvariant());
    }

    /// <summary>
    /// Gets all abilities that are ready to use (off cooldown and unlocked).
    /// </summary>
    /// <returns>An enumerable of ready abilities.</returns>
    public IEnumerable<PlayerAbility> GetReadyAbilities()
    {
        return Abilities.Values.Where(a => a.IsReady);
    }

    /// <summary>
    /// Sets the player's level.
    /// </summary>
    /// <param name="level">The new level (must be at least 1).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when level is less than 1.</exception>
    public void SetLevel(int level)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);
        Level = level;
    }

    /// <summary>
    /// Gets the player's current health points.
    /// </summary>
    /// <value>A non-negative integer representing current HP. Zero means the player is dead.</value>
    public int Health { get; private set; }

    /// <summary>
    /// Gets the player's base statistics (max health, attack, defense).
    /// </summary>
    public Stats Stats { get; private set; }

    /// <summary>
    /// Gets the player's current position in the dungeon.
    /// </summary>
    public Position Position { get; private set; }

    /// <summary>
    /// Gets the player's inventory containing collected items.
    /// </summary>
    public Inventory Inventory { get; private set; }

    /// <summary>
    /// Gets the player's currently equipped items, keyed by equipment slot.
    /// </summary>
    /// <remarks>
    /// Equipped items are separate from inventory. When an item is equipped,
    /// it is removed from inventory. When unequipped, it returns to inventory.
    /// </remarks>
    public Dictionary<EquipmentSlot, Item> Equipment { get; private set; } = new();

    /// <summary>
    /// Gets the number of equipped items.
    /// </summary>
    public int EquippedItemCount => Equipment.Count;

    /// <summary>
    /// Gets a value indicating whether the player is alive (health greater than zero).
    /// </summary>
    public bool IsAlive => Health > 0;

    /// <summary>
    /// Gets a value indicating whether the player is dead (health is zero or less).
    /// </summary>
    public bool IsDead => Health <= 0;

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Player()
    {
        Name = null!;
        Inventory = null!;
    }

    /// <summary>
    /// Creates a new player with the specified name and optional stats.
    /// </summary>
    /// <param name="name">The player's display name.</param>
    /// <param name="stats">Optional custom stats. If null, default stats are used.</param>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    public Player(string name, Stats? stats = null)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Stats = stats ?? Stats.Default;
        Health = Stats.MaxHealth;
        Position = Position.Origin;
        Inventory = new Inventory();
        Equipment = new Dictionary<EquipmentSlot, Item>();
    }

    /// <summary>
    /// Creates a new player with full character creation options.
    /// </summary>
    /// <param name="name">The player's display name.</param>
    /// <param name="raceId">The race definition ID.</param>
    /// <param name="backgroundId">The background definition ID.</param>
    /// <param name="attributes">The player's core attributes.</param>
    /// <param name="description">Optional character description/backstory.</param>
    /// <param name="stats">Optional custom stats. If null, stats are derived from attributes.</param>
    public Player(
        string name,
        string raceId,
        string backgroundId,
        PlayerAttributes attributes,
        string description = "",
        Stats? stats = null)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        RaceId = raceId ?? throw new ArgumentNullException(nameof(raceId));
        BackgroundId = backgroundId ?? throw new ArgumentNullException(nameof(backgroundId));
        Attributes = attributes;
        Description = description ?? string.Empty;

        // Derive stats from attributes if not provided
        Stats = stats ?? DeriveStatsFromAttributes(attributes);
        Health = Stats.MaxHealth;
        Position = Position.Origin;
        Inventory = new Inventory();
        Equipment = new Dictionary<EquipmentSlot, Item>();
    }

    /// <summary>
    /// Sets the player's description (max 500 characters).
    /// </summary>
    /// <param name="description">The description text.</param>
    /// <exception cref="ArgumentException">Thrown if description exceeds limits.</exception>
    public void SetDescription(string description)
    {
        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters");

        var lineBreaks = description.Count(c => c == '\n');
        if (lineBreaks > 5)
            throw new ArgumentException("Description cannot have more than 5 line breaks");

        Description = description;
    }

    /// <summary>
    /// Sets the player's class and archetype.
    /// </summary>
    /// <param name="archetypeId">The archetype ID.</param>
    /// <param name="classId">The class ID.</param>
    /// <exception cref="ArgumentException">Thrown if IDs are null or empty.</exception>
    public void SetClass(string archetypeId, string classId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(classId);

        ArchetypeId = archetypeId.ToLowerInvariant();
        ClassId = classId.ToLowerInvariant();
    }

    /// <summary>
    /// Sets the player's stats directly (used when applying class modifiers).
    /// </summary>
    /// <param name="stats">The new stats.</param>
    public void SetStats(Stats stats)
    {
        var wasFullHealth = Health == Stats.MaxHealth;
        Stats = stats;
        // Adjust current health if at full health before
        if (wasFullHealth || Health > Stats.MaxHealth)
        {
            Health = Stats.MaxHealth;
        }
    }

    /// <summary>
    /// Derives combat stats from player attributes.
    /// </summary>
    private static Stats DeriveStatsFromAttributes(PlayerAttributes attributes)
    {
        // MaxHealth = 100 + (Fortitude - 10) * 5
        // Attack = Might
        // Defense = Finesse / 2
        var maxHealth = 100 + (attributes.Fortitude - 10) * 5;
        var attack = attributes.Might;
        var defense = attributes.Finesse / 2;

        return new Stats(maxHealth, attack, defense);
    }

    /// <summary>
    /// Moves the player to a new position in the dungeon.
    /// </summary>
    /// <param name="newPosition">The new position to move to.</param>
    public void MoveTo(Position newPosition)
    {
        Position = newPosition;
    }

    /// <summary>
    /// Applies damage to the player, reduced by their defense stat.
    /// </summary>
    /// <param name="damage">The raw damage amount before defense reduction.</param>
    /// <returns>The actual damage dealt after defense calculation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when damage is negative.</exception>
    /// <remarks>
    /// Damage is reduced by the player's defense stat (minimum 0 damage taken).
    /// The player's health cannot go below zero.
    /// </remarks>
    public int TakeDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative");

        var actualDamage = Math.Max(0, damage - Stats.Defense);
        Health = Math.Max(0, Health - actualDamage);
        return actualDamage;
    }

    /// <summary>
    /// Heals the player by the specified amount, up to their maximum health.
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    /// <returns>The actual amount of health restored (may be less if at max health).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is negative.</exception>
    public int Heal(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Heal amount cannot be negative");

        var previousHealth = Health;
        Health = Math.Min(Stats.MaxHealth, Health + amount);
        return Health - previousHealth;
    }

    /// <summary>
    /// Attempts to add an item to the player's inventory.
    /// </summary>
    /// <param name="item">The item to pick up.</param>
    /// <returns><c>true</c> if the item was added successfully; <c>false</c> if the inventory is full.</returns>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public bool TryPickUpItem(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        return Inventory.TryAdd(item);
    }

    /// <summary>
    /// Gets the item equipped in the specified slot, or null if empty.
    /// </summary>
    /// <param name="slot">The equipment slot to check.</param>
    /// <returns>The equipped item, or null if the slot is empty.</returns>
    public Item? GetEquippedItem(EquipmentSlot slot)
    {
        return Equipment.TryGetValue(slot, out var item) ? item : null;
    }

    /// <summary>
    /// Checks whether the specified equipment slot is occupied.
    /// </summary>
    /// <param name="slot">The equipment slot to check.</param>
    /// <returns>True if an item is equipped in the slot.</returns>
    public bool IsSlotOccupied(EquipmentSlot slot)
    {
        return Equipment.ContainsKey(slot);
    }

    /// <summary>
    /// Equips an item to its designated slot.
    /// </summary>
    /// <param name="item">The item to equip.</param>
    /// <returns>True if the item was equipped successfully.</returns>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when item has no equipment slot.</exception>
    /// <remarks>
    /// The item must have an EquipmentSlot defined. If the slot is already occupied,
    /// this method returns false. Use EquipmentService for swap logic.
    /// </remarks>
    public bool TryEquip(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!item.EquipmentSlot.HasValue)
            throw new InvalidOperationException($"Item '{item.Name}' cannot be equipped (no equipment slot).");

        var slot = item.EquipmentSlot.Value;

        // If slot is occupied, return false (use EquipmentService for swap logic)
        if (IsSlotOccupied(slot))
            return false;

        Equipment[slot] = item;
        return true;
    }

    /// <summary>
    /// Unequips the item from the specified slot.
    /// </summary>
    /// <param name="slot">The slot to unequip from.</param>
    /// <returns>The unequipped item, or null if the slot was empty.</returns>
    public Item? Unequip(EquipmentSlot slot)
    {
        if (Equipment.TryGetValue(slot, out var item))
        {
            Equipment.Remove(slot);
            return item;
        }
        return null;
    }

    /// <summary>
    /// Gets all currently equipped items.
    /// </summary>
    /// <returns>A read-only collection of equipped items.</returns>
    public IReadOnlyCollection<Item> GetAllEquippedItems()
    {
        return Equipment.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Returns a string representation of this player.
    /// </summary>
    /// <returns>A string containing the player name and current/max health.</returns>
    public override string ToString() => $"{Name} (HP: {Health}/{Stats.MaxHealth})";
}
