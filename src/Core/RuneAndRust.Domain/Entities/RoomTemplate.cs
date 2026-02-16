using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// A template for generating rooms. Contains placeholders and spawn rules.
/// </summary>
public class RoomTemplate : IEntity
{
    public Guid Id { get; private set; }
    public string TemplateId { get; private set; }
    public string Name { get; private set; }
    public RoomArchetype Archetype { get; private set; }
    public Biome Biome { get; private set; }
    public int MinExits { get; private set; }
    public int MaxExits { get; private set; }
    public string BaseDescription { get; private set; }
    public int Weight { get; private set; }

    /// <summary>
    /// Pattern for generating room names (may contain placeholders).
    /// Defaults to <see cref="Name"/> when not explicitly set.
    /// </summary>
    public string NamePattern { get; private set; }

    /// <summary>
    /// Pattern for generating room descriptions (may contain placeholders).
    /// Defaults to <see cref="BaseDescription"/> when not explicitly set.
    /// </summary>
    public string DescriptionPattern { get; private set; }

    /// <summary>
    /// List of biome string identifiers this template is valid for.
    /// </summary>
    public IReadOnlyList<string> ValidBiomes { get; private set; } = [];

    /// <summary>
    /// Content slots defined for this template.
    /// </summary>
    public IReadOnlyList<TemplateSlot> Slots { get; private set; } = [];

    /// <summary>
    /// Minimum dungeon depth where this template can appear.
    /// </summary>
    public int MinDepth { get; private set; }

    /// <summary>
    /// Maximum dungeon depth where this template can appear, or null for no limit.
    /// </summary>
    public int? MaxDepth { get; private set; }

    /// <summary>
    /// The room type category for this template.
    /// </summary>
    public RoomType RoomType { get; private set; }

    private readonly List<string> _tags = [];
    private readonly List<RoomFeature> _features = [];

    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public IReadOnlyList<RoomFeature> Features => _features.AsReadOnly();

    private RoomTemplate()
    {
        TemplateId = null!;
        Name = null!;
        BaseDescription = null!;
        NamePattern = null!;
        DescriptionPattern = null!;
    } // For EF Core

    public RoomTemplate(
        string templateId,
        string name,
        RoomArchetype archetype,
        Biome biome,
        string baseDescription,
        int minExits = 1,
        int maxExits = 4,
        int weight = 1)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID cannot be empty", nameof(templateId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(baseDescription))
            throw new ArgumentException("Description cannot be empty", nameof(baseDescription));
        if (minExits < 1)
            throw new ArgumentOutOfRangeException(nameof(minExits), "Minimum exits must be at least 1");
        if (maxExits < minExits)
            throw new ArgumentOutOfRangeException(nameof(maxExits), "Maximum exits must be >= minimum exits");
        if (weight < 1)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1");

