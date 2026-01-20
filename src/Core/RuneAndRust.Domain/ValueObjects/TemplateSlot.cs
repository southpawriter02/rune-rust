using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines a variable content position within a room template.
/// </summary>
/// <remarks>
/// Template slots specify points where content can be placed during
/// room instantiation. Each slot has a type, quantity constraints,
/// fill probability, and optional content-specific constraints.
/// </remarks>
public readonly record struct TemplateSlot
{
    /// <summary>
    /// Gets the slot identifier within the template.
    /// </summary>
    public string SlotId { get; init; }

    /// <summary>
    /// Gets the type of content this slot accepts.
    /// </summary>
    public SlotType Type { get; init; }

    /// <summary>
    /// Gets whether this slot must be filled.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets the minimum quantity to place.
    /// </summary>
    public int MinQuantity { get; init; }

    /// <summary>
    /// Gets the maximum quantity to place.
    /// </summary>
    public int MaxQuantity { get; init; }

    /// <summary>
    /// Gets the probability this slot is filled (0.0-1.0).
    /// </summary>
    public float FillProbability { get; init; }

    /// <summary>
    /// Gets constraints for content selection.
    /// </summary>
    public IReadOnlyDictionary<string, string> Constraints { get; init; }

    /// <summary>
    /// Gets the descriptor pool for description slots.
    /// </summary>
    public string? DescriptorPool { get; init; }

    /// <summary>
    /// Creates a monster spawn slot.
    /// </summary>
    public static TemplateSlot Monster(
        string slotId,
        bool required = false,
        int min = 1,
        int max = 1,
        float probability = 0.5f,
        IReadOnlyDictionary<string, string>? constraints = null) => new()
    {
        SlotId = slotId,
        Type = SlotType.Monster,
        IsRequired = required,
        MinQuantity = min,
        MaxQuantity = max,
        FillProbability = probability,
        Constraints = constraints ?? new Dictionary<string, string>()
    };

    /// <summary>
    /// Creates an item placement slot.
    /// </summary>
    public static TemplateSlot Item(
        string slotId,
        bool required = false,
        int min = 1,
        int max = 1,
        float probability = 0.3f,
        IReadOnlyDictionary<string, string>? constraints = null) => new()
    {
        SlotId = slotId,
        Type = SlotType.Item,
        IsRequired = required,
        MinQuantity = min,
        MaxQuantity = max,
        FillProbability = probability,
        Constraints = constraints ?? new Dictionary<string, string>()
    };

    /// <summary>
    /// Creates a description variable slot.
    /// </summary>
    public static TemplateSlot Description(
        string slotId,
        string descriptorPool,
        bool required = true) => new()
    {
        SlotId = slotId,
        Type = SlotType.Description,
        IsRequired = required,
        MinQuantity = 1,
        MaxQuantity = 1,
        FillProbability = 1.0f,
        DescriptorPool = descriptorPool,
        Constraints = new Dictionary<string, string>()
    };

    /// <summary>
    /// Creates a container slot.
    /// </summary>
    public static TemplateSlot Container(
        string slotId,
        bool required = false,
        float probability = 0.25f,
        IReadOnlyDictionary<string, string>? constraints = null) => new()
    {
        SlotId = slotId,
        Type = SlotType.Container,
        IsRequired = required,
        MinQuantity = 1,
        MaxQuantity = 1,
        FillProbability = probability,
        Constraints = constraints ?? new Dictionary<string, string>()
    };

    /// <summary>
    /// Creates a feature slot.
    /// </summary>
    public static TemplateSlot Feature(
        string slotId,
        bool required = false,
        float probability = 0.2f,
        IReadOnlyDictionary<string, string>? constraints = null) => new()
    {
        SlotId = slotId,
        Type = SlotType.Feature,
        IsRequired = required,
        MinQuantity = 1,
        MaxQuantity = 1,
        FillProbability = probability,
        Constraints = constraints ?? new Dictionary<string, string>()
    };

    /// <summary>
    /// Creates a hazard slot.
    /// </summary>
    public static TemplateSlot Hazard(
        string slotId,
        bool required = false,
        float probability = 0.15f,
        IReadOnlyDictionary<string, string>? constraints = null) => new()
    {
        SlotId = slotId,
        Type = SlotType.Hazard,
        IsRequired = required,
        MinQuantity = 1,
        MaxQuantity = 1,
        FillProbability = probability,
        Constraints = constraints ?? new Dictionary<string, string>()
    };

    /// <summary>
    /// Creates an exit slot.
    /// </summary>
    public static TemplateSlot Exit(
        string slotId,
        bool required = false,
        float probability = 0.5f,
        IReadOnlyDictionary<string, string>? constraints = null) => new()
    {
        SlotId = slotId,
        Type = SlotType.Exit,
        IsRequired = required,
        MinQuantity = 1,
        MaxQuantity = 1,
        FillProbability = probability,
        Constraints = constraints ?? new Dictionary<string, string>()
    };

    /// <summary>
    /// Gets a constraint value by key.
    /// </summary>
    public string? GetConstraint(string key) =>
        Constraints?.TryGetValue(key, out var value) == true ? value : null;

    /// <summary>
    /// Checks if a constraint matches a specified value.
    /// </summary>
    public bool HasConstraint(string key, string value) =>
        Constraints != null &&
        Constraints.TryGetValue(key, out var actual) &&
        actual.Equals(value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the effective fill probability (1.0 for required slots).
    /// </summary>
    public float EffectiveFillProbability => IsRequired ? 1.0f : FillProbability;
}
