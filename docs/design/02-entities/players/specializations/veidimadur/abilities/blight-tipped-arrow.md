---
id: ABILITY-VEIDIMADUR-24005
title: "Blight-Tipped Arrow"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Blight-Tipped Arrow

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Ranged) |
| **Resource Cost** | 45 Stamina |
| **Cooldown** | 3 turns |
| **Status Effects** | [Blighted Toxin], [Glitch] |
| **Ranks** | 2 → 3 |

---

## Description

An arrow coated in distilled Blight toxin. Against corrupted creatures, it causes cascading system failures.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Damage: 4d6 Physical damage
- Apply [Blighted Toxin] (2d6/turn, 4 turns)
- 60% [Glitch] chance vs targets with 30+ Corruption

**Formula:**
```
Damage = Roll(4d6)
Target.AddStatus("BlightedToxin", DamagePerTurn: Roll(2d6), Duration: 4)
If Target.Corruption >= 30:
    If Roll(1d100) <= 60:
        Target.AddStatus("Glitch", Duration: 1)
```

**Tooltip:** "Blight-Tipped Arrow (Rank 2): 4d6 Physical + [Blighted Toxin] 2d6/turn for 4 turns. 60% [Glitch] vs 30+ Corruption. Cost: 45 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- [Blighted Toxin] damage: 3d6/turn
- [Glitch] chance: 80%
- [Glitch] duration: 1 turn

**Formula:**
```
Damage = Roll(4d6)
Target.AddStatus("BlightedToxin", DamagePerTurn: Roll(3d6), Duration: 4)
If Target.Corruption >= 30:
    If Roll(1d100) <= 80:
        Target.AddStatus("Glitch", Duration: 1)
```

**Tooltip:** "Blight-Tipped Arrow (Rank 3): 4d6 Physical + [Blighted Toxin] 3d6/turn for 4 turns. 80% [Glitch] vs 30+ Corruption. Cost: 45 Stamina"

---

## Status Effects

### [Blighted Toxin]
| Property | Value |
|----------|-------|
| **Duration** | 4 turns |
| **Icon** | Dripping poison |
| **Effects** | 2d6-3d6 damage per turn |

### [Glitch]
| Property | Value |
|----------|-------|
| **Duration** | 1 turn |
| **Icon** | Static/error |
| **Effects** | Target skips next action |

---

## Combat Log Examples

- "Blight-Tipped Arrow deals 16 damage! [Blighted Toxin] applied for 4 turns."
- "[Enemy] is Glitched! Skips next action."
- "Blight-Tipped Arrow (Rank 3): 80% Glitch chance triggered!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Veiðimaðr Overview](../veidimadur-overview.md) | Parent specialization |
