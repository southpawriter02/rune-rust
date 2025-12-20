using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Represents the result of resolving a Breaking Point event.
/// Contains the outcome, any trauma acquired, and the new stress level.
/// </summary>
/// <param name="Outcome">The result of the resolve check (Stabilized, Trauma, or Catastrophe).</param>
/// <param name="AcquiredTrauma">The trauma acquired, if any (null on Stabilized).</param>
/// <param name="NewStressLevel">The stress level after resolution (75 for Stabilized, 50 for Trauma/Catastrophe).</param>
/// <param name="ResolveSuccesses">Number of successes rolled on the WILL check.</param>
/// <param name="ResolveBotches">Number of botches rolled on the WILL check.</param>
/// <param name="WasStunned">Whether the Stunned status was applied (Catastrophe only).</param>
/// <param name="WasDisoriented">Whether the Disoriented status was applied (Stabilized only).</param>
public record BreakingPointResult(
    BreakingPointOutcome Outcome,
    Trauma? AcquiredTrauma,
    int NewStressLevel,
    int ResolveSuccesses,
    int ResolveBotches,
    bool WasStunned,
    bool WasDisoriented
);
