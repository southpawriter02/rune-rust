namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the high-level state of the application loop.
/// Controls what input is valid and how the game responds.
/// </summary>
public enum GamePhase
{
    /// <summary>
    /// The main menu phase. Player can start a new game or quit.
    /// </summary>
    MainMenu = 0,

    /// <summary>
    /// The exploration phase. Player navigates the world.
    /// </summary>
    Exploration = 1,

    /// <summary>
    /// The combat phase. Player engages in turn-based combat.
    /// </summary>
    Combat = 2,

    /// <summary>
    /// The quit phase. Signals the game loop to terminate.
    /// </summary>
    Quit = 3
}
