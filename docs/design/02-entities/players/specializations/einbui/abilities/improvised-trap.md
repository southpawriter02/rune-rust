---
id: ABILITY-EINBUI-27011
title: "Improvised Trap"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Improvised Trap

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Tile within 2 tiles |
| **Resource Cost** | 15 Stamina + 1 [Scrap Metal] + 1 [Tough Leather] |
| **Status Effect** | [Rooted], [Bleeding] |
| **Tags** | [Trap], [Crafting] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Using salvaged materials, you construct a crude but effective trap. When triggered, it snares the target's leg, preventing movement and potentially causing bleeding wounds.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Place trap on any unoccupied tile within 2 tiles
- First enemy to enter: [Rooted] for 1 turn
- Trap is consumed on trigger

**Formula:**
```
PlaceTrap(TargetTile, Range: 2)
Caster.Stamina -= 15
Caster.Inventory.Remove("Scrap Metal", 1)
Caster.Inventory.Remove("Tough Leather", 1)

OnEnemyEnterTrap:
    Enemy.AddStatus("Rooted", Duration: 1)
    Trap.Destroy()
```

**Tooltip:** "Improvised Trap (Rank 1): Place trap within 2 tiles. Triggers [Rooted] 1 turn. Cost: 15 Stamina + materials."

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Place trap on any unoccupied tile within 2 tiles
- First enemy to enter: [Rooted] for 2 turns
- Trap is consumed on trigger

**Formula:**
```
PlaceTrap(TargetTile, Range: 2)
Caster.Stamina -= 15
Caster.Inventory.Remove("Scrap Metal", 1)
Caster.Inventory.Remove("Tough Leather", 1)

OnEnemyEnterTrap:
    Enemy.AddStatus("Rooted", Duration: 2)
    Trap.Destroy()
```

**Tooltip:** "Improvised Trap (Rank 2): Place trap within 2 tiles. Triggers [Rooted] 2 turns. Cost: 15 Stamina + materials."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Place trap on any unoccupied tile within 2 tiles
- First enemy to enter: [Rooted] for 2 turns + [Bleeding] 1d4/turn for 3 turns
- Trap is consumed on trigger

**Formula:**
```
PlaceTrap(TargetTile, Range: 2)
Caster.Stamina -= 15
Caster.Inventory.Remove("Scrap Metal", 1)
Caster.Inventory.Remove("Tough Leather", 1)

OnEnemyEnterTrap:
    Enemy.AddStatus("Rooted", Duration: 2)
    Enemy.AddStatus("Bleeding", Duration: 3, DamagePerTurn: "1d4")
    Trap.Destroy()
```

**Tooltip:** "Improvised Trap (Rank 3): [Rooted] 2 turns + [Bleeding] 1d4 for 3 turns. Cost: 15 Stamina + materials."

---

## Material Requirements

| Material | Quantity | Source |
|----------|----------|--------|
| [Scrap Metal] | 1 | Salvage from ruins |
| [Tough Leather] | 1 | Creature drops |

---

## [Rooted] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 1-2 turns |
| **Effect** | Cannot move |
| **Removal** | Ally can use action to free |

---

## [Bleeding] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 3 turns |
| **Effect** | 1d4 Physical damage per turn |
| **Removal** | Healing, [Clean Cloth] |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Chokepoint | Place before combat begins |
| Pursuit | Drop while fleeing |
| Ambush | Set up kill zone |

---

## Combat Log Examples

- "[Einbui] places Improvised Trap at tile (5,3)"
- "[Enemy] triggers trap! [Rooted] for 2 turns!"
- "[Enemy] takes 3 Bleeding damage (Improvised Trap)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Einbui Overview](../einbui-overview.md) | Parent specialization |
| [Rooted Status](../../../../04-systems/status-effects/rooted.md) | Status effect details |
| [Bleeding Status](../../../../04-systems/status-effects/bleeding.md) | Status effect details |
| [Master Improviser](master-improviser.md) | Upgrades this ability |
