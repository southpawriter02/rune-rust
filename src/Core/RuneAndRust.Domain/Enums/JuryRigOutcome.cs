// ------------------------------------------------------------------------------
// <copyright file="JuryRigOutcome.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The outcome categories for a jury-rigging experiment attempt.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// The outcome categories for a jury-rigging experiment attempt.
/// </summary>
/// <remarks>
/// <para>
/// Outcomes are determined by the roll result and complication table:
/// <list type="bullet">
///   <item><description>CriticalSuccess: Net successes >= 5, yields salvage</description></item>
///   <item><description>Success: Net successes > 0, mechanism bypassed</description></item>
///   <item><description>Failure: Net successes &lt;= 0, roll on complication table</description></item>
///   <item><description>PartialSuccess: Complication roll 8-9, one function works</description></item>
///   <item><description>Fumble: 0 successes + at least 1 botch, mechanism destroyed</description></item>
///   <item><description>PermanentLock: Complication roll 1, permanently locked</description></item>
/// </list>
/// </para>
/// </remarks>
public enum JuryRigOutcome
{
    /// <summary>
    /// Net successes > 0: Mechanism bypassed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The bypass attempt succeeded. The mechanism responds favorably
    /// and the desired access is granted. The mechanism remains intact.
    /// </para>
    /// </remarks>
    [Description("Bypass successful")]
    Success = 0,

    /// <summary>
    /// Net successes >= 5: Bypassed with salvage components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Exceptional success—not only is the mechanism bypassed, but the
    /// character identifies salvageable components that can be claimed.
    /// </para>
    /// </remarks>
    [Description("Critical success with salvage")]
    CriticalSuccess = 1,

    /// <summary>
    /// Net successes &lt;= 0, no botches: Roll on complication table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The attempt failed but without catastrophic consequences.
    /// A roll on the complication table determines what happens next.
    /// The character may typically retry with reduced DC from iteration.
    /// </para>
    /// </remarks>
    [Description("Failed, roll complication")]
    Failure = 2,

    /// <summary>
    /// Complication roll 8-9: One function works.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A partial victory—while the mechanism isn't fully bypassed,
    /// one of its secondary functions activates. This may be enough
    /// for the character's purposes.
    /// </para>
    /// </remarks>
    [Description("Partial success, one function works")]
    PartialSuccess = 3,

    /// <summary>
    /// 0 successes and at least 1 botch: Mechanism destroyed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Catastrophic failure—the mechanism sparks, smokes, and falls silent.
    /// It is permanently destroyed beyond any hope of repair.
    /// No salvage is recoverable from a fumble.
    /// </para>
    /// </remarks>
    [Description("Fumble, mechanism destroyed")]
    Fumble = 4,

    /// <summary>
    /// Complication roll 1: Permanently locked.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The mechanism detects tampering and engages a final lockout protocol.
    /// No further jury-rigging attempts are possible. The character must
    /// find an alternative approach (key, destruction, or alternate route).
    /// </para>
    /// </remarks>
    [Description("Permanently locked")]
    PermanentLock = 5
}
