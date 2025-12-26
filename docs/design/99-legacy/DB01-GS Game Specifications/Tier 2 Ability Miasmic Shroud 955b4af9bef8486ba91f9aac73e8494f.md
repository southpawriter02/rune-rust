# Tier 2 Ability: Miasmic Shroud

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-MYRSTALKER-MIASMICSHROUD-v5.0
Parent item: Myr-Stalker (Entropic Predator) (Myr-Stalker%20(Entropic%20Predator)%20288c265eb42447bda85d985ed17ea3be.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 2
Template Validated: No
Voice Validated: No

## Core Identity

**Miasmic Shroud** wraps you in a personal cloud of toxic vapor—a defensive aura that punishes anyone who dares strike you. Enemies who attack at close range breathe in your corruption.

---

## Mechanics

### Cost

20 Stamina | **Bonus Action** | **Self-Buff (3 turns)**

### Effect

- Surround yourself with a **[Miasmic Shroud]** aura (2m radius)
- **Reactive Poison:** When an enemy hits you with a melee attack, they gain **1 stack [Poisoned]**
- **Concealment:** +1 Defense vs. ranged attacks (obscured by vapor)
- **Duration:** 3 turns

### Rank Progression

| Rank | PP Cost | Effect |
| --- | --- | --- |
| 1 | 4 | 1 stack [Poisoned] on melee attackers; +1 Defense (ranged) |
| 2 | 22 | 2 stacks [Poisoned]; 15 Stamina cost; 4 turns duration |
| 3 | 38 | 2 stacks; enemies entering aura also get 1 stack; +2 Defense (ranged) |

---

## Tactical Applications

**Frontline Presence:**

```jsx
Activate Miasmic Shroud → Move into melee
Enemy attacks you: They gain [Poisoned]
Your turn: Venom-Laced Shiv to stack more

Result: Passive damage on defense + active damage on offense
```

**Anti-Flanker:**

```jsx
Enemy rogue flanks you: Must enter aura
Rank 3: 1 stack [Poisoned] on entry
Enemy attacks: +2 more stacks

Result: 3 stacks [Poisoned] just for trying to flank you
```

**Synergies:**

- **Blighted Symbiosis:** Standing in your own shroud is harmless
- **Venom-Laced Shiv:** Layer active + passive poison
- **Corruption Catalyst:** More poison = more damage

---

## Failure Modes

<aside>
⚠️

**Ranged-Only Enemies** — If enemies won't melee you, the reactive poison never triggers.

</aside>

<aside>
⚠️

**Limited Duration** — 3-4 turns requires timing. Activating too early wastes the buff.

</aside>

---

## Integration Notes

**Parent Specialization:** Myr-Stalker (Entropic Predator)

**Archetype:** Skirmisher

**Role:** Defensive Utility, Passive Damage

**Prerequisite:** Tier 1 (3 abilities)