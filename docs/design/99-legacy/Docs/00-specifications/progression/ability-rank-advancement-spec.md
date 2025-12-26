# Ability Rank Advancement System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-19
> **Status**: Draft
> **Specification ID**: SPEC-PROGRESSION-003

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-19 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [ ] **Review**: Ready for stakeholder review
- [ ] **Approved**: Approved for implementation
- [ ] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders
- **Primary Owner**: Game Designer
- **Design**: Ability progression pacing, rank scaling
- **Balance**: PP economy for rank-ups, power curves
- **Implementation**: AbilityService.cs, AbilityRepository.cs
- **QA/Testing**: Rank progression testing, scaling verification

---

## Executive Summary

### Purpose Statement
The Ability Rank Advancement System provides automatic incremental power scaling for abilities as players progress through specialization trees. Ranks increase automatically based on tree progression milestones (learning Tier 2 abilities grants Rank 2, Capstone grants Rank 3), creating meaningful power growth tied to specialization investment.

### Scope
**In Scope**:
- Ability learning mechanics within specialization trees
- Automatic rank progression system (Tier 1: Rank 1→2→3, Tier 2: Rank 2→3, Tier 3/Capstone: single rank)
- Rank advancement triggers (learning Tier 2 abilities, learning Capstone)
- Rank scaling formulas (damage, healing, duration increases per rank)
- PP costs for learning abilities (Tier 1=2 PP, Tier 2=4 PP, Tier 3=5 PP, Capstone=6 PP)
- Validation rules for ability learning
- Integration with specialization tier unlock system

**Out of Scope**:
- Specialization tree structure → `SPEC-PROGRESSION-002`
- Ability reset/respec mechanics → `SPEC-PROGRESSION-006` (future)
- Individual ability design → Implementation documentation
- Per-specialization balance tuning → Per-spec documents
- Manual rank-up spending (ranks are automatic, not purchased)

### Success Criteria
- **Player Experience**: Rank-ups feel like rewarding milestones; automatic upgrades feel exciting
- **Technical**: Rank scaling applies correctly when triggers are met; all Tier 1/2 abilities upgrade simultaneously
- **Design**: Rank progression naturally flows with specialization tree investment
- **Balance**: Rank 3 abilities feel powerful and reward deep specialization investment

---

## Related Documentation

### Dependencies
**Depends On**:
- Character Progression System: PP economy → `SPEC-PROGRESSION-001`
- Archetype & Specialization System: Ability trees, tier unlocks → `SPEC-PROGRESSION-002`
- Combat Resolution: Abilities execute in combat → `SPEC-COMBAT-001`

**Depended Upon By**:
- Combat effectiveness: Ranked abilities deal more damage → `SPEC-COMBAT-001`
- Build optimization: Players optimize rank investments → Player strategies
- Specialization viability: Higher ranks unlock specialization potential → `SPEC-PROGRESSION-002`

### Related Specifications
- `SPEC-PROGRESSION-001`: Character Progression (PP economy)
- `SPEC-PROGRESSION-002`: Archetype & Specialization System (ability trees)
- `SPEC-COMBAT-001`: Combat Resolution (ability execution)
- `SPEC-PROGRESSION-006`: Ability Respec System (future, rank resets)

### Implementation Documentation
- **System Docs**: None (this spec fills that gap)
- **Statistical Registry**: `docs/02-statistical-registry/abilities/`
- **Code Reference**: `AbilityService.cs`, `AbilityRepository.cs`

