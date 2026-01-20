---
trigger: always_on
---

# Data-Driven Design Rules

## 1.1 Configuration Over Code
- **PREFER** loading definitions from JSON configuration files over hardcoded values
- **NEVER** hardcode game constants like max levels, stat values, or terminology
- **ALWAYS** use `*Definition` entities for extensible type systems (e.g., `ClassDefinition`, `MonsterDefinition`, `DamageTypeDefinition`)

## 1.2 Terminology Abstraction
- **NEVER** hardcode player-facing terminology in code
- **ALWAYS** support configurable terminology for:
  - Experience points (XP, Legend, Glory, Essence)
  - Levels (Level, Saga, Rank, Tier)
  - Health (Health, Vitality, Life Force, HP)
  - Resources (Mana, Arcane Power, Rage, Fury)

## 1.3 Definition Pattern
When adding new game systems:

```csharp
// ✅ DO: Use data-driven definitions
public class DamageTypeDefinition : IEntity
{
    public string Id { get; private set; }        // "fire", "ice"
    public string DisplayName { get; private set; } // "Fire", "Ice"
    // Loaded from config/damage-types.json
}

// ❌ DON'T: Use hardcoded enums for extensible types
public enum DamageType { Fire, Ice, Poison }
```