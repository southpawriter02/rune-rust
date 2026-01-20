using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing puzzle interactions and validation.
/// </summary>
/// <remarks>
/// <para>
/// IPuzzleService provides methods for:
/// </para>
/// <list type="bullet">
///   <item><description>Querying puzzles in rooms</description></item>
///   <item><description>Managing puzzle attempts</description></item>
///   <item><description>Validating solutions</description></item>
///   <item><description>Processing resets and prerequisites</description></item>
///   <item><description>Riddle NPC validation (v0.4.2c)</description></item>
///   <item><description>Multi-part puzzle tracking (v0.4.2c)</description></item>
/// </list>
/// </remarks>
public interface IPuzzleService
{
    /// <summary>
    /// Gets a puzzle by keyword from the room.
    /// </summary>
    /// <param name="room">The room to search.</param>
    /// <param name="puzzleKeyword">The keyword to match.</param>
    /// <returns>The matching puzzle, or null if not found.</returns>
    Puzzle? GetPuzzle(Room room, string puzzleKeyword);

    /// <summary>
    /// Gets all puzzles in a room.
    /// </summary>
    /// <param name="room">The room to query.</param>
    /// <returns>An enumerable of all puzzles.</returns>
    IEnumerable<Puzzle> GetPuzzles(Room room);

    /// <summary>
    /// Gets all unsolved puzzles in a room.
    /// </summary>
    /// <param name="room">The room to query.</param>
    /// <returns>An enumerable of unsolved puzzles.</returns>
    IEnumerable<Puzzle> GetUnsolvedPuzzles(Room room);

    /// <summary>
    /// Begins an attempt on a puzzle.
    /// </summary>
    /// <param name="player">The player making the attempt.</param>
    /// <param name="puzzle">The puzzle to attempt.</param>
    /// <returns>Result of beginning the attempt.</returns>
    PuzzleAttemptResult BeginAttempt(Player player, Puzzle puzzle);

    /// <summary>
    /// Submits a solution attempt for a puzzle.
    /// </summary>
    /// <param name="player">The player solving.</param>
    /// <param name="puzzle">The puzzle to solve.</param>
    /// <param name="input">The solution input.</param>
    /// <returns>Result of the solve attempt.</returns>
    PuzzleSolveResult AttemptSolve(Player player, Puzzle puzzle, string input);

    /// <summary>
    /// Validates a sequence step.
    /// </summary>
    /// <param name="puzzle">The sequence puzzle.</param>
    /// <param name="attempt">The current attempt.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <returns>Result of the step validation.</returns>
    PuzzleStepResult ValidateStep(Puzzle puzzle, PuzzleAttempt attempt, string stepId);

    /// <summary>
    /// Gets the current attempt for a puzzle (if any).
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="puzzle">The puzzle.</param>
    /// <returns>The active attempt, or null.</returns>
    PuzzleAttempt? GetCurrentAttempt(Player player, Puzzle puzzle);

    /// <summary>
    /// Requests a hint for a puzzle.
    /// </summary>
    /// <param name="player">The player requesting.</param>
    /// <param name="puzzle">The puzzle.</param>
    /// <returns>Result of the hint request.</returns>
    PuzzleHintResult RequestHint(Player player, Puzzle puzzle);

    /// <summary>
    /// Processes puzzle resets at end of turn.
    /// </summary>
    /// <param name="room">The room to process.</param>
    /// <returns>Puzzles that were reset.</returns>
    IEnumerable<Puzzle> ProcessPuzzleResets(Room room);

    /// <summary>
    /// Checks and updates puzzle prerequisites.
    /// </summary>
    /// <param name="room">The room to update.</param>
    void UpdatePuzzleLocks(Room room);

    /// <summary>
    /// Applies puzzle completion effects.
    /// </summary>
    /// <param name="player">The player receiving rewards.</param>
    /// <param name="puzzle">The solved puzzle.</param>
    /// <returns>Result of applying rewards.</returns>
    PuzzleRewardResult ApplyPuzzleReward(Player player, Puzzle puzzle);

    /// <summary>
    /// Gets a description of all puzzles in a room.
    /// </summary>
    /// <param name="room">The room.</param>
    /// <returns>A formatted description string.</returns>
    string GetRoomPuzzlesDescription(Room room);

    /// <summary>
    /// Gets the examination description for a puzzle.
    /// </summary>
    /// <param name="puzzle">The puzzle to examine.</param>
    /// <returns>A detailed description.</returns>
    string ExaminePuzzle(Puzzle puzzle);

