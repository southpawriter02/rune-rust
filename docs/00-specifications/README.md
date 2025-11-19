# Specification Writing Guide

> **Layer 0: Feature & System Specifications**
>
> This directory contains high-level, non-technical specifications that define WHAT systems do and WHY they exist, independent of HOW they are implemented.

---

## Table of Contents

1. [Purpose & Philosophy](#purpose--philosophy)
2. [Specification Governance](#specification-governance)
3. [Directory Structure](#directory-structure)
4. [Writing Standards](#writing-standards)
5. [Specification Lifecycle](#specification-lifecycle)
6. [Checklists & Templates](#checklists--templates)
7. [Cross-Referencing System](#cross-referencing-system)
8. [AI Collaboration Guidelines](#ai-collaboration-guidelines)
9. [Common Patterns & Anti-Patterns](#common-patterns--anti-patterns)
10. [FAQ](#faq)

---

## Purpose & Philosophy

### What Are Specifications?

**Specifications** are design documents that:
- Define **WHAT** a system/feature does (functionality)
- Explain **WHY** it exists (design rationale, player experience goals)
- Are written **independently** of implementation details
- Serve as **contracts** between design intent and technical implementation
- Provide **continuity** across development sessions (human and AI)

### What Specifications Are NOT

Specifications are **not**:
- ❌ Implementation guides (that's Layer 1: Systems)
- ❌ Code documentation (that's Layer 3: Technical Reference)
- ❌ Balance tuning notes (that's Layer 5: Balance Reference)
- ❌ Statistical registries (that's Layer 2: Statistical Registry)
- ❌ Player-facing documentation (that's external wikis/manuals)

### Why Specifications Matter

**For Human Developers**:
- 📋 **Memory Aid**: Recall design decisions months later
- 🎯 **Focus**: Separate "what we want" from "how to build it"
- 🔄 **Iteration**: Evaluate changes against original goals
- 🧪 **Testing**: Verify implementation matches intent

**For AI Collaborators**:
- 🧠 **Context**: Understand system purpose and constraints
- ✅ **Consistency**: Implement features aligned with design philosophy
- 🔍 **Thoroughness**: Use checklists to ensure complete implementation
- 🔗 **Integration**: Understand dependencies and interaction points

**For the Project**:
- 📚 **Knowledge Base**: Centralized source of truth
- 🚀 **Onboarding**: New contributors understand systems quickly
- 🔬 **Analysis**: Identify gaps, redundancies, and opportunities
- 🎮 **Design Integrity**: Maintain coherent player experience

---

## Specification Governance

### Authority & Ownership

**Specification Owner**: The person/role responsible for maintaining specification accuracy and making design decisions.

**Roles**:
- **Author**: Creates initial specification
- **Owner**: Maintains specification, approves changes
- **Reviewers**: Provide feedback during review process
- **Implementers**: Use specification to build features
- **Stakeholders**: Affected by or interested in the system

### Approval Process

1. **Draft**: Author creates specification using template
2. **Self-Review**: Author completes all checklists
3. **Peer Review**: Reviewers provide feedback
4. **Revision**: Author addresses feedback
5. **Approval**: Owner marks specification as "Approved"
6. **Active**: Specification drives implementation
7. **Maintenance**: Ongoing updates as system evolves

### Change Management

**Minor Changes** (typos, clarifications, examples):
- Update version (e.g., 1.0 → 1.1)
- Document in version history
- No re-approval needed

**Major Changes** (requirements, mechanics, scope):
- Update version (e.g., 1.5 → 2.0)
- Re-enter review process
- Require owner approval
- Notify all stakeholders

**Deprecation**:
- Mark status as "Deprecated"
- Link to superseding specification
- Preserve for historical reference
- Move to archive after 1 version cycle

### Versioning Convention

**Format**: `MAJOR.MINOR`

- **MAJOR**: Incremented for significant changes (requirements, mechanics, scope)
- **MINOR**: Incremented for clarifications, examples, non-breaking updates

**Version History**:
- Every change must be logged in version history table
- Include: version, date, author, description, reviewers

---

## Directory Structure

```
docs/00-specifications/
├── README.md                    # This file - Specification Writing Guide
├── TEMPLATE.md                  # Master specification template
│
├── combat/                      # Combat-related specifications
│   ├── combat-resolution-spec.md
│   ├── status-effects-spec.md
│   ├── boss-encounters-spec.md
│   └── counter-attacks-spec.md
│
├── progression/                 # Character progression specifications
│   ├── character-progression-spec.md
│   ├── archetype-system-spec.md
│   ├── specialization-spec.md
│   └── legend-leveling-spec.md
│
├── world/                       # World generation & exploration
│   ├── procedural-generation-spec.md
│   ├── biome-system-spec.md
│   ├── room-design-spec.md
│   └── dungeon-graph-spec.md
│
├── narrative/                   # Narrative & presentation systems
│   ├── descriptor-framework-spec.md
│   ├── dialogue-system-spec.md
│   ├── faction-system-spec.md
│   └── quest-system-spec.md
│
└── economy/                     # Resource & economy systems
    ├── loot-system-spec.md
    ├── equipment-system-spec.md
    ├── crafting-spec.md
    └── trauma-economy-spec.md
```

### Naming Conventions

**File Names**:
- Format: `[system-name]-spec.md`
- Use kebab-case: `combat-resolution-spec.md`
- Be specific: `boss-encounters-spec.md` not `bosses-spec.md`
- Suffix with `-spec`: distinguishes from implementation docs

**Specification IDs**:
- Format: `SPEC-[DOMAIN]-[NUMBER]`
- Domain: COMBAT, PROGRESSION, WORLD, NARRATIVE, ECONOMY
- Number: Zero-padded 3-digit (001, 002, etc.)
- Examples: `SPEC-COMBAT-001`, `SPEC-PROGRESSION-002`

### Domain Categories

| Domain | Description | Examples |
|--------|-------------|----------|
| **COMBAT** | Combat mechanics, resolution, AI | Combat resolution, status effects, boss AI |
| **PROGRESSION** | Character growth, abilities, stats | Leveling, archetypes, specializations |
| **WORLD** | Procedural generation, exploration | Dungeon generation, biomes, room design |
| **NARRATIVE** | Story, dialogue, flavor text | Factions, quests, descriptors, dialogue |
| **ECONOMY** | Resources, loot, crafting, trading | Loot tables, equipment, crafting recipes |

---

## Writing Standards

### Language & Style

**Voice**:
- Use **active voice**: "The system calculates damage" not "Damage is calculated"
- Use **present tense**: "Player selects ability" not "Player will select ability"
- Use **imperative** for requirements: "The system must track HP" not "It would be good if..."

**Clarity**:
- **Be specific**: "Roll 5d6" not "Roll some dice"
- **Be concrete**: "Reduce HP by 10" not "Reduce HP significantly"
- **Avoid ambiguity**: Define edge cases, special conditions
- **Use examples**: Show don't just tell

**Consistency**:
- **Terminology**: Use project glossary (see Appendix)
- **Formatting**: Follow template structure
- **Units**: Always specify (seconds, points, percentage)
- **Capitalization**: Consistent for game terms (e.g., "Legend" for XP system)

### Formatting Standards

**Headers**:
- Use ATX-style headers (`#`, `##`, `###`)
- One H1 (`#`) per document (title only)
- Logical hierarchy (don't skip levels)

**Lists**:
- **Unordered**: Use `-` for bullet points
- **Ordered**: Use `1.`, `2.`, `3.` (markdown auto-numbers)
- **Checklists**: Use `- [ ]` for incomplete, `- [x]` for complete

**Code Blocks**:
- Use triple backticks with language: ```csharp, ```sql, ```json
- Use single backticks for inline code: `VariableName`
- Use pseudocode for language-agnostic logic

**Tables**:
- Use markdown tables with header row
- Align columns for readability
- Keep tables focused (max 6-7 columns)

**Emphasis**:
- **Bold**: For important terms, warnings, emphasis
- *Italic*: For citations, technical terms, slight emphasis
- `Code`: For variables, functions, file names
- > Blockquotes: For notes, examples, templates

### Required vs. Optional Sections

**Always Required**:
- ✅ Document Control (version history, stakeholders)
- ✅ Executive Summary (purpose, scope, success criteria)
- ✅ Functional Requirements (at least 3-5)
- ✅ System Mechanics (core functionality)
- ✅ Integration Points (dependencies)
- ✅ Implementation Guidance (for AI/implementers)

**Conditionally Required**:
- ⚡ State Management (if system maintains state)
- ⚡ Data Requirements (if system processes/stores data)
- ⚡ Balance & Tuning (if system has tunable parameters)
- ⚡ Non-Functional Requirements (if performance/usability critical)

**Optional**:
- 🔹 User Experience Flow (helpful for player-facing features)
- 🔹 Testing & Verification (if complex testing needed)
- 🔹 Appendices (diagrams, math, terminology)

**When to Omit**:
- If section is genuinely not applicable, mark as `## [Section] (N/A - [reason])`
- Don't delete the section header; this shows you considered it

---

## Specification Lifecycle

### 1. Planning Phase

**Before Writing**:
- [ ] Identify the system/feature to specify
- [ ] Gather existing documentation (code, notes, PRs)
- [ ] Identify stakeholders (who cares about this?)
- [ ] Define scope boundaries (what's in/out)
- [ ] Choose specification ID (SPEC-[DOMAIN]-[NUMBER])

**Resource Gathering**:
- Review existing implementation (if any)
- Review related specifications
- Review Layer 1 system docs
- Review code (`RuneAndRust.Engine/`, `RuneAndRust.Core/`)
- Review data files (`Data/`, SQL schemas)

### 2. Drafting Phase

**Process**:
1. Copy `TEMPLATE.md` to appropriate domain directory
2. Rename to `[system-name]-spec.md`
3. Fill in Document Control section
4. Write Executive Summary (purpose, scope, goals)
5. List Functional Requirements (what it must do)
6. Describe System Mechanics (how it works)
7. Document Integration Points (dependencies)
8. Add Implementation Guidance (for builders)
9. Complete all applicable sections
10. Work through Document Completeness Checklist

**Tips**:
- Start with Executive Summary to clarify thinking
- Write Functional Requirements before Mechanics
- Use examples liberally
- Reference existing code/data to ground in reality
- Mark uncertain areas with `[TBD]` or `[QUESTION]`

### 3. Review Phase

**Self-Review**:
- [ ] Run through Document Completeness Checklist
- [ ] Verify all cross-references are valid
- [ ] Check spelling, grammar, formatting
- [ ] Ensure examples are accurate
- [ ] Confirm code references are correct
- [ ] Test any formulas with example calculations

**Peer Review** (if applicable):
- Share with stakeholders
- Collect feedback
- Address questions/concerns
- Revise specification

### 4. Approval Phase

**Approval Criteria**:
- All required sections complete
- All checklists satisfied
- All `[TBD]` placeholders resolved
- Stakeholders consulted
- Technical feasibility confirmed

**Approval Actions**:
- Change status from "Draft" to "Approved"
- Update version history
- Announce to team/stakeholders
- Begin implementation

### 5. Active Phase

**During Implementation**:
- Implementers reference specification
- Specification guides design decisions
- Discrepancies between spec and implementation are resolved:
  - If spec is wrong: Update spec (major version)
  - If implementation is wrong: Fix implementation

**Maintenance**:
- Update as systems evolve
- Document changes in version history
- Keep aligned with actual implementation
- Mark open questions as resolved

### 6. Deprecation Phase

**When to Deprecate**:
- System removed from game
- System completely redesigned (new spec created)
- Specification superseded by improved version

**Deprecation Process**:
1. Change status to "Deprecated"
2. Add deprecation notice at top
3. Link to replacement specification (if any)
4. Update version history with deprecation date
5. Move to archive after one version cycle

---

## Checklists & Templates

### Pre-Writing Checklist

**Before creating a new specification**:
- [ ] System/feature is well-defined enough to specify
- [ ] System is significant enough to warrant specification (not trivial)
- [ ] No existing specification covers this (check directory)
- [ ] Scope boundaries are clear (in/out of scope)
- [ ] Stakeholders identified
- [ ] Specification ID assigned (SPEC-[DOMAIN]-[NUMBER])

### Writing Checklist

**While writing specification**:
- [ ] Using official template (`TEMPLATE.md`)
- [ ] Following naming convention (`[system-name]-spec.md`)
- [ ] Document Control section filled out
- [ ] Executive Summary written (purpose, scope, goals)
- [ ] At least 3-5 Functional Requirements defined
- [ ] Each requirement has acceptance criteria
- [ ] System Mechanics explained with examples
- [ ] Formulas include example calculations
- [ ] Integration Points identified
- [ ] Implementation Guidance provided
- [ ] All cross-references valid (file paths, line numbers)
- [ ] All placeholders resolved (`[TBD]`, `[QUESTION]`)
- [ ] Examples concrete and accurate

### Completeness Checklist

**Before marking as "Review" status**:

#### Structure
- [ ] All required sections present
- [ ] Version history populated
- [ ] Stakeholders identified
- [ ] Related documentation linked
- [ ] Specification ID assigned

#### Content
- [ ] Executive summary complete
- [ ] All functional requirements documented (min 3-5)
- [ ] All mechanics explained with examples
- [ ] Integration points identified
- [ ] Balance targets defined (if applicable)
- [ ] Implementation guidance provided

#### Quality
- [ ] Technical accuracy verified
- [ ] No ambiguous language
- [ ] Examples provided for complex concepts
- [ ] Cross-references valid
- [ ] Formatting consistent
- [ ] Spelling/grammar checked

#### Traceability
- [ ] All requirements have IDs (FR-001, FR-002, etc.)
- [ ] All mechanics trace to requirements
- [ ] All code references valid
- [ ] All test scenarios cover requirements

#### Completeness
- [ ] All `[TBD]` placeholders resolved
- [ ] All open questions addressed or tracked
- [ ] All stakeholders consulted (if applicable)
- [ ] Implementation feasibility confirmed

### Review Checklist

**For reviewers**:

#### Design
- [ ] Purpose clearly stated
- [ ] Design goals reasonable
- [ ] Player experience well-defined
- [ ] Scope appropriate (not too broad/narrow)

#### Requirements
- [ ] Requirements complete (nothing missing)
- [ ] Requirements testable (can verify)
- [ ] Requirements unambiguous (no confusion)
- [ ] Requirements traceable (linked to goals)

#### Mechanics
- [ ] Mechanics clearly explained
- [ ] Formulas correct and well-documented
- [ ] Parameters have ranges and defaults
- [ ] Edge cases identified
- [ ] Examples accurate

#### Integration
- [ ] Dependencies identified
- [ ] Integration points clear
- [ ] Data flow documented
- [ ] Event handling specified

#### Implementation
- [ ] Guidance sufficient for implementation
- [ ] Code architecture appropriate
- [ ] Data requirements clear
- [ ] Testing approach sound

---

## Cross-Referencing System

### Why Cross-Reference?

Cross-references connect specifications to:
- Other specifications (dependencies, related systems)
- Implementation documentation (Layer 1: Systems)
- Code (actual implementation)
- Data (schemas, JSON files)
- Tests (verification)

This creates a **knowledge graph** enabling navigation between "what" (spec) and "how" (implementation).

### Cross-Reference Format

**To Specifications**:
```markdown
`SPEC-COMBAT-001` - Combat Resolution System
See also: `SPEC-PROGRESSION-002` for how damage scales with Legend
```

**To Documentation**:
```markdown
**Implementation Guide**: `docs/01-systems/combat-resolution.md`
**Balance Data**: `docs/05-balance-reference/damage-analysis.md`
```

**To Code**:
```markdown
**Service**: `RuneAndRust.Engine/CombatEngine.cs:1234`
**Model**: `RuneAndRust.Core/PlayerCharacter.cs:56`
**Factory**: `RuneAndRust.Engine/EnemyFactory.cs:890`
```

**To Data Files**:
```markdown
**Schema**: `Data/v0.38.12_combat_mechanics_schema.sql`
**Config**: `Data/BiomeConfigs/muspelheim.json`
```

**To Tests**:
```markdown
**Unit Tests**: `RuneAndRust.Tests/CombatEngineTests.cs:45`
**Integration Tests**: `RuneAndRust.Tests/CombatIntegrationTests.cs:123`
```

### Cross-Reference Best Practices

**DO**:
- ✅ Use relative paths from project root
- ✅ Include line numbers for code references (when specific)
- ✅ Update cross-references when files move
- ✅ Link bidirectionally (spec → code AND code → spec)
- ✅ Verify links are valid before committing

**DON'T**:
- ❌ Use absolute file system paths (`/home/user/...`)
- ❌ Link to external URLs (unless stable references)
- ❌ Create circular dependencies (A depends on B depends on A)
- ❌ Let references go stale (update when refactoring)

### Link Verification

**Manual Verification**:
```bash
# Check if file exists
ls -la RuneAndRust.Engine/CombatEngine.cs

# Check if line number is approximately correct
head -n 1234 RuneAndRust.Engine/CombatEngine.cs | tail -n 5
```

**Automated Verification** (future):
- Script to validate all cross-references
- Report broken links
- Suggest corrections

---

## AI Collaboration Guidelines

### Context Provision for AI

When working with AI to implement a system:

1. **Provide Specification**: Share the complete specification
2. **Set Expectations**: "Implement according to SPEC-COMBAT-001"
3. **Reference Checklists**: "Use implementation checklist from spec"
4. **Point to Examples**: "Follow code examples in Implementation Guidance"
5. **Verify Alignment**: "Does this implementation satisfy FR-003?"

### Using Specifications to Guide AI

**Good Prompts**:
```
"Implement the Combat Resolution System according to SPEC-COMBAT-001.
Use the implementation checklist to ensure completeness. Follow the
code architecture recommendations in the specification."
```

```
"Review the current CombatEngine implementation against SPEC-COMBAT-001.
Identify any discrepancies between the specification requirements and
the actual implementation."
```

```
"Create unit tests for the Status Effects system that verify all
acceptance criteria in SPEC-COMBAT-002, FR-001 through FR-005."
```

**Poor Prompts**:
```
"Make a combat system" (no spec reference, too vague)
"Implement combat like other games" (no project-specific guidance)
"Add some combat features" (no requirements, no verification criteria)
```

### AI Implementation Workflow

**Step 1: Context Loading**
```
AI: Read SPEC-[DOMAIN]-[NUMBER]
AI: Read related specifications
AI: Review existing code structure
AI: Identify dependencies
```

**Step 2: Planning**
```
AI: Create implementation plan based on spec
AI: Identify code files to create/modify
AI: Note integration points
AI: Flag any specification ambiguities
```

**Step 3: Implementation**
```
AI: Implement per specification requirements
AI: Follow code architecture guidance
AI: Use checklists to ensure completeness
AI: Add code comments referencing spec sections
```

**Step 4: Verification**
```
AI: Verify each FR acceptance criteria met
AI: Run through implementation checklist
AI: Create tests covering requirements
AI: Update specification with implementation status
```

### Handling Specification Ambiguities

If AI encounters ambiguity:

1. **Flag It**: Note the ambiguous section
2. **Propose Interpretations**: Offer 2-3 reasonable interpretations
3. **Request Clarification**: Ask human to clarify
4. **Document Decision**: Update spec with clarified intent
5. **Proceed**: Implement with confirmed interpretation

**Example**:
```
AI: "SPEC-COMBAT-001, FR-003 states 'system calculates damage based on
successes.' Does this mean:
  A) Linear: Damage = Successes × BaseDamage
  B) Stepped: Damage = BaseDamage + (Successes × Bonus)
  C) Exponential: Damage = BaseDamage × (1.5 ^ Successes)

The specification doesn't clarify. Please confirm intended formula."
```

### Continuous Alignment

Throughout implementation:
- Reference specification frequently
- Check off implemented requirements
- Update "Implementation Status" in spec
- Note any deviations or discoveries
- Propose spec updates when implementation reveals issues

---

## Common Patterns & Anti-Patterns

### Specification Patterns (GOOD)

#### Pattern 1: Concrete Examples
**DO**:
```markdown
### Mechanic 1: Damage Calculation

**Formula**: `Damage = (Successes × BaseDamage) - TargetArmor`

**Example**:
  Given:
    Successes = 3
    BaseDamage = 5
    TargetArmor = 2

  Result:
    Damage = (3 × 5) - 2 = 13
```

**Why**: Removes ambiguity, easy to verify implementation.

---

#### Pattern 2: Layered Detail
**DO**:
```markdown
### FR-001: Track Player Health

**Description**: System must track player hit points (HP) from 0 to maximum.

**Details**:
- HP starts at maximum (determined by STURDINESS attribute)
- HP cannot go below 0
- HP cannot exceed maximum
- HP changes trigger events for UI updates

**Edge Cases**:
- Healing when at max HP: No effect, no overheal
- Damage when at 0 HP: Player is already defeated
- Maximum HP changes: Current HP adjusts proportionally
```

**Why**: Progressively reveals detail, covers edge cases.

---

#### Pattern 3: Bidirectional Cross-References
**DO**:

In specification:
```markdown
**Implementation**: `RuneAndRust.Engine/CombatEngine.cs:1234`
```

In code:
```csharp
/// <summary>
/// Calculates damage based on dice successes.
/// Implements FR-003 from SPEC-COMBAT-001.
/// </summary>
public int CalculateDamage(int successes, int baseDamage)
{
    // Implementation per SPEC-COMBAT-001
}
```

**Why**: Enables navigation both directions (design ↔ implementation).

---

### Specification Anti-Patterns (BAD)

#### Anti-Pattern 1: Vague Requirements
**DON'T**:
```markdown
### FR-001: Combat Feels Good
The combat system should feel responsive and fun.
```

**Why**: Not measurable, not testable, not implementable.

**FIX**:
```markdown
### FR-001: Combat Provides Immediate Feedback
**Description**: When player attacks, system provides feedback within 100ms.

**Acceptance Criteria**:
- [ ] Dice roll animation completes in ≤ 500ms
- [ ] Damage numbers appear within 100ms of resolution
- [ ] UI updates within 50ms of HP change

**Player Experience Goal**: Combat feels responsive and engaging.
```

---

#### Anti-Pattern 2: Implementation Details in Spec
**DON'T**:
```markdown
### Mechanic 1: Damage Calculation

The CombatEngine class should have a CalculateDamage method that
takes int parameters and returns an int. It should use a for loop
to iterate through the dice results array...
```

**Why**: Specification dictates HOW (implementation), not WHAT (functionality).

**FIX**:
```markdown
### Mechanic 1: Damage Calculation

**Description**: System calculates damage by multiplying successful
dice rolls by base damage value, then subtracting target armor.

**Formula**: `Damage = (Successes × BaseDamage) - TargetArmor`

**Parameters**:
- Successes: Number of 5-6 results from dice roll
- BaseDamage: Damage value from ability or weapon
- TargetArmor: Damage reduction from target

**Implementation Notes**: See `docs/01-systems/combat-resolution.md`
for technical implementation guidance.
```

---

#### Anti-Pattern 3: Missing Edge Cases
**DON'T**:
```markdown
### Mechanic 1: HP Modification

When damage is dealt, subtract damage from HP.
```

**Why**: Doesn't cover boundary conditions, undefined behavior.

**FIX**:
```markdown
### Mechanic 1: HP Modification

**Normal Case**: Subtract damage from current HP

**Edge Cases**:
1. **Damage > Current HP**: Set HP to 0, trigger defeat
2. **Damage = 0**: No change, log event
3. **Damage < 0 (healing)**: Add to HP, cap at maximum
4. **Current HP = 0**: Already defeated, no further damage
```

---

#### Anti-Pattern 4: Circular Dependencies
**DON'T**:
```markdown
SPEC-COMBAT-001: Combat System
  Depends on: SPEC-PROGRESSION-001 (Legend System)

SPEC-PROGRESSION-001: Legend System
  Depends on: SPEC-COMBAT-001 (Combat System)
```

**Why**: Creates chicken-and-egg problem, unclear implementation order.

**FIX**:
Break circular dependency by extracting shared concept:
```markdown
SPEC-SHARED-001: Core Attributes System
  (defines MIGHT, FINESSE, etc.)

SPEC-COMBAT-001: Combat System
  Depends on: SPEC-SHARED-001

SPEC-PROGRESSION-001: Legend System
  Depends on: SPEC-SHARED-001
```

---

## FAQ

### General Questions

**Q: How detailed should specifications be?**
A: Detailed enough that an AI or new developer can implement the system correctly without guessing. Include concrete examples, edge cases, and acceptance criteria. Err on the side of more detail.

**Q: What if the system is already implemented?**
A: Specifications can be written retroactively to document existing systems. This is valuable for maintenance, refactoring, and ensuring AI understands the system's purpose.

**Q: Do I need a specification for every system?**
A: Prioritize specifications for:
  - Complex systems (multiple interacting parts)
  - Core gameplay loops (combat, progression)
  - Unique/novel mechanics (trauma economy)
  - Systems with many dependencies (integration-heavy)

Small, simple systems may not need full specifications.

**Q: Can specifications evolve?**
A: Yes! Specifications should evolve as understanding deepens. Use version control, document changes, and maintain version history.

---

### Writing Questions

**Q: What if I'm unsure about a requirement?**
A: Mark it with `[TBD - reason]` or `[QUESTION: clarification needed]`. Document the uncertainty in "Open Questions" section. Resolve before marking spec as "Approved."

**Q: How do I handle systems with many mechanics?**
A: Break into multiple specifications if needed:
  - `combat-resolution-spec.md` (core combat loop)
  - `status-effects-spec.md` (buffs/debuffs)
  - `boss-encounters-spec.md` (special AI)

Link them with cross-references.

**Q: What if a section doesn't apply?**
A: Don't delete it. Mark as `## [Section Name] (N/A - [reason])`. This shows you considered it.

**Q: How do I document formulas?**
A: Use plain text math notation, provide concrete examples, show step-by-step calculation. Include units. Example:

```markdown
**Formula**: `DPS = (AvgDamage × HitRate) / AttackSpeed`

**Example**:
  Given:
    AvgDamage = 15 points
    HitRate = 0.75 (75%)
    AttackSpeed = 2 seconds

  Calculation:
    DPS = (15 × 0.75) / 2
    DPS = 11.25 / 2
    DPS = 5.625 points per second
```

---

### Process Questions

**Q: When should I create a specification?**
A: **Before implementing** (ideal) or **after implementing** (documentation). Specifications are valuable at any stage but most effective before implementation.

**Q: Who approves specifications?**
A: The specification owner (usually the designer/lead for that system). For solo projects, self-approval is fine, but still follow the process for discipline.

**Q: How do I handle conflicting specifications?**
A: Identify the conflict, document it in "Open Questions," and resolve through:
  1. Design discussion (which approach is better?)
  2. Refactoring (extract shared dependency)
  3. Prioritization (which system's needs are more important?)

Update both specifications with resolution.

**Q: What if implementation reveals spec is wrong?**
A: Update the specification! Implementation often uncovers edge cases or impracticalities. This is expected and healthy. Document the change in version history.

---

### AI Collaboration Questions

**Q: Should AI read specifications before implementing?**
A: **Yes, always!** Provide the specification as context. This ensures AI understands the design intent, requirements, and constraints.

**Q: Can AI help write specifications?**
A: Yes! AI can:
  - Draft specifications based on existing code
  - Identify missing requirements
  - Suggest test scenarios
  - Create examples
  - Check completeness

But human review is essential for design decisions.

**Q: What if AI deviates from specification?**
A: Either:
  A) AI made a mistake → Fix implementation
  B) Specification was unclear/wrong → Update specification

Always reconcile specification and implementation.

**Q: How do I ensure AI implements completely?**
A: Reference the "Implementation Checklist" in the specification. Ask AI to verify each checklist item. Review "Implementation Status" section.

---

## Appendix: Project-Specific Terminology

| Term | Definition | Usage |
|------|------------|-------|
| **Legend** | Experience/progression system (NOT "XP") | "Player gains Legend from defeating enemies" |
| **Aether** | In-world term for magical energy (NOT "mana") | "Mystics spend Aether to cast abilities" |
| **Corruption** | Permanent sanity damage (0-100) | "Using heretical abilities increases Corruption" |
| **Psychic Stress** | Temporary sanity damage (0-100, recoverable) | "Environmental stress increases Psychic Stress" |
| **Trauma** | Permanent debuffs from breaking points | "At 100 Stress, gain a random Trauma" |
| **Glitch** | The apocalyptic event that broke the world | "800 years after the Glitch" |
| **Layer 2 Diagnostic Voice** | Technical/machine-like narration style | "Use clinical terminology, avoid flowery prose" |
| **Success** | 5-6 result on a d6 | "Roll 5d6, count successes (5-6)" |
| **Dice Pool** | XdY dice rolled together | "Attack uses MIGHT dice pool (5d6)" |
| **Archetype** | Core character class (Warrior/Skirmisher/Mystic) | "Player chooses Archetype at creation" |
| **Specialization** | Sub-class specialization | "Vard-Warden is a Mystic Specialization" |
| **Progression Point (PP)** | Currency for buying abilities/upgrades | "Spend 1 PP to unlock new ability" |
| **Milestone** | Major story/progression checkpoint | "Reaching Milestone 2 unlocks Tier 2 abilities" |
| **Quality Tier** | Equipment rarity (Jury-Rigged → Myth-Forged) | "Boss drops Myth-Forged quality gear" |

---

## Document Version

**Version**: 1.0
**Last Updated**: 2025-11-19
**Author**: AI + Human Collaboration
**Status**: Active

---

**End of Specification Writing Guide**
