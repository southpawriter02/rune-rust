---
id: SPEC-REST-001
title: Rest & Recovery System
version: 1.0.0
status: Implemented
last_updated: 2025-12-23
related_specs: [SPEC-DICE-001, SPEC-INV-001]
---

# SPEC-REST-001: Rest & Recovery System

> **Version:** 1.0.0
> **Status:** Implemented (v0.3.2b)
> **Services:** `RestService`, `AmbushService`
> **Location:** `RuneAndRust.Engine/Services/`

---

## Overview

The Rest & Recovery System manages character restoration outside of combat. It provides two rest types with different safety/reward tradeoffs, integrates with the inventory system for supply consumption, and includes an ambush mechanic for wilderness rest that ties into the combat system.

---

## Rest Types

### Sanctuary Rest
- **Location:** Rooms with `RoomFeature.RunicAnchor`
- **Safety:** 100% safe, no ambush risk
- **Recovery:** Full HP, Stamina, Stress recovery
- **Supplies:** Not required
- **Exhausted:** Cured automatically

### Wilderness Rest
- **Location:** Any non-sanctuary room
- **Safety:** Risk of ambush based on danger level
- **Recovery:** Partial HP, Stamina, Stress recovery
- **Supplies:** Consumes 1 Ration + 1 Water (or applies Exhausted)
- **Exhausted:** Applied if no supplies; halves recovery

---

## Behaviors

### Primary Behaviors

#### 1. Rest Execution (`PerformRestAsync`)

```csharp
Task<RestResult> PerformRestAsync(Character character, RestType type)
Task<RestResult> PerformRestAsync(Character character, RestType type, Room room)
```

**Sanctuary Path:**
```csharp
// Full recovery, no checks
character.CurrentHP = character.MaxHP;
character.CurrentStamina = character.MaxStamina;
character.PsychicStress = 0;
character.RemoveStatusEffect(StatusEffectType.Exhausted);
```

**Wilderness Path with Room:**
1. Calculate ambush risk via `AmbushService`
2. If ambushed → consume supplies, no recovery, return encounter
3. If safe → proceed with normal wilderness rest

#### 2. Sanctuary Rest (`PerformSanctuaryRestAsync`)

**Sequence:**
1. Record starting values (HP, Stamina, Stress)
2. Set all values to maximum
3. Clear Psychic Stress to 0
4. Remove Exhausted status if present
5. Return result with recovery amounts

**Result:**
```csharp
return new RestResult(
    HpRecovered: character.MaxHP - oldHp,
    StaminaRecovered: character.MaxStamina - oldStamina,
    StressRecovered: oldStress,  // Full stress cleared
    SuppliesConsumed: false,
    IsExhausted: false
);
```

#### 3. Wilderness Rest (`PerformWildernessRestAsync`)

**Sequence:**
1. Check for supplies (Ration + Water tags)
2. If supplies available → consume 1 of each
3. If no supplies → apply Exhausted status
4. Record starting values
5. Calculate HP recovery: `10 + (Sturdiness × 2)`, halved if Exhausted
6. Apply HP recovery (capped at MaxHP)
7. Recover Stamina to max (halved if Exhausted)
8. Recover Stress: `Will × 5` (0 if Exhausted)
9. Return result

**Recovery Formulas:**
| Stat | Formula | If Exhausted |
|------|---------|--------------|
| HP | `10 + (Sturdiness × 2)` | ÷2 |
| Stamina | Full recovery to MaxStamina | MaxStamina ÷ 2 |
| Stress | `Will × 5` | 0 (no recovery) |

#### 4. Ambushed Rest (`PerformAmbushedRestAsync`)

**Sequence:**
1. Log ambush warning
2. Check supplies (consumed even on ambush - wasted)
3. If supplies present → consume them
4. Apply Disoriented status (caught off-guard)
5. Return result with 0 recovery + encounter definition

**Result:**
```csharp
return new RestResult(
    HpRecovered: 0,
    StaminaRecovered: 0,
    StressRecovered: 0,
    SuppliesConsumed: hadSupplies,
    IsExhausted: false,
    WasAmbushed: true,
    AmbushDetails: ambushResult
);
```

#### 5. Supply Checking (`HasRequiredSuppliesAsync`)

```csharp
Task<bool> HasRequiredSuppliesAsync(Character character)
```

