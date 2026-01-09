using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Validates environment contexts and builds coherent environments from biomes.
/// </summary>
public class EnvironmentCoherenceService
{
    private readonly EnvironmentCategoryConfiguration _categoryConfig;
    private readonly BiomeConfiguration _biomeConfig;
    private readonly ILogger<EnvironmentCoherenceService> _logger;

    public EnvironmentCoherenceService(
        EnvironmentCategoryConfiguration categoryConfig,
        BiomeConfiguration biomeConfig,
        ILogger<EnvironmentCoherenceService> logger)
    {
        _categoryConfig = categoryConfig ?? throw new ArgumentNullException(nameof(categoryConfig));
        _biomeConfig = biomeConfig ?? throw new ArgumentNullException(nameof(biomeConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "EnvironmentCoherenceService initialized with {CategoryCount} categories, {RuleCount} exclusion rules, {BiomeCount} biomes",
            _categoryConfig.Categories.Count,
            _categoryConfig.ExclusionRules.Count,
            _biomeConfig.Biomes.Count);
    }

    /// <summary>
    /// Validates that an environment context has no conflicting values.
    /// </summary>
    /// <param name="context">The environment context to validate.</param>
    /// <returns>A validation result with any violations found.</returns>
    public EnvironmentValidationResult Validate(EnvironmentContext context)
    {
        var violations = new List<EnvironmentViolation>();

        foreach (var rule in _categoryConfig.ExclusionRules)
        {
            if (ViolatesRule(context, rule, out var violation))
            {
                violations.Add(violation);

                if (rule.IsHardRule)
                {
                    _logger.LogWarning(
                        "Environment violation (hard): {RuleId} - {Reason}",
                        rule.Id, rule.Reason);
                }
                else
                {
                    _logger.LogDebug(
                        "Environment violation (soft): {RuleId} - {Reason}",
                        rule.Id, rule.Reason);
                }
            }
        }

        return new EnvironmentValidationResult(violations);
    }

    /// <summary>
    /// Creates an environment context from a biome with default values.
    /// </summary>
    /// <param name="biomeId">The biome identifier.</param>
    /// <param name="overrides">Optional category value overrides.</param>
    /// <returns>A coherent environment context.</returns>
    /// <exception cref="ArgumentException">Thrown if biome is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the context violates hard exclusion rules.</exception>
    public EnvironmentContext CreateFromBiome(
        string biomeId,
        IReadOnlyDictionary<string, string>? overrides = null)
    {
        if (!_biomeConfig.Biomes.TryGetValue(biomeId, out var biome))
        {
            _logger.LogError("Biome not found: {BiomeId}", biomeId);
            throw new ArgumentException($"Biome not found: {biomeId}", nameof(biomeId));
        }

        _logger.LogDebug("Creating environment context from biome: {BiomeId}", biomeId);

        // Start with biome defaults
        var categoryValues = new Dictionary<string, string>(biome.DefaultCategoryValues)
        {
            ["biome"] = biomeId
        };

        // Apply category defaults for any missing required categories
        foreach (var (categoryId, category) in _categoryConfig.Categories)
        {
            if (category.IsRequired && !categoryValues.ContainsKey(categoryId))
            {
                if (category.DefaultValue != null)
                {
                    categoryValues[categoryId] = category.DefaultValue;
                }
            }
        }

        // Apply overrides
        if (overrides != null)
        {
            foreach (var (categoryId, valueId) in overrides)
            {
                categoryValues[categoryId] = valueId;
            }
        }

        // Collect derived tags
        var derivedTags = CollectDerivedTags(categoryValues, biome);

        var context = new EnvironmentContext(categoryValues, derivedTags);

        // Validate result
        var validation = Validate(context);
        if (validation.HasHardViolations)
        {
            _logger.LogError(
                "Created environment context has violations: {ViolationCount} violations",
                validation.Violations.Count);
            throw new InvalidOperationException(
                $"Environment context violates exclusion rules: {validation.Violations[0].Reason}");
        }

        _logger.LogDebug(
            "Created environment context: {Context} with {TagCount} derived tags",
            context, derivedTags.Count);

        return context;
    }

    /// <summary>
    /// Gets all valid values for a category given the current context.
    /// </summary>
    /// <param name="categoryId">The category to query.</param>
    /// <param name="currentContext">The current environment context.</param>
    /// <returns>Values that would not violate any hard exclusion rules.</returns>
    public IEnumerable<CategoryValue> GetValidValues(
        string categoryId,
        EnvironmentContext currentContext)
    {
        if (!_categoryConfig.Categories.TryGetValue(categoryId, out var category))
        {
            _logger.LogWarning("Category not found: {CategoryId}", categoryId);
            return [];
        }

        return category.Values.Where(value =>
        {
            var testContext = currentContext.WithValue(categoryId, value.Id);
            var validation = Validate(testContext);
            return !validation.HasHardViolations;
        });
    }

    /// <summary>
    /// Gets a biome definition by ID.
    /// </summary>
    /// <param name="biomeId">The biome identifier.</param>
    /// <returns>The biome definition if found, otherwise null.</returns>
    public BiomeDefinition? GetBiome(string biomeId)
    {
        return _biomeConfig.Biomes.TryGetValue(biomeId, out var biome) ? biome : null;
    }

    /// <summary>
    /// Gets all available biome definitions.
    /// </summary>
    public IEnumerable<BiomeDefinition> GetAllBiomes() => _biomeConfig.Biomes.Values;

    /// <summary>
    /// Gets all environment categories.
    /// </summary>
    public IEnumerable<EnvironmentCategory> GetAllCategories() => _categoryConfig.Categories.Values;

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The category if found, otherwise null.</returns>
    public EnvironmentCategory? GetCategory(string categoryId)
    {
        return _categoryConfig.Categories.TryGetValue(categoryId, out var category) ? category : null;
    }

    private bool ViolatesRule(
        EnvironmentContext context,
        CategoryExclusionRule rule,
        out EnvironmentViolation violation)
    {
        var value1 = context.GetValue(rule.Category1);
        var value2 = context.GetValue(rule.Category2);

        if (value1 != null && value2 != null &&
            rule.Values1.Contains(value1) && rule.Values2.Contains(value2))
        {
            violation = new EnvironmentViolation(
                rule.Id,
                rule.Reason,
                rule.Category1, value1,
                rule.Category2, value2,
                rule.IsHardRule);
            return true;
        }

        violation = default!;
        return false;
    }

    private List<string> CollectDerivedTags(
        Dictionary<string, string> categoryValues,
        BiomeDefinition biome)
    {
        var tags = new List<string>();

        // Add biome-implied tags
        tags.AddRange(biome.ImpliedTags);

        // Add category value-implied tags
        foreach (var (categoryId, valueId) in categoryValues)
        {
            if (_categoryConfig.Categories.TryGetValue(categoryId, out var category))
            {
                var value = category.Values.FirstOrDefault(v => v.Id == valueId);
                if (value != null)
                {
                    tags.AddRange(value.ImpliedTags);
                }
            }
        }

        return tags.Distinct().ToList();
    }
}

/// <summary>
/// Result of environment context validation.
/// </summary>
/// <param name="Violations">List of exclusion rule violations.</param>
public record EnvironmentValidationResult(IReadOnlyList<EnvironmentViolation> Violations)
{
    /// <summary>
    /// Returns true if the environment is valid (no violations).
    /// </summary>
    public bool IsValid => Violations.Count == 0;

    /// <summary>
    /// Returns true if there are any hard rule violations.
    /// </summary>
    public bool HasHardViolations => Violations.Any(v => v.IsHardRule);

    /// <summary>
    /// Returns true if there are only soft rule violations.
    /// </summary>
    public bool HasSoftViolationsOnly => Violations.Count > 0 && !HasHardViolations;
}

/// <summary>
/// Represents a violation of an exclusion rule.
/// </summary>
/// <param name="RuleId">The violated rule identifier.</param>
/// <param name="Reason">Human-readable explanation of the conflict.</param>
/// <param name="Category1">First conflicting category.</param>
/// <param name="Value1">First conflicting value.</param>
/// <param name="Category2">Second conflicting category.</param>
/// <param name="Value2">Second conflicting value.</param>
/// <param name="IsHardRule">Whether this is a hard (error) or soft (warning) rule.</param>
public record EnvironmentViolation(
    string RuleId,
    string Reason,
    string Category1,
    string Value1,
    string Category2,
    string Value2,
    bool IsHardRule);
