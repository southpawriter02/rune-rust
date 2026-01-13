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
    /// Gets the default interaction for an object.
    /// </summary>
    InteractionType GetDefaultInteraction(InteractiveObject obj);
}
