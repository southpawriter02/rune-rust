using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for selecting room templates based on context.
/// </summary>
public class TemplateSelectionService : ITemplateSelectionService
{
    private readonly RoomTemplateConfiguration _configuration;
    private readonly ILogger<TemplateSelectionService> _logger;
    private readonly IGameEventLogger? _eventLogger;
    private readonly Random _random = new();

    public TemplateSelectionService(
        RoomTemplateConfiguration configuration,
        ILogger<TemplateSelectionService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;
        _logger.LogDebug("TemplateSelectionService initialized with {Count} templates",
            _configuration.Templates.Count);
    }

    /// <inheritdoc/>
    public RoomTemplate? SelectTemplate(TemplateSelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var validTemplates = GetValidTemplates(context);

        if (validTemplates.Count == 0)
        {
            _logger.LogWarning(
                "No valid templates for context: Biome={Biome}, Depth={Depth}",
                context.Biome, context.Depth);
            return null;
        }

        var selected = WeightedSelect(validTemplates, context.Seed);

        _logger.LogDebug(
            "Selected template {TemplateId} from {Count} valid templates",
            selected.TemplateId, validTemplates.Count);

        _eventLogger?.LogSystem("TemplateSelected", $"Selected template {selected.TemplateId}",
            data: new Dictionary<string, object>
            {
                ["templateId"] = selected.TemplateId,
                ["biome"] = context.Biome ?? "any",
                ["depth"] = context.Depth,
                ["candidateCount"] = validTemplates.Count
            });

        return selected;
    }

    /// <inheritdoc/>
    public IReadOnlyList<RoomTemplate> GetValidTemplates(TemplateSelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var templates = _configuration.Templates.Values
            .Where(t => t.IsValidForBiome(context.Biome))
            .Where(t => t.IsValidForDepth(context.Depth))
            .Where(t => context.RequiredTags.Count == 0 || t.HasAllTags(context.RequiredTags))
            .Where(t => context.ExcludedTags.Count == 0 || t.HasNoTags(context.ExcludedTags))
            .ToList();

        _logger.LogDebug(
            "Found {TemplateCount} valid templates for Biome={BiomeId}, Depth={DepthLevel}",
            templates.Count, context.Biome, context.Depth);

        return templates;
    }

    /// <inheritdoc/>
    public RoomTemplate GetFallbackTemplate(string biome)
    {
        return new RoomTemplate(
            templateId: $"fallback-{biome}",
            namePattern: "Chamber",
            descriptionPattern: "A nondescript chamber.",
            validBiomes: [biome],
            roomType: RoomType.Standard,
            slots: [],
            weight: 1,
            minDepth: 0,
            maxDepth: null,
            tags: ["fallback"]);
    }

    private RoomTemplate WeightedSelect(IReadOnlyList<RoomTemplate> templates, int? seed)
    {
        var random = seed.HasValue ? new Random(seed.Value) : _random;
        var totalWeight = templates.Sum(t => t.Weight);
        var roll = random.Next(totalWeight);
        var cumulative = 0;

        foreach (var template in templates)
        {
            cumulative += template.Weight;
            if (roll < cumulative)
                return template;
        }

        // Fallback (shouldn't reach here if weights > 0)
        return templates[0];
    }
}
