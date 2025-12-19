using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements a d10 dice pool system.
/// Success threshold: 8+ on d10.
/// Botch: roll of 1.
/// Fumble: Zero successes with at least one botch.
/// </summary>
public class DiceService : IDiceService
{
    private readonly ILogger<DiceService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiceService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public DiceService(ILogger<DiceService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public DiceResult Roll(int poolSize, string context = "Unspecified")
    {
        // Validate and clamp pool size
        var actualPoolSize = ValidatePoolSize(poolSize, context);

        _logger.LogTrace("Rolling {PoolSize}d10 for {Context}", actualPoolSize, context);

        var rolls = new List<int>(actualPoolSize);
        var successes = 0;
        var botches = 0;

        for (int i = 0; i < actualPoolSize; i++)
        {
            int dieNumber = i + 1;
            int value = Random.Shared.Next(1, 11);
            rolls.Add(value);

            _logger.LogTrace("Die {DieNumber}: rolled {Value}", dieNumber, value);

            if (value >= 8)
            {
                successes++;
                _logger.LogTrace("Die {DieNumber}: {Value} is a SUCCESS (8+)", dieNumber, value);
            }
            else if (value == 1)
            {
                botches++;
                _logger.LogTrace("Die {DieNumber}: {Value} is a BOTCH (1)", dieNumber, value);
            }
        }

        var rollsString = string.Join(", ", rolls);
        _logger.LogDebug(
            "Roll complete: {Successes} successes, {Botches} botches from {PoolSize}d10 [{Rolls}] ({Context})",
            successes, botches, actualPoolSize, rollsString, context);

        // Check for fumble condition
        if (successes == 0 && botches > 0)
        {
            _logger.LogWarning(
                "FUMBLE! Zero successes with {Botches} botch(es) on {PoolSize}d10 ({Context})",
                botches, actualPoolSize, context);
        }

        return new DiceResult(successes, botches, rolls.AsReadOnly());
    }

    /// <summary>
    /// Validates the pool size and returns a valid value (minimum 1).
    /// </summary>
    private int ValidatePoolSize(int poolSize, string context)
    {
        if (poolSize < 0)
        {
            _logger.LogError(
                "Negative dice pool {RequestedSize} requested for {Context}. This indicates a logic error.",
                poolSize, context);
            _logger.LogWarning(
                "Invalid dice pool {RequestedSize} for {Context}. Clamping to minimum of 1.",
                poolSize, context);
            return 1;
        }

        if (poolSize == 0)
        {
            _logger.LogWarning(
                "Invalid dice pool {RequestedSize} for {Context}. Clamping to minimum of 1.",
                poolSize, context);
            return 1;
        }

        return poolSize;
    }
}
