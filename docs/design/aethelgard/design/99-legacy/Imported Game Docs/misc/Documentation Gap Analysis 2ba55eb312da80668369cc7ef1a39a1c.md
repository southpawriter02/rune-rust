# Documentation Gap Analysis

Parent item: Specification Writing Guide (Specification%20Writing%20Guide%202ba55eb312da8032b14de6752422e93e.md)

> Generated: 2025-11-27
Last Updated: 2025-11-27
Purpose: Identify undocumented systems and prioritize documentation efforts
> 

### Recent Updates

- **2025-11-27**: Created 9 comprehensive system specifications addressing critical gaps:
    - `systems/save-load-system-spec.md` (SPEC-SYSTEM-001) ✅
    - `systems/inventory-equipment-spec.md` (SPEC-SYSTEM-002) ✅
    - `systems/vertical-movement-spec.md` (SPEC-SYSTEM-003) ✅
    - `systems/crafting-system-spec.md` (SPEC-SYSTEM-004) ✅
    - `systems/enemy-ai-behavior-spec.md` (SPEC-SYSTEM-005) ✅
    - `systems/loot-generation-spec.md` (SPEC-ECONOMY-001) ✅
    - `systems/encounter-generation-spec.md` (SPEC-SYSTEM-007) ✅
    - `systems/advanced-status-effects-spec.md` (SPEC-COMBAT-003) ✅
    - `systems/procedural-room-generation-spec.md` (SPEC-SYSTEM-008) ✅
- **2025-11-27**: Added 6 specialization specs (SkarHordeAspirant, IronBane, AtgeirWielder, ScrapTinker, Einbui, Seidkona) - all 19 implemented specializations now documented

---

## Coverage Statistics

| Metric | Value |
| --- | --- |
| **Total Documentation Files** | 128 markdown files |
| **Service Classes in Engine** | 121 active services |
| **Documented Services** | ~10 services |
| **Documentation Coverage** | ~8% of services |
| **Code Files** | 493 total (221 Core, 272 Engine) |

---

## Critical Gaps - STATUS UPDATE

All critical gaps have been addressed with comprehensive specifications:

| System | Code Exists | Documentation | Status |
| --- | --- | --- | --- |
| **Save/Load System** | `SaveRepository.cs` (134KB), 4 services | `systems/save-load-system-spec.md` | ✅ RESOLVED |
| **Inventory & Equipment** | `EquipmentService.cs`, `EquipmentDatabase.cs` | `systems/inventory-equipment-spec.md` | ✅ RESOLVED |
| **Vertical Movement (Z-Axis)** | `VerticalTraversalService.cs`, `SpatialLayoutService.cs` | `systems/vertical-movement-spec.md` | ✅ RESOLVED |
| **Crafting System** | `AdvancedCraftingService.cs`, 5+ services | `systems/crafting-system-spec.md` | ✅ RESOLVED |
| **Enemy AI Behavior** | 11 AI services in `/AI/` folder | `systems/enemy-ai-behavior-spec.md` | ✅ RESOLVED |
| **Loot Generation (ROG)** | `LootService.cs`, `BossLootService.cs` | `systems/loot-generation-spec.md` | ✅ RESOLVED |

### Critical Gap Details

### 1. Save/Load System

**Code Location:**

- `RuneAndRust.Persistence/SaveRepository.cs` (134KB)
- `RuneAndRust.Persistence/SaveData.cs`
- `RuneAndRust.DesktopUI/Services/SaveGameService.cs`
- `RuneAndRust.DesktopUI/Services/ISaveGameService.cs`
- `RuneAndRust.DesktopUI/ViewModels/SaveLoadViewModel.cs`

**What's NOT documented:**

- Save file format and structure
- Data serialization strategy (JSON vs binary)
- Save slot management and versioning
- Auto-save mechanics and frequency
- Load/recovery procedures
- Corruption handling and backup strategy

### 2. Inventory & Equipment Management

**Code Location:**

- `RuneAndRust.Engine/EquipmentService.cs`
- `RuneAndRust.Engine/EquipmentDatabase.cs` (60+ equipment definitions)
- `RuneAndRust.DesktopUI/Services/SaveGameService.cs`
- `RuneAndRust.Persistence/SaveRepository.cs`

