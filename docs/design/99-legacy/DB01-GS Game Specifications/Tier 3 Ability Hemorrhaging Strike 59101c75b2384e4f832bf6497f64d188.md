# Tier 3 Ability: Hemorrhaging Strike

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BERSERKR-HEMORRHAGINGSTRIKE-v5.0
Parent item: Berserkr (The Roaring Fire) — Specialization Specification v5.0 (Berserkr%20(The%20Roaring%20Fire)%20%E2%80%94%20Specialization%20Speci%20edde253a7f9d46ff8e936655162ef4f7.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 3
Template Validated: No
Voice Validated: No

## Core Identity

**Specialization:** Berserkr (The Roaring Fire)

**Ability ID:** 1107

**Tier:** 3 (Ultimate Fury)

**Type:** Active (Standard Action)

**Prerequisite:** 20 PP invested in Berserkr tree

**Cost:** 5 PP

---

## Description

You drive your weapon deep into an enemy's vital point with surgical precision, twisting to maximize trauma. The wound channels psychic energy that prevents natural healing, causing continuous hemorrhaging that cannot be stopped.

This is not the wild violence of Whirlwind—this is a **deliberate, calculated strike** designed to inflict a wound that will not heal. Beneath the rage is a predator's instinct for finding vital points and exploiting them ruthlessly.

---

## Mechanics

### Core Formulas

```jsx
BurstDamage = Roll(Nd10) + MIGHT
BleedingDoT = Roll(Xd10) per turn, Duration turns
AntiHeal = Target cannot heal while [Bleeding]

By Rank:
  Rank 2: 4d10 burst, 2d10/turn for 2 turns, 45 Sta + 35 Fury
  Rank 3: 5d10 burst, 3d10/turn for 3 turns, 45 Sta + 40 Fury, anti-heal
```

### Progression & Milestones

**Rank 2 (Starting — when ability is learned)**

- **Cost:** 45 Stamina + 35 Fury
- **Action:** Standard Action
- **Range:** Melee (single target)
- **Attack:** MIGHT-based melee attack vs Defense
- **Burst Damage:** 4d10 + MIGHT Physical damage
- **[Bleeding]:** Target afflicted with [Bleeding] (2d10 damage/turn, 2 turns)
- **Average Total:** ~22 burst + ~22 DoT = **~44 damage**

**Rank 3 (Mastery — Capstone trained)**

- **Cost:** 45 Stamina + 40 Fury
- **Burst Damage:** 5d10 + MIGHT Physical damage
- **[Hemorrhaging]:** Target afflicted with [Bleeding] (3d10 damage/turn, 3 turns)
- **Anti-Heal:** Target cannot benefit from healing or regeneration while [Bleeding]
- **Average Total:** ~27.5 burst + ~49.5 DoT = **~77 damage**

---

## Worked Examples

### Example 1: Standard Execution (Rank 2)

> **Situation:** Elite enemy at 60% HP. Berserkr has MIGHT 5, 50 Fury.
> 

> 
> 

> **Cost:** -45 Stamina, -35 Fury (now 15 Fury)
> 

> 
> 

> **Attack Roll:** MIGHT vs Defense 13 → Roll 16 → **Hit**
> 

> 
> 

> **Burst Damage:** Roll 4d10: [7, 5, 8, 6] = 26 + 5 = **31 Physical damage**
> 

> 
> 

> **Bleeding Applied:** [Bleeding] for 2 turns
> 

> - Turn 1: 2d10 = [6, 8] = **14 damage**
> 

> - Turn 2: 2d10 = [5, 7] = **12 damage**
> 

> 
> 

> **Total Damage:** 31 + 26 = **57 damage** from one ability
> 

### Example 2: Anti-Regeneration Value (Rank 3)

> **Situation:** Fighting regenerating troll boss (15 HP/turn regen). MIGHT 6.
> 

> 
> 

> **Cost:** -45 Stamina, -40 Fury
> 

> 
> 

> **Without Hemorrhaging Strike:** Boss heals 45 HP over 3 turns.
> 

> 
> 

> **With Hemorrhaging Strike:**
> 

> - Burst: 5d10 + 6 = ~33 damage
> 

> - DoT: ~50 damage over 3 turns
> 

> - Healing prevented: 45 HP
> 

> - **Effective value:** ~128 damage equivalent
> 

### Example 3: Death or Glory Combo (Rank 3)

> **Situation:** Berserkr at 45% HP ([Bloodied]), Death or Glory active (+5 damage, +75% Fury gen, 18-20 crit).
> 

> 
> 

> **Hemorrhaging Strike:**
> 

> - Burst: 5d10 + MIGHT + 5 = ~38 damage
> 

> - Crit chance: 20% (18-20 on d20)
> 

> - If crit: Double damage = ~76 burst
> 

> 
> 

> **Result:** Bloodied Berserkr deals amplified damage with expanded crit range.
> 

---

## Failure Modes

### Attack Misses

- **Cost paid:** 45 Stamina + 35/40 Fury consumed
- **Bleeding:** Not applied (requires hit)
- **Recovery:** Significant resource loss; rebuild Fury before retry

### Target Dies Before Bleed Completes

- **Remaining DoT:** Wasted (target already dead)
- **Strategy:** Don't use on low-HP targets; save for high-HP priorities

### Target Immune to Bleeding

- **Burst still applies:** 4-5d10 + MIGHT damage dealt
- **DoT negated:** Some constructs/undead may resist [Bleeding]
- **Value:** Reduced but not zero

---

## Tactical Applications

### Boss Execution

Primary finisher vs single high-value targets:

- **Rank 2:** ~44 total damage
- **Rank 3:** ~77 total damage + anti-heal
- Best used when target will survive initial burst

### Anti-Regeneration (Rank 3)

Shuts down healing enemies:

- Trolls with regeneration
- Bosses with heal phases
- Enemies with lifesteal
- Prevents 30-60 HP recovery over duration

### Whirlwind Alternative

Same tier, different use case:

- **Whirlwind:** AoE burst, multiple targets, friendly fire risk
- **Hemorrhaging Strike:** Single-target burst + DoT, anti-heal, safe

### Fury Dump

When at high Fury with single priority target:

- Spend 35-40 Fury for ~44-77 damage
- Better than holding excess Fury near cap

---

## Integration Notes

**Role:** Single-target finisher and anti-heal tool. Boss killer.

**Design Philosophy:**

- Counterpart to Whirlwind (single-target vs AoE)
- Higher total damage through sustained DoT
- Anti-heal utility unlocks at Rank 3 for specific encounters
- Safe ability (no friendly fire, no self-debuff)

**Notable Synergies:**

- **Death or Glory** (Tier 3): +5 damage while [Bloodied], 20% crit
- **Blood-Fueled** (Tier 2): Generate Fury quickly for next cast
- **[Furious]** (Blood-Fueled R3): +4 MIGHT amplifies burst
- **Unleashed Roar** (Tier 2): Build Fury via taunted attacks

**Related Abilities:**

- Whirlwind of Destruction (Tier 2): AoE alternative
- Reckless Assault (Tier 1): Lower-cost single-target option
- Death or Glory (Tier 3): Amplifies burst damage