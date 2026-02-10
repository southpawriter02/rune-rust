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

    // New properties for generator service compatibility
    public string NamePattern { get; private set; }
    public string DescriptionPattern { get; private set; }
    public IReadOnlyList<string> ValidBiomes { get; private set; }
    public int? MinDepth { get; private set; }
    public int? MaxDepth { get; private set; }
    public RoomType RoomType { get; private set; }

    private readonly List<string> _tags = [];
    private readonly List<RoomFeature> _features = [];
    private readonly List<TemplateSlot> _slots = [];

    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public IReadOnlyList<RoomFeature> Features => _features.AsReadOnly();
    public IReadOnlyList<TemplateSlot> Slots => _slots.AsReadOnly();

    private RoomTemplate()
    {
        TemplateId = null!;
        Name = null!;
        BaseDescription = null!;
        NamePattern = null!;
        DescriptionPattern = null!;
        ValidBiomes = [];
    } // For EF Core

    /// <summary>
    /// Legacy constructor.
    /// </summary>
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

        // Initialize new properties for compatibility
        NamePattern = name;
        DescriptionPattern = baseDescription;
        ValidBiomes = [biome.ToString()];
        MinDepth = 0;
        MaxDepth = 100;
        RoomType = RoomType.Standard;
    }

    /// <summary>
    /// Extended constructor for generator service.
    /// </summary>
    public RoomTemplate(
        string templateId,
        string namePattern,
        IEnumerable<string> validBiomes,
        string descriptionPattern,
        RoomType roomType,
        IEnumerable<TemplateSlot>? slots = null,
        int weight = 1,
        int? minDepth = null,
        int? maxDepth = null,
        IEnumerable<string>? tags = null,
        RoomArchetype archetype = RoomArchetype.Chamber,
        int minExits = 1,
        int maxExits = 4)
    {
        Id = Guid.NewGuid();
        TemplateId = templateId;
        NamePattern = namePattern;
        DescriptionPattern = descriptionPattern;
        RoomType = roomType;
        Archetype = archetype;
        ValidBiomes = validBiomes.ToList().AsReadOnly();
        MinDepth = minDepth;
        MaxDepth = maxDepth;
        MinExits = minExits;
        MaxExits = maxExits;
        Weight = weight;

        if (slots != null)
            _slots.AddRange(slots);

        if (tags != null)
            _tags.AddRange(tags);

        // Map to legacy properties
        Name = namePattern;
        BaseDescription = descriptionPattern;

        // Try to map primary biome to enum
        var primaryBiome = ValidBiomes.FirstOrDefault();
        if (primaryBiome != null && Enum.TryParse<Biome>(primaryBiome, true, out var b))
        {
            Biome = b;
        }
        else
        {
            Biome = Biome.Citadel; // Default fallback
        }
    }

    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !_tags.Contains(tag))
            _tags.Add(tag);
    }

    public void AddTags(IEnumerable<string> tags)
    {
        foreach (var tag in tags)
            AddTag(tag);
    }

    public void AddFeature(RoomFeature feature)
    {
        if (feature == null)
            throw new ArgumentNullException(nameof(feature));
        _features.Add(feature);
    }

    public void AddFeatures(IEnumerable<RoomFeature> features)
    {
        foreach (var feature in features)
            AddFeature(feature);
    }

    public bool IsCompatibleWithExitCount(int exitCount) =>
        exitCount >= MinExits && exitCount <= MaxExits;

    public bool IsCompatibleWithArchetype(RoomArchetype archetype) =>
        Archetype == archetype;

    public bool IsCompatibleWithBiome(Biome biome) => IsValidForBiome(biome);

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

    // New methods
    public IReadOnlyList<TemplateSlot> GetSlotsByType(SlotType type) =>
        _slots.Where(s => s.Type == type).ToList().AsReadOnly();

    public bool IsValidForBiome(string biomeId) =>
        ValidBiomes.Contains(biomeId, StringComparer.OrdinalIgnoreCase);

    public bool IsValidForBiome(Biome biome) =>
        IsValidForBiome(biome.ToString());

    public bool IsValidForDepth(int depth)
    {
        if (MinDepth.HasValue && depth < MinDepth.Value) return false;
        if (MaxDepth.HasValue && depth > MaxDepth.Value) return false;
        return true;
    }

    public bool HasAllTags(IEnumerable<string> tags)
    {
        if (tags == null) return true;
        return tags.All(t => _tags.Any(rt => rt.Equals(t, StringComparison.OrdinalIgnoreCase)));
    }

    public bool HasNoTags(IEnumerable<string> tags)
    {
        if (tags == null) return true;
        return !tags.Any(t => _tags.Any(rt => rt.Equals(t, StringComparison.OrdinalIgnoreCase)));
    }

    public override string ToString() => $"{TemplateId} ({Archetype}, {Biome})";
}
