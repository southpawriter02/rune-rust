using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a static lore entry in the Scavenger's Journal (Codex).
/// Players compile Data Captures (fragments) to unlock and reveal these entries.
/// </summary>
/// <remarks>
/// CodexEntry is the "target" that players work toward completing.
/// Each entry has a set number of fragments needed for 100% completion.
/// The UI redacts portions of FullText based on the player's fragment count.
/// UnlockThresholds define what additional information becomes available
/// at specific completion percentages (e.g., weakness revealed at 25%).
/// </remarks>
public class CodexEntry
{
    #region Identity

    /// <summary>
    /// Gets or sets the unique identifier for this Codex entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the display title of the entry.
    /// Shown in the Journal index and entry headers.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    #endregion

    #region Classification

    /// <summary>
    /// Gets or sets the category for organizing this entry.
    /// Determines which tab displays the entry in the Journal UI.
    /// </summary>
    public EntryCategory Category { get; set; } = EntryCategory.FieldGuide;

    #endregion

    #region Content

    /// <summary>
    /// Gets or sets the complete text of the entry.
    /// The UI redacts portions based on player's completion percentage.
    /// Must be Domain 4 compliant (no precision measurements in POST-Glitch content).
    /// </summary>
    public string FullText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of fragments needed for 100% completion.
    /// Used to calculate completion percentage: (PlayerFragments / TotalFragments) * 100.
    /// </summary>
    public int TotalFragments { get; set; } = 1;

    #endregion

    #region Unlock Logic

    /// <summary>
    /// Gets or sets the unlock thresholds for progressive reveals.
    /// Key = completion percentage (e.g., 25, 50, 75, 100).
    /// Value = unlock tag or keyword (e.g., "WEAKNESS_REVEALED", "HABITAT_REVEALED").
    /// </summary>
    /// <remarks>
    /// Stored as JSONB in PostgreSQL. The UI uses these thresholds to
    /// determine what additional information to reveal at each milestone.
    /// Example: { 25: "WEAKNESS_REVEALED", 50: "HABITAT_REVEALED", 100: "FULL_ENTRY" }
    /// </remarks>
    public Dictionary<int, string> UnlockThresholds { get; set; } = new();

    #endregion

    #region Navigation

    /// <summary>
    /// Gets or sets the collection of Data Captures that contribute to this entry.
    /// Navigation property for EF Core relationship mapping.
    /// </summary>
    public ICollection<DataCapture> Fragments { get; set; } = new List<DataCapture>();

    #endregion

    #region Metadata

    /// <summary>
    /// Gets or sets the timestamp when this entry was created in the database.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this entry was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    #endregion
}
