# Tier 1 Ability: Inspiring Word

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-THUL-INSPIRINGWORD-v5.0
Mechanical Role: Controller/Debuffer, Support/Healer
Parent item: Thul (Jötun-Reader Diagnostician) — Specialization Specification v5.0 (Thul%20(J%C3%B6tun-Reader%20Diagnostician)%20%E2%80%94%20Specialization%206740a2ac8e2a4a4fafa8694c56818d48.md)
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
| **Specialization** | Thul (Jötun-Reader Diagnostician) |
| **Tier** | 1 (Foundational Rhetoric) |
| **Type** | Active (Buff) |
| **Prerequisite** | Unlock Thul Specialization (10 PP) |
| **Cost** | 35 Stamina |
| **Target** | Single ally |
| **Duration** | 2 rounds (or until used) |

---

## I. Design Context (Layer 4)

### Core Design Intent

Inspiring Word is the Thul's **single-target accuracy buff**—a logical, tactical encouragement that enhances an ally's next attack. Unlike the Skald's emotional inspiration, this is cold, precise tactical guidance.

### Mechanical Role

- **Primary:** Grant ally +1 bonus die to next attack's Accuracy
- **Secondary:** Establishes Thul as offensive enabler, not just debuffer
- **Fantasy Delivery:** The strategic commander directing precise strikes

### Balance Considerations

- **Power Level:** Moderate (single-target, single-attack buff)
- **Duration:** 2 rounds or until attack is made
- **Comparison:** Less flashy than Skald's [Inspired], but more focused

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Thul doesn't shout encouragement like a bard—they deliver **tactical clarity**. A short, precise phrase that cuts through the chaos of battle:

"Strike now. Low sweep. It's off-balance."

"The joint. Second from the left. Don't hesitate."

"Wait... wait... NOW."

These aren't words of emotional support; they're words of logical perfection. The ally doesn't feel *inspired*—they feel *certain*.

### Thematic Resonance

The Thul's version of inspiration is fundamentally different from the Skald's. Where the Skald appeals to heroism and emotion, the Thul appeals to logic and precision. Both work—but they work differently.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 35 Stamina
- **Range:** 6 meters (voice range)
- **Target:** Single ally

### Effect

**Target ally gains [Tactically Guided] for 2 rounds:**

- Their **next attack** gains **+1 bonus die (+1d10)** to the Accuracy roll
- Effect is consumed when the attack is made
- If not used within 2 rounds, effect expires

### Resolution Pipeline

1. **Targeting:** Thul selects ally within 6 meters
2. **Cost Payment:** Thul spends 35 Stamina
3. **Buff Application:** Target gains [Tactically Guided]
4. **Duration Track:** Effect lasts 2 rounds or until attack is made
5. **Consumption:** On next attack, apply +1d10 to Accuracy, then remove buff

### Edge Cases

- **Multiple Attacks:** Only applies to first attack made
- **Stacking:** Does not stack with itself; reapplication refreshes
- **AoE Attacks:** Bonus applies to the entire AoE attack's accuracy roll
- **No Attack Made:** Effect expires unused after 2 rounds

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +1 bonus die to ally's next attack Accuracy
- Duration: 2 rounds or until used
- Cost: 35 Stamina

### Rank 2 (Expert — 20 PP)

- +2 bonus dice to ally's next attack Accuracy
- Duration: 3 rounds or until used
- Cost: 30 Stamina
- **New:** If the attack is a Critical Success, ally regains 10 Stamina

### Rank 3 (Mastery — Capstone)

- +2 bonus dice to ally's next attack Accuracy
- Duration: 4 rounds or until used
- Cost: 25 Stamina
- **New:** Can target 2 allies simultaneously
- **New:** If attack hits, target enemy also gets -1 die to their next action (disoriented by precise strike)

---

## V. Tactical Applications

1. **Critical Strike Setup:** Maximize accuracy on high-value attacks
2. **Ability Enhancement:** Boost accuracy on expensive abilities to ensure they hit
3. **Boss Damage:** Ensure party's biggest hits land against priority targets
4. **Berserkr Support:** Accuracy bonus compensates for Berserkr's accuracy penalties
5. **Coordinated Strikes:** Set up precision attacks for maximum efficiency

---

## VI. Synergies & Interactions

### Positive Synergies

- **Berserkr (Reckless Assault):** Compensates for accuracy penalty from recklessness
- **Veiðimaðr (Aimed Shot):** Stack accuracy for near-guaranteed crits
- **High-damage abilities:** Ensuring expensive attacks hit is extremely valuable
- **The Sage's Insight:** [Analyzed] + [Tactically Guided] = very accurate attacks

### Negative Synergies

- **Already-accurate attackers:** Diminishing returns on accuracy stacking
- **Multi-attack builds:** Only affects one attack
- **Action economy:** Standard Action for single buff is expensive