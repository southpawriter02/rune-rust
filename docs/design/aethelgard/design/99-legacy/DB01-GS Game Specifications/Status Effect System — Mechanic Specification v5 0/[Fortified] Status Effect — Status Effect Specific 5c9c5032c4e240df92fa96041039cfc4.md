# [Fortified] Status Effect — Status Effect Specification v5.0

## I. Status Effect Overview

**Name:** `[Fortified]`

**Category:** Buff (Defensive Enhancement)

**Type:** Soak enhancement (passive damage mitigation)

**Summary:**

Premier Soak enhancement buff. Grants significant flat bonus to Soak value. Pure passive damage mitigation. Turns character into temporary bastion of defense.

**Thematic Framing (Layer 2):**

Hardware reinforcement subroutine. Skin turns to stone, armor hardens alchemically, or psionic field deflects kinetic energy. Physical form becomes unnaturally resilient.

---

## II. Effect Classification

**Duration Type:** Fixed rounds

**Duration Value:** 3 rounds (typical)

**Duration Decrement:** At start of affected character's turn

**Stacking Rules:** Refresh/Overwrite (strongest active, see Section III)

**Max Stacks:** 1 (only strongest instance active)

**Visual Icon:** 🛡️ (shield)

---

## III. Mechanical Implementation

### Primary Effect: The Soak Bonus

**Effect:**

```
Soak Value: +X (flat bonus)
```

**Formula:**

```
Modified Soak = Base Soak + Fortified Bonus
```

**Potency Tiers:**

- **Standard:** +3 Soak (Stoneskin Stout)
- **High-tier:** +5 Soak (Vard-Warden spell)
- **Boss/Elite:** +7+ Soak (powerful effects)

**Rationale:** Simple, powerful, universally effective. Reduces damage from majority of attacks.

**Variable Definitions:**

- **Fortified Bonus:** Set by ability (typically +3 to +5)
- **Duration:** 3 rounds (typical)

---

### Special Stacking: Overwrite Logic

**If new instance stronger:**

- Overwrites weaker effect
- Set Soak bonus and duration to new values
- Example: +5 overwrites existing +3

**If new instance same/weaker:**

- Refresh duration of existing stronger effect
- Soak bonus unchanged
- Example: +3 applied to active +5 = refresh +5 duration

**Rationale:** Prevents absurd Soak stacking. Only strongest "reinforcement subroutine" active.

---

*Migration in progress. Remaining sections to be added incrementally.*

## IV. Application Methods

### Primary: Support Specialists & Artisans

**Brewmaster "Stoneskin Stout":**

- Most common/reliable source
- Crafted consumable
- Standard tier: +3 Soak

**Vard-Warden "Glyph of Sanctuary":**

- Premier magical source
- High-tier spell
- Can apply to entire party: +5 Soak

**Skjaldmær "Oath of the Protector":**

- Single-target buff
- Protective aura
- Tank-focused application

---

### Enemy/Boss Usage

**High-Tier Defensive Enemies:**

- Undying Juggernaut
- Clan Oathsworn
- Heavily armored bosses

**Signal to Party:** Switch to armor-piercing tactics or wait out buff

---

## V. Application & Dispelling

### Application

**No Resistance Check:** Willing buff, no check required

---

### Dispelling (Enemy Actions)

**Dispel Type:** Arcane or Alchemical

**Enemy Dispel Methods:**

- Enemy Mystic "Dispel Magic" ability
- Rust-Witch curse (strips fortification before applying [Corroded])

**Rationale:** Powerful defensive buff = priority dispel target for intelligent enemies

---

## VI. Tactical Implications

### The Tank's Foundation

**Core Tank Buff:**

- Foundational for tank builds
- Pushes Soak to boss-damage mitigation levels
- Enables reliable tanking

---

### Countering Burst Damage

**Proactive Defense:**

- Prepare for high-damage phase
- Boss telegraphs massive attack
- Apply [Fortified] to party
- Difference between survival and wipe

---

### Hard Counters to [Fortified]

**[Corroded] Status Effect:**

- Ultimate anti-tank/anti-fortification tool
- Directly shreds Soak
- Rock-paper-scissors dynamic

**[Armor Piercing] Property:**

- Abilities designed to bypass Soak
- Ignores [Fortified] bonus
- Tactical counter option

---

### Limitations (Balance)

**Only Affects Physical Damage Pipeline:**

- Does NOTHING vs. Psychic Damage
- Does NOTHING vs. effects bypassing Soak
- Temporary nature limits power
- Powerful but not universal "I win" button

---

## VII. Integration with Other Systems

### Synergy

**Works well with:**

- High base Soak (heavy armor)
- STURDINESS-focused builds
- Defensive stances

---

### Anti-Synergy

**Countered by:**

- [Corroded] (reduces Soak over time)
- Armor Piercing abilities
- Psychic damage (bypasses entirely)

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 [Fortified] Status Effect Feature Specification

**Target:** DB10 [Fortified] Status Effect — Status Effect Specification v5.0

**Status:** ✅ Draft Complete

**Parent Spec:** [Status Effect System — Mechanic Specification v5.0](../Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200%20903cf8feb4084a9c9b05efcacd7c89fb.md)