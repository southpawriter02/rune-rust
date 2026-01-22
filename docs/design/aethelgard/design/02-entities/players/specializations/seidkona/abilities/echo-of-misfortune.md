---
id: ABILITY-SEIDKONA-27003
title: "Echo of Misfortune"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Echo of Misfortune

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | Aether |
| **Status Effect** | [Cursed] |
| **Spirit Bargain** | Spread on crit |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You channel echoes of misfortune from those who fell to the Great Silence. Their bad luck becomes your enemy's burden.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- Apply [Cursed] for 2 turns
- [Cursed]: 25% miss chance, -1d10 to all checks
- **[Spirit Bargain] On Crit:** Spread to one adjacent enemy

**Formula:**
```
Target.AddStatus("Cursed", Duration: 2, MissChance: 0.25, CheckPenalty: 1d10)

If CriticalHit:  // Spirit Bargain trigger
    AdjacentEnemy.AddStatus("Cursed", Duration: 2)
    Log("Spirit Bargain: [Cursed] spreads!")
```

**Tooltip:** "Echo of Misfortune (Rank 1): [Cursed] 2 turns (25% miss, -1d10). Spreads on crit."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- [Cursed] duration: 3 turns
- Miss chance: 30%

**Formula:**
```
Target.AddStatus("Cursed", Duration: 3, MissChance: 0.30, CheckPenalty: 1d10)
```

**Tooltip:** "Echo of Misfortune (Rank 2): [Cursed] 3 turns (30% miss, -1d10)."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- [Cursed] duration: 3 turns
- Miss chance: 30%
- Check penalty: -2d10

**Formula:**
```
Target.AddStatus("Cursed", Duration: 3, MissChance: 0.30, CheckPenalty: 2d10)
```

**Tooltip:** "Echo of Misfortune (Rank 3): [Cursed] 3 turns (30% miss, -2d10)."

---

## [Cursed] Status Effect

| Property | Rank 1 | Rank 2 | Rank 3 |
|----------|--------|--------|--------|
| Duration | 2 turns | 3 turns | 3 turns |
| Miss Chance | 25% | 30% | 30% |
| Check Penalty | -1d10 | -1d10 | -2d10 |

---

## Spirit Bargain: Spreading

During **Moment of Clarity**, the spread effect is **guaranteed** on every hit, not just crits.

---

## Combat Log Examples

- "Echo of Misfortune: [Enemy] is Cursed for 2 turns!"
- "[Enemy] attack misses! (Cursed - 25% miss chance)"
- "Spirit Bargain: [Cursed] spreads to adjacent enemy!"
- "Moment of Clarity: [Cursed] guaranteed spread!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Seiðkona Overview](../seidkona-overview.md) | Parent specialization |
| [Moment of Clarity](moment-of-clarity.md) | Guaranteed spread |
