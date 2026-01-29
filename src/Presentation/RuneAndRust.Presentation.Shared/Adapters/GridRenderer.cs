using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Shared.DTOs;
using RuneAndRust.Presentation.Shared.Interfaces;

namespace RuneAndRust.Presentation.Shared.Adapters;

/// <summary>
/// Renders the combat grid as ASCII art with terrain and cover support.
/// </summary>
/// <remarks>
/// Provides both full grid display with box-drawing characters and compact display.
/// Display priority (highest to lowest):
/// <list type="bullet">
/// <item><description>@ = Player</description></item>
/// <item><description>M = Monster</description></item>
/// <item><description>Cover symbol (if present)</description></item>
/// <item><description>Terrain symbol (based on type)</description></item>
/// </list>
/// </remarks>
public class GridRenderer : IGridRenderer
{
    private const char PlayerChar = '@';
    private const char MonsterChar = 'M';
    private const char EmptyChar = '.';
    private const char WallChar = '#';

    private readonly ILogger<GridRenderer>? _logger;
    private readonly ICoverService? _coverService;
    private readonly ITerrainService? _terrainService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GridRenderer"/> class.
    /// </summary>
    /// <param name="logger">Optional logger.</param>
    /// <param name="coverService">Optional cover service for cover display.</param>
    /// <param name="terrainService">Optional terrain service for terrain display.</param>
    public GridRenderer(
        ILogger<GridRenderer>? logger = null,
        ICoverService? coverService = null,
        ITerrainService? terrainService = null)
    {
        _logger = logger;
        _coverService = coverService;
        _terrainService = terrainService;
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
                var pos = new GridPosition(x, y);
                var cell = grid.GetCell(x, y);
                var cover = options.ShowCover ? grid.GetCover(pos) : null;
                var ch = RenderCell(cell, cover, options);
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
            sb.AppendLine(RenderLegend(grid, options));
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

        // Collect legend items for compact display
        var legendLines = GetCompactLegendLines(grid, options);

        // Grid rows with optional legend on right
        for (int y = 0; y < grid.Height; y++)
        {
            // Row number
            if (options.ShowCoordinates)
                sb.Append($"{y + 1} ");
            
            // Cells
            for (int x = 0; x < grid.Width; x++)
            {
                var pos = new GridPosition(x, y);
                var cell = grid.GetCell(x, y);
                var cover = options.ShowCover ? grid.GetCover(pos) : null;
                var ch = RenderCell(cell, cover, options);
                sb.Append($" {ch}");
            }

            // Legend on right side
            if (options.ShowLegend && y < legendLines.Count)
            {
                sb.Append($"     {legendLines[y]}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Renders a single cell with display priority: entity > cover > terrain.
    /// </summary>
    /// <param name="cell">The cell to render.</param>
    /// <param name="cover">Cover object at this position, if any.</param>
    /// <param name="options">Render options.</param>
    /// <returns>The character to display.</returns>
    public char RenderCell(GridCell? cell, CoverObject? cover, GridRenderOptions options)
    {
        if (cell == null)
            return EmptyChar;

        // Priority 1: Entity (highest)
        if (cell.IsOccupied)
            return cell.IsPlayerOccupied ? PlayerChar : MonsterChar;

        // Priority 2: Cover
        if (cover != null && !cover.IsDestroyed && options.ShowCover)
            return cover.DisplayChar;

        // Priority 3: Terrain
        if (options.ShowTerrain)
            return GetTerrainChar(cell);

        // Fallback: basic cell display
        return cell.GetDisplayChar();
    }

    /// <summary>
    /// Gets the display character for terrain type.
    /// </summary>
    /// <param name="cell">The cell to get terrain for.</param>
    /// <returns>Character representing the terrain.</returns>
    public char GetTerrainChar(GridCell cell)
    {
        // Check for terrain definition first
        if (!string.IsNullOrEmpty(cell.TerrainDefinitionId) && _terrainService != null)
        {
            var def = _terrainService.GetTerrainDefinition(cell.Position);
            if (def != null)
                return def.DisplayChar;
        }

        // Use type-based default
        return cell.TerrainType switch
        {
            TerrainType.Normal => '.',
            TerrainType.Difficult => '~',
            TerrainType.Impassable => '#',
            TerrainType.Hazardous => '▲',
            _ => '.'
        };
    }

    /// <inheritdoc/>
    public string RenderLegend(CombatGrid grid)
    {
        return RenderLegend(grid, GridRenderOptions.Default);
    }

    /// <summary>
    /// Renders dynamic legend based on grid content.
    /// </summary>
    /// <param name="grid">The combat grid.</param>
    /// <param name="options">Render options.</param>
    /// <returns>Legend string.</returns>
    public string RenderLegend(CombatGrid grid, GridRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(grid);

        var sb = new StringBuilder();
        sb.AppendLine("Legend:");

        // Player position
        var playerPos = GetPlayerPosition(grid);
        var posStr = playerPos.HasValue ? $" ({playerPos.Value})" : "";
        sb.AppendLine($"  @ = You{posStr}");
        sb.AppendLine("  M = Monster");
        sb.AppendLine("  . = Empty");
        sb.AppendLine("  # = Wall");

        // Add unique terrain types
        if (options.ShowTerrain)
        {
            var terrainTypes = GetUniqueTerrainTypes(grid);
            foreach (var def in terrainTypes)
            {
                var info = GetTerrainInfo(def);
                sb.AppendLine($"  {def.DisplayChar} = {def.Name}{info}");
            }
        }

        // Add unique cover types
        if (options.ShowCover)
        {
            var coverTypes = GetUniqueCoverTypes(grid);
            foreach (var cover in coverTypes)
            {
                var info = cover.CoverType == CoverType.Full
                    ? " (full cover)"
                    : $" (+{cover.DefenseBonus} def)";
                sb.AppendLine($"  {cover.DisplayChar} = {cover.Name}{info}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets unique terrain definitions present on the grid.
    /// </summary>
    public IEnumerable<TerrainDefinition> GetUniqueTerrainTypes(CombatGrid grid)
    {
        if (_terrainService == null)
            yield break;

        var seen = new HashSet<string>();
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                var cell = grid.GetCell(x, y);
                if (cell == null || string.IsNullOrEmpty(cell.TerrainDefinitionId))
                    continue;

                var def = _terrainService.GetTerrainDefinition(cell.Position);
                if (def != null && def.Type != TerrainType.Normal && seen.Add(def.Id))
                    yield return def;
            }
        }
    }

    /// <summary>
    /// Gets unique cover objects present on the grid.
    /// </summary>
    public IEnumerable<CoverObject> GetUniqueCoverTypes(CombatGrid grid)
    {
        var seen = new HashSet<string>();
        foreach (var (_, cover) in grid.CoverObjects)
        {
            if (!cover.IsDestroyed && seen.Add(cover.DefinitionId))
                yield return cover;
        }
    }

    /// <inheritdoc/>
    public string RenderCombatantList(CombatGrid grid, Player player, IEnumerable<Monster> monsters)
    {
        return RenderCombatantList(grid, player, monsters, null);
    }

    /// <summary>
    /// Renders combatant list with cover status.
    /// </summary>
    /// <param name="grid">The combat grid.</param>
    /// <param name="player">The player.</param>
    /// <param name="monsters">The monsters.</param>
    /// <param name="playerIdForCover">Player ID for cover calculations.</param>
    /// <returns>Combatant list string.</returns>
    public string RenderCombatantList(CombatGrid grid, Player player, IEnumerable<Monster> monsters, Guid? playerIdForCover)
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

        // Monsters with cover status
        foreach (var monster in monsters.Where(m => m.IsAlive))
        {
            var monsterPos = monster.CombatGridPosition;
            var monsterPosStr = monsterPos.HasValue ? $"({monsterPos})" : "";
            var coverInfo = "";
            
            // Get cover status if we have player ID and cover service
            if (playerIdForCover.HasValue && _coverService != null && monsterPos.HasValue)
            {
                coverInfo = GetCoverStatusForDisplay(playerIdForCover.Value, monster.Id);
            }

            sb.AppendLine($"  M {monster.Name} {monsterPosStr} - HP: {monster.Health}/{monster.MaxHealth}{coverInfo}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the cover status display string for a target.
    /// </summary>
    private string GetCoverStatusForDisplay(Guid attackerId, Guid targetId)
    {
        if (_coverService == null)
            return "";

        var cover = _coverService.GetCoverBetween(attackerId, targetId);
        if (cover.CoverType == CoverType.None)
            return "";
        if (cover.CoverType == CoverType.Full)
            return $" [Behind {cover.CoverObject?.Name}: FULL COVER]";
        return $" [Behind {cover.CoverObject?.Name}: +{cover.DefenseBonus} def]";
    }

    /// <summary>
    /// Gets terrain info for legend display.
    /// </summary>
    private static string GetTerrainInfo(TerrainDefinition def)
    {
        if (def.Type == TerrainType.Hazardous && def.DealsDamage)
            return $" ({def.DamageOnEntry})";
        if (def.MovementCostMultiplier > 1)
            return $" ({def.MovementCostMultiplier}x move)";
        return "";
    }

    /// <summary>
    /// Gets compact legend lines for right-side display.
    /// </summary>
    private List<string> GetCompactLegendLines(CombatGrid grid, GridRenderOptions options)
    {
        var lines = new List<string>
        {
            "Legend:",
            "  @ = You    M = Monster"
        };

        // Add common terrain/cover if present
        if (options.ShowTerrain)
        {
            lines.Add("  . = Floor  ~ = Difficult");
            lines.Add("  ▲ = Hazard  # = Wall");
        }

        if (options.ShowCover && grid.CoverObjects.Count > 0)
        {
            lines.Add("  Cover provides defense bonus");
        }

        return lines;
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