### Code References
- **Primary Service**: `RuneAndRust.Engine/AbilityService.cs`
- **Repository**: `RuneAndRust.Persistence/AbilityRepository.cs`
- **Core Models**: `RuneAndRust.Core/Ability.cs`, `RuneAndRust.Core/AbilityData.cs`
- **UI**: `RuneAndRust.ConsoleApp/AbilityTreeUI.cs`
- **Tests**: `RuneAndRust.Tests/AbilityServiceTests.cs`, `AbilityRankTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Automatic Mastery Through Investment**
   - **Rationale**: Ranks improve automatically as players invest in specialization; mastery comes from dedication, not micro-spending
   - **Examples**: Learn 2 Tier 2 abilities → all Tier 1 abilities automatically upgrade to Rank 2; feels like a training montage reward

2. **Clear Power Scaling**
   - **Rationale**: Rank increases must provide visible, measurable improvements
   - **Examples**: Rank 2 deals +50% damage (2d6 → 3d6); Rank 3 deals +100% damage (2d6 → 4d6)

3. **Milestone-Driven Progression**
   - **Rationale**: Rank-ups tied to meaningful tree progression milestones (Tier 2 abilities, Capstone), not arbitrary PP spending
   - **Examples**: Rank 1 (starting), Rank 2 (trained, unlocked Tier 2), Rank 3 (mastery, achieved Capstone)

### Player Experience Goals
**Target Experience**: "As I progress through my specialization, my foundational abilities automatically become stronger - my character is mastering their craft"

**Moment-to-Moment Gameplay**:
- Player unlocks specialization (grants ACCESS to learn Tier 1 abilities)
- Player spends PP to learn Tier 1 abilities (2 PP each) at Rank 1
- Player uses Tier 1 abilities in combat, learns their mechanics
- Player learns Tier 2 abilities to access advanced techniques
- **Milestone moment**: After learning 2nd Tier 2 ability → all Tier 1 abilities automatically upgrade to Rank 2
- Player notices Strike now deals 3d6 instead of 2d6 (exciting power spike)
- Player progresses toward Capstone
- **Ultimate moment**: Player learns Capstone → all Tier 1 abilities jump to Rank 3, all Tier 2 abilities jump to Rank 3
- Player feels peak mastery of specialization

**Learning Curve**:
- **Novice** (0-2 hours): Learn Tier 1 abilities (Rank 1), understand specialization identity
- **Intermediate** (2-10 hours): Unlock Tier 2, experience first automatic rank-up to Rank 2
- **Expert** (10+ hours): Achieve Capstone, witness all abilities reach Rank 3 mastery

### Design Constraints
- **Technical**: Must track learned abilities per tier to trigger automatic rank-ups
- **Gameplay**: Tier 1 abilities have 3 ranks, Tier 2 have 2 ranks (start at Rank 2), Tier 3/Capstone have 1 rank
- **Narrative**: Rank advancement represents mastery through practice and training
- **Scope**: All 3 ranks implemented; automatic rank-up triggers based on tier progression

---

## Functional Requirements

### FR-001: Learn Ability from Specialization Tree
**Priority**: Critical
**Status**: Implemented

**Description**:
Players can learn abilities from unlocked specializations by spending PP. Abilities start at different ranks based on tier: Tier 1 starts at Rank 1, Tier 2 starts at Rank 2, Tier 3 starts at Rank 3, Capstone starts at Rank 3. Learning requires: specialization unlocked, tier unlocked (PP in tree threshold), sufficient PP, and prerequisites met.

**Rationale**:
Learning is the entry point to ability progression. Initial rank depends on tier - higher tier abilities are inherently more powerful. Tier 1 abilities will rank up automatically as player progresses.

**Acceptance Criteria**:
- [ ] Player can browse abilities in unlocked specializations
- [ ] Abilities display PP cost (Tier 1=2 PP, Tier 2=4 PP, Tier 3=5 PP, Capstone=6 PP)
- [ ] Learning deducts PP from PlayerCharacter.ProgressionPoints
- [ ] **Tier 1 abilities** added at Rank 1 (will auto-upgrade to Rank 2, then Rank 3)
- [ ] **Tier 2 abilities** added at Rank 2 (will auto-upgrade to Rank 3 on Capstone)
- [ ] **Tier 3 and Capstone abilities** added at Rank 3 (single rank only)
- [ ] Learned abilities tracked in CharacterAbility table with correct starting rank
- [ ] PP spent on ability added to specialization's PPSpentInTree counter
- [ ] Error handling: insufficient PP, unmet prerequisites, tier locked

**Example Scenarios**:
1. **Scenario**: Warrior learns "Furious Strike" (Berserkr Tier 1 ability)
   - **Input**: Warrior has Berserkr unlocked, 2 PP available
   - **Expected Output**: Furious Strike added at Rank 1, PP = 0, PPInTree = 2
   - **Success Condition**: Ability usable in combat at Rank 1 power

2. **Scenario**: Warrior learns "Berserk Rage" (Berserkr Tier 2 ability)
   - **Input**: Warrior has Berserkr unlocked, 8 PP in Berserkr tree, 4 PP available
   - **Expected Output**: Berserk Rage added at Rank 2 (starts powerful), PP = 0, PPInTree = 12
   - **Success Condition**: Ability starts at Rank 2, will upgrade to Rank 3 when Capstone is learned

3. **Edge Case**: Player tries to learn Tier 3 ability with only 10 PP in tree (requires 16)
   - **Input**: PP in tree = 10, tries to learn Tier 3 ability
   - **Expected Behavior**: Error "Requires 16 PP in specialization tree (you have 10)"
   - **Success Condition**: Learning blocked, PP not deducted

**Dependencies**:
- Requires: Specialization unlocked → `SPEC-PROGRESSION-002:FR-005`
- Requires: PP available → `SPEC-PROGRESSION-001:FR-002`
- Blocks: FR-002 (must learn before ranking up)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/AbilityService.cs:LearnAbility()`
- **Data Requirements**: AbilityData (PP cost, tier, prerequisites), CharacterSpecialization (track unlock)
- **Performance Considerations**: Database insert on learn

---

### FR-002: Automatic Rank Advancement to Rank 2
**Priority**: High
**Status**: Needs Implementation

