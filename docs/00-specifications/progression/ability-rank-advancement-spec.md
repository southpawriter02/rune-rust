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
The Ability Rank Advancement System provides incremental power scaling for abilities where players invest Progression Points (PP) to upgrade abilities from Rank 1 → Rank 2 → Rank 3, creating meaningful build investment and long-term character development.

### Scope
**In Scope**:
- Ability learning mechanics within specialization trees
- Rank progression system (Rank 1, Rank 2, Rank 3)
- Rank scaling formulas (damage, healing, duration increases)
- Rank-up PP costs (typically 5 PP for Rank 1→2)
- Validation rules for ability learning and ranking
- Integration with specialization tier unlock system

**Out of Scope**:
- Specialization tree structure → `SPEC-PROGRESSION-002`
- Ability reset/respec mechanics → `SPEC-PROGRESSION-006` (future)
- Individual ability design → Implementation documentation
- Per-specialization balance tuning → Per-spec documents

### Success Criteria
- **Player Experience**: Rank-ups feel impactful; players can see clear power increases
- **Technical**: Rank scaling formulas apply correctly; PP costs deduct properly
- **Design**: Rank progression pacing aligns with Milestone earning rate
- **Balance**: Rank 3 abilities feel powerful but not mandatory for viability

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

1. **Meaningful Investment**
   - **Rationale**: Ranking abilities requires scarce PP; players must choose which abilities to invest in
   - **Examples**: 5 PP for Rank 2 = 5 Milestones of earning; can't rank everything

2. **Clear Power Scaling**
   - **Rationale**: Rank increases must provide visible, measurable improvements
   - **Examples**: Rank 2 deals +50% damage (2d6 → 3d6); Rank 3 adds status effects

3. **Incremental Progression**
   - **Rationale**: 3-rank system provides smooth progression curve, not binary on/off
   - **Examples**: Rank 1 (basic), Rank 2 (improved), Rank 3 (mastery/capstone effects)

### Player Experience Goals
**Target Experience**: "I invested PP into this ability and it became noticeably stronger"

**Moment-to-Moment Gameplay**:
- Player browses abilities in specialization tree
- Player learns ability at Rank 1 (3-4 PP cost)
- Player uses ability in combat, sees Rank 1 power
- After several Milestones, player ranks up ability to Rank 2 (5 PP cost)
- Player uses ability, notices clear damage/effect increase
- Eventually, player reaches Rank 3 (ultimate version)

**Learning Curve**:
- **Novice** (0-2 hours): Learn Tier 1 abilities at Rank 1, understand basic mechanics
- **Intermediate** (2-10 hours): Start ranking up key abilities to Rank 2
- **Expert** (10+ hours): Achieve Rank 3 on signature abilities, optimize PP spending

### Design Constraints
- **Technical**: Must support 3 ranks for all rankable abilities
- **Gameplay**: Rank 3 locked until future update (CostToRank3 = 0)
- **Narrative**: Rank names could use Aethelgard terminology (future flavor)
- **Scope**: v0.19 supports Rank 1→2 advancement only; Rank 3 future expansion

---

## Functional Requirements

### FR-001: Learn Ability at Rank 1
**Priority**: Critical
**Status**: Implemented

**Description**:
Players can learn abilities from unlocked specializations by spending PP. All abilities start at Rank 1. Learning requires: specialization unlocked, tier unlocked (PP in tree threshold), sufficient PP, and prerequisites met.

**Rationale**:
Learning is the entry point to ability progression. Rank 1 provides baseline functionality; players rank up later for power.

**Acceptance Criteria**:
- [ ] Player can browse abilities in unlocked specializations
- [ ] Abilities display PP cost (varies by tier: Tier 1=0 PP, Tier 2=4 PP, Tier 3=5 PP, Capstone=6 PP)
- [ ] Learning deducts PP from PlayerCharacter.ProgressionPoints
- [ ] Ability added to character at Rank 1 (CurrentRank = 1)
- [ ] Learned abilities tracked in CharacterAbility table
- [ ] PP spent on ability added to specialization's PPSpentInTree counter
- [ ] Error handling: insufficient PP, unmet prerequisites, tier locked

**Example Scenarios**:
1. **Scenario**: Warrior learns "Berserk Rage I" (Berserkr Tier 2 ability)
   - **Input**: Warrior has Berserkr unlocked, 8 PP in Berserkr tree, 4 PP available
   - **Expected Output**: Berserk Rage added at Rank 1, PP = 0, PPInTree = 12
   - **Success Condition**: Ability usable in combat immediately

2. **Edge Case**: Player tries to learn Tier 3 ability with only 10 PP in tree (requires 16)
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

### FR-002: Rank Up Ability (Rank 1 → Rank 2)
**Priority**: High
**Status**: Implemented

