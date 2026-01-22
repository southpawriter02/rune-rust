---
id: ABILITY-BLOT-PRIEST-30018
title: "Heartstopper"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Heartstopper

**Type:** Active | **Tier:** 4 (Capstone) | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Variable (see modes) |
| **Resource Cost** | 50% of Current HP (cannot use AP) |
| **Cooldown** | Once per combat |
| **Attribute** | WILL |
| **Tags** | [Capstone], [Sacrificial], [Ultimate] |
| **Ranks** | None (full power when unlocked) |

---

## Description

*"The Priest performs the ultimate sacrament, becoming a pure conduit for the world's cycle of corrupted life and death. They willingly stop their own heart for a single, terrifying moment."*

The ultimate clutch ability—massive power at terrible cost. Choose between saving your entire party (at the cost of corrupting them all) or destroying a single enemy (at the cost of your own soul).

---

## Core Mechanic

**Cost:** 50% of CURRENT HP (not Max HP)
- Cannot be paid with AP
- Cannot be reduced by Sanguine Pact
- Once per combat limitation

**Death Defiance:** If the HP cost would kill you (reduce to 0 or below):
- Set HP to 1 instead
- Corruption gain is DOUBLED

---

## Mode Selection

Choose ONE effect when activating:

### Option 1: Crimson Deluge (AoE Heal)

**Effect:**
- Heal ALL allies for 10d10 HP
- Transfer 5 Corruption to EACH ally healed
- Caster gains +10 Corruption

**Formula:**
```
HPCost = Floor(Caster.HP × 0.50)

If Caster.HP - HPCost <= 0:
    // Death Defiance
    Caster.HP = 1
    CorruptionMultiplier = 2
    Log("DEATH DEFIANCE! HP set to 1, Corruption doubled!")
Else:
    Caster.HP -= HPCost
    CorruptionMultiplier = 1

// Heal all allies
HealAmount = Roll(10d10)
For Each Ally in Party:
    Ally.HP = Min(Ally.HP + HealAmount, Ally.MaxHP)
    Ally.Corruption += (5 × CorruptionMultiplier)
    Log("Crimson Deluge: {Ally} healed {HealAmount}, +{5 × CorruptionMultiplier} Corruption")

Caster.Corruption += (10 × CorruptionMultiplier)
Log("CRIMSON DELUGE! Party healed, Corruption spread!")
```

### Option 2: Final Anathema (Execute)

**Effect:**
- Deal 20d10 Arcane damage to single enemy
- Damage bypasses ALL resistances
- Transfer ALL caster's Corruption to target
- Target makes WILL save DC 20 or suffers [Reality Glitch]
- Caster gains +15 Corruption after transfer

**Formula:**
```
HPCost = Floor(Caster.HP × 0.50)

If Caster.HP - HPCost <= 0:
    Caster.HP = 1
    CorruptionMultiplier = 2
Else:
    Caster.HP -= HPCost
    CorruptionMultiplier = 1

// Massive damage (bypasses resistance)
Damage = Roll(20d10)
Target.TakeDamage(Damage, "Arcane", BypassResistance=True)

// Transfer ALL Corruption
TransferredCorruption = Caster.Corruption
Target.Corruption += TransferredCorruption
Caster.Corruption = 0
Log("Corruption transferred: {TransferredCorruption} to {Target}")

// Reality Glitch save
WillSave = Target.RollWill()
If WillSave < 20:
    Target.ApplyStatus("[Reality Glitch]")
    Log("{Target} fails save—REALITY GLITCH!")

// Caster gains Corruption after transfer
Caster.Corruption += (15 × CorruptionMultiplier)
Log("FINAL ANATHEMA! {Damage} damage, Corruption purged then gained!")
```

---

## Crimson Deluge Details

### Healing Output

| Stat | Value |
|------|-------|
| Dice | 10d10 |
| Average | 55 HP |
| Minimum | 10 HP |
| Maximum | 100 HP |
| Targets | All allies |

### Corruption Spread

| Target | Normal | Death Defiance (2×) |
|--------|--------|---------------------|
| Each Ally | +5 | +10 |
| Caster | +10 | +20 |

**Example (4-person party, normal):**
```
Party healed for 55 HP each
Ally 1: +5 Corruption
Ally 2: +5 Corruption
Ally 3: +5 Corruption
Caster: +10 Corruption
Total party Corruption: +25
```

### With Crimson Vigor (Bloodied)

If caster is Bloodied when using Crimson Deluge:
```
Base heal: 10d10 (55 average)
Crimson Vigor R3: +100%
Final heal: 20d10 effective (110 average)
```

---

## Final Anathema Details

### Damage Output

