# [Bleeding] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Bleeding]`

**Category:** Debuff (Damage Over Time)

**Type:** Physical DOT with stacking

**Summary:**

Character suffers ongoing blood loss, taking Physical damage at the start of each turn. Stacks up to 5 times for cumulative effect.

**Thematic Framing (Layer 2):**

Represents catastrophic breach in physical "hardware"—arterial damage that won't self-seal without intervention. Blood loss causes continuous system integrity failure.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 3 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Intensify (multiple applications add stacks)

**Max Stacks:** 5

**Visual Icon:** 🩸 (blood drop)

---

## III. Mechanical Implementation

### Primary Effect: Ongoing Blood Loss

**Damage Formula:**

```
Damage per turn = 1d6 × Current Stacks
```

**Variable Definitions:**

- **1d6:** Base bleeding damage die
- **Current Stacks:** Number of [Bleeding] stacks on character (range: 1-5)

**Trigger Timing:** Start of affected character's turn

**Damage Type:** Physical (untyped)

**Soak Application:** **Bleeding damage ignores Soak**

**Rationale:** Internal damage beneath armor. This makes Bleeding exceptionally powerful against high-Soak tanks.

**Value Ranges:**

- 1 stack: 1d6 damage (1-6 per turn)
- 3 stacks: 3d6 damage (3-18 per turn)
- 5 stacks (max): 5d6 damage (5-30 per turn)

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Critical Hits with Slashing/Piercing Weapons

**Trigger:** Critical hit (3+ net successes on Accuracy Check)

**Weapon Requirements:**

- Must deal slashing or piercing damage
- Examples: Swords, axes, spears, arrows, claws

**Application:** Automatically applies 1 stack (no resistance check)

**Rationale:** A perfect strike finds an artery or vital point.

---

### Secondary: Bleeding-Specific Abilities

**Vargr-Born "Savage Claws":**

- Applies 1-2 stacks based on ability tier
- Resistance: STURDINESS Resolve Check (DC varies)

**Hólmgangr "Hemorrhaging Strike":**

- High-cost finisher ability
- Applies 2 stacks automatically on hit

**Veiðimaðr "Barbed Arrows":**

- Crafted ammunition type
- Chance to apply 1 stack on hit

---

*Next: Section V - Resistance & Immunity.*

## V. Resistance & Immunity

### Resistance Check

**Resistance Type:** Physical Resolve (STURDINESS-based)

**When Applied:** Only for ability-applied bleeding (not critical hits)

**DC Range:** 10-16 (set by ability)

**Success:** Completely resist, no stacks applied

**Failure:** Apply full stacks

**No Partial Resistance:** All-or-nothing

---

### Immunity

**Immune Entities:**

- **Constructs:** No biological systems, cannot bleed
- **Incorporeal Undead:** No physical form (Forlorn, Draugr Shades)
- **Elementals:** No blood or vital fluids

**Resistant Entities:**

- **Stone-Kin:** Thick hide, requires higher DC
- **Carapace creatures:** Natural armor provides resistance

---

## VI. Cleansing Methods

### Active Cleansing

**Cleanse Type:** Physical

**Primary Method: Bone-Setter's "Apply Tourniquet"**

- Removes all [Bleeding] stacks instantly
- Requires Standard Action in combat
- Most reliable cleanse method

**Consumables:**

- [Soot-Stained Bandage]: Removes up to 3 stacks
- Field medicine items from crafting

**High-Tier Magic:**

- Grove-Warden regeneration spells (rare)
- Physically re-knits flesh

---

### Natural Expiration

**Default:** Effect expires when duration reaches 0

**Cannot be ignored:** Must be actively cleansed or will persist full duration

---

## VII. Tactical Implications

### The Anti-Tank Tool

**Primary Use Case:** Bypassing high-Soak enemies

**Synergy:** Multiple Bleed-focused characters stack damage rapidly

**Counter:** High STURDINESS for resistance, Bone-Setter for cleansing

---

### Battle of Attrition

**Pressure Tool:** Forces enemy action economy (cleanse or risk death)

**Burst Windows:** Max stacks (5) deal devastating DoT (5-30 per turn)

**Risk vs. Reward:** Focusing stacks on one target vs. spreading pressure

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Bleeding] Status Effect Feature Specification

**Target:** DB10 [Bleeding] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)