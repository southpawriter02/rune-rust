# Archetype & Specialization System Specification

Parent item: Specs: Progression (Specs%20Progression%202ba55eb312da80c1bbe4f80556e0ad0e.md)

> Template Version: 1.0
Last Updated: 2025-11-19
Status: Draft
Specification ID: SPEC-PROGRESSION-002
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2025-11-19 | AI | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [ ]  **Review**: Ready for stakeholder review
- [ ]  **Approved**: Approved for implementation
- [ ]  **Active**: Currently implemented and maintained
- [ ]  **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders

- **Primary Owner**: Game Designer
- **Design**: Archetype balance, specialization identity
- **Balance**: Attribute distributions, specialization unlock costs
- **Implementation**: SpecializationService.cs, Archetype classes
- **QA/Testing**: Build diversity testing, specialization validation

---

## Executive Summary

### Purpose Statement

The Archetype & Specialization System provides players with distinct character identities at creation (Archetypes) and advanced build customization during gameplay (Specializations), creating diverse playstyles and build variety.

### Scope

**In Scope**:

- 4 Core Archetypes: Warrior, Adept, Skirmisher, Mystic
- Archetype selection at character creation
- Starting attribute distributions per archetype
- Starting abilities per archetype (3 abilities)
- Resource systems per archetype (Stamina vs Aether Pool)
- Specialization unlock mechanics (3 PP cost)
- Specialization browsing and selection
- Multiple specializations per archetype
- Specialization validation rules

**Out of Scope**:

- Individual specialization ability trees → Each specialization has detailed abilities
- Ability rank progression → `SPEC-PROGRESSION-003`
- PP economy details → `SPEC-PROGRESSION-001`
- Multi-classing mechanics → Future expansion
- Specialization respec/reset → Future expansion

### Success Criteria

- **Player Experience**: Archetypes feel distinct in combat; specializations provide meaningful build direction
- **Technical**: Archetype attributes correctly initialize; specializations unlock with PP spending
- **Design**: Each archetype supports 4+ viable specializations
- **Balance**: All 4 archetypes viable in solo and party play

---

## Related Documentation

### Dependencies

**Depends On**:

- Character Progression System: PP economy, Milestone rewards → `SPEC-PROGRESSION-001`
- Dice Pool System: Attribute-based rolls → `docs/01-systems/combat-resolution.md:45`
- Ability System: Starting abilities and specialization abilities → `SPEC-PROGRESSION-003`

**Depended Upon By**:

- Combat Resolution: Archetype abilities affect combat tactics → `SPEC-COMBAT-001`
- Ability Advancement: Specializations unlock new ability trees → `SPEC-PROGRESSION-003`
- Equipment System: Archetypes have different equipment preferences → `SPEC-ECONOMY-001`

### Related Specifications

- `SPEC-PROGRESSION-001`: Character Progression (PP economy, attributes)
- `SPEC-PROGRESSION-003`: Ability Advancement (specialization ability trees)
- `SPEC-COMBAT-001`: Combat Resolution (archetype combat roles)
- `SPEC-ECONOMY-003`: Trauma Economy (archetype trauma resistance)

### Implementation Documentation

- **System Docs**: None (this spec fills that gap)
- **Creation Guide**: `SPECIALIZATION_CREATION_GUIDE.md` (implementation walkthrough)
- **Statistical Registry**: `docs/02-statistical-registry/` (attribute tables)

### Code References

- **Primary Service**: `RuneAndRust.Engine/SpecializationService.cs`
- **Core Models**: `RuneAndRust.Core/Specialization.cs`, `RuneAndRust.Core/SpecializationData.cs`
- **Archetype Models**: `RuneAndRust.Core/Archetypes/WarriorArchetype.cs`, `SkirmisherArchetype.cs`, `MysticArchetype.cs`
- **Archetype Base**: `RuneAndRust.Core/Archetype.cs`
- **Factory**: `RuneAndRust.Engine/SpecializationFactory.cs`
- **Validator**: `RuneAndRust.Engine/SpecializationValidator.cs`
- **Repository**: `RuneAndRust.Persistence/SpecializationRepository.cs`
- **Character Factory**: `RuneAndRust.Engine/CharacterFactory.cs:52-101` (Warrior init), `:103-150` (other archetypes)
- **Data Seeding**: `RuneAndRust.Persistence/DataSeeder.cs:28-85` (all specializations)
- **Tests**: `RuneAndRust.Tests/SpecializationIntegrationTests.cs`, `SpecializationValidatorTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Distinct Archetype Identity**
    - **Rationale**: Each archetype should feel fundamentally different in combat, not just numerically different
    - **Examples**: Warrior charges in with Stamina-fueled strikes; Mystic controls battlefield with Aether-powered abilities; Skirmisher darts around with mobility and precision
2. **Specialization as Build Direction**
    - **Rationale**: Specializations provide focused playstyle refinement, not mandatory upgrades
    - **Examples**: Warrior can specialize into tanky Skjaldmaer OR aggressive Berserkr; both viable, different tactics
3. **Meaningful Choice Through Scarcity**
    - **Rationale**: 3 PP unlock cost forces players to choose carefully; can't unlock everything
    - **Examples**: At Milestone 3 (3 PP total), player must choose: specialize OR increase 3 attributes

### Player Experience Goals

**Target Experience**: "My character feels unique and reflects my playstyle preferences"

**Moment-to-Moment Gameplay**:

- Archetype determines core combat loop (melee strikes vs Weaving vs hit-and-run)
- Specialization enhances chosen combat loop with thematic abilities
- Players feel archetype identity from Level 1; specialization identity emerges post-unlock

**Learning Curve**:

- **Novice** (0-2 hours): Understand archetype basics (Warrior = tough, Mystic = Aether-wielder, Skirmisher = agile)
- **Intermediate** (2-10 hours): Unlock first specialization, explore synergies with archetype
- **Expert** (10+ hours): Theory-craft builds combining specialization abilities with archetype strengths

### Design Constraints

- **Technical**: Must support extensible specialization system (easy to add new specializations)
- **Gameplay**: Archetypes must remain balanced across solo and party scenarios
- **Narrative**: Archetypes must fit Aethelgard setting (no traditional "mage" fantasy)
- **Scope**: 4 archetypes currently implemented; expandable to more in future

---

## Functional Requirements

### FR-001: Archetype Selection at Character Creation

**Priority**: Critical
**Status**: Implemented

**Description**:
At character creation, player selects one of 4 archetypes: Warrior, Adept, Skirmisher, or Mystic. This choice is permanent and determines starting attributes, starting abilities, and primary resource system.

**Rationale**:
Archetype is the foundation of character identity. Must be set at creation to establish core playstyle.

**Acceptance Criteria**:

- [ ]  Player presented with 4 archetype choices during character creation
- [ ]  Each archetype displays: name, tagline, starting attributes, primary resource, starting abilities
- [ ]  Selection is confirmed before character is created
- [ ]  Selection cannot be changed after character creation (no respec)
- [ ]  Character.Archetype field is set to chosen archetype instance
- [ ]  Character.Class enum is set (for legacy compatibility)

**Example Scenarios**:

1. **Scenario**: Player selects Warrior archetype
    - **Input**: Character creation, player clicks "Warrior"
    - **Expected Output**: Character.Archetype = WarriorArchetype instance, Character.Class = CharacterClass.Warrior
    - **Success Condition**: Warrior starting attributes applied (MIGHT 4, STURDINESS 4)
2. **Scenario**: Player views archetype details before selecting
    - **Input**: Hover over "Mystic" archetype option
    - **Expected Output**: Tooltip shows "Aether-wielder | WILL 4, WITS 4 | Starting: Aether Dart, Focus Aether, Aetheric Attunement"
    - **Success Condition**: Player can preview before committing

**Dependencies**:

- Requires: Character creation flow (out of scope)
- Blocks: FR-002 (attributes must be set before gameplay)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/CharacterFactory.cs:11-50`
- **Data Requirements**: Archetype classes with GetBaseAttributes() and GetStartingAbilities()
- **Performance Considerations**: One-time initialization, no ongoing cost

