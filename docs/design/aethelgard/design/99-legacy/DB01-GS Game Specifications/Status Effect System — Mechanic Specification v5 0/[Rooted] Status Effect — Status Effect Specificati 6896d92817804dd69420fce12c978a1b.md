# [Rooted] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Rooted]`

**Category:** Debuff (Positional Control)

**Type:** Movement denial (soft control)

**Summary:**

Primary positional control tool. Complete movement denial but can still attack/use abilities. Can't change position on combat grid. Dictates WHERE actions must be taken from.

**Thematic Framing (Layer 2):**

Positional data locked or overwritten. Physically entangled (chains, pinned, binding energy). Ability to navigate battlefield grid temporarily revoked.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 2 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Refresh (duration resets, cannot be "more rooted")

**Max Stacks:** 1 (refreshes only)

**Visual Icon:** ⛓️ (chains symbol)

---

## III. Mechanical Implementation

### Primary Effect: Movement Denial

**Effect:**

```
Cannot use ANY ability/command that changes position on combat grid
```

**Blocked Actions:**

- `move` (between rows)
- `glitch-dash`
- `climb`, `leap`
- Any special ability with movement component (e.g., Strandhögg's Harrier's Whirlwind)

**Allowed Actions:**

- Attacks from current position
- Abilities without movement
- Defensive actions (Parry, Block)
- Non-movement standard actions

**Rationale:**

- Offensive: Trap dangerous enemy in frontline
- Defensive: Prevent melee brute from reaching back row

**Variable Definitions:**

- **Duration:** 2 rounds (typical)
- **Movement Restriction:** Absolute (no movement possible)

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Battlefield Control Specialists

**Hlekkr-master "Netting Shot" / "Sweeping Fetter":**

- Quintessential rooting master
- Nets and chains
- Can control multiple targets

**Grove-Warden "Entangling Roots":**

- Premier magical root source
- Nature-based binding

**Veiðimaðr "Set Snare":**

- Trap-based root
- Punishes enemy movement

**Skar-Horde Aspirant "Impaling Spike":**

- High-damage + root combo
- Physical pinning

---

### Enemy Sources

- Nets, chains, bolas
- Entangling vines
- Pinning attacks

---

## V. Resistance & Immunity

### Resistance Check

**Resistance Type:** Physical Resolve (STURDINESS-based)

**DC Range:** 11-15 (set by ability, typically DC 13)

**Success:** Resist, no [Rooted] applied

**Failure:** Apply [Rooted] effect

**Rationale:** Physical ability to break bonds

---

### Immunity

**Immune Entities:**

- **Non-corporeal:** Forlorn (no physical body to root)
- **Flying creatures:** Not on ground, can't be rooted
- **Colossal creatures:** Too massive to restrain

**Resistant Entities:**

- **Very large/heavy:** Ogryns (hard to hold)
- **Very agile:** Hard to get grip on

---

## VI. Cleansing Methods

### The Struggle Action (Universal Counter)

**Special Action: `struggle`**

- Any character can attempt to break free
- Costs Standard Action (entire turn)
- MIGHT-based check vs. Root DC
- Success: Immediately remove [Rooted]

**Rationale:** Universal counter ensures never permanent death sentence. Meaningful tactical choice: attack from rooted position or sacrifice turn for mobility?

---

### Active Cleansing

**Cleanse Type:** Physical

**Abilities:** Certain abilities can cleanse [Rooted] from allies

**Ineffective Methods:**

- Alchemical cleanses - NO EFFECT
- Most magical cleanses - NO EFFECT
- Physical bonds require physical solution

---

## VII. Tactical Implications

### The Controller's Keystone

**Primary Use Case:** Dictate terms of engagement

**Strategic Applications:**

- Isolate dangerous targets
- Protect vulnerable allies
- Turn combat grid into weapon

---

### The Anti-Skirmisher Tool

**Absolute Hard Counter:**

- Shuts down high-mobility enemies
- Neutralizes KE-focused builds (Strandhögg, Iron-Husk Rider)
- Denies resource generation (KE requires movement)
- Counters hit-and-run tactics

---

### Synergy with AoE

**Most Powerful Combo:**

1. Root group of enemies (Hlekkr-master/Grove-Warden)
2. Unleash devastating AoE on helpless position (Galdr-caster/Brewmaster)
3. Enemies can't escape damage zone

**Makes Control Parties Viable:** Formation control + AoE = high-strategy build

---

### The Meaningful Choice

**Every Application Creates Decision:**

- Attack from rooted position?
- Sacrifice turn to struggle free?
- Depends on: range to target, importance of mobility, remaining duration

**Never Trivial:** 2-round duration means choice matters

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Rooted] Status Effect Feature Specification

**Target:** DB10 [Rooted] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)