using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Adapters;

/// <summary>
/// Console-based implementation of <see cref="IInputHandler"/> using Spectre.Console.
/// </summary>
/// <remarks>
/// ConsoleInputHandler reads text input from the console and parses it into
/// game commands. It supports direction commands, look, inventory, take, attack,
/// save, load, help, and quit commands.
/// </remarks>
public class ConsoleInputHandler : IInputHandler
{
    /// <summary>
    /// Logger for input handling operations and diagnostics.
    /// </summary>
    private readonly ILogger<ConsoleInputHandler> _logger;

    /// <summary>
    /// Creates a new console input handler instance.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics. If null, a no-op logger is used.</param>
    public ConsoleInputHandler(ILogger<ConsoleInputHandler>? logger = null)
    {
        _logger = logger ?? NullLogger<ConsoleInputHandler>.Instance;
        _logger.LogDebug("ConsoleInputHandler initialized");
    }

    /// <inheritdoc/>
    public Task<GameCommand> GetNextCommandAsync(CancellationToken ct = default)
    {
        AnsiConsole.Markup("[grey]> [/]");
        var input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;

        _logger.LogDebug("Raw input received: '{Input}'", input);

        var command = ParseCommand(input);

        _logger.LogDebug("Parsed command: {CommandType} from input '{Input}'", command.GetType().Name, input);

        return Task.FromResult(command);
    }

    /// <summary>
    /// Parses a text input string into a game command.
    /// </summary>
    /// <param name="input">The user input to parse.</param>
    /// <returns>The parsed game command, or UnknownCommand if not recognized.</returns>
    private GameCommand ParseCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _logger.LogDebug("Empty input received");
            return new UnknownCommand(input);
        }

        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0];
        var argument = parts.Length > 1 ? parts[1] : string.Empty;

        _logger.LogDebug("Parsing command: '{Command}', Argument: '{Argument}'", command, argument);

        return command switch
        {
            "n" or "north" => new MoveCommand(Direction.North),
            "s" or "south" => new MoveCommand(Direction.South),
            "e" or "east" => new MoveCommand(Direction.East),
            "w" or "west" => new MoveCommand(Direction.West),
            "look" or "l" => new LookCommand(),
            "inventory" or "inv" or "i" => new InventoryCommand(),
            "take" or "get" or "pick" => string.IsNullOrEmpty(argument)
                ? LogAndReturn(new UnknownCommand(input), "Take command missing item argument")
                : new TakeCommand(argument),
            "drop" => string.IsNullOrEmpty(argument)
                ? LogAndReturn(new UnknownCommand(input), "Drop command missing item argument")
                : new DropCommand(argument),
            "use" or "consume" or "drink" or "eat" => string.IsNullOrEmpty(argument)
                ? LogAndReturn(new UnknownCommand(input), "Use command missing item argument")
                : new UseCommand(argument),
            "examine" or "inspect" or "x" => string.IsNullOrEmpty(argument)
                ? LogAndReturn(new UnknownCommand(input), "Examine command missing target")
                : new ExamineCommand(argument),
            "status" or "stats" or "stat" => new StatusCommand(),
            "attack" or "fight" or "a" => new AttackCommand(),
            "save" => new SaveCommand(),
            "load" => new LoadCommand(),
            "help" or "h" or "?" => new HelpCommand(),
            "quit" or "exit" or "q" => new QuitCommand(),
            _ => LogAndReturn(new UnknownCommand(input), $"Unrecognized command: '{command}'")
        };
    }

    /// <summary>
    /// Logs a warning message and returns the provided command.
    /// </summary>
    /// <param name="command">The command to return.</param>
    /// <param name="logMessage">The message to log.</param>
    /// <returns>The same command that was passed in.</returns>
    private GameCommand LogAndReturn(GameCommand command, string logMessage)
    {
        _logger.LogWarning("{Message}", logMessage);
        return command;
    }

    /// <inheritdoc/>
    public Task<string> GetTextInputAsync(string prompt, CancellationToken ct = default)
    {
        _logger.LogDebug("Prompting for text input: {Prompt}", prompt);
        var result = AnsiConsole.Ask<string>($"[cyan]{prompt}[/]");
        _logger.LogDebug("Text input received: {Input}", result);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<T> GetSelectionAsync<T>(
        string prompt,
        IEnumerable<T> options,
        Func<T, string> displaySelector,
        CancellationToken ct = default) where T : notnull
    {
        _logger.LogDebug("Prompting for selection: {Prompt}", prompt);

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<T>()
                .Title($"[cyan]{prompt}[/]")
                .PageSize(10)
                .UseConverter(displaySelector)
                .AddChoices(options));

        _logger.LogDebug("Selection made: {Selection}", displaySelector(selection));
        return Task.FromResult(selection);
    }

    /// <inheritdoc/>
    public Task<bool> GetConfirmationAsync(string prompt, CancellationToken ct = default)
    {
        _logger.LogDebug("Prompting for confirmation: {Prompt}", prompt);
        var result = AnsiConsole.Confirm($"[cyan]{prompt}[/]");
        _logger.LogDebug("Confirmation result: {Result}", result);
        return Task.FromResult(result);
    }
}
