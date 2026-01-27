# Tier 1 Ability: Savage Claws

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-VARGRBORN-SAVAGECLAWS-v5.0
Mechanical Role: Damage Dealer
Parent item: Vargr-Born (Uncorrupted Predator) — Specialization Specification v5.0 (Vargr-Born%20(Uncorrupted%20Predator)%20%E2%80%94%20Specialization%203c9731930e1d4cef9c89565e7941ceac.md)
Proof-of-Concept Flag: No
Resource System: Charges/Uses, Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Vargr-Born (Uncorrupted Predator) |
| **Tier** | 1 (Foundational Ferocity) |
| **Type** | Active (Melee Attack) |
| **Prerequisite** | Unlock Vargr-Born Specialization (10 PP) |
| **Cost** | 40 Stamina |
| **Target** | Single enemy (melee range) |
| **Fury Generation** | +15 Feral Fury on hit |

---

## I. Design Context (Layer 4)

### Core Design Intent

Savage Claws is the Vargr-Born's **signature unarmed attack**—the core of their feral combat identity. This establishes their unique fighting style (claw-based unarmed) and introduces the [Bleeding] application that fuels their Fury economy.

### Mechanical Role

- **Primary:** Deal MIGHT-based unarmed melee damage
- **Secondary:** Apply [Bleeding] on Solid/Critical Success
- **Resource Generation:** Generate 15 Feral Fury on hit
- **Fantasy Delivery:** The primal savagery of the wolf tearing flesh

### Balance Considerations

- **Power Level:** Moderate (standard damage + conditional bleed)
- **Fury Engine:** Primary method of building Feral Fury resource
- **Bleed Synergy:** Sets up damage-over-time and Fury generation loop
- **Unarmed Focus:** Unique combat identity distinct from weapon users

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Vargr-Born's hands are weapons. Their nails have thickened into claws—not through corruption, but through the expression of their primal bloodline. When they attack, they do not punch or grapple. They **tear**.

The strike is blindingly fast—a blur of motion ending in raked flesh and spraying blood. The wounds are ragged, difficult to close, designed by evolution to bleed. The prey will weaken. The prey will slow. And then the Vargr-Born will finish what they started.

### Thematic Resonance

Savage Claws is the Vargr-Born rejecting civilization's weapons in favor of the tools nature provided. They are not soldiers with swords; they are predators with claws.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 40 Stamina
- **Range:** Melee (adjacent tile)
- **Attack Type:** MIGHT-based unarmed melee attack

### Effect

**Primary:** Deal unarmed melee damage (3d8 + MIGHT Physical)

**Conditional [Bleeding]:**

- On **Solid Success:** Apply [Bleeding] for 3 rounds
- On **Critical Success:** Apply [Bleeding] for 5 rounds + bonus damage

### [Bleeding] Effect

- 1d6 Physical damage at start of target's turn
- Duration: 3-5 rounds depending on success level
- Stacks with existing [Bleeding] (extends duration)

### Fury Generation

- Generate **15 Feral Fury** on successful hit
- Additional Fury from [Bleeding] via Taste for Blood (Tier 2)

### Resolution Pipeline

1. **Cost Payment:** Vargr-Born spends 40 Stamina
2. **Attack Roll:** MIGHT-based attack vs target Defense
3. **Damage Application:** On hit, deal 3d8 + MIGHT Physical
4. **Bleed Check:** On Solid/Critical, apply [Bleeding]
5. **Fury Generation:** Gain 15 Feral Fury

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- 3d8 + MIGHT Physical damage
- [Bleeding] on Solid/Critical Success
- Generates 15 Feral Fury
- Cost: 40 Stamina

### Rank 2 (Expert — 20 PP)

- 4d8 + MIGHT Physical damage
- [Bleeding] on any hit (not just Solid/Critical)
- Generates 20 Feral Fury
- Cost: 35 Stamina

### Rank 3 (Mastery — Capstone)

- 5d8 + MIGHT Physical damage
- [Bleeding] on any hit; Critical applies [Hemorrhaging] (cannot be healed)
- Generates 25 Feral Fury
- Cost: 30 Stamina
- **New:** Can attack twice if target is already [Bleeding] (second attack at -1 die)

---

## V. Tactical Applications

1. **Fury Generation:** Primary method of building Feral Fury
2. **Bleed Application:** Opens the wound for sustained damage
3. **Damage-Over-Time:** [Bleeding] chips away at target HP
4. **Setup Attack:** Prepares targets for Go for the Throat finisher
5. **Resource Loop:** Bleed + Taste for Blood = sustained Fury generation

---

## VI. Synergies & Interactions

### Positive Synergies

- **Taste for Blood (Tier 2):** Bleed ticks generate bonus Fury
- **Go for the Throat (Tier 2):** Increased crit chance vs [Bleeding] targets
- **Feral Maelstrom (Tier 3):** AoE bleed application enables multi-target Fury
- **Aspect of the Great Wolf (Capstone):** Savage Claws costs no Stamina

### Negative Synergies

- **Bleed-immune enemies:** Undying, constructs, elementals negate bleed
- **Armor-heavy targets:** Unarmed may struggle vs high Soak
- **Ranged enemies:** Must close to melee range