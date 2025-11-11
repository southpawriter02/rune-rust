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
    public Specialization Specialization { get; set; } = Specialization.None; // v0.7: Unlocked with 10 PP

    // Progression (Aethelgard Saga System)
    public int CurrentMilestone { get; set; }
    public int CurrentLegend { get; set; }
    public int ProgressionPoints { get; set; }

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

    // Trauma Economy (v0.5)
    public int PsychicStress { get; set; }
    public int Corruption { get; set; }
    public int RoomsExploredSinceRest { get; set; }

    // v0.7: Adept Status Effects
    public int VulnerableTurnsRemaining { get; set; }
    public int AnalyzedTurnsRemaining { get; set; }
    public int SeizedTurnsRemaining { get; set; }
    public bool IsPerforming { get; set; }
    public int PerformingTurnsRemaining { get; set; }
    public string? CurrentPerformance { get; set; }
    public int InspiredTurnsRemaining { get; set; }
    public int SilencedTurnsRemaining { get; set; }
    public int TempHP { get; set; }

    // World State
    public int CurrentRoomId { get; set; }
    public string ClearedRoomsJson { get; set; } = "[]"; // JSON array of room IDs
    public bool PuzzleSolved { get; set; }
    public bool BossDefeated { get; set; }

    // Equipment System (v0.3)
    public string? EquippedWeaponJson { get; set; } = null; // Serialized Equipment object
    public string? EquippedArmorJson { get; set; } = null; // Serialized Equipment object
    public string InventoryJson { get; set; } = "[]"; // JSON array of Equipment objects
    public string RoomItemsJson { get; set; } = "{}"; // JSON dictionary: { roomId: [Equipment...] }

    // Metadata
    public DateTime LastSaved { get; set; }
}

public class SaveInfo
{
    public string CharacterName { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }
    public Specialization Specialization { get; set; } = Specialization.None; // v0.7
    public int CurrentMilestone { get; set; }
    public DateTime LastPlayed { get; set; }
    public bool BossDefeated { get; set; }
}
