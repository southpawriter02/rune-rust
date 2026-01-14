using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Shared.DTOs;
using RuneAndRust.Presentation.Shared.Interfaces;

namespace RuneAndRust.Presentation.Shared.Adapters;

/// <summary>
/// Renders the combat grid as ASCII art.
/// </summary>
/// <remarks>
/// Provides both full grid display with box-drawing characters and compact display.
/// Symbol mapping:
/// <list type="bullet">
/// <item><description>@ = Player</description></item>
/// <item><description>M = Monster</description></item>
/// <item><description>. = Empty cell</description></item>
/// <item><description># = Wall/impassable</description></item>
/// </list>
/// </remarks>
public class GridRenderer : IGridRenderer
{
    private const char PlayerChar = '@';
    private const char MonsterChar = 'M';
    private const char EmptyChar = '.';
    private const char WallChar = '#';

    private readonly ILogger<GridRenderer>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GridRenderer"/> class.
    /// </summary>
    /// <param name="logger">Optional logger.</param>
    public GridRenderer(ILogger<GridRenderer>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string RenderGrid(CombatGrid grid, GridRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(grid);
        options ??= GridRenderOptions.Default;

        _logger?.LogDebug("Rendering grid {Width}x{Height}, compact={Compact}", 
            grid.Width, grid.Height, options.Compact);

        if (options.Compact)
            return RenderCompactGrid(grid, options);

        var sb = new StringBuilder();

        // Header with optional turn number
        if (options.TurnNumber.HasValue)
        {
            var turnPadding = new string(' ', Math.Max(0, 30 - "Combat Grid".Length));
            sb.AppendLine($"Combat Grid{turnPadding}[Turn {options.TurnNumber}]");
        }
        else
        {
            sb.AppendLine("Combat Grid");
        }
        sb.AppendLine(new string('=', 47));
        sb.AppendLine();

        // Column headers (A, B, C, ...)
        if (options.ShowCoordinates)
        {
            sb.Append("     ");
            for (int x = 0; x < grid.Width; x++)
                sb.Append($" {(char)('A' + x)}  ");
            sb.AppendLine();
        }

        // Top border
        sb.Append("   ");
        sb.AppendLine("+" + string.Join("+", Enumerable.Repeat("---", grid.Width)) + "+");

        // Grid rows
        for (int y = 0; y < grid.Height; y++)
        {
            // Row number
            if (options.ShowCoordinates)
                sb.Append($" {y + 1} ");
            else
                sb.Append("   ");

            // Cells
            sb.Append("|");
            for (int x = 0; x < grid.Width; x++)
            {
                var cell = grid.GetCell(x, y);
                var ch = GetCellChar(cell);
                sb.Append($" {ch} |");
            }
            sb.AppendLine();

            // Row border
            sb.Append("   ");
            sb.AppendLine("+" + string.Join("+", Enumerable.Repeat("---", grid.Width)) + "+");
        }

        // Legend
        if (options.ShowLegend)
        {
            sb.AppendLine();
            sb.AppendLine(RenderLegend(grid));
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderCompactGrid(CombatGrid grid, GridRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(grid);
        options ??= GridRenderOptions.CompactDefault;

        var sb = new StringBuilder();

        // Column headers
        if (options.ShowCoordinates)
        {
            sb.Append("  ");
            for (int x = 0; x < grid.Width; x++)
                sb.Append($" {(char)('A' + x)}");
            sb.AppendLine();
        }

        // Grid rows with optional legend on right
        for (int y = 0; y < grid.Height; y++)
        {
            // Row number
            if (options.ShowCoordinates)
                sb.Append($"{y + 1} ");
            
            // Cells
            for (int x = 0; x < grid.Width; x++)
            {
                var cell = grid.GetCell(x, y);
                var ch = GetCellChar(cell);
                sb.Append($" {ch}");
            }

            // Legend on right side for first few rows
            if (options.ShowLegend)
            {
                switch (y)
                {
                    case 1:
                        sb.Append("     Legend:");
                        break;
                    case 2:
                        sb.Append("       @ = You");
                        break;
                    case 3:
                        sb.Append("       M = Monster");
                        break;
                    case 4:
                        sb.Append("       . = Empty");
                        break;
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderLegend(CombatGrid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        var playerPos = GetPlayerPosition(grid);
        var playerPosStr = playerPos.HasValue ? $" ({playerPos})" : "";
        return $"Legend: @ = You{playerPosStr}  M = Monster  . = Empty  # = Wall";
    }

    /// <inheritdoc/>
    public string RenderCombatantList(CombatGrid grid, Player player, IEnumerable<Monster> monsters)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(monsters);

        var sb = new StringBuilder();
        sb.AppendLine("Combatants:");

        // Player
        var playerPos = player.CombatGridPosition;
        var playerPosStr = playerPos.HasValue ? $"({playerPos})" : "";
        var playerMove = $"{player.GetDisplayMovementRemaining():0.#}/{player.MovementSpeed}";
        sb.AppendLine($"  @ You {playerPosStr} - HP: {player.Health}/{player.Stats.MaxHealth} - Move: {playerMove}");

        // Monsters
        foreach (var monster in monsters.Where(m => m.IsAlive))
        {
            var monsterPos = monster.CombatGridPosition;
            var monsterPosStr = monsterPos.HasValue ? $"({monsterPos})" : "";
            sb.AppendLine($"  M {monster.Name} {monsterPosStr} - HP: {monster.Health}/{monster.MaxHealth}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the display character for a cell.
    /// </summary>
    private static char GetCellChar(GridCell? cell)
    {
        if (cell == null)
            return EmptyChar;

        // Use the cell's built-in display method
        return cell.GetDisplayChar();
    }

    /// <summary>
    /// Finds the player's position on the grid.
    /// </summary>
    private static GridPosition? GetPlayerPosition(CombatGrid grid)
    {
        foreach (var (entityId, pos) in grid.EntityPositions)
        {
            var cell = grid.GetCell(pos);
            if (cell?.IsPlayerOccupied == true)
                return pos;
        }
        return null;
    }
}
