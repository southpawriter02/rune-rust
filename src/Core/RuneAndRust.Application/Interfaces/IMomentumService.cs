// ═══════════════════════════════════════════════════════════════════════════════
// IMomentumService.cs
// Service contract for managing Storm Blade Momentum resource — a flow-state
// combat resource that rewards sustained aggression and punishes interruption.
// Version: 0.18.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service contract for managing Momentum resource for the Storm Blade specialization.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// The momentum service handles all aspects of the Momentum system — a flow-state
/// combat resource that builds from successful attacks and movement. Momentum
/// rewards sustained aggression with escalating attack/defense bonuses and bonus
/// attacks, but punishes interruption with severe decay penalties.
/// </para>
/// <para>
/// <strong>Responsibilities:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Querying:</strong> Read current momentum state and derived bonuses
///       (attack bonus, defense bonus, movement bonus, bonus attacks) without
///       modifying character state.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Gaining Momentum:</strong> Apply momentum from various sources
///       (SuccessfulAttack, KillingBlow, ChainAttack, MovementAction) with
///       chain bonus tracking and threshold transition detection.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Decay:</strong> Apply momentum decay from missed attacks (25 flat),
///       stun/freeze effects (full reset), or idle turns (15 per turn).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Chain Tracking:</strong> Record hits and misses to track
///       consecutive hit chains for bonus momentum generation.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Momentum Thresholds:</strong>
/// Momentum is a positive resource in the range 0-100. As momentum accumulates,
/// characters gain escalating bonuses across five threshold tiers:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Threshold</term>
///     <description>Range / Effect</description>
///   </listheader>
///   <item>
///     <term>Stationary (0-20)</term>
///     <description>No bonuses, 0 bonus attacks</description>
///   </item>
///   <item>
///     <term>Moving (21-40)</term>
///     <description>+1 attack/defense, +1 movement, 0 bonus attacks</description>
///   </item>
///   <item>
///     <term>Flowing (41-60)</term>
///     <description>+2 attack/defense, +2 movement, 1 bonus attack</description>
///   </item>
///   <item>
///     <term>Surging (61-80)</term>
///     <description>+3 attack/defense, +3 movement, 1 bonus attack</description>
///   </item>
///   <item>
///     <term>Unstoppable (81-100)</term>
///     <description>+4 attack/defense, +4-5 movement, 2 bonus attacks, +10% crit</description>
///   </item>
/// </list>
/// <para>
/// <strong>Momentum Generation:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>SuccessfulAttack: flat 10 momentum</description></item>
///   <item><description>KillingBlow: flat 20 momentum</description></item>
///   <item><description>ChainAttack: 5 × ConsecutiveHits bonus</description></item>
///   <item><description>MovementAction: floor(distance / 2) momentum</description></item>
/// </list>
/// <para>
/// <strong>Decay Events:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Missed Attack: 25 flat, breaks chain</description></item>
///   <item><description>Stunned/Frozen: 100 (full reset)</description></item>
///   <item><description>No Attack Action: 15 per idle turn</description></item>
/// </list>
/// <para>
/// <strong>Consumers:</strong> CombatService (bonus attacks, attack/defense bonuses),
/// MovementService (movement bonus), ChainService (hit tracking), AIService (targeting).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Query momentum state
/// var state = momentumService.GetMomentumState(characterId);
/// var bonusAttacks = momentumService.GetBonusAttacks(characterId);
///
/// // Record successful hit
/// momentumService.RecordHit(characterId);
/// var result = momentumService.GainMomentum(
///     characterId,
///     amount: 10,
///     source: MomentumSource.SuccessfulAttack);
///
/// if (result.ThresholdChanged)
///     ShowThresholdNotification(result.NewThreshold);
///
/// // Handle missed attack
/// momentumService.RecordMiss(characterId);
///
/// // Check for stun reset
/// var stunResult = momentumService.ResetMomentum(characterId, "Stunned by spell");
/// </code>
/// </example>
/// <seealso cref="MomentumState"/>
/// <seealso cref="MomentumGainResult"/>
/// <seealso cref="MomentumDecayResult"/>
/// <seealso cref="MomentumThreshold"/>
/// <seealso cref="MomentumSource"/>
public interface IMomentumService
{
    #region Query Methods

