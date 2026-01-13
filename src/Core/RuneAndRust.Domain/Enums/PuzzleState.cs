namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the current state of a puzzle.
/// </summary>
/// <remarks>
/// <para>
/// PuzzleState tracks the lifecycle of a puzzle from unsolved through completion or failure.
/// The state machine supports:
/// </para>
/// <list type="bullet">
///   <item><description>Initial unsolved state for new puzzles</description></item>
///   <item><description>In-progress tracking for multi-step puzzles</description></item>
///   <item><description>Solved state for completed puzzles</description></item>
///   <item><description>Failed state when max attempts exceeded</description></item>
///   <item><description>Locked state for prerequisite-gated puzzles</description></item>
/// </list>
/// </remarks>
/// <seealso cref="Entities.Puzzle"/>
public enum PuzzleState
{
    /// <summary>
    /// Puzzle has not been attempted.
    /// </summary>
    /// <remarks>
    /// Initial state for all puzzles. Transitions to InProgress when player begins attempt,
    /// or to Locked if prerequisite is not met.
    /// </remarks>
    Unsolved = 0,

    /// <summary>
    /// Puzzle attempt is in progress.
    /// </summary>
    /// <remarks>
    /// Active state while player is working on the puzzle. For sequence puzzles, this means
    /// steps are being completed. For combination puzzles, input is being entered.
    /// </remarks>
    InProgress = 1,

    /// <summary>
    /// Puzzle has been successfully solved.
    /// </summary>
    /// <remarks>
    /// Terminal state indicating the player has correctly solved the puzzle.
    /// Solved puzzles cannot be reset.
    /// </remarks>
    Solved = 2,

    /// <summary>
    /// Puzzle was failed (max attempts reached).
    /// </summary>
    /// <remarks>
    /// Failure state when player has exhausted all allowed attempts.
    /// If CanReset is true, the puzzle may reset to Unsolved after ResetDelay turns.
    /// </remarks>
    Failed = 3,

    /// <summary>
    /// Puzzle is locked (prerequisite not met).
    /// </summary>
    /// <remarks>
    /// Blocked state for puzzles that require another puzzle to be solved first.
    /// Transitions to Unsolved when the prerequisite puzzle is solved.
    /// </remarks>
    Locked = 4
}
