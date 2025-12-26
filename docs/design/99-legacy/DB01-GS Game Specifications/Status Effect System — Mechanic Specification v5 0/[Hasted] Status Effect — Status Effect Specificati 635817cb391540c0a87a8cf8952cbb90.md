# [Hasted] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Hasted]`

**Category:** Buff (Action Economy Enhancement)

**Type:** Stamina efficiency & tempo boost

**Summary:**

Primary action economy enhancement. Reduces Stamina costs by 25% AND increases passive regen by 50%. Soft power buff dramatically increasing tempo and efficiency. Enables "burst" phases.

**Thematic Framing (Layer 2):**

Personal operating system overclocked. Physical/neural subroutines run at dangerously accelerated rate. Fueled by alchemical stimulants or pure coherent energy.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 2-3 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Refresh (duration resets, cannot be "more hasted")

**Max Stacks:** 1 (refreshes only)

**Visual Icon:** ⚡ (lightning bolt)

---

## III. Mechanical Implementation

### Primary Effect: Dual Stamina Boost

**Effect 1 - Action Efficiency:**

```
All action Stamina costs: -25% (rounded down)
```

**Formula:**

```
Hasted Stamina Cost = Base Stamina Cost × 0.75 (rounded down)
```

**Effect 2 - Accelerated Recovery:**

```
Passive Stamina regeneration: +50%
```

**Formula:**

```
Hasted Stamina Regen = Base Stamina Regen × 1.5
```

**Special Interaction - Glitch-Dash:**

```
Glitch-dash while Hasted: Cost × 0.67 (-33% instead of -25%)
```

**Rationale:** Processor-intensive action benefits more from overclocked system.

**Variable Definitions:**

- **Cost Multiplier:** 0.75x (25% reduction)
- **Regen Multiplier:** 1.5x (50% increase)
- **Glitch-Dash Multiplier:** 0.67x (33% reduction)
- **Rounding:** Always round down

**Examples:**

```
Strike (40 Stamina) while Hasted:
  40 × 0.75 = 30 Stamina

Shield Bash (35 Stamina) while Hasted:
  35 × 0.75 = 26.25 → 26 Stamina (rounded down)

Glitch-Dash (30 Stamina) while Hasted:
  30 × 0.67 = 20 Stamina (33% reduction)

Passive Regen (20 Stamina/turn) while Hasted:
  20 × 1.5 = 30 Stamina/turn
```

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Support-Oriented Specialists

**Brewmaster (Alchemical) "Frenzy-Brew":**

- High-tier crafted consumable
- Most common source

**Bone-Setter (Alchemical) "Adrenaline Shot":**

- Shorter duration, potent effect
- Emergency burst tool

**Örlög-bound / Norn-Spinner (Arcane):**

- High-tier magical haste
- "Accelerate timeline" effect

---

### Enemy Sources (Rare)

**Enemy Commanders/Alchemists:**

- Buff allies with [Hasted]
- High-priority target for party

**Rationale:** Very rare on enemies, signals tactical priority

---

## V. Application & Dispelling

### Application

**No Resistance Check:** Willing buff, no check required

---

### Dispelling (Enemy Actions)

**Dispel Type:** Alchemical or Arcane

**Enemy Dispel Methods:**

- Enemy Mystic "Dispel Magic" ability
- Enemy curse effects

---

### Natural Counter: [Slowed]

**Special Interaction:**

```
[Hasted] + [Slowed] applied = BOTH effects cancel and remove
```

**Rationale:** Clear, intuitive counter-play mechanic

---

## VI. Tactical Implications

### The Tempo King

**Primary Use Case:** Enable burst/burn phases

**Power:**

- Unleash massive number of high-cost abilities
- Short window of accelerated action
- Tempo advantage over enemies

---

### Synergy with High-Cost Builds

**Incredible Synergy:**

- **Berserker:** Multiple Fury-spending abilities in row
- **Strandhögg:** High-mobility, high-cost actions
- **Any high-Stamina-cost spec:** Enables sustained special ability use

---

### Strategic Application Decisions

**Critical Tactical Choice:**

- WHO to Haste? Warrior for attacks? Bone-Setter for healing/cleansing?
- WHEN to Haste? Save for burst phase? Use early for tempo?
- Depends on battle state

**Resource Management:** Precious, limited buff = deploy at critical moment

---

### Balancing Factors

**Power Balanced By:**

- **Rarity:** Difficult to obtain/apply
- **Finite duration:** 2-3 rounds, temporary "super mode"
- **[Slowed] counter:** Clear counter-play exists
- **Prevents Stamina cost reaching zero:** 25% reduction, not elimination

---

## VII. Combat Log Examples

**Application:**

> Gorm is **[Hasted]**! His movements blur with unnatural speed!
> 

**Action Usage:**

> Gorm is [Hasted]! His **Shield Bash** costs only 26 Stamina (normally 35).
> 

**Expiration:**

> The surge of energy fades. Gorm is no longer [Hasted].
> 

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Hasted] Status Effect Feature Specification

**Target:** DB10 [Hasted] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)