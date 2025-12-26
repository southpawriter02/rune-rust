# [Inspired] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Inspired]`

**Category:** Buff (Offensive Enhancement)

**Type:** Damage enhancement (pure focused offense)

**Summary:**

Premier damage enhancement buff. Grants +2 bonus dice to damage pools. Pure focused offense. Turns character into temporary engine of destruction.

**Thematic Framing (Layer 2):**

Personal operating system overwritten by powerful, coherent narrative of victory. Skald's saga, alchemical courage, or personal conviction purges doubts. Singular focused intent: strike true and hard.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 3 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Refresh/Overwrite (strongest active, see Section III)

**Max Stacks:** 1 (only strongest instance active)

**Visual Icon:** 🏆 (trophy/victory symbol)

---

## III. Mechanical Implementation

### Primary Effect: The Damage Bonus

**Effect:**

```
Damage Dice Pool: +X dice
```

**Formula:**

```
Inspired Damage Pool = Base Damage Pool + Inspired Bonus
```

**Potency Tiers:**

- **Standard:** +2 dice (most common)
- **High-tier:** +3 dice (Skald capstone abilities)
- **Rare/Powerful:** +4+ dice

**Example:**

```
Weapon: 4d6 base damage
Character MIGHT: 8 (adds 8d6)
Base Pool: 4d6 + 8d6 = 12d6

While [Inspired] (+2):
Inspired Pool: 12d6 + 2d6 = 14d6
```

**Rationale:** Simple, powerful, universally effective. Direct tangible increase to damage output.

**Variable Definitions:**

- **Inspired Bonus:** Set by ability (typically +2 dice)
- **Duration:** 3 rounds (typical)

---

### Special Stacking: Overwrite Logic

**If new instance stronger:**

- Overwrites weaker effect
- Set damage bonus and duration to new values
- Example: +3 overwrites existing +2

**If new instance same/weaker:**

- Refresh duration of existing stronger effect
- Damage bonus unchanged
- Example: +2 applied to active +3 = refresh +3 duration

**Rationale:** Prevents absurd damage stacking. Only strongest "heroic narrative" active.

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Morale & Combat Prowess Specialists

**Skald "Saga of Courage" / "Saga of the Einherjar":**

- Premier [Inspired] master
- Powerful, often party-wide sagas
- Highest potency sources

**Brewmaster "Inspiring Mead":**

- Most common single-target source
- Crafted consumable
- Standard tier: +2 dice

**Thul "Keeper of Oaths":**

- Powerful single-target buff
- [Inspired] + Fear immunity combo

---

### Enemy Sources (Very Rare)

**High-Tier Leader/Chieftain/Skald Enemies:**

- Buff allied enemies
- Signals HIGHEST PRIORITY target for party

**Rationale:** Enemy with [Inspired] allies = focus fire the buffer

---

## V. Application & Dispelling

### Application

**No Resistance Check:** Willing buff, no check required

---

### Dispelling (Enemy Actions)

**Dispel Type:** Mental (Narrative) or Alchemical

**Enemy Dispel Methods:**

- Enemy Mystic "Dispel Magic"
- Enemy Thul/Skald "Demoralizing Diatribe" / "Dirge of Defeat"

**Battle of Narratives:**

- Enemy counter-narrative can overwrite [Inspired]
- Competing stories for control of morale

---

## VI. Tactical Implications

### The Burst Damage Enabler

**Primary Use Case:** Create burst/burn phases

**Most Effective Application:**

- Apply to highest-damage characters (Berserker, Galdr-caster)
- Just before unleashing powerful abilities
- Maximize damage spike

---

### The Counter to Defense

**Offensive Solution:**

- Rust-Witch shreds Soak (defensive counter)
- [Inspired] Berserker brute-forces through defenses (offensive counter)
- Massively increased damage rolls overcome mitigation

---

### Tactical Application Choices

**Limited Sources = Strategic Decisions:**

- Single-target now (Inspiring Mead on main DPS)?
- Party-wide later (Skald saga for entire group)?
- Depends on battle state and phase

---

### The Glass Cannon Effect

**Purely Offensive Benefit:**

- No defensive boost
- No utility benefit
- Character remains vulnerable

**Requires Protection:**

- Must be protected by tanks/controllers
- High damage but high risk
- Full potential requires team support

---

### Balancing Factors

**Power Balanced By:**

- **Temporary nature:** 3 rounds typical
- **Purely offensive:** No survivability increase
- **Limited sources:** Rare/valuable resource
- **Refresh stacking:** Prevents absurd damage stacking

---

## VII. Combat Log Examples

**Application:**

> The Skald's saga of ancient heroes echoes in Gorm's soul! He is **[Inspired]**!
> 

**Attack:**

> Driven by the inspiring tale, Gorm's axe strikes with heroic force! (14d6 damage pool)
> 

**Expiration:**

> The inspiring narrative fades. Gorm is no longer [Inspired].
> 

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Inspired] Status Effect Feature Specification

**Target:** DB10 [Inspired] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)

---

## 🎉 TIER 3 COMPLETE!

**All 12 Status Effect Specifications Migrated:**

- ✅ [Bleeding]
- ✅ [Poisoned]
- ✅ [Stunned]
- ✅ [Feared]
- ✅ [Corroded]
- ✅ [Disoriented]
- ✅ [Slowed]
- ✅ [Rooted]
- ✅ [Silenced]
- ✅ [Fortified]
- ✅ [Hasted]
- ✅ [Inspired]

**TIER 2 & TIER 3 Migration Complete!**