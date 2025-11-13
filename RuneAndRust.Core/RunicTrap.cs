namespace RuneAndRust.Core;

/// <summary>
/// v0.19.10: Type of runic trap
/// </summary>
public enum RunicTrapType
{
    Hagalaz,        // Ice damage AoE
    Disruption,     // Applies [Disoriented] status
    Isolation       // Applies [Rooted] status + blocks buffs
}

/// <summary>
/// v0.19.10: Runic Trap placed by Rúnasmiðr on battlefield
/// Traps are invisible to enemies and trigger when an enemy enters the tile
/// </summary>
public class RunicTrap
{
    public RunicTrapType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Row { get; set; } // Battlefield position
    public int Column { get; set; }
    public int TurnsRemaining { get; set; } = 3; // Expires after 3 turns if not triggered
    public string OwnerName { get; set; } = string.Empty; // Who placed it (for Runic Synergy)
    public bool IsVisible { get; set; } = false; // Invisible to enemies (always false for tactical)

    // Trap effects (set based on type and rank)
    public string DamageDice { get; set; } = "2d6"; // e.g., "2d6", "3d6", "4d6"
    public string DamageType { get; set; } = "Ice"; // Ice, Psychic, Arcane
    public bool IsAoE { get; set; } = false;
    public string AoEPattern { get; set; } = string.Empty; // "row", "column", "3x3"
    public string StatusEffect { get; set; } = string.Empty; // "Disoriented", "Rooted", "Chilled"
    public int StatusDuration { get; set; } = 0; // Turns
    public int SaveDC { get; set; } = 0; // For status effects
    public string SaveAttribute { get; set; } = string.Empty; // "WILL", "FINESSE"
    public bool BlocksExternalBuffs { get; set; } = false; // Rune of Isolation special

    /// <summary>
    /// Create a Hagalaz Trap (Tier 1)
    /// </summary>
    public static RunicTrap CreateHagalazTrap(int row, int column, string ownerName, int rank = 1)
    {
        var damageDice = rank switch
        {
            1 => "2d6",
            2 => "3d6",
            3 => "4d6",
            _ => "2d6"
        };

        return new RunicTrap
        {
            Type = RunicTrapType.Hagalaz,
            Name = "Hagalaz Trap",
            Row = row,
            Column = column,
            OwnerName = ownerName,
            TurnsRemaining = 3,
            IsVisible = false,
            DamageDice = damageDice,
            DamageType = "Ice",
            IsAoE = true,
            AoEPattern = "row",
            StatusEffect = rank >= 3 ? "Chilled" : "",
            StatusDuration = rank >= 3 ? 2 : 0
        };
    }

    /// <summary>
    /// Create a Rune of Disruption (Tier 2)
    /// </summary>
    public static RunicTrap CreateDisruptionTrap(int row, int column, string ownerName, int rank = 1)
    {
        var dc = rank switch
        {
            1 => 14,
            2 => 16,
            3 => 18,
            _ => 14
        };

        var duration = rank switch
        {
            1 => 2,
            2 => 3,
            3 => 3,
            _ => 2
        };

        return new RunicTrap
        {
            Type = RunicTrapType.Disruption,
            Name = "Rune of Disruption",
            Row = row,
            Column = column,
            OwnerName = ownerName,
            TurnsRemaining = 3,
            IsVisible = false,
            DamageDice = rank >= 3 ? "2d6" : "0",
            DamageType = "Psychic",
            StatusEffect = "Disoriented",
            StatusDuration = duration,
            SaveDC = dc,
            SaveAttribute = "WILL"
        };
    }

    /// <summary>
    /// Create a Rune of Isolation (Tier 3)
    /// </summary>
    public static RunicTrap CreateIsolationTrap(int row, int column, string ownerName, int rank = 1)
    {
        var duration = rank switch
        {
            1 => 2,
            2 => 3,
            3 => 3,
            _ => 2
        };

        return new RunicTrap
        {
            Type = RunicTrapType.Isolation,
            Name = "Rune of Isolation",
            Row = row,
            Column = column,
            OwnerName = ownerName,
            TurnsRemaining = 3,
            IsVisible = false,
            DamageDice = rank >= 3 ? "3d6" : "0",
            DamageType = "Arcane",
            StatusEffect = "Rooted",
            StatusDuration = duration,
            BlocksExternalBuffs = true
        };
    }

    /// <summary>
    /// Get trap description for UI
    /// </summary>
    public string GetDescription()
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(DamageDice) && DamageDice != "0")
        {
            string aoeText = IsAoE ? $" {AoEPattern} AoE" : "";
            parts.Add($"{DamageDice} {DamageType} damage{aoeText}");
        }

        if (!string.IsNullOrEmpty(StatusEffect))
        {
            string saveText = SaveDC > 0 ? $" (DC {SaveDC})" : "";
            parts.Add($"[{StatusEffect}] for {StatusDuration} turns{saveText}");
        }

        if (BlocksExternalBuffs)
        {
            parts.Add("Blocks external buffs/healing");
        }

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Decrement trap duration (returns true if still active)
    /// </summary>
    public bool DecrementDuration()
    {
        TurnsRemaining--;
        return TurnsRemaining > 0;
    }
}
