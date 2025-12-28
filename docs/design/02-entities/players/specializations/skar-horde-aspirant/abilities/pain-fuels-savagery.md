---
id: ABILITY-SKAR-HORDE-29006
title: "Pain Fuels Savagery"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Pain Fuels Savagery

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action |
| **Target** | Self |
| **Trigger** | When taking damage |
| **Resource Cost** | None |
| **Ranks** | 2 → 3 |

---

## Description

Every wound is fuel. Every blow against you is a gift. Pain is just another resource.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Generate Savagery equal to 15% of damage taken
- Maximum 25 Savagery per hit

**Formula:**
```
OnDamageTaken:
    SavageryGained = Min(DamageTaken * 0.15, 25)
```

**Tooltip:** "Pain Fuels Savagery (Rank 2): Generate 15% of damage taken as Savagery (max 25/hit)"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Conversion rate: 20%
- Maximum: 30 Savagery per hit
- **NEW:** Gain +1 Soak per 25 Savagery held

**Formula:**
```
OnDamageTaken:
    SavageryGained = Min(DamageTaken * 0.20, 30)
BonusSoak = Floor(Savagery / 25)
```

**Tooltip:** "Pain Fuels Savagery (Rank 3): 20% conversion (max 30/hit). +1 Soak per 25 Savagery."

---

## Conversion Examples

| Damage Taken | Rank 2 (15%) | Rank 3 (20%) |
|--------------|--------------|--------------|
| 10 | 1.5 → 1 | 2 |
| 20 | 3 | 4 |
| 50 | 7.5 → 7 | 10 |
| 100 | 15 | 20 |
| 200+ | 25 (cap) | 30 (cap) |

---

## Soak Bonus (Rank 3)

| Savagery | Bonus Soak |
|----------|------------|
| 0-24 | 0 |
| 25-49 | +1 |
| 50-74 | +2 |
| 75-99 | +3 |
| 100 | +4 |

---

## Combat Log Examples

- "Pain Fuels Savagery: Took 30 damage, +4 Savagery"
- "Pain Fuels Savagery: Took 80 damage, +12 Savagery (capped at 25)"
- "Pain Fuels Savagery (Rank 3): +3 Soak from 75 Savagery"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skar-Horde Aspirant Overview](../skar-horde-aspirant-overview.md) | Parent specialization |
| [The Price of Power](the-price-of-power.md) | Savagery multiplier |
