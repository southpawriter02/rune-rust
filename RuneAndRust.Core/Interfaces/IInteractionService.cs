using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for player-object interaction operations.
/// Handles examine, open, close, and search commands.
/// </summary>
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
}
