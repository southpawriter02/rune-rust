# Tier 1 Ability: Reckless Assault

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BERSERKR-RECKLESSASSAULT-v5.0
Parent item: Berserkr (The Roaring Fire) — Specialization Specification v5.0 (Berserkr%20(The%20Roaring%20Fire)%20%E2%80%94%20Specialization%20Speci%20edde253a7f9d46ff8e936655162ef4f7.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 1
Template Validated: No
Voice Validated: No

## Core Identity

**Specialization:** Berserkr (The Roaring Fire)

**Tier:** 1 (Foundational Fury)

**Type:** Active (Standard Action)

**Prerequisite:** Unlock Berserkr specialization

**Cost:** 3 PP

---

## Description

Lowering your guard completely, you lunge forward with all your might, channeling a spike of raw psychic trauma into a powerful, single-target attack made with absolutely no regard for personal safety.

You strike with overwhelming force, but leave yourself dangerously exposed to counterattack. This is the Berserkr's philosophy embodied: **trade safety for power**.

---

## Mechanics

### Core Formulas

```
Damage = Roll(Nd10) + MIGHT
FuryGained = FuryBase (on hit only)
VulnerablePenalty = DamageTaken × Multiplier
StressChance = Roll d100 ≤ Threshold → +1 Psychic Stress

By Rank:
  Rank 1: 4d10, +15 Fury, +25% Vulnerable, 50% Stress chance
  Rank 2: 5d10, +18 Fury, +20% Vulnerable, 35% Stress chance
  Rank 3: 6d10, +20 Fury, +15% Vulnerable, 25% Stress chance
```

### Progression & Milestones

**Rank 1 (Foundational — 0 PP)**

- **Unlocked:** Immediately upon learning Reckless Assault
- **Cost:** 35 Stamina
- **Action:** Standard Action
- **Range:** Melee (single target)
- **Attack:** MIGHT-based melee attack vs Defense
- **Damage:** 4d10 + MIGHT Physical damage
- **Fury Generation:** +15 Fury on hit
- **Drawback:** Apply [Vulnerable] to self for 1 turn (+25% damage taken)
- **Trauma Risk:** 50% chance to gain +1 Psychic Stress on use

**Rank 2 (Expert — 2 Tier 2 abilities trained)**

- **Cost:** 35 Stamina
- **Damage:** 5d10 + MIGHT Physical damage
- **Fury Generation:** +18 Fury on hit
- **Drawback:** [Vulnerable] reduced to +20% damage taken
- **Trauma Risk:** 35% chance to gain +1 Psychic Stress on use

**Rank 3 (Mastery — Capstone trained)**

- **Cost:** 30 Stamina (reduced from 35)
- **Damage:** 6d10 + MIGHT Physical damage
- **Fury Generation:** +20 Fury on hit
- **Drawback:** [Vulnerable] reduced to +15% damage taken
- **Trauma Risk:** 25% chance to gain +1 Psychic Stress on use
- **Weaponized Vulnerability:** If attacked while [Vulnerable] from this ability, gain +10 bonus Fury

---

## Worked Examples

### Example 1: High-Risk Opener (Rank 1)

> **Situation:** Solo boss fight begins. Berserkr has MIGHT 5, Stress 2/20.
> 

> 
> 

> **Attack Roll:** MIGHT attack vs Defense 14 → Roll 17 → **Hit**
> 

> 
> 

> **Damage:** Roll 4d10: [8, 6, 9, 4] = 27 + 5 = **32 Physical damage**
> 

> 
> 

> **Fury Gained:** +15 Fury
> 

> 
> 

> **Stress Check:** Roll d100: 43 → 43 ≤ 50 → **+1 Psychic Stress** (now 3/20)
> 

> 
> 

> **Status Applied:** [Vulnerable] for 1 turn (+25% damage taken)
> 

> 
> 

> **Risk Assessment:** If boss deals 20 damage next turn → Berserkr takes 25 damage instead.
> 

### Example 2: Blood-Fueled Synergy (Rank 2)

> **Situation:** Berserkr at 40 Fury, has Blood-Fueled passive. Takes hit while [Vulnerable].
> 

> 
> 

> **Enemy Attack:** 18 base damage × 1.20 (Vulnerable) = **21.6 → 22 damage taken**
> 

> 
> 

> **Blood-Fueled Trigger:** 22 damage × 2 (doubled) = **+44 Fury from damage**
> 

> 
> 

> **Net Fury:** Started at 40, gained 18 from Reckless Assault, gained 44 from damage = **102 Fury (capped at 100)**
> 

> 
> 

> **Result:** [Vulnerable] became an asset, not a liability.
> 

### Example 3: Weaponized Vulnerability (Rank 3)

> **Situation:** Rank 3 Berserkr uses Reckless Assault, then gets attacked while [Vulnerable].
> 

> 
> 

> **Reckless Assault:** Hit → +20 Fury, [Vulnerable] applied (+15%)
> 

> 
> 

> **Enemy Attacks:** Boss strikes Berserkr.
> 

> 
> 

> **Weaponized Vulnerability:** +10 bonus Fury (Rank 3 effect)
> 

> 
> 

> **Blood-Fueled (if active):** Additional Fury from damage taken
> 

> 
> 

> **Total Fury Gained:** 20 (ability) + 10 (Weaponized) + damage Fury = **30+ Fury from one exchange**
> 

---

## Failure Modes

### Attack Misses

- **Cost paid:** 35 Stamina (30 at Rank 3) consumed
- **Fury gained:** 0 (Fury only on hit)
- **Vulnerable:** Still applied (you committed to the reckless stance)
- **Stress check:** Still rolled (the psychic strain occurs regardless)

### Already at Stress Threshold

- **Stress accumulation:** If at 19/20 Stress and roll triggers +1, reach 20 → Resolve Check required
- **Mitigation:** Consider holding Reckless Assault when near Breaking Point

---

## Tactical Applications

### Burst Fury Generation

Best single-target Fury generator. +15/+18/+20 Fury per hit vs Wild Swing's +5/+7/+10 per enemy.

### Boss Fights

Excellent vs solo bosses. [Vulnerable] less dangerous when only 1 attacker can exploit it.

### Death or Glory Synergy

When [Bloodied], +50% Fury generation:

- **Rank 1 + Bloodied:** 15 × 1.5 = +22 Fury
- **Rank 3 + Bloodied:** 20 × 1.5 = +30 Fury

### Blood-Fueled Combo

With Blood-Fueled passive, [Vulnerable] becomes an asset:

- Take more damage → Generate more Fury from Blood-Fueled
- Weaponized Vulnerability (Rank 3) adds +10 Fury on top

### Stress Management

Track cumulative Stress from repeated use:

- **Rank 1:** ~2-3 Stress per 5 uses (50% chance)
- **Rank 3:** ~1-2 Stress per 5 uses (25% chance)

---

## Integration Notes

**Role:** High-risk single-target Fury generator. Embodies Berserkr philosophy of trading safety for power.

**Risk/Reward Balance:**

- **Damage:** Highest for Tier 1 (4d10–6d10)
- **Fury:** Best single-target generation
- **Cost:** [Vulnerable] penalty (reduces with mastery)
- **Trauma Risk:** Variable Stress accumulation (reduces with mastery)

**Design Philosophy:**

- Heretical ability that directly interfaces with Trauma Economy
- Risk decreases as Berserkr masters the technique
- Weaponized Vulnerability rewards leaning into the danger

**Notable Synergies:**

- **Primal Vigor** (Tier 1): Fury generated activates regen thresholds
- **Blood-Fueled** (Tier 2): [Vulnerable] → more damage → more Fury
- **Death or Glory** (Tier 3): [Bloodied] amplifies Fury generation
- **Unleashed Roar** (Tier 2): Taunt + Weaponized Vulnerability = Fury engine

**Anti-Synergies:**

- **High-Stress situations:** Avoid when near Breaking Point
- **Multi-enemy fights:** Multiple attackers can exploit [Vulnerable]

**Related Abilities:**

- Primal Vigor (Tier 1): Benefits from Fury generated
- Wild Swing (Tier 1): Safer AoE alternative
- Blood-Fueled (Tier 2): Transforms [Vulnerable] into advantage
- Unleashed Roar (Tier 2): Taunt synergy with Weaponized Vulnerability