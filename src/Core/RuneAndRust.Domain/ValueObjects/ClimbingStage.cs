using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single stage of a multi-stage climbing attempt.
/// </summary>
/// <remarks>
/// <para>
/// Each stage represents progress toward a climbing target. Heights are divided into stages:
/// <list type="bullet">
///   <item><description>Stage 1: 0ft → 20ft</description></item>
///   <item><description>Stage 2: 20ft → 40ft</description></item>
///   <item><description>Stage 3: 40ft → destination</description></item>
/// </list>
/// </para>
/// <para>
/// Stage results are recorded using <see cref="WithResult"/> which creates a new instance
/// with the outcome and timestamp preserved.
/// </para>
/// </remarks>
/// <param name="StageNumber">The 1-based stage number (1, 2, or 3).</param>
/// <param name="HeightReached">The height reached upon completing this stage in feet.</param>
/// <param name="SurfaceType">The surface type affecting this stage's difficulty.</param>
/// <param name="StageDc">The difficulty class for this stage's skill check.</param>
/// <param name="StageResult">The skill outcome if this stage has been attempted.</param>
/// <param name="AttemptedAt">The timestamp when this stage was attempted.</param>
public readonly record struct ClimbingStage(
    int StageNumber,
    int HeightReached,
    SurfaceType SurfaceType,
    int StageDc,
    SkillOutcome? StageResult = null,
    DateTime? AttemptedAt = null)
{
    /// <summary>
    /// Gets the height at which this stage begins.
    /// </summary>
    /// <remarks>
    /// Used to calculate fall height if the climber fumbles during this stage.
    /// The climber falls from where they started, not where they would have ended.
    /// </remarks>
    public int StartingHeight => StageNumber switch
    {
        1 => 0,
        2 => 20,
        3 => 40,
        _ => 0
    };

    /// <summary>
    /// Gets the height from which the character would fall if they fumble on this stage.
    /// </summary>
    /// <remarks>
    /// Equivalent to <see cref="StartingHeight"/> - a fumble causes a fall from
    /// the height at which the climber began the stage, not the target height.
    /// </remarks>
    public int FallHeight => StartingHeight;

    /// <summary>
    /// Gets a value indicating whether this stage has been successfully completed.
    /// </summary>
    /// <remarks>
    /// A stage is completed if it has a result and that result is at least a marginal success.
    /// </remarks>
    public bool IsCompleted => StageResult.HasValue &&
        StageResult.Value >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets a value indicating whether this stage resulted in a fumble.
    /// </summary>
    /// <remarks>
    /// A fumble (0 successes + ≥1 botch) triggers [The Slip] fumble consequence
    /// and causes the character to fall from their current height.
    /// </remarks>
    public bool IsFumble => StageResult == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets a value indicating whether this stage was a critical success.
    /// </summary>
    /// <remarks>
    /// Critical success (margin ≥ 5) allows the climber to advance 2 stages
    /// instead of the normal 1 stage.
    /// </remarks>
    public bool IsCriticalSuccess => StageResult == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Creates a new ClimbingStage with the specified result recorded.
    /// </summary>
    /// <param name="result">The skill outcome from attempting this stage.</param>
    /// <returns>A new ClimbingStage with the result and current timestamp.</returns>
    /// <example>
    /// <code>
    /// var stage = context.CreateStage(1);
    /// var completedStage = stage.WithResult(SkillOutcome.FullSuccess);
    /// </code>
    /// </example>
    public ClimbingStage WithResult(SkillOutcome result)
    {
        return this with
        {
            StageResult = result,
            AttemptedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Returns a description of this stage for display purposes.
    /// </summary>
    /// <returns>A formatted description of the stage and its status.</returns>
    /// <example>
    /// Output: "Stage 1 (0ft → 20ft): FullSuccess at 2026-01-22T19:45:00Z"
    /// </example>
    public string ToDescription()
    {
        var baseDesc = $"Stage {StageNumber} ({StartingHeight}ft → {HeightReached}ft)";

        if (!StageResult.HasValue)
        {
            return $"{baseDesc}: Not attempted";
        }

        var resultStr = StageResult.Value.ToString();
        var timeStr = AttemptedAt?.ToString("O") ?? "unknown time";

        return $"{baseDesc}: {resultStr} at {timeStr}";
    }

    /// <inheritdoc/>
    public override string ToString() => ToDescription();
}
