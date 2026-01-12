using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for handling object interactions.
/// </summary>
public interface IInteractionService
{
    /// <summary>
    /// Performs the default interaction on an object.
    /// </summary>
    InteractionResult Interact(InteractiveObject obj);

    /// <summary>
    /// Performs a specific interaction on an object.
    /// </summary>
    InteractionResult Interact(InteractiveObject obj, InteractionType type);

    /// <summary>
    /// Opens an object.
    /// </summary>
    InteractionResult Open(InteractiveObject obj);

    /// <summary>
    /// Closes an object.
    /// </summary>
    InteractionResult Close(InteractiveObject obj);

    /// <summary>
    /// Examines an object.
    /// </summary>
    InteractionResult Examine(InteractiveObject obj);

    /// <summary>
    /// Finds an object by keyword in a collection.
    /// </summary>
    InteractiveObject? FindObject(IEnumerable<InteractiveObject> objects, string keyword);

    /// <summary>
    /// Finds an object by keyword in a room.
    /// </summary>
    /// <param name="room">The room to search.</param>
    /// <param name="keyword">The keyword to search for.</param>
    /// <returns>The matching object, or null if not found.</returns>
    InteractiveObject? FindObject(Room room, string keyword);

    /// <summary>
    /// Gets the default interaction for an object.
    /// </summary>
    InteractionType GetDefaultInteraction(InteractiveObject obj);

    /// <summary>
    /// Gets a formatted description of visible objects in a room.
    /// </summary>
    /// <param name="room">The room to describe.</param>
    /// <returns>A formatted string listing visible objects, or empty if no objects.</returns>
    string GetRoomObjectsDescription(Room room);

    // ===== Lock Methods (v0.4.0b) =====

    /// <summary>
    /// Attempts to unlock an interactive object using a key from player inventory.
    /// </summary>
    /// <param name="obj">The object to unlock.</param>
    /// <param name="player">The player attempting to unlock.</param>
    /// <returns>The unlock result.</returns>
    UnlockResult UnlockWithKey(InteractiveObject obj, Player player);

    /// <summary>
    /// Attempts to pick the lock on an interactive object.
    /// </summary>
    /// <param name="obj">The object to unlock.</param>
    /// <param name="player">The player attempting to pick the lock.</param>
    /// <returns>The unlock result.</returns>
    UnlockResult PickLock(InteractiveObject obj, Player player);

    /// <summary>
    /// Attempts to lock an interactive object.
    /// </summary>
    /// <param name="obj">The object to lock.</param>
    /// <returns>The interaction result.</returns>
    InteractionResult Lock(InteractiveObject obj);

    // ===== Container Methods (v0.4.0b) =====

    /// <summary>
    /// Takes an item from a container.
    /// </summary>
    /// <param name="container">The container to take from.</param>
    /// <param name="itemName">The name of the item to take.</param>
    /// <param name="player">The player taking the item.</param>
    /// <returns>The interaction result.</returns>
    InteractionResult TakeFromContainer(InteractiveObject container, string itemName, Player player);

    /// <summary>
    /// Takes all items from a container.
    /// </summary>
    /// <param name="container">The container to empty.</param>
    /// <param name="player">The player taking the items.</param>
    /// <returns>The interaction result with list of items taken.</returns>
    InteractionResult TakeAllFromContainer(InteractiveObject container, Player player);

    /// <summary>
    /// Puts an item into a container.
    /// </summary>
    /// <param name="container">The container to put the item in.</param>
    /// <param name="itemName">The name of the item to put.</param>
    /// <param name="player">The player putting the item.</param>
    /// <returns>The interaction result.</returns>
    InteractionResult PutInContainer(InteractiveObject container, string itemName, Player player);

    /// <summary>
    /// Gets the contents description of a container.
    /// </summary>
    /// <param name="container">The container to describe.</param>
    /// <returns>The contents description.</returns>
    string GetContainerContents(InteractiveObject container);
}
