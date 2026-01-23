// ------------------------------------------------------------------------------
// <copyright file="InterrogationStatus.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the possible states of an interrogation session.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the current state of an interrogation session.
/// </summary>
/// <remarks>
/// <para>
/// An interrogation progresses through states as rounds are conducted.
/// The session starts as NotStarted, transitions to InProgress once
/// the first round begins, and ends in one of three terminal states.
/// </para>
/// <para>
/// State transitions:
/// <list type="bullet">
///   <item><description>NotStarted → InProgress: First round conducted</description></item>
///   <item><description>InProgress → SubjectBroken: Resistance reaches 0</description></item>
///   <item><description>InProgress → Abandoned: Interrogator abandons session</description></item>
///   <item><description>InProgress → SubjectResisting: Max rounds reached without breaking</description></item>
/// </list>
/// </para>
/// </remarks>
public enum InterrogationStatus
{
    /// <summary>
    /// The interrogation has not yet begun.
    /// </summary>
    /// <remarks>
    /// Initial state when an InterrogationState is created. The interrogator
    /// has the subject available but has not yet conducted any rounds.
    /// </remarks>
    NotStarted = 0,

    /// <summary>
    /// The interrogation is actively in progress.
    /// </summary>
    /// <remarks>
    /// The interrogator has conducted at least one round. The subject still
    /// has resistance remaining and the session has not been abandoned or
    /// reached its maximum rounds.
    /// </remarks>
    InProgress = 1,

    /// <summary>
    /// The subject's will has been broken - they are ready to talk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal success state. The subject's resistance has been reduced to 0.
    /// Information can now be extracted with reliability based on the primary
    /// method used during the interrogation.
    /// </para>
    /// <para>
    /// The information reliability is determined by:
    /// <list type="bullet">
    ///   <item><description>Primary method's base reliability</description></item>
    ///   <item><description>Whether Torture was used at any point (caps at 60%)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    SubjectBroken = 2,

    /// <summary>
    /// The interrogation was abandoned before completion.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal failure state. The interrogator chose to stop the session
    /// before breaking the subject's resistance. Any partial information
    /// gained may still be available but with reduced reliability.
    /// </para>
    /// <para>
    /// Reasons for abandonment might include:
    /// <list type="bullet">
    ///   <item><description>Time pressure requiring the interrogator elsewhere</description></item>
    ///   <item><description>Moral objections to continuing</description></item>
    ///   <item><description>Resource depletion (no gold for Bribery)</description></item>
    ///   <item><description>Risk assessment (avoiding Torture consequences)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Abandoned = 3,

    /// <summary>
    /// The subject successfully resisted all attempts to extract information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal failure state. The interrogation reached its maximum allowed
    /// rounds without reducing the subject's resistance to 0. This typically
    /// indicates an exceptionally resilient subject or ineffective methods.
    /// </para>
    /// <para>
    /// This state can occur when:
    /// <list type="bullet">
    ///   <item><description>Maximum session rounds reached (configurable limit)</description></item>
    ///   <item><description>Subject has immunity to certain methods</description></item>
    ///   <item><description>Repeated fumbles strengthened subject's resolve</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    SubjectResisting = 4
}
