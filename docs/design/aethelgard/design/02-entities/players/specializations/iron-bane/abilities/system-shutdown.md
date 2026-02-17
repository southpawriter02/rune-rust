---
id: ABILITY-IRONBANE-1104
title: "System Shutdown"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# System Shutdown

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Mechanical/Undying only) |
| **Resource Cost** | 40 Stamina + 30 Fervor |
| **Cooldown** | 3 turns |
| **Status Effects** | [Stunned], [System Malfunction] |
| **Ranks** | 2 → 3 |

---

## Description

You strike at the central processor, the corrupted core. Their systems crash. They stand frozen, helpless.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Damage: 4d10 Fire damage
- Target makes WILL save DC 17
- Failed save: [Stunned] for 2 turns
- **NEW:** Failed save also applies -3 to all actions for rest of combat
- Only affects Mechanical/Undying enemies

**Formula:**
```
Requires: Target.Type == "Mechanical" OR Target.Type == "Undying"
Damage = Roll(4d10)
If (Target.WILLSave < 17):
    Target.AddStatus("Stunned", Duration: 2)
    Target.AddStatus("SystemDamage", ActionPenalty: -3, Duration: COMBAT)
```

**Tooltip:** "System Shutdown (Rank 2): 4d10 Fire. WILL DC 17 or Stunned 2 turns + -3 actions (permanent). Mech/Undying only. Cost: 40 Stamina, 30 Fervor"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Damage: 5d10 Fire damage
- Stun duration: 3 turns
- **NEW:** Failed save adds [System Malfunction] (30% chance to skip turn each round)

**Formula:**
```
Damage = Roll(5d10)
StunDuration = 3
If (SaveFailed):
    Target.AddStatus("SystemMalfunction", SkipChance: 0.30, Duration: COMBAT)
```

**Tooltip:** "System Shutdown (Rank 3): 5d10 Fire. 3 turn Stun. [System Malfunction]: 30% skip turn."

---

## Status Effect: [System Malfunction]

| Property | Value |
|----------|-------|
| **Duration** | Rest of combat |
| **Icon** | Error symbol |
| **Effects** | 30% chance to skip turn each round |

---

## Combat Log Examples

- "System Shutdown deals 32 Fire damage! [Automaton] is Stunned for 2 turns!"
- "System Shutdown (Rank 3): [System Malfunction] applied! 30% chance to skip turn."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Iron-Bane Overview](../iron-bane-overview.md) | Parent specialization |
| [Stunned](../../../../04-systems/status-effects/stunned.md) | Applied status effect |
