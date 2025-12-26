# v0.44.7: Victory & Endgame Transition

Type: Technical
Description: VictoryController for victory condition detection, run summary display, achievement unlocks, rewards calculation, and endgame mode selection (NG+, Challenge Sectors, Boss Gauntlet, Endless Mode).
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.44.3, v0.40 (Endgame Content), v0.41 (Meta-Progression)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.44: Game Flow Integration & Controllers (v0%2044%20Game%20Flow%20Integration%20&%20Controllers%200f28bd7b1ab1400fb9cc0377e89bb095.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.44.3, v0.40 (Endgame Content), v0.41 (Meta-Progression)

**Estimated Time:** 6-8 hours

**Phase:** End Conditions

**Deliverable:** Victory flow with run summary and endgame mode selection

---

## Executive Summary

v0.44.7 implements the VictoryController that handles dungeon completion, displays victory screens with run summaries, awards achievements, and transitions to endgame mode selection (NG+, Challenge Sectors, etc.).

**What This Delivers:**

- VictoryController implementation
- Victory condition detection (boss defeated, dungeon completed)
- Victory screen with run summary
- Rewards calculation and display
- Achievement unlock system
- Transition to endgame mode selection
- Integration with v0.40 endgame modes
- Integration with v0.41 meta-progression

**Success Metric:** Clearing the Sector displays victory screen with saga summary (trauma endured, Legend earned, Undying defeated), awards achievements, and transitions to endgame mode selection.

---

## Service Implementation

### VictoryController

```csharp
using RuneAndRust.Core;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using RuneAndRust.DesktopUI.ViewModels;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

public class VictoryController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly IMetaProgressionService _metaProgressionService;
    private readonly IAchievementService _achievementService;
    private readonly IRewardCalculationService _rewardService;
    private readonly INavigationService _navigationService;
    
    public VictoryController(
        ILogger logger,
        GameStateController gameStateController,
        IMetaProgressionService metaProgressionService,
        IAchievementService achievementService,
        IRewardCalculationService rewardService,
        INavigationService navigationService)
    {
        _logger = logger;
        _gameStateController = gameStateController;
        _metaProgressionService = metaProgressionService;
        _achievementService = achievementService;
        _rewardService = rewardService;
        _navigationService = navigationService;
    }
    
    public async Task CheckVictoryConditionAsync()
    {
        var gameState = _gameStateController.CurrentGameState;
        
        // Check if final boss defeated or dungeon completed
        bool dungeonCompleted = CheckDungeonCompletion(gameState);
        
        if (dungeonCompleted)
        {
            await HandleVictoryAsync();
        }
    }
    
    private bool CheckDungeonCompletion(GameState gameState)
    {
        var dungeon = gameState.CurrentDungeon;
        if (dungeon == null) return false;
        
        // Check if final boss room reached and boss defeated
        if (dungeon.FinalBossRoom != null)
        {
            return dungeon.FinalBossRoom.EncounterDefeated;
        }
        
        // Alternative: Check if reached maximum depth
        if (gameState.CurrentRoom?.Depth >= dungeon.MaxDepth)
        {
            return true;
        }
        
        return false;
    }
    
    public async Task HandleVictoryAsync()
    {
        var player = _gameStateController.CurrentGameState.Player;
        
        _logger.Information("Victory! {Name} completed the dungeon at Legend {Legend}",
            [player.Name](http://player.Name),
            player.Legend);
        
        // Create victory snapshot
        var victorySnapshot = _gameStateController.CurrentGameState.CreateSnapshot();
        
        try
        {
            // Update phase
            await _gameStateController.UpdatePhaseAsync(GamePhase.Victory, "Dungeon completed");
            
            // Calculate run statistics
            var runStats = CalculateVictoryStatistics(victorySnapshot);
            
            // Calculate rewards
            var rewards = await _rewardService.CalculateVictoryRewardsAsync(runStats);
            
            // Record in meta-progression
            await _metaProgressionService.RecordRunEndAsync(victorySnapshot, wasVictory: true);
            
            // Check for achievements
            var newAchievements = await _achievementService.CheckAchievementsAsync(runStats);
            
            // Unlock new content based on completion
            var unlockedContent = await _metaProgressionService.UnlockContentAsync(runStats);
            
            // Show victory screen
            await ShowVictoryScreenAsync(runStats, rewards, newAchievements, unlockedContent);
            
            // Transition to endgame menu
            await TransitionToEndgameMenuAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling victory");
            throw;
        }
    }
    
    private RunStatistics CalculateVictoryStatistics(GameStateSnapshot snapshot)
    {
        var gameState = _gameStateController.CurrentGameState;
        
        return new RunStatistics
        {
            CharacterName = [snapshot.PlayerSnapshot.Name](http://snapshot.PlayerSnapshot.Name),
            Archetype = snapshot.PlayerSnapshot.Archetype,
            Specialization = snapshot.PlayerSnapshot.Specialization,
            FinalLegend = snapshot.PlayerSnapshot.Legend,
            MaxDepthReached = gameState.CurrentRoom?.Depth ?? 0,
            TotalPlayTime = gameState.PlayTime,
            EnemiesDefeated = gameState.Player.EnemiesDefeated,
            DamageDealt = gameState.Player.TotalDamageDealt,
            DamageTaken = gameState.Player.TotalDamageTaken,
            ItemsCollected = gameState.Player.Inventory.Count,
            RoomsExplored = gameState.CurrentDungeon?.Rooms.Count(r => r.IsExplored) ?? 0,
            DeathsAvoided = 1, // Completed without dying
            WasVictory = true,
            CompletionTime = gameState.PlayTime,
            NGPlusTier = gameState.CurrentNGPlusTier ?? 0,
            ChallengeSector = gameState.CurrentChallengeSector?.Name
        };
    }
    
    private async Task ShowVictoryScreenAsync(
        RunStatistics stats,
        VictoryRewards rewards,
        List<Achievement> newAchievements,
        List<UnlockedContent> unlockedContent)
    {
        _logger.Information("Displaying victory screen");
        
        var victoryViewModel = new VictoryScreenViewModel
        {
            CharacterName = stats.CharacterName,
            FinalLegend = stats.FinalLegend,
            RunDuration = stats.TotalPlayTime,
            Statistics = stats,
            Rewards = rewards,
            NewAchievements = new ObservableCollection<Achievement>(newAchievements),
            UnlockedContent = new ObservableCollection<UnlockedContent>(unlockedContent)
        };
        
        // Navigate to victory screen
        await _navigationService.NavigateTo<VictoryScreenViewModel>();
        
        // Wait for player to acknowledge
        await victoryViewModel.WaitForAcknowledgmentAsync();
    }
    
    private async Task TransitionToEndgameMenuAsync()
    {
        _logger.Information("Transitioning to endgame menu");
        
        await _gameStateController.UpdatePhaseAsync(GamePhase.EndgameMenu, "Victory acknowledged");
        
        // Navigate to endgame mode selection
        await _navigationService.NavigateTo<EndgameModeViewModel>();
        
        // Initialize endgame menu with available modes
        var endgameViewModel = _navigationService.CurrentView as EndgameModeViewModel;
        await InitializeEndgameMenuAsync(endgameViewModel!);
    }
    
    private async Task InitializeEndgameMenuAsync(EndgameModeViewModel viewModel)
    {
        var player = _gameStateController.CurrentGameState.Player;
        
        // Load available endgame modes based on unlocks
        var availableModes = await _metaProgressionService.GetAvailableEndgameModesAsync(player);
        
        viewModel.AvailableModes = new ObservableCollection<EndgameMode>(availableModes);
        
        // Load NG+ progression
        var ngPlusTier = await _metaProgressionService.GetUnlockedNGPlusTierAsync(player);
        viewModel.MaxNGPlusTier = ngPlusTier;
        
        // Load unlocked challenge sectors
        var challengeSectors = await _metaProgressionService.GetUnlockedChallengeSectorsAsync(player);
        viewModel.AvailableChallengeSectors = new ObservableCollection<ChallengeSector>(challengeSectors);
        
        _logger.Information("Endgame menu initialized: NG+ Tier {Tier}, {SectorCount} challenge sectors",
            ngPlusTier, challengeSectors.Count);
    }
    
    public async Task OnSelectEndgameModeAsync(EndgameMode mode)
    {
        _logger.Information("Player selected endgame mode: {Mode}", [mode.Name](http://mode.Name));
        
        switch (mode.Type)
        {
            case EndgameModeType.NewGamePlus:
                await StartNewGamePlusAsync(mode.SelectedTier);
                break;
            
            case EndgameModeType.ChallengeSector:
                await StartChallengeSectorAsync(mode.SelectedSector!);
                break;
            
            case EndgameModeType.BossGauntlet:
                await StartBossGauntletAsync();
                break;
            
            case EndgameModeType.EndlessMode:
                await StartEndlessModeAsync();
                break;
            
            default:
                _logger.Warning("Unknown endgame mode type: {Type}", mode.Type);
                break;
        }
    }
    
    private async Task StartNewGamePlusAsync(int tier)
    {
        _logger.Information("Starting NG+ Tier {Tier}", tier);
        
        // Reset dungeon but keep character progression
        var player = _gameStateController.CurrentGameState.Player;
        
        // Generate new dungeon with increased difficulty
        var dungeon = await _dungeonGenerator.GenerateAsync(
            player.Legend,
            ngPlusTier: tier);
        
        // Update game state
        _gameStateController.CurrentGameState.CurrentNGPlusTier = tier;
        _gameStateController.CurrentGameState.CurrentDungeon = dungeon;
        _gameStateController.CurrentGameState.CurrentRoom = dungeon.StartingRoom;
        
        // Start exploration
        await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration, $"NG+ Tier {tier} started");
        
        var explorationController = _serviceProvider.GetRequiredService<ExplorationController>();
        await explorationController.BeginExplorationAsync();
    }
    
    private async Task StartChallengeSectorAsync(ChallengeSector sector)
    {
        _logger.Information("Starting Challenge Sector: {Name}", [sector.Name](http://sector.Name));
        
        // Similar to NG+ but with specific modifiers
        var player = _gameStateController.CurrentGameState.Player;
        
        // Generate challenge sector dungeon
        var dungeon = await _dungeonGenerator.GenerateChallengeSectorAsync(
            sector,
            player.Legend);
        
        // Update game state
        _gameStateController.CurrentGameState.CurrentChallengeSector = sector;
        _gameStateController.CurrentGameState.CurrentDungeon = dungeon;
        _gameStateController.CurrentGameState.CurrentRoom = dungeon.StartingRoom;
        
        // Start exploration
        await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration, $"Challenge: {[sector.Name](http://sector.Name)}");
        
        var explorationController = _serviceProvider.GetRequiredService<ExplorationController>();
        await explorationController.BeginExplorationAsync();
    }
    
    private async Task StartBossGauntletAsync()
    {
        _logger.Information("Starting Boss Gauntlet");
        
        // Create boss gauntlet encounter sequence
        var bossGauntlet = await _encounterService.GenerateBossGauntletAsync(
            _gameStateController.CurrentGameState.Player.Legend);
        
        // Start first boss fight
        var combatController = _serviceProvider.GetRequiredService<CombatController>();
        await combatController.StartCombatAsync(bossGauntlet.Bosses.First());
    }
    
    private async Task StartEndlessModeAsync()
    {
        _logger.Information("Starting Endless Mode");
        
        // Initialize endless mode state
        var endlessMode = new EndlessModeState
        {
            CurrentWave = 1,
            Score = 0,
            StartTime = DateTime.UtcNow
        };
        
        _gameStateController.CurrentGameState.EndlessMode = endlessMode;
        
        // Start first wave
        await StartEndlessWaveAsync(1);
    }
    
    public async Task OnReturnToMainMenuAsync()
    {
        _logger.Information("Returning to main menu from endgame");
        
        // Save final state
        await _gameStateController.AutoSaveAsync();
        
        // Reset game state
        _gameStateController.Reset();
        
        // Navigate to main menu
        await _navigationService.NavigateTo<MainMenuViewModel>();
    }
}
```

---

## Integration Points

**With v0.44.3 (Exploration):**

- Victory condition checked after boss encounters
- Triggered when dungeon completion detected

**With v0.40 (Endgame Content):**

- Transitions to NG+ mode selection
- Starts Challenge Sectors
- Initiates Boss Gauntlet and Endless Mode

**With v0.41 (Meta-Progression):**

- Records run completion
- Awards achievements
- Unlocks new content based on performance

**With v0.43.16 (Endgame Mode Selection UI):**

- Populates EndgameModeViewModel with available modes
- Handles mode selection and configuration

---

## Functional Requirements

### FR1: Victory Detection

**Requirement:** Detect when player completes the dungeon.

**Test:**

```csharp
[Fact]
public void CheckVictoryCondition_WithFinalBossDefeated_ReturnsTrue()
{
    var dungeon = CreateTestDungeon();
    dungeon.FinalBossRoom.EncounterDefeated = true;
    
    var controller = CreateVictoryController();
    var result = controller.CheckDungeonCompletion(gameState);
    
    Assert.True(result);
}

[Fact]
public async Task CheckVictoryCondition_TriggersVictoryFlow()
{
    // Setup: Defeat final boss
    var controller = CreateVictoryController();
    
    await controller.CheckVictoryConditionAsync();
    
    Assert.Equal(GamePhase.Victory, _gameStateController.CurrentGameState.CurrentPhase);
}
```

### FR2: Run Statistics

**Requirement:** Calculate and display comprehensive run statistics.

**Test:**

```csharp
[Fact]
public void CalculateVictoryStatistics_IncludesAllMetrics()
{
    var controller = CreateVictoryController();
    var snapshot = _gameStateController.CurrentGameState.CreateSnapshot();
    
    var stats = controller.CalculateVictoryStatistics(snapshot);
    
    Assert.True(stats.WasVictory);
    Assert.True(stats.FinalLegend > 0);
    Assert.True(stats.EnemiesDefeated >= 0);
    Assert.NotNull(stats.CharacterName);
}
```

### FR3: Achievement Unlocks

**Requirement:** Check and award achievements on victory.

**Test:**

```csharp
[Fact]
public async Task HandleVictory_ChecksAchievements()
{
    var controller = CreateVictoryController();
    
    await controller.HandleVictoryAsync();
    
    // Verify achievement service was called
    _achievementService.Verify(s => s.CheckAchievementsAsync(It.IsAny<RunStatistics>()), Times.Once);
}
```

### FR4: Endgame Mode Selection

**Requirement:** Allow selection of endgame modes after victory.

**Test:**

```csharp
[Fact]
public async Task TransitionToEndgameMenu_LoadsAvailableModes()
{
    var controller = CreateVictoryController();
    
    await controller.TransitionToEndgameMenuAsync();
    
    Assert.IsType<EndgameModeViewModel>(_navigationService.CurrentView);
    Assert.NotEmpty(((EndgameModeViewModel)_navigationService.CurrentView).AvailableModes);
}

[Fact]
public async Task SelectNGPlus_StartsNewRun()
{
    var controller = CreateVictoryController();
    
    await controller.OnSelectEndgameModeAsync(new EndgameMode { Type = EndgameModeType.NewGamePlus, SelectedTier = 1 });
    
    Assert.Equal(GamePhase.DungeonExploration, _gameStateController.CurrentGameState.CurrentPhase);
    Assert.Equal(1, _gameStateController.CurrentGameState.CurrentNGPlusTier);
}
```

---

## Testing Requirements

### Unit Tests (Target: 80%+ coverage)

**VictoryController Tests:**

- Victory condition detection
- Run statistics calculation
- Achievement checking
- Reward calculation
- Endgame mode initialization
- Mode selection handling

### Integration Tests

**End-to-End Victory Flow:**

```csharp
[Fact]
public async Task FullVictoryFlow_FromBossDefeatToNGPlus()
{
    // Setup: Complete dungeon
    var explorationController = CreateExplorationController();
    var combatController = CreateCombatController();
    var victoryController = CreateVictoryController();
    
    // Defeat final boss
    await combatController.HandleCombatVictoryAsync();
    
    // Victory detected
    await victoryController.CheckVictoryConditionAsync();
    
    // Should be at victory screen
    Assert.Equal(GamePhase.Victory, _gameStateController.CurrentGameState.CurrentPhase);
    
    // Transition to endgame menu
    await victoryController.TransitionToEndgameMenuAsync();
    
    // Select NG+
    await victoryController.OnSelectEndgameModeAsync(new EndgameMode { Type = EndgameModeType.NewGamePlus, SelectedTier = 1 });
    
    // Should start new exploration
    Assert.Equal(GamePhase.DungeonExploration, _gameStateController.CurrentGameState.CurrentPhase);
    Assert.Equal(1, _gameStateController.CurrentGameState.CurrentNGPlusTier);
}
```

---

## Success Criteria

**v0.44.7 is DONE when:**

### ✅ Victory Detection

- [ ]  Dungeon completion detected
- [ ]  Final boss defeat triggers victory
- [ ]  Alternative victory conditions work
- [ ]  No false positives

### ✅ Victory Screen

- [ ]  Run statistics displayed
- [ ]  Rewards shown
- [ ]  New achievements unlocked
- [ ]  Content unlocks displayed
- [ ]  Can acknowledge and proceed

### ✅ Endgame Modes

- [ ]  Endgame menu shows available modes
- [ ]  NG+ tier selection works
- [ ]  Challenge Sector selection works
- [ ]  Boss Gauntlet starts correctly
- [ ]  Endless Mode initializes

### ✅ Meta-Progression

- [ ]  Run recorded in database
- [ ]  Achievements saved
- [ ]  Unlocks persisted
- [ ]  Statistics updated

### ✅ Flow

- [ ]  Victory → Rewards → Endgame Selection
- [ ]  Can start new NG+ run
- [ ]  Can return to main menu
- [ ]  No state corruption

---

**Victory flow complete! v0.44 fully specified - GUI now playable from start to finish.**