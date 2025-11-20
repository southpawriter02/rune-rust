# Accuracy & Evasion System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-20
> **Status**: Draft
> **Specification ID**: SPEC-COMBAT-004

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-20 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [ ] **Review**: Ready for stakeholder review
- [ ] **Approved**: Approved for implementation
- [ ] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders
- **Primary Owner**: Combat Designer
- **Design**: Hit/miss determination, opposed roll balance
- **Balance**: Hit chance targets, accuracy bonus economy, STURDINESS scaling
- **Implementation**: CombatEngine.cs, DiceService.cs
- **QA/Testing**: Probability verification, edge case validation

---

## Executive Summary

### Purpose Statement
The Accuracy & Evasion System determines whether attacks hit or miss using opposed dice pool rolls. Attackers roll dice based on weapon attributes (MIGHT/FINESSE/WILL) plus accuracy bonuses, defenders roll STURDINESS dice, and the net successes determine hit success. This creates tactical uncertainty while rewarding attribute investment and equipment upgrades.

### Scope
**In Scope**:
- Opposed roll mechanics (attack roll vs defense roll)
- Attack dice pool calculation (base attribute + accuracy bonuses)
- Defense dice pool calculation (STURDINESS attribute)
- Net success determination (attack successes - defense successes)
- Hit/miss resolution (net > 0 = hit, ≤ 0 = miss)
- Accuracy bonus sources (equipment, abilities, status effects)
- Tie-breaking rule (defender wins ties)
- Combat log integration for roll display
- Probability balancing and hit chance targets

**Out of Scope**:
- Damage calculation after hit lands → `SPEC-COMBAT-002`
- Status effect application mechanics → `SPEC-COMBAT-003`
- Critical hit damage multipliers → Future enhancement
- Flanking position calculation → `SPEC-COMBAT-008`
- Environmental accuracy modifiers → `SPEC-WORLD-003`
- Dodge/parry active abilities → Individual ability specs
- Cover and concealment → Future feature

### Success Criteria
- **Player Experience**: Hits feel earned not guaranteed; misses feel explainable; tactical choices (accuracy bonuses, STURDINESS) matter
- **Technical**: Roll calculations deterministic; dice pool size limits enforced; probability distributions verified
- **Design**: Balanced builds (offense vs defense) have ~50-60% hit chance; glass cannons hit ~80%; tanks hit ~30%
- **Balance**: Average 30-40% miss rate prevents "rocket tag"; STURDINESS investment provides measurable survivability improvement

---

## Related Documentation

### Dependencies
**Depends On**:
- Combat Resolution System: Turn sequence and combat flow → `SPEC-COMBAT-001`
- Dice Pool System: d6 dice rolling, success counting (5-6 = success) → `docs/01-systems/combat-resolution.md`
- Attribute System: MIGHT, FINESSE, WILL, STURDINESS → `SPEC-PROGRESSION-001`
- Equipment System: Weapon attributes and accuracy bonuses → `SPEC-ECONOMY-001` (planned)
- Status Effect System: [Analyzed] and other accuracy modifiers → `SPEC-COMBAT-003`

**Depended Upon By**:
- Damage Calculation System: Requires hit determination before damage → `SPEC-COMBAT-002`
- Status Effect Application: Hit must land before applying effects → `SPEC-COMBAT-003`
- Combat AI: Enemy AI uses hit chance to make decisions → Enemy AI System (future)
- Ability Execution: Abilities check hit/miss before applying effects → `SPEC-PROGRESSION-003`

### Related Specifications
- `SPEC-COMBAT-001`: Combat Resolution System (turn sequence, combat state)
- `SPEC-COMBAT-002`: Damage Calculation System (post-hit damage)
- `SPEC-COMBAT-003`: Status Effects System (accuracy modifiers)
- `SPEC-PROGRESSION-001`: Character Progression (attribute values)
- `SPEC-ECONOMY-001`: Loot & Equipment System (accuracy bonuses) - Planned

### Implementation Documentation
- **Layer 1 Docs**: `docs/01-systems/accuracy-evasion.md` - Comprehensive implementation reference with formulas, examples, probability tables
- **Statistical Registry**: `docs/02-statistical-registry/` - Hit chance tables, dice pool statistics
- **Code Reference**: `RuneAndRust.Engine/CombatEngine.cs` (lines 130-251, 420-480)

