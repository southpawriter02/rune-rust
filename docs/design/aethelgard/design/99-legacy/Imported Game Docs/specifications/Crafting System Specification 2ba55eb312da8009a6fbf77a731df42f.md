# Crafting System Specification

Parent item: Specs: Systems (Specs%20Systems%202ba55eb312da80c6aa36ce6564319160.md)

> Template Version: 1.0
Last Updated: 2025-11-27
Status: Active
Specification ID: SPEC-SYSTEM-004
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2025-11-27 | AI | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [x]  **Review**: Ready for stakeholder review
- [x]  **Approved**: Approved for implementation
- [x]  **Active**: Currently implemented and maintained
- [ ]  **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders

- **Primary Owner**: Game Designer
- **Design**: Recipe design, station types, component economy
- **Balance**: Success rates, component costs, quality outcomes
- **Implementation**: AdvancedCraftingService.cs, ModificationService.cs
- **QA/Testing**: Recipe validation, quality calculation edge cases

---

## Executive Summary

### Purpose Statement

The Crafting System enables players to create consumables, modify equipment, and craft items through a skill-based recipe system using gathered components at specialized crafting stations.

### Scope

**In Scope**:

- Recipe-based item creation
- Crafting station types and validation
- Component gathering and consumption
- Skill check mechanics for crafting success
- Quality tier calculation
- Equipment modification (runic inscriptions)
- Specialization-specific recipes (Bone-Setter, Einbui)

**Out of Scope**:

- Component drop mechanics → `SPEC-ECONOMY-001` (Loot System)
- Equipment stat definitions → `SPEC-SYSTEM-002` (Inventory & Equipment)
- Merchant trading → `SPEC-ECONOMY-002` (Economy System)
- Consumable effects → Individual consumable specifications

### Success Criteria

- **Player Experience**: Crafting feels rewarding and impactful
- **Technical**: Craft operations complete in <200ms
- **Design**: Recipes provide meaningful progression paths
- **Balance**: Skill checks create tension without frustrating failure rates

---

## Related Documentation

### Dependencies

**Depends On**:

- Character System: Skill attributes for checks → `SPEC-PROGRESSION-001`
- Inventory System: Component storage → `SPEC-SYSTEM-002`
- Dice System: Skill check resolution → `docs/01-systems/dice-pool.md`

**Depended Upon By**:

- Field Medicine: Bone-Setter crafting recipes
- Combat: Consumable usage
- Economy: Component trading

### Related Specifications

- `SPEC-SYSTEM-002`: Inventory & Equipment
- `SPEC-ECONOMY-001`: Loot & Equipment Generation
- `SPEC-ECONOMY-002`: Economy System

### Code References

