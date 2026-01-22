# Grappling Hook Toss

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-GRAPPLINGHOOKTOSS-v5.0
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
| **Tier** | 1 (Foundational) |
| **PP Cost** | 3 / 3 / 3 |
| **Resource Cost** | 30 Stamina |
| **Cooldown** | 3 turns |
| **Target** | Single enemy in Back Row only |
| **Damage** | 2d8 Physical (FINESSE attack) |

---

## Thematic Description

> *"You swing a three-pronged grappling hook, snagging vulnerable targets and dragging them into the frontline. Corrupted enemies slide through space more easily."*
> 

The grappling hook is the Hlekkr-master's signature tool for **forced repositioning**. By yanking priority targets from the safety of the back row into melee range, you collapse enemy formations and expose their vulnerabilities. Corrupted enemies—their connection to physical space already unstable—slide through the pull with sickening ease.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 30 Stamina
- **Target**: Single enemy in **Back Row only** (Standard-size; Large/Huge immune)
- **Attack**: FINESSE vs target's Defense
- **Damage**: 2d8 Physical
- **On Hit**: **Pull target from Back Row to Front Row**
- **Secondary**: Apply [Disoriented] (1 turn)
- **Cooldown**: 3 turns

### Rank 2

- **Damage**: 3d8 Physical (increased from 2d8)
- **Pull Range**: Increased effective range for hook throw
- **PP Cost**: 3

### Rank 3

- **Corruption Synergy**: Generate **10 Focus** when successfully pulling a corrupted target
- **PP Cost**: 3

---

## Targeting Restrictions

| Size Category | Can Be Pulled? |
| --- | --- |
| Small | ✅ Yes |
| Standard | ✅ Yes |
| Large | ❌ No (immune) |
| Huge | ❌ No (immune) |
| Colossal | ❌ No (immune) |

---

## Status Effect: [Disoriented]

**Duration**: 1 turn

**Effects**:

- Target suffers -2d10 penalty to all attack rolls
- Target cannot take Reactions
- Removed at end of target's next turn

---

## Synergies

### Internal (Hlekkr-master Tree)

- **Pragmatic Preparation I**: Extends [Disoriented] by +1 turn
- **Snag the Glitch**: Increased success chance vs corrupted enemies
- **Punish the Helpless**: +50/75/100% damage vs [Disoriented] targets
- **Master of Puppets**: Pulled enemies also become [Vulnerable]

### External (Party Composition)

- **Front-line strikers**: Pull enemies into melee range for Warriors
- **AoE damage**: Group pulled enemies for efficient cleave
- **Assassination synergy**: Expose casters/healers for focus fire

---

## Tactical Applications

### Priority Targets for Pulls

1. **Enemy Casters**: Interrupt spell preparation, force concentration checks
2. **Ranged Attackers**: Remove range advantage, expose to melee
3. **Support Units**: Drag healers/buffers into danger
4. **Command Units**: Break morale by exposing leaders

### Positioning Combos

- Pull → Chain Scythe (hit pulled target with AoE)
- Pull → Ally opportunity attacks (pulled through threatened squares)
- Pull → Environmental hazards (fire, acid, pit edges)

---

## v5.0 Compliance Notes

✅ **Technology, Not Magic**: Physical hook-and-chain mechanics

✅ **Forced Movement**: Core Hlekkr-master identity

✅ **Corruption Integration**: Rank 3 Focus generation vs corrupted targets

✅ **Size Restrictions**: Realistic limitations on what can be pulled