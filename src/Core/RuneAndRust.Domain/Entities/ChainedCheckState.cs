using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Tracks the state of a multi-step chained skill check in progress.
/// </summary>
/// <remarks>
/// <para>
/// Chained checks represent multi-stage procedures where each step must succeed
/// before the next is attempted. Examples include:
/// <list type="bullet">
///   <item><description>Terminal hacking: Access → Authentication → Navigation</description></item>
///   <item><description>Trap disarmament: Detection → Analysis → Disarmament</description></item>
///   <item><description>Tracking pursuit: Acquisition → Pursuit → Closing</description></item>
/// </list>
/// </para>
/// <para>
/// The state tracks progress through steps, retry attempts, and individual results.
/// </para>
/// </remarks>
public sealed class ChainedCheckState
{
    /// <summary>
    /// Unique identifier for this chained check instance.
    /// </summary>
    public string CheckId { get; private set; } = string.Empty;

    /// <summary>
    /// The character performing the chained check.
    /// </summary>
    public string CharacterId { get; private set; } = string.Empty;

    /// <summary>
    /// Display name for this chain (e.g., "Terminal Hacking", "Trap Disarmament").
    /// </summary>
    public string ChainName { get; private set; } = string.Empty;

    /// <summary>
    /// The steps in this chained check, in order.
    /// </summary>
    public IReadOnlyList<ChainedCheckStep> Steps { get; private set; } = Array.Empty<ChainedCheckStep>();

    /// <summary>
    /// Index of the current step (0-based).
    /// </summary>
    /// <remarks>
    /// When equal to <see cref="Steps"/>.Count, all steps have been completed.
    /// </remarks>
    public int CurrentStepIndex { get; private set; }

    /// <summary>
    /// Results from each completed step.
    /// </summary>
    public List<ChainedStepResult> StepResults { get; private set; } = new();

    /// <summary>
    /// Remaining retry attempts for each step.
    /// </summary>
    /// <remarks>
    /// Indexed by step index. Decremented each time a step is retried.
    /// </remarks>
    public List<int> RetriesRemaining { get; private set; } = new();

    /// <summary>
    /// Current status of the chain.
    /// </summary>
    public ChainedCheckStatus Status { get; private set; } = ChainedCheckStatus.NotStarted;

    /// <summary>
    /// When the chain was started.
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// When the chain completed (succeeded or failed).
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Optional context data for tracking (e.g., target terminal ID).
    /// </summary>
    public string? TargetId { get; private set; }

    // Private constructor for deserialization
    private ChainedCheckState() { }

    /// <summary>
    /// Creates a new chained check state.
    /// </summary>
    /// <param name="checkId">Unique identifier for this chain.</param>
    /// <param name="characterId">Character performing the chain.</param>
    /// <param name="chainName">Display name for the chain.</param>
    /// <param name="steps">The steps in order.</param>
    /// <param name="targetId">Optional target identifier.</param>
    /// <returns>A new <see cref="ChainedCheckState"/> ready to process.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are missing.</exception>
    public static ChainedCheckState Create(
        string checkId,
        string characterId,
        string chainName,
        IReadOnlyList<ChainedCheckStep> steps,
        string? targetId = null)
    {
        if (string.IsNullOrWhiteSpace(checkId))
            throw new ArgumentException("Check ID is required.", nameof(checkId));
        if (string.IsNullOrWhiteSpace(characterId))
            throw new ArgumentException("Character ID is required.", nameof(characterId));
        if (steps == null || steps.Count == 0)
            throw new ArgumentException("At least one step is required.", nameof(steps));

        return new ChainedCheckState
        {
            CheckId = checkId,
            CharacterId = characterId,
            ChainName = chainName,
            Steps = steps.ToList(),
            CurrentStepIndex = 0,
            StepResults = new List<ChainedStepResult>(),
            RetriesRemaining = steps.Select(s => s.MaxRetries).ToList(),
            Status = ChainedCheckStatus.NotStarted,
            StartedAt = DateTime.UtcNow,
            TargetId = targetId
        };
    }

