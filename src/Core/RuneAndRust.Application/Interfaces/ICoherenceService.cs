// ═══════════════════════════════════════════════════════════════════════════════
// ICoherenceService.cs
// Service contract for managing Arcanist Coherence resource — a reality-stability
// meter that provides spell power bonuses, critical cast chance, and
// cascade/apotheosis mechanics at threshold extremes.
// Version: 0.18.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service contract for managing Coherence resource for the Arcanist specialization.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// The coherence service handles all aspects of the Coherence system — a reality-stability
/// meter that measures the Arcanist's control over arcane forces. Unlike Rage and Momentum
/// which reward high values, Coherence has a "sweet spot" in the middle (Balanced threshold)
/// with dangerous effects at both extremes: Cascade at low coherence, Apotheosis at high.
/// </para>
/// <para>
/// <strong>Responsibilities:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Querying:</strong> Read current coherence state and derived values
///       (spell power bonus, critical cast chance, cascade risk, apotheosis status)
///       without modifying character state.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Gaining Coherence:</strong> Apply coherence from various sources
///       (SuccessfulCast, ControlledChannel, MeditationAction, StabilityField).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Losing Coherence:</strong> Apply coherence loss from failed casts,
///       interruptions, or other disruptive events.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Cascade Management:</strong> Check cascade risk at low coherence
///       thresholds and determine cascade effects.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Apotheosis Management:</strong> Manage entry, maintenance, and exit
///       from the powerful but costly Apotheosis state.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Meditation:</strong> Provide meditation interface for non-combat
///       coherence recovery.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Coherence Thresholds:</strong>
/// Coherence is a stability resource in the range 0-100. The thresholds have
/// different characteristics than Rage/Momentum:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Threshold</term>
///     <description>Range / Effect</description>
///   </listheader>
///   <item>
///     <term>Destabilized (0-20)</term>
///     <description>-2 spell power, 0% crit, 25% cascade risk per cast — DANGEROUS</description>
///   </item>
///   <item>
///     <term>Unstable (21-40)</term>
///     <description>-1 spell power, 0% crit, 10% cascade risk per cast</description>
///   </item>
///   <item>
///     <term>Balanced (41-60)</term>
///     <description>+0 spell power, 5% crit, 0% cascade risk — IDEAL</description>
///   </item>
///   <item>
///     <term>Focused (61-80)</term>
///     <description>+2 spell power, 10% crit, reduced mana costs</description>
///   </item>
///   <item>
///     <term>Apotheosis (81-100)</term>
///     <description>+5 spell power, 20% crit, ultimate abilities, 10 stress/turn — POWERFUL BUT COSTLY</description>
///   </item>
/// </list>
/// <para>
/// <strong>Coherence Generation:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>SuccessfulCast: flat 5 coherence</description></item>
///   <item><description>ControlledChannel: 3 per turn maintained</description></item>
///   <item><description>MeditationAction: flat 20 (outside combat only)</description></item>
///   <item><description>StabilityField: 1-10 variable (environment/ally effect)</description></item>
/// </list>
/// <para>
/// <strong>Special Mechanics:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Cascade: Random negative effect at Destabilized/Unstable on failed stability check</description></item>
///   <item><description>Apotheosis: Ultimate power state that costs 10 stress per turn to maintain</description></item>
///   <item><description>Meditation: 20 coherence recovery action, only available outside combat</description></item>
/// </list>
/// <para>
/// <strong>Consumers:</strong> SpellService (power/crit bonuses), CombatService (cascade checks),
/// StressService (apotheosis cost), RestService (meditation), UIService (stability display).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Query coherence state
/// var state = coherenceService.GetCoherenceState(characterId);
/// var spellPower = coherenceService.GetSpellPowerBonus(characterId);
///
/// // Apply coherence from successful cast
/// coherenceService.GainCoherence(characterId, 5, CoherenceSource.SuccessfulCast);
///
/// // Check cascade risk before casting at low coherence
/// var cascadeResult = coherenceService.CheckCascade(characterId);
/// if (cascadeResult.CascadeTriggered)
///     ApplyCascadeEffect(cascadeResult);
///
/// // Check apotheosis state
/// var apotheosis = coherenceService.UpdateApotheosis(characterId);
/// if (apotheosis.EnteredApotheosis)
///     ShowApotheosisAnimation();
///
/// // Meditation during exploration
/// if (coherenceService.CanMeditate(characterId))
///     coherenceService.Meditate(characterId);
/// </code>
/// </example>
/// <seealso cref="CoherenceState"/>
/// <seealso cref="CascadeResult"/>
/// <seealso cref="ApotheosisResult"/>
/// <seealso cref="CoherenceThreshold"/>
/// <seealso cref="CoherenceSource"/>
public interface ICoherenceService
{
    #region Query Methods

