# CoherentGlitchRuleEngine Service

**File Path:** `RuneAndRust.Engine/CoherentGlitch/CoherentGlitchRuleEngine.cs`
**Version:** v0.12
**Last Updated:** 2025-11-27
**Status:** ✅ Implemented

---

## Overview

The `CoherentGlitchRuleEngine` manages environmental storytelling through context-aware rules that modify room population. It ensures procedurally generated content follows logical patterns—water conducts electricity, boss arenas have enhanced hazards, secret rooms contain better rewards.

**Design Philosophy:**
> "The engine's generation is not purely random; it is logically chaotic."

The system creates emergent gameplay moments where environmental interactions feel deliberate rather than arbitrary.

---

## Architecture

### Rule Execution Model

```
                    ┌─────────────────────────┐
                    │ CoherentGlitchRuleEngine│
                    └───────────┬─────────────┘
                                │
            ┌───────────────────┼───────────────────┐
            │                   │                   │
            ▼                   ▼                   ▼
    ┌───────────────┐   ┌───────────────┐   ┌───────────────┐
    │   Critical    │   │     High      │   │    Medium     │
    │   Priority    │   │   Priority    │   │   Priority    │
    │  (Structure)  │   │  (Safety)     │   │  (Thematic)   │
    └───────────────┘   └───────────────┘   └───────────────┘
            │                   │                   │
            ▼                   ▼                   ▼
    ┌─────────────────────────────────────────────────────┐
    │              Room Population Modified               │
    └─────────────────────────────────────────────────────┘
```

### Directory Structure

```
RuneAndRust.Engine/CoherentGlitch/
├── CoherentGlitchRule.cs           # Base class + enums
├── CoherentGlitchRuleEngine.cs     # Rule orchestrator
└── Rules/                          # Individual rule implementations
    ├── BossArenaAmplifierRule.cs
    ├── BrokenMaintenanceCycleRule.cs
    ├── ChasmInfrastructureRule.cs
    ├── DarknessStressAmplifierRule.cs
    ├── EntryHallSafetyRule.cs
    ├── FailedEvacuationNarrativeRule.cs
    ├── FloodedElectricalDangerRule.cs
    ├── GeothermalSteamRule.cs
    ├── HiddenContainerDiscoveryRule.cs
    ├── LongCorridorAmbushRule.cs
    ├── MaintenanceHubOrganizationRule.cs
    ├── NoSteamInFloodedRule.cs
    ├── PowerStationElectricalRule.cs
    ├── ResourceVeinClusterRule.cs
    ├── SecretRoomRewardRule.cs
    ├── TacticalCoverPlacementRule.cs
    └── UnstableCeilingRubbleRule.cs
```

---

## Public API

### `ApplyRules`

Applies all coherent glitch rules to a single room.

```csharp
public void ApplyRules(Room room, PopulationContext context)
```

**Parameters:**
- `room` - The room to process
- `context` - Population context with biome elements and tracking

**Behavior:**
1. Sets `context.CurrentRoom = room`
2. Groups rules by priority
3. Executes rules in priority order (Critical → High → Medium → Low)
4. Tracks total rules fired and per-rule fire counts

---

### `ApplyRulesToDungeon`

Applies all coherent glitch rules to every room in a dungeon.

```csharp
public void ApplyRulesToDungeon(Dungeon dungeon, PopulationContext context)
```

**Parameters:**
- `dungeon` - The dungeon containing rooms to process
- `context` - Population context shared across all rooms

**Behavior:**
1. Sets `context.CurrentDungeon = dungeon`
2. Iterates through all rooms in dungeon
3. Calls `ApplyRules()` for each room
4. Logs final statistics (total rules fired, per-rule counts)

---

### `GetRules`

Returns all registered rules (read-only).

```csharp
public IReadOnlyList<CoherentGlitchRule> GetRules()
```

---

## Rule System

### Base Rule Class

All rules inherit from `CoherentGlitchRule`:

```csharp
public abstract class CoherentGlitchRule
{
    public string RuleId { get; set; }
    public string Description { get; set; }
    public RulePriority Priority { get; set; }
    public RuleType Type { get; set; }

    // Override these in derived classes
    public abstract bool ShouldApply(Room room, PopulationContext context);
    public abstract void Apply(Room room, PopulationContext context);

    // Called by engine - handles logging and tracking
    public bool Execute(Room room, PopulationContext context);
}
```

