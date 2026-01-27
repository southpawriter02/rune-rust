# Weapon-Stump Augmentation System — Mechanic Specification v5.0

Type: Mechanic
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-SKARHORDEAUGMENTATION-v5.0
Parent item: Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0 (Skar-Horde%20Aspirant%20(Augmented%20Brawler)%20%E2%80%94%20Speciali%20dcff21d0a06040698381b59039deaf60.md)
Proof-of-Concept Flag: No
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Validated: No

## I. Core Identity

| Attribute | Value |
| --- | --- |
| **Parent Specialization** | Skar-Horde Aspirant |
| **System Type** | Equipment Subsystem |
| **Unlocked By** | Heretical Augmentation (Tier 1 Passive) |
| **Replaces** | Main Hand Weapon Slot |
| **Primary Attribute** | MIGHT |
| **Crafting Skill** | Tinkering |

### Design Philosophy

The Augmentation System is the mechanical heart of the Skar-Horde Aspirant's identity. It represents their **permanent** choice to sacrifice humanity for power — once the ritual is performed, there is no going back. The weapon-stump isn't a prosthetic born of necessity; it's a deliberate grafting of brutal functionality onto scarred flesh.

This system serves three design goals:

1. **Pre-Combat Strategy:** Scout encounters, return to workbench, equip optimal augment
2. **Build Diversity:** Different augments enable different ability branches
3. **Progression Path:** Augment quality replaces weapon upgrades for power scaling

---

## II. Slot Replacement Mechanics

### A. The Ritual of Replacement

**Trigger:** Learning the Heretical Augmentation passive ability

**Resolution Pipeline:**

```
1. VALIDATE: Character has Skar-Horde Aspirant specialization unlocked
2. EXECUTE: Main Hand weapon slot → DISABLED (permanent)
3. CREATE: [Augmentation] slot → ENABLED
4. UNEQUIP: Any weapon currently in Main Hand → Inventory
5. GRANT: Basic [Serrated Claw] augment (starter equipment)
6. FLAG: Character.HasAugmentationSlot = true
```

**Permanence:**

- This transformation is **irreversible**
- Character can never equip standard weapons in Main Hand again
- Off-hand slot remains functional (shields, tools, etc.)
- Two-handed weapons become permanently unavailable

### B. Equipment Slot Schema

**Before Augmentation:**

```
Equipment Slots:
├── Main Hand: [Weapon]
├── Off Hand: [Shield/Tool/Weapon]
├── Armor: [Body]
├── Accessory: [Trinket]
└── ... (other slots)
```

**After Augmentation:**

```
Equipment Slots:
├── Augmentation: [Weapon-Stump] ← REPLACED
├── Off Hand: [Shield/Tool/Weapon]
├── Armor: [Body]
├── Accessory: [Trinket]
└── ... (other slots)
```

---

## III. Augment Types

### A. Core Augment Registry

| Augment | Tag | Damage Type | Base Die | Unlocks | Special Property |
| --- | --- | --- | --- | --- | --- |
| **[Serrated Claw]** | [Piercing] | Piercing/Slashing | d8 | Impaling Spike | +[Bleeding] on crit |
| **[Piston Hammer]** | [Blunt] | Bludgeoning | d10 | Overcharged Piston Slam | [Armor Piercing] property |
| **[Injector Spike]** | [Piercing] | Piercing + Poison | d6 | Toxin abilities (future) | +[Poisoned] on hit |
| **[Taser Gauntlet]** | [Energy] | Lightning | d8 | Shock abilities (future) | +[Shocked] on crit |

### B. Augment Properties Schema

Each augment has the following properties:

```
Augment {
    Name: string              // Display name
    Tag: enum                 // [Piercing], [Blunt], [Energy]
    DamageType: enum          // Physical subtype or Energy
    BaseDie: dice             // d6, d8, d10, d12
    Quality: enum             // Crude, Standard, Refined, Optimized
    SpecialProperty: effect   // On-hit or on-crit bonus
    AbilityUnlocks: ability[] // Abilities requiring this tag
    CraftingTier: int         // Tinkering DC to create
    HorrorRating: int         // Horrific Form DC modifier
}
```

