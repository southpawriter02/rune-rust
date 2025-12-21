# Specification Audit Matrix

> **Audit Date:** 2025-12-21
> **Version Audited:** v0.3.3c
> **Auditor:** The Architect
> **Status:** GAPS IDENTIFIED

---

## Executive Summary

| Category | Count |
|----------|-------|
| **Documented Specifications** | 12 |
| **Implemented Services** | 38 |
| **Services WITH Specs** | 21 |
| **Services MISSING Specs** | 17 |
| **Coverage Rate** | 55.3% |

---

## Coverage Matrix

### Legend
- **Covered** = Has dedicated spec document
- **Partial** = Mentioned in related spec but not primary focus
- **MISSING** = No specification documentation

---

## DOCUMENTED SYSTEMS (Covered)

| Spec ID | System | Primary Services | Status |
|---------|--------|------------------|--------|
| SPEC-DICE-001 | Dice Pool System | `DiceService` | Covered |
| SPEC-COMBAT-001 | Combat System | `CombatService`, `AttackResolutionService`, `InitiativeService` | Covered |
| SPEC-ABILITY-001 | Ability System | `AbilityService`, `EffectScriptExecutor` | Covered |
| SPEC-CHAR-001 | Character & Progression | `StatCalculationService`, `CharacterFactory` | Covered |
| SPEC-TRAUMA-001 | Trauma & Stress System | `TraumaService`, `TraumaRegistry` | Covered |
| SPEC-CORRUPT-001 | Corruption System | `TraumaService` (shared) | Covered |
| SPEC-REST-001 | Rest & Recovery System | `RestService`, `AmbushService` | Covered |
| SPEC-INV-001 | Inventory & Equipment | `InventoryService`, `LootService` | Covered |
| SPEC-HAZARD-001 | Dynamic Hazard System | `HazardService`, `EffectScriptExecutor` | Covered |
| SPEC-COND-001 | Ambient Condition System | `ConditionService` | Covered |
| SPEC-STATUS-001 | Status Effect System | `StatusEffectService` | Covered |
| SPEC-CODEX-001 | Scavenger's Journal (Codex) | `CodexEntryRepository`, `DataCaptureRepository` | Covered |

---

## UNDOCUMENTED SYSTEMS (Missing Specifications)

### Priority 1: Core Gameplay Systems (Critical)

| Proposed Spec ID | System | Service(s) | Location | Impact | Notes |
|------------------|--------|------------|----------|--------|-------|
| **SPEC-CRAFT-001** | Crafting System | `CraftingService` | `Engine/Services/CraftingService.cs` | HIGH | Recipe validation, WITS-based rolls, quality outcomes (Normal/Masterwork), trade-specific catastrophes |
| **SPEC-REPAIR-001** | Repair & Salvage System | `BodgingService` | `Engine/Services/BodgingService.cs` | HIGH | Repair mechanics, salvage-to-Scrap conversion, damage-based DC modifiers |
| **SPEC-ENEMY-001** | Enemy AI System | `EnemyAIService` | `Engine/Services/EnemyAIService.cs` | HIGH | Archetype-based behavior, weighted probability tables, HP threshold triggers |
| **SPEC-TRAIT-001** | Creature Trait System | `CreatureTraitService` | `Engine/Services/CreatureTraitService.cs` | HIGH | Trait generation, runtime effects (Regeneration, Vampiric, Thorns, Explosive Death, Resilient, Bloodlust) |
| **SPEC-INTERACT-001** | Interaction System | `InteractionService` | `Engine/Services/InteractionService.cs` | HIGH | WITS-based object interaction, examine/open/close/search/loot commands, tiered discovery |

### Priority 2: Exploration & World Systems (Important)

| Proposed Spec ID | System | Service(s) | Location | Impact | Notes |
|------------------|--------|------------|----------|--------|-------|
| **SPEC-NAV-001** | Navigation System | `NavigationService` | `Engine/Services/NavigationService.cs` | MEDIUM | Room traversal, exit validation, movement hazard triggers |
| **SPEC-DUNGEON-001** | Dungeon Generation System | `DungeonGenerator` | `Engine/Services/DungeonGenerator.cs` | MEDIUM | Room graph generation, procedural layout (future), test map structure |
| **SPEC-ENVPOP-001** | Environment Population System | `EnvironmentPopulator` | `Engine/Services/EnvironmentPopulator.cs` | MEDIUM | Biome-based hazard/condition spawning, danger level probability scaling |
| **SPEC-SPAWN-001** | Object Spawning System | `ObjectSpawner` | `Engine/Services/ObjectSpawner.cs` | MEDIUM | Interactable object placement in rooms |

### Priority 3: Infrastructure & Support Systems (Standard)

