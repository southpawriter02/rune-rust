// ═══════════════════════════════════════════════════════════════════════════════
// TechnicalMemoryRecord.cs
// Immutable value object representing a puzzle solution recorded by the
// Jötun-Reader's Technical Memory ability. Supports pattern matching against
// new puzzles to provide exact solutions or DC reductions.
// Version: 0.20.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a puzzle solution stored in the Jötun-Reader's memory.
/// </summary>
/// <remarks>
/// <para>
/// Technical Memory (Tier 2) allows the Jötun-Reader to recall previously solved
/// puzzles. When encountering a new puzzle, the character can spend 1 AP and 2
/// Lore Insight to check if a matching puzzle has been recorded.
/// </para>
/// <para>
/// Matching uses keyword-based similarity: if two puzzles share the same category
/// and ≥50% keyword overlap, the earlier solution provides a DC reduction of 5.
/// An exact category + description match provides the full solution (DC bypass).
/// </para>
/// <para>
/// <b>Cost:</b> 1 AP, 2 Lore Insight.
/// <b>Tier:</b> 2 (requires 8 PP invested in Jötun-Reader tree).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var record = TechnicalMemoryRecord.Create(
///     "Mechanical", "Gear alignment puzzle with three rotors",
///     "Align gears: top-right, bottom-left, center-top",
///     locationId, "Bright Halls", 14);
///
/// var matches = record.MatchesPuzzle("Mechanical", "Gear alignment with four rotors");
/// // matches = true (same category, >50% keyword overlap)
/// </code>
/// </example>
/// <seealso cref="RuneAndRust.Domain.Enums.JotunReaderAbilityId"/>
public sealed record TechnicalMemoryRecord
{
    /// <summary>
    /// Similarity threshold for keyword matching (50%).
    /// </summary>
    public const decimal SimilarityThreshold = 0.5m;

    /// <summary>
    /// Minimum word length for keyword extraction (4 characters).
    /// </summary>
    public const int MinKeywordLength = 4;

    /// <summary>
    /// DC reduction applied when a similar (non-exact) pattern is found.
    /// </summary>
    public const int SimilarPatternDCReduction = 5;

    /// <summary>Gets the unique identifier for this record.</summary>
    public Guid RecordId { get; init; }

    /// <summary>Gets the category of the puzzle (e.g., "Mechanical", "Electrical").</summary>
    public string PuzzleCategory { get; init; } = string.Empty;

    /// <summary>Gets the description of the puzzle mechanism.</summary>
    public string PuzzleDescription { get; init; } = string.Empty;

    /// <summary>Gets the solution method or key.</summary>
    public string Solution { get; init; } = string.Empty;

    /// <summary>Gets the timestamp when the puzzle was solved.</summary>
    public DateTime SolvedAt { get; init; }

    /// <summary>Gets the ID of the location where the puzzle was solved.</summary>
    public Guid LocationId { get; init; }

    /// <summary>Gets the display name of the location where the puzzle was solved.</summary>
    public string LocationName { get; init; } = string.Empty;

    /// <summary>Gets the original DC of the puzzle when it was solved.</summary>
    public int OriginalDC { get; init; }

    /// <summary>
    /// Creates a new puzzle memory record.
    /// </summary>
    /// <param name="category">Category of the puzzle (e.g., "Mechanical").</param>
    /// <param name="description">Description of the puzzle mechanism.</param>
    /// <param name="solution">The solution method or key.</param>
    /// <param name="locationId">ID of the location where the puzzle was solved.</param>
    /// <param name="locationName">Display name of the location.</param>
    /// <param name="originalDC">The original DC of the puzzle.</param>
    /// <returns>A new <see cref="TechnicalMemoryRecord"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="category"/>, <paramref name="description"/>,
    /// or <paramref name="solution"/> is null or whitespace.
    /// </exception>
    public static TechnicalMemoryRecord Create(
        string category,
        string description,
        string solution,
        Guid locationId,
        string locationName,
        int originalDC)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(solution);
        ArgumentException.ThrowIfNullOrWhiteSpace(locationName);
        ArgumentOutOfRangeException.ThrowIfNegative(originalDC);

        return new TechnicalMemoryRecord
        {
            RecordId = Guid.NewGuid(),
            PuzzleCategory = category,
            PuzzleDescription = description,
            Solution = solution,
            SolvedAt = DateTime.UtcNow,
            LocationId = locationId,
            LocationName = locationName,
            OriginalDC = originalDC
        };
    }

    /// <summary>
    /// Determines if another puzzle matches this one's pattern.
    /// </summary>
    /// <remarks>
    /// A match requires the same category and ≥50% keyword overlap
    /// between the two puzzle descriptions.
    /// </remarks>
    /// <param name="otherCategory">Category of the puzzle to compare.</param>
    /// <param name="otherDescription">Description of the puzzle to compare.</param>
    /// <returns><c>true</c> if the puzzles share a similar pattern.</returns>
    public bool MatchesPuzzle(string otherCategory, string otherDescription)
    {
        if (!PuzzleCategory.Equals(otherCategory, StringComparison.OrdinalIgnoreCase))
            return false;

        return ArePatternsSimular(PuzzleDescription, otherDescription);
    }

    /// <summary>
    /// Gets a contextual hint about the previously solved puzzle.
    /// </summary>
    /// <returns>A formatted hint referencing the original location and category.</returns>
    public string GetSolutionHint() =>
        $"Earlier puzzle in {LocationName}: {PuzzleCategory.ToLower()} mechanism";

    /// <summary>
    /// Returns a human-readable representation of the record.
    /// </summary>
    public override string ToString() =>
        $"Technical Memory [{PuzzleCategory}] at {LocationName}: DC {OriginalDC}";

    private static bool ArePatternsSimular(string desc1, string desc2)
    {
        var keywords1 = ExtractKeywords(desc1).ToHashSet();
        var keywords2 = ExtractKeywords(desc2).ToHashSet();

        if (keywords1.Count == 0 || keywords2.Count == 0)
            return false;

        var matches = keywords1.Intersect(keywords2).Count();
        var totalUnique = keywords1.Union(keywords2).Count();

        return matches > 0 && (decimal)matches / totalUnique >= SimilarityThreshold;
    }

    private static IEnumerable<string> ExtractKeywords(string text)
    {
        return text.ToLowerInvariant()
            .Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= MinKeywordLength);
    }
}
