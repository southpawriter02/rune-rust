# Challenge of Honour

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-CHALLENGEOFHONOUR-v5.0
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
| **Ability Type** | Active — Bonus Action |
| **Tier** | 1 (Foundational) |
| **PP Cost** | 3 / 3 / 3 |
| **Resource Cost** | 20 Stamina |
| **Duration** | Combat (until target dies or new target chosen) |

---

## Thematic Description

> *"Name the opponent and draw the line; for a time, the mind and blade attend to that one above all others."*
> 

Challenge of Honour is the **defining ability** of the Hólmgangr. You formally challenge an enemy to single combat, gaining bonuses against them—but the duel cuts both ways.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 20 Stamina | Bonus Action
- **Target**: Single enemy within line of sight
- **Duration**: Until target dies or you choose new target
- **Effect**: Mark target as your **[Dueling Target]**
    - You gain **+1 die** to all attacks against them
    - They gain **+1 die** to attacks against you

### Rank 2

- **Your Bonus**: +2 dice (up from +1)
- **Their Bonus**: Remains +1 die
- **PP Cost**: 3

### Rank 3

- **On Kill**: When Dueling Target dies, immediately **heal 15 HP** and **gain 20 Focus**
- **PP Cost**: 3

---

## [Dueling Target] Status

**Enables**:

- Precise Thrust Focus generation
- Reactive Parry counterattacks
- Singular Focus stacking
- Exploit Opening requirement
- Blade Dance targeting
- Finishing Lesson targeting

**The Duel Cuts Both Ways**:

- Your bonus: +1/+2 dice to attacks
- Their bonus: +1 die to attacks against you
- **Risk/Reward**: You commit to one enemy, they can focus you

---

## Synergies

### Internal (Hólmgangr Tree)

- **Every Hólmgangr ability**: Most require or benefit from [Dueling Target]
- **Finishing Lesson**: Kill rewards chain into new challenge
- **Singular Focus**: Consecutive hit bonuses

### External (Party Composition)

- **Skjaldmær**: Protects you while you duel
- **Controllers**: Keep others off you
- **Gorge-Maw**: Fissure isolates your duel target

---

## Tactical Applications

### Target Selection

1. Identify **highest-priority single threat**
2. Challenge of Honour (Bonus Action)
3. Begin duel with Standard Action attack
4. Allies peel other enemies away
5. Finish target, chain to next (R3)

### Rank 3: Kill Chain

- Kill Dueling Target → Heal 15 HP + 20 Focus
- Immediately designate new target
- Sustain dueling momentum through combat

---

## v5.0 Compliance Notes

✅ **Duelist Identity**: Defines single-target specialization

✅ **Risk/Reward**: Mutual bonuses create tension

✅ **Bonus Action**: Enables same-turn attack

✅ **Combat Duration**: No tracking/management overhead