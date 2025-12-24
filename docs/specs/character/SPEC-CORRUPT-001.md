---
id: SPEC-CORRUPT-001
title: Corruption System
version: 1.1.0
status: Implemented
last_updated: 2025-12-23
related_specs: [SPEC-CHAR-001, SPEC-COMBAT-001, SPEC-TRAUMA-001]
---

# SPEC-CORRUPT-001: Corruption System

> **Version:** 1.1.0
> **Status:** Implemented (v0.3.0b)
> **Service:** `TraumaService` (Corruption methods)
> **Location:** `RuneAndRust.Engine/Services/TraumaService.cs`

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Corruption Tiers](#corruption-tiers)
- [Behaviors](#behaviors)
- [CorruptionState Details](#corruptionstate-details)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Integration with StatCalculationService](#integration-with-statcalculationservice)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Changelog](#changelog)

---

## Overview

The Corruption System tracks permanent magical contamination from exposure to Runic Blight, unstable Aether, and other supernatural hazards. Unlike Stress, Corruption is generally **not recoverable** and accumulates over a character's lifetime, eventually leading to Terminal Error (character death/transformation).

Corruption represents the slow decay of a character's humanity in a world saturated with malfunctioning magic.

---

## Core Concepts

### Runic Blight (Corruption)
- **Range:** 0-100
- **Nature:** Permanent (rare purge opportunities)
- **Impact:** Attribute penalties, MaxAP reduction, eventual character loss
- **Sources:** Corrupted zones, Aether misuse, cursed items, Blight creatures

### Terminal Error
- **Trigger:** Corruption reaches 100
- **Effect:** Character becomes "Forlorn" (unplayable)
- **Nature:** Permanent character death

---

## Corruption Tiers

| Tier | Range | MaxAP Penalty | WILL Penalty | WITS Penalty |
|------|-------|---------------|--------------|--------------|
| **Pristine** | 0-20 | None | None | None |
| **Tainted** | 21-40 | None | None | None |
| **Corrupted** | 41-60 | -10% (×0.90) | None | None |
| **Blighted** | 61-80 | -20% (×0.80) | -1 | None |
| **Fractured** | 81-99 | -40% (×0.60) | -2 | -1 |
| **Terminal** | 100 | 100% (×0.00) | -2 | -1 |

**Notes:**
- Pristine and Tainted tiers have cosmetic effects only (visual glitches, UI distortion)
- Corrupted tier is the first mechanical penalty threshold
- Terminal tier results in character loss (becomes Forlorn)

---

## Behaviors

### Primary Behaviors

#### 1. Corruption Accumulation (`AddCorruption`)

```csharp
CorruptionResult AddCorruption(Character character, int amount, string source)
CorruptionResult AddCorruption(Combatant combatant, int amount, string source)
```

**Key Difference from Stress:**
- **No mitigation** - Corruption accumulates directly
- WILL does not reduce incoming corruption
- Only capped at 100 maximum

**Resolution Sequence:**
1. Validate amount is positive
2. Record previous corruption and tier
3. Add corruption directly (no roll)
4. Clamp to 0-100
5. Calculate new tier and tier change
6. Check for Terminal Error (crossing 100)
7. Handle Terminal Error if triggered
8. Return result

**Example:**
```csharp
// Player touches corrupted artifact
var result = traumaService.AddCorruption(character, 15, "Corrupted Relic");
// If player was at 35, now at 50 → Tier changed to "Corrupted"!
```

**Combatant Overload:**
For non-player combatants, only updates local state (no persistence):
```csharp
if (combatant.CharacterSource == null)
{
    combatant.CurrentCorruption = Math.Min(100, current + amount);
    return result; // No persistent update
}
```

#### 2. Corruption Purge (`PurgeCorruption`)

```csharp
CorruptionResult PurgeCorruption(Character character, int amount, string source)
```

**Mechanics:**
- Rare mechanic (special items, rituals, locations)
- Direct reduction, no roll
- Cannot go below 0
- Can reverse tier progression

**Example:**
```csharp
// Sacred spring purges corruption
var result = traumaService.PurgeCorruption(character, 25, "Sacred Spring");
// If player was at 50, now at 25 → Tier improved to "Tainted"!
```

#### 3. Corruption State Query (`GetCorruptionState`)

```csharp
CorruptionState GetCorruptionState(int corruptionValue)
```

**Returns:** Comprehensive state object with all tier effects.

#### 4. Terminal Error Handling (`HandleTerminalError`)

```csharp
void HandleTerminalError(Character character)
```

**Current Implementation (Placeholder):**
- Logs critical error
- Future: Lock character as Forlorn, force main menu

---

## CorruptionState Details

### Property Breakdown

```csharp
public record CorruptionState(int Value)
{
    public int Value { get; }               // Current corruption (0-100)
    public CorruptionTier Tier { get; }     // Current tier enum
    public bool IsTerminal { get; }         // Value >= 100
    public bool HasPenalties { get; }       // Tier >= Corrupted

    // Attribute Penalties
    public int WillPenalty { get; }         // 0, 1, or 2
    public int WitsPenalty { get; }         // 0 or 1

    // Aether Pool Penalties
    public double MaxApMultiplier { get; }  // 1.0, 0.90, 0.80, 0.60, or 0.0
    public int MaxApPenaltyPercent { get; } // 0, 10, 20, 40, or 100

    // Descriptive
    public string TierDisplayName { get; }  // "Pristine", "Tainted", etc.
}
```

### Tier Effect Table

| Property | Pristine | Tainted | Corrupted | Blighted | Fractured | Terminal |
|----------|----------|---------|-----------|----------|-----------|----------|
| WillPenalty | 0 | 0 | 0 | 1 | 2 | 2 |
| WitsPenalty | 0 | 0 | 0 | 0 | 1 | 1 |
| MaxApMultiplier | 1.0 | 1.0 | 0.90 | 0.80 | 0.60 | 0.0 |
| MaxApPenaltyPercent | 0% | 0% | 10% | 20% | 40% | 100% |

---

## Restrictions

### Corruption Mechanics
1. **Cannot be mitigated** - No roll reduces incoming corruption
2. **Cannot exceed 100** - Clamped at Terminal
3. **Cannot go negative** - Minimum 0
4. **Rare purge only** - No natural recovery

### Terminal Error
1. **Immediate effect** - No grace period
2. **Character is lost** - No resurrection mechanic
3. **Save preserved** - Game continues with new character

### Tier Transitions
1. **Always recalculate on change** - Stats update immediately
2. **Bidirectional** - Purge can lower tier

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Corruption range | 0-100 | Hard limits |
| Max WILL penalty | 2 | At Fractured/Terminal tier |
| Max WITS penalty | 1 | At Fractured/Terminal tier |
| Max AP penalty | 100% | At Terminal tier |

### System Gaps
- No corruption resistance mechanics
- No partial corruption effects (binary tier thresholds)
- No corruption-specific abilities or curses

---

## Use Cases

### UC-1: Environmental Corruption
```csharp
// Player enters corrupted zone (per-turn tick from ConditionService)
var result = traumaService.AddCorruption(character, 2, "Blight Zone");

if (result.TierChanged)
{
    // Display tier transition warning
    // "The Blight deepens. You feel your grip on reality loosening."
}

if (result.IsTerminal)
{
    // Handle character loss
    HandleTerminalError(character);
}
```

### UC-2: Corrupted Item Acquisition
```csharp
// Player picks up cursed artifact
var result = traumaService.AddCorruption(character, 10, "Cursed Blade of Ash");

// Immediate tier recalculation affects MaxAP
statCalcService.RecalculateDerivedStats(character);
```

### UC-3: Purification Ritual
```csharp
// Special quest reward
var result = traumaService.PurgeCorruption(character, 30, "Purification Ritual");

if (result.TierChanged && result.NewTier < result.PreviousTier)
{
    // "The corruption recedes. You feel more yourself again."
}

// Recalculate stats for improved MaxAP
statCalcService.RecalculateDerivedStats(character);
```

### UC-4: Mystic Aether Overcharge
```csharp
// Mystic uses forbidden power
void OverchargeAether(Character character, int aetherBeyondMax)
{
    var corruptionGain = aetherBeyondMax * 2;
    traumaService.AddCorruption(character, corruptionGain, "Aether Overcharge");
}
```

---

## Decision Trees

### Corruption Accumulation Flow

```
┌─────────────────────────────────┐
│  AddCorruption(character, amt) │
└───────────────┬─────────────────┘
                │
        ┌───────┴───────┐
        │ amount <= 0?  │
        └───────┬───────┘
                │
       ┌────────┼────────┐
       │ YES    │   NO   │
       ▼        │        ▼
┌──────────┐    │  ┌─────────────────┐
│ Skip,    │    │  │ previousCorr =  │
│ Return   │    │  │ char.Corruption │
│ NoChange │    │  └────────┬────────┘
└──────────┘    │           │
                │           ▼
                │  ┌─────────────────────┐
                │  │ newCorr = prev +    │
                │  │ amt, clamp(0, 100)  │
                │  └────────┬────────────┘
                │           │
                │           ▼
                │  ┌─────────────────────┐
                │  │ Tier changed?       │
                │  └────────┬────────────┘
                │           │
                │  ┌────────┼────────┐
                │  │ NO     │  YES   │
                │  │        ▼        │
                │  │  ┌────────────┐ │
                │  │  │Log tier    │ │
                │  │  │transition  │ │
                │  │  └────────────┘ │
                │  └────────┬────────┘
                │           │
                │           ▼
                │  ┌─────────────────────┐
                │  │ newCorr == 100 &&   │
                │  │ prev < 100?         │
                │  └────────┬────────────┘
                │           │
                │  ┌────────┼────────┐
                │  │ NO     │  YES   │
                │  │        ▼        │
                │  │ ┌─────────────┐ │
                │  │ │HandleTerm.  │ │
                │  │ │Error()      │ │
                │  │ └─────────────┘ │
                │  └────────┬────────┘
                │           │
                │           ▼
                │  ┌─────────────────────┐
                │  │ Return Corruption   │
                │  │ Result              │
                └──└─────────────────────┘
```

### Tier Threshold Determination

```
┌────────────────────────────────┐
│  GetCorruptionState(value)     │
└───────────────┬────────────────┘
                │
        ┌───────┴───────┐
        │ value >= 100? │
        └───────┬───────┘
                │
       ┌────────┼────────┐
       │ YES    │   NO   │
       ▼        │        │
 ┌──────────┐   │        ▼
 │ Terminal │   │  ┌───────────────┐
 └──────────┘   │  │ value >= 81?  │
                │  └───────┬───────┘
                │          │
                │  ┌───────┼────────┐
                │  │ YES   │   NO   │
                │  ▼       │        │
                │ Fractured│        ▼
                │          │  ┌───────────────┐
                │          │  │ value >= 61?  │
                │          │  └───────┬───────┘
                │          │          │
                │          │  ┌───────┼────────┐
                │          │  │ YES   │   NO   │
                │          │  ▼       │        │
                │          │ Blighted │        ▼
                │          │          │  ┌───────────────┐
                │          │          │  │ value >= 41?  │
                │          │          │  └───────┬───────┘
                │          │          │          │
                │          │          │  ┌───────┼────────┐
                │          │          │  │ YES   │   NO   │
                │          │          │  ▼       │        │
                │          │          │Corrupted │        ▼
                │          │          │          │  ┌───────────────┐
                │          │          │          │  │ value >= 21?  │
                │          │          │          │  └───────┬───────┘
                │          │          │          │          │
                │          │          │          │  ┌───────┼────────┐
                │          │          │          │  │ YES   │   NO   │
                │          │          │          │  ▼       ▼        │
                │          │          │          │ Tainted Pristine  │
                └──────────┴──────────┴──────────┴──────────────────┘
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `ILogger` | Infrastructure | Terminal Error logging |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `StatCalculationService` | [SPEC-CHAR-001](SPEC-CHAR-001.md) | Attribute penalties, MaxAP modifier |
| `CombatService` | [SPEC-COMBAT-001](../combat/SPEC-COMBAT-001.md) | Combat log corruption events |
| `ConditionService` | [SPEC-COND-001](../combat/SPEC-COND-001.md) | Corruption tick effects |

---

## Related Services

### Primary Implementation
| File | Purpose | Key Lines |
|------|---------|-----------|
| `TraumaService.cs` | Corruption methods (co-located with stress) | 307-486 |

### Supporting Types
| File | Purpose | Key Lines |
|------|---------|-----------|
| `CorruptionState.cs` | Tier calculation record | 1-97 |
| `CorruptionResult.cs` | Operation result record | 1-26 |
| `CorruptionTier.cs` | Tier enum (6 values) | 1-39 |

---

## Data Models

### CorruptionResult
```csharp
public record CorruptionResult(
    int RawCorruption,            // Incoming corruption amount
    int NetCorruptionApplied,     // Actual change (clamped)
    int CurrentTotal,             // Final corruption value
    CorruptionTier PreviousTier,
    CorruptionTier NewTier,
    bool TierChanged,             // Did we cross a threshold?
    bool IsTerminal,              // Reached 100?
    string Source                 // What caused the corruption
);
```

### CorruptionTier Enum
```csharp
public enum CorruptionTier
{
    Pristine = 0,   // 0-20: No penalties
    Tainted = 1,    // 21-40: Cosmetic only
    Corrupted = 2,  // 41-60: -10% MaxAP
    Blighted = 3,   // 61-80: -20% MaxAP, -1 WILL
    Fractured = 4,  // 81-99: -40% MaxAP, -2 WILL, -1 WITS
    Terminal = 5    // 100: Character lost (Forlorn)
}
```

---

## Integration with StatCalculationService

### RecalculateDerivedStats Integration
```csharp
// In StatCalculationService.RecalculateDerivedStats() (lines 116-212)
var corruptionState = new CorruptionState(character.Corruption);

// Apply attribute penalties (lines 129-145)
if (corruptionState.WillPenalty > 0)
{
    effectiveWill = Math.Max(1, effectiveWill - corruptionState.WillPenalty);
}

if (corruptionState.WitsPenalty > 0)
{
    effectiveWits = Math.Max(1, effectiveWits - corruptionState.WitsPenalty);
}

// Apply MaxAP penalty (Mystics only) (lines 159-168)
var baseMaxAp = CalculateBaseMaxAp(character.Archetype, effectiveWill);
character.MaxAp = (int)(baseMaxAp * corruptionState.MaxApMultiplier);
```

---

## Configuration

### Tier Thresholds
```csharp
// CorruptionState.cs:16-24
public CorruptionTier Tier => Value switch
{
    >= 100 => CorruptionTier.Terminal,
    >= 81 => CorruptionTier.Fractured,
    >= 61 => CorruptionTier.Blighted,
    >= 41 => CorruptionTier.Corrupted,
    >= 21 => CorruptionTier.Tainted,
    _ => CorruptionTier.Pristine
};
```

### Penalty Values
```csharp
// CorruptionState.cs:30-37
public double MaxApMultiplier => Tier switch
{
    CorruptionTier.Terminal => 0.0,
    CorruptionTier.Fractured => 0.60,
    CorruptionTier.Blighted => 0.80,
    CorruptionTier.Corrupted => 0.90,
    _ => 1.0
};

// CorruptionState.cs:55-61
public int WillPenalty => Tier switch
{
    CorruptionTier.Terminal => 2,
    CorruptionTier.Fractured => 2,
    CorruptionTier.Blighted => 1,
    _ => 0
};

// CorruptionState.cs:67-72
public int WitsPenalty => Tier switch
{
    CorruptionTier.Terminal => 1,
    CorruptionTier.Fractured => 1,
    _ => 0
};
```

---

## Testing

### Test Files
| File | Tests | Coverage |
|------|-------|----------|
| `CorruptionStateTests.cs` | 30+ tests | Tier classification, penalties, boundaries |
| `TraumaServiceTests.cs` | 15+ corruption tests | Accumulation, purge, terminal |

### Critical Test Scenarios
1. Corruption accumulation without mitigation
2. Tier threshold crossing (all 6 transitions)
3. Terminal Error at exactly 100
4. Purge mechanics and tier improvement
5. Attribute penalty application (WILL, WITS)
6. MaxAP multiplier calculation (×0.90, ×0.80, ×0.60)
7. Non-player combatant handling
8. Edge cases (0, 100, negative, overflow)
9. HasPenalties returns true only at Corrupted+
10. TierDisplayName returns correct strings

### Validation Checklist
- [x] Tier thresholds match code (0-20, 21-40, 41-60, 61-80, 81-99, 100)
- [x] Tier names match code (Pristine, Tainted, Corrupted, Blighted, Fractured, Terminal)
- [x] Penalty values match code (×0.90, ×0.80, ×0.60, ×0.0)
- [x] WILL penalties: 0, 0, 0, 1, 2, 2
- [x] WITS penalties: 0, 0, 0, 0, 1, 1
- [x] CorruptionState is record type (not struct)
- [x] MaxApMultiplier is double (not float)
- [x] HasPenalties property documented

---

## Design Rationale

### Why No Mitigation?
- Creates permanent stakes
- Differentiates from recoverable Stress
- Corruption is "the clock" for long-term play

### Why Tiered Effects?
- Gradual degradation feels fair
- Players can manage early corruption
- Terminal isn't sudden (warnings at each tier)

### Why MaxAP Penalty?
- Mystics (magic users) are most affected by corruption
- Creates interesting high-risk Mystic playstyle
- Non-Mystics still suffer attribute penalties

### Why Six Tiers Instead of Five?
- Finer granularity for progression feel
- "Corrupted" tier provides early warning before severe penalties
- Matches 20-point threshold increments for intuitive math

### Why Terminal Error?
- Ultimate consequence for corruption accumulation
- Encourages corruption avoidance
- Creates permanent character loss stakes

### Why Co-Located with Trauma Service?
- Both are "psychological damage" systems
- Share similar resolution patterns
- Reduces service count without losing clarity

---

## Changelog

### v1.1.0 (2025-12-23)
- Fixed tier names to match code: Untainted→Pristine, Touched→Tainted, added Corrupted tier
- Fixed tier thresholds: 0-20, 21-40, 41-60, 61-80, 81-99, 100 (6 tiers)
- Fixed MaxAP multipliers: ×0.90 (Corrupted), ×0.80 (Blighted), ×0.60 (Fractured)
- Fixed CorruptionState from `readonly struct` to `record`
- Fixed MaxApMultiplier type from `float` to `double`
- Fixed property name from `TierName` to `TierDisplayName`
- Removed non-existent `Description` property
- Added `HasPenalties` property documentation
- Added Table of Contents
- Added Decision Trees (accumulation flow, tier determination)
- Added Related Services with line numbers
- Added Testing section with test files and scenarios
- Added Changelog

### v1.0.0 (2025-12-20)
- Initial specification
