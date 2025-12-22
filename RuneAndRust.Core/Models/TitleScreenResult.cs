using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Encapsulates the result of the title screen interaction (v0.3.4a).
/// </summary>
public class TitleScreenResult
{
    /// <summary>
    /// The menu option selected by the user.
    /// </summary>
    public MainMenuOption SelectedOption { get; private init; }

    /// <summary>
    /// The slot number of the save game to load (only set when SelectedOption is LoadGame).
    /// </summary>
    public int? SaveSlotNumber { get; private init; }

    private TitleScreenResult() { }

    /// <summary>
    /// Creates a result indicating the user wants to start a new game.
    /// </summary>
    public static TitleScreenResult CreateNewGame() =>
        new() { SelectedOption = MainMenuOption.NewGame };

    /// <summary>
    /// Creates a result indicating the user wants to load a saved game.
    /// </summary>
    /// <param name="slotNumber">The slot number of the save game to load.</param>
    public static TitleScreenResult LoadGame(int slotNumber) =>
        new() { SelectedOption = MainMenuOption.LoadGame, SaveSlotNumber = slotNumber };

    /// <summary>
    /// Creates a result indicating the user wants to quit the application.
    /// </summary>
    public static TitleScreenResult Quit() =>
        new() { SelectedOption = MainMenuOption.Quit };
}
