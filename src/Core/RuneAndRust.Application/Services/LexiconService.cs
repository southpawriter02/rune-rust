using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for retrieving and selecting terminology with context-aware synonym support.
/// </summary>
public class LexiconService
{
    private readonly LexiconConfiguration _config;
    private readonly ILogger<LexiconService> _logger;
    private readonly Random _random = new();

    public LexiconService(LexiconConfiguration config, ILogger<LexiconService> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("LexiconService initialized with {TermCount} terms", _config.Terms.Count);
    }

    /// <summary>
    /// Gets a term, optionally selecting a synonym based on context.
    /// </summary>
    /// <param name="termId">The term identifier (e.g., "attack", "hit").</param>
    /// <param name="context">Optional context for selection (e.g., "combat", "formal").</param>
    /// <param name="useSynonym">Whether to potentially use a synonym instead of default.</param>
    /// <returns>The selected term string.</returns>
    public string GetTerm(string termId, string? context = null, bool useSynonym = true)
    {
        if (!_config.Terms.TryGetValue(termId, out var term))
        {
            _logger.LogWarning("Unknown term requested: {TermId}", termId);
            return termId; // Return the ID itself as fallback
        }

        if (!useSynonym)
        {
            return term.Default;
        }

        // Check for contextual override
        if (context != null && term.Contextual.TryGetValue(context, out var contextualSynonyms))
        {
            return SelectWeighted(contextualSynonyms.ToList(), term.Weights);
        }

        // Use general synonyms with weighting
        var allOptions = new List<string> { term.Default };
        allOptions.AddRange(term.Synonyms);
        return SelectWeighted(allOptions, term.Weights);
    }

    /// <summary>
    /// Gets a damage severity descriptor based on percentage of max health.
    /// </summary>
    /// <param name="damagePercent">Damage as percentage of target's max health.</param>
    /// <returns>A severity-appropriate damage word.</returns>
    public string GetDamageSeverity(double damagePercent)
    {
        if (!_config.Terms.TryGetValue("damage", out var term))
            return "damage";

        var severity = damagePercent switch
        {
            < 0.1 => "light",
            < 0.25 => "moderate",
            < 0.5 => "heavy",
            _ => "critical"
        };

        if (term.Severity.TryGetValue(severity, out var options))
        {
            return options[_random.Next(options.Count)];
        }

        return term.Default;
    }

    /// <summary>
    /// Gets a quantity descriptor based on count.
    /// </summary>
    /// <param name="count">The number of items.</param>
    /// <returns>A quantity descriptor phrase.</returns>
    public string GetQuantity(int count)
    {
        if (!_config.Terms.TryGetValue("quantities", out var term))
            return count.ToString();

        var category = count switch
        {
            0 => "none",
            1 => "one",
            <= 3 => "few",
            <= 10 => "many",
            _ => "horde"
        };

        if (term.Severity.TryGetValue(category, out var options))
        {
            return options[_random.Next(options.Count)];
        }

        return count.ToString();
    }

    /// <summary>
    /// Gets a condition descriptor based on health percentage.
    /// </summary>
    /// <param name="healthPercent">Current health as percentage of max.</param>
    /// <returns>A condition descriptor phrase.</returns>
    public string GetCondition(double healthPercent)
    {
        if (!_config.Terms.TryGetValue("conditions", out var term))
            return "wounded";

        var category = healthPercent switch
        {
            >= 1.0 => "healthy",
            >= 0.5 => "wounded",
            >= 0.25 => "bloodied",
            _ => "nearDeath"
        };

        if (term.Severity.TryGetValue(category, out var options))
        {
            return options[_random.Next(options.Count)];
        }

        return "wounded";
    }

    private string SelectWeighted(IList<string> options, IReadOnlyDictionary<string, int>? weights)
    {
        if (options.Count == 0) return string.Empty;
        if (options.Count == 1) return options[0];

        if (weights == null || weights.Count == 0)
        {
            return options[_random.Next(options.Count)];
        }

        var totalWeight = options.Sum(o => weights.GetValueOrDefault(o, 10));
        var roll = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var option in options)
        {
            cumulative += weights.GetValueOrDefault(option, 10);
            if (roll < cumulative)
                return option;
        }

        return options[0];
    }
}
