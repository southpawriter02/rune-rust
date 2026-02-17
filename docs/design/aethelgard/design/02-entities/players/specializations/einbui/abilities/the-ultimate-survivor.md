---
id: ABILITY-EINBUI-27018
title: "The Ultimate Survivor"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# The Ultimate Survivor

**Type:** Passive + Active (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive + Standard Action (Blight Haven) |
| **Target** | Self / Cleared Room |
| **Resource Cost** | None |
| **Cooldown** | Blight Haven: Once per expedition |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |
| **Tags** | [Capstone], [Survival], [Party] |
| **Ranks** | None (full power when unlocked) |

---

## Description

You have mastered every skill needed to survive the wasteland. Where others specialize, you have become competent at everything—because the alternative was death. And now, you can create a haven where your party can truly rest.

---

## Mechanical Effect

### Passive: Universal Competence

**Effect:**
- +2 dice to ALL non-combat skill checks you're not proficient in
- Represents knowing "a little about everything"

**Formula:**
```
OnSkillCheck:
    If NOT Caster.IsProficient(Skill):
        If NOT Skill.IsCombat:
            BonusDice += 2
```

**Tooltip:** "Universal Competence: +2 dice to non-combat skill checks you're not proficient in."

---

### Active: Blight Haven

**Effect:**
- Designate any cleared room as [Hidden Camp]
- The [Hidden Camp] provides:
  - 0% Ambush Chance (guaranteed safe rest)
  - All Wilderness Rest benefits apply
  - +30 to all party recovery rolls
  - Protected from environmental Psychic Stress
  - Party can perform advanced crafting without station
- Usable ONCE per expedition

**Formula:**
```
OnBlightHavenActivate:
    If Room.IsCleared AND NOT UsedThisExpedition:
        Room.AddStatus("Hidden Camp")
        UsedThisExpedition = true

        // Hidden Camp effects
        Room.AmbushChance = 0
        Room.AllowWildernessRest = true
        Room.RecoveryBonus = 30
        Room.PsychicStressProtection = true
        Room.AllowAdvancedCrafting = true
```

**Tooltip:** "Blight Haven: Designate cleared room as [Hidden Camp]. 0% Ambush, +30 recovery, full rest benefits. 1×/expedition."

---

## [Hidden Camp] Status

| Property | Value |
|----------|-------|
| **Ambush Chance** | 0% |
| **Rest Type** | Full Wilderness Rest |
| **Recovery Bonus** | +30 to all recovery rolls |
| **Psychic Protection** | Immune to environmental Stress |
| **Crafting** | Advanced crafting allowed |
| **Duration** | Permanent for expedition |

---

## Universal Competence Examples

| Skill Check | Proficient? | Dice Pool |
|-------------|-------------|-----------|
| Lockpicking (not trained) | No | Base + 2 |
| Rhetoric (not trained) | No | Base + 2 |
| System Bypass (not trained) | No | Base + 2 |
| Wasteland Survival (trained) | Yes | Base (no bonus) |

---

## Recovery Roll Impact

**Without Blight Haven:**
- HP Recovery: 2d6 + CON
- Stress Recovery: 1d6 + WIL

**With Blight Haven (+30):**
- HP Recovery: 2d6 + CON + 30
- Stress Recovery: 1d6 + WIL + 30

This effectively guarantees maximum recovery for most characters.

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Deep dungeon | Establish forward base |
| Dangerous environment | Safe haven from hazards |
| Extended expedition | Full recovery point |
| Party injured | Guaranteed safe healing |

---

## Optimal Usage

1. **Clear a defensible room** in strategic location
2. **Activate Blight Haven** when party needs full recovery
3. **Rest safely** with guaranteed benefits
4. **Continue expedition** from fortified position

---

## Combat Log Examples

- "Universal Competence: +2 dice to Lockpicking (not proficient)"
- "BLIGHT HAVEN established in Collapsed Watchtower!"
- "[Hidden Camp]: 0% Ambush Chance, +30 Recovery, Safe Rest"
- "Party rests in Blight Haven: Full HP and Stress recovery guaranteed"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Einbui Overview](../einbui-overview.md) | Parent specialization |
| [Persistence](../../../../01-core/persistence.md) | Rest and recovery |
| [Stress System](../../../../01-core/resources/stress.md) | Psychic Stress mechanics |
