using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for cover calculations and management.
/// </summary>
/// <remarks>
/// <para>
/// Provides tactical cover mechanics for ranged combat, including:
/// </para>
/// <list type="bullet">
///   <item><description>Cover detection between attackers and targets</description></item>
///   <item><description>Defense bonus calculation for partial cover</description></item>
///   <item><description>Full cover blocking attack targeting</description></item>
///   <item><description>Destructible cover with HP tracking</description></item>
/// </list>
/// </remarks>
public class CoverService : ICoverService
{
    private readonly ICombatGridService _gridService;
    private readonly ILineOfSightService _losService;
    private readonly IGameConfigurationProvider _config;
    private readonly ILogger<CoverService> _logger;

    private readonly Dictionary<string, CoverDefinition> _definitions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CoverService"/> class.
    /// </summary>
    /// <param name="gridService">The combat grid service.</param>
    /// <param name="losService">The line of sight service.</param>
    /// <param name="config">The configuration provider.</param>
    /// <param name="logger">The logger.</param>
    public CoverService(
        ICombatGridService gridService,
        ILineOfSightService losService,
        IGameConfigurationProvider config,
        ILogger<CoverService> logger)
    {
        _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
        _losService = losService ?? throw new ArgumentNullException(nameof(losService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        LoadDefinitions();
    }

    /// <summary>
    /// Loads cover definitions from configuration.
    /// </summary>
    private void LoadDefinitions()
    {
        _definitions.Clear();

        foreach (var def in _config.GetCoverDefinitions())
        {
            _definitions[def.Id.ToLowerInvariant()] = def;
            _logger.LogDebug("Loaded cover definition: {CoverId} ({CoverType})", def.Id, def.CoverType);
        }

        _logger.LogInformation("Loaded {Count} cover definitions", _definitions.Count);
    }

    /// <inheritdoc/>
    public CoverCheckResult GetCoverBetween(Guid attackerId, Guid targetId)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            return new CoverCheckResult(CoverType.None, 0, null, false, "No active grid.");

        var attackerPos = grid.GetEntityPosition(attackerId);
        var targetPos = grid.GetEntityPosition(targetId);

        if (!attackerPos.HasValue || !targetPos.HasValue)
            return new CoverCheckResult(CoverType.None, 0, null, false, "Entity not on grid.");

        return GetCoverBetween(attackerPos.Value, targetPos.Value);
    }

    /// <inheritdoc/>
    public CoverCheckResult GetCoverBetween(GridPosition attacker, GridPosition target)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            return new CoverCheckResult(CoverType.None, 0, null, false, "No active grid.");

        var protectingCover = GetProtectingCover(target, attacker).ToList();

        if (!protectingCover.Any())
        {
            _logger.LogDebug(
                "No cover between {Attacker} and {Target}",
                attacker, target);
            return CoverCheckResult.None;
        }

        // Get the best cover (full > partial, highest defense bonus)
        var bestCover = protectingCover
            .OrderByDescending(c => c.CoverType)
            .ThenByDescending(c => c.DefenseBonus)
            .First();

        if (bestCover.CoverType == CoverType.Full)
        {
            _logger.LogDebug(
                "Full cover at {Position} blocks attack from {Attacker} to {Target}",
                bestCover.Position, attacker, target);

            return new CoverCheckResult(
                CoverType.Full, 0, bestCover, true,
                $"Target has full cover behind {bestCover.Name}.");
        }

        _logger.LogDebug(
            "Partial cover at {Position} provides +{Bonus} defense from {Attacker} to {Target}",
            bestCover.Position, bestCover.DefenseBonus, attacker, target);

        return new CoverCheckResult(
            CoverType.Partial, bestCover.DefenseBonus, bestCover, false,
            $"Target has partial cover (+{bestCover.DefenseBonus} defense) from {bestCover.Name}.");
    }

    /// <inheritdoc/>
    public int GetDefenseBonus(Guid attackerId, Guid targetId) =>
        GetCoverBetween(attackerId, targetId).DefenseBonus;

    /// <inheritdoc/>
    public bool HasFullCover(Guid attackerId, Guid targetId) =>
        GetCoverBetween(attackerId, targetId).CoverType == CoverType.Full;

    /// <inheritdoc/>
    public IEnumerable<CoverObject> GetProtectingCover(
        GridPosition position, GridPosition attackerPosition)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            yield break;

