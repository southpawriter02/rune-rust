namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks the exploration state of a room.
/// </summary>
/// <remarks>
/// Exploration state affects map display, gameplay mechanics,
/// and is tracked per-room rather than per-session for persistence.
/// </remarks>
public enum ExplorationState
{
    /// <summary>
    /// Room exists and is known (e.g., visible exit) but player has not entered.
    /// </summary>
    /// <remarks>
    /// Shown as '?' on the map. The room's existence is known but contents are unknown.
    /// </remarks>
    Unexplored = 0,

    /// <summary>
    /// Player has entered the room at least once.
    /// </summary>
    /// <remarks>
    /// Shown with the room type symbol on the map. Basic room info is known.
    /// </remarks>
    Visited = 1,

    /// <summary>
    /// Room is fully explored: searched for hidden content, all monsters defeated.
    /// </summary>
    /// <remarks>
    /// May display differently (e.g., dimmed) to indicate no further action needed.
    /// Used for exploration progress tracking and achievements.
    /// </remarks>
    Cleared = 2
}
