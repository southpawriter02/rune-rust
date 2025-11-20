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
