# Master Alchemist

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-MASTERALCHEMIST-v5.0
Parent item: Alka-hestur (Combat Alchemist) — Specialization Specification v5.0 (Alka-hestur%20(Combat%20Alchemist)%20%E2%80%94%20Specialization%20Sp%207a5ef9641dbd40a48db84245bc6540f1.md)
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
| **Active Cost** | 50 Stamina + 1 Payload Charge (Once per combat) |
| **Prerequisite** | 24 PP invested + any Tier 3 ability |

---

## Thematic Description

> *"You have transcended the distinction between alchemist and weapon. Your touch delivers ruin calibrated to molecular precision. Every enemy has a solution—you carry all of them."*
> 

The Master Alchemist has achieved **perfect synthesis**—the ability to analyze any target and create the exact payload that will destroy them. Your chemistry is no longer preparation; it's instinct.

---

## Mechanical Implementation

### Passive Component: Universal Reagent

**Base Effect (Rank 1)**:

- Your payloads deal **+2d8 bonus damage** of their element
- Payload effects last **+2 turns** baseline
- You can identify **ALL creature vulnerabilities automatically** (no check required)

**Rank 2**:

- **+3d8 bonus** (up from +2d8)
- Enemies struck by your payloads have **-2 to saves** vs subsequent payload effects
- **PP Cost**: 6

**Rank 3**:

- **+4d8 bonus**
- Once per combat, your payload can apply its effect **twice** (double duration AND double damage over time)
- **PP Cost**: 6

---

### Active Component: Perfect Solution

**Trigger**: Standard Action — **Once per combat**

**Cost**: 50 Stamina + 1 Payload Charge

**Effect**:

1. Analyze target instantly
2. Create **custom payload** designed for their specific vulnerabilities
3. This attack **automatically hits** (no roll required)
4. Deal **8d10 damage** of target's **weakest resistance type**
5. Apply [Perfect Exploitation]: Target takes **+100% damage from all sources** for 2 turns

**Rank 2**:

- **10d10 damage** (up from 8d10)
- [Perfect Exploitation] lasts **3 turns**
- **PP Cost**: 6

**Rank 3**:

- **12d10 damage**
- **Payload Recovery**: If target is killed, recover **ALL expended payload charges** from precision synthesis
- **PP Cost**: 6

---

## Damage Calculation: Perfect Solution

| Rank | Base Damage | Average | [Perfect Exploitation] |
| --- | --- | --- | --- |
| 1 | 8d10 | 44 | 2 turns |
| 2 | 10d10 | 55 | 3 turns |
| 3 | 12d10 | 66 | 3 turns + charge recovery |

---

## [Perfect Exploitation] Status

**Duration**: 2/3 turns (by rank)

**Effect**: Target takes **+100% damage from ALL sources**

This is a **party-wide damage amplifier**:

- Your 30 damage hit = 60 actual
- Ally's 50 damage hit = 100 actual
- AoE effects doubled against this target

---

## Synergies

### Internal (Alka-hestur Tree)

- **Alchemical Analysis I**: Capstone auto-analysis supersedes
- **All payload abilities**: +2-4d8 bonus damage baseline
- **Targeted Injection**: Use Perfect Solution on boss, Targeted on adds

### External (Party Composition)

- **Burst damage**: Coordinate big hits during [Perfect Exploitation]
- **Boss encounters**: Perfect Solution initiates damage windows
- **Kill confirmation**: Rank 3 recovery rewards finishing blows

---

## Tactical Applications

### Boss Opener

1. Perfect Solution (auto-hit, 8-12d10)
2. [Perfect Exploitation] applied
3. Party focuses target for +100% damage
4. 2-3 turns of massive amplified damage

### Rank 3: Payload Economy

If Perfect Solution kills the target:

- Recover ALL expended payloads
- Full rack restored
- Enables sustained combat after burst

---

## v5.0 Compliance Notes

✅ **Capstone Power Level**: Auto-hit + massive damage + party amplification

✅ **Technology, Not Magic**: Perfect chemical analysis, not supernatural

✅ **Once Per Combat**: Appropriately gated ultimate

✅ **33 PP Investment**: Power level justified by full tree commitment