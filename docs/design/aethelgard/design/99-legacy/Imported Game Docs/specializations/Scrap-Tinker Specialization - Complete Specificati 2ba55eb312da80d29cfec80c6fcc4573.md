# Scrap-Tinker Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-SCRAP-TINKER
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Scrap-Tinker specialization, including:

- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- **GUI display specifications per rank**
- Current implementation status
- Combat system integration points

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Data Seeding | `RuneAndRust.Persistence/DataSeeder.cs` (line 1805) | Implemented |
| Specialization Enum | `RuneAndRust.Core/Specialization.cs` | Defined |
| Specialization Factory | `RuneAndRust.Engine/SpecializationFactory.cs` | Referenced |
| Tests | N/A | Not Yet Implemented |

### Change Log

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | 2025-11-27 | Initial specification from DataSeeder implementation |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | ScrapTinker |
| **Display Name** | Scrap-Tinker |
| **Specialization ID** | 14 |
| **Archetype** | Adept (ArchetypeID = 2) |
| **Path Type** | Coherent |
| **Mechanical Role** | Crafter / Pet Controller |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | FINESSE |
| **Resource System** | Stamina + Scrap Materials |
| **Trauma Risk** | None |
| **Icon** | :wrench: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 3 | Early-game specialization |
| **Maximum Corruption** | 100 | No corruption restriction |
| **Minimum Corruption** | 0 | No minimum corruption |
| **Required Quest** | None | No quest prerequisite |

### 1.3 Design Philosophy

**Tagline**: "Salvage and innovation — craft gadgets, deploy drones, modify weapons"

**Core Fantasy**: You are the scavenger-engineer who sees treasure in ruins. Where others see broken machines, you see repurposable parts. You salvage corrupted technology, reverse-engineer pre-Glitch devices, and cobble together functional gadgets from scrap.

You craft drones for reconnaissance, bombs for crowd control, and weapon mods for allies. You're the tinkerer who proves that in a crashed system, the best debugger is the one who can rebuild from the ground up.

**Mechanical Identity**:

1. **Scrap Material Economy**: Collect and spend Scrap Materials to craft gadgets and deployables
2. **Gadget Deployment**: Flash bombs, shock mines, and other tactical devices
3. **Pet/Minion Control**: Scout Drones and the ultimate Scrap Golem
4. **Crafting Specialization**: Weapon modifications and superior quality items

### 1.4 The Scrap Materials System

**Scrap Materials** are the secondary resource for the Scrap-Tinker:

| Source | Scrap Gained |
| --- | --- |
| Defeated Mechanical enemies | +50-100% (by rank) |
| Loot containers | +50-100% (by rank) |
| Salvaging broken equipment | Variable |
| Automated Scavenging (post-combat) | 5-15 (by rank) |
| Scout Drone scavenging | +5 per combat |
| Scrap Golem scavenging | +10 per combat |
| Expedition start (Rank 3) | 20 Scrap |

**Scrap Costs**:

| Gadget | Base Cost | With Efficient Assembly |
| --- | --- | --- |
| Flash Bomb | Standard | Reduced |
| Shock Mine | Standard | Reduced |
| Scout Drone | 15 Scrap | 8-15 Scrap |
| Weapon Modification | 20-25 Scrap | 10-18 Scrap |
| Scrap Golem | 50 Scrap | 25-50 Scrap |

### 1.5 Quality Tiers

The Scrap-Tinker creates items of varying quality:

| Quality | Effect Bonus | Chance (by rank) |
| --- | --- | --- |
| **Standard** | Base effects | Default |
| **Masterwork** | Enhanced effects | 15% → 25% → 40% |
| **Prototype** | Superior effects | 0% → 0% → 10% |

### 1.6 Specialization Description (Full Text)

