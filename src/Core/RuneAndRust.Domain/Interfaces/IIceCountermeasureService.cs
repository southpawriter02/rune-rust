// ------------------------------------------------------------------------------
// <copyright file="IIceCountermeasureService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for handling ICE (Intrusion Countermeasures Electronics)
// encounters during terminal hacking attempts.
// Part of v0.15.4c ICE Countermeasures implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for handling ICE (Intrusion Countermeasures Electronics) encounters
/// during terminal hacking attempts.
/// </summary>
/// <remarks>
/// <para>
/// ICE represents ancient automated defense programs that protect secured terminals
/// in Aethelgard. Characters encounter ICE when authentication fails (Layer 2),
/// triggering hostile digital entities they must evade, defeat, or endure.
/// </para>
/// <para>
/// ICE types and their resolution methods:
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Passive (Trace):</b> Contested System Bypass vs. ICE Rating.
///       Failure reveals the hacker's location.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Active (Attack):</b> Contested System Bypass vs. ICE Rating.
///       Failure forces disconnect with temporary lockout.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Lethal (Neural):</b> WILL save DC 16.
///       Failure deals psychic damage and stress with permanent lockout.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// <b>ICE by Terminal Type:</b>
/// <list type="bullet">
///   <item><description>CivilianDataPort: No ICE</description></item>
///   <item><description>CorporateMainframe: Passive (Rating 12)</description></item>
///   <item><description>SecurityHub: Active (Rating 16)</description></item>
///   <item><description>MilitaryServer: Active + Lethal (Rating 20)</description></item>
///   <item><description>JotunArchive: Lethal (Rating 24)</description></item>
///   <item><description>GlitchedManifold: Unpredictable</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IIceCountermeasureService
{
    // -------------------------------------------------------------------------
    // ICE Information Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines what ICE types, if any, protect a terminal based on its type.
    /// </summary>
    /// <param name="terminalType">The type of terminal being accessed.</param>
    /// <returns>List of ICE types present on this terminal (empty if none).</returns>
    /// <remarks>
    /// <para>
    /// Some terminals have multiple ICE types (e.g., MilitaryServer has both
    /// Active and Lethal ICE). All active ICE types must be resolved when triggered.
    /// </para>
    /// <para>
    /// Returns an empty list for terminals without ICE protection (CivilianDataPort).
    /// </para>
    /// </remarks>
    IReadOnlyList<IceType> GetIceForTerminal(TerminalType terminalType);

    /// <summary>
    /// Gets the ICE rating (difficulty) for a terminal type.
    /// </summary>
    /// <param name="terminalType">The type of terminal.</param>
    /// <returns>The ICE rating for this terminal's ICE, or 0 if no ICE present.</returns>
    /// <remarks>
    /// <para>
    /// ICE ratings by terminal:
    /// <list type="bullet">
    ///   <item><description>CivilianDataPort: 0 (no ICE)</description></item>
    ///   <item><description>CorporateMainframe: 12</description></item>
    ///   <item><description>SecurityHub: 16</description></item>
    ///   <item><description>MilitaryServer: 20</description></item>
    ///   <item><description>JotunArchive: 24</description></item>
    ///   <item><description>GlitchedManifold: 0 (variable, determined at runtime)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    int GetIceRating(TerminalType terminalType);

    /// <summary>
    /// Determines if a terminal has ICE protection.
    /// </summary>
    /// <param name="terminalType">The type of terminal.</param>
    /// <returns>True if the terminal has at least one ICE type; otherwise false.</returns>
    bool HasIce(TerminalType terminalType);

    // -------------------------------------------------------------------------
    // ICE Encounter Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Triggers an ICE encounter, creating the encounter record.
    /// </summary>
    /// <param name="iceType">The type of ICE activating.</param>
    /// <param name="iceRating">The ICE's difficulty rating.</param>
    /// <returns>A new triggered IceEncounter with pending resolution.</returns>
    /// <remarks>
    /// <para>
    /// This method creates the encounter but does not resolve it. Call
    /// <see cref="ResolveIce"/> to determine the outcome.
    /// </para>
    /// <para>
    /// ICE typically triggers on Layer 2 authentication failure. The encounter
    /// must be resolved before the hacker can proceed.
    /// </para>
    /// </remarks>
    IceEncounter TriggerIce(IceType iceType, int iceRating);

    /// <summary>
    /// Resolves an ICE encounter using the appropriate resolution method.
    /// </summary>
    /// <param name="encounter">The ICE encounter to resolve.</param>
    /// <param name="player">The player character facing the ICE.</param>
    /// <returns>The complete resolution result with all consequences.</returns>
    /// <remarks>
    /// <para>
    /// Resolution methods by ICE type:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <b>Passive/Active:</b> Contested System Bypass check vs. ICE DC
    ///       (Rating / 6, rounded up, minimum 1).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Lethal:</b> WILL save DC 16 (fixed, not based on rating).
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// The returned result includes all consequences (damage, stress, lockout,
    /// alert changes) that must be applied via <see cref="ApplyIceConsequences"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when encounter or player is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the encounter has already been resolved (not Pending).
    /// </exception>
    IceResolutionResult ResolveIce(IceEncounter encounter, Player player);

    // -------------------------------------------------------------------------
    // Consequence Application Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies the consequences of an ICE resolution to the character and terminal state.
    /// </summary>
    /// <param name="result">The resolution result to apply.</param>
    /// <param name="player">The player character to apply damage/stress to.</param>
    /// <param name="infiltrationState">The terminal hacking state to update.</param>
    /// <remarks>
    /// <para>
    /// This method applies all consequences from the resolution:
    /// <list type="bullet">
    ///   <item><description>Psychic damage (Lethal ICE failure)</description></item>
    ///   <item><description>Stress (Lethal ICE)</description></item>
    ///   <item><description>Forced disconnect with lockout</description></item>
    ///   <item><description>Alert level changes</description></item>
    ///   <item><description>Location reveal flag</description></item>
    ///   <item><description>ICE encounter recording</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Call this method after <see cref="ResolveIce"/> to finalize the encounter.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when result, player, or infiltrationState is null.
    /// </exception>
    void ApplyIceConsequences(
        IceResolutionResult result,
        Player player,
        TerminalInfiltrationState infiltrationState);
}