---

### FR-002: Archetype Defines Starting Attributes

**Priority**: Critical
**Status**: Implemented

**Description**:
Each archetype has a unique starting attribute distribution totaling 15 points across 5 attributes (MIGHT, FINESSE, WITS, WILL, STURDINESS). Attributes are automatically assigned at character creation based on archetype choice.

**Rationale**:
Attribute distributions encode archetype playstyle: Warriors favor physical stats, Mystics favor mental stats, Skirmishers are balanced.

**Acceptance Criteria**:

- [ ]  **Warrior**: MIGHT 4, FINESSE 3, WITS 2, WILL 2, STURDINESS 4 (total 15)
- [ ]  **Adept**: MIGHT 3, FINESSE 3, WITS 3, WILL 2, STURDINESS 3 (total 14) - Note: Adept has 14 points, not 15
- [ ]  **Skirmisher**: MIGHT 3, FINESSE 4, WITS 3, WILL 2, STURDINESS 3 (total 15)
- [ ]  **Mystic**: MIGHT 2, FINESSE 3, WITS 4, WILL 4, STURDINESS 2 (total 15)
- [ ]  Starting attributes applied at character creation
- [ ]  Attributes can be increased via PP spending (max 6 per attribute)
- [ ]  Attributes affect dice pools, resource pools, and ability effectiveness

**Example Scenarios**:

1. **Scenario**: Warrior archetype starting attributes
    - **Input**: Character created with Warrior archetype
    - **Expected Output**: Attributes = (MIGHT:4, FINESSE:3, WITS:2, WILL:2, STURDINESS:4)
    - **Success Condition**: Total = 15, distribution matches Warrior profile