> You are the scavenger-engineer who sees treasure in ruins. Where others see broken machines, you see repurposable parts. You salvage corrupted technology, reverse-engineer pre-Glitch devices, and cobble together functional gadgets from scrap.
> 
> 
> You craft drones for reconnaissance, bombs for crowd control, and weapon mods for allies. You're the tinkerer who proves that in a crashed system, the best debugger is the one who can rebuild from the ground up.
> 

---

## 2. Rank Progression System

### 2.1 CRITICAL: Rank Unlock Rules

**Ranks are unlocked through TREE PROGRESSION, not PP spending.**

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
| --- | --- | --- | --- |
| **Tier 1** | Rank 1 (when learned) | Rank 2 (when 2 Tier 2 trained) | Capstone trained |
| **Tier 2** | Rank 2 (when learned) | Rank 3 (when Capstone trained) | Capstone trained |
| **Tier 3** | Rank 2 (when learned) | Rank 3 (when Capstone trained) | Capstone trained |
| **Capstone** | Rank 1 | Rank 2→3 (tree-based) | Full tree completion |

### 2.2 Ability Structure by Tier

| Tier | Abilities | PP Cost to Unlock | Starting Rank | Max Rank | Rank Progression |
| --- | --- | --- | --- | --- | --- |
| **Tier 1** | 3 | 3 PP each | 1 | 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| **Tier 2** | 3 | 4 PP each | 2 | 3 | 2→3 (Capstone) |
| **Tier 3** | 2 | 5 PP each | 2 | 3 | 2→3 (Capstone) |
| **Capstone** | 1 | 6 PP | 1 | 3 | 1→2→3 (tree-based) |

### 2.3 Total PP Investment

| Milestone | PP Spent | Abilities Unlocked | Tier 1 Rank | Tier 2 Rank |
| --- | --- | --- | --- | --- |
| Unlock Specialization | 3 PP | 0 | - | - |
| All Tier 1 | 3 + 9 = 12 PP | 3 Tier 1 | Rank 1 | - |
| 2 Tier 2 | 12 + 8 = 20 PP | 3 Tier 1 + 2 Tier 2 | **Rank 2** | Rank 2 |
| All Tier 2 | 20 + 4 = 24 PP | 3 Tier 1 + 3 Tier 2 | Rank 2 | Rank 2 |
| All Tier 3 | 24 + 10 = 34 PP | 3 Tier 1 + 3 Tier 2 + 2 Tier 3 | Rank 2 | Rank 2 |
| Capstone | 34 + 6 = 40 PP | All 9 abilities | **Rank 3** | **Rank 3** |

---

## 3. Ability Tree Overview

### 3.1 Visual Tree Structure

```
                    TIER 1: FOUNDATION (3 PP each, Ranks 1-3)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Master Scavenger]  [Deploy Flash Bomb]  [Salvage Expertise]
   (Passive)            (Active)            (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
              ════════════════════════
              RANK 2 UNLOCKS HERE
              (when 2 Tier 2 trained)
              ════════════════════════
                          │
                          ▼
                TIER 2: ADVANCED (4 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Deploy Scout Drone] [Deploy Shock Mine] [Weapon Modification]
      (Active)            (Active)            (Active)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
   [Automated            [Efficient Assembly]
    Scavenging]               (Passive)
     (Passive)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              ════════════════════════
              RANK 3 UNLOCKS HERE
              (when Capstone trained)
              ════════════════════════
                          │
                          ▼
              TIER 4: CAPSTONE (6 PP)
                          │
                 [Deploy Scrap Golem]
                      (Active)

```

