---
id: ABILITY-VARD-WARDEN-28011
title: "Runic Barrier"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Runic Barrier

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Adjacent Tile |
| **Resource Cost** | 20-25 Aether |
| **Cooldown** | None |
| **Attribute** | WILL |
| **Tags** | [Construct], [Defensive], [Battlefield Control] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You inscribe runes of stability into the air itself, solidifying Aether into a physical barrier. These walls block movement, projectiles, and line-of-sight—creating defensible positions where none existed.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Create a barrier on an adjacent tile
- Barrier HP: 30
- Duration: 2 turns
- Blocks: Movement, line-of-sight, projectiles
- Cost: 20 Aether

**Formula:**
```
Caster.Aether -= 20

Barrier = CreateEntity("RunicBarrier")
Barrier.HP = 30
Barrier.MaxHP = 30
Barrier.Duration = 2
Barrier.Position = TargetTile
Barrier.BlocksMovement = True
Barrier.BlocksLoS = True
Barrier.BlocksProjectiles = True

Log("Runic Barrier created (30 HP, 2 turns)")
```

**Tooltip:** "Runic Barrier (Rank 1): Create wall (30 HP, 2 turns). Cost: 20 Aether"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Barrier HP: 40
- Duration: 3 turns
- Cost: 22 Aether

**Formula:**
```
Caster.Aether -= 22

Barrier = CreateEntity("RunicBarrier")
Barrier.HP = 40
Barrier.MaxHP = 40
Barrier.Duration = 3
Barrier.Position = TargetTile
Barrier.BlocksMovement = True
Barrier.BlocksLoS = True
Barrier.BlocksProjectiles = True

Log("Runic Barrier created (40 HP, 3 turns)")
```

**Tooltip:** "Runic Barrier (Rank 2): Create wall (40 HP, 3 turns). Cost: 22 Aether"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Barrier HP: 50
- Duration: 4 turns
- **NEW:** When destroyed, deals 2d6 Arcane damage in AoE
- Cost: 25 Aether

**Formula:**
```
Caster.Aether -= 25

Barrier = CreateEntity("RunicBarrier")
Barrier.HP = 50
Barrier.MaxHP = 50
Barrier.Duration = 4
Barrier.Position = TargetTile
Barrier.BlocksMovement = True
Barrier.BlocksLoS = True
Barrier.BlocksProjectiles = True
Barrier.OnDestroy = DetonateBarrier

Function DetonateBarrier():
    Damage = Roll(2d6)
    For Each Enemy in AdjacentTiles(Barrier.Position):
        Enemy.TakeDamage(Damage, "Arcane")
    Log("Barrier detonates! 2d6 Arcane AoE!")

Log("Runic Barrier created (50 HP, 4 turns, explodes on death)")
```

**Tooltip:** "Runic Barrier (Rank 3): Create wall (50 HP, 4 turns). Explodes for 2d6 Arcane AoE when destroyed."

---

## Barrier Properties

| Property | R1 | R2 | R3 |
|----------|----|----|------|
| HP | 30 | 40 | 50 |
| Duration | 2 turns | 3 turns | 4 turns |
| Blocks Movement | ✓ | ✓ | ✓ |
| Blocks LoS | ✓ | ✓ | ✓ |
| Blocks Projectiles | ✓ | ✓ | ✓ |
| Detonation | — | — | 2d6 Arcane |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Chokepoint | Block corridor, force single-file |
| Ranged protection | Block enemy archer lines |
| Ally cover | Create firing positions |
| Retreat support | Block pursuit paths |
| Trap (R3) | Bait enemies to destroy barrier |

---

## Barrier Interaction Rules

| Interaction | Result |
|-------------|--------|
| Ally movement | Blocked (must destroy or wait) |
| Enemy movement | Blocked |
| Melee attacks | Can attack barrier |
| Ranged attacks | Blocked by barrier |
| AoE effects | Barrier takes damage, blocks spread |
| Duration expires | Barrier vanishes (no detonation) |

---

## Combat Log Examples

- "Runic Barrier created at [position] (50 HP, 4 turns)"
- "Runic Barrier takes 15 damage (35/50 HP)"
- "[Enemy] cannot move through Runic Barrier"
- "Runic Barrier destroyed! 2d6 Arcane detonation!"
- "Runic Barrier expires (duration ended)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Vard-Warden Overview](../vard-warden-overview.md) | Parent specialization |
| [Reinforce Ward](reinforce-ward.md) | Heal barriers |
| [Aegis of Sanctity](aegis-of-sanctity.md) | Barrier reflection |
