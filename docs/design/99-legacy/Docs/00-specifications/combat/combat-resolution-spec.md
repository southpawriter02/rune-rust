# Combat Resolution System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-19
> **Status**: Active
> **Specification ID**: SPEC-COMBAT-001

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
- **Design**: Turn-based combat flow, initiative system
- **Balance**: Combat pacing, action economy
- **Implementation**: CombatEngine.cs, CombatState.cs
- **QA/Testing**: Combat encounter testing

---

## Executive Summary

### Purpose Statement
The Combat Resolution System provides the core turn-based tactical combat loop where player and enemies take sequential actions based on initiative order until victory, defeat, or escape conditions are met.

### Scope
**In Scope**:
- Initiative system (rolling and ordering)
- Turn sequence management
- Combat state transitions (setup → active → resolution)
- End-of-turn processing
- Environmental hazard integration during combat
- Victory/defeat/flee conditions

**Out of Scope**:
- Damage calculation mechanics → `SPEC-COMBAT-002`
- Status effect mechanics → `SPEC-COMBAT-003`
- Boss encounter special mechanics → `SPEC-COMBAT-004`
- AI decision-making → `SPEC-COMBAT-005`
- Loot generation → `SPEC-ECONOMY-001`
- Combat abilities → `SPEC-PROGRESSION-003`

### Success Criteria
- **Player Experience**: Combat feels orderly, tactical, and responsive
- **Technical**: Initiative deterministic from same inputs; turn order always valid
- **Design**: Clear turn structure with meaningful action choices
- **Balance**: FINESSE attribute significantly influences turn order advantage

---

## Related Documentation

### Dependencies
**Depends On**:
- Dice Pool System: Initiative rolls → `docs/01-systems/combat-resolution.md:45`
- Attribute System: FINESSE attribute → `SPEC-PROGRESSION-001`
- Room System: Environmental hazards → `SPEC-WORLD-002`
- Character System: Player/Enemy models → `SPEC-PROGRESSION-001`

**Depended Upon By**:
- Damage Calculation: Requires turn framework → `SPEC-COMBAT-002`
- Status Effects: Applied during turn processing → `SPEC-COMBAT-003`
- Boss Encounters: Extends combat loop → `SPEC-COMBAT-004`
- Combat AI: Executes during enemy turns → `SPEC-COMBAT-005`

### Related Specifications
- `SPEC-COMBAT-002`: Damage Calculation System
- `SPEC-COMBAT-003`: Status Effects System
- `SPEC-PROGRESSION-001`: Character Attributes & Stats
- `SPEC-WORLD-002`: Room & Environmental Hazards

### Implementation Documentation
- **System Docs**: `docs/01-systems/combat-resolution.md`
- **Statistical Registry**: `docs/02-statistical-registry/` (initiative probability tables)
- **Balance Reference**: `docs/05-balance-reference/` (combat pacing analysis)

