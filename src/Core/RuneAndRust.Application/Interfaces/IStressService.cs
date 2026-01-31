// ═══════════════════════════════════════════════════════════════════════════════
// IStressService.cs
// Interface defining the contract for managing character Psychic Stress,
// including stress queries, stress application with resistance, rest-based
// recovery, and post-Trauma Check reset operations.
// Version: 0.18.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Exceptions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for managing character Psychic Stress.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// The stress service handles all aspects of the Psychic Stress system —
/// the primary psychological damage resource that creates tactical pressure
/// during combat and exploration. Stress accumulates from horror encounters
/// and penalizes Defense, creating a death spiral that rewards proactive
/// stress management.
/// </para>
/// <para>
/// <strong>Responsibilities:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Querying:</strong> Read current stress state and derived penalties
///       (defense penalty, skill disadvantage, trauma check requirement) without
///       modifying character state.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Applying Stress:</strong> Apply stress from various sources
///       (Combat, Exploration, Narrative, Heretical, Environmental, Corruption)
///       with optional WILL-based resistance checks that can reduce the amount.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Recovery:</strong> Recover stress through rest types (Short, Long,
///       Sanctuary, Milestone) or arbitrary named sources such as abilities or
///       consumables.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Trauma Reset:</strong> Reset stress to specific values after a
///       Trauma Check is resolved (75 for passed, 50 for failed).
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Stress Mechanics:</strong>
/// Stress is a negative resource in the range 0-100. As stress accumulates,
/// characters suffer escalating penalties across six threshold tiers:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Threshold</term>
///     <description>Effect</description>
///   </listheader>
///   <item>
///     <term>Calm (0-19)</term>
///     <description>No penalty</description>
///   </item>
///   <item>
///     <term>Uneasy (20-39)</term>
///     <description>Defense -1</description>
///   </item>
///   <item>
///     <term>Anxious (40-59)</term>
///     <description>Defense -2</description>
///   </item>
///   <item>
///     <term>Panicked (60-79)</term>
///     <description>Defense -3</description>
///   </item>
///   <item>
///     <term>Breaking (80-99)</term>
///     <description>Defense -4, skill disadvantage</description>
///   </item>
///   <item>
///     <term>Trauma (100)</term>
///     <description>Defense -5, Trauma Check triggered</description>
///   </item>
/// </list>
/// <para>
/// <strong>Death Spiral:</strong>
/// High Stress → Defense Penalty → More Hits → More Damage → More Stress → Breaking Point.
/// This feedback loop is intentional — it creates genuine tension and rewards proactive
/// stress management.
/// </para>
/// <para>
/// <strong>Consumers:</strong> CombatService (defense penalty, stress application),
/// SkillCheckService (skill disadvantage), RestService (recovery),
/// TraumaService (trauma check, reset), UI Layer (stress bar display).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Query stress state
/// var state = stressService.GetStressState(characterId);
/// var penalty = stressService.GetDefensePenalty(characterId);
///
/// // Apply combat stress with resistance check
/// var result = stressService.ApplyStress(
///     characterId,
///     amount: 20,
///     source: StressSource.Combat,
///     resistDc: 2);
///
/// if (result.ThresholdCrossed)
///     ShowThresholdNotification(result.NewThreshold);
///
/// if (result.TraumaCheckTriggered)
///     InitiateTraumaCheck(characterId);
///
/// // Recover stress from Long Rest
/// var recovery = stressService.RecoverStress(characterId, RestType.Long);
///
/// if (recovery.ThresholdDropped)
///     ShowRecoveryFeedback(recovery.PreviousThreshold, recovery.NewThreshold);
/// </code>
/// </example>
/// <seealso cref="StressState"/>
/// <seealso cref="StressApplicationResult"/>
/// <seealso cref="StressRecoveryResult"/>
/// <seealso cref="StressCheckResult"/>
/// <seealso cref="StressThreshold"/>
/// <seealso cref="StressSource"/>
/// <seealso cref="RestType"/>
public interface IStressService
{
    #region Query Methods

