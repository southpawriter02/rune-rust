---
id: SPEC-ADVANCEMENT-001
title: Character Advancement System
version: 1.0.0
status: Scaffolded
related_specs: [SPEC-XP-001, SPEC-CHAR-001, SPEC-ABILITY-001, SPEC-TRAIT-001]
---

# SPEC-ADVANCEMENT-001: Character Advancement System

> **Version:** 1.0.0
> **Status:** Scaffolded (Formulas exist, level-up logic incomplete)
> **Service:** `StatCalculationService` (partial), `CharacterAdvancementService` (planned)
> **Location:** `RuneAndRust.Engine/Services/StatCalculationService.cs`

---

## Overview

The Character Advancement System manages level-up rewards and character growth from Level 1 to Level 5. When characters cross XP thresholds, they receive attribute points, MaxHP/Stamina increases, full healing, and unlock higher-tier abilities.

This system translates XP accumulation into tangible power increases while maintaining the low-level-cap design philosophy.

---

## Core Concepts

### Level-Up Rewards

**Per-Level Bonuses:**
| Reward Type | Amount | Application |
|-------------|--------|-------------|
| **MaxHP Increase** | +10 | Flat bonus per level |
| **MaxStamina Increase** | +5 | Flat bonus per level |
| **Attribute Point** | +1 | Player chooses which attribute to increase |
| **Full Heal** | 100% | CurrentHP and CurrentStamina restored |
| **Ability Tier Unlock** | See table | Tier 2 at Level 5, Tier 3 at Level 10 (planned) |

**Ability Tier Unlocks:**
| Level | Max Tier | Example Abilities |
|-------|----------|-------------------|
| 1-4 | Tier 1 | Basic abilities (Power Strike, Aimed Shot) |
| 5-9 | Tier 2 | Mid-tier abilities (Cleave, Precision Shot) |
| 10+ | Tier 3 | Advanced abilities (Whirlwind, Headshot) |

**Note:** Current level cap is 5, so only Tier 1 and Tier 2 abilities are accessible.

---

### Derived Stat Scaling

**Formulas (from StatCalculationService):**
```csharp
MaxHP = 50 + (Sturdiness × 10)
MaxStamina = 20 + (Finesse × 5) + (Sturdiness × 3)
ActionPoints = 2 + (Wits ÷ 4)
MaxAP = 10 + (Will × 5)  // Mystic archetype only
```

**Level Scaling:**
- **Attribute-Driven:** MaxHP/Stamina scale with STU/FIN increases
- **Flat Bonus (Planned):** +10 HP, +5 Stamina per level (NOT YET IMPLEMENTED)
- **Compound Growth:** Each attribute point from level-up increases derived stats

---

## Behaviors

### Primary Behaviors

#### 1. Process Level-Up (`ProcessLevelUp`) - NOT IMPLEMENTED

```csharp
void ProcessLevelUp(Character character)
```

**Planned Sequence:**
1. Increment `Character.Level` by 1
2. Add +10 to `Character.MaxHP`
3. Add +5 to `Character.MaxStamina`
4. Recalculate derived stats via `StatCalculationService`
5. Full heal: `CurrentHP = MaxHP`, `CurrentStamina = MaxStamina`
6. Award +1 attribute point (UI handles distribution)
7. Check ability tier unlock
8. Log level-up event
9. Persist character state

**Example:**
```csharp
// Character at Level 1, 100 XP (threshold for Level 2)
_advancementService.ProcessLevelUp(character);

// Before:
// Level: 1, MaxHP: 70, MaxStamina: 35, STU: 5, FIN: 5

// After:
// Level: 2, MaxHP: 80 (+10), MaxStamina: 40 (+5), CurrentHP: 80 (full heal)
// +1 attribute point pending player distribution
```

---

#### 2. Apply Attribute Point (`ApplyAttributePoint`) - NOT IMPLEMENTED

```csharp
void ApplyAttributePoint(Character character, Attribute attribute)
```

**Planned Sequence:**
1. Validate attribute point available (`PendingAttributePoints > 0`)
2. Increment chosen attribute by 1
3. Decrement `PendingAttributePoints` by 1
4. Recalculate derived stats
5. Persist character state