**Description**:
Players can advance learned abilities from Rank 1 to Rank 2 by spending additional PP (typically 5 PP). Ranking up improves ability power: increased damage dice, improved healing, longer durations, or added effects.

**Rationale**:
Rank progression creates long-term investment in signature abilities. 5 PP cost is significant (5 Milestones), making rank-ups meaningful choices.

**Acceptance Criteria**:
- [ ] Player can rank up any learned ability (CurrentRank = 1)
- [ ] Rank 2 costs defined per ability (AbilityData.CostToRank2, typically 5 PP)
- [ ] Ranking up deducts PP from PlayerCharacter.ProgressionPoints
- [ ] CharacterAbility.CurrentRank incremented (1 → 2)
- [ ] Rank 2 scaling applied automatically (damage dice +50%, healing +50%, duration +1-2 turns)
- [ ] Rank-up PP cost added to specialization's PPSpentInTree
- [ ] Cannot rank up abilities already at Rank 2 (until Rank 3 unlocked)

**Example Scenarios**:
1. **Scenario**: Warrior ranks up "Strike" from Rank 1 → Rank 2
   - **Input**: Warrior has Strike at Rank 1 (2d6 damage), 5 PP available
   - **Expected Output**: Strike now Rank 2 (3d6 damage, +50% dice), PP = 0
   - **Success Condition**: Next use of Strike deals 3d6 damage instead of 2d6

2. **Scenario**: Mystic ranks up "Aether Dart" healing ability
   - **Input**: Aether Dart Rank 1 (heals 2d6), 5 PP available
   - **Expected Output**: Aether Dart Rank 2 (heals 3d6)
   - **Success Condition**: Healing increased

3. **Edge Case**: Player tries to rank up with insufficient PP
   - **Input**: Player has 3 PP, tries to rank up (costs 5 PP)
   - **Expected Behavior**: Error "Requires 5 PP (you have 3)"
   - **Success Condition**: Rank-up blocked, PP not deducted

**Dependencies**:
- Requires: FR-001 (ability must be learned first)
- Requires: PP available → `SPEC-PROGRESSION-001:FR-002`
- Blocks: Rank 3 advancement (future)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/AbilityService.cs:RankUpAbility()`
- **Data Requirements**: AbilityData.CostToRank2, CharacterAbility.CurrentRank
- **Performance Considerations**: Database update on rank-up

---

### FR-003: Rank Scaling Formulas
**Priority**: High
**Status**: Implemented

**Description**:
Abilities scale in power when ranked up according to standardized formulas. Damage abilities gain +50% dice per rank (2d6 → 3d6 → 4d6). Healing abilities gain +50% dice. Duration-based effects gain +1-2 turns per rank. Status effects may gain additional effects at higher ranks.

**Rationale**:
Consistent scaling ensures all abilities benefit meaningfully from ranking. +50% damage is noticeable but not overwhelming.

**Acceptance Criteria**:
- [ ] **Damage scaling**: Rank 1 = Nd6, Rank 2 = (N+1)d6, Rank 3 = (N+2)d6 (+50% per rank)
- [ ] **Healing scaling**: Same as damage (+1d6 per rank)
- [ ] **Duration scaling**: Rank 1 = D turns, Rank 2 = D+1 turns, Rank 3 = D+2 turns
- [ ] **Status effect scaling**: Rank 2 may add secondary effect, Rank 3 adds tertiary or AoE
- [ ] Scaling applied automatically based on CurrentRank
- [ ] Custom scaling supported per ability (some abilities scale differently)

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

### FR-004: Ability Tier Unlock Validation
**Priority**: High
**Status**: Implemented

**Description**:
Abilities are organized into tiers (1/2/3/Capstone) within specialization trees. Higher tiers require progressively more PP invested in the specialization tree. Tier 2 requires 8 PP in tree, Tier 3 requires 16 PP, Capstone requires 24 PP.

**Rationale**:
Tier gating ensures players progress through specialization gradually. Can't skip straight to Capstone; must invest in lower tiers first.

**Acceptance Criteria**:
- [ ] Tier 1 abilities: 0 PP in tree required (granted free on specialization unlock)
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

### Mechanic 1: Rank Progression Flow

**Overview**:
Ability ranking is a multi-step investment: learn at Rank 1 (tier-dependent PP cost), use ability in combat to evaluate power, invest 5 PP to rank up to Rank 2, ability becomes measurably stronger.

**How It Works**:
1. Player unlocks specialization (3 PP, grants 3 free Tier 1 abilities)
2. Player browses ability tree, selects ability to learn
3. System validates: tier unlocked? prerequisites met? PP available?
4. Player spends PP, ability added at Rank 1
5. Player uses ability in combat (Rank 1 power)
6. After earning more PP, player chooses to rank up ability
7. Player spends 5 PP (typical), ability advances to Rank 2
8. Scaling formula applies (+1d6 damage, +1 turn duration, etc.)
9. Player uses ability, sees improved power

**Formula/Logic**:
```
Learning Cost = AbilityData.PPCost (varies by tier)
  Tier 1: 0 PP (free with specialization unlock)
  Tier 2: 4 PP
  Tier 3: 5 PP
  Capstone: 6 PP

