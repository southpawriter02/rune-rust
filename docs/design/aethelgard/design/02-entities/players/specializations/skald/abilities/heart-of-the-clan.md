---
id: ABILITY-SKALD-28008
title: "Heart of the Clan"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Heart of the Clan

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (aura) |
| **Target** | Allies in Same Row + Adjacent Row |
| **Resource Cost** | None |
| **Ranks** | None (full power when unlocked) |

---

## Description

The Skald is the living heart of their clan. Their presence alone is a source of unshakeable resolve, steadying allies against mental assault.

---

## Mechanical Effect

**Passive Aura:**
- All allies in same row as Skald gain +2d10 to defensive Resolve Checks
- Applies to: Fear, Disoriented resistance
- Aura extends to adjacent row
- **Inactive** if Skald is [Stunned], [Feared], or [Silenced]

**Formula:**
```
If NOT Skald.HasStatus("Stunned", "Feared", "Silenced"):
    For each Ally in SameRow OR AdjacentRow:
        Ally.ResolveBonus_Fear += 2d10
        Ally.ResolveBonus_Disoriented += 2d10
```

**Tooltip:** "Heart of the Clan: Allies in your row and adjacent row +2d10 to Resolve vs Fear/Disoriented. Inactive if Stunned/Feared/Silenced."

---

## Aura Range

```
[Back Row]  ← Adjacent (receives bonus)
[Front Row] ← Skald's Row (receives bonus)
```

Or if Skald is in back row:
```
[Back Row]  ← Skald's Row (receives bonus)
[Front Row] ← Adjacent (receives bonus)
```

---

## Deactivation Conditions

The aura is temporarily disabled when:
- Skald becomes [Stunned]
- Skald becomes [Feared]
- Skald becomes [Silenced]

Aura reactivates when these conditions end.

---

## Combat Log Examples

- "Heart of the Clan: [Ally] gains +2d10 to Resolve vs Fear"
- "[Skald] is Stunned! Heart of the Clan deactivated."
- "[Skald] recovered. Heart of the Clan reactivated."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skald Overview](../skald-overview.md) | Parent specialization |
