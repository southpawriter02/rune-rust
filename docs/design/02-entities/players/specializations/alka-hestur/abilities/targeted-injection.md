---
id: ABILITY-ALKA-HESTUR-29014
title: "Targeted Injection"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Targeted Injection

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 35 Stamina + 1 Payload Charge |
| **Cooldown** | 3 turns |
| **Damage Type** | Physical + Payload Element |
| **Attribute** | FINESSE |
| **Tags** | [Payload], [Armor Penetration], [Precision] |
| **Ranks** | 2 → 3 |

---

## Description

*"You aim for the weak point—the joint seal, the exposed conduit, the gap in the carapace."*

Basic payload delivery is effective. Targeted injection is surgical. You identify the structural vulnerability and drive your lance directly through it, bypassing armor entirely and delivering your payload where it will do maximum damage.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Deal 3d8 + FINESSE Physical damage
- **IGNORES SOAK** (armor penetration)
- Payload effect applied with +50% potency
- Cost: 35 Stamina + 1 Payload Charge
- Cooldown: 3 turns

**Formula:**
```
Caster.Stamina -= 35
Caster.PayloadCharges -= 1
LoadedPayload = Caster.Lance.CurrentPayload

// Base damage ignores armor
BaseDamage = Roll(3d8) + FINESSE
Target.TakeDamage(BaseDamage, "Physical", IgnoreSoak=True)

// Enhanced payload
PayloadDamage = Roll(1d6) * 1.5  // +50% potency
PayloadDuration = LoadedPayload.Duration * 1.5

Target.TakeDamage(PayloadDamage, LoadedPayload.Element)
Target.ApplyStatus(LoadedPayload.Status, PayloadDuration)

Log("Targeted Injection: {BaseDamage} (ignores Soak) + {PayloadDamage} {Element}!")
Log("[{Status}] at +50% potency!")
```

**Tooltip:** "Targeted Injection (Rank 2): 3d8+FINESSE ignores Soak. Payload +50% potency. CD: 3"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Deal 5d8 + FINESSE Physical damage (ignores Soak)
- Payload effect at +100% potency (doubled)
- **NEW:** Target is [Vulnerable] for 1 turn
- **NEW:** If target has vulnerability to payload element, deal TRIPLE damage instead of double
- Cost: 35 Stamina + 1 Payload Charge
- Cooldown: 3 turns

**Formula:**
```
Caster.Stamina -= 35
Caster.PayloadCharges -= 1
LoadedPayload = Caster.Lance.CurrentPayload

// Base damage ignores armor
BaseDamage = Roll(5d8) + FINESSE
Target.TakeDamage(BaseDamage, "Physical", IgnoreSoak=True)

// Check for vulnerability
PayloadElement = LoadedPayload.Element
If Target.IsVulnerableTo(PayloadElement):
    PayloadDamage = Roll(1d6) * 3  // TRIPLE damage
    Log("Vulnerability exploited! TRIPLE payload damage!")
Else:
    PayloadDamage = Roll(1d6) * 2  // Double potency

// Apply effects
Target.TakeDamage(PayloadDamage, PayloadElement)
Target.ApplyStatus(LoadedPayload.Status, LoadedPayload.Duration * 2)
Target.ApplyStatus("[Vulnerable]", 1)

Log("Targeted Injection: {TotalDamage} damage! Target is [Vulnerable]!")
```

**Tooltip:** "Targeted Injection (Rank 3): 5d8+FINESSE ignores Soak. +100% potency. [Vulnerable] 1 turn. Vulnerability = triple damage."

---

## Damage Comparison

### Base Damage (FINESSE +3)

| Rank | Dice | Average | With Soak 5 |
|------|------|---------|-------------|
| Payload Strike R3 | 4d8 | 18 + 3 = 21 | 16 after Soak |
| Targeted Injection R2 | 3d8 | 13.5 + 3 = 16.5 | 16.5 (ignores Soak) |
| Targeted Injection R3 | 5d8 | 22.5 + 3 = 25.5 | 25.5 (ignores Soak) |

### Payload Potency

| Rank | Potency | 1d6 Average | Effective |
|------|---------|-------------|-----------|
| Base | 100% | 3.5 | 3.5 |
| R2 | +50% | 3.5 | 5.25 |
| R3 | +100% | 3.5 | 7.0 |
| R3 + Vuln | +200% | 3.5 | 10.5 |

---

## Armor Penetration Value

**Against High-Armor Targets:**

| Target Soak | Payload Strike (4d8) | Targeted Injection (5d8) | Difference |
|-------------|----------------------|--------------------------|------------|
| Soak 2 | 19 damage | 25.5 damage | +6.5 |
| Soak 5 | 16 damage | 25.5 damage | +9.5 |
| Soak 8 | 13 damage | 25.5 damage | +12.5 |
| Soak 10 | 11 damage | 25.5 damage | +14.5 |

**Takeaway:** Targeted Injection becomes increasingly valuable against heavily armored enemies.

---

## [Vulnerable] Status Effect

Applied at Rank 3:
- Duration: 1 turn
- Effect: Target takes +50% damage from all sources
- Enables massive follow-up damage from party

---

## Optimal Targets

| Target Type | Why Targeted Injection |
|-------------|------------------------|
| Heavy armor (Soak 5+) | Soak ignored |
| Boss with resistances | Triple damage if vulnerable |
| Priority target | [Vulnerable] for party burst |
| Damage sponge | Maximum single-hit damage |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Armored elite | Bypass Soak entirely |
| Analyzed weakness | Triple damage on vulnerability |
| Burst window | Apply [Vulnerable] for party |
| Cooldown management | Save for high-value targets |

---

## Combo Example

```
Turn 1: Alchemical Analysis → Target vulnerable to Fire
Turn 2: Load Ignition, Targeted Injection → Triple Fire damage + [Vulnerable]
Turn 3: Party attacks vulnerable target for +50% damage
```

---

## Combat Log Examples

- "Targeted Injection: 28 damage (ignores Soak)!"
- "[Burning] applied at +100% potency (6 turns)!"
- "Vulnerability to Fire! TRIPLE payload damage!"
- "[Armored Sentinel] is now [Vulnerable] (1 turn)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](../alka-hestur-overview.md) | Parent specialization |
| [Payload Strike](payload-strike.md) | Base delivery comparison |
| [Alchemical Analysis I](alchemical-analysis-i.md) | Identify vulnerabilities |
