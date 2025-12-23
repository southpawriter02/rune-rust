---
id: SPEC-COMBAT-001
title: Combat System
version: 1.0.0
status: Implemented
related_specs: [SPEC-DICE-001, SPEC-TRAUMA-001, SPEC-ABILITY-001, SPEC-STATUS-001, SPEC-HAZARD-001, SPEC-COND-001, SPEC-ENEMY-001]
---

# SPEC-COMBAT-001: Combat System

> **Version:** 1.0.0
> **Status:** Implemented
> **Services:** `CombatService`, `AttackResolutionService`, `InitiativeService`, `EnemyAIService`
> **Location:** `RuneAndRust.Engine/Services/`

---

## Overview

The Combat System manages turn-based tactical encounters between the player and enemies. It orchestrates initiative, attack resolution, ability usage, status effects, and victory/defeat conditions while integrating with hazards, conditions, and trauma systems.

Combat follows a round-based structure where combatants act in initiative order. Each combatant has one turn per round to attack, use abilities, defend, or pass.

---

## Behaviors

### Primary Behaviors

#### 1. Combat Initialization (`StartCombat`)
Establishes the combat encounter with proper state setup.

```csharp
void StartCombat(List<Enemy> enemies)
```

**Sequence:**
1. Validate active character exists
2. Create new `CombatState` instance
3. Load player abilities from `ActiveAbilityRepository` by archetype
4. Create `Combatant` wrapper for player with abilities attached
5. Create `Combatant` wrappers for each enemy
6. Roll initiative for all combatants
7. Apply ambient condition modifiers (v0.3.3b) to all combatants
8. Sort turn order by initiative (descending)
9. Set `GamePhase.Combat`
10. Initialize combat log

**Ambient Condition Integration:**
```csharp
if (_gameState.CurrentRoomId.HasValue)
{
    var condition = await _conditionService.GetRoomConditionAsync(roomId);
    if (condition != null)
    {
        foreach (var combatant in state.TurnOrder)
        {
            _conditionService.ApplyPassiveModifiers(combatant, condition.Type);
        }
    }
}
```

#### 2. Turn Advancement (`NextTurn`)
Progresses combat to the next combatant's turn.

**Turn Start Processing:**
1. Increment turn index (wrap to round 0 at end)
2. Process status effect DoT damage
3. Check for DoT death → remove combatant, check victory
4. Process trait regeneration (v0.2.2c)
5. Process stamina regeneration
6. Process ability cooldowns
7. Process trauma triggers for player (v0.3.0c)
8. Process ambient condition tick (v0.3.3b)
9. Reset defending stance
10. Check stunned status → skip turn if stunned

**Stun Handling:**
```csharp
if (!_statusEffects.CanAct(active))
{
    LogCombatEvent($"{active.Name} is stunned and loses their turn!");
    _statusEffects.ProcessTurnEnd(active);
    NextTurn(); // Recursive skip
    return;
}
```

#### 3. Player Attack Execution (`ExecutePlayerAttack`)
Resolves a player-initiated attack against an enemy.

```csharp
string ExecutePlayerAttack(string targetName, AttackType attackType)
```

**Attack Types:**
| Type | Stamina Cost | Hit Modifier | Damage Bonus |
|------|--------------|--------------|--------------|
| Light | 15 | +1 | +0 |
| Standard | 25 | +0 | +2 |
| Heavy | 40 | -1 | +4 |

**Resolution Sequence:**
1. Validate combat is active and it's player's turn
2. Find target by name (case-insensitive partial match)
3. Check stamina affordability
4. Deduct stamina cost
5. Roll attack pool (`MIGHT + hitModifier`)
6. Calculate defense threshold (`Defense ÷ 5`)
7. Determine outcome based on net successes
8. Roll weapon damage on hit
9. Apply damage modifiers (glancing/critical)
10. Apply vulnerability multiplier
11. Subtract soak (with Fortified modifier)
12. Apply damage to target
13. Process thorns damage (v0.2.2c) if applicable
14. Check for death → remove combatant
15. Check victory condition
16. Process turn end effects

