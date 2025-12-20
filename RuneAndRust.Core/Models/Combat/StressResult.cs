using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Represents the result of inflicting or recovering psychic stress on a combatant.
/// Tracks raw stress, WILL-based mitigation, and the resulting stress total.
/// </summary>
/// <param name="RawStress">The initial stress amount before mitigation (positive for inflict, negative for recover).</param>
/// <param name="MitigatedAmount">Stress reduced by WILL resolve check successes (only for infliction).</param>
/// <param name="NetStressApplied">Actual stress change after mitigation (clamped to valid range).</param>
/// <param name="CurrentTotal">Combatant's new stress total after application.</param>
/// <param name="PreviousStatus">Stress status tier before this change.</param>
/// <param name="NewStatus">Stress status tier after this change.</param>
/// <param name="IsBreakingPoint">True if stress reached 100, triggering trauma event.</param>
/// <param name="ResolveSuccesses">Number of successes rolled on WILL resolve check.</param>
/// <param name="Source">Description of what caused the stress (for logging and UI).</param>
public record StressResult(
    int RawStress,
    int MitigatedAmount,
    int NetStressApplied,
    int CurrentTotal,
    StressStatus PreviousStatus,
    StressStatus NewStatus,
    bool IsBreakingPoint,
    int ResolveSuccesses,
    string Source
);
