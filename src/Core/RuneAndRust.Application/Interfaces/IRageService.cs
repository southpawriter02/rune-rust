// ═══════════════════════════════════════════════════════════════════════════════
// IRageService.cs
// Service contract for managing Berserker Rage resource — a volatile combat
// resource powered by violence and pain that grants damage bonuses, damage
// reduction, and special effects at extreme thresholds.
// Version: 0.18.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service contract for managing Rage resource for the Berserker specialization.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// The rage service handles all aspects of the Rage system — a volatile combat
/// resource that builds from violence and pain. As rage accumulates, Berserkers
/// gain escalating damage bonuses and damage reduction, culminating in the
/// FrenzyBeyondReason state where they become fear-immune killing machines.
/// </para>
/// <para>
/// <strong>Responsibilities:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Querying:</strong> Read current rage state and derived bonuses
///       (damage bonus, soak bonus, fear immunity, forced attack targeting)
///       without modifying character state.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Gaining Rage:</strong> Apply rage from various sources
///       (TakingDamage, DealingDamage, AllyDamaged, EnemyKill, RageMaintenance)
///       with threshold transition tracking.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Decay:</strong> Apply non-combat rage decay (10 per turn)
///       when character is not engaged in active combat.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Bonus Calculation:</strong> Provide damage bonus, soak bonus,
///       and special effect status for combat integration.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Rage Thresholds:</strong>
/// Rage is a positive resource in the range 0-100. As rage accumulates,
/// characters gain escalating bonuses across five threshold tiers:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Threshold</term>
///     <description>Range / Effect</description>
///   </listheader>
///   <item>
///     <term>Calm (0-20)</term>
///     <description>No special effects, damage bonus +0 to +2</description>
///   </item>
///   <item>
///     <term>Simmering (21-40)</term>
///     <description>+1 soak, damage bonus +3 to +4</description>
///   </item>
///   <item>
///     <term>Burning (41-60)</term>
///     <description>+2 soak, damage bonus +5 to +6, intimidation bonus</description>
///   </item>
///   <item>
///     <term>BerserkFury (61-80)</term>
///     <description>+3 soak, +7 to +8 damage, must attack nearest, fear resistance</description>
///   </item>
///   <item>
///     <term>FrenzyBeyondReason (81-100)</term>
///     <description>+4 soak, +9 to +10 damage, fear immune, must attack nearest, party stress reduction</description>
///   </item>
/// </list>
/// <para>
/// <strong>Rage Generation:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>TakingDamage: floor(damage / 5) rage</description></item>
///   <item><description>DealingDamage: floor(damage / 10) rage</description></item>
///   <item><description>AllyDamaged: flat 5 rage</description></item>
///   <item><description>EnemyKill: flat 15 rage</description></item>
///   <item><description>RageMaintenance: flat 5 per turn at FrenzyBeyondReason</description></item>
/// </list>
/// <para>
/// <strong>Consumers:</strong> CombatService (damage bonus), DefenseService (soak),
/// FearService (immunity check), AIService (forced targeting), RestService (party stress).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Query rage state
/// var state = rageService.GetRageState(characterId);
/// var damageBonus = rageService.GetDamageBonus(characterId);
///
/// // Apply rage from taking damage
/// var result = rageService.GainRage(
///     characterId,
///     amount: 10,
///     source: RageSource.TakingDamage);
///
/// if (result.ThresholdChanged)
///     ShowThresholdNotification(result.NewThreshold);
///
/// // Check combat behavior restrictions
/// if (rageService.MustAttackNearest(characterId))
///     ForceTargetNearestEnemy(characterId);
///
/// // Apply non-combat decay
/// var decayResult = rageService.ApplyDecay(characterId);
/// </code>
/// </example>
/// <seealso cref="RageState"/>
/// <seealso cref="RageGainResult"/>
/// <seealso cref="RageDecayResult"/>
/// <seealso cref="RageThreshold"/>
/// <seealso cref="RageSource"/>
public interface IRageService
{
    #region Query Methods

