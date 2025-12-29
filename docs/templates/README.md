# Rune & Rust Documentation Templates

> **Version:** 1.0.0
> **Last Updated:** 2025-12-29
> **Maintainer:** The Architect

## Overview

This directory contains the **Gold Standard Templates** for all documentation in the Rune & Rust project. These templates enforce consistency, completeness, and Domain 4 compliance across all canonical content.

---

## Template Index

| Template | File | Purpose | Primary Users |
|----------|------|---------|---------------|
| **Specification** | `SPEC-TEMPLATE.md` | System/feature technical specifications | Developers, Architects |
| **Lore** | `LORE-TEMPLATE.md` | Narrative world-building content | Writers, Archivists |
| **Bestiary** | `BESTIARY-TEMPLATE.md` | Creature/enemy documentation | Writers, Game Designers |
| **Gazette** | `GAZETTE-TEMPLATE.md` | Location/region documentation | Writers, Level Designers |
| **Design** | `DESIGN-TEMPLATE.md` | Mechanics and systems design | Game Designers |
| **Changelog** | `CHANGELOG-TEMPLATE.md` | Release documentation | Developers, QA |
| **Validation** | `VALIDATION-TEMPLATE.md` | Domain compliance testing | QA, Validators |
| **Implementation Plan** | `PLAN-TEMPLATE.md` | Version implementation blueprints | Project Managers, Leads |

---

## Quick Reference: Document Type Selection

```
                    ┌─────────────────────────────────────┐
                    │     What are you documenting?       │
                    └─────────────────┬───────────────────┘
                                      │
          ┌───────────────────────────┼───────────────────────────┐
          │                           │                           │
          ▼                           ▼                           ▼
    ┌───────────┐             ┌───────────┐             ┌───────────┐
    │  SYSTEM   │             │  CONTENT  │             │  PROCESS  │
    │ (How it   │             │ (What     │             │ (When/Why │
    │  works)   │             │  exists)  │             │  changes) │
    └─────┬─────┘             └─────┬─────┘             └─────┬─────┘
          │                         │                         │
    ┌─────┴─────┐           ┌───────┴───────┐           ┌─────┴─────┐
    │           │           │       │       │           │           │
    ▼           ▼           ▼       ▼       ▼           ▼           ▼
┌────────┐ ┌────────┐  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────────┐ ┌──────────┐
│  SPEC  │ │ DESIGN │  │ LORE │ │BEAST │ │GAZET │ │ PLAN     │ │CHANGELOG │
│Template│ │Template│  │      │ │      │ │      │ │ Template │ │ Template │
└────────┘ └────────┘  └──────┘ └──────┘ └──────┘ └──────────┘ └──────────┘
                                  │
                                  ▼
                       ┌─────────────────────┐
                       │    VALIDATION       │
                       │    Template         │
                       │ (Quality Assurance) │
                       └─────────────────────┘
```

---

## Frontmatter Schema Standards

All documents MUST include YAML frontmatter. Field names use **hyphens** (not underscores).

### Universal Required Fields

```yaml
---
id: [TYPE]-[DOMAIN]-[NNN]     # Unique identifier
title: "[Human-Readable Title]"
version: [X.Y.Z]               # Semantic versioning
status: [Standard Status]      # See status enum below
last-updated: YYYY-MM-DD       # ISO 8601 date format
---
```

### Standard Status Enum

| Status | Meaning | Color Code |
|--------|---------|------------|
| `draft` | Initial creation, incomplete | Gray |
| `review` | Awaiting peer review | Yellow |
| `approved` | Reviewed and accepted | Green |
| `implemented` | Code/content deployed | Blue |
| `deprecated` | No longer valid/active | Orange |
| `archived` | Historical reference only | Red |
| `canonical` | Official lore/content | Purple |

---

## ID Naming Conventions

