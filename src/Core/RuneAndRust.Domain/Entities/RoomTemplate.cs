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

    // Missing properties required by TemplateValidationService
    public string NamePattern { get; set; } = string.Empty;
    public string DescriptionPattern { get; set; } = string.Empty;
    public List<string> ValidBiomes { get; set; } = new();
    public int MinDepth { get; set; }
    public int? MaxDepth { get; set; }
    public List<TemplateSlot> Slots { get; set; } = new();

    public IEnumerable<TemplateSlot> GetSlotsByType(SlotType type) => Slots.Where(s => s.Type == type);

    public bool IsValidForBiome(Biome biome)
    {
        return ValidBiomes.Contains(biome.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    public bool IsValidForBiome(string biome)
    {
        return ValidBiomes.Contains(biome, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsValidForDepth(int depth)
    {
        if (depth < MinDepth) return false;
        if (MaxDepth.HasValue && depth > MaxDepth.Value) return false;
        return true;
    }

    public bool HasAllTags(IEnumerable<string> tags)
    {
        return tags.All(t => _tags.Contains(t, StringComparer.OrdinalIgnoreCase));
    }

    public bool HasNoTags(IEnumerable<string> tags)
    {
        return !tags.Any(t => _tags.Contains(t, StringComparer.OrdinalIgnoreCase));
    }

    public RoomType RoomType { get; private set; } = RoomType.Standard;

    private readonly List<string> _tags = [];
    private readonly List<RoomFeature> _features = [];

    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public IReadOnlyList<RoomFeature> Features => _features.AsReadOnly();

    private RoomTemplate()
    {
        TemplateId = null!;
        Name = null!;
        BaseDescription = null!;
    } // For EF Core

    public RoomTemplate(
        string templateId,
        string name = "Unknown",
        RoomArchetype archetype = RoomArchetype.Chamber,
        Biome biome = Biome.Citadel,
        string baseDescription = "Description pending.",
        int minExits = 1,
        int maxExits = 4,
        int weight = 1,
        string namePattern = "",
        string descriptionPattern = "",
        List<string>? validBiomes = null,
        int minDepth = 0,
        int? maxDepth = null,
        RoomType roomType = RoomType.Standard,
        List<TemplateSlot>? slots = null,
        List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID cannot be empty", nameof(templateId));
        // Relaxed validation for optional parameters
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
        NamePattern = namePattern;
        DescriptionPattern = descriptionPattern;
        ValidBiomes = validBiomes ?? new List<string>();
        MinDepth = minDepth;
        MaxDepth = maxDepth;
        RoomType = roomType;
        Slots = slots ?? new List<TemplateSlot>();
        if (tags != null)
        {
            _tags.AddRange(tags);
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

    public override string ToString() => $"{TemplateId} ({Archetype}, {Biome})";
}
