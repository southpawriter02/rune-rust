namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Statistics about dungeon exploration progress.
/// </summary>
/// <param name="TotalRooms">Total rooms discovered.</param>
/// <param name="VisitedRooms">Rooms the player has entered.</param>
/// <param name="ClearedRooms">Rooms fully cleared.</param>
/// <param name="ExploredLevels">Z levels with visited rooms.</param>
public readonly record struct ExplorationStatistics(
    int TotalRooms,
    int VisitedRooms,
    int ClearedRooms,
    IReadOnlyList<int> ExploredLevels)
{
    /// <summary>
    /// Gets the exploration percentage (0-100).
    /// </summary>
    public float ExplorationPercent => TotalRooms > 0
        ? (float)VisitedRooms / TotalRooms * 100
        : 0;

    /// <summary>
    /// Gets the cleared percentage (0-100).
    /// </summary>
    public float ClearedPercent => TotalRooms > 0
        ? (float)ClearedRooms / TotalRooms * 100
        : 0;

    /// <summary>
    /// Gets the number of unexplored rooms.
    /// </summary>
    public int UnexploredRooms => TotalRooms - VisitedRooms;

    /// <summary>
    /// Gets the deepest explored level.
    /// </summary>
    public int DeepestLevel => ExploredLevels.Count > 0
        ? ExploredLevels.Max()
        : 0;

    /// <summary>
    /// Empty statistics value.
    /// </summary>
    public static ExplorationStatistics Empty => new(0, 0, 0, Array.Empty<int>());
}
