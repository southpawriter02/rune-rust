---
id: SPEC-TRAUMA-001
title: Trauma & Stress System
version: 1.1.0
status: Implemented
last_updated: 2025-12-23
related_specs: [SPEC-DICE-001, SPEC-COMBAT-001]
---

# SPEC-TRAUMA-001: Trauma & Stress System

> **Version:** 1.1.0
> **Status:** Implemented (v0.3.0c)
> **Service:** `TraumaService`, `TraumaRegistry`
> **Location:** `RuneAndRust.Engine/Services/TraumaService.cs`

---

## Overview

The Trauma & Stress System implements psychological damage mechanics that affect combat performance and can result in permanent character changes. Stress is a recoverable resource that accumulates during gameplay and can trigger Breaking Points, while Traumas are permanent psychological injuries acquired at Breaking Points.

This system represents the mental toll of surviving in a hostile, post-apocalyptic environment.

---

## Core Concepts

### Psychic Stress
- **Range:** 0-100
- **Nature:** Recoverable via rest, abilities, and sanctuary
- **Impact:** Reduces Defense Score, triggers Breaking Point at 100
- **Sources:** Combat damage, trauma triggers, environmental horror, ambient conditions

### Traumas
- **Nature:** Permanent psychological injuries
- **Acquisition:** Breaking Point failures
- **Effects:** Attribute penalties, conditional stress gain, behavioral restrictions
- **Types:** Phobia, Compulsion, Delusion, Somatic

---

## Behaviors

### Primary Behaviors

#### 1. Stress Infliction (`InflictStress`)

```csharp
StressResult InflictStress(Combatant target, int amount, string source)
```

**Resolution Sequence:**
1. Record previous stress and status
2. Roll WILL dice pool for resolve check
3. Calculate mitigation (1 stress per success)
4. Apply net stress (raw - mitigation), clamped to 0-100
5. Determine new stress status
6. Check for Breaking Point trigger (crossing 100)
7. Handle Breaking Point if triggered
8. Return comprehensive result

**Mitigation Formula:**
```csharp
var resolveRoll = _dice.Roll(target.Will, "Resolve Check");
var mitigation = resolveRoll.Successes;
var netStress = Math.Max(0, rawStress - mitigation);
```

**Example:**
```csharp
// Player with WILL 4 takes 10 stress
var result = traumaService.InflictStress(player, 10, "Grotesque Sight");
// Rolls 4d10: [3, 8, 9, 2] = 2 successes
// Mitigation: 2
// Net stress: 10 - 2 = 8
// If player was at 95 stress, now at 100 → Breaking Point!
```

#### 2. Stress Recovery (`RecoverStress`)

```csharp
StressResult RecoverStress(Combatant target, int amount, string source)
```

**Mechanics:**
- Direct reduction, no roll required
- Cannot go below 0
- Used by rest system and healing abilities

**Example:**
```csharp
// Sanctuary rest fully clears stress
var result = traumaService.RecoverStress(player, 100, "Sanctuary Rest");
```

#### 3. Breaking Point Resolution (`ResolveBreakingPoint`)

```csharp
BreakingPointResult ResolveBreakingPoint(Character character, string source)
```

**Resolution Sequence:**
1. Roll WILL dice pool
2. Determine outcome based on successes and botches
3. Apply appropriate consequences
4. Return result with trauma if acquired

**Outcome Table:**
| Successes | Botches | Outcome | Consequences |
|-----------|---------|---------|--------------|
| 3+ | Any | **Stabilized** | Stress → 75, Disoriented status |
| 0 | 1+ | **Catastrophe** | Stress → 50, Trauma + Stunned |
| 1-2 | Any | **Trauma** | Stress → 50, Trauma acquired |

**Trauma Acquisition:**
```csharp
var traumaDefinition = TraumaRegistry.GetRandom();
var trauma = traumaDefinition.CreateInstance(source);
character.ActiveTraumas.Add(trauma);
ApplyTraumaPenalties(character, trauma);
```

#### 4. Defense Penalty Calculation (`GetDefensePenalty`)

```csharp
int GetDefensePenalty(int stressValue)
```

**Formula:**
```csharp
return Math.Min(5, stressValue / 20);
```

| Stress Range | Penalty |
|--------------|---------|
| 0-19 | 0 |
| 20-39 | -1 |
| 40-59 | -2 |
| 60-79 | -3 |
| 80-99 | -4 |
| 100 | -5 |

#### 5. Stress Status Classification (`GetStressStatus`)

```csharp
StressStatus GetStressStatus(int stressValue)
```

