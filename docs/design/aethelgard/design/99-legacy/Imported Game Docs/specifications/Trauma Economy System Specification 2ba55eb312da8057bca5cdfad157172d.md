# Trauma Economy System Specification

Parent item: Specs: Economy (Specs%20Economy%202ba55eb312da8027b1e6d535f27ee714.md)

> Template Version: 1.0
Last Updated: 2025-11-19
Status: Active
Specification ID: SPEC-ECONOMY-003
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2025-11-19 | AI + Human | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [x]  **Review**: Ready for stakeholder review
- [x]  **Approved**: Approved for implementation
- [x]  **Active**: Currently implemented and maintained
- [ ]  **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders

- **Primary Owner**: Game Designer
- **Design**: Risk/reward balance, psychological horror elements
- **Balance**: Stress gain rates, Corruption costs
- **Implementation**: TraumaEconomyService.cs, PlayerCharacter.cs
- **QA/Testing**: Stress accumulation, Breaking Point testing

---

## Executive Summary

### Purpose Statement

The Trauma Economy System tracks the psychological cost of survival through two resources: Psychic Stress (temporary, recoverable) and Corruption (permanent, irreversible), creating strategic tension between power and mental stability.

### Scope

**In Scope**:

- Psychic Stress accumulation and resistance (0-100, recoverable)
- Corruption accumulation (0-100, permanent)
- WILL-based Resolve Checks for stress mitigation
- Breaking Point mechanics (100 Stress → Trauma acquisition)
- Stress recovery at Sanctuary
- Corruption thresholds and consequences
- Terminal Corruption (Game Over at 100)

**Out of Scope**:

- Individual Trauma definitions → `docs/02-statistical-registry/trauma-library.md`
- Heretical ability details → `SPEC-PROGRESSION-003`
- Sanctuary/rest system → `SPEC-WORLD-003`
- Narrative choice system → `SPEC-NARRATIVE-002`

### Success Criteria

- **Player Experience**: Stress creates meaningful tension; Corruption feels weighty and permanent
- **Technical**: Stress/Corruption never exceed 100; Breaking Point triggers reliably
- **Design**: High WILL builds have 20-30% stress reduction advantage
- **Balance**: Average player reaches 1-2 Breaking Points per session (if playing risky)

---

## Related Documentation

### Dependencies

**Depends On**:

- Attribute System: WILL for Resolve Checks → `SPEC-PROGRESSION-001`
- Dice Service: WILL-based stress resistance rolls → `docs/01-systems/combat-resolution.md:45`
- Combat System: Stress sources (Boss, Forlorn) → `SPEC-COMBAT-001`

**Depended Upon By**:

- Heretical Abilities: Corruption/Stress costs → `SPEC-PROGRESSION-003`
- Environmental Hazards: Psychic Resonance damage → `SPEC-WORLD-002`
- Narrative System: Trauma-based dialogue → `SPEC-NARRATIVE-001`
- Faction System: Corruption affects NPC reactions → `SPEC-NARRATIVE-004`

### Related Specifications

- `SPEC-PROGRESSION-001`: Character Progression (WILL attribute)
- `SPEC-COMBAT-001`: Combat Resolution (stress sources)
- `SPEC-WORLD-002`: Environmental Hazards (Psychic Resonance)
- `SPEC-NARRATIVE-001`: Dialogue System (Trauma responses)

### Implementation Documentation

- **System Docs**: `docs/01-systems/stress-corruption.md`
- **System Docs**: `docs/01-systems/traumas.md`
- **Statistical Registry**: `docs/02-statistical-registry/trauma-library.md`
- **Balance Reference**: `docs/05-balance-reference/` (stress gain analysis)

### Code References

