# Tier 2 Ability: Corrosive Haze

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-MYRSTALKER-CORROSIVEHAZE-v5.0
Parent item: Myr-Stalker (Entropic Predator) (Myr-Stalker%20(Entropic%20Predator)%20288c265eb42447bda85d985ed17ea3be.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 2
Template Validated: No
Voice Validated: No

## Core Identity

**Corrosive Haze** upgrades your single-target poison delivery into area-of-effect devastation. You exhale concentrated Blight spores in a noxious cloud that corrodes armor and sickens all caught within.

---

## Mechanics

### Cost

25 Stamina | **Standard Action** | **Cone Attack (6m)**

### Effect

- Exhale a **6m cone** of corrosive vapor
- All enemies in cone: Apply **1 stack [Poisoned]** + **[Corroded]** (2 turns)
- [Corroded]: **-2 Soak** (armor degradation)
- Creates **[Toxic Cloud]** terrain (2 turns) in affected area

### Rank Progression

| Rank | PP Cost | Effect |
| --- | --- | --- |
| 1 | 4 | 1 stack [Poisoned] + [Corroded] (-2 Soak) |
| 2 | 22 | 2 stacks [Poisoned]; 20 Stamina cost |
| 3 | 38 | 2 stacks; [Corroded] = -3 Soak; cloud lasts 3 turns |

---

## Tactical Applications

**Multi-Target Poison Spread:**

```jsx
3 enemies clustered: Use Corrosive Haze
Result: All 3 get [Poisoned] + [Corroded]
Follow-up: Venom-Laced Shiv to stack poison on priority target
```

**Armor Stripping:**

```jsx
Heavy armor enemy (8 Soak) → Corrosive Haze
8 Soak → 6 Soak (Rank 1) or 5 Soak (Rank 3)
Result: Party deals +2-3 damage per hit
```

**Synergies:**

- **Blighted Symbiosis:** You can stand in your own toxic cloud for healing
- **Create Toxin Trap:** Layer multiple hazard zones
- **Pandemic Bloom:** AoE setup for capstone detonation

---

## Failure Modes

<aside>
⚠️

**Cone Targeting** — Friendly fire risk. Must position carefully to avoid allies.

</aside>

<aside>
⚠️

**High Stamina Cost** — 25 Stamina is expensive. Can't spam freely.

</aside>

---

## Integration Notes

**Parent Specialization:** Myr-Stalker (Entropic Predator)

**Archetype:** Skirmisher

**Role:** AoE Debuffer, Armor Stripper

**Prerequisite:** Tier 1 (3 abilities)