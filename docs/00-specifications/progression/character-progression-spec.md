# Character Progression System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-19
> **Status**: Active
> **Specification ID**: SPEC-PROGRESSION-001

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-19 | AI + Human | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [x] **Review**: Ready for stakeholder review
- [x] **Approved**: Approved for implementation
- [x] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders
- **Primary Owner**: Game Designer
- **Design**: Progression pacing, build variety
- **Balance**: PP economy, attribute scaling
- **Implementation**: SagaService.cs, PlayerCharacter.cs
- **QA/Testing**: Progression path testing

---

## Executive Summary

### Purpose Statement
The Character Progression System provides milestone-based advancement where players earn Legend (XP) from encounters, reach Milestones for major rewards, and spend Progression Points (PP) to customize their character build.

### Scope
**In Scope**:
- Legend (XP) earning and accumulation
- Milestone thresholds and rewards
- Progression Point (PP) economy
- Attribute increases (5 core attributes)
- Archetype system (Warrior, Skirmisher, Mystic)
- Resource pool scaling (HP, Stamina, Aether Pool)

**Out of Scope**:
- Detailed ability trees → `SPEC-PROGRESSION-003`
- Specialization mechanics → `SPEC-PROGRESSION-004`
- Equipment progression → `SPEC-ECONOMY-002`
- Trauma economy → `SPEC-NARRATIVE-003`

### Success Criteria
- **Player Experience**: Progression feels meaningful; Milestones are moments of triumph
- **Technical**: PP spending correctly updates attributes and resource pools
- **Design**: Multiple viable build paths with 5 PP budget
- **Balance**: Legend earning paces at ~3 Milestones per 20-minute session

---

## Related Documentation

### Dependencies
**Depends On**:
- Dice Pool System: Attribute-based rolls → `docs/01-systems/combat-resolution.md:45`
- Combat System: Legend awards from victories → `SPEC-COMBAT-001`
- Character Creation: Starting attributes and archetype → (out of scope)

**Depended Upon By**:
- Combat Resolution: FINESSE determines initiative → `SPEC-COMBAT-001`
- Damage Calculation: MIGHT/FINESSE/WILL affect damage → `SPEC-COMBAT-002`
- Ability System: Attributes scale ability effectiveness → `SPEC-PROGRESSION-003`
- Trauma Economy: WILL and WITS resist stress/corruption → `SPEC-NARRATIVE-003`

### Related Specifications
- `SPEC-COMBAT-001`: Combat Resolution (uses attributes)
- `SPEC-PROGRESSION-003`: Ability Advancement (PP spending)
- `SPEC-PROGRESSION-004`: Specialization System (PP unlock)
- `SPEC-NARRATIVE-003`: Trauma Economy (WILL/WITS)

### Implementation Documentation
- **System Docs**: `docs/01-systems/legend-leveling.md`
- **System Docs**: `docs/01-systems/attributes.md`
- **Statistical Registry**: `docs/02-statistical-registry/` (attribute scaling tables)
- **Balance Reference**: `docs/05-balance-reference/` (PP economy analysis)