| Proposed Spec ID | System | Service(s) | Location | Impact | Notes |
|------------------|--------|------------|----------|--------|-------|
| **SPEC-SAVE-001** | Save/Load System | `SaveManager` | `Engine/Services/SaveManager.cs` | MEDIUM | GameState persistence, JSON serialization, SaveGame entity translation |
| **SPEC-RESOURCE-001** | Resource Management System | `ResourceService` | `Engine/Services/ResourceService.cs` | MEDIUM | Stamina/Aether management, regeneration, overcast mechanic |
| **SPEC-JOURNAL-001** | Journal Display System | `JournalService` | `Engine/Services/JournalService.cs` | LOW | Journal formatting, fragment display, text redaction |
| **SPEC-CAPTURE-001** | Data Capture System | `DataCaptureService` | `Engine/Services/DataCaptureService.cs` | LOW | Lore fragment generation, auto-assignment to Codex, capture chance modifiers |
| **SPEC-CMD-001** | Command Parser System | `CommandParser` | `Engine/Services/CommandParser.cs` | LOW | Terminal input parsing, action interpretation |

### Priority 4: Factory & Generation Systems (Supporting)

| Proposed Spec ID | System | Service(s) | Location | Impact | Notes |
|------------------|--------|------------|----------|--------|-------|
| **SPEC-ENEMYFAC-001** | Enemy Factory System | `EnemyFactory` | `Engine/Factories/EnemyFactory.cs` | LOW | Template-based enemy creation, scaling/variance, trait application |
| **SPEC-GAME-001** | Game Orchestration System | `GameService` | `Engine/Services/GameService.cs` | LOW | Main loop, phase management, command routing |

---

## PARTIAL COVERAGE (Services Mentioned but Not Primary Focus)

| Service | Mentioned In | Missing Details |
|---------|--------------|-----------------|
| `LootService` | SPEC-INV-001 | Loot table structure, biome weighting, quality tier distribution |
| `LootTables` | SPEC-INV-001 | Configuration schema, pool definitions |
| `CaptureTemplates` | SPEC-CODEX-001, SPEC-CAPTURE-001 | Template catalog, flavor text patterns |
| `AmbushService` | SPEC-REST-001 | Risk calculation formula, biome enemy tables |
| `EffectScriptExecutor` | SPEC-ABILITY-001, SPEC-HAZARD-001 | Full script syntax reference, command catalog |
| `CharacterFactory` | SPEC-CHAR-001 | Attribute allocation rules, lineage/archetype bonuses |
| `TraumaRegistry` | SPEC-TRAUMA-001 | Full trauma catalog, flavor text templates |
| `DescriptorEngine` | (none) | Dynamic descriptor generation rules |
| `TextRedactor` | SPEC-CODEX-001 | Redaction patterns, algorithm |

---

## GAP ANALYSIS BY DOMAIN

### Combat Domain
| System | Spec Coverage | Gap |
|--------|---------------|-----|
| Core Combat | SPEC-COMBAT-001 | None |
| Enemy AI | **MISSING** | No AI behavior documentation |
| Creature Traits | **MISSING** | No trait effect documentation |
| Status Effects | SPEC-STATUS-001 | None |

### Exploration Domain
| System | Spec Coverage | Gap |
|--------|---------------|-----|
| Navigation | **MISSING** | No traversal rules documented |
| Dungeon Generation | **MISSING** | No generation algorithm documented |
| Hazards | SPEC-HAZARD-001 | None |
| Conditions | SPEC-COND-001 | None |
| Interactions | **MISSING** | No interaction rules documented |

### Economy Domain
| System | Spec Coverage | Gap |
|--------|---------------|-----|
| Inventory | SPEC-INV-001 | None |
| Crafting | **MISSING** | No crafting rules documented |
| Repair/Salvage | **MISSING** | No repair/salvage rules documented |
| Loot Generation | Partial (INV-001) | Needs expansion |

### Character Domain
| System | Spec Coverage | Gap |
|--------|---------------|-----|
| Character Creation | SPEC-CHAR-001 | None |
| Trauma/Stress | SPEC-TRAUMA-001 | None |
| Corruption | SPEC-CORRUPT-001 | None |
| Resources | **MISSING** | No resource management documented |

### Meta/Infrastructure Domain
| System | Spec Coverage | Gap |
|--------|---------------|-----|
| Save/Load | **MISSING** | No persistence rules documented |
| Command Parsing | **MISSING** | No command syntax documented |
| Game Loop | **MISSING** | No orchestration documented |

---

## ENTITY COVERAGE AUDIT

### Entities WITH Spec References

