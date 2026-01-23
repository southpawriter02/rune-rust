// ------------------------------------------------------------------------------
// <copyright file="InfluenceStatusExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the InfluenceStatus enum providing state checks,
// display names, and descriptions for extended influence tracking.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="InfluenceStatus"/> enum providing
/// state checks, display names, and descriptions.
/// </summary>
public static class InfluenceStatusExtensions
{
    /// <summary>
    /// Gets whether this status represents a terminal state (influence cannot continue normally).
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>True if the influence has permanently ended; false otherwise.</returns>
    /// <remarks>
    /// <para>
    /// Terminal states are:
    /// <list type="bullet">
    ///   <item><description>Successful: Conviction threshold reached, belief changed</description></item>
    ///   <item><description>Failed: Maximum resistance reached or quest failure</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: Stalled is NOT terminal - it can resume with external events.
    /// </para>
    /// </remarks>
    public static bool IsTerminal(this InfluenceStatus status)
    {
        return status == InfluenceStatus.Successful ||
               status == InfluenceStatus.Failed;
    }

    /// <summary>
    /// Gets whether this status represents successful influence.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>True if the belief has been successfully changed; otherwise, false.</returns>
    public static bool IsSuccess(this InfluenceStatus status)
    {
        return status == InfluenceStatus.Successful;
    }

    /// <summary>
    /// Gets whether this status represents failed influence.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>True if the influence has permanently failed; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// The Failed status indicates the NPC will not change their mind through
    /// normal influence mechanics. This can occur when:
    /// <list type="bullet">
    ///   <item><description>Maximum resistance modifier (6) reached with insufficient pool</description></item>
    ///   <item><description>A related quest fails making belief change impossible</description></item>
    ///   <item><description>Critical fumble creates unrecoverable situation</description></item>
    ///   <item><description>NPC becomes permanently unavailable</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: Active and Stalled are not considered failures - they may still succeed.
    /// </para>
    /// </remarks>
    public static bool IsFailure(this InfluenceStatus status)
    {
        return status == InfluenceStatus.Failed;
    }

    /// <summary>
    /// Gets whether more influence attempts can be made in this status.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>True if influence attempts can continue immediately; otherwise, false.</returns>
    /// <remarks>
    /// Only the Active status allows immediate influence attempts.
    /// Stalled requires external events before resuming.
    /// Terminal states (Successful, Failed) cannot continue.
    /// </remarks>
    public static bool CanContinue(this InfluenceStatus status)
    {
        return status == InfluenceStatus.Active;
    }

    /// <summary>
    /// Gets whether this influence can be resumed after being stalled.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>True if the status is Stalled and can potentially resume.</returns>
    /// <remarks>
    /// <para>
    /// Stalled influence awaits external conditions before resuming:
    /// <list type="bullet">
    ///   <item><description>Resistance ≥ 4: Wait 24+ hours game time</description></item>
    ///   <item><description>Fumble consequence: Complete a related quest</description></item>
    ///   <item><description>Contradicted by evidence: Find counter-evidence</description></item>
    ///   <item><description>NPC emotional state: Wait for mood change</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When resumed, resistance is typically reduced by 2 points.
    /// </para>
    /// </remarks>
    public static bool CanResume(this InfluenceStatus status)
    {
        return status == InfluenceStatus.Stalled;
    }

    /// <summary>
    /// Gets whether this influence tracking is still relevant (not terminally failed).
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>True if the influence may still succeed eventually.</returns>
    /// <remarks>
    /// Returns true for Active, Successful, and Stalled statuses.
    /// Returns false only for Failed status.
    /// </remarks>
    public static bool IsRelevant(this InfluenceStatus status)
    {
        return status != InfluenceStatus.Failed;
    }

    /// <summary>
    /// Gets whether this influence is in a paused/waiting state.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>True if waiting for external events; otherwise, false.</returns>
    public static bool IsPaused(this InfluenceStatus status)
    {
        return status == InfluenceStatus.Stalled;
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>Human-readable status name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetDisplayName(this InfluenceStatus status)
    {
        return status switch
        {
            InfluenceStatus.Active => "Active",
            InfluenceStatus.Successful => "Successful",
            InfluenceStatus.Failed => "Failed",
            InfluenceStatus.Stalled => "Stalled",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown influence status")
        };
    }

    /// <summary>
    /// Gets a narrative description of the current status.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>Descriptive text suitable for display to the player.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetDescription(this InfluenceStatus status)
    {
        return status switch
        {
            InfluenceStatus.Active =>
                "The influence attempt is ongoing. Continue making progress toward changing their belief.",
            InfluenceStatus.Successful =>
                "The NPC's belief has been changed. New dialogue and options may be available.",
            InfluenceStatus.Failed =>
                "The NPC is completely resistant. Normal influence can no longer change this belief.",
            InfluenceStatus.Stalled =>
                "Progress is blocked. An external event or condition must be resolved to continue.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown influence status")
        };
    }

