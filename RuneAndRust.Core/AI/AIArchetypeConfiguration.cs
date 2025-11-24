namespace RuneAndRust.Core.AI;

/// <summary>
/// Configuration for an AI archetype defining behavior modifiers and preferences.
/// v0.42.2: Ability Usage & Behavior Patterns
/// </summary>
public class AIArchetypeConfiguration
{
    /// <summary>
    /// Database ID.
    /// </summary>
    public int ArchetypeId { get; set; }

    /// <summary>
    /// Archetype name (matches AIArchetype enum).
    /// </summary>
    public string ArchetypeName { get; set; } = string.Empty;

    /// <summary>
    /// Archetype enum value.
    /// </summary>
    public AIArchetype Archetype { get; set; }

    /// <summary>
    /// Human-readable description of the archetype.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Modifier for damage-dealing abilities (0.0 to 2.0).
    /// Higher values = prioritizes damage abilities more.
    /// </summary>
    public decimal DamageAbilityModifier { get; set; }

    /// <summary>
    /// Modifier for utility abilities (buffs, debuffs, CC) (0.0 to 2.0).
    /// Higher values = prioritizes utility abilities more.
    /// </summary>
    public decimal UtilityAbilityModifier { get; set; }

    /// <summary>
    /// Modifier for defensive abilities (heals, shields, defensive buffs) (0.0 to 2.0).
    /// Higher values = prioritizes defensive abilities more.
    /// </summary>
    public decimal DefensiveAbilityModifier { get; set; }

    /// <summary>
    /// Aggression level (1-5).
    /// 1 = Very passive, 5 = Extremely aggressive.
    /// </summary>
    public int AggressionLevel { get; set; }

    /// <summary>
    /// HP percentage at which to retreat (0.0 to 1.0).
    /// Null = never retreats.
    /// </summary>
    public decimal? RetreatThresholdHP { get; set; }

    /// <summary>
    /// Preferred combat range: "Melee", "Medium", "Long"
    /// </summary>
    public string PreferredRange { get; set; } = "Medium";

    /// <summary>
    /// Whether this archetype uses coordination with allies.
    /// </summary>
    public bool UsesCoordination { get; set; }

    /// <summary>
    /// Created timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