**Example:**
```csharp
// Player chooses to increase Sturdiness after Level 2
_advancementService.ApplyAttributePoint(character, Attribute.Sturdiness);

// Before:
// STU: 5, MaxHP: 80, PendingAttributePoints: 1

// After:
// STU: 6, MaxHP: 90 (80 + 10 from STU increase), PendingAttributePoints: 0
```

---

#### 3. Get Max Ability Tier (`GetMaxAbilityTier`) - PARTIALLY IMPLEMENTED

```csharp
int GetMaxAbilityTier(Character character)
```

**Current Implementation:**
```csharp
// In CombatService.StartCombat() (Line 386-393)
var abilities = _abilityRepository.GetByArchetypeAsync(character.Archetype, maxTier: 1)
    .GetAwaiter().GetResult().ToList();
```

**Hardcoded to Tier 1** - does not check character level.

**Planned Implementation:**
```csharp
public int GetMaxAbilityTier(Character character)
{
    return character.Level switch
    {
        >= 10 => 3,  // Advanced (not reachable with Level 5 cap)
        >= 5 => 2,   // Mid-tier
        _ => 1       // Basic
    };
}
```

**Example:**
```csharp
// Character at Level 5
var maxTier = _advancementService.GetMaxAbilityTier(character);
// Returns: 2

// Load abilities for combat
var abilities = await _abilityRepository.GetByArchetypeAsync(character.Archetype, maxTier);
// Returns: All Tier 1 and Tier 2 abilities for archetype
```

---

#### 4. Recalculate Derived Stats (`RecalculateDerivedStats`) - IMPLEMENTED

```csharp
void RecalculateDerivedStats(Character character)
```

**Current Implementation (StatCalculationService.cs:116-212):**

**Sequence:**
1. Calculate effective attributes (base + equipment + buffs - penalties)
2. Apply formulas:
   - `MaxHP = 50 + (Sturdiness × 10)`
   - `MaxStamina = 20 + (FIN × 5) + (STU × 3)`
   - `ActionPoints = 2 + (WIT ÷ 4)`
   - `MaxAP = 10 + (WILL × 5)` (Mystic only)
3. Calculate Defense: `10 + Finesse`
4. Calculate Soak: `Sturdiness ÷ 2`
5. Update `Character` properties
6. Persist changes

**Example:**
```csharp
// Character with STU 8, FIN 6 gains +1 STU from level-up attribute point
character.SetAttribute(Attribute.Sturdiness, 9);
_statCalculationService.RecalculateDerivedStats(character);

// Before:
// MaxHP: 130 (50 + 8×10)
// MaxStamina: 74 (20 + 6×5 + 8×3)

// After:
// MaxHP: 140 (50 + 9×10)
// MaxStamina: 77 (20 + 6×5 + 9×3)
```

---

## Restrictions

### Level Cap Constraints

**Hard Cap:** Level 5
- No level-ups beyond Level 5
- Attribute points cannot be awarded beyond Level 5
- Tier 3 abilities unreachable (require Level 10)

---

### Attribute Point Limits

**Attribute Range:** 1 to 10
- Cannot decrease attributes below 1
- Cannot increase attributes above 10 (even with equipment bonuses)
- Each level-up awards exactly 1 point (no skipping or doubling)

---

### Ability Tier Restrictions

**Tier Access:**
- Tier 1: Available from character creation
- Tier 2: Unlocked at Level 5
- Tier 3: Requires Level 10 (unreachable with current cap)

**No Manual Unlock:**
- Players cannot "skip" to higher tiers
- Tier gating enforced by `GetByArchetypeAsync(maxTier)` filter

---

## Limitations

### Numerical Bounds