### Code References
- **Primary Service**: `RuneAndRust.Engine/CombatEngine.cs:150-240`
- **Core Models**: `RuneAndRust.Core/CombatState.cs`
- **Participant Model**: `RuneAndRust.Core/CombatParticipant.cs`
- **Tests**: `RuneAndRust.Tests/CombatEngineTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Turn Order Clarity**
   - **Rationale**: Players must always understand when they act and when enemies act
   - **Examples**: Initiative displayed clearly; turn order never changes mid-combat (except death)

2. **Tactical Pacing**
   - **Rationale**: Each turn is a meaningful decision point, not a speed contest
   - **Examples**: No real-time pressure; player has full information to make decisions

3. **FINESSE Attribute Rewards Speed**
   - **Rationale**: FINESSE investment should yield consistent first-strike advantage
   - **Examples**: Higher FINESSE = more initiative successes = more likely to act first

### Player Experience Goals
**Target Experience**: Deliberate, tactical decision-making with clear consequences

**Moment-to-Moment Gameplay**:
- Player sees current turn order
- Player knows whose turn it is
- Player selects action from menu
- System resolves action immediately
- Turn advances to next participant

**Learning Curve**:
- **Novice** (0-2 hours): Understand initiative = turn order; basic action selection
- **Intermediate** (2-10 hours): Optimize FINESSE for first-strike; use Defend strategically
- **Expert** (10+ hours): Manipulate turn order via status effects; exploit environmental timing

### Design Constraints
- **Technical**: Must support 1 player vs 1-6 enemies
- **Gameplay**: Combat must be solvable in <50 turns (prevent infinite loops)
- **Narrative**: Combat occurs in rooms with environmental context
- **Scope**: No simultaneous actions; strictly sequential turns

---

## Functional Requirements

> **Completeness Checklist**:
> - [x] All requirements have unique IDs (FR-[NUMBER])
> - [x] All requirements have priority assigned
> - [x] All requirements have acceptance criteria
> - [x] All requirements have at least one example scenario
> - [x] All requirements trace to design goals
> - [x] All requirements are testable

### FR-001: Initialize Combat Encounter
**Priority**: Critical
**Status**: Implemented

**Description**:
System must set up combat state with player, enemies, and room context, then roll initiative to determine turn order.

**Rationale**:
Combat cannot begin without establishing participants and turn order. Initiative roll creates tactical variation.

**Acceptance Criteria**:
- [ ] Combat state created with player and 1-6 enemies
- [ ] Initiative rolled for all participants (player + all enemies)
- [ ] Turn order sorted by initiative score descending, then FINESSE attribute descending
- [ ] Combat state marked as active
- [ ] Environmental hazards identified and announced
- [ ] Initial turn index set to 0

**Example Scenarios**:
1. **Scenario**: Player encounters 3 enemies in Muspelheim Hazard Room
   - **Input**: PlayerCharacter (FINESSE 5), 3 Enemies (FINESSE 3, 4, 6), Room with Fire Hazard
   - **Expected Output**: CombatState with 4 participants in initiative order, IsActive=true, hazard warning displayed
   - **Success Condition**: Initiative order matches dice rolls; hazard announced

2. **Edge Case**: Player encounters boss (CanFlee=false)
   - **Input**: PlayerCharacter, Boss Enemy, CanFlee=false
   - **Expected Behavior**: Combat initializes with CanFlee=false; flee option disabled

**Dependencies**:
- Requires: DiceService for initiative rolls
- Requires: Room model with hazard data
- Blocks: FR-002 (cannot advance turns without initialization)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:InitializeCombat()`
- **Data Requirements**: PlayerCharacter, List<Enemy>, Room (optional), CanFlee (bool)
- **Performance Considerations**: Initialization must complete in <100ms

---

### FR-002: Determine Current Turn Participant
**Priority**: Critical
**Status**: Implemented

**Description**:
System must identify whose turn it is based on current turn index and initiative order.

**Rationale**:
Turn advancement requires knowing who acts next. Clear turn ownership enables proper action resolution.

**Acceptance Criteria**:
- [ ] Current participant retrieved from InitiativeOrder[CurrentTurnIndex]
- [ ] Participant type identified (IsPlayer true/false)
- [ ] Dead participants skipped (if HP ≤ 0, advance to next)
- [ ] Turn index wraps around at end of initiative order (modulo length)

**Example Scenarios**:
1. **Scenario**: Normal turn advancement
   - **Input**: CurrentTurnIndex = 1, InitiativeOrder has 4 participants
   - **Expected Output**: Return InitiativeOrder[1] (Enemy 2)
   - **Success Condition**: Correct participant returned

2. **Edge Case**: Last participant's turn, wraps to first
   - **Input**: CurrentTurnIndex = 3 (last index), InitiativeOrder length = 4
   - **Expected Behavior**: Next turn wraps to index 0 (first participant)

