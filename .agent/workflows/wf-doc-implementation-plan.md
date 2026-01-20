---
description: Workflow for creating high-detail implementation plans following the project template
---

# Implementation Plan Workflow

Ensures implementation plans align with design specifications, scope breakdowns, prior plans, and changelogs.

---

## ⚠️ Critical Prerequisites

### Required Source Documents

| Document Type | Location | Purpose |
|---------------|----------|---------|
| **Design Spec** | `docs/v0.X.x/vX.X.Xa-design-specification.md` | WHAT to build |
| **Scope Breakdown** | Parent version or ROADMAP.md | Boundaries |
| **Prior Plans** | `docs/v0.X.x/vX.X.X*-implementation-plan.md` | Patterns/dependencies |
| **Changelogs** | Version changelogs or git history | What's implemented |
| **Codebase** | `src/` directory | Current state |

### Document Hierarchy
```
ROADMAP.md → Scope Breakdown → Design Spec → Implementation Plan ◀── YOU ARE HERE → Changelog
```

---

## Phase 1: Pre-Planning Research

### 1.1 Review Design Specification
- [ ] Read the entire design specification for this version
- [ ] Extract all features from the Feature Overview section
- [ ] Note all entities/value objects from Data Model section
- [ ] List all configuration files from Config Schemas section
- [ ] Count expected unit tests from Unit Testing section
- [ ] Copy acceptance criteria for reference

### 1.2 Review Prior Implementation Plans
- [ ] Naming conventions used?
- [ ] ASCII diagram style?
- [ ] Code patterns established?
- [ ] Components this version depends on?

### 1.3 Review Changelogs and Code
- [ ] Review changelogs for prior versions
- [ ] Verify components exist in `src/` before referencing
- [ ] Confirm config files exist before extending

**Never assume—verify!**

### 1.4 Review Scope Breakdown
If a parent version scope breakdown exists:
- [ ] Confirm this sub-version's features match the scope
- [ ] Note any features explicitly deferred
- [ ] Identify integration points with sibling versions

---

## Phase 2: Document Setup

### 2.1 Create File
Create file at: `docs/v0.X.x/vX.X.Xa-implementation-plan.md`

### 2.2 Header Block
```markdown
# vX.X.Xa Implementation Plan: [Feature Name]

## Overview

**Version:** X.X.Xa
**Focus:** [Single sentence describing the feature]
**Prerequisites:** vX.X.Y ([Prior Version]), vX.X.Z ([Another Prior Version])
**Estimated Unit Tests:** ~[number from design spec]
**Estimated Implementation Effort:** [Low/Medium/Medium-High/High] complexity

---

## Phase 3: Executive Summary

### 3.1 Opening Paragraph
Describe what this implementation achieves:
- What system/feature is being built
- How it builds on prior versions
- What design patterns it uses

### 3.2 Key Features List
Extract from design specification:
```markdown
### Key Features
- **Feature 1**: Brief description
- **Feature 2**: Brief description
```

### 3.3 Design Principles
Document architectural decisions:
```markdown
### Design Principles
1. **Principle 1**: Why this approach
2. **Principle 2**: Why this approach
```

---

## Phase 4: Prerequisites & Dependencies

### 4.1 Dependencies from Prior Versions
**Critical: Verify each dependency exists!**

For each prerequisite version, create a table:
```markdown
### From vX.X.Y ([Feature Name])

| Component | Purpose for vX.X.X | Verified ✓ |
|-----------|-------------------|------------|
| `ComponentName` | How this version uses it | ✓ |
| `InterfaceName` | How this version uses it | ✓ |
```

### 4.2 Dependency Verification Checklist
Before proceeding:
- [ ] All listed components exist in codebase
- [ ] All listed interfaces exist in codebase
- [ ] All configuration files exist (if extending)
- [ ] Prior version's changelog confirms implementation

### 4.3 From Existing Codebase
List components from base version:
```markdown
### From Existing Codebase (vX.X.X)

| Component | Purpose for vX.X.X |
|-----------|-------------------|
| `Player` | Target for effects |
| `GameSessionService` | Integration point |
```

---

## Phase 5: Architecture Overview

### 5.1 Layer Diagram
Create ASCII diagram showing all layers with `(new)` and `(updated)` markers.

### 5.2 Flow Diagrams
For complex operations, show execution flow with validation phases.

### 5.3 Diagram Requirements
- [ ] New components marked `(new)`, updated marked `(updated)`
- [ ] Method signatures in boxes
- [ ] ASCII characters properly aligned

---

## Phase 6: Layer-by-Layer Implementation

### 6.1 Domain Layer
For each entity/value object, include:
- File path
- Full implementation with XML docs
- Private EF Core constructor
- Factory method with validation

### 6.2 Application Layer
For each service, include:
- Constructor with DI parameters
- All public methods with XML docs
- Structured logging points

### 6.3 Infrastructure Layer
Configuration loader updates for new config files.

### 6.4 Presentation Layer
TUI/GUI view updates and command handlers.

### 6.5 Code Quality Checklist
For every code block:
- [ ] Complete implementation (not pseudocode)
- [ ] XML documentation on public members
- [ ] Argument validation included
- [ ] Logging points included
- [ ] Matches patterns from prior plans

---

## Phase 7: Configuration Files

Include full JSON with `$schema` reference and 3-5 realistic examples.

---

## Phase 8: Implementation Phases

Break into logical phases (Domain → Application → Infrastructure → Presentation) with deliverables and tests per phase.

---

## Phase 9: Unit Test Specifications

List test files with counts using `MethodName_Scenario_ExpectedBehavior` naming pattern.

---

## Phase 10: Integration Points

Document components requiring integration and what this version provides to future versions.

---

## Phase 11: Acceptance Criteria

Copy from design spec using `AC-Xa-N` format. Add logging and unit test criteria.

---

## Phase 12: Deliverable Checklist

Organize by layer: Domain → Application → Infrastructure → Presentation → Tests → Documentation.

---

## Phase 13: Future Considerations

List deferred features with target versions. Include ASCII dependency tree showing REQUIRES and PROVIDES.
---

## Document Footer

End with:
```markdown
---

*This implementation plan provides a comprehensive roadmap for implementing vX.X.Xa. It builds upon the established patterns from prior versions while maintaining the clean architecture principles already present in the codebase.*
```

---

## Reference Documents

### Exemplar Implementation Plans
- `docs/v0.0.x/v0.0.4c-implementation-plan.md` (~2600 lines)
- `docs/v0.0.x/v0.0.4d-implementation-plan.md`
- `wf-doc-design-specification.md`
- `customizability-guidelines.md`