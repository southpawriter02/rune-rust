# Specification Backlog & Registry

**Purpose**: Central tracking document for all specifications (existing, in-progress, and planned)

**Last Updated**: 2025-11-19
**Total Specs**: 3 completed, 16 planned

---

## Quick Status Summary

| Domain | Completed | Planned | Total | Coverage |
|--------|-----------|---------|-------|----------|
| Combat | 1 | 3 | 4 | 25% |
| Progression | 1 | 2 | 3 | 33% |
| Economy | 1 | 2 | 3 | 33% |
| World | 0 | 3 | 3 | 0% |
| Narrative | 0 | 4 | 4 | 0% |
| **TOTAL** | **3** | **14** | **17** | **18%** |

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
**File**: `combat/combat-resolution-spec.md`
**Lines**: ~600
**Completed**: 2025-11-19
**Layer 1 Docs**: 🔗 `docs/01-systems/combat-resolution.md`

**Scope**: Initiative system, turn sequence, combat state management, victory/defeat/flee conditions

**Why Complete**: Core combat flow is foundational to all combat-related systems.

---

### 📋 SPEC-COMBAT-002: Damage Calculation System
**Status**: Planned
**Priority**: High
**Domain**: Combat
**Layer 1 Docs**: 🔗 `docs/01-systems/damage-calculation.md`

**Proposed Scope**:
- Base damage calculation formulas
- Damage type system (Physical, Aetheric, Corruption)
- Armor/resistance application
- Critical hits and critical failures
- Damage mitigation mechanics
- Integration with equipment stats

**Dependencies**:
- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-ECONOMY-001 (Equipment System) - Not yet drafted

**Why Needed**: Damage calculation is referenced in combat resolution but not fully specified. Layer 1 docs exist but design-level spec needed for consistency.

**Implementation Exists**: Yes (partial, in `RuneAndRust.Engine/Services/CombatEngine.cs`)

---

### 📋 SPEC-COMBAT-003: Status Effects System
**Status**: Planned
**Priority**: High
**Domain**: Combat
**Layer 1 Docs**: 🔗 `docs/01-systems/status-effects.md`

**Proposed Scope**:
- Status effect types (buffs, debuffs, DoT, HoT)
- Duration tracking and expiration
- Stacking rules (stack vs. refresh vs. replace)
- Status effect application and removal
- Immunity and resistance
- Integration with combat turn sequence

**Dependencies**:
- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-002 (Damage Calculation) - Not yet drafted (for DoT mechanics)

**Why Needed**: Status effects are mentioned throughout combat docs but lack comprehensive design specification.

**Implementation Exists**: Yes (partial, in `RuneAndRust.Engine/Services/StatusEffectService.cs`)

---

### 📋 SPEC-COMBAT-004: Accuracy & Evasion System
**Status**: Planned
**Priority**: Medium
**Domain**: Combat
**Layer 1 Docs**: 🔗 `docs/01-systems/accuracy-evasion.md`

**Proposed Scope**:
- Accuracy roll mechanics (opposed FINESSE checks)
- Evasion calculation and modifiers
- Cover and environmental factors
- Status effect impacts on accuracy/evasion
- Critical hit/miss thresholds

**Dependencies**:
- SPEC-COMBAT-001 (Combat Resolution) - Completed
- SPEC-COMBAT-003 (Status Effects) - Not yet drafted

**Why Needed**: Layer 1 docs exist but design rationale and balance targets not formally specified.

**Implementation Exists**: Yes (in `RuneAndRust.Engine/Services/CombatEngine.cs`)

---

## PROGRESSION Domain

### ✅ SPEC-PROGRESSION-001: Character Progression System
**Status**: Completed
**File**: `progression/character-progression-spec.md`
**Lines**: ~650
**Completed**: 2025-11-19
**Layer 1 Docs**: 🔗 `docs/01-systems/legend-leveling.md`

**Scope**: Legend/XP economy, Milestone system, Progression Points (PP), attribute increases

**Why Complete**: Core progression loop is foundational to player advancement and reward systems.

---

### 📋 SPEC-PROGRESSION-002: Archetype & Specialization System
**Status**: Planned
**Priority**: High
**Domain**: Progression
**Layer 1 Docs**: 🔗 None (implementation exists but no comprehensive Layer 1 doc)

**Proposed Scope**:
- Archetype selection (Mystic, Warden, Blade, Rune-Caster, Shadowmancer, Naturalist)
- Specialization mechanics and restrictions
- Starting attribute distributions
- Signature abilities per archetype
- Multi-classing rules (if any)
- Integration with ability trees

**Dependencies**:
- SPEC-PROGRESSION-001 (Character Progression) - Completed
- SPEC-PROGRESSION-003 (Ability System) - Not yet drafted

**Why Needed**: Archetypes are implemented but design philosophy, balance targets, and player experience goals not formally documented.

**Implementation Exists**: Yes (in `RuneAndRust.Core/Models/Specialization.cs` and related)

---

### 📋 SPEC-PROGRESSION-003: Ability & Skill Advancement System
**Status**: Planned
**Priority**: High
**Domain**: Progression
**Layer 1 Docs**: 🔗 None (partial docs in `docs/02-statistical-registry/abilities/`)

**Proposed Scope**:
- Ability unlock mechanics
- Ability rank progression (Rank 1 → Rank 2 → Rank 3)
- PP costs for ability upgrades
- Skill tree structure and branching
- Prerequisites and dependencies
- Ability reset/respec mechanics (if any)

**Dependencies**:
- SPEC-PROGRESSION-001 (Character Progression) - Completed
- SPEC-PROGRESSION-002 (Archetype System) - Not yet drafted

**Why Needed**: Ability progression is referenced in PP economy but not comprehensively specified.

