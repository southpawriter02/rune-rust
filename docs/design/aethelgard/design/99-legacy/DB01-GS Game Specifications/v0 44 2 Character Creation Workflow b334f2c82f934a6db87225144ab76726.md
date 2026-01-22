# v0.44.2: Character Creation Workflow

Type: Technical
Description: CharacterCreationController for complete Survivor initialization: Lineage → Background → Attributes → Archetype → Specialization. Saga System initialization (Legend=0, PP=0).
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.44.1, v0.43.9-v0.43.11, v0.6-v0.14 (Character Systems)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.44: Game Flow Integration & Controllers (v0%2044%20Game%20Flow%20Integration%20&%20Controllers%200f28bd7b1ab1400fb9cc0377e89bb095.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.44.1, v0.43.9-v0.43.11, v0.6-v0.14 (Character Systems)

**Estimated Time:** 6-9 hours

**Phase:** Initialization

**Deliverable:** Complete character creation wizard producing valid PlayerCharacter

---

## Executive Summary

v0.44.2 implements the CharacterCreationController that orchestrates the complete Survivor initialization workflow. Follows the canonical character creation sequence from v2.0 specifications: Lineage → Background → Attributes → Archetype → Specialization.

**What This Delivers:**

- Complete CharacterCreationController implementation
- **Step 1:** Lineage selection (Clan-Born, Rune-Marked, Iron-Blooded, Vargr-Kin)
- **Step 2:** Background selection (skill bonuses + starting gear)
- **Step 3:** Attribute allocation via point-buy system (15 points)
    - Simple mode: Recommended build for selected archetype
    - Advanced mode: Manual allocation with cost scaling
- **Step 4:** Archetype selection (Warrior/Adept/Mystic/Ranger)
- **Step 5:** Specialization selection (3+ options per archetype)
- Starting abilities assignment (3 archetype abilities + specialization passives)
- **Legend & PP initialization** (0 Legend, 0 PP, Milestone 0)
- Alternative start scenarios (if unlocked via v0.41 meta-progression)
- Character validation (prevents invalid builds)
- Transition to Sector exploration

**The Canonical Character Creation Flow:**

Following v2.0 specifications, every Survivor must:

1. **Define their inherited interface** (Lineage: genetic/bloodline traits)
2. **Define their acquired expertise** (Background: pre-crash profession)
3. **Allocate their core operating parameters** (Attributes: MIGHT, FINESSE, STURDINESS, WITS, WILL)
4. **Choose their fundamental interface** (Archetype: resource system & combat philosophy)
5. **Choose their specialized subroutine** (Specialization: tactical identity & ability tree)

**Aethelgard Voice:**

- Player = **Survivor** (not "player character")
- Dungeon = **Sector** (not "dungeon")
- Enemies = **Undying** or **Jötun-Forged** (not "monsters")
- Experience = **Legend** (saga-worthy deeds)
- Skill Points = **Progression Points (PP)** (earned at Milestones)

**Success Metric:** Can create a valid Survivor and begin navigating their first Sector.

---

## Database Schema Changes

No new tables - uses existing Characters table from v0.6 with progression fields from v0.2 Saga System.

**Critical Progression Fields Initialized:**

```sql
-- These fields MUST be initialized on character creation
legend_points = 0                    -- No Legend earned yet
progression_points = 0               -- No PP available to spend
current_milestone_level = 0          -- Starting at Milestone 0
legend_to_next_milestone = 500       -- First milestone requires 500 Legend
```

---

## Service Implementation

### CharacterCreationController (Complete Implementation)

**Full implementation covering all 6 steps:**

The CharacterCreationController orchestrates the canonical v2.0 character creation sequence with complete Saga System integration. Key implementation details:

**Core Methods:**

- `Initialize(CharacterCreationViewModel)` - Starts workflow at Lineage step
- `OnLineageSelectedAsync(string)` - Handles Lineage selection → proceeds to Background
- `OnBackgroundSelectedAsync(string)` - Handles Background selection → proceeds to Attributes
- `OnAttributeAllocationModeChanged(bool)` - Toggles simple/advanced mode
- `OnAttributeChanged(string, int)` - Manual attribute allocation with cost calculation
- `CalculateAttributeCost(int, int)` - Point-buy: 1pt for 5→8, 2pt for 8→10
- `OnAttributesConfirmedAsync()` - Validates allocation → proceeds to Archetype
- `OnArchetypeSelectedAsync(string)` - Loads specializations, applies recommended build if simple mode
- `ApplyRecommendedBuild(string)` - Auto-allocates 15 points per archetype
- `OnSpecializationSelectedAsync(string)` - Finalizes choice → shows summary
- `ShowSummary()` - Displays review screen
- `OnConfirmCharacterAsync(string)` - Creates Survivor with **critical Saga System initialization**
- `IsASCIIOnly(string)` - Validates ASCII-only names (v5.0 mandatory)
- `OnCancelAsync()` - Returns to main menu

**Critical Saga System Initialization in OnConfirmCharacterAsync:**

```csharp
// MANDATORY: Initialize Saga progression fields
validatedCharacter.LegendPoints = 0;                  // No saga-worthy deeds yet
validatedCharacter.ProgressionPoints = 0;              // No PP available
validatedCharacter.CurrentMilestoneLevel = 0;          // Starting saga
validatedCharacter.LegendToNextMilestone = 500;        // First milestone formula

_logger.Information(
    "Survivor initialized: Legend={Legend}, PP={PP}, Milestone={Milestone}, NextMilestone={Next}",
    0, 0, 0, 500);
```

**Aethelgard Voice in Logging:**

```csharp
_logger.Information("Creating Survivor: {Name} ({Lineage} {Archetype}/{Specialization})", ...);
_logger.Information("Starting Sector generated: {Depth} depth, {RoomCount} rooms", ...);
_logger.Information("Survivor {Name}'s saga begins in Sector {SectorId}", ...);
```

### CharacterCreationViewModel Enhancements

```csharp
// Add to existing CharacterCreationViewModel from v0.43

public enum CharacterCreationStep
{
    Lineage,           // Step 1: Bloodline traits
    Background,        // Step 2: Pre-crash profession
    Attributes,        // Step 3: Point-buy allocation
    Archetype,         // Step 4: Resource system
    Specialization,    // Step 5: Tactical identity
    Summary            // Step 6: Final confirmation
}

public class CharacterCreationViewModel : ViewModelBase
{
    private CharacterCreationStep _currentStep = CharacterCreationStep.Archetype;
    private bool _useAdvancedMode = false;
    private int _remainingAttributePoints = 0;
    
    public CharacterCreationStep CurrentStep
    {
        get => _currentStep;
        set => this.RaiseAndSetIfChanged(ref _currentStep, value);
    }
    
    public bool UseAdvancedMode
    {
        get => _useAdvancedMode;
        set => this.RaiseAndSetIfChanged(ref _useAdvancedMode, value);
    }
    
    public int RemainingAttributePoints
    {
        get => _remainingAttributePoints;
        set => this.RaiseAndSetIfChanged(ref _remainingAttributePoints, value);
    }
    
    public ObservableCollection<string> AvailableArchetypes { get; set; } = new();
    public ObservableCollection<string> AvailableSpecializations { get; set; } = new();
    
    public AttributeSet RecommendedAttributes { get; set; } = new();
    
    public ICommand SelectArchetypeCommand { get; }
    public ICommand SelectSpecializationCommand { get; }
    public ICommand ToggleAdvancedModeCommand { get; }
    public ICommand AdjustAttributeCommand { get; }
    public ICommand ConfirmCharacterCommand { get; }
    public ICommand CancelCommand { get; }
    
    public void ShowValidationErrors(List<string> errors)
    {
        // Display validation errors to user
    }
}
```

```csharp
// Add to existing CharacterCreationViewModel from v0.43

public enum CharacterCreationStep
{
    Archetype,
    Specialization,
    Attributes,
    Summary
}

public class CharacterCreationViewModel : ViewModelBase
{
    private CharacterCreationStep _currentStep = CharacterCreationStep.Archetype;
    private bool _useAdvancedMode = false;
    private int _remainingAttributePoints = 0;
    
    public CharacterCreationStep CurrentStep
    {
        get => _currentStep;
        set => this.RaiseAndSetIfChanged(ref _currentStep, value);
    }
    
    public bool UseAdvancedMode
    {
        get => _useAdvancedMode;
        set => this.RaiseAndSetIfChanged(ref _useAdvancedMode, value);
    }
    
    public int RemainingAttributePoints
    {
        get => _remainingAttributePoints;
        set => this.RaiseAndSetIfChanged(ref _remainingAttributePoints, value);
    }
    
    // Step 1: Lineage
    public ObservableCollection<string> AvailableLineages { get; set; } = new();
    public string? SelectedLineage { get; set; }
    
    // Step 2: Background
    public ObservableCollection<string> AvailableBackgrounds { get; set; } = new();
    public string? SelectedBackground { get; set; }
    
    // Step 4: Archetype
    public ObservableCollection<string> AvailableArchetypes { get; set; } = new();
    public string? SelectedArchetype { get; set; }
    
    // Step 5: Specialization
    public ObservableCollection<string> AvailableSpecializations { get; set; } = new();
    public string? SelectedSpecialization { get; set; }
    
    // Step 6: Summary
    public string? SummaryLineage { get; set; }
    public string? SummaryBackground { get; set; }
    public string? SummaryArchetype { get; set; }
    public string? SummarySpecialization { get; set; }
    public string? SummaryAttributes { get; set; }
    
    public AttributeSet RecommendedAttributes { get; set; } = new();
    
    public ICommand SelectArchetypeCommand { get; }
    public ICommand SelectSpecializationCommand { get; }
    public ICommand ToggleAdvancedModeCommand { get; }
    public ICommand AdjustAttributeCommand { get; }
    public ICommand ConfirmCharacterCommand { get; }
    public ICommand CancelCommand { get; }
    
    public void ShowValidationErrors(List<string> errors)
    {
        // Display validation errors to user
    }
}
```

---

## Integration Points

**With v0.44.1 (Main Menu):**

- Receives initialized GameState
- Returns to main menu on cancel

**With v0.44.3 (Exploration):**

- Transitions to exploration after character confirmed
- Passes complete PlayerCharacter and generated dungeon

**With v0.6-v0.14 (Character Systems):**

- Uses CharacterCreationService for validation
- Applies specialization abilities
- Calculates derived stats

---

## Functional Requirements

### FR1: Complete Character Creation Flow

**Test:**

```csharp
[Fact]
public async Task CompleteCharacterCreation_AllSteps()
{
    var controller = CreateCharacterCreationController();
    controller.Initialize(_viewModel);
    
    // Step 1: Lineage
    await controller.OnLineageSelectedAsync("Clan-Born");
    Assert.Equal(CharacterCreationStep.Background, _viewModel.CurrentStep);
    
    // Step 2: Background
    await controller.OnBackgroundSelectedAsync("Village Blacksmith");
    Assert.Equal(CharacterCreationStep.Attributes, _viewModel.CurrentStep);
    
    // Step 3: Attributes (simple mode)
    controller.OnAttributeAllocationModeChanged(false);
    await controller.OnAttributesConfirmedAsync();
    Assert.Equal(CharacterCreationStep.Archetype, _viewModel.CurrentStep);
    
    // Step 4: Archetype
    await controller.OnArchetypeSelectedAsync("Warrior");
    Assert.Equal(CharacterCreationStep.Specialization, _viewModel.CurrentStep);
    
    // Step 5: Specialization
    await controller.OnSpecializationSelectedAsync("Skjaldmaer (Shieldmaiden)");
    Assert.Equal(CharacterCreationStep.Summary, _viewModel.CurrentStep);
    
    // Step 6: Confirm
    await controller.OnConfirmCharacterAsync("Bjorn");
    Assert.Equal(GamePhase.DungeonExploration, _gameStateController.CurrentGameState.CurrentPhase);
}
```

### FR2: Saga System Initialization

**Test:**

```csharp
[Fact]
public async Task ConfirmCharacter_InitializesSagaProgression()
{
    var controller = CreateCharacterCreationController();
    // ... complete character creation flow
    
    await controller.OnConfirmCharacterAsync("Test Survivor");
    
    var character = _gameStateController.CurrentGameState.Player;
    
    // Verify Saga System fields initialized correctly
    Assert.Equal(0, character.LegendPoints);
    Assert.Equal(0, character.ProgressionPoints);
    Assert.Equal(0, character.CurrentMilestoneLevel);
    Assert.Equal(500, character.LegendToNextMilestone); // First milestone formula
}
```

### FR3: Archetype Selection

**Test:**

```csharp
[Fact]
public async Task SelectArchetype_LoadsSpecializations()
{
    var controller = CreateCharacterCreationController();
    controller.Initialize(_viewModel);
    
    await controller.OnArchetypeSelectedAsync("Warrior");
    
    Assert.Equal(CharacterCreationStep.Specialization, _viewModel.CurrentStep);
    Assert.NotEmpty(_viewModel.AvailableSpecializations);
}
```

### FR2: Attribute Allocation

**Test:**

```csharp
[Fact]
public void AdvancedMode_AllowsAttributeCustomization()
{
    var controller = CreateCharacterCreationController();
    controller.Initialize(_viewModel);
    controller.OnAttributeAllocationModeChanged(true);
    
    controller.OnAttributeChanged("MIGHT", 7);
    
    Assert.True(_viewModel.RemainingAttributePoints < 15);
}

[Fact]
public void SimpleMode_AppliesRecommendedBuild()
{
    var controller = CreateCharacterCreationController();
    controller.Initialize(_viewModel);
    
    controller.OnAttributeAllocationModeChanged(false);
    
    Assert.Equal(0, _viewModel.RemainingAttributePoints);
}
```

### FR3: Character Validation

**Test:**

```csharp
[Fact]
public async Task ConfirmCharacter_ValidatesAttributes()
{
    // Allocate more than 15 points
    // Should fail validation
}

[Fact]
public async Task ConfirmCharacter_WithValidCharacter_StartsExploration()
{
    var controller = CreateCharacterCreationController();
    // ... setup valid character
    
    await controller.OnConfirmCharacterAsync("Test Hero");
    
    Assert.Equal(GamePhase.DungeonExploration, _gameStateController.CurrentGameState.CurrentPhase);
    Assert.NotNull(_gameStateController.CurrentGameState.Player);
    Assert.NotNull(_gameStateController.CurrentGameState.CurrentDungeon);
}
```

---

## Success Criteria

**v0.44.2 is DONE when:**

### ✅ Character Creation Flow

- [ ]  Can select archetype
- [ ]  Can select specialization
- [ ]  Can allocate attributes (simple mode)
- [ ]  Can allocate attributes (advanced mode)
- [ ]  Can confirm character
- [ ]  Can cancel and return to menu

### ✅ Validation

- [ ]  Cannot exceed attribute point budget
- [ ]  Must select archetype and specialization
- [ ]  Must provide character name
- [ ]  Starting abilities applied correctly

### ✅ Transition

- [ ]  Character added to GameState
- [ ]  Dungeon generated
- [ ]  Navigates to exploration view

---

**Character creation complete. Ready for exploration in v0.44.3.**