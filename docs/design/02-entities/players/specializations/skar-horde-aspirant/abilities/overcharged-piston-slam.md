---
id: ABILITY-SKAR-HORDE-29007
title: "Overcharged Piston Slam"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Overcharged Piston Slam

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 55 Stamina + 40 Savagery |
| **Cooldown** | 4 turns |
| **Required Augment** | Blunt-type |
| **Status Effect** | [Stunned] |
| **Ranks** | 2 → 3 |

---

## Description

Superheated steam vents. Pistons compress. And then—impact. A concussive blast that reduces bone to powder.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Roll: MIGHT + 3d10 (Success Threshold: 2)
- Damage: 7d10 + MIGHT Bludgeoning
- 75% chance to apply [Stunned] for 1 turn
- **Requires Blunt-type augment**

**Formula:**
```
Requires: Augment.Type == "Blunt"
AttackRoll = Roll(MIGHT + 3d10) >= 2 successes
Damage = Roll(7d10) + MIGHT
If Random() < 0.75:
    Target.AddStatus("Stunned", Duration: 1)
```

**Tooltip:** "Overcharged Piston Slam (Rank 2): 7d10+MIGHT Bludgeoning. 75% [Stunned] 1 turn. Requires Blunt augment. Cost: 55 Stamina, 40 Savagery"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Stun chance: 100% (guaranteed)
- **NEW:** Your next attack against stunned target deals double damage

**Formula:**
```
StunChance = 1.00  // Guaranteed
OnStunApplied:
    Self.NextAttackBonus = "DoubleDamage"
```

**Tooltip:** "Overcharged Piston Slam (Rank 3): 100% [Stunned]. Next attack deals double damage. Cost: 55 Stamina, 40 Savagery"

---

## Double Damage Follow-Up

At Rank 3, after stunning:
1. Piston Slam stuns target
2. Your next attack (any ability) deals 2× damage
3. Buff consumed after one attack

**Combo Example:**
1. Overcharged Piston Slam (48 damage, target Stunned)
2. Savage Strike (normally 14) → 28 damage (doubled!)

---

## Combat Log Examples

- "OVERCHARGED PISTON SLAM! 48 Bludgeoning damage!"
- "[Enemy] is STUNNED for 1 turn! (75% triggered)"
- "Piston Slam (Rank 3): Guaranteed stun! Next attack doubled!"
- "Double Damage active! Savage Strike: 28 damage!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skar-Horde Aspirant Overview](../skar-horde-aspirant-overview.md) | Parent specialization |
| [Stunned Status](../../../../04-systems/status-effects/stunned.md) | Status effect details |