**Dependencies**:
- Requires: FR-001 (combat must be initialized)
- Blocks: FR-003 (cannot process turn without knowing who acts)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:GetCurrentParticipant()`
- **Data Requirements**: CombatState with valid InitiativeOrder
- **Performance Considerations**: O(1) lookup operation

---

### FR-003: Process Player Turn
**Priority**: Critical
**Status**: Implemented

**Description**:
When current participant is player, system must pause for player action input, then resolve selected action.

**Rationale**:
Player agency requires waiting for input. Turn structure ensures player always gets opportunity to act.

**Acceptance Criteria**:
- [ ] System pauses for player input (awaits action selection)
- [ ] Valid actions presented: Attack, Defend, Use Ability, Use Consumable, Flee (if CanFlee)
- [ ] Selected action resolved via appropriate subsystem
- [ ] Combat log updated with action result
- [ ] Turn does not advance until action resolves

**Example Scenarios**:
1. **Scenario**: Player selects Attack action
   - **Input**: Player selects "Attack" targeting Enemy 1
   - **Expected Output**: Attack resolution called, damage applied, combat log updated, turn advances
   - **Success Condition**: Enemy HP reduced, combat log shows attack result

2. **Edge Case**: Player tries to flee boss fight
   - **Input**: Player selects "Flee" in CanFlee=false combat
   - **Expected Behavior**: Flee option not presented/disabled

**Dependencies**:
- Requires: FR-002 (must identify player turn)
- Requires: Action subsystems (damage calc, ability system, etc.)
- Blocks: FR-005 (turn cannot advance until action resolves)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.ConsoleApp/CombatUI.cs` (UI), `RuneAndRust.Engine/CombatEngine.cs` (resolution)
- **Data Requirements**: Player action selection, target selection
- **Performance Considerations**: Human input = no time constraint

---

### FR-004: Process Enemy Turn
**Priority**: Critical
**Status**: Implemented

**Description**:
When current participant is enemy, system must invoke AI to select action, then resolve that action automatically.

**Rationale**:
Enemies must act autonomously. AI decision-making provides tactical challenge.

**Acceptance Criteria**:
- [ ] Enemy AI invoked to select action (via EnemyAI service)
- [ ] AI-selected action resolved immediately
- [ ] Combat log updated with enemy action
- [ ] Turn advances automatically after resolution
- [ ] Stunned enemies skip their turn (AI not invoked)

**Example Scenarios**:
1. **Scenario**: Enemy executes normal attack
   - **Input**: Enemy's turn, AI selects Attack action
   - **Expected Output**: Attack resolves against player, damage applied, turn advances
   - **Success Condition**: Player HP reduced, combat log shows enemy attack

2. **Edge Case**: Enemy is stunned
   - **Input**: Enemy's turn, enemy has Stunned status
   - **Expected Behavior**: Turn skipped, stun duration decremented, next turn

**Dependencies**:
- Requires: FR-002 (must identify enemy turn)
- Requires: EnemyAI service for action selection
- Blocks: FR-005 (turn cannot advance until action resolves)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:ProcessEnemyTurn()`, `RuneAndRust.Engine/EnemyAI.cs`
- **Data Requirements**: Enemy stats, player stats, current combat state
- **Performance Considerations**: AI decision must complete in <500ms

---

### FR-005: Advance Turn
**Priority**: Critical
**Status**: Implemented

**Description**:
After current participant's action resolves, system must apply end-of-turn effects and advance to next participant.

**Rationale**:
Turn sequence drives combat flow. End-of-turn effects (hazards, status duration) maintain gameplay rhythm.

**Acceptance Criteria**:
- [ ] Apply environmental hazard damage (if in hazard room)
- [ ] Apply Forlorn enemy aura stress damage (if applicable)
- [ ] Decrement status effect durations (per-turn and per-round effects)
- [ ] Remove expired status effects
- [ ] Increment CurrentTurnIndex (with wraparound)
- [ ] Check combat end conditions (FR-007)

**Example Scenarios**:
1. **Scenario**: Normal turn advance in hazard room
   - **Input**: Player finishes turn in fire hazard room
   - **Expected Output**: Fire hazard deals 5 HP damage, turn advances to next participant
   - **Success Condition**: Player HP reduced by hazard, CurrentTurnIndex incremented

2. **Edge Case**: Status effect expires during turn advance
   - **Input**: Player has DefenseTurnsRemaining=1, turn ends
   - **Expected Behavior**: DefenseTurnsRemaining decrements to 0, defense bonus removed

**Dependencies**:
- Requires: FR-003 or FR-004 (turn action must complete first)
- Blocks: FR-002 (next turn cannot start without advancing)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:NextTurn()`
- **Data Requirements**: CombatState with current room, participant statuses
- **Performance Considerations**: Must complete in <200ms for responsive gameplay

---

### FR-006: Handle Combat Victory
**Priority**: High
**Status**: Implemented

