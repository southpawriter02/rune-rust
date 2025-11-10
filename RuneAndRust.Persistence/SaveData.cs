using RuneAndRust.Core;

namespace RuneAndRust.Persistence;

/// <summary>
/// Data transfer object for saving/loading game state
/// </summary>
public class SaveData
{
    // Character Info
    public string CharacterName { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }

    // Progression
    public int Level { get; set; }
    public int CurrentXP { get; set; }

    // Attributes
    public int Might { get; set; }
    public int Finesse { get; set; }
    public int Wits { get; set; }
    public int Will { get; set; }
    public int Sturdiness { get; set; }

    // Resources
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }

    // World State
    public int CurrentRoomId { get; set; }
    public string ClearedRoomsJson { get; set; } = "[]"; // JSON array of room IDs
    public bool PuzzleSolved { get; set; }
    public bool BossDefeated { get; set; }

    // Metadata
    public DateTime LastSaved { get; set; }
}

public class SaveInfo
{
    public string CharacterName { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }
    public int Level { get; set; }
    public DateTime LastPlayed { get; set; }
    public bool BossDefeated { get; set; }
}
