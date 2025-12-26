---
id: ABILITY-ECHO-CALLER-28011
title: "Scream of Silence"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Scream of Silence

**Type:** Active [Echo] | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 35 Aether |
| **Damage Type** | Psychic |
| **Tags** | [Echo] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You project a fragment of the Great Silence's eternal scream into your target's mind. Against those already gripped by terror, the effect is devastating.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- Deal 3d8 Psychic damage
- If target is already [Feared]: +1d8 bonus damage

**Formula:**
```
Damage = Roll(3d8)
If Target.HasStatus("Feared"):
    Damage += Roll(1d8)
```

**Tooltip:** "Scream of Silence (Rank 1): 3d8 Psychic. +1d8 vs [Feared]. Cost: 35 Aether"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Deal 4d8 Psychic damage
- If target is [Feared]: +2d8 bonus damage

**Formula:**
```
Damage = Roll(4d8)
If Target.HasStatus("Feared"):
    Damage += Roll(2d8)
```

**Tooltip:** "Scream of Silence (Rank 2): 4d8 Psychic. +2d8 vs [Feared]. Cost: 35 Aether"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Deal 5d8 Psychic damage
- If target is [Feared]: +2d8 bonus damage
- **[Echo Chain]:** 50% damage spreads to adjacent enemy

**Formula:**
```
Damage = Roll(5d8)
If Target.HasStatus("Feared"):
    Damage += Roll(2d8)

// Echo Chain
If AdjacentEnemy exists:
    ChainDamage = Damage * 0.50
    AdjacentEnemy.TakeDamage(ChainDamage)
```

**Tooltip:** "Scream of Silence (Rank 3): 5d8 Psychic. +2d8 vs Feared. Echo Chain: 50% to adjacent."

---

## Echo Chain Mechanics

At Rank 3, this ability gains [Echo Chain]:
- 50% of final damage spreads to one adjacent enemy
- With Echo Cascade: range and damage % increase
- With Echo Attunement Rank 3: +1 tile range

---

## Combat Log Examples

- "Scream of Silence: 18 Psychic damage to [Enemy]"
- "Scream of Silence vs [Feared]: 26 Psychic damage (bonus applied)"
- "ECHO CHAIN: 13 damage spreads to [Adjacent Enemy]!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Echo-Caller Overview](../echo-caller-overview.md) | Parent specialization |
| [Echo Cascade](echo-cascade.md) | Chain enhancement |