### Code References
- **Primary Service**: `RuneAndRust.Engine/CombatEngine.cs`
  - `PlayerAttack()` - Attack roll calculation and execution
  - `EnemyAttack()` - Enemy attack roll processing
  - `CalculateNetSuccesses()` - Hit/miss determination
- **Dice Service**: `RuneAndRust.Engine/DiceService.cs`
  - `Roll(int diceCount)` - Core d6 dice pool rolling
- **Equipment Service**: `RuneAndRust.Engine/EquipmentService.cs`
  - `GetEffectiveAttributeValue()` - Attribute value with equipment bonuses
- **Core Models**: `RuneAndRust.Core/Attributes.cs`, `RuneAndRust.Core/DiceResult.cs`
- **Tests**: `RuneAndRust.Tests/CombatEngineTests.cs`, `DiceServiceTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Meaningful Uncertainty**
   - **Rationale**: Dice-based opposed rolls create tactical uncertainty; no attack is guaranteed, no defense is perfect
   - **Examples**:
     - Glass cannon (10 MIGHT) vs tank (10 STURDINESS): ~50% hit chance - uncertain outcome creates tension
     - Balanced attacker (6 FINESSE) vs balanced defender (5 STURDINESS): ~60% hit - favorable but not guaranteed
     - Weak attacker (3 MIGHT) vs strong defender (8 STURDINESS): ~15% hit - long odds but still possible
   - **Trade-offs**: High variance at low dice pools (1-3 dice) creates frustration; mitigated by starting attributes at 3-4 minimum

2. **Investment Rewards Consistency**
   - **Rationale**: Higher offensive attributes (MIGHT/FINESSE/WILL) increase average successes, making hits more reliable; higher STURDINESS reduces enemy hit chance consistently
   - **Examples**:
     - Investing 1 PP in MIGHT: +1 attack die = +0.33 avg successes = ~+10-15% hit chance
     - Investing 1 PP in STURDINESS: +1 defense die = -0.33 enemy successes = ~-10-15% enemy hit chance
     - Extreme investment (attribute 10 vs 1): ~85-95% hit chance vs ~15% hit chance
   - **Trade-offs**: Linear scaling (1 die = +0.33 successes) means diminishing returns at very high attribute values; mitigated by attribute cap at 10

3. **Opposed Rolls Over Static Targets**
   - **Rationale**: Defender rolls actively resist attacks, making defensive investment meaningful; creates build diversity (offense-focused vs defense-focused)
   - **Examples**:
     - Static target system: All enemies have fixed Defense 15; STURDINESS adds +1 to static defense (boring, linear)
     - Opposed roll system: Tank with STURDINESS 10 rolls 10d6 defense; varies 2-5 successes, feels dynamic
   - **Trade-offs**: Two rolls per attack (slower) vs one roll (faster); mitigated by fast dice service implementation

4. **Tactical Bonus Stacking**
   - **Rationale**: Equipment, abilities, and status effects grant accuracy bonuses that stack additively; rewards tactical preparation and combo play
   - **Examples**:
     - Equipment +2 accuracy + [Analyzed] status +2 + Battle Rage +2 = +6 total dice (huge advantage)
     - Single bonus (+2) is noticeable (~+0.67 successes = +20% hit chance)
     - Stacking multiple bonuses creates "power spike" moments (total +6 = ~+2 successes = +60% hit chance)
   - **Trade-offs**: Uncapped stacking creates degenerate combos; addressed by bonus cap proposals (max +5 or +7)

### Player Experience Goals
**Target Experience**: "My attacks sometimes miss, but I can improve my hit chance through smart equipment choices, status effects, and attribute investment - the system feels fair and responsive to my decisions."

**Moment-to-Moment Gameplay**:
- **Attack Declared**: Player chooses attack action
- **Bonuses Displayed**: UI shows "Base 6d6 + Equipment +2 + [Analyzed] +2 = 10d6 attack roll"
- **Dice Rolled**: Visual feedback shows dice results [6, 5, 4, 6, 2, 5, 3, 1, 6, 4] = 5 successes
- **Defense Rolled**: Enemy STURDINESS 7 rolls [3, 5, 2, 6, 4, 1, 5] = 3 successes
- **Net Calculated**: 5 - 3 = 2 net successes → HIT lands
- **Outcome**: Player sees "Your attack hits! (2 net successes)" and proceeds to damage
- **Feedback Loop**: Player learns that accuracy bonuses significantly increased hit chance; encourages tactical play

**Emotional Beats**:
- **Tension**: "Will this attack hit the boss?" (dice roll creates suspense)
- **Relief**: "Yes! My accuracy bonuses paid off!" (hit lands)
- **Frustration**: "Ugh, I missed even with bonuses..." (miss on 50% chance feels bad but acceptable)
- **Mastery**: "I stacked +6 accuracy bonuses - this is almost guaranteed" (player feels smart)
- **Despair**: "The tank keeps deflecting everything!" (high STURDINESS creates challenge)

**Learning Curve**:
- **Novice** (0-2 hours): Understand basic mechanic - attack roll vs defense roll, net successes determine hit
- **Intermediate** (2-10 hours): Learn to recognize accuracy bonuses, prioritize equipment with +accuracy, use [Analyzed] status tactically
- **Expert** (10+ hours): Master bonus stacking, calculate probabilities mentally, balance offense vs defense attributes

### Design Constraints
- **Technical**:
  - Dice pool size capped at 20 dice (performance limit)
  - Dice rolls must be deterministic from same seed (for testing/replay)
  - Net success calculation must handle negative results (0 net = miss, not error)
- **Gameplay**:
  - Success rate per die fixed at 33% (5-6 on d6) for probability consistency
  - Defender always wins ties (design philosophy: slight defensive advantage)
  - Minimum 1 die for any roll (0-attribute characters impossible in normal play)
- **Narrative**:
  - Opposed rolls represent active defense (parry, dodge) not passive armor absorption
  - High STURDINESS represents combat awareness and reflexes, not just bulk
  - Net successes represent degree of success (barely hit vs overwhelming hit)
- **Scope**:
  - Current version: No critical hit system (net successes don't multiply damage)
  - Future version: May add critical hits at net 4+ successes, glancing blows at net 0

---

## Functional Requirements

### FR-001: Calculate Attack Dice Pool
**Priority**: Critical
**Status**: Implemented

**Description**:
When an attack is declared, the system calculates the total attack dice pool by summing the base weapon attribute (MIGHT, FINESSE, or WILL) plus all applicable accuracy bonuses from equipment, abilities, status effects, and temporary combat state. The dice pool represents the attacker's offensive capability for this specific attack.

**Rationale**:
Accurate dice pool calculation is foundational to the entire accuracy system. All tactical decisions (equipment choices, status effect application, ability usage) depend on bonuses properly adding to the attack roll. Incorrect calculation breaks game balance and player trust.

**Acceptance Criteria**:
- [ ] Base attribute determined by equipped weapon's WeaponAttribute property ("MIGHT", "FINESSE", or "WILL")
- [ ] Equipment accuracy bonus added (0-3 dice from weapon/gear)
- [ ] Ability bonus dice added (temporary from abilities like Exploit Weakness)
- [ ] Status effect bonuses added:
  - [ ] [Analyzed] status on target: +2 dice
  - [ ] Battle Rage active on attacker: +2 dice
  - [ ] Saga of Courage performance active: +2 dice
- [ ] All bonuses stack additively (no multiplicative stacking)
- [ ] Total dice pool capped at maximum 20 dice (performance limit)
- [ ] Minimum 1 die enforced (characters always have at least 1 in attack attribute)
- [ ] Combat log displays dice pool breakdown: "Base 6d6 + Equipment +2 + [Analyzed] +2 = 10d6"

**Example Scenarios**:
1. **Scenario**: Warrior attacks with MIGHT weapon, no bonuses
   - **Input**: MIGHT = 6, no equipment bonuses, no status effects
   - **Expected Output**: Attack roll = 6d6
   - **Success Condition**: Dice pool exactly equals base attribute

2. **Scenario**: Rogue attacks with FINESSE weapon and +2 accuracy dagger against [Analyzed] target
   - **Input**: FINESSE = 7, weapon +2 accuracy, target has [Analyzed] status
   - **Expected Output**: Attack roll = 7 + 2 + 2 = 11d6
   - **Success Condition**: All bonuses correctly added, log shows breakdown

3. **Edge Case**: Glass cannon with MIGHT 10 + all bonuses active (equipment +3, Battle Rage +2, [Analyzed] +2, ability +3)
   - **Input**: Total bonuses = +10, base MIGHT 10
   - **Expected Behavior**: Attack roll capped at 20d6 (not 20+)
   - **Success Condition**: Dice pool never exceeds cap, log warns "Attack dice capped at 20"

**Dependencies**:
- Requires: Equipment System for weapon attribute and accuracy bonus → `SPEC-ECONOMY-001`
- Requires: Status Effect System for [Analyzed] and Battle Rage → `SPEC-COMBAT-003`
- Requires: Attribute System for MIGHT/FINESSE/WILL values → `SPEC-PROGRESSION-001`
- Blocks: FR-003 (net success calculation needs attack roll first)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:PlayerAttack()` lines 230-260
- **Data Requirements**: PlayerCharacter.Attributes, Equipment.AccuracyBonus, Enemy.AnalyzedTurnsRemaining
- **Performance Considerations**: Simple arithmetic, no performance concerns

