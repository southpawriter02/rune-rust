---
id: ABILITY-GORGE-MAW-26018
title: "Earthshaker"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Earthshaker

**Type:** Active (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All Ground Enemies |
| **Resource Cost** | 60 Stamina |
| **Damage Type** | Physical |
| **Cooldown** | Once Per Combat |
| **Status Effect** | [Knocked Down], [Vulnerable] |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |
| **Tags** | [Capstone], [AoE], [Terrain] |
| **Ranks** | None (full power when unlocked) |

---

## Description

The ultimate expression of seismic mastery. You command the earth itself to rise up in a massive earthquake, devastating all ground-based enemies and permanently reshaping the battlefield.

---

## Mechanical Effect

**Battlefield-Wide Assault:**
- Deal 6d10 + MIGHT Physical damage to ALL ground enemies
- Apply [Knocked Down] for 2 turns (guaranteed, no save)
- Apply [Vulnerable] for 1 turn
- Create permanent [Difficult Terrain] (5×5 area) with Cover

**Flying Exclusion:**
- Flying enemies are COMPLETELY UNAFFECTED (Tremorsense limitation)

**Formula:**
```
For each Enemy where NOT Enemy.IsFlying:
    Damage = Roll(6d10) + MIGHT
    Enemy.TakeDamage(Damage, "Physical")
    Enemy.AddStatus("KnockedDown", Duration: 2)
    Enemy.AddStatus("Vulnerable", Duration: 1)
    Log("Earthshaker: {Enemy} takes {Damage} and is Knocked Down!")

CreatePermanentTerrain(
    Type: "DifficultTerrain",
    Size: 5x5,
    CenterPoint: Caster.Position,
    GrantsCover: true
)

// Warning for flying enemies
For each Enemy where Enemy.IsFlying:
    Log("WARNING: {Enemy} is unaffected by Earthshaker (flying)")
```

**Tooltip:** "Earthshaker: 6d10+MIGHT to ALL ground enemies. Knocked Down 2 turns + Vulnerable. Creates permanent terrain. Once/combat."

---

## Damage Output

| Metric | Value |
|--------|-------|
| Base Dice | 6d10 |
| Average (MIGHT +4) | 37 |
| Minimum | 10 |
| Maximum | 64 |

---

## [Knocked Down] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 2 turns |
| **Attack Penalty** | -2 dice to all attack rolls |
| **Defense Penalty** | +2 dice to attacks against this target |
| **Recovery** | Standing up costs a Standard Action |

---

## Permanent Terrain Creation

| Property | Value |
|----------|-------|
| **Size** | 5×5 tiles (25 total) |
| **Center** | Caster's position |
| **Type** | Difficult Terrain |
| **Bonus** | Grants Cover to occupants |
| **Duration** | Permanent (for combat) |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Mass encounter | Devastate multiple ground enemies |
| Boss + adds | Lock down everything grounded |
| Defensive setup | Create permanent cover zone |
| Combo finisher | Use after Earthen Grasp for maximum impact |

---

## Optimal Setup

1. **Earthen Grasp** → Root all enemies (3 turns)
2. **Party damage** → Exploit Vulnerable status
3. **Earthshaker** → Devastating AoE while enemies still Rooted
4. **Result:** Enemies Rooted + Knocked Down + Vulnerable

---

## Flying Enemy Limitation

**CRITICAL:** Earthshaker has NO EFFECT on flying enemies:
- No damage
- No Knocked Down
- No Vulnerable
- They don't even feel the earthquake

This is the fundamental trade-off of Tremorsense mastery.

---

## Combat Log Examples

- "EARTHSHAKER! The battlefield trembles!"
- "5 ground enemies take massive damage and are Knocked Down!"
- "Permanent Difficult Terrain created (5×5 with Cover)"
- "WARNING: [Flying Enemy] is unaffected by Earthshaker"
- "[Knocked Down Enemy] cannot act effectively (-2 dice)"

---

## GUI Display - Capstone Notification

```
┌─────────────────────────────────────────────┐
│            EARTHSHAKER ACTIVATED!           │
├─────────────────────────────────────────────┤
│                                             │
│  The earth itself obeys your command!       │
│                                             │
│  • ALL ground enemies take massive damage   │
│  • ALL ground enemies Knocked Down          │
│  • Permanent Difficult Terrain created      │
│                                             │
│  "The world trembles at your feet."         │
│                                             │
└─────────────────────────────────────────────┘
```

- Screen shake effect
- Terrain visually transforms
- Once-per-combat indicator grays out

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Gorge-Maw Ascetic Overview](../gorge-maw-ascetic-overview.md) | Parent specialization |
| [Tremorsense](tremorsense.md) | Flying limitation source |
| [Earthen Grasp](earthen-grasp.md) | Setup ability |
| [Rooted Status](../../../../04-systems/status-effects/rooted.md) | Related status |
