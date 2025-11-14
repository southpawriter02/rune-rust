using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20: Service responsible for initializing and configuring the tactical combat grid
/// </summary>
public class GridInitializationService
{
    private static readonly ILogger _log = Log.ForContext<GridInitializationService>();

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

        // v0.20.2: Generate cover
        GenerateCover(grid, room);

        // TODO v0.20: Additional environmental features
        // - Place glitched tiles based on Corruption level
        // - Place high ground tiles in specific biomes

        _log.Debug("Environmental features applied to grid: Room={RoomId}", room.Id);
    }

    /// <summary>
    /// Generates cover on the battlefield based on room archetype
    /// v0.20.2: Cover System implementation
    /// </summary>
    private void GenerateCover(BattlefieldGrid grid, Room room)
    {
        // Calculate number of cover pieces based on grid size
        int coverCount = CalculateCoverCount(grid.Columns);

        _log.Information("Generating cover: Columns={Columns}, CoverCount={Count}, RoomArchetype={Archetype}",
            grid.Columns, coverCount, room.Archetype);

        var random = new Random();
        var placedCover = 0;

        // Attempt to place cover pieces
        for (int attempt = 0; attempt < coverCount * 3 && placedCover < coverCount; attempt++)
        {
            var coverType = SelectCoverType(room, random);
            var position = SelectCoverPosition(grid, random);

            if (position == null)
            {
                _log.Warning("Failed to find valid cover position: Attempt={Attempt}", attempt);
                continue;
            }

            var tile = grid.GetTile(position.Value);
            if (tile != null && !tile.IsOccupied && tile.Cover == CoverType.None)
            {
                PlaceCover(tile, coverType, random);
                placedCover++;
            }
        }

        _log.Information("Cover generation complete: PlacedCover={PlacedCount}, TargetCount={TargetCount}",
            placedCover, coverCount);
    }

    /// <summary>
    /// Calculates number of cover pieces based on grid size
    /// Formula: 1 cover per 2 columns, minimum 1
    /// </summary>
    private int CalculateCoverCount(int columns)
    {
        return Math.Max(1, columns / 2);
    }

    /// <summary>
    /// Selects cover type based on room archetype and biome
    /// </summary>
    private CoverType SelectCoverType(Room room, Random random)
    {
        var roll = random.NextDouble();

        // Boss arenas have more metaphysical cover (reality anchors)
        if (room.IsBossRoom)
        {
            if (roll < 0.50) return CoverType.Physical;      // 50% physical
            if (roll < 0.85) return CoverType.Metaphysical;  // 35% metaphysical
            return CoverType.Both;                           // 15% both
        }

        // Standard rooms favor physical cover
        if (roll < 0.70) return CoverType.Physical;      // 70% physical (pillars, crates)
        if (roll < 0.85) return CoverType.Metaphysical;  // 15% metaphysical (Runic Anchors)
        return CoverType.Both;                           // 5% both (rare)
    }

    /// <summary>
    /// Selects a random position for cover placement
    /// Prefers back rows for tactical positioning
    /// </summary>
    private GridPosition? SelectCoverPosition(BattlefieldGrid grid, Random random)
    {
        // Try to place in back rows first (60% chance)
        var preferBackRow = random.NextDouble() < 0.6;
        var row = preferBackRow ? Row.Back : Row.Front;

        // Randomly select zone and column
        var zone = random.Next(2) == 0 ? Zone.Player : Zone.Enemy;
        var column = random.Next(grid.Columns);

        var position = new GridPosition(zone, row, column, elevation: 0);

        // Validate position exists
        if (grid.GetTile(position) != null)
        {
            return position;
        }

        return null;
    }

    /// <summary>
    /// Places cover on a tile with appropriate properties
    /// </summary>
    private void PlaceCover(BattlefieldTile tile, CoverType coverType, Random random)
    {
        tile.Cover = coverType;

        // Set health for physical cover
        if (coverType == CoverType.Physical || coverType == CoverType.Both)
        {
            tile.CoverHealth = 20;  // Standard cover HP
            tile.CoverDescription = SelectPhysicalCoverDescription(random);
        }

        // Set description for metaphysical cover
        if (coverType == CoverType.Metaphysical)
        {
            tile.CoverDescription = "Runic Anchor";
        }
        else if (coverType == CoverType.Both)
        {
            tile.CoverDescription = "Fortified Anchor";  // Both types
        }

        _log.Information("Cover placed: Position={Position}, Type={CoverType}, Description={Description}, HP={HP}",
            tile.Position, coverType, tile.CoverDescription, tile.CoverHealth);
    }

    /// <summary>
    /// Selects a random description for physical cover
    /// </summary>
    private string SelectPhysicalCoverDescription(Random random)
    {
        var descriptions = new[]
        {
            "Pillar",
            "Crate",
            "Debris",
            "Wall Section",
            "Console",
            "Rubble",
            "Barricade"
        };

        return descriptions[random.Next(descriptions.Length)];
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
