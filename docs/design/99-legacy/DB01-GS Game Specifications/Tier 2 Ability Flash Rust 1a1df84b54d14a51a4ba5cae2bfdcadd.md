# Tier 2 Ability: Flash Rust

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Mystic
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-RUSTWITCH-FLASHRUST-v5.0
Parent item: Rust-Witch (Agent of Entropy) — Specialization Specification v5.0 (Rust-Witch%20(Agent%20of%20Entropy)%20%E2%80%94%20Specialization%20Spe%205a64fb8a16e8431b85fc4cf3d4f1a7a7.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 2
Template Validated: No
Voice Validated: No

## Core Identity

**Specialization:** Rust-Witch (Agent of Entropy)

**Tier:** 2 (Advanced Decay)

**Type:** Active (Standard Action)

**Prerequisite:** 8 PP in Rust-Witch tree, Corrosive Curse + 1 other Tier 1 ability

**Cost:** 4 PP

---

## Description

You clap your hands together, releasing an expanding cloud of hyper-corrosive particles that blankets an entire area. The rust spreads like plague, affecting everything in its path. Metal surfaces bloom with oxidation, armor tarnishes instantly, and the air itself becomes caustic. This is entropy weaponized as area denial.

---

## Mechanics

### Action Economy

| Property | Value |
| --- | --- |
| **Action Cost** | Standard Action |
| **Resource Cost** | 35 Aether |
| **Targeting** | All enemies in target row |
| **Duration** | 2 turns (shorter than single-target) |

### Per-Target Corruption Cost

| Enemies Hit | Corruption Gained |
| --- | --- |
| 1 | +1 |
| 2 | +2 |
| 3 | +3 |
| 4+ | +1 per target |

### [Corroded] Application

- Apply **1 stack of [Corroded]** to each enemy
- **1d6 damage/turn** + **-2 Armor** per target
- **Mechanical/Undying:** Double damage (2d6)
- Stacks with existing [Corroded] from Corrosive Curse

---

## Tactical Applications

1. **Mass Debuff:** Your only multi-target [Corroded] applicator
2. **Boss Pack Fights:** Devastating against boss + adds formations
3. **Entropic Field Combo:** Use inside your zone for maximum armor shred (-4 total)
4. **Accelerated Entropy Synergy:** Multiple targets = multiple Aether regen procs

---

## Risk Assessment

<aside>
⚠️

**High Corruption Risk:** Hitting 3 enemies = +3 Corruption in single cast. Extreme risk/reward—is devastating armor shred worth the soul cost?

</aside>

---

## Integration Notes

- **AOE Trade-Off:** Less duration than Corrosive Curse, but affects entire formations
- **Resource Intensive:** High Aether + high Corruption cost
- **Combo Potential:** Entropic Field + Flash Rust = -4 Armor to entire row

---

## Related Abilities

- **Corrosive Curse** (Tier 1): Single-target version with better duration
- **Accelerated Entropy** (Tier 2): Multiple targets = multiple Aether regen procs
- **Inevitable Decay** (Tier 3): Makes this [Corroded] permanent
- **Entropic Field** (Tier 1): Stack with this for devastating area control