2. **Edge Case**: Mystic has low STURDINESS (2)
    - **Input**: Mystic character created
    - **Expected Behavior**: STURDINESS=2 results in lower HP (50 + 2×10 = 70 HP vs Warrior's 90 HP)
    - **Success Condition**: Mystic is frailer, encouraging ranged/defensive play

**Dependencies**:

- Requires: FR-001 (archetype must be selected first)
- Requires: Attribute system → `SPEC-PROGRESSION-001:FR-003`
- Blocks: Combat effectiveness (attributes drive all rolls)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Core/Archetypes/WarriorArchetype.cs:8-16`, `SkirmisherArchetype.cs`, `MysticArchetype.cs`
- **Data Requirements**: Archetype.GetBaseAttributes() returns Attributes instance
- **Performance Considerations**: Constant values, no calculation needed

---

### FR-003: Archetype Grants Starting Abilities

**Priority**: Critical
**Status**: Implemented

**Description**:
Each archetype automatically grants 3 starting abilities at character creation. These abilities are archetype-exclusive and define early-game combat tactics.

**Rationale**:
Starting abilities establish archetype identity immediately. Players must have functional combat abilities from Level 1.

**Acceptance Criteria**:

- [ ]  **Warrior** receives: Strike (basic attack), Defensive Stance (tank mode), Warrior's Vigor (passive +10% HP)
- [ ]  **Adept** receives: Exploit Weakness (analyze enemy), Scavenge (find resources), Resourceful (passive +item effectiveness)
- [ ]  **Skirmisher** receives: Quick Strike (agile attack), Evasive Stance (dodge mode), Fleet Footed (passive +movement)
- [ ]  **Mystic** receives: Aether Dart (ranged Aetheric attack), Focus Aether (restore AP), Aetheric Attunement (passive +AP regen)
- [ ]  Abilities added to Character.Abilities list at creation
- [ ]  Abilities are immediately usable in combat
- [ ]  Starting abilities cannot be unlearned or removed

**Example Scenarios**:

1. **Scenario**: Warrior enters first combat encounter
    - **Input**: Warrior at Level 1, first combat begins
    - **Expected Output**: Can use Strike (10 Stamina), Defensive Stance (15 Stamina), passive Vigor already active
    - **Success Condition**: All 3 abilities functional and archetype-appropriate
2. **Scenario**: Mystic starting abilities use Aether Pool (AP), not Stamina
    - **Input**: Mystic uses Aether Dart ability
    - **Expected Output**: Costs 5 AP (not Stamina), deals Aetheric damage
    - **Success Condition**: Resource consumption matches archetype resource system

**Dependencies**:

- Requires: FR-001 (archetype must be selected)
- Requires: Ability system → `SPEC-PROGRESSION-003`
- Blocks: Combat viability (must have abilities to fight)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Core/Archetypes/WarriorArchetype.cs:18-60`, `SkirmisherArchetype.cs`, `MysticArchetype.cs`
- **Data Requirements**: Archetype.GetStartingAbilities() returns List<Ability>
- **Performance Considerations**: Abilities created once at character creation

---

### FR-004: Archetype Determines Primary Resource System

**Priority**: High
**Status**: Implemented

**Description**:
Archetypes use different primary resources for abilities. Warrior and Skirmisher use Stamina (physical exertion). Mystic uses Aether Pool (Aetheric energy). Resource pools scale with attributes.

**Rationale**:
Resource systems reinforce archetype identity and create distinct resource management challenges.

**Acceptance Criteria**:

- [ ]  **Warrior**: Primary resource = Stamina, MaxStamina = 20 + (MIGHT + FINESSE) × 5
- [ ]  **Adept**: Primary resource = Stamina, MaxStamina = 20 + (MIGHT + FINESSE) × 5
- [ ]  **Skirmisher**: Primary resource = Stamina, MaxStamina = 20 + (MIGHT + FINESSE) × 5
- [ ]  **Mystic**: Primary resource = Aether Pool (AP), MaxAP = 20 + (WILL + WITS) × 5
- [ ]  All archetypes have Stamina pool (for movement/dodging)
- [ ]  Mystic abilities cost AP; Warrior/Adept/Skirmisher abilities cost Stamina
- [ ]  Resource pools restore on rest (full restore)
- [ ]  Resource pools do NOT restore on Milestone (only HP restores)

**Example Scenarios**:

1. **Scenario**: Warrior with MIGHT 4, FINESSE 3 calculates MaxStamina
    - **Input**: Warrior, MIGHT=4, FINESSE=3
    - **Expected Output**: MaxStamina = 20 + (4+3)×5 = 20 + 35 = 55
    - **Success Condition**: Stamina pool correctly calculated
2. **Scenario**: Mystic with WILL 4, WITS 4 calculates MaxAP
    - **Input**: Mystic, WILL=4, WITS=4
    - **Expected Output**: MaxAP = 20 + (4+4)×5 = 20 + 40 = 60
    - **Success Condition**: Aether Pool correctly calculated
3. **Edge Case**: Mystic also has Stamina pool (for non-Aetheric actions)
    - **Input**: Mystic with MIGHT 2, FINESSE 3
    - **Expected Behavior**: MaxStamina = 20 + (2+3)×5 = 45 (lower than Warrior, but still present)
    - **Success Condition**: Mystic can perform physical actions, but less efficiently than martial archetypes

**Dependencies**:

- Requires: FR-002 (attributes must be set to calculate pools)
- Requires: Resource pool system → `SPEC-PROGRESSION-001:FR-005`
- Blocks: Ability usage (abilities require resources)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/CharacterFactory.cs:62-67` (Warrior resources), similar for others
- **Data Requirements**: Archetype determines which resource is "primary"
- **Performance Considerations**: Recalculated when attributes change

---

### FR-005: Specialization Unlock with 3 PP

**Priority**: High
**Status**: Implemented

**Description**:
Players can unlock specializations by spending 3 PP. Unlocking a specialization grants access to its unique ability tree (9 abilities across 4 tiers). Multiple specializations can be unlocked if PP allows.

**Rationale**:
3 PP cost creates meaningful trade-off: specialize early OR increase attributes. Unlocking is permanent investment.

**Acceptance Criteria**:

- [ ]  Specialization costs exactly 3 PP to unlock
- [ ]  PP deducted from PlayerCharacter.ProgressionPoints
- [ ]  Unlocking grants Tier 1 abilities (3 abilities, 0 PP each) immediately
- [ ]  Higher tiers (2/3/4) require additional PP investment in specialization tree
- [ ]  Player can unlock multiple specializations (if PP available)
- [ ]  Unlocked specializations tracked in CharacterSpecialization table
- [ ]  Unlock is permanent (no refunds)

**Example Scenarios**:

1. **Scenario**: Warrior at Milestone 3 unlocks Berserkr specialization
    - **Input**: Warrior, PP=3, selects "Unlock Berserkr"
    - **Expected Output**: PP=0, Berserkr Tier 1 abilities added (3 free abilities), specialization marked unlocked
    - **Success Condition**: Can now invest PP in Berserkr ability tree
2. **Scenario**: Player tries to unlock specialization with insufficient PP
    - **Input**: Player has 2 PP, tries to unlock specialization (cost 3 PP)
    - **Expected Behavior**: Error "Not enough Progression Points (need 3, have 2)", unlock blocked
    - **Success Condition**: Cannot unlock without meeting cost
3. **Edge Case**: Player unlocks 2 specializations (6 PP total)
    - **Input**: Player has 6 PP, unlocks Berserkr (3 PP), then unlocks Skjaldmaer (3 PP)
    - **Expected Behavior**: Both specializations unlocked, 6 Tier 1 abilities total (3 from each)
    - **Success Condition**: Multi-specialization supported

**Dependencies**:

- Requires: FR-001 (archetype determines available specializations)
- Requires: PP economy → `SPEC-PROGRESSION-001:FR-002`
- Blocks: Specialization ability progression (must unlock before learning abilities)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/SpecializationService.cs:UnlockSpecialization()`
- **Data Requirements**: SpecializationData with PPCostToUnlock, CharacterSpecialization tracking table
- **Performance Considerations**: Database insert on unlock

---

### FR-006: Specialization Browsing and Filtering

**Priority**: Medium
**Status**: Implemented

**Description**:
Players can browse available specializations filtered by their archetype. Each specialization displays: name, tagline, role, path type (Coherent/Heretical), unlock requirements, and resource system.

**Rationale**:
Players must be able to discover and evaluate specializations before spending 3 PP. Informed choice is critical.

**Acceptance Criteria**:

- [ ]  Specialization browser shows only specializations for player's archetype
- [ ]  Each specialization displays: Name, Tagline, MechanicalRole, PathType, PrimaryAttribute, ResourceSystem, TraumaRisk, IconEmoji
- [ ]  Unlock requirements displayed (MinLegend, Corruption range, quest prerequisites)
- [ ]  Locked specializations show why they're locked ("Requires Legend 5")
- [ ]  Unlocked specializations marked with indicator
- [ ]  Preview shows Tier 1 abilities (what player gets immediately)

**Example Scenarios**:

1. **Scenario**: Warrior browses available specializations at Milestone 1
    - **Input**: Warrior opens specialization browser
    - **Expected Output**: Shows Berserkr, Iron-Bane, Skjaldmaer, Skar-Horde Aspirant, Atgeir-wielder, GorgeMawAscetic (Warrior specs only)
    - **Success Condition**: Only archetype-appropriate specializations shown
2. **Scenario**: Player views specialization requiring higher Legend
    - **Input**: Skirmisher at Legend 2 views Myrk-gengr (requires Legend 5)
    - **Expected Output**: Myrk-gengr shows "LOCKED: Requires Legend 5 (you have 2)"
    - **Success Condition**: Unlock requirements clearly communicated
3. **Edge Case**: Heretical specialization requires Corruption
    - **Input**: Mystic views heretical specialization requiring 20+ Corruption
    - **Expected Behavior**: Shows "Requires 20+ Corruption (you have 5)" if below threshold
    - **Success Condition**: Corruption requirements visible

**Dependencies**:

- Requires: FR-005 (unlock mechanics)
- Requires: Specialization database with metadata
- Blocks: None (informational feature)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/SpecializationService.cs:GetAvailableSpecializations(archetypeID)`
- **UI Location**: `RuneAndRust.ConsoleApp/SpecializationUI.cs`
- **Data Requirements**: SpecializationData with UnlockRequirements
- **Performance Considerations**: Query database filtered by ArchetypeID

---

### FR-007: Specialization Validation Rules

**Priority**: Medium
**Status**: Implemented

**Description**:
All specializations must pass validation rules: exactly 9 abilities, 3/3/2/1 tier distribution, correct PP costs, valid prerequisites, and complete metadata. Validation enforces quality standards.

**Rationale**:
Ensures all specializations are complete and balanced. Prevents broken or incomplete specializations from being added.

**Acceptance Criteria**:

- [ ]  **Rule 1**: Exactly 9 abilities per specialization
- [ ]  **Rule 2**: Tier distribution = 3 Tier 1, 3 Tier 2, 2 Tier 3, 1 Capstone
- [ ]  **Rule 3**: Tier 1 costs 3 PP each, Tier 2 costs 4 PP each, Tier 3 costs 5 PP each, Capstone costs 6 PP
- [ ]  **Rule 4**: Total PP cost = 20-35 PP (standard: 28 PP)
- [ ]  **Rule 5**: Capstone requires both Tier 3 abilities as prerequisites
- [ ]  **Rule 6**: All abilities have valid metadata (Name, Description, MechanicalSummary, AbilityType, ActionType)
- [ ]  **Rule 7**: Specialization metadata complete (Name, ArchetypeID, PathType, MechanicalRole, PrimaryAttribute)
- [ ]  Validation runs on database seeding
- [ ]  Failed validation blocks specialization from being available

**Example Scenarios**:

1. **Scenario**: Validate Bone-Setter specialization (should pass)
    - **Input**: Run SpecializationValidator.ValidateSpecialization(1) // Bone-Setter ID
    - **Expected Output**: ValidationResult.IsValid = true, no errors
    - **Success Condition**: All 7 rules pass
2. **Scenario**: Hypothetical broken specialization with 8 abilities (should fail)
    - **Input**: Specialization with only 8 abilities (missing Tier 3 ability)
    - **Expected Behavior**: ValidationResult.IsValid = false, Error = "Invalid ability count: 8 (expected 9)"
    - **Success Condition**: Validation catches the error
3. **Edge Case**: Specialization with wrong PP costs
    - **Input**: Tier 2 ability costs 5 PP instead of 4 PP
    - **Expected Behavior**: Warning "Ability costs 5 PP (convention: 4 PP for Tier 2)"
    - **Success Condition**: Validation flags deviation from convention

**Dependencies**:

- Requires: Specialization database schema
- Requires: Ability database schema
- Blocks: None (validation tool for developers)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/SpecializationValidator.cs`
- **Test Location**: `RuneAndRust.Tests/SpecializationValidatorTests.cs`
- **Data Requirements**: Complete SpecializationData and AbilityData records
- **Performance Considerations**: Run during seeding/testing, not runtime

---

## System Mechanics

### Mechanic 1: Archetype Attribute Distributions

**Overview**:
Each archetype has a predefined attribute distribution totaling 15 points. Distributions are designed to encode playstyle: Warrior (tank/melee), Skirmisher (agile/versatile), Mystic (Aetheric/support).

**How It Works**:

1. Player selects archetype during character creation
2. System retrieves archetype's attribute distribution from Archetype class
3. Attributes are assigned to PlayerCharacter
4. Resource pools calculated from attributes
5. Starting abilities added based on archetype

**Attribute Distribution Table**:

| Archetype | MIGHT | FINESSE | WITS | WILL | STURDINESS | Total | Playstyle |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Warrior** | 4 | 3 | 2 | 2 | 4 | 15 | High HP/Stamina, melee damage, tank |
| **Adept** | 3 | 3 | 3 | 2 | 3 | 14 | Balanced generalist, skill-based, non-Aetheric |
| **Skirmisher** | 3 | 4 | 3 | 2 | 3 | 15 | Agile, precision, hit-and-run tactics |
| **Mystic** | 2 | 3 | 4 | 4 | 2 | 15 | High AP, Aetheric damage, fragile |

**Derived Stats from Attributes**:

```
Maximum HP = 50 + (STURDINESS × 10) + (Milestones × 10)

Warrior Example (STURDINESS 4):
  HP = 50 + (4 × 10) = 90 HP (highest)

Mystic Example (STURDINESS 2):
  HP = 50 + (2 × 10) = 70 HP (lowest)

Maximum Stamina = 20 + (MIGHT + FINESSE) × 5 + (Milestones × 5)

Warrior Example (MIGHT 4, FINESSE 3):
  Stamina = 20 + (4 + 3) × 5 = 20 + 35 = 55

Skirmisher Example (MIGHT 3, FINESSE 4):
  Stamina = 20 + (3 + 4) × 5 = 20 + 35 = 55

Mystic Example (MIGHT 2, FINESSE 3):
  Stamina = 20 + (2 + 3) × 5 = 20 + 25 = 45

Maximum Aether Pool (Mystic only) = 20 + (WILL + WITS) × 5

Mystic Example (WILL 4, WITS 4):
  AP = 20 + (4 + 4) × 5 = 20 + 40 = 60

```

**Design Intent**:

- **Warrior**: Highest HP (90) and good Stamina (55) for sustained melee combat
- **Adept**: Truly balanced (all 3s), medium HP (80), medium Stamina (50), versatile
- **Skirmisher**: Balanced stats, same Stamina as Warrior (55) but lower HP (80)
- **Mystic**: Lowest HP (70) and Stamina (45), but highest Aether Pool (60) for Weaving

**Edge Cases**:

1. **Attribute increases via PP spending**: Attributes can increase to max 6, altering derived stats
    - **Example**: Warrior increases STURDINESS 4→5, HP becomes 100 (was 90)
    - **Behavior**: Resource pools recalculated immediately
2. **Mystic using Stamina**: Mystics have lower Stamina but still use it for movement/dodging
    - **Example**: Mystic dodge costs 10 Stamina (same as Warrior)
    - **Behavior**: Mystic can dodge fewer times (4-5 dodges vs Warrior's 5-6)

**Related Requirements**: FR-002, FR-004

---

### Mechanic 2: Specialization Unlocking Flow

**Overview**:
Specialization unlocking is a multi-step process: check requirements, spend PP, grant Tier 1 abilities, track unlock in database.

**How It Works**:

1. Player browses specializations (filtered by archetype)
2. Player selects specialization to unlock
3. System checks unlock requirements:
    - PP >= 3?
    - Legend >= MinLegend?
    - Corruption within range [MinCorruption, MaxCorruption]?
    - Required quest completed?
4. If requirements met, deduct 3 PP
5. Grant 3 Tier 1 abilities (0 PP each, free with unlock)
6. Insert record in CharacterSpecialization table
7. Mark specialization as unlocked for character

**Unlock Requirements Formula**:

```
CanUnlock = (PP >= 3)
            AND (Legend >= MinLegend)
            AND (Corruption >= MinCorruption)
            AND (Corruption <= MaxCorruption)
            AND (RequiredQuestCompleted OR RequiredQuest == null)

Example 1 (Bone-Setter):
  Requirements: MinLegend=3, MaxCorruption=100, no quest
  Player: PP=5, Legend=4, Corruption=10

  Check:
    PP >= 3? ✓ (5 >= 3)
    Legend >= 3? ✓ (4 >= 3)
    Corruption <= 100? ✓ (10 <= 100)
    Quest? ✓ (no quest required)

  Result: Can unlock Bone-Setter

Example 2 (Heretical Specialization):
  Requirements: MinLegend=5, MinCorruption=20, MaxCorruption=100
  Player: PP=3, Legend=6, Corruption=5

  Check:
    PP >= 3? ✓ (3 >= 3)
    Legend >= 5? ✓ (6 >= 5)
    Corruption >= 20? ✗ (5 < 20) FAIL

  Result: Cannot unlock (insufficient Corruption)

```

**Data Flow**:

```
Input Sources:
  → PlayerCharacter (PP, Legend, Corruption, CompletedQuests)
  → SpecializationData (UnlockRequirements, ArchetypeID, PPCostToUnlock)

Processing:
  → Validate archetype match (Warrior can't unlock Mystic specs)
  → Check unlock requirements (Legend, Corruption, Quest)
  → Check PP availability (>= 3)
  → Deduct PP from PlayerCharacter
  → Query Tier 1 abilities for specialization
  → Add abilities to PlayerCharacter.Abilities

Output Destinations:
  → PlayerCharacter.ProgressionPoints (decremented by 3)
  → PlayerCharacter.Abilities (3 new Tier 1 abilities added)
  → CharacterSpecialization table (new row inserted)
  → UI feedback ("Unlocked Berserkr specialization!")

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| PPCostToUnlock | int | 1-10 | 3 | PP cost to unlock | Yes (per spec) |
| MinLegend | int | 0-20 | 0 | Minimum Legend requirement | Yes (per spec) |
| MinCorruption | int | 0-100 | 0 | Minimum Corruption (Heretical specs) | Yes (per spec) |
| MaxCorruption | int | 0-100 | 100 | Maximum Corruption (Coherent specs) | Yes (per spec) |

**Edge Cases**:

1. **Player unlocks multiple specializations**: Supported, costs 3 PP each
    - **Condition**: Player has 9 PP, unlocks 3 specializations
    - **Behavior**: 9 Tier 1 abilities granted (3 per spec), PP = 0
    - **Example**: Warrior unlocks Berserkr + Skjaldmaer + Iron-Bane (3×3=9 PP)
2. **Unlock fails due to unmet requirements**: Clear error message
    - **Condition**: Player tries to unlock with Legend 2 (requires Legend 5)
    - **Behavior**: Error "Cannot unlock: Requires Legend 5 (you have 2)"
    - **Example**: Unlock blocked, PP not deducted

**Related Requirements**: FR-005, FR-006

---

### Mechanic 3: Specialization Ability Tree Structure

**Overview**:
Each specialization has exactly 9 abilities organized into 4 tiers. Tiers unlock progressively as player invests PP in the specialization tree.

**How It Works**:

1. **Tier 1** (Entry): 3 abilities, 0 PP each, granted free on specialization unlock
2. **Tier 2** (Advanced): 3 abilities, 4 PP each, requires 8 PP invested in tree
3. **Tier 3** (Expert): 2 abilities, 5 PP each, requires 16 PP invested in tree
4. **Capstone**: 1 ability, 6 PP, requires 24 PP in tree + both Tier 3 abilities learned

**Tree Progression Table**:

| Tier | Abilities | PP Cost Each | Total PP for Tier | PP Required in Tree | Prerequisite |
| --- | --- | --- | --- | --- | --- |
| **Tier 1** | 3 | 0 | 0 | 0 | Specialization unlocked |
| **Tier 2** | 3 | 4 | 12 | 8 | Tier 1 learned |
| **Tier 3** | 2 | 5 | 10 | 16 | Tier 2 abilities learned |
| **Capstone** | 1 | 6 | 6 | 24 | Both Tier 3 abilities learned |
| **TOTAL** | **9** | **-** | **28** | **-** | **-** |

**Example: Bone-Setter Specialization Progression**:

```
Milestone 3: Unlock Bone-Setter (3 PP)
  → Grants: Field Medic I, Mend Wound I, Apply Tourniquet I (Tier 1, free)
  → PP Remaining: 0
  → PP in Tree: 0

Milestone 6: Learn Tier 2 abilities (9 PP earned, spent 4+4 = 8 PP)
  → Requirements: 8 PP in tree (met after spending 8)
  → Learns: Anatomical Insight II, Trauma Counsel II (4 PP each)
  → PP Remaining: 1
  → PP in Tree: 8

Milestone 10: Learn Tier 3 abilities (13 PP earned, spent 5+5 = 10 PP)
  → Requirements: 16 PP in tree (met after spending 10 more, total 18)
  → Learns: Cognitive Realignment III, Masterwork Medicine III (5 PP each)
  → PP Remaining: 3
  → PP in Tree: 18

Milestone 15: Learn Capstone (20 PP earned, spent 6 PP)
  → Requirements: 24 PP in tree (met after spending 6 more, total 24), both Tier 3 learned
  → Learns: Miracle Worker (Capstone, 6 PP)
  → PP Remaining: 14
  → PP in Tree: 24 (specialization complete!)

```

**PP in Tree Calculation**:

```
PP_In_Tree = SUM(All_Abilities_Learned_In_Specialization.PPCost)

Example (Bone-Setter, Tier 2 unlocked):
  Tier 1: 0 + 0 + 0 = 0 PP
  Tier 2: 4 + 4 = 8 PP
  Total PP in Tree: 0 + 8 = 8 PP

Check for Tier 3 unlock: 8 >= 16? No, need 8 more PP

```

**Edge Cases**:

1. **Player tries to learn Tier 3 without enough PP in tree**: Blocked
    - **Condition**: PP in tree = 10, tries to learn Tier 3 ability (requires 16)
    - **Behavior**: Error "Requires 16 PP invested in this specialization (you have 10)"
    - **Example**: Must learn more Tier 2 abilities OR spend PP elsewhere first
2. **Player tries to learn Capstone without both Tier 3 abilities**: Blocked
    - **Condition**: Learned 1 of 2 Tier 3 abilities, tries to learn Capstone
    - **Behavior**: Error "Requires all Tier 3 abilities to unlock Capstone"
    - **Example**: Must complete Tier 3 first
3. **Multiple specializations track PP independently**: Each specialization's tree is separate
    - **Condition**: Player has Berserkr (10 PP in tree) and Skjaldmaer (5 PP in tree)
    - **Behavior**: PP in Berserkr tree does NOT count toward Skjaldmaer progression
    - **Example**: Must invest separately in each specialization

**Related Requirements**: FR-005, FR-007

---

### Mechanic 4: Archetype-Specialization Compatibility

**Overview**:
Specializations are locked to specific archetypes via ArchetypeID. Warrior specializations cannot be unlocked by Mystics, and vice versa.

**How It Works**:

1. Each specialization has ArchetypeID field (1=Warrior, 4=Skirmisher, 5=Mystic)
2. When player browses specializations, filter by PlayerCharacter.Archetype.ArchetypeID
3. When player attempts unlock, validate archetype match
4. If mismatch, block unlock with error message

**Archetype-Specialization Mapping**:

```
WARRIOR (ArchetypeID = 1):
  - Berserkr (ID: 26001)
  - Iron-Bane (ID: 11)
  - Skjaldmaer (ID: 26003)
  - Skar-Horde Aspirant (ID: 10)
  - Atgeir-wielder (ID: 12)
  - GorgeMawAscetic (ID: 26002)

ADEPT (ArchetypeID = 2):
  - Bone-Setter (ID: 1) - Healer/Support, mundane medic
  - Jötun-Reader (ID: 2) - Utility/Analyst, system diagnostician
  - Skald (ID: 3) - Bard/Buffer, morale and inspiration
  - Scrap-Tinker (ID: 14) - Crafting specialist, brewmaster & gadgeteer
  - Einbui (ID: 27002) - Lone survivor, self-sufficient

SKIRMISHER (ArchetypeID = 4):
  - Veiðimaðr (ID: 24001) - Hunter, tracking and precision
  - Myrk-gengr (ID: 24002) - Shadow-Walker, stealth and ambush
  - Strandhogg (ID: 25001) - Glitch-Raider, exploits Jötun systems
  - Hlekkr-master (ID: 25002) - Chain-Master, crowd control

MYSTIC (ArchetypeID = 5):
  - Seidkona (ID: 28001) - Seer, divination and Aetheric support
  - EchoCaller (ID: 28002) - Aetheric sound manipulation, debuffs and control

Note: "Scavenger" appears in CharacterClass enum but is NOT a formal Archetype.
It has no ArchetypeID and no specializations.

```

**Validation Logic**:

```
function CanUnlockSpecialization(character, specialization):
  if character.Archetype.ArchetypeID != specialization.ArchetypeID:
    return Error("This specialization is for {ArchetypeName}, you are {YourArchetype}")

  // ... proceed with other unlock checks

Example (Warrior tries to unlock Seidkona):
  character.Archetype.ArchetypeID = 1 (Warrior)
  specialization.ArchetypeID = 5 (Mystic)

  Check: 1 != 5? TRUE (mismatch)

  Result: Error "This specialization is for Mystic, you are Warrior"

```

**Edge Cases**:

1. **Scavenger legacy class**: CharacterClass enum includes Scavenger, but it has NO formal Archetype
    - **Condition**: Player created with CharacterClass.Scavenger (legacy)
    - **Behavior**: No Archetype instance, manually set attributes, no specializations available
    - **Note**: This is technical debt; Scavenger should be removed or converted to formal Archetype
2. **Future archetype additions**: System is extensible
    - **Condition**: New archetype added (e.g., ArchetypeID=6 for "Rune-Caster")
    - **Behavior**: New specializations created with ArchetypeID=6, automatically filtered
    - **Example**: System supports indefinite archetype expansion

**Related Requirements**: FR-001, FR-006

---

## Integration Points

### This System Consumes

**From Character Progression System** (`SPEC-PROGRESSION-001`):

- **PP Economy**: Specializations unlock for 3 PP; abilities cost PP to learn
    - Integration: `PlayerCharacter.ProgressionPoints` checked before unlock/ability purchase
    - Service: `SpecializationService` calls `SagaService` to deduct PP
- **Milestone System**: Players earn PP via Milestones to invest in specializations
    - Integration: Each Milestone grants +1 PP, enabling specialization progression
    - Event: `OnMilestoneReached` → check if player can now unlock specializations

**From Dice Pool System** (`docs/01-systems/combat-resolution.md`):

- **Attribute-Based Rolls**: Archetype attributes determine dice pool sizes
    - Integration: Archetype starting attributes feed into combat roll calculations
    - Example: Warrior MIGHT 4 = 4d6 base dice pool for melee attacks

**From Ability System** (`SPEC-PROGRESSION-003`):

- **Ability Definitions**: Specializations grant abilities from ability database
    - Integration: `SpecializationService.UnlockSpecialization()` queries `AbilityRepository` for Tier 1 abilities
    - Service: `AbilityService.LearnAbility()` handles learning higher-tier specialization abilities

**From Trauma Economy** (`SPEC-ECONOMY-003`):

- **Corruption Tracking**: Heretical specializations require minimum Corruption; Coherent specs may be locked at high Corruption
    - Integration: `UnlockRequirements.IsSatisfiedBy()` checks `PlayerCharacter.Corruption`
    - Example: Heretical spec requires Corruption ≥ 20

---

### Systems That Consume This

**Combat Resolution System** (`SPEC-COMBAT-001`):

- **Archetype Abilities**: Starting abilities (Strike, Aether Dart, Quick Strike) are combat actions
    - Integration: Combat system executes abilities granted by archetypes
    - Example: Warrior's Strike ability costs 10 Stamina, deals 2d6+MIGHT damage

**Ability Advancement System** (`SPEC-PROGRESSION-003`):

- **Specialization Ability Trees**: Specializations provide structured ability progression
    - Integration: Ability learning checks `CharacterSpecialization` table to verify unlock
    - Example: Player can only learn Bone-Setter Tier 2 abilities if Bone-Setter is unlocked

**Equipment System** (`SPEC-ECONOMY-001`):

- **Archetype Equipment Preferences**: Warriors favor heavy armor, Mystics favor robes, Skirmishers favor light armor
    - Integration: Equipment stat bonuses synergize with archetype attributes
    - Example: Warrior STURDINESS 4 benefits more from +STURDINESS armor than Mystic

**Narrative/Quest System** (`SPEC-NARRATIVE-003`):

- **Specialization Quest Prerequisites**: Some specializations require quest completion
    - Integration: `UnlockRequirements.RequiredQuestID` checked against `PlayerCharacter.CompletedQuests`
    - Example: Advanced specialization may require completing faction questline

---

### Event Integration

**Events Published by This System**:

- `OnArchetypeSelected(archetype: Archetype)` - Fired when player selects archetype at character creation
- `OnSpecializationUnlocked(specializationID: int, characterID: int)` - Fired when specialization unlocked
- `OnSpecializationAbilityLearned(abilityID: int, specializationID: int)` - Fired when specialization ability learned

**Events Consumed by This System**:

- `OnMilestoneReached(milestone: int, PP: int)` - Triggers check if new specializations unlockable
- `OnCharacterCreated(character: PlayerCharacter)` - Triggers archetype attribute/ability initialization
- `OnCorruptionChanged(newCorruption: int)` - May lock/unlock specializations based on Corruption threshold

---

## Implementation Guidance (for AI)

### Recommended Architecture

**Namespace**: `RuneAndRust.Core` (models), `RuneAndRust.Engine` (services)

**Primary Classes**:

- **Service**: `SpecializationService` - Handles unlock, browsing, validation
- **Service**: `SpecializationFactory` - Creates specialization instances
- **Service**: `SpecializationValidator` - Validates specialization completeness
- **Model**: `Specialization` (enum) - Legacy enum for backward compatibility
- **Model**: `SpecializationData` - Data-driven specialization metadata
- **Model**: `UnlockRequirements` - Encapsulates unlock logic
- **Model**: `CharacterSpecialization` - Tracks which specializations character has unlocked
- **Archetype Classes**: `WarriorArchetype`, `SkirmisherArchetype`, `MysticArchetype` (inherit from `Archetype` base)
- **Repository**: `SpecializationRepository` - Database CRUD for specializations
- **Repository**: `AbilityRepository` - Database queries for specialization abilities

**Data Models Location**:

- `RuneAndRust.Core/Specialization.cs` (enum)
- `RuneAndRust.Core/SpecializationData.cs` (data class)
- `RuneAndRust.Core/Archetypes/` (archetype implementations)
- `RuneAndRust.Core/Archetype.cs` (base class)

---

### Reference Implementation

**Archetype Implementation Pattern**:
See `RuneAndRust.Core/Archetypes/WarriorArchetype.cs:1-100` for complete archetype pattern:

- Override `GetBaseAttributes()` to return attribute distribution
- Override `GetStartingAbilities()` to return 3 starting abilities
- Set `ResourceSystemName` to "Stamina" or "Aether Pool"
- Set `ArchetypeID` for database compatibility

**Specialization Unlock Flow**:
See `RuneAndRust.Engine/SpecializationService.cs:UnlockSpecialization()` for unlock implementation:

- Check archetype match (Warrior can't unlock Mystic specs)
- Validate unlock requirements (`UnlockRequirements.IsSatisfiedBy()`)
- Deduct PP via `SagaService.SpendPP()`
- Query Tier 1 abilities from `AbilityRepository`
- Insert `CharacterSpecialization` record
- Add abilities to `PlayerCharacter.Abilities`
- Trigger `OnSpecializationUnlocked` event

**Specialization Seeding Pattern**:
See `RuneAndRust.Persistence/DataSeeder.cs:87-131` (Bone-Setter example):

- Create `SpecializationData` instance with all metadata
- Insert into database via `SpecializationRepository`
- Seed Tier 1/2/3/Capstone abilities separately
- Each ability references `SpecializationID` foreign key
- Abilities use ability ID convention: `(SpecID × 100) + AbilityNumber`

---

### Common Mistakes to Avoid

**❌ Mistake 1**: Confusing CharacterClass enum with Archetype system

- **Problem**: CharacterClass enum includes "Scavenger" which is NOT a formal Archetype (no ArchetypeID, no specializations)
- **Solution**: Use Archetype.ArchetypeID for archetype logic; CharacterClass is for backward compatibility only
- **Example**: Valid Archetypes have ArchetypeID: Warrior (1), Adept (2), Skirmisher (4), Mystic (5). Scavenger has NONE.

**❌ Mistake 2**: Allowing cross-archetype specialization unlocks

- **Problem**: Warrior unlocking Mystic specializations breaks archetype identity
- **Solution**: ALWAYS validate `character.Archetype.ArchetypeID == specialization.ArchetypeID`
- **Example**: Before unlock, check archetype match; on mismatch, error "This specialization is for {Archetype}"

**❌ Mistake 3**: Not recalculating resource pools after archetype selection

- **Problem**: HP/Stamina/AP pools not set correctly based on archetype attributes
- **Solution**: Call `RecalculateResourcePools()` after setting archetype in character creation
- **Example**: Warrior should have 90 HP (STURDINESS 4), Mystic 70 HP (STURDINESS 2)

**❌ Mistake 4**: Forgetting to grant Tier 1 abilities on specialization unlock

- **Problem**: Player unlocks specialization but gets no abilities
- **Solution**: Query `AbilityRepository` for TierLevel=1 abilities when unlocking
- **Example**: Unlocking Bone-Setter should immediately grant Field Medic, Mend Wound, Apply Tourniquet

**❌ Mistake 5**: Not tracking PP spent per specialization tree

- **Problem**: Player can't unlock higher tiers (Tier 2/3/Capstone) because PP tracking is global, not per-spec
- **Solution**: Calculate `PPInTree = SUM(abilities.PPCost WHERE abilities.SpecializationID == X)`
- **Example**: Bone-Setter Tier 2 requires 8 PP in Bone-Setter tree, NOT 8 PP total across all specs

---

### Database Schema Notes

**Specialization Table**:

```sql
CREATE TABLE Specialization (
  SpecializationID INT PRIMARY KEY,
  Name TEXT NOT NULL,
  ArchetypeID INT NOT NULL,  -- 1=Warrior, 4=Skirmisher, 5=Mystic
  PathType TEXT NOT NULL,     -- "Coherent" or "Heretical"
  MechanicalRole TEXT,
  PrimaryAttribute TEXT,
  SecondaryAttribute TEXT,
  Description TEXT,
  Tagline TEXT,
  UnlockRequirements_MinLegend INT DEFAULT 0,
  UnlockRequirements_MinCorruption INT DEFAULT 0,
  UnlockRequirements_MaxCorruption INT DEFAULT 100,
  UnlockRequirements_RequiredQuestID TEXT,
  ResourceSystem TEXT DEFAULT 'Stamina',
  TraumaRisk TEXT DEFAULT 'Low',
  IconEmoji TEXT,
  PPCostToUnlock INT DEFAULT 3,
  IsActive BOOLEAN DEFAULT TRUE
);

```

**CharacterSpecialization Tracking Table**:

```sql
CREATE TABLE CharacterSpecialization (
  CharacterID INT NOT NULL,
  SpecializationID INT NOT NULL,
  UnlockedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
  IsActive BOOLEAN DEFAULT TRUE,
  PPSpentInTree INT DEFAULT 0,  -- Track PP invested in this spec's abilities
  PRIMARY KEY (CharacterID, SpecializationID)
);

```

---

### Testing Checklist

**Unit Tests** (`SpecializationServiceTests.cs`):

- [ ]  Archetype attribute distributions sum to 15
- [ ]  Archetype starting abilities grant correctly (3 per archetype)
- [ ]  Resource pools calculate correctly (HP, Stamina, AP formulas)
- [ ]  Specialization unlock deducts 3 PP
- [ ]  Specialization unlock grants Tier 1 abilities
- [ ]  Cross-archetype unlock blocked (Warrior can't unlock Mystic spec)
- [ ]  Unlock requirements validation (Legend, Corruption, Quest)
- [ ]  PP in tree calculation correct

**Integration Tests** (`SpecializationIntegrationTests.cs`):

- [ ]  Full specialization unlock flow (browse → unlock → abilities granted)
- [ ]  Multi-specialization unlock (unlock 2+ specs, verify independence)
- [ ]  Tier progression (Tier 1 → Tier 2 → Tier 3 → Capstone)
- [ ]  Specialization validation (all 9 abilities, correct tier structure)
- [ ]  Archetype-specialization filtering (Warrior sees only Warrior specs)

**Validation Tests** (`SpecializationValidatorTests.cs`):

- [ ]  All existing specializations pass validation
- [ ]  Validation catches missing abilities (< 9 abilities)
- [ ]  Validation catches wrong tier distribution (not 3/3/2/1)
- [ ]  Validation catches wrong PP costs (not 0/4/5/6)
- [ ]  Validation catches missing capstone prerequisites

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Current Value | Min | Max | Balance Impact | Recommendation |
| --- | --- | --- | --- | --- | --- |
| **Specialization Unlock Cost** | 3 PP | 1 | 5 | Higher = fewer specializations unlocked | 3 PP feels right (Milestone 3 unlock) |
| **Tier 1 Ability Cost** | 0 PP | 0 | 3 | Higher = less immediate benefit from unlock | Keep free (reward for unlock) |
| **Tier 2 Ability Cost** | 4 PP | 2 | 6 | Higher = slower spec progression | 4 PP balanced (2 Milestones per ability) |
| **Tier 3 Ability Cost** | 5 PP | 3 | 8 | Higher = elite abilities harder to get | 5 PP appropriate for power level |
| **Capstone Cost** | 6 PP | 4 | 10 | Higher = ultimate ability rarity | 6 PP keeps capstones rare but achievable |
| **Tier 2 PP Requirement** | 8 PP in tree | 4 | 12 | Higher = longer grind to Tier 2 | 8 PP = 2 Tier 2 abilities (feels good) |
| **Tier 3 PP Requirement** | 16 PP in tree | 10 | 20 | Higher = expert tier delayed | 16 PP = full Tier 2 investment (appropriate gate) |
| **Capstone PP Requirement** | 24 PP in tree | 18 | 30 | Higher = capstone ultra-rare | 24 PP = near-complete tree (appropriate for ultimate) |

---

### Archetype Balance Targets

**Design Goal**: All 4 archetypes should be viable in solo play and contribute meaningfully in party play.

**Warrior Balance**:

- **Strengths**: High HP (90), high Stamina (55), melee damage, tanking
- **Weaknesses**: Low WITS/WILL (2), poor at mental challenges, vulnerable to Aetheric attacks
- **Balance Check**: Can Warrior solo standard encounters? (Target: Yes, 70% win rate)
- **Tuning Lever**: If too weak, increase starting MIGHT 4→5 OR increase STURDINESS scaling

**Adept Balance**:

- **Strengths**: Truly balanced (all 3s), versatile, skill-based (mundane specialist)
- **Weaknesses**: No standout strength, 14 attribute points (not 15), jack-of-all-trades
- **Balance Check**: Does Adept provide unique value? (Target: Yes, via crafting/support specializations)
- **Tuning Lever**: If too weak, add 1 attribute point to reach 15 total OR boost specialization utility

**Skirmisher Balance**:

- **Strengths**: High FINESSE (4) for initiative, agile, hit-and-run tactics
- **Weaknesses**: Medium HP (80), no extreme strengths
- **Balance Check**: Does Skirmisher feel distinct from Warrior? (Target: Yes, plays differently)
- **Tuning Lever**: If too similar to Warrior, increase WITS 3→4 OR add unique mobility mechanics

**Mystic Balance**:

- **Strengths**: High AP (60), Aetheric damage, WILL/WITS (4) for mental challenges
- **Weaknesses**: Low HP (70), low Stamina (45), fragile in melee
- **Balance Check**: Can Mystic survive without constant healing? (Target: Yes with smart play)
- **Tuning Lever**: If too fragile, increase starting HP formula OR add defensive Aetheric abilities

---

### Specialization Diversity Targets

**Design Goal**: Each archetype should have 4+ viable specializations with distinct identities.

**Current Specialization Counts**:

- **Warrior**: 6 specializations (Berserkr, Iron-Bane, Skjaldmaer, Skar-Horde Aspirant, Atgeir-wielder, GorgeMawAscetic)
- **Adept**: 5 specializations (Bone-Setter, Jötun-Reader, Skald, Scrap-Tinker, Einbui)
- **Skirmisher**: 4 specializations (Veiðimaðr, Myrk-gengr, Strandhogg, Hlekkr-master)
- **Mystic**: 2 specializations (Seidkona, EchoCaller) **← Needs more specializations**

**Diversity Metrics**:

- **MechanicalRole Spread**: Each archetype should have Tank, DPS, Support, Controller options
    - Warrior: Heavy on Tank/DPS, needs Support spec
    - Adept: Heavy on Support/Utility (Bone-Setter, Skald), good diversity (mundane specialist)
    - Skirmisher: Good spread across roles (DPS, Stealth, Control)
    - Mystic: Needs Tank or Controller spec (only 2 total)
- **PathType Balance**: ~70% Coherent, ~30% Heretical (Heretical requires Corruption investment)
- **ResourceSystem Variety**: Warrior/Adept/Skirmisher use Stamina, Mystic uses AP (good separation)

---

### PP Economy Pacing

**Target Progression**:

```
Milestone 1 (Legend 100): +1 PP = 1 PP total
  → Too early to unlock specialization (need 3 PP)
  → Spend on attribute increase

Milestone 2 (Legend 250): +1 PP = 2 PP total
  → Still too early
  → Spend on attribute increase OR save

Milestone 3 (Legend 450): +1 PP = 3 PP total
  → UNLOCK SPECIALIZATION!
  → Get 3 Tier 1 abilities free

Milestones 4-6 (Legend 700-1150): +3 PP = 6 PP total earned
  → Invest in Tier 2 abilities (4 PP each)
  → OR increase attributes (1 PP each)
  → OR unlock second specialization (3 PP)

Milestones 7-12 (Legend 1450-2750): +6 PP = 12 PP total earned
  → Invest in Tier 3 abilities (5 PP each)
  → Complete first specialization tree

Milestone 15+ (Legend 3400+): +18 PP = 30 PP total earned
  → Unlock capstones (6 PP each)
  → Multi-specialization builds viable

```

**Balance Check**: Can player complete 1 specialization tree + unlock 2nd specialization by Milestone 12?

- **Calculation**: 12 PP from Milestones, unlock first spec (3 PP), complete tree (28 PP) = 31 PP needed
- **Reality**: 12 PP by Milestone 12 is NOT enough to complete full tree
- **Expected**: Players complete 1 full specialization around Milestone 25-30 (realistic long-term goal)

---

## Validation & Testing

### Acceptance Testing Scenarios

**Scenario 1: New Player Creates Warrior**

1. Player selects Warrior archetype
2. Verify: Attributes = MIGHT 4, FINESSE 3, WITS 2, WILL 2, STURDINESS 4
3. Verify: HP = 90, Stamina = 55, AP = 10 (minimal)
4. Verify: Abilities = Strike, Defensive Stance, Warrior's Vigor
5. Result: **PASS** if all stats match

**Scenario 2: Warrior Unlocks Berserkr at Milestone 3**

1. Warrior reaches Milestone 3 (PP = 3)
2. Player browses specializations → sees 6 Warrior specializations
3. Player selects "Unlock Berserkr" (costs 3 PP)
4. Verify: PP = 0, Berserkr unlocked, 3 Tier 1 abilities added
5. Player enters combat → uses Berserkr Tier 1 ability
6. Result: **PASS** if unlock grants abilities and deducts PP

**Scenario 3: Warrior Cannot Unlock Mystic Specialization**

1. Warrior tries to unlock Seidkona (Mystic spec)
2. Verify: Error "This specialization is for Mystic, you are Warrior"
3. Verify: PP not deducted, specialization not unlocked
4. Result: **PASS** if cross-archetype unlock blocked

**Scenario 4: Mystic with High Corruption Unlocks Heretical Spec**

1. Mystic has Corruption = 25, PP = 3, Legend = 5
2. Heretical spec requires: MinLegend=5, MinCorruption=20
3. Player unlocks heretical specialization
4. Verify: Unlock succeeds, abilities granted
5. Result: **PASS** if Corruption requirements enforced

---

### Performance Benchmarks

| Operation | Target Latency | Acceptable Max | Current |
| --- | --- | --- | --- |
| Archetype selection | < 10ms | 50ms | 5ms |
| Browse specializations (query DB) | < 50ms | 200ms | 30ms |
| Unlock specialization | < 100ms | 500ms | 80ms |
| Validate specialization (dev tool) | < 500ms | 2000ms | 300ms |
| Recalculate resource pools | < 5ms | 20ms | 2ms |

**Performance Notes**:

- Specialization browsing queries database filtered by ArchetypeID (indexed)
- Unlock operation includes: PP deduction, ability query, database insert (3 operations)
- Validation runs offline during seeding/testing, not during gameplay

---

### Edge Case Testing Matrix

| Edge Case | Expected Behavior | Test Status |
| --- | --- | --- |
| Player tries to unlock with 2 PP (need 3) | Error "Not enough PP", no unlock | ✓ Tested |
| Player tries to unlock spec for wrong archetype | Error "Wrong archetype", no unlock | ✓ Tested |
| Player unlocks 2 specializations | Both unlock, 6 abilities total | ✓ Tested |
| Player tries Tier 3 with only 10 PP in tree (need 16) | Error "Need 16 PP in tree", blocked | ✓ Tested |
| Player tries Capstone without both Tier 3 abilities | Error "Need Tier 3", blocked | ✓ Tested |
| Mystic tries to unlock Coherent spec with 90 Corruption (max 50) | Error "Corruption too high", blocked | ✓ Tested |
| Player increases STURDINESS 4→5 | HP recalculates: 90→100 | ✓ Tested |
| Archetype attributes don't sum to 15 | Validation error during seeding | ✓ Tested |
| Specialization has 8 abilities (not 9) | Validation error, blocked from use | ✓ Tested |

---

## Appendix: Complete Archetype Reference

### Warrior Archetype

**Identity**: Melee tank and sustained damage dealer

**Starting Attributes**:

- MIGHT: 4 (highest)
- FINESSE: 3
- WITS: 2 (lowest)
- WILL: 2 (lowest)
- STURDINESS: 4 (highest)
- **Total**: 15

**Starting Abilities**:

1. **Strike**: Standard melee attack, 2d6+MIGHT damage, 10 Stamina
2. **Defensive Stance**: Enter defensive mode, +3 Soak, -25% damage dealt, 15 Stamina
3. **Warrior's Vigor** (Passive): +10% Maximum HP

**Resource Pools** (at creation, Milestone 0):

- HP: 50 + (4 × 10) = 90 → 99 (with Vigor bonus) = **99 HP**
- Stamina: 20 + (4+3) × 5 = **55 Stamina**
- AP: **10 AP** (minimal, not primary resource)

**Available Specializations**:

- Berserkr (Burst DPS, Fury resource, high risk)
- Iron-Bane (Anti-mechanical/undying, Righteous Fervor resource)
- Skjaldmaer (Shield tank, defensive specialist)
- Skar-Horde Aspirant (Savage berserker, Savagery resource)
- Atgeir-wielder (Reach weapon specialist, versatile)
- GorgeMawAscetic (Heretical, Corruption-focused)

---

### Adept Archetype

**Identity**: Balanced skill-based generalist, mundane specialist

**Starting Attributes**:

- MIGHT: 3
- FINESSE: 3
- WITS: 3
- WILL: 2 (lowest)
- STURDINESS: 3
- **Total**: 14 (Note: Only archetype with 14 points, not 15)

**Starting Abilities**:

1. **Exploit Weakness**: Analyze enemy defenses, grant +2 bonus dice to next attack, 5 Stamina
2. **Scavenge**: Search area for resources, find consumable items, 10 Stamina
3. **Resourceful** (Passive): +20% effectiveness of consumable items (potions, poultices, etc.)

**Resource Pools** (at creation, Milestone 0):

- HP: 50 + (3 × 10) = **80 HP**
- Stamina: 20 + (3+3) × 5 = **50 Stamina**
- AP: **10 AP** (minimal, not primary resource)

**Available Specializations**:

- Bone-Setter (Healer/Support, mundane medic, field medicine)
- Jötun-Reader (Utility/Analyst, system diagnostician, lore specialist)
- Skald (Bard/Buffer, morale and inspiration, performance)
- Scrap-Tinker (Crafting specialist, brewmaster & gadgeteer)
- Einbui (Lone survivor, self-sufficient, isolation specialist)

---

### Skirmisher Archetype

**Identity**: Agile, precision-based combatant with mobility

**Starting Attributes**:

- MIGHT: 3
- FINESSE: 4 (highest)
- WITS: 3
- WILL: 2 (lowest)
- STURDINESS: 3
- **Total**: 15

**Starting Abilities**:

1. **Quick Strike**: Fast attack, 2d6+FINESSE damage, 8 Stamina
2. **Evasive Stance**: Enter dodge mode, +3 Evasion, -10% damage dealt, 12 Stamina
3. **Fleet Footed** (Passive): +1 Movement range, +1 initiative

**Resource Pools** (at creation, Milestone 0):

- HP: 50 + (3 × 10) = **80 HP**
- Stamina: 20 + (3+4) × 5 = **55 Stamina** (same as Warrior)
- AP: **10 AP** (minimal, not primary resource)

**Available Specializations**:

- Veiðimaðr (Hunter, tracking and precision)
- Myrk-gengr (Shadow-Walker, stealth and ambush)
- Strandhogg (Glitch-Raider, exploits Jötun systems)
- Hlekkr-master (Chain-Master, crowd control)

---

### Mystic Archetype

**Identity**: Aether-wielding Weaver, mental challenges specialist

**Starting Attributes**:

- MIGHT: 2 (lowest)
- FINESSE: 3
- WITS: 4 (highest)
- WILL: 4 (highest)
- STURDINESS: 2 (lowest)
- **Total**: 15

**Starting Abilities**:

1. **Aether Dart**: Ranged Aetheric attack, 2d6+WILL damage, 5 AP
2. **Focus Aether**: Restore 15 AP, 10 Stamina cost, channeling action
3. **Aetheric Attunement** (Passive): +10 Maximum AP, +1 AP regen per turn

**Resource Pools** (at creation, Milestone 0):

- HP: 50 + (2 × 10) = **70 HP** (lowest)
- Stamina: 20 + (2+3) × 5 = **45 Stamina** (lowest)
- AP: 20 + (4+4) × 5 + 10 (Attunement) = **70 AP** (highest)

**Available Specializations**:

- Seidkona (Seer, divination and Aetheric support)
- EchoCaller (Aetheric sound manipulation, debuffs and control)

---

**End of Specification**