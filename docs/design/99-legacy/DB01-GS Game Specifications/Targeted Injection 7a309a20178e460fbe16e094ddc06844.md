# Targeted Injection

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-TARGETEDINJECTION-v5.0
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
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 / 4 / 4 |
| **Resource Cost** | 35 Stamina + 1 Payload Charge |
| **Cooldown** | 3 turns |
| **Prerequisite** | 8 PP invested in Alka-hestur tree |

---

## Thematic Description

> *"You aim for the weak point—the joint seal, the exposed conduit, the gap in the carapace. Your payload bypasses armor entirely."*
> 

Targeted Injection is the Alka-hestur's **precision strike**. By finding the gaps in armor and defenses, you deliver payloads directly to vulnerable systems, bypassing Soak entirely and maximizing payload potency.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 35 Stamina + 1 Payload Charge
- **Attack**: FINESSE vs target's Defense
- **Damage**: 3d8 Physical (base)
- **Armor Penetration**: This attack **ignores Soak**
- **Potency**: Payload effect at **+50% potency** (damage or duration)
- **Cooldown**: 3 turns

### Rank 2

- **Damage**: 4d8 Physical
- **Potency**: +75% (up from +50%)
- **Debuff**: Target becomes [Vulnerable] for 1 turn
- **PP Cost**: 4

### Rank 3

- **Damage**: 5d8 Physical
- **Potency**: +100% (double effectiveness)
- **Vulnerability Exploitation**: If target has vulnerability to payload element, deal **triple damage** instead of double
- **PP Cost**: 4

---

## Potency Scaling

| Effect Type | Base | +50% | +75% | +100% |
| --- | --- | --- | --- | --- |
| [Burning] (1d6/turn) | 3 turns | 4-5 turns | 5 turns | 6 turns |
| [Corroded] (-3 Soak) | 3 turns | 4-5 turns | 5 turns | 6 turns |
| [Slowed] | 2 turns | 3 turns | 3-4 turns | 4 turns |

---

## Rank 3: Triple Damage vs Vulnerability

**Condition**: Target must have **vulnerability** to payload element type

**Example**:

- Target: Corrupted Servitor (Vulnerable to EMP)
- Payload: EMP
- Base damage: 5d8 = 22.5 avg
- Triple damage: 5d8 × 3 = **67.5 avg**

This synergizes powerfully with **Alchemical Analysis I** for identifying vulnerabilities.

---

## Synergies

### Internal (Alka-hestur Tree)

- **Alchemical Analysis I**: Identify vulnerabilities for triple damage
- **Payload Strike**: Targeted Injection is the "premium" delivery option
- **Perfect Solution**: Capstone auto-hits; Targeted Injection is precision alternative

### External (Party Composition)

- **High-armor enemies**: Soak bypass crucial
- **Controllers**: Locked enemies easier to target precisely
- **Vulnerability-focused parties**: Coordinate element attacks

---

## Tactical Applications

### Priority Targeting

Use Targeted Injection against:

1. **High-Soak enemies**: Armor bypass maximizes damage
2. **Vulnerable enemies**: Triple damage potential (Rank 3)
3. **Priority targets**: Enhanced debuff duration shuts down threats

### Rank 2: [Vulnerable] Setup

The [Vulnerable] debuff (+50% damage from next attack) sets up follow-up attacks from allies.

---

## v5.0 Compliance Notes

✅ **Technology, Not Magic**: Precision targeting, not supernatural accuracy

✅ **Armor Penetration**: Meaningful counter to high-Soak enemies

✅ **Vulnerability Synergy**: Rewards analysis and preparation

✅ **Cooldown Balance**: 3-turn CD prevents spam