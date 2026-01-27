---
id: ABILITY-MYRK-GENGR-24011
title: "Enter the Void"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Enter the Void

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action (Bonus Action at Rank 3) |
| **Target** | Self |
| **Resource Cost** | 40 Stamina (35 at Rank 2+) |
| **Check** | FINESSE + Acrobatics (Stealth) |
| **Tags** | [Stealth], [State Change] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You focus your will, synchronizing your presence with the world's psychic static. You vanish from sight—not hidden in shadow, but erased from perception.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Cost: 40 Stamina
- Make FINESSE + Acrobatics (Stealth) check vs DC 16
- Success: Enter [Hidden] state
- While Hidden: enemies cannot target you directly

**Formula:**
```
Caster.Stamina -= 40
StealthCheck = Roll(FINESSE + AcrobaticsBonus + OneWithStaticBonus)

If StealthCheck >= 16:
    Caster.AddStatus("Hidden")
    Log("You vanish into the psychic static!")
Else:
    Log("Enter the Void: Stealth check failed (DC 16)")
```

**Tooltip:** "Enter the Void (Rank 1): DC 16 to enter [Hidden]. Cost: 40 Stamina"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Cost reduced to 35 Stamina
- DC reduced to 14
- All other effects unchanged

**Formula:**
```
Caster.Stamina -= 35
StealthCheck = Roll(FINESSE + AcrobaticsBonus + OneWithStaticBonus)

If StealthCheck >= 14:
    Caster.AddStatus("Hidden")
Else:
    Log("Enter the Void: Stealth check failed (DC 14)")
```

**Tooltip:** "Enter the Void (Rank 2): DC 14 to enter [Hidden]. Cost: 35 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Cost: 35 Stamina
- DC reduced to 12
- **UPGRADE:** Can use as Bonus Action instead of Standard Action
- Allows entering stealth and attacking in same turn

**Formula:**
```
Caster.Stamina -= 35
ActionType = BonusAction  // Can still use Standard Action for attack

StealthCheck = Roll(FINESSE + AcrobaticsBonus + OneWithStaticBonus)

If StealthCheck >= 12:
    Caster.AddStatus("Hidden")
```

**Tooltip:** "Enter the Void (Rank 3): DC 12, Bonus Action. Enter stealth and attack same turn!"

---

## DC Comparison

| Rank | DC | Stamina | Action |
|------|-----|---------|--------|
| 1 | 16 | 40 | Standard |
| 2 | 14 | 35 | Standard |
| 3 | 12 | 35 | Bonus |

---

## [Hidden] State

| Property | Value |
|----------|-------|
| **Effect** | Enemies cannot target you directly |
| **Duration** | Until broken |
| **Breaks On** | Attack, failed stealth, AoE detection |
| **Visual** | Character becomes translucent/static |

---

## Stealth Check Example

**Rank 2, FINESSE 4, One with Static Rank 2, in Resonance Zone:**
```
Roll = 4d10 (FINESSE) + 2d10 (One with Static) + 2d10 (Resonance) = 8d10
DC = 14
Average roll of 8d10 = 44, very likely success
```

---

## Combat Log Examples

- "Enter the Void: Stealth check 23 vs DC 16. Success!"
- "You vanish into the psychic static!"
- "Enter the Void (Rank 3): Bonus Action stealth entry!"
- "Enter the Void: Stealth check 11 vs DC 14. Failed."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Myrk-Gengr Overview](../myrk-gengr-overview.md) | Parent specialization |
| [Shadow Strike](shadow-strike.md) | Attack from [Hidden] |
| [One with the Static I](one-with-the-static-i.md) | Stealth bonus |
