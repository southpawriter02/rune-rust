---
id: ABILITY-HLEKKR-25018
title: "Master of Puppets"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Master of Puppets

**Type:** Hybrid (Passive + Active) (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive + Standard Action (Corruption Bomb) |
| **Target** | Self / Single High-Corruption Enemy |
| **Resource Cost** | 50 Stamina (Active only) |
| **Cooldown** | Once Per Combat (Active) |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |
| **Tags** | [Capstone], [Control], [Corruption] |
| **Ranks** | None (full power when unlocked) |

---

## Description

You have achieved complete mastery over the battlefield. Every enemy is a puppet to be moved at will. Those you displace become vulnerable, and the deeply corrupted can be turned into living bombs.

---

## Mechanical Effect

### Passive Component: Displacement Vulnerability

**Effect:**
- Whenever you Pull or Push an enemy, they become [Vulnerable] for 2 turns
- +2d10 bonus to all Pull/Push attempts

**Formula:**
```
OnPullOrPush(Target):
    Target.AddStatus("Vulnerable", Duration: 2)
    Log("Master of Puppets: {Target} is now [Vulnerable]!")

OnPullPushAttempt:
    BonusDice += 2  // d10s
```

### Active Component: Corruption Bomb

**Effect:**
- Target single enemy with High (60+) or Extreme (90+) Corruption
- Make opposed FINESSE check
- If successful: Trigger catastrophic feedback loop
- Explosion deals 10d10 Psychic damage to all OTHER enemies
- Target is NOT damaged (they are the bomb)

**Formula:**
```
If Target.Corruption < 60:
    Fail("Target must have 60+ Corruption")
    return

OpposedCheck = Caster.FINESSE vs Target.WILL

If OpposedCheck.Success:
    For each OtherEnemy (excluding Target):
        Damage = Roll(10d10)
        OtherEnemy.TakeDamage(Damage, "Psychic")
    Log("CORRUPTION BOMB! {Damage} Psychic damage to all enemies!")
Else:
    Log("Corruption Bomb: {Target} resists the feedback loop")
```

**Tooltip:** "Master of Puppets: Pull/Push = [Vulnerable] 2 turns. Corruption Bomb: 10d10 Psychic AoE (1×/combat). Cost: 50 Stamina"

---

## [Vulnerable] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 2 turns |
| **Effect** | +2 dice to all attacks against this target |
| **Source** | Any Pull or Push you perform |

---

## Corruption Bomb Details

| Property | Value |
|----------|-------|
| **Valid Targets** | 60+ Corruption enemies |
| **Check** | FINESSE vs WILL |
| **Damage** | 10d10 Psychic |
| **Average** | 55 damage |
| **Target** | All enemies EXCEPT the bomb |
| **Uses** | Once per combat |

---

## Combo Potential

**Maximum Setup:**
1. Netting Shot → [Rooted] + (corruption: [Slowed])
2. Grappling Hook → Pull + [Disoriented] + [Vulnerable]
3. Attack → Double damage (Punish) + Advantage (Vulnerable)
4. Corruption Bomb → 10d10 Psychic to all others

**Single Target Stack:**
- [Rooted] + [Disoriented] + [Vulnerable]
- Punish the Helpless: +100% damage
- [Vulnerable]: +2 dice to hit
- Advantage from control

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Every Pull/Push | Automatic [Vulnerable] setup |
| Formation control | Vulnerable targets wherever you want them |
| Group fights | Corruption Bomb devastates adds |
| Boss + adds | Turn corrupted add into bomb |

---

## Combat Log Examples

- "Master of Puppets: [Enemy] pulled and is now [Vulnerable]!"
- "+2d10 bonus to Pull attempt"
- "CORRUPTION BOMB activated on [Corrupted Elite]!"
- "10d10 Psychic damage to all 4 other enemies: 52, 61, 48, 57!"
- "[Corrupted Elite] was the bomb—unharmed"

---

## GUI Display - Capstone Notification

```
┌─────────────────────────────────────────────┐
│          MASTER OF PUPPETS!                 │
├─────────────────────────────────────────────┤
│                                             │
│  You see only pieces to be moved at will!   │
│                                             │
│  PASSIVE: Pull/Push = [Vulnerable]          │
│                                             │
│  ACTIVE: Corruption Bomb                    │
│  • Target high-corruption enemy             │
│  • 10d10 Psychic damage to all others       │
│                                             │
│  "Dance, puppets. Dance."                   │
│                                             │
└─────────────────────────────────────────────┘
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Hlekkr-Master Overview](../hlekkr-master-overview.md) | Parent specialization |
| [Grappling Hook Toss](grappling-hook-toss.md) | Pull source |
| [Concussive Pulse](../gorge-maw-ascetic/abilities/concussive-pulse.md) | Push comparison |
| [Punish the Helpless](punish-the-helpless.md) | Damage multiplier |
