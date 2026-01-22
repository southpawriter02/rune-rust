# Tier 3 Ability: Brace for Charge

Type: Ability
Priority: Nice-to-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-ATGEIR-BRACEFORCHARGE-v5.0
Mechanical Role: Damage Dealer, Tank/Durability
Parent item: Atgeir-wielder (Formation Master) — Specialization Specification v5.0 (Atgeir-wielder%20(Formation%20Master)%20%E2%80%94%20Specialization%20432d149cac2a41cfad275a49efd9785b.md)
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
| **Specialization** | Atgeir-wielder (Formation Master) |
| **Tier** | 3 (Mastery of Formation Warfare) |
| **Type** | Active (Defensive Counter-Stance) |
| **Prerequisite** | 20 PP spent in Atgeir-wielder tree |
| **Cost** | 40 Stamina |
| **Duration** | 1 round (or until triggered) |
| **Effect** | +10 Soak vs next melee attack, immune to [Knocked Down], counter-damage |

---

## I. Design Context (Layer 4)

### Core Design Intent

Brace for Charge is the Atgeir-wielder's **signature anti-charge counter-stance**—the moment of perfect defensive preparation where the warrior sets their weapon to receive a charging enemy. The attacker impales themselves on their own momentum.

### Mechanical Role

- **Primary:** Massive Soak bonus (+10) against next melee attack
- **Secondary:** Immunity to [Knocked Down] from that attack
- **Tertiary:** Automatic counter-damage to attacker
- **Fantasy Delivery:** The unstoppable force meets the immovable object

### Balance Considerations

- **Power Level:** Very High (conditional)
- **Predictive Defense:** Must anticipate enemy attack
- **Single Use:** Consumed by first melee attack received
- **Polearm Requirement:** Must have [Polearm] equipped
- **Front Row Requirement:** Must be in Front Row

---

## II. Narrative Context (Layer 2)

### In-World Framing

The beast charges. A thousand pounds of corrupted muscle and blind fury, thundering toward the warrior like an avalanche of death. Others would flee. Others would try to dodge.

The Atgeir-wielder **plants their feet**.

The polearm's butt strikes the ground with finality. The point angles upward at precisely calculated trajectory. Every muscle locks. Every breath stills. In this moment, the warrior is not a person—they are a **geometric certainty**.

The beast hits. The beast *stops*. Impaled on its own momentum, its charge transformed from devastating assault into catastrophic self-inflicted wound.

### Thematic Resonance

Brace for Charge is the Atgeir-wielder's mastery of defensive timing—proof that the right preparation defeats overwhelming force.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 40 Stamina
- **Target:** Self
- **Prerequisites:**
    - [Polearm] weapon equipped
    - Must be in Front Row

### The [Braced] Status Effect

**Duration:** 1 round (or until consumed by melee attack)

**Trigger:** When Atgeir-wielder is hit by a melee attack

**Effects (on trigger):**

| Effect | Value |
| --- | --- |
| Soak Bonus | +10 Soak (this attack only) |
| Knockdown Immunity | Immune to [Knocked Down] from this attack |
| Counter-Damage | Attacker takes MIGHT + 2d10 Physical damage (automatic) |

### Stance Interaction

- Overwrites any active core stance
- Consumed after first melee attack received
- Expires unused at start of next turn

### Resolution Pipeline

1. **Validation:** Confirm Polearm equipped, in Front Row
2. **Cost Payment:** Spend 40 Stamina
3. **Status Application:** Apply [Braced] for 1 round
4. **Trigger Wait:** On melee attack received:
    - Apply +10 Soak to damage calculation
    - Block [Knocked Down] application
    - Apply counter-damage to attacker (no attack roll)
5. **Consumption:** Remove [Braced] status after trigger

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +10 Soak vs next melee attack
- Immune to [Knocked Down]
- Counter-damage: MIGHT + 2d10 Physical
- Cost: 40 Stamina

### Rank 2 (Expert — This is Tier 3's base form)

- As above (Tier 3 abilities start at "Expert" equivalent)

### Rank 3 (Mastery — Capstone)

- +12 Soak vs next melee attack
- Immune to [Knocked Down]
- Counter-damage: MIGHT + 3d10 Physical
- Cost: 35 Stamina
- **New:** Counter-damage also applies [Staggered] to attacker for 1 round
- **New:** If counter-damage exceeds 20, attacker is also [Knocked Down]

---

## V. Tactical Applications

1. **Charge Counter:** Devastating response to charging enemies
2. **Predictive Defense:** Reward for reading enemy intent
3. **Boss Fights:** Survive heavy single hits that would obliterate others
4. **Action Economy:** Turn enemy's action into your damage
5. **Capstone Synergy:** Living Fortress allows this as Reaction

---

## VI. Synergies & Interactions

### Positive Synergies

- **Living Fortress (Capstone):** Use as Reaction instead of Standard Action
- **Disciplined Stance:** Alternative defensive option for different situations
- **Guarding Presence:** Stacking defenses when needed
- **Versus charging enemies:** Maximum value vs Berserkr, beasts, etc.

### Negative Synergies

- **Ranged attacks:** Does not trigger on ranged damage
- **Multiple attackers:** Only affects first melee hit
- **Predictive failure:** Wasted if enemy doesn't attack you
- **AoE attacks:** Does not trigger on area damage