        // Get line from attacker to target
        var lineCells = _losService.GetLineCells(attackerPosition, position).ToList();

        // Check cells on the line for cover (excluding start and end)
        foreach (var cell in lineCells)
        {
            if (cell.Equals(attackerPosition) || cell.Equals(position))
                continue;

            var cover = grid.GetCover(cell);
            if (cover != null && !cover.IsDestroyed)
                yield return cover;
        }

        // Also check adjacent cells to target for directional cover
        foreach (var adjacent in grid.GetAdjacentCells(position))
        {
            var cover = grid.GetCover(adjacent.Position);
            if (cover != null && !cover.IsDestroyed)
            {
                // Check if cover is between attacker and target
                if (IsPositionBetween(attackerPosition, position, adjacent.Position))
                    yield return cover;
            }
        }
    }

    /// <summary>
    /// Checks if a cover position is between the attacker and target.
    /// </summary>
    /// <param name="attacker">The attacker's position.</param>
    /// <param name="target">The target's position.</param>
    /// <param name="cover">The cover's position.</param>
    /// <returns>True if the cover is between the attacker and target.</returns>
    private static bool IsPositionBetween(GridPosition attacker, GridPosition target, GridPosition cover)
    {
        // Cover is between if:
        // 1. It's closer to the attacker than the target is
        // 2. It's adjacent to the target
        var attackerToTarget = target.DistanceTo(attacker);
        var attackerToCover = cover.DistanceTo(attacker);
        var coverToTarget = cover.DistanceTo(target);

        return attackerToCover < attackerToTarget && coverToTarget <= 1;
    }

    /// <inheritdoc/>
    public bool AddCover(CoverDefinition definition, GridPosition position)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
        {
            _logger.LogWarning("Cannot add cover - no active grid");
            return false;
        }

        var cover = CoverObject.Create(definition, position);
        var result = grid.AddCover(cover);

        if (result)
        {
            _logger.LogDebug(
                "Added cover '{Name}' at {Position}",
                definition.Name, position);
        }
        else
        {
            _logger.LogWarning(
                "Failed to add cover '{Name}' at {Position} - position invalid or occupied",
                definition.Name, position);
        }

        return result;
    }

    /// <inheritdoc/>
    public bool RemoveCover(GridPosition position)
    {
        var grid = _gridService.GetActiveGrid();
        var result = grid?.RemoveCover(position) ?? false;

        if (result)
        {
            _logger.LogDebug("Removed cover at {Position}", position);
        }

        return result;
    }

    /// <inheritdoc/>
    public CoverDamageResult DamageCover(GridPosition position, int damage)
    {
        var grid = _gridService.GetActiveGrid();
        var cover = grid?.GetCover(position);

        if (cover == null)
        {
            _logger.LogDebug("No cover to damage at {Position}", position);
            return CoverDamageResult.None;
        }

        if (!cover.IsDestructible)
        {
            _logger.LogDebug(
                "Cover '{Name}' at {Position} is not destructible",
                cover.Name, position);

            return new CoverDamageResult(
                false, 0, cover.CurrentHitPoints, false,
                cover.Name, $"{cover.Name} cannot be destroyed.");
        }

        var hpBefore = cover.CurrentHitPoints;
        var destroyed = cover.TakeDamage(damage);

        if (destroyed)
        {
            grid!.RemoveCover(position);
            _logger.LogInformation(
                "Cover '{Name}' at {Position} destroyed (took {Damage} damage, had {HP} HP)",
                cover.Name, position, damage, hpBefore);

            return new CoverDamageResult(
                true, damage, 0, true,
                cover.Name, $"{cover.Name} is destroyed!");
        }

        _logger.LogDebug(
            "Cover '{Name}' at {Position} took {Damage} damage ({Current}/{Max} HP)",
            cover.Name, position, damage, cover.CurrentHitPoints, cover.MaxHitPoints);

        return new CoverDamageResult(
            true, damage, cover.CurrentHitPoints, false,
            cover.Name,
            $"{cover.Name} takes {damage} damage ({cover.CurrentHitPoints}/{cover.MaxHitPoints} HP).");
    }

    /// <inheritdoc/>
    public CoverObject? GetCover(GridPosition position) =>
        _gridService.GetActiveGrid()?.GetCover(position);

    /// <inheritdoc/>
    public IEnumerable<CoverDefinition> GetAllCoverDefinitions() => _definitions.Values;
}
