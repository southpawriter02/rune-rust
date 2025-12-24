---
id: SPEC-DICE-001
title: Dice Pool System
version: 1.0.1
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-COMBAT-001, SPEC-STATUS-001]
---

# SPEC-DICE-001: Dice Pool System

> **Version:** 1.0.1
> **Status:** Implemented
> **Service:** `DiceService`
> **Location:** `RuneAndRust.Engine/Services/DiceService.cs`

---

## Overview

The Dice Pool System is the foundational randomization layer for all probability-based mechanics in Rune & Rust. It implements a **d10 dice pool** system where players roll multiple ten-sided dice and count successes.

This system is intentionally simple and stateless, serving as a pure utility that other systems consume.

---

## Behaviors

### Primary Behaviors

#### 1. Pool Rolling (`Roll`)
Rolls a pool of d10 dice and counts successes and botches.

```csharp
DiceResult Roll(int poolSize, string context = "Unspecified")
```

**Mechanics:**
- Each die is rolled as a random value from 1-10 (inclusive)
- **Success:** Die result of 8, 9, or 10 (8+)
- **Botch:** Die result of exactly 1
- **Fumble Condition:** Zero successes AND at least one botch

**Example:**
```csharp
// Rolling for a MIGHT 5 attack
var result = _diceService.Roll(5, "Player Attack");
// result.Successes = 2
// result.Botches = 1
// result.Rolls = [3, 8, 1, 10, 5]
```

#### 2. Single Die Rolling (`RollSingle`)
Rolls a single die of any size for damage, random selection, etc.

```csharp
int RollSingle(int sides, string context = "Unspecified")
```

**Mechanics:**
- Returns a value from 1 to `sides` (inclusive)
- Used for weapon damage dice (d6, d8, d10, d12)
- Used for percentile rolls (d100) in ambush calculations

**Example:**
```csharp
// Rolling weapon damage
var damage = _diceService.RollSingle(8, "Iron Sword d8");
// Returns 1-8
```

### Edge Case Behaviors

#### Pool Size Validation
| Input | Behavior | Log Level |
|-------|----------|-----------|
| `poolSize < 0` | Clamps to 1, logs error | ERROR + WARNING |
| `poolSize == 0` | Clamps to 1, logs warning | WARNING |
| `poolSize > 0` | Uses as-is | TRACE |

**Rationale:** Negative pools indicate logic errors upstream. Zero pools should never occur but are recoverable. The system guarantees at least 1 die is always rolled.

#### Single Die Validation
| Input | Behavior |
|-------|----------|
| `sides < 1` | Clamps to 1, logs warning |
| `sides >= 1` | Uses as-is |

---

## Restrictions

### MUST NOT
1. **Seed the random number generator in production** - Uses `Random.Shared` for thread safety (optional seed parameter acceptable for testing)
2. **Persist roll history** - Rolls are ephemeral; logging handles audit trail
3. **Apply modifiers** - Modifier logic belongs in consuming services
4. **Interpret results** - Success/failure determination is caller responsibility

### MUST
1. **Always return consistent structure** - `DiceResult` is never null
2. **Log all rolls** - Every roll is logged at DEBUG level minimum
3. **Accept context parameter** - All rolls must be traceable

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Minimum pool size | 1 | Enforced via clamping |
| Maximum pool size | Unbounded | Practical limit ~100 for performance |
| Die sides | 1+ | No maximum; d100, d1000 supported |
| Success threshold | 8+ | Hardcoded, not configurable |
| Botch value | 1 | Hardcoded, not configurable |

### Threading
- `Random.Shared` is thread-safe in .NET 6+
- No locking required
- Safe for parallel combat resolution

---

## Use Cases

### UC-1: Attack Resolution
```csharp
// Attacker has MIGHT 6, attack type gives +1 modifier
var pool = attacker.Might + hitModifier; // 7
var roll = _dice.Roll(pool, $"{attacker.Name} Attack");

// Compare successes against defense threshold
var threshold = defender.Defense / 5; // e.g., 15 defense = 3 threshold
var netSuccesses = roll.Successes - threshold;
var isHit = netSuccesses > 0;
```

### UC-2: Stress Mitigation
```csharp
// WILL-based resolve check reduces incoming stress
var willPool = target.Will;
var resolveRoll = _dice.Roll(willPool, $"{target.Name} Resolve Check");

// Each success reduces stress by 1
var mitigatedStress = Math.Max(0, rawStress - resolveRoll.Successes);
```

### UC-3: Breaking Point Resolution
```csharp
// Roll WILL dice pool, need 3+ successes to stabilize
var resolveRoll = _dice.Roll(character.Will, "Breaking Point Resolve");

if (resolveRoll.Successes >= 3)
{
    // Stabilized - reset stress to 75
}
else if (resolveRoll.Successes == 0 && resolveRoll.Botches > 0)
{
    // Catastrophe - acquire trauma + stunned
}
else
{
    // Trauma acquired - stress reset to 50
}
```

### UC-4: Ambush Determination
```csharp
// Roll d100 against final risk percentage
var roll = _dice.RollSingle(100, "Ambush Determination");
var isAmbush = roll <= finalRiskPercent;
```

