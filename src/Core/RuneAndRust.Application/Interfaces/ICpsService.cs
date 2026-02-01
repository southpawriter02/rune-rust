// ═══════════════════════════════════════════════════════════════════════════════
// ICpsService.cs
// Defines the Application Layer contract for the Cognitive Paradox Syndrome (CPS)
// service. CPS tracks mental deterioration from processing reality-bending
// paradoxes, derived from Psychic Stress. Unlike Corruption (physical taint),
// CPS can recover when stress is reduced.
// Version: 0.18.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Exceptions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines the contract for managing a character's Cognitive Paradox Syndrome (CPS) state.
/// </summary>
/// <remarks>
/// <para>
/// CPS is a trauma mechanic derived from Psychic Stress that tracks mental
/// deterioration from processing reality-bending paradoxes. Unlike Corruption
/// (physical taint), CPS can recover when stress is reduced.
/// </para>
/// <para>
/// <strong>CPS Stages:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>None (0-19 stress): Clear-minded, no symptoms</description></item>
///   <item><description>WeightOfKnowing (20-39 stress): Reality feels "off"</description></item>
///   <item><description>GlimmerMadness (40-59 stress): Reality flickers and distorts</description></item>
///   <item><description>RuinMadness (60-79 stress): Panic Table active, mind fracturing</description></item>
///   <item><description>HollowShell (80+ stress): Terminal state, survival check required</description></item>
/// </list>
/// <para>
/// <strong>Key Differences from ICorruptionService:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>CPS derives from Psychic Stress (recoverable)</description></item>
///   <item><description>Corruption is near-permanent physical taint</description></item>
///   <item><description>CPS uses 5 stages (vs 6 for Corruption)</description></item>
///   <item><description>CPS terminal state is HollowShell at 80+ (vs Consumed at 100)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CpsState"/>
/// <seealso cref="CpsStage"/>
/// <seealso cref="PanicEffect"/>
/// <seealso cref="IStressService"/>
/// <seealso cref="ICorruptionService"/>
public interface ICpsService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STATE QUERY METHODS — Retrieve Current CPS Information
    // ═══════════════════════════════════════════════════════════════════════════

    #region State Query Methods

    /// <summary>
    /// Gets the complete CPS state for a character.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <returns>
    /// A <see cref="CpsState"/> containing the character's current stress level,
    /// stage, and computed properties (RequiresPanicCheck, IsTerminal, etc.).
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method retrieves the character's Psychic Stress from <see cref="IStressService"/>
    /// and constructs a <see cref="CpsState"/> snapshot. The state includes computed
    /// properties like <c>RequiresPanicCheck</c> and <c>PercentageToHollowShell</c>.
    /// </para>
    /// <para>
    /// For UI rendering, prefer <see cref="GetUiEffects"/> which returns only
    /// the visual distortion parameters.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var cpsState = cpsService.GetCpsState(characterId);
    /// if (cpsState.RequiresPanicCheck)
    /// {
    ///     // Character is in RuinMadness or HollowShell
    ///     var panicResult = cpsService.RollPanicTable(characterId);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="GetCurrentStage"/>
    /// <seealso cref="GetUiEffects"/>
    CpsState GetCpsState(Guid characterId);

    /// <summary>
    /// Gets the current CPS stage for a character.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <returns>
    /// The <see cref="CpsStage"/> representing the character's current mental state.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method that returns only the stage without constructing
    /// the full <see cref="CpsState"/>. Useful for quick stage checks in game logic.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var stage = cpsService.GetCurrentStage(characterId);
    /// if (stage >= CpsStage.RuinMadness)
    /// {
    ///     // Trigger Panic Table on stress-inducing events
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="GetCpsState"/>
    CpsStage GetCurrentStage(Guid characterId);

    /// <summary>
    /// Gets the UI distortion effects for a character's current CPS stage.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <returns>
    /// A <see cref="CpsUiEffects"/> containing visual effect parameters for
    /// the character's current CPS stage (distortion intensity, text glitching,
    /// leetspeak level, color tint, etc.).
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The UI layer should call this method to retrieve visual parameters for
    /// rendering the game interface. As CPS worsens, effects intensify:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>WeightOfKnowing: Subtle peripheral static</description></item>
    ///   <item><description>GlimmerMadness: Text glitching, moderate distortion</description></item>
    ///   <item><description>RuinMadness: Heavy distortion, maximum leetspeak</description></item>
    ///   <item><description>HollowShell: Screen blackout</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var effects = cpsService.GetUiEffects(characterId);
    /// if (effects.TextGlitching)
    /// {
    ///     ApplyTextGlitchShader(effects.LeetSpeakLevel);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="CpsUiEffects"/>
    CpsUiEffects GetUiEffects(Guid characterId);

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // STAGE TRANSITION METHODS — Detect CPS Stage Changes
    // ═══════════════════════════════════════════════════════════════════════════

    #region Stage Transition Methods

    /// <summary>
    /// Checks whether a stress change would cause a CPS stage transition.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <param name="previousStress">The stress level before the change (0-100).</param>
    /// <param name="newStress">The stress level after the change (0-100).</param>
    /// <returns>
    /// A <see cref="CpsStageChangeResult"/> indicating whether the stage changed,
    /// the direction of change (worsened/improved), and whether critical thresholds
    /// were crossed (RuinMadness or HollowShell entry).
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method compares two stress values to determine if a stage transition
    /// occurred. It is typically called by <see cref="IStressService"/> after
    /// applying stress changes.
    /// </para>
    /// <para>
    /// <strong>Critical Transitions:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>EnteredRuinMadness: Enables Panic Table rolls</description></item>
    ///   <item><description>EnteredHollowShell: Requires survival check</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = cpsService.CheckStageChange(characterId, previousStress: 55, newStress: 65);
    /// if (result.EnteredRuinMadness)
    /// {
    ///     DisplayWarning("Your mind fractures under the weight of paradox!");
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="CpsStageChangeResult"/>
    CpsStageChangeResult CheckStageChange(Guid characterId, int previousStress, int newStress);

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // PANIC TABLE METHODS — Handle RuinMadness Panic Effects
    // ═══════════════════════════════════════════════════════════════════════════

    #region Panic Table Methods

    /// <summary>
    /// Rolls the Panic Table (d10) for a character in RuinMadness or HollowShell stage.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <returns>
    /// A <see cref="PanicResult"/> containing the die roll, panic effect, duration,
    /// status effects to apply, and any forced actions.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the character is not in a stage that requires Panic Table rolls
    /// (i.e., not in RuinMadness or HollowShell).
    /// </exception>
    /// <remarks>
    /// <para>
    /// The Panic Table is triggered when a character in RuinMadness or HollowShell
    /// experiences a stress-inducing event (combat, horror, etc.). The d10 roll
    /// determines the panic effect:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>1 (Frozen): Logic Lock — Stunned 1 round</description></item>
    ///   <item><description>2 (Scream): Primal Scream — Alerts enemies</description></item>
    ///   <item><description>3 (Flee): Evacuation Protocol — Must flee</description></item>
    ///   <item><description>4 (Fetal): Protective Curl — Prone + disadvantage</description></item>
    ///   <item><description>5 (Blackout): System Shutdown — Unconscious 1d4 rounds</description></item>
    ///   <item><description>6 (Denial): Selective Blindness — Cannot perceive threat</description></item>
    ///   <item><description>7 (Violence): Paradox Fury — Attack nearest creature</description></item>
    ///   <item><description>8 (Catatonia): System Crash — Prone + Stunned until hit</description></item>
    ///   <item><description>9 (Dissociation): Reality Slip — Random action</description></item>
    ///   <item><description>10 (None): Lucky Break — No effect</description></item>
    /// </list>
    /// <para>
    /// This method uses <c>IDiceService</c> for the d10 roll. The returned
    /// <see cref="PanicResult"/> should be passed to <see cref="ApplyPanicEffect"/>
    /// to apply the mechanical effects.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (cpsState.RequiresPanicCheck)
    /// {
    ///     var panicResult = cpsService.RollPanicTable(characterId);
    ///     if (!panicResult.IsLuckyBreak)
    ///     {
    ///         DisplayPanicEffect(panicResult.EffectName, panicResult.Description);
    ///         cpsService.ApplyPanicEffect(characterId, panicResult);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="PanicResult"/>
    /// <seealso cref="PanicEffect"/>
    /// <seealso cref="ApplyPanicEffect"/>
    PanicResult RollPanicTable(Guid characterId);

    /// <summary>
    /// Applies a panic effect to a character.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <param name="panicResult">The panic result to apply.</param>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="panicResult"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method applies the mechanical effects of a <see cref="PanicResult"/>:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Applies status effects (Stunned, Prone, Unconscious, etc.)</description></item>
    ///   <item><description>Applies self-damage if applicable</description></item>
    ///   <item><description>Sets forced action flags if the effect forces behavior</description></item>
    /// </list>
    /// <para>
    /// Status effects are applied via the status effect service.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var panicResult = cpsService.RollPanicTable(characterId);
    /// cpsService.ApplyPanicEffect(characterId, panicResult);
    /// 
    /// if (panicResult.ForcesAction)
    /// {
    ///     // Handle forced action (flee, attack nearest, random action)
    ///     HandleForcedAction(characterId, panicResult.ForcedActionType);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="RollPanicTable"/>
    /// <seealso cref="PanicResult"/>
    void ApplyPanicEffect(Guid characterId, PanicResult panicResult);

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // RECOVERY METHODS — Query Recovery Options
    // ═══════════════════════════════════════════════════════════════════════════

    #region Recovery Methods

    /// <summary>
    /// Checks whether a character's current CPS stage is recoverable.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <returns>
    /// <c>true</c> if the character can recover from their current CPS stage through
    /// rest and stress reduction; <c>false</c> if recovery is not possible (RuinMadness
    /// or HollowShell).
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Recovery is possible from None, WeightOfKnowing, and GlimmerMadness stages.
    /// Once a character enters RuinMadness or HollowShell, normal recovery is not
    /// possible — special intervention (GM discretion, Sanctuary Rest) may be required.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (cpsService.IsRecoverable(characterId))
    /// {
    ///     var protocol = cpsService.GetRecoveryProtocol(characterId);
    ///     DisplayRecoverySteps(protocol.Steps);
    /// }
    /// else
    /// {
    ///     DisplayWarning("Recovery is no longer possible through normal means.");
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="GetRecoveryProtocol"/>
    bool IsRecoverable(Guid characterId);

    /// <summary>
    /// Gets the recovery protocol for a character's current CPS stage.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <returns>
    /// A <see cref="CpsRecoveryProtocol"/> containing the protocol name, ordered
    /// recovery steps, estimated recovery time, and urgency level.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Each CPS stage has a defined recovery protocol:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>None: Standard Mental Health — no action needed</description></item>
    ///   <item><description>WeightOfKnowing: Cognitive Hygiene — rest 12-24 hours</description></item>
    ///   <item><description>GlimmerMadness: Silent Room Protocol — isolation 48+ hours</description></item>
    ///   <item><description>RuinMadness: Terminal Protocol — recovery not possible</description></item>
    ///   <item><description>HollowShell: None — character lost</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var protocol = cpsService.GetRecoveryProtocol(characterId);
    /// Console.WriteLine($"Recovery Protocol: {protocol.ProtocolName}");
    /// Console.WriteLine($"Urgency: {protocol.Urgency}");
    /// foreach (var step in protocol.Steps)
    /// {
    ///     Console.WriteLine($"  - {step}");
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="CpsRecoveryProtocol"/>
    /// <seealso cref="IsRecoverable"/>
    CpsRecoveryProtocol GetRecoveryProtocol(Guid characterId);

    #endregion
}
