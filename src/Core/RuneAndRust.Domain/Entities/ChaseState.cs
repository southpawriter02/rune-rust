namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the complete state of an active or completed chase encounter.
/// </summary>
/// <remarks>
/// <para>
/// ChaseState is an entity that tracks all aspects of a chase: participants,
/// distance, obstacle history, and outcome. It maintains invariants such as
/// distance bounds and proper status transitions. The entity is mutable to
/// allow round-by-round updates during chase resolution.
/// </para>
/// <para>
/// <b>Distance Track:</b>
/// <list type="bullet">
///   <item><description>0 or below: Caught (pursuer catches fleeing character)</description></item>
///   <item><description>1-2: Close (pursuer can attempt capture)</description></item>
///   <item><description>3: Near (default starting position)</description></item>
///   <item><description>4-5: Far (fleeing character is pulling ahead)</description></item>
///   <item><description>6 or above: Escaped (fleeing character escapes)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2e:</b> Initial implementation of chase state tracking.
/// </para>
/// </remarks>
public class ChaseState : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Distance threshold for caught status (0 or below).
    /// </summary>
    public const int CaughtThreshold = 0;

    /// <summary>
    /// Distance threshold for escaped status (6 or above).
    /// </summary>
    public const int EscapedThreshold = 6;

    /// <summary>
    /// Default starting distance for a chase (Near zone).
    /// </summary>
    public const int DefaultStartDistance = 3;

    /// <summary>
    /// Minimum valid start distance.
    /// </summary>
    public const int MinStartDistance = 1;

    /// <summary>
    /// Maximum valid start distance.
    /// </summary>
    public const int MaxStartDistance = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the entity ID (required by IEntity).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique string identifier for this chase encounter.
    /// </summary>
    public string ChaseId { get; private set; }

    /// <summary>
    /// Gets the ID of the character attempting to flee.
    /// </summary>
    public string FleeingId { get; private set; }

    /// <summary>
    /// Gets the ID of the character pursuing.
    /// </summary>
    public string PursuerId { get; private set; }

    /// <summary>
    /// Gets the current distance on the chase track.
    /// </summary>
    /// <remarks>
    /// Distance 0 or below means caught, distance 6 or above means escaped.
    /// Default starting position is 3 (Near zone).
    /// </remarks>
    public int Distance { get; private set; }

    /// <summary>
    /// Gets the history of obstacles encountered during this chase.
    /// </summary>
    public IReadOnlyList<ChaseObstacle> ObstacleHistory => _obstacleHistory.AsReadOnly();
    private readonly List<ChaseObstacle> _obstacleHistory;

    /// <summary>
    /// Gets the history of round results.
    /// </summary>
    public IReadOnlyList<ChaseRoundResult> RoundHistory => _roundHistory.AsReadOnly();
    private readonly List<ChaseRoundResult> _roundHistory;

    /// <summary>
    /// Gets the current round number (0 before first round, 1+ during chase).
    /// </summary>
    public int RoundNumber { get; private set; }

    /// <summary>
    /// Gets the current chase status.
    /// </summary>
    public ChaseStatus Status { get; private set; }

    /// <summary>
    /// Gets the optional maximum round limit for this chase.
    /// </summary>
    /// <remarks>
    /// If set and RoundNumber reaches MaxRounds without resolution,
    /// Status transitions to TimedOut.
    /// </remarks>
    public int? MaxRounds { get; private set; }

    /// <summary>
    /// Gets the ID of the character who abandoned the chase, if applicable.
    /// </summary>
    public string? AbandonedById { get; private set; }

    /// <summary>
    /// Gets when the chase started.
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// Gets when the chase ended, if it has ended.
    /// </summary>
    public DateTime? EndedAt { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private ChaseState()
    {
        ChaseId = string.Empty;
        FleeingId = string.Empty;
        PursuerId = string.Empty;
        _obstacleHistory = [];
        _roundHistory = [];
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new chase encounter.
    /// </summary>
    /// <param name="chaseId">Unique identifier for this chase.</param>
    /// <param name="fleeingId">ID of the fleeing character.</param>
    /// <param name="pursuerId">ID of the pursuing character.</param>
    /// <param name="startDistance">Starting position on distance track (default 3).</param>
    /// <param name="maxRounds">Optional maximum round limit.</param>
    /// <returns>A new ChaseState instance.</returns>
    /// <exception cref="ArgumentException">If IDs are invalid or startDistance out of range.</exception>
    public static ChaseState Create(
        string chaseId,
        string fleeingId,
        string pursuerId,
        int startDistance = DefaultStartDistance,
        int? maxRounds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chaseId, nameof(chaseId));
        ArgumentException.ThrowIfNullOrWhiteSpace(fleeingId, nameof(fleeingId));
        ArgumentException.ThrowIfNullOrWhiteSpace(pursuerId, nameof(pursuerId));

        if (fleeingId == pursuerId)
        {
            throw new ArgumentException(
                "Fleeing and pursuer cannot be the same character.",
                nameof(pursuerId));
        }

        if (startDistance < MinStartDistance || startDistance > MaxStartDistance)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startDistance),
                $"Start distance must be between {MinStartDistance} and {MaxStartDistance}.");
        }

        if (maxRounds.HasValue && maxRounds.Value < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxRounds),
                "Max rounds must be at least 1.");
        }

        return new ChaseState
        {
            Id = Guid.NewGuid(),
            ChaseId = chaseId,
            FleeingId = fleeingId,
            PursuerId = pursuerId,
            Distance = startDistance,
            RoundNumber = 0,
            Status = ChaseStatus.InProgress,
            MaxRounds = maxRounds,
            StartedAt = DateTime.UtcNow
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE MUTATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records an obstacle for the current round.
    /// </summary>
    /// <param name="obstacle">The obstacle to add to history.</param>
    /// <exception cref="InvalidOperationException">If chase is not in progress.</exception>
    public void AddObstacle(ChaseObstacle obstacle)
    {
        if (Status != ChaseStatus.InProgress)
        {
            throw new InvalidOperationException("Cannot add obstacles to a completed chase.");
        }

        _obstacleHistory.Add(obstacle);
    }

    /// <summary>
    /// Processes the results of a chase round and updates state.
    /// </summary>
    /// <param name="roundResult">The complete round result to record.</param>
    /// <exception cref="InvalidOperationException">If chase is not in progress.</exception>
    public void ProcessRoundResult(ChaseRoundResult roundResult)
    {
        if (Status != ChaseStatus.InProgress)
        {
            throw new InvalidOperationException("Cannot process rounds for a completed chase.");
        }

        RoundNumber++;
        Distance = roundResult.NewDistance;
        _roundHistory.Add(roundResult);

        UpdateStatus();
    }

    /// <summary>
    /// Marks the chase as abandoned by the specified character.
    /// </summary>
    /// <param name="characterId">ID of the character abandoning.</param>
    /// <exception cref="InvalidOperationException">If chase is not in progress.</exception>
    /// <exception cref="ArgumentException">If character is not a participant.</exception>
    public void Abandon(string characterId)
    {
        if (Status != ChaseStatus.InProgress)
        {
            throw new InvalidOperationException("Cannot abandon a completed chase.");
        }

        if (characterId != FleeingId && characterId != PursuerId)
        {
            throw new ArgumentException(
                "Only chase participants can abandon.",
                nameof(characterId));
        }

        AbandonedById = characterId;
        Status = ChaseStatus.Abandoned;
        EndedAt = DateTime.UtcNow;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current zone description based on distance.
    /// </summary>
    /// <returns>Zone name (Caught, Close, Near, Far, or Lost).</returns>
    public string GetCurrentZone() => Distance switch
    {
        <= CaughtThreshold => "Caught",
        <= 1 => "Close",
        <= 3 => "Near",
        <= 5 => "Far",
        _ => "Lost"
    };

    /// <summary>
    /// Determines if the pursuer can attempt capture (at distance 0-1).
    /// </summary>
    public bool CanAttemptCapture => Distance <= 1 && Status == ChaseStatus.InProgress;

    /// <summary>
    /// Determines if the fleeing character is close to escape (at distance 5).
    /// </summary>
    public bool NearEscape => Distance == 5 && Status == ChaseStatus.InProgress;

    /// <summary>
    /// Determines if the chase is still active.
    /// </summary>
    public bool IsActive => Status == ChaseStatus.InProgress;

    /// <summary>
    /// Gets the total number of rounds completed.
    /// </summary>
    public int TotalRounds => RoundNumber;

    /// <summary>
    /// Gets the duration of the chase so far.
    /// </summary>
    public TimeSpan Duration => (EndedAt ?? DateTime.UtcNow) - StartedAt;

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Updates the chase status based on current distance and round count.
    /// </summary>
    private void UpdateStatus()
    {
        if (Distance <= CaughtThreshold)
        {
            Status = ChaseStatus.Caught;
            EndedAt = DateTime.UtcNow;
        }
        else if (Distance >= EscapedThreshold)
        {
            Status = ChaseStatus.Escaped;
            EndedAt = DateTime.UtcNow;
        }
        else if (MaxRounds.HasValue && RoundNumber >= MaxRounds.Value)
        {
            Status = ChaseStatus.TimedOut;
            EndedAt = DateTime.UtcNow;
        }
    }
}