### 3.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Resource Cost | Key Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1401 | Master Scavenger | 1 | Passive | 1→2→3 | None | +Scrap from enemies/containers |
| 1402 | Deploy Flash Bomb | 1 | Active | 1→2→3 | 30 Stamina | AoE [Blinded] |
| 1403 | Salvage Expertise | 1 | Passive | 1→2→3 | None | +Crafting, Masterwork chance |
| 1404 | Deploy Scout Drone | 2 | Active | 2→3 | 40 Stamina + 15 Scrap | Recon drone pet |
| 1405 | Deploy Shock Mine | 2 | Active | 2→3 | 35 Stamina | Trap: damage + stun |
| 1406 | Weapon Modification | 2 | Active | 2→3 | 20-25 Scrap | Permanent weapon enhancements |
| 1407 | Automated Scavenging | 3 | Passive | 2→3 | None | Auto-collect Scrap post-combat |
| 1408 | Efficient Assembly | 3 | Passive | 2→3 | None | Reduced costs, faster crafting |
| 1409 | Deploy Scrap Golem | 4 | Active | 1→2→3 | 50 Stamina + 50 Scrap | Powerful combat pet |

---

## 4. Tier 1 Abilities (Detailed Rank Specifications)

---

### 4.1 Master Scavenger (ID: 1401)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |

### Description

You see value where others see junk. Every bolt, every wire, every corroded gear—repurposable.

### Rank Details

### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:

- +1d10 bonus to scavenging Scrap Materials checks
- Find 50% more Scrap from defeated mechanical enemies
- Find 50% more Scrap from loot containers

**Formula**:

```
ScavengingBonus = 1d10
ScrapFromMechanical *= 1.50
ScrapFromContainers *= 1.50

```

**GUI Display**:

- Passive icon: Gear with magnifying glass
- Tooltip: "Master Scavenger (Rank 1): +1d10 scavenging. +50% Scrap from Mechanical enemies and containers."
- Color: Bronze border

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- +2d10 scavenging bonus
- Find 75% more Scrap
- **NEW**: Can salvage Scrap from broken weapons/armor (dismantle for materials)

**Formula**:

```
ScavengingBonus = 2d10
ScrapBonus = 1.75
CanDismantle = true

```

**GUI Display**:

- Tooltip: "Master Scavenger (Rank 2): +2d10 scavenging. +75% Scrap. Can dismantle broken equipment."
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- +3d10 scavenging bonus
- Find 100% more Scrap (double)
- Salvaged materials include rare components
- **NEW**: Start expeditions with 20 Scrap

**Formula**:

```
ScavengingBonus = 3d10
ScrapBonus = 2.00
SalvageIncludesRare = true
ExpeditionStartScrap = 20

```

**GUI Display**:

- Tooltip: "Master Scavenger (Rank 3): +3d10 scavenging. Double Scrap. Rare components. Start with 20 Scrap."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedScrapTinkerTier1()`
- [ ]  Economy: Scrap Materials resource system
- [ ]  Economy: Scavenge multiplier

---

### 4.2 Deploy Flash Bomb (ID: 1402)

**Type**: Active | **Action**: Standard Action | **Target**: Ground location (3x3 area)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 30 Stamina |
| **Cooldown** | 2 turns |
| **Attribute Used** | WITS |
| **Status Effect** | [Blinded] |

### Description

You lob the improvised device. Flash! Their optics overload, their eyes burn.

### Rank Details

### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:

- Throw Flash Bomb to target location
- All enemies in 3x3 area make WILL save DC 13
- Failed save: [Blinded] for 2 turns
- Consumes 1 Flash Bomb from inventory

**Formula**:

```
For each Enemy in 3x3 Area:
    If (Enemy.WILLSave < 13):
        Enemy.AddStatus("Blinded", Duration: 2)
ConsumeItem("FlashBomb", 1)

```

**GUI Display**:

- Ability button: Flashing grenade
- Tooltip: "Deploy Flash Bomb (Rank 1): 3x3 AoE. DC 13 WILL or [Blinded] 2 turns. Cost: 30 Stamina + 1 Flash Bomb"
- Color: Bronze border

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- Save DC: 15
- Blinded enemies also take -2 Defense
- Resource cost reduced to 25 Stamina

**Formula**:

```
SaveDC = 15
If (Blinded):
    DefensePenalty = -2
StaminaCost = 25

