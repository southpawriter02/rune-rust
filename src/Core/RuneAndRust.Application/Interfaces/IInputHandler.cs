using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Base record for all game commands parsed from user input.
/// </summary>
public abstract record GameCommand;

/// <summary>
/// Command to move the player in a cardinal direction.
/// </summary>
/// <param name="Direction">The direction to move (North, South, East, or West).</param>
public record MoveCommand(Direction Direction) : GameCommand;

/// <summary>
/// Command to examine the current room in detail.
/// </summary>
public record LookCommand : GameCommand;

/// <summary>
/// Command to display the player's inventory contents.
/// </summary>
public record InventoryCommand : GameCommand;

/// <summary>
/// Command to pick up an item from the current room.
/// </summary>
/// <param name="ItemName">The name of the item to pick up.</param>
public record TakeCommand(string ItemName) : GameCommand;

/// <summary>
/// Command to attack a monster in the current room.
/// </summary>
public record AttackCommand : GameCommand;

/// <summary>
/// Command to save the current game session.
/// </summary>
public record SaveCommand : GameCommand;

/// <summary>
/// Command to load a previously saved game session.
/// </summary>
public record LoadCommand : GameCommand;

/// <summary>
/// Command to display help information about available commands.
/// </summary>
public record HelpCommand : GameCommand;

/// <summary>
/// Command to quit the current game session.
/// </summary>
public record QuitCommand : GameCommand;

/// <summary>
/// Represents an unrecognized or invalid command.
/// </summary>
/// <param name="Input">The original user input that could not be parsed.</param>
public record UnknownCommand(string Input) : GameCommand;

/// <summary>
/// Command to drop an item from inventory into the current room.
/// </summary>
/// <param name="ItemName">The name of the item to drop.</param>
public record DropCommand(string ItemName) : GameCommand;

/// <summary>
/// Command to use/consume an item from inventory.
/// </summary>
/// <param name="ItemName">The name of the item to use.</param>
public record UseCommand(string ItemName) : GameCommand;

/// <summary>
/// Command to examine an item, monster, or room feature in detail.
/// </summary>
/// <param name="Target">The name of the target to examine.</param>
public record ExamineCommand(string Target) : GameCommand;

/// <summary>
/// Command to display detailed player statistics.
/// </summary>
public record StatusCommand : GameCommand;

/// <summary>
/// Command to display the player's abilities.
/// </summary>
public record AbilitiesCommand : GameCommand;

/// <summary>
/// Command to use an ability.
/// </summary>
/// <param name="AbilityName">The name or ID of the ability to use.</param>
public record UseAbilityCommand(string AbilityName) : GameCommand;

/// <summary>
/// Defines the contract for handling user input and converting it to game commands.
/// </summary>
/// <remarks>
/// Implementations of this interface handle reading user input from various sources
/// (console, GUI widgets) and parsing it into structured game commands.
/// </remarks>
public interface IInputHandler
{
    /// <summary>
    /// Waits for and parses the next game command from user input.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A <see cref="GameCommand"/> representing the user's intent.</returns>
    Task<GameCommand> GetNextCommandAsync(CancellationToken ct = default);

    /// <summary>
    /// Prompts the user for free-form text input.
    /// </summary>
    /// <param name="prompt">The prompt message to display.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>The text entered by the user.</returns>
    Task<string> GetTextInputAsync(string prompt, CancellationToken ct = default);

    /// <summary>
    /// Prompts the user to select from a list of options.
    /// </summary>
    /// <typeparam name="T">The type of options to select from.</typeparam>
    /// <param name="prompt">The prompt message to display.</param>
    /// <param name="options">The available options to choose from.</param>
    /// <param name="displaySelector">A function to convert options to display strings.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>The selected option.</returns>
    Task<T> GetSelectionAsync<T>(string prompt, IEnumerable<T> options, Func<T, string> displaySelector, CancellationToken ct = default) where T : notnull;

    /// <summary>
    /// Prompts the user for a yes/no confirmation.
    /// </summary>
    /// <param name="prompt">The confirmation question to display.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns><c>true</c> if the user confirmed; otherwise, <c>false</c>.</returns>
    Task<bool> GetConfirmationAsync(string prompt, CancellationToken ct = default);
}
