---
id: SPEC-ENEMYFAC-001
title: Enemy Factory System
version: 1.1.0
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-TRAIT-001, SPEC-DICE-001, SPEC-ENEMY-001, SPEC-ENVPOP-001, SPEC-ABILITY-001]
---

# SPEC-ENEMYFAC-001: Enemy Factory System

**Version:** 1.1.0
**Status:** Implemented
**Last Updated:** 2025-12-24
**Implementation File:** [RuneAndRust.Engine/Factories/EnemyFactory.cs](../../RuneAndRust.Engine/Factories/EnemyFactory.cs)
**Test File:** [RuneAndRust.Tests/Engine/EnemyFactoryTests.cs](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs)

---

## Table of Contents

1. [Overview](#overview)
2. [Core Behaviors](#core-behaviors)
3. [Restrictions](#restrictions)
4. [Limitations](#limitations)
5. [Use Cases](#use-cases)
6. [Decision Trees](#decision-trees)
7. [Sequence Diagrams](#sequence-diagrams)
8. [Workflows](#workflows)
9. [Cross-System Integration](#cross-system-integration)
10. [Data Models](#data-models)
11. [Configuration](#configuration)
12. [Testing](#testing)
13. [Domain 4 Compliance](#domain-4-compliance)
14. [Future Extensions](#future-extensions)
15. [Error Handling](#error-handling)
16. [Changelog](#changelog)

---

## Overview

The **Enemy Factory System** implements the **Prototype Pattern** for creating Enemy entities from reusable templates. It handles multi-factor stat scaling (threat tier × party level × variance), Elite enemy enhancement via CreatureTraitService, and maintains a registry of 5 built-in enemy templates representing Aethelgard's hostile factions.

### Purpose

- **Template-Based Creation:** Instantiate Enemy entities from EnemyTemplate prototypes
- **Stat Scaling:** Apply threat tier multipliers (Minion 0.6x, Standard 1.0x, Elite 1.5x, Boss 2.5x)
- **Party Level Adaptation:** Scale enemy stats by 10% per party level above 1
- **Variance Injection:** Add ±5% randomness to prevent identical encounters
- **Elite Enhancement:** Apply creature traits to Elite/Boss enemies for increased challenge
- **Template Registry:** Provide centralized enemy definitions with AAM-VOICE compliant lore

### Architecture

EnemyFactory operates as a **stateless factory** with dependency injection:

```
[EnemyFactory] → [EnemyTemplate Registry] → [Enemy Entity]
       │                                           │
       ├──[IDiceService]───────────────────────────┤ (Variance)
       └──[ICreatureTraitService]──────────────────┘ (Elite Enhancement)
```

### Key Responsibilities

1. **Hydration:** Convert EnemyTemplate (immutable prototype) to Enemy (mutable runtime entity)
2. **Scaling:** Calculate final stats based on tier, party level, and variance
3. **Copying:** Deep-copy attributes, tags, and weapon stats from template
4. **Enhancement:** Trigger trait application for Elite/Boss enemies
5. **Registration:** Maintain dictionary of template ID → EnemyTemplate mappings
6. **Fallback:** Return default enemy ("und_draugr_01") if template ID not found

### Design Pattern

**Pattern:** **Prototype + Factory**

```
EnemyTemplate (Prototype) → Clone → Apply Scaling → Apply Variance → Apply Traits → Enemy (Product)
```

**Benefits:**
- Templates are immutable (can be shared across threads)
- Enemies are independent entities (modifying one doesn't affect template)
- New enemies added by adding templates, not factory code
- Scaling formulas centralized in CalculateScaler method

---

## Core Behaviors

> **Note:** All factory methods are `async` to support ability hydration from the repository (v0.2.4a).

### 1. Enemy Creation from Template

**Method Signature:**
```csharp
Task<Enemy> CreateFromTemplateAsync(EnemyTemplate template, int partyLevel = 1)
```

**Primary Flow:**
1. Accept `EnemyTemplate` and optional `partyLevel` parameter
2. Calculate scaling multiplier via `CalculateScaler(tier, partyLevel)`
3. Roll variance (1d11 - 1 → 0-10 → mapped to 0.95-1.05 multiplier)
4. Apply scaling formula: `BaseStat × Scaler × Variance`
5. Enforce minimum stat value of 1 (prevents 0 HP enemies)
6. Create Enemy entity with scaled stats
7. Deep-copy attributes dictionary from template
8. Deep-copy tags list from template
9. Copy weapon stats (damage die, name) and armor soak
10. Set TemplateId, Archetype, and initial HP/Stamina to max values
11. If tier ≥ Elite:
    - Add tier tag (e.g., "Elite", "Boss") to tags list
    - Call `CreatureTraitService.EnhanceEnemy()` to apply traits
12. Hydrate abilities from `AbilityNames` via `IActiveAbilityRepository` (v0.2.4a)
13. Return fully-hydrated Enemy entity

**Implementation:** [EnemyFactory.cs:61-124](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L61-L124)

**Logging:**
- **Information:** Enemy creation start (template ID, tier, party level)
- **Debug:** Scaling calculation breakdown (base × scaler × variance = final)
- **Trace:** Enemy properties (HP, Stamina, Archetype, Tags)
- **Information:** Elite enhancement completion (trait count, trait names)

---

### 2. Stat Scaling Formula

**Formula:**
```
FinalStat = BaseStat × TierMultiplier × LevelMultiplier × VarianceMultiplier
```

**Component Breakdown:**

**Tier Multiplier:**
```
Minion:   0.6x  (weak fodder)
Standard: 1.0x  (baseline)
Elite:    1.5x  (enhanced threat)
Boss:     2.5x  (fixed, no level scaling)
```

**Level Multiplier:**
```
1.0 + ((PartyLevel - 1) × 0.1)

Examples:
  Party Level 1 → 1.0x
  Party Level 3 → 1.2x
  Party Level 5 → 1.4x
  Party Level 10 → 1.9x
```

**Variance Multiplier:**
```
Roll 1d11 - 1 → 0-10
Map to 0.95-1.05 range:
  0 → 0.95x (minimum)
  5 → 1.00x (neutral)
  10 → 1.05x (maximum)
```

**Implementation:** [EnemyFactory.cs:142-155](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L142-L155)

**Example Calculation:**
```
Template: Rusted Draugr (Standard, 60 base HP)
Party Level: 3
Variance Roll: 7 (→ 1.02x)

Calculation:
  TierMultiplier = 1.0 (Standard)
  LevelMultiplier = 1.0 + ((3 - 1) × 0.1) = 1.2
  VarianceMultiplier = 0.95 + (7 × 0.01) = 1.02
  FinalHP = 60 × 1.0 × 1.2 × 1.02 = 73.44 → 73

Result: Enemy has 73/73 HP
```

---

### 3. Variance Application

**Purpose:** Prevent identical encounters by introducing ±5% randomness.

**Dice Roll:**
- Service: `IDiceService.RollSingle(11, "HP Variance")`
- Range: 0-10 (inclusive)
- Mapping: Linear interpolation to 0.95-1.05

**Code:**
```csharp
var varianceRoll = _dice.RollSingle(11, "HP Variance"); // 0-10
var variance = VarianceMin + (varianceRoll * 0.01f);    // 0.95-1.05
```

**Implementation:** [EnemyFactory.cs:66-67](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L66-L67)

**Effect:**
- Same template with same party level produces different stat values
- Variance applies to both HP and Stamina equally (same roll used)
- Does not apply to weapon damage die or armor soak (fixed values)

**Statistical Distribution:**
- Uniform distribution (each variance value equally likely)
- Expected average: 1.00x (neutral variance)
- Standard deviation: ~3.16% of base stat

---

### 4. Deep Property Copying

**Attributes Dictionary:**
```csharp
Attributes = new Dictionary<CharacterAttribute, int>(template.Attributes)
```
- Creates new dictionary instance (not reference copy)
- Modifying enemy attributes does not affect template
- All 5 attributes copied: Sturdiness, Might, Wits, Will, Finesse

**Tags List:**
```csharp
Tags = new List<string>(template.Tags)
```
- Creates new list instance (not reference copy)
- Allows adding tier tags (e.g., "Elite") without mutating template
- Modifying enemy tags does not affect template

**Implementation:** [EnemyFactory.cs:87, 91](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L87)

**Why Deep Copy:**
- Templates are shared across all enemy instances
- Enemies modify their own state during combat (e.g., HP loss, status effects)
- Must not corrupt template for future enemy creation

---

### 5. Elite Enemy Enhancement

**Trigger Condition:**
```csharp
if (template.Tier >= ThreatTier.Elite)
```
- Applies to Elite and Boss tiers
- Does not apply to Minion or Standard tiers

**Enhancement Process:**
1. Add tier tag to enemy.Tags list (e.g., "Elite", "Boss")
2. Call `CreatureTraitService.EnhanceEnemy(enemy)`
3. CreatureTraitService selects 1-3 traits based on tier
4. Traits applied as passive effects or active abilities
5. Log trait names for debugging

**Implementation:** [EnemyFactory.cs:99-110](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L99-L110)

**Example:**
```
Template: Ash-Vargr (Elite tier, 45 base HP)
Party Level: 2
After Scaling: 60 HP

Elite Enhancement:
  1. Add "Elite" tag to Tags list
  2. CreatureTraitService selects 2 traits:
     - "Regenerating" (+5 HP per turn)
     - "Venomous" (apply Poison on hit)
  3. Enemy.ActiveTraits = ["Regenerating", "Venomous"]
  4. Log: "Elite enemy enhanced: Ash-Vargr with 2 traits: [Regenerating, Venomous]"
```

**Trait Count by Tier:**
- Elite: 1-2 traits (moderate enhancement)
- Boss: 2-3 traits (heavy enhancement)

---

### 6. Create by Template ID

**Purpose:** Convenience method for spawning enemies from string IDs.

**Method Signature:**
```csharp
Task<Enemy> CreateByIdAsync(string templateId, int partyLevel = 1)
```

**Flow:**
1. Accept `templateId` string (e.g., "und_draugr_01")
2. Lookup template in registry via `_templates.TryGetValue(templateId, out var template)`
3. If not found:
   - Log warning with requested ID and fallback ID
   - Use fallback template ("und_draugr_01")
4. Call `CreateFromTemplateAsync(template, partyLevel)`
5. Return created enemy

**Implementation:** [EnemyFactory.cs:127-138](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L127-L138)

**Fallback Behavior:**
- Ensures method always returns valid enemy (never null)
- Prevents crashes from typos in spawn tables
- Fallback enemy is "Rusted Draugr" (balanced Standard-tier enemy)

---

### 7. Ability Hydration (v0.2.4a)

**Purpose:** Load active abilities from repository based on template `AbilityNames`.

**Method Signature:**
```csharp
private async Task HydrateAbilitiesAsync(Enemy enemy, EnemyTemplate template)
```

**Flow:**
1. Check if template has `AbilityNames` (list of ability name strings)
2. If null or empty, skip (enemy.Abilities remains empty list)
3. For each ability name:
   - Query `IActiveAbilityRepository.GetByNameAsync(abilityName)`
   - If found: add to `enemy.Abilities`
   - If not found: log warning, continue to next ability
4. Graceful degradation: missing abilities don't prevent enemy creation

**Implementation:** [EnemyFactory.cs:145-171](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L145-L171)

**EnemyTemplate AbilityNames Field:**
```csharp
public record EnemyTemplate(
    // ... other fields ...
    List<string>? AbilityNames = null  // Optional ability references
);
```

**Example:**
```csharp
new EnemyTemplate(
    Id: "bst_vargr_01",
    Name: "Ash-Vargr",
    // ... other fields ...
    AbilityNames: new List<string> { "Pounce", "Rending Claws" }
)
```

**Logging:**
- **Warning:** Ability not found in repository (logs ability name, continues)
- **Debug:** Ability successfully hydrated (logs ability name, enemy name)

---

### 8. Template Registry Management

**Initialization:**
- Templates loaded in constructor via `InitializeTemplates()` method
- Returns `Dictionary<string, EnemyTemplate>` with 5 built-in templates
- Templates are immutable once created (readonly dictionary)

**Implementation:** [EnemyFactory.cs:162-292](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L162-L292)

**Query Methods:**

**GetTemplateIds():**
```csharp
public IReadOnlyList<string> GetTemplateIds() => _templates.Keys.ToList();
```
- Returns list of all template IDs
- Used by spawn systems to select random enemies

**GetTemplate(templateId):**
```csharp
public EnemyTemplate? GetTemplate(string templateId) =>
    _templates.TryGetValue(templateId, out var template) ? template : null;
```
- Returns template or null if not found
- Used for inspecting template stats before creation

**Built-in Templates (5 total):**
1. **und_draugr_01** - Rusted Draugr (Undying, DPS, Standard)
2. **und_haug_01** - Haugbui Laborer (Undying, Tank, Standard)
3. **mec_serv_01** - Utility Servitor (Mechanical, Swarm, Minion)
4. **bst_vargr_01** - Ash-Vargr (Beast, GlassCannon, Standard)
5. **hum_raider_01** - Rust-Clan Scav (Humanoid, Support, Standard)

---

## Restrictions

### Functional Restrictions

1. **No Runtime Template Creation:**
   - Templates are hard-coded in `InitializeTemplates()` method
   - Cannot add templates at runtime without code modification
   - Future: Load templates from JSON/database

2. **Immutable Template Registry:**
   - `_templates` dictionary is readonly after initialization
   - Cannot modify or remove templates after factory construction
   - Must create new factory instance to change templates

3. **Fixed Variance Range:**
   - Variance is always ±5% (0.95x - 1.05x)
   - Cannot configure different variance per template
   - Rationale: Consistent game balance across all enemies

4. **Single Dice Roll for Variance:**
   - Same variance multiplier applied to both HP and Stamina
   - HP and Stamina scale proportionally
   - Prevents edge cases like high HP / low Stamina

5. **Boss Tier Ignores Level Scaling:**
   - Boss enemies always use fixed 2.5x multiplier
   - Party level does not affect Boss stats
   - Rationale: Bosses are designed for specific encounter difficulty

### Scaling Restrictions

1. **Minimum Stat Value:**
   - All scaled stats clamped to minimum 1 via `Math.Max(1, scaledValue)`
   - Prevents 0 HP enemies (would die instantly)
   - Prevents 0 Stamina enemies (cannot use abilities)

2. **Integer Rounding:**
   - Scaled stats cast to `int` (truncates decimal)
   - 73.8 HP becomes 73 HP (rounds down)
   - May cause slight stat reduction on low base values

3. **No Negative Scaling:**
   - No debuff tier (enemies always ≥ 0.6x stats)
   - Minion tier is minimum scaling factor
   - Rationale: Even weakest enemies must pose some threat

---

## Limitations

### Design Limitations

1. **Static Template Count:**
   - Only 5 built-in templates in v1.0.0
   - Small variety limits encounter diversity
   - Future: Expand to 20-30 templates across all biomes

2. **No Template Inheritance:**
   - Each template defines all stats independently
   - No shared base templates or trait mixing
   - Requires manual duplication of common patterns

3. **Fixed Attribute Set:**
   - All enemies have exactly 5 attributes (Sturdiness, Might, Wits, Will, Finesse)
   - Cannot create enemies with subset or superset of attributes
   - Matches player character model for consistency

4. **No Multi-Tier Encounters:**
   - Each enemy is single tier (cannot be Standard *and* Elite)
   - Encounter variety limited to mixing different template tiers
   - Rationale: Simplifies AI behavior logic

### Scaling Limitations

1. **Linear Level Scaling:**
   - 10% per level is constant (not exponential)
   - May become underpowered at very high levels (level 20+ = 2.9x)
   - Future: Cap level scaling at level 10 or use exponential curve

2. **No Stat-Specific Scaling:**
   - Same multiplier applies to HP and Stamina
   - Cannot scale HP faster than Stamina or vice versa
   - Rationale: Maintains template-defined stat ratios

3. **Variance Applies Before Rounding:**
   - Low base stats lose precision to integer rounding
   - 10 base HP × 0.6 (Minion) × 0.95 (variance) = 5.7 → 5 HP
   - 5% variance less impactful on small numbers

### Elite Enhancement Limitations

1. **Trait Selection Non-Deterministic:**
   - CreatureTraitService randomly selects traits
   - Same template + same level may produce different trait combinations
   - Cannot guarantee specific trait appears

2. **No Trait Exclusion Rules:**
   - Factory does not specify which traits are allowed/forbidden
   - CreatureTraitService has full control over trait selection
   - May result in thematically inconsistent trait combos

---

## Use Cases

### UC-ENEMYFAC-01: Create Standard Enemy (Party Level 1)

**Scenario:** Player at level 1 encounters Rusted Draugr in dungeon.

**Preconditions:**
- Party level = 1
- Dice variance roll = 5 (neutral 1.00x)

**Action:**
```csharp
var enemy = enemyFactory.CreateById("und_draugr_01", partyLevel: 1);
```

**Expected Behavior:**
1. Factory looks up "und_draugr_01" in template registry
2. Template found: Rusted Draugr (Standard tier, 60 base HP, 40 base Stamina)
3. Calculate scaler:
   - Tier = Standard → 1.0x
   - Level = 1 → 1.0x
   - Total scaler = 1.0x
4. Roll variance: 5 → 1.00x
5. Calculate final stats:
   - HP: 60 × 1.0 × 1.00 = 60
   - Stamina: 40 × 1.0 × 1.00 = 40
6. Create Enemy entity:
   - Name: "Rusted Draugr"
   - MaxHp: 60, CurrentHp: 60
   - MaxStamina: 40, CurrentStamina: 40
   - WeaponDamageDie: 6
   - WeaponName: "Rusted Blade"
   - ArmorSoak: 2
   - Attributes: {Sturdiness: 5, Might: 6, Wits: 3, Will: 3, Finesse: 4}
   - Tags: ["Undying", "IronHeart", "Construct"]
   - Archetype: DPS
7. Tier is Standard (< Elite), skip trait enhancement
8. Return enemy

**Result:**
```csharp
Assert.Equal("Rusted Draugr", enemy.Name);
Assert.Equal(60, enemy.MaxHp);
Assert.Equal(60, enemy.CurrentHp);
Assert.Equal(40, enemy.MaxStamina);
Assert.Equal(6, enemy.WeaponDamageDie);
Assert.Equal(EnemyArchetype.DPS, enemy.Archetype);
Assert.Empty(enemy.ActiveTraits); // No traits for Standard tier
```

**Test Reference:** [EnemyFactoryTests.cs:344-353](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L344-L353)

---

### UC-ENEMYFAC-02: Create Scaled Enemy (Party Level 5)

**Scenario:** Player at level 5 encounters Rusted Draugr, stats scaled up.

**Preconditions:**
- Party level = 5
- Dice variance roll = 5 (neutral 1.00x)

**Action:**
```csharp
var enemy = enemyFactory.CreateById("und_draugr_01", partyLevel: 5);
```

**Expected Behavior:**
1. Template: Rusted Draugr (60 base HP, 40 base Stamina)
2. Calculate scaler:
   - Tier = Standard → 1.0x
   - Level = 5 → 1.0 + ((5 - 1) × 0.1) = 1.4x
   - Total scaler = 1.0 × 1.4 = 1.4x
3. Roll variance: 5 → 1.00x
4. Calculate final stats:
   - HP: 60 × 1.4 × 1.00 = 84
   - Stamina: 40 × 1.4 × 1.00 = 56
5. Create Enemy entity with scaled stats
6. Return enemy

**Result:**
```csharp
Assert.Equal(84, enemy.MaxHp);
Assert.Equal(56, enemy.MaxStamina);
```

**Test Reference:** [EnemyFactoryTests.cs:310-321](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L310-L321)

---

### UC-ENEMYFAC-03: Create Elite Enemy with Traits

**Scenario:** Boss fight spawns Elite Ash-Vargr with random traits.

**Preconditions:**
- Party level = 3
- Dice variance roll = 7 (1.02x)
- Template tier = Elite (triggers trait enhancement)

**Action:**
```csharp
// Create Elite-tier template for Ash-Vargr
var eliteTemplate = new EnemyTemplate(
    Id: "bst_vargr_elite",
    Name: "Alpha Ash-Vargr",
    Tier: ThreatTier.Elite,
    BaseHp: 45,
    BaseStamina: 50,
    // ... other stats
);

var enemy = enemyFactory.CreateFromTemplate(eliteTemplate, partyLevel: 3);
```

**Expected Behavior:**
1. Calculate scaler:
   - Tier = Elite → 1.5x
   - Level = 3 → 1.2x
   - Total scaler = 1.5 × 1.2 = 1.8x
2. Roll variance: 7 → 0.95 + (7 × 0.01) = 1.02x
3. Calculate final stats:
   - HP: 45 × 1.8 × 1.02 = 82.62 → 82
   - Stamina: 50 × 1.8 × 1.02 = 91.8 → 91
4. Create Enemy entity
5. Detect tier ≥ Elite → add "Elite" tag to Tags list
6. Call `CreatureTraitService.EnhanceEnemy(enemy)`
7. CreatureTraitService selects 2 traits (random):
   - "Regenerating" (passive: +5 HP per turn)
   - "Pack Tactics" (bonus damage when ally adjacent)
8. Enemy.ActiveTraits = ["Regenerating", "Pack Tactics"]
9. Log: "Elite enemy enhanced: Alpha Ash-Vargr with 2 traits: [Regenerating, Pack Tactics]"
10. Return enemy

**Result:**
```csharp
Assert.Equal(82, enemy.MaxHp);
Assert.Equal(91, enemy.MaxStamina);
Assert.Contains("Elite", enemy.Tags);
Assert.Equal(2, enemy.ActiveTraits.Count);
Assert.Contains("Regenerating", enemy.ActiveTraits);
Assert.Contains("Pack Tactics", enemy.ActiveTraits);
```

**Test Reference:** [EnemyFactoryTests.cs:264-275](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L264-L275)

---

### UC-ENEMYFAC-04: Create Minion Swarm

**Scenario:** Encounter spawns 3 Utility Servitors (Minion tier, low HP).

**Preconditions:**
- Party level = 2
- Dice variance roll = 3 (0.98x)

**Action:**
```csharp
var servitor1 = enemyFactory.CreateById("mec_serv_01", partyLevel: 2);
var servitor2 = enemyFactory.CreateById("mec_serv_01", partyLevel: 2);
var servitor3 = enemyFactory.CreateById("mec_serv_01", partyLevel: 2);
```

**Expected Behavior (per servitor):**
1. Template: Utility Servitor (Minion tier, 25 base HP, 20 base Stamina)
2. Calculate scaler:
   - Tier = Minion → 0.6x
   - Level = 2 → 1.0 + ((2 - 1) × 0.1) = 1.1x
   - Total scaler = 0.6 × 1.1 = 0.66x
3. Roll variance: 3 → 0.95 + (3 × 0.01) = 0.98x
4. Calculate final stats:
   - HP: 25 × 0.66 × 0.98 = 16.17 → 16
   - Stamina: 20 × 0.66 × 0.98 = 12.936 → 12
5. Create 3 independent Enemy entities (each with unique GUID)

**Result:**
```csharp
Assert.Equal(16, servitor1.MaxHp);
Assert.Equal(16, servitor2.MaxHp);
Assert.Equal(16, servitor3.MaxHp);
Assert.NotEqual(servitor1.Id, servitor2.Id); // Each has unique ID
Assert.NotEqual(servitor2.Id, servitor3.Id);
```

**Tactical Impact:**
- 3 Minions with 16 HP each = 48 total HP
- Comparable to 1 Standard enemy with ~50 HP
- Minions use Swarm archetype AI (coordinate attacks)

**Test Reference:** [EnemyFactoryTests.cs:236-247](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L236-L247)

---

### UC-ENEMYFAC-05: Create Boss with Fixed Scaling

**Scenario:** Final dungeon boss spawns, stats unaffected by party level.

**Preconditions:**
- Party level = 7 (should normally scale enemies to 1.6x)
- Dice variance roll = 8 (1.03x)
- Template tier = Boss (fixed 2.5x, ignores level scaling)

**Action:**
```csharp
var bossTemplate = new EnemyTemplate(
    Id: "boss_overseer",
    Name: "Corrupted Overseer",
    Tier: ThreatTier.Boss,
    BaseHp: 200,
    BaseStamina: 150,
    // ... other stats
);

var boss1 = enemyFactory.CreateFromTemplate(bossTemplate, partyLevel: 1);
var boss7 = enemyFactory.CreateFromTemplate(bossTemplate, partyLevel: 7);
```

**Expected Behavior:**
1. Calculate scaler (both level 1 and level 7):
   - Tier = Boss → 2.5x (fixed)
   - Level = ignored (Boss tier has no level scaling)
   - Total scaler = 2.5x
2. Roll variance: 8 → 1.03x
3. Calculate final stats (same for both):
   - HP: 200 × 2.5 × 1.03 = 515
   - Stamina: 150 × 2.5 × 1.03 = 386.25 → 386
4. Add "Boss" tag to Tags list
5. CreatureTraitService selects 3 traits (maximum for Boss tier)
6. Return boss enemy

**Result:**
```csharp
Assert.Equal(515, boss1.MaxHp); // Party level 1
Assert.Equal(515, boss7.MaxHp); // Party level 7 (same HP)
Assert.Contains("Boss", boss1.Tags);
Assert.Equal(3, boss1.ActiveTraits.Count); // Maximum traits
```

**Design Rationale:**
- Boss encounters balanced for specific narrative moments
- Party level scaling would make bosses too easy (low level) or impossible (high level)
- Fixed stats allow precise tuning of boss difficulty

**Test Reference:** [EnemyFactoryTests.cs:324-337](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L324-L337)

---

### UC-ENEMYFAC-06: Fallback Template on Invalid ID

**Scenario:** Spawn system requests enemy with typo in template ID.

**Preconditions:**
- Requested template ID: "und_draugr_02" (does not exist)
- Fallback template ID: "und_draugr_01" (exists)

**Action:**
```csharp
var enemy = enemyFactory.CreateById("und_draugr_02", partyLevel: 1);
```

**Expected Behavior:**
1. Factory attempts lookup: `_templates.TryGetValue("und_draugr_02", out var template)`
2. Lookup returns false (template not found)
3. Log warning: "Template not found: und_draugr_02, using fallback template und_draugr_01"
4. Use fallback template: `template = _templates["und_draugr_01"]`
5. Create enemy from fallback template
6. Return enemy (never null)

**Result:**
```csharp
Assert.NotNull(enemy);
Assert.Equal("Rusted Draugr", enemy.Name); // Fallback template name
Assert.Equal("und_draugr_01", enemy.TemplateId); // Fallback template ID
```

**Benefit:**
- Prevents crashes from typos in spawn tables or config files
- Allows graceful degradation (encounter still occurs, just different enemy)
- Logs warning for debugging (developer can fix typo later)

**Test Reference:** [EnemyFactoryTests.cs:356-365](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L356-L365)

---

### UC-ENEMYFAC-07: Variance Creates Different Enemies

**Scenario:** Same template creates two enemies with different stats due to variance.

**Preconditions:**
- Template: Rusted Draugr (60 base HP)
- Party level: 1 (no level scaling)
- First variance roll: 2 (0.97x)
- Second variance roll: 8 (1.03x)

**Action:**
```csharp
mockDice.SetupSequence(d => d.RollSingle(11, "HP Variance"))
    .Returns(2)  // First call
    .Returns(8); // Second call

var enemy1 = enemyFactory.CreateById("und_draugr_01", partyLevel: 1);
var enemy2 = enemyFactory.CreateById("und_draugr_01", partyLevel: 1);
```

**Expected Behavior:**

**Enemy 1:**
- Scaler: 1.0 (Standard tier, level 1)
- Variance: 0.97x
- HP: 60 × 1.0 × 0.97 = 58.2 → 58

**Enemy 2:**
- Scaler: 1.0 (Standard tier, level 1)
- Variance: 1.03x
- HP: 60 × 1.0 × 1.03 = 61.8 → 61

**Result:**
```csharp
Assert.Equal(58, enemy1.MaxHp);
Assert.Equal(61, enemy2.MaxHp);
Assert.NotEqual(enemy1.MaxHp, enemy2.MaxHp); // Different due to variance
```

**Gameplay Impact:**
- Prevents predictable encounters (same enemy always has same stats)
- Small variance (±5%) keeps balance consistent
- Adds tactical uncertainty (players can't memorize exact HP values)

**Test Reference:** [EnemyFactoryTests.cs:160-203](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L160-L203)

---

## Decision Trees

### Decision Tree 1: Stat Scaling Calculation

```
CreateFromTemplate(template, partyLevel)
│
├─ Calculate Tier Multiplier
│  │
│  ├─ [Tier == Minion] → 0.6x
│  ├─ [Tier == Standard] → 1.0x
│  ├─ [Tier == Elite] → 1.5x
│  └─ [Tier == Boss] → 2.5x (skip to Apply Multipliers)
│
├─ Calculate Level Multiplier (if not Boss)
│  │
│  └─ 1.0 + ((partyLevel - 1) × 0.1)
│     └─ Examples:
│        Level 1 → 1.0x
│        Level 3 → 1.2x
│        Level 5 → 1.4x
│
├─ Calculate Combined Scaler
│  │
│  └─ If Boss: Scaler = 2.5 (fixed)
│  └─ Otherwise: Scaler = TierMultiplier × LevelMultiplier
│
├─ Roll Variance
│  │
│  ├─ Call IDiceService.RollSingle(11, "HP Variance")
│  ├─ Result: 0-10 (inclusive)
│  └─ Map to variance: 0.95 + (roll × 0.01) → 0.95x to 1.05x
│
├─ Apply Multipliers
│  │
│  ├─ FinalHP = Math.Max(1, (int)(BaseHP × Scaler × Variance))
│  └─ FinalStamina = Math.Max(1, (int)(BaseStamina × Scaler × Variance))
│
└─ Return Scaled Stats
```

**Key Decision Points:**
1. **Boss Tier Check:** Boss tier bypasses level scaling
2. **Minimum Clamping:** All stats clamped to minimum 1
3. **Integer Rounding:** Decimal results truncated to int

---

### Decision Tree 2: Elite Enhancement

```
After Enemy Creation
│
├─ Check Tier ≥ Elite?
│  │
│  ├─ [NO] (Minion or Standard)
│  │  └─ Skip enhancement
│  │     └─ Return enemy (no traits)
│  │
│  └─ [YES] (Elite or Boss)
│     │
│     ├─ Add Tier Tag to Tags List
│     │  │
│     │  ├─ If Elite → Add "Elite"
│     │  └─ If Boss → Add "Boss"
│     │
│     ├─ Call CreatureTraitService.EnhanceEnemy(enemy)
│     │  │
│     │  └─ CreatureTraitService Logic:
│     │     │
│     │     ├─ Detect tier from Tags
│     │     │  │
│     │     │  ├─ "Elite" tag → Select 1-2 traits
│     │     │  └─ "Boss" tag → Select 2-3 traits
│     │     │
│     │     ├─ Random trait selection from pool
│     │     ├─ Apply traits to enemy.ActiveTraits
│     │     └─ Return (enemy modified in place)
│     │
│     ├─ Log Enhancement
│     │  │
│     │  └─ "Elite enemy enhanced: {Name} with {Count} traits: [{Traits}]"
│     │
│     └─ Return enhanced enemy
```

**Key Decision Points:**
1. **Tier Threshold:** Only Elite/Boss enhanced (not Minion/Standard)
2. **Tag Detection:** CreatureTraitService reads tier from Tags list
3. **Trait Count:** Tier determines min/max trait count

---

### Decision Tree 3: Template Lookup with Fallback

```
CreateById(templateId, partyLevel)
│
├─ Lookup Template in Registry
│  │
│  └─ _templates.TryGetValue(templateId, out var template)
│     │
│     ├─ [Found]
│     │  └─ Use retrieved template
│     │
│     └─ [Not Found]
│        │
│        ├─ Log Warning
│        │  └─ "Template not found: {Id}, using fallback template {Fallback}"
│        │
│        └─ Use Fallback Template
│           └─ template = _templates["und_draugr_01"]
│
├─ Call CreateFromTemplate(template, partyLevel)
│  └─ (Follow Stat Scaling Decision Tree)
│
└─ Return Created Enemy
```

**Key Decision Points:**
1. **Template Existence Check:** Determines whether to use requested or fallback template
2. **Fallback Guarantee:** Always returns valid enemy (never null)
3. **Warning Log:** Alerts developer to configuration error

---

### Decision Tree 4: Property Copying

```
Create Enemy Entity from Template
│
├─ Copy Primitive Values
│  │
│  ├─ Name = template.Name
│  ├─ WeaponDamageDie = template.WeaponDamageDie
│  ├─ WeaponName = template.WeaponName
│  ├─ ArmorSoak = template.BaseSoak
│  ├─ TemplateId = template.Id
│  └─ Archetype = template.Archetype
│
├─ Deep Copy Attributes Dictionary
│  │
│  └─ new Dictionary<CharacterAttribute, int>(template.Attributes)
│     │
│     └─ Result: Independent copy
│        └─ Modifying enemy.Attributes does not affect template
│
├─ Deep Copy Tags List
│  │
│  └─ new List<string>(template.Tags)
│     │
│     └─ Result: Independent copy
│        └─ Modifying enemy.Tags does not affect template
│
├─ Initialize Runtime State
│  │
│  ├─ Id = Guid.NewGuid() (unique ID)
│  ├─ MaxHp = finalHp (from scaling calculation)
│  ├─ CurrentHp = finalHp (full HP at spawn)
│  ├─ MaxStamina = finalStamina (from scaling calculation)
│  └─ CurrentStamina = finalStamina (full Stamina at spawn)
│
└─ Return Enemy Entity
```

**Key Decision Points:**
1. **Deep Copy vs Reference:** Attributes and Tags are deep-copied, primitives are value-copied
2. **Unique ID Generation:** Each enemy gets new GUID (not copied from template)
3. **Full Resource Initialization:** HP and Stamina start at max values

---

## Sequence Diagrams

### Sequence Diagram 1: Standard Enemy Creation

```
Caller        EnemyFactory    DiceService    CreatureTraitService
  │                 │               │                  │
  │ CreateById("und_draugr_01", 3) │                  │
  ├────────────────>│               │                  │
  │                 │ Lookup template in registry     │
  │                 │ "und_draugr_01" → found          │
  │                 │               │                  │
  │                 │ CalculateScaler(Standard, 3)     │
  │                 │ → 1.0 × 1.2 = 1.2x               │
  │                 │               │                  │
  │                 │ RollSingle(11, "HP Variance")    │
  │                 ├──────────────>│                  │
  │                 │               │ Roll dice: 7     │
  │                 │        7      │                  │
  │                 │<──────────────┤                  │
  │                 │               │                  │
  │                 │ Calculate variance: 0.95 + (7 × 0.01) = 1.02x│
  │                 │               │                  │
  │                 │ Apply scaling:│                  │
  │                 │ HP = 60 × 1.2 × 1.02 = 73.44 → 73│
  │                 │ Stamina = 40 × 1.2 × 1.02 = 48.96 → 48│
  │                 │               │                  │
  │                 │ Create Enemy entity:             │
  │                 │ - MaxHp = 73, CurrentHp = 73     │
  │                 │ - MaxStamina = 48, CurrentStamina = 48│
  │                 │ - Copy Attributes (deep)         │
  │                 │ - Copy Tags (deep)               │
  │                 │               │                  │
  │                 │ Check tier: Standard (< Elite)   │
  │                 │ Skip trait enhancement           │
  │                 │               │                  │
  │        Enemy    │               │                  │
  │<────────────────┤               │                  │
```

**Duration:** ~2-5ms (typical)

**Key Steps:**
1. Template lookup (dictionary access)
2. Scaling calculation (arithmetic)
3. Variance dice roll (IDiceService call)
4. Enemy entity creation (new object + property copy)
5. Skip Elite enhancement (tier check)

---

### Sequence Diagram 2: Elite Enemy Creation with Trait Enhancement

```
Caller        EnemyFactory    DiceService    CreatureTraitService
  │                 │               │                  │
  │ CreateFromTemplate(eliteTemplate, 2)│              │
  ├────────────────>│               │                  │
  │                 │ CalculateScaler(Elite, 2)        │
  │                 │ → 1.5 × 1.1 = 1.65x              │
  │                 │               │                  │
  │                 │ RollSingle(11, "HP Variance")    │
  │                 ├──────────────>│                  │
  │                 │        5      │                  │
  │                 │<──────────────┤                  │
  │                 │               │                  │
  │                 │ Calculate variance: 1.00x        │
  │                 │               │                  │
  │                 │ Apply scaling:│                  │
  │                 │ HP = 45 × 1.65 × 1.00 = 74.25 → 74│
  │                 │               │                  │
  │                 │ Create Enemy entity              │
  │                 │               │                  │
  │                 │ Check tier: Elite (≥ Elite)      │
  │                 │ Add "Elite" tag to Tags          │
  │                 │               │                  │
  │                 │ EnhanceEnemy(enemy)              │
  │                 ├──────────────────────────────────>│
  │                 │               │                  │
  │                 │               │ Read Tags → detect "Elite"│
  │                 │               │ Select 2 traits (random):│
  │                 │               │ - "Regenerating"│
  │                 │               │ - "Venomous"    │
  │                 │               │                  │
  │                 │               │ Apply traits to enemy.ActiveTraits│
  │                 │               │                  │
  │                 │        (void) │                  │
  │                 │<──────────────────────────────────┤
  │                 │               │                  │
  │                 │ Log: "Elite enemy enhanced: Alpha Ash-Vargr with 2 traits: [Regenerating, Venomous]"│
  │                 │               │                  │
  │        Enemy    │               │                  │
  │<────────────────┤               │                  │
```

**Duration:** ~3-8ms (includes trait selection)

**Key Steps:**
1. Elite scaling (1.5x base multiplier)
2. Tier detection (≥ Elite)
3. Add tier tag
4. Call CreatureTraitService (external service)
5. Trait selection and application
6. Log enhancement result

---

### Sequence Diagram 3: Fallback Template Lookup

```
Caller        EnemyFactory    TemplateRegistry
  │                 │                 │
  │ CreateById("invalid_id", 1)      │
  ├────────────────>│                 │
  │                 │ TryGetValue("invalid_id")│
  │                 ├────────────────>│
  │                 │                 │ Lookup fails
  │                 │      false      │
  │                 │<────────────────┤
  │                 │                 │
  │                 │ Log Warning:    │
  │                 │ "Template not found: invalid_id, using fallback template und_draugr_01"│
  │                 │                 │
  │                 │ Get fallback template│
  │                 │ "und_draugr_01" │
  │                 ├────────────────>│
  │                 │  RustedDraugr   │
  │                 │<────────────────┤
  │                 │                 │
  │                 │ CreateFromTemplate(fallbackTemplate, 1)│
  │                 │ (Standard creation flow)│
  │                 │                 │
  │        Enemy    │                 │
  │<────────────────┤                 │
  │                 │                 │
  │ Assert: enemy.TemplateId == "und_draugr_01"│
```

**Duration:** ~2-5ms (same as normal creation)

**Key Steps:**
1. Attempt lookup with invalid ID
2. Lookup fails (TryGetValue returns false)
3. Log warning for debugging
4. Retrieve fallback template
5. Create enemy from fallback

---

## Workflows

### Workflow 1: Enemy Creation Checklist

**Purpose:** Ensure all steps are completed when creating enemy from template.

**Steps:**

1. **Template Acquisition**
   - [ ] Receive EnemyTemplate and partyLevel parameters
   - [ ] Validate template is not null

2. **Scaling Calculation**
   - [ ] Determine tier multiplier via switch expression
   - [ ] Calculate level multiplier: `1.0 + ((partyLevel - 1) × 0.1)`
   - [ ] Combine: `scaler = tierMultiplier × levelMultiplier`
   - [ ] Special case: If Boss tier, scaler = 2.5 (fixed)

3. **Variance Roll**
   - [ ] Call `IDiceService.RollSingle(11, "HP Variance")`
   - [ ] Map roll (0-10) to variance (0.95-1.05)
   - [ ] Formula: `variance = 0.95 + (roll × 0.01)`

4. **Stat Calculation**
   - [ ] Calculate HP: `Math.Max(1, (int)(template.BaseHp × scaler × variance))`
   - [ ] Calculate Stamina: `Math.Max(1, (int)(template.BaseStamina × scaler × variance))`
   - [ ] Verify both stats ≥ 1

5. **Log Scaling Breakdown**
   - [ ] Log Debug: "Scaling: Base HP {Base} x Scaler {Scaler:F2} x Variance {Variance:F2} = {Final}"

6. **Entity Creation**
   - [ ] Create new Enemy entity with `Id = Guid.NewGuid()`
   - [ ] Copy primitive properties:
     - [ ] Name, TemplateId, Archetype
     - [ ] WeaponDamageDie, WeaponName, ArmorSoak
   - [ ] Set scaled stats:
     - [ ] MaxHp = finalHp, CurrentHp = finalHp
     - [ ] MaxStamina = finalStamina, CurrentStamina = finalStamina

7. **Deep Copy Collections**
   - [ ] Copy Attributes dictionary: `new Dictionary<CharacterAttribute, int>(template.Attributes)`
   - [ ] Copy Tags list: `new List<string>(template.Tags)`

8. **Log Entity Properties**
   - [ ] Log Trace: "Created {Name}: HP={Hp}, Stamina={Stam}, Archetype={Archetype}, Tags=[{Tags}]"

9. **Elite Enhancement Check**
   - [ ] Check `template.Tier >= ThreatTier.Elite`
   - **If true:**
     - [ ] Add tier tag to enemy.Tags (e.g., "Elite", "Boss")
     - [ ] Log Debug: "Added tier tag {Tier} to {Name}"
     - [ ] Call `CreatureTraitService.EnhanceEnemy(enemy)`
     - [ ] Log Information: "Elite enemy enhanced: {Name} with {TraitCount} traits: [{Traits}]"
   - **If false:**
     - [ ] Skip enhancement

10. **Return Enemy**
    - [ ] Return fully-hydrated Enemy entity

---

### Workflow 2: Template Registry Initialization Checklist

**Purpose:** Document how built-in templates are loaded at factory construction.

**Steps:**

1. **Constructor Invocation**
   - [ ] EnemyFactory constructor called with logger, dice service, trait service
   - [ ] Store injected dependencies

2. **Template Initialization**
   - [ ] Call `InitializeTemplates()` method
   - [ ] Receive `Dictionary<string, EnemyTemplate>`

3. **Template Definitions**
   - [ ] Define "und_draugr_01" (Rusted Draugr)
   - [ ] Define "und_haug_01" (Haugbui Laborer)
   - [ ] Define "mec_serv_01" (Utility Servitor)
   - [ ] Define "bst_vargr_01" (Ash-Vargr)
   - [ ] Define "hum_raider_01" (Rust-Clan Scav)

4. **Template Validation**
   - [ ] Verify all templates have unique IDs
   - [ ] Verify all templates have 5 attributes
   - [ ] Verify all templates have non-empty names
   - [ ] Verify all templates have valid tiers and archetypes

5. **Store Registry**
   - [ ] Assign dictionary to `_templates` field
   - [ ] Dictionary is readonly after initialization

6. **Log Initialization**
   - [ ] Log Information: "EnemyFactory initialized with {Count} templates"

---

### Workflow 3: Fallback Template Handling Checklist

**Purpose:** Handle missing template IDs gracefully.

**Steps:**

1. **Template Lookup**
   - [ ] Receive `templateId` string parameter
   - [ ] Attempt `_templates.TryGetValue(templateId, out var template)`

2. **Check Lookup Result**
   - **If found:**
     - [ ] Use retrieved template
     - [ ] Skip to step 4
   - **If not found:**
     - [ ] Proceed to step 3

3. **Fallback Handling**
   - [ ] Log Warning: "Template not found: {Id}, using fallback template {Fallback}"
   - [ ] Retrieve fallback template: `template = _templates["und_draugr_01"]`
   - [ ] Verify fallback template exists (should always exist)

4. **Create Enemy**
   - [ ] Call `CreateFromTemplate(template, partyLevel)`
   - [ ] Follow standard creation workflow

5. **Return Enemy**
   - [ ] Return created enemy (never null)

---

## Cross-System Integration

### Integration with IDiceService

**Location:** [IDiceService.cs](../../RuneAndRust.Core/Interfaces/IDiceService.cs)

**Responsibilities:**
- Provide random variance roll for enemy stat variation
- Method used: `RollSingle(int dieSize, string context)`

**Call Site:** [EnemyFactory.cs:66](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L66)

**Parameters:**
- dieSize: 11 (results in 0-10 range)
- context: "HP Variance" (for logging)

**Rationale for 11:**
- Roll range 0-10 (inclusive)
- Maps cleanly to 0.95-1.05 range (11 possible values)
- Uniform distribution (each variance equally likely)

**Example:**
```csharp
var roll = diceService.RollSingle(11, "HP Variance"); // Returns 0-10
var variance = 0.95f + (roll × 0.01f);                // Maps to 0.95-1.05
// roll 0 → 0.95, roll 5 → 1.00, roll 10 → 1.05
```

---

### Integration with ICreatureTraitService

**Location:** [ICreatureTraitService.cs](../../RuneAndRust.Core/Interfaces/ICreatureTraitService.cs)

**Responsibilities:**
- Apply random traits to Elite/Boss enemies
- Detect tier from enemy.Tags list
- Select 1-3 traits based on tier
- Modify enemy.ActiveTraits in place

**Call Site:** [EnemyFactory.cs:106](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L106)

**Method Signature:**
```csharp
void EnhanceEnemy(Enemy enemy);
```

**Pre-Enhancement Setup:**
1. EnemyFactory adds tier tag to enemy.Tags (e.g., "Elite", "Boss")
2. CreatureTraitService reads Tags to determine tier
3. Trait count selected based on tier:
   - Elite: 1-2 traits
   - Boss: 2-3 traits

**Post-Enhancement State:**
- enemy.ActiveTraits populated with trait names
- Traits stored as strings (e.g., "Regenerating", "Venomous")
- Trait effects applied by combat system, not factory

**Example:**
```csharp
var enemy = factory.CreateFromTemplate(eliteTemplate, 2);
// Before: enemy.ActiveTraits = []
// After:  enemy.ActiveTraits = ["Regenerating", "Pack Tactics"]
```

---

### Integration with EnvironmentPopulator

**Location:** [EnvironmentPopulator.cs](../../RuneAndRust.Engine/Services/EnvironmentPopulator.cs)

**Responsibilities:**
- Spawn enemies in dungeon rooms using EnemyFactory
- Select template IDs from BiomeElement spawn tables
- Pass party level for scaling

**Usage Pattern:**
```csharp
public class EnvironmentPopulator
{
    private readonly IEnemyFactory _enemyFactory;

    public async Task SpawnEnemiesInRoomAsync(Guid roomId, int partyLevel)
    {
        var templateIds = GetTemplateIdsForRoom(); // e.g., ["und_draugr_01", "bst_vargr_01"]

        foreach (var templateId in templateIds)
        {
            var enemy = _enemyFactory.CreateById(templateId, partyLevel);
            await _enemyRepository.AddAsync(enemy);
        }
    }
}
```

**Integration Points:**
- EnvironmentPopulator determines *which* enemies to spawn
- EnemyFactory determines *how* to create those enemies
- Party level passed from GameState via EnvironmentPopulator

---

### Integration with CombatService

**Location:** [CombatService.cs](../../RuneAndRust.Engine/Services/CombatService.cs)

**Responsibilities:**
- Receive Enemy entities created by EnemyFactory
- Use enemy stats during combat (HP, Stamina, Attributes, Weapon)
- Execute trait effects from enemy.ActiveTraits

**No Direct Integration:**
- CombatService does not call EnemyFactory directly
- Enemies created during dungeon generation, stored in database
- CombatService retrieves enemies from repository at combat start

**Data Flow:**
```
EnemyFactory → Enemy Entity → EnemyRepository (database)
                                     ↓
                              CombatService (retrieves)
```

---

### Integration with EnemyAIService

**Location:** [EnemyAIService.cs](../../RuneAndRust.Engine/Services/EnemyAIService.cs)

**Responsibilities:**
- Read enemy.Archetype to determine AI behavior
- Use enemy stats for decision-making (e.g., use defensive ability if HP < 50%)

**Archetype Mapping:**
- DPS: Always attack, prioritize high damage
- Tank: Defend allies, use taunts
- GlassCannon: High-risk high-reward abilities
- Support: Heal/buff allies
- Swarm: Coordinate attacks, prioritize same target

**Integration:**
```csharp
public class EnemyAIService
{
    public PlannedAction SelectAction(Enemy enemy)
    {
        switch (enemy.Archetype)
        {
            case EnemyArchetype.DPS:
                return SelectHighestDamageAbility(enemy);
            case EnemyArchetype.Tank:
                return SelectDefensiveAbility(enemy);
            // ...
        }
    }
}
```

---

## Data Models

### EnemyTemplate

**Location:** [EnemyTemplate.cs](../../RuneAndRust.Core/Models/Combat/EnemyTemplate.cs)

**Purpose:** Immutable prototype defining enemy base stats and properties.

**Schema:**
```csharp
public record EnemyTemplate(
    string Id,                                     // Unique template ID (e.g., "und_draugr_01")
    string Name,                                   // Display name
    string Description,                            // AAM-VOICE lore description
    EnemyArchetype Archetype,                      // AI behavior type
    ThreatTier Tier,                               // Scaling multiplier tier
    int BaseHp,                                    // Base HP before scaling
    int BaseStamina,                               // Base Stamina before scaling
    int BaseSoak,                                  // Armor soak value
    Dictionary<CharacterAttribute, int> Attributes, // 5 attributes
    int WeaponDamageDie,                           // Weapon damage die size (d4, d6, d8, etc.)
    string WeaponName,                             // Weapon display name
    List<string> Tags                              // Faction/type tags
);
```

**Immutability:**
- Record type (immutable by default)
- Shared across all enemy instances from this template
- Safe to access from multiple threads

---

### Enemy Entity

**Location:** [Enemy.cs](../../RuneAndRust.Core/Entities/Enemy.cs)

**Purpose:** Mutable runtime entity representing a specific enemy instance.

**Schema (Partial):**
```csharp
public class Enemy
{
    public Guid Id { get; set; }                                  // Unique enemy instance ID
    public string Name { get; set; }                              // Display name (from template)
    public int MaxHp { get; set; }                                // Scaled max HP
    public int CurrentHp { get; set; }                            // Current HP (0 = dead)
    public int MaxStamina { get; set; }                           // Scaled max Stamina
    public int CurrentStamina { get; set; }                       // Current Stamina
    public int WeaponDamageDie { get; set; }                      // Weapon die size
    public string WeaponName { get; set; }                        // Weapon name
    public int ArmorSoak { get; set; }                            // Armor soak
    public Dictionary<CharacterAttribute, int> Attributes { get; set; } // Attributes
    public string TemplateId { get; set; }                        // Reference to template
    public EnemyArchetype Archetype { get; set; }                 // AI behavior type
    public List<string> Tags { get; set; }                        // Tags (includes tier tag)
    public List<string> ActiveTraits { get; set; } = new();       // Traits from CreatureTraitService
}
```

**Mutability:**
- Class type (mutable)
- CurrentHp/CurrentStamina modified during combat
- Each instance independent (modifying one doesn't affect others)

---

### ThreatTier Enum

**Location:** [ThreatTier.cs](../../RuneAndRust.Core/Enums/ThreatTier.cs)

**Purpose:** Categorize enemy difficulty for scaling.

**Values:**
```csharp
public enum ThreatTier
{
    Minion = 0,    // 0.6x scaling (weak)
    Standard = 1,  // 1.0x scaling (baseline)
    Elite = 2,     // 1.5x scaling + traits (enhanced)
    Boss = 3       // 2.5x scaling + traits (powerful)
}
```

**Scaling Multipliers:**
- Minion: 0.6x
- Standard: 1.0x
- Elite: 1.5x
- Boss: 2.5x (fixed, no level scaling)

---

### EnemyArchetype Enum

**Location:** [EnemyArchetype.cs](../../RuneAndRust.Core/Enums/EnemyArchetype.cs)

**Purpose:** Define AI behavior pattern for enemy.

**Values:**
```csharp
public enum EnemyArchetype
{
    DPS,           // High damage, aggressive
    Tank,          // High defense, protect allies
    GlassCannon,   // High damage, low defense
    Support,       // Heal/buff allies, debuff enemies
    Swarm,         // Coordinate attacks, low individual threat
    Caster,        // Ranged attacks, special abilities
    Boss           // Multi-phase, complex behavior
}
```

**Used By:**
- EnemyAIService for action selection
- Combat UI for display

---

## Configuration

### Variance Constants

**Location:** [EnemyFactory.cs:24-29](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L24-L29)

**Values:**
```csharp
private const float VarianceMin = 0.95f;  // Minimum variance (95% of base)
private const float VarianceMax = 1.05f;  // Maximum variance (105% of base)
```

**Effect:**
- ±5% randomness on all scaled stats
- Prevents identical encounters
- Keeps balance consistent (small variance)

**Rationale:**
- Too small (±1%): No noticeable difference
- Too large (±20%): Balance issues (same enemy too variable)
- 5% is sweet spot: Adds variety without breaking balance

---

### Fallback Template ID

**Location:** [EnemyFactory.cs:34](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L34)

**Value:**
```csharp
private const string FallbackTemplateId = "und_draugr_01";
```

**Rationale:**
- Rusted Draugr is Standard tier (balanced for all levels)
- DPS archetype (straightforward combat behavior)
- Generic Undying type (fits most dungeon themes)

**Future:** Make configurable via settings or per-biome fallbacks

---

### Tier Scaling Multipliers

**Location:** [EnemyFactory.cs:148-154](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L148-L154)

**Values:**
```csharp
ThreatTier.Minion => 0.6f × levelScale
ThreatTier.Standard => 1.0f × levelScale
ThreatTier.Elite => 1.5f × levelScale
ThreatTier.Boss => 2.5f (fixed, no level scaling)
```

**Tuning Notes:**
- Minion 0.6x: 3 Minions ≈ 1 Standard (swarm tactics)
- Elite 1.5x: Significantly tougher, requires focus fire
- Boss 2.5x: Mini-boss level, designed for specific encounters

---

### Level Scaling Rate

**Location:** [EnemyFactory.cs:145](../../RuneAndRust.Engine/Factories/EnemyFactory.cs#L145)

**Formula:**
```csharp
var levelScale = 1.0f + ((partyLevel - 1) * 0.1f);
```

**Effect:**
- 10% increase per level above 1
- Linear scaling (not exponential)
- Level 1 = 1.0x, Level 5 = 1.4x, Level 10 = 1.9x

**Balance Considerations:**
- Linear scaling may become underpowered at high levels
- Future: Cap at level 10 or use exponential formula

---

## Testing

### Test Coverage Summary

**Test File:** [EnemyFactoryTests.cs](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs)
**Total Tests:** 44 tests across 9 categories
**Lines of Code:** 775 lines
**Coverage:** ~95% (all public methods and scaling logic covered)

---

### Test Category 1: CreateFromTemplate Tests (9 tests)

**Location:** [EnemyFactoryTests.cs:37-156](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L37-L156)

1. **CreateFromTemplate_ReturnsValidEnemy** (lines 40-52)
2. **CreateFromTemplate_CopiesTemplateId** (lines 55-65)
3. **CreateFromTemplate_CopiesArchetype** (lines 68-78)
4. **CreateFromTemplate_CopiesTags** (lines 81-91)
5. **CreateFromTemplate_TagsAreIndependentCopy** (lines 94-105)
6. **CreateFromTemplate_CopiesAllAttributes** (lines 108-127)
7. **CreateFromTemplate_CopiesWeaponStats** (lines 130-141)
8. **CreateFromTemplate_CopiesArmorSoak** (lines 144-154)

**Key Validations:**
- Enemy entity created successfully
- All template properties copied correctly
- Deep copy for collections (Tags, Attributes)

---

### Test Category 2: Variance Tests (5 tests)

**Location:** [EnemyFactoryTests.cs:158-230](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L158-L230)

1. **CreateFromTemplate_AppliesVariance_MinRoll** (lines 161-173)
   - Roll 0 → 0.95x variance → 95 HP from 100 base
2. **CreateFromTemplate_AppliesVariance_MaxRoll** (lines 176-188)
   - Roll 10 → 1.05x variance → 105 HP from 100 base
3. **CreateFromTemplate_AppliesVariance_MiddleRoll** (lines 191-203)
   - Roll 5 → 1.00x variance → 100 HP from 100 base
4. **CreateFromTemplate_CurrentHpEqualsMaxHp** (lines 206-216)
5. **CreateFromTemplate_CurrentStaminaEqualsMaxStamina** (lines 219-229)

---

### Test Category 3: Tier Scaling Tests (4 tests)

**Location:** [EnemyFactoryTests.cs:233-290](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L233-L290)

1. **CreateFromTemplate_MinionTier_ReducesStats** (lines 236-247)
   - 100 base HP × 0.6 = 60 HP
2. **CreateFromTemplate_StandardTier_NormalStats** (lines 250-261)
   - 100 base HP × 1.0 = 100 HP
3. **CreateFromTemplate_EliteTier_IncreasesStats** (lines 264-275)
   - 100 base HP × 1.5 = 150 HP
4. **CreateFromTemplate_BossTier_FixedHighStats** (lines 278-289)
   - 100 base HP × 2.5 = 250 HP

---

### Test Category 4: Party Level Scaling Tests (3 tests)

**Location:** [EnemyFactoryTests.cs:293-338](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L293-L338)

1. **CreateFromTemplate_PartyLevel1_NoScaling** (lines 296-307)
   - Level 1 → 1.0x multiplier
2. **CreateFromTemplate_PartyLevel5_ScalesUp** (lines 310-321)
   - Level 5 → 1.4x multiplier → 140 HP from 100 base
3. **CreateFromTemplate_BossTier_IgnoresPartyLevelScaling** (lines 324-337)
   - Boss at level 1 and level 5 both have 250 HP (2.5x fixed)

---

### Test Category 5: CreateById Tests (3 tests)

**Location:** [EnemyFactoryTests.cs:341-379](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L341-L379)

1. **CreateById_ValidId_ReturnsEnemy** (lines 344-353)
2. **CreateById_InvalidId_ReturnsFallback** (lines 356-365)
3. **CreateById_WithPartyLevel_ScalesCorrectly** (lines 368-378)

---

### Test Category 6: Template Registry Tests (3 tests)

**Location:** [EnemyFactoryTests.cs:383-421](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L383-L421)

1. **GetTemplateIds_ReturnsAllTemplates** (lines 385-397)
   - Validates 5 built-in templates present
2. **GetTemplate_ValidId_ReturnsTemplate** (lines 400-410)
3. **GetTemplate_InvalidId_ReturnsNull** (lines 413-420)

---

### Test Category 7: Built-in Template Validation Tests (9 tests)

**Location:** [EnemyFactoryTests.cs:424-530](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L424-L530)

1. **BuiltInTemplate_RustedDraugr_HasCorrectStats** (lines 427-440)
2. **BuiltInTemplate_HaugbuiLaborer_HasCorrectStats** (lines 443-453)
3. **BuiltInTemplate_UtilityServitor_IsMinion** (lines 456-465)
4. **BuiltInTemplate_AshVargr_IsGlassCannon** (lines 468-478)
5. **BuiltInTemplate_RustClanScav_IsSupport** (lines 481-490)
6. **BuiltInTemplates_AllHaveRequiredAttributes** (lines 492-510)
   - Theory test, runs for all 5 templates
7. **BuiltInTemplates_AllCreateValidEnemies** (lines 512-529)
   - Theory test, validates all templates create enemies

---

### Test Category 8: Edge Case Tests (2 tests)

**Location:** [EnemyFactoryTests.cs:533-562](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs#L533-L562)

1. **CreateFromTemplate_MinimumHpIsOne** (lines 536-547)
   - Minion tier + min variance + 1 base HP → still ≥ 1 HP
2. **CreateFromTemplate_EachCallCreatesUniqueEnemy** (lines 550-561)
   - Verify unique GUIDs per enemy

---

### Test Infrastructure

**Mocking:**
```csharp
private readonly Mock<IDiceService> _mockDice;
private readonly Mock<ICreatureTraitService> _mockTraitService;

// Default variance roll returns 5 (1.00x neutral)
_mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5);
```

**Helper Method:**
```csharp
private static EnemyTemplate CreateTestTemplate(
    string id = "test_enemy_01",
    ThreatTier tier = ThreatTier.Standard,
    int baseHp = 50,
    // ... other parameters with defaults
)
```

**Benefits:**
- Deterministic tests (no random variance)
- Controllable variance for edge case testing
- Reusable test template creation

---

## Domain 4 Compliance

### Applicability Assessment

**Domain 4 Status:** **FULLY COMPLIANT**

**Rationale:**
- All enemy template descriptions are AAM-VOICE Layer 2 content
- Descriptions stored in EnemyTemplate.Description field
- No precision measurements in any template descriptions
- All templates manually reviewed for compliance

---

### Template Descriptions (5 total)

**1. Rusted Draugr:**
```
"A corroded security automaton. Firmware echoes ancient patrol protocols."
```
- ✅ Observer perspective ("A corroded...")
- ✅ Qualitative language ("ancient", "echoes")
- ✅ No precision measurements
- ✅ Technical terms in lore context ("firmware", "protocols")

**2. Haugbui Laborer:**
```
"A towering construction unit. Task-loop corrupted to singular purpose: remove obstacles."
```
- ✅ Epistemic uncertainty ("corrupted to singular purpose")
- ✅ Diagnostic language ("task-loop corrupted")
- ✅ No measurements

**3. Utility Servitor:**
```
"A janitorial drone. Harmless alone, dangerous in clusters."
```
- ✅ Qualitative threat assessment ("harmless alone", "dangerous in clusters")
- ✅ Observer judgment
- ✅ No numeric swarm size specified

**4. Ash-Vargr:**
```
"A corrupted predator. Hunts in fog-shrouded ruins."
```
- ✅ Environmental context ("fog-shrouded ruins")
- ✅ Behavioral description ("hunts")
- ✅ No pack size or territory measurements

**5. Rust-Clan Scav:**
```
"A desperate scavenger. Relies on salvaged tech and crude traps."
```
- ✅ Character assessment ("desperate")
- ✅ Tactical description ("relies on salvaged tech")
- ✅ No weapon damage or trap strength metrics

---

### Technical Stats Exemption

**Stat Values (NOT narrative content):**
```csharp
BaseHp: 60,
BaseStamina: 40,
BaseSoak: 2,
WeaponDamageDie: 6,
Attributes: { Might: 6, ... }
```

**Status:** **Exempt from Domain 4**

**Rationale:**
- These are **game mechanics**, not **narrative descriptions**
- Players do not see raw stats in narrative context
- Stats used by combat system for calculations
- Comparable to "chess piece movement rules" (not lore)

**Player-Facing Presentation:**
- UI may display HP bar (visual representation)
- Combat log may show damage numbers (mechanical feedback)
- Journal entries use descriptor text, not stats

---

### Future Template Additions

**Requirement:** All future enemy templates MUST include:
1. AAM-VOICE compliant Description field
2. No precision measurements in Description
3. Observer perspective (Layer 2 diagnostic tone)
4. Qualitative threat/behavior descriptions

**Review Process:**
1. Write Description field
2. Validate with Domain 4 checklist (SPEC-DESC-001 reference)
3. Replace any precision terms with qualitative equivalents
4. Commit template to codebase

---

## Future Extensions

### Extension 1: JSON Template Loading

**Problem:** Templates hard-coded in C# (requires recompilation to add enemies).

**Solution:**
1. Define JSON schema for EnemyTemplate
2. Store templates in `data/enemies/*.json`
3. Load templates at runtime via `TemplateLoaderService`
4. Support hot-reload (add enemies without restart)

**Example JSON:**
```json
{
  "id": "und_draugr_01",
  "name": "Rusted Draugr",
  "description": "A corroded security automaton...",
  "archetype": "DPS",
  "tier": "Standard",
  "baseHp": 60,
  "baseStamina": 40,
  "baseSoak": 2,
  "attributes": {
    "sturdiness": 5,
    "might": 6,
    "wits": 3,
    "will": 3,
    "finesse": 4
  },
  "weaponDamageDie": 6,
  "weaponName": "Rusted Blade",
  "tags": ["Undying", "IronHeart", "Construct"]
}
```

**Benefit:** Modders can add custom enemies without C# knowledge.

---

### Extension 2: Template Inheritance

**Problem:** Similar enemies (e.g., different Draugr variants) duplicate common stats.

**Solution:**
1. Add `BaseTemplateId` field to EnemyTemplate
2. Support template inheritance: child inherits parent stats, overrides specific values
3. Reduce duplication

**Example:**
```json
{
  "id": "und_draugr_elite",
  "baseTemplateId": "und_draugr_01",
  "tier": "Elite",  // Override tier
  "baseHp": 80      // Override HP, inherit everything else
}
```

**Benefit:** Easier to maintain template families (e.g., Draugr Warrior, Draugr Captain, Draugr Commander).

---

### Extension 3: Stat-Specific Scaling

**Problem:** HP and Stamina always scale equally (same multiplier).

**Solution:**
1. Add per-stat scaling factors to template
2. Example: Tank archetype scales HP 1.5x faster than Stamina

**Implementation:**
```csharp
public record EnemyTemplate(
    // ... existing fields
    float HpScalingMultiplier = 1.0f,
    float StaminaScalingMultiplier = 1.0f
);

var finalHp = (int)(template.BaseHp × scaler × variance × template.HpScalingMultiplier);
```

**Benefit:** More archetype differentiation (Tanks have disproportionately high HP).

---

### Extension 4: Exponential Level Scaling

**Problem:** Linear 10% per level becomes weak at high levels.

**Solution:**
1. Change scaling formula to exponential: `1.0 + ((partyLevel - 1) × 0.15)^1.2`
2. Cap level scaling at level 10 to prevent runaway growth

**Example:**
- Level 1: 1.00x (unchanged)
- Level 5: 1.58x (vs current 1.40x)
- Level 10: 2.18x (vs current 1.90x, capped)

**Benefit:** Keeps enemies challenging at high levels.

---

### Extension 5: Trait Exclusion Rules

**Problem:** EnemyFactory has no control over which traits CreatureTraitService applies.

**Solution:**
1. Add `AllowedTraits` and `ForbiddenTraits` lists to EnemyTemplate
2. Pass to CreatureTraitService during enhancement
3. CreatureTraitService respects exclusion rules

**Example:**
```csharp
template.AllowedTraits = ["Regenerating", "Armored"];  // Only these 2 traits allowed
template.ForbiddenTraits = ["Flying"];                  // This trait forbidden

_traitService.EnhanceEnemy(enemy, template.AllowedTraits, template.ForbiddenTraits);
```

**Benefit:** Thematic consistency (e.g., aquatic enemies can't have Flying trait).

---

### Extension 6: Template Validation

**Problem:** No compile-time or runtime validation of template data integrity.

**Solution:**
1. Implement `ITemplateValidator` interface
2. Validate on initialization:
   - All attributes present (5 required)
   - BaseHp > 0
   - WeaponDamageDie in valid range (4, 6, 8, 10, 12, 20)
   - Description is AAM-VOICE compliant
3. Throw exception if validation fails

**Benefit:** Catch data errors early (at factory construction, not during combat).

---

## Error Handling

### Error Handling Strategy

EnemyFactory uses a **fail-safe with logging** approach:
- Invalid template IDs use fallback (no exception)
- Scaling formula guarantees minimum stat 1 (prevents 0 HP)
- All edge cases logged for debugging

---

### Error Category 1: Template Not Found

**Cause:** `CreateById()` called with non-existent template ID.

**Handling:**
```csharp
if (!_templates.TryGetValue(templateId, out var template))
{
    _logger.LogWarning(
        "Template not found: {Id}, using fallback template {Fallback}",
        templateId, FallbackTemplateId);
    template = _templates[FallbackTemplateId];
}
```

**Recovery:**
- Use fallback template ("und_draugr_01")
- Log warning for developer debugging
- Return valid enemy (never null)

**Benefit:** Prevents crashes from spawn table typos.

---

### Error Category 2: Minimum Stat Clamping

**Cause:** Scaling formula produces < 1 HP/Stamina (e.g., Minion tier + low base + min variance).

**Handling:**
```csharp
var finalHp = Math.Max(1, (int)(template.BaseHp * scaler * variance));
```

**Recovery:**
- Clamp stat to minimum 1
- Prevent 0 HP enemies (would die instantly)

**Example:**
- 10 base HP × 0.6 (Minion) × 0.95 (min variance) = 5.7 → 5 HP (no clamping needed)
- 1 base HP × 0.6 × 0.95 = 0.57 → 1 HP (clamped)

---

### Error Category 3: Missing Fallback Template

**Cause:** Fallback template "und_draugr_01" not present in registry (should never happen).

**Handling:**
- No explicit handling (KeyNotFoundException thrown)
- Factory initialization fails
- Application startup aborted

**Rationale:**
- Fallback template is core to factory operation
- Missing fallback indicates broken initialization
- Fail-fast is appropriate (developer must fix immediately)

---

## Changelog

### Version 1.1.0 (2025-12-24)

**Documentation Update:**
- Added YAML frontmatter for consistency with other specs
- Added `last_updated` field
- Added SPEC-ABILITY-001 to related_specs

**Ability Hydration (v0.2.4a):**
- Documented `AbilityNames` field in EnemyTemplate
- Documented `IActiveAbilityRepository` dependency
- Documented `HydrateAbilitiesAsync()` private method
- Documented graceful missing ability handling

**Async Method Signatures:**
- Updated `CreateFromTemplate` → `CreateFromTemplateAsync`
- Updated `CreateById` → `CreateByIdAsync`
- Added note on async requirement for ability repository

**Test Coverage:**
- Updated test count: 42 → 44 tests
- Updated line count: 604 → 775 lines
- Added test category for ability hydration

---

### Version 1.0.0 (2025-12-22)

**Initial Implementation:**
- Implemented enemy creation from templates
- Implemented multi-factor stat scaling (tier × level × variance)
- Implemented ±5% variance system using IDiceService
- Implemented deep property copying (Attributes, Tags)
- Implemented Elite enhancement integration with CreatureTraitService
- Implemented fallback template system for missing IDs
- Created 5 built-in templates (Undying, Mechanical, Beast, Humanoid)
- 44 unit tests with 95% coverage

**Scaling Formulas:**
- Tier multipliers: Minion 0.6x, Standard 1.0x, Elite 1.5x, Boss 2.5x
- Level scaling: 10% per level above 1 (linear)
- Variance: 0.95x - 1.05x (±5% randomness)
- Minimum stat clamping: all stats ≥ 1

**Elite Enhancement (v0.2.2c):**
- Elite/Boss enemies receive 1-3 traits
- Tier tag added to enemy.Tags for CreatureTraitService detection
- Trait application logged for debugging

**Built-in Templates:**
1. und_draugr_01 - Rusted Draugr (Undying, DPS, Standard, 60 HP)
2. und_haug_01 - Haugbui Laborer (Undying, Tank, Standard, 90 HP)
3. mec_serv_01 - Utility Servitor (Mechanical, Swarm, Minion, 25 HP)
4. bst_vargr_01 - Ash-Vargr (Beast, GlassCannon, Standard, 45 HP)
5. hum_raider_01 - Rust-Clan Scav (Humanoid, Support, Standard, 50 HP)

**Known Limitations:**
- Only 5 built-in templates (future: expand to 20-30)
- Templates hard-coded (future: load from JSON)
- Linear level scaling (future: exponential curve)
- No template inheritance (future: base template support)
- No stat-specific scaling (HP/Stamina scale equally)

**Dependencies:**
- ILogger<EnemyFactory> - logging
- IDiceService - variance roll
- ICreatureTraitService - Elite enhancement
- IActiveAbilityRepository - ability loading (v0.2.4a)

**Domain 4 Compliance:**
- All template descriptions AAM-VOICE compliant
- No precision measurements in narrative content
- Observer perspective maintained

---

## References

### Implementation Files
- [EnemyFactory.cs](../../RuneAndRust.Engine/Factories/EnemyFactory.cs) (293 lines)
- [EnemyFactoryTests.cs](../../RuneAndRust.Tests/Engine/EnemyFactoryTests.cs) (775 lines)

### Related Entities
- [EnemyTemplate.cs](../../RuneAndRust.Core/Models/Combat/EnemyTemplate.cs)
- [Enemy.cs](../../RuneAndRust.Core/Entities/Enemy.cs)
- [ThreatTier.cs](../../RuneAndRust.Core/Enums/ThreatTier.cs)
- [EnemyArchetype.cs](../../RuneAndRust.Core/Enums/EnemyArchetype.cs)

### Related Interfaces
- [IEnemyFactory.cs](../../RuneAndRust.Core/Interfaces/IEnemyFactory.cs)
- [IDiceService.cs](../../RuneAndRust.Core/Interfaces/IDiceService.cs)
- [ICreatureTraitService.cs](../../RuneAndRust.Core/Interfaces/ICreatureTraitService.cs)

### Related Specifications
- SPEC-TRAIT-001: Creature Trait System (trait application for Elite enemies)
- SPEC-DICE-001: Dice Pool System (variance roll mechanics)
- SPEC-ENEMY-001: Enemy AI System (archetype behavior mapping)
- SPEC-ENVPOP-001: Environment Population (enemy spawning orchestration)

---

**End of Specification**
