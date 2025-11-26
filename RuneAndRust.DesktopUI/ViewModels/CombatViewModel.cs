using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Controllers;
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
/// v0.38: Enhanced with CombatFlavorTextService for rich narrative combat.
/// </summary>
public class CombatViewModel : ViewModelBase
{
    private static readonly ILogger _log = Log.ForContext<CombatViewModel>();
    private readonly ISpriteService _spriteService;
    private readonly IDialogService _dialogService;
    private readonly CombatEngine _combatEngine;
    private readonly EnemyAI _enemyAI;
    private readonly IStatusEffectIconService _statusEffectIconService;
    private readonly IHazardVisualizationService _hazardVisualizationService;
    private readonly IAnimationService _animationService;
    private readonly IBossDisplayService? _bossDisplayService;
    private readonly CombatController? _combatController;

    // v0.38: Combat flavor text service for rich narrative
    private readonly CombatFlavorTextService? _combatFlavorService;

    private CombatState? _combatState;
    private BossCombatViewModel? _bossCombatViewModel;
    private int _columns = 3;
    private double _cellSize = 80.0;
    private GridPosition? _selectedPosition;
    private GridPosition? _hoveredPosition;
    private HashSet<GridPosition> _highlightedPositions = new();
    private Dictionary<GridPosition, SKBitmap> _unitSprites = new();
    private Dictionary<GridPosition, Combatant> _unitData = new();
    private List<EnvironmentalObject> _environmentalObjects = new();
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
            // Note: Highlighting is done via specific targeting methods
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
    /// Gets the battlefield grid with tiles, cover, and terrain data.
    /// </summary>
    public BattlefieldGrid? Grid => _combatState?.Grid;

    /// <summary>
    /// Gets the list of environmental objects (hazards, cover, interactive objects).
    /// </summary>
    public List<EnvironmentalObject> EnvironmentalObjects
    {
        get => _environmentalObjects;
        private set => this.RaiseAndSetIfChanged(ref _environmentalObjects, value);
    }

    /// <summary>
    /// Gets the hazard visualization service.
    /// </summary>
    public IHazardVisualizationService HazardVisualizationService => _hazardVisualizationService;

    /// <summary>
    /// Gets the animation service.
    /// </summary>
    public IAnimationService AnimationService => _animationService;

    /// <summary>
    /// v0.43.17: Gets the boss combat view model for boss-specific UI.
    /// </summary>
    public BossCombatViewModel? BossCombatViewModel
    {
        get => _bossCombatViewModel;
        private set => this.RaiseAndSetIfChanged(ref _bossCombatViewModel, value);
    }

