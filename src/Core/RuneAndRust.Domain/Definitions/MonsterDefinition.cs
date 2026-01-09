using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a monster type that can be spawned in the game.
/// </summary>
/// <remarks>
/// Monster definitions are loaded from configuration and used to spawn
/// Monster instances with consistent stats and behaviors.
/// </remarks>
public class MonsterDefinition
{
    /// <summary>
    /// Gets the unique identifier for this monster type.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of this monster type.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description shown when examining this monster type.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the base health points for this monster type.
    /// </summary>
    public int BaseHealth { get; init; }

    /// <summary>
    /// Gets the base attack stat for this monster type.
    /// </summary>
    public int BaseAttack { get; init; }

    /// <summary>
    /// Gets the base defense stat for this monster type.
    /// </summary>
    public int BaseDefense { get; init; }

    /// <summary>
    /// Gets the experience points awarded when this monster is defeated.
    /// </summary>
    public int ExperienceValue { get; init; }

    /// <summary>
    /// Gets the AI behavior pattern for this monster type.
    /// </summary>
    public AIBehavior Behavior { get; init; } = AIBehavior.Aggressive;

    /// <summary>
    /// Gets the tags that categorize this monster type.
    /// </summary>
    /// <remarks>
    /// Tags are used for filtering monsters by category (e.g., "humanoid", "undead", "beast").
    /// </remarks>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets whether this monster type can heal itself or allies.
    /// </summary>
    public bool CanHeal { get; init; }

    /// <summary>
    /// Gets the heal amount if this monster type can heal.
    /// </summary>
    public int? HealAmount { get; init; }

    /// <summary>
    /// Gets the spawn weight for weighted random selection.
    /// </summary>
    /// <remarks>
    /// Higher weights increase the probability of this monster being selected.
    /// Default is 100.
    /// </remarks>
    public int SpawnWeight { get; init; } = 100;

    /// <summary>
    /// Gets the initiative modifier for combat turn order.
    /// </summary>
    /// <remarks>
    /// Higher values mean the monster acts earlier in combat.
    /// Default is 0.
    /// </remarks>
    public int InitiativeModifier { get; init; }

    /// <summary>
    /// Gets the base damage resistances for this monster type.
    /// </summary>
    /// <remarks>
    /// These resistances are copied to spawned Monster instances.
    /// Values are percentages from -100 (double damage) to +100 (immune).
    /// Can be modified by tiers or traits in future versions.
    /// </remarks>
    public DamageResistances BaseResistances { get; init; } = DamageResistances.None;

    // ===== Tier & Trait Properties (v0.0.9c) =====

    /// <summary>
    /// Gets the list of tier IDs this monster can spawn as.
    /// </summary>
    /// <remarks>
    /// If empty or null, defaults to ["common"].
    /// Tier is selected using weighted random from available tiers.
    /// </remarks>
    public IReadOnlyList<string> PossibleTiers { get; init; } = ["common"];

    /// <summary>
    /// Gets the list of trait IDs this monster can spawn with.
    /// </summary>
    /// <remarks>
    /// Traits are randomly selected based on tier.
    /// Higher tiers may have more traits.
    /// Empty list means no traits can be assigned.
    /// </remarks>
    public IReadOnlyList<string> PossibleTraits { get; init; } = [];

    /// <summary>
    /// Gets the name generator configuration for Named tier monsters.
    /// </summary>
    /// <remarks>
    /// If null, uses the default name generator.
    /// Only used when the selected tier has GeneratesUniqueName = true.
    /// </remarks>
    public NameGeneratorConfig? NameGenerator { get; init; }

    /// <summary>
    /// Private parameterless constructor for JSON deserialization.
    /// </summary>
    private MonsterDefinition()
    {
    }