- **Primary Service**: `RuneAndRust.Engine/TraumaEconomyService.cs`
- **Core Models**: `RuneAndRust.Core/PlayerCharacter.cs` (Stress, Corruption properties)
- **Trauma Models**: `RuneAndRust.Core/Trauma.cs`
- **Tests**: `RuneAndRust.Tests/TraumaEconomyServiceTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Two-Tier Resource System**
    - **Rationale**: Stress (recoverable) creates short-term tension; Corruption (permanent) creates long-term consequences
    - **Examples**: Can spam heretical abilities for power, but Corruption accumulates permanently
2. **Power at a Cost**
    - **Rationale**: Most powerful abilities require Stress or Corruption payment
    - **Examples**: Psychic Lash deals heavy damage but costs 10 Stress; Corruption Nova is devastating but costs 10 Corruption
3. **WILL Rewards Mental Resilience**
    - **Rationale**: WILL investment provides concrete benefit (stress resistance)
    - **Examples**: WILL 6 reduces stress by ~2 per check (20-30% reduction)

### Player Experience Goals

**Target Experience**: Psychological horror through resource scarcity; every heretical ability use is a calculated risk

**Moment-to-Moment Gameplay**:

- Player sees Stress bar creeping toward 100 (Breaking Point)
- Player weighs: "Use heretical ability for power, or avoid Stress/Corruption?"
- Reaching Breaking Point is traumatic (permanent Trauma acquired)
- Corruption is visible, permanent reminder of sacrifices made

**Learning Curve**:

- **Novice** (0-2 hours): Understand Stress fills, Corruption never decreases
- **Intermediate** (2-10 hours): Manage Stress proactively; avoid Breaking Points
- **Expert** (10+ hours): Calculate exact Corruption budget for campaign; optimize WILL

### Design Constraints

- **Technical**: Stress and Corruption capped at 100 (cannot overflow)
- **Gameplay**: Corruption cannot be removed (no "cure" exists)
- **Narrative**: Trauma reflects psychological realism (not gamey debuffs)
- **Scope**: Terminal Corruption (100) is absolute game over

---

## Functional Requirements

### FR-001: Track Psychic Stress (0-100, Recoverable)

**Priority**: Critical
**Status**: Implemented

**Description**:
System must track player's Psychic Stress from 0-100, allowing accumulation from various sources and recovery at Sanctuary.

**Rationale**:
Stress is the primary mental health resource. Tracking enables risk/reward decisions around stressful actions.

**Acceptance Criteria**:

- [ ]  Stress stored as integer (0-100 range)
- [ ]  Stress can increase from sources (combat, abilities, environment)
- [ ]  Stress can decrease via Sanctuary rest (full recovery to 0)
- [ ]  Stress capped at 100 (cannot exceed)
- [ ]  Reaching 100 triggers Breaking Point (FR-005)
- [ ]  Stress displayed to player (UI bar/number)

**Example Scenarios**:

1. **Scenario**: Player uses Psychic Lash ability (10 Stress)
    - **Input**: Current Stress = 35, use Psychic Lash
    - **Expected Output**: Stress = 45 (35 + 10)
    - **Success Condition**: Stress increased by 10, displayed correctly
2. **Edge Case**: Stress at 95, gain 10 Stress
    - **Input**: Current Stress = 95, gain 10 Stress
    - **Expected Behavior**: Stress = 100, trigger Breaking Point immediately

**Dependencies**:

- Requires: PlayerCharacter model with Stress property
- Blocks: FR-005 (Breaking Point detection requires stress tracking)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Core/PlayerCharacter.cs:PsychicStress`
- **Data Requirements**: Integer property, persistence in save system
- **Performance Considerations**: Instant update

---

### FR-002: Track Corruption (0-100, Permanent)

**Priority**: Critical
**Status**: Implemented

**Description**:
System must track player's Corruption from 0-100, allowing only increases (never decreases), with Terminal Corruption at 100.

**Rationale**:
Corruption represents permanent transformation. Irreversibility creates weight to heretical ability usage.

**Acceptance Criteria**:

- [ ]  Corruption stored as integer (0-100 range)
- [ ]  Corruption can only increase (no decrease mechanism exists)
- [ ]  Corruption capped at 100
- [ ]  Reaching 100 triggers Terminal Corruption (Game Over)
- [ ]  Corruption persisted across saves (permanent)
- [ ]  Corruption displayed to player (UI bar/number)

**Example Scenarios**:

1. **Scenario**: Player uses Void Strike (3 Corruption)
    - **Input**: Current Corruption = 15, use Void Strike
    - **Expected Output**: Corruption = 18 (15 + 3)
    - **Success Condition**: Corruption increased permanently
2. **Edge Case**: Corruption at 98, use Desperate Gambit (5 Corruption)
    - **Input**: Corruption = 98, use Desperate Gambit
    - **Expected Behavior**: Corruption = 100, trigger Terminal Corruption (Game Over)

