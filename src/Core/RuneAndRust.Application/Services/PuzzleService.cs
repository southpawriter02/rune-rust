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
///   <item><description>Type-specific validation for Sequence, Combination, Pattern puzzles (v0.4.2b)</description></item>
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
            "AttemptSolve: Player {PlayerId} attempting {PuzzleType} puzzle {PuzzleId}",
            player.Id, puzzle.Type, puzzle.Id);

        // Delegate to type-specific validation (v0.4.2b)
        return puzzle.Type switch
        {
            PuzzleType.Combination => ValidateCombination(puzzle, input),
            PuzzleType.Pattern => ValidatePattern(puzzle, input),
            _ => ValidateGeneric(puzzle, input)
        };
    }

    /// <inheritdoc/>
    public PuzzleStepResult ValidateStep(Puzzle puzzle, PuzzleAttempt attempt, string stepId)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        ArgumentNullException.ThrowIfNull(attempt);

        // Delegate to ValidateSequenceStep for sequence puzzles
        return ValidateSequenceStep(puzzle, attempt, stepId);
    }

    // ===== Type-Specific Validation Methods (v0.4.2b) =====

    /// <inheritdoc/>
    public PuzzleStepResult ValidateSequenceStep(Puzzle puzzle, PuzzleAttempt attempt, string stepId)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        ArgumentNullException.ThrowIfNull(attempt);

        // Validate puzzle type
        if (puzzle.Type != PuzzleType.Sequence || puzzle.SequenceData == null)
        {
            _logger.LogWarning(
                "ValidateSequenceStep: Called on non-sequence puzzle {PuzzleId} (Type={PuzzleType})",
                puzzle.Id, puzzle.Type);
            return PuzzleStepResult.WrongStep(puzzle, stepId, false);
        }

        var sequenceData = puzzle.SequenceData;

        // Check if this is the correct next step
        if (sequenceData.IsCorrectNextStep(attempt.CompletedSteps, stepId))
        {
            attempt.AddStep(stepId);
            var remaining = sequenceData.GetRemainingSteps(attempt.CompletedSteps);
            var description = sequenceData.GetStepDescription(stepId);

            // Check if sequence is complete
            if (sequenceData.IsComplete(attempt.CompletedSteps))
            {
                puzzle.Solve();
                attempt.Complete(true);

                _logger.LogInformation(
                    "ValidateSequenceStep: Puzzle {PuzzleId} ({PuzzleName}) solved via sequence completion",
                    puzzle.Id, puzzle.Name);

                return PuzzleStepResult.SequenceCompleted(puzzle, stepId);
            }

            _logger.LogDebug(
                "ValidateSequenceStep: Step {StepId} correct for puzzle {PuzzleId}, {Remaining} steps remaining",
                stepId, puzzle.Id, remaining);

            return PuzzleStepResult.CorrectStep(puzzle, stepId, remaining);
        }
        else
        {
            // Wrong step - check if we should reset
            var shouldReset = sequenceData.ResetOnWrongStep;

            if (shouldReset)
            {
                attempt.ClearSteps();
                _logger.LogDebug(
                    "ValidateSequenceStep: Sequence reset for puzzle {PuzzleId} due to wrong step {StepId}",
                    puzzle.Id, stepId);
            }
            else
            {
                _logger.LogDebug(
                    "ValidateSequenceStep: Wrong step {StepId} for puzzle {PuzzleId}, no reset",
                    stepId, puzzle.Id);
            }

            return PuzzleStepResult.WrongStep(puzzle, stepId, shouldReset);
        }
    }

    /// <inheritdoc/>
    public PuzzleSolveResult ValidateCombination(Puzzle puzzle, string input)
    {
        ArgumentNullException.ThrowIfNull(puzzle);

        // Validate puzzle type
        if (puzzle.Type != PuzzleType.Combination || puzzle.CombinationData == null)
        {
            _logger.LogWarning(
                "ValidateCombination: Called on non-combination puzzle {PuzzleId} (Type={PuzzleType})",
                puzzle.Id, puzzle.Type);
            return PuzzleSolveResult.NotSolvable(puzzle);
        }

        var combinationData = puzzle.CombinationData;

        // Validate input characters
        if (!string.IsNullOrEmpty(input) && !combinationData.IsValidInput(input))
        {
            _logger.LogDebug(
                "ValidateCombination: Invalid input characters for puzzle {PuzzleId}",
                puzzle.Id);
            return PuzzleSolveResult.Incorrect(puzzle, GetRemainingAttempts(puzzle), "Invalid characters in input.");
        }

        // Check solution
        if (combinationData.Validate(input ?? string.Empty))
        {
            puzzle.Solve();

            _logger.LogInformation(
                "ValidateCombination: Puzzle {PuzzleId} ({PuzzleName}) solved",
                puzzle.Id, puzzle.Name);

            return PuzzleSolveResult.Success(puzzle);
        }
        else
        {
            puzzle.RecordFailedAttempt();

            if (puzzle.IsFailed)
            {
                _logger.LogInformation(
                    "ValidateCombination: Puzzle {PuzzleId} failed - max attempts reached",
                    puzzle.Id);
                return PuzzleSolveResult.Failure(puzzle);
            }

            var remaining = GetRemainingAttempts(puzzle);
            var feedback = combinationData.GetFeedback(input ?? string.Empty);

            // Build feedback message
            var message = combinationData.ShowPartialFeedback && feedback.HasCorrectChars
                ? $"Incorrect. {feedback.CorrectPositions} correct position(s), {feedback.CorrectCharacters} correct character(s) in wrong position."
                : "Incorrect solution.";

            _logger.LogDebug(
                "ValidateCombination: Attempt failed for puzzle {PuzzleId}, {Remaining} attempts remaining",
                puzzle.Id, remaining);

            return PuzzleSolveResult.Incorrect(puzzle, remaining, message);
        }
    }

    /// <inheritdoc/>
    public PuzzleSolveResult ValidatePattern(Puzzle puzzle, string input)
    {
        ArgumentNullException.ThrowIfNull(puzzle);

        // Validate puzzle type
        if (puzzle.Type != PuzzleType.Pattern || puzzle.PatternData == null)
        {
            _logger.LogWarning(
                "ValidatePattern: Called on non-pattern puzzle {PuzzleId} (Type={PuzzleType})",
                puzzle.Id, puzzle.Type);
            return PuzzleSolveResult.NotSolvable(puzzle);
        }

        var patternData = puzzle.PatternData;

        // Validate input elements
        if (!string.IsNullOrEmpty(input) && !patternData.IsValidInput(input))
        {
            _logger.LogDebug(
                "ValidatePattern: Invalid pattern elements for puzzle {PuzzleId}",
                puzzle.Id);
            return PuzzleSolveResult.Incorrect(puzzle, GetRemainingAttempts(puzzle), "Invalid pattern elements.");
        }

        // Check pattern
        if (patternData.Validate(input ?? string.Empty))
        {
            puzzle.Solve();

            _logger.LogInformation(
                "ValidatePattern: Puzzle {PuzzleId} ({PuzzleName}) solved",
                puzzle.Id, puzzle.Name);

            return PuzzleSolveResult.Success(puzzle);
        }
        else
        {
            puzzle.RecordFailedAttempt();

            if (puzzle.IsFailed)
            {
                _logger.LogInformation(
                    "ValidatePattern: Puzzle {PuzzleId} failed - max attempts reached",
                    puzzle.Id);
                return PuzzleSolveResult.Failure(puzzle);
            }

            var remaining = GetRemainingAttempts(puzzle);

            _logger.LogDebug(
                "ValidatePattern: Attempt failed for puzzle {PuzzleId}, {Remaining} attempts remaining",
                puzzle.Id, remaining);

            return PuzzleSolveResult.Incorrect(puzzle, remaining, "Pattern does not match.");
        }
    }

    /// <inheritdoc/>
    public SequenceProgress GetSequenceProgress(Puzzle puzzle, PuzzleAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        ArgumentNullException.ThrowIfNull(attempt);

        if (puzzle.Type != PuzzleType.Sequence || puzzle.SequenceData == null)
        {
            return new SequenceProgress
            {
                TotalSteps = 0,
                CompletedSteps = 0,
                StepsRemaining = 0
            };
        }

        var sequenceData = puzzle.SequenceData;
        return new SequenceProgress
        {
            TotalSteps = sequenceData.TotalSteps,
            CompletedSteps = attempt.CompletedSteps.Count,
            StepsRemaining = sequenceData.GetRemainingSteps(attempt.CompletedSteps)
        };
    }

    // ===== Private Validation Helpers =====

    private PuzzleSolveResult ValidateGeneric(Puzzle puzzle, string input)
    {
        // Generic validation for Logic type or types without specific validators
        _logger.LogDebug(
            "ValidateGeneric: Generic validation for {PuzzleType} puzzle {PuzzleId}",
            puzzle.Type, puzzle.Id);

        puzzle.RecordFailedAttempt();

        if (puzzle.IsFailed)
        {
            return PuzzleSolveResult.Failure(puzzle);
        }

        return PuzzleSolveResult.Incorrect(puzzle, GetRemainingAttempts(puzzle));
    }

    private int GetRemainingAttempts(Puzzle puzzle)
    {
        if (puzzle.MaxAttempts < 0)
            return -1; // Unlimited

        return Math.Max(0, puzzle.MaxAttempts - puzzle.AttemptCount);
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
