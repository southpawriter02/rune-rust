using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a logic challenge that players can attempt to solve.
/// </summary>
/// <remarks>
/// <para>
/// Puzzle entities manage the lifecycle of interactive brain teasers, including:
/// </para>
/// <list type="bullet">
///   <item><description>State tracking (Unsolved, InProgress, Solved, Failed, Locked)</description></item>
///   <item><description>Attempt counting with configurable limits</description></item>
///   <item><description>Reset mechanics (time-based or manual)</description></item>
///   <item><description>Prerequisite support for chained puzzles</description></item>
///   <item><description>Hint reveal tracking</description></item>
/// </list>
/// <para>
/// Type-specific validation is implemented in v0.4.2b via PuzzleService.
/// </para>
/// </remarks>
/// <seealso cref="PuzzleState"/>
/// <seealso cref="PuzzleType"/>
public class Puzzle : IEntity
{
    // ===== Core Properties =====

    /// <summary>
    /// Gets the unique identifier for this puzzle.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the definition ID from configuration.
    /// </summary>
    public string DefinitionId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the display name of this puzzle.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description shown when examining the puzzle.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current state of this puzzle.
    /// </summary>
    public PuzzleState State { get; private set; } = PuzzleState.Unsolved;

    /// <summary>
    /// Gets the type of this puzzle.
    /// </summary>
    public PuzzleType Type { get; private set; }

    // ===== Attempt Properties =====

    /// <summary>
    /// Gets the maximum number of attempts allowed (-1 = unlimited).
    /// </summary>
    public int MaxAttempts { get; private set; } = -1;

    /// <summary>
    /// Gets the current attempt count.
    /// </summary>
    public int AttemptCount { get; private set; }

    // ===== Reset Properties =====

    /// <summary>
    /// Gets whether this puzzle can be reset after failure.
    /// </summary>
    public bool CanReset { get; private set; } = true;

    /// <summary>
    /// Gets the turns until reset after failure (-1 = manual reset only).
    /// </summary>
    public int ResetDelay { get; private set; } = -1;

    /// <summary>
    /// Gets turns remaining until reset.
    /// </summary>
    public int? TurnsUntilReset { get; private set; }

    // ===== Meta Properties =====

    /// <summary>
    /// Gets the difficulty rating (1-5 scale for UI display).
    /// </summary>
    public int Difficulty { get; private set; } = 1;

    /// <summary>
    /// Gets whether hints are available for this puzzle.
    /// </summary>
    public bool HasHints { get; private set; }

    /// <summary>
    /// Gets the number of hints revealed.
    /// </summary>
    public int HintsRevealed { get; private set; }

    /// <summary>
    /// Gets the associated reward definition ID (null if no reward).
    /// </summary>
    public string? RewardId { get; private set; }

    /// <summary>
    /// Gets the prerequisite puzzle ID that must be solved first.
    /// </summary>
    public string? PrerequisitePuzzleId { get; private set; }

