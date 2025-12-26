# Finishing Lesson

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-FINISHINGLESSON-v5.0
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
| **Ability Type** | Active — Standard Action (Capstone) |
| **Tier** | Capstone |
| **PP Cost** | 6 / 6 / 6 |
| **Resource Cost** | 40 Stamina + 60 Focus |
| **Cooldown** | None |

---

## Thematic Description

> *"When an opponent's strength is plainly failing and their rhythm broken, end it with a single, exacting stroke."*
> 

Finishing Lesson is the **ultimate expression** of the Hólmgangr's mastery. A single, perfect strike that ends the duel decisively. Only delivered when your opponent's defeat is certain—and when they receive it, the lesson is final.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 40 Stamina + 60 Focus
- **Target**: Your **[Dueling Target] only** (melee range)
- **Requirement**: Target must be **below 40% HP**
- **Attack**: FINESSE vs target's Defense
- **Damage**: 8d12 Physical
- **On Kill**: May immediately designate a **new [Dueling Target]** and gain **40 Focus**

### Rank 2

- **Execute Threshold**: 50% HP (up from 40%)
- **Damage**: 10d12 Physical (up from 8d12)
- **PP Cost**: 6

### Rank 3

- **On Kill**: All **cooldowns reset**; gain **[Empowered]** for 2 rounds (+25% damage)
- **PP Cost**: 6

---

## Execute Thresholds

| Rank | HP Threshold | Example (100 HP target) |
| --- | --- | --- |
| R1 | Below 40% | Below 40 HP |
| R2 | Below 50% | Below 50 HP |
| R3 | Below 50% | Below 50 HP |

---

## Kill Chain Mechanics

### On Successful Kill:

1. Target eliminated
2. Immediately designate new [Dueling Target] (no action cost)
3. Gain 40 Focus
4. (R3) All cooldowns reset
5. (R3) Gain [Empowered] (+25% damage) for 2 rounds

### [Empowered] Status (Rank 3)

- **Duration**: 2 rounds
- **Effect**: +25% damage to all attacks
- **Stacking**: Does not stack with itself

---

## Synergies

### Internal (Hólmgangr Tree)

- **Challenge of Honour**: Kill chain maintains momentum
- **Singular Focus**: Max stacks + Finishing Lesson = devastation
- **Crippling Cut**: Weaken target to execute threshold

### External (Party Composition)

- **Damage dealers**: Soften targets to execute range
- **Controllers**: Keep targets in melee for execution
- **Skald**: [Empowered] stacks with performance buffs

---

## Tactical Applications

### The Dueling Protocol: Finale

1. Challenge of Honour (mark target)
2. Build Focus via Precise Thrust, Reactive Parry
3. Weaken target with Crippling Cut, Exploit Opening
4. When below threshold: Finishing Lesson
5. On kill: Chain to next target with 40 Focus

### Rank 3: Rampage Mode

- Kill → Reset cooldowns → [Empowered]
- New Dueling Target + 40 Focus
- Blade Dance immediately available
- +25% damage on all follow-up attacks
- Potential to chain multiple executions

---

## v5.0 Compliance Notes

✅ **Capstone Power**: Defines specialization's peak

✅ **Execute Mechanic**: Rewards proper setup

✅ **Kill Chain**: Maintains combat momentum

✅ **Focus Management**: 60 cost / 40 recovery on kill