**What's NOT documented:**

- Inventory system mechanics (capacity, management, sorting)
- Equipment slot system and incompatibilities
- Item quality tiers and modifiers
- Equipment stat bonuses and scaling
- Inventory data flow (pickup, drop, equip/unequip)

### 3. Vertical Movement / Z-Axis Mechanics

**Code Location:**

- `RuneAndRust.Engine/AdvancedMovementService.cs`
- `RuneAndRust.Engine/CoordinatedMovementService.cs`
- `RuneAndRust.Engine/ForcedMovementService.cs`
- `RuneAndRust.Engine/Spatial/VerticalTraversalService.cs`
- `RuneAndRust.Engine/Spatial/SpatialLayoutService.cs`
- `RuneAndRust.Core/Spatial/` - Spatial data models
- `RuneAndRust.Core/Grid/` - Grid system

**What's NOT documented:**

- 3D battlefield coordinate system (X, Y, Z)
- Z-axis movement costs and mechanics
- Elevation effects on combat (high ground advantage)
- Vertical terrain features (stairs, platforms, gaps)
- Multi-layer dungeon room rendering
- Climbing and descent mechanics

### 4. Crafting System

**Code Location:**

- `RuneAndRust.Engine/Crafting/AdvancedCraftingService.cs`
- `RuneAndRust.Engine/Crafting/ModificationService.cs`
- `RuneAndRust.Engine/Crafting/RecipeService.cs`
- `RuneAndRust.Engine/FieldMedicineCraftingService.cs`
- `RuneAndRust.Persistence/CraftingRepository.cs`

**What's NOT documented:**

- Crafting mechanics (recipes, stations, resources)
- Quality calculation and crafting success rates
- Component types and material requirements
- Equipment modification system (enchanting, upgrading)
- Recipe discovery and unlocking

### 5. Enemy/Mob AI Behavior

**Code Location:**

- `RuneAndRust.Engine/AI/EnemyAIOrchestrator.cs`
- `RuneAndRust.Engine/AI/AbilityPrioritizationService.cs`
- `RuneAndRust.Engine/AI/TargetSelectionService.cs`
- `RuneAndRust.Engine/AI/ThreatAssessmentService.cs`
- `RuneAndRust.Engine/AI/SituationalAnalysisService.cs`
- `RuneAndRust.Engine/AI/AdaptiveDifficultyService.cs`
- `RuneAndRust.Engine/AI/AbilityRotationService.cs`
- `RuneAndRust.Engine/AI/BehaviorPatternService.cs`
- `RuneAndRust.Engine/AI/ChallengeSectorAIService.cs`
- `RuneAndRust.Engine/AI/BossAIService.cs`

**What's NOT documented:**

- AI decision-making algorithm (evaluation order)
- Ability prioritization weights and calculations
- Target selection criteria and priorities
- Threat assessment formulas
- Adaptive difficulty scaling
- Boss-specific AI behaviors
- Behavior pattern execution

### 6. Random Object Generation (Loot System)

**Code Location:**

- `RuneAndRust.Engine/LootService.cs`
- `RuneAndRust.Engine/LootSpawner.cs`
- `RuneAndRust.Engine/BossLootService.cs`
- `RuneAndRust.Persistence/BossLootSeeder.cs`

**What's NOT documented:**

- ROG algorithm and random selection process
- Loot rarity tiers and drop rates
- Enemy-based loot generation logic
- Boss loot tables and special drops
- Loot quality modifiers
- Currency drop mechanics

---

## Major Gaps - STATUS UPDATE

Most major gaps have been addressed:

| System | Services | Status |
| --- | --- | --- |
| **Encounter Generation** | 4 services | ✅ `systems/encounter-generation-spec.md` |
| **Advanced Status Effects** | 3 services | ✅ `systems/advanced-status-effects-spec.md` |
| **Procedural Room Generation** | 5 services | ✅ `systems/procedural-room-generation-spec.md` |
| **GUI Implementation** | 23 UI services | ⚠️ Still needs documentation |

