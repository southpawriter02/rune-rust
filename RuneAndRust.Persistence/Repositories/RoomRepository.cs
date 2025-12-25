using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Room entity operations.
/// Provides specialized queries for spatial navigation.
/// </summary>
/// <remarks>See: SPEC-REPO-001 for Repository Pattern design.</remarks>
public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    private readonly ILogger<RoomRepository> _roomLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The generic repository logger.</param>
    /// <param name="roomLogger">The room-specific logger.</param>
    public RoomRepository(
        RuneAndRustDbContext context,
        ILogger<GenericRepository<Room>> logger,
        ILogger<RoomRepository> roomLogger)
        : base(context, logger)
    {
        _roomLogger = roomLogger;
    }

    /// <inheritdoc/>
    public async Task<Room?> GetByPositionAsync(Coordinate position)
    {
        _roomLogger.LogDebug("Fetching room at position {Position}", position);

        // Use backing fields for EF Core LINQ translation (v0.3.18a)
        var room = await _dbSet
            .FirstOrDefaultAsync(r => r.PositionX == position.X
                                   && r.PositionY == position.Y
                                   && r.PositionZ == position.Z);

        if (room == null)
        {
            _roomLogger.LogDebug("No room found at position {Position}", position);
        }
        else
        {
            _roomLogger.LogDebug("Found room '{RoomName}' at position {Position}", room.Name, position);
        }

        return room;
    }

    /// <inheritdoc/>
    public async Task<Room?> GetStartingRoomAsync()
    {
        _roomLogger.LogDebug("Fetching starting room");

        var room = await _dbSet.FirstOrDefaultAsync(r => r.IsStartingRoom);

        if (room == null)
        {
            _roomLogger.LogWarning("No starting room found in database");
        }
        else
        {
            _roomLogger.LogDebug("Found starting room: '{RoomName}' ({RoomId})", room.Name, room.Id);
        }

        return room;
    }

    /// <inheritdoc/>
    public async Task<bool> PositionExistsAsync(Coordinate position)
    {
        _roomLogger.LogDebug("Checking if room exists at position {Position}", position);

        // Use backing fields for EF Core LINQ translation (v0.3.18a)
        var exists = await _dbSet
            .AnyAsync(r => r.PositionX == position.X
                        && r.PositionY == position.Y
                        && r.PositionZ == position.Z);

        _roomLogger.LogDebug("Position {Position} exists: {Exists}", position, exists);

        return exists;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Room>> GetAllRoomsAsync()
    {
        _roomLogger.LogDebug("Fetching all rooms ordered by position");

        // Use backing fields for EF Core LINQ translation (v0.3.18a)
        var rooms = await _dbSet
            .OrderBy(r => r.PositionZ)
            .ThenBy(r => r.PositionY)
            .ThenBy(r => r.PositionX)
            .ToListAsync();

        _roomLogger.LogDebug("Retrieved {Count} rooms", rooms.Count);

        return rooms;
    }

    /// <inheritdoc/>
    public async Task ClearAllRoomsAsync()
    {
        _roomLogger.LogInformation("Clearing all rooms from database");

        var allRooms = await _dbSet.ToListAsync();
        _dbSet.RemoveRange(allRooms);
        await _context.SaveChangesAsync();

        _roomLogger.LogInformation("Cleared {Count} rooms from database", allRooms.Count);
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<Room> rooms)
    {
        var roomList = rooms.ToList();
        _roomLogger.LogDebug("Adding {Count} rooms to database", roomList.Count);

        await _dbSet.AddRangeAsync(roomList);

        _roomLogger.LogDebug("Successfully added {Count} rooms to context", roomList.Count);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Room>> GetRoomsInGridAsync(int z, int minX, int maxX, int minY, int maxY)
    {
        _roomLogger.LogDebug(
            "[Room] Fetching rooms in grid Z={Z}, X=[{MinX},{MaxX}], Y=[{MinY},{MaxY}]",
            z, minX, maxX, minY, maxY);

        // Use backing fields for EF Core LINQ translation (v0.3.18a)
        var rooms = await _dbSet
            .Where(r => r.PositionZ == z
                     && r.PositionX >= minX && r.PositionX <= maxX
                     && r.PositionY >= minY && r.PositionY <= maxY)
            .ToListAsync();

        _roomLogger.LogDebug("[Room] Retrieved {Count} rooms in grid", rooms.Count);

        return rooms;
    }
}
