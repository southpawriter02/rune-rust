using RuneAndRust.Domain.Constants;
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
    /// Gets the player's current 3D position in the dungeon.
    /// </summary>
    public Position3D Position { get; private set; }

    /// <summary>
    /// Gets the player's inventory containing collected items.
    /// </summary>
    public Inventory Inventory { get; private set; }

    /// <summary>
    /// Gets the player's damage resistances.
    /// </summary>
    /// <remarks>
    /// Can be modified by equipment, buffs, or class abilities.
    /// </remarks>
    public DamageResistances Resistances { get; private set; } = DamageResistances.None;

    // ===== Recipe Book Properties (v0.11.1b) =====

    /// <summary>
    /// Gets the player's recipe book containing known crafting recipes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The recipe book is created automatically when a player is created
    /// and tracks which recipes the player has learned.
    /// </para>
    /// <para>
    /// Default recipes are initialized by the RecipeService during
    /// player creation. Additional recipes can be learned through
    /// discovery, scrolls, or quest rewards.
    /// </para>
    /// </remarks>
    public RecipeBook RecipeBook { get; private set; } = null!;

    // ===== Statistics Properties (v0.12.0a) =====

    /// <summary>
    /// Gets the player's statistics tracking entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The statistics entity tracks comprehensive game statistics across combat,
    /// exploration, progression, and time categories. It is created lazily
    /// when first accessed via <see cref="InitializeStatistics"/> or when
    /// the StatisticsService records a statistic.
    /// </para>
    /// <para>
    /// For existing players loaded from database, use <see cref="EnsureStatistics"/>
    /// to ensure the statistics entity exists.
    /// </para>
    /// </remarks>
    public PlayerStatistics? Statistics { get; private set; }

    // ===== Currency Properties (v0.0.9d) =====

    /// <summary>
    /// Dictionary of currency amounts owned by the player.
    /// </summary>
    private readonly Dictionary<string, int> _currency = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a read-only view of the player's currency.
    /// </summary>
    public IReadOnlyDictionary<string, int> Currency => _currency.AsReadOnly();

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
        RecipeBook = null!;
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
        Position = Position3D.Origin;
        Inventory = new Inventory();
        Equipment = new Dictionary<EquipmentSlot, Item>();
        RecipeBook = RecipeBook.Create(Id);
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
        Position = Position3D.Origin;
        Inventory = new Inventory();
        Equipment = new Dictionary<EquipmentSlot, Item>();
        RecipeBook = RecipeBook.Create(Id);
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
    /// Moves the player to a new 3D position in the dungeon.
    /// </summary>
    /// <param name="newPosition">The new 3D position to move to.</param>
    public void MoveTo(Position3D newPosition)
    {
        Position = newPosition;
    }

    /// <summary>
    /// Sets the player's damage resistances.
    /// </summary>
    /// <param name="resistances">The new resistances to apply.</param>
    /// <remarks>
    /// This replaces the current resistances entirely.
    /// Use for equipment changes or buff applications.
    /// </remarks>
    public void SetResistances(DamageResistances resistances)
    {
        Resistances = resistances;
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

    // ===== Currency Methods (v0.0.9d) =====

    /// <summary>
    /// Gets the amount of a specific currency the player has.
    /// </summary>
    /// <param name="currencyId">The currency ID.</param>
    /// <returns>The amount, or 0 if the player has none.</returns>
    public int GetCurrency(string currencyId)
    {
        if (string.IsNullOrWhiteSpace(currencyId))
            return 0;

        return _currency.TryGetValue(currencyId.ToLowerInvariant(), out var amount)
            ? amount
            : 0;
    }

    /// <summary>
    /// Adds currency to the player.
    /// </summary>
    /// <param name="currencyId">The currency ID.</param>
    /// <param name="amount">The amount to add.</param>
    /// <exception cref="ArgumentException">Thrown when currencyId is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is negative.</exception>
    public void AddCurrency(string currencyId, int amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyId);
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (amount == 0) return;

        var id = currencyId.ToLowerInvariant();
        if (_currency.TryGetValue(id, out var current))
        {
            _currency[id] = current + amount;
        }
        else
        {
            _currency[id] = amount;
        }
    }

    /// <summary>
    /// Removes currency from the player.
    /// </summary>
    /// <param name="currencyId">The currency ID.</param>
    /// <param name="amount">The amount to remove.</param>
    /// <returns>True if the player had enough currency; false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown when currencyId is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is negative.</exception>
    public bool RemoveCurrency(string currencyId, int amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyId);
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (amount == 0) return true;

        var id = currencyId.ToLowerInvariant();
        var current = GetCurrency(id);

        if (current < amount)
            return false;

        _currency[id] = current - amount;
        return true;
    }

    /// <summary>
    /// Checks if the player can afford a specific amount of currency.
    /// </summary>
    /// <param name="currencyId">The currency ID.</param>
    /// <param name="amount">The amount needed.</param>
    /// <returns>True if the player has enough.</returns>
    public bool CanAfford(string currencyId, int amount)
    {
        return GetCurrency(currencyId) >= amount;
    }

    // ===== Quest Properties (v0.3.2c) =====

    private readonly List<Quest> _activeQuests = [];
    private readonly List<Quest> _completedQuests = [];
    private readonly List<Quest> _failedQuests = [];
    private readonly List<string> _abandonedQuestIds = [];

    /// <summary>Gets the player's active quests.</summary>
    public IReadOnlyList<Quest> ActiveQuests => _activeQuests;

    /// <summary>Gets the player's completed quests.</summary>
    public IReadOnlyList<Quest> CompletedQuests => _completedQuests;

    /// <summary>Gets the player's failed quests.</summary>
    public IReadOnlyList<Quest> FailedQuests => _failedQuests;

    /// <summary>Gets quests that have been abandoned by the player.</summary>
    public IReadOnlyList<string> AbandonedQuestIds => _abandonedQuestIds;

    /// <summary>Gets the count of abandoned quests.</summary>
    public int AbandonedQuestCount => _abandonedQuestIds.Count;

    // ===== Quest Methods (v0.3.2c) =====

    /// <summary>Adds a quest to active quests.</summary>
    public void AddQuest(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);
        if (!_activeQuests.Contains(quest))
            _activeQuests.Add(quest);
    }

    /// <summary>Moves a quest to completed.</summary>
    public void CompleteQuest(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);
        if (_activeQuests.Remove(quest))
            _completedQuests.Add(quest);
    }

    /// <summary>Moves a quest to failed.</summary>
    public void FailQuest(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);
        if (_activeQuests.Remove(quest))
            _failedQuests.Add(quest);
    }

    /// <summary>Abandons a quest.</summary>
    public void AbandonQuest(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);

        if (_activeQuests.Remove(quest))
        {
            quest.Abandon();
            _abandonedQuestIds.Add(quest.DefinitionId);
        }
    }

    /// <summary>Gets whether a specific quest has been abandoned.</summary>
    public bool HasAbandonedQuest(string questDefinitionId)
    {
        if (string.IsNullOrWhiteSpace(questDefinitionId))
            return false;

        return _abandonedQuestIds.Any(id =>
            id.Equals(questDefinitionId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Gets active quests filtered by category.</summary>
    public IEnumerable<Quest> GetQuestsByCategory(QuestCategory category)
    {
        return _activeQuests.Where(q => q.Category == category);
    }

    /// <summary>Gets all quests (active, completed, failed) filtered by category.</summary>
    public IEnumerable<Quest> GetAllQuestsByCategory(QuestCategory category)
    {
        var results = new List<Quest>();
        results.AddRange(_activeQuests.Where(q => q.Category == category));
        results.AddRange(_completedQuests.Where(q => q.Category == category));
        results.AddRange(_failedQuests.Where(q => q.Category == category));
        return results;
    }

    /// <summary>Gets all main quests.</summary>
    public IEnumerable<Quest> GetMainQuests()
    {
        return GetAllQuestsByCategory(QuestCategory.Main);
    }

    /// <summary>Gets all daily quests.</summary>
    public IEnumerable<Quest> GetDailyQuests()
    {
        return GetAllQuestsByCategory(QuestCategory.Daily);
    }

    /// <summary>Gets completed daily quests that need to be reset.</summary>
    public IEnumerable<Quest> GetCompletedDailyQuests()
    {
        return _completedQuests.Where(q => q.Category == QuestCategory.Daily);
    }

    // ===== Skills (v0.4.3c) =====

    /// <summary>
    /// Dictionary of player skills keyed by skill ID.
    /// </summary>
    private readonly Dictionary<string, PlayerSkill> _skills = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the player's skills collection.
    /// </summary>
    public IReadOnlyDictionary<string, PlayerSkill> Skills => _skills;

    /// <summary>
    /// Gets a skill by ID.
    /// </summary>
    /// <param name="skillId">The skill ID.</param>
    /// <returns>The player skill, or null if not found.</returns>
    public PlayerSkill? GetSkill(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
            return null;

        return _skills.TryGetValue(skillId, out var skill) ? skill : null;
    }

    /// <summary>
    /// Adds or updates a skill.
    /// </summary>
    /// <param name="skill">The skill to add.</param>
    public void AddSkill(PlayerSkill skill)
    {
        ArgumentNullException.ThrowIfNull(skill);
        _skills[skill.SkillId] = skill;
    }

    /// <summary>
    /// Checks if the player has a skill.
    /// </summary>
    /// <param name="skillId">The skill ID.</param>
    /// <returns>True if the skill exists.</returns>
    public bool HasSkill(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
            return false;

        return _skills.ContainsKey(skillId);
    }

    /// <summary>
    /// Gets the proficiency level for a skill.
    /// </summary>
    /// <param name="skillId">The skill ID.</param>
    /// <returns>The proficiency, or Untrained if not found.</returns>
    public SkillProficiency GetSkillProficiency(string skillId)
    {
        var skill = GetSkill(skillId);
        return skill?.Proficiency ?? SkillProficiency.Untrained;
    }

    // ===== Vision & Light Properties (v0.4.3b) =====

    /// <summary>
    /// Gets the type of vision this player has.
    /// </summary>
    /// <remarks>
    /// Affects how the player perceives light levels.
    /// DarkVision allows seeing in darkness as if it were dim light.
    /// TrueSight penetrates all darkness including magical.
    /// </remarks>
    public VisionType VisionType { get; private set; } = VisionType.Normal;

    /// <summary>
    /// Gets the player's currently active light source (if any).
    /// </summary>
    public LightSource? ActiveLightSource { get; private set; }

    /// <summary>
    /// Sets the vision type for this player.
    /// </summary>
    /// <param name="visionType">The vision type to set.</param>
    public void SetVisionType(VisionType visionType) => VisionType = visionType;

    /// <summary>
    /// Sets the player's active light source.
    /// </summary>
    /// <param name="lightSource">The light source to set as active, or null to clear.</param>
    public void SetActiveLightSource(LightSource? lightSource) => ActiveLightSource = lightSource;

    /// <summary>
    /// Gets the effective light level considering vision type.
    /// </summary>
    /// <param name="room">The room the player is in.</param>
    /// <returns>The perceived light level.</returns>
    /// <remarks>
    /// DarkVision: Dark → Dim
    /// TrueSight: All → Bright
    /// </remarks>
    public LightLevel GetEffectiveLightLevel(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        var roomLight = room.CurrentLightLevel;

        return VisionType switch
        {
            VisionType.TrueSight => LightLevel.Bright,
            VisionType.DarkVision when roomLight == LightLevel.Dark => LightLevel.Dim,
            _ => roomLight
        };
    }

    // ===== Combat Grid Movement Properties (v0.5.0b) =====

    /// <summary>
    /// Gets or sets the base movement speed (from configuration/class).
    /// </summary>
    /// <remarks>
    /// Speed 4 = 8 movement points per turn (4 cardinal moves).
    /// </remarks>
    public int BaseMovementSpeed { get; private set; } = 4;

    /// <summary>
    /// Gets the current movement speed (with modifiers applied).
    /// </summary>
    /// <remarks>
    /// Future: Will include modifiers from equipment, buffs, etc.
    /// </remarks>
    public int MovementSpeed => BaseMovementSpeed;

    /// <summary>
    /// Gets the current combat grid position (null if not in combat).
    /// </summary>
    public GridPosition? CombatGridPosition { get; private set; }

    /// <summary>
    /// Gets remaining movement points this turn.
    /// </summary>
    public int MovementPointsRemaining { get; private set; }

    /// <summary>
    /// Sets the base movement speed.
    /// </summary>
    /// <param name="speed">The new base movement speed (minimum 1).</param>
    public void SetMovementSpeed(int speed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(speed, 1);
        BaseMovementSpeed = speed;
    }

    /// <summary>
    /// Sets the combat grid position.
    /// </summary>
    /// <param name="position">The grid position, or null to clear.</param>
    public void SetCombatGridPosition(GridPosition? position)
    {
        CombatGridPosition = position;
    }

    /// <summary>
    /// Resets movement points to full (called at start of turn).
    /// </summary>
    public void ResetMovementPoints()
    {
        MovementPointsRemaining = MovementCosts.SpeedToPoints(MovementSpeed);
    }

    /// <summary>
    /// Uses movement points for a move action.
    /// </summary>
    /// <param name="cost">The movement point cost.</param>
    /// <returns><c>true</c> if successful; <c>false</c> if insufficient points.</returns>
    public bool UseMovementPoints(int cost)
    {
        if (cost > MovementPointsRemaining)
            return false;
        MovementPointsRemaining -= cost;
        return true;
    }

    /// <summary>
    /// Gets movement points remaining as a display-friendly value.
    /// </summary>
    /// <returns>Movement points converted to display speed (points / 2).</returns>
    public float GetDisplayMovementRemaining() =>
        MovementPointsRemaining / (float)MovementCosts.PointMultiplier;

    /// <summary>
    /// Returns a string representation of this player.
    /// </summary>
    /// <returns>A string containing the player name and current/max health.</returns>
    public override string ToString() => $"{Name} (HP: {Health}/{Stats.MaxHealth})";

    // ===== Facing Properties (v0.5.3a) =====

    /// <summary>
    /// Gets the direction this player is facing.
    /// </summary>
    /// <remarks>
    /// Facing affects flanking calculations and opportunity attacks.
    /// Players default to facing South (toward enemies).
    /// </remarks>
    public FacingDirection Facing { get; private set; } = FacingDirection.South;

    // ===== Talent Point Properties (v0.10.2b) =====

    /// <summary>
    /// Gets the talent points available to spend.
    /// </summary>
    /// <remarks>
    /// Points are earned on level-up and spent on talent tree nodes.
    /// </remarks>
    public int UnspentTalentPoints { get; private set; }

    /// <summary>
    /// Gets the total talent points ever earned by this player.
    /// </summary>
    /// <remarks>
    /// This value only increases (never decreases, even when points are spent).
    /// Used to track progression and validate consistency.
    /// </remarks>
    public int TotalTalentPointsEarned { get; private set; }

    /// <summary>
    /// Backing field for talent allocations.
    /// </summary>
    private readonly List<TalentAllocation> _talentAllocations = [];

    /// <summary>
    /// Gets the player's current talent allocations.
    /// </summary>
    /// <remarks>
    /// Each allocation represents points invested in a talent tree node.
    /// </remarks>
    public IReadOnlyList<TalentAllocation> TalentAllocations => _talentAllocations;

    /// <summary>
    /// Sets the player's facing direction.
    /// </summary>
    /// <param name="direction">The direction to face.</param>
    public void SetFacing(FacingDirection direction)
    {
        Facing = direction;
    }

    /// <summary>
    /// Faces the player toward a target position.
    /// </summary>
    /// <param name="targetPosition">The target position to face toward.</param>
    /// <param name="currentPosition">The player's current grid position.</param>
    /// <remarks>
    /// Calculates the direction from current position to target and sets facing.
    /// If positions are identical, facing remains unchanged.
    /// </remarks>
    public void FaceToward(GridPosition targetPosition, GridPosition currentPosition)
    {
        if (targetPosition == currentPosition)
            return;

        Facing = Extensions.FacingDirectionExtensions.GetDirectionTo(currentPosition, targetPosition);
    }

    // ===== Talent Point Methods (v0.10.2b) =====

    /// <summary>
    /// Adds talent points to the player.
    /// </summary>
    /// <param name="count">The number of points to add (must be positive).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is not positive.</exception>
    /// <remarks>
    /// Increases both UnspentTalentPoints and TotalTalentPointsEarned.
    /// </remarks>
    public void AddTalentPoints(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count, nameof(count));
        UnspentTalentPoints += count;
        TotalTalentPointsEarned += count;
    }

    /// <summary>
    /// Spends talent points from the player's unspent pool.
    /// </summary>
    /// <param name="count">The number of points to spend (must be positive).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is not positive.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the player has insufficient points.</exception>
    /// <remarks>
    /// Only decreases UnspentTalentPoints (TotalTalentPointsEarned remains unchanged).
    /// </remarks>
    public void SpendTalentPoints(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count, nameof(count));
        if (count > UnspentTalentPoints)
        {
            throw new InvalidOperationException(
                $"Cannot spend {count} talent points, only have {UnspentTalentPoints}");
        }
        UnspentTalentPoints -= count;
    }

    /// <summary>
    /// Adds a new talent allocation to the player.
    /// </summary>
    /// <param name="allocation">The allocation to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when allocation is null.</exception>
    /// <remarks>
    /// Does not validate for duplicates - caller should ensure the node
    /// is not already allocated before calling this method.
    /// </remarks>
    public void AddTalentAllocation(TalentAllocation allocation)
    {
        ArgumentNullException.ThrowIfNull(allocation, nameof(allocation));
        _talentAllocations.Add(allocation);
    }

    /// <summary>
    /// Clears all talent allocations (used for respec operations).
    /// </summary>
    /// <remarks>
    /// This only clears the allocations list. Call RefundAllTalentPoints()
    /// before this method to restore spent points to UnspentTalentPoints.
    /// </remarks>
    public void ClearTalentAllocations()
    {
        _talentAllocations.Clear();
    }

    /// <summary>
    /// Refunds all spent talent points back to the unspent pool.
    /// </summary>
    /// <remarks>
    /// Calculates total spent from all allocations and adds it back to UnspentTalentPoints.
    /// This should typically be called before ClearTalentAllocations() during a respec.
    /// </remarks>
    public void RefundAllTalentPoints()
    {
        var totalSpent = _talentAllocations.Sum(a => a.GetTotalPointsSpent());
        UnspentTalentPoints += totalSpent;
    }

    /// <summary>
    /// Gets the allocation for a specific node.
    /// </summary>
    /// <param name="nodeId">The node ID to look up.</param>
    /// <returns>The TalentAllocation if found, null otherwise.</returns>
    /// <remarks>
    /// Lookup is case-insensitive.
    /// </remarks>
    public TalentAllocation? GetAllocation(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        return _talentAllocations.FirstOrDefault(a =>
            a.NodeId.Equals(nodeId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the player has any allocation in a specific node.
    /// </summary>
    /// <param name="nodeId">The node ID to check.</param>
    /// <returns>True if the player has invested at least 1 rank in the node.</returns>
    public bool HasAllocation(string nodeId)
    {
        return GetAllocation(nodeId) != null;
    }

    // ===== Recipe Book Methods (v0.11.1b) =====

    /// <summary>
    /// Ensures the player has a recipe book (for existing players loaded from database).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is used to handle backwards compatibility with existing player
    /// data that was created before the recipe book system was introduced.
    /// </para>
    /// <para>
    /// If the player already has a recipe book, this method does nothing.
    /// Otherwise, it creates a new empty recipe book for the player.
    /// </para>
    /// </remarks>
    public void EnsureRecipeBook()
    {
        RecipeBook ??= RecipeBook.Create(Id);
    }

    // ===== Statistics Methods (v0.12.0a) =====

    /// <summary>
    /// Initializes the player's statistics entity.
    /// </summary>
    /// <param name="statistics">The statistics entity to associate with this player.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="statistics"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the statistics entity's PlayerId doesn't match this player's Id.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is typically called by the StatisticsService
    /// when statistics are first recorded for a player. It should only be called once
    /// per player instance.
    /// </para>
    /// <para>
    /// The statistics entity must have been created for this specific player
    /// (PlayerId must match).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = PlayerStatistics.Create(player.Id);
    /// player.InitializeStatistics(stats);
    /// </code>
    /// </example>
    public void InitializeStatistics(PlayerStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics, nameof(statistics));

        if (statistics.PlayerId != Id)
        {
            throw new ArgumentException(
                "Statistics PlayerId must match player Id",
                nameof(statistics));
        }

        Statistics = statistics;
    }

    /// <summary>
    /// Ensures the player has a statistics entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is used to handle backwards compatibility with existing player
    /// data that was created before the statistics system was introduced (v0.12.0a).
    /// </para>
    /// <para>
    /// If the player already has a statistics entity, this method does nothing.
    /// Otherwise, it creates a new statistics entity for the player.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // When loading a player from database
    /// var player = await repository.GetPlayerAsync(playerId);
    /// player.EnsureStatistics();
    /// </code>
    /// </example>
    public void EnsureStatistics()
    {
        Statistics ??= PlayerStatistics.Create(Id);
    }

    // ===== Dice History Properties (v0.12.0b) =====

    /// <summary>
    /// Gets the player's dice roll history tracking entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The DiceRollHistory entity tracks all dice rolls made by the player,
    /// including d20 averages, natural 20/1 counts, and streak information.
    /// This property may be null if the player hasn't made any dice rolls yet.
    /// </para>
    /// <para>
    /// Use <see cref="EnsureDiceHistory"/> or the DiceHistoryService
    /// to ensure the dice history entity exists.
    /// </para>
    /// </remarks>
    public DiceRollHistory? DiceHistory { get; private set; }

    /// <summary>
    /// Initializes the player's dice roll history entity.
    /// </summary>
    /// <param name="history">The dice history entity to associate with this player.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="history"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the history entity's PlayerId doesn't match this player's Id.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is typically called by the DiceHistoryService
    /// when dice rolls are first recorded for a player. It should only be called once
    /// per player instance.
    /// </para>
    /// <para>
    /// The dice history entity must have been created for this specific player
    /// (PlayerId must match).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var history = DiceRollHistory.Create(player.Id);
    /// player.InitializeDiceHistory(history);
    /// </code>
    /// </example>
    public void InitializeDiceHistory(DiceRollHistory history)
    {
        ArgumentNullException.ThrowIfNull(history, nameof(history));

        if (history.PlayerId != Id)
        {
            throw new ArgumentException(
                "DiceHistory PlayerId must match player Id",
                nameof(history));
        }

        DiceHistory = history;
    }

    /// <summary>
    /// Ensures the player has a dice roll history entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is used to handle backwards compatibility with existing player
    /// data that was created before the dice history system was introduced (v0.12.0b).
    /// </para>
    /// <para>
    /// If the player already has a dice history entity, this method does nothing.
    /// Otherwise, it creates a new dice history entity for the player.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // When loading a player from database
    /// var player = await repository.GetPlayerAsync(playerId);
    /// player.EnsureDiceHistory();
    /// </code>
    /// </example>
    public void EnsureDiceHistory()
    {
        DiceHistory ??= DiceRollHistory.Create(Id);
    }

    // ===== Achievement Properties (v0.12.1b) =====

    /// <summary>
    /// Backing field for player achievements.
    /// </summary>
    /// <remarks>
    /// Achievements are stored as a list and exposed as read-only externally.
    /// Use <see cref="AddAchievement"/> to add new achievements.
    /// </remarks>
    private readonly List<PlayerAchievement> _achievements = new();

    /// <summary>
    /// Gets the achievements this player has unlocked.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a read-only view of the player's unlocked achievements.
    /// Each <see cref="PlayerAchievement"/> contains the achievement ID,
    /// unlock timestamp, and points awarded at unlock time.
    /// </para>
    /// <para>
    /// To unlock a new achievement, use <see cref="AddAchievement"/>.
    /// To check if an achievement is unlocked, use <see cref="HasAchievement"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Display all unlocked achievements
    /// foreach (var achievement in player.Achievements)
    /// {
    ///     Console.WriteLine($"{achievement.AchievementId}: +{achievement.PointsAwarded} pts");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyList<PlayerAchievement> Achievements => _achievements.AsReadOnly();

    /// <summary>
    /// Adds a new achievement to the player's collection.
    /// </summary>
    /// <param name="achievementId">
    /// The achievement definition ID. Must not be null, empty, or whitespace.
    /// Should correspond to a valid <see cref="AchievementDefinition.AchievementId"/>.
    /// </param>
    /// <param name="points">
    /// The points to award. Must be non-negative.
    /// Typically derived from the achievement's tier (Bronze=10, Silver=25, Gold=50, Platinum=100).
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="achievementId"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the achievement is already unlocked for this player.
    /// Check with <see cref="HasAchievement"/> before calling.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method creates a new <see cref="PlayerAchievement"/> record with the
    /// current UTC timestamp and adds it to the player's achievement collection.
    /// </para>
    /// <para>
    /// The points are captured at unlock time to preserve historical accuracy,
    /// even if the achievement definition's point value changes later.
    /// </para>
    /// <para>
    /// This method is typically called by the AchievementService when all
    /// conditions for an achievement have been met.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Unlock an achievement (typically done by AchievementService)
    /// if (!player.HasAchievement("first-blood"))
    /// {
    ///     player.AddAchievement("first-blood", 10);
    /// }
    /// </code>
    /// </example>
    public void AddAchievement(string achievementId, int points)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(achievementId, nameof(achievementId));

        if (HasAchievement(achievementId))
        {
            throw new InvalidOperationException(
                $"Achievement '{achievementId}' is already unlocked for this player.");
        }

        var achievement = PlayerAchievement.Create(achievementId, points);
        _achievements.Add(achievement);
    }

    /// <summary>
    /// Checks if the player has unlocked a specific achievement.
    /// </summary>
    /// <param name="achievementId">
    /// The achievement ID to check. Must not be null, empty, or whitespace.
    /// </param>
    /// <returns>
    /// <c>true</c> if the player has unlocked the achievement; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="achievementId"/> is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Use this method before calling <see cref="AddAchievement"/> to avoid
    /// the <see cref="InvalidOperationException"/> for duplicate unlocks.
    /// </para>
    /// <para>
    /// Achievement IDs are compared using ordinal case-sensitive comparison,
    /// so ensure consistency with the IDs used in achievement definitions.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (player.HasAchievement("monster-slayer"))
    /// {
    ///     Console.WriteLine("Already a Monster Slayer!");
    /// }
    /// </code>
    /// </example>
    public bool HasAchievement(string achievementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(achievementId, nameof(achievementId));
        return _achievements.Any(a => a.AchievementId == achievementId);
    }

    /// <summary>
    /// Gets the total achievement points earned by this player.
    /// </summary>
    /// <returns>
    /// The sum of all points from unlocked achievements.
    /// Returns 0 if no achievements have been unlocked.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This calculates the sum of <see cref="PlayerAchievement.PointsAwarded"/>
    /// from all unlocked achievements. Points are captured at unlock time,
    /// so this reflects historical accuracy.
    /// </para>
    /// <para>
    /// Use this for displaying total achievement points in the UI or
    /// for leaderboard calculations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var totalPoints = player.GetTotalAchievementPoints();
    /// Console.WriteLine($"Total Achievement Points: {totalPoints}");
    /// </code>
    /// </example>
    public int GetTotalAchievementPoints()
    {
        return _achievements.Sum(a => a.PointsAwarded);
    }

    // ===== Faction Knowledge Properties (v0.15.5f) =====

    /// <summary>
    /// Backing field for known faction IDs.
    /// </summary>
    /// <remarks>
    /// Uses case-insensitive comparison for faction ID matching.
    /// </remarks>
    private readonly HashSet<string> _knownFactionIds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the factions this player has encountered and learned about.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Known factions receive no DC penalty when interpreting their scavenger signs.
    /// Unknown factions impose a +4 DC penalty on sign interpretation checks.
    /// </para>
    /// <para>
    /// Major factions (Iron Covenant, Rust Walkers, Silent Ones, Verdant Circle, Ash-Born)
    /// are always considered known to all players and do not need to be in this collection.
    /// Use <see cref="KnowsFaction"/> to check if a faction is known, as it accounts for
    /// both learned factions and major factions.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check all learned factions (excluding major factions)
    /// foreach (var factionId in player.KnownFactions)
    /// {
    ///     Console.WriteLine($"Learned faction: {factionId}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlySet<string> KnownFactions => _knownFactionIds;

    /// <summary>
    /// The set of major faction IDs that are known to all players.
    /// </summary>
    /// <remarks>
    /// These factions have well-established sign systems that any wasteland survivor would recognize.
    /// </remarks>
    private static readonly HashSet<string> MajorFactionIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "iron-covenant",
        "rust-walkers",
        "silent-ones",
        "verdant-circle",
        "ash-born"
    };

    // ===== Faction Knowledge Methods (v0.15.5f) =====

    /// <summary>
    /// Adds a faction to the player's known factions list.
    /// </summary>
    /// <param name="factionId">
    /// The faction ID to add. Must not be null, empty, or whitespace.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="factionId"/> is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Once a faction is known, the player no longer receives a DC penalty
    /// when interpreting that faction's scavenger signs.
    /// </para>
    /// <para>
    /// Factions can be learned through:
    /// <list type="bullet">
    ///   <item><description>Successfully interpreting a sign from an unknown faction</description></item>
    ///   <item><description>Meeting faction members</description></item>
    ///   <item><description>Information gathering through dialogue</description></item>
    ///   <item><description>Finding faction-related items or documents</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Adding a major faction has no effect since they are always known.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Player learns about the Scrap Collectors faction
    /// player.AddKnownFaction("scrap-collectors");
    /// </code>
    /// </example>
    public void AddKnownFaction(string factionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(factionId, nameof(factionId));
        _knownFactionIds.Add(factionId);
    }

    /// <summary>
    /// Checks whether the player knows a specific faction.
    /// </summary>
    /// <param name="factionId">
    /// The faction ID to check. Returns false for null, empty, or whitespace.
    /// </param>
    /// <returns>
    /// <c>true</c> if the faction is known (either a major faction or learned);
    /// <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method checks both the player's learned factions AND the major factions list.
    /// Major factions are always known to all players.
    /// </para>
    /// <para>
    /// Use this method when calculating sign interpretation DCs:
    /// <list type="bullet">
    ///   <item><description>Known faction: No DC penalty</description></item>
    ///   <item><description>Unknown faction: +4 DC penalty</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check if faction is known for DC calculation
    /// var dcModifier = player.KnowsFaction(sign.FactionId) ? 0 : 4;
    /// var totalDc = sign.BaseDc + dcModifier;
    /// </code>
    /// </example>
    public bool KnowsFaction(string factionId)
    {
        if (string.IsNullOrWhiteSpace(factionId))
            return false;

        // Major factions are always known
        if (MajorFactionIds.Contains(factionId))
            return true;

        // Check learned factions
        return _knownFactionIds.Contains(factionId);
    }

    /// <summary>
    /// Checks whether a faction ID represents a major faction.
    /// </summary>
    /// <param name="factionId">The faction ID to check.</param>
    /// <returns>
    /// <c>true</c> if the faction is a major faction; <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// Major factions are well-established wasteland powers whose sign systems
    /// are known to all survivors. They include:
    /// <list type="bullet">
    ///   <item><description>iron-covenant - Iron Covenant</description></item>
    ///   <item><description>rust-walkers - Rust Walkers</description></item>
    ///   <item><description>silent-ones - Silent Ones</description></item>
    ///   <item><description>verdant-circle - Verdant Circle</description></item>
    ///   <item><description>ash-born - Ash-Born</description></item>
    /// </list>
    /// </remarks>
    public static bool IsMajorFaction(string factionId)
    {
        if (string.IsNullOrWhiteSpace(factionId))
            return false;

        return MajorFactionIds.Contains(factionId);
    }

    /// <summary>
    /// Gets the list of major faction IDs.
    /// </summary>
    /// <returns>A read-only collection of major faction identifiers.</returns>
    public static IReadOnlyCollection<string> GetMajorFactionIds() => MajorFactionIds;

    // ===== Lineage System Properties (v0.17.0f) =====

    /// <summary>
    /// Gets the player's selected lineage, or <c>null</c> if no lineage has been assigned.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lineage is set during character creation and represents the character's bloodline
    /// heritage from before the Great Silence. Once set, lineage cannot be changed.
    /// </para>
    /// <para>
    /// Use <see cref="HasLineage"/> to check whether a lineage has been assigned
    /// before attempting to read this value.
    /// </para>
    /// </remarks>
    /// <seealso cref="SetLineage"/>
    /// <seealso cref="HasLineage"/>
    public Lineage? SelectedLineage { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this player has a lineage assigned.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="SelectedLineage"/> has been set; otherwise, <c>false</c>.
    /// </value>
    public bool HasLineage => SelectedLineage.HasValue;

    /// <summary>
    /// Gets the lineage trait registered for this player, or <c>null</c> if none.
    /// </summary>
    /// <remarks>
    /// The lineage trait provides a signature ability unique to the character's bloodline.
    /// It is registered during lineage application via <see cref="RegisterLineageTrait"/>.
    /// </remarks>
    /// <seealso cref="RegisterLineageTrait"/>
    public LineageTrait? LineageTrait { get; private set; }

    /// <summary>
    /// Gets the player's current Corruption value from the Trauma Economy.
    /// </summary>
    /// <remarks>
    /// Corruption represents the taint of the Runic Blight on the character's soul.
    /// Some lineages (Rune-Marked) start with permanent Corruption that cannot be
    /// cleansed below the lineage baseline.
    /// </remarks>
    public int Corruption { get; private set; }

    /// <summary>
    /// Gets the player's current Psychic Stress value from the Trauma Economy.
    /// </summary>
    /// <remarks>
    /// Stress represents psychological trauma from exposure to the horrors of
    /// the post-Silence world.
    /// </remarks>
    public int PsychicStress { get; private set; }

    /// <summary>
    /// Gets the lineage-based modifier to Corruption resistance checks.
    /// </summary>
    /// <remarks>
    /// A negative value means the character is more susceptible to gaining Corruption.
    /// Rune-Marked has -1 to Corruption resistance.
    /// </remarks>
    public int CorruptionResistanceModifier { get; private set; }

    /// <summary>
    /// Gets the lineage-based modifier to Psychic Stress resistance checks.
    /// </summary>
    /// <remarks>
    /// A negative value means the character is more susceptible to gaining Stress.
    /// Iron-Blooded has -1 to Stress resistance.
    /// </remarks>
    public int StressResistanceModifier { get; private set; }

    /// <summary>
    /// Gets the lineage bonus modifier applied to Max HP.
    /// </summary>
    /// <remarks>
    /// This is separate from the base Max HP calculated from attributes.
    /// Clan-Born receives +5 Max HP from their lineage.
    /// </remarks>
    public int LineageMaxHpModifier { get; private set; }

    /// <summary>
    /// Gets the lineage bonus modifier applied to Max AP (Aether Points).
    /// </summary>
    /// <remarks>
    /// This is separate from the base Max AP calculated from attributes.
    /// Rune-Marked receives +5 Max AP from their lineage.
    /// </remarks>
    public int LineageMaxApModifier { get; private set; }

    /// <summary>
    /// Gets the lineage bonus modifier applied to Soak (damage absorption).
    /// </summary>
    /// <remarks>
    /// Iron-Blooded receives +2 Soak from their lineage.
    /// </remarks>
    public int LineageSoakModifier { get; private set; }

    // ===== Lineage System Methods (v0.17.0f) =====

    /// <summary>
    /// Sets the player's lineage. Can only be called once per character.
    /// </summary>
    /// <param name="lineage">The lineage to assign to this player.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a lineage has already been assigned to this player.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Lineage is a permanent choice made during character creation.
    /// This method enforces that it can only be set once by throwing
    /// if <see cref="HasLineage"/> is already <c>true</c>.
    /// </para>
    /// <para>
    /// This method only sets the lineage identifier. Use the
    /// ILineageApplicationService (in the Application layer) to apply all lineage
    /// components (attributes, bonuses, traits, trauma baseline).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var player = new Player("TestHero");
    /// player.SetLineage(Lineage.ClanBorn);
    /// // player.HasLineage == true
    /// // player.SelectedLineage == Lineage.ClanBorn
    /// </code>
    /// </example>
    public void SetLineage(Lineage lineage)
    {
        if (HasLineage)
        {
            throw new InvalidOperationException(
                $"Player '{Name}' already has lineage '{SelectedLineage}' assigned. " +
                "Lineage cannot be changed after character creation.");
        }

        SelectedLineage = lineage;
    }

    // ===== Background System (v0.17.1e) =====

    /// <summary>
    /// Gets the player's selected background, or <c>null</c> if none has been chosen.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Background represents the character's pre-Silence profession and determines
    /// starting skills and equipment. Once set, background cannot be changed.
    /// </para>
    /// <para>
    /// Use <see cref="HasBackground"/> to check whether a background has been assigned
    /// before attempting to read this value.
    /// </para>
    /// </remarks>
    /// <seealso cref="SetBackground"/>
    /// <seealso cref="HasBackground"/>
    public Background? SelectedBackground { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this player has a background assigned.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="SelectedBackground"/> has been set; otherwise, <c>false</c>.
    /// </value>
    public bool HasBackground => SelectedBackground.HasValue;

    /// <summary>
    /// Sets the player's background during character creation.
    /// </summary>
    /// <param name="background">The background to assign to the player.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the player already has a background assigned.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Background is a permanent choice made during character creation.
    /// This method enforces that it can only be set once by throwing
    /// if <see cref="HasBackground"/> is already <c>true</c>.
    /// </para>
    /// <para>
    /// This method only sets the background identifier. Use the
    /// IBackgroundApplicationService (in the Application layer) to apply all background
    /// grants (skill bonuses, starting equipment).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var player = new Player("TestHero");
    /// player.SetBackground(Background.VillageSmith);
    /// // player.HasBackground == true
    /// // player.SelectedBackground == Background.VillageSmith
    /// </code>
    /// </example>
    public void SetBackground(Background background)
    {
        if (HasBackground)
        {
            throw new InvalidOperationException(
                $"Player '{Name}' already has background '{SelectedBackground}' assigned. " +
                "Background cannot be changed after character creation.");
        }

        SelectedBackground = background;
    }

    /// <summary>
    /// Modifies a core attribute by the specified amount.
    /// </summary>
    /// <param name="attribute">The core attribute to modify.</param>
    /// <param name="amount">The amount to add (positive) or subtract (negative).</param>
    /// <remarks>
    /// <para>
    /// Creates a new <see cref="PlayerAttributes"/> instance with the modified value.
    /// Attribute values are clamped to the valid range (1-30) by the
    /// <see cref="PlayerAttributes.WithModifiers"/> method.
    /// </para>
    /// <para>
    /// Used by the LineageApplicationService to apply lineage attribute modifiers
    /// during character creation. For example, Rune-Marked applies +2 Will, -1 Sturdiness.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// player.ModifyAttribute(CoreAttribute.Will, 2);    // +2 Will
    /// player.ModifyAttribute(CoreAttribute.Sturdiness, -1); // -1 Sturdiness
    /// </code>
    /// </example>
    public void ModifyAttribute(CoreAttribute attribute, int amount)
    {
        if (amount == 0) return;

        // Map CoreAttribute to PlayerAttributes field name
        var attributeName = attribute switch
        {
            CoreAttribute.Might => "might",
            CoreAttribute.Finesse => "finesse",
            CoreAttribute.Wits => "wits",
            CoreAttribute.Will => "will",
            CoreAttribute.Sturdiness => "fortitude",
            _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute,
                $"Unknown core attribute: {attribute}")
        };

        var modifiers = new Dictionary<string, int> { { attributeName, amount } };
        Attributes = Attributes.WithModifiers(modifiers);
    }

    /// <summary>
    /// Modifies the lineage Max HP bonus by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the Max HP modifier.</param>
    /// <remarks>
    /// This modifier is additive to the base Max HP from attributes.
    /// Clan-Born lineage grants +5 Max HP through this modifier.
    /// </remarks>
    public void ModifyMaxHp(int amount)
    {
        LineageMaxHpModifier += amount;
    }

    /// <summary>
    /// Modifies the lineage Max AP bonus by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the Max AP modifier.</param>
    /// <remarks>
    /// This modifier is additive to the base Max AP from attributes.
    /// Rune-Marked lineage grants +5 Max AP through this modifier.
    /// </remarks>
    public void ModifyMaxAp(int amount)
    {
        LineageMaxApModifier += amount;
    }

    /// <summary>
    /// Modifies the lineage Soak bonus by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the Soak modifier.</param>
    /// <remarks>
    /// Soak reduces incoming physical damage.
    /// Iron-Blooded lineage grants +2 Soak through this modifier.
    /// </remarks>
    public void ModifySoak(int amount)
    {
        LineageSoakModifier += amount;
    }

    /// <summary>
    /// Modifies the base movement speed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to base movement speed.</param>
    /// <remarks>
    /// Vargr-Kin lineage grants +1 Movement through this modifier.
    /// Movement speed cannot go below 1.
    /// </remarks>
    public void ModifyMovement(int amount)
    {
        BaseMovementSpeed = Math.Max(1, BaseMovementSpeed + amount);
    }

    /// <summary>
    /// Modifies a skill bonus for the specified skill, adding it if not present.
    /// </summary>
    /// <param name="skillId">The skill identifier to modify.</param>
    /// <param name="amount">The bonus amount to apply.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="skillId"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// If the player already has the skill, this creates an updated skill with
    /// the added bonus. If the player does not have the skill, a new skill
    /// is added at the <see cref="SkillProficiency.Untrained"/> level with
    /// the specified bonus as a modifier.
    /// </para>
    /// <para>
    /// Used by the LineageApplicationService to grant lineage-based skill bonuses
    /// (e.g., Clan-Born gets +1 Social, Iron-Blooded gets +1 Craft).
    /// </para>
    /// </remarks>
    public void ModifySkill(string skillId, int amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));

        var normalizedId = skillId.ToLowerInvariant();

        if (!HasSkill(normalizedId))
        {
            // Add new skill at Untrained proficiency — the lineage bonus
            // is tracked as starting experience (amount points worth)
            var newSkill = PlayerSkill.Create(normalizedId, Id);
            _skills[normalizedId] = newSkill;
        }

        // For lineage bonuses, we record the bonus as starting experience
        // The actual modifier effect is handled by the skill check system
        // which reads LineagePassiveBonuses for bonus amounts
    }

    /// <summary>
    /// Registers a lineage trait for this player.
    /// </summary>
    /// <param name="trait">The lineage trait to register.</param>
    /// <remarks>
    /// <para>
    /// Each player can have at most one lineage trait, corresponding to their
    /// bloodline heritage. The trait provides conditional bonuses that activate
    /// under specific circumstances.
    /// </para>
    /// <para>
    /// The four lineage traits are:
    /// <list type="bullet">
    ///   <item><description>[Survivor's Resolve] - +1d10 to Rhetoric with Clan-Born NPCs</description></item>
    ///   <item><description>[Aether-Tainted] - +10% Maximum Aether Pool</description></item>
    ///   <item><description>[Hazard Acclimation] - +1d10 vs environmental hazards</description></item>
    ///   <item><description>[Primal Clarity] - -10% Psychic Stress from all sources</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// player.RegisterLineageTrait(LineageTrait.SurvivorsResolve);
    /// // player.LineageTrait.Value.TraitName == "[Survivor's Resolve]"
    /// </code>
    /// </example>
    public void RegisterLineageTrait(LineageTrait trait)
    {
        LineageTrait = trait;
    }

    /// <summary>
    /// Sets the Trauma Economy baseline values from lineage selection.
    /// </summary>
    /// <param name="startingCorruption">The starting Corruption value (permanent for some lineages).</param>
    /// <param name="startingStress">The starting Psychic Stress value.</param>
    /// <param name="corruptionResistModifier">Modifier to Corruption resistance checks.</param>
    /// <param name="stressResistModifier">Modifier to Stress resistance checks.</param>
    /// <remarks>
    /// <para>
    /// Sets both the current Corruption/Stress values and the resistance modifiers.
    /// For Rune-Marked, starting Corruption of 5 is permanent and cannot be cleansed
    /// below this value.
    /// </para>
    /// <para>
    /// Trauma baselines by lineage:
    /// <list type="bullet">
    ///   <item><description>Clan-Born: (0, 0, 0, 0) — No vulnerabilities</description></item>
    ///   <item><description>Rune-Marked: (5, 0, -1, 0) — Permanent Corruption, Corruption vulnerability</description></item>
    ///   <item><description>Iron-Blooded: (0, 0, 0, -1) — Stress vulnerability</description></item>
    ///   <item><description>Vargr-Kin: (0, 0, 0, 0) — No vulnerabilities</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Apply Rune-Marked trauma baseline
    /// player.SetTraumaBaseline(5, 0, -1, 0);
    /// </code>
    /// </example>
    public void SetTraumaBaseline(
        int startingCorruption,
        int startingStress,
        int corruptionResistModifier,
        int stressResistModifier)
    {
        Corruption = startingCorruption;
        PsychicStress = startingStress;
        CorruptionResistanceModifier = corruptionResistModifier;
        StressResistanceModifier = stressResistModifier;
    }

    /// <summary>
    /// Sets the player's current Psychic Stress value.
    /// </summary>
    /// <param name="stress">
    /// The new stress value. Must be in the range 0-100 inclusive.
    /// Values outside this range are clamped to [0, 100].
    /// </param>
    /// <remarks>
    /// <para>
    /// This method is the primary mutation point for the Psychic Stress system
    /// (v0.18.0d). It is called by <c>StressService</c> during stress application,
    /// recovery, and post-Trauma Check reset operations.
    /// </para>
    /// <para>
    /// The value is clamped rather than throwing to match the defensive behavior
    /// of <see cref="StressState.Create(int)"/>, which also clamps to [0, 100].
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// player.SetPsychicStress(50);  // Set to Anxious threshold
    /// player.SetPsychicStress(0);   // Reset to Calm
    /// player.SetPsychicStress(100); // Trauma threshold
    /// </code>
    /// </example>
    /// <seealso cref="PsychicStress"/>
    /// <seealso cref="SetTraumaBaseline"/>
    public void SetPsychicStress(int stress)
    {
        PsychicStress = Math.Clamp(stress, 0, 100);
    }

    // ===== Specialization System (v0.17.4e) =====

    /// <summary>
    /// Internal tracking of specialization assignments and unlocked tiers.
    /// Maps each <see cref="SpecializationId"/> to the set of tier numbers
    /// that have been unlocked (Tier 1 is automatically included when the
    /// specialization is added).
    /// </summary>
    private readonly Dictionary<SpecializationId, HashSet<int>> _specializations = new();

    /// <summary>
    /// Gets the player's available Progression Points (PP) for unlocking
    /// specializations and ability tiers.
    /// </summary>
    /// <value>
    /// The current PP balance. Starts at 0 for new characters.
    /// Earned through Saga progression milestones.
    /// </value>
    /// <remarks>
    /// <para>
    /// Progression Points are spent on:
    /// <list type="bullet">
    ///   <item><description>Additional specializations: 3 PP each</description></item>
    ///   <item><description>Tier 2 abilities: 2 PP per tier</description></item>
    ///   <item><description>Tier 3 abilities: 3 PP per tier</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Use <see cref="SpendProgressionPoints"/> to deduct PP and
    /// <see cref="SetProgressionPoints"/> for test setup.
    /// </para>
    /// </remarks>
    /// <seealso cref="SpendProgressionPoints"/>
    /// <seealso cref="ProgressionRank"/>
    public int ProgressionPoints { get; private set; }

    /// <summary>
    /// Gets the player's current progression rank, which gates ability tier unlocks.
    /// </summary>
    /// <value>
    /// The current rank. Starts at 1 for new characters. Tier 2 abilities
    /// require Rank 2; Tier 3 abilities require Rank 3.
    /// </value>
    /// <remarks>
    /// Use <see cref="SetProgressionRank"/> for test setup.
    /// </remarks>
    /// <seealso cref="ProgressionPoints"/>
    public int ProgressionRank { get; private set; } = 1;

    /// <summary>
    /// Gets the number of specializations the player has acquired.
    /// </summary>
    /// <value>
    /// The count of specializations. The first specialization is free
    /// during character creation; additional specializations cost 3 PP.
    /// </value>
    /// <seealso cref="AddSpecialization"/>
    /// <seealso cref="HasSpecialization"/>
    public int SpecializationCount => _specializations.Count;

    /// <summary>
    /// Gets the typed <see cref="Archetype"/> enum value parsed from <see cref="ArchetypeId"/>,
    /// or <c>null</c> if no archetype has been set or the ID cannot be parsed.
    /// </summary>
    /// <value>
    /// The <see cref="Archetype"/> enum value corresponding to the player's archetype,
    /// or <c>null</c> if <see cref="ArchetypeId"/> is not set or does not map to a valid enum value.
    /// </value>
    /// <remarks>
    /// Used by the SpecializationApplicationService to validate that a specialization's
    /// parent archetype matches the player's archetype during character creation Step 5.
    /// </remarks>
    /// <seealso cref="ArchetypeId"/>
    public Archetype? Archetype =>
        ArchetypeId != null && Enum.TryParse<Archetype>(ArchetypeId, ignoreCase: true, out var archetype)
            ? archetype
            : null;

    /// <summary>
    /// Adds a specialization to the player, automatically unlocking Tier 1.
    /// </summary>
    /// <param name="specializationId">The specialization to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the player already has this specialization.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Adding a specialization automatically registers Tier 1 as unlocked.
    /// The first specialization during character creation is free (0 PP);
    /// additional specializations cost 3 PP (handled by the application service).
    /// </para>
    /// <para>
    /// This method does not deduct PP or grant abilities — those operations
    /// are performed by the <c>ISpecializationApplicationService</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// player.AddSpecialization(SpecializationId.Berserkr);
    /// // player.HasSpecialization(SpecializationId.Berserkr) == true
    /// // player.HasUnlockedTier(SpecializationId.Berserkr, 1) == true
    /// // player.SpecializationCount == 1
    /// </code>
    /// </example>
    /// <seealso cref="HasSpecialization"/>
    /// <seealso cref="SpecializationCount"/>
    public void AddSpecialization(SpecializationId specializationId)
    {
        if (_specializations.ContainsKey(specializationId))
        {
            throw new InvalidOperationException(
                $"Player '{Name}' already has specialization '{specializationId}'. " +
                "Duplicate specializations are not allowed.");
        }

        // Tier 1 is automatically unlocked when the specialization is added
        _specializations[specializationId] = new HashSet<int> { 1 };
    }

    /// <summary>
    /// Checks whether the player has a specific specialization.
    /// </summary>
    /// <param name="specializationId">The specialization to check.</param>
    /// <returns>
    /// <c>true</c> if the player has this specialization; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// bool hasBerserkr = player.HasSpecialization(SpecializationId.Berserkr);
    /// </code>
    /// </example>
    /// <seealso cref="AddSpecialization"/>
    public bool HasSpecialization(SpecializationId specializationId)
    {
        return _specializations.ContainsKey(specializationId);
    }

    /// <summary>
    /// Deducts Progression Points from the player's balance.
    /// </summary>
    /// <param name="cost">The number of PP to spend (must be non-negative).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="cost"/> is negative.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the player does not have enough PP to cover the cost.
    /// </exception>
    /// <remarks>
    /// <para>
    /// PP costs in the specialization system:
    /// <list type="bullet">
    ///   <item><description>First specialization: 0 PP (free at character creation)</description></item>
    ///   <item><description>Additional specializations: 3 PP each</description></item>
    ///   <item><description>Tier 2 abilities: 2 PP per tier</description></item>
    ///   <item><description>Tier 3 abilities: 3 PP per tier</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// player.SetProgressionPoints(10);
    /// player.SpendProgressionPoints(3);
    /// // player.ProgressionPoints == 7
    /// </code>
    /// </example>
    /// <seealso cref="ProgressionPoints"/>
    public void SpendProgressionPoints(int cost)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cost);

        if (cost == 0) return;

        if (ProgressionPoints < cost)
        {
            throw new InvalidOperationException(
                $"Player '{Name}' has {ProgressionPoints} PP but requires {cost} PP. " +
                "Insufficient Progression Points.");
        }

        ProgressionPoints -= cost;
    }

    /// <summary>
    /// Checks whether a specific ability tier has been unlocked for a specialization.
    /// </summary>
    /// <param name="specializationId">The specialization to check.</param>
    /// <param name="tier">The tier number (1, 2, or 3).</param>
    /// <returns>
    /// <c>true</c> if the player has this specialization and the tier is unlocked;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// bool hasTier2 = player.HasUnlockedTier(SpecializationId.Berserkr, 2);
    /// </code>
    /// </example>
    /// <seealso cref="UnlockSpecializationTier"/>
    public bool HasUnlockedTier(SpecializationId specializationId, int tier)
    {
        return _specializations.TryGetValue(specializationId, out var tiers)
            && tiers.Contains(tier);
    }

    /// <summary>
    /// Marks a specialization tier as unlocked.
    /// </summary>
    /// <param name="specializationId">The specialization containing the tier.</param>
    /// <param name="tier">The tier number to unlock (2 or 3; Tier 1 is automatic).</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the player does not have the specialization or if the tier
    /// is already unlocked.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method only marks the tier as unlocked — it does not deduct PP
    /// or grant abilities. Those operations are performed by the
    /// <c>ISpecializationApplicationService.UnlockTier</c> method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// player.UnlockSpecializationTier(SpecializationId.Berserkr, 2);
    /// // player.HasUnlockedTier(SpecializationId.Berserkr, 2) == true
    /// </code>
    /// </example>
    /// <seealso cref="HasUnlockedTier"/>
    public void UnlockSpecializationTier(SpecializationId specializationId, int tier)
    {
        if (!_specializations.TryGetValue(specializationId, out var tiers))
        {
            throw new InvalidOperationException(
                $"Player '{Name}' does not have specialization '{specializationId}'. " +
                "Cannot unlock tier for a specialization the player does not have.");
        }

        if (tiers.Contains(tier))
        {
            throw new InvalidOperationException(
                $"Player '{Name}' already has tier {tier} unlocked for '{specializationId}'.");
        }

        tiers.Add(tier);
    }

    /// <summary>
    /// Grants an ability to the player by creating a <see cref="PlayerAbility"/> instance.
    /// </summary>
    /// <param name="abilityId">The ability definition ID to grant.</param>
    /// <remarks>
    /// <para>
    /// Convenience wrapper around <see cref="AddAbility(PlayerAbility)"/> that creates
    /// a <see cref="PlayerAbility"/> via <see cref="PlayerAbility.Create(string, bool)"/>
    /// with <c>isUnlocked: true</c>.
    /// </para>
    /// <para>
    /// If the player already has the ability, it is silently replaced (upsert behavior
    /// inherited from the <see cref="Abilities"/> dictionary).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// player.GrantAbility("rage-strike");
    /// // player.HasAbility("rage-strike") == true
    /// </code>
    /// </example>
    /// <seealso cref="AddAbility"/>
    /// <seealso cref="HasAbility"/>
    public void GrantAbility(string abilityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId);
        var playerAbility = PlayerAbility.Create(abilityId, isUnlocked: true);
        AddAbility(playerAbility);
    }

    /// <summary>
    /// Initializes a special resource from a <see cref="SpecialResourceDefinition"/>.
    /// </summary>
    /// <param name="resource">The special resource definition to initialize.</param>
    /// <remarks>
    /// <para>
    /// Convenience wrapper around <see cref="InitializeResource(string, int, bool)"/>
    /// that extracts the resource ID, max value, and starting behavior from the
    /// definition value object.
    /// </para>
    /// <para>
    /// Special resources by specialization:
    /// <list type="bullet">
    ///   <item><description>Berserkr: Rage (0-100, starts at 0)</description></item>
    ///   <item><description>Skjaldmaer: Block Charges (0-3, starts at 3)</description></item>
    ///   <item><description>Iron-Bane: Righteous Fervor (0-50, starts at 0)</description></item>
    ///   <item><description>Seiðkona: Aether Resonance (0-10, starts at 0)</description></item>
    ///   <item><description>Echo-Caller: Echoes (0-5, starts at 0)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var rageResource = SpecialResourceDefinition.Create("rage", "Rage", 0, 100, 0, 0, 5, "...");
    /// player.InitializeSpecialResource(rageResource);
    /// // player.HasResource("rage") == true
    /// </code>
    /// </example>
    /// <seealso cref="InitializeResource"/>
    public void InitializeSpecialResource(SpecialResourceDefinition resource)
    {
        if (!resource.HasResource) return;

        InitializeResource(
            resource.ResourceId,
            resource.MaxValue,
            startAtZero: resource.StartsAt == 0);
    }

    /// <summary>
    /// Sets the player's Progression Points balance directly.
    /// </summary>
    /// <param name="pp">The PP value to set.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="pp"/> is negative.
    /// </exception>
    /// <remarks>
    /// Intended for test setup and character initialization.
    /// During gameplay, PP are earned through Saga progression and
    /// spent via <see cref="SpendProgressionPoints"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// player.SetProgressionPoints(10);
    /// // player.ProgressionPoints == 10
    /// </code>
    /// </example>
    public void SetProgressionPoints(int pp)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pp);
        ProgressionPoints = pp;
    }

    /// <summary>
    /// Sets the player's progression rank directly.
    /// </summary>
    /// <param name="rank">The rank value to set (must be at least 1).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="rank"/> is less than 1.
    /// </exception>
    /// <remarks>
    /// Intended for test setup and character initialization.
    /// During gameplay, rank increases are earned through Saga progression.
    /// </remarks>
    /// <example>
    /// <code>
    /// player.SetProgressionRank(3);
    /// // player.ProgressionRank == 3
    /// </code>
    /// </example>
    public void SetProgressionRank(int rank)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rank, 1);
        ProgressionRank = rank;
    }
}