### Code References
- **Primary Service**: `RuneAndRust.Engine/SagaService.cs`
- **Core Models**: `RuneAndRust.Core/PlayerCharacter.cs`
- **Archetype Models**: `RuneAndRust.Core/Archetypes/`
- **Tests**: `RuneAndRust.Tests/SagaServiceTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Milestone-Based Rewards**
   - **Rationale**: Concentrated rewards feel more impactful than gradual trickle
   - **Examples**: Full heal on Milestone creates "moment of triumph"; +10 HP is significant

2. **Build Variety Through Choice**
   - **Rationale**: Limited PP forces meaningful trade-offs; no "optimal" build
   - **Examples**: 5 PP can specialize 1 attribute OR diversify OR unlock specialization

3. **Deed-Driven Progression**
   - **Rationale**: Earn Legend through actions (combat, puzzles, exploration), not grinding
   - **Examples**: Boss kills award bonus Legend; clever puzzle solving grants Legend

### Player Experience Goals
**Target Experience**: Progression feels earned; each Milestone is a celebration of achievement

**Moment-to-Moment Gameplay**:
- Player sees Legend bar fill toward next Milestone
- Reaching Milestone triggers full heal + reward announcement
- Player spends PP between encounters to customize build
- Attribute increases immediately affect combat effectiveness

**Learning Curve**:
- **Novice** (0-2 hours): Understand Legend → Milestone → PP → Spend PP
- **Intermediate** (2-10 hours): Plan builds around archetype synergies
- **Expert** (10+ hours): Optimize PP spending for specific boss encounters

### Design Constraints
- **Technical**: Must support 3 archetypes with different attribute priorities
- **Gameplay**: Progression must fit within 20-minute play session (3 Milestones)
- **Narrative**: "Legend" terminology (not "XP") fits post-apocalyptic setting
- **Scope**: v0.1 caps at Milestone 3 (scalable to higher levels later)

---

## Functional Requirements

### FR-001: Award Legend from Encounters
**Priority**: Critical
**Status**: Implemented

**Description**:
System must calculate and award Legend (XP) to player after completing combat encounters, puzzles, or significant actions.

**Rationale**:
Legend is the primary progression driver. Players must earn it through gameplay to advance.

**Acceptance Criteria**:
- [ ] Legend calculated using formula: `Legend = BLV × DM × TM`
- [ ] Base Legend Value (BLV) varies by encounter (20-150)
- [ ] Difficulty Modifier (DM) defaults to 1.0 (future: difficulty modes)
- [ ] Trauma Modifier (TM) ranges 1.0-1.25 (Boss/Puzzle = 1.25)
- [ ] Legend added to PlayerCharacter.CurrentLegend
- [ ] Legend award displayed to player (combat log / UI)

**Example Scenarios**:
1. **Scenario**: Player defeats standard enemy (BLV=30)
   - **Input**: Combat victory, enemy BLV=30, DM=1.0, TM=1.0
   - **Expected Output**: Legend += 30 (30 × 1.0 × 1.0 = 30)
   - **Success Condition**: CurrentLegend increased by 30

2. **Edge Case**: Player defeats boss (BLV=120, TM=1.25)
   - **Input**: Boss defeated, BLV=120, DM=1.0, TM=1.25
   - **Expected Behavior**: Legend += 150 (120 × 1.0 × 1.25 = 150)

**Dependencies**:
- Requires: Combat victory event
- Requires: Enemy.BaseLegendValue property
- Blocks: FR-002 (cannot check Milestone without Legend)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/SagaService.cs:AwardLegend()`
- **Data Requirements**: Enemy BLV, encounter type for TM
- **Performance Considerations**: Instant calculation

---

### FR-002: Check and Award Milestones
**Priority**: Critical
**Status**: Implemented

**Description**:
When CurrentLegend ≥ LegendToNextMilestone, system must advance Milestone and grant rewards (+10 HP, +5 Stamina, +1 PP, full heal).

**Rationale**:
Milestones are the primary reward mechanic. Players must receive immediate, significant benefits.

**Acceptance Criteria**:
- [ ] Detect when CurrentLegend ≥ LegendToNextMilestone
- [ ] Increment CurrentMilestone by 1
- [ ] Grant +10 Maximum HP
- [ ] Grant +5 Maximum Stamina
- [ ] Grant +1 Progression Point (PP)
- [ ] Set HP = Maximum HP (full heal)
- [ ] Set Stamina = Maximum Stamina (full stamina restore)
- [ ] Recalculate LegendToNextMilestone: `(CurrentMilestone × 50) + 100`
- [ ] Display Milestone achievement message
- [ ] Legend does NOT reset (carries over to next Milestone)

**Example Scenarios**:
1. **Scenario**: Player reaches Milestone 1
   - **Input**: CurrentLegend = 120, LegendToNextMilestone = 100, Milestone 0
   - **Expected Output**: Milestone = 1, MaxHP += 10, MaxStamina += 5, PP += 1, HP/Stamina full, LegendToNext = 150
   - **Success Condition**: All rewards applied, Legend stays 120 (carries over)

2. **Edge Case**: Player earns enough Legend to skip a Milestone
   - **Input**: CurrentLegend jumps from 80 to 260 (massive boss kill)
   - **Expected Behavior**: Award Milestone 1 (100) AND Milestone 2 (250 cumulative), all rewards stack

