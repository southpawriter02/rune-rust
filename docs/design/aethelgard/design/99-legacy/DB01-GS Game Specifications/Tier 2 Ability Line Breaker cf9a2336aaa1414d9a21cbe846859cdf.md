# Tier 2 Ability: Line Breaker

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-ATGEIR-LINEBREAKER-v5.0
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
| **Type** | Active (AoE Control Attack) |
| **Prerequisite** | 8 PP spent in Atgeir-wielder tree |
| **Cost** | 50 Stamina |
| **Target** | All enemies in Front Row |
| **Condition** | Atgeir-wielder must be in Front Row |
| **Effect** | Moderate damage + [Push] to Back Row |

---

## I. Design Context (Layer 4)

### Core Design Intent

Line Breaker is the Atgeir-wielder's **primary push and AoE formation-breaking tool**—a wide, powerful horizontal sweep that drives back the entire enemy frontline. This shatters defensive formations and exposes vulnerable back-row targets.

### Mechanical Role

- **Primary:** [Push] all enemies from Front Row to Back Row
- **Secondary:** Deal moderate Physical damage to all targets
- **Condition:** Atgeir-wielder must be in Front Row
- **Fantasy Delivery:** The irresistible sweep that breaks shield walls

### Balance Considerations

- **Power Level:** High (AoE damage + AoE control)
- **High Cost:** 50 Stamina is significant investment
- **Accuracy Penalty:** Wide sweep is less precise (-1 die)
- **Opposed Checks:** Each target can resist [Push] individually

---

## II. Narrative Context (Layer 2)

### In-World Framing

The atgeir is not meant for dueling. It is meant for **war**.

The Atgeir-wielder plants their feet, draws the weapon back, and unleashes a sweeping horizontal arc that catches the entire enemy frontline. The polearm's weight becomes momentum, momentum becomes force, and force becomes **chaos**.

Shields crack. Footing fails. The enemy's carefully organized formation dissolves into stumbling disorder as warriors are hurled backward into their own back line.

### Thematic Resonance

Line Breaker is the Atgeir-wielder as formation destroyer—proof that one warrior with the right weapon can reshape an entire battlefield.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 50 Stamina
- **Range:** Melee (AoE)
- **Target:** All enemies in Enemy Front Row
- **User Requirement:** Must be in Player Front Row

### Effect

**Attack Resolution:**

- Separate attack roll against each target
- Accuracy Pool: FINESSE + Weapon Skill **-1 die** (sweep is less precise)

**Damage:** Weapon damage + MIGHT Physical (Concussive) per target hit

**[Push] Attempt (per target hit):**

- **Opposed Check:** Atgeir-wielder's MIGHT vs target's STURDINESS
- **Success:** Target is [Pushed] from Front Row to Back Row
- **Failure:** Target holds ground, remains in Front Row
- **Critical Hit Bonus:** Also inflicts [Knocked Down] for 1 round

### Resolution Pipeline

1. **Position Validation:** User in Front Row
2. **Cost Payment:** Spend 50 Stamina
3. **Target Acquisition:** All enemies in Enemy Front Row
4. **Per-Target Loop:**
    - Attack Roll (FINESSE + Weapon Skill -1) vs Defense
    - On Hit: Apply damage
    - On Hit: Opposed MIGHT vs STURDINESS for [Push]
5. **Movement:** Move successfully pushed targets to Back Row

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Weapon + MIGHT Physical to all Front Row enemies
- [Push] attempt per target (opposed check)
- -1 die Accuracy penalty
- Cost: 50 Stamina

### Rank 2 (Expert — 20 PP)

- Weapon + MIGHT + 1d6 Physical damage
- [Push] attempt per target
- No Accuracy penalty (removed)
- Cost: 45 Stamina
- **New:** Pushed targets cannot use movement on their next turn

### Rank 3 (Mastery — Capstone)

- Weapon + MIGHT + 2d6 Physical damage
- [Push] attempt with +2 dice to MIGHT check
- No Accuracy penalty
- Cost: 40 Stamina
- Pushed targets cannot move next turn
- **New:** Deals bonus 2d6 damage to targets in defensive stances ([Blocking], [Shield Wall])
- **New:** Targets already in Back Row take +1d6 damage (crushed against wall)

---

## V. Tactical Applications

1. **Shield Wall Breaker:** Hard counter to defensive formations
2. **Exposure:** Push frontline away, expose vulnerable backline
3. **Setup Combo:** Line Breaker → Hook and Drag the now-exposed caster
4. **Environmental Combo:** Push enemies into Back Row hazards
5. **Crowd Control:** Disrupt multiple enemies simultaneously

---

## VI. Synergies & Interactions

### Positive Synergies

- **Hook and Drag:** Push front back, pull back forward—total chaos
- **Party ranged attackers:** Exposed backline becomes easy targets
- **Environmental hazards:** Push into [Chasms], [Burning Ground], etc.
- **Formal Training I:** Stamina regen sustains high-cost ability

### Negative Synergies

- **Back Row positioning:** Cannot use from Back Row
- **High-STURDINESS enemies:** Brutes may resist the push
- **Full Back Row:** No space to push into = push fails