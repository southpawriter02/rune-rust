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

    // Economy (v0.9)
    public int Currency { get; set; }

    // Trauma Economy (v0.5)
    public int PsychicStress { get; set; }
    public int Corruption { get; set; }
    public int RoomsExploredSinceRest { get; set; }

    // Trauma Economy (v0.15) - Permanent Traumas
    public string TraumasJson { get; set; } = "[]"; // JSON array of Trauma objects

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

    // Procedural Generation (v0.10)
    public int CurrentDungeonSeed { get; set; } = 0; // Seed for current dungeon
    public int DungeonsCompleted { get; set; } = 0; // Total dungeons completed
    public string? CurrentRoomStringId { get; set; } = null; // String room ID for procedural dungeons
    public bool IsProceduralDungeon { get; set; } = false; // True if using procedural generation

    // Equipment System (v0.3)
    public string? EquippedWeaponJson { get; set; } = null; // Serialized Equipment object
    public string? EquippedArmorJson { get; set; } = null; // Serialized Equipment object
    public string InventoryJson { get; set; } = "[]"; // JSON array of Equipment objects
    public string RoomItemsJson { get; set; } = "{}"; // JSON dictionary: { roomId: [Equipment...] }

    // Consumables & Crafting (v0.7)
    public string ConsumablesJson { get; set; } = "[]"; // JSON array of Consumable objects
    public string CraftingComponentsJson { get; set; } = "{}"; // JSON dictionary: { ComponentType: count }

    // NPC & Quest System (v0.8)
    public string FactionReputationsJson { get; set; } = "{}"; // JSON dictionary: { FactionType: reputation }
    public string ActiveQuestsJson { get; set; } = "[]"; // JSON array of Quest objects
    public string CompletedQuestsJson { get; set; } = "[]"; // JSON array of Quest objects
    public string NPCStatesJson { get; set; } = "[]"; // JSON array of NPC objects with state

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
