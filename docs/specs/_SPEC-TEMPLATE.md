# Specification Template

> **Workflow:** See [SPEC-WORKFLOW-001](./SPEC-WORKFLOW-001.md) for creation, deep dive, and update workflows.
> **Usage:** Copy this template when creating a new specification.

---

## Template Instructions

1. **Copy this file** to the appropriate domain subdirectory
2. **Rename** to `SPEC-[ABBREV]-NNN.md` (e.g., `SPEC-COMBAT-001.md`)
3. **Replace all `[PLACEHOLDER]` text** with actual content
4. **Delete this instructions section** before finalizing
5. **Run the Validation Checklist** from SPEC-WORKFLOW-001

---

## Begin Template

```markdown
---
id: SPEC-[DOMAIN]-[NNN]
title: [Descriptive Title]
version: 1.0.0
status: Draft
last_updated: [YYYY-MM-DD]
related_specs: []
---

# SPEC-[DOMAIN]-[NNN]: [Title]

> **Version:** 1.0.0
> **Status:** Draft
> **Service:** `[PrimaryService]`, `[SecondaryService]`
> **Location:** `RuneAndRust.[Layer]/[Path]/[Service].cs`

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Behaviors](#behaviors)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Changelog](#changelog)

---

## Overview

[2-3 paragraphs describing what this system does, why it exists, and its role in the game.]

---

## Core Concepts

### [Concept 1 Name]

[Definition and explanation]

### [Concept 2 Name]

[Definition and explanation]

---

## Behaviors

### Primary Behaviors

#### 1. [Behavior Name] (`MethodName`)

```csharp
ReturnType MethodName(Parameters)
```

**Purpose:** [What this behavior does]

**Logic:**
1. [Step 1]
2. [Step 2]
3. [Step 3]

**Example:**
```csharp
// Example usage
var result = service.MethodName(param1, param2);
```

#### 2. [Behavior Name] (`MethodName`)

[Repeat structure for each major behavior]

---

## Restrictions

### What This System MUST NOT Do

1. **[Restriction Name]:** [Explanation]
2. **[Restriction Name]:** [Explanation]

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| [Limit Name] | [Value] | [Why this limit exists] |
| [Limit Name] | [Value] | [Why this limit exists] |

---

## Use Cases

### UC-1: [Use Case Name]

**Actor:** [Who performs this action]
**Trigger:** [What initiates this use case]
**Preconditions:** [What must be true before]

```csharp
// Code example showing the use case
[code]
```

**Postconditions:** [What is true after]

### UC-2: [Use Case Name]

[Repeat structure]

---

## Decision Trees

### [Flow Name]

```
┌─────────────────────────────┐
│  [Starting State]           │
└────────────┬────────────────┘
             │
    ┌────────┴────────┐
    │ [Decision]      │
    └────────┬────────┘
             │
    ┌────────┼────────┐
    │        │        │
    ▼        ▼        ▼
[Option1] [Option2] [Option3]
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `[ServiceName]` | [SPEC-XXX-001](../domain/SPEC-XXX-001.md) | [How it's used] |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `[ServiceName]` | [SPEC-XXX-001](../domain/SPEC-XXX-001.md) | [How it's used] |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `[ServiceName].cs` | [Purpose] | [Line range] |

### Supporting Types

| File | Purpose | Key Lines |
|------|---------|-----------|
| `[TypeName].cs` | [Purpose] | [Line range] |

---

## Data Models

### [Entity Name]

```csharp
public class [EntityName]
{
    // Core properties
    public Guid Id { get; set; }

    // [Category]
    [properties]
}
```

### [Enum Name]

```csharp
public enum [EnumName]
{
    [Value1] = 0,  // [Description]
    [Value2] = 1,  // [Description]
}
```

---

## Configuration

### Constants

```csharp
// [Category]
private const int [ConstantName] = [Value];
```

### Settings

| Setting | Default | Range | Purpose |
|---------|---------|-------|---------|
| [Setting] | [Default] | [Range] | [Purpose] |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `[TestFileName].cs` | [Count] | [Coverage area] |

### Critical Test Scenarios

1. [Scenario description]
2. [Scenario description]
3. [Scenario description]

### Validation Checklist

- [ ] [Validation item 1]
- [ ] [Validation item 2]
- [ ] [Validation item 3]

---

## Design Rationale

### Why [Design Decision]?

- [Reason 1]
- [Reason 2]
- [Reason 3]

### Why [Design Decision]?

- [Reason 1]
- [Reason 2]

---

## Changelog

### v1.0.0 ([YYYY-MM-DD])

- Initial specification
```

---

## Quick Reference

### Required Sections (15)

1. Table of Contents
2. Overview
3. Core Concepts
4. Behaviors
5. Restrictions
6. Limitations
7. Use Cases
8. Decision Trees
9. Cross-Links
10. Related Services
11. Data Models
12. Configuration
13. Testing
14. Design Rationale
15. Changelog

### Frontmatter Fields

| Field | Required | Example |
|-------|----------|---------|
| `id` | Yes | `SPEC-COMBAT-001` |
| `title` | Yes | `Combat System` |
| `version` | Yes | `1.0.0` |
| `status` | Yes | `Draft`, `Review`, `Approved`, `Deprecated`, `Scaffolded`, `Implemented`, `In Progress` |
| `last_updated` | Yes | `2025-12-23` |
| `related_specs` | Yes | `[SPEC-DICE-001, SPEC-STATUS-001]` |

### Domain Abbreviations

| Domain | Abbreviation | Example |
|--------|--------------|---------|
| Core | CORE, DICE, GAME | SPEC-DICE-001 |
| Combat | COMBAT, ABILITY, ATTACK, STATUS, AI, ENEMY, ENEMYFAC, TRAIT | SPEC-COMBAT-001 |
| Character | CHAR, ADVANCEMENT, XP, LEGEND, TRAUMA, CORRUPT, RESOURCE, REST | SPEC-CHAR-001 |
| Exploration | NAV, DUNGEON, ENVPOP, SPAWN, INTERACT | SPEC-NAV-001 |
| Environment | HAZARD, COND | SPEC-HAZARD-001 |
| Economy | INV, CRAFT, REPAIR, LOOT | SPEC-INV-001 |
| Knowledge | CODEX, CAPTURE, JOURNAL | SPEC-CODEX-001 |
| Content | DESC, TEMPLATE | SPEC-DESC-001 |
| UI | UI, RENDER, INPUT, THEME | SPEC-UI-001 |
| Data | SAVE, REPO, SEED, MIGRATE | SPEC-SAVE-001 |

### Validation Checklist

See [SPEC-WORKFLOW-001](./SPEC-WORKFLOW-001.md#validation-checklist) for the complete 20+ item checklist.