| Entity | Referenced In | Coverage |
|--------|---------------|----------|
| `Character` | SPEC-CHAR-001 | Full |
| `Enemy` | SPEC-COMBAT-001 | Partial |
| `Item` | SPEC-INV-001 | Full |
| `Equipment` | SPEC-INV-001 | Full |
| `InventoryItem` | SPEC-INV-001 | Full |
| `ActiveAbility` | SPEC-ABILITY-001 | Full |
| `Room` | SPEC-HAZARD-001, SPEC-COND-001 | Partial |
| `DynamicHazard` | SPEC-HAZARD-001 | Full |
| `AmbientCondition` | SPEC-COND-001 | Full |
| `Trauma` | SPEC-TRAUMA-001 | Full |
| `CodexEntry` | SPEC-CODEX-001 | Full |
| `DataCapture` | SPEC-CODEX-001 | Full |

### Entities WITHOUT Spec References

| Entity | Location | Missing Spec |
|--------|----------|--------------|
| `InteractableObject` | `Core/Entities/InteractableObject.cs` | SPEC-INTERACT-001 |
| `Recipe` | `Core/Entities/Recipe.cs` | SPEC-CRAFT-001 |
| `SaveGame` | `Core/Entities/SaveGame.cs` | SPEC-SAVE-001 |
| `ItemProperty` | `Core/Entities/ItemProperty.cs` | SPEC-INV-001 (expansion) |

---

## ENUM COVERAGE AUDIT

### Enums WITHOUT Dedicated Documentation

| Enum | Location | Should Be Documented In |
|------|----------|------------------------|
| `CraftingTrade` | `Core/Enums/CraftingTrade.cs` | SPEC-CRAFT-001 |
| `CraftingOutcome` | `Core/Enums/CraftingOutcome.cs` | SPEC-CRAFT-001 |
| `CatastropheType` | `Core/Enums/CatastropheType.cs` | SPEC-CRAFT-001 |
| `CreatureTraitType` | `Core/Enums/CreatureTraitType.cs` | SPEC-TRAIT-001 |
| `EnemyArchetype` | `Core/Enums/EnemyArchetype.cs` | SPEC-ENEMY-001 |
| `ObjectType` | `Core/Enums/ObjectType.cs` | SPEC-INTERACT-001 |
| `Direction` | `Core/Enums/Direction.cs` | SPEC-NAV-001 |
| `RoomFeature` | `Core/Enums/RoomFeature.cs` | SPEC-DUNGEON-001 |
| `PendingGameAction` | `Core/Enums/PendingGameAction.cs` | SPEC-GAME-001 |

---

## RECOMMENDED SPECIFICATION PRIORITY ORDER

### Immediate (Next Sprint)
1. **SPEC-CRAFT-001** - Crafting System (high player impact, complex rules)
2. **SPEC-REPAIR-001** - Repair & Salvage System (economy balance critical)
3. **SPEC-ENEMY-001** - Enemy AI System (core combat dependency)

### Short-Term (Following Sprint)
4. **SPEC-TRAIT-001** - Creature Trait System (combat variety)
5. **SPEC-INTERACT-001** - Interaction System (exploration core)
6. **SPEC-NAV-001** - Navigation System (player movement)

### Medium-Term
7. **SPEC-DUNGEON-001** - Dungeon Generation
8. **SPEC-SAVE-001** - Save/Load System
9. **SPEC-RESOURCE-001** - Resource Management

### Backlog
10. **SPEC-ENVPOP-001** - Environment Population
11. **SPEC-JOURNAL-001** - Journal Display
12. **SPEC-CAPTURE-001** - Data Capture
13. **SPEC-CMD-001** - Command Parser
14. **SPEC-ENEMYFAC-001** - Enemy Factory
15. **SPEC-SPAWN-001** - Object Spawning
16. **SPEC-GAME-001** - Game Orchestration

---

## CROSS-REFERENCE GAPS

The following cross-system dependencies are NOT documented:

| From Service | To Service | Undocumented Interaction |
|--------------|------------|-------------------------|
| `CombatService` | `EnemyAIService` | AI decision integration |
| `CombatService` | `CreatureTraitService` | Trait effect application |
| `NavigationService` | `EnvironmentPopulator` | Room population on entry |
| `InteractionService` | `LootService` | Container loot generation |
| `CraftingService` | `InventoryService` | Material consumption |
| `BodgingService` | `InventoryService` | Item modification |
| `RestService` | `ResourceService` | Resource restoration |
| `DungeonGenerator` | `EnvironmentPopulator` | Initial room setup |

---

## SUMMARY

**Total Specifications Needed:** 17 new specs  
**Current Coverage:** 12 documented specs covering 21/38 systems (55.3%)  
**Target Coverage:** 38/38 systems (100%)

### Action Items
1. Prioritize SPEC-CRAFT-001, SPEC-REPAIR-001, SPEC-ENEMY-001 for immediate creation
2. Expand SPEC-INV-001 to include LootService details
3. Create EffectScript syntax reference document
4. Document all cross-system dependencies in SPEC-INDEX

---

*Generated by The Architect - Legacy Content Audit*
