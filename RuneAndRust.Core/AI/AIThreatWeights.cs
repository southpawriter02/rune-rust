namespace RuneAndRust.Core.AI;

/// <summary>
/// Configuration weights for threat assessment.
/// Different AI archetypes use different weights to prioritize threat factors.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public class AIThreatWeights
{
    /// <summary>
    /// Database ID.
    /// </summary>
    public int ArchetypeId { get; set; }

    /// <summary>
    /// AI archetype name (matches AIArchetype enum).
    /// </summary>
    public string ArchetypeName { get; set; } = string.Empty;

    /// <summary>
    /// Archetype enum value.
    /// </summary>
    public AIArchetype Archetype { get; set; }

    /// <summary>
    /// Weight for damage output factor (0.0 to 1.0).
    /// High value = prioritize high-damage targets.
    /// </summary>
    public decimal DamageWeight { get; set; }

    /// <summary>
    /// Weight for HP factor (0.0 to 1.0).
    /// High value = prioritize low-HP targets (finish wounded enemies).
    /// </summary>
    public decimal HPWeight { get; set; }

    /// <summary>
    /// Weight for positioning factor (0.0 to 1.0).
    /// High value = prioritize tactically vulnerable targets.
    /// </summary>
    public decimal PositionWeight { get; set; }

    /// <summary>
    /// Weight for abilities factor (0.0 to 1.0).
    /// High value = prioritize targets with dangerous abilities.
    /// </summary>
    public decimal AbilityWeight { get; set; }

    /// <summary>
    /// Weight for status effects factor (0.0 to 1.0).
    /// High value = prioritize buffed/debuffed targets.
    /// </summary>
    public decimal StatusWeight { get; set; }

    /// <summary>
    /// Created timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last updated timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