    /// <summary>
    /// Gets the current stress state for a character.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <returns>
    /// A <see cref="StressState"/> snapshot containing the current stress value,
    /// computed threshold tier, defense penalty, skill disadvantage flag, and
    /// trauma check requirement flag.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a read-only query with no side effects. The returned
    /// <see cref="StressState"/> is an immutable value object — modifying it
    /// does not affect the character's persisted stress.
    /// </para>
    /// <para>
    /// Use the convenience methods <see cref="GetDefensePenalty"/>,
    /// <see cref="HasSkillDisadvantage"/>, and <see cref="RequiresTraumaCheck"/>
    /// when only a single derived value is needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = stressService.GetStressState(characterId);
    /// Console.WriteLine($"Stress: {state.CurrentStress}/{StressState.MaxStress}");
    /// Console.WriteLine($"Threshold: {state.Threshold}");
    /// Console.WriteLine($"Defense Penalty: -{state.DefensePenalty}");
    /// </code>
    /// </example>
    StressState GetStressState(Guid characterId);

    /// <summary>
    /// Gets the defense penalty from the character's current stress level.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <returns>
    /// The defense penalty as an integer in the range 0-5, calculated as
    /// <c>floor(stress / 20)</c>. Returns 0 for Calm characters and 5 for
    /// characters at Trauma threshold.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetStressState(characterId).DefensePenalty</c>
    /// </para>
    /// <para>
    /// Primarily consumed by the combat system when calculating effective defense
    /// during hit resolution.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During combat hit resolution
    /// int stressPenalty = stressService.GetDefensePenalty(characterId);
    /// int effectiveDefense = baseDefense - stressPenalty;
    /// </code>
    /// </example>
    int GetDefensePenalty(Guid characterId);

    /// <summary>
    /// Gets whether the character has skill check disadvantage from stress.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <returns>
    /// <c>true</c> if the character's stress is 80 or higher (Breaking or Trauma
    /// threshold); <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetStressState(characterId).HasSkillDisadvantage</c>
    /// </para>
    /// <para>
    /// At Breaking (80-99): disadvantage on non-combat skill checks.
    /// At Trauma (100): disadvantage on ALL checks.
    /// </para>
    /// <para>
    /// Primarily consumed by the skill check system when determining whether
    /// a character rolls with disadvantage.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During skill check resolution
    /// if (stressService.HasSkillDisadvantage(characterId))
    ///     dicePool = ApplyDisadvantage(dicePool);
    /// </code>
    /// </example>
    bool HasSkillDisadvantage(Guid characterId);

    /// <summary>
    /// Gets whether a Trauma Check is required for the character.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <returns>
    /// <c>true</c> if the character's stress is at the maximum value (100);
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetStressState(characterId).RequiresTraumaCheck</c>
    /// </para>
    /// <para>
    /// When this returns <c>true</c>, the Trauma System (v0.18.3) should initiate
    /// a Trauma Check. After the check resolves, call
    /// <see cref="ResetAfterTraumaCheck"/> to set the appropriate stress level.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After applying stress
    /// if (stressService.RequiresTraumaCheck(characterId))
    /// {
    ///     bool passed = traumaService.ExecuteTraumaCheck(characterId);
    ///     stressService.ResetAfterTraumaCheck(characterId, passed);
    /// }
    /// </code>
    /// </example>
    bool RequiresTraumaCheck(Guid characterId);

    #endregion

    #region Command Methods

