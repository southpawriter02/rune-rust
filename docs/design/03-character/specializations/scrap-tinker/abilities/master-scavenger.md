---
id: ABILITY-SCRAP-TINKER-26001
title: "Master Scavenger"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Master Scavenger

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You see value where others see junk. Every bolt, every wire, every corroded gear—repurposable.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- +1d10 bonus to scavenging Scrap Materials checks
- Find 50% more Scrap from defeated mechanical enemies
- Find 50% more Scrap from loot containers

**Formula:**
```
ScavengingCheckPool += 1d10
ScrapFromMechanical *= 1.50
ScrapFromContainers *= 1.50
```

**Tooltip:** "Master Scavenger (Rank 1): +1d10 scavenging. +50% Scrap from Mechanical enemies and containers."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- +2d10 scavenging bonus
- Find 75% more Scrap
- **NEW:** Can salvage Scrap from broken weapons/armor (dismantle for materials)

**Formula:**
```
ScavengingCheckPool += 2d10
ScrapBonus = 1.75
CanDismantle = true
```

**Tooltip:** "Master Scavenger (Rank 2): +2d10 scavenging. +75% Scrap. Can dismantle broken equipment."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +3d10 scavenging bonus
- Find 100% more Scrap (double)
- Salvaged materials include rare components
- **NEW:** Start expeditions with 20 Scrap

**Formula:**
```
ScavengingCheckPool += 3d10
ScrapBonus = 2.00
SalvageIncludesRare = true
ExpeditionStartScrap = 20
```

**Tooltip:** "Master Scavenger (Rank 3): +3d10 scavenging. Double Scrap. Rare components. Start with 20 Scrap."

---

## Combat Log Examples

- "Master Scavenger: +15 Scrap from defeated Automaton (+50% bonus)"
- "Master Scavenger: Dismantled broken shield for 8 Scrap"
- "Master Scavenger: Expedition start bonus - 20 Scrap"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Scrap-Tinker Overview](../scrap-tinker-overview.md) | Parent specialization |
