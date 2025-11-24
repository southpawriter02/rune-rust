using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.Engine;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Serilog;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// View model for the combat view.
/// Manages combat state, actions, targeting, and turn flow.
/// Integrates with CombatEngine from v0.1 for action execution.
/// </summary>
public class CombatViewModel : ViewModelBase
{
    private static readonly ILogger _log = Log.ForContext<CombatViewModel>();
    private readonly ISpriteService _spriteService;
    private readonly IDialogService _dialogService;
    private readonly CombatEngine _combatEngine;
    private readonly EnemyAI _enemyAI;
    private readonly IStatusEffectIconService _statusEffectIconService;

    private CombatState? _combatState;
    private int _columns = 3;
    private double _cellSize = 80.0;
    private GridPosition? _selectedPosition;
    private GridPosition? _hoveredPosition;
    private HashSet<GridPosition> _highlightedPositions = new();
    private Dictionary<GridPosition, SKBitmap> _unitSprites = new();
    private Dictionary<GridPosition, Combatant> _unitData = new();
    private string _statusMessage = "Combat will begin once initialized...";
    private TargetingMode _targetingMode = TargetingMode.None;
    private string? _selectedAbilityName;

    /// <summary>
    /// Gets the view title.
    /// </summary>
    public string Title => "Tactical Combat";

    /// <summary>
    /// Gets or sets the combat state.
    /// </summary>
    public CombatState? CombatState
    {
        get => _combatState;
        private set => this.RaiseAndSetIfChanged(ref _combatState, value);
    }

    /// <summary>
    /// Gets or sets the number of columns in the grid.
    /// </summary>
    public int Columns
    {
        get => _columns;
        set => this.RaiseAndSetIfChanged(ref _columns, value);
    }

    /// <summary>
    /// Gets or sets the cell size in pixels.
    /// </summary>
    public double CellSize
    {
        get => _cellSize;
        set => this.RaiseAndSetIfChanged(ref _cellSize, value);
    }

