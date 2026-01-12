namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of interactions with objects.
/// </summary>
public enum InteractionType
{
    /// <summary>
    /// Examine the object for details.
    /// </summary>
    Examine,

    /// <summary>
    /// Open the object (doors, chests, etc.).
    /// </summary>
    Open,

    /// <summary>
    /// Close the object.
    /// </summary>
    Close,

    /// <summary>
    /// Activate the object (levers, buttons, etc.).
    /// </summary>
    Activate,

    /// <summary>
    /// Deactivate the object.
    /// </summary>
    Deactivate,

    /// <summary>
    /// Push the object.
    /// </summary>
    Push,

    /// <summary>
    /// Pull the object.
    /// </summary>
    Pull,

    /// <summary>
    /// Use the object (generic interaction).
    /// </summary>
    Use,

    // v0.4.0a additions:

    /// <summary>
    /// Unlock a locked object with a key.
    /// </summary>
    Unlock,

    /// <summary>
    /// Lock an unlocked object with a key.
    /// </summary>
    Lock,

    /// <summary>
    /// Break/destroy the object.
    /// </summary>
    Break,

    /// <summary>
    /// Search the object for hidden items or secrets.
    /// </summary>
    Search,

    /// <summary>
    /// Take items from a container.
    /// </summary>
    Take,

    /// <summary>
    /// Put items into a container.
    /// </summary>
    Put
}
