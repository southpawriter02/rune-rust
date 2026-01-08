using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Services;
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
            "abilities" or "ab" or "spells" or "skills" => new AbilitiesCommand(),
            "cast" or "ability" => string.IsNullOrEmpty(argument)
                ? LogAndReturn(new UnknownCommand(input), "Cast command missing ability argument")
                : new UseAbilityCommand(argument),
            "attack" or "fight" or "a" => new AttackCommand(),
            "roll" or "r" => ParseRollCommand(argument, input),
            "check" or "ch" => ParseSkillCheckCommand(argument, input),
            "equip" => ParseEquipCommand(argument),
            "unequip" => ParseUnequipCommand(argument),
            "equipment" or "eq" or "gear" => new EquipmentCommand(),
            "save" => new SaveCommand(),
            "load" => new LoadCommand(),
            "help" or "h" or "?" => new HelpCommand(),
            "quit" or "exit" or "q" => new QuitCommand(),
            _ => LogAndReturn(new UnknownCommand(input), $"Unrecognized command: '{command}'")
        };
    }

    /// <summary>
    /// Parses a roll command from user input.
    /// </summary>
    private GameCommand ParseRollCommand(string? argument, string fullInput)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            _logger.LogDebug("Roll command missing notation");
            return LogAndReturn(new UnknownCommand(fullInput),
                "Roll command requires dice notation (e.g., 'roll 3d6+5')");
        }

        // Check for advantage/disadvantage flags
        var advantage = AdvantageType.Normal;
        var notation = argument;

        if (argument.EndsWith(" adv", StringComparison.OrdinalIgnoreCase) ||
            argument.EndsWith(" advantage", StringComparison.OrdinalIgnoreCase))
        {
            advantage = AdvantageType.Advantage;
            notation = argument[..argument.LastIndexOf(' ')].Trim();
        }
        else if (argument.EndsWith(" dis", StringComparison.OrdinalIgnoreCase) ||
                 argument.EndsWith(" disadvantage", StringComparison.OrdinalIgnoreCase))
        {
            advantage = AdvantageType.Disadvantage;
            notation = argument[..argument.LastIndexOf(' ')].Trim();
        }

        _logger.LogDebug("Parsed roll command: {Notation}, Advantage: {Advantage}", notation, advantage);
        return new RollCommand(notation, advantage);
    }

    /// <summary>
    /// Parses a skill check command from user input.
    /// </summary>
    private GameCommand ParseSkillCheckCommand(string? argument, string fullInput)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            _logger.LogDebug("Check command missing skill name");
            return LogAndReturn(new UnknownCommand(fullInput),
                "Check command requires skill name (e.g., 'check perception', 'check stealth moderate')");
        }

        var cmdParts = argument.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var skillId = cmdParts[0].ToLowerInvariant();
        string? difficultyId = null;
        var advantage = AdvantageType.Normal;

        // Parse remaining parts for DC and advantage
        for (int i = 1; i < cmdParts.Length; i++)
        {
            var part = cmdParts[i].ToLowerInvariant();
            if (part == "adv" || part == "advantage")
            {
                advantage = AdvantageType.Advantage;
            }
            else if (part == "dis" || part == "disadvantage")
            {
                advantage = AdvantageType.Disadvantage;
            }
            else if (difficultyId == null)
            {
                difficultyId = part;
            }
        }

        _logger.LogDebug("Parsed check command: Skill={Skill}, DC={DC}, Advantage={Advantage}",
            skillId, difficultyId ?? "default", advantage);

        return new SkillCheckCommand(skillId, difficultyId, advantage);
    }

    /// <summary>
    /// Parses an equip command from user input.
    /// </summary>
    private GameCommand ParseEquipCommand(string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            _logger.LogDebug("Equip command missing item name");
            return new InvalidCommand("Usage: equip <item name>");
        }

        return new EquipCommand(argument.Trim());
    }

    /// <summary>
    /// Parses an unequip command from user input.
    /// </summary>
    private GameCommand ParseUnequipCommand(string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            _logger.LogDebug("Unequip command missing slot name");
            return new InvalidCommand($"Usage: unequip <slot>\nSlots: {EquipmentService.GetValidSlotNames()}");
        }

        if (EquipmentService.TryParseSlot(argument.Trim(), out var slot))
        {
            return new UnequipCommand(slot);
        }

        return new InvalidCommand($"Unknown slot '{argument}'. Valid slots: {EquipmentService.GetValidSlotNames()}");
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
