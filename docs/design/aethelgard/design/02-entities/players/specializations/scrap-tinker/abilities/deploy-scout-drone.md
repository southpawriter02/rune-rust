---
id: ABILITY-SCRAP-TINKER-26004
title: "Deploy Scout Drone"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Deploy Scout Drone

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Self (deploys drone) |
| **Resource Cost** | 40 Stamina + 15 Scrap |
| **Cooldown** | 4 turns |
| **Duration** | Until destroyed or dismissed |
| **Ranks** | 2 → 3 |

---

## Description

The jerry-rigged drone buzzes to life. Its optics scan the battlefield, feeding you tactical data.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Deploy Scout Drone (15 HP, 2 Armor)
- Grants vision in 7x7 area
- Reveals hidden enemies and traps
- Moves 3 spaces per turn (your command)
- **NEW:** Can mark priority targets (+1 ally to hit vs marked enemy)

**Formula:**
```
SpawnPet("ScoutDrone", HP: 15, Armor: 2)
Drone.VisionRadius = 7
Drone.MovementSpeed = 3
Drone.Ability("MarkTarget"): MarkedEnemy.AlliesHitBonus += 1
Duration = UNTIL_DESTROYED
```

**Tooltip:** "Deploy Scout Drone (Rank 2): 15 HP, 2 Armor. 7x7 vision. Mark targets: +1 ally hit. Cost: 40 Stamina, 15 Scrap"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Drone has 20 HP, 4 Armor
- Vision radius: 10x10
- **NEW:** Can self-destruct for 4d6 damage (3x3 AoE, destroys drone)

**Formula:**
```
Drone.HP = 20
Drone.Armor = 4
Drone.VisionRadius = 10
Drone.Ability("SelfDestruct"): AoE(3x3, Roll(4d6)), DestroyDrone()
```

**Tooltip:** "Deploy Scout Drone (Rank 3): 20 HP, 4 Armor. 10x10 vision. Can self-destruct: 4d6 AoE."

---

## Drone Stats by Rank

| Property | Rank 2 | Rank 3 |
|----------|--------|--------|
| HP | 15 | 20 |
| Armor | 2 | 4 |
| Vision | 7x7 | 10x10 |
| Movement | 3 | 3 |
| Mark Target | +1 hit | +1 hit |
| Self-Destruct | — | 4d6 AoE |

---

## Combat Log Examples

- "Scout Drone deployed! 15 HP, 2 Armor. Vision active."
- "Scout Drone: Marked [Enemy]. Allies gain +1 to hit."
- "Scout Drone moves to reveal hidden area..."
- "Scout Drone SELF-DESTRUCT! 18 damage to 2 enemies!"
- "Scout Drone destroyed by enemy attack."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Scrap-Tinker Overview](../scrap-tinker-overview.md) | Parent specialization |
| [Deploy Scrap Golem](deploy-scrap-golem.md) | Advanced pet |
