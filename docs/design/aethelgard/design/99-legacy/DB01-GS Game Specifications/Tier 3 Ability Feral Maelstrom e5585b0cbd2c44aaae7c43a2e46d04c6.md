# Tier 3 Ability: Feral Maelstrom

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-VARGRBORN-FERALMAELSTROM-v5.0
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
| **Tier** | 3 (Mastery of the Hunt) |
| **Type** | Active (AoE Attack) |
| **Prerequisite** | 20 PP spent in Vargr-Born tree |
| **Cost** | 55 Stamina + 40 Feral Fury |
| **Target** | All enemies in Front Row |
| **Effect** | AoE damage + high chance to apply [Bleeding] |

---

## I. Design Context (Layer 4)

### Core Design Intent

Feral Maelstrom is the Vargr-Born's **AoE damage ability**—a whirlwind of claws and fangs that tears through the entire front line. This expands their damage profile from single-target hunter to multi-target threat while maintaining their bleed identity.

### Mechanical Role

- **Primary:** Deal moderate Physical damage to all enemies in Front Row
- **Secondary:** High chance to apply [Bleeding] to all targets hit
- **Resource Cost:** Significant Stamina + Fury investment
- **Fantasy Delivery:** The wolf unleashed—pure, focused violence

### Balance Considerations

- **Power Level:** High (AoE damage + bleed spread)
- **High Resource Cost:** 55 Stamina + 40 Fury is substantial
- **Bleed Spread:** Enables Taste for Blood passive on multiple targets
- **Front Row Limitation:** Only affects front-line enemies

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Vargr-Born stops holding back.

They explode into motion—a **maelstrom** of primal violence, claws and fangs lashing out in every direction. There is no thought, no hesitation, only the pure expression of the predator's will to destroy. The front line of the enemy formation is torn apart in seconds, blood spraying across the battlefield.

When it ends, the Vargr-Born stands among the wounded and the dead, breathing hard, eyes wild with the joy of the hunt.

### Thematic Resonance

Feral Maelstrom is the Vargr-Born at maximum violence—the moment when the patient hunter becomes the unleashed storm. This is not finesse; this is overwhelming force.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 55 Stamina + 40 Feral Fury
- **Range:** Melee (Front Row)
- **Target:** All enemies in Front Row

### Effect

**For each enemy in Front Row:**

| Effect | Value |
| --- | --- |
| **Damage** | 4d8 + MIGHT Physical damage |
| **[Bleeding]** | 70% chance to apply [Bleeding] for 3 rounds |

### Resolution Pipeline

1. **Cost Payment:** Spend 55 Stamina + 40 Feral Fury
2. **Target Identification:** Identify all enemies in Front Row
3. **Attack Rolls:** Make single attack roll vs each target's Defense
4. **Damage Application:** On hit, deal 4d8 + MIGHT Physical per target
5. **Bleed Check:** 70% chance per target to apply [Bleeding]

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- 4d8 + MIGHT Physical damage to Front Row
- 70% chance to apply [Bleeding]
- Cost: 55 Stamina + 40 Feral Fury

### Rank 2 (Expert — 20 PP)

- 5d8 + MIGHT Physical damage to Front Row
- 85% chance to apply [Bleeding]
- Cost: 50 Stamina + 35 Feral Fury
- **New:** Enemies already [Bleeding] take +2d8 bonus damage

### Rank 3 (Mastery — Capstone)

- 6d8 + MIGHT Physical damage to Front Row
- 100% chance to apply [Bleeding] (guaranteed)
- Cost: 45 Stamina + 30 Feral Fury
- Bonus damage vs [Bleeding]: +3d8
- **New:** Can target BOTH rows (cost increases to 65 Stamina + 50 Fury)
- **New:** Critical hits apply [Hemorrhaging] instead of [Bleeding]

---

## V. Tactical Applications

1. **Bleed Spread:** Apply [Bleeding] to entire enemy front line
2. **Fury Farming:** Multiple bleeds = massive Taste for Blood Fury
3. **AoE Pressure:** Clear weak enemies; soften strong ones
4. **Setup Attack:** Prepare multiple targets for Go for the Throat
5. **Action Economy:** Single action affects multiple enemies

---

## VI. Synergies & Interactions

### Positive Synergies

- **Taste for Blood:** Multiple bleeds generate massive Fury
- **Go for the Throat:** Spread bleed, then execute priority targets
- **Wounded Animal's Ferocity (Tier 3):** Reduced cost when [Bloodied]
- **Aspect of the Great Wolf (Capstone):** Enables sustained AoE assault

### Negative Synergies

- **Bleed-immune enemies:** Loses secondary effect entirely
- **Single strong enemy:** Overkill; Go for the Throat more efficient
- **High Fury cost:** Competes with Terrifying Howl for Fury