namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a piece of evidence discovered during investigation.
/// Clues can be combined to form deductions and reveal hidden truths.
/// </summary>
/// <remarks>
/// <para>
/// Each clue represents a discrete piece of information that, while
/// potentially meaningful on its own, becomes more significant when
/// combined with other related clues.
/// </para>
/// </remarks>
public sealed record Clue
{
    /// <summary>
    /// Unique identifier for this clue.
    /// </summary>
    public required string ClueId { get; init; }

    /// <summary>
    /// The investigation target type this clue was found on.
    /// </summary>
    public required InvestigationTarget SourceType { get; init; }

    /// <summary>
    /// The identifier of the specific target where this clue was found.
    /// </summary>
    public required string SourceTargetId { get; init; }

    /// <summary>
    /// Category of clue for deduction matching.
    /// </summary>
    public required ClueCategory Category { get; init; }

    /// <summary>
    /// Difficulty class required to discover this clue.
    /// </summary>
    public required int DiscoveryDc { get; init; }

    /// <summary>
    /// Short name for the clue (for tracking/display).
    /// </summary>
    public required string ClueName { get; init; }

    /// <summary>
    /// Detailed description revealed when the clue is discovered.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Narrative text shown when the clue is discovered.
    /// </summary>
    public required string DiscoveryText { get; init; }

    /// <summary>
    /// Optional tags for grouping related clues.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Weight for determining how significant this clue is.
    /// Higher weight clues contribute more to deduction confidence.
    /// </summary>
    public int Weight { get; init; } = 1;

    /// <summary>
    /// Gets whether this clue has any tags.
    /// </summary>
    public bool HasTags => Tags.Count > 0;

    /// <summary>
    /// Creates a simple clue for testing.
    /// </summary>
    public static Clue Create(
        string clueId,
        InvestigationTarget sourceType,
        string sourceTargetId,
        ClueCategory category,
        int discoveryDc,
        string clueName,
        string description,
        string discoveryText) =>
        new()
        {
            ClueId = clueId,
            SourceType = sourceType,
            SourceTargetId = sourceTargetId,
            Category = category,
            DiscoveryDc = discoveryDc,
            ClueName = clueName,
            Description = description,
            DiscoveryText = discoveryText
        };
}
