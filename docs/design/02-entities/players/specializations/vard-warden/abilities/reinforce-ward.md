---
id: ABILITY-VARD-WARDEN-28014
title: "Reinforce Ward"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Reinforce Ward

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | One Runic Barrier OR Sanctified Ground zone |
| **Resource Cost** | 15-20 Aether |
| **Cooldown** | 2 turns |
| **Attribute** | WILL |
| **Tags** | [Construct], [Zone], [Maintenance] |
| **Ranks** | 2 → 3 |

---

## Description

Your constructs are not static—they respond to your will. With a gesture, you channel fresh Aether into existing wards, repairing damaged barriers or extending the reach of your sanctified ground.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- **If targeting Runic Barrier:** Restore 15 HP to barrier
- **If targeting Sanctified Ground:** Extend duration by 1 turn
- Cost: 15 Aether

**Formula:**
```
Caster.Aether -= 15

If Target.Type == "RunicBarrier":
    HealAmount = 15
    Target.HP = Min(Target.HP + HealAmount, Target.MaxHP)
    Log("Reinforce Ward: Barrier restored {HealAmount} HP ({Target.HP}/{Target.MaxHP})")

Else If Target.Type == "SanctifiedGround":
    Target.Duration += 1
    Log("Reinforce Ward: Zone extended +1 turn ({Target.Duration} remaining)")
```

**Tooltip:** "Reinforce Ward (Rank 2): Heal barrier 15 HP OR extend zone +1 turn. Cost: 15 Aether"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- **If targeting Runic Barrier:** Restore 25 HP to barrier
- **If targeting Sanctified Ground:** Extend duration by 2 turns AND increase healing/damage by +2
- Cost: 20 Aether

**Formula:**
```
Caster.Aether -= 20

If Target.Type == "RunicBarrier":
    HealAmount = 25
    Target.HP = Min(Target.HP + HealAmount, Target.MaxHP)
    Log("Reinforce Ward: Barrier restored {HealAmount} HP ({Target.HP}/{Target.MaxHP})")

Else If Target.Type == "SanctifiedGround":
    Target.Duration += 2
    Target.HealBonus += 2
    Target.DamageBonus += 2
    Log("Reinforce Ward: Zone extended +2 turns, +2 healing/damage")
```

**Tooltip:** "Reinforce Ward (Rank 3): Heal barrier 25 HP OR extend zone +2 turns (+2 heal/damage). Cost: 20 Aether"

---

## Effect Summary

### Barrier Healing

| Property | R2 | R3 |
|----------|----|----|
| HP Restored | 15 | 25 |
| Max HP Cap | Yes | Yes |

### Zone Extension

| Property | R2 | R3 |
|----------|----|----|
| Duration Added | +1 turn | +2 turns |
| Healing Bonus | — | +2 |
| Damage Bonus | — | +2 |

---

## Aether Efficiency

| Target | R2 Cost | R3 Cost | Value |
|--------|---------|---------|-------|
| Barrier (15-25 HP) | 15 | 20 | Better than recasting |
| Zone (+1-2 turns) | 15 | 20 | Extends expensive effect |

**Comparison:** Creating a new Runic Barrier costs 25 Aether for 50 HP. Reinforce Ward costs 20 Aether for 25 HP—but preserves the barrier's current position and remaining duration.

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Barrier under attack | Heal damaged walls |
| Extended defense | Keep zone active longer |
| Choke point | Maintain barrier indefinitely |
| Resource efficiency | Cheaper than recasting |

---

## Optimal Timing

**Reinforce Barrier When:**
- Barrier below 50% HP
- Under sustained enemy attack
- Position is critical

**Extend Zone When:**
- Combat will last longer than zone duration
- Allies are benefiting from healing
- Blighted enemies still present

---

## Combat Log Examples

- "Reinforce Ward: Barrier restored 25 HP (50/50 HP)"
- "Reinforce Ward: Zone extended +2 turns (5 remaining)"
- "Reinforce Ward: Zone gains +2 healing/damage"
- "Cannot reinforce: No valid target in range"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Vard-Warden Overview](../vard-warden-overview.md) | Parent specialization |
| [Runic Barrier](runic-barrier.md) | Primary barrier ability |
| [Consecrate Ground](consecrate-ground.md) | Primary zone ability |
