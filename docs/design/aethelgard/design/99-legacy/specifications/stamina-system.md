# Stamina System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-STAMINA-v5.0
Parent item: Resource Systems — Core System Specification v5.0 (Resource%20Systems%20%E2%80%94%20Core%20System%20Specification%20v5%200%20341e0d234c7d4e0d9be146f9579c2bd2.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Stamina measures **immediate readiness**—the universal fuel for physical and mental exertion. Unlike HP (long-term integrity), Stamina represents the capacity to act *right now*.

Stamina regenerates rapidly (25% per turn) to maintain combat tempo. It is the **action currency** for Warriors, Skirmishers, and Adepts, while Mystics use it as a secondary resource.

**Key Distinction:** Stamina is **NOT penalized by Corruption** (unlike HP and AP).

---

## Calculation Formulas

### Max Stamina

```
Max Stamina = (50 + [STURDINESS × 5] + [FINESSE × 2] + Flat Bonuses) × (1 + % Bonuses)
```

**Floor:** Minimum 20 Stamina

### Passive Regeneration

```
Stamina Regen = (Max Stamina × 0.25) × Regen Multiplier
```

**Trigger:** Start of character's turn in combat

### Regen Multipliers

- [Exhausted]: ×0.50
- Baseline: ×1.00
- Formal Training: ×1.10
- Rousing Verse: ×1.25

---

## Action Stamina Costs

| Action Type | Typical Cost |
| --- | --- |
| Basic Attack | 5 Stamina |
| Movement (Standard) | 5 Stamina |
| Movement (Sprint) | 15 Stamina |
| Defensive Action | 10-15 Stamina |
| Tier 1 Ability | 10-15 Stamina |
| Tier 2 Ability | 20-30 Stamina |
| Tier 3 Ability | 40-50 Stamina |
| Capstone Ability | 60-80 Stamina |

---

## Zero Stamina State

**Can Still Do:** Basic Attack, Standard Move (covered by turn start regen)

**Cannot Do:** Special Abilities, Defensive Actions, Sprint/Climb

**Note:** Not unconscious—just limited in options until regeneration.

---

## Integration Points

**Dependencies:** Attributes (STURDINESS, FINESSE), Equipment ([Energizing] bonuses), Status Effects, Combat System

**Referenced By:** All Non-Mystic Specializations, Combat System, all Physical Abilities