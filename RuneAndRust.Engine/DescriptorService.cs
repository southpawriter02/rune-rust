using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38: Service for the Descriptor Library Framework
/// Provides high-level API for working with base templates, modifiers, and composites
/// Includes composition logic, querying, and weighted selection
/// </summary>
public class DescriptorService : IDescriptorService
{
    private static readonly ILogger _log = Log.ForContext<DescriptorService>();
    private readonly DescriptorRepository _repository;
    private readonly Random _random;

    public DescriptorService(DescriptorRepository repository)
    {
        _repository = repository;
        _random = new Random();
        _log.Information("DescriptorService initialized");
    }

    #region Tier 1: Base Templates

    public DescriptorBaseTemplate? GetBaseTemplate(string templateName)
    {
        _log.Debug("Getting base template: {TemplateName}", templateName);
        return _repository.GetBaseTemplateByName(templateName);
    }

    public DescriptorBaseTemplate? GetBaseTemplate(int templateId)
    {
        _log.Debug("Getting base template: ID {TemplateId}", templateId);
        return _repository.GetBaseTemplateById(templateId);
    }

    public List<DescriptorBaseTemplate> GetBaseTemplatesByCategory(string category)
    {
        _log.Debug("Getting base templates by category: {Category}", category);
        return _repository.GetBaseTemplatesByCategory(category);
    }

    public List<DescriptorBaseTemplate> GetBaseTemplatesByArchetype(string archetype)
    {
        _log.Debug("Getting base templates by archetype: {Archetype}", archetype);
        return _repository.GetBaseTemplatesByArchetype(archetype);
    }

    public List<DescriptorBaseTemplate> GetAllBaseTemplates()
    {
        _log.Debug("Getting all base templates");
        return _repository.GetAllBaseTemplates();
    }

    #endregion

    #region Tier 2: Thematic Modifiers

    public ThematicModifier? GetModifier(string modifierName)
    {
        _log.Debug("Getting modifier: {ModifierName}", modifierName);
        return _repository.GetModifierByName(modifierName);
    }

    public ThematicModifier? GetModifier(int modifierId)
    {
        _log.Debug("Getting modifier: ID {ModifierId}", modifierId);
        return _repository.GetModifierById(modifierId);
    }

    public List<ThematicModifier> GetModifiersForBiome(string biomeName)
    {
        _log.Debug("Getting modifiers for biome: {BiomeName}", biomeName);
        return _repository.GetModifiersForBiome(biomeName);
    }

    public List<ThematicModifier> GetAllModifiers()
    {
        _log.Debug("Getting all modifiers");
        return _repository.GetAllModifiers();
    }

    #endregion

    #region Tier 3: Composite Descriptors

    public DescriptorComposite? GetComposite(int compositeId)
    {
        _log.Debug("Getting composite: ID {CompositeId}", compositeId);
        var composite = _repository.GetCompositeById(compositeId);

        if (composite != null)
        {
            // Populate navigation properties
            composite.BaseTemplate = GetBaseTemplate(composite.BaseTemplateId);
            if (composite.ModifierId.HasValue)
            {
                composite.Modifier = GetModifier(composite.ModifierId.Value);
            }
        }

        return composite;
    }

    public DescriptorComposite? GetCompositeByName(string finalName)
    {
        _log.Debug("Getting composite by name: {FinalName}", finalName);
        var composite = _repository.GetCompositeByName(finalName);

        if (composite != null)
        {
            // Populate navigation properties
            composite.BaseTemplate = GetBaseTemplate(composite.BaseTemplateId);
            if (composite.ModifierId.HasValue)
            {
                composite.Modifier = GetModifier(composite.ModifierId.Value);
            }
        }

        return composite;
    }

    public DescriptorComposite ComposeDescriptor(string baseTemplateName, string? modifierName = null)
    {
        _log.Information("Composing descriptor: {BaseTemplate} + {Modifier}",
            baseTemplateName, modifierName ?? "none");

        // Get base template
        var baseTemplate = GetBaseTemplate(baseTemplateName);
        if (baseTemplate == null)
        {
            _log.Error("Base template not found: {TemplateName}", baseTemplateName);
            throw new ArgumentException($"Base template not found: {baseTemplateName}");
        }

        // Get modifier (if specified)
        ThematicModifier? modifier = null;
        if (!string.IsNullOrEmpty(modifierName))
        {
            modifier = GetModifier(modifierName);
            if (modifier == null)
            {
                _log.Error("Modifier not found: {ModifierName}", modifierName);
                throw new ArgumentException($"Modifier not found: {modifierName}");
            }
        }

        // Check if composite already exists in database
        var existingComposite = _repository.QueryComposites(new DescriptorQuery
        {
            BaseTemplateName = baseTemplateName,
            ModifierName = modifierName
        }).FirstOrDefault();

        if (existingComposite != null)
        {
            _log.Debug("Using existing composite: {FinalName}", existingComposite.FinalName);
            existingComposite.BaseTemplate = baseTemplate;
            existingComposite.Modifier = modifier;
            return existingComposite;
        }

        // Generate new composite on-the-fly
        _log.Debug("Generating new composite on-the-fly");
        return GenerateComposite(baseTemplate, modifier);
    }