    /// <summary>
    /// Gets or sets the currently selected grid position.
    /// </summary>
    public GridPosition? SelectedPosition
    {
        get => _selectedPosition;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPosition, value);
            UpdateHighlightedPositions();
            UpdateStatusMessage();
        }
    }

    /// <summary>
    /// Gets or sets the currently hovered grid position.
    /// </summary>
    public GridPosition? HoveredPosition
    {
        get => _hoveredPosition;
        set => this.RaiseAndSetIfChanged(ref _hoveredPosition, value);
    }

    /// <summary>
    /// Gets the collection of highlighted grid positions (e.g., valid move/attack targets).
    /// </summary>
    public IReadOnlyCollection<GridPosition> HighlightedPositions => _highlightedPositions;

    /// <summary>
    /// Gets the dictionary of unit sprites at grid positions.
    /// </summary>
    public Dictionary<GridPosition, SKBitmap> UnitSprites
    {
        get => _unitSprites;
        private set => this.RaiseAndSetIfChanged(ref _unitSprites, value);
    }

    /// <summary>
    /// Gets the dictionary of unit data (Combatant objects) at grid positions.
    /// Includes HP, status effects, and other combat-relevant data.
    /// </summary>
    public Dictionary<GridPosition, Combatant> UnitData
    {
        get => _unitData;
        private set => this.RaiseAndSetIfChanged(ref _unitData, value);
    }

    /// <summary>
    /// Gets the status effect icon service.
    /// </summary>
    public IStatusEffectIconService StatusEffectIconService => _statusEffectIconService;

    /// <summary>
    /// Gets the current status message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    /// <summary>
    /// Gets the combat log (recent first).
    /// </summary>
    public ObservableCollection<string> CombatLog { get; } = new();

    /// <summary>
    /// Gets the turn order list.
    /// </summary>
    public ObservableCollection<TurnOrderEntry> TurnOrder { get; } = new();

    /// <summary>
    /// Gets whether it's currently the player's turn.
    /// </summary>
    public bool IsPlayerTurn => _combatState?.IsPlayerTurn() ?? false;

    /// <summary>
    /// Command to handle cell clicks.
    /// </summary>
    public ReactiveCommand<GridPosition, Unit> CellClickedCommand { get; }

    /// <summary>
    /// Command to handle cell hover.
    /// </summary>
    public ReactiveCommand<GridPosition, Unit> CellHoveredCommand { get; }

    /// <summary>
    /// Command to execute attack action.
    /// </summary>
    public ReactiveCommand<Unit, Unit> AttackCommand { get; }

    /// <summary>
    /// Command to execute defend action.
    /// </summary>
    public ReactiveCommand<Unit, Unit> DefendCommand { get; }

    /// <summary>
    /// Command to use ability.
    /// </summary>
    public ReactiveCommand<Unit, Unit> UseAbilityCommand { get; }

    /// <summary>
    /// Command to use item.
    /// </summary>
    public ReactiveCommand<Unit, Unit> UseItemCommand { get; }

    /// <summary>
    /// Command to move.
    /// </summary>
    public ReactiveCommand<Unit, Unit> MoveCommand { get; }

    /// <summary>
    /// Command to end turn.
    /// </summary>
    public ReactiveCommand<Unit, Unit> EndTurnCommand { get; }

    /// <summary>
    /// Command to flee combat.
    /// </summary>
    public ReactiveCommand<Unit, Unit> FleeCommand { get; }

    public CombatViewModel(
        ISpriteService spriteService,
        IDialogService dialogService,
        CombatEngine combatEngine,
        EnemyAI enemyAI,
        IStatusEffectIconService statusEffectIconService)
    {
        _spriteService = spriteService ?? throw new ArgumentNullException(nameof(spriteService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _combatEngine = combatEngine ?? throw new ArgumentNullException(nameof(combatEngine));
        _enemyAI = enemyAI ?? throw new ArgumentNullException(nameof(enemyAI));
        _statusEffectIconService = statusEffectIconService ?? throw new ArgumentNullException(nameof(statusEffectIconService));

        // Observable for whether it's player's turn
        var canExecutePlayerAction = this.WhenAnyValue(x => x.IsPlayerTurn);

        // Initialize grid interaction commands
        CellClickedCommand = ReactiveCommand.Create<GridPosition>(OnCellClicked);
        CellHoveredCommand = ReactiveCommand.Create<GridPosition>(OnCellHovered);

        // Initialize combat action commands
        AttackCommand = ReactiveCommand.Create(StartAttackTargeting, canExecutePlayerAction);
        DefendCommand = ReactiveCommand.Create(ExecuteDefend, canExecutePlayerAction);
        UseAbilityCommand = ReactiveCommand.CreateFromTask(SelectAndUseAbilityAsync, canExecutePlayerAction);
        UseItemCommand = ReactiveCommand.CreateFromTask(SelectAndUseItemAsync, canExecutePlayerAction);
        MoveCommand = ReactiveCommand.Create(StartMoveTargeting, canExecutePlayerAction);
        EndTurnCommand = ReactiveCommand.Create(EndPlayerTurn, canExecutePlayerAction);
        FleeCommand = ReactiveCommand.CreateFromTask(AttemptFleeAsync, canExecutePlayerAction);

        _log.Information("CombatViewModel initialized");

        // Initialize with demo scenario for v0.43.5
        InitializeDemoScenario();
    }

    /// <summary>
    /// Initializes combat with a real CombatState from the engine.
    /// </summary>
    public void LoadCombatState(CombatState combatState)
    {
        _log.Information("Loading combat state with {EnemyCount} enemies", combatState.Enemies.Count);

        CombatState = combatState;
        RefreshGridAndSprites();
        UpdateTurnOrder();
        SyncCombatLog();
        UpdateStatusMessage();

        this.RaisePropertyChanged(nameof(IsPlayerTurn));

        _log.Information("Combat state loaded, IsActive={IsActive}, IsPlayerTurn={IsPlayerTurn}",
            combatState.IsActive, IsPlayerTurn);
    }

    private void OnCellClicked(GridPosition position)
    {
        if (_targetingMode == TargetingMode.None)
        {
            SelectedPosition = position;
            return;
        }

        // Handle targeting click
        HandleTargetingClick(position);
    }

    private void OnCellHovered(GridPosition position)
    {
        HoveredPosition = position;
    }

    #region Action Commands

    private void StartAttackTargeting()
    {
        if (_combatState == null) return;

        _targetingMode = TargetingMode.AttackTarget;
        _log.Information("Started attack targeting");

        // Highlight enemy positions
        HighlightEnemyPositions();

        StatusMessage = "Select an enemy to attack...";
    }

    private void ExecuteDefend()
    {
        if (_combatState == null) return;

        try
        {
            _combatEngine.PlayerDefend(_combatState);
            AddToCombatLog("You take a defensive stance!");

            CompletePlayerTurn();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing defend action");
            StatusMessage = "Failed to execute defend action";
        }
    }

    private async Task SelectAndUseAbilityAsync()
    {
        if (_combatState == null) return;

        var abilities = _combatState.Player.Abilities;
        if (abilities.Count == 0)
        {
            await _dialogService.ShowMessageAsync("No Abilities", "You have no abilities available.");
            return;
        }

        // For v0.43.5, show simple dialog (full ability UI in v0.43.6)
        var abilityNames = abilities.Select(a => $"{a.Name} (Cost: {a.Cost} {a.ResourceType})").ToList();
        // TODO: Use proper ability selection dialog
        // For now, just use first ability as demo
        if (abilities.Count > 0)
        {
            var ability = abilities[0];
            _selectedAbilityName = ability.Name;

            // Check if ability requires target
            if (ability.Type == AbilityType.Attack || ability.Type == AbilityType.Control)
            {
                _targetingMode = TargetingMode.AbilityTarget;
                HighlightEnemyPositions();
                StatusMessage = $"Select target for {ability.Name}...";
            }
            else
            {
                // Self/AOE ability
                ExecuteAbility(ability, null);
            }
        }
    }

    private async Task SelectAndUseItemAsync()
    {
        if (_combatState == null) return;

        // TODO: Implement item selection in v0.43.10
        await _dialogService.ShowMessageAsync("Not Implemented", "Item usage will be implemented in v0.43.10");
    }

    private void StartMoveTargeting()
    {
        if (_combatState == null || _combatState.Grid == null) return;

        _targetingMode = TargetingMode.MovementTarget;
        _log.Information("Started movement targeting");

        // Highlight valid movement positions (for demo, highlight adjacent empty cells)
        HighlightValidMovementPositions();

        StatusMessage = "Select position to move to...";
    }

    private void EndPlayerTurn()
    {
        ClearTargeting();
        CompletePlayerTurn();
    }

    private async Task AttemptFleeAsync()
    {
        if (_combatState == null) return;

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Flee Combat",
            "Are you sure you want to flee? You may take damage and lose rewards.");

        if (confirm)
        {
            bool fled = _combatEngine.PlayerFlee(_combatState);
            if (fled)
            {
                AddToCombatLog("You successfully fled from combat!");
                _combatState.IsActive = false;
                await _dialogService.ShowMessageAsync("Fled", "You have fled from combat.");
            }
            else
            {
                AddToCombatLog("Failed to flee!");
                CompletePlayerTurn();
            }
        }
    }

    #endregion

    #region Targeting

    private void HandleTargetingClick(GridPosition position)
    {
        switch (_targetingMode)
        {
            case TargetingMode.AttackTarget:
                ExecuteAttackOnTarget(position);
                break;

            case TargetingMode.MovementTarget:
                ExecuteMovement(position);
                break;

            case TargetingMode.AbilityTarget:
                ExecuteAbilityOnTarget(position);
                break;
        }

        ClearTargeting();
    }

    private void ExecuteAttackOnTarget(GridPosition position)
    {
        if (_combatState == null) return;

        var target = FindEnemyAtPosition(position);
        if (target == null)
        {
            StatusMessage = "No enemy at that position";
            return;
        }

        try
        {
            _combatEngine.PlayerAttack(_combatState, target);
            SyncCombatLog();
            RefreshGridAndSprites();

            if (_combatEngine.IsCombatOver(_combatState))
            {
                HandleCombatEnd();
                return;
            }

            CompletePlayerTurn();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing attack");
            StatusMessage = "Failed to execute attack";
        }
    }

    private void ExecuteMovement(GridPosition position)
    {
        if (_combatState == null || _combatState.Grid == null) return;

        // TODO: Integrate with AdvancedMovementService for proper movement
        // For v0.43.5, this is a placeholder
        AddToCombatLog($"Movement to {position} (full implementation in future version)");
        CompletePlayerTurn();
    }

    private void ExecuteAbilityOnTarget(GridPosition position)
    {
        if (_combatState == null || _selectedAbilityName == null) return;

        var ability = _combatState.Player.Abilities.FirstOrDefault(a => a.Name == _selectedAbilityName);
        if (ability == null) return;

        var target = FindEnemyAtPosition(position);
        ExecuteAbility(ability, target);
    }

    private void ExecuteAbility(Ability ability, Enemy? target)
    {
        if (_combatState == null) return;

        try
        {
            bool success = _combatEngine.PlayerUseAbility(_combatState, ability, target);
            SyncCombatLog();
            RefreshGridAndSprites();

            if (!success)
            {
                StatusMessage = "Failed to use ability";
                return;
            }

            if (_combatEngine.IsCombatOver(_combatState))
            {
                HandleCombatEnd();
                return;
            }

            CompletePlayerTurn();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing ability {AbilityName}", ability.Name);
            StatusMessage = $"Failed to execute {ability.Name}";
        }
    }

    private void ClearTargeting()
    {
        _targetingMode = TargetingMode.None;
        _selectedAbilityName = null;
        _highlightedPositions.Clear();
        this.RaisePropertyChanged(nameof(HighlightedPositions));
    }

    #endregion

    #region Turn Management

    private void CompletePlayerTurn()
    {
        if (_combatState == null) return;

        _log.Information("Completing player turn");

        _combatEngine.NextTurn(_combatState);
        UpdateTurnOrder();
        RefreshGridAndSprites();
        this.RaisePropertyChanged(nameof(IsPlayerTurn));

        // Process enemy turns
        ProcessEnemyTurns();
    }

    private void ProcessEnemyTurns()
    {
        if (_combatState == null) return;

        while (!_combatState.IsPlayerTurn() && _combatState.IsActive)
        {
            var currentEnemy = _combatState.CurrentParticipant.Character as Enemy;
            if (currentEnemy != null && currentEnemy.IsAlive)
            {
                _log.Information("Processing enemy turn for {EnemyName}", currentEnemy.Name);

                _enemyAI.ExecuteTurn(_combatState, currentEnemy);
                SyncCombatLog();
                RefreshGridAndSprites();

                if (_combatEngine.IsCombatOver(_combatState))
                {
                    HandleCombatEnd();
                    return;
                }
            }

            _combatEngine.NextTurn(_combatState);
            UpdateTurnOrder();
        }

        this.RaisePropertyChanged(nameof(IsPlayerTurn));
        StatusMessage = "Your turn - choose an action";
    }

    private void HandleCombatEnd()
    {
        if (_combatState == null) return;

        _log.Information("Combat ended");

        bool playerVictory = _combatState.Enemies.All(e => !e.IsAlive);

        Task.Run(async () =>
        {
            if (playerVictory)
            {
                await _dialogService.ShowMessageAsync("Victory!", "You have defeated all enemies!");
            }
            else
            {
                await _dialogService.ShowMessageAsync("Defeat", "You have been defeated...");
            }
        });
    }

    #endregion

    #region UI Updates

    private void RefreshGridAndSprites()
    {
        if (_combatState == null || _combatState.Grid == null) return;

        // Update columns from grid
        Columns = _combatState.Grid.Columns;

        // Rebuild unit sprites and data from grid
        _unitSprites.Clear();
        _unitData.Clear();

        // Add player
        if (_combatState.Player.CurrentPosition != null)
        {
            var sprite = _spriteService.GetSpriteBitmap("warrior", 3);
            if (sprite != null)
            {
                _unitSprites[_combatState.Player.CurrentPosition.Value] = sprite;
                _unitData[_combatState.Player.CurrentPosition.Value] = _combatState.Player;
            }
        }

        // Add enemies
        foreach (var enemy in _combatState.Enemies.Where(e => e.IsAlive && e.CurrentPosition != null))
        {
            var spriteName = GetEnemySpriteNameFrom(enemy);
            var sprite = _spriteService.GetSpriteBitmap(spriteName, 3);
            if (sprite != null)
            {
                _unitSprites[enemy.CurrentPosition!.Value] = sprite;
                _unitData[enemy.CurrentPosition!.Value] = enemy;
            }
        }

        this.RaisePropertyChanged(nameof(UnitSprites));
        this.RaisePropertyChanged(nameof(UnitData));
    }

    private string GetEnemySpriteNameFrom(Enemy enemy)
    {
        // Map enemy type to sprite name
        var name = enemy.Name.ToLowerInvariant();
        if (name.Contains("goblin")) return "goblin";
        if (name.Contains("blessed") || name.Contains("priest")) return "blessed";
        return "goblin"; // Default
    }

    private void UpdateTurnOrder()
    {
        if (_combatState == null) return;

        TurnOrder.Clear();

        foreach (var participant in _combatState.InitiativeOrder)
        {
            var entry = new TurnOrderEntry
            {
                Name = participant.Name,
                IsPlayer = participant.IsPlayer,
                IsCompanion = participant.IsCompanion,
                IsActive = participant == _combatState.CurrentParticipant
            };
            TurnOrder.Add(entry);
        }
    }

    private void SyncCombatLog()
    {
        if (_combatState == null) return;

        // Clear and rebuild from combat state (recent first)
        CombatLog.Clear();
        for (int i = _combatState.CombatLog.Count - 1; i >= Math.Max(0, _combatState.CombatLog.Count - 50); i--)
        {
            CombatLog.Add(_combatState.CombatLog[i]);
        }
    }

    private void AddToCombatLog(string message)
    {
        _combatState?.AddLogEntry(message);
        CombatLog.Insert(0, message);
        if (CombatLog.Count > 50)
        {
            CombatLog.RemoveAt(CombatLog.Count - 1);
        }
    }

    private void UpdateStatusMessage()
    {
        if (_combatState == null)
        {
            StatusMessage = "No active combat";
            return;
        }

        if (_targetingMode != TargetingMode.None)
        {
            // Status message set by targeting method
            return;
        }

        if (IsPlayerTurn)
        {
            StatusMessage = "Your turn - choose an action";
        }
        else
        {
            var currentName = _combatState.CurrentParticipant.Name;
            StatusMessage = $"{currentName}'s turn...";
        }
    }

    private void HighlightEnemyPositions()
    {
        if (_combatState == null) return;

        _highlightedPositions.Clear();
        foreach (var enemy in _combatState.Enemies.Where(e => e.IsAlive && e.CurrentPosition != null))
        {
            _highlightedPositions.Add(enemy.CurrentPosition!.Value);
        }

        this.RaisePropertyChanged(nameof(HighlightedPositions));
    }

    private void HighlightValidMovementPositions()
    {
        // TODO: Use AdvancedMovementService to calculate valid positions
        // For v0.43.5, show adjacent empty cells as placeholder
        _highlightedPositions.Clear();

        if (_combatState?.Player.CurrentPosition == null) return;

        var currentPos = _combatState.Player.CurrentPosition.Value;

        // Adjacent columns in same row/zone
        for (int colOffset = -1; colOffset <= 1; colOffset++)
        {
            if (colOffset == 0) continue;
            var targetCol = currentPos.Column + colOffset;
            if (targetCol >= 0 && targetCol < Columns)
            {
                var pos = new GridPosition(currentPos.Zone, currentPos.Row, targetCol);
                if (!IsPositionOccupied(pos))
                {
                    _highlightedPositions.Add(pos);
                }
            }
        }

        // Opposite row in same zone
        var oppositeRow = currentPos.Row == Row.Front ? Row.Back : Row.Front;
        var backPos = new GridPosition(currentPos.Zone, oppositeRow, currentPos.Column);
        if (!IsPositionOccupied(backPos))
        {
            _highlightedPositions.Add(backPos);
        }

        this.RaisePropertyChanged(nameof(HighlightedPositions));
    }

    private bool IsPositionOccupied(GridPosition position)
    {
        return _unitSprites.ContainsKey(position);
    }

    private Enemy? FindEnemyAtPosition(GridPosition position)
    {
        if (_combatState == null) return null;

        return _combatState.Enemies.FirstOrDefault(e =>
            e.IsAlive &&
            e.CurrentPosition.HasValue &&
            e.CurrentPosition.Value == position);
    }

    #endregion

    #region Demo Initialization

    private void InitializeDemoScenario()
    {
        // Create demo combat scenario using real CombatEngine
        var diceService = new DiceService();
        var player = new PlayerCharacter
        {
            Name = "Hero",
            Level = 3,
            HP = 45,
            MaxHP = 50,
            Might = 4,
            Finesse = 3,
            Resolve = 3,
            Wits = 2
        };

        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Goblin Scout", HP = 12, MaxHP = 12, Armor = 1, Might = 2, Finesse = 3 },
            new Enemy { Name = "Goblin Warrior", HP = 15, MaxHP = 15, Armor = 2, Might = 3, Finesse = 2 },
            new Enemy { Name = "Goblin Shaman", HP = 10, MaxHP = 10, Armor = 0, Might = 1, Finesse = 2, Wits = 4 }
        };

        var demoCombatState = _combatEngine.InitializeCombat(player, enemies, canFlee: true);

        LoadCombatState(demoCombatState);

        _log.Information("Demo combat scenario initialized");
    }

    #endregion
}

/// <summary>
/// Targeting mode for combat actions.
/// </summary>
public enum TargetingMode
{
    None,
    AttackTarget,
    MovementTarget,
    AbilityTarget
}

/// <summary>
/// Turn order entry for display.
/// </summary>
public class TurnOrderEntry
{
    public string Name { get; set; } = "";
    public bool IsPlayer { get; set; }
    public bool IsCompanion { get; set; }
    public bool IsActive { get; set; }
}