### Major Gap Details

### Encounter Generation

**Code Location:**

- `RuneAndRust.Engine/EncounterService.cs`
- `RuneAndRust.Engine/BossEncounterService.cs`
- `RuneAndRust.Engine/FactionEncounterService.cs`
- `RuneAndRust.Engine/RoomPopulationService.cs`
- `RuneAndRust.Engine/BudgetDistributionService.cs`

**Missing Documentation:**

- Encounter composition algorithm
- Difficulty scaling based on player level
- Enemy group formations
- Boss phase mechanics and transitions
- Faction-based encounter variations
- Encounter reward scaling

### Advanced Status Effects

**Code Location:**

- `RuneAndRust.Engine/AdvancedStatusEffectService.cs`
- `RuneAndRust.Engine/StatusEffectFlavorTextService.cs`
- `RuneAndRust.Persistence/StatusEffectRepository.cs`

**Missing Documentation:**

- Status effect stacking rules
- Duration mechanics (permanent vs temporary)
- Stack limit enforcement
- Effect removal logic and conditions
- Conditional status effects
- Immunity and resistance mechanics

### Procedural Room Generation

**Code Location:**

- `RuneAndRust.Engine/DungeonGenerator.cs`
- `RuneAndRust.Engine/DungeonService.cs`
- `RuneAndRust.Engine/RoomInstantiator.cs`
- `RuneAndRust.Engine/RoomPopulationService.cs`
- `RuneAndRust.Engine/DormantProcessSpawner.cs`
- `RuneAndRust.Engine/EnvironmentalObjectService.cs`
- `RuneAndRust.Engine/ObjectInteractionService.cs`

**Missing Documentation:**

- Room template system and customization
- Procedural room layout generation
- Hazard placement algorithms
- Environmental puzzle generation
- Secret room discovery mechanics
- Room connectivity and navigation rules
- Biome-specific generation variations

### GUI Implementation

**Code Location:**

- `RuneAndRust.DesktopUI/Controllers/` - 10+ UI controllers
- `RuneAndRust.DesktopUI/Services/` - 23 UI services
- `RuneAndRust.DesktopUI/ViewModels/` - UI viewmodels
- `RuneAndRust.DesktopUI/Views/` - XAML UI definitions
- `RuneAndRust.DesktopUI/Converters/` - Data converters

**Missing Documentation:**

- MVVM architecture and data binding
- View-ViewModel communication patterns
- State management across screens
- Combat grid rendering system
- Inventory UI management
- Equipment panel layout and interactions
- Character sheet display structure
- Navigation between screens

---

## Moderate Gaps (Lower Priority)

| System | Count | Examples |
| --- | --- | --- |
| **Companion System** | 4 services | CompanionAIService, RecruitmentService |
| **Faction & Territory** | 5 services | FactionWarService, TerritoryService |
| **NPC & Dialogue** | 3 services | DialogueService, NPCService |
| **Environmental Hazards** | 9 services | HazardService, TrapService, CoverService |
| **Specialization Implementation** | 11 services | Per-specialization mechanics |

### Moderate Gap Service Listing

### Companion System

- `CompanionService.cs` - Companion AI and management
- `CompanionAIService.cs` - Companion tactical AI
- `RecruitmentService.cs` - Companion recruitment
- `CompanionProgressionService.cs` - Companion leveling

### Faction & Territory

- `FactionService.cs` - Faction reputation system
- `FactionWarService.cs` - Territory warfare
- `ReputationService.cs` - Reputation tracking
- `TerritoryService.cs` - Territory control
- `MerchantService.cs` - Merchant interactions

### NPC & Dialogue

- `NPCService.cs` - NPC interactions
- `DialogueService.cs` - Dialogue tree evaluation
- `PricingService.cs` - Dynamic pricing
- `TransactionService.cs` - Buy/sell logic

### Environmental Hazards

