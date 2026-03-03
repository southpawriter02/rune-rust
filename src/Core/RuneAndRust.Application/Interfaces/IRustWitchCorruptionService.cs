using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for evaluating and applying Rust-Witch self-Corruption.
/// </summary>
/// <remarks>
/// <para>Unlike the Seiðkona's probability-based Corruption system (d100 vs percentage threshold),
/// the Rust-Witch uses <strong>deterministic self-Corruption</strong>. Every active ability inflicts
/// a fixed amount of Corruption on the caster. The evaluation is a simple lookup rather than a
/// dice roll.</para>
///
/// <para><b>Self-Corruption Schedule:</b></para>
/// <list type="table">
///   <listheader><term>Ability</term><description>R1/R2 → R3</description></listheader>
///   <item><term>Corrosive Curse (25002)</term><description>+2 → +1</description></item>
///   <item><term>System Shock (25004)</term><description>+3 → +2</description></item>
///   <item><term>Flash Rust (25005)</term><description>+4 → +3</description></item>
///   <item><term>Unmaking Word (25007)</term><description>+4 → +4 (no rank reduction)</description></item>
///   <item><term>Entropic Cascade (25009)</term><description>+6 → +6 (no rank reduction)</description></item>
/// </list>
///
/// <para>Corruption is evaluated BEFORE resource spending, consistent with the system-wide
/// pattern established by the Berserkr (v0.20.5a). However, since the evaluation is deterministic,
/// the result is always the same for a given ability and rank — there is no randomness.</para>
///
/// <para><b>Usage in ability execution:</b></para>
/// <code>
/// // Inside RustWitchAbilityService.ExecuteCorrosiveCurse:
/// var corruptionResult = _corruptionService.EvaluateRisk(
///     RustWitchAbilityId.CorrosiveCurse, rank);
/// // ... execute ability effects ...
/// if (corruptionResult.IsTriggered)
///     _corruptionService.ApplyCorruption(player.Id, corruptionResult);
/// </code>
/// </remarks>
public interface IRustWitchCorruptionService
{
    /// <summary>
    /// Evaluates the deterministic Corruption cost for a Rust-Witch ability at a given rank.
    /// </summary>
    /// <param name="abilityId">The ability being cast.</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Affects Corruption amount for T1-T2 abilities.</param>
    /// <returns>
    /// A <see cref="RustWitchCorruptionRiskResult"/> with the fixed Corruption amount.
    /// Returns a safe result (0 Corruption) for passive abilities (25001, 25003, 25006, 25008).
    /// </returns>
    RustWitchCorruptionRiskResult EvaluateRisk(RustWitchAbilityId abilityId, int rank);

    /// <summary>
    /// Applies Corruption to the caster based on a previous evaluation result.
    /// </summary>
    /// <param name="characterId">The player character's unique identifier.</param>
    /// <param name="result">The evaluated Corruption result from <see cref="EvaluateRisk"/>.</param>
    /// <remarks>
    /// Only applies Corruption if <see cref="RustWitchCorruptionRiskResult.IsTriggered"/> is true.
    /// Logs the Corruption application at INFO level with structured logging.
    /// </remarks>
    void ApplyCorruption(Guid characterId, RustWitchCorruptionRiskResult result);

    /// <summary>
    /// Gets a human-readable description of a Corruption trigger.
    /// </summary>
    /// <param name="trigger">The trigger to describe.</param>
    /// <returns>A display-friendly string describing what caused the Corruption.</returns>
    string GetTriggerDescription(RustWitchCorruptionTrigger trigger);

    /// <summary>
    /// Gets the fixed Corruption cost for an ability at a given rank.
    /// </summary>
    /// <param name="abilityId">The ability to query.</param>
    /// <param name="rank">The ability rank (1, 2, or 3).</param>
    /// <returns>The Corruption amount, or 0 for passive abilities.</returns>
    int GetCorruptionCost(RustWitchAbilityId abilityId, int rank);
}
