---
id: SPEC-ENEMY-001
title: Enemy Definitions & Archetypes
version: 2.0.0
status: Implemented
created: 2025-12-22
last_updated: 2025-12-24
tags: [enemy, archetype, entity, combat]
related_specs: [SPEC-COMBAT-001, SPEC-TRAIT-001, SPEC-AI-001, SPEC-ABILITY-001, SPEC-ENEMYFAC-001]
---

# Enemy Definitions & Archetypes

## Overview

This specification defines the **Enemy entity structure**, **archetype classifications**, and **tag system** for enemy combatants in Rune & Rust. Each enemy is assigned one of seven archetypes that define their combat role and influence AI behavior.

> **Important:** For AI decision logic and scoring algorithms, see [SPEC-AI-001](./SPEC-AI-001.md). This specification focuses on *what enemies are*, not *how they think*.

### Scope

| This Spec Covers | See Instead |
|------------------|-------------|
| Enemy entity properties | |
| EnemyArchetype enum definitions | |
| Tags and trait integration | |
| AI behavior | [SPEC-AI-001](./SPEC-AI-001.md) |
| Enemy templates/factory | [SPEC-ENEMYFAC-001](./SPEC-ENEMYFAC-001.md) |

### Implementation Location

- **Entity**: [RuneAndRust.Core/Entities/Enemy.cs](../../RuneAndRust.Core/Entities/Enemy.cs)
- **Archetype Enum**: [RuneAndRust.Core/Enums/EnemyArchetype.cs](../../RuneAndRust.Core/Enums/EnemyArchetype.cs)
- **AI Service**: [RuneAndRust.Engine/Services/EnemyAIService.cs](../../RuneAndRust.Engine/Services/EnemyAIService.cs) (see SPEC-AI-001)

---

## EnemyArchetype Enum

Combat role archetype that influences AI behavior through scoring modifiers in [SPEC-AI-001](./SPEC-AI-001.md).

```csharp
public enum EnemyArchetype
{
    Tank,        // High HP/Soak, prioritizes defending
    DPS,         // Balanced offense
    GlassCannon, // High damage, low HP
    Support,     // Buff/debuff role
    Swarm,       // Light attacks, numerical superiority
    Caster,      // Ranged/AoE attacks
    Boss         // Multi-phase encounters
}
```

### Archetype Definitions

| Archetype | Philosophy | AI Scoring Effect | Examples |
|-----------|------------|-------------------|----------|
| **Tank** | Protect allies, absorb damage | +55 Defend when <40% HP, +10 Defend always | Haugbui Laborer, Shield-Drone |
| **DPS** | Reliable damage output | No special modifiers (baseline) | Rusted Draugr, Rust-Clan Warrior |
| **GlassCannon** | High-risk offense | +20 damage abilities | Ash-Vargr, Scrap-Hound |
| **Support** | Enable allies | +15 Defend when <50% HP | Rust-Clan Engineer, Blight-Priest |
| **Swarm** | Strength in numbers | +20 Light attacks | Utility Servitor, Scrap-Mite swarm |
| **Caster** | Ranged attacks | Standard scoring | Rust-Witch, Corrupted Turret |
| **Boss** | Major threat | +10 Heavy attacks | Haugbui Warlord, The Corruptor |

---

## Enemy Entity

The `Enemy` class represents an enemy combatant instance.

### Core Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique identifier |
| `Name` | `string` | Display name |
| `Attributes` | `Dictionary<Attribute, int>` | STURD, MIGHT, WITS, WILL, FIN scores |
| `MaxHp` / `CurrentHp` | `int` | Health points |
| `MaxStamina` / `CurrentStamina` | `int` | Stamina points |

### Equipment Stats

| Property | Type | Description |
|----------|------|-------------|
| `WeaponDamageDie` | `int` | Damage die size (e.g., 6 for d6) |
| `WeaponAccuracyBonus` | `int` | Accuracy modifier |
| `ArmorSoak` | `int` | Damage reduction |
| `WeaponName` | `string` | Display name for weapon |

### AI & Template Properties

| Property | Type | Description |
|----------|------|-------------|
| `TemplateId` | `string?` | Source template ID |
| `Archetype` | `EnemyArchetype` | Combat role (default: DPS) |
| `Tags` | `List<string>` | Status effect immunities and AI triggers |
| `ActiveTraits` | `List<CreatureTraitType>` | Elite/Champion traits |
| `Abilities` | `List<ActiveAbility>` | Available combat abilities |

### Derived Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsElite` | `bool` | True if `ActiveTraits.Count > 0` |

---

## Tags System

Tags modify AI behavior and provide status effect interactions.

