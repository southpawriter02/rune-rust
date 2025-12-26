# Specification Backlog & Registry

Parent item: Specification Writing Guide (Specification%20Writing%20Guide%202ba55eb312da8032b14de6752422e93e.md)

**Purpose**: Central tracking document for all specifications (existing, in-progress, and planned)

**Last Updated**: 2025-11-27
**Total Specs**: 23 completed, 0 in progress, 27 planned, 50 total

---

## Quick Status Summary

| Domain | Completed | In Progress | Planned | Total | Coverage |
| --- | --- | --- | --- | --- | --- |
| Combat | 8 | 0 | 4 | 12 | 67% |
| Progression | 3 | 0 | 4 | 7 | 43% |
| Economy | 3 | 0 | 3 | 6 | 50% |
| World | 1 | 0 | 7 | 8 | 13% |
| Narrative | 1 | 0 | 5 | 6 | 17% |
| Faction | 0 | 0 | 3 | 3 | 0% |
| AI | 1 | 0 | 1 | 2 | 50% |
| Systems | 6 | 0 | 0 | 6 | 100% |
| **TOTAL** | **23** | **0** | **27** | **50** | **46%** |

---

## Specification Status Legend

- ✅ **Completed**: Specification drafted, reviewed, and committed
- 🚧 **In Progress**: Currently being drafted
- 📋 **Planned**: Identified as needed, not yet started
- 🔮 **Aspirational**: Future system, lower priority
- 🔗 **Has Layer 1 Docs**: Implementation documentation exists in `docs/01-systems/`

---

## COMBAT Domain

### ✅ SPEC-COMBAT-001: Combat Resolution System

**Status**: Completed
**File**: `combat/combat-resolution-spec.md`**Lines**: ~939
**Completed**: 2025-11-19
**Layer 1 Docs**: 🔗 `docs/01-systems/combat-resolution.md`

**Scope**: Initiative system, turn sequence, combat state management, victory/defeat/flee conditions

**Why Complete**: Core combat flow is foundational to all combat-related systems.

---

### ✅ SPEC-COMBAT-002: Damage Calculation System

**Status**: Completed
**File**: `combat/damage-calculation-spec.md`**Lines**: ~1832
**Completed**: 2025-11-19
**Priority**: High
**Domain**: Combat
**Layer 1 Docs**: 🔗 `docs/01-systems/damage-calculation.md`

**Scope**:

- Base damage calculation (weapon damage dice + flat bonuses)
- Attribute-based attack accuracy (FINESSE/MIGHT/WILL attack rolls)
- Status effect damage modifiers (Vulnerable, Inspired, Defensive Stance)
- Damage mitigation mechanics (Defense Bonus, Soak/armor)
- Minimum damage rule (successful hits deal ≥1 damage)
- Ignore Armor mechanic (abilities bypass defense)
- Critical hit mechanics (flanking-based, double damage dice)
- Stance-based flat damage bonuses (Aggressive stance +4 damage)

**Out of Scope**:

- Status effect application/removal → SPEC-COMBAT-003
- Equipment acquisition → SPEC-ECONOMY-001
- Environmental damage → SPEC-WORLD-003

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- Dice Pool System - Documented in Layer 1

**Why Complete**: Damage calculation is core to combat outcomes; needed formal specification for damage formulas, mitigation mechanics, and balance targets.

**Implementation Exists**: Yes (`RuneAndRust.Engine/Services/CombatEngine.cs`)

---

### ✅ SPEC-COMBAT-003: Status Effects System

**Status**: Completed
**File**: `combat/status-effects-spec.md`**Lines**: ~1931
**Completed**: 2025-11-19
**Priority**: High
**Domain**: Combat
**Layer 1 Docs**: 🔗 `docs/01-systems/status-effects.md`

**Scope**:

- Four status effect categories (Control Debuffs, DoT, Stat Modifications, Buffs)
- 11+ implemented status effects with defined behaviors
- Stacking mechanics with per-effect maximum stack limits
- Duration tracking with turn-based decrement
- Three interaction types (Conversion, Amplification, Suppression)
- Start-of-turn and end-of-turn processing
- Damage Over Time (DoT) calculation with stacking
- Stat modification application to damage/accuracy/defense
- Control effect enforcement (action restrictions)
- Effect removal and cleansing mechanics
- Trauma Economy integration (stress from debuffs)

**Out of Scope**:

- Persistent character buffs → Equipment/Progression systems
- Environmental hazards → SPEC-WORLD-003
- Specific ability implementations → Ability System spec

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-002 (Damage Calculation) - Completed

**Why Complete**: Status effects create tactical depth in combat; needed comprehensive design specification for stacking rules, interactions, and balance.

**Implementation Exists**: Yes (`RuneAndRust.Engine/Services/StatusEffectService.cs`)

---

### ✅ SPEC-COMBAT-004: Accuracy & Evasion System

**Status**: Completed
**File**: `combat/accuracy-evasion-spec.md`**Lines**: ~1488
**Completed**: 2025-11-20
**Priority**: Medium
**Domain**: Combat
**Layer 1 Docs**: 🔗 `docs/01-systems/accuracy-evasion.md`

**Scope**:

- Opposed dice pool mechanics (attack roll vs defense roll)
- Attack dice pool calculation (base attribute + accuracy bonuses)
- Defense dice pool calculation (STURDINESS attribute)
- Net success determination (attack successes - defense successes)
- Hit/miss resolution (net > 0 = hit, ≤ 0 = miss)
- Accuracy bonus sources (equipment, abilities, status effects)
- Tie-breaking rule (defender wins ties)
- Combat log integration for roll display
- Probability balancing and hit chance targets
- 5 detailed combat examples
- Comprehensive probability tables (hit chance matrix, bonus impact, build archetypes)
- Balance targets and known issues documentation

**Out of Scope**:

- Damage calculation after hit lands → SPEC-COMBAT-002
- Status effect application mechanics → SPEC-COMBAT-003
- Critical hit damage multipliers → Future enhancement
- Flanking position calculation → SPEC-COMBAT-008
- Environmental accuracy modifiers → SPEC-WORLD-003
- Cover and concealment → SPEC-COMBAT-007

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-002 (Damage Calculation) - Completed
- SPEC-COMBAT-003 (Status Effects) - Completed
- SPEC-PROGRESSION-001 (Character Progression) - Completed

**Why Complete**: Opposed dice pool accuracy system is core combat mechanic; needed comprehensive design specification for roll mechanics, accuracy bonus economy, probability targets, and balance tuning parameters. Layer 1 implementation docs existed but design philosophy and player experience goals were not formally specified.

**Implementation Exists**: Yes (`RuneAndRust.Engine/CombatEngine.cs`, `DiceService.cs`)

---

### ✅ SPEC-COMBAT-005: Boss Encounter System

**Status**: Completed
**File**: `combat/boss-encounter-spec.md`**Lines**: ~1530
**Completed**: 2025-11-22
**Priority**: High
**Domain**: Combat
**Layer 1 Docs**: 🔗 `BOSS_COMBAT_INTEGRATION_GUIDE.md`

**Scope**:

- Multi-phase boss combat mechanics (2-4 phases per boss)
- HP threshold-based phase transitions (75%, 50%, 25%)
- Telegraphed ability system with charge times and interrupt mechanics
- Enrage triggers (HP-based and turn-based timers)
- Vulnerability windows after ultimate abilities
- Phase-specific stat modifiers (damage, defense, regeneration, bonus actions)
- Add wave spawning during phase transitions
- Invulnerability mechanics during phase transitions
- Boss loot table system (artifacts, guaranteed drops, unique items, set bonuses)
- 4 implemented boss archetypes (Ruin-Warden, Aetheric Aberration, Forlorn Archivist, Omega Sentinel)
- Balance targets (TTK, damage output, phase timing)
- Boss encounter design checklist

**Out of Scope**:

- General enemy design guidelines → SPEC-COMBAT-012
- Standard enemy AI behavior → SPEC-AI-001 (planned)
- Movement and positioning mechanics → SPEC-COMBAT-009 (planned)
- Status effect mechanics → SPEC-COMBAT-003
- Individual boss lore and narrative descriptions → Bestiary entries

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-002 (Damage Calculation) - Completed
- SPEC-COMBAT-003 (Status Effects) - Completed
- SPEC-COMBAT-012 (Enemy Design & Bestiary) - Completed
- SPEC-ECONOMY-001 (Loot & Equipment) - Completed
- SPEC-PROGRESSION-001 (Character Progression) - Completed

**Why Complete**: Boss encounter system is fully implemented; needed comprehensive design specification for multi-phase mechanics, telegraphed abilities, enrage system, and boss loot economy.

**Implementation Exists**: Yes (`RuneAndRust.Engine/BossEncounterService.cs`, `BossCombatIntegration.cs`, `BossDatabase.cs`, `BossLootService.cs`, `TelegraphedAbilityService.cs`)

**Key Features**:

- 6 functional requirements (FR-001 through FR-006) covering full boss encounter lifecycle
- Detailed phase transition mechanics with invulnerability and stat modifiers
- Comprehensive telegraphed ability system (4 ability types: Standard, Telegraphed, Ultimate, Passive)
- Interrupt mechanics with damage thresholds and stagger effects
- Vulnerability window system for ultimate abilities
- Enrage triggers with HP-based and turn-based conditions
- Boss loot system with guaranteed quality drops, artifacts, unique items, crafting materials
- Balance targets for TTK (8-25 turns), damage output, and phase timing
- 3 appendices: Boss stat reference tables, design checklist, full Ruin-Warden example

---

### ✅ SPEC-COMBAT-006: Counter-Attack & Parry System

**Status**: Completed
**File**: `combat/counter-attack-parry-spec.md`**Lines**: ~1913
**Completed**: 2025-11-22
**Priority**: High
**Domain**: Combat
**Layer 1 Docs**: 🔗 `COUNTER_ATTACK_INTEGRATION.md`

**Scope**:

- Universal parry mechanics (FINESSE + Weapon Skill + bonus dice)
- Four-tier parry outcomes (Failed, Standard, Superior, Critical)
- Parry pool calculation with specialization/equipment bonuses
- Riposte and counter-attack system (free attacks on successful parries)
- Riposte trigger conditions (Critical = all characters, Superior = Hólmgangr only)
- Hólmgangr Reactive Parry specialization (3 ranks: +1d10/+2d10, Superior Riposte, 2 parries/round)
- Atgeir-wielder parry bonus (+1d10 reach advantage)
- Trauma economy integration (stress relief/gain by parry outcome)
- Parry limits and turn management (1-2 parries per round)
- Persistent statistics tracking (ParryStatistics, ParryBonuses, ParryAttempts tables)
- Combat flow integration (reactive parry preparation and execution)
- Balance targets (success rates 40-85%, riposte damage contribution 5-50%)

**Out of Scope**:

- Enemy parrying mechanics → Future enhancement
- Shield equipment parry bonuses → SPEC-COMBAT-007 (planned)
- Status effect interactions with parry pool → SPEC-COMBAT-003
- Specific ability implementations → Ability System spec

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-002 (Damage Calculation) - Completed
- SPEC-PROGRESSION-001 (Character Progression & Specializations) - Completed
- SPEC-MENTAL-001 (Trauma Economy) - Completed

**Why Complete**: Counter-attack and parry system is fully implemented; needed comprehensive design specification for universal parry mechanics, quality-based outcomes, riposte triggers, specialization integration, and balance targets.

**Implementation Exists**: Yes (`RuneAndRust.Engine/CounterAttackService.cs`, `RuneAndRust.Core/CounterAttack.cs`, `RuneAndRust.Persistence/CounterAttackRepository.cs`, `RuneAndRust.Engine/Commands/ParryCommand.cs`)

**Key Features**:

- 6 functional requirements (FR-001 through FR-006) covering full parry/riposte lifecycle
- Universal parry pool calculation (FINESSE + Weapon Skill + Bonus Dice × d10)
- Four-tier outcome system with distinct mechanical effects (Failed/Standard/Superior/Critical)
- Critical parry threshold (5+ margin) triggering universal riposte
- Hólmgangr Reactive Parry specialization with 3 ranks (escalating bonuses)
- Superior Riposte capability (Hólmgangr exclusive for Superior parries)
- Trauma economy integration (-8 stress for Critical Parry, +5 stress for Failed Parry)
- Parry limit system (1/round standard, 2/round for Hólmgangr Rank 3 Mastery)
- Database persistence (3 tables: ParryStatistics, ParryBonuses, ParryAttempts)
- Balance targets and design guidelines for future extensions
- 3 appendices: Parry outcome reference tables, full combat example, design checklist

---

### ✅ SPEC-COMBAT-012: Enemy Design & Bestiary System

**Status**: Completed
**File**: `combat/enemy-design-spec.md`**Lines**: ~1494
**Completed**: 2025-11-22
**Priority**: High
**Domain**: Combat
**Layer 1 Docs**: 🔗 6 Templates (Enemy Design Proposal, Enemy Design Worksheet, AI Behavior Pattern, Design Review Checklist, Encounter Composition Calculator, Enemy Bestiary Entry)

**Scope**:

- Threat Level Classification System (5 tiers: Low, Medium, High, Lethal, Boss)
- Stat Budget Allocation formulas per tier (HP, attributes, damage ranges)
- Enemy Archetype Taxonomy (8 archetypes: Tank, DPS, Glass Cannon, Support, Swarm, Caster, Mini-Boss, Boss)
- Special Mechanics Flags (IsForlorn trauma aura, IsBoss multi-phase, IsChampion elite variants, Soak damage reduction)
- AI Behavior Decision-Making (probability-based patterns per archetype)
- Enemy Scaling vs. Player Progression (Legend-based scaling formulas)
- Loot Table Integration (threat tier → equipment quality mapping)
- Trauma Economy Integration (Stress/Corruption infliction rules)
- Balance & Tuning (TTK targets, damage output balance, encounter composition guidelines, tunable parameters)
- Complete enemy statistics for all 20 implemented enemies organized by tier and archetype
- v0.18 balance adjustment rationale and lessons learned

**Out of Scope**:

- Individual enemy lore/narrative descriptions → Enemy Bestiary entries
- Specific boss encounter mechanics → SPEC-COMBAT-005
- AI pathfinding implementation → AI Domain specs
- Enemy spawning/population mechanics → World Domain specs

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-002 (Damage Calculation) - Completed
- SPEC-COMBAT-003 (Status Effects) - Completed
- SPEC-PROGRESSION-001 (Character Progression) - Completed (Legend scaling)
- SPEC-ECONOMY-001 (Loot & Equipment) - Completed (loot quality mapping)

**Why Complete**: Enemy design system needed formal specification for stat budgets, archetype patterns, AI behavior, and balance targets. Implementation exists for 20 enemies but design philosophy and balance guidelines were undocumented. This spec provides comprehensive design framework for creating balanced enemies across all threat tiers with detailed balance tuning parameters and TTK targets.

**Implementation Exists**: Yes (`RuneAndRust.Engine/EnemyFactory.cs`, `RuneAndRust.Engine/EnemyAI.cs`, `RuneAndRust.Core/Enemy.cs`)

**Key Features**:

- Analyzed all 20 implemented enemies to derive stat budget formulas
- 8 enemy archetypes with tactical roles and AI patterns
- TTK analysis: Solo player 2-3 turns (Low) to 12-20 turns (Boss)
- Damage balance: No one-shots rule, variance caps, sustained threat design
- Encounter composition guidelines: ETS formula, recommended group types
- Tunable parameters: Safe adjustments (HP ±20%, Soak ±1-2) vs. dangerous (damage dice, threat tier changes)
- v0.18 balance lessons: Soak caps (8→6), damage redistribution (4d6+3→3d6+4), variance reduction (5d6→4d6)
- 4 comprehensive appendices: Enemy stats by tier, stats by archetype, special mechanics reference, v0.18 adjustment summary
- **6 practical templates** covering full enemy design lifecycle:
    - **Enemy Design Proposal** (153 lines) - Concept pitch with roster diversity validation
    - **Enemy Design Worksheet** (333 lines) - Full stat allocation and balance validation
    - **AI Behavior Pattern Template** (658 lines) - Code templates for EnemyAI.cs implementation
    - **Design Review Checklist** (307 lines) - Pre-implementation reviewer validation
    - **Encounter Composition Calculator** (650 lines) - Multi-enemy encounter balancing
    - **Enemy Bestiary Entry** (284 lines) - Post-implementation documentation

