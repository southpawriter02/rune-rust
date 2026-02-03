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
public record LookCommand(string? Target = null) : GameCommand;
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
public record SearchCommand(string? Target = null) : GameCommand;
public record InvestigateCommand(string Target) : GameCommand;
public record TravelCommand(string? Destination = null) : GameCommand;
public record EnterCommand(string? Location = null) : GameCommand;
public record ExitCommand(string? Direction = null) : GameCommand;
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
/// Command to roll dice directly using dice notation.
/// </summary>
/// <param name="Notation">The dice notation to roll (e.g., "3d6+5", "1d20!", "2d8").</param>
/// <param name="Advantage">Optional advantage/disadvantage modifier.</param>
/// <remarks>
/// Supports standard dice notation including:
/// - Basic rolls: "1d20", "3d6", "2d8"
/// - Modifiers: "1d20+5", "3d6-2"
/// - Exploding dice: "1d6!" (reroll on max)
/// </remarks>
public record RollCommand(string Notation, AdvantageType Advantage = AdvantageType.Normal) : GameCommand;

/// <summary>
/// Command to perform a skill check.
/// </summary>
/// <param name="SkillId">The skill to check (e.g., "perception", "stealth").</param>
/// <param name="DifficultyId">Optional difficulty class ID. If omitted, defaults to "moderate".</param>
/// <param name="Advantage">Optional advantage/disadvantage modifier.</param>
/// <remarks>
/// If the skill ID is not found, displays available skills.
/// If the difficulty ID is not found, displays available difficulty classes.
/// </remarks>
public record SkillCheckCommand(
    string SkillId,
    string? DifficultyId = null,
    AdvantageType Advantage = AdvantageType.Normal) : GameCommand;

/// <summary>
/// Command to equip an item from inventory.
/// </summary>
/// <param name="ItemName">The name of the item to equip.</param>
public record EquipCommand(string ItemName) : GameCommand;

/// <summary>
/// Command to unequip an item from a specific equipment slot.
/// </summary>
/// <param name="Slot">The equipment slot to unequip from.</param>
public record UnequipCommand(EquipmentSlot Slot) : GameCommand;

/// <summary>
/// Command to display the player's current equipment.
/// </summary>
public record EquipmentCommand : GameCommand;

/// <summary>
/// Represents an invalid command with an error message.
/// </summary>
/// <param name="Message">The error message explaining why the command was invalid.</param>
public record InvalidCommand(string Message) : GameCommand;

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
