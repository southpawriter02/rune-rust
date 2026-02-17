---
id: SPEC-ABILITY-001
title: Ability System
version: 1.0.0
status: Implemented
last_updated: 2025-12-23
related_specs: [SPEC-DICE-001, SPEC-COMBAT-001]
---

# SPEC-ABILITY-001: Ability System

> **Version:** 1.0.0
> **Status:** Implemented (v0.2.3c)
> **Services:** `AbilityService`, `EffectScriptExecutor`, `ResourceService`
> **Location:** `RuneAndRust.Engine/Services/`

---

## Overview

The Ability System implements active skills that characters can use during combat. Abilities are archetype-specific, have resource costs (Stamina/Aether), cooldowns, and execute effect scripts to deal damage, heal, or apply status effects.

This system differentiates character classes and provides tactical depth beyond basic attacks.

---

## Core Concepts

### Ability Properties
| Property | Description |
|----------|-------------|
| **Archetype** | Which class can use this ability |
| **Tier** | Level requirement (Starting, Level 3, Level 5) |
| **StaminaCost** | Physical resource consumed |
| **AetherCost** | Magical resource consumed (Mystics) |
| **CooldownTurns** | Turns before reuse |
| **Range** | 0 = self, 1+ = target selection |
| **EffectScript** | Semicolon-delimited effect commands |

### Archetypes and Focus
| Archetype | Primary Resource | Focus |
|-----------|------------------|-------|
| **Warrior** | Stamina | Heavy damage, self-buffs |
| **Skirmisher** | Stamina | Debuffs, positioning |
| **Adept** | Stamina | Support, utility |
| **Mystic** | Aether + Stamina | Magical effects, AoE |

---

## Behaviors

### Primary Behaviors

#### 1. Ability Usage Check (`CanUse`)

```csharp
bool CanUse(Combatant user, ActiveAbility ability)
```

**Validation Sequence:**
1. Check cooldown: `Cooldowns[ability.Id] > 0` → false
2. Check Stamina: `CanAfford(user, Stamina, cost)` → false if insufficient
3. Check Aether: `CanAfford(user, Aether, cost)` → false if insufficient
4. All checks pass → true

**Example:**
```csharp
if (!abilityService.CanUse(player, powerStrike))
{
    // Display reason: "Power Strike is on cooldown (2 turns)"
}
```

#### 2. Ability Execution (`Execute`)

```csharp
AbilityResult Execute(Combatant user, Combatant target, ActiveAbility ability)
```

**Execution Sequence:**
1. Validate usage (`CanUse`)
2. Deduct Stamina cost
3. Deduct Aether cost
4. Set cooldown: `user.Cooldowns[ability.Id] = ability.CooldownTurns`
5. Execute EffectScript via `EffectScriptExecutor`
6. Build result message
7. Return `AbilityResult`

**Example:**
```csharp
var result = abilityService.Execute(player, enemy, ability);
// "Valdris uses Power Strike on Ash-Vargr! 12 damage, Vulnerable applied."
```

#### 3. Cooldown Processing (`ProcessCooldowns`)

```csharp
void ProcessCooldowns(Combatant combatant)
```

**Called:** At start of each combatant's turn.

**Sequence:**
1. Iterate all cooldowns in `combatant.Cooldowns`
2. Decrement each by 1
3. If cooldown reaches 0 → remove from dictionary
4. Ability becomes usable again

#### 4. Cooldown Query (`GetCooldownRemaining`)

```csharp
int GetCooldownRemaining(Combatant combatant, Guid abilityId)
```

**Returns:** Turns remaining, or 0 if ready.

---

## Effect Script Language

### Supported Commands
| Command | Format | Description |
|---------|--------|-------------|
| **DAMAGE** | `DAMAGE:{type}:{amount}` | Deal damage to target |
| **HEAL** | `HEAL:{amount}` | Restore HP to target |
| **STATUS** | `STATUS:{effect}:{duration}` | Apply status effect |
| **BUFF** | `BUFF:{stat}:{amount}:{duration}` | Temporary stat boost |
| **STAMINA** | `STAMINA:{amount}` | Restore stamina |

### Damage Types
- `PHYSICAL` - Standard melee/ranged
- `FIRE` - Elemental fire
- `COLD` - Elemental ice
- `LIGHTNING` - Elemental shock
- `POISON` - Damage over time source
- `AETHER` - Magical damage

### Amount Notation
| Format | Example | Result |
|--------|---------|--------|
| Flat | `10` | Always 10 |
| Dice | `2d6` | Roll 2d6 |
| Dice+Flat | `1d8+3` | Roll 1d8, add 3 |

