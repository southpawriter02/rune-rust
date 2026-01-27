# System Integration Map

**Last Updated:** 2025-11-27
**Documentation Version:** v0.39

---

## Overview

This document maps the integration points between major game systems in Rune & Rust. Understanding these connections is essential for modifying existing systems or adding new features.

---

## System Dependency Matrix

| System | Depends On | Depended By |
|--------|------------|-------------|
| **Combat** | Dice, Equipment, Status, Trauma, Territory | Progression, Quest |
| **Generation** | Template, Biome, Population | World State |
| **Progression** | Dice, Repository | Combat, Quest |
| **Economy** | Currency, Pricing | Quest, Merchant, Loot |
| **Territory** | Repository, Reputation | Combat, Companion, Quest |
| **Companion** | AI, Repository, Territory | Combat |
| **Quest** | Currency, Territory | Progression |
| **Biome** | Environmental, Dice | Generation, Combat |

---

## Integration Diagrams

### Combat ↔ Progression

```
┌─────────────────────────────────────────────────────────────────────┐
│                   COMBAT ↔ PROGRESSION                              │
└─────────────────────────────────────────────────────────────────────┘

                    ┌─────────────────┐
                    │  CombatEngine   │
                    └────────┬────────┘
                             │
    ┌────────────────────────┼────────────────────────┐
    │                        │                        │
    ▼                        ▼                        ▼
┌─────────┐          ┌─────────────┐          ┌─────────────┐
│ Enemy   │          │  XP/Legend  │          │  Trauma     │
│ Defeated│          │  Award      │          │  Economy    │
└────┬────┘          └──────┬──────┘          └──────┬──────┘
     │                      │                        │
     │                      ▼                        ▼
     │               ┌─────────────┐          ┌─────────────┐
     │               │ Progression │          │ Breaking    │
     │               │ Service     │          │ Point Check │
     │               └──────┬──────┘          └──────┬──────┘
     │                      │                        │
     │                      ▼                        ▼
     │               ┌─────────────┐          ┌─────────────┐
     │               │ Level Up?   │          │ Acquire     │
     │               │ PP Award    │          │ Trauma?     │
     │               └─────────────┘          └─────────────┘
     │
     └──────────────────────┐
                            │
                            ▼
                    ┌─────────────────┐
                    │ TerritoryService│
                    │ .RecordKill()   │
                    └─────────────────┘
```

**Integration Points:**
- `CombatEngine.AwardCombatLegend()` → ProgressionService
- `CombatEngine` → `TraumaEconomyService.AddStress()` (combat stress)
- Enemy defeat → `TerritoryService.RecordEnemyKill()`

---

### Combat ↔ Territory

```
┌─────────────────────────────────────────────────────────────────────┐
│                    COMBAT ↔ TERRITORY                               │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────┐                      ┌─────────────────────┐
│   CombatEngine      │                      │  TerritoryService   │
└──────────┬──────────┘                      └──────────┬──────────┘
           │                                            │
           │  ┌─────────────────────────────────────┐   │
           │  │         INTEGRATION EVENTS          │   │
           │  └─────────────────────────────────────┘   │
           │                                            │
           ├──► Enemy Killed ──────────────────────────►├── Update faction control
           │                                            │
           ├──► Room Cleared ─────────────────────────►├── Mark territory cleared
           │                                            │
           ◄──────────────────────────── Territory Buff ◄── Apply faction bonuses
           │                                            │
           ◄──────────────────────── Reinforcement Check◄── Spawn faction allies?
           │                                            │
           └────────────────────────────────────────────┘

Data Flow:
  Combat → Territory:
    - RecordEnemyKillForTerritory(enemy, player)
    - faction, location, timestamp

  Territory → Combat:
    - GetTerritoryBonuses(location) → accuracy/damage buffs
    - CheckReinforcementSpawn(faction, location) → ally spawns
```

---

### Combat ↔ Companion