```

**GUI Display**:

- Tooltip: "Deploy Flash Bomb (Rank 2): DC 15. [Blinded] also -2 Defense. Cost: 25 Stamina"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Save DC: 17
- [Blinded] duration: 3 turns
- **[Masterwork Flash Bomb]**: Also deals 2d6 damage

**Formula**:

```
SaveDC = 17
BlindDuration = 3
If (Masterwork):
    Damage = Roll(2d6)

```

**GUI Display**:

- Tooltip: "Deploy Flash Bomb (Rank 3): DC 17. [Blinded] 3 turns. Masterwork: +2d6 damage."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedScrapTinkerTier1()`
- [ ]  Combat: Ground-targeted AoE
- [ ]  Inventory: Flash Bomb consumable

---

### 4.3 Salvage Expertise (ID: 1403)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |

### Description

Your understanding of pre-Glitch engineering is encyclopedic. Your work is precise, efficient, masterful.

### Rank Details

### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:

- +1d10 bonus to all Engineering crafting checks
- Crafted gadgets have 15% chance to be [Masterwork]

**Formula**:

```
EngineeringBonus = 1d10
MasterworkChance = 0.15

```

**GUI Display**:

- Passive icon: Blueprint with checkmark
- Tooltip: "Salvage Expertise (Rank 1): +1d10 Engineering. 15% Masterwork chance."
- Color: Bronze border

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- +2d10 crafting bonus
- Masterwork chance: 25%
- **NEW**: Crafting time reduced by 25%

**Formula**:

```
EngineeringBonus = 2d10
MasterworkChance = 0.25
CraftingTimeMultiplier = 0.75

```

**GUI Display**:

- Tooltip: "Salvage Expertise (Rank 2): +2d10 Engineering. 25% Masterwork. 25% faster crafting."
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- +3d10 crafting bonus
- Masterwork chance: 40%
- **NEW**: Can craft [Prototype] quality (10% chance, superior to Masterwork)

**Formula**:

```
EngineeringBonus = 3d10
MasterworkChance = 0.40
PrototypeChance = 0.10

```

**GUI Display**:

- Tooltip: "Salvage Expertise (Rank 3): +3d10 Engineering. 40% Masterwork, 10% Prototype."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedScrapTinkerTier1()`
- [ ]  Crafting: Quality tier system
- [ ]  Crafting: Bonus dice integration

---

## 5. Tier 2 Abilities (Rank 2→3 Progression)

---

### 5.1 Deploy Scout Drone (ID: 1404)

**Type**: Active | **Action**: Standard Action | **Target**: Self (deploys drone)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Scrap-Tinker tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 40 Stamina + 15 Scrap Materials |
| **Cooldown** | 4 turns |

### Description

The jerry-rigged drone buzzes to life. Its optics scan the battlefield, feeding you tactical data.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Deploy Scout Drone (15 HP, 2 Armor)
- Grants vision in 7x7 area
- Reveals hidden enemies and traps
- Moves 3 spaces per turn (your command)
- Duration: Until destroyed or dismissed
- **NEW**: Can mark priority targets (+1 ally to hit vs marked enemy)

**Formula**:

```
SpawnPet("ScoutDrone", HP: 15, Armor: 2)
Drone.VisionRadius = 7
Drone.MovementSpeed = 3
Drone.Ability("MarkTarget"): MarkedEnemy.AlliesHitBonus += 1
Duration = UNTIL_DESTROYED

```

**GUI Display**:

- Ability button: Flying drone
- Tooltip: "Deploy Scout Drone (Rank 2): 15 HP, 2 Armor. 7x7 vision. Mark targets: +1 ally hit. Cost: 40 Stamina, 15 Scrap"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Drone has 20 HP, 4 Armor
- Vision radius: 10x10
- **NEW**: Can self-destruct for 4d6 damage (3x3 AoE, destroys drone)

**Formula**:

```
Drone.HP = 20
Drone.Armor = 4
Drone.VisionRadius = 10
Drone.Ability("SelfDestruct"): AoE(3x3, Roll(4d6))

