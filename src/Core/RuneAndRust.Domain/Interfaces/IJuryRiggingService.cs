// ------------------------------------------------------------------------------
// <copyright file="IJuryRiggingService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for handling the five-step jury-rigging procedure.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service interface for handling the five-step jury-rigging procedure.
/// </summary>
/// <remarks>
/// <para>
/// The jury-rigging procedure follows a five-step trial-and-error flow:
/// <list type="number">
///   <item><description>Observe (optional): Study the mechanism visually (WITS DC 10)</description></item>
///   <item><description>Probe (automatic): Try obvious buttons and levers</description></item>
///   <item><description>Pattern (optional): Recognize mechanism type (WITS DC 12)</description></item>
///   <item><description>Method Selection: Choose a bypass approach</description></item>
///   <item><description>Experiment: Attempt the bypass (System Bypass check)</description></item>
///   <item><description>Iterate: Learn from failure (DC -1 next attempt)</description></item>
/// </list>
/// </para>
/// <para>
/// The procedure loops from Iterate back to Method Selection until success,
/// permanent lockout, mechanism destruction, or player abandonment.
/// </para>
/// </remarks>
public interface IJuryRiggingService
{
    // -------------------------------------------------------------------------
    // Session Initialization
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initiates a new jury-rigging session for the specified mechanism.
    /// </summary>
    /// <param name="characterId">The ID of the character attempting bypass.</param>
    /// <param name="mechanismType">The type category of the mechanism.</param>
    /// <param name="mechanismName">The display name of the mechanism.</param>
    /// <param name="baseDc">The base DC for bypassing this mechanism.</param>
    /// <param name="isGlitched">Whether the mechanism is in a glitched state.</param>
    /// <param name="knownMechanismTypes">Types the character is already familiar with.</param>
    /// <returns>A new JuryRigState initialized at the Observe step.</returns>
    /// <remarks>
    /// <para>
    /// Familiarity is determined by checking if <paramref name="mechanismType"/>
    /// exists in <paramref name="knownMechanismTypes"/>. Familiar mechanisms
    /// grant +2d10 bonus dice on experiment rolls.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when required parameters are invalid.</exception>
    JuryRigState InitiateJuryRig(
        string characterId,
        string mechanismType,
        string mechanismName,
        int baseDc,
        bool isGlitched,
        IEnumerable<string>? knownMechanismTypes = null);

    // -------------------------------------------------------------------------
    // Observe Step
    // -------------------------------------------------------------------------

    /// <summary>
    /// Performs the optional Observe step to study the mechanism.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <param name="witsScore">The character's WITS attribute score.</param>
    /// <returns>The observation result with any revealed hints.</returns>
    /// <remarks>
    /// <para>
    /// Uses WITS check against DC 10.
    /// </para>
    /// <para>
    /// Outcomes:
    /// <list type="bullet">
    ///   <item><description>Success: Learn mechanism type, reveal potential bypass methods</description></item>
    ///   <item><description>Failure: No information gained, may still proceed</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when not at Observe step or session is terminal.</exception>
    ObservationResult PerformObservation(JuryRigState state, int witsScore);

    /// <summary>
    /// Skips the Observe step, proceeding directly to Probe.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <returns>A skipped observation result.</returns>
    /// <remarks>
    /// Skipping observation forfeits any hints that might have been revealed.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when not at Observe step or session is terminal.</exception>
    ObservationResult SkipObservation(JuryRigState state);

    // -------------------------------------------------------------------------
    // Probe Step
    // -------------------------------------------------------------------------

    /// <summary>
    /// Performs the automatic Probe step.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <returns>The probe result with observed reactions.</returns>
    /// <remarks>
    /// <para>
    /// This step is automatic (no roll required).
    /// </para>
    /// <para>
    /// Outcomes:
    /// <list type="bullet">
    ///   <item><description>Machine reacts: Beeps, lights, sounds, or movement</description></item>
    ///   <item><description>No reaction: Mechanism appears dormant or unpowered</description></item>
    ///   <item><description>Glitched behavior: Erratic responses, enables Glitch Exploitation</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when not at Probe step or session is terminal.</exception>
    ProbeResult PerformProbe(JuryRigState state);

    // -------------------------------------------------------------------------
    // Pattern Recognition Step
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts the optional Pattern Recognition step.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <param name="witsScore">The character's WITS attribute score.</param>
    /// <returns>The pattern recognition result.</returns>
    /// <remarks>
    /// <para>
    /// Uses WITS check against DC 12.
    /// </para>
    /// <para>
    /// Outcomes:
    /// <list type="bullet">
    ///   <item><description>Success + Familiar: +2d10 bonus dice on experiment</description></item>
    ///   <item><description>Success + Unfamiliar: Mark mechanism type as "seen" for future</description></item>
    ///   <item><description>Failure: No recognition, proceed without bonus</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when not at Pattern step or session is terminal.</exception>
    PatternResult AttemptPatternRecognition(JuryRigState state, int witsScore);