### Specification IDs
```
SPEC-[DOMAIN]-[NNN]

Domains:
├── CORE     - Core game systems
├── COMBAT   - Combat mechanics
├── CHAR     - Character systems
├── NAV      - Navigation/exploration
├── INV      - Inventory/economy
├── UI       - User interface
├── DATA     - Data structures
├── ENV      - Environment systems
├── STATUS   - Status effects
├── ABILITY  - Ability systems
└── ENEMY    - Enemy AI/behavior
```

### Lore IDs
```
LORE-[CATEGORY]-[NNN]

Categories:
├── ENT      - Entities (NPCs, groups)
├── FAC      - Factions
├── FAU      - Fauna (creatures)
├── FLO      - Flora (plants)
├── GEO      - Geography
├── HAZ      - Hazards
├── HIS      - History
├── ALC      - Alchemy
└── LNG      - Linguistics
```

### Bestiary IDs
```
BEAST-[THREAT]-[NNN]

Threat Levels:
├── MINOR    - Threat Level 1-2
├── MODERATE - Threat Level 3-4
├── SEVERE   - Threat Level 5-6
├── DEADLY   - Threat Level 7-8
└── APEX     - Threat Level 9-10
```

### Gazette IDs
```
GAZ-[REGION]-[NNN]

Regions:
├── NORTH    - Northern territories
├── SOUTH    - Southern territories
├── EAST     - Eastern territories
├── WEST     - Western territories
├── UNDER    - Underground/caverns
├── COASTAL  - Coastal areas
├── URBAN    - Settlements
└── WILD     - Wilderness
```

### Plan IDs
```
v[MAJOR].[MINOR].[PATCH][LETTER]

Examples:
├── v0.4.4e  - Feature within v0.4.4
├── v0.5.0a  - First feature of v0.5.0
└── v1.0.0   - Major release plan
```

---

## Voice & Layer Classification

### Content Layers

| Layer | Name | Voice | Precision Allowed |
|-------|------|-------|-------------------|
| **L1** | Mythic | Oral tradition, sagas | None - purely qualitative |
| **L2** | Diagnostic | Field observer, clinical | Relative only ("a spear's throw") |
| **L3** | Technical | Pre-Glitch archives | Full precision (Hz, meters, %) |
| **L4** | Ground Truth | Designer's reality | Full precision + rationale |

### Domain 4 Quick Reference

```
┌─────────────────────────────────────────────────────────────────┐
│                    DOMAIN 4: TECHNOLOGY                         │
│           "Archaeologists, not Engineers"                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  FORBIDDEN in L1/L2:              ALLOWED in L1/L2:             │
│  ├── 95%                          ├── "Almost certain"          │
│  ├── 4.2 meters                   ├── "A spear's throw"         │
│  ├── 35°C                         ├── "Oppressively hot"         │
│  ├── 18 seconds                   ├── "Several heartbeats"      │
│  ├── 200 Hz                       ├── "A low rumble"            │
│  ├── API, Bug, Glitch             ├── "Anomaly, Phenomenon"     │
│  └── 12.5 kg                      └── "Heavy as a small child"  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Workflow: Creating New Documentation

```
┌────────────────────────────────────────────────────────────────────────┐
│                        DOCUMENTATION WORKFLOW                          │
└────────────────────────────────────────────────────────────────────────┘

Step 1: SELECT TEMPLATE
        │
        ├── Is it a system/feature? ──────────────► SPEC-TEMPLATE
        ├── Is it a creature/enemy? ──────────────► BESTIARY-TEMPLATE
        ├── Is it a location/region? ─────────────► GAZETTE-TEMPLATE
        ├── Is it narrative content? ─────────────► LORE-TEMPLATE
        ├── Is it a mechanics design? ────────────► DESIGN-TEMPLATE
        ├── Is it a release record? ──────────────► CHANGELOG-TEMPLATE
        ├── Is it a domain check? ────────────────► VALIDATION-TEMPLATE
        └── Is it a version plan? ────────────────► PLAN-TEMPLATE
                │
                ▼
Step 2: COPY TEMPLATE
        │
        ├── Copy template to appropriate directory
        ├── Rename file following ID conventions
        └── Update frontmatter with unique ID
                │
                ▼
