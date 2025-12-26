# Disarm Mechanism

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABIL-DISARMMECHANISM-v5.0
Parent item: Ruin-Stalker (System Anomaly Hunter) — Specialization Specification v5.0 (Ruin-Stalker%20(System%20Anomaly%20Hunter)%20%E2%80%94%20Specializat%200719a7284d6f4828a3362ce07a4ad058.md)
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
| **Resource Cost** | 20 Stamina |
| **Cooldown** | None (limited by traps present) |

---

## Thematic Description

> *"Your hands know the logic of Old World design. Springs, triggers, pressure plates—all follow patterns if you've studied them."*
> 

The Ruin-Stalker's hands are trained instruments of precision. You understand the mechanical logic underlying Old World trap design—the tension points, the release mechanisms, the fail-safes. Where others see deadly puzzles, you see **problems with solutions**.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 20 Stamina
- **Check**: FINESSE or WITS vs Trap DC
- **On Success**:
    - Trap neutralized
    - 50% chance to salvage 1 **Trap Component**
- **On Failure**: Trap triggers (you may attempt to dodge)

### Rank 2

- **Bonus**: +2d10 to disarm check
- **Salvage**: 75% chance (up from 50%)
- **New Option**: Can attempt to **redirect trap** to target enemies
- **PP Cost**: 3

### Rank 3

- **Bonus**: +3d10 to disarm check
- **Auto-Success**: Automatic success on **Minor** and **Moderate** traps
- **Safe Severe**: Can disarm **Severe** traps safely (failure doesn't trigger)
- **Salvage**: 100% chance
- **PP Cost**: 3

---

## Trap Difficulty Tiers

| Trap Level | Typical DC | Rank 3 Handling |
| --- | --- | --- |
| **Minor** | DC 10-14 | Automatic success |
| **Moderate** | DC 15-19 | Automatic success |
| **Severe** | DC 20-24 | Safe attempt (no trigger on fail) |
| **Lethal** | DC 25+ | Normal rules apply |

---

## Trap Components

**Salvaged Components** can be used for:

- Crafting new traps (with Master Trapsmith)
- Trading with artificers and engineers
- Repairing damaged equipment
- Creating improvised devices

| Component Type | Source Traps | Common Uses |
| --- | --- | --- |
| **Springs** | Dart, spike traps | Mechanical devices |
| **Triggers** | Pressure plates, tripwires | Alarm systems, new traps |
| **Containers** | Gas, acid traps | Alchemical storage |
| **Mechanisms** | Complex Old World traps | Advanced engineering |

---

## Redirect Trap (Rank 2+)

Instead of disarming, you can **redirect** the trap:

- Same check required
- On success: Trap remains active but now targets enemies
- Trap triggers when enemy enters its zone
- Requires trap to have directional component

---

## Synergies

### Internal (Ruin-Stalker Tree)

- **Anomaly Sense I**: Detection required before disarming
- **Careful Advance**: Reveals traps for disarming
- **Hazard Mapping**: Mark disarmed locations
- **Master Trapsmith**: Salvaged components fuel trap deployment

### External (Party Composition)

- **Artificers**: Appreciate salvaged components
- **Low-FINESSE parties**: Rely on your expertise
- **Stealth teams**: Silently neutralize threats

---

## Tactical Applications

### Standard Protocol

1. Detect trap via Anomaly Sense
2. Approach with Careful Advance
3. Assess trap type and DC
4. Roll Disarm Mechanism
5. Salvage components on success

### Redirect Strategy

- Position redirected traps along enemy patrol routes
- Use as ambush preparation
- Convert hazards into assets

### Failure Contingency

- Rank 3 Severe traps: No trigger on failure
- Below Rank 3: Prepare dodge reaction
- Party should maintain safe distance

---

## v5.0 Compliance Notes

✅ **Technology, Not Magic**: Mechanical skill, not supernatural ability

✅ **Resource Economy**: Trap Components integrate with crafting systems

✅ **Risk-Reward**: Higher ranks reduce failure consequences

✅ **Layer 2 Voice**: Professional survey and salvage terminology