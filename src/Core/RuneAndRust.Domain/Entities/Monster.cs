using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a hostile creature in the dungeon that can engage in combat with players.
/// </summary>
/// <remarks>
/// Monsters have their own health, attack, and defense stats. They can take damage during
/// combat and are defeated when their health reaches zero.
/// </remarks>
public class Monster : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this monster.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the display name of this monster.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description of this monster shown to players.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the monster's current health points.
    /// </summary>
    public int Health { get; private set; }

    /// <summary>
    /// Gets the monster's maximum health points.
    /// </summary>
    public int MaxHealth { get; private set; }

    /// <summary>
    /// Gets the monster's combat statistics (max health, attack, defense).
    /// </summary>
    public Stats Stats { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this monster has been defeated (health is zero or less).
    /// </summary>
    public bool IsDefeated => Health <= 0;

    /// <summary>
    /// Gets a value indicating whether this monster is still alive (health greater than zero).
    /// </summary>
    public bool IsAlive => Health > 0;

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Monster()
    {
        Name = null!;
        Description = null!;
    }

    /// <summary>
    /// Creates a new monster with the specified properties.
    /// </summary>
    /// <param name="name">The display name of the monster.</param>
    /// <param name="description">The description shown to players.</param>
    /// <param name="maxHealth">The maximum health points of the monster.</param>
    /// <param name="stats">The combat statistics for the monster.</param>
    /// <exception cref="ArgumentNullException">Thrown when name or description is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxHealth is not positive.</exception>
    public Monster(string name, string description, int maxHealth, Stats stats)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        MaxHealth = maxHealth > 0 ? maxHealth : throw new ArgumentOutOfRangeException(nameof(maxHealth));
        Health = maxHealth;
        Stats = stats;
    }

    /// <summary>
    /// Applies damage to the monster, reduced by their defense stat.
    /// </summary>
    /// <param name="damage">The raw damage amount before defense reduction.</param>
    /// <returns>The actual damage dealt after defense calculation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when damage is negative.</exception>
    /// <remarks>
    /// Damage is reduced by the monster's defense stat (minimum 0 damage taken).
    /// The monster's health cannot go below zero.
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
    /// Factory method to create a basic goblin enemy.
    /// </summary>
    /// <returns>A new goblin monster.</returns>
    public static Monster CreateGoblin() => new(
        "Goblin",
        "A small, green creature with sharp teeth and beady eyes. It looks hostile.",
        30,
        new Stats(30, 8, 2)
    );

    /// <summary>
    /// Returns a string representation of this monster.
    /// </summary>
    /// <returns>A string containing the monster name and current/max health.</returns>
    public override string ToString() => $"{Name} (HP: {Health}/{MaxHealth})";
}
