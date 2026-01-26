namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a hidden element that was revealed by passive perception on room entry.
/// </summary>
public sealed record RevealedElement
{
    /// <summary>
    /// The hidden element ID that was revealed.
    /// </summary>
    public required string ElementId { get; init; }

    /// <summary>
    /// The type of the hidden element (e.g., "Trap", "Secret", "Clue").
    /// </summary>
    public required string ElementType { get; init; }

    /// <summary>
    /// The DC that was beaten to reveal this element.
    /// </summary>
    public required int DiscoveryDc { get; init; }

    /// <summary>
    /// The perception value that revealed this element.
    /// </summary>
    public required int PerceptionUsed { get; init; }

    /// <summary>
    /// How much the perception exceeded the DC.
    /// </summary>
    public int ExcessPerception => PerceptionUsed - DiscoveryDc;

    /// <summary>
    /// The description text to display for this revelation.
    /// </summary>
    public required string RevealedDescription { get; init; }

    /// <summary>
    /// Creates a revealed element for testing.
    /// </summary>
    public static RevealedElement Create(
        string elementId,
        string elementType,
        int discoveryDc,
        int perceptionUsed,
        string revealedDescription) =>
        new()
        {
            ElementId = elementId,
            ElementType = elementType,
            DiscoveryDc = discoveryDc,
            PerceptionUsed = perceptionUsed,
            RevealedDescription = revealedDescription
        };
}
