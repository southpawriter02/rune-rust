// ═══════════════════════════════════════════════════════════════════════════════
// ITraumaEconomyService.cs
// Interface defining the unified orchestration contract for all trauma economy
// operations. Provides state aggregation, unified processing delegation, and
// cross-system query methods.
// Version: 0.18.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unified orchestration service for all trauma economy operations.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// ITraumaEconomyService provides a single entry point for all trauma economy
/// operations across multiple subsystems: Stress, Corruption, CPS, Trauma, and
/// Specialization Resources (Rage, Momentum, Coherence).
/// </para>
/// <para>
/// <strong>Orchestration Model:</strong>
/// Rather than requiring consumers to coordinate multiple services directly,
/// the trauma economy service aggregates state and delegates processing to
/// specialized handlers:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>State Access:</strong> Composes a unified <see cref="TraumaEconomyState"/>
///       from all subsystem states and creates point-in-time snapshots.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Damage Processing:</strong> Delegates to <c>UnifiedDamageHandler</c>
///       for soak calculation, stress generation, rage gain, and trauma triggers.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Rest Processing:</strong> Delegates to <c>UnifiedRestHandler</c>
///       for stress recovery, resource resets, and party bonuses.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Turn Processing:</strong> Delegates to <c>UnifiedTurnHandler</c>
///       for resource decay, Apotheosis costs, and panic checks.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Query Methods:</strong> Calculates cross-system derived values
///       such as effective HP, total penalties, and warning levels.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Thread Safety:</strong> Implementations should be scoped per-request
/// in DI to ensure thread-safe character state access.
/// </para>
/// <para>
/// <strong>Consumers:</strong> Combat system, rest system, turn manager, UI layer.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get unified state
/// var state = traumaEconomyService.GetState(characterId);
/// Console.WriteLine($"Stress: {state.StressState.CurrentStress}");
/// Console.WriteLine($"Warning Level: {state.GetWarningLevel()}");
/// 
/// // Process damage through unified handler
/// var context = new DamageContext(IsCriticalHit: true);
/// var result = traumaEconomyService.ProcessDamage(characterId, 30, context);
/// if (result.TraumaCheckTriggered)
///     InitiateTraumaCheck(characterId);
/// 
/// // Get cross-system penalties
/// int penalty = traumaEconomyService.GetTotalDefensePenalty(characterId);
/// </code>
/// </example>
/// <seealso cref="TraumaEconomyState"/>
/// <seealso cref="TraumaEconomySnapshot"/>
/// <seealso cref="DamageIntegrationResult"/>
/// <seealso cref="RestIntegrationResult"/>
/// <seealso cref="TurnIntegrationResult"/>
public interface ITraumaEconomyService
{
    #region State Access

    /// <summary>
    /// Gets the unified trauma economy state for a character.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>
    /// A <see cref="TraumaEconomyState"/> aggregating stress, corruption, CPS,
    /// trauma, and specialization resource states.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The returned state is a composed view of all subsystem states at the
    /// current moment. It is not cached — each call queries the underlying
    /// services for fresh data.
    /// </para>
    /// <para>
    /// Use <see cref="CreateSnapshot"/> when you need an immutable record
    /// for comparison or logging.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = service.GetState(characterId);
    /// Console.WriteLine($"Stress: {state.StressState.CurrentStress}/100");
    /// Console.WriteLine($"Corruption: {state.CorruptionState.CurrentCorruption}/100");
    /// Console.WriteLine($"Effective Max HP: {state.EffectiveMaxHp}");
    /// </code>
    /// </example>
    TraumaEconomyState GetState(Guid characterId);

