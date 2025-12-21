# SPEC-STATUS-001: Status Effect System

> **Version:** 1.0.1
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
| **Bleeding** | 1d6 per stack per turn | Yes | Physical attacks, abilities |
| **Poisoned** | 1d6 per stack per turn | Yes | Poison abilities, conditions |

### Debuffs
| Effect | Impact | Source |
|--------|--------|--------|
| **Stunned** | Cannot act for turn | Critical hits, abilities |
| **Vulnerable** | +50% damage received | Abilities, conditions |
| **Disoriented** | -1 to all dice pools | Ambush, Breaking Point recovery |
| **Exhausted** | Halved rest recovery | No supplies during rest |

### Buffs
| Effect | Impact | Source |
|--------|--------|--------|
| **Defending** | (Flag) +2 Defense | Defend action |
| **Fortified** | +2 soak per stack | Abilities |
| **Inspired** | +1 bonus die to damage | Abilities |
| **Hasted** | +1 action per turn | Abilities |

---

## Behaviors

### Primary Behaviors

#### 1. Process Turn Start (`ProcessTurnStart`)

```csharp
int ProcessTurnStart(Combatant combatant)
```

**Sequence:**
1. Check for Bleeding → deal `1d6` damage per stack (ignores soak)
2. Check for Poisoned → deal `1d6` damage per stack (applies soak)
3. Return total DoT damage dealt

**Example:**
```csharp
// Combatant has Bleeding x2
var dot = statusEffects.ProcessTurnStart(combatant);
// Bleeding: 2d6 rolled (e.g., 3 + 5 = 8)
// Returns: 8 total
```

#### 2. Process Turn End (`ProcessTurnEnd`)

```csharp
void ProcessTurnEnd(Combatant combatant)
```

**Sequence:**
1. Iterate all status effects
2. Decrement `DurationRemaining` by 1
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
- `+2 × stacks` if `Fortified` active
- `0` otherwise

**Used by:** `AttackResolutionService` for damage reduction.

#### 6. Apply Effect (`ApplyEffect`)

```csharp
void ApplyEffect(Combatant target, StatusEffectType type, int duration, Guid sourceId)
```

**Stack Behavior:**
- If already has effect and stackable → add stacks (up to 5), refresh duration
- If already has effect and not stackable → refresh duration only
- If new → add to `StatusEffects` list

#### 7. Remove Effect (`RemoveEffect`)

```csharp
void RemoveEffect(Combatant target, StatusEffectType type)
```

**Removes:** All instances of the specified effect type.

---

## Stack Mechanics

### Stacking Effects
| Effect | Stacks | Max Stacks | Behavior |
|--------|--------|------------|----------|
| Bleeding | Yes | 5 | Damage = 1d6 per stack |
| Poisoned | Yes | 5 | Damage = 1d6 per stack |
| Fortified | Yes | 5 | Soak = 2 per stack |

### Non-Stacking Effects
| Effect | Behavior on Reapply |
|--------|---------------------|
| Stunned | Refreshes duration |
| Vulnerable | Refreshes duration |
| Disoriented | Refreshes duration |
| Exhausted | Refreshes duration |
| Defending | Refreshes (but usually 1 turn anyway) |
| Inspired | Refreshes duration |
| Hasted | Refreshes duration |

---

## Duration Rules

### Turn Counting
- Duration is in "turns of the affected combatant"
- Decremented at END of affected combatant's turn
- Effect active for its full final turn

### Example Timeline
```
Turn 1: Apply Bleeding x2 for 3 turns
Turn 1: Combatant takes DoT, duration → 2
Turn 2: Combatant takes DoT, duration → 1
Turn 3: Combatant takes DoT, duration → 0 (removed)
```

---

## Restrictions

### Application Rules
1. **Combat only** - Most effects only apply during combat
2. **Persistent effects** - `Exhausted` persists outside combat
3. **Stack limits** - Cannot exceed max stack count (5)

### Removal Rules
1. **Duration expiry** - Automatic at turn end
2. **Explicit removal** - `RemoveEffect()` call
3. **Combat end** - Most effects cleared on combat end
4. **Persistent exceptions** - `Exhausted` survives combat

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Max stacks | 5 | Per effect type |
| Max duration | Unbounded | Typically 1-5 turns |
| DoT damage | 1d6 | Per stack |
| Vulnerable multiplier | 1.5x | +50% damage |
| Fortified bonus | +2 | Soak per stack |

### System Gaps
- No effect resistance/immunity
- No effect dispelling (only duration)
- No effect interaction (burning clears frozen, etc.)
- No effect triggers on apply/expire

---

## Use Cases

### UC-1: Bleeding from Ability
```csharp
// Ability with EffectScript: "DAMAGE:PHYSICAL:1d6;STATUS:Bleeding:3"
_statusEffectService.ApplyEffect(target, StatusEffectType.Bleeding, duration: 3, sourceId);

// Each turn start
var dot = _statusEffectService.ProcessTurnStart(target);
// 1d6 damage from Bleeding x1 (Ignores Armor)
```

### UC-2: Stacking Poison
```csharp
// First poison application (2 stacks)
_statusEffectService.ApplyEffect(target, StatusEffectType.Poisoned, 3, sourceId);
_statusEffectService.ApplyEffect(target, StatusEffectType.Poisoned, 3, sourceId);
// Poisoned x2 for 3 turns

// Second poison (stacks +1)
_statusEffectService.ApplyEffect(target, StatusEffectType.Poisoned, 3, sourceId);
// Poisoned x3 for 3 turns (refreshed)

// Turn start: 3d6 poison damage (Subject to Armor)
```

### UC-3: Defend Action
```csharp
// Player chooses defend
combatant.IsDefending = true;

// Until next turn:
// +2 Defense from IsDefending flag
// Flag cleared at turn start
```

### UC-4: Stun Skip Turn
```csharp
// Enemy applies stun
_statusEffectService.ApplyEffect(player, StatusEffectType.Stunned, 1, sourceId);

// Next turn check
if (!_statusEffectService.CanAct(player))
{
    LogCombatEvent("You are stunned and lose your turn!");
    _statusEffectService.ProcessTurnEnd(player);
    NextTurn();  // Skip to next combatant
}
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Usage |
|---------|-------|
| `IDiceService` | DoT damage rolls |
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
    public int DurationRemaining { get; set; }
    public int Stacks { get; set; } = 1;
    public Guid SourceId { get; set; }
}
```

### StatusEffectType Enum
```csharp
public enum StatusEffectType
{
    // Debuffs
    Bleeding = 0,     // 1d6/stack (True Dmg)
    Poisoned = 1,     // 1d6/stack (Soakable)
    Stunned = 2,      // Skip turn
    Vulnerable = 3,   // +50% dmg taken
    Disoriented = 4,  // -1 to dice pools
    Exhausted = 5,    // Halved recovery

    // Buffs
    Fortified = 100,  // +2 Soak/stack
    Hasted = 101,     // +1 Action (Reserved)
    Inspired = 102    // +1 Dmg Die (Reserved)
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

## Design Rationale

### Why 1d6 DoT?
- Standardizes damage rolls
- Scales cleanly with stacks (2d6, 3d6)
- "Bleeding" ignores armor (internal injury) vs "Poisoned" which is soakable (bodily resistance)

### Why No Burning?
- Removed to simplify elemental effects for now
- Can be re-added if fire mechanics become central

### Why Persistent Exhausted?
- Rest penalty needs to survive combat
- Creates meaningful resource pressure
- Cleared only at sanctuary
