---
id: ABILITY-IRONBANE-1109
title: "Divine Purge"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Divine Purge

**Type:** Active | **Tier:** 4 (Capstone) | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Mechanical/Undying only) |
| **Resource Cost** | 60 Stamina + 75 Fervor |
| **Cooldown** | Once per combat |
| **Ranks** | 1 → 2 → 3 |
| **Special** | Training upgrades all Tier 1, 2, & 3 abilities to Rank 3 |

---

## Description

You channel every lesson, every moment of study, into one perfect strike. This is not combat. This is deletion.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- Damage: 10d10 Fire damage
- Target makes WILL save DC 18
- Failed save: Instant death
- Passed save: Double damage + [Stunned] 2 turns

**Formula:**
```
Requires: Target.Type == "Mechanical" OR Target.Type == "Undying"
Damage = Roll(10d10)
If (Target.WILLSave < 18):
    Target.HP = 0  // Instant death
Else:
    Damage *= 2
    Target.AddStatus("Stunned", Duration: 2)
```

**Tooltip:** "Divine Purge (Rank 1): 10d10 Fire. WILL DC 18: fail = death, pass = 2x damage + Stunned 2 turns. Mech/Undying only. Cost: 60 Stamina, 75 Fervor"

---

### Rank 2 (Unlocked: Based on tree progression)

**Effect:**
- Damage: 12d10 Fire
- Save DC: 20
- **NEW:** Destroyed enemies explode for 6d6 Fire AoE (adjacent enemies)

**Formula:**
```
Damage = Roll(12d10)
SaveDC = 20
OnTargetDeath:
    For each Adjacent Enemy:
        Enemy.TakeDamage(Roll(6d6), "Fire")
```

**Tooltip:** "Divine Purge (Rank 2): 12d10 Fire. DC 20. Destroyed enemies explode for 6d6 Fire AoE."

---

### Rank 3 (Unlocked: Full tree completion)

**Effect:**
- Damage: 15d10 Fire
- Save DC: 22
- Success still causes death (but target gets death save)
- **NEW:** Destruction causes [Feared] on all other Mechanical/Undying for 3 turns

**Formula:**
```
Damage = Roll(15d10)
SaveDC = 22
// Even on save, target dies but gets death save
OnTargetDeath:
    For each Adjacent Enemy:
        Enemy.TakeDamage(Roll(6d6), "Fire")
    For each Enemy where (Type == "Mechanical" OR Type == "Undying"):
        Enemy.AddStatus("Feared", Duration: 3)
```

**Tooltip:** "Divine Purge (Rank 3): 15d10 Fire. DC 22. Even saves require death save. All Mech/Undying [Feared] 3 turns on kill."

---

## GUI Display - Activation

```
┌─────────────────────────────────────────────┐
│            DIVINE PURGE                     │
├─────────────────────────────────────────────┤
│                                             │
│  "This is not combat. This is deletion."   │
│                                             │
│  Target: [Corrupted Automaton]              │
│  Damage: 15d10 Fire                         │
│  Save DC: 22 WILL                           │
│                                             │
│  Failure: INSTANT DEATH                     │
│  Success: Death Save Required               │
│                                             │
└─────────────────────────────────────────────┘
```

---

## Combat Log Examples

- "DIVINE PURGE: 'This is not combat. This is deletion.'"
- "[Corrupted Automaton] fails WILL save! INSTANT DEATH!"
- "[Undying Sentinel] passes WILL save but takes 124 Fire damage and is Stunned!"
- "Divine Purge (Rank 3): [Automaton] destroyed! All machines are [Feared]!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Iron-Bane Overview](../iron-bane-overview.md) | Parent specialization |
| [Feared](../../../../04-systems/status-effects/feared.md) | Applied status effect |
| [Stunned](../../../../04-systems/status-effects/stunned.md) | Fallback status effect |
