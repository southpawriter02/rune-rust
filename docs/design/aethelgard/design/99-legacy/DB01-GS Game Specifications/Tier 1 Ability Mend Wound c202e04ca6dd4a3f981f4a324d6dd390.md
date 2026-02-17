# Tier 1 Ability: Mend Wound

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BONESETTER-MENDWOUND-v5.0
Mechanical Role: Support/Healer
Parent item: Bone-Setter (Restorer of Coherence) — Specialization Specification v5.0 (Bone-Setter%20(Restorer%20of%20Coherence)%20%E2%80%94%20Specializati%20b254ea23b3664889b545c17166ce7e7f.md)
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
| **Specialization** | Bone-Setter (Restorer of Coherence) |
| **Tier** | 1 (Foundational Medicine) |
| **Type** | Active (Healing) |
| **Prerequisite** | Unlock Bone-Setter Specialization (10 PP) |
| **Cost** | Standard Action + 1 [Healing Poultice] |
| **Target** | Single ally |

---

## I. Design Context (Layer 4)

### Core Design Intent

Mend Wound is the Bone-Setter's **core healing ability**—a reliable, resource-consuming heal that establishes the preparation-based gameplay loop. Effectiveness scales with both consumable quality AND the Bone-Setter's WITS attribute.

### Mechanical Role

- **Primary:** Single-target healing that consumes [Healing Poultice]
- **Secondary:** Creates meaningful resource management decisions
- **Fantasy Delivery:** The skilled medic efficiently applying field dressings

### Balance Considerations

- **Power Level:** Moderate healing (consumable quality + WITS bonus)
- **Resource Gating:** Requires consumable, cannot spam infinitely
- **Scaling:** WITS bonus rewards attribute investment

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Bone-Setter kneels beside the wounded, hands steady despite the chaos. Clean the wound, apply pressure, dress it properly—the fundamentals that separate survival from bleeding out in a ditch. Every motion is practiced, efficient, wasting nothing.

### Thematic Resonance

Healing in Aethelgard is not magic—it's skill. The Bone-Setter's hands are their tools, and their knowledge is what transforms a simple poultice into a life-saving intervention.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Consumable Cost:** 1 [Healing Poultice] from inventory
- **Range:** Touch (adjacent ally)
- **Target:** Single ally (can target self)

### Effect

**Healing Formula:**

- **Base Healing:** Determined by poultice quality
    - [Standard Healing Poultice]: 2d8 HP
    - [Enhanced Healing Poultice]: 3d8 HP
    - [Masterwork Healing Poultice]: 4d8 HP
- **WITS Bonus:** + Bone-Setter's WITS score (flat addition)

**Total Healing = Poultice Base Roll + WITS**

### Resolution Pipeline

1. **Targeting:** Bone-Setter selects adjacent ally (or self)
2. **Consumable Check:** Verify [Healing Poultice] in inventory
3. **Consumable Consumption:** Remove poultice from inventory
4. **Healing Roll:** Roll dice based on poultice quality
5. **WITS Addition:** Add Bone-Setter's WITS score to roll
6. **HP Restoration:** Target gains HP equal to total
7. **Overheal:** HP cannot exceed maximum

### Worked Example

> **Scenario:** Bone-Setter (WITS 14) uses [Enhanced Healing Poultice] on Berserkr
> 

> - Poultice roll: 3d8 = 15 HP base
> 

> - WITS bonus: +14 HP
> 

> - **Total healing: 29 HP**
> 

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Healing = Poultice roll + WITS
- Single target, touch range
- Consumes 1 [Healing Poultice]

### Rank 2 (Expert — 20 PP)

- Healing = Poultice roll + (WITS × 1.5, rounded down)
- Range increased to 2 meters
- **New:** If target is [Bloodied], gain +1 bonus die to healing roll

### Rank 3 (Mastery — Capstone)

- Healing = Poultice roll + (WITS × 2)
- Range increased to 4 meters
- **New:** Mend Wound also removes one minor debuff ([Bleeding], [Poisoned], or [Disoriented])
- **New:** If healing exceeds target's missing HP, excess converts to temporary HP (max 10)

---

## V. Tactical Applications

1. **Sustained Healing:** Reliable HP restoration throughout combat
2. **Resource Management:** Choose when to spend limited poultices
3. **WITS Investment Payoff:** High-WITS Bone-Setters heal significantly more
4. **Poultice Quality Leverage:** [Masterwork] poultices create massive heals
5. **Self-Healing:** Can keep themselves alive when necessary

---

## VI. Synergies & Interactions

### Positive Synergies

- **Field Medic I:** Superior crafting produces better poultices
- **Triage (Tier 2):** +25% healing on [Bloodied] allies
- **Jötun-Reader (Calculated Triage):** Adjacent positioning adds +25% more
- **High-WITS builds:** Flat bonus scales extremely well
- **Skjaldmær ("First, Do No Harm"):** Triggers +2 Defense for Bone-Setter

### Negative Synergies

- **No poultices:** Completely ineffective without consumables
- **Low-WITS builds:** Significantly reduced healing output
- **Ranged parties:** Touch range can be difficult to maintain