**Dependencies**:
- Requires: FR-001 (Legend must be awarded first)
- Blocks: FR-003 (PP cannot be spent until awarded)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/SagaService.cs:CheckMilestone()`
- **Data Requirements**: PlayerCharacter with CurrentLegend, CurrentMilestone
- **Performance Considerations**: Instant check after each Legend award

---

### FR-003: Spend PP on Attribute Increases
**Priority**: High
**Status**: Implemented

**Description**:
Player can spend 1 PP to increase any core attribute (MIGHT, FINESSE, WITS, WILL, STURDINESS) by 1, up to maximum of 6 per attribute.

**Rationale**:
Attribute customization is core build expression. 1 PP = 1 attribute point provides clear cost/benefit.

**Acceptance Criteria**:
- [ ] Player can select attribute to increase (UI menu)
- [ ] Cost = 1 PP per +1 attribute
- [ ] Attribute cannot exceed 6 (hard cap)
- [ ] PP deducted from PlayerCharacter.ProgressionPoints
- [ ] Attribute value incremented
- [ ] Resource pools recalculated if affected (HP from STURDINESS, Stamina from MIGHT+FINESSE, Aether from WILL+WITS)
- [ ] Confirmation message displayed

**Example Scenarios**:
1. **Scenario**: Player increases MIGHT from 3 to 4
   - **Input**: Player has 5 PP, MIGHT = 3, selects "Increase MIGHT"
   - **Expected Output**: PP = 4, MIGHT = 4, Stamina recalculated (+5)
   - **Success Condition**: MIGHT increased, Stamina updated, PP deducted

2. **Edge Case**: Player tries to increase attribute at cap (6)
   - **Input**: FINESSE = 6, player selects "Increase FINESSE"
   - **Expected Behavior**: Error message "FINESSE is already at maximum (6)", no PP spent

3. **Edge Case**: Player has 0 PP
   - **Input**: PP = 0, player tries to increase attribute
   - **Expected Behavior**: Error message "Not enough Progression Points", action blocked

**Dependencies**:
- Requires: FR-002 (PP must be awarded via Milestones)
- Requires: Attribute system (5 core attributes)
- Blocks: Combat effectiveness (attributes affect dice pools)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/SagaService.cs:SpendPPOnAttribute()`
- **Data Requirements**: PlayerCharacter.ProgressionPoints, PlayerCharacter.Attributes
- **Performance Considerations**: Instant update

---

### FR-004: Archetype Defines Starting Attributes
**Priority**: High
**Status**: Implemented

**Description**:
At character creation, selected archetype (Warrior, Skirmisher, Mystic) determines starting attribute distribution (total 15 points, distributed differently).

**Rationale**:
Archetypes provide distinct playstyles. Starting attributes guide early-game tactics.

**Acceptance Criteria**:
- [ ] Warrior: MIGHT 4, FINESSE 3, WITS 2, WILL 2, STURDINESS 4 (total 15)
- [ ] Skirmisher: MIGHT 3, FINESSE 4, WITS 3, WILL 2, STURDINESS 3 (total 15)
- [ ] Mystic: MIGHT 2, FINESSE 3, WITS 4, WILL 4, STURDINESS 2 (total 15)
- [ ] Starting attributes set at character creation
- [ ] Cannot be changed after creation (only increased via PP)

**Example Scenarios**:
1. **Scenario**: Player selects Warrior archetype
   - **Input**: Character creation, archetype = Warrior
   - **Expected Output**: MIGHT=4, FINESSE=3, WITS=2, WILL=2, STURDINESS=4
   - **Success Condition**: Attributes match Warrior distribution

**Dependencies**:
- Requires: Character creation system (out of scope)
- Blocks: FR-003 (starting attributes provide baseline for increases)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Core/Archetypes/WarriorArchetype.cs`, `SkirmisherArchetype.cs`, `MysticArchetype.cs`
- **Data Requirements**: Archetype selection
- **Performance Considerations**: One-time assignment at creation

---

### FR-005: Resource Pools Scale with Attributes
**Priority**: High
**Status**: Implemented

**Description**:
Maximum HP, Stamina, and Aether Pool dynamically recalculate when attributes change (via PP spending or Milestones).

**Rationale**:
Attributes must meaningfully affect survivability and resource availability. Scaling creates investment value.

**Acceptance Criteria**:
- [ ] Maximum HP = 50 + (STURDINESS × 10) + (Milestones × 10)
- [ ] Maximum Stamina = 20 + (MIGHT + FINESSE) × 5 + (Milestones × 5)
- [ ] Maximum Aether Pool (Mystics only) = 20 + (WILL + WITS) × 5
- [ ] Recalculation triggered on attribute increase
- [ ] Recalculation triggered on Milestone advancement
- [ ] Current HP/Stamina/Aether do NOT increase proportionally (only max increases)

**Example Scenarios**:
1. **Scenario**: Warrior increases STURDINESS from 4 to 5
   - **Input**: STURDINESS = 4, MaxHP = 90, spend 1 PP to increase STURDINESS
   - **Expected Output**: STURDINESS = 5, MaxHP = 100 (+10)
   - **Success Condition**: MaxHP recalculated correctly, CurrentHP unchanged

2. **Edge Case**: Warrior at low HP receives Milestone
   - **Input**: CurrentHP = 15, MaxHP = 90, reach Milestone (MaxHP += 10)
   - **Expected Behavior**: MaxHP = 100, CurrentHP = 100 (full heal on Milestone)

**Dependencies**:
- Requires: FR-003 (attribute increases trigger recalc)
- Requires: FR-002 (Milestones trigger recalc)
- Blocks: Combat viability (HP/Stamina affect survival)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Core/PlayerCharacter.cs:RecalculateResourcePools()`
- **Data Requirements**: Attributes, CurrentMilestone
- **Performance Considerations**: Instant recalculation

