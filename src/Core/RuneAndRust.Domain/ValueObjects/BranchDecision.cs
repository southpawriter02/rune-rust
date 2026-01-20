using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the branching decision for a room's exits.
/// </summary>
public readonly record struct BranchDecision
{
    /// <summary>
    /// Gets the position where the decision was made.
    /// </summary>
    public Position3D Position { get; init; }

    /// <summary>
    /// Gets the branch type for each exit direction.
    /// </summary>
    public IReadOnlyDictionary<Direction, BranchType> ExitDecisions { get; init; }

    /// <summary>
    /// Gets whether this room is a dead end (no continuing paths).
    /// </summary>
    public bool IsDeadEnd { get; init; }

    /// <summary>
    /// Gets all directions that have main path exits.
    /// </summary>
    public IEnumerable<Direction> MainPaths =>
        ExitDecisions.Where(kv => kv.Value == BranchType.MainPath).Select(kv => kv.Key);

    /// <summary>
    /// Gets all directions that have side path exits.
    /// </summary>
    public IEnumerable<Direction> SidePaths =>
        ExitDecisions.Where(kv => kv.Value == BranchType.SidePath).Select(kv => kv.Key);

    /// <summary>
    /// Gets all directions that have dead end exits.
    /// </summary>
    public IEnumerable<Direction> DeadEnds =>
        ExitDecisions.Where(kv => kv.Value == BranchType.DeadEnd).Select(kv => kv.Key);

    /// <summary>
    /// Gets all directions that connect to loops.
    /// </summary>
    public IEnumerable<Direction> Loops =>
        ExitDecisions.Where(kv => kv.Value == BranchType.Loop).Select(kv => kv.Key);

    /// <summary>
    /// Gets all directions with valid exits (not None).
    /// </summary>
    public IEnumerable<Direction> AllExits =>
        ExitDecisions.Where(kv => kv.Value != BranchType.None).Select(kv => kv.Key);

    /// <summary>
    /// Creates a simple decision with all exits as main paths.
    /// </summary>
    public static BranchDecision AllMainPath(Position3D position, IEnumerable<Direction> exits)
    {
        var decisions = exits.ToDictionary(d => d, _ => BranchType.MainPath);
        return new BranchDecision
        {
            Position = position,
            ExitDecisions = decisions,
            IsDeadEnd = false
        };
    }

    /// <summary>
    /// Creates a dead end decision (no exits).
    /// </summary>
    public static BranchDecision CreateDeadEnd(Position3D position) => new()
    {
        Position = position,
        ExitDecisions = new Dictionary<Direction, BranchType>(),
        IsDeadEnd = true
    };
}
