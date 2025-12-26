---
id: ABILITY-JOTUNREADER-202
title: "Analyze Weakness"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Analyze Weakness

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 30 Stamina + Variable Psychic Stress |
| **Attribute** | WITS |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Clinical observation reveals structural flaws. You document weakness like a pathologist identifies cause of death. Your analysis enables allies to strike with precision.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- Make WITS check vs target
- **Success:** Reveal 1 Resistance + 1 Vulnerability
- **Critical Success:** Reveal ALL Resistances/Vulnerabilities + AI behavior hint
- **Psychic Stress Cost:** 5 Stress

**Formula:**
```
WITSCheck = Roll(WITS dice) vs TargetDifficulty
StaminaCost = 30
StressCost = 5

If Success:
    Reveal(target.Resistances[0], target.Vulnerabilities[0])
If CriticalSuccess:
    Reveal(target.AllResistances, target.AllVulnerabilities, target.AIHint)
```

**Tooltip:** "Analyze Weakness (Rank 1): Reveal 1 Resistance + 1 Vulnerability. Cost: 30 Stamina, 5 Psychic Stress"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Make WITS check vs target
- **Success:** Reveal 2 Resistances + 2 Vulnerabilities
- **Critical Success:** Also reveals special ability information
- **Reduced Psychic Stress Cost:** 3 Stress

**Formula:**
```
WITSCheck = Roll(WITS dice) vs TargetDifficulty
StaminaCost = 30
StressCost = 3  // Reduced from 5

If Success:
    Reveal(target.Resistances[0..1], target.Vulnerabilities[0..1])
If CriticalSuccess:
    Reveal(All + target.SpecialAbilityInfo)
```

**Tooltip:** "Analyze Weakness (Rank 2): Reveal 2 Resistances + 2 Vulnerabilities. Cost: 30 Stamina, 3 Psychic Stress"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Make WITS check vs target
- **Success:** Auto-reveals Critical-level information
- **NEW:** Can use as Free Action once per combat
- **Zero Psychic Stress Cost**
- **Reduced Stamina Cost:** 25

**Formula:**
```
WITSCheck = Roll(WITS dice) vs TargetDifficulty
StaminaCost = 25  // Reduced from 30
StressCost = 0    // Eliminated

If Success:
    Reveal(target.AllResistances, target.AllVulnerabilities, target.SpecialAbilityInfo)

FreeActionUsesRemaining = 1 per combat
```

**Tooltip:** "Analyze Weakness (Rank 3): Full analysis on Success. Free Action 1/combat. Cost: 25 Stamina, 0 Stress"

---

## Combat Log Examples

- "Analyze Weakness reveals: [Enemy] is Resistant to Fire, Vulnerable to Lightning"
- "CRITICAL: Full analysis complete! [Enemy] behavior pattern identified"
- "Analyze Weakness (Rank 3) [FREE ACTION]: Complete analysis of [Enemy]"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Jötun-Reader Overview](../jotun-reader-overview.md) | Parent specialization |
| [Exploit Design Flaw](exploit-design-flaw.md) | Synergy ability |
