---
id: SPEC-CHAR-001
title: Character & Progression System
version: 1.1.0
status: Implemented
last_updated: 2025-12-23
related_specs: [SPEC-CORRUPT-001, SPEC-INV-001, SPEC-ADVANCEMENT-001]
---

# SPEC-CHAR-001: Character & Progression System

> **Version:** 1.1.0
> **Status:** Implemented
> **Services:** `StatCalculationService`, `CharacterFactory`
> **Location:** `RuneAndRust.Engine/Services/`, `RuneAndRust.Engine/Factories/`

---

## Table of Contents

- [Overview](#overview)
- [Core Attributes](#core-attributes)
- [Behaviors](#behaviors)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Changelog](#changelog)

---

## Overview

The Character & Progression System defines player characters through a combination of base attributes, lineage bonuses, archetype specializations, and equipment modifiers. It handles derived stat calculation, level progression, and the integration of multiple modifier sources.

---

## Core Attributes

### The Five Pillars

| Attribute | Description | Primary Usage |
|-----------|-------------|---------------|
| **STURDINESS** | Physical resilience and endurance | Max HP, Max Stamina |
| **MIGHT** | Raw physical power | Attack damage, carry capacity |
| **FINESSE** | Agility and precision | Defense score, Max Stamina |
| **WITS** | Perception and quick thinking | Action Points, ambush mitigation |
| **WILL** | Mental fortitude and magical aptitude | Stress mitigation, Max Aether |

### Attribute Bounds
- **Minimum:** 1 (never reduced below)
- **Maximum:** 10 (base cap)
- **Starting Range:** 1-5 (before bonuses)

---

## Behaviors

### Primary Behaviors

#### 1. Derived Stat Calculation (`RecalculateDerivedStats`)

```csharp
void RecalculateDerivedStats(Character character)
```

**Calculation Sequence:**
1. Get corruption state for penalty modifiers
2. Calculate effective attributes (base + equipment)
3. Apply corruption attribute penalties
4. Calculate MaxHP from effective STURDINESS
5. Calculate MaxStamina from effective FINESSE + STURDINESS
6. Calculate ActionPoints from effective WITS
7. Calculate MaxAP from effective WILL (Mystic only)
8. Apply corruption MaxAP penalty (percentage reduction)
9. Preserve current HP/Stamina ratio if max changed
10. Clamp CurrentAP to new MaxAP if reduced

**Formulas:**
```csharp
MaxHP = 50 + (Sturdiness × 10)
MaxStamina = 20 + (Finesse × 5) + (Sturdiness × 3)
ActionPoints = 2 + (Wits ÷ 4)
MaxAP = (Mystic only) 10 + (Will × 5)
```

**Corruption Integration:**
```csharp
// Corruption penalties from CorruptionState
effectiveWill -= corruptionState.WillPenalty;    // -1 at Blighted, -2 at Fractured/Terminal
effectiveWits -= corruptionState.WitsPenalty;    // -1 at Fractured/Terminal

// Corruption MaxAP Multipliers:
// - Pristine/Tainted: 100%
// - Corrupted: 90%
// - Blighted: 80%
// - Fractured: 60%
// - Terminal: 0%
MaxAP *= corruptionState.MaxApMultiplier;
```

#### 2. Attribute Bonuses by Archetype

```csharp
Dictionary<Attribute, int> GetArchetypeBonuses(ArchetypeType archetype)
```

| Archetype | Primary (+2) | Secondary (+1) |
|-----------|--------------|----------------|
| Warrior | STURDINESS | MIGHT |
| Skirmisher | FINESSE | WITS |
| Adept | WITS | WILL |
| Mystic | WILL | STURDINESS |

#### 3. Attribute Bonuses by Lineage

```csharp
Dictionary<Attribute, int> GetLineageBonuses(LineageType lineage)
```

| Lineage | Bonuses | Penalty |
|---------|---------|---------|
| Human | +1 to ALL attributes | (none) |
| Rune-Marked | +2 WITS, +2 WILL | -1 STURDINESS |
| Iron-Blooded | +2 STURDINESS, +2 MIGHT | -1 WITS |
| Vargr-Kin | +2 FINESSE, +2 WITS | -1 WILL |

#### 4. Effective Attribute Calculation

```csharp
// From Character entity
public int GetEffectiveAttribute(Attribute attr)
{
    var baseValue = GetAttribute(attr);
    var equipmentBonus = EquipmentBonuses.GetValueOrDefault(attr, 0);
    return baseValue + equipmentBonus;
}
```

**Modifier Stack:**
1. Base attribute (1-10)
2. + Lineage bonus/penalty
3. + Archetype bonus
4. + Equipment bonuses (from `EquipmentBonuses` dictionary)
5. + Condition modifiers (combat only, from ambient conditions)
6. - Corruption penalties (from corruption tier)
7. - Trauma penalties (from active traumas)

### Edge Case Behaviors

#### First-Time Initialization
When `MaxHP == 0` (new character), current values are set to max:
```csharp
if (previousMaxHP == 0)
{
    character.CurrentHP = character.MaxHP;
}
```

#### Ratio Preservation
When max stats change mid-game (equipment change):
```csharp
var hpRatio = (double)character.CurrentHP / previousMaxHP;
character.CurrentHP = Math.Max(1, (int)(character.MaxHP * hpRatio));

var staminaRatio = (double)character.CurrentStamina / previousMaxStamina;
character.CurrentStamina = Math.Max(0, (int)(character.MaxStamina * staminaRatio));
```

**HP vs Stamina Floor:**
- HP uses `Math.Max(1, ...)` - Character cannot die from ratio preservation
- Stamina uses `Math.Max(0, ...)` - Exhaustion (0 stamina) is a valid state

#### Minimum Attribute Floor
Attributes can never go below 1:
```csharp
effectiveWill = Math.Max(1, effectiveWill - corruptionState.WillPenalty);
```

---

## Restrictions

### Character Creation
1. **All attributes must be set** - No null/default attributes allowed
2. **Archetype required** - Determines abilities and bonuses
3. **Lineage required** - Determines racial bonuses

### Stat Modification
1. **No direct MaxHP/Stamina assignment** - Always recalculate via service
2. **Equipment changes trigger recalculation** - Must call after equip/unequip
3. **Corruption changes trigger recalculation** - MaxAP may change

### Level Progression
1. **Level 1-5 only** - Hard cap on progression
2. **XP thresholds are cumulative** - Cannot skip levels

---

## Limitations

### Numerical Bounds
| Stat | Minimum | Maximum | Notes |
|------|---------|---------|-------|
| Base Attributes | 1 | 10 | After lineage/archetype |
| Effective Attributes | 1 | Unbounded | Equipment can exceed 10 |
| Level | 1 | 5 | Hard cap |
| MaxHP | 60 | 150 | At STURDINESS 1-10 |
| MaxStamina | 28 | 73 | At FINESSE/STURDINESS 1-10 |
| MaxAether (Mystic) | 15 | 60 | At WILL 1-10 |

### Character Slots
- Currently no limit on save slots
- No character transfer between saves

---

## Use Cases

### UC-1: Character Creation
```csharp
var factory = new CharacterFactory(statCalcService, abilityRepo);

var character = await factory.CreateAsync(
    name: "Valdris",
    lineage: LineageType.IronBlooded,
    archetype: ArchetypeType.Warrior,
    baseAttributes: new Dictionary<Attribute, int>
    {
        { Attribute.Sturdiness, 4 },
        { Attribute.Might, 4 },
        { Attribute.Finesse, 3 },
        { Attribute.Wits, 2 },
        { Attribute.Will, 2 }
    }
);

// Final attributes after bonuses:
// STU: 4 + 2 (Archetype) + 2 (Lineage) = 8
// MGT: 4 + 1 (Archetype) + 2 (Lineage) = 7
// FIN: 3 + 0 + 0 = 3
// WIT: 2 + 0 - 1 (Lineage penalty) = 1
// WIL: 2 + 0 + 0 = 2
```

### UC-2: Equipment Change
```csharp
await inventoryService.EquipItemAsync(character, "Iron Helm");
// Helm provides +1 STURDINESS

// Automatic recalculation:
// MaxHP: 50 + (9 × 10) = 140 (was 130)
// CurrentHP preserved at ratio
```

### UC-3: Level Up - NOT YET IMPLEMENTED

> **Status:** Planned - See [SPEC-ADVANCEMENT-001](SPEC-ADVANCEMENT-001.md)

```csharp
// Planned implementation:
void LevelUp(Character character)
{
    character.Level++;

    // Bonuses
    character.MaxHP += 10;
    character.MaxStamina += 5;
    // +1 attribute point (UI handles distribution)

    // Full heal on level up
    character.CurrentHP = character.MaxHP;
    character.CurrentStamina = character.MaxStamina;
}
```

---

## Decision Trees

### Character Creation Flow

```
┌─────────────────────────────────┐
│  Player Starts New Character    │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    │ Enter Name      │
    │ (2-50 chars)    │
    └────────┬────────┘
             │
    ┌────────┴────────────┐
    │ Select Lineage      │
    │ (Human/RuneMarked/  │
    │  IronBlooded/Vargr) │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Select Archetype    │
    │ (Warrior/Skirmisher/│
    │  Adept/Mystic)      │
    └────────┬────────────┘
             │
    ┌────────┴─────────────────┐
    │ Apply Bonuses:           │
    │ 1. Base attributes (5)   │
    │ 2. + Lineage bonus/pen   │
    │ 3. + Archetype bonus     │
    │ 4. Clamp to [1, 10]      │
    └────────┬─────────────────┘
             │
    ┌────────┴────────────┐
    │ Calculate Derived   │
    │ Stats (MaxHP, etc.) │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Persist to Database │
    └─────────────────────┘
```

### Derived Stat Recalculation Flow

```
┌────────────────────────────────────┐
│  Trigger: Equipment/Corruption     │
│  change or level-up                │
└───────────────┬────────────────────┘
                │
       ┌────────┴────────────┐
       │ Get Corruption State │
       └────────┬────────────┘
                │
       ┌────────┴────────────────────┐
       │ Calculate Effective Attrs:  │
       │ Base + Equipment - Penalties│
       └────────┬────────────────────┘
                │
       ┌────────┴────────────────────┐
       │ Apply Formulas:             │
       │ MaxHP = 50 + (STU × 10)     │
       │ MaxStamina = 20 + FIN×5... │
       │ ActionPoints = 2 + WIT÷4   │
       │ MaxAP = Mystic? 10+WIL×5:0 │
       └────────┬────────────────────┘
                │
       ┌────────┴────────────────────┐
       │ Apply Corruption Penalties: │
       │ MaxAP *= Multiplier         │
       └────────┬────────────────────┘
                │
       ┌────────┴────────────────────┐
       │ Preserve Ratios:            │
       │ HP: Math.Max(1, ratio×new)  │
       │ STA: Math.Max(0, ratio×new) │
       └────────┬────────────────────┘
                │
       ┌────────┴──────────┐
       │ Update Character  │
       └───────────────────┘
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `ILogger<StatCalculationService>` | Infrastructure | Calculation tracing |
| `CorruptionState` | [SPEC-CORRUPT-001](SPEC-CORRUPT-001.md) | Penalty calculations |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `CharacterFactory` | [SPEC-CHAR-001](SPEC-CHAR-001.md) | Initial stat calculation |
| `InventoryService` | [SPEC-INV-001](SPEC-INV-001.md) | Recalculate on equip |
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Effective attributes in combat |
| `AttackResolutionService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | MIGHT/FINESSE for combat |
| `TraumaService` | [SPEC-TRAUMA-001](SPEC-TRAUMA-001.md) | WILL for resolve checks |
| `AmbushService` | [SPEC-REST-001](SPEC-REST-001.md) | WITS for mitigation |

---

## Related Services

### Primary Implementation
| File | Purpose | Key Lines |
|------|---------|-----------|
| `StatCalculationService.cs` | Derived stat calculations | 61-113 (formulas), 116-212 (recalculation) |
| `CharacterFactory.cs` | Character creation | 50-86 (CreateSimple), 96-150 (CreateWithDistribution) |

### Supporting Types
| File | Purpose | Key Lines |
|------|---------|-----------|
| `Character.cs` | Core entity | 15-273 (all properties and methods) |
| `ArchetypeType.cs` | Archetype enum | 1-32 (4 archetypes) |
| `LineageType.cs` | Lineage enum | 1-32 (4 lineages) |
| `Attribute.cs` | Attribute enum | 1-33 (5 attributes) |
| `BackgroundType.cs` | Background enum (v0.3.4c) | 1-26 (6 backgrounds) |

---

## Data Models

### Character Entity
```csharp
public class Character
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";

    // Core Attributes (1-10)
    public int Sturdiness { get; set; }
    public int Might { get; set; }
    public int Finesse { get; set; }
    public int Wits { get; set; }
    public int Will { get; set; }

    // Derived Stats
    public int MaxHP { get; set; }
    public int CurrentHP { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentStamina { get; set; }
    public int ActionPoints { get; set; }
    public int MaxAp { get; set; }
    public int CurrentAp { get; set; }

    // Progression
    public int Level { get; set; } = 1;
    public int ExperiencePoints { get; set; } = 0;

    // Psychological Stats
    public int PsychicStress { get; set; } = 0;
    public int Corruption { get; set; } = 0;

    // Classification
    public LineageType Lineage { get; set; }
    public ArchetypeType Archetype { get; set; }
    public BackgroundType Background { get; set; }  // v0.3.4c

    // Equipment Integration
    public Dictionary<Attribute, int> EquipmentBonuses { get; set; } = new();

    // Trauma Tracking
    public List<Trauma> ActiveTraumas { get; set; } = new();

    // Status Effects (persistent, like Exhausted)
    public HashSet<StatusEffectType> ActiveStatusEffects { get; set; } = new();
}
```

### ArchetypeType Enum
```csharp
public enum ArchetypeType
{
    Warrior,    // Frontline combatant, high STURDINESS
    Skirmisher, // Agile fighter, high FINESSE
    Adept,      // Tactical mind, high WITS
    Mystic      // Aether wielder, high WILL (only class with MaxAP)
}
```

### LineageType Enum
```csharp
public enum LineageType
{
    Human,      // Balanced, +1 all
    RuneMarked, // Magical heritage, +WITS/WILL, -STURDINESS
    IronBlooded,// Dwarven heritage, +STURDINESS/MIGHT, -WITS
    VargrKin    // Beast heritage, +FINESSE/WITS, -WILL
}
```

### BackgroundType Enum (v0.3.4c)
```csharp
public enum BackgroundType
{
    Scavenger = 0,  // Born among rust heaps, surviving on salvage
    Exile = 1,      // Cast out from settlement for crimes real or imagined
    Scholar = 2,    // Seeker of forbidden knowledge from the old world
    Soldier = 3,    // Former soldier from a fallen outpost
    Noble = 4,      // Survivor of noble lineage, now dispossessed
    Cultist = 5     // Once a follower of a corrupted faith
}
```

**Note:** BackgroundType is narrative flavor only. Does not affect attributes or derived stats. Used in prologue generation and dialogue flavor.

---

## Configuration

### Stat Formulas (Hardcoded)
```csharp
// MaxHP
private const int BaseHP = 50;
private const int HPPerSturdiness = 10;

// MaxStamina
private const int BaseStamina = 20;
private const int StaminaPerFinesse = 5;
private const int StaminaPerSturdiness = 3;

// ActionPoints
private const int BaseAP = 2;
private const int APPerWits = 4; // divisor

// MaxAether (Mystic)
private const int BaseAether = 10;
private const int AetherPerWill = 5;
```

### Level Progression (Placeholder)
```csharp
// XP thresholds not yet implemented
// Target: 100/300/600/1000 XP for levels 2-5
```

---

## Testing

### Test Files
| File | Tests | Coverage |
|------|-------|----------|
| `StatCalculationServiceTests.cs` | 50+ tests | Formula validation, edge cases, corruption penalties |
| `CharacterFactoryTests.cs` | 16+ tests | All lineage/archetype combinations |
| `CharacterTests.cs` | TBD | Entity behavior, status effects |

### Critical Test Scenarios
1. Derived stat calculation accuracy (all 4 formulas)
2. Archetype bonus application (4 archetypes × 5 attributes)
3. Lineage bonus/penalty application (4 lineages × 5 attributes)
4. All 16 lineage/archetype combinations validated
5. Equipment bonus integration via GetEffectiveAttribute
6. Corruption penalty application (5 tiers)
7. Ratio preservation on max changes (HP and Stamina separate)
8. Minimum attribute enforcement (≥1 after penalties)
9. Mystic-only Aether pool (non-Mystic returns 0)
10. First-time initialization vs. recalculation logic

### Validation Checklist
- [ ] All 4 archetypes create with correct bonuses
- [ ] All 4 lineages apply correct bonuses/penalties
- [ ] MaxHP formula matches spec: `50 + (STU × 10)`
- [ ] MaxStamina formula matches spec: `20 + (FIN × 5) + (STU × 3)`
- [ ] ActionPoints formula matches spec: `2 + (WIT ÷ 4)`
- [ ] MaxAP formula matches spec: `10 + (WILL × 5)` (Mystic only)
- [ ] Corruption multipliers: 100%/90%/80%/60%/0%
- [ ] HP ratio preserves with floor of 1
- [ ] Stamina ratio preserves with floor of 0
- [ ] Equipment bonuses add to effective attributes

---

## Design Rationale

### Why Five Attributes?
- Covers all major RPG archetypes
- Each attribute has clear primary and secondary uses
- Enables meaningful build variety without analysis paralysis

### Why Separate Base/Effective Attributes?
- Equipment bonuses should be removable
- Corruption penalties should stack separately
- UI can show both base and modified values

### Why Level 1-5 Cap?
- Focus on horizontal progression (equipment, abilities)
- Keeps balance manageable
- Encourages mechanical mastery over level grinding

### Why Ratio Preservation?
- Prevents "full heal on equip" exploits
- Maintains sense of damage taken
- Equipment changes should feel like adjustments, not resets

---

## Changelog

### v1.1.0 (2025-12-23)
- Added Table of Contents
- Fixed corruption multiplier values to match implementation (100%/90%/80%/60%/0%)
- Added BackgroundType to Data Models (v0.3.4c)
- Added line number references for key methods
- Updated UC-3 level-up status to "NOT YET IMPLEMENTED"
- Added Decision Trees for creation and recalculation flows
- Documented HP vs Stamina floor difference (HP ≥1, Stamina ≥0)
- Added detailed test coverage matrix
- Added Validation Checklist
- Added related_specs: SPEC-ADVANCEMENT-001

### v1.0.0 (Initial)
- Initial specification
