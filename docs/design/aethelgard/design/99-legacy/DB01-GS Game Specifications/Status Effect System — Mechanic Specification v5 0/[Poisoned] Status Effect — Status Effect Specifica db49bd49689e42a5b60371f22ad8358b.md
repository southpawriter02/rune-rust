# [Poisoned] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Poisoned]`

**Category:** Debuff (Damage Over Time + Stat Penalty)

**Type:** Alchemical DOT with defensive degradation

**Summary:**

Character suffers toxic damage over time AND reduced Defense Score. Dual-threat debuff that both damages and softens target for party. Stacks up to 5 times.

**Thematic Framing (Layer 2):**

Represents corrupting agent invading biological/mechanical operating system. Not a clean wound—invasive internal sickness degrading from within.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 3 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Intensify (stacks increase both damage AND Defense penalty)

**Max Stacks:** 5

**Visual Icon:** ☠️ (skull and crossbones)

---

## III. Mechanical Implementation

### Primary Effect: Dual Threat

**Effect 1 - Damage Over Time:**

```
Damage per turn = Base Damage × Current Stacks
Base Damage = 1d6 (typically)
```

**Effect 2 - Defensive Degradation:**

```
Defense Penalty = -2 per stack
Modified Defense = Base Defense - (2 × Current Stacks)
```

**Variable Definitions:**

- **Current Stacks:** Number of [Poisoned] stacks (range: 1-5)
- **Base Damage:** Set by ability (typically 1d6)
- **Defense Penalty:** -2 per stack (cumulative)

**Trigger Timing:**

- DoT damage: Start of affected character's turn
- Defense penalty: Continuous (active while poisoned)

**Damage Type:** Poison

**Soak Application:** Poison damage IS mitigated by Soak (unlike Bleeding)

**Value Ranges:**

- 1 stack: 1d6 damage, -2 Defense
- 3 stacks: 3d6 damage, -6 Defense
- 5 stacks (max): 5d6 damage, -10 Defense

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Myr-Stalker Abilities

**"Envenomed Strike":**

- Melee attack with poison application
- Applies 1-2 stacks on hit
- Resistance: STURDINESS Resolve Check (DC varies by tier)

**"Spore Trap":**

- Area effect trap
- Applies 1 stack to all in area
- Resistance: STURDINESS Resolve Check

---

### Secondary: Alchemical Coatings

**Brewmaster "Venom of the Myr-Drekar":**

- Weapon coating consumable
- Adds poison application to any weapon
- Duration: 3 attacks or 1 combat

**Veiðimaðr "Poisoned Arrow":**

- Active ability, single-shot delivery
- High potency: 2 stacks on hit

---

### Enemy Sources

- Mutated beasts from toxic swamps (Myr-Drekar)
- Giant insects
- Rival alchemists

---

*Next: Section V - Resistance & Immunity.*

## V. Resistance & Immunity

### Resistance Check

**Resistance Type:** Physical Resolve (STURDINESS-based)

**DC Range:** 10-16 (set by ability)

**Success:** Completely resist, no stacks applied

**Failure:** Apply full stacks

---

### Immunity

**Immune Entities:**

- **Mechanical Constructs:** Jötun-Forged, Undying (no biology)
- **Incorporeal Undead:** Forlorn (no physical form)
- **Myr-Stalker:** Toxic Acclimation passive (immune to own poison)

**Resistant Entities:**

- **Iron-Blooded:** Filtered bodies provide natural resistance

---

## VI. Cleansing Methods

### Active Cleansing

**Cleanse Type:** Alchemical

**Primary Method: Bone-Setter's "Administer Antidote"**

- Removes all [Poisoned] stacks instantly
- Requires Standard Action in combat

**Consumables:**

- [Common Antidote]: Removes up to 3 stacks
- Alchemical items from crafting

**Ineffective Methods:**

- Physical treatments (Apply Tourniquet) do NOT work
- Standard magical healing does NOT cleanse poison
- Chemical counter-agent required

---

## VII. Tactical Implications

### The Debuffer's Tool

**Primary Use Case:** Softening high-evasion targets

**Comparison to [Bleeding]:**

- [Bleeding]: Anti-tank (bypasses Soak)
- [Poisoned]: Anti-evasion (reduces Defense)

**Synergy:** Powerful combo with [Corroded]

- [Poisoned] reduces Defense (easier to hit)
- [Corroded] reduces Soak (more damage through)
- Combined: Target is completely exposed

---

### Battle of Attrition

**Resource Management:** Players must decide: cleanse or out-damage?

**Party Enabler:** Defense penalty benefits entire party (everyone hits more often)

**Stacking Pressure:** High stacks create urgency (5d6 damage + -10 Defense = critical threat)

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Poisoned] Status Effect Feature Specification

**Target:** DB10 [Poisoned] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)