**Dependencies**:

- Requires: PlayerCharacter model with Corruption property
- Blocks: FR-006 (Corruption thresholds require tracking)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Core/PlayerCharacter.cs:Corruption`
- **Data Requirements**: Integer property, persistence critical
- **Performance Considerations**: Instant update

---

### FR-003: Apply WILL-Based Resolve Checks

**Priority**: High
**Status**: Implemented

**Description**:
When Stress is gained from resistible sources (environmental, combat), player rolls WILL-based Resolve Check to reduce Stress (1 success = -1 Stress).

**Rationale**:
WILL attribute must have meaningful defensive value. Resolve Checks reward WILL investment.

**Acceptance Criteria**:

- [ ]  Resolve Check triggered for resistible Stress sources
- [ ]  Player rolls WILLd6, count successes (5-6)
- [ ]  Each success reduces Stress gain by 1 (minimum 0)
- [ ]  Non-resistible sources (heretical abilities) bypass Resolve Check
- [ ]  Resolve Check result displayed to player
- [ ]  Final Stress gain calculated: `max(0, BaseStress - Successes)`

**Example Scenarios**:

1. **Scenario**: Psychic Resonance zone (8 base Stress), WILL 4
    - **Input**: BaseStress = 8, WILL = 4, roll [5, 6, 3, 2] = 2 successes
    - **Expected Output**: Stress gain = 6 (8 - 2)
    - **Success Condition**: Resolve Check reduced stress by 2
2. **Edge Case**: Resolve Check exceeds base Stress
    - **Input**: BaseStress = 3, WILL = 6, roll 5 successes
    - **Expected Behavior**: Stress gain = 0 (cannot go negative)

**Dependencies**:

- Requires: DiceService for WILL rolls
- Requires: FR-001 (Stress tracking)
- Blocks: FR-004 (stress application uses resolve results)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/TraumaEconomyService.cs:AddStress(allowResolveCheck=true)`
- **Data Requirements**: Player WILL attribute, BaseStress value
- **Performance Considerations**: Instant dice roll + calculation

---

### FR-004: Add Stress from Sources

**Priority**: Critical
**Status**: Implemented

**Description**:
System must apply Stress from various sources (combat, abilities, environment), applying Resolve Checks where appropriate.

**Rationale**:
Stress accumulation from gameplay events drives the trauma economy. Different sources have different resistance rules.

**Acceptance Criteria**:

- [ ]  Combat sources: Boss fights (10-15 Stress, resistible), Forlorn auras (5-10 Stress, resistible)
- [ ]  Environmental sources: Psychic Resonance (8 Stress, resistible), Blight zones (10-20 Stress, resistible)
- [ ]  Ability sources: Psychic Lash (10 Stress, non-resistible), Mass Psychic Lash (20 Stress, non-resistible)
- [ ]  Trauma sources: Passive Stress from Traumas (2-3/turn, non-resistible)
- [ ]  Source logged for tracking/debugging

**Example Scenarios**:

1. **Scenario**: Boss fight Stress (15 base, resistible)
    - **Input**: Engage boss, BaseStress = 15, WILL = 5, roll 2 successes
    - **Expected Output**: Stress += 13 (15 - 2)
    - **Success Condition**: Boss Stress applied with Resolve Check
2. **Edge Case**: Self-inflicted Stress (Psychic Lash)
    - **Input**: Use Psychic Lash, 10 Stress (non-resistible)
    - **Expected Behavior**: Stress += 10 (no Resolve Check, player chose ability)

**Dependencies**:

