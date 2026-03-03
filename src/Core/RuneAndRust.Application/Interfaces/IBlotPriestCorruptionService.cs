using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for evaluating and applying Blót-Priest deterministic self-Corruption.
/// </summary>
/// <remarks>
/// <para>The Blót-Priest is the most Corruption-intensive specialization in the system.
/// Unlike the Rust-Witch (which has 5 triggers), the Blót-Priest has 8 distinct triggers
/// and generates Corruption from almost every action including healing.</para>
///
/// <para>Corruption is deterministic — no dice roll. Fixed amounts per ability/trigger.
/// Evaluation happens BEFORE resource spending, consistent with the system-wide pattern.</para>
///
/// <para>Unique mechanic: Blight Transference. Gift of Vitae and Crimson Deluge transfer
/// Corruption FROM the Blót-Priest TO allies. The <see cref="BlotPriestCorruptionRiskResult"/>
/// includes both <c>CorruptionAmount</c> (self) and <c>CorruptionTransferred</c> (to ally).</para>
/// </remarks>
public interface IBlotPriestCorruptionService
{
    /// <summary>
    /// Evaluates the deterministic Corruption risk for a Blót-Priest ability.
    /// </summary>
    /// <param name="abilityId">The ability being cast.</param>
    /// <param name="rank">The ability rank (1-3).</param>
    /// <returns>
    /// A <see cref="BlotPriestCorruptionRiskResult"/> containing both self-Corruption
    /// and ally-transferred Corruption amounts.
    /// </returns>
    BlotPriestCorruptionRiskResult EvaluateRisk(BlotPriestAbilityId abilityId, int rank);

    /// <summary>
    /// Applies the evaluated Corruption to the character (delegates to game state).
    /// </summary>
    /// <param name="characterId">The character (Blót-Priest) ID.</param>
    /// <param name="result">The evaluated Corruption result to apply.</param>
    void ApplyCorruption(Guid characterId, BlotPriestCorruptionRiskResult result);

    /// <summary>
    /// Gets a human-readable description of a Corruption trigger.
    /// </summary>
    /// <param name="trigger">The Corruption trigger.</param>
    /// <returns>A descriptive string for UI or logging.</returns>
    string GetTriggerDescription(BlotPriestCorruptionTrigger trigger);

    /// <summary>
    /// Gets the deterministic self-Corruption cost for a given ability at a given rank.
    /// </summary>
    /// <param name="abilityId">The ability to query.</param>
    /// <param name="rank">The ability rank (1-3).</param>
    /// <returns>The fixed self-Corruption amount (0 for passive abilities).</returns>
    int GetCorruptionCost(BlotPriestAbilityId abilityId, int rank);

    /// <summary>
    /// Gets the Corruption amount transferred to allies for abilities with Blight Transference.
    /// </summary>
    /// <param name="abilityId">The ability to query.</param>
    /// <param name="rank">The ability rank (1-3).</param>
    /// <returns>The Corruption amount transferred to allies (0 for non-transfer abilities).</returns>
    int GetTransferAmount(BlotPriestAbilityId abilityId, int rank);
}
