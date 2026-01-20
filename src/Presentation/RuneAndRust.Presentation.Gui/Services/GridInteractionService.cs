namespace RuneAndRust.Presentation.Gui.Services;

using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;

/// <summary>
/// Implementation of grid interaction for combat targeting.
/// </summary>
/// <remarks>
/// Handles mode management, valid target calculation, and action execution.
/// Coordinates with ICombatGridService for grid state and IMovementService for movement.
/// </remarks>
public class GridInteractionService : IGridInteractionService
{
    private readonly ICombatGridService? _combatGridService;

    private GridInteractionMode _currentMode = GridInteractionMode.None;
    private Guid _activeEntityId;
    private GridPosition _activePosition;
    private AbilityDefinition? _activeAbility;
    private int _activeRange;
    private readonly HashSet<GridPosition> _validTargets = [];
    private GridPosition? _hoveredPosition;

    public GridInteractionMode CurrentMode => _currentMode;
    public bool IsTargeting => _currentMode != GridInteractionMode.None;

    public event Action<TargetingResult>? OnTargetingComplete;
    public event Action? OnHighlightsChanged;
    public event Action<HoverInfo?>? OnHoverChanged;

    /// <summary>
    /// Creates a new GridInteractionService.
    /// </summary>
    /// <param name="combatGridService">Optional combat grid service for entity queries.</param>
    public GridInteractionService(ICombatGridService? combatGridService = null)
    {
        _combatGridService = combatGridService;
        Log.Debug("GridInteractionService created");
    }

    public void EnterMovementMode(Guid moverId, GridPosition currentPosition, int remainingMovement)
    {
        _currentMode = GridInteractionMode.Movement;
        _activeEntityId = moverId;
        _activePosition = currentPosition;
        _activeRange = remainingMovement;

        // Calculate reachable positions using simple range (for now)
        _validTargets.Clear();
        CalculateMovementTargets(currentPosition, remainingMovement);

        OnHighlightsChanged?.Invoke();
        Log.Debug("Entered movement mode for entity {EntityId}, {Count} reachable cells",
            moverId, _validTargets.Count);
    }

    public void EnterAttackMode(Guid attackerId, GridPosition currentPosition, int range)
    {
        _currentMode = GridInteractionMode.Attack;
        _activeEntityId = attackerId;
        _activePosition = currentPosition;
        _activeRange = range;

        // Calculate cells with enemies in range
        _validTargets.Clear();
        CalculateAttackTargets(currentPosition, range);

        OnHighlightsChanged?.Invoke();
        Log.Debug("Entered attack mode for entity {EntityId}, range {Range}, {Count} targets",
            attackerId, range, _validTargets.Count);
    }

    public void EnterAbilityMode(Guid casterId, GridPosition currentPosition, AbilityDefinition ability)
    {
        _currentMode = GridInteractionMode.Ability;
        _activeEntityId = casterId;
        _activePosition = currentPosition;
        _activeAbility = ability;
        _activeRange = ability.Range;

        // Calculate valid ability targets
        _validTargets.Clear();
        CalculateAbilityTargets(currentPosition, ability);

        OnHighlightsChanged?.Invoke();
        Log.Debug("Entered ability mode for entity {EntityId}, ability {Ability}, {Count} targets",
            casterId, ability.Name, _validTargets.Count);
    }

    public void CancelTargeting()
    {
        var previousMode = _currentMode;

        _currentMode = GridInteractionMode.None;
        _activeEntityId = Guid.Empty;
        _activeAbility = null;
        _activeRange = 0;
        _validTargets.Clear();

        OnHighlightsChanged?.Invoke();

        if (previousMode != GridInteractionMode.None)
        {
            Log.Debug("Cancelled targeting for mode {Mode}", previousMode);
        }
    }

    public void HandleCellClick(GridPosition position)
    {
        if (_currentMode == GridInteractionMode.None)
            return;

        if (!_validTargets.Contains(position))
        {
            Log.Debug("Invalid target position: {Position}", position);
            var failResult = new TargetingResult(_currentMode, position, false, "Invalid target");
            OnTargetingComplete?.Invoke(failResult);
            return;
        }

        // Create success result - actual action execution is handled by the ViewModel/command
        var mode = _currentMode;
        var successResult = new TargetingResult(mode, position, true);

        Log.Information("Target selected: {Mode} at {Position}", mode, position);

        CancelTargeting();
        OnTargetingComplete?.Invoke(successResult);
    }