    /// <summary>
    /// Gets IDs of objects affected when puzzle is solved.
    /// </summary>
    public IReadOnlyList<Guid> AffectedObjectIds { get; private set; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets keywords for identifying this puzzle in commands.
    /// </summary>
    public IReadOnlyList<string> Keywords { get; private set; } = Array.Empty<string>();

    // ===== Type-Specific Data Properties (v0.4.2b) =====

    /// <summary>
    /// Gets the sequence puzzle configuration (for Sequence type puzzles).
    /// </summary>
    public SequencePuzzle? SequenceData { get; private set; }

    /// <summary>
    /// Gets the combination puzzle configuration (for Combination type puzzles).
    /// </summary>
    public CombinationPuzzle? CombinationData { get; private set; }

    /// <summary>
    /// Gets the pattern puzzle configuration (for Pattern type puzzles).
    /// </summary>
    public PatternPuzzle? PatternData { get; private set; }

    // ===== Computed Properties =====

    /// <summary>
    /// Gets whether this puzzle is currently solvable.
    /// </summary>
    public bool IsSolvable => State == PuzzleState.Unsolved || State == PuzzleState.InProgress;

    /// <summary>
    /// Gets whether this puzzle has been solved.
    /// </summary>
    public bool IsSolved => State == PuzzleState.Solved;

    /// <summary>
    /// Gets whether this puzzle has failed.
    /// </summary>
    public bool IsFailed => State == PuzzleState.Failed;

    /// <summary>
    /// Gets whether this puzzle is locked (requires prerequisite).
    /// </summary>
    public bool IsLocked => State == PuzzleState.Locked;

    /// <summary>
    /// Gets whether attempts remain for this puzzle.
    /// </summary>
    public bool HasAttemptsRemaining => MaxAttempts < 0 || AttemptCount < MaxAttempts;

    /// <summary>
    /// Gets whether this puzzle has a prerequisite.
    /// </summary>
    public bool HasPrerequisite => !string.IsNullOrEmpty(PrerequisitePuzzleId);

    /// <summary>
    /// Gets whether this puzzle has a reward.
    /// </summary>
    public bool HasReward => !string.IsNullOrEmpty(RewardId);

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private Puzzle()
    {
    }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a new puzzle.
    /// </summary>
    /// <param name="definitionId">The definition ID from configuration.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The examination description.</param>
    /// <param name="type">The puzzle type.</param>
    /// <param name="maxAttempts">Maximum attempts (-1 = unlimited).</param>
    /// <param name="canReset">Whether puzzle can reset.</param>
    /// <param name="resetDelay">Turns until reset (-1 = manual).</param>
    /// <param name="difficulty">Difficulty rating (1-5).</param>
    /// <param name="hasHints">Whether hints are available.</param>
    /// <param name="rewardId">Associated reward ID.</param>
    /// <param name="prerequisiteId">Prerequisite puzzle ID.</param>
    /// <param name="affectedObjects">Objects affected on solve.</param>
    /// <param name="keywords">Reference keywords.</param>
    /// <returns>A new Puzzle instance.</returns>
    public static Puzzle Create(
        string definitionId,
        string name,
        string description,
        PuzzleType type,
        int maxAttempts = -1,
        bool canReset = true,
        int resetDelay = -1,
        int difficulty = 1,
        bool hasHints = false,
        string? rewardId = null,
        string? prerequisiteId = null,
        IEnumerable<Guid>? affectedObjects = null,
        IEnumerable<string>? keywords = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Puzzle
        {
            Id = Guid.NewGuid(),
            DefinitionId = definitionId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            Type = type,
            MaxAttempts = maxAttempts,
            CanReset = canReset,
            ResetDelay = resetDelay,
            Difficulty = Math.Clamp(difficulty, 1, 5),
            HasHints = hasHints,
            RewardId = rewardId,
            PrerequisitePuzzleId = prerequisiteId,
            AffectedObjectIds = (IReadOnlyList<Guid>?)affectedObjects?.ToList() ?? Array.Empty<Guid>(),
            Keywords = (IReadOnlyList<string>?)keywords?.Select(k => k.ToLowerInvariant()).ToList() ?? Array.Empty<string>()
        };
    }

    // ===== State Transition Methods =====

    /// <summary>
    /// Begins an attempt on this puzzle.
    /// </summary>
    /// <returns>True if attempt was started; false if not solvable.</returns>
    public bool BeginAttempt()
    {
        if (!IsSolvable || !HasAttemptsRemaining)
            return false;

        if (State == PuzzleState.Unsolved)
            State = PuzzleState.InProgress;

        return true;
    }

    /// <summary>
    /// Records a failed attempt.
    /// </summary>
    /// <remarks>
    /// Increments attempt count and transitions to Failed state if max reached.
    /// If CanReset and ResetDelay >= 0, starts the reset countdown.
    /// </remarks>
    public void RecordFailedAttempt()
    {
        AttemptCount++;

        if (MaxAttempts > 0 && AttemptCount >= MaxAttempts)
        {
            State = PuzzleState.Failed;
            if (CanReset && ResetDelay >= 0)
            {
                TurnsUntilReset = ResetDelay;
            }
        }
    }

    /// <summary>
    /// Marks this puzzle as solved.
    /// </summary>
    public void Solve()
    {
        State = PuzzleState.Solved;
    }

    /// <summary>
    /// Resets this puzzle to unsolved state.
    /// </summary>
    /// <returns>True if reset; false if cannot reset or already solved.</returns>
    public bool Reset()
    {
        if (!CanReset || State == PuzzleState.Solved)
            return false;

        State = PuzzleState.Unsolved;
        AttemptCount = 0;
        TurnsUntilReset = null;
        return true;
    }

    /// <summary>
    /// Processes turn tick for resetting puzzles.
    /// </summary>
    /// <returns>True if puzzle was reset this tick.</returns>
    public bool TickReset()
    {
        if (State != PuzzleState.Failed || !TurnsUntilReset.HasValue)
            return false;

        TurnsUntilReset--;

        if (TurnsUntilReset <= 0)
        {
            return Reset();
        }

        return false;
    }

    // ===== Hint Methods =====

    /// <summary>
    /// Reveals a hint for this puzzle.
    /// </summary>
    /// <returns>True if hint was revealed; false if no hints available.</returns>
    public bool RevealHint()
    {
        if (!HasHints)
            return false;

        HintsRevealed++;
        return true;
    }

    // ===== Lock Methods =====

    /// <summary>
    /// Locks this puzzle (requires prerequisite).
    /// </summary>
    public void Lock()
    {
        if (State == PuzzleState.Unsolved)
            State = PuzzleState.Locked;
    }

    /// <summary>
    /// Unlocks this puzzle when prerequisite is met.
    /// </summary>
    public void Unlock()
    {
        if (State == PuzzleState.Locked)
            State = PuzzleState.Unsolved;
    }

    // ===== Type-Specific Data Setters (v0.4.2b) =====

    /// <summary>
    /// Sets the sequence data for a Sequence type puzzle.
    /// </summary>
    /// <param name="sequenceData">The sequence configuration.</param>
    /// <exception cref="InvalidOperationException">If puzzle type is not Sequence.</exception>
    public void SetSequenceData(SequencePuzzle sequenceData)
    {
        if (Type != PuzzleType.Sequence)
            throw new InvalidOperationException($"Cannot set sequence data on {Type} puzzle.");

        SequenceData = sequenceData ?? throw new ArgumentNullException(nameof(sequenceData));
    }

    /// <summary>
    /// Sets the combination data for a Combination type puzzle.
    /// </summary>
    /// <param name="combinationData">The combination configuration.</param>
    /// <exception cref="InvalidOperationException">If puzzle type is not Combination.</exception>
    public void SetCombinationData(CombinationPuzzle combinationData)
    {
        if (Type != PuzzleType.Combination)
            throw new InvalidOperationException($"Cannot set combination data on {Type} puzzle.");

        CombinationData = combinationData ?? throw new ArgumentNullException(nameof(combinationData));
    }

    /// <summary>
    /// Sets the pattern data for a Pattern type puzzle.
    /// </summary>
    /// <param name="patternData">The pattern configuration.</param>
    /// <exception cref="InvalidOperationException">If puzzle type is not Pattern.</exception>
    public void SetPatternData(PatternPuzzle patternData)
    {
        if (Type != PuzzleType.Pattern)
            throw new InvalidOperationException($"Cannot set pattern data on {Type} puzzle.");

        PatternData = patternData ?? throw new ArgumentNullException(nameof(patternData));
    }

    // ===== Keyword Matching =====

    /// <summary>
    /// Checks if this puzzle matches the specified keyword (case-insensitive).
    /// </summary>
    /// <param name="keyword">The keyword to match.</param>
    /// <returns>True if puzzle matches keyword.</returns>
    public bool MatchesKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return false;

        var lowerKeyword = keyword.ToLowerInvariant();

        // Check explicit keywords
        if (Keywords.Any(k => k.Equals(lowerKeyword, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Check name contains keyword
        if (Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            return true;

        // Check definition ID
        if (DefinitionId.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Returns a string representation of this puzzle.
    /// </summary>
    public override string ToString() =>
        $"{Name} ({Type}, {State}, Attempts={AttemptCount}/{(MaxAttempts < 0 ? "∞" : MaxAttempts.ToString())})";
}
