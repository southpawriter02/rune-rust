using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

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
    public void ParseAndExecute(string input, GameState state)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _logger.LogTrace("Empty input received, ignoring.");
            return;
        }

        var command = input.Trim().ToLowerInvariant();
        _logger.LogDebug("Parsing command: '{Command}' in Phase: {Phase}", command, state.Phase);

        switch (state.Phase)
        {
            case GamePhase.MainMenu:
                HandleMainMenu(command, state);
                break;
            case GamePhase.Exploration:
                HandleExploration(command, state);
                break;
            case GamePhase.Combat:
                HandleCombat(command, state);
                break;
            default:
                _logger.LogWarning("Input received in unhandled phase: {Phase}", state.Phase);
                break;
        }
    }

    /// <summary>
    /// Handles commands in the MainMenu phase.
    /// </summary>
    private void HandleMainMenu(string command, GameState state)
    {
        switch (command)
        {
            case "start":
            case "new":
            case "play":
                state.Phase = GamePhase.Exploration;
                state.IsSessionActive = true;
                state.TurnCount = 0;
                _logger.LogInformation("Game Started. Transitioning to Exploration phase.");
                _inputHandler.DisplayMessage("You awaken in a world touched by the Glitch...");
                break;

            case "load":
                state.PendingAction = PendingGameAction.Load;
                _logger.LogInformation("Load command received from MainMenu.");
                break;

            case "quit":
            case "exit":
            case "q":
                state.Phase = GamePhase.Quit;
                _logger.LogInformation("Quit command received from MainMenu.");
                break;

            case "help":
            case "?":
                DisplayMainMenuHelp();
                break;

            default:
                _inputHandler.DisplayError($"Unknown command: '{command}'. Type 'help' for available commands.");
                _logger.LogDebug("Unknown MainMenu command: {Command}", command);
                break;
        }
    }

    /// <summary>
    /// Handles commands in the Exploration phase.
    /// </summary>
    private void HandleExploration(string command, GameState state)
    {
        switch (command)
        {
            case "quit":
            case "exit":
            case "q":
                state.Phase = GamePhase.Quit;
                _logger.LogInformation("Player quit from Exploration phase.");
                break;

            case "menu":
            case "mainmenu":
                state.Phase = GamePhase.MainMenu;
                state.IsSessionActive = false;
                _logger.LogInformation("Returning to MainMenu from Exploration.");
                _inputHandler.DisplayMessage("Returning to main menu...");
                break;

            case "help":
            case "?":
                DisplayExplorationHelp();
                break;

            case "look":
            case "l":
                _inputHandler.DisplayMessage("You see the remnants of a world once whole. Rust and runes intertwine.");
                state.TurnCount++;
                _logger.LogTrace("Look command executed. Turn: {TurnCount}", state.TurnCount);
                break;

            case "status":
            case "stats":
                DisplayStatus(state);
                break;

            case "save":
                state.PendingAction = PendingGameAction.Save;
                _logger.LogInformation("Save command received.");
                break;

            case "load":
                state.PendingAction = PendingGameAction.Load;
                _logger.LogInformation("Load command received.");
                break;

            default:
                _inputHandler.DisplayError($"Unknown command: '{command}'. Type 'help' for available commands.");
                _logger.LogDebug("Unknown Exploration command: {Command}", command);
                break;
        }
    }

    /// <summary>
    /// Handles commands in the Combat phase (placeholder for v0.0.3).
    /// </summary>
    private void HandleCombat(string command, GameState state)
    {
        switch (command)
        {
            case "quit":
            case "exit":
            case "q":
                state.Phase = GamePhase.Quit;
                _logger.LogInformation("Player quit from Combat phase.");
                break;

            case "flee":
            case "run":
                state.Phase = GamePhase.Exploration;
                _logger.LogInformation("Player fled combat. Returning to Exploration.");
                _inputHandler.DisplayMessage("You flee from the encounter!");
                break;

            case "help":
            case "?":
                DisplayCombatHelp();
                break;

            default:
                _inputHandler.DisplayError($"Unknown combat command: '{command}'. Type 'help' for available commands.");
                _logger.LogDebug("Unknown Combat command: {Command}", command);
                break;
        }
    }

    /// <summary>
    /// Displays help for the MainMenu phase.
    /// </summary>
    private void DisplayMainMenuHelp()
    {
        _inputHandler.DisplayMessage("=== MAIN MENU ===");
        _inputHandler.DisplayMessage("  start, new, play - Start a new game");
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
        _inputHandler.DisplayMessage("  look, l          - Examine your surroundings");
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
