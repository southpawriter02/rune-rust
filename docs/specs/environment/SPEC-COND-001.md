---
id: SPEC-COND-001
title: Ambient Condition System
version: 1.0.2
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-DICE-001, SPEC-COMBAT-001, SPEC-NAV-001]
---

# SPEC-COND-001: Ambient Condition System

> **Version:** 1.0.2
> **Status:** Implemented (v0.3.3b)
> **Service:** `ConditionService`
> **Location:** `RuneAndRust.Engine/Services/ConditionService.cs`

---

## Overview

The Ambient Condition System implements persistent environmental effects that affect rooms and all combatants within them. Conditions apply passive stat modifiers on combat entry and may deal periodic damage, stress, or corruption via tick effects each combat turn.

This system creates distinct zone atmospheres that influence tactical decisions.

---

## Core Concepts

### Ambient Conditions
- Room-level environmental effects
- Applied to all combatants on combat start
- May have turn-based tick effects
- Tied to biome types for procedural placement

### Condition Types
| Type | Theme | Passive Effect | Tick Effect |
|------|-------|----------------|-------------|
| **PsychicResonance** | Mental pressure | -1 WILL | +2 Stress |
| **ToxicAtmosphere** | Poisonous air | None | 1d4 Poison Dmg |
| **DeepCold** | Freezing temperature | -1 FINESSE | 1 Ice Dmg |
| **ScorchingHeat** | Extreme heat | -1 STURDINESS | 1 Fire Dmg |
| **LowVisibility** | Obscured vision | -2 WITS | None |
| **BlightedGround** | Corrupted terrain | -1 WILL, -1 WITS | +1 Corruption |
| **StaticField** | Electrical discharge | -1 FINESSE | 1d6 Lightning Dmg (25%) |
| **DreadPresence** | Ancient horror | -2 WILL | +3 Stress |

---

## Behaviors

### Primary Behaviors

#### 1. Get Room Condition (`GetRoomConditionAsync`)

```csharp
Task<AmbientCondition?> GetRoomConditionAsync(Guid roomId)
```

**Sequence:**
1. Load room from repository
2. Check if `ConditionId` is set
3. Load condition entity if present
4. Return condition or null

#### 2. Get Stat Modifiers (`GetStatModifiers`)

```csharp
Dictionary<Attribute, int> GetStatModifiers(ConditionType type)
```

**Returns:** Attribute penalties/bonuses for the condition type.

**Implementation:**
```csharp
// From ConditionTypeExtensions
public static Dictionary<Attribute, int> GetPassivePenalties(this ConditionType type)
{
    return type switch
    {
        ConditionType.PsychicResonance => new() { [Attribute.Will] = -1 },
        ConditionType.DeepCold => new() { [Attribute.Finesse] = -1 },
        ConditionType.ScorchingHeat => new() { [Attribute.Sturdiness] = -1 },
        ConditionType.LowVisibility => new() { [Attribute.Wits] = -2 },
        ConditionType.BlightedGround => new() { [Attribute.Will] = -1, [Attribute.Wits] = -1 },
        ConditionType.StaticField => new() { [Attribute.Finesse] = -1 },
        ConditionType.DreadPresence => new() { [Attribute.Will] = -2 },
        ConditionType.ToxicAtmosphere => new(), // None
        _ => new()
    };
}
```

#### 3. Apply Passive Modifiers (`ApplyPassiveModifiers`)

```csharp
void ApplyPassiveModifiers(Combatant combatant, ConditionType? conditionType)
```

**Sequence:**
1. Get penalties for condition type
2. Apply each penalty to combatant's condition modifiers:
   - `ConditionSturdinessModifier`
   - `ConditionFinesseModifier`
   - `ConditionWitsModifier`
   - `ConditionWillModifier`
3. Set `ActiveCondition` on combatant

**Called By:** `CombatService.StartCombat()` for all combatants.

#### 4. Process Turn Tick (`ProcessTurnTickAsync`)

```csharp
Task<ConditionTickResult> ProcessTurnTickAsync(
    Combatant combatant,
    AmbientCondition condition)
```

**Sequence:**
1. Check if condition has TickScript
2. Roll tick chance (if < 100%)
3. If triggered, parse and execute tick commands:
   - `DAMAGE:{type}:{amount}` → Apply damage
   - `STRESS:{amount}` → Add stress
   - `CORRUPTION:{amount}` → Add corruption
4. Build narrative message
5. Return result

**Tick Chance Example:**
```csharp
if (condition.TickChance < 1.0f)
{
    var roll = _diceService.RollSingle(100, "Condition tick chance");
    var threshold = (int)(condition.TickChance * 100);
    if (roll > threshold)
    {
        return ConditionTickResult.None; // No effect this turn
    }
}
```

---

## Tick Script Language

### Supported Commands
| Command | Format | Example |
|---------|--------|---------|
| **DAMAGE** | `DAMAGE:{type}:{amount}` | `DAMAGE:Fire:1d6` |
| **STRESS** | `STRESS:{amount}` | `STRESS:3` |
| **CORRUPTION** | `CORRUPTION:{amount}` | `CORRUPTION:1` |

### Multi-Command Scripts
```
DAMAGE:Poison:1d4;STRESS:2
// Deals poison damage AND applies stress
```

---

## Condition Definitions