- Requires: FR-001 (Stress tracking)
- Requires: FR-003 (Resolve Checks)
- Blocks: FR-005 (Stress accumulation triggers Breaking Points)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/TraumaEconomyService.cs:AddStress()`
- **Data Requirements**: Source type, BaseStress, resistible flag
- **Performance Considerations**: Instant per-source application

---

### FR-005: Trigger Breaking Point at 100 Stress

**Priority**: Critical
**Status**: Implemented

**Description**:
When Stress reaches exactly 100, system must trigger Breaking Point: acquire permanent Trauma, reset Stress to 60.

**Rationale**:
Breaking Point is the consequence of poor Stress management. Trauma acquisition creates permanent impact.

**Acceptance Criteria**:

- [ ]  Detect when Stress >= 100
- [ ]  Trigger Breaking Point event
- [ ]  Acquire random Trauma from appropriate category (context-based)
- [ ]  Reset Stress to 60 (not 0 - still rattled)
- [ ]  Display Breaking Point message and Trauma acquired
- [ ]  Trauma immediately applies mechanical effects
- [ ]  Event logged for narrative tracking

**Example Scenarios**:

1. **Scenario**: Stress reaches 100 from combat
    - **Input**: Stress = 97, gain 8 Stress (Resolve failed)
    - **Expected Output**: Stress = 100 → Breaking Point → acquire [HYPERVIGILANCE] Trauma, Stress = 60
    - **Success Condition**: Trauma acquired, Stress reset to 60
2. **Edge Case**: Stress exceeds 100 in single gain
    - **Input**: Stress = 85, use Mass Psychic Lash (+20 Stress)
    - **Expected Behavior**: Stress = 100 (capped), trigger Breaking Point, Stress = 60

**Dependencies**:

- Requires: FR-001 (Stress tracking)
- Requires: FR-004 (Stress sources)
- Requires: Trauma library (random selection)
- Blocks: Trauma effects activation

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/TraumaEconomyService.cs:CheckBreakingPoint()`
- **Data Requirements**: Trauma library, context tags (combat, isolation, etc.)
- **Performance Considerations**: Instant detection + selection

---

### FR-006: Apply Corruption Thresholds

**Priority**: High
**Status**: Implemented

**Description**:
At Corruption thresholds (25, 50, 75, 100), system must trigger narrative/mechanical effects reflecting increasing machine affinity.

**Rationale**:
Corruption milestones create escalating consequences. Visible transformation reinforces permanence.

**Acceptance Criteria**:

- [ ]  Threshold 25: +1 Tech checks, -1 Social checks, display message
- [ ]  Threshold 50: +2 Tech, -2 Social, lose human faction reputation gains
- [ ]  Threshold 75: Acquire [MACHINE AFFINITY] Trauma, NPCs react with fear
- [ ]  Threshold 100: Terminal Corruption (Game Over), character lost to Blight
- [ ]  Thresholds trigger only once (not every time Corruption increases)
- [ ]  Threshold effects displayed clearly to player

**Example Scenarios**:

1. **Scenario**: Corruption reaches 25
    - **Input**: Corruption = 23, use Void Strike (+3)
    - **Expected Output**: Corruption = 26 → Threshold 25 triggered, +1 Tech/-1 Social, message displayed
    - **Success Condition**: Threshold effects applied
2. **Edge Case**: Corruption jumps across threshold (e.g., 23 → 28)
    - **Input**: Corruption = 23, use Desperate Gambit (+5)
    - **Expected Behavior**: Threshold 25 triggered (cross-threshold detection)
3. **Terminal Corruption**: Corruption reaches 100
    - **Input**: Corruption = 97, use Blood Sacrifice (+3)
    - **Expected Behavior**: Corruption = 100, Game Over screen, character lost

**Dependencies**:

- Requires: FR-002 (Corruption tracking)
- Blocks: Narrative responses, Game Over trigger

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/TraumaEconomyService.cs:CheckCorruptionThreshold()`
- **Data Requirements**: Threshold flags (to prevent re-triggering)
- **Performance Considerations**: Instant check after each Corruption gain

---

### FR-007: Recover Stress at Sanctuary

**Priority**: Medium
**Status**: Implemented

**Description**:
When player rests at Sanctuary, Psychic Stress fully recovers to 0. Corruption does NOT recover.

**Rationale**:
Stress recovery creates safe havens. Corruption permanence reinforces its weight.

**Acceptance Criteria**:

- [ ]  Resting at Sanctuary sets Stress = 0
- [ ]  Corruption unchanged (no recovery)
- [ ]  HP and Stamina also restored (integrated rest system)
- [ ]  Recovery message displayed
- [ ]  Trauma effects remain active (permanent)

**Example Scenarios**:

1. **Scenario**: Rest at Sanctuary with 75 Stress
    - **Input**: Stress = 75, Corruption = 30, rest at Sanctuary
    - **Expected Output**: Stress = 0, Corruption = 30 (unchanged), HP/Stamina full
    - **Success Condition**: Stress fully recovered

**Dependencies**:

- Requires: FR-001 (Stress tracking)
- Requires: Sanctuary/rest system (out of scope)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Engine/RestService.cs:RestAtSanctuary()`
- **Data Requirements**: Sanctuary detection
- **Performance Considerations**: Instant recovery

