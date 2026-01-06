using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

public abstract record GameCommand;

public record MoveCommand(Direction Direction) : GameCommand;
public record LookCommand : GameCommand;
public record InventoryCommand : GameCommand;
public record TakeCommand(string ItemName) : GameCommand;
public record AttackCommand : GameCommand;
public record SaveCommand : GameCommand;
public record LoadCommand : GameCommand;
public record HelpCommand : GameCommand;
public record QuitCommand : GameCommand;
public record UnknownCommand(string Input) : GameCommand;

public interface IInputHandler
{
    Task<GameCommand> GetNextCommandAsync(CancellationToken ct = default);
    Task<string> GetTextInputAsync(string prompt, CancellationToken ct = default);
    Task<T> GetSelectionAsync<T>(string prompt, IEnumerable<T> options, Func<T, string> displaySelector, CancellationToken ct = default);
    Task<bool> GetConfirmationAsync(string prompt, CancellationToken ct = default);
}
