# Tier 3 Ability: Systemic Collapse

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-MYRSTALKER-SYSTEMICCOLLAPSE-v5.0
Parent item: Myr-Stalker (Entropic Predator) (Myr-Stalker%20(Entropic%20Predator)%20288c265eb42447bda85d985ed17ea3be.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 3
Template Validated: No
Voice Validated: No

## Core Identity

**Systemic Collapse** is your single-target execution ability. You inject a concentrated payload of entropic toxins that cascades through the victim's system, multiplying the damage of all existing poisons into catastrophic organ failure.

---

## Mechanics

### Cost

35 Stamina | **Standard Action** | **Melee Attack**

### Effect

- FINESSE-based melee attack against [Poisoned] target
- **Toxin Cascade:** Consume all [Poisoned] stacks on target
- Deal **2d6 damage per stack consumed** (instant)
- Target gains **[Weakened]** for 2 turns (-2 to all attacks)

### Rank Progression

| Rank | PP Cost | Effect |
| --- | --- | --- |
| 1 | 5 | 2d6 per stack; [Weakened] 2 turns |
| 2 | 25 | 2d8 per stack; 30 Stamina cost |
| 3 | 40 | 3d6 per stack; [Weakened] 3 turns; half stacks remain |

### Damage Scaling

| Poison Stacks | Rank 1 | Rank 2 | Rank 3 |
| --- | --- | --- | --- |
| 2 | 4d6 (~14) | 4d8 (~18) | 6d6 (~21) |
| 3 | 6d6 (~21) | 6d8 (~27) | 9d6 (~31) |
| 5 (Max) | 10d6 (~35) | 10d8 (~45) | 15d6 (~52) |

---

## Tactical Applications

**Execution Combo:**

```jsx
Turns 1-3: Stack [Poisoned] to 5 via Venom-Laced Shiv
Turn 4: Systemic Collapse

Result: 10d6 burst (~35 damage) + [Weakened]
```

**Rank 3 Sustained Pressure:**

```jsx
Stack 5 [Poisoned] → Systemic Collapse
Rank 3: Half stacks remain (2-3 stacks)
Continue DoT + restack for second Collapse
```

**Synergies:**

- **Venom-Laced Shiv / Corrosive Haze:** Stack builders
- **Corruption Catalyst:** Amplifies poison before consumption
- **Pandemic Bloom:** Alternative AoE finisher

---

## Failure Modes

<aside>
⚠️

**Stack Consumption** — Consumes all stacks. Timing matters—use too early and you waste potential DoT.

</aside>

<aside>
⚠️

**Single Target Only** — Unlike Pandemic Bloom, this only affects one enemy.

</aside>

---

## Integration Notes

**Parent Specialization:** Myr-Stalker (Entropic Predator)

**Archetype:** Skirmisher

**Role:** Single-Target Burst Finisher

**Prerequisite:** Tier 2 (2 abilities)