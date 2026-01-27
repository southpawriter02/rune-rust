# Version Scope Breakdown Template

Use this template for creating scope breakdown files (e.g., `v0.0.X-scope-breakdown.md`) when a version is complex enough to warrant multiple sub-phases.

---

# v0.0.X [Version Theme] - Scope Breakdown

**Version:** 0.0.X
**Theme:** [Version Theme/Name]
**Prerequisites:** [e.g., "v0.0.5 Complete (Dice Pool System)"]
**Total Estimated Tests:** ~[number] new tests

---

## Executive Summary

[2-3 sentence description of what this version accomplishes and why it's being broken into sub-phases.]

The work is divided into **[number] sub-phases**:

| Phase | Name | Focus | Est. Tests |
|-------|------|-------|------------|
| v0.0.Xa | [Phase Name] | [Brief focus] | ~[number] |
| v0.0.Xb | [Phase Name] | [Brief focus] | ~[number] |
| v0.0.Xc | [Phase Name] | [Brief focus] | ~[number] |

---

## Feature Analysis & Categorization

### From v0.0.X-index.md - [Category Name]

| Feature | Complexity | Dependencies | Assigned Phase |
|---------|------------|--------------|----------------|
| [Feature Name] | Low/Medium/High | [Dependencies] | **v0.0.Xa** |
| [Feature Name] | Low/Medium/High | [Already in v0.0.Y] | *Complete* |

### From v0.0.X-index.md - [Category Name]

| Feature | Complexity | Dependencies | Assigned Phase |
|---------|------------|--------------|----------------|
| [Feature Name] | Low/Medium/High | [Dependencies] | **v0.0.Xb** |

---

## Phase Definitions

---

## v0.0.Xa: [Phase Name]

[v0.0.Xa Design Specification](v0.0.Xa-design-specification.md)

### Overview
[1-2 sentence description of what this phase accomplishes.]

### Scope

**In Scope:**
- [Bullet points of what IS included in this phase]
- [Include specific features, entities, services]

**Out of Scope:**
- [Features explicitly NOT included, with reference to future phase]
- [Example: "Monster AI behaviors (v0.0.Xb)"]

### Key Deliverables

| Type | Count | Details |
|------|-------|---------|
| Domain Entities | [#] | [Entity names] |
| Domain Value Objects | [#] | [Value object names] |
| Domain Services | [#] | [Service names] |
| Application Services | [#] | [Service names or "Update [ServiceName]"] |
| Commands | [#] | [Command names] |
| DTOs | [#] | [DTO names] |
| Configuration | [#] | [Config file names] |
| Unit Tests | ~[#] | |

### Data Model Changes

```
NEW: [EntityName] (Aggregate/Entity/Value Object)
├── Property: Type
├── Property: Type
└── Property: Type

MODIFY: [ExistingEntityName]
├── ADD: Property: Type
└── ADD: Property: Type

CLARIFY: [EntityName]
└── [Clarification about existing behavior]
```

### User-Facing Changes

**Commands:**
```
> [command]              # [Description]
> [command] <arg>        # [Description with argument]
```

**Output Example:**
```
[Example of what the user sees when using this feature]
```

### Acceptance Criteria

- [ ] [Specific, testable criterion]
- [ ] [Specific, testable criterion]
- [ ] ~[#] unit tests pass

---

## v0.0.Xb: [Phase Name]

[v0.0.Xb Design Specification](v0.0.Xb-design-specification.md)

### Overview
[1-2 sentence description of what this phase accomplishes.]

### Scope

**In Scope:**
- [Items in scope]

**Out of Scope:**
- [Items not in scope]

### Key Deliverables

| Type | Count | Details |
|------|-------|---------|
| [Type] | [#] | [Details] |

### [Feature-Specific Section] *(optional)*

[If the phase has a significant feature, provide details here, such as enum definitions, pseudocode, or configuration examples.]

### Acceptance Criteria

- [ ] [Specific, testable criterion]
- [ ] ~[#] unit tests pass

---

## Dependencies & Prerequisites

```
v0.0.Y ([Prerequisite Version]) - REQUIRED
    │
    ├── v0.0.Ya: [Phase Name] ──────┐
    ├── v0.0.Yb: [Phase Name] ──────┤
    └── v0.0.Yc: [Phase Name] ──────┘
                                    │
                                    ▼
v0.0.X ([This Version])
    │
    ├── v0.0.Xa: [Phase Name] ──────────────────┐
    │       Dependencies: [specific dependency]  │
    │                                            │
    ├── v0.0.Xb: [Phase Name] ──────────────────┤
    │       Dependencies: v0.0.Xa               │
    │                                            │
    └── v0.0.Xc: [Phase Name] ──────────────────┘
            Dependencies: v0.0.Xa, v0.0.Xb
```

---

## Estimated Effort Summary

| Phase | New Files | Modified Files | Est. Tests | Complexity |
|-------|-----------|----------------|------------|------------|
| v0.0.Xa | ~[#] | ~[#] | ~[#] | Low/Medium/High |
| v0.0.Xb | ~[#] | ~[#] | ~[#] | Low/Medium/High |
| **Total** | **~[#]** | **~[#]** | **~[#]** | |

---

## Risk Assessment

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| [Risk description] | High/Medium/Low | High/Medium/Low | [Mitigation strategy] |

---

## Design Decisions (Confirmed)

*Document confirmed design decisions here so they apply across all sub-phases.*

### [Category]

| Decision | Value | Notes |
|----------|-------|-------|
| **[Decision Name]** | [Value] | [Explanation] |

---

## Next Steps

1. **Review & Approve** - Confirm scope breakdown with stakeholders
2. **v0.0.Xa Design Spec** - Create detailed design specification
3. **v0.0.Xa Implementation Plan** - Create implementation plan *(optional)*
4. **Implement v0.0.Xa** - Build [phase focus]
5. **Repeat for v0.0.Xb, v0.0.Xc**

---

*This scope breakdown provides a structured approach to implementing v0.0.X [Version Theme]. Each sub-phase builds on the previous, allowing for incremental development and testing.*

---

## Usage Notes

1. **When to create a scope breakdown:**
   - Version has 6+ features
   - Features have complex interdependencies
   - Total estimated effort exceeds 50 new tests
   - Multiple distinct areas of the codebase are affected

2. **Phase naming:**
   - Use lowercase letters (a, b, c, d)
   - Maximum 4-5 phases per version
   - Each phase should be independently testable

3. **Dependency management:**
   - Later phases should depend on earlier phases
   - Avoid circular dependencies
   - Consider parallel development where possible