    /// <summary>
    /// Applies stress to a character from a specified source.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <param name="amount">
    /// The base stress amount before any resistance reduction. Must be a positive
    /// integer. Typical values range from 5 (minor scare) to 30 (major horror).
    /// </param>
    /// <param name="source">
    /// The category of stress source. Determines whether resistance is typically
    /// possible — <see cref="StressSource.Narrative"/> and
    /// <see cref="StressSource.Corruption"/> are generally unavoidable.
    /// </param>
    /// <param name="resistDc">
    /// Optional difficulty class for a WILL-based resistance check. If 0 or omitted,
    /// no resistance check is performed and full stress is applied. Typical DCs
    /// range from 1 (easy) to 4 (very hard).
    /// </param>
    /// <returns>
    /// A <see cref="StressApplicationResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Previous and new stress values</description></item>
    ///   <item><description>The actual stress gained (after any resistance)</description></item>
    ///   <item><description>Previous and new threshold tiers</description></item>
    ///   <item><description>Whether a threshold boundary was crossed</description></item>
    ///   <item><description>Whether a Trauma Check was triggered (stress reached 100)</description></item>
    ///   <item><description>The optional resistance check result</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Resistance Check:</strong>
    /// If <paramref name="resistDc"/> is greater than 0, a WILL-based resistance
    /// check is performed via <see cref="PerformResistanceCheck"/>. The number of
    /// successes determines stress reduction:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>0 successes: 0% reduction (full stress applied)</description></item>
    ///   <item><description>1 success: 50% reduction (half stress applied)</description></item>
    ///   <item><description>2-3 successes: 75% reduction (quarter stress applied)</description></item>
    ///   <item><description>4+ successes: 100% reduction (no stress gained)</description></item>
    /// </list>
    /// <para>
    /// <strong>Clamping:</strong>
    /// The resulting stress is clamped to the range 0-100. Stress cannot exceed
    /// <see cref="StressState.MaxStress"/> (100).
    /// </para>
    /// <para>
    /// <strong>Trauma Check:</strong>
    /// After applying stress, check <see cref="StressApplicationResult.TraumaCheckTriggered"/>
    /// to determine if a Trauma Check should be initiated by the Trauma System (v0.18.3).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Apply combat stress with resistance check (DC 2)
    /// var result = stressService.ApplyStress(
    ///     characterId,
    ///     amount: 20,
    ///     source: StressSource.Combat,
    ///     resistDc: 2);
    ///
    /// // Apply unavoidable narrative stress (no resistance)
    /// var narrativeResult = stressService.ApplyStress(
    ///     characterId,
    ///     amount: 15,
    ///     source: StressSource.Narrative);
    ///
    /// if (result.ThresholdCrossed)
    ///     ShowThresholdNotification(result.NewThreshold);
    ///
    /// if (result.TraumaCheckTriggered)
    ///     InitiateTraumaCheck(characterId);
    /// </code>
    /// </example>
    StressApplicationResult ApplyStress(
        Guid characterId,
        int amount,
        StressSource source,
        int resistDc = 0);

    /// <summary>
    /// Recovers stress for a character based on rest type.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <param name="restType">
    /// The type of rest determining the recovery amount. Recovery formulas are
    /// based on the character's WILL attribute:
    /// <list type="bullet">
    ///   <item><description><see cref="RestType.Short"/>: WILL × 2</description></item>
    ///   <item><description><see cref="RestType.Long"/>: WILL × 5</description></item>
    ///   <item><description><see cref="RestType.Sanctuary"/>: Full reset to 0</description></item>
    ///   <item><description><see cref="RestType.Milestone"/>: Fixed 25 (ignores WILL)</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// A <see cref="StressRecoveryResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Previous and new stress values</description></item>
    ///   <item><description>The amount of stress recovered</description></item>
    ///   <item><description>The recovery source (rest type)</description></item>
    ///   <item><description>Previous and new threshold tiers</description></item>
    ///   <item><description>Whether a threshold boundary was crossed downward</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Recovery is clamped at 0 — stress cannot go below
    /// <see cref="StressState.MinStress"/> (0).
    /// </para>
    /// <para>
    /// Sanctuary rest is the only rest type that guarantees a full reset to 0 stress,
    /// regardless of the character's WILL attribute. This makes Sanctuary locations
    /// strategically important in the game world.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Long Rest recovery (WILL × 5)
    /// var recovery = stressService.RecoverStress(characterId, RestType.Long);
    /// Console.WriteLine($"Recovered {recovery.AmountRecovered} stress");
    ///
    /// if (recovery.ThresholdDropped)
    ///     ShowRecoveryFeedback(recovery.PreviousThreshold, recovery.NewThreshold);
    ///
    /// // Sanctuary full reset
    /// var sanctuary = stressService.RecoverStress(characterId, RestType.Sanctuary);
    /// // sanctuary.IsFullyRecovered == true
    /// </code>
    /// </example>
    StressRecoveryResult RecoverStress(Guid characterId, RestType restType);

    /// <summary>
    /// Recovers a specific amount of stress from a named source.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <param name="amount">
    /// The amount of stress to recover. Must be a positive integer.
    /// </param>
    /// <param name="source">
    /// Descriptive name of the recovery source (e.g., "Bone-Setter Ability",
    /// "Calming Draught", "Sanctuary Ward"). Used for logging and history tracking.
    /// </param>
    /// <returns>
    /// A <see cref="StressRecoveryResult"/> containing recovery details and
    /// threshold changes. The <see cref="StressRecoveryResult.RecoverySource"/>
    /// will reflect <see cref="RestType.Milestone"/> as a generic non-rest
    /// recovery category.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Use this overload for non-rest recovery sources such as abilities,
    /// consumables, or special events. Recovery is clamped at 0 — stress
    /// cannot go below <see cref="StressState.MinStress"/>.
    /// </para>
    /// <para>
    /// The <paramref name="source"/> string is logged for analytics and history
    /// tracking but does not affect the recovery calculation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Ability-based recovery
    /// var recovery = stressService.RecoverStress(
    ///     characterId,
    ///     amount: 10,
    ///     source: "Bone-Setter Ability");
    ///
    /// // Consumable-based recovery
    /// var potionRecovery = stressService.RecoverStress(
    ///     characterId,
    ///     amount: 15,
    ///     source: "Calming Draught");
    /// </code>
    /// </example>
    StressRecoveryResult RecoverStress(Guid characterId, int amount, string source);

