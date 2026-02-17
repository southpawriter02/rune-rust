# Reactive Parry

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-REACTIVEPARRY-v5.0
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
| **Ability Type** | Passive (Triggered) |
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 / 4 / 4 |

---

## Thematic Description

> *"Turn the strike aside with timing so clean it leaves a gap to answer into."*
> 

Reactive Parry represents the Hólmgangr's **mastery of defensive timing**. When your Dueling Target's attack fails to land, you exploit the opening with an immediate counterattack—the rhythm of the duel shifting in your favor.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Trigger**: Your [Dueling Target] **misses you** with a melee attack
- **Frequency**: Once per round
- **Response**: Make an immediate **counterattack** (Free Action, no Stamina cost)

### Rank 2

- **Bonus Damage**: Counterattack deals **+1d6 bonus damage**
- **PP Cost**: 4

### Rank 3

- **Focus Generation**: Counterattack generates **15 Focus**
- **PP Cost**: 4

---

## Trigger Conditions

| Condition | Reactive Parry Triggers? |
| --- | --- |
| Dueling Target misses melee attack | ✅ Yes |
| Dueling Target misses ranged attack | ❌ No (melee only) |
| Non-Dueling Target misses | ❌ No |
| Dueling Target hits | ❌ No |
| Already used this round | ❌ No (once/round) |

---

## Synergies

### Internal (Hólmgangr Tree)

- **Unencumbered Speed**: Higher Defense = more misses = more counters
- **Challenge of Honour**: Required for [Dueling Target]
- **Singular Focus**: Counters contribute to consecutive hit stacks

### External (Party Composition)

- **Controllers**: Debuffs reduce enemy accuracy
- **Skald**: Defense buffs increase counter opportunities
- **Tanks**: Absorb non-Dueling Target attacks

---

## Tactical Applications

### The Counter-Puncher

1. Challenge high-damage melee enemy
2. High Defense causes misses
3. Each miss = free counterattack
4. Build Focus while dealing damage
5. Execute when Focus threshold reached

### Rank 3: Focus Engine

- Miss → Counter → 15 Focus
- Combined with Precise Thrust → rapid Focus building
- Enables faster access to finishers

---

## v5.0 Compliance Notes

✅ **Duelist Identity**: Rewards 1v1 combat

✅ **Free Action Economy**: No action/resource cost

✅ **Defensive Offense**: Turns defense into damage

✅ **Focus Integration**: R3 ties into resource system