---

## System Mechanics

### Mechanic 1: Stress Accumulation and Resistance

**Overview**:
Stress accumulates from combat, environmental hazards, and ability usage. WILL-based Resolve Checks mitigate resistible Stress sources.

**How It Works**:

1. Stress source identified (combat, environment, ability)
2. If resistible: Roll WILL-based Resolve Check
3. Reduce Stress by successes: `FinalStress = max(0, BaseStress - Successes)`
4. If non-resistible: Apply full BaseStress
5. Add FinalStress to Current Stress (cap at 100)
6. Check for Breaking Point (Stress >= 100)

**Formula/Logic**:

```
Resolve_Check_Reduction:
  Successes = Roll(WILLd6).Count(die >= 5)
  Final_Stress = max(0, Base_Stress - Successes)

Example (Resistible):
  Base_Stress = 10 (Boss fight)
  WILL = 5, Roll = [6, 5, 4, 2, 6] = 3 successes
  Final_Stress = max(0, 10 - 3) = 7
  Current_Stress = 40 + 7 = 47

Example (Non-Resistible):
  Base_Stress = 10 (Psychic Lash ability)
  No Resolve Check (self-inflicted)
  Final_Stress = 10
  Current_Stress = 40 + 10 = 50

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| BaseStress | int | 2-20 | 10 | Stress before resistance | Yes (per source) |
| WILL | int | 1-10 | 3 | Resolve Check dice pool | No (character stat) |
| SuccessThreshold | int | 5-6 | 5 | Die value for success | No (core mechanic) |

**Edge Cases**:

1. **Resolve Check reduces Stress to 0**: Valid, no Stress gained
    - **Condition**: BaseStress = 5, Successes = 6
    - **Behavior**: FinalStress = 0 (cannot go negative)
2. **Stress gain pushes over 100**: Cap at 100, trigger Breaking Point
    - **Condition**: CurrentStress = 95, gain 10
    - **Behavior**: Stress = 100, Breaking Point triggered

**Related Requirements**: FR-003, FR-004

---

### Mechanic 2: Breaking Point and Trauma Acquisition

**Overview**:
Reaching 100 Stress triggers Breaking Point: player acquires permanent Trauma, Stress resets to 60 (not 0).

**How It Works**:

1. Detect Stress >= 100
2. Determine Trauma context (combat, isolation, darkness, etc.)
3. Select random Trauma from appropriate category
4. Add Trauma to PlayerCharacter.Traumas list
5. Apply immediate Trauma effects (mechanical debuffs, passive Stress)
6. Reset Stress to 60 (player still rattled)
7. Display Breaking Point narrative + Trauma details

**Formula/Logic**:

```
Breaking_Point_Check:
  IF Current_Stress >= 100:
    context = DetermineContext(recent_events)
    trauma = SelectRandomTrauma(context)
    player.Traumas.Add(trauma)
    ApplyTraumaEffects(trauma)
    player.PsychicStress = 60
    DisplayBreakingPoint(trauma)

Context Examples:
  - Recent combat → [HYPERVIGILANCE], [PARANOIA]
  - Isolation → [ISOLOPHOBIA]
  - Dark spaces → [NYCTOPHOBIA]
  - Forlorn encounter → [EXISTENTIAL DREAD]

Trauma Effects (examples):
  [HYPERVIGILANCE]: -1 WITS, +3 Stress per combat turn
  [PARANOIA]: Cannot benefit from NPC assistance
  [NYCTOPHOBIA]: +5 Stress in dark rooms

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| BreakingPointThreshold | int | 90-100 | 100 | Stress level triggering trauma | No (core mechanic) |
| StressResetValue | int | 40-80 | 60 | Stress after Breaking Point | Yes (difficulty) |
| TraumaPoolSize | int | 8-20 | 12 | Number of available Traumas | Yes (content expansion) |

**Edge Cases**:

1. **Multiple Breaking Points in quick succession**: Each triggers separately
    - **Condition**: Stress 100 → Trauma → Stress 60 → gain 40 → Stress 100 again
    - **Behavior**: Second Breaking Point triggers, second Trauma acquired