### Rule Priorities

Rules execute in priority order (lowest enum value first):

| Priority | Value | Purpose | Example Rules |
|----------|-------|---------|---------------|
| `Critical` | 0 | Structural requirements | `UnstableCeilingRubbleRule` |
| `High` | 1 | Safety and exclusions | `EntryHallSafetyRule`, `NoSteamInFloodedRule` |
| `Medium` | 2 | Thematic coherence, tactical | `BossArenaAmplifierRule`, `TacticalCoverPlacementRule` |
| `Low` | 3 | Polish and aesthetics | `HiddenContainerDiscoveryRule`, `ResourceVeinClusterRule` |

### Rule Types

| Type | Purpose | Effect |
|------|---------|--------|
| `Mandatory` | Must fire if conditions met | Always applies when conditions match |
| `Weighted` | Modifies spawn probabilities | Changes element spawn weights |
| `Exclusion` | Prevents invalid combinations | Blocks conflicting spawns |
| `Contextual` | Based on room properties | Reacts to room archetype, terrain |
| `Tactical` | Gameplay positioning | Places elements strategically |

---

## Registered Rules (17 Total)

### Critical Priority (1 Rule)

| Rule | Type | Description |
|------|------|-------------|
| `UnstableCeilingRubbleRule` | Mandatory | Structural rubble in unstable areas |

### High Priority (4 Rules)

| Rule | Type | Description |
|------|------|-------------|
| `FloodedElectricalDangerRule` | Weighted | Water enhances electrical hazards (2x damage, 1.5x range) |
| `DarknessStressAmplifierRule` | Weighted | Darkness increases stress effects |
| `NoSteamInFloodedRule` | Exclusion | Steam hazards cannot spawn in flooded rooms |
| `EntryHallSafetyRule` | Exclusion | Entry halls have reduced hazards |

### Medium Priority (6 Rules)

| Rule | Type | Description |
|------|------|-------------|
| `GeothermalSteamRule` | Contextual | Geothermal areas spawn steam vents |
| `MaintenanceHubOrganizationRule` | Contextual | Maintenance rooms have organized storage |
| `PowerStationElectricalRule` | Contextual | Power stations spawn electrical hazards |
| `TacticalCoverPlacementRule` | Tactical | Places cover in combat-appropriate locations |
| `LongCorridorAmbushRule` | Tactical | Long corridors set up ambush points |
| `SecretRoomRewardRule` | Contextual | Secret rooms have enhanced loot |
| `BossArenaAmplifierRule` | Contextual | Boss rooms have amplified hazards |

### Low Priority (6 Rules)

| Rule | Type | Description |
|------|------|-------------|
| `FailedEvacuationNarrativeRule` | Contextual | Adds evacuation narrative elements |
| `BrokenMaintenanceCycleRule` | Contextual | Shows broken maintenance systems |
| `ChasmInfrastructureRule` | Contextual | Chasm areas have damaged infrastructure |
| `HiddenContainerDiscoveryRule` | Contextual | Hidden containers with special loot |
| `ResourceVeinClusterRule` | Contextual | Resource veins cluster logically |

---

## Rule Example: FloodedElectricalDangerRule

```csharp
public class FloodedElectricalDangerRule : CoherentGlitchRule
{
    public FloodedElectricalDangerRule()
    {
        RuleId = "flooded_electrical_danger";
        Description = "Flooded condition enhances electrical hazards";
        Priority = RulePriority.High;
        Type = RuleType.Weighted;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        // Check if room has flooded condition
        return room.AmbientConditions.Any(c => c.Type == "Flooded");
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // 1. Increase power conduit spawn weight (2.5x)
        var powerConduit = context.BiomeElements.GetElement("live_power_conduit");
        if (powerConduit != null)
            powerConduit.Weight *= 2.5f;

        // 2. Enhance existing power conduits
        foreach (var hazard in room.DynamicHazards.OfType<LivePowerConduitHazard>())
        {
            hazard.DamageMultiplier = 2.0;  // Double damage
            hazard.RangeMultiplier = 1.5;   // +50% range
            hazard.IsFloodedEnhanced = true;
        }

        // 3. Update room description
        room.Description += " The stagnant water conducts residual electrical charge.";
    }
}
```

