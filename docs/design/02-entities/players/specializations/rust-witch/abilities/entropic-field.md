---
id: ABILITY-RUST-WITCH-25003
title: "Entropic Field"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Entropic Field

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (aura) |
| **Target** | All enemies in your row |
| **Resource Cost** | None |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Your presence accelerates decay. Metal weakens, seals fail, and armor crumbles simply by standing near you.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- All enemies in the same row as you suffer -1 Armor
- Effect is automatic while you're alive
- Does not stack with multiple Rust-Witches

**Formula:**
```
For each Enemy in Caster.Row:
    Enemy.Armor -= 1
```

**Tooltip:** "Entropic Field (Rank 1): Enemies in your row suffer -1 Armor."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Armor reduction increases to -2

**Formula:**
```
For each Enemy in Caster.Row:
    Enemy.Armor -= 2
```

**Tooltip:** "Entropic Field (Rank 2): Enemies in your row suffer -2 Armor."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Armor reduction remains -2
- Extends to adjacent rows
- Enemies with [Corroded] suffer additional -1 Armor

**Formula:**
```
For each Enemy in Caster.Row OR AdjacentRows:
    Enemy.Armor -= 2
    If Enemy.HasStatus("Corroded"):
        Enemy.Armor -= 1  // Additional penalty
```

**Tooltip:** "Entropic Field (Rank 3): Enemies in your row and adjacent rows suffer -2 Armor. [Corroded] enemies: additional -1 Armor."

---

## Combat Log Examples

- "Entropic Field: [Enemy] armor reduced by 1 (aura effect)"
- "Entropic Field (Rank 3): [Enemy] armor reduced by 3 (aura + [Corroded] penalty)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Rust-Witch Overview](../rust-witch-overview.md) | Parent specialization |
