namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Provides detailed metadata about a skill check outcome beyond the basic <see cref="SkillOutcome"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="OutcomeDetails"/> encapsulates all outcome-related information in a single immutable object,
/// enabling rich narrative feedback and mechanical effects without requiring multiple property lookups.
/// </para>
/// <para>
/// This value object is created during skill check resolution and attached to <see cref="SkillCheckResult"/>.
/// </para>
/// </remarks>
public sealed class OutcomeDetails
{
    /// <summary>
    /// Gets the classified outcome tier from the 6-tier scale.
    /// </summary>
    public SkillOutcome OutcomeType { get; }

    /// <summary>
    /// Gets the margin of success or failure (NetSuccesses - DifficultyClass).
    /// </summary>
    /// <remarks>
    /// Positive values indicate success degree, negative values indicate failure severity.
    /// </remarks>
    public int Margin { get; }

    /// <summary>
    /// Gets a value indicating whether this outcome is a fumble (0 successes + ≥1 botch).
    /// </summary>
    public bool IsFumble { get; }

    /// <summary>
    /// Gets a value indicating whether this outcome is a critical success (net ≥ 5 or margin ≥ 5).
    /// </summary>
    public bool IsCritical { get; }

    /// <summary>
    /// Gets the descriptor category for voice guidance narrative lookup.
    /// </summary>
    public DescriptorCategory DescriptorCategory { get; }

    /// <summary>
    /// Gets a value indicating whether the check was successful (met or exceeded DC).
    /// </summary>
    public bool IsSuccess => OutcomeType >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets a value indicating whether the check was a failure (did not meet DC).
    /// </summary>
    public bool IsFailure => OutcomeType <= SkillOutcome.Failure;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutcomeDetails"/> class.
    /// </summary>
    /// <param name="outcomeType">The classified outcome tier.</param>
    /// <param name="margin">The margin of success/failure.</param>
    /// <param name="isFumble">Whether this is a fumble.</param>
    /// <param name="isCritical">Whether this is a critical success.</param>
    public OutcomeDetails(
        SkillOutcome outcomeType,
        int margin,
        bool isFumble,
        bool isCritical)
    {
        OutcomeType = outcomeType;
        Margin = margin;
        IsFumble = isFumble;
        IsCritical = isCritical;
        DescriptorCategory = MapToDescriptorCategory(outcomeType);
    }

    /// <summary>
    /// Creates an <see cref="OutcomeDetails"/> from a dice roll result and difficulty class.
    /// </summary>
    /// <param name="diceResult">The dice roll result.</param>
    /// <param name="difficultyClass">The difficulty class to compare against.</param>
    /// <returns>A new <see cref="OutcomeDetails"/> instance.</returns>
    public static OutcomeDetails FromDiceResult(DiceRollResult diceResult, int difficultyClass)
    {
        var margin = diceResult.NetSuccesses - difficultyClass;
        var outcomeType = ClassifyOutcome(diceResult, margin);

        return new OutcomeDetails(
            outcomeType,
            margin,
            diceResult.IsFumble,
            diceResult.IsCriticalSuccess || margin >= 5);
    }

    /// <summary>
    /// Classifies the outcome based on fumble detection and margin calculation.
    /// </summary>
    private static SkillOutcome ClassifyOutcome(DiceRollResult diceResult, int margin)
    {
        // Fumble always results in CriticalFailure regardless of DC
        if (diceResult.IsFumble)
        {
            return SkillOutcome.CriticalFailure;
        }

        return margin switch
        {
            < 0 => SkillOutcome.Failure,
            0 => SkillOutcome.MarginalSuccess,
            1 or 2 => SkillOutcome.FullSuccess,
            3 or 4 => SkillOutcome.ExceptionalSuccess,
            _ => SkillOutcome.CriticalSuccess // margin >= 5
        };
    }

    /// <summary>
    /// Maps SkillOutcome to the corresponding DescriptorCategory for voice guidance.
    /// </summary>
    private static DescriptorCategory MapToDescriptorCategory(SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalFailure => DescriptorCategory.Catastrophic,
            SkillOutcome.Failure => DescriptorCategory.Failed,
            SkillOutcome.MarginalSuccess => DescriptorCategory.Marginal,
            SkillOutcome.FullSuccess => DescriptorCategory.Competent,
            SkillOutcome.ExceptionalSuccess => DescriptorCategory.Impressive,
            SkillOutcome.CriticalSuccess => DescriptorCategory.Masterful,
            _ => DescriptorCategory.Failed
        };
    }

    /// <summary>
    /// Returns a human-readable description of the outcome.
    /// </summary>
    public string ToDescription()
    {
        var baseDescription = OutcomeType switch
        {
            SkillOutcome.CriticalFailure => "Critical Failure (Fumble)",
            SkillOutcome.Failure => "Failure",
            SkillOutcome.MarginalSuccess => "Marginal Success",
            SkillOutcome.FullSuccess => "Success",
            SkillOutcome.ExceptionalSuccess => "Exceptional Success",
            SkillOutcome.CriticalSuccess => "Critical Success",
            _ => "Unknown"
        };

        if (Margin != 0 && OutcomeType != SkillOutcome.CriticalFailure)
        {
            var marginText = Margin > 0 ? $"+{Margin}" : Margin.ToString();
            return $"{baseDescription} (Margin: {marginText})";
        }

        return baseDescription;
    }
}
