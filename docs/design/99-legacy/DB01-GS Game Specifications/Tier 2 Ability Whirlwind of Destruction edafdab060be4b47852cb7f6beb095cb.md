# Tier 2 Ability: Whirlwind of Destruction

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BERSERKR-WHIRLWIND-v5.0
Parent item: Berserkr (The Roaring Fire) — Specialization Specification v5.0 (Berserkr%20(The%20Roaring%20Fire)%20%E2%80%94%20Specialization%20Speci%20edde253a7f9d46ff8e936655162ef4f7.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 2
Template Validated: No
Voice Validated: No

## Core Identity

**Specialization:** Berserkr (The Roaring Fire)

**Tier:** 2 (Escalating Violence)

**Type:** Active (Standard Action)

**Prerequisite:** 8 PP invested in Berserkr tree

**Cost:** 4 PP

---

## Description

You become a spinning cyclone of destruction, your weapon a blur of steel that strikes everything within reach. In this state of pure, uncontrolled fury, you cannot distinguish between friend and foe—your allies in the front row risk being caught in the maelstrom of violence.

This is not controlled precision. This is the Berserkr at their most dangerous and uncontrollable—**overwhelming destructive power that cannot distinguish friend from foe**.

---

## Mechanics

### Core Formulas

```
Damage = Roll(Nd10) + MIGHT (per target)
FuryGained = FuryPerHit × EnemiesHit
FriendlyFireCheck = Roll d100 ≤ Threshold (per ally in Front Row)

By Rank:
  Rank 2: 3d10, 50 Stamina + 30 Fury, 35% friendly fire, +5 Fury/hit
  Rank 3: 5d10, 45 Stamina + 25 Fury, 15% friendly fire, +8 Fury/hit, [Bleeding] all
```

### Progression & Milestones

**Rank 2 (Starting — when ability is learned)**

- **Cost:** 50 Stamina + 30 Fury
- **Action:** Standard Action
- **Range:** Self (AoE burst)
- **Targeting:** All enemies in Front Row AND Back Row
- **Attack:** MIGHT-based melee attack vs each target's Defense
- **Damage:** 3d10 + MIGHT Physical damage per enemy hit
- **Fury Generation:** +5 Fury per enemy hit
- **Friendly Fire:** 35% chance to hit each ally in Front Row (same damage)

**Rank 3 (Mastery — Capstone trained)**

- **Cost:** 45 Stamina + 25 Fury (reduced)
- **Damage:** 5d10 + MIGHT Physical damage per enemy hit
- **Fury Generation:** +8 Fury per enemy hit
- **Friendly Fire:** 15% chance to hit each ally in Front Row (reduced)
- **Bleeding Maelstrom:** All enemies hit are afflicted with [Bleeding] (2d6 damage/turn, 3 turns)

---

## Worked Examples

### Example 1: Standard AoE Clear (Rank 2)

> **Situation:** 4 enemies (2 Front, 2 Back), 1 ally in Front Row. Berserkr has MIGHT 5, 50 Fury.
> 

> 
> 

> **Cost:** -50 Stamina, -30 Fury (now 20 Fury)
> 

> 
> 

> **Attacks:** Roll vs each enemy's Defense
> 

> - All 4 enemies hit
> 

> 
> 

> **Damage:** Roll 3d10 per target: [5, 8, 6] = 19 + 5 = **24 damage each**
> 

> 
> 

> **Fury Gained:** 4 × 5 = **+20 Fury** (now 40 Fury)
> 

> 
> 

> **Friendly Fire Check:** Roll d100 for ally: 42 → 42 > 35 → **Ally safe**
> 

> 
> 

> **Result:** 96 total damage, net -10 Fury, ally unharmed.
> 

### Example 2: Friendly Fire Incident (Rank 2)

> **Situation:** Same as above, but friendly fire triggers.
> 

> 
> 

> **Friendly Fire Check:** Roll d100: 28 → 28 ≤ 35 → **Ally hit!**
> 

> 
> 

> **Ally Damage:** 3d10 + MIGHT: [7, 4, 9] = 20 + 5 = **25 damage to Bone-Setter**
> 

> 
> 

> **Tactical Note:** Always warn allies before using. Position squishies in Back Row.
> 

### Example 3: Bleeding Maelstrom (Rank 3)

> **Situation:** 5 enemies across both rows. Berserkr has MIGHT 6.
> 

> 
> 

> **Cost:** -45 Stamina, -25 Fury
> 

> 
> 

> **Immediate Damage:** 5d10 + 6 = ~33 damage × 5 = **~165 total damage**
> 

> 
> 

> **Fury Gained:** 5 × 8 = **+40 Fury**
> 

> 
> 

> **Bleeding Applied:** All 5 enemies now [Bleeding]
> 

> - Turn 1: 5 × 2d6 (~7) = **~35 damage**
> 

> - Turn 2: ~35 damage
> 

> - Turn 3: ~35 damage
> 

> 
> 

> **Total Damage:** 165 + 105 = **~270 damage** from one ability
> 

---

## Failure Modes

### All Attacks Miss

- **Cost paid:** Stamina and Fury consumed
- **Fury gained:** 0 (only generates on hits)
- **Friendly fire:** Still checked (you're spinning wildly regardless)

### No Enemies Present

- **Ability unusable:** Cannot activate with no valid targets
- **UI:** Button grayed out

### Ally Killed by Friendly Fire

- **Full damage dealt:** No reduction for allies
- **Consequences:** Ally incapacitated, potential party conflict
- **Mitigation:** Coordinate positioning before combat

---

## Tactical Applications

### Multi-Enemy Optimal

Best value with 4+ enemies:

- **Rank 2:** 4 hits = 20 Fury back (net -10 Fury for ~100 damage)
- **Rank 3:** 4 hits = 32 Fury back (net +7 Fury for ~140 damage + Bleeding)

### Minimize Friendly Fire

- Position allies in Back Row before use
- Communicate with party: "Whirlwind incoming, get back!"
- Rank 3 reduces risk to 15% (manageable)

### Bleeding Cascade (Rank 3)

Immediate burst + sustained DoT:

- **Burst:** ~30-35 damage per target
- **DoT:** ~21 damage per target over 3 turns
- **Total:** ~55 damage per target = exceptional value

### Fury Cycling

Use when Fury near 100 to prevent overcapping:

- Spend 25-30 Fury → Deal massive damage → Regain 20-40 Fury
- Efficient resource management

### Death or Glory Synergy

While [Bloodied], +50% Fury generation:

- **Rank 3 + Bloodied:** 5 hits = 8 × 5 × 1.5 = **+60 Fury**

---

## Integration Notes

**Role:** Primary AoE Fury sink. Ultimate expression of Berserkr's destructive power.

**Fury Economy:**

- **Rank 2:** Cost 30 | Refund 5-40 (1-8 enemies) | Break-even: 6 enemies
- **Rank 3:** Cost 25 | Refund 8-64 (1-8 enemies) | Break-even: 4 enemies

**Design Philosophy:**

- Friendly fire is feature, not bug
- Encourages tactical positioning and party coordination
- Rank progression represents mastery of chaos (better control = less collateral)
- Bleeding at Rank 3 transforms burst into sustained pressure

**Notable Synergies:**

- **Blood-Fueled** (Tier 2): Build Fury quickly to afford this
- **Unleashed Roar** (Tier 2): Taunt to generate Fury for Whirlwind
- **Wild Swing** (Tier 1): Lesser AoE Fury builder → Whirlwind finisher
- **Death or Glory** (Tier 3): Amplifies Fury refund while [Bloodied]
- **Primal Vigor** (Tier 1): High Fury after Whirlwind = strong regen

**Anti-Synergies:**

- **Melee allies:** Risk friendly fire (Skjaldmær, other Warriors)
- **Low enemy count:** Inefficient Fury spend vs single targets

**Related Abilities:**

- Wild Swing (Tier 1): Smaller AoE, Fury generator
- Hemorrhaging Strike (Tier 3): Single-target Bleeding alternative
- Blood-Fueled (Tier 2): Enables rapid Fury accumulation