**Description**:
When all enemies are defeated (HP ≤ 0), system must end combat, award rewards (Legend/XP, loot), and transition to exploration.

**Rationale**:
Victory is primary goal of combat. Rewards incentivize combat engagement.

**Acceptance Criteria**:
- [ ] Detect all enemies have HP ≤ 0
- [ ] Set IsActive = false
- [ ] Award Legend (XP) based on enemy difficulty
- [ ] Generate loot drops (equipment, consumables, currency)
- [ ] Display victory message with rewards
- [ ] Return to exploration mode

**Example Scenarios**:
1. **Scenario**: Player defeats last enemy
   - **Input**: Player attack reduces last enemy to HP = 0
   - **Expected Output**: Combat ends, "Victory!" message, XP awarded, loot displayed
   - **Success Condition**: IsActive=false, player Legend increased, loot in inventory

2. **Edge Case**: Multiple enemies die simultaneously (AoE ability)
   - **Input**: All 3 remaining enemies reduced to HP ≤ 0 by AoE attack
   - **Expected Behavior**: Single victory resolution, XP/loot for all enemies

**Dependencies**:
- Requires: SagaService for XP awards
- Requires: LootService for drop generation
- Requires: FR-001 (combat must have started)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:IsCombatOver()`, `AwardCombatLegend()`
- **Data Requirements**: Enemy difficulty values, loot tables
- **Performance Considerations**: Loot generation should complete in <500ms

---

### FR-007: Handle Combat Defeat
**Priority**: High
**Status**: Implemented

**Description**:
When player HP ≤ 0, system must end combat immediately and trigger defeat/death handling.

**Rationale**:
Player death is lose condition. Immediate feedback prevents confusion.

**Acceptance Criteria**:
- [ ] Detect player HP ≤ 0
- [ ] Set IsActive = false immediately (no further turns)
- [ ] Display defeat message
- [ ] Trigger death/respawn system
- [ ] No XP or loot awarded

**Example Scenarios**:
1. **Scenario**: Player HP reduced to 0 by enemy attack
   - **Input**: Enemy attack reduces player HP from 8 to 0
   - **Expected Output**: Combat ends immediately, "Defeated!" message
   - **Success Condition**: IsActive=false, death system triggered

2. **Edge Case**: Player dies from environmental hazard
   - **Input**: Fire hazard reduces player HP from 3 to 0 during end-of-turn
   - **Expected Behavior**: Combat ends before next turn, defeat triggered

**Dependencies**:
- Requires: Death/respawn system (out of scope for this spec)
- Requires: FR-001 (combat must have started)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:IsCombatOver()`
- **Data Requirements**: Player HP status
- **Performance Considerations**: Immediate detection (<50ms)

---

### FR-008: Handle Flee Attempt
**Priority**: Medium
**Status**: Implemented

**Description**:
If CanFlee=true, player can attempt to flee combat via opposed roll. Success exits combat without rewards; failure wastes turn.

**Rationale**:
Flee option provides escape valve for unwinnable fights. Risk/reward balance prevents abuse.

**Acceptance Criteria**:
- [ ] Flee action only available when CanFlee=true
- [ ] Flee attempt uses opposed roll (player FINESSE vs average enemy FINESSE)
- [ ] Success: Combat ends, no XP/loot, return to exploration
- [ ] Failure: Turn wasted, enemies act next
- [ ] Boss fights (CanFlee=false) never allow fleeing

**Example Scenarios**:
1. **Scenario**: Successful flee attempt
   - **Input**: Player selects Flee, rolls 4 successes vs enemy average 2 successes
   - **Expected Output**: "You flee successfully!", combat ends, no rewards
   - **Success Condition**: IsActive=false, player returns to room exploration

2. **Edge Case**: Failed flee attempt in dangerous situation
   - **Input**: Player at low HP, flee fails (1 success vs 3 successes)
   - **Expected Behavior**: "Flee failed!", turn ends, enemies attack next