    /// <summary>
    /// Gets the current coherence state for a character.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an Arcanist
    /// specialization character.
    /// </param>
    /// <returns>
    /// A <see cref="CoherenceState"/> snapshot containing the current coherence value,
    /// computed threshold tier, spell power bonus, critical cast chance, cascade risk,
    /// apotheosis status, and meditation availability; or <c>null</c> if the character
    /// has no coherence state (non-Arcanist).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a read-only query with no side effects. The returned
    /// <see cref="CoherenceState"/> is an immutable value object — modifying it
    /// does not affect the character's persisted coherence.
    /// </para>
    /// <para>
    /// Returns <c>null</c> for characters who are not Arcanist specialization.
    /// Use the convenience methods <see cref="GetSpellPowerBonus"/>,
    /// <see cref="GetCriticalCastChance"/>, and <see cref="CanMeditate"/>
    /// when only a single derived value is needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = coherenceService.GetCoherenceState(characterId);
    /// if (state is not null)
    /// {
    ///     Console.WriteLine($"Coherence: {state.CurrentCoherence}/{CoherenceState.MaxCoherence}");
    ///     Console.WriteLine($"Threshold: {state.Threshold}");
    ///     Console.WriteLine($"Spell Power: {state.SpellPowerBonus:+#;-#;0}");
    ///     Console.WriteLine($"In Apotheosis: {state.InApotheosis}");
    /// }
    /// </code>
    /// </example>
    CoherenceState? GetCoherenceState(Guid characterId);

    /// <summary>
    /// Gets the spell power bonus from the character's coherence threshold.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// Spell power bonus (-2 to +5) scaled by threshold:
    /// Destabilized (-2), Unstable (-1), Balanced (0), Focused (+2), Apotheosis (+5).
    /// Returns 0 if character has no coherence state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetCoherenceState(characterId)?.SpellPowerBonus ?? 0</c>
    /// </para>
    /// <para>
    /// Negative values at low coherence represent unstable spell formation,
    /// while positive values represent enhanced arcane focus. The +5 bonus
    /// at Apotheosis threshold reflects the Arcanist's transcendent power.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During spell damage calculation
    /// int coherenceBonus = coherenceService.GetSpellPowerBonus(characterId);
    /// int totalSpellPower = baseSpellPower + coherenceBonus;
    /// </code>
    /// </example>
    int GetSpellPowerBonus(Guid characterId);

    /// <summary>
    /// Gets the critical casting chance bonus from the character's coherence threshold.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// Critical casting chance as percentage (0-20) scaled by threshold:
    /// Destabilized/Unstable (0%), Balanced (5%), Focused (10%), Apotheosis (20%).
    /// Returns 0 if character has no coherence state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetCoherenceState(characterId)?.CriticalCastChance ?? 0</c>
    /// </para>
    /// <para>
    /// Low coherence eliminates critical casting potential entirely, as the
    /// Arcanist cannot focus enough to achieve precise spell effects. The
    /// 20% chance at Apotheosis represents supernatural arcane precision.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During spell resolution
    /// int critChance = coherenceService.GetCriticalCastChance(characterId);
    /// if (diceService.RollPercentile() &lt;= critChance)
    ///     ApplyCriticalSpellEffect();
    /// </code>
    /// </example>
    int GetCriticalCastChance(Guid characterId);

    /// <summary>
    /// Determines if meditation is available for the character.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// <c>true</c> if meditation is available (character is outside combat and
    /// has an active coherence state); <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Meditation is a coherence recovery action that can only be performed
    /// outside of combat. It restores 20 coherence per action.
    /// </para>
    /// <para>
    /// Apotheosis state does not prevent meditation outside combat — an Arcanist
    /// can meditate to maintain or increase coherence even while in Apotheosis.
    /// </para>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetCoherenceState(characterId)?.CanMeditate ?? false</c>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During exploration
    /// if (coherenceService.CanMeditate(characterId))
    /// {
    ///     ShowMeditationPrompt();
    /// }
    /// </code>
    /// </example>
    bool CanMeditate(Guid characterId);

    #endregion

    #region Command Methods

