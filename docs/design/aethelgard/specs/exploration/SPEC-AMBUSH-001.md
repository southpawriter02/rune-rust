---
id: SPEC-AMBUSH-001
title: Ambush Risk System
version: 1.0.0
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-REST-001, SPEC-DICE-001, SPEC-COMBAT-001]
---

# SPEC-AMBUSH-001: Ambush Risk System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `IAmbushService`, `AmbushService`
> **Location:** `RuneAndRust.Engine/Services/AmbushService.cs`

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

The Ambush Risk System calculates the probability of enemy ambush during wilderness rest periods. When a character rests in dangerous territory without the protection of a Sanctuary, they risk being attacked while their guard is down.

The system uses a two-phase probability calculation: first determining base risk from the room's DangerLevel, then applying WITS-based mitigation through dice rolls. A minimum 5% risk floor ensures that dangerous zones are never completely safe, maintaining tension even for high-WITS characters.

When an ambush triggers, the system generates a scaled encounter using biome-appropriate enemy templates with prioritization for ambush-suitable creature types (fast, stealthy). The resulting combat applies the "Disoriented" status to the player, representing the disadvantage of being caught off-guard.

---

## Core Concepts

### Danger Level

A room property indicating environmental threat level. Maps directly to base ambush risk:

| DangerLevel | Base Risk | Description |
|-------------|-----------|-------------|
| Safe | 0% | Protected areas, no ambush possible |
| Unstable | 15% | Moderate danger, some hostile presence |
| Hostile | 30% | Active threats, combat imminent |
| Lethal | 50% | Extreme danger, survival unlikely |

### Mitigation

Risk reduction from the character's awareness and camp-making skills. Uses WITS attribute for dice pool. Each success (roll ≥ 8 on d10) reduces risk by 5%.

### Minimum Risk Floor

In non-safe zones, the final risk can never drop below 5%. This ensures that even highly capable characters face some danger when resting in hostile territory.

### Ambush Encounter

A scaled combat encounter generated when ambush triggers. Uses 80% of standard encounter budget and prioritizes fast/stealthy enemy templates.

---

## Behaviors

### Primary Behaviors

#### 1. Calculate Ambush Risk (`CalculateAmbushAsync`)

```csharp
Task<AmbushResult> CalculateAmbushAsync(Character character, Room room)
```

**Purpose:** Determines whether an ambush occurs during wilderness rest.

**Logic:**
1. Get base risk from room's DangerLevel
2. If Safe zone (0% risk), return immediately with no ambush
3. Roll WITS dice pool for mitigation (Camp Craft check)
4. Calculate mitigation: successes × 5%
5. Apply minimum 5% floor for non-safe zones
6. Roll d100 against final risk percentage
7. If roll ≤ risk: generate ambush encounter
8. Return AmbushResult with all calculation details

**Example:**
```csharp
var character = new Character { Wits = 6 };
var room = new Room { DangerLevel = DangerLevel.Hostile }; // 30% base

var result = await ambushService.CalculateAmbushAsync(character, room);
// Rolls 6 dice, gets 2 successes = 10% mitigation
// Final risk: max(30 - 10, 5) = 20%
// Rolls d100 = 15
// 15 <= 20: AMBUSH!
```

#### 2. Get Base Risk (`GetBaseRisk`)

```csharp
int GetBaseRisk(DangerLevel dangerLevel)
```

**Purpose:** Returns the base ambush probability for a danger level.

**Logic:**
- Safe → 0%
- Unstable → 15%
- Hostile → 30%
- Lethal → 50%
- Unknown → 15% (defaults to Unstable)

**Example:**
```csharp
var risk = ambushService.GetBaseRisk(DangerLevel.Lethal);
// Returns: 50
```

#### 3. Generate Ambush Encounter (`GenerateAmbushEncounter`)

```csharp
EncounterDefinition GenerateAmbushEncounter(Room room, int partyLevel = 1)
```

**Purpose:** Creates a scaled enemy encounter for an ambush.

**Logic:**
1. Calculate scaled budget: `100 × (1 + (partyLevel - 1) × 0.1)`
2. Apply ambush multiplier: budget × 0.8
3. Select templates prioritizing ambush types (Vargr → Servitor → Raider)
4. Fill budget with appropriate enemy count
5. Return EncounterDefinition with IsAmbush=true

