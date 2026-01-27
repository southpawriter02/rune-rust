# ADR-007: Resource System Design

**Status:** Accepted
**Date:** 2026-01-06
**Deciders:** Development Team

## Context

Different character classes in Rune and Rust use different resource systems:

- **Warriors**: Rage that builds during combat and decays outside combat
- **Mystics**: Mana that regenerates slowly over time
- **Rogues**: Stamina/Focus that regenerates quickly

The resource system must:
- Support different regeneration and decay behaviors per resource type
- Distinguish between combat and non-combat regeneration rates
- Allow resources to start at zero (rage) or full (mana)
- Integrate with ability costs and turn processing
- Be configurable via JSON without code changes

## Decision

We will implement a configurable resource system with the following components:

### Core Components

1. **ResourceTypeDefinition**: Configures resource behavior
2. **ResourcePool**: Tracks current/max values for a specific resource
3. **ResourceService**: Handles resource operations (spend, restore, process turn)
4. **Turn-End Processing**: Applies regeneration/decay each turn

### Resource Type Properties

```csharp
public class ResourceTypeDefinition
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Abbreviation { get; init; }  // "MP", "RG", "ST"
    public string Color { get; init; }          // For display
    public int DefaultMaxValue { get; init; }   // Base max (before class modifiers)

    // Regeneration (positive) or decay (negative)
    public int RegenPerTurnCombat { get; init; }     // During combat
    public int RegenPerTurnNonCombat { get; init; }  // Outside combat

    public bool StartsAtZero { get; init; }  // Rage starts at 0, Mana starts at max
}
```

### Resource Behaviors by Type

| Resource | Start | Combat Regen | Non-Combat Regen | Behavior |
|----------|-------|--------------|------------------|----------|
| Mana | Full | +5/turn | +10/turn | Slow regeneration |
| Rage | Zero | +15/turn | -10/turn | Builds in combat, decays outside |
| Faith | Full | +3/turn | +5/turn | Slow, steady regeneration |
| Stamina | Full | +8/turn | +15/turn | Fast regeneration |
| Focus | Full | +5/turn | +10/turn | Moderate regeneration |

### Turn Processing Flow

```
Turn Start
    │
    ▼
Player Action (may spend resource)
    │
    ▼
Turn End Processing
    ├── Is in combat?
    │   ├── Yes: Apply combat regen rate
    │   └── No: Apply non-combat regen rate
    │
    ├── Apply rate to current value
    │   └── Clamp between 0 and max
    │
    └── Return resource change info
```

## Consequences

### Positive

- **Flexibility**: Different classes feel mechanically distinct
- **Configuration**: Resource behaviors tunable without code changes
- **Extensibility**: Easy to add new resource types
- **Clear Model**: ResourcePool encapsulates state cleanly
- **Combat Identity**: Warriors and rogues play differently in vs. out of combat

### Negative

- **Complexity**: Turn processing must track combat state
- **Balance Challenge**: Multiple resource types harder to balance
- **Testing Surface**: More combinations to test

### Neutral

- Resources are per-player, not shared
- Each class has exactly one primary resource
- Resource maximum can be modified by equipment/effects

## Implementation Details

### ResourceTypeDefinition Configuration

```json
{
  "resourceTypes": [
    {
      "id": "rage",
      "name": "Rage",
      "abbreviation": "RG",
      "color": "#FF4444",
      "defaultMaxValue": 100,
      "regenPerTurnCombat": 15,
      "regenPerTurnNonCombat": -10,
      "startsAtZero": true
    },
    {
      "id": "mana",
      "name": "Mana",
      "abbreviation": "MP",
      "color": "#4444FF",
      "defaultMaxValue": 100,
      "regenPerTurnCombat": 5,
      "regenPerTurnNonCombat": 10,
      "startsAtZero": false
    }
  ]
}
```

### ResourcePool Value Object

```csharp
public readonly record struct ResourcePool(
    string TypeId,
    int Current,
    int Maximum)
{
    public bool IsEmpty => Current <= 0;
    public bool IsFull => Current >= Maximum;
    public float Percentage => Maximum > 0 ? (float)Current / Maximum : 0;

    public ResourcePool Spend(int amount)
    {
        var newCurrent = Math.Max(0, Current - amount);
        return this with { Current = newCurrent };
    }

    public ResourcePool Restore(int amount)
    {
        var newCurrent = Math.Min(Maximum, Current + amount);
        return this with { Current = newCurrent };
    }

    public ResourcePool ApplyRegen(int amount)
    {
        var newCurrent = Math.Clamp(Current + amount, 0, Maximum);
        return this with { Current = newCurrent };
    }
}
```

### ResourceService

