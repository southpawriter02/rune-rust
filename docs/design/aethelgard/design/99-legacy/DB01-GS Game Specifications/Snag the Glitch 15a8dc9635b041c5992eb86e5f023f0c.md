# Snag the Glitch

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-SNAGTHEGLITCH-v5.0
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

> *"You've learned to read the Blight's rhythm. You anticipate stutters and flickers, making your chains preternaturally accurate against corrupted foes."*
> 

Corrupted enemies exist in a state of spatial instability—their bodies glitch, stutter, and phase as the Blight eats at their connection to reality. Where others see chaos, the Hlekkr-master sees **predictable patterns**. You've trained your eye to anticipate when a corrupted body will flicker *into* your chains rather than away from them.

---

## Mechanical Implementation

### Base Effect (Rank 1)

Your control effects gain increased success chance against corrupted enemies based on their Corruption level:

| Corruption Level | Range | Control Success Bonus |
| --- | --- | --- |
| Low | 1-29 | +10% |
| Medium | 30-59 | +20% |
| High | 60-89 | +40% |
| Extreme | 90+ | +60% |

### Rank 2

- **Doubled Bonuses**: All success bonuses doubled (see table below)
- **Damage Bonus**: +1d10 damage vs corrupted enemies
- **PP Cost**: 4

| Corruption Level | Range | Rank 2 Bonus |
| --- | --- | --- |
| Low | 1-29 | +20% |
| Medium | 30-59 | +40% |
| High | 60-89 | +80% |
| Extreme | 90+ | +100% (cannot miss) |

### Rank 3

- **Damage Bonus**: +3d10 damage vs corrupted enemies (up from +1d10)
- **Guaranteed Control**: Control effects **cannot miss** vs Extreme Corruption (90+)
- **PP Cost**: 4

---

## Affected Abilities

Snag the Glitch applies to all control effects from the Hlekkr-master tree:

| Ability | Control Effect |
| --- | --- |
| Netting Shot | [Rooted] application |
| Grappling Hook Toss | Pull + [Disoriented] |
| Unyielding Grip | [Seized] application |
| Chain Scythe | [Slowed] / [Knocked Down] |
| Corruption Siphon Chain | [Stunned] application |

---

## Design Philosophy

Snag the Glitch represents the **core identity** of Hlekkr-master's Blight exploitation:

1. **Inverted Threat**: Corruption normally makes enemies more dangerous; here it makes them more *controllable*
2. **Scaling Reward**: The more corrupted the battlefield, the more dominant the Hlekkr-master becomes
3. **Team Utility**: Your increased control success protects allies from high-Corruption threats

---

## Synergies

### Internal (Hlekkr-master Tree)

- **All control abilities**: Direct beneficiary of success bonuses
- **Punish the Helpless**: More successful controls = more damage triggers
- **Master of Puppets**: Guaranteed control vs Extreme Corruption enables capstone combo

### External (Party Composition)

- **Corruption appliers**: Rust-Witch, Veiðimaðr can *increase* enemy Corruption to boost your control
- **Blight-focused encounters**: Hlekkr-master excels when enemies are already corrupted

---

## Tactical Applications

### Priority Targeting

Always identify the **most corrupted enemy** on the battlefield—they're your guaranteed control target.

### Corruption Farming

In prolonged fights, enemy Corruption often increases naturally. Patient Hlekkr-masters become *more* effective as combat continues.

### Boss Encounters

Many late-game bosses have Extreme Corruption. Snag the Glitch transforms these encounters by providing reliable crowd control against otherwise control-resistant foes.

---

## v5.0 Compliance Notes

✅ **Blight Integration**: Core mechanical identity tied to Corruption system

✅ **Scaling Design**: Power increases with Corruption level (not flat bonus)

✅ **Technology, Not Magic**: Reading patterns, anticipating glitches—skill, not sorcery

✅ **Team Synergy**: Creates explicit synergy with Corruption-applying party members