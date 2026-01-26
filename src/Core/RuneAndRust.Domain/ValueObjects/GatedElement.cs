namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single perception-gated description element within a room.
/// The element is revealed only if the viewer's passive perception meets
/// or exceeds the required DC.
/// </summary>
/// <remarks>
/// <para>
/// Gated elements allow rooms to have layered descriptions where more
/// perceptive characters notice additional details. Elements can represent
/// environmental details, hidden features, or atmospheric observations.
/// </para>
/// </remarks>
public sealed record GatedElement
{
    /// <summary>
    /// Unique identifier for this gated element.
    /// </summary>
    public required string ElementId { get; init; }

    /// <summary>
    /// The minimum passive perception required to reveal this element.
    /// </summary>
    public required int RequiredDc { get; init; }

    /// <summary>
    /// The description fragment shown when this element is revealed.
    /// Should be written to flow naturally within a larger description.
    /// </summary>
    public required string DescriptionFragment { get; init; }

    /// <summary>
    /// Whether this element has been revealed based on the current
    /// viewer's perception. Updated during description evaluation.
    /// </summary>
    public bool IsRevealed { get; init; }

    /// <summary>
    /// Optional category for organizing elements (e.g., "environmental",
    /// "hidden", "atmospheric", "flora", "fauna").
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Optional priority for ordering elements within the description.
    /// Lower values appear earlier in the composed description.
    /// </summary>
    public int Priority { get; init; } = 100;

    /// <summary>
    /// Optional reference to a hidden element that this description reveals.
    /// When set, revealing this element also marks the hidden element as known.
    /// </summary>
    public string? LinkedHiddenElementId { get; init; }

    /// <summary>
    /// Optional specialization that receives bonus perception for this element.
    /// </summary>
    public string? BonusSpecialization { get; init; }

    /// <summary>
    /// Bonus to the effective DC reduction for characters with the bonus specialization.
    /// </summary>
    public int SpecializationBonus { get; init; }

    /// <summary>
    /// Creates a gated element for testing.
    /// </summary>
    public static GatedElement Create(
        string elementId,
        int requiredDc,
        string descriptionFragment,
        string? category = null,
        int priority = 100) =>
        new()
        {
            ElementId = elementId,
            RequiredDc = requiredDc,
            DescriptionFragment = descriptionFragment,
            Category = category,
            Priority = priority
        };
}
