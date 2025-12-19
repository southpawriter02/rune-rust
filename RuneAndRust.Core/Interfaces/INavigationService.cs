using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Provides navigation capabilities for room-to-room movement.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Attempts to move the player in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to move.</param>
    /// <returns>A result message describing the outcome (success or blocked).</returns>
    Task<string> MoveAsync(Direction direction);

    /// <summary>
    /// Gets the description of the current room.
    /// </summary>
    /// <returns>The current room's formatted description.</returns>
    Task<string> LookAsync();

    /// <summary>
    /// Gets the current room the player is in.
    /// </summary>
    /// <returns>The current room, or null if not in a valid room.</returns>
    Task<Room?> GetCurrentRoomAsync();

    /// <summary>
    /// Gets a list of available exits from the current room.
    /// </summary>
    /// <returns>Collection of available directions.</returns>
    Task<IEnumerable<Direction>> GetAvailableExitsAsync();
}
