using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using System.Text;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing puzzle interactions and validation.
/// </summary>
/// <remarks>
/// <para>
/// PuzzleService provides the core puzzle infrastructure including:
/// </para>
/// <list type="bullet">
///   <item><description>Attempt tracking via in-memory dictionary</description></item>
///   <item><description>State validation and transitions</description></item>
///   <item><description>Placeholder solution validation (extended in v0.4.2b)</description></item>
///   <item><description>Reset processing and prerequisite management</description></item>
/// </list>
/// </remarks>
public class PuzzleService : IPuzzleService
{
    private readonly ILogger<PuzzleService> _logger;
    private readonly Dictionary<(Guid PlayerId, Guid PuzzleId), PuzzleAttempt> _activeAttempts = [];

    /// <summary>
    /// Initializes a new instance of the PuzzleService.
    /// </summary>
    /// <param name="logger">Logger for puzzle operations.</param>
    public PuzzleService(ILogger<PuzzleService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("PuzzleService initialized");
    }

    // ===== Query Methods =====

    /// <inheritdoc/>
    public Puzzle? GetPuzzle(Room room, string puzzleKeyword)
    {
        ArgumentNullException.ThrowIfNull(room);

        var puzzle = room.GetPuzzleByKeyword(puzzleKeyword);

        _logger.LogDebug(
            "GetPuzzle: Room={RoomName}, Keyword={Keyword}, Found={Found}",
            room.Name, puzzleKeyword, puzzle != null);

        return puzzle;
    }

    /// <inheritdoc/>
    public IEnumerable<Puzzle> GetPuzzles(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        return room.Puzzles;
    }

    /// <inheritdoc/>
    public IEnumerable<Puzzle> GetUnsolvedPuzzles(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        return room.GetUnsolvedPuzzles();
    }

    // ===== Attempt Methods =====

    /// <inheritdoc/>
    public PuzzleAttemptResult BeginAttempt(Player player, Puzzle puzzle)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(puzzle);

        // Check if already solved
        if (puzzle.IsSolved)
        {
            _logger.LogDebug(
                "BeginAttempt: Player {PlayerId} attempted already-solved puzzle {PuzzleId}",
                player.Id, puzzle.Id);
            return PuzzleAttemptResult.AlreadySolved(puzzle);
        }

        // Check if locked
        if (puzzle.IsLocked)
        {
            _logger.LogDebug(
                "BeginAttempt: Player {PlayerId} attempted locked puzzle {PuzzleId}",
                player.Id, puzzle.Id);
            return PuzzleAttemptResult.PuzzleLocked(puzzle);
        }

        // Check attempts remaining
        if (!puzzle.HasAttemptsRemaining)
        {
            _logger.LogDebug(
                "BeginAttempt: Player {PlayerId} has no attempts remaining for puzzle {PuzzleId}",
                player.Id, puzzle.Id);
            return PuzzleAttemptResult.NoAttemptsRemaining(puzzle);
        }

        // Check for existing attempt
        var key = (player.Id, puzzle.Id);
        if (_activeAttempts.TryGetValue(key, out var existingAttempt) && existingAttempt.IsActive)
        {
            _logger.LogDebug(
                "BeginAttempt: Player {PlayerId} resuming existing attempt on puzzle {PuzzleId}",
                player.Id, puzzle.Id);
            return PuzzleAttemptResult.AlreadyInProgress(puzzle, existingAttempt);
        }

        // Start new attempt
        puzzle.BeginAttempt();
        var attempt = PuzzleAttempt.Create(puzzle.Id);
        _activeAttempts[key] = attempt;

        _logger.LogInformation(
            "BeginAttempt: Player {PlayerId} began attempt on puzzle {PuzzleId} ({PuzzleName})",
            player.Id, puzzle.Id, puzzle.Name);