```

**GUI Display**:

- Tooltip: "Deploy Scout Drone (Rank 3): 20 HP, 4 Armor. 10x10 vision. Can self-destruct: 4d6 AoE."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedScrapTinkerTier2()`
- [ ]  Combat: Pet/Minion system
- [ ]  Combat: Vision fog-of-war
- [ ]  Combat: Target marking

---

### 5.2 Deploy Shock Mine (ID: 1405)

**Type**: Active | **Action**: Standard Action | **Target**: Ground location

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Scrap-Tinker tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 35 Stamina |
| **Cooldown** | None |
| **Damage Type** | Lightning |
| **Status Effect** | [Stunned], [Slowed] |

### Description

You carefully arm the mine. Step on it—instant overload. Nervous system fried.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Place Shock Mine at location (hidden to enemies)
- Trigger: Enemy moves onto mine
- Damage: 4d8 Lightning
- STURDINESS save DC 16 or [Stunned] 2 turns
- Can place 2 mines per combat

**Formula**:

```
PlaceTrap("ShockMine", Location)
OnEnemyTrigger:
    Damage = Roll(4d8, "Lightning")
    If (Enemy.STURDINESSSave < 16):
        Enemy.AddStatus("Stunned", Duration: 2)
MaxMinesPerCombat = 2

```

**GUI Display**:

- Ability button: Electrical mine
- Tooltip: "Deploy Shock Mine (Rank 2): 4d8 Lightning. DC 16 or [Stunned] 2 turns. Max 2/combat. Cost: 35 Stamina"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Damage: 5d8 Lightning
- Save DC: 18
- **[Masterwork Mine]**: Also applies [Slowed] for 2 turns after Stun ends

**Formula**:

```
Damage = Roll(5d8)
SaveDC = 18
If (Masterwork):
    OnStunEnd: Enemy.AddStatus("Slowed", Duration: 2)

```

**GUI Display**:

- Tooltip: "Deploy Shock Mine (Rank 3): 5d8 Lightning. DC 18. Masterwork: [Slowed] 2 turns after Stun."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedScrapTinkerTier2()`
- [ ]  Combat: Trap system
- [ ]  Combat: Hidden object detection

---

### 5.3 Weapon Modification (ID: 1406)

**Type**: Active | **Action**: Standard Action (out-of-combat) | **Target**: Single ally weapon

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Scrap-Tinker tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 20 Scrap Materials |
| **Requirement** | Workbench, out-of-combat |
| **Duration** | Permanent |

### Description

You disassemble the weapon, integrate salvaged components, reassemble. Better than factory spec.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Apply permanent modification (10 minutes at workbench):
    - **[Elemental]**: +2d6 Fire/Frost/Lightning damage
    - **[Precision]**: +2 to hit
    - **[Reinforced]**: +100% durability + 10% crit chance

**Formula**:

```
Requires: Workbench, OutOfCombat
ModOptions:
    Elemental: Damage += Roll(2d6, ChosenElement)
    Precision: HitBonus += 2
    Reinforced: Durability *= 2, CritChance += 0.10
ScrapCost = 20

```

**GUI Display**:

- Ability button: Wrench on weapon
- Tooltip: "Weapon Modification (Rank 2): [Elemental] +2d6, [Precision] +2 hit, or [Reinforced] +100% dur +10% crit. Cost: 20 Scrap"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- **NEW**: Can apply 2 modifications to same weapon (stacking)
- Prototype quality mods: bonus doubled

**Formula**:

```
MaxModsPerWeapon = 2
If (Prototype):
    ModBonus *= 2

```

**GUI Display**:

- Tooltip: "Weapon Modification (Rank 3): Can apply 2 mods. Prototype: bonuses doubled."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedScrapTinkerTier2()`
- [ ]  Crafting: Weapon modification system
- [ ]  Equipment: Mod slot system

---

## 6. Tier 3 Abilities (Rank 2→3 Progression)

---