Rank-Up Cost = AbilityData.CostToRank2
  Typically: 5 PP (Rank 1 → Rank 2)
  Future: AbilityData.CostToRank3 (currently 0 = locked)

Damage Scaling:
  Rank 1: Base dice (e.g., 2d6)
  Rank 2: Base + 1d6 (e.g., 3d6) = +50% expected damage
  Rank 3: Base + 2d6 (e.g., 4d6) = +100% expected damage (future)

Example (Strike ability):
  Learn Strike (Tier 1): 0 PP (free with Warrior archetype)
  Strike Rank 1: 2d6 damage (expected: 7 damage)
  Rank up to Rank 2: 5 PP
  Strike Rank 2: 3d6 damage (expected: 10.5 damage, +50%)
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| CostToRank2 | int | 3-10 | 5 | PP cost Rank 1→2 | Yes (per ability) |
| CostToRank3 | int | 5-15 | 0 | PP cost Rank 2→3 (locked) | Yes (future) |
| DamageScalingPerRank | int | 1-2 | 1 | Dice added per rank | No (consistency) |
| DurationScalingPerRank | int | 1-3 | 1 | Turns added per rank | Yes (per ability) |

**Edge Cases**:
1. **Ability already at max rank**: Cannot rank up further
   - **Condition**: CurrentRank = 2, CostToRank3 = 0 (locked)
   - **Behavior**: Error "Ability is at maximum rank (2)"
   - **Example**: Rank 3 not yet implemented in v0.19

2. **Custom scaling abilities**: Some abilities have non-standard scaling
   - **Condition**: Ability defines custom Rank 2/3 effects
   - **Behavior**: Use custom effects instead of formula
   - **Example**: Capstone abilities may have unique Rank 2 effects

**Related Requirements**: FR-001, FR-002, FR-003

---

### Mechanic 2: PP-in-Tree Tracking

**Overview**:
Each specialization tracks total PP spent on abilities within that tree. This value gates access to higher tiers (Tier 2 @ 8 PP, Tier 3 @ 16 PP, Capstone @ 24 PP).

**How It Works**:
1. Player unlocks specialization (3 PP spent, but this goes to unlock, not tree)
2. Tier 1 abilities granted free (0 PP added to tree)
3. Player learns Tier 2 ability (4 PP) → PPInTree = 4
4. Player learns another Tier 2 (4 PP) → PPInTree = 8 (Tier 2 now fully unlocked)
5. Player ranks up one ability (5 PP) → PPInTree = 13
6. Player learns Tier 3 ability (5 PP) → PPInTree = 18 (Tier 3 unlocked at 16)
7. PPInTree continues accumulating with each ability learned or ranked