    /// <summary>
    /// Gets the current momentum state for a character.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to a Storm Blade
    /// specialization character.
    /// </param>
    /// <returns>
    /// A <see cref="MomentumState"/> snapshot containing the current momentum value,
    /// computed threshold tier, attack/defense bonuses, movement bonus, bonus attacks,
    /// and chain tracking information; or <c>null</c> if the character has no momentum
    /// state (non-Storm Blade).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a read-only query with no side effects. The returned
    /// <see cref="MomentumState"/> is an immutable value object — modifying it
    /// does not affect the character's persisted momentum.
    /// </para>
    /// <para>
    /// Returns <c>null</c> for characters who are not Storm Blade specialization.
    /// Use the convenience methods <see cref="GetBonusAttacks"/>,
    /// <see cref="GetMovementBonus"/>, <see cref="GetAttackBonus"/>, and
    /// <see cref="GetDefenseBonus"/> when only a single derived value is needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = momentumService.GetMomentumState(characterId);
    /// if (state is not null)
    /// {
    ///     Console.WriteLine($"Momentum: {state.CurrentMomentum}/{MomentumState.MaxMomentum}");
    ///     Console.WriteLine($"Threshold: {state.Threshold}");
    ///     Console.WriteLine($"Bonus Attacks: {state.BonusAttacks}");
    ///     Console.WriteLine($"Consecutive Hits: {state.ConsecutiveHits}");
    /// }
    /// </code>
    /// </example>
    MomentumState? GetMomentumState(Guid characterId);

    /// <summary>
    /// Gets the number of bonus attacks from the character's momentum.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// Bonus attack count (0-2) scaled by threshold:
    /// Stationary/Moving (0), Flowing/Surging (1), Unstoppable (2).
    /// Returns 0 if character has no momentum state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetMomentumState(characterId)?.BonusAttacks ?? 0</c>
    /// </para>
    /// <para>
    /// Bonus attacks are additional attack actions the Storm Blade can take
    /// during their combat turn, representing their fluid fighting style.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During combat turn
    /// int bonusAttacks = momentumService.GetBonusAttacks(characterId);
    /// int totalAttacks = baseAttacks + bonusAttacks;
    /// </code>
    /// </example>
    int GetBonusAttacks(Guid characterId);

    /// <summary>
    /// Gets the movement speed bonus from the character's momentum.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// Movement bonus in squares (0-5), calculated as <c>floor(CurrentMomentum / 20)</c>.
    /// Returns 0 if character has no momentum state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetMomentumState(characterId)?.MovementBonus ?? 0</c>
    /// </para>
    /// <para>
    /// Movement bonus increases the Storm Blade's tactical mobility,
    /// allowing them to reposition more effectively during combat.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During movement calculation
    /// int momentumBonus = momentumService.GetMovementBonus(characterId);
    /// int totalMovement = baseMovement + momentumBonus;
    /// </code>
    /// </example>
    int GetMovementBonus(Guid characterId);

    /// <summary>
    /// Gets the attack bonus from the character's momentum threshold.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// Attack bonus (0-4) scaled by threshold:
    /// Stationary (0), Moving (1), Flowing (2), Surging (3), Unstoppable (4).
    /// Returns 0 if character has no momentum state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetMomentumState(characterId)?.AttackBonus ?? 0</c>
    /// </para>
    /// <para>
    /// Attack bonus is added to attack rolls, representing the Storm Blade's
    /// increased precision and power from sustained combat flow.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During attack resolution
    /// int momentumAttackBonus = momentumService.GetAttackBonus(characterId);
    /// int totalAttackBonus = baseAttack + momentumAttackBonus + otherModifiers;
    /// </code>
    /// </example>
    int GetAttackBonus(Guid characterId);