    /// <summary>
    /// Resets stress after a Trauma Check is resolved.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <param name="passed">
    /// <c>true</c> if the character passed the Trauma Check (stress resets to 75);
    /// <c>false</c> if the character failed (stress resets to 50, and the character
    /// gains a permanent Trauma via the Trauma System).
    /// </param>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Stress Reset Values:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <strong>Passed (75):</strong> The character barely maintained composure.
    ///       They are still under extreme stress (Breaking threshold) but avoided
    ///       permanent psychological damage.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Failed (50):</strong> The character broke under pressure. Stress
    ///       drops further because the psyche "released" through the trauma — but the
    ///       character now carries a permanent Trauma.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// This method should only be called after a Trauma Check is fully resolved.
    /// The Trauma Check execution itself and permanent Trauma application are
    /// handled by the Trauma System (v0.18.3).
    /// </para>
    /// <para>
    /// This is a simple state mutation with no return value. The stress state
    /// can be queried afterward via <see cref="GetStressState"/> if needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After Trauma Check resolution
    /// bool passed = traumaService.ExecuteTraumaCheck(characterId);
    /// stressService.ResetAfterTraumaCheck(characterId, passed);
    ///
    /// // Verify new state
    /// var state = stressService.GetStressState(characterId);
    /// // passed: state.CurrentStress == 75 (Breaking threshold)
    /// // failed: state.CurrentStress == 50 (Anxious threshold)
    /// </code>
    /// </example>
    void ResetAfterTraumaCheck(Guid characterId, bool passed);

    #endregion

    #region Internal Methods

    /// <summary>
    /// Performs a WILL-based resistance check against incoming stress.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Used to look up the character's WILL
    /// attribute for the dice pool.
    /// </param>
    /// <param name="baseStress">
    /// The original stress amount before any resistance reduction. Must be a
    /// non-negative integer.
    /// </param>
    /// <param name="dc">
    /// The difficulty class for the resistance check. Each die in the WILL pool
    /// that meets or exceeds this DC counts as a success.
    /// </param>
    /// <returns>
    /// A <see cref="StressCheckResult"/> containing the number of successes,
    /// the reduction percentage (0%/50%/75%/100%), and the final stress amount
    /// after reduction.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is typically called internally by <see cref="ApplyStress"/>
    /// when <c>resistDc</c> is greater than 0. It is exposed on the interface
    /// for testing and for specialized use cases where resistance needs to be
    /// calculated independently of stress application.
    /// </para>
    /// <para>
    /// The resistance check uses the character's WILL attribute as the dice pool
    /// size, rolling against the specified DC via the <c>IDiceService</c>.
    /// </para>
    /// <para>
    /// <strong>Reduction Table:</strong>
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Successes</term>
    ///     <description>Reduction</description>
    ///   </listheader>
    ///   <item>
    ///     <term>0</term>
    ///     <description>0% — full stress applied</description>
    ///   </item>
    ///   <item>
    ///     <term>1</term>
    ///     <description>50% — half stress applied</description>
    ///   </item>
    ///   <item>
    ///     <term>2-3</term>
    ///     <description>75% — quarter stress applied</description>
    ///   </item>
    ///   <item>
    ///     <term>4+</term>
    ///     <description>100% — no stress gained (fully resisted)</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Manually perform a resistance check
    /// var checkResult = stressService.PerformResistanceCheck(
    ///     characterId,
    ///     baseStress: 20,
    ///     dc: 2);
    ///
    /// Console.WriteLine($"Successes: {checkResult.Successes}");
    /// Console.WriteLine($"Reduction: {checkResult.ReductionPercent:P0}");
    /// Console.WriteLine($"Final Stress: {checkResult.FinalStress}");
    /// </code>
    /// </example>
    StressCheckResult PerformResistanceCheck(Guid characterId, int baseStress, int dc);

    #endregion
}