### AI-Affecting Tags

| Tag | Effect | AI Behavior |
|-----|--------|-------------|
| `Cowardly` | Triggers flee behavior | +80 Flee score when HP <25% |

### Immunity Tags

| Tag | Effect |
|-----|--------|
| `Mechanical` | Bleed immune |
| `Undying` | Poison immune |

### Trait-Applied Tags

Creature traits from [SPEC-TRAIT-001](./SPEC-TRAIT-001.md) can apply tags during enemy creation:

- `Cowardly` trait → adds "Cowardly" tag
- `Berserker` trait → (future) adds "Berserker" tag

---

## AI Integration

Enemy properties influence AI decision-making through [SPEC-AI-001](./SPEC-AI-001.md).

### Archetype → Scoring

The `Archetype` property determines scoring modifiers:

```
Archetype.Tank + HP < 40% → +55 Defend score
Archetype.GlassCannon → +20 damage ability score
Archetype.Swarm → +20 Light attack score
```

### Tags → Behavior Triggers

Tags can override normal AI behavior:

```
Tags.Contains("Cowardly") + HP < 25% → +80 Flee score
```

### Abilities → Action Evaluation

The `Abilities` list is evaluated by `EvaluateAbilities()` in EnemyAIService:

```
foreach ability in enemy.Abilities:
  if CanAfford(ability.StaminaCost):
    score = BaseScore + ContextModifiers + ArchetypeBonus
    actions.Add(ability, score)
```

---

## Cross-System Integration

| System | Relationship |
|--------|--------------|
| **EnemyAIService** | Reads Archetype, Tags, Abilities to determine actions |
| **EnemyFactory** | Creates Enemy instances from templates |
| **CreatureTraitService** | Applies traits and tags to enemies |
| **CombatService** | Converts Enemy to Combatant for combat |
| **AttackResolutionService** | Uses WeaponDamageDie, ArmorSoak for damage calculation |

---

## Data Models

### CombatAction (Record)

Represents an enemy's intended action during their combat turn.

```csharp
public record CombatAction(
    ActionType Type,
    Guid SourceId,
    Guid? TargetId,
    AttackType? AttackType = null,
    string? FlavorText = null
);
```

### ActionType (Enum)

```csharp
public enum ActionType
{
    Attack,   // Offensive action (requires TargetId and AttackType)
    Defend,   // Defensive stance (no target)
    Flee,     // Retreat from combat (no target)
    Pass      // Skip turn (no target)
}
```

### AttackType (Enum)

```csharp
public enum AttackType
{
    Light,      // Low stamina cost (5), low damage
    Standard,   // Medium stamina cost (10), medium damage
    Heavy       // High stamina cost (20), high damage
}
```

---

## Changelog

### v2.0.0 (2025-12-24)
**Scope Change** - Refactored as "Enemy Definitions & Archetypes" specification.

#### Changed
- Title: "Enemy AI System" → "Enemy Definitions & Archetypes"
- Scope: AI decision logic moved to SPEC-AI-001

#### Removed
- All Execute*Logic method documentation (obsolete v0.2.2b decision trees)
- 5 decision tree diagrams (replaced by utility scoring in SPEC-AI-001)
- 4 sequence diagrams (reference removed methods)
- 7 use cases (reference removed code)
- Probability tables (replaced by scoring constants)
- Obsolete future extensions (already implemented)

#### Added
- Cross-reference to SPEC-AI-001 for behavior logic
- Updated Enemy entity properties from Enemy.cs
- Archetype scoring summary (reference to SPEC-AI-001 constants)

### v1.0.0 (2025-12-22)
**Initial Release** - Original Enemy AI System specification.

- Documented all 7 archetypes (Tank, DPS, GlassCannon, Support, Swarm, Caster, Boss)
- 27 unit tests documented with references
- 5 decision trees created (archetype routing, aggressive, tank, boss, stamina cascade)
- 4 sequence diagrams created (full flow, tank wounded, cowardly flee, stamina fallback)
- 7 use cases documented with code walkthroughs
- Cross-system integration matrix completed

---

## Notes

- **AI Behavior**: For detailed AI decision logic, scoring constants, and weighted selection, see [SPEC-AI-001](./SPEC-AI-001.md).
- **Enemy Templates**: For template definitions and factory creation, see [SPEC-ENEMYFAC-001](./SPEC-ENEMYFAC-001.md).
- **Boss Phases**: Multi-phase encounters are defined at archetype level but phase transition logic is in SPEC-AI-001.

---

**Specification Status**: ✅ Complete - Verified against Enemy.cs and EnemyArchetype.cs