### C. Damage Type Inheritance

**Core Rule:** Skar-Horde abilities inherit damage type from equipped augment.

**Resolution Pipeline:**

```
1. ABILITY triggers (e.g., Savage Strike)
2. CHECK: Character.EquippedAugment
3. INHERIT: Ability.DamageType = Augment.DamageType
4. INHERIT: Ability.BaseDie = Augment.BaseDie (for variable-damage abilities)
5. APPLY: Augment.SpecialProperty (if trigger condition met)
```

**Worked Example — Savage Strike with [Serrated Claw]:**

```
Grimnir uses Savage Strike
├── Equipped Augment: [Serrated Claw]
├── Damage Type: Piercing/Slashing (inherited)
├── Base Damage: 2d8 + MIGHT (d8 from Claw)
├── Roll: 2d8[6,4] + MIGHT[4] = 14 Piercing damage
├── Savagery Generated: +15
└── Crit Check: 3 successes (not crit) → No [Bleeding]
```

**Worked Example — Savage Strike with [Piston Hammer]:**

```
Grimnir uses Savage Strike
├── Equipped Augment: [Piston Hammer]
├── Damage Type: Bludgeoning (inherited)
├── Base Damage: 2d10 + MIGHT (d10 from Hammer)
├── Roll: 2d10[8,7] + MIGHT[4] = 19 Bludgeoning damage
├── Savagery Generated: +15
└── [Armor Piercing]: Target Soak reduced by 2 for this attack
```

---

## IV. Quality Tiers

### A. Quality Progression

| Quality | Die Upgrade | Bonus | Tinkering DC | Material Cost |
| --- | --- | --- | --- | --- |
| **Crude** | Base -1 step | None | DC 2 | Scrap only |
| **Standard** | Base die | None | DC 3 | Common materials |
| **Refined** | Base +1 step | +1 damage | DC 4 | Rare materials |
| **[Optimized]** | Base +2 steps | +2 damage, special | DC 5 | Exotic + Scrap-Tinker |

**Die Progression:** d4 → d6 → d8 → d10 → d12

**Worked Example — Quality Scaling:**

```
[Serrated Claw] (Base d8):
├── Crude:    d6, no bonus
├── Standard: d8, no bonus
├── Refined:  d10, +1 damage
└── Optimized: d12, +2 damage, [Bleeding] on normal hit (not just crit)
```

### B. [Optimized] Augments

[Optimized] quality augments can only be crafted by **Scrap-Tinker** specialists. They provide:

- Maximum die upgrade (+2 steps)
- Flat damage bonus (+2)
- Enhanced special property (e.g., [Bleeding] on any hit, not just crit)
- Unique visual effects (glowing runes, steam vents, etc.)

---

## V. Crafting & Swapping

### A. Crafting Resolution Pipeline

**Location Requirement:** Workbench (cannot craft in field)

```
CRAFT AUGMENT:
1. VALIDATE: Character is at Workbench
2. VALIDATE: Character has required materials
3. VALIDATE: Character has Tinkering skill (or Scrap-Tinker for Optimized)
4. ROLL: Tinkering check vs. Quality DC
5. IF success:
   a. CONSUME: Materials
   b. CREATE: Augment of target quality
   c. ADD: To inventory
6. IF failure:
   a. CONSUME: 50% materials (wasted)
   b. OUTPUT: "Crafting failed — augment components ruined"
```

**Material Requirements:**

| Quality | Metal Scrap | Mechanical Parts | Special Component |
| --- | --- | --- | --- |
| Crude | 3 | 1 | — |
| Standard | 5 | 3 | — |
| Refined | 8 | 5 | 1 Rare Component |
| Optimized | 12 | 8 | 1 Exotic Component + Scrap-Tinker |

### B. Swap Resolution Pipeline

**Standard Swap (Rank 1 Heretical Augmentation):**

```
SWAP AUGMENT:
1. VALIDATE: Character is at Workbench
2. VALIDATE: Character has target augment in inventory
3. COST: 2 Standard Actions
4. UNEQUIP: Current augment → Inventory
5. EQUIP: Target augment → [Augmentation] slot
6. UPDATE: All Skar-Horde ability damage types
7. OUTPUT: "[Old Augment] removed. [New Augment] installed."
```

