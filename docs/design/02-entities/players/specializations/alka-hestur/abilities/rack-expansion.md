---
id: ABILITY-ALKA-HESTUR-29013
title: "Rack Expansion"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Rack Expansion

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Tags** | [Capacity], [Equipment], [Utility] |
| **Ranks** | 2 → 3 |

---

## Description

*"More solutions means more answers."*

You've optimized your cartridge rack—additional mounting points, improved weight distribution, quick-release mechanisms. Every slot represents another problem you can solve without returning to your bench.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Increase payload carrying capacity by +2 (total: 6 base)
- Can carry up to 2 of the same payload type
- Improved organization reduces fumbling

**Formula:**
```
OnAbilityTrained:
    Caster.RackCapacity += 2  // 4 base + 2 = 6
    Caster.MaxSamePayloadType = 2
    Log("Rack Expansion: Capacity increased to 6")
```

**Tooltip:** "Rack Expansion (Rank 2): +2 payload capacity (6 total). Can carry 2 of same type."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Capacity +6 total (base 4 + 6 = 10 slots)
- Can carry up to 3 of the same payload type
- **NEW:** Quick-swap loaded payload as Bonus Action (once per turn)

**Formula:**
```
OnAbilityUpgraded:
    Caster.RackCapacity = 4 + 6  // 10 total
    Caster.MaxSamePayloadType = 3
    Caster.CanQuickSwap = True
    Log("Rack Expansion: Capacity increased to 10, Quick-swap enabled")

OnQuickSwap:
    If Caster.BonusActionAvailable AND Caster.CanQuickSwap:
        OldPayload = Caster.Lance.CurrentPayload
        NewPayload = SelectedPayloadFromRack
        Caster.Lance.CurrentPayload = NewPayload
        Caster.ReturnToRack(OldPayload)
        Caster.BonusActionAvailable = False
        Log("Quick-swap: {OldPayload} → {NewPayload}")
```

**Tooltip:** "Rack Expansion (Rank 3): +6 capacity (10 total). 3 of same type. Bonus Action quick-swap."

---

## Capacity Progression

| Stage | Base | Bonus | Total | Same Type Max |
|-------|------|-------|-------|---------------|
| No ability | 4 | — | 4 | 1 |
| Rank 2 | 4 | +2 | 6 | 2 |
| Rank 3 | 4 | +6 | 10 | 3 |

---

## Quick-Swap Mechanics (Rank 3)

**What It Does:**
- Change loaded payload without using Standard Action
- Costs Bonus Action (once per turn)
- Does not consume the swapped payload

**Why It Matters:**
- React to new threats mid-combat
- Switch from single-target to AoE payload
- Adapt when analysis reveals unexpected weakness

**Quick-Swap Flow:**
```
Turn 1: Load Ignition (Free), attack fire-resistant enemy, realize mistake
Turn 2: Quick-swap to Cryo (Bonus), Payload Strike (Standard) — success!
```

---

## Loading vs Quick-Swap

| Action | Type | When to Use |
|--------|------|-------------|
| Load Payload | Free Action | Start of turn, planned attack |
| Quick-Swap | Bonus Action | Mid-turn adaptation |
| Unload | Free Action | Clear lance for reload |

---

## Rack Organization Strategies

### Balanced Loadout (10 slots)
```
[Ignition] [Ignition] [Cryo] [Cryo] [EMP]
[EMP] [Acidic] [Acidic] [Concussive] [Concussive]
```

### Specialist Loadout (10 slots)
```
[Acidic] [Acidic] [Acidic] [EMP] [EMP]
[EMP] [Concussive] [Concussive] [Ignition] [Cryo]
```

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Long expedition | More payloads = less resupply |
| Varied enemies | Carry all payload types |
| Quick-swap | Adapt to revealed weaknesses |
| Cocktail prep | More ingredients available |

---

## Synergy with Other Abilities

| Ability | Synergy |
|---------|---------|
| Field Preparation | More capacity to fill during rest |
| Cocktail Mixing | More ingredients for combinations |
| Area Saturation | Carry 3 of same type for AoE |
| Alchemical Analysis | Quick-swap to exploit revealed weakness |

---

## Combat Log Examples

- "Rack Expansion: Capacity is now 10 payloads"
- "Quick-swap: Ignition → Cryo (Bonus Action)"
- "Payload rack: [Acidic×3, Cryo×2, EMP×3, Ignition×2]"
- "Cannot quick-swap—Bonus Action already used"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](../alka-hestur-overview.md) | Parent specialization |
| [Field Preparation](field-preparation.md) | Craft payloads to fill rack |
| [Alchemical Lance Specification](../alchemical-lance-spec.md) | Loading mechanics |
