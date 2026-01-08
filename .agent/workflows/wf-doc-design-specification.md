---
description: Workflow for creating consistent, high-quality, high-detail design specifications
---

# Design Specification Workflow

This workflow ensures design specifications are consistent, comprehensive, and strictly aligned with the parent version's scope without introducing unapproved features.

---

## ⚠️ Critical Scope Rules

### Rule 1: No Feature Invention
> **DO NOT** create new features, systems, or mechanics that are not explicitly defined in the ROADMAP.md entry for this version.

If during specification you identify a need for something not in scope:
1. Document it in "Future Considerations" section
2. Note which future version it might belong to
3. **DO NOT** include it in deliverables or acceptance criteria

### Rule 2: Scope Alignment
Before writing any section, verify against:
- [ ] ROADMAP.md entry for this version
- [ ] Parent version's scope breakdown (if exists)
- [ ] Prior version's design spec (for dependency verification)

### Rule 3: Explicit Over Implicit
Only include features that are **explicitly stated**. If a feature requires interpretation:
- Ask for clarification before proceeding
- Document assumptions clearly
- Mark uncertain items with `[TBD]` for review

---

## Phase 1: Pre-Writing Preparation

### 1.1 Gather Source Materials
- [ ] Read ROADMAP.md entry for this version completely
- [ ] Review parent version scope breakdown (if applicable)
- [ ] Review prerequisite version(s) design specs
- [ ] Identify all features explicitly listed

### 1.2 Create Feature Inventory
Create a checklist of ALL features from ROADMAP.md:
```markdown
## Features from ROADMAP.md v0.X.X

- [ ] Feature A (explicitly listed)
- [ ] Feature B (explicitly listed)
- [ ] Feature C (explicitly listed)
...
```

### 1.3 Verify: Nothing Added, Nothing Missing
- [ ] Every feature in inventory exists in ROADMAP.md
- [ ] No features invented or assumed
- [ ] All features will be addressed in the spec

---

## Phase 2: Document Structure

### 2.1 Header Block
```markdown
# vX.X.X Design Specification: [Feature Title]

**Version:** X.X.X
**Status:** Planning
**Focus:** [Single sentence from ROADMAP.md]
**Prerequisite:** vX.X.Y ([Prior Version Name])
```

### 2.2 Required Sections (15 Sections)
Every design spec MUST include:

1. **Executive Summary** — Overview and key deliverables table
2. **Feature Overview** — ASCII tree of all features
3. **Architecture Diagrams** — Flow diagrams, layer diagrams
4. **Feature Section 1** — First major feature area
5. **Feature Section 2** — Second major feature area (as needed)
6. **Feature Section N** — Additional feature areas
7. **Data Model Changes** — New entities, value objects, enums
8. **Configuration File Schemas** — JSON structure specifications
9. **Logging Specifications** — Log levels by component
10. **Unit Testing Requirements** — Test counts by feature
11. **Use Cases** — Actor-flow pairs (UC-XXX format)
12. **Deliverable Checklist** — All artifacts to create
13. **Acceptance Criteria** — Functional and quality criteria
14. **Dependencies** — What this version requires/provides
15. **Future Considerations** — Deferred items (NOT in scope)

### 2.3 Table of Contents Format
Use numbered sections with sub-sections:
```markdown
## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Feature Overview](#2-feature-overview)
3. [Architecture Diagrams](#3-architecture-diagrams)
4. [Feature Area Name](#4-feature-area-name)
   - 4.1 [Sub-feature A](#41-sub-feature-a)
   - 4.2 [Sub-feature B](#42-sub-feature-b)
...
```

---

## Phase 3: Executive Summary

### 3.1 Opening Paragraph
Write 2-3 sentences describing:
- What this version introduces
- Its significance to the overall project
- Key architectural patterns established

### 3.2 Key Deliverables Table
```markdown
### Key Deliverables

| Category | Items |
|----------|-------|
| **Feature Area A** | Item 1, Item 2, Item 3 |
| **Feature Area B** | Item 1, Item 2 |
| **Tests** | ~N new unit tests |
```

