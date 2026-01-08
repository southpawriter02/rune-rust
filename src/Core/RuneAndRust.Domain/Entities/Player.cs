using RuneAndRust.Domain.Definitions;
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
    /// Gets the player's current experience points.
    /// </summary>
    /// <remarks>
    /// Experience is gained by defeating monsters. When enough XP is accumulated,
    /// the player levels up (handled by ProgressionService in v0.0.8b).
    /// </remarks>
    public int Experience { get; private set; } = 0;

    /// <summary>
    /// Gets the experience points required to reach the next level using default progression.
    /// </summary>
    /// <remarks>
    /// Use <see cref="GetExperienceToNextLevel"/> for configurable progression.
    /// </remarks>
    public int ExperienceToNextLevel => GetExperienceToNextLevel(null);

    /// <summary>
    /// Gets the experience points required to reach the next level.
    /// </summary>
    /// <param name="progression">The progression configuration to use, or null for defaults.</param>
    /// <returns>XP needed for next level, or 0 if at max level.</returns>
    public int GetExperienceToNextLevel(ProgressionDefinition? progression)
    {
        progression ??= ProgressionDefinition.Default;

        if (progression.MaxLevel > 0 && Level >= progression.MaxLevel)
        {
            return 0; // At max level
        }

        return progression.GetExperienceForLevel(Level + 1);
    }

    /// <summary>
    /// Gets the player's progress toward the next level as a percentage (0-100).
    /// </summary>
    /// <remarks>
    /// Calculated as: (Experience / ExperienceToNextLevel) * 100, clamped to 0-100.
    /// </remarks>
    public int ExperienceProgressPercent
    {
        get
        {
            if (ExperienceToNextLevel <= 0) return 100;
            var percent = (int)((double)Experience / ExperienceToNextLevel * 100);
            return Math.Clamp(percent, 0, 100);
        }
    }

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
    /// Adds experience points to the player.
    /// </summary>
    /// <param name="amount">The amount of experience to add.</param>
    /// <returns>The new total experience points.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is negative.</exception>
    /// <remarks>
    /// This method only adds XP. Level-up detection and stat increases are
    /// handled by ProgressionService in v0.0.8b.
    /// </remarks>
    public int AddExperience(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        Experience += amount;
        return Experience;
    }

    /// <summary>
    /// Gets the cumulative XP required to reach the specified level.
    /// </summary>
    /// <param name="level">The target level.</param>
    /// <param name="progression">The progression configuration to use, or null for defaults.</param>
    /// <returns>The cumulative XP required.</returns>
    public static int GetExperienceForLevel(int level, ProgressionDefinition? progression = null)
    {
        progression ??= ProgressionDefinition.Default;
        return progression.GetExperienceForLevel(level);
    }

    /// <summary>
    /// Gets what level the player should be at for a given XP amount.
    /// </summary>
    /// <param name="experience">The experience amount.</param>
    /// <param name="progression">The progression configuration to use, or null for defaults.</param>
    /// <returns>The level corresponding to that experience.</returns>
    public static int GetLevelForExperience(int experience, ProgressionDefinition? progression = null)
    {
        progression ??= ProgressionDefinition.Default;
        return progression.GetLevelForExperience(experience);
    }

    /// <summary>
    /// Applies stat modifications from leveling up.
    /// </summary>
    /// <param name="modifiers">The stat modifiers to apply.</param>
    /// <param name="healToNewMax">If true, restores health to new maximum after applying modifiers.</param>
    /// <remarks>
    /// Used by ProgressionService when the player levels up.
    /// </remarks>
    public void ApplyLevelStatModifiers(LevelStatModifiers modifiers, bool healToNewMax = false)
    {
        var newStats = new Stats(
            Stats.MaxHealth + modifiers.MaxHealth,
            Stats.Attack + modifiers.Attack,
            Stats.Defense + modifiers.Defense);

        Stats = newStats;

        if (healToNewMax)
        {
            Health = Stats.MaxHealth;
        }
        else if (Health > Stats.MaxHealth)
        {
            // Cap health at new maximum (shouldn't happen with positive modifiers)
            Health = Stats.MaxHealth;
        }
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
    /// Calculates the player's effective stats including equipment bonuses.
    /// </summary>
    /// <returns>Stats with all equipment bonuses applied.</returns>
    /// <remarks>
    /// Defense is calculated as: Base Defense + Sum of all DefenseBonus from equipment + StatModifiers.Defense
    /// MaxHealth is calculated as: Base MaxHealth + Sum of all StatModifiers.MaxHealth
    /// </remarks>
    public Stats GetEffectiveStats()
    {
        var totalDefenseBonus = 0;
        var totalStatModifiers = StatModifiers.None;

        foreach (var (slot, item) in Equipment)
        {
            totalDefenseBonus += item.DefenseBonus;
            totalStatModifiers += item.StatModifiers;
        }

        return new Stats(
            maxHealth: Stats.MaxHealth + totalStatModifiers.MaxHealth,
            attack: Stats.Attack + totalStatModifiers.Attack,
            defense: Stats.Defense + totalDefenseBonus + totalStatModifiers.Defense
        );
    }

    /// <summary>
    /// Calculates the player's effective attributes including equipment bonuses.
    /// </summary>
    /// <returns>Attributes with all equipment bonuses applied.</returns>
    /// <remarks>
    /// Combines base attributes with:
    /// - WeaponBonuses from equipped weapon
    /// - StatModifiers from all equipped items
    /// </remarks>
    public PlayerAttributes GetEffectiveAttributes()
    {
        var totalStatModifiers = StatModifiers.None;
        var weaponBonuses = WeaponBonuses.None;

        foreach (var (slot, item) in Equipment)
        {
            totalStatModifiers += item.StatModifiers;

            if (slot == EquipmentSlot.Weapon && item.IsWeapon)
            {
                weaponBonuses = item.WeaponBonuses;
            }
        }

        // Clamp values to valid range (1-30)
        return new PlayerAttributes(
            Math.Clamp(Attributes.Might + totalStatModifiers.Might + weaponBonuses.Might, 1, 30),
            Math.Clamp(Attributes.Fortitude + totalStatModifiers.Fortitude + weaponBonuses.Fortitude, 1, 30),
            Math.Clamp(Attributes.Will + totalStatModifiers.Will + weaponBonuses.Will, 1, 30),
            Math.Clamp(Attributes.Wits + totalStatModifiers.Wits + weaponBonuses.Wits, 1, 30),
            Math.Clamp(Attributes.Finesse + totalStatModifiers.Finesse + weaponBonuses.Finesse, 1, 30)
        );
    }

    /// <summary>
    /// Gets the total initiative penalty from all equipped items.
    /// </summary>
    /// <returns>Total initiative penalty (should be 0 or negative).</returns>
    public int GetTotalInitiativePenalty()
    {
        var penalty = 0;
        foreach (var (_, item) in Equipment)
        {
            penalty += item.InitiativePenalty;
        }
        return penalty;
    }

    /// <summary>
    /// Gets the total defense bonus from all equipped armor and items.
    /// </summary>
    /// <returns>Sum of all defense bonuses.</returns>
    public int GetTotalDefenseBonus()
    {
        var bonus = 0;
        foreach (var (_, item) in Equipment)
        {
            bonus += item.DefenseBonus;
            bonus += item.StatModifiers.Defense;
        }
        return bonus;
    }

    /// <summary>
    /// Returns a string representation of this player.
    /// </summary>
    /// <returns>A string containing the player name and current/max health.</returns>
    public override string ToString() => $"{Name} (HP: {Health}/{Stats.MaxHealth})";
}