**Outcome Classification:**
| Net Successes | Outcome | Damage Modifier |
|---------------|---------|-----------------|
| ≤0 | Miss | 0 |
| 0 + Botches | Fumble | 0 |
| 1-2 | Glancing | ×0.5 |
| 3-4 | Solid | ×1.0 |
| 5+ | Critical | ×2.0 |

#### 4. Enemy AI Processing (`ProcessEnemyTurnAsync`)
Handles autonomous enemy decision-making and action execution.

```csharp
async Task ProcessEnemyTurnAsync(Combatant enemy)
```

**AI Decision Flow:**
1. Wait 750ms for UX pacing
2. Reset defending state
3. Query `EnemyAIService.DetermineAction()` for decision
4. Execute action based on type:
   - `Attack` → `ExecuteEnemyAttack()`
   - `Defend` → `ProcessDefend()`
   - `Flee` → `ProcessFlee()`
   - `Pass` → Log hesitation
5. Process turn end effects
6. Check player death
7. Check victory (enemy fled)
8. Advance to next turn

**Vampiric Healing (v0.2.2c):**
```csharp
var vampiricHeal = _traitService.ProcessTraitOnDamageDealt(attacker, damage);
if (vampiricHeal > 0)
{
    LogCombatEvent($"{attacker.Name} drains {vampiricHeal} HP!");
}
```

#### 5. Ability Execution (`ExecutePlayerAbility`)
Executes a player ability by hotkey or name.

```csharp
string ExecutePlayerAbility(int hotkey, string? targetName = null)
string ExecutePlayerAbility(string abilityName, string? targetName = null)
```

**Target Resolution Logic:**
- Range 0 abilities → self-targeting
- Single enemy → auto-target
- Multiple enemies → require explicit target
- Explicit target → case-insensitive partial match

**Ability Cost Types:**
| Resource | Description |
|----------|-------------|
| Stamina | Physical exertion cost |
| Aether | Magical energy cost (Mystics only) |

#### 6. Combat Ending (`EndCombat`)
Concludes combat and distributes rewards.

```csharp
CombatResult? EndCombat()
```

**Victory Rewards:**
- XP: Base 50 (placeholder for enemy XP values)
- Loot: Generated via `LootService` based on biome/danger

**Cleanup:**
- Set `GamePhase.Exploration`
- Clear `CombatState` from `GameState`

### Edge Case Behaviors

#### Recursive Turn Advancement
When a combatant is killed by DoT or loses turn to stun, `NextTurn()` recursively calls itself to find the next active combatant.

#### Turn Index Adjustment on Death
When a combatant is removed mid-round:
- If removed index < current turn index → decrement turn index
- If removed index == current turn index at end → wrap to 0

#### Empty Enemy List
`StartCombat()` with empty enemy list still initializes combat but immediately triggers victory on first check.

---

## Restrictions

### Combat State Requirements
1. **Must have active character** - `StartCombat` returns immediately if no character
2. **Must be player's turn for player actions** - All player actions validate `IsPlayerTurn`
3. **Cannot attack non-existent targets** - Target lookup required
4. **Cannot act while stunned** - Turn automatically skipped

### Turn Order Integrity
1. **No out-of-order actions** - Actions only valid during combatant's turn
2. **No simultaneous attacks** - Strictly sequential resolution
3. **Death processed immediately** - Combatant removed before continuing

### Resource Validation
1. **Stamina checked before attack** - Insufficient stamina blocks action
2. **Ability costs deducted atomically** - All costs deducted before execution

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Max combat log entries | 10 | Rolling buffer via `Queue<string>` |
| Enemy action delay | 750ms | Hardcoded for UX |
| Minimum damage on hit | 1 | Even with soak, hits deal damage |
| Max defense penalty from stress | 5 | At 100 stress |

