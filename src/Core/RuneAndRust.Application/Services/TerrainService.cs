using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for terrain calculations and management.
/// </summary>
/// <remarks>
/// <para>
/// Handles terrain-related operations including movement cost calculations,
/// passability checks, and hazard damage application.
/// </para>
/// <para>
/// Terrain definitions are loaded from configuration and cached for lookups.
/// </para>
/// </remarks>
public class TerrainService : ITerrainService
{
    private readonly ICombatGridService _gridService;
    private readonly IDiceService _diceService;
    private readonly IGameConfigurationProvider _config;
    private readonly ILogger<TerrainService> _logger;

    // Cached terrain definitions for fast lookups
    private readonly Dictionary<string, TerrainDefinition> _definitions = new();

    // TODO: Inject game session service for applying damage to entities
    // For now, we return damage info and let caller handle application
    private Player? _player;
    private readonly Dictionary<Guid, Monster> _monsters = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainService"/> class.
    /// </summary>
    /// <param name="gridService">The combat grid service.</param>
    /// <param name="diceService">The dice service for damage rolls.</param>
    /// <param name="config">The configuration provider.</param>
    /// <param name="logger">The logger.</param>
    public TerrainService(
        ICombatGridService gridService,
        IDiceService diceService,
        IGameConfigurationProvider config,
        ILogger<TerrainService> logger)
    {
        _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        LoadDefinitions();
    }

    /// <summary>
    /// Registers a player for damage application.
    /// </summary>
    /// <param name="player">The player to register.</param>
    public void RegisterPlayer(Player player)
    {
        _player = player;
    }

