# [Slowed] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Slowed]`

**Category:** Debuff (Action Economy Disruption)

**Type:** Stamina cost increase (soft control)

**Summary:**

Primary action economy disruption tool. Increases Stamina cost of ALL actions by 50%. Doesn't deny turns but makes every action more costly. Soft control for tempo manipulation.

**Thematic Framing (Layer 2):**

Severe system lag. Personal operating system desynchronized from world's grid. Subroutines executing with delay. Speed and efficiency critically compromised.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 2 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Refresh (cost increase doesn't stack, duration resets)

**Max Stacks:** 1 (refreshes only)

**Visual Icon:** 🐢 (turtle/slowness symbol)

---

## III. Mechanical Implementation

### Primary Effect: The Stamina Tax

**Effect:**

```
All action Stamina costs: +50% (rounded up)
```

**Formula:**

```
Slowed Stamina Cost = Base Stamina Cost × 1.5 (rounded up)
```

**Applies To:**

- **All combat actions:** Attacks, abilities, movement
- **Defensive actions:** Parry, Block
- **Special actions:** Glitch-dash (see below)

**Special Interaction - Glitch-Dash:**

```
Glitch-dash while Slowed: Cost × 2 (doubled, not +50%)
```

**Rationale:** Processor-intensive action particularly susceptible to system lag.

**Variable Definitions:**

- **Cost Multiplier:** 1.5x (fixed, does not stack)
- **Glitch-Dash Multiplier:** 2.0x (special case)
- **Rounding:** Always round up

**Examples:**

```
Strike (40 Stamina) while Slowed:
  40 × 1.5 = 60 Stamina

Shield Bash (35 Stamina) while Slowed:
  35 × 1.5 = 52.5 → 53 Stamina (rounded up)

Glitch-Dash (30 Stamina) while Slowed:
  30 × 2.0 = 60 Stamina (doubled)
```

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Battlefield Control Specialists

**Hlekkr-master "Binding Chains":**

- Quintessential [Slowed] master
- Can apply to entire row
- Physical restraint

**Brewmaster "Ensnaring Sap Bomb":**

- Crafted consumable
- Creates zone of difficult terrain + [Slowed]

**Myr-Stalker Advanced Poisons:**

- Alchemical application
- May combo with other effects

**Atgeir-wielder "Trip":**

- High-tier polearm ability
- Physical application

---

### Enemy Sources

- Nets and chains
- Cryogenic attacks
- Physical impediments

---

## V. Resistance & Immunity

### Resistance Check

**Resistance Type:** Physical Resolve (STURDINESS-based)

**DC Range:** 10-14 (set by ability)

**Success:** Resist, no [Slowed] applied

**Failure:** Apply [Slowed] effect

**Rationale:** Physical ability to power through impediment

---

### Vulnerability

**Vulnerable Entities:**

- **Skirmisher-types:** Aether-Vultures, rival Myrk-gengr
- **High-mobility enemies:** Speed-focused foes especially susceptible
- **Effect:** Penalty to Resolve Check

---

## VI. Cleansing Methods

### Active Cleansing

**Cleanse Type:** Physical or Alchemical (depends on source)

**Physical Source:**

- Requires physical intervention ability
- Break chains/restraints

**Alchemical Source (Toxin):**

- Requires [Antidote]
- Neutralize neurotoxin

**Magical Cleanse:**

- Vard-Warden "Cleansing Ward" (high-tier)
- Restores system coherence

---

## VII. Tactical Implications

### The Tempo Controller

**Primary Use Case:** Direct assault on action economy

**Effect on Resource-Dependent Enemies:**

- Forces recovery turns
- Creates openings for party
- Exhausts Stamina pools faster

---

### The Anti-Skirmisher Tool

**Hard Counter to High-Mobility:**

- Cripples movement abilities
- Expensive abilities become prohibitive
- Neutralizes Strandhögg, fast enemies

---

### Synergy with DoT Effects

**Powerful Combination:**

- [Slowed] enemy has fewer resources for cleanse actions
- DoTs tick for full duration
- Enemy forced to choose: act or cleanse?

**Makes Setup Easier:**

- Slowed targets easier to hit with complex abilities
- Reduced ability to reposition
- Tempo advantage for party

---

### Impact by Archetype

**Warriors/Skirmishers:**

- Direct assault on primary resource (Stamina)
- Forces fewer special actions or exhaustion

**Mystics/Adepts:**

- Taxes secondary resource
- Less mobility
- Harder to use defensive actions (Parry)

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Slowed] Status Effect Feature Specification

**Target:** DB10 [Slowed] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)