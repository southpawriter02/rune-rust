using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for validating room template configurations.
/// </summary>
public class TemplateValidationService : ITemplateValidationService
{
    private readonly RoomTemplateConfiguration _configuration;
    private readonly IReadOnlySet<string> _validBiomes;
    private readonly IReadOnlySet<string> _validDescriptorPools;
    private readonly ILogger<TemplateValidationService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public TemplateValidationService(
        RoomTemplateConfiguration configuration,
        ILogger<TemplateValidationService> logger,
        IReadOnlySet<string>? validBiomes = null,
        IReadOnlySet<string>? validDescriptorPools = null,
        IGameEventLogger? eventLogger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validBiomes = validBiomes ?? new HashSet<string> { "dungeon", "cave", "ruins", "volcanic" };
        _validDescriptorPools = validDescriptorPools ?? new HashSet<string>();
        _eventLogger = eventLogger;
    }

    /// <inheritdoc/>
    public TemplateValidationResult ValidateTemplate(RoomTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate template ID
        if (string.IsNullOrWhiteSpace(template.TemplateId))
        {
            errors.Add("Template ID is required.");
        }

        // Validate name pattern
        if (string.IsNullOrWhiteSpace(template.NamePattern))
        {
            errors.Add("Name pattern is required.");
        }

        // Validate biomes
        if (template.ValidBiomes.Count == 0)
        {
            errors.Add("At least one valid biome is required.");
        }
        else if (_validBiomes.Count > 0)
        {
            foreach (var biome in template.ValidBiomes)
            {
                if (!_validBiomes.Contains(biome))
                {
                    warnings.Add($"Biome '{biome}' is not in the known biome list.");
                }
            }
        }

        // Validate depth range
        if (template.MinDepth < 0)
        {
            errors.Add("Minimum depth cannot be negative.");
        }

        if (template.MaxDepth.HasValue && template.MaxDepth < template.MinDepth)
        {
            errors.Add("Maximum depth cannot be less than minimum depth.");
        }

        // Validate weight
        if (template.Weight <= 0)
        {
            errors.Add("Weight must be positive.");
        }

        // Validate slots
        var slotIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var slot in template.Slots)
        {
            if (!slotIds.Add(slot.SlotId))
            {
                errors.Add($"Duplicate slot ID: '{slot.SlotId}'.");
            }

            var slotErrors = ValidateSlot(slot, template.TemplateId);
            errors.AddRange(slotErrors);
        }

        // Validate pattern placeholders match slots
        ValidatePatternPlaceholders(template, errors, warnings);

        _logger.LogDebug(
            "Validated template {TemplateId}: {ErrorCount} errors, {WarningCount} warnings",
            template.TemplateId, errors.Count, warnings.Count);

        return new TemplateValidationResult
        {
            TemplateId = template.TemplateId,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <inheritdoc/>
    public IReadOnlyList<TemplateValidationResult> ValidateAllTemplates()
    {
        var results = new List<TemplateValidationResult>();

        foreach (var template in _configuration.Templates.Values)
        {
            results.Add(ValidateTemplate(template));
        }

        var errorCount = results.Count(r => !r.IsValid);
        _logger.LogInformation(
            "Validated {Count} templates: {ErrorCount} with errors",
            results.Count, errorCount);

        _eventLogger?.LogSystem("TemplatesValidated", $"Validated {results.Count} templates",
            data: new Dictionary<string, object>
            {
                ["templateCount"] = results.Count,
                ["errorCount"] = errorCount,
                ["validCount"] = results.Count - errorCount
            });

        return results;
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> ValidateSlot(TemplateSlot slot, string templateId)
    {
        var errors = new List<string>();
        var prefix = $"[{templateId}/{slot.SlotId}]";

        // Validate slot ID
        if (string.IsNullOrWhiteSpace(slot.SlotId))
        {
            errors.Add($"{prefix} Slot ID is required.");
        }

        // Validate quantity range
        if (slot.MinQuantity < 0)
        {
            errors.Add($"{prefix} Minimum quantity cannot be negative.");
        }

        if (slot.MaxQuantity < slot.MinQuantity)
        {
            errors.Add($"{prefix} Maximum quantity cannot be less than minimum.");
        }

        // Validate fill probability
        if (slot.FillProbability < 0 || slot.FillProbability > 1)
        {
            errors.Add($"{prefix} Fill probability must be between 0.0 and 1.0.");
        }

        // Type-specific validation
        switch (slot.Type)
        {
            case SlotType.Description:
                if (string.IsNullOrWhiteSpace(slot.DescriptorPool))
                {
                    errors.Add($"{prefix} Description slots require a descriptor pool.");
                }
                else if (_validDescriptorPools.Count > 0 && !_validDescriptorPools.Contains(slot.DescriptorPool))
                {
                    errors.Add($"{prefix} Unknown descriptor pool: '{slot.DescriptorPool}'.");
                }
                break;

            case SlotType.Monster:
                ValidateMonsterConstraints(slot, prefix, errors);
                break;

            case SlotType.Container:
                ValidateContainerConstraints(slot, prefix, errors);
                break;
        }

        return errors;
    }

    private void ValidatePatternPlaceholders(RoomTemplate template, List<string> errors, List<string> warnings)
    {
        var namePlaceholders = ExtractPlaceholders(template.NamePattern);
        var descPlaceholders = ExtractPlaceholders(template.DescriptionPattern);
        var allPlaceholders = namePlaceholders.Union(descPlaceholders).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var descriptionSlotIds = template.Slots
            .Where(s => s.Type == SlotType.Description)
            .Select(s => s.SlotId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var placeholder in allPlaceholders)
        {
            if (!descriptionSlotIds.Contains(placeholder))
            {
                warnings.Add($"Placeholder '{{{placeholder}}}' has no corresponding Description slot.");
            }
        }

        foreach (var slotId in descriptionSlotIds)
        {
            if (!allPlaceholders.Contains(slotId))
            {
                warnings.Add($"Description slot '{slotId}' is not referenced in any pattern.");
            }
        }
    }

    private static HashSet<string> ExtractPlaceholders(string pattern)
    {
        var placeholders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var matches = Regex.Matches(pattern, @"\{([^}]+)\}");

        foreach (Match match in matches)
        {
            placeholders.Add(match.Groups[1].Value);
        }

        return placeholders;
    }

    private static void ValidateMonsterConstraints(TemplateSlot slot, string prefix, List<string> errors)
    {
        var minTier = slot.GetConstraint("minTier");
        var maxTier = slot.GetConstraint("maxTier");

        var validTiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "minion", "standard", "elite", "named", "boss"
        };

        if (minTier != null && !validTiers.Contains(minTier))
        {
            errors.Add($"{prefix} Invalid minTier: '{minTier}'.");
        }

        if (maxTier != null && !validTiers.Contains(maxTier))
        {
            errors.Add($"{prefix} Invalid maxTier: '{maxTier}'.");
        }
    }

    private static void ValidateContainerConstraints(TemplateSlot slot, string prefix, List<string> errors)
    {
        var lootQuality = slot.GetConstraint("lootQuality");

        var validQualities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "low", "medium", "high", "treasure"
        };

        if (lootQuality != null && !validQualities.Contains(lootQuality))
        {
            errors.Add($"{prefix} Invalid lootQuality: '{lootQuality}'.");
        }
    }
}