---

### FR-002: Calculate Defense Dice Pool
**Priority**: Critical
**Status**: Implemented

**Description**:
When an attack targets a character, the defender's dice pool is calculated as their STURDINESS attribute value. Defense dice represent the character's ability to parry, dodge, or otherwise avoid incoming attacks through reflexes and combat awareness.

**Rationale**:
Defense must be simple and predictable - STURDINESS directly determines defense dice. This creates clear player understanding: "If I invest 1 PP in STURDINESS, I get +1 defense die against all attacks." Complexity belongs in attack rolls (bonuses), not defense (straightforward).

**Acceptance Criteria**:
- [ ] Defense dice = defender's STURDINESS attribute (no modifications)
- [ ] STURDINESS range: 1-10 (corresponds to 1d6-10d6 defense)
- [ ] No status effects modify defense dice pool (status effects modify attack pool or damage, not defense)
- [ ] No equipment modifies defense dice pool (equipment may grant STURDINESS bonus, which then affects dice)
- [ ] Combat log displays defense roll: "Enemy defends! Rolled 7d6: [results] = 3 successes"
- [ ] Edge case: STURDINESS 0 (impossible in normal play) = 0 dice = always hit

**Example Scenarios**:
1. **Scenario**: Balanced character with STURDINESS 5 defends
   - **Input**: STURDINESS = 5
   - **Expected Output**: Defense roll = 5d6
   - **Success Condition**: Dice pool exactly equals STURDINESS

