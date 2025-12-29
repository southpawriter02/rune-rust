# DESIGN-[DOMAIN]-[NNN]: [Design Document Title]

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                           DESIGN TEMPLATE                                     ║
║                            Version 1.0.0                                      ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  PURPOSE: Game mechanics, systems design, and feature documentation          ║
║  LAYER: Layer 4 (Ground Truth) - Designer's perspective                      ║
║  AUDIENCE: Game Designers, Developers, Balance Team                          ║
║                                                                              ║
║  This template covers mechanics design before specification.                  ║
║  Once approved, designs feed into SPEC documents for implementation.          ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->

---

```yaml
# ══════════════════════════════════════════════════════════════════════════════
# FRONTMATTER - ⚠️ REQUIRED
# ══════════════════════════════════════════════════════════════════════════════
id: DESIGN-[DOMAIN]-[NNN]
title: "[Design Document Title]"
version: 1.0.0
status: draft                    # draft | review | approved | implemented | deprecated
layer: Layer 4
design-type: "[Type]"            # Mechanic | System | Feature | Balance | UI/UX | Content

# Classification
domain: "[DOMAIN]"               # COMBAT | CHAR | NAV | INV | UI | ENV | STATUS | ABILITY | etc.
scope: "[Scope]"                 # Core | Subsystem | Feature | Polish
complexity: "[Complexity]"       # Simple | Moderate | Complex | Epic
priority: P2                     # P1-Critical | P2-High | P3-Medium | P4-Low

# Dependencies
requires:
  - DESIGN-[DOMAIN]-[NNN]        # Design dependencies
blocks:
  - SPEC-[DOMAIN]-[NNN]          # What this blocks

# Metadata
last-updated: YYYY-MM-DD
author: "[Author Name]"
reviewers: []
stakeholders:
  - "[Stakeholder 1]"
  - "[Stakeholder 2]"

# Output
target-specs:
  - SPEC-[DOMAIN]-[NNN]          # Specifications this will generate

tags:
  - [tag1]
  - [tag2]
```

---

## Table of Contents

