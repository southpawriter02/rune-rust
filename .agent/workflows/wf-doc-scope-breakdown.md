---
description: Workflow for creating high-detail implementation plans following the project template.
---

# Scope Breakdown Workflow

This workflow ensures consistency and comprehensive detail when drafting implementation plans for new features.

---

## Phase 1: Initial Setup

### 1.1 Pre-Planning Checklist
- [ ] Read the ROADMAP.md entry for this feature
- [ ] Identify the design specification document (if exists)
- [ ] Determine version number following pattern: `v0.X.Ya` (letter for sub-version)
- [ ] Create file: `docs/v0.X.x/vX.X.Xa-implementation-plan.md`

### 1.2 Header Block
Fill in the overview metadata:

```markdown
# vX.X.Xa Implementation Plan: [Feature Name]

## Overview

**Version:** X.X.Xa
**Status:** [Designed/Planned/Implemented/Released]
**Focus:** [Single sentence describing the feature]
**Prerequisites:** [List prior versions this depends on]
**Estimated Unit Tests:** ~[number]
**Estimated Implementation Effort:** [Low/Medium/Medium-High/High] complexity
```

---

## Phase 2: Document Structure

### 2.1 Table of Contents (Required Sections)
// turbo
Ensure all these sections are present:
1. Executive Summary
2. Prerequisites & Dependencies
3. Architecture Overview
4. Domain Layer Implementation
5. Application Layer Implementation
6. Infrastructure Layer Implementation
7. Presentation Layer Implementation
8. Configuration Files
9. Implementation Phases
10. Unit Test Specifications
11. Integration Points
12. Acceptance Criteria
13. Deliverable Checklist

### 2.2 Executive Summary Requirements
- [ ] High-level paragraph explaining what the feature does
- [ ] **Key Features** bulleted list (5-8 items)
- [ ] **Design Principles** numbered list (3-5 architectural decisions)

---

## Phase 3: Dependencies Documentation

### 3.1 Prerequisites Table
For each dependency version, create a table:
```markdown
### From v0.0.Xa ([Feature Name])

| Component | Purpose for vX.X.X |
|-----------|-------------------|
| `ComponentName` | How this version uses it |
```

### 3.2 Dependency Verification
- [ ] All referenced components actually exist in prior versions
- [ ] No circular dependencies introduced
- [ ] Dependencies listed in topological order

---

## Phase 4: Architecture Documentation

### 4.1 ASCII Layer Diagram
**REQUIRED**: Create ASCII box diagram showing:
- Presentation Layer (views, renderers)
- Application Layer (services, DTOs)
- Domain Layer (entities, value objects, enums)
- Infrastructure Layer (repositories, configuration)

### 4.2 Flow Diagrams
For complex operations, include:
- [ ] Execution flow diagram (e.g., ability execution, combat flow)
- [ ] Validation phase clearly marked
- [ ] Error paths shown
- [ ] State changes documented

### 4.3 Diagram Quality Checklist
- [ ] Uses box-drawing characters (┌ ─ ┐ │ └ ┘ ├ ┤ ┬ ┴ ┼)
- [ ] Consistent width per box
- [ ] Methods listed with signatures
- [ ] New additions marked with `// NEW`

---

## Phase 5: Layer Implementation Details

### 5.1 Domain Layer Section
For each new entity/value object:
- [ ] **File path** specified
- [ ] **Full C# code** with XML documentation
- [ ] Properties with access modifiers
- [ ] Factory methods (if applicable)
- [ ] Private EF Core constructor shown
- [ ] Argument validation included

### 5.2 Application Layer Section
For each service:
- [ ] Constructor with DI parameters
- [ ] All public methods with signatures
- [ ] Return types documented
- [ ] Logging points identified

### 5.3 Infrastructure Layer Section
- [ ] Configuration loader updates
- [ ] Repository changes (if any)
- [ ] External service integrations

### 5.4 Presentation Layer Section
- [ ] UI component changes
- [ ] Command handlers
- [ ] View updates (TUI/GUI)

### 5.5 Code Snippet Requirements
Each code block must have:
- [ ] Namespace declaration
- [ ] XML summary comments
- [ ] Complete implementation (not pseudocode)
- [ ] Follows .editorconfig conventions