---

## Integration Points

### Called By

| Caller | Context |
|--------|---------|
| `PopulationPipeline` | During room population phase |
| `DungeonGenerator` | After room instantiation (via PopulationPipeline) |

### Works With

| System | Interaction |
|--------|-------------|
| `PopulationContext` | Shares biome elements, tracks rule statistics |
| `BiomeElementCache` | Modifies element spawn weights |
| `Room` | Modifies hazards, terrain, descriptions |
| `DynamicHazard` | Enhances or restricts hazard properties |

---

## Data Flow

### Rule Execution Flow

```
PopulationPipeline.PopulateRoom()
           │
           ▼
┌─────────────────────────────┐
│ CoherentGlitchRuleEngine    │
│   .ApplyRules(room, ctx)    │
└───────────┬─────────────────┘
            │
            ▼
┌─────────────────────────────┐
│  Group Rules by Priority    │
│  Critical → High → Medium   │
│            → Low            │
└───────────┬─────────────────┘
            │
            ▼
    ┌───────────────┐
    │  For Each     │
    │    Rule       │◄──────────┐
    └───────┬───────┘           │
            │                   │
            ▼                   │
    ┌───────────────┐           │
    │ ShouldApply() │           │
    └───────┬───────┘           │
            │                   │
      Yes   │   No              │
    ┌───────┴───────┐           │
    │               │           │
    ▼               │           │
┌─────────┐         │           │
│ Apply() │         │           │
└────┬────┘         │           │
     │              │           │
     ▼              ▼           │
┌─────────────────────────────┐ │
│  Track Statistics           │ │
│  - TotalRulesFired++        │ │
│  - RuleFireCounts[id]++     │ │
└───────────┬─────────────────┘ │
            │                   │
            └───────────────────┘
                 Next Rule
```

---

## PopulationContext

The `PopulationContext` tracks state across rule execution:

```csharp
public class PopulationContext
{
    public Room? CurrentRoom { get; set; }
    public Dungeon? CurrentDungeon { get; set; }
    public BiomeElementCache BiomeElements { get; set; }

    // Rule statistics
    public int TotalRulesFired { get; set; }
    public Dictionary<string, int> RuleFireCounts { get; }
}
```

---

## Creating New Rules

### Step 1: Create Rule Class

```csharp
// File: Rules/MyCustomRule.cs
namespace RuneAndRust.Engine.CoherentGlitch.Rules;

public class MyCustomRule : CoherentGlitchRule
{
    public MyCustomRule()
    {
        RuleId = "my_custom_rule";
        Description = "Description of what this rule does";
        Priority = RulePriority.Medium;
        Type = RuleType.Contextual;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        // Return true if rule should fire
        return room.Archetype == "MyArchetype";
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Modify room, hazards, weights, descriptions
    }
}
```

### Step 2: Register in Engine

```csharp
// In CoherentGlitchRuleEngine.RegisterRules()
_rules.Add(new Rules.MyCustomRule());
```

---

## Design Considerations

### Rule Ordering

Rules are grouped by priority, then executed in registration order within each group. This means:
- Critical rules always run first (structural setup)
- High-priority exclusions can block later spawns
- Medium rules apply thematic modifications
- Low rules add polish without affecting core gameplay

### Rule Conflicts

The exclusion rule type exists specifically to prevent conflicts:
- `NoSteamInFloodedRule` prevents steam in water
- `EntryHallSafetyRule` keeps starting areas safer

When creating new rules, consider:
1. Does this conflict with existing rules?
2. Should there be an exclusion rule?
3. What priority ensures correct execution order?

### Performance

Rules are evaluated for every room in the dungeon. Keep `ShouldApply()` lightweight:
- Avoid expensive LINQ queries
- Cache frequently-accessed properties
- Use early returns for common negative cases

---

## Version History

| Version | Changes |
|---------|---------|
| v0.12 | Initial implementation with 17 rules |

---

## Cross-References

### Related Documentation

- [PopulationPipeline](./population-pipeline.md) - Room population orchestration
- [BiomeLibrary](./biome-library.md) - Biome element definitions

### Related Services

- `PopulationPipeline` - Calls CoherentGlitchRuleEngine during population
- `BiomeElementCache` - Modified by weighted rules
- `HazardSpawner` - Creates hazards that rules enhance

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27
