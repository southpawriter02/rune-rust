namespace RuneAndRust.Core;

/// <summary>
/// Represents a position on the tactical combat grid.
/// Grid structure: 2 zones (Player/Enemy) × 2 rows (Front/Back) × N columns
/// </summary>
public struct GridPosition
{
    public Zone Zone { get; set; }          // Player or Enemy
    public Row Row { get; set; }            // Front or Back
    public int Column { get; set; }         // 0-based column index
    public int Elevation { get; set; }      // Z-coordinate (0 = ground, 1+ = elevated)

    public GridPosition(Zone zone, Row row, int column, int elevation = 0)
    {
        Zone = zone;
        Row = row;
        Column = column;
        Elevation = elevation;
    }

    public override string ToString()
    {
        return $"{Zone}/{Row}/Col{Column}" + (Elevation > 0 ? $"/+{Elevation}" : "");
    }

    public override bool Equals(object? obj)
    {
        if (obj is not GridPosition other)
            return false;

        return Zone == other.Zone
            && Row == other.Row
            && Column == other.Column
            && Elevation == other.Elevation;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Zone, Row, Column, Elevation);
    }

    public static bool operator ==(GridPosition left, GridPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridPosition left, GridPosition right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// Zone on the battlefield - Player side or Enemy side
/// </summary>
public enum Zone
{
    Player,     // Player-controlled combatants
    Enemy       // Enemy combatants
}

/// <summary>
/// Row within a zone - Front row (melee) or Back row (support/ranged)
/// </summary>
public enum Row
{
    Front,      // Front row - can perform melee attacks
    Back        // Back row - requires Reach or Ranged to attack
}
