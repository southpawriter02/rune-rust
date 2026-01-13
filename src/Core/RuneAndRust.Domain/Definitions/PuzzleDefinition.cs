using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Configuration definition for a puzzle type.
/// </summary>
/// <remarks>
/// <para>
/// PuzzleDefinition is loaded from puzzles.json and used to create Puzzle instances.
/// All properties use init setters for JSON deserialization.
/// </para>
/// </remarks>
/// <seealso cref="Entities.Puzzle"/>
public class PuzzleDefinition
{
    /// <summary>
    /// Gets or sets the unique identifier for this definition.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of puzzles created from this definition.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the description shown when examining the puzzle.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of puzzle.
    /// </summary>
    public PuzzleType Type { get; init; }

    /// <summary>
    /// Gets or sets the difficulty rating (1-5 scale).
    /// </summary>
    public int Difficulty { get; init; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of attempts (-1 = unlimited).
    /// </summary>
    public int MaxAttempts { get; init; } = -1;

    /// <summary>
    /// Gets or sets whether this puzzle can be reset after failure.
    /// </summary>
    public bool CanReset { get; init; } = true;

    /// <summary>
    /// Gets or sets the turns until reset after failure (-1 = manual only).
    /// </summary>
    public int ResetDelay { get; init; } = -1;

    /// <summary>
    /// Gets or sets whether hints are available.
    /// </summary>
    public bool HasHints { get; init; }

    /// <summary>
    /// Gets or sets the associated reward definition ID.
    /// </summary>
    public string? RewardId { get; init; }

    /// <summary>
    /// Gets or sets the prerequisite puzzle ID.
    /// </summary>
    public string? PrerequisiteId { get; init; }

    /// <summary>
    /// Gets or sets keywords for referencing the puzzle.
    /// </summary>
    public List<string> Keywords { get; init; } = [];
}
