namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Tracks which rooms the player has explored.
/// </summary>
public interface IExplorationTracker
{
    /// <summary>
    /// Marks a room as explored.
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    void MarkExplored(Guid roomId);
    
    /// <summary>
    /// Checks if a room has been explored.
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    /// <returns>True if the room has been visited.</returns>
    bool IsExplored(Guid roomId);
    
    /// <summary>
    /// Gets all explored room IDs.
    /// </summary>
    IReadOnlySet<Guid> ExploredRooms { get; }
    
    /// <summary>
    /// Gets rooms adjacent to explored rooms (known but unvisited).
    /// </summary>
    IReadOnlySet<Guid> KnownAdjacentRooms { get; }
    
    /// <summary>
    /// Clears all exploration data.
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Event raised when a room is explored.
    /// </summary>
    event Action<Guid>? OnRoomExplored;
}
