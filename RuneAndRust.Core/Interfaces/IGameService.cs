namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for the main game service.
/// This abstraction allows the UI layer to call the Engine without knowing its implementation.
/// </summary>
/// <remarks>See: SPEC-GAME-001 for Game Orchestration System design.</remarks>
public interface IGameService
{
    /// <summary>
    /// Starts the game engine asynchronously.
    /// Runs the main game loop until the user quits.
    /// </summary>
    /// <returns>A task representing the game execution.</returns>
    Task StartAsync();
}
