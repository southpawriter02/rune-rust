using System.Collections.Generic;

namespace RuneAndRust.Core.AI;

/// <summary>
/// Configuration for a boss phase transition.
/// Defines what happens when a boss moves between phases.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public class BossPhaseTransition
{
    /// <summary>
    /// Database ID.
    /// </summary>
    public int TransitionId { get; set; }

    /// <summary>
    /// Boss type ID.
    /// </summary>
    public int BossTypeId { get; set; }

    /// <summary>
    /// Phase transitioning from.
    /// </summary>
    public BossPhase FromPhase { get; set; }

    /// <summary>
    /// Phase transitioning to.
    /// </summary>
    public BossPhase ToPhase { get; set; }

    /// <summary>
    /// HP percentage threshold to trigger transition (0.0 to 1.0).
    /// </summary>
    public decimal HPThreshold { get; set; }

    /// <summary>
    /// Dialogue displayed during transition.
    /// </summary>
    public string? TransitionDialogue { get; set; }

    /// <summary>
    /// Special ability ID used during transition (e.g., AOE knockback).
    /// </summary>
    public int? TransitionAbilityId { get; set; }

    /// <summary>
    /// JSON array of buffs/status effects applied in new phase.
    /// </summary>
    public string? PhaseBonuses { get; set; }

    /// <summary>
    /// Created timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
