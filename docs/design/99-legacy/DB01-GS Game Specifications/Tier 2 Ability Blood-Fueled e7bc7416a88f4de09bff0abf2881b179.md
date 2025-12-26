# Tier 2 Ability: Blood-Fueled

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BERSERKR-BLOODFUELED-v5.0
Parent item: Berserkr (The Roaring Fire) — Specialization Specification v5.0 (Berserkr%20(The%20Roaring%20Fire)%20%E2%80%94%20Specialization%20Speci%20edde253a7f9d46ff8e936655162ef4f7.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 2
Template Validated: No
Voice Validated: No

## Core Identity

**Specialization:** Berserkr (The Roaring Fire)

**Tier:** 2 (Escalating Violence)

**Type:** Passive (Always Active)

**Prerequisite:** 8 PP invested in Berserkr tree

**Cost:** 4 PP

---

## Description

Pain fuels your rage. Whenever you take damage, you gain multiplied Fury from that damage. Each wound is not a setback but an **invitation to escalate**. The blood trickling down your face, the ache in your ribs, the burning in your muscles—all of it feeds the rage.

This is the **cornerstone of the Berserkr's resource economy**—transforming the typical "avoid damage" incentive into a playstyle where **controlled aggression and calculated risk-taking** are rewarded.

---

## Mechanics

### Core Formulas

```
FuryFromDamage = DamageTaken × Multiplier
InspiredCheck = Roll d100 ≤ 50 → gain [Inspired]  [Rank 3 only]
FuriousCheck = DamageTaken ≥ 30 → gain [Furious]  [Rank 3 only]

By Rank:
  Rank 2: 2× Fury from damage
  Rank 3: 3× Fury from damage, 50% [Inspired] chance, [Furious] on 30+ damage
```

### Progression & Milestones

**Rank 2 (Starting — when ability is learned)**

- **Effect:** Gain **2× Fury from all damage taken** (instead of standard 1×)
- **Applies to:** Physical, magical, environmental, DoT, friendly fire — **ALL damage sources**
- **Example:** Take 25 damage → Gain 50 Fury (instead of 25)
- **Always active**, no cost

**Rank 3 (Mastery — Capstone trained)**

- **Effect:** Gain **3× Fury from all damage taken** (increased from 2×)
- **Example:** Take 25 damage → Gain 75 Fury
- **Pain Inspires:** 50% chance when taking damage to gain [Inspired] for 1 round
    - [Inspired]: +2 to all action rolls, immune to [Feared]
- **Primal Fury:** Taking **30+ damage in a single hit** grants [Furious] for 2 rounds
    - [Furious]: +4 MIGHT, +2 to attack rolls

---

## Worked Examples

### Example 1: Standard Damage Conversion (Rank 2)

> **Situation:** Enemy deals 20 Physical damage to Berserkr.
> 

> 
> 

> **Blood-Fueled Calculation:**
> 

> - Base Fury from damage: 20
> 

> - Blood-Fueled multiplier: 2×
> 

> - **Fury gained:** 20 × 2 = **40 Fury**
> 

> 
> 

> **Result:** A hit that would normally give 20 Fury now gives 40.
> 

### Example 2: Vulnerable Amplification (Rank 2)

> **Situation:** Berserkr used Reckless Assault, has [Vulnerable] (+20%). Enemy attacks for 30 base damage.
> 

> 
> 

> **Damage Calculation:**
> 

> - Base damage: 30
> 

> - Vulnerable bonus: 30 × 1.20 = **36 damage taken**
> 

> 
> 

> **Blood-Fueled Calculation:**
> 

> - 36 × 2 = **72 Fury gained**
> 

> 
> 

> **Result:** [Vulnerable] isn't a downside — it's a Fury multiplier.
> 

### Example 3: Maximum Fury Engine (Rank 3)

> **Situation:** Boss deals 45 damage to Berserkr with Blood-Fueled Rank 3.
> 

> 
> 

> **Blood-Fueled Calculation:**
> 

> - 45 × 3 = **135 Fury** (capped at 100)
> 

> 
> 

> **Primal Fury Check:** 45 ≥ 30 → **[Furious] gained** (+4 MIGHT, +2 attack, 2 rounds)
> 

> 
> 

> **Pain Inspires Check:** Roll d100: 38 ≤ 50 → **[Inspired] gained** (+2 action rolls, Fear immune, 1 round)
> 

> 
> 

> **Result:** One big hit = capped Fury + two powerful combat buffs.
> 

### Example 4: Countering the WILL Weakness (Rank 3)

> **Situation:** Berserkr at 80 Fury (suffering -2 WILL penalty). Psychic enemy prepares Fear attack.
> 

> 
> 

> **Takes 15 damage from ally's Whirlwind friendly fire:**
> 

> - Fury: 15 × 3 = +45 (capped at 100)
> 

> - Pain Inspires: Roll 42 ≤ 50 → **[Inspired] gained (Fear immune)**
> 

> 
> 

> **Psychic enemy uses Fear attack:** Berserkr is **immune** due to [Inspired]
> 

> 
> 

> **Result:** Taking damage gave defensive buff that countered the Berserkr's main weakness.
> 

---

## Failure Modes

### No Damage Taken

- **Effect:** Blood-Fueled does nothing if Berserkr avoids all damage
- **Mitigation:** Use Reckless Assault [Vulnerable] or Unleashed Roar to guarantee hits

### Overkill Damage

- **Fury cap:** Fury caps at 100; excess Fury from massive hits is wasted
- **Strategy:** Don't let Fury sit at 100 — spend it before taking big hits

### Healing Negation

- **Conflict:** Healers want to prevent damage; Blood-Fueled wants damage
- **Coordination:** Communicate with Bone-Setter about optimal HP threshold

---

## Tactical Applications

### Intentional Damage Taking

Seek damage sources actively:

- Use **Unleashed Roar** to guarantee enemy attacks
- Use **Reckless Assault** for [Vulnerable] (+15-25% damage taken)
- Position in Front Row to absorb hits

### Fury Generation Comparison

| Source | Fury Generated | Cost |
| --- | --- | --- |
| Wild Swing (3 enemies) | +15-30 | 40 Stamina |
| Reckless Assault | +15-20 | 35 Stamina + [Vulnerable] |
| Blood-Fueled (20 dmg, Rank 2) | +40 | 20 HP |
| Blood-Fueled (20 dmg, Rank 3) | +60 | 20 HP |

**Result:** Blood-Fueled is the most efficient Fury source — but costs HP.

### Reckless Assault Synergy

1. Reckless Assault → [Vulnerable] (+20% damage)
2. Enemy attacks → 40 base → 48 actual damage
3. Blood-Fueled Rank 3: 48 × 3 = **144 Fury (capped at 100)**
4. Single enemy attack **caps your Fury**

### Fear Immunity Loop (Rank 3)

The Berserkr's main weakness is -2 WILL while holding Fury. At Rank 3:

- Take damage → 50% chance [Inspired] → **immune to Fear**
- Pain becomes your defense against psychic attacks

---

## Integration Notes

**Role:** Cornerstone of Berserkr identity. Inverts standard damage-avoidance into damage-seeking.

**Design Philosophy:**

- Core identity ability (not optional for serious Berserkr play)
- Transforms playstyle fundamentally: damage is fuel, not threat
- High-skill expression through intentional damage manipulation
- Rank 3 provides defensive answer to WILL weakness via [Inspired]
- Requires healing support to sustain

**Notable Synergies:**

- **Reckless Assault** (Tier 1): **CRITICAL** — [Vulnerable] amplifies damage taken = more Fury
- **Unleashed Roar** (Tier 2): **CRITICAL** — forces enemy attacks for guaranteed damage
- **Whirlwind of Destruction** (Tier 2): Primary Fury sink after Blood-Fueled generation
- **Death or Glory** (Tier 3): +50% Fury gen while [Bloodied] stacks with Blood-Fueled
- **Primal Vigor** (Tier 1): Fury from Blood-Fueled activates Stamina regen thresholds

**External Synergies:**

- **Bone-Setter** (healer): Essential partner — keeps Berserkr alive through damage-seeking
- **Skjaldmær** (tank): Can absorb overflow damage when Berserkr needs rest

**Related Abilities:**

- Reckless Assault (Tier 1): [Vulnerable] synergy
- Unleashed Roar (Tier 2): Forced attack synergy
- Wild Swing (Tier 1): Backup Fury source when avoiding damage
- Whirlwind of Destruction (Tier 2): Primary Fury expenditure