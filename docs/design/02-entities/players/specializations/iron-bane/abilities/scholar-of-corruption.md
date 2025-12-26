---
id: ABILITY-IRONBANE-1101
title: "Scholar of Corruption"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Scholar of Corruption

**Type:** Passive/Active | **Tier:** 1 | **PP Cost:** 0 (free with specialization)

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action |
| **Target** | Single Enemy |
| **Resource Cost** | None |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You have studied the schematics of the Undying, memorized their corrupted code. Where others see invincible foes, you see exploitable bugs.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- **Observe** (Free Action): Target enemy reveals type, resistances, and vulnerabilities
- Mechanical/Undying targets reveal 1 additional weakness

**Formula:**
```
Observe(Target):
    Reveal: EnemyType, Resistances, Vulnerabilities
    If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
        Reveal: AdditionalWeakness
    Target.IsIdentified = true
```

**Tooltip:** "Scholar of Corruption (Rank 1): Observe (Free Action) reveals enemy type, resistances, vulnerabilities. Mech/Undying reveal 1 extra weakness."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- All Rank 1 effects
- **NEW:** Observing an enemy grants +10 Righteous Fervor
- **NEW:** Auto-observe all enemies at combat start

**Formula:**
```
OnCombatStart:
    For each Enemy in AllEnemies:
        Observe(Enemy)
OnObserve:
    Fervor += 10
```

**Tooltip:** "Scholar of Corruption (Rank 2): +10 Fervor on Observe. Auto-observe all enemies at combat start."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- All Rank 2 effects
- **NEW:** See exact enemy HP values
- **NEW:** Mechanical/Undying below 30% HP marked [Critical Failure] (guaranteed crit)

**Formula:**
```
Target.ShowHP = true
If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
    If (Target.HP < Target.MaxHP * 0.30):
        Target.AddStatus("CriticalFailure")
        // All attacks against target auto-crit
```

**Tooltip:** "Scholar of Corruption (Rank 3): See enemy HP. Mech/Undying below 30% HP: guaranteed crits."

---

## Status Effect: [Critical Failure]

| Property | Value |
|----------|-------|
| **Duration** | Until HP > 30% |
| **Icon** | Target with X |
| **Effects** | All attacks against target auto-crit |

---

## Combat Log Examples

- "Scholar of Corruption: [Automaton] analyzed. Type: Mechanical. Weak to Fire, Electricity."
- "Scholar of Corruption (Rank 2): +10 Fervor gained from observation."
- "[Corrupted Drone] enters Critical Failure state! All attacks auto-crit."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Iron-Bane Overview](../iron-bane-overview.md) | Parent specialization |
| [Weakness Exploiter](weakness-exploiter.md) | Synergy ability |
