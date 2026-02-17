# Tier 1 Ability: Primal Vigor I

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BERSERKR-PRIMALVIGOR1-v5.0
Parent item: Berserkr (The Roaring Fire) — Specialization Specification v5.0 (Berserkr%20(The%20Roaring%20Fire)%20%E2%80%94%20Specialization%20Speci%20edde253a7f9d46ff8e936655162ef4f7.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 1
Template Validated: No
Voice Validated: No

## Core Identity

**Specialization:** Berserkr (The Roaring Fire)

**Tier:** 1 (Foundational Fury)

**Type:** Passive (Always Active)

**Prerequisite:** Unlock Berserkr specialization

**Cost:** 3 PP

---

## Description

Your very physiology is tied to your rage. As your fury builds, your body surges with adrenaline and tainted psychic energy. Your heart pounds faster, your breathing deepens, your muscles recover with unnatural speed.

The more of the Great Silence's trauma you channel, the faster your body regenerates the energy to continue the carnage. Rage sustains you. Rage **is** you.

---

## Mechanics

### Core Formula

```
StaminaRegen = Floor(Fury / 25) × RegenPerStack

Where RegenPerStack:
  Rank 1: 2
  Rank 2: 3
  Rank 3: 4
```

### Progression & Milestones

**Rank 1 (Foundational — 0 PP)**

- **Unlocked:** Immediately upon learning Primal Vigor
- **Effect:** Gain scaling Stamina regeneration based on current Fury:
    - **25+ Fury:** +2 Stamina per turn
    - **50+ Fury:** +4 Stamina per turn
    - **75+ Fury:** +6 Stamina per turn
    - **100 Fury:** +8 Stamina per turn
- **Always active**, no additional cost

**Rank 2 (Expert — 2 Tier 2 abilities trained)**

- **25+ Fury:** +3 Stamina per turn
- **50+ Fury:** +6 Stamina per turn
- **75+ Fury:** +9 Stamina per turn
- **100 Fury:** +12 Stamina per turn

**Rank 3 (Mastery — Capstone trained)**

- **25+ Fury:** +4 Stamina per turn
- **50+ Fury:** +8 Stamina per turn
- **75+ Fury:** +12 Stamina per turn
- **100 Fury:** +16 Stamina per turn
- **Bonus:** At exactly 100 Fury, additionally gain **+2 HP regeneration per turn**

---

## Worked Examples

### Example 1: Early Combat Ramp (Rank 1)

> **Situation:** Berserkr at 0 Fury, takes 30 HP damage from enemy attack.
> 

> 
> 

> **Calculation:**
> 

> - Fury gained from damage: 30 HP × 1 Fury/HP = **30 Fury**
> 

> - Primal Vigor check: 30 ≥ 25 → **1 stack**
> 

> - Stamina regen bonus: 1 × 2 = **+2 Stamina/turn**
> 

> 
> 

> **Result:** Next turn, Berserkr regenerates base Stamina + 2 bonus.
> 

### Example 2: Mid-Combat Sustain (Rank 2)

> **Situation:** Berserkr at 60 Fury after several exchanges. Has Blood-Fueled passive.
> 

> 
> 

> **Calculation:**
> 

> - Fury stacks: Floor(60 / 25) = **2 stacks**
> 

> - Stamina regen: 2 × 3 = **+6 Stamina/turn**
> 

> 
> 

> **Tactical note:** At 60 Fury, Berserkr can use Wild Swing (40 Stamina) every other turn while maintaining positive Stamina flow if base regen is 4+.
> 

### Example 3: Maximum Output (Rank 3 with 100 Fury)

> **Situation:** Berserkr at 100 Fury during boss fight, Capstone unlocked.
> 

> 
> 

> **Calculation:**
> 

> - Fury stacks: Floor(100 / 25) = **4 stacks**
> 

> - Stamina regen: 4 × 4 = **+16 Stamina/turn**
> 

> - HP regen (100 Fury bonus): **+2 HP/turn**
> 

> 
> 

> **Result:** Berserkr regenerates 16 bonus Stamina AND 2 HP per turn. Over a 10-turn boss fight, this equals **+160 Stamina** and **+20 HP** — effectively a second resource pool.
> 

---

## Tactical Applications

### Fury Sweet Spots

Maintain 50–75 Fury for optimal regen without overcapping. Build to threshold, hold there, spend excess on abilities only when tactically necessary.

### Extended Boss Fights

Excel in 10+ turn encounters. Cumulative bonus over a long fight:

- **Rank 1:** +60–80 Stamina
- **Rank 2:** +90–120 Stamina
- **Rank 3:** +120–160 Stamina + 20 HP (at 100 Fury)

### Spend vs Hold Decision

Sometimes holding Fury is optimal. Calculate: *"If I spend 40 Fury now, I drop from 3 stacks to 1 stack, losing 4–8 Stamina/turn for several turns."* Weigh burst damage vs sustained output.

---

## Integration Notes

**Role:** Sustain passive. Rewards maintaining mid-high Fury for extended combat presence.

**Design Philosophy:**

- Passive reward for Fury accumulation
- Encourages maintaining mid-high Fury over repeated spending
- Rank 3 transforms Berserkr into self-sustaining raid boss with HP regen

**Notable Synergies:**

- **Wild Swing** (Tier 1): Build Fury to activate regen stacks
- **Reckless Assault** (Tier 1): High Fury generation enables sustained regen
- **Blood-Fueled** (Tier 2): Doubled/tripled Fury from damage = faster stack buildup
- **Death or Glory** (Tier 3): +50% Fury gen while Bloodied = more stacks faster
- **Whirlwind of Destruction** (Tier 2): High Stamina cost offset by sustained regen

**Related Abilities:**

- Wild Swing (Tier 1): Fury generator
- Reckless Assault (Tier 1): Fury generator
- All Fury spenders: Balance spending vs holding for regen value