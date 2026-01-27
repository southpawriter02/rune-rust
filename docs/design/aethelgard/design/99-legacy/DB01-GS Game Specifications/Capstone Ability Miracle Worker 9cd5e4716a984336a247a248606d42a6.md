# Capstone Ability: Miracle Worker

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BONESETTER-MIRACLEWORKER-v5.0
Mechanical Role: Support/Healer
Parent item: Bone-Setter (Restorer of Coherence) — Specialization Specification v5.0 (Bone-Setter%20(Restorer%20of%20Coherence)%20%E2%80%94%20Specializati%20b254ea23b3664889b545c17166ce7e7f.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Capstone
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Bone-Setter (Restorer of Coherence) |
| **Tier** | Capstone (Ultimate Expression) |
| **Type** | Active (Massive Heal + Full Cleanse) |
| **Prerequisite** | 40 PP spent in Bone-Setter tree |
| **Cost** | Standard Action + 1 [Miracle Tincture] |
| **Target** | Single ally (must be [Bloodied]) |

---

## I. Design Context (Layer 4)

### Core Design Intent

Miracle Worker is the Bone-Setter's **ultimate save**—a capstone ability that delivers massive healing AND removes all negative physical status effects. This is the "turn the tide" moment that justifies investing heavily in the Bone-Setter tree.

### Mechanical Role

- **Primary:** Massive HP restoration (base + WITS × 3)
- **Primary:** Remove ALL negative physical status effects
- **Fantasy Delivery:** The impossible recovery—field surgery, stimulants, and sheer willpower

### Balance Considerations

- **Power Level:** Extremely high (massive heal + full cleanse)
- **[Bloodied] Requirement:** Cannot be used proactively; target must be in danger
- **Rare Consumable:** [Miracle Tincture] is difficult and expensive to craft
- **Once Per Combat Effective:** Limited by rare consumable availability

---

## II. Narrative Context (Layer 2)

### In-World Framing

The ally is broken—bones shattered, blood pooling, eyes glazing. Any reasonable assessment says they're beyond saving. But the Bone-Setter is not reasonable. They are **relentless**.

Hands move with desperate precision. A rare tincture—hoarded, irreplaceable—is administered. Splints are set by feel alone. Bleeding is stopped through pressure points and improvised sutures. The Bone-Setter's voice, steady and commanding, refuses to let the patient slip away.

"You don't get to die. Not today. Not on my watch."

And somehow, impossibly, the ally's eyes focus. They breathe. They *live*.

### Thematic Resonance

Miracle Worker is the Bone-Setter's defining moment—the proof that skill, preparation, and sheer refusal to accept defeat can overcome even the most grievous wounds.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Consumable Cost:** 1 [Miracle Tincture] from inventory
- **Range:** Touch (adjacent ally)
- **Target:** Single ally who is **[Bloodied]** (below 50% HP)

### Effect

**Dual Function:**

1. **Massive Healing:**
    - Base healing (determined by tincture quality) + **(WITS × 3)**
    - [Standard Miracle Tincture]: 4d10 base + WITS × 3
    - [Masterwork Miracle Tincture]: 6d10 base + WITS × 3
2. **Full Physical Cleanse:**
    - Removes ALL negative physical status effects from target:
        - [Bleeding]
        - [Poisoned]
        - [Crippled]
        - [Staggered]
        - [Knocked Down]
        - [Stunned]
        - Any other physical debuff

### [Miracle Tincture] Crafting

- **Difficulty:** Very High
- **Materials:** Rare reagents (expensive, scarce)
- **Crafting Location:** Alchemist's Lab required (not Campfire)
- **Enhanced by:** Field Medic I bonus die
- **Typical Availability:** 1-2 per expedition if prepared properly

### Resolution Pipeline

1. **Targeting:** Bone-Setter selects adjacent ally
2. **[Bloodied] Check:** Verify target is below 50% HP
3. **Consumable Check:** Verify [Miracle Tincture] in inventory
4. **Consumable Consumption:** Remove tincture from inventory
5. **Healing Calculation:** Roll base dice + (WITS × 3)
6. **Triage Check:** If [Bloodied] and Triage passive, apply 25% bonus
7. **HP Restoration:** Apply total healing to target
8. **Status Cleanse:** Remove all negative physical status effects

### Worked Example

> **Scenario:** Bone-Setter (WITS 14) uses [Standard Miracle Tincture] on [Bloodied] Berserkr
> 

> - Base roll: 4d10 = 26 HP
> 

> - WITS bonus: 14 × 3 = 42 HP
> 

> - Subtotal: 68 HP
> 

> - Triage bonus (25%): 68 × 1.25 = **85 HP**
> 

> - Status cleanse: Removes [Bleeding], [Staggered], [Crippled]
> 

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- 4d10 + (WITS × 3) healing on [Bloodied] ally
- Remove all negative physical status effects
- Consumes 1 [Miracle Tincture]

### Rank 2 (Expert — 20 PP)

- 5d10 + (WITS × 3.5, rounded down) healing
- Remove all negative physical status effects
- **New:** Target gains +2 Soak for 2 turns after healing
- **New:** Range increased to 2 meters

### Rank 3 (Mastery — Capstone)

- 6d10 + (WITS × 4) healing
- Remove all negative physical AND mental status effects
- Target gains +3 Soak for 3 turns
- Range increased to 4 meters
- **New:** If target would still be [Bloodied] after healing, they are instead restored to exactly 50% HP
- **New:** Can be used on target at 0 HP (dying state) to stabilize and heal

---

## V. Tactical Applications

1. **Ultimate Save:** Bring ally from near-death to full combat effectiveness
2. **Status Reset:** Remove all accumulated physical debuffs in one action
3. **Boss Fight Recovery:** Counter devastating boss attacks that leave allies crippled
4. **Berserkr Partnership:** Perfect for Berserkr who operates [Bloodied] and accumulates debuffs
5. **Expedition Insurance:** Having Miracle Tinctures available changes party's risk tolerance

---

## VI. Synergies & Interactions

### Positive Synergies

- **Triage (Tier 2):** [Bloodied] requirement means Triage always applies
- **High-WITS builds:** WITS × 3 multiplier makes WITS investment extremely valuable
- **"First, Do No Harm" (Tier 3):** Triggers +2 Defense after use
- **Berserkr:** Perfect synergy—Berserkr operates [Bloodied], accumulates status effects, Miracle Worker fixes everything
- **Jötun-Reader (Calculated Triage):** Additional +25% healing if positioned correctly

### Negative Synergies

- **No [Miracle Tincture]:** Cannot be used without rare consumable
- **Target not [Bloodied]:** Cannot use proactively
- **Preparation failure:** If Bone-Setter didn't craft during downtime, capstone is unavailable
- **Short expeditions:** May not have time to craft sufficient tinctures