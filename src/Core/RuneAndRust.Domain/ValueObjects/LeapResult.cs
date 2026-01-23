using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a leap attempt.
/// </summary>
/// <remarks>
/// <para>
/// Captures all relevant information about a completed leap including:
/// <list type="bullet">
///   <item><description>Success or failure status</description></item>
///   <item><description>Fall triggering and depth</description></item>
///   <item><description>Stamina cost (modified by outcome)</description></item>
///   <item><description>Fumble effects ([The Long Fall])</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Context">The leap context that was attempted.</param>
/// <param name="Outcome">The skill check outcome.</param>
/// <param name="NetSuccesses">Number of net successes from the dice roll.</param>
/// <param name="Margin">Margin of success or failure (NetSuccesses - DC).</param>
/// <param name="Landed">Whether the character successfully landed.</param>
/// <param name="FallTriggered">Whether a fall was triggered by failure.</param>
/// <param name="FallResult">Fall details if a fall was triggered.</param>
/// <param name="StaminaCost">Final stamina cost after outcome modification.</param>
/// <param name="IsFumble">Whether this was a fumble ([The Long Fall]).</param>
/// <param name="BonusDamage">Bonus damage dice from fumble.</param>
/// <param name="StressGained">Stress gained from fumble.</param>
/// <param name="StatusApplied">Status effect applied (e.g., [Disoriented]).</param>
/// <param name="StatusDuration">Duration of applied status in rounds.</param>
/// <param name="Description">Narrative description of the outcome.</param>
public readonly record struct LeapResult(
    LeapContext Context,
    SkillOutcome Outcome,
    int NetSuccesses,
    int Margin,
    bool Landed,
    bool FallTriggered = false,
    FallResult? FallResult = null,
    int StaminaCost = 0,
    bool IsFumble = false,
    int BonusDamage = 0,
    int StressGained = 0,
    string? StatusApplied = null,
    int StatusDuration = 0,
    string? Description = null)
{
    /// <summary>
    /// Gets a value indicating whether this was a successful leap.
    /// </summary>
    public bool IsSuccess => Outcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets a value indicating whether this was a critical success.
    /// </summary>
    public bool IsCriticalSuccess => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Gets a value indicating whether this was a marginal success.
    /// </summary>
    public bool IsMarginalSuccess => Outcome == SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Creates a successful leap result.
    /// </summary>
    /// <param name="context">The leap context.</param>
    /// <param name="outcome">The skill outcome (must be success variant).</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <param name="margin">Margin of success.</param>
    /// <returns>A LeapResult representing a successful landing.</returns>
    public static LeapResult Success(
        LeapContext context,
        SkillOutcome outcome,
        int netSuccesses,
        int margin)
    {
        var baseStamina = context.BaseStaminaCost;
        var staminaCost = outcome switch
        {
            SkillOutcome.CriticalSuccess => baseStamina / 2,
            SkillOutcome.MarginalSuccess => baseStamina + 1,
            _ => baseStamina
        };

        var description = outcome switch
        {
            SkillOutcome.CriticalSuccess =>
                "You soar through the air with perfect form, landing gracefully on the other side.",
            SkillOutcome.ExceptionalSuccess =>
                "An impressive leap! You clear the gap with room to spare.",
            SkillOutcome.FullSuccess =>
                "You clear the gap and land solidly on the other side.",
            SkillOutcome.MarginalSuccess =>
                "You barely make it, stumbling slightly as you land on the edge.",
            _ => "You land successfully."
        };

        return new LeapResult(
            Context: context,
            Outcome: outcome,
            NetSuccesses: netSuccesses,
            Margin: margin,
            Landed: true,
            StaminaCost: staminaCost,
            Description: description);
    }

    /// <summary>
    /// Creates a failed leap result (fall into gap).
    /// </summary>
    /// <param name="context">The leap context.</param>
    /// <param name="outcome">The skill outcome (Failure).</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <param name="margin">Margin of failure (negative).</param>
    /// <returns>A LeapResult representing a fall.</returns>
    public static LeapResult Failure(
        LeapContext context,
        SkillOutcome outcome,
        int netSuccesses,
        int margin)
    {
        var fallResult = Domain.ValueObjects.FallResult.FromHeight(
            context.FallDepth,
            FallSource.Leaping,
            triggeredByFumble: false);

        return new LeapResult(
            Context: context,
            Outcome: outcome,
            NetSuccesses: netSuccesses,
            Margin: margin,
            Landed: false,
            FallTriggered: true,
            FallResult: fallResult,
            Description: "You don't quite make it. Your fingers scrape the edge as you fall into the gap.");
    }

    /// <summary>
    /// Creates a fumble result ([The Long Fall]).
    /// </summary>
    /// <param name="context">The leap context.</param>
    /// <param name="netSuccesses">Net successes rolled (0 for fumble).</param>
    /// <param name="margin">Margin of failure.</param>
    /// <returns>A LeapResult representing [The Long Fall] fumble.</returns>
    /// <remarks>
    /// [The Long Fall] applies:
    /// <list type="bullet">
    ///   <item><description>+1d10 bonus fall damage</description></item>
    ///   <item><description>+2 Stress</description></item>
    ///   <item><description>[Disoriented] for 2 rounds (-2d10 to all checks)</description></item>
    /// </list>
    /// </remarks>
    public static LeapResult Fumble(
        LeapContext context,
        int netSuccesses,
        int margin)
    {
        var fallResult = Domain.ValueObjects.FallResult.TheLongFall(
            context.FallDepth,
            bonusDamageDice: 1);

        return new LeapResult(
            Context: context,
            Outcome: SkillOutcome.CriticalFailure,
            NetSuccesses: netSuccesses,
            Margin: margin,
            Landed: false,
            FallTriggered: true,
            FallResult: fallResult,
            IsFumble: true,
            BonusDamage: 1,
            StressGained: 2,
            StatusApplied: "Disoriented",
            StatusDuration: 2,
            Description: "You misjudge the leap catastrophically! Tumbling through the air, " +
                        "you land badly, the impact jarring your senses. [Disoriented] for 2 rounds.");
    }

    /// <summary>
    /// Gets a formatted summary of the leap result.
    /// </summary>
    /// <returns>A single-line summary for logging or display.</returns>
    public string ToSummary()
    {
        var outcomeStr = Outcome switch
        {
            SkillOutcome.CriticalSuccess => "CRITICAL SUCCESS",
            SkillOutcome.ExceptionalSuccess => "Exceptional Success",
            SkillOutcome.FullSuccess => "Success",
            SkillOutcome.MarginalSuccess => "Marginal Success",
            SkillOutcome.Failure => "Failure",
            SkillOutcome.CriticalFailure => "FUMBLE - The Long Fall!",
            _ => Outcome.ToString()
        };

        var resultStr = Landed ? "Landed!" : $"Fell {Context.FallDepth}ft";

        var extras = new List<string>();
        if (StaminaCost > 0) extras.Add($"Stamina: {StaminaCost}");
        if (IsFumble)
        {
            extras.Add($"+{BonusDamage}d10 bonus damage");
            extras.Add($"+{StressGained} Stress");
            extras.Add($"[{StatusApplied}] {StatusDuration} rounds");
        }

        var extrasStr = extras.Count > 0 ? $" ({string.Join(", ", extras)})" : "";

        return $"Leap {Context.DistanceFeet}ft: {outcomeStr} - {resultStr}{extrasStr}";
    }

    /// <inheritdoc/>
    public override string ToString() => ToSummary();
}
