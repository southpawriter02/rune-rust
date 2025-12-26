# Chain Scythe

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-CHAINSCYTHE-v5.0
Parent item: Hlekkr-master (Glitch Exploiter) — Specialization Specification v5.0 (Hlekkr-master%20(Glitch%20Exploiter)%20%E2%80%94%20Specialization%20%20e50dff9a1cb14a8fb79a7ba71da8f771.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Validated: No

## Overview

| Attribute | Value |
| --- | --- |
| **Ability Type** | Active — Standard Action |
| **Tier** | 3 (Mastery) |
| **PP Cost** | 5 / 5 / 5 |
| **Resource Cost** | 35 Stamina |
| **Cooldown** | 4 turns |
| **Prerequisite** | 16 PP invested in Hlekkr-master tree |

---

## Thematic Description

> *"You whirl a massive chain scythe in a devastating horizontal sweep, designed to trip, entangle, and utterly break enemy formations."*
> 

The Chain Scythe is the Hlekkr-master's premier **area control** ability. A weighted blade on a long chain sweeps through the front line, cutting legs from under enemies and leaving formations in disarray. Against corrupted targets, the destabilizing effect is catastrophic.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 35 Stamina
- **Target**: **All enemies in Front Row**
- **Attack**: FINESSE vs each target's Defense
- **Damage**: 2d8 Physical to all targets
- **Secondary**: Apply [Slowed] (2 turns) to all hit
- **Corruption Bonus**: Vs 60+ Corruption: 40% chance to apply [Knocked Down] instead of [Slowed]
- **Cooldown**: 4 turns

### Rank 2

- **Damage**: 3d8 Physical (up from 2d8)
- **Duration**: [Slowed] 3 turns (up from 2)
- **Knockdown**: 60% chance (up from 40%)
- **PP Cost**: 5

### Rank 3

- **Expanded Targeting**: Can target **Back Row** as well as Front Row
- **Knockdown**: 80% chance vs 60+ Corruption
- **Disorient**: +[Disoriented] (1 turn) to all hit
- **PP Cost**: 5

---

## Status Effects Applied

### [Slowed] (Default)

**Duration**: 2/3 turns

- Target's movement speed halved
- Target cannot take Reactions
- -1d10 penalty to Defense

### [Knocked Down] (vs 60+ Corruption)

**Duration**: 1 turn

- Target is **prone** (melee attacks have Advantage, ranged attacks have Disadvantage)
- Target must spend Move Action to stand
- Standing provokes Opportunity Attacks

### [Disoriented] (Rank 3)

**Duration**: 1 turn

- -2d10 penalty to attack rolls
- Cannot take Reactions

---

## Targeting Diagram

### Rank 1-2: Front Row Only

```
[Back Row]     ❌  ❌  ❌
[Front Row]    ✅  ✅  ✅  ← Chain Scythe hits all
```

### Rank 3: Full Battlefield

```
[Back Row]     ✅  ✅  ✅  ← Now included
[Front Row]    ✅  ✅  ✅  ← Chain Scythe hits all
```

---

## Synergies

### Internal (Hlekkr-master Tree)

- **Punish the Helpless**: +50/75/100% damage vs [Slowed]/[Disoriented] targets
- **Snag the Glitch**: Increased knockdown success vs corrupted enemies
- **Grappling Hook Toss**: Pull enemies to Front Row before sweeping
- **Pragmatic Preparation I**: [Slowed] duration extended +1 turn

### External (Party Composition)

- **AoE damage**: Allies can capitalize on grouped, slowed enemies
- **Opportunity attack builds**: Prone enemies standing provoke attacks
- **Front-line holders**: Slowed enemies can't close distance

---

## Tactical Applications

### Formation Breaking

Chain Scythe excels at **disrupting organized enemy formations**:

- Slow/knock down front-line tanks → back row exposed
- Prevent pursuit → allies can reposition safely
- Create chokepoint at prone bodies → block enemy movement

### Corruption Exploitation

Highly corrupted enemy groups become Chain Scythe's ideal targets:

- 60+ Corruption → high knockdown chance
- Knocked enemies → Opportunity Attack triggers
- Stacking with Snag the Glitch → near-guaranteed control

### Rank 3 Expansion

Back Row targeting transforms Chain Scythe into a **true battlefield-wide control**:

- Hit casters/archers with [Slowed] + [Disoriented]
- Deny defensive repositioning
- Set up for Master of Puppets capstone

---

## v5.0 Compliance Notes

✅ **Technology, Not Magic**: Physical chain-and-blade weapon, not supernatural

✅ **AoE Control**: Tier 3 power level appropriate for wide-area effect

✅ **Blight Integration**: Corruption-based knockdown bonus

✅ **Formation Tactics**: Supports Layer 2 battlefield geometry