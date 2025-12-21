# SPEC-STATUS-001: Status Effect System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `StatusEffectService`
> **Location:** `RuneAndRust.Engine/Services/StatusEffectService.cs`

---

## Overview

The Status Effect System manages temporary combat modifiers that affect combatants. Effects can be beneficial (buffs) or detrimental (debuffs), have durations in turns, and may stack or apply damage over time (DoT).

This system creates tactical depth through buff/debuff management and timing.

---

## Status Effect Categories

### Damaging (DoT)
| Effect | Damage | Stacks? | Source |
|--------|--------|---------|--------|
| **Bleeding** | 2 per stack per turn | Yes | Physical attacks, abilities |
| **Poisoned** | 3 per stack per turn | Yes | Poison abilities, conditions |
| **Burning** | 4 per turn | No | Fire abilities, hazards |

### Debuffs
| Effect | Impact | Source |
|--------|--------|--------|
| **Stunned** | Cannot act for turn | Critical hits, abilities |
| **Vulnerable** | +50% damage received | Abilities, conditions |
| **Slowed** | Reduced initiative (future) | Abilities |
| **Disoriented** | -2 to all rolls (future) | Ambush, abilities |
| **Exhausted** | Halved rest recovery | No supplies during rest |

### Buffs
| Effect | Impact | Source |
|--------|--------|--------|
| **Defending** | +2 Defense until next turn | Defend action |
| **Fortified** | +soak per stack | Abilities |
| **Inspired** | +2 to attacks | Abilities |
| **Hasted** | +2 Initiative (future) | Abilities |
| **Blessed** | Stress reduction | Sacred ground |

---

## Behaviors

### Primary Behaviors

#### 1. Process Turn Start (`ProcessTurnStart`)

```csharp
int ProcessTurnStart(Combatant combatant)
```

**Sequence:**
1. Check for Bleeding → deal `2 × stacks` damage
2. Check for Poisoned → deal `3 × stacks` damage
3. Check for Burning → deal `4` flat damage
4. Return total DoT damage dealt

**Example:**
```csharp
// Combatant has Bleeding x2 and Burning
var dot = statusEffects.ProcessTurnStart(combatant);
// Bleeding: 2 × 2 = 4 damage
// Burning: 4 damage
// Returns: 8 total
```

#### 2. Process Turn End (`ProcessTurnEnd`)

```csharp
void ProcessTurnEnd(Combatant combatant)
```

**Sequence:**
1. Iterate all status effects
2. Decrement `RemainingTurns` by 1
3. If turns reach 0 → mark for removal
4. Remove expired effects
5. Reset `IsDefending` (handled separately in combat)

#### 3. Can Act Check (`CanAct`)

```csharp
bool CanAct(Combatant combatant)
```

**Returns:** `false` if combatant has `Stunned` effect.

**Used by:** `CombatService.NextTurn()` to skip stunned turns.

#### 4. Damage Multiplier (`GetDamageMultiplier`)

```csharp
float GetDamageMultiplier(Combatant combatant)
```

**Returns:**
- `1.5f` if `Vulnerable` active
- `1.0f` otherwise

**Used by:** `AttackResolutionService` for damage calculation.

#### 5. Soak Modifier (`GetSoakModifier`)

```csharp
int GetSoakModifier(Combatant combatant)
```

**Returns:**
- `+stacks` if `Fortified` active
- `0` otherwise

**Used by:** `AttackResolutionService` for damage reduction.

#### 6. Apply Status (`ApplyStatus`)

```csharp
void ApplyStatus(Combatant target, StatusEffectType type, int duration, int stacks = 1)
```

**Stack Behavior:**
- If already has effect and stackable → add stacks, refresh duration
- If already has effect and not stackable → refresh duration only
- If new → add to `StatusEffects` list

#### 7. Remove Status (`RemoveStatus`)

```csharp
void RemoveStatus(Combatant target, StatusEffectType type)
```