| Status | Stress Range | Description |
|--------|--------------|-------------|
| Stable | 0-19 | Calm and focused |
| Unsettled | 20-39 | Slightly disturbed |
| Shaken | 40-59 | Noticeably affected |
| Distressed | 60-79 | Struggling to cope |
| Fractured | 80-99 | Near breaking |
| Breaking | 100 | Mental crisis |

#### 6. Trauma Penalty Application (`ApplyTraumaPenalties`)

```csharp
void ApplyTraumaPenalties(Character character, Trauma trauma)
```

**Mechanics:**
- Looks up trauma definition from registry
- Applies permanent attribute penalties (minimum 1)
- Penalties are cumulative with multiple traumas

**Example:**
```csharp
// "Paranoia" trauma applies -1 WITS, -1 WILL
var character = { Wits: 5, Will: 4 };
ApplyTraumaPenalties(character, paranoia);
// Result: Wits: 4, Will: 3
```

### Edge Case Behaviors

#### Non-Player Combatant Breaking Point
Enemies don't have full trauma mechanics:
```csharp
if (target.CharacterSource == null)
{
    target.CurrentStress = 75; // Simple reset
    return;
}
```

#### Minimum Attribute Floor
Trauma penalties cannot reduce attributes below 1:
```csharp
var newValue = Math.Max(1, currentValue + penalty);
```

---

## Restrictions

### Stress Mechanics
1. **Cannot exceed 100** - Clamped at maximum
2. **Cannot go negative** - Clamped at minimum
3. **Always requires source string** - For logging/narrative
4. **Mitigation only on infliction** - Recovery is direct

### Trauma Mechanics
1. **Only players acquire traumas** - Enemies are exempt
2. **Traumas are permanent** - No removal mechanism currently
3. **One trauma per Breaking Point** - Single acquisition per trigger

### Breaking Point Triggers
1. **Only triggers on crossing 100** - Not on staying at 100
2. **Requires player character** - Enemies skip full resolution

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Stress range | 0-100 | Hard limits |
| Max defense penalty | 5 | At 100 stress |
| Trauma attribute penalty minimum | 1 | Attributes can't go below 1 |
| Trauma registry size | ~10 | Current implementation |

### Trauma System Gaps
- No trauma removal/healing
- No trauma-specific gameplay triggers (phobia detection)
- No trauma escalation/stacking effects

---

## Use Cases

### UC-1: Combat Stress Application
```csharp
// Enemy attack inflicts 5 stress as collateral psychological damage
var result = traumaService.InflictStress(player, 5, "Vargr Savage Attack");

if (result.IsBreakingPoint)
{
    // Handle Breaking Point UI/narrative
}
else if (result.NewStatus != result.PreviousStatus)
{
    // Show status transition message
}
```

### UC-2: Trauma Trigger in Combat
```csharp
// During turn processing for player with active traumas
foreach (var trauma in character.ActiveTraumas.Where(t => t.IsActive))
{
    var definition = TraumaRegistry.GetById(trauma.DefinitionId);
    if (definition.TriggerCondition.Contains("Always active"))
    {
        traumaService.InflictStress(combatant,
            definition.StressPerTurnInCondition,
            $"Trauma: {trauma.Name}");
    }
}
```

### UC-3: Sanctuary Recovery
```csharp
// Full stress recovery at sanctuary
traumaService.RecoverStress(combatant, combatant.CurrentStress, "Sanctuary Rest");
// Stress → 0
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `IDiceService` | [SPEC-DICE-001](SPEC-DICE-001.md) | WILL resolve checks |
| `TraumaRegistry` | [SPEC-TRAUMA-001](SPEC-TRAUMA-001.md) | Trauma definitions |
| `ILogger` | Infrastructure | Event traceability |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Defense penalty, trauma triggers |
| `AttackResolutionService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Defense score calculation |
| `RestService` | [SPEC-REST-001](SPEC-REST-001.md) | Stress recovery |
| `ConditionService` | [SPEC-COND-001](SPEC-COND-001.md) | Stress tick effects |

---

## Related Services

### Primary Implementation
| File | Purpose |
|------|---------|
| `TraumaService.cs` | Core stress/trauma mechanics |
| `TraumaRegistry.cs` | Static trauma definitions |

### Supporting Types
| File | Purpose |
|------|---------|
| `Trauma.cs` | Trauma entity |
| `StressResult.cs` | Stress operation result |
| `BreakingPointResult.cs` | Breaking Point outcome |
| `StressStatus.cs` | Status enum |
| `TraumaType.cs` | Trauma classification enum |

---

## Data Models

