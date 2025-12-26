# Unyielding Grip

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-UNYIELDINGGRIP-v5.0
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
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 / 4 / 4 |
| **Resource Cost** | 25 Stamina |
| **Cooldown** | 4 turns |
| **Prerequisite** | 8 PP invested in Hlekkr-master tree |

---

## Thematic Description

> *"Your chain wraps around sparking servos and malfunctioning joints, locking them in place and forcing a critical system error."*
> 

The Unyielding Grip technique exploits the vulnerabilities of mechanical and Undying enemies. By wrapping chains around critical joints and servos, you don't merely restrain—you **seize** the target completely, triggering cascading system failures that lock down all functions.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 25 Stamina
- **Target**: Single enemy
- **Attack**: FINESSE vs target's Defense
- **Damage**: 2d8 Physical
- **Special**: If target is **Undying or Mechanical**:
    - 60% chance to apply [Seized] (1 turn)
- **Cooldown**: 4 turns

### Rank 2

- **Success Rate**: 80% chance (up from 60%)
- **Duration**: [Seized] lasts 2 turns (up from 1)
- **PP Cost**: 4

### Rank 3

- **Duration**: [Seized] lasts 3 turns
- **Expanded Targeting**: Works on **all enemy types** at 40% chance
- **Crush Damage**: +1d6 Physical damage per turn to [Seized] targets (chains tighten)
- **PP Cost**: 4

---

## Status Effect: [Seized]

**Duration**: 1/2/3 turns (by rank)

**Effects**:

- Target **cannot take ANY actions** (Standard, Move, Bonus, Reaction)
- Target cannot be moved by forced movement effects
- Target's concentration effects are broken
- Removed if target takes damage exceeding 25% of max HP (chains break)

---

## Target Type Effectiveness

| Target Type | Rank 1-2 | Rank 3 |
| --- | --- | --- |
| Undying | 60%/80% | 80% |
| Mechanical | 60%/80% | 80% |
| Organic | No effect | 40% |
| Corrupted | No effect | 40% (+Snag bonus) |

---

## Synergies

### Internal (Hlekkr-master Tree)

- **Snag the Glitch**: +10-60% success vs corrupted targets (stacks with base chance)
- **Punish the Helpless**: +50/75/100% damage vs [Seized] targets
- **Pragmatic Preparation I**: No direct interaction ([Seized] duration not extended)

### External (Party Composition)

- **Burst damage dealers**: [Seized] creates perfect damage windows
- **Setup abilities**: Allies can position around immobile target
- **AoE specialists**: [Seized] target anchors enemy formation

---

## Tactical Applications

### Priority Targets

1. **Mechanical bosses**: System error creates massive damage windows
2. **Undying spellcasters**: Break concentration, prevent casting
3. **Charging enemies**: Stop momentum-based attacks cold
4. **Fleeing priority targets**: Prevent escape

### Damage Window Optimization

[Seized] targets cannot defend or evade. Coordinate with party to:

- Stack DoT effects before Seize breaks
- Line up high-damage single-target abilities
- Position party for optimal follow-up

### Chain Crush (Rank 3)

The 1d6/turn crush damage adds sustained pressure. Against high-HP targets, extended [Seized] duration becomes significant DPS contribution.

---

## v5.0 Compliance Notes

✅ **Technology, Not Magic**: Physical chain manipulation, not supernatural binding

✅ **Type Restrictions**: Mechanical/Undying focus reflects exploiting system vulnerabilities

✅ **Scaling Design**: Rank 3 expands targeting to all enemy types

✅ **Blight Integration**: Snag the Glitch synergy for corrupted enemies