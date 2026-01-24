// ------------------------------------------------------------------------------
// <copyright file="ComplicationEffect.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The effects from the jury-rigging complication table (d10).
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// The effects from the jury-rigging complication table (d10).
/// </summary>
/// <remarks>
/// <para>
/// When a jury-rigging experiment fails (net successes &lt;= 0 without fumble),
/// a d10 is rolled on this complication table:
/// <list type="bullet">
///   <item><description>1: PermanentLock - Machine locks permanently</description></item>
///   <item><description>2-3: AlarmTriggered - Security alert, enemies alerted</description></item>
///   <item><description>4-5: SparksFly - Take 1d6 electrical damage</description></item>
///   <item><description>6-7: Nothing - No effect, may retry</description></item>
///   <item><description>8-9: PartialSuccess - One function works</description></item>
///   <item><description>10: GlitchInFavor - Lucky malfunction, auto-success</description></item>
/// </list>
/// </para>
/// <para>
/// Results 1 and 10 are terminal (end the attempt with lock or success).
/// Results 2-9 allow continued attempts with DC reduced by 1 from iteration.
/// </para>
/// </remarks>
public enum ComplicationEffect
{
    /// <summary>
    /// d10 = 1: Machine locks permanently.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The mechanism detects tampering and engages a final lockout protocol.
    /// A deep clunk echoes from within as internal bolts slide into place.
    /// No further jury-rigging attempts are possible.
    /// </para>
    /// </remarks>
    [Description("Machine locks permanently")]
    PermanentLock = 0,

    /// <summary>
    /// d10 = 2-3: Alarm triggers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Your fumbling activates a security protocol! A klaxon begins wailing,
    /// and warning lights flash throughout the area.
    /// Enemies within earshot are alerted.
    /// +2 to encounter check, guards investigate in 1d4 rounds.
    /// May continue attempting (alarm already sounding anyway).
    /// </para>
    /// </remarks>
    [Description("Alarm triggered")]
    AlarmTriggered = 1,

    /// <summary>
    /// d10 = 4-5: Sparks fly (1d6 electrical damage).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A shower of sparks erupts from the mechanism, burning your hands and face!
    /// Take 1d6 electrical damage.
    /// Damage is NOT reduced by armor.
    /// May continue attempting (mechanism still functional).
    /// </para>
    /// </remarks>
    [Description("Sparks fly (1d6 damage)")]
    SparksFly = 2,

    /// <summary>
    /// d10 = 6-7: Nothing happens.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The mechanism buzzes indifferently. Your attempt achieves nothing,
    /// but nothing goes wrong either. The machine waits, impassive as ever.
    /// No change to mechanism state.
    /// May attempt again (DC reduced by 1 from iteration).
    /// </para>
    /// </remarks>
    [Description("Nothing happens")]
    Nothing = 3,

    /// <summary>
    /// d10 = 8-9: One function works.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The mechanism partially responds! While not fully bypassed, one of its
    /// secondary functions activates. This may be enough for your purposes.
    /// Specific function depends on mechanism type.
    /// May continue attempting for full access.
    /// </para>
    /// </remarks>
    [Description("Partial success, one function works")]
    PartialSuccess = 4,

    /// <summary>
    /// d10 = 10: Machine glitches in your favor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The machine's own instability works in your favor! A fortuitous glitch
    /// causes the mechanism to bypass itself, flickering and then granting access.
    /// Automatic success—mechanism is bypassed.
    /// The Old World technology, corrupted by centuries, helps you
    /// despite your lack of true understanding.
    /// </para>
    /// </remarks>
    [Description("Glitch in your favor, auto-success")]
    GlitchInFavor = 5
}
