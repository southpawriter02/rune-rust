// ------------------------------------------------------------------------------
// <copyright file="NegotiationStatusExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the NegotiationStatus enum providing display names,
// descriptions, and state queries.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="NegotiationStatus"/> enum providing
/// display names, narrative descriptions, and state queries.
/// </summary>
public static class NegotiationStatusExtensions
{
    /// <summary>
    /// Gets whether this is a terminal state (negotiation is over).
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>True if the negotiation has concluded (either successfully or collapsed).</returns>
    /// <remarks>
    /// Terminal states are DealReached and Collapsed. No further rounds
    /// can be played once a terminal state is reached.
    /// </remarks>
    public static bool IsTerminal(this NegotiationStatus status)
    {
        return status == NegotiationStatus.DealReached ||
               status == NegotiationStatus.Collapsed;
    }

    /// <summary>
    /// Gets whether this status represents a successful negotiation.
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>True if the negotiation succeeded with a deal reached.</returns>
    public static bool IsSuccess(this NegotiationStatus status)
    {
        return status == NegotiationStatus.DealReached;
    }

    /// <summary>
    /// Gets whether this status represents a failed negotiation.
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>True if the negotiation collapsed without agreement.</returns>
    public static bool IsFailure(this NegotiationStatus status)
    {
        return status == NegotiationStatus.Collapsed;
    }

    /// <summary>
    /// Gets whether this status indicates the negotiation is at risk.
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>True if the negotiation is in CrisisManagement phase.</returns>
    /// <remarks>
    /// During CrisisManagement, a fumble will cause immediate collapse.
    /// Players should consider concessions to stabilize the negotiation.
    /// </remarks>
    public static bool IsAtRisk(this NegotiationStatus status)
    {
        return status == NegotiationStatus.CrisisManagement;
    }

    /// <summary>
    /// Gets whether this status indicates the negotiation is close to success.
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>True if the negotiation is in Finalization phase.</returns>
    public static bool IsNearSuccess(this NegotiationStatus status)
    {
        return status == NegotiationStatus.Finalization;
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>Human-readable status name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetDisplayName(this NegotiationStatus status)
    {
        return status switch
        {
            NegotiationStatus.Opening => "Opening",
            NegotiationStatus.Bargaining => "Bargaining",
            NegotiationStatus.CrisisManagement => "Crisis Management",
            NegotiationStatus.Finalization => "Finalization",
            NegotiationStatus.DealReached => "Deal Reached",
            NegotiationStatus.Collapsed => "Collapsed",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown negotiation status")
        };
    }

    /// <summary>
    /// Gets a narrative description of the current phase.
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>Descriptive text for the current phase.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetDescription(this NegotiationStatus status)
    {
        return status switch
        {
            NegotiationStatus.Opening =>
                "Both parties are sizing each other up, establishing initial positions.",
            NegotiationStatus.Bargaining =>
                "Active negotiation in progress. Each side pushes for advantage.",
            NegotiationStatus.CrisisManagement =>
                "The negotiation is on the verge of collapse. Careful moves required.",
            NegotiationStatus.Finalization =>
                "Positions are close. Final terms are being hammered out.",
            NegotiationStatus.DealReached =>
                "A deal has been struck. Both parties have agreed to terms.",
            NegotiationStatus.Collapsed =>
                "The negotiation has failed completely. No deal is possible.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown negotiation status")
        };
    }

    /// <summary>
    /// Gets a short status indicator for compact display.
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>Short status indicator string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetShortIndicator(this NegotiationStatus status)
    {
        return status switch
        {
            NegotiationStatus.Opening => "[OPENING]",
            NegotiationStatus.Bargaining => "[BARGAINING]",
            NegotiationStatus.CrisisManagement => "[CRISIS!]",
            NegotiationStatus.Finalization => "[FINALIZING]",
            NegotiationStatus.DealReached => "[DEAL!]",
            NegotiationStatus.Collapsed => "[COLLAPSED]",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown negotiation status")
        };
    }

    /// <summary>
    /// Gets a tactical hint for the player based on current status.
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>Tactical advice for the current phase.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetTacticalHint(this NegotiationStatus status)
    {
        return status switch
        {
            NegotiationStatus.Opening =>
                "Assess the situation. A concession now can set up later success.",
            NegotiationStatus.Bargaining =>
                "Choose tactics based on the gap and your strengths. Concessions build momentum.",
            NegotiationStatus.CrisisManagement =>
                "Danger! Consider conceding to stabilize. A fumble here means collapse.",
            NegotiationStatus.Finalization =>
                "Almost there! One successful check should seal the deal.",
            NegotiationStatus.DealReached =>
                "Negotiation complete. The deal has been finalized.",
            NegotiationStatus.Collapsed =>
                "Negotiation failed. Consider your next approach carefully.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown negotiation status")
        };
    }

    /// <summary>
    /// Gets the color code for UI display based on status.
    /// </summary>
    /// <param name="status">The negotiation status.</param>
    /// <returns>Color name string for UI styling.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown status is provided.
    /// </exception>
    public static string GetStatusColor(this NegotiationStatus status)
    {
        return status switch
        {
            NegotiationStatus.Opening => "blue",
            NegotiationStatus.Bargaining => "yellow",
            NegotiationStatus.CrisisManagement => "red",
            NegotiationStatus.Finalization => "cyan",
            NegotiationStatus.DealReached => "green",
            NegotiationStatus.Collapsed => "darkred",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown negotiation status")
        };
    }
}