---

## PROGRESSION Domain

### ✅ SPEC-PROGRESSION-001: Character Progression System

**Status**: Completed
**File**: `progression/character-progression-spec.md`**Lines**: ~650
**Completed**: 2025-11-19
**Layer 1 Docs**: 🔗 `docs/01-systems/legend-leveling.md`

**Scope**: Legend/XP economy, Milestone system, Progression Points (PP), attribute increases

**Why Complete**: Core progression loop is foundational to player advancement and reward systems.

---

### ✅ SPEC-PROGRESSION-002: Archetype & Specialization System

**Status**: Completed
**File**: `progression/archetype-specialization-spec.md`**Lines**: ~1240
**Completed**: 2025-11-19
**Priority**: High
**Domain**: Progression
**Layer 1 Docs**: None (this spec provides comprehensive design documentation)

**Scope**:

- 4 Core Archetypes: Warrior, Adept, Skirmisher, Mystic
- Archetype selection at character creation
- Starting attribute distributions per archetype
- Starting abilities per archetype (3 abilities each)
- Resource systems (Stamina vs Aether Pool)
- Specialization unlock mechanics (3 PP cost)
- Specialization ability tree structure (9 abilities, 4 tiers)
- Specialization validation rules
- Archetype-specialization compatibility

**Dependencies**:

- SPEC-PROGRESSION-001 (Character Progression) - Completed
- SPEC-PROGRESSION-003 (Ability System) - Not yet drafted

**Why Complete**: Formal design specification created documenting the 4 Archetypes (correcting previous confusion with CharacterClass enum), specialization unlock mechanics, and complete system integration.

**Implementation Exists**: Yes (`RuneAndRust.Core/Archetypes/`, `RuneAndRust.Engine/SpecializationService.cs`, `RuneAndRust.Persistence/DataSeeder.cs`)

**Notable Corrections**:

- Clarified that 4 formal Archetypes exist: Warrior, Adept, Skirmisher, Mystic
- Documented that "Scavenger" in CharacterClass enum is a legacy entry, NOT a formal Archetype
- Mapped all existing specializations to their correct archetypes (6 Warrior, 5 Adept, 4 Skirmisher, 2 Mystic)

---

### ✅ SPEC-PROGRESSION-003: Ability Rank Advancement System

**Status**: Completed
**File**: `progression/ability-rank-advancement-spec.md`**Lines**: ~720
**Completed**: 2025-11-19
**Priority**: High
**Domain**: Progression
**Layer 1 Docs**: 🔗 None (this spec provides design documentation)

**Scope**:

- Ability learning mechanics within specialization trees (Tier 1=2 PP, Tier 2=4 PP, Tier 3=5 PP, Capstone=6 PP)
- Automatic rank progression system (Tier 1: Rank 1→2→3, Tier 2: Rank 2→3, Tier 3/Capstone: single rank)
- Rank advancement triggers (learning 2nd Tier 2 ability grants Rank 2, Capstone grants Rank 3)
- Rank scaling formulas (+1d6 damage/rank, +1 turn duration/rank)
- Tier unlock validation (8/16/24 PP in tree thresholds)
- PP-in-tree tracking per specialization
- Prerequisite validation (capstones require Tier 3)

**Out of Scope**:

- Skill tree structure → SPEC-PROGRESSION-002
- Ability reset/respec → SPEC-PROGRESSION-006
- Individual ability design → Implementation docs
- Per-specialization balance → Per-spec tuning

**Dependencies**:

- SPEC-PROGRESSION-001 (Character Progression) - Completed
- SPEC-PROGRESSION-002 (Archetype & Specialization System) - Completed

**Why Complete**: Automatic rank advancement is core progression mechanic; needed formal design specification for automatic triggers, scaling formulas, and milestone-driven power progression.

**Implementation Exists**: Yes (`AbilityService.cs:LearnAbility()`, `AbilityService.cs:RankUpAbility()`, `AbilityRepository.cs`)

---

## ECONOMY Domain

### ✅ SPEC-ECONOMY-003: Trauma Economy System

**Status**: Completed
**File**: `economy/trauma-economy-spec.md`**Lines**: ~650
**Completed**: 2025-11-19
**Layer 1 Docs**: 🔗 `docs/01-systems/stress-corruption.md`, `docs/01-systems/traumas.md`

**Scope**: Psychic Stress accumulation, Corruption mechanics, Breaking Points, Trauma acquisition

**Why Complete**: Trauma economy is unique to Rune & Rust and core to the horror/psychological theme.

---

### ✅ SPEC-ECONOMY-001: Loot & Equipment System

**Status**: Completed
**File**: `economy/loot-equipment-spec.md`**Lines**: ~1595
**Completed**: 2025-11-21
**Priority**: High
**Domain**: Economy
**Layer 1 Docs**: 🔗 `docs/03-equipment/equipment-overview.md`

**Scope**:

- 5 quality tier system (Jury-Rigged → Scavenged → Clan-Forged → Optimized → Myth-Forged)
- 2 equipment slots (weapon + armor) with 5-item inventory
- 11 weapon categories (Axe, Greatsword, Spear, Dagger, Staff, Focus, Blade, Blunt, EnergyMelee, Rifle, HeavyBlunt)
- 3 armor categories (Light, Medium, Heavy) with distinct trade-offs
- Enemy-based loot generation (Servitors 60% T0, Drones 40% T2, Bosses 70% T4)
- Class-appropriate loot filtering (60% for standard enemies, 100% for bosses)
- Equipment attribute bonuses beyond cap (Tier 2-4 grant +1 to +4 attributes)
- Power curve balancing (~3-5× damage increase, ~2× survivability increase from T0→T4)
- Special effects (Tier 4 Myth-Forged exclusive)
- v0.18 balance adjustments (starter weapon buffs, heavy armor FINESSE penalties)

**Out of Scope**:

- Crafting system mechanics → SPEC-ECONOMY-002
- Equipment durability/repair → Future enhancement
- Set bonuses → Future enhancement

**Dependencies**:

- SPEC-COMBAT-002 (Damage Calculation) - Completed
- SPEC-COMBAT-004 (Accuracy & Evasion) - Completed
- SPEC-PROGRESSION-001 (Character Progression) - Completed
- SPEC-PROGRESSION-002 (Archetype & Specialization) - Completed

**Why Complete**: Comprehensive design specification created for loot economy, quality tier progression, drop rate balancing, and equipment stat scaling. Includes 6 appendices with complete stat tables, tier progression examples, loot simulation, BiS recommendations, and equipment comparison formulas.

**Implementation Exists**: Yes (`RuneAndRust.Core/Equipment.cs`, `RuneAndRust.Engine/EquipmentDatabase.cs`, `RuneAndRust.Engine/LootService.cs`)

---

### 📋 SPEC-ECONOMY-002: Crafting & Resource System

**Status**: Planned
**Priority**: Medium
**Domain**: Economy
**Layer 1 Docs**: 🔗 None (no implementation or docs exist)

**Proposed Scope**:

- Crafting recipes and requirements
- Resource types and acquisition
- Component system (salvage, materials)
- Crafting success/failure mechanics
- Crafting cost economy (currency, resources, time)
- Upgrade vs. new crafting
- Integration with loot system

**Dependencies**:

- SPEC-ECONOMY-001 (Loot & Equipment) - Not yet drafted

**Why Needed**: Crafting is aspirational feature mentioned in roadmap but not yet designed.

**Implementation Exists**: No

---

## WORLD Domain

### 📋 SPEC-WORLD-001: Procedural Generation System

**Status**: Planned
**Priority**: High
**Domain**: World
**Layer 1 Docs**: 🔗 None (Wave Function Collapse docs exist in `docs/01-systems/` but need consolidation)

**Proposed Scope**:

- Wave Function Collapse algorithm parameters
- Room template system and constraints
- Biome integration with generation
- Connectivity and graph structure
- Hazard placement rules
- Boss room and special room generation
- Seed-based generation for reproducibility

