---
id: ABILITY-JOTUNREADER-209
title: "Architect of the Silence"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Architect of the Silence

**Type:** Active | **Tier:** 4 (Capstone) | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Jötun-Forged or Undying Enemy |
| **Resource Cost** | 60 Stamina + 10-15 Psychic Stress |
| **Cooldown** | Once per combat |
| **Valid Targets** | Jötun-Forged or Undying ONLY |
| **Ranks** | None (full power when unlocked) |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |

---

## Description

"PRIORITY OVERRIDE: TIWAZ PROTOCOL ALPHA. CEASE HOSTILE OPERATIONS." You speak command-line code in the language of the Great Silence. The machine's logic wars with corrupted directives. You are the Architect—you built this system, and you can shut it down.

---

## Mechanical Effect

### Active Component (Once per combat)
- Speak command syntax against Jötun-Forged or Undying enemy
- Target makes high-DC WILL check
- **Target Fails WILL:** [Seized] for 2 rounds (complete paralysis)
- **Target Passes WILL:** [Disoriented] for 1 round
- If target <50% HP: Effect is automatic (but locks ability for entire day)

### Passive Component (Always active)
- Auto-Critical analyze ALL Jötun-Forged/Undying enemies at combat start
- No Analyze Weakness action needed vs machines

**Formula:**
```
StaminaCost = 60
StressCost = Roll(10 + 1d6)  // 10-15

ValidTargets: enemy.Type IN ["Jötun-Forged", "Undying"]
UsesPerCombat = 1

TargetWILLCheck = Target.Roll(WILL) vs DC 18  // High DC

If TargetWILLCheck.Fail:
    Target.AddStatusEffect("Seized", Duration: 2)  // Complete paralysis
If TargetWILLCheck.Pass:
    Target.AddStatusEffect("Disoriented", Duration: 1)

If Target.HP < Target.MaxHP * 0.5:
    // Auto-success, but daily lockout
    Target.AddStatusEffect("Seized", Duration: 2)
    Ability.LockedUntilNextDay = true

// Passive
OnCombatStart:
    For each enemy where enemy.Type IN ["Jötun-Forged", "Undying"]:
        AutoCriticalAnalyze(enemy)
```

**Tooltip:** "Architect of the Silence: WILL check. Fail = [Seized] 2 rounds. Pass = [Disoriented] 1 round. <50% HP = Auto-Seized (daily lockout). Jötun-Forged/Undying only. Cost: 60 Stamina, 10-15 Stress. 1/combat."

---

## Status Effect: [Seized]

| Property | Value |
|----------|-------|
| **Applied By** | Architect of the Silence |
| **Duration** | 2 rounds |
| **Icon** | Frozen gears / HALT symbol |
| **Color** | Blue/White |

**Effects:**
- Cannot take ANY actions (complete paralysis)
- Cannot move
- Cannot use abilities or reactions
- Defense -2 (vulnerable)
- Attacks against target gain +2 Accuracy

---

## Combat Log Examples

- "ARCHITECT OF THE SILENCE: 'PRIORITY OVERRIDE: TIWAZ PROTOCOL ALPHA'"
- "[Jötun-Forged Guardian] fails WILL check! [SEIZED] for 2 rounds - complete system lockdown!"
- "[Undying Sentinel] resists the override but is [Disoriented] for 1 round"
- "[Jötun-Forged at 35% HP] - Emergency override successful! [SEIZED] (ability locked until tomorrow)"
- "[PASSIVE] Combat start: All Jötun-Forged enemies auto-analyzed!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Jötun-Reader Overview](../jotun-reader-overview.md) | Parent specialization |
| [Enemy Types](../../../../03-combat/creature-traits.md) | Jötun-Forged/Undying definitions |
| [Disoriented](../../../../04-systems/status-effects/disoriented.md) | Fallback status effect |
