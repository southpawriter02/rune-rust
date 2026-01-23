namespace RuneAndRust.Application.Services;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides narrative descriptors for skill check outcomes from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// Loads skill-specific descriptor pools from <c>skill-descriptors.json</c>
/// and selects appropriate text based on outcome category and context.
/// </para>
/// <para>
/// Context priority for descriptor selection:
/// <list type="number">
///   <item>Corruption context (glitched, blighted, resonance)</item>
///   <item>Surface type context (wet, compromised, etc.)</item>
///   <item>Target disposition context (hostile, friendly)</item>
///   <item>Default pool</item>
/// </list>
/// </para>
/// </remarks>
public class VoiceGuidanceService : IVoiceGuidanceService
{
    private readonly ILogger<VoiceGuidanceService> _logger;
    private readonly Dictionary<string, SkillDescriptorConfig> _skills = new();
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VoiceGuidanceService"/> class.
    /// </summary>
    /// <param name="configurationPath">Path to the skill-descriptors.json file.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public VoiceGuidanceService(
        string configurationPath,
        ILogger<VoiceGuidanceService> logger)
    {
        _logger = logger;
        LoadConfiguration(configurationPath);
    }

    /// <inheritdoc/>
    public SkillDescriptor GetDescriptor(
        string skillId,
        DescriptorCategory category,
        SkillContext? context = null)
    {
        var normalizedSkillId = skillId.ToLowerInvariant();
        var categoryKey = GetCategoryKey(category);

        _logger.LogTrace(
            "Getting descriptor for {SkillId}, category {Category}",
            skillId,
            category);

        // Try skill-specific descriptor
        if (_skills.TryGetValue(normalizedSkillId, out var skillConfig))
        {
            var descriptor = SelectDescriptor(skillConfig, categoryKey, context, skillId, category);
            if (descriptor.HasContent)
            {
                _logger.LogDebug(
                    "Selected descriptor for {SkillId}/{Category}: contextual={IsContextual}",
                    skillId,
                    category,
                    descriptor.IsContextual);
                return descriptor;
            }
        }

        // Fall back to generic descriptor
        _logger.LogTrace("Using generic fallback for {SkillId}/{Category}", skillId, category);
        return SkillDescriptor.GenericFallback(category);
    }

    /// <inheritdoc/>
    public SkillDescriptor GetDescriptor(
        string skillId,
        OutcomeDetails outcomeDetails,
        SkillContext? context = null)
    {
        return GetDescriptor(skillId, outcomeDetails.DescriptorCategory, context);
    }

    /// <inheritdoc/>
    public IReadOnlyList<DescriptorCategory> GetAvailableCategories(string skillId)
    {
        var normalizedSkillId = skillId.ToLowerInvariant();

        if (!_skills.TryGetValue(normalizedSkillId, out var skillConfig))
        {
            _logger.LogTrace("No categories found for skill: {SkillId}", skillId);
            return Array.Empty<DescriptorCategory>();
        }

        var categories = skillConfig.Categories.Keys
            .Select(ParseCategoryKey)
            .Where(c => c.HasValue)
            .Select(c => c!.Value)
            .ToList();

        _logger.LogTrace(
            "Found {Count} categories for skill: {SkillId}",
            categories.Count,
            skillId);

        return categories;
    }

    /// <inheritdoc/>
    public bool HasDescriptors(string skillId)
    {
        var normalizedSkillId = skillId.ToLowerInvariant();
        return _skills.ContainsKey(normalizedSkillId);
    }

    /// <inheritdoc/>
    public int GetPoolSize(string skillId, DescriptorCategory category)
    {
        var normalizedSkillId = skillId.ToLowerInvariant();
        var categoryKey = GetCategoryKey(category);

        if (!_skills.TryGetValue(normalizedSkillId, out var skillConfig))
            return 0;

        if (!skillConfig.Categories.TryGetValue(categoryKey, out var categoryConfig))
            return 0;

        return categoryConfig.Default?.Count ?? 0;
    }

    /// <summary>
    /// Selects a descriptor from the skill configuration, considering context.
    /// </summary>
    private SkillDescriptor SelectDescriptor(
        SkillDescriptorConfig skillConfig,
        string categoryKey,
        SkillContext? context,
        string skillId,
        DescriptorCategory category)
    {
        if (!skillConfig.Categories.TryGetValue(categoryKey, out var categoryConfig))
        {
            return SkillDescriptor.Empty(skillId, category);
        }

        // Try context-specific descriptors first
        if (context != null)
        {
            var contextType = DetermineContextType(context);
            if (!string.IsNullOrEmpty(contextType))
            {
                // Check if we have a pool matching the category key with context
                var contextualDescriptor = TryGetContextualDescriptor(
                    categoryConfig, contextType, skillId, category);

                if (contextualDescriptor.HasContent)
                {
                    return contextualDescriptor;
                }
            }
        }

        // Fall back to default pool
        if (categoryConfig.Default != null && categoryConfig.Default.Count > 0)
        {
            var text = SelectRandomFromPool(categoryConfig.Default);
            return new SkillDescriptor(
                SkillId: skillId,
                Category: category,
                Text: text);
        }

        return SkillDescriptor.Empty(skillId, category);
    }

    /// <summary>
    /// Attempts to get a contextual descriptor from the category config.
    /// </summary>
    private SkillDescriptor TryGetContextualDescriptor(
        CategoryConfig categoryConfig,
        string contextType,
        string skillId,
        DescriptorCategory category)
    {
        // First check for exact context match in the category's additional properties
        // Context pools are stored as additional keys in the category config
        if (categoryConfig.Contextual != null &&
            categoryConfig.Contextual.TryGetValue(contextType, out var contextPool) &&
            contextPool.Count > 0)
        {
            var text = SelectRandomFromPool(contextPool);
            return new SkillDescriptor(
                SkillId: skillId,
                Category: category,
                Text: text,
                IsContextual: true,
                ContextType: contextType);
        }

        return SkillDescriptor.Empty(skillId, category);
    }

    /// <summary>
    /// Determines the context type from the skill context, in priority order.
    /// </summary>
    private static string? DetermineContextType(SkillContext context)
    {
        // Priority 1: Corruption
        foreach (var mod in context.EnvironmentModifiers)
        {
            if (mod.CorruptionTier.HasValue && mod.CorruptionTier.Value != CorruptionTier.Normal)
            {
                return mod.CorruptionTier.Value.ToString().ToLowerInvariant();
            }
        }

        // Priority 2: Surface type
        foreach (var mod in context.EnvironmentModifiers)
        {
            if (mod.SurfaceType.HasValue)
            {
                return mod.SurfaceType.Value.ToString().ToLowerInvariant();
            }
        }

        // Priority 3: Lighting
        foreach (var mod in context.EnvironmentModifiers)
        {
            if (mod.LightingLevel.HasValue && mod.LightingLevel.Value != LightingLevel.Normal)
            {
                return mod.LightingLevel.Value.ToString().ToLowerInvariant();
            }
        }

        // Priority 4: Target disposition (from ID pattern)
        foreach (var mod in context.TargetModifiers)
        {
            if (mod.ModifierId.Contains("hostile", StringComparison.OrdinalIgnoreCase))
                return "hostile";
            if (mod.ModifierId.Contains("friendly", StringComparison.OrdinalIgnoreCase))
                return "friendly";
        }

        return null;
    }

    /// <summary>
    /// Selects a random descriptor from a pool.
    /// </summary>
    private string SelectRandomFromPool(List<string> pool)
    {
        if (pool.Count == 0)
            return string.Empty;

        var index = _random.Next(pool.Count);
        return pool[index];
    }

    /// <summary>
    /// Converts a DescriptorCategory to a JSON key.
    /// </summary>
    private static string GetCategoryKey(DescriptorCategory category)
    {
        return category.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Parses a category key back to an enum value.
    /// </summary>
    private static DescriptorCategory? ParseCategoryKey(string key)
    {
        return key.ToLowerInvariant() switch
        {
            "catastrophic" => DescriptorCategory.Catastrophic,
            "failed" => DescriptorCategory.Failed,
            "marginal" => DescriptorCategory.Marginal,
            "competent" => DescriptorCategory.Competent,
            "impressive" => DescriptorCategory.Impressive,
            "masterful" => DescriptorCategory.Masterful,
            _ => null
        };
    }

    /// <summary>
    /// Loads descriptor configuration from JSON file.
    /// </summary>
    private void LoadConfiguration(string configurationPath)
    {
        try
        {
            if (!File.Exists(configurationPath))
            {
                _logger.LogWarning(
                    "Skill descriptors configuration not found at {Path}",
                    configurationPath);
                return;
            }

            var json = File.ReadAllText(configurationPath);
            var config = JsonSerializer.Deserialize<SkillDescriptorsRootConfig>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (config?.Skills != null)
            {
                foreach (var (skillId, skillConfig) in config.Skills)
                {
                    _skills[skillId.ToLowerInvariant()] = skillConfig;
                }

                _logger.LogInformation(
                    "Loaded skill descriptors for {Count} skills",
                    _skills.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load skill descriptors configuration");
        }
    }

    #region Configuration DTOs

    private class SkillDescriptorsRootConfig
    {
        public Dictionary<string, SkillDescriptorConfig> Skills { get; set; } = new();
    }

    private class SkillDescriptorConfig
    {
        public Dictionary<string, CategoryConfig> Categories { get; set; } = new();
    }

    private class CategoryConfig
    {
        public List<string>? Default { get; set; }
        public Dictionary<string, List<string>>? Contextual { get; set; }
    }

    #endregion
}