**Dependencies**:
- Requires: DiceService for flee roll
- Requires: FR-003 (flee is player action)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:AttemptFlee()`
- **Data Requirements**: Player FINESSE, enemy FINESSE values, CanFlee flag
- **Performance Considerations**: Instant resolution

---

## System Mechanics

### Mechanic 1: Initiative Rolling

**Overview**:
At combat start, all participants roll initiative using their FINESSE attribute as dice pool. Results determine turn order for entire combat.

**How It Works**:
1. Each participant rolls XdY where X = FINESSE attribute
2. Count successes (5-6 on d6)
3. Sort participants by successes descending
4. Ties broken by FINESSE attribute value (higher wins)
5. Player wins ties if FINESSE equal
6. Initiative order fixed for combat duration (barring deaths)

**Formula/Logic**:
```
InitiativeScore = Roll(FINESSE_d6).Count(die >= 5)

TurnOrder = Participants
  .OrderByDescending(p => p.InitiativeScore)
  .ThenByDescending(p => p.FINESSE)
  .ThenBy(p => p.IsPlayer ? 0 : 1)  // Player wins final ties

Example:
  Player (FINESSE 5): Rolls [6, 4, 5, 2, 6] = 3 successes
  Enemy A (FINESSE 3): Rolls [5, 1, 6] = 2 successes
  Enemy B (FINESSE 6): Rolls [5, 4, 6, 3, 5, 2] = 3 successes

  Turn Order:
    1. Enemy B (3 successes, FINESSE 6) ← Wins tie via FINESSE
    2. Player (3 successes, FINESSE 5)
    3. Enemy A (2 successes, FINESSE 3)
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| FINESSE | int | 1-10 | 3 | Dice pool size | No (character stat) |
| SuccessThreshold | int | 5-6 | 5 | Minimum die value for success | No |

**Edge Cases**:
1. **All participants tie on initiative**: Resolved by FINESSE, then player priority
   - **Condition**: Same success count, same FINESSE
   - **Behavior**: Player always goes first
   - **Example**: Player and Enemy both roll 2 successes with FINESSE 4 → Player first

2. **Participant dies mid-combat**: Dead participants skipped in turn order, not removed
   - **Condition**: Participant HP ≤ 0
   - **Behavior**: Turn skipped, index advances
   - **Example**: InitiativeOrder[2] is dead → skip to InitiativeOrder[3]

**Related Requirements**: FR-001

---

### Mechanic 2: Turn Cycling

**Overview**:
Combat proceeds in rounds. Each round cycles through all participants in initiative order. Turn index wraps around after last participant.

**How It Works**:
1. CurrentTurnIndex starts at 0
2. Process current participant's turn
3. Apply end-of-turn effects
4. Increment CurrentTurnIndex
5. If index >= InitiativeOrder.Count, wrap to 0 (new round)
6. Repeat until combat ends

**Formula/Logic**:
```
NextTurnIndex = (CurrentTurnIndex + 1) % InitiativeOrder.Count

Example (4 participants):
  Round 1: Index 0, 1, 2, 3
  Round 2: Index 0 (wraps), 1, 2, 3
  Round 3: Index 0, 1, 2, 3
  ...
```

**Data Flow**:
```
Input Sources:
  → CombatState.CurrentTurnIndex
  → CombatState.InitiativeOrder (length)

Processing:
  → Retrieve current participant
  → Process turn (player input OR AI action)
  → Apply end-of-turn effects
  → Increment index with wraparound

Output Destinations:
  → CombatState.CurrentTurnIndex (updated)
  → Combat log (action results)
  → Participant HP/status updates
```

**Edge Cases**:
1. **Combat ends mid-round**: Turn cycling stops immediately when victory/defeat detected
   - **Condition**: Player or all enemies HP ≤ 0
   - **Behavior**: IsActive set false, cycling terminates
   - **Example**: Enemy 2 kills player → Combat ends, Enemy 3 never gets turn

2. **Infinite combat prevention**: Combat should not exceed 100 turns (implementation safeguard)
   - **Condition**: CurrentTurnIndex wraps 25+ times (4 participants × 25 rounds)
   - **Behavior**: Force flee or draw outcome (edge case handling)
   - **Example**: Player and enemy both unkillable → force resolution

**Related Requirements**: FR-002, FR-005

---

### Mechanic 3: End-of-Turn Processing

**Overview**:
After each participant's action, system applies persistent effects: environmental hazards, status durations, aura effects.

