# [Corroded] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Corroded]` (also called "Rust" for machines)

**Category:** Debuff (Armor Shredding + DOT)

**Type:** Arcane DOT with permanent Soak reduction

**Summary:**

Premier armor-shredding effect. Deals Arcane damage over time AND permanently reduces Soak each turn. One-two punch: consistent pressure + progressive vulnerability. Stacks up to 3 times.

**Thematic Framing (Layer 2):**

Direct application of Runic Blight's unmaking entropy to target's source code. Not biological poison—active de-compilation process. Hardware (armor, hide, form) overwritten with decay logic, crumbling to dust.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 3 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Intensify (increases both damage AND Soak reduction per stack)

**Max Stacks:** 3

**Visual Icon:** 🪠 (rust/decay symbol)

---

## III. Mechanical Implementation

### Primary Effect: Dual Degradation

**Effect 1 - Damage Over Time:**

```
Damage per turn = Base Damage × Current Stacks
Base Damage = 3 (Arcane, typically)
```

**Effect 2 - Soak Reduction (PERMANENT for combat):**

```
Soak Reduction per turn = -1 per stack
Modified Soak = Current Soak - (1 × Current Stacks)
```

**Critical Detail:** Soak reduction is **permanent for remainder of combat**. It does NOT reset when [Corroded] expires.

**Trigger Timing:**

1. Start of affected character's turn: Apply DoT damage
2. After damage: Reduce Soak permanently

**Variable Definitions:**

- **Current Stacks:** Number of [Corroded] stacks (range: 1-3)
- **Base Damage:** Arcane damage per stack (typically 3)
- **Soak Reduction:** -1 Soak per stack per turn (cumulative and permanent)

**Value Ranges:**

- 1 stack: 3 Arcane damage, -1 Soak per turn
- 2 stacks: 6 Arcane damage, -2 Soak per turn
- 3 stacks (max): 9 Arcane damage, -3 Soak per turn

**Example Over Time:**

```
Turn 1: [Corroded x2] applied
  - Take 6 damage, Soak reduced by 2 (Soak: 10 → 8)
Turn 2: [Corroded x2] continues
  - Take 6 damage, Soak reduced by 2 (Soak: 8 → 6)
Turn 3: [Corroded x2] expires
  - Take 6 damage, Soak reduced by 2 (Soak: 6 → 4)
  - Effect ends, but Soak remains at 4 for rest of combat
```

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Rust-Witch Abilities

**"Corrosive Curse":**

- Primary corrosion application ability
- Applies 1-2 stacks based on tier
- Resistance: WILL Resolve Check

**"Flash Rust":**

- Instant, high-potency application
- Targets mechanical foes specifically

---

### Secondary: Alchemical Tools

**Myr-Stalker "Corrosive Slime":**

- Alchemical application method
- Single potent stack
- Area effect option

---

### Enemy Sources

- Rare, powerful Blighted creatures
- Ancient cursed traps
- Direct Runic Blight exposure

---

*Next: Section V - Resistance & Immunity.*

## V. Resistance & Immunity

### Resistance Check

**Resistance Type:** Mental Resolve (WILL-based)

**DC Range:** 12-16 (set by ability)

**Success:** Completely resist, no corrosion applied

**Failure:** Apply [Corroded] effect

**Rationale:** Metaphysical curse targets willpower to resist

---

### Vulnerability

**Vulnerable Entities:**

- **All Jötun-Forged:** Mechanical constructs especially susceptible
- **All Undying:** Hardware-based existence vulnerable
- **Effect:** Penalty to Resolve Check to resist

---

## VI. Cleansing Methods

### Active Cleansing

**Cleanse Type:** Arcane (most difficult to cleanse)

**Primary Method: Vard-Warden's "Cleansing Ward"**

- High-tier arcane ability required
- Overwrites corrupted code with coherent script

**Ineffective Methods:**

- Physical treatments (Apply Tourniquet) - NO EFFECT
- Alchemical cures (Common Antidote) - NO EFFECT
- Standard healing - NO EFFECT

**Rationale:** Corrupted code must be overwritten by superior arcane script

---

### Soak Reduction Permanence

**Critical Note:** Even if [Corroded] is cleansed:

- DoT damage stops
- No further Soak reduction occurs
- **Already reduced Soak does NOT restore**
- Soak remains reduced for remainder of combat

---

## VII. Tactical Implications

### The Ultimate Anti-Tank Tool

**Primary Use Case:** Hard counter to high-STURDINESS, heavy-armor enemies

**Strategic Role:** Rust-Witch/Myr-Stalker become vital for dismantling tanks

**Synergy with [Poisoned]:**

- [Poisoned] reduces Defense (easier to hit)
- [Corroded] reduces Soak (more damage through)
- Combined: Target completely dismantled

---

### Battle of Attrition

**Against Players:**

- Terrifying threat forcing desperate choices
- Win quickly before defenses gone?
- Dedicate resources to cleansing?

**Progressive Vulnerability:**

- Each turn makes target weaker
- Permanent Soak loss creates snowball effect
- 3 stacks over 3 turns = -9 Soak (devastating)

---

### Systemic Integration

**Rust-Witch Identity:** Undisputed master of armor-shredding

**Party Composition:** Essential for dealing with tank enemies

**Physical Damage Synergy:** All party physical attacks benefit from reduced Soak

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Corroded] Status Effect Feature Specification

**Target:** DB10 [Corroded] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)