---

## System Mechanics

### Mechanic 1: Legend Calculation

**Overview**:
Legend (XP) awarded after encounters is calculated using three multipliers: Base Legend Value (encounter difficulty), Difficulty Modifier (game mode), and Trauma Modifier (encounter type).

**How It Works**:
1. Retrieve enemy/encounter Base Legend Value (BLV)
2. Apply Difficulty Modifier (DM) - currently always 1.0
3. Apply Trauma Modifier (TM) - 1.25 for Boss/Puzzle, 1.0 otherwise
4. Multiply: `Legend = BLV × DM × TM`
5. Round down to integer
6. Add to PlayerCharacter.CurrentLegend

**Formula/Logic**:
```
Legend_Awarded = FLOOR(BLV × DM × TM)

Where:
  BLV = Base Legend Value (enemy-specific, range 20-150)
  DM = Difficulty Modifier (default 1.0)
  TM = Trauma Modifier (1.0 or 1.25)

Example 1 (Standard Enemy):
  BLV = 30 (standard melee enemy)
  DM = 1.0 (normal difficulty)
  TM = 1.0 (normal encounter)
  Legend = 30 × 1.0 × 1.0 = 30

Example 2 (Boss):
  BLV = 120 (boss enemy)
  DM = 1.0 (normal difficulty)
  TM = 1.25 (boss modifier)
  Legend = 120 × 1.0 × 1.25 = 150
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| BLV | int | 20-150 | 30 | Encounter difficulty value | Yes (per enemy) |
| DM | float | 0.75-1.5 | 1.0 | Difficulty mode multiplier | Yes (future) |
| TM | float | 1.0-1.25 | 1.0 | Encounter type multiplier | Yes (per type) |

**Edge Cases**:
1. **Multiple enemies defeated**: Sum BLV of all enemies, apply TM once
   - **Condition**: Combat with 3 enemies (BLV 30, 30, 40)
   - **Behavior**: Legend = (30 + 30 + 40) × 1.0 × 1.0 = 100
   - **Example**: Total BLV summed before modifiers

2. **Puzzle completion**: Use fixed BLV (30-60), apply TM=1.25
   - **Condition**: Puzzle solved
   - **Behavior**: Legend = 50 × 1.0 × 1.25 = 62.5 → 62 (rounded)

**Related Requirements**: FR-001

---

### Mechanic 2: Milestone Thresholds

**Overview**:
Each Milestone requires progressively more Legend to reach. Threshold increases linearly: `(Milestone × 50) + 100`. Legend does NOT reset at Milestones.

**How It Works**:
1. Calculate Legend required for next Milestone: `(CurrentMilestone × 50) + 100`
2. When CurrentLegend ≥ LegendToNextMilestone, trigger Milestone
3. Increment CurrentMilestone
4. Recalculate LegendToNextMilestone
5. Legend continues accumulating (no reset)

**Formula/Logic**:
```
Legend_To_Next_Milestone = (Current_Milestone × 50) + 100

Milestone Table:
  Milestone 0 → 1: 100 Legend (cumulative: 100)
  Milestone 1 → 2: 150 Legend (cumulative: 250)
  Milestone 2 → 3: 200 Legend (cumulative: 450)

Example:
  Player starts: Milestone 0, CurrentLegend = 0, LegendToNext = 100
  Earns 120 Legend: CurrentLegend = 120
  Check: 120 ≥ 100 → Trigger Milestone 1
  After Milestone 1: Milestone = 1, CurrentLegend = 120 (not reset), LegendToNext = 150
  Needs 150 total (already at 120, needs 30 more to reach Milestone 2)
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| MilestoneMultiplier | int | 30-100 | 50 | Linear scaling factor | Yes (balance) |
| MilestoneBase | int | 50-200 | 100 | Starting threshold | Yes (balance) |
| MaxMilestone | int | 3-10 | 3 | Milestone cap (v0.1) | Yes (scope) |