---

## Phase 6: Configuration Files

### 6.1 JSON Configuration
- [ ] Full example JSON with realistic data
- [ ] Schema file reference (`$schema`)
- [ ] File path specified (`config/*.json`)
- [ ] Minimum 3-5 sample entries to show patterns

### 6.2 Configuration Validation
- [ ] All IDs use kebab-case
- [ ] References between configs are valid
- [ ] Default values documented

---

## Phase 7: Unit Test Specifications

### 7.1 Test File Structure
List every test file to create:
```markdown
### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `EntityNameTests.cs` | ~X | Constructor, methods, validation |
```

### 7.2 Test Case Format
For each test area, specify:
```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
```

### 7.3 Test Categories Required
- [ ] Happy path tests
- [ ] Edge cases (null, empty, boundaries)
- [ ] Exception tests (`act.Should().Throw<>()`)
- [ ] Integration points

---

## Phase 8: Acceptance Criteria

### 8.1 Criteria ID Format
Use pattern: `AC-[version]-[number]`
```markdown
### AC-4c-1: [Feature Area]
- [ ] Specific testable requirement
- [ ] Another testable requirement
```

### 8.2 Criteria Categories
Include acceptance criteria for:
- [ ] Core functionality
- [ ] Validation/error handling
- [ ] UI/display
- [ ] Logging requirements
- [ ] Unit test passage

### 8.3 Criteria Quality
Each criterion must be:
- [ ] Binary (pass/fail, not subjective)
- [ ] Testable (can be verified)
- [ ] Specific (one thing per line)

---

## Phase 9: Deliverable Checklist

### 9.1 Organize by Layer
```markdown
### Domain Layer
- [ ] Entity1.cs
- [ ] ValueObject1.cs

### Application Layer
- [ ] Service1.cs
- [ ] DTO1.cs

### Infrastructure Layer
- [ ] ConfigLoader updates
- [ ] config/feature.json

### Presentation Layer
- [ ] View changes
- [ ] Command handlers

### Tests
- [ ] EntityTests.cs
- [ ] ServiceTests.cs

### Documentation
- [ ] XML documentation
- [ ] JSON schema
- [ ] ROADMAP.md updates
```

---

## Phase 10: Future Considerations

### 10.1 Deferred Features
List features explicitly NOT in scope:
```markdown
### Deferred Features (Not in vX.X.X Scope)
- **Feature A**: Brief reason it's deferred
- **Feature B**: What version it targets
```

### 10.2 Integration Points
Document how this feature connects to next version:
```markdown
### vX.X.Y Integration Points
- What this provides to future versions
- Entry points for extension
```

### 10.3 Dependencies Summary (ASCII)
Create ASCII dependency tree:
```
vX.X.Xa Feature
    │
    ├── REQUIRES from vX.X.X
    │   ├── Component A
    │   └── Component B
    │
    └── PROVIDES to vX.X.Xb
        ├── New service
        └── New entity
```

---

## Quality Review Checklist

Before considering the implementation plan complete:

### Content Completeness
- [ ] All 13 required sections present
- [ ] Every code snippet is complete (not pseudocode)
- [ ] All file paths specified
- [ ] All method signatures documented

### Visual Quality
- [ ] ASCII diagrams are properly aligned
- [ ] Tables use consistent column widths
- [ ] Code blocks have language specifiers
- [ ] Horizontal rules between major sections

### Technical Accuracy
- [ ] References to prior versions are accurate
- [ ] ID naming follows conventions (kebab-case)
- [ ] Clean Architecture compliance
- [ ] No hardcoded terminology (uses services)
- [ ] No functionality is being made up and hasn't already been outlined in existing documentation

### Testability
- [ ] Each acceptance criterion is testable
- [ ] Test file locations specified
- [ ] Test count estimates provided
- [ ] Logging requirements documented

---

## Template Reference

See exemplar implementation plans:
- `docs/v0.0.x/v0.0.4c-implementation-plan.md` (~2600 lines, Ability System)
- `docs/v0.0.x/v0.0.4d-implementation-plan.md` (~2000 lines, Status Effects)

These demonstrate the expected level of detail for all implementation plans.