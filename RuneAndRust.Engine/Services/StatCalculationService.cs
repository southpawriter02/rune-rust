using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Provides basic stat calculation operations including modifier application and value clamping.
/// </summary>
public class StatCalculationService : IStatCalculationService
{
    private readonly ILogger<StatCalculationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatCalculationService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public StatCalculationService(ILogger<StatCalculationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public int ApplyModifier(int baseValue, int modifier)
    {
        _logger.LogTrace("Applying modifier {Modifier} to base value {BaseValue}", modifier, baseValue);

        var result = baseValue + modifier;

        _logger.LogDebug("Modifier applied: {BaseValue} + {Modifier} = {Result}", baseValue, modifier, result);

        return result;
    }

    /// <inheritdoc/>
    public int ClampAttribute(int value, int min = 1, int max = 10)
    {
        _logger.LogTrace("Clamping value {Value} to range [{Min}, {Max}]", value, min, max);

        if (value < min)
        {
            _logger.LogWarning("Value {Value} below minimum {Min}, clamped to {Min}", value, min, min);
            return min;
        }

        if (value > max)
        {
            _logger.LogWarning("Value {Value} above maximum {Max}, clamped to {Max}", value, max, max);
            return max;
        }

        _logger.LogTrace("Value {Value} within range [{Min}, {Max}], no clamping needed", value, min, max);
        return value;
    }
}
