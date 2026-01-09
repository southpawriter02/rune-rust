namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a monster difficulty tier with stat and reward multipliers.
/// </summary>
/// <remarks>
/// Tiers modify base monster stats when spawning, creating varied
/// difficulty levels from common enemies to powerful bosses.
/// </remarks>
public class TierDefinition
{
    /// <summary>
    /// Gets the unique identifier for this tier.
    /// </summary>
    /// <example>"common", "named", "elite", "boss"</example>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of this tier.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional prefix added to monster names.
    /// </summary>
    /// <remarks>
    /// Null for Common and Named tiers (Named uses generated names instead).
    /// "Elite" prefix for Elite tier, "Boss" prefix for Boss tier.
    /// </remarks>
    /// <example>null, "Elite", "Boss"</example>
    public string? NamePrefix { get; init; }

    /// <summary>
    /// Gets the health multiplier applied to base health.
    /// </summary>
    public float HealthMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Gets the attack multiplier applied to base attack.
    /// </summary>
    public float AttackMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Gets the defense multiplier applied to base defense.
    /// </summary>
    public float DefenseMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Gets the experience multiplier applied to base XP reward.
    /// </summary>
    public float ExperienceMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Gets the loot multiplier applied to drop rates and currency.
    /// </summary>
    /// <remarks>
    /// Used in v0.0.9d for loot table calculations.
    /// </remarks>
    public float LootMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Gets the color used for displaying this tier.
    /// </summary>
    /// <remarks>
    /// Used by the renderer for colored monster names.
    /// Should be a valid Spectre.Console color name.
    /// </remarks>
    public string Color { get; init; } = "white";

    /// <summary>
    /// Gets the relative spawn weight for random tier selection.
    /// </summary>
    /// <remarks>
    /// Higher weights mean more frequent spawns.
    /// Boss tier typically has very low or zero weight (spawned manually).
    /// </remarks>
    public int SpawnWeight { get; init; } = 100;

    /// <summary>
    /// Gets whether this tier generates unique names.
    /// </summary>
    /// <remarks>
    /// When true, monsters get procedurally generated names like "Grok the Orc".
    /// Typically only true for the "named" tier.
    /// </remarks>
    public bool GeneratesUniqueName { get; init; } = false;

    /// <summary>
    /// Gets the display sort order.
    /// </summary>
    public int SortOrder { get; init; } = 0;

    /// <summary>
    /// Private parameterless constructor for JSON deserialization.
    /// </summary>
    private TierDefinition()
    {
    }

    /// <summary>
    /// Creates a validated TierDefinition.
    /// </summary>
    /// <param name="id">The unique identifier (will be normalized to lowercase).</param>
    /// <param name="name">The display name.</param>
    /// <param name="namePrefix">Optional prefix for monster names.</param>
    /// <param name="healthMultiplier">Health multiplier (must be > 0).</param>
    /// <param name="attackMultiplier">Attack multiplier (must be > 0).</param>
    /// <param name="defenseMultiplier">Defense multiplier (must be >= 0).</param>
    /// <param name="experienceMultiplier">XP multiplier (must be >= 0).</param>
    /// <param name="lootMultiplier">Loot multiplier (must be >= 0).</param>
    /// <param name="color">Spectre.Console color name.</param>
    /// <param name="spawnWeight">Spawn weight for random selection.</param>
    /// <param name="generatesUniqueName">Whether to generate procedural names.</param>
    /// <param name="sortOrder">Display sort order.</param>
    /// <returns>A new validated TierDefinition.</returns>
    /// <exception cref="ArgumentException">When id or name is null/empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When multipliers are invalid.</exception>
    public static TierDefinition Create(
        string id,
        string name,
        string? namePrefix = null,
        float healthMultiplier = 1.0f,
        float attackMultiplier = 1.0f,
        float defenseMultiplier = 1.0f,
        float experienceMultiplier = 1.0f,
        float lootMultiplier = 1.0f,
        string color = "white",
        int spawnWeight = 100,
        bool generatesUniqueName = false,
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(healthMultiplier, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(attackMultiplier, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(defenseMultiplier);
        ArgumentOutOfRangeException.ThrowIfNegative(experienceMultiplier);
        ArgumentOutOfRangeException.ThrowIfNegative(lootMultiplier);

        return new TierDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            NamePrefix = namePrefix,
            HealthMultiplier = healthMultiplier,
            AttackMultiplier = attackMultiplier,
            DefenseMultiplier = defenseMultiplier,
            ExperienceMultiplier = experienceMultiplier,
            LootMultiplier = lootMultiplier,
            Color = string.IsNullOrWhiteSpace(color) ? "white" : color,
            SpawnWeight = Math.Max(0, spawnWeight),
            GeneratesUniqueName = generatesUniqueName,
            SortOrder = sortOrder
        };
    }

    /// <summary>
    /// Gets the default Common tier with 1.0x multipliers.
    /// </summary>
    public static TierDefinition Common => Create(
        "common",
        "Common",
        namePrefix: null,
        healthMultiplier: 1.0f,
        attackMultiplier: 1.0f,
        defenseMultiplier: 1.0f,
        experienceMultiplier: 1.0f,
        lootMultiplier: 1.0f,
        color: "white",
        spawnWeight: 70,
        generatesUniqueName: false,
        sortOrder: 0);
}
