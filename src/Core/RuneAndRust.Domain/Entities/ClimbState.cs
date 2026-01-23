using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Tracks the ongoing state of a multi-stage climbing attempt.
/// </summary>
/// <remarks>
/// <para>
/// ClimbState manages the lifecycle of a climbing attempt from initiation through
/// completion, abandonment, or failure. The entity maintains history of all stage
/// attempts for audit and narrative purposes.
/// </para>
/// <para>
/// State Lifecycle:
/// <list type="bullet">
///   <item><description>StartClimb → Creates state with InProgress status</description></item>
///   <item><description>RecordStageAttempt → Updates based on stage result</description></item>
///   <item><description>Abandon → Sets Abandoned status and resets to ground</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ClimbState : IEntity
{
    private readonly List<ClimbingStage> _stageHistory = [];

    /// <summary>
    /// Gets the unique identifier for this climb attempt.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets alias for Id to maintain climbing-specific semantics.
    /// </summary>
    public Guid ClimbId => Id;

    /// <summary>
    /// Gets the identifier of the character performing the climb.
    /// </summary>
    public string CharacterId { get; private set; }

    /// <summary>
    /// Gets the climbing context defining the climb parameters.
    /// </summary>
    public ClimbContext Context { get; private set; }

    /// <summary>
    /// Gets the current stage number (0 = ground, 1-3 = climbing stages).
    /// </summary>
    public int CurrentStage { get; private set; }

    /// <summary>
    /// Gets the history of all stage attempts.
    /// </summary>
    public IReadOnlyList<ClimbingStage> StageHistory => _stageHistory.AsReadOnly();

    /// <summary>
    /// Gets the current status of the climb.
    /// </summary>
    public ClimbStatus Status { get; private set; }

    /// <summary>
    /// Gets the timestamp when the climb was started.
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the climb ended, if applicable.
    /// </summary>
    public DateTime? EndedAt { get; private set; }

    /// <summary>
    /// Gets the current height reached in feet.
    /// </summary>
    public int CurrentHeight => CurrentStage switch
    {
        0 => 0,
        1 => 20,
        2 => 40,
        3 => Context.TotalHeight,
        _ => 0
    };

    /// <summary>
    /// Gets a value indicating whether the climb is currently in progress.
    /// </summary>
    public bool IsInProgress => Status == ClimbStatus.InProgress;

    /// <summary>
    /// Gets a value indicating whether the climb has ended (any terminal state).
    /// </summary>
    public bool HasEnded => Status != ClimbStatus.InProgress;

    /// <summary>
    /// Private constructor for EF Core and factory method usage.
    /// </summary>
    private ClimbState()
    {
        CharacterId = null!;
    }

    /// <summary>
    /// Starts a new climbing attempt.
    /// </summary>
    /// <param name="characterId">The climbing character's identifier.</param>
    /// <param name="context">The climbing context with all parameters.</param>
    /// <returns>A new ClimbState in InProgress status at stage 0.</returns>
    /// <exception cref="ArgumentException">Thrown if characterId is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown if context has 0 stages required.</exception>
    public static ClimbState StartClimb(string characterId, ClimbContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);

        if (context.StagesRequired <= 0)
        {
            throw new ArgumentException(
                "Cannot start climb with 0 or fewer stages required.",
                nameof(context));
        }

        return new ClimbState
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            Context = context,
            CurrentStage = 0,
            Status = ClimbStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Records a stage attempt result and updates the climb state accordingly.
    /// </summary>
    /// <param name="result">The result of the stage attempt.</param>
    /// <exception cref="InvalidOperationException">Thrown if climb is not in progress.</exception>
    /// <remarks>
    /// This method updates:
    /// <list type="bullet">
    ///   <item><description>CurrentStage based on result.NewStage</description></item>
    ///   <item><description>Status based on result.ClimbStatus</description></item>
    ///   <item><description>StageHistory with the attempted stage</description></item>
    ///   <item><description>EndedAt if climb has ended</description></item>
    /// </list>
    /// </remarks>
    public void RecordStageAttempt(ClimbStageResult result)
    {
        if (!IsInProgress)
        {
            throw new InvalidOperationException(
                $"Cannot record stage attempt for climb with status {Status}.");
        }

        // Record the attempted stage in history
        _stageHistory.Add(result.StageAttempted);

        // Update current stage
        CurrentStage = result.NewStage;

        // Update status
        Status = result.ClimbStatus;

        // Mark end time if climb has concluded
        if (HasEnded)
        {
            EndedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Abandons the current climb, returning safely to ground.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if climb is not in progress.</exception>
    /// <remarks>
    /// Abandoning a climb:
    /// <list type="bullet">
    ///   <item><description>Sets status to Abandoned</description></item>
    ///   <item><description>Resets current stage to 0</description></item>
    ///   <item><description>Records end timestamp</description></item>
    ///   <item><description>Does NOT cause fall damage</description></item>
    /// </list>
    /// </remarks>
    public void Abandon()
    {
        if (!IsInProgress)
        {
            throw new InvalidOperationException(
                $"Cannot abandon climb with status {Status}.");
        }

        Status = ClimbStatus.Abandoned;
        CurrentStage = 0;
        EndedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the next stage to attempt.
    /// </summary>
    /// <returns>The next ClimbingStage, or null if climb is not in progress.</returns>
    /// <remarks>
    /// Returns the stage for CurrentStage + 1 (the next stage to climb).
    /// Returns null if the climb has ended or CurrentStage >= StagesRequired.
    /// </remarks>
    public ClimbingStage? GetNextStage()
    {
        if (!IsInProgress)
        {
            return null;
        }

        var nextStageNumber = CurrentStage + 1;
        if (nextStageNumber > Context.StagesRequired)
        {
            return null;
        }

        return Context.CreateStage(nextStageNumber);
    }

    /// <summary>
    /// Returns a summary of the current climb state.
    /// </summary>
    /// <returns>Formatted climb summary for display.</returns>
    public string ToSummary()
    {
        var statusStr = Status.ToString();
        var heightStr = $"{CurrentHeight}ft of {Context.TotalHeight}ft";
        var stageStr = $"Stage {CurrentStage}/{Context.StagesRequired}";

        var summary = $"Climb [{statusStr}]: {stageStr} ({heightStr})";

        if (HasEnded && EndedAt.HasValue)
        {
            var duration = EndedAt.Value - StartedAt;
            summary += $" - Ended after {duration.TotalSeconds:F1}s";
        }

        return summary;
    }

    /// <inheritdoc/>
    public override string ToString() => ToSummary();
}
