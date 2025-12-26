# Data Flow Documentation

Parent item: Technical Reference (Technical%20Reference%202ba55eb312da8079a291e020980301c1.md)

**Last Updated:** 2025-11-27
**Documentation Version:** v0.39

---

## Overview

This document describes how data flows through the Rune & Rust system during key game operations. Understanding these flows is essential for debugging, extending functionality, and maintaining system coherence.

---

## 1. Game Initialization Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                      APPLICATION STARTUP                            │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  STATIC SERVICE INSTANTIATION                                       │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │ Foundation → Single-Dep → Multi-Dep → Repository-Backed     │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  MAIN() ENTRY                                                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│  │ Serilog Init │→│ Command      │→│ Game Loop    │               │
│  │              │  │ Dispatcher   │  │ Start        │               │
│  └──────────────┘  └──────────────┘  └──────────────┘               │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  DATA LOADING                                                       │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ BiomeLibrary.LoadBiomes() ← Data/Biomes/*.json               │   │
│  │ TemplateLibrary.LoadTemplates() ← Data/Templates/*.json      │   │
│  │ NPCDatabase.Load() ← Data/NPCs/*.json                        │   │
│  │ QuestDatabase.Load() ← Data/Quests/*.json                    │   │
│  │ DialogueDatabase.Load() ← Data/Dialogues/*.json              │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  CHARACTER CREATION                                                 │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ CharacterFactory.CreateCharacter()                           │   │
│  │   ├── Base attributes assigned                               │   │
│  │   ├── AccountProgressionService.ApplyUnlocks()               │   │
│  │   ├── SpecializationFactory.ApplySpecialization()            │   │
│  │   └── AbilityService.AssignStartingAbilities()               │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  WORLD GENERATION                                                   │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ DungeonGenerator.GenerateComplete()                          │   │
│  │   ├── Generate graph structure                               │   │
│  │   ├── SpatialLayoutService.ConvertTo3D()                     │   │
│  │   ├── RoomInstantiator.Instantiate()                         │   │
│  │   └── PopulationPipeline.PopulateDungeon()                   │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
                        GAME LOOP READY

```

---

## 2. Combat Turn Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                      COMBAT INITIALIZATION                          │
└─────────────────────────────────────────────────────────────────────┘
                                │
    ┌───────────────────────────┴───────────────────────────┐
    │                                                       │
    ▼                                                       ▼
┌───────────────────┐                           ┌───────────────────┐
│ CombatEngine.     │                           │ CompanionService. │
│ InitializeCombat()│                           │ GetPartyCompanions│
└─────────┬─────────┘                           └─────────┬─────────┘
          │                                               │
          └───────────────────────┬───────────────────────┘
                                  │
                                  ▼
                    ┌─────────────────────────┐
                    │ GridInitializationService│
                    │ .InitializeGrid()       │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ RollInitiative()        │
                    │ DiceService.Roll() for  │
                    │ each participant        │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ Sort by initiative      │
                    │ (desc score, desc attr) │
                    └────────────┬────────────┘
                                 │
                                 ▼
                         COMBAT ACTIVE

┌─────────────────────────────────────────────────────────────────────┐
│                         TURN PROCESSING                             │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │ NextTurn()              │
                    └────────────┬────────────┘
                                 │
          ┌──────────────────────┼──────────────────────┐
          │                      │                      │
          ▼                      ▼                      ▼
┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐
│ Environmental   │   │ Status Effect   │   │ Stamina         │
│ Hazard Check    │   │ Tick            │   │ Regeneration    │
│ (HazardService) │   │ (StatusService) │   │                 │
└────────┬────────┘   └────────┬────────┘   └────────┬────────┘
         │                     │                     │
         └─────────────────────┼─────────────────────┘
                               │
                               ▼
              ┌────────────────────────────────┐
              │ Determine Current Participant  │
              └────────────────┬───────────────┘
                               │
          ┌────────────────────┼────────────────────┐
          │                    │                    │
          ▼                    ▼                    ▼
    ┌──────────┐        ┌──────────┐        ┌──────────┐
    │ Player   │        │ Enemy    │        │ Companion│
    │ Turn     │        │ Turn     │        │ Turn     │
    └────┬─────┘        └────┬─────┘        └────┬─────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ Await Player    │  │ EnemyAI.        │  │ CompanionAI.    │
│ Input           │  │ DetermineAction │  │ DetermineAction │
└─────────────────┘  └─────────────────┘  └─────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                      PLAYER ATTACK FLOW                             │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │ PlayerAttack(target)    │
                    └────────────┬────────────┘
                                 │
    ┌────────────────────────────┼────────────────────────────┐
    │                            │                            │
    ▼                            ▼                            ▼
┌───────────────┐      ┌───────────────┐      ┌───────────────┐
│ Equipment     │      │ Flanking      │      │ Status        │
│ Service       │      │ Service       │      │ Bonuses       │
│ (weapon stats)│      │ (positioning) │      │ (Analyzed,etc)│
└───────┬───────┘      └───────┬───────┘      └───────┬───────┘
        │                      │                      │
        └──────────────────────┼──────────────────────┘
                               │
                               ▼
                    ┌─────────────────────────┐
                    │ Calculate Attack Dice   │
                    │ = attribute + bonuses   │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ DiceService.Roll()      │
                    │ Count successes (5-6)   │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ Check for Fumble        │
                    │ (0 successes + 1s)      │
                    └────────────┬────────────┘
                                 │
          ┌──────────────────────┴──────────────────────┐
          │ Not Fumble                                  │ Fumble
          ▼                                             ▼
┌─────────────────────────┐               ┌─────────────────────────┐
│ Calculate Defense       │               │ ProcessAttackFumble()   │
│ ├── Target Sturdiness   │               │ (end turn, penalties)   │
│ ├── Cover Bonus         │               └─────────────────────────┘
│ ├── High Ground Bonus   │
│ └── Flanking Penalty    │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ DiceService.Roll()      │
│ (defense roll)          │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Net Successes =         │
│ attack - defense        │
└────────────┬────────────┘
             │
      ┌──────┴──────┐
      │ > 0         │ = 0 (miss)
      ▼             ▼
┌──────────┐  ┌──────────────┐
│ Calculate│  │ Miss/Deflect │
│ Damage   │  │ Flavor Text  │
└────┬─────┘  └──────────────┘
     │
     ▼
┌─────────────────────────┐
│ Roll Weapon Damage Dice │
│ + Stance Bonus          │
│ + Critical (2x dice)    │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Apply Defense Reduction │
│ (if target defending)   │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Apply Damage to Target  │
│ target.HP -= damage     │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ CombatFlavorTextService │
│ .GenerateAttackText()   │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Check Target Defeated   │
│ (!target.IsAlive)       │
└────────────┬────────────┘
             │
      ┌──────┴──────┐
      │ Yes         │ No
      ▼             ▼
┌──────────────┐  Continue
│ TerritoryServ│
│ .RecordKill()│
└──────────────┘

```

---

## 3. Dungeon Generation Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                  DUNGEON GENERATION PIPELINE                        │
└─────────────────────────────────────────────────────────────────────┘

PHASE 1: GRAPH GENERATION
─────────────────────────
                    ┌─────────────────────────┐
                    │ Generate(seed, biome)   │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ Initialize RNG(seed)    │
                    └────────────┬────────────┘
                                 │
    ┌────────────────────────────┼────────────────────────────┐
    │                            │                            │
    ▼                            ▼                            ▼
┌───────────────┐      ┌───────────────┐      ┌───────────────┐
│ Generate Main │      │ Add Branching │      │ Add Secret    │
│ Path          │      │ Paths (60%)   │      │ Rooms (30%)   │
│ (Entrance→Exit)      │ (1-3 branches)│      │               │
└───────┬───────┘      └───────┬───────┘      └───────┬───────┘
        │                      │                      │
        └──────────────────────┼──────────────────────┘
                               │
                               ▼
                    ┌─────────────────────────┐
                    │ Calculate Node Depths   │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ DirectionAssigner       │
                    │ .AssignDirections()     │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ Validate Connectivity   │
                    └────────────┬────────────┘
                                 │
                                 ▼
                           DungeonGraph

PHASE 2: SPATIAL LAYOUT (v0.39.1)
─────────────────────────────────
                    ┌─────────────────────────┐
                    │ SpatialLayoutService    │
                    │ .ConvertGraphTo3DLayout │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ Assign 3D Coordinates   │
                    │ (x, y, elevation)       │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ Generate Vertical       │
                    │ Connections (stairs)    │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ SpatialValidationService│
                    │ .ValidateSector()       │
                    └────────────┬────────────┘
                                 │
                                 ▼
                      3D Positions + Connections

PHASE 3: ROOM INSTANTIATION
───────────────────────────
                    ┌─────────────────────────┐
                    │ RoomInstantiator        │
                    │ .Instantiate()          │
                    └────────────┬────────────┘
                                 │
                                 ▼
              ┌──────────────────────────────────────┐
              │ For each DungeonNode:                │
              │   ├── Create Room object             │
              │   ├── Apply template properties      │
              │   ├── Set position from 3D layout    │
              │   └── Link to adjacent rooms         │
              └──────────────────┬───────────────────┘
                                 │
                                 ▼
                              Dungeon

PHASE 4: POPULATION (v0.11)
───────────────────────────
                    ┌─────────────────────────┐
                    │ PopulationPipeline      │
                    │ .PopulateDungeon()      │
                    └────────────┬────────────┘
                                 │
    ┌────────────────────────────┼────────────────────────────┐
    │                            │                            │
    ▼                            ▼                            ▼
┌───────────────┐      ┌───────────────┐      ┌───────────────┐
│ Calculate     │      │ Classify Room │      │ Distribute    │
│ Global Budget │      │ Densities     │      │ Budget        │
└───────┬───────┘      └───────┬───────┘      └───────┬───────┘
        │                      │                      │
        └──────────────────────┼──────────────────────┘
                               │
                               ▼
              ┌──────────────────────────────────────┐
              │ For each non-handcrafted Room:       │
              │   1. ConditionApplier.Apply()        │
              │   2. HazardSpawner.PopulateRoom()    │
              │   3. TerrainSpawner.PopulateRoom()   │
              │   4. DormantProcessSpawner.Populate()│
              │   5. LootSpawner.PopulateRoom()      │
              └──────────────────┬───────────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ CoherentGlitchRuleEngine│
                    │ .ApplyRulesToDungeon()  │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ ThreatHeatmapService    │
                    │ .GenerateHeatmap()      │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │ ValidatePopulation()    │
                    └────────────┬────────────┘
                                 │
                                 ▼
                       Populated Dungeon

```

---

## 4. Save/Load Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                          SAVE FLOW                                  │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────┐
│ User Requests Save      │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ SaveRepository          │
│ .SaveGame()             │
└────────────┬────────────┘
             │
    ┌────────┴────────┐
    │                 │
    ▼                 ▼
┌──────────┐    ┌──────────────┐
│ Character│    │ World State  │
│ Data     │    │ Data         │
└────┬─────┘    └──────┬───────┘
     │                 │
     ▼                 ▼
┌──────────────────────────────────────────┐
│ Serialized to Database                   │
│ ├── Characters table                     │
│ ├── Inventory table                      │
│ ├── WorldState table                     │
│ ├── QuestProgress table                  │
│ ├── FactionReputation table              │
│ └── TerritoryControl table               │
└──────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                          LOAD FLOW                                  │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────┐
│ User Requests Load      │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ SaveRepository          │
│ .LoadGame()             │
└────────────┬────────────┘
             │
             ▼
┌──────────────────────────────────────────┐
│ Deserialize from Database                │
│ ├── Restore PlayerCharacter              │
│ ├── Restore Inventory                    │
│ ├── Restore World State                  │
│ ├── Restore Quest Progress               │
│ └── Restore Faction Data                 │
└──────────────────┬───────────────────────┘
                   │
                   ▼
┌─────────────────────────┐
│ Regenerate Dungeon      │
│ from stored seed        │
│ (deterministic)         │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Apply saved world state │
│ (cleared rooms, killed  │
│ enemies, looted items)  │
└────────────┬────────────┘
             │
             ▼
        Game Restored

```

---

## 5. Trauma/Stress Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                      STRESS ACCUMULATION                            │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────┐
│ Stress-Inducing Event   │
│ (combat, horror, etc.)  │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ TraumaEconomyService    │
│ .AddStress()            │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Check Resolve Check?    │
└────────────┬────────────┘
             │
      ┌──────┴──────┐
      │ Yes         │ No
      ▼             │
┌──────────────┐    │
│ DiceService  │    │
│ .Roll(WILL)  │    │
└──────┬───────┘    │
       │            │
       ▼            │
┌──────────────┐    │
│ Reduce by    │    │
│ successes    │    │
└──────┬───────┘    │
       │            │
       └──────┬─────┘
              │
              ▼
┌─────────────────────────┐
│ Apply Trauma Multipliers│
│ (existing traumas may   │
│ increase stress gain)   │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Add to Current Stress   │
│ (clamp 0-100)           │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Check for Breaking Point│
│ (stress >= 100?)        │
└────────────┬────────────┘
             │
      ┌──────┴──────┐
      │ Yes         │ No
      ▼             ▼
┌──────────────┐   Done
│ BREAKING     │
│ POINT        │
└──────┬───────┘
       │
       ▼
┌─────────────────────────┐
│ TraumaLibrary           │
│ .SelectTraumaForSource()│
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Acquire Trauma          │
│ (add to character)      │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Apply Trauma Effects    │
│ (stat penalties, etc.)  │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Reset Stress to 60      │
│ (not 0 - still rattled) │
└─────────────────────────┘

```

---

## 6. Territory Control Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                    TERRITORY CONTROL UPDATE                         │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────┐
│ Player Action in Sector │
│ (kill enemy, complete   │
│ quest, etc.)            │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ TerritoryService        │
│ .RecordAction()         │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ TerritoryControlService │
│ .UpdateControl()        │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Calculate New Control % │
│ for each faction        │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Check Control Threshold │
│ (>70% = faction control)│
└────────────┬────────────┘
             │
      ┌──────┴──────┐
      │ Changed     │ No Change
      ▼             ▼
┌──────────────┐   Done
│ WorldEvent   │
│ Service      │
│ .Trigger()   │
└──────┬───────┘
       │
       ▼
┌─────────────────────────┐
│ Update Faction Relations│
│ - ReputationService     │
│ - FactionWarService     │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Companion Reactions     │
│ (CompanionTerritory     │
│ Reactions)              │
└─────────────────────────┘

```

---

## Cross-References

- [Service Architecture](https://www.notion.so/service-architecture.md) - Service organization
- [System Integration Map](https://www.notion.so/system-integration-map.md) - Cross-system interactions
- [Combat Resolution](https://www.notion.so/01-systems/combat-resolution.md) - Combat mechanics detail

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27