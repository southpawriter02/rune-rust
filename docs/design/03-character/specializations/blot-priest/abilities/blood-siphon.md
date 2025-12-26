---
id: ABILITY-BLOT-PRIEST-30011
title: "Blood Siphon"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Blood Siphon

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (30 ft range) |
| **Resource Cost** | 35 AP (or 70 HP via Sacrificial Casting) |
| **Cooldown** | None |
| **Damage Type** | Arcane |
| **Attribute** | WILL |
| **Tags** | [Siphon], [Lifesteal], [Heretical] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

*"A crimson tendril of energy leeches the tainted life force of a foe, drawing it back to the caster."*

This is your core offensive spell and sustain tool. The life force you drain is contaminated with Blight—it heals you, but at the cost of your spiritual integrity.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Deal 3d6 Arcane damage
- Heal self for 50% of damage dealt
- +1 Corruption (from consuming Blighted life force)
- Cost: 35 AP (or 70 HP)

**Formula:**
```
Caster.AP -= 35  // OR Caster.HP -= 70 via Sanguine Pact

Damage = Roll(3d6)
Target.TakeDamage(Damage, "Arcane")

HealAmount = Floor(Damage × 0.50)
Caster.HP += HealAmount

Caster.Corruption += 1
Log("Blood Siphon: {Damage} damage, healed {HealAmount}, +1 Corruption")
```

**Tooltip:** "Blood Siphon (Rank 1): 3d6 Arcane, 50% lifesteal. +1 Corruption. Cost: 35 AP"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Deal 4d6 Arcane damage
- Heal self for 60% of damage dealt
- +1 Corruption
- Cost: 35 AP (or 70 HP at R1 / 53 HP at Sanguine Pact R2)

**Formula:**
```
Caster.AP -= 35

Damage = Roll(4d6)
Target.TakeDamage(Damage, "Arcane")

HealAmount = Floor(Damage × 0.60)
Caster.HP += HealAmount

Caster.Corruption += 1
Log("Blood Siphon: {Damage} damage, healed {HealAmount}")
```

**Tooltip:** "Blood Siphon (Rank 2): 4d6 Arcane, 60% lifesteal. +1 Corruption."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Deal 5d6 Arcane damage
- Heal self for 75% of damage dealt
- **NEW:** AoE—hits 2 enemies (split targeting)
- +1 Corruption (total, not per target)
- Cost: 35 AP (or 35 HP at Sanguine Pact R3)

**Formula:**
```
Caster.AP -= 35

Targets = SelectTargets(2, "Enemy")

TotalHeal = 0
For Each Target in Targets:
    Damage = Roll(5d6)
    Target.TakeDamage(Damage, "Arcane")
    TotalHeal += Floor(Damage × 0.75)

Caster.HP += TotalHeal
Caster.Corruption += 1

Log("Blood Siphon: {TotalDamage} damage to {TargetCount} targets, healed {TotalHeal}")
```

**Tooltip:** "Blood Siphon (Rank 3): 5d6 Arcane to 2 targets, 75% lifesteal. +1 Corruption."

---

## Damage & Healing Output

| Rank | Dice | Avg Damage | Lifesteal % | Avg Heal |
|------|------|------------|-------------|----------|
| 1 | 3d6 | 10.5 | 50% | 5.25 |
| 2 | 4d6 | 14 | 60% | 8.4 |
| 3 | 5d6 | 17.5 | 75% | 13.1 |

### Rank 3 Multi-Target

| Targets | Total Damage | Total Heal |
|---------|--------------|------------|
| 1 | 17.5 | 13.1 |
| 2 | 35 | 26.2 |

---

## Synergy with Crimson Vigor

When [Bloodied] (<50% HP), Crimson Vigor enhances Blood Siphon:

| Crimson Vigor Rank | Siphon Bonus |
|--------------------|--------------|
| R1 | +25% lifesteal |
| R2 | +40% lifesteal |
| R3 | +60% lifesteal |

**Combined Lifesteal (R3 Blood Siphon + R3 Crimson Vigor):**
```
75% base + 60% bonus = 135% lifesteal when Bloodied!
```

This creates a self-sustaining combat loop where low HP actually increases survival.

---

## Corruption Economics

| Cast Method | Resource Cost | Corruption |
|-------------|---------------|------------|
| AP cast | 35 AP | +1 |
| HP cast (Sanguine Pact R1) | 70 HP | +2 (1 from siphon, 1 from HP cast) |
| HP cast (Sanguine Pact R3) | 35 HP | +1.5 (1 from siphon, 0.5 from HP cast) |

**Warning:** HP-casting Blood Siphon generates double Corruption at early ranks!

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Sustain damage | Heal while dealing damage |
| Low HP combat | Get Bloodied → Crimson Vigor → massive healing |
| Multi-target (R3) | Efficient against 2+ enemies |
| AP conservation | Use HP when AP is needed elsewhere |

---

## The Corrupted Harvest

Every use of Blood Siphon makes you more monstrous:

```
Combat starts: 20 Corruption
Blood Siphon ×1: 21 Corruption
Blood Siphon ×2: 22 Corruption
Blood Siphon ×3: 23 Corruption
Combat ends: 23 Corruption

One fight added 3 permanent Corruption.
```

This is the price of the Blót-Priest's sustain.

---

## Combat Log Examples

- "Blood Siphon: 14 Arcane damage to [Enemy]!"
- "Life Siphon: Healed 8 HP (60% of 14)"
- "+1 Corruption from consuming Blighted life force"
- "Blood Siphon (AoE): 32 total damage, healed 24 HP!"
- "[Bloodied] + Crimson Vigor: Lifesteal increased to 135%!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Blót-Priest Overview](../blot-priest-overview.md) | Parent specialization |
| [Sanguine Pact](sanguine-pact.md) | HP casting option |
| [Crimson Vigor](crimson-vigor.md) | [Bloodied] synergy |
| [Exsanguinate](exsanguinate.md) | DoT lifesteal alternative |