### Standard Conditions (Seeded)
| Name | Type | Passive | TickScript | TickChance |
|------|------|---------|------------|------------|
| Psychic Resonance | PsychicResonance | -1 WIL | STRESS:2 | 100% |
| Toxic Atmosphere | ToxicAtmosphere | None | DAMAGE:Poison:1d4 | 100% |
| Deep Cold | DeepCold | -1 FIN | DAMAGE:Ice:1d1 | 100% |
| Scorching Heat | ScorchingHeat | -1 STU | DAMAGE:Fire:1d1 | 100% |
| Low Visibility | LowVisibility | -2 WIT | (None) | 0% |
| Blighted Ground | BlightedGround | -1 WIL, -1 WIT | CORRUPTION:1 | 100% |
| Static Field | StaticField | -1 FIN | DAMAGE:Lightning:1d6 | 25% |
| Dread Presence | DreadPresence | -2 WIL | STRESS:3 | 100% |

---

## Restrictions

### Application Rules
1. **One condition per room** - No stacking multiple conditions
2. **Applied on combat start** - Not during exploration
3. **Affects all combatants** - No immunity system (yet)

### Tick Processing
1. **Only during combat** - No exploration ticks
2. **Once per turn** - At turn start for active combatant
3. **Chance roll per combatant** - Independent rolls

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Max passive penalty | -2 | DreadPresence (WILL), LowVis (WITS) |
| Min tick chance | 0% | Effectively disabled (LowVis) |
| Max tick chance | 100% | Guaranteed effect |
| Corruption per tick | 1 | Low to prevent Terminal Error rush |

### System Gaps
- No condition immunity
- No condition resistance (reduce effect)
- No condition clearing (except leaving room)
- No condition stacking

---

## Use Cases

### UC-1: Combat in Toxic Room
```csharp
// On StartCombat
var condition = await conditionService.GetRoomConditionAsync(roomId);
if (condition != null)
{
    foreach (var combatant in state.TurnOrder)
    {
        conditionService.ApplyPassiveModifiers(combatant, condition.Type);
    }
}

// Each turn
if (combatant.ActiveCondition != null)
{
    var tickResult = await conditionService.ProcessTurnTickAsync(combatant, condition);
    if (tickResult.WasApplied)
    {
        // "Toxic Atmosphere: 3 Poison damage."
        combatant.CurrentHp -= tickResult.DamageDealt;
    }
}
```

### UC-2: Static Field (Probabilistic Tick)
```csharp
// TickChance = 0.25 (25%)
// Each combatant's turn, 25% chance of 1d6 lightning damage
var tickResult = await conditionService.ProcessTurnTickAsync(combatant, staticField);
// May or may not apply damage this turn
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `IRepository<AmbientCondition>` | Infrastructure | Condition entity lookup |
| `IRoomRepository` | Infrastructure | Room → Condition link |
| `EffectScriptExecutor` | [SPEC-ABILITY-001](SPEC-ABILITY-001.md) | DAMAGE command execution |
| `IDiceService` | [SPEC-DICE-001](SPEC-DICE-001.md) | Tick chance rolls, damage dice |
| `ILogger` | Infrastructure | Event tracing |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Passive modifiers, tick processing |
| `EnvironmentPopulator` | [SPEC-ENVPOP-001](SPEC-ENVPOP-001.md) | Procedural condition placement |

---

## Related Services

### Primary Implementation
| File | Purpose |
|------|---------|
| `ConditionService.cs` | Condition mechanics |
| `ConditionTypeExtensions.cs` | Passive penalties |

### Supporting Types
| File | Purpose |
|------|---------|
| `AmbientCondition.cs` | Condition entity |
| `ConditionTickResult.cs` | Tick operation result |
| `ConditionType.cs` | Type enum |

---

## Data Models

### AmbientCondition Entity
```csharp
public class AmbientCondition
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ConditionType Type { get; set; }
    public string Description { get; set; }
    public string? Color { get; set; }

    // Tick Effect Configuration
    public string? TickScript { get; set; }
    public float TickChance { get; set; } = 1.0f;  // 0.0-1.0

    // Procedural Placement
    public List<BiomeType> BiomeTags { get; set; } = new();
}
```

### ConditionType Enum
```csharp
public enum ConditionType
{
    PsychicResonance = 0,
    ToxicAtmosphere = 1,
    DeepCold = 2,
    ScorchingHeat = 3,
    LowVisibility = 4,
    BlightedGround = 5,
    StaticField = 6,
    DreadPresence = 7
}
```

---

## Combatant Condition Integration

### Condition Modifier Fields
```csharp
public class Combatant
{
    // Applied by ConditionService.ApplyPassiveModifiers
    public ConditionType? ActiveCondition { get; set; }
    public int ConditionSturdinessModifier { get; set; }
    public int ConditionFinesseModifier { get; set; }
    public int ConditionWitsModifier { get; set; }
    public int ConditionWillModifier { get; set; }

    // Include condition modifiers in attribute calculation
    public int GetAttribute(Attribute attr)
    {
        var baseValue = GetBaseAttribute(attr);
        var conditionMod = attr switch
        {
            Attribute.Sturdiness => ConditionSturdinessModifier,
            Attribute.Finesse => ConditionFinesseModifier,
            Attribute.Wits => ConditionWitsModifier,
            Attribute.Will => ConditionWillModifier,
            _ => 0
        };
        return baseValue + conditionMod;
    }
}
```

---

## Changelog

### v1.0.2 (2025-12-24)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- Added code traceability remarks to implementation files:
  - `IConditionService.cs` - interface spec reference
  - `ConditionService.cs` - service spec reference

### v1.0.1 (2025-12-22)
- Added BiomeTags to AmbientCondition entity (v0.3.3c)
- Documented ConditionSeeder integration

### v1.0.0 (2025-12-20)
**Initial Release:**
- Ambient Condition System specification
- 8 condition types with passive penalties and tick effects
- ConditionService interface and implementation
- Combatant integration for condition modifiers
