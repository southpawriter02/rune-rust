---
id: ABILITY-SKAR-HORDE-29003
title: "Horrific Form"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Horrific Form

**Type:** Passive | **Tier:** 1 | **PP Cost:** 0 (free with specialization)

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (triggered) |
| **Target** | Self |
| **Trigger** | When hit by melee attack |
| **Status Effect** | [Feared] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Your self-mutilation is deeply unsettling. Good. Let them see what you have become.

---

## Rank Progression

### Rank 1 (Starting Rank - When specialization is chosen)

**Effect:**
- When hit by melee attack: 25% chance to apply [Feared] to attacker for 1 turn

**Formula:**
```
OnMeleeHit:
    If Random() < 0.25:
        Attacker.AddStatus("Feared", Duration: 1)
```

**Tooltip:** "Horrific Form (Rank 1): 25% chance to Fear melee attackers for 1 turn."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Fear chance increased to 35%
- **NEW:** Feared enemies deal -2 damage to you

**Formula:**
```
FearChance = 0.35
If Attacker.HasStatus("Feared"):
    Attacker.DamageToYou -= 2
```

**Tooltip:** "Horrific Form (Rank 2): 35% Fear chance. Feared enemies deal -2 damage to you."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Fear chance increased to 50%
- **NEW:** Gain +5 Savagery when enemy becomes Feared

**Formula:**
```
FearChance = 0.50
OnEnemyFeared:
    Savagery += 5
```

**Tooltip:** "Horrific Form (Rank 3): 50% Fear chance. +5 Savagery when enemy Feared."

---

## Fear Chance by Rank

| Rank | Chance | Bonus Effect |
|------|--------|--------------|
| 1 | 25% | — |
| 2 | 35% | -2 damage from Feared |
| 3 | 50% | +5 Savagery per Fear |

---

## Combat Log Examples

- "[Enemy] strikes you..."
- "Horrific Form: [Enemy] recoils in horror! [Feared] for 1 turn"
- "[Enemy] is Feared - deals -2 damage to you (Horrific Form)"
- "Horrific Form (Rank 3): +5 Savagery from causing Fear"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skar-Horde Aspirant Overview](../skar-horde-aspirant-overview.md) | Parent specialization |
| [Feared Status](../../../../04-systems/status-effects/feared.md) | Status effect details |
