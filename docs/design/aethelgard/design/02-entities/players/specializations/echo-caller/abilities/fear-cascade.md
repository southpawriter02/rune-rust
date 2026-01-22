---
id: ABILITY-ECHO-CALLER-28016
title: "Fear Cascade"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Fear Cascade

**Type:** Active [Echo] | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All Enemies in 3-tile radius |
| **Resource Cost** | 45 Aether |
| **Status Effect** | [Feared] |
| **Tags** | [Echo] |
| **Ranks** | None (full power when unlocked) |

---

## Description

You release a wave of concentrated terror, a psychic shockwave that ripples through enemy minds. Those already afraid are utterly broken.

---

## Mechanical Effect

**AoE Fear Application:**
- Target point within range
- All enemies within 3 tiles of target point make WILL Resolve check (DC 16)
- Failed save: Become [Feared] for 3 turns
- Already-Feared enemies automatically fail AND take 4d6 Psychic damage
- **[Echo Chain]:** Auto-spreads Fear to one enemy outside initial radius

**Formula:**
```
For each Enemy in 3-tile radius of TargetPoint:
    If Enemy.HasStatus("Feared"):
        // Auto-fail, take damage
        Enemy.TakeDamage(Roll(4d6), "Psychic")
        Enemy.ExtendStatus("Feared", Duration: 3)
    Else:
        If Enemy.WILLResolve < 16:
            Enemy.AddStatus("Feared", Duration: 3)

// Echo Chain
SelectNearestEnemyOutsideRadius()
EchoTarget.AddStatus("Feared", Duration: 2)
```

**Tooltip:** "Fear Cascade: DC 16 WILL or [Feared] 3 turns. Already-Feared take 4d6 Psychic. Echo Chain spreads Fear outside radius. Cost: 45 Aether"

---

## Tactical Setup

**Optimal Combo:**
1. Phantom Menace on central target (Fear 1 enemy)
2. Wait for enemies to cluster around Feared target
3. Fear Cascade centered on Feared target
4. Feared target takes 4d6 damage + all others must save

---

## Combat Log Examples

- "Fear Cascade centered on [location]! 4 enemies in radius."
- "[Enemy A] already Feared - takes 18 Psychic damage!"
- "[Enemy B] fails WILL save - [Feared] for 3 turns!"
- "[Enemy C] resists Fear Cascade"
- "Echo Chain: [Enemy D] outside radius becomes [Feared] for 2 turns!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Echo-Caller Overview](../echo-caller-overview.md) | Parent specialization |
| [Terror Feedback](terror-feedback.md) | Aether recovery |