```csharp
public class ResourceService : IResourceService
{
    private readonly IConfigurationProvider _config;

    public ResourceChange ProcessTurnEnd(Player player, bool isInCombat)
    {
        var changes = new List<ResourceChange>();

        foreach (var (typeId, pool) in player.Resources)
        {
            var definition = _config.GetResourceType(typeId);
            var regenRate = isInCombat
                ? definition.RegenPerTurnCombat
                : definition.RegenPerTurnNonCombat;

            var before = pool.Current;
            var newPool = pool.ApplyRegen(regenRate);
            player.SetResource(typeId, newPool);

            changes.Add(new ResourceChange(
                TypeId: typeId,
                Before: before,
                After: newPool.Current,
                Amount: newPool.Current - before));
        }

        return new ResourceChangeSummary(changes);
    }

    public bool CanSpend(Player player, string typeId, int amount)
    {
        return player.Resources.TryGetValue(typeId, out var pool)
            && pool.Current >= amount;
    }

    public SpendResult Spend(Player player, string typeId, int amount)
    {
        if (!CanSpend(player, typeId, amount))
            return SpendResult.InsufficientResource(typeId, amount);

        var pool = player.Resources[typeId];
        player.SetResource(typeId, pool.Spend(amount));
        return SpendResult.Success(typeId, amount);
    }
}
```

### Player Resource Initialization

```csharp
public class ClassService : IClassService
{
    public void AssignClass(Player player, ClassDefinition classDef)
    {
        player.ClassId = classDef.Id;

        // Initialize primary resource
        var resourceDef = _config.GetResourceType(classDef.PrimaryResourceId);
        var startValue = resourceDef.StartsAtZero ? 0 : resourceDef.DefaultMaxValue;

        player.InitializeResource(new ResourcePool(
            TypeId: resourceDef.Id,
            Current: startValue,
            Maximum: resourceDef.DefaultMaxValue));
    }
}
```

### Ability Cost Integration

```csharp
public class AbilityService : IAbilityService
{
    public UseAbilityResult UseAbility(Player player, string abilityId)
    {
        var ability = _config.GetAbility(abilityId);

        // Check resource cost
        if (!_resourceService.CanSpend(player, ability.Cost.ResourceTypeId, ability.Cost.Amount))
        {
            return UseAbilityResult.InsufficientResource(
                ability.Cost.ResourceTypeId,
                ability.Cost.Amount,
                player.GetResource(ability.Cost.ResourceTypeId).Current);
        }

        // Spend resource
        _resourceService.Spend(player, ability.Cost.ResourceTypeId, ability.Cost.Amount);

        // Apply ability effects...
        return UseAbilityResult.Success(ability, effects);
    }
}
```

## Example Gameplay Flow

### Berserker (Rage)

```
Turn 1 (Combat): Rage 0 → 15 (builds)
Turn 2 (Combat): Use Rage Strike (-25) → Rage 15 → -10 → then +15 = 5
Turn 3 (Combat): Rage 5 → 20 (builds)
Turn 4 (Exit Combat): Rage 20 → 10 (decays)
Turn 5 (Exploring): Rage 10 → 0 (decays to zero)
```

### Mage (Mana)

```
Turn 1 (Combat): Mana 100 (full)
Turn 2 (Combat): Cast Fireball (-30) → Mana 70 → 75 (slow regen)
Turn 3 (Combat): Cast Fireball (-30) → Mana 45 → 50
Turn 4 (Exit Combat): Mana 50 → 60 (faster regen)
Turn 5 (Exploring): Mana 60 → 70
```

## Alternatives Considered

### Alternative 1: Unified Resource System

All classes use the same resource (e.g., "Energy").

**Rejected because:**
- Classes feel too similar mechanically
- Loses class identity and fantasy
- Less interesting strategic decisions

### Alternative 2: Cooldown-Only System

No resources, only ability cooldowns.

**Rejected because:**
- Less resource management depth
- Doesn't capture rage/mana fantasy
- Cooldowns already used alongside resources

### Alternative 3: Per-Ability Resources

Each ability has its own resource pool.

**Rejected because:**
- Too complex to track
- Confusing for players
- Harder to balance

### Alternative 4: Fixed Regeneration Rates

Same regen rate regardless of combat state.

**Rejected because:**
- Rage wouldn't feel right (should decay outside combat)
- Less tactical depth
- Combat vs. exploration less distinct

## Related

- [ADR-002](ADR-002-json-configuration.md): JSON Configuration (resource type definitions)
- [ADR-006](ADR-006-definition-pattern.md): Definition Pattern (ResourceTypeDefinition)
- [UC-102](../use-cases/system/UC-102-regenerate-resources.md): Regenerate Resources use case
- [v0.0.4 Implementation Specification](../v0.0.x/implementation-specifications/v0.0.4-implementation-specification.md): Resource system implementation
