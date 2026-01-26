using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Repository interface for hidden element data access.
/// </summary>
/// <remarks>
/// <para>
/// Provides queries for hidden elements filtered by room and revelation status.
/// Used by <see cref="IPassivePerceptionService"/> for room entry checks.
/// </para>
/// </remarks>
public interface IHiddenElementRepository
{
    /// <summary>
    /// Gets a hidden element by its configuration-based element ID.
    /// </summary>
    /// <param name="elementId">The element ID from configuration (e.g., "crypt-pressure-plate-01").</param>
    /// <returns>The hidden element, or null if not found.</returns>
    HiddenElement? GetByElementId(string elementId);

    /// <summary>
    /// Gets all unrevealed hidden elements in a room.
    /// </summary>
    /// <param name="roomId">The room's database ID.</param>
    /// <returns>List of unrevealed hidden elements in the room.</returns>
    IReadOnlyList<HiddenElement> GetUnrevealedByRoomId(Guid roomId);

    /// <summary>
    /// Gets all hidden elements in a room regardless of revelation status.
    /// </summary>
    /// <param name="roomId">The room's database ID.</param>
    /// <returns>List of all hidden elements in the room.</returns>
    IReadOnlyList<HiddenElement> GetByRoomId(Guid roomId);

    /// <summary>
    /// Gets a hidden element by its database ID.
    /// </summary>
    /// <param name="id">The element's database GUID.</param>
    /// <returns>The hidden element, or null if not found.</returns>
    HiddenElement? GetById(Guid id);

    /// <summary>
    /// Adds a hidden element to the repository.
    /// </summary>
    /// <param name="element">The element to add.</param>
    void Add(HiddenElement element);

    /// <summary>
    /// Updates a hidden element in the repository.
    /// </summary>
    /// <param name="element">The element to update.</param>
    void Update(HiddenElement element);
}
