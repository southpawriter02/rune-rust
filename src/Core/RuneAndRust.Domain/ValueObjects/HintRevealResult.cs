namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains the result of checking for and revealing puzzle hints
/// during an examination, including any newly unlocked interactions.
/// </summary>
/// <remarks>
/// This result is returned by IPuzzleHintService.CheckForHints() and
/// integrated into the ExaminationResult to provide complete feedback
/// to the player about their discovery.
/// </remarks>
/// <param name="ObjectId">The examined object.</param>
/// <param name="HintsRevealed">List of hints discovered during this examination.</param>
/// <param name="InteractionsUnlocked">List of new commands now available.</param>
/// <param name="ConditionsNotMet">Count of hints that exist but weren't revealed due to unmet conditions.</param>
public readonly record struct HintRevealResult(
    string ObjectId,
    IReadOnlyList<ExaminationPuzzleHint> HintsRevealed,
    IReadOnlyList<string> InteractionsUnlocked,
    int ConditionsNotMet)
{
    /// <summary>
    /// Gets whether any hints were revealed.
    /// </summary>
    public bool HasRevealedHints => HintsRevealed.Count > 0;

    /// <summary>
    /// Gets whether any new interactions were unlocked.
    /// </summary>
    public bool HasUnlockedInteractions => InteractionsUnlocked.Count > 0;

    /// <summary>
    /// Gets whether there are hints the player couldn't access due to conditions.
    /// </summary>
    public bool HasHiddenHints => ConditionsNotMet > 0;

    /// <summary>
    /// Gets the total number of hints revealed.
    /// </summary>
    public int TotalHintsRevealed => HintsRevealed.Count;

    /// <summary>
    /// Creates a display-ready summary of the reveal result.
    /// </summary>
    /// <returns>A formatted string summarizing what was discovered.</returns>
    public string ToSummaryString()
    {
        if (!HasRevealedHints)
        {
            return string.Empty;
        }

        var parts = new List<string>();
        foreach (var hint in HintsRevealed)
        {
            parts.Add(hint.ToRevealString());
        }

        return string.Join("\n", parts);
    }

    /// <summary>
    /// Creates an empty result for when no hints are found.
    /// </summary>
    /// <param name="objectId">The examined object ID.</param>
    /// <returns>An empty hint reveal result.</returns>
    public static HintRevealResult Empty(string objectId) =>
        new(objectId, Array.Empty<ExaminationPuzzleHint>(), Array.Empty<string>(), 0);
}
