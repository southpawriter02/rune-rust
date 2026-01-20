using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing combat grid operations.
/// </summary>
/// <remarks>
/// <para>
/// CombatGridService handles the lifecycle of combat grids including creation,
/// entity placement, and queries. It maintains a reference to the currently
/// active grid for the ongoing combat encounter.
/// </para>
/// <para>
/// Player placement follows the configured start position (default: center-south).
/// Monsters are distributed evenly across the configured spawn zone (default: north).
/// </para>
/// </remarks>
public class CombatGridService : ICombatGridService
{
    private readonly IGameConfigurationProvider _config;
    private readonly ILogger<CombatGridService> _logger;
    private CombatGrid? _activeGrid;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatGridService"/> class.
    /// </summary>
    /// <param name="config">Configuration provider for grid settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public CombatGridService(
        IGameConfigurationProvider config,
        ILogger<CombatGridService> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <inheritdoc/>
    public CombatGrid CreateGrid(Room room, int? width = null, int? height = null)
    {
        var settings = _config.GetGridSettings();
        var gridWidth = width ?? settings.DefaultWidth;
        var gridHeight = height ?? settings.DefaultHeight;

        var grid = CombatGrid.Create(gridWidth, gridHeight, room.Id);

        _logger.LogInformation(
            "Created {Width}x{Height} combat grid for room {RoomName}",
            gridWidth, gridHeight, room.Name);

        return grid;
    }

    /// <inheritdoc/>
    public GridInitializationResult InitializePositions(
        CombatGrid grid,
        Player player,
        IEnumerable<Monster> monsters)
    {
        var monsterPositions = new Dictionary<Guid, GridPosition>();

        // Place player at center-south (middle column, near bottom row)
        var playerPos = new GridPosition(grid.Width / 2, grid.Height - 2);

        if (!grid.PlaceEntity(player.Id, playerPos, isPlayer: true))
        {
            _logger.LogWarning(
                "Failed to place player at {Position} on grid",
                playerPos);

            return new GridInitializationResult(
                Success: false,
                PlayerPosition: default,
                MonsterPositions: monsterPositions,
                Message: "Failed to place player on grid.");
        }

        _logger.LogDebug("Placed player at {Position}", playerPos);

        // Place monsters in north zone, distributed evenly
        var monsterList = monsters.ToList();
        if (monsterList.Count > 0)
        {
            var spacing = Math.Max(1, grid.Width / (monsterList.Count + 1));
            var monsterRow = 1; // Second row from top

            for (int i = 0; i < monsterList.Count; i++)
            {
                var monster = monsterList[i];
                var x = spacing * (i + 1);

                // Clamp to valid range
                if (x >= grid.Width)
                    x = grid.Width - 1;

                var pos = new GridPosition(x, monsterRow);

                if (grid.PlaceEntity(monster.Id, pos, isPlayer: false))
                {
                    monsterPositions[monster.Id] = pos;
                    _logger.LogDebug(
                        "Placed {MonsterName} at {Position}",
                        monster.Name, pos);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to place {MonsterName} at {Position}",
                        monster.Name, pos);
                }
            }
        }

        _logger.LogInformation(
            "Initialized grid with player and {MonsterCount} monsters",
            monsterPositions.Count);

        return new GridInitializationResult(
            Success: true,
            PlayerPosition: playerPos,
            MonsterPositions: monsterPositions,
            Message: $"Initialized grid with player and {monsterPositions.Count} monsters.");
    }

    /// <inheritdoc/>
    public CombatGrid? GetActiveGrid() => _activeGrid;

    /// <inheritdoc/>
    public void SetActiveGrid(CombatGrid? grid)
    {
        _activeGrid = grid;

        if (grid != null)
        {
            _logger.LogDebug(
                "Set active grid: {Width}x{Height}, {EntityCount} entities",
                grid.Width, grid.Height, grid.EntityPositions.Count);
        }
    }

    /// <inheritdoc/>
    public void ClearGrid()
    {
        if (_activeGrid != null)
        {
            _logger.LogDebug("Cleared active combat grid");
            _activeGrid = null;
        }
    }

    /// <inheritdoc/>
    public GridPosition? GetEntityPosition(Guid entityId) =>
        _activeGrid?.GetEntityPosition(entityId);

    /// <inheritdoc/>
    public int? GetDistance(Guid entityId1, Guid entityId2) =>
        _activeGrid?.GetDistanceBetween(entityId1, entityId2);

    /// <inheritdoc/>
    public bool AreAdjacent(Guid entityId1, Guid entityId2) =>
        _activeGrid?.AreAdjacent(entityId1, entityId2) ?? false;

    // ===== Room Initialization Methods (v0.5.2c) =====

    /// <inheritdoc/>
    public CombatGrid InitializeFromRoom(
        Room room,
        IEnumerable<DTOs.TerrainLayoutEntry>? terrainLayout = null,
        IEnumerable<DTOs.CoverLayoutEntry>? coverLayout = null)
    {
        var grid = CreateGrid(room);
        SetActiveGrid(grid);

        if (terrainLayout != null)
            ApplyRoomTerrain(terrainLayout);

        if (coverLayout != null)
            ApplyRoomCover(coverLayout);

        _logger.LogInformation(
            "Initialized grid from room {Room}: {W}x{H} with terrain/cover",
            room.Name, grid.Width, grid.Height);

        return grid;
    }

    /// <inheritdoc/>
    public void ApplyRoomTerrain(IEnumerable<DTOs.TerrainLayoutEntry> terrainLayout)
    {
        if (_activeGrid == null)
        {
            _logger.LogWarning("Cannot apply terrain: no active grid");
            return;
        }

        foreach (var entry in terrainLayout)
        {
            var terrainDef = _config.GetTerrainDefinitionById(entry.TerrainId);
            if (terrainDef == null)
            {
                _logger.LogWarning("Unknown terrain '{Id}' in room configuration", entry.TerrainId);
                continue;
            }

            foreach (var posStr in entry.Positions)
            {
                if (GridPosition.TryParse(posStr, out var pos) && _activeGrid.IsInBounds(pos))
                {
                    var cell = _activeGrid.GetCell(pos);
                    cell?.SetTerrainDefinition(entry.TerrainId);
                    cell?.SetTerrain(terrainDef.Type);

                    _logger.LogDebug("Applied terrain '{Terrain}' at {Position}", entry.TerrainId, pos);
                }
                else
                {
                    _logger.LogWarning("Invalid or out-of-bounds position '{Pos}' for terrain", posStr);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void ApplyRoomCover(IEnumerable<DTOs.CoverLayoutEntry> coverLayout)
    {
        if (_activeGrid == null)
        {
            _logger.LogWarning("Cannot apply cover: no active grid");
            return;
        }

        foreach (var entry in coverLayout)
        {
            var coverDef = _config.GetCoverDefinitionById(entry.CoverId);
            if (coverDef == null)
            {
                _logger.LogWarning("Unknown cover '{Id}' in room configuration", entry.CoverId);
                continue;
            }

            foreach (var posStr in entry.GetAllPositions())
            {
                if (GridPosition.TryParse(posStr, out var pos) && _activeGrid.IsInBounds(pos))
                {
                    var coverObject = CoverObject.Create(coverDef, pos);
                    if (_activeGrid.AddCover(coverObject))
                    {
                        _logger.LogDebug("Added cover '{Cover}' at {Position}", entry.CoverId, pos);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to add cover '{Cover}' at {Position} (occupied?)", entry.CoverId, pos);
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid or out-of-bounds position '{Pos}' for cover", posStr);
                }
            }
        }
    }
}

