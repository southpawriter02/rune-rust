// ------------------------------------------------------------------------------
// <copyright file="InterrogationResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The final outcome of a completed interrogation session including
// all information gained, costs incurred, and cumulative effects.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// The final outcome of a completed interrogation session.
/// </summary>
/// <remarks>
/// <para>
/// An InterrogationResult is generated when an interrogation session ends,
/// either through success (subject broken), abandonment, or failure (subject
/// successfully resisted). It provides a complete summary of the session.
/// </para>
/// <para>
/// The result includes:
/// <list type="bullet">
///   <item><description>Overall outcome and final status</description></item>
///   <item><description>All information extracted (if any)</description></item>
///   <item><description>Information reliability assessment</description></item>
///   <item><description>Cumulative costs (gold, reputation, disposition)</description></item>
///   <item><description>Complete round history</description></item>
///   <item><description>Narrative summary for display</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record InterrogationResult
{
    /// <summary>
    /// Gets the overall outcome of the interrogation.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>Success: Subject was broken, information obtained</description></item>
    ///   <item><description>Failure: Subject resisted or session abandoned</description></item>
    /// </list>
    /// </remarks>
    public required SkillOutcome Outcome { get; init; }

    /// <summary>
    /// Gets the final status of the interrogation.
    /// </summary>
    public required InterrogationStatus FinalStatus { get; init; }

    /// <summary>
    /// Gets the number of rounds conducted.
    /// </summary>
    public required int RoundsUsed { get; init; }

    /// <summary>
    /// Gets the total time elapsed in minutes.
    /// </summary>
    public required int TotalTimeMinutes { get; init; }

    /// <summary>
    /// Gets the most frequently used method (primary method).
    /// </summary>
    /// <remarks>
    /// The primary method determines the base reliability of extracted information.
    /// </remarks>
    public required InterrogationMethod PrimaryMethod { get; init; }

    /// <summary>
    /// Gets the reliability percentage of extracted information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Based on the primary method used, but capped at 60% if Torture was
    /// used at any point during the session.
    /// </para>
    /// </remarks>
    public required int InformationReliability { get; init; }

    /// <summary>
    /// Gets all information extracted from the subject.
    /// </summary>
    /// <remarks>
    /// Empty if the interrogation failed or was abandoned before breaking the subject.
    /// </remarks>
    public required IReadOnlyList<InformationGained> InformationGained { get; init; }

    /// <summary>
    /// Gets the total disposition change with the subject.
    /// </summary>
    /// <remarks>
    /// Cumulative disposition change from all rounds. Negative values indicate
    /// relationship damage that will affect future interactions.
    /// </remarks>
    public required int TotalDispositionChange { get; init; }

    /// <summary>
    /// Gets the total reputation cost incurred.
    /// </summary>
    /// <remarks>
    /// Primarily from Torture usage (-30 per session, plus additional -20 on fumble).
    /// </remarks>
    public required int TotalReputationCost { get; init; }

    /// <summary>
    /// Gets the total resource cost (gold) spent.
    /// </summary>
    /// <remarks>
    /// From Bribery method usage. Total of all bribes offered during the session.
    /// </remarks>
    public required int TotalResourceCost { get; init; }

    /// <summary>
    /// Gets a value indicating whether Torture was used at any point.
    /// </summary>
    /// <remarks>
    /// If true, information reliability is capped at 60% regardless of
    /// primary method used.
    /// </remarks>
    public required bool TortureWasUsed { get; init; }

    /// <summary>
    /// Gets a value indicating whether the subject has been traumatized.
    /// </summary>
    /// <remarks>
    /// True if Torture was used. The subject gains the permanent [Traumatized]
    /// condition, affecting all future interactions.
    /// </remarks>
    public required bool SubjectTraumatized { get; init; }

    /// <summary>
    /// Gets the complete history of all rounds.
    /// </summary>
    public required IReadOnlyList<InterrogationRound> History { get; init; }

    /// <summary>
    /// Gets the narrative summary of the interrogation.
    /// </summary>
    /// <remarks>
    /// A human-readable summary suitable for display to the player,
    /// describing how the interrogation concluded.
    /// </remarks>
    public required string NarrativeSummary { get; init; }

    /// <summary>
    /// Gets a value indicating whether the interrogation succeeded.
    /// </summary>
    public bool IsSuccess => FinalStatus.IsSuccess();

    /// <summary>
    /// Gets a value indicating whether the interrogation failed.
    /// </summary>
    public bool IsFailure => FinalStatus.IsFailure();

    /// <summary>
    /// Gets a value indicating whether the subject was broken.
    /// </summary>
    public bool SubjectBroken => FinalStatus == InterrogationStatus.SubjectBroken;

    /// <summary>
    /// Gets a value indicating whether any information was obtained.
    /// </summary>
    public bool HasInformation => InformationGained.Count > 0;

    /// <summary>
    /// Gets a value indicating whether there were significant costs.
    /// </summary>
    /// <remarks>
    /// True if reputation was lost, significant disposition damage occurred,
    /// or resources were spent.
    /// </remarks>
    public bool HasSignificantCosts =>
        TotalReputationCost < 0 ||
        TotalDispositionChange < -10 ||
        TotalResourceCost > 100;

    /// <summary>
    /// Gets a formatted display string for the result.
    /// </summary>
    /// <returns>A multi-line summary suitable for UI display.</returns>
    public string GetDisplaySummary()
    {
        var lines = new List<string>
        {
            $"=== INTERROGATION {(IsSuccess ? "SUCCESSFUL" : "FAILED")} ===",
            string.Empty,
            NarrativeSummary,
            string.Empty,
            $"Status: {FinalStatus.GetDisplayName()}",
            $"Rounds: {RoundsUsed} ({TotalTimeMinutes / 60}h {TotalTimeMinutes % 60}m)",
            $"Primary Method: {PrimaryMethod.GetDisplayName()}"
        };

        if (IsSuccess)
        {
            lines.Add($"Information Reliability: {InformationReliability}%");
            if (TortureWasUsed)
            {
                lines.Add("⚠️ WARNING: Torture usage capped reliability at 60%");
            }
        }

        lines.Add(string.Empty);
        lines.Add("--- COSTS ---");

        if (TotalDispositionChange != 0)
        {
            lines.Add($"Disposition: {TotalDispositionChange:+0;-0}");
        }

        if (TotalReputationCost != 0)
        {
            lines.Add($"Reputation: {TotalReputationCost:+0;-0}");
        }

        if (TotalResourceCost > 0)
        {
            lines.Add($"Gold Spent: {TotalResourceCost}");
        }

        if (SubjectTraumatized)
        {
            lines.Add("⚠️ Subject is now [Traumatized]");
        }

        if (HasInformation)
        {
            lines.Add(string.Empty);
            lines.Add("--- INFORMATION GAINED ---");
            foreach (var info in InformationGained)
            {
                lines.Add(info.GetSummary());
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a compact one-line summary of the result.
    /// </summary>
    /// <returns>A brief summary string.</returns>
    public string ToShortDisplay()
    {
        var statusIcon = IsSuccess ? "✓" : "✗";
        var infoCount = InformationGained.Count;
        var infoText = infoCount > 0 ? $", {infoCount} info" : string.Empty;

        return $"{statusIcon} {FinalStatus.GetDisplayName()} ({RoundsUsed} rounds{infoText})";
    }

    /// <summary>
    /// Creates a successful interrogation result.
    /// </summary>
    /// <param name="state">The final interrogation state.</param>
    /// <param name="information">Information extracted.</param>
    /// <param name="narrativeSummary">Narrative summary text.</param>
    /// <returns>A successful InterrogationResult.</returns>
    public static InterrogationResult Success(
        Entities.InterrogationState state,
        IReadOnlyList<InformationGained> information,
        string narrativeSummary)
    {
        var totalTime = state.History.Sum(r => r.TimeElapsedMinutes);

        return new InterrogationResult
        {
            Outcome = SkillOutcome.FullSuccess,
            FinalStatus = InterrogationStatus.SubjectBroken,
            RoundsUsed = state.RoundNumber,
            TotalTimeMinutes = totalTime,
            PrimaryMethod = state.GetPrimaryMethod(),
            InformationReliability = state.CalculateReliability(),
            InformationGained = information,
            TotalDispositionChange = state.TotalDispositionChange,
            TotalReputationCost = state.TotalReputationCost,
            TotalResourceCost = state.TotalResourceCost,
            TortureWasUsed = state.TortureUsed,
            SubjectTraumatized = state.TortureUsed,
            History = state.History,
            NarrativeSummary = narrativeSummary
        };
    }

    /// <summary>
    /// Creates a failed interrogation result.
    /// </summary>
    /// <param name="state">The final interrogation state.</param>
    /// <param name="finalStatus">The failure status (Abandoned or SubjectResisting).</param>
    /// <param name="narrativeSummary">Narrative summary text.</param>
    /// <returns>A failed InterrogationResult.</returns>
    public static InterrogationResult Failure(
        Entities.InterrogationState state,
        InterrogationStatus finalStatus,
        string narrativeSummary)
    {
        var totalTime = state.History.Sum(r => r.TimeElapsedMinutes);

        return new InterrogationResult
        {
            Outcome = SkillOutcome.Failure,
            FinalStatus = finalStatus,
            RoundsUsed = state.RoundNumber,
            TotalTimeMinutes = totalTime,
            PrimaryMethod = state.GetPrimaryMethod(),
            InformationReliability = 0,
            InformationGained = Array.Empty<InformationGained>(),
            TotalDispositionChange = state.TotalDispositionChange,
            TotalReputationCost = state.TotalReputationCost,
            TotalResourceCost = state.TotalResourceCost,
            TortureWasUsed = state.TortureUsed,
            SubjectTraumatized = state.TortureUsed,
            History = state.History,
            NarrativeSummary = narrativeSummary
        };
    }
}
