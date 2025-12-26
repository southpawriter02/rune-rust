# Crippling Cut

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-CRIPPLINGCUT-v5.0
Parent item: Hólmgangr (Master Duelist) — Specialization Specification v5.0 (H%C3%B3lmgangr%20(Master%20Duelist)%20%E2%80%94%20Specialization%20Specif%20e786956d4e3e4dcaab01a7c79067c9ae.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Validated: No

## Overview

| Attribute | Value |
| --- | --- |
| **Ability Type** | Active — Standard Action |
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 / 4 / 4 |
| **Resource Cost** | 40 Stamina |
| **Cooldown** | None |

---

## Thematic Description

> *"A low sweep that robs stance and step; the foe moves as if in mud for a brief span."*
> 

Crippling Cut is the Hólmgangr's **debilitating strike**. A precise attack to the legs or joints that impairs your opponent's mobility and defensive capability—setting them up for the kill.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 40 Stamina
- **Target**: Single enemy (melee range)
- **Attack**: FINESSE vs target's Defense
- **Damage**: 3d6 Physical
- **Status**: Target gains **[Slowed]** for 2 rounds
    - Half movement speed
    - -2 Defense

### Rank 2

- **Damage**: 4d6 Physical (up from 3d6)
- **Duration**: [Slowed] lasts **3 rounds** (up from 2)
- **PP Cost**: 4

### Rank 3

- **Additional Status**: Also applies **[Hobbled]**
    - Cannot use Reactions
- **PP Cost**: 4

---

## Status Effects Applied

### [Slowed]

- **Movement**: Halved
- **Defense**: -2 penalty
- **Duration**: 2/3 rounds (by rank)

### [Hobbled] (Rank 3)

- **Reactions**: Cannot use
- **Duration**: Same as [Slowed]

---

## Synergies

### Internal (Hólmgangr Tree)

- **Exploit Opening**: [Slowed] enables Exploit Opening
- **Unencumbered Speed**: -2 Defense stacks with your +Defense
- **Singular Focus**: Maintains consecutive hit chain

### External (Party Composition)

- **Ranged specialists**: Slowed enemies can't close distance
- **Controllers**: Stack debuffs for Exploit Opening
- **Mobile strikers**: Kite slowed enemies easily

---

## Tactical Applications

### Setup for Execution

1. Challenge of Honour (mark target)
2. Crippling Cut (apply [Slowed])
3. Target now meets Exploit Opening requirement
4. Build Focus to 30
5. Exploit Opening for massive damage

### Rank 3: Reaction Denial

- [Hobbled] prevents enemy Reactions
- No opportunity attacks when repositioning
- No defensive abilities
- Complete tactical control of the duel

---

## v5.0 Compliance Notes

✅ **FINESSE Attack**: Uses primary attribute

✅ **Setup Tool**: Enables other abilities

✅ **Stacking Debuffs**: Creates combo potential

✅ **Clear Duration**: Easy tracking