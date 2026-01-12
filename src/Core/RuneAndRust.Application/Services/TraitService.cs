using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implementation of trait management with tier-based trait selection.
/// </summary>
public class TraitService : ITraitService
{
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<TraitService> _logger;
    private readonly IGameEventLogger? _eventLogger;
    private readonly Random _random;

    /// <summary>
    /// Creates a new trait service.
    /// </summary>
    /// <param name="configProvider">Configuration provider for trait definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="eventLogger">Optional event logger.</param>
    public TraitService(
        IGameConfigurationProvider configProvider,
        ILogger<TraitService> logger,
        IGameEventLogger? eventLogger = null)
        : this(configProvider, logger, eventLogger, Random.Shared)
    {
    }

    /// <summary>
    /// Creates a new trait service with explicit random source (for testing).
    /// </summary>
    /// <param name="configProvider">Configuration provider for trait definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="eventLogger">Optional event logger.</param>
    /// <param name="random">Random number generator.</param>
    internal TraitService(
        IGameConfigurationProvider configProvider,
        ILogger<TraitService> logger,
        IGameEventLogger? eventLogger,
        Random random)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    /// <inheritdoc/>
    public MonsterTrait? GetTrait(string traitId)
    {
        if (string.IsNullOrWhiteSpace(traitId))
            return null;

        return _configProvider.GetTraitById(traitId.ToLowerInvariant());
    }

    /// <inheritdoc/>
    public IReadOnlyList<MonsterTrait> GetAllTraits()
    {
        return _configProvider.GetTraits();
    }

    /// <inheritdoc/>
    public IReadOnlyList<MonsterTrait> GetTraits(IEnumerable<string> traitIds)
    {
        if (traitIds == null)
            return [];

        return traitIds
            .Select(id => _configProvider.GetTraitById(id?.ToLowerInvariant() ?? ""))
            .Where(t => t != null)
            .Cast<MonsterTrait>()
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> SelectRandomTraits(IReadOnlyList<string> possibleTraitIds, TierDefinition tier)
    {
        ArgumentNullException.ThrowIfNull(tier);

        // Determine trait count based on tier
        var traitCount = GetTraitCountForTier(tier);

        if (traitCount == 0)
        {
            _logger.LogDebug("Tier {TierId} gets 0 traits", tier.Id);
            return [];
        }

        if (possibleTraitIds == null || possibleTraitIds.Count == 0)
        {
            _logger.LogDebug("No possible traits available for tier {TierId}", tier.Id);
            return [];
        }

        // Get valid trait IDs (ones that exist in configuration)
        var validTraitIds = possibleTraitIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Where(id => _configProvider.GetTraitById(id.ToLowerInvariant()) != null)
            .Select(id => id.ToLowerInvariant())
            .Distinct()
            .ToList();

        if (validTraitIds.Count == 0)
        {
            _logger.LogDebug("No valid traits found for tier {TierId}", tier.Id);
            return [];
        }

        // Cap trait count to available traits
        var actualCount = Math.Min(traitCount, validTraitIds.Count);

        // Fisher-Yates shuffle to select random traits
        var shuffled = validTraitIds.ToList();
        for (var i = shuffled.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        var selectedTraits = shuffled.Take(actualCount).ToList();

        _logger.LogDebug("Selected {Count} traits for tier {TierId}: [{Traits}]",
            actualCount, tier.Id, string.Join(", ", selectedTraits));

        _eventLogger?.LogAI("TraitsSelected", $"Selected {actualCount} traits for {tier.Id} monster",
            data: new Dictionary<string, object>
            {
                ["tierId"] = tier.Id,
                ["traitCount"] = actualCount,
                ["traits"] = selectedTraits
            });

        return selectedTraits;
    }

    /// <summary>
    /// Gets the number of traits a monster should have based on tier.
    /// </summary>
    /// <param name="tier">The tier definition.</param>
    /// <returns>Number of traits to assign.</returns>
    /// <remarks>
    /// Trait count rules:
    /// - common: 0 traits
    /// - named: 1 trait
    /// - elite: 2 traits
    /// - boss: 3 traits
    /// </remarks>
    private static int GetTraitCountForTier(TierDefinition tier)
    {
        return tier.Id.ToLowerInvariant() switch
        {
            "common" => 0,
            "named" => 1,
            "elite" => 2,
            "boss" => 3,
            _ => 0 // Unknown tiers get no traits by default
        };
    }
}
