namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents a pending asynchronous game action requested by the player.
/// Used to signal save/load operations from the synchronous command parser
/// to the async game loop.
/// </summary>
public enum PendingGameAction
{
    /// <summary>
    /// No pending action.
    /// </summary>
    None = 0,

    /// <summary>
    /// Save the current game state.
    /// </summary>
    Save = 1,

    /// <summary>
    /// Load a saved game state.
    /// </summary>
    Load = 2
}
