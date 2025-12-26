# Tier 1 Ability: Oath of the Protector

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKJALDMAER-OATHPROTECTOR-v5.0
Mechanical Role: Support/Healer, Tank/Durability
Parent item: Skjaldmær (Bastion of Coherence) — Specialization Specification v5.0 (Skjaldm%C3%A6r%20(Bastion%20of%20Coherence)%20%E2%80%94%20Specialization%20%2083c338d903f54a5692dbaa63a5cf7b07.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Skjaldmær (Bastion of Coherence) |
| **Tier** | 1 (Foundational) |
| **Type** | Active (Buff) |
| **Prerequisite** | Unlock Skjaldmær Specialization (10 PP) |
| **Cost** | 35 Stamina |
| **Target** | Single ally |
| **Duration** | 2 turns |

---

## I. Design Context (Layer 4)

### Core Design Intent

Oath of the Protector is the Skjaldmær's **signature single-target protection ability**. It grants both physical defense (Soak) and mental defense (Resolve bonus), establishing the dual-protection identity that defines the specialization.

### Mechanical Role

- **Primary:** +2 Soak and +1 die to Psychic Stress Resolve Checks for one ally
- **Secondary:** Core single-target protection establishing unique tank identity
- **Fantasy Delivery:** The solemn vow of the shield-bearer to protect body and mind

### Balance Considerations

- **Power Level:** Moderate (targeted buff vs party-wide)
- **Duration Trade-off:** Short duration (2 turns) for significant effect
- **Action Economy:** Requires Standard Action, limiting offensive output

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skjaldmær speaks a solemn vow—not a Performance like the Skald's, but a **personal oath**. Her shield becomes a symbol of this promise, a visible manifestation of her will to protect. The target feels a warmth settle over them, a certainty that someone stands between them and harm.

### Thematic Resonance

In Aethelgard, protection is both physical and metaphysical. The Oath of the Protector acknowledges that shielding someone's body while their mind shatters is no protection at all. The Skjaldmær's oath covers both vulnerabilities.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 35 Stamina
- **Range:** 4 meters
- **Target:** Single ally (cannot target self)

### Effect

**Target ally gains for 2 turns:**

1. **Soak Bonus:** +2 Soak
2. **Resolve Bonus:** +1 bonus die (+1d10) to Resolve Checks against Psychic Stress

### Resolution Pipeline

1. **Targeting:** Skjaldmær selects ally within 4 meters
2. **Cost Payment:** Skjaldmær spends 35 Stamina
3. **Buff Application:** Target gains +2 Soak and +1d10 to Psychic Stress Resolve
4. **Duration Track:** Effect lasts 2 turns (ends at start of Skjaldmær's third turn)
5. **Expiration:** Remove both bonuses when duration ends

### Edge Cases

- **Self-Target:** Not allowed (Skjaldmær protects others, not herself)
- **Stacking:** Does not stack with itself; reapplication refreshes duration
- **Target Movement:** Effect persists regardless of target's movement
- **Skjaldmær [Unconscious]:** Effect persists; oath holds even when she falls

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +2 Soak, +1 die to Psychic Stress Resolve
- Duration: 2 turns
- Cost: 35 Stamina

### Rank 2 (Expert — 20 PP)

- +3 Soak, +2 dice to Psychic Stress Resolve
- Duration: 3 turns
- Cost: 30 Stamina
- **New:** If target takes damage while Oath is active, Skjaldmær gains 10 temporary HP

### Rank 3 (Mastery — Capstone)

- +4 Soak, +2 dice to Psychic Stress Resolve
- Duration: 4 turns
- Cost: 25 Stamina
- **New:** Can target 2 allies simultaneously
- **New:** If target would gain a Trauma while Oath is active, Skjaldmær can choose to take the Trauma instead

---

## V. Tactical Applications

1. **Fragile Ally Protection:** Shield Jötun-Reader or Bone-Setter during dangerous moments
2. **Stress Management:** Protect ally about to take large Psychic Stress hit
3. **Focus Fire Defense:** Apply to ally being targeted by multiple enemies
4. **Exploration Coverage:** Protect point character entering dangerous areas
5. **Combo with Aegis of the Clan:** Automatic free application when ally reaches High Stress

---

## VI. Synergies & Interactions

### Positive Synergies

- **Jötun-Reader:** Protects fragile analyst during high-Stress analysis
- **Berserkr:** Compensates for Berserkr's lack of Soak and low WILL
- **Skald (Saga of Courage):** Stacking mental protection creates near-immunity
- **Aegis of the Clan (Tier 3):** Can be auto-applied for free when ally hits High Stress

### Negative Synergies

- **Skjaldmær self-survival:** Cannot use on self; other abilities needed for personal defense
- **Party spread:** Short range (4m) requires positioning