- **Primary Service**: `RuneAndRust.Engine/Crafting/AdvancedCraftingService.cs`
- **Modification Service**: `RuneAndRust.Engine/Crafting/ModificationService.cs`
- **Recipe Service**: `RuneAndRust.Engine/Crafting/RecipeService.cs`
- **Core Models**: `RuneAndRust.Core/CraftingRecipe.cs`, `RuneAndRust.Core/CraftingComponent.cs`
- **Repository**: `RuneAndRust.Persistence/CraftingRepository.cs`
- **Tests**: `RuneAndRust.Tests/RecipeServiceTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Crafting as Skill Expression**
    - **Rationale**: Character build choices (WITS, specialization) should matter for crafting
    - **Examples**: Higher WITS = better success rate; Bone-Setter = exclusive medical recipes
2. **Component Scarcity Creates Value**
    - **Rationale**: Rare components should feel meaningful to find and use
    - **Examples**: Epic components enable powerful items; component choice affects quality
3. **Station-Based Specialization**
    - **Rationale**: Different stations enable different crafting types
    - **Examples**: Medical stations for consumables; forge stations for equipment

### Player Experience Goals

**Target Experience**: Anticipation when gathering components, satisfaction when crafting succeeds

**Moment-to-Moment Gameplay**:

- Player gathers components from loot and exploration
- Finds crafting station in dungeon
- Reviews available recipes and required components
- Makes skill check; experiences tension
- Receives crafted item with quality determined by inputs

**Learning Curve**:

- **Novice** (0-2 hours): Understands basic recipes, uses field medicine
- **Intermediate** (2-10 hours): Plans component gathering, invests in WITS
- **Expert** (10+ hours): Optimizes component quality for best outputs; uses modifications

### Design Constraints

- **Technical**: All recipes data-driven via database
- **Gameplay**: Components always consumed on attempt (success or failure)
- **Narrative**: Crafting fits post-apocalyptic scavenging aesthetic
- **Scope**: No blueprint discovery (all recipes available)

---

## Functional Requirements

### FR-001: Craft Item via Recipe

**Priority**: Critical
**Status**: Implemented

**Description**:
System must allow players to craft items using recipes at appropriate stations, consuming components and applying skill checks.

**Rationale**:
Core crafting gameplay loop enabling item creation from gathered materials.

**Acceptance Criteria**:

- [x]  Recipe lookup by ID
- [x]  Station type validation
- [x]  Component availability check
- [x]  Skill check resolution
- [x]  Component consumption (success and failure)
- [x]  Quality calculation on success
- [x]  Item added to inventory on success

**Example Scenarios**:

1. **Scenario**: Craft Health Potion at Medical Station
    - **Input**: Recipe(HealthPotion), Station(Medical), Components(2x CommonHerb, 1x CleanCloth)
    - **Skill Check**: WITS 5 + d20 vs DC 10
    - **Expected Output**: Health Potion (Quality based on components)
    - **Success Condition**: Potion in inventory, components consumed
2. **Edge Case**: Crafting fails skill check
    - **Input**: Same as above, but roll total < DC
    - **Expected Behavior**: Components consumed, no item created, failure message

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/Crafting/AdvancedCraftingService.cs:CraftItem()`

---

### FR-002: Validate Crafting Station

**Priority**: High
**Status**: Implemented

**Description**:
System must validate that the crafting station type matches recipe requirements.

**Rationale**:
Station types enable specialization and exploration incentives.

**Acceptance Criteria**:

- [x]  Recipe specifies RequiredStation (or "Any")
- [x]  Station.StationType must match recipe requirement
- [x]  "Any" station requirement bypasses check
- [x]  Clear error message on mismatch

**Station Types**:

| Station Type | Description | Recipe Examples |
| --- | --- | --- |
| Medical | Field medicine, bandages | Health Potions, Trauma Kits |
| Forge | Weapons, armor | Equipment crafting |
| Alchemy | Potions, buffs | Stamina Elixirs |
| Runic | Inscriptions | Equipment modifications |
| Any | No station required | Basic items |

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/Crafting/AdvancedCraftingService.cs:ValidateStation()`

---

### FR-003: Validate and Consume Components

**Priority**: Critical
**Status**: Implemented

**Description**:
System must validate player has required components and consume them on crafting attempt.

**Rationale**:
Component economy drives exploration and meaningful resource decisions.

**Acceptance Criteria**:

- [x]  Check player inventory for each required component type
- [x]  Check quantity meets recipe requirements
- [x]  Track lowest quality component (affects output)
- [x]  Consume all required components on attempt (success or failure)
- [x]  Clear feedback on missing components

**Example Scenarios**:

1. **Scenario**: Player has exact components needed
    - **Input**: Needs 2x CommonHerb, player has 3
    - **Expected Output**: Validation passes, 2 consumed
2. **Edge Case**: Player missing component
    - **Input**: Needs 1x Stimulant, player has 0
    - **Expected Behavior**: Failure with "Missing: Stimulant (0/1)"

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/Crafting/AdvancedCraftingService.cs:ValidateComponents()`

---

### FR-004: Resolve Crafting Skill Check

**Priority**: Critical
**Status**: Implemented

**Description**:
System must resolve skill checks using recipe's skill attribute vs DC.

**Rationale**:
Skill checks create tension and reward character investment.

**Formula/Logic**:

```
Skill Check = Roll(AttributeValue d6).TotalValue vs DC

Example:
  Recipe.SkillAttribute = "WITS"
  Character.WITS = 5
  Recipe.SkillCheckDC = 10

  Roll: 5d6 = [3, 5, 6, 2, 4] = 20
  Result: 20 >= 10 → SUCCESS

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| SkillCheckDC | int | 8-20 | 10 | Target number for success | Yes (per recipe) |
| SkillAttribute | string | - | WITS | Attribute used for check | Yes (per recipe) |

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/Crafting/AdvancedCraftingService.cs:CraftItem()`

---

### FR-005: Calculate Quality Outcome

**Priority**: High
**Status**: Implemented

**Description**:
System must calculate crafted item quality based on component quality, station tier, and recipe bonuses.

**Rationale**:
Quality system rewards using better components and finding better stations.

**Formula/Logic**:

```
Final Quality = min(
  LowestComponentQuality,
  Station.MaxQualityTier
) + Recipe.QualityBonus

Quality Tiers:
  0 = Jury-Rigged
  1 = Scavenged
  2 = Clan-Forged
  3 = Optimized
  4 = Myth-Forged

Example:
  Lowest component quality = Clan-Forged (2)
  Station max tier = 3 (Optimized)
  Recipe quality bonus = 0

  Final Quality = min(2, 3) + 0 = 2 (Clan-Forged)

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| LowestComponentQuality | int | 0-4 | - | Quality of worst component | No |
| MaxQualityTier | int | 0-4 | 3 | Station quality cap | Yes (per station) |
| QualityBonus | int | -1 to +1 | 0 | Recipe quality modifier | Yes (per recipe) |

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/Crafting/AdvancedCraftingService.cs:CalculateQuality()`

---

### FR-006: Apply Equipment Modification

**Priority**: High
**Status**: Implemented

**Description**:
System must allow applying runic inscriptions to equipment, consuming components and credits.

**Rationale**:
Modifications enable equipment customization and progression.

**Acceptance Criteria**:

- [x]  Validate inscription exists
- [x]  Validate equipment exists in inventory
- [x]  Validate equipment type matches inscription target
- [x]  Check modification slot limit (max 3)
- [x]  Validate and consume component requirements
- [x]  Deduct credit cost if applicable
- [x]  Apply modification to equipment

**Example Scenarios**:

1. **Scenario**: Apply Sharpness Rune to weapon
    - **Input**: Equipment(Iron Sword), Inscription(Sharpness I)
    - **Cost**: 2x AethericDust, 50 credits
    - **Expected Output**: Sword gains +1 damage modifier
2. **Edge Case**: Equipment at max modifications
    - **Input**: Sword already has 3 inscriptions
    - **Expected Behavior**: Failure with "max modifications" message

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/Crafting/ModificationService.cs:ApplyModification()`
- **Max Modifications**: 3 per item

---

### FR-007: Remove Equipment Modification

**Priority**: Medium
**Status**: Implemented

**Description**:
System must allow removing modifications from equipment.

**Rationale**:
Players may want to replace modifications or prepare equipment for sale.

**Acceptance Criteria**:

- [x]  Validate modification exists and belongs to character
- [x]  Remove modification from equipment
- [x]  No component refund on removal

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/Crafting/ModificationService.cs:RemoveModification()`

---

## System Mechanics

### Mechanic 1: Component Types and Rarity

**Overview**:
Components are categorized by type and rarity, affecting crafting options and quality.

**Component Categories**:

| Category | Types | Rarity Range | Use |
| --- | --- | --- | --- |
| Field Medicine | CommonHerb, CleanCloth, Suture, Antiseptic, Splint, Stimulant | Common-Rare | Medical consumables |
| Economy Common | ScrapMetal, RustedComponents, ClothScraps, BoneShards | Common | Basic crafting |
| Economy Uncommon | StructuralScrap, AethericDust, TemperedSprings, MedicinalHerbs | Uncommon | Quality crafting |
| Economy Rare | DvergrAlloyIngot, CorruptedCrystal, AncientCircuitBoard | Rare | Advanced crafting |
| Economy Epic | JotunCoreFragment, RunicEtchingTemplate | Epic | Legendary crafting |

**Rarity Effects**:

