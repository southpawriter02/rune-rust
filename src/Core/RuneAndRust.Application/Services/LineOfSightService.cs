using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for line of sight calculations using Bresenham's line algorithm.
/// </summary>
/// <remarks>
/// LOS validation rules:
/// - Adjacent cells always have LOS (distance 1)
/// - Start/end positions are not checked for blocking
/// - Walls (!IsPassable) block LOS
/// - Occupied cells do NOT block LOS
/// </remarks>
public class LineOfSightService : ILineOfSightService
{
    private readonly ICombatGridService _gridService;
    private readonly ILogger<LineOfSightService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineOfSightService"/> class.
    /// </summary>
    /// <param name="gridService">The combat grid service.</param>
    /// <param name="logger">The logger.</param>
    public LineOfSightService(ICombatGridService gridService, ILogger<LineOfSightService> logger)
    {
        _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public LineOfSightResult HasLineOfSight(Guid fromEntityId, Guid toEntityId)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            return new LineOfSightResult(false, default, default, null, "No active combat grid.");

        var fromPos = grid.GetEntityPosition(fromEntityId);
        var toPos = grid.GetEntityPosition(toEntityId);

        if (!fromPos.HasValue || !toPos.HasValue)
            return new LineOfSightResult(false, default, default, null, "Entity not on grid.");

        return HasLineOfSight(fromPos.Value, toPos.Value);
    }

    /// <inheritdoc/>
    public LineOfSightResult HasLineOfSight(GridPosition from, GridPosition to)
    {
        _logger.LogDebug("LOS check from {From} to {To}", from, to);

        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            return new LineOfSightResult(false, from, to, null, "No active combat grid.");

        // Adjacent cells always have LOS (melee range bypass)
        if (from.IsAdjacentTo(to))
        {
            _logger.LogDebug("LOS: Adjacent positions, automatically clear");
            return new LineOfSightResult(true, from, to, null, "Adjacent - line of sight clear.");
        }

        // Check each cell along the line for blocking
        var blockingCell = GetFirstBlockingCell(from, to);

        if (blockingCell.HasValue)
        {
            _logger.LogDebug("LOS blocked from {From} to {To} by cell at {Block}",
                from, to, blockingCell.Value);

            return new LineOfSightResult(false, from, to, blockingCell.Value,
                $"Line of sight blocked by obstacle at {blockingCell.Value}.");
        }

        _logger.LogDebug("LOS clear from {From} to {To}", from, to);
        return new LineOfSightResult(true, from, to, null, "Line of sight clear.");
    }

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetLineCells(GridPosition from, GridPosition to)
    {
        // Bresenham's line algorithm
        int x0 = from.X, y0 = from.Y;
        int x1 = to.X, y1 = to.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            yield return new GridPosition(x0, y0);

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    /// <inheritdoc/>
    public GridPosition? GetFirstBlockingCell(GridPosition from, GridPosition to)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null) return null;

        foreach (var pos in GetLineCells(from, to))
        {
            // Skip start and end positions
            if (pos.Equals(from) || pos.Equals(to))
                continue;

            var cell = grid.GetCell(pos);
            if (cell != null && cell.EffectivelyBlocksLOS)
            {
                _logger.LogDebug("Blocking cell found at {Pos}", pos);
                return pos;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetVisiblePositions(GridPosition from, int maxRange)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null) yield break;

        _logger.LogDebug("Getting visible positions from {From} within range {Range}", from, maxRange);

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                var pos = new GridPosition(x, y);

                if (pos.Equals(from)) continue;
                if (from.DistanceTo(pos) > maxRange) continue;

                var losResult = HasLineOfSight(from, pos);
                if (losResult.HasLOS)
                    yield return pos;
            }
        }
    }
}
