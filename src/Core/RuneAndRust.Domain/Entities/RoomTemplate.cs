using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Defines a reusable room pattern with variable content slots.
/// </summary>
/// <remarks>
/// Room templates specify the structure and possible content of generated rooms.
/// They include name/description patterns with placeholder variables that are
/// filled during room instantiation, along with typed slots for placing
/// monsters, items, features, and other content.
/// </remarks>
public class RoomTemplate : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this template instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the template's configuration ID (e.g., "dungeon-corridor-01").
    /// </summary>
    public string TemplateId { get; private set; }

    /// <summary>
    /// Gets the display name pattern (may contain {variables}).
    /// </summary>
    public string NamePattern { get; private set; }

    /// <summary>
    /// Gets the description pattern with descriptor pool references.
    /// </summary>
    public string DescriptionPattern { get; private set; }

    /// <summary>
    /// Gets the biomes this template can be used in.
    /// </summary>
    public IReadOnlyList<string> ValidBiomes { get; private set; }

    /// <summary>
    /// Gets the room type this template produces.
    /// </summary>
    public RoomType RoomType { get; private set; }

    /// <summary>
    /// Gets the content slots defined in this template.
    /// </summary>
    public IReadOnlyList<TemplateSlot> Slots { get; private set; }

    /// <summary>
    /// Gets the selection weight for random template selection.
    /// </summary>
    public int Weight { get; private set; }

    /// <summary>
    /// Gets the minimum depth (Z-level) where this template can appear.
    /// </summary>
    public int MinDepth { get; private set; }

    /// <summary>
    /// Gets the maximum depth (Z-level) where this template can appear.
    /// </summary>
    public int? MaxDepth { get; private set; }

    /// <summary>
    /// Gets tags for additional filtering during selection.
    /// </summary>
    public IReadOnlyList<string> Tags { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private RoomTemplate()
    {
        TemplateId = string.Empty;
        NamePattern = string.Empty;
        DescriptionPattern = string.Empty;
        ValidBiomes = [];
        Slots = [];
        Tags = [];
    }

    /// <summary>
    /// Creates a new room template with the specified properties.
    /// </summary>
    public RoomTemplate(
        string templateId,
        string namePattern,
        string descriptionPattern,
        IReadOnlyList<string> validBiomes,
        RoomType roomType,
        IReadOnlyList<TemplateSlot> slots,
        int weight = 10,
        int minDepth = 0,
        int? maxDepth = null,
        IReadOnlyList<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID cannot be empty.", nameof(templateId));

        if (string.IsNullOrWhiteSpace(namePattern))
            throw new ArgumentException("Name pattern cannot be empty.", nameof(namePattern));

        if (validBiomes == null || validBiomes.Count == 0)
            throw new ArgumentException("At least one valid biome is required.", nameof(validBiomes));

        if (weight <= 0)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");

        if (minDepth < 0)
            throw new ArgumentOutOfRangeException(nameof(minDepth), "Minimum depth cannot be negative.");

        if (maxDepth.HasValue && maxDepth < minDepth)
            throw new ArgumentException("Maximum depth cannot be less than minimum depth.", nameof(maxDepth));

        Id = Guid.NewGuid();
        TemplateId = templateId;
        NamePattern = namePattern;
        DescriptionPattern = descriptionPattern;
        ValidBiomes = validBiomes;
        RoomType = roomType;
        Slots = slots ?? [];
        Weight = weight;
        MinDepth = minDepth;
        MaxDepth = maxDepth;
        Tags = tags ?? [];
    }

    /// <summary>
    /// Checks if this template is valid for the specified biome.
    /// </summary>
    public bool IsValidForBiome(string biome) =>
        ValidBiomes.Contains(biome, StringComparer.OrdinalIgnoreCase);

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
    public bool HasAllTags(IEnumerable<string> requiredTags) =>
        requiredTags.All(tag => Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if this template has none of the specified tags.
    /// </summary>
    public bool HasNoTags(IEnumerable<string> excludedTags) =>
        !excludedTags.Any(tag => Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Gets all required slots in this template.
    /// </summary>
    public IEnumerable<TemplateSlot> GetRequiredSlots() =>
        Slots.Where(s => s.IsRequired);

    /// <summary>
    /// Gets all optional slots in this template.
    /// </summary>
    public IEnumerable<TemplateSlot> GetOptionalSlots() =>
        Slots.Where(s => !s.IsRequired);

    /// <summary>
    /// Gets slots of a specific type.
    /// </summary>
    public IEnumerable<TemplateSlot> GetSlotsByType(SlotType type) =>
        Slots.Where(s => s.Type == type);

    /// <summary>
    /// Gets a specific slot by its ID.
    /// </summary>
    public TemplateSlot? GetSlot(string slotId) =>
        Slots.FirstOrDefault(s => s.SlotId.Equals(slotId, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc/>
    public override string ToString() =>
        $"RoomTemplate[{TemplateId}]: {RoomType} ({ValidBiomes.Count} biomes, {Slots.Count} slots)";
}
