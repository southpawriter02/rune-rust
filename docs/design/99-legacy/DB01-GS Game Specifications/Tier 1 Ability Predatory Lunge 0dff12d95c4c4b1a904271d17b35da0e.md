# Tier 1 Ability: Predatory Lunge

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-VARGRBORN-PREDATORYLUNGE-v5.0
Mechanical Role: Damage Dealer, Utility/Versatility
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
| **Type** | Active (Mobility + Attack) |
| **Prerequisite** | Unlock Vargr-Born Specialization (10 PP) |
| **Cost** | 35 Stamina |
| **Effect** | Attack Back Row enemy from Front Row |
| **Fury Generation** | +10 Feral Fury |

---

## I. Design Context (Layer 4)

### Core Design Intent

Predatory Lunge is the Vargr-Born's **mobility attack**—the ability to bypass front-line defenders and strike at vulnerable back-row targets. This delivers the predator fantasy of leaping past obstacles to reach priority prey.

### Mechanical Role

- **Primary:** Attack Back Row enemy while positioned in Front Row
- **Secondary:** Generate Feral Fury on hit
- **Fantasy Delivery:** The explosive burst of predatory speed

### Balance Considerations

- **Power Level:** Moderate (positional utility + attack)
- **Back-Line Pressure:** Threatens healers, casters, ranged enemies
- **Lower Fury Generation:** 10 Fury vs Savage Claws' 15 (trade-off for mobility)
- **Stamina Efficient:** 35 cost is manageable for repeated use

---

## II. Narrative Context (Layer 2)

### In-World Framing

The enemy thinks they are safe. They stand in the back, behind their wall of shields and steel, confident that distance will protect them. They are wrong.

The Vargr-Born coils—and then **launches**. A blur of primal fury that sails over the front line, claws extended, eyes locked on the target. The impact is savage, unexpected, and often fatal. The prey never saw it coming.

### Thematic Resonance

Predatory Lunge is the Vargr-Born as apex predator—a creature that cannot be contained by formations or tactics. They hunt what they choose to hunt, and no wall of shields will stop them.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 35 Stamina
- **Range:** Front Row → Back Row (special)
- **Requirement:** Vargr-Born must be in Front Row

### Effect

**Positional Override:**

- Attack a single enemy in the **Back Row** while remaining in Front Row
- This attack ignores the normal melee range restriction

**Damage:** Deal unarmed melee damage (2d8 + MIGHT Physical)

### Fury Generation

- Generate **10 Feral Fury** on successful hit

### Resolution Pipeline

1. **Position Check:** Confirm Vargr-Born is in Front Row
2. **Target Selection:** Select enemy in Back Row
3. **Cost Payment:** Vargr-Born spends 35 Stamina
4. **Attack Roll:** MIGHT-based attack vs target Defense
5. **Damage Application:** On hit, deal 2d8 + MIGHT Physical
6. **Fury Generation:** Gain 10 Feral Fury
7. **Position Maintained:** Vargr-Born remains in Front Row

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Attack Back Row from Front Row
- 2d8 + MIGHT Physical damage
- Generates 10 Feral Fury
- Cost: 35 Stamina

### Rank 2 (Expert — 20 PP)

- 3d8 + MIGHT Physical damage
- Generates 15 Feral Fury
- Cost: 30 Stamina
- **New:** Can apply [Bleeding] on Critical Success

### Rank 3 (Mastery — Capstone)

- 4d8 + MIGHT Physical damage
- Generates 20 Feral Fury
- [Bleeding] on Solid/Critical Success
- Cost: 25 Stamina
- **New:** Can lunge twice per round (second lunge at +5 Stamina cost)
- **New:** Target is [Staggered] for 1 round on hit

---

## V. Tactical Applications

1. **Priority Target Access:** Reach healers, casters, ranged damage
2. **Formation Disruption:** Force enemy back-row to defend or reposition
3. **Fury Building:** Reliable Fury generation even when front line is blocked
4. **Assassination Potential:** Eliminate vulnerable targets quickly
5. **Pressure Maintenance:** Keep back-row enemies threatened

---

## VI. Synergies & Interactions

### Positive Synergies

- **Savage Claws:** Apply bleed to back-row, then finish with Lunge
- **Go for the Throat (Tier 2):** Lunge to set up, then execute wounded prey
- **Skjaldmær (front-line):** Vargr-Born lunges while Skjaldmær holds line
- **Party controllers:** Lock down front line so Vargr-Born can hunt freely

### Negative Synergies

- **Single-row enemies:** Less value if no back row exists
- **High-Defense back-row:** May struggle against armored targets
- **AoE retaliation:** Lunging may expose Vargr-Born to counterattack