**Formula/Logic**:
```
PP_In_Tree = SUM(
  AbilityData.PPCost for all learned abilities in this specialization
  + SUM(RankUpCost for all ranked abilities in this specialization)
)

Tier Unlock Thresholds:
  Tier 1: 0 PP (always available after specialization unlock)
  Tier 2: 8 PP in tree
  Tier 3: 16 PP in tree
  Capstone: 24 PP in tree + both Tier 3 abilities learned

Example (Bone-Setter progression):
  Unlock Bone-Setter: 3 PP (not counted in tree)
  Tier 1 (3 abilities): 0 PP (granted free) → PPInTree = 0
  Learn Tier 2 ability #1: 4 PP → PPInTree = 4
  Learn Tier 2 ability #2: 4 PP → PPInTree = 8 (Tier 2 fully unlocked)
  Rank up Tier 1 ability: 5 PP → PPInTree = 13
  Learn Tier 3 ability #1: 5 PP → PPInTree = 18 (Tier 3 unlocked)
  Learn Tier 3 ability #2: 5 PP → PPInTree = 23
  Learn Capstone: 6 PP → PPInTree = 29 (tree complete)
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
| CostToRank2 | AbilityData | 5 PP | 3 | 10 | Rank progression pacing | Medium |
| CostToRank3 | AbilityData | 0 (locked) | 5 | 15 | Future rank 3 gating | Low (future) |
| Tier 2 PP Requirement | SpecializationValidator | 8 PP | 4 | 12 | Tier 2 unlock speed | Low |
| Tier 3 PP Requirement | SpecializationValidator | 16 PP | 10 | 20 | Tier 3 unlock speed | Low |
| Capstone PP Requirement | SpecializationValidator | 24 PP | 18 | 30 | Capstone unlock speed | Low |
| Damage Scaling | Hardcoded formula | +1d6/rank | +1d6 | +2d6 | Power curve steepness | Medium |

### Balance Targets

**Target 1: First Rank 2 by Milestone 8-10**
- **Metric**: Average Milestone when player first ranks up an ability
- **Current**: Milestone 8-9 (5 PP earned from Milestones 3-8)
- **Target**: Milestone 8-12 (gives flexibility for attribute investment)
- **Levers**: CostToRank2 (currently 5 PP)

**Target 2: Rank 2 Abilities ~50% Stronger**
- **Metric**: Expected damage increase from Rank 1 → Rank 2
- **Current**: 2d6 (avg 7) → 3d6 (avg 10.5) = +50%
- **Target**: 40-60% increase (noticeable, not overwhelming)
- **Levers**: Damage scaling formula (+1d6 per rank)

**Target 3: Complete One Specialization by Milestone 25-30**
- **Metric**: Milestones needed to learn all 9 abilities + rank some to Rank 2
- **Current**: 28 PP for full tree + ranking = ~30 Milestones
- **Target**: Achievable in medium-length campaign (20-40 hours)
- **Levers**: PP per Milestone (currently 1), Tier unlock thresholds

---

## Open Questions & Future Work

### Open Questions

| ID | Question | Priority | Blocking? | Owner | Resolution Date |
|----|----------|----------|-----------|-------|-----------------|
| Q-001 | Should Rank 3 be implemented in v0.20 or delayed further? | Medium | No | Design | - |
| Q-002 | Should some abilities have variable rank costs (not all 5 PP)? | Low | No | Balance | - |
| Q-003 | Should capstone abilities have higher rank-up costs? | Low | No | Balance | - |

### Future Enhancements

**Enhancement 1: Rank 3 Implementation**
- **Rationale**: Completes 3-rank progression; provides ultimate power level
- **Complexity**: Medium (requires balance testing, new effects)
- **Priority**: Medium (post-v0.20)
- **Dependencies**: Milestone cap increase (currently 3, need 15+ for Rank 3 progression)

**Enhancement 2: Ability Synergy Bonuses**
- **Rationale**: Reward learning multiple related abilities (e.g., all fire abilities grant +10% fire damage)
- **Complexity**: High (requires ability tagging system, effect stacking)
- **Priority**: Low (nice-to-have)
- **Dependencies**: Ability categorization system (Element, School, etc.)

**Enhancement 3: Prestige Abilities**
- **Rationale**: Ultra-rare abilities unlocked via quests or achievements
- **Complexity**: Medium (requires unlock condition system)
- **Priority**: Low (post-launch content)
- **Dependencies**: Quest system, achievement system

---

## Appendix

### Appendix A: Rank Scaling Examples

**Damage Ability (Strike)**:
- Rank 1: 2d6 damage, 10 Stamina cost
- Rank 2: 3d6 damage, 10 Stamina cost (+50% damage, same cost)
- Rank 3: 4d6 damage, 10 Stamina cost (+100% damage) [FUTURE]

**Healing Ability (Mend Wound)**:
- Rank 1: Heals 2d6 HP, 8 Stamina cost
- Rank 2: Heals 3d6 HP, 8 Stamina cost
- Rank 3: Heals 4d6 HP + removes [Bleeding], 8 Stamina cost [FUTURE]

**Buff Ability (Defensive Stance)**:
- Rank 1: +3 Defense for 2 turns, 15 Stamina
- Rank 2: +3 Defense for 3 turns, 15 Stamina
- Rank 3: +4 Defense for 4 turns, 15 Stamina [FUTURE]

**Status Effect Ability (Intimidate)**:
- Rank 1: Applies [Feared] to 1 enemy for 2 turns
- Rank 2: Applies [Feared] to 1 enemy for 3 turns
- Rank 3: Applies [Feared] to all enemies for 2 turns (AoE upgrade) [FUTURE]

### Appendix B: PP Progression Table

| Milestone | Cumulative PP | Can Afford |
|-----------|---------------|------------|
| 0 (Start) | 2 (starting) | 2 attribute increases |
| 1 | 3 | Unlock specialization OR 3 attributes |
| 2 | 4 | 4 attributes OR unlock spec + 1 attribute |
| 3 | 5 | Unlock spec + 2 attributes OR first Rank 2 |
| 5 | 7 | Unlock spec + Rank 2 + 1 attribute |
| 8 | 10 | Unlock spec + 2 Rank 2s OR spec + 7 attributes |
| 10 | 12 | Unlock spec + 2 Rank 2s + 2 attributes |
| 15 | 17 | Unlock spec + 3 Rank 2s + learn Tier 3 abilities |
| 20 | 22 | Near-complete one specialization tree |
| 30 | 32 | Complete one spec + start second OR multi-spec build |

---

**End of Specification**
