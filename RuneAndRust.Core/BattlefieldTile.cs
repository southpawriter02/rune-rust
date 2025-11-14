namespace RuneAndRust.Core;

/// <summary>
/// Represents a single tile on the tactical combat grid.
/// Each tile can contain terrain features, cover, traps, and at most one combatant.
/// </summary>
public class BattlefieldTile
{
    public GridPosition Position { get; set; }
    public TileType Type { get; set; }                      // Normal, HighGround, Glitched
    public CoverType Cover { get; set; }                    // None, Physical, Metaphysical, Both
    public int? CoverHealth { get; set; }                   // HP for physical cover (null if no physical cover)
    public string? CoverDescription { get; set; }           // "Pillar", "Crate", "Runic Anchor", etc.
    public List<BattlefieldTrap> Traps { get; set; }       // Active traps on this tile
    public bool IsOccupied { get; set; }
    public string? OccupantId { get; set; }                // Combatant ID (player or enemy)

    // Glitched tile properties
    public GlitchType? GlitchType { get; set; }            // Flickering, InvertedGravity, Looping
    public int GlitchSeverity { get; set; }                // 1-3 (DC scaling: 12/14/16)

    public BattlefieldTile(GridPosition position)
    {
        Position = position;
        Type = TileType.Normal;
        Cover = CoverType.None;
        CoverHealth = null;
        CoverDescription = null;
        Traps = new List<BattlefieldTrap>();
        IsOccupied = false;
        OccupantId = null;
        GlitchType = null;
        GlitchSeverity = 0;
    }

    /// <summary>
    /// Checks if this tile is passable (not occupied, not blocked by terrain)
    /// </summary>
    public bool IsPassable()
    {
        return !IsOccupied;
    }

    /// <summary>
    /// Gets a string representation of the tile for debugging
    /// </summary>
    public override string ToString()
    {
        var status = IsOccupied ? $"[{OccupantId}]" : "[ ]";
        var coverStr = Cover != CoverType.None ? $" Cover:{Cover}" : "";
        if (CoverHealth.HasValue)
        {
            coverStr += $"({CoverHealth}HP)";
        }
        if (!string.IsNullOrEmpty(CoverDescription))
        {
            coverStr += $"[{CoverDescription}]";
        }
        var glitchStr = GlitchType != null ? $" GLITCH:{GlitchType}({GlitchSeverity})" : "";
        var trapStr = Traps.Count > 0 ? $" Traps:{Traps.Count}" : "";

        return $"{Position} {status}{coverStr}{glitchStr}{trapStr}";
    }
}

/// <summary>
/// Type of battlefield tile
/// </summary>
public enum TileType
{
    Normal,         // Standard ground tile
    HighGround,     // Elevated position (+2 Accuracy, +2 Defense)
    Glitched        // Corrupted tile with hazards
}

/// <summary>
/// Type of cover provided by a tile
/// </summary>
public enum CoverType
{
    None,               // No cover
    Physical,           // Blocks physical ranged attacks (+4 Defense)
    Metaphysical,       // Dampens Psychic Stress (+4 WILL)
    Both                // Blocks both physical and psychic threats
}

/// <summary>
/// Type of glitch affecting a corrupted tile
/// </summary>
public enum GlitchType
{
    Flickering,         // Platform flickers in/out - FINESSE check or fall
    InvertedGravity,    // Gravity reversed - STURDINESS check or [Disoriented]
    Looping             // Space loops - teleports to random tile
}
