---
id: ABILITY-SKAR-HORDE-29002
title: "Savage Strike"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Savage Strike

**Type:** Active | **Tier:** 1 | **PP Cost:** 0 (free with specialization)

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 40 Stamina (35 at Rank 2+) |
| **Attribute** | MIGHT |
| **Damage Type** | Varies (based on augment) |
| **Ranks** | 1 → 2 → 3 |

---

## Description

A brutal, straightforward blow with your augmented stump. Savage. Effective. Yours.

---

## Rank Progression

### Rank 1 (Starting Rank - When specialization is chosen)

**Effect:**
- Roll: MIGHT + 2d10 vs Target Defense (Success Threshold: 2)
- Damage: 2d[Augment] + MIGHT
- Generates +15 Savagery on hit

**Formula:**
```
AttackRoll = Roll(MIGHT + 2d10) >= 2 successes
Damage = Roll(2 * AugmentDie) + MIGHT
SavageryGained = 15 (on hit)
```

**Tooltip:** "Savage Strike (Rank 1): 2d[Augment]+MIGHT damage. +15 Savagery. Cost: 40 Stamina"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Generates +20 Savagery on hit
- Resource cost reduced to 35 Stamina

**Formula:**
```
SavageryGained = 20
StaminaCost = 35
```

**Tooltip:** "Savage Strike (Rank 2): +20 Savagery. Cost: 35 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Generates +25 Savagery on hit
- **NEW:** Apply [Bleeding] on critical hit (3+ extra successes)

**Formula:**
```
SavageryGained = 25
If Successes >= 5:  // 3+ over threshold
    Target.AddStatus("Bleeding", Duration: 2)
```

**Tooltip:** "Savage Strike (Rank 3): +25 Savagery. [Bleeding] on crit. Cost: 35 Stamina"

---

## Savagery Generation by Rank

| Rank | Savagery on Hit | With The Price of Power |
|------|-----------------|------------------------|
| 1 | 15 | 26 (+75%) |
| 2 | 20 | 35 (+75%) |
| 3 | 25 | 50 (+100%, Rank 3) |

---

## Combat Log Examples

- "Savage Strike: 14 damage (2d8[6,4] + MIGHT[4]). +15 Savagery"
- "Savage Strike (Rank 2): 18 damage. +20 Savagery"
- "CRITICAL! Savage Strike: 22 damage. +25 Savagery. [Bleeding] applied!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skar-Horde Aspirant Overview](../skar-horde-aspirant-overview.md) | Parent specialization |
| [The Price of Power](the-price-of-power.md) | Savagery multiplier |
