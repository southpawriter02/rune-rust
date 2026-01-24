// ------------------------------------------------------------------------------
// <copyright file="ITrapDisarmamentService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for handling the three-step trap disarmament procedure.
// Part of v0.15.4d Trap Disarmament System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service interface for handling the three-step trap disarmament procedure.
/// </summary>
/// <remarks>
/// <para>
/// The trap disarmament procedure follows three steps:
/// <list type="number">
///   <item><description>Detection: Find the trap before triggering it</description></item>
///   <item><description>Analysis: Study the trap to reveal information (optional)</description></item>
///   <item><description>Disarmament: Neutralize the trap</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ITrapDisarmamentService
{
    // -------------------------------------------------------------------------
    // Detection
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to detect a trap before triggering it.
    /// </summary>
    /// <param name="player">The player attempting detection.</param>
    /// <param name="trapType">The type of trap in the area.</param>
    /// <returns>Detection result, including trap trigger if failed.</returns>
    /// <remarks>
    /// <para>
    /// Uses Perception check against the trap's detection DC.
    /// </para>
    /// <para>
    /// Outcomes:
    /// <list type="bullet">
    ///   <item><description>Success: Trap detected, may proceed to Analysis or Disarmament</description></item>
    ///   <item><description>Failure: Walk into trap, trigger effects immediately</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    TrapDisarmResult AttemptDetection(Player player, TrapType trapType);

    // -------------------------------------------------------------------------
    // Analysis
    // -------------------------------------------------------------------------

    /// <summary>
    /// Analyzes a detected trap to reveal information.
    /// </summary>
    /// <param name="state">The current disarmament state.</param>
    /// <param name="player">The player performing analysis.</param>
    /// <returns>Analysis info with revealed information.</returns>
    /// <remarks>
    /// <para>
    /// Uses WITS check against tiered thresholds:
    /// <list type="bullet">
    ///   <item><description>DC - 2: Reveal disarm DC</description></item>
    ///   <item><description>DC: Reveal failure consequences</description></item>
    ///   <item><description>DC + 2: Reveal hint (+1d10 on disarmament)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Analysis is optional—characters may skip directly to Disarmament.
    /// </para>
    /// </remarks>
    TrapAnalysisInfo AnalyzeTrap(TrapDisarmState state, Player player);

    // -------------------------------------------------------------------------
    // Disarmament
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to disarm a detected trap.
    /// </summary>
    /// <param name="state">The current disarmament state.</param>
    /// <param name="player">The player attempting disarmament.</param>
    /// <param name="toolQuality">Quality of tools being used.</param>
    /// <returns>Disarmament result, including salvage or fumble consequences.</returns>
    /// <remarks>
    /// <para>
    /// Uses higher of WITS or FINESSE against the trap's disarm DC.
    /// </para>
    /// <para>
    /// Outcomes:
    /// <list type="bullet">
    ///   <item><description>Success: Trap disabled</description></item>
    ///   <item><description>Critical (net ≥ 5): Trap disabled + salvage components</description></item>
    ///   <item><description>Failure: DC +1 for next attempt</description></item>
    ///   <item><description>Fumble: [Forced Execution] - trap triggers on disarmer</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting DC 4+ trap with BareHands.
    /// </exception>
    TrapDisarmResult AttemptDisarmament(TrapDisarmState state, Player player, ToolQuality toolQuality);

    // -------------------------------------------------------------------------
    // Trap Information
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the trap effect when triggered (damage, alerts, etc.).
    /// </summary>
    /// <param name="trapType">The type of trap triggered.</param>
    /// <returns>
    /// Tuple of:
    /// <list type="bullet">
    ///   <item><description>DamageDice: Dice expression for damage (e.g., "2d10")</description></item>
    ///   <item><description>DamageType: Type of damage (e.g., "physical", "lightning")</description></item>
    ///   <item><description>Alert: Whether the trap triggers an alert</description></item>
    ///   <item><description>Lockdown: Whether the trap triggers a lockdown</description></item>
    /// </list>
    /// </returns>
    (string DamageDice, string DamageType, bool Alert, bool Lockdown) GetTrapEffect(TrapType trapType);

    /// <summary>
    /// Gets the salvageable components for a trap type.
    /// </summary>
    /// <param name="trapType">The type of trap.</param>
    /// <returns>List of component IDs that can be salvaged on critical success.</returns>
    IReadOnlyList<string> GetSalvageableComponents(TrapType trapType);

    /// <summary>
    /// Gets the detection DC for a trap type.
    /// </summary>
    /// <param name="trapType">The type of trap.</param>
    /// <returns>The detection DC.</returns>
    int GetDetectionDc(TrapType trapType);

    /// <summary>
    /// Gets the base disarm DC for a trap type.
    /// </summary>
    /// <param name="trapType">The type of trap.</param>
    /// <returns>The base disarm DC (before failure escalation).</returns>
    int GetDisarmDc(TrapType trapType);
}
