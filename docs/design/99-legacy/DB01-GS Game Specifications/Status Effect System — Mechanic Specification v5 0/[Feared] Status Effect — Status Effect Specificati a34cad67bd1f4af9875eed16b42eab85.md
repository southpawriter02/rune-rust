# [Feared] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Feared]`

**Category:** Debuff (Mental Control)

**Type:** Mental hard control with willpower check

**Summary:**

Powerful mental debuff forcing willpower check each turn. Failure = lose turn entirely. Success = act with -2 dice penalty. Cannot move toward fear source regardless of check outcome.

**Thematic Framing (Layer 2):**

Primal, irrational override command hijacks personal operating system. Logical/tactical subroutines shut down, replaced by single directive: ESCAPE_THREAT. Base survival instincts in control.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 1-2 rounds (typically)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Refresh (duration resets, no "more afraid")

**Max Stacks:** 1 (refreshes only)

**Visual Icon:** 😱 (fearful face)

---

## III. Mechanical Implementation

### Primary Effect: Willpower Check System

**Trigger Timing:** Start of affected character's turn

**Willpower Check:**

```
Resolve Pool = WILL + bonuses
DC = Fear Potency (set by ability, typically 12-16)

If Successes >= DC: Fight through fear (act with penalty)
If Successes < DC: Overcome by panic (lose turn)
```

**Outcome 1 - Check Failed:**

- Character loses **entire turn**
- Cannot take any actions
- Panic overwhelms them

**Outcome 2 - Check Succeeded:**

- Character can act normally
- **-2 dice penalty** to ALL action checks for that turn
- Still fighting through terror

---

### Secondary Effect: Movement Restriction

**Absolute Rule (regardless of willpower check):**

```
Cannot move closer to fear source
Can move sideways or backwards only
Cannot advance forward
```

**Rationale:** Even when acting, terror prevents approach. Powerful formation-breaking tool.

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Psychological Warfare Specialists

**Vargr-Born "Terrifying Howl":**

- Primal dominance display
- AOE fear application
- Quintessential fear master

**Echo-Caller "Echo of Terror":**

- Channels memory of pure terror
- Psychic fear delivery
- High mental potency

**Skald-Screamer "Banshee's Keening":**

- High-tier sonic + psychic attack
- Dual damage + fear application

---

### Enemy Sources

- Large, terrifying Blighted beasts
- Powerful Undying with psychic transmitters
- All Forlorn (masters of terror)

---

*Next: Section V - Resistance & Immunity.*

## V. Resistance & Immunity

### Resistance Check

**Resistance Type:** Mental Resolve (WILL-based)

**DC Range:** 12-16 (set by ability)

**Success:** Completely resist, no fear applied

**Failure:** Apply [Feared] effect

---

### Immunity

**Immune Entities:**

- **Mindless Mechanical Constructs:** Jötun-Forged, Undying (no emotion processing)
- **Creatures of Pure Terror:** Forlorn (cannot fear what IS fear)

**Resistant Entities:**

- **Disciplined/Fanatical:** Iron-Banes, zealots (trained resistance)

---

## VI. Cleansing Methods

### Active Cleansing

**Cleanse Type:** Mental

**Primary Method: Skald's "Saga of Courage"**

- Removes [Feared] and provides resistance
- Restores coherent narrative

**Secondary Methods:**

- **Thul's logical guidance:** Bonus to willpower checks
- **Bone-Setter's "Stabilizing Draught":** Bonus to overcome checks

**Ineffective Methods:**

- Physical cleanses (bandages, tourniquets)
- Alchemical cleanses (antidotes)

---

## VII. Tactical Implications & Trauma Economy

### The Ultimate Control Tool

**Power vs. Balance:**

- Can potentially deny entire turns
- Shorter duration balances power
- Many dangerous foes immune (machines)

---

### Trauma Economy Integration

**Psychic Stress Impact:**

- Applying [Feared] **ALSO inflicts Psychic Stress**
- Even if Fear resisted, Stress still applied
- Makes fear-based enemies threat to long-term sanity

---

### The Role of WILL

**WILL Becomes Critical:**

- Not just for Mystics anymore
- Low-WILL Warriors vulnerable despite physical might
- Party liability if easily broken by fear

**Strategic Consideration:** Every character needs minimum WILL investment

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Feared] Status Effect Feature Specification

**Target:** DB10 [Feared] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)