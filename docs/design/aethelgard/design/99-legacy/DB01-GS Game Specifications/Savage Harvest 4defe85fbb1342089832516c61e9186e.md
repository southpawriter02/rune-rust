# Savage Harvest

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-SAVAGEHARVEST-v5.0
Parent item: Strandhögg (Glitch-Raider) — Specialization Specification v5.0 (Strandh%C3%B6gg%20(Glitch-Raider)%20%E2%80%94%20Specialization%20Specif%20a2c08028a3fa417e80a01dce77f9e69a.md)
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
| **Resource Cost** | 50 Stamina + 30 Pace |
| **Cooldown** | None |

---

## Thematic Description

> *"A decisive finishing sequence meant to close the ledger on a marked target; when the mark falls, breath and pace return in kind."*
> 

Savage Harvest is the Strandhögg's **execute ability**. You finish wounded enemies with brutal efficiency, and each kill restores your momentum for the next.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 50 Stamina + 30 Pace
- **Target**: Single enemy **below 50% HP** (melee range)
- **Attack**: FINESSE vs target's Defense
- **Damage**: 6d10 Physical
- **On Kill**: Heal **20 HP** and generate **40 Pace**

### Rank 2

- **Damage**: 8d10 Physical (up from 6d10)
- **On Kill Healing**: 30 HP (up from 20)
- **PP Cost**: 5

### Rank 3

- **Execute Threshold**: 60% HP (up from 50%)
- **On Kill Bonus**: Next ability costs **no Pace**
- **PP Cost**: 5

---

## Execute Thresholds

| Rank | HP Threshold | Example (100 HP target) |
| --- | --- | --- |
| R1 | Below 50% | Below 50 HP |
| R2 | Below 50% | Below 50 HP |
| R3 | Below 60% | Below 60 HP |

---

## Kill Rewards

| Rank | Healing | Pace Gained | Special |
| --- | --- | --- | --- |
| R1 | 20 HP | +40 Pace | — |
| R2 | 30 HP | +40 Pace | — |
| R3 | 30 HP | +40 Pace | Next ability free |

**Net Pace on Kill**: -30 (cost) + 40 (reward) = **+10 Pace**

---

## Synergies

### Internal (Strandhögg Tree)

- **Vicious Flank**: Weaken target to execute threshold
- **No Quarter**: Kill → free movement + sustain
- **Riptide**: R3 free ability enables instant follow-up

### External (Party Composition)

- **Damage dealers**: Soften targets for execute
- **Controllers**: CC prevents target escape
- **Healers**: Kill healing supplements party healing

---

## Tactical Applications

### The Harvest Chain

1. Allies weaken target to 50%/60%
2. Savage Harvest (6-8d10 damage)
3. Kill confirmed: +30 HP, +40 Pace
4. No Quarter: Free movement to next target
5. R3: Next ability costs no Pace
6. Repeat on next wounded enemy

### Sustain Through Killing

- Each kill heals 20-30 HP
- Net positive Pace per kill
- Self-sustaining aggression loop
- Momentum increases with each finish

---

## v5.0 Compliance Notes

✅ **Execute Mechanic**: HP threshold requirement

✅ **Kill Incentive**: Rewards finishing enemies

✅ **Pace Economy**: Net positive on kills

✅ **Sustain**: Healing enables extended combat