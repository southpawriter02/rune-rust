using System.Collections.Generic;

namespace RuneAndRust.Core.AI;

/// <summary>
/// Represents the result of a threat assessment on a target.
/// Contains the total threat score and individual factor scores.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public class ThreatAssessment
{
    /// <summary>
    /// The character being assessed.
    /// </summary>
    public object Target { get; set; } = null!;

    /// <summary>
    /// The character's ID (for polymorphic targets).
    /// </summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>
    /// The character's name.
    /// </summary>
    public string TargetName { get; set; } = string.Empty;

    /// <summary>
    /// The total threat score (sum of all weighted factors).
    /// Higher = more dangerous/higher priority target.
    /// </summary>
    public float TotalThreatScore { get; set; }

    /// <summary>
    /// Individual scores for each threat factor (before weighting).
    /// </summary>
    public Dictionary<ThreatFactor, float> FactorScores { get; set; } = new();

    /// <summary>
    /// Human-readable explanation of the threat assessment.
    /// Used for debugging and AI transparency.
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// The AI archetype that performed this assessment.
    /// </summary>
    public AIArchetype AssessorArchetype { get; set; }

    /// <summary>
    /// Timestamp of the assessment.
    /// </summary>
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}
