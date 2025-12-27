namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for the main game service.
/// This abstraction allows the UI layer to call the Engine without knowing its implementation.
/// </summary>
/// <remarks>
/// See: SPEC-GAME-001 for Game Orchestration System design.
/// v0.3.23b: Added CancellationToken support for graceful shutdown.
/// </remarks>
public interface IGameService
{
    /// <summary>
    /// Starts the game engine asynchronously.
    /// Runs the main game loop until the user quits or cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">Token for graceful shutdown on SIGINT or quit command.</param>
    /// <returns>A task representing the game execution.</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
}
