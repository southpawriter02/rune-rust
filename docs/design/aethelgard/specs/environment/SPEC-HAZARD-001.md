---
id: SPEC-HAZARD-001
title: Dynamic Hazard System
version: 1.0.1
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-DICE-001, SPEC-COMBAT-001, SPEC-NAV-001]
---

# SPEC-HAZARD-001: Dynamic Hazard System

> **Version:** 1.0.1
> **Status:** Implemented (v0.3.3a)
> **Service:** `HazardService`, `EffectScriptExecutor`
> **Location:** `RuneAndRust.Engine/Services/`

---

## Overview

The Dynamic Hazard System implements interactive environmental dangers that can trigger during exploration and combat. Hazards have states (Dormant, Triggered, Cooldown, Destroyed), multiple trigger types, and execute effect scripts that can deal damage and apply status effects.

This system creates environmental gameplay beyond simple enemy encounters.

---

## Core Concepts

### Hazard Types
| Type | Description | Examples |
|------|-------------|----------|
| **Mechanical** | Traps and devices | Spike traps, pressure plates |
| **Environmental** | Natural/elemental hazards | Steam vents, acid pools |
| **Biological** | Living/organic hazards | Spore clusters, poison plants |

### Hazard States
| State | Description | Can Trigger? |
|-------|-------------|--------------|
| **Dormant** | Ready to activate | Yes |
| **Triggered** | Just activated (transient) | No |
| **Cooldown** | Recharging | No |
| **Destroyed** | Permanently disabled | No |

### Trigger Types
| Trigger | When Activated |
|---------|----------------|
| **Movement** | On room entry |
| **DamageTaken** | When damage occurs in room |
| **TurnStart** | At start of combat rounds |
| **ManualInteraction** | Player explicitly activates |

---

## Behaviors

### Primary Behaviors

#### 1. Room Entry Triggers (`TriggerOnRoomEnterAsync`)

```csharp
Task<List<HazardResult>> TriggerOnRoomEnterAsync(Room room, Combatant? entrant = null)
```

**Sequence:**
1. Get active hazards in room
2. Filter for `TriggerType.Movement`
3. Filter for `HazardState.Dormant`
4. Activate each matching hazard
5. Return list of results

**Example:**
```csharp
// Player enters room with Pressure Plate trap
var results = await hazardService.TriggerOnRoomEnterAsync(room, player);
// "The Pressure Plate activates with a grinding sound! 8 damage."
```

#### 2. Damage-Triggered Hazards (`TriggerOnDamageAsync`)

```csharp
Task<List<HazardResult>> TriggerOnDamageAsync(
    Room room,
    DamageType damageType,
    int amount,
    Combatant? target = null)
```

**Filter Checks:**
1. State must be `Dormant`
2. Trigger must be `DamageTaken`
3. If `RequiredDamageType` set → must match
4. If `DamageThreshold` set → amount must meet or exceed

**Example:**
```csharp
// Fire damage in room with Volatile Gas hazard
var results = await hazardService.TriggerOnDamageAsync(
    room, DamageType.Fire, 10, target);
// "The Volatile Gas erupts violently! 15 damage."
```

#### 3. Turn-Start Hazards (`ProcessTurnStartHazardsAsync`)

```csharp
Task<List<HazardResult>> ProcessTurnStartHazardsAsync(
    Room room,
    List<Combatant> combatants)
```

**Sequence:**
1. Get active hazards with `TriggerType.TurnStart`
2. For each Dormant hazard → activate on ALL combatants
3. Returns list of all results

**Use Case:** Poison gas that damages everyone each round.

#### 4. Cooldown Processing (`TickCooldownsAsync`)

```csharp
Task TickCooldownsAsync(Room room)
```

**Sequence:**
1. Get all hazards in Cooldown state
2. Decrement `CooldownRemaining` by 1
3. If cooldown reaches 0 → transition to `Dormant`
4. Persist changes

**Called:** At end of combat rounds.

#### 5. Manual Activation (`ManualActivateAsync`)

```csharp
Task<HazardResult> ManualActivateAsync(DynamicHazard hazard, Combatant activator)
```

**Restrictions:**
- Only works for `TriggerType.ManualInteraction` hazards
- Must be in `Dormant` state

**Use Case:** Player deliberately triggers trap on enemies.

#### 6. Active Hazard Query (`GetActiveHazardsAsync`)

```csharp
Task<List<DynamicHazard>> GetActiveHazardsAsync(Guid roomId)
```

**Returns:** All hazards not in `Destroyed` state.

### Hazard Activation (`ActivateHazardAsync`)

**Private method handling all activations:**

1. Set state to `Triggered`
2. Build trigger message (custom or default by type)
3. If target provided and EffectScript set:
   - Execute script via `EffectScriptExecutor`
   - Collect damage, healing, status results
