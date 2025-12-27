---
id: ABILITY-BLOT-PRIEST-30012
title: "Gift of Vitae"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Gift of Vitae

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Ally (30 ft range) |
| **Resource Cost** | 15% of Caster's Max HP (cannot use AP) |
| **Cooldown** | None |
| **Attribute** | WILL |
| **Tags** | [Healing], [Sacrificial], [Blight Transfer] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

*"The Priest anoints an ally with their own vital essence—a potent act of healing, a direct transfer of life from caster to ward."*

The most powerful single-target heal in the early game—but it spreads the sickness. Every life you save carries a piece of your Corruption with it.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Cost: 15% of Max HP (cannot use AP)
- Heal ally for 4d10 HP
- **Blight Transfer:** Transfer 1 Corruption to target

**Formula:**
```
HPCost = Floor(Caster.MaxHP × 0.15)

If Caster.HP - HPCost < 1:
    Log("Cannot cast: Would reduce HP below 1")
    Return FAIL

Caster.HP -= HPCost

HealAmount = Roll(4d10)
Target.HP = Min(Target.HP + HealAmount, Target.MaxHP)

// Blight Transfer
Target.Corruption += 1
Log("Gift of Vitae: {Target} healed {HealAmount} HP")
Log("Blight Transfer: {Target} received 1 Corruption")
```

**Tooltip:** "Gift of Vitae (Rank 1): Heal ally 4d10. Transfers 1 Corruption. Cost: 15% Max HP"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Cost: 15% of Max HP
- Heal ally for 6d10 HP
- **Blight Transfer:** Transfer 1 Corruption (unchanged)

**Formula:**
```
HPCost = Floor(Caster.MaxHP × 0.15)
Caster.HP -= HPCost

HealAmount = Roll(6d10)
Target.HP = Min(Target.HP + HealAmount, Target.MaxHP)

Target.Corruption += 1
Log("Gift of Vitae: {Target} healed {HealAmount} HP, +1 Corruption")
```

**Tooltip:** "Gift of Vitae (Rank 2): Heal ally 6d10. Transfers 1 Corruption. Cost: 15% Max HP"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Cost: 15% of Max HP
- Heal ally for 8d10 HP
- **Blight Transfer:** Transfer 1 Corruption
- **NEW OPTION:** Can choose to DOUBLE the healing (16d10) at cost of transferring 3 Corruption

**Formula:**
```
HPCost = Floor(Caster.MaxHP × 0.15)
Caster.HP -= HPCost

DoubleMode = PromptChoice("Double healing for +3 Corruption?")

If DoubleMode:
    HealAmount = Roll(16d10)  // Doubled
    CorruptionTransfer = 3
    Log("EMPOWERED Gift of Vitae!")
Else:
    HealAmount = Roll(8d10)
    CorruptionTransfer = 1

Target.HP = Min(Target.HP + HealAmount, Target.MaxHP)
Target.Corruption += CorruptionTransfer

Log("Gift of Vitae: {Target} healed {HealAmount} HP, +{CorruptionTransfer} Corruption")
```

**Tooltip:** "Gift of Vitae (Rank 3): Heal ally 8d10 (+1 Corruption) OR 16d10 (+3 Corruption). Cost: 15% Max HP"

---

## Healing Output

| Rank | Dice | Average | Max |
|------|------|---------|-----|
| 1 | 4d10 | 22 | 40 |
| 2 | 6d10 | 33 | 60 |
| 3 | 8d10 | 44 | 80 |
| 3 (Double) | 16d10 | 88 | 160 |

### Comparison to Other Healers

| Healer | Typical Heal | Corruption Cost |
|--------|--------------|-----------------|
| Bone-Setter | 3d8 (13.5) | 0 |
| Gift of Vitae R1 | 4d10 (22) | 1 to ally |
| Gift of Vitae R3 | 8d10 (44) | 1 to ally |
| Gift of Vitae R3 (Double) | 16d10 (88) | 3 to ally |

**The Blót-Priest is the strongest healer—at a price.**

---

## HP Cost Calculation

| Max HP | 15% Cost |
|--------|----------|
| 60 | 9 HP |
| 80 | 12 HP |
| 100 | 15 HP |
| 120 | 18 HP |

**Note:** This ability CANNOT use AP. You must always sacrifice your own life force.

---

## Synergy with Crimson Vigor

When [Bloodied] (<50% HP), Crimson Vigor enhances Gift of Vitae:

| Crimson Vigor Rank | Healing Bonus |
|--------------------|---------------|
| R1 | +50% |
| R2 | +75% |
| R3 | +100% (double) |

**Combined Output (R3 Gift + R3 Crimson Vigor, not doubled mode):**
```
8d10 × 2.0 = 16d10 effective healing (88 average)
With only 1 Corruption transfer!
```

Being low on HP makes your healing MORE effective.

---

## Blight Transfer: The Moral Dimension

Every heal spreads Corruption. Consider:

| Scenario | Decision |
|----------|----------|
| Ally at 5 Corruption | Safe to heal—far from thresholds |
| Ally at 23 Corruption | One heal puts them at 24—still safe |
| Ally at 24 Corruption | One heal = 25 = first threshold trigger |
| Ally at 49 Corruption | One heal = 50 = loses faction rep |
| Ally at 74 Corruption | One heal = 75 = gains [MACHINE AFFINITY] trauma |

**The question: Is saving their life now worth corrupting their soul?**

---

## Double Healing Decision (Rank 3)

| Mode | Healing | Corruption | When to Use |
|------|---------|------------|-------------|
| Standard | 8d10 | 1 | Ally can't afford more Corruption |
| Double | 16d10 | 3 | Ally is dying AND can afford Corruption |

**Never use Double mode on:**
- Allies approaching Corruption thresholds
- Allies already corrupted
- Situations where standard healing is sufficient

---

## Combat Log Examples

- "Gift of Vitae: [Berserkr] healed for 34 HP"
- "Blight Transfer: [Berserkr] received 1 Corruption (now 15)"
- "EMPOWERED Gift of Vitae: [Tank] healed for 78 HP!"
- "Blight Transfer: [Tank] received 3 Corruption (now 28)"
- "WARNING: [Ally] is approaching Corruption threshold 25!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Blót-Priest Overview](../blot-priest-overview.md) | Parent specialization |
| [Crimson Vigor](crimson-vigor.md) | [Bloodied] healing bonus |
| [Corruption Resource](../../../../01-core/resources/corruption.md) | Corruption thresholds |
| [Heartstopper](heartstopper.md) | AoE healing alternative |