2. **Scenario**: Tank with STURDINESS 10 (max) defends
   - **Input**: STURDINESS = 10
   - **Expected Output**: Defense roll = 10d6
   - **Success Condition**: Maximum defense pool achievable

3. **Edge Case**: Hypothetical character with STURDINESS 0 (debug/testing)
   - **Input**: STURDINESS = 0
   - **Expected Behavior**: Defense roll = 0d6 = 0 successes (always hit)
   - **Success Condition**: No crash/error, attack automatically succeeds

**Dependencies**:
- Requires: Attribute System for STURDINESS values → `SPEC-PROGRESSION-001`
- Blocks: FR-003 (net success calculation needs defense roll)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:PlayerAttack()` lines 265-268
- **Data Requirements**: Defender.Attributes.Sturdiness
- **Performance Considerations**: Single attribute read, no performance concerns

---

### FR-003: Determine Hit/Miss via Net Successes
**Priority**: Critical
**Status**: Implemented

**Description**:
After both attack and defense rolls complete, the system calculates net successes (attack successes minus defense successes). If net successes > 0, the attack hits and proceeds to damage calculation. If net successes ≤ 0 (including ties), the attack misses and no damage is dealt.

**Rationale**:
Net success determination is the core decision point of the accuracy system. The "defender wins ties" rule creates a slight defensive bias, preventing "rocket tag" combat where both sides always hit. This encourages tactical play and defensive investment.

**Acceptance Criteria**:
- [ ] Net successes = attack roll successes - defense roll successes
- [ ] Net > 0: Attack hits, proceed to damage calculation
- [ ] Net = 0: Attack misses (tie goes to defender)
- [ ] Net < 0: Attack misses (defense overwhelmed offense)
- [ ] Combat log displays: "Net successes: 3 - 2 = 1 (HIT!)" or "Net successes: 2 - 3 = -1 (MISS)"
- [ ] Hit message: "Your attack lands! (X net successes)"
- [ ] Miss message: "The attack is deflected!" or "Enemy dodges your strike!"
- [ ] Net success value passed to damage system for future use (currently unused, may affect damage in future)

**Example Scenarios**:
1. **Scenario**: Attack succeeds with positive net successes
   - **Input**: Attack roll = 5 successes, defense roll = 2 successes
   - **Expected Output**: Net = 5 - 2 = 3 → HIT, proceed to damage
   - **Success Condition**: Damage calculation begins, log shows "Your attack lands! (3 net successes)"

2. **Scenario**: Tie - equal successes
   - **Input**: Attack roll = 4 successes, defense roll = 4 successes
   - **Expected Output**: Net = 4 - 4 = 0 → MISS (defender wins ties)
   - **Success Condition**: No damage dealt, log shows "The attack is deflected!"

3. **Scenario**: Attack fails - defense overwhelms
   - **Input**: Attack roll = 2 successes, defense roll = 5 successes
   - **Expected Output**: Net = 2 - 5 = -3 → MISS
   - **Success Condition**: No damage dealt, log shows enemy successfully defended

4. **Edge Case**: Overwhelming attack (10 attack vs 0 defense)
   - **Input**: Attack = 10 successes, defense = 0 successes
   - **Expected Behavior**: Net = 10 → HIT with high margin
   - **Success Condition**: Hit confirmed, high net success value preserved for future damage scaling

**Dependencies**:
- Requires: FR-001 (attack roll) and FR-002 (defense roll)
- Blocks: Damage Calculation System → `SPEC-COMBAT-002`

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:PlayerAttack()` lines 270-283
- **Data Requirements**: DiceResult.Successes from attack and defense rolls
- **Performance Considerations**: Simple subtraction and comparison, no concerns