4. Update state based on `OneTimeUse`:
   - If true → state = `Destroyed`
   - If false → state = `Cooldown`, set `CooldownRemaining`
5. Persist changes
6. Return `HazardResult`

---

## Effect Script Language

### Supported Commands
Effect scripts are semicolon-delimited command sequences.

| Command | Format | Example |
|---------|--------|---------|
| **DAMAGE** | `DAMAGE:{type}:{amount}` | `DAMAGE:FIRE:2d6` |
| **HEAL** | `HEAL:{amount}` | `HEAL:10` |
| **STATUS** | `STATUS:{effect}:{duration}` | `STATUS:BURNING:3` |

### Damage Dice Notation
- `Nd[X]` - Roll N dice with X sides
- `N` - Flat damage value
- Examples: `2d6`, `1d10`, `15`

### Multi-Effect Scripts
```
DAMAGE:FIRE:2d6;STATUS:BURNING:2
// Deals fire damage AND applies 2-turn burning
```

---

## Restrictions

### Trigger Restrictions
1. **Only Dormant hazards trigger** - Other states blocked
2. **Damage type must match** - If filter set
3. **Damage threshold must be met** - If set

### State Transitions
1. **Dormant → Triggered** - On any activation
2. **Triggered → Cooldown** - If not `OneTimeUse`
3. **Triggered → Destroyed** - If `OneTimeUse`
4. **Cooldown → Dormant** - When cooldown reaches 0
5. **Destroyed is final** - No recovery

### Manual Activation
1. **Only ManualInteraction type** - Other types reject
2. **Player-initiated only** - No AI manual activation

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Max cooldown | Unbounded | Typically 1-5 turns |
| Damage amount | Unbounded | Based on effect script |
| Hazards per room | Unbounded | Practical limit ~5 |

### System Gaps
- No hazard destruction by player (killing the trap)
- No hazard detection/disarm skill checks
- No hazard visibility states (hidden vs revealed)
- No area-of-effect patterns

---

## Use Cases

### UC-1: Spike Trap on Entry
```csharp
// Hazard Definition
var spikeTrap = new DynamicHazard
{
    Name = "Rusted Spike Plate",
    HazardType = HazardType.Mechanical,
    Trigger = TriggerType.Movement,
    EffectScript = "DAMAGE:PHYSICAL:2d6",
    OneTimeUse = false,
    MaxCooldown = 2,
    TriggerMessage = "Rusted spikes burst from the floor!"
};

// On player entering room
var results = await hazardService.TriggerOnRoomEnterAsync(room, player);
// Player takes 2d6 physical damage, trap enters 2-turn cooldown
```

### UC-2: Volatile Gas Chain Reaction
```csharp
// Hazard responds to fire damage
var volatileGas = new DynamicHazard
{
    Name = "Volatile Gas Pocket",
    HazardType = HazardType.Environmental,
    Trigger = TriggerType.DamageTaken,
    RequiredDamageType = DamageType.Fire,
    DamageThreshold = 5,
    EffectScript = "DAMAGE:FIRE:4d6",
    OneTimeUse = true,  // Explodes once
    TriggerMessage = "The gas pocket EXPLODES!"
};

// Combat: Enemy uses fire attack for 10 damage
var results = await hazardService.TriggerOnDamageAsync(
    room, DamageType.Fire, 10, target);
// Gas explodes, dealing 4d6 fire to target
// Hazard is now Destroyed
```

### UC-3: Poison Fog Per-Turn
```csharp
// Hazard ticks each combat round
var poisonFog = new DynamicHazard
{
    Name = "Toxic Spore Cloud",
    HazardType = HazardType.Biological,
    Trigger = TriggerType.TurnStart,
    EffectScript = "DAMAGE:POISON:1d4;STATUS:POISONED:2",
    OneTimeUse = false,
    MaxCooldown = 0  // No cooldown, triggers every round
};

// At start of each combat round
var results = await hazardService.ProcessTurnStartHazardsAsync(room, combatants);
// Everyone in room takes poison damage and may become Poisoned
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `IInteractableObjectRepository` | Infrastructure | Hazard persistence |
| `EffectScriptExecutor` | [SPEC-ABILITY-001](SPEC-ABILITY-001.md) | Script execution |
| `ILogger` | Infrastructure | Event tracing |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Damage-triggered hazards |
| `NavigationService` | [SPEC-NAV-001](SPEC-NAV-001.md) | Room entry triggers |
| `EnvironmentPopulator` | [SPEC-ENVPOP-001](SPEC-ENVPOP-001.md) | Procedural hazard placement |

---

## Related Services

### Primary Implementation
| File | Purpose |
|------|---------|
| `HazardService.cs` | Hazard lifecycle management |
| `EffectScriptExecutor.cs` | Shared script execution |
| `EnvironmentPopulator.cs` | Procedural placement |

### Supporting Types
| File | Purpose |
|------|---------|
| `DynamicHazard.cs` | Hazard entity |
| `HazardResult.cs` | Activation result |
| `HazardType.cs` | Type enum |
| `HazardState.cs` | State enum |
| `TriggerType.cs` | Trigger enum |

---

## Data Models

### DynamicHazard Entity
```csharp
public class DynamicHazard : InteractableObject
{
    public HazardType HazardType { get; set; }
    public HazardState State { get; set; } = HazardState.Dormant;
    public TriggerType Trigger { get; set; }