### 3.3 Architectural Significance
If this version establishes patterns for future use:
```markdown
### Architectural Significance

This version establishes **[pattern name]** that will be used throughout the game:
- Pattern aspect 1
- Pattern aspect 2
```

---

## Phase 4: Feature Overview

### 4.1 ASCII Feature Tree
Create comprehensive tree showing ALL features:
```markdown
## 2. Feature Overview

```
vX.X.X Features
├── Major Feature Area A
│   ├── Sub-feature A1
│   ├── Sub-feature A2
│   └── Sub-feature A3
├── Major Feature Area B
│   ├── Sub-feature B1
│   └── Sub-feature B2
└── Major Feature Area C
    └── Sub-feature C1
```
```

### 4.2 Scope Verification Checkpoint
After creating the tree:
- [ ] Compare every item to ROADMAP.md
- [ ] Remove any items not in ROADMAP.md
- [ ] Flag any ROADMAP.md items not in tree

---

## Phase 5: Architecture Diagrams

### 5.1 Required Diagrams
Include at minimum:
- [ ] **Flow Diagram** — User/system interaction flow
- [ ] **Layer Diagram** — Clean Architecture layers involved
- [ ] **Service Diagram** — How services interact

### 5.2 ASCII Diagram Standards
Use box-drawing characters:
```
┌───────────────────┐   ┌───────────────────┐
│   Component A     │──▶│   Component B     │
├───────────────────┤   ├───────────────────┤
│ - Method1()       │   │ - Method1()       │
│ - Method2()       │   │ - Method2()       │
└───────────────────┘   └───────────────────┘
```

### 5.3 Diagram Quality Checklist
- [ ] Boxes are aligned and consistent width
- [ ] Arrows use proper characters (─ │ ▶ ▼)
- [ ] Layer names are clearly labeled
- [ ] Methods/properties listed in boxes

---

## Phase 6: Feature Sections

### 6.1 Section Structure Template
Each feature section should follow:
```markdown
## N. [Feature Name]

### N.1 [Sub-feature Name]

**Purpose:** [One sentence explaining why this exists]

#### [Implementation Aspect]

[Description or code]

#### [Configuration/Data]

[JSON or schema examples]

#### [UI/UX] (if applicable)

[ASCII mockups or interaction descriptions]
```

### 6.2 Code Snippet Requirements
When including C# code:
- [ ] Full file path specified
- [ ] Complete implementation (not pseudocode)
- [ ] XML documentation comments included
- [ ] Follows .editorconfig conventions

### 6.3 JSON Configuration Requirements
When including config files:
- [ ] `$schema` reference included
- [ ] Realistic sample data (3-5 entries minimum)
- [ ] All fields documented
- [ ] File path specified

### 6.4 UI Mockup Requirements
For TUI/GUI elements:
- [ ] ASCII mockup for TUI
- [ ] Key bindings/commands shown
- [ ] User flow documented

---

## Phase 7: Data Model Changes

### 7.1 New Entities Table
```markdown
## 9. Data Model Changes

### New Entities

| Entity | Layer | Description |
|--------|-------|-------------|
| `EntityName` | Domain | What it represents |
```

### 7.2 Entity Detail Format
For each entity:
```markdown
#### EntityName

**File:** `src/Core/RuneAndRust.Domain/Entities/EntityName.cs`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique identifier |
| `Name` | `string` | Display name |
```

### 7.3 Value Objects and Enums
Document separately with same format.

---

## Phase 8: Logging Specifications

### 8.1 Log Levels Table
```markdown
## 11. Logging Specifications

### Log Levels by Component

| Component | Level | Events |
|-----------|-------|--------|
| `ServiceName` | Information | Major operations |
| `ServiceName` | Debug | Detailed steps |
| `ServiceName` | Warning | Recoverable issues |
| `ServiceName` | Error | Failures |
```

### 8.2 Logging Rules
- Information: User-visible actions
- Debug: Internal state changes
- Warning: Unexpected but handled
- Error: Failed operations

---

## Phase 9: Unit Testing Requirements