---

### FR-004: Aggregate Accuracy Bonuses from Multiple Sources
**Priority**: High
**Status**: Implemented

**Description**:
The system identifies and sums all active accuracy bonuses from diverse sources (equipment, abilities, status effects, performances, temporary combat state) before calculating the attack dice pool. Bonuses stack additively without restrictions, though future versions may implement caps.

**Rationale**:
Additive stacking rewards tactical preparation and creates exciting "combo" moments when players stack multiple bonuses. Transparent aggregation (showing each bonus source in log) helps players learn the system and make informed decisions about equipment and ability usage.

**Acceptance Criteria**:
- [ ] Equipment accuracy bonus queried from PlayerCharacter.EquippedWeapon.AccuracyBonus
- [ ] Ability bonus dice queried from CombatState.PlayerNextAttackBonusDice
- [ ] [Analyzed] status checked on target (Enemy.AnalyzedTurnsRemaining > 0 → +2 dice)
- [ ] Battle Rage checked on attacker (PlayerCharacter.BattleRageTurnsRemaining > 0 → +2 dice)
- [ ] Saga of Courage performance checked (PlayerCharacter.IsPerforming && CurrentPerformance == "Saga of Courage" → +2 dice)
- [ ] All bonuses summed additively (no multiplication)
- [ ] Bonus breakdown displayed in combat log before roll:
  ```
  Calculating attack bonuses:
    Base MIGHT: 6d6
    Equipment: +2
    [Analyzed]: +2
    Battle Rage: +2
    Total: 12d6
  ```
- [ ] Bonus sources persist for duration (equipment always active, status effects have turn counters)
- [ ] Temporary bonuses consumed after use (ability bonus dice reset to 0 after attack)

**Example Scenarios**:
1. **Scenario**: Stacking multiple bonuses for critical attack
   - **Input**: Equipment +3, [Analyzed] +2, Battle Rage +2, ability bonus +3
   - **Expected Output**: Total bonus = +10 added to base attribute
   - **Success Condition**: All bonuses correctly summed, log shows each source

2. **Scenario**: Partial bonuses (only equipment active)
   - **Input**: Equipment +2, no status effects, no ability bonuses
   - **Expected Output**: Total bonus = +2
   - **Success Condition**: Only active bonuses counted

3. **Scenario**: [Analyzed] status expires mid-combat
   - **Input**: Turn 1: [Analyzed] active (+2), Turn 3: [Analyzed] expired (duration 2 turns)
   - **Expected Behavior**: Turn 1 attack gets +2, Turn 3 attack gets +0 from [Analyzed]
   - **Success Condition**: Bonus correctly appears/disappears based on status duration

4. **Edge Case**: Ability bonus consumed after single attack
   - **Input**: Ability grants +5 bonus dice for next attack
   - **Expected Behavior**: First attack after ability uses +5, second attack has +0 ability bonus
   - **Success Condition**: Temporary bonus resets to 0 after consumption

