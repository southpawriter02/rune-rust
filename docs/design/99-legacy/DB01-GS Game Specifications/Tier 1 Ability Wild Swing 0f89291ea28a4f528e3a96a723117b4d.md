# Tier 1 Ability: Wild Swing

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BERSERKR-WILDSWING-v5.0
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

You unleash a wide, reckless swing of your weapon, caring little for precision and focusing only on widespread destruction. Your blade arcs through the air in a brutal, horizontal sweep, aiming to hit everything in front of you.

It's not elegant. It's not refined. It's visceral, effective carnage—exactly what a Berserkr craves.

---

## Mechanics

### Core Formulas

```
Damage = Roll(Nd10) + MIGHT  (per target)
FuryGained = FuryPerHit × EnemiesHit

Where N (dice count) and FuryPerHit by Rank:
  Rank 1: 2d10, +5 Fury/hit
  Rank 2: 3d10, +7 Fury/hit
  Rank 3: 4d10, +10 Fury/hit
```

### Progression & Milestones

**Rank 1 (Foundational — 0 PP)**

- **Unlocked:** Immediately upon learning Wild Swing
- **Cost:** 40 Stamina
- **Action:** Standard Action
- **Range:** Front Row (all enemies)
- **Attack:** MIGHT-based melee attack vs each enemy's Defense
- **Damage:** 2d10 + MIGHT Physical damage per enemy hit
- **Fury Generation:** +5 Fury per enemy hit
    - 1 enemy = +5 Fury
    - 2 enemies = +10 Fury
    - 3 enemies = +15 Fury

**Rank 2 (Expert — 2 Tier 2 abilities trained)**

- **Cost:** 40 Stamina
- **Damage:** 3d10 + MIGHT Physical damage
- **Fury Generation:** +7 Fury per enemy hit
    - 1 enemy = +7 Fury
    - 2 enemies = +14 Fury
    - 3 enemies = +21 Fury

**Rank 3 (Mastery — Capstone trained)**

- **Cost:** 35 Stamina (reduced from 40)
- **Damage:** 4d10 + MIGHT Physical damage
- **Fury Generation:** +10 Fury per enemy hit
    - 1 enemy = +10 Fury
    - 2 enemies = +20 Fury
    - 3 enemies = +30 Fury
- **Back Row Reach:** If Front Row is empty, Wild Swing targets Back Row instead

---

## Worked Examples

### Example 1: Opening Salvo (Rank 1)

> **Situation:** Combat begins. 3 enemies in Front Row. Berserkr has MIGHT 4.
> 

> 
> 

> **Attack Rolls:** Roll vs each enemy's Defense (separate rolls)
> 

> - Enemy 1 (Defense 12): Roll 15 → **Hit**
> 

> - Enemy 2 (Defense 10): Roll 8 → **Miss**
> 

> - Enemy 3 (Defense 11): Roll 14 → **Hit**
> 

> 
> 

> **Damage Calculation (per hit):**
> 

> - Roll 2d10: [6, 8] = 14
> 

> - Add MIGHT: 14 + 4 = **18 Physical damage**
> 

> 
> 

> **Fury Gained:** 2 enemies hit × 5 = **+10 Fury**
> 

> 
> 

> **Result:** 18 damage to 2 enemies, +10 Fury, -40 Stamina
> 

### Example 2: Fury Farming (Rank 2)

> **Situation:** Mid-combat, 3 weak enemies remain in Front Row. Berserkr needs Fury for Whirlwind of Destruction.
> 

> 
> 

> **All 3 attacks hit.**
> 

> 
> 

> **Fury Gained:** 3 × 7 = **+21 Fury**
> 

> 
> 

> **Tactical Note:** Combined with existing 15 Fury, Berserkr now has 36 Fury — enough for Whirlwind (30 Fury cost) next turn.
> 

### Example 3: Back Row Reach (Rank 3)

> **Situation:** Front Row cleared. 2 ranged enemies hiding in Back Row. Berserkr has MIGHT 6.
> 

> 
> 

> **Rank 3 Effect:** Front Row empty → Wild Swing targets Back Row
> 

> 
> 

> **Both attacks hit.**
> 

> 
> 

> **Damage:** Roll 4d10: [3, 7, 9, 5] = 24 + 6 = **30 Physical damage each**
> 

> 
> 

> **Fury Gained:** 2 × 10 = **+20 Fury**
> 

> 
> 

> **Result:** Ranged enemies can no longer hide. 30 damage each, +20 Fury, only -35 Stamina.
> 

---

## Failure Modes

### All Attacks Miss

- **Cost paid:** 40 Stamina (35 at Rank 3) is still consumed
- **Fury gained:** 0 (Fury only generates on hits)
- **Recovery:** Wait for Stamina regen, or use Reckless Assault for guaranteed single-target Fury

### Front and Back Rows Empty

- **Targeting fails:** No valid targets, ability cannot be used
- **UI:** Button grayed out when no enemies present

---

## Tactical Applications

### Multi-Enemy Opener

Ideal Turn 1–2 ability vs multiple enemies. Hit 3 enemies = +15/+21/+30 Fury (Rank 1/2/3). Enables Fury spender use by Turn 3.

### Fury Generation Efficiency

- **Rank 1:** 3 hits = 15 Fury for 40 Stamina (0.375 Fury/Stamina)
- **Rank 2:** 3 hits = 21 Fury for 40 Stamina (0.525 Fury/Stamina)
- **Rank 3:** 3 hits = 30 Fury for 35 Stamina (0.857 Fury/Stamina)

### Death or Glory Synergy

When [Bloodied], generate 50% more Fury:

- **Rank 3 + Bloodied:** 3 hits = 45 Fury for 35 Stamina (1.29 Fury/Stamina)

### Back Row Pressure (Rank 3)

No longer need to wait for tanks to clear Front Row. If allies eliminate front-liners, immediately threaten Back Row casters/archers.

---

## Integration Notes

**Role:** Primary AoE Fury generator. Bread-and-butter ability for multi-enemy encounters.

**Design Philosophy:**

- Low-commitment Fury builder with AoE scaling
- Rewards target-rich environments
- Rank 3 removes positioning limitations

**Notable Synergies:**

- **Primal Vigor** (Tier 1): Fury generated activates regen thresholds
- **Death or Glory** (Tier 3): +50% Fury when [Bloodied]
- **Whirlwind of Destruction** (Tier 2): Build Fury with Wild Swing → spend on Whirlwind
- **Blood-Fueled** (Tier 2): Generate Fury from damage taken while exposed in melee

**Related Abilities:**

- Primal Vigor (Tier 1): Benefits from Fury generated
- Whirlwind of Destruction (Tier 2): Upgraded AoE version (hits all rows at base)
- Death or Glory (Tier 3): Amplifies Fury generation
- Reckless Assault (Tier 1): Single-target alternative with higher per-hit Fury