**Description**:
When a player learns their 2nd Tier 2 ability within a specialization, ALL Tier 1 abilities in that specialization automatically advance from Rank 1 to Rank 2. This advancement is free (no PP cost) and happens immediately. The power increase represents the character's growing mastery of the specialization's foundational techniques.

**Rationale**:
Automatic rank-ups reward specialization investment and create exciting "power spike" moments. Learning advanced techniques (Tier 2) naturally improves mastery of basic techniques (Tier 1). Free advancement removes micro-management and emphasizes milestone achievements.

**Acceptance Criteria**:
- [ ] System tracks how many Tier 2 abilities player has learned per specialization
- [ ] When 2nd Tier 2 ability is learned, trigger automatic rank-up event
- [ ] ALL learned Tier 1 abilities in that specialization upgrade from Rank 1 → Rank 2
- [ ] CharacterAbility.CurrentRank updated (1 → 2) for each Tier 1 ability
- [ ] Rank 2 scaling applied automatically (+1d6 damage, +1 turn duration, etc.)
- [ ] UI notification: "Your foundational abilities have improved! [List of Tier 1 abilities] are now Rank 2"
- [ ] No PP cost for automatic rank-up (free mastery reward)
- [ ] Multiple specializations track rank-ups independently

**Example Scenarios**:
1. **Scenario**: Warrior learns 2nd Tier 2 ability in Berserkr tree
   - **Input**: Warrior has learned 3 Tier 1 Berserkr abilities (Furious Strike, Battle Shout, Reckless Charge all Rank 1), just learned 2nd Tier 2 ability
   - **Expected Output**: Furious Strike, Battle Shout, Reckless Charge all automatically upgrade to Rank 2
   - **Success Condition**: Furious Strike damage increases from 2d6 → 3d6, all Tier 1 abilities gain +50% power

2. **Scenario**: Player has multiple specializations
   - **Input**: Player has Berserkr (1 Tier 2 learned) and Skjaldmaer (2 Tier 2 learned)
   - **Expected Output**: Only Skjaldmaer Tier 1 abilities are Rank 2; Berserkr Tier 1s remain Rank 1
   - **Success Condition**: Rank-ups are per-specialization, not global

