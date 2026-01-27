---
id: ABILITY-MYRK-GENGR-24014
title: "Sensory Scramble"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Sensory Scramble

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Target Row |
| **Resource Cost** | 30 Stamina + 1 Alchemical Component |
| **Cooldown** | 4 turns |
| **Creates** | [Psychic Resonance] Zone |
| **Tags** | [Zone], [Debuff], [Support] |
| **Ranks** | 2 → 3 |

---

## Description

You shatter a dart of Blighted reagents, releasing powder that overloads senses with corrupted data. A temporary zone of pure psychic noise—chaos for them, camouflage for you.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Create [Psychic Resonance] zone in target row for 2 turns
- Enemies in zone: -1d10 to Perception checks
- You: +2d10 to Enter the Void checks while in zone
- Requires 1 Alchemical Component (consumed)

**Formula:**
```
Caster.Stamina -= 30
Caster.Inventory.Remove("Alchemical Component", 1)

CreateZone(
    Type: "PsychicResonance",
    Location: TargetRow,
    Duration: 2
)

// Zone effects
OnEnemyInZone:
    Enemy.PerceptionPenalty = -1d10

OnCasterInZone:
    EnterTheVoidBonus = +2d10
```

**Tooltip:** "Sensory Scramble (Rank 2): Create 2-turn Resonance zone. Enemies -1d10 Perception. You +2d10 stealth."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Duration increases to 4 turns
- Zone inflicts 1d6 Psychic Stress per turn to enemies within
- **NEW:** You can move through zone without requiring new stealth check
- All Rank 2 effects apply

**Formula:**
```
CreateZone(
    Type: "PsychicResonance",
    Location: TargetRow,
    Duration: 4
)

OnEnemyTurnInZone:
    Enemy.PsychicStress += Roll(1d6)

OnCasterMoveInZone:
    NoStealthCheckRequired = true
```

**Tooltip:** "Sensory Scramble (Rank 3): 4-turn zone. Enemies -1d10 + 1d6 Stress/turn. Free movement in zone."

---

## [Psychic Resonance] Zone

| Property | Rank 2 | Rank 3 |
|----------|--------|--------|
| Duration | 2 turns | 4 turns |
| Enemy Penalty | -1d10 Perception | -1d10 + 1d6 Stress/turn |
| Your Bonus | +2d10 Enter the Void | +2d10 + free movement |
| Area | Target Row | Target Row |

---

## Synergy with One with the Static

In a Psychic Resonance zone:
- One with the Static I: Additional +2d10 Stealth
- Sensory Scramble: +2d10 Enter the Void
- **Combined:** +4d10 to stealth checks in zone

At Rank 3 of both:
- Total stealth bonus in zone: +5d10
- Enemy detection penalty: -2d10
- DC to enter stealth: 12
- Result: Nearly automatic stealth success

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Pre-combat | Create zone before engagement |
| Re-stealth | Move into zone for easy Enter the Void |
| Stress pressure | Rank 3 zone damages enemy sanity |
| Escape route | Safe passage through zone |

---

## Combat Log Examples

- "Sensory Scramble: [Psychic Resonance] zone created in Back Row"
- "[Enemy] suffers -1d10 to Perception (Resonance zone)"
- "[Enemy] takes 4 Psychic Stress (Resonance zone DoT)"
- "Free movement in Resonance zone (Rank 3)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Myrk-Gengr Overview](../myrk-gengr-overview.md) | Parent specialization |
| [One with the Static I](one-with-the-static-i.md) | Zone synergy |
| [Enter the Void](enter-the-void.md) | Zone-enhanced stealth |
