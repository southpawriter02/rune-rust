using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Infrastructure.Services;

/// <summary>
/// Tracks explored rooms during a game session.
/// </summary>
/// <remarks>
/// Maintains session-based exploration state including:
/// - Set of visited rooms
/// - Set of known adjacent rooms (visible but unvisited)
/// - Event notifications for UI updates
/// </remarks>
public class ExplorationTracker : IExplorationTracker
{
    private readonly HashSet<Guid> _exploredRooms = new();
    private readonly HashSet<Guid> _knownAdjacentRooms = new();
    private readonly ILogger<ExplorationTracker>? _logger;
    
    /// <inheritdoc/>
    public IReadOnlySet<Guid> ExploredRooms => _exploredRooms;
    
    /// <inheritdoc/>
    public IReadOnlySet<Guid> KnownAdjacentRooms => _knownAdjacentRooms;
    
    /// <inheritdoc/>
    public event Action<Guid>? OnRoomExplored;
    
    /// <summary>
    /// Initializes a new instance of <see cref="ExplorationTracker"/>.
    /// </summary>
    /// <param name="logger">Optional logger.</param>
    public ExplorationTracker(ILogger<ExplorationTracker>? logger = null)
    {
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public void MarkExplored(Guid roomId)
    {
        if (_exploredRooms.Contains(roomId))
        {
            return; // Already explored
        }
        
        _exploredRooms.Add(roomId);
        _knownAdjacentRooms.Remove(roomId); // No longer "adjacent unknown"
        
        _logger?.LogDebug("Room {RoomId} marked as explored. " +
            "Total explored: {ExploredCount}, Adjacent: {AdjacentCount}",
            roomId, _exploredRooms.Count, _knownAdjacentRooms.Count);
        
        OnRoomExplored?.Invoke(roomId);
    }
    
    /// <summary>
    /// Marks a room as known adjacent (discovered but not visited).
    /// </summary>
    /// <param name="roomId">The adjacent room ID.</param>
    public void MarkKnownAdjacent(Guid roomId)
    {
        if (_exploredRooms.Contains(roomId))
        {
            return; // Already explored, don't add to adjacent
        }
        
        _knownAdjacentRooms.Add(roomId);
    }
    
    /// <inheritdoc/>
    public bool IsExplored(Guid roomId)
    {
        return _exploredRooms.Contains(roomId);
    }
    
    /// <inheritdoc/>
    public void Reset()
    {
        _exploredRooms.Clear();
        _knownAdjacentRooms.Clear();
        _logger?.LogInformation("Exploration data reset");
    }
}