    /// <summary>
    /// Gets the current rage state for a character.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to a Berserker
    /// specialization character.
    /// </param>
    /// <returns>
    /// A <see cref="RageState"/> snapshot containing the current rage value,
    /// computed threshold tier, damage bonus, soak bonus, and special effects;
    /// or <c>null</c> if the character has no rage state (non-Berserker).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a read-only query with no side effects. The returned
    /// <see cref="RageState"/> is an immutable value object — modifying it
    /// does not affect the character's persisted rage.
    /// </para>
    /// <para>
    /// Returns <c>null</c> for characters who are not Berserker specialization.
    /// Use the convenience methods <see cref="GetDamageBonus"/>,
    /// <see cref="GetSoakBonus"/>, <see cref="IsFearImmune"/>, and
    /// <see cref="MustAttackNearest"/> when only a single derived value is needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = rageService.GetRageState(characterId);
    /// if (state is not null)
    /// {
    ///     Console.WriteLine($"Rage: {state.CurrentRage}/{RageState.MaxRage}");
    ///     Console.WriteLine($"Threshold: {state.Threshold}");
    ///     Console.WriteLine($"Damage Bonus: +{state.DamageBonus}");
    /// }
    /// </code>
    /// </example>
    RageState? GetRageState(Guid characterId);

    /// <summary>
    /// Gets the current damage bonus from the character's rage level.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// The damage bonus as an integer in the range 0-10, calculated as
    /// <c>floor(CurrentRage / 10)</c>. Returns 0 if character has no rage state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetRageState(characterId)?.DamageBonus ?? 0</c>
    /// </para>
    /// <para>
    /// Primarily consumed by the combat system when calculating total damage
    /// output for Berserker attacks.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During combat damage calculation
    /// int rageBonus = rageService.GetDamageBonus(characterId);
    /// int totalDamage = baseDamage + rageBonus + otherModifiers;
    /// </code>
    /// </example>
    int GetDamageBonus(Guid characterId);

    /// <summary>
    /// Gets the current soak (damage reduction) bonus from the character's rage threshold.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// The soak bonus as an integer in the range 0-4, scaled by threshold:
    /// Calm (0), Simmering (1), Burning (2), BerserkFury (3), FrenzyBeyondReason (4).
    /// Returns 0 if character has no rage state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetRageState(characterId)?.SoakBonus ?? 0</c>
    /// </para>
    /// <para>
    /// Primarily consumed by the defense system when calculating damage
    /// absorption during combat.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During damage resolution
    /// int rageSoak = rageService.GetSoakBonus(characterId);
    /// int effectiveDamage = Math.Max(0, incomingDamage - rageSoak);
    /// </code>
    /// </example>
    int GetSoakBonus(Guid characterId);

    /// <summary>
    /// Determines if the character is immune to fear effects.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// <c>true</c> if the character is at FrenzyBeyondReason threshold (81+ rage);
    /// <c>false</c> otherwise or if character has no rage state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Fear immunity is only active at the FrenzyBeyondReason threshold.
    /// At this extreme rage level, the Berserker is so consumed by battle fury
    /// that fear has no hold on them.
    /// </para>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetRageState(characterId)?.FearImmune ?? false</c>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // When applying fear effect
    /// if (!rageService.IsFearImmune(characterId))
    /// {
    ///     ApplyFearEffect(characterId, fearSource);
    /// }
    /// </code>
    /// </example>
    bool IsFearImmune(Guid characterId);

    /// <summary>
    /// Determines if the character must attack the nearest target.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// <c>true</c> if the character is at BerserkFury or FrenzyBeyondReason threshold
    /// (61+ rage); <c>false</c> otherwise or if character has no rage state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// At BerserkFury and FrenzyBeyondReason thresholds, the Berserker loses
    /// tactical control and must attack the closest enemy target. This represents
    /// the trade-off for the extreme power bonuses at high rage.
    /// </para>
    /// <para>
    /// Should be checked during turn resolution to enforce targeting behavior.
    /// The AI/combat system should override player targeting when this returns true.
    /// </para>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetRageState(characterId)?.MustAttackNearest ?? false</c>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During combat turn resolution
    /// if (rageService.MustAttackNearest(characterId))
    /// {
    ///     var nearestEnemy = FindNearestEnemy(characterId);
    ///     ForceTarget(characterId, nearestEnemy);
    /// }
    /// </code>
    /// </example>
    bool MustAttackNearest(Guid characterId);

    /// <summary>
    /// Gets the party stress reduction bonus when at FrenzyBeyondReason threshold.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// 10 if character is at FrenzyBeyondReason threshold; <c>null</c> otherwise
    /// or if character has no rage state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// When a Berserker enters FrenzyBeyondReason, the terrifying display of
    /// primal fury actually calms their allies — the party gains 10 stress
    /// reduction at the next rest/recovery event.
    /// </para>
    /// <para>
    /// This bonus is checked by the rest service when calculating party-wide
    /// stress recovery. It does not stack with multiple FrenzyBeyondReason entries.
    /// </para>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetRageState(characterId)?.PartyStressReduction</c>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During party rest calculation
    /// var stressReduction = rageService.GetPartyStressReduction(berserkerId);
    /// if (stressReduction.HasValue)
    /// {
    ///     ApplyPartyStressReduction(party, stressReduction.Value);
    /// }
    /// </code>
    /// </example>
    int? GetPartyStressReduction(Guid characterId);

