using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// A template for generating rooms. Contains placeholders, spawn rules, and biome restrictions.
/// </summary>
/// <remarks>
/// RoomTemplate supports two construction patterns:
/// 1. Legacy constructor using Biome enum for single-biome templates
/// 2. Extended constructor using string-based ValidBiomes for data-driven generation
/// </remarks>
public class RoomTemplate : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Core Properties
    // ═══════════════════════════════════════════════════════════════════════════

    public Guid Id { get; private set; }
    public string TemplateId { get; private set; }
    public string Name { get; private set; }
    public RoomArchetype Archetype { get; private set; }
    public Biome Biome { get; private set; }
    public int MinExits { get; private set; }
    public int MaxExits { get; private set; }
    public string BaseDescription { get; private set; }
    public int Weight { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Extended Properties (for data-driven generation)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the name pattern with placeholders for procedural generation.
    /// </summary>
    public string NamePattern { get; private set; }

    /// <summary>
    /// Gets the description pattern with placeholders for procedural generation.
    /// </summary>
    public string DescriptionPattern { get; private set; }

    /// <summary>
    /// Gets the list of valid biome IDs this template can be used in.
    /// </summary>
    public IReadOnlyList<string> ValidBiomes => _validBiomes.AsReadOnly();
    private readonly List<string> _validBiomes = [];

    /// <summary>
    /// Gets the minimum dungeon depth for this template.
    /// </summary>
    public int MinDepth { get; private set; }

    /// <summary>
    /// Gets the maximum dungeon depth for this template (null = no limit).
    /// </summary>
    public int? MaxDepth { get; private set; }

    /// <summary>
    /// Gets the room type for this template.
    /// </summary>
    public RoomType RoomType { get; private set; } = RoomType.Standard;

    /// <summary>
    /// Gets the template slots for content placement.
    /// </summary>
    public IReadOnlyList<TemplateSlot> Slots => _slots.AsReadOnly();
    private readonly List<TemplateSlot> _slots = [];

    // ═══════════════════════════════════════════════════════════════════════════
    // Tag and Feature Collections
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly List<string> _tags = [];
    private readonly List<RoomFeature> _features = [];

    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public IReadOnlyList<RoomFeature> Features => _features.AsReadOnly();

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private RoomTemplate()
    {
        TemplateId = null!;
        Name = null!;
        BaseDescription = null!;
        NamePattern = null!;
        DescriptionPattern = null!;
    }

    /// <summary>
    /// Creates a RoomTemplate with legacy Biome enum (single biome support).
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
        NamePattern = name;
        Archetype = archetype;
        Biome = biome;
        BaseDescription = baseDescription;
        DescriptionPattern = baseDescription;
        MinExits = minExits;
        MaxExits = maxExits;
        Weight = weight;
        MinDepth = 0;
        MaxDepth = null;

        // Add biome as string for compatibility
        _validBiomes.Add(biome.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Creates a RoomTemplate with extended data-driven configuration.
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
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID cannot be empty", nameof(templateId));
        if (string.IsNullOrWhiteSpace(namePattern))
            throw new ArgumentException("Name pattern cannot be empty", nameof(namePattern));
        if (string.IsNullOrWhiteSpace(descriptionPattern))
            throw new ArgumentException("Description pattern cannot be empty", nameof(descriptionPattern));
        if (weight < 1)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1");
        if (minDepth < 0)
            throw new ArgumentOutOfRangeException(nameof(minDepth), "Minimum depth cannot be negative");
        if (maxDepth.HasValue && maxDepth < minDepth)
            throw new ArgumentOutOfRangeException(nameof(maxDepth), "Maximum depth cannot be less than minimum depth");
        if (validBiomes == null || !validBiomes.Any())
            throw new ArgumentException("At least one valid biome must be specified", nameof(validBiomes));

        Id = Guid.NewGuid();
        TemplateId = templateId;
        Name = namePattern;
        NamePattern = namePattern;
        BaseDescription = descriptionPattern;
        DescriptionPattern = descriptionPattern;
        RoomType = roomType;
        Weight = weight;
        MinDepth = minDepth;
        MaxDepth = maxDepth;
        MinExits = 1;
        MaxExits = 4;
        Archetype = RoomArchetype.Chamber;
        Biome = Biome.Citadel;

        _validBiomes.AddRange(validBiomes);
        _slots.AddRange(slots);

        if (tags != null)
        {
            _tags.AddRange(tags);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Tag Methods
    // ═══════════════════════════════════════════════════════════════════════════

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
    /// Checks if this template has all specified tags.
    /// </summary>
    public bool HasAllTags(IEnumerable<string> requiredTags) =>
        requiredTags.All(tag => _tags.Contains(tag, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if this template has none of the specified tags.
    /// </summary>
    public bool HasNoTags(IEnumerable<string> excludedTags) =>
        !excludedTags.Any(tag => _tags.Contains(tag, StringComparer.OrdinalIgnoreCase));

    // ═══════════════════════════════════════════════════════════════════════════
    // Feature Methods
    // ═══════════════════════════════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════════════════════════════
    // Compatibility Check Methods
    // ═══════════════════════════════════════════════════════════════════════════

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
    /// Checks if this template is compatible with a specific biome (enum).
    /// </summary>
    public bool IsCompatibleWithBiome(Biome biome) => Biome == biome;

    /// <summary>
    /// Checks if this template is valid for the specified biome ID (string).
    /// </summary>
    public bool IsValidForBiome(string? biomeId)
    {
        if (string.IsNullOrEmpty(biomeId))
            return true; // No biome filter means all templates are valid

        return _validBiomes.Count == 0 ||
               _validBiomes.Contains(biomeId, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if this template is valid for the specified dungeon depth.
    /// </summary>
    public bool IsValidForDepth(int depth)
    {
        if (depth < MinDepth)
            return false;

        if (MaxDepth.HasValue && depth > MaxDepth.Value)
            return false;

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Slot Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all slots of the specified type.
    /// </summary>
    public IEnumerable<TemplateSlot> GetSlotsByType(SlotType type) =>
        _slots.Where(s => s.Type == type);

    /// <summary>
    /// Adds a slot to this template.
    /// </summary>
    public void AddSlot(TemplateSlot slot) => _slots.Add(slot);

    // ═══════════════════════════════════════════════════════════════════════════
    // Pattern Processing Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a room name from this template, with optional placeholder replacement.
    /// </summary>
    public string ProcessName(Dictionary<string, string>? placeholders = null)
    {
        var result = NamePattern;
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
        var result = DescriptionPattern;
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
