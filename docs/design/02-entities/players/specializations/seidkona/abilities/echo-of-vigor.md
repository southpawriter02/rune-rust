---
id: ABILITY-SEIDKONA-27002
title: "Echo of Vigor"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Echo of Vigor

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Ally |
| **Resource Cost** | Aether |
| **Spirit Bargain** | Cleanse debuff |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You channel echoes of vitality from those who once lived, restoring health to an ally. Sometimes, the spirits offer more.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- Restore 3d8 HP to target ally
- **[Spirit Bargain] 25%:** Cleanse one minor physical debuff ([Bleeding], [Poisoned], [Disease])

**Formula:**
```
Healing = Roll(3d8)
Target.HP += Healing

If Random() < 0.25:  // Spirit Bargain
    Target.RemoveStatus(OneOf: ["Bleeding", "Poisoned", "Disease"])
    Log("Spirit Bargain TRIGGERED!")
```

**Tooltip:** "Echo of Vigor (Rank 1): Heal 3d8 HP. 25% chance to cleanse physical debuff."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Restore 4d8 HP
- Spirit Bargain chance: 30%

**Formula:**
```
Healing = Roll(4d8)
SpiritBargainChance = 0.30
```

**Tooltip:** "Echo of Vigor (Rank 2): Heal 4d8 HP. 30% cleanse chance."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Restore 5d8 HP
- Spirit Bargain chance: 35%
- **NEW:** Can target self

**Formula:**
```
Healing = Roll(5d8)
SpiritBargainChance = 0.35
CanTargetSelf = true
```

**Tooltip:** "Echo of Vigor (Rank 3): Heal 5d8 HP. 35% cleanse. Can target self."

---

## Spirit Bargain Interaction

| Condition | Cleanse Chance |
|-----------|---------------|
| Base (Rank 1) | 25% |
| Rank 2 | 30% |
| Rank 3 | 35% |
| With Fickle Fortune | +15-25% |
| During Moment of Clarity | **100%** |

---

## Combat Log Examples

- "Echo of Vigor: Healed [Ally] for 18 HP"
- "Spirit Bargain TRIGGERED: Cleansed [Bleeding] from [Ally]!"
- "Echo of Vigor (Rank 3): Self-heal for 28 HP"
- "Moment of Clarity: Spirit Bargain guaranteed - cleansing [Poisoned]"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Seiðkona Overview](../seidkona-overview.md) | Parent specialization |
| [Fickle Fortune](fickle-fortune.md) | Bargain modifier |
| [Moment of Clarity](moment-of-clarity.md) | Guaranteed bargains |
