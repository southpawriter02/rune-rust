---
id: ABILITY-SCRAP-TINKER-26007
title: "Automated Scavenging"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Automated Scavenging

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action |
| **Target** | Self |
| **Trigger** | After combat ends |
| **Resource Cost** | None |
| **Ranks** | 2 → 3 |

---

## Description

You've built automated collection systems. Magnets, sensors, retrieval claws—never leave materials behind.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- After combat: Auto-scavenge 10 Scrap Materials (no action required)
- **NEW:** Scout Drone can scavenge while deployed (+5 Scrap per combat)

**Formula:**
```
OnCombatEnd:
    ScrapMaterials += 10
If ScoutDrone.Active:
    ScrapMaterials += 5
```

**Tooltip:** "Automated Scavenging (Rank 2): +10 Scrap after combat. Scout Drone: +5 Scrap."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Auto-scavenge 15 Scrap
- 25% chance to find rare components
- **NEW:** Scrap Golem (if active) scavenges additional 10 Scrap

**Formula:**
```
OnCombatEnd:
    ScrapMaterials += 15
    If Random() < 0.25:
        AddRareComponent()
If ScrapGolem.Active:
    ScrapMaterials += 10
```

**Tooltip:** "Automated Scavenging (Rank 3): +15 Scrap. 25% rare components. Golem: +10 Scrap."

---

## Scrap Generation Summary

| Source | Rank 2 | Rank 3 |
|--------|--------|--------|
| Base (auto) | 10 | 15 |
| Scout Drone | +5 | +5 |
| Scrap Golem | — | +10 |
| Rare Component | — | 25% chance |
| **Max per combat** | 15 | 30+ |

---

## Combat Log Examples

- "Combat ended. Automated Scavenging: +10 Scrap"
- "Scout Drone scavenging: +5 Scrap"
- "Scrap Golem scavenging: +10 Scrap"
- "Automated Scavenging: Rare component found!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Scrap-Tinker Overview](../scrap-tinker-overview.md) | Parent specialization |
| [Deploy Scout Drone](deploy-scout-drone.md) | Scavenger pet |
| [Deploy Scrap Golem](deploy-scrap-golem.md) | Scavenger pet |
