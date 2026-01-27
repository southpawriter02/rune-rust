# Rune & Rust - Specification Index

> **Version:** 0.5.3
> **Last Updated:** 2025-12-25
> **Total Specifications:** 58
> **Maintained By:** The Architect

This document serves as the master index for all game system specifications. Each specification provides comprehensive documentation including behaviors, restrictions, limitations, use cases, and cross-system dependencies.

---

## Quick Navigation

| Domain | Specs | Directory | Description |
|--------|-------|-----------|-------------|
| [Core](#core-infrastructure) | 2 | [`core/`](./core/) | Foundational systems (dice, orchestration) |
| [Combat](#combat-systems) | 9 | [`combat/`](./combat/) | Combat mechanics, abilities, enemies, intent |
| [Character](#character--progression) | 8 | [`character/`](./character/) | Character stats, advancement, trauma |
| [Exploration](#exploration--world) | 6 | [`exploration/`](./exploration/) | Navigation, dungeon generation, ambush |
| [Environment](#environment-systems) | 2 | [`environment/`](./environment/) | Hazards, ambient conditions |
| [Economy](#economy--items) | 4 | [`economy/`](./economy/) | Inventory, crafting, loot |
| [Knowledge](#knowledge--lore) | 4 | [`knowledge/`](./knowledge/) | Codex, data captures, journals, library |
| [Content](#content-generation) | 3 | [`content/`](./content/) | Descriptor engine, templates, localization |
| [UI](#ui--rendering) | 9 | [`ui/`](./ui/) | User interface, rendering, visual effects, settings, crash handling |
| [Data](#data--persistence) | 4 | [`data/`](./data/) | Save system, repositories, migrations |
| [Tools](#developer-tools) | 5 | [`tools/`](./tools/) | Audit, documentation generation, debug console, cheats, testing |
| [Meta](#meta-specifications) | 2 | [`./`](./) | Specification workflow, governance |

---

## Meta Specifications

Specifications about specifications - governance, workflows, and standards.

| Spec ID | Title | Scope | Description |
|---------|-------|-------|-------------|
| [SPEC-WORKFLOW-001](./SPEC-WORKFLOW-001.md) | Specification Writing Workflow | All specs | Deep dive, creation, update, and validation workflows |
| [_SPEC-TEMPLATE](./_SPEC-TEMPLATE.md) | Specification Template | All specs | Copy this template when creating new specifications |

> **AI Agents:** Start with [SPEC-WORKFLOW-001](./SPEC-WORKFLOW-001.md) for all specification work. It contains Quick Start instructions, the template link, and complete workflows.

---

## Core Infrastructure

Foundational systems that other domains depend on.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-DICE-001](./core/SPEC-DICE-001.md) | Dice Pool System | `DiceService` | D10 dice pool mechanics, success counting, probability |
| [SPEC-GAME-001](./core/SPEC-GAME-001.md) | Game Orchestration | `GameService` | Main loop, phase management, state coordination |

---

## Combat Systems

All combat-related mechanics including attacks, abilities, enemies, and status effects.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-COMBAT-001](./combat/SPEC-COMBAT-001.md) | Combat System | `CombatService` | Core combat loop, turn structure, action flow |
| [SPEC-ABILITY-001](./combat/SPEC-ABILITY-001.md) | Ability System | `AbilityService` | Active abilities, resource costs, effects |
| [SPEC-ATTACK-001](./combat/SPEC-ATTACK-001.md) | Attack Resolution | `AttackResolutionService` | Hit calculation, damage, defense |
| [SPEC-STATUS-001](./combat/SPEC-STATUS-001.md) | Status Effects | `StatusEffectService` | Buffs, debuffs, duration tracking |
| [SPEC-AI-001](./combat/SPEC-AI-001.md) | Enemy AI & Behavior | `EnemyAIService` | High-level AI patterns, archetype behaviors |
| [SPEC-ENEMY-001](./combat/SPEC-ENEMY-001.md) | Enemy AI System | `EnemyAIService` | Specific implementations, threat assessment |
| [SPEC-ENEMYFAC-001](./combat/SPEC-ENEMYFAC-001.md) | Enemy Factory | `EnemyFactory` | Enemy creation, scaling, templates |
| [SPEC-TRAIT-001](./combat/SPEC-TRAIT-001.md) | Creature Traits | `CreatureTraitService` | Creature traits, special abilities |
| [SPEC-INTENT-001](./combat/SPEC-INTENT-001.md) | Enemy Intent System | `CombatService` | WITS-based intent visibility, telegraph icons |

---

## Character & Progression

Character creation, stats, advancement, and psychological systems.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-CHAR-001](./character/SPEC-CHAR-001.md) | Character System | `StatCalculationService` | Core stats, derived values, lineage/archetype |
| [SPEC-ADVANCEMENT-001](./character/SPEC-ADVANCEMENT-001.md) | Character Advancement | `CharacterService` | Level progression, stat improvements |
| [SPEC-XP-001](./character/SPEC-XP-001.md) | Experience & Leveling | `XPService` | XP rewards, level thresholds |
| [SPEC-LEGEND-001](./character/SPEC-LEGEND-001.md) | Legend Points | `LegendService` | Meta-currency, special abilities |
| [SPEC-TRAUMA-001](./character/SPEC-TRAUMA-001.md) | Trauma & Stress | `TraumaService` | Psychological damage, breaking points |
| [SPEC-CORRUPT-001](./character/SPEC-CORRUPT-001.md) | Corruption System | `CorruptionService` | Corruption tiers, mutations |
| [SPEC-RESOURCE-001](./character/SPEC-RESOURCE-001.md) | Resource Management | `ResourceService` | Stamina, Aether, regeneration |
| [SPEC-REST-001](./character/SPEC-REST-001.md) | Rest & Recovery | `RestService` | Wilderness rest, recovery mechanics |

---

## Exploration & World

Navigation, dungeon generation, and world interaction systems.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-NAV-001](./exploration/SPEC-NAV-001.md) | Navigation System | `NavigationService` | Room traversal, movement, exits |
| [SPEC-DUNGEON-001](./exploration/SPEC-DUNGEON-001.md) | Dungeon Generation | `DungeonGenerator` | Procedural room generation, layouts |
| [SPEC-ENVPOP-001](./exploration/SPEC-ENVPOP-001.md) | Environment Population | `EnvironmentPopulator` | Biome-based spawning, population |
| [SPEC-SPAWN-001](./exploration/SPEC-SPAWN-001.md) | Object Spawning | `ObjectSpawner` | Interactable object placement |
| [SPEC-INTERACT-001](./exploration/SPEC-INTERACT-001.md) | Interaction System | `InteractionService` | Object interaction, examination |
| [SPEC-AMBUSH-001](./exploration/SPEC-AMBUSH-001.md) | Ambush Risk System | `AmbushService` | Wilderness rest ambush mechanics |

---

## Environment Systems

Environmental effects, hazards, and ambient conditions.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-HAZARD-001](./environment/SPEC-HAZARD-001.md) | Dynamic Hazards | `HazardService` | Environmental dangers, triggers |
| [SPEC-COND-001](./environment/SPEC-COND-001.md) | Ambient Conditions | `ConditionService` | Weather, atmospheric effects |

---

## Economy & Items

Inventory management, crafting, loot generation, and item systems.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-INV-001](./economy/SPEC-INV-001.md) | Inventory & Equipment | `InventoryService` | Item storage, equipment slots |
| [SPEC-CRAFT-001](./economy/SPEC-CRAFT-001.md) | Crafting System | `CraftingService` | Recipe-based crafting, quality |
| [SPEC-REPAIR-001](./economy/SPEC-REPAIR-001.md) | Repair & Salvage | `BodgingService` | Item repair, salvaging |
| [SPEC-LOOT-001](./economy/SPEC-LOOT-001.md) | Loot Generation | `LootService` | Loot tables, drop rates |

---

## Knowledge & Lore

Codex system, data captures, and journal display.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-CODEX-001](./knowledge/SPEC-CODEX-001.md) | Scavenger's Journal | `CodexService` | Lore entries, fragment collection |
| [SPEC-CAPTURE-001](./knowledge/SPEC-CAPTURE-001.md) | Data Capture | `DataCaptureService` | Lore fragment generation |
| [SPEC-JOURNAL-001](./knowledge/SPEC-JOURNAL-001.md) | Journal Display | `JournalService` | Journal UI, text formatting |
| [SPEC-LIBRARY-001](./knowledge/SPEC-LIBRARY-001.md) | Library Service | `LibraryService` | Reflection-based Field Guide generation |

---

## Content Generation

Dynamic text generation, template systems, and localization.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-DESC-001](./content/SPEC-DESC-001.md) | Descriptor Engine | `DescriptorEngine` | Dynamic room/item descriptions |
| [SPEC-LOC-001](./content/SPEC-LOC-001.md) | Localization System | `LocalizationService` | Multi-language support, type-safe keys |
| [SPEC-TEMPLATE-001](./content/SPEC-TEMPLATE-001.md) | Template System | `TemplateLoaderService` | Biome/room templates, token substitution |

---

## UI & Rendering

User interface framework, rendering pipeline, and input handling.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-UI-001](./ui/SPEC-UI-001.md) | UI Framework | `UIService` | Avalonia UI architecture |
| [SPEC-RENDER-001](./ui/SPEC-RENDER-001.md) | Rendering Pipeline | `RenderService` | Terminal rendering, colors |
| [SPEC-INPUT-001](./ui/SPEC-INPUT-001.md) | Input Handling | `InputService` | Keyboard input, command parsing |
| [SPEC-THEME-001](./ui/SPEC-THEME-001.md) | Theme System | `ThemeService` | Color themes, styling |
| [SPEC-TRANSITION-001](./ui/SPEC-TRANSITION-001.md) | Screen Transitions | `ScreenTransitionService` | Phase transition animations |
| [SPEC-SETTINGS-001](./ui/SPEC-SETTINGS-001.md) | Settings Persistence | `SettingsService` | JSON-based user preferences persistence |
| [SPEC-OPTIONS-001](./ui/SPEC-OPTIONS-001.md) | Options Menu | `OptionsController` | Tabbed options UI with key rebinding |
| [SPEC-VISUAL-001](./ui/SPEC-VISUAL-001.md) | Visual Effects | `VisualEffectService` | Border flash effects, ReduceMotion accessibility |
| [SPEC-CRASH-001](./ui/SPEC-CRASH-001.md) | Crash Handling | `CrashService`, `CrashScreenRenderer` | Global exception handler, crash reports |

---

## Data & Persistence

Database access, save system, and data migrations.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-SAVE-001](./data/SPEC-SAVE-001.md) | Save/Load System | `SaveManager` | Game state persistence, slots |
| [SPEC-REPO-001](./data/SPEC-REPO-001.md) | Repository Pattern | `GenericRepository<T>` | Data access patterns, EF Core |
| [SPEC-SEED-001](./data/SPEC-SEED-001.md) | Database Seeding | `Seeders` | Initial data population |
| [SPEC-MIGRATE-001](./data/SPEC-MIGRATE-001.md) | Migration System | `EF Core Migrations` | Schema versioning |

---

## Developer Tools

Audit and validation tools for game balance testing.

| Spec ID | Title | Primary Service(s) | Description |
|---------|-------|-------------------|-------------|
| [SPEC-AUDIT-001](./tools/SPEC-AUDIT-001.md) | Audit Framework | `LootAuditService`, `CombatAuditService` | Monte Carlo simulation for economy/combat validation |
| [SPEC-DOCGEN-001](./tools/SPEC-DOCGEN-001.md) | Documentation Generator | `DocGenService` | CLI Markdown export for Field Guide |
| [SPEC-DEBUG-001](./tools/SPEC-DEBUG-001.md) | Debug Console | `DebugConsoleService` | Quake-style console overlay for developer commands |
| [SPEC-CHEAT-001](./tools/SPEC-CHEAT-001.md) | Cheat Commands | `CheatService` | God Mode, teleportation, full heal, map reveal |
| [SPEC-JOURNEY-001](./tools/SPEC-JOURNEY-001.md) | E2E Journey Testing | `TestGameHost`, `ScriptedInputHandler` | Integration test framework with deterministic seeding |

---

## Cross-System Dependencies

```
                    ┌─────────────────────────────────────────────────────────────┐
                    │                      SPEC-GAME-001                          │
                    │                   (Game Orchestration)                       │
                    └────────────────────────────┬────────────────────────────────┘
                                                 │
          ┌──────────────────┬───────────────────┼───────────────────┬──────────────────┐
          │                  │                   │                   │                  │
          ▼                  ▼                   ▼                   ▼                  ▼
   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
   │ COMBAT-001   │  │  REST-001    │  │  NAV-001     │  │ INTERACT-001 │  │  SAVE-001    │
   │   Combat     │  │    Rest      │  │  Navigation  │  │ Interaction  │  │    Save      │
   └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘
          │                 │                  │                 │                 │
    ┌─────┴─────┐     ┌─────┴─────┐     ┌─────┴─────┐     ┌─────┴─────┐     ┌─────┴─────┐
    │           │     │           │     │           │     │           │     │           │
    ▼           ▼     ▼           ▼     ▼           ▼     ▼           ▼     ▼           ▼
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│ABILITY │ │ ENEMY  │ │RESOURCE│ │TRAUMA  │ │HAZARD  │ │ COND   │ │  INV   │ │ REPO   │
│  001   │ │  001   │ │  001   │ │  001   │ │  001   │ │  001   │ │  001   │ │  001   │
└────┬───┘ └────┬───┘ └────────┘ └────────┘ └────────┘ └────────┘ └────┬───┘ └────────┘
     │          │                                                      │
     └──────────┴───────────────────────┬──────────────────────────────┘
                                        │
                                        ▼
                               ┌──────────────┐
                               │  DICE-001    │
                               │ (Foundation) │
                               └──────────────┘
```

---

## Dependency Matrix

| Spec | Depends On | Depended By |
|------|-----------|-------------|
| **SPEC-DICE-001** | *(none)* | Combat, Ability, Rest, Hazard, Condition, Craft, Repair, Loot |
| **SPEC-COMBAT-001** | DICE, STATUS, ABILITY | GAME |
| **SPEC-ABILITY-001** | DICE, RESOURCE, STATUS | COMBAT |
| **SPEC-REST-001** | RESOURCE, INV, TRAUMA | GAME |
| **SPEC-NAV-001** | HAZARD, COND | GAME |
| **SPEC-CRAFT-001** | DICE, INV | GAME |
| **SPEC-REPAIR-001** | DICE, INV | GAME |
| **SPEC-LOOT-001** | INV, DICE | COMBAT, INTERACT |
| **SPEC-CAPTURE-001** | CODEX | INTERACT, COMBAT |
| **SPEC-JOURNAL-001** | CODEX, CAPTURE | UI |
| **SPEC-LOC-001** | *(none)* | UI, Terminal |
| **SPEC-TRANSITION-001** | THEME | GAME |
| **SPEC-SETTINGS-001** | *(none)* | OPTIONS, GAME |
| **SPEC-OPTIONS-001** | SETTINGS, THEME, INPUT, LOC | GAME |
| **SPEC-VISUAL-001** | THEME, SETTINGS | COMBAT |
| **SPEC-AUDIT-001** | LOOT, COMBAT | *(developer tool)* |
| **SPEC-LIBRARY-001** | *(none)* | JOURNAL, DOCGEN |
| **SPEC-DOCGEN-001** | LIBRARY | *(developer tool)* |
| **SPEC-REPO-001** | *(none)* | SAVE, SEED, MIGRATE |
| **SPEC-SAVE-001** | REPO | GAME, CRASH |
| **SPEC-CRASH-001** | SAVE | GAME |
| **SPEC-AMBUSH-001** | DICE, REST | REST, COMBAT |
| **SPEC-INTENT-001** | DICE, STATUS, AI | COMBAT |
| **SPEC-DEBUG-001** | *(none)* | CHEAT, GAME |
| **SPEC-CHEAT-001** | DEBUG, COMBAT, TRAUMA, REST | GAME |
| **SPEC-JOURNEY-001** | DICE, SAVE, GAME | *(developer tool)* |

---

## Specification Status

| Status | Count | Percentage |
|--------|-------|------------|
| Implemented | 44 | 94% |
| Scaffolded | 2 | 4% |
| Planned | 1 | 2% |

### Planned Specifications

| Spec ID | Title | Priority | Notes |
|---------|-------|----------|-------|
| SPEC-LEGEND-001 | Legend Points | MEDIUM | Meta-currency, special abilities |

### Scaffolded Specifications

| Spec ID | Title | Notes |
|---------|-------|-------|
| SPEC-ADVANCEMENT-001 | Character Advancement | Level progression system |
| SPEC-XP-001 | Experience & Leveling | XP rewards, level thresholds |

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.5.3 | 2025-12-25 | Added SPEC-JOURNEY-001 E2E Journey Testing Framework |
| 0.5.2 | 2025-12-25 | Added SPEC-CHEAT-001 Cheat Command System |
| 0.5.1 | 2025-12-25 | Added SPEC-DEBUG-001 Debug Console System |
| 0.5.0 | 2025-12-25 | Added SPEC-INTENT-001 Enemy Intent & Telegraph System |
| 0.4.9 | 2025-12-25 | Added SPEC-AMBUSH-001 Ambush Risk System |
| 0.4.8 | 2025-12-25 | Added SPEC-CRASH-001 Global Exception Handler & Crash Reporting |
| 0.4.7 | 2025-12-25 | Added SPEC-VISUAL-001 Visual Effects System |
| 0.4.6 | 2025-12-25 | Added SPEC-SETTINGS-001 Settings Persistence, SPEC-OPTIONS-001 Options Menu |
| 0.4.5 | 2025-12-25 | Added SPEC-LIBRARY-001 Library Service, SPEC-DOCGEN-001 Documentation Generator |
| 0.4.4 | 2025-12-25 | Added SPEC-AUDIT-001 Audit Framework; created Developer Tools section |
| 0.4.3 | 2025-12-25 | Added SPEC-TRANSITION-001 Screen Transition System |
| 0.4.2 | 2025-12-24 | Added SPEC-LOC-001 Localization System |
| 0.4.0 | 2025-12-22 | Reorganized into domain subdirectories; Added 26 new specs |
| 0.3.3c | 2025-12-21 | Initial specification documentation (15 specs) |

---

## Architecture Overview

```
┌────────────────────────────────────────────────────────────────────────────┐
│                              TERMINAL LAYER                                │
│  ┌─────────────────┐  ┌───────────────────┐  ┌─────────────────────────┐   │
│  │ CombatRenderer  │  │ RestScreenRenderer│  │ CharacterCreationCtrl   │   │
│  └────────┬────────┘  └─────────┬─────────┘  └───────────┬─────────────┘   │
└───────────┼─────────────────────┼────────────────────────┼─────────────────┘
            │                     │                        │
┌───────────┼─────────────────────┼────────────────────────┼─────────────────┐
│           ▼                     ▼                        ▼   ENGINE LAYER  │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │                          GameService                               │    │
│  │  (Main Orchestrator - Manages GameState, Phase Transitions)        │    │
│  └────────────────────────────────────────────────────────────────────┘    │
│           │                     │                        │                 │
│  ┌────────┴──────┐     ┌───────┴───────┐      ┌─────────┴─────────┐        │
│  │ CombatService │     │  RestService  │      │  NavigationService│        │
│  └───────┬───────┘     └───────┬───────┘      └─────────┬─────────┘        │
│          │                     │                        │                  │
│  ┌───────┴───────────────────────────────────────────────────────────┐     │
│  │  SUPPORTING SERVICES                                              │     │
│  │  ┌───────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────────┐ │     │
│  │  │ TraumaSvc     │ │ HazardSvc   │ │ ConditionSvc│ │ AbilitySvc │ │     │
│  │  └───────────────┘ └─────────────┘ └─────────────┘ └────────────┘ │     │
│  │  ┌───────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────────┐ │     │
│  │  │ InventorySvc│ │ │ LootSvc     │ │ AmbushSvc   │ │ ResourceSvc│ │     │
│  │  └───────────────┘ └─────────────┘ └─────────────┘ └────────────┘ │     │
│  └───────────────────────────────────────────────────────────────────┘     │
│                                    │                                       │
│  ┌─────────────────────────────────┴─────────────────────────────────┐     │
│  │  FOUNDATIONAL SERVICES                                            │     │
│  │  ┌───────────────┐ ┌─────────────────────┐ ┌───────────────────┐  │     │
│  │  │  DiceService  │ │ StatCalculationSvc  │ │ StatusEffectService│ │     │
│  │  └───────────────┘ └─────────────────────┘ └───────────────────┘  │     │
│  └───────────────────────────────────────────────────────────────────┘     │
└────────────────────────────────────────────────────────────────────────────┘
            │
┌───────────┼────────────────────────────────────────────────────────────────┐
│           ▼                      PERSISTENCE LAYER                         │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  REPOSITORIES                                                       │   │
│  │  CharacterRepo │ RoomRepo │ ItemRepo │ InventoryRepo │ SaveGameRepo │   │
│  │  CodexEntryRepo│ DataCaptureRepo                                    │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                    │                                       │
│  ┌─────────────────────────────────┴───────────────────────────────────┐   │
│  │                     PostgreSQL 16 + EF Core 8.0                     │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Quick Reference: Core Formulas

| Calculation | Formula | Source Spec |
|-------------|---------|-------------|
| Max HP | `50 + (Sturdiness × 10)` | SPEC-CHAR-001 |
| Max Stamina | `20 + (Finesse × 5) + (Sturdiness × 3)` | SPEC-CHAR-001 |
| Max Aether (Mystic) | `10 + (Will × 5)` | SPEC-CHAR-001 |
| Action Points | `2 + (Wits ÷ 4)` | SPEC-CHAR-001 |
| Defense Score | `10 + Finesse - (Stress ÷ 20)` | SPEC-ATTACK-001 |
| Success Threshold | `Defense ÷ 5` | SPEC-ATTACK-001 |
| Carry Capacity | `Might × 10,000g` | SPEC-INV-001 |
| HP Recovery (Wilderness) | `10 + (Sturdiness × 2)` | SPEC-REST-001 |
| Stress Recovery | `Will × 5` | SPEC-REST-001 |

---

*Generated by The Architect | Rune & Rust v0.4.7*
