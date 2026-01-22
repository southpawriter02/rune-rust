# Tier 2 Ability: Corruption Catalyst

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-MYRSTALKER-CORRUPTIONCATALYST-v5.0
Parent item: Myr-Stalker (Entropic Predator) (Myr-Stalker%20(Entropic%20Predator)%20288c265eb42447bda85d985ed17ea3be.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 2
Template Validated: No
Voice Validated: No

## Core Identity

**Corruption Catalyst** transforms your personal Runic Blight Corruption into weaponized lethality. The more corrupted you become, the more potent your poisons. This is the Myr-Stalker's dark bargain made manifest.

---

## Mechanics

### Always Active (Passive)

**Corruption Scaling:**

- Your [Poisoned] damage scales with your **Runic Blight Corruption** level
- Base: +0 poison damage
- At 25 Corruption: +1 poison damage per tick
- At 50 Corruption: +2 poison damage per tick
- At 75 Corruption: +3 poison damage per tick
- At 100 Corruption: +5 poison damage per tick

### Rank Progression

| Rank | PP Cost | Effect |
| --- | --- | --- |
| 1 | 4 | Corruption scaling (+1/+2/+3/+5 at thresholds) |
| 2 | 22 | Thresholds shift: 20/40/60/80 Corruption |
| 3 | 38 | Thresholds shift: 15/30/50/70; max bonus = +7 |

### Damage Calculation Example

| Corruption | Base Poison (5 stacks) | Catalyst Bonus | Total/Turn |
| --- | --- | --- | --- |
| 0 | 5d6 (~17) | +0 | ~17 |
| 50 | 5d6 (~17) | +10 (+2×5) | ~27 |
| 100 | 5d6 (~17) | +25 (+5×5) | ~42 |

---

## Tactical Applications

**High-Risk/High-Reward:**

```jsx
Low Corruption (safe): Normal poison damage
High Corruption (dangerous): Nearly double poison damage

Decision: How much soul will you trade for power?
```

**Synergies:**

- **Blighted Symbiosis:** Generates Corruption via environmental exposure
- **Venom-Laced Shiv:** Each stack benefits from Catalyst
- **Pandemic Bloom:** Capstone detonation includes Catalyst scaling

---

## Failure Modes

<aside>
⚠️

**Corruption Risk** — High Corruption triggers Blight manifestations. Your power comes with severe narrative and mechanical consequences.

</aside>

<aside>
⚠️

**No Direct Combat Use** — This passive only enhances existing poison; it has no standalone effect.

</aside>

---

## Integration Notes

**Parent Specialization:** Myr-Stalker (Entropic Predator)

**Archetype:** Skirmisher

**Role:** Damage Amplifier (Corruption-Scaled)

**Prerequisite:** Tier 1 (3 abilities)