| Rarity | Drop Rate | Sell Value | Quality Contribution |
| --- | --- | --- | --- |
| Common | High | 5-20 Cogs | Tier 1 |
| Uncommon | Medium | 25-75 Cogs | Tier 2 |
| Rare | Low | 100-300 Cogs | Tier 3 |
| Epic | Very Low | 500-1000 Cogs | Tier 4 |

**Related Requirements**: FR-003, FR-005

---

### Mechanic 2: Skill Check Resolution

**Overview**:
Crafting success is determined by dice pool checks using character attributes.

**How It Works**:

1. Recipe specifies SkillAttribute (usually WITS)
2. Get character's attribute value
3. Roll that many d6
4. Sum all dice for total
5. Compare total to recipe DC
6. Success if total >= DC

**Data Flow**:

```
Input Sources:
  → Recipe.SkillAttribute
  → Recipe.SkillCheckDC
  → Character.Attributes[SkillAttribute]

Processing:
  → DiceService.Roll(attributeValue)
  → Compare total to DC

Output Destinations:
  → CraftedItemResult.SkillCheckPassed
  → CraftedItemResult.SkillCheckRoll

```

**Edge Cases**:

1. **Very high attribute**: Auto-success likely but not guaranteed
    - **Condition**: WITS 10 vs DC 10
    - **Behavior**: Average roll 35, almost always succeeds
2. **Very low attribute**: High failure rate
    - **Condition**: WITS 2 vs DC 12
    - **Behavior**: Average roll 7, usually fails

**Related Requirements**: FR-004

---

### Mechanic 3: Station Quality Caps

**Overview**:
Crafting stations impose maximum quality limits on outputs.

**Station Quality Tiers**:

| Station Tier | Name | Max Quality Output |
| --- | --- | --- |
| 1 | Makeshift | Scavenged (1) |
| 2 | Functional | Clan-Forged (2) |
| 3 | Advanced | Optimized (3) |
| 4 | Master | Myth-Forged (4) |

**Quality Cap Logic**:

```
Even with Epic components (Tier 4):
  - At Makeshift station: Output capped at Scavenged (1)
  - At Advanced station: Output capped at Optimized (3)

Finding better stations enables better crafting outcomes.

```

**Related Requirements**: FR-002, FR-005

---

## State Management

### System State

**State Variables**:

| Variable | Type | Persistence | Default | Description |
| --- | --- | --- | --- | --- |
| PlayerComponents | Dict<ComponentType, int> | Permanent | {} | Component inventory |
| EquipmentModifications | List<Modification> | Permanent | [] | Active modifications |
| TimesCrafted | Dict<RecipeId, int> | Permanent | {} | Craft count per recipe |

**State Transitions**:

```
[Components in Inventory] ---CraftItem(success)---> [Item Created, Components Consumed]
[Components in Inventory] ---CraftItem(failure)---> [Components Consumed]
[Equipment Unmodified] ---ApplyModification---> [Equipment Modified]
[Equipment Modified] ---RemoveModification---> [Modification Removed]

```

### Persistence Requirements

**Must Persist**:

- Player component inventory
- Equipment modifications
- Crafting statistics (times crafted)

**Save Format**:

- Components: JSON dictionary in SaveData.CraftingComponentsJson
- Modifications: Stored in Modifications table linked to equipment
- Statistics: PlayerRecipes table with counts

---

## Integration Points

### Systems This System Consumes

### Integration with Dice Service

**What We Use**: Dice pool rolling for skill checks
**How We Use It**: Roll(attributeValue).TotalValue
**Dependency Type**: Soft (can use default)
**Failure Handling**: Use internal dice roller if service unavailable

### Integration with Inventory System

**What We Use**: Equipment storage for modifications
**How We Use It**: Validate equipment ownership, apply stat changes
**Dependency Type**: Hard
**Failure Handling**: Fail modification if equipment not found

### Systems That Consume This System

### Consumed By Field Medicine

**What They Use**: Crafting recipes for medical items
**How They Use It**: Bone-Setter specialization uses medical recipes
**Stability Contract**: Recipe IDs stable for specialization references

### Consumed By Economy System

