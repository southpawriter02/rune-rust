namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a logical conclusion that can be drawn from combining
/// multiple clues. Deductions reveal hidden truths and advance understanding.
/// </summary>
/// <remarks>
/// <para>
/// Deductions become available when the required clues are gathered.
/// Each deduction specifies which clues are prerequisites. When all
/// prerequisites are met, the deduction can be made.
/// </para>
/// </remarks>
public sealed record Deduction
{
    /// <summary>
    /// Unique identifier for this deduction.
    /// </summary>
    public required string DeductionId { get; init; }

    /// <summary>
    /// Display name for the deduction.
    /// </summary>
    public required string DeductionName { get; init; }

    /// <summary>
    /// Collection of clue IDs required to make this deduction.
    /// </summary>
    public required IReadOnlyList<string> RequiredClueIds { get; init; }

    /// <summary>
    /// The logical conclusion text revealed when deduction is made.
    /// </summary>
    public required string ConclusionText { get; init; }

    /// <summary>
    /// Narrative text shown when the deduction is made.
    /// </summary>
    public required string DeductionNarrative { get; init; }

    /// <summary>
    /// Optional: New interactions or commands unlocked by this deduction.
    /// </summary>
    public IReadOnlyList<string> UnlocksInteractions { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Optional: Quest or story flags set by this deduction.
    /// </summary>
    public IReadOnlyList<string> SetsFlags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Whether this deduction reveals the identity of someone/something.
    /// </summary>
    public bool RevealsIdentity { get; init; }

    /// <summary>
    /// Whether this deduction reveals a cause or motive.
    /// </summary>
    public bool RevealsCause { get; init; }

    /// <summary>
    /// The minimum number of required clues needed to attempt this deduction.
    /// </summary>
    public int MinimumCluesRequired => RequiredClueIds.Count;

    /// <summary>
    /// Gets whether this deduction unlocks any interactions.
    /// </summary>
    public bool HasUnlocks => UnlocksInteractions.Count > 0;

    /// <summary>
    /// Gets whether this deduction sets any flags.
    /// </summary>
    public bool HasFlags => SetsFlags.Count > 0;

    /// <summary>
    /// Checks if all required clues have been gathered.
    /// </summary>
    /// <param name="gatheredClueIds">Collection of gathered clue IDs.</param>
    /// <returns>True if all required clues are present.</returns>
    public bool CanDeduce(IEnumerable<string> gatheredClueIds)
    {
        var clueSet = gatheredClueIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return RequiredClueIds.All(id => clueSet.Contains(id));
    }

    /// <summary>
    /// Gets the count of required clues that are still missing.
    /// </summary>
    /// <param name="gatheredClueIds">Collection of gathered clue IDs.</param>
    /// <returns>Number of missing clues.</returns>
    public int GetMissingClueCount(IEnumerable<string> gatheredClueIds)
    {
        var clueSet = gatheredClueIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return RequiredClueIds.Count(id => !clueSet.Contains(id));
    }

    /// <summary>
    /// Creates a simple deduction for testing.
    /// </summary>
    public static Deduction Create(
        string deductionId,
        string deductionName,
        IReadOnlyList<string> requiredClueIds,
        string conclusionText,
        string deductionNarrative) =>
        new()
        {
            DeductionId = deductionId,
            DeductionName = deductionName,
            RequiredClueIds = requiredClueIds,
            ConclusionText = conclusionText,
            DeductionNarrative = deductionNarrative
        };
}
