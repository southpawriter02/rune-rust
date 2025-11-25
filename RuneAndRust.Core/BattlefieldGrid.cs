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

    // Additional methods for backward compatibility

    /// <summary>
    /// Gets the cover level at a position (0-3)
    /// </summary>
    public int GetCoverLevel(GridPosition position)
    {
        var tile = GetTile(position);
        return tile?.CoverValue ?? 0;
    }

    /// <summary>
    /// Gets positions in a cone shape from origin in a direction
    /// </summary>
    public List<GridPosition> GetConeArea(GridPosition origin, Direction direction, int range)
    {
        var positions = new List<GridPosition>();
        // Simplified: get positions in the general direction
        for (int i = 1; i <= range; i++)
        {
            for (int offset = -i; offset <= i; offset++)
            {
                var newCol = origin.Column + offset;
                if (newCol >= 0 && newCol < Columns)
                {
                    var pos = new GridPosition(origin.Zone, origin.Row, newCol, origin.Elevation);
                    if (!positions.Contains(pos))
                        positions.Add(pos);
                }
            }
        }
        return positions;
    }

    /// <summary>
    /// Gets combatants at a position
    /// </summary>
    public List<object> GetCombatantsAtPosition(GridPosition position)
    {
        var tile = GetTile(position);
        return tile?.Occupants ?? new List<object>();
    }

    /// <summary>
    /// Gets direction from one position to another
    /// </summary>
    public Direction GetDirectionFrom(GridPosition from, GridPosition to)
    {
        if (to.Column > from.Column) return Direction.East;
        if (to.Column < from.Column) return Direction.West;
        if (to.Zone != from.Zone) return to.Zone == Zone.Enemy ? Direction.North : Direction.South;
        return Direction.North;
    }

    /// <summary>
    /// Gets position in a direction from origin
    /// </summary>
    public GridPosition? GetPositionInDirection(GridPosition origin, Direction direction, int distance = 1)
    {
        var newCol = origin.Column;
        var newRow = origin.Row;
        var newZone = origin.Zone;

        switch (direction)
        {
            case Direction.East:
                newCol += distance;
                break;
            case Direction.West:
                newCol -= distance;
                break;
            case Direction.North:
                // Move toward enemy zone or front row
                if (origin.Row == Row.Back)
                    newRow = Row.Front;
                else if (origin.Zone == Zone.Player)
                    newZone = Zone.Enemy;
                break;
            case Direction.South:
                // Move toward player zone or back row
                if (origin.Row == Row.Front)
                    newRow = Row.Back;
                else if (origin.Zone == Zone.Enemy)
                    newZone = Zone.Player;
                break;
        }

        if (newCol < 0 || newCol >= Columns) return null;
        return new GridPosition(newZone, newRow, newCol, origin.Elevation);
    }

    /// <summary>
    /// Checks if a position is blocked
    /// </summary>
    public bool IsBlocked(GridPosition position)
    {
        var tile = GetTile(position);
        return tile?.Type == TileType.Impassable || tile?.IsOccupied == true;
    }

    /// <summary>
    /// Gets all tiles within a radius
    /// </summary>
    public List<GridPosition> GetTilesInRadius(GridPosition center, int radius)
    {
        var positions = new List<GridPosition>();
        for (int colOffset = -radius; colOffset <= radius; colOffset++)
        {
            var newCol = center.Column + colOffset;
            if (newCol >= 0 && newCol < Columns)
            {
                var pos = new GridPosition(center.Zone, center.Row, newCol, center.Elevation);
                positions.Add(pos);
            }
        }
        return positions;
    }

    /// <summary>
    /// Gets area around a position
    /// </summary>
    public List<GridPosition> GetAreaAround(GridPosition center, int size)
    {
        return GetTilesInRadius(center, size);
    }

    /// <summary>
    /// Gets adjacent combatants
    /// </summary>
    public List<object> GetAdjacentCombatants(GridPosition position)
    {
        var combatants = new List<object>();
        foreach (var tile in GetAdjacentTiles(position))
        {
            combatants.AddRange(tile.Occupants);
        }
        return combatants;
    }

    /// <summary>
    /// Gets combatants within a radius
    /// </summary>
    public List<object> GetCombatantsInRadius(GridPosition center, int radius)
    {
        var combatants = new List<object>();
        foreach (var pos in GetTilesInRadius(center, radius))
        {
            var tile = GetTile(pos);
            if (tile != null)
                combatants.AddRange(tile.Occupants);
        }
        return combatants;
    }

    /// <summary>
    /// Gets distance between two positions
    /// </summary>
    public int GetDistance(GridPosition from, GridPosition to)
    {
        int colDist = Math.Abs(to.Column - from.Column);
        int zoneDist = from.Zone != to.Zone ? 2 : 0;
        int rowDist = from.Row != to.Row ? 1 : 0;
        return colDist + zoneDist + rowDist;
    }
}
