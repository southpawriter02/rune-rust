---
id: ABILITY-SCRAP-TINKER-26002
title: "Deploy Flash Bomb"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Deploy Flash Bomb

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Ground location (3x3 area) |
| **Resource Cost** | 30 Stamina + 1 Flash Bomb |
| **Cooldown** | 2 turns |
| **Status Effect** | [Blinded] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You lob the improvised device. Flash! Their optics overload, their eyes burn.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- Throw Flash Bomb to target location
- All enemies in 3x3 area make WILL save DC 13
- Failed save: [Blinded] for 2 turns
- Consumes 1 Flash Bomb from inventory

**Formula:**
```
For each Enemy in 3x3Area:
    If Enemy.WILLSave < 13:
        Enemy.AddStatus("Blinded", Duration: 2)
ConsumeItem("FlashBomb", 1)
```

**Tooltip:** "Deploy Flash Bomb (Rank 1): 3x3 AoE. DC 13 WILL or [Blinded] 2 turns. Cost: 30 Stamina + 1 Flash Bomb"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Save DC: 15
- Blinded enemies also take -2 Defense
- Resource cost reduced to 25 Stamina

**Formula:**
```
SaveDC = 15
If Enemy.HasStatus("Blinded"):
    Enemy.Defense -= 2
StaminaCost = 25
```

**Tooltip:** "Deploy Flash Bomb (Rank 2): DC 15. [Blinded] also -2 Defense. Cost: 25 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Save DC: 17
- [Blinded] duration: 3 turns
- **[Masterwork Flash Bomb]:** Also deals 2d6 damage

**Formula:**
```
SaveDC = 17
BlindDuration = 3
If Masterwork:
    Damage = Roll(2d6)
```

**Tooltip:** "Deploy Flash Bomb (Rank 3): DC 17. [Blinded] 3 turns. Masterwork: +2d6 damage."

---

## [Blinded] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 2-3 turns |
| **Effects** | -4 to all attack rolls |
| **Additional** | Cannot use abilities requiring sight |
| **Rank 2+** | -2 Defense |

---

## Combat Log Examples

- "Flash Bomb deployed! 3 enemies in blast radius."
- "[Enemy A] fails WILL save - Blinded for 2 turns!"
- "[Enemy B] saves against Flash Bomb effect."
- "Masterwork Flash Bomb: 8 damage + Blinded!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Scrap-Tinker Overview](../scrap-tinker-overview.md) | Parent specialization |
