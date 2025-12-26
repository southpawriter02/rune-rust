---
id: ABILITY-ECHO-CALLER-28017
title: "Echo Displacement"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Echo Displacement

**Type:** Active [Echo] | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 50 Aether + 3 Psychic Stress |
| **Damage Type** | Psychic |
| **Status Effect** | [Disoriented] |
| **Tags** | [Echo] |
| **Ranks** | None (full power when unlocked) |

---

## Description

You tear the target's spatial awareness apart, forcibly relocating them through a moment of pure psychic dislocation. The violence to reality echoes.

---

## Mechanical Effect

**Forced Teleportation:**
- Teleport enemy to any unoccupied tile within 10 tiles
- No line-of-sight required
- Target takes 4d8 Psychic damage
- Apply [Disoriented] for 1 turn
- **COST:** +3 Psychic Stress (forceful reality manipulation)
- **[Echo Chain]:** Adjacent enemy teleported to random tile within 3 tiles

**Formula:**
```
Target.Position = TargetTile  // Within 10 tiles, no LOS required
Damage = Roll(4d8, "Psychic")
Target.AddStatus("Disoriented", Duration: 1)

Caster.PsychicStress += 3

// Echo Chain
If AdjacentEnemy exists:
    RandomTile = GetRandomTileWithin(3, AdjacentEnemy.Position)
    AdjacentEnemy.Position = RandomTile
```

**Tooltip:** "Echo Displacement: Teleport enemy within 10 tiles. 4d8 Psychic + [Disoriented]. Echo Chain: Adjacent also teleported randomly. Cost: 50 Aether + 3 Stress"

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Priority target | Teleport healer into melee |
| Environmental hazard | Teleport enemy into danger |
| Formation break | Scatter clustered enemies |
| Isolation | Teleport tank away from allies |

---

## Psychic Stress Cost

The +3 Psychic Stress represents the mental strain of forcibly rewriting spatial reality. This is a significant cost that must be managed.

---

## Combat Log Examples

- "Echo Displacement: Teleporting [Enemy] to [location]..."
- "[Enemy] takes 22 Psychic damage and is [Disoriented]!"
- "+3 Psychic Stress (Echo Displacement cost)"
- "Echo Chain: [Adjacent Enemy] teleported to random location!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Echo-Caller Overview](../echo-caller-overview.md) | Parent specialization |
| [Stress System](../../../../01-core/resources/stress.md) | Psychic Stress mechanics |
