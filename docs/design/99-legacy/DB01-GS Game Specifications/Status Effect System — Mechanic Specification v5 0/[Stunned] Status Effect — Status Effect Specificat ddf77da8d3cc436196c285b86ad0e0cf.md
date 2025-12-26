# [Stunned] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Stunned]`

**Category:** Debuff (Hard Control)

**Type:** Turn denial with complete vulnerability

**Summary:**

Most potent hard control effect. Character loses entire turn AND becomes completely defenseless. Cannot act, Defense reduced to zero, all incoming attacks guaranteed hits.

**Thematic Framing (Layer 2):**

Critical, temporary system crash. Personal operating system hit with overwhelming concussive force or paralyzing data jolt. All higher functions shut down during system reboot.

---

## II. Effect Classification

**Duration Type:** Fixed rounds (typically 1)

**Duration Value:** 1 round (rarely 2 for high-tier abilities)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** No Stack (cannot be stunned while already stunned)

**Max Stacks:** 1 (no stacking possible)

**Visual Icon:** 💥 (collision symbol)

---

## III. Mechanical Implementation

### Primary Effect: Turn Deletion

**Effect 1 - Complete Action Denial:**

- Character loses **entire turn**
- Cannot perform Standard Actions
- Cannot perform Free Actions (stance changes, etc.)
- Cannot use Reactions (Parry, Block, etc.)

**Effect 2 - Complete Vulnerability:**

```
Defense Score = 0 (reduced to zero while stunned)
Parry/Block = Disabled
All incoming physical attacks = Guaranteed Solid Hit or Critical Hit
```

**Trigger Timing:** Effect active for entire duration

**Rationale:** System completely offline and unresponsive. Creates "burn window" for coordinated party offense.

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Concussive Force Specialists

**Gorge-Maw Ascetic "Shattering Wave":**

- AOE concussive blast
- High STURDINESS Resolve DC
- Premier stun specialist

**Skar-Horde Aspirant "Overcharged Piston Slam":**

- Single-target mechanical stun
- Weapon-stump attack
- High cost, high impact

---

### Secondary: System Disruption

**Scrap-Tinker "EMP Grenade":**

- Stuns mechanical foes specifically
- Lower DC vs. constructs

**Rust-Witch "System Shock":**

- Magical stun targeting constructs
- Arcane system disruption

---

### Enemy Sources

- Large brute-force enemies (Draugr Juggernauts)
- High-tech Undying with shock weapons
- Boss telegraphed attacks (often un-resistable)

---

*Next: Section V - Resistance & Immunity.*

## V. Resistance & Immunity

### Resistance Check

**Resistance Type:** Physical Resolve (STURDINESS-based)

**DC Range:** Very high (typically 16-20)

**Success:** Completely resist, no effect

**Failure:** Apply [Stunned]

**Rationale:** Most powerful effect requires highest resistance DC

---

### Immunity

**Immune Entities (Critical Balancing):**

- **All Bosses:** Cannot be stunned (prevents trivializing encounters)
- **Colossal creatures:** Too massive to stun
- **Purpose:** Ensures major encounters remain challenging

**Resistant Entities:**

- **Champion/Elite enemies:** Can be stunned but higher DC

---

## VI. Cleansing Methods

### Active Cleansing

**Cleanse Type:** None (standard methods)

**No Standard Cleanse Available:**

- No abilities can cleanse [Stunned]
- No consumables remove it
- Duration too short to warrant cleanse

**Legendary Exception:**

- Potential [Artifact] or [Myth-Forged] item
- Incredibly powerful and rare
- Unique property to cleanse stun

---

### Natural Expiration

**Only Removal Method:** Effect expires when duration reaches 0

**Rationale:** By the time ally could cleanse, effect would expire naturally

---

## VII. Tactical Implications

### The Ultimate Turn-Denial

**Primary Use Case:** Create critical burst window

**Tactical Applications:**

- Burst down dangerous enemy
- Heal party without reprisal
- Interrupt boss channeled spell

---

### High Cost, High Reward

**Balancing Factors:**

- Very high resource costs (Stamina/AP)
- Low success rate vs. powerful foes
- Long cooldowns
- Boss immunity

**No Stack Rule Impact:** Prevents "stunlock" chains that trivialize encounters

---

### Different Enemy Tiers

**Champion/Elite enemies:** CAN be stunned (control-focused tactics)

**Bosses:** IMMUNE to stun (must engage with mechanics)

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Stunned] Status Effect Feature Specification

**Target:** DB10 [Stunned] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)