**What They Use**: Component values for trading
**How They Use It**: SellValue property determines merchant prices
**Stability Contract**: Sell values defined per component type

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
| --- | --- | --- | --- | --- | --- | --- |
| BaseSkillDC | Per recipe | 10 | 8 | 20 | Success rate | Medium |
| MaxModifications | ModificationService | 3 | 1 | 5 | Equipment power | Low |
| ComponentConsume | AdvancedCraftingService | Always | - | - | Economy pressure | Low |

### Balance Targets

**Target 1: Crafting Success Rate**

- **Metric**: Success rate for average character (WITS 5) on DC 10 recipe
- **Current**: ~95% (5d6 avg = 17.5)
- **Target**: 85-95% for common recipes
- **Levers**: DC adjustment, attribute requirements

**Target 2: Component Value Progression**

- **Metric**: Ratio between rarity tier sell values
- **Current**: ~3-5x per tier
- **Target**: 3-4x per tier for smooth progression
- **Levers**: Individual component SellValue

---

## Implementation Guidance

### Implementation Status

**Current State**: Complete

**Completed**:

- [x]  AdvancedCraftingService with full crafting flow
- [x]  ModificationService for equipment inscriptions
- [x]  RecipeService for recipe management
- [x]  CraftingRepository for data persistence
- [x]  Component types and rarity system
- [x]  Quality calculation
- [x]  Station validation

### Code Architecture

**Recommended Structure**:

```
RuneAndRust.Core/
  ├─ CraftingRecipe.cs        // Recipe model
  ├─ CraftingComponent.cs     // Component types and factory
  └─ Crafting/
      └─ CraftingModels.cs    // Supporting models

RuneAndRust.Engine/Crafting/
  ├─ AdvancedCraftingService.cs  // Main crafting logic
  ├─ ModificationService.cs      // Equipment modifications
  ├─ RecipeService.cs            // Recipe management
  └─ CraftingStation.cs          // Station model

RuneAndRust.Persistence/
  └─ CraftingRepository.cs       // Database operations

```

---

## Testing & Verification

### Test Scenarios

### Test Case 1: Successful Craft with Quality Calculation

**Type**: Integration

**Objective**: Verify complete crafting flow with quality output

**Test Steps**:

1. Set up player with components (Uncommon quality)
2. Set up Advanced station (Tier 3)
3. Create recipe with DC 10
4. Mock dice roll to succeed
5. Call CraftItem

**Expected Results**:

- Result.Success = true
- Result.FinalQuality = 2 (min of component tier and station tier)
- Components consumed from inventory

### Test Case 2: Failed Skill Check Consumes Components

**Type**: Unit

**Objective**: Verify components consumed even on failure

**Test Steps**:

1. Set up player with 2x CommonHerb
2. Mock dice roll to fail DC
3. Call CraftItem

**Expected Results**:

- Result.Success = false
- Player has 0 CommonHerb (consumed)

---

## Appendix

### Appendix A: Terminology

| Term | Definition |
| --- | --- |
| **Recipe** | Blueprint defining components, skill check, and output |
| **Component** | Gathered material used in crafting |
| **Station** | Location that enables crafting with quality cap |
| **Modification** | Runic inscription applied to equipment |
| **Quality Tier** | Output power level (0-4) |

### Appendix B: Component Type Summary

| Type | Rarity | Sell Value | Primary Use |
| --- | --- | --- | --- |
| CommonHerb | Common | 5 | Medical crafting |
| CleanCloth | Common | 0 | Medical crafting (not tradeable) |
| ScrapMetal | Common | 10 | Basic equipment |
| AethericDust | Uncommon | 60 | Modifications |
| DvergrAlloyIngot | Rare | 200 | Quality equipment |
| JotunCoreFragment | Epic | 750 | Legendary items |

### Appendix C: Recipe DC Guidelines

| Difficulty | DC | Target Success Rate (WITS 5) |
| --- | --- | --- |
| Easy | 8 | ~99% |
| Standard | 10 | ~95% |
| Moderate | 12 | ~85% |
| Difficult | 15 | ~65% |
| Expert | 18 | ~40% |

---

**End of Specification**