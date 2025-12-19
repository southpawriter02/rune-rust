using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Represents the result of parsing a command, indicating if async follow-up is needed.
/// </summary>
public class ParseResult
{
    /// <summary>
    /// Gets or sets whether the command requires async navigation.
    /// </summary>
    public bool RequiresNavigation { get; set; }

    /// <summary>
    /// Gets or sets the direction for navigation commands.
    /// </summary>
    public Direction? NavigationDirection { get; set; }

    /// <summary>
    /// Gets or sets whether the look command was issued.
    /// </summary>
    public bool RequiresLook { get; set; }

    /// <summary>
    /// Gets or sets whether character creation should be triggered.
    /// </summary>
    public bool RequiresCharacterCreation { get; set; }

    /// <summary>
    /// Gets or sets whether an examine command was issued.
    /// </summary>
    public bool RequiresExamine { get; set; }

    /// <summary>
    /// Gets or sets the target of the examine command.
    /// </summary>
    public string? ExamineTarget { get; set; }

    /// <summary>
    /// Gets or sets whether an open command was issued.
    /// </summary>
    public bool RequiresOpen { get; set; }

    /// <summary>
    /// Gets or sets the target of the open command.
    /// </summary>
    public string? OpenTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a close command was issued.
    /// </summary>
    public bool RequiresClose { get; set; }

    /// <summary>
    /// Gets or sets the target of the close command.
    /// </summary>
    public string? CloseTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a search command was issued.
    /// </summary>
    public bool RequiresSearch { get; set; }

    /// <summary>
    /// Gets or sets whether a list objects command was issued.
    /// </summary>
    public bool RequiresListObjects { get; set; }

    /// <summary>
    /// Gets or sets whether the inventory display was requested.
    /// </summary>
    public bool RequiresInventory { get; set; }

    /// <summary>
    /// Gets or sets whether the equipment display was requested.
    /// </summary>
    public bool RequiresEquipment { get; set; }

    /// <summary>
    /// Gets or sets whether an equip command was issued.
    /// </summary>
    public bool RequiresEquip { get; set; }

    /// <summary>
    /// Gets or sets the target of the equip command.
    /// </summary>
    public string? EquipTarget { get; set; }

    /// <summary>
    /// Gets or sets whether an unequip command was issued.
    /// </summary>
    public bool RequiresUnequip { get; set; }

    /// <summary>
    /// Gets or sets the target of the unequip command (item name or slot).
    /// </summary>
    public string? UnequipTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a drop command was issued.
    /// </summary>
    public bool RequiresDrop { get; set; }

    /// <summary>
    /// Gets or sets the target of the drop command.
    /// </summary>
    public string? DropTarget { get; set; }

    /// <summary>
    /// Gets a default result with no async requirements.
    /// </summary>
    public static ParseResult None => new();
}

/// <summary>
/// Parses raw user input and executes commands based on the current game phase.
/// Implements a simple state machine pattern for command handling.
/// </summary>
public class CommandParser
{
    private readonly ILogger<CommandParser> _logger;
    private readonly IInputHandler _inputHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandParser"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="inputHandler">The input handler for displaying messages.</param>
    public CommandParser(ILogger<CommandParser> logger, IInputHandler inputHandler)
    {
        _logger = logger;
        _inputHandler = inputHandler;
    }

