# Specification Writing Guide

> **Layer 0: Feature & System Specifications**
>
> This directory contains high-level, non-technical specifications that define WHAT systems do and WHY they exist, independent of HOW they are implemented.

---

## 🚀 Quick Start for AI Sessions

**New to drafting specifications?** Start here: **[START_HERE.md](./START_HERE.md)**

This quick-start guide provides:
- **5-step workflow** for creating specifications (onboarding → exploration → planning → drafting → validation)
- **Example specifications** to model your work on
- **Critical reminders** for setting compliance and quality standards
- **Quick navigation** to all key resources

**Looking for what to work on?** See **[SPEC_BACKLOG.md](./SPEC_BACKLOG.md)** for:
- **Registry of all specifications** (3 completed, 34 planned, 37 total)
- **Priority matrix** and recommended drafting order
- **Dependencies** between specifications
- **Proposed scope** for each planned spec

**Experienced?** Jump directly to:
- **[TEMPLATE.md](./TEMPLATE.md)** - Copy this to create new specs
- **[SETTING_COMPLIANCE.md](./SETTING_COMPLIANCE.md)** - Validate against Aethelgard canon
- **[Walkthroughs](#comprehensive-walkthroughs)** (below) - Detailed step-by-step processes

---

## Table of Contents

1. [Purpose & Philosophy](#purpose--philosophy)
2. [Specification Governance](#specification-governance)
3. [Directory Structure](#directory-structure)
4. [Writing Standards](#writing-standards)
5. [Specification Lifecycle](#specification-lifecycle)
6. [Comprehensive Walkthroughs](#comprehensive-walkthroughs)
   - [Walkthrough 1: Analyzing an Existing System](#walkthrough-1-analyzing-an-existing-system)
   - [Walkthrough 2: Planning a New Specification](#walkthrough-2-planning-a-new-specification)
   - [Walkthrough 3: Mapping System Relationships](#walkthrough-3-mapping-system-relationships)
   - [Walkthrough 4: Drafting the Specification](#walkthrough-4-drafting-the-specification)
   - [Walkthrough 5: Leveraging Existing Documentation](#walkthrough-5-leveraging-existing-documentation)
7. [Checklists & Templates](#checklists--templates)
8. [Cross-Referencing System](#cross-referencing-system)
9. [AI Collaboration Guidelines](#ai-collaboration-guidelines)
10. [Common Patterns & Anti-Patterns](#common-patterns--anti-patterns)
11. [FAQ](#faq)

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

## Comprehensive Walkthroughs

> **Purpose**: Step-by-step workflows for creating thorough, well-researched specifications by systematically analyzing systems, mapping relationships, and leveraging existing documentation.

---

### Walkthrough 1: Analyzing an Existing System

**Goal**: Deeply understand a system before specifying it

**When to Use**: When documenting an existing system (retroactive specification)

---

#### Step 1: Identify the System Scope

**Questions to Answer**:
- What is the official name of this system?
- What player-facing problem does it solve?
- When/where does the player interact with this system?
- What are the clear boundaries (in scope vs out of scope)?

**Actions**:
```bash
# Search for system references in code
cd /home/user/rune-rust
grep -r "SystemName" RuneAndRust.Engine/ --include="*.cs"
grep -r "SystemName" RuneAndRust.Core/ --include="*.cs"

# Find related documentation
ls docs/01-systems/ | grep -i "system"
ls docs/02-statistical-registry/ | grep -i "system"
```

**Output**: System name, scope definition, boundary list

---

#### Step 2: Map the Code Implementation

**Questions to Answer**:
- Which service class implements this system?
- Which models/POCOs store system data?
- Which database tables persist system state?
- What are the entry points (public methods)?

**Actions**:
```bash
# Find service implementation
find RuneAndRust.Engine -name "*SystemName*Service.cs"

# Find models
find RuneAndRust.Core -name "*SystemName*.cs"

# Find database schemas
find Data -name "*system*.sql" | sort

# Count lines of code (complexity indicator)
wc -l RuneAndRust.Engine/SystemNameService.cs
```

**Read Key Files**:
1. Service file (main business logic)
2. Model files (data structures)
3. Related factories/databases

**Output**: File paths, line counts, entry point methods

---

#### Step 3: Extract Functional Requirements

**Questions to Answer**:
- What MUST the system do? (core requirements)
- What edge cases exist?
- What failure modes are handled?
- What validation rules apply?

**Actions**:
1. **Read service public methods**:
   - Each public method likely maps to a functional requirement
   - Method signature reveals inputs/outputs
   - XML comments (if present) explain purpose

2. **Trace a typical execution path**:
   - Pick common use case (e.g., "player uses ability")
   - Follow code from entry point to completion
   - Note all validation checks, state changes, integrations

3. **Identify edge cases in code**:
   ```csharp
   // Look for patterns like:
   if (value <= 0) return; // Edge: zero/negative input
   if (count >= MAX) throw; // Edge: overflow
   if (resource == null) // Edge: missing dependency
   ```

**Output**: List of 5-10 functional requirements with acceptance criteria

---

#### Step 4: Document System Mechanics

**Questions to Answer**:
- What formulas/algorithms drive the system?
- What parameters are configurable?
- How do inputs transform to outputs?
- What random elements exist (dice rolls, RNG)?

**Actions**:
1. **Extract formulas from code**:
   ```csharp
   // Example: Find damage calculation
   var damage = (successes * baseDamage) - targetArmor;

   // Convert to specification format:
   // Damage = (Successes × BaseDamage) - TargetArmor
   ```

2. **Identify tunable parameters**:
   - Look for hardcoded constants: `const int MAX_HP = 100;`
   - Look for database lookups: `_database.GetBaseDamage(weaponId)`
   - Check configuration files: JSON, SQL seed data

3. **Create worked examples**:
   - Use actual values from code/database
   - Show step-by-step calculation
   - Verify against unit tests (if available)

**Output**: Formulas with parameters, worked examples, parameter tables

---

#### Step 5: Map Integration Points

**Questions to Answer**:
- Which systems does this system CALL (dependencies)?
- Which systems CALL this system (dependents)?
- What events does it publish?
- What events does it subscribe to?

**Actions**:
1. **Find constructor dependencies** (injected services):
   ```csharp
   public SystemService(
       DependencyA depA,  // This system uses DependencyA
       DependencyB depB   // This system uses DependencyB
   )
   ```

2. **Search for event subscriptions**:
   ```bash
   grep -n "OnEventName +=" RuneAndRust.Engine/SystemService.cs
   ```

3. **Search for event publications**:
   ```bash
   grep -n "OnEventName?.Invoke" RuneAndRust.Engine/SystemService.cs
   ```

4. **Find calling code** (who uses this system):
   ```bash
   grep -r "new SystemService" RuneAndRust.Engine/
   grep -r "_systemService\." RuneAndRust.Engine/
   ```

**Output**: Dependency graph, event list, integration notes

---

#### Step 6: Gather Statistical Data

**Questions to Answer**:
- What are typical/min/max values?
- What probabilities govern outcomes?
- What balance targets exist?

**Actions**:
1. **Check existing Layer 1 docs**: `docs/01-systems/system-name.md`
2. **Check statistical registry**: `docs/02-statistical-registry/`
3. **Check balance reference**: `docs/05-balance-reference/`
4. **Extract from database**:
   ```bash
   # Example: Find all enemy HP values
   sqlite3 Data/game.db "SELECT name, hp FROM enemies ORDER BY hp;"
   ```

5. **Read unit tests** for expected values:
   ```bash
   grep -A 5 "Assert.AreEqual" RuneAndRust.Tests/SystemServiceTests.cs
   ```

**Output**: Value ranges, probability tables, balance notes

---

#### Step 7: Review Player Experience

**Questions to Answer**:
- How does the player perceive this system?
- What feedback does the system provide?
- What decisions does the player make?
- What emotions should the player feel?

**Actions**:
1. **Read UI code**: `RuneAndRust.ConsoleApp/` - see how system is presented
2. **Review combat logs**: What messages are displayed?
3. **Check descriptor systems**: Flavor text, feedback messages
4. **Playtest mentally**: Walk through player interaction step-by-step

**Output**: Player experience description, UX flow, feedback mechanisms

---

#### Step 8: Consolidate Findings

**Actions**:
1. Create specification outline from template
2. Fill in sections with gathered information:
   - Executive Summary ← Steps 1, 7
   - Functional Requirements ← Step 3
   - System Mechanics ← Step 4
   - Integration Points ← Step 5
   - Balance & Tuning ← Step 6
   - Implementation Guidance ← Step 2

3. Cross-reference all claims to code:
   - Every formula → code line number
   - Every requirement → method name
   - Every parameter → constant/database location

**Output**: Complete specification draft

---

### Walkthrough 2: Planning a New Specification

**Goal**: Plan a specification for a system that doesn't exist yet (forward-looking design)

**When to Use**: Before implementing a new feature

---

#### Step 1: Define the Design Problem

**Questions to Answer**:
- What player problem are we solving?
- What is the "happy path" player experience?
- What are we NOT solving (scope boundaries)?
- Why does this system need to exist?

**Actions**:
1. **Write a problem statement**:
   ```
   Problem: Players feel progression is too linear and lacks meaningful choice.
   Goal: Create a branching skill tree with mutually exclusive choices.
   Non-Goal: Rebalance existing abilities (out of scope).
   ```

2. **Define success criteria**:
   ```
   Success Metrics:
   - Players can create 3+ distinct builds with same archetype
   - Each build is competitively viable
   - Choices feel impactful (no "trap" options)
   ```

3. **Identify design constraints**:
   - **Technical**: Must work with existing PP system
   - **Gameplay**: No ability respecs (choices are permanent)
   - **Scope**: v0.1 supports 2 tiers only (expand later)

**Output**: Problem statement, success criteria, constraints

---

#### Step 2: Research Similar Systems

**Questions to Answer**:
- Do we have similar systems already? (reuse patterns)
- What have other games done? (inspiration)
- What pitfalls should we avoid?

**Actions**:
1. **Search existing codebase**:
   ```bash
   # Find similar patterns
   grep -r "SkillTree\|TalentTree\|AbilityTree" RuneAndRust.Engine/
   grep -r "BranchingChoice\|MutuallyExclusive" RuneAndRust.Engine/
   ```

2. **Review existing specifications**:
   - Check `docs/00-specifications/` for related systems
   - Note reusable patterns (PP spending, validation, etc.)

3. **External research** (if needed):
   - Check design notes, wikis, references
   - Document inspirations in spec appendix

**Output**: Related systems list, inspiration notes, pattern library

---

#### Step 3: Draft Core Requirements

**Questions to Answer**:
- What are the 5-10 critical requirements?
- What edge cases must be handled?
- What validation rules apply?

**Actions**:
1. **Brainstorm user stories**:
   ```
   As a player, I want to unlock skills in a tree so that I can specialize my character.
   As a player, I want to see prerequisites clearly so that I can plan my build.
   As a designer, I want to prevent invalid builds so that balance is maintained.
   ```

2. **Convert to functional requirements**:
   ```
   FR-001: Display Skill Tree with Prerequisites
   FR-002: Unlock Skill with PP Payment
   FR-003: Validate Prerequisites Before Unlock
   FR-004: Prevent Unlocking Mutually Exclusive Skills
   FR-005: Persist Unlocked Skills Across Sessions
   ```

3. **Define acceptance criteria for each**:
   - What inputs trigger this?
   - What outputs result?
   - What error conditions exist?

**Output**: 5-10 functional requirements with acceptance criteria

---

#### Step 4: Design System Mechanics

**Questions to Answer**:
- What formulas govern the system?
- What parameters need tuning?
- How does data flow through the system?

**Actions**:
1. **Draft formulas** (can be rough):
   ```
   Skill_Cost = Base_Cost + (Tier × 2)

   Example:
     Tier 1 Skill: 2 + (1 × 2) = 4 PP
     Tier 2 Skill: 2 + (2 × 2) = 6 PP
   ```

2. **Identify tunable parameters**:
   - What designer might want to change later?
   - Mark as "Tunable: Yes" in parameter tables

3. **Create data flow diagram** (ASCII art is fine):
   ```
   Player Clicks Skill
     ↓
   Check PP >= Cost? → NO → Error Message
     ↓ YES
   Check Prerequisites Met? → NO → Error Message
     ↓ YES
   Deduct PP, Unlock Skill, Update UI
   ```

**Output**: Formulas, parameter tables, data flow diagrams

---

#### Step 5: Map Planned Integrations

**Questions to Answer**:
- Which existing systems will this integrate with?
- What new dependencies are needed?
- What events should be published?

**Actions**:
1. **List dependencies**:
   ```
   Depends On:
   - Progression System (PP spending) → SPEC-PROGRESSION-001
   - Save System (persist unlocks) → Existing SaveService
   - UI System (display tree) → New SkillTreeUI component
   ```

2. **List dependents** (who will use this):
   ```
   Consumed By:
   - Ability System (check if skill unlocked) → SPEC-PROGRESSION-003
   - Character Sheet UI (display unlocks)
   ```

3. **Plan events**:
   ```
   Events to Publish:
   - OnSkillUnlocked (SkillID, PlayerID)
   - OnSkillTreeOpened (PlayerID)

   Events to Subscribe:
   - OnPPChanged (to update UI)
   ```

**Output**: Integration plan, dependency list, event list

---

#### Step 6: Plan Implementation Strategy

**Questions to Answer**:
- What code needs to be written?
- What models/services are needed?
- What database changes are required?
- What's the implementation order?

**Actions**:
1. **Architect the solution**:
   ```
   Models (RuneAndRust.Core):
   - SkillNode.cs (skill definition)
   - SkillTree.cs (tree structure)
   - UnlockedSkill.cs (player progress)

   Services (RuneAndRust.Engine):
   - SkillTreeService.cs (business logic)
   - SkillTreeFactory.cs (tree creation)

   Database (Data/):
   - v0.XX_skill_tree_schema.sql
   ```

2. **Define implementation order**:
   ```
   Phase 1: Models + Database
   Phase 2: Service (unlock logic)
   Phase 3: UI Integration
   Phase 4: Testing
   ```

3. **Add to Implementation Guidance section** of spec

**Output**: Code architecture plan, implementation order

---

#### Step 7: Write the Specification

**Actions**:
1. Copy `TEMPLATE.md` to new file:
   ```bash
   cp docs/00-specifications/TEMPLATE.md \
      docs/00-specifications/progression/skill-tree-spec.md
   ```

2. Fill in all sections using gathered information:
   - Executive Summary ← Step 1
   - Functional Requirements ← Step 3
   - System Mechanics ← Step 4
   - Integration Points ← Step 5
   - Implementation Guidance ← Step 6

3. Mark status as "Draft"

4. Work through document completeness checklist

**Output**: Complete specification draft

---

### Walkthrough 3: Mapping System Relationships

**Goal**: Create a comprehensive dependency map showing how systems interconnect

**When to Use**: Before implementing complex features, during architecture reviews

---

#### Step 1: Identify All Systems in Scope

**Actions**:
```bash
# List all specifications
ls -1 docs/00-specifications/*/*.md | grep -v "README\|TEMPLATE"

# List all service files
ls -1 RuneAndRust.Engine/*Service.cs | wc -l

# List all major system docs
ls -1 docs/01-systems/*.md
```

**Create Master System List**:
```
1. Combat Resolution (SPEC-COMBAT-001)
2. Character Progression (SPEC-PROGRESSION-001)
3. Trauma Economy (SPEC-ECONOMY-003)
4. Ability System (SPEC-PROGRESSION-003)
5. Equipment System (SPEC-ECONOMY-002)
... (continue for all systems)
```

**Output**: Numbered list of all systems

---

#### Step 2: Extract Dependencies from Specifications

**For each specification**:

**Actions**:
1. Open specification file
2. Find "Dependencies" section
3. Extract "Depends On" list
4. Extract "Depended Upon By" list

**Example**:
```markdown
From SPEC-COMBAT-001:
  Depends On:
    - Dice Service
    - Attribute System (SPEC-PROGRESSION-001)
    - Room System

  Depended Upon By:
    - Damage Calculation (SPEC-COMBAT-002)
    - Status Effects (SPEC-COMBAT-003)
    - Boss Encounters (SPEC-COMBAT-004)
```

**Output**: Dependency list per system

---

#### Step 3: Create Dependency Matrix

**Actions**:
Create a table showing which systems depend on which:

```
| System A | Depends On System B? |
|----------|----------------------|
| Combat Resolution | ✅ Progression (FINESSE) |
| Combat Resolution | ✅ Dice Service |
| Combat Resolution | ✅ Room System |
| Progression | ❌ Combat |
| Trauma Economy | ✅ Progression (WILL) |
| Trauma Economy | ✅ Combat (stress sources) |
```

**Identify Circular Dependencies**:
```
⚠️ CIRCULAR: Combat → Progression (uses FINESSE)
              Progression → Combat (awards Legend)

Resolution: Extract shared concept (Attributes) as separate system
```

**Output**: Dependency matrix, circular dependency warnings

---

#### Step 4: Visualize the Dependency Graph

**Actions**:
Create ASCII art dependency graph (or use tools like Mermaid):

```
┌──────────────┐
│ Dice Service │ (Foundation - no dependencies)
└──────┬───────┘
       │
       ├──────> ┌─────────────┐
       │        │ Attributes  │
       │        │ (SPEC-PROG-X)│
       │        └──────┬──────┘
       │               │
       ▼               ▼
┌──────────────┐ ┌────────────────┐
│Combat Resolu-│ │ Progression    │
│tion (001)    │◄┤ System (001)   │
└──────┬───────┘ └────────────────┘
       │
       ├──────> ┌─────────────────┐
       │        │ Damage Calc     │
       │        │ (SPEC-COMBAT-002)│
       │        └─────────────────┘
       │
       └──────> ┌─────────────────┐
                │ Status Effects  │
                │ (SPEC-COMBAT-003)│
                └─────────────────┘
```

**Layers**:
- **Layer 0** (Foundation): Dice Service, Utilities
- **Layer 1** (Core Data): Attributes, Resources
- **Layer 2** (Systems): Combat, Progression, Trauma
- **Layer 3** (Features): Abilities, Equipment, Quests

**Output**: Dependency graph, layering diagram

---

#### Step 5: Map Data Flow

**Actions**:
For critical paths, map how data flows through systems:

**Example: Combat Victory Flow**:
```
1. CombatEngine detects all enemies dead
   ↓
2. CombatEngine calls SagaService.AwardLegend(player, legendAmount)
   ↓
3. SagaService adds Legend to PlayerCharacter.CurrentLegend
   ↓
4. SagaService checks if CurrentLegend >= LegendToNextMilestone
   ↓
5. IF YES: SagaService triggers Milestone advancement
   ↓
6. Milestone grants: +10 HP, +5 Stamina, +1 PP, Full Heal
   ↓
7. SagaService publishes OnMilestoneReached event
   ↓
8. UI subscribes to event, displays celebration screen
```

**Output**: Data flow diagrams for key interactions

---

#### Step 6: Identify Integration Risks

**Questions to Answer**:
- Are there hidden circular dependencies?
- Are there single points of failure?
- Are there tightly coupled systems that should be decoupled?
- Are there missing abstraction layers?

**Actions**:
1. **Check for coupling smells**:
   - System A directly modifies System B's state (tight coupling)
   - System A knows too much about System B's internals
   - Changes to System B always break System A

2. **Identify shared concerns**:
   - Multiple systems implement same logic (duplication)
   - Multiple systems need same data (missing shared service)

3. **Document integration risks**:
   ```
   RISK-001: Combat and Progression are tightly coupled via Legend awards
     Impact: Changes to Legend calculation require touching both systems
     Mitigation: Consider extracting RewardService abstraction
   ```

**Output**: Integration risk register

---

#### Step 7: Document Relationships in Specifications

**Actions**:
For each specification, ensure "Integration Points" section includes:

1. **Systems This System Consumes**:
   - What we use from them
   - How we use it
   - What happens if they fail

2. **Systems That Consume This System**:
   - What they use from us
   - Stability contracts (what we guarantee)

3. **Event System Integration**:
   - Events we publish
   - Events we subscribe to

**Update specifications** if relationships are missing or incorrect.

**Output**: Updated Integration Points sections in all specs

---

### Walkthrough 4: Drafting the Specification

**Goal**: Write a complete, thorough specification from scratch using the template

**When to Use**: After completing analysis (Walkthrough 1) or planning (Walkthrough 2)

---

#### Phase 1: Setup (5 minutes)

**Actions**:
1. Copy template:
   ```bash
   cp docs/00-specifications/TEMPLATE.md \
      docs/00-specifications/[domain]/[system-name]-spec.md
   ```

2. Fill in header metadata:
   ```markdown
   # [System Name] Specification

   > **Template Version**: 1.0
   > **Last Updated**: 2025-11-19
   > **Status**: Draft
   > **Specification ID**: SPEC-[DOMAIN]-[NUMBER]
   ```

3. Fill in Document Control table:
   - Version: 1.0
   - Date: Today
   - Author: Your name or "AI + Human"
   - Changes: "Initial specification"

**Output**: Initialized specification file

---

#### Phase 2: Executive Summary (15 minutes)

**Actions**:
1. **Write Purpose Statement** (1-2 sentences):
   ```markdown
   The [System Name] provides [core functionality] where [key mechanic]
   determines [outcome/player experience].
   ```

   Examples:
   - "Combat Resolution provides turn-based tactical combat where initiative
     determines turn order and tactical choices drive outcomes."
   - "Trauma Economy tracks psychological cost of survival through Stress and
     Corruption, creating tension between power and mental stability."

2. **Define Scope** (bulleted lists):
   **In Scope**: List 5-7 core features
   **Out of Scope**: List 3-5 related but excluded features (link to other specs)

3. **Write Success Criteria** (3-5 criteria):
   - Player Experience: Emotional/cognitive goal
   - Technical: Performance/reliability goal
   - Design: Gameplay goal
   - Balance: Numerical target

**Output**: Complete Executive Summary

---

#### Phase 3: Related Documentation (10 minutes)

**Actions**:
1. **Map Dependencies**:
   - List systems this depends on
   - Link to their SPECs or docs
   - Explain WHY each dependency exists

2. **Map Dependents**:
   - List systems that depend on this
   - Link to their SPECs
   - Explain WHAT they use

3. **Link Implementation Docs**:
   - Layer 1: `docs/01-systems/[file].md`
   - Layer 2: `docs/02-statistical-registry/`
   - Layer 3: `docs/03-technical-reference/`
   - Layer 5: `docs/05-balance-reference/`

4. **Link Code**:
   - Primary Service: `RuneAndRust.Engine/[Service].cs:line`
   - Core Models: `RuneAndRust.Core/[Model].cs`
   - Tests: `RuneAndRust.Tests/[Tests].cs`

**Output**: Complete Related Documentation section

---

#### Phase 4: Design Philosophy (20 minutes)

**Actions**:
1. **Define 2-4 Design Pillars**:
   For each pillar:
   - **Name**: Short phrase (e.g., "Turn Order Clarity")
   - **Rationale**: Why this matters (1 sentence)
   - **Examples**: How it manifests in gameplay (2-3 bullets)

2. **Describe Player Experience**:
   - **Target Experience**: Emotional/cognitive state (1 sentence)
   - **Moment-to-Moment**: What players do second-to-second (3-4 bullets)
   - **Learning Curve**:
     - Novice (0-2 hours): What they learn
     - Intermediate (2-10 hours): Deepening understanding
     - Expert (10+ hours): Mastery elements

3. **List Design Constraints** (4 categories):
   - Technical: Platform, engine limitations
   - Gameplay: Genre conventions, accessibility
   - Narrative: Lore consistency, thematic requirements
   - Scope: Time, budget, team constraints

**Output**: Complete Design Philosophy section

---

#### Phase 5: Functional Requirements (45-60 minutes)

**THIS IS THE CORE OF THE SPECIFICATION - TAKE TIME HERE**

**For each requirement** (aim for 5-10 total):

1. **Assign ID**: FR-001, FR-002, etc. (sequential)

2. **Set Priority**: Critical | High | Medium | Low

3. **Write Description** (2-3 sentences):
   - Use active voice
   - Be specific about inputs, outputs, behavior
   - Avoid implementation details

4. **Write Rationale** (1-2 sentences):
   - WHY this requirement exists
   - Link to design goals or player experience

5. **Define Acceptance Criteria** (checkbox list):
   ```markdown
   - [ ] Specific, measurable criterion 1
   - [ ] Specific, measurable criterion 2
   - [ ] Edge case handling criterion 3
   ```

   Aim for 4-6 criteria per requirement

6. **Create Example Scenarios** (2-3 per requirement):
   ```markdown
   1. **Scenario**: [Normal case description]
      - **Input**: [Concrete input values]
      - **Expected Output**: [Concrete output]
      - **Success Condition**: [How to verify]

   2. **Edge Case**: [Boundary condition]
      - **Input**: [Edge case input]
      - **Expected Behavior**: [How system handles it]
   ```

7. **List Dependencies**:
   - Requires: Other FR-IDs or systems needed first
   - Blocks: FR-IDs that can't proceed without this

8. **Add Implementation Notes**:
   - Code Location: File path and method
   - Data Requirements: What data this needs
   - Performance Considerations: Any constraints

**Repeat for all requirements**

**Output**: 5-10 complete functional requirements

---

#### Phase 6: System Mechanics (45-60 minutes)

**For each core mechanic** (aim for 3-5 total):

1. **Write Overview** (2-3 sentences):
   - Plain language explanation
   - Why this mechanic exists

2. **Describe How It Works** (numbered steps):
   ```markdown
   1. [Step 1 in the process]
   2. [Step 2 in the process]
   3. [Step 3 in the process]
   ```

3. **Document Formula/Logic**:
   ```markdown
   Formula:
     Result = (Input1 × Multiplier) + Bonus

   Example:
     Given:
       Input1 = 5
       Multiplier = 3
       Bonus = 2

     Calculation:
       Result = (5 × 3) + 2 = 17
   ```

4. **Create Parameter Table**:
   | Parameter | Type | Range | Default | Description | Tunable? |
   |-----------|------|-------|---------|-------------|----------|
   | Input1 | int | 1-10 | 5 | ... | Yes |
   | Multiplier | int | 1-5 | 3 | ... | Yes |

5. **Document Data Flow**:
   ```markdown
   Input Sources:
     → [Source 1]
     → [Source 2]

   Processing:
     → [Step 1]
     → [Step 2]

   Output Destinations:
     → [Destination 1]
     → [Destination 2]
   ```

6. **List Edge Cases** (2-4 per mechanic):
   ```markdown
   1. **[Edge Case Name]**: [Description]
      - **Condition**: [When this occurs]
      - **Behavior**: [How system handles it]
      - **Example**: [Concrete example]
   ```

7. **Link Related Requirements**: FR-001, FR-003, etc.

**Repeat for all mechanics**

**Output**: 3-5 complete system mechanics

---

#### Phase 7: Integration Points (30 minutes)

**For Systems This System Consumes**:

For each consumed system:
1. **What We Use**: Specific functionality/data
2. **How We Use It**: Description of integration
3. **Dependency Type**: Hard | Soft | Optional
4. **Failure Handling**: What happens if it fails
5. **API/Interface**: Code example
6. **Data Contract**: Input/output specification

**For Systems That Consume This System**:

For each consumer:
1. **What They Use**: Functionality we provide
2. **How They Use It**: Description
3. **Stability Contract**: What we guarantee not to break
4. **API We Expose**: Public interface example

**Event System**:

Create two tables:
1. **Events Published**: Name, Trigger, Payload, Consumers
2. **Events Subscribed**: Name, Source, Handler, Purpose

**Output**: Complete Integration Points section

---

#### Phase 8: Implementation Guidance (30 minutes)

**For AI Implementers**:

1. **Current State**: Not Started | Partial | Complete | Refactor Needed

2. **Completed Checklist**:
   ```markdown
   - [ ] Core models created
   - [ ] Service/Engine implemented
   - [ ] Factory (if needed) implemented
   - [ ] Database/Registry populated
   - [ ] Integration with dependent systems
   - [ ] Unit tests written
   - [ ] Integration tests written
   - [ ] Balance testing completed
   - [ ] Documentation updated
   ```

3. **Code Architecture**:
   ```markdown
   Recommended Structure:
   RuneAndRust.Core/
     └─ [SystemName]/
         ├─ [Model1].cs
         └─ [Model2].cs

   RuneAndRust.Engine/
     └─ [SystemName]/
         ├─ [Service].cs
         └─ [Factory].cs
   ```

4. **Implementation Checklist** (5-10 items):
   - [ ] Create POCOs in RuneAndRust.Core
   - [ ] Create service with dependency injection
   - [ ] Add error handling and validation
   - [ ] etc.

5. **Code Examples**:
   ```csharp
   // Example service structure
   public class SystemService
   {
       // Constructor, methods, etc.
   }
   ```

6. **Implementation Notes**:
   - Performance Considerations
   - Error Handling
   - Future Extensibility

**Output**: Complete Implementation Guidance section

---

#### Phase 9: Balance & Tuning (20 minutes)

1. **Tunable Parameters Table**:
   | Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
   |-----------|----------|---------------|-----|-----|--------|------------------|
   | [Param1] | [File:line] | [Value] | [Min] | [Max] | [Effect] | High/Med/Low |

2. **Balance Targets** (3-5 targets):
   For each:
   - **Target Name**: Specific measurable goal
   - **Metric**: How we measure
   - **Current**: Current state (if known)
   - **Target**: Desired state
   - **Levers**: Parameters to adjust

3. **Testing Scenarios** (2-4 scenarios):
   For each:
   - **Purpose**: What this tests
   - **Setup**: Preconditions
   - **Procedure**: Steps
   - **Expected Results**: Success criteria
   - **Pass Criteria**: Definition of success

**Output**: Complete Balance & Tuning section

---

#### Phase 10: Review & Polish (30 minutes)

**Actions**:
1. **Run through Document Completeness Checklist**:
   - [ ] All required sections present
   - [ ] Version history populated
   - [ ] All cross-references valid
   - [ ] All formulas have examples
   - [ ] All requirements have IDs
   - [ ] All edge cases documented
   - [ ] All placeholders ([TBD]) resolved

2. **Verify Cross-References**:
   ```bash
   # Check if files exist
   ls -la RuneAndRust.Engine/SystemService.cs
   ls -la docs/01-systems/system-name.md
   ```

3. **Proofread**:
   - Spelling and grammar
   - Consistent terminology
   - Clear, unambiguous language
   - No implementation details in design sections

4. **Add Appendices** (if needed):
   - Terminology definitions
   - Diagrams
   - Mathematical derivations
   - References

5. **Update status**:
   - Change from "Draft" to "Review" when ready

**Output**: Complete, polished specification

---

### Walkthrough 5: Leveraging Existing Documentation

**Goal**: Maximize efficiency by extracting information from existing Layer 1-5 documentation

**When to Use**: When retroactively creating specifications for documented systems

---

#### Mining Layer 1: System Documentation

**Location**: `docs/01-systems/`

**What to Extract**:
1. **Functional Overview** → Use for Executive Summary, Design Philosophy
2. **Player Experience Flow** → Use for UX Flow section
3. **Key Features List** → Convert to Functional Requirements
4. **Edge Cases** → Add to FR acceptance criteria
5. **Integration Points** → Use for Integration Points section

**Actions**:
```bash
# Find system docs
ls docs/01-systems/*.md

# Read relevant system doc
# Copy functional descriptions to spec Executive Summary
# Extract bullet points as FR starting points
```

**Example Extraction**:
```markdown
From docs/01-systems/combat-resolution.md:

"Combat begins when player encounters enemies"
→ Becomes FR-001: Initialize Combat Encounter

"Initiative based on FINESSE rolls"
→ Becomes Mechanic 1: Initiative Rolling

"Player can flee if CanFlee=true"
→ Becomes FR-008: Handle Flee Attempt
```

**Output**: Executive Summary draft, FR list, mechanics outline

---

#### Mining Layer 2: Statistical Registry

**Location**: `docs/02-statistical-registry/` or `docs/02-abilities/`, `docs/03-equipment/`, `docs/04-enemies/`

**What to Extract**:
1. **Formulas** → Use for System Mechanics formulas
2. **Stat Ranges** → Use for Parameter tables (min/max/default)
3. **Probability Tables** → Use for Appendices
4. **Cost Tables** → Use for Balance & Tuning

**Actions**:
```bash
# Find registry files
ls docs/02-statistical-registry/*.md
ls docs/02-abilities/*.md

# Search for formulas
grep -n "Formula:" docs/02-statistical-registry/*.md
grep -n "Calculation:" docs/02-statistical-registry/*.md
```

**Example Extraction**:
```markdown
From docs/02-statistical-registry/damage-formulas.md:

Damage = (Successes × BaseDamage) - TargetArmor

→ Copy directly to System Mechanics section
→ Add to parameter table:
  | BaseDamage | int | 1-100 | 5 | Damage per success | Yes |
  | TargetArmor | int | 0-50 | 0 | Damage reduction | No |
```

**Output**: Formulas, parameter tables, balance data

---

#### Mining Layer 3: Technical Reference

**Location**: `docs/03-technical-reference/`

**What to Extract**:
1. **Service Architecture** → Use for Integration Points
2. **Database Schemas** → Use for Data Requirements, State Management
3. **Code Patterns** → Use for Implementation Guidance
4. **API Documentation** → Use for Integration APIs

**Actions**:
```bash
# Find technical docs
ls docs/03-technical-reference/*.md

# Extract architecture info
grep -A 10 "Service Dependencies" docs/03-technical-reference/*.md
```

**Example Extraction**:
```markdown
From docs/03-technical-reference/service-architecture.md:

CombatEngine depends on:
- DiceService
- SagaService
- LootService

→ Add to Integration Points > Systems This System Consumes:
  - DiceService: Random number generation for initiative
  - SagaService: XP/Legend awards on victory
  - LootService: Loot generation on victory
```

**Output**: Integration mappings, code architecture notes

---

#### Mining Layer 5: Balance Reference

**Location**: `docs/05-balance-reference/`

**What to Extract**:
1. **Design Intent** → Use for Design Philosophy
2. **Power Budgets** → Use for Balance Targets
3. **Tuning Notes** → Use for Tunable Parameters
4. **Playtesting Results** → Use for Known Issues, Future Work

**Actions**:
```bash
# Find balance docs
ls docs/05-balance-reference/*.md

# Extract balance targets
grep -n "Target:" docs/05-balance-reference/*.md
grep -n "Goal:" docs/05-balance-reference/*.md
```

**Example Extraction**:
```markdown
From docs/05-balance-reference/combat-pacing.md:

"Target: Average combat duration 8-15 turns"

→ Add to Balance & Tuning > Balance Targets:
  Target 2: Combat Duration
  - Metric: Average turns to victory
  - Current: 8-15 turns
  - Target: 5-20 turns (prevent too fast or slow)
  - Levers: Enemy HP, damage values, hazard damage
```

**Output**: Balance targets, design intent notes

---

#### Mining Code Comments & Documentation

**What to Extract**:
1. **XML Comments** → Use for FR descriptions, method explanations
2. **Inline Comments** → Use for edge case documentation
3. **Test Names** → Convert to test scenarios

**Actions**:
```bash
# Find well-commented code
grep -B 2 "///" RuneAndRust.Engine/CombatEngine.cs | head -50

# Find edge case comments
grep -n "Edge:" RuneAndRust.Engine/*.cs
grep -n "Special case:" RuneAndRust.Engine/*.cs
```

**Example Extraction**:
```csharp
/// <summary>
/// Initializes combat with player and enemies, rolls initiative
/// </summary>
/// <returns>CombatState with initialized turn order</returns>
public CombatState InitializeCombat(...)

→ Becomes FR-001 description:
  "System must initialize combat state with player, enemies, and room
   context, then roll initiative to determine turn order."
```

**Output**: FR descriptions, implementation notes

---

#### Mining Database Schemas

**Location**: `Data/*.sql`

**What to Extract**:
1. **Table Definitions** → Use for Data Requirements
2. **Column Names/Types** → Use for state variables
3. **Constraints** → Use for validation rules
4. **Relationships** → Use for integration mappings

**Actions**:
```bash
# Find schemas
ls Data/*.sql | grep -i "system\|combat\|progression"

# Extract table definitions
grep "CREATE TABLE" Data/v0.*.sql
```

**Example Extraction**:
```sql
CREATE TABLE combat_state (
    id INTEGER PRIMARY KEY,
    current_turn_index INTEGER NOT NULL,
    is_active BOOLEAN DEFAULT 1,
    can_flee BOOLEAN DEFAULT 1
);

→ Add to State Management > State Variables:
  | CurrentTurnIndex | int | Session | 0 | Index in initiative order |
  | IsActive | bool | Session | false | Whether combat ongoing |
  | CanFlee | bool | Session | true | Whether flee available |
```

**Output**: State variables, persistence requirements

---

#### Consolidation Strategy

**After mining all layers**:

1. **Prioritize Sources**:
   - Layer 1 (System Docs) → Most comprehensive, start here
   - Layer 2 (Stats) → Formulas and numbers
   - Layer 5 (Balance) → Design intent
   - Code → Fill gaps, verify claims

2. **Cross-Verify Information**:
   - If Layer 1 says "Formula: X" and code says "Formula: Y", investigate
   - Note discrepancies in specification as open questions
   - Prefer code as source of truth for implementation

3. **Fill Template Systematically**:
   - Executive Summary ← Layer 1 + Layer 5
   - Functional Requirements ← Layer 1 + Code
   - System Mechanics ← Layer 2 + Code
   - Integration Points ← Layer 3 + Code
   - Balance & Tuning ← Layer 5
   - Implementation Guidance ← Code + Layer 3

4. **Cross-Reference Everything**:
   - Every formula links to Layer 2 doc AND code line
   - Every requirement links to Layer 1 section
   - Every parameter links to database or constant location

**Output**: Complete specification leveraging all existing documentation

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
