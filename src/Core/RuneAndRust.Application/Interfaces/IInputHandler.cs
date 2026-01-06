using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

public abstract record GameCommand;

public record MoveCommand(Direction Direction) : GameCommand;
public record LookCommand(string? Target = null) : GameCommand;
public record InventoryCommand : GameCommand;
public record TakeCommand(string ItemName) : GameCommand;
public record AttackCommand : GameCommand;
public record SearchCommand(string? Target = null) : GameCommand;
public record InvestigateCommand(string Target) : GameCommand;
public record ExamineCommand(string Target) : GameCommand;
public record TravelCommand(string? Destination = null) : GameCommand;
public record EnterCommand(string? Location = null) : GameCommand;
public record ExitCommand(string? Direction = null) : GameCommand;
public record SaveCommand : GameCommand;
public record LoadCommand : GameCommand;
public record HelpCommand : GameCommand;
public record QuitCommand : GameCommand;
public record UnknownCommand(string Input) : GameCommand;

public interface IInputHandler
{
    Task<GameCommand> GetNextCommandAsync(CancellationToken ct = default);
    Task<string> GetTextInputAsync(string prompt, CancellationToken ct = default);
    Task<T> GetSelectionAsync<T>(string prompt, IEnumerable<T> options, Func<T, string> displaySelector, CancellationToken ct = default) where T : notnull;
    Task<bool> GetConfirmationAsync(string prompt, CancellationToken ct = default);
}