### 9.1 Test Count Table
```markdown
## 12. Unit Testing Requirements

### Test Count by Feature

| Feature | Test Count |
|---------|------------|
| Feature A | ~N |
| Feature B | ~N |
| **Total** | **~N** |
```

### 9.2 Test Categories
For each feature, tests should cover:
- Happy path
- Edge cases
- Error conditions
- Boundary values

---

## Phase 10: Use Cases

### 10.1 Use Case Format
```markdown
## 13. Use Cases

### UC-001: [Action Name]
**Actor:** [Player/System/Admin]
**Flow:** Step 1 → Step 2 → Step 3 → Outcome

### UC-002: [Action Name]
**Actor:** [Player/System/Admin]
**Flow:** Step 1 → Step 2 → Outcome
```

### 10.2 Use Case Coverage
- [ ] Every major user action has a use case
- [ ] System-initiated flows documented
- [ ] Error flows included where relevant

---

## Phase 11: Deliverable Checklist

### 11.1 Organize by Category
```markdown
## 14. Deliverable Checklist

### [Feature Area A]
- [ ] Specific deliverable 1
- [ ] Specific deliverable 2

### [Feature Area B]
- [ ] Specific deliverable 1
- [ ] Specific deliverable 2

### Configuration Files
- [ ] config/feature.json created
- [ ] config/schemas/feature.schema.json created

### Testing
- [ ] ~N unit tests implemented
- [ ] All tests passing
```

### 11.2 Deliverable Scope Check
Before finalizing:
- [ ] Every deliverable traces to ROADMAP.md
- [ ] No deliverables added beyond scope
- [ ] All ROADMAP.md features have deliverables

---

## Phase 12: Acceptance Criteria

### 12.1 Acceptance Criteria Categories
```markdown
## 15. Acceptance Criteria

### Functional
- [ ] Testable functional requirement 1
- [ ] Testable functional requirement 2

### Quality
- [ ] Build succeeds with 0 errors/warnings
- [ ] All tests pass
- [ ] Configuration files validate against schemas
- [ ] XML documentation complete
```

### 12.2 Criteria Quality Rules
Each criterion MUST be:
- [ ] **Binary** — Pass or fail, no gray area
- [ ] **Testable** — Can be objectively verified
- [ ] **Specific** — One thing per criterion
- [ ] **In Scope** — Traces to ROADMAP.md feature

---

## Phase 13: Future Considerations

### 13.1 Purpose
Document features that were considered but are **NOT** in scope:
```markdown
## Future Considerations

### Deferred to vX.X.Y
- **Feature Name**: Why it's deferred, target version

### Out of Scope
- **Feature Name**: Why it doesn't belong in this version
```

### 13.2 Critical Rule
> Items in "Future Considerations" **MUST NOT** appear in:
> - Deliverable Checklist
> - Acceptance Criteria
> - Unit Test Requirements

---

## Final Review Checklist

### Scope Compliance
- [ ] Every feature traces to ROADMAP.md
- [ ] No invented features included
- [ ] Parent scope breakdown honored (if exists)
- [ ] Future items properly deferred

### Document Completeness
- [ ] All 15 sections present
- [ ] Table of contents accurate
- [ ] All code snippets complete
- [ ] All JSON examples realistic

### Quality Standards
- [ ] ASCII diagrams properly aligned
- [ ] Tables use consistent column widths
- [ ] Code blocks have language specifiers
- [ ] Horizontal rules between sections

### Cross-References
- [ ] Prerequisites accurately listed
- [ ] Dependencies verified to exist
- [ ] Version numbers consistent

---

## Document Metadata Footer

End every design spec with:
```markdown
---

*Document Version: 1.0*
*Last Updated: YYYY-MM-DD*
*Author: [Name]*
```

---

## Template Reference

See exemplar design specifications:
- `docs/v0.0.x/v0.0.3-design-specification.md` (~2800 lines, comprehensive)
- `docs/v0.0.x/v0.0.4a-design-specification.md`
- `docs/v0.0.x/v0.0.4b-design-specification.md`

These demonstrate the expected level of detail for all design specifications.
