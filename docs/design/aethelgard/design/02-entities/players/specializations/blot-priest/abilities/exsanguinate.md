---
id: ABILITY-BLOT-PRIEST-30014
title: "Exsanguinate"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Exsanguinate

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (30 ft range) |
| **Resource Cost** | 50 AP (or 100 HP via Sacrificial Casting) |
| **Cooldown** | 4 turns |
| **Damage Type** | Arcane (DoT) |
| **Attribute** | WILL |
| **Tags** | [Curse], [DoT], [Lifesteal], [Heretical] |
| **Ranks** | 2 → 3 |

---

## Description

*"A vicious, parasitic curse—a Blighted echo that slowly drains the victim's life force and spiritual integrity."*

This is your attrition tool. Where Blood Siphon is immediate, Exsanguinate is sustained—a curse that drains the enemy over multiple turns while feeding you a steady trickle of corrupted life force.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Curse enemy for 3 turns
- Damage: 2d6 Arcane per turn
- Lifesteal: Heal for 25% of each tick
- Corruption Cost: +1 per tick (total +3 over duration)
- Cost: 50 AP (or 100 HP)

**Formula:**
```
Caster.AP -= 50  // OR Caster.HP -= 100 via Sanguine Pact

Target.ApplyCurse("Exsanguinate", {
    Duration: 3,
    DamagePerTurn: "2d6",
    DamageType: "Arcane",
    LifestealPercent: 0.25,
    CorruptionPerTick: 1,
    Caster: Caster
})

Log("Exsanguinate: {Target} cursed for 3 turns")
```

**On Each Turn:**
```
OnTurnStart(Target):
    If Target.HasCurse("Exsanguinate"):
        Damage = Roll(2d6)
        Target.TakeDamage(Damage, "Arcane")

        HealAmount = Floor(Damage × 0.25)
        Curse.Caster.HP += HealAmount
        Curse.Caster.Corruption += 1

        Log("Exsanguinate: {Damage} damage, healed {HealAmount}, +1 Corruption")
```

**Tooltip:** "Exsanguinate (Rank 2): 2d6/turn for 3 turns, 25% lifesteal. +1 Corruption/tick."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Curse duration: 4 turns
- Damage: 4d6 Arcane per turn
- Lifesteal: 40% of each tick
- Corruption Cost: +1 per tick (total +4 over duration)
- Cost: 50 AP (or 50 HP at Sanguine Pact R3)

**Formula:**
```
Target.ApplyCurse("Exsanguinate", {
    Duration: 4,
    DamagePerTurn: "4d6",
    LifestealPercent: 0.40,
    CorruptionPerTick: 1
})
```

**Tooltip:** "Exsanguinate (Rank 3): 4d6/turn for 4 turns, 40% lifesteal. +1 Corruption/tick."

---

## Total Damage & Healing

### Rank 2 (3 turns)

| Turn | Damage (avg) | Lifesteal (25%) | Corruption |
|------|--------------|-----------------|------------|
| 1 | 7 | 1.75 | +1 |
| 2 | 7 | 1.75 | +1 |
| 3 | 7 | 1.75 | +1 |
| **Total** | **21** | **5.25** | **+3** |

### Rank 3 (4 turns)

| Turn | Damage (avg) | Lifesteal (40%) | Corruption |
|------|--------------|-----------------|------------|
| 1 | 14 | 5.6 | +1 |
| 2 | 14 | 5.6 | +1 |
| 3 | 14 | 5.6 | +1 |
| 4 | 14 | 5.6 | +1 |
| **Total** | **56** | **22.4** | **+4** |

---

## Comparison: Exsanguinate vs Blood Siphon

| Metric | Blood Siphon R3 | Exsanguinate R3 |
|--------|-----------------|-----------------|
| Total Damage | 17.5 (instant) | 56 (over 4 turns) |
| Total Heal | 13.1 (instant) | 22.4 (over 4 turns) |
| Corruption | +1 | +4 |
| AP Cost | 35 | 50 |
| Targets | 2 | 1 |

**Blood Siphon:** Burst damage and healing, lower Corruption.
**Exsanguinate:** Sustained damage and healing, higher Corruption.

---

## Corruption Economics

| Cast Method | Ability Cost | Per-Tick Cost | Total |
|-------------|--------------|---------------|-------|
| AP cast R2 | 0 | +1 × 3 | +3 |
| HP cast R1 Pact | +1 | +1 × 3 | +4 |
| AP cast R3 | 0 | +1 × 4 | +4 |
| HP cast R3 Pact | +0.5 | +1 × 4 | +4.5 |

**Warning:** Exsanguinate has the highest per-ability Corruption cost in Tier 2!

---

## Synergy with Crimson Vigor

When [Bloodied], Crimson Vigor enhances Exsanguinate's lifesteal:

| Crimson Vigor Rank | Lifesteal Bonus |
|--------------------|-----------------|
| R1 | +25% |
| R2 | +40% |
| R3 | +60% |

**Combined Lifesteal (R3 Exsanguinate + R3 Crimson Vigor):**
```
40% base + 60% bonus = 100% lifesteal per tick!
```

At 4d6 per turn with 100% lifesteal, you heal for the full damage dealt.

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Long fight | Sustained damage + healing |
| Boss battle | Apply early, benefit all fight |
| Low HP sustain | Combined with Crimson Vigor |
| Multiple enemies | Stack on different targets |

---

## Curse Stacking

- Multiple Exsanguinate curses can be active on different targets
- Only ONE Exsanguinate per target (refreshes on recast)
- Each tick generates Corruption from ALL active curses

**Example (2 cursed enemies):**
```
Turn Start:
  → Exsanguinate ticks on Enemy A: +1 Corruption
  → Exsanguinate ticks on Enemy B: +1 Corruption
  → Total: +2 Corruption this turn
```

---

## Combat Log Examples

- "Exsanguinate: [Boss] cursed for 4 turns"
- "Exsanguinate tick: 12 Arcane damage to [Boss]"
- "Life drain: Healed 4 HP (40% of 12)"
- "+1 Corruption from Exsanguinate tick"
- "Exsanguinate expires on [Boss]"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Blót-Priest Overview](../blot-priest-overview.md) | Parent specialization |
| [Blood Siphon](blood-siphon.md) | Burst lifesteal alternative |
| [Crimson Vigor](crimson-vigor.md) | [Bloodied] lifesteal bonus |
| [Hemorrhaging Curse](hemorrhaging-curse.md) | Advanced DoT curse |
