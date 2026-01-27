# [Disoriented] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Disoriented]`

**Category:** Debuff (Accuracy & Skill Penalty)

**Type:** Universal action penalty (soft control)

**Summary:**

Primary accuracy/skill-check debuff. -2 dice penalty to ALL action checks. Doesn't deny turns but makes every action significantly less reliable. Soft control for blunting offensive capabilities.

**Thematic Framing (Layer 2):**

Cognitive glitch causing desynchronization between intent and action. Personal operating system flooded with contradictory/nonsensical data. Targeting frayed, subroutines delayed, precision compromised.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 2 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Refresh (penalty doesn't increase, duration resets)

**Max Stacks:** 1 (refreshes only)

**Visual Icon:** 🔄 (disorientation symbol)

---

## III. Mechanical Implementation

### Primary Effect: Universal Penalty Die

**Effect:**

```
All action checks: -2 dice penalty
```

**Applies To:**

- **Accuracy Rolls:** More likely to miss or graze
- **Skill Checks (combat):** Harder to perform skill actions
- **Resolve Checks:** More vulnerable to status effects
- **Mystic Spell Potency:** Reduced damage/healing output

**Variable Definitions:**

- **Penalty:** -2 dice (fixed, does not stack)
- **Duration:** 2 rounds (typical)

**Rationale:** Universal debuff affecting every aspect of performance. "Noisy signal" interferes with all actions.

**Example:**

```
Normal Accuracy Pool: 10d10
Disoriented: 10d10 - 2d10 = 8d10

Normal Skill Check: 8d6
Disoriented: 8d6 - 2d6 = 6d6
```

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Psychological Warfare & Disruption

**Echo-Caller / Psychic Virus Enemies:**

- Psychic disorientation masters
- WILL-based Resolve Check to resist

**Thul "Demoralizing Diatribe":**

- Logic-based cognitive dissonance attack
- Mental origin

**Scrap-Tinker "Flash Bomb":**

- Technological sensory overload
- STURDINESS-based Resolve Check (concussive)

**Strandhögg "Dread Charge":**

- Illogical speed causes disorientation
- Physical/mental hybrid

---

### Enemy Sources

- Psychic attackers
- Concussive blasts
- Disorienting sonic screeches

---

*Next: Section V - Resistance & Immunity.*

## V. Resistance & Immunity

### Resistance Check

**Resistance Type:** Depends on source

- **Psychic source:** Mental Resolve (WILL-based)
- **Concussive source:** Physical Resolve (STURDINESS-based)

**DC Range:** 10-14 (set by ability)

**Success:** Completely resist, no disorientation

**Failure:** Apply [Disoriented] effect

---

## VI. Cleansing Methods

### Active Cleansing

**Cleanse Type:** Depends on source (Mental or Physical)

**Mental Source Cleansing:**

- **Skald's "Inspiring Word":** Re-centers the mind
- **Thul's logical guidance:** Restores clarity

**Physical Source Cleansing:**

- **Bone-Setter's "Administer Stimulant":** Clears the head
- Physical intervention

**Natural Expiration:**

- Often the only practical method
- Wait for effect to expire (2 rounds typical)

---

## VII. Tactical Implications & Trauma Economy

### The Great Equalizer

**Primary Use Case:** Leveling field against high-skill bosses/elites

**Effect on Powerful Enemies:**

- Reduced dice pools = less reliable attacks
- More vulnerable to party status effects
- Powerful attacks less likely to land

---

### The Anti-Skirmisher Tool

**Counter to High-Evasion Enemies:**

- Attacks core FINESSE strengths
- Reduced accuracy AND defense (penalized Resolve)
- Complements [Slowed] (which attacks action economy)

---

### Trauma Economy Integration

**Psychic Stress Impact:**

- Psychic-based disorientation inflicts **Psychic Stress**
- Even if [Disoriented] resisted, Stress still applied
- Constant threat to party mental fortitude

---

### No Stunlock Prevention

**Refresh Rule Rationale:**

- Prevents dice pools from reaching zero
- Goal: Make target unreliable, not useless
- Maintains tactical challenge

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Disoriented] Status Effect Feature Specification

**Target:** DB10 [Disoriented] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)