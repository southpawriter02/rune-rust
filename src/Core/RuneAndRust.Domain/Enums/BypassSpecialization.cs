// ------------------------------------------------------------------------------
// <copyright file="BypassSpecialization.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the character specializations that focus on System Bypass skills,
// each granting unique abilities for lock manipulation, terminal hacking,
// trap handling, or working under pressure.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Character specializations with System Bypass skill focus.
/// </summary>
/// <remarks>
/// <para>
/// Each bypass specialization grants unique abilities that modify
/// how characters interact with locks, terminals, traps, and Old World technology.
/// </para>
/// <para>
/// <b>Specialization Abilities:</b>
/// <list type="table">
///   <listheader>
///     <term>Specialization</term>
///     <description>Abilities</description>
///   </listheader>
///   <item>
///     <term>ScrapTinker</term>
///     <description>[Master Craftsman] - Craft masterwork tools; [Relock] - Re-secure bypassed locks</description>
///   </item>
///   <item>
///     <term>RuinStalker</term>
///     <description>[Trap Artist] - Re-arm disabled traps; [Sixth Sense] - Auto-detect nearby traps</description>
///   </item>
///   <item>
///     <term>JotunReader</term>
///     <description>[Deep Access] - Auto-gain Admin-Level on success; [Pattern Recognition] - Reduce glitch penalties</description>
///   </item>
///   <item>
///     <term>GantryRunner</term>
///     <description>[Fast Pick] - Reduce bypass time; [Bypass Under Fire] - Ignore combat penalties</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
public enum BypassSpecialization
{
    /// <summary>
    /// No bypass specialization. Standard bypass rules apply.
    /// </summary>
    /// <remarks>
    /// Characters without a bypass-focused specialization do not gain
    /// any special abilities when using System Bypass skills.
    /// </remarks>
    [Description("No bypass specialization")]
    None = 0,

    /// <summary>
    /// Scrap-Tinker specialization focusing on tool crafting and lock manipulation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Masters of salvage and improvised crafting. Scrap-Tinkers understand
    /// the inner workings of Old World mechanisms intimately.
    /// </para>
    /// <para>
    /// <b>Abilities:</b>
    /// <list type="bullet">
    ///   <item><description>[Master Craftsman] - Craft [Masterwork Tools] from rare components</description></item>
    ///   <item><description>[Relock] - Re-lock bypassed locks to slow pursuit (WITS DC 12)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Bypass Focus:</b> Lockpicking (v0.15.4a), Tool Crafting (v0.15.4g)
    /// </para>
    /// </remarks>
    [Description("Tool crafting and lock manipulation")]
    ScrapTinker = 1,

    /// <summary>
    /// Ruin-Stalker specialization focusing on trap detection and manipulation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Experts in navigating trapped environments. Ruin-Stalkers have developed
    /// an almost supernatural sense for danger.
    /// </para>
    /// <para>
    /// <b>Abilities:</b>
    /// <list type="bullet">
    ///   <item><description>[Trap Artist] - Re-arm disabled traps as party defenses (WITS DC 14)</description></item>
    ///   <item><description>[Sixth Sense] - Automatically detect traps within 10 feet</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Bypass Focus:</b> Trap Disarmament (v0.15.4d)
    /// </para>
    /// </remarks>
    [Description("Trap detection and manipulation")]
    RuinStalker = 2,

    /// <summary>
    /// Jötun-Reader specialization focusing on terminal hacking and glitch exploitation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Those who commune with ancient technology. Jötun-Readers have learned
    /// to speak the language of Old World machines.
    /// </para>
    /// <para>
    /// <b>Abilities:</b>
    /// <list type="bullet">
    ///   <item><description>[Deep Access] - Any terminal hack success grants Admin-Level access</description></item>
    ///   <item><description>[Pattern Recognition] - Reduce [Glitched] DC penalty by 2</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Bypass Focus:</b> Terminal Hacking (v0.15.4b), Glitch Exploitation (v0.15.4f)
    /// </para>
    /// </remarks>
    [Description("Terminal hacking and glitch exploitation")]
    JotunReader = 3,

    /// <summary>
    /// Gantry-Runner specialization focusing on speed and working under pressure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Speed and precision under pressure. Gantry-Runners have learned to work
    /// fast and stay calm when danger looms.
    /// </para>
    /// <para>
    /// <b>Abilities:</b>
    /// <list type="bullet">
    ///   <item><description>[Fast Pick] - Reduce bypass time by 1 round (minimum 1)</description></item>
    ///   <item><description>[Bypass Under Fire] - Ignore penalties when in combat or under threat</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Bypass Focus:</b> All bypass systems (speed and pressure resistance)
    /// </para>
    /// </remarks>
    [Description("Speed and working under pressure")]
    GantryRunner = 4
}