**Dependencies**:
- Requires: Equipment System → `SPEC-ECONOMY-001`
- Requires: Status Effect System → `SPEC-COMBAT-003`
- Requires: Ability System → `SPEC-PROGRESSION-003`
- Blocks: FR-001 (dice pool calculation uses aggregated bonuses)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:PlayerAttack()` lines 235-257
- **Data Requirements**: Multiple sources queried, see acceptance criteria
- **Performance Considerations**: 5-7 property reads per attack (negligible cost)

---

### FR-005: Display Dice Rolls in Combat Log
**Priority**: High
**Status**: Implemented

**Description**:
All dice rolls (attack and defense) are displayed in the combat log with individual die results, success counts, and narrative context. This provides transparency for players to understand why attacks hit/miss and verify the system is working correctly.

**Rationale**:
Transparency builds player trust in the dice system. Showing individual die results (not just success counts) lets players verify fairness and understand probability. Narrative messages ("The attack is deflected!") add flavor while reinforcing mechanical outcomes.

**Acceptance Criteria**:
- [ ] Attack roll displayed: "Rolled Xd6: [6, 5, 4, 3, 2, 1] = Y successes"
- [ ] Defense roll displayed: "Rolled Xd6: [results] = Y successes"
- [ ] Individual die results shown in brackets (e.g., [6, 5, 4, 3, 2, 1])
- [ ] Success count shown after results (e.g., "= 3 successes")
- [ ] Bonus breakdown shown before attack roll (see FR-004)
- [ ] Net success calculation shown: "3 - 2 = 1 (HIT!)"
- [ ] Outcome message:
  - Hit: "Your attack lands!" or "Enemy strike connects!"
  - Miss: "The attack is deflected!" or "You dodge the blow!"
- [ ] All messages timestamped and logged in CombatState.LogEntries
- [ ] Log accessible for replay/debugging

**Example Scenarios**:
1. **Scenario**: Typical attack sequence
   - **Expected Output**:
     ```
     Calculating attack bonuses:
       Base FINESSE: 6d6
       Equipment: +2
       Total: 8d6
     You attack Skeleton Warrior!
     Rolled 8d6: [6, 5, 4, 6, 2, 5, 3, 1] = 4 successes
     Skeleton Warrior defends!
     Rolled 4d6: [3, 5, 2, 6] = 2 successes
     Net successes: 4 - 2 = 2 (HIT!)
     Your attack lands!
     [Proceed to damage calculation]
     ```
   - **Success Condition**: All stages logged clearly and sequentially

2. **Scenario**: Attack misses (tie)
   - **Expected Output**:
     ```
     Rolled 5d6: [6, 5, 4, 3, 2] = 2 successes
     Enemy defends!
     Rolled 5d6: [5, 6, 4, 1, 3] = 2 successes
     Net successes: 2 - 2 = 0 (MISS - Defender wins ties)
     The attack is deflected!
     ```
   - **Success Condition**: Tie rule explicitly stated, miss confirmed

3. **Edge Case**: Perfect roll (all 6s)
   - **Input**: 5d6 attack roll, all dice show 6
   - **Expected Output**: "Rolled 5d6: [6, 6, 6, 6, 6] = 5 successes (PERFECT ROLL!)"
   - **Success Condition**: Special message for maximum successes

**Dependencies**:
- Requires: Combat State logging system
- Requires: FR-001, FR-002, FR-003 (all rolls to log)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:FormatRolls()`, `AddLogEntry()`
- **Data Requirements**: DiceResult.Rolls (individual die values), DiceResult.Successes
- **Performance Considerations**: String formatting cost negligible compared to combat logic

---

## System Mechanics

### Mechanic 1: Opposed Dice Pool Resolution

**Overview**:
The accuracy system uses opposed dice pool rolls where both attacker and defender roll simultaneously, count successes (5-6 on d6), and compare results to determine hit/miss. This creates dynamic, probabilistic combat where neither side has guaranteed outcomes.

**How It Works**:
1. **Attack Declaration**: Player or AI declares attack action against target
2. **Bonus Aggregation**: System gathers all accuracy bonuses (equipment, status, abilities)
3. **Attack Dice Calculation**: Base attribute + all bonuses = attack dice pool (cap at 20)
4. **Defense Dice Calculation**: Defender's STURDINESS = defense dice pool
5. **Simultaneous Rolls**: Both pools rolled simultaneously using DiceService.Roll()
6. **Success Counting**: Count dice showing 5-6 as successes for each pool
7. **Net Success Calculation**: Attack successes - Defense successes = net
8. **Hit Determination**: If net > 0, proceed to damage; if net ≤ 0, miss
9. **Combat Log**: Display all steps, rolls, and outcome

