---
id: SPEC-SPECIALIZATION-TEMPLATE
title: "Specialization Template"
version: 1.0
status: approved
last-updated: 2025-12-07
---

# [Specialization Name] — Complete Specification

> **Use this template for all new specialization specifications.**

---

## Document Control

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | YYYY-MM-DD | Initial specification |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
|----------|-------|
| Internal Name | `[PascalCase]` |
| Display Name | `[Display Name]` |
| Specialization ID | `[Numeric ID]` |
| Archetype | `[Archetype Name]` (ArchetypeID = X) |
| Path Type | Coherent / Divergent / Hybrid |
| Mechanical Role | `[Role description]` |
| Primary Attribute | `[ATTRIBUTE]` |
| Secondary Attribute | `[ATTRIBUTE]` |
| Resource System | Stamina / Mana / Both / Unique |
| Trauma Risk | None / Low / Medium / High |
| Icon | `:emoji:` |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| PP Cost to Unlock | X PP | Standard cost |
| Minimum Legend | X | Early/Mid/Late-game |
| Maximum Corruption | X | Corruption restriction |
| Minimum Corruption | X | Corruption minimum |
| Required Quest | None / Quest Name | Prerequisite |

### 1.3 Design Philosophy

**Tagline**: "[One-liner description]"

**Core Fantasy**: [2-3 sentences describing the player experience]

**Mechanical Identity**:
1. [Primary mechanic or keyword]
2. [Secondary mechanic or strength]
3. [Tertiary mechanic or unique feature]
4. [Fourth mechanic if applicable]

### 1.4 Keywords Used

| Keyword | Description | Opposed Check |
|---------|-------------|---------------|
| `[Keyword]` | [Effect description] | [If applicable] |

### 1.5 Full Description

[Extended narrative description for player-facing text. 3-5 sentences.]

---

## 2. Rank Progression System

### 2.1 Rank Upgrade Rules

All abilities start at **Rank 1** when unlocked. Players choose which abilities to invest in for higher ranks:

| Upgrade | PP Cost | Effect |
|---------|---------|--------|
| **Rank 2** | +2 PP | Enhanced effects, reduced costs |
| **Rank 3** | +3 PP | Mastered form, bonus effects |

> [!TIP]
> Rank upgrades are **optional per-ability choices**. You don't need to buy other abilities to upgrade the ones you love.

### 2.2 Ability Structure by Tier

| Tier | Abilities | Unlock Cost (R1) | +Rank 2 | +Rank 3 | Max Total |
|------|-----------|------------------|---------|---------|-----------|
| Tier 1 | 3 | 3 PP each | +2 PP | +3 PP | 8 PP each |
| Tier 2 | 3 | 4 PP each | +2 PP | +3 PP | 9 PP each |
| Tier 3 | 2 | 5 PP each | +2 PP | +3 PP | 10 PP each |
| Capstone | 1 | 6 PP | +2 PP | +3 PP | 11 PP |

### 2.3 Total PP Investment

| Investment Level | PP Spent | Notes |
|-----------------|----------|-------|
| Unlock Specialization | 3 PP | Access to ability tree |
| All Tier 1 (Rank 1) | 12 PP | Foundation abilities |
| Full Tree (Rank 1) | 40 PP | All 9 abilities at base power |
| Full Tree (all Rank 3) | 85 PP | Maximum investment |

---

## 3. Ability Tree Overview

### 3.1 Visual Tree Structure

```
TIER 1: FOUNDATION (3 PP each, +2 for R2, +3 for R3)
┌─────────────────────┼─────────────────────┐
│                     │                     │
[Ability 1]       [Ability 2]       [Ability 3]
(Type)              (Type)              (Type)
│                     │                     │
└─────────────────────┴─────────────────────┘
                      │
                      ▼
TIER 2: ADVANCED (4 PP each, +2 for R2, +3 for R3)
┌─────────────────────┼─────────────────────┐
│                     │                     │
[Ability 4]       [Ability 5]       [Ability 6]
(Type)              (Type)              (Type)
│                     │                     │
└─────────────────────┴─────────────────────┘
                      │
                      ▼
TIER 3: MASTERY (5 PP each, +2 for R2, +3 for R3)
┌───────────────┴───────────────┐
│                               │
[Ability 7]                 [Ability 8]
(Type)                        (Type)
│                               │
└───────────────┬───────────────┘
                │
                ▼
TIER 4: CAPSTONE (6 PP, +2 for R2, +3 for R3)
                │
        [Ability 9]
          (Type)
```