    // ===== Type-Specific Validation Methods (v0.4.2b) =====

    /// <summary>
    /// Validates a sequence puzzle step.
    /// </summary>
    /// <param name="puzzle">The sequence puzzle.</param>
    /// <param name="attempt">The current attempt.</param>
    /// <param name="stepId">The step being activated.</param>
    /// <returns>Result of the step validation.</returns>
    PuzzleStepResult ValidateSequenceStep(Puzzle puzzle, PuzzleAttempt attempt, string stepId);

    /// <summary>
    /// Validates a combination puzzle input.
    /// </summary>
    /// <param name="puzzle">The combination puzzle.</param>
    /// <param name="input">The player's input.</param>
    /// <returns>Result of the validation.</returns>
    PuzzleSolveResult ValidateCombination(Puzzle puzzle, string input);

    /// <summary>
    /// Validates a pattern puzzle input.
    /// </summary>
    /// <param name="puzzle">The pattern puzzle.</param>
    /// <param name="input">The player's pattern input.</param>
    /// <returns>Result of the validation.</returns>
    PuzzleSolveResult ValidatePattern(Puzzle puzzle, string input);

    /// <summary>
    /// Gets the progress for a sequence puzzle.
    /// </summary>
    /// <param name="puzzle">The sequence puzzle.</param>
    /// <param name="attempt">The current attempt.</param>
    /// <returns>Progress information.</returns>
    SequenceProgress GetSequenceProgress(Puzzle puzzle, PuzzleAttempt attempt);

    // ===== Riddle & Advanced Methods (v0.4.2c) =====

    /// <summary>
    /// Validates a riddle answer from a riddle NPC.
    /// </summary>
    /// <param name="npc">The riddle NPC.</param>
    /// <param name="answer">The player's answer.</param>
    /// <param name="player">The player attempting.</param>
    /// <returns>The riddle answer result.</returns>
    RiddleAnswerResult ValidateRiddleAnswer(RiddleNpc npc, string answer, Player player);

    /// <summary>
    /// Gets the next available hint for a puzzle with hint data.
    /// </summary>
    /// <param name="puzzle">The puzzle.</param>
    /// <param name="player">The player requesting.</param>
    /// <param name="hints">Available hints for the puzzle.</param>
    /// <returns>The hint result with actual hint data.</returns>
    PuzzleHintResult GetNextHint(Puzzle puzzle, Player player, IReadOnlyList<PuzzleHint> hints);

    /// <summary>
    /// Records a multi-part puzzle component as solved.
    /// </summary>
    /// <param name="multiPartPuzzle">The multi-part puzzle.</param>
    /// <param name="componentId">The component puzzle ID.</param>
    /// <returns>The step result including completion status.</returns>
    PuzzleStepResult RecordMultiPartComponent(MultiPartPuzzle multiPartPuzzle, string componentId);
}

// ===== Result Types =====

/// <summary>
/// Result of beginning a puzzle attempt.
/// </summary>
public readonly record struct PuzzleAttemptResult
{
    /// <summary>Gets the puzzle.</summary>
    public Puzzle Puzzle { get; init; }

    /// <summary>Gets whether the attempt started successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the active attempt (if started).</summary>
    public PuzzleAttempt? Attempt { get; init; }

    /// <summary>Gets the result message.</summary>
    public string Message { get; init; }

    /// <summary>Creates a successful start result.</summary>
    public static PuzzleAttemptResult Started(Puzzle puzzle, PuzzleAttempt attempt) =>
        new() { Puzzle = puzzle, Success = true, Attempt = attempt, Message = "Puzzle attempt started." };

    /// <summary>Creates an already-in-progress result.</summary>
    public static PuzzleAttemptResult AlreadyInProgress(Puzzle puzzle, PuzzleAttempt attempt) =>
        new() { Puzzle = puzzle, Success = true, Attempt = attempt, Message = "Puzzle attempt already in progress." };

    /// <summary>Creates a puzzle-locked result.</summary>
    public static PuzzleAttemptResult PuzzleLocked(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Success = false, Attempt = null, Message = "This puzzle is locked." };

    /// <summary>Creates a no-attempts-remaining result.</summary>
    public static PuzzleAttemptResult NoAttemptsRemaining(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Success = false, Attempt = null, Message = "No attempts remaining." };

    /// <summary>Creates an already-solved result.</summary>
    public static PuzzleAttemptResult AlreadySolved(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Success = false, Attempt = null, Message = "This puzzle has already been solved." };
}

