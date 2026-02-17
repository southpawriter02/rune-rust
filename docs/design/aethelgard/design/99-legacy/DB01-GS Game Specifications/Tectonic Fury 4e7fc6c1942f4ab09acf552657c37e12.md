# Tectonic Fury

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-TECTONICFURY-v5.0
Parent item: Gorge-Maw Ascetic (Seismic Monk) — Specialization Specification v5.0 (Gorge-Maw%20Ascetic%20(Seismic%20Monk)%20%E2%80%94%20Specialization%20%200746d736f9a04324976a9f605c4ac276.md)
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
| **Ability Type** | Active — Standard Action (Capstone) |
| **Tier** | Capstone |
| **PP Cost** | 6 / 6 / 6 |
| **Resource Cost** | 60 Stamina + 80 Resonance |
| **Duration** | 3 rounds |

---

## Thematic Description

> *"You have learned to speak the old language—the tongue of fault lines and magma chambers. The earth answers your call with apocalyptic violence."*
> 

Tectonic Fury is the **ultimate expression** of the Gorge-Maw Ascetic's mastery. You become an avatar of seismic destruction, transforming the very ground around you into a weapon. Enemies cannot stand, cannot approach, cannot escape the wrath of the deep earth.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 60 Stamina + 80 Resonance
- **Duration**: 3 rounds
- **State**: Enter [Tectonic Fury]

**[Tectonic Fury] Effects:**

- All tiles within **3 tiles** of you become [Unstable Ground]
- At **start of each of your turns**: All enemies on [Unstable Ground] take **3d6 Physical damage** and must make **STURDINESS check DC 14** or be [Knocked Down]
- Your **Resonance generation is doubled**
- You **cannot move**, but gain **+5 Soak**

### Rank 2

- **Damage**: 4d6 Physical (up from 3d6)
- **Save DC**: 16 (up from 14)
- **Radius**: 4 tiles (up from 3)
- **PP Cost**: 6

### Rank 3

- **Damage**: 5d6 Physical
- **Movement**: Can move **2 tiles/turn** (no longer locked)
- **Persistence**: [Unstable Ground] persists for **1 round** after Tectonic Fury ends
- **PP Cost**: 6

---

## State: [Tectonic Fury]

| Round | Effect |
| --- | --- |
| Activation | 80 Resonance spent, area becomes [Unstable Ground] |
| Turn 1 | 3d6+ damage to all enemies in zone, knockdown saves |
| Turn 2 | 3d6+ damage, knockdown, doubled Resonance regeneration |
| Turn 3 | 3d6+ damage, knockdown, final pulse |
| End | State expires (R3: terrain persists 1 round) |

---

## Resonance Interaction

### Building to Tectonic Fury

| Source | Base | During Fury |
| --- | --- | --- |
| Tremor Strike | +15 | +30 |
| Taking damage | +10 | +20 |
| Rooted Stance R3 | +10/turn | +20/turn |
| [Unstable Ground] | +5/turn | +10/turn |

During Tectonic Fury, rapid Resonance regeneration enables:

- Follow-up Earthshaker or Fissure
- Potentially chain into second Tectonic Fury (sustained)

---

## Synergies

### Internal (Gorge-Maw Tree)

- **Rooted Stance**: Stationary during Fury aligns with stance
- **Stone Skin**: +5 Soak stacks with reactive hardening
- **Mountain's Endurance**: R3 generates 50 Resonance toward Fury
- **Seismic Sense**: Detect all enemies in your kill zone

### External (Party Composition)

- **Mobile strikers**: Keep enemies in your zone
- **Controllers**: Chain knockdowns for total lockdown
- **Ranged specialists**: Safe firing while you dominate melee

---

## Tactical Applications

### The Earthquake Protocol

1. Build 80+ Resonance through combat
2. Position at center of engagement
3. Activate Tectonic Fury
4. Allies push/pull enemies into your zone
5. 3 rounds of unavoidable area damage + knockdowns
6. Enemies eliminated or scattered

### Sustained Fury (Rank 3)

- Movement allows repositioning
- Chase fleeing enemies (slowly)
- Extend [Unstable Ground] coverage
- Potentially chain Tectonic Fury activations

### Defensive Anchor

- +5 Soak during Fury
- Combined with Stone Skin + Rooted Stance
- Near-invulnerable while dealing area damage
- Perfect for holding critical positions

---

## v5.0 Compliance Notes

✅ **Capstone Power**: Defines specialization's peak

✅ **High Resonance Cost**: 80 Resonance requires significant buildup

✅ **Sustained Duration**: 3-round state rewards positioning

✅ **Clear Limitations**: Immobility (base) balances power

✅ **Scaling Progression**: Movement unlock at R3 rewards investment