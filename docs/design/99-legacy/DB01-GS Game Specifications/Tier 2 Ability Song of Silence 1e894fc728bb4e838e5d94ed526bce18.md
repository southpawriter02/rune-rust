# Tier 2 Ability: Song of Silence

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKALD-SONGSILENCE-v5.0
Mechanical Role: Controller/Debuffer
Parent item: Skald (Chronicler of Coherence) — Specialization Specification v5.0 (Skald%20(Chronicler%20of%20Coherence)%20%E2%80%94%20Specialization%20S%203faadeffffc94a9fb7f3ce1e643ad740.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Skald (Chronicler of Coherence) |
| **Tier** | 2 (Advanced) |
| **Type** | Active (Targeted) |
| **Prerequisite** | 8 PP spent in Skald tree |
| **Cost** | 45 Stamina |
| **Target** | Single enemy caster |

---

## I. Design Context (Layer 4)

### Core Design Intent

Song of Silence is the Skald's **hard counter to enemy casters**. It inflicts the [Silenced] status, preventing verbal components of galdr and abilities. This positions the Skald as essential against Mystic-type enemies.

### Mechanical Role

- **Primary:** Caster shutdown via [Silenced] status
- **Secondary:** Interrupt enemy ability chains
- **Fantasy Delivery:** The counter-song that drowns out enemy incantations

### Balance Considerations

- **Power Level:** High (caster shutdown is extremely valuable)
- **Limitation:** Only affects abilities with verbal components
- **Counterplay:** Some abilities are gesture-only; some enemies have [Silence] immunity
- **Risk:** Requires Resolve Check; failure wastes resources

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skald speaks a **paradox-verse**—a string of words that cancel themselves out, creating a localized pocket of linguistic entropy. The target's vocal cords seize as their brain struggles to process the impossible syntax. No sound can form; no incantation can be completed.

### Thematic Resonance

Galdr requires precise pronunciation of the Futhark. The Song of Silence corrupts the verbal channel, introducing enough noise to make accurate runic pronunciation impossible. It's not magic—it's aggressive linguistic interference.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 45 Stamina
- **Range:** 8 meters (hearing range)
- **Target:** Single enemy

### Resolution

**Contested Check:**

- **Skald:** WILL + Rhetoric
- **Target:** WILL + Resolve
- **DC:** Target's Resolve Check result

### On Success

- Target gains [Silenced] status
- **Duration:** 2 rounds
- **[Silenced] Effect:** Cannot use abilities with verbal components (most galdr, Performance abilities, verbal commands)

### On Failure

- No effect
- Stamina cost is still expended
- Skald gains **5 Psychic Stress** (backlash from failed linguistic override)

### Resolution Pipeline

1. **Targeting:** Skald selects enemy within 8 meters
2. **Cost Payment:** Skald spends 45 Stamina
3. **Skald Check:** Roll WILL + Rhetoric
4. **Target Check:** Roll WILL + Resolve (or use static DC for NPCs)
5. **Comparison:** If Skald meets or exceeds target, apply [Silenced]
6. **Failure Penalty:** If Skald fails, apply 5 Psychic Stress to Skald
7. **Duration Track:** [Silenced] lasts 2 rounds from application

### Edge Cases

- **[Silence] Immune Targets:** Ability automatically fails (no Psychic Stress)
- **Already [Silenced]:** Refreshes duration
- **Non-verbal Abilities:** Not affected by [Silenced] status

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Inflict [Silenced] for 2 rounds on success
- Cost: 45 Stamina
- Failure penalty: 5 Psychic Stress

### Rank 2 (Expert — 20 PP)

- Inflict [Silenced] for 3 rounds on success
- Cost: 40 Stamina
- Failure penalty: 3 Psychic Stress
- **New:** On Critical Success, target also suffers -2 dice to next Resolve Check

### Rank 3 (Mastery — Capstone)

- Inflict [Silenced] for 4 rounds on success
- Cost: 35 Stamina
- No failure penalty
- **New:** Can target 2 adjacent enemies simultaneously
- **New:** [Silenced] duration cannot be reduced by enemy abilities

---

## V. Tactical Applications

1. **Caster Shutdown:** Silence enemy Seiðkona/Rust-Witch equivalents before they can act
2. **Interrupt Channeling:** Break enemy Performances or channeled galdr
3. **Boss Control:** Deny boss signature abilities with verbal components
4. **Forlorn Counter:** Many Forlorn abilities are verbal; Silence renders them nearly helpless
5. **Priority Targeting:** Identify and silence the most dangerous caster first

---

## VI. Synergies & Interactions

### Positive Synergies

- **High-Accuracy Attackers:** Silenced casters are easy targets
- **Jötun-Reader (Analyze Weakness):** Identify which enemies rely on verbal abilities
- **Interrupt-focused Builds:** Combined with Stun effects, locks down enemy action economy
- **Anti-caster Compositions:** Essential for parties lacking Mystic counter-options

### Negative Synergies

- **Physical-only Enemies:** Useless against beasts, constructs, melee-only humanoids
- **[Silence] Immune Enemies:** Some bosses and elites are immune
- **Low-WILL Skalds:** Contest becomes unfavorable