# Tier 1 Ability: Shield Bash

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKJALDMAER-SHIELDBASH-v5.0
Mechanical Role: Controller/Debuffer, Tank/Durability
Parent item: Skjaldmær (Bastion of Coherence) — Specialization Specification v5.0 (Skjaldm%C3%A6r%20(Bastion%20of%20Coherence)%20%E2%80%94%20Specialization%20%2083c338d903f54a5692dbaa63a5cf7b07.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Skjaldmær (Bastion of Coherence) |
| **Tier** | 1 (Foundational) |
| **Type** | Active (Attack) |
| **Prerequisite** | Unlock Skjaldmær Specialization (10 PP) |
| **Cost** | 40 Stamina |
| **Target** | Single enemy (melee range) |

---

## I. Design Context (Layer 4)

### Core Design Intent

Shield Bash is the Skjaldmær's **basic offensive option**—a shield-based attack that deals low damage but can inflict [Staggered]. This establishes the shield as both defensive tool and weapon, reinforcing the "bastion" identity.

### Mechanical Role

- **Primary:** Low-damage melee attack with control component
- **Secondary:** Establishes shield-as-weapon identity
- **Fantasy Delivery:** The brutal, declarative strike of reality against corruption

### Balance Considerations

- **Power Level:** Low damage, high utility (appropriate for tank)
- **Control Trade-off:** Damage is sacrificed for [Staggered] potential
- **Accuracy Requirement:** Needs Solid or Critical Success for control effect

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skjaldmær slams her shield into her foe—not a graceful strike, but a **brutal declaration**. In a world of glitches and corrupted data, this is pure, undeniable physical truth. The shield is coherent matter; the enemy's face is not. The collision makes that hierarchy clear.

### Thematic Resonance

The shield is the Skjaldmær's symbol of stability. When she uses it offensively, she projects that stability outward—literally forcing coherence onto a chaotic enemy. The [Staggered] effect represents the momentary imposition of order.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 40 Stamina
- **Range:** Melee (1 meter)
- **Target:** Single enemy

### Resolution

**Accuracy Check:**

- Roll: MIGHT + Melee Combat
- DC: Target's Defense

### On Hit

- **Damage:** 2d6 Physical (low)
- **Control Effect (Solid/Critical Success only):** Target gains [Staggered] for 1 round

### [Staggered] Effect

- -1 die to all actions
- Cannot use Reactions
- Duration: 1 round

### Resolution Pipeline

1. **Targeting:** Skjaldmær selects enemy within melee range
2. **Cost Payment:** Skjaldmær spends 40 Stamina
3. **Accuracy Check:** Roll MIGHT + Melee Combat vs target Defense
4. **Hit Determination:** Compare roll to Defense
5. **Damage Application:** On any hit, deal 2d6 Physical damage
6. **Control Check:** On Solid or Critical Success, apply [Staggered]
7. **Duration Track:** [Staggered] expires at end of target's next turn

### Success Thresholds

- **Glancing Hit (meet DC):** 2d6 damage, no [Staggered]
- **Solid Hit (exceed DC by 5+):** 2d6 damage + [Staggered]
- **Critical Hit (exceed DC by 10+):** 2d6 damage + [Staggered] + bonus damage die

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- 2d6 Physical damage
- [Staggered] on Solid/Critical Success
- Cost: 40 Stamina

### Rank 2 (Expert — 20 PP)

- 3d6 Physical damage
- [Staggered] on any hit (not just Solid/Critical)
- Cost: 35 Stamina
- **New:** [Staggered] duration increases to 2 rounds

### Rank 3 (Mastery — Capstone)

- 4d6 Physical damage
- [Staggered] on any hit, 2 rounds duration
- Cost: 30 Stamina
- **New:** Can target 2 adjacent enemies (split damage evenly)
- **New:** Enemies [Staggered] by Shield Bash have -2 Accuracy against Skjaldmær specifically

---

## V. Tactical Applications

1. **Interrupt Setup:** [Staggered] prevents enemy Reactions, enabling ally burst damage
2. **Crowd Control:** Debuff dangerous enemy before they can act
3. **Initiative Denial:** -1 die penalty reduces enemy effectiveness
4. **Combo Opener:** Stagger enemy, then allies unload while they're weakened
5. **Reaction Denial:** Prevents enemy from using defensive reactions

---

## VI. Synergies & Interactions

### Positive Synergies

- **High-accuracy Attackers:** [Staggered] enemies are easier to hit
- **Berserkr:** Staggered enemies can't use Reactions to mitigate Berserkr damage
- **Jötun-Reader (Exploit Design Flaw):** Debuff stacking
- **Guardian's Taunt:** Staggered enemies are less dangerous when forced to attack Skjaldmær

### Negative Synergies

- **Damage-focused builds:** Low base damage compared to pure DPS abilities
- **Ranged compositions:** Melee range requirement limits flexibility