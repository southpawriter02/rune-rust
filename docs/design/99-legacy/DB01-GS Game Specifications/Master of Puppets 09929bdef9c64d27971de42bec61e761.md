# Master of Puppets

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-MASTEROFPUPPETS-v5.0
Parent item: Hlekkr-master (Glitch Exploiter) — Specialization Specification v5.0 (Hlekkr-master%20(Glitch%20Exploiter)%20%E2%80%94%20Specialization%20%20e50dff9a1cb14a8fb79a7ba71da8f771.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Capstone
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Validated: No

## Overview

| Attribute | Value |
| --- | --- |
| **Ability Type** | Capstone (Passive + Active) |
| **Tier** | Capstone |
| **PP Cost** | 6 / 6 / 6 |
| **Active Cost** | 50 Stamina (Once per combat) |
| **Prerequisite** | 24 PP invested + any Tier 3 ability |

---

## Thematic Description

> *"You have achieved perfect understanding of battlefield manipulation in a corrupted world. You see only glitching pieces to be moved at will."*
> 

The Master of Puppets has transcended mere chain-work. You understand that corrupted enemies aren't threats—they're **pieces on a board**, and you hold all the strings. Every pull weakens, every displacement creates opportunity, and when the moment is right, you can weaponize an enemy's own corruption against the entire battlefield.

---

## Mechanical Implementation

### Passive Component: Vulnerable on Displacement

**Base Effect (Rank 1)**:

- Whenever you **Pull or Push** an enemy, they become [Vulnerable] (1 turn)
- [Vulnerable]: Target takes **+50% damage** from next attack

**Rank 2**:

- [Vulnerable] duration extended to **2 turns**
- **PP Cost**: 6

**Rank 3**:

- +2d10 bonus to all Pull/Push attempts
- **PP Cost**: 6

---

### Active Component: Corruption Cascade

**Trigger**: Bonus Action — **Once per combat**

**Cost**: 50 Stamina

**Target**: Single enemy with **Extreme Corruption** (90+)

**Mechanic**:

1. Make opposed **FINESSE check** vs target
2. On success: Trigger **catastrophic feedback loop**
3. Target's corruption explodes outward
4. **8d10 Psychic damage to ALL OTHER enemies**
5. Original target takes no damage (serves as conduit)

**Rank 2**:

- Damage increased to **10d10 Psychic**
- **PP Cost**: 6

**Rank 3**:

- Can target **High Corruption** (60+) enemies instead of requiring Extreme
- **PP Cost**: 6

---

## Status Effect: [Vulnerable]

**Duration**: 1/2 turns (by rank)

**Effects**:

- Target takes **+50% damage from next attack** that hits them
- Stacks multiplicatively with Punish the Helpless bonus
- Consumed when damage is dealt OR duration expires

---

## Damage Calculation: Corruption Cascade

### Base (Rank 1): 8d10

- Average: 44 Psychic damage
- Range: 8-80 Psychic damage
- **To all enemies except conduit**

### Rank 2: 10d10

- Average: 55 Psychic damage
- Range: 10-100 Psychic damage

### Damage Amplification

With Punish the Helpless active on multiple enemies:

- Base 44 × 1.5 = **66 average** (Rank 1 Punish)
- Base 44 × 2.0 = **88 average** (Rank 3 Punish)

---

## Synergies

### Internal (Hlekkr-master Tree)

- **Grappling Hook Toss**: Pull → [Vulnerable] (automatic combo)
- **Chain Scythe**: [Knocked Down] enemies remain vulnerable to Cascade
- **Snag the Glitch**: Increased success on Cascade FINESSE check vs corrupted
- **Punish the Helpless**: [Vulnerable] + control bonus = massive damage multipliers

### External (Party Composition)

- **Corruption appliers**: Rust-Witch, Veiðimaðr push enemies to Extreme for Cascade
- **Burst damage**: Time big hits for [Vulnerable] windows after pulls
- **AoE specialists**: Cascade softens entire battlefield for follow-up

---

## Tactical Applications

### Passive: Pull → Vulnerable Loop

Every Grappling Hook Toss now grants [Vulnerable]:

1. Pull target to Front Row
2. Target gains [Disoriented] + [Vulnerable]
3. Ally strikes for +50% damage
4. Repeat with next pull

### Active: Cascade Setup

Optimal Corruption Cascade timing:

1. Control multiple enemies (Chain Scythe sweep)
2. Identify Extreme Corruption conduit
3. Trigger Cascade for 8-10d10 to all others
4. Punish the Helpless amplifies Cascade damage vs controlled targets

### Rank 3 Threshold Reduction

High Corruption (60+) is much more common than Extreme (90+):

- Cascade becomes available in most mid-game encounters
- Strategic value increases dramatically
- Enables Cascade against bosses before they reach Extreme

---

## Combat Example

**Setup**: 4 enemies, 1 with Extreme Corruption (92)

**Turn 1**: Chain Scythe all Front Row → [Slowed]

**Turn 2**: Grappling Hook Toss Back Row caster → [Disoriented] + [Vulnerable]

**Turn 3**: Trigger Cascade on Extreme target

**Result**:

- 8d10 (avg 44) Psychic to 3 enemies
- Punish the Helpless (+100% at Rank 3): 88 damage each
- Total: **264 damage** to non-conduit enemies
- Conduit survives for continued control exploitation

---

## v5.0 Compliance Notes

✅ **Capstone Power Level**: Battlefield-wide damage potential appropriate for 33 PP investment

✅ **Corruption Integration**: Requires Extreme/High Corruption target as conduit

✅ **Technology, Not Magic**: Exploiting physical corruption resonance, not spellcasting

✅ **Once Per Combat**: Appropriately gated ultimate ability

✅ **Synergy Design**: Rewards full tree investment with multiplicative bonuses