**Edge Cases**:
1. **Earn enough Legend to skip Milestone**: Award multiple Milestones in sequence
   - **Condition**: CurrentLegend jumps from 80 to 300 (huge boss kill)
   - **Behavior**: Award Milestone 1 (100), then check again → Award Milestone 2 (250)
   - **Example**: Player gets compounded rewards (HP, Stamina, PP stacked)

2. **Reach MaxMilestone cap**: Stop progression, continue earning Legend (future use)
   - **Condition**: CurrentMilestone = 3 (MaxMilestone)
   - **Behavior**: Legend continues accumulating, no further Milestones awarded

**Related Requirements**: FR-002

---

### Mechanic 3: Progression Point (PP) Economy

**Overview**:
PP is the primary currency for character customization. Players start with 2 PP, earn +1 per Milestone, and spend on attributes (1 PP each), ability ranks (2-3 PP), or specializations (3 PP).

**How It Works**:
1. Player starts with 2 PP at creation
2. Each Milestone grants +1 PP
3. Player spends PP via menu between encounters:
   - Increase attribute: -1 PP, +1 attribute (cap 6)
   - Advance ability rank: -2 to -3 PP, improve ability
   - Unlock specialization: -3 PP, gain specialization abilities
4. PP cannot be refunded (permanent spending)

