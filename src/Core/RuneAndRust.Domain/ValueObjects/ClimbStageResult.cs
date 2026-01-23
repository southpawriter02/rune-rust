using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a single climbing stage attempt.
/// </summary>
/// <remarks>
/// <para>
/// Stage results determine climb progression based on the outcome matrix:
/// <list type="bullet">
///   <item><description>Critical Success (margin ≥ 5): Advance 2 stages or reach top</description></item>
///   <item><description>Success (margin 0-4): Advance 1 stage</description></item>
///   <item><description>Failure (margin &lt; 0): Slip back 1 stage (minimum stage 0)</description></item>
///   <item><description>Fumble (0 successes + ≥1 botch): Fall from current height</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="StageAttempted">The stage that was attempted.</param>
/// <param name="Outcome">The skill outcome from the check.</param>
/// <param name="NetSuccesses">Net successes rolled (successes - botches).</param>
/// <param name="Margin">The margin (net successes - DC).</param>
/// <param name="NewStage">The stage the climber is now at after this result.</param>
/// <param name="ClimbStatus">The climb status after this result.</param>
/// <param name="FallTriggered">Whether this result triggered a fall.</param>
/// <param name="FallHeight">Height of the fall if triggered, in feet.</param>
/// <param name="StagesAdvanced">Number of stages advanced (0, 1, or 2).</param>
/// <param name="Description">Optional narrative description of the result.</param>
public readonly record struct ClimbStageResult(
    ClimbingStage StageAttempted,
    SkillOutcome Outcome,
    int NetSuccesses,
    int Margin,
    int NewStage,
    ClimbStatus ClimbStatus,
    bool FallTriggered = false,
    int FallHeight = 0,
    int StagesAdvanced = 0,
    string? Description = null)
{
    /// <summary>
    /// Gets a value indicating whether this result represents a success.
    /// </summary>
    public bool IsSuccess => Outcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets a value indicating whether this result is a critical success.
    /// </summary>
    public bool IsCriticalSuccess => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Gets a value indicating whether this result is a fumble.
    /// </summary>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets a value indicating whether the climb was completed with this result.
    /// </summary>
    public bool ClimbCompleted => ClimbStatus == Enums.ClimbStatus.Completed;

    /// <summary>
    /// Creates a successful stage result.
    /// </summary>
    /// <param name="stage">The stage that was attempted.</param>
    /// <param name="outcome">The successful skill outcome.</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <param name="margin">The margin over the DC.</param>
    /// <param name="newStage">The new stage after advancement.</param>
    /// <param name="totalStages">Total stages required for the climb.</param>
    /// <param name="stagesAdvanced">Number of stages advanced (1 or 2).</param>
    /// <returns>A success result with appropriate status.</returns>
    public static ClimbStageResult Success(
        ClimbingStage stage,
        SkillOutcome outcome,
        int netSuccesses,
        int margin,
        int newStage,
        int totalStages,
        int stagesAdvanced = 1)
    {
        var isComplete = newStage >= totalStages;
        var status = isComplete ? Enums.ClimbStatus.Completed : Enums.ClimbStatus.InProgress;

        var description = outcome == SkillOutcome.CriticalSuccess
            ? "Magnificent climb! You surge upward with incredible speed."
            : "You make steady progress up the surface.";

        return new ClimbStageResult(
            StageAttempted: stage.WithResult(outcome),
            Outcome: outcome,
            NetSuccesses: netSuccesses,
            Margin: margin,
            NewStage: newStage,
            ClimbStatus: status,
            FallTriggered: false,
            FallHeight: 0,
            StagesAdvanced: stagesAdvanced,
            Description: description);
    }

    /// <summary>
    /// Creates a failure stage result (slip back).
    /// </summary>
    /// <param name="stage">The stage that was attempted.</param>
    /// <param name="outcome">The failure skill outcome.</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <param name="margin">The negative margin.</param>
    /// <param name="newStage">The stage after slipping back.</param>
    /// <returns>A failure result with slip-back status.</returns>
    public static ClimbStageResult Failure(
        ClimbingStage stage,
        SkillOutcome outcome,
        int netSuccesses,
        int margin,
        int newStage)
    {
        // If slipped to below stage 0, status becomes SlippedToGround
        var status = newStage <= 0
            ? Enums.ClimbStatus.SlippedToGround
            : Enums.ClimbStatus.InProgress;

        return new ClimbStageResult(
            StageAttempted: stage.WithResult(outcome),
            Outcome: outcome,
            NetSuccesses: netSuccesses,
            Margin: margin,
            NewStage: Math.Max(0, newStage),
            ClimbStatus: status,
            FallTriggered: false,
            FallHeight: 0,
            StagesAdvanced: 0,
            Description: "Your grip falters and you slip back down.");
    }

    /// <summary>
    /// Creates a fumble stage result (fall triggered).
    /// </summary>
    /// <param name="stage">The stage where the fumble occurred.</param>
    /// <param name="netSuccesses">Net successes rolled (should be 0).</param>
    /// <param name="margin">The margin (should be negative).</param>
    /// <param name="fallHeight">Height of the fall in feet.</param>
    /// <returns>A fumble result with fall triggered.</returns>
    public static ClimbStageResult Fumble(
        ClimbingStage stage,
        int netSuccesses,
        int margin,
        int fallHeight)
    {
        return new ClimbStageResult(
            StageAttempted: stage.WithResult(SkillOutcome.CriticalFailure),
            Outcome: SkillOutcome.CriticalFailure,
            NetSuccesses: netSuccesses,
            Margin: margin,
            NewStage: 0,
            ClimbStatus: Enums.ClimbStatus.Fallen,
            FallTriggered: true,
            FallHeight: fallHeight,
            StagesAdvanced: 0,
            Description: "You lose your grip entirely and plummet to the ground!");
    }

    /// <summary>
    /// Returns a summary of the stage result for display.
    /// </summary>
    /// <returns>Formatted result summary.</returns>
    public string ToSummary()
    {
        var outcomeStr = Outcome.ToString();
        var marginStr = Margin >= 0 ? $"+{Margin}" : Margin.ToString();

        var summary = $"Stage {StageAttempted.StageNumber}: {outcomeStr} ({NetSuccesses} successes, margin {marginStr})";

        if (FallTriggered)
        {
            summary += $" - FELL {FallHeight}ft!";
        }
        else if (StagesAdvanced > 0)
        {
            summary += $" - Advanced {StagesAdvanced} stage{(StagesAdvanced > 1 ? "s" : "")}";
        }
        else if (Outcome == SkillOutcome.Failure)
        {
            summary += " - Slipped back";
        }

        return summary;
    }

    /// <inheritdoc/>
    public override string ToString() => ToSummary();
}
