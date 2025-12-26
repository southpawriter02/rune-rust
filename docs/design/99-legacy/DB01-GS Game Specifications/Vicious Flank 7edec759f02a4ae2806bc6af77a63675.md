# Vicious Flank

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-VICIOUSFLANK-v5.0
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
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 / 4 / 4 |
| **Resource Cost** | 40 Stamina |
| **Cooldown** | None |

---

## Thematic Description

> *"Walks the edge of a foe's failing code—when minds stumble or bodies stutter, the Strandhögg is already there."*
> 

Vicious Flank is the Strandhögg's **opportunistic strike**. You exploit every weakness, every faltering moment—the more compromised your target, the more devastating your attack.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 40 Stamina
- **Target**: Single enemy **with negative status effect** (melee range)
- **Attack**: FINESSE vs target's Defense
- **Damage**: 4d8 Physical
- **Scaling**: +2d8 damage for **each negative status effect** on target (max +6d8)

### Rank 2

- **Base Damage**: 5d8 Physical (up from 4d8)
- **Scaling**: +3d8 per status effect (up from +2d8)
- **PP Cost**: 4

### Rank 3

- **Auto-Crit**: **Automatically crits** against targets with **3+ status effects**
- **PP Cost**: 4

---

## Damage Scaling

| Status Effects | R1 Damage | R2 Damage | R3 Crit? |
| --- | --- | --- | --- |
| 1 | 4d8 + 2d8 = 6d8 | 5d8 + 3d8 = 8d8 | No |
| 2 | 4d8 + 4d8 = 8d8 | 5d8 + 6d8 = 11d8 | No |
| 3+ | 4d8 + 6d8 = 10d8 | 5d8 + 9d8 = 14d8 | ✅ Auto-crit |

**Maximum Potential (R3, 3+ effects, crit)**: 28d8 damage

---

## Synergies

### Internal (Strandhögg Tree)

- **Dread Charge**: [Disoriented] enables Vicious Flank
- **Tidal Rush**: Generates Pace when status applied
- **Savage Harvest**: Weaken to execute threshold

### External (Party Composition)

- **Controllers**: Stack effects for maximum scaling
- **Hólmgangr**: Crippling Cut adds [Slowed]
- **Hlekkr-master**: Chain effects feed damage

---

## Tactical Applications

### The Stack Attack

1. Allies apply [Feared], [Slowed], [Staggered]
2. Target has 3+ status effects
3. Vicious Flank R3: Auto-crit
4. 14d8 × 2 = 28d8 damage
5. Target likely eliminated or execute-ready

### Status Effect Sources

- Dread Charge: [Disoriented]
- Hólmgangr Crippling Cut: [Slowed], [Hobbled]
- Hlekkr-master chains: [Restrained]
- Fear effects from various sources

---

## v5.0 Compliance Notes

✅ **Conditional Power**: Requires target debuffs

✅ **Scaling Damage**: Clear progression per status

✅ **Team Synergy**: Rewards coordinated play

✅ **Finisher Potential**: R3 auto-crit enables burst