        Id = Guid.NewGuid();
        TemplateId = templateId;
        Name = name;
        Archetype = archetype;
        Biome = biome;
        BaseDescription = baseDescription;
        MinExits = minExits;
        MaxExits = maxExits;
        Weight = weight;
        NamePattern = name;
        DescriptionPattern = baseDescription;
    }

    /// <summary>
    /// Creates a room template from pattern-based configuration with biome string identifiers and slots.
    /// </summary>
    public RoomTemplate(
        string templateId,
        string namePattern,
        string descriptionPattern,
        IReadOnlyList<string> validBiomes,
        RoomType roomType,
        IReadOnlyList<TemplateSlot> slots,
        int weight = 1,
        int minDepth = 0,
        int? maxDepth = null,
        IReadOnlyList<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID cannot be empty", nameof(templateId));

        Id = Guid.NewGuid();
        TemplateId = templateId;
        Name = namePattern;
        NamePattern = namePattern;
        BaseDescription = descriptionPattern;
        DescriptionPattern = descriptionPattern;
        ValidBiomes = validBiomes ?? [];
        RoomType = roomType;
        Slots = slots ?? [];
        Weight = weight < 1 ? 1 : weight;
        MinDepth = minDepth;
        MaxDepth = maxDepth;
        MinExits = 1;
        MaxExits = 4;

        if (tags != null)
        {
            foreach (var tag in tags)
                AddTag(tag);
        }
    }

    /// <summary>
    /// Adds a tag to this template. Tags are inherited by instantiated rooms.
    /// </summary>
    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !_tags.Contains(tag))
            _tags.Add(tag);
    }

    /// <summary>
    /// Adds multiple tags to this template.
    /// </summary>
    public void AddTags(IEnumerable<string> tags)
    {
        foreach (var tag in tags)
            AddTag(tag);
    }

    /// <summary>
    /// Adds a feature spawn rule to this template.
    /// </summary>
    public void AddFeature(RoomFeature feature)
    {
        if (feature == null)
            throw new ArgumentNullException(nameof(feature));
        _features.Add(feature);
    }

    /// <summary>
    /// Adds multiple features to this template.
    /// </summary>
    public void AddFeatures(IEnumerable<RoomFeature> features)
    {
        foreach (var feature in features)
            AddFeature(feature);
    }

    /// <summary>
    /// Checks if this template is compatible with a node based on exit count.
    /// </summary>
    public bool IsCompatibleWithExitCount(int exitCount) =>
        exitCount >= MinExits && exitCount <= MaxExits;

    /// <summary>
    /// Checks if this template is compatible with a node's archetype.
    /// </summary>
    public bool IsCompatibleWithArchetype(RoomArchetype archetype) =>
        Archetype == archetype;

    /// <summary>
    /// Checks if this template is compatible with a specific biome.
    /// </summary>
    public bool IsCompatibleWithBiome(Biome biome) => Biome == biome;

    /// <summary>
    /// Creates a room name from this template, with optional placeholder replacement.
    /// </summary>
    public string ProcessName(Dictionary<string, string>? placeholders = null)
    {
        var result = Name;
        if (placeholders != null)
        {
            foreach (var (key, value) in placeholders)
            {
                result = result.Replace($"{{{key}}}", value, StringComparison.OrdinalIgnoreCase);
            }
        }
        return result;
    }

    /// <summary>
    /// Creates a room description from this template, with placeholder replacement.
    /// Supported placeholders: {ADJ_SIZE}, {ADJ_CONDITION}, {ADJ_ATMOSPHERE}
    /// </summary>
    public string ProcessDescription(Dictionary<string, string>? placeholders = null)
    {
        var result = BaseDescription;
        if (placeholders != null)
        {
            foreach (var (key, value) in placeholders)
            {
                result = result.Replace($"{{{key}}}", value, StringComparison.OrdinalIgnoreCase);
            }
        }
        return result;
    }

    /// <summary>
    /// Checks if this template is valid for a biome specified by string identifier.
    /// </summary>
    public bool IsValidForBiome(string? biomeId)
    {
        if (string.IsNullOrEmpty(biomeId)) return true;
        if (ValidBiomes.Count == 0) return true;
        return ValidBiomes.Any(b => b.Equals(biomeId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if this template is valid for the specified depth.
    /// </summary>
    public bool IsValidForDepth(int depth)
    {
        if (depth < MinDepth) return false;
        if (MaxDepth.HasValue && depth > MaxDepth.Value) return false;
        return true;
    }

    /// <summary>
    /// Checks if this template has all the specified tags.
    /// </summary>
    public bool HasAllTags(IReadOnlyList<string> requiredTags)
    {
        if (requiredTags.Count == 0) return true;
        return requiredTags.All(t => _tags.Contains(t, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if this template has none of the specified tags.
    /// </summary>
    public bool HasNoTags(IReadOnlyList<string> excludedTags)
    {
        if (excludedTags.Count == 0) return true;
        return !excludedTags.Any(t => _tags.Contains(t, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all slots of the specified type.
    /// </summary>
    public IEnumerable<TemplateSlot> GetSlotsByType(SlotType type)
    {
        return Slots.Where(s => s.Type == type);
    }

    public override string ToString() => $"{TemplateId} ({Archetype}, {Biome})";
}