    public List<DescriptorComposite> GetCompositesForBaseTemplate(int baseTemplateId)
    {
        _log.Debug("Getting composites for base template: ID {BaseTemplateId}", baseTemplateId);
        return _repository.GetCompositesForBaseTemplate(baseTemplateId);
    }

    public List<DescriptorComposite> GetCompositesForModifier(int modifierId)
    {
        _log.Debug("Getting composites for modifier: ID {ModifierId}", modifierId);
        return _repository.GetCompositesForModifier(modifierId);
    }

    #endregion

    #region Query System

    public DescriptorQueryResult QueryDescriptors(DescriptorQuery query)
    {
        _log.Information("Querying descriptors: Category={Category}, Archetype={Archetype}, Biome={Biome}",
            query.Category, query.Archetype, query.Biome);

        var composites = _repository.QueryComposites(query);

        // Post-processing: Filter by tags if specified
        if (query.RequiredTags != null && query.RequiredTags.Count > 0)
        {
            composites = composites.Where(c =>
            {
                var baseTemplate = GetBaseTemplate(c.BaseTemplateId);
                if (baseTemplate == null) return false;

                var tags = baseTemplate.GetTags();
                return query.RequiredTags.All(requiredTag => tags.Contains(requiredTag));
            }).ToList();

            _log.Debug("Filtered by required tags: {Count} remaining", composites.Count);
        }

        if (query.ExcludedTags != null && query.ExcludedTags.Count > 0)
        {
            composites = composites.Where(c =>
            {
                var baseTemplate = GetBaseTemplate(c.BaseTemplateId);
                if (baseTemplate == null) return true;

                var tags = baseTemplate.GetTags();
                return !query.ExcludedTags.Any(excludedTag => tags.Contains(excludedTag));
            }).ToList();

            _log.Debug("Filtered by excluded tags: {Count} remaining", composites.Count);
        }

        // Populate navigation properties
        foreach (var composite in composites)
        {
            composite.BaseTemplate = GetBaseTemplate(composite.BaseTemplateId);
            if (composite.ModifierId.HasValue)
            {
                composite.Modifier = GetModifier(composite.ModifierId.Value);
            }
        }

        _log.Information("Query complete: {Count} descriptors found", composites.Count);

        return new DescriptorQueryResult
        {
            Descriptors = composites,
            TotalCount = composites.Count,
            Query = query
        };
    }