    /// <summary>
    /// Skips the Pattern Recognition step.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <returns>A skipped pattern recognition result.</returns>
    /// <remarks>
    /// Skipping pattern recognition forfeits any familiarity bonus.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when not at Pattern step or session is terminal.</exception>
    PatternResult SkipPatternRecognition(JuryRigState state);

    // -------------------------------------------------------------------------
    // Method Selection
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the available bypass methods for the current context.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <returns>A list of method options with availability and modifiers.</returns>
    /// <remarks>
    /// <para>
    /// Method availability depends on:
    /// <list type="bullet">
    ///   <item><description>MemorizedSequence: Requires familiarity with mechanism type</description></item>
    ///   <item><description>GlitchExploitation: Requires mechanism to be [Glitched]</description></item>
    ///   <item><description>Other methods: Always available</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IReadOnlyList<MethodOption> GetAvailableMethods(JuryRigState state);

    /// <summary>
    /// Selects a bypass method for the experiment.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <param name="method">The bypass method to use.</param>
    /// <exception cref="InvalidOperationException">Thrown when not at MethodSelection step or session is terminal.</exception>
    /// <exception cref="ArgumentException">Thrown when method is not valid for current context.</exception>
    void SelectMethod(JuryRigState state, BypassMethod method);

    // -------------------------------------------------------------------------
    // Experiment
    // -------------------------------------------------------------------------

    /// <summary>
    /// Performs the Experiment step with the selected bypass method.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <param name="context">The jury-rig context with method and modifiers.</param>
    /// <param name="systemBypassScore">The character's System Bypass skill score.</param>
    /// <returns>The experiment result with outcome and consequences.</returns>
    /// <remarks>
    /// <para>
    /// Uses System Bypass check against the modified DC.
    /// </para>
    /// <para>
    /// Outcomes:
    /// <list type="bullet">
    ///   <item><description>Critical Success (net ≥ 5): Bypass + salvage components</description></item>
    ///   <item><description>Success (net > 0): Mechanism bypassed</description></item>
    ///   <item><description>Failure (net ≤ 0): Roll on complication table</description></item>
    ///   <item><description>Fumble (0 successes + botch): Mechanism destroyed</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when not at Experiment step or session is terminal.</exception>
    JuryRigResult PerformExperiment(JuryRigState state, JuryRigContext context, int systemBypassScore);

    // -------------------------------------------------------------------------
    // Complication Processing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Processes a complication roll from the d10 table.
    /// </summary>
    /// <param name="roll">The d10 roll result (1-10).</param>
    /// <returns>The complication effect for the roll.</returns>
    /// <remarks>
    /// <para>
    /// Complication table:
    /// <list type="bullet">
    ///   <item><description>1: PermanentLock - Machine locks permanently</description></item>
    ///   <item><description>2-3: AlarmTriggered - Security alert</description></item>
    ///   <item><description>4-5: SparksFly - 1d6 electrical damage</description></item>
    ///   <item><description>6-7: Nothing - No effect, may retry</description></item>
    ///   <item><description>8-9: PartialSuccess - One function works</description></item>
    ///   <item><description>10: GlitchInFavor - Auto-success</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when roll is not between 1 and 10.</exception>
    ComplicationEffect ProcessComplication(int roll);

    // -------------------------------------------------------------------------
    // Iteration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies iteration learning after a failed attempt.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <remarks>
    /// <para>
    /// Effects:
    /// <list type="bullet">
    ///   <item><description>DC reduced by 1 for next attempt (minimum 4)</description></item>
    ///   <item><description>Bypass method reset for new selection</description></item>
    ///   <item><description>State returns to MethodSelection step</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when not at Iterate step or session is terminal.</exception>
    void ApplyIteration(JuryRigState state);

    // -------------------------------------------------------------------------
    // Session Management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Abandons the current jury-rigging session.
    /// </summary>
    /// <param name="state">The current jury-rigging state.</param>
    /// <remarks>
    /// The mechanism remains in its current state and may be attempted again
    /// in a new session.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when session is already terminal.</exception>
    void AbandonSession(JuryRigState state);

    // -------------------------------------------------------------------------
    // Information Queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the salvageable components for a mechanism type.
    /// </summary>
    /// <param name="mechanismType">The type of mechanism.</param>
    /// <returns>List of component IDs that can be salvaged.</returns>
    IReadOnlyList<string> GetSalvageableComponents(string mechanismType);

    /// <summary>
    /// Gets hints for a mechanism type based on observation success level.
    /// </summary>
    /// <param name="mechanismType">The type of mechanism.</param>
    /// <param name="netSuccesses">The net successes from the observation roll.</param>
    /// <returns>List of hints revealed at this success level.</returns>
    IReadOnlyList<string> GetMechanismHints(string mechanismType, int netSuccesses);

    /// <summary>
    /// Gets the probe reactions for a mechanism type.
    /// </summary>
    /// <param name="mechanismType">The type of mechanism.</param>
    /// <param name="isGlitched">Whether the mechanism is glitched.</param>
    /// <param name="isPowered">Whether the mechanism has power.</param>
    /// <returns>List of observable reactions from probing.</returns>
    IReadOnlyList<string> GetProbeReactions(string mechanismType, bool isGlitched, bool isPowered);
}