- `HazardService.cs` - Hazard mechanics
- `HazardSpawner.cs` - Hazard placement
- `HazardDatabase.cs` - 8+ hazard types
- `EnvironmentalStressService.cs` - Environmental stress
- `EnvironmentalCombatService.cs` - Environmental interactions
- `DestructionService.cs` - Destructible terrain
- `TrapService.cs` - Trap mechanics
- `CoverService.cs` - Cover mechanics
- `WeatherEffectService.cs` - Weather effects

### Specialization Implementation Services

- `BerserkrService.cs`
- `EchoCallerService.cs`
- `MyrkgengrService.cs`
- `SkaldService.cs`
- `SkjaldmaerService.cs`
- `StrandhoggService.cs`
- `VeidimadurService.cs`
- `GorgeMawAsceticService.cs`
- `HlekkmasterService.cs`
- `SeidkonaService.cs`
- `EinbuiService.cs`

---

## Recommended Priority Order - UPDATED

### Tier 1: Critical (Blocks Other Systems) - ✅ ALL COMPLETED

1. ✅ **Save/Load System** - `systems/save-load-system-spec.md`
2. ✅ **Inventory & Equipment** - `systems/inventory-equipment-spec.md`
3. ✅ **Vertical Movement** - `systems/vertical-movement-spec.md`

### Tier 2: High-Value (Enables Key Gameplay) - ✅ ALL COMPLETED

1. ✅ **Crafting System** - `systems/crafting-system-spec.md`
2. ✅ **Enemy AI Behavior** - `systems/enemy-ai-behavior-spec.md`
3. ✅ **Loot Generation (ROG)** - `systems/loot-generation-spec.md`

### Tier 3: Important (Completes Core Gameplay) - ✅ MOSTLY COMPLETED

1. ✅ **Encounter Generation** - `systems/encounter-generation-spec.md`
2. ✅ **Advanced Status Effects** - `systems/advanced-status-effects-spec.md`
3. ✅ **Procedural Room Generation** - `systems/procedural-room-generation-spec.md`

### Tier 4: Complementary (System Coverage) - REMAINING WORK

1. ⚠️ **GUI Implementation** - Developer onboarding (still needs documentation)
2. ⚠️ **Companion System** - Party gameplay (specs exist partially)
3. ⚠️ **Faction & Territory** - World systems (specs exist partially)

---

## Documentation Roadmap

### Phase 1: Critical Systems (Est. 18 hours)

| System | Effort | Output Files |
| --- | --- | --- |
| Save/Load | 6 hours | `save-load-system.md`, `save-load-spec.md` |
| Inventory & Equipment | 7 hours | `inventory-management.md`, `equipment-system.md` |
| Vertical Movement | 5 hours | `vertical-movement-mechanics.md`, `spatial-system.md` |

### Phase 2: Core Gameplay (Est. 19 hours)

| System | Effort | Output Files |
| --- | --- | --- |
| Crafting | 6 hours | `crafting-system.md`, `crafting-spec.md` |
| Enemy AI | 8 hours | `ai-behavior-system.md`, `ai-behavior-spec.md` |
| Loot Generation | 5 hours | `loot-generation.md`, `loot-system.md` |

### Phase 3: Complete Coverage (Est. 15 hours)

| System | Effort | Output Files |
| --- | --- | --- |
| Encounter Generation | 5 hours | `encounter-generation.md` |
| Status Effects | 4 hours | `status-effects-advanced.md` |
| Room Generation | 6 hours | `room-generation.md` |

**Total Estimated Effort**: 52 hours (~7-8 weeks at 8 hours/week)

---

## Existing Documentation Reference

### Well-Documented Areas

- `/docs/00-specifications/combat/` - 7 combat spec files
- `/docs/00-specifications/progression/` - 3 progression spec files
- `/docs/00-specifications/economy/` - 2 economy spec files
- `/docs/00-specifications/specializations/` - 19 specialization specs (all implemented specializations now documented)
- `/docs/03-technical-reference/services/` - 10 service docs

### Partial Documentation

- `/docs/01-systems/status-effects.md` - Lists effects, lacks advanced mechanics
- `/docs/03-technical-reference/services/enemy-ai.md` - Basic AI, lacks decision algorithms
- `/docs/00-specifications/economy/loot-equipment-spec.md` - Spec only, no implementation

---

**End of Analysis**