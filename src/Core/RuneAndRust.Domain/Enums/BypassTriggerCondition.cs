// ------------------------------------------------------------------------------
// <copyright file="BypassTriggerCondition.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the conditions that trigger specialization abilities in the
// System Bypass skill system.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Conditions that trigger specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// Trigger conditions determine when an ability activates. For passive abilities,
/// the condition is checked continuously. For triggered abilities, the condition
/// is evaluated after bypass outcomes.
/// </para>
/// <para>
/// <b>Trigger Condition Mapping:</b>
/// <list type="table">
///   <listheader>
///     <term>Condition</term>
///     <description>Abilities</description>
///   </listheader>
///   <item>
///     <term>ManualActivation</term>
///     <description>[Relock], [Trap Artist]</description>
///   </item>
///   <item>
///     <term>OnMovement</term>
///     <description>[Sixth Sense]</description>
///   </item>
///   <item>
///     <term>OnBypassAttempt</term>
///     <description>[Fast Pick]</description>
///   </item>
///   <item>
///     <term>OnBypassInDanger</term>
///     <description>[Bypass Under Fire]</description>
///   </item>
///   <item>
///     <term>OnGlitchedInteraction</term>
///     <description>[Pattern Recognition]</description>
///   </item>
///   <item>
///     <term>OnTerminalHackSuccess</term>
///     <description>[Deep Access]</description>
///   </item>
///   <item>
///     <term>OnCraftAttempt</term>
///     <description>[Master Craftsman]</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
public enum BypassTriggerCondition
{
    /// <summary>
    /// Manual player activation. Used for unique actions.
    /// </summary>
    /// <remarks>
    /// Abilities with this trigger require the player to explicitly
    /// invoke them as a separate action after a bypass is complete.
    /// <para>
    /// <b>Abilities:</b> [Relock], [Trap Artist]
    /// </para>
    /// </remarks>
    [Description("Manual player activation")]
    ManualActivation = 0,

    /// <summary>
    /// Triggers on character movement.
    /// </summary>
    /// <remarks>
    /// Abilities with this trigger activate when the character moves
    /// into a new area or changes position.
    /// <para>
    /// <b>Abilities:</b> [Sixth Sense] (auto-detect traps within 10 ft)
    /// </para>
    /// </remarks>
    [Description("On character movement")]
    OnMovement = 1,

    /// <summary>
    /// Triggers on any bypass attempt.
    /// </summary>
    /// <remarks>
    /// Abilities with this trigger apply their effects before or during
    /// any bypass attempt (lockpicking, hacking, disarming, etc.).
    /// <para>
    /// <b>Abilities:</b> [Fast Pick] (reduce bypass time by 1 round)
    /// </para>
    /// </remarks>
    [Description("On bypass attempt")]
    OnBypassAttempt = 2,

    /// <summary>
    /// Triggers when attempting bypass while in danger.
    /// </summary>
    /// <remarks>
    /// Abilities with this trigger activate when the character attempts
    /// a bypass while in combat, under threat, or being observed by enemies.
    /// <para>
    /// <b>Abilities:</b> [Bypass Under Fire] (negate combat/danger penalties)
    /// </para>
    /// </remarks>
    [Description("On bypass while in danger")]
    OnBypassInDanger = 3,

    /// <summary>
    /// Triggers when interacting with [Glitched] or [Blighted] technology.
    /// </summary>
    /// <remarks>
    /// Abilities with this trigger apply when the bypass target has a
    /// corruption level of [Glitched] or higher.
    /// <para>
    /// <b>Abilities:</b> [Pattern Recognition] (reduce [Glitched] penalty by 2)
    /// </para>
    /// </remarks>
    [Description("On glitched interaction")]
    OnGlitchedInteraction = 4,

    /// <summary>
    /// Triggers on successful terminal hack.
    /// </summary>
    /// <remarks>
    /// Abilities with this trigger activate after a terminal hack succeeds,
    /// modifying the outcome before it is returned.
    /// <para>
    /// <b>Abilities:</b> [Deep Access] (upgrade to Admin-Level access)
    /// </para>
    /// </remarks>
    [Description("On terminal hack success")]
    OnTerminalHackSuccess = 5,

    /// <summary>
    /// Triggers on craft attempt.
    /// </summary>
    /// <remarks>
    /// Abilities with this trigger apply when the character attempts to
    /// craft a tool using the Improvised Tool Crafting system.
    /// <para>
    /// <b>Abilities:</b> [Master Craftsman] (unlock masterwork recipes)
    /// </para>
    /// </remarks>
    [Description("On craft attempt")]
    OnCraftAttempt = 6
}
