# Tier 1 Ability: Heretical Augmentation

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKARHORDEAPIRANT-HERETICALAUGMENTATION-v5.0
Mechanical Role: Damage Dealer
Parent item: Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0 (Skar-Horde%20Aspirant%20(Augmented%20Brawler)%20%E2%80%94%20Speciali%20dcff21d0a06040698381b59039deaf60.md)
Proof-of-Concept Flag: No
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Validated: No

## Overview

| Property | Value | Property | Value |
| --- | --- | --- | --- |
| **Ability Name** | Heretical Augmentation | **Ability Type** | Passive |
| **Tier** | 1 (Foundation) | **PP Cost** | 0 PP (free with spec) |
| **Specialization** | Skar-Horde Aspirant | **Ranks** | R1 → R2 → R3 |
| **Prerequisite** | Unlock Skar-Horde Aspirant (3 PP) | **Trauma Risk** | None (implicit in augmentation) |

---

## Rank Progression

| Rank | Trigger | Key Unlock |
| --- | --- | --- |
| **Rank 1** | Unlock specialization (3 PP) | [Augmentation] slot, basic augments |
| **Rank 2** | Train 2 Tier 2 abilities | Quick-swap (1 action), [Optimized] augments |
| **Rank 3** | Train Capstone | Dual augments, [Masterwork] augments |

---

## Thematic Description

> *"You cut off your own hand. Not in desperation, but in calculation. The stump is not a wound—it is a socket. A platform for power."*
> 

Heretical Augmentation represents the defining choice of the Skar-Horde path: the permanent sacrifice of flesh for the promise of mechanical transcendence. This isn't a curse or an accident—it's a deliberate surgical act, usually performed by the Aspirant themselves with crude tools and sheer determination. The augmentation socket that remains can accept various weapon-stump attachments, each offering different combat capabilities.

---

## Mechanical Implementation

### Rank 1 (Foundation — With Spec Unlock)

- **[Augmentation] Slot:** Gain a permanent augmentation equipment slot (replaces main-hand weapon slot)
- **Starting Augment:** Begin with [Basic Serrated Claw] or [Basic Piston Hammer] (player choice)
- **Augment Swapping:** Can swap augments at any **Workbench** (requires 1 minute outside combat)
- **Crafting Access:** Can craft [Basic] tier augments at Workbenches

### Rank 2 (Advanced — 2 Tier 2 Abilities Trained)

- **Quick-Swap:** Can swap augments as a **Standard Action** (1 action in combat)
- **[Optimized] Augments:** Can now craft and equip [Optimized] tier augments
- **Augment Affinity:** +1 to hit with augment attacks after swapping (1 turn)

### Rank 3 (Mastery — Capstone Trained)

- **Dual Augment Mount:** Can have **two augments** socketed simultaneously
- **Instant Swap:** Swap between socketed augments as a **Free Action**
- **[Masterwork] Augments:** Can now craft and equip [Masterwork] tier augments
- **Augment Synergy:** When swapping, retain 25% of Savagery (instead of losing all)

---

## Resolution Pipeline

### Augment Swap Resolution

```jsx
1. VALIDATE: At Workbench (Rank 1) OR in combat (Rank 2+)
2. CHECK: Rank for action cost
   - Rank 1: 1 minute (out of combat only)
   - Rank 2: Standard Action
   - Rank 3: Free Action (between socketed augments)
3. IF in combat AND Rank 1:
   a. BLOCK: "Cannot swap augments in combat at Rank 1."
4. IF valid:
   a. UNEQUIP: Current augment
   b. EQUIP: New augment
   c. APPLY: Augment stats and ability unlocks
   d. IF Rank 2+: Grant +1 to hit (1 turn)
   e. IF Rank 3 AND had Savagery: Retain 25% of current Savagery
```

### Dual Augment Resolution (Rank 3)

```jsx
1. SLOT_1: Primary augment (active)
2. SLOT_2: Secondary augment (socketed but inactive)
3. ON swap (Free Action):
   a. SWITCH: Active augment status
   b. APPLY: New augment's stats and ability unlocks
   c. RETAIN: 25% Savagery
4. CRAFTING: Can have different tiers in each slot
```

---

## Worked Examples

### Example 1: Basic Augment Swap (Rank 1)

```jsx
Grimnir at Workbench, wants to swap [Basic Serrated Claw] → [Basic Piston Hammer].
├── Location: Workbench ✓
├── Rank: 1
├── Time: 1 minute (out of combat)
├── SWAP: [Basic Serrated Claw] → [Basic Piston Hammer]
├── Ability Change: Impaling Spike now BLOCKED (needs [Piercing])
├── Ability Change: Overcharged Piston Slam now ENABLED
└── Result: Ready for [Blunt] damage and Slam attacks
```

### Example 2: Combat Quick-Swap (Rank 2)