    /// <summary>
    /// Creates an immutable snapshot of the current trauma economy state.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>
    /// A <see cref="TraumaEconomySnapshot"/> capturing all current values
    /// with a timestamp.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// Snapshots are useful for:
    /// <list type="bullet">
    ///   <item><description>Before/after comparisons (e.g., combat deltas)</description></item>
    ///   <item><description>Logging and audit trails</description></item>
    ///   <item><description>Serialization for save games</description></item>
    ///   <item><description>UI delta display ("Stress increased by 15")</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var before = service.CreateSnapshot(characterId);
    /// var result = service.ProcessDamage(characterId, 50, context);
    /// var after = service.CreateSnapshot(characterId);
    /// 
    /// int stressDelta = after.Stress - before.Stress;
    /// Console.WriteLine($"Stress changed by {stressDelta}");
    /// </code>
    /// </example>
    TraumaEconomySnapshot CreateSnapshot(Guid characterId);

    #endregion

    #region Unified Processing

    /// <summary>
    /// Processes damage through all trauma economy systems.
    /// </summary>
    /// <param name="characterId">The character taking damage.</param>
    /// <param name="damage">Raw damage amount before soak.</param>
    /// <param name="context">Damage context with bonus flags and conditions.</param>
    /// <returns>
    /// A <see cref="DamageIntegrationResult"/> containing stress gained,
    /// soak applied, specialization effects, and trauma trigger status.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Processing Order:</strong>
    /// </para>
    /// <list type="number">
    ///   <item><description>Calculate soak (base armor + Berserker rage bonus)</description></item>
    ///   <item><description>Apply stress generation formula (damage/10 + bonuses)</description></item>
    ///   <item><description>Process specialization effects (rage gain, momentum decay, coherence loss)</description></item>
    ///   <item><description>Check for trauma triggers (stress at 100)</description></item>
    /// </list>
    /// <para>
    /// After calling this method, check <see cref="DamageIntegrationResult.TraumaCheckTriggered"/>
    /// to determine if a trauma check should be initiated.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = new DamageContext(
    ///     IsCriticalHit: true,
    ///     IsNearDeath: false,
    ///     AllyDied: false);
    /// var result = service.ProcessDamage(characterId, 30, context);
    /// 
    /// Console.WriteLine($"Soak: {result.TotalSoak}");
    /// Console.WriteLine($"Stress Gained: {result.StressGained}");
    /// if (result.TraumaCheckTriggered)
    ///     InitiateTraumaCheck(characterId);
    /// </code>
    /// </example>
    DamageIntegrationResult ProcessDamage(Guid characterId, int damage, DamageContext context);

    /// <summary>
    /// Processes a rest event through all trauma economy systems.
    /// </summary>
    /// <param name="characterId">The character taking rest.</param>
    /// <param name="restType">The type of rest being taken.</param>
    /// <param name="partyContext">Optional party context for party bonuses.</param>
    /// <returns>
    /// A <see cref="RestIntegrationResult"/> containing stress recovered,
    /// resource resets, CPS changes, and party bonus information.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Processing Order:</strong>
    /// </para>
    /// <list type="number">
    ///   <item><description>Calculate and apply stress recovery based on rest type</description></item>
    ///   <item><description>Reset specialization resources (Rage/Momentum to 0, Coherence to 50 on Long/Sanctuary)</description></item>
    ///   <item><description>Apply party bonuses if partyContext provided</description></item>
    ///   <item><description>Perform trauma checks if applicable (Long/Sanctuary rest)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = service.ProcessRest(characterId, RestType.Long);
    /// Console.WriteLine($"Stress Recovered: {result.StressRecovered}");
    /// if (result.CpsStageChanged)
    ///     ShowCpsRecoveryNotification(result.NewCpsStage);
    /// </code>
    /// </example>
    RestIntegrationResult ProcessRest(Guid characterId, RestType restType, PartyContext? partyContext = null);