| Constraint | Value | Notes |
|------------|-------|-------|
| Min Attribute | 1 | Cannot go below 1 even with penalties |
| Max Attribute | 10 | Hard cap (equipment can't exceed) |
| HP per Level | +10 | Flat bonus (planned) |
| Stamina per Level | +5 | Flat bonus (planned) |
| Attribute Points per Level | +1 | Exactly 1, no variance |
| Max Tier at Level 5 | 2 | Tier 3 requires Level 10 |

---

### System Gaps

- **No level-up service** - Logic not implemented, only formulas exist
- **No attribute point UI** - No interface for distributing points
- **No pending points tracking** - No `PendingAttributePoints` field on Character
- **No ability tier auto-unlock** - Still hardcoded to Tier 1 in CombatService
- **No level-up animation/UI** - No celebration screen or notification
- **No retroactive level-ups** - If XP jumps 0 → 1000, only levels up once (not implemented)

---

## Use Cases

### UC-1: First Level-Up (Level 1 → 2)

```csharp
// Character defeats enemies, earns 100 XP (threshold for Level 2)
_progressionService.AwardExperience(character, 100);

// XP check triggers level-up
if (_progressionService.CheckLevelUp(character))
{
    _advancementService.ProcessLevelUp(character);

    // Before Level-Up:
    // Level: 1, MaxHP: 70, MaxStamina: 35, CurrentHP: 40 (damaged), STU: 5

    // After Level-Up:
    // Level: 2, MaxHP: 80, MaxStamina: 40, CurrentHP: 80 (full heal), STU: 5
    // PendingAttributePoints: 1
}

// Player distributes attribute point (UI prompt)
_advancementService.ApplyAttributePoint(character, Attribute.Sturdiness);

// Final State:
// Level: 2, MaxHP: 90 (80 + 10 from STU 6), STU: 6, PendingAttributePoints: 0
```

**Narrative Impact:** Near-death character fully healed on level-up, rewarding risk-taking.

---

### UC-2: Tier 2 Ability Unlock (Level 4 → 5)

```csharp
// Character gains enough XP to reach Level 5
_progressionService.AwardExperience(character, 1000);  // Cumulative 2000 XP
_advancementService.ProcessLevelUp(character);

// Level: 4 → 5
// Ability Tier: 1 → 2

// Next combat, load abilities with new tier
var maxTier = _advancementService.GetMaxAbilityTier(character);  // Returns: 2
var abilities = await _abilityRepository.GetByArchetypeAsync(Archetype.Ranger, maxTier);

// abilities now includes:
// Tier 1: Aimed Shot, Quick Draw
// Tier 2: Precision Shot (NEW), Multi-Shot (NEW)
```

**Narrative Impact:** Level 5 milestone provides significant power spike through new abilities.

---

### UC-3: Attribute Stacking (Multiple Level-Ups)

```csharp
// Character levels from 1 → 5, always chooses STU
// Starting STU: 5 (Warrior archetype + Human lineage)

// Level 1 → 2: +1 STU → 6
_advancementService.ApplyAttributePoint(character, Attribute.Sturdiness);

// Level 2 → 3: +1 STU → 7
_advancementService.ApplyAttributePoint(character, Attribute.Sturdiness);

// Level 3 → 4: +1 STU → 8
_advancementService.ApplyAttributePoint(character, Attribute.Sturdiness);

// Level 4 → 5: +1 STU → 9
_advancementService.ApplyAttributePoint(character, Attribute.Sturdiness);

// Final State at Level 5:
// STU: 9 (5 base + 4 from levels)
// MaxHP: 140 (50 + 9×10 + 4×10 level bonuses) = 50 + 90 + 40 = 180
// MaxStamina: 75 (20 + 5×5 + 9×3 + 4×5 level bonuses) = 20 + 25 + 27 + 20 = 92
```

**Note:** Current implementation would be `MaxHP: 140` (no flat bonus), planned would be `180`.

**Narrative Impact:** Focused builds (tank STU, finesse FIN) create distinct playstyles.

---

### UC-4: HP/Stamina Breakpoints

```csharp
// Warrior with base STU 7, FIN 3
// Level 1:
// MaxHP: 120 (50 + 7×10)
// MaxStamina: 41 (20 + 3×5 + 7×3)

// Level 2 → Increases STU to 8:
// MaxHP: 130 (50 + 8×10) → +10 from attribute
// MaxStamina: 44 (20 + 3×5 + 8×3) → +3 from attribute
// PLUS +10 HP, +5 Stamina from level-up (planned)
// Planned Total: MaxHP 140, MaxStamina 49

// Level 3 → Increases FIN to 4:
// MaxHP: 140 (no STU change)
// MaxStamina: 49 (20 + 4×5 + 8×3) → +5 from attribute
// PLUS +10 HP, +5 Stamina from level-up
// Planned Total: MaxHP 150, MaxStamina 54

// Demonstrates: STU increases HP more than Stamina, FIN increases Stamina more than HP
```

**Narrative Impact:** Strategic attribute choices matter for survivability vs. action economy.

---

### UC-5: Full Heal Clutch

```csharp
// Character in combat at 8 HP (MaxHP: 70), out of stamina
// Enemy deals 5 damage → 3 HP remaining (near death)
// Victory! +50 XP

_progressionService.AwardExperience(character, 50);  // Total: 100 XP
if (_progressionService.CheckLevelUp(character))  // True (Level 2 threshold)
{
    _advancementService.ProcessLevelUp(character);

    // Before:
    // CurrentHP: 3, MaxHP: 70, CurrentStamina: 0, MaxStamina: 35

    // After:
    // CurrentHP: 80 (full heal), MaxHP: 80, CurrentStamina: 40, MaxStamina: 40
    // SAVED FROM DEATH by level-up full heal
}
```

**Narrative Impact:** Level-up timing creates dramatic tension, rewards pushing through low-HP combats.

---

## Decision Trees

### Level-Up Processing Flow

```
┌─────────────────────────────────┐
│  XP Threshold Crossed           │
│  (CheckLevelUp returns true)    │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    │ Increment Level │
    └────────┬────────┘
             │
    ┌────────┴────────────┐
    │ Add +10 MaxHP       │
    │ Add +5 MaxStamina   │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Recalculate Derived │
    │ Stats (formulas)    │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Full Heal:          │
    │ CurrentHP = MaxHP   │
    │ CurrentStam = MaxSt │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Award +1 Attribute  │
    │ Point (pending)     │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Check Ability Tier  │
    │ Unlock (Level 5?)   │
    └────────┬────────────┘
         ┌───┴───┐
         │       │
      Level 5  Other
         │       │
         ▼       └─> End
    ┌────────┐
    │ Unlock │
    │ Tier 2 │
    └────────┘
```

---

### Attribute Point Distribution

```
┌─────────────────────────────────┐
│  Player Selects Attribute       │
│  (STU, FIN, WIT, or WILL)       │
└────────────┬────────────────────┘
             │
    ┌────────┴────────────┐
    │ Pending Points > 0? │
    └────────┬────────────┘
         ┌───┴───┐
         │       │
        YES     NO
         │       │
         │       └─> Error: "No points available"
         │
    ┌────┴──────────┐
    │ Attribute < 10?│
    └────┬──────────┘
     ┌───┴───┐
     │       │
    YES     NO
     │       │
     │       └─> Error: "Attribute maxed"
     │
     ▼
┌─────────────┐
│ +1 Attribute│
│ -1 Pending  │
└─────┬───────┘
      │
      ▼
┌──────────────────┐
│ Recalculate Stats│
│ (MaxHP, Stamina) │
└──────────────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IProgressionService` | [SPEC-XP-001](SPEC-XP-001.md) | Receives level-up triggers from XP threshold checks |
| `IStatCalculationService` | Infrastructure | Recalculates MaxHP, MaxStamina, Defense, Soak after attribute changes |
| `IActiveAbilityRepository` | [SPEC-ABILITY-001](SPEC-ABILITY-001.md) | Filters abilities by tier based on character level |
| `IRepository<Character>` | Infrastructure | Persists level, attributes, MaxHP/Stamina changes |
| `ILogger` | Infrastructure | Level-up event tracing, attribute point application |

### Dependents (Provides To)

| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Receives updated ability lists based on tier unlocks |
| `CharacterSheet UI` | [SPEC-UI-001](SPEC-UI-001.md) | Displays level, pending attribute points, level progress |

### Related Systems

- [SPEC-XP-001](SPEC-XP-001.md) - **XP System**: Triggers level-up processing when thresholds crossed
- [SPEC-CHAR-001](SPEC-CHAR-001.md) - **Character System**: Defines attribute storage and derived stat formulas
- [SPEC-ABILITY-001](SPEC-ABILITY-001.md) - **Ability System**: Provides tier-gated ability access

---

## Related Services

### Primary Implementation

| File | Purpose |
|------|----------|
| `StatCalculationService.cs` | Derived stat recalculation (MaxHP, Stamina, Defense, Soak) |
| `CharacterAdvancementService.cs` (PLANNED) | Level-up processing, attribute point distribution |

### Supporting Types

| File | Purpose |
|------|----------|
| `Character.cs` | Level, attributes, MaxHP/Stamina storage |
| `ActiveAbilityRepository.cs` | Tier-filtered ability loading |

---

## Data Models

### Character Entity (Relevant Fields)

```csharp
public class Character
{
    public int Level { get; set; } = 1;
    public int ExperiencePoints { get; set; } = 0;

    // Base Attributes (1-10)
    private Dictionary<Attribute, int> _attributes = new();

    // Derived Stats (calculated from attributes)
    public int MaxHP { get; set; }
    public int CurrentHP { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentStamina { get; set; }
    public int ActionPoints { get; set; }

    // Planned Addition:
    // public int PendingAttributePoints { get; set; } = 0;

    public int GetAttribute(Attribute attr) => _attributes[attr];
    public void SetAttribute(Attribute attr, int value)
    {
        _attributes[attr] = Math.Clamp(value, 1, 10);
    }
}
```

---

### ActiveAbility Entity (Tier Field)

```csharp
public class ActiveAbility
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ArchetypeType Archetype { get; set; }

    /// <summary>
    /// Tier level (1-3). Higher tiers unlock at higher character levels.
    /// Tier 1: Level 1+, Tier 2: Level 5+, Tier 3: Level 10+.
    /// </summary>
    public int Tier { get; set; } = 1;

    // Other properties...
}
```

---

## Configuration

### Level-Up Rewards (Constants)

```csharp
public static class AdvancementConstants
{
    public const int HP_PER_LEVEL = 10;
    public const int STAMINA_PER_LEVEL = 5;
    public const int ATTRIBUTE_POINTS_PER_LEVEL = 1;

    public const int MIN_ATTRIBUTE_VALUE = 1;
    public const int MAX_ATTRIBUTE_VALUE = 10;

    public static int GetMaxAbilityTier(int characterLevel)
    {
        return characterLevel switch
        {
            >= 10 => 3,
            >= 5 => 2,
            _ => 1
        };
    }
}
```

---

### Derived Stat Formulas (Constants)

```csharp
public static class DerivedStatFormulas
{
    // MaxHP = BASE_HP + (Sturdiness × HP_PER_STURDINESS)
    public const int BASE_HP = 50;
    public const int HP_PER_STURDINESS = 10;

    // MaxStamina = BASE_STAMINA + (Finesse × STAMINA_PER_FINESSE) + (Sturdiness × STAMINA_PER_STURDINESS)
    public const int BASE_STAMINA = 20;
    public const int STAMINA_PER_FINESSE = 5;
    public const int STAMINA_PER_STURDINESS = 3;

    // ActionPoints = BASE_AP + (Wits ÷ AP_PER_WITS_DIVISOR)
    public const int BASE_AP = 2;
    public const int AP_PER_WITS_DIVISOR = 4;

    // MaxAP = BASE_AETHER + (Will × AETHER_PER_WILL) [Mystic only]
    public const int BASE_AETHER = 10;
    public const int AETHER_PER_WILL = 5;

    // Defense = BASE_DEFENSE + Finesse
    public const int BASE_DEFENSE = 10;

    // Soak = Sturdiness ÷ SOAK_DIVISOR
    public const int SOAK_DIVISOR = 2;
}
```

---

## Testing

### Test Coverage (Planned)

`CharacterAdvancementServiceTests.cs` (NOT YET CREATED) should cover:

1. **Level-Up Tests** (8 tests)
   - ProcessLevelUp increments level
   - ProcessLevelUp adds +10 MaxHP
   - ProcessLevelUp adds +5 MaxStamina
   - ProcessLevelUp full heals CurrentHP and CurrentStamina
   - ProcessLevelUp awards +1 PendingAttributePoints
   - ProcessLevelUp at Level 5 does not exceed cap
   - ProcessLevelUp recalculates derived stats
   - Multiple level-ups in sequence (Level 1 → 5)

2. **Attribute Point Tests** (7 tests)
   - ApplyAttributePoint increases chosen attribute
   - ApplyAttributePoint decrements PendingAttributePoints
   - ApplyAttributePoint recalculates MaxHP when STU increased
   - ApplyAttributePoint recalculates MaxStamina when FIN increased
   - ApplyAttributePoint fails when no points available
   - ApplyAttributePoint fails when attribute at max (10)
   - ApplyAttributePoint persists character state

3. **Ability Tier Tests** (5 tests)
   - GetMaxAbilityTier returns 1 for Level 1-4
   - GetMaxAbilityTier returns 2 for Level 5-9
   - GetMaxAbilityTier returns 3 for Level 10+ (future-proofing)
   - Load Tier 1 abilities only for Level 1 character
   - Load Tier 1 + Tier 2 abilities for Level 5 character

4. **Stat Calculation Tests** (6 tests)
   - RecalculateDerivedStats with STU 5 → MaxHP 100
   - RecalculateDerivedStats with STU 10 → MaxHP 150
   - RecalculateDerivedStats with FIN 6, STU 4 → MaxStamina 62
   - RecalculateDerivedStats with WIT 8 → ActionPoints 4
   - RecalculateDerivedStats with equipment bonuses (STU +2)
   - RecalculateDerivedStats with corruption penalties (STU -1)

---

## Design Rationale

### Why +1 Attribute Point Per Level?

**Decision:** Award exactly 1 attribute point per level, player chooses distribution.

**Rationale:**
- **Player Agency:** Choice matters more than predetermined stat increases
- **Build Diversity:** Allows specialization (tank STU, finesse FIN, hybrid)
- **Pacing:** 4 points total (Level 2-5) provides meaningful growth without stat inflation

**Alternative Considered:** Archetype-specific auto-increases (Warrior always gets STU). Rejected because:
- Removes player choice
- Homogenizes builds within archetypes

---

### Why Flat HP/Stamina Bonuses?

**Decision:** +10 HP, +5 Stamina per level as flat bonuses (in addition to attribute scaling).

**Rationale:**
- **Consistent Growth:** Guarantees progression even without attribute increases
- **Survivability:** Ensures characters can survive higher-tier enemies
- **Simple Math:** Easy for players to predict growth

**Alternative Considered:** Only attribute-driven scaling. Rejected because:
- No guaranteed HP increase if player chooses non-STU attributes
- Makes STU mandatory for survival

---

### Why Full Heal on Level-Up?

**Decision:** Restore CurrentHP and CurrentStamina to max on level-up.

**Rationale:**
- **Dramatic Moment:** Level-up feels impactful, can turn losing battle into victory
- **Resource Respite:** Encourages pushing through tough fights for level threshold
- **Standard RPG Pattern:** Familiar to players (Final Fantasy, Dragon Quest)

**Alternative Considered:** No heal, or partial heal. Rejected because:
- Less exciting
- Punishes players for barely surviving level-up combat

---

### Why Ability Tier Gating?

**Decision:** Lock Tier 2 abilities behind Level 5, Tier 3 behind Level 10.

**Rationale:**
- **Progression Milestones:** Level 5 feels like major power spike
- **Balance:** Prevents early access to overpowered abilities
- **Content Pacing:** Spreads ability discovery across character lifetime

**Alternative Considered:** All abilities available from start, player chooses. Rejected because:
- No sense of progression
- Optimal builds solved immediately (everyone picks same abilities)

---

## Changelog

### v1.0.0 (Current - Scaffolded)

**Implemented:**
- `StatCalculationService` with derived stat formulas
- `Character.Level` and `Character.ExperiencePoints` properties
- `ActiveAbility.Tier` property for ability gating
- `ActiveAbilityRepository.GetByArchetypeAsync(maxTier)` filtering

**NOT Implemented:**
- `CharacterAdvancementService` (no level-up processing)
- `ProcessLevelUp()` method (no HP/Stamina bonuses applied)
- `ApplyAttributePoint()` method (no UI for distribution)
- `GetMaxAbilityTier()` integration (still hardcoded to Tier 1)
- `PendingAttributePoints` field on Character entity
- Level-up UI/animation

**Design Decisions:**
- +10 HP, +5 Stamina per level (planned but not coded)
- +1 attribute point per level (player choice)
- Full heal on level-up
- Tier 2 unlock at Level 5, Tier 3 at Level 10
- Attribute range 1-10 (hard caps)

**Integration Points:**
- `StatCalculationService.RecalculateDerivedStats()` called after attribute changes
- `CombatService.StartCombat()` loads abilities with hardcoded `maxTier: 1` (Line 386-393)

---

## Future Enhancements

### Automatic Attribute Distribution (Optional Mode)

**Concept:** Allow players to enable auto-distribution based on archetype preferences.

**Implementation:**
```csharp
public void AutoDistributeAttributePoint(Character character)
{
    var preferredAttribute = character.Archetype switch
    {
        Archetype.Warrior => Attribute.Sturdiness,
        Archetype.Ranger => Attribute.Finesse,
        Archetype.Investigator => Attribute.Wits,
        Archetype.Mystic => Attribute.Will,
        _ => Attribute.Sturdiness  // Default
    };

    ApplyAttributePoint(character, preferredAttribute);
}
```

**Benefit:** Streamlines gameplay for players who don't want micromanagement.

---

### Respec System

**Concept:** Allow players to reallocate all attribute points earned from levels.

**Implementation:**
```csharp
public void RespecAttributes(Character character, int scrapCost = 500)
{
    // Reset attributes to base (archetype + lineage bonuses only)
    character.ResetAttributesToBase();

    // Return points equal to (Level - 1)
    character.PendingAttributePoints = character.Level - 1;

    // Deduct scrip cost
    character.Scrip -= scrapCost;

    // Player redistributes points via UI
}
```

**Benefit:** Allows experimentation without restarting character.

---

### Milestone Bonuses

**Concept:** Special rewards at Level 3 and Level 5 beyond normal level-up bonuses.

**Examples:**
```csharp
if (character.Level == 3)
{
    // Unlock "Combat Veteran" passive: +1 ActionPoint permanently
    character.ActionPoints += 1;
}

if (character.Level == 5)
{
    // Unlock "Legendary Scavenger" passive: +10% loot quality chance
    character.LootQualityBonus += 0.10f;

    // Grant one-time reward: Free Clan-Forged weapon or armor
    var reward = _lootService.GenerateLegendaryReward(character);
    character.Inventory.Add(reward);
}
```

**Benefit:** Makes specific levels feel extra special, provides variety beyond stat increases.

---

### Negative Levels (Corruption Penalty)

**Concept:** Extreme corruption (80-100) temporarily reduces effective level.

**Implementation:**
```csharp
public int GetEffectiveLevel(Character character)
{
    if (character.CorruptionState.CorruptionLevel >= 80)
    {
        return Math.Max(1, character.Level - 1);  // -1 level penalty
    }
    return character.Level;
}

// Use in ability tier calculation
var maxTier = GetMaxAbilityTier(GetEffectiveLevel(character));
// High corruption: Level 5 treated as Level 4 → still Tier 1 only
```

**Benefit:** Makes corruption mechanically impactful, not just narrative.

---

## AAM-VOICE Compliance

This specification describes mechanical systems and is exempt from Domain 4 constraints. In-game level-up narration must follow AAM-VOICE guidelines:

**Compliant Example:**
```
Your trials in the depths have hardened your resolve. You feel...stronger. [Level 2]
The scars of battle teach lessons no mentor could impart. [+1 Attribute Point]
```

**Non-Compliant Example:**
```
Level Up! +10 HP, +5 Stamina. Stat points available: 1. [Layer 4 technical bleed]
```