### 3.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Resource Cost | Key Effect |
|----|--------------|------|------|-------|---------------|------------|
| XXXX | [Name] | 1 | Passive/Active | 1→2→3 | None/X Stamina | [Brief] |
| ... | ... | ... | ... | ... | ... | ... |

---

## 4. Tier 1 Abilities (Detailed Rank Specifications)

### 4.1 [Ability Name] (ID: XXXX)

**Type**: Passive/Active | **Action**: Free/Standard/Bonus/Reaction | **Target**: Self/Single/AoE

#### Overview

| Property | Value |
|----------|-------|
| Tier | 1 (Foundation) |
| PP Cost to Unlock | 3 PP |
| Ranks | 3 |
| Resource Cost | None/X Stamina |
| Attribute Used | [If applicable] |
| Damage Type | [If applicable] |
| Cooldown | [If applicable] |
| Special | [Keywords] |

**Description**: [Narrative description of the ability]

#### Rank Details

**Rank 1** (Base — included with ability unlock)
- **Effect**: [Mechanical description]
- **Formula**:
  ```
  [Pseudocode formula]
  ```
- **GUI Display**:
  - Icon: [Description]
  - Tooltip: "[Exact tooltip text]"
  - Color: Bronze (`#CD7F32`)

**Rank 2** (Upgrade Cost: +2 PP)
- **Effect**: [Mechanical description]
- **Formula**:
  ```
  [Pseudocode formula]
  ```
- **GUI Display**:
  - Tooltip: "[Exact tooltip text]"
  - Color: Silver (`#C0C0C0`)

**Rank 3** (Upgrade Cost: +3 PP, requires Rank 2)
- **Effect**: [Mechanical description]
- **Formula**:
  ```
  [Pseudocode formula]
  ```
- **GUI Display**:
  - Tooltip: "[Exact tooltip text]"
  - Color: Gold (`#FFD700`)

#### Implementation Status

| Component | Status |
|-----------|--------|
| Data seeding | [Implemented/Planned] |
| Combat integration | [Implemented/Planned] |

---

## 5. Tier 2 Abilities (Rank 2→3 Progression)

[Repeat Section 4 format for each Tier 2 ability]

---

## 6. Tier 3 Abilities (Rank 2→3 Progression)

[Repeat Section 4 format for each Tier 3 ability]

---

## 7. Capstone Ability

[Repeat Section 4 format with emphasis on tree-wide effects]

---

## 8. Status Effect Definitions

| Effect | Applied By | Duration | Icon | Color | Effects |
|--------|------------|----------|------|-------|---------|
| `[Status]` | [Ability Name] | X turns | [Description] | [Color Name] | [Bullet list] |

---

## 9. GUI Requirements

### 9.1 Position Indicator
```
┌─────────────────────────────────────────┐
│ POSITION: [ROW]                         │
│ 🛡️ [Indicator]                          │
│ ⚔️ [Effect]: X affected                 │
└─────────────────────────────────────────┘
```

### 9.2 Ability Card Rank Indicators

| Rank | Border Color | Badge |
|------|--------------|-------|
| 1 | Bronze (`#CD7F32`) | "I" |
| 2 | Silver (`#C0C0C0`) | "II" |
| 3 | Gold (`#FFD700`) | "III" |

---

## 10. Implementation Status

### 10.1 Related Files

| Component | File Path | Status |
|-----------|-----------|--------|
| Data Seeding | `RuneAndRust.Persistence/DataSeeder.cs` | [Status] |
| Enum | `RuneAndRust.Core/Specialization.cs` | [Status] |
| Factory | `RuneAndRust.Engine/SpecializationFactory.cs` | [Status] |
| Tests | `RuneAndRust.Engine.Tests/` | [Status] |

---

## 11. Implementation Priority

### Phase 1: Foundation
1. [Critical task]

### Phase 2: Combat Integration
1. [Combat task]

### Phase 3: Advanced Mechanics
1. [Advanced task]

### Phase 4: Status Effects
1. [Status effect task]

### Phase 5: Polish
1. [GUI/polish task]

---

## 12. Testing Requirements

### Unit Tests
- [ ] Ability damage calculations
- [ ] Rank progression logic
- [ ] Resource cost validation

### Integration Tests
- [ ] Combat engine integration
- [ ] Status effect application
- [ ] GUI tooltip accuracy

---

*End of Specification*