### 6.1 Automated Scavenging (ID: 1407)

**Type**: Passive | **Action**: Free Action | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Scrap-Tinker tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | None (passive) |
| **Trigger** | After combat ends |

### Description

You've built automated collection systems. Magnets, sensors, retrieval claws—never leave materials behind.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- After combat: Auto-scavenge 10 Scrap Materials (no action required)
- **NEW**: Scout Drone can scavenge while deployed (+5 Scrap per combat)

**Formula**:

```
OnCombatEnd:
    ScrapMaterials += 10
If (ScoutDrone.Active):
    ScrapMaterials += 5

```

**GUI Display**:

- Passive icon: Magnetic collector
- Tooltip: "Automated Scavenging (Rank 2): +10 Scrap after combat. Scout Drone: +5 Scrap."
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Auto-scavenge 15 Scrap
- 25% chance to find rare components
- **NEW**: Scrap Golem (if active) scavenges additional 10 Scrap

**Formula**:

```
OnCombatEnd:
    ScrapMaterials += 15
    If (Random() < 0.25):
        AddRareComponent()
If (ScrapGolem.Active):
    ScrapMaterials += 10

```

**GUI Display**:

- Tooltip: "Automated Scavenging (Rank 3): +15 Scrap. 25% rare components. Golem: +10 Scrap."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedScrapTinkerTier3()`
- [ ]  Economy: Post-combat scavenging
- [ ]  Economy: Rare component drops

---

### 6.2 Efficient Assembly (ID: 1408)

**Type**: Passive | **Action**: Free Action | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Scrap-Tinker tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | None (passive) |

### Description

Muscle memory. Optimized workflows. You assemble gadgets faster than most people load a gun.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- All gadget crafting costs 40% less Scrap Materials
- Crafting time reduced by 75%
- **NEW**: Can craft 2 gadgets simultaneously

**Formula**:

```
ScrapCostMultiplier = 0.60
CraftingTimeMultiplier = 0.25
SimultaneousCrafts = 2

```

**GUI Display**:

- Passive icon: Assembly line
- Tooltip: "Efficient Assembly (Rank 2): -40% Scrap cost. -75% time. Craft 2 at once."
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Costs 50% less Scrap
- Some gadgets craftable instantly (Flash Bombs, Repair Kits)
- **NEW**: Can craft 3 gadgets simultaneously

**Formula**:

```
ScrapCostMultiplier = 0.50
InstantCraft = ["FlashBomb", "RepairKit"]
SimultaneousCrafts = 3

```

**GUI Display**:

- Tooltip: "Efficient Assembly (Rank 3): -50% Scrap cost. Flash Bombs instant. Craft 3 at once."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedScrapTinkerTier3()`
- [ ]  Crafting: Cost reduction system
- [ ]  Crafting: Parallel crafting

---

## 7. Capstone Ability

---

### 7.1 Deploy Scrap Golem (ID: 1409)

**Type**: Active | **Action**: Standard Action | **Target**: Self (deploys golem)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 4 (Capstone) |
| **PP Cost to Unlock** | 6 PP |
| **Prerequisite** | 24 PP invested + both Tier 3 abilities |
| **Ranks** | 1→2→3 |
| **Resource Cost** | 50 Stamina + 50 Scrap Materials |
| **Cooldown** | Once per expedition |
| **Assembly Time** | 1 hour (out-of-combat) |
| **Special** | Training this ability upgrades all Tier 1, 2, & 3 abilities to Rank 3 |

### Description

Your masterpiece. A walking junk pile animated by salvaged power cores. Loyal. Brutal. Yours.

### Rank Details

### Rank 1 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Deploy Scrap Golem (40 HP, 6 Armor, immune to psychic effects)
- Acts on your turn
- **Slam**: 3d10 Physical damage
- **Defend**: Grant adjacent ally +3 Soak
- Requires 50 Scrap Materials, 1 hour assembly (out-of-combat)
- Duration: Until destroyed or expedition ends