        return PuzzleAttemptResult.Started(puzzle, attempt);
    }

    /// <inheritdoc/>
    public PuzzleSolveResult AttemptSolve(Player player, Puzzle puzzle, string input)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(puzzle);

        if (!puzzle.IsSolvable)
        {
            _logger.LogDebug(
                "AttemptSolve: Puzzle {PuzzleId} not solvable (State={State})",
                puzzle.Id, puzzle.State);
            return PuzzleSolveResult.NotSolvable(puzzle);
        }

        _logger.LogInformation(
            "AttemptSolve: Player {PlayerId} attempting puzzle {PuzzleId} with input length {InputLength}",
            player.Id, puzzle.Id, input?.Length ?? 0);

        // Type-specific validation will be implemented in v0.4.2b
        // For now, this is a placeholder that always fails
        // TODO: Implement type-specific validators for Sequence, Combination, Pattern, Riddle, Logic
        puzzle.RecordFailedAttempt();

        if (puzzle.IsFailed)
        {
            _logger.LogInformation(
                "AttemptSolve: Player {PlayerId} failed puzzle {PuzzleId} - max attempts reached",
                player.Id, puzzle.Id);
            return PuzzleSolveResult.Failure(puzzle);
        }

        var remaining = puzzle.MaxAttempts < 0 ? -1 : puzzle.MaxAttempts - puzzle.AttemptCount;

        _logger.LogDebug(
            "AttemptSolve: Incorrect solution for puzzle {PuzzleId}, {Remaining} attempts remaining",
            puzzle.Id, remaining < 0 ? "unlimited" : remaining.ToString());

        return PuzzleSolveResult.Incorrect(puzzle, remaining);
    }

    /// <inheritdoc/>
    public PuzzleStepResult ValidateStep(Puzzle puzzle, PuzzleAttempt attempt, string stepId)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        ArgumentNullException.ThrowIfNull(attempt);

        // Implementation deferred to v0.4.2b (SequencePuzzle)
        _logger.LogDebug(
            "ValidateStep: Puzzle {PuzzleId}, Step {StepId} (implementation deferred to v0.4.2b)",
            puzzle.Id, stepId);

        return PuzzleStepResult.WrongStep(puzzle, stepId, false);
    }

    /// <inheritdoc/>
    public PuzzleAttempt? GetCurrentAttempt(Player player, Puzzle puzzle)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(puzzle);

        var key = (player.Id, puzzle.Id);
        return _activeAttempts.TryGetValue(key, out var attempt) && attempt.IsActive ? attempt : null;
    }

    // ===== Hint Methods =====

    /// <inheritdoc/>
    public PuzzleHintResult RequestHint(Player player, Puzzle puzzle)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(puzzle);

        // Hint system deferred to v0.4.2c
        if (!puzzle.HasHints)
        {
            _logger.LogDebug(
                "RequestHint: No hints available for puzzle {PuzzleId}",
                puzzle.Id);
            return PuzzleHintResult.NoHintsAvailable(puzzle);
        }

        _logger.LogDebug(
            "RequestHint: Hint requested for puzzle {PuzzleId} (implementation deferred to v0.4.2c)",
            puzzle.Id);

        return PuzzleHintResult.NoHintsAvailable(puzzle);
    }

    // ===== Processing Methods =====

    /// <inheritdoc/>
    public IEnumerable<Puzzle> ProcessPuzzleResets(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var resetPuzzles = new List<Puzzle>();

        foreach (var puzzle in room.Puzzles.Where(p => p.IsFailed && p.TurnsUntilReset.HasValue))
        {
            if (puzzle.TickReset())
            {
                resetPuzzles.Add(puzzle);
                _logger.LogDebug(
                    "ProcessPuzzleResets: Puzzle {PuzzleId} ({PuzzleName}) reset after timeout",
                    puzzle.Id, puzzle.Name);
            }
        }

        if (resetPuzzles.Count > 0)
        {
            _logger.LogInformation(
                "ProcessPuzzleResets: {Count} puzzle(s) reset in room {RoomName}",
                resetPuzzles.Count, room.Name);
        }

        return resetPuzzles;
    }

    /// <inheritdoc/>
    public void UpdatePuzzleLocks(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var solvedIds = room.Puzzles
            .Where(p => p.IsSolved)
            .Select(p => p.DefinitionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var puzzle in room.Puzzles.Where(p => p.HasPrerequisite && p.IsLocked))
        {
            if (solvedIds.Contains(puzzle.PrerequisitePuzzleId!))
            {
                puzzle.Unlock();
                _logger.LogInformation(
                    "UpdatePuzzleLocks: Puzzle {PuzzleId} ({PuzzleName}) unlocked - prerequisite solved",
                    puzzle.Id, puzzle.Name);
            }
        }

        // Lock puzzles with unsolved prerequisites
        foreach (var puzzle in room.Puzzles.Where(p => p.HasPrerequisite && p.State == PuzzleState.Unsolved))
        {
            if (!solvedIds.Contains(puzzle.PrerequisitePuzzleId!))
            {
                puzzle.Lock();
                _logger.LogDebug(
                    "UpdatePuzzleLocks: Puzzle {PuzzleId} ({PuzzleName}) locked - prerequisite not solved",
                    puzzle.Id, puzzle.Name);
            }
        }
    }

    /// <inheritdoc/>
    public PuzzleRewardResult ApplyPuzzleReward(Player player, Puzzle puzzle)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(puzzle);

        if (!puzzle.IsSolved)
        {
            _logger.LogDebug(
                "ApplyPuzzleReward: Puzzle {PuzzleId} not solved, cannot apply reward",
                puzzle.Id);
            return PuzzleRewardResult.NotSolved(puzzle);
        }

        if (!puzzle.HasReward)
        {
            _logger.LogDebug(
                "ApplyPuzzleReward: Puzzle {PuzzleId} has no reward",
                puzzle.Id);
            return PuzzleRewardResult.NoReward(puzzle);
        }

        // Reward application deferred to v0.4.2c integration with LootService
        _logger.LogInformation(
            "ApplyPuzzleReward: Player {PlayerId} granted reward {RewardId} for puzzle {PuzzleId}",
            player.Id, puzzle.RewardId, puzzle.Id);

        return PuzzleRewardResult.RewardGranted(puzzle, puzzle.RewardId!);
    }

    // ===== Description Methods =====

    /// <inheritdoc/>
    public string GetRoomPuzzlesDescription(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var puzzles = room.Puzzles.ToList();
        if (puzzles.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("[PUZZLES]");

        foreach (var puzzle in puzzles)
        {
            var statusIcon = puzzle.State switch
            {
                PuzzleState.Solved => "✓",
                PuzzleState.Failed => "✗",
                PuzzleState.Locked => "🔒",
                PuzzleState.InProgress => "...",
                _ => "○"
            };

            sb.AppendLine($"  {statusIcon} {puzzle.Name} ({puzzle.Type})");
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string ExaminePuzzle(Puzzle puzzle)
    {
        ArgumentNullException.ThrowIfNull(puzzle);

        var sb = new StringBuilder();

        // Header
        sb.AppendLine(puzzle.Name);
        sb.AppendLine(new string('=', puzzle.Name.Length));
        sb.AppendLine(puzzle.Description);
        sb.AppendLine();

        // Status
        sb.AppendLine($"Type: {puzzle.Type}");
        sb.AppendLine($"Status: {puzzle.State}");

        // Difficulty stars
        var stars = new string('★', puzzle.Difficulty) + new string('☆', 5 - puzzle.Difficulty);
        sb.AppendLine($"Difficulty: {stars}");

        // Attempts
        if (puzzle.MaxAttempts > 0)
        {
            sb.AppendLine($"Attempts: {puzzle.AttemptCount}/{puzzle.MaxAttempts}");
        }
        else
        {
            sb.AppendLine("Attempts: Unlimited");
        }

        // Reset info
        if (puzzle.IsFailed && puzzle.TurnsUntilReset.HasValue)
        {
            sb.AppendLine($"Resets in: {puzzle.TurnsUntilReset} turns");
        }

        // Hints
        if (puzzle.HasHints)
        {
            sb.AppendLine($"Hints: {puzzle.HintsRevealed} revealed");
        }

        // Locked message
        if (puzzle.IsLocked)
        {
            sb.AppendLine();
            sb.AppendLine("This puzzle requires solving another puzzle first.");
        }

        return sb.ToString();
    }
}
