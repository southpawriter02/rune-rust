namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Tracks clues and deductions discovered by each player character.
/// </summary>
public interface IPlayerClueTracker
{
    /// <summary>
    /// Records that a character has discovered a clue.
    /// </summary>
    /// <param name="characterId">The character who discovered the clue.</param>
    /// <param name="clueId">The clue identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordClueDiscoveryAsync(string characterId, string clueId, CancellationToken ct = default);

    /// <summary>
    /// Records that a character has made a deduction.
    /// </summary>
    /// <param name="characterId">The character who made the deduction.</param>
    /// <param name="deductionId">The deduction identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordDeductionAsync(string characterId, string deductionId, CancellationToken ct = default);

    /// <summary>
    /// Gets all clue IDs discovered by a character.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>Collection of discovered clue IDs.</returns>
    IReadOnlyList<string> GetAllClues(string characterId);

    /// <summary>
    /// Gets all deduction IDs made by a character.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>Collection of made deduction IDs.</returns>
    IReadOnlyList<string> GetAllDeductions(string characterId);

    /// <summary>
    /// Checks if a character has discovered a specific clue.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="clueId">The clue identifier.</param>
    /// <returns>True if the character has discovered the clue.</returns>
    bool HasClue(string characterId, string clueId);

    /// <summary>
    /// Checks if a character has made a specific deduction.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="deductionId">The deduction identifier.</param>
    /// <returns>True if the character has made the deduction.</returns>
    bool HasDeduction(string characterId, string deductionId);

    /// <summary>
    /// Gets the count of clues discovered by a character.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>Number of clues discovered.</returns>
    int GetClueCount(string characterId);

    /// <summary>
    /// Clears all clues and deductions for a character (for testing/reset).
    /// </summary>
    /// <param name="characterId">The character to clear.</param>
    void ClearAll(string characterId);
}