Step 3: COMPLETE SECTIONS
        │
        ├── Fill all REQUIRED sections (marked with ⚠️)
        ├── Fill RECOMMENDED sections where applicable
        ├── Remove OPTIONAL sections if not needed
        └── Replace all [PLACEHOLDER] text
                │
                ▼
Step 4: VALIDATE
        │
        ├── Run Domain 4 compliance check (if L1/L2 content)
        ├── Verify all cross-references resolve
        ├── Check frontmatter completeness
        └── Review against template checklist
                │
                ▼
Step 5: REVIEW & APPROVE
        │
        ├── Set status to 'review'
        ├── Obtain peer review
        ├── Address feedback
        └── Update status to 'approved' or 'canonical'
```

---

## Section Marking Convention

Templates use the following markers to indicate section requirements:

| Marker | Meaning |
|--------|---------|
| `⚠️ REQUIRED` | Section MUST be completed |
| `📋 RECOMMENDED` | Section SHOULD be completed |
| `📎 OPTIONAL` | Section MAY be included |
| `🔒 LOCKED` | Section content is fixed/standard |
| `⚡ AUTO-GENERATED` | Section populated by tooling |

---

## Cross-Reference Standards

### Internal Links
```markdown
<!-- Same directory -->
[Related Document](./RELATED-DOC.md)

<!-- Parent directory -->
[Parent Document](../PARENT-DOC.md)

<!-- Absolute from docs root -->
[Specification](/docs/specs/combat/SPEC-COMBAT-001.md)
```

### Reference Tables
```markdown
| Dependency | Type | Link |
|------------|------|------|
| SPEC-COMBAT-001 | Requires | [Link](../specs/combat/SPEC-COMBAT-001.md) |
| LORE-ENT-015 | References | [Link](../lore/entities/LORE-ENT-015.md) |
```

---

## Validation Checklist (All Documents)

Before submitting any document, verify:

- [ ] Frontmatter is complete and valid YAML
- [ ] ID follows naming convention for document type
- [ ] Status is set appropriately
- [ ] All REQUIRED sections are complete
- [ ] All [PLACEHOLDER] text has been replaced
- [ ] Cross-references link to existing documents
- [ ] Domain 4 compliance verified (for L1/L2 content)
- [ ] Spelling and grammar checked
- [ ] Tables are properly formatted
- [ ] Code blocks have language specifiers
- [ ] Decision trees/diagrams render correctly

---

## Directory Structure

```
docs/
├── templates/                    # THIS DIRECTORY
│   ├── README.md                 # This file
│   ├── SPEC-TEMPLATE.md          # Specification template
│   ├── LORE-TEMPLATE.md          # Lore template
│   ├── BESTIARY-TEMPLATE.md      # Bestiary template
│   ├── GAZETTE-TEMPLATE.md       # Gazette template
│   ├── DESIGN-TEMPLATE.md        # Design template
│   ├── CHANGELOG-TEMPLATE.md     # Changelog template
│   ├── VALIDATION-TEMPLATE.md    # Validation template
│   └── PLAN-TEMPLATE.md          # Implementation plan template
│
├── specs/                        # ← Use SPEC-TEMPLATE
├── lore/                         # ← Use LORE-TEMPLATE
│   ├── fauna/                    # ← Use BESTIARY-TEMPLATE
│   └── geography/                # ← Use GAZETTE-TEMPLATE
├── design/                       # ← Use DESIGN-TEMPLATE
├── changelogs/                   # ← Use CHANGELOG-TEMPLATE
├── validations/                  # ← Use VALIDATION-TEMPLATE
└── plans/                        # ← Use PLAN-TEMPLATE
```

---

## Getting Help

- **Template Issues:** File in `docs/templates/issues/`
- **Domain 4 Questions:** Consult `docs/validations/domain-04-technology.md`
- **Voice Guidelines:** See `docs/guides/capture-template-authoring.md`
- **Agent Rules:** Check `docs/agent_rules/tech-writer-agent.md`

---

*"Consistency is the foundation of quality. Quality is the foundation of trust."*
— The Architect