**Implementation Exists**: Yes (partial, in ability-related services)

---

## ECONOMY Domain

### ✅ SPEC-ECONOMY-003: Trauma Economy System
**Status**: Completed
**File**: `economy/trauma-economy-spec.md`
**Lines**: ~650
**Completed**: 2025-11-19
**Layer 1 Docs**: 🔗 `docs/01-systems/stress-corruption.md`, `docs/01-systems/traumas.md`

**Scope**: Psychic Stress accumulation, Corruption mechanics, Breaking Points, Trauma acquisition

**Why Complete**: Trauma economy is unique to Rune & Rust and core to the horror/psychological theme.

---

### 📋 SPEC-ECONOMY-001: Loot & Equipment System
**Status**: Planned
**Priority**: High
**Domain**: Economy
**Layer 1 Docs**: 🔗 None (equipment stats exist in `docs/02-statistical-registry/equipment/`)

**Proposed Scope**:
- Equipment slots and restrictions
- Quality tiers (Mundane, Fine, Masterwork, Runic, Artifact)
- Loot drop tables and rarity
- Enemy-based loot generation
- Equipment stat generation (ranges, variance)
- Unique/legendary item rules
- Integration with crafting system

**Dependencies**:
- SPEC-COMBAT-002 (Damage Calculation) - Not yet drafted (for weapon stats)
- SPEC-ECONOMY-002 (Crafting System) - Not yet drafted

**Why Needed**: Equipment is implemented but design philosophy for loot economy, rarity balance, and progression pacing not specified.

**Implementation Exists**: Yes (in `RuneAndRust.Core/Models/Equipment/` and `RuneAndRust.Engine/Services/LootService.cs`)

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

### 📋 SPEC-NARRATIVE-002: Dialogue & NPC Interaction System
**Status**: Planned
**Priority**: Low
**Domain**: Narrative
**Layer 1 Docs**: 🔗 None

**Proposed Scope**:
- Dialogue tree structure
- Branching conversation mechanics
- Skill check integration (WILL, FINESSE, CORPUS checks in dialogue)
- NPC personality and response variation
- Consequence tracking (reputation, quest triggers)
- Setting compliance (Layer 2 Diagnostic Voice for NPCs)

**Dependencies**:
- SPEC-NARRATIVE-004 (Faction System) - Not yet drafted
- SETTING_COMPLIANCE.md - Voice & Tone standards

**Why Needed**: Dialogue system is aspirational feature for future development.

**Implementation Exists**: No

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

7. **SPEC-COMBAT-004: Accuracy & Evasion System** - Existing implementation, medium complexity
8. **SPEC-ECONOMY-002: Crafting & Resource System** - Aspirational, adds depth
9. **SPEC-WORLD-002: Biome & Environment System** - Enhances procedural generation
10. **SPEC-WORLD-003: Environmental Hazard System** - Partial implementation exists
11. **SPEC-NARRATIVE-001: Descriptor Framework System** - Existing implementation needs design spec

### Low Priority (Future Features)
These specs are for systems not yet implemented, lower impact on core gameplay:

12. **SPEC-NARRATIVE-002: Dialogue & NPC Interaction System** - No implementation yet
13. **SPEC-NARRATIVE-003: Quest & Objective System** - No implementation yet
14. **SPEC-NARRATIVE-004: Faction & Reputation System** - No implementation yet

---

## Recommended Drafting Order

Based on dependencies and priority, recommended order for drafting specifications:

### Phase 1: Core Combat & Progression (4 specs)
1. **SPEC-COMBAT-002: Damage Calculation** - No dependencies, foundation for other combat specs
2. **SPEC-COMBAT-003: Status Effects** - Depends on SPEC-COMBAT-002
3. **SPEC-PROGRESSION-002: Archetype & Specialization** - Minimal dependencies
4. **SPEC-PROGRESSION-003: Ability & Skill Advancement** - Depends on SPEC-PROGRESSION-002

### Phase 2: Economy & World (3 specs)
5. **SPEC-ECONOMY-001: Loot & Equipment** - Depends on SPEC-COMBAT-002 (for weapon stats)
6. **SPEC-WORLD-001: Procedural Generation** - Minimal dependencies, foundational
7. **SPEC-WORLD-003: Environmental Hazard** - Depends on SPEC-WORLD-001

### Phase 3: Polish & Enhancement (4 specs)
8. **SPEC-COMBAT-004: Accuracy & Evasion** - Depends on SPEC-COMBAT-003
9. **SPEC-WORLD-002: Biome & Environment** - Depends on SPEC-WORLD-001
10. **SPEC-NARRATIVE-001: Descriptor Framework** - Minimal dependencies
11. **SPEC-ECONOMY-002: Crafting & Resource** - Depends on SPEC-ECONOMY-001

### Phase 4: Future Systems (3 specs)
12. **SPEC-NARRATIVE-004: Faction & Reputation** - Foundational for other narrative specs
13. **SPEC-NARRATIVE-002: Dialogue & NPC Interaction** - Depends on SPEC-NARRATIVE-004
14. **SPEC-NARRATIVE-003: Quest & Objective** - Depends on SPEC-NARRATIVE-004, SPEC-WORLD-001

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
4. Follow [START_HERE.md](./START_HERE.md) 5-step workflow

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

- **[START_HERE.md](./START_HERE.md)** - Quick start guide for drafting specifications
- **[TEMPLATE.md](./TEMPLATE.md)** - Master specification template
- **[README.md](./README.md)** - Complete specification writing guide
- **[SETTING_COMPLIANCE.md](./SETTING_COMPLIANCE.md)** - Aethelgard setting validation

---

**Last Updated**: 2025-11-19
**Maintained By**: Specification governance framework
**Update Frequency**: Update this document whenever specifications are completed or new systems are identified