**Formula/Logic**:
```
Attack_Dice = Base_Attribute + Equipment_Bonus + Ability_Bonus + Status_Bonuses
Attack_Dice = MIN(Attack_Dice, 20) // Cap at 20 dice

Defense_Dice = Defender.STURDINESS

Attack_Roll = DiceService.Roll(Attack_Dice)
Defense_Roll = DiceService.Roll(Defense_Dice)

Net_Successes = Attack_Roll.Successes - Defense_Roll.Successes

IF Net_Successes > 0:
  Hit = TRUE
  Proceed_To_Damage()
ELSE:
  Hit = FALSE
  Log("Attack deflected")
END IF
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| Success_Threshold | int | 1-6 | 5 | Minimum die value for success (5-6 = success) | No (core mechanic) |
| Max_Dice_Pool | int | 10-30 | 20 | Maximum dice in attack/defense pool | Yes (performance) |
| Defender_Wins_Ties | bool | - | TRUE | Ties (net 0) result in miss | Yes (balance) |
| Attribute_Cap | int | 5-15 | 10 | Maximum attribute value | No (progression system) |

**Edge Cases**:
1. **Zero defense (STURDINESS 0)**:
   - **Condition**: Defender has 0 STURDINESS (impossible in normal play, debug only)
   - **Behavior**: Defense roll = 0d6 = 0 successes → attacker always hits
   - **Example**: Attack roll 3 successes - Defense 0 successes = 3 net → HIT

2. **Dice pool overflow (20+ dice)**:
   - **Condition**: Base attribute 10 + bonuses 12 = 22 dice
   - **Behavior**: Capped at 20 dice, log warns "Attack dice capped at 20"
   - **Example**: Prevents performance issues with massive dice pools

3. **Perfect rolls (all 6s or all 1s)**:
   - **Condition**: All dice show max (6) or min (1-4)
   - **Behavior**: Log shows special message for perfect/terrible rolls
   - **Example**: 5d6 = [6,6,6,6,6] → "PERFECT ROLL! 5 successes"

**Related Requirements**: FR-001, FR-002, FR-003

---

### Mechanic 2: Accuracy Bonus Economy

**Overview**:
Accuracy bonuses from various sources (equipment, abilities, status effects) stack additively to increase attack dice pools. Each +1 bonus die adds ~0.33 average successes, translating to ~10-15% increased hit chance depending on opponent's defense.

**How It Works**:
1. **Equipment Bonuses**: Weapons/gear grant static accuracy bonuses (0-3 typical range)
2. **Status Effect Bonuses**: [Analyzed], Battle Rage grant +2 dice while active
3. **Ability Bonuses**: Temporary bonuses from abilities (e.g., +3 for next attack)
4. **Performance Bonuses**: Saga of Courage grants +2 while performing
5. **Additive Stacking**: All bonuses sum together before applying to base attribute
6. **No Diminishing Returns**: Each +1 adds exactly +1 die (linear scaling)
7. **Consumption**: Temporary bonuses (ability) consumed after use, others persist

**Formula/Logic**:
```
Total_Bonus = 0

// Equipment (persistent)
Total_Bonus += EquippedWeapon.AccuracyBonus

// Ability (temporary, consumed after attack)
Total_Bonus += CombatState.PlayerNextAttackBonusDice

// Status Effects (duration-based)
IF Target.AnalyzedTurnsRemaining > 0:
  Total_Bonus += 2

IF Attacker.BattleRageTurnsRemaining > 0:
  Total_Bonus += 2

// Performance (active check)
IF Attacker.IsPerforming AND Attacker.CurrentPerformance == "Saga of Courage":
  Total_Bonus += 2

Attack_Dice = Base_Attribute + Total_Bonus
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| Equipment_Accuracy_Max | int | 0-5 | 3 | Maximum equipment accuracy bonus | Yes (balance) |
| Analyzed_Bonus | int | 0-5 | 2 | Accuracy bonus from [Analyzed] status | Yes (balance) |
| Battle_Rage_Bonus | int | 0-5 | 2 | Accuracy bonus from Battle Rage | Yes (balance) |
| Saga_Bonus | int | 0-5 | 2 | Accuracy bonus from Saga of Courage | Yes (balance) |
| Bonus_Cap | int | 0-15 | None | Maximum total bonuses (currently uncapped) | Yes (future) |

**Edge Cases**:
1. **Bonus cap enforcement (future)**:
   - **Condition**: If bonus cap implemented (e.g., max +7 total)
   - **Behavior**: Total_Bonus = MIN(calculated_bonus, cap)
   - **Example**: Equipment +3 + [Analyzed] +2 + Battle Rage +2 + Ability +3 = 10 → capped at 7

