using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Adapters;

public class ConsoleInputHandler : IInputHandler
{
    public Task<GameCommand> GetNextCommandAsync(CancellationToken ct = default)
    {
        AnsiConsole.Markup("[grey]> [/]");
        var input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;

        var command = ParseCommand(input);
        return Task.FromResult(command);
    }

    private static GameCommand ParseCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new UnknownCommand(input);

        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0];
        var argument = parts.Length > 1 ? parts[1] : string.Empty;

        return command switch
        {
            "n" or "north" => new MoveCommand(Direction.North),
            "s" or "south" => new MoveCommand(Direction.South),
            "e" or "east" => new MoveCommand(Direction.East),
            "w" or "west" => new MoveCommand(Direction.West),
            "look" or "l" => new LookCommand(),
            "inventory" or "inv" or "i" => new InventoryCommand(),
            "take" or "get" or "pick" => string.IsNullOrEmpty(argument)
                ? new UnknownCommand(input)
                : new TakeCommand(argument),
            "attack" or "fight" or "a" => new AttackCommand(),
            "save" => new SaveCommand(),
            "load" => new LoadCommand(),
            "help" or "h" or "?" => new HelpCommand(),
            "quit" or "exit" or "q" => new QuitCommand(),
            _ => new UnknownCommand(input)
        };
    }

    public Task<string> GetTextInputAsync(string prompt, CancellationToken ct = default)
    {
        var result = AnsiConsole.Ask<string>($"[cyan]{prompt}[/]");
        return Task.FromResult(result);
    }

    public Task<T> GetSelectionAsync<T>(
        string prompt,
        IEnumerable<T> options,
        Func<T, string> displaySelector,
        CancellationToken ct = default) where T : notnull
    {
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<T>()
                .Title($"[cyan]{prompt}[/]")
                .PageSize(10)
                .UseConverter(displaySelector)
                .AddChoices(options));

        return Task.FromResult(selection);
    }

    public Task<bool> GetConfirmationAsync(string prompt, CancellationToken ct = default)
    {
        var result = AnsiConsole.Confirm($"[cyan]{prompt}[/]");
        return Task.FromResult(result);
    }
}
