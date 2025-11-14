namespace RuneAndRust.Core;

/// <summary>
/// Represents the tactical combat grid battlefield.
/// Structure: 2 zones (Player/Enemy) × 2 rows (Front/Back) × N columns (dynamic)
/// </summary>
public class BattlefieldGrid
{
    public string GridId { get; set; }
    public int Columns { get; set; }                                // Dynamic based on combatant count
    public Dictionary<GridPosition, BattlefieldTile> Tiles { get; set; }

    public BattlefieldGrid(int columns, string? gridId = null)
    {
        GridId = gridId ?? Guid.NewGuid().ToString();
        Columns = columns;
        Tiles = new Dictionary<GridPosition, BattlefieldTile>();
        InitializeGrid();
    }

    /// <summary>
    /// Initializes all tiles in the grid
    /// </summary>
    private void InitializeGrid()
    {
        // Generate tiles for all positions
        foreach (Zone zone in Enum.GetValues(typeof(Zone)))
        {
            foreach (Row row in Enum.GetValues(typeof(Row)))
            {
                for (int col = 0; col < Columns; col++)
                {
                    var pos = new GridPosition(zone, row, col);
                    Tiles[pos] = new BattlefieldTile(pos);
                }
            }
        }
    }

    /// <summary>
    /// Gets the tile at the specified grid position
    /// </summary>
    public BattlefieldTile? GetTile(GridPosition position)
    {
        return Tiles.TryGetValue(position, out var tile) ? tile : null;
    }

    /// <summary>
    /// Gets all tiles adjacent to the specified position (same zone/row, ±1 column)
    /// </summary>
    public List<BattlefieldTile> GetAdjacentTiles(GridPosition position)
    {
        var adjacent = new List<BattlefieldTile>();

        // Check left column
        if (position.Column > 0)
        {
            var leftPos = new GridPosition(position.Zone, position.Row, position.Column - 1, position.Elevation);
            var leftTile = GetTile(leftPos);
            if (leftTile != null)
                adjacent.Add(leftTile);
        }

        // Check right column
        if (position.Column < Columns - 1)
        {
            var rightPos = new GridPosition(position.Zone, position.Row, position.Column + 1, position.Elevation);
            var rightTile = GetTile(rightPos);
            if (rightTile != null)
                adjacent.Add(rightTile);
        }

        return adjacent;
    }

    /// <summary>
    /// Gets all occupied tiles in the specified zone
    /// </summary>
    public List<BattlefieldTile> GetOccupiedTilesInZone(Zone zone)
    {
        return Tiles.Values
            .Where(t => t.Position.Zone == zone && t.IsOccupied)
            .ToList();
    }

    /// <summary>
    /// Gets all tiles with active traps
    /// </summary>
    public List<BattlefieldTile> GetTilesWithTraps()
    {
        return Tiles.Values
            .Where(t => t.Traps.Count > 0)
            .ToList();
    }

    /// <summary>
    /// Gets all glitched tiles
    /// </summary>
    public List<BattlefieldTile> GetGlitchedTiles()
    {
        return Tiles.Values
            .Where(t => t.Type == TileType.Glitched)
            .ToList();
    }

    /// <summary>
    /// Checks if a position is valid within the grid bounds
    /// </summary>
    public bool IsValidPosition(GridPosition position)
    {
        return position.Column >= 0
            && position.Column < Columns
            && position.Elevation >= 0;
    }

    /// <summary>
    /// Gets the distance in columns between two positions in the same zone/row
    /// </summary>
    public int GetColumnDistance(GridPosition from, GridPosition to)
    {
        return Math.Abs(to.Column - from.Column);
    }

    public override string ToString()
    {
        return $"BattlefieldGrid ({Columns} columns, {Tiles.Count} tiles)";
    }
}
