# START HERE: Specification Development Guide for AI Sessions

**Purpose**: This document is the central hub for AI sessions tasked with drafting feature/system/mechanic specifications for the Rune & Rust project.

**Last Updated**: 2025-11-19
**Target Audience**: AI assistants creating new specifications

---

## Quick Navigation

| Resource | Purpose | When to Use |
|----------|---------|-------------|
| **[This Document]** | Onboarding & workflow | Start every spec session here |
| **[SPEC_BACKLOG.md](./SPEC_BACKLOG.md)** | Spec registry & backlog | See what specs exist & what's needed |
| **[TEMPLATE.md](./TEMPLATE.md)** | Master specification template | Copy this to create new specs |
| **[README.md](./README.md)** | Complete writing guide (2,050 lines) | Deep dive on governance, walkthroughs |
| **[SETTING_COMPLIANCE.md](./SETTING_COMPLIANCE.md)** | Aethelgard lore validation | Check during draft & review |
| **[Example Specs](#example-specifications)** | Reference implementations | Model your work on these |

---

## AI Session Workflow (5 Steps)

### Step 1: Understand Your Assignment (5 minutes)

**Questions to answer before you start:**
1. What system/feature am I specifying? (e.g., "Status Effects System")
2. What domain does this belong to? (Combat, Progression, World, Narrative, Economy)
3. What existing systems does this integrate with?
4. Has the user provided context documents or files to reference?

**Check the backlog first:**
- Read **[SPEC_BACKLOG.md](./SPEC_BACKLOG.md)** to see if this spec is already planned
- If planned: Review "Proposed Scope" and "Dependencies" sections
- If not planned: Add an entry to the backlog after completing the spec
- Note the recommended Spec ID (SPEC-{DOMAIN}-{NUMBER})

**Action**: Read the user's request carefully and ask clarifying questions if:
- The scope is ambiguous (what's in vs. out of scope)
- There are multiple valid approaches (architectural choices)
- Integration points are unclear

---

### Step 2: Explore the Codebase (15-30 minutes)

**Use Walkthrough 1 from README.md**: [Analyzing an Existing System](./README.md#walkthrough-1-analyzing-an-existing-system)

**Critical exploration tasks:**

```bash
# Find related code
find RuneAndRust.Engine -name "*SystemName*.cs"
grep -r "SystemName" RuneAndRust.Engine/ --include="*.cs"

# Find existing documentation
ls -la docs/01-systems/*system-name*.md
grep -r "System Name" docs/01-systems/ docs/02-statistical-registry/

# Find integration points (events, services)
grep -n "SystemNameService" RuneAndRust.Engine/ -r
grep -n "OnEventName" RuneAndRust.Engine/ -r
```

**Deliverable**: Notes on:
- Existing code location (if any)
- Related systems and dependencies
- Existing documentation references
- Data structures and key classes

---

### Step 3: Plan the Specification (15-30 minutes)

**Use Walkthrough 2 from README.md**: [Planning a New Specification](./README.md#walkthrough-2-planning-a-new-specification)

**Key planning activities:**

1. **Define Scope**: What's in vs. out of scope?
2. **Identify Functional Requirements**: What must this system do? (Target: 5-12 FRs)
3. **Map Integration Points**: What systems does this consume? What consumes this?
4. **List Mechanics**: What formulas, calculations, or algorithms are involved?
5. **Check Setting Compliance**: Review [SETTING_COMPLIANCE.md](./SETTING_COMPLIANCE.md) for applicable domains

**Deliverable**: Outline with:
- Scope boundaries (in/out)
- Draft list of 5-12 functional requirements
- Integration point diagram (text-based)
- Preliminary mechanics list

**Present this plan to the user for approval before proceeding to drafting.**

---

### Step 4: Draft the Specification (2-4 hours)

**Use Walkthrough 4 from README.md**: [Drafting the Specification](./README.md#walkthrough-4-drafting-the-specification)

**Process:**

1. **Copy TEMPLATE.md** to `docs/00-specifications/{domain}/{system-name}-spec.md`
2. **Assign Spec ID**: `SPEC-{DOMAIN}-{NUMBER}` (check existing specs for next number)
3. **Work through template sections systematically**:
   - Document Control (version 1.0.0, status: Draft)
   - Executive Summary (1-2 sentences, scope, success criteria)
   - **Functional Requirements** (45-60 min) ← THE CORE
     - Each FR has: Description, Rationale, Acceptance Criteria, Examples, Dependencies
   - **System Mechanics** (45-60 min)
     - Formulas with worked examples
     - Parameter tables
     - Edge cases
   - Integration Points (15 min)
   - Implementation Guidance (30 min)
   - Balance & Tuning (20 min)
   - Validation & Testing (15 min)

4. **Use concrete examples from codebase**:
   - Actual attribute values (FINESSE, WILL, CORPUS, etc.)
   - Real dice mechanics (roll Xd6, count 5-6 as successes)
   - Existing service names (DiceService, SagaService, etc.)

5. **Reference existing specs for patterns**:
   - [SPEC-COMBAT-001](./combat/combat-resolution-spec.md) - Combat mechanics with initiative
   - [SPEC-PROGRESSION-001](./progression/character-progression-spec.md) - Economy with formulas
   - [SPEC-ECONOMY-003](./economy/trauma-economy-spec.md) - State tracking with thresholds

---

### Step 5: Validate & Review (20-30 minutes)

**Validation checklist:**

1. **Completeness** (use checklist at bottom of TEMPLATE.md):
   - [ ] All functional requirements have acceptance criteria
   - [ ] All mechanics have worked examples
   - [ ] All integration points documented
   - [ ] Implementation guidance provided for AI

2. **Setting Compliance** (use SETTING_COMPLIANCE.md):
   - [ ] Run Quick Compliance Check (critical "must not" questions)
   - [ ] Validate against applicable domain checklists
   - [ ] Check voice/tone (Layer 2 Diagnostic Voice for game text)
   - [ ] Verify canonical terminology

3. **Cross-References**:
   - [ ] Links to related specs (SPEC-XXX-XXX format)
   - [ ] Links to Layer 1 docs (implementation guides)
   - [ ] Links to code files (path:line format)

4. **Examples Quality**:
   - [ ] Every formula has worked example with real values
   - [ ] Every FR has concrete scenario
   - [ ] Edge cases explicitly handled

**Deliverable**: Completed specification ready for commit

---

## Example Specifications

Use these as models for different specification types:

### SPEC-COMBAT-001: Combat Resolution System
**File**: [combat/combat-resolution-spec.md](./combat/combat-resolution-spec.md)
**Lines**: ~600
**Best for**: Turn-based systems, state machines, event-driven mechanics
**Key Patterns**:
- Initiative calculation with tie-breaking
- State transitions (setup → active → resolution)
- Integration with multiple services (Dice, Saga, Loot, Hazard)

### SPEC-PROGRESSION-001: Character Progression System
**File**: [progression/character-progression-spec.md](./progression/character-progression-spec.md)
**Lines**: ~650
**Best for**: Economy systems, progression mechanics, resource management
**Key Patterns**:
- Multi-variable formulas (Legend = BLV × DM × TM)
- Threshold tables (Milestone progression)
- Hard caps and constraints (attributes ≤ 6)

### SPEC-ECONOMY-003: Trauma Economy System
**File**: [economy/trauma-economy-spec.md](./economy/trauma-economy-spec.md)
**Lines**: ~650
**Best for**: Accumulation systems, threshold mechanics, irreversible effects
**Key Patterns**:
- Resistance mechanics (WILL-based Resolve checks)
- Breaking points (stress ≥ 100 → trauma)
- No-recovery systems (Corruption)

---

## Critical Reminders for AI Sessions

### 1. Setting Compliance is Non-Negotiable

**Before writing any game content**:
- Read [SETTING_COMPLIANCE.md](./SETTING_COMPLIANCE.md) Quick Compliance Check
- Identify which of the 9 domains apply to your spec
- Use canonical terminology (Aether not mana, Legend not XP, Weaving not spellcasting)

**Common pitfalls**:
- ❌ Traditional spell-casting without runic focal points
- ❌ Pre-Glitch magic users or "Galdr" as entity type
- ❌ Creating new Data-Slates or programming Pre-Glitch systems
- ❌ Precision measurement tools for Jötun-Readers
- ❌ Resolving the Counter-Rune paradox (must remain mysterious)

### 2. Functional Requirements are the Core

**Every FR must have**:
- **Description**: What does this requirement specify?
- **Rationale**: Why does this exist? What player experience or design goal?
- **Acceptance Criteria**: How do we know it's implemented correctly? (Checklist format)
- **Examples**: Concrete scenarios with actual values
- **Dependencies**: What does this require? What does it block?

**Quality bar**: If a developer (human or AI) cannot implement the FR without asking questions, it's incomplete.

### 3. Worked Examples are Mandatory

**Every formula needs**:
```markdown
Formula:
  Result = BaseValue × Modifier

Example:
  BaseValue = 30 (standard melee enemy)
  Modifier = 1.25 (traumatic encounter)
  Result = 30 × 1.25 = 37.5 → 37 (floored)
```

**Not acceptable**:
```markdown
Formula:
  Result = BaseValue × Modifier
```

### 4. Integration Points Drive Implementation

**Document both directions**:
- **This system consumes**: What services/data does this system need?
  - Example: `CombatService` consumes `DiceService.RollDice()`
- **Systems that consume this**: What depends on this system's output?
  - Example: `SagaService` consumes `CombatService.OnCombatVictory` event

**Why this matters**: AI implementers need to know where to hook into existing code.

### 5. Implementation Guidance is for AI Implementers

**This section answers**:
- Where should this code live? (namespace, project)
- What architectural pattern? (service, manager, system)
- What existing code to reference?
- What common mistakes to avoid?

**Example**:
```markdown
## Implementation Guidance (for AI)

**Recommended Architecture**:
- **Namespace**: `RuneAndRust.Engine.Combat`
- **Primary Class**: `StatusEffectService` (service pattern)
- **Data Models**: `StatusEffect` (Core/Models/Combat/)

**Reference Implementation**:
See `RuneAndRust.Engine/Services/CombatEngine.cs:150-300` for event-driven
combat state management pattern.
```

---

## Specification ID Assignment

**Format**: `SPEC-{DOMAIN}-{NUMBER}`

**Domains**:
- `COMBAT` - Combat, initiative, damage, status effects
- `PROGRESSION` - Leveling, XP, milestones, attributes
- `ECONOMY` - Resources, currencies, costs, trauma, corruption
- `WORLD` - Exploration, travel, locations, hazards
- `NARRATIVE` - Story, quests, dialogue, lore integration

**Number Assignment**:
1. Check existing specs in your domain folder
2. Assign next sequential number (e.g., if SPEC-COMBAT-001 exists, use SPEC-COMBAT-002)
3. Update spec ID in Document Control table

**Current Spec Registry**:
- SPEC-COMBAT-001: Combat Resolution System
- SPEC-PROGRESSION-001: Character Progression System
- SPEC-ECONOMY-003: Trauma Economy System

---

## Common Questions

### Q: How detailed should specifications be?

**A**: Target 400-700 lines for a complete specification. Key sections:
- Functional Requirements: 200-300 lines (5-12 FRs with full examples)
- System Mechanics: 150-250 lines (formulas, tables, edge cases)
- Integration Points: 50-100 lines
- Implementation Guidance: 100-150 lines

See example specs for reference.

### Q: What if the system isn't implemented yet?

**A**: Perfect! Specifications define WHAT should exist and WHY (design intent), not HOW it's currently implemented. In the Implementation Guidance section, note "Status: Not yet implemented" and provide architecture recommendations for when it is built.

### Q: What if I find contradictions in existing documentation?

**A**: Flag them explicitly:
1. Note the contradiction in a `[DESIGN DECISION REQUIRED]` section
2. Present options to the user
3. Document the chosen resolution in the spec
4. Add a note to update the conflicting documentation

### Q: How do I handle edge cases?

**A**: Every mechanic section should include an "Edge Cases" subsection:
```markdown
### Edge Cases
1. **What if FINESSE is 0?**
   - Cannot roll 0d6, default to 1d6 for initiative
2. **What if two participants tie with same initiative AND same FINESSE?**
   - Randomize order for tied participants
```

### Q: Should I include balance tuning parameters?

**A**: Yes! The "Balance & Tuning" section documents:
- Which parameters are tunable vs. hard-coded
- Current values vs. target values
- What metrics to watch (player feedback, telemetry)
- What levers to pull if balance is off

---

## Getting Help

**If you encounter blockers**:

1. **Unclear requirements**: Ask the user for clarification before proceeding
2. **Missing codebase context**: Use Walkthrough 1 exploration commands to find relevant code
3. **Setting compliance questions**: Reference [SETTING_COMPLIANCE.md](./SETTING_COMPLIANCE.md) domain checklists
4. **Specification structure questions**: Check example specs for patterns

**Key resources**:
- **Complete writing guide**: [README.md](./README.md) (2,050 lines)
- **All walkthroughs**: [README.md - Walkthroughs section](./README.md#part-5-comprehensive-walkthroughs)
- **Setting fundamentals**: [SETTING_COMPLIANCE.md](./SETTING_COMPLIANCE.md)
- **Governance framework**: [README.md - Governance section](./README.md#specification-governance)

---

## Success Criteria for Your Specification

Your specification is ready for commit when:

- ✅ **Complete**: All template sections filled with meaningful content (not placeholder text)
- ✅ **Concrete**: Every formula has worked examples, every FR has scenarios
- ✅ **Setting-Compliant**: Passes Quick Compliance Check and domain validations
- ✅ **Implementable**: A developer could build this without asking clarifying questions
- ✅ **Integrated**: Documents both upstream dependencies and downstream consumers
- ✅ **Validated**: Includes acceptance criteria, test scenarios, and edge case handling

**Time estimate**: 2.5-5 hours for a complete, high-quality specification

---

## Ready to Start?

**Before you begin drafting**:
1. ✅ Read the user's assignment carefully
2. ✅ Run Step 2 codebase exploration
3. ✅ Complete Step 3 planning and get user approval
4. ✅ Review applicable setting compliance domains

**Then**:
1. Copy [TEMPLATE.md](./TEMPLATE.md) to your new spec file
2. Follow Step 4 drafting workflow
3. Validate with Step 5 checklist
4. Commit with descriptive message

**Good luck! Create specifications that are detailed, implementable, and setting-compliant.**

---

**Document Version**: 1.0.0
**Maintained by**: Specification governance framework
**Feedback**: Update this document as you discover better patterns or common issues