    /// <summary>
    /// Parses raw input and mutates GameState based on the current phase context.
    /// </summary>
    /// <param name="input">The raw user input string.</param>
    /// <param name="state">The current game state to potentially modify.</param>
    /// <returns>A ParseResult indicating any async operations needed.</returns>
    public ParseResult ParseAndExecute(string input, GameState state)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _logger.LogTrace("Empty input received, ignoring.");
            return ParseResult.None;
        }

        var command = input.Trim().ToLowerInvariant();
        _logger.LogDebug("Parsing command: '{Command}' in Phase: {Phase}", command, state.Phase);

        ParseResult result;
        switch (state.Phase)
        {
            case GamePhase.MainMenu:
                result = HandleMainMenu(command, state);
                break;
            case GamePhase.Exploration:
                result = HandleExploration(command, state);
                break;
            case GamePhase.Combat:
                result = HandleCombat(command, state);
                break;
            default:
                _logger.LogWarning("Input received in unhandled phase: {Phase}", state.Phase);
                result = ParseResult.None;
                break;
        }

        return result;
    }

    /// <summary>
    /// Handles commands in the MainMenu phase.
    /// </summary>
    private ParseResult HandleMainMenu(string command, GameState state)
    {
        switch (command)
        {
            case "new":
            case "create":
                _logger.LogInformation("Character creation requested from MainMenu.");
                return new ParseResult { RequiresCharacterCreation = true };

            case "start":
            case "play":
                state.Phase = GamePhase.Exploration;
                state.IsSessionActive = true;
                state.TurnCount = 0;
                _logger.LogInformation("Game Started. Transitioning to Exploration phase.");
                // Note: The actual room description will be shown by the game loop
                // after it initializes the dungeon and sets CurrentRoomId
                return new ParseResult { RequiresLook = true };

            case "load":
                state.PendingAction = PendingGameAction.Load;
                _logger.LogInformation("Load command received from MainMenu.");
                return ParseResult.None;

            case "quit":
            case "exit":
            case "q":
                state.Phase = GamePhase.Quit;
                _logger.LogInformation("Quit command received from MainMenu.");
                return ParseResult.None;

            case "help":
            case "?":
                DisplayMainMenuHelp();
                return ParseResult.None;

            default:
                _inputHandler.DisplayError($"Unknown command: '{command}'. Type 'help' for available commands.");
                _logger.LogDebug("Unknown MainMenu command: {Command}", command);
                return ParseResult.None;
        }
    }

    /// <summary>
    /// Handles commands in the Exploration phase.
    /// </summary>
    private ParseResult HandleExploration(string command, GameState state)
    {
        // Check for direction aliases first
        var direction = ParseDirection(command);
        if (direction.HasValue)
        {
            _logger.LogDebug("Movement command: {Direction}", direction.Value);
            return new ParseResult
            {
                RequiresNavigation = true,
                NavigationDirection = direction.Value
            };
        }

        // Check for "go <direction>" format
        if (command.StartsWith("go "))
        {
            var dirString = command.Substring(3).Trim();
            direction = ParseDirection(dirString);
            if (direction.HasValue)
            {
                _logger.LogDebug("Go command: {Direction}", direction.Value);
                return new ParseResult
                {
                    RequiresNavigation = true,
                    NavigationDirection = direction.Value
                };
            }
            else
            {
                _inputHandler.DisplayError($"Unknown direction: '{dirString}'. Valid directions: north, south, east, west, up, down.");
                return ParseResult.None;
            }
        }

        // Check for examine command with target
        if (command.StartsWith("examine ") || command.StartsWith("x ") || command.StartsWith("look "))
        {
            var target = ExtractTarget(command, new[] { "examine ", "x ", "look " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Examine command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresExamine = true,
                    ExamineTarget = target
                };
            }
        }

        // Check for open command with target
        if (command.StartsWith("open "))
        {
            var target = command.Substring(5).Trim();
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Open command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresOpen = true,
                    OpenTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Open what? Specify a target.");
                return ParseResult.None;
            }
        }

        // Check for close command with target
        if (command.StartsWith("close "))
        {
            var target = command.Substring(6).Trim();
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Close command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresClose = true,
                    CloseTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Close what? Specify a target.");
                return ParseResult.None;
            }
        }

        // Check for equip command with target
        if (command.StartsWith("equip ") || command.StartsWith("bind "))
        {
            var target = ExtractTarget(command, new[] { "equip ", "bind " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                _logger.LogDebug("Equip command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresEquip = true,
                    EquipTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Equip what? Specify an item.");
                return ParseResult.None;
            }
        }

        // Check for unequip command with target
        if (command.StartsWith("unequip ") || command.StartsWith("unbind ") || command.StartsWith("remove "))
        {
            var target = ExtractTarget(command, new[] { "unequip ", "unbind ", "remove " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                _logger.LogDebug("Unequip command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresUnequip = true,
                    UnequipTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Unequip what? Specify an item or slot.");
                return ParseResult.None;
            }
        }

        // Check for drop command with target
        if (command.StartsWith("drop ") || command.StartsWith("discard "))
        {
            var target = ExtractTarget(command, new[] { "drop ", "discard " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                _logger.LogDebug("Drop command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresDrop = true,
                    DropTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Drop what? Specify an item.");
                return ParseResult.None;
            }
        }

        switch (command)
        {
            case "quit":
            case "exit":
            case "q":
                state.Phase = GamePhase.Quit;
                _logger.LogInformation("Player quit from Exploration phase.");
                return ParseResult.None;

            case "menu":
            case "mainmenu":
                state.Phase = GamePhase.MainMenu;
                state.IsSessionActive = false;
                state.CurrentRoomId = null;
                _logger.LogInformation("Returning to MainMenu from Exploration.");
                _inputHandler.DisplayMessage("Returning to main menu...");
                return ParseResult.None;

            case "help":
            case "?":
                DisplayExplorationHelp();
                return ParseResult.None;

            case "look":
            case "l":
                state.TurnCount++;
                _logger.LogTrace("Look command executed. Turn: {TurnCount}", state.TurnCount);
                return new ParseResult { RequiresLook = true };

            case "exits":
                _logger.LogTrace("Exits command executed.");
                return new ParseResult { RequiresLook = true };

            case "status":
            case "stats":
                DisplayStatus(state);
                return ParseResult.None;

            case "save":
                state.PendingAction = PendingGameAction.Save;
                _logger.LogInformation("Save command received.");
                return ParseResult.None;

            case "load":
                state.PendingAction = PendingGameAction.Load;
                _logger.LogInformation("Load command received.");
                return ParseResult.None;

            case "search":
                state.TurnCount++;
                _logger.LogDebug("Search command executed.");
                return new ParseResult { RequiresSearch = true };

            case "objects":
            case "items":
                _logger.LogDebug("List objects command executed.");
                return new ParseResult { RequiresListObjects = true };

            case "inventory":
            case "i":
            case "pack":
                _logger.LogDebug("Inventory display command executed.");
                return new ParseResult { RequiresInventory = true };

            case "equipment":
            case "gear":
            case "equipped":
                _logger.LogDebug("Equipment display command executed.");
                return new ParseResult { RequiresEquipment = true };

            default:
                _inputHandler.DisplayError($"Unknown command: '{command}'. Type 'help' for available commands.");
                _logger.LogDebug("Unknown Exploration command: {Command}", command);
                return ParseResult.None;
        }
    }

    /// <summary>
    /// Parses a direction string or alias into a Direction enum.
    /// </summary>
    /// <param name="input">The direction string to parse.</param>
    /// <returns>The parsed direction, or null if invalid.</returns>
    private Direction? ParseDirection(string input)
    {
        return input switch
        {
            "north" or "n" => Direction.North,
            "south" or "s" => Direction.South,
            "east" or "e" => Direction.East,
            "west" or "w" => Direction.West,
            "up" or "u" => Direction.Up,
            "down" or "d" => Direction.Down,
            _ => null
        };
    }

    /// <summary>
    /// Extracts the target from a command that starts with one of the given prefixes.
    /// </summary>
    /// <param name="command">The full command string.</param>
    /// <param name="prefixes">The possible command prefixes.</param>
    /// <returns>The target string, or null if not found.</returns>
    private static string? ExtractTarget(string command, string[] prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (command.StartsWith(prefix))
            {
                return command.Substring(prefix.Length).Trim();
            }
        }
        return null;
    }

    /// <summary>
    /// Handles commands in the Combat phase (placeholder for v0.0.3).
    /// </summary>
    private ParseResult HandleCombat(string command, GameState state)
    {
        switch (command)
        {
            case "quit":
            case "exit":
            case "q":
                state.Phase = GamePhase.Quit;
                _logger.LogInformation("Player quit from Combat phase.");
                return ParseResult.None;

            case "flee":
            case "run":
                state.Phase = GamePhase.Exploration;
                _logger.LogInformation("Player fled combat. Returning to Exploration.");
                _inputHandler.DisplayMessage("You flee from the encounter!");
                return new ParseResult { RequiresLook = true };

            case "help":
            case "?":
                DisplayCombatHelp();
                return ParseResult.None;

            default:
                _inputHandler.DisplayError($"Unknown combat command: '{command}'. Type 'help' for available commands.");
                _logger.LogDebug("Unknown Combat command: {Command}", command);
                return ParseResult.None;
        }
    }

    /// <summary>
    /// Displays help for the MainMenu phase.
    /// </summary>
    private void DisplayMainMenuHelp()
    {
        _inputHandler.DisplayMessage("=== MAIN MENU ===");
        _inputHandler.DisplayMessage("  new, create      - Create a new character");
        _inputHandler.DisplayMessage("  start, play      - Start the game");
        _inputHandler.DisplayMessage("  load             - Load a saved game");
        _inputHandler.DisplayMessage("  help, ?          - Show this help");
        _inputHandler.DisplayMessage("  quit, exit, q    - Exit the game");
    }

    /// <summary>
    /// Displays help for the Exploration phase.
    /// </summary>
    private void DisplayExplorationHelp()
    {
        _inputHandler.DisplayMessage("=== EXPLORATION ===");
        _inputHandler.DisplayMessage("Movement:");
        _inputHandler.DisplayMessage("  n, s, e, w       - Move north, south, east, or west");
        _inputHandler.DisplayMessage("  u, d             - Move up or down");
        _inputHandler.DisplayMessage("  go <direction>   - Move in a direction");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Interaction:");
        _inputHandler.DisplayMessage("  examine <target> - Examine an object (uses WITS)");
        _inputHandler.DisplayMessage("  x <target>       - Shorthand for examine");
        _inputHandler.DisplayMessage("  look <target>    - Examine an object");
        _inputHandler.DisplayMessage("  search           - Search the room for objects (uses WITS)");
        _inputHandler.DisplayMessage("  open <target>    - Open a container");
        _inputHandler.DisplayMessage("  close <target>   - Close a container");
        _inputHandler.DisplayMessage("  objects          - List visible objects in the room");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Inventory:");
        _inputHandler.DisplayMessage("  inventory, i     - Display your inventory and burden");
        _inputHandler.DisplayMessage("  equipment, gear  - Display equipped items");
        _inputHandler.DisplayMessage("  equip <item>     - Equip an item from inventory");
        _inputHandler.DisplayMessage("  unequip <slot>   - Unequip item from slot (e.g., mainhand)");
        _inputHandler.DisplayMessage("  drop <item>      - Drop an item from inventory");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Actions:");
        _inputHandler.DisplayMessage("  look, l          - Examine your surroundings");
        _inputHandler.DisplayMessage("  exits            - Show available exits");
        _inputHandler.DisplayMessage("  status, stats    - View your current status");
        _inputHandler.DisplayMessage("  save             - Save your progress");
        _inputHandler.DisplayMessage("  load             - Load a saved game");
        _inputHandler.DisplayMessage("  menu, mainmenu   - Return to main menu");
        _inputHandler.DisplayMessage("  help, ?          - Show this help");
        _inputHandler.DisplayMessage("  quit, exit, q    - Exit the game");
    }

    /// <summary>
    /// Displays help for the Combat phase.
    /// </summary>
    private void DisplayCombatHelp()
    {
        _inputHandler.DisplayMessage("=== COMBAT ===");
        _inputHandler.DisplayMessage("  flee, run        - Attempt to flee combat");
        _inputHandler.DisplayMessage("  help, ?          - Show this help");
        _inputHandler.DisplayMessage("  quit, exit, q    - Exit the game");
    }

    /// <summary>
    /// Displays the current game status.
    /// </summary>
    private void DisplayStatus(GameState state)
    {
        _inputHandler.DisplayMessage($"=== STATUS ===");
        _inputHandler.DisplayMessage($"  Phase: {state.Phase}");
        _inputHandler.DisplayMessage($"  Turn: {state.TurnCount}");
        _inputHandler.DisplayMessage($"  Session Active: {state.IsSessionActive}");

        if (state.CurrentCharacter != null)
        {
            _inputHandler.DisplayMessage($"  Character: {state.CurrentCharacter.Name}");
        }
        else
        {
            _inputHandler.DisplayMessage("  Character: None");
        }
    }
}