3. **Edge Case**: Player only learned 1 Tier 1 ability before reaching Tier 2
   - **Input**: Skipped 2 Tier 1 abilities, learned 2nd Tier 2 ability
   - **Expected Behavior**: Only the 1 learned Tier 1 ability upgrades to Rank 2 (can't upgrade unlearned abilities)
   - **Success Condition**: Rank-up only affects learned abilities

**Dependencies**:
- Requires: FR-001 (Tier 1 and Tier 2 abilities must be learned)
- Requires: Tier tracking per specialization
- Blocks: None (automatic system)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/AbilityService.cs:LearnAbility()` (check after learning Tier 2)
- **Data Requirements**: Query CharacterAbility by SpecializationID and TierLevel
- **Performance Considerations**: Batch update all Tier 1 abilities in single transaction

---

### FR-003: Automatic Rank Advancement to Rank 3 (Capstone Trigger)
**Priority**: High
**Status**: Needs Implementation

**Description**:
When a player learns the Capstone ability within a specialization, ALL Tier 1 and ALL Tier 2 abilities in that specialization automatically advance to Rank 3. This is the ultimate mastery moment - the character has completed the specialization tree and all their abilities reach peak power.

**Rationale**:
Capstone represents ultimate mastery. Learning it should trigger a cascade of power-ups across all abilities in the spec. This creates a climactic "power fantasy" moment and rewards deep specialization investment.

**Acceptance Criteria**:
- [ ] When Capstone ability is learned, trigger automatic Rank 3 advancement event
- [ ] ALL learned Tier 1 abilities in specialization upgrade to Rank 3 (Rank 1→3 if skipped Tier 2, or Rank 2→3 if already ranked)
- [ ] ALL learned Tier 2 abilities in specialization upgrade to Rank 3 (Rank 2→3)
- [ ] CharacterAbility.CurrentRank updated to 3 for all affected abilities
- [ ] Rank 3 scaling applied (+2d6 total from Rank 1, or +1d6 from Rank 2)
- [ ] UI notification: "You have achieved mastery! ALL abilities in [Specialization] are now Rank 3!"
- [ ] No PP cost (free mastery reward)
- [ ] Tier 3 and Capstone abilities remain at Rank 3 (already at max)

**Example Scenarios**:
1. **Scenario**: Warrior learns Berserkr Capstone after full tree progression
   - **Input**: Warrior has 3 Tier 1 (Rank 2), 3 Tier 2 (Rank 2), 2 Tier 3 (Rank 3), learns Capstone
   - **Expected Output**: All 3 Tier 1 → Rank 3, all 3 Tier 2 → Rank 3, dramatic power spike
   - **Success Condition**: Furious Strike goes from 3d6 (Rank 2) → 4d6 (Rank 3) damage

2. **Scenario**: Player skipped some Tier 1 abilities, learned Capstone
   - **Input**: Only learned 1 of 3 Tier 1 abilities (Rank 2), learned Capstone
   - **Expected Output**: That 1 Tier 1 ability → Rank 3; unlearned abilities NOT affected
   - **Success Condition**: Only learned abilities upgrade

3. **Edge Case**: Player rushed to Capstone, only has 1 Tier 2 learned (never triggered Rank 2)
   - **Input**: 2 Tier 1 abilities still Rank 1, learned Capstone
   - **Expected Behavior**: Tier 1 abilities jump from Rank 1 → Rank 3 (skipping Rank 2 milestone)
   - **Success Condition**: Capstone grants Rank 3 regardless of previous rank

**Dependencies**:
- Requires: FR-001 (abilities must be learned)
- Requires: FR-002 (may have already triggered Rank 2)
- Blocks: None (ultimate advancement)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/AbilityService.cs:LearnAbility()` (detect Capstone tier)
- **Data Requirements**: Query all learned abilities in specialization (Tier 1, Tier 2)
- **Performance Considerations**: Batch update all rankable abilities

---

### FR-004: Rank Scaling Formulas
**Priority**: High
**Status**: Implemented

**Description**:
Abilities scale in power when ranked up according to standardized formulas. Damage abilities gain +1d6 per rank (2d6 → 3d6 → 4d6). Healing abilities gain +1d6 per rank. Duration-based effects gain +1 turn per rank. Status effects may gain additional effects or targets at higher ranks.

**Rationale**:
Consistent scaling ensures all abilities benefit meaningfully from automatic rank-ups. +1d6 per rank (~+3.5 average damage) is noticeable and impactful.

**Acceptance Criteria**:
- [ ] **Damage scaling**: Rank 1 = Nd6, Rank 2 = (N+1)d6, Rank 3 = (N+2)d6
- [ ] **Healing scaling**: Same as damage (+1d6 per rank)
- [ ] **Duration scaling**: Rank 1 = D turns, Rank 2 = D+1 turns, Rank 3 = D+2 turns
- [ ] **Status effect scaling**: Rank 2 may add secondary effect, Rank 3 may add AoE or enhanced effect
- [ ] Scaling applied automatically when CurrentRank changes
- [ ] Custom scaling supported per ability (some abilities scale differently)
- [ ] Tier 2 abilities start at Rank 2 scaling (already powerful base values)

**Example Scenarios**:
1. **Scenario**: "Berserk Rage" damage scaling
   - Rank 1: Deals 2d6 damage
   - Rank 2: Deals 3d6 damage (+50%)
   - Rank 3: Deals 4d6 damage + [Vulnerable] status (future)

2. **Scenario**: "Defensive Stance" duration scaling
   - Rank 1: +3 Defense for 2 turns
   - Rank 2: +3 Defense for 3 turns
   - Rank 3: +4 Defense for 4 turns (future)

3. **Scenario**: "Heal Wounds" healing scaling
   - Rank 1: Heals 2d6 HP
   - Rank 2: Heals 3d6 HP
   - Rank 3: Heals 4d6 HP + removes [Bleeding] (future)

**Dependencies**:
- Requires: FR-002 (rank must be set before scaling applied)
- Requires: Combat system reads CurrentRank → `SPEC-COMBAT-001`

**Implementation Notes**:
- **Code Location**: Ability effects calculated at execution time based on CurrentRank
- **Data Requirements**: AbilityData defines base values; scaling applies multiplicatively
- **Performance Considerations**: No performance impact (simple arithmetic)

---

### FR-005: Ability Tier Unlock Validation
**Priority**: High
**Status**: Implemented

**Description**:
Abilities are organized into tiers (1/2/3/Capstone) within specialization trees. Higher tiers require progressively more PP invested in the specialization tree. Tier 2 requires 8 PP in tree, Tier 3 requires 16 PP, Capstone requires 24 PP.

**Rationale**:
Tier gating ensures players progress through specialization gradually. Can't skip straight to Capstone; must invest in lower tiers first.

**Acceptance Criteria**:
- [ ] Tier 1 abilities: 0 PP in tree required (available after specialization unlock, cost 2 PP each to learn)
- [ ] Tier 2 abilities: 8 PP in tree required
- [ ] Tier 3 abilities: 16 PP in tree required
- [ ] Capstone ability: 24 PP in tree required + both Tier 3 abilities learned
- [ ] PP in tree calculated as SUM(learned abilities' PP costs from this specialization)
- [ ] Tier validation checked before learning ability
- [ ] Clear error messages if tier locked

**Example Scenarios**:
1. **Scenario**: Player unlocks Berserkr, tries to learn Tier 2 ability immediately
   - **Input**: Berserkr just unlocked, 0 PP in Berserkr tree, tries to learn Tier 2 (costs 4 PP)
   - **Expected Behavior**: Error "Requires 8 PP in Berserkr tree (you have 0)"
   - **Success Condition**: Learning blocked until player learns more Tier 1/2 abilities

2. **Scenario**: Player with 12 PP in Bone-Setter tree learns Tier 3 ability
   - **Input**: 12 PP in tree, tries to learn Tier 3 (requires 16 PP)
   - **Expected Behavior**: Error "Requires 16 PP in tree (you have 12)"
   - **Success Condition**: Must spend 4 more PP before Tier 3 unlocks

**Dependencies**:
- Requires: Specialization unlock → `SPEC-PROGRESSION-002:FR-005`
- Requires: PP tracking per specialization
- Blocks: FR-001 (tier must be unlocked before learning)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/AbilityService.cs:ValidatePrerequisites()`
- **Data Requirements**: CharacterSpecialization.PPSpentInTree, AbilityPrerequisites.RequiredPPInTree
- **Performance Considerations**: Simple query and arithmetic

---

## System Mechanics

### Mechanic 1: Automatic Rank Progression Flow

**Overview**:
Ability ranks advance automatically based on specialization tree progression milestones. No PP is spent on rank-ups - they are free rewards for deepening specialization investment. Tiers and ranks are separate concepts: tier determines when you can learn an ability, rank determines its power level.

**How It Works**:
1. **Specialization Unlock Phase** (3 PP):
   - Player unlocks specialization
   - Grants ACCESS to learn abilities from this specialization's tree
   - All Tier 1 abilities become available to learn (2 PP each)

2. **Learning Phase**:
   - Player spends PP to learn abilities:
     - Tier 1: 2 PP each (start at Rank 1)
     - Tier 2: 4 PP each (start at Rank 2)
     - Tier 3: 5 PP each (start at Rank 3)
     - Capstone: 6 PP (starts at Rank 3)

3. **First Rank-Up Trigger** (Rank 2):
   - Player learns 2nd Tier 2 ability
   - System detects: "This character now has 2 Tier 2 abilities learned"
   - **Automatic event**: ALL Tier 1 abilities (Rank 1) → Rank 2
   - Free power spike (no PP cost)

4. **Ultimate Rank-Up Trigger** (Rank 3):
   - Player learns Capstone ability
   - System detects: "Capstone acquired"
   - **Automatic event**: ALL Tier 1 (Rank 1 or 2) → Rank 3, ALL Tier 2 (Rank 2) → Rank 3
   - Massive power spike (no PP cost)

**Formula/Logic**:
```
Learning Costs (Tier-based):
  Tier 1: 2 PP → Start at Rank 1
  Tier 2: 4 PP → Start at Rank 2
  Tier 3: 5 PP → Start at Rank 3 (single rank)
  Capstone: 6 PP → Start at Rank 3 (single rank)

Automatic Rank-Up Triggers:
  Trigger 1: Learn 2nd Tier 2 ability
    → ALL Tier 1 abilities: Rank 1 → Rank 2 (free)

  Trigger 2: Learn Capstone
    → ALL Tier 1 abilities: → Rank 3 (free)
    → ALL Tier 2 abilities: Rank 2 → Rank 3 (free)

Rank Scaling (applied automatically):
  Rank 1→2: +1d6 damage / +1 turn duration
  Rank 2→3: +1d6 damage / +1 turn duration
  Total Rank 1→3: +2d6 damage / +2 turns duration

Example (Berserkr Furious Strike):
  Unlock Berserkr: Gain access to Berserkr abilities
  Learn Furious Strike (Tier 1): 2 PP → Starts at Rank 1 (2d6 damage)
  Learn Tier 2 #1: No rank change
  Learn Tier 2 #2: Furious Strike → Rank 2 (3d6 damage) AUTOMATIC
  Learn Capstone: Furious Strike → Rank 3 (4d6 damage) AUTOMATIC

Example (Tier 2 ability - Berserk Rage):
  Learn Berserk Rage (Tier 2): Starts at Rank 2 (3d6 damage)
  Learn Capstone: Berserk Rage → Rank 3 (4d6 damage) AUTOMATIC
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| Tier2TriggerCount | int | 1-3 | 2 | Tier 2 abilities needed for Rank 2 | Yes (balance) |
| DamageScalingPerRank | int | 1-2 | 1 | Dice added per rank | No (consistency) |
| DurationScalingPerRank | int | 1-3 | 1 | Turns added per rank | Yes (per ability) |

**Edge Cases**:
1. **Player skips Tier 1 abilities, rushes to Capstone**:
   - **Condition**: Only 1 Tier 1 ability learned, acquire Capstone
   - **Behavior**: That 1 Tier 1 ability jumps Rank 1 → Rank 3 (skips Rank 2 milestone)
   - **Example**: Extreme build, but mastery still applies to learned abilities

2. **Custom scaling abilities**: Some abilities have non-standard scaling
   - **Condition**: Ability defines custom Rank 2/3 effects (e.g., AoE at Rank 3)
   - **Behavior**: Use custom effects instead of +1d6 formula
   - **Example**: "Intimidate" adds AoE fear at Rank 3 instead of +1d6

3. **Multiple specializations**: Rank-ups are per-spec
   - **Condition**: Player has Berserkr (2 Tier 2) and Skjaldmaer (1 Tier 2)
   - **Behavior**: Only Berserkr Tier 1s are Rank 2; Skjaldmaer Tier 1s stay Rank 1
   - **Example**: Independent progression per specialization

**Related Requirements**: FR-001, FR-002, FR-003, FR-004

---

### Mechanic 2: PP-in-Tree Tracking

**Overview**:
Each specialization tracks total PP spent on abilities within that tree. This value gates access to higher tiers (Tier 2 @ 8 PP, Tier 3 @ 16 PP, Capstone @ 24 PP).

**How It Works**:
1. Player unlocks specialization (3 PP spent, but this goes to unlock cost, not tree)
2. Tier 1 abilities become available to learn (2 PP each)
3. Player learns Tier 1 abilities (2 PP each) → PPInTree increases
4. Player learns Tier 2 ability (4 PP) → PPInTree increases
5. Player learns another Tier 2 (4 PP) → PPInTree increases, may unlock Tier 3
6. Player learns Tier 3 ability (5 PP) → PPInTree increases
7. PPInTree continues accumulating with each ability learned (NO rank-up costs, ranks are free)

**Formula/Logic**:
```
PP_In_Tree = SUM(AbilityData.PPCost for all learned abilities in this specialization)

Note: Automatic rank-ups do NOT add to PPInTree (ranks are free rewards)

Tier Unlock Thresholds:
  Tier 1: 0 PP (available after specialization unlock, costs 2 PP each to learn)
  Tier 2: 8 PP in tree
  Tier 3: 16 PP in tree
  Capstone: 24 PP in tree + both Tier 3 abilities learned

Example (Bone-Setter progression):
  Unlock Bone-Setter: 3 PP (not counted in tree) → PPInTree = 0
  Learn Tier 1 ability #1: 2 PP → PPInTree = 2
  Learn Tier 1 ability #2: 2 PP → PPInTree = 4
  Learn Tier 1 ability #3: 2 PP → PPInTree = 6
  Learn Tier 2 ability #1: 4 PP → PPInTree = 10 (Tier 2 unlocked at 8)
  Learn Tier 2 ability #2: 4 PP → PPInTree = 14 (Tier 1s → Rank 2 AUTOMATIC)
  Learn Tier 2 ability #3: 4 PP → PPInTree = 18 (Tier 3 unlocked at 16)
  Learn Tier 3 ability #1: 5 PP → PPInTree = 23
  Learn Tier 3 ability #2: 5 PP → PPInTree = 28 (Capstone unlocked at 24)
  Learn Capstone: 6 PP → PPInTree = 34 (ALL abilities → Rank 3 AUTOMATIC)
```

**Data Flow**:
```
Input Sources:
  → AbilityData (PPCost for learning)
  → AbilityData (CostToRank2 for ranking)
  → CharacterSpecialization (PPSpentInTree counter)

Processing:
  → On ability learn: PPSpentInTree += AbilityData.PPCost
  → On ability rank-up: PPSpentInTree += AbilityData.CostToRank2
  → On tier validation: Check PPSpentInTree >= RequiredPPInTree

Output Destinations:
  → CharacterSpecialization.PPSpentInTree (database field)
  → UI display (show progress toward next tier)
  → Ability learning validation (gate higher tiers)
```

**Edge Cases**:
1. **Multi-specialization independence**: PP in one tree doesn't count for another
   - **Condition**: Player has Berserkr (10 PP in tree) and Skjaldmaer (5 PP in tree)
   - **Behavior**: PP counts tracked separately; Berserkr at 10, Skjaldmaer at 5
   - **Example**: Tier 2 unlocked in Berserkr, locked in Skjaldmaer

2. **Specialization unlock cost NOT in tree**: 3 PP to unlock doesn't count toward tree
   - **Condition**: Unlock Berserkr (3 PP)
   - **Behavior**: PPInTree = 0 (unlock cost separate from tree progression)
   - **Example**: Player must still earn 8 PP within tree for Tier 2

**Related Requirements**: FR-004

---

### Mechanic 3: Ability Prerequisite Validation

**Overview**:
Some abilities require other abilities to be learned first (prerequisites). Capstone abilities always require both Tier 3 abilities. Prerequisites enforce logical progression through ability trees.

**How It Works**:
1. AbilityData defines Prerequisites.RequiredAbilityIDs (list of ability IDs)
2. When player tries to learn ability, system queries CharacterAbility table
3. For each required ability ID, check if character has learned it
4. If any prerequisite missing, block learning with error message
5. If all prerequisites met + tier unlocked + PP available → allow learning

**Formula/Logic**:
```
CanLearnAbility =
  (PPInTree >= RequiredPPInTree)
  AND (PlayerCharacter.PP >= AbilityData.PPCost)
  AND (ALL abilities in RequiredAbilityIDs are learned)

Example (Bone-Setter Capstone):
  Capstone: "Miracle Worker"
  Prerequisites:
    - PPInTree >= 24
    - RequiredAbilityIDs = [104, 105] (both Tier 3 abilities)

  Check:
    PPInTree = 26? ✓
    Ability 104 learned? ✓
    Ability 105 learned? ✓
    PP >= 6? ✓
  → Can learn Capstone

  If Ability 105 NOT learned:
    → Error "Requires: Masterwork Medicine III (Tier 3)"
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| RequiredAbilityIDs | List<int> | 0-5 IDs | [] | Abilities that must be learned first | Yes (per ability) |
| RequiredPPInTree | int | 0-24 | Varies | PP threshold for tier | No (tier system) |

**Edge Cases**:
1. **Chain prerequisites**: Ability C requires B, which requires A
   - **Condition**: Must learn A → B → C in order
   - **Behavior**: Each step validates previous ability learned
   - **Example**: Advanced abilities build on basic abilities

2. **No prerequisites**: Most abilities have no prerequisites (empty list)
   - **Condition**: RequiredAbilityIDs = []
   - **Behavior**: Only check tier and PP
   - **Example**: Tier 1 and Tier 2 abilities typically have no prerequisites

**Related Requirements**: FR-001, FR-004

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
|-----------|----------|---------------|-----|-----|--------|------------------|
| Tier 1 Ability Cost | AbilityData | 2 PP | 1 | 5 | Tier 1 learning cost | Low |
| Tier 2 Ability Cost | AbilityData | 4 PP | 2 | 8 | Tier 2 learning cost | Low |
| Tier 3 Ability Cost | AbilityData | 5 PP | 3 | 10 | Tier 3 learning cost | Low |
| Capstone Ability Cost | AbilityData | 6 PP | 4 | 12 | Capstone learning cost | Low |
| Rank 2 Trigger Count | Hardcoded | 2 Tier 2s | 1 | 3 | How soon Rank 2 occurs | Medium |
| Tier 2 PP Requirement | SpecializationValidator | 8 PP | 4 | 12 | Tier 2 unlock speed | Low |
| Tier 3 PP Requirement | SpecializationValidator | 16 PP | 10 | 20 | Tier 3 unlock speed | Low |
| Capstone PP Requirement | SpecializationValidator | 24 PP | 18 | 30 | Capstone unlock speed | Low |
| Damage Scaling | Hardcoded formula | +1d6/rank | +1d6 | +2d6 | Power curve steepness | Medium |

### Balance Targets

**Target 1: First Rank 2 Trigger by Milestone 6-8**
- **Metric**: Average Milestone when player learns 2nd Tier 2 ability (Rank 2 trigger)
- **Current**: Milestone 6-8 (unlock spec Milestone 3, earn 8 PP in tree by Milestone 6-8)
- **Target**: Milestone 6-10 (gives flexibility for Tier 1 vs. Tier 2 learning order)
- **Levers**: Tier 2 PP requirement (currently 8 PP), Rank 2 trigger count (currently 2 Tier 2s)

**Target 2: Rank 2 Abilities ~50% Stronger**
- **Metric**: Expected damage increase from Rank 1 → Rank 2 (automatic)
- **Current**: 2d6 (avg 7) → 3d6 (avg 10.5) = +50%
- **Target**: 40-60% increase (noticeable power spike)
- **Levers**: Damage scaling formula (+1d6 per rank)

**Target 3: Complete One Specialization Tree by Milestone 25-30**
- **Metric**: Milestones needed to learn all 9 abilities (total 28 PP)
- **Current**: 28 PP for full tree = ~25-28 Milestones (accounting for starting PP and attribute spending)
- **Target**: Achievable in medium-length campaign (20-40 hours)
- **Levers**: PP per Milestone (currently 1), Tier unlock thresholds, ability costs

**Target 4: Capstone Achievement (Rank 3 trigger) by Milestone 30-35**
- **Metric**: Average Milestone when player learns Capstone (triggers Rank 3 for all abilities)
- **Current**: Requires 24 PP in tree + 6 PP for Capstone = ~30 PP total
- **Target**: Ultimate power moment in late campaign
- **Levers**: Capstone PP requirement (currently 24 PP in tree), Capstone cost (6 PP)

---

## Open Questions & Future Work

### Open Questions

| ID | Question | Priority | Blocking? | Owner | Resolution Date |
|----|----------|----------|-----------|-------|-----------------|
| Q-001 | Should the Rank 2 trigger require 2 or 3 Tier 2 abilities? | Medium | No | Balance | - |
| Q-002 | Should Tier 2 abilities at Rank 3 gain custom effects beyond +1d6? | Low | No | Design | - |
| Q-003 | Should there be a UI celebration/animation when automatic rank-up triggers? | Low | No | UX/UI | - |

### Future Enhancements

**Enhancement 1: Custom Rank 3 Effects for Tier 2 Abilities**
- **Rationale**: Tier 2 abilities currently just gain +1d6 at Rank 3; could gain unique effects instead
- **Complexity**: Medium (requires per-ability design and balance testing)
- **Priority**: Medium (post-core implementation)
- **Dependencies**: Rank 3 triggers implemented
- **Example**: "Berserk Rage" at Rank 3 adds [Vulnerable] status to enemies in addition to damage

**Enhancement 2: Ability Synergy Bonuses**
- **Rationale**: Reward learning multiple related abilities (e.g., all fire abilities grant +10% fire damage)
- **Complexity**: High (requires ability tagging system, effect stacking)
- **Priority**: Low (nice-to-have)
- **Dependencies**: Ability categorization system (Element, School, etc.)

**Enhancement 3: Visual Rank-Up Celebrations**
- **Rationale**: Make automatic rank-ups feel even more rewarding with UI animations
- **Complexity**: Low (UI/UX work)
- **Priority**: Medium (enhances player experience)
- **Dependencies**: UI framework for notifications and animations
- **Example**: Flash effect + sound + "Your abilities have evolved!" message

---

## Appendix

### Appendix A: Rank Scaling Examples

**Damage Ability (Strike - Tier 1)**:
- Rank 1: 2d6 damage, 10 Stamina cost
- Rank 2: 3d6 damage, 10 Stamina cost (+50% damage, same cost) - *Automatic after learning 2 Tier 2 abilities*
- Rank 3: 4d6 damage, 10 Stamina cost (+100% damage, same cost) - *Automatic after learning Capstone*

**Healing Ability (Mend Wound - Tier 1)**:
- Rank 1: Heals 2d6 HP, 8 Stamina cost
- Rank 2: Heals 3d6 HP, 8 Stamina cost - *Automatic*
- Rank 3: Heals 4d6 HP + removes [Bleeding], 8 Stamina cost - *Automatic*

**Buff Ability (Defensive Stance - Tier 1)**:
- Rank 1: +3 Defense for 2 turns, 15 Stamina
- Rank 2: +3 Defense for 3 turns, 15 Stamina - *Automatic*
- Rank 3: +4 Defense for 4 turns, 15 Stamina - *Automatic*

**Status Effect Ability (Intimidate - Tier 1)**:
- Rank 1: Applies [Feared] to 1 enemy for 2 turns
- Rank 2: Applies [Feared] to 1 enemy for 3 turns - *Automatic*
- Rank 3: Applies [Feared] to all enemies for 2 turns (AoE upgrade) - *Automatic*

**Tier 2 Ability (Berserk Rage - starts at Rank 2)**:
- Rank 2: 3d6 damage, costs 15 Stamina (inherently powerful)
- Rank 3: 4d6 damage, costs 15 Stamina - *Automatic after learning Capstone*

### Appendix B: PP Progression Table

**Note**: Ranks advance automatically (free). This table shows learning costs only.

| Milestone | Cumulative PP | Progression Example | Notes |
|-----------|---------------|---------------------|-------|
| 0 (Start) | 2 (starting) | 2 attribute increases OR save for spec | - |
| 3 | 5 | Unlock specialization (3 PP) + 2 attributes | Grants ACCESS to learn Tier 1 abilities |
| 4 | 6 | Learn 1 Tier 1 (2 PP) OR 1 attribute | PPInTree = 2 |
| 5 | 7 | Learn 2 Tier 1s (4 PP) + 1 attribute | PPInTree = 6 |
| 6 | 8 | Learn 1st Tier 2 (4 PP) | 8 PP in tree unlocks Tier 2, PPInTree = 10 |
| 7 | 9 | Save for 2nd Tier 2 | - |
| 8 | 10 | Learn 2nd Tier 2 (4 PP) | **Rank 2 TRIGGER: All Tier 1s → Rank 2** |
| 10 | 12 | Learn 3rd Tier 2 (4 PP) | - |
| 15 | 17 | Learn Tier 3 abilities (5 PP each) | 16 PP in tree unlocks Tier 3 |
| 20 | 22 | Learn both Tier 3s (10 PP) | - |
| 25 | 27 | Learn Capstone (6 PP, need 24 PP in tree) | **Rank 3 TRIGGER: ALL → Rank 3** |
| 30 | 32 | Spec complete, start 2nd spec or attributes | Ultimate mastery achieved |

---

**End of Specification**
