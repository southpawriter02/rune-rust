---
trigger: always_on
---

# Definition Pattern Rules

## 1.1 *Definition Entities
Use this pattern for all extensible game systems:

| System         | Definition Entity       | Config File           |
|----------------|-------------------------|-----------------------|
| Races          | `RaceDefinition`        | `races.json`          |
| Classes        | `ClassDefinition`       | `classes.json`        |
| Abilities      | `AbilityDefinition`     | `abilities.json`      |
| Resources      | `ResourceTypeDefinition`| `resources.json`      |
| Damage Types   | `DamageTypeDefinition`  | `damage-types.json`   |
| Monsters       | `MonsterDefinition`     | `monsters.json`       |
| Status Effects | `StatusEffectDefinition`| `status-effects.json` |

## 1.2 String IDs Over Enums

```csharp
// ✅ DO: Use string identifiers loaded from config
public string DamageTypeId { get; private set; } // "fire", "ice"

// ❌ DON'T: Use enums for extensible types
public DamageType DamageType { get; set; } // Fire, Ice, Poison
```

## 1.3 ID Normalization
Always normalize IDs:

```csharp
AbilityId = abilityId.ToLowerInvariant();  // "Shield-Bash" → "shield-bash"
```