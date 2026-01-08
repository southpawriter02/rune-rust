using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Provides descriptive flavor text for dice rolls and skill checks.
/// </summary>
/// <remarks>
/// <para>Descriptors add immersion by providing contextual flavor text for notable
/// dice results, particularly critical successes and failures.</para>
/// <para>Descriptors are loaded from JSON configuration and selected randomly
/// from available options for variety.</para>
/// </remarks>
public class DiceDescriptorService
{
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<DiceDescriptorService> _logger;
    private readonly Random _random;
    private IReadOnlyDictionary<string, IReadOnlyList<string>>? _descriptors;

    /// <summary>
    /// Initializes a new instance of the DiceDescriptorService.
    /// </summary>
    /// <param name="configProvider">Provider for loading descriptor configuration.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="random">Optional random instance for testing. If null, creates new Random.</param>
    public DiceDescriptorService(
        IGameConfigurationProvider configProvider,
        ILogger<DiceDescriptorService> logger,
        Random? random = null)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? new Random();
        _logger.LogDebug("DiceDescriptorService initialized");
    }

    /// <summary>
    /// Gets a random descriptor from the specified category.
    /// </summary>
    /// <param name="category">The descriptor category (e.g., "dice.natural_max").</param>
    /// <returns>A random descriptor string, or null if category not found.</returns>
    public string? GetDescriptor(string category)
    {
        EnsureDescriptorsLoaded();

        if (_descriptors == null || !_descriptors.TryGetValue(category, out var options))
        {
            _logger.LogDebug("No descriptors found for category: {Category}", category);
            return null;
        }

        if (options.Count == 0)
        {
            return null;
        }

        var selected = options[_random.Next(options.Count)];
        _logger.LogDebug("Selected descriptor for {Category}: {Descriptor}", category, selected);
        return selected;
    }

    /// <summary>
    /// Gets a descriptor for a dice roll based on its result.
    /// </summary>
    /// <param name="result">The dice roll result.</param>
    /// <returns>Appropriate descriptor, or null if no special result.</returns>
    public string? GetDiceRollDescriptor(DiceRollResult result)
    {
        if (result.IsNaturalMax)
        {
            return GetDescriptor("dice.natural_max");
        }

        if (result.IsNaturalOne)
        {
            return GetDescriptor("dice.natural_one");
        }

        // Check for high/low rolls (85%/15% of maximum)
        if (result.Pool.MaximumResult > 0)
        {
            var percentage = (float)result.DiceTotal / result.Pool.MaximumResult;

            if (percentage >= 0.85f)
            {
                return GetDescriptor("dice.high_roll");
            }

            if (percentage <= 0.15f)
            {
                return GetDescriptor("dice.low_roll");
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a descriptor for a skill check based on its result.
    /// </summary>
    /// <param name="result">The skill check result.</param>
    /// <returns>Appropriate descriptor, or null if no special result.</returns>
    public string? GetSkillCheckDescriptor(SkillCheckResult result)
    {
        if (result.IsCriticalSuccess)
        {
            return GetDescriptor("skill.critical_success");
        }

        if (result.IsCriticalFailure)
        {
            return GetDescriptor("skill.critical_failure");
        }

        // Check for narrow success/failure (within 1-2 of DC)
        var margin = Math.Abs(result.Margin);
        if (margin <= 1)
        {
            return result.IsSuccess
                ? GetDescriptor("skill.narrow_success")
                : GetDescriptor("skill.narrow_failure");
        }

        // Check for overwhelming success/failure
        if (result.IsSuccess && result.Margin >= 10)
        {
            return GetDescriptor("skill.overwhelming_success");
        }

        if (!result.IsSuccess && result.Margin <= -10)
        {
            return GetDescriptor("skill.overwhelming_failure");
        }

        return null;
    }

    /// <summary>
    /// Gets a descriptor for a combat critical hit or miss.
    /// </summary>
    /// <param name="isCriticalHit">True for critical hit, false for critical miss.</param>
    /// <returns>Appropriate combat descriptor.</returns>
    public string? GetCombatCriticalDescriptor(bool isCriticalHit)
    {
        return isCriticalHit
            ? GetDescriptor("combat.critical_hit")
            : GetDescriptor("combat.critical_miss");
    }

    /// <summary>
    /// Ensures descriptors are loaded from configuration.
    /// </summary>
    private void EnsureDescriptorsLoaded()
    {
        if (_descriptors != null) return;

        try
        {
            _descriptors = _configProvider.GetDiceDescriptors();
            _logger.LogInformation("Loaded {Count} descriptor categories", _descriptors?.Count ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load dice descriptors, using defaults");
            _descriptors = GetDefaultDescriptors();
        }
    }

    /// <summary>
    /// Gets default descriptors when configuration is unavailable.
    /// </summary>
    private static IReadOnlyDictionary<string, IReadOnlyList<string>> GetDefaultDescriptors()
    {
        return new Dictionary<string, IReadOnlyList<string>>
        {
            ["dice.natural_max"] = new[]
            {
                "A perfect roll!",
                "Fortune smiles upon you!",
                "The dice glow with luck!"
            },
            ["dice.natural_one"] = new[]
            {
                "A dismal result...",
                "Fate turns against you.",
                "The dice betray you."
            },
            ["skill.critical_success"] = new[]
            {
                "Masterfully done!",
                "A display of true expertise!",
                "Perfection incarnate!"
            },
            ["skill.critical_failure"] = new[]
            {
                "A catastrophic blunder!",
                "Everything that could go wrong, did.",
                "A moment best forgotten..."
            },
            ["combat.critical_hit"] = new[]
            {
                "A devastating blow!",
                "You strike true!",
                "A mighty critical hit!"
            },
            ["combat.critical_miss"] = new[]
            {
                "Your attack goes wildly astray!",
                "A clumsy miss!",
                "You stumble badly!"
            }
        };
    }
}
