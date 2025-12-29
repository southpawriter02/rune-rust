# v[X.Y.Z][letter]: [Thematic Title]

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                    IMPLEMENTATION PLAN TEMPLATE                               ║
║                            Version 1.0.0                                      ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  PURPOSE: Detailed implementation blueprints for version releases             ║
║  LAYER: Layer 4 (Ground Truth) - Technical planning                          ║
║  AUDIENCE: Developers, Project Managers, QA                                   ║
║                                                                              ║
║  Create one plan per feature/version increment.                               ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->

---

```yaml
# ══════════════════════════════════════════════════════════════════════════════
# FRONTMATTER - ⚠️ REQUIRED
# ══════════════════════════════════════════════════════════════════════════════
version: v[X.Y.Z][letter]
title: "[Thematic Title]"
status: planned                  # planned | in-progress | completed | blocked | cancelled
priority: P[N]                   # P1-Critical | P2-High | P3-Medium | P4-Low

# Classification
milestone: "[Milestone Name]"
theme: "[Brief thematic description]"
scope: "[Scope Level]"           # Feature | Enhancement | Fix | Refactor | Infrastructure

# Dependencies
depends-on:
  - "v[X.Y.Z][letter]"           # Previous version that must complete first
blocks:
  - "v[X.Y.Z][letter]"           # Versions waiting on this

# Estimates
complexity: "[Complexity]"       # Simple | Moderate | Complex | Epic
phases: [N]                      # Number of implementation phases

# Metadata
created: YYYY-MM-DD
last-updated: YYYY-MM-DD
author: "[Author Name]"
assignees:
  - "[Assignee 1]"

# Related documents
specifications:
  - SPEC-[DOMAIN]-[NNN]
designs:
  - DESIGN-[DOMAIN]-[NNN]

tags:
  - [tag1]
  - [tag2]
```

---

## Table of Contents

