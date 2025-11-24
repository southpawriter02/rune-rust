namespace RuneAndRust.Core.NewGamePlus;

/// <summary>
/// v0.40.1: Snapshot of character progression data for NG+ carryover
/// Stores what persists between campaign replays
/// </summary>
public class CarryoverSnapshot
{
    /// <summary>Database ID for this snapshot</summary>
    public int CarryoverId { get; set; }

    /// <summary>Character ID this snapshot belongs to</summary>
    public int CharacterId { get; set; }

    /// <summary>Target NG+ tier for this snapshot</summary>
    public int NGPlusTier { get; set; }

    /// <summary>When this snapshot was created (UTC)</summary>
    public DateTime TimestampUtc { get; set; }

    // ═══════════════════════════════════════════════════════════
    // CHARACTER PROGRESSION (RETAINED)
    // ═══════════════════════════════════════════════════════════

    /// <summary>Character level</summary>
    public int CharacterLevel { get; set; }

    /// <summary>Accumulated Legend points</summary>
    public int LegendPoints { get; set; }

    /// <summary>Total Progression Points earned</summary>
    public int ProgressionPoints { get; set; }

    /// <summary>Unspent Progression Points</summary>
    public int UnspentProgressionPoints { get; set; }

    // ═══════════════════════════════════════════════════════════
    // ATTRIBUTES (RETAINED)
    // ═══════════════════════════════════════════════════════════

    /// <summary>Character attributes (MIGHT, FINESSE, WITS, WILL, STURDINESS)</summary>
    public Dictionary<string, int> Attributes { get; set; } = new();

    // ═══════════════════════════════════════════════════════════
    // SPECIALIZATIONS & ABILITIES (RETAINED)
    // ═══════════════════════════════════════════════════════════

    /// <summary>Unlocked specializations</summary>
    public List<string> UnlockedSpecializations { get; set; } = new();

    /// <summary>Learned abilities</summary>
    public List<string> LearnedAbilities { get; set; } = new();

    // ═══════════════════════════════════════════════════════════
    // EQUIPMENT (RETAINED)
    // ═══════════════════════════════════════════════════════════

    /// <summary>Currently equipped items</summary>
    public List<Equipment> EquippedItems { get; set; } = new();

    /// <summary>Inventory items (not equipped)</summary>
    public List<Equipment> InventoryItems { get; set; } = new();

    // ═══════════════════════════════════════════════════════════
    // CRAFTING (RETAINED)
    // ═══════════════════════════════════════════════════════════

    /// <summary>Crafting materials and components</summary>
    public Dictionary<string, int> CraftingMaterials { get; set; } = new();

    /// <summary>Unlocked crafting recipes</summary>
    public List<string> UnlockedRecipes { get; set; } = new();

    // ═══════════════════════════════════════════════════════════
    // CURRENCY (RETAINED)
    // ═══════════════════════════════════════════════════════════

    /// <summary>Scrap currency</summary>
    public int Scrap { get; set; }

    // ═══════════════════════════════════════════════════════════
    // PRE-RESET SNAPSHOTS (FOR DEBUGGING/VERIFICATION)
    // ═══════════════════════════════════════════════════════════

    /// <summary>Quest state before reset (JSON)</summary>
    public string? QuestStateSnapshot { get; set; }

    /// <summary>World state before reset (JSON)</summary>
    public string? WorldStateSnapshot { get; set; }
}
