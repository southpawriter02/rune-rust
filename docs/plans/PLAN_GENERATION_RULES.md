# Agent Instructions: Generating High-Fidelity Implementation Plans

**Goal:** Generate comprehensive implementation plans that serve as both a roadmap for development and a technical specification. The output must match the level of detail found in `v0.3.10.md` (multi-phase) or `v0.3.11.md` (two-phase).

---

## 1. General Formatting Rules

### File Naming & Location
- **Filename:** `vX.Y.Z.md` (version file)
- **Location:** `docs/plans/` folder

### Title Header Format
```markdown
# vX.Y.Z: The Codename (Feature Theme)

> **Status:** Planned | In Progress | Complete
> **Milestone:** X.Y - Milestone Name
> **Theme:** Brief description of focus area
```

### General Formatting
- **Separators:** Use horizontal rules `---` between major sections
- **Language:** Technical, precise, future tense for planned features
- **Headers:** Use `##` for major sections, `###` for subsections, `####` for sub-subsections
- **Lists:** Use standard markdown `-` for bullets, proper indentation for nesting
- **Code Blocks:** Always use triple backticks with language hint (e.g., ` ```csharp `)

---

## 2. Table of Contents (Required)

**Placement:** Immediately after the metadata block, before the Overview section.

### Format
```markdown
## Table of Contents
- [Overview](#overview)
- [Phase A: The Codename](#phase-a-the-codename)
- [Phase B: The Codename](#phase-b-the-codename)
- [Phase C: The Codename](#phase-c-the-codename)
- [Testing Strategy](#testing-strategy)
- [Changelog](#changelog-vxyz---the-codename)
```

### Anchor Link Rules (GitHub-Flavored Markdown)
- Convert header text to lowercase
- Replace spaces with hyphens (`-`)
- Remove punctuation (except hyphens)
- For duplicate headers: append `-1`, `-2`, etc.

---

## 3. Overview Section (Required)

### Structure
```markdown
## Overview

[1-2 paragraph summary of the version's goals and architectural approach]

To manage [complexity description], this version is split into [N] sub-versions:

| Phase | Codename | Focus |
|-------|----------|-------|
| A | The Name | Brief description of Phase A |
| B | The Name | Brief description of Phase B |
| C | The Name | Brief description of Phase C |
```

### Guidelines
- Explain the "why" behind the version
- Identify architectural patterns being introduced
- Reference predecessor versions if building on previous work

---

## 4. Phase Sections (Required)

Each phase follows a consistent internal structure. Repeat for each phase (A, B, C, etc.).

### Phase Header Format
```markdown
---

## Phase A: The Codename (Feature Name)

**Goal:** [Single sentence describing the deliverable]
```

### 4.1 Architecture & Data Flow

**Purpose:** Explain how components interact and data moves through the system.

```markdown
### 1. Architecture & Data Flow

[Prose description of the architectural approach]

**Components:**
- **ComponentName (Layer):** Role and responsibility
- **ComponentName (Layer):** Role and responsibility

**Workflow: [Scenario Name]**
1. **Step Name:** Description
2. **Step Name:** Description
   - Sub-step detail
   - Sub-step detail
```

### 4.2 Use Cases (Optional)

**Purpose:** Concrete examples of how the feature behaves in practice.

```markdown
### 2. Use Cases

1. **Scenario Name:** User does X. System responds with Y.
2. **Scenario Name:** User does X. System responds with Y.
3. **Edge Case:** Description of edge case handling.
```

### 4.3 Logic Decision Trees

**Purpose:** Precise branching logic for complex operations.

```markdown
### 3. Logic Decision Trees

#### A. Operation Name
**Input:** Description of input

1. **Step Name:** Description
   - Condition A: Result
   - Condition B: Result
2. **Step Name:** Description
   - Sub-condition: Detail
```

### 4.4 Code Implementation

**Purpose:** Concrete code examples showing the implementation approach.

```markdown
### 4. Code Implementation

#### A. Layer Name (Component Type)
**File:** `Project.Layer/Folder/FileName.cs`

```csharp
// Code example with key implementation details
public class ServiceName : IServiceName
{
    // Implementation
}
```
```

**Guidelines:**
- Show interface contracts first, then implementations
- Include key methods, not boilerplate
- Add inline comments for non-obvious logic

### 4.5 Logging Requirements

**Purpose:** Document all structured log events for the phase.

```markdown
### 5. Logging Requirements

| System | Event | Level | Message Template | Properties |
|--------|-------|-------|------------------|------------|
| ServiceName | EventName | Info/Debug/Warn/Error | "Message with {Placeholder}" | Placeholder |
```

**Guidelines:**
- Use Serilog message template syntax
- Include all relevant properties
- Match log levels to event significance

### 4.6 Testing Requirements

**Purpose:** Define the testing strategy and specific test cases.

```markdown
### 6. Testing Requirements

**Unit Tests (TestClassName.cs)**
- **TestName:** Description of what is being tested and expected outcome
- **TestName:** Description

**Integration Tests**
- **Scenario:** End-to-end workflow description
```

### 4.7 Deliverable Checklist

**Purpose:** Actionable items that must be completed for the phase.

```markdown
### 7. Deliverable Checklist

- [ ] Layer: Task description
- [ ] Layer: Task description
- [ ] Layer: Task description
```

**Guidelines:**
- Group by layer (Core, Engine, Terminal, Test)
- Use checkbox syntax for tracking
- Be specific about what file or component

---

## 5. Testing Strategy Section (Required)

**Placement:** After all phase sections, before the Changelog.

```markdown
---

## Testing Strategy

### Unit Tests
| Test Class | Focus | Key Scenarios |
|------------|-------|---------------|
| ServiceNameTests | Feature focus | Scenario list |

### Integration Tests
| Scenario | Components | Validation |
|----------|------------|------------|
| Workflow name | Component list | What to verify |
```

---

## 6. Changelog Section (Required)

**Placement:** At the end of the document.

```markdown
---

## Changelog: vX.Y.Z - The Codename

**Release Date:** YYYY-XX-XX

### Summary
[1-2 paragraph summary of what was delivered]

### Features
- **Feature Name:** Description
- **Feature Name:** Description

### Technical
- Implementation detail
- Implementation detail
```

---

## 7. Multi-Phase vs Single-Phase Plans

### When to Use Multi-Phase
- Feature spans multiple sub-versions (a, b, c)
- Distinct architectural boundaries between components
- Phases have independent deliverables

### When to Use Single-Phase
- Small feature with unified implementation
- No natural breaking points
- Can be delivered in one release

### Single-Phase Simplification
For single-phase plans, omit the phase table and use simplified headers:

```markdown
## Overview

[Summary paragraph]

---

## Architecture & Data Flow

[Content]

---

## Code Implementation

[Content]
```

---

## 8. Quality Checklist

Before finalizing a plan, verify:

- [ ] **Metadata Block:** Status, Milestone, Theme present
- [ ] **Table of Contents:** All sections linked with valid anchors
- [ ] **Phase Table:** Phases summarized in Overview
- [ ] **Architecture:** Data flow and components documented
- [ ] **Decision Trees:** Complex logic branches specified
- [ ] **Code Examples:** Key implementations shown with file paths
- [ ] **Logging Tables:** All log events documented with proper format
- [ ] **Test Strategy:** Unit and integration tests defined
- [ ] **Checklists:** Deliverables broken down by layer
- [ ] **Horizontal Rules:** `---` between major sections
- [ ] **Consistent Headers:** `###` for numbered sections, `####` for sub-sections
- [ ] **Markdown Lists:** Standard `-` syntax, no special bullets
- [ ] **Code Fencing:** All code blocks have language hints

---

## 9. Reference Examples

| Type | Reference File | Description |
|------|----------------|-------------|
| Multi-Phase (3 phases) | [v0.3.10.md](v0.3.10.md) | Golden standard with TOC, three phases, comprehensive testing |
| Multi-Phase (2 phases) | [v0.3.11.md](v0.3.11.md) | Two-phase plan with detailed specifications |
| Single-Phase | [v0.0.2.md](v0.0.2.md) | Simpler single-phase structure |

---

## 10. Common Mistakes to Avoid

### Formatting Errors
- **Tab-indented bullets:** Use `-` with space indentation, not `•` or `◦`
- **Broken tables:** Ensure all columns are on a single line with `|` delimiters
- **Missing code fencing:** Always use ` ```language ` for code blocks
- **Inconsistent headers:** Maintain hierarchy (`##` > `###` > `####`)

### Content Errors
- **Vague descriptions:** Be specific about what, where, and how
- **Missing file paths:** Always include the full path to files
- **Incomplete logging:** Document ALL log events, not just a sample
- **Generic test names:** Test names should describe behavior, not just "Test1"

### Structural Errors
- **Missing Overview table:** Always include the phase summary table
- **No horizontal rules:** Separate major sections with `---`
- **Missing TOC:** Every plan needs a Table of Contents
- **Orphan sections:** Every phase needs all required subsections

---

## 11. Workflow: Creating a New Plan

1. **Create File:** `docs/plans/vX.Y.Z.md`
2. **Add Metadata:** Title, Status, Milestone, Theme
3. **Draft Overview:** Summary + phase table
4. **Add TOC:** Link to all planned sections
5. **Build Phases:** For each phase:
   - Architecture & Data Flow
   - Logic Decision Trees (if complex)
   - Code Implementation
   - Logging Requirements
   - Testing Requirements
   - Deliverable Checklist
6. **Add Testing Strategy:** Consolidated test matrix
7. **Draft Changelog:** Placeholder for release notes
8. **Validate:** Run through Quality Checklist
9. **Update README:** Add version to `docs/plans/README.md`

---

*Generated by The Architect - Rune & Rust Development*