1. [Plan Summary](#1-plan-summary)
2. [Context & Motivation](#2-context--motivation)
3. [Requirements](#3-requirements)
4. [Architecture Overview](#4-architecture-overview)
5. [Implementation Phases](#5-implementation-phases)
6. [Code Implementation](#6-code-implementation)
7. [Data Requirements](#7-data-requirements)
8. [Testing Strategy](#8-testing-strategy)
9. [Logging Requirements](#9-logging-requirements)
10. [Integration Checklist](#10-integration-checklist)
11. [Risk Assessment](#11-risk-assessment)
12. [Acceptance Criteria](#12-acceptance-criteria)
13. [Rollback Plan](#13-rollback-plan)
14. [Plan Changelog](#14-plan-changelog)

---

## Plan Naming Convention

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      VERSION NAMING SCHEME                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  FORMAT: v[MAJOR].[MINOR].[PATCH][LETTER]                                   │
│                                                                             │
│  EXAMPLES:                                                                  │
│  ─────────                                                                  │
│  v0.4.4a  = First feature in v0.4.4 patch                                   │
│  v0.4.4b  = Second feature in v0.4.4 patch                                  │
│  v0.4.4c  = Third feature in v0.4.4 patch                                   │
│  v0.5.0   = Minor version release (no letter = release plan)               │
│  v1.0.0   = Major version release                                           │
│                                                                             │
│  LETTER SEQUENCE:                                                           │
│  a → b → c → d → e → f → ... → z                                           │
│  (Features within same patch version)                                       │
│                                                                             │
│  STATUS PROGRESSION:                                                        │
│  planned → in-progress → completed                                          │
│          ↘ blocked → (unblock) → in-progress                               │
│          ↘ cancelled                                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Plan Summary

> ⚠️ REQUIRED

### 1.1 Overview

| Attribute | Value |
|-----------|-------|
| **Version** | v[X.Y.Z][letter] |
| **Title** | [Thematic Title] |
| **Status** | [planned / in-progress / completed] |
| **Milestone** | [Milestone Name] |
| **Priority** | P[N] |
| **Complexity** | [Simple / Moderate / Complex / Epic] |
| **Phases** | [N] |

### 1.2 Objective

> One paragraph summarizing what this plan accomplishes

[What are we building? What problem does it solve? What is the end result?]

### 1.3 Deliverables Summary

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         DELIVERABLES                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  CODE DELIVERABLES                                                          │
│  ─────────────────                                                          │
│  ├── [N] new files                                                          │
│  ├── [N] modified files                                                     │
│  └── [N] new tests                                                          │
│                                                                             │
│  DOCUMENTATION DELIVERABLES                                                 │
│  ──────────────────────────                                                 │
│  ├── [N] specifications                                                     │
│  └── [N] design documents                                                   │
│                                                                             │
│  DATA DELIVERABLES                                                          │
│  ─────────────────                                                          │
│  ├── [N] database migrations                                                │
│  └── [N] seed data files                                                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.4 Success Metrics

- [ ] [Measurable success criterion]
- [ ] [Measurable success criterion]
- [ ] [Measurable success criterion]

---

## 2. Context & Motivation

> ⚠️ REQUIRED

### 2.1 Background

[Why is this work needed? What is the current state? What prompted this implementation?]

### 2.2 Problem Statement

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         PROBLEM STATEMENT                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  CURRENT STATE:                                                             │
│  [Description of the current situation]                                     │
│                                                                             │
│  PROBLEMS:                                                                  │
│  ├── [Problem 1]                                                            │
│  ├── [Problem 2]                                                            │
│  └── [Problem 3]                                                            │
│                                                                             │
│  DESIRED STATE:                                                             │
│  [Description of what we want to achieve]                                   │
│                                                                             │
│  GAP:                                                                       │
│  [What needs to change to get from current to desired state]                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.3 Dependencies

| Dependency | Type | Status | Impact if Unavailable |
|------------|------|--------|----------------------|
| v[X.Y.Z] | Previous version | [Status] | [What happens] |
| SPEC-XXX-NNN | Specification | [Status] | [What happens] |
| [External system] | Integration | [Status] | [What happens] |

---

## 3. Requirements

> ⚠️ REQUIRED

### 3.1 Functional Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| FR-001 | [Requirement description] | Must | [Spec/Design] |
| FR-002 | [Requirement description] | Must | [Spec/Design] |
| FR-003 | [Requirement description] | Should | [Spec/Design] |
| FR-004 | [Requirement description] | Could | [Spec/Design] |

### 3.2 Non-Functional Requirements

| ID | Requirement | Category | Metric |
|----|-------------|----------|--------|
| NFR-001 | [Requirement] | Performance | [Measurable target] |
| NFR-002 | [Requirement] | Security | [Measurable target] |
| NFR-003 | [Requirement] | Maintainability | [Measurable target] |

### 3.3 Out of Scope

- [Explicitly excluded item]
- [Explicitly excluded item]
- [Explicitly excluded item]

---

## 4. Architecture Overview

> ⚠️ REQUIRED

### 4.1 System Context

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      SYSTEM CONTEXT DIAGRAM                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                         ┌─────────────────────┐                             │
│                         │    User/Player      │                             │
│                         └──────────┬──────────┘                             │
│                                    │                                        │
│                                    ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        APPLICATION BOUNDARY                          │   │
│  │                                                                      │   │
│  │   ┌─────────────┐      ┌─────────────┐      ┌─────────────┐        │   │
│  │   │ UI Layer    │ ───► │ Service     │ ───► │ Data        │        │   │
│  │   │             │      │ Layer       │      │ Layer       │        │   │
│  │   └─────────────┘      └─────────────┘      └─────────────┘        │   │
│  │                               │                    │                │   │
│  │                               │                    │                │   │
│  │   ┌───────────────────────────┴────────────────────┘                │   │
│  │   │                                                                 │   │
│  │   ▼                                                                 │   │
│  │   ╔═══════════════════════════════════════════════════════════╗    │   │
│  │   ║              NEW: [This Feature]                          ║    │   │
│  │   ║                                                           ║    │   │
│  │   ║   [Diagram of new components being added]                 ║    │   │
│  │   ║                                                           ║    │   │
│  │   ╚═══════════════════════════════════════════════════════════╝    │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Component Design

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      COMPONENT DIAGRAM                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  NEW COMPONENTS:                                                            │
│  ═══════════════                                                            │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  I[Feature]Service                                                   │   │
│  │  ┌─────────────────────────────────────────────────────────────────┐│   │
│  │  │  [Feature]Service                                               ││   │
│  │  │  ─────────────────                                              ││   │
│  │  │  - _logger: ILogger<[Feature]Service>                           ││   │
│  │  │  - _dependency: I[Dependency]Service                            ││   │
│  │  │                                                                 ││   │
│  │  │  + Method1(): Task<Result>                                      ││   │
│  │  │  + Method2(param): Result                                       ││   │
│  │  │  + Method3(param1, param2): Task<Result>                        ││   │
│  │  └─────────────────────────────────────────────────────────────────┘│   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                            │                                                │
│                            │ uses                                           │
│                            ▼                                                │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  [Feature]Model                                                      │   │
│  │  ───────────────                                                     │   │
│  │  + Id: Guid                                                          │   │
│  │  + Name: string                                                      │   │
│  │  + [Property]: [Type]                                                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Data Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         DATA FLOW DIAGRAM                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. [Input Source]                                                          │
│     │                                                                       │
│     │ [Data Type]                                                           │
│     ▼                                                                       │
│  2. [Processing Step]                                                       │
│     │                                                                       │
│     │ [Transformed Data]                                                    │
│     ▼                                                                       │
│  3. [Processing Step]                                                       │
│     │                                                                       │
│     │ [Final Data]                                                          │
│     ▼                                                                       │
│  4. [Output/Storage]                                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Implementation Phases

> ⚠️ REQUIRED

### 5.1 Phase Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      PHASE TIMELINE                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PHASE A              PHASE B              PHASE C                          │
│  Foundation           Core Feature         Integration                      │
│  ═══════════          ════════════         ═══════════                      │
│  │                    │                    │                                │
│  ├── Models           ├── Service impl     ├── DI setup                     │
│  ├── Interfaces       ├── Business logic   ├── Testing                      │
│  └── Base setup       └── Core features    └── Documentation                │
│                                                                             │
│  BLOCKING: None       BLOCKING: Phase A    BLOCKING: Phase B                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Phase A: [Phase Name]

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  PHASE A: [Phase Name]                                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  OBJECTIVE:                                                                 │
│  [What this phase accomplishes]                                             │
│                                                                             │
│  DELIVERABLES:                                                              │
│  ├── [ ] [Deliverable 1]                                                    │
│  ├── [ ] [Deliverable 2]                                                    │
│  └── [ ] [Deliverable 3]                                                    │
│                                                                             │
│  TASKS:                                                                     │
│  ├── [ ] Task A.1: [Description]                                            │
│  │       └── Files: [files affected]                                        │
│  ├── [ ] Task A.2: [Description]                                            │
│  │       └── Files: [files affected]                                        │
│  └── [ ] Task A.3: [Description]                                            │
│          └── Files: [files affected]                                        │
│                                                                             │
│  VERIFICATION:                                                              │
│  [How we know this phase is complete]                                       │
│                                                                             │
│  BLOCKS: Phase B                                                            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.3 Phase B: [Phase Name]

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  PHASE B: [Phase Name]                                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  OBJECTIVE:                                                                 │
│  [What this phase accomplishes]                                             │
│                                                                             │
│  REQUIRES: Phase A complete                                                 │
│                                                                             │
│  DELIVERABLES:                                                              │
│  ├── [ ] [Deliverable 1]                                                    │
│  ├── [ ] [Deliverable 2]                                                    │
│  └── [ ] [Deliverable 3]                                                    │
│                                                                             │
│  TASKS:                                                                     │
│  ├── [ ] Task B.1: [Description]                                            │
│  │       └── Files: [files affected]                                        │
│  ├── [ ] Task B.2: [Description]                                            │
│  │       └── Files: [files affected]                                        │
│  └── [ ] Task B.3: [Description]                                            │
│          └── Files: [files affected]                                        │
│                                                                             │
│  VERIFICATION:                                                              │
│  [How we know this phase is complete]                                       │
│                                                                             │
│  BLOCKS: Phase C                                                            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.4 Phase C: [Phase Name]

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  PHASE C: [Phase Name]                                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  OBJECTIVE:                                                                 │
│  [What this phase accomplishes]                                             │
│                                                                             │
│  REQUIRES: Phase B complete                                                 │
│                                                                             │
│  DELIVERABLES:                                                              │
│  ├── [ ] [Deliverable 1]                                                    │
│  ├── [ ] [Deliverable 2]                                                    │
│  └── [ ] [Deliverable 3]                                                    │
│                                                                             │
│  TASKS:                                                                     │
│  ├── [ ] Task C.1: [Description]                                            │
│  │       └── Files: [files affected]                                        │
│  ├── [ ] Task C.2: [Description]                                            │
│  │       └── Files: [files affected]                                        │
│  └── [ ] Task C.3: [Description]                                            │
│          └── Files: [files affected]                                        │
│                                                                             │
│  VERIFICATION:                                                              │
│  [How we know this phase is complete]                                       │
│                                                                             │
│  COMPLETION: Plan complete when Phase C verified                            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Code Implementation

> ⚠️ REQUIRED

### 6.1 Interface Definition

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// FILE: src/Services/I[Feature]Service.cs
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// [Description of service purpose]
/// </summary>
public interface I[Feature]Service
{
    /// <summary>
    /// [Method description]
    /// </summary>
    /// <param name="param">[Parameter description]</param>
    /// <returns>[Return description]</returns>
    Task<[ReturnType]> [MethodName]([ParamType] param);

    /// <summary>
    /// [Method description]
    /// </summary>
    [ReturnType] [MethodName]([ParamType] param1, [ParamType] param2);
}
```

### 6.2 Implementation

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// FILE: src/Services/[Feature]Service.cs
// ═══════════════════════════════════════════════════════════════════════════

public class [Feature]Service : I[Feature]Service
{
    private readonly ILogger<[Feature]Service> _logger;
    private readonly I[Dependency]Service _dependency;

    public [Feature]Service(
        ILogger<[Feature]Service> logger,
        I[Dependency]Service dependency)
    {
        _logger = logger;
        _dependency = dependency;
    }

    /// <inheritdoc />
    public async Task<[ReturnType]> [MethodName]([ParamType] param)
    {
        _logger.LogInformation("Entering [MethodName] with param={Param}", param);

        try
        {
            // Implementation
            var result = await ProcessAsync(param);

            _logger.LogInformation("Exiting [MethodName] with result={Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in [MethodName] for param={Param}", param);
            throw;
        }
    }
}
```

### 6.3 Model Definition

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// FILE: src/Models/[Feature]Model.cs
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// [Model description]
/// </summary>
public class [Feature]Model
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// [Property description]
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// [Property description]
    /// </summary>
    public [Type] [Property] { get; set; }
}
```

### 6.4 Files Summary

| File | Type | Action | Description |
|------|------|--------|-------------|
| `src/Services/I[Feature]Service.cs` | Interface | Create | [Purpose] |
| `src/Services/[Feature]Service.cs` | Class | Create | [Purpose] |
| `src/Models/[Feature]Model.cs` | Class | Create | [Purpose] |
| `Program.cs` | Entry | Modify | Add DI registration |

---

## 7. Data Requirements

> 📋 RECOMMENDED

### 7.1 Database Schema

```sql
-- ═══════════════════════════════════════════════════════════════════════════
-- MIGRATION: V[N]__[Description].sql
-- ═══════════════════════════════════════════════════════════════════════════

-- Create new table
CREATE TABLE [table_name] (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    name            TEXT            NOT NULL,
    [column]        [TYPE]          [CONSTRAINTS],
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- Create indexes
CREATE INDEX idx_[table]_[column] ON [table_name]([column]);

-- Add constraints
ALTER TABLE [table_name]
    ADD CONSTRAINT fk_[table]_[related]
    FOREIGN KEY ([column]) REFERENCES [related_table](id);
```

### 7.2 Seed Data

```json
// ═══════════════════════════════════════════════════════════════════════════
// SEED: [description]_seed.json
// ═══════════════════════════════════════════════════════════════════════════

[
    {
        "id": "[uuid]",
        "name": "[value]",
        "[property]": "[value]"
    }
]
```

### 7.3 Data Migration

| Migration | Description | Reversible |
|-----------|-------------|------------|
| V[N]__[Name].sql | [What it does] | Yes/No |

---

## 8. Testing Strategy

> ⚠️ REQUIRED

### 8.1 Test Coverage Goals

| Component | Unit Tests | Integration Tests | Coverage Target |
|-----------|------------|-------------------|-----------------|
| [Feature]Service | [N] tests | [N] tests | 80% |
| [Feature]Model | [N] tests | - | 90% |

### 8.2 Test Cases

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         TEST CASES                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  UNIT TESTS: [Feature]ServiceTests.cs                                       │
│  ══════════════════════════════════════                                     │
│                                                                             │
│  ├── [MethodName]_WithValidInput_ReturnsExpectedResult                      │
│  │   └── Arrange: [Setup] → Act: [Call method] → Assert: [Verify result]   │
│  │                                                                          │
│  ├── [MethodName]_WithNullInput_ThrowsArgumentNullException                 │
│  │   └── Arrange: null input → Act: Call → Assert: Exception thrown        │
│  │                                                                          │
│  ├── [MethodName]_WithInvalidInput_ThrowsArgumentException                  │
│  │   └── Arrange: invalid input → Act: Call → Assert: Exception thrown     │
│  │                                                                          │
│  ├── [MethodName]_WhenDependencyFails_HandlesGracefully                     │
│  │   └── Arrange: Mock failure → Act: Call → Assert: Proper handling       │
│  │                                                                          │
│  └── [MethodName]_EdgeCase_[Description]                                    │
│      └── Arrange: [Edge case] → Act: Call → Assert: [Expected behavior]    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.3 Test Implementation

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// FILE: tests/Services/[Feature]ServiceTests.cs
// ═══════════════════════════════════════════════════════════════════════════

public class [Feature]ServiceTests
{
    private readonly Mock<ILogger<[Feature]Service>> _loggerMock;
    private readonly Mock<I[Dependency]Service> _dependencyMock;
    private readonly [Feature]Service _sut;

    public [Feature]ServiceTests()
    {
        _loggerMock = new Mock<ILogger<[Feature]Service>>();
        _dependencyMock = new Mock<I[Dependency]Service>();
        _sut = new [Feature]Service(_loggerMock.Object, _dependencyMock.Object);
    }

    [Fact]
    public async Task [MethodName]_WithValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var input = new [InputType] { /* properties */ };
        var expected = new [OutputType] { /* properties */ };

        // Act
        var result = await _sut.[MethodName](input);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task [MethodName]_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        [InputType]? input = null;

        // Act
        var act = async () => await _sut.[MethodName](input!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
```

---

## 9. Logging Requirements

> ⚠️ REQUIRED

### 9.1 Logging Matrix

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         LOGGING MATRIX                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SERVICE: [Feature]Service                                                  │
│  ═════════════════════════                                                  │
│                                                                             │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │ Method          │ Entry │ Exit │ Error │ Debug │ Metrics │ Notes  │    │
│  ├─────────────────┼───────┼──────┼───────┼───────┼─────────┼────────┤    │
│  │ [Method1]       │  ✓    │  ✓   │  ✓    │  ○    │    ○    │        │    │
│  │ [Method2]       │  ✓    │  ✓   │  ✓    │  ✓    │    ✓    │ perf   │    │
│  │ [Method3]       │  ✓    │  ✓   │  ✓    │  ○    │    ○    │        │    │
│  └────────────────────────────────────────────────────────────────────┘    │
│                                                                             │
│  LEGEND:                                                                    │
│  ✓ = Required                                                               │
│  ○ = Optional / As needed                                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Log Messages

| Method | Level | Message Template | Parameters |
|--------|-------|------------------|------------|
| [Method] | Information | "Entering [Method] with param={Param}" | param |
| [Method] | Information | "Exiting [Method] with result={Result}" | result |
| [Method] | Error | "Error in [Method] for param={Param}" | param |

---

## 10. Integration Checklist

> ⚠️ REQUIRED

### 10.1 DI Registration

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// ADD TO: Program.cs (or App.axaml.cs)
// ═══════════════════════════════════════════════════════════════════════════

// Services
services.AddSingleton<I[Feature]Service, [Feature]Service>();

// Configuration (if needed)
services.Configure<[Feature]Settings>(
    configuration.GetSection("[Feature]Settings"));
```

### 10.2 Integration Steps

- [ ] Add interface to service collection
- [ ] Add implementation to service collection
- [ ] Configure any settings
- [ ] Update any consuming services
- [ ] Run integration tests
- [ ] Verify DI resolution

### 10.3 Configuration

```json
// appsettings.json additions
{
  "[Feature]Settings": {
    "Property": "value"
  }
}
```

---

## 11. Risk Assessment

> 📋 RECOMMENDED

### 11.1 Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| [Risk] | Low/Med/High | Low/Med/High | [How to prevent/handle] |

### 11.2 Blockers

| Blocker | Depends On | Workaround |
|---------|------------|------------|
| [Blocker] | [What's blocking] | [Alternative approach] |

---

## 12. Acceptance Criteria

> ⚠️ REQUIRED

### 12.1 Functional Criteria

- [ ] AC-1: [Specific testable criterion]
- [ ] AC-2: [Specific testable criterion]
- [ ] AC-3: [Specific testable criterion]

### 12.2 Technical Criteria

- [ ] All tests passing
- [ ] Code coverage ≥ 80%
- [ ] Logging implemented per matrix
- [ ] DI properly configured
- [ ] No build warnings

### 12.3 Documentation Criteria

- [ ] Specification updated
- [ ] Code documented with XML comments
- [ ] Changelog entry prepared

---

## 13. Rollback Plan

> 📋 RECOMMENDED

### 13.1 Rollback Steps

1. [Step to revert change]
2. [Step to revert change]
3. [Step to verify rollback]

### 13.2 Rollback Triggers

| Trigger | Action |
|---------|--------|
| [Failure condition] | [Rollback action] |

---

## 14. Plan Changelog

> 🔒 LOCKED

| Date | Author | Changes |
|------|--------|---------|
| YYYY-MM-DD | [Author] | Initial plan |

---

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                              END OF TEMPLATE                                  ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  PLAN COMPLETION CHECKLIST:                                                   ║
║  ────────────────────────────────────────────────────────────────────────────║
║  □ All phases defined with tasks                                              ║
║  □ Code implementation outlined                                               ║
║  □ Testing strategy documented                                                ║
║  □ Logging requirements specified                                             ║
║  □ Integration steps listed                                                   ║
║  □ Acceptance criteria defined                                                ║
║  □ Dependencies identified                                                    ║
║  □ Risks assessed                                                             ║
║  □ Status set appropriately                                                   ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->