**Example:**
```csharp
var encounter = ambushService.GenerateAmbushEncounter(room, partyLevel: 2);
// Budget: 100 × 1.1 × 0.8 = 88
// Spawns: 1× Ash-Vargr (40), 1× Servitor (25), 1× Servitor (25) = 90
```

---

## Restrictions

### What This System MUST NOT Do

1. **Never trigger ambush in Safe zones:** Safe zones have 0% risk and bypass all calculations.

2. **Never allow negative risk:** Risk is clamped to minimum 0 before floor application.

3. **Never spawn zero enemies:** Encounter generation ensures at least one enemy.

4. **Never exceed budget significantly:** Template selection respects budget constraints.

5. **Never generate encounters for non-triggered ambushes:** AmbushResult.Encounter is null when IsAmbush=false.

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Minimum risk floor | 5% | Ensures tension in dangerous areas |
| Mitigation per success | 5% | Balanced for average 2-3 success rolls |
| Ambush budget multiplier | 0.8× | Ambushes are smaller than planned encounters |
| Max WITS dice | *(attribute cap)* | Limited by character build |
| Enemy template pool | 3 types | Phase 1 implementation |

---

## Use Cases

### UC-1: Wilderness Rest in Hostile Zone

**Actor:** Player character
**Trigger:** Player uses "rest" command outside a Sanctuary
**Preconditions:** Room.DangerLevel > Safe, no RunicAnchor feature

```csharp
// RestService calls AmbushService
var ambushResult = await _ambushService.CalculateAmbushAsync(character, room);

if (ambushResult.IsAmbush)
{
    // Transition to combat with Disoriented status
    await _gameService.StartCombatAsync(ambushResult.Encounter);
    character.ApplyStatus(StatusEffectType.Disoriented, duration: 2);
}
else
{
    // Proceed with normal rest
    await _restService.ExecuteWildernessRestAsync(character, room);
}
```

**Postconditions:** Either combat initiated with Disoriented debuff, or rest completes normally

### UC-2: High-WITS Character Mitigates Risk

**Actor:** Player character with 8+ WITS
**Trigger:** Wilderness rest in Lethal zone
**Preconditions:** Character.Wits = 10, Room.DangerLevel = Lethal

```csharp
// Base risk: 50%
// WITS roll: 10 dice, assume 6 successes (60% average)
// Mitigation: 6 × 5% = 30%
// Final risk: max(50 - 30, 5) = 20%
// Still 20% chance of ambush despite excellent WITS
```

**Postconditions:** Risk reduced but not eliminated; 5% floor ensures permanent danger

### UC-3: Safe Zone Automatic Pass

**Actor:** Player character in safe area
**Trigger:** Rest in room with DangerLevel.Safe
**Preconditions:** Room.DangerLevel = Safe

```csharp
var result = await _ambushService.CalculateAmbushAsync(character, room);
// No dice rolls performed
// result.IsAmbush = false
// result.Message = "The area appears secure."
```

**Postconditions:** Rest proceeds without any ambush check rolls

---

## Decision Trees

### Ambush Risk Calculation Flow

```
┌─────────────────────────────────────────────────────────────┐
│               CalculateAmbushAsync(character, room)          │
└────────────────────────────┬────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │ GetBaseRisk()   │
                    │ (from DangerLvl)│
                    └────────┬────────┘
                             │
              ┌──────────────┴──────────────┐
              │ baseRisk == 0? (Safe zone)  │
              └──────────────┬──────────────┘
                    YES      │      NO
                     │       │       │
                     ▼       │       ▼
           ┌─────────────┐   │   ┌──────────────────┐
           │ Return:     │   │   │ Roll WITS pool   │
           │ IsAmbush=   │   │   │ for mitigation   │
           │ false       │   │   └────────┬─────────┘
           └─────────────┘   │            │
                             │            ▼
                             │   ┌────────────────────────┐
                             │   │ finalRisk = base -     │
                             │   │ (successes × 5%)       │
                             │   │ Apply 5% floor         │
                             │   └────────┬───────────────┘
                             │            │
                             │            ▼
                             │   ┌────────────────────────┐
                             │   │ Roll d100 vs finalRisk │
                             │   └────────┬───────────────┘
                             │            │
                             │     ┌──────┴──────┐
                             │     │ roll <= risk?│
                             │     └──────┬──────┘
                             │      YES   │   NO
                             │       │    │    │
                             │       ▼    │    ▼
                             │   ┌─────────┐  ┌─────────────┐
                             │   │ Generate │  │ Return:     │
                             │   │ Encounter│  │ IsAmbush=   │
                             │   └────┬────┘  │ false       │
                             │        │       └─────────────┘
                             │        ▼
                             │   ┌─────────────┐
                             │   │ Return:     │
                             │   │ IsAmbush=   │
                             │   │ true +      │
                             │   │ Encounter   │
                             │   └─────────────┘
```

