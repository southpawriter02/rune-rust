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

        // TODO v0.20: Integrate with procedural generation
        // - Place cover based on room type (ruins have more physical cover)
        // - Place glitched tiles based on Corruption level
        // - Place high ground tiles in specific biomes

        _log.Debug("Environmental features applied to grid: Room={RoomId}", room.Id);
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
