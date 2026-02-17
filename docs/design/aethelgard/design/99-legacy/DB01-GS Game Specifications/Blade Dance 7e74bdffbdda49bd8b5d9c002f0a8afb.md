# Blade Dance

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-BLADEDANCE-v5.0
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
| **Tier** | 3 (Mastery) |
| **PP Cost** | 5 / 5 / 5 |
| **Resource Cost** | 55 Stamina + 40 Focus |
| **Cooldown** | None |

---

## Thematic Description

> *"A flurry that flows past the guard in three clean lines—only for the named opponent."*
> 

Blade Dance is the Hólmgangr's **rapid assault technique**. Three precise strikes in quick succession, each finding a new angle of attack. This ability can only be used against your sworn opponent.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 55 Stamina + 40 Focus
- **Target**: Your **[Dueling Target] only** (melee range)
- **Attacks**: Make **three separate FINESSE attacks**
- **Damage**: Each attack deals 2d8 Physical
- **Focus Recovery**: Each hit that lands generates **5 Focus**

### Rank 2

- **Damage**: Each attack deals 3d8 Physical (up from 2d8)
- **PP Cost**: 5

### Rank 3

- **Status Effect**: If **all three attacks hit**, target is **[Staggered]** for 1 round
- **PP Cost**: 5

---

## Attack Resolution

| Attacks Hit | Total Damage (R1) | Total Damage (R2) | Focus Recovered |
| --- | --- | --- | --- |
| 0/3 | 0 | 0 | 0 |
| 1/3 | 2d8 | 3d8 | 5 |
| 2/3 | 4d8 | 6d8 | 10 |
| 3/3 | 6d8 + [Staggered] (R3) | 9d8 + [Staggered] (R3) | 15 |

---

## Synergies

### Internal (Hólmgangr Tree)

- **Challenge of Honour**: Required (Dueling Target only)
- **Singular Focus**: 3 hits = 3 stack increases
- **Exploit Opening**: R3 [Staggered] enables Exploit Opening

### External (Party Composition)

- **Accuracy buffers**: Each hit matters
- **Skald**: Tempo buffs enhance multi-attack value
- **Controllers**: Keep target in melee range

---

## Tactical Applications

### The Pressure Combo

1. Challenge of Honour (mark target)
2. Build to 40 Focus
3. Blade Dance (3 attacks, 6d8/9d8 total)
4. If all hit: target [Staggered] (R3)
5. Follow with Exploit Opening

### Focus Economy

- Costs 40 Focus
- Recovers up to 15 Focus on full hits
- Net cost: 25 Focus if all hit
- Enables rapid ability chaining

### Singular Focus Synergy

- 3 hits = +3 to consecutive hit stacks
- Rapid path to maximum stacks
- Set up devastating Finishing Lesson

---

## v5.0 Compliance Notes

✅ **Dueling Target Exclusive**: Reinforces single-target identity

✅ **Multi-Attack**: Distinct from single-hit abilities

✅ **Focus Spend/Recovery**: Resource management depth

✅ **Combo Enabler**: R3 [Staggered] chains into other abilities