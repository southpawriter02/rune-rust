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
}
