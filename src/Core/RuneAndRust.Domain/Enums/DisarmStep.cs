// ------------------------------------------------------------------------------
// <copyright file="DisarmStep.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the three-step trap disarmament procedure.
// Part of v0.15.4d Trap Disarmament System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the three-step trap disarmament procedure.
/// </summary>
/// <remarks>
/// <para>
/// The trap disarmament procedure follows three sequential steps:
/// <list type="number">
///   <item><description>Detection: Find the trap before triggering it (Perception check)</description></item>
///   <item><description>Analysis: Study the trap to reveal information (optional, WITS check)</description></item>
///   <item><description>Disarmament: Neutralize the trap (higher of WITS or FINESSE)</description></item>
/// </list>
/// </para>
/// <para>
/// Detection is required—failed detection triggers the trap immediately.
/// Analysis is optional but provides valuable information and a +1d10 bonus
/// on subsequent disarmament if a hint is revealed.
/// </para>
/// </remarks>
public enum DisarmStep
{
    /// <summary>
    /// Step 1: Find the trap before triggering it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses Perception check against the trap's detection DC.
    /// Failure means walking into the trap (immediate trigger).
    /// Success allows proceeding to Analysis or Disarmament.
    /// </para>
    /// </remarks>
    Detection = 0,

    /// <summary>
    /// Step 2: Study the trap to reveal information (optional).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses WITS check against the trap's base DC with tiered thresholds:
    /// <list type="bullet">
    ///   <item><description>DC - 2: Reveal disarm DC</description></item>
    ///   <item><description>DC: Reveal failure consequences</description></item>
    ///   <item><description>DC + 2: Reveal disarm hint (+1d10 on disarmament)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Analysis is optional—characters may skip directly to Disarmament.
    /// </para>
    /// </remarks>
    Analysis = 1,

    /// <summary>
    /// Step 3: Neutralize the trap.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses higher of WITS or FINESSE against the trap's disarm DC.
    /// Tool quality modifiers apply:
    /// <list type="bullet">
    ///   <item><description>BareHands: -2d10 (only for DC &lt; 4)</description></item>
    ///   <item><description>Improvised: +0</description></item>
    ///   <item><description>Proper: +1d10</description></item>
    ///   <item><description>Masterwork: +2d10</description></item>
    /// </list>
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
    Disarmament = 2
}
