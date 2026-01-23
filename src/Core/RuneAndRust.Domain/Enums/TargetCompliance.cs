// ------------------------------------------------------------------------------
// <copyright file="TargetCompliance.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents the level of compliance achieved from an intimidation target.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the level of compliance achieved from an intimidation target.
/// </summary>
/// <remarks>
/// <para>
/// Compliance level is determined by the outcome of the intimidation check
/// and affects what actions the NPC will perform or information they will provide.
/// </para>
/// <para>
/// Compliance levels map to outcomes as follows:
/// </para>
/// <list type="bullet">
///   <item><description>None: Failure/Fumble - Target refuses to comply</description></item>
///   <item><description>Minimal: Marginal Success - Target provides minimal cooperation</description></item>
///   <item><description>Reluctant: Full Success - Target cooperates reluctantly</description></item>
///   <item><description>Full: Exceptional Success - Target fully complies</description></item>
///   <item><description>Complete: Critical Success - Target is completely cowed</description></item>
/// </list>
/// </remarks>
public enum TargetCompliance
{
    /// <summary>
    /// Target completely refuses to comply.
    /// </summary>
    /// <remarks>
    /// Result of failed or fumbled intimidation. Target may become hostile
    /// and is unlikely to respond to future intimidation attempts.
    /// </remarks>
    None = 0,

    /// <summary>
    /// Target provides minimal cooperation.
    /// </summary>
    /// <remarks>
    /// Result of marginal success. Target does the bare minimum required,
    /// may "forget" details or provide incomplete information.
    /// </remarks>
    Minimal = 1,

    /// <summary>
    /// Target cooperates reluctantly but adequately.
    /// </summary>
    /// <remarks>
    /// Result of full success. Target complies with the demand but
    /// maintains their dignity. Will remember the insult.
    /// </remarks>
    Reluctant = 2,

    /// <summary>
    /// Target fully complies with demands.
    /// </summary>
    /// <remarks>
    /// Result of exceptional success. Target is visibly intimidated and
    /// cooperates fully. May provide additional helpful information.
    /// </remarks>
    Full = 3,

    /// <summary>
    /// Target is completely cowed, may offer additional information.
    /// </summary>
    /// <remarks>
    /// Result of critical success. Target is thoroughly intimidated,
    /// provides everything requested and may volunteer extra information
    /// or assistance to avoid further confrontation.
    /// </remarks>
    Complete = 4
}