**How It Works**:
1. Apply environmental hazard damage (if room has hazards)
2. Apply Forlorn enemy aura (psychic stress per turn)
3. Apply environmental psychic resonance (biome-based stress)
4. Decrement status effect durations:
   - Per-turn effects (defense bonus, stun, etc.)
   - Per-round effects (bleeding, inspired, etc.)
5. Remove expired effects (duration = 0)
6. Tick performance durations (Skald abilities)

**Formula/Logic**:
```
EndOfTurnProcessing:
  1. IF room.HasHazard:
       ApplyHazardDamage(currentParticipant)

  2. FOR each forlornEnemy IN enemies WHERE isForlorn:
       ApplyPsychicStressAura(player, stressAmount)

  3. FOR each statusEffect IN currentParticipant.StatusEffects:
       statusEffect.Duration -= 1
       IF statusEffect.Duration <= 0:
          RemoveStatusEffect(statusEffect)

Example:
  Player finishes turn in Fire Hazard room with Defense active (1 turn remaining)
  → Fire deals 5 HP damage
  → DefenseTurnsRemaining: 1 → 0 (removed)
  → Advance to next turn
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| HazardDamage | int | 2-15 | 5 | HP damage per turn in hazard | Yes (per room) |
| ForlornStress | int | 3-8 | 5 | Stress per turn from Forlorn aura | Yes (per enemy) |
| StatusDuration | int | 1-5 | 3 | Turns/rounds until expiration | Yes (per effect) |

**Edge Cases**:
1. **Hazard kills participant**: Detected during end-of-turn, triggers combat end check
   - **Condition**: Hazard damage reduces HP to ≤ 0
   - **Behavior**: Mark participant dead, check victory/defeat
   - **Example**: Player at 3 HP, hazard deals 5 → Player dies, defeat triggered

2. **Multiple effects expire simultaneously**: All processed in single pass
   - **Condition**: Multiple status durations reach 0 same turn
   - **Behavior**: All removed together, single combat log entry
   - **Example**: Defense (1 turn) and Inspired (1 turn) both expire → both removed

**Related Requirements**: FR-005

---

## State Management

### System State

**State Variables**:
| Variable | Type | Persistence | Default | Description |
|----------|------|-------------|---------|-------------|
| IsActive | bool | Session | false | Whether combat is ongoing |
| CurrentTurnIndex | int | Session | 0 | Index in InitiativeOrder for current turn |
| InitiativeOrder | List | Session | empty | Sorted participants by initiative |
| CanFlee | bool | Session | true | Whether flee action is available |
| CombatLog | List<string> | Session | empty | History of combat actions |

**State Transitions**:
```
[Not In Combat] ---InitializeCombat()---> [Active Combat]

[Active Combat] ---All Enemies Dead---> [Victory] ---> [Not In Combat]
[Active Combat] ---Player Dead---> [Defeat] ---> [Not In Combat]
[Active Combat] ---Successful Flee---> [Fled] ---> [Not In Combat]
```

**State Diagram**:
```
┌──────────────┐
│ Not In Combat│ <──────────────┐
└──────┬───────┘                │
       │ InitializeCombat()     │
       ▼                        │
┌──────────────┐                │
│Active Combat │                │
│(Turn Loop)   │                │
└──┬───┬───┬───┘                │
   │   │   │                    │
   │   │   └─Flee Success───────┤
   │   │                        │
   │   └─All Enemies Dead───────┤
   │     (Victory)              │
   │                            │
   └─Player Dead────────────────┘
     (Defeat)
