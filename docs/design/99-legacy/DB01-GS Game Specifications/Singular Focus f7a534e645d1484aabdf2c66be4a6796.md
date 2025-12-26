# Singular Focus

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-SINGULARFOCUS-v5.0
Parent item: Hólmgangr (Master Duelist) — Specialization Specification v5.0 (H%C3%B3lmgangr%20(Master%20Duelist)%20%E2%80%94%20Specialization%20Specif%20e786956d4e3e4dcaab01a7c79067c9ae.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Validated: No

## Overview

| Attribute | Value |
| --- | --- |
| **Ability Type** | Passive |
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 / 4 / 4 |

---

## Thematic Description

> *"Each clean touch on the named opponent sharpens the next; the eye learns their rhythm."*
> 

Singular Focus represents the Hólmgangr's **escalating precision**. Each consecutive hit on your Dueling Target teaches you more about their defenses—your strikes becoming increasingly devastating as you read their patterns.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Trigger**: Each consecutive hit on your [Dueling Target]
- **Stacking Bonus**: +1 cumulative damage bonus per hit
- **Maximum Stacks**: +5
- **Reset Condition**: Miss or attack different enemy

### Rank 2

- **Maximum Stacks**: +8 (up from +5)
- **PP Cost**: 4

### Rank 3

- **Critical Expansion**: At max stacks, critical hit range expands by **+10%**
- **PP Cost**: 4

---

## Stack Tracking

| Consecutive Hits | Damage Bonus | R3 Crit Bonus |
| --- | --- | --- |
| 1 | +1 | — |
| 2 | +2 | — |
| 3 | +3 | — |
| 4 | +4 | — |
| 5 (R1 max) | +5 | +10% crit range |
| 6-8 (R2+) | +6 to +8 | +10% crit range |

**Reset Triggers**:

- Miss an attack
- Attack a non-Dueling Target
- Dueling Target dies (stacks reset, new target starts at 0)

---

## Synergies

### Internal (Hólmgangr Tree)

- **Precise Thrust**: Low Stamina cost enables frequent attacks
- **Reactive Parry**: Counters contribute to stacks
- **Blade Dance**: 3 attacks = 3 stack opportunities

### External (Party Composition)

- **Controllers**: CC prevents target from fleeing/hiding
- **Skald**: Attack speed buffs build stacks faster
- **Debuffers**: Accuracy buffs prevent misses

---

## Tactical Applications

### Stack Building Protocol

1. Challenge of Honour (mark target)
2. Precise Thrust (hit 1: +1)
3. Precise Thrust (hit 2: +2)
4. Continue building to max
5. At max: unleash finisher with bonus damage + crit chance

### Rank 3: Critical Execution

- +8 damage bonus at max stacks
- +10% critical hit chance
- Finishing Lesson + max stacks = devastating execute
- Critical on execute = overkill guarantee

---

## v5.0 Compliance Notes

✅ **Duelist Identity**: Rewards sustained single-target focus

✅ **Stacking System**: Clear caps prevent runaway scaling

✅ **Risk/Reward**: Miss resets progress

✅ **Build Synergy**: Integrates with entire tree