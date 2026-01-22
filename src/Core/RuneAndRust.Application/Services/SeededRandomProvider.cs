using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Provides seeded random number generation with state save/restore capability.
/// </summary>
/// <remarks>
/// <para>
/// This implementation wraps <see cref="System.Random"/> with seed management,
/// enabling deterministic sequences for testing and gameplay scenarios.
/// </para>
/// <para>
/// Thread Safety: This class is NOT thread-safe. If used across threads,
/// external synchronization is required, or use separate instances per thread.
/// </para>
/// </remarks>
public class SeededRandomProvider : IRandomProvider
{
    private Random _random;
    private int _currentSeed;
    private int _savedSeed;
    private bool _hasSavedState;
    private readonly ILogger<SeededRandomProvider>? _logger;

    /// <summary>
    /// Creates a new SeededRandomProvider with an optional initial seed.
    /// </summary>
    /// <param name="initialSeed">
    /// The seed to use. If null, generates a seed from current time.
    /// </param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <remarks>
    /// When no seed is provided, a time-based seed is generated using
    /// <see cref="DateTime.UtcNow"/> ticks for randomness.
    /// </remarks>
    public SeededRandomProvider(int? initialSeed = null, ILogger<SeededRandomProvider>? logger = null)
    {
        _logger = logger;
        _currentSeed = initialSeed ?? GenerateTimeSeed();
        _random = new Random(_currentSeed);
        _savedSeed = _currentSeed;
        _hasSavedState = false;

        _logger?.LogDebug(
            "SeededRandomProvider initialized with seed {Seed}",
            _currentSeed);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxExclusive"/> is less than or equal to <paramref name="minInclusive"/>.
    /// </exception>
    public int Next(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxExclusive),
                $"maxExclusive ({maxExclusive}) must be greater than minInclusive ({minInclusive})");
        }

        var result = _random.Next(minInclusive, maxExclusive);

        _logger?.LogTrace(
            "Next({Min}, {Max}) = {Result} (seed: {Seed})",
            minInclusive, maxExclusive, result, _currentSeed);

        return result;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="count"/> is negative or 
    /// <paramref name="maxExclusive"/> is less than or equal to <paramref name="minInclusive"/>.
    /// </exception>
    public int[] NextMany(int count, int minInclusive, int maxExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (minInclusive >= maxExclusive)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxExclusive),
                $"maxExclusive ({maxExclusive}) must be greater than minInclusive ({minInclusive})");
        }

        if (count == 0)
        {
            _logger?.LogTrace(
                "NextMany({Count}, {Min}, {Max}) = [] (empty, seed: {Seed})",
                count, minInclusive, maxExclusive, _currentSeed);
            return Array.Empty<int>();
        }

        var results = new int[count];
        for (var i = 0; i < count; i++)
        {
            results[i] = _random.Next(minInclusive, maxExclusive);
        }

        _logger?.LogTrace(
            "NextMany({Count}, {Min}, {Max}) = [{Results}] (seed: {Seed})",
            count, minInclusive, maxExclusive, string.Join(", ", results), _currentSeed);

        return results;
    }

    /// <inheritdoc />
    public void SetSeed(int seed)
    {
        _logger?.LogDebug(
            "Setting seed from {OldSeed} to {NewSeed}",
            _currentSeed, seed);

        _currentSeed = seed;
        _random = new Random(seed);
    }

    /// <inheritdoc />
    public int GetCurrentSeed() => _currentSeed;

    /// <inheritdoc />
    public void SaveState()
    {
        _savedSeed = _currentSeed;
        _hasSavedState = true;

        _logger?.LogDebug(
            "Saved RNG state with seed {Seed}",
            _savedSeed);
    }

    /// <inheritdoc />
    public void RestoreState()
    {
        if (!_hasSavedState)
        {
            _logger?.LogWarning(
                "RestoreState called without prior SaveState; restoring to initial seed {Seed}",
                _savedSeed);
        }
        else
        {
            _logger?.LogDebug(
                "Restoring RNG state from saved seed {Seed}",
                _savedSeed);
        }

        SetSeed(_savedSeed);
    }

    /// <summary>
    /// Generates a seed based on current UTC time.
    /// </summary>
    /// <returns>A seed value derived from the current time.</returns>
    /// <remarks>
    /// Uses the lower bits of <see cref="DateTime.UtcNow"/> ticks
    /// to provide a unique seed for each instantiation.
    /// </remarks>
    private static int GenerateTimeSeed()
    {
        return (int)(DateTime.UtcNow.Ticks % int.MaxValue);
    }
}