```
┌─────────────────────────────────────────────────────────────────────┐
│                    COMBAT ↔ COMPANION                               │
└─────────────────────────────────────────────────────────────────────┘

INITIALIZATION:
┌─────────────────────┐         ┌─────────────────────┐
│ CombatEngine.       │────────►│ CompanionService.   │
│ InitializeCombat()  │         │ GetPartyCompanions()│
└─────────────────────┘         └──────────┬──────────┘
                                           │
                                           ▼
                                ┌─────────────────────┐
                                │ List<Companion>     │
                                │ added to CombatState│
                                └─────────────────────┘

TURN PROCESSING:
┌─────────────────────┐         ┌─────────────────────┐
│ CombatEngine.       │────────►│ CompanionAIService. │
│ ProcessCompanionTurn│         │ DetermineAction()   │
└─────────────────────┘         └──────────┬──────────┘
                                           │
                                           ▼
                                ┌─────────────────────┐
                                │ Execute companion   │
                                │ action (attack,     │
                                │ support, etc.)      │
                                └─────────────────────┘

DAMAGE/RECOVERY:
┌─────────────────────┐         ┌─────────────────────┐
│ CombatEngine.       │────────►│ Companion.HP        │
│ DamageCompanion()   │         │ update              │
└─────────────────────┘         └─────────────────────┘

┌─────────────────────┐         ┌─────────────────────┐
│ CombatEngine.       │────────►│ Companion recovery  │
│ RecoverCompanions() │         │ post-combat         │
└─────────────────────┘         └─────────────────────┘
```

---

### Generation ↔ Population

```
┌─────────────────────────────────────────────────────────────────────┐
│                   GENERATION ↔ POPULATION                           │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│  DungeonGenerator                                                   │
│  .GenerateComplete()                                                │
│         │                                                           │
│         ▼                                                           │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐              │
│  │   Generate  │───►│ Spatial     │───►│ Room        │              │
│  │   Graph     │    │ Layout      │    │ Instantiate │              │
│  └─────────────┘    └─────────────┘    └──────┬──────┘              │
│                                               │                     │
│                                               ▼                     │
│                                      ┌─────────────────┐            │
│                                      │ PopulationPipeline│           │
│                                      │ .PopulateDungeon()│           │
│                                      └────────┬────────┘            │
│                                               │                     │
│         ┌─────────────────────────────────────┼──────────────────┐  │
│         │                 │                   │                  │  │
│         ▼                 ▼                   ▼                  ▼  │
│  ┌────────────┐   ┌────────────┐   ┌────────────┐   ┌────────────┐  │
│  │ Condition  │   │  Hazard    │   │  Terrain   │   │   Enemy    │  │
│  │ Applier    │   │  Spawner   │   │  Spawner   │   │  Spawner   │  │
│  └─────┬──────┘   └─────┬──────┘   └─────┬──────┘   └─────┬──────┘  │
│        │                │                │                │         │
│        └────────────────┼────────────────┼────────────────┘         │
│                         │                │                          │
│                         ▼                ▼                          │
│                  ┌─────────────────────────────┐                    │
│                  │ CoherentGlitchRuleEngine    │                    │
│                  │ (post-processing rules)     │                    │
│                  └─────────────────────────────┘                    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

Data Dependencies:
  Generation → Population:
    - Dungeon with instantiated Room objects
    - BiomeDefinition with element tables
    - Seeded Random for determinism

  Population → Generation:
    - None (one-way dependency)
```

---

### Territory ↔ Faction

```
┌─────────────────────────────────────────────────────────────────────┐
│                    TERRITORY ↔ FACTION                              │
└─────────────────────────────────────────────────────────────────────┘

                    ┌─────────────────┐
                    │TerritoryService │ (Orchestrator)
                    └────────┬────────┘
                             │
    ┌────────────────────────┼────────────────────────┐
    │                        │                        │
    ▼                        ▼                        ▼
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│ Territory   │      │ FactionWar  │      │ Reputation  │
│ Control     │◄────►│ Service     │◄────►│ Service     │
│ Service     │      │             │      │             │
└──────┬──────┘      └──────┬──────┘      └──────┬──────┘
       │                    │                    │
       │                    │                    │
       ▼                    ▼                    ▼
┌─────────────────────────────────────────────────────────┐
│                    SHARED STATE                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐      │
│  │ Sector      │  │ War State   │  │ Player      │      │
│  │ Control %   │  │ (active     │  │ Reputation  │      │
│  │ per faction │  │ conflicts)  │  │ per faction │      │
│  └─────────────┘  └─────────────┘  └─────────────┘      │
└─────────────────────────────────────────────────────────┘

Interactions:
  Territory → Faction:
    - Control change triggers faction war events
    - High control enables faction-specific quests

  Faction → Territory:
    - Reputation affects territory bonus strength
    - Allied factions may assist in territory control
```

---

### Quest ↔ Multiple Systems

