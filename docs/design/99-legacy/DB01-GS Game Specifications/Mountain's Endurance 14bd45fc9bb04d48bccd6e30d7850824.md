# Mountain's Endurance

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-MOUNTAINSENDURANCE-v5.0
Parent item: Gorge-Maw Ascetic (Seismic Monk) — Specialization Specification v5.0 (Gorge-Maw%20Ascetic%20(Seismic%20Monk)%20%E2%80%94%20Specialization%20%200746d736f9a04324976a9f605c4ac276.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Validated: No

## Overview

| Attribute | Value |
| --- | --- |
| **Ability Type** | Passive (Triggered) |
| **Tier** | 3 (Mastery) |
| **PP Cost** | 5 / 5 / 5 |

---

## Thematic Description

> *"You have become the bedrock; you endure when all else crumbles."*
> 

Mountain's Endurance represents the Gorge-Maw's **ultimate defensive ability**. Like the mountain that stands through ages of erosion, you refuse to fall. When death approaches, you simply... don't.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Trigger**: When you would be reduced to **0 HP**
- **Uses**: Once per combat
- **Effect**: Instead drop to **1 HP**
- **Immunity**: Become **immune to damage** until end of your next turn
- **Restriction**: Cannot move during immunity

### Rank 2

- **Trigger**: Activates at **20% HP** (instead of 0 HP)
- **Healing**: Heal **15 HP** when immunity ends
- **PP Cost**: 5

### Rank 3

- **Uses**: Can trigger **twice per combat**
- **Healing**: **25 HP** when immunity ends (up from 15)
- **Resonance**: Generate **50 Resonance** when triggered
- **PP Cost**: 5

---

## Trigger Timing

### Rank 1: Death Prevention

| HP Before Hit | Damage Taken | Result |
| --- | --- | --- |
| 15 | 20 | Trigger → 1 HP + Immunity |
| 1 | 50 | Trigger → 1 HP + Immunity |
| Already at 0 | Any | Cannot trigger (already down) |

### Rank 2: Early Activation

| HP Before Hit | 20% Threshold | Result |
| --- | --- | --- |
| 25 | 20 | Trigger → 25 HP + Immunity |
| 18 | 20 | Trigger → 18 HP + Immunity |
| 50 | 20 | No trigger (above 20%) |

---

## Synergies

### Internal (Gorge-Maw Tree)

- **Stone Skin**: High Soak delays trigger
- **Rooted Stance**: Can't move anyway during immunity
- **Tectonic Fury**: Rank 3 generates 50 Resonance for capstone

### External (Party Composition)

- **Healers**: Immunity window allows emergency healing
- **Damage dealers**: You hold threat while party eliminates threats
- **Controllers**: You survive focus fire

---

## Tactical Applications

### The Unkillable Anchor

1. Position at chokepoint
2. Draw enemy focus
3. Take massive damage
4. Mountain's Endurance triggers
5. Immunity window: healer tops you off
6. Continue fighting

### Rank 3: Resonance Burst

- Trigger generates 50 Resonance instantly
- Often enough for Tectonic Fury (80 Resonance)
- Near-death → Capstone activation
- **Thematic**: Fury born from survival

### Rank 2: Proactive Defense

- 20% threshold means trigger before critical
- More time to recover during immunity
- Less risky than waiting for 0 HP

---

## v5.0 Compliance Notes

✅ **Tank Capstone**: Defines unkillable identity

✅ **Limited Uses**: Once/twice per combat balances power

✅ **Movement Lock**: Prevents exploitation

✅ **Resonance Integration**: Rank 3 fuels capstone