# Tier 2 Ability: Triage

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BONESETTER-TRIAGE-v5.0
Mechanical Role: Support/Healer
Parent item: Bone-Setter (Restorer of Coherence) — Specialization Specification v5.0 (Bone-Setter%20(Restorer%20of%20Coherence)%20%E2%80%94%20Specializati%20b254ea23b3664889b545c17166ce7e7f.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Bone-Setter (Restorer of Coherence) |
| **Tier** | 2 (Advanced Treatment) |
| **Type** | Passive |
| **Prerequisite** | 8 PP spent in Bone-Setter tree |
| **Cost** | None (Passive) |

---

## I. Design Context (Layer 4)

### Core Design Intent

Triage is the Bone-Setter's **efficiency passive**—a skill that rewards reactive healing by amplifying effectiveness on critically wounded allies. This creates tactical decisions about *when* to heal.

### Mechanical Role

- **Primary:** +25% healing on [Bloodied] allies (below 50% HP)
- **Secondary:** Rewards reactive play and efficient resource usage
- **Fantasy Delivery:** The grim expertise of the battlefield medic who knows to treat the worst wounds first

### Balance Considerations

- **Power Level:** High (multiplicative healing bonus)
- **Conditional:** Only applies to [Bloodied] targets
- **Decision-Making:** Creates tension between healing early vs. waiting for Triage bonus

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Bone-Setter's eyes sweep across the battlefield, assessing wounds with clinical detachment. Not all injuries are equal—a scratch can wait, but arterial bleeding cannot. Years of experience have taught them to prioritize: treat the dying first, the injured second, the scratched last. This focus on critical cases makes their interventions devastatingly effective when they're needed most.

### Thematic Resonance

Triage is the hardest skill in medicine—knowing who to save and who to let go. The Bone-Setter has mastered this grim calculus, and their expertise shows in how effectively they treat the most severe cases.

---

## III. Mechanical Specification (Layer 3)

### Effect

**All Bone-Setter healing abilities restore 25% more HP when used on an ally who is [Bloodied].**

### [Bloodied] Definition

- A character is [Bloodied] when their current HP is **below 50%** of their maximum HP
- This is a status marker, not a debuff

### Affected Abilities

- Mend Wound (Tier 1)
- Miracle Worker (Capstone)
- Any other Bone-Setter healing ability

### Calculation

**Final Healing = Base Healing × 1.25**

(Triage bonus is applied after all other modifiers)

### Resolution Pipeline

1. **Healing Initiation:** Bone-Setter uses healing ability on ally
2. **[Bloodied] Check:** System checks if target is below 50% HP
3. **Base Calculation:** Calculate healing from ability + WITS + other modifiers
4. **Triage Application:** If [Bloodied], multiply total by 1.25
5. **HP Restoration:** Apply final healing to target

### Worked Example

> **Without Triage:**
> 

> - Mend Wound with [Enhanced Poultice]: 3d8 (14) + WITS 14 = 28 HP
> 

> 
> 

> **With Triage ([Bloodied] target):**
> 

> - Base healing: 28 HP
> 

> - Triage bonus: 28 × 1.25 = **35 HP**
> 

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +25% healing on [Bloodied] allies

### Rank 2 (Expert — 20 PP)

- +30% healing on [Bloodied] allies
- **New:** [Bloodied] threshold increased to 60% HP (more allies qualify)

### Rank 3 (Mastery — Capstone)

- +40% healing on [Bloodied] allies
- [Bloodied] threshold increased to 60% HP
- **New:** Healing a [Bloodied] ally also grants them +1 Defense for 1 turn
- **New:** If target is below 25% HP ("Critical"), bonus increases to +60%

---

## V. Tactical Applications

1. **Efficient Healing:** Maximize value of limited healing resources
2. **Clutch Saves:** Most effective when party members are in danger
3. **Resource Conservation:** Wait for allies to drop before healing to maximize efficiency
4. **Berserkr Synergy:** Berserkr often operates [Bloodied]; perfect Triage target
5. **Decision Tension:** Balance early healing (safer) vs. late healing (more efficient)

---

## VI. Synergies & Interactions

### Positive Synergies

- **Mend Wound:** Primary healing ability directly boosted
- **Miracle Worker:** Capstone already requires [Bloodied]; Triage stacks
- **Berserkr (Death or Glory):** Wants to stay [Bloodied]; perfect healing target
- **Jötun-Reader (Calculated Triage):** Name similarity is thematic; bonuses stack
- **High-WITS builds:** Multiplicative bonus amplifies base healing

### Negative Synergies

- **Overhealing:** If allies rarely drop below 50%, passive is wasted
- **Risk-averse parties:** May heal too early to benefit
- **Preventative healing:** Proactive healing doesn't trigger Triage