    public void HandleCellHover(GridPosition position)
    {
        if (_hoveredPosition == position)
            return;

        _hoveredPosition = position;

        var info = CreateHoverInfo(position);
        OnHoverChanged?.Invoke(info);
    }

    public void ClearHover()
    {
        _hoveredPosition = null;
        OnHoverChanged?.Invoke(null);
    }

    public IReadOnlyList<HighlightedCell> GetHighlightedCells()
    {
        var highlightType = _currentMode switch
        {
            GridInteractionMode.Movement => HighlightType.Movement,
            GridInteractionMode.Attack => HighlightType.Attack,
            GridInteractionMode.Ability => HighlightType.Ability,
            _ => HighlightType.Movement // Default, won't be used when None
        };

        var result = _validTargets
            .Select(p => new HighlightedCell(p, highlightType))
            .ToList();

        // Add hover highlight if hovering over a cell not in valid targets
        if (_hoveredPosition.HasValue && IsTargeting)
        {
            if (!_validTargets.Contains(_hoveredPosition.Value))
            {
                result.Add(new HighlightedCell(_hoveredPosition.Value, HighlightType.Selected));
            }
        }

        return result;
    }

    public HoverInfo? GetHoverInfo()
    {
        return _hoveredPosition.HasValue
            ? CreateHoverInfo(_hoveredPosition.Value)
            : null;
    }

    private void CalculateMovementTargets(GridPosition center, int range)
    {
        var grid = _combatGridService?.GetActiveGrid();

        // Use Manhattan distance for movement range
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                // Skip center (current position)
                if (dx == 0 && dy == 0)
                    continue;

                var distance = Math.Abs(dx) + Math.Abs(dy);
                if (distance > range)
                    continue;

                var pos = new GridPosition(center.X + dx, center.Y + dy);

                // Check grid bounds and walkability
                if (grid is not null)
                {
                    if (!grid.IsValidPosition(pos))
                        continue;

                    var cell = grid.GetCell(pos);
                    if (cell is null || !cell.IsPassable || cell.OccupantId.HasValue)
                        continue;
                }

                _validTargets.Add(pos);
            }
        }
    }

    private void CalculateAttackTargets(GridPosition center, int range)
    {
        var grid = _combatGridService?.GetActiveGrid();
        if (grid is null)
            return;

        // Find all cells with enemies within range
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var pos = new GridPosition(x, y);
                var cell = grid.GetCell(pos);

                if (cell?.OccupantId is null)
                    continue;

                // Check if within range (Chebyshev distance for attacks)
                var distance = Math.Max(Math.Abs(pos.X - center.X), Math.Abs(pos.Y - center.Y));
                if (distance > range)
                    continue;

                // Don't include self
                if (cell.OccupantId == _activeEntityId)
                    continue;

                _validTargets.Add(pos);
            }
        }
    }

    private void CalculateAbilityTargets(GridPosition center, AbilityDefinition ability)
    {
        var grid = _combatGridService?.GetActiveGrid();
        var range = ability.Range;

        // Get all positions within ability range
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                var pos = new GridPosition(center.X + dx, center.Y + dy);

                // Check Manhattan distance
                var distance = Math.Abs(dx) + Math.Abs(dy);
                if (distance > range)
                    continue;

                // Validate position against grid if available
                if (grid is not null && !grid.IsValidPosition(pos))
                    continue;

                _validTargets.Add(pos);
            }
        }
    }

    private HoverInfo CreateHoverInfo(GridPosition position)
    {
        var label = $"{(char)('A' + position.Y)}{position.X + 1}";
        var isValid = _validTargets.Contains(position);

        // Try to get entity info from grid
        string? entityName = null;
        int? health = null;
        int? maxHealth = null;

        var grid = _combatGridService?.GetActiveGrid();
        if (grid is not null)
        {
            var cell = grid.GetCell(position);
            if (cell?.OccupantId.HasValue == true)
            {
                // Entity at position - we'd need to query the entity
                // For now, just indicate an entity is present
                entityName = "Entity";
            }
        }

        return new HoverInfo(
            Position: position,
            CellLabel: label,
            EntityName: entityName,
            EntityHealth: health,
            EntityMaxHealth: maxHealth,
            IsValidTarget: isValid);
    }
}
