# v0.44.3: Exploration Loop Controller

Type: Technical
Description: ExplorationController for Sector navigation, room movement, search actions, rest handling, random encounter triggers, and minimap updates.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.44.2, v0.43.12-v0.43.14, v0.4-v0.5 (Dungeon Systems)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.44: Game Flow Integration & Controllers (v0%2044%20Game%20Flow%20Integration%20&%20Controllers%200f28bd7b1ab1400fb9cc0377e89bb095.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.44.2, v0.43.12-v0.43.14, v0.4-v0.5 (Dungeon Systems)

**Estimated Time:** 6-8 hours

**Phase:** Core Game Loop

**Deliverable:** Functional dungeon exploration with room navigation and encounters

---

## Executive Summary

v0.44.3 implements the ExplorationController that manages the dungeon exploration loop - the core gameplay outside of combat. Handles room navigation, searching, resting, and triggering encounters.

**What This Delivers:**

- ExplorationController implementation
- Room navigation (N/S/E/W/U/D)
- Search action coordination
- Rest/camp handling
- Random encounter triggers
- Minimap updates
- Transition to combat when encounters occur
- Integration with v0.43.12-v0.43.14 exploration UI

**Success Metric:** Can navigate dungeon rooms, search for loot, rest, and trigger combat encounters.

---

## Service Implementation

### ExplorationController (Complete Implementation)

```csharp
using RuneAndRust.Core.Dungeon;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using RuneAndRust.DesktopUI.ViewModels;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// Orchestrates Sector navigation with Aethelgard terminology.
/// Survivor explores rooms, searches for loot, rests at field camps, encounters Undying.
/// </summary>
public class ExplorationController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly IDungeonGenerator _dungeonGenerator;
    private readonly IEncounterService _encounterService;
    private readonly IRoomFeatureService _roomFeatureService;
    private readonly INavigationService _navigationService;
    private readonly CombatController _combatController;
    private DungeonExplorationViewModel _viewModel = null!;
    
    public ExplorationController(ILogger logger, GameStateController gameStateController,
        IDungeonGenerator dungeonGenerator, IEncounterService encounterService,
        IRoomFeatureService roomFeatureService, INavigationService navigationService,
        CombatController combatController)
    {
        _logger = logger;
        _gameStateController = gameStateController;
        _dungeonGenerator = dungeonGenerator;
        _encounterService = encounterService;
        _roomFeatureService = roomFeatureService;
        _navigationService = navigationService;
        _combatController = combatController;
    }
    
    public void Initialize(DungeonExplorationViewModel viewModel)
    {
        _viewModel = viewModel;
        _logger.Information("Sector navigation initialized: Survivor {Name} at depth {Depth}",
            _[gameStateController.CurrentGameState.Player.Name](http://gameStateController.CurrentGameState.Player.Name),
            _gameStateController.CurrentGameState.CurrentDungeon?.CurrentDepth ?? 0);
    }
    
    public async Task OnMoveAsync(Direction direction)
    {
        var gameState = _gameStateController.CurrentGameState;
        var currentRoom = gameState.CurrentRoom;
        
        _logger.Debug("Survivor moving {Direction} from room {RoomId}", direction, currentRoom?.RoomId);
        
        if (!currentRoom.HasExit(direction))
        {
            _viewModel.ShowMessage("Cannot move in that direction.");
            return;
        }
        
        var newRoom = gameState.CurrentDungeon.GetAdjacentRoom(currentRoom, direction);
        gameState.CurrentRoom = newRoom;
        newRoom.HasBeenExplored = true;
        
        _viewModel.LoadRoom(newRoom);
        _viewModel.UpdateMinimap();
        
        // Forced encounters (Undying patrols)
        if (newRoom.HasForcedEncounter && !newRoom.EncounterDefeated)
        {
            _logger.Information("Undying patrol encountered in room {RoomId}", newRoom.RoomId);
            await _combatController.StartCombatAsync(newRoom.ForcedEncounter);
            return;
        }
        
        // Random encounters
        var encounterRoll = await _encounterService.RollForRandomEncounterAsync(gameState);
        if (encounterRoll.EncounterTriggered)
        {
            _logger.Information("Random Undying encounter: difficulty {Difficulty}",
                encounterRoll.Encounter.DifficultyRating);
            await _combatController.StartCombatAsync(encounterRoll.Encounter);
        }
    }
    
    public async Task OnSearchRoomAsync()
    {
        var currentRoom = _gameStateController.CurrentGameState.CurrentRoom;
        if (currentRoom.HasBeenSearched)
        {
            _viewModel.ShowMessage("Room already searched.");
            return;
        }
        
        var searchResult = await _roomFeatureService.SearchRoomAsync(currentRoom);
        currentRoom.HasBeenSearched = true;
        _viewModel.ShowSearchResult(searchResult);
        
        if (searchResult.TriggeredEncounter)
        {
            _logger.Warning("Search disturbed dormant Undying");
            await _combatController.StartCombatAsync(searchResult.Encounter);
        }
    }
    
    public async Task OnRestAsync()
    {
        var survivor = _gameStateController.CurrentGameState.Player;
        _logger.Information("Survivor field rest: HP={HP}/{MaxHP}, Stress={Stress}",
            survivor.CurrentHP, survivor.MaxHP, survivor.PsychicStress);
        
        var confirmed = await _viewModel.ShowRestConfirmationAsync();
        if (!confirmed) return;
        
        var restResult = await _roomFeatureService.PerformFieldRestAsync(_gameStateController.CurrentGameState);
        survivor.CurrentHP = Math.Min(survivor.CurrentHP + restResult.HPRestored, survivor.MaxHP);
        survivor.PsychicStress += restResult.StressGained; // Trauma cost
        
        _viewModel.ShowRestResult(restResult);
        
        if (restResult.InterruptedByEncounter)
        {
            _logger.Warning("Field rest interrupted by Undying!");
            await _combatController.StartCombatAsync(restResult.InterruptingEncounter);
        }
    }
    
    public async Task OnReturnFromCombatAsync(bool wasVictory)
    {
        await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration,
            reason: wasVictory ? "Combat victory" : "Fled combat");
        _viewModel.LoadRoom(_gameStateController.CurrentGameState.CurrentRoom);
    }
}
```

---

## Integration Points

**With v0.44.2 (Character Creation):**

- Receives character and dungeon from character creation
- Begins exploration loop

**With v0.44.4 (Combat):**

- Triggers combat when encounters occur
- Receives control back after combat ends

**With v0.43.12-v0.43.14 (Exploration UI):**

- Updates DungeonExplorationViewModel
- Displays room descriptions and features
- Updates minimap

**With v0.4-v0.5 (Dungeon Systems):**

- Uses dungeon room graph
- Calls RoomFeatureService for searching
- Triggers encounters via EncounterService

---

## Success Criteria

**v0.44.3 is DONE when:**

### ✅ Room Navigation

- [ ]  Can move in all 6 directions
- [ ]  Invalid directions blocked
- [ ]  New rooms marked as explored
- [ ]  Minimap updates

### ✅ Room Actions

- [ ]  Can search rooms
- [ ]  Loot collection works
- [ ]  Search encounters trigger combat
- [ ]  Cannot search same room twice

### ✅ Rest System

- [ ]  Confirmation dialog shows
- [ ]  HP/Stamina recovery works
- [ ]  Psychic Stress increases
- [ ]  Random encounters can interrupt

### ✅ Encounters

- [ ]  Random encounters trigger
- [ ]  Forced encounters trigger on entry
- [ ]  Transitions to combat correctly
- [ ]  Returns from combat correctly

---

**Exploration loop complete. Ready for combat controller in v0.44.4.**