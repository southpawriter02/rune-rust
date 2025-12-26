# Tier 2 Ability: Impaling Spike

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKARHORDEAPIRANT-IMPALINGSPIKE-v5.0
Mechanical Role: Controller/Debuffer
Parent item: Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0 (Skar-Horde%20Aspirant%20(Augmented%20Brawler)%20%E2%80%94%20Speciali%20dcff21d0a06040698381b59039deaf60.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Validated: No

## Overview

| Property | Value | Property | Value |
| --- | --- | --- | --- |
| **Ability Name** | Impaling Spike | **Ability Type** | Active |
| **Tier** | 2 (Advanced) | **PP Cost** | 4 PP |
| **Specialization** | Skar-Horde Aspirant | **Resource Cost** | 40 Stamina + 25 Savagery |
| **Action Type** | Standard Action | **Cooldown** | 3 turns |
| **Prerequisite** | 8 PP in Skar-Horde tree | **Requirement** | [Piercing] augment equipped |

---

## Thematic Description

> *"You slam your spike through foot, pinning them to the broken earth. They're not going anywhere."*
> 

Impaling Spike demonstrates the brutal practicality of the Skar-Horde approach: if an enemy might flee, nail them to the ground. The [Serrated Claw] or [Injector Spike] augment drives through armor, flesh, and bone to anchor the target in place. It's not elegant. It's not merciful. It's effective.

---

## Mechanical Implementation

### Rank 2 (Foundation — 4 PP)

- **Cost:** 40 Stamina + 25 Savagery | Standard Action
- **Cooldown:** 3 turns
- **Requirement:** [Piercing] tag augment ([Serrated Claw] or [Injector Spike])
- **Target:** Single enemy within melee range (1 hex)
- **Attack Roll:** MIGHT vs. target Defense
- **Damage:** 2d10 Physical (Piercing)
- **CC:** **75% chance** to apply **[Rooted]** for 2 turns
    - [Rooted]: Cannot move, -2 Defense

### Rank 3 (Mastery — With Capstone)

- **Damage:** 3d10 Physical (increased)
- **Root Chance:** **100%** (guaranteed)
- **Root Duration:** 3 turns (extended)
- **Bonus:** Gain **+2 to hit** against [Rooted] targets
- **Bonus:** [Rooted] targets take **+2 damage** from all your attacks

---

## Resolution Pipeline

### Attack Resolution

```
1. VALIDATE: Target in melee range (1 hex)
2. VALIDATE: Stamina >= 40 AND Savagery >= 25
3. VALIDATE: Cooldown available (not on cooldown)
4. VALIDATE: Augment has [Piercing] tag
   - Valid: [Serrated Claw], [Injector Spike]
   - Invalid: [Piston Hammer], [Taser Gauntlet]
   - IF invalid: BLOCK "Impaling Spike requires [Piercing] augment."
5. COST: Deduct 40 Stamina + 25 Savagery
6. SET: Cooldown = 3 turns
7. ROLL: MIGHT-based attack vs. target Defense
8. IF hit:
   a. DAMAGE: 2d10/3d10 Physical (by rank)
   b. ROLL: d100 vs. Root Threshold
      - Rank 2: 75% (roll 1-75 = root)
      - Rank 3: 100% (guaranteed)
   c. IF root triggers:
      i. APPLY: [Rooted] status
      ii. Duration: 2/3 turns (by rank)
      iii. IF Rank 3: Aspirant gains +2 hit vs. this target
      iv. IF Rank 3: Aspirant gains +2 damage vs. this target
9. IF miss:
   a. No damage
   b. No [Rooted] applied
   c. Resources still consumed
   d. Cooldown still triggered
```

### Augment Gating Check

```
1. CHECK: Character.EquippedAugment.Tag
2. IF Tag includes [Piercing]:
   a. PROCEED with ability
3. IF Tag does NOT include [Piercing]:
   a. BLOCK: "Cannot use Impaling Spike. Requires [Piercing] augment."
   b. GUI: Ability button grayed out with tooltip
   c. Suggest: "Swap to [Serrated Claw] or [Injector Spike]."
```

---

## Worked Examples

### Example 1: Basic Root (Rank 2)

```
Grimnir uses Impaling Spike against Blight-Touched Raider.
├── Augment Check: [Serrated Claw] has [Piercing] tag ✓
├── Stamina: 75 → 35 (cost: 40)
├── Savagery: 40 → 15 (cost: 25)
├── Attack Roll: MIGHT 4 vs. Defense 2 → 2 successes (HIT)
├── Damage: 2d10[7,5] = 12 Physical
├── Root Roll: d100 = 42 (≤75, ROOT TRIGGERS)
├── [Rooted] Applied: 2 turns
│   ├── Raider cannot move
│   └── Raider suffers -2 Defense
└── Result: Target pinned for 2 turns, vulnerable to follow-up
```

### Example 2: Guaranteed Root + Bonus Damage (Rank 3)

```
Grimnir (Rank 3) uses Impaling Spike against Corrupted Hound.
├── Augment Check: [Injector Spike] has [Piercing] tag ✓
├── Attack Roll: MIGHT 5 vs. Defense 2 → 3 successes (HIT)
├── Damage: 3d10[8,6,9] = 23 Physical
├── [Injector Spike] bonus: +[Poisoned] on hit
├── Root: 100% guaranteed at Rank 3
├── [Rooted] Applied: 3 turns
├── Rank 3 Bonus: +2 to hit this target, +2 damage vs. this target
└── Result: Target pinned for 3 turns, Poisoned, highly vulnerable
```

### Example 3: Root → Grievous Wound Combo

```
Turn 1: Grimnir uses Impaling Spike
├── Target: [Rooted] for 3 turns
├── Grimnir: +2 hit, +2 damage vs. target (Rank 3)

Turn 2: Grimnir uses Savage Strike to rebuild Savagery
├── +2 to hit (Rank 3 vs. Rooted)
├── +2 damage (Rank 3 vs. Rooted)
├── Savagery: 15 → 40 (+25 on hit)

Turn 3: Grimnir uses Grievous Wound
├── +2 to hit (vs. still-Rooted target)
├── +2 initial damage
├── [Grievous Wound] applied → DoT begins
└── Result: Locked down target takes full DoT duration
```

---

## Failure Modes

### Wrong Augment Equipped

```
Grimnir has [Piston Hammer] equipped, attempts Impaling Spike.
├── Augment Tag: [Blunt] (not [Piercing])
├── BLOCK: "Cannot use Impaling Spike. Requires [Piercing] augment."
├── GUI: Ability grayed out
└── Suggest: "Swap to [Serrated Claw] or [Injector Spike] at workbench."
```

### Root Chance Failed (Rank 2 Only)

```
Grimnir rolls for root at Rank 2.
├── Root Roll: d100 = 82 (>75, FAILED)
├── Damage: Still dealt
├── [Rooted]: NOT applied
└── Result: Damage dealt but no CC effect
```

### Target Root-Immune

```
Certain bosses or flying enemies resist [Rooted].
├── Attack: Hits
├── Damage: Dealt normally
├── Root: Resisted (immune)
└── Result: Reduced effectiveness, damage only
```

### Miss

```
Grimnir rolls MIGHT 2 vs. Defense 4 → 0 successes.
├── MISS: No damage, no root
├── Resources: Still consumed (40 Stamina + 25 Savagery)
├── Cooldown: Still triggered (3 turns)
└── Result: Complete waste of resources
```

---

## Synergies & Interactions

### Internal Synergies (Skar-Horde Tree)

- **Heretical Augmentation:** Requires [Piercing] tag augment ([Serrated Claw] or [Injector Spike])
- **Grievous Wound:** Root target → Apply DoT safely (target can't flee)
- **Savage Strike:** Build Savagery on immobilized target
- **Overcharged Piston Slam:** Root → Swap augment → Slam (multi-turn combo)

### External Synergies

- **Atgeir-wielder (Skewer):** Multiple root sources for guaranteed lockdown
- **Veiðimaðr:** Pin target for ranged execution
- **Alka-hestur:** Rooted target can't escape poison payload
- **Casters:** Immobilized targets easy to target with AoE

### Counter-Interactions

- **Teleportation Effects:** Some abilities bypass physical root
- **Root Immunity:** Certain bosses resist [Rooted]
- **Flying Enemies:** Often immune to ground-based roots

### Augment Requirements

| Augment | Tag | Impaling Spike |
| --- | --- | --- |
| [Serrated Claw] | [Piercing] | ✅ Valid |
| [Injector Spike] | [Piercing] | ✅ Valid |
| [Piston Hammer] | [Blunt] | ❌ Invalid |
| [Taser Gauntlet] | [Energy] | ❌ Invalid |

---

## Tactical Applications

1. **Pursuit Prevention:** Stop fleeing enemies—they can't escape the DoT chain
2. **Setup Tool:** Immobilize before Grievous Wound or ally burst damage
3. **Zone Control:** Pin dangerous enemies away from vulnerable party members
4. **Augment Build Path:** [Piercing] augment enables control-focused playstyle
5. **Combo Enabler:** Rank 3 +2 hit/damage makes follow-up attacks devastating

---

## v5.0 Compliance Notes

- **Augment Requirement:** Demonstrates build diversity through equipment gating
- **CC Scaling:** Foundation (75% chance, 2 turns) → Mastery (100%, 3 turns, +hit/damage)
- **Savagery Cost:** Moderate (25) for control utility
- **Tier 2 Structure:** R2→R3 progression (unlocks at 8 PP, Rank 3 with Capstone)
- **Augment System:** See [Weapon-Stump Augmentation System — Mechanic Specification v5.0](Weapon-Stump%20Augmentation%20System%20%E2%80%94%20Mechanic%20Specif%20c3328afcc82e442bb9680f41336f44d5.md) for gating details