**Formula**:

```
SpawnPet("ScrapGolem", HP: 40, Armor: 6)
Golem.Immunity = ["Psychic"]
Golem.Ability("Slam"): Roll(3d10, "Physical")
Golem.Ability("Defend"): AdjacentAlly.Soak += 3
ScrapCost = 50
AssemblyTime = 1 hour
Duration = UNTIL_EXPEDITION_END

```

**GUI Display**:

- Ability button: Hulking scrap construct
- Tooltip: "Deploy Scrap Golem (Rank 1): 40 HP, 6 Armor. Slam: 3d10. Defend: +3 Soak to ally. Cost: 50 Stamina, 50 Scrap"
- Color: Bronze border

---

### Rank 2 (Unlocked: Based on tree progression)

**Mechanical Effect**:

- Golem: 60 HP, 8 Armor
- Slam: 4d10 damage
- **NEW**: Repair Protocol: Once per combat, self-heal 20 HP
- **NEW**: Can carry 50 extra Scrap capacity

**Formula**:

```
Golem.HP = 60
Golem.Armor = 8
Golem.Slam = Roll(4d10)
Golem.Ability("RepairProtocol"): Golem.HP += 20 (1/combat)
Golem.ScrapCapacity = 50

```

**GUI Display**:

- Tooltip: "Deploy Scrap Golem (Rank 2): 60 HP, 8 Armor. Slam: 4d10. Repair: +20 HP 1x/combat. +50 Scrap capacity."
- Color: Silver border

---

### Rank 3 (Unlocked: Full tree completion)

**Mechanical Effect**:

- Golem: 80 HP, 10 Armor
- Slam: 5d10 damage
- **NEW**: Detonate: Command golem to self-destruct (8d10 damage, 5x5 AoE, destroys golem)
- **NEW**: Can rebuild golem for 25 Scrap (half cost)

**Formula**:

```
Golem.HP = 80
Golem.Armor = 10
Golem.Slam = Roll(5d10)
Golem.Ability("Detonate"): AoE(5x5, Roll(8d10)), DestroyGolem()
RebuildCost = 25 Scrap

```

**GUI Display**:

- Tooltip: "Deploy Scrap Golem (Rank 3): 80 HP, 10 Armor. Slam: 5d10. Detonate: 8d10 AoE. Rebuild: 25 Scrap."
- Color: Gold border

### GUI Display - SCRAP GOLEM DEPLOYMENT

When deployed:

```
┌─────────────────────────────────────────┐
│         SCRAP GOLEM DEPLOYED            │
├─────────────────────────────────────────┤
│                                         │
│  HP: [████████████████████] 80/80       │
│  Armor: 10                              │
│  Immune: Psychic                        │
│                                         │
│  Actions:                               │
│  [SLAM] 5d10 Physical                   │
│  [DEFEND] +3 Soak to adjacent ally      │
│  [REPAIR] Heal 20 HP (1/combat)         │
│  [DETONATE] 8d10 AoE (destroys golem)   │
│                                         │
└─────────────────────────────────────────┘

```

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedScrapTinkerCapstone()`
- [ ]  Combat: Advanced pet system with multiple abilities
- [ ]  Combat: Self-destruct mechanic
- [ ]  Economy: Scrap carrying capacity

---

## 8. Status Effect Definitions

### 8.1 [Blinded]

| Property | Value |
| --- | --- |
| **Applied By** | Deploy Flash Bomb |
| **Duration** | 2-3 turns |
| **Icon** | Covered eyes |
| **Color** | White |

**Effects**:

- 4 to all attack rolls
- Cannot use abilities requiring sight
- At Rank 2+: -2 Defense

---

### 8.2 [Stunned]

| Property | Value |
| --- | --- |
| **Applied By** | Deploy Shock Mine |
| **Duration** | 1-2 turns |
| **Icon** | Stars/electricity |
| **Color** | Yellow-blue |

**Effects**:

- Cannot take actions
- 4 to Defense

---

### 8.3 [Slowed]

| Property | Value |
| --- | --- |
| **Applied By** | Deploy Shock Mine (Masterwork) |
| **Duration** | 2 turns |
| **Icon** | Weighted feet |
| **Color** | Blue |

**Effects**:

- Movement costs doubled
- Cannot use movement abilities

---

### 8.4 [Masterwork]

| Property | Value |
| --- | --- |
| **Applied To** | Crafted gadgets |
| **Duration** | Permanent |
| **Icon** | Star quality |
| **Color** | Silver |

**Effects**:

- Enhanced base effects (varies by gadget)
- Flash Bomb: +2d6 damage
- Shock Mine: Applies [Slowed] after Stun

---

### 8.5 [Prototype]

| Property | Value |
| --- | --- |
| **Applied To** | Crafted gadgets (Rank 3 only) |
| **Duration** | Permanent |
| **Icon** | Double star |
| **Color** | Gold |

**Effects**:

- Bonuses doubled compared to Masterwork
- Weapon mods: 2x normal bonus

---

## 9. GUI Requirements

### 9.1 Scrap Materials Display

```
┌─────────────────────────────────────────┐
│  SCRAP MATERIALS: 75 [████████░░] /100  │
│  :wrench: Scrap Golem Capacity: +50              │
└─────────────────────────────────────────┘

