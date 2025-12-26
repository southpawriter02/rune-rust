---
id: ABILITY-SCRAP-TINKER-26009
title: "Deploy Scrap Golem"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Deploy Scrap Golem

**Type:** Active (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Self (deploys golem) |
| **Resource Cost** | 50 Stamina + 50 Scrap |
| **Cooldown** | Once per expedition |
| **Assembly Time** | 1 hour (out-of-combat) |
| **Duration** | Until destroyed or expedition ends |
| **Special** | Training upgrades all Tier 1, 2, & 3 abilities to Rank 3 |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Your masterpiece. A walking junk pile animated by salvaged power cores. Loyal. Brutal. Yours.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- Deploy Scrap Golem (40 HP, 6 Armor, immune to psychic effects)
- Acts on your turn
- **Slam:** 3d10 Physical damage
- **Defend:** Grant adjacent ally +3 Soak
- Requires 50 Scrap Materials, 1 hour assembly

**Formula:**
```
SpawnPet("ScrapGolem", HP: 40, Armor: 6)
Golem.Immunity = ["Psychic"]
Golem.Ability("Slam"): Roll(3d10, "Physical")
Golem.Ability("Defend"): AdjacentAlly.Soak += 3
ScrapCost = 50
AssemblyTime = 1 hour
```

**Tooltip:** "Deploy Scrap Golem (Rank 1): 40 HP, 6 Armor. Slam: 3d10. Defend: +3 Soak. Cost: 50 Stamina, 50 Scrap"

---

### Rank 2 (Unlocked: Based on tree progression)

**Effect:**
- Golem: 60 HP, 8 Armor
- Slam: 4d10 damage
- **NEW:** Repair Protocol: Once per combat, self-heal 20 HP
- **NEW:** Can carry 50 extra Scrap capacity

**Formula:**
```
Golem.HP = 60
Golem.Armor = 8
Golem.Slam = Roll(4d10)
Golem.Ability("RepairProtocol"): Golem.HP += 20 (1/combat)
Golem.ScrapCapacity = 50
```

**Tooltip:** "Deploy Scrap Golem (Rank 2): 60 HP, 8 Armor. Slam: 4d10. Repair: +20 HP 1x/combat. +50 Scrap capacity."

---

### Rank 3 (Unlocked: Full tree completion)

**Effect:**
- Golem: 80 HP, 10 Armor
- Slam: 5d10 damage
- **NEW:** Detonate: Command golem to self-destruct (8d10 damage, 5x5 AoE)
- **NEW:** Can rebuild golem for 25 Scrap (half cost)

**Formula:**
```
Golem.HP = 80
Golem.Armor = 10
Golem.Slam = Roll(5d10)
Golem.Ability("Detonate"): AoE(5x5, Roll(8d10)), DestroyGolem()
RebuildCost = 25 Scrap
```

**Tooltip:** "Deploy Scrap Golem (Rank 3): 80 HP, 10 Armor. Slam: 5d10. Detonate: 8d10 AoE. Rebuild: 25 Scrap."

---

## Golem Stats by Rank

| Property | Rank 1 | Rank 2 | Rank 3 |
|----------|--------|--------|--------|
| HP | 40 | 60 | 80 |
| Armor | 6 | 8 | 10 |
| Slam | 3d10 | 4d10 | 5d10 |
| Defend | +3 Soak | +3 Soak | +3 Soak |
| Repair | — | 20 HP/combat | 20 HP/combat |
| Detonate | — | — | 8d10 AoE |
| Scrap Carry | — | 50 | 50 |

---

## Golem Abilities

### Slam (Standard Action)
- Melee attack dealing Physical damage
- Primary offensive ability

### Defend (Standard Action)
- Grant adjacent ally +3 Soak until your next turn
- Defensive utility

### Repair Protocol (Free Action, 1/combat)
- Self-heal 20 HP
- Available at Rank 2+

### Detonate (Standard Action, Destroys Golem)
- 8d10 damage in 5x5 AoE
- Sacrifices golem
- Available at Rank 3

---

## Combat Log Examples

- "Scrap Golem deployed! 40 HP, 6 Armor. Ready for orders."
- "Scrap Golem SLAM: 24 Physical damage to [Enemy]!"
- "Scrap Golem DEFEND: [Ally] gains +3 Soak."
- "Scrap Golem REPAIR PROTOCOL: +20 HP (40 → 60)"
- "Scrap Golem DETONATE! 48 damage to 4 enemies in blast!"
- "Scrap Golem destroyed. Rebuild available for 25 Scrap."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Scrap-Tinker Overview](../scrap-tinker-overview.md) | Parent specialization |
| [Deploy Scout Drone](deploy-scout-drone.md) | Tier 2 pet |
