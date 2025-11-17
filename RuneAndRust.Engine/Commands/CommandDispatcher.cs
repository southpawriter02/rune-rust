using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.1: Command Dispatcher
/// Routes parsed commands to appropriate ICommand implementations.
/// Central registry for all executable commands in the game.
/// </summary>
public class CommandDispatcher
{
    private static readonly ILogger _log = Log.ForContext<CommandDispatcher>();
    private readonly Dictionary<CommandType, ICommand> _commandRegistry;

    public CommandDispatcher(
        DiceService diceService,
        LootService lootService,
        CombatEngine? combatEngine = null,
        StanceService? stanceService = null)
    {
        _log.Information("Initializing CommandDispatcher with navigation and combat commands");

        _commandRegistry = new Dictionary<CommandType, ICommand>();

        // Register v0.37.1 Navigation Commands
        RegisterCommand(CommandType.Look, new LookCommand());
        RegisterCommand(CommandType.Move, new GoCommand());
        RegisterCommand(CommandType.Investigate, new InvestigateCommand(diceService));
        RegisterCommand(CommandType.Search, new SearchCommand(lootService));

        // Register v0.37.2 Combat Commands (if services available)
        if (combatEngine != null)
        {
            RegisterCommand(CommandType.Attack, new AttackCommand(combatEngine));
            RegisterCommand(CommandType.Ability, new AbilityCommand(combatEngine));
            RegisterCommand(CommandType.Defend, new BlockCommand(combatEngine)); // Block/Defend
            RegisterCommand(CommandType.Flee, new FleeCommand(combatEngine));
            RegisterCommand(CommandType.Parry, new ParryCommand(combatEngine));
        }

        if (stanceService != null)
        {
            RegisterCommand(CommandType.Stance, new StanceCommand(stanceService));
        }

        _log.Information("CommandDispatcher initialized with {CommandCount} commands", _commandRegistry.Count);
    }

    /// <summary>
    /// Register a command implementation for a command type
    /// </summary>
    public void RegisterCommand(CommandType type, ICommand command)
    {
        if (_commandRegistry.ContainsKey(type))
        {
            _log.Warning("Overwriting existing command registration: {CommandType}", type);
        }

        _commandRegistry[type] = command;
        _log.Debug("Registered command: {CommandType} -> {CommandClass}",
            type,
            command.GetType().Name);
    }

    /// <summary>
    /// Dispatch a parsed command to the appropriate handler
    /// </summary>
    public CommandResult Dispatch(ParsedCommand parsedCommand, GameState state)
    {
        if (parsedCommand == null)
        {
            _log.Warning("Null ParsedCommand received");
            return CommandResult.Failure("Invalid command.");
        }

        if (parsedCommand.Type == CommandType.Unknown)
        {
            _log.Debug("Unknown command type: {RawInput}", parsedCommand.RawInput);
            return CommandResult.Failure($"Unknown command: '{parsedCommand.RawInput}'. Type 'help' for available commands.");
        }

        // Check if command is registered
        if (!_commandRegistry.ContainsKey(parsedCommand.Type))
        {
            _log.Warning("Command not registered in dispatcher: {CommandType}", parsedCommand.Type);
            return CommandResult.Failure($"Command '{parsedCommand.Type}' is not yet implemented.");
        }

        try
        {
            var command = _commandRegistry[parsedCommand.Type];

            // Build arguments array from parsed command
            var args = BuildArgumentsArray(parsedCommand);

            _log.Information(
                "Dispatching command: Type={CommandType}, Args={Args}",
                parsedCommand.Type,
                string.Join(" ", args));

            // Execute command
            var result = command.Execute(state, args);

            _log.Information(
                "Command execution complete: Type={CommandType}, Success={Success}",
                parsedCommand.Type,
                result.Success);

            return result;
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Command execution failed: Type={CommandType}, Error={ErrorMessage}",
                parsedCommand.Type,
                ex.Message);

            return CommandResult.Failure($"An error occurred while executing the command: {ex.Message}");
        }
    }

    /// <summary>
    /// Build arguments array from ParsedCommand
    /// Handles different command types and their specific argument structures
    /// </summary>
    private string[] BuildArgumentsArray(ParsedCommand parsedCommand)
    {
        var argsList = new List<string>();

        // For Move commands, use Direction
        if (parsedCommand.Type == CommandType.Move && !string.IsNullOrEmpty(parsedCommand.Direction))
        {
            argsList.Add(parsedCommand.Direction);
        }
        // For Attack commands, use Target
        else if (parsedCommand.Type == CommandType.Attack && !string.IsNullOrEmpty(parsedCommand.Target))
        {
            argsList.Add(parsedCommand.Target);
        }
        // For Ability commands, use AbilityName
        else if (parsedCommand.Type == CommandType.Ability && !string.IsNullOrEmpty(parsedCommand.AbilityName))
        {
            argsList.Add(parsedCommand.AbilityName);
        }
        // For other commands, use generic Arguments list
        else if (parsedCommand.Arguments != null && parsedCommand.Arguments.Any())
        {
            argsList.AddRange(parsedCommand.Arguments);
        }

        return argsList.ToArray();
    }

    /// <summary>
    /// Check if a command type is registered
    /// </summary>
    public bool IsCommandRegistered(CommandType type)
    {
        return _commandRegistry.ContainsKey(type);
    }

    /// <summary>
    /// Get list of all registered command types
    /// </summary>
    public IEnumerable<CommandType> GetRegisteredCommands()
    {
        return _commandRegistry.Keys;
    }
}