```

### Persistence Requirements

**Must Persist**:
- Player HP/Stamina: Persisted after combat ends (save system)
- Legend/XP gains: Persisted via SagaService
- Loot acquired: Persisted via InventoryService
- Trauma/Corruption changes: Persisted via TraumaEconomyService

**Can Be Transient**:
- CombatState: Session-only, recreated each combat
- InitiativeOrder: Recalculated each combat
- CombatLog: Display-only, not persisted
- Temporary combat buffs: Expire at combat end

**Save Format**:
- No database persistence for combat state itself
- Player state persisted via existing `saves` table
- Combat outcomes trigger saves in dependent systems (SagaService, InventoryService)

---

## Integration Points

### Systems This System Consumes

#### Integration with DiceService
**What We Use**: Random dice rolling for initiative
**How We Use It**: `Roll(int diceCount)` returns success count
**Dependency Type**: Hard (combat cannot function without RNG)
**Failure Handling**: Critical error if DiceService unavailable

**API/Interface**:
```csharp
var initiativeRoll = _diceService.Roll(participant.Attributes.Finesse);
// Returns: DiceRollResult { Successes = int }
```

---

#### Integration with Room/Hazard System
**What We Use**: Environmental hazard definitions and damage values
**How We Use It**: Query current room for active hazards, apply damage each turn
**Dependency Type**: Soft (combat works without hazards, just no environmental damage)
**Failure Handling**: If room is null, skip hazard processing

**API/Interface**:
```csharp
if (currentRoom?.HasHazard == true)
{
    var damage = _hazardService.ProcessAutomaticHazard(currentRoom, participant);
}
```

---

#### Integration with SagaService (XP/Legend)
**What We Use**: Legend award calculation and application
**How We Use It**: On victory, call `AwardLegend(player, amount)`
**Dependency Type**: Hard for victory condition
**Failure Handling**: Log error, allow combat to end (XP not awarded)

**API/Interface**:
```csharp
var legendEarned = CalculateLegendReward(enemies);
_sagaService.AwardLegend(player, legendEarned);
```

---

### Systems That Consume This System

#### Consumed By Damage Calculation System
**What They Use**: Turn structure and combat state
**How They Use It**: Damage calculation occurs during player/enemy action resolution within turns
**Stability Contract**: CombatState remains valid throughout combat; participants always accessible

---

#### Consumed By Status Effects System
**What They Use**: End-of-turn processing hooks
**How They Use It**: Status effects applied/removed during FR-005 end-of-turn phase
**Stability Contract**: End-of-turn processing always occurs before turn advancement

---

#### Consumed By EnemyAI System
**What They Use**: Current combat state for AI decision-making
**How They Use It**: AI evaluates CombatState to choose actions during enemy turns (FR-004)
**Stability Contract**: CombatState fully populated with current participant HP, statuses, positions

---

### Event System Integration

**Events Published**:
| Event Name | Trigger | Payload | Consumers |
|------------|---------|---------|-----------|
| OnCombatStart | InitializeCombat() | CombatState | UI (display combat screen) |
| OnTurnStart | Each turn begins | CurrentParticipant | UI (highlight current actor) |
| OnCombatEnd | Victory/Defeat/Flee | CombatResult | UI, SagaService, LootService |
| OnTurnAdvance | End of turn processing | TurnNumber | Status effect system |

**Events Subscribed**:
| Event Name | Source | Handler | Purpose |
|------------|--------|---------|---------|
| OnPlayerDeath | PlayerCharacter | EndCombat() | Trigger defeat immediately |
| OnEnemyDeath | Enemy | CheckVictory() | Check if all enemies dead |

---

## User Experience Flow

### Primary User Flow: Normal Combat Encounter

```
1. Player Action: Enters room with enemies
   └─> System Response: InitializeCombat() called
       └─> Feedback: "Combat begins! Rolling initiative..."

2. System: Rolls initiative for all participants
   └─> System Response: Sorts initiative order
       └─> Feedback: "Turn order: Enemy B, Player, Enemy A"