**Quick Swap (Rank 2 Heretical Augmentation):**

```
QUICK SWAP AUGMENT:
1. VALIDATE: Character is at Workbench
2. VALIDATE: Character has target augment in inventory
3. COST: 1 Standard Action (reduced from 2)
4. ... (same as above)
```

**Field Swap (Rank 3 Heretical Augmentation):**

```
FIELD SWAP AUGMENT:
1. VALIDATE: Character has Field Swap available (1/long rest)
2. VALIDATE: Character has target augment in inventory
3. COST: 2 Standard Actions
4. ... (same as above)
5. CONSUME: Field Swap use
```

---

## VI. Ability Gating

### A. Augment Tag Requirements

Certain abilities require specific augment tags to use:

| Ability | Required Tag | Valid Augments |
| --- | --- | --- |
| Impaling Spike | [Piercing] | [Serrated Claw], [Injector Spike] |
| Overcharged Piston Slam | [Blunt] | [Piston Hammer] |
| Toxin Injection (future) | [Poison] | [Injector Spike] |
| Shock Discharge (future) | [Energy] | [Taser Gauntlet] |

### B. Gating Resolution Pipeline

```
ABILITY USE ATTEMPT:
1. CHECK: Ability.RequiredTag
2. IF RequiredTag is null:
   a. PROCEED (ability has no augment requirement)
3. IF RequiredTag exists:
   a. CHECK: Character.EquippedAugment.Tag
   b. IF Tag matches RequiredTag:
      i. PROCEED
   c. IF Tag does NOT match:
      i. BLOCK: "Cannot use [Ability]. Requires [Tag] augment."
      ii. GUI: Ability button grayed out with tooltip
```

**Worked Example — Impaling Spike Blocked:**

```
Grimnir attempts Impaling Spike
├── Required Tag: [Piercing]
├── Equipped Augment: [Piston Hammer] (Tag: [Blunt])
├── Tag Match: NO
└── Result: BLOCKED — "Cannot use Impaling Spike. Requires [Piercing] augment."
```

---

## VII. Horrific Form Integration

### A. Horror Rating

Each augment has a Horror Rating that affects Horrific Form's Fear DC:

| Augment | Horror Rating | Fear DC Modifier | Rationale |
| --- | --- | --- | --- |
| [Piston Hammer] | Low | +0 (DC 3) | Brutal but comprehensible |
| [Serrated Claw] | High | +1 (DC 4) | Predatory, animalistic |
| [Injector Spike] | High | +1 (DC 4) | Medical horror, violation |
| [Taser Gauntlet] | Medium | +0 (DC 3) | Technological, impersonal |

### B. Horror Resolution Pipeline

```
HORRIFIC FORM TRIGGER:
1. TRIGGER: Aspirant hit by melee attack
2. CALCULATE: Base Fear chance (25%/35%/50% by rank)
3. ROLL: Random check vs. Fear chance
4. IF Fear triggers:
   a. CALCULATE: DC = 3 + Augment.HorrorRating
   b. ENEMY ROLL: WILL check vs. DC
   c. IF enemy fails:
      i. APPLY: [Feared] status
      ii. IF Rank 3: Aspirant gains +5 Savagery
```

---

## VIII. GUI Display Requirements

### A. Character Sheet — Augmentation Slot

```
┌─────────────────────────────────────────────┐
│  AUGMENTATION                               │
├─────────────────────────────────────────────┤
│  [🦾] Serrated Claw (Refined)               │
│  ─────────────────────────────────────────  │
│  Damage: d10 Piercing/Slashing (+1)         │
│  Tag: [Piercing]                            │
│  Special: [Bleeding] on critical hit        │
│  ─────────────────────────────────────────  │
│  Unlocks: Impaling Spike                    │
│  [SWAP] (1 Action at Workbench)             │
└─────────────────────────────────────────────┘
```

### B. Combat HUD — Augment Indicator

```
┌─────────────────────────────────────────────┐
│  [🦾 Serrated Claw]  Tag: [Piercing]        │
│  Damage: Piercing/Slashing                  │
└─────────────────────────────────────────────┘
```

