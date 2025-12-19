using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for interactable object repository operations.
/// Extends IRepository with room-based queries.
/// </summary>
public interface IInteractableObjectRepository : IRepository<InteractableObject>
{
    /// <summary>
    /// Gets all interactable objects in a specific room.
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    /// <returns>A collection of objects in the room.</returns>
    Task<IEnumerable<InteractableObject>> GetByRoomIdAsync(Guid roomId);

    /// <summary>
    /// Gets an object by name within a specific room.
    /// Uses case-insensitive matching.
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    /// <param name="name">The object name to search for.</param>
    /// <returns>The object if found, null otherwise.</returns>
    Task<InteractableObject?> GetByNameInRoomAsync(Guid roomId, string name);

    /// <summary>
    /// Gets all container objects in a specific room.
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    /// <returns>A collection of container objects in the room.</returns>
    Task<IEnumerable<InteractableObject>> GetContainersInRoomAsync(Guid roomId);

    /// <summary>
    /// Checks if an object with the given name exists in a room.
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    /// <param name="name">The object name to check.</param>
    /// <returns>True if the object exists, false otherwise.</returns>
    Task<bool> ObjectExistsInRoomAsync(Guid roomId, string name);

    /// <summary>
    /// Adds multiple objects to the repository in a single operation.
    /// </summary>
    /// <param name="objects">The objects to add.</param>
    Task AddRangeAsync(IEnumerable<InteractableObject> objects);

    /// <summary>
    /// Removes all objects from a specific room.
    /// Used when regenerating dungeons.
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    Task ClearRoomObjectsAsync(Guid roomId);
}