2. **Temporary bonus expiration mid-combat**:
   - **Condition**: [Analyzed] duration expires between attacks
   - **Behavior**: Bonus active for attack 1, inactive for attack 2
   - **Example**: Turn 1: 10d6 (with [Analyzed]), Turn 3: 8d6 (expired)

3. **Ability bonus consumption**:
   - **Condition**: Ability grants +5 for "next attack"
   - **Behavior**: First attack uses +5, CombatState.PlayerNextAttackBonusDice resets to 0
   - **Example**: Attack 1: 13d6, Attack 2: 8d6 (bonus consumed)

**Related Requirements**: FR-001, FR-004

---

### Mechanic 3: Probabilistic Hit Chance Scaling

**Overview**:
Each die in the attack or defense pool has a 33% chance of success (rolling 5-6 on d6). Higher dice pools produce more consistent results due to law of large numbers, while small pools (1-3 dice) have high variance. This creates meaningful differences between builds.

**How It Works**:
1. **Individual Die Success Rate**: Each d6 has 2/6 (33.33%) chance of success
2. **Expected Successes**: Dice pool size × 0.33 = average successes
3. **Variance**: Larger pools have lower relative variance (more predictable)
4. **Small Pools**: 1-3 dice highly variable (0-3 successes range)
5. **Large Pools**: 10+ dice more predictable (~3-4 success range typical)

**Formula/Logic**:
```
Expected_Successes(dice_count) = dice_count × 0.333

Variance(dice_count) = dice_count × p × (1 - p)
  where p = 0.333 (success rate)

Standard_Deviation(dice_count) = SQRT(Variance)

// Example: 6d6 attack vs 5d6 defense
Attack_Expected = 6 × 0.333 = 2.0 successes
Defense_Expected = 5 × 0.333 = 1.67 successes
Expected_Net = 2.0 - 1.67 = 0.33 successes

// Probability of hit (approximate)
Hit_Chance ≈ 55-60% (due to variance and defender-wins-ties rule)
```

**Hit Chance Table (Attack Dice vs Defense Dice)**:
| Attack | vs 3d6 (STR 3) | vs 5d6 (STR 5) | vs 7d6 (STR 7) | vs 10d6 (STR 10) |
|--------|----------------|----------------|----------------|-------------------|
| 3d6 (ATT 3) | 50% | 30% | 15% | 5% |
| 5d6 (ATT 5) | 70% | 50% | 30% | 15% |
| 7d6 (ATT 7) | 85% | 70% | 50% | 30% |
| 10d6 (ATT 10) | 95% | 85% | 70% | 50% |
| 12d6 (ATT 6 +6 bonus) | 98% | 92% | 80% | 65% |
| 15d6 (ATT 8 +7 bonus) | 99% | 96% | 88% | 75% |

*Approximate probabilities based on dice pool variance analysis*

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| Die_Sides | int | 4-20 | 6 | Sides on each die (d6 standard) | No (core mechanic) |
| Success_Rate_Per_Die | float | 0.1-0.9 | 0.333 | Probability per die (5-6 on d6) | No (core mechanic) |
| Min_Pool_Size | int | 0-5 | 1 | Minimum dice in pool | No (attribute minimum) |
| Max_Pool_Size | int | 10-30 | 20 | Maximum dice in pool | Yes (performance) |

**Edge Cases**:
1. **Extreme mismatch (15d6 vs 1d6)**:
   - **Condition**: Glass cannon (MIGHT 10 + bonuses 5) vs low-STURDINESS enemy (1)
   - **Behavior**: Hit chance ~98-99% (nearly guaranteed)
   - **Example**: Expected net successes = 5.0 - 0.33 = 4.67 → almost always positive

2. **Perfect balance (equal pools)**:
   - **Condition**: 6d6 attack vs 6d6 defense
   - **Behavior**: Hit chance ~45-48% (slightly favors defender due to tie rule)
   - **Example**: Expected net = 2.0 - 2.0 = 0 → coin flip with defensive bias

3. **Variance extremes (small pools)**:
   - **Condition**: 2d6 attack vs 2d6 defense
   - **Behavior**: High variance - possible outcomes 0-2 net successes or 0-2 net failures
   - **Example**: Actual results wildly variable (could be -2, -1, 0, +1, +2 net)

**Related Requirements**: FR-001, FR-002, FR-003

---
