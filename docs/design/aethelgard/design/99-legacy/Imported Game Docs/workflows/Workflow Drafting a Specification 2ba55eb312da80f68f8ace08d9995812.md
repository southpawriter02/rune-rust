# Workflow: Drafting a Specification

Parent item: Specification Writing Guide (Specification%20Writing%20Guide%202ba55eb312da8032b14de6752422e93e.md)

**Purpose**: Step-by-step workflow for drafting a complete feature/system specification after planning is approved.

**Target Audience**: AI assistants and human developers creating specifications

**Last Updated**: 2025-11-19

**Prerequisites**: Planning document approved (see `WORKFLOW_PLANNING.md`)

---

## Table of Contents

1. [When to Use This Workflow](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
2. [Drafting Workflow Overview](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
3. [Step 1: Setup & Initialization](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
4. [Step 2: Document Control & Executive Summary](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
5. [Step 3: Related Documentation & Dependencies](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
6. [Step 4: Design Philosophy](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
7. [Step 5: Functional Requirements](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
8. [Step 6: System Mechanics](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
9. [Step 7: Integration Points](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
10. [Step 8: Implementation Guidance](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
11. [Step 9: Balance & Tuning](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
12. [Step 10: Validation & Testing](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
13. [Step 11: Quality Review](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
14. [Step 12: Finalization](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)
15. [Drafting Checklist](Workflow%20Drafting%20a%20Specification%202ba55eb312da80f68f8ace08d9995812.md)

---

## When to Use This Workflow

Use this workflow when:

- ✅ Planning document has been approved by user
- ✅ You have clear scope, requirements, and mechanics defined
- ✅ Ready to write the complete, formal specification

**Do NOT use this workflow if**:

- ❌ Planning not yet approved (use `WORKFLOW_PLANNING.md` first)
- ❌ Implementing an existing spec (use `WORKFLOW_IMPLEMENTING.md`)
- ❌ Making minor updates to existing spec (just edit the spec directly)

---

## Drafting Workflow Overview

**Estimated Time**: 2-5 hours for complete specification (depending on complexity)

**Process**:

```
1. Setup & Initialization (10 min)
   ↓
2. Document Control & Executive Summary (20 min)
   ↓
3. Related Documentation & Dependencies (15 min)
   ↓
4. Design Philosophy (20 min)
   ↓
5. Functional Requirements (60-90 min) ← THE CORE
   ↓
6. System Mechanics (45-60 min)
   ↓
7. Integration Points (20 min)
   ↓
8. Implementation Guidance (30 min)
   ↓
9. Balance & Tuning (20 min)
   ↓
10. Validation & Testing (15 min)
   ↓
11. Quality Review (30 min)
   ↓
12. Finalization (10 min)

```

**Goal**: Produce a complete, high-quality specification ready for "Review" status.

---

## Step 1: Setup & Initialization

**Time**: 10 minutes

**Objective**: Create spec file and set up structure.

### Actions

1. **Copy Template**:
    
    ```bash
    cp docs/00-specifications/TEMPLATE.md docs/00-specifications/[domain]/[system-name]-spec.md
    
    ```
    
2. **Assign Spec ID**:
    - Check `SPEC_BACKLOG.md` for next available number in domain
    - Format: `SPEC-{DOMAIN}-{NUMBER}` (e.g., SPEC-COMBAT-002)
    - Update SPEC_BACKLOG.md status to "In Progress"
3. **Set File Metadata**:
    - Update `Specification ID` at top of file
    - Update `Last Updated` date
    - Set `Status` to "Draft"
4. **Create Git Branch** (if needed):
    
    ```bash
    git checkout -b spec/[system-name]
    
    ```
    

### Output

✅ New spec file created from template
✅ Spec ID assigned and unique
✅ SPEC_BACKLOG.md updated to "In Progress"
✅ Git branch created (optional)

---

## Step 2: Document Control & Executive Summary

**Time**: 20 minutes

**Objective**: Fill in document metadata and write concise summary.

### Document Control Section

**Fill in**:

- **Version History**: Version 1.0, today's date, your name, "Initial specification"
- **Approval Status**: Check "Draft" box
- **Stakeholders**: Identify primary owner, design, balance, implementation, QA roles

### Executive Summary Section

**Purpose Statement** (1-2 sentences):

- What does this system exist to accomplish?
- Focus on player experience and design intent

**Scope**:

- **In Scope**: Copy from planning document (3-8 bullet points)
- **Out of Scope**: Copy from planning document (3-5 bullet points with spec references)

**Success Criteria**:

- **Player Experience**: Desired emotional/gameplay outcome
- **Technical**: Performance or implementation constraints
- **Design**: Design goals achieved
- **Balance**: Balance targets met

### Example

```markdown
### Purpose Statement
The Damage Calculation System provides consistent, tunable combat damage mechanics where player tactical choices (target selection, damage type, critical hits) create meaningful moment-to-moment decisions.

### Scope
**In Scope**:
- Physical damage calculation (melee, ranged)
- Aetheric damage calculation (Weaving-based)
- Armor and resistance application
- Critical hit and critical failure mechanics
- Damage type vulnerabilities and resistances

**Out of Scope**:
- Status effect damage over time → SPEC-COMBAT-003
- Healing mechanics → Future spec
- Environmental damage sources → SPEC-WORLD-003
- Boss-specific damage modifiers → SPEC-COMBAT-005

### Success Criteria
- **Player Experience**: Damage feels impactful; crits are exciting; armor meaningfully reduces damage
- **Technical**: Damage calculations < 5ms per attack; support 10+ combatants simultaneously
- **Design**: Physical and Aetheric damage equally viable; armor ~30-50% damage reduction
- **Balance**: No one-shot kills from standard attacks; crits ~50% damage increase

```

### Actions

- [ ]  Fill Document Control version history
- [ ]  Check "Draft" approval status
- [ ]  Identify stakeholders
- [ ]  Write Purpose Statement (1-2 sentences)
- [ ]  Copy In Scope from planning (3-8 items)
- [ ]  Copy Out of Scope from planning (3-5 items with refs)
- [ ]  Write Success Criteria (4 metrics)

---

## Step 3: Related Documentation & Dependencies

**Time**: 15 minutes

**Objective**: Link to all related specs, docs, and code.

### Dependencies Section

**Depends On** (systems this spec requires):

- Copy from planning document relationship map
- Format: `[System Name]: [Reason] → SPEC-XXX-YYY or docs/path:line`

**Depended Upon By** (systems that require this spec):

- Copy from planning document relationship map
- Format: `[System Name]: [Reason] → SPEC-XXX-YYY`

### Related Specifications

List all related specs (same domain, integration points, parallel systems)

### Implementation Documentation

Link to existing Layer 1 docs, statistical registry, balance reference

### Code References

Link to existing code (services, models, tests) with file:line format

### Example

```markdown
### Dependencies
**Depends On**:
- Dice Pool System: Calculates successes from attribute rolls → `docs/01-systems/combat-resolution.md:45`
- Character Progression: Provides attribute values (MIGHT, FINESSE, etc.) → `SPEC-PROGRESSION-001`
- Equipment System: Provides weapon/armor stats → `SPEC-ECONOMY-001` (not yet drafted)

**Depended Upon By**:
- Combat Resolution: Applies calculated damage to entities → `SPEC-COMBAT-001`
- Status Effects: Damage triggers status effects → `SPEC-COMBAT-003` (not yet drafted)
- Trauma Economy: Damage taken increases stress → `SPEC-ECONOMY-003`

### Related Specifications
- `SPEC-COMBAT-001`: Combat Resolution (uses damage calculations)
- `SPEC-COMBAT-003`: Status Effects (triggers from damage)
- `SPEC-PROGRESSION-001`: Character Progression (provides attributes)

### Implementation Documentation
- **System Docs**: `docs/01-systems/damage-calculation.md`
- **Statistical Registry**: None (calculations, not data-driven)

### Code References
- **Primary Service**: `RuneAndRust.Engine/Services/CombatEngine.cs:200-450`
- **Core Models**: `RuneAndRust.Core/Models/DamageResult.cs`
- **Enums**: `RuneAndRust.Core/Enums/DamageType.cs`
- **Tests**: `RuneAndRust.Tests/CombatEngineTests.cs`

```

### Actions

- [ ]  List all upstream dependencies (Depends On)
- [ ]  List all downstream dependents (Depended Upon By)
- [ ]  List related specifications
- [ ]  Link to implementation documentation
- [ ]  Link to code references with file:line

---

## Step 4: Design Philosophy

**Time**: 20 minutes

**Objective**: Articulate design principles and player experience goals.

### Design Pillars (3 pillars)

For each pillar:

- **Name**: Brief descriptor
- **Rationale**: Why this principle matters
- **Examples**: How it manifests in gameplay

### Player Experience Goals

- **Target Experience**: Emotional/cognitive experience desired
- **Moment-to-Moment Gameplay**: What players do second-to-second
- **Learning Curve**: Novice → Intermediate → Expert progression

### Design Constraints

- **Technical**: Platform, performance, engine limitations
- **Gameplay**: Genre conventions, accessibility requirements
- **Narrative**: Lore consistency, thematic requirements
- **Scope**: Time, budget, team size constraints

### Example

```markdown
### Design Pillars

1. **Tactical Damage Type Selection**
   - **Rationale**: Players should choose damage type (Physical vs. Aetheric) based on enemy resistances
   - **Examples**: Armored enemies weak to Aetheric; Aetheric entities resist Aetheric but vulnerable to Physical

2. **Predictable Base, Variable Outcomes**
   - **Rationale**: Players should understand base damage, but dice rolls create excitement
   - **Examples**: "This attack does ~20 damage" (predictable) but actual result 15-25 (variable)

3. **Armor Feels Meaningful**
   - **Rationale**: Investing in armor should visibly reduce incoming damage
   - **Examples**: 10 armor reduces 22 damage to 12 (46% reduction, noticeable)

### Player Experience Goals
**Target Experience**: "I understand my damage output and can make tactical choices to optimize it."

**Moment-to-Moment Gameplay**:
- Choose weapon/ability (Physical or Aetheric damage)
- Observe enemy type (does it have high armor? Resistance?)
- Execute attack, see damage number
- Adjust tactics based on damage effectiveness

**Learning Curve**:
- **Novice** (0-2 hours): Understand Physical vs. Aetheric damage
- **Intermediate** (2-10 hours): Recognize enemy resistances, choose damage type tactically
- **Expert** (10+ hours): Optimize builds for specific damage types, exploit vulnerabilities

### Design Constraints
- **Technical**: Damage calculations must run in <5ms (combat can have 10+ entities)
- **Gameplay**: Damage must scale with character progression (Legend 1-20)
- **Narrative**: Aetheric damage must require Aether Pool expenditure (setting constraint)
- **Scope**: Damage system must support future expansion (new damage types possible)

```

### Actions

- [ ]  Write 3 design pillars (name, rationale, examples)
- [ ]  Define target player experience (1 sentence)
- [ ]  Describe moment-to-moment gameplay (bullet points)
- [ ]  Define learning curve (novice/intermediate/expert)
- [ ]  List design constraints (technical, gameplay, narrative, scope)

---

## Step 5: Functional Requirements

**Time**: 60-90 minutes (THIS IS THE CORE OF THE SPEC)

**Objective**: Fully specify all functional requirements with acceptance criteria and examples.

### For Each FR (from planning document)

Expand each FR with:

1. **Priority**: Critical / High / Medium / Low
2. **Status**: Not Started / In Progress / Implemented / Tested
3. **Description**: Detailed description (2-4 sentences)
4. **Rationale**: Why this requirement exists (link to design goals)
5. **Acceptance Criteria**: 3-6 checkboxes (specific, measurable)
6. **Example Scenarios**: 2-3 concrete scenarios with inputs/outputs
7. **Dependencies**: Required FRs, blocked FRs
8. **Implementation Notes**: Code location, data requirements, performance notes

### FR Template (copy for each requirement)

```markdown
### FR-001: [Requirement Name]
**Priority**: Critical | High | Medium | Low
**Status**: Not Started | In Progress | Implemented | Tested

**Description**:
[2-4 sentence detailed description of what the system must do]

**Rationale**:
[Why this requirement exists - link to design goals or player experience]

**Acceptance Criteria**:
- [ ] [Specific, measurable criterion 1]
- [ ] [Specific, measurable criterion 2]
- [ ] [Specific, measurable criterion 3]
- [ ] [Specific, measurable criterion 4]

**Example Scenarios**:
1. **Scenario**: [Concrete gameplay situation]
   - **Input**: [What the player/system does]
   - **Expected Output**: [What the system produces]
   - **Success Condition**: [How we know it worked]

2. **Edge Case**: [Unusual or boundary condition]
   - **Input**: [What happens]
   - **Expected Behavior**: [How system handles it]

**Dependencies**:
- Requires: [Other FR-IDs or systems]
- Blocks: [FR-IDs that depend on this]

**Implementation Notes**:
- **Code Location**: `Path/To/Service.cs:line`
- **Data Requirements**: [What data this needs]
- **Performance Considerations**: [Any performance notes]

```

### Example FR

```markdown
### FR-001: Physical Damage Calculation
**Priority**: Critical
**Status**: Implemented

**Description**:
The system must calculate Physical damage using weapon base damage, dice successes from attribute rolls, and target armor. Physical damage is reduced by target's armor rating. Result is floored to integer (no fractional damage).

**Rationale**:
Physical damage is the primary damage type for Warrior and Skirmisher archetypes. It must be consistent, predictable in base amount, and clearly affected by armor to reward tanky builds.

**Acceptance Criteria**:
- [ ] Physical damage uses formula: BaseDamage = Weapon.BaseDamage + (SuccessCount × AttributeModifier)
- [ ] Attribute modifier is MIGHT for melee weapons, FINESSE for ranged weapons
- [ ] Armor reduces damage: FinalDamage = max(0, BaseDamage - TargetArmor)
- [ ] Damage is floored to integer (17.9 → 17)
- [ ] Minimum damage is 0 (armor can fully negate attack)

**Example Scenarios**:
1. **Scenario**: Warrior with MIGHT 4 attacks armored enemy with longsword
   - **Input**: Weapon.BaseDamage=10, SuccessCount=3, MIGHT=4, TargetArmor=5
   - **Expected Output**: BaseDamage = 10 + (3×4) = 22, FinalDamage = 22 - 5 = 17
   - **Success Condition**: Enemy takes 17 damage

2. **Edge Case**: High armor fully negates low-damage attack
   - **Input**: BaseDamage=8, TargetArmor=15
   - **Expected Behavior**: FinalDamage = max(0, 8-15) = 0 (no damage dealt)
   - **Success Condition**: Enemy HP unchanged, feedback "Attack deflected by armor!"

3. **Edge Case**: Fractional damage is floored
   - **Input**: BaseDamage=22, TargetArmor=5
   - **Expected Behavior**: FinalDamage = 17 (not 17.0 or 17.5)
   - **Success Condition**: Damage is integer type

**Dependencies**:
- Requires: Dice Pool System (provides SuccessCount)
- Requires: FR-004 (Attribute System integration)
- Blocks: FR-003 (Critical hits apply after base damage calculation)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/CombatEngine.cs:215-240`
- **Data Requirements**: Weapon.BaseDamage (int), Character.Attributes.MIGHT/FINESSE (int), Enemy.Armor (int)
- **Performance Considerations**: Simple arithmetic, <1ms per calculation

```

### Actions (repeat for each FR)

- [ ]  Copy FR template
- [ ]  Write detailed description (2-4 sentences)
- [ ]  Write rationale (link to design goals)
- [ ]  Write 3-6 acceptance criteria (specific, measurable)
- [ ]  Write 2-3 example scenarios (with concrete values)
- [ ]  List dependencies (requires/blocks)
- [ ]  Add implementation notes

**Target**: 5-12 complete FRs (this section is 200-400 lines typically)

---

## Step 6: System Mechanics

**Time**: 45-60 minutes

**Objective**: Fully document all formulas, algorithms, and mechanics.

### For Each Mechanic (from planning document)

Expand each mechanic with:

1. **Overview**: Plain language explanation
2. **How It Works**: Step-by-step process
3. **Formula/Logic**: Mathematical formula or pseudocode
4. **Parameters**: Table with type, range, default, description, tunable?
5. **Data Flow**: Input sources → Processing → Output destinations
6. **Edge Cases**: 2-4 edge cases with behavior
7. **Related Requirements**: Link to FR-IDs

### Mechanic Template (copy for each mechanic)

```markdown
### Mechanic [X]: [Mechanic Name]

**Overview**:
[Plain language explanation of what this mechanic does and why it exists]

**How It Works**:
1. [Step 1 in the mechanic]
2. [Step 2 in the mechanic]
3. [Step 3 in the mechanic]

**Formula/Logic**:

```

[Mathematical formula or pseudocode]

Example:
[Concrete values]
[Intermediate steps]
[Final result]

```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| [Param1] | int | 1-100 | 10 | [What it controls] | Yes |
| [Param2] | float | 0.5-3.0 | 1.5 | [What it controls] | Yes |

**Data Flow**:

```

Input Sources:
→ [Source 1: e.g., Player Character stats]
→ [Source 2: e.g., Weapon database]

Processing:
→ [Step 1: e.g., Calculate base pool]
→ [Step 2: e.g., Apply modifiers]

Output Destinations:
→ [Destination 1: e.g., Update character HP]
→ [Destination 2: e.g., Combat log]

```

**Edge Cases**:
1. **[Edge Case Name]**: [Description]
   - **Condition**: [When this occurs]
   - **Behavior**: [How system handles it]
   - **Example**: [Concrete example]

2. **[Edge Case Name]**: [Description]
   - **Condition**: [When this occurs]
   - **Behavior**: [How system handles it]
   - **Example**: [Concrete example]

**Related Requirements**: FR-001, FR-003

```

### Actions (repeat for each mechanic)

- [ ]  Write overview (plain language)
- [ ]  List steps (How It Works)
- [ ]  Write formula/pseudocode
- [ ]  Add worked example with concrete values
- [ ]  Document parameters (table with tunable flag)
- [ ]  Map data flow (input → processing → output)
- [ ]  Document 2-4 edge cases
- [ ]  Link to related FRs

**Target**: 3-8 mechanics (this section is 150-300 lines typically)

---

## Step 7: Integration Points

**Time**: 20 minutes

**Objective**: Document how this system integrates with other systems.

### This System Consumes

For each system consumed:

- **What We Use**: Specific functionality/data
- **How We Use It**: Description of integration
- **Dependency Type**: Hard | Soft | Optional
- **Failure Handling**: What happens if consumed system fails

### Systems That Consume This

For each system that consumes this:

- **What They Use**: Specific functionality/data we provide
- **How They Use It**: Description
- **Stability Contract**: What we guarantee not to break

### Event Integration

**Events Published** (table):

| Event Name | Trigger | Payload | Consumers |
| --- | --- | --- | --- |
| OnEventName | [When] | [Data] | [Who listens] |

**Events Subscribed** (table):

| Event Name | Source | Handler | Purpose |
| --- | --- | --- | --- |
| OnExternalEvent | [System] | [Method] | [Why] |

### Example

```markdown
### This System Consumes

**From Dice Pool System**:
- **What We Use**: `DiceService.RollDice(poolSize)` to calculate successes
- **How We Use It**: Physical/Aetheric attacks roll attribute-based dice pool
- **Dependency Type**: Hard (combat cannot function without dice rolls)
- **Failure Handling**: If dice service fails, combat aborts with error

**From Character Progression System** (`SPEC-PROGRESSION-001`):
- **What We Use**: `Character.Attributes` (MIGHT, FINESSE, WILL, etc.)
- **How We Use It**: Attribute values determine dice pool size and damage modifiers
- **Dependency Type**: Hard (damage cannot be calculated without attributes)
- **Failure Handling**: If attributes missing, use default values (all attributes = 2)

### Systems That Consume This

**Combat Resolution System** (`SPEC-COMBAT-001`):
- **What They Use**: `DamageResult` object (damage amount, damage type, is critical)
- **How They Use It**: Applies calculated damage to target HP
- **Stability Contract**: `DamageResult` structure will not change (additive only)

**Status Effects System** (`SPEC-COMBAT-003`):
- **What They Use**: `OnDamageDealt` event (damage type, amount, source, target)
- **How They Use It**: Triggers status effects (burning on fire damage, bleeding on critical)
- **Stability Contract**: Event payload will always include damage type and amount

### Event Integration

**Events Published**:
| Event Name | Trigger | Payload | Consumers |
|------------|---------|---------|-----------|
| OnDamageDealt | After damage calculated | DamageResult, source, target | StatusEffectService, TraumaService |
| OnCriticalHit | Critical hit detected | DamageResult, weapon | CombatLog, AchievementService |

**Events Subscribed**:
| Event Name | Source | Handler | Purpose |
|------------|--------|---------|---------|
| OnWeaponEquipped | EquipmentService | RecalculateBaseDamage() | Update weapon base damage |
| OnAttributeChanged | ProgressionService | RecalculateDamageModifiers() | Update attribute modifiers |

```

### Actions

- [ ]  Document all systems this consumes (with integration details)
- [ ]  Document all systems that consume this (with stability contracts)
- [ ]  List all events published (table)
- [ ]  List all events subscribed (table)

---

## Step 8: Implementation Guidance

**Time**: 30 minutes

**Objective**: Provide guidance for AI/human implementers.

### Implementation Status

- [ ]  Core models created
- [ ]  Service/Engine implemented
- [ ]  Factory (if needed) implemented
- [ ]  Database/Registry (if needed) populated
- [ ]  Integration with dependent systems
- [ ]  Unit tests written
- [ ]  Integration tests written
- [ ]  Balance testing completed
- [ ]  Documentation updated

### Recommended Architecture

- **Namespace**: `RuneAndRust.[Layer].[System]`
- **Primary Classes**: [Service, Factory, Database, etc.]
- **Data Models Location**: `RuneAndRust.Core/...`

### Reference Implementation

Point to existing code patterns to follow:

- "See `RuneAndRust.Engine/CombatEngine.cs:150-300` for event-driven pattern"
- "See `RuneAndRust.Core/Archetypes/WarriorArchetype.cs` for archetype pattern"

### Common Mistakes to Avoid

List 3-5 common mistakes with examples:

```
❌ Mistake 1: [Problem description]
- **Problem**: [What goes wrong]
- **Solution**: [How to avoid]
- **Example**: [Concrete example]

```

### Database Schema Notes

If system uses database, provide schema:

```sql
CREATE TABLE [TableName] (
    [Column1] [Type] [Constraints],
    PRIMARY KEY ([Column]),
    FOREIGN KEY ([Column]) REFERENCES [OtherTable]([Column])
);

```

### Actions

- [ ]  Check implementation status boxes
- [ ]  Document recommended architecture
- [ ]  Point to reference implementations
- [ ]  List 3-5 common mistakes to avoid
- [ ]  Provide database schema (if applicable)

---

## Step 9: Balance & Tuning

**Time**: 20 minutes

**Objective**: Document tunable parameters and balance targets.

### Tunable Parameters (table)

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
| --- | --- | --- | --- | --- | --- | --- |
| [Param] | [Code/Data location] | [Value] | [Min] | [Max] | [What it affects] | High/Med/Low |

### Balance Targets

For each balance target:

- **Target**: Specific measurable goal
- **Metric**: How we measure
- **Current**: Current state
- **Target**: Desired state
- **Levers**: Parameters to adjust

### Testing Scenarios

For 2-4 balance tests:

- **Purpose**: What this tests
- **Setup**: Preconditions
- **Procedure**: Steps
- **Expected Results**: Metrics and values
- **Pass Criteria**: What constitutes success

### Example

```markdown
### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
|-----------|----------|---------------|-----|-----|--------|------------------|
| WeaponBaseDamage | Weapon.json | 10 | 5 | 50 | Base damage output | Medium |
| CritMultiplier | CombatConfig.cs | 1.5 | 1.0 | 3.0 | Critical hit damage | Low |
| ArmorEffectiveness | ArmorConfig.cs | 1.0 | 0.5 | 2.0 | Armor damage reduction | Medium |

### Balance Targets

**Target 1: Armor reduces damage by 30-50%**
- **Metric**: (Damage without armor - Damage with armor) / Damage without armor
- **Current**: ~40% reduction (10 armor vs 25 base damage = 15 final damage, 40% reduction)
- **Target**: 30-50% reduction (feels meaningful)
- **Levers**: Armor values (5-20 range), ArmorEffectiveness multiplier

**Target 2: Critical hits ~50% damage increase**
- **Metric**: Crit damage / Normal damage
- **Current**: 1.5x (CritMultiplier = 1.5)
- **Target**: 1.4x - 1.6x (exciting but not overpowered)
- **Levers**: CritMultiplier parameter

### Testing Scenarios

#### Balance Test 1: Standard Combat Damage Range
**Purpose**: Verify damage output is in expected range for Level 5 character

**Setup**:
- Level 5 Warrior (MIGHT 4)
- Standard longsword (BaseDamage 12)
- Enemy with 8 armor

**Procedure**:
1. Run 100 attacks
2. Record damage dealt per attack
3. Calculate average, min, max

**Expected Results**:
- Average: ~15 damage
- Min: ~8 damage (1 success)
- Max: ~28 damage (5 successes)
- Standard deviation: ~5 damage

**Pass Criteria**: Average within 13-17 damage range

```

### Actions

- [ ]  List all tunable parameters (table with 5+ params)
- [ ]  Define 2-4 balance targets
- [ ]  Write 2-4 balance testing scenarios

---

## Step 10: Validation & Testing

**Time**: 15 minutes

**Objective**: Define how to verify specification is correctly implemented.

### Acceptance Testing Scenarios

For 3-5 scenarios:

- **Scenario**: [What to test]
- **Steps**: [How to test it]
- **Verify**: [Expected results with checkboxes]
- **Result**: PASS / FAIL / BLOCKED

### Performance Benchmarks (table)

| Operation | Target Latency | Acceptable Max | Current |
| --- | --- | --- | --- |
| [Operation 1] | <10ms | 50ms | [Value or TBD] |
| [Operation 2] | <50ms | 200ms | [Value or TBD] |

### Edge Case Testing Matrix (table)

| Edge Case | Expected Behavior | Test Status |
| --- | --- | --- |
| [Case 1] | [Behavior] | ✓ Tested / Not Started |
| [Case 2] | [Behavior] | ✓ Tested / Not Started |

### Example

```markdown
### Acceptance Testing Scenarios

**Scenario 1: Physical Damage Calculation**
1. Create Warrior with MIGHT 4
2. Equip longsword (BaseDamage 12)
3. Attack enemy with 8 armor
4. Roll dice (assume 3 successes)
5. Verify:
   - [ ] BaseDamage = 12 + (3×4) = 24
   - [ ] FinalDamage = 24 - 8 = 16
   - [ ] Enemy HP reduced by 16
6. Result: **PASS**

**Scenario 2: Critical Hit Multiplier**
1. Character attacks with BaseDamage 20
2. Trigger critical hit
3. Verify:
   - [ ] CritDamage = 20 × 1.5 = 30
   - [ ] Enemy takes 30 damage (before armor)
   - [ ] OnCriticalHit event fired
4. Result: **PASS**

### Performance Benchmarks

| Operation | Target Latency | Acceptable Max | Current |
|-----------|----------------|----------------|---------|
| Calculate damage (single attack) | <5ms | 20ms | 2ms |
| Apply damage to 10 targets (AoE) | <50ms | 200ms | 35ms |
| Process 100 attacks (combat simulation) | <500ms | 2000ms | 420ms |

### Edge Case Testing Matrix

| Edge Case | Expected Behavior | Test Status |
|-----------|-------------------|-------------|
| Armor > BaseDamage | Damage = 0, "Attack deflected" message | ✓ Tested |
| Critical hit + armor | Apply crit multiplier, then subtract armor | ✓ Tested |
| Negative attribute modifier | Use 1 (minimum) instead of negative | ✓ Tested |
| Fractional damage result | Floor to integer (17.9 → 17) | ✓ Tested |

```

### Actions

- [ ]  Write 3-5 acceptance testing scenarios
- [ ]  Define performance benchmarks (table)
- [ ]  Create edge case testing matrix (table)

---

## Step 11: Quality Review

**Time**: 30 minutes

**Objective**: Self-review specification for completeness and quality.

### Review Checklists

Use these checklists from `SPECIFICATION_DEVELOPMENT_RULES.md`:

**Specification Rules Check**:

- [ ]  SPEC-RULE-001: Uses [TEMPLATE.md](http://template.md/) structure
- [ ]  SPEC-RULE-002: All checklists completed
- [ ]  SPEC-RULE-003: All formulas have worked examples
- [ ]  SPEC-RULE-004: All FRs have acceptance criteria
- [ ]  SPEC-RULE-005: No ambiguous language
- [ ]  SPEC-RULE-006: Setting compliance validated
- [ ]  SPEC-RULE-007: Related documentation linked
- [ ]  SPEC-RULE-008: Unique Spec ID assigned
- [ ]  SPEC-RULE-009: SPEC_BACKLOG.md updated
- [ ]  SPEC-RULE-010: No TBD/TODO placeholders

**Setting Compliance Check** (if applicable):

- [ ]  SETTING-RULE-001: Read SETTING_COMPLIANCE.md
- [ ]  SETTING-RULE-002: No generic fantasy tropes
- [ ]  SETTING-RULE-003: Layer 2 Diagnostic Voice (if game text)
- [ ]  SETTING-RULE-004: Mysteries preserved
- [ ]  SETTING-RULE-005: No Pre-Glitch tech creation

**Quality Check**:

- [ ]  All functional requirements have acceptance criteria (3-6 each)
- [ ]  All mechanics have worked examples
- [ ]  All integration points documented
- [ ]  Implementation guidance provided
- [ ]  Balance targets defined
- [ ]  Test scenarios complete
- [ ]  No spelling/grammar errors
- [ ]  Formatting consistent
- [ ]  Cross-references valid

### Completeness Checklist (from template)

Run through the Document Completeness Checklist at bottom of `TEMPLATE.md`:

- [ ]  All required sections present
- [ ]  Version history populated
- [ ]  Stakeholders identified
- [ ]  Related documentation linked
- [ ]  Executive summary complete
- [ ]  All functional requirements documented
- [ ]  All mechanics explained with examples
- [ ]  Integration points identified
- [ ]  Balance targets defined
- [ ]  Implementation guidance provided
- [ ]  Technical accuracy verified
- [ ]  No ambiguous language
- [ ]  Examples provided for complex concepts
- [ ]  Cross-references valid
- [ ]  Formatting consistent
- [ ]  All requirements have IDs
- [ ]  All mechanics trace to requirements
- [ ]  All code references valid
- [ ]  All test scenarios cover requirements
- [ ]  All TBD placeholders resolved
- [ ]  All open questions addressed or tracked
- [ ]  Implementation feasibility confirmed

### Actions

- [ ]  Run through SPEC-RULE checklist (10 rules)
- [ ]  Run through SETTING-RULE checklist (5 rules, if applicable)
- [ ]  Run through Quality Check (9 items)
- [ ]  Run through Document Completeness Checklist (19 items)
- [ ]  Fix any violations found
- [ ]  Re-check after fixes

---

## Step 12: Finalization

**Time**: 10 minutes

**Objective**: Mark specification as ready for review.

### Final Actions

1. **Update Status**:
    - Change status from "Draft" to "Review"
    - Check "Review" box in Approval Status
2. **Update SPEC_BACKLOG.md**:
    - Change status from "In Progress" to "Completed"
    - Add completion date
    - Add file path
3. **Commit Specification**:
    
    ```bash
    git add docs/00-specifications/[domain]/[system-name]-spec.md
    git add docs/00-specifications/SPEC_BACKLOG.md
    git commit -m "docs: Add SPEC-[DOMAIN]-[NUMBER] ([System Name])
    
    Complete specification for [System Name] including:
    - 5-12 functional requirements with acceptance criteria
    - 3-8 system mechanics with worked examples
    - Integration points documentation
    - Implementation guidance for AI/human developers
    - Balance targets and testing scenarios
    
    Refs: SPEC-[DOMAIN]-[NUMBER]"
    
    ```
    
4. **Present to User**:
    - Notify user specification is ready for review
    - Provide summary of what was documented
    - Ask for feedback

### Output

✅ Specification marked "Review" status
✅ SPEC_BACKLOG.md updated to "Completed"
✅ Committed to git
✅ User notified

### Example Completion Message

```
✅ Specification Complete: SPEC-COMBAT-002 (Damage Calculation System)

**Summary**:
- 8 Functional Requirements (all critical/high priority)
- 5 System Mechanics with worked examples
- Integration with Combat Resolution, Status Effects, Trauma Economy
- Implementation guidance for AI developers
- 3 Balance targets with testing scenarios
- ~650 lines total

**Status**: Ready for Review

**Next Steps**:
- Please review the specification
- Provide feedback or approval
- If approved, I can proceed to implementation using WORKFLOW_IMPLEMENTING.md

```

---

## Drafting Checklist

Before marking specification as "Review", verify:

### Setup

- [ ]  Spec file created from [TEMPLATE.md](http://template.md/)
- [ ]  Unique Spec ID assigned (SPEC-XXX-YYY)
- [ ]  SPEC_BACKLOG.md updated to "In Progress"

### Document Control

- [ ]  Version history filled (v1.0, date, author)
- [ ]  Approval status set to "Draft" (then "Review" at end)
- [ ]  Stakeholders identified

### Executive Summary

- [ ]  Purpose statement written (1-2 sentences)
- [ ]  In Scope defined (3-8 items)
- [ ]  Out of Scope defined (3-5 items with refs)
- [ ]  Success criteria defined (4 metrics)

### Related Documentation

- [ ]  Upstream dependencies listed (Depends On)
- [ ]  Downstream dependents listed (Depended Upon By)
- [ ]  Related specifications listed
- [ ]  Implementation documentation linked
- [ ]  Code references linked (file:line format)

### Design Philosophy

- [ ]  3 design pillars written
- [ ]  Player experience goals defined
- [ ]  Learning curve described
- [ ]  Design constraints listed

### Functional Requirements (THE CORE)

- [ ]  5-12 FRs written
- [ ]  Each FR has priority, status, description, rationale
- [ ]  Each FR has 3-6 acceptance criteria
- [ ]  Each FR has 2-3 example scenarios
- [ ]  Each FR has dependencies and implementation notes
- [ ]  All FRs trace to design goals

### System Mechanics

- [ ]  3-8 mechanics written
- [ ]  Each mechanic has overview, steps, formula
- [ ]  Each mechanic has worked example with concrete values
- [ ]  Each mechanic has parameters table (with tunable flag)
- [ ]  Each mechanic has data flow diagram
- [ ]  Each mechanic has 2-4 edge cases
- [ ]  Each mechanic links to related FRs

### Integration Points

- [ ]  All consumed systems documented
- [ ]  All consuming systems documented
- [ ]  Events published listed (table)
- [ ]  Events subscribed listed (table)

### Implementation Guidance

- [ ]  Implementation status checklist present
- [ ]  Recommended architecture documented
- [ ]  Reference implementations pointed to
- [ ]  3-5 common mistakes listed
- [ ]  Database schema provided (if applicable)

### Balance & Tuning

- [ ]  Tunable parameters table (5+ params)
- [ ]  2-4 balance targets defined
- [ ]  2-4 balance testing scenarios written

### Validation & Testing

- [ ]  3-5 acceptance testing scenarios
- [ ]  Performance benchmarks table
- [ ]  Edge case testing matrix

### Quality Review

- [ ]  All SPEC-RULEs checked (10 rules)
- [ ]  All SETTING-RULEs checked (5 rules, if applicable)
- [ ]  Document Completeness Checklist complete (19 items)
- [ ]  No TBD/TODO placeholders
- [ ]  No ambiguous language
- [ ]  All formulas have worked examples
- [ ]  All cross-references valid

### Finalization

- [ ]  Status changed from "Draft" to "Review"
- [ ]  SPEC_BACKLOG.md updated to "Completed"
- [ ]  Committed to git with descriptive message
- [ ]  User notified

**Total Expected Length**: 400-800 lines for complete specification

---

**Document Version**: 1.0
**Maintained By**: Specification governance framework
**Feedback**: Update this workflow as drafting patterns emerge