2. **Maximum Traumas limit**: Cap at 5 Traumas (design constraint)
    - **Condition**: Player has 5 Traumas, reaches Breaking Point
    - **Behavior**: Display warning, potentially force Sanctuary or death

**Related Requirements**: FR-005

---

### Mechanic 3: Corruption Accumulation (No Recovery)

**Overview**:
Corruption increases from heretical abilities and Blight encounters. Corruption NEVER decreases. Reaching 100 = Terminal Corruption (Game Over).

**How It Works**:

1. Player uses heretical ability or encounters Blight
2. Corruption cost applied (2-10 Corruption)
3. Add to Current Corruption (cap at 100)
4. Check Corruption thresholds (25, 50, 75, 100)
5. Apply threshold effects if newly crossed
6. Display Corruption gain + current total

**Formula/Logic**:

```
Corruption_Gain:
  Current_Corruption += Corruption_Cost
  Current_Corruption = min(100, Current_Corruption)

  IF Current_Corruption >= 100:
    TriggerTerminalCorruption() // Game Over

Corruption Costs by Ability:
  Void Strike: 3 Corruption
  Blight Surge: 2 Corruption
  Blood Sacrifice: 3 Corruption
  Desperate Gambit: 5 Corruption
  Corruption Nova: 10 Corruption

Thresholds:
  25: +1 Tech, -1 Social
  50: +2 Tech, -2 Social, no human faction rep
  75: Acquire [MACHINE AFFINITY] Trauma
  100: Terminal Corruption (Game Over)

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| AbilityCorruptionCost | int | 2-10 | 3 | Corruption per heretical ability | Yes (per ability) |
| TerminalThreshold | int | 90-100 | 100 | Corruption for Game Over | No (core mechanic) |
| ThresholdIntervals | int | 20-30 | 25 | Corruption between thresholds | No (narrative design) |

**Edge Cases**:

1. **Corruption jumps to exactly 100**: Immediate Terminal Corruption
    - **Condition**: Corruption = 95, use Desperate Gambit (+5)
    - **Behavior**: Corruption = 100, Game Over
2. **Corruption exceeds 100 in single gain**: Cap at 100, still Game Over
    - **Condition**: Corruption = 92, use Corruption Nova (+10)
    - **Behavior**: Corruption = 100 (capped), Terminal Corruption

**Related Requirements**: FR-002, FR-006

---

## State Management

### System State

**State Variables**:

| Variable | Type | Persistence | Default | Description |
| --- | --- | --- | --- | --- |
| PsychicStress | int | Permanent (save) | 0 | Current Stress (0-100) |
| Corruption | int | Permanent (save) | 0 | Current Corruption (0-100) |
| Traumas | List<Trauma> | Permanent (save) | empty | Acquired permanent Traumas |
| CorruptionThresholds | bool[4] | Permanent (save) | all false | Which thresholds crossed (25,50,75,100) |

**State Transitions**:

```
[Safe] (Stress 0-25) ---Stress Sources---> [Strained] (26-50) ---More Stress---> [Severe] (51-75)
[Severe] ---More Stress---> [Critical] (76-99) ---Stress Caps at 100---> [Breaking Point]
[Breaking Point] ---Acquire Trauma---> [Stress Resets to 60]

[No Corruption] (0) ---Heretical Abilities---> [Low Corruption] (1-24)
[Low Corruption] ---More Corruption---> [Threshold 25] (+1 Tech/-1 Social)
[Threshold 25] ---More Corruption---> [Threshold 50] (Lose human rep)
[Threshold 50] ---More Corruption---> [Threshold 75] ([MACHINE AFFINITY] Trauma)
[Threshold 75] ---More Corruption---> [Terminal Corruption] (100, Game Over)

