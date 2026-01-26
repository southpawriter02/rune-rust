namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the conditions that must be met for a perception ability to activate.
/// </summary>
public sealed record AbilityCondition
{
    /// <summary>
    /// The type of condition check.
    /// </summary>
    public required ConditionType Type { get; init; }

    /// <summary>
    /// Target object categories that trigger this ability (for Examination).
    /// Example: ["Machinery", "Terminal"] for Jötun-Reader's Deep Scan.
    /// </summary>
    public IReadOnlyList<string> ObjectCategories { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Target object types that trigger this ability (more specific).
    /// Example: ["JotunTech", "AncientTerminal"] for Pattern Recognition.
    /// </summary>
    public IReadOnlyList<string> ObjectTypes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Hidden element types that trigger this ability (for Passive Perception).
    /// Example: ["Trap"] for Ruin-Stalker's Trap Sense.
    /// </summary>
    public IReadOnlyList<string> ElementTypes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Investigation target types that trigger this ability.
    /// Example: ["Remains"] for Veiðimaðr's Read the Signs.
    /// </summary>
    public IReadOnlyList<string> InvestigationTargets { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Distance in feet within which the ability applies (for proximity-based).
    /// Example: 10 for Ruin-Stalker's Sixth Sense.
    /// </summary>
    public int? ProximityFeet { get; init; }

    /// <summary>
    /// Whether this condition is always met (for unconditional bonuses).
    /// Example: true for Veiðimaðr's Keen Senses.
    /// </summary>
    public bool AlwaysActive { get; init; }

    /// <summary>
    /// Tags that must be present on the target for activation.
    /// Example: ["historical", "ancient"] for Thul's Lore Keeper.
    /// </summary>
    public IReadOnlyList<string> RequiredTags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Creates a condition that is always active.
    /// </summary>
    public static AbilityCondition Always() => new()
    {
        Type = ConditionType.Always,
        AlwaysActive = true
    };

    /// <summary>
    /// Creates a condition based on object categories.
    /// </summary>
    public static AbilityCondition ForCategories(params string[] categories) => new()
    {
        Type = ConditionType.ObjectCategory,
        ObjectCategories = categories
    };

    /// <summary>
    /// Creates a condition based on element types.
    /// </summary>
    public static AbilityCondition ForElementTypes(params string[] elementTypes) => new()
    {
        Type = ConditionType.ElementType,
        ElementTypes = elementTypes
    };
}
