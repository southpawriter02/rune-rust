---
id: ABILITY-STRANDHOGG-25009
title: "Riptide of Carnage"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Riptide of Carnage

**Type:** Active (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Up to 4 different enemies |
| **Resource Cost** | 60 Stamina + 75 Momentum |
| **Cooldown** | Once Per Combat |
| **Damage Type** | Physical |
| **Attribute** | MIGHT |
| **Self-Stress** | +10 Psychic Stress |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |
| **Tags** | [Capstone], [Multi-Attack], [Momentum] |
| **Ranks** | None (full power when unlocked) |

---

## Description

You become a blur of violence, moving so fast that time itself stutters. In a single moment, you strike four different enemies—a riptide of carnage that leaves bodies in its wake. The causality violation takes its toll on your mind.

---

## Mechanical Effect

**Multi-Attack:**
- Make 4 attacks against different enemies in a single turn
- Each attack deals 4d10 + MIGHT Physical damage
- After all attacks, gain 10 Psychic Stress (causality violation)
- Each kill refunds 10 Momentum

**Target Requirement:**
- Requires 4 different valid targets
- If fewer targets available, makes fewer attacks
- Cannot attack same enemy twice

**Formula:**
```
Caster.Stamina -= 60
Caster.Momentum -= 75

Targets = SelectUpTo4DifferentEnemies()
KillCount = 0

For each Target in Targets:
    Damage = Roll(4d10) + MIGHT
    Target.TakeDamage(Damage, "Physical")
    Log("Riptide attack #{i}: {Damage} to {Target}")

    If Target.HP <= 0:
        KillCount += 1
        Caster.Momentum += 10  // Per-kill refund

Caster.PsychicStress += 10
Log("Reality violation: +10 Psychic Stress")
Log("Kills: {KillCount}, Momentum refunded: {KillCount * 10}")
```

**Tooltip:** "Riptide of Carnage: 4 attacks (4d10+MIGHT each) vs different enemies. +10 Stress. Kill = +10 Momentum. Once/combat."

---

## Damage Output

**Per Attack:**
- 4d10 + MIGHT
- Average (MIGHT +3): 22 + 3 = 25 damage

**Total (4 Attacks):**
- Total dice: 16d10 + 4×MIGHT
- Average total: 100 + 12 = 112 damage distributed

---

## Momentum Economics

| Scenario | Spent | Refunded | Net Cost |
|----------|-------|----------|----------|
| 0 kills | 75 | 0 | 75 |
| 1 kill | 75 | 10 | 65 |
| 2 kills | 75 | 20 | 55 |
| 3 kills | 75 | 30 | 45 |
| 4 kills | 75 | 40 | 35 |

---

## No Quarter Synergy

Each kill from Riptide triggers No Quarter:
- +10 Momentum (from No Quarter)
- +15 Temporary HP
- Free movement (per kill)

**4-Kill Scenario:**
- Riptide kill refund: +40 Momentum
- No Quarter: +40 Momentum + 60 TempHP + 4 free moves
- Total: Spent 75, gained 80 = +5 net Momentum

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Mob clear | Delete 4 weakened enemies |
| Opener | Max Momentum → Riptide → Chain kills |
| Turning point | Shift battle momentum dramatically |
| Cleanup | Finish wounded enemies for refunds |

---

## Setup Sequence

1. Build to 75+ Momentum via Reaver's Strike + Tidal Rush
2. Apply [Disoriented] to multiple enemies via Dread Charge
3. Riptide of Carnage → 4 attacks, likely multiple kills
4. No Quarter triggers → Resources refunded
5. Continue the slaughter

---

## Combat Log Example

```
═══════════════════════════════════════════
        RIPTIDE OF CARNAGE!
═══════════════════════════════════════════
Time stutters. Bodies fall.

Attack 1: 27 damage to [Enemy A] - KILLED!
Attack 2: 22 damage to [Enemy B] - KILLED!
Attack 3: 25 damage to [Enemy C] - KILLED!
Attack 4: 24 damage to [Enemy D]

Kills: 3
Momentum refunded: +30

Reality violation: +10 Psychic Stress
No Quarter (×3): Free moves + 30 Momentum + 45 TempHP
═══════════════════════════════════════════
```

---

## GUI Display - Capstone Notification

```
┌─────────────────────────────────────────────┐
│         RIPTIDE OF CARNAGE!                 │
├─────────────────────────────────────────────┤
│                                             │
│  You become a blur of violence!             │
│                                             │
│  • 4 attacks against different enemies      │
│  • 4d10+MIGHT each                          │
│  • +10 Psychic Stress (reality violation)   │
│                                             │
│  "Time stutters. Bodies fall."              │
│                                             │
└─────────────────────────────────────────────┘
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Strandhogg Overview](../strandhogg-overview.md) | Parent specialization |
| [No Quarter](no-quarter.md) | Kill synergy |
| [Tidal Rush](tidal-rush.md) | Momentum building |
| [Momentum Resource](../../../../01-core/resources/momentum.md) | Resource system |