### UC-5: Weapon Damage
```csharp
// Roll weapon's damage die
var weaponDamage = _dice.RollSingle(
    attacker.WeaponDamageDie,  // e.g., 8 for d8
    $"{attacker.Name} Weapon ({attacker.WeaponName})"
);
var rawDamage = attacker.Might + weaponDamage + attackTypeBonus;
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Usage |
|---------|-------|
| `ILogger<DiceService>` | Roll traceability |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `AttackResolutionService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Attack/defense rolls |
| `TraumaService` | [SPEC-TRAUMA-001](SPEC-TRAUMA-001.md) | WILL resolve checks |
| `AmbushService` | [SPEC-REST-001](SPEC-REST-001.md) | WITS mitigation, d100 ambush |
| `HazardService` | [SPEC-HAZARD-001](SPEC-HAZARD-001.md) | Damage rolls via EffectScriptExecutor |
| `ConditionService` | [SPEC-COND-001](SPEC-COND-001.md) | Tick chance rolls |
| `InitiativeService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Initiative determination |

---

## Related Services

### Primary Implementation
- **File:** `RuneAndRust.Engine/Services/DiceService.cs`
- **Interface:** `RuneAndRust.Core/Interfaces/IDiceService.cs`

### Supporting Types
- **DiceResult:** `RuneAndRust.Core/Interfaces/IDiceService.cs` (nested record)

---

## Data Models

### DiceResult
```csharp
public record DiceResult(
    int Successes,          // Count of dice showing 8+
    int Botches,            // Count of dice showing 1
    IReadOnlyList<int> Rolls // Individual die values for logging
)
{
    public bool IsFumble => Successes == 0 && Botches > 0;
}
```

---

## Configuration

### Hardcoded Constants
| Constant | Value | Rationale |
|----------|-------|-----------|
| Success Threshold | 8+ | Core mechanic, not runtime configurable |
| Botch Value | 1 | Core mechanic, not runtime configurable |
| Die Type | d10 | Core mechanic, not runtime configurable |

**Design Decision:** These values are intentionally not configurable to prevent gameplay inconsistency and ensure all documentation/tooltips remain accurate.

---

## Testing

### Test Coverage
- **File:** `RuneAndRust.Tests/Engine/Services/DiceServiceTests.cs`
- **Coverage:** 100% line coverage

### Key Test Scenarios
1. Valid pool sizes produce correct result structure
2. Invalid pool sizes (0, negative) clamp to 1
3. Success counting is accurate
4. Botch counting is accurate
5. Fumble detection (0 successes + botches)
6. Single die rolls respect bounds
7. Context is passed to logger

### Testing Strategy
```csharp
[Fact]
public void Roll_ValidPool_CountsSuccessesCorrectly()
{
    // Arrange
    var logger = NSubstitute.Substitute.For<ILogger<DiceService>>();
    var service = new DiceService(logger);

    // Act - Roll many times for statistical validation
    var results = Enumerable.Range(0, 1000)
        .Select(_ => service.Roll(5, "Test"))
        .ToList();

    // Assert - Successes should be 0-5 range
    Assert.All(results, r => Assert.InRange(r.Successes, 0, 5));
}
```

---

## Logging Patterns

### Standard Log Messages
```
[TRACE] Rolling {PoolSize}d10 for {Context}
[TRACE] Die {DieNumber}: rolled {Value}
[DEBUG] Roll complete: {Successes} successes, {Botches} botches from {PoolSize}d10 [{Rolls}] ({Context})
[WARN]  FUMBLE! Zero successes with {Botches} botch(es) on {PoolSize}d10 ({Context})
[WARN]  Invalid dice pool {RequestedSize} for {Context}. Clamping to minimum of 1.
[ERROR] Negative dice pool {RequestedSize} requested for {Context}. This indicates a logic error.
```

---

## Design Rationale

### Why d10 Pool System?
1. **Granularity:** More nuanced than d20, less swingy than d6
2. **Mental Math:** Easy success counting (8, 9, 10)
3. **Scalability:** Pool size naturally represents competence
4. **Botch Mechanic:** Risk increases with more dice (more chances to roll 1)

### Why 8+ as Success?
- 30% success rate per die is mathematically balanced
- Provides meaningful difference between pool sizes 1-10
- Allows for both "impossible" (1d10) and "trivial" (10d10) challenges

### Why Stateless?
- Enables parallel processing
- Simplifies testing (no state management)
- Clear separation of concerns (rolling vs. interpretation)

---

## Changelog

### v1.0.1 (2025-12-24)
**Documentation Update:**
- Added `last_updated` field to frontmatter
- Added `related_specs` linking to SPEC-COMBAT-001 and SPEC-STATUS-001
- Clarified seeding restriction applies to production (test seeding is acceptable)
- Added code traceability remarks to interface and service

### v1.0.0 (Initial)
**Initial Release:**
- d10 dice pool system with success threshold (8+) and botch detection (1)
- RollSingle for damage dice (d4, d6, d8, etc.)
- Comprehensive logging at Trace, Debug, Warning levels
- DiceResult record with Successes, Botches, Rolls properties
