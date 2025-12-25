using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository interface specific to SaveGame operations.
/// Extends generic repository with slot-based lookups.
/// </summary>
/// <remarks>See: SPEC-SAVE-001 for Save/Load System design.</remarks>
public interface ISaveGameRepository : IRepository<SaveGame>
{
    /// <summary>
    /// Retrieves a save game by its slot number.
    /// </summary>
    /// <param name="slotNumber">The slot number to look up.</param>
    /// <returns>The save game if found; otherwise, null.</returns>
    Task<SaveGame?> GetBySlotAsync(int slotNumber);

    /// <summary>
    /// Checks if a save game exists in the specified slot.
    /// </summary>
    /// <param name="slotNumber">The slot number to check.</param>
    /// <returns>True if a save exists in the slot; otherwise, false.</returns>
    Task<bool> SlotExistsAsync(int slotNumber);

    /// <summary>
    /// Gets all save games ordered by last played date (most recent first).
    /// </summary>
    /// <returns>A collection of save games ordered by recency.</returns>
    Task<IEnumerable<SaveGame>> GetAllOrderedByLastPlayedAsync();

    /// <summary>
    /// Gets save slot summaries without loading full JSON blobs (v0.3.18c).
    /// Uses database-level projection for minimal memory footprint.
    /// </summary>
    /// <returns>A list of save game summaries containing only metadata.</returns>
    Task<List<SaveGameSummary>> GetSummariesAsync();
}