```

### 9.2 Deployed Pets Panel

```
┌─────────────────────────────────────────┐
│  DEPLOYED UNITS                         │
├─────────────────────────────────────────┤
│  :small_red_triangle: Scout Drone [████░░░░] 15/20 HP     │
│    • Vision: Active (10x10)             │
│    • [Mark Target] [Self-Destruct]      │
├─────────────────────────────────────────┤
│  :robot: Scrap Golem [████████████] 80/80 HP │
│    • Armor: 10 | Immune: Psychic        │
│    • [Slam] [Defend] [Repair] [Detonate]│
└─────────────────────────────────────────┘

```

### 9.3 Crafting Quality Indicators

| Quality | Border | Indicator |
| --- | --- | --- |
| Standard | Gray | None |
| Masterwork | Silver | :star: |
| Prototype | Gold | :star::star: |

### 9.4 Ability Card Rank Indicators

| Rank | Border Color | Badge |
| --- | --- | --- |
| 1 | Bronze (#CD7F32) | "I" |
| 2 | Silver (#C0C0C0) | "II" |
| 3 | Gold (#FFD700) | "III" |

---

## 10. Implementation Priority

### Phase 1: Critical (Foundation)

1. **Implement Scrap Materials resource** - Collection, storage, spending
2. **Implement crafting quality tiers** - Standard, Masterwork, Prototype
3. **Implement basic gadget inventory** - Flash Bombs, Shock Mines
4. **Implement rank calculation logic** based on tree progression

### Phase 2: Pet/Minion System

1. **Implement Scout Drone pet** - HP, movement, vision
2. **Implement pet command system** - Move, special abilities
3. **Implement target marking** (Scout Drone Rank 2)
4. **Implement self-destruct mechanic**

### Phase 3: Combat Integration

1. **Implement trap system** - Hidden placement, trigger detection
2. **Implement AoE ground targeting** (Flash Bomb)
3. **Route Scrap-Tinker abilities** through CombatEngine

### Phase 4: Crafting System

1. **Implement weapon modification** - Elemental, Precision, Reinforced
2. **Implement mod stacking** (Rank 3)
3. **Implement crafting time reduction**

### Phase 5: Scrap Golem

1. **Implement Scrap Golem pet** - Full stat block
2. **Implement multi-ability pets** - Slam, Defend, Repair, Detonate
3. **Implement expedition-long duration**

### Phase 6: Polish

1. **Add deployed pets panel** to CombatView
2. **Add Scrap Materials tracker** to UI
3. **Implement crafting quality indicators**

---

**End of Specification**