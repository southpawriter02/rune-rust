using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// The main game service implementation.
/// Handles game initialization and core game loop logic.
/// </summary>
public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;

    public GameService(ILogger<GameService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Start()
    {
        _logger.LogInformation("Rune & Rust Engine Initialized.");
        // v0.0.1 Proof of Life logic
    }
}
