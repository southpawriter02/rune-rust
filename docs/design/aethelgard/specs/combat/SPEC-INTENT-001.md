---
id: SPEC-INTENT-001
title: Enemy Intent & Telegraph System
version: 1.0.0
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-COMBAT-001, SPEC-AI-001, SPEC-STATUS-001]
---

# SPEC-INTENT-001: Enemy Intent & Telegraph System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `CombatService` (Intent methods)
> **Location:** `RuneAndRust.Engine/Services/CombatService.cs`

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Behaviors](#behaviors)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Changelog](#changelog)

---

## Overview

The Enemy Intent & Telegraph System (introduced in v0.3.6c) provides players with tactical information about enemy actions before they occur. At combat start and each round, the AI determines what each enemy will do, and the player's ability to perceive these intentions is governed by WITS-based checks or the Analyzed status effect.

This creates a strategic layer where high-WITS characters and the Adept archetype can "read" enemies, anticipating attacks and choosing optimal responses. The system uses iconographic representation (⚔️🛡️💨💤) to communicate intent quickly without revealing specific targets or damage values.

The intent system was introduced in v0.3.6c as part of the Tactical Combat expansion.

---

## Core Concepts

### Planned Action

Each living enemy has a `PlannedAction` calculated at round start by the EnemyAIService. This action represents what the enemy intends to do on their turn. The planning happens before any player actions, allowing the player to react strategically.

### Intent Visibility

The player doesn't automatically see enemy intentions. Each enemy's intent is hidden by default and revealed only through:
1. **WITS Check:** Roll WITS dice pool, success (1+) reveals that enemy's intent
2. **Analyzed Status:** Always reveals intent regardless of WITS roll
3. **Adept Archetype:** +2 bonus to WITS pool for intent checks

### Intent Icons

When intent is revealed, it displays as an icon in the combat grid:

| Icon | Action Type | Description |
|------|-------------|-------------|
| ⚔️ | Attack | Enemy will attempt to deal damage |
| 🛡️ | Defend | Enemy will take defensive posture |
| 💨 | Flee | Enemy will attempt to escape combat |
| 💤 | Pass | Enemy will skip their turn |
| ? | Unknown | Intent not revealed to player |

---

## Behaviors

### Primary Behaviors

#### 1. Plan Enemy Actions (`PlanEnemyActions`)

```csharp
private void PlanEnemyActions()
```

**Purpose:** Calculates intended actions for all living enemies and determines visibility.

**Logic:**
1. Iterate through all living enemies in turn order
2. For each enemy, call `_aiService.DetermineAction(enemy, state)` to get planned action
3. Calculate intent visibility via `CalculateIntentVisibility(player, enemy)`
4. Store results in `enemy.PlannedAction` and `enemy.IsIntentRevealed`

**Triggers:**
- Combat start (after initiative roll)
- Round start (when TurnIndex resets to 0)
- State changes (HP drop, status effect application via `OnStateChange`)

**Example:**
```csharp
// Called at combat start
_sut.StartCombat(enemies);

// Enemy planned actions are now populated
var enemyCombatant = state.TurnOrder.First(c => !c.IsPlayer);
// enemyCombatant.PlannedAction may be Attack, Defend, Flee, or Pass
// enemyCombatant.IsIntentRevealed is true/false based on WITS check
```

#### 2. Calculate Intent Visibility (`CalculateIntentVisibility`)

```csharp
private bool CalculateIntentVisibility(Combatant player, Combatant enemy)
```

**Purpose:** Determines whether the player can perceive an enemy's planned action.

**Logic:**
1. Check if enemy has Analyzed status → automatic reveal
2. Calculate WITS pool: `Base WITS + Archetype Bonus + Condition Modifier`
   - Adept archetype grants +2 bonus
   - Environmental conditions may modify WITS
3. Roll WITS check (minimum pool of 1)
4. Success = 1+ successes on the dice roll
5. Return true if Analyzed or check succeeds, false otherwise

**Formula:**
```
Total Pool = Base WITS + (Archetype == Adept ? 2 : 0) + ConditionWitsModifier
Roll = DiceService.Roll(max(1, TotalPool), "Intent Check")
Revealed = HasAnalyzed || Roll.Successes >= 1
```

**Example:**
```csharp
// Warrior with WITS 3, no Analyzed status
var baseWits = 3;
var archetypeBonus = 0; // Not Adept
var pool = 3;
// Rolls 3 dice, needs 1+ successes to reveal

// Adept with WITS 3
var baseWits = 3;
var archetypeBonus = 2;
var pool = 5;
// Rolls 5 dice, higher chance to reveal
```

#### 3. Get Intent Icon (`GetIntentIcon`)

```csharp
private static string GetIntentIcon(CombatAction? action, bool isRevealed)
```

**Purpose:** Maps action type to display icon for the combat grid.

**Logic:**
1. If not revealed or action is null → return "?"
2. Otherwise, return icon based on `action.Type`

**Example:**
```csharp
GetIntentIcon(attackAction, true);  // Returns "⚔️"
GetIntentIcon(defendAction, true);  // Returns "🛡️"
GetIntentIcon(attackAction, false); // Returns "?" (hidden)
```

---

## Restrictions

### What This System MUST NOT Do

1. **Never reveal specific targets:** The icon shows action type only, not who the enemy will attack or what ability they'll use.

2. **Never reveal damage values:** Intent is general (Attack, Defend), not "will deal 25 damage."

3. **Never bypass the visibility check for non-Analyzed enemies:** Even with high WITS, each enemy requires its own check.

4. **Never plan actions for dead enemies:** The `PlanEnemyActions` method filters to `CurrentHp > 0`.

5. **Never plan actions for the player:** `PlannedAction` is always null for player combatants.

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Intent icons | 5 types | Limited to core action types; no ability-specific icons |
| Visibility check | Per-enemy | Each enemy requires separate WITS check |
| Replanning | Round-based | Actions set at round start; no mid-round updates except state changes |
| Archetype bonus | Adept only | Other archetypes have no built-in advantage |
| Pool minimum | 1 die | Even 0 WITS characters get a chance |

---

## Use Cases

### UC-1: Standard Intent Check at Combat Start

**Actor:** Combat system
**Trigger:** `StartCombat()` called with enemy list
**Preconditions:** Player character exists, enemies are alive

```csharp
// Player with WITS 4, Warrior archetype
var character = new Character { Wits = 4, Archetype = ArchetypeType.Warrior };
_gameState.CurrentCharacter = character;

var enemies = new List<Enemy> { enemy1, enemy2, enemy3 };
_sut.StartCombat(enemies);

// After StartCombat:
// - All enemies have PlannedAction set
// - Each enemy has IsIntentRevealed based on individual WITS checks
// - Player can see icons for revealed enemies
```

**Postconditions:**
- All living enemies have `PlannedAction` populated
- `IsIntentRevealed` is set per-enemy based on visibility check
- ViewModel returns appropriate icons ("⚔️" or "?")

### UC-2: Analyzed Status Bypass

**Actor:** Player (via ability or item)
**Trigger:** Analyzed status applied to enemy
**Preconditions:** Combat active, target enemy alive

```csharp
// Apply Analyzed status to enemy
_statusEffectService.ApplyEffect(enemy, StatusEffectType.Analyzed, 3, sourceId);

// On next PlanEnemyActions call:
// CalculateIntentVisibility checks for Analyzed first
// Returns true immediately, bypassing WITS check
enemy.IsIntentRevealed.Should().BeTrue();
```

**Postconditions:** Enemy intent always visible while Analyzed is active

### UC-3: Adept Archetype Advantage

**Actor:** Adept player character
**Trigger:** Combat starts or new round begins
**Preconditions:** Player is Adept archetype

```csharp
// Adept with WITS 3
var character = new Character { Wits = 3, Archetype = ArchetypeType.Adept };
_gameState.CurrentCharacter = character;

// During CalculateIntentVisibility:
var baseWits = 3;
var archetypeBonus = 2;  // Adept bonus
var totalPool = 5;       // Rolls 5 dice instead of 3
```

**Postconditions:** Adept has significantly higher chance to reveal enemy intents

### UC-4: Round Start Replanning

**Actor:** Combat system
**Trigger:** `NextTurn()` advances to new round
**Preconditions:** Combat active, TurnIndex reaches end of TurnOrder

```csharp
// Set TurnIndex to last combatant
state.TurnIndex = state.TurnOrder.Count - 1;

// NextTurn advances to new round
_sut.NextTurn();

// PlanEnemyActions called again at round start
// All enemies have fresh PlannedAction values
// New visibility checks are performed
```

**Postconditions:** Enemy actions replanned for new round; visibility recalculated

---

## Decision Trees

### Intent Visibility Flow

```
┌─────────────────────────────────────────────────────────────┐
│                 CalculateIntentVisibility                    │
└────────────────────────────┬────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │ Enemy has       │
                    │ Analyzed status?│
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │ YES          │              │ NO
              ▼              │              ▼
    ┌─────────────────┐      │    ┌─────────────────────────┐
    │ Return TRUE     │      │    │ Calculate WITS pool:    │
    │ (always visible)│      │    │ Base + ArchetypeBonus   │
    └─────────────────┘      │    │ + ConditionModifier     │
                             │    └───────────┬─────────────┘
                             │                │
                             │                ▼
                             │    ┌─────────────────────────┐
                             │    │ Roll dice pool          │
                             │    │ (minimum 1 die)         │
                             │    └───────────┬─────────────┘
                             │                │
                             │       ┌────────┴────────┐
                             │       │ 1+ Successes?   │
                             │       └────────┬────────┘
                             │                │
                             │    ┌───────────┼───────────┐
                             │    │ YES       │           │ NO
                             │    ▼           │           ▼
                             │ ┌──────────┐   │    ┌──────────┐
                             │ │ Return   │   │    │ Return   │
                             │ │ TRUE     │   │    │ FALSE    │
                             │ └──────────┘   │    └──────────┘
```

### Intent Icon Mapping Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    GetIntentIcon                             │
└────────────────────────────┬────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │ IsRevealed &&   │
                    │ Action != null? │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │ NO           │              │ YES
              ▼              │              ▼
    ┌─────────────────┐      │    ┌─────────────────────────┐
    │ Return "?"      │      │    │ Switch on ActionType:   │
    │ (unknown)       │      │    │ Attack → "⚔️"           │
    └─────────────────┘      │    │ Defend → "🛡️"           │
                             │    │ Flee → "💨"             │
                             │    │ Pass → "💤"             │
                             │    │ default → "?"           │
                             │    └─────────────────────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IEnemyAIService` | [SPEC-AI-001](./SPEC-AI-001.md) | Determines enemy planned action |
| `IStatusEffectService` | [SPEC-STATUS-001](./SPEC-STATUS-001.md) | Checks for Analyzed status |
| `IDiceService` | [SPEC-DICE-001](../core/SPEC-DICE-001.md) | WITS check dice pool |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatRenderer` | [SPEC-RENDER-001](../ui/SPEC-RENDER-001.md) | Displays intent icons in grid |
| `CombatViewModel` | [SPEC-UI-001](../ui/SPEC-UI-001.md) | Maps intent to view model |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Engine/Services/CombatService.cs` | Intent calculation methods | 190-267 |
| `RuneAndRust.Core/Models/Combat/Combatant.cs` | PlannedAction, IsIntentRevealed properties | 133-149 |
| `RuneAndRust.Core/ViewModels/CombatViewModel.cs` | IntentIcon property in CombatantView | 49-64 |

### Supporting Types

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Core/Enums/StatusEffectType.cs` | Analyzed status definition | 74-79 |
| `RuneAndRust.Core/Models/Combat/CombatAction.cs` | ActionType enum | 1-30 |

---

## Data Models

### Combatant Intent Properties

```csharp
// Combatant.cs (v0.3.6c additions)
#region Intent System (v0.3.6c)

/// <summary>
/// The action the AI intends to take this round.
/// Calculated at round start and on state changes (HP, status).
/// Null for player combatants.
/// </summary>
public CombatAction? PlannedAction { get; set; }

/// <summary>
/// Whether the player has successfully perceived this enemy's intent.
/// Set by CombatService based on WITS check or Analyzed status.
/// Always false for player combatants.
/// </summary>
public bool IsIntentRevealed { get; set; } = false;

#endregion
```

### CombatantView Intent Display

```csharp
// CombatantView record (v0.3.6c additions)
/// <param name="IntentIcon">
/// Icon showing enemy's planned action: ⚔️/🛡️/💨/💤/? or null for player.
/// </param>
public record CombatantView(
    // ... other properties ...
    string? IntentIcon = null
);
```

### Analyzed Status Effect

```csharp
// StatusEffectType.cs
/// <summary>
/// Intel debuff that reveals enemy intent regardless of WITS check.
/// Does not stack; reapplication refreshes duration only (v0.3.6c).
/// </summary>
[GameDocument(
    "Analyzed",
    "The target's behavioral patterns have been catalogued. " +
    "Their next action becomes predictable, revealing intent regardless of other factors.")]
Analyzed = 6,
```

---

## Configuration

### Constants

```csharp
// CombatService.cs (implicit)
private const int AdeptWitsBonusForIntent = 2;
private const int MinimumWitsPool = 1;
private const int SuccessThreshold = 1;  // 1+ successes reveals intent
```

### Settings

| Setting | Default | Range | Purpose |
|---------|---------|-------|---------|
| Adept WITS bonus | +2 | Fixed | Bonus dice for Adept archetype |
| Success threshold | 1 | Fixed | Minimum successes to reveal |
| Minimum pool | 1 | Fixed | Ensures even 0-WITS can attempt |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `RuneAndRust.Tests/Engine/IntentSystemTests.cs` | 11 | Intent planning, visibility, icons |
| `RuneAndRust.Tests/Engine/AnalyzedStatusTests.cs` | 11 | Analyzed status behavior |
| `RuneAndRust.Tests/Terminal/CombatGridRendererIntentTests.cs` | * | Intent icon rendering |

### Critical Test Scenarios

1. **StartCombat_PlansActionsForAllEnemies** - All enemies get PlannedAction
2. **StartCombat_SkipsDeadEnemies_WhenPlanningActions** - Dead enemies ignored
3. **StartCombat_PlayerCombatant_HasNullPlannedAction** - Player never has intent
4. **StartCombat_AnalyzedStatus_AlwaysRevealsIntent** - Analyzed bypass
5. **StartCombat_HighWits_WithSuccessfulRoll_RevealsIntent** - WITS check success
6. **StartCombat_LowWits_WithFailedRoll_HidesIntent** - WITS check failure
7. **StartCombat_AdeptArchetype_GetsBonusToWitsPool** - +2 bonus applied
8. **StartCombat_NonAdeptArchetype_NoBonus** - No bonus for others
9. **GetViewModel_Enemy_WithRevealedIntent_ShowsIntentIcon** - Icon display
10. **GetViewModel_Enemy_WithHiddenIntent_ShowsQuestionMark** - Hidden display
11. **GetViewModel_DefendAction_ShowsShieldIcon** - Defend icon mapping

### Validation Checklist

- [x] All living enemies have PlannedAction at round start
- [x] Dead enemies are skipped during planning
- [x] Player combatants have null PlannedAction
- [x] Analyzed status bypasses WITS check
- [x] Adept archetype receives +2 WITS bonus
- [x] Successful WITS check reveals intent
- [x] Failed WITS check hides intent
- [x] Intent icons map correctly to action types
- [x] Hidden intents show "?" icon

---

## Design Rationale

### Why Per-Enemy Visibility Checks?

- Creates tactical variability - you might see some enemies but not others
- Encourages multiple rounds to gather intelligence
- Makes Analyzed status valuable for priority targets

### Why Adept Archetype Bonus?

- Adepts are the "perception" specialists in the archetype system
- Reinforces class identity without mandatory abilities
- +2 bonus is meaningful but not overwhelming (20% more dice)

### Why Replan at State Changes?

- Enemies should react to changing circumstances
- HP dropping may trigger Flee intent
- Status effects may alter behavior
- Keeps AI feeling reactive rather than scripted

### Why Icon-Based Display?

- Quick recognition at a glance
- Works across language barriers
- Fits terminal-based UI aesthetic
- Matches Unicode status effect icons

### Why No Target Revelation?

- Maintains some uncertainty even with revealed intent
- "They will attack" is useful but not overpowering
- Full information would trivialize tactical decisions
- Aligns with Domain 4 epistemic uncertainty principles

---

## Changelog

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.6c implementation
- Documents PlanEnemyActions, CalculateIntentVisibility, GetIntentIcon
- Documents Analyzed status integration
- Documents Adept archetype bonus mechanics
- Documents intent icon mapping
