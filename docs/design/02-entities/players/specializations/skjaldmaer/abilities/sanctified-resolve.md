---
id: ABILITY-SKJALDMAER-26019
title: "Sanctified Resolve"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Sanctified Resolve

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Attribute** | WILL |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Mental fortitude training grants resistance to Fear and Psychic Stress. The Skjaldmær's mind is a fortress, unyielding against the horrors that shatter lesser warriors.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- +1 bonus die to all WILL Resolve Checks vs [Fear] effects
- +1 bonus die to all WILL Resolve Checks vs Psychic Stress

**Formula:**
```
ResolveCheckDicePool = WILL + 1
```

**Tooltip:** "Sanctified Resolve (Rank 1): +1 die vs Fear and Psychic Stress checks"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- +2 bonus dice to all WILL Resolve Checks vs [Fear] effects
- +2 bonus dice to all WILL Resolve Checks vs Psychic Stress

**Formula:**
```
ResolveCheckDicePool = WILL + 2
```

**Tooltip:** "Sanctified Resolve (Rank 2): +2 dice vs Fear and Psychic Stress checks"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +3 bonus dice to all WILL Resolve Checks vs [Fear] effects
- +3 bonus dice to all WILL Resolve Checks vs Psychic Stress
- **NEW:** Reduce all ambient Psychic Stress gain by 10%

**Formula:**
```
ResolveCheckDicePool = WILL + 3
AmbientStressGain = BaseAmbientStress * 0.90
```

**Tooltip:** "Sanctified Resolve (Rank 3): +3 dice vs Fear and Psychic Stress checks. Ambient Stress reduced by 10%."

---

## Combat Log Examples

- "Sanctified Resolve grants +1 die to Fear resistance"
- "Rolling WILL (3) + Sanctified Resolve (2) = 5 dice vs [Fear]"
- "Ambient Stress reduced by 10% (Sanctified Resolve)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skjaldmær Overview](../skjaldmaer-overview.md) | Parent specialization |
| [Stress](../../../../01-core/resources/stress.md) | Psychic Stress system |
