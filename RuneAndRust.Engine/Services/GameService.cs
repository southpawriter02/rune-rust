using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// The main game service implementation.
/// Handles game initialization and the core game loop.
/// </summary>
public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    private readonly IInputHandler _inputHandler;
    private readonly CommandParser _parser;
    private readonly GameState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="inputHandler">The input handler for user interaction.</param>
    /// <param name="parser">The command parser for processing input.</param>
    /// <param name="state">The game state singleton.</param>
    public GameService(
        ILogger<GameService> logger,
        IInputHandler inputHandler,
        CommandParser parser,
        GameState state)
    {
        _logger = logger;
        _inputHandler = inputHandler;
        _parser = parser;
        _state = state;
    }

    /// <inheritdoc/>
    public async Task StartAsync()
    {
        _logger.LogInformation("Game Loop Initialized.");
        _inputHandler.DisplayMessage("Welcome to Rune & Rust!");
        _inputHandler.DisplayMessage("Type 'help' for available commands.");
        _inputHandler.DisplayMessage("");

        while (_state.Phase != GamePhase.Quit)
        {
            // 1. Determine prompt based on current phase
            string prompt = GetPhasePrompt();

            // 2. Get input from the user (abstracted for testability)
            string input = _inputHandler.GetInput(prompt);

            // 3. Process the input through the command parser
            await _parser.ParseAndExecuteAsync(input, _state);
        }

        _logger.LogInformation("Game Loop Ended. Shutting down.");
        _inputHandler.DisplayMessage("Thank you for playing Rune & Rust. Farewell!");
    }

    /// <summary>
    /// Returns the appropriate prompt string based on the current game phase.
    /// </summary>
    private string GetPhasePrompt()
    {
        return _state.Phase switch
        {
            GamePhase.MainMenu => "[MENU]",
            GamePhase.Exploration => "[EXPLORE]",
            GamePhase.Combat => "[COMBAT]",
            _ => "[???]"
        };
    }
}