    /// <summary>
    /// Registers a monster for damage application.
    /// </summary>
    /// <param name="monster">The monster to register.</param>
    public void RegisterMonster(Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);
        _monsters[monster.Id] = monster;
    }

    /// <summary>
    /// Clears all registered entities.
    /// </summary>
    public void ClearRegistrations()
    {
        _player = null;
        _monsters.Clear();
    }

    /// <summary>
    /// Loads terrain definitions from configuration.
    /// </summary>
    private void LoadDefinitions()
    {
        _definitions.Clear();

        foreach (var def in _config.GetTerrainDefinitions())
        {
            _definitions[def.Id.ToLowerInvariant()] = def;
            _logger.LogDebug("Loaded terrain definition: {TerrainId} ({TerrainType})", def.Id, def.Type);
        }

        _logger.LogInformation("Loaded {Count} terrain definitions", _definitions.Count);
    }

    /// <inheritdoc/>
    public int GetMovementCost(GridPosition position, MovementDirection direction)
    {
        var baseCost = MovementCosts.GetCost(direction);
        var multiplier = GetMovementCostMultiplier(position);

        var totalCost = (int)Math.Ceiling(baseCost * multiplier);

        _logger.LogDebug(
            "Movement cost for {Position} ({Direction}): base={Base}, multiplier={Multiplier}, total={Total}",
            position, direction, baseCost, multiplier, totalCost);

        return totalCost;
    }

    /// <inheritdoc/>
    public float GetMovementCostMultiplier(GridPosition position)
    {
        var grid = _gridService.GetActiveGrid();
        var cell = grid?.GetCell(position);

        if (cell == null)
        {
            return 1.0f;
        }

        // If cell has a specific terrain definition, use its multiplier
        if (!string.IsNullOrEmpty(cell.TerrainDefinitionId) &&
            _definitions.TryGetValue(cell.TerrainDefinitionId.ToLowerInvariant(), out var def))
        {
            return def.MovementCostMultiplier;
        }

        // Otherwise use the base terrain type multiplier
        return cell.GetMovementCostMultiplier();
    }

    /// <inheritdoc/>
    public bool IsPassable(GridPosition position)
    {
        var grid = _gridService.GetActiveGrid();
        var cell = grid?.GetCell(position);

        if (cell == null)
        {
            return false;
        }

        // If cell has a specific terrain definition, use its passability
        if (!string.IsNullOrEmpty(cell.TerrainDefinitionId) &&
            _definitions.TryGetValue(cell.TerrainDefinitionId.ToLowerInvariant(), out var def))
        {
            return def.IsPassable;
        }

        // Otherwise use the cell's computed passability
        return cell.IsPassable;
    }

    /// <inheritdoc/>
    public bool DealsDamage(GridPosition position)
    {
        var grid = _gridService.GetActiveGrid();
        var cell = grid?.GetCell(position);

        if (cell == null)
        {
            return false;
        }

        // If cell has a specific terrain definition, check if it deals damage
        if (!string.IsNullOrEmpty(cell.TerrainDefinitionId) &&
            _definitions.TryGetValue(cell.TerrainDefinitionId.ToLowerInvariant(), out var def))
        {
            return def.DealsDamage;
        }

        // Base terrain types: only Hazardous can deal damage, but requires a definition
        return false;
    }

    /// <inheritdoc/>
    public TerrainDamageResult? GetTerrainDamage(GridPosition position)
    {
        var grid = _gridService.GetActiveGrid();
        var cell = grid?.GetCell(position);

        if (cell == null || string.IsNullOrEmpty(cell.TerrainDefinitionId))
        {
            return null;
        }

        if (!_definitions.TryGetValue(cell.TerrainDefinitionId.ToLowerInvariant(), out var def) ||
            !def.DealsDamage)
        {
            return null;
        }

        // Roll damage
        var roll = _diceService.Roll(def.DamageOnEntry!);
        var damage = roll.Total;
        var damageType = def.DamageType ?? "untyped";

        _logger.LogDebug(
            "Terrain damage rolled at {Position}: {DamageExpr} = {Damage} {DamageType}",
            position, def.DamageOnEntry, damage, damageType);

        return new TerrainDamageResult(
            DamageDealt: true,
            Damage: damage,
            DamageType: damageType,
            TerrainName: def.Name,
            Message: $"You take {damage} {damageType} damage from {def.Name}!");
    }

    /// <inheritdoc/>
    public TerrainDamageResult ApplyTerrainDamage(Guid entityId, GridPosition position)
    {
        var damageResult = GetTerrainDamage(position);

        if (damageResult == null || !damageResult.Value.DamageDealt)
        {
            return TerrainDamageResult.None;
        }

        var damage = damageResult.Value;

        // Apply damage to the appropriate entity
        if (_player?.Id == entityId)
        {
            _player.TakeDamage(damage.Damage);
            _logger.LogInformation(
                "Player took {Damage} {DamageType} damage from {Terrain} at {Position}",
                damage.Damage, damage.DamageType, damage.TerrainName, position);
        }
        else if (_monsters.TryGetValue(entityId, out var monster))
        {
            monster.TakeDamage(damage.Damage);
            _logger.LogInformation(
                "Monster {MonsterName} took {Damage} {DamageType} damage from {Terrain} at {Position}",
                monster.Name, damage.Damage, damage.DamageType, damage.TerrainName, position);
        }
        else
        {
            _logger.LogWarning(
                "Could not apply terrain damage to entity {EntityId} - entity not registered",
                entityId);
        }

        return damage;
    }

    /// <inheritdoc/>
    public TerrainType GetTerrainType(GridPosition position)
    {
        var grid = _gridService.GetActiveGrid();
        var cell = grid?.GetCell(position);

        if (cell == null)
        {
            return TerrainType.Normal;
        }

        // If cell has a specific terrain definition, use its type
        if (!string.IsNullOrEmpty(cell.TerrainDefinitionId) &&
            _definitions.TryGetValue(cell.TerrainDefinitionId.ToLowerInvariant(), out var def))
        {
            return def.Type;
        }

        return cell.TerrainType;
    }

    /// <inheritdoc/>
    public TerrainDefinition? GetTerrainDefinition(GridPosition position)
    {
        var grid = _gridService.GetActiveGrid();
        var cell = grid?.GetCell(position);

        if (cell == null || string.IsNullOrEmpty(cell.TerrainDefinitionId))
        {
            return null;
        }

        return _definitions.TryGetValue(cell.TerrainDefinitionId.ToLowerInvariant(), out var def)
            ? def
            : null;
    }

    /// <inheritdoc/>
    public void SetTerrain(GridPosition position, TerrainType type)
    {
        var grid = _gridService.GetActiveGrid();
        var cell = grid?.GetCell(position);

        if (cell == null)
        {
            _logger.LogWarning("Cannot set terrain at {Position} - cell not found", position);
            return;
        }

        cell.SetTerrain(type);
        _logger.LogDebug("Set terrain at {Position} to {TerrainType}", position, type);
    }

    /// <inheritdoc/>
    public void SetTerrain(GridPosition position, string terrainDefinitionId)
    {
        var grid = _gridService.GetActiveGrid();
        var cell = grid?.GetCell(position);

        if (cell == null)
        {
            _logger.LogWarning("Cannot set terrain at {Position} - cell not found", position);
            return;
        }

        var normalizedId = terrainDefinitionId?.ToLowerInvariant();

        if (!string.IsNullOrEmpty(normalizedId) && !_definitions.ContainsKey(normalizedId))
        {
            _logger.LogWarning(
                "Terrain definition '{TerrainId}' not found, setting anyway",
                terrainDefinitionId);
        }

        // Set the base terrain type first (if we have a definition)
        // This must come before SetTerrainDefinition since SetTerrain clears the ID
        if (_definitions.TryGetValue(normalizedId ?? string.Empty, out var def))
        {
            cell.SetTerrain(def.Type);
        }

        // Now set the definition ID
        cell.SetTerrainDefinition(terrainDefinitionId!);

        _logger.LogDebug("Set terrain at {Position} to definition '{TerrainId}'", position, terrainDefinitionId);
    }

    /// <inheritdoc/>
    public IEnumerable<TerrainDefinition> GetAllTerrainDefinitions() => _definitions.Values;
}
