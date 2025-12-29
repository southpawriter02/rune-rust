---
id: GUIDE-DOC-METADATA-001
title: "Documentation Metadata Templates"
version: 1.0.0
status: Draft
last_updated: 2025-12-29
---

# Documentation Metadata Templates

> **Purpose:** Standardized YAML frontmatter schemas for all Rune & Rust documentation.
> **Audience:** Content authors, technical writers, AI agents, and tooling developers.

---

## Table of Contents

- [1. Core Metadata Schema](#1-core-metadata-schema)
- [2. Document Type Taxonomy](#2-document-type-taxonomy)
- [3. Field Standardization Rules](#3-field-standardization-rules)
- [4. Type-Specific Templates](#4-type-specific-templates)
  - [4.1 Specification Template](#41-specification-template)
  - [4.2 Changelog Template](#42-changelog-template)
  - [4.3 Plan/Roadmap Template](#43-planroadmap-template)
  - [4.4 Lore Document Template](#44-lore-document-template)
  - [4.5 Design Document Template](#45-design-document-template)
  - [4.6 Validation Framework Template](#46-validation-framework-template)
  - [4.7 Guide Template](#47-guide-template)
- [5. ID Naming Conventions](#5-id-naming-conventions)
- [6. Migration Guidelines](#6-migration-guidelines)

---

## 1. Core Metadata Schema

All documents share a **Core Schema** with required and optional fields. Type-specific fields extend this base.

### Required Fields (All Documents)

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `id` | string | Unique document identifier following naming conventions | `SPEC-COMBAT-001` |
| `title` | string | Human-readable document title (quoted) | `"Combat Resolution System"` |
| `version` | string | Semantic version (X.Y.Z format) | `1.0.0` |
| `status` | enum | Current document lifecycle state | `Draft` |
| `last_updated` | date | Last modification date (YYYY-MM-DD) | `2025-12-29` |

### Optional Fields (All Documents)

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `doc_type` | enum | Document category (see taxonomy) | `specification` |
| `author` | string | Primary author or team | `The Architect` |
| `related_docs` | list | Cross-references to related documents | `[SPEC-DICE-001, SPEC-STATUS-001]` |
| `keywords` | list | Search/filter terms | `[combat, damage, initiative]` |
| `supersedes` | string | ID of document this replaces | `SPEC-COMBAT-000` |
| `superseded_by` | string | ID of document replacing this | `SPEC-COMBAT-002` |

### Status Vocabulary

Use these exact values (case-sensitive):

| Status | Description |
|--------|-------------|
| `Draft` | Initial creation, incomplete or unreviewed |
| `In Review` | Under peer review or validation |
| `Approved` | Reviewed and approved for use |
| `Published` | Released and canonical |
| `Deprecated` | Superseded but retained for reference |
| `Archived` | Historical reference only |

---

## 2. Document Type Taxonomy

Standardized `doc_type` values:

| doc_type | Description | ID Prefix |
|----------|-------------|-----------|
| `specification` | Technical system specifications | `SPEC-` |
| `changelog` | Version release notes | `CLOG-` |
| `plan` | Implementation roadmaps | `PLAN-` |
| `lore` | World content and narratives | `AAM-`, `CDS-`, `CDF-` |
| `design` | Game design documents | `DSGN-` |
| `validation` | Domain validation frameworks | `DOM-` |
| `guide` | Author/developer guides | `GUIDE-` |
| `workflow` | Process documentation | `WKFL-` |

---

## 3. Field Standardization Rules

### Naming Conventions

| Standard | Use | Avoid |
|----------|-----|-------|
| `last_updated` | ✅ Preferred | ❌ `last-updated` |
| `related_docs` | ✅ Preferred | ❌ `related-files`, `related_specs` |
| `doc_type` | ✅ Preferred | ❌ `document-type`, `type` |

### Version Format

Always use semantic versioning: `MAJOR.MINOR.PATCH`

| Context | Format | Example |
|---------|--------|---------|
| Specifications | `X.Y.Z` | `2.1.0` |
| Lore documents | `X.Y.Z` | `5.0.0` |
| Changelogs | `X.Y.Z[suffix]` | `0.4.0b` |

**Note:** Avoid `v` prefix in frontmatter (`1.0.0` not `v1.0.0`). Titles may use the prefix for readability.

### Date Format

Always use ISO 8601: `YYYY-MM-DD`

```yaml
last_updated: 2025-12-29
```

### String Quoting

- Quote titles containing special characters: `title: "Combat: Phase 2"`
- Plain values for simple strings: `status: Draft`

---

## 4. Type-Specific Templates

### 4.1 Specification Template

For technical system specifications (e.g., `SPEC-COMBAT-001.md`).

```yaml
---
id: SPEC-[DOMAIN]-[NNN]
title: "[Descriptive Title]"
doc_type: specification
version: 1.0.0
status: Draft
last_updated: YYYY-MM-DD
author: "[Author Name]"
related_docs: []
keywords: []
# Specification-specific fields
service: "[PrimaryService], [SecondaryService]"
location: "RuneAndRust.[Layer]/[Path]/[Service].cs"
domain: "[Domain Name]"
---
```

**Example:**

```yaml
---
id: SPEC-COMBAT-015
title: "Stance-Based Combat Resolution"
doc_type: specification
version: 2.1.0
status: Approved
last_updated: 2025-12-28
author: "The Forge-Master"
related_docs: [SPEC-DICE-001, SPEC-STATUS-003]
keywords: [combat, stances, damage, initiative]
service: "ICombatService, IStanceService"
location: "RuneAndRust.Engine/Services/CombatService.cs"
domain: "Combat"
---
```

---

### 4.2 Changelog Template

For version release notes (e.g., `v0.4.0b.md`).

```yaml
---
id: CLOG-[VERSION]
title: "Changelog: v[VERSION] - [Codename]"
doc_type: changelog
version: [VERSION]
status: Published
last_updated: YYYY-MM-DD
release_date: YYYY-MM-DD
# Changelog-specific fields
milestone: "[Milestone Name]"
summary: "[One-line summary]"
breaking_changes: true | false
files_added: [count]
files_modified: [count]
test_count: [count]
---
```

**Example:**

```yaml
---
id: CLOG-0.4.0b
title: "Changelog: v0.4.0b - The Growth (Attribute Upgrades)"
doc_type: changelog
version: 0.4.0b
status: Published
last_updated: 2025-12-27
release_date: 2025-12-27
milestone: "v0.4.x - The Saga & The Weaver"
summary: "Implements Attribute Upgrade System with PP spending"
breaking_changes: false
files_added: 4
files_modified: 1
test_count: 19
---
```

---

### 4.3 Plan/Roadmap Template

For implementation plans (e.g., `v0.4.4e.md`).

```yaml
---
id: PLAN-[VERSION]
title: "[Codename]: [Description]"
doc_type: plan
version: [VERSION]
status: Planned | In Progress | Completed
last_updated: YYYY-MM-DD
# Plan-specific fields
milestone: "[Parent Milestone]"
theme: "[Thematic Description]"
dependencies: []
target_date: YYYY-MM-DD | null
phases: [count]
---
```

**Example:**

```yaml
---
id: PLAN-0.4.4e
title: "The Grimoire: Mystic Spell Tree & TUI"
doc_type: plan
version: 0.4.4e
status: Planned
last_updated: 2025-12-29
milestone: "v0.4.x - The Saga & The Weaver"
theme: "The meticulous study of the Old Ones' language"
dependencies: [PLAN-0.4.4a, PLAN-0.4.4c, PLAN-0.4.1]
target_date: null
phases: 3
---
```

---

### 4.4 Lore Document Template

For world content and narratives. Includes Layer 2 (Diagnostic) fields for voice compliance.

```yaml
---
id: [CLASSIFICATION-CODE]
title: "[Lore Title]"
doc_type: lore
version: 1.0.0
status: Canonical | Draft | Archived
last_updated: YYYY-MM-DD
# Lore-specific fields
classification: "[ARCHIVE] // [CATEGORY] // [TYPE]"
layer: Layer 1 (Mythic) | Layer 2 (Diagnostic) | Layer 3 (Technical) | Layer 4 (Ground Truth)
domain: "[Content Domain]"
author: "[In-Universe Author]"
author_faction: "[Faction or Office]"
author_voice: "[Voice Register]"
faction_association: "[Associated Faction]"
geographic_scope: "[Locations]"
keywords: []
---
```

**Example:**

```yaml
---
id: CDS-POP-VARGR-002
title: "Population Dossier: Rune-Lupins (The Vargr-Syn)"
doc_type: lore
version: 5.0.0
status: Canonical
last_updated: 2025-12-27
classification: "AETHELGARD-ARCHIVES-MASTER // POPULATION DOSSIER // BIO-DIGITAL"
layer: Layer 2 (Diagnostic)
domain: "Cybernetics & Fauna"
author: "Magister Kaelen"
author_faction: "Scriptorium-Primus, Division of Crypto-Zoology"
author_voice: "Scholar — Archivist-Pathologist"
faction_association: "Independent / Skaldic Pact-Bound"
geographic_scope: "Midgard (Forests), Alfheim (Data-Groves)"
keywords: [fauna, rune-lupin, vargr-syn, cybernetics, mesh-mind]
---
```

---

### 4.5 Design Document Template

For game design documents (e.g., specialization specs).

```yaml
---
id: DSGN-[CATEGORY]-[NNN]
title: "[Design Document Title]"
doc_type: design
version: 1.0.0
status: Draft | Approved | Implemented
last_updated: YYYY-MM-DD
# Design-specific fields
category: "[Category Name]"
parent: "[Path to parent document]"
related_docs: []
implementation_status: Planned | In Progress | Implemented
---
```

**Example:**

```yaml
---
id: DSGN-SPEC-BERSERKER-001
title: "Berserker Specialization Complete Specification"
doc_type: design
version: 3.2.0
status: Approved
last_updated: 2025-12-20
category: "Player Specializations"
parent: "docs/design/02-entities/players/specializations/"
related_docs: [SPEC-COMBAT-001, SPEC-ABILITY-005]
implementation_status: In Progress
---
```

---

### 4.6 Validation Framework Template

For domain validation checks (e.g., Domain 4 Technology Constraints).

```yaml
---
id: DOM-[N]
title: "Domain [N]: [Domain Name] Validation Check"
doc_type: validation
version: 1.0.0
status: Published
last_updated: YYYY-MM-DD
# Validation-specific fields
severity: P1-CRITICAL | P2-HIGH | P3-MEDIUM | P4-LOW
applies_to: []
check_count: [count]
---
```

**Example:**

```yaml
---
id: DOM-4
title: "Domain 4: Technology Constraints Validation Check"
doc_type: validation
version: 2.0.0
status: Published
last_updated: 2025-12-25
severity: P1-CRITICAL
applies_to: [lore, bestiary, item descriptions, NPC dialogue]
check_count: 15
---
```

---

### 4.7 Guide Template

For author/developer guides.

```yaml
---
id: GUIDE-[TOPIC]-[NNN]
title: "[Guide Title]"
doc_type: guide
version: 1.0.0
status: Published
last_updated: YYYY-MM-DD
# Guide-specific fields
audience: []
prerequisites: []
related_docs: []
---
```

**Example:**

```yaml
---
id: GUIDE-DOC-METADATA-001
title: "Documentation Metadata Templates"
doc_type: guide
version: 1.0.0
status: Draft
last_updated: 2025-12-29
audience: [content authors, technical writers, AI agents]
prerequisites: []
related_docs: [SPEC-WORKFLOW-001]
---
```

---

## 5. ID Naming Conventions

### General Pattern

```
[PREFIX]-[CATEGORY]-[DESCRIPTOR]-[NUMBER]
```

### Prefix Reference

| Document Type | Prefix | Pattern | Example |
|---------------|--------|---------|---------|
| Specification | `SPEC-` | `SPEC-[DOMAIN]-[NNN]` | `SPEC-COMBAT-015` |
| Changelog | `CLOG-` | `CLOG-[VERSION]` | `CLOG-0.4.0b` |
| Plan | `PLAN-` | `PLAN-[VERSION]` | `PLAN-0.4.4e` |
| Lore (Archives) | `AAM-` | `AAM-[CAT]-[DESC]-v[N.N]` | `AAM-HIST-AGEOFCREEDS-v1.1` |
| Lore (Dossier) | `CDS-` | `CDS-[CAT]-[ENTITY]-[NNN]` | `CDS-POP-VARGR-002` |
| Lore (Faction) | `CDF-` | `CDF-[FACTION]-[NNN]` | `CDF-IRONBANE-001` |
| Design | `DSGN-` | `DSGN-[CAT]-[NAME]-[NNN]` | `DSGN-SPEC-BERSERKER-001` |
| Validation | `DOM-` | `DOM-[N]` | `DOM-4` |
| Guide | `GUIDE-` | `GUIDE-[TOPIC]-[NNN]` | `GUIDE-DOC-METADATA-001` |
| Workflow | `WKFL-` | `WKFL-[NAME]-[NNN]` | `WKFL-DEV-001` |

### Domain Abbreviations (Specifications)

| Domain | Abbreviations |
|--------|---------------|
| Core | `CORE`, `DICE`, `GAME` |
| Combat | `COMBAT`, `ABILITY`, `ATTACK`, `STATUS`, `AI`, `ENEMY` |
| Character | `CHAR`, `ADVANCEMENT`, `XP`, `LEGEND`, `TRAUMA`, `CORRUPT` |
| Exploration | `NAV`, `DUNGEON`, `ENVPOP`, `SPAWN`, `INTERACT` |
| Environment | `HAZARD`, `COND` |
| Economy | `INV`, `CRAFT`, `REPAIR`, `LOOT` |
| Knowledge | `CODEX`, `CAPTURE`, `JOURNAL` |
| Content | `DESC`, `TEMPLATE` |
| UI | `UI`, `RENDER`, `INPUT`, `THEME` |
| Data | `SAVE`, `REPO`, `SEED`, `MIGRATE` |

---

## 6. Migration Guidelines

### Updating Existing Documents

1. **Add frontmatter** to documents lacking it (changelogs, plans, validations)
2. **Normalize field names** (`last-updated` → `last_updated`)
3. **Standardize version format** (remove `v` prefix)
4. **Apply status vocabulary** (capitalize first letter only)
5. **Generate ID** using naming conventions

### Priority Order

1. **High Priority:** Specifications, Validations (directly reference in tooling)
2. **Medium Priority:** Changelogs, Plans (frequently queried)
3. **Low Priority:** Lore documents (already have good metadata)

### Validation Script Pattern

```bash
# Example: Check for missing required fields
grep -L "^id:" docs/**/*.md
grep -L "^status:" docs/**/*.md
grep -L "^last_updated:" docs/**/*.md
```

---

## Quick Reference Card

### Minimal Frontmatter (All Documents)

```yaml
---
id: [UNIQUE-ID]
title: "[Title]"
version: 1.0.0
status: Draft
last_updated: YYYY-MM-DD
---
```

### Full Frontmatter (Lore/Spec)

```yaml
---
id: [UNIQUE-ID]
title: "[Title]"
doc_type: [type]
version: 1.0.0
status: Draft
last_updated: YYYY-MM-DD
author: "[Author]"
related_docs: []
keywords: []
# Type-specific fields...
---
```

---

*Template Version: 1.0.0 | Last Updated: 2025-12-29*
