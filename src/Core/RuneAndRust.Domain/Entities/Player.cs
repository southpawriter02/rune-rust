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
    /// Returns a string representation of this player.
    /// </summary>
    /// <returns>A string containing the player name and current/max health.</returns>
    public override string ToString() => $"{Name} (HP: {Health}/{Stats.MaxHealth})";
}
