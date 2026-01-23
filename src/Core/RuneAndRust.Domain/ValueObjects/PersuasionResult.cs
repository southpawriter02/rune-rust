// ------------------------------------------------------------------------------
// <copyright file="PersuasionResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents the outcome of a persuasion attempt.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Represents the outcome of a persuasion attempt.
/// </summary>
/// <remarks>
/// <para>
/// Persuasion results include whether the request was granted, how deeply
/// the NPC was convinced (conviction depth), relationship changes, and
/// any dialogue options that were unlocked or locked as a result.
/// </para>
/// </remarks>
/// <param name="TargetId">The NPC that was persuaded.</param>
/// <param name="RequestType">The complexity of the request.</param>
/// <param name="Outcome">The skill check outcome.</param>
/// <param name="OutcomeDetails">Enhanced outcome metadata.</param>
/// <param name="RequestGranted">Whether the NPC agreed to the request.</param>
/// <param name="ConvictionDepth">How deeply the NPC was convinced.</param>
/// <param name="DispositionChange">Change to NPC disposition.</param>
/// <param name="NewDisposition">NPC disposition after the attempt.</param>
/// <param name="ReputationChange">Change to faction reputation.</param>
/// <param name="AffectedFactionId">The faction affected by reputation change.</param>
/// <param name="UnlockedOptions">Dialogue options now available.</param>
/// <param name="LockedOptions">Dialogue options now unavailable.</param>
/// <param name="FumbleConsequence">The fumble consequence if applicable.</param>
/// <param name="NarrativeText">Flavor text describing the outcome.</param>
public readonly record struct PersuasionResult(
    string TargetId,
    PersuasionRequest RequestType,
    SkillOutcome Outcome,
    OutcomeDetails OutcomeDetails,
    bool RequestGranted,
    ConvictionDepth ConvictionDepth,
    int DispositionChange,
    DispositionLevel NewDisposition,
    int ReputationChange,
    string? AffectedFactionId,
    IReadOnlyList<string> UnlockedOptions,
    IReadOnlyList<string> LockedOptions,
    FumbleConsequence? FumbleConsequence,
    string? NarrativeText)
{
    /// <summary>
    /// Gets whether the persuasion succeeded.
    /// </summary>
    public bool IsSuccess => Outcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets whether this was a critical success.
    /// </summary>
    public bool IsCriticalSuccess => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Gets whether this was a fumble.
    /// </summary>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets whether [Trust Shattered] was triggered.
    /// </summary>
    public bool IsTrustShattered => FumbleConsequence?.ConsequenceType == FumbleType.TrustShattered;

    /// <summary>
    /// Gets whether any dialogue options were unlocked.
    /// </summary>
    public bool HasUnlockedOptions => UnlockedOptions.Count > 0;

    /// <summary>
    /// Gets whether any dialogue options were locked.
    /// </summary>
    public bool HasLockedOptions => LockedOptions.Count > 0;

    /// <summary>
    /// Creates a successful persuasion result.
    /// </summary>
    /// <param name="targetId">The NPC that was persuaded.</param>
    /// <param name="requestType">The complexity of the request.</param>
    /// <param name="outcome">The skill check outcome.</param>
    /// <param name="details">The outcome details.</param>
    /// <param name="currentDisposition">The NPC's current disposition.</param>
    /// <param name="factionId">The NPC's faction ID.</param>
    /// <param name="unlockedOptions">Dialogue options to unlock.</param>
    /// <param name="narrativeText">Flavor text for the outcome.</param>
    /// <returns>A successful PersuasionResult.</returns>
    public static PersuasionResult CreateSuccess(
        string targetId,
        PersuasionRequest requestType,
        SkillOutcome outcome,
        OutcomeDetails details,
        DispositionLevel currentDisposition,
        string? factionId = null,
        IReadOnlyList<string>? unlockedOptions = null,
        string? narrativeText = null)
    {
        var convictionDepth = outcome switch
        {
            SkillOutcome.CriticalSuccess => ConvictionDepth.Deep,
            SkillOutcome.ExceptionalSuccess => ConvictionDepth.Strong,
            SkillOutcome.FullSuccess => ConvictionDepth.Moderate,
            SkillOutcome.MarginalSuccess => ConvictionDepth.Shallow,
            _ => ConvictionDepth.None
        };

        var dispositionChange = outcome switch
        {
            SkillOutcome.CriticalSuccess => 15,
            SkillOutcome.ExceptionalSuccess => 12,
            SkillOutcome.FullSuccess => 10,
            SkillOutcome.MarginalSuccess => 5,
            _ => 0
        };

        var reputationChange = outcome switch
        {
            SkillOutcome.CriticalSuccess => 5,
            SkillOutcome.ExceptionalSuccess => 3,
            SkillOutcome.FullSuccess => 2,
            _ => 0
        };

        return new PersuasionResult(
            TargetId: targetId,
            RequestType: requestType,
            Outcome: outcome,
            OutcomeDetails: details,
            RequestGranted: true,
            ConvictionDepth: convictionDepth,
            DispositionChange: dispositionChange,
            NewDisposition: currentDisposition.Modify(dispositionChange),
            ReputationChange: reputationChange,
            AffectedFactionId: factionId,
            UnlockedOptions: unlockedOptions ?? Array.Empty<string>(),
            LockedOptions: Array.Empty<string>(),
            FumbleConsequence: null,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a failed persuasion result.
    /// </summary>
    /// <param name="targetId">The NPC that was persuaded.</param>
    /// <param name="requestType">The complexity of the request.</param>
    /// <param name="outcome">The skill check outcome.</param>
    /// <param name="details">The outcome details.</param>
    /// <param name="currentDisposition">The NPC's current disposition.</param>
    /// <param name="factionId">The NPC's faction ID.</param>
    /// <param name="lockedOptions">Dialogue options to lock.</param>
    /// <param name="narrativeText">Flavor text for the outcome.</param>
    /// <returns>A failed PersuasionResult.</returns>
    public static PersuasionResult CreateFailure(
        string targetId,
        PersuasionRequest requestType,
        SkillOutcome outcome,
        OutcomeDetails details,
        DispositionLevel currentDisposition,
        string? factionId = null,
        IReadOnlyList<string>? lockedOptions = null,
        string? narrativeText = null)
    {
        return new PersuasionResult(
            TargetId: targetId,
            RequestType: requestType,
            Outcome: outcome,
            OutcomeDetails: details,
            RequestGranted: false,
            ConvictionDepth: ConvictionDepth.None,
            DispositionChange: -5,
            NewDisposition: currentDisposition.Modify(-5),
            ReputationChange: -2,
            AffectedFactionId: factionId,
            UnlockedOptions: Array.Empty<string>(),
            LockedOptions: lockedOptions ?? Array.Empty<string>(),
            FumbleConsequence: null,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a fumble (Trust Shattered) persuasion result.
    /// </summary>
    /// <param name="targetId">The NPC that was persuaded.</param>
    /// <param name="requestType">The complexity of the request.</param>
    /// <param name="details">The outcome details.</param>
    /// <param name="currentDisposition">The NPC's current disposition.</param>
    /// <param name="consequence">The fumble consequence entity.</param>
    /// <param name="factionId">The NPC's faction ID.</param>
    /// <param name="lockedOptions">Dialogue options to lock.</param>
    /// <param name="narrativeText">Flavor text for the outcome.</param>
    /// <returns>A fumble PersuasionResult with TrustShattered consequence.</returns>
    public static PersuasionResult CreateTrustShattered(
        string targetId,
        PersuasionRequest requestType,
        OutcomeDetails details,
        DispositionLevel currentDisposition,
        FumbleConsequence consequence,
        string? factionId = null,
        IReadOnlyList<string>? lockedOptions = null,
        string? narrativeText = null)
    {
        return new PersuasionResult(
            TargetId: targetId,
            RequestType: requestType,
            Outcome: SkillOutcome.CriticalFailure,
            OutcomeDetails: details,
            RequestGranted: false,
            ConvictionDepth: ConvictionDepth.None,
            DispositionChange: -30,
            NewDisposition: currentDisposition.Modify(-30),
            ReputationChange: -10,
            AffectedFactionId: factionId,
            UnlockedOptions: Array.Empty<string>(),
            LockedOptions: lockedOptions ?? new[] { "*" }, // Lock all persuasion options
            FumbleConsequence: consequence,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Gets a summary of the result for display.
    /// </summary>
    /// <returns>A multi-line string summarizing the persuasion outcome.</returns>
    public string ToSummary()
    {
        var lines = new List<string>
        {
            $"Persuasion ({RequestType.GetShortName()}): {Outcome}"
        };

        if (RequestGranted)
        {
            lines.Add($"Request GRANTED (Conviction: {ConvictionDepth})");
        }
        else
        {
            lines.Add("Request DENIED");
        }

        if (DispositionChange != 0)
        {
            var sign = DispositionChange > 0 ? "+" : "";
            lines.Add($"Disposition: {sign}{DispositionChange} → {NewDisposition.Category.GetDescription()}");
        }

        if (ReputationChange != 0 && !string.IsNullOrEmpty(AffectedFactionId))
        {
            var sign = ReputationChange > 0 ? "+" : "";
            lines.Add($"Reputation ({AffectedFactionId}): {sign}{ReputationChange}");
        }

        if (HasUnlockedOptions)
        {
            lines.Add($"Unlocked: {string.Join(", ", UnlockedOptions)}");
        }

        if (HasLockedOptions)
        {
            var lockedDisplay = LockedOptions.Contains("*") ? "ALL PERSUASION OPTIONS" : string.Join(", ", LockedOptions);
            lines.Add($"Locked: {lockedDisplay}");
        }

        if (IsTrustShattered)
        {
            lines.Add("FUMBLE: [Trust Shattered] - All future persuasion locked with this NPC");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <inheritdoc/>
    public override string ToString() => ToSummary();
}