| Stat | Value |
|------|-------|
| Dice | 20d10 |
| Average | 110 damage |
| Minimum | 20 damage |
| Maximum | 200 damage |
| Resistance | Bypassed |

### Corruption Economy

```
Before cast: Caster has 45 Corruption
Final Anathema:
  → Transfer 45 to target
  → Caster now has 0
  → Caster gains +15
  → Caster now has 15

Net effect: Caster went from 45 to 15 Corruption
(But target gained 45 Corruption!)
```

### [Reality Glitch] Status

If target fails WILL save DC 20:
- Target is [Disoriented] for 2 turns
- Target takes 2d10 Psychic damage
- Target's abilities may malfunction

---

## Rank Progression (via continued investment)

### Rank 2 Enhancement

**Crimson Deluge:**
- Heal: 12d10 (66 average)

**Final Anathema:**
- Damage: 24d10 (132 average)
- [Reality Glitch] duration extended

### Rank 3 Enhancement

**Crimson Deluge:**
- Heal: 15d10 (82.5 average)
- Corruption transfer reduced to 3 per ally

**Final Anathema:**
- Damage: 30d10 (165 average)
- If target is killed, recover ALL party's Corruption spent in this combat

---

## Death Defiance

**Trigger:** HP cost would reduce you to 0 or below

**Effect:**
- HP is set to 1 instead of dying
- All Corruption costs are DOUBLED
- You survive, but at terrible spiritual cost

**Example:**
```
Caster HP: 20
50% cost: 10 HP
Final HP: 10 (survives normally)

Caster HP: 8
50% cost: 4 HP
Final HP: 4 (survives normally)

Caster HP: 6
50% cost: 3 HP
Final HP: 3 (survives normally)

Caster HP: 2
50% cost: 1 HP
Final HP: 1 (survives normally)

Caster HP: 1
50% cost: 0.5 → rounds to 0 or 1
If would die: HP = 1, Corruption ×2
```

---

## Strategic Decision Matrix

### When to Use Crimson Deluge

| Scenario | Reasoning |
|----------|-----------|
| Multiple allies dying | Mass heal saves the fight |
| Party can afford Corruption | Allies far from thresholds |
| You're at high Corruption | You were going to corrupt anyway |
| Victory is certain with heal | End the fight healthy |

### When to Use Final Anathema

| Scenario | Reasoning |
|----------|-----------|
| Priority target must die | Boss at low HP |
| You're heavily corrupted | Transfer cleanses you |
| Ally Corruption is high | Don't spread more |
| Target is single threat | Eliminate the problem |

### When NOT to Use Either

| Scenario | Reasoning |
|----------|-----------|
| Fight is already won | Save for harder fights |
| Party already corrupted | Don't push thresholds |
| You're at 1 HP | Death Defiance doubles cost |

---

## The Ultimate Question

*"Is winning this fight worth damning yourself and everyone you heal?"*

Every Blót-Priest must answer this question when they reach for Heartstopper. There is no safe option—only degrees of sacrifice.

---

## Capstone Upgrade Effect

When Heartstopper is trained:
- All Tier 1 abilities upgrade to Rank 3
- All Tier 2 abilities upgrade to Rank 3

**Affected Abilities:**
- Sanguine Pact → Rank 3 (1:1 HP:AP, reduced Corruption)
- Blood Siphon → Rank 3 (5d6, AoE, 75% lifesteal)
- Gift of Vitae → Rank 3 (8d10, double option)
- Blood Ward → Rank 3 (3.5× shield, +2 Soak)
- Exsanguinate → Rank 3 (4d6/turn, 4 turns, 40%)
- Crimson Vigor → Rank 3 (+100% healing, +60% siphon, +1 AP)

---

## Combat Log Examples

- "HEARTSTOPPER activated! Choose: Crimson Deluge or Final Anathema?"
- "HP cost: 50% of 40 = 20 HP (40 → 20)"
- "CRIMSON DELUGE! Party healed for 58 HP!"
- "[Warrior] receives +5 Corruption (Blight Transfer)"
- "Caster gains +10 Corruption (now 35)"
- "DEATH DEFIANCE! HP set to 1, Corruption DOUBLED!"
- "FINAL ANATHEMA! 124 damage (bypasses resistance)!"
- "45 Corruption transferred to [Boss]!"
- "[Boss] fails WILL save—REALITY GLITCH!"
- "Caster gains +15 Corruption (now 15)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Blót-Priest Overview](../blot-priest-overview.md) | Parent specialization |
| [Gift of Vitae](gift-of-vitae.md) | Single-target heal comparison |
| [Crimson Vigor](crimson-vigor.md) | Enhances Crimson Deluge |
| [Corruption Resource](../../../../01-core/resources/corruption.md) | Corruption thresholds |
