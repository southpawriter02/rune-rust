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

    private readonly List<string> _tags = [];
    private readonly List<RoomFeature> _features = [];
    private readonly List<string> _validBiomes = [];
    private readonly List<TemplateSlot> _slots = [];

    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public IReadOnlyList<RoomFeature> Features => _features.AsReadOnly();

    public IReadOnlyList<string> ValidBiomes => _validBiomes.AsReadOnly();
    public int MinDepth { get; private set; }
    public int? MaxDepth { get; private set; }
    public IReadOnlyList<TemplateSlot> Slots => _slots.AsReadOnly();
    public string NamePattern { get; private set; } = "";
    public string DescriptionPattern { get; private set; } = "";
    public RoomType RoomType { get; private set; } = RoomType.Standard;

    public bool IsValidForBiome(Biome biome) => _validBiomes.Contains(biome.ToString()) || _validBiomes.Count == 0;
    public bool IsValidForBiome(string biomeName) => _validBiomes.Contains(biomeName, StringComparer.OrdinalIgnoreCase) || _validBiomes.Count == 0;
    public bool IsValidForDepth(int depth) => depth >= MinDepth && (!MaxDepth.HasValue || depth <= MaxDepth.Value);
    public bool HasAllTags(IEnumerable<string> tags) => !tags.Except(_tags, StringComparer.OrdinalIgnoreCase).Any();
    public bool HasNoTags(IEnumerable<string> tags) => !tags.Intersect(_tags, StringComparer.OrdinalIgnoreCase).Any();
    public IEnumerable<TemplateSlot> GetSlotsByType(SlotType type) => _slots.Where(s => s.Type == type);

    private RoomTemplate()
    {
        TemplateId = null!;
        Name = null!;
        BaseDescription = null!;
    } // For EF Core

    public RoomTemplate(
        string templateId,
        string name,
        RoomArchetype archetype,
        Biome biome,
        string baseDescription,
        int minExits = 1,
        int maxExits = 4,
        int weight = 1,
        string namePattern = "",
        string descriptionPattern = "",
        IEnumerable<TemplateSlot>? slots = null,
        IEnumerable<string>? validBiomes = null,
        int minDepth = 0,
        int? maxDepth = null,
        RoomType roomType = RoomType.Standard,
        IEnumerable<string>? tags = null)
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
        NamePattern = namePattern;
        DescriptionPattern = descriptionPattern;
        MinDepth = minDepth;
        MaxDepth = maxDepth;
        RoomType = roomType;

        if (slots != null) _slots.AddRange(slots);
        if (validBiomes != null) _validBiomes.AddRange(validBiomes);
        if (tags != null) _tags.AddRange(tags);
    }

    /// <summary>
    /// Constructor for procedural generation usage (name/description from patterns).
    /// </summary>
    public RoomTemplate(
        string templateId,
        string namePattern,
        string descriptionPattern,
        IEnumerable<string> validBiomes,
        RoomType roomType,
        IEnumerable<TemplateSlot> slots,
        int weight = 1,
        int minDepth = 0,
        int? maxDepth = null,
        IEnumerable<string>? tags = null)
    {
        Id = Guid.NewGuid();
        TemplateId = templateId;
        NamePattern = namePattern;
        DescriptionPattern = descriptionPattern;
        RoomType = roomType;
        Weight = weight;
        MinDepth = minDepth;
        MaxDepth = maxDepth;

        if (validBiomes != null) _validBiomes.AddRange(validBiomes);
        if (slots != null) _slots.AddRange(slots);
        if (tags != null) _tags.AddRange(tags);

        // Defaults
        Name = "Generated Room";
        BaseDescription = "Generated Description";
        Archetype = RoomArchetype.Chamber;
        Biome = Biome.Citadel;
        MinExits = 1;
        MaxExits = 4;
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
