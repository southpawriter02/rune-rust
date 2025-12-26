# Tier 2 Ability: Guardian's Taunt

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKJALDMAER-GUARDIANSTAUNT-v5.0
Mechanical Role: Controller/Debuffer, Tank/Durability
Parent item: Skjaldmær (Bastion of Coherence) — Specialization Specification v5.0 (Skjaldm%C3%A6r%20(Bastion%20of%20Coherence)%20%E2%80%94%20Specialization%20%2083c338d903f54a5692dbaa63a5cf7b07.md)
Proof-of-Concept Flag: No
Resource System: Corruption/Psychic Stress, Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Medium
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Skjaldmær (Bastion of Coherence) |
| **Tier** | 2 (Advanced) |
| **Type** | Active (Taunt) |
| **Prerequisite** | 8 PP spent in Skjaldmær tree |
| **Cost** | 30 Stamina + 5 Psychic Stress (self) |
| **Target** | All enemies in Front Row |
| **Duration** | 2 rounds |

---

## I. Design Context (Layer 4)

### Core Design Intent

Guardian's Taunt is the Skjaldmær's **AoE aggro control ability**—the core tanking tool that forces enemies to attack her instead of allies. The self-inflicted Psychic Stress cost represents the mental toll of drawing the world's corruption to herself.

### Mechanical Role

- **Primary:** Force all Front Row enemies to target Skjaldmær for 2 rounds
- **Secondary:** Embodiment of selfless sacrifice (absorbs both physical and mental threat)
- **Fantasy Delivery:** The rallying cry of the bastion, a beacon of sanity drawing chaos toward itself

### Balance Considerations

- **Power Level:** High (AoE taunt is extremely powerful)
- **Self-Damage Trade-off:** Psychic Stress cost creates meaningful risk
- **Position Limitation:** Only affects Front Row enemies
- **Counterplay:** Some enemies may be taunt-immune

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skjaldmær bangs her weapon against her shield and **bellows a challenge**. It is not mere noise—it is a projection of pure, coherent will into the static of the battlefield. A signal of sanity so strong that even the most maddened creatures are drawn to it like moths to flame.

But such projection is taxing. Opening herself as a beacon means absorbing some of the world's ambient trauma. The Psychic Stress cost is the price of being seen.

### Thematic Resonance

The Skjaldmær doesn't just tank damage; she tanks *attention*. In a world where being noticed by the wrong things can shatter minds, she deliberately draws that attention away from allies. Her taunt is an act of controlled madness.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 30 Stamina + **5 Psychic Stress** (self-inflicted)
- **Range:** Front Row enemies (all)
- **Target:** All enemies currently in Front Row

### Effect

**All affected enemies gain [Taunted: Skjaldmær] for 2 rounds:**

- Must direct attacks and single-target abilities at Skjaldmær
- Cannot target other allies unless Skjaldmær is [Unconscious] or [Unreachable]

### Special Property: Effective Against Corrupted

- This taunt affects even creatures normally resistant to mental effects
- The projection of coherent will is "readable" by corrupted entities

### Resolution Pipeline

1. **Activation:** Skjaldmær spends 30 Stamina
2. **Stress Cost:** Skjaldmær immediately gains 5 Psychic Stress
3. **Target Identification:** System identifies all enemies in Front Row
4. **Taunt Application:** Each enemy gains [Taunted: Skjaldmær]
5. **Duration Track:** Effect lasts 2 rounds
6. **Termination Conditions:** Effect ends early if Skjaldmær becomes [Unconscious] or [Unreachable]

### Taunt Immunity

Some enemies may be immune to taunt effects:

- Mindless constructs without target prioritization
- Bosses with specific taunt immunity
- Enemies already locked onto specific targets by other mechanics

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Taunt all Front Row enemies for 2 rounds
- Cost: 30 Stamina + 5 Psychic Stress

### Rank 2 (Expert — 20 PP)

- Taunt all Front Row enemies for 3 rounds
- Cost: 25 Stamina + 3 Psychic Stress
- **New:** Taunted enemies deal -1 die damage to Skjaldmær

### Rank 3 (Mastery — Capstone)

- Taunt all enemies (any row) for 3 rounds
- Cost: 20 Stamina + 0 Psychic Stress
- **New:** Skjaldmær gains +2 Defense while enemies are [Taunted: Skjaldmær]
- **New:** When taunt ends, enemies suffer -1 die to their next action (disorientation)

---

## V. Tactical Applications

1. **Ally Protection:** Redirect all Front Row damage to self
2. **Fragile Ally Safety:** Create safe window for Jötun-Reader analysis or Bone-Setter healing
3. **Damage Consolidation:** All incoming damage on one well-armored target is easier to manage
4. **Corrupted Counter:** Works on enemies that resist most mental effects
5. **Formation Control:** Forces enemy targeting, enabling tactical positioning

---

## VI. Synergies & Interactions

### Positive Synergies

- **Sanctified Resolve I:** Helps resist self-inflicted Psychic Stress cost
- **Shield Wall:** Increased Soak while absorbing redirected attacks
- **Bone-Setter:** Can heal Skjaldmær's accumulated Psychic Stress
- **Skald (Saga of Courage):** Bonus to Stress Resolve helps manage taunt cost
- **High-Soak builds:** Consolidated damage is efficiently mitigated

### Negative Synergies

- **Low-WILL Skjaldmær:** Stress cost accumulates dangerously
- **Solo tanking without healer:** Stress can accumulate to dangerous levels
- **Back Row enemies:** Not affected by taunt (position-dependent)