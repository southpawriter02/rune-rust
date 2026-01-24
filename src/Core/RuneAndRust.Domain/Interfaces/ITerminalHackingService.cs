// ------------------------------------------------------------------------------
// <copyright file="ITerminalHackingService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for executing terminal hacking attempts as part of the
// System Bypass skill subsystem.
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for executing multi-layer terminal hacking attempts.
/// </summary>
/// <remarks>
/// <para>
/// Implements the terminal hacking subsystem of the System Bypass skill.
/// Terminal hacking in Aethelgard follows cargo cult mechanics—characters manipulate
/// incomprehensible systems through observed patterns rather than true understanding.
/// </para>
/// <para>
/// Infiltration proceeds through three layers:
/// <list type="bullet">
///   <item><description>Layer 1 (Access): Establish connection, bypass firewall</description></item>
///   <item><description>Layer 2 (Authentication): Verify identity, gain access level</description></item>
///   <item><description>Layer 3 (Navigation): Locate and access specific data</description></item>
/// </list>
/// </para>
/// <para>
/// Layer outcomes:
/// <list type="bullet">
///   <item><description>Success: Proceed to next layer</description></item>
///   <item><description>Critical Success: Proceed + gain AdminLevel access</description></item>
///   <item><description>Failure: Layer-specific consequence (lockout, alert, partial)</description></item>
///   <item><description>Fumble: [System Lockout] - Terminal permanently disabled</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ITerminalHackingService
{
    // -------------------------------------------------------------------------
    // Core Infiltration Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Begins a new terminal infiltration attempt.
    /// </summary>
    /// <param name="player">The player attempting to hack.</param>
    /// <param name="context">Context containing terminal information.</param>
    /// <returns>A new infiltration state ready for layer attempts.</returns>
    /// <remarks>
    /// <para>
    /// Creates a fresh infiltration state starting at Layer 1.
    /// The player must then use <see cref="AttemptCurrentLayer"/> to progress through layers.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player or context is null.
    /// </exception>
    TerminalInfiltrationState BeginInfiltration(Player player, TerminalContext context);

    /// <summary>
    /// Attempts the current layer of an infiltration.
    /// </summary>
    /// <param name="player">The player attempting the hack.</param>
    /// <param name="state">The current infiltration state.</param>
    /// <param name="context">The terminal context.</param>
    /// <returns>The result of the layer attempt.</returns>
    /// <remarks>
    /// <para>
    /// The attempt process:
    /// <list type="number">
    ///   <item><description>Determine DC for current layer</description></item>
    ///   <item><description>Build skill context with modifiers</description></item>
    ///   <item><description>Perform skill check against DC</description></item>
    ///   <item><description>Process outcome and update state</description></item>
    ///   <item><description>Create fumble consequence if applicable</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player, state, or context is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when infiltration is already complete.
    /// </exception>
    LayerResult AttemptCurrentLayer(
        Player player,
        TerminalInfiltrationState state,
        TerminalContext context);

    /// <summary>
    /// Attempts to cover tracks after successful infiltration.
    /// </summary>
    /// <param name="player">The player covering tracks.</param>
    /// <param name="state">The infiltration state.</param>
    /// <returns>Whether tracks were successfully covered.</returns>
    /// <remarks>
    /// <para>
    /// Uses a Stealth check against DC 14 to wipe access logs.
    /// Only available after successful infiltration (Layer 3 complete).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player or state is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when infiltration was not successful.
    /// </exception>
    bool AttemptCoverTracks(Player player, TerminalInfiltrationState state);

    // -------------------------------------------------------------------------
    // Information Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the DC for a specific layer given the context.
    /// </summary>
    /// <param name="layer">The infiltration layer.</param>
    /// <param name="context">The terminal context.</param>
    /// <returns>The effective DC for the layer.</returns>
    /// <remarks>
    /// <para>
    /// DC calculation by layer:
    /// <list type="bullet">
    ///   <item><description>Layer 1: Terminal base DC + corruption + failure modifier</description></item>
    ///   <item><description>Layer 2: Layer 1 DC + security level modifier</description></item>
    ///   <item><description>Layer 3: Data type DC (10-22)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    int GetLayerDc(InfiltrationLayer layer, TerminalContext context);

    /// <summary>
    /// Determines if a player can attempt to hack a specific terminal.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="context">The terminal context.</param>
    /// <returns>True if the attempt can be made; otherwise false.</returns>
    /// <remarks>
    /// <para>
    /// An attempt may be blocked if:
    /// <list type="bullet">
    ///   <item><description>A [System Lockout] consequence exists on the terminal</description></item>
    ///   <item><description>Player lacks required skill training</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    bool CanAttempt(Player player, TerminalContext context);

    /// <summary>
    /// Gets the reason why an attempt cannot be made.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="context">The terminal context.</param>
    /// <returns>Reason string if cannot attempt; null if can attempt.</returns>
    /// <remarks>
    /// <para>
    /// Use this method to provide feedback to the player about why their
    /// hacking attempt is blocked. Returns null if the attempt is allowed.
    /// </para>
    /// </remarks>
    string? GetAttemptBlockedReason(Player player, TerminalContext context);
}