**Mechanics:**
- Searches inventory for items with `Ration` tag
- Searches inventory for items with `Water` tag
- Returns true only if both found

---

## AmbushService Behaviors

### 1. Ambush Calculation (`CalculateAmbushAsync`)

```csharp
Task<AmbushResult> CalculateAmbushAsync(Character character, Room room)
```

**Sequence:**
1. Get base risk from danger level
2. If Safe zone → return no ambush (0% risk)
3. Roll WITS dice pool for mitigation
4. Calculate final risk with floor
5. Roll d100 against final risk
6. If ambush triggered → generate encounter
7. Return comprehensive result

**Base Risk Table:**
| Danger Level | Base Risk |
|--------------|-----------|
| Safe | 0% |
| Unstable | 15% |
| Hostile | 30% |
| Lethal | 50% |

**Mitigation Formula:**
```csharp
var mitigation = witsRoll.Successes * 5; // 5% per success
var rawFinalRisk = Math.Max(0, baseRisk - mitigation);
var finalRisk = dangerLevel > Safe
    ? Math.Max(5, rawFinalRisk)  // 5% floor in danger zones
    : rawFinalRisk;
```

### 2. Encounter Generation (`GenerateAmbushEncounter`)

```csharp
EncounterDefinition GenerateAmbushEncounter(Room room, int partyLevel = 1)
```

**Budget Calculation:**
```csharp
var scaledBudget = 100f * (1f + (partyLevel - 1) * 0.1f);
var ambushBudget = scaledBudget * 0.8f; // 80% of standard
```

**Template Selection:**
Prioritizes fast/stealthy enemies:
1. `bst_vargr_01` (Ash-Vargr) - Cost: 40
2. `mec_serv_01` (Utility Servitor) - Cost: 25
3. Fill remaining budget with Servitors

---

## Exhausted Status

### Application Conditions
- Wilderness rest without Ration or Water

### Effects on Wilderness Rest
| Stat | Normal | Exhausted |
|------|--------|-----------|
| HP Recovery | `10 + (STU × 2)` | `(10 + (STU × 2)) ÷ 2` |
| Stamina | 100% | 50% |
| Stress | `WILL × 5` | 0 |

### Removal
- Sanctuary rest only
- Not cleared by wilderness rest (even with supplies)

---

## Restrictions

### Rest Availability
1. **Not in combat** - Rest cannot be initiated during combat
2. **Sanctuary requires feature** - Only rooms with RunicAnchor
3. **Supplies for wilderness** - Or accept Exhausted penalty

### Supply Consumption
1. **Always consumed** - Even on ambush (wasted)
2. **Exactly 1 of each** - No partial consumption
3. **Tag-based lookup** - Not item name

### Ambush Mechanics
1. **Only wilderness rest** - Sanctuary is always safe
2. **Minimum 5% risk** - In any danger zone
3. **Cannot prevent** - Only mitigate with WITS

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Max base risk | 50% | Lethal zones |
| Min risk floor | 5% | In danger zones |
| Mitigation per success | 5% | WITS-based |
| Ambush budget modifier | 0.8× | 80% of standard |
| HP recovery base | 10 | Before STURDINESS |
| HP per STURDINESS | 2 | Scaling factor |
| Stress per WILL | 5 | Recovery scaling |

### System Gaps
- No camp quality mechanic
- No watch rotation / partial party rest
- No consumable quality affecting recovery
- No time passage tracking

---

## Use Cases

### UC-1: Safe Sanctuary Rest
```csharp
var room = await roomRepo.GetByIdAsync(roomId);
if (room.Features.Contains(RoomFeature.RunicAnchor))
{
    var result = await restService.PerformRestAsync(character, RestType.Sanctuary);
    // Full recovery guaranteed
}
```

### UC-2: Risky Wilderness Rest
```csharp
var room = await roomRepo.GetByIdAsync(roomId);
var result = await restService.PerformRestAsync(character, RestType.Wilderness, room);

if (result.WasAmbushed)
{
    // Transition to combat with result.AmbushDetails.Encounter
    var enemies = await enemyFactory.CreateFromTemplates(
        result.AmbushDetails.Encounter.TemplateIds);
    combatService.StartCombat(enemies);
}
else
{
    // Show recovery results
    DisplayRecovery(result);
}
```

