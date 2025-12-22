using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for rendering room names and descriptions from templates with variable substitution.
/// Uses IDiceService for random selection to ensure deterministic seeding compatibility.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public class TemplateRendererService : ITemplateRendererService
{
    private readonly IDiceService _diceService;
    private readonly ILogger<TemplateRendererService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateRendererService"/> class.
    /// </summary>
    /// <param name="diceService">The dice service for random selection.</param>
    /// <param name="logger">Logger instance.</param>
    public TemplateRendererService(
        IDiceService diceService,
        ILogger<TemplateRendererService> logger)
    {
        _diceService = diceService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public string RenderRoomName(RoomTemplate template)
    {
        if (template.NameTemplates.Count == 0)
        {
            _logger.LogWarning("[TemplateRenderer] Template '{TemplateId}' has no NameTemplates", template.TemplateId);
            return "Unnamed Room";
        }

        // Select random name template
        var nameTemplate = SelectRandom(template.NameTemplates, "name template");

        // Substitute {Adjective} token
        var adjective = template.Adjectives.Count > 0
            ? SelectRandom(template.Adjectives, "adjective")
            : "Unknown";

        var renderedName = nameTemplate.Replace("{Adjective}", adjective);

        _logger.LogDebug("[TemplateRenderer] Rendered room name: {Name} (template: {TemplateId})",
            renderedName, template.TemplateId);

        return renderedName;
    }

    /// <inheritdoc/>
    public string RenderRoomDescription(RoomTemplate template, BiomeDefinition biome)
    {
        if (template.DescriptionTemplates.Count == 0)
        {
            _logger.LogWarning("[TemplateRenderer] Template '{TemplateId}' has no DescriptionTemplates", template.TemplateId);
            return "This area is shrouded in mystery.";
        }

        // Select random description template
        var descriptionTemplate = SelectRandom(template.DescriptionTemplates, "description template");

        // Substitute {Adjective} token (lowercase for mid-sentence usage)
        var adjective = template.Adjectives.Count > 0
            ? SelectRandom(template.Adjectives, "adjective").ToLowerInvariant()
            : "unknown";

        var description = descriptionTemplate.Replace("{Adjective}", adjective);

        // Substitute {Detail} token
        var detail = template.Details.Count > 0
            ? SelectRandom(template.Details, "detail")
            : string.Empty;

        description = description.Replace("{Detail}", detail);

        // Optional: Append atmospheric details from biome (sounds or smells)
        if (_diceService.RollSingle(100, "Atmospheric detail chance") <= 30) // 30% chance
        {
            var atmospheric = TryAppendAtmosphericDetail(biome);
            if (!string.IsNullOrEmpty(atmospheric))
            {
                description = $"{description.TrimEnd('.')}. {atmospheric}";
            }
        }

        _logger.LogDebug("[TemplateRenderer] Rendered description for template: {TemplateId}", template.TemplateId);

        return description;
    }

    /// <summary>
    /// Attempts to append an atmospheric detail (sound or smell) from the biome.
    /// </summary>
    private string TryAppendAtmosphericDetail(BiomeDefinition biome)
    {
        var categories = biome.DescriptorCategories;

        // Combine sounds and smells into one pool
        var atmosphericPool = new List<string>();

        if (categories.Sounds.Count > 0)
        {
            atmosphericPool.AddRange(categories.Sounds.Select(s => $"You hear {s}"));
        }

        if (categories.Smells.Count > 0)
        {
            atmosphericPool.AddRange(categories.Smells.Select(s => $"The air smells of {s}"));
        }

        if (atmosphericPool.Count == 0)
        {
            return string.Empty;
        }

        return SelectRandom(atmosphericPool, "atmospheric detail");
    }

    /// <summary>
    /// Selects a random item from a list using IDiceService for consistency.
    /// </summary>
    private T SelectRandom<T>(List<T> items, string context)
    {
        if (items.Count == 0)
        {
            throw new InvalidOperationException($"Cannot select random {context} from empty list");
        }

        var index = _diceService.RollSingle(items.Count, context) - 1; // Roll returns 1-based, adjust to 0-based
        return items[index];
    }
}