### Concurrency
- Combat is single-threaded per session
- No parallel player/enemy action processing
- Async used only for UX delays

### Target Limitations
- Single target attacks only (no AoE in current implementation)
- No friendly fire
- No terrain/positioning system

---

## Use Cases

### UC-1: Standard Combat Flow
```csharp
// 1. Start combat
var enemies = enemyFactory.Create("bst_vargr_01", 2);
combatService.StartCombat(enemies);

// 2. Player turn
var result = combatService.ExecutePlayerAttack("Vargr", AttackType.Standard);
combatService.NextTurn();

// 3. Enemy turns (async processing)
while (!combatState.IsPlayerTurn)
{
    await combatService.ProcessEnemyTurnAsync(combatState.ActiveCombatant);
}

// 4. Repeat until victory/defeat
```

### UC-2: Ability Usage
```csharp
// By hotkey (1-based index)
var result = combatService.ExecutePlayerAbility(1, "Vargr");

// By name
var result = combatService.ExecutePlayerAbility("Power Strike", "Vargr");

// Self-targeting ability (no target needed)
var result = combatService.ExecutePlayerAbility("Second Wind");
```

### UC-3: Defend Action
```csharp
// Player defends (increases defense until next turn)
combatService.ProcessDefend(playerCombatant);
// Sets IsDefending = true, provides defensive bonuses
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `GameState` | [SPEC-GAME-001](SPEC-GAME-001.md) | Phase management, current character/room |
| `IInitiativeService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Turn order calculation |
| `IAttackResolutionService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Attack/defense resolution |
| `ILootService` | [SPEC-INV-001](SPEC-INV-001.md) | Victory reward generation |
| `IStatusEffectService` | [SPEC-STATUS-001](SPEC-STATUS-001.md) | DoT processing, modifier queries |
| `IEnemyAIService` | [SPEC-ENEMY-001](SPEC-ENEMY-001.md) | Enemy decision making |
| `ICreatureTraitService` | [SPEC-TRAIT-001](SPEC-TRAIT-001.md) | Regen, thorns, vampiric, explosive |
| `IResourceService` | [SPEC-ABILITY-001](SPEC-ABILITY-001.md) | Stamina regeneration |
| `IAbilityService` | [SPEC-ABILITY-001](SPEC-ABILITY-001.md) | Ability execution, cooldowns |
| `IActiveAbilityRepository` | [SPEC-ABILITY-001](SPEC-ABILITY-001.md) | Player ability loading |
| `ITraumaService` | [SPEC-TRAUMA-001](SPEC-TRAUMA-001.md) | Stress/trauma triggers |
| `IHazardService` | [SPEC-HAZARD-001](SPEC-HAZARD-001.md) | Damage-triggered hazards (v0.3.3a) |
| `IConditionService` | [SPEC-COND-001](SPEC-COND-001.md) | Ambient condition effects (v0.3.3b) |
| `IRoomRepository` | [SPEC-NAV-001](SPEC-NAV-001.md) | Current room for hazard/condition lookup |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `GameService` | [SPEC-GAME-001](SPEC-GAME-001.md) | Main game loop orchestration |
| `CombatScreenRenderer` | UI Layer | ViewModel generation |

---

## Related Services

### Primary Implementation
| File | Purpose |
|------|---------|
| `CombatService.cs` | Combat lifecycle orchestration |
| `AttackResolutionService.cs` | Attack mechanics, damage calculation |
| `InitiativeService.cs` | Turn order determination |
| `EnemyAIService.cs` | Enemy behavior trees |
| `StatusEffectService.cs` | Combat modifiers, DoT |
| `CreatureTraitService.cs` | Elite/Champion abilities |

---

## Data Models

### CombatState
```csharp
public class CombatState
{
    public List<Combatant> TurnOrder { get; set; }
    public int RoundNumber { get; set; } = 1;
    public int TurnIndex { get; set; } = 0;
    public Combatant? ActiveCombatant => TurnOrder.ElementAtOrDefault(TurnIndex);
    public bool IsPlayerTurn => ActiveCombatant?.IsPlayer ?? false;
}
```

### Combatant
```csharp
public class Combatant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsPlayer { get; set; }
    public Character? CharacterSource { get; set; }
    public Enemy? EnemySource { get; set; }

    // Stats
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentStress { get; set; }
    public int CurrentCorruption { get; set; }
    public int CurrentAp { get; set; }

    // Combat State
    public bool IsDefending { get; set; }
    public int Initiative { get; set; }
    public List<ActiveStatusEffect> StatusEffects { get; set; }
    public Dictionary<Guid, int> Cooldowns { get; set; }
    public List<ActiveAbility> Abilities { get; set; }

    // Equipment
    public int WeaponDamageDie { get; set; }
    public string WeaponName { get; set; }
    public int ArmorSoak { get; set; }

    // Condition Modifiers (v0.3.3b)
    public ConditionType? ActiveCondition { get; set; }
    public int ConditionSturdinessModifier { get; set; }
    public int ConditionFinesseModifier { get; set; }
    public int ConditionWitsModifier { get; set; }
    public int ConditionWillModifier { get; set; }
}
```

### AttackResult
```csharp
public record AttackResult(
    AttackOutcome Outcome,
    int NetSuccesses,
    int RawDamage,
    int FinalDamage,
    bool IsHit
);
```

### CombatResult
```csharp
public record CombatResult(
    bool Victory,
    int XpEarned,
    List<Item> LootFound,
    string Summary
);
```

---

## Configuration

### Stamina Costs (AttackResolutionService)
```csharp
private static readonly Dictionary<AttackType, int> StaminaCosts = new()
{
    { AttackType.Light, 15 },
    { AttackType.Standard, 25 },
    { AttackType.Heavy, 40 }
};
```

### Damage Bonuses
```csharp
private static readonly Dictionary<AttackType, int> DamageBonuses = new()
{
    { AttackType.Light, 0 },
    { AttackType.Standard, 2 },
    { AttackType.Heavy, 4 }
};
```

### Hit Modifiers
```csharp
private static readonly Dictionary<AttackType, int> HitModifiers = new()
{
    { AttackType.Light, 1 },
    { AttackType.Standard, 0 },
    { AttackType.Heavy, -1 }
};
```

---

## Combat Formulas Reference

### Defense Score
```
Defense = 10 + FINESSE - (Stress ÷ 20)
```

### Success Threshold
```
Threshold = Defense ÷ 5 (rounded down)
```

### Damage Calculation
```
RawDamage = MIGHT + WeaponDie + AttackTypeBonus
ModifiedDamage = RawDamage × OutcomeModifier × VulnerabilityMultiplier
FinalDamage = Max(1, ModifiedDamage - (ArmorSoak + FortifiedBonus))
```

---

## Testing

### Test Files
- `CombatServiceTests.cs`
- `AttackResolutionServiceTests.cs`
- `InitiativeServiceTests.cs`

### Critical Test Scenarios
1. Combat initialization with valid/invalid state
2. Turn advancement with wraparound
3. Attack resolution at all outcome levels
4. Damage calculation with modifiers
5. Death handling and turn index adjustment
6. Victory/defeat condition detection
7. Ability usage with cooldowns
8. Status effect application and expiry
9. Stress defense penalty calculation

---

## Design Rationale

### Why Sequential Turn Order?
- Predictable for players
- Simplifies AI decision making
- Enables strategic planning (defending before enemy turn)

### Why Partial Name Matching?
- User-friendly targeting
- Reduces command complexity
- Works with "attack varg" instead of full "Ash-Vargr"

### Why Minimum 1 Damage on Hit?
- Prevents "tank and spank" strategies with pure soak builds
- Ensures combat progresses
- Rewards successful attacks

### Why 750ms Enemy Delay?
- Gives player time to read combat log
- Creates dramatic pacing
- Distinguishes AI turns from player turns visually
