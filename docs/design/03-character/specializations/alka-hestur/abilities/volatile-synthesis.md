---
id: ABILITY-ALKA-HESTUR-29017
title: "Volatile Synthesis"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Volatile Synthesis

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Bonus Action |
| **Target** | Self (emergency crafting) |
| **Resource Cost** | 20 Stamina + 3-5 Stress |
| **Cooldown** | Once per combat |
| **Tags** | [Crafting], [Emergency], [Stress], [Improvisation] |
| **Ranks** | None (full power when unlocked) |

---

## Description

*"In the press of combat, you cannot reach your bench. But you can improvise—dangerously, brilliantly."*

When your rack runs empty and the enemy still stands, a true Alka-hestur doesn't surrender—they synthesize. Drawing on ambient materials and sheer chemical intuition, you create payloads on the fly. The process is taxing, the results unstable, but sometimes improvisation is the only solution.

---

## Mechanical Effect

**Emergency Payload Synthesis:**
- Create 1 payload of any type from ambient materials (no reagent components required)
- Payload is **Unstable**: Must be used within 3 turns or degrades to nothing
- Costs Bonus Action
- Costs 20 Stamina + 5 Stress
- Once per combat limitation

**Formula:**
```
Caster.Stamina -= 20
Caster.Stress += 5

SelectedType = ChoosePayloadType()

UnstablePayload = CreatePayload({
    Type: SelectedType,
    Element: SelectedType.Element,
    Status: SelectedType.Status,
    Unstable: True,
    DegradeTimer: 3
})

Caster.PayloadCharges += 1
Caster.AddPayloadToRack(UnstablePayload)

Log("Volatile Synthesis: Unstable {SelectedType} created!")
Log("+5 Stress from volatile handling")
Log("WARNING: Payload degrades in 3 turns!")
```

**Tooltip:** "Volatile Synthesis: Create 1 unstable payload (any type). 3 turns to use. Cost: 20 Stamina + 5 Stress. Once/combat."

---

## Rank 2 Upgrade (via Capstone)

**Enhanced Synthesis:**
- Create 2 payloads per use
- Stress reduced to 3
- Payloads stable for 5 turns

**Formula:**
```
Caster.Stamina -= 20
Caster.Stress += 3

For i = 1 to 2:
    SelectedType = ChoosePayloadType()
    UnstablePayload = CreatePayload({
        Type: SelectedType,
        Unstable: True,
        DegradeTimer: 5
    })
    Caster.PayloadCharges += 1
    Caster.AddPayloadToRack(UnstablePayload)

Log("Volatile Synthesis: 2 payloads created! (5 turn stability)")
```

---

## Rank 3 Upgrade (via Capstone)

**Mastered Synthesis:**
- Create 3 payloads per use
- Stress reduced to 2
- Payloads are STABLE (no degradation)
- **NEW:** Can directly create Cocktails (counts as 2 payloads)

**Formula:**
```
Caster.Stamina -= 20
Caster.Stress += 2

CreateMode = Choose("Standard", "Cocktail")

If CreateMode == "Standard":
    For i = 1 to 3:
        Payload = CreatePayload({Type: ChooseType(), Unstable: False})
        Caster.AddPayloadToRack(Payload)
    Log("Volatile Synthesis: 3 stable payloads created!")

Else If CreateMode == "Cocktail":
    Cocktail = CreateCocktail({
        Components: [ChooseType(), ChooseType()],
        Unstable: False
    })
    Caster.AddPayloadToRack(Cocktail)

    ExtraPayload = CreatePayload({Type: ChooseType(), Unstable: False})
    Caster.AddPayloadToRack(ExtraPayload)
    Log("Volatile Synthesis: 1 Cocktail + 1 payload created!")
```

---

## Progression Summary

| Rank | Payloads | Stress | Stability | Special |
|------|----------|--------|-----------|---------|
| Base | 1 | 5 | 3 turns | — |
| R2 | 2 | 3 | 5 turns | — |
| R3 | 3 | 2 | Permanent | Can create Cocktails |

---

## Trauma Economy Note

**This is the ONLY Stress source in the Alka-hestur specialization.**

The Alka-hestur is a Coherent specialization—power derives from skill and preparation, not from channeling the Blight. Volatile Synthesis represents the inherent risk of field improvisation: working with volatile materials without proper equipment carries CPS-adjacent psychological stress.

**Stress Mitigation:**
- Rank 2 reduces Stress from 5 to 3
- Rank 3 reduces Stress from 3 to 2
- At Rank 3, Stress cost is minimal and manageable

---

## Unstable Payload Mechanics

**Degradation Timer:**
- Starts at 3 turns (base) or 5 turns (R2)
- Counts down at START of your turn
- When timer reaches 0, payload is destroyed
- Using the payload stops the timer

**Stable Payloads (R3):**
- No degradation timer
- Function identically to crafted payloads
- Can be saved for future combats

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Rack depleted | Emergency resupply mid-combat |
| Unexpected weakness | Create matching payload on demand |
| Long fight | Sustain damage output |
| Cocktail emergency | R3 instant cocktail creation |

---

## Optimal Usage

**When to Use:**
- Rack is empty or nearly empty
- Combat will last multiple more turns
- Specific payload type needed that you don't have
- Setting up for a big combo

**When NOT to Use:**
- Combat is almost over (don't waste Stress)
- You have plenty of charges remaining
- Stress is already high

---

## Combat Log Examples

- "Volatile Synthesis: Unstable Acidic payload created!"
- "+5 Stress from volatile handling (now 12/20)"
- "WARNING: Unstable payload degrades in 3 turns!"
- "Unstable payload degraded—lost 1 Acidic charge"
- "Volatile Synthesis (Mastered): 3 stable payloads created!"
- "Volatile Synthesis: Cocktail (Fire+Ice) created!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](../alka-hestur-overview.md) | Parent specialization |
| [Field Preparation](field-preparation.md) | Non-combat crafting |
| [Cocktail Mixing](cocktail-mixing.md) | R3 cocktail creation |
| [Stress Resource](../../../../01-core/resources/stress.md) | Stress mechanics |