**Formula/Logic**:
```
Total_PP_Available = Starting_PP + Milestones_Reached

Starting_PP = 2
PP_Per_Milestone = 1

By Milestone 3:
  Total_PP = 2 + 3 = 5 PP

Spending Options:
  Attribute increase: 1 PP → +1 to any attribute (max 6)
  Ability rank: 2-3 PP → Rank 1 to Rank 2
  Specialization: 3 PP → Unlock specialization + Tier 1 abilities

Build Example (5 PP):
  Option A: +3 MIGHT (3 PP) + Advance 1 ability (2 PP) = 5 PP
  Option B: Unlock specialization (3 PP) + +2 attributes (2 PP) = 5 PP
  Option C: +5 to one attribute (5 PP, e.g., MIGHT 3→6, FINESSE 3→4)
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| StartingPP | int | 0-5 | 2 | PP at character creation | Yes (balance) |
| PPPerMilestone | int | 1-3 | 1 | PP gained per Milestone | Yes (balance) |
| AttributeCost | int | 1-2 | 1 | PP cost per attribute point | No (core mechanic) |
| AttributeCap | int | 6-10 | 6 | Maximum attribute value | Yes (balance) |

**Edge Cases**:
1. **Insufficient PP for purchase**: Block action, display error
   - **Condition**: Player has 1 PP, tries to unlock specialization (3 PP)
   - **Behavior**: Error: "Not enough PP (need 3, have 1)"

2. **Attribute at cap**: Block further increases
   - **Condition**: MIGHT = 6, player tries to increase MIGHT
   - **Behavior**: Error: "MIGHT is at maximum (6)"

**Related Requirements**: FR-003

---

## Integration Points

### Systems This System Consumes

#### Integration with Combat System
**What We Use**: Combat victory events to trigger Legend awards
**How We Use It**: Subscribe to OnCombatVictory event, calculate Legend, award to player
**Dependency Type**: Hard (progression requires combat victories)
**Failure Handling**: If combat victory event not fired, no Legend awarded (log error)

**API/Interface**:
```csharp
OnCombatVictory(List<Enemy> defeatedEnemies)
{
    var legend = CalculateLegend(defeatedEnemies);
    _sagaService.AwardLegend(player, legend);
}
```

---

#### Integration with Dice Service
**What We Use**: Attribute values as dice pool sizes
**How We Use It**: Pass attribute value as dice count: `Roll(playerAttribute.Value)`
**Dependency Type**: Hard (combat/checks require dice rolling)
**Failure Handling**: Critical error if attribute invalid (should never happen)

---

### Systems That Consume This System

#### Consumed By Combat Resolution
**What They Use**: Attribute values (FINESSE for initiative, MIGHT/FINESSE for attacks)
**How They Use It**: Read PlayerCharacter.Attributes.Finesse for dice pools
**Stability Contract**: Attributes always valid integers (1-10 range), never null

---

#### Consumed By Trauma Economy
**What They Use**: WILL and WITS for stress/corruption resistance
**How They Use It**: Higher WILL/WITS = better resistance rolls
**Stability Contract**: WILL and WITS always accessible, valid values

---

### Event System Integration

**Events Published**:
| Event Name | Trigger | Payload | Consumers |
|------------|---------|---------|-----------|
| OnMilestoneReached | Milestone threshold met | MilestoneNumber | UI (celebration screen) |
| OnLegendAwarded | Legend granted | LegendAmount | UI (progress bar update) |
| OnAttributeIncreased | PP spent on attribute | AttributeName, NewValue | UI, Combat |
| OnPPSpent | PP deducted | PPRemaining | UI (PP display update) |

**Events Subscribed**:
| Event Name | Source | Handler | Purpose |
|------------|--------|---------|---------|
| OnCombatVictory | CombatEngine | AwardLegend() | Trigger Legend award |
| OnPuzzleSolved | PuzzleSystem | AwardLegend() | Trigger Legend award |

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
|-----------|----------|---------------|-----|-----|--------|------------------|
| Starting PP | CharacterFactory | 2 | 0 | 5 | Build variety at start | Low |
| PP Per Milestone | SagaService | 1 | 1 | 3 | Progression speed | Medium |
| Attribute Cap | Validation | 6 | 5 | 10 | Maximum power level | Low |
| Milestone Multiplier | SagaService | 50 | 30 | 100 | Progression pacing | High |
| Specialization Cost | SagaService | 3 | 2 | 10 | Specialization access | Medium |

### Balance Targets

**Target 1: Reach Milestone 3 in 20 Minutes**
- **Metric**: Average playtime to Milestone 3
- **Current**: 18-22 minutes (450 Legend required)
- **Target**: 15-25 minutes (flexible)
- **Levers**: Enemy BLV, Milestone thresholds

**Target 2: 3+ Viable Builds with 5 PP**
- **Metric**: Number of competitively viable builds
- **Current**: 4+ viable builds identified
- **Target**: Minimum 3 distinct builds
- **Levers**: PP costs, attribute caps, specialization costs

---

## Open Questions & Future Work

### Future Enhancements

**Enhancement 1: Prestige Levels (Beyond Milestone 3)**
- **Rationale**: Extended gameplay for longer sessions
- **Complexity**: Low (extend Milestone formula)
- **Priority**: Medium (post-v0.1)
- **Dependencies**: Longer game sessions, more content

**Enhancement 2: PP Refund/Respec System**
- **Rationale**: Allow build experimentation
- **Complexity**: Medium (track spending history)
- **Priority**: Low (permanent choices create weight)
- **Dependencies**: UI for respec interface

---

## Appendix

### Appendix A: Terminology

| Term | Definition |
|------|------------|
| **Legend** | Experience points earned through gameplay (not "XP") |
| **Milestone** | Major progression threshold (not "level") |
| **Progression Point (PP)** | Currency for character customization |
| **Archetype** | Core character class (Warrior/Skirmisher/Mystic) |
| **Specialization** | Sub-class unlocked with PP (Bone-Setter, Jötun-Reader, etc.) |

### Appendix B: Attribute Definitions

| Attribute | Abbrev | Affects | Archetypes Prioritize |
|-----------|--------|---------|----------------------|
| **MIGHT** | MGT | Physical attack damage, Stamina, carry capacity | Warrior |
| **FINESSE** | FIN | Initiative, evasion, ranged attacks, Stamina | Skirmisher |
| **WITS** | WIT | Puzzle solving, perception, Aether Pool (Mystics) | Mystic, Skirmisher |
| **WILL** | WIL | Mental resistance, Corruption resist, Aether Pool | Mystic |
| **STURDINESS** | STR | Maximum HP, physical resistance | Warrior |

### Appendix C: Milestone Progression Table

| Milestone | Legend Required (This Level) | Cumulative Legend | Total HP Gained | Total Stamina Gained | Total PP Gained |
|-----------|------------------------------|-------------------|-----------------|----------------------|-----------------|
| 0 (Start) | - | 0 | 0 | 0 | 2 (starting) |
| 1 | 100 | 100 | +10 (10 total) | +5 (5 total) | +1 (3 total) |
| 2 | 150 | 250 | +10 (20 total) | +5 (10 total) | +1 (4 total) |
| 3 | 200 | 450 | +10 (30 total) | +5 (15 total) | +1 (5 total) |

---

**End of Specification**
