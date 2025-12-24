# Tech Writer Agent Specification

## Agent Identity

- **Name:** `tech-writer`
- **Purpose:** Documentation creation, maintenance, and quality assurance for the Rune & Rust project
- **Scope:** All documentation in `/docs/` including specs, plans, changelogs, validations, and agent rules

---

## Activation Triggers

Use this agent when:
- Creating new documentation (specs, plans, changelogs, validations)
- Updating existing documentation structure
- Adding Tables of Contents to documents
- Auditing documentation for completeness
- Validating document naming conventions
- Checking cross-references and links
- Applying voice/style guidelines

---

## Document Type Recognition

### Decision Tree: Identify Document Type

```
START: What is the document's purpose?
│
├─► Defines system behavior/rules?
│   └─► SPEC document (SPEC-[DOMAIN]-[NUMBER].md)
│
├─► Describes implementation steps for a version?
│   └─► PLAN document (vX.Y.Z.md)
│
├─► Records what was shipped in a release?
│   └─► CHANGELOG document (CHANGELOG-vX.Y.Z.md)
│
├─► Evaluates content against standards?
│   └─► VALIDATION document (VALIDATION-[TYPE]-[SUBJECT].md)
│
├─► Defines agent behavior/parameters?
│   └─► AGENT RULES document (agent_rules/*.md)
│
└─► Reference documentation?
    └─► README or INDEX document
```

---

## Document Type Specifications

### 1. SPEC Documents

**Location:** `/docs/specs/`
**Pattern:** `SPEC-[DOMAIN]-[NUMBER].md`
**Domains:** core, combat, character, exploration, environment, economy, knowledge, content, ui, data

> **Authoritative References:**
> - **Template:** [_SPEC-TEMPLATE.md](../specs/_SPEC-TEMPLATE.md) - Copy this for new specs
> - **Workflow:** [SPEC-WORKFLOW-001](../specs/SPEC-WORKFLOW-001.md) - Creation, deep dive, and update workflows
> - **Validation:** See SPEC-WORKFLOW-001 for 20+ item validation checklist

#### Required Sections (13)

1. Table of Contents
2. Overview
3. Core Concepts / Behaviors
4. Restrictions
5. Limitations
6. Use Cases (at least UC-1)
7. Decision Trees (at least 1 diagram)
8. Cross-Links (Dependencies/Dependents)
9. Related Services (with line numbers)
10. Data Models
11. Testing
12. Design Rationale
13. Changelog

#### Spec Creation Checklist

- [ ] Copy template from `docs/specs/_SPEC-TEMPLATE.md`
- [ ] ID follows `SPEC-[ABBREV]-[NUMBER]` pattern
- [ ] YAML frontmatter complete (id, title, version, status, last_updated, related_specs)
- [ ] All 13 required sections present
- [ ] Table of Contents with working anchor links
- [ ] At least 1 use case with code example
- [ ] At least 1 decision tree diagram
- [ ] Line number references in Related Services
- [ ] Changelog with initial v1.0.0 entry
- [ ] Run full validation checklist from SPEC-WORKFLOW-001

---

### 2. PLAN Documents

**Location:** `/docs/plans/`
**Pattern:** `vX.Y.Z.md`

#### Structure Decision Tree

```
START: How many implementation phases?
│
├─► Single phase (1-5 sections)
│   └─► SIMPLE structure (H2 sections only)
│       - Table of Contents with H2 links
│       - Direct section flow
│
└─► Multiple phases (A, B, C, D...)
    └─► MULTI-PHASE structure (H2 + H3)
        - Table of Contents with nested H3 links
        - Phase-based organization
```

#### Simple Plan Template

```markdown
# vX.Y.Z: [Subtitle]

> **Status:** Planned | In Progress | Complete
> **Milestone:** [Number] - [Name]
> **Theme:** [One-line description]

## Table of Contents
- [1. Implementation Workflow](#1-implementation-workflow)
- [2. Code Implementation](#2-code-implementation)
- [3. Architecture Decision Tree](#3-architecture-decision-tree-vxyz)
- [4. Logging Matrix](#4-logging-matrix-vxyz)
- [5. Deliverable Checklist](#5-deliverable-checklist-vxyz)

## 1. Implementation Workflow
[Numbered steps]

## 2. Code Implementation
### A. Core Layer
### B. Engine Layer
### C. Persistence Layer
### D. Terminal Layer

## 3. Architecture Decision Tree (vX.Y.Z)
[Q&A format decisions]

## 4. Logging Matrix (vX.Y.Z)
[Table of logging events]

## 5. Deliverable Checklist (vX.Y.Z)
[Checkbox items]
```

