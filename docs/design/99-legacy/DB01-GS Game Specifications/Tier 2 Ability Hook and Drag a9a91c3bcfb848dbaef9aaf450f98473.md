# Tier 2 Ability: Hook and Drag

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-ATGEIR-HOOKANDDRAG-v5.0
Mechanical Role: Controller/Debuffer, Damage Dealer
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
| **Tier** | 2 (Advanced Battlefield Control) |
| **Type** | Active (Control Attack) |
| **Prerequisite** | 8 PP spent in Atgeir-wielder tree |
| **Cost** | 45 Stamina |
| **Target** | Single enemy in Back Row |
| **Condition** | Atgeir-wielder must be in Front Row |
| **Effect** | Minor damage + [Pull] to Front Row |

---

## I. Design Context (Layer 4)

### Core Design Intent

Hook and Drag is the Atgeir-wielder's **primary pull ability**—using the hooked blade of their polearm to snag vulnerable back-row enemies and brutally yank them into the frontline meat grinder. This is the ultimate formation-breaking tool.

### Mechanical Role

- **Primary:** [Pull] enemy from Back Row to Front Row
- **Secondary:** Deal minor Physical damage
- **Condition:** Atgeir-wielder must be in Front Row
- **Fantasy Delivery:** The unwilling conscription into the killing field

### Balance Considerations

- **Power Level:** High (powerful control effect)
- **Positional Requirements:** User in Front Row, target in Back Row
- **Opposed Check:** Target can resist with STURDINESS
- **Setup Power:** Enables devastating follow-ups from party

---

## II. Narrative Context (Layer 2)

### In-World Framing

The enemy caster thinks they're safe. Hidden behind their frontline, protected by meat shields, they prepare their devastating magic. Then the atgeir's hooked blade flashes out—a blur of steel that snags cloth, catches armor, finds purchase.

And then they're *flying*.

Dragged screaming from their sanctuary, the caster crashes into the front row, suddenly surrounded by the very warriors they thought they'd never have to face. The Atgeir-wielder's pull is not gentle. It is an act of **violent relocation**.

### Thematic Resonance

Hook and Drag is the Atgeir-wielder's answer to protected enemies—proof that no position is truly safe from a master of the polearm.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 45 Stamina
- **Range:** Extended melee (Front Row → Back Row)
- **Target:** Single enemy in Enemy Back Row
- **User Requirement:** Must be in Player Front Row

### Effect

**Damage:** (Weapon Base / 2) + MIGHT Physical (Piercing/Slashing)

- Reduced damage—control is the primary purpose

**[Pull] Attempt:**

- **Opposed Check:** Atgeir-wielder's MIGHT vs target's STURDINESS
- **Success:** Target is [Pulled] from Back Row to unoccupied Front Row tile
- **Failure:** Target resists, remains in Back Row
- **No Space:** If Front Row is full, [Pull] automatically fails

### Resolution Pipeline

1. **Position Validation:** User in Front Row, target in Back Row
2. **Cost Payment:** Spend 45 Stamina
3. **Attack Roll:** FINESSE + Weapon Skill vs target Defense
4. **Damage Application:** On hit, deal reduced weapon damage
5. **Pull Attempt:** Opposed MIGHT vs STURDINESS check
6. **Movement:** On failed resist, move target to Front Row

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Minor damage + [Pull] attempt
- Opposed MIGHT vs STURDINESS
- Cost: 45 Stamina

### Rank 2 (Expert — 20 PP)

- Minor damage + [Pull] attempt
- Opposed MIGHT vs STURDINESS
- Cost: 40 Stamina
- **New:** Successfully pulled targets are also [Staggered] for 1 round

### Rank 3 (Mastery — Capstone)

- Moderate damage + [Pull] attempt
- +2 dice bonus to the MIGHT check
- Cost: 35 Stamina
- Pulled targets are [Staggered] for 1 round
- **New:** Can target enemies in Front Row to [Pull] them into environmental hazards
- **New:** If target dies within 1 round of being pulled, restore 15 Stamina

---

## V. Tactical Applications

1. **Back-Row Assassination:** Pull fragile casters into melee range
2. **Formation Breaking:** Disrupt enemy positioning entirely
3. **Setup:** Serve up targets for Berserkr/Hólmgangr follow-up
4. **Hazard Manipulation:** Pull enemies into environmental dangers
5. **Counter-Strategy:** Hard counter to "protect the caster" tactics

---

## VI. Synergies & Interactions

### Positive Synergies

- **Line Breaker:** Push frontline back, then pull backline forward
- **Berserkr ally:** Pull target into Berserkr's melee range
- **Environmental hazards:** Pull into [Burning Ground], [Chasms], etc.
- **Party damage dealers:** All melee allies can now reach the target

### Negative Synergies

- **Back Row positioning:** Cannot use if Atgeir-wielder is in Back Row
- **High-STURDINESS targets:** Brutes can resist the pull
- **Full Front Row:** Cannot pull if no space available