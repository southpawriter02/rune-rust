using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for Data Capture repository operations.
/// Extends IRepository with capture-specific queries for the Scavenger's Journal.
/// </summary>
public interface IDataCaptureRepository : IRepository<DataCapture>
{
    /// <summary>
    /// Gets all Data Captures discovered by a specific character.
    /// Primary query for populating the player's Journal.
    /// </summary>
    /// <param name="characterId">The character ID to filter by.</param>
    /// <returns>A collection of captures discovered by that character.</returns>
    Task<IEnumerable<DataCapture>> GetByCharacterIdAsync(Guid characterId);

    /// <summary>
    /// Gets all Data Captures for a specific Codex Entry and character.
    /// Used to determine completion percentage for an entry.
    /// </summary>
    /// <param name="entryId">The Codex Entry ID to filter by.</param>
    /// <param name="characterId">The character ID to filter by.</param>
    /// <returns>A collection of captures contributing to that entry.</returns>
    Task<IEnumerable<DataCapture>> GetByEntryIdAsync(Guid entryId, Guid characterId);

    /// <summary>
    /// Gets the count of fragments a character has for a specific entry.
    /// Used for completion percentage calculation.
    /// </summary>
    /// <param name="entryId">The Codex Entry ID to count fragments for.</param>
    /// <param name="characterId">The character ID to filter by.</param>
    /// <returns>The number of fragments found.</returns>
    Task<int> GetFragmentCountAsync(Guid entryId, Guid characterId);

    /// <summary>
    /// Gets all unassigned Data Captures for a character.
    /// These are fragments awaiting auto-assignment to a Codex Entry.
    /// </summary>
    /// <param name="characterId">The character ID to filter by.</param>
    /// <returns>A collection of unassigned captures.</returns>
    Task<IEnumerable<DataCapture>> GetUnassignedAsync(Guid characterId);

    /// <summary>
    /// Adds multiple Data Captures in a single operation.
    /// Used for batch operations during testing or data import.
    /// </summary>
    /// <param name="captures">The captures to add.</param>
    Task AddRangeAsync(IEnumerable<DataCapture> captures);
}
