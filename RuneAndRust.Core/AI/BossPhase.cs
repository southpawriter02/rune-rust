namespace RuneAndRust.Core.AI;

/// <summary>
/// Represents the current phase of a boss encounter.
/// Bosses change tactics and abilities based on HP thresholds.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public enum BossPhase
{
    /// <summary>
    /// Phase 1: Teaching Phase (100-66% HP)
    /// Standard abilities, predictable patterns, teaches mechanics.
    /// </summary>
    Phase1 = 1,

    /// <summary>
    /// Phase 2: Escalation Phase (66-33% HP)
    /// Increased aggression, new abilities, summons adds.
    /// </summary>
    Phase2 = 2,

    /// <summary>
    /// Phase 3: Desperation Phase (33-0% HP)
    /// All abilities available, maximum lethality, enrage mechanics.
    /// </summary>
    Phase3 = 3
}
