# Tier 1 Ability: Apply Tourniquet

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BONESETTER-APPLYTOURNIQUET-v5.0
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
| **Type** | Active (Cleanse) |
| **Prerequisite** | Unlock Bone-Setter Specialization (10 PP) |
| **Cost** | Standard Action (no consumable) |
| **Target** | Single ally |

---

## I. Design Context (Layer 4)

### Core Design Intent

Apply Tourniquet is the Bone-Setter's **zero-cost emergency response**—a skill-based cleanse that removes [Bleeding] without consuming any resources. This provides critical utility against bleed-focused enemies.

### Mechanical Role

- **Primary:** Remove [Bleeding] status from single ally
- **Secondary:** Zero consumable cost for situational utility
- **Fantasy Delivery:** The immediate, practiced response to arterial trauma

### Balance Considerations

- **Power Level:** High situational value, low general value
- **Cost Efficiency:** No consumable requirement allows spam in relevant encounters
- **Limitation:** Only removes [Bleeding], not other conditions

---

## II. Narrative Context (Layer 2)

### In-World Framing

Blood spurts from the wound in rhythmic pulses—arterial. The Bone-Setter doesn't hesitate. Hands find the pressure point above the injury, fingers pressing hard to cut off flow. A strip of treated cloth wraps tight, twisting, tighter still until the bleeding stops. The limb may go numb, but numb is alive.

### Thematic Resonance

The tourniquet is one of medicine's oldest tools—brutal, simple, effective. In Aethelgard's harsh world, sometimes the most basic interventions are the most valuable.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** None (skill-based, no consumable)
- **Range:** Touch (adjacent ally)
- **Target:** Single ally with [Bleeding] status

### Effect

**Immediately removes the [Bleeding] status effect from target ally.**

### [Bleeding] Status Reminder

- Deals damage at start of affected character's turn
- Damage varies by source (typically 1d4 to 2d6 per turn)
- Persists until cleansed or combat ends
- Can stack from multiple sources

### Resolution Pipeline

1. **Targeting:** Bone-Setter selects adjacent ally with [Bleeding]
2. **Status Verification:** Confirm target has [Bleeding] status
3. **Cleanse Application:** Remove [Bleeding] from target
4. **Stack Handling:** Removes ALL [Bleeding] stacks, not just one

### Edge Cases

- **No [Bleeding]:** Ability can still be used, but no effect occurs
- **Multiple Stacks:** Removes all stacks simultaneously
- **Self-Target:** Can target self if Bone-Setter is [Bleeding]
- **[Bleeding] from Multiple Sources:** All removed regardless of source

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Remove [Bleeding] from adjacent ally
- Standard Action, no cost

### Rank 2 (Expert — 20 PP)

- Remove [Bleeding] from ally within 2 meters
- **New:** Target also regains 1d6 HP when [Bleeding] is removed
- **New:** Can be used as a Bonus Action instead of Standard Action

### Rank 3 (Mastery — Capstone)

- Remove [Bleeding] from ally within 4 meters
- Target regains 2d6 HP when [Bleeding] is removed
- Bonus Action
- **New:** Can target 2 allies simultaneously if both are adjacent to each other
- **New:** Target gains immunity to [Bleeding] for 1 round after cleanse

---

## V. Tactical Applications

1. **Bleed Counters:** Essential against bleed-heavy enemies (Draugr, beasts)
2. **Action Efficiency:** Zero cost means no resource trade-off
3. **Damage Prevention:** Removing [Bleeding] prevents ongoing damage
4. **Critical Saves:** Can prevent death-by-bleed in low-HP situations
5. **Free Utility:** Useful even when not optimal (no waste)

---

## VI. Synergies & Interactions

### Positive Synergies

- **Enemies with bleed mechanics:** Maximum value against slashing/piercing heavy encounters
- **Melee Warriors (Berserkr, Skjaldmær):** Front-line allies take most bleeds
- **"First, Do No Harm" (Tier 3):** Triggers defensive bonus after use
- **Low-resource situations:** Valuable when poultices are depleted

### Negative Synergies

- **Enemies without bleed:** Completely useless against non-bleeding damage
- **Back-row positioning:** Touch range requires Bone-Setter near front
- **Action economy:** Standard Action is expensive if bleed is minor