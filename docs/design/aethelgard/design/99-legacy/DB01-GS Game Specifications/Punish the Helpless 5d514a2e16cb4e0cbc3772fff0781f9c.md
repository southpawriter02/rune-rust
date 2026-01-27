# Punish the Helpless

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-PUNISHTHEHELPLESS-v5.0
Parent item: Hlekkr-master (Glitch Exploiter) — Specialization Specification v5.0 (Hlekkr-master%20(Glitch%20Exploiter)%20%E2%80%94%20Specialization%20%20e50dff9a1cb14a8fb79a7ba71da8f771.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Validated: No

## Overview

| Attribute | Value |
| --- | --- |
| **Ability Type** | Passive |
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 / 4 / 4 |
| **Prerequisite** | 8 PP invested in Hlekkr-master tree |

---

## Thematic Description

> *"A trapped enemy is a dead enemy. Your predator's instinct capitalizes on immobility and confusion with brutal, focused strikes."*
> 

The Hlekkr-master's control abilities aren't merely defensive—they're **setup for execution**. Punish the Helpless converts every successful control effect into a damage amplifier, rewarding the tactical patience of locking enemies down before striking.

---

## Mechanical Implementation

### Base Effect (Rank 1)

Your attacks deal **+50% damage** against enemies suffering any of these control effects:

- [Rooted]
- [Slowed]
- [Stunned]
- [Seized]
- [Disoriented]

### Rank 2

- **Damage Bonus**: +75% (up from +50%)
- **Attack Advantage**: Gain **Advantage** on attack rolls vs controlled enemies
- **PP Cost**: 4

### Rank 3

- **Damage Bonus**: +100% (double damage)
- **Chain Bleed**: Controlled enemies take **1d6 Physical damage per turn** from your chains (passive DoT)
- **PP Cost**: 4

---

## Qualifying Control Effects

| Status Effect | Source | Duration |
| --- | --- | --- |
| [Rooted] | Netting Shot | 2-3+ turns |
| [Slowed] | Netting Shot (Rank 3), Chain Scythe | 2-3 turns |
| [Stunned] | Corruption Siphon Chain | 1-2 turns |
| [Seized] | Unyielding Grip | 1-3 turns |
| [Disoriented] | Grappling Hook Toss, Chain Scythe (Rank 3) | 1 turn |

---

## Damage Calculation Examples

### Rank 1 (+50%)

- Base attack: 20 damage → **30 damage** vs controlled target

### Rank 2 (+75%)

- Base attack: 20 damage → **35 damage** vs controlled target
- Plus Advantage on attack roll

### Rank 3 (+100%)

- Base attack: 20 damage → **40 damage** vs controlled target
- Plus 1d6/turn passive bleed

---

## Synergies

### Internal (Hlekkr-master Tree)

- **All control abilities**: Direct damage amplification on successful control
- **Pragmatic Preparation I**: Extended control duration = more damage windows
- **Master of Puppets**: [Vulnerable] from pulls stacks with Punish bonus

### External (Party Composition)

- **Other controllers**: Any ally-applied control effects also trigger Punish
- **Burst damage**: Coordinate big hits during control windows
- **DoT specialists**: Rank 3 bleed stacks with other persistent damage

---

## Tactical Applications

### Control-to-Damage Loop

The Hlekkr-master's core combat loop:

1. Apply control (Netting Shot, Grappling Hook, etc.)
2. Punish controlled target (+50/75/100% damage)
3. Maintain control to extend damage window
4. Repeat

### Team Coordination

Communicate control application to allies—Punish the Helpless benefits don't require *you* to apply the control. Any controlled enemy is a valid target.

### Rank 3 Chain Bleed

The passive 1d6/turn adds sustained pressure without action economy cost. Against multiple controlled enemies, this becomes significant passive DPS.

---

## Design Philosophy

Punish the Helpless addresses the Hlekkr-master's core tension: **low personal damage vs. high control**. Rather than increasing base damage, this ability converts control success into damage amplification, preserving the specialization's tactical identity while ensuring meaningful combat contribution.

---

## v5.0 Compliance Notes

✅ **Control-to-Damage Conversion**: Core mechanical identity

✅ **Scaling Design**: 50% → 75% → 100% clear progression

✅ **Team Synergy**: Supports coordinated play

✅ **Action Economy Efficient**: Passive damage boost, no action cost