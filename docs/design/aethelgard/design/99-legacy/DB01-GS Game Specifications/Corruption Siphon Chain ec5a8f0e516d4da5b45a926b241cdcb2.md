# Corruption Siphon Chain

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-CORRUPTIONSIPHONCHAIN-v5.0
Parent item: Hlekkr-master (Glitch Exploiter) — Specialization Specification v5.0 (Hlekkr-master%20(Glitch%20Exploiter)%20%E2%80%94%20Specialization%20%20e50dff9a1cb14a8fb79a7ba71da8f771.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Validated: No

## Overview

| Attribute | Value |
| --- | --- |
| **Ability Type** | Active — Standard Action |
| **Tier** | 3 (Mastery) |
| **PP Cost** | 5 / 5 / 5 |
| **Resource Cost** | 30 Stamina + 5 Psychic Stress |
| **Cooldown** | 3 turns |
| **Prerequisite** | 16 PP invested in Hlekkr-master tree |

---

## Thematic Description

> *"Using a rune-etched chain, you lash to a corrupted foe and siphon their chaotic energy, causing system shock."*
> 

The Corruption Siphon Chain represents the Hlekkr-master's most **dangerous technique**—direct contact with raw Blight energy. By channeling corruption through specialized chains, you induce catastrophic system failures in the target. But touching the Blight, even briefly, leaves psychic scars.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 30 Stamina + **5 Psychic Stress**
- **Target**: Single enemy with **existing Corruption** (minimum 1)
- **Requirement**: Does NOT work on 0 Corruption targets
- **Attack**: FINESSE vs target's Defense
- **Damage**: None
- **Effect**: Chance to apply [Stunned] (1 turn) based on target's Corruption:

| Corruption Level | Range | Stun Chance |
| --- | --- | --- |
| Low | 1-29 | 20% |
| Medium | 30-59 | 40% |
| High | 60-89 | 70% |
| Extreme | 90+ | 90% |
- **Cooldown**: 3 turns

### Rank 2

- **Duration**: [Stunned] 2 turns (up from 1)
- **PP Cost**: 5

### Rank 3

- **Corruption Purge**: If successful vs Extreme Corruption (90+), also **purge 10 Corruption** from target
- **Reduced Cost**: Psychic Stress reduced to **3** (down from 5)
- **PP Cost**: 5

---

## Status Effect: [Stunned]

**Duration**: 1/2 turns (by rank)

**Effects**:

- Target **cannot take any actions** (Standard, Move, Bonus)
- Target **cannot take Reactions**
- Target's concentration effects are broken
- Attacks against target have **Advantage**
- [Stunned] is stronger than [Seized] (no damage threshold to break)

---

## Trauma Economy

### Psychic Stress Cost

This ability represents **direct Blight contact**—the only ability in the Hlekkr-master tree with a Stress cost.

| Rank | Stress Cost | Risk Level |
| --- | --- | --- |
| 1-2 | 5 | Moderate |
| 3 | 3 | Low-Moderate |

### Risk-Reward Balance

- **High reward**: Near-guaranteed stun vs Extreme Corruption
- **Moderate risk**: Psychic Stress accumulation over extended fights
- **Mitigation**: Use sparingly; reserve for critical targets

---

## Corruption-Based Success Rates

| Corruption | Base Chance | + Snag the Glitch R1 | + Snag the Glitch R3 |
| --- | --- | --- | --- |
| Low (1-29) | 20% | 30% | 40% |
| Medium (30-59) | 40% | 60% | 80% |
| High (60-89) | 70% | 110% (capped) | 150% (capped) |
| Extreme (90+) | 90% | 150% (capped) | Cannot miss |

*Note: Success rates cap at 100%*

---

## Synergies

### Internal (Hlekkr-master Tree)

- **Snag the Glitch**: Dramatically increases stun success rates
- **Punish the Helpless**: +50/75/100% damage vs [Stunned] targets
- **Master of Puppets**: Can set up capstone combo vs Extreme Corruption

### External (Party Composition)

- **Corruption appliers**: Rust-Witch, Veiðimaðr increase enemy Corruption → higher stun chance
- **Burst damage**: [Stunned] creates massive damage windows
- **Boss encounters**: High-Corruption bosses become reliable stun targets

---

## Tactical Applications

### Priority Targeting

1. **Extreme Corruption bosses**: 90% stun chance, potential purge at Rank 3
2. **High Corruption elites**: 70% stun for critical disables
3. **Corrupted casters**: Interrupt spellcasting via stun

### Corruption Purge (Rank 3)

The 10-point Corruption purge has strategic implications:

- Reduce boss Corruption below dangerous thresholds
- Counter Corruption-stacking enemy abilities
- Tactical choice: maintain high Corruption (for control bonuses) vs purge (for safety)

### Stress Management

Monitor your Psychic Stress when using this ability:

- 5 Stress per use (3 at Rank 3) adds up quickly
- Reserve for high-value targets
- Consider party Stress mitigation support

---

## v5.0 Compliance Notes

✅ **Trauma Economy Integration**: Psychic Stress cost reflects dangerous Blight contact

✅ **Blight Mechanics**: Corruption-scaling success rates

✅ **Risk-Reward Balance**: Power requires Stress investment

✅ **Technology, Not Magic**: Rune-etched chains channel energy (not spellcasting)