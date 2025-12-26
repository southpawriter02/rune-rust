---
id: ABILITY-VARD-WARDEN-28017
title: "Aegis of Sanctity"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Aegis of Sanctity

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (triggered) |
| **Target** | Your Runic Barriers and Sanctified Ground zones |
| **Resource Cost** | None |
| **Tags** | [Construct], [Zone], [Enhancement], [Counter] |
| **Ranks** | None (full power when unlocked) |

---

## Description

Your wards transcend mere protection—they actively punish those who would assault them. Enemies striking your barriers find their attacks reflected back, while your sanctified ground burns corruption from those who stand within.

---

## Mechanical Effect

**Barrier Enhancement:**
- When an enemy attacks your Runic Barrier, they take 1d6 Arcane damage (reflection)
- This triggers on every attack against the barrier

**Zone Enhancement:**
- Sanctified Ground now removes one negative status effect from each ally at the start of their turn
- Cleansing occurs before healing

**Formula:**
```
// Barrier Reflection
OnBarrierAttacked(Attacker):
    ReflectDamage = Roll(1d6)
    Attacker.TakeDamage(ReflectDamage, "Arcane")
    Log("Aegis of Sanctity: Barrier reflects {ReflectDamage} Arcane damage!")

// Zone Cleansing
OnZoneTurnStart(Ally):
    If Ally.HasNegativeStatus():
        RemovedStatus = Ally.RemoveOldestNegativeStatus()
        Log("Sanctified Ground cleanses {RemovedStatus} from {Ally}")

    // Then normal zone healing proceeds
    ...
```

**Tooltip:** "Aegis of Sanctity: Barriers reflect 1d6 damage when attacked. Sanctified Ground removes one debuff from allies per turn."

---

## Effect Summary

| Enhancement | Effect |
|-------------|--------|
| Barrier Reflection | 1d6 Arcane per attack received |
| Zone Cleansing | Remove 1 negative status per ally/turn |

---

## Barrier Reflection Details

| Property | Value |
|----------|-------|
| Damage | 1d6 Arcane |
| Trigger | Each attack against barrier |
| Range | Melee attackers only |
| Cost | None (passive) |

**Example:**
- Enemy attacks barrier, deals 15 damage to barrier
- Barrier reflects 4 Arcane damage to attacker
- Net: Barrier -15 HP, Enemy -4 HP

---

## Zone Cleansing Details

**Status Effects Removed:**
- [Poisoned]
- [Bleeding]
- [Burning]
- [Slowed]
- [Disoriented]
- [Feared]
- [Corroded]
- Any other negative status

**Removal Priority:**
- Removes oldest active status first
- One removal per ally per turn
- Allies with multiple debuffs are cleansed over multiple turns

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Melee swarm | Punish enemies destroying barriers |
| DoT-heavy fight | Cleanse Bleeding/Poison from allies |
| Control effects | Remove Slowed/Disoriented |
| Fear enemies | Cleanse [Feared] from party |

---

## Enhanced Barrier Strategy

**Bait Attacks:**
1. Place barrier in enemy path
2. Force melee enemies to attack barrier
3. Each attack = 1d6 reflected damage
4. 5 attacks to destroy R3 barrier = ~17 total reflect damage

---

## Enhanced Zone Strategy

**Cleansing Priority:**
- Position debuffed allies in zone
- Zone cleanses before healing
- Multi-turn cleansing for heavily debuffed allies

---

## Combat Log Examples

- "Aegis of Sanctity: Barrier reflects 5 Arcane damage to [Enemy]!"
- "[Enemy] attacks Runic Barrier—takes 3 reflection damage"
- "Sanctified Ground cleanses [Poisoned] from [Ally]"
- "[Ally] enters zone with [Bleeding], [Slowed]—[Bleeding] cleansed"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Vard-Warden Overview](../vard-warden-overview.md) | Parent specialization |
| [Runic Barrier](runic-barrier.md) | Enhanced by reflection |
| [Consecrate Ground](consecrate-ground.md) | Enhanced by cleansing |
