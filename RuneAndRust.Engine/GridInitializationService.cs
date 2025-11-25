using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20: Service responsible for initializing and configuring the tactical combat grid
/// </summary>
public class GridInitializationService
{
    private static readonly ILogger _log = Log.ForContext<GridInitializationService>();
    private readonly CoverService _coverService; // v0.20.2
    private readonly Random _random;

    public GridInitializationService()
    {
        _coverService = new CoverService();
        _random = new Random();
    }

    /// <summary>
    /// Initializes a battlefield grid for combat based on the number of combatants
    /// </summary>
    public BattlefieldGrid InitializeGrid(PlayerCharacter player, List<Enemy> enemies)
    {
        // Calculate grid columns: max(party size, enemy count) + 2 for spacing
        // Currently only single player, but designed for future party support
        int partySize = 1;
        int enemyCount = enemies.Count;
        int columns = Math.Max(partySize, enemyCount) + 2;

        // Ensure minimum of 3 columns for tactical positioning
        columns = Math.Max(columns, 3);

        _log.Information("Initializing battlefield grid: Columns={Columns}, Enemies={EnemyCount}",
            columns, enemyCount);

        var grid = new BattlefieldGrid(columns);

        // Place player in starting position (center back row of Player zone)
        PlacePlayer(player, grid);

        // Place enemies in their starting positions
        PlaceEnemies(enemies, grid);

        return grid;
    }

    /// <summary>
    /// Places the player in their default starting position
    /// Default: Center column, Back row, Player zone
    /// </summary>
    private void PlacePlayer(PlayerCharacter player, BattlefieldGrid grid)
    {
        int centerColumn = grid.Columns / 2;
        var playerPosition = new GridPosition(Zone.Player, Row.Back, centerColumn, elevation: 0);

        player.Position = playerPosition;

        var tile = grid.GetTile(playerPosition);
        if (tile != null)
        {
            tile.IsOccupied = true;
            tile.OccupantId = "player";

            _log.Debug("Player placed: Position={Position}", playerPosition);
        }
    }

    /// <summary>
    /// Places enemies on the battlefield grid
    /// Distributes enemies across Front row of Enemy zone
    /// </summary>
    private void PlaceEnemies(List<Enemy> enemies, BattlefieldGrid grid)
    {
        int enemyCount = enemies.Count;

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];

            // Distribute enemies evenly across available columns
            // Formula: spread enemies across columns, starting from left
            int column = CalculateEnemyColumn(i, enemyCount, grid.Columns);

            // Default: Front row, Enemy zone
            // Boss enemies might start in Back row for thematic reasons
            Row row = enemy.IsBoss && enemyCount > 1 ? Row.Back : Row.Front;

            var enemyPosition = new GridPosition(Zone.Enemy, row, column, elevation: 0);
            enemy.Position = enemyPosition;