    // Effect Configuration
    public string? EffectScript { get; set; }
    public string? TriggerMessage { get; set; }

    // Cooldown Management
    public int MaxCooldown { get; set; } = 1;
    public int CooldownRemaining { get; set; } = 0;
    public bool OneTimeUse { get; set; } = false;

    // Damage Trigger Filters
    public DamageType? RequiredDamageType { get; set; }
    public int DamageThreshold { get; set; } = 0;
}
```

### HazardResult
```csharp
public record HazardResult(
    bool WasTriggered,
    string HazardName,
    string Message,
    int TotalDamage,
    int TotalHealing,
    List<string> StatusesApplied,
    HazardState NewState
)
{
    public static HazardResult None => new(false, "", "", 0, 0, new(), HazardState.Dormant);
}
```

### Enums
```csharp
public enum HazardType
{
    Mechanical,
    Environmental,
    Biological
}

public enum HazardState
{
    Dormant,
    Triggered,
    Cooldown,
    Destroyed
}

public enum TriggerType
{
    Movement,
    DamageTaken,
    TurnStart,
    ManualInteraction
}
```

---

## EffectScriptExecutor Behavior

### Execute Method
```csharp
ScriptResult Execute(
    string effectScript,
    Combatant target,
    string sourceName,
    Guid? sourceId = null)
```

### ScriptResult
```csharp
public record ScriptResult(
    int TotalDamage,
    int TotalHealing,
    List<string> StatusesApplied,
    string Narrative
);
```

### Command Parsing
```csharp
var commands = effectScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
foreach (var command in commands)
{
    var parts = command.Trim().Split(':');
    switch (parts[0].ToUpperInvariant())
    {
        case "DAMAGE": ProcessDamage(parts, target); break;
        case "HEAL": ProcessHeal(parts, target); break;
        case "STATUS": ProcessStatus(parts, target); break;
    }
}
```

---

## Procedural Population

### EnvironmentPopulator Integration
```csharp
// Based on BiomeEnvironmentMapping
var hazardTemplate = biomeMapping.GetHazardForBiome(room.BiomeType, room.DangerLevel);
if (hazardTemplate != null)
{
    var hazard = CreateFromTemplate(hazardTemplate, room);
    room.Hazards.Add(hazard);
}
```

### Biome-Hazard Mapping
| Biome | Common Hazards |
|-------|----------------|
| Industrial | Steam Vents, Mechanical Traps |
| Aetheric | Energy Discharges, Corruption Zones |
| Organic | Spore Clouds, Toxic Plants |
| Ruined | Unstable Floors, Debris Falls |

---

## Testing

### Test Files
- `HazardServiceTests.cs`
- `EffectScriptExecutorTests.cs`

### Critical Test Scenarios
1. Movement trigger on room entry
2. Damage trigger with type filter
3. Damage trigger with threshold
4. Turn-start affects all combatants
5. Cooldown decrement and reset
6. OneTimeUse destruction
7. State transition validity
8. Manual activation restrictions
9. Effect script parsing (all commands)
10. Multi-effect script execution

---

## Design Rationale

### Why State Machine?
- Clear lifecycle for hazards
- Prevents double-activation bugs
- Supports cooldown and destruction mechanics

### Why Effect Scripts?
- Data-driven hazard effects
- Shared executor with abilities
- Easy to create new hazards without code

### Why Damage Type Filters?
- Creates tactical opportunities (use fire near gas)
- Emergent gameplay from environment
- Encourages player experimentation

### Why Cooldown System?
- Prevents infinite damage loops
- Creates timing-based tactics
- Balances powerful hazards

### Why OneTimeUse Flag?
- Some hazards should permanently break
- Creates sense of world change
- Prevents infinite exploitation

---

## Changelog

### v1.0.1 (2025-12-24)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- **FIX:** Removed `HazardType.Catastrophic` from spec (never implemented in code)
- Updated HazardType enum to match implementation: `Mechanical`, `Environmental`, `Biological`
- Added code traceability remarks to implementation files:
  - `IHazardService.cs` - interface spec reference
  - `HazardService.cs` - service spec reference
