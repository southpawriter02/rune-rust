namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Represents the outcome of a social interaction check.
/// </summary>
/// <remarks>
/// <para>
/// Social results capture not just success/failure but also the ripple effects
/// of the interaction: disposition changes, reputation changes, unlocked or
/// locked dialogue options, stress costs, and fumble consequences.
/// </para>
/// <para>
/// This value object is created by social interaction services after resolving
/// the skill check and applying all interaction-specific rules.
/// </para>
/// </remarks>
/// <param name="InteractionType">The type of social interaction performed.</param>
/// <param name="TargetId">The ID of the NPC interacted with.</param>
/// <param name="Outcome">The skill check outcome.</param>
/// <param name="OutcomeDetails">Enhanced outcome metadata.</param>
/// <param name="DispositionChange">How the NPC's disposition changed.</param>
/// <param name="NewDisposition">The NPC's disposition after the interaction.</param>
/// <param name="ReputationChange">Faction reputation change, if any.</param>
/// <param name="AffectedFactionId">The faction affected by reputation change.</param>
/// <param name="UnlockedOptions">Dialogue options now available.</param>
/// <param name="LockedOptions">Dialogue options now unavailable.</param>
/// <param name="StressCost">Psychic Stress incurred by the interaction.</param>
/// <param name="FumbleConsequence">The fumble consequence if this was a fumble.</param>
/// <param name="NarrativeText">Flavor text describing the outcome.</param>
public readonly record struct SocialResult(
    SocialInteractionType InteractionType,
    string TargetId,
    SkillOutcome Outcome,
    OutcomeDetails OutcomeDetails,
    int DispositionChange,
    DispositionLevel NewDisposition,
    int ReputationChange,
    string? AffectedFactionId,
    IReadOnlyList<string> UnlockedOptions,
    IReadOnlyList<string> LockedOptions,
    int StressCost,
    FumbleConsequence? FumbleConsequence,
    string? NarrativeText)
{
    /// <summary>
    /// Gets whether the interaction succeeded.
    /// </summary>
    public bool IsSuccess => Outcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets whether the interaction was a critical success.
    /// </summary>
    public bool IsCriticalSuccess => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Gets whether the interaction was a fumble.
    /// </summary>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets whether any dialogue options were unlocked.
    /// </summary>
    public bool HasUnlockedOptions => UnlockedOptions.Count > 0;

    /// <summary>
    /// Gets whether any dialogue options were locked.
    /// </summary>
    public bool HasLockedOptions => LockedOptions.Count > 0;

    /// <summary>
    /// Gets whether the interaction incurred stress.
    /// </summary>
    public bool HasStressCost => StressCost > 0;

    /// <summary>
    /// Gets whether the interaction affected reputation.
    /// </summary>
    public bool HasReputationChange => ReputationChange != 0 && !string.IsNullOrEmpty(AffectedFactionId);

    /// <summary>
    /// Gets whether the interaction affected disposition.
    /// </summary>
    public bool HasDispositionChange => DispositionChange != 0;

    /// <summary>
    /// Gets whether a fumble consequence was created.
    /// </summary>
    public bool HasFumbleConsequence => FumbleConsequence != null;

    /// <summary>
    /// Gets the fumble type if this was a fumble, otherwise null.
    /// </summary>
    public FumbleType? FumbleType => IsFumble ? FumbleConsequence?.ConsequenceType : null;

    /// <summary>
    /// Creates a successful social result.
    /// </summary>
    /// <param name="interactionType">The type of interaction.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="outcome">The skill outcome.</param>
    /// <param name="details">The outcome details.</param>
    /// <param name="currentDisposition">The current disposition before modification.</param>
    /// <param name="dispositionChange">The change in disposition.</param>
    /// <param name="reputationChange">The change in reputation.</param>
    /// <param name="factionId">The affected faction ID.</param>
    /// <param name="unlockedOptions">Dialogue options unlocked.</param>
    /// <param name="stressCost">Stress incurred.</param>
    /// <param name="narrativeText">Flavor text.</param>
    /// <returns>A successful social result.</returns>
    public static SocialResult CreateSuccess(
        SocialInteractionType interactionType,
        string targetId,
        SkillOutcome outcome,
        OutcomeDetails details,
        DispositionLevel currentDisposition,
        int dispositionChange,
        int reputationChange = 0,
        string? factionId = null,
        IReadOnlyList<string>? unlockedOptions = null,
        int stressCost = 0,
        string? narrativeText = null)
    {
        return new SocialResult(
            InteractionType: interactionType,
            TargetId: targetId,
            Outcome: outcome,
            OutcomeDetails: details,
            DispositionChange: dispositionChange,
            NewDisposition: currentDisposition.Modify(dispositionChange),
            ReputationChange: reputationChange,
            AffectedFactionId: factionId,
            UnlockedOptions: unlockedOptions ?? Array.Empty<string>(),
            LockedOptions: Array.Empty<string>(),
            StressCost: stressCost,
            FumbleConsequence: null,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a failed social result.
    /// </summary>
    /// <param name="interactionType">The type of interaction.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="outcome">The skill outcome.</param>
    /// <param name="details">The outcome details.</param>
    /// <param name="currentDisposition">The current disposition before modification.</param>
    /// <param name="dispositionChange">The change in disposition (typically negative).</param>
    /// <param name="reputationChange">The change in reputation.</param>
    /// <param name="factionId">The affected faction ID.</param>
    /// <param name="lockedOptions">Dialogue options now locked.</param>
    /// <param name="stressCost">Stress incurred.</param>
    /// <param name="narrativeText">Flavor text.</param>
    /// <returns>A failed social result.</returns>
    public static SocialResult CreateFailure(
        SocialInteractionType interactionType,
        string targetId,
        SkillOutcome outcome,
        OutcomeDetails details,
        DispositionLevel currentDisposition,
        int dispositionChange,
        int reputationChange = 0,
        string? factionId = null,
        IReadOnlyList<string>? lockedOptions = null,
        int stressCost = 0,
        string? narrativeText = null)
    {
        return new SocialResult(
            InteractionType: interactionType,
            TargetId: targetId,
            Outcome: outcome,
            OutcomeDetails: details,
            DispositionChange: dispositionChange,
            NewDisposition: currentDisposition.Modify(dispositionChange),
            ReputationChange: reputationChange,
            AffectedFactionId: factionId,
            UnlockedOptions: Array.Empty<string>(),
            LockedOptions: lockedOptions ?? Array.Empty<string>(),
            StressCost: stressCost,
            FumbleConsequence: null,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Creates a fumble social result with consequences.
    /// </summary>
    /// <param name="interactionType">The type of interaction.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="details">The outcome details.</param>
    /// <param name="currentDisposition">The current disposition before modification.</param>
    /// <param name="dispositionChange">The change in disposition (typically very negative).</param>
    /// <param name="consequence">The fumble consequence entity.</param>
    /// <param name="stressCost">Stress incurred from fumble.</param>
    /// <param name="reputationChange">The change in reputation.</param>
    /// <param name="factionId">The affected faction ID.</param>
    /// <param name="lockedOptions">Dialogue options now locked.</param>
    /// <param name="narrativeText">Flavor text.</param>
    /// <returns>A fumble social result.</returns>
    public static SocialResult CreateFumble(
        SocialInteractionType interactionType,
        string targetId,
        OutcomeDetails details,
        DispositionLevel currentDisposition,
        int dispositionChange,
        FumbleConsequence consequence,
        int stressCost,
        int reputationChange = 0,
        string? factionId = null,
        IReadOnlyList<string>? lockedOptions = null,
        string? narrativeText = null)
    {
        return new SocialResult(
            InteractionType: interactionType,
            TargetId: targetId,
            Outcome: SkillOutcome.CriticalFailure,
            OutcomeDetails: details,
            DispositionChange: dispositionChange,
            NewDisposition: currentDisposition.Modify(dispositionChange),
            ReputationChange: reputationChange,
            AffectedFactionId: factionId,
            UnlockedOptions: Array.Empty<string>(),
            LockedOptions: lockedOptions ?? Array.Empty<string>(),
            StressCost: stressCost,
            FumbleConsequence: consequence,
            NarrativeText: narrativeText);
    }

    /// <summary>
    /// Gets a summary of the result for display.
    /// </summary>
    /// <returns>A formatted string summarizing the result.</returns>
    public string ToSummary()
    {
        var lines = new List<string>
        {
            $"{InteractionType.GetDisplayName()}: {Outcome}"
        };

        if (HasDispositionChange)
        {
            var sign = DispositionChange > 0 ? "+" : "";
            lines.Add($"Disposition: {sign}{DispositionChange} → {NewDisposition.Category.GetDescription()}");
        }

        if (HasReputationChange)
        {
            var sign = ReputationChange > 0 ? "+" : "";
            lines.Add($"Reputation ({AffectedFactionId}): {sign}{ReputationChange}");
        }

        if (HasStressCost)
        {
            lines.Add($"Stress: +{StressCost}");
        }

        if (HasUnlockedOptions)
        {
            lines.Add($"Unlocked: {string.Join(", ", UnlockedOptions)}");
        }

        if (HasLockedOptions)
        {
            lines.Add($"Locked: {string.Join(", ", LockedOptions)}");
        }

        if (HasFumbleConsequence)
        {
            lines.Add($"FUMBLE: {FumbleConsequence!.ConsequenceType.GetDisplayName()}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a compact one-line summary for quick display.
    /// </summary>
    /// <returns>A compact result summary.</returns>
    public string ToCompactSummary()
    {
        var parts = new List<string>
        {
            $"{InteractionType.GetDisplayName()}: {Outcome}"
        };

        if (HasDispositionChange)
        {
            var sign = DispositionChange > 0 ? "+" : "";
            parts.Add($"Disposition {sign}{DispositionChange}");
        }

        if (HasStressCost)
        {
            parts.Add($"Stress +{StressCost}");
        }

        if (HasFumbleConsequence)
        {
            parts.Add($"[{FumbleConsequence!.ConsequenceType.GetDisplayName()}]");
        }

        return string.Join(" | ", parts);
    }

    /// <inheritdoc/>
    public override string ToString() => ToSummary();
}