3. System: First turn (Enemy B's turn per initiative)
   └─> System Response: EnemyAI selects action
       └─> Feedback: "Enemy B attacks! Deals 8 damage."

4. System: Player's turn
   └─> System Response: Displays action menu
       └─> Feedback: "Your turn. [Attack] [Defend] [Ability] [Item] [Flee]"

5. Player Action: Selects "Attack" targeting Enemy A
   └─> System Response: Resolves attack (dice rolls, damage calc)
       └─> Feedback: "You attack Enemy A for 12 damage! (HP: 18/30)"

6. System: End of player turn
   └─> System Response: Applies fire hazard (5 damage)
       └─> Feedback: "Fire hazard deals 5 damage! (HP: 45/50)"

7. System: Next turn (Enemy A)
   └─> Loop continues until victory/defeat/flee
```

### Alternative Flow: Successful Flee

```
1. Player Action: Selects "Flee" during player turn
   └─> System Response: Rolls opposed flee check
       └─> Feedback: "Attempting to flee... (rolling FINESSE)"

2. System: Player wins opposed roll (4 successes vs 2)
   └─> System Response: Sets IsActive=false, returns to exploration
       └─> Feedback: "You flee successfully! No rewards gained."

3. Resolution: Player back in room exploration mode
   └─> Feedback: Room description displayed
```

### Error Flow: Invalid Action During Enemy Turn

```
1. Error Condition: Player tries to act during enemy turn
   └─> System Response: Action rejected
       └─> Feedback: "It's not your turn! Wait for Enemy B to finish."

2. Recovery: Wait for player's turn in initiative order
   └─> System Response: Presents action menu when player's turn arrives
```

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
|-----------|----------|---------------|-----|-----|--------|------------------|
| Success Threshold | DiceService | 5 | 4 | 6 | Initiative variance | Low (core mechanic) |
| Max Flee Attempts | CombatEngine | Unlimited | 1 | 5 | Flee viability | Medium |
| Turn Limit | CombatEngine | 100 | 50 | 200 | Prevents infinite combat | Low |

### Balance Targets

**Target 1: FINESSE Advantage**
- **Metric**: Win rate for acting first
- **Current**: FINESSE 6 acts before FINESSE 3 ~65% of time
- **Target**: 60-70% advantage for +3 FINESSE delta
- **Levers**: Success threshold, tie-breaking rules

**Target 2: Combat Duration**
- **Metric**: Average turns to victory
- **Current**: 8-15 turns for balanced encounter
- **Target**: 5-20 turns (prevent too fast or too slow)
- **Levers**: Enemy HP, damage values, hazard damage

---

## Implementation Guidance

### Implementation Status

**Current State**: Complete

**Completed**:
- [x] Core models created (CombatState, CombatParticipant)
- [x] Service/Engine implemented (CombatEngine.cs)
- [x] Initiative rolling and sorting
- [x] Turn cycling logic
- [x] End-of-turn processing
- [x] Victory/defeat/flee conditions
- [x] Environmental hazard integration
- [x] Unit tests written
- [x] Integration tests written
- [x] Balance testing completed
- [x] Documentation updated

---

## Open Questions & Future Work

### Future Enhancements

**Enhancement 1: Initiative Reroll Abilities**
- **Rationale**: Add tactical depth by allowing initiative manipulation mid-combat
- **Complexity**: Medium (requires new ability type)
- **Priority**: Low (core loop is solid)
- **Dependencies**: Ability system must support "reroll initiative" effect type

**Enhancement 2: Simultaneous Turn Resolution**
- **Rationale**: Speed-tied participants act simultaneously (more dynamic)
- **Complexity**: High (major refactor of turn structure)
- **Priority**: Low (current sequential model works well)
- **Dependencies**: Complete overhaul of turn advancement logic

---

## Appendix

### Appendix A: Terminology

| Term | Definition |
|------|------------|
| **Initiative** | The turn order determined by dice rolls at combat start |
| **Success** | A die result of 5 or 6 on a d6 |
| **Net Successes** | Attacker successes minus defender successes in opposed roll |
| **Participant** | Any combatant (player or enemy) in the encounter |
| **Round** | One complete cycle through all participants in initiative order |
| **Turn** | One participant's action within a round |
| **Forlorn** | Enemy type that emits psychic stress aura each turn |

### Appendix B: Initiative Probability Table

**Probability of Acting First (vs opponent with FINESSE 3)**:

| Your FINESSE | P(Act First) | Expected Initiative | Notes |
|--------------|--------------|---------------------|-------|
| 1 | 20% | 0.33 | Severe disadvantage |
| 2 | 35% | 0.67 | Disadvantage |
| 3 | 50% | 1.00 | Even match (tie-break rules apply) |
| 4 | 62% | 1.33 | Slight advantage |
| 5 | 71% | 1.67 | Moderate advantage |
| 6 | 78% | 2.00 | Strong advantage |
| 7 | 84% | 2.33 | Very strong advantage |
| 8 | 88% | 2.67 | Dominant |
| 9 | 91% | 3.00 | Overwhelming |
| 10 | 93% | 3.33 | Nearly guaranteed |

---

**End of Specification**
