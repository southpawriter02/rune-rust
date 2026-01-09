using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implementation of tier management with weighted random selection.
/// </summary>
public class TierService : ITierService
{
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<TierService> _logger;
    private readonly Random _random;

    /// <summary>
    /// Creates a new tier service.
    /// </summary>
    /// <param name="configProvider">Configuration provider for tier definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public TierService(IGameConfigurationProvider configProvider, ILogger<TierService> logger)
        : this(configProvider, logger, Random.Shared)
    {
    }

    /// <summary>
    /// Creates a new tier service with explicit random source (for testing).
    /// </summary>
    /// <param name="configProvider">Configuration provider for tier definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="random">Random number generator.</param>
    internal TierService(IGameConfigurationProvider configProvider, ILogger<TierService> logger, Random random)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    /// <inheritdoc/>
    public TierDefinition? GetTier(string tierId)
    {
        if (string.IsNullOrWhiteSpace(tierId))
            return null;

        return _configProvider.GetTierById(tierId.ToLowerInvariant());
    }

    /// <inheritdoc/>
    public IReadOnlyList<TierDefinition> GetAllTiers()
    {
        return _configProvider.GetTiers();
    }

    /// <inheritdoc/>
    public TierDefinition SelectRandomTier(IReadOnlyList<string> possibleTierIds)
    {
        if (possibleTierIds == null || possibleTierIds.Count == 0)
        {
            _logger.LogDebug("No possible tiers specified, returning default tier");
            return GetDefaultTier();
        }

        // Get valid tier definitions for the provided IDs
        var validTiers = possibleTierIds
            .Select(id => _configProvider.GetTierById(id?.ToLowerInvariant() ?? ""))
            .Where(t => t != null)
            .Cast<TierDefinition>()
            .ToList();

        if (validTiers.Count == 0)
        {
            _logger.LogWarning("No valid tiers found for IDs: [{TierIds}], returning default",
                string.Join(", ", possibleTierIds));
            return GetDefaultTier();
        }

        // If only one valid tier, return it directly
        if (validTiers.Count == 1)
        {
            return validTiers[0];
        }

        // Weighted random selection based on SpawnWeight
        var totalWeight = validTiers.Sum(t => t.SpawnWeight);
        if (totalWeight <= 0)
        {
            // If all weights are zero, pick uniformly at random
            var uniformIndex = _random.Next(validTiers.Count);
            var uniformTier = validTiers[uniformIndex];
            _logger.LogDebug("All spawn weights zero, selected tier {TierId} uniformly", uniformTier.Id);
            return uniformTier;
        }

        var roll = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var tier in validTiers)
        {
            cumulative += tier.SpawnWeight;
            if (roll < cumulative)
            {
                _logger.LogDebug("Selected tier {TierId} (weight {Weight}/{Total}, roll {Roll})",
                    tier.Id, tier.SpawnWeight, totalWeight, roll);
                return tier;
            }
        }

        // Fallback (should not reach here)
        return validTiers[^1];
    }

    /// <inheritdoc/>
    public TierDefinition GetDefaultTier()
    {
        var commonTier = _configProvider.GetTierById("common");
        if (commonTier != null)
            return commonTier;

        _logger.LogWarning("Common tier not found in configuration, returning static default");
        return TierDefinition.Common;
    }
}
