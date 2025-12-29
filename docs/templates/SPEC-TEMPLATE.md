# SPEC-[DOMAIN]-[NNN]: [Specification Title]

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                         SPECIFICATION TEMPLATE                                ║
║                              Version 1.0.0                                    ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  PURPOSE: Technical specifications for systems, features, and mechanics       ║
║  LAYER: Layer 4 (Ground Truth) - Full precision allowed                       ║
║  AUDIENCE: Developers, Architects, QA Engineers                               ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->

---

```yaml
# ══════════════════════════════════════════════════════════════════════════════
# FRONTMATTER - ⚠️ REQUIRED
# ══════════════════════════════════════════════════════════════════════════════
id: SPEC-[DOMAIN]-[NNN]
title: "[Human-Readable Specification Title]"
version: 1.0.0
status: draft                    # draft | review | approved | implemented | deprecated
layer: Layer 4
domain: [DOMAIN]                 # CORE | COMBAT | CHAR | NAV | INV | UI | DATA | ENV | STATUS | ABILITY | ENEMY
priority: P2                     # P1-Critical | P2-High | P3-Medium | P4-Low
last-updated: YYYY-MM-DD
author: "[Author Name]"
reviewers: []

# Dependencies
depends-on:
  - SPEC-[DOMAIN]-[NNN]          # Required specifications
implements:
  - DESIGN-[DOMAIN]-[NNN]        # Design documents this implements
supersedes:
  - SPEC-[DOMAIN]-[NNN]          # Previous version (if applicable)

# Tags for searchability
tags:
  - [tag1]
  - [tag2]
  - [tag3]
```

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Scope & Boundaries](#2-scope--boundaries)
3. [Core Concepts](#3-core-concepts)
4. [System Architecture](#4-system-architecture)
5. [Data Models](#5-data-models)
6. [Behaviors & Methods](#6-behaviors--methods)
7. [Business Rules](#7-business-rules)
8. [Restrictions & Limitations](#8-restrictions--limitations)
9. [Use Cases](#9-use-cases)
10. [Decision Trees](#10-decision-trees)
11. [Error Handling](#11-error-handling)
12. [Configuration](#12-configuration)
13. [Testing Strategy](#13-testing-strategy)
14. [Cross-References](#14-cross-references)
15. [Design Rationale](#15-design-rationale)
16. [Implementation Checklist](#16-implementation-checklist)
17. [Changelog](#17-changelog)

---

## 1. Executive Summary

> ⚠️ REQUIRED | 2-4 paragraphs

### 1.1 Purpose

[Describe WHAT this specification defines and WHY it exists. What problem does it solve?]

### 1.2 Scope

[Define the boundaries of this specification. What is IN scope? What is explicitly OUT of scope?]

### 1.3 Key Deliverables

| Deliverable | Type | Description |
|-------------|------|-------------|
| `I[Feature]Service` | Interface | [Brief description] |
| `[Feature]Service` | Implementation | [Brief description] |
| `[Feature]Model` | Data Model | [Brief description] |
| `[Feature]ServiceTests` | Test Suite | [Brief description] |

---

## 2. Scope & Boundaries

> ⚠️ REQUIRED

### 2.1 In Scope

- [x] [Feature/behavior that IS covered]
- [x] [Feature/behavior that IS covered]
- [x] [Feature/behavior that IS covered]

### 2.2 Out of Scope

- [ ] [Feature/behavior explicitly NOT covered]
- [ ] [Feature/behavior explicitly NOT covered]
- [ ] [Feature/behavior explicitly NOT covered]

### 2.3 Assumptions

| ID | Assumption | Impact if Invalid |
|----|------------|-------------------|
| A1 | [Assumption text] | [Impact description] |
| A2 | [Assumption text] | [Impact description] |

### 2.4 Constraints

| ID | Constraint | Rationale |
|----|------------|-----------|
| C1 | [Constraint text] | [Why this constraint exists] |
| C2 | [Constraint text] | [Why this constraint exists] |

---

## 3. Core Concepts

> ⚠️ REQUIRED

### 3.1 Terminology

| Term | Definition | Example |
|------|------------|---------|
| [Term 1] | [Clear definition] | [Usage example] |
| [Term 2] | [Clear definition] | [Usage example] |
| [Term 3] | [Clear definition] | [Usage example] |

### 3.2 Key Principles

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           DESIGN PRINCIPLES                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. [PRINCIPLE NAME]                                                        │
│     └── [Description of the principle and why it matters]                   │
│                                                                             │
│  2. [PRINCIPLE NAME]                                                        │
│     └── [Description of the principle and why it matters]                   │
│                                                                             │
│  3. [PRINCIPLE NAME]                                                        │
│     └── [Description of the principle and why it matters]                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.3 Conceptual Model

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                          CONCEPTUAL OVERVIEW                                  │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐          │
│    │  [Entity A] │ ──────► │  [Entity B] │ ──────► │  [Entity C] │          │
│    │             │         │             │         │             │          │
│    │  - prop1    │         │  - prop1    │         │  - prop1    │          │
│    │  - prop2    │         │  - prop2    │         │  - prop2    │          │
│    └─────────────┘         └─────────────┘         └─────────────┘          │
│           │                       │                       │                  │
│           │                       │                       │                  │
│           └───────────────────────┼───────────────────────┘                  │
│                                   │                                          │
│                                   ▼                                          │
│                          ┌─────────────┐                                     │
│                          │  [Result]   │                                     │
│                          └─────────────┘                                     │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. System Architecture

> ⚠️ REQUIRED

### 4.1 Component Overview

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                          COMPONENT DIAGRAM                                    │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                         PRESENTATION LAYER                           │    │
│  │  ┌─────────────────┐    ┌─────────────────┐                         │    │
│  │  │   ViewModel     │    │      View       │                         │    │
│  │  │  [Feature]VM    │◄───│   [Feature]     │                         │    │
│  │  └────────┬────────┘    └─────────────────┘                         │    │
│  └───────────┼─────────────────────────────────────────────────────────┘    │
│              │                                                               │
│              │ DI                                                            │
│              ▼                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                          SERVICE LAYER                               │    │
│  │  ┌─────────────────────────────────────────────────────────────┐    │    │
│  │  │                   I[Feature]Service                          │    │    │
│  │  │  ┌─────────────────────────────────────────────────────────┐│    │    │
│  │  │  │              [Feature]Service                           ││    │    │
│  │  │  │  - Method1()                                            ││    │    │
│  │  │  │  - Method2()                                            ││    │    │
│  │  │  │  - Method3()                                            ││    │    │
│  │  │  └─────────────────────────────────────────────────────────┘│    │    │
│  │  └─────────────────────────────────────────────────────────────┘    │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│              │                                                               │
│              │ Repository Pattern                                            │
│              ▼                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                           DATA LAYER                                 │    │
│  │  ┌─────────────────┐    ┌─────────────────┐                         │    │
│  │  │   Repository    │    │    DbContext    │                         │    │
│  │  └─────────────────┘    └─────────────────┘                         │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Interface Definition

```csharp
/// <summary>
/// [Description of what this service does]
/// </summary>
public interface I[Feature]Service
{
    /// <summary>
    /// [Method description]
    /// </summary>
    /// <param name="param1">[Parameter description]</param>
    /// <returns>[Return value description]</returns>
    Task<[ReturnType]> [MethodName]([ParamType] param1);

    /// <summary>
    /// [Method description]
    /// </summary>
    [ReturnType] [MethodName]([ParamType] param1, [ParamType] param2);
}
```

### 4.3 Dependencies

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         DEPENDENCY GRAPH                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  [This Service]                                                             │
│       │                                                                     │
│       ├──► ILogger<[Feature]Service>           [Required - Logging]         │
│       ├──► I[Dependency1]Service               [Required - Description]     │
│       ├──► I[Dependency2]Service               [Required - Description]     │
│       └──► I[Dependency3]Service               [Optional - Description]     │
│                                                                             │
│  Consumed By:                                                               │
│       │                                                                     │
│       ├──◄ [Consumer1]ViewModel                [UI Layer]                   │
│       ├──◄ [Consumer2]Service                  [Service Layer]              │
│       └──◄ [Consumer3]Controller               [API Layer]                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.4 Sequence Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      SEQUENCE: [Operation Name]                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────┐      ┌───────────┐      ┌────────────┐      ┌─────────────┐     │
│  │ User  │      │ ViewModel │      │  Service   │      │  Repository │     │
│  └───┬───┘      └─────┬─────┘      └──────┬─────┘      └──────┬──────┘     │
│      │                │                   │                   │            │
│      │  1. Action     │                   │                   │            │
│      │───────────────►│                   │                   │            │
│      │                │                   │                   │            │
│      │                │  2. Command       │                   │            │
│      │                │──────────────────►│                   │            │
│      │                │                   │                   │            │
│      │                │                   │  3. Query         │            │
│      │                │                   │──────────────────►│            │
│      │                │                   │                   │            │
│      │                │                   │  4. Data          │            │
│      │                │                   │◄──────────────────│            │
│      │                │                   │                   │            │
│      │                │  5. Result        │                   │            │
│      │                │◄──────────────────│                   │            │
│      │                │                   │                   │            │
│      │  6. Update     │                   │                   │            │
│      │◄───────────────│                   │                   │            │
│      │                │                   │                   │            │
│  ┌───┴───┐      ┌─────┴─────┐      ┌──────┴─────┐      ┌──────┴──────┐     │
│  │ User  │      │ ViewModel │      │  Service   │      │  Repository │     │
│  └───────┘      └───────────┘      └────────────┘      └─────────────┘     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Data Models

> ⚠️ REQUIRED

### 5.1 Primary Model

```csharp
/// <summary>
/// [Description of the model's purpose]
/// </summary>
public class [Feature]Model
{
    // ═══════════════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Unique identifier for this [feature]
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Human-readable name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// [Property description]
    /// </summary>
    public [Type] [PropertyName] { get; set; }

    // ═══════════════════════════════════════════════════════════════════════
    // RELATIONSHIPS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// [Relationship description]
    /// </summary>
    public [RelatedType]? [RelationName] { get; set; }

    // ═══════════════════════════════════════════════════════════════════════
    // TIMESTAMPS
    // ═══════════════════════════════════════════════════════════════════════

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 5.2 Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          ENTITY RELATIONSHIPS                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────┐                    ┌─────────────────┐                 │
│  │   [Entity A]    │                    │   [Entity B]    │                 │
│  ├─────────────────┤                    ├─────────────────┤                 │
│  │ PK: Id          │                    │ PK: Id          │                 │
│  │    Name         │                    │ FK: EntityAId   │                 │
│  │    Property1    │ 1              * │    Property1    │                 │
│  │    Property2    │◄───────────────────│    Property2    │                 │
│  └─────────────────┘                    └─────────────────┘                 │
│                                                │                            │
│                                                │ 1                          │
│                                                │                            │
│                                                ▼ *                          │
│                                         ┌─────────────────┐                 │
│                                         │   [Entity C]    │                 │
│                                         ├─────────────────┤                 │
│                                         │ PK: Id          │                 │
│                                         │ FK: EntityBId   │                 │
│                                         │    Property1    │                 │
│                                         └─────────────────┘                 │
│                                                                             │
│  LEGEND:                                                                    │
│  ─────────────────                                                          │
│  PK = Primary Key       1 ◄──────── * = One-to-Many                        │
│  FK = Foreign Key       * ◄──────── * = Many-to-Many                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.3 Database Schema

```sql
-- ═══════════════════════════════════════════════════════════════════════════
-- TABLE: [table_name]
-- ═══════════════════════════════════════════════════════════════════════════

CREATE TABLE [table_name] (
    -- Identity
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    name            TEXT            NOT NULL,

    -- Properties
    [column_name]   [TYPE]          [CONSTRAINTS],

    -- Relationships
    [foreign_key]   UUID            REFERENCES [other_table](id),

    -- Timestamps
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_[table]_[column] ON [table_name]([column_name]);

-- Triggers
CREATE TRIGGER update_[table]_timestamp
    BEFORE UPDATE ON [table_name]
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

---

## 6. Behaviors & Methods

> ⚠️ REQUIRED

### 6.1 Method: [MethodName]

#### Signature

```csharp
public async Task<[ReturnType]> [MethodName]([ParamType] param1, [ParamType] param2)
```

#### Purpose

[Describe what this method does and why it exists]

#### Parameters

| Parameter | Type | Required | Description | Validation |
|-----------|------|----------|-------------|------------|
| `param1` | `[Type]` | Yes | [Description] | [Validation rules] |
| `param2` | `[Type]` | No | [Description] | [Validation rules] |

#### Return Value

| Type | Description | Possible Values |
|------|-------------|-----------------|
| `[ReturnType]` | [Description] | [Examples] |

#### Algorithm

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        ALGORITHM: [MethodName]                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  INPUT: param1, param2                                                      │
│  OUTPUT: [ReturnType]                                                       │
│                                                                             │
│  1. VALIDATE inputs                                                         │
│     ├── IF param1 is null THEN throw ArgumentNullException                  │
│     └── IF param2 is invalid THEN throw ArgumentException                   │
│                                                                             │
│  2. PROCESS                                                                 │
│     ├── Step 2.1: [Description]                                             │
│     ├── Step 2.2: [Description]                                             │
│     └── Step 2.3: [Description]                                             │
│                                                                             │
│  3. CALCULATE result                                                        │
│     └── result = [formula or logic]                                         │
│                                                                             │
│  4. RETURN result                                                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### Pseudocode

```
FUNCTION [MethodName](param1, param2):
    LOG "Entering [MethodName] with param1={param1}, param2={param2}"

    // Validation
    IF param1 IS NULL:
        THROW ArgumentNullException("param1")

    // Processing
    intermediate_result = PROCESS(param1)

    // Calculation
    result = CALCULATE(intermediate_result, param2)

    LOG "Exiting [MethodName] with result={result}"
    RETURN result
```

#### Example Usage

```csharp
// Example 1: Basic usage
var service = serviceProvider.GetRequiredService<I[Feature]Service>();
var result = await service.[MethodName](value1, value2);

// Example 2: With error handling
try
{
    var result = await service.[MethodName](value1, value2);
    Console.WriteLine($"Success: {result}");
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Invalid argument: {ex.ParamName}");
}
```

---

### 6.2 Method: [MethodName2]

[Repeat structure for each method...]

---

## 7. Business Rules

> ⚠️ REQUIRED

### 7.1 Rule Matrix

| ID | Rule | Condition | Action | Exception |
|----|------|-----------|--------|-----------|
| BR-001 | [Rule name] | [When this happens] | [Do this] | [Unless this] |
| BR-002 | [Rule name] | [When this happens] | [Do this] | [Unless this] |
| BR-003 | [Rule name] | [When this happens] | [Do this] | [Unless this] |

### 7.2 Rule Flowchart

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      BUSINESS RULES FLOWCHART                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                         ┌─────────────────┐                                 │
│                         │  START: Input   │                                 │
│                         │   Received      │                                 │
│                         └────────┬────────┘                                 │
│                                  │                                          │
│                                  ▼                                          │
│                         ┌─────────────────┐                                 │
│                         │   BR-001:       │                                 │
│                    ┌────│   [Condition]?  │────┐                            │
│                    │    └─────────────────┘    │                            │
│                   YES                         NO                            │
│                    │                           │                            │
│                    ▼                           ▼                            │
│           ┌─────────────────┐         ┌─────────────────┐                   │
│           │  Apply Rule A   │         │   BR-002:       │                   │
│           │  [Action]       │    ┌────│   [Condition]?  │────┐              │
│           └────────┬────────┘    │    └─────────────────┘    │              │
│                    │            YES                         NO              │
│                    │             │                           │              │
│                    │             ▼                           ▼              │
│                    │    ┌─────────────────┐         ┌─────────────────┐     │
│                    │    │  Apply Rule B   │         │  Default Action │     │
│                    │    │  [Action]       │         │  [Action]       │     │
│                    │    └────────┬────────┘         └────────┬────────┘     │
│                    │             │                           │              │
│                    └─────────────┼───────────────────────────┘              │
│                                  │                                          │
│                                  ▼                                          │
│                         ┌─────────────────┐                                 │
│                         │   END: Result   │                                 │
│                         │   Generated     │                                 │
│                         └─────────────────┘                                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.3 Validation Rules

```csharp
public class [Feature]Validator : AbstractValidator<[Feature]Model>
{
    public [Feature]Validator()
    {
        // BR-001: [Rule description]
        RuleFor(x => x.[Property])
            .NotEmpty()
            .WithMessage("[Error message]");

        // BR-002: [Rule description]
        RuleFor(x => x.[Property])
            .[ValidationMethod]()
            .WithMessage("[Error message]");
    }
}
```

---

## 8. Restrictions & Limitations

> 📋 RECOMMENDED

### 8.1 Restrictions (MUST NOT)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           RESTRICTIONS                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ❌ R-001: [Restriction description]                                        │
│     └── Reason: [Why this is forbidden]                                     │
│     └── Impact: [What happens if violated]                                  │
│                                                                             │
│  ❌ R-002: [Restriction description]                                        │
│     └── Reason: [Why this is forbidden]                                     │
│     └── Impact: [What happens if violated]                                  │
│                                                                             │
│  ❌ R-003: [Restriction description]                                        │
│     └── Reason: [Why this is forbidden]                                     │
│     └── Impact: [What happens if violated]                                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Limitations (Boundaries)

| Limitation | Value | Unit | Rationale |
|------------|-------|------|-----------|
| Maximum [X] | [N] | [unit] | [Why this limit] |
| Minimum [Y] | [N] | [unit] | [Why this limit] |
| Timeout | [N] | ms | [Why this limit] |
| Batch size | [N] | items | [Why this limit] |

### 8.3 Known Limitations

| ID | Limitation | Workaround | Future Fix |
|----|------------|------------|------------|
| L-001 | [Current limitation] | [How to work around it] | [Planned resolution] |
| L-002 | [Current limitation] | [How to work around it] | [Planned resolution] |

---

## 9. Use Cases

> ⚠️ REQUIRED

### 9.1 UC-001: [Use Case Name]

| Attribute | Value |
|-----------|-------|
| **Actor** | [Who performs this] |
| **Preconditions** | [What must be true before] |
| **Postconditions** | [What is true after] |
| **Priority** | High / Medium / Low |

#### Main Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    USE CASE: UC-001 - [Name]                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. [Actor] initiates [action]                                              │
│     │                                                                       │
│     ▼                                                                       │
│  2. System validates [input]                                                │
│     │                                                                       │
│     ├──► [Validation passes]                                                │
│     │    │                                                                  │
│     │    ▼                                                                  │
│     │ 3. System processes [operation]                                       │
│     │    │                                                                  │
│     │    ▼                                                                  │
│     │ 4. System returns [result]                                            │
│     │    │                                                                  │
│     │    ▼                                                                  │
│     │ 5. [Actor] receives [outcome]                                         │
│     │                                                                       │
│     └──► [Validation fails] → See Alternative Flow A                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### Alternative Flows

| Flow | Trigger | Steps |
|------|---------|-------|
| A | Validation fails | 2a. Display error → Return to step 1 |
| B | [Trigger] | [Steps] |

#### Exception Flows

| Flow | Trigger | Steps |
|------|---------|-------|
| E1 | System error | Log error → Display generic message → Rollback |
| E2 | [Trigger] | [Steps] |

---

### 9.2 UC-002: [Use Case Name]

[Repeat structure...]

---

## 10. Decision Trees

> 📋 RECOMMENDED

### 10.1 [Decision Name] Decision Tree

```
┌─────────────────────────────────────────────────────────────────────────────┐
│               DECISION TREE: [Decision Name]                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                         ┌─────────────────┐                                 │
│                         │  [Initial       │                                 │
│                         │   Question?]    │                                 │
│                         └────────┬────────┘                                 │
│                                  │                                          │
│                    ┌─────────────┼─────────────┐                            │
│                    │             │             │                            │
│                   YES          MAYBE          NO                            │
│                    │             │             │                            │
│                    ▼             ▼             ▼                            │
│            ┌───────────┐ ┌───────────┐ ┌───────────┐                        │
│            │ Question  │ │ Question  │ │ Question  │                        │
│            │ 2A?       │ │ 2B?       │ │ 2C?       │                        │
│            └─────┬─────┘ └─────┬─────┘ └─────┬─────┘                        │
│                  │             │             │                              │
│            ┌─────┴─────┐ ┌─────┴─────┐ ┌─────┴─────┐                        │
│            │     │     │ │     │     │ │     │     │                        │
│           YES   NO    YES   NO    YES   NO                                  │
│            │     │     │     │     │     │                                  │
│            ▼     ▼     ▼     ▼     ▼     ▼                                  │
│         ┌────┐┌────┐┌────┐┌────┐┌────┐┌────┐                                │
│         │ A1 ││ A2 ││ B1 ││ B2 ││ C1 ││ C2 │                                │
│         └────┘└────┘└────┘└────┘└────┘└────┘                                │
│                                                                             │
│  OUTCOMES:                                                                  │
│  ─────────                                                                  │
│  A1: [Outcome description]                                                  │
│  A2: [Outcome description]                                                  │
│  B1: [Outcome description]                                                  │
│  B2: [Outcome description]                                                  │
│  C1: [Outcome description]                                                  │
│  C2: [Outcome description]                                                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 10.2 Decision Table

| Condition 1 | Condition 2 | Condition 3 | → Action |
|:-----------:|:-----------:|:-----------:|----------|
| T | T | T | Action A |
| T | T | F | Action B |
| T | F | T | Action C |
| T | F | F | Action D |
| F | T | T | Action E |
| F | T | F | Action F |
| F | F | T | Action G |
| F | F | F | Action H |

---

## 11. Error Handling

> 📋 RECOMMENDED

### 11.1 Error Catalog

| Code | Name | Severity | Message | Resolution |
|------|------|----------|---------|------------|
| E001 | [ErrorName] | Error | "[User message]" | [How to fix] |
| E002 | [ErrorName] | Warning | "[User message]" | [How to fix] |
| E003 | [ErrorName] | Critical | "[User message]" | [How to fix] |

### 11.2 Exception Hierarchy

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       EXCEPTION HIERARCHY                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Exception (System)                                                         │
│  └── [Feature]Exception (Base)                                              │
│      ├── [Feature]ValidationException                                       │
│      │   ├── Invalid[Property]Exception                                     │
│      │   └── Missing[Property]Exception                                     │
│      ├── [Feature]NotFoundException                                         │
│      ├── [Feature]ConflictException                                         │
│      └── [Feature]OperationException                                        │
│          ├── [Feature]TimeoutException                                      │
│          └── [Feature]ProcessingException                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 11.3 Error Handling Flow

```csharp
try
{
    // Operation code
}
catch ([Feature]ValidationException ex)
{
    _logger.LogWarning(ex, "Validation failed for [feature]");
    return Result.Failure(ex.Message);
}
catch ([Feature]NotFoundException ex)
{
    _logger.LogWarning(ex, "[Feature] not found: {Id}", ex.Id);
    return Result.NotFound(ex.Message);
}
catch ([Feature]Exception ex)
{
    _logger.LogError(ex, "Unexpected error in [feature]");
    return Result.Error("An unexpected error occurred");
}
```

---

## 12. Configuration

> 📋 RECOMMENDED

### 12.1 Configuration Schema

```json
{
  "[Feature]Settings": {
    "Enabled": true,
    "MaxItems": 100,
    "Timeout": 30000,
    "RetryCount": 3,
    "Options": {
      "Option1": "value1",
      "Option2": true
    }
  }
}
```

### 12.2 Configuration Class

```csharp
public class [Feature]Settings
{
    public const string SectionName = "[Feature]Settings";

    /// <summary>
    /// Whether this feature is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of items to process
    /// </summary>
    public int MaxItems { get; set; } = 100;

    /// <summary>
    /// Operation timeout in milliseconds
    /// </summary>
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 3;
}
```

### 12.3 Constants

```csharp
public static class [Feature]Constants
{
    // ═══════════════════════════════════════════════════════════════════════
    // LIMITS
    // ═══════════════════════════════════════════════════════════════════════

    public const int MaxNameLength = 100;
    public const int MinNameLength = 1;
    public const int MaxItems = 1000;

    // ═══════════════════════════════════════════════════════════════════════
    // DEFAULTS
    // ═══════════════════════════════════════════════════════════════════════

    public const int DefaultTimeout = 30000;
    public const int DefaultPageSize = 25;

    // ═══════════════════════════════════════════════════════════════════════
    // MAGIC NUMBERS (documented)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// [Explanation of why this value]
    /// </summary>
    public const double [SpecificValue] = 1.5;
}
```

---

## 13. Testing Strategy

> ⚠️ REQUIRED

### 13.1 Test Coverage Requirements

| Category | Minimum Coverage | Target Coverage |
|----------|------------------|-----------------|
| Service Methods | 80% | 95% |
| Business Rules | 100% | 100% |
| Error Handling | 80% | 90% |
| Edge Cases | 70% | 85% |

### 13.2 Test Matrix

| Test ID | Method | Scenario | Input | Expected Output | Priority |
|---------|--------|----------|-------|-----------------|----------|
| T-001 | `[Method]` | Happy path | Valid input | Success result | P1 |
| T-002 | `[Method]` | Null input | `null` | `ArgumentNullException` | P1 |
| T-003 | `[Method]` | Empty input | `""` | `ArgumentException` | P2 |
| T-004 | `[Method]` | Boundary value | Max value | Success at boundary | P2 |
| T-005 | `[Method]` | Over boundary | Max + 1 | Exception | P2 |

### 13.3 Test Categories

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          TEST PYRAMID                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                              /\                                             │
│                             /  \                                            │
│                            /E2E \                  E2E: 5-10%               │
│                           /──────\                                          │
│                          /        \                                         │
│                         /Integration\              Integration: 20-30%      │
│                        /──────────────\                                     │
│                       /                \                                    │
│                      /    Unit Tests    \          Unit: 60-70%             │
│                     /────────────────────\                                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 13.4 Sample Unit Test

```csharp
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

        _dependencyMock
            .Setup(x => x.[Method](It.IsAny<[Type]>()))
            .ReturnsAsync([value]);

        // Act
        var result = await _sut.[MethodName](input);

        // Assert
        result.Should().BeEquivalentTo(expected);
        _dependencyMock.Verify(x => x.[Method](It.IsAny<[Type]>()), Times.Once);
    }

    [Fact]
    public async Task [MethodName]_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        [InputType]? input = null;

        // Act
        var act = async () => await _sut.[MethodName](input!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("input");
    }
}
```

---

## 14. Cross-References

> 📋 RECOMMENDED

### 14.1 Dependencies (This Spec Requires)

| Specification | Type | Usage |
|---------------|------|-------|
| [SPEC-XXX-001](../domain/SPEC-XXX-001.md) | Hard | [How it's used] |
| [SPEC-YYY-002](../domain/SPEC-YYY-002.md) | Soft | [How it's used] |

### 14.2 Dependents (Other Specs Require This)

| Specification | Type | Usage |
|---------------|------|-------|
| [SPEC-ZZZ-001](../domain/SPEC-ZZZ-001.md) | Consumer | [How this is used] |

### 14.3 Related Design Documents

| Document | Relationship |
|----------|--------------|
| [DESIGN-XXX-001](../../design/DESIGN-XXX-001.md) | Design source |
| [LORE-YYY-001](../../lore/LORE-YYY-001.md) | Content reference |

### 14.4 Implementation Files

| File | Purpose |
|------|---------|
| `src/Services/I[Feature]Service.cs` | Interface definition |
| `src/Services/[Feature]Service.cs` | Implementation |
| `src/Models/[Feature]Model.cs` | Data model |
| `tests/Services/[Feature]ServiceTests.cs` | Unit tests |

---

## 15. Design Rationale

> 📎 OPTIONAL

### 15.1 Why This Approach?

[Explain the reasoning behind key design decisions]

### 15.2 Alternatives Considered

| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| [Alternative 1] | [Benefits] | [Drawbacks] | [Reason] |
| [Alternative 2] | [Benefits] | [Drawbacks] | [Reason] |

### 15.3 Trade-offs Accepted

| Trade-off | Benefit Gained | Cost Accepted |
|-----------|----------------|---------------|
| [Trade-off 1] | [What we get] | [What we sacrifice] |
| [Trade-off 2] | [What we get] | [What we sacrifice] |

---

## 16. Implementation Checklist

> ⚠️ REQUIRED

### 16.1 Pre-Implementation

- [ ] Specification reviewed and approved
- [ ] Dependencies identified and available
- [ ] Test cases defined
- [ ] Database migrations planned

### 16.2 Implementation

- [ ] Interface created: `I[Feature]Service.cs`
- [ ] Implementation created: `[Feature]Service.cs`
- [ ] Models created: `[Feature]Model.cs`
- [ ] DI registration added
- [ ] Logging implemented (entry/exit/exceptions)
- [ ] Error handling implemented
- [ ] Configuration wired up

### 16.3 Testing

- [ ] Unit tests written and passing
- [ ] Integration tests written and passing
- [ ] Code coverage meets requirements (≥80%)
- [ ] Edge cases tested

### 16.4 Documentation

- [ ] XML documentation complete
- [ ] README updated (if applicable)
- [ ] Changelog entry added

### 16.5 Review

- [ ] Code review completed
- [ ] Security review (if applicable)
- [ ] Performance review (if applicable)
- [ ] Specification status updated to `implemented`

---

## 17. Changelog

> 🔒 LOCKED - Format is standardized

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | YYYY-MM-DD | [Author] | Initial specification |

---

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                              END OF TEMPLATE                                  ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  CHECKLIST BEFORE SUBMISSION:                                                 ║
║  ────────────────────────────────────────────────────────────────────────────║
║  □ All [PLACEHOLDER] text replaced                                           ║
║  □ All ⚠️ REQUIRED sections completed                                         ║
║  □ Frontmatter is valid YAML                                                  ║
║  □ Cross-references link to existing documents                                ║
║  □ Code examples are syntactically correct                                    ║
║  □ Diagrams render correctly                                                  ║
║  □ Status set to 'review' when ready                                          ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->