```
┌─────────────────────────────────────────────────────────────────────┐
│                    QUEST INTEGRATION HUB                            │
└─────────────────────────────────────────────────────────────────────┘

                         ┌─────────────────┐
                         │   QuestService  │
                         └────────┬────────┘
                                  │
    ┌─────────────────────────────┼─────────────────────────────┐
    │              │              │              │              │
    ▼              ▼              ▼              ▼              ▼
┌────────┐   ┌────────┐   ┌────────┐   ┌────────┐   ┌────────┐
│Combat  │   │Currency│   │Territory│  │Dialogue│   │Progress│
│Engine  │   │Service │   │Service │   │Service │   │Service │
└────┬───┘   └────┬───┘   └────┬───┘   └────┬───┘   └────┬───┘
     │            │            │            │            │
     │            │            │            │            │
     ▼            ▼            ▼            ▼            ▼
┌─────────────────────────────────────────────────────────────────┐
│                     QUEST EVENTS                                │
├─────────────────────────────────────────────────────────────────┤
│  • Enemy killed (combat) ──────► Update kill objectives         │
│  • Item acquired (loot) ───────► Update collection objectives   │
│  • Area explored (movement) ───► Update exploration objectives  │
│  • NPC dialogue (dialogue) ────► Trigger quest stages           │
│  • Territory captured ─────────► Complete territory objectives  │
│  • Quest complete ─────────────► Award XP, currency, reputation │
└─────────────────────────────────────────────────────────────────┘
```

---

### Biome ↔ Combat/Environment

```
┌─────────────────────────────────────────────────────────────────────┐
│                    BIOME INTEGRATION                                │
└─────────────────────────────────────────────────────────────────────┘

                    ┌─────────────────┐
                    │ BiomeDefinition │
                    └────────┬────────┘
                             │
    ┌────────────────────────┼────────────────────────┐
    │                        │                        │
    ▼                        ▼                        ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ [Biome]Service  │  │ Environmental   │  │ Generation      │
│ (Muspelheim,    │  │ Combat Service  │  │ Services        │
│ Niflheim, etc.) │  │                 │  │                 │
└────────┬────────┘  └────────┬────────┘  └────────┬────────┘
         │                    │                    │
         ▼                    ▼                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                   BIOME EFFECTS                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  MUSPELHEIM (Heat):                                             │
│    • IntenseHeatService → damage per turn                       │
│    • BrittlenessService → equipment degradation                 │
│                                                                 │
│  NIFLHEIM (Cold):                                               │
│    • FrigidColdService → movement penalties                     │
│    • SlipperyTerrainService → fall/slip chance                  │
│                                                                 │
│  ALFHEIM (Reality):                                             │
│    • RunicInstabilityService → spell misfires                   │
│    • RealityTearService → random teleports                      │
│                                                                 │
│  JOTUNHEIM (Corruption):                                        │
│    • JotunCorpseTerrainService → corruption damage              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Integration Checklist for New Features

### Adding a New System

When adding a new system, consider integration with:

- [ ] **Combat** - Does it affect combat? (buffs, debuffs, actions)
- [ ] **Progression** - Does it grant XP or progression?
- [ ] **Economy** - Does it involve currency or items?
- [ ] **Territory** - Does it affect territory control?
- [ ] **Quest** - Can it be a quest objective?
- [ ] **Save/Load** - Does state need persistence?
- [ ] **UI** - How is it displayed to the player?

### Integration Pattern Template

```csharp
// 1. Define integration interface (if bidirectional)
public interface IMySystemIntegration
{
    void OnMyEvent(MyEventArgs args);
}

// 2. Add optional dependency to consuming service
public class ConsumingService
{
    private readonly IMySystemIntegration? _mySystemIntegration;

    public ConsumingService(
        RequiredService required,
        IMySystemIntegration? mySystemIntegration = null) // Optional
    {
        _mySystemIntegration = mySystemIntegration;
    }

    public void DoSomething()
    {
        // Null-safe integration call
        _mySystemIntegration?.OnMyEvent(new MyEventArgs());
    }
}

// 3. Register in Program.cs initialization
private static MySystemService _mySystemService = new();
private static ConsumingService _consumingService = new(
    _requiredService,
    _mySystemService); // Wire up integration
```

---

## Cross-References

- [Service Architecture](./service-architecture.md) - Service organization and dependencies
- [Data Flow Documentation](./data-flow.md) - How data moves through systems
- [Individual Service Docs](./services/) - Detailed service documentation

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27
