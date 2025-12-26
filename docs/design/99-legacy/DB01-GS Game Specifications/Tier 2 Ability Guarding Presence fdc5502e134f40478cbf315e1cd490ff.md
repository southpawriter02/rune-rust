# Tier 2 Ability: Guarding Presence

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-ATGEIR-GUARDINGPRESENCE-v5.0
Mechanical Role: Support/Healer, Tank/Durability
Parent item: Atgeir-wielder (Formation Master) — Specialization Specification v5.0 (Atgeir-wielder%20(Formation%20Master)%20%E2%80%94%20Specialization%20432d149cac2a41cfad275a49efd9785b.md)
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
| **Specialization** | Atgeir-wielder (Formation Master) |
| **Tier** | 2 (Advanced Battlefield Control) |
| **Type** | Passive (Defensive Aura) |
| **Prerequisite** | 8 PP spent in Atgeir-wielder tree |
| **Cost** | None (Passive) |
| **Condition** | Must be in Front Row, not [Stunned]/[Feared]/[Knocked Down] |
| **Effect** | +1 Soak to self and adjacent Front Row allies |

---

## I. Design Context (Layer 4)

### Core Design Intent

Guarding Presence is the Atgeir-wielder's **foundational support aura**—their calm, disciplined presence inspiring fortitude in nearby allies. This transforms them from a personal controller into a **formation protector**.

### Mechanical Role

- **Primary:** Grant +1 Soak to self and adjacent Front Row allies
- **Secondary:** Reward tight tactical positioning
- **Condition:** Only active while in Front Row and not incapacitated
- **Fantasy Delivery:** The rallying point whose composure strengthens the line

### Balance Considerations

- **Power Level:** Moderate (small but consistent party buff)
- **Positioning Dependent:** Requires Front Row and adjacency
- **State Dependent:** Breaks if Atgeir-wielder is disabled
- **Formation Reward:** Encourages classic phalanx positioning

---

## II. Narrative Context (Layer 2)

### In-World Framing

There is a presence on the battlefield that cannot be measured in damage or armor—the **steadying influence** of a professional warrior who simply *refuses to break*. The Atgeir-wielder's perfect footwork, controlled breathing, and absolute composure become a psychological anchor for their allies.

When the line threatens to shatter, when fear gnaws at resolve, the warriors beside the Atgeir-wielder find themselves standing a little straighter, holding their shields a little tighter, absorbing blows they might otherwise have let through.

### Thematic Resonance

Guarding Presence is the Atgeir-wielder's leadership made mechanical—proof that true formation mastery elevates everyone around them.

---

## III. Mechanical Specification (Layer 3)

### Activation Conditions

**Aura Active When:**

1. Atgeir-wielder is in Player Front Row
2. Atgeir-wielder is NOT afflicted with: [Stunned], [Feared], [Knocked Down]

### The Soak Aura

**Passive Effect:**

- Atgeir-wielder gains +1 Soak
- All allies in adjacent Front Row tiles gain +1 Soak
- "Adjacent" = tiles directly next to the Atgeir-wielder in Front Row

### Resolution Pipeline

1. **Position Check:** Verify Atgeir-wielder in Front Row
2. **Status Check:** Verify no disabling conditions
3. **Aura Application:** Apply +1 Soak to self and adjacent allies
4. **Continuous:** Re-check on position/status changes

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +1 Soak aura to self and adjacent Front Row allies
- Conditions: Front Row, not disabled

### Rank 2 (Expert — 20 PP)

- +2 Soak aura to self and adjacent Front Row allies
- Conditions: Front Row, not disabled
- **New:** Aura range extends to include Back Row allies directly behind

### Rank 3 (Mastery — Capstone)

- +3 Soak aura to self and all affected allies
- Extended range (Front + Back Row behind)
- **New:** Affected allies also gain +1 die to resist [Push] and [Pull]
- **New:** Aura persists for 1 round after leaving Front Row

---

## V. Tactical Applications

1. **Formation Buff:** Entire frontline gains damage reduction
2. **Party Durability:** +1 Soak × 3 allies = 3 damage prevented per AoE
3. **Positioning Reward:** Encourages tight, coordinated formations
4. **Tank Synergy:** Stack with Skjaldmær for massive combined Soak
5. **Leadership:** Mechanical representation of command presence

---

## VI. Synergies & Interactions

### Positive Synergies

- **Skjaldmær ally:** Combined Soak bonuses create unbreakable wall
- **Disciplined Stance:** Personal Soak + Aura Soak = massive mitigation
- **Tight formations:** Maximum value with 2+ adjacent allies
- **AoE defense:** Reduces damage across entire affected group

### Negative Synergies

- **Scattered formations:** No benefit if allies not adjacent
- **Back Row positioning:** Aura inactive from Back Row
- **Disabling effects:** [Stunned]/[Feared]/[Knocked Down] breaks aura
- **Enemy controllers:** Push/Pull can break formation, reducing aura value