**Removes:** All instances of the specified effect type.

---

## Stack Mechanics

### Stacking Effects
| Effect | Stacks | Max Stacks | Behavior |
|--------|--------|------------|----------|
| Bleeding | Yes | 5 | Damage = 2 × stacks |
| Poisoned | Yes | 5 | Damage = 3 × stacks |
| Fortified | Yes | 5 | Soak = stacks |

### Non-Stacking Effects
| Effect | Behavior on Reapply |
|--------|---------------------|
| Burning | Refreshes duration |
| Stunned | Refreshes duration |
| Vulnerable | Refreshes duration |
| Defending | Refreshes (but usually 1 turn anyway) |

---

## Duration Rules

### Turn Counting
- Duration is in "turns of the affected combatant"
- Decremented at END of affected combatant's turn
- Effect active for its full final turn

### Example Timeline
```
Turn 1: Apply Bleeding x2 for 3 turns
Turn 1: Combatant takes 4 DoT, duration → 2
Turn 2: Combatant takes 4 DoT, duration → 1
Turn 3: Combatant takes 4 DoT, duration → 0 (removed)
```

---

## Restrictions

### Application Rules
1. **Combat only** - Most effects only apply during combat
2. **Persistent effects** - `Exhausted` persists outside combat
3. **Stack limits** - Cannot exceed max stack count

### Removal Rules
1. **Duration expiry** - Automatic at turn end
2. **Explicit removal** - `RemoveStatus()` call
3. **Combat end** - Most effects cleared on combat end
4. **Persistent exceptions** - `Exhausted` survives combat

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Max stacks | 5 | Per effect type |
| Max duration | Unbounded | Typically 1-5 turns |
| DoT damage | Fixed formula | 2/3/4 per type |
| Vulnerable multiplier | 1.5x | +50% damage |

### System Gaps
- No effect resistance/immunity
- No effect dispelling (only duration)
- No effect interaction (burning clears frozen, etc.)
- No effect triggers on apply/expire

---

## Use Cases

### UC-1: Bleeding from Ability
```csharp
// Ability with EffectScript: "DAMAGE:PHYSICAL:1d6;STATUS:BLEEDING:3"
statusEffects.ApplyStatus(target, StatusEffectType.Bleeding, duration: 3, stacks: 1);

// Each turn start
var dot = statusEffects.ProcessTurnStart(target);
// 2 damage from Bleeding x1
```

### UC-2: Stacking Poison
```csharp
// First poison application
statusEffects.ApplyStatus(target, StatusEffectType.Poisoned, 3, 2);
// Poisoned x2 for 3 turns

// Second poison (stacks)
statusEffects.ApplyStatus(target, StatusEffectType.Poisoned, 3, 1);
// Poisoned x3 for 3 turns (refreshed)

// Turn start: 3 × 3 = 9 poison damage
```

### UC-3: Defend Action
```csharp
// Player chooses defend
combatant.IsDefending = true;
statusEffects.ApplyStatus(combatant, StatusEffectType.Defending, 1);

// Until next turn:
// +2 Defense from IsDefending flag
// Effect cleared at turn start
```

### UC-4: Stun Skip Turn
```csharp
// Enemy applies stun
statusEffects.ApplyStatus(player, StatusEffectType.Stunned, 1);

// Next turn check
if (!statusEffects.CanAct(player))
{
    LogCombatEvent("You are stunned and lose your turn!");
    statusEffects.ProcessTurnEnd(player);
    NextTurn();  // Skip to next combatant
}
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Usage |
|---------|-------|
| `ILogger` | Effect application/removal tracing |

### Dependents (Provides To)
| Service | Usage |
|---------|-------|
| `CombatService` | DoT processing, stun check |
| `AttackResolutionService` | Damage multiplier, soak modifier |
| `ConditionService` | Status application from conditions |
| `AbilityService` | Status application from abilities |
| `HazardService` | Status application from hazards |

---

## Related Services

### Primary Implementation
| File | Purpose |
|------|---------|
| `StatusEffectService.cs` | Effect management |

### Supporting Types
| File | Purpose |
|------|---------|
| `ActiveStatusEffect.cs` | Effect instance |
| `StatusEffectType.cs` | Effect type enum |

---

## Data Models

### ActiveStatusEffect
```csharp
public class ActiveStatusEffect
{
    public StatusEffectType Type { get; set; }
    public int RemainingTurns { get; set; }
    public int Stacks { get; set; } = 1;
    public string? Source { get; set; }
}
```

### StatusEffectType Enum
```csharp
public enum StatusEffectType
{
    // DoT
    Bleeding,
    Poisoned,
    Burning,

