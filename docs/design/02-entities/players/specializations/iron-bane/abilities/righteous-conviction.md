---
id: ABILITY-IRONBANE-1108
title: "Righteous Conviction"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Righteous Conviction

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Ranks** | 2 → 3 |

---

## Description

Your faith is unshakeable. Your purpose is clear. The corrupted will fall.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- +75% Righteous Fervor generation from all sources
- +3 dice to WILL saves vs Psychic/Corruption effects
- **NEW:** Attacks vs Mechanical/Undying cannot miss (95% minimum hit chance)

**Formula:**
```
FervorGeneration *= 1.75
WILLSaveBonus += 3 (vs Psychic, Corruption)
If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
    MinHitChance = 0.95
```

**Tooltip:** "Righteous Conviction (Rank 2): +75% Fervor. +3 WILL vs Psychic/Corruption. 95% min hit vs Mech/Undying."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +100% Fervor generation (double)
- +5 dice to WILL saves
- **NEW:** Defeating Mechanical/Undying refunds 50% ability Stamina cost

**Formula:**
```
FervorGeneration *= 2.0
WILLSaveBonus += 5
OnKillMechUndying:
    RefundStamina = LastAbility.StaminaCost * 0.50
```

**Tooltip:** "Righteous Conviction (Rank 3): +100% Fervor. +5 WILL. 50% Stamina refund on Mech/Undying kill."

---

## Combat Log Examples

- "Righteous Conviction: +75% Fervor generation active."
- "Righteous Conviction: Attack cannot miss vs [Automaton] (95% minimum)."
- "[Automaton] destroyed! 50% Stamina refunded (Righteous Conviction)."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Iron-Bane Overview](../iron-bane-overview.md) | Parent specialization |