### Encounter Budget Flow

```
┌────────────────────────────────────────────────────┐
│         GenerateAmbushEncounter(room, level)        │
└─────────────────────────┬──────────────────────────┘
                          │
                          ▼
              ┌───────────────────────────┐
              │ scaledBudget =            │
              │ 100 × (1 + (level-1)×0.1) │
              └───────────┬───────────────┘
                          │
                          ▼
              ┌───────────────────────────┐
              │ ambushBudget =            │
              │ scaledBudget × 0.8        │
              └───────────┬───────────────┘
                          │
                          ▼
              ┌───────────────────────────┐
              │ Priority: Vargr (40)      │
              │ Then: Servitors (25 each) │
              │ Fill until budget < 25    │
              └───────────┬───────────────┘
                          │
                          ▼
              ┌───────────────────────────┐
              │ Return EncounterDefinition│
              │ IsAmbush=true             │
              └───────────────────────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IDiceService` | [SPEC-DICE-001](../core/SPEC-DICE-001.md) | WITS pool rolls, d100 ambush determination |
| `Room.DangerLevel` | [SPEC-DUNGEON-001](./SPEC-DUNGEON-001.md) | Base risk lookup |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `RestService` | [SPEC-REST-001](../character/SPEC-REST-001.md) | Wilderness rest ambush checks |
| `CombatService` | [SPEC-COMBAT-001](../combat/SPEC-COMBAT-001.md) | Initiates combat from encounter |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Engine/Services/AmbushService.cs` | Full ambush logic | 1-227 |
| `RuneAndRust.Engine/Services/RestService.cs` | Calls ambush service | (integration) |

### Supporting Types

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Core/Interfaces/IAmbushService.cs` | Service contract | 1-39 |
| `RuneAndRust.Core/Models/AmbushResult.cs` | Result record | 1-25 |
| `RuneAndRust.Core/Models/EncounterDefinition.cs` | Encounter data | 1-16 |
| `RuneAndRust.Core/Enums/DangerLevel.cs` | Risk tiers | 1-28 |

---

## Data Models

### AmbushResult

```csharp
/// <summary>
/// Result of an ambush check during wilderness rest.
/// </summary>
/// <remarks>See: SPEC-AMBUSH-001 for Ambush Risk System design.</remarks>
public record AmbushResult(
    bool IsAmbush,              // True if ambush triggered
    string Message,             // AAM-VOICE narrative message
    int BaseRiskPercent,        // Initial risk from DangerLevel
    int MitigationPercent,      // Risk reduction from WITS
    int FinalRiskPercent,       // Final risk after floor (min 5%)
    int RollValue,              // The d100 roll result
    int MitigationSuccesses,    // Number of successes on WITS roll
    EncounterDefinition? Encounter = null  // Combat data if triggered
);
```

### EncounterDefinition

```csharp
/// <summary>
/// Defines a combat encounter with enemy templates and scaling.
/// </summary>
public record EncounterDefinition(
    IReadOnlyList<string> TemplateIds,  // Enemy template IDs to spawn
    float Budget,                        // Encounter budget used
    bool IsAmbush,                       // Affects initiative/status
    string EncounterType = "Ambush"      // Type for logging
);
```

### DangerLevel Enum