**Dependencies**:

- SPEC-WORLD-002 (Biome System) - Not yet drafted
- SPEC-WORLD-003 (Hazard System) - Not yet drafted

**Why Needed**: Procedural generation is core to roguelike experience but design parameters, balance targets, and player experience goals not formally specified.

**Implementation Exists**: Yes (partial, in `RuneAndRust.Engine/Services/Generators/`)

---

### 📋 SPEC-WORLD-002: Biome & Environment System

**Status**: Planned
**Priority**: Medium
**Domain**: World
**Layer 1 Docs**: 🔗 None

**Proposed Scope**:

- Biome types (Blight-corrupted, Frozen Wastes, Runic Sanctuaries, etc.)
- Environmental themes and aesthetics
- Biome-specific hazards and mechanics
- Biome progression and difficulty scaling
- Descriptive framework integration (Layer 2 Diagnostic Voice)
- Setting compliance (Aethelgard's 9 realms)

**Dependencies**:

- SPEC-WORLD-001 (Procedural Generation) - Not yet drafted
- SPEC-WORLD-003 (Hazard System) - Not yet drafted
- SETTING_COMPLIANCE.md - Domain 8 (Geographic Fundamentals)

**Why Needed**: Biomes referenced in generation docs but environmental design philosophy and setting integration not specified.

**Implementation Exists**: No (conceptual only)

---

### 📋 SPEC-WORLD-003: Environmental Hazard System

**Status**: Planned
**Priority**: Medium
**Domain**: World
**Layer 1 Docs**: 🔗 None (hazards mentioned in combat docs)

**Proposed Scope**:

- Hazard types (Blight zones, collapsing terrain, Aether storms, etc.)
- Hazard activation and duration
- Hazard damage/effect application
- Player interaction with hazards (avoidance, mitigation, exploitation)
- Integration with combat turn sequence
- Setting compliance (Runic Blight, CPS mechanics)

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-WORLD-002 (Biome System) - Not yet drafted
- SETTING_COMPLIANCE.md - Domain 7 (Reality/Logic Rules)

**Why Needed**: Hazards are mentioned but mechanics and design intent not comprehensively specified.

**Implementation Exists**: Yes (partial, in `RuneAndRust.Engine/Services/HazardService.cs`)

---

## NARRATIVE Domain

### 📋 SPEC-NARRATIVE-001: Descriptor Framework System

**Status**: Planned
**Priority**: Medium
**Domain**: Narrative
**Layer 1 Docs**: 🔗 None (descriptor examples exist in `docs/02-statistical-registry/combat-descriptors.md`)

**Proposed Scope**:

- Descriptor generation rules and templates
- Layer 2 Diagnostic Voice implementation
- Context-aware descriptor selection
- Dynamic descriptor composition
- Trauma/stress impact on descriptors
- Setting compliance integration

**Dependencies**:

- SETTING_COMPLIANCE.md - Voice & Tone standards

**Why Needed**: Descriptors are implemented but design philosophy for voice, tone, and atmospheric consistency not formally specified.

**Implementation Exists**: Yes (in `RuneAndRust.Engine/Services/DescriptorService.cs`)

---

### ✅ SPEC-NARRATIVE-002: Dialogue System

**Status**: Completed
**File**: `specifications/DIALOGUE_SYSTEM_SPECIFICATION.md`**Lines**: ~1950
**Completed**: 2025-11-27
**Priority**: High (upgraded from Low - critical GUI gap)
**Domain**: Narrative
**Layer 1 Docs**: 🔗 `RuneAndRust.Engine/DialogueService.cs`, `Data/Dialogues/*.json`

**Scope**:

- Dialogue tree structure (DialogueNode, DialogueOption, DialogueOutcome)
- Branching conversation mechanics with skill-gated options
- Hard skill check system (attribute and specialization-based)
- 8 OutcomeTypes (Information, QuestGiven, QuestComplete, ReputationChange, ItemReceived, ItemLost, InitiateCombat, EndConversation)
- NPC disposition system (-100 to +100 with 5 tiers)
- Topic history tracking and first-meeting flags
- GUI requirements (pending implementation)
- 12 functional requirements (FR-001 through FR-012)
- Complete data format specification with JSON examples
- Integration points with Faction, Quest, Combat, and Merchant systems

**Dependencies**:

- SPEC-PROGRESSION-001 (Character Progression) - Completed (for skill checks)
- SPEC-FACTION-001 (Faction Reputation) - Planned (for reputation outcomes)

**Why Complete**: Dialogue system identified as critical GUI gap (0% coverage) in GUI_GAP_ANALYSIS.md. Core v0.8 implementation exists with DialogueService (313 lines), 8 NPC dialogue files (50+ nodes), and full skill check filtering. GUI implementation pending.

**Implementation Exists**: Yes

- `RuneAndRust.Core/Dialogue/DialogueNode.cs`
- `RuneAndRust.Core/Dialogue/DialogueOption.cs`
- `RuneAndRust.Core/Dialogue/DialogueOutcome.cs`
- `RuneAndRust.Core/Dialogue/SkillCheckRequirement.cs`
- `RuneAndRust.Engine/DialogueService.cs`
- `RuneAndRust.Engine/Commands/TalkCommand.cs`
- `Data/Dialogues/*.json` (8 files)

---

### 📋 SPEC-NARRATIVE-003: Quest & Objective System

**Status**: Planned
**Priority**: Low
**Domain**: Narrative
**Layer 1 Docs**: 🔗 None

**Proposed Scope**:

- Quest generation and templates
- Objective types (kill, fetch, explore, survive)
- Quest tracking and progress
- Reward structures (Legend, loot, reputation)
- Dynamic quest generation based on player state
- Integration with procedural generation

**Dependencies**:

- SPEC-WORLD-001 (Procedural Generation) - Not yet drafted
- SPEC-NARRATIVE-004 (Faction System) - Not yet drafted

**Why Needed**: Quest system is aspirational feature for structured gameplay.

**Implementation Exists**: No

---

### 📋 SPEC-NARRATIVE-004: Faction & Reputation System

**Status**: Planned
**Priority**: Low
**Domain**: Narrative
**Layer 1 Docs**: 🔗 None

**Proposed Scope**:

- Faction definitions and relationships
- Reputation tracking and thresholds
- Faction-based rewards and consequences
- Territory control and influence
- Faction quests and storylines
- Setting compliance (Aethelgard factions, political landscape)

**Dependencies**:

- SPEC-NARRATIVE-002 (Dialogue System) - Not yet drafted
- SETTING_COMPLIANCE.md - Domain 1 (Cosmology), Domain 8 (Geographic Fundamentals)

**Why Needed**: Factions are part of setting but game mechanics not designed.

**Implementation Exists**: No

---

## Priority Matrix

### High Priority (Core Gameplay Loop)

These specs are needed for complete documentation of existing implemented systems:

1. **SPEC-COMBAT-002: Damage Calculation System** - Existing implementation needs design spec
2. **SPEC-COMBAT-003: Status Effects System** - Existing implementation needs design spec
3. **SPEC-PROGRESSION-002: Archetype & Specialization System** - Existing implementation needs design spec
4. **SPEC-PROGRESSION-003: Ability & Skill Advancement System** - Existing implementation needs design spec
5. **SPEC-ECONOMY-001: Loot & Equipment System** - Existing implementation needs design spec
6. **SPEC-WORLD-001: Procedural Generation System** - Existing implementation needs design spec

### Medium Priority (Enhanced Gameplay)

These specs support existing features or near-term planned features:

1. **SPEC-COMBAT-004: Accuracy & Evasion System** - Existing implementation, medium complexity
2. **SPEC-ECONOMY-002: Crafting & Resource System** - Aspirational, adds depth
3. **SPEC-WORLD-002: Biome & Environment System** - Enhances procedural generation
4. **SPEC-WORLD-003: Environmental Hazard System** - Partial implementation exists
5. **SPEC-NARRATIVE-001: Descriptor Framework System** - Existing implementation needs design spec

### Low Priority (Future Features)

These specs are for systems not yet implemented, lower impact on core gameplay:

1. **SPEC-NARRATIVE-002: Dialogue & NPC Interaction System** - No implementation yet
2. **SPEC-NARRATIVE-003: Quest & Objective System** - No implementation yet
3. **SPEC-NARRATIVE-004: Faction & Reputation System** - No implementation yet

---

## Recommended Drafting Order

Based on dependencies and priority, recommended order for drafting specifications:

### Phase 1: Core Combat & Progression (4 specs)

1. **SPEC-COMBAT-002: Damage Calculation** - No dependencies, foundation for other combat specs
2. **SPEC-COMBAT-003: Status Effects** - Depends on SPEC-COMBAT-002
3. **SPEC-PROGRESSION-002: Archetype & Specialization** - Minimal dependencies
4. **SPEC-PROGRESSION-003: Ability & Skill Advancement** - Depends on SPEC-PROGRESSION-002

### Phase 2: Economy & World (3 specs)

1. **SPEC-ECONOMY-001: Loot & Equipment** - Depends on SPEC-COMBAT-002 (for weapon stats)
2. **SPEC-WORLD-001: Procedural Generation** - Minimal dependencies, foundational
3. **SPEC-WORLD-003: Environmental Hazard** - Depends on SPEC-WORLD-001

### Phase 3: Polish & Enhancement (4 specs)

1. **SPEC-COMBAT-004: Accuracy & Evasion** - Depends on SPEC-COMBAT-003
2. **SPEC-WORLD-002: Biome & Environment** - Depends on SPEC-WORLD-001
3. **SPEC-NARRATIVE-001: Descriptor Framework** - Minimal dependencies
4. **SPEC-ECONOMY-002: Crafting & Resource** - Depends on SPEC-ECONOMY-001

### Phase 4: Future Systems (3 specs)

1. **SPEC-NARRATIVE-004: Faction & Reputation** - Foundational for other narrative specs
2. **SPEC-NARRATIVE-002: Dialogue & NPC Interaction** - Depends on SPEC-NARRATIVE-004
3. **SPEC-NARRATIVE-003: Quest & Objective** - Depends on SPEC-NARRATIVE-004, SPEC-WORLD-001

---

## Additional Identified Systems (20 Specs)

The following specifications have been identified based on existing implementations, services, and data files in the codebase. These represent real, implemented functionality that needs design-level documentation.

### COMBAT Domain (Additional 7 specs)

### ✅ SPEC-COMBAT-005: Boss Encounter System

**Status**: Completed
**File**: `combat/boss-encounter-spec.md`**Lines**: ~1530
**Completed**: 2025-11-22
**Priority**: High
**Domain**: Combat
**Layer 1 Docs**: 🔗 `BOSS_COMBAT_INTEGRATION_GUIDE.md`

**Scope**:

- Multi-phase boss combat mechanics (2-4 phases per boss)
- HP threshold-based phase transitions (75%, 50%, 25%)
- Telegraphed ability system with charge times and interrupt mechanics
- Enrage triggers (HP-based and turn-based timers)
- Vulnerability windows after ultimate abilities
- Phase-specific stat modifiers (damage, defense, regeneration, bonus actions)
- Add wave spawning during phase transitions
- Invulnerability mechanics during phase transitions
- Boss loot table system (artifacts, guaranteed drops, unique items, set bonuses)
- 4 implemented boss archetypes (Ruin-Warden, Aetheric Aberration, Forlorn Archivist, Omega Sentinel)
- Balance targets (TTK, damage output, phase timing)
- Boss encounter design checklist

**Out of Scope**:

- General enemy design guidelines → SPEC-COMBAT-012
- Standard enemy AI behavior → SPEC-AI-001 (planned)
- Movement and positioning mechanics → SPEC-COMBAT-009 (planned)
- Status effect mechanics → SPEC-COMBAT-003
- Individual boss lore and narrative descriptions → Bestiary entries

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-002 (Damage Calculation) - Completed
- SPEC-COMBAT-003 (Status Effects) - Completed
- SPEC-COMBAT-012 (Enemy Design & Bestiary) - Completed
- SPEC-ECONOMY-001 (Loot & Equipment) - Completed
- SPEC-PROGRESSION-001 (Character Progression) - Completed

**Why Complete**: Boss encounter system is fully implemented; needed comprehensive design specification for multi-phase mechanics, telegraphed abilities, enrage system, and boss loot economy.

**Implementation Exists**: Yes (`RuneAndRust.Engine/BossEncounterService.cs`, `BossCombatIntegration.cs`, `BossDatabase.cs`, `BossLootService.cs`, `TelegraphedAbilityService.cs`)

**Key Features**:

- 6 functional requirements (FR-001 through FR-006) covering full boss encounter lifecycle
- Detailed phase transition mechanics with invulnerability and stat modifiers
- Comprehensive telegraphed ability system (4 ability types: Standard, Telegraphed, Ultimate, Passive)
- Interrupt mechanics with damage thresholds and stagger effects
- Vulnerability window system for ultimate abilities
- Enrage triggers with HP-based and turn-based conditions
- Boss loot system with guaranteed quality drops, artifacts, unique items, crafting materials
- Balance targets for TTK (8-25 turns), damage output, and phase timing
- 3 appendices: Boss stat reference tables, design checklist, full Ruin-Warden example

---

### 📋 SPEC-COMBAT-006: Counter-Attack & Parry System

**Status**: Planned
**Priority**: Medium
**Domain**: Combat
**Layer 1 Docs**: None (see `COUNTER_ATTACK_INTEGRATION.md`)

**Proposed Scope**:

- Universal parry mechanics (all characters can parry)
- Parry quality tiers (Failed/Standard/Superior/Critical)
- Riposte trigger conditions and execution
- Specialization parry bonuses (Hólmgangr, Atgeir-wielder)
- Parry limitations (per-turn limits, stamina costs)
- Counter-attack damage calculation
- Trauma economy integration (stress on failed parry)

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-002 (Damage Calculation) - Not yet drafted

**Why Needed**: Counter-attack system is fully implemented with CounterAttackService and database persistence but lacks design specification for parry mechanics and balance tuning.

**Implementation Exists**: Yes (`RuneAndRust.Engine/CounterAttackService.cs`)

---

### 📋 SPEC-COMBAT-007: Cover System

**Status**: Planned
**Priority**: Medium
**Domain**: Combat
**Layer 1 Docs**: None

**Proposed Scope**:

- Cover types (half cover, full cover, destructible cover)
- Cover bonus calculation (defense, evasion modifiers)
- Breaking line of sight mechanics
- Cover destruction and durability
- Integration with positioning and movement
- Environmental cover vs. constructed cover

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-004 (Accuracy & Evasion) - Not yet drafted
- SPEC-WORLD-005 (Environmental Objects) - Not yet drafted

**Why Needed**: CoverService exists and is integrated into combat but design philosophy, cover values, and balance targets not documented.

**Implementation Exists**: Yes (`RuneAndRust.Engine/CoverService.cs`)

---

### 📋 SPEC-COMBAT-008: Flanking System

**Status**: Planned
**Priority**: Medium
**Domain**: Combat
**Layer 1 Docs**: None

**Proposed Scope**:

- Flanking position determination (geometric calculation)
- Flanking bonuses (accuracy, damage modifiers)
- Formation interaction with flanking
- Multiple flanker mechanics
- Anti-flanking abilities and counters
- Integration with positioning service

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-009 (Movement & Positioning) - Not yet drafted

**Why Needed**: FlankingService exists for tactical combat positioning but lacks design specification for flanking rules and bonuses.

**Implementation Exists**: Yes (`RuneAndRust.Engine/FlankingService.cs`)

---

### 📋 SPEC-COMBAT-009: Movement & Positioning System

**Status**: Planned
**Priority**: High
**Domain**: Combat
**Layer 1 Docs**: None

**Proposed Scope**:

- Movement range and action economy (move + attack, double move)
- Advanced movement types (dash, teleport, leap)
- Coordinated movement (formation movement, companion positioning)
- Forced movement (knockback, pull, displacement)
- Movement cost modifiers (terrain, status effects)
- Positioning service (grid positions, distance calculation)
- Opportunity attacks on movement

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed

**Why Needed**: Multiple movement services exist (AdvancedMovementService, CoordinatedMovementService, ForcedMovementService, PositioningService, FormationService) but lack unified design specification.

**Implementation Exists**: Yes (`RuneAndRust.Engine/AdvancedMovementService.cs`, `CoordinatedMovementService.cs`, `ForcedMovementService.cs`, `PositioningService.cs`, `FormationService.cs`)

---

### 📋 SPEC-COMBAT-010: Telegraphed Abilities System

**Status**: Planned
**Priority**: Medium
**Domain**: Combat
**Layer 1 Docs**: None

**Proposed Scope**:

- Telegraphed ability mechanics (announce turn, execute turn)
- Player counterplay options (interrupt, dodge, block)
- Visual/text indicators for telegraphed attacks
- Interrupt system (interrupt sources, success conditions)
- Boss telegraphed abilities vs. standard enemy
- Failure states (interrupted, dodged, blocked)

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-005 (Boss Encounters) - Not yet drafted

**Why Needed**: TelegraphedAbilityService exists for boss mechanics and complex enemy behaviors but lacks design specification.

**Implementation Exists**: Yes (`RuneAndRust.Engine/TelegraphedAbilityService.cs`)

---

### 📋 SPEC-COMBAT-011: Environmental Combat System

**Status**: Planned
**Priority**: Medium
**Domain**: Combat
**Layer 1 Docs**: None

**Proposed Scope**:

- Environmental feature types (chasms, pillars, explosive barrels)
- Environmental manipulation (push into hazard, use object as weapon)
- Environmental stress triggers (psychic stress from surroundings)
- Destructible environment interaction
- Terrain advantage/disadvantage
- Integration with hazard system

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-WORLD-003 (Environmental Hazards) - Not yet drafted
- SPEC-WORLD-005 (Environmental Objects) - Not yet drafted

**Why Needed**: EnvironmentalCombatService, EnvironmentalManipulationService, and EnvironmentalStressService exist but lack unified design specification.

**Implementation Exists**: Yes (`RuneAndRust.Engine/EnvironmentalCombatService.cs`, `EnvironmentalManipulationService.cs`, `EnvironmentalStressService.cs`)

---

### WORLD Domain (Additional 5 specs)

### 📋 SPEC-WORLD-004: Room Template System

**Status**: Planned
**Priority**: High
**Domain**: World
**Layer 1 Docs**: None (see `PROCEDURAL_GENERATION.md`)

**Proposed Scope**:

- Room template structure and JSON schema
- Room categories (corridors, chambers, junctions, boss arenas, secret rooms)
- Connector system (entry/exit points, direction constraints)
- Room population (enemy placement, loot placement, hazard placement)
- Biome-specific room templates
- Handcrafted rooms vs. procedural variations
- Template library organization

**Dependencies**:

- SPEC-WORLD-001 (Procedural Generation) - Not yet drafted
- SPEC-WORLD-002 (Biome System) - Not yet drafted

**Why Needed**: Extensive room template data exists in `Data/RoomTemplates/` with multiple categories, and RoomInstantiator/HandcraftedRoomLibrary services exist, but design philosophy for room creation not documented.

**Implementation Exists**: Yes (`Data/RoomTemplates/`, `RuneAndRust.Engine/RoomInstantiator.cs`, `HandcraftedRoomLibrary.cs`, `TemplateLibrary.cs`)

---

### 📋 SPEC-WORLD-005: Environmental Objects & Destruction System

**Status**: Planned
**Priority**: Medium
**Domain**: World
**Layer 1 Docs**: None

**Proposed Scope**:

- Environmental object types (crates, pillars, doors, barriers)
- Object interaction mechanics (examine, manipulate, destroy)
- Destruction system (HP pools, destruction triggers, debris)
- Persistent destruction (world state tracking)
- Object-based cover and obstacles
- Loot from destroyed objects
- Environmental storytelling via objects

**Dependencies**:

- SPEC-WORLD-001 (Procedural Generation) - Not yet drafted
- SPEC-COMBAT-007 (Cover System) - Not yet drafted

**Why Needed**: EnvironmentalObjectService, EnvironmentalFeatureService, and DestructionService exist but lack design specification for object interaction and destruction mechanics.

**Implementation Exists**: Yes (`RuneAndRust.Engine/EnvironmentalObjectService.cs`, `EnvironmentalFeatureService.cs`, `DestructionService.cs`, `ObjectInteractionService.cs`)

---

### 📋 SPEC-WORLD-006: Ambient Conditions & Weather System

**Status**: Planned
**Priority**: Low
**Domain**: World
**Layer 1 Docs**: None

**Proposed Scope**:

- Ambient condition types (Blight fog, Aether storms, frigid cold, intense heat)
- Weather effect mechanics and gameplay impact
- Condition-based status effects and penalties
- Biome-specific ambient conditions
- Atmospheric descriptor integration
- Setting compliance (Aethelgard environmental lore)

**Dependencies**:

- SPEC-WORLD-002 (Biome System) - Not yet drafted
- SPEC-NARRATIVE-001 (Descriptor Framework) - Not yet drafted

**Why Needed**: AmbientConditionService, WeatherEffectService, AtmosphericDescriptorService, FrigidColdService, and IntenseHeatService exist but lack design specification.

**Implementation Exists**: Yes (`RuneAndRust.Engine/AmbientConditionService.cs`, `WeatherEffectService.cs`, `AtmosphericDescriptorService.cs`, `FrigidColdService.cs`, `IntenseHeatService.cs`)

---

### 📋 SPEC-WORLD-007: Trap System

**Status**: Planned
**Priority**: Low
**Domain**: World
**Layer 1 Docs**: None

**Proposed Scope**:

- Trap types (pressure plates, tripwires, magical wards, Blight corruption zones)
- Trap detection mechanics (passive FINESSE checks, active searching)
- Trap disarming (skill checks, failure consequences)
- Trap triggering and damage/effects
- Trap density and placement in procedural generation
- Respawning traps vs. one-time traps

**Dependencies**:

- SPEC-WORLD-001 (Procedural Generation) - Not yet drafted

**Why Needed**: TrapService exists but lacks design specification for trap mechanics and detection/disarming rules.

**Implementation Exists**: Yes (`RuneAndRust.Engine/TrapService.cs`)

---

### 📋 SPEC-WORLD-008: Resource Node System

**Status**: Planned
**Priority**: Low
**Domain**: World
**Layer 1 Docs**: None

**Proposed Scope**:

- Resource node types (scrap piles, Aether crystals, fungal gardens, data cores)
- Harvesting mechanics and skill checks
- Resource yield and randomization
- Node respawning and depletion
- Quest anchor integration (special resource nodes)
- Setting compliance (resource types aligned with Aethelgard lore)

**Dependencies**:

- SPEC-ECONOMY-002 (Crafting System) - Not yet drafted
- SPEC-WORLD-001 (Procedural Generation) - Not yet drafted

**Why Needed**: ResourceNodeService exists and quest anchors reference resource nodes but design specification for resource economy not documented.

**Implementation Exists**: Yes (`RuneAndRust.Engine/ResourceNodeService.cs`, `Data/QuestAnchors/`)

---

### NARRATIVE Domain (Additional 2 specs)

### 📋 SPEC-NARRATIVE-005: NPC System

**Status**: Planned
**Priority**: Medium
**Domain**: Narrative
**Layer 1 Docs**: None

**Proposed Scope**:

- NPC definition structure (name, faction, dialogue, inventory)
- NPC spawning and placement
- NPC persistence across game sessions
- NPC faction reactions and relationships
- Merchant NPCs vs. quest NPCs vs. flavor NPCs
- NPC flavor text generation
- Integration with dialogue, quest, and merchant systems

**Dependencies**:

- SPEC-NARRATIVE-002 (Dialogue System) - Not yet drafted
- SPEC-FACTION-001 (Faction Reputation) - Not yet drafted

**Why Needed**: NPCService and NPCFlavorTextService exist, 11 NPC data files exist in `Data/NPCs/`, and dialogue files exist in `Data/Dialogues/`, but design specification missing.

**Implementation Exists**: Yes (`RuneAndRust.Engine/NPCService.cs`, `NPCFlavorTextService.cs`, `NPCFactionReactions.cs`, `Data/NPCs/`, `Data/Dialogues/`)

---

### 📋 SPEC-NARRATIVE-006: Companion System

**Status**: Planned
**Priority**: Medium
**Domain**: Narrative
**Layer 1 Docs**: None (see `COMPANION_INTEGRATION_GUIDE.md`)

**Proposed Scope**:

- 6 base companion definitions (recruitment, personalities, abilities)
- Companion recruitment mechanics and prerequisites
- Companion progression system (leveling, equipment, abilities)
- Companion personal quests
- Companion AI behavior in combat
- Party composition and management
- Companion-specific dialogue and reactions

**Dependencies**:

- SPEC-PROGRESSION-001 (Character Progression) - Completed
- SPEC-NARRATIVE-002 (Dialogue System) - Not yet drafted
- SPEC-AI-002 (Companion AI) - Not yet drafted

**Why Needed**: Companion system is fully implemented with database schema, CompanionService, CompanionAIService, CompanionProgressionService, 18 companion abilities, but lacks design specification.

**Implementation Exists**: Yes (`RuneAndRust.Engine/CompanionService.cs`, `CompanionAIService.cs`, `CompanionProgressionService.cs`, database tables for companions)

---

### PROGRESSION Domain (Additional 2 specs)

### 📋 SPEC-PROGRESSION-004: Specialization System

**Status**: Planned
**Priority**: High
**Domain**: Progression
**Layer 1 Docs**: None (see `SPECIALIZATION_CREATION_GUIDE.md`)

**Proposed Scope**:

- Specialization selection and restrictions (distinct from Archetype)
- Specialization-specific abilities and passive bonuses
- Specialization progression and unlocks
- Multi-specialization rules (if any)
- Specialization identity and playstyle design
- Validation rules and constraints

**Dependencies**:

- SPEC-PROGRESSION-002 (Archetype System) - Not yet drafted
- SPEC-PROGRESSION-003 (Ability Advancement) - Not yet drafted

**Why Needed**: SpecializationService, SpecializationFactory, SpecializationValidator exist with SPECIALIZATION_CREATION_GUIDE.md but design philosophy and progression mechanics not formally specified. Note: This is distinct from Archetypes (Mystic, Warden, etc.).

**Implementation Exists**: Yes (`RuneAndRust.Engine/SpecializationService.cs`, `SpecializationFactory.cs`, `SpecializationValidator.cs`)

---

### 📋 SPEC-PROGRESSION-005: Stance System

**Status**: Planned
**Priority**: Low
**Domain**: Progression
**Layer 1 Docs**: None

**Proposed Scope**:

- Stance types and mechanical effects (offensive, defensive, balanced)
- Stance switching mechanics (action cost, restrictions)
- Stance-specific bonuses and penalties
- Integration with combat abilities
- Archetype/specialization stance synergies

**Dependencies**:

- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-PROGRESSION-002 (Archetype System) - Completed

**Why Needed**: StanceService exists but lacks design specification for stance mechanics and switching rules.

**Implementation Exists**: Yes (`RuneAndRust.Engine/StanceService.cs`)

---

### 📋 SPEC-PROGRESSION-006: Ability Respec & Reset System

**Status**: Planned
**Priority**: Low
**Domain**: Progression
**Layer 1 Docs**: None

**Proposed Scope**:

- Ability unlearning mechanics (refund PP or permanent?)
- Rank reset rules (can abilities be de-ranked?)
- Respec costs and limitations (currency, one-time, cooldowns)
- Specialization unlearning (if allowed)
- Full character respec vs. partial ability respec
- Persistence of respec history (for analytics/restrictions)

**Dependencies**:

- SPEC-PROGRESSION-003 (Ability Rank Advancement) - Completed
- SPEC-PROGRESSION-001 (Character Progression) - Completed

**Why Needed**: Allows player experimentation and build pivots without starting new character. Design philosophy not yet decided (permanent choices vs. flexible respec).

**Implementation Exists**: No (future feature)

---

### ECONOMY Domain (Additional 3 specs)

### 📋 SPEC-ECONOMY-004: Currency & Transaction System

**Status**: Planned
**Priority**: Medium
**Domain**: Economy
**Layer 1 Docs**: None

**Proposed Scope**:

- Currency types (scrap, favor tokens, faction currency)
- Currency sources (loot, quests, selling items)
- Currency sinks (merchants, crafting, services)
- Transaction mechanics (buy, sell, barter)
- Currency conversion and exchange rates
- Economy balance targets (income vs. expenses)

**Dependencies**:

- SPEC-ECONOMY-001 (Loot & Equipment) - Not yet drafted
- SPEC-ECONOMY-005 (Merchant System) - Not yet drafted

**Why Needed**: CurrencyService and TransactionService exist but currency economy and balance targets not formally specified.

**Implementation Exists**: Yes (`RuneAndRust.Engine/CurrencyService.cs`, `TransactionService.cs`)

---

### 📋 SPEC-ECONOMY-005: Merchant & Pricing System

**Status**: Planned
**Priority**: Medium
**Domain**: Economy
**Layer 1 Docs**: None

**Proposed Scope**:

- Merchant types (wandering, faction merchants, specialty vendors)
- Merchant inventory generation and refresh
- Dynamic pricing (reputation modifiers, rarity, supply/demand)
- Faction merchant inventory differences
- Buy/sell price ratios
- Special merchant services (repairs, crafting commissions)

**Dependencies**:

- SPEC-ECONOMY-001 (Loot & Equipment) - Not yet drafted
- SPEC-ECONOMY-004 (Currency & Transaction) - Not yet drafted
- SPEC-FACTION-001 (Faction Reputation) - Not yet drafted

**Why Needed**: MerchantService, PricingService, and MerchantFactionInventory exist with faction-based pricing modifiers but design specification missing.

**Implementation Exists**: Yes (`RuneAndRust.Engine/MerchantService.cs`, `PricingService.cs`, `MerchantFactionInventory.cs`)

---

### 📋 SPEC-ECONOMY-006: Consumables System

**Status**: Planned
**Priority**: Medium
**Domain**: Economy
**Layer 1 Docs**: None

**Proposed Scope**:

- Consumable types (healing items, buffs, debuff cures, utility items)
- Consumable usage mechanics (combat vs. out-of-combat)
- Consumable rarity and acquisition
- Field medicine crafting integration
- Consumable stacking and inventory limits
- Balance targets (healing potency, buff duration)

**Dependencies**:

- SPEC-ECONOMY-001 (Loot & Equipment) - Not yet drafted
- SPEC-ECONOMY-002 (Crafting System) - Not yet drafted

**Why Needed**: ConsumableDatabase exists with consumable definitions and FieldMedicineCraftingService exists but design specification for consumables missing.

**Implementation Exists**: Yes (`RuneAndRust.Engine/ConsumableDatabase.cs`, `FieldMedicineCraftingService.cs`)

---

### FACTION Domain (New, 3 specs)

### 📋 SPEC-FACTION-001: Faction Reputation System

**Status**: Planned
**Priority**: High
**Domain**: Faction
**Layer 1 Docs**: None (see `FACTION_SYSTEM_TEST_COVERAGE.md`)

**Proposed Scope**:

- 5 faction definitions (Iron-Banes, Jötun-Readers, Rust Clans, God-Sleepers, Independents)
- Reputation tiers (Exalted, Allied, Friendly, Neutral, Hostile, Hated)
- Reputation gain/loss mechanics (quest completion, enemy kills, choices)
- Reputation tier thresholds (-100 to +100 scale)
- Faction relationships and rivalries (reputation with one affects others)
- Price modifiers by reputation tier
- Faction-specific rewards and benefits

**Dependencies**:

- SPEC-NARRATIVE-003 (Quest System) - Not yet drafted

**Why Needed**: Faction system is fully implemented with 98 tests, FactionService, ReputationService, 5 factions, 25 faction quests, but lacks design-level specification.

**Implementation Exists**: Yes (`RuneAndRust.Engine/FactionService.cs`, `ReputationService.cs`, `FactionEncounterService.cs`, `Data/Quests/faction_*.json`)

---

### 📋 SPEC-FACTION-002: Faction War System

**Status**: Planned
**Priority**: Low
**Domain**: Faction
**Layer 1 Docs**: None

**Proposed Scope**:

- Faction conflict mechanics (territory disputes, skirmishes)
- Faction war triggers and escalation
- Player involvement in faction wars (choose sides, mercenary)
- War outcome effects (territory control changes, reputation shifts)
- Peace and ceasefire mechanics

**Dependencies**:

- SPEC-FACTION-001 (Faction Reputation) - Not yet drafted
- SPEC-FACTION-003 (Territory Control) - Not yet drafted

**Why Needed**: FactionWarService exists but design specification for faction conflict mechanics missing.

**Implementation Exists**: Yes (`RuneAndRust.Engine/FactionWarService.cs`)

---

### 📋 SPEC-FACTION-003: Territory Control System

**Status**: Planned
**Priority**: Medium
**Domain**: Faction
**Layer 1 Docs**: None (see `TERRITORY_INTEGRATION_GUIDE.md`)

**Proposed Scope**:

- Territory definition and boundaries
- Faction territory ownership and influence
- Territory control mechanics (capture, hold, lose)
- World events triggered by territory changes
- Player actions affecting territory control
- Territory-based faction encounter rates
- Daily update loop for territory shifts

**Dependencies**:

- SPEC-FACTION-001 (Faction Reputation) - Not yet drafted
- SPEC-WORLD-001 (Procedural Generation) - Not yet drafted

**Why Needed**: Territory control system is 100% implemented with database schema, TerritoryService, TerritoryControlService, WorldEventService, 170+ tests, but lacks design specification.

**Implementation Exists**: Yes (`RuneAndRust.Engine/TerritoryService.cs`, `TerritoryControlService.cs`, `WorldEventService.cs`, `TerritorialQuestGenerator.cs`)

---

### SYSTEMS Domain (New, 6 specs - ALL COMPLETED)

### ✅ SPEC-SYSTEM-001: Save/Load System

**Status**: Completed
**File**: `systems/save-load-system-spec.md`**Completed**: 2025-11-27
**Priority**: Critical
**Domain**: Systems

**Scope**: Save/load mechanics, data persistence, schema migrations, JSON serialization

**Implementation Exists**: Yes (`SaveRepository.cs`, `SaveData.cs`, `SaveGameService.cs`)

---

### ✅ SPEC-SYSTEM-002: Inventory & Equipment System

**Status**: Completed
**File**: `systems/inventory-equipment-spec.md`**Completed**: 2025-11-27
**Priority**: Critical
**Domain**: Systems

**Scope**: Inventory capacity, equipment slots, quality tiers, stat bonuses, equip/unequip

**Implementation Exists**: Yes (`EquipmentService.cs`, `Equipment.cs`)

---

### ✅ SPEC-SYSTEM-003: Vertical Movement (Z-Axis) System

**Status**: Completed
**File**: `systems/vertical-movement-spec.md`**Completed**: 2025-11-27
**Priority**: Critical
**Domain**: Systems

**Scope**: 7 vertical layers, connection types, traversal mechanics, fall damage

**Implementation Exists**: Yes (`VerticalTraversalService.cs`, `VerticalLayer.cs`, `VerticalConnection.cs`)

---

### ✅ SPEC-SYSTEM-004: Crafting System

**Status**: Completed
**File**: `systems/crafting-system-spec.md`**Completed**: 2025-11-27
**Priority**: High
**Domain**: Systems

**Scope**: Recipe system, crafting stations, components, skill checks, quality calculation, modifications

**Implementation Exists**: Yes (`AdvancedCraftingService.cs`, `ModificationService.cs`)

---

### ✅ SPEC-SYSTEM-007: Encounter Generation System

**Status**: Completed
**File**: `systems/encounter-generation-spec.md`**Completed**: 2025-11-27
**Priority**: High
**Domain**: Systems

**Scope**: Encounter budget, enemy composition, difficulty scaling, boss phases

**Implementation Exists**: Yes (`EncounterService.cs`, `BudgetDistributionService.cs`)

---

### ✅ SPEC-SYSTEM-008: Procedural Room Generation System

**Status**: Completed
**File**: `systems/procedural-room-generation-spec.md`**Completed**: 2025-11-27
**Priority**: High
**Domain**: Systems

**Scope**: Room templates, layout generation, hazard placement, biome theming, secret rooms

**Implementation Exists**: Yes (`DungeonGenerator.cs`, `RoomPopulationService.cs`)

---

### AI Domain (2 specs - 1 COMPLETED)

### ✅ SPEC-SYSTEM-005: Enemy AI Behavior System

**Status**: Completed
**File**: `systems/enemy-ai-behavior-spec.md`**Completed**: 2025-11-27
**Priority**: High
**Domain**: AI

**Scope**: AI archetypes, threat assessment, target selection, ability prioritization, boss AI, adaptive difficulty

**Implementation Exists**: Yes (`EnemyAIOrchestrator.cs`, `ThreatAssessmentService.cs`, `TargetSelectionService.cs`)

---

### 📋 SPEC-AI-002: Companion AI System

**Status**: Planned
**Priority**: Medium
**Domain**: AI
**Layer 1 Docs**: None

**Proposed Scope**:

- Companion AI behavior modes (autonomous, player-directed, custom tactics)
- Companion targeting logic
- Companion ability usage AI
- Companion positioning and formation behavior
- Companion self-preservation AI (healing, retreat)
- Player control vs. autonomous action balance

**Dependencies**:

- SPEC-NARRATIVE-006 (Companion System) - Not yet drafted
- SPEC-COMBAT-009 (Movement & Positioning) - Not yet drafted

**Why Needed**: CompanionAIService exists for autonomous companion behavior but design specification missing.

**Implementation Exists**: Yes (`RuneAndRust.Engine/CompanionAIService.cs`, `CompanionCommands.cs`)

---

## How to Use This Backlog

### For Planning Work

1. Review priority matrix and recommended drafting order
2. Select next spec based on dependencies and project needs
3. Check if Layer 1 docs exist (indicates implementation to reference)
4. Check setting compliance domains that apply

### For Starting a New Spec

1. Find the spec in this backlog
2. Copy the "Proposed Scope" to inform your planning (Step 3 of START_HERE.md workflow)
3. Review dependencies to understand integration requirements
4. Follow [START_HERE.md](https://www.notion.so/START_HERE.md) 5-step workflow

### For Tracking Progress

- Update status from 📋 Planned → 🚧 In Progress when drafting begins
- Update status from 🚧 In Progress → ✅ Completed when spec is committed
- Add "Completed" date and file path
- Update the Quick Status Summary table

### For Identifying Gaps

- Systems with 🔗 **Has Layer 1 Docs** but no spec are good candidates (implementation exists, need design documentation)
- Systems with no Layer 1 docs and no spec are aspirational (design first, implement later)

---

## Template for Adding New Specs

When you identify a new system that needs a specification, add an entry using this template:

```markdown
### 📋 SPEC-{DOMAIN}-{NUMBER}: {System Name}
**Status**: Planned
**Priority**: High/Medium/Low
**Domain**: Combat/Progression/Economy/World/Narrative
**Layer 1 Docs**: 🔗 {path} OR None

**Proposed Scope**:
- Bullet point 1
- Bullet point 2
- Bullet point 3

**Dependencies**:
- SPEC-XXX-YYY - Status
- SPEC-XXX-YYY - Status

**Why Needed**: {Justification for creating this spec}

**Implementation Exists**: Yes/No ({path if yes})

```

---

## Related Documentation

- [**START_HERE.md**](https://www.notion.so/START_HERE.md) - Quick start guide for drafting specifications
- [**TEMPLATE.md**](https://www.notion.so/TEMPLATE.md) - Master specification template
- [**README.md**](https://www.notion.so/README.md) - Complete specification writing guide
- [**SETTING_COMPLIANCE.md**](https://www.notion.so/SETTING_COMPLIANCE.md) - Aethelgard setting validation

---

**Last Updated**: 2025-11-27
**Maintained By**: Specification governance framework
**Update Frequency**: Update this document whenever specifications are completed or new systems are identified