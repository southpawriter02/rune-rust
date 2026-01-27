---
id: ABILITY-SKJALDMAER-26024
title: "Interposing Shield"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Interposing Shield

**Type:** Reaction | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Reaction |
| **Target** | Adjacent Ally |
| **Resource Cost** | 25 Stamina |
| **Trigger** | Adjacent ally is hit by a Critical Hit |
| **Limit** | Once per round |
| **Ranks** | 2 → 3 |

---

## Description

React to incoming Critical Hit on adjacent ally, redirecting to self. The Skjaldmær interposes her shield at the last moment, taking the brunt of the blow.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- **Trigger:** When an adjacent ally would take damage from a Critical Hit
- **Effect:** Redirect the attack to the Skjaldmær
- **Damage Reduction:** Skjaldmær takes 50% of the original damage
- **Original Target:** Takes 0 damage

**Formula:**
```
Trigger: Ally.IsAdjacent(Skjaldmaer) AND IncomingAttack.IsCritical
If Triggered:
    OriginalDamage = IncomingAttack.Damage
    Skjaldmaer.HP -= Floor(OriginalDamage * 0.50)
    Ally.HP -= 0
    Skjaldmaer.Stamina -= 25
```

**Tooltip:** "Interposing Shield (Rank 2): Intercept critical hit, take 50% damage. Cost: 25 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- **Trigger:** When an adjacent ally would take damage from a Critical Hit
- **Effect:** Redirect the attack to the Skjaldmær
- **Damage Reduction:** Skjaldmær takes only 35% of the original damage (improved)
- **Original Target:** Takes 0 damage
- **NEW:** Reflect 15% of original damage back to attacker

**Formula:**
```
Trigger: Ally.IsAdjacent(Skjaldmaer) AND IncomingAttack.IsCritical
If Triggered:
    OriginalDamage = IncomingAttack.Damage
    Skjaldmaer.HP -= Floor(OriginalDamage * 0.35)
    Ally.HP -= 0
    Attacker.HP -= Floor(OriginalDamage * 0.15)  // Reflect
    Skjaldmaer.Stamina -= 25
```

**Tooltip:** "Interposing Shield (Rank 3): Intercept critical hit, take only 35% damage, reflect 15% to attacker. Cost: 25 Stamina"

---

## Reaction Prompt

```
┌─────────────────────────────────────────────┐
│     INTERPOSING SHIELD                      │
├─────────────────────────────────────────────┤
│ [Ally Name] is about to take a CRITICAL HIT │
│ for [X] damage!                             │
│                                             │
│ Intercept?                                  │
│ • You will take: [X × 50%] = [Y] damage     │
│ • Cost: 25 Stamina                          │
│                                             │
│    [INTERCEPT]        [DECLINE]             │
└─────────────────────────────────────────────┘
```

---

## Combat Log Examples

- "REACTION: Interposing Shield! [Skjaldmær] intercepts critical hit meant for [Ally]!"
- "[Skjaldmær] takes 15 damage (50% of 30)"
- "[Ally] is protected from critical hit!"
- "Interposing Shield (Rank 3): [Enemy] takes 5 reflected damage!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skjaldmær Overview](../skjaldmaer-overview.md) | Parent specialization |