1. [Design Overview](#1-design-overview)
2. [Design Goals](#2-design-goals)
3. [Player Experience](#3-player-experience)
4. [Core Mechanics](#4-core-mechanics)
5. [System Integration](#5-system-integration)
6. [Balance Framework](#6-balance-framework)
7. [Edge Cases & Exceptions](#7-edge-cases--exceptions)
8. [User Interface](#8-user-interface)
9. [Progression & Unlocks](#9-progression--unlocks)
10. [Testing & Validation](#10-testing--validation)
11. [Implementation Phases](#11-implementation-phases)
12. [Risk Assessment](#12-risk-assessment)
13. [Open Questions](#13-open-questions)
14. [Design Changelog](#14-design-changelog)

---

## Design Document Workflow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    DESIGN DOCUMENT LIFECYCLE                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────┐                   │
│  │   DRAFT     │────►│   REVIEW    │────►│  APPROVED   │                   │
│  │             │     │             │     │             │                   │
│  │  Initial    │     │  Feedback   │     │  Ready for  │                   │
│  │  concept    │     │  iteration  │     │  spec gen   │                   │
│  └─────────────┘     └──────┬──────┘     └──────┬──────┘                   │
│                             │                   │                          │
│                    [Revisions needed]    [Generate specs]                   │
│                             │                   │                          │
│                             ▼                   ▼                          │
│                      ┌─────────────┐     ┌─────────────┐                   │
│                      │   DRAFT     │     │ IMPLEMENTED │                   │
│                      │  (revised)  │     │             │                   │
│                      └─────────────┘     │  Specs      │                   │
│                                          │  created &  │                   │
│                                          │  code done  │                   │
│                                          └─────────────┘                   │
│                                                                             │
│  STATUS MEANINGS:                                                           │
│  ├── draft: Work in progress, incomplete                                    │
│  ├── review: Ready for stakeholder feedback                                 │
│  ├── approved: Design finalized, ready for specification                    │
│  ├── implemented: Corresponding specs exist and are implemented             │
│  └── deprecated: Superseded or cancelled                                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Design Overview

> ⚠️ REQUIRED | Executive summary for stakeholders

### 1.1 Elevator Pitch

> One paragraph maximum

[What is this design? What player need does it address? Why does it matter?]

### 1.2 Design Summary

| Aspect | Description |
|--------|-------------|
| **What** | [What is being designed] |
| **Why** | [Problem being solved / opportunity] |
| **Who** | [Target player type] |
| **Where** | [Where in the game this appears] |
| **When** | [When players encounter this] |

### 1.3 Success Criteria

- [ ] **SC-1:** [Measurable success criterion]
- [ ] **SC-2:** [Measurable success criterion]
- [ ] **SC-3:** [Measurable success criterion]

### 1.4 Related Documents

| Document | Type | Relationship |
|----------|------|--------------|
| [Document] | [Design/Spec/Lore] | [How related] |

---

## 2. Design Goals

> ⚠️ REQUIRED

### 2.1 Primary Goals

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         DESIGN GOALS                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  🎯 PRIMARY GOALS (Must achieve)                                            │
│  ════════════════════════════════                                           │
│                                                                             │
│  1. [Goal Name]                                                             │
│     └── [Description of what this means and why it matters]                 │
│     └── Metric: [How we'll know we achieved it]                             │
│                                                                             │
│  2. [Goal Name]                                                             │
│     └── [Description]                                                       │
│     └── Metric: [Measurement]                                               │
│                                                                             │
│  3. [Goal Name]                                                             │
│     └── [Description]                                                       │
│     └── Metric: [Measurement]                                               │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  🎯 SECONDARY GOALS (Nice to have)                                          │
│  ══════════════════════════════════                                         │
│                                                                             │
│  4. [Goal Name]                                                             │
│     └── [Description]                                                       │
│                                                                             │
│  5. [Goal Name]                                                             │
│     └── [Description]                                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Non-Goals

> Explicitly state what this design is NOT trying to do

| Non-Goal | Rationale |
|----------|-----------|
| [What we're not doing] | [Why we're not doing it] |
| [What we're not doing] | [Why we're not doing it] |

### 2.3 Design Pillars

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       DESIGN PILLARS                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│        ┌─────────────┐   ┌─────────────┐   ┌─────────────┐                  │
│        │  PILLAR 1   │   │  PILLAR 2   │   │  PILLAR 3   │                  │
│        │             │   │             │   │             │                  │
│        │  [Name]     │   │  [Name]     │   │  [Name]     │                  │
│        │             │   │             │   │             │                  │
│        └──────┬──────┘   └──────┬──────┘   └──────┬──────┘                  │
│               │                 │                 │                         │
│               └─────────────────┼─────────────────┘                         │
│                                 │                                           │
│                                 ▼                                           │
│                    ┌─────────────────────────┐                              │
│                    │     DESIGN OUTPUT       │                              │
│                    │   [This Feature/System] │                              │
│                    └─────────────────────────┘                              │
│                                                                             │
│  PILLAR DEFINITIONS:                                                        │
│  ───────────────────                                                        │
│  [Pillar 1]: [What it means for this design]                                │
│  [Pillar 2]: [What it means for this design]                                │
│  [Pillar 3]: [What it means for this design]                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Player Experience

> ⚠️ REQUIRED

### 3.1 Target Experience

> What should the player feel?

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     EMOTIONAL JOURNEY                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ENCOUNTER → LEARN → MASTER → EXCEL                                         │
│                                                                             │
│  ┌──────────────┐                                                           │
│  │  ENCOUNTER   │  Emotion: [What players feel on first exposure]           │
│  │              │  Goal: [What we want them to understand]                  │
│  └──────┬───────┘                                                           │
│         │                                                                   │
│         ▼                                                                   │
│  ┌──────────────┐                                                           │
│  │    LEARN     │  Emotion: [What players feel while learning]              │
│  │              │  Goal: [What skills/knowledge they develop]               │
│  └──────┬───────┘                                                           │
│         │                                                                   │
│         ▼                                                                   │
│  ┌──────────────┐                                                           │
│  │   MASTER     │  Emotion: [What players feel at competency]               │
│  │              │  Goal: [What mastery looks like]                          │
│  └──────┬───────┘                                                           │
│         │                                                                   │
│         ▼                                                                   │
│  ┌──────────────┐                                                           │
│  │    EXCEL     │  Emotion: [What players feel at expertise]                │
│  │              │  Goal: [What excellence rewards]                          │
│  └──────────────┘                                                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 User Stories

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  USER STORY: US-001                                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  AS A [player type]                                                         │
│  I WANT TO [action/capability]                                              │
│  SO THAT [benefit/outcome]                                                  │
│                                                                             │
│  ACCEPTANCE CRITERIA:                                                       │
│  ├── AC-1: [Specific testable criterion]                                    │
│  ├── AC-2: [Specific testable criterion]                                    │
│  └── AC-3: [Specific testable criterion]                                    │
│                                                                             │
│  PRIORITY: [Must Have / Should Have / Nice to Have]                         │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.3 Player Scenarios

| Scenario | Player Type | Context | Expected Behavior | Outcome |
|----------|-------------|---------|-------------------|---------|
| [Name] | [New/Experienced/Expert] | [Situation] | [What they do] | [What happens] |

### 3.4 Friction Points

| Potential Friction | Cause | Mitigation |
|-------------------|-------|------------|
| [Frustration point] | [Why it might happen] | [How we prevent/address] |

---

## 4. Core Mechanics

> ⚠️ REQUIRED

### 4.1 Mechanic Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      CORE MECHANIC: [Name]                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  TYPE: [Action / Resource / State / Progression / Social]                   │
│  FREQUENCY: [How often players engage with this]                            │
│  DEPTH: [Simple / Moderate / Deep / Complex]                                │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  DESCRIPTION:                                                               │
│  [Clear explanation of how the mechanic works]                              │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  INPUT → PROCESS → OUTPUT                                                   │
│                                                                             │
│  ┌────────────┐      ┌────────────────┐      ┌────────────┐                │
│  │   INPUT    │      │    PROCESS     │      │   OUTPUT   │                │
│  │            │ ───► │                │ ───► │            │                │
│  │ [What the  │      │ [What happens  │      │ [What the  │                │
│  │  player    │      │  internally]   │      │  player    │                │
│  │  does]     │      │                │      │  receives] │                │
│  └────────────┘      └────────────────┘      └────────────┘                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Mechanic Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      MECHANIC FLOWCHART                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                         ┌─────────────────┐                                 │
│                         │  Player Action  │                                 │
│                         │  [Trigger]      │                                 │
│                         └────────┬────────┘                                 │
│                                  │                                          │
│                                  ▼                                          │
│                         ┌─────────────────┐                                 │
│                         │  Validation     │                                 │
│                    ┌────│  [Check valid?] │────┐                            │
│                    │    └─────────────────┘    │                            │
│                   YES                         NO                            │
│                    │                           │                            │
│                    ▼                           ▼                            │
│           ┌─────────────────┐         ┌─────────────────┐                   │
│           │  Process        │         │  Error Feedback │                   │
│           │  [Calculate]    │         │  [Show why]     │                   │
│           └────────┬────────┘         └─────────────────┘                   │
│                    │                                                        │
│                    ▼                                                        │
│           ┌─────────────────┐                                               │
│           │  Apply Effects  │                                               │
│           │  [Change state] │                                               │
│           └────────┬────────┘                                               │
│                    │                                                        │
│                    ▼                                                        │
│           ┌─────────────────┐                                               │
│           │  Feedback       │                                               │
│           │  [Show result]  │                                               │
│           └─────────────────┘                                               │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Variables & Parameters

| Variable | Type | Range | Default | Description |
|----------|------|-------|---------|-------------|
| [Variable] | [int/float/bool/enum] | [min-max] | [default] | [What it controls] |

### 4.4 Formulas

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  FORMULA: [Name]                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PURPOSE: [What this formula calculates]                                    │
│                                                                             │
│  FORMULA:                                                                   │
│  ─────────                                                                  │
│  result = (base_value + modifier) * multiplier                              │
│                                                                             │
│  WHERE:                                                                     │
│  ├── base_value: [Source and meaning]                                       │
│  ├── modifier: [Source and meaning]                                         │
│  └── multiplier: [Source and meaning]                                       │
│                                                                             │
│  EXAMPLES:                                                                  │
│  ─────────                                                                  │
│  Input: base=10, modifier=5, multiplier=1.5                                 │
│  Output: (10 + 5) * 1.5 = 22.5                                              │
│                                                                             │
│  BOUNDS:                                                                    │
│  ├── Minimum output: [value]                                                │
│  └── Maximum output: [value or "uncapped"]                                  │
│                                                                             │
│  RATIONALE:                                                                 │
│  [Why this formula was chosen]                                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.5 State Machine

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      STATE DIAGRAM                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────┐                                   ┌─────────────┐          │
│  │   STATE A   │────────[Trigger]──────────────────│   STATE B   │          │
│  │             │                                   │             │          │
│  │  Entry:     │                                   │  Entry:     │          │
│  │  [Action]   │                                   │  [Action]   │          │
│  │             │                                   │             │          │
│  │  While:     │◄──────[Trigger]───────────────────│  While:     │          │
│  │  [Behavior] │                                   │  [Behavior] │          │
│  │             │                                   │             │          │
│  │  Exit:      │                                   │  Exit:      │          │
│  │  [Action]   │                                   │  [Action]   │          │
│  └─────────────┘                                   └──────┬──────┘          │
│                                                          │                  │
│                                                    [Trigger]                │
│                                                          │                  │
│                                                          ▼                  │
│                                                   ┌─────────────┐           │
│                                                   │   STATE C   │           │
│                                                   │             │           │
│                                                   │  [Details]  │           │
│                                                   └─────────────┘           │
│                                                                             │
│  STATE DEFINITIONS:                                                         │
│  ──────────────────                                                         │
│  State A: [What this state represents]                                      │
│  State B: [What this state represents]                                      │
│  State C: [What this state represents]                                      │
│                                                                             │
│  TRANSITION TRIGGERS:                                                       │
│  ────────────────────                                                       │
│  A→B: [What causes this transition]                                         │
│  B→A: [What causes this transition]                                         │
│  B→C: [What causes this transition]                                         │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. System Integration

> ⚠️ REQUIRED

### 5.1 System Dependencies

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    SYSTEM DEPENDENCY MAP                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                    UPSTREAM (This design uses)                              │
│                    ═══════════════════════════                              │
│                               │                                             │
│              ┌────────────────┼────────────────┐                            │
│              │                │                │                            │
│              ▼                ▼                ▼                            │
│       ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                       │
│       │  [System]   │ │  [System]   │ │  [System]   │                       │
│       │             │ │             │ │             │                       │
│       │ Provides:   │ │ Provides:   │ │ Provides:   │                       │
│       │ [What]      │ │ [What]      │ │ [What]      │                       │
│       └─────────────┘ └─────────────┘ └─────────────┘                       │
│                               │                                             │
│                               ▼                                             │
│                    ╔═════════════════════╗                                  │
│                    ║   THIS DESIGN       ║                                  │
│                    ║   [Design Name]     ║                                  │
│                    ╚═════════════════════╝                                  │
│                               │                                             │
│                               ▼                                             │
│                    DOWNSTREAM (Uses this design)                            │
│                    ═══════════════════════════                              │
│                               │                                             │
│              ┌────────────────┼────────────────┐                            │
│              │                │                │                            │
│              ▼                ▼                ▼                            │
│       ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                       │
│       │  [System]   │ │  [System]   │ │  [System]   │                       │
│       │             │ │             │ │             │                       │
│       │ Receives:   │ │ Receives:   │ │ Receives:   │                       │
│       │ [What]      │ │ [What]      │ │ [What]      │                       │
│       └─────────────┘ └─────────────┘ └─────────────┘                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Integration Points

| System | Integration Type | Data Exchanged | Notes |
|--------|------------------|----------------|-------|
| [System] | [Read/Write/Both] | [What data] | [Implementation notes] |

### 5.3 API Surface

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  INTERFACE: [What this design exposes]                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  EVENTS EMITTED:                                                            │
│  ├── [EventName]: Fired when [condition]. Payload: [data]                   │
│  └── [EventName]: Fired when [condition]. Payload: [data]                   │
│                                                                             │
│  EVENTS CONSUMED:                                                           │
│  ├── [EventName]: From [source]. Response: [action]                         │
│  └── [EventName]: From [source]. Response: [action]                         │
│                                                                             │
│  QUERIES SUPPORTED:                                                         │
│  ├── [QueryName]: Returns [data type]                                       │
│  └── [QueryName]: Returns [data type]                                       │
│                                                                             │
│  COMMANDS SUPPORTED:                                                        │
│  ├── [CommandName]: Causes [effect]                                         │
│  └── [CommandName]: Causes [effect]                                         │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Balance Framework

> ⚠️ REQUIRED (for mechanics affecting gameplay)

### 6.1 Balance Goals

| Goal | Target | Tolerance | Measurement |
|------|--------|-----------|-------------|
| [Balance goal] | [Target value] | [+/- range] | [How to measure] |

### 6.2 Balance Levers

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      BALANCE LEVERS                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  These values can be adjusted to tune the mechanic:                         │
│                                                                             │
│  LEVER: [Variable Name]                                                     │
│  ├── Current: [value]                                                       │
│  ├── Range: [min] - [max]                                                   │
│  ├── Impact: [What changing this affects]                                   │
│  └── Sensitivity: [Low/Medium/High] - [How much change causes]              │
│                                                                             │
│  LEVER: [Variable Name]                                                     │
│  ├── Current: [value]                                                       │
│  ├── Range: [min] - [max]                                                   │
│  ├── Impact: [What changing this affects]                                   │
│  └── Sensitivity: [Low/Medium/High]                                         │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  TUNING NOTES:                                                              │
│  [Any specific guidance on balance tuning]                                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6.3 Comparative Balance

| Compared To | Relationship | Notes |
|-------------|--------------|-------|
| [Other mechanic] | [Stronger/Equal/Weaker] | [Why this relationship] |

### 6.4 Economy Impact

> 📎 OPTIONAL - If affects game economy

| Resource | Input/Output | Rate | Notes |
|----------|--------------|------|-------|
| [Resource] | [Consumed/Generated] | [Amount per X] | [Balance implications] |

---

## 7. Edge Cases & Exceptions

> ⚠️ REQUIRED

### 7.1 Edge Case Matrix

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  EDGE CASE: [Case Name]                                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SCENARIO:                                                                  │
│  [Description of the unusual situation]                                     │
│                                                                             │
│  TRIGGER:                                                                   │
│  [What causes this edge case]                                               │
│                                                                             │
│  EXPECTED BEHAVIOR:                                                         │
│  [What should happen]                                                       │
│                                                                             │
│  RATIONALE:                                                                 │
│  [Why this behavior was chosen]                                             │
│                                                                             │
│  PRIORITY: [Must handle / Should handle / Won't handle]                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Exception Handling

| Exception | Cause | Response | Player Communication |
|-----------|-------|----------|---------------------|
| [Exception] | [What causes it] | [System response] | [What player sees] |

### 7.3 Conflict Resolution

| Conflict | Systems Involved | Resolution Rule |
|----------|------------------|-----------------|
| [Conflict scenario] | [System A vs System B] | [How to resolve] |

---

## 8. User Interface

> 📋 RECOMMENDED

### 8.1 UI Requirements

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      UI REQUIREMENTS                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  INFORMATION TO DISPLAY:                                                    │
│  ├── [Information]: [Where/how displayed]                                   │
│  ├── [Information]: [Where/how displayed]                                   │
│  └── [Information]: [Where/how displayed]                                   │
│                                                                             │
│  PLAYER CONTROLS:                                                           │
│  ├── [Action]: [Control method - button, key, gesture]                      │
│  ├── [Action]: [Control method]                                             │
│  └── [Action]: [Control method]                                             │
│                                                                             │
│  FEEDBACK REQUIRED:                                                         │
│  ├── [Feedback type]: [When shown, duration]                                │
│  ├── [Feedback type]: [When shown, duration]                                │
│  └── [Feedback type]: [When shown, duration]                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.2 UI Mockup

> 📎 OPTIONAL - ASCII wireframe or reference to mockup file

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      UI WIREFRAME                                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         [Header Area]                                │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │                                                                     │   │
│  │   ┌─────────┐                                                       │   │
│  │   │ [Icon]  │  [Title]                      [Status Indicator]      │   │
│  │   └─────────┘                                                       │   │
│  │                                                                     │   │
│  │   ┌─────────────────────────────────────────────────────────────┐   │   │
│  │   │                   [Main Content Area]                        │   │   │
│  │   │                                                              │   │   │
│  │   │   [Content elements here]                                    │   │   │
│  │   │                                                              │   │   │
│  │   └─────────────────────────────────────────────────────────────┘   │   │
│  │                                                                     │   │
│  │   [Button A]        [Button B]        [Button C]                   │   │
│  │                                                                     │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  NOTES:                                                                     │
│  [Explanation of UI elements]                                               │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.3 Accessibility Considerations

| Consideration | Requirement | Implementation |
|---------------|-------------|----------------|
| [Accessibility need] | [What's required] | [How to address] |

---

## 9. Progression & Unlocks

> 📎 OPTIONAL - If design involves progression

### 9.1 Progression Structure

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    PROGRESSION PATH                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  LEVEL 1              LEVEL 2              LEVEL 3                          │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────┐                  │
│  │ [Unlock 1]  │ ───► │ [Unlock 2]  │ ───► │ [Unlock 3]  │                  │
│  │             │      │             │      │             │                  │
│  │ Req: [X]    │      │ Req: [X]    │      │ Req: [X]    │                  │
│  └─────────────┘      └─────────────┘      └─────────────┘                  │
│         │                   │                    │                          │
│         └───────────────────┼────────────────────┘                          │
│                             │                                               │
│                             ▼                                               │
│                      ┌─────────────┐                                        │
│                      │ [Capstone]  │                                        │
│                      │             │                                        │
│                      │ Req: ALL    │                                        │
│                      └─────────────┘                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Unlock Requirements

| Unlock | Requirements | Rewards | Notes |
|--------|--------------|---------|-------|
| [Unlock] | [What's needed] | [What's gained] | [Balance notes] |

---

## 10. Testing & Validation

> ⚠️ REQUIRED

### 10.1 Test Scenarios

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  TEST SCENARIO: [Scenario Name]                                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  CATEGORY: [Unit / Integration / Balance / UX / Edge Case]                  │
│  PRIORITY: [Critical / High / Medium / Low]                                 │
│                                                                             │
│  SETUP:                                                                     │
│  [Initial conditions required]                                              │
│                                                                             │
│  STEPS:                                                                     │
│  1. [Action]                                                                │
│  2. [Action]                                                                │
│  3. [Action]                                                                │
│                                                                             │
│  EXPECTED RESULT:                                                           │
│  [What should happen]                                                       │
│                                                                             │
│  PASS CRITERIA:                                                             │
│  [Specific measurable criteria]                                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 10.2 Test Coverage Matrix

| Aspect | Test Types | Coverage Goal |
|--------|------------|---------------|
| Core mechanic | [Unit, Integration] | 100% |
| Edge cases | [Specified, Exploratory] | 90% |
| Balance | [Simulation, Playtest] | Validated |
| UX | [Usability, A/B] | Positive feedback |

### 10.3 Playtest Focus Areas

- [ ] [Specific area to test with players]
- [ ] [Specific area to test with players]
- [ ] [Specific area to test with players]

---

## 11. Implementation Phases

> ⚠️ REQUIRED

### 11.1 Phase Breakdown

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    IMPLEMENTATION PHASES                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PHASE 1: Foundation                                                        │
│  ════════════════════                                                       │
│  │                                                                          │
│  ├── [ ] [Task 1]: [Brief description]                                      │
│  ├── [ ] [Task 2]: [Brief description]                                      │
│  └── [ ] [Task 3]: [Brief description]                                      │
│  │                                                                          │
│  └── DELIVERABLE: [What's usable after this phase]                          │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  PHASE 2: Core Features                                                     │
│  ══════════════════════                                                     │
│  │                                                                          │
│  ├── [ ] [Task 1]: [Brief description]                                      │
│  ├── [ ] [Task 2]: [Brief description]                                      │
│  └── [ ] [Task 3]: [Brief description]                                      │
│  │                                                                          │
│  └── DELIVERABLE: [What's usable after this phase]                          │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  PHASE 3: Polish & Integration                                              │
│  ═════════════════════════════                                              │
│  │                                                                          │
│  ├── [ ] [Task 1]: [Brief description]                                      │
│  ├── [ ] [Task 2]: [Brief description]                                      │
│  └── [ ] [Task 3]: [Brief description]                                      │
│  │                                                                          │
│  └── DELIVERABLE: [Feature complete]                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 11.2 Dependencies & Blockers

| Phase | Blocked By | Blocks |
|-------|------------|--------|
| Phase 1 | [What must be done first] | Phase 2 |
| Phase 2 | Phase 1 | Phase 3 |
| Phase 3 | Phase 2 | [Downstream work] |

### 11.3 MVP Definition

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    MINIMUM VIABLE PRODUCT                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  INCLUDED IN MVP:                                                           │
│  ├── ✓ [Core feature 1]                                                     │
│  ├── ✓ [Core feature 2]                                                     │
│  └── ✓ [Core feature 3]                                                     │
│                                                                             │
│  DEFERRED FROM MVP:                                                         │
│  ├── ○ [Nice-to-have 1] - [When: Phase 2]                                   │
│  ├── ○ [Nice-to-have 2] - [When: Phase 3]                                   │
│  └── ○ [Nice-to-have 3] - [When: Future]                                    │
│                                                                             │
│  MVP SUCCESS CRITERIA:                                                      │
│  [What makes MVP acceptable]                                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 12. Risk Assessment

> 📋 RECOMMENDED

### 12.1 Risk Matrix

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| [Risk description] | Low/Medium/High | Low/Medium/High | [How to prevent/respond] |

### 12.2 Technical Risks

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  RISK: [Risk Name]                                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  DESCRIPTION:                                                               │
│  [What could go wrong]                                                      │
│                                                                             │
│  LIKELIHOOD: [Low / Medium / High]                                          │
│  IMPACT: [Low / Medium / High]                                              │
│  PRIORITY: [Likelihood × Impact assessment]                                 │
│                                                                             │
│  INDICATORS:                                                                │
│  [How we'll know if this is happening]                                      │
│                                                                             │
│  PREVENTION:                                                                │
│  [What we're doing to prevent this]                                         │
│                                                                             │
│  CONTINGENCY:                                                               │
│  [What we'll do if it happens anyway]                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 12.3 Design Risks

| Risk | Concern | Mitigation |
|------|---------|------------|
| [Design risk] | [What might not work as intended] | [How to validate/adjust] |

---

## 13. Open Questions

> ⚠️ REQUIRED

### 13.1 Unresolved Questions

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  QUESTION: Q-001                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  "[The question that needs answering]"                                      │
│                                                                             │
│  CONTEXT:                                                                   │
│  [Why this question matters]                                                │
│                                                                             │
│  OPTIONS CONSIDERED:                                                        │
│  ├── Option A: [Description] - [Pros/Cons]                                  │
│  ├── Option B: [Description] - [Pros/Cons]                                  │
│  └── Option C: [Description] - [Pros/Cons]                                  │
│                                                                             │
│  DECISION NEEDED BY: [Phase or date]                                        │
│  DECISION OWNER: [Who decides]                                              │
│                                                                             │
│  CURRENT LEANING: [Option X] - [Why]                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 13.2 Questions Index

| ID | Question | Status | Blocker? |
|----|----------|--------|----------|
| Q-001 | [Short form] | Open/Resolved | Yes/No |
| Q-002 | [Short form] | Open/Resolved | Yes/No |

### 13.3 Assumptions to Validate

| Assumption | Validation Method | Status |
|------------|-------------------|--------|
| [What we're assuming] | [How to verify] | Unvalidated/Validated |

---

## 14. Design Changelog

> 🔒 LOCKED

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | YYYY-MM-DD | [Author] | Initial design |

---

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                              END OF TEMPLATE                                  ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  SUBMISSION CHECKLIST:                                                        ║
║  ────────────────────────────────────────────────────────────────────────────║
║  □ Design goals clearly stated                                                ║
║  □ Player experience articulated                                              ║
║  □ Core mechanics fully documented                                            ║
║  □ Balance framework defined                                                  ║
║  □ Edge cases considered                                                      ║
║  □ Implementation phases planned                                              ║
║  □ Open questions documented                                                  ║
║  □ All stakeholders identified                                                ║
║  □ Status set to 'review' when ready                                          ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->
