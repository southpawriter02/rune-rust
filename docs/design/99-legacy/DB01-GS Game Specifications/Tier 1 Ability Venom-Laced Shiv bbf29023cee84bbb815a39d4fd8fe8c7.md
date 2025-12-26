# Tier 1 Ability: Venom-Laced Shiv

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-MYRSTALKER-VENOMLACEDSHIV-v5.0
Parent item: Myr-Stalker (Entropic Predator) (Myr-Stalker%20(Entropic%20Predator)%20288c265eb42447bda85d985ed17ea3be.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 1
Template Validated: No
Voice Validated: No

## Core Identity

**Venom-Laced Shiv** is your primary damage-over-time applicator. You strike with a blade coated in distilled entropy—toxins harvested from Blighted flora and concentrated into lethal poison. Each cut introduces decay into the target's system.

---

## Mechanics

### Cost

15 Stamina | **Standard Action** | **Melee Attack**

### Effect

- FINESSE-based melee attack
- Deal weapon damage + apply **1 stack [Poisoned]**
- [Poisoned]: **1d6 damage per turn** per stack
- Can stack up to **5 times** on single target
- Mechanical/Undying take normal damage (poison affects their systems)

### Rank Progression

| Rank | PP Cost | Effect |
| --- | --- | --- |
| 1 | 3 | Apply 1 stack [Poisoned] (1d6/turn) |
| 2 | 20 | Apply 2 stacks per hit; 10 Stamina cost |
| 3 | 35 | Apply 2 stacks; [Poisoned] deals 1d8/turn per stack |

### Maximum Stack Damage

| Stacks | Rank 1-2 Damage | Rank 3 Damage |
| --- | --- | --- |
| 1 | 1d6/turn | 1d8/turn |
| 3 | 3d6/turn (~10) | 3d8/turn (~13) |
| 5 (Max) | 5d6/turn (~17) | 5d8/turn (~22) |

---

## Tactical Applications

**Core Combat Loop:**

```jsx
Turn 1: Venom-Laced Shiv → 1 stack [Poisoned]
Turn 2: Venom-Laced Shiv → 2 stacks [Poisoned]
Turn 3: Venom-Laced Shiv → 3 stacks [Poisoned]
...continue until 5 stacks

Result: 5d6 damage per turn while you do other things
```

**Synergies:**

- **Corruption Catalyst:** Higher Corruption = more poison damage
- **Miasmic Shroud:** Defensive + auto-poison attackers
- **Systemic Collapse:** Ultimate single-target DoT

---

## Failure Modes

<aside>
⚠️

**Melee Requirement** — Must close to melee range. Myr-Stalker is fragile; requires positioning discipline.

</aside>

<aside>
⚠️

**Poison Immunity** — Some enemies are immune to [Poisoned]. Against them, this is just a basic attack.

</aside>

---

## Integration Notes

**Parent Specialization:** Myr-Stalker (Entropic Predator)

**Archetype:** Skirmisher

**Role:** Sustained Damage Dealer