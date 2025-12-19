using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a lore fragment discovered by a player character.
/// Data Captures compile into Codex Entries to reveal world lore.
/// </summary>
/// <remarks>
/// Unlike inventory items, Data Captures have no weight burden and exist
/// in a separate progression system (the Scavenger's Journal). They are
/// automatically stored when discovered and contribute to Codex completion.
///
/// A capture may be unassigned (CodexEntryId = null) if its target entry
/// hasn't been identified yet, allowing for mystery fragments that auto-assign
/// when the player discovers enough related information.
/// </remarks>
public class DataCapture
{
    #region Identity

    /// <summary>
    /// Gets or sets the unique identifier for this data capture.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    #endregion

    #region Ownership

    /// <summary>
    /// Gets or sets the ID of the character who discovered this capture.
    /// Foreign key to the Character entity.
    /// </summary>
    public Guid CharacterId { get; set; }

    #endregion

    #region Assignment

    /// <summary>
    /// Gets or sets the ID of the Codex Entry this capture contributes to.
    /// Nullable for unassigned fragments awaiting auto-assignment.
    /// </summary>
    public Guid? CodexEntryId { get; set; }

    #endregion

    #region Classification

    /// <summary>
    /// Gets or sets how this fragment was captured or recorded.
    /// Affects sorting in the Codex UI and may influence Legend rewards.
    /// </summary>
    public CaptureType Type { get; set; } = CaptureType.TextFragment;

    #endregion

    #region Content

    /// <summary>
    /// Gets or sets the specific text content of this fragment.
    /// The unique lore snippet discovered at the source location.
    /// Must be Domain 4 compliant (no precision measurements).
    /// </summary>
    public string FragmentContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets where this fragment was discovered.
    /// Provides context for the player (e.g., "Found on Rusted Servitor corpse").
    /// </summary>
    public string Source { get; set; } = string.Empty;

    #endregion

    #region Quality

    /// <summary>
    /// Gets or sets the quality value of this capture.
    /// Affects Legend reward when the parent entry is completed.
    /// Standard captures: 15 points. Specialist captures: 30 points.
    /// </summary>
    public int Quality { get; set; } = 15;

    #endregion

    #region State

    /// <summary>
    /// Gets or sets whether this capture has been analyzed.
    /// Reserved for future mechanic integration (e.g., deeper analysis reveals more).
    /// </summary>
    public bool IsAnalyzed { get; set; } = false;

    #endregion

    #region Metadata

    /// <summary>
    /// Gets or sets the timestamp when this capture was discovered.
    /// </summary>
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;

    #endregion

    #region Navigation

    /// <summary>
    /// Gets or sets the Codex Entry this capture contributes to.
    /// Navigation property for EF Core relationship mapping.
    /// </summary>
    public CodexEntry? CodexEntry { get; set; }

    #endregion
}
