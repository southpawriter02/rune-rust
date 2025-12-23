---
id: SPEC-CORRUPT-001
title: Corruption System
version: 1.0.0
status: Implemented
related_specs: [SPEC-CHAR-001, SPEC-COMBAT-001]
---

# SPEC-CORRUPT-001: Corruption System

> **Version:** 1.0.0
> **Status:** Implemented (v0.3.0b)
> **Service:** `TraumaService` (Corruption methods)
> **Location:** `RuneAndRust.Engine/Services/TraumaService.cs`

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

| Tier | Range | Effects |
|------|-------|---------|
| **Untainted** | 0-24 | No effects |
| **Touched** | 25-49 | Cosmetic manifestations only |
| **Blighted** | 50-74 | WILL -1, MaxAP ×0.75 |
| **Fractured** | 75-99 | WILL -2, WITS -1, MaxAP ×0.5 |
| **Terminal** | 100 | Character lost (Forlorn) |

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
// If player was at 45, now at 60 → Tier changed to "Blighted"!
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
// If player was at 60, now at 35 → Tier improved to "Touched"!
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
public readonly struct CorruptionState
{
    public int Value { get; }           // Current corruption (0-100)
    public CorruptionTier Tier { get; } // Current tier enum
    public bool IsTerminal { get; }     // Value >= 100

    // Attribute Penalties
    public int WillPenalty { get; }     // 0, 1, or 2
    public int WitsPenalty { get; }     // 0 or 1

    // Aether Pool Penalties
    public float MaxApMultiplier { get; }      // 1.0, 0.75, or 0.5
    public int MaxApPenaltyPercent { get; }    // 0, 25, or 50

    // Descriptive
    public string TierName { get; }
    public string Description { get; }
}
```

### Tier Effect Table

| Property | Untainted | Touched | Blighted | Fractured |
|----------|-----------|---------|----------|-----------|
| WillPenalty | 0 | 0 | 1 | 2 |
| WitsPenalty | 0 | 0 | 0 | 1 |
| MaxApMultiplier | 1.0 | 1.0 | 0.75 | 0.5 |
| MaxApPenaltyPercent | 0% | 0% | 25% | 50% |

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
| Max WILL penalty | 2 | At Fractured tier |
| Max WITS penalty | 1 | At Fractured tier |
| Max AP penalty | 50% | At Fractured tier |

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

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `ILogger` | Infrastructure | Terminal Error logging |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `StatCalculationService` | [SPEC-CHAR-001](SPEC-CHAR-001.md) | Attribute penalties, MaxAP modifier |
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Combat log corruption events |
| `ConditionService` | [SPEC-COND-001](SPEC-COND-001.md) | Corruption tick effects |

---

## Related Services

### Primary Implementation
| File | Purpose |
|------|---------|
| `TraumaService.cs` | Corruption methods (co-located with stress) |

### Supporting Types
| File | Purpose |
|------|---------|
| `CorruptionState.cs` | Tier calculation value object |
| `CorruptionResult.cs` | Operation result record |
| `CorruptionTier.cs` | Tier enum |

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
    Untainted,  // 0-24
    Touched,    // 25-49
    Blighted,   // 50-74
    Fractured,  // 75-99
    Terminal    // 100 (Forlorn)
}
```

---

## Integration with StatCalculationService

### RecalculateDerivedStats Integration
```csharp
// In StatCalculationService.RecalculateDerivedStats()
var corruptionState = new CorruptionState(character.Corruption);

// Apply attribute penalties
effectiveWill = Math.Max(1, effectiveWill - corruptionState.WillPenalty);
effectiveWits = Math.Max(1, effectiveWits - corruptionState.WitsPenalty);

// Apply MaxAP penalty (Mystics only)
var baseMaxAp = CalculateBaseMaxAp(character.Archetype, effectiveWill);
character.MaxAp = (int)(baseMaxAp * corruptionState.MaxApMultiplier);
```

---

## Configuration

### Tier Thresholds
```csharp
public CorruptionTier Tier => Value switch
{
    >= 100 => CorruptionTier.Terminal,
    >= 75 => CorruptionTier.Fractured,
    >= 50 => CorruptionTier.Blighted,
    >= 25 => CorruptionTier.Touched,
    _ => CorruptionTier.Untainted
};
```

### Penalty Values
```csharp
public int WillPenalty => Tier switch
{
    CorruptionTier.Fractured => 2,
    CorruptionTier.Blighted => 1,
    _ => 0
};

public int WitsPenalty => Tier switch
{
    CorruptionTier.Fractured => 1,
    _ => 0
};

public float MaxApMultiplier => Tier switch
{
    CorruptionTier.Fractured => 0.5f,
    CorruptionTier.Blighted => 0.75f,
    _ => 1.0f
};
```

---

## Testing

### Test Files
- `TraumaServiceTests.cs` (corruption sections)
- `CorruptionStateTests.cs`

### Critical Test Scenarios
1. Corruption accumulation without mitigation
2. Tier threshold crossing (all transitions)
3. Terminal Error at exactly 100
4. Purge mechanics and tier improvement
5. Attribute penalty application
6. MaxAP multiplier calculation
7. Non-player combatant handling
8. Edge cases (0, 100, overflow)

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

### Why Terminal Error?
- Ultimate consequence for corruption accumulation
- Encourages corruption avoidance
- Creates permanent character loss stakes

### Why Co-Located with Trauma Service?
- Both are "psychological damage" systems
- Share similar resolution patterns
- Reduces service count without losing clarity