    /// <summary>
    /// Applies coherence gain from a specified source.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an Arcanist
    /// specialization character with an active coherence state.
    /// </param>
    /// <param name="amount">
    /// Amount of coherence to gain. Must be a positive integer. Typical values:
    /// <list type="bullet">
    ///   <item><description>SuccessfulCast: 5</description></item>
    ///   <item><description>ControlledChannel: 3 per turn</description></item>
    ///   <item><description>MeditationAction: 20</description></item>
    ///   <item><description>StabilityField: 1-10 variable</description></item>
    /// </list>
    /// </param>
    /// <param name="source">
    /// The source of coherence generation. Used for logging and analysis.
    /// </param>
    /// <returns>
    /// <c>true</c> if gain was applied successfully; <c>false</c> if the character
    /// has no coherence state (non-Arcanist).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Coherence is capped at 100 (<see cref="CoherenceState.MaxCoherence"/>).
    /// Excess coherence is discarded.
    /// </para>
    /// <para>
    /// <strong>Threshold Considerations:</strong>
    /// Gaining coherence moves the Arcanist toward stability (Balanced) or
    /// transcendence (Apotheosis). Moving from Unstable to Balanced is generally
    /// desirable; moving into Apotheosis grants great power but costs stress.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Award coherence for successful spell
    /// bool success = coherenceService.GainCoherence(
    ///     characterId,
    ///     amount: 5,
    ///     source: CoherenceSource.SuccessfulCast);
    ///
    /// if (success)
    /// {
    ///     var state = coherenceService.GetCoherenceState(characterId);
    ///     UpdateCoherenceDisplay(state?.CurrentCoherence ?? 0);
    /// }
    /// </code>
    /// </example>
    bool GainCoherence(Guid characterId, int amount, CoherenceSource source);

    /// <summary>
    /// Applies coherence loss with a specified reason.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an Arcanist
    /// specialization character with an active coherence state.
    /// </param>
    /// <param name="amount">
    /// Amount of coherence to lose. Must be a positive integer.
    /// </param>
    /// <param name="reason">
    /// Description of why coherence is being lost. Used for logging and UI feedback.
    /// Typical reasons: "Failed Cast", "Spell Interrupted", "Psychic Attack".
    /// </param>
    /// <returns>
    /// <c>true</c> if loss was applied successfully; <c>false</c> if the character
    /// has no coherence state (non-Arcanist).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Coherence is clamped at 0 (<see cref="CoherenceState.MinCoherence"/>).
    /// Excess loss is discarded — coherence cannot go negative.
    /// </para>
    /// <para>
    /// <strong>Cascade Warning:</strong>
    /// Losing coherence may push the Arcanist into Unstable or Destabilized
    /// thresholds where cascade risk increases significantly. After significant
    /// coherence loss, consider calling <see cref="CheckCascade"/> before the
    /// next spellcast.
    /// </para>
    /// <para>
    /// Also updates LastCastTime if the loss is spell-related.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Spell was interrupted
    /// bool applied = coherenceService.LoseCoherence(
    ///     characterId,
    ///     amount: 15,
    ///     reason: "Spell Interrupted");
    ///
    /// if (applied)
    /// {
    ///     var state = coherenceService.GetCoherenceState(characterId);
    ///     if (state?.CascadeRisk > 0)
    ///         ShowCascadeWarning();
    /// }
    /// </code>
    /// </example>
    bool LoseCoherence(Guid characterId, int amount, string reason);

    /// <summary>
    /// Checks whether a cascade effect should trigger.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// A <see cref="CascadeResult"/> indicating:
    /// <list type="bullet">
    ///   <item><description>Whether cascade triggered</description></item>
    ///   <item><description>Coherence lost from cascade</description></item>
    ///   <item><description>Self-damage (if any)</description></item>
    ///   <item><description>Stress gained (if any)</description></item>
    ///   <item><description>Corruption gained (if any)</description></item>
    ///   <item><description>Whether the triggering spell was disrupted</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Cascade Check:</strong>
    /// Cascade risk is based on current threshold:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Destabilized (0-20): 25% chance per cast</description></item>
    ///   <item><description>Unstable (21-40): 10% chance per cast</description></item>
    ///   <item><description>Balanced and above: 0% (no cascade possible)</description></item>
    /// </list>
    /// <para>
    /// <strong>Effect Application:</strong>
    /// This method only returns the cascade result — it does NOT automatically
    /// apply cascade effects (damage, stress, corruption). The caller is
    /// responsible for applying effects based on the result.
    /// </para>
    /// <para>
    /// Cascade effect determination is handled by a random table (v0.19.x).
    /// The CascadeEffectId in the result identifies which specific effect occurred.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check cascade before casting at low coherence
    /// var cascadeResult = coherenceService.CheckCascade(characterId);
    ///
    /// if (cascadeResult.CascadeTriggered)
    /// {
    ///     if (cascadeResult.SelfDamage.HasValue)
    ///         ApplySelfDamage(characterId, cascadeResult.SelfDamage.Value);
    ///     
    ///     if (cascadeResult.StressGained.HasValue)
    ///         stressService.ApplyStress(characterId, cascadeResult.StressGained.Value, StressSource.Heretical);
    ///     
    ///     if (cascadeResult.SpellDisrupted)
    ///         CancelCurrentSpell();
    ///     
    ///     ShowCascadeAnimation(cascadeResult.CascadeEffectId);
    /// }
    /// </code>
    /// </example>
    CascadeResult CheckCascade(Guid characterId);

