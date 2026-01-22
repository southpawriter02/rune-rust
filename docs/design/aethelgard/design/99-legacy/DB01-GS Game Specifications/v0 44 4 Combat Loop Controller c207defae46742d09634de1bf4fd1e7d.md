# v0.44.4: Combat Loop Controller

Type: Technical
Description: CombatController for combat initialization, turn management, action execution, enemy AI coordination, and combat end detection (victory/defeat/flee).
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.44.3, v0.43.4-v0.43.8, v0.1-v0.23 (Combat Systems)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.44: Game Flow Integration & Controllers (v0%2044%20Game%20Flow%20Integration%20&%20Controllers%200f28bd7b1ab1400fb9cc0377e89bb095.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.44.3, v0.43.4-v0.43.8, v0.1-v0.23 (Combat Systems)

**Estimated Time:** 6-8 hours

**Phase:** Core Game Loop

**Deliverable:** Complete combat loop from encounter start to loot/death

---

## Executive Summary

v0.44.4 implements the CombatController that orchestrates combat encounters. Coordinates turn management, action execution, enemy AI, and transitions to victory/defeat/loot.

**What This Delivers:**

- CombatController implementation
- Combat initialization from encounters
- Player turn management
- Enemy turn management with AI
- Action execution coordination
- Combat end detection (victory/defeat/flee)
- Transition to loot or death
- Integration with v0.43.4-v0.43.8 combat UI

**Success Metric:** Can engage Undying/Jötun-Forged from encounter start to conclusion, with proper turn order, trauma accumulation (Psychic Stress/Corruption), Legend awards, and transitions.

---

## Service Implementation

### CombatController

```csharp
using RuneAndRust.Core.Combat;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using RuneAndRust.DesktopUI.ViewModels;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

public class CombatController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly ICombatEngine _combatEngine;
    private readonly IEnemyAIService _enemyAIService;
    private readonly INavigationService _navigationService;
    private readonly DeathController _deathController;
    private readonly ProgressionController _progressionController;
    
    private CombatViewModel? _viewModel;
    private bool _isProcessingTurn = false;
    
    public CombatController(
        ILogger logger,
        GameStateController gameStateController,
        ICombatEngine combatEngine,
        IEnemyAIService enemyAIService,
        INavigationService navigationService,
        DeathController deathController,
        ProgressionController progressionController)
    {
        _logger = logger;
        _gameStateController = gameStateController;
        _combatEngine = combatEngine;
        _enemyAIService = enemyAIService;
        _navigationService = navigationService;
        _deathController = deathController;
        _progressionController = progressionController;
    }
    
    public async Task StartCombatAsync(CombatEncounter encounter)
    {
        _logger.Information("Starting combat: {EnemyCount} enemies", encounter.Enemies.Count);
        
        try
        {
            // Initialize combat state
            var player = _gameStateController.CurrentGameState.Player;
            var combatState = await _combatEngine.InitializeCombatAsync(player, encounter);
            
            // Update game state
            _gameStateController.CurrentGameState.CurrentCombat = combatState;
            await _gameStateController.UpdatePhaseAsync(GamePhase.Combat, "Encounter started");
            
            // Navigate to combat view
            await _navigationService.NavigateTo<CombatViewModel>();
            
            // Load combat in ViewModel
            _viewModel = _navigationService.CurrentView as CombatViewModel;
            _viewModel!.LoadCombatState(combatState);
            
            // Start first turn
            await StartNextTurnAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error starting combat");
            throw;
        }
    }
    
    public void Initialize(CombatViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    
    private async Task StartNextTurnAsync()
    {
        var combatState = _gameStateController.CurrentGameState.CurrentCombat!;
        
        // Get next combatant from turn order
        var currentCombatant = combatState.TurnOrder[combatState.CurrentTurnIndex];
        
        _logger.Information("Turn {Turn}: {Name} ({IsPlayer})",
            combatState.TurnNumber,
            [currentCombatant.Name](http://currentCombatant.Name),
            currentCombatant.IsPlayer ? "Player" : "Enemy");
        
        _viewModel!.UpdateTurnDisplay(currentCombatant);
        
        if (currentCombatant.IsPlayer)
        {
            await StartPlayerTurnAsync();
        }
        else
        {
            await StartEnemyTurnAsync(currentCombatant as Enemy);
        }
    }
    
    private async Task StartPlayerTurnAsync()
    {
        _logger.Debug("Player turn started");
        
        // Enable action buttons
        _viewModel!.EnableActionButtons(true);
        
        // Wait for player input (handled by OnPlayerActionAsync)
    }
    
    private async Task OnPlayerActionAsync(CombatAction action)
    {
        if (_isProcessingTurn) return;
        
        _isProcessingTurn = true;
        
        try
        {
            _logger.Information("Player action: {ActionType}", action.Type);
            
            // Disable action buttons during processing
            _viewModel!.EnableActionButtons(false);
            
            // Execute action through combat engine
            var result = await _combatEngine.ProcessPlayerActionAsync(action);
            
            // Update ViewModel with result
            _viewModel.UpdateFromActionResult(result);
            
            // Play animation
            await _viewModel.AnimationService.PlayActionAnimationAsync(result);
            
            // Add to combat log
            _viewModel.AddToCombatLog(result.LogMessage);
            
            // Check if combat ended
            if (await CheckCombatEndConditionAsync())
                return;
            
            // End turn
            await EndCurrentTurnAsync();
        }
        finally
        {
            _isProcessingTurn = false;
        }
    }
    
    private async Task StartEnemyTurnAsync(Enemy enemy)
    {
        _logger.Debug("Enemy turn started: {Name}", [enemy.Name](http://enemy.Name));
        
        // Slight delay for player to read
        await Task.Delay(500);
        
        try
        {
            // Get AI decision
            var action = await _enemyAIService.DecideActionAsync(
                enemy,
                _gameStateController.CurrentGameState.CurrentCombat!);
            
            _logger.Information("Enemy {Name} action: {ActionType} targeting {Target}",
                [enemy.Name](http://enemy.Name),
                action.Type,
                action.TargetPosition);
            
            // Execute action
            var result = await _combatEngine.ProcessEnemyActionAsync(action);
            
            // Update ViewModel
            _viewModel!.UpdateFromActionResult(result);
            
            // Play animation
            await _viewModel.AnimationService.PlayActionAnimationAsync(result);
            
            // Add to combat log
            _viewModel.AddToCombatLog(result.LogMessage);
            
            // Check if player died
            if (_gameStateController.CurrentGameState.Player.CurrentHP <= 0)
            {
                await _deathController.HandlePlayerDeathAsync();
                return;
            }
            
            // Check if combat ended
            if (await CheckCombatEndConditionAsync())
                return;
            
            // End turn
            await EndCurrentTurnAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing enemy turn");
            // Continue to next turn
            await EndCurrentTurnAsync();
        }
    }
    
    private async Task EndCurrentTurnAsync()
    {
        var combatState = _gameStateController.CurrentGameState.CurrentCombat!;
        
        // Advance turn index
        combatState.CurrentTurnIndex++;
        
        // If reached end of turn order, start new round
        if (combatState.CurrentTurnIndex >= combatState.TurnOrder.Count)
        {
            combatState.CurrentTurnIndex = 0;
            combatState.TurnNumber++;
            
            _logger.Debug("Starting round {Round}", combatState.TurnNumber);
            
            // Process round-based effects (DoT, buffs expiring, etc.)
            await _combatEngine.ProcessRoundEndEffectsAsync(combatState);
        }
        
        // Start next turn
        await StartNextTurnAsync();
    }
    
    private async Task<bool> CheckCombatEndConditionAsync()
    {
        var combatState = _gameStateController.CurrentGameState.CurrentCombat!;
        
        // Check for victory (all enemies dead)
        bool allEnemiesDead = combatState.Enemies.All(e => e.CurrentHP <= 0);
        if (allEnemiesDead)
        {
            await HandleCombatVictoryAsync();
            return true;
        }
        
        // Check for defeat (player dead)
        if (combatState.Player.CurrentHP <= 0)
        {
            await _deathController.HandlePlayerDeathAsync();
            return true;
        }
        
        return false;
    }
    
    private async Task HandleCombatVictoryAsync()
    {
        _logger.Information("Combat victory!");
        
        var combatState = _gameStateController.CurrentGameState.CurrentCombat!;
        
        // Calculate rewards
        var xpGained = combatState.Enemies.Sum(e => e.XPReward);
        var loot = await _combatEngine.GenerateLootAsync(combatState.Enemies);
        
        _logger.Information("Victory rewards: {XP} XP, {ItemCount} items", xpGained, loot.Count);
        
        // Award XP
        var player = _gameStateController.CurrentGameState.Player;
        player.CurrentXP += xpGained;
        
        // Check for level up
        bool leveledUp = false;
        while (player.CurrentXP >= player.XPForNextLevel)
        {
            leveledUp = true;
            player.Legend++;
            player.CurrentXP -= player.XPForNextLevel;
            _logger.Information("Level up! Now Legend {Legend}", player.Legend);
        }
        
        // Show victory screen with loot
        _viewModel!.ShowVictoryScreen(xpGained, loot, leveledUp);
        
        // Transition to loot collection
        await _progressionController.ShowLootCollectionAsync(loot, leveledUp);
    }
    
    public async Task OnFleeAsync()
    {
        _logger.Information("Player attempting to flee");
        
        // Roll flee check
        var fleeRoll = [Random.Shared.Next](http://Random.Shared.Next)(1, 101);
        var fleeChance = 50; // Base 50% chance
        
        if (fleeRoll <= fleeChance)
        {
            _logger.Information("Flee successful (rolled {Roll} <= {Chance})", fleeRoll, fleeChance);
            
            _viewModel!.ShowMessage("You successfully flee from combat!");
            
            // Return to exploration
            await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration, "Fled from combat");
            _gameStateController.CurrentGameState.CurrentCombat = null;
            
            var explorationController = _serviceProvider.GetRequiredService<ExplorationController>();
            await explorationController.OnReturnFromCombatAsync(wasVictory: false);
        }
        else
        {
            _logger.Information("Flee failed (rolled {Roll} > {Chance})", fleeRoll, fleeChance);
            
            _viewModel!.ShowMessage("You fail to escape! Enemies get a free attack.");
            
            // Enemies get opportunity attacks
            // (Implementation depends on combat rules)
            
            // Continue combat
            await EndCurrentTurnAsync();
        }
    }
}
```

---

## Success Criteria

**v0.44.4 is DONE when:**

### ✅ Combat Initialization

- [ ]  Encounters start combat correctly
- [ ]  Combat state initialized
- [ ]  Turn order calculated
- [ ]  Navigates to combat view

### ✅ Turn Management

- [ ]  Player and enemy turns alternate
- [ ]  Turn order displayed
- [ ]  Action buttons enabled on player turn
- [ ]  Enemy AI executes actions

### ✅ Combat Resolution

- [ ]  Victory detected when all enemies dead
- [ ]  Defeat detected when player dead
- [ ]  XP and loot awarded
- [ ]  Level up detection works

### ✅ Transitions

- [ ]  Transitions to loot after victory
- [ ]  Transitions to death screen after defeat
- [ ]  Flee returns to exploration

---

**Combat loop complete. Ready for progression controller in v0.44.5.**