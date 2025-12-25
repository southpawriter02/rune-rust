namespace RuneAndRust.Core.Models;

/// <summary>
/// Lightweight DTO for save slot display (v0.3.18c - The Snapshot).
/// Intentionally excludes SerializedState for minimal memory footprint.
/// Used by repository projection queries to avoid fetching large blob columns.
/// </summary>
/// <remarks>See: SPEC-SAVE-001 for Save/Load System design.</remarks>
public class SaveGameSummary
{
    /// <summary>
    /// Gets or sets the slot number (1-3).
    /// </summary>
    public int SlotNumber { get; set; }

    /// <summary>
    /// Gets or sets the character name displayed for this save.
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the save was last played.
    /// </summary>
    public DateTime LastPlayed { get; set; }

    /// <summary>
    /// Gets or sets whether the slot is empty.
    /// True for placeholder entries where no save exists.
    /// </summary>
    public bool IsEmpty { get; set; } = true;
}
