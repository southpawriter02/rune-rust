using RuneAndRust.Core.Entities;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository interface for Room entity operations.
/// Extends the generic repository with room-specific queries.
/// </summary>
public interface IRoomRepository : IRepository<Room>
{
    /// <summary>
    /// Gets a room by its 3D coordinate position.
    /// </summary>
    /// <param name="position">The coordinate to search for.</param>
    /// <returns>The room at that position, or null if none exists.</returns>
    Task<Room?> GetByPositionAsync(Coordinate position);

    /// <summary>
    /// Gets the designated starting room for new games.
    /// </summary>
    /// <returns>The starting room, or null if none is designated.</returns>
    Task<Room?> GetStartingRoomAsync();

    /// <summary>
    /// Checks if a room exists at the specified position.
    /// </summary>
    /// <param name="position">The coordinate to check.</param>
    /// <returns>True if a room exists at that position.</returns>
    Task<bool> PositionExistsAsync(Coordinate position);

    /// <summary>
    /// Gets all rooms in the current world/dungeon.
    /// </summary>
    /// <returns>All rooms ordered by position.</returns>
    Task<IEnumerable<Room>> GetAllRoomsAsync();

    /// <summary>
    /// Clears all rooms from the database (used when generating a new world).
    /// </summary>
    Task ClearAllRoomsAsync();

    /// <summary>
    /// Adds multiple rooms in a single transaction.
    /// </summary>
    /// <param name="rooms">The rooms to add.</param>
    Task AddRangeAsync(IEnumerable<Room> rooms);

    /// <summary>
    /// Gets all rooms within a coordinate grid on a specific Z-level (v0.3.5b).
    /// Used for minimap rendering to batch-fetch local rooms.
    /// </summary>
    /// <param name="z">The Z-level to query.</param>
    /// <param name="minX">Minimum X coordinate (inclusive).</param>
    /// <param name="maxX">Maximum X coordinate (inclusive).</param>
    /// <param name="minY">Minimum Y coordinate (inclusive).</param>
    /// <param name="maxY">Maximum Y coordinate (inclusive).</param>
    /// <returns>All rooms within the specified grid bounds.</returns>
    Task<IEnumerable<Room>> GetRoomsInGridAsync(int z, int minX, int maxX, int minY, int maxY);
}
