namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for Scavenger's Journal display formatting.
/// Provides methods to format journal views with redaction based on completion percentages.
/// </summary>
public interface IJournalService
{
    /// <summary>
    /// Formats the journal list view showing all discovered entries grouped by category.
    /// Entries are displayed with completion percentages and status indicators.
    /// </summary>
    /// <param name="characterId">The character whose journal to display.</param>
    /// <returns>Formatted string for terminal display with Spectre.Console markup.</returns>
    Task<string> FormatJournalListAsync(Guid characterId);

    /// <summary>
    /// Formats a specific Codex entry detail view with redaction based on completion.
    /// Shows entry metadata, redacted content, and unlocked threshold discoveries.
    /// </summary>
    /// <param name="characterId">The character viewing the entry.</param>
    /// <param name="entryTitle">The title of the entry to display.</param>
    /// <returns>Formatted string for terminal display with Spectre.Console markup.</returns>
    Task<string> FormatEntryDetailAsync(Guid characterId, string entryTitle);

    /// <summary>
    /// Formats the list of unassigned captures awaiting matching entries.
    /// Shows fragments that have been discovered but not yet linked to a Codex entry.
    /// </summary>
    /// <param name="characterId">The character whose fragments to display.</param>
    /// <returns>Formatted string for terminal display with Spectre.Console markup.</returns>
    Task<string> FormatUnassignedCapturesAsync(Guid characterId);
}
