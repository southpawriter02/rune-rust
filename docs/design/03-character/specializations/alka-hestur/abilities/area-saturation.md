---
id: ABILITY-ALKA-HESTUR-29016
title: "Area Saturation"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Area Saturation

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Area (3×3 to 5×5) |
| **Resource Cost** | 45 Stamina + 3 Payload Charges (same type) |
| **Cooldown** | 4 turns |
| **Damage Type** | Payload Element |
| **Attribute** | FINESSE |
| **Tags** | [Payload], [AoE], [Area Denial] |
| **Ranks** | None (full power when unlocked) |

---

## Description

*"Sometimes the answer isn't a precise injection—it's a cloud, a splash, a wave of reagent."*

You've mastered the art of area delivery. Rather than a single precise injection, you rupture the entire payload reservoir, saturating a wide area with your chosen chemical. Every enemy in the zone suffers the same fate.

---

## Mechanical Effect

**Area Payload Delivery:**
- Affect all enemies in target area
- Deal 4d8 elemental damage + apply payload status to ALL targets
- Requires 3 charges of the SAME payload type
- Area size: 3×3 tiles

**Formula:**
```
Caster.Stamina -= 45
Caster.PayloadCharges -= 3  // Must be same type
LoadedPayload = Caster.Lance.CurrentPayload

If NOT HasThreeOfSameType(LoadedPayload.Type):
    Log("Area Saturation requires 3 charges of same payload type!")
    Return FAIL

// Target all enemies in area
TargetArea = Select3x3Area()
AffectedEnemies = GetEnemiesInArea(TargetArea)

For Each Enemy in AffectedEnemies:
    Damage = Roll(4d8)
    Enemy.TakeDamage(Damage, LoadedPayload.Element)
    Enemy.ApplyStatus(LoadedPayload.Status, LoadedPayload.Duration)

Log("Area Saturation: {LoadedPayload.Type} saturates {AreaSize}!")
Log("{AffectedCount} enemies affected!")
```

**Tooltip:** "Area Saturation: 4d8 AoE + status to all enemies in 3×3. Costs 3 same-type payloads. CD: 4"

---

## Rank 2 Upgrade (via Capstone)

**Enhanced Effects:**
- Damage: 5d8 elemental
- Area: 4×4 tiles
- Effects last +1 turn
- Cost: 45 Stamina + 3 Payload Charges

---

## Rank 3 Upgrade (via Capstone)

**Mastered Effects:**
- Damage: 6d8 elemental
- Area: 5×5 tiles
- Effects last +1 turn
- **NEW:** Can use with Cocktail payloads (costs 3 of EACH type in cocktail)
- **NEW:** Enemies in center tile (epicenter) take +50% damage

**Formula (Rank 3):**
```
Caster.Stamina -= 45

// Check for cocktail usage
If UsingCocktail:
    For Each PayloadType in Cocktail:
        Caster.PayloadCharges -= 3  // 3 of each type
Else:
    Caster.PayloadCharges -= 3

TargetArea = Select5x5Area()
CenterTile = TargetArea.Center
AffectedEnemies = GetEnemiesInArea(TargetArea)

For Each Enemy in AffectedEnemies:
    Damage = Roll(6d8)

    // Epicenter bonus
    If Enemy.Position == CenterTile:
        Damage = Damage * 1.5
        Log("{Enemy} in epicenter! +50% damage!")

    Enemy.TakeDamage(Damage, PayloadElement)
    Enemy.ApplyStatus(PayloadStatus, Duration + 1)
```

---

## Area Scaling

| Rank | Area Size | Tiles | Max Enemies |
|------|-----------|-------|-------------|
| Base | 3×3 | 9 | 9 |
| R2 | 4×4 | 16 | 16 |
| R3 | 5×5 | 25 | 25 |

---

## Damage Output

| Rank | Dice | Average | Epicenter (R3) |
|------|------|---------|----------------|
| Base | 4d8 | 18 | — |
| R2 | 5d8 | 22.5 | — |
| R3 | 6d8 | 27 | 40.5 |

### Multi-Target Value

| Enemies Hit | Base Damage (4d8) | R3 Damage (6d8) |
|-------------|-------------------|-----------------|
| 1 | 18 | 27 |
| 3 | 54 | 81 |
| 5 | 90 | 135 |
| 9 | 162 | 243 |

---

## Payload Type Recommendations

| Payload | AoE Use Case |
|---------|--------------|
| Ignition | Clustered organics, area denial |
| Cryo | Slow enemy advance, kiting |
| EMP | Mechanical swarms |
| Acidic | Armored formations |
| Concussive | Disrupt caster groups |

---

## Cocktail AoE (Rank 3)

**Requirements:**
- 3 charges of EACH payload type in cocktail
- Example: Fire+Acid cocktail AoE = 3 Ignition + 3 Acidic = 6 charges

**Benefits:**
- Multiple status effects on all targets
- Synergy effects (e.g., [Melting]) apply to all
- Massive area control

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Enemy cluster | Maximum multi-target damage |
| Chokepoint | Area denial with status effects |
| Swarm control | Clear multiple weak enemies |
| Boss + adds | Damage boss while clearing minions |

---

## Positioning Strategies

### Optimal Placement
```
     [E] [E] [E]
     [E] [X] [E]    X = Epicenter (+50% damage at R3)
     [E] [E] [E]    E = Standard damage
```

### Chokepoint Denial
```
     [Wall] [E] [E] [E] [Wall]
            [E] [X] [E]
            [E] [E] [E]
```

---

## Combat Log Examples

- "Area Saturation: Ignition payload saturates 5×5 area!"
- "8 enemies caught in saturation zone!"
- "[Frost Giant] in epicenter! +50% damage! (40 Ice damage)"
- "[Burning] applied to all affected enemies (4 turns)"
- "Cocktail AoE: Fire+Acid! [Melting] applied to 6 targets!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](../alka-hestur-overview.md) | Parent specialization |
| [Cocktail Mixing](cocktail-mixing.md) | AoE cocktail synergy |
| [Rack Expansion](rack-expansion.md) | Carry enough for AoE |
| [Field Preparation](field-preparation.md) | Resupply after AoE |
