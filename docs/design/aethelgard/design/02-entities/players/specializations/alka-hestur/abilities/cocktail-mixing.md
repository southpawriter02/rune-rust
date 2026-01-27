---
id: ABILITY-ALKA-HESTUR-29015
title: "Cocktail Mixing"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Cocktail Mixing

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (during load) |
| **Target** | Self (payload combination) |
| **Resource Cost** | 2-3 Payload Charges per Cocktail |
| **Tags** | [Payload], [Combination], [Chemistry] |
| **Ranks** | 2 → 3 |

---

## Description

*"Why use one reagent when two creates something new?"*

True alchemy isn't just about individual reactions—it's about synergy. You've learned to combine payloads in the injection chamber, creating volatile cocktails that deliver multiple effects in a single strike.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Can combine 2 payload types into a single Cocktail Payload
- Cocktail applies BOTH effects
- Consumes both payload charges
- Both elemental damages apply

**Formula:**
```
OnLoadCocktail:
    If Caster.HasAbility("CocktailMixing"):
        Payload1 = SelectPayload()
        Payload2 = SelectPayload()

        CocktailPayload = CreateCocktail({
            Elements: [Payload1.Element, Payload2.Element],
            Statuses: [Payload1.Status, Payload2.Status],
            Damage: Payload1.Damage + Payload2.Damage,
            ChargesCost: 2
        })

        Caster.PayloadCharges -= 2
        Caster.Lance.CurrentPayload = CocktailPayload
        Log("Cocktail loaded: {Payload1.Type} + {Payload2.Type}")
```

**Tooltip:** "Cocktail Mixing (Rank 2): Combine 2 payloads. Both effects apply. Costs 2 charges."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Cocktails deal +2d8 bonus damage from chemical reaction synergy
- **NEW:** Can create Triple Cocktails (3 payloads combined)
- **NEW:** Certain combinations create synergy effects (see table)
- Consumes all combined charges

**Formula:**
```
OnLoadCocktail:
    PayloadCount = SelectPayloadCount()  // 2 or 3

    Payloads = []
    For i = 1 to PayloadCount:
        Payloads.Add(SelectPayload())

    // Calculate synergy bonus
    SynergyBonus = GetSynergyBonus(Payloads)
    ReactionDamage = Roll(2d8)

    CocktailPayload = CreateCocktail({
        Elements: Payloads.Map(p => p.Element),
        Statuses: Payloads.Map(p => p.Status),
        Damage: Sum(Payloads.Map(p => p.Damage)) + ReactionDamage,
        SynergyEffect: SynergyBonus,
        ChargesCost: PayloadCount
    })

    Caster.PayloadCharges -= PayloadCount
    Caster.Lance.CurrentPayload = CocktailPayload
    Log("Cocktail loaded: {PayloadTypes} (+2d8 reaction damage)")
    If SynergyBonus:
        Log("Synergy effect: [{SynergyBonus}]!")
```

**Tooltip:** "Cocktail Mixing (Rank 3): Combine 2-3 payloads. +2d8 bonus damage. Special synergies."

---

## Cocktail Combinations (Rank 3)

### Two-Payload Synergies

| Combination | Synergy Effect |
|-------------|----------------|
| Fire + Acid | **[Melting]**: Double armor reduction (Soak -4) |
| Ice + Concussive | **[Shattered]**: Target has -2d10 to all actions |
| EMP + Acid | **[System Failure]**: Mechanical targets take +50% damage |
| Fire + Ice | **[Thermal Shock]**: 2d6 bonus damage, [Disoriented] |
| Concussive + EMP | **[Disrupted]**: Target loses next reaction |

### Triple Cocktail Synergies

| Combination | Synergy Effect |
|-------------|----------------|
| Fire + Acid + Concussive | **[Total Breakdown]**: All three statuses + [Vulnerable] |
| Ice + EMP + Acid | **[Frozen Cascade]**: AoE spread to adjacent enemies |
| Any 3 different | **[Chemical Storm]**: +4d8 bonus damage |

---

## Damage Calculation

### Standard Cocktail (2 payloads)

| Component | Damage |
|-----------|--------|
| Payload 1 | 1d6 (elemental) |
| Payload 2 | 1d6 (elemental) |
| Reaction (R3) | +2d8 |
| **Total Bonus** | 2d6 + 2d8 |

### Triple Cocktail (3 payloads)

| Component | Damage |
|-----------|--------|
| Payload 1 | 1d6 |
| Payload 2 | 1d6 |
| Payload 3 | 1d6 |
| Reaction (R3) | +2d8 |
| **Total Bonus** | 3d6 + 2d8 |

---

## When to Use Cocktails

| Scenario | Cocktail Choice |
|----------|-----------------|
| Armored + organic | Fire + Acid ([Melting]) |
| Fast + dangerous | Ice + Concussive ([Shattered]) |
| Mechanical boss | EMP + Acid ([System Failure]) |
| Maximizing damage | Any 3 ([Chemical Storm]) |

---

## Resource Economics

| Cocktail Type | Charges | Effects | Efficiency |
|---------------|---------|---------|------------|
| 2-payload | 2 | 2 statuses + synergy | High |
| 3-payload | 3 | 3 statuses + synergy | Very High |
| Single | 1 | 1 status | Baseline |

**Trade-off:** Cocktails consume more charges but deliver more value per strike.

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Single tough target | Maximum debuff stacking |
| Synergy exploitation | Combine for special effects |
| Burst damage | +2d8 reaction bonus |
| Status overload | Apply 2-3 statuses at once |

---

## Combat Log Examples

- "Cocktail loaded: Ignition + Acidic"
- "Cocktail Strike: Fire + Corrosive + 9 reaction damage!"
- "[Burning] and [Corroded] applied!"
- "Synergy effect: [Melting]! Soak reduced by 4!"
- "Triple Cocktail: [Chemical Storm] +4d8 bonus damage!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](../alka-hestur-overview.md) | Parent specialization |
| [Payload Strike](payload-strike.md) | Delivery mechanism |
| [Area Saturation](area-saturation.md) | Cocktails work with AoE |
| [Rack Expansion](rack-expansion.md) | Carry more ingredients |
