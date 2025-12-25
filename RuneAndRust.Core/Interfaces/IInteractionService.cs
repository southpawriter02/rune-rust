using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for player-object interaction operations.
/// Handles examine, open, close, search, and loot commands.
/// </summary>
/// <remarks>See: SPEC-INTERACT-001 for Interaction System design.</remarks>
public interface IInteractionService
{
    /// <summary>
    /// Examines an object using a WITS-based dice check.
    /// Higher WITS values reveal more detailed information.
    /// </summary>
    /// <param name="targetName">The name of the object to examine.</param>
    /// <returns>An ExaminationResult containing the revealed description and roll data.</returns>
    Task<ExaminationResult> ExamineAsync(string targetName);

    /// <summary>
    /// Attempts to open a container or similar object.
    /// </summary>
    /// <param name="targetName">The name of the object to open.</param>
    /// <returns>A narrative description of the result.</returns>
    Task<string> OpenAsync(string targetName);

    /// <summary>
    /// Attempts to close a container or similar object.
    /// </summary>
    /// <param name="targetName">The name of the object to close.</param>
    /// <returns>A narrative description of the result.</returns>
    Task<string> CloseAsync(string targetName);

    /// <summary>
    /// Searches the current room for hidden objects.
    /// Uses a WITS-based check to reveal concealed items.
    /// </summary>
    /// <returns>A narrative description of what was found.</returns>
    Task<string> SearchAsync();

    /// <summary>
    /// Gets all visible objects in the current room.
    /// </summary>
    /// <returns>A collection of visible interactable objects.</returns>
    Task<IEnumerable<InteractableObject>> GetVisibleObjectsAsync();

    /// <summary>
    /// Lists the objects in the current room in a formatted string.
    /// </summary>
    /// <returns>A narrative list of objects, or a message if the room is empty.</returns>
    Task<string> ListObjectsAsync();

    /// <summary>
    /// Searches an open container for loot.
    /// Generates loot based on biome, danger level, and container tier.
    /// </summary>
    /// <param name="targetName">The name of the container to search.</param>
    /// <returns>A loot result containing any found items.</returns>
    Task<LootResult> SearchContainerAsync(string targetName);

    /// <summary>
    /// Attempts to take an item from the current room or container.
    /// </summary>
    /// <param name="itemName">The name of the item to take.</param>
    /// <returns>A narrative description of the result.</returns>
    Task<string> TakeItemAsync(string itemName);

    /// <summary>
    /// Gets all items available in the current room or open containers.
    /// </summary>
    /// <returns>A collection of available items.</returns>
    Task<IEnumerable<Item>> GetAvailableItemsAsync();
}