### Multi-Effect Scripts
```
DAMAGE:PHYSICAL:2d8;STATUS:BLEEDING:3
// Deals damage AND applies 3-turn bleeding
```

---

## Sample Abilities by Archetype

### Warrior Abilities
| Name | Tier | Cost | Cooldown | Effect |
|------|------|------|----------|--------|
| Power Strike | Starting | 20 STA | 0 | DAMAGE:PHYSICAL:2d8 |
| Shield Wall | Starting | 15 STA | 3 | STATUS:FORTIFIED:2 (self) |
| Crushing Blow | Level 3 | 30 STA | 2 | DAMAGE:PHYSICAL:3d8;STATUS:VULNERABLE:2 |
| Unstoppable | Level 5 | 40 STA | 5 | HEAL:20;STATUS:INSPIRED:3 (self) |

### Skirmisher Abilities
| Name | Tier | Cost | Cooldown | Effect |
|------|------|------|----------|--------|
| Quick Slash | Starting | 10 STA | 0 | DAMAGE:PHYSICAL:1d6+2 |
| Hamstring | Starting | 15 STA | 2 | DAMAGE:PHYSICAL:1d6;STATUS:SLOWED:2 |
| Poison Blade | Level 3 | 20 STA | 3 | DAMAGE:POISON:2d6;STATUS:POISONED:3 |

### Adept Abilities
| Name | Tier | Cost | Cooldown | Effect |
|------|------|------|----------|--------|
| Analyze | Starting | 10 STA | 2 | STATUS:VULNERABLE:2 (target) |
| Field Repair | Starting | 20 STA | 4 | HEAL:15 (self) |
| Overcharge | Level 3 | 25 STA | 3 | BUFF:MIGHT:2:3 (ally) |

### Mystic Abilities
| Name | Tier | Cost | Cooldown | Effect |
|------|------|------|----------|--------|
| Aether Bolt | Starting | 15 AP | 0 | DAMAGE:AETHER:2d6 |
| Runic Shield | Starting | 20 AP | 3 | STATUS:FORTIFIED:3 (self) |
| Blightfire | Level 3 | 30 AP | 2 | DAMAGE:FIRE:3d6;STATUS:BURNING:2 |
| Purify | Level 5 | 40 AP | 5 | HEAL:30;STATUS:BLESSED:2 |

---

## Restrictions

### Usage Restrictions
1. **Combat only** - Abilities can't be used in exploration
2. **Turn restriction** - Only usable on user's turn
3. **Resource requirement** - Must afford full cost
4. **Cooldown enforcement** - Must wait for cooldown

### Targeting Restrictions
1. **Range 0 = self only** - Cannot target others
2. **Range 1+ requires target** - Must specify valid target
3. **Auto-targeting** - Single enemy auto-selected
4. **No friendly fire** - Player can't target self with damage

### Archetype Lock
1. **Abilities are archetype-specific** - Warrior can't use Mystic abilities
2. **Tier locked by level** - Level 3 abilities need character level 3+

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Max abilities per archetype | ~5 | Starting + leveled |
| Max cooldown | 5 turns | Design maximum |
| Min cooldown | 0 | Spammable abilities |
| Max Stamina cost | 50 | Heavy abilities |
| Max Aether cost | 50 | Heavy spells |

### System Gaps
- No ability upgrade/evolution
- No ability combo system
- No passive abilities (all are active)
- No ability customization

---

## Use Cases

### UC-1: Execute Ability by Hotkey
```csharp
// Player presses '2' for second ability
var result = combatService.ExecutePlayerAbility(2, "Vargr");
// Internally calls abilityService.Execute()
```

### UC-2: Check Ability Availability for UI
```csharp
var abilityViews = player.Abilities
    .Select((ability, index) => new AbilityView(
        Hotkey: index + 1,
        Name: ability.Name,
        CostDisplay: FormatCost(ability),
        CooldownRemaining: abilityService.GetCooldownRemaining(player, ability.Id),
        IsUsable: abilityService.CanUse(player, ability)
    ))
    .ToList();
```

