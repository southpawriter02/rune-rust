namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements chase encounter management including obstacle generation,
/// round processing, and chase resolution.
/// </summary>
/// <remarks>
/// <para>
/// The ChaseService maintains a registry of active chases and orchestrates
/// the round-by-round resolution of chase encounters. It integrates with
/// the dice system for skill checks and generates varied obstacles.
/// </para>
/// <para>
/// <b>Distance Change Rules:</b>
/// <list type="bullet">
///   <item><description>Fleeing Success: +1 distance (pulling ahead)</description></item>
///   <item><description>Fleeing Critical: +2 distance (major lead)</description></item>
///   <item><description>Fleeing Failure: -1 distance (stumble)</description></item>
///   <item><description>Pursuer Success: -1 distance (closing gap)</description></item>
///   <item><description>Pursuer Critical: -2 distance (surge)</description></item>
///   <item><description>Pursuer Failure: +1 distance (falls behind)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2e:</b> Initial implementation of chase service.
/// </para>
/// </remarks>
public sealed class ChaseService : IChaseService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Margin required for a critical success in chase obstacles.
    /// </summary>
    private const int CriticalSuccessMargin = 5;

    /// <summary>
    /// Base distance change for success.
    /// </summary>
    private const int SuccessDistanceChange = 1;

    /// <summary>
    /// Distance change for critical success.
    /// </summary>
    private const int CriticalDistanceChange = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IDiceService _diceService;
    private readonly ILogger<ChaseService> _logger;

    private readonly Dictionary<string, ChaseState> _activeChases = new();
    private readonly Dictionary<string, ChaseObstacle> _currentObstacles = new();
    private int _nextChaseId = 1;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="ChaseService"/> class.
    /// </summary>
    /// <param name="diceService">Service for performing dice rolls.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public ChaseService(
        IDiceService diceService,
        ILogger<ChaseService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("ChaseService initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public ChaseState StartChase(
        string fleeingId,
        string pursuerId,
        int startDistance = ChaseState.DefaultStartDistance,
        int? maxRounds = null)
    {
        var chaseId = $"chase-{_nextChaseId++:D4}";

        var chase = ChaseState.Create(
            chaseId,
            fleeingId,
            pursuerId,
            startDistance,
            maxRounds);

        _activeChases[chaseId] = chase;

        _logger.LogInformation(
            "Chase {ChaseId} started: {FleeingId} fleeing from {PursuerId}, " +
            "starting distance {Distance}, max rounds {MaxRounds}",
            chaseId, fleeingId, pursuerId, startDistance, maxRounds ?? -1);

        return chase;
    }

    /// <inheritdoc/>
    public ChaseObstacle GenerateObstacle(string chaseId, string? environmentTag = null)
    {
        if (!_activeChases.TryGetValue(chaseId, out var chase))
        {
            throw new InvalidOperationException($"Chase {chaseId} not found.");
        }

        if (chase.Status != ChaseStatus.InProgress)
        {
            throw new InvalidOperationException($"Chase {chaseId} is not in progress.");
        }

        // Select obstacle avoiding recent repetition
        var obstacle = SelectObstacle(chase.ObstacleHistory, environmentTag);

        chase.AddObstacle(obstacle);
        _currentObstacles[chaseId] = obstacle;

        _logger.LogDebug(
            "Chase {ChaseId} round {Round}: Generated {ObstacleType} obstacle (DC {Dc})",
            chaseId, chase.RoundNumber + 1, obstacle.ObstacleType, obstacle.Dc);

        return obstacle;
    }

    /// <inheritdoc/>
    public ChaseRoundResult ProcessRound(
        string chaseId,
        int fleeingDicePool,
        int pursuerDicePool)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fleeingDicePool, nameof(fleeingDicePool));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pursuerDicePool, nameof(pursuerDicePool));

        if (!_activeChases.TryGetValue(chaseId, out var chase))
        {
            throw new InvalidOperationException($"Chase {chaseId} not found.");
        }

        if (chase.Status != ChaseStatus.InProgress)
        {
            throw new InvalidOperationException($"Chase {chaseId} is not in progress.");
        }

        // Get the current obstacle (most recently added)
        if (!_currentObstacles.TryGetValue(chaseId, out var obstacle))
        {
            throw new InvalidOperationException(
                $"No obstacle generated for chase {chaseId}. Call GenerateObstacle first.");
        }

        _logger.LogDebug(
            "Processing chase {ChaseId} round {Round}: fleeing pool {FleePool}, pursuer pool {PursuerPool}",
            chaseId, chase.RoundNumber + 1, fleeingDicePool, pursuerDicePool);

        // Process fleeing character's attempt
        var (fleeingSuccesses, fleeingOutcome, fleeingDistanceChange) =
            ProcessObstacleAttempt(fleeingDicePool, obstacle.Dc, isFleeingCharacter: true);

        // Process pursuer's attempt
        var (pursuerSuccesses, pursuerOutcome, pursuerDistanceChange) =
            ProcessObstacleAttempt(pursuerDicePool, obstacle.Dc, isFleeingCharacter: false);

        // Calculate new distance
        var previousDistance = chase.Distance;
        var totalChange = fleeingDistanceChange + pursuerDistanceChange;
        var newDistance = Math.Clamp(previousDistance + totalChange, 0, 10);

        // Determine status after this round
        var newStatus = DetermineStatus(chase, newDistance);

        // Create round result
        var roundResult = new ChaseRoundResult(
            RoundNumber: chase.RoundNumber + 1,
            Obstacle: obstacle,
            FleeingNetSuccesses: fleeingSuccesses,
            FleeingOutcome: fleeingOutcome,
            FleeingDistanceChange: fleeingDistanceChange,
            PursuerNetSuccesses: pursuerSuccesses,
            PursuerOutcome: pursuerOutcome,
            PursuerDistanceChange: pursuerDistanceChange,
            PreviousDistance: previousDistance,
            NewDistance: newDistance,
            ChaseStatus: newStatus);

        // Update chase state
        chase.ProcessRoundResult(roundResult);

        // Clear current obstacle
        _currentObstacles.Remove(chaseId);

        // Log round result
        LogRoundResult(chase, roundResult);

        // Log chase end if applicable
        if (chase.Status != ChaseStatus.InProgress)
        {
            _logger.LogInformation(
                "Chase {ChaseId} ended with status {Status} after {Rounds} rounds",
                chaseId, chase.Status, chase.RoundNumber);
        }

        return roundResult;
    }

    /// <inheritdoc/>
    public void AbandonChase(string chaseId, string byCharacterId)
    {
        if (!_activeChases.TryGetValue(chaseId, out var chase))
        {
            throw new InvalidOperationException($"Chase {chaseId} not found.");
        }

        chase.Abandon(byCharacterId);

        _logger.LogInformation(
            "Chase {ChaseId} abandoned by {CharacterId} after {Rounds} rounds",
            chaseId, byCharacterId, chase.RoundNumber);
    }

    /// <inheritdoc/>
    public ChaseState? GetChase(string chaseId)
    {
        return _activeChases.TryGetValue(chaseId, out var chase) ? chase : null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ChaseState> GetActiveChases()
    {
        return _activeChases.Values
            .Where(c => c.Status == ChaseStatus.InProgress)
            .ToList()
            .AsReadOnly();
    }

    /// <inheritdoc/>
    public string? GetCurrentZone(string chaseId)
    {
        return _activeChases.TryGetValue(chaseId, out var chase)
            ? chase.GetCurrentZone()
            : null;
    }

    /// <inheritdoc/>
    public int CalculateDistanceChange(SkillOutcome outcome, bool isFleeingCharacter)
    {
        var baseChange = outcome switch
        {
            SkillOutcome.CriticalSuccess => CriticalDistanceChange,
            SkillOutcome.ExceptionalSuccess => SuccessDistanceChange,
            SkillOutcome.FullSuccess => SuccessDistanceChange,
            SkillOutcome.MarginalSuccess => SuccessDistanceChange,
            SkillOutcome.Failure => -SuccessDistanceChange,
            SkillOutcome.CriticalFailure => -CriticalDistanceChange,
            _ => 0
        };

        // For fleeing: success = +distance (getting away)
        // For pursuer: success = -distance (catching up)
        return isFleeingCharacter ? baseChange : -baseChange;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes a single character's obstacle attempt.
    /// </summary>
    private (int NetSuccesses, SkillOutcome Outcome, int DistanceChange) ProcessObstacleAttempt(
        int dicePool,
        int dc,
        bool isFleeingCharacter)
    {
        // Build and roll dice pool
        var pool = DicePool.D10(dicePool);
        var roll = _diceService.Roll(pool, context: RollContexts.Skill("Acrobatics"));

        // Calculate margin
        var margin = roll.NetSuccesses - dc;

        // Classify outcome
        var outcome = ClassifyOutcome(margin, roll);

        // Calculate distance change
        var distanceChange = CalculateDistanceChange(outcome, isFleeingCharacter);

        _logger.LogDebug(
            "Obstacle attempt: pool {Pool}, successes {Successes}, DC {Dc}, " +
            "margin {Margin}, outcome {Outcome}, distance change {Change}",
            dicePool, roll.NetSuccesses, dc, margin, outcome, distanceChange);

        return (roll.NetSuccesses, outcome, distanceChange);
    }

    /// <summary>
    /// Classifies the skill outcome based on margin and roll properties.
    /// </summary>
    private static SkillOutcome ClassifyOutcome(int margin, DiceRollResult roll)
    {
        if (roll.IsFumble)
        {
            return SkillOutcome.CriticalFailure;
        }

        return margin switch
        {
            >= CriticalSuccessMargin => SkillOutcome.CriticalSuccess,
            >= 3 => SkillOutcome.ExceptionalSuccess,
            >= 1 => SkillOutcome.FullSuccess,
            0 => SkillOutcome.MarginalSuccess,
            _ => SkillOutcome.Failure
        };
    }

    /// <summary>
    /// Determines the chase status based on new distance.
    /// </summary>
    private static ChaseStatus DetermineStatus(ChaseState chase, int newDistance)
    {
        if (newDistance <= ChaseState.CaughtThreshold)
        {
            return ChaseStatus.Caught;
        }

        if (newDistance >= ChaseState.EscapedThreshold)
        {
            return ChaseStatus.Escaped;
        }

        if (chase.MaxRounds.HasValue && chase.RoundNumber + 1 >= chase.MaxRounds.Value)
        {
            return ChaseStatus.TimedOut;
        }

        return ChaseStatus.InProgress;
    }

    /// <summary>
    /// Selects an obstacle avoiding recent repetition.
    /// </summary>
    private ChaseObstacle SelectObstacle(
        IReadOnlyList<ChaseObstacle> history,
        string? environmentTag)
    {
        // Get all available obstacle types
        var allTypes = Enum.GetValues<ObstacleType>();

        // Avoid repeating the last 2 obstacle types
        var recentTypes = history
            .TakeLast(2)
            .Select(o => o.ObstacleType)
            .ToHashSet();

        var candidates = allTypes
            .Where(t => !recentTypes.Contains(t))
            .ToList();

        if (candidates.Count == 0)
        {
            candidates = allTypes.ToList();
        }

        // Random selection from candidates
        var selectedType = candidates[Random.Shared.Next(candidates.Count)];

        // Generate obstacle with random DC (2-4)
        var dc = Random.Shared.Next(2, 5);

        _logger.LogDebug(
            "Obstacle selection: available types {Available}, selected {Type} (DC {Dc})",
            candidates.Count, selectedType, dc);

        return selectedType switch
        {
            ObstacleType.Gap => ChaseObstacle.CreateGap(dc),
            ObstacleType.Climb => ChaseObstacle.CreateClimb(dc),
            ObstacleType.Debris => ChaseObstacle.CreateDebris(dc),
            ObstacleType.Crowd => ChaseObstacle.CreateCrowd(dc),
            ObstacleType.Hazard => ChaseObstacle.CreateHazard(dc),
            _ => ChaseObstacle.CreateDebris(dc)
        };
    }

    /// <summary>
    /// Logs the round result with appropriate detail.
    /// </summary>
    private void LogRoundResult(ChaseState chase, ChaseRoundResult result)
    {
        _logger.LogInformation(
            "Chase {ChaseId} round {Round}: " +
            "Fleeing {FleeOutcome} ({FleeChange:+#;-#;0}), " +
            "Pursuer {PursuerOutcome} ({PursuerChange:+#;-#;0}), " +
            "Distance {PrevDist}→{NewDist} ({Zone})",
            chase.ChaseId,
            result.RoundNumber,
            result.FleeingOutcome,
            result.FleeingDistanceChange,
            result.PursuerOutcome,
            result.PursuerDistanceChange,
            result.PreviousDistance,
            result.NewDistance,
            chase.GetCurrentZone());
    }
}
