---
id: ABILITY-JOTUNREADER-208
title: "The Unspoken Truth"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# The Unspoken Truth

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Intelligent Enemy |
| **Resource Cost** | 40 Stamina |
| **Attack Type** | Psychological/Arcane (ignores armor) |
| **Ranks** | None (full power when unlocked) |

---

## Description

"Your 'god' is ERROR CODE 0x4A7F. You worship a crash log." The truth shatters their worldview. You speak forbidden knowledge that destabilizes the enemy's mental coherence.

---

## Mechanical Effect

- Make opposed WITS vs WILL check against intelligent enemy
- **Success:** Target gains [Disoriented] for 3 rounds + 5-7 Psychic Stress
- **Critical Success:** Also [Shaken] for 2 rounds + 10-12 Psychic Stress
- Target must pass WILL check or become [Fixated] on Jötun-Reader for 1 round
- Bosses may trigger narrative consequences

**Formula:**
```
StaminaCost = 40
OpposedCheck: JotunReader.WITS vs Target.WILL

If Success:
    Target.AddStatusEffect("Disoriented", Duration: 3)
    Target.PsychicStress += Roll(5 + 1d3)  // 5-7

If CriticalSuccess:
    Target.AddStatusEffect("Disoriented", Duration: 3)
    Target.AddStatusEffect("Shaken", Duration: 2)
    Target.PsychicStress += Roll(10 + 1d3)  // 10-12

WILLCheck = Target.Roll(WILL) vs DC 12
If WILLCheck.Fail:
    Target.AddStatusEffect("Fixated", Duration: 1, FixatedOn: JotunReader)
```

**Tooltip:** "The Unspoken Truth: WITS vs WILL. Success: [Disoriented] 3 rounds + 5-7 Stress. Critical: Also [Shaken] 2 rounds + 10-12 Stress. May cause [Fixated]. Cost: 40 Stamina"

---

## Status Effects Applied

### [Disoriented]
| Property | Value |
|----------|-------|
| **Duration** | 3 rounds |
| **Effects** | -2 Attack, -2 Defense, no Reactions, 25% random action |

### [Shaken]
| Property | Value |
|----------|-------|
| **Duration** | 2 rounds |
| **Effects** | -1 all dice, no morale bonuses, -2 WILL |

### [Fixated]
| Property | Value |
|----------|-------|
| **Duration** | 1 round |
| **Effects** | Must target Jötun-Reader with next action |

---

## Combat Log Examples

- "The Unspoken Truth strikes [Enemy]'s psyche! [Disoriented] for 3 rounds, 6 Psychic Stress!"
- "CRITICAL! [Enemy] is [Disoriented], [Shaken], and takes 11 Psychic Stress!"
- "[Enemy] becomes [Fixated] on the Jötun-Reader!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Jötun-Reader Overview](../jotun-reader-overview.md) | Parent specialization |
| [Disoriented](../../../../04-systems/status-effects/disoriented.md) | Applied status effect |
