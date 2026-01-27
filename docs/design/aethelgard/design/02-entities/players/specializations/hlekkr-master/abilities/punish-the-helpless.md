---
id: ABILITY-HLEKKR-25015
title: "Punish the Helpless"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Punish the Helpless

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Tags** | [Damage], [Execution] |
| **Ranks** | 2 → 3 |

---

## Description

Enemies who cannot move cannot defend themselves. Your chains are designed to punish the helpless—those locked in your grip take devastating damage from your attacks.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- +75% damage vs enemies with control effects:
  - [Rooted]
  - [Slowed]
  - [Stunned]
  - [Seized]
  - [Disoriented]
- Gain Advantage on attacks vs controlled enemies

**Formula:**
```
OnAttack vs Target:
    If Target.HasAnyStatus("Rooted", "Slowed", "Stunned", "Seized", "Disoriented"):
        DamageMultiplier = 1.75  // +75%
        AttackAdvantage = true
        Log("Punish the Helpless: +75% damage!")
```

**Tooltip:** "Punish the Helpless (Rank 2): +75% damage vs controlled enemies. Advantage on attacks."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- +100% damage (DOUBLE) vs controlled enemies
- Advantage on attacks vs controlled enemies
- **NEW:** Controlled enemies take 1d6 damage per turn from tightening chains

**Formula:**
```
OnAttack vs Target:
    If Target.HasAnyStatus("Rooted", "Slowed", "Stunned", "Seized", "Disoriented", "KnockedDown"):
        DamageMultiplier = 2.0  // +100% (double)
        AttackAdvantage = true
        Log("Punish the Helpless: DOUBLE damage!")

OnControlledEnemyTurnStart:
    If Hlekkr.IsInCombat:
        For each ControlledEnemy:
            ChainDamage = Roll(1d6)
            ControlledEnemy.TakeDamage(ChainDamage, "Physical")
            Log("Chains tighten: {ControlledEnemy} takes {ChainDamage}")
```

**Tooltip:** "Punish the Helpless (Rank 3): DOUBLE damage vs controlled. Controlled take 1d6/turn from chains."

---

## Qualifying Status Effects

| Status | Triggers Bonus? |
|--------|-----------------|
| [Rooted] | Yes |
| [Slowed] | Yes |
| [Stunned] | Yes |
| [Seized] | Yes |
| [Disoriented] | Yes |
| [Knocked Down] | Yes (Rank 3 only) |
| [Feared] | No |
| [Bleeding] | No |

---

## Damage Calculation Example

**Base Attack:** 20 damage

| Rank | Multiplier | Final Damage |
|------|------------|--------------|
| None | 1.0× | 20 |
| Rank 2 | 1.75× | 35 |
| Rank 3 | 2.0× | 40 |

**With DoT (Rank 3):** +3.5 avg damage per turn per controlled enemy

---

## Combo Potential

**Maximum Single-Target Setup:**
1. Netting Shot → [Rooted]
2. Grappling Hook → [Disoriented]
3. Attack with Punish the Helpless → Double damage + Advantage

**AoE Control:**
1. Chain Scythe → [Slowed] on row
2. All attacks on row → Double damage each

---

## Combat Log Examples

- "Punish the Helpless: +75% damage vs [Rooted] target!"
- "Punish the Helpless (Rank 3): DOUBLE damage (18 → 36)!"
- "Chains tighten: [Rooted Enemy] takes 4 damage"
- "Attack with Advantage vs [Disoriented] target"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Hlekkr-Master Overview](../hlekkr-master-overview.md) | Parent specialization |
| [Netting Shot](netting-shot.md) | [Rooted] source |
| [Unyielding Grip](unyielding-grip.md) | [Seized] source |
| [Chain Scythe](chain-scythe.md) | AoE [Slowed] source |
