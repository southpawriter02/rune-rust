# Tier 3 Ability: Veil of Madness

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Mystic
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SEIDKONA-VEILMADNESS-v5.0
Mechanical Role: Controller/Debuffer
Parent item: Seiðkona (Psychic Archaeologist) — Specialization Specification v5.0 (Sei%C3%B0kona%20(Psychic%20Archaeologist)%20%E2%80%94%20Specialization%20%200efa3567b04046b2bdb9dfc23ed8160c.md)
Proof-of-Concept Flag: No
Resource System: Aether Pool, Corruption/Psychic Stress
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: High
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Seiðkona (Psychic Archaeologist) |
| **Tier** | 3 (Mastery of the Veil) |
| **Type** | Active (AoE Debuff) |
| **Prerequisite** | 20 PP spent in Seiðkona tree |
| **Cost** | 65 AP + Large Psychic Stress |
| **Target** | All enemies in 6m radius |
| **Duration** | 3 rounds |

---

## I. Design Context (Layer 4)

### Core Design Intent

Veil of Madness is the Seiðkona's **offensive control ability**—a wave of psychic terror that drowns enemies in the voices of the dead. This represents the darker side of their communion: weaponizing the grief and madness of the Forlorn.

### Mechanical Role

- **Primary:** Apply [Disoriented] to all enemies in area
- **Secondary:** Chance to inflict [Feared] on failed Resolve
- **Self-Cost:** Large Psychic Stress (channeling madness affects the channeler)
- **Fantasy Delivery:** Unleashing the screaming dead upon enemies

### Balance Considerations

- **Power Level:** Very high (AoE control)
- **Significant Self-Cost:** Large Psychic Stress creates real risk
- **Resolve Check:** Strong-willed enemies may resist [Feared]
- **Duration:** 3 rounds of disruption is substantial

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Seiðkona opens the veil—not to speak with the dead, but to let them **scream**.

The air fills with voices. Not words, but pure, unfiltered grief—the collective anguish of every spirit that died in pain, in fear, in rage. The sound is not heard with the ears but felt in the soul, a psychic pressure that crushes rational thought and replaces it with primal terror.

Enemies clutch their heads, stagger, some turn to flee. They are drowning in the voices of the dead, and the Seiðkona is the flood.

### Thematic Resonance

Veil of Madness is the Seiðkona as **psychic weapon**—proof that communion with the dead can be turned to terrible purpose. The spirits do not attack willingly; they simply exist in such torment that their presence alone is devastating.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 65 AP + **Large Psychic Stress** (18-22)
- **Range:** Self (centered on caster)
- **Area:** 6 meter radius

### Effect

**All enemies in area:**

| Effect | Application |
| --- | --- |
| **[Disoriented]** | Automatic; -2 dice to all checks for 3 rounds |
| **[Feared]** | Resolve Check; failed = [Feared] for 2 rounds |

### [Disoriented] Effect

- -2 dice to all checks (attacks, skills, Resolve)
- Duration: 3 rounds
- Does not stack with itself

### [Feared] Effect

- Must spend movement fleeing from Seiðkona
- Cannot willingly move closer
- -1 die to all checks (stacks with [Disoriented])
- Duration: 2 rounds

### Resolution Pipeline

1. **Activation:** Seiðkona spends 65 AP; gains Large Psychic Stress
2. **Area Determination:** Identify all enemies within 6m
3. **[Disoriented] Application:** All enemies gain [Disoriented] automatically
4. **Resolve Checks:** Each enemy rolls Resolve vs DC 14
5. **[Feared] Application:** Failed checks result in [Feared] for 2 rounds

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- [Disoriented] (-2 dice) for 3 rounds + Resolve check for [Feared]
- Cost: 65 AP + 20 Psychic Stress
- 6m radius

### Rank 2 (Expert — 20 PP)

- [Disoriented] (-3 dice) for 4 rounds + Resolve check for [Feared] (DC 15)
- Cost: 60 AP + 15 Psychic Stress
- 8m radius
- **New:** [Feared] duration increased to 3 rounds

### Rank 3 (Mastery — Capstone)

- [Disoriented] (-4 dice) for 5 rounds + Resolve check for [Feared] (DC 16)
- Cost: 55 AP + 10 Psychic Stress
- 10m radius
- [Feared] duration: 4 rounds
- **New:** Enemies who fail Resolve by 5+ are [Stunned] for 1 round
- **New:** Allies in area gain +2 dice to their next attack (spirits guide their strikes)

---

## V. Tactical Applications

1. **Crowd Control:** Disable multiple enemies simultaneously
2. **Positioning Disruption:** [Feared] enemies flee, breaking formations
3. **Accuracy Reduction:** [Disoriented] makes enemy attacks less dangerous
4. **Defensive Setup:** Buy time for party to reposition or heal
5. **Alpha Strike Enabler:** Debuffed enemies easier to hit

---

## VI. Synergies & Interactions

### Positive Synergies

- **Memory Siphon:** [Exposed] + [Disoriented] = severely weakened enemy
- **Spirit Bolt:** Follow up on disoriented targets
- **Party AoE:** Coordinate with other area attacks
- **Thul (Demoralizing Diatribe):** Stacking mental debuffs devastates enemy checks

### Negative Synergies

- **High-Resolve enemies:** May resist [Feared]
- **Mindless creatures:** Immune to fear effects
- **Self-Psychic Stress:** Large cost limits repeated use
- **Close range:** Must be near enemies to affect them