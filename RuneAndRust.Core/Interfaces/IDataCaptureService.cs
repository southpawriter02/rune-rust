using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for Data Capture generation and management.
/// Handles discovery of lore fragments during gameplay and tracks Codex completion.
/// </summary>
public interface IDataCaptureService
{
    /// <summary>
    /// Attempts to generate a data capture during container search.
    /// Uses base 25% chance modified by WITS bonus.
    /// </summary>
    /// <param name="characterId">The character discovering the capture.</param>
    /// <param name="container">The container being searched.</param>
    /// <param name="witsBonus">Character's WITS attribute bonus.</param>
    /// <returns>The capture result indicating success and any generated capture.</returns>
    Task<CaptureResult> TryGenerateFromSearchAsync(
        Guid characterId,
        InteractableObject container,
        int witsBonus = 0);

    /// <summary>
    /// Attempts to generate a data capture during object examination.
    /// Higher tiers have better capture chances (Expert: 75%, Detailed: 37%, Base: 0%).
    /// </summary>
    /// <param name="characterId">The character examining the object.</param>
    /// <param name="target">The object being examined.</param>
    /// <param name="tierRevealed">The examination tier achieved (0=Base, 1=Detailed, 2=Expert).</param>
    /// <param name="witsBonus">Character's WITS attribute bonus.</param>
    /// <returns>The capture result indicating success and any generated capture.</returns>
    Task<CaptureResult> TryGenerateFromExaminationAsync(
        Guid characterId,
        InteractableObject target,
        int tierRevealed,
        int witsBonus = 0);

    /// <summary>
    /// Gets the completion percentage for a specific Codex entry for a character.
    /// </summary>
    /// <param name="entryId">The Codex entry to check.</param>
    /// <param name="characterId">The character whose progress to check.</param>
    /// <returns>Completion percentage (0-100).</returns>
    Task<int> GetCompletionPercentageAsync(Guid entryId, Guid characterId);

    /// <summary>
    /// Gets all unlocked threshold tags for a character's progress on an entry.
    /// </summary>
    /// <param name="entryId">The Codex entry to check.</param>
    /// <param name="characterId">The character whose unlocks to retrieve.</param>
    /// <returns>Collection of unlocked threshold tags (e.g., "WEAKNESS_REVEALED").</returns>
    Task<IEnumerable<string>> GetUnlockedThresholdsAsync(Guid entryId, Guid characterId);
}
