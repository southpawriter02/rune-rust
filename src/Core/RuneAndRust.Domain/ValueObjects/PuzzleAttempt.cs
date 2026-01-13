namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks the progress of a puzzle attempt.
/// </summary>
/// <remarks>
/// <para>
/// PuzzleAttempt maintains the state of an active puzzle attempt, including:
/// </para>
/// <list type="bullet">
///   <item><description>Completed steps for sequence puzzles</description></item>
///   <item><description>Current input for combination puzzles</description></item>
///   <item><description>Attempt timing and completion status</description></item>
/// </list>
/// <para>
/// Attempts are managed by PuzzleService and keyed by (PlayerId, PuzzleId) tuple.
/// </para>
/// </remarks>
/// <seealso cref="Entities.Puzzle"/>
public class PuzzleAttempt
{
    // ===== Core Properties =====

    /// <summary>
    /// Gets the puzzle ID being attempted.
    /// </summary>
    public Guid PuzzleId { get; private set; }

    /// <summary>
    /// Gets when the attempt started.
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// Gets the steps completed so far (for sequence puzzles).
    /// </summary>
    public IReadOnlyList<string> CompletedSteps { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets the current input (for combination puzzles).
    /// </summary>
    public string CurrentInput { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether this attempt is still active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the result of this attempt (null if still active).
    /// </summary>
    public bool? Succeeded { get; private set; }

    // ===== Computed Properties =====

    /// <summary>
    /// Gets the number of steps completed.
    /// </summary>
    public int StepCount => _completedSteps.Count;

    /// <summary>
    /// Gets the length of current input.
    /// </summary>
    public int InputLength => CurrentInput.Length;

    /// <summary>
    /// Gets whether any progress has been made.
    /// </summary>
    public bool HasProgress => StepCount > 0 || InputLength > 0;

    // ===== Private Fields =====

    private readonly List<string> _completedSteps = [];

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private PuzzleAttempt()
    {
    }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a new puzzle attempt.
    /// </summary>
    /// <param name="puzzleId">The puzzle being attempted.</param>
    /// <returns>A new active puzzle attempt.</returns>
    public static PuzzleAttempt Create(Guid puzzleId)
    {
        return new PuzzleAttempt
        {
            PuzzleId = puzzleId,
            StartedAt = DateTime.UtcNow
        };
    }

    // ===== Step Methods (Sequence Puzzles) =====

    /// <summary>
    /// Adds a completed step (for sequence puzzles).
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    public void AddStep(string stepId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);
        _completedSteps.Add(stepId);
        CompletedSteps = _completedSteps.ToList();
    }

    /// <summary>
    /// Clears all completed steps.
    /// </summary>
    public void ClearSteps()
    {
        _completedSteps.Clear();
        CompletedSteps = Array.Empty<string>();
    }

    // ===== Input Methods (Combination Puzzles) =====

    /// <summary>
    /// Sets the current input (for combination puzzles).
    /// </summary>
    /// <param name="input">The input value.</param>
    public void SetInput(string input)
    {
        CurrentInput = input ?? string.Empty;
    }

    /// <summary>
    /// Appends to the current input.
    /// </summary>
    /// <param name="value">Value to append.</param>
    public void AppendInput(string value)
    {
        CurrentInput += value ?? string.Empty;
    }

    /// <summary>
    /// Clears the current input.
    /// </summary>
    public void ClearInput()
    {
        CurrentInput = string.Empty;
    }

    // ===== Completion Methods =====

    /// <summary>
    /// Marks this attempt as completed.
    /// </summary>
    /// <param name="succeeded">Whether the attempt succeeded.</param>
    public void Complete(bool succeeded)
    {
        IsActive = false;
        Succeeded = succeeded;
    }

    /// <summary>
    /// Resets this attempt for a retry.
    /// </summary>
    public void Reset()
    {
        _completedSteps.Clear();
        CompletedSteps = Array.Empty<string>();
        CurrentInput = string.Empty;
        IsActive = true;
        Succeeded = null;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a string representation of this attempt.
    /// </summary>
    public override string ToString() =>
        $"PuzzleAttempt({PuzzleId}, Steps={StepCount}, Input={InputLength}chars, Active={IsActive})";
}