            var tile = grid.GetTile(enemyPosition);
            if (tile != null)
            {
                tile.IsOccupied = true;
                tile.OccupantId = enemy.Id;

                _log.Debug("Enemy placed: Enemy={EnemyName}, Position={Position}",
                    enemy.Name, enemyPosition);
            }
        }
    }

    /// <summary>
    /// Calculates the column position for an enemy based on index and total count
    /// Distributes enemies evenly across the grid
    /// </summary>
    private int CalculateEnemyColumn(int enemyIndex, int totalEnemies, int totalColumns)
    {
        if (totalEnemies == 1)
        {
            // Single enemy: center column
            return totalColumns / 2;
        }
        else if (totalEnemies == 2)
        {
            // Two enemies: left and right
            return enemyIndex == 0 ? totalColumns / 3 : (2 * totalColumns) / 3;
        }
        else
        {
            // Three or more enemies: distribute evenly
            int spacing = totalColumns / (totalEnemies + 1);
            return spacing * (enemyIndex + 1);
        }
    }

    /// <summary>
    /// Applies environmental features to grid tiles (cover, high ground, glitched tiles)
    /// Called after grid initialization to add tactical elements
    /// </summary>
    public void ApplyEnvironmentalFeatures(BattlefieldGrid grid, Room? room)
    {
        if (room == null)
            return;

        // v0.20.2: Generate procedural cover
        GenerateCover(grid, room);

        // v0.20.3: Generate glitched tiles
        GenerateGlitchedTiles(grid, room);

        // TODO v0.21+: Integrate with procedural generation
        // - Place high ground tiles in specific biomes

        _log.Debug("Environmental features applied to grid: Room={RoomId}", room.Id);
    }

    /// <summary>
    /// v0.20.2: Generates procedural cover placement on the battlefield
    /// Cover count: 1 piece per 2 columns (minimum 1)
    /// Distribution: 70% Physical, 15% Metaphysical, 5% Both (for The Roots biome)
    /// </summary>
    private void GenerateCover(BattlefieldGrid grid, Room room)
    {
        int coverCount = CalculateCoverCount(grid.Columns);

        _log.Information("Generating cover: Columns={Columns}, CoverCount={Count}, Room={RoomId}",
            grid.Columns, coverCount, room.Id);

        for (int i = 0; i < coverCount; i++)
        {
            var coverType = SelectCoverType();
            var position = SelectCoverPosition(grid);

            if (position == null)
            {
                _log.Warning("Failed to place cover {Index}: no valid positions remaining", i);
                break;
            }

            var tile = grid.GetTile(position.Value);
            if (tile != null)
            {
                _coverService.PlaceCover(tile, coverType);

                _log.Debug("Cover placed: Position={Position}, Type={CoverType}, Description={Description}",
                    position, coverType, tile.CoverDescription);
            }
        }
    }

    /// <summary>
    /// Calculates number of cover pieces based on grid size
    /// Formula: 1 cover per 2 columns (minimum 1)
    /// </summary>
    private int CalculateCoverCount(int columns)
    {
        return Math.Max(1, columns / 2);
    }

    /// <summary>
    /// Selects a cover type based on biome-specific distributions
    /// The Roots biome: 70% Physical, 25% Metaphysical, 5% Both
    /// </summary>
    private CoverType SelectCoverType()
    {
        var roll = _random.NextDouble();

        // TODO: Add biome parameter and biome-specific distributions
        // For now, using The Roots distribution as default
        if (roll < 0.70)
            return CoverType.Physical;      // 70% physical (pillars, crates)
        else if (roll < 0.95)
            return CoverType.Metaphysical;  // 25% metaphysical (Runic Anchors)
        else
            return CoverType.Both;          // 5% both (rare sanctified locations)
    }

    /// <summary>
    /// Selects a random valid position for cover placement
    /// Avoids occupied tiles and prefers back rows for tactical gameplay
    /// </summary>
    private GridPosition? SelectCoverPosition(BattlefieldGrid grid)
    {
        var availablePositions = new List<GridPosition>();

        // Collect all valid, unoccupied positions
        // Prefer back rows (60% chance) for more tactical gameplay
        foreach (var tile in grid.Tiles.Values)
        {
            if (!tile.IsOccupied && tile.Cover == CoverType.None)
            {
                // Weight back rows more heavily
                if (tile.Position.Row == Row.Back)
                {
                    availablePositions.Add(tile.Position);
                    availablePositions.Add(tile.Position); // Add twice for 2x weight
                }
                else
                {
                    availablePositions.Add(tile.Position);
                }
            }
        }

        if (availablePositions.Count == 0)
        {
            return null;
        }

        // Select random position
        int index = _random.Next(availablePositions.Count);
        return availablePositions[index];
    }

    /// <summary>
    /// v0.20.3: Generates procedural glitched tile placement on the battlefield
    /// Glitch count: 1 glitch per 3 columns (minimum 1), biome-adjusted
    /// </summary>
    private void GenerateGlitchedTiles(BattlefieldGrid grid, Room room)
    {
        int glitchCount = CalculateGlitchCount(grid.Columns, room.Biome);

        _log.Information("Generating glitched tiles: Columns={Columns}, Biome={Biome}, GlitchCount={Count}, Room={RoomId}",
            grid.Columns, room.Biome, glitchCount, room.Id);

        for (int i = 0; i < glitchCount; i++)
        {
            var glitchType = SelectGlitchType(room.Biome);
            var severity = SelectSeverity(room.DifficultyLevel);
            var position = SelectGlitchPosition(grid);

            if (position == null)
            {
                _log.Warning("Failed to place glitched tile {Index}: no valid positions", i);
                break;
            }

            PlaceGlitch(grid, position.Value, glitchType, severity);
        }
    }

    /// <summary>
    /// Calculates number of glitched tiles based on grid size and biome
    /// Formula: 1 glitch per 3 columns (minimum 1), with biome modifiers
    /// </summary>
    private int CalculateGlitchCount(int columns, string biome)
    {
        // Base: 1 glitch per 3 columns
        int baseCount = Math.Max(1, columns / 3);

        // Biome modifier
        return biome switch
        {
            "TheRoots" => baseCount,           // Standard
            "Muspelheim" => baseCount + 1,     // More hazards (fire biome)
            "Niflheim" => baseCount + 1,       // More hazards (ice biome)
            _ => baseCount
        };
    }

    /// <summary>
    /// Selects a glitch type based on biome-specific distributions
    /// </summary>
    private Core.GlitchType SelectGlitchType(string biome)
    {
        var roll = _random.NextDouble();

        // Biome-specific distributions
        switch (biome)
        {
            case "TheRoots":
                if (roll < 0.40) return Core.GlitchType.Looping;        // 40% Looping (spatial)
                if (roll < 0.70) return Core.GlitchType.Flickering;     // 30% Flickering
                return Core.GlitchType.InvertedGravity;                 // 30% Gravity

            case "Muspelheim":
                if (roll < 0.50) return Core.GlitchType.Flickering;     // 50% Flickering (unstable)
                if (roll < 0.75) return Core.GlitchType.InvertedGravity; // 25% Gravity
                return Core.GlitchType.Looping;                         // 25% Looping

            case "Niflheim":
                if (roll < 0.45) return Core.GlitchType.InvertedGravity; // 45% Gravity (frozen air)
                if (roll < 0.75) return Core.GlitchType.Flickering;     // 30% Flickering
                return Core.GlitchType.Looping;                         // 25% Looping

            default:
                // Default distribution
                if (roll < 0.33) return Core.GlitchType.Flickering;
                if (roll < 0.66) return Core.GlitchType.InvertedGravity;
                return Core.GlitchType.Looping;
        }
    }

    /// <summary>
    /// Selects glitch severity based on room difficulty level
    /// Difficulty 1-3 = Severity 1 (DC 12)
    /// Difficulty 4-6 = Severity 2 (DC 14)
    /// Difficulty 7+ = Severity 3 (DC 16)
    /// </summary>
    private int SelectSeverity(int difficultyLevel)
    {
        if (difficultyLevel <= 3) return 1;
        if (difficultyLevel <= 6) return 2;
        return 3;
    }

    /// <summary>
    /// Selects a random valid position for glitched tile placement
    /// Avoids occupied tiles and tiles with cover
    /// </summary>
    private GridPosition? SelectGlitchPosition(BattlefieldGrid grid)
    {
        var availablePositions = grid.Tiles.Values
            .Where(t => !t.IsOccupied && t.Cover == CoverType.None && t.Type == TileType.Normal)
            .Select(t => t.Position)
            .ToList();

        if (availablePositions.Count == 0)
            return null;

        return availablePositions[_random.Next(availablePositions.Count)];
    }

    /// <summary>
    /// Places a glitched tile on the grid
    /// </summary>
    private void PlaceGlitch(BattlefieldGrid grid, GridPosition position, Core.GlitchType glitchType, int severity)
    {
        var tile = grid.GetTile(position);
        if (tile == null)
            return;

        tile.Type = TileType.Glitched;
        tile.GlitchType = glitchType;
        tile.GlitchSeverity = severity;

        int dc = 10 + (severity * 2);

        _log.Information("Glitched tile placed: Position={Position}, Type={GlitchType}, Severity={Severity}, DC={DC}",
            position, glitchType, severity, dc);
    }

    /// <summary>
    /// Resets positions at the end of combat
    /// </summary>
    public void ResetCombatantPositions(PlayerCharacter player, List<Enemy> enemies)
    {
        player.Position = null;
        player.KineticEnergy = 0;
        player.TilesMovedThisTurn = 0;
        player.HasMovedThisTurn = false;

        foreach (var enemy in enemies)
        {
            enemy.Position = null;
            enemy.KineticEnergy = 0;
            enemy.TilesMovedThisTurn = 0;
            enemy.HasMovedThisTurn = false;
        }

        _log.Debug("Combat grid positions reset");
    }
}
