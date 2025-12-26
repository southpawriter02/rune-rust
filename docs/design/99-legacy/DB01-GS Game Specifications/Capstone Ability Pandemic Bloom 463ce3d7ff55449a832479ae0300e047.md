# Capstone Ability: Pandemic Bloom

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-MYRSTALKER-PANDEMICBLOOM-v5.0
Parent item: Myr-Stalker (Entropic Predator) (Myr-Stalker%20(Entropic%20Predator)%20288c265eb42447bda85d985ed17ea3be.md)
Proof-of-Concept Flag: Yes
Sub-Type: Capstone
Template Validated: No
Voice Validated: No

## Core Identity

**Pandemic Bloom** is the ultimate expression of the Myr-Stalker's entropic mastery. You release a catastrophic wave of Blight spores that transforms every poisoned enemy into an epicenter of contagion—their suffering blooms outward, spreading to all nearby.

---

## Mechanics

### Cost

50 Stamina | **Full-Round Action** | **20m Radius (Self-Centered)**

### Effect

- All [Poisoned] enemies within 20m: **Consume all stacks**
- Each affected enemy takes **3d6 damage per stack consumed**
- **Contagion Burst:** Each affected enemy creates a **4m radius [Toxic Cloud]** (3 turns)
- Non-poisoned enemies adjacent to affected targets: Gain **2 stacks [Poisoned]**
- You gain **+1 Corruption** per enemy affected

### Rank Progression

| Rank | PP Cost | Effect |
| --- | --- | --- |
| 1 | 6 | 3d6 per stack; 2 stacks spread; +1 Corruption/target |
| 2 | 28 | 3d8 per stack; 40 Stamina cost |
| 3 | 45 | 4d6 per stack; 3 stacks spread; clouds last 4 turns |

### Damage Calculation (3 enemies, 5 stacks each)

| Rank | Per Enemy | Total Damage | Cloud Zones |
| --- | --- | --- | --- |
| 1 | 15d6 (~52) | ~156 | 3 clouds |
| 2 | 15d8 (~67) | ~201 | 3 clouds |
| 3 | 20d6 (~70) | ~210 | 3 clouds (4 turns) |

---

## Tactical Applications

**Mass Combat Finisher:**

```jsx
Setup: Corrosive Haze spreads [Poisoned] to 5 enemies
Turns 2-3: Let DoT tick, stack more with Venom-Laced Shiv
Turn 4: Pandemic Bloom

Result: 15d6+ damage to each enemy + 5 toxic clouds
```

**Chain Reaction:**

```jsx
3 poisoned enemies near 2 clean enemies
Pandemic Bloom: Damage poisoned targets
Contagion Burst: Clean enemies gain 2 stacks [Poisoned]

Result: Battlefield-wide poison spread
```

**Synergies:**

- **Corruption Catalyst:** Amplifies poison damage before consumption
- **One with the Rot:** Standing in resulting clouds heals you
- **Blighted Symbiosis:** Clouds heal you, hurt enemies

---

## Failure Modes

<aside>
⚠️

**Full-Round Action** — Consumes your entire turn. Vulnerable during cast.

</aside>

<aside>
⚠️

**Setup Requirement** — Enemies must be [Poisoned] for damage. Useless against clean targets.

</aside>

<aside>
⚠️

**Corruption Accumulation** — +1 Corruption per target. Mass use accelerates soul decay.

</aside>

---

## Integration Notes

**Parent Specialization:** Myr-Stalker (Entropic Predator)

**Archetype:** Skirmisher

**Role:** AoE Burst Finisher, Battlefield Control

**Prerequisite:** Tier 3 (1 ability)

---

## v5.0 Compliance

**Trauma Economy:** Corruption accumulation creates tension between power and soul integrity

**Environmental Consequence:** Creates lasting toxic terrain that affects future combat

**Heretical Fantasy:** Peak expression of Blight symbiosis—you are the plague vector