using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a persistent save game record stored in the database.
/// Maps runtime GameState to a serializable database entity.
/// </summary>
/// <remarks>
/// v0.3.21a: Added Metadata property for lightweight save previews.
/// v0.3.21b: SlotNumber 0 = Autosave, -1/-2 = Rolling Backups.
/// </remarks>
public class SaveGame
{
    /// <summary>
    /// Gets or sets the unique identifier for this save game.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the save slot number.
    /// Slot 0 = Primary Autosave, Slots 1-3 = Manual Saves,
    /// Slots -1/-2 = Rolling Backup Autosaves (v0.3.21b).
    /// Must be unique per user/session.
    /// </summary>
    public int SlotNumber { get; set; }

    /// <summary>
    /// Gets or sets the name of the character associated with this save.
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when this save was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp of the last time this save was updated.
    /// </summary>
    public DateTime LastPlayed { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the lightweight metadata snapshot for save preview (v0.3.21a).
    /// Stored as JSONB in PostgreSQL. Contains HP, Level, Location, etc.
    /// </summary>
    public SaveMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the JSON-serialized state of the game.
    /// Contains character data, phase, turn count, and other runtime state.
    /// Stored as JSONB in PostgreSQL for efficient querying.
    /// </summary>
    public string SerializedState { get; set; } = "{}";
}