    /// <summary>
    /// v0.43.17: Gets whether this is a boss fight.
    /// </summary>
    public bool IsBossFight => BossCombatViewModel?.IsBossFight ?? false;

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
        IStatusEffectIconService statusEffectIconService,
        IHazardVisualizationService hazardVisualizationService,
        IAnimationService animationService,
        IBossDisplayService? bossDisplayService = null,
        CombatController? combatController = null,
        CombatFlavorTextService? combatFlavorService = null)
    {
        _spriteService = spriteService ?? throw new ArgumentNullException(nameof(spriteService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _combatEngine = combatEngine ?? throw new ArgumentNullException(nameof(combatEngine));
        _enemyAI = enemyAI ?? throw new ArgumentNullException(nameof(enemyAI));
        _statusEffectIconService = statusEffectIconService ?? throw new ArgumentNullException(nameof(statusEffectIconService));
        _hazardVisualizationService = hazardVisualizationService ?? throw new ArgumentNullException(nameof(hazardVisualizationService));
        _animationService = animationService ?? throw new ArgumentNullException(nameof(animationService));
        _bossDisplayService = bossDisplayService;
        _combatController = combatController;
        _combatFlavorService = combatFlavorService;

        // v0.43.17: Initialize boss combat view model if service available
        if (_bossDisplayService != null)
        {
            BossCombatViewModel = new BossCombatViewModel(_bossDisplayService, _animationService);
        }

        // v0.44.4: Initialize controller with this ViewModel
        _combatController?.Initialize(this);

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

        // v0.43.17: Check for boss enemies and initialize boss UI
        CheckForBossEncounter(combatState);

        this.RaisePropertyChanged(nameof(IsPlayerTurn));
        this.RaisePropertyChanged(nameof(IsBossFight));

        _log.Information("Combat state loaded, IsActive={IsActive}, IsPlayerTurn={IsPlayerTurn}, IsBossFight={IsBossFight}",
            combatState.IsActive, IsPlayerTurn, IsBossFight);
    }

    /// <summary>
    /// v0.43.17: Checks for boss enemies and initializes boss combat UI.
    /// </summary>
    private void CheckForBossEncounter(CombatState combatState)
    {
        if (BossCombatViewModel == null) return;

        var boss = combatState.Enemies.FirstOrDefault(e => e.IsBoss);
        if (boss != null)
        {
            _log.Information("Boss encounter detected: {BossName}", boss.Name);
            BossCombatViewModel.InitializeBossFight(boss);
        }
        else
        {
            BossCombatViewModel.ClearBossFight();
        }
    }

    /// <summary>
    /// v0.43.17: Adds a mechanic warning for a telegraphed boss ability.
    /// </summary>
    public void AddBossMechanicWarning(string abilityName, string description, int turnsRemaining, DangerLevel dangerLevel, bool canInterrupt)
    {
        BossCombatViewModel?.AddMechanicWarning(abilityName, description, turnsRemaining, dangerLevel, canInterrupt);
    }

    /// <summary>
    /// v0.43.17: Updates boss state after an action (HP change, phase transition, etc.).
    /// </summary>
    public void UpdateBossState()
    {
        if (BossCombatViewModel?.CurrentBoss != null)
        {
            BossCombatViewModel.UpdateBossHP();
            BossCombatViewModel.UpdateWarningTurns();
            this.RaisePropertyChanged(nameof(IsBossFight));
        }
    }

    /// <summary>
    /// v0.43.17: Called at the end of a turn to update boss enrage timer.
    /// </summary>
    public void OnBossTurnEnd()
    {
        BossCombatViewModel?.OnTurnEnd();
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
        var abilityNames = abilities.Select(a => $"{a.Name} (Stamina: {a.StaminaCost}, AP: {a.APCost})").ToList();
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
            // v0.44.4: Delegate to CombatController for proper flee handling
            if (_combatController != null)
            {
                await _combatController.OnFleeAttemptAsync();
            }
            else
            {
                // Fallback for demo mode without controller
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
            // Find player position from unit data
            var attackerPos = _unitData.FirstOrDefault(kvp => kvp.Value.IsPlayer).Key;
            if (attackerPos == default(GridPosition)) return;

            var targetHpBefore = target.HP;
            var logCountBefore = _combatState.CombatLog.Count;

            _combatEngine.PlayerAttack(_combatState, target);
            var damage = targetHpBefore - target.HP;
            var attackHit = damage > 0;

            // Check combat log for critical hit
            var recentLog = _combatState.CombatLog.Skip(logCountBefore).ToList();
            var wasCritical = recentLog.Any(log => log.Contains("CRITICAL", StringComparison.OrdinalIgnoreCase));

            // Trigger attack animation (fire-and-forget)
            _ = Task.Run(async () =>
            {
                await _animationService.PlayAttackAnimationAsync(
                    attackerPos,
                    position,
                    attackHit,
                    Math.Max(0, damage),
                    wasCritical);
            });

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

                var playerHpBefore = _combatState.Player.HP;
                var logCountBefore = _combatState.CombatLog.Count;

                // Determine and execute enemy action
                var action = _enemyAI.DetermineAction(currentEnemy);
                _enemyAI.ExecuteAction(currentEnemy, action, _combatState.Player, _combatState);

                // Trigger enemy attack animation if player took damage
                var playerHpAfter = _combatState.Player.HP;
                if (playerHpAfter < playerHpBefore && currentEnemy.Position.HasValue)
                {
                    var damage = playerHpBefore - playerHpAfter;
                    var playerPos = _unitData.FirstOrDefault(kvp => kvp.Value.IsPlayer).Key;

                    if (playerPos != default(GridPosition))
                    {
                        // Check combat log for critical hit
                        var recentLog = _combatState.CombatLog.Skip(logCountBefore).ToList();
                        var wasCritical = recentLog.Any(log => log.Contains("CRITICAL", StringComparison.OrdinalIgnoreCase));

                        // Trigger animation (fire-and-forget)
                        _ = Task.Run(async () =>
                        {
                            await _animationService.PlayAttackAnimationAsync(
                                currentEnemy.Position.Value,
                                playerPos,
                                true,
                                damage,
                                wasCritical);
                        });

                        // Brief delay to let animation start
                        Task.Delay(100).Wait();
                    }
                }

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
        bool playerDefeat = _combatState.Player.HP <= 0;

        // v0.44.4: Delegate to CombatController for proper phase transitions
        if (_combatController != null)
        {
            Task.Run(async () =>
            {
                if (playerVictory)
                {
                    await _combatController.OnCombatVictoryAsync();
                }
                else if (playerDefeat)
                {
                    await _combatController.OnCombatDefeatAsync();
                }
            });
        }
        else
        {
            // Fallback for demo mode without controller
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

        // Get player position from CombatState or find from occupied tiles
        GridPosition? playerPosition = _combatState.Player.Position;

        // If player has no position set, find from grid's occupied tiles
        if (!playerPosition.HasValue && _combatState.Grid.Tiles != null)
        {
            foreach (var kvp in _combatState.Grid.Tiles)
            {
                var tile = kvp.Value;
                if (tile.IsOccupied && tile.OccupantId == _combatState.Player.Name)
                {
                    playerPosition = tile.Position;
                    break;
                }
            }
        }

        // Add player
        if (playerPosition.HasValue)
        {
            var sprite = _spriteService.GetSpriteBitmap("warrior", 3);
            if (sprite != null)
            {
                var playerCombatant = new Combatant(_combatState.Player);
                Combatant.SetPlayerPositionStatic(playerPosition.Value);

                _unitSprites[playerPosition.Value] = sprite;
                _unitData[playerPosition.Value] = playerCombatant;
            }
        }

        // Add enemies (Note: Enemy uses Position property, not CurrentPosition)
        foreach (var enemy in _combatState.Enemies.Where(e => e.IsAlive && e.Position != null))
        {
            var spriteName = GetEnemySpriteNameFrom(enemy);
            var sprite = _spriteService.GetSpriteBitmap(spriteName, 3);
            if (sprite != null)
            {
                var enemyCombatant = new Combatant(enemy);
                _unitSprites[enemy.Position!.Value] = sprite;
                _unitData[enemy.Position!.Value] = enemyCombatant;
            }
        }

        // Add companions
        foreach (var companion in _combatState.Companions.Where(c => c.IsAlive && c.Position != null))
        {
            var sprite = _spriteService.GetSpriteBitmap("warrior", 3); // TODO: Companion-specific sprites
            if (sprite != null)
            {
                var companionCombatant = new Combatant(companion);
                _unitSprites[companion.Position!.Value] = sprite;
                _unitData[companion.Position!.Value] = companionCombatant;
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

    /// <summary>
    /// Adds a message to the combat log.
    /// </summary>
    public void AddToCombatLog(string message)
    {
        _combatState?.AddLogEntry(message);
        CombatLog.Insert(0, message);
        if (CombatLog.Count > 50)
        {
            CombatLog.RemoveAt(CombatLog.Count - 1);
        }
    }

    /// <summary>
    /// v0.38: Adds a flavored combat message using CombatFlavorTextService when available.
    /// </summary>
    public void AddFlavoredCombatLog(string baseMessage, string weaponType = "Melee", string outcome = "SolidHit", bool isPlayer = true, string? enemyArchetype = null)
    {
        string flavoredMessage = baseMessage;

        if (_combatFlavorService != null)
        {
            try
            {
                if (isPlayer)
                {
                    var flavorText = _combatFlavorService.GeneratePlayerAttackText(weaponType, outcome);
                    if (!string.IsNullOrEmpty(flavorText))
                    {
                        flavoredMessage = $"{flavorText} {baseMessage}";
                    }
                }
                else if (!string.IsNullOrEmpty(enemyArchetype))
                {
                    var flavorText = _combatFlavorService.GenerateEnemyAttackText(enemyArchetype, outcome);
                    if (!string.IsNullOrEmpty(flavorText))
                    {
                        flavoredMessage = $"{flavorText} {baseMessage}";
                    }
                }
            }
            catch
            {
                // Fall back to base message if flavor generation fails
            }
        }

        AddToCombatLog(flavoredMessage);
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
        foreach (var enemy in _combatState.Enemies.Where(e => e.IsAlive && e.Position != null))
        {
            _highlightedPositions.Add(enemy.Position!.Value);
        }

        this.RaisePropertyChanged(nameof(HighlightedPositions));
    }

    private void HighlightValidMovementPositions()
    {
        // TODO: Use AdvancedMovementService to calculate valid positions
        // For v0.43.5, show adjacent empty cells as placeholder
        _highlightedPositions.Clear();

        // Find player position from unit data
        var playerEntry = _unitData.FirstOrDefault(kvp => kvp.Value.IsPlayer);
        if (playerEntry.Value == null) return;

        var currentPos = playerEntry.Key;

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
            e.Position.HasValue &&
            e.Position.Value == position);
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
            HP = 45,
            MaxHP = 50,
            Attributes = new Attributes
            {
                Might = 4,
                Finesse = 3,
                Will = 3,
                Wits = 2,
                Sturdiness = 3
            }
        };

        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Goblin Scout", HP = 12, MaxHP = 12, Soak = 1, Attributes = new Attributes { Might = 2, Finesse = 3, Wits = 2, Will = 1, Sturdiness = 1 } },
            new Enemy { Name = "Goblin Warrior", HP = 15, MaxHP = 15, Soak = 2, Attributes = new Attributes { Might = 3, Finesse = 2, Wits = 1, Will = 2, Sturdiness = 2 } },
            new Enemy { Name = "Goblin Shaman", HP = 10, MaxHP = 10, Soak = 0, Attributes = new Attributes { Might = 1, Finesse = 2, Wits = 4, Will = 3, Sturdiness = 1 } }
        };

        var demoCombatState = _combatEngine.InitializeCombat(player, enemies, canFlee: true);

        LoadCombatState(demoCombatState);

        // Create demo environmental objects for v0.43.7 hazard visualization testing
        EnvironmentalObjects = new List<EnvironmentalObject>
        {
            // Fire hazard (animated) - Front Left Center
            new EnvironmentalObject
            {
                ObjectId = 1,
                Name = "Fire Pit",
                Description = "A roaring fire that damages anyone who enters",
                ObjectType = EnvironmentalObjectType.Hazard,
                IsHazard = true,
                DamageType = "Fire",
                DamageFormula = "2d6 Fire",
                StatusEffect = "[Burning]",
                GridPosition = "Front_Left_Column_1",
                State = EnvironmentalObjectState.Active
            },
            // Poison cloud - Front Right Center
            new EnvironmentalObject
            {
                ObjectId = 2,
                Name = "Toxic Cloud",
                Description = "A noxious cloud of poison gas",
                ObjectType = EnvironmentalObjectType.Hazard,
                IsHazard = true,
                DamageType = "Poison",
                DamageFormula = "1d8 Poison",
                StatusEffect = "[Poisoned]",
                GridPosition = "Front_Right_Column_1",
                State = EnvironmentalObjectState.Active
            },
            // Ice hazard - Back Left Center
            new EnvironmentalObject
            {
                ObjectId = 3,
                Name = "Frozen Ground",
                Description = "Slippery ice that slows movement",
                ObjectType = EnvironmentalObjectType.Hazard,
                IsHazard = true,
                DamageType = "Ice",
                DamageFormula = "1d6 Ice",
                StatusEffect = "[Slowed]",
                GridPosition = "Back_Left_Column_1",
                State = EnvironmentalObjectState.Active
            },
            // Lightning hazard (animated) - Back Right Center
            new EnvironmentalObject
            {
                ObjectId = 4,
                Name = "Electric Trap",
                Description = "Crackling electricity that shocks intruders",
                ObjectType = EnvironmentalObjectType.Hazard,
                IsHazard = true,
                DamageType = "Lightning",
                DamageFormula = "2d8 Lightning",
                StatusEffect = "[Stunned]",
                GridPosition = "Back_Right_Column_1",
                State = EnvironmentalObjectState.Active
            }
        };

        _log.Information("Demo combat scenario initialized with {HazardCount} environmental hazards", EnvironmentalObjects.Count);
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

