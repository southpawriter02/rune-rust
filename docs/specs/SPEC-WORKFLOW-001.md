---
id: SPEC-WORKFLOW-001
title: Specification Writing Workflow
version: 1.2.0
status: Implemented
last_updated: 2025-12-25
related_specs: []
---

# SPEC-WORKFLOW-001: Specification Writing Workflow

> **Version:** 1.2.0
> **Status:** Implemented
> **Scope:** All SPEC-* documents in `docs/specs/`
> **Template:** [_SPEC-TEMPLATE.md](./_SPEC-TEMPLATE.md) - Copy this for new specs
> **Authority:** This specification is the authoritative source for spec creation and maintenance

---

## Quick Start

**For AI Agents:** Follow these steps for specification work:

1. **Creating a new spec?** → Copy [_SPEC-TEMPLATE.md](./_SPEC-TEMPLATE.md), follow [New Spec Creation Workflow](#new-spec-creation-workflow)
2. **Deep dive on existing spec?** → Follow [Deep Dive Workflow](#deep-dive-workflow) (6 phases)
3. **Updating a spec?** → Follow [Spec Update Workflow](#spec-update-workflow), bump version
4. **Validating?** → Run the [Validation Checklist](#validation-checklist) (20+ items)

---

## Table of Contents

- [Overview](#overview)
- [Workflow Types](#workflow-types)
- [Deep Dive Workflow](#deep-dive-workflow)
- [New Spec Creation Workflow](#new-spec-creation-workflow)
- [Spec Update Workflow](#spec-update-workflow)
- [Validation Checklist](#validation-checklist)
- [Decision Trees](#decision-trees)
- [Spec Structure Requirements](#spec-structure-requirements)
- [Code Traceability](#code-traceability)
- [Testing Requirements](#testing-requirements)
- [Version Control](#version-control)
- [Cross-Reference Management](#cross-reference-management)
- [Quality Gates](#quality-gates)
- [Examples](#examples)
- [Changelog](#changelog)

---

## Overview

The Specification Writing Workflow defines the authoritative process for creating, validating, and maintaining game system specifications. This document captures the patterns established through deep dive work on SPEC-CHAR-001 and SPEC-ADVANCEMENT-001.

**Key Principle:** Specifications must accurately reflect implementation code. When discrepancies exist, they must be resolved through this workflow.

**Scope:** All SPEC-* documents in `docs/specs/` across all 11 domains (Core, Combat, Character, Exploration, Environment, Economy, Knowledge, Content, UI, Data, Tools).

---

## Workflow Types

| Workflow | Trigger | Output |
|----------|---------|--------|
| **Deep Dive** | "Deep dive on SPEC-X" request | Updated spec + code annotations |
| **New Spec** | New system implementation | New SPEC-DOMAIN-NNN.md |
| **Update** | Code changes affecting spec | Version bump + changelog entry |

---

## Deep Dive Workflow

The Deep Dive workflow validates an existing specification against the current codebase, fixing discrepancies and ensuring bidirectional traceability.

### Phase 1: Exploration

```
EXPLORATION
├── Launch parallel Explore agents (max 3)
│   ├── Agent 1: Primary service/entity files
│   ├── Agent 2: Supporting enums/types
│   └── Agent 3: Related specs and tests
├── Gather file paths and line numbers
└── Document existing code state
```

**Outputs:**
- List of relevant files with line counts
- Current code behavior summary
- Initial discrepancy candidates

### Phase 2: Discrepancy Detection

```
DISCREPANCY DETECTION
├── Compare spec statements to code
├── Document each discrepancy:
│   ├── Location (spec line, code file:line)
│   ├── Spec Says: [quoted text]
│   ├── Code Says: [actual behavior]
│   └── Action: [fix spec | fix code | document intent]
└── Categorize: Critical | Minor | Cosmetic
```

**Discrepancy Template:**
```markdown
### Issue #N: [Short Description]
**Location:** Spec Line XX, `File.cs:NN`
**Spec Says:** "[Quoted text from spec]"
**Code Says:** [Actual implementation behavior]
**Category:** Critical | Minor | Cosmetic
**Action:** Fix spec to match code / Fix code to match spec / Document intentional difference
```

### Phase 3: Plan Creation

```
PLAN CREATION
├── Create todo list with actionable items
├── Prioritize: Critical discrepancies first
├── Include validation checklist
└── Get user approval before proceeding
```

### Phase 4: Spec Updates

```
SPEC UPDATES
├── Update metadata (version, last_updated)
├── Add/update Table of Contents
├── Fix all discrepancies
├── Add Decision Trees (if missing)
├── Add Validation Checklist (if missing)
├── Add Changelog entry
└── Mark todo items complete
```

**Required Updates:**
1. Bump version (MINOR for structural changes)
2. Update `last_updated` to current date
3. Add/update Table of Contents
4. Fix all identified discrepancies
5. Add Decision Trees if missing
6. Add Changelog entry summarizing changes

### Phase 5: Code Annotations

```
CODE ANNOTATIONS
├── Add XML doc comments to primary entities
│   /// <remarks>See: SPEC-XXX-001, Section "Name"</remarks>
├── Add spec refs to related enums
├── Add spec refs to factory/service classes
└── Verify all key files reference spec
```

### Phase 6: Final Validation

```
FINAL VALIDATION
├── Run validation checklist
├── Verify all links work
├── Confirm code builds (if applicable)
└── Mark all todos complete
```

---

## New Spec Creation Workflow

### Step 1: Determine Domain

Use the [Domain Assignment Decision Tree](#domain-assignment) to categorize the system.

### Step 2: Assign Spec ID

Check `docs/specs/README.md` for the next available number in the target domain.

| Domain | Number Range | Example |
|--------|-------------|---------|
| CORE | 001-099 | SPEC-CORE-001 |
| COMBAT | 001-099 | SPEC-COMBAT-001 |
| CHARACTER | 001-099 | SPEC-CHAR-001 |
| EXPLORATION | 001-099 | SPEC-NAV-001 |
| ENVIRONMENT | 001-099 | SPEC-HAZARD-001 |
| ECONOMY | 001-099 | SPEC-INV-001 |
| KNOWLEDGE | 001-099 | SPEC-CODEX-001 |
| CONTENT | 001-099 | SPEC-DESC-001 |
| UI | 001-099 | SPEC-UI-001 |
| DATA | 001-099 | SPEC-SAVE-001 |
| TOOLS | 001-099 | SPEC-AUDIT-001 |

### Step 3: Create Spec File

Create `docs/specs/[domain]/SPEC-[ABBREV]-NNN.md`

### Step 4: Fill Required Sections

See [Spec Structure Requirements](#spec-structure-requirements) for mandatory sections.

### Step 5: Update Index

Add entry to `docs/specs/README.md` in the appropriate domain table.

### Step 6: Run Validation Checklist

Complete the [Validation Checklist](#validation-checklist) before marking complete.

---

## Spec Update Workflow

### Step 1: Identify Trigger

- Code change affecting documented behavior
- Bug fix revealing spec inaccuracy
- Feature addition requiring new sections

### Step 2: Determine Update Scope

| Scope | Version Bump | Examples |
|-------|--------------|----------|
| **PATCH** | 1.0.X | Typos, formatting, clarifications |
| **MINOR** | 1.X.0 | New sections, behavior updates, structural changes |
| **MAJOR** | X.0.0 | Breaking changes, complete rewrites |

### Step 3: Make Updates

1. Update relevant sections
2. Bump version in frontmatter
3. Update `last_updated` date
4. Add changelog entry

### Step 4: Verify Cross-References

Check `related_specs` are still accurate and update if needed.

---

## Validation Checklist

### Structure Validation
- [ ] YAML frontmatter complete (id, title, version, status, last_updated, related_specs)
- [ ] Table of Contents present and all links work
- [ ] All required sections exist (Overview, Behaviors, Testing, Changelog)
- [ ] Version follows semver (X.Y.Z)
- [ ] Header block matches frontmatter

### Content Validation
- [ ] Spec statements match actual code behavior
- [ ] Line number references are accurate
- [ ] Formula documentation matches implementation
- [ ] Status indicators are current (Implemented/Planned/Deprecated)
- [ ] Code blocks use correct syntax highlighting

### Cross-Reference Validation
- [ ] All `related_specs` exist and are valid
- [ ] Internal anchor links resolve correctly
- [ ] External file paths are correct
- [ ] Reverse dependencies noted in affected specs

### Code Traceability
- [ ] Primary entity has `/// <remarks>See: SPEC-XXX</remarks>`
- [ ] Related enums reference spec sections
- [ ] Factory/Service classes reference relevant use cases
- [ ] Test files reference spec requirements (where applicable)

### Quality Gates
- [ ] No TODO placeholders in published content
- [ ] Decision trees cover primary flows
- [ ] At least 1 use case documented
- [ ] Changelog reflects current version changes
- [ ] Design rationale explains key decisions

---

## Decision Trees

### Workflow Selection

```
START: What is the task?
│
├─► "Deep dive on SPEC-X" or "Verify spec accuracy"
│   └─► DEEP DIVE WORKFLOW
│
├─► Implementing new system without existing spec
│   └─► NEW SPEC CREATION WORKFLOW
│
├─► Code changed and spec needs update
│   └─► SPEC UPDATE WORKFLOW
│
└─► Adding documentation to existing code
    └─► CODE ANNOTATION WORKFLOW (Phase 5 of Deep Dive)
```

### Domain Assignment

```
START: What does this system do?
│
├─► Foundational mechanics (dice, game loop)
│   └─► CORE domain
│
├─► Combat, abilities, enemies, status effects
│   └─► COMBAT domain
│
├─► Character stats, progression, trauma, corruption
│   └─► CHARACTER domain
│
├─► Navigation, dungeon generation, spawning
│   └─► EXPLORATION domain
│
├─► Hazards, weather, ambient conditions
│   └─► ENVIRONMENT domain
│
├─► Inventory, crafting, loot, repair
│   └─► ECONOMY domain
│
├─► Codex, journals, data captures
│   └─► KNOWLEDGE domain
│
├─► Dynamic text, templates, descriptors
│   └─► CONTENT domain
│
├─► UI framework, rendering, input, themes
│   └─► UI domain
│
└─► Persistence, repositories, migrations, saves
    └─► DATA domain
```

### Status Assignment

```
START: What is the implementation state?
│
├─► Code complete, tested, in production
│   └─► status: Implemented
│
├─► Code exists but incomplete or untested
│   └─► status: Scaffolded
│
├─► Spec written, no code yet
│   └─► status: Planned
│
├─► Under active development
│   └─► status: In Progress
│
├─► Being reviewed before approval
│   └─► status: Review
│
└─► Superseded by newer spec
    └─► status: Deprecated
```

---

## Spec Structure Requirements

### Frontmatter (Required)

```yaml
---
id: SPEC-DOMAIN-NNN
title: Descriptive Title
version: X.Y.Z
status: Draft | Review | Approved | Deprecated | Scaffolded | Implemented | In Progress
last_updated: YYYY-MM-DD
related_specs: [SPEC-XXX-001, SPEC-YYY-001]
---
```

### Header Block (Required)

```markdown
# SPEC-DOMAIN-NNN: Title

> **Version:** X.Y.Z
> **Status:** Status (context note if needed)
> **Service:** `PrimaryService`, `SecondaryService`
> **Location:** `Path/To/Service.cs`
```

### Required Sections

| # | Section | Purpose |
|---|---------|---------|
| 1 | Table of Contents | Navigation with anchor links |
| 2 | Overview | 2-3 paragraph summary |
| 3 | Core Concepts | Key terminology and definitions |
| 4 | Behaviors | Primary functionality documentation |
| 5 | Restrictions | What the system MUST NOT do |
| 6 | Limitations | Known constraints and caps |
| 7 | Use Cases | At least UC-1 with code example |
| 8 | Decision Trees | At least 1 flow diagram |
| 9 | Cross-Links | Dependencies and dependents |
| 10 | Related Services | File paths with line numbers |
| 11 | Data Models | Entity/enum definitions with code blocks |
| 12 | Configuration | Constants and settings |
| 13 | Testing | Test files, counts, scenarios |
| 14 | Design Rationale | Explains "why" for key decisions |
| 15 | Changelog | Version history |

---

## Code Traceability

### XML Doc Comment Patterns

**Entity/Class Level:**
```csharp
/// <summary>
/// Description of the class.
/// </summary>
/// <remarks>
/// See: SPEC-XXX-001 (Title) for design documentation.
/// See: SPEC-YYY-001 for related systems.
/// </remarks>
public class EntityName
```

**Enum Level:**
```csharp
/// <summary>
/// Description of the enum.
/// </summary>
/// <remarks>See: SPEC-XXX-001, Section "Section Name"</remarks>
public enum EnumName
```

**Method Level (for key behaviors):**
```csharp
/// <summary>
/// Description of the method.
/// </summary>
/// <remarks>See: SPEC-XXX-001, Section "Behavior Name"</remarks>
public ReturnType MethodName()
```

**Property Level (for formulas):**
```csharp
/// <summary>
/// Maximum health points. Derived: 50 + (Sturdiness * 10).
/// </summary>
/// <remarks>See: SPEC-ADVANCEMENT-001, Section "Derived Stat Scaling"</remarks>
public int MaxHP { get; set; }
```

---

## Testing Requirements

### Spec Must Document

- Test file names and locations
- Test counts per category
- Critical test scenarios (numbered list)
- Validation checklist for manual testing

### Example Format

```markdown
## Testing

### Test Files
| File | Tests | Coverage |
|------|-------|----------|
| `ServiceNameTests.cs` | 50+ tests | Formula validation, edge cases |
| `FactoryTests.cs` | 16+ tests | All combinations |

### Critical Test Scenarios
1. Scenario description (formula)
2. Scenario description (edge case)
3. Scenario description (boundary condition)
...

### Validation Checklist
- [ ] All formulas produce expected results
- [ ] Edge cases handled gracefully
- [ ] Integration with dependent systems verified
```

---

## Version Control

### Semantic Versioning for Specs

| Type | Bump | Examples |
|------|------|----------|
| **MAJOR** | X.0.0 | Breaking changes, complete rewrites |
| **MINOR** | 1.X.0 | New sections, behavior updates, structural changes |
| **PATCH** | 1.0.X | Typos, formatting, minor clarifications |

### Changelog Format

```markdown
## Changelog

### vX.Y.Z (YYYY-MM-DD)
- Change description 1
- Change description 2
- Change description 3

### vX.Y.Z-1 (YYYY-MM-DD)
- Previous change 1
- Previous change 2
```

---

## Cross-Reference Management

### Frontmatter Dependencies

```yaml
related_specs: [SPEC-CORRUPT-001, SPEC-INV-001, SPEC-ADVANCEMENT-001]
```

### In-Document Links

```markdown
See [SPEC-CORRUPT-001](../character/SPEC-CORRUPT-001.md) for corruption penalties.
```

### Reverse Dependency Tracking

When updating a spec:
1. Check `docs/specs/README.md` Dependency Matrix
2. Identify specs that depend on the one being updated
3. Verify changes don't break dependent specs
4. Update dependent specs if interface changes

---

## Quality Gates

| Gate | Criteria | Enforced By |
|------|----------|-------------|
| **Draft** | All sections present (may have TODOs) | Author |
| **Review** | No TODOs, all links work, validated against code | Peer |
| **Approved** | Peer review passed, code traceability complete | Maintainer |
| **Deprecated** | Replacement spec linked, migration guide added | Maintainer |

### Gate Transition Requirements

**Draft → Review:**
- All 15 required sections present
- No placeholder text
- All code blocks have syntax highlighting

**Review → Approved:**
- Peer validation against codebase
- All anchor links verified
- Code annotations added

**Approved → Deprecated:**
- Replacement spec identified and linked
- Migration guide in Changelog
- Status updated with deprecation date

---

## Examples

### Example 1: Deep Dive on SPEC-CHAR-001

**Trigger:** User request "Deep dive on SPEC-CHAR-001"

**Phase 1 - Exploration Output:**
- `Character.cs` (274 lines)
- `Attribute.cs`, `ArchetypeType.cs`, `LineageType.cs`, `BackgroundType.cs`
- `StatCalculationService.cs` (290 lines)
- `CharacterFactory.cs` (150 lines)

**Phase 2 - Discrepancies Found:**

| # | Issue | Category |
|---|-------|----------|
| 1 | Corruption multipliers: Spec said 0.75x/0.5x, Code says 0.80/0.60 | Critical |
| 2 | BackgroundType (v0.3.4c) not documented in spec | Minor |
| 3 | Level-up marked as implemented but NOT YET IMPLEMENTED | Critical |
| 4 | Missing Table of Contents | Cosmetic |
| 5 | Stamina floor uses 0 (not 1 like HP) - intentional but undocumented | Minor |

**Phase 4 - Updates Made:**
- Version 1.0.0 → 1.1.0
- Added Table of Contents
- Fixed corruption multiplier values (100%/90%/80%/60%/0%)
- Added BackgroundType to Data Models
- Updated UC-3 with "NOT YET IMPLEMENTED" banner
- Added Decision Trees (Character Creation, Stat Recalculation)
- Added Validation Checklist
- Added Changelog

**Phase 5 - Code Annotations Added:**
- `Character.cs`: SPEC-CHAR-001, SPEC-ADVANCEMENT-001 references
- `Attribute.cs`: SPEC-CHAR-001 "Core Attributes" section
- `ArchetypeType.cs`: SPEC-CHAR-001 "Archetype Bonuses" section
- `LineageType.cs`: SPEC-CHAR-001 "Lineage Bonuses" section
- `CharacterFactory.cs`: SPEC-CHAR-001 UC-1 reference

### Example 2: New Spec Creation

**Trigger:** Implementing new Faction Reputation system

**Step 1 - Domain:** ECONOMY (affects standing/prices)

**Step 2 - ID:** SPEC-FACTION-001 (next available in economy)

**Step 3 - File:** `docs/specs/economy/SPEC-FACTION-001.md`

**Step 4 - Sections Filled:**
- Frontmatter with status: `draft`
- Overview explaining faction mechanics
- Core Concepts: Reputation tiers, decay, bonuses
- Restrictions: Cannot exceed +100 or -100
- Use Cases: UC-1 (Gain Reputation), UC-2 (Check Discount)
- Decision Trees: Reputation Change Flow
- Related Services: `FactionService.cs` with planned line numbers
- Testing: Planned test scenarios

**Step 5 - Index:** Added to README.md Economy section

---

## Changelog

### v1.2.0 (2025-12-25)

- Fixed domain count from 10 to 11 (added TOOLS domain)
- Added TOOLS domain to domain assignment table
- Updated required sections from 13 to 15 (added Configuration, split Core Concepts/Behaviors)
- Fixed status value casing to use Title Case (Draft, Review, etc.)
- Added "In Progress" as valid status option
- Updated gate transition requirement to reference 15 sections

### v1.1.0 (2025-12-23)
- Added Quick Start section for AI agents
- Added template reference to header block
- Created _SPEC-TEMPLATE.md as the authoritative spec template
- Updated tech-writer-agent.md to reference this workflow

### v1.0.0 (2025-12-23)
- Initial specification
- Documented Deep Dive workflow (6 phases)
- Documented New Spec Creation workflow (6 steps)
- Documented Spec Update workflow (4 steps)
- Added comprehensive Validation Checklist (20+ items)
- Added Decision Trees (Workflow Selection, Domain Assignment, Status Assignment)
- Added Spec Structure Requirements (13 required sections)
- Added Code Traceability patterns (4 XML doc comment types)
- Added Version Control guidelines (semver for specs)
- Added Quality Gates (4 stages with criteria)
- Added Examples (SPEC-CHAR-001 deep dive, new spec creation)