    /// <summary>
    /// Processes start-of-turn effects for a character.
    /// </summary>
    /// <param name="characterId">The character to process.</param>
    /// <param name="isInCombat">Whether the character is currently in combat.</param>
    /// <returns>
    /// A <see cref="TurnIntegrationResult"/> containing resource decay,
    /// Apotheosis state changes, and any triggered effects.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Turn Start Processing:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Resource decay (Rage 10/turn, Momentum 15/turn) when out of combat</description></item>
    ///   <item><description>Apotheosis stress cost (10/turn) if coherence is 81+</description></item>
    ///   <item><description>Auto-exit Apotheosis if stress reaches 100</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = service.ProcessTurnStart(characterId, isInCombat: false);
    /// if (result.AutoExitedApotheosis)
    ///     ShowApotheosisExitMessage(result.ApotheosisExitReason);
    /// </code>
    /// </example>
    TurnIntegrationResult ProcessTurnStart(Guid characterId, bool isInCombat);

    /// <summary>
    /// Processes end-of-turn effects for a character.
    /// </summary>
    /// <param name="characterId">The character to process.</param>
    /// <param name="environmentalStress">Environmental stress to apply (0-5).</param>
    /// <returns>
    /// A <see cref="TurnIntegrationResult"/> containing environmental stress,
    /// panic checks, and trauma trigger status.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Turn End Processing:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Apply environmental stress (capped at configuration maximum)</description></item>
    ///   <item><description>Panic check if in RuinMadness CPS stage</description></item>
    ///   <item><description>Check for trauma triggers</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = service.ProcessTurnEnd(characterId, environmentalStress: 3);
    /// if (result.PanicEffectApplied.HasValue)
    ///     ApplyPanicBehavior(result.PanicEffectApplied.Value);
    /// </code>
    /// </example>
    TurnIntegrationResult ProcessTurnEnd(Guid characterId, int environmentalStress = 0);

    #endregion

    #region Query Methods

    /// <summary>
    /// Gets the effective maximum HP after corruption penalty.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>
    /// Effective max HP calculated as <c>BaseMaxHp * (1 - Corruption/100)</c>.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// At 0 corruption: 100% of base max HP.
    /// At 50 corruption: 50% of base max HP.
    /// At 100 corruption: Character is effectively terminal.
    /// </remarks>
    int GetEffectiveMaxHp(Guid characterId);

    /// <summary>
    /// Gets the total defense penalty from all trauma economy sources.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>
    /// Combined defense penalty from stress threshold and corruption.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// Defense penalty sources:
    /// <list type="bullet">
    ///   <item><description>Stress: 0-5 based on threshold tier</description></item>
    ///   <item><description>Corruption: Additional penalty at high levels</description></item>
    /// </list>
    /// </remarks>
    int GetTotalDefensePenalty(Guid characterId);

    /// <summary>
    /// Gets the total skill penalty from all trauma economy sources.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>
    /// Combined skill penalty from stress, corruption, and CPS effects.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// Skill penalty sources:
    /// <list type="bullet">
    ///   <item><description>Stress: Disadvantage at Breaking (80+) threshold</description></item>
    ///   <item><description>CPS: Logic disadvantage from Glimmer Madness onward</description></item>
    /// </list>
    /// </remarks>
    int GetTotalSkillPenalty(Guid characterId);

    /// <summary>
    /// Gets the overall warning level for UI and narrative purposes.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>
    /// The highest <see cref="WarningLevel"/> across all trauma economy systems.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// Warning levels escalate through: None → Low → Moderate → High → Critical → Terminal.
    /// The returned level is the maximum severity across stress, corruption, and CPS.
    /// </remarks>
    WarningLevel GetWarningLevel(Guid characterId);

    /// <summary>
    /// Gets all active warning messages based on current state.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>
    /// A list of player-facing warning messages for systems above their
    /// warning thresholds.
    /// </returns>
    /// <exception cref="Exceptions.CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// Messages are loaded from <see cref="Configuration.ITraumaEconomyConfiguration"/>
    /// and filtered based on current system thresholds.
    /// </remarks>
    /// <example>
    /// <code>
    /// var warnings = service.GetActiveWarnings(characterId);
    /// foreach (var warning in warnings)
    ///     ShowWarningNotification(warning);
    /// </code>
    /// </example>
    IReadOnlyList<string> GetActiveWarnings(Guid characterId);

    #endregion
}