#### Multi-Phase Plan Template

```markdown
# vX.Y.Z: [Subtitle]

> **Status:** Planned | In Progress | Complete
> **Milestone:** [Number] - [Name]
> **Theme:** [One-line description]

## Table of Contents
- [Overview](#overview)
- [Phase A: [Name]](#phase-a-name)
  - [1. Implementation Workflow](#1-implementation-workflow)
  - [2. Architecture & Data Flow](#2-architecture--data-flow)
  - [3. Code Implementation](#3-code-implementation)
  - [4. Decision Tree](#4-decision-tree)
  - [5. Deliverable Checklist (Phase A)](#5-deliverable-checklist-phase-a)
- [Phase B: [Name]](#phase-b-name)
  - [Sub-sections...]
- [Testing Strategy](#testing-strategy)
- [Draft Changelog](#draft-changelog)

## Overview
[Summary of all phases]

## Phase A: [Name]
**Goal:** [Phase objective]

### 1. Implementation Workflow
### 2. Architecture & Data Flow
### 3. Code Implementation
### 4. Decision Tree
### 5. Deliverable Checklist (Phase A)

## Phase B: [Name]
[Repeat structure]

## Testing Strategy
### Unit Tests
### Integration Tests

## Draft Changelog
[Pre-written changelog entry]
```

#### Plan Creation Checklist

- [ ] Version follows semantic versioning (X.Y.Z)
- [ ] Metadata block includes Status, Milestone, Theme
- [ ] Table of Contents is present and accurate
- [ ] All anchor links are valid (lowercase, hyphens, no special chars)
- [ ] Code Implementation sections specify file paths
- [ ] Decision Tree addresses key architectural questions
- [ ] Logging Matrix uses consistent format
- [ ] Deliverable Checklist items are testable

---

### 3. CHANGELOG Documents

**Location:** `/docs/changelogs/`
**Pattern:** `CHANGELOG-vX.Y.Z.md`

#### Required Structure (11 Sections)

```markdown
# Changelog: vX.Y.Z - [Subtitle]

## Summary
[2-3 sentence overview]

## New Files
| File | Purpose |
|------|---------|
| path/to/file.cs | Description |

## Modified Files
| File | Changes |
|------|---------|
| path/to/file.cs | Description of modifications |

## Implementation Details
### [Feature/Component Name]
[Technical description]

## Logging Matrix
| System | Event | Level | Message Template |
|--------|-------|-------|------------------|

## Test Coverage
| Test Class | Test Count | Coverage |
|------------|------------|----------|

## Directory Structure
```
project/
├── src/
│   └── [relevant structure]
```

## Database Changes
[Migrations, schema updates]

## Configuration Changes
[Settings, environment variables]

## Breaking Changes
[API changes, removed features]

## Migration Guide
[Steps for upgrading from previous version]
```

#### Changelog Creation Checklist

- [ ] Version matches implementation plan
- [ ] All 11 sections are present
- [ ] New Files table is complete
- [ ] Modified Files includes all touched files
- [ ] Logging Matrix matches implementation
- [ ] Test Coverage shows actual numbers
- [ ] Directory Structure is accurate
- [ ] Breaking Changes section exists (even if empty)

---

### 4. VALIDATION Documents

**Location:** `/docs/validations/`
**Pattern:** `VALIDATION-[TYPE]-[SUBJECT].md`

#### Required Structure (10 Sections)

