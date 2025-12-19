namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for the main game service.
/// This abstraction allows the UI layer to call the Engine without knowing its implementation.
/// </summary>
public interface IGameService
{
    /// <summary>
    /// Starts the game engine.
    /// </summary>
    void Start();
}