/// <summary>
/// Result of attempting to solve a puzzle.
/// </summary>
public readonly record struct PuzzleSolveResult
{
    /// <summary>Gets the puzzle.</summary>
    public Puzzle Puzzle { get; init; }

    /// <summary>Gets whether the puzzle was solved.</summary>
    public bool Solved { get; init; }

    /// <summary>Gets whether the puzzle was failed (max attempts).</summary>
    public bool Failed { get; init; }

    /// <summary>Gets attempts remaining.</summary>
    public int AttemptsRemaining { get; init; }

    /// <summary>Gets the result message.</summary>
    public string Message { get; init; }

    /// <summary>Creates a success result.</summary>
    public static PuzzleSolveResult Success(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Solved = true, Failed = false, AttemptsRemaining = 0, Message = "Puzzle solved!" };

    /// <summary>Creates an incorrect result.</summary>
    public static PuzzleSolveResult Incorrect(Puzzle puzzle, int remaining) =>
        new() { Puzzle = puzzle, Solved = false, Failed = false, AttemptsRemaining = remaining, Message = "Incorrect solution." };

    /// <summary>Creates a failure result.</summary>
    public static PuzzleSolveResult Failure(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Solved = false, Failed = true, AttemptsRemaining = 0, Message = "Puzzle failed. Maximum attempts reached." };

    /// <summary>Creates a not-solvable result.</summary>
    public static PuzzleSolveResult NotSolvable(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Solved = false, Failed = false, AttemptsRemaining = 0, Message = "This puzzle cannot be solved in its current state." };

    /// <summary>Creates an incorrect result with custom message.</summary>
    public static PuzzleSolveResult Incorrect(Puzzle puzzle, int remaining, string message) =>
        new() { Puzzle = puzzle, Solved = false, Failed = false, AttemptsRemaining = remaining, Message = message };
}

/// <summary>
/// Result of validating a puzzle step.
/// </summary>
public readonly record struct PuzzleStepResult
{
    /// <summary>Gets the puzzle.</summary>
    public Puzzle Puzzle { get; init; }

    /// <summary>Gets the step ID.</summary>
    public string StepId { get; init; }

    /// <summary>Gets whether the step was correct.</summary>
    public bool Correct { get; init; }

    /// <summary>Gets whether the sequence is complete.</summary>
    public bool SequenceComplete { get; init; }

    /// <summary>Gets whether the sequence failed (wrong step).</summary>
    public bool SequenceFailed { get; init; }

    /// <summary>Gets steps remaining.</summary>
    public int StepsRemaining { get; init; }

    /// <summary>Gets the result message.</summary>
    public string Message { get; init; }

    /// <summary>Creates a correct step result.</summary>
    public static PuzzleStepResult CorrectStep(Puzzle puzzle, string stepId, int remaining) =>
        new() { Puzzle = puzzle, StepId = stepId, Correct = true, SequenceComplete = false, SequenceFailed = false, StepsRemaining = remaining, Message = "Correct step." };

    /// <summary>Creates a sequence complete result.</summary>
    public static PuzzleStepResult SequenceCompleted(Puzzle puzzle, string stepId) =>
        new() { Puzzle = puzzle, StepId = stepId, Correct = true, SequenceComplete = true, SequenceFailed = false, StepsRemaining = 0, Message = "Sequence completed!" };

    /// <summary>Creates a wrong step result.</summary>
    public static PuzzleStepResult WrongStep(Puzzle puzzle, string stepId, bool reset) =>
        new() { Puzzle = puzzle, StepId = stepId, Correct = false, SequenceComplete = false, SequenceFailed = reset, StepsRemaining = 0, Message = reset ? "Wrong step. Sequence reset." : "Wrong step." };
}

/// <summary>
/// Result of requesting a hint.
/// </summary>
public readonly record struct PuzzleHintResult
{
    /// <summary>Gets the puzzle.</summary>
    public Puzzle Puzzle { get; init; }

    /// <summary>Gets whether a hint was revealed.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the hint text.</summary>
    public string? Hint { get; init; }

    /// <summary>Gets total hints revealed.</summary>
    public int HintsRevealed { get; init; }

    /// <summary>Gets the result message.</summary>
    public string Message { get; init; }

    /// <summary>Creates a hint revealed result.</summary>
    public static PuzzleHintResult HintRevealed(Puzzle puzzle, string hint, int revealed) =>
        new() { Puzzle = puzzle, Success = true, Hint = hint, HintsRevealed = revealed, Message = "Hint revealed." };

    /// <summary>Creates a no-hints-available result.</summary>
    public static PuzzleHintResult NoHintsAvailable(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Success = false, Hint = null, HintsRevealed = 0, Message = "No hints available for this puzzle." };

    /// <summary>Creates an all-hints-revealed result.</summary>
    public static PuzzleHintResult AllHintsRevealed(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Success = false, Hint = null, HintsRevealed = puzzle.HintsRevealed, Message = "All hints have been revealed." };
}