    /// <summary>
    /// Creates a new monster definition with validation.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The description.</param>
    /// <param name="baseHealth">The base health points.</param>
    /// <param name="baseAttack">The base attack stat.</param>
    /// <param name="baseDefense">The base defense stat.</param>
    /// <param name="experienceValue">The XP awarded on defeat.</param>
    /// <param name="behavior">The AI behavior pattern.</param>
    /// <param name="tags">The categorization tags.</param>
    /// <param name="canHeal">Whether this monster can heal.</param>
    /// <param name="healAmount">The heal amount if applicable.</param>
    /// <param name="spawnWeight">The spawn weight for random selection.</param>
    /// <param name="initiativeModifier">The initiative modifier.</param>
    /// <param name="baseResistances">The base damage resistances.</param>
    /// <param name="possibleTiers">The tier IDs this monster can spawn as.</param>
    /// <param name="possibleTraits">The trait IDs this monster can have.</param>
    /// <param name="nameGenerator">Custom name generator configuration.</param>
    /// <returns>A new validated MonsterDefinition.</returns>
    /// <exception cref="ArgumentException">Thrown when id, name, or description is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when baseHealth is not positive.</exception>
    public static MonsterDefinition Create(
        string id,
        string name,
        string description,
        int baseHealth,
        int baseAttack,
        int baseDefense,
        int experienceValue = 0,
        AIBehavior behavior = AIBehavior.Aggressive,
        IReadOnlyList<string>? tags = null,
        bool canHeal = false,
        int? healAmount = null,
        int spawnWeight = 100,
        int initiativeModifier = 0,
        DamageResistances? baseResistances = null,
        IEnumerable<string>? possibleTiers = null,
        IEnumerable<string>? possibleTraits = null,
        NameGeneratorConfig? nameGenerator = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Monster definition ID cannot be null or empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Monster definition name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Monster definition description cannot be null or empty.", nameof(description));
        if (baseHealth <= 0)
            throw new ArgumentOutOfRangeException(nameof(baseHealth), "Base health must be positive.");

        return new MonsterDefinition
        {
            Id = id,
            Name = name,
            Description = description,
            BaseHealth = baseHealth,
            BaseAttack = Math.Max(0, baseAttack),
            BaseDefense = Math.Max(0, baseDefense),
            ExperienceValue = Math.Max(0, experienceValue),
            Behavior = behavior,
            Tags = tags ?? [],
            CanHeal = canHeal,
            HealAmount = canHeal ? healAmount : null,
            SpawnWeight = Math.Max(1, spawnWeight),
            InitiativeModifier = initiativeModifier,
            BaseResistances = baseResistances ?? DamageResistances.None,
            PossibleTiers = possibleTiers?.ToList() ?? ["common"],
            PossibleTraits = possibleTraits?.ToList() ?? [],
            NameGenerator = nameGenerator
        };
    }

    /// <summary>
    /// Creates a Monster instance from this definition.
    /// </summary>
    /// <returns>A new Monster with stats based on this definition.</returns>
    public Monster CreateMonster()
    {
        var stats = new Stats(BaseHealth, BaseAttack, BaseDefense);
        var monster = new Monster(
            Name,
            Description,
            BaseHealth,
            stats,
            InitiativeModifier,
            Id,
            ExperienceValue,
            BaseResistances);

        monster.SetBehavior(Behavior);

        if (CanHeal && HealAmount.HasValue)
        {
            monster.EnableHealing(HealAmount.Value);
        }

        return monster;
    }

    /// <summary>
    /// Checks if this monster definition has the specified tag.
    /// </summary>
    /// <param name="tag">The tag to check for.</param>
    /// <returns>True if the monster has the tag; otherwise, false.</returns>
    public bool HasTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return false;
        return Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if this monster definition has all of the specified tags.
    /// </summary>
    /// <param name="requiredTags">The tags to check for.</param>
    /// <returns>True if the monster has all tags; otherwise, false.</returns>
    public bool HasAllTags(IEnumerable<string> requiredTags)
    {
        if (requiredTags == null) return true;
        return requiredTags.All(HasTag);
    }

    /// <summary>
    /// Checks if this monster definition has any of the specified tags.
    /// </summary>
    /// <param name="tags">The tags to check for.</param>
    /// <returns>True if the monster has at least one tag; otherwise, false.</returns>
    public bool HasAnyTag(IEnumerable<string> tags)
    {
        if (tags == null) return false;
        return tags.Any(HasTag);
    }
}