### UC-3: Rest Without Supplies
```csharp
if (!await restService.HasRequiredSuppliesAsync(character))
{
    // Warn player about Exhausted penalty
    var proceed = await PromptUser("Rest without supplies? You will become Exhausted.");
    if (!proceed) return;
}

var result = await restService.PerformRestAsync(character, RestType.Wilderness, room);
// result.IsExhausted = true if no supplies
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `IInventoryService` | [SPEC-INV-001](SPEC-INV-001.md) | Supply lookup and consumption |
| `IAmbushService` | [SPEC-REST-001](SPEC-REST-001.md) | Risk calculation, encounter generation |
| `ILogger` | Infrastructure | Rest traceability |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `GameService` | [SPEC-GAME-001](SPEC-GAME-001.md) | Rest command processing |
| `RestScreenRenderer` | UI Layer | UI display |

---

## Related Services

### Primary Implementation
| File | Purpose |
|------|---------|
| `RestService.cs` | Core rest mechanics |
| `AmbushService.cs` | Risk and encounter generation |

### Supporting Types
| File | Purpose |
|------|---------|
| `RestResult.cs` | Rest operation result |
| `AmbushResult.cs` | Ambush calculation result |
| `EncounterDefinition.cs` | Enemy spawn template |
| `RestType.cs` | Rest type enum |

---

## Data Models

### RestResult
```csharp
public record RestResult(
    int HpRecovered,
    int StaminaRecovered,
    int StressRecovered,
    bool SuppliesConsumed,
    bool IsExhausted,
    bool WasAmbushed = false,
    AmbushResult? AmbushDetails = null
);
```

### AmbushResult
```csharp
public record AmbushResult(
    bool IsAmbush,
    string Message,
    int BaseRiskPercent,
    int MitigationPercent,
    int FinalRiskPercent,
    int RollValue,
    int MitigationSuccesses,
    EncounterDefinition? Encounter = null
);
```

### EncounterDefinition
```csharp
public record EncounterDefinition(
    List<string> TemplateIds,
    float Budget,
    bool IsAmbush,
    string EncounterType
);
```

---

## Configuration

### RestService Constants
```csharp
private const string RationTag = "Ration";
private const string WaterTag = "Water";
private const int BaseHpRecovery = 10;
private const int HpRecoveryPerSturdiness = 2;
private const int StressReductionPerWill = 5;
```

### AmbushService Constants
```csharp
private const int MitigationPerSuccess = 5;      // 5% per WITS success
private const int MinimumRiskFloor = 5;          // 5% minimum in danger zones
private const float AmbushBudgetMultiplier = 0.8f;
private const float BaseEncounterBudget = 100f;
```

### Template Costs
```csharp
private static readonly Dictionary<string, float> TemplateCosts = new()
{
    ["bst_vargr_01"] = 40f,   // Ash-Vargr
    ["mec_serv_01"] = 25f,    // Utility Servitor
    ["hum_raider_01"] = 35f   // Rust-Clan Scav
};
```

---

## Testing

### Test Files
- `RestServiceTests.cs`
- `AmbushServiceTests.cs`

### Critical Test Scenarios
1. Sanctuary rest full recovery
2. Wilderness rest with supplies
3. Wilderness rest without supplies (Exhausted)
4. Exhausted penalty calculation
5. Ambush risk at all danger levels
6. WITS mitigation effectiveness
7. Minimum risk floor (5%)
8. Ambush encounter generation
9. Supply tag-based lookup
10. Supply consumption on ambush

---

## Design Rationale

### Why Two Rest Types?
- Creates meaningful exploration decisions
- Safe havens are rewards for exploration
- Wilderness rest encourages resource management

### Why Tag-Based Supply Lookup?
- Allows multiple item types (e.g., "Dried Rations", "Fresh Rations")
- Easier to extend without code changes
- Cleaner than exact name matching

### Why Exhausted Penalty?
- Punishment for poor preparation
- Not immediately lethal (can still rest)
- Incentivizes supply gathering

### Why WITS for Mitigation?
- Perception/awareness theme
- Creates attribute value for non-combat characters
- Consistent with "camp craft" flavor

### Why 5% Minimum Risk?
- Danger zones are never truly safe
- Prevents trivial risk elimination
- Maintains tension even with high WITS

### Why Reduced Ambush Budget?
- Ambush enemies caught player off-guard (narrative)
- Prevents unfair difficulty spike
- Still meaningful combat challenge
