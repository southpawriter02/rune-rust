using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.1: Command Interface
/// Defines the contract for all executable commands in the game.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Execute the command with the given game state and arguments.
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="args">Command arguments (after verb)</param>
    /// <returns>Result of command execution</returns>
    CommandResult Execute(GameState state, string[] args);
}

/// <summary>
/// v0.37.1: Command execution result
/// Provides feedback about command success/failure and output message.
/// </summary>
public class CommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool ShouldRedrawRoom { get; set; } = false; // For commands that change room state

    public static CommandResult Success(string message, bool redrawRoom = false)
    {
        return new CommandResult
        {
            Success = true,
            Message = message,
            ShouldRedrawRoom = redrawRoom
        };
    }

    public static CommandResult Failure(string message)
    {
        return new CommandResult { Success = false, Message = message };
    }
}