### UC-3: Self-Targeting Ability
```csharp
// Range 0 ability (Shield Wall)
var shieldWall = new ActiveAbility
{
    Range = 0,  // Self-target
    EffectScript = "STATUS:FORTIFIED:2"
};

// No target needed
var result = abilityService.Execute(player, player, shieldWall);
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `IResourceService` | [SPEC-ABILITY-001](SPEC-ABILITY-001.md) | Resource validation and deduction |
| `EffectScriptExecutor` | [SPEC-ABILITY-001](SPEC-ABILITY-001.md) | Effect execution |
| `ILogger` | Infrastructure | Operation tracing |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Player ability execution |
| `EnemyAIService` | [SPEC-ENEMY-001](SPEC-ENEMY-001.md) | Enemy ability selection |

---

## Related Services

### Primary Implementation
| File | Purpose |
|------|---------|
| `AbilityService.cs` | Ability execution, cooldowns |
| `EffectScriptExecutor.cs` | Script parsing/execution |
| `ResourceService.cs` | Stamina/Aether management |

### Supporting Types
| File | Purpose |
|------|---------|
| `ActiveAbility.cs` | Ability entity |
| `AbilityResult.cs` | Execution result |
| `ResourceType.cs` | Resource enum |

---

## Data Models

### ActiveAbility Entity
```csharp
public class ActiveAbility
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string EffectScript { get; set; }

    // Costs
    public int StaminaCost { get; set; }
    public int AetherCost { get; set; }
    public int CooldownTurns { get; set; }

    // Classification
    public ArchetypeType Archetype { get; set; }
    public int Tier { get; set; }  // 1 = Starting, 2 = Level 3, 3 = Level 5
    public int Range { get; set; } // 0 = self, 1+ = target
}
```

### AbilityResult
```csharp
public record AbilityResult(
    bool Success,
    string Message,
    int TotalDamage = 0,
    int TotalHealing = 0,
    List<string>? StatusesApplied = null
)
{
    public static AbilityResult Failure(string reason) =>
        new(false, reason);

    public static AbilityResult Ok(
        string message,
        int damage = 0,
        int healing = 0,
        List<string>? statuses = null) =>
        new(true, message, damage, healing, statuses);
}
```

### Combatant Cooldown Tracking
```csharp
public class Combatant
{
    // Key: AbilityId, Value: Turns remaining
    public Dictionary<Guid, int> Cooldowns { get; set; } = new();

    // Loaded on combat start
    public List<ActiveAbility> Abilities { get; set; } = new();
}
```

---

## ResourceService Integration

### Resource Types
```csharp
public enum ResourceType
{
    Stamina,
    Aether
}
```

### Affordability Check
```csharp
bool CanAfford(Combatant combatant, ResourceType type, int amount)
{
    return type switch
    {
        ResourceType.Stamina => combatant.CurrentStamina >= amount,
        ResourceType.Aether => combatant.CurrentAp >= amount,
        _ => false
    };
}
```

### Deduction
```csharp
void Deduct(Combatant combatant, ResourceType type, int amount)
{
    switch (type)
    {
        case ResourceType.Stamina:
            combatant.CurrentStamina -= amount;
            break;
        case ResourceType.Aether:
            combatant.CurrentAp -= amount;
            break;
    }
}
```

---

## EffectScriptExecutor Integration

### Execute Flow
```csharp
ScriptResult Execute(string script, Combatant target, string sourceName, Guid? sourceId)
{
    var commands = script.Split(';');
    foreach (var command in commands)
    {
        ProcessCommand(command, target);
    }
    return new ScriptResult(damage, healing, statuses, narrative);
}
```

### Result Aggregation
The executor collects:
- Total damage dealt
- Total healing applied
- List of status effects applied
- Combined narrative message

---

## Testing

### Test Files
- `AbilityServiceTests.cs`
- `EffectScriptExecutorTests.cs`

### Critical Test Scenarios
1. CanUse with sufficient resources
2. CanUse with insufficient Stamina
3. CanUse with insufficient Aether
4. CanUse with active cooldown
5. Execute deducts resources correctly
6. Execute sets cooldown correctly
7. ProcessCooldowns decrements properly
8. ProcessCooldowns removes expired cooldowns
9. Self-targeting (Range 0) works
10. Multi-effect scripts execute fully

---

## Design Rationale

### Why Effect Scripts?
- Data-driven ability creation
- No code changes for new abilities
- Shared executor with hazards/conditions

### Why Dual Resource (Stamina/Aether)?
- Separates physical and magical abilities
- Creates Mystic class identity
- Enables mixed-cost abilities (future)

### Why Cooldowns Instead of Charges?
- Simpler mental model
- Encourages ability cycling
- Predictable availability

### Why Archetype Lock?
- Creates class identity
- Prevents optimal builds from cherry-picking
- Encourages multiple playthroughs

### Why Tier System?
- Power progression within class
- Level gates for strong abilities
- Something to unlock at levels 3 and 5
