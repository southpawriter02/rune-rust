---
id: ABILITY-BLOT-PRIEST-30017
title: "Martyr's Resolve"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Martyr's Resolve

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Trigger** | [Bloodied] status (<50% HP) |
| **Resource Cost** | None |
| **Tags** | [Bloodied], [Defensive], [Resolve] |
| **Ranks** | None (full power when unlocked) |

---

## Description

*"The Priest's acceptance of pain and corruption grants unnatural resilience. When on the verge of death, their corrupted soul hardens."*

The defensive complement to Crimson Vigor. While Crimson Vigor enhances your offensive output when Bloodied, Martyr's Resolve ensures you can survive in that dangerous state long enough to make use of it.

---

## Mechanical Effect

**When [Bloodied] (<50% HP):**
- Gain +5 Soak (damage reduction)
- Gain +2d10 to all Resolve checks against Psychic Stress
- These bonuses persist as long as you remain Bloodied

**Formula:**
```
OnBloodiedStateChange(IsNowBloodied):
    If IsNowBloodied:
        Caster.AddBuff("MartyrsResolve", {
            Soak: +5,
            ResolveBonus: "+2d10",
            Duration: "WhileBloodied"
        })
        Log("Martyr's Resolve: +5 Soak, +2d10 Resolve")
    Else:
        Caster.RemoveBuff("MartyrsResolve")
        Log("Martyr's Resolve: Deactivated (no longer Bloodied)")

OnResolveCheck(BaseCheck):
    If Caster.IsBloodied AND Caster.HasAbility("MartyrsResolve"):
        BonusDice = Roll(2d10)
        Return BaseCheck + BonusDice
    Return BaseCheck
```

**Tooltip:** "Martyr's Resolve: When Bloodied, +5 Soak, +2d10 Resolve vs Stress."

---

## Rank 2 Enhancement (via Capstone)

**Upgraded Effects:**
- Soak: +7
- Resolve: +3d10
- Duration: While Bloodied

---

## Rank 3 Enhancement (via Capstone)

**Mastered Effects:**
- Soak: +10
- Resolve: +4d10
- **NEW:** Immune to [Fear] while Bloodied

---

## Effect Summary

| Rank | Soak | Resolve Dice | Fear Immunity |
|------|------|--------------|---------------|
| Base | +5 | +2d10 | No |
| R2 | +7 | +3d10 | No |
| R3 | +10 | +4d10 | Yes |

---

## Soak Mechanics

**How Soak Works:**
- Reduces incoming damage from all sources
- Applied after damage roll, before HP loss
- Stacks with other Soak sources

**Example:**
```
Caster at 40% HP (Bloodied)
Martyr's Resolve: +5 Soak
Blood Ward (R3): +2 Soak (if active)
Total Soak: 7

Enemy attacks for 20 damage:
20 - 7 Soak = 13 HP lost
```

---

## Resolve Check Enhancement

**When resisting Psychic Stress:**

```
Base WILL: 6 dice
Martyr's Resolve (R3): +4d10
Total: 6 dice + 4d10 bonus

Against DC 3 Stress check:
  → More likely to resist
  → Less Stress accumulated
  → Safer to remain Bloodied
```

---

## Synergy with Crimson Vigor

The complete [Bloodied] package:

| Passive | Offensive Bonus | Defensive Bonus |
|---------|-----------------|-----------------|
| Crimson Vigor R3 | +100% healing, +60% siphon, +1 AP/turn | — |
| Martyr's Resolve R3 | — | +10 Soak, +4d10 Resolve, Fear immunity |

**Combined Effect:**
- You're MORE effective at healing/siphoning when low
- You're HARDER to kill when low
- You're RESISTANT to Stress when low
- You're IMMUNE to Fear when low

The Blót-Priest thrives on the edge of death.

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Taking damage | +10 Soak significantly reduces incoming |
| Horror effects | +4d10 to resist Stress buildup |
| Fear enemies | Immunity prevents [Feared] status |
| Sustained combat | Stay Bloodied for bonuses |

---

## Fear Immunity (Rank 3)

When Bloodied with R3 Martyr's Resolve:
- Cannot gain [Feared] status
- Fear-inducing abilities automatically fail
- Fear auras have no effect

**Narrative Implication:** You have accepted suffering so completely that fear holds no power over you.

---

## The Bloodied Threshold

| HP State | Martyr's Resolve Active? |
|----------|--------------------------|
| 51% HP | No |
| 50% HP | **YES** |
| 25% HP | **YES** |
| 1 HP | **YES** |

---

## Risk Management

### Why Stay Bloodied?

| Benefit | Value |
|---------|-------|
| Crimson Vigor | +100% healing output |
| Crimson Vigor | +60% siphon effectiveness |
| Crimson Vigor | +1 AP/turn |
| Martyr's Resolve | +10 Soak |
| Martyr's Resolve | +4d10 Resolve |
| Martyr's Resolve | Fear immunity |

### Why NOT Stay Bloodied?

| Risk | Consequence |
|------|-------------|
| Burst damage | Could die before healing |
| Critical threshold | Below 25% = death saves |
| No healing available | Can't recover if Blood Siphon on cooldown |

---

## Combat Log Examples

- "HP dropped below 50%—[Bloodied]!"
- "Martyr's Resolve: +10 Soak activated"
- "Martyr's Resolve: +4d10 Resolve vs Stress"
- "Martyr's Resolve: Immune to [Fear]!"
- "Enemy attacks for 25 → 15 after Soak"
- "Resolve check: 6 dice + 4d10 (Martyr's Resolve) = 4 successes"
- "[Fear] effect blocked! (Martyr's Resolve)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Blót-Priest Overview](../blot-priest-overview.md) | Parent specialization |
| [Crimson Vigor](crimson-vigor.md) | Offensive [Bloodied] synergy |
| [Blood Ward](blood-ward.md) | Additional protection |
| [Stress Resource](../../../../01-core/resources/stress.md) | Resolve mechanics |
