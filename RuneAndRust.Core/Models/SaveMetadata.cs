namespace RuneAndRust.Core.Models;

/// <summary>
/// Lightweight snapshot of game state stored with save files (v0.3.21a - The Preview).
/// Enables the Load Menu to display character vitals and location without deserializing
/// the full SerializedState blob.
/// </summary>
/// <remarks>See: SPEC-SAVE-001 for Save/Load System design.</remarks>
public class SaveMetadata
{
    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character's archetype (e.g., "Warrior", "Mystic").
    /// </summary>
    public string Archetype { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character's level.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Gets or sets the character's current health points.
    /// </summary>
    public int CurrentHp { get; set; }

    /// <summary>
    /// Gets or sets the character's maximum health points.
    /// </summary>
    public int MaxHp { get; set; }

    /// <summary>
    /// Gets or sets the current room/location name.
    /// </summary>
    public string LocationName { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the total playtime for this save.
    /// </summary>
    public TimeSpan TotalPlaytime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the save was created.
    /// </summary>
    public DateTime SaveTimestamp { get; set; }

    /// <summary>
    /// Creates an empty/default metadata instance for corrupt or empty slots.
    /// </summary>
    /// <returns>A SaveMetadata with default "empty" values.</returns>
    public static SaveMetadata Empty() =>
        new()
        {
            CharacterName = "Empty",
            Archetype = "None",
            Level = 0,
            CurrentHp = 0,
            MaxHp = 0,
            LocationName = "Void",
            TotalPlaytime = TimeSpan.Zero,
            SaveTimestamp = DateTime.MinValue
        };
}
