using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// Tab options for the Journal UI navigation (v0.3.7c).
/// Maps to grouped EntryCategory values.
/// </summary>
public enum JournalTab
{
    /// <summary>
    /// Lore entries: BlightOrigin, Factions, Technical, Geography categories.
    /// </summary>
    Codex = 0,

    /// <summary>
    /// Creature data and weaknesses: Bestiary category.
    /// </summary>
    Bestiary = 1,

    /// <summary>
    /// Game mechanics and tutorials: FieldGuide category.
    /// </summary>
    FieldGuide = 2,

    /// <summary>
    /// Active and completed quests (future feature placeholder).
    /// </summary>
    Contracts = 3
}

/// <summary>
/// Main ViewModel for the full-screen Journal UI (v0.3.7c).
/// Contains all display-ready data for rendering THE ARCHIVE screen.
/// </summary>
/// <param name="CharacterName">The name of the character viewing the journal.</param>
/// <param name="StressLevel">Current psychic stress level (0-100) for glitch effects.</param>
/// <param name="ActiveTab">The currently selected tab for filtering entries.</param>
/// <param name="Entries">Filtered entry list for the active tab.</param>
/// <param name="SelectedEntryIndex">Navigation cursor position (0-based).</param>
/// <param name="SelectedDetail">Expanded detail panel data for the selected entry.</param>
public record JournalViewModel(
    string CharacterName,
    int StressLevel,
    JournalTab ActiveTab,
    List<JournalEntryView> Entries,
    int SelectedEntryIndex,
    JournalEntryDetailView? SelectedDetail
);

/// <summary>
/// Summary row for an entry in the Journal list panel (v0.3.7c).
/// Displays title, category, and completion status.
/// </summary>
/// <param name="Index">1-based display index for navigation.</param>
/// <param name="EntryId">Unique identifier for detail lookup.</param>
/// <param name="Title">Display title of the Codex entry.</param>
/// <param name="Category">Original EntryCategory for icon/color display.</param>
/// <param name="CompletionPercent">Current completion percentage (0-100).</param>
/// <param name="IsComplete">True if CompletionPercent >= 100.</param>
public record JournalEntryView(
    int Index,
    Guid EntryId,
    string Title,
    EntryCategory Category,
    int CompletionPercent,
    bool IsComplete
);

/// <summary>
/// Expanded detail view for a selected Journal entry (v0.3.7c).
/// Shows redacted content, milestones, and fragment progress.
/// </summary>
/// <param name="EntryId">Unique identifier of the entry.</param>
/// <param name="Title">Display title of the entry.</param>
/// <param name="Category">Entry category for display.</param>
/// <param name="CompletionPercent">Current completion percentage (0-100).</param>
/// <param name="RedactedContent">Entry text with redaction applied via TextRedactor.</param>
/// <param name="UnlockedThresholds">List of milestone tags that have been unlocked.</param>
/// <param name="FragmentsCollected">Number of fragments the character has collected.</param>
/// <param name="FragmentsRequired">Total fragments needed for 100% completion.</param>
public record JournalEntryDetailView(
    Guid EntryId,
    string Title,
    EntryCategory Category,
    int CompletionPercent,
    string RedactedContent,
    List<string> UnlockedThresholds,
    int FragmentsCollected,
    int FragmentsRequired
);