    /// <summary>
    /// Gets the defense bonus from the character's momentum threshold.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// Defense bonus (0-4) scaled by threshold:
    /// Stationary (0), Moving (1), Flowing (2), Surging (3), Unstoppable (4).
    /// Returns 0 if character has no momentum state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetMomentumState(characterId)?.DefenseBonus ?? 0</c>
    /// </para>
    /// <para>
    /// Defense bonus makes the Storm Blade harder to hit while in motion,
    /// representing their evasive combat style.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During defense calculation
    /// int momentumDefenseBonus = momentumService.GetDefenseBonus(characterId);
    /// int effectiveDefense = baseDefense + momentumDefenseBonus;
    /// </code>
    /// </example>
    int GetDefenseBonus(Guid characterId);

    #endregion

    #region Command Methods

    /// <summary>
    /// Applies momentum gain from a specified source.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to a Storm Blade
    /// specialization character with an active momentum state.
    /// </param>
    /// <param name="amount">
    /// Amount of momentum to gain (before chain bonuses and capping). Must be a
    /// positive integer. Typical values:
    /// <list type="bullet">
    ///   <item><description>SuccessfulAttack: 10</description></item>
    ///   <item><description>KillingBlow: 20</description></item>
    ///   <item><description>ChainAttack: 5 × ConsecutiveHits</description></item>
    ///   <item><description>MovementAction: floor(distance / 2)</description></item>
    /// </list>
    /// </param>
    /// <param name="source">
    /// The source of momentum generation. Affects logging and may influence
    /// chain bonus calculations.
    /// </param>
    /// <returns>
    /// A <see cref="MomentumGainResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Previous and new momentum values</description></item>
    ///   <item><description>The actual momentum gained (after capping at 100)</description></item>
    ///   <item><description>Chain bonus applied (if any)</description></item>
    ///   <item><description>Whether a threshold boundary was crossed</description></item>
    ///   <item><description>The new threshold if changed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Momentum is capped at 100 (<see cref="MomentumState.MaxMomentum"/>). If the
    /// addition would exceed the cap, the result's NewMomentum will be 100 and
    /// AmountGained will reflect only the momentum that could be added.
    /// </para>
    /// <para>
    /// <strong>Chain Bonus:</strong>
    /// When the source is <see cref="MomentumSource.ChainAttack"/>, the chain bonus
    /// is calculated as 5 × ConsecutiveHits. Call <see cref="RecordHit"/> before
    /// this method to update the consecutive hit counter.
    /// </para>
    /// <para>
    /// <strong>Threshold Transitions:</strong>
    /// When momentum crosses a threshold boundary (e.g., 60→61 entering Surging),
    /// the result's ThresholdChanged flag is set. UI systems should display
    /// threshold transition notifications and update bonus displays.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Storm Blade lands a hit
    /// momentumService.RecordHit(characterId);
    /// var result = momentumService.GainMomentum(
    ///     characterId,
    ///     amount: 10,
    ///     source: MomentumSource.SuccessfulAttack);
    ///
    /// if (result.ThresholdChanged &amp;&amp; result.NewThreshold == MomentumThreshold.Unstoppable)
    ///     PlayUnstoppableAnimation(characterId);
    /// </code>
    /// </example>
    MomentumGainResult GainMomentum(Guid characterId, int amount, MomentumSource source);

    /// <summary>
    /// Applies momentum decay from a specified reason.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to a Storm Blade
    /// specialization character with an active momentum state.
    /// </param>
    /// <param name="reason">
    /// Description of why momentum is decaying. Used for logging and UI feedback.
    /// Typical reasons: "Missed Attack", "No Action", "Critical Damage Taken".
    /// </param>
    /// <returns>
    /// A <see cref="MomentumDecayResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Previous and new momentum values</description></item>
    ///   <item><description>The amount of momentum decayed</description></item>
    ///   <item><description>The decay reason</description></item>
    ///   <item><description>Whether the hit chain was broken</description></item>
    ///   <item><description>Whether a threshold boundary was crossed downward</description></item>
    ///   <item><description>The new threshold if changed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Decay Amounts:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Missed Attack: 25 flat, breaks chain</description></item>
    ///   <item><description>No Action (idle turn): 15 flat</description></item>
    ///   <item><description>Critical Damage Taken: 20 flat</description></item>
    /// </list>
    /// <para>
    /// <strong>Chain Break:</strong>
    /// Missed attacks always reset the ConsecutiveHits counter to 0. The result's
    /// ChainBroken flag indicates whether a chain was active and got broken.
    /// </para>
    /// <para>
    /// For full momentum reset (stun/freeze), use <see cref="ResetMomentum"/> instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Storm Blade missed an attack
    /// var decayResult = momentumService.ApplyDecay(characterId, "Missed Attack");
    ///
    /// if (decayResult.ChainBroken)
    ///     ShowChainBrokenNotification();
    ///
    /// if (decayResult.ThresholdChanged)
    ///     UpdateMomentumDisplay(decayResult.NewThreshold);
    /// </code>
    /// </example>
    MomentumDecayResult ApplyDecay(Guid characterId, string reason);

    /// <summary>
    /// Resets momentum to zero with a specified reason.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to a Storm Blade
    /// specialization character with an active momentum state.
    /// </param>
    /// <param name="reason">
    /// Reason for the full reset. Typical reasons: "Stunned by spell",
    /// "Frozen by cold", "Knocked unconscious".
    /// </param>
    /// <returns>
    /// A <see cref="MomentumDecayResult"/> with AmountDecayed equal to the
    /// previous momentum (full loss). ChainBroken is always true for a reset.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Full Reset:</strong>
    /// This method sets momentum to 0 regardless of current value. It is used
    /// for hard interrupts like stun, freeze, or unconsciousness that completely
    /// break the Storm Blade's combat flow.
    /// </para>
    /// <para>
    /// <strong>Chain Reset:</strong>
    /// The ConsecutiveHits counter is always reset to 0. The result's ChainBroken
    /// flag will be true if there was any active chain.
    /// </para>
    /// <para>
    /// For gradual decay (missed attacks, idle turns), use <see cref="ApplyDecay"/>
    /// instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Storm Blade gets stunned
    /// var resetResult = momentumService.ResetMomentum(characterId, "Stunned by spell");
    ///
    /// // resetResult.IsFullReset == true
    /// // resetResult.NewMomentum == 0
    /// ShowMomentumLostNotification(resetResult);
    /// </code>
    /// </example>
    MomentumDecayResult ResetMomentum(Guid characterId, string reason);

    /// <summary>
    /// Records a successful attack hit to build chain momentum.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <remarks>
    /// <para>
    /// Increments the ConsecutiveHits counter by 1. Should be called immediately
    /// after a successful attack hit, before calling <see cref="GainMomentum"/>.
    /// </para>
    /// <para>
    /// The ConsecutiveHits value is used to calculate chain bonuses when gaining
    /// momentum with <see cref="MomentumSource.ChainAttack"/>.
    /// </para>
    /// <para>
    /// Also updates LastActionTime for idle tracking.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Attack lands successfully
    /// momentumService.RecordHit(characterId);
    ///
    /// // Award base momentum + chain bonus
    /// var state = momentumService.GetMomentumState(characterId);
    /// int chainBonus = state?.ConsecutiveHits * 5 ?? 0;
    /// momentumService.GainMomentum(characterId, 10 + chainBonus, MomentumSource.SuccessfulAttack);
    /// </code>
    /// </example>
    void RecordHit(Guid characterId);

    /// <summary>
    /// Records a failed attack to break momentum chain.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <remarks>
    /// <para>
    /// Resets ConsecutiveHits to 0 and applies the miss decay penalty (25 momentum).
    /// Should be called immediately after a failed attack roll.
    /// </para>
    /// <para>
    /// This is a convenience method that combines chain break and decay application.
    /// It is equivalent to resetting the hit counter and calling
    /// <see cref="ApplyDecay"/> with reason "Missed Attack".
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Attack misses
    /// momentumService.RecordMiss(characterId);
    /// // ConsecutiveHits is now 0
    /// // Momentum reduced by 25
    /// </code>
    /// </example>
    void RecordMiss(Guid characterId);

    #endregion
}