```csharp
public enum DangerLevel
{
    Safe = 0,      // 0% ambush risk
    Unstable = 1,  // 15% ambush risk
    Hostile = 2,   // 30% ambush risk
    Lethal = 3     // 50% ambush risk
}
```

---

## Configuration

### Constants

```csharp
// AmbushService.cs
private const int MitigationPerSuccess = 5;      // 5% risk reduction per success
private const int MinimumRiskFloor = 5;          // 5% minimum in danger zones
private const float AmbushBudgetMultiplier = 0.8f;  // 80% of standard budget
private const float BaseEncounterBudget = 100f;  // Base budget before scaling
```

### Enemy Template Costs

| Template ID | Cost | Type |
|-------------|------|------|
| `bst_vargr_01` | 40 | GlassCannon Beast |
| `mec_serv_01` | 25 | Swarm Minion |
| `hum_raider_01` | 35 | Support Humanoid |

### Risk Table

| DangerLevel | Base Risk | After 2 Successes | After 4 Successes |
|-------------|-----------|-------------------|-------------------|
| Safe | 0% | 0% | 0% |
| Unstable | 15% | 5% (floor) | 5% (floor) |
| Hostile | 30% | 20% | 10% |
| Lethal | 50% | 40% | 30% |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `RuneAndRust.Tests/Engine/AmbushServiceTests.cs` | 16 | Full system coverage |

### Critical Test Scenarios

1. **GetBaseRisk_SafeZone_ReturnsZero** - Safe zones have 0% risk
2. **GetBaseRisk_LethalZone_ReturnsFifty** - Lethal zones have 50% risk
3. **CalculateAmbush_SafeZone_AlwaysReturnsSafe** - No ambush possible in safe zones
4. **CalculateAmbush_SafeZone_NoRollPerformed** - No dice rolls in safe zones
5. **CalculateAmbush_HighWits_ReducesRisk** - Mitigation calculation correct
6. **CalculateAmbush_SuccessfulMitigation_AppliesFivePercentPerSuccess** - 5% per success
7. **CalculateAmbush_ExcessiveMitigation_ClampsToFivePercent** - Floor enforced
8. **CalculateAmbush_DangerousZone_NeverBelowFivePercent** - Floor at 5%
9. **CalculateAmbush_RollBelowRisk_TriggersAmbush** - Ambush on low roll
10. **CalculateAmbush_RollAboveRisk_ReturnsSafe** - No ambush on high roll
11. **CalculateAmbush_RollEqualsRisk_TriggersAmbush** - <= comparison
12. **GenerateAmbushEncounter_UsesBudgetMultiplier** - 0.8× multiplier
13. **GenerateAmbushEncounter_AlwaysReturnsAtLeastOneEnemy** - Minimum 1 enemy
14. **GenerateAmbushEncounter_HigherPartyLevel_ScalesBudget** - Level scaling
15. **GenerateAmbushEncounter_PrioritizesVargr** - Vargr selected first

### Validation Checklist

- [x] Safe zones bypass all calculations
- [x] Mitigation uses WITS attribute for pool size
- [x] 5% minimum floor applied in non-safe zones
- [x] d100 roll determines ambush trigger
- [x] Encounter generated only on trigger
- [x] Budget scales with party level
- [x] At least one enemy always spawned

---

## Design Rationale

### Why 5% Minimum Floor?

- Maintains tension even for optimized builds
- Prevents "trivial" rest in dangerous areas
- Rewards Sanctuary-seeking behavior
- Balances high-WITS character power

### Why WITS for Mitigation?

- Represents awareness and perception
- Consistent with Camp Craft skill thematically
- Balanced attribute (not dump stat, not required)
- Future integration with skill system

### Why 5% per Success?

- Average roll of 5 dice = 2-3 successes = 10-15% reduction
- Keeps high-risk zones meaningfully dangerous
- Rewards investment without trivializing
- Scales linearly for predictability

### Why 0.8× Ambush Budget?

- Ambushes should be survivable
- Surprise already advantages enemies
- Player starts Disoriented
- Full encounters would be too punishing

---

## Changelog

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.2b implementation
- Documents IAmbushService contract and AmbushService implementation
- Documents AmbushResult and EncounterDefinition models
- Documents DangerLevel enum and risk mappings
- Documents 16 unit test scenarios
