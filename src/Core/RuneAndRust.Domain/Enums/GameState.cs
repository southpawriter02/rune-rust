namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the current state of a game session.
/// </summary>
public enum GameState
{
    /// <summary>
    /// The player is at the main menu.
    /// </summary>
    MainMenu,

    /// <summary>
    /// The game is actively being played.
    /// </summary>
    Playing,

    /// <summary>
    /// The game is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The player has been defeated.
    /// </summary>
    GameOver,

    /// <summary>
    /// The player has completed the dungeon successfully.
    /// </summary>
    Victory
}