### C. Ability Button States

**Enabled (Tag Matches):**

- Normal ability icon
- Full color
- Tooltip shows ability details

**Disabled (Tag Mismatch):**

- Grayed out icon
- Red "X" overlay or lock icon
- Tooltip: "Requires [Tag] augment. Currently equipped: [Current Tag]"

### D. Workbench Interface

```
┌─────────────────────────────────────────────┐
│  AUGMENTATION WORKBENCH                     │
├─────────────────────────────────────────────┤
│  Currently Equipped: [Serrated Claw]        │
│  ─────────────────────────────────────────  │
│  Available Augments:                        │
│  [🔨] Piston Hammer (Standard)    [SWAP]    │
│  [💉] Injector Spike (Crude)      [SWAP]    │
│  ─────────────────────────────────────────  │
│  Craft New Augment:                         │
│  [Serrated Claw ▼] [Standard ▼] [CRAFT]     │
│  Materials: 5 Scrap, 3 Parts                │
└─────────────────────────────────────────────┘
```

---

## IX. Edge Cases & Failure Modes

### A. Equipment Conflicts

**Scenario:** Player attempts to equip two-handed weapon after augmentation.

```
Resolution:
├── BLOCK: "Cannot equip [Two-Handed Weapon]. Your [Augmentation] slot replaces your Main Hand."
└── Suggest: "Consider Off-hand weapons or shields instead."
```

**Scenario:** Player attempts to remove augment without replacement.

```
Resolution:
├── ALLOW: Augment can be unequipped
├── WARNING: "Without an augment, you cannot use Skar-Horde abilities."
└── Default: Savage Strike deals unarmed damage (1d4 + MIGHT)
```

### B. Crafting Failures

**Scenario:** Tinkering check fails during crafting.

```
Resolution:
├── CONSUME: 50% of materials
├── OUTPUT: "Crafting failed. The augment components were ruined."
└── NO augment created
```

**Scenario:** Insufficient materials for crafting.

```
Resolution:
├── BLOCK: "Insufficient materials. Need: [List]. Have: [List]."
└── Cannot initiate craft
```

### C. Swap Edge Cases

**Scenario:** Attempting swap without Workbench (Rank 1-2).

```
Resolution:
├── BLOCK: "Must be at a Workbench to swap augments."
└── Suggest: "Find a settlement or camp with crafting facilities."
```

**Scenario:** Field swap attempted with no uses remaining.

```
Resolution:
├── BLOCK: "Field swap already used. Recharges after long rest."
└── Suggest: "Return to a Workbench or complete a long rest."
```

### D. Ability Gating Edge Cases

**Scenario:** Augment swapped mid-combat, invalidating queued ability.

```
Resolution:
├── CANCEL: Queued ability that no longer meets requirements
├── OUTPUT: "[Ability] cancelled. [Required Tag] augment no longer equipped."
└── Refund: Any Stamina/Savagery pre-committed
```

---

## X. Systemic Integration

### A. Trauma Economy Interface

- Augmentation itself does **not** directly generate Stress
- Stress generation comes from **The Price of Power** passive (Savagery → Stress)
- Thematic connection: Self-mutilation is the price of entry, ongoing Stress is the price of use

### B. Crafting System Interface

- Augments use **Tinkering** skill for crafting
- **Scrap-Tinker** specialization can craft [Optimized] quality
- **Einbúi** specialization can craft augment components in the field
- Material economy ties into scavenging and dungeon rewards

### C. Combat System Interface

- Damage type inheritance affects enemy resistances/vulnerabilities
- [Armor Piercing] property on [Piston Hammer] interacts with Soak calculation
- Status effects ([Bleeding], [Poisoned], [Shocked]) follow standard Status Effect System

---

## XI. Related Documents

- **Parent Specialization:** Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0
- **Gateway Ability:** Tier 1 Ability: Heretical Augmentation
- **Gated Abilities:** Tier 2 Ability: Impaling Spike, Tier 3 Ability: Overcharged Piston Slam
- **Crafting System:** Tinkering Skill Specification (TBD)
- **Status Effects:** SPEC-COMBAT-003: Status Effects System