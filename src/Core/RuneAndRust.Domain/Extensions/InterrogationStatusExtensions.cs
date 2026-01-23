// ------------------------------------------------------------------------------
// <copyright file="InterrogationStatusExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the InterrogationStatus enum providing state checks,
// display names, and descriptions.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="InterrogationStatus"/> enum providing
/// state checks, display names, and descriptions.
/// </summary>
public static class InterrogationStatusExtensions
{
    /// <summary>
    /// Gets whether this status represents a terminal state (interrogation cannot continue).
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <returns>True if the interrogation has ended; false if it can continue.</returns>
    /// <remarks>
    /// Terminal states are:
    /// <list type="bullet">
    ///   <item><description>SubjectBroken: Success - subject is ready to talk</description></item>
    ///   <item><description>Abandoned: Failure - interrogator gave up</description></item>
    ///   <item><description>SubjectResisting: Failure - max rounds reached</description></item>
    /// </list>
    /// </remarks>
    public static bool IsTerminal(this InterrogationStatus status)
    {
        return status == InterrogationStatus.SubjectBroken ||
               status == InterrogationStatus.Abandoned ||
               status == InterrogationStatus.SubjectResisting;
    }

    /// <summary>
    /// Gets whether this status represents a successful interrogation.
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <returns>True if the subject has been broken; otherwise, false.</returns>
    public static bool IsSuccess(this InterrogationStatus status)
    {
        return status == InterrogationStatus.SubjectBroken;
    }

    /// <summary>
    /// Gets whether this status represents a failed interrogation.
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <returns>True if the interrogation failed; otherwise, false.</returns>
    /// <remarks>
    /// Failure states include:
    /// <list type="bullet">
    ///   <item><description>Abandoned: Interrogator gave up</description></item>
    ///   <item><description>SubjectResisting: Subject successfully resisted</description></item>
    /// </list>
    /// Note: NotStarted and InProgress are not considered failures.
    /// </remarks>
    public static bool IsFailure(this InterrogationStatus status)
    {
        return status == InterrogationStatus.Abandoned ||
               status == InterrogationStatus.SubjectResisting;
    }

    /// <summary>
    /// Gets whether more rounds can be conducted in this status.
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <returns>True if the interrogation can continue; otherwise, false.</returns>
    /// <remarks>
    /// Only NotStarted and InProgress statuses allow further rounds.
    /// All terminal states prevent additional rounds.
    /// </remarks>
    public static bool CanContinue(this InterrogationStatus status)
    {
        return status == InterrogationStatus.NotStarted ||
               status == InterrogationStatus.InProgress;
    }

    /// <summary>
    /// Gets whether information can be extracted in this status.
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <returns>True if information extraction is possible.</returns>
    /// <remarks>
    /// Information can only be extracted when the subject's will has been broken.
    /// Attempts to extract information in other states will fail.
    /// </remarks>
    public static bool CanExtractInformation(this InterrogationStatus status)
    {
        return status == InterrogationStatus.SubjectBroken;
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <returns>Human-readable status name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetDisplayName(this InterrogationStatus status)
    {
        return status switch
        {
            InterrogationStatus.NotStarted => "Not Started",
            InterrogationStatus.InProgress => "In Progress",
            InterrogationStatus.SubjectBroken => "Subject Broken",
            InterrogationStatus.Abandoned => "Abandoned",
            InterrogationStatus.SubjectResisting => "Subject Resisting",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown interrogation status")
        };
    }

    /// <summary>
    /// Gets a narrative description of the current status.
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <returns>Descriptive text suitable for display to the player.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetDescription(this InterrogationStatus status)
    {
        return status switch
        {
            InterrogationStatus.NotStarted =>
                "The interrogation has not yet begun.",
            InterrogationStatus.InProgress =>
                "The interrogation is underway. The subject is still resisting.",
            InterrogationStatus.SubjectBroken =>
                "The subject's will has been broken. They are ready to talk.",
            InterrogationStatus.Abandoned =>
                "The interrogation was abandoned before completion.",
            InterrogationStatus.SubjectResisting =>
                "The subject successfully resisted all attempts to extract information.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown interrogation status")
        };
    }

    /// <summary>
    /// Gets the icon or indicator character for this status.
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <returns>A unicode character representing the status.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetStatusIndicator(this InterrogationStatus status)
    {
        return status switch
        {
            InterrogationStatus.NotStarted => "○",   // Empty circle
            InterrogationStatus.InProgress => "◐",   // Half-filled circle
            InterrogationStatus.SubjectBroken => "✓", // Checkmark
            InterrogationStatus.Abandoned => "✗",    // X mark
            InterrogationStatus.SubjectResisting => "⚡", // Resistance symbol
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown interrogation status")
        };
    }

    /// <summary>
    /// Gets a color hint for UI presentation.
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <returns>A suggested color name for the status.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    /// <remarks>
    /// Color names are generic (not hex codes) to allow UI flexibility:
    /// <list type="bullet">
    ///   <item><description>Gray: Neutral/inactive states</description></item>
    ///   <item><description>Yellow: Active/in-progress states</description></item>
    ///   <item><description>Green: Success states</description></item>
    ///   <item><description>Red: Failure states</description></item>
    ///   <item><description>Orange: Warning/resistance states</description></item>
    /// </list>
    /// </remarks>
    public static string GetColorHint(this InterrogationStatus status)
    {
        return status switch
        {
            InterrogationStatus.NotStarted => "Gray",
            InterrogationStatus.InProgress => "Yellow",
            InterrogationStatus.SubjectBroken => "Green",
            InterrogationStatus.Abandoned => "Red",
            InterrogationStatus.SubjectResisting => "Orange",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown interrogation status")
        };
    }
}
