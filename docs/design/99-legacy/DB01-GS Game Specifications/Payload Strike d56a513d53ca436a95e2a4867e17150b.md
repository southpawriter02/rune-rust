# Payload Strike

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-PAYLOADSTRIKE-v5.0
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
| **Tier** | 1 (Foundational) |
| **PP Cost** | 3 / 3 / 3 |
| **Resource Cost** | 25 Stamina + 1 Payload Charge |
| **Cooldown** | None (limited by payload charges) |

---

## Thematic Description

> *"You drive the lance home and trigger the injector. The payload enters the system. The reaction begins."*
> 

Payload Strike is the Alka-hestur's **core delivery mechanism**. The injector lance pierces the target, delivering the selected alchemical cartridge directly into their system. Physical damage is incidental—the real weapon is the chemistry.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 25 Stamina + 1 Payload Charge
- **Attack**: FINESSE vs target's Defense
- **Damage**: 2d8 Physical (base)
- **Effect**: Apply payload effect based on loaded cartridge type
- **Cooldown**: None (limited by payload charges)

### Rank 2

- **Damage**: 3d8 Physical (up from 2d8)
- **Duration**: Payload effects last **+1 turn**
- **PP Cost**: 3

### Rank 3

- **Damage**: 4d8 Physical
- **Critical Hit**: Payload effect is **doubled** (damage or duration)
- **PP Cost**: 3

---

## Payload Effects Reference

| Payload | Damage Type | Status Effect | Duration |
| --- | --- | --- | --- |
| **Ignition** | Fire | [Burning] (1d6/turn) | 3 turns |
| **Cryo** | Ice | [Slowed] | 2 turns |
| **EMP** | Energy | [System Shock] | 1 turn |
| **Acidic** | Physical | [Corroded] (-3 Soak) | 3 turns |
| **Concussive** | Physical | [Staggered] | 1 turn |
| **Smoke** | None | Obscured area | 2 turns |
| **Marking** | None | Revealed + Tracked | Combat |

---

## Synergies

### Internal (Alka-hestur Tree)

- **Alchemical Analysis I**: Informs payload selection
- **Field Preparation**: Creates payloads for consumption
- **Rack Expansion**: More payloads = more strikes
- **Cocktail Mixing**: Enables dual-payload strikes

### External (Party Composition)

- **Controllers**: Locked enemies can't dodge injections
- **Tanks**: Create openings for your precise delivery
- **Follow-up damage**: [Corroded] enables party damage

---

## Tactical Applications

### Payload Selection Guide

| Situation | Recommended Payload |
| --- | --- |
| Organic enemy | Ignition |
| Fast enemy | Cryo |
| Mechanical/Undying | EMP |
| High-armor | Acidic |
| Caster | Concussive |
| Need escape | Smoke |
| Invisible enemy | Marking |

### Critical Hit (Rank 3)

Doubled effects on critical:

- [Burning]: 2d6/turn or 6 turns
- [Corroded]: -6 Soak or 6 turns
- [Slowed]: 4 turns

---

## v5.0 Compliance Notes

✅ **Technology, Not Magic**: Injector lance, alchemical cartridges

✅ **Resource Management**: Payload charges create strategic decisions

✅ **Elemental Versatility**: Multiple damage types available

✅ **Layer 2 Voice**: Clinical delivery terminology