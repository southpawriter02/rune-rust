using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.TestUtilities.Builders;

/// <summary>
/// Fluent builder for creating test Monster instances.
/// </summary>
/// <remarks>
/// Provides sensible defaults and preset monster types for common test scenarios.
/// </remarks>
public class MonsterBuilder
{
    private string _name = "Test Monster";
    private string _description = "A monster for testing.";
    private int _maxHealth = 50;
    private Stats _stats = new(50, 10, 5);
    private int? _currentHealth;
    private string? _definitionId;
    private int _experienceValue = 25;
    private int _initiativeModifier = 0;
    private AIBehavior _behavior = AIBehavior.Aggressive;
    private DamageResistances _resistances = DamageResistances.None;

    /// <summary>
    /// Creates a new MonsterBuilder with default values.
    /// </summary>
    public static MonsterBuilder Create() => new();

    /// <summary>
    /// Creates a builder preset for a goblin.
    /// </summary>
    public static MonsterBuilder Goblin() => Create()
        .WithName("Goblin")
        .WithDescription("A small, green creature with sharp teeth.")
        .WithDefinitionId("goblin")
        .WithStats(30, 8, 2)
        .WithExperienceValue(25)
        .WithInitiativeModifier(1)
        .WithBehavior(AIBehavior.Cowardly);

    /// <summary>
    /// Creates a builder preset for an orc.
    /// </summary>
    public static MonsterBuilder Orc() => Create()
        .WithName("Orc")
        .WithDescription("A hulking, brutish warrior.")
        .WithDefinitionId("orc")
        .WithStats(50, 12, 4)
        .WithExperienceValue(50)
        .WithInitiativeModifier(-1)
        .WithBehavior(AIBehavior.Aggressive);

    /// <summary>
    /// Creates a builder preset for a skeleton.
    /// </summary>
    public static MonsterBuilder Skeleton() => Create()
        .WithName("Skeleton")
        .WithDescription("An animated pile of bones.")
        .WithDefinitionId("skeleton")
        .WithStats(25, 6, 3)
        .WithExperienceValue(20)
        .WithBehavior(AIBehavior.Aggressive);

    /// <summary>
    /// Creates a builder preset for a slime.
    /// </summary>
    public static MonsterBuilder Slime() => Create()
        .WithName("Slime")
        .WithDescription("A gelatinous blob.")
        .WithDefinitionId("slime")
        .WithStats(40, 5, 5)
        .WithExperienceValue(15)
        .WithInitiativeModifier(-2)
        .WithBehavior(AIBehavior.Chaotic);

    /// <summary>
    /// Sets the monster name.
    /// </summary>
    public MonsterBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the monster description.
    /// </summary>
    public MonsterBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the monster stats using individual values.
    /// </summary>
    public MonsterBuilder WithStats(int maxHealth, int attack, int defense)
    {
        _maxHealth = maxHealth;
        _stats = new Stats(maxHealth, attack, defense);
        return this;
    }

    /// <summary>
    /// Sets the current health (monster will take damage to reach this value if below max).
    /// </summary>
    public MonsterBuilder WithCurrentHealth(int health)
    {
        _currentHealth = health;
        return this;
    }

    /// <summary>
    /// Sets the monster definition ID.
    /// </summary>
    public MonsterBuilder WithDefinitionId(string definitionId)
    {
        _definitionId = definitionId;
        return this;
    }

    /// <summary>
    /// Sets the experience value awarded when defeated.
    /// </summary>
    public MonsterBuilder WithExperienceValue(int xp)
    {
        _experienceValue = xp;
        return this;
    }

    /// <summary>
    /// Sets the initiative modifier.
    /// </summary>
    public MonsterBuilder WithInitiativeModifier(int modifier)
    {
        _initiativeModifier = modifier;
        return this;
    }

    /// <summary>
    /// Sets the AI behavior pattern.
    /// </summary>
    public MonsterBuilder WithBehavior(AIBehavior behavior)
    {
        _behavior = behavior;
        return this;
    }

    /// <summary>
    /// Sets the damage resistances.
    /// </summary>
    public MonsterBuilder WithResistances(DamageResistances resistances)
    {
        _resistances = resistances;
        return this;
    }

    /// <summary>
    /// Builds the Monster instance.
    /// </summary>
    public Monster Build()
    {
        var monster = new Monster(
            _name,
            _description,
            _maxHealth,
            _stats,
            _initiativeModifier,
            _definitionId,
            _experienceValue,
            _resistances);

        monster.SetBehavior(_behavior);

        // Apply damage to reach target health
        if (_currentHealth.HasValue && _currentHealth.Value < _maxHealth)
        {
            var damageNeeded = _maxHealth - _currentHealth.Value;
            // Apply raw damage (bypassing defense) to get to target health
            monster.TakeDamage(damageNeeded + _stats.Defense);
        }

        return monster;
    }
}
