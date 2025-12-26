---
id: ABILITY-HLEKKR-25016
title: "Chain Scythe"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Chain Scythe

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All enemies in one row (Front or Back) |
| **Resource Cost** | 35 Stamina |
| **Cooldown** | 4 turns |
| **Damage Type** | Physical |
| **Status Effect** | [Slowed], [Knocked Down], [Disoriented] |
| **Tags** | [AoE], [Control] |
| **Ranks** | None (full power when unlocked) |

---

## Description

You sweep your weighted chain in a devastating arc, striking all enemies in a row. The blow leaves them slowed and disoriented—and corrupted enemies may find themselves knocked to the ground entirely.

---

## Mechanical Effect

**Row-Wide AoE:**
- Deal 3d8 Physical damage to ALL enemies in target row
- Choose to target Front Row OR Back Row
- Apply [Slowed] for 3 turns to all hit
- Apply [Disoriented] for 1 turn to all hit
- **Corruption Bonus:** Against 60+ Corruption: 80% chance to apply [Knocked Down] instead of [Slowed]

**Formula:**
```
SelectRow(FrontRow OR BackRow)

For each Enemy in SelectedRow:
    Damage = Roll(3d8)
    Enemy.TakeDamage(Damage, "Physical")
    Enemy.AddStatus("Disoriented", Duration: 1)

    If Enemy.Corruption >= 60:
        If Roll(1d100) <= 80:
            Enemy.AddStatus("KnockedDown", Duration: 2)
            Log("{Enemy} knocked down by Chain Scythe!")
        Else:
            Enemy.AddStatus("Slowed", Duration: 3)
    Else:
        Enemy.AddStatus("Slowed", Duration: 3)
```

**Tooltip:** "Chain Scythe: 3d8 Physical to row. [Slowed] 3 turns + [Disoriented]. High corruption: 80% [Knocked Down]. Cost: 35 Stamina"

---

## [Slowed] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 3 turns (+2 with PP Rank 3 = 5) |
| **Effect** | Movement costs doubled |
| **Agility** | -1 die to Agility checks |

---

## [Knocked Down] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 2 turns |
| **Attack Penalty** | -2 dice |
| **Defense Penalty** | +2 dice to attacks against |
| **Recovery** | Stand up costs Standard Action |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Front row clear | Disable melee threats |
| Back row snipe | Control ranged/casters |
| Formation break | [Slowed] prevents repositioning |
| High corruption | [Knocked Down] for massive setup |

---

## Punish the Helpless Synergy

All enemies hit by Chain Scythe become valid targets for Punish the Helpless:
- [Slowed] triggers +100% damage
- [Knocked Down] triggers +100% damage
- [Disoriented] triggers +100% damage

**Result:** Entire row becomes vulnerable to doubled damage attacks.

---

## Combat Log Examples

- "Chain Scythe: Targeting Back Row! 4 enemies in range."
- "[Enemy A] takes 18 damage, [Slowed] 3 turns, [Disoriented]"
- "High Corruption: [Enemy B] is [Knocked Down] instead of Slowed!"
- "Pragmatic Preparation: [Slowed] extended to 5 turns"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Hlekkr-Master Overview](../hlekkr-master-overview.md) | Parent specialization |
| [Punish the Helpless](punish-the-helpless.md) | Damage multiplier |
| [Slowed Status](../../../../04-systems/status-effects/slowed.md) | Status effect |
| [Pragmatic Preparation I](pragmatic-preparation-i.md) | Duration bonus |
