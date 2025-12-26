---
id: ABILITY-ALKA-HESTUR-29012
title: "Field Preparation"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Field Preparation

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Extended Action (5-10 minutes) |
| **Target** | Self (crafting) |
| **Resource Cost** | 10 Stamina + Reagent Components |
| **Context** | Non-combat (rest/downtime) |
| **Tags** | [Crafting], [Sustainability], [Preparation] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

*"The bench is where battles are won."*

Every Alka-hestur knows that combat is merely the delivery phase. The true work happens before—grinding reagents, mixing solutions, pressurizing cartridges. This is your sustainability tool for long expeditions, ensuring you never fully run dry.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Prepare up to 4 payloads from raw materials during rest/downtime
- Preparation time: 10 minutes
- Requires appropriate reagent components
- Cost: 10 Stamina per batch

**Formula:**
```
// During rest or downtime
If Context != "Combat" AND Caster.HasReagents():
    Caster.Stamina -= 10

    For i = 1 to 4:
        If Caster.HasReagentFor(SelectedPayloadType):
            Caster.ConsumeReagent(SelectedPayloadType)
            Caster.PayloadCharges += 1
            Caster.AddPayloadToRack(SelectedPayloadType)

    Wait(10 minutes)
    Log("Field Preparation: {Count} payloads crafted")
```

**Tooltip:** "Field Preparation (Rank 1): Craft up to 4 payloads during rest. 10 minutes. Cost: 10 Stamina + reagents"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Prepare up to 6 payloads per rest
- **UPGRADE:** Preparation time reduced to 5 minutes
- Cost: 10 Stamina per batch

**Formula:**
```
If Context != "Combat" AND Caster.HasReagents():
    Caster.Stamina -= 10

    For i = 1 to 6:
        If Caster.HasReagentFor(SelectedPayloadType):
            Caster.ConsumeReagent(SelectedPayloadType)
            Caster.PayloadCharges += 1
            Caster.AddPayloadToRack(SelectedPayloadType)

    Wait(5 minutes)
    Log("Field Preparation: {Count} payloads crafted (5 min)")
```

**Tooltip:** "Field Preparation (Rank 2): Craft up to 6 payloads. 5 minutes."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Prepare up to 8 payloads per rest
- 5 minute preparation time
- **UPGRADE:** Can prepare during SHORT rests (not just long rests)
- **BONUS:** 20% chance for bonus payload per crafting batch

**Formula:**
```
If Context != "Combat" AND Caster.HasReagents():
    Caster.Stamina -= 10
    CraftedCount = 0

    For i = 1 to 8:
        If Caster.HasReagentFor(SelectedPayloadType):
            Caster.ConsumeReagent(SelectedPayloadType)
            Caster.PayloadCharges += 1
            Caster.AddPayloadToRack(SelectedPayloadType)
            CraftedCount += 1

    // Bonus payload chance
    If Random() <= 0.20:
        BonusType = RandomPayloadType()
        Caster.PayloadCharges += 1
        Caster.AddPayloadToRack(BonusType)
        CraftedCount += 1
        Log("Bonus payload! Extra {BonusType} crafted!")

    Wait(5 minutes)
    Log("Field Preparation: {CraftedCount} payloads crafted")
```

**Tooltip:** "Field Preparation (Rank 3): Craft up to 8 payloads. Works on short rests. 20% bonus payload chance."

---

## Crafting Capacity

| Rank | Max Payloads | Time | Short Rest? | Bonus Chance |
|------|--------------|------|-------------|--------------|
| 1 | 4 | 10 min | No | — |
| 2 | 6 | 5 min | No | — |
| 3 | 8 | 5 min | Yes | 20% |

---

## Reagent Requirements

| Payload Type | Reagent Components |
|--------------|-------------------|
| Ignition | Sulfur, Phosphorus, Oil |
| Cryo | Frost Salts, Distilled Water |
| EMP | Copper Wire, Charged Crystal |
| Acidic | Corrosive Extract, Glass Vial |
| Concussive | Compressed Air, Impact Powder |

**Reagent Acquisition:**
- Scavenging in ruins
- Purchasing from merchants
- Looting from defeated enemies
- Trading with other alchemists

---

## Rest Integration

### Long Rest
- Full Field Preparation available
- Can craft maximum payload count
- Stamina fully restored first

### Short Rest (Rank 3 only)
- Reduced preparation available
- Same payload count as long rest
- Enables mid-expedition resupply

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Pre-expedition | Max out payload rack |
| Mid-expedition | Resupply during short rest (R3) |
| Resource scarcity | Prioritize key payload types |
| Varied encounters | Prepare diverse payload mix |

---

## Recommended Loadouts

### Balanced (General Purpose)
- 2× Ignition
- 2× Cryo
- 2× Acidic
- 2× Concussive

### Anti-Mechanical
- 4× EMP
- 2× Acidic
- 2× Concussive

### Anti-Armor
- 4× Acidic
- 2× Ignition
- 2× Concussive

---

## Combat Log Examples

- "Field Preparation: 6 payloads crafted during rest"
- "Crafting: 2× Ignition, 2× Cryo, 2× Acidic"
- "Bonus payload! Extra Concussive crafted!"
- "Field Preparation: Insufficient reagents for Cryo"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](../alka-hestur-overview.md) | Parent specialization |
| [Rack Expansion](rack-expansion.md) | Increase carrying capacity |
| [Alchemy Crafting](../../../../04-systems/crafting/alchemy.md) | General alchemy rules |