    // Debuffs
    Stunned,
    Vulnerable,
    Slowed,
    Disoriented,

    // Buffs
    Defending,
    Fortified,
    Inspired,
    Hasted,
    Blessed,

    // Persistent
    Exhausted
}
```

### Combatant Integration
```csharp
public class Combatant
{
    public List<ActiveStatusEffect> StatusEffects { get; set; } = new();
    public bool IsDefending { get; set; }

    public bool HasStatus(StatusEffectType type) =>
        StatusEffects.Any(e => e.Type == type);

    public int GetStacks(StatusEffectType type) =>
        StatusEffects.FirstOrDefault(e => e.Type == type)?.Stacks ?? 0;
}
```

---

## Character Persistent Effects

### Character Entity Integration
```csharp
public class Character
{
    // Persists between combats
    public HashSet<StatusEffectType> ActiveStatusEffects { get; set; } = new();

    public bool HasStatusEffect(StatusEffectType type) =>
        ActiveStatusEffects.Contains(type);

    public void AddStatusEffect(StatusEffectType type) =>
        ActiveStatusEffects.Add(type);

    public void RemoveStatusEffect(StatusEffectType type) =>
        ActiveStatusEffects.Remove(type);
}
```

### Persistent Effect List
Currently only `Exhausted` persists outside combat.

---

## UI Display

### Effect Icons
```csharp
var effectIcons = combatant.StatusEffects.Select(e => e.Type switch
{
    StatusEffectType.Bleeding => $"[red]BLD×{e.Stacks}[/]",
    StatusEffectType.Poisoned => $"[green]PSN×{e.Stacks}[/]",
    StatusEffectType.Stunned => "[purple]STUN[/]",
    StatusEffectType.Vulnerable => "[orange1]VULN[/]",
    StatusEffectType.Fortified => $"[blue]FRT×{e.Stacks}[/]",
    StatusEffectType.Hasted => "[cyan]HAST[/]",
    StatusEffectType.Inspired => "[yellow]INSP[/]",
    _ => ""
});
```

---

## Testing

### Test Files
- `StatusEffectServiceTests.cs`

### Critical Test Scenarios
1. DoT damage calculation (all types)
2. Stack accumulation
3. Stack cap enforcement
4. Duration decrement
5. Effect expiry and removal
6. Stun blocks action
7. Vulnerable multiplier
8. Fortified soak bonus
9. Non-stacking effect refresh
10. Persistent effect (Exhausted) survival

---

## Design Rationale

### Why Duration-Based?
- Predictable for players
- Tactical timing decisions
- Cleaner than charge-based for combat pacing

### Why Stacking DoTs?
- Rewards focused strategies
- Creates build-up threat
- Stack cap prevents runaway damage

### Why Fixed DoT Values?
- Simple mental math
- Predictable damage planning
- Balance adjustable per effect type

### Why Separate Defending Flag?
- Reset logic is simpler
- Clear semantic meaning
- Doesn't need duration tracking

### Why Persistent Exhausted?
- Rest penalty needs to survive combat
- Creates meaningful resource pressure
- Cleared only at sanctuary

### Why No Effect Interactions?
- Keeps system simple
- Avoids complex priority rules
- Can add later if needed