    /// <summary>
    /// Updates Apotheosis state and returns transition result.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// An <see cref="ApotheosisResult"/> indicating:
    /// <list type="bullet">
    ///   <item><description>Whether Apotheosis was entered this update</description></item>
    ///   <item><description>Turns remaining (if in Apotheosis)</description></item>
    ///   <item><description>Abilities unlocked (on first entry)</description></item>
    ///   <item><description>Stress cost per turn (10)</description></item>
    ///   <item><description>Whether Apotheosis was exited this update</description></item>
    ///   <item><description>Exit reason (if exited)</description></item>
    ///   <item><description>Final coherence after update</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Apotheosis Entry:</strong>
    /// Entry occurs when coherence reaches 81+ and the character is not already
    /// in Apotheosis. On first entry ever, special abilities are permanently unlocked.
    /// </para>
    /// <para>
    /// <strong>Apotheosis Maintenance:</strong>
    /// While in Apotheosis, the Arcanist pays 10 stress per turn. This cost is
    /// recorded but not automatically applied — the caller should use
    /// <see cref="IStressService"/> to apply the stress.
    /// </para>
    /// <para>
    /// <strong>Apotheosis Exit:</strong>
    /// Exit occurs when:
    /// <list type="bullet">
    ///   <item><description>Coherence drops below 81</description></item>
    ///   <item><description>Stress reaches 100 (forced exit)</description></item>
    ///   <item><description>Voluntary exit during non-combat</description></item>
    /// </list>
    /// Cannot voluntarily exit during combat.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check apotheosis status at start of turn
    /// var apoResult = coherenceService.UpdateApotheosis(characterId);
    ///
    /// if (apoResult.EnteredApotheosis)
    /// {
    ///     ShowApotheosisEntryAnimation();
    ///     foreach (var ability in apoResult.AbilitiesUnlocked)
    ///         UnlockAbility(characterId, ability);
    /// }
    ///
    /// if (apoResult.TurnsRemaining.HasValue)
    /// {
    ///     // Apply stress cost
    ///     stressService.ApplyStress(characterId, apoResult.StressCostPerTurn, StressSource.Heretical);
    /// }
    ///
    /// if (apoResult.ExitedApotheosis)
    ///     ShowApotheosisExitNotification(apoResult.ExitReason);
    /// </code>
    /// </example>
    ApotheosisResult UpdateApotheosis(Guid characterId);

    /// <summary>
    /// Performs a meditation action to restore coherence.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier.
    /// </param>
    /// <returns>
    /// <c>true</c> if meditation was performed successfully; <c>false</c> if
    /// meditation is not available (in combat or no coherence state).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Meditation grants 20 coherence and is only available outside of combat.
    /// The action represents the Arcanist focusing their mind to stabilize
    /// their connection to arcane forces.
    /// </para>
    /// <para>
    /// <strong>Interruption:</strong>
    /// Meditation can be interrupted by taking damage during the action. If
    /// interrupted, no coherence is gained and the action is wasted.
    /// </para>
    /// <para>
    /// <strong>Strategic Use:</strong>
    /// Meditation is especially valuable when an Arcanist has dropped to
    /// Unstable or Destabilized thresholds, as it provides a reliable way
    /// to return to the safe Balanced threshold.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During exploration, player requests meditation
    /// if (coherenceService.CanMeditate(characterId))
    /// {
    ///     bool success = coherenceService.Meditate(characterId);
    ///     if (success)
    ///     {
    ///         var state = coherenceService.GetCoherenceState(characterId);
    ///         ShowMeditationSuccess($"Coherence restored to {state?.CurrentCoherence}");
    ///     }
    /// }
    /// else
    /// {
    ///     ShowMessage("Cannot meditate during combat!");
    /// }
    /// </code>
    /// </example>
    bool Meditate(Guid characterId);

    #endregion
}