```jsx
Mid-combat, Grimnir needs to switch from [Serrated Claw] to [Piston Hammer].
├── Rank: 2 (Quick-Swap enabled)
├── Action Cost: Standard Action
├── Savagery: 45 → 0 (lost on swap at Rank 2)
├── +1 to hit for next attack (Augment Affinity)
├── SWAP: [Optimized Serrated Claw] → [Optimized Piston Hammer]
└── Result: Can now use Overcharged Piston Slam next turn
```

### Example 3: Instant Dual-Augment Swap (Rank 3)

```jsx
Grimnir has [Masterwork Serrated Claw] and [Masterwork Piston Hammer] socketed.
├── Rank: 3 (Dual Augment Mount)
├── Current: [Serrated Claw] active
├── Savagery: 60
├── ACTION: Swap to [Piston Hammer] (FREE ACTION)
├── Savagery: 60 × 25% = 15 retained
├── Abilities: Impaling Spike → Overcharged Piston Slam
└── Result: Instant tactical pivot without losing turn or all Savagery

Same turn: Use Overcharged Piston Slam
├── Savagery: 15 + 40 (cost) = needs 40, have 15
├── BLOCK: "Insufficient Savagery"
└── Result: Still need to rebuild Savagery after swap
```

### Example 4: Crafting Progression

```jsx
Rank 1: Can craft [Basic] augments
├── [Basic Serrated Claw]: 2d6 damage, [Piercing]
├── [Basic Piston Hammer]: 2d8 damage, [Blunt], [Armor Piercing]

Rank 2: Can craft [Optimized] augments
├── [Optimized Serrated Claw]: 2d8 damage, [Piercing], +[Bleeding] on crit
├── [Optimized Piston Hammer]: 2d10 damage, [Blunt], [Armor Piercing]

Rank 3: Can craft [Masterwork] augments
├── [Masterwork Serrated Claw]: 3d8 damage, [Piercing], +[Bleeding] on hit
├── [Masterwork Piston Hammer]: 3d10 damage, [Blunt], [Armor Piercing], +Stun on crit
```

---

## Failure Modes

### Swap Blocked (Rank 1 in Combat)

```jsx
Grimnir (Rank 1) attempts to swap augments mid-combat.
├── BLOCK: "Cannot swap augments in combat at Rank 1."
├── Suggest: "Return to Workbench or advance to Rank 2."
└── Result: Must plan augment loadout before combat
```

### No Workbench Available

```jsx
Grimnir wants to swap augments but no Workbench nearby.
├── Rank 1: BLOCK (requires Workbench)
├── Rank 2-3: Can swap in combat (action cost)
└── Result: Field swaps possible at higher ranks
```

### Wrong Augment for Ability

```jsx
Grimnir has [Piston Hammer], attempts Impaling Spike.
├── Impaling Spike requires: [Piercing] tag
├── [Piston Hammer] has: [Blunt] tag
├── BLOCK: "Impaling Spike requires [Piercing] augment."
└── Result: Must swap to [Serrated Claw] or [Injector Spike]
```

---

## Synergies & Interactions

### Internal Synergies (Skar-Horde Tree)

- **Savage Strike:** Uses equipped augment's damage dice
- **Impaling Spike:** Requires [Piercing] augment
- **Overcharged Piston Slam:** Requires [Blunt] augment
- **Monstrous Apotheosis:** Augment damage amplified during transformation

### External Synergies

- **Scrap-Tinker:** Can craft superior augments for Aspirant
- **Workbench Access:** Sanctuary/camp Workbench essential for Rank 1 swaps
- **Pre-Combat Intel:** Knowing enemy types enables optimal augment selection

### Augment Tag Summary

| Augment | Tag | Enables | Special |
| --- | --- | --- | --- |
| [Serrated Claw] | [Piercing] | Impaling Spike | +[Bleeding] |
| [Piston Hammer] | [Blunt] | Overcharged Piston Slam | [Armor Piercing] |
| [Injector Spike] | [Piercing] | Impaling Spike | +[Poisoned] |
| [Taser Gauntlet] | [Energy] | — | +[Shocked] |

---

## Tactical Applications

1. **Pre-Combat Preparation:** Choose augment based on expected enemies (armor → [Blunt], mobility → [Piercing])
2. **Mid-Combat Adaptation:** Rank 2+ enables tactical pivots based on situation
3. **Dual-Augment Flexibility:** Rank 3 allows instant response to changing threats
4. **Crafting Investment:** Higher-tier augments dramatically increase effectiveness
5. **Build Identity:** Augment choice defines playstyle (control vs. burst damage)

---

## v5.0 Compliance Notes

- **Tier 1 Structure:** R1→R2→R3 progression (free with spec, ranks via tree investment)
- **Rank 2 Trigger:** 2 Tier 2 abilities trained
- **Rank 3 Trigger:** Capstone trained
- **Augment System:** Core identity mechanic; see [Weapon-Stump Augmentation System — Mechanic Specification v5.0](Weapon-Stump%20Augmentation%20System%20%E2%80%94%20Mechanic%20Specif%20c3328afcc82e442bb9680f41336f44d5.md)
- **No PP Cost:** Tier 1 abilities are granted free when specialization is unlocked