// ------------------------------------------------------------------------------
// <copyright file="BypassEffectType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the types of effects that specialization abilities can apply
// in the System Bypass skill system.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of effects applied by specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// Effect types categorize what the ability does when it activates:
/// <list type="bullet">
///   <item><description><see cref="AutoDetection"/>: Automatically reveal something</description></item>
///   <item><description><see cref="DcReduction"/>: Reduce the difficulty class</description></item>
///   <item><description><see cref="TimeReduction"/>: Reduce time required</description></item>
///   <item><description><see cref="PenaltyNegation"/>: Negate penalties</description></item>
///   <item><description><see cref="UpgradeResult"/>: Improve the outcome</description></item>
///   <item><description><see cref="UnlockRecipes"/>: Make new options available</description></item>
///   <item><description><see cref="UniqueAction"/>: Allow a new action</description></item>
/// </list>
/// </para>
/// </remarks>
public enum BypassEffectType
{
    /// <summary>
    /// No effect. Default value.
    /// </summary>
    [Description("No effect")]
    None = 0,

    /// <summary>
    /// Automatic detection of something (traps, dangers, etc.).
    /// </summary>
    /// <remarks>
    /// Used by abilities that automatically reveal hidden elements
    /// without requiring a check.
    /// <para>
    /// <b>Abilities:</b> [Sixth Sense] (auto-detect traps within 10 ft)
    /// </para>
    /// </remarks>
    [Description("Auto-detect something")]
    AutoDetection = 1,

    /// <summary>
    /// Reduction to difficulty class.
    /// </summary>
    /// <remarks>
    /// Used by abilities that make bypass attempts easier by reducing
    /// the DC or negating DC penalties.
    /// <para>
    /// <b>Abilities:</b> [Pattern Recognition] (reduce [Glitched] penalty by 2)
    /// </para>
    /// </remarks>
    [Description("Reduce DC")]
    DcReduction = 2,

    /// <summary>
    /// Reduction to time required.
    /// </summary>
    /// <remarks>
    /// Used by abilities that speed up bypass attempts by reducing
    /// the number of rounds required.
    /// <para>
    /// <b>Abilities:</b> [Fast Pick] (reduce time by 1 round, min 1)
    /// </para>
    /// </remarks>
    [Description("Reduce time")]
    TimeReduction = 3,

    /// <summary>
    /// Negation of penalties.
    /// </summary>
    /// <remarks>
    /// Used by abilities that remove penalties that would normally apply,
    /// such as combat penalties or distraction penalties.
    /// <para>
    /// <b>Abilities:</b> [Bypass Under Fire] (negate combat/danger penalties)
    /// </para>
    /// </remarks>
    [Description("Negate penalties")]
    PenaltyNegation = 4,

    /// <summary>
    /// Upgrade the outcome of a bypass.
    /// </summary>
    /// <remarks>
    /// Used by abilities that improve the result of a successful bypass,
    /// such as upgrading access level or improving quality.
    /// <para>
    /// <b>Abilities:</b> [Deep Access] (upgrade to Admin-Level on success)
    /// </para>
    /// </remarks>
    [Description("Upgrade result")]
    UpgradeResult = 5,

    /// <summary>
    /// Unlock new recipes or options.
    /// </summary>
    /// <remarks>
    /// Used by abilities that make previously unavailable options
    /// accessible to the character.
    /// <para>
    /// <b>Abilities:</b> [Master Craftsman] (unlock masterwork recipes)
    /// </para>
    /// </remarks>
    [Description("Unlock recipes")]
    UnlockRecipes = 6,

    /// <summary>
    /// Enable a unique action.
    /// </summary>
    /// <remarks>
    /// Used by abilities that grant new actions not available to
    /// characters without the specialization.
    /// <para>
    /// <b>Abilities:</b> [Relock], [Trap Artist]
    /// </para>
    /// </remarks>
    [Description("Unique action")]
    UniqueAction = 7
}
