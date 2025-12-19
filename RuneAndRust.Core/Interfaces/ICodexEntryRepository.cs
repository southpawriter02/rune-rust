using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for Codex Entry repository operations.
/// Extends IRepository with Codex-specific queries.
/// </summary>
public interface ICodexEntryRepository : IRepository<CodexEntry>
{
    /// <summary>
    /// Gets all Codex entries in a specific category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>A collection of entries in that category.</returns>
    Task<IEnumerable<CodexEntry>> GetByCategoryAsync(EntryCategory category);

    /// <summary>
    /// Gets a Codex entry by its title using case-insensitive matching.
    /// </summary>
    /// <param name="title">The entry title to search for.</param>
    /// <returns>The entry if found, null otherwise.</returns>
    Task<CodexEntry?> GetByTitleAsync(string title);

    /// <summary>
    /// Gets a Codex entry with its related Data Capture fragments included.
    /// Uses eager loading for the Fragments navigation property.
    /// </summary>
    /// <param name="id">The entry ID to retrieve.</param>
    /// <returns>The entry with fragments if found, null otherwise.</returns>
    Task<CodexEntry?> GetWithFragmentsAsync(Guid id);

    /// <summary>
    /// Adds multiple Codex entries in a single operation.
    /// Used for batch seeding of lore content.
    /// </summary>
    /// <param name="entries">The entries to add.</param>
    Task AddRangeAsync(IEnumerable<CodexEntry> entries);
}
