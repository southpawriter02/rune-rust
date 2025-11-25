namespace RuneAndRust.Core;

/// <summary>
/// Represents a direction on the tactical grid.
/// v0.32.2: Used for steam vent facing, forced movement, and conveyor belts.
/// </summary>
public enum GridDirection
{
    North,
    South,
    East,
    West,
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest
}

/// <summary>
/// Alias for BattlefieldTile used in terrain services.
/// Provides X/Y coordinate access and terrain feature queries.
/// v0.32.2: Extended grid tile for Jötunheim biome services.
/// </summary>
public class GridTile
{
    private readonly BattlefieldTile _tile;
    private readonly List<string> _terrainFeatures = new();
    private readonly List<string> _environmentalFeatures = new();

    public GridPosition Position => _tile.Position;
    public int X => Position.Column;
    public int Y => (int)Position.Row;

    public GridTile(BattlefieldTile tile)
    {
        _tile = tile;
    }

    public GridTile(GridPosition position)
    {
        _tile = new BattlefieldTile(position);
    }

    /// <summary>
    /// Check if this tile has a specific environmental feature.
    /// </summary>
    public bool HasEnvironmentalFeature(string featureName)
    {
        return _environmentalFeatures.Contains(featureName);
    }

    /// <summary>
    /// Check if this tile has a specific terrain type.
    /// </summary>
    public bool HasTerrain(string terrainName)
    {
        return _terrainFeatures.Contains(terrainName);
    }

    /// <summary>
    /// Add an environmental feature to this tile.
    /// </summary>
    public void AddEnvironmentalFeature(string featureName)
    {
        if (!_environmentalFeatures.Contains(featureName))
        {
            _environmentalFeatures.Add(featureName);
        }
    }

    /// <summary>
    /// Remove an environmental feature from this tile.
    /// </summary>
    public void RemoveEnvironmentalFeature(string featureName)
    {
        _environmentalFeatures.Remove(featureName);
    }

    /// <summary>
    /// Add terrain to this tile.
    /// </summary>
    public void AddTerrain(string terrainName)
    {
        if (!_terrainFeatures.Contains(terrainName))
        {
            _terrainFeatures.Add(terrainName);
        }
    }

    /// <summary>
    /// Remove terrain from this tile.
    /// </summary>
    public void RemoveTerrain(string terrainName)
    {
        _terrainFeatures.Remove(terrainName);
    }

    /// <summary>
    /// Get the underlying BattlefieldTile.
    /// </summary>
    public BattlefieldTile GetBattlefieldTile() => _tile;
}

/// <summary>
/// Represents the state of a simple 2D grid for terrain generation.
/// v0.32.4: Used for Jötun corpse terrain generation before conversion to BattlefieldGrid.
/// </summary>
public class GridState
{
    public int Width { get; }
    public int Height { get; }
    public GridTile[,] Tiles { get; }

    public GridState(int width, int height)
    {
        Width = width;
        Height = height;
        Tiles = new GridTile[width, height];

        // Initialize all tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var zone = y < height / 2 ? Zone.Player : Zone.Enemy;
                var row = y % 2 == 0 ? Row.Front : Row.Back;
                var position = new GridPosition(zone, row, x);
                Tiles[x, y] = new GridTile(position);
            }
        }
    }

    /// <summary>
    /// Get tile at specified coordinates.
    /// </summary>
    public GridTile? GetTile(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return Tiles[x, y];
        }
        return null;
    }

    /// <summary>
    /// Set terrain at specified coordinates.
    /// </summary>
    public void SetTerrain(int x, int y, string terrainType)
    {
        var tile = GetTile(x, y);
        tile?.AddTerrain(terrainType);
    }

    /// <summary>
    /// Set elevation at specified coordinates.
    /// </summary>
    public void SetElevation(int x, int y, int elevation)
    {
        var tile = GetTile(x, y);
        if (tile != null)
        {
            // Store elevation in position
            var pos = tile.Position;
            // GridPosition is immutable, so we'd need to recreate the tile
            // For now, store in terrain features
            tile.AddTerrain($"Elevation:{elevation}");
        }
    }

    /// <summary>
    /// Add environmental feature at specified coordinates.
    /// </summary>
    public void AddEnvironmentalFeature(int x, int y, string featureName)
    {
        var tile = GetTile(x, y);
        tile?.AddEnvironmentalFeature(featureName);
    }
}

/// <summary>
/// Character reference type for traversal services.
/// v0.34: Lightweight character reference for movement calculations.
/// </summary>
public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public GridPosition? Position { get; set; }
    public int MovementSpeed { get; set; } = 6;
    public bool CanFly { get; set; } = false;
    public bool CanClimb { get; set; } = true;
    public Dictionary<string, int> Attributes { get; set; } = new();

    /// <summary>
    /// Get attribute value by name.
    /// </summary>
    public int GetAttribute(string attributeName)
    {
        return Attributes.TryGetValue(attributeName, out var value) ? value : 0;
    }
}
