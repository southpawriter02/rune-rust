using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of a cooperative skill check involving multiple participants.
/// </summary>
/// <remarks>
/// <para>
/// Contains the outcome of a cooperative check along with details about each
/// participant's contribution and the cooperation method used.
/// </para>
/// </remarks>
/// <param name="CooperationType">The cooperation method used.</param>
/// <param name="ParticipantIds">IDs of all participants in the check.</param>
/// <param name="SkillId">The skill used for the check.</param>
/// <param name="SubType">Optional skill subtype.</param>
/// <param name="DifficultyClass">The DC that was checked against.</param>
/// <param name="FinalOutcome">The final outcome of the cooperative check.</param>
/// <param name="FinalNetSuccesses">The final net successes (varies by cooperation type).</param>
/// <param name="ActiveRollerId">ID of the participant whose roll determined the outcome (for WeakestLink/BestAttempt).</param>
/// <param name="IndividualResults">Individual check results for each participant (for BestAttempt/Combined).</param>
/// <param name="HelperBonuses">For Assisted: which helpers contributed bonus dice.</param>
/// <param name="ContributingParticipants">IDs of participants who contributed to success.</param>
public readonly record struct CooperativeCheckResult(
    CooperationType CooperationType,
    IReadOnlyList<string> ParticipantIds,
    string SkillId,
    string? SubType,
    int DifficultyClass,
    SkillOutcome FinalOutcome,
    int FinalNetSuccesses,
    string? ActiveRollerId = null,
    IReadOnlyList<SkillCheckResult>? IndividualResults = null,
    IReadOnlyList<HelperContribution>? HelperBonuses = null,
    IReadOnlyList<string>? ContributingParticipants = null)
{
    /// <summary>
    /// Whether the cooperative check succeeded (any success outcome).
    /// </summary>
    public bool IsSuccess => FinalOutcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Whether any participant fumbled during the check.
    /// </summary>
    public bool HadFumble => IndividualResults?.Any(r => r.IsFumble) ?? false;

    /// <summary>
    /// Gets the margin of success (positive) or failure (negative).
    /// </summary>
    public int Margin => FinalNetSuccesses - DifficultyClass;

    /// <summary>
    /// Gets a display string summarizing the cooperative check.
    /// </summary>
    public string ToDisplayString()
    {
        var typeStr = CooperationType switch
        {
            CooperationType.WeakestLink => "Weakest Link",
            CooperationType.BestAttempt => "Best Attempt",
            CooperationType.Combined => "Combined Effort",
            CooperationType.Assisted => "Assisted",
            _ => "Unknown"
        };

        var outcomeStr = FinalOutcome switch
        {
            SkillOutcome.CriticalFailure => "Critical Failure!",
            SkillOutcome.Failure => "Failure",
            SkillOutcome.MarginalSuccess => "Marginal Success",
            SkillOutcome.FullSuccess => "Success",
            SkillOutcome.ExceptionalSuccess => "Exceptional Success",
            SkillOutcome.CriticalSuccess => "Critical Success!",
            _ => "Unknown"
        };

        return $"{typeStr} ({ParticipantIds.Count} participants): {outcomeStr} ({FinalNetSuccesses} vs DC {DifficultyClass})";
    }
}

/// <summary>
/// Records a helper's contribution to an assisted check.
/// </summary>
/// <param name="HelperId">The helper's character ID.</param>
/// <param name="NetSuccesses">Net successes the helper rolled.</param>
/// <param name="GrantedBonus">Whether this helper granted a bonus die (net >= 2).</param>
public readonly record struct HelperContribution(
    string HelperId,
    int NetSuccesses,
    bool GrantedBonus);
