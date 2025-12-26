---
id: ABILITY-ALKA-HESTUR-29011
title: "Payload Strike"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Payload Strike

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 25 Stamina + 1 Payload Charge |
| **Cooldown** | None |
| **Damage Type** | Physical + Payload Element |
| **Attribute** | FINESSE |
| **Tags** | [Payload], [Melee], [Status Application] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

*"You drive the lance home and trigger the injector. The payload enters the system. The reaction begins."*

This is the primary delivery mechanism for your alchemical arsenal. The lance punctures, the reservoir empties, and your carefully prepared payload courses through the target's system—with predictable, devastating results.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Deal 2d8 + FINESSE Physical damage (base)
- Apply loaded cartridge's elemental damage (+1d6)
- Apply loaded cartridge's status effect
- Cost: 25 Stamina + 1 Payload Charge

**Formula:**
```
Caster.Stamina -= 25
Caster.PayloadCharges -= 1
LoadedPayload = Caster.Lance.CurrentPayload

// Base damage
BaseDamage = Roll(2d8) + FINESSE

// Payload damage
PayloadDamage = Roll(1d6)
PayloadElement = LoadedPayload.Element

// Apply damage
Target.TakeDamage(BaseDamage, "Physical")
Target.TakeDamage(PayloadDamage, PayloadElement)

// Apply status
Target.ApplyStatus(LoadedPayload.Status, LoadedPayload.Duration)

Log("Payload Strike: {BaseDamage} Physical + {PayloadDamage} {PayloadElement}!")
Log("[{LoadedPayload.Status}] applied to {Target}")
```

**Tooltip:** "Payload Strike (Rank 1): 2d8+FINESSE + payload effect. Cost: 25 Stamina + 1 Payload"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Deal 3d8 + FINESSE Physical damage
- Payload damage: +1d6 elemental
- **UPGRADE:** Payload effects last +1 turn
- Cost: 25 Stamina + 1 Payload Charge

**Formula:**
```
Caster.Stamina -= 25
Caster.PayloadCharges -= 1
LoadedPayload = Caster.Lance.CurrentPayload

BaseDamage = Roll(3d8) + FINESSE
PayloadDamage = Roll(1d6)

Target.TakeDamage(BaseDamage, "Physical")
Target.TakeDamage(PayloadDamage, LoadedPayload.Element)

// Extended duration
ExtendedDuration = LoadedPayload.Duration + 1
Target.ApplyStatus(LoadedPayload.Status, ExtendedDuration)

Log("Payload Strike: {TotalDamage} damage, [{Status}] for {ExtendedDuration} turns")
```

**Tooltip:** "Payload Strike (Rank 2): 3d8+FINESSE + payload. Effects last +1 turn."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Deal 4d8 + FINESSE Physical damage
- Payload damage: +1d6 elemental
- Effects last +1 turn
- **NEW:** On critical hit, payload effect is DOUBLED (damage and duration)
- Cost: 25 Stamina + 1 Payload Charge

**Formula:**
```
Caster.Stamina -= 25
Caster.PayloadCharges -= 1
LoadedPayload = Caster.Lance.CurrentPayload

AttackRoll = Roll(1d100)
IsCritical = (AttackRoll >= 95)

BaseDamage = Roll(4d8) + FINESSE
PayloadDamage = Roll(1d6)

If IsCritical:
    PayloadDamage = PayloadDamage * 2
    Duration = (LoadedPayload.Duration + 1) * 2
    Log("CRITICAL PAYLOAD! Double effect!")
Else:
    Duration = LoadedPayload.Duration + 1

Target.TakeDamage(BaseDamage, "Physical")
Target.TakeDamage(PayloadDamage, LoadedPayload.Element)
Target.ApplyStatus(LoadedPayload.Status, Duration)
```

**Tooltip:** "Payload Strike (Rank 3): 4d8+FINESSE + payload. +1 turn duration. Critical = double payload effect."

---

## Damage Breakdown

| Rank | Base Dice | Payload Dice | Average (FINESSE +3) |
|------|-----------|--------------|----------------------|
| 1 | 2d8 | 1d6 | 9 + 3.5 + 3 = 15.5 |
| 2 | 3d8 | 1d6 | 13.5 + 3.5 + 3 = 20 |
| 3 | 4d8 | 1d6 | 18 + 3.5 + 3 = 24.5 |

---

## Payload Reference

| Payload | Element | Status | Base Duration |
|---------|---------|--------|---------------|
| Ignition | Fire | [Burning] | 3 turns |
| Cryo | Ice | [Slowed] | 2 turns |
| EMP | Energy | [System Shock] | 1 turn |
| Acidic | Physical | [Corroded] | 3 turns |
| Concussive | Physical | [Staggered] | 1 turn |

---

## Tactical Applications

| Scenario | Payload Choice |
|----------|----------------|
| High-armor target | Acidic ([Corroded] reduces Soak) |
| Fast enemy | Cryo ([Slowed] halves movement) |
| Mechanical enemy | EMP ([System Shock] skips turn) |
| Organic/cold-resistant | Ignition ([Burning] DoT) |
| Caster/charger | Concussive ([Staggered] disrupts) |

---

## Loading Requirements

**Before Using Payload Strike:**
1. Must have payload loaded in lance
2. Must have Payload Charges remaining
3. Lance must be equipped

**Loading a Payload:**
- Free Action (once per turn)
- Select from available payloads in rack

---

## Combat Log Examples

- "Payload Strike: 18 Physical + 4 Fire damage!"
- "[Burning] applied to [Blighted Husk] (4 turns)"
- "CRITICAL PAYLOAD! [Corroded] doubled! (6 turns, -4 Soak)"
- "Payload Strike fails—no payload loaded!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](../alka-hestur-overview.md) | Parent specialization |
| [Alchemical Lance Specification](../alchemical-lance-spec.md) | Weapon details |
| [Targeted Injection](targeted-injection.md) | Advanced delivery |
| [Cocktail Mixing](cocktail-mixing.md) | Combined payloads |