    /// <summary>
    /// Gets the icon or indicator character for this status.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>A unicode character representing the status.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetStatusIndicator(this InfluenceStatus status)
    {
        return status switch
        {
            InfluenceStatus.Active => "◐",     // Half-filled circle (in progress)
            InfluenceStatus.Successful => "✓", // Checkmark (success)
            InfluenceStatus.Failed => "✗",     // X mark (failure)
            InfluenceStatus.Stalled => "⏸",   // Pause symbol (waiting)
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown influence status")
        };
    }

    /// <summary>
    /// Gets a color hint for UI presentation.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>A CSS-compatible color name or hex code.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    /// <remarks>
    /// Color meanings:
    /// <list type="bullet">
    ///   <item><description>Blue (#2196F3): Active - work in progress</description></item>
    ///   <item><description>Green (#4CAF50): Successful - goal achieved</description></item>
    ///   <item><description>Red (#F44336): Failed - permanently blocked</description></item>
    ///   <item><description>Orange (#FF9800): Stalled - temporarily blocked</description></item>
    /// </list>
    /// </remarks>
    public static string GetColorHint(this InfluenceStatus status)
    {
        return status switch
        {
            InfluenceStatus.Active => "#2196F3",     // Blue
            InfluenceStatus.Successful => "#4CAF50", // Green
            InfluenceStatus.Failed => "#F44336",     // Red
            InfluenceStatus.Stalled => "#FF9800",    // Orange
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown influence status")
        };
    }

    /// <summary>
    /// Gets a narrative hint for what the player should do next.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>Guidance text for the player.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetNextStepHint(this InfluenceStatus status)
    {
        return status switch
        {
            InfluenceStatus.Active =>
                "Continue dialogue to make progress. Successful rhetoric checks add to your influence pool.",
            InfluenceStatus.Successful =>
                "Check for new dialogue options or quest updates reflecting the changed belief.",
            InfluenceStatus.Failed =>
                "Normal influence has failed. Extraordinary circumstances may still change their mind.",
            InfluenceStatus.Stalled =>
                "Wait for the required condition to be met, then attempt to resume the conversation.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown influence status")
        };
    }

    /// <summary>
    /// Gets whether this status allows viewing progress details.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>True if progress details should be displayed.</returns>
    /// <remarks>
    /// Active and Stalled show progress (how close to threshold).
    /// Successful shows final result.
    /// Failed may or may not show progress depending on UI preference.
    /// </remarks>
    public static bool ShowsProgress(this InfluenceStatus status)
    {
        return status == InfluenceStatus.Active ||
               status == InfluenceStatus.Stalled ||
               status == InfluenceStatus.Successful;
    }

    /// <summary>
    /// Gets a log-friendly string representation of the status.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>A concise string suitable for logging.</returns>
    public static string ToLogString(this InfluenceStatus status)
    {
        return status switch
        {
            InfluenceStatus.Active => "ACTIVE",
            InfluenceStatus.Successful => "SUCCESS",
            InfluenceStatus.Failed => "FAILED",
            InfluenceStatus.Stalled => "STALLED",
            _ => $"UNKNOWN({(int)status})"
        };
    }

    /// <summary>
    /// Gets the sort order for displaying statuses (active first, then by importance).
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <returns>An integer for sorting (lower values appear first).</returns>
    /// <remarks>
    /// Sort order: Active (0), Stalled (1), Successful (2), Failed (3)
    /// This places actionable items first, then completed ones.
    /// </remarks>
    public static int GetSortOrder(this InfluenceStatus status)
    {
        return status switch
        {
            InfluenceStatus.Active => 0,
            InfluenceStatus.Stalled => 1,
            InfluenceStatus.Successful => 2,
            InfluenceStatus.Failed => 3,
            _ => 99
        };
    }
}
