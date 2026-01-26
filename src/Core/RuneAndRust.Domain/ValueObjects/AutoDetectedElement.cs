namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a hidden element that was automatically detected by a
/// specialization ability (e.g., Ruin-Stalker's Sixth Sense).
/// </summary>
public sealed record AutoDetectedElement
{
    /// <summary>
    /// The hidden element ID that was auto-detected.
    /// </summary>
    public required string ElementId { get; init; }

    /// <summary>
    /// The type of the hidden element (e.g., "Trap").
    /// </summary>
    public required string ElementType { get; init; }

    /// <summary>
    /// The ability ID that triggered the auto-detection.
    /// </summary>
    public required string DetectingAbilityId { get; init; }

    /// <summary>
    /// The name of the ability that triggered detection.
    /// </summary>
    public required string DetectingAbilityName { get; init; }

    /// <summary>
    /// Distance in feet from the character to the element.
    /// </summary>
    public required int DistanceFeet { get; init; }

    /// <summary>
    /// The description text to display for this auto-detection.
    /// </summary>
    public required string AutoDetectDescription { get; init; }

    /// <summary>
    /// Creates an auto-detected element for testing.
    /// </summary>
    public static AutoDetectedElement Create(
        string elementId,
        string elementType,
        string abilityId,
        string abilityName,
        int distanceFeet,
        string description) =>
        new()
        {
            ElementId = elementId,
            ElementType = elementType,
            DetectingAbilityId = abilityId,
            DetectingAbilityName = abilityName,
            DistanceFeet = distanceFeet,
            AutoDetectDescription = description
        };
}
