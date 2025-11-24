namespace RuneAndRust.Core.AI;

/// <summary>
/// Represents the score of an ability for AI decision-making.
/// v0.42.2: Ability Usage & Behavior Patterns
/// </summary>
public class AbilityScore
{
    /// <summary>
    /// The ability being scored.
    /// </summary>
    public object Ability { get; set; } = null!;

    /// <summary>
    /// The ability's ID or name.
    /// </summary>
    public string AbilityId { get; set; } = string.Empty;

    /// <summary>
    /// The ability's name.
    /// </summary>
    public string AbilityName { get; set; } = string.Empty;

    /// <summary>
    /// The total score for this ability (sum of all weighted factors).
    /// Higher = better choice.
    /// </summary>
    public float TotalScore { get; set; }

    /// <summary>
    /// Damage component score (0-100).
    /// Expected damage output.
    /// </summary>
    public float DamageScore { get; set; }

    /// <summary>
    /// Utility component score (0-50).
    /// Non-damage value (CC, buffs, debuffs).
    /// </summary>
    public float UtilityScore { get; set; }

    /// <summary>
    /// Efficiency component score (0-30).
    /// Resource cost vs benefit ratio.
    /// </summary>
    public float EfficiencyScore { get; set; }

    /// <summary>
    /// Situational component score (0-20).
    /// Contextual value for current battlefield state.
    /// </summary>
    public float SituationScore { get; set; }

    /// <summary>
    /// The archetype modifier applied to this score.
    /// </summary>
    public float ArchetypeModifier { get; set; } = 1.0f;

    /// <summary>
    /// Human-readable explanation of why this score was assigned.
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the scoring.
    /// </summary>
    public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
}
