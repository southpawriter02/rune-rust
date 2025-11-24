using System;
using System.Collections.Generic;

namespace RuneAndRust.Core.AI;

/// <summary>
/// Report summarizing AI decisions for a combat encounter.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public class AIDecisionReport
{
    /// <summary>
    /// Combat encounter ID.
    /// </summary>
    public Guid EncounterId { get; set; }

    /// <summary>
    /// Total number of AI decisions made.
    /// </summary>
    public int TotalDecisions { get; set; }

    /// <summary>
    /// Average decision time in milliseconds.
    /// </summary>
    public double AverageDecisionTimeMs { get; set; }

    /// <summary>
    /// Decisions grouped by archetype.
    /// </summary>
    public Dictionary<AIArchetype, int> DecisionsByArchetype { get; set; } = new();

    /// <summary>
    /// Most common targets (top 5).
    /// </summary>
    public Dictionary<string, int> MostCommonTargets { get; set; } = new();

    /// <summary>
    /// Ability usage frequency (top 10).
    /// </summary>
    public Dictionary<string, int> AbilityUsageFrequency { get; set; } = new();

    /// <summary>
    /// Number of intentional errors made (low intelligence).
    /// </summary>
    public int IntentionalErrors { get; set; }

    /// <summary>
    /// Average intelligence level for this encounter.
    /// </summary>
    public double AverageIntelligenceLevel { get; set; }

    /// <summary>
    /// Report generation timestamp.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
