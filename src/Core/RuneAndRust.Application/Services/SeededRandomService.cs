using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Provides reproducible random number generation from seeds.
/// </summary>
/// <remarks>
/// The SeededRandomService ensures that:
/// - Same master seed produces identical results
/// - Same position + context produces identical sub-results
/// - Different positions/contexts are properly isolated
/// - Results are deterministic across sessions and platforms
/// </remarks>
public class SeededRandomService : ISeededRandomService
{
    private readonly int _masterSeed;
    private readonly Dictionary<int, Random> _subGenerators = new();
    private readonly ILogger<SeededRandomService> _logger;

    /// <inheritdoc/>
    public int MasterSeed => _masterSeed;

    /// <summary>
    /// Creates a new seeded random service with the specified seed.
    /// </summary>
    /// <param name="seed">The master seed for this generator.</param>
    /// <param name="logger">Logger for seed events.</param>
    public SeededRandomService(int seed, ILogger<SeededRandomService> logger)
    {
        _masterSeed = seed;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation(
            "SeededRandomService initialized with seed {Seed} ({SeedString})",
            seed, SeedStringUtility.ToSeedString(seed));
    }

    /// <inheritdoc/>
    public int NextForPosition(Position3D position, string context = "default")
    {
        var subSeed = DeriveSubSeed(position, context);
        var generator = GetOrCreateGenerator(subSeed);
        var result = generator.Next();

        _logger.LogDebug(
            "NextForPosition({Position}, {Context}): subSeed={SubSeed}, value={Value}",
            position, context, subSeed, result);

        return result;
    }

    /// <inheritdoc/>
    public int NextForPosition(Position3D position, int minInclusive, int maxExclusive, string context = "default")
    {
        var subSeed = DeriveSubSeed(position, context);
        var generator = GetOrCreateGenerator(subSeed);
        var result = generator.Next(minInclusive, maxExclusive);

        _logger.LogDebug(
            "NextForPosition({Position}, {Min}-{Max}, {Context}): value={Value}",
            position, minInclusive, maxExclusive, context, result);

        return result;
    }

    /// <inheritdoc/>
    public float NextFloatForPosition(Position3D position, string context = "default")
    {
        var subSeed = DeriveSubSeed(position, context);
        var generator = GetOrCreateGenerator(subSeed);
        var result = (float)generator.NextDouble();

        _logger.LogDebug(
            "NextFloatForPosition({Position}, {Context}): value={Value:F4}",
            position, context, result);

        return result;
    }

    /// <inheritdoc/>
    public T SelectWeighted<T>(Position3D position, IEnumerable<(T item, int weight)> items, string context = "default")
    {
        var itemList = items.ToList();

        if (itemList.Count == 0)
            throw new ArgumentException("Items collection cannot be empty.", nameof(items));

        var totalWeight = itemList.Sum(x => x.weight);
        if (totalWeight <= 0)
            throw new ArgumentException("Total weight must be positive.", nameof(items));

        var subSeed = DeriveSubSeed(position, context);
        var generator = GetOrCreateGenerator(subSeed);
        var roll = generator.Next(totalWeight);
        var cumulative = 0;

        foreach (var (item, weight) in itemList)
        {
            cumulative += weight;
            if (roll < cumulative)
            {
                _logger.LogDebug(
                    "SelectWeighted({Position}, {Context}): roll={Roll}/{Total}, selected at cumulative={Cumulative}",
                    position, context, roll, totalWeight, cumulative);
                return item;
            }
        }

        // Fallback (shouldn't reach here if weights > 0)
        return itemList[^1].item;
    }

    /// <inheritdoc/>
    public void ClearSubGenerators()
    {
        var count = _subGenerators.Count;
        _subGenerators.Clear();
        _logger.LogDebug("Cleared {Count} cached sub-generators", count);
    }

    // ===== Private Methods =====

    /// <summary>
    /// Derives a sub-seed from position and context.
    /// </summary>
    private int DeriveSubSeed(Position3D position, string context)
    {
        unchecked
        {
            var hash = _masterSeed;
            hash = hash * 31 + position.X;
            hash = hash * 31 + position.Y;
            hash = hash * 31 + position.Z;
            hash = hash * 31 + context.GetHashCode(StringComparison.Ordinal);
            return hash;
        }
    }

    /// <summary>
    /// Gets or creates a cached Random generator for a sub-seed.
    /// </summary>
    private Random GetOrCreateGenerator(int subSeed)
    {
        if (!_subGenerators.TryGetValue(subSeed, out var generator))
        {
            generator = new Random(subSeed);
            _subGenerators[subSeed] = generator;

            _logger.LogDebug("Created new sub-generator for subSeed {SubSeed}", subSeed);
        }

        return generator;
    }
}