    /// <summary>
    /// Gets the current step to process.
    /// </summary>
    /// <returns>The current step, or null if chain is complete.</returns>
    public ChainedCheckStep? GetCurrentStep()
    {
        if (CurrentStepIndex >= Steps.Count)
            return null;

        return Steps[CurrentStepIndex];
    }

    /// <summary>
    /// Records the result of a step attempt and advances state.
    /// </summary>
    /// <param name="result">The skill check result for this step.</param>
    /// <param name="wasRetry">Whether this was a retry attempt.</param>
    public void RecordStepResult(SkillCheckResult result, bool wasRetry = false)
    {
        var step = Steps[CurrentStepIndex];
        var stepResult = new ChainedStepResult(
            StepId: step.StepId,
            StepIndex: CurrentStepIndex,
            CheckResult: result,
            AttemptNumber: wasRetry ? step.MaxRetries - RetriesRemaining[CurrentStepIndex] + 1 : 1,
            WasRetry: wasRetry);

        StepResults.Add(stepResult);

        if (result.Outcome >= SkillOutcome.MarginalSuccess)
        {
            // Step succeeded - advance to next
            CurrentStepIndex++;
            Status = CurrentStepIndex >= Steps.Count
                ? ChainedCheckStatus.Succeeded
                : ChainedCheckStatus.InProgress;

            if (Status == ChainedCheckStatus.Succeeded)
            {
                CompletedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Step failed
            if (RetriesRemaining[CurrentStepIndex] > 0)
            {
                RetriesRemaining[CurrentStepIndex]--;
                Status = ChainedCheckStatus.AwaitingRetry;
            }
            else
            {
                Status = ChainedCheckStatus.Failed;
                CompletedAt = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Checks if a retry is available for the current step.
    /// </summary>
    /// <returns>True if retries remain and status is AwaitingRetry.</returns>
    public bool CanRetry()
    {
        return Status == ChainedCheckStatus.AwaitingRetry &&
               CurrentStepIndex < Steps.Count &&
               RetriesRemaining[CurrentStepIndex] > 0;
    }

    /// <summary>
    /// Abandons the chain, marking it as failed.
    /// </summary>
    public void Abandon()
    {
        if (Status == ChainedCheckStatus.Succeeded || Status == ChainedCheckStatus.Failed)
            return;

        Status = ChainedCheckStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the total number of attempts across all steps.
    /// </summary>
    public int TotalAttempts => StepResults.Count;

    /// <summary>
    /// Gets the number of successful steps.
    /// </summary>
    public int SuccessfulSteps => StepResults
        .Where(r => r.CheckResult.Outcome >= SkillOutcome.MarginalSuccess)
        .GroupBy(r => r.StepIndex)
        .Count();

    /// <summary>
    /// Checks if the chain is in a terminal state.
    /// </summary>
    public bool IsComplete => Status == ChainedCheckStatus.Succeeded ||
                              Status == ChainedCheckStatus.Failed;

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    public override string ToString() =>
        $"[{CheckId}] {ChainName}: Step {CurrentStepIndex + 1}/{Steps.Count} ({Status})";
}

/// <summary>
/// Records the result of a single step attempt within a chain.
/// </summary>
/// <param name="StepId">The step's identifier.</param>
/// <param name="StepIndex">Index of the step in the chain.</param>
/// <param name="CheckResult">The skill check result.</param>
/// <param name="AttemptNumber">Which attempt this was (1 = first try).</param>
/// <param name="WasRetry">Whether this was a retry attempt.</param>
public readonly record struct ChainedStepResult(
    string StepId,
    int StepIndex,
    SkillCheckResult CheckResult,
    int AttemptNumber,
    bool WasRetry);
