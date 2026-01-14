using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for calculating area effect targeting and cell enumeration.
/// </summary>
public class AreaEffectService : IAreaEffectService
{
    private readonly ICombatGridService _gridService;
    private readonly ILineOfSightService _losService;
    private readonly ILogger<AreaEffectService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AreaEffectService"/>.
    /// </summary>
    public AreaEffectService(
        ICombatGridService gridService,
        ILineOfSightService losService,
        ILogger<AreaEffectService> logger)
    {
        _gridService = gridService;
        _losService = losService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetAffectedCells(
        AreaEffect areaEffect,
        GridPosition origin,
        GridPosition? targetPoint = null,
        FacingDirection? direction = null)
    {
        var grid = _gridService.GetActiveGrid();

        var cells = areaEffect.Shape switch
        {
            AreaEffectShape.Circle => GetCircleCells(targetPoint ?? origin, areaEffect.Radius),
            AreaEffectShape.Cone => GetConeCells(origin, direction ?? FacingDirection.North,
                areaEffect.Length, areaEffect.Width),
            AreaEffectShape.Line => GetLineCells(origin, targetPoint ?? origin, areaEffect.Width),
            AreaEffectShape.Square => GetSquareCells(targetPoint ?? origin, areaEffect.Radius * 2 + 1),
            _ => Enumerable.Empty<GridPosition>()
        };

        // Filter to valid grid positions (in bounds, but may be occupied)
        foreach (var cell in cells)
        {
            if (grid == null || grid.IsInBounds(cell))
                yield return cell;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetCircleCells(GridPosition center, int radius)
    {
        _logger.LogDebug("Calculating circle cells at {Center} with radius {Radius}", center, radius);

        for (var x = center.X - radius; x <= center.X + radius; x++)
        {
            for (var y = center.Y - radius; y <= center.Y + radius; y++)
            {
                var pos = new GridPosition(x, y);
                // Use Chebyshev distance for grid-based circle (square-ish)
                if (pos.DistanceTo(center) <= radius)
                    yield return pos;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetConeCells(
        GridPosition origin,
        FacingDirection direction,
        int length,
        int angleDegrees)
    {
        _logger.LogDebug("Calculating cone cells from {Origin} facing {Direction}, length {Length}, angle {Angle}",
            origin, direction, length, angleDegrees);

        var (dx, dy) = GetDirectionVector(direction);
        var halfAngleRad = (angleDegrees / 2.0) * Math.PI / 180.0;
        var yielded = new HashSet<GridPosition>();

        for (var dist = 1; dist <= length; dist++)
        {
            // Calculate spread at this distance
            var spread = (int)Math.Ceiling(dist * Math.Tan(halfAngleRad));

            var centerX = origin.X + dx * dist;
            var centerY = origin.Y + dy * dist;

            for (var s = -spread; s <= spread; s++)
            {
                int px, py;

                // Perpendicular spread based on direction
                if (dx == 0)
                {
                    // Vertical cone (N/S) - spread horizontally
                    px = centerX + s;
                    py = centerY;
                }
                else if (dy == 0)
                {
                    // Horizontal cone (E/W) - spread vertically
                    px = centerX;
                    py = centerY + s;
                }
                else
                {
                    // Diagonal cone - spread perpendicular to diagonal
                    px = centerX + s;
                    py = centerY - s * dx * dy;
                }

                var pos = new GridPosition(px, py);
                if (yielded.Add(pos))
                    yield return pos;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetLineCells(GridPosition origin, GridPosition target, int width)
    {
        _logger.LogDebug("Calculating line cells from {Origin} to {Target}, width {Width}",
            origin, target, width);

        // Use LOS service for basic line
        var lineCells = _losService.GetLineCells(origin, target).ToList();

        if (width <= 1)
        {
            foreach (var cell in lineCells)
                yield return cell;
            yield break;
        }

        // For wider lines, add perpendicular cells
        var halfWidth = width / 2;
        var dx = target.X - origin.X;
        var dy = target.Y - origin.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);

        if (length == 0)
        {
            yield return origin;
            yield break;
        }

        // Perpendicular direction
        var perpX = -dy / length;
        var perpY = dx / length;

        var yielded = new HashSet<GridPosition>();

        foreach (var cell in lineCells)
        {
            for (var w = -halfWidth; w <= halfWidth; w++)
            {
                var offsetX = (int)Math.Round(perpX * w);
                var offsetY = (int)Math.Round(perpY * w);
                var pos = new GridPosition(cell.X + offsetX, cell.Y + offsetY);

                if (yielded.Add(pos))
                    yield return pos;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetSquareCells(GridPosition center, int size)
    {
        _logger.LogDebug("Calculating square cells at {Center} with size {Size}", center, size);

        var halfSize = size / 2;

        for (var x = center.X - halfSize; x <= center.X + halfSize; x++)
        {
            for (var y = center.Y - halfSize; y <= center.Y + halfSize; y++)
            {
                yield return new GridPosition(x, y);
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<Guid> GetAffectedEntities(
        AreaEffect areaEffect,
        GridPosition origin,
        GridPosition? targetPoint = null,
        FacingDirection? direction = null,
        Guid? casterId = null)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null) yield break;

        // Determine if caster is player based on their cell
        var casterIsPlayer = false;
        if (casterId.HasValue)
        {
            var casterPos = grid.GetEntityPosition(casterId.Value);
            if (casterPos.HasValue)
            {
                var casterCell = grid.GetCell(casterPos.Value);
                casterIsPlayer = casterCell?.IsPlayerOccupied ?? false;
            }
        }

        var cells = GetAffectedCells(areaEffect, origin, targetPoint, direction);

        foreach (var cellPos in cells)
        {
            var gridCell = grid.GetCell(cellPos);
            if (gridCell == null || !gridCell.OccupantId.HasValue)
                continue;

            var occupantId = gridCell.OccupantId.Value;

            // Filter caster if not included
            if (!areaEffect.IncludesCaster && occupantId == casterId)
                continue;

            // Determine if occupant is ally or enemy
            var isPlayer = gridCell.IsPlayerOccupied;
            var isAlly = isPlayer == casterIsPlayer;

            // Filter based on ally/enemy settings
            if (isAlly && !areaEffect.AffectsAllies)
                continue;
            if (!isAlly && !areaEffect.AffectsEnemies)
                continue;

            yield return occupantId;
        }
    }

    /// <inheritdoc/>
    public AreaEffectPreview GetPreview(
        AreaEffect areaEffect,
        GridPosition origin,
        GridPosition? targetPoint = null,
        FacingDirection? direction = null,
        Guid? casterId = null)
    {
        var grid = _gridService.GetActiveGrid();
        var cells = GetAffectedCells(areaEffect, origin, targetPoint, direction).ToList();

        var enemies = new List<AffectedEntityInfo>();
        var allies = new List<AffectedEntityInfo>();

        if (grid != null)
        {
            // Determine if caster is player based on their cell
            var casterIsPlayer = false;
            if (casterId.HasValue)
            {
                var casterPos = grid.GetEntityPosition(casterId.Value);
                if (casterPos.HasValue)
                {
                    var casterCell = grid.GetCell(casterPos.Value);
                    casterIsPlayer = casterCell?.IsPlayerOccupied ?? false;
                }
            }

            foreach (var cellPos in cells)
            {
                var gridCell = grid.GetCell(cellPos);
                if (gridCell == null || !gridCell.OccupantId.HasValue)
                    continue;

                var occupantId = gridCell.OccupantId.Value;

                // Skip caster if not included
                if (!areaEffect.IncludesCaster && occupantId == casterId)
                    continue;

                var isPlayer = gridCell.IsPlayerOccupied;
                var isAlly = isPlayer == casterIsPlayer;

                // Get entity name - default to ID if not available
                var name = isPlayer ? "Player" : $"Entity-{occupantId.ToString()[..8]}";

                var info = new AffectedEntityInfo(occupantId, name, cellPos, isAlly);

                if (isAlly)
                {
                    if (areaEffect.AffectsAllies)
                        allies.Add(info);
                }
                else
                {
                    if (areaEffect.AffectsEnemies)
                        enemies.Add(info);
                }
            }
        }

        var description = $"{areaEffect.Shape} affects {cells.Count} cells, {enemies.Count} enemies";
        if (allies.Count > 0)
            description += $", {allies.Count} allies";

        _logger.LogInformation("Generated AoE preview: {Description}", description);

        return new AreaEffectPreview(cells, enemies, allies, description);
    }

    /// <inheritdoc/>
    public AreaEffectValidation ValidateTarget(
        AreaEffect areaEffect,
        GridPosition origin,
        GridPosition targetPoint,
        int range)
    {
        var distance = origin.DistanceTo(targetPoint);

        if (distance > range)
        {
            _logger.LogDebug("AoE target {Target} out of range ({Distance} > {Range})",
                targetPoint, distance, range);
            return AreaEffectValidation.OutOfRange;
        }

        var losResult = _losService.HasLineOfSight(origin, targetPoint);
        if (!losResult.HasLOS)
        {
            _logger.LogDebug("AoE target {Target} has no line of sight", targetPoint);
            return AreaEffectValidation.NoLineOfSight;
        }

        return AreaEffectValidation.Success;
    }

    /// <summary>
    /// Gets the unit direction vector for a facing direction.
    /// </summary>
    private static (int dx, int dy) GetDirectionVector(FacingDirection direction) =>
        direction switch
        {
            FacingDirection.North => (0, -1),
            FacingDirection.NorthEast => (1, -1),
            FacingDirection.East => (1, 0),
            FacingDirection.SouthEast => (1, 1),
            FacingDirection.South => (0, 1),
            FacingDirection.SouthWest => (-1, 1),
            FacingDirection.West => (-1, 0),
            FacingDirection.NorthWest => (-1, -1),
            _ => (0, -1)
        };
}
