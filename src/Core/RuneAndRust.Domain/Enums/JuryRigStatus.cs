// ------------------------------------------------------------------------------
// <copyright file="JuryRigStatus.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The status of a jury-rigging attempt.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// The status of a jury-rigging attempt.
/// </summary>
/// <remarks>
/// <para>
/// Status tracks the overall state of the jury-rigging attempt:
/// <list type="bullet">
///   <item><description>InProgress: Attempt is ongoing, may continue</description></item>
///   <item><description>Bypassed: Successfully bypassed the mechanism</description></item>
///   <item><description>Destroyed: Mechanism destroyed via Brute Disassembly</description></item>
///   <item><description>MechanismDestroyed: Mechanism destroyed via fumble</description></item>
///   <item><description>PermanentlyLocked: Complication 1 triggered, no further attempts</description></item>
///   <item><description>Abandoned: Player chose to abandon the attempt</description></item>
/// </list>
/// </para>
/// <para>
/// Terminal states (all except InProgress) end the jury-rigging session.
/// The distinction between Destroyed and MechanismDestroyed indicates whether
/// destruction was intentional (Brute Disassembly yields salvage) or accidental (fumble does not).
/// </para>
/// </remarks>
public enum JuryRigStatus
{
    /// <summary>
    /// Attempt is ongoing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The jury-rigging session is active and the player may continue
    /// attempting to bypass the mechanism.
    /// </para>
    /// </remarks>
    [Description("Attempt in progress")]
    InProgress = 0,

    /// <summary>
    /// Mechanism was successfully bypassed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state: The bypass succeeded and the mechanism is now accessible.
    /// The mechanism remains intact and functional.
    /// </para>
    /// </remarks>
    [Description("Successfully bypassed")]
    Bypassed = 1,

    /// <summary>
    /// Mechanism was intentionally destroyed via Brute Disassembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state: The mechanism was torn apart to gain access.
    /// Yields salvageable components but mechanism cannot be reused.
    /// Distinct from MechanismDestroyed in that this was intentional.
    /// </para>
    /// </remarks>
    [Description("Destroyed via disassembly")]
    Destroyed = 2,

    /// <summary>
    /// Mechanism was accidentally destroyed by fumble.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state: A fumble during the experiment step destroyed the mechanism.
    /// No salvage is available—the mechanism is ruined beyond use.
    /// Distinct from Destroyed in that this was accidental.
    /// </para>
    /// </remarks>
    [Description("Destroyed by fumble")]
    MechanismDestroyed = 3,

    /// <summary>
    /// Mechanism is permanently locked (complication roll = 1).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state: The complication table result locked the mechanism permanently.
    /// No further jury-rigging attempts are possible.
    /// Alternative approaches: Find a key, destroy it, or find another route.
    /// </para>
    /// </remarks>
    [Description("Permanently locked")]
    PermanentlyLocked = 4,

    /// <summary>
    /// Attempt was abandoned by player.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state: The player chose to stop attempting this mechanism.
    /// The mechanism remains in its current state and may be attempted again
    /// in a new jury-rigging session.
    /// </para>
    /// </remarks>
    [Description("Abandoned")]
    Abandoned = 5
}