/// <summary>
/// Result of applying puzzle rewards.
/// </summary>
public readonly record struct PuzzleRewardResult
{
    /// <summary>Gets the puzzle.</summary>
    public Puzzle Puzzle { get; init; }

    /// <summary>Gets whether reward was applied.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the reward ID.</summary>
    public string? RewardId { get; init; }

    /// <summary>Gets the result message.</summary>
    public string Message { get; init; }

    /// <summary>Creates a reward granted result.</summary>
    public static PuzzleRewardResult RewardGranted(Puzzle puzzle, string rewardId) =>
        new() { Puzzle = puzzle, Success = true, RewardId = rewardId, Message = "Reward granted." };

    /// <summary>Creates a no-reward result.</summary>
    public static PuzzleRewardResult NoReward(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Success = true, RewardId = null, Message = "No reward for this puzzle." };

    /// <summary>Creates a not-solved result.</summary>
    public static PuzzleRewardResult NotSolved(Puzzle puzzle) =>
        new() { Puzzle = puzzle, Success = false, RewardId = null, Message = "Puzzle must be solved first." };
}

/// <summary>
/// Progress information for a sequence puzzle.
/// </summary>
public readonly record struct SequenceProgress
{
    /// <summary>Gets the total steps in the sequence.</summary>
    public int TotalSteps { get; init; }

    /// <summary>Gets the number of completed steps.</summary>
    public int CompletedSteps { get; init; }

    /// <summary>Gets the number of steps remaining.</summary>
    public int StepsRemaining { get; init; }

    /// <summary>Gets whether the sequence is complete.</summary>
    public bool IsComplete => StepsRemaining == 0 && TotalSteps > 0;

    /// <summary>Gets the progress percentage (0-100).</summary>
    public int ProgressPercent => TotalSteps > 0 ? (CompletedSteps * 100) / TotalSteps : 0;
}

// ===== Riddle & Advanced Result Types (v0.4.2c) =====

/// <summary>
/// Result of answering a riddle NPC.
/// </summary>
public readonly record struct RiddleAnswerResult
{
    /// <summary>Gets the riddle NPC.</summary>
    public RiddleNpc Npc { get; init; }

    /// <summary>Gets whether the answer was correct.</summary>
    public bool Correct { get; init; }

    /// <summary>Gets the response message.</summary>
    public string Message { get; init; }

    /// <summary>Gets remaining attempts (-1 if unlimited).</summary>
    public int RemainingAttempts { get; init; }

    /// <summary>Gets whether max failures was reached.</summary>
    public bool MaxFailuresReached { get; init; }

    /// <summary>Gets the consequence applied (if max failures).</summary>
    public Domain.Enums.RiddleConsequence? ConsequenceApplied { get; init; }

    /// <summary>Creates a correct answer result.</summary>
    public static RiddleAnswerResult CorrectAnswer(RiddleNpc npc, string message) =>
        new() { Npc = npc, Correct = true, Message = message, RemainingAttempts = 0, MaxFailuresReached = false };

    /// <summary>Creates a wrong answer result.</summary>
    public static RiddleAnswerResult WrongAnswer(RiddleNpc npc, string message, int remaining) =>
        new() { Npc = npc, Correct = false, Message = message, RemainingAttempts = remaining, MaxFailuresReached = false };

    /// <summary>Creates a max failures result.</summary>
    public static RiddleAnswerResult MaxFailures(RiddleNpc npc, string message, Domain.Enums.RiddleConsequence? consequence) =>
        new() { Npc = npc, Correct = false, Message = message, RemainingAttempts = 0, MaxFailuresReached = true, ConsequenceApplied = consequence };

    /// <summary>Creates an already solved result.</summary>
    public static RiddleAnswerResult AlreadySolved(RiddleNpc npc) =>
        new() { Npc = npc, Correct = true, Message = "This riddle has already been solved.", RemainingAttempts = 0 };
}
