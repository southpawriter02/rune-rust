# Tidal Rush

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-TIDALRUSH-v5.0
Parent item: Strandhögg (Glitch-Raider) — Specialization Specification v5.0 (Strandh%C3%B6gg%20(Glitch-Raider)%20%E2%80%94%20Specialization%20Specif%20a2c08028a3fa417e80a01dce77f9e69a.md)
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

> *"Each visible system error in the foe feeds the raider's pace; momentum compounds when control falters."*
> 

Tidal Rush is the Strandhögg's **passive momentum engine**. Every time an enemy falters—gaining a debuff, failing a save—you gain Pace. The chaos feeds your speed.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Trigger**: Whenever an enemy **within 6 tiles** gains a negative status effect
- **Pace Generation**: +5 Pace

### Rank 2

- **Pace Generation**: +8 Pace (up from 5)
- **Additional Trigger**: Also triggers when enemy **fails a save**
- **PP Cost**: 4

### Rank 3

- **Pace Generation**: +12 Pace (up from 8)
- **High Pace Bonus**: At **75+ Pace**, your attacks gain **+1 bonus die**
- **PP Cost**: 4

---

## Trigger Examples

| Event | Triggers Tidal Rush? |
| --- | --- |
| Ally applies [Feared] to enemy | ✅ Yes (+5/8/12 Pace) |
| Enemy fails WILL save | ✅ Yes (R2+) |
| You apply [Disoriented] via Dread Charge | ✅ Yes |
| Enemy cured of status | ❌ No |
| Ally outside 6 tiles applies status | ❌ No |

---

## Synergies

### Internal (Strandhögg Tree)

- **Dread Charge**: Self-triggered Pace generation
- **Vicious Flank**: More statuses = more Pace + more damage
- **Riptide**: Rapid Pace buildup to 80 threshold

### External (Party Composition)

- **Controllers**: Every CC generates Pace for you
- **Fear-appliers**: Tidal Rush triggers constantly
- **Debuff-heavy teams**: Exponential momentum

---

## Tactical Applications

### Passive Pace Engine

- Controllers apply 3 debuffs in one round
- Tidal Rush: 3 × 12 = 36 Pace
- Combined with movement and attacks
- Reach Riptide threshold rapidly

### Rank 3: High Pace Combat

- At 75+ Pace: +1 bonus die to all attacks
- Sustained high Pace = sustained accuracy
- Incentivizes staying at peak momentum
- Snowball effect when team applies CC

---

## v5.0 Compliance Notes

✅ **Passive Pace Generation**: No action cost

✅ **Team Synergy**: Rewards CC-heavy compositions

✅ **Range Limitation**: 6 tiles prevents abuse

✅ **Momentum Snowball**: R3 bonus die at high Pace