```

### Persistence Requirements

**Must Persist**:

- PsychicStress: Carries across sessions (save/load)
- Corruption: Permanently tracked (no recovery)
- Traumas: Permanent acquisitions (never removed)
- CorruptionThresholds: Track which thresholds crossed (no re-triggering)

**Can Be Transient**:

- Stress gain calculations (computed per-source)
- Resolve Check results (one-time rolls)
- Breaking Point context (determined at time of trigger)

**Save Format**:

- **Database Table**: `saves` table includes `psychic_stress`, `corruption`, `traumas` (JSON array)
- **Versioning**: Trauma list must handle schema updates (new Traumas added in future)

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
| --- | --- | --- | --- | --- | --- | --- |
| Boss Stress | EnemyDatabase | 10-15 | 5 | 20 | Stress gain rate | Medium |
| Psychic Lash Stress | AbilityDatabase | 10 | 5 | 20 | Risk/reward of ability | Medium |
| Corruption Nova Cost | AbilityDatabase | 10 | 5 | 15 | Corruption economy | Low |
| Stress Reset (Breaking) | TraumaEconomyService | 60 | 40 | 80 | Recovery from trauma | Medium |
| WILL Success Threshold | DiceService | 5 | 4 | 6 | Resolve Check effectiveness | Low |

### Balance Targets

**Target 1: 1-2 Breaking Points per Session (Risky Play)**

- **Metric**: Average Breaking Points per 20-minute session
- **Current**: ~1.5 (if using heretical abilities)
- **Target**: 1-2 (creates tension without overwhelming)
- **Levers**: Boss Stress, heretical ability Stress costs

**Target 2: WILL 5-6 Reduces Stress by 20-30%**

- **Metric**: Avg Stress reduction from Resolve Checks
- **Current**: WILL 5 = ~1.67 successes = 20-25% reduction (8 Stress → 6)
- **Target**: 20-30% reduction for high WILL
- **Levers**: Base Stress values, WILL scaling

**Target 3: Corruption Budget of ~30-40 for Full Campaign**

- **Metric**: Total Corruption available before Terminal
- **Current**: 100 Corruption = Game Over
- **Target**: ~30-40 Corruption used in typical playthrough (3-4 heretical abilities)
- **Levers**: Ability Corruption costs, Corruption threshold effects

---

## Open Questions & Future Work

### Future Enhancements

**Enhancement 1: Partial Stress Recovery (Field Medicine)**

- **Rationale**: Allow non-Sanctuary Stress reduction (limited)
- **Complexity**: Low (add consumables that reduce Stress)
- **Priority**: Medium (Quality of Life)
- **Dependencies**: Consumable system expansion

**Enhancement 2: Corruption Slow-Down (Threshold 75+)**

- **Rationale**: Reduce Corruption gain rate after 75 (player already committed)
- **Complexity**: Low (multiply Corruption gains by 0.5)
- **Priority**: Low (narrative vs gameplay trade-off)
- **Dependencies**: Playtest feedback on Corruption economy

---

## Appendix

### Appendix A: Terminology

| Term | Definition |
| --- | --- |
| **Psychic Stress** | Temporary mental strain (0-100, recoverable at Sanctuary) |
| **Corruption** | Permanent transformation by Blight (0-100, irreversible) |
| **Resolve Check** | WILL-based dice roll to resist Stress gain |
| **Breaking Point** | Event at 100 Stress where Trauma is acquired |
| **Trauma** | Permanent psychological debuff from Breaking Point |
| **Terminal Corruption** | Game Over condition at 100 Corruption |
| **Forlorn** | Enemy type emitting Psychic Stress aura |
| **Heretical Ability** | Powerful ability with Stress/Corruption cost |

### Appendix B: Stress Threshold Visual

```
Stress Bar:
  [████████░░] 0-25   (Safe - Green)
  [████████████████░░] 26-50  (Strained - Yellow)
  [████████████████████████░░] 51-75  (Severe - Orange)
  [██████████████████████████████] 76-99  (Critical - Red)
  [██████████████████████████████] 100 (Breaking Point - Flashing Red)
                                  ↓
                            BREAKING POINT
                          Acquire Trauma
                          Stress → 60

```

### Appendix C: Corruption Threshold Effects

| Corruption | Effect | Mechanical Impact | Narrative Impact |
| --- | --- | --- | --- |
| 0-24 | None | - | Human |
| 25 | +1 Tech/-1 Social | Slight machine affinity | Feels machine logic |
| 50 | +2 Tech/-2 Social, No human rep | Cannot gain human faction rep | Prefers machines to humans |
| 75 | [MACHINE AFFINITY] Trauma | Passive Corruption gain, NPCs fear | Humanity questioned |
| 100 | Terminal Corruption (Game Over) | Character lost | No longer human |

---

**End of Specification**