    #endregion

    #region Command Methods

    /// <summary>
    /// Applies rage gain from a specified source.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to a Berserker
    /// specialization character with an active rage state.
    /// </param>
    /// <param name="amount">
    /// Amount of rage to gain (before capping). Must be a positive integer.
    /// Typical values:
    /// <list type="bullet">
    ///   <item><description>TakingDamage: floor(damage / 5)</description></item>
    ///   <item><description>DealingDamage: floor(damage / 10)</description></item>
    ///   <item><description>AllyDamaged: 5</description></item>
    ///   <item><description>EnemyKill: 15</description></item>
    ///   <item><description>RageMaintenance: 5</description></item>
    /// </list>
    /// </param>
    /// <param name="source">
    /// The source of rage generation. Used for logging and analysis.
    /// </param>
    /// <returns>
    /// A <see cref="RageGainResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Previous and new rage values</description></item>
    ///   <item><description>The actual rage gained (after capping at 100)</description></item>
    ///   <item><description>Whether a threshold boundary was crossed</description></item>
    ///   <item><description>The new threshold if changed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Rage is capped at 100 (<see cref="RageState.MaxRage"/>). If the addition
    /// would exceed the cap, the result's NewRage will be 100 and AmountGained
    /// will reflect only the rage that could be added.
    /// </para>
    /// <para>
    /// <strong>Threshold Transitions:</strong>
    /// When rage crosses a threshold boundary (e.g., 60→61 entering BerserkFury),
    /// the result's ThresholdChanged flag is set to true and NewThreshold contains
    /// the new tier. UI systems should display threshold transition notifications.
    /// </para>
    /// <para>
    /// This method should be called by combat services when:
    /// <list type="bullet">
    ///   <item><description>Berserker takes damage</description></item>
    ///   <item><description>Berserker deals damage</description></item>
    ///   <item><description>An ally takes damage within perception range</description></item>
    ///   <item><description>Berserker kills an enemy</description></item>
    ///   <item><description>Turn ends while at FrenzyBeyondReason (maintenance)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Berserker takes 50 damage → gain floor(50/5) = 10 rage
    /// var result = rageService.GainRage(
    ///     characterId,
    ///     amount: 10,
    ///     source: RageSource.TakingDamage);
    ///
    /// if (result.ThresholdChanged &amp;&amp; result.NewThreshold == RageThreshold.FrenzyBeyondReason)
    ///     PlayFrenzyAnimation(characterId);
    /// </code>
    /// </example>
    RageGainResult GainRage(Guid characterId, int amount, RageSource source);

    /// <summary>
    /// Applies non-combat rage decay.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to a Berserker
    /// specialization character with an active rage state.
    /// </param>
    /// <returns>
    /// A <see cref="RageDecayResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Previous and new rage values</description></item>
    ///   <item><description>The amount of rage decayed (10 per call)</description></item>
    ///   <item><description>Whether a threshold boundary was crossed downward</description></item>
    ///   <item><description>The new threshold if changed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Decay Rate:</strong>
    /// Rage decays at 10 points per non-combat turn. This method applies one
    /// turn's worth of decay.
    /// </para>
    /// <para>
    /// <strong>Combat Eligibility:</strong>
    /// Decay should only be applied if the character has not engaged in combat
    /// recently (typically 1+ minutes since LastCombatTime). The caller is
    /// responsible for checking combat eligibility before calling this method.
    /// </para>
    /// <para>
    /// <strong>Minimum Rage:</strong>
    /// Rage is clamped at 0 (<see cref="RageState.MinRage"/>). If decay would
    /// reduce rage below 0, the result's NewRage will be 0.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Apply decay during exploration (non-combat)
    /// if (IsOutOfCombat(characterId))
    /// {
    ///     var decayResult = rageService.ApplyDecay(characterId);
    ///     
    ///     if (decayResult.ThresholdChanged)
    ///         ShowRageDecayNotification(decayResult);
    ///     
    ///     if (decayResult.ZeroedOut)
    ///         ShowCalmRestoredMessage();
    /// }
    /// </code>
    /// </example>
    RageDecayResult ApplyDecay(Guid characterId);

    #endregion
}
