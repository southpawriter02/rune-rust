using System.Collections.Generic;

namespace RuneAndRust.Core.AI;

/// <summary>
/// Context information for AI decision-making, used for debugging and analysis.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public class DecisionContext
{
    /// <summary>
    /// Intelligence level (0-5) for this decision.
    /// </summary>
    public int IntelligenceLevel { get; set; }

    /// <summary>
    /// Threat assessments for all potential targets.
    /// </summary>
    public List<ThreatAssessment> ThreatAssessments { get; set; } = new();

    /// <summary>
    /// Abilities available for selection.
    /// </summary>
    public List<int> AvailableAbilityIds { get; set; } = new();

    /// <summary>
    /// Human-readable reasoning for the decision.
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// Whether this decision was intentionally suboptimal (low intelligence error).
    /// </summary>
    public bool IsIntentionalError { get; set; }

    /// <summary>
    /// Type of error made, if any.
    /// </summary>
    public string? ErrorType { get; set; }

    /// <summary>
    /// Decision timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