    public DescriptorComposite? WeightedRandomSelection(List<DescriptorComposite> descriptors, Random? random = null)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            _log.Warning("WeightedRandomSelection called with empty list");
            return null;
        }

        var rng = random ?? _random;

        // Calculate total weight
        float totalWeight = descriptors.Sum(d => d.SpawnWeight);
        if (totalWeight <= 0)
        {
            _log.Warning("Total weight is 0, selecting first descriptor");
            return descriptors[0];
        }

        // Random selection
        float randomValue = (float)(rng.NextDouble() * totalWeight);
        float cumulativeWeight = 0f;

        foreach (var descriptor in descriptors)
        {
            cumulativeWeight += descriptor.SpawnWeight;
            if (randomValue <= cumulativeWeight)
            {
                _log.Debug("Selected descriptor: {Name} (weight {Weight}/{Total})",
                    descriptor.FinalName, descriptor.SpawnWeight, totalWeight);
                return descriptor;
            }
        }

        // Fallback to last descriptor (should never happen)
        _log.Warning("WeightedRandomSelection fallback, selecting last descriptor");
        return descriptors[^1];
    }

    #endregion

    #region Generation & Composition

    public DescriptorComposite GenerateComposite(
        DescriptorBaseTemplate baseTemplate,
        ThematicModifier? modifier = null)
    {
        _log.Debug("Generating composite: {BaseTemplate} + {Modifier}",
            baseTemplate.TemplateName, modifier?.ModifierName ?? "none");

        var composite = new DescriptorComposite
        {
            BaseTemplateId = baseTemplate.TemplateId,
            ModifierId = modifier?.ModifierId,
            BaseTemplate = baseTemplate,
            Modifier = modifier
        };

        // Generate final name by replacing placeholders
        composite.FinalName = GenerateFinalName(baseTemplate, modifier);

        // Generate final description
        composite.FinalDescription = GenerateFinalDescription(baseTemplate, modifier);

        // Merge mechanics
        composite.FinalMechanics = MergeMechanics(baseTemplate.BaseMechanics, modifier?.StatModifiers);

        // Set biome restrictions
        if (modifier != null)
        {
            composite.BiomeRestrictions = JsonSerializer.Serialize(new[] { modifier.PrimaryBiome });
        }

        // Default spawn weight
        composite.SpawnWeight = 1.0f;

        _log.Information("Generated composite: {FinalName}", composite.FinalName);

        return composite;
    }

    public string MergeMechanics(string? baseMechanics, string? statModifiers)
    {
        var merged = new Dictionary<string, object>();

        // Parse base mechanics
        if (!string.IsNullOrEmpty(baseMechanics))
        {
            try
            {
                var baseDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(baseMechanics);
                if (baseDict != null)
                {
                    foreach (var kvp in baseDict)
                    {
                        merged[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to parse base mechanics: {BaseMechanics}", baseMechanics);
            }
        }

        // Parse and apply stat modifiers
        if (!string.IsNullOrEmpty(statModifiers))
        {
            try
            {
                var modDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(statModifiers);
                if (modDict != null)
                {
                    foreach (var kvp in modDict)
                    {
                        var key = kvp.Key;
                        var value = kvp.Value;

                        // Check for multipliers (e.g., "hp_multiplier")
                        if (key.EndsWith("_multiplier"))
                        {
                            var baseKey = key.Replace("_multiplier", "");
                            if (merged.ContainsKey(baseKey) && value.ValueKind == JsonValueKind.Number)
                            {
                                // Apply multiplier
                                var baseValue = GetNumericValue(merged[baseKey]);
                                var multiplier = value.GetDouble();
                                merged[baseKey] = baseValue * multiplier;

                                _log.Debug("Applied multiplier: {Key} * {Multiplier} = {Result}",
                                    baseKey, multiplier, merged[baseKey]);
                            }
                        }
                        else
                        {
                            // Direct override or addition
                            merged[key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to parse stat modifiers: {StatModifiers}", statModifiers);
            }
        }

        return JsonSerializer.Serialize(merged);
    }

    private double GetNumericValue(object value)
    {
        if (value is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDouble();
            }
        }
        else if (value is int intValue)
        {
            return intValue;
        }
        else if (value is double doubleValue)
        {
            return doubleValue;
        }
        else if (value is float floatValue)
        {
            return floatValue;
        }

        return 0;
    }

    private string GenerateFinalName(DescriptorBaseTemplate baseTemplate, ThematicModifier? modifier)
    {
        var name = baseTemplate.NameTemplate;

        if (modifier != null)
        {
            // Replace {Modifier} with modifier name
            name = name.Replace("{Modifier}", modifier.ModifierName);
        }
        else
        {
            // Remove {Modifier} placeholder for unmodified templates
            name = name.Replace("{Modifier} ", "");
            name = name.Replace("{Modifier}", "");
        }

        return name.Trim();
    }

    private string GenerateFinalDescription(DescriptorBaseTemplate baseTemplate, ThematicModifier? modifier)
    {
        var description = baseTemplate.DescriptionTemplate;

        if (modifier != null)
        {
            // Replace {Modifier_Adj} with modifier adjective
            description = description.Replace("{Modifier_Adj}", modifier.Adjective);

            // Replace {Modifier_Detail} with modifier detail fragment
            description = description.Replace("{Modifier_Detail}", modifier.DetailFragment);

            // Replace {Modifier} with modifier name
            description = description.Replace("{Modifier}", modifier.ModifierName.ToLower());
        }
        else
        {
            // Remove modifier placeholders for unmodified templates
            description = description.Replace("{Modifier_Adj}", "standard");
            description = description.Replace("{Modifier_Detail}", "shows typical wear");
            description = description.Replace("{Modifier}", "neutral");
        }

        return description.Trim();
    }

    #endregion

    #region Utility

    public bool CanSpawnInRoom(DescriptorComposite composite, Room room)
    {
        var spawnRules = composite.GetSpawnRules();
        if (spawnRules == null)
        {
            _log.Debug("No spawn rules for {Composite}, allowing spawn", composite.FinalName);
            return true;  // No restrictions
        }

        // Check min_room_size
        if (spawnRules.TryGetValue("min_room_size", out var minSizeObj))
        {
            var minSize = minSizeObj.ToString();
            // TODO: Compare room size once Room has size property
            // For now, skip this check
        }

        // Check max_room_size
        if (spawnRules.TryGetValue("max_room_size", out var maxSizeObj))
        {
            var maxSize = maxSizeObj.ToString();
            // TODO: Compare room size once Room has size property
        }

        // Check requires_tag
        if (spawnRules.TryGetValue("requires_tag", out var requiredTagObj))
        {
            var requiredTag = requiredTagObj.ToString();
            if (room.Tags != null && !room.Tags.Contains(requiredTag ?? ""))
            {
                _log.Debug("Room missing required tag {Tag} for {Composite}",
                    requiredTag, composite.FinalName);
                return false;
            }
        }

        // Check max_per_room
        if (spawnRules.TryGetValue("max_per_room", out var maxPerRoomObj))
        {
            // TODO: Track how many instances of this composite are already in the room
            // For now, skip this check
        }

        return true;
    }

    public DescriptorLibraryStats GetLibraryStats()
    {
        _log.Information("Getting descriptor library stats");
        return _repository.GetLibraryStats();
    }

    #endregion
}
