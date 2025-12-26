# Area Saturation

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-AREASATURATION-v5.0
Parent item: Alka-hestur (Combat Alchemist) — Specialization Specification v5.0 (Alka-hestur%20(Combat%20Alchemist)%20%E2%80%94%20Specialization%20Sp%207a5ef9641dbd40a48db84245bc6540f1.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Validated: No

## Overview

| Attribute | Value |
| --- | --- |
| **Ability Type** | Active — Standard Action |
| **Tier** | 3 (Mastery) |
| **PP Cost** | 5 / 5 / 5 |
| **Resource Cost** | 45 Stamina + 3 Payload Charges (same type) |
| **Cooldown** | 4 turns |
| **Prerequisite** | 16 PP invested in Alka-hestur tree |

---

## Thematic Description

> *"Sometimes the answer isn't a precise injection—it's a cloud, a splash, a wave of reagent that catches everything in the zone."*
> 

Area Saturation is the Alka-hestur's **mass delivery system**. By expending multiple payloads simultaneously, you saturate an entire area with alchemical effects, hitting every enemy in the zone.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 45 Stamina + 3 Payload Charges (same type)
- **Target**: All enemies in **3×3 area**
- **Damage**: 4d8 elemental damage (based on payload type)
- **Effect**: Apply payload effect to **all targets**
- **Cooldown**: 4 turns

### Rank 2

- **Damage**: 5d8 (up from 4d8)
- **Area**: **4×4** (up from 3×3)
- **Duration**: Payload effects last **+1 turn**
- **PP Cost**: 5

### Rank 3

- **Damage**: 6d8
- **Area**: **5×5**
- **Cocktail Compatible**: Can use with Cocktail payloads (all enemies receive both effects)
- **Center Bonus**: Enemies in center tile take **+50% damage**
- **PP Cost**: 5

---

## Area Progression

| Rank | Area Size | Max Targets | Damage |
| --- | --- | --- | --- |
| 1 | 3×3 | 9 | 4d8 |
| 2 | 4×4 | 16 | 5d8 |
| 3 | 5×5 | 25 | 6d8 |

---

## Payload Area Effects

| Payload | Area Effect |
| --- | --- |
| **Ignition** | Fire cloud, all enemies [Burning] |
| **Cryo** | Frost wave, all enemies [Slowed] |
| **EMP** | Energy pulse, all mechanical [System Shock] |
| **Acidic** | Acid splash, all enemies [Corroded] |
| **Concussive** | Shockwave, all enemies [Staggered] |
| **Smoke** | Dense cloud, area obscured |

---

## Synergies

### Internal (Alka-hestur Tree)

- **Rack Expansion**: Capacity for 3 same-type payloads required
- **Cocktail Mixing**: Rank 3 enables AoE dual-effect delivery
- **Field Preparation**: Stock up on saturation-ready payload sets

### External (Party Composition)

- **Hlekkr-master**: Displace enemies into saturation zone
- **Controllers**: Group enemies for maximum targets
- **AoE synergy**: Stack with other party AoE

---

## Tactical Applications

### Optimal Scenarios

- Enemy groups clustered together
- Chokepoint defense
- Ambush initiation
- Mass debuff application

### Rank 3: Cocktail Saturation

Using Cocktail payloads with Area Saturation:

- Cost: 6 charges (3×2 for cocktail)
- Effect: All enemies in 5×5 receive both payload effects
- Example: Fire + Acidic = [Burning] + [Corroded] to entire group

---

## v5.0 Compliance Notes

✅ **Technology, Not Magic**: Dispersal device, not spell

✅ **Resource Intensive**: 3 payloads creates meaningful cost

✅ **AoE Power Level**: Appropriate for Tier 3

✅ **Cooldown Balance**: 4 turns prevents spam