# v0.44.5: Progression & Loot Controllers

Type: Technical
Description: LootController and ProgressionController for post-combat loot collection, Milestone PP spending, attribute allocation, and ability unlocks.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.44.4, v0.43.9-v0.43.11, v0.10 (Progression)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.44: Game Flow Integration & Controllers (v0%2044%20Game%20Flow%20Integration%20&%20Controllers%200f28bd7b1ab1400fb9cc0377e89bb095.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.44.4, v0.43.9-v0.43.11, v0.10 (Progression)

**Estimated Time:** 4-6 hours

**Phase:** Core Game Loop

**Deliverable:** Post-combat loot collection and level-up workflows

---

## Executive Summary

v0.44.5 implements the controllers for post-combat rewards: loot collection and character progression. Handles level-up attribute allocation and ability unlocks.

**What This Delivers:**

- LootController for item collection
- ProgressionController for level-up flow
- Attribute point allocation UI coordination
- Ability unlock/rank-up coordination
- Return to exploration after rewards

**Success Metric:** Can collect loot and spend Progression Points at Milestones (when Legend bar fills), unlocking abilities through the Saga System.

---

## Service Implementation

### LootController

```csharp
using RuneAndRust.Core.Items;
using RuneAndRust.DesktopUI.ViewModels;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

public class LootController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly INavigationService _navigationService;
    private readonly ExplorationController _explorationController;
    private readonly ProgressionController _progressionController;
    
    public LootController(
        ILogger logger,
        GameStateController gameStateController,
        INavigationService navigationService,
        ExplorationController explorationController,
        ProgressionController progressionController)
    {
        _logger = logger;
        _gameStateController = gameStateController;
        _navigationService = navigationService;
        _explorationController = explorationController;
        _progressionController = progressionController;
    }
    
    public async Task ShowLootCollectionAsync(List<Item> loot, bool leveledUp)
    {
        _logger.Information("Showing loot collection: {ItemCount} items, LeveledUp: {LeveledUp}",
            loot.Count, leveledUp);
        
        // Create loot collection ViewModel
        var lootViewModel = new LootCollectionViewModel
        {
            LootItems = new ObservableCollection<ItemViewModel>([loot.Select](http://loot.Select)(i => new ItemViewModel(i))),
            CanProceed = true
        };
        
        // Show loot screen (could be modal or full view)
        await _navigationService.ShowModalAsync(lootViewModel);
        
        // Wait for user to collect all items
        await lootViewModel.WaitForCollectionCompleteAsync();
        
        // Add items to player inventory
        var player = _gameStateController.CurrentGameState.Player;
        foreach (var item in loot)
        {
            player.Inventory.Add(item);
            _logger.Debug("Added {ItemName} to inventory", [item.Name](http://item.Name));
        }
        
        // If leveled up, show progression screen
        if (leveledUp)
        {
            await _progressionController.ShowLevelUpScreenAsync();
        }
        else
        {
            // Return to exploration
            await ReturnToExplorationAsync();
        }
    }
    
    private async Task ReturnToExplorationAsync()
    {
        _logger.Information("Returning to exploration after loot collection");
        
        await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration, "Loot collected");
        _gameStateController.CurrentGameState.CurrentCombat = null;
        
        await _explorationController.OnReturnFromCombatAsync(wasVictory: true);
    }
}

### ProgressionController

```

using RuneAndRust.Core.Characters;

using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);

using RuneAndRust.DesktopUI.ViewModels;

using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

public class ProgressionController

{

private readonly ILogger _logger;

private readonly GameStateController _gameStateController;

private readonly ICharacterProgressionService _progressionService;

private readonly INavigationService _navigationService;

private readonly ExplorationController _explorationController;

public ProgressionController(

ILogger logger,

GameStateController gameStateController,

ICharacterProgressionService progressionService,

INavigationService navigationService,

ExplorationController explorationController)

{

_logger = logger;

_gameStateController = gameStateController;

_progressionService = progressionService;

_navigationService = navigationService;

_explorationController = explorationController;

}

public async Task ShowLootCollectionAsync(List<Item> loot, bool leveledUp)

{

var lootController = new LootController(

_logger,

_gameStateController,

_navigationService,

_explorationController,

this);

await lootController.ShowLootCollectionAsync(loot, leveledUp);

}

public async Task ShowLevelUpScreenAsync()

{

var player = _gameStateController.CurrentGameState.Player;

_logger.Information("Showing level up screen: Legend {Legend}", player.Legend);

await _gameStateController.UpdatePhaseAsync(GamePhase.CharacterProgression, "Level up");

// Calculate available points

var attributePoints = _progressionService.GetAttributePointsForLevel(player.Legend);

var progressionPoints = _progressionService.GetProgressionPointsForLevel(player.Legend);

// Create progression ViewModel

var progressionViewModel = new LevelUpViewModel

{

CurrentLegend = player.Legend,

AttributePointsAvailable = attributePoints,

ProgressionPointsAvailable = progressionPoints,

Player = player

};

// Show level up screen

await _navigationService.ShowModalAsync(progressionViewModel);

// Wait for user to allocate points

await progressionViewModel.WaitForAllocationCompleteAsync();

// Apply changes

_progressionService.RecalculateDerivedStats(player);

_logger.Information("Level up complete: {AttributePoints} attribute points, {PP} PP spent",

attributePoints, progressionPoints);

// Return to exploration

await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration, "Progression complete");

await _explorationController.OnReturnFromCombatAsync(wasVictory: true);

}

public async Task OnAttributeIncreasedAsync(string attributeName)

{

var player = _gameStateController.CurrentGameState.Player;

_logger.Information("Player increasing {Attribute}", attributeName);

// Validation handled by ViewModel

// Apply increase (already done by ViewModel binding)

// Recalculate derived stats

_progressionService.RecalculateDerivedStats(player);

}

public async Task OnAbilityUnlockedAsync(string abilityId)

{

var player = _gameStateController.CurrentGameState.Player;

_logger.Information("Player unlocking ability: {AbilityId}", abilityId);

// Unlock ability through service

await _progressionService.UnlockAbilityAsync(player, abilityId);

}

public async Task OnAbilityRankedUpAsync(string abilityId)

{

var player = _gameStateController.CurrentGameState.Player;

_logger.Information("Player ranking up ability: {AbilityId}", abilityId);

// Rank up ability through service

await _progressionService.RankUpAbilityAsync(player, abilityId);

}

}

```

---

## Success Criteria

**v0.44.5 is DONE when:**

### ✅ Loot Collection
- [ ] Loot screen shows after victory
- [ ] Can view all loot items
- [ ] Items added to inventory
- [ ] Proceeds to level up if applicable

### ✅ Level Up
- [ ] Level up screen shows
- [ ] Can allocate attribute points
- [ ] Can unlock abilities with PP
- [ ] Can rank up abilities
- [ ] Derived stats recalculate

### ✅ Flow
- [ ] Loot → Level up → Exploration
- [ ] Returns to exploration correctly
- [ ] No progression if didn't level

---

**Progression complete. Ready for death handling in v0.44.6.**
```