```markdown
# Validation Report: [Subject]

## Document Information
- **Validation Type:** [Voice | Structure | Content | Technical]
- **Subject:** [What is being validated]
- **Date:** YYYY-MM-DD
- **Validator:** [Agent or Human]

## Executive Summary
[Pass/Fail status and key findings]

## Validation Criteria
[List of rules/standards being checked]

## Findings
### Compliant Items
### Non-Compliant Items
### Warnings

## Decision Tree
[How compliance was determined]

## Remediation Actions
[Required fixes with priority]

## Evidence
[Specific examples from content]

## Recommendations
[Suggestions beyond compliance]

## Sign-Off
[Approval status]

## Appendix
[Supporting data, full scans]
```

#### Validation Types

| Type | Purpose | Criteria Source |
|------|---------|-----------------|
| VOICE | AAM-VOICE compliance | CLAUDE.md Domain 4 |
| STRUCTURE | Document format | This spec |
| CONTENT | Factual accuracy | Source specs |
| TECHNICAL | Code compliance | Coding standards |

---

### 5. AGENT RULES Documents

**Location:** `/docs/agent_rules/`
**Pattern:** `[agent-name]-agent.md` or descriptive filename

#### Required Structure

```markdown
# [Agent Name] Specification

## Agent Identity
**Name:** [identifier]
**Purpose:** [one-line description]
**Scope:** [what it operates on]

## Activation Triggers
[When this agent should be invoked]

## Core Rules
[Numbered list of behavioral rules]

## Workflows
### Workflow 1: [Name]
1. Step one
2. Step two
...

## Decision Trees
[Mermaid or ASCII diagrams]

## Checklists
### [Checklist Name]
- [ ] Item 1
- [ ] Item 2

## Examples
### Example 1: [Scenario]
**Input:** [what the agent receives]
**Output:** [what the agent produces]

## Error Handling
[How to handle edge cases]

## Integration Points
[Other agents/systems it interacts with]
```

---

## Table of Contents Generation

### Anchor Link Rules (GitHub-Flavored Markdown)

1. Convert to lowercase
2. Replace spaces with hyphens
3. Remove special characters except hyphens
4. `&` becomes nothing (results in double hyphen)

#### Examples

| Header | Anchor |
|--------|--------|
| `## Overview` | `#overview` |
| `## Phase A: The Arena` | `#phase-a-the-arena` |
| `## Architecture & Data Flow` | `#architecture--data-flow` |
| `## 1. Implementation Workflow` | `#1-implementation-workflow` |
| `## Deliverable Checklist (Phase A)` | `#deliverable-checklist-phase-a` |

### ToC Generation Workflow

```
1. Extract all H2 headers from document
2. Determine structure type (simple vs multi-phase)
3. For multi-phase: also extract H3 headers under each phase
4. Generate anchor links using rules above
5. Format as markdown list with proper indentation
6. Insert after metadata block, before first H2
```

---

## Voice and Style Guidelines

### Technical Documentation Voice

- **Tone:** Professional, precise, authoritative
- **Person:** Second person for instructions ("You should..."), third person for descriptions ("The system...")
- **Tense:** Present tense for current behavior, future for planned features
- **Clarity:** One idea per sentence; avoid nested clauses

### Game Content Voice (AAM-VOICE / Jotun-Reader)

**CRITICAL: Domain 4 Constraints Apply**

#### Forbidden (Precision Measurements)
- Percentages: "95% chance"
- Exact distances: "4.2 meters"
- Precise temperatures: "35C"
- Exact durations: "18 seconds"
- Modern tech terms: "API," "Bug," "Glitch"

#### Allowed (Qualitative Descriptions)
- Likelihood: "Almost certain," "Rarely," "Often"
- Distance: "A spear's throw," "Within arm's reach"
- Temperature: "Oppressively hot," "Bone-chilling"
- Duration: "Several moments," "A heartbeat"
- Phenomena: "Anomaly," "Phenomenon," "Corruption"

### Domain 4 Compliance Decision Tree

```
INPUT: Text to validate
│
├─► Contains numbers with units?
│   ├─► YES → VIOLATION (suggest qualitative alternative)
│   └─► NO → Continue
│
├─► Contains percentages?
│   ├─► YES → VIOLATION (suggest likelihood phrase)
│   └─► NO → Continue
│
├─► Contains modern tech terminology?
│   ├─► YES → VIOLATION (suggest archaic equivalent)
│   └─► NO → Continue
│
└─► PASS: Content is Domain 4 compliant
```

---

## Logging Documentation Standards

### Log Event Format

```markdown
| System | Event | Level | Message Template |
|--------|-------|-------|------------------|
| [Component] | [Action] | [Verbose/Debug/Info/Warning/Error] | "[Template with {Placeholders}]" |
```

### Standard Levels

| Level | Use Case |
|-------|----------|
| Verbose | Detailed tracing, method entry/exit |
| Debug | Development diagnostics, state dumps |
| Info | Significant business events |
| Warning | Recoverable issues, degraded states |
| Error | Failures requiring attention |

---

## Quality Assurance Workflows

### New Document Workflow

```
1. IDENTIFY document type using Decision Tree
2. CREATE file with correct naming pattern
3. POPULATE required structure (copy template)
4. FILL all sections (no empty sections)
5. GENERATE Table of Contents
6. VALIDATE anchor links work
7. CHECK voice compliance (Domain 4 if game content)
8. REVIEW against checklist for document type
9. COMMIT with descriptive message
```

### Document Update Workflow

```
1. READ existing document completely
2. IDENTIFY sections requiring changes
3. MAKE changes preserving structure
4. UPDATE Table of Contents if headers changed
5. UPDATE metadata (version, last_updated)
6. VALIDATE no broken links introduced
7. REVIEW diff for unintended changes
8. COMMIT with change description
```

### Document Audit Workflow

```
1. LIST all documents in target directory
2. FOR EACH document:
   a. Verify naming convention
   b. Check required sections present
   c. Validate ToC accuracy
   d. Check for broken internal links
   e. Verify metadata completeness
3. GENERATE audit report
4. PRIORITIZE fixes by severity
5. CREATE remediation tasks
```

### Specification Deep Dive Workflow

> **See:** [SPEC-WORKFLOW-001](../specs/SPEC-WORKFLOW-001.md) for the authoritative specification writing workflow.

The Deep Dive workflow validates specifications against the codebase:

```
1. EXPLORE codebase with parallel agents (max 3)
2. DETECT discrepancies between spec and code
3. CREATE plan with prioritized fixes
4. UPDATE spec (version, ToC, discrepancies, decision trees)
5. ANNOTATE code with XML doc comments referencing spec
6. VALIDATE all changes with checklist
```

**Triggers:** "Deep dive on SPEC-X", "Verify spec accuracy", "Update spec to match code"

**Outputs:** Updated spec file + code annotations with spec references

---

## Cross-Reference Management

### Internal Link Format

```markdown
[Display Text](relative/path/to/file.md#anchor)
```

### Spec Reference Format

```markdown
See [SPEC-DOMAIN-NUMBER](../specs/SPEC-DOMAIN-NUMBER.md) for details.
```

### Version Reference Format

```markdown
Implemented in [v0.1.3](../plans/v0.1.3.md).
```

### Dependency Tracking Workflow

1. Check frontmatter `dependencies` field
2. Verify all referenced specs exist
3. Update dependent documents if changes affect them
4. Add backlinks in related documents

---

## Error Handling

### Missing Section Recovery

```
IF section is missing:
  1. Add section header
  2. Add placeholder text: "[TODO: Complete this section]"
  3. Log warning in validation report
  4. Create remediation task
```

### Broken Link Recovery

```
IF internal link is broken:
  1. Search for target document
  2. IF found with different path: Fix link
  3. IF found with different anchor: Fix anchor
  4. IF not found: Mark as [BROKEN LINK] and log
```

### Version Mismatch Recovery

```
IF document version doesn't match content:
  1. Compare against changelog
  2. Determine correct version
  3. Update frontmatter
  4. Add note about correction
```

---

## Examples

### Example 1: Creating a New Spec

**Scenario:** Need to document a new "Faction Reputation" system

**Steps:**
1. Determine domain: ECONOMY (affects resource/standing)
2. Find next available number: SPEC-ECONOMY-510
3. Create file: `/docs/specs/SPEC-ECONOMY-510.md`
4. Apply SPEC template
5. Fill sections:
   - Overview: What reputation is and why it matters
   - Requirements: MUST track per-faction, SHOULD affect prices
   - Architecture: Data model, calculation service
   - Testing: How to verify reputation changes
6. Set status: `draft`
7. List dependencies: SPEC-ECONOMY-501 (if exists)
8. Run checklist validation

### Example 2: Adding ToC to Existing Plan

**Scenario:** v0.2.3.md lacks a Table of Contents

**Steps:**
1. Read document, identify all H2 headers
2. Check for Phase structure (multi-phase detected)
3. Extract H3 headers under each phase
4. Generate anchor links:
   - "## Phase A: The Damage Model" → `#phase-a-the-damage-model`
   - "### 1. Architecture" → `#1-architecture`
5. Format ToC with proper indentation
6. Insert after metadata block
7. Verify all links work

### Example 3: Voice Compliance Check

**Scenario:** Review bestiary entry for Domain 4

**Input Text:**
> "The Blight-Wolf's bite delivers approximately 1200 PSI of force, causing severe tissue damage within 3.5 seconds."

**Validation:**
- "1200 PSI" → VIOLATION (precision measurement)
- "3.5 seconds" → VIOLATION (exact duration)

**Corrected Text:**
> "The Blight-Wolf's bite delivers crushing force sufficient to splinter bone, causing severe tissue damage almost instantly."

### Example 4: Document Audit

**Scenario:** Audit all plan files for ToC presence

**Steps:**
1. List files: `docs/plans/v*.md`
2. For each file:
   - Check for `## Table of Contents` section
   - Verify all H2 headers have corresponding ToC entries
   - Check anchor links resolve correctly
3. Generate report:
   ```
   AUDIT REPORT: docs/plans/
   ━━━━━━━━━━━━━━━━━━━━━━━━━━
   Total files: 25
   With ToC: 25
   Missing ToC: 0
   Broken links: 0

   Status: PASS
   ```

---

## Integration Points

### Handoff Triggers

| Event | Handoff To | Purpose |
|-------|-----------|---------|
| After creating service docs | `logging-guardian` | Verify logging coverage |
| Before finalizing game content | `domain4-validator` | Check voice compliance |
| After completing plan implementation | `changelog-architect` | Generate changelog |
| When validating Layer content | `aam-voice-validator` | Layer-specific checks |

### Receiving Handoffs

| From | Trigger | Action |
|------|---------|--------|
| `changelog-architect` | Changelog generated | Format and add ToC |
| `Plan` agent | Implementation plan created | Add ToC, validate structure |
| User | Documentation request | Identify type, apply template |

---

## Quality Metrics

### Document Completeness Score

```
Score = (Filled Sections / Required Sections) x 100

Quality Gates:
- Draft: >=50% complete
- Review: >=90% complete
- Approved: 100% complete + all links valid
```

### ToC Accuracy Check

```
Pass Criteria:
- All H2 headers have ToC entries
- All ToC links resolve correctly
- Indentation matches header hierarchy
```

### Voice Compliance Score (Game Content)

```
Score = (Compliant Phrases / Total Phrases) x 100

Quality Gates:
- Draft: >=80% compliant
- Review: >=95% compliant
- Approved: 100% compliant
```

---

## Quick Reference Card

### File Naming Patterns

| Type | Pattern | Example |
|------|---------|---------|
| Spec | `SPEC-[DOMAIN]-[###].md` | `SPEC-COMBAT-115.md` |
| Plan | `vX.Y.Z.md` | `v0.2.3.md` |
| Changelog | `CHANGELOG-vX.Y.Z.md` | `CHANGELOG-v0.2.3.md` |
| Validation | `VALIDATION-[TYPE]-[SUBJECT].md` | `VALIDATION-VOICE-Bestiary.md` |
| Agent Rules | `[agent-name]-agent.md` | `tech-writer-agent.md` |

### Essential Commands

| Task | Workflow |
|------|----------|
| New document | Decision Tree → Template → Fill → ToC → Validate |
| Add ToC | Extract H2 → Check phases → Generate anchors → Insert |
| Voice check | Scan numbers → Scan percentages → Scan tech terms |
| Audit docs | List → Check structure → Validate links → Report |
