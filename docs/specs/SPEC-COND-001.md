# SPEC-COND-001: Ambient Condition System

> **Version:** 1.0.0
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
| **Poisoned** | Toxic air | -1 STURDINESS | Damage |
| **Burning** | Intense heat | -1 FINESSE | Damage |
| **Frozen** | Extreme cold | -2 FINESSE | Damage |
| **Radioactive** | Lingering radiation | -1 STURDINESS, -1 WILL | Stress |
| **Corrupted** | Aetheric corruption | -1 WILL | Corruption |
| **StaticField** | Electrical interference | -1 WITS | Damage (25% chance) |
| **Blessed** | Sacred ground | +1 WILL | Stress recovery |
| **Neutral** | No effect | None | None |

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
        ConditionType.Poisoned => new() { [Attribute.Sturdiness] = -1 },
        ConditionType.Burning => new() { [Attribute.Finesse] = -1 },
        ConditionType.Frozen => new() { [Attribute.Finesse] = -2 },
        ConditionType.Radioactive => new()
        {
            [Attribute.Sturdiness] = -1,
            [Attribute.Will] = -1
        },
        ConditionType.Corrupted => new() { [Attribute.Will] = -1 },
        ConditionType.StaticField => new() { [Attribute.Wits] = -1 },
        ConditionType.Blessed => new() { [Attribute.Will] = 1 },
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
| **DAMAGE** | `DAMAGE:{type}:{amount}` | `DAMAGE:FIRE:1d6` |
| **STRESS** | `STRESS:{amount}` | `STRESS:3` |
| **CORRUPTION** | `CORRUPTION:{amount}` | `CORRUPTION:1` |

### Multi-Command Scripts
```
DAMAGE:POISON:2d4;STRESS:2
// Deals poison damage AND applies stress
```

---

## Condition Definitions

### Standard Conditions
| Name | Type | Passive | TickScript | TickChance |
|------|------|---------|------------|------------|
| Noxious Fumes | Poisoned | -1 STU | DAMAGE:POISON:1d4 | 100% |
| Blazing Heat | Burning | -1 FIN | DAMAGE:FIRE:1d6 | 100% |
| Bitter Cold | Frozen | -2 FIN | DAMAGE:COLD:1d4 | 100% |
| Radiation Zone | Radioactive | -1 STU, -1 WIL | STRESS:3 | 100% |
| Blight Zone | Corrupted | -1 WIL | CORRUPTION:1 | 100% |
| Static Field | StaticField | -1 WIT | DAMAGE:LIGHTNING:2d6 | 25% |
| Sacred Ground | Blessed | +1 WIL | STRESS:-5 | 100% |

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
| Max passive penalty | -2 | Frozen (FINESSE) |
| Min tick chance | 0% | Effectively disabled |
| Max tick chance | 100% | Guaranteed effect |
| Corruption per tick | Typically 1 | Low to prevent Terminal Error rush |

### System Gaps
- No condition immunity
- No condition resistance (reduce effect)
- No condition clearing (except leaving room)
- No condition stacking

---

## Use Cases

### UC-1: Combat in Poisoned Room
```csharp
// On StartCombat
var condition = await conditionService.GetRoomConditionAsync(roomId);
if (condition != null)
{
    foreach (var combatant in state.TurnOrder)
    {
        conditionService.ApplyPassiveModifiers(combatant, condition.Type);
    }
    // All combatants now have -1 STURDINESS
}

// Each turn
if (combatant.ActiveCondition != null)
{
    var tickResult = await conditionService.ProcessTurnTickAsync(combatant, condition);
    if (tickResult.WasApplied)
    {
        // "Noxious Fumes: 3 damage."
        combatant.CurrentHp -= tickResult.DamageDealt;
    }
}
```

### UC-2: Static Field (Probabilistic Tick)
```csharp
// TickChance = 0.25 (25%)
// Each combatant's turn, 25% chance of 2d6 lightning damage
var tickResult = await conditionService.ProcessTurnTickAsync(combatant, staticField);
// May or may not apply damage this turn
```

### UC-3: Sanctuary (Blessed Ground)
```csharp
// Positive condition: +1 WILL, recovers 5 stress per turn
var blessed = new AmbientCondition
{
    Name = "Sacred Ground",
    Type = ConditionType.Blessed,
    TickScript = "STRESS:-5"  // Negative stress = recovery
};

// Combat in blessed room is safer, stress naturally drops
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Usage |
|---------|-------|
| `IRepository<AmbientCondition>` | Condition entity lookup |
| `IRoomRepository` | Room → Condition link |
| `EffectScriptExecutor` | DAMAGE command execution |
| `IDiceService` | Tick chance rolls, damage dice |
| `ILogger` | Event tracing |

### Dependents (Provides To)
| Service | Usage |
|---------|-------|
| `CombatService` | Passive modifiers, tick processing |
| `EnvironmentPopulator` | Procedural condition placement |

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
    public string Description { get; set; }  // AAM-VOICE compliant
    public string? Color { get; set; }        // UI rendering hint

    // Tick Effect Configuration
    public string? TickScript { get; set; }
    public float TickChance { get; set; } = 1.0f;  // 0.0-1.0

    // Procedural Placement
    public List<string> BiomeTags { get; set; } = new();
}
```

### ConditionTickResult
```csharp
public record ConditionTickResult(
    bool WasApplied,
    string ConditionName,
    string Message,
    int DamageDealt,
    int StressApplied,
    int CorruptionApplied
)
{
    public static ConditionTickResult None =>
        new(false, "", "", 0, 0, 0);
}
```

### ConditionType Enum
```csharp
public enum ConditionType
{
    Neutral,      // No effect
    Poisoned,     // Toxic
    Burning,      // Fire/heat
    Frozen,       // Cold/ice
    Radioactive,  // Radiation
    Corrupted,    // Aetheric blight
    StaticField,  // Electrical
    Blessed       // Positive (sacred)
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

## Procedural Population

### BiomeEnvironmentMapping Integration
```csharp
// EnvironmentPopulator assigns conditions based on biome
var conditionId = biomeMapping.GetConditionForBiome(room.BiomeType, room.DangerLevel);
if (conditionId != null)
{
    room.ConditionId = conditionId;
}
```

### Biome-Condition Mapping
| Biome | Common Conditions |
|-------|-------------------|
| Industrial | StaticField, Burning |
| Aetheric | Corrupted, Radioactive |
| Organic | Poisoned |
| Frozen | Frozen |
| Sacred | Blessed |

---

## Testing

### Test Files
- `ConditionServiceTests.cs`

### Critical Test Scenarios
1. Get room condition (present and absent)
2. Passive modifier application for all types
3. Tick processing with 100% chance
4. Tick processing with probability (25%)
5. Multi-command tick scripts
6. Stress tick (positive and negative)
7. Corruption tick
8. Modifier aggregation in Combatant.GetAttribute()

---

## Design Rationale

### Why Room-Level?
- Simplifies navigation ("this room is dangerous")
- Creates distinct tactical zones
- Supports dungeon design (avoid the radiation zone)

### Why Passive + Tick Separation?
- Passive affects decision making immediately
- Ticks create ongoing pressure
- Allows varied condition severity

### Why Tick Chance?
- Some conditions should be unpredictable
- Static Field (25%) creates tension without guarantee
- Allows condition balancing without changing damage

### Why Corruption Tick?
- Creates zones where time matters
- Connects environment to progression system
- Encourages speed vs. caution tradeoffs

### Why Blessed Condition?
- Not all conditions should be negative
- Creates safe havens worth finding
- Balances dungeon difficulty spikes
