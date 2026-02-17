# Tier 2 Ability: Unleashed Roar

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BERSERKR-UNLEASHEDROAR-v5.0
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

You let out a terrifying, guttural war cry, challenging a single foe to face your wrath. The sound is primal, savage—a promise of violence that demands their attention.

This is a hybrid offensive/defensive ability that converts incoming attacks into Fury fuel.

---

## Mechanics

### Core Formulas

```
TauntDuration = DurationByRank rounds
OnTauntedAttack: Berserkr.Fury += FuryPerAttack
OnTauntedDeath: Berserkr.AddStatus("Empowered", 1 round)  [Rank 3 only]
FirstAttackDamage = BaseDamage × 0.80  [Rank 3 only]

By Rank:
  Rank 2: 30 Stamina + 20 Fury, 2 rounds, +10 Fury/attack
  Rank 3: 25 Stamina + 15 Fury, 3 rounds, +15 Fury/attack, -20% first attack, [Empowered] on kill
```

### Progression & Milestones

**Rank 2 (Starting — when ability is learned)**

- **Cost:** 30 Stamina + 20 Fury
- **Action:** Standard Action
- **Range:** Ranged (single target, line of sight)
- **Effect:** Applies [Taunted] to target enemy for 2 rounds
- **Fury Generation:** +10 Fury each time taunted enemy attacks you

**Rank 3 (Mastery — Capstone trained)**

- **Cost:** 25 Stamina + 15 Fury (reduced)
- **Taunt Duration:** 3 rounds (from 2)
- **Fury Generation:** +15 Fury per attack received (from +10)
- **First Attack Mitigation:** First attack from taunted enemy deals -20% damage
- **Kill Reward:** If taunted enemy dies while [Taunted], gain [Empowered] for 1 round (+2 dice to damage)

---

## Worked Examples

### Example 1: Basic Taunt Cycle (Rank 2)

> **Situation:** Dangerous elite enemy in back row targeting your Bone-Setter. Berserkr at 35 Fury.
> 

> 
> 

> **Unleashed Roar:** -30 Stamina, -20 Fury (now 15 Fury)
> 

> 
> 

> **Enemy Behavior (2 rounds):**
> 

> - Round 1: Elite attacks Berserkr → +10 Fury (now 25)
> 

> - Round 2: Elite attacks Berserkr → +10 Fury (now 35)
> 

> 
> 

> **Net Fury:** Started 35, spent 20, gained 20 = **35 Fury** (broke even on Fury, protected ally)
> 

> 
> 

> **Bonus:** Blood-Fueled would add Fury from damage taken on top of this.
> 

### Example 2: Fury Engine with Blood-Fueled (Rank 2)

> **Situation:** Berserkr has Blood-Fueled passive. Taunts boss.
> 

> 
> 

> **Boss attacks Berserkr:** 25 damage
> 

> 
> 

> **Fury Gained:**
> 

> - Unleashed Roar trigger: +10 Fury
> 

> - Blood-Fueled (2× damage taken): 25 × 2 = +50 Fury
> 

> - **Total:** +60 Fury from one attack
> 

> 
> 

> **Result:** Single boss attack generates 60 Fury — enough for Whirlwind of Destruction.
> 

### Example 3: Kill Reward Chain (Rank 3)

> **Situation:** Taunt weakened enemy, finish with Hemorrhaging Strike.
> 

> 
> 

> **Unleashed Roar:** Taunt applied (3 rounds)
> 

> 
> 

> **Enemy attacks once:** +15 Fury, takes -20% damage on first hit
> 

> 
> 

> **Hemorrhaging Strike kills target:** [Taunted] enemy dies → **[Empowered] gained**
> 

> 
> 

> **Next turn:** [Empowered] active (+2 dice to damage) → Whirlwind of Destruction with bonus damage
> 

> 
> 

> **Result:** Taunt → tank hit → kill → empowered AoE follow-up.
> 

---

## Failure Modes

### Taunted Enemy Dies Before Attacking

- **Fury generation:** 0 from Unleashed Roar (no attacks received)
- **Kill Reward (Rank 3):** Still triggers [Empowered] if you killed them
- **Cost:** Stamina and Fury still spent

### Enemy Immune to Taunt

- **Ability fails:** Some bosses or elite enemies resist [Taunted]
- **Cost:** Resources consumed, no effect applied
- **UI:** "Target is immune to Taunt" message

### Multiple Enemies Attacking

- **Only taunted enemy triggers Fury:** Other enemies attacking you don't generate bonus Fury
- **Strategy:** Use in single-target or boss scenarios for best value

---

## Tactical Applications

### Tank and Fuel

Force dangerous enemies to attack you. Convert their attacks into Fury generation for devastating abilities.

### Blood-Fueled Synergy

**CRITICAL COMBO:** Taunted enemy attacks → take damage → Blood-Fueled doubles Fury from damage + Unleashed Roar bonus Fury. Single attack can generate 40-70 Fury.

### Protect Squishies

Pull aggro from fragile party members (Bone-Setter, Skald) while building resources.

### Fury Generation Potential

- **Rank 2:** 2 attacks over 2 rounds = +20 Fury (net 0 after cost)
- **Rank 3:** 3 attacks over 3 rounds = +45 Fury (net +30 after cost)
- **With Blood-Fueled:** Each attack adds 2× damage taken as additional Fury

### Weaponized Vulnerability Combo (Rank 3)

1. Reckless Assault → apply [Vulnerable] to self
2. Unleashed Roar → taunt enemy
3. Taunted enemy attacks your [Vulnerable] self → amplified damage → more Blood-Fueled Fury + Weaponized Vulnerability +10 Fury

---

## Integration Notes

**Role:** Hybrid offensive/defensive ability. Converts incoming attacks into Fury fuel while protecting allies.

**Design Philosophy:**

- Transforms the Berserkr's weakness (taking damage) into strength
- Creates tactical choice: protect allies OR maximize self-damage Fury generation
- Rank 3 rewards finishing taunted enemies with combat buff

**Notable Synergies:**

- **Blood-Fueled** (Tier 2): **CRITICAL** — Damage from taunted attacks = doubled/tripled Fury
- **Reckless Assault** (Tier 1): [Vulnerable] + Taunt = amplified Blood-Fueled value
- **Whirlwind of Destruction** (Tier 2): Spend generated Fury on AoE
- **Death or Glory** (Tier 3): [Bloodied] amplifies all Fury generation including Unleashed Roar triggers

**Related Abilities:**

- Blood-Fueled (Tier 2): Primary synergy for damage-to-Fury conversion
- Reckless Assault (Tier 1): Amplifies incoming damage for more Fury
- Whirlwind of Destruction (Tier 2): Primary Fury sink after generation