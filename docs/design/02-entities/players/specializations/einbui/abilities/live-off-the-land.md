---
id: ABILITY-EINBUI-27017
title: "Live off the Land"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Live off the Land

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self / Party |
| **Resource Cost** | None |
| **Tags** | [Survival], [Resource], [Party] |
| **Ranks** | None (full power when unlocked) |

---

## Description

The wasteland provides for those who know where to look. You need no rations when the land itself feeds you, and your knowledge extends to keeping your entire party sustained.

---

## Mechanical Effect

**Personal Sustenance:**
- You require no [Rations] during Wilderness Rest
- Automatically find 1d3/2d3/3d3 [Common Herbs] when foraging

**Party Benefit:**
- Reduce party [Ration] consumption by 25%/40%/50%
- At full power: No Ration consumption during Wilderness Rest if Einbui present

**Formula:**
```
// Personal sustenance
OnWildernessRest:
    Einbui.RationsRequired = 0

OnForage:
    HerbsFound = Roll(3d3)  // Full power
    Einbui.Inventory.Add("Common Herb", HerbsFound)

// Party benefit
OnPartyRationConsumption:
    If Einbui.InParty:
        RationCost = BaseRationCost * 0.5  // 50% reduction

OnWildernessRest:
    If Einbui.InParty:
        PartyRationsRequired = 0
```

**Tooltip:** "Live off the Land: No personal rations needed. Find 3d3 herbs when foraging. Party uses 50% fewer rations. Wilderness Rest requires no rations if Einbui present."

---

## Ration Economy Impact

### Standard Party (4 members, no Einbui)

| Rest Type | Rations Needed |
|-----------|----------------|
| Wilderness Rest | 4 |
| Extended Rest | 8 |
| Per day travel | 4 |

### Party with Einbui (Live off the Land)

| Rest Type | Rations Needed |
|-----------|----------------|
| Wilderness Rest | 0 |
| Extended Rest | 2 (50% of 3 other members) |
| Per day travel | 1.5 (50% of 3) |

---

## Herb Finding

| Rank | Herbs per Forage |
|------|------------------|
| 1 | 1d3 (avg 2) |
| 2 | 2d3 (avg 4) |
| 3 | 3d3 (avg 6) |

At full power: Reliable supply of 3-9 herbs per foraging attempt

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Deep wilderness | Extended expeditions possible |
| Supply shortage | Party survives without rations |
| Crafting support | Constant herb supply for concoctions |
| Emergency | Strip minimum supplies, travel light |

---

## Expedition Impact

**Without Einbui (7-day expedition):**
- 4 party members × 7 days = 28 rations needed
- Weight: 28 inventory slots

**With Einbui (7-day expedition):**
- Wilderness Rest: 0 rations
- Other consumption: 1.5 × 7 = ~11 rations
- Weight savings: 17 slots freed

---

## Combat Log Examples

- "Live off the Land: [Einbui] requires no rations for Wilderness Rest"
- "Foraging: Found 7 [Common Herbs] (Live off the Land)"
- "Party Ration consumption reduced by 50% (Einbui present)"
- "Wilderness Rest: No rations consumed (Einbui present)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Einbui Overview](../einbui-overview.md) | Parent specialization |
| [Persistence](../../../../01-core/persistence.md) | Rest mechanics |
| [Basic Concoction](basic-concoction.md) | Uses gathered herbs |
