---
id: ABILITY-BLOT-PRIEST-30015
title: "Crimson Vigor"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Crimson Vigor

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Trigger** | [Bloodied] status (<50% HP) |
| **Resource Cost** | None |
| **Tags** | [Bloodied], [Enhancement], [Heretical] |
| **Ranks** | 2 → 3 |

---

## Description

*"The Blót-Priest learns to thrive on the edge of oblivion. As their life force wanes and their corruption deepens, their connection to the Blight's power solidifies."*

This passive rewards the precarious edge-of-death playstyle that defines the Blót-Priest. The lower you fall, the stronger your gifts become—a dark bargain that makes survival through sacrifice not just possible, but optimal.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- When [Bloodied] (<50% HP):
  - Healing abilities gain +50% potency
  - Siphon effects (lifesteal) gain +25% effectiveness

**Formula:**
```
OnHealingAbility(BaseHeal):
    If Caster.IsBloodied:
        ModifiedHeal = Floor(BaseHeal × 1.50)
        Log("Crimson Vigor: Healing enhanced +50%!")
        Return ModifiedHeal
    Return BaseHeal

OnLifesteal(BaseLifesteal):
    If Caster.IsBloodied:
        ModifiedLifesteal = Floor(BaseLifesteal × 1.25)
        Log("Crimson Vigor: Siphon enhanced +25%!")
        Return ModifiedLifesteal
    Return BaseLifesteal
```

**Tooltip:** "Crimson Vigor (Rank 2): When Bloodied, +50% healing, +25% siphon."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- When [Bloodied] (<50% HP):
  - Healing abilities gain +100% potency (doubled!)
  - Siphon effects gain +60% effectiveness
  - **NEW:** Regenerate +1 AP per turn

**Formula:**
```
OnHealingAbility(BaseHeal):
    If Caster.IsBloodied:
        ModifiedHeal = Floor(BaseHeal × 2.00)  // Doubled
        Return ModifiedHeal
    Return BaseHeal

OnLifesteal(BaseLifesteal):
    If Caster.IsBloodied:
        ModifiedLifesteal = Floor(BaseLifesteal × 1.60)
        Return ModifiedLifesteal
    Return BaseLifesteal

OnTurnStart:
    If Caster.IsBloodied AND Caster.HasAbility("CrimsonVigor", Rank >= 3):
        Caster.AP += 1
        Log("Crimson Vigor: +1 AP regeneration (Bloodied)")
```

**Tooltip:** "Crimson Vigor (Rank 3): When Bloodied, +100% healing, +60% siphon, +1 AP/turn."

---

## Bonus Summary

| Rank | Healing Bonus | Siphon Bonus | AP Regen |
|------|---------------|--------------|----------|
| R2 | +50% | +25% | — |
| R3 | +100% | +60% | +1/turn |

---

## Affected Abilities

### Healing Abilities

| Ability | Base | With R3 Crimson Vigor |
|---------|------|-----------------------|
| Gift of Vitae R1 | 4d10 (22) | 8d10 effective (44) |
| Gift of Vitae R2 | 6d10 (33) | 12d10 effective (66) |
| Gift of Vitae R3 | 8d10 (44) | 16d10 effective (88) |
| Gift of Vitae R3 Double | 16d10 (88) | 32d10 effective (176!) |

### Siphon Abilities

| Ability | Base Lifesteal | With R3 Crimson Vigor |
|---------|----------------|----------------------|
| Blood Siphon R3 | 75% | 135% (75 × 1.6) |
| Exsanguinate R3 | 40% | 64% (40 × 1.6) |
| Hemorrhaging Curse | 25% | 40% (25 × 1.6) |

---

## The [Bloodied] Threshold

| HP State | Crimson Vigor Active? |
|----------|----------------------|
| 100% HP | No |
| 51% HP | No |
| 50% HP | **YES** |
| 25% HP | **YES** |
| 1 HP | **YES** |

**Important:** The bonuses activate the moment you cross below 50% HP and deactivate if healed above 50%.

---

## Strategic Implications

### Stay Low, Heal High

The Crimson Vigor paradox:
1. Your healing is strongest when you're low on HP
2. Healing allies doesn't raise YOUR HP
3. Blood Siphon heals you—but might push you above 50%
4. Optimal play: Stay Bloodied, maximize output

### Managing the Edge

| Risk | Mitigation |
|------|------------|
| Death at low HP | Martyr's Resolve gives +Soak |
| Accidental over-heal | Control siphon usage |
| Burst damage | Blood Ward on self |
| Corruption spiral | Inevitable—plan for it |

---

## Synergy Examples

### Gift of Vitae + Crimson Vigor R3

```
Caster at 40% HP (Bloodied)
Gift of Vitae R3: 8d10
Crimson Vigor: +100%
Final heal: 16d10 (88 average)

Cost: 15% Max HP + 1 Corruption transfer
```

### Blood Siphon + Crimson Vigor R3

```
Caster at 30% HP (Bloodied)
Blood Siphon R3: 5d6 to 2 targets (35 total damage)
Base lifesteal: 75% = 26.25 HP
Crimson Vigor: +60% = 42 HP total

Caster heals from 30% to potentially 70%+
(Now above Bloodied threshold—bonuses end)
```

### The Optimization Loop

```
1. Take damage → Drop below 50% → [Bloodied]
2. Crimson Vigor activates → +100% healing
3. Heal ally with Gift of Vitae → Massive heal
4. Stay below 50% → Keep bonuses
5. Use Blood Siphon sparingly to avoid going above 50%
```

---

## AP Regeneration (Rank 3)

**+1 AP per turn while Bloodied:**
- Helps sustain casting when HP-casting isn't viable
- Stacks with other AP regen sources
- Encourages staying in the danger zone

---

## Combat Log Examples

- "HP dropped below 50%—[Bloodied]!"
- "Crimson Vigor: Healing abilities +100%"
- "Crimson Vigor: Siphon effects +60%"
- "Gift of Vitae: 44 base → 88 actual (Crimson Vigor)"
- "Blood Siphon lifesteal: 75% → 120% (Crimson Vigor)"
- "Crimson Vigor: +1 AP regeneration"
- "HP restored above 50%—Crimson Vigor deactivated"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Blót-Priest Overview](../blot-priest-overview.md) | Parent specialization |
| [Blood Siphon](blood-siphon.md) | Enhanced by siphon bonus |
| [Gift of Vitae](gift-of-vitae.md) | Enhanced by healing bonus |
| [Martyr's Resolve](martyrs-resolve.md) | Defensive [Bloodied] synergy |
