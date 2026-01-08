using RuneAndRust.Domain.Enums;
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
    /// Gets the experience points awarded when this monster is defeated.
    /// </summary>
    /// <remarks>
    /// This value is added to the player's experience upon defeating the monster.
    /// Higher-level or more difficult monsters award more experience.
    /// </remarks>
    public int ExperienceValue { get; private set; }

    /// <summary>
    /// Gets the initiative modifier for combat turn order.
    /// </summary>
    /// <remarks>
    /// Higher values mean the monster acts earlier in combat.
    /// Typically based on creature agility/reflexes.
    /// Default is 0 for average creatures.
    /// </remarks>
    public int InitiativeModifier { get; private set; } = 0;

    /// <summary>
    /// Gets the definition ID this monster was created from.
    /// </summary>
    /// <remarks>
    /// Used for grouping duplicate monsters and loading configuration.
    /// Null for dynamically created monsters.
    /// </remarks>
    public string? MonsterDefinitionId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this monster has been defeated (health is zero or less).
    /// </summary>
    public bool IsDefeated => Health <= 0;

    /// <summary>
    /// Gets a value indicating whether this monster is still alive (health greater than zero).
    /// </summary>
    public bool IsAlive => Health > 0;

    // ===== AI Behavior Properties (v0.0.6b) =====

    /// <summary>
    /// Gets the AI behavior pattern for this monster.
    /// </summary>
    /// <remarks>
    /// Determines how the monster makes decisions during combat.
    /// Defaults to <see cref="AIBehavior.Aggressive"/> if not set.
    /// </remarks>
    public AIBehavior Behavior { get; private set; } = AIBehavior.Aggressive;

    /// <summary>
    /// Gets whether this monster can heal itself or allies.
    /// </summary>
    public bool CanHeal { get; private set; }

    /// <summary>
    /// Gets the heal amount if the monster can heal.
    /// </summary>
    public int? HealAmount { get; private set; }

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
    /// <param name="initiativeModifier">The initiative modifier (default 0).</param>
    /// <param name="monsterDefinitionId">The definition ID for grouping (optional).</param>
    /// <param name="experienceValue">The XP awarded when defeated (default 0).</param>
    /// <exception cref="ArgumentNullException">Thrown when name or description is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxHealth is not positive.</exception>
    public Monster(
        string name,
        string description,
        int maxHealth,
        Stats stats,
        int initiativeModifier = 0,
        string? monsterDefinitionId = null,
        int experienceValue = 0)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        MaxHealth = maxHealth > 0 ? maxHealth : throw new ArgumentOutOfRangeException(nameof(maxHealth));
        Health = maxHealth;
        Stats = stats;
        InitiativeModifier = initiativeModifier;
        MonsterDefinitionId = monsterDefinitionId;
        ExperienceValue = Math.Max(0, experienceValue);
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

    // ===== AI Behavior Methods (v0.0.6b) =====

    /// <summary>
    /// Sets the AI behavior for this monster.
    /// </summary>
    /// <param name="behavior">The behavior pattern to use.</param>
    public void SetBehavior(AIBehavior behavior)
    {
        Behavior = behavior;
    }

    /// <summary>
    /// Enables healing capability for this monster.
    /// </summary>
    /// <param name="healAmount">The amount of HP restored per heal action.</param>
    public void EnableHealing(int healAmount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(healAmount);
        CanHeal = true;
        HealAmount = healAmount;
    }

    /// <summary>
    /// Restores health to this monster (used for healing).
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    /// <returns>The actual amount healed (capped at MaxHealth).</returns>
    public int Heal(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        var actualHeal = Math.Min(amount, MaxHealth - Health);
        Health += actualHeal;
        return actualHeal;
    }

    /// <summary>
    /// Factory method to create a basic goblin enemy with Cowardly behavior.
    /// </summary>
    /// <returns>A new goblin monster with 25 XP value.</returns>
    public static Monster CreateGoblin()
    {
        return new(
            "Goblin",
            "A small, green creature with sharp teeth and beady eyes. It looks hostile.",
            30,
            new Stats(30, 8, 2),
            initiativeModifier: 1,
            monsterDefinitionId: "goblin",
            experienceValue: 25)
        {
            Behavior = AIBehavior.Cowardly
        };
    }

    /// <summary>
    /// Factory method to create a skeleton enemy with Aggressive behavior.
    /// </summary>
    /// <returns>A new skeleton monster with 20 XP value.</returns>
    public static Monster CreateSkeleton()
    {
        return new(
            "Skeleton",
            "An animated pile of bones held together by dark magic.",
            25,
            new Stats(25, 6, 3),
            initiativeModifier: 0,
            monsterDefinitionId: "skeleton",
            experienceValue: 20)
        {
            Behavior = AIBehavior.Aggressive
        };
    }

    /// <summary>
    /// Factory method to create an orc enemy with Aggressive behavior.
    /// </summary>
    /// <returns>A new orc monster with 40 XP value.</returns>
    public static Monster CreateOrc()
    {
        return new(
            "Orc",
            "A large, brutish creature with green skin and tusks. It wields a crude axe.",
            45,
            new Stats(45, 12, 4),
            initiativeModifier: -1,
            monsterDefinitionId: "orc",
            experienceValue: 40)
        {
            Behavior = AIBehavior.Aggressive
        };
    }

    /// <summary>
    /// Factory method to create a goblin shaman with Support behavior.
    /// </summary>
    /// <returns>A new goblin shaman monster with 30 XP value.</returns>
    public static Monster CreateGoblinShaman()
    {
        var shaman = new Monster(
            "Goblin Shaman",
            "A goblin adorned with crude fetishes and glowing runes.",
            25,
            new Stats(25, 6, 1),
            initiativeModifier: 2,
            monsterDefinitionId: "goblin_shaman",
            experienceValue: 30)
        {
            Behavior = AIBehavior.Support,
            CanHeal = true,
            HealAmount = 10
        };
        return shaman;
    }

    /// <summary>
    /// Factory method to create a slime enemy with Chaotic behavior.
    /// </summary>
    /// <returns>A new slime monster with 15 XP value.</returns>
    public static Monster CreateSlime()
    {
        return new(
            "Slime",
            "A gelatinous blob that oozes across the floor.",
            40,
            new Stats(40, 5, 5),
            initiativeModifier: -2,
            monsterDefinitionId: "slime",
            experienceValue: 15)
        {
            Behavior = AIBehavior.Chaotic
        };
    }

    /// <summary>
    /// Returns a string representation of this monster.
    /// </summary>
    /// <returns>A string containing the monster name and current/max health.</returns>
    public override string ToString() => $"{Name} (HP: {Health}/{MaxHealth})";
}
