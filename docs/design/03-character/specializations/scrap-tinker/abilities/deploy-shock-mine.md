---
id: ABILITY-SCRAP-TINKER-26005
title: "Deploy Shock Mine"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Deploy Shock Mine

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Ground location |
| **Resource Cost** | 35 Stamina |
| **Cooldown** | None |
| **Damage Type** | Lightning |
| **Status Effects** | [Stunned], [Slowed] |
| **Ranks** | 2 → 3 |

---

## Description

You carefully arm the mine. Step on it—instant overload. Nervous system fried.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Place Shock Mine at location (hidden to enemies)
- Trigger: Enemy moves onto mine
- Damage: 4d8 Lightning
- STURDINESS save DC 16 or [Stunned] 2 turns
- Can place 2 mines per combat

**Formula:**
```
PlaceTrap("ShockMine", Location)
Mine.Hidden = true

OnEnemyTrigger:
    Damage = Roll(4d8, "Lightning")
    If Enemy.STURDINESSSave < 16:
        Enemy.AddStatus("Stunned", Duration: 2)
MaxMinesPerCombat = 2
```

**Tooltip:** "Deploy Shock Mine (Rank 2): 4d8 Lightning. DC 16 or [Stunned] 2 turns. Max 2/combat. Cost: 35 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Damage: 5d8 Lightning
- Save DC: 18
- **[Masterwork Mine]:** Also applies [Slowed] for 2 turns after Stun ends

**Formula:**
```
Damage = Roll(5d8, "Lightning")
SaveDC = 18
If Masterwork:
    OnStunEnd: Enemy.AddStatus("Slowed", Duration: 2)
```

**Tooltip:** "Deploy Shock Mine (Rank 3): 5d8 Lightning. DC 18. Masterwork: [Slowed] 2 turns after Stun."

---

## Trap Mechanics

| Property | Value |
|----------|-------|
| **Visibility** | Hidden until triggered |
| **Trigger** | Enemy movement onto tile |
| **Friendly Fire** | No (allies can walk over safely) |
| **Detection** | Perception check DC 15 |
| **Disarm** | Cannot be disarmed by enemies |

---

## Combat Log Examples

- "Shock Mine placed at [location]. Hidden from enemies."
- "[Enemy] triggers Shock Mine! 28 Lightning damage!"
- "[Enemy] fails STURDINESS save - Stunned for 2 turns!"
- "Masterwork Mine: [Slowed] will apply when Stun ends."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Scrap-Tinker Overview](../scrap-tinker-overview.md) | Parent specialization |
| [Stunned Status](../../../../04-systems/status-effects/stunned.md) | Status effect details |
