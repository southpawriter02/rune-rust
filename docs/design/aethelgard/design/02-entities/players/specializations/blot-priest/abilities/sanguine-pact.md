---
id: ABILITY-BLOT-PRIEST-30010
title: "Sanguine Pact"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Sanguine Pact

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None (enables HP spending) |
| **Tags** | [Sacrificial], [Resource], [Heretical] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

*"The Priest makes the initial, irreversible pact, learning to open their veins to the tainted Aether. Their blood is now a conduit for magic—a source of power that is both potent and perilous."*

This is the gateway passive—the core mechanic enabler that defines the Blót-Priest's sacrificial identity. Once learned, you can never go back. Your blood IS magic now.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Unlocks **Sacrificial Casting**: Spend HP instead of AP for any Mystic ability
- Conversion Rate: 2 HP per 1 AP
- Corruption Cost: +1 Corruption per HP-cast
- Limit: Cannot reduce HP below 1

**Formula:**
```
OnAbilityCast(Ability, PayWithHP):
    If PayWithHP AND Caster.HasAbility("SanguinePact"):
        HPCost = Ability.APCost × 2

        If Caster.HP - HPCost < 1:
            Log("Cannot sacrifice: Would reduce HP below 1")
            Return FAIL

        Caster.HP -= HPCost
        Caster.Corruption += 1

        Log("Sacrificial Cast: -{HPCost} HP, +1 Corruption")
        Return SUCCESS
```

**Tooltip:** "Sanguine Pact (Rank 1): Cast with HP (2:1 ratio). +1 Corruption per cast."

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Conversion Rate improved: 1.5 HP per 1 AP (rounded up)
- Corruption Cost: +1 per HP-cast

**Formula:**
```
OnAbilityCast(Ability, PayWithHP):
    If PayWithHP:
        HPCost = Ceiling(Ability.APCost × 1.5)

        If Caster.HP - HPCost < 1:
            Return FAIL

        Caster.HP -= HPCost
        Caster.Corruption += 1

        Log("Sacrificial Cast: -{HPCost} HP, +1 Corruption")
```

**Tooltip:** "Sanguine Pact (Rank 2): Cast with HP (1.5:1 ratio). +1 Corruption per cast."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Conversion Rate: 1 HP per 1 AP (perfect efficiency)
- **UPGRADE:** Corruption cost reduced to +0.5 per cast (rounds up after 2 casts)
- Can cast even at 1 HP (but Corruption still applies)

**Formula:**
```
OnAbilityCast(Ability, PayWithHP):
    If PayWithHP:
        HPCost = Ability.APCost  // 1:1 ratio

        If Caster.HP - HPCost < 1:
            // At Rank 3, can cast at 1 HP
            If Caster.HP == 1 AND HPCost <= 1:
                HPCost = Caster.HP - 1  // Minimum 0 HP cost at 1 HP
            Else:
                Return FAIL

        Caster.HP -= HPCost
        Caster.CorruptionFraction += 0.5

        If Caster.CorruptionFraction >= 1.0:
            Caster.Corruption += 1
            Caster.CorruptionFraction -= 1.0

        Log("Sacrificial Cast: -{HPCost} HP, +0.5 Corruption")
```

**Tooltip:** "Sanguine Pact (Rank 3): Cast with HP (1:1 ratio). +0.5 Corruption per cast."

---

## Conversion Rate Summary

| Rank | HP per AP | 35 AP Ability | Corruption |
|------|-----------|---------------|------------|
| 1 | 2:1 | 70 HP | +1 |
| 2 | 1.5:1 | 53 HP | +1 |
| 3 | 1:1 | 35 HP | +0.5 |

---

## Strategic Value

### Why Use HP Instead of AP?

| Scenario | Reason |
|----------|--------|
| AP depleted | Emergency casting when out of Aether |
| Burst healing | Massive output by spending HP reserves |
| Crimson Vigor active | When [Bloodied], healing is stronger anyway |
| Near sanctuary | Can recover HP more easily than AP |
| Critical moment | When victory matters more than survival |

### Risk Assessment

| Risk | Mitigation |
|------|------------|
| Death from HP spending | Cannot reduce below 1 (R1-R2) |
| Corruption accumulation | Inevitable—plan for it |
| Combat vulnerability | Use Blood Siphon to recover |
| Long-term Corruption spiral | This is the price of the Blót-Priest |

---

## Corruption Economics

**Per-Ability Corruption Cost:**

| Ability | AP Cost | R1 HP Cost | R3 HP Cost | Corruption |
|---------|---------|------------|------------|------------|
| Blood Siphon | 35 | 70 HP | 35 HP | +1 (R1) / +0.5 (R3) |
| Gift of Vitae | N/A (HP only) | 15% Max HP | 15% Max HP | Transfer only |
| Exsanguinate | 50 | 100 HP | 50 HP | +1 / +0.5 |

---

## The Irreversible Pact

Once you learn Sanguine Pact:
- You cannot unlearn it
- Your blood permanently responds to Aetheric resonance
- You will always feel the pull of Sacrificial Casting
- This is the first step on the path to becoming a conduit

---

## Combat Log Examples

- "Sanguine Pact: Sacrificial casting enabled"
- "Sacrificial Cast: -70 HP (Blood Siphon), +1 Corruption"
- "Sacrificial Cast: -35 HP (Rank 3 efficiency), +0.5 Corruption"
- "Cannot sacrifice: Would reduce HP below 1"
- "[Character] is at 1 HP—Rank 3 allows emergency casting"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Blót-Priest Overview](../blot-priest-overview.md) | Parent specialization |
| [Blood Siphon](blood-siphon.md) | Primary offensive ability |
| [Corruption Resource](../../../../01-core/resources/corruption.md) | Corruption mechanics |