### StressResult
```csharp
public record StressResult(
    int RawStress,            // Original stress amount
    int MitigatedAmount,      // Successes from resolve check
    int NetStressApplied,     // Actual stress change
    int CurrentTotal,         // Final stress value
    StressStatus PreviousStatus,
    StressStatus NewStatus,
    bool IsBreakingPoint,     // Triggered Breaking Point?
    int ResolveSuccesses,     // Dice pool successes
    string Source             // What caused the stress
);
```

### BreakingPointResult
```csharp
public record BreakingPointResult(
    BreakingPointOutcome Outcome,  // Stabilized/Trauma/Catastrophe
    Trauma? AcquiredTrauma,        // Null if Stabilized
    int NewStressLevel,            // 50 or 75 depending on outcome
    int ResolveSuccesses,
    int ResolveBotches,
    bool WasStunned,               // Catastrophe only
    bool WasDisoriented            // Stabilized only
);
```

### Trauma Entity
```csharp
public class Trauma
{
    public Guid Id { get; set; }
    public string DefinitionId { get; set; }  // Links to TraumaRegistry
    public TraumaType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Source { get; set; }        // What caused acquisition
    public DateTime AcquiredAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### TraumaDefinition (Registry Entry)
```csharp
public record TraumaDefinition(
    string Id,
    string Name,
    string Description,
    TraumaType Type,
    Dictionary<Attribute, int> AttributePenalties,  // Permanent stat penalties
    string TriggerCondition,                        // When trauma activates
    int StressPerTurnInCondition                    // Stress while triggered
);
```

---

## TraumaRegistry Definitions

### Current Trauma Pool (10 Definitions)
| Trauma | Type | Penalties | Trigger | Stress/Turn |
|--------|------|-----------|---------|-------------|
| Nyctophobia | Phobia | (none) | In dark or dimly lit areas | 2 |
| Claustrophobia | Phobia | (none) | In small rooms or tunnels | 2 |
| Hemophobia | Phobia | (none) | When any combatant takes damage | 1 |
| The Shakes | Somatic | FINESSE -1 | Always active | 0 |
| Chronic Migraines | Somatic | WITS -1 | Always active | 0 |
| Bone-Deep Exhaustion | Somatic | STURDINESS -1 | Always active | 0 |
| Paranoia | Delusion | (none) | When receiving beneficial effects from allies | 1 |
| The Whispers | Delusion | WILL -1 | Always active | 0 |
| Self-Destructive Urges | Compulsion | (none) | When stress reaches Fractured tier | 0 |
| Compulsive Hoarding | Compulsion | (none) | When discarding items | 1 |

---

## Configuration

### Resolve Check Threshold
```csharp
// Breaking Point stabilization requires 3+ successes
private const int StabilizationThreshold = 3;
```

### Stress Reset Values
```csharp
private const int StabilizedStressReset = 75;
private const int TraumaStressReset = 50;
private const int CatastropheStressReset = 50;
```

### Defense Penalty Formula
```csharp
private const int StressPerPenaltyPoint = 20;
private const int MaxDefensePenalty = 5;
```

---

## Testing

### Test Files
- `TraumaServiceTests.cs`
- `TraumaRegistryTests.cs`

### Critical Test Scenarios
1. Stress infliction with mitigation
2. Stress clamping at 0 and 100
3. Breaking Point trigger at exactly 100
4. All three Breaking Point outcomes
5. Trauma penalty application
6. Minimum attribute floor (≥1)
7. Defense penalty calculation at all thresholds
8. Status classification accuracy

---

## Design Rationale

### Why WILL-Based Mitigation?
- WILL represents mental fortitude
- Creates meaningful attribute choice
- High-WILL characters are more stress resistant

### Why Permanent Traumas?
- Raises stakes for high-stress play
- Creates character history and narrative
- Encourages careful stress management

### Why Defense Penalty from Stress?
- Mental state affects physical performance
- Creates death spiral risk (stressed → hit more → more stressed)
- Encourages stress recovery between fights

### Why Three Breaking Point Outcomes?
- Reward high-WILL characters (Stabilization)
- Punish fumbles (Catastrophe with stun)
- Standard failure path (Trauma only)

---

## Changelog

### v1.1.0 (2025-12-23)
- Added `last_updated` field to YAML frontmatter
- Updated TraumaRegistry table to match actual implementation (10 traumas)
- Fixed trauma names, penalties, and trigger conditions to reflect code

### v1.0.0 (Initial)
- Core stress mechanics (infliction, recovery, mitigation)
- Breaking Point resolution (3 outcomes)
- Defense penalty calculation
- TraumaRegistry with initial definitions
