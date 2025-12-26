# Workflow: Planning a Specification

Parent item: Specification Writing Guide (Specification%20Writing%20Guide%202ba55eb312da8032b14de6752422e93e.md)

**Purpose**: Step-by-step workflow for planning a feature/system specification before drafting.

**Target Audience**: AI assistants and human developers preparing to create specifications

**Last Updated**: 2025-11-19

---

## Table of Contents

1. [When to Use This Workflow](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
2. [Planning Workflow Overview](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
3. [Step 1: Understand the Assignment](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
4. [Step 2: Explore the Codebase](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
5. [Step 3: Map System Relationships](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
6. [Step 4: Define Scope & Requirements](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
7. [Step 5: Identify Mechanics & Formulas](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
8. [Step 6: Check Setting Compliance](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
9. [Step 7: Present Plan for Approval](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
10. [Planning Outputs & Next Steps](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
11. [Common Planning Pitfalls](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)
12. [Planning Checklist](Workflow%20Planning%20a%20Specification%202ba55eb312da80bfac51f08e07779f4a.md)

---

## When to Use This Workflow

Use this workflow when:

- ✅ Starting a new specification from scratch
- ✅ User requests a specification for a specific system
- ✅ You've identified a gap in documentation that needs a spec
- ✅ Planning major changes to an existing system (requiring new spec version)

**Do NOT use this workflow if**:

- ❌ Specification already exists and just needs minor updates (use WORKFLOW_DRAFTING.md to update)
- ❌ Implementing an existing specification (use WORKFLOW_IMPLEMENTING.md)
- ❌ Writing Layer 1 implementation documentation (that's not a specification)

---

## Planning Workflow Overview

**Estimated Time**: 30-60 minutes for complex systems, 15-30 minutes for simple systems

**Process**:

```
1. Understand Assignment (5-10 min)
   ↓
2. Explore Codebase (15-30 min)
   ↓
3. Map System Relationships (10-15 min)
   ↓
4. Define Scope & Requirements (15-20 min)
   ↓
5. Identify Mechanics & Formulas (10-15 min)
   ↓
6. Check Setting Compliance (5-10 min)
   ↓
7. Present Plan for Approval (5 min)

```

**Goal**: Produce a detailed planning document that defines WHAT the system does, WHY it exists, and HOW it integrates, ready for user approval before drafting.

---

## Step 1: Understand the Assignment

**Time**: 5-10 minutes

**Objective**: Clarify what you're specifying and why.

### Questions to Answer

1. **What system am I specifying?**
    - Name of feature/system
    - Domain (Combat, Progression, Economy, World, Narrative, etc.)
2. **What is the user's intent?**
    - Document existing implementation?
    - Design new feature?
    - Formalize informal design?
3. **Who will use this specification?**
    - AI implementers?
    - Human developers?
    - Game designers?
    - All of the above?
4. **What's the scope boundary?**
    - What's explicitly IN scope?
    - What's explicitly OUT of scope?
    - What overlaps with other systems?
5. **Are there existing resources?**
    - Existing code?
    - Layer 1 implementation docs?
    - Design notes?
    - Discord discussions?
    - User-provided context documents?

### Actions

- [ ]  Read user's request carefully
- [ ]  Check `SPEC_BACKLOG.md` to see if this spec is already planned
- [ ]  Identify domain (COMBAT, PROGRESSION, ECONOMY, WORLD, NARRATIVE)
- [ ]  List any user-provided context documents/files

### Output

**Assignment Summary** (to be presented to user):

```
System: [Name]
Domain: [DOMAIN]
User Intent: [Document existing / Design new / Formalize]
Scope (preliminary): [What's in scope]
Existing Resources: [List of files, docs, discussions]

```

### Example

```
System: Damage Calculation System
Domain: COMBAT
User Intent: Document existing implementation (code exists in CombatEngine.cs)
Scope (preliminary):
  - Physical/Aetheric damage types
  - Armor/resistance application
  - Critical hit mechanics
  - NOT status effect damage (that's SPEC-COMBAT-003)
Existing Resources:
  - RuneAndRust.Engine/Services/CombatEngine.cs (lines 200-450)
  - docs/01-systems/damage-calculation.md (implementation guide)
  - Balance spreadsheet (damage-balance-v0.38.xlsx)

```

### Questions to Ask User (if unclear)

- "Is this system already implemented, or are we designing it from scratch?"
- "What's the priority: High, Medium, or Low?"
- "Are there specific design goals or player experience goals?"
- "What's explicitly OUT of scope for this spec?"
- "Do you have existing design notes or documents I should reference?"

---

## Step 2: Explore the Codebase

**Time**: 15-30 minutes

**Objective**: Discover existing implementation, data, and documentation related to the system.

### Exploration Commands

Use these tools to explore:

**1. Find Related Code**:

```bash
# Find files with system name in filename
find RuneAndRust.Engine -name "*SystemName*.cs"

# Search for system name in code
grep -r "SystemName" RuneAndRust.Engine/ --include="*.cs"

# Find service classes (likely implementation)
find RuneAndRust.Engine -name "*Service.cs" | grep -i "SystemName"

```

**2. Find Documentation**:

```bash
# Search Layer 1 implementation docs
ls -la docs/01-systems/*system-name*.md

# Search for system mentions in specs
grep -r "System Name" docs/00-specifications/ --include="*.md"

# Search statistical registry
ls -la docs/02-statistical-registry/*system*.md

```

**3. Find Data Files**:

```bash
# JSON data files
find Data/ -name "*system*.json"

# SQL schema files
find Data/ -name "*.sql" | xargs grep -l "SystemName"

```

**4. Find Integration Points**:

```bash
# Find event subscriptions
grep -rn "OnSystemEvent" RuneAndRust.Engine/ --include="*.cs"

# Find service dependencies
grep -rn "SystemService" RuneAndRust.Engine/ --include="*.cs"

```

### Actions

- [ ]  Find all related code files (services, models, factories, databases)
- [ ]  Find all related documentation (Layer 1 docs, specs, guides)
- [ ]  Find all related data files (JSON, SQL, CSV)
- [ ]  Identify integration points (events, service dependencies)
- [ ]  Note existing tests (unit tests, integration tests)

### Output

**Exploration Notes** (structured list):

```
Code Locations:
  - RuneAndRust.Engine/Services/[System]Service.cs (main implementation)
  - RuneAndRust.Core/Models/[System].cs (data model)
  - RuneAndRust.Engine/[System]Factory.cs (object creation)
  - RuneAndRust.Engine/[System]Database.cs (static data)

Documentation:
  - docs/01-systems/[system].md (implementation guide)
  - docs/02-statistical-registry/[system].md (stat tables)
  - [Integration guide or guide file]

Data Files:
  - Data/[system]_data.json (system configurations)
  - Data/v0.XX.Y_[system]_schema.sql (database schema)

Integration Points:
  - Consumes: DiceService, SagaService
  - Consumed By: CombatEngine, LootService
  - Events: OnSystemTriggered, OnSystemCompleted

Tests:
  - RuneAndRust.Tests/[System]ServiceTests.cs
  - RuneAndRust.Tests/[System]IntegrationTests.cs

```

### Example (Damage Calculation System)

```
Code Locations:
  - RuneAndRust.Engine/Services/CombatEngine.cs:200-450 (damage calculation logic)
  - RuneAndRust.Core/Models/DamageResult.cs (damage result model)
  - RuneAndRust.Core/Enums/DamageType.cs (Physical, Aetheric, Corruption)

Documentation:
  - docs/01-systems/damage-calculation.md (implementation formulas)

Data Files:
  - None (damage is calculated, not data-driven)

Integration Points:
  - Consumes: DiceService.RollDice(), Character.Attributes
  - Consumed By: CombatEngine (applies damage to HP)
  - Events: OnDamageDealt

Tests:
  - RuneAndRust.Tests/CombatEngineTests.cs (damage tests)

```

---

## Step 3: Map System Relationships

**Time**: 10-15 minutes

**Objective**: Understand how this system integrates with other systems.

### Relationship Mapping

**Identify**:

1. **Upstream Dependencies** (systems this system NEEDS):
    - Services consumed
    - Data sources
    - Events subscribed to
2. **Downstream Dependents** (systems that NEED this system):
    - Services that consume this system
    - Events published
    - Data provided
3. **Parallel Systems** (related but independent):
    - Systems in same domain
    - Systems with similar mechanics
    - Systems that might conflict

### Actions

- [ ]  List all upstream dependencies
- [ ]  List all downstream dependents
- [ ]  Identify parallel/related systems
- [ ]  Check for circular dependencies (flag if found)
- [ ]  Map to existing specifications (SPEC-XXX-YYY)

### Output

**System Relationship Map**:

```
Upstream Dependencies (this system consumes):
  → [System A]: [What we use]
     - Spec: SPEC-XXX-YYY
     - Integration: [How integrated]
  → [System B]: [What we use]
     - Spec: SPEC-XXX-YYY
     - Integration: [How integrated]

Downstream Dependents (systems that consume this):
  → [System X]: [What they use from us]
     - Spec: SPEC-XXX-YYY
     - Integration: [How integrated]
  → [System Y]: [What they use from us]
     - Spec: SPEC-XXX-YYY
     - Integration: [How integrated]

Parallel Systems (related, independent):
  → [System P]: [Relationship]
  → [System Q]: [Relationship]

Circular Dependencies:
  [None] OR [FLAG: Circular dependency detected with System Z]

```

### Example (Damage Calculation System)

```
Upstream Dependencies (this system consumes):
  → Dice Pool System: Calculates successes from attribute rolls
     - Spec: docs/01-systems/combat-resolution.md (no spec yet)
     - Integration: DiceService.RollDice(poolSize) → successCount
  → Attribute System: Provides character attributes (MIGHT, WILL, etc.)
     - Spec: SPEC-PROGRESSION-001
     - Integration: Character.Attributes.MIGHT → damage modifier
  → Equipment System: Provides weapon/armor stats
     - Spec: SPEC-ECONOMY-001 (not yet drafted)
     - Integration: Weapon.BaseDamage, Armor.Resistance

Downstream Dependents (systems that consume this):
  → Combat Resolution: Applies calculated damage to entities
     - Spec: SPEC-COMBAT-001
     - Integration: CombatEngine.ApplyDamage(target, damageResult)
  → Status Effects: Damage triggers status effects (e.g., burning)
     - Spec: SPEC-COMBAT-003 (not yet drafted)
     - Integration: OnDamageDealt event → StatusEffectService
  → Trauma Economy: Damage taken increases psychic stress
     - Spec: SPEC-ECONOMY-003
     - Integration: OnDamageReceived → TraumaService.AddStress()

Parallel Systems (related, independent):
  → Healing System: Inverse of damage (restores HP)
  → Resistance System: Modifies damage taken (overlaps with armor)

Circular Dependencies:
  None detected

```

---

## Step 4: Define Scope & Requirements

**Time**: 15-20 minutes

**Objective**: Define what the system MUST do (functional requirements).

### Scope Definition

**In Scope** (what this specification WILL cover):

- List 3-8 major functional areas
- Be specific about boundaries

**Out of Scope** (what this specification will NOT cover):

- List related functionality explicitly excluded
- Reference other specs where excluded functionality belongs

### Functional Requirements Draft

**Target**: 5-12 functional requirements (FR-XXX)

**For Each Requirement**:

1. **Name**: Brief descriptor (e.g., "Damage Type Application")
2. **Description**: What must the system do?
3. **Rationale**: Why is this needed?
4. **Priority**: Critical / High / Medium / Low

### Actions

- [ ]  Define "In Scope" boundary (3-8 items)
- [ ]  Define "Out of Scope" boundary (3-5 items)
- [ ]  Draft 5-12 functional requirements
- [ ]  Assign priorities to requirements
- [ ]  Write rationale for each requirement

### Output

**Scope Document**:

```
IN SCOPE:
  - [Functional area 1]
  - [Functional area 2]
  - [Functional area 3]
  - ...

OUT OF SCOPE:
  - [Excluded functionality 1] → SPEC-XXX-YYY
  - [Excluded functionality 2] → SPEC-XXX-YYY
  - ...

FUNCTIONAL REQUIREMENTS (Draft):

FR-001: [Requirement Name]
  Priority: Critical
  Description: [What system must do]
  Rationale: [Why this is needed]

FR-002: [Requirement Name]
  Priority: High
  Description: [What system must do]
  Rationale: [Why this is needed]

[... continue for all requirements ...]

```

### Example (Damage Calculation System)

```
IN SCOPE:
  - Physical damage calculation (melee, ranged)
  - Aetheric damage calculation (Weaving-based)
  - Armor and resistance application
  - Critical hit and critical failure mechanics
  - Damage type vulnerabilities and resistances
  - Integration with dice pool system

OUT OF SCOPE:
  - Status effect damage over time → SPEC-COMBAT-003
  - Healing mechanics → Future spec
  - Environmental damage sources → SPEC-WORLD-003
  - Boss-specific damage modifiers → SPEC-COMBAT-005

FUNCTIONAL REQUIREMENTS (Draft):

FR-001: Damage Type System
  Priority: Critical
  Description: System must support Physical, Aetheric, and Corruption damage types with distinct calculation methods.
  Rationale: Different damage types interact with armor/resistance differently; core to combat variety.

FR-002: Base Damage Calculation
  Priority: Critical
  Description: System must calculate base damage from weapon stats and attribute modifiers.
  Rationale: Foundation for all damage; must be consistent and tunable.

FR-003: Armor & Resistance Application
  Priority: Critical
  Description: System must apply target's armor (vs Physical) and resistance (vs Aetheric) to reduce damage.
  Rationale: Defense stats must meaningfully reduce incoming damage.

FR-004: Critical Hit Mechanics
  Priority: High
  Description: System must support critical hits with configurable multiplier (default 1.5x).
  Rationale: Crits provide exciting moments and reward high-FINESSE builds.

FR-005: Damage Vulnerabilities
  Priority: Medium
  Description: System must support enemy vulnerabilities (+50% damage) and resistances (-50% damage) to specific damage types.
  Rationale: Encourages tactical choice of damage type against specific enemies.

[... 5-12 total requirements ...]

```

---

## Step 5: Identify Mechanics & Formulas

**Time**: 10-15 minutes

**Objective**: Identify key formulas, calculations, and algorithms.

### Mechanic Discovery

**For Each Requirement**, identify:

1. **Formula/Algorithm**: Mathematical or logical process
2. **Inputs**: What data is needed?
3. **Outputs**: What is produced?
4. **Parameters**: Tunable values (defaults, ranges)

### Actions

- [ ]  Extract formulas from existing code (if exists)
- [ ]  Extract formulas from existing docs (if exists)
- [ ]  Draft formulas for new mechanics (if designing)
- [ ]  Identify all parameters and their ranges
- [ ]  Create worked examples for each formula

### Output

**Mechanics & Formulas Draft**:

```
MECHANIC 1: [Mechanic Name]
  Formula:
    [Mathematical formula or pseudocode]

  Inputs:
    - [Input 1]: [Type, source]
    - [Input 2]: [Type, source]

  Outputs:
    - [Output]: [Type, destination]

  Parameters:
    - [Parameter 1]: [Type] = [Default] (Range: [Min-Max])
    - [Parameter 2]: [Type] = [Default] (Range: [Min-Max])

  Example:
    [Worked example with concrete values]

---

MECHANIC 2: [Mechanic Name]
  [Same structure]

```

### Example (Damage Calculation System)

```
MECHANIC 1: Physical Damage Calculation
  Formula:
    BaseDamage = Weapon.BaseDamage + (SuccessCount × AttributeModifier)
    FinalDamage = max(0, BaseDamage - TargetArmor)

  Inputs:
    - Weapon.BaseDamage: int (from equipment)
    - SuccessCount: int (from dice roll)
    - AttributeModifier: int (MIGHT for melee, FINESSE for ranged)
    - TargetArmor: int (from enemy stats)

  Outputs:
    - FinalDamage: int (applied to target HP)

  Parameters:
    - Weapon.BaseDamage: int = 10 (Range: 5-50)
    - AttributeModifier: int = 3 (Range: 1-6)
    - TargetArmor: int = 5 (Range: 0-30)

  Example:
    Weapon.BaseDamage = 10
    SuccessCount = 3
    AttributeModifier = 4 (MIGHT)
    TargetArmor = 5

    BaseDamage = 10 + (3 × 4) = 10 + 12 = 22
    FinalDamage = max(0, 22 - 5) = 17

---

MECHANIC 2: Critical Hit Multiplier
  Formula:
    CritDamage = BaseDamage × CritMultiplier

  Inputs:
    - BaseDamage: int (from base damage calculation)
    - CritMultiplier: float = 1.5 (configurable)

  Outputs:
    - CritDamage: int (applied instead of BaseDamage)

  Parameters:
    - CritMultiplier: float = 1.5 (Range: 1.0-3.0)

  Example:
    BaseDamage = 22
    CritMultiplier = 1.5

    CritDamage = 22 × 1.5 = 33

[... additional mechanics ...]

```

---

## Step 6: Check Setting Compliance

**Time**: 5-10 minutes

**Objective**: Ensure specification aligns with Aethelgard setting lore.

### Setting Compliance Review

**Read**: `SETTING_COMPLIANCE.md`

**Quick Compliance Check** (answer these):

1. Does this system use canonical terminology? (Aether not mana, Legend not XP, etc.)
2. Does this system respect established mysteries? (Counter-Rune paradox, Glitch nature, etc.)
3. Does this system avoid Pre-Glitch technology creation?
4. Does this system fit Layer 2 Diagnostic Voice for game text?
5. Does this system avoid generic fantasy tropes?

**Identify Applicable Domains** (from `SETTING_COMPLIANCE.md`):

- Domain 1: Cosmological & Metaphysical Fundamentals
- Domain 2: Aetheric Magic System
- Domain 3: Technological Fundamentals
- Domain 4: Entity & Species Fundamentals
- Domain 5: Immutable Historical Events
- Domain 6: Sociopolitical & Factional Landscape
- Domain 7: Reality & Logic Rules
- Domain 8: Geographic & Environmental Fundamentals
- Domain 9: Linguistic & Communicative Standards

### Actions

- [ ]  Read Quick Compliance Check in `SETTING_COMPLIANCE.md`
- [ ]  Answer 5 compliance questions
- [ ]  Identify applicable compliance domains
- [ ]  Review domain-specific checklists
- [ ]  Document compliance validation

### Output

**Setting Compliance Validation**:

```
Quick Compliance Check:
  1. Canonical terminology? [Yes/No] - [Notes]
  2. Respects mysteries? [Yes/No] - [Notes]
  3. No Pre-Glitch tech creation? [Yes/No] - [Notes]
  4. Layer 2 Diagnostic Voice? [Yes/No] - [Notes]
  5. No generic fantasy? [Yes/No] - [Notes]

Applicable Domains:
  - Domain [X]: [Name]
    - [Checklist item 1]: [Compliant]
    - [Checklist item 2]: [Compliant]
  - Domain [Y]: [Name]
    - [Checklist item 1]: [Compliant]

Violations Found:
  [None] OR [List violations and proposed fixes]

```

### Example (Damage Calculation System)

```
Quick Compliance Check:
  1. Canonical terminology? Yes - Uses "Aetheric damage" not "magic damage"
  2. Respects mysteries? Yes - No Glitch-related damage mechanics (out of scope)
  3. No Pre-Glitch tech creation? Yes - No tech damage types
  4. Layer 2 Diagnostic Voice? N/A - System spec, not narrative text
  5. No generic fantasy? Yes - Damage types are Physical/Aetheric/Corruption (setting-specific)

Applicable Domains:
  - Domain 2: Aetheric Magic System
    - ✅ Aetheric damage requires Aether Pool expenditure
    - ✅ Weaving-based damage uses WILL attribute
    - ✅ Runic focal points required for Aetheric attacks (per ability design)

Violations Found:
  None

```

---

## Step 7: Present Plan for Approval

**Time**: 5 minutes

**Objective**: Present planning document to user for approval before drafting.

### Plan Presentation Format

**Structure**:

```
# Planning Document: [System Name]

## Assignment Summary
[From Step 1]

## Exploration Results
[From Step 2 - key findings only]

## System Relationships
[From Step 3 - diagram or summary]

## Proposed Scope
IN SCOPE: [List]
OUT OF SCOPE: [List]

## Functional Requirements (Draft)
[FR-001 through FR-XXX - names and descriptions only]

## Key Mechanics
[Mechanic 1: Formula + Example]
[Mechanic 2: Formula + Example]

## Setting Compliance
[Validation summary - any issues flagged]

## Next Steps
If approved, I will proceed to draft the complete specification using TEMPLATE.md, including:
- Complete FR sections with acceptance criteria
- Detailed mechanics with all edge cases
- Integration points documentation
- Implementation guidance
- Balance & tuning parameters

Estimated drafting time: [X hours]

## Questions for User
[Any remaining ambiguities or design choices that need input]

```

### Actions

- [ ]  Compile all planning outputs into single document
- [ ]  Format for readability
- [ ]  Highlight any open questions or design choices
- [ ]  Present to user

### Output

**Planning Document** (present to user for approval)

---

## Planning Outputs & Next Steps

### Successful Planning Produces

✅ **Assignment Summary**: Clear understanding of what to specify
✅ **Exploration Notes**: Complete inventory of existing code/docs/data
✅ **Relationship Map**: Integration points identified
✅ **Scope Definition**: In/out boundaries clear
✅ **Functional Requirements Draft**: 5-12 FRs with priorities
✅ **Mechanics Draft**: Key formulas with examples
✅ **Setting Compliance Validation**: Confirmed compliant

### User Approval

**User responds with**:

1. ✅ **Approved**: Proceed to drafting (use `WORKFLOW_DRAFTING.md`)
2. 🔄 **Revise**: Make requested changes to plan
3. ❌ **Rejected**: Clarify requirements and restart planning

### After Approval

**Next Workflow**: `WORKFLOW_DRAFTING.md`

**Drafting Process**:

1. Copy `TEMPLATE.md` to new spec file
2. Fill in all sections using planning outputs
3. Expand FRs with acceptance criteria and examples
4. Document all mechanics with edge cases
5. Complete integration points section
6. Add implementation guidance
7. Complete all checklists
8. Mark as "Review" status

---

## Common Planning Pitfalls

### Pitfall 1: Skipping Exploration

**Problem**: Drafting without exploring codebase leads to misalignment with existing implementation.

**Solution**: Always run Step 2 exploration commands, even if you think you know the system.

### Pitfall 2: Scope Creep

**Problem**: Trying to specify too much in one spec (e.g., "Combat System" instead of "Damage Calculation").

**Solution**: Define narrow, focused scope. Split large systems into multiple specs.

### Pitfall 3: Vague Requirements

**Problem**: FRs like "System should handle damage" are too vague.

**Solution**: Be specific: "System must calculate Physical damage using Weapon.BaseDamage + (SuccessCount × MIGHT)."

### Pitfall 4: Ignoring Setting Compliance

**Problem**: Designing mechanics that violate Aethelgard lore (e.g., traditional spellcasting).

**Solution**: Read `SETTING_COMPLIANCE.md` BEFORE designing mechanics.

### Pitfall 5: No Worked Examples

**Problem**: Formulas without examples are ambiguous and error-prone.

**Solution**: Every formula needs at least one worked example with concrete numbers.

### Pitfall 6: Missing Integration Points

**Problem**: Designing system in isolation without considering dependencies.

**Solution**: Always map upstream dependencies and downstream dependents in Step 3.

### Pitfall 7: Not Asking Questions

**Problem**: Proceeding with assumptions when requirements are unclear.

**Solution**: Ask user for clarification in Step 1 or Step 7. Better to ask than guess wrong.

---

## Planning Checklist

Before presenting plan to user, verify:

### Assignment Clarity

- [ ]  System name is clear and specific
- [ ]  Domain identified (COMBAT, PROGRESSION, etc.)
- [ ]  User intent understood (document existing vs. design new)
- [ ]  Spec ID assigned (SPEC-XXX-YYY)

### Exploration Completeness

- [ ]  All related code files identified
- [ ]  All related documentation found
- [ ]  All related data files found
- [ ]  Integration points mapped
- [ ]  Tests identified (if exist)

### Relationship Mapping

- [ ]  Upstream dependencies listed
- [ ]  Downstream dependents listed
- [ ]  Parallel systems identified
- [ ]  No circular dependencies (or flagged if present)

### Scope Definition

- [ ]  In Scope clearly defined (3-8 items)
- [ ]  Out of Scope clearly defined (3-5 items)
- [ ]  Boundaries specific and unambiguous
- [ ]  Related specs referenced for excluded functionality

### Functional Requirements

- [ ]  5-12 FRs drafted
- [ ]  Each FR has name, description, rationale, priority
- [ ]  FRs are specific and testable
- [ ]  FRs cover all in-scope functionality
- [ ]  FRs trace to design goals

### Mechanics & Formulas

- [ ]  Key formulas identified for each FR
- [ ]  Formulas have inputs, outputs, parameters
- [ ]  Each formula has worked example
- [ ]  Parameters have defaults and ranges
- [ ]  Edge cases noted

### Setting Compliance

- [ ]  Quick Compliance Check completed
- [ ]  Applicable domains identified
- [ ]  Domain checklists reviewed
- [ ]  Violations flagged (or none found)
- [ ]  Compliance documented

### Presentation Quality

- [ ]  Planning document well-formatted
- [ ]  All sections complete
- [ ]  Open questions highlighted
- [ ]  Next steps clear
- [ ]  Ready for user review

---

## Template: Planning Document

```markdown
# Planning Document: [System Name]

**Spec ID**: SPEC-[DOMAIN]-[NUMBER]
**Domain**: [DOMAIN]
**Date**: [YYYY-MM-DD]
**Planner**: [AI/Human name]

---

## Assignment Summary

**System**: [System Name]
**User Intent**: [Document existing / Design new / Formalize]
**Priority**: High / Medium / Low
**Existing Resources**:
- [Resource 1]
- [Resource 2]

---

## Exploration Results

**Code Locations**:
- [File 1]: [Description]
- [File 2]: [Description]

**Documentation**:
- [Doc 1]: [Description]

**Data Files**:
- [File 1]: [Description]

**Integration Points**:
- Consumes: [System A], [System B]
- Consumed By: [System X], [System Y]

---

## System Relationships

**Upstream Dependencies**:
→ [System A]: [What we use] (SPEC-XXX-YYY)
→ [System B]: [What we use] (SPEC-XXX-YYY)

**Downstream Dependents**:
→ [System X]: [What they use] (SPEC-XXX-YYY)
→ [System Y]: [What they use] (SPEC-XXX-YYY)

---

## Proposed Scope

**IN SCOPE**:
- [Item 1]
- [Item 2]
- [Item 3]

**OUT OF SCOPE**:
- [Excluded 1] → SPEC-XXX-YYY
- [Excluded 2] → SPEC-XXX-YYY

---

## Functional Requirements (Draft)

### FR-001: [Requirement Name]
**Priority**: Critical
**Description**: [What system must do]
**Rationale**: [Why this is needed]

### FR-002: [Requirement Name]
**Priority**: High
**Description**: [What system must do]
**Rationale**: [Why this is needed]

[... continue for all FRs ...]

---

## Key Mechanics

### Mechanic 1: [Name]
**Formula**:

```

[Formula or pseudocode]

```

**Example**:

```

[Worked example with concrete values]

```

### Mechanic 2: [Name]
**Formula**:

```

[Formula or pseudocode]

```

**Example**:

```

[Worked example with concrete values]

```

---

## Setting Compliance Validation

**Quick Compliance Check**:
1. Canonical terminology? [Yes/No] - [Notes]
2. Respects mysteries? [Yes/No] - [Notes]
3. No Pre-Glitch tech creation? [Yes/No] - [Notes]
4. Layer 2 Diagnostic Voice? [Yes/No] - [Notes]
5. No generic fantasy? [Yes/No] - [Notes]

**Applicable Domains**:
- Domain [X]: [Name] - [Compliant]
- Domain [Y]: [Name] - [Compliant]

**Violations**: [None] OR [List violations]

---

## Next Steps

If approved, I will proceed to draft the complete specification using TEMPLATE.md.

**Estimated Drafting Time**: [X hours]

**Deliverables**:
- Complete specification with all FR sections
- Detailed mechanics with edge cases
- Integration points documentation
- Implementation guidance
- Balance & tuning parameters

---

## Questions for User

1. [Question 1 if any ambiguity]
2. [Question 2 if design choice needed]

---

**Ready for Approval**: Yes

```

---

**Document Version**: 1.0
**Maintained By**: Specification governance framework
**Feedback**: Update this workflow as planning patterns emerge