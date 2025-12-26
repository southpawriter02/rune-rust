# Health Pool (HP) System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-HP-v5.0
Parent item: Resource Systems — Core System Specification v5.0 (Resource%20Systems%20%E2%80%94%20Core%20System%20Specification%20v5%200%20341e0d234c7d4e0d9be146f9579c2bd2.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

HP measures **System Integrity**—the coherence of a character's physical form against Aethelgard's entropic pressure. Every point of damage is a file deletion; every point of healing is data recovery. At 0 HP, the character enters **[System Crashing]** state.

HP is the **survival metric** within Resource Systems. Slow regeneration and Corruption penalties make HP management a constant tactical and strategic concern.

---

## Calculation Formulas

### Max HP

```
Max HP = (50 + [STURDINESS × 10] + Flat Bonuses) × (1 + % Bonuses) × (1 - Corruption Penalty)
```

### Corruption Penalty

```
Corruption Penalty = MIN((Corruption ÷ 10) × 0.05, 0.50)
```

- 0 Corruption → 0% penalty
- 30 Corruption → -15% penalty
- 100+ Corruption → -50% penalty (capped)

### Bloodied Threshold

```
Bloodied Threshold = Max HP × 0.5
```

### System Lag Healing Penalty

```
Adjusted Heal = Base Heal × 0.75
```

Triggered when Psychic Stress ≥ 67%

---

## Trigger Events

| Trigger | Effect |
| --- | --- |
| **Damage Application** | Reduces Current HP |
| **Healing Application** | Increases Current HP (blocked if [System Crashing]) |
| **Corruption Change (10+)** | Recalculates Max HP |
| **Gear/Buff Changes** | Recalculates Max HP |
| **Sanctuary Rest** | Full HP restoration |
| **HP Threshold Crossing** | [Bloodied] at <50%, [System Crashing] at 0 |

---

## Integration Points

**Dependencies:** Attributes (STURDINESS), Trauma Economy (Corruption, Psychic Stress), Equipment ([Reinforced] bonuses), Status Effects

**Referenced By:** Combat System, Death & Resurrection, all damage/healing abilities, Environmental Systems