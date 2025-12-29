# Notion-to-Repository Synchronization Audit Strategy

> **Version:** 1.0.0
> **Created:** 2025-12-29
> **Author:** The Architect
> **Purpose:** Detect and remediate scope creep between Notion source of truth and repository implementation

---

## Executive Summary

This document provides a systematic framework for auditing alignment between your **Notion source of truth** and the **Rune & Rust repository**. Scope creep manifests in three primary forms:

1. **Feature Creep**: Implementation contains features not defined in Notion
2. **Specification Drift**: Documented specs deviate from Notion definitions
3. **Content Divergence**: Lore, dialogue, and game data differ from canonical sources

---

## Table of Contents

1. [Audit Dimensions](#audit-dimensions)
2. [Pre-Audit Preparation](#pre-audit-preparation)
3. [Phase 1: Inventory Audit](#phase-1-inventory-audit)
4. [Phase 2: Content Comparison](#phase-2-content-comparison)
5. [Phase 3: Implementation Alignment](#phase-3-implementation-alignment)
6. [Phase 4: Remediation](#phase-4-remediation)
7. [Ongoing Synchronization Protocol](#ongoing-synchronization-protocol)
8. [Audit Templates](#audit-templates)
9. [Tooling Recommendations](#tooling-recommendations)

---

## Audit Dimensions

### Dimension Matrix

| Dimension | Notion Content Type | Repo Content Type | Scope Creep Risk |
|-----------|---------------------|-------------------|------------------|
| **Features** | Feature Backlog, Roadmap | Implementation Plans, Changelogs | HIGH |
| **Specifications** | System Designs, Mechanics | `/docs/specs/*.md` | HIGH |
| **Game Content** | Lore, Bestiary, Items | `/docs/lore/`, `/data/*.json` | MEDIUM |
| **Balance Data** | Tuning Tables | Code constants, seed data | MEDIUM |
| **Terminology** | Glossary, Style Guide | All narrative content | LOW |
| **Architecture** | Tech Stack Decisions | Actual implementation | LOW |

### Scope Creep Indicators

| Indicator | Detection Method | Severity |
|-----------|------------------|----------|
| Repo contains features not in Notion backlog | Feature inventory diff | CRITICAL |
| Spec defines mechanics not in Notion design | Cross-reference scan | HIGH |
| Implementation has undocumented behaviors | Code review vs spec | HIGH |
| Lore content contradicts Notion canon | Content hash comparison | MEDIUM |
| Balance values differ from tuning tables | Constant extraction audit | MEDIUM |
| Terminology inconsistencies | Terminology scan | LOW |

---

## Pre-Audit Preparation

### Step 1: Export Notion Content

**Option A: Manual Export (Recommended for First Audit)**
1. Export each Notion database as CSV/Markdown
2. Organize by dimension (Features, Specs, Lore, Balance)
3. Create a dated snapshot directory: `notion-exports/YYYY-MM-DD/`

**Option B: API Integration (Recommended for Ongoing Sync)**
1. Use Notion API to extract structured data
2. Generate machine-readable JSON for comparison
3. Automate weekly/monthly snapshot generation

### Step 2: Establish Baseline Inventory

Create a master inventory of both sources:

```
# Notion Inventory
- Total Features Planned: [count]
- System Specifications: [count]
- Lore Articles: [count]
- Balance Tables: [count]
- Style Guide Entries: [count]

# Repository Inventory
- Implemented Services: 54
- Specification Documents: 42+
- Lore Documents: ~200+ files in /docs/lore/
- Data Files: 38 JSON files
- Changelogs: 100+ versions
```

---

## Phase 1: Inventory Audit

### 1.1 Feature Inventory Comparison

**Objective**: Identify features in repo that are not in Notion backlog (and vice versa).

**Method**:
1. Extract feature list from Notion roadmap/backlog
2. Extract implemented features from `/docs/changelogs/`
3. Cross-reference to identify:
   - **Implemented but Unplanned**: Scope creep candidates
   - **Planned but Unimplemented**: Expected backlog items
   - **Implemented and Planned**: Aligned features

**Audit Worksheet**:

| Feature | In Notion | In Repo | Status |
|---------|-----------|---------|--------|
| Dice Pool System | ✓ | ✓ | Aligned |
| Combat System | ✓ | ✓ | Aligned |
| Localization | ✓ | ✓ | Aligned |
| [Feature X] | ✗ | ✓ | **SCOPE CREEP** |
| [Feature Y] | ✓ | ✗ | Backlog |

### 1.2 Specification Inventory Comparison

**Objective**: Verify all repo specs have Notion counterparts.

**Method**:
1. List all `/docs/specs/**/*.md` files (currently 42+)
2. Map each to Notion design document
3. Flag orphaned specs (no Notion parent)

**Current Repo Specs to Map**:
```
docs/specs/
├── core/         (2 specs: DICE-001, GAME-001)
├── combat/       (8 specs: ABILITY, AI, ATTACK, COMBAT, ENEMY, ENEMYFAC, STATUS, TRAIT)
├── character/    (9 specs: ADVANCEMENT, CHAR, CORRUPT, LEGEND, RESOURCE, REST, SPECIALIZATION, TRAUMA, XP)
├── exploration/  (6 specs: AMBUSH, DUNGEON, ENVPOP, INTERACT, NAV, SPAWN)
├── environment/  (2 specs: COND, HAZARD)
├── economy/      (4 specs: CRAFT, INV, LOOT, REPAIR)
├── knowledge/    (4 specs: CAPTURE, CODEX, JOURNAL, LIBRARY)
├── content/      (3 specs: DESC, LOC, TEMPLATE)
├── ui/           (9 specs: CRASH, INPUT, OPTIONS, RENDER, SETTINGS, THEME, TRANSITION, UI, VISUAL)
├── data/         (4 specs: MIGRATE, REPO, SAVE, SEED)
└── tools/        (5 specs: AUDIT, CHEAT, DEBUG, DOCGEN, JOURNEY)
```

### 1.3 Content Inventory Comparison

**Objective**: Map all game content files to Notion sources.

**Data Files to Audit**:
```
data/
├── biomes/               (1 biome definition)
├── templates/            (20 room templates)
├── dialogues/            (8 NPC dialogue files)
├── locales/              (1 locale file)
├── input_bindings.json
└── game_state_schema.json
```

**Lore Documents to Audit**:
```
docs/lore/
├── 00-cosmology/         (Creation myths, void, Aether)
├── 01-time/              (Pre-Glitch history, POST-Glitch timeline)
├── 02-magic-science/     (Aether mechanics, Glitch events)
├── 03-technology/        (Domain 4 constraints, tech evolution)
├── 04-factions/          (Major factions, relationships)
├── 05-species/           (Playable races, creatures)
├── 06-entities/          (Gods, spirits, aberrations)
├── 07-reality/           (Physics, metaphysics)
├── 08-geography/         (Regions, landmarks)
├── 09-culture/           (Societies, traditions)
└── 10-language/          (Terminology, dialects)
```

---

## Phase 2: Content Comparison

### 2.1 Structural Comparison

For each matched pair (Notion doc ↔ Repo doc):

**Comparison Checklist**:
- [ ] Section headers match
- [ ] Core concepts align
- [ ] Mechanics/rules are identical
- [ ] Terminology is consistent
- [ ] Examples are equivalent
- [ ] No repo-only additions

### 2.2 Semantic Comparison

**Method**: Side-by-side reading with delta highlighting

**Semantic Drift Categories**:
| Category | Example | Severity |
|----------|---------|----------|
| **Contradiction** | Notion says X, Repo says NOT-X | CRITICAL |
| **Addition** | Repo adds mechanic not in Notion | HIGH |
| **Omission** | Repo missing Notion requirement | HIGH |
| **Elaboration** | Repo expands on Notion concept | MEDIUM |
| **Reordering** | Same content, different structure | LOW |
| **Rewording** | Same meaning, different words | LOW |

### 2.3 Hash-Based Drift Detection

For exact-match content (dialogue, item descriptions):

```bash
# Generate content hashes for comparison
find data/ -name "*.json" -exec md5sum {} \; > repo_content_hashes.txt

# Compare against Notion export hashes
diff notion_content_hashes.txt repo_content_hashes.txt
```

---

## Phase 3: Implementation Alignment

### 3.1 Code vs Specification Audit

**Objective**: Verify implementation matches documented specs.

**Method**:
1. For each spec, identify implementing service(s)
2. Review service code against spec behaviors
3. Flag undocumented behaviors

**Current Coverage** (from SPEC-AUDIT-MATRIX.md):
- Services WITH Specs: 38/38 (100%)
- Service-Interface Alignment: 52/54 (96.3%)
- Test Coverage: 38/54 (70.4%)

### 3.2 Constant Extraction Audit

**Objective**: Extract hardcoded values and compare to Notion balance tables.

**Key Areas**:
```csharp
// Example constants to audit
public const int BaseRepairDC = 8;
public const float RegenerationPercent = 0.10f;
public const int MasterworkThreshold = 5;
```

**Extraction Method**:
```bash
# Find all const declarations
grep -rn "const " RuneAndRust.*/Services/*.cs > constants.txt

# Find all static readonly fields
grep -rn "static readonly" RuneAndRust.*/Services/*.cs >> constants.txt
```

### 3.3 Behavior Mapping

For each service, document:

| Behavior | In Spec | In Code | In Notion | Status |
|----------|---------|---------|-----------|--------|
| Dice pool success threshold | ✓ | ✓ | ? | Verify |
| Critical success on 10s | ✓ | ✓ | ? | Verify |
| Complication on 1s | ✓ | ✓ | ? | Verify |

---

## Phase 4: Remediation

### 4.1 Remediation Decision Tree

```
Scope Creep Detected
        │
        ▼
┌───────────────────────────────┐
│ Is the addition valuable?     │
└───────────────┬───────────────┘
                │
       ┌────────┴────────┐
       │ YES             │ NO
       ▼                 ▼
┌──────────────┐  ┌──────────────┐
│ Backport to  │  │ Remove from  │
│ Notion       │  │ Repository   │
└──────────────┘  └──────────────┘
        │                 │
        ▼                 ▼
┌──────────────┐  ┌──────────────┐
│ Document     │  │ Delete code/ │
│ decision     │  │ content      │
└──────────────┘  └──────────────┘
```

### 4.2 Remediation Actions

| Drift Type | Action | Priority |
|------------|--------|----------|
| Feature in repo, not Notion | Add to Notion backlog OR remove | CRITICAL |
| Spec differs from Notion design | Update spec OR update Notion | HIGH |
| Lore contradicts Notion canon | Correct repo content | HIGH |
| Balance values differ | Sync to Notion tuning tables | MEDIUM |
| Terminology inconsistency | Standardize across both | LOW |

### 4.3 Documentation Requirements

For each remediation:
1. Create audit finding record
2. Document decision rationale
3. Link to relevant Notion page
4. Update CHANGELOG if code changes

---

## Ongoing Synchronization Protocol

### Weekly Sync Checklist

- [ ] Review new Notion additions
- [ ] Check for repo changes not reflected in Notion
- [ ] Update cross-reference mappings
- [ ] Run terminology consistency check

### Monthly Audit Cycle

| Week | Activity |
|------|----------|
| 1 | Feature inventory comparison |
| 2 | Specification alignment review |
| 3 | Content drift scan |
| 4 | Remediation and documentation |

### Change Management Process

```
1. Change originates in Notion (Source of Truth)
        │
        ▼
2. Create implementation task with Notion link
        │
        ▼
3. Implement in repository
        │
        ▼
4. Update local spec with Notion reference
        │
        ▼
5. Mark Notion item as "Implemented" with commit hash
        │
        ▼
6. Record in changelog
```

### Bidirectional Sync Rules

| Direction | When Allowed | Approval |
|-----------|--------------|----------|
| Notion → Repo | Always (primary flow) | None required |
| Repo → Notion | Emergent improvements only | Design review |
| Repo divergence | Never permanent | Must remediate |

---

## Audit Templates

### Template A: Feature Alignment Audit

```markdown
# Feature Alignment Audit - [DATE]

## Audit Scope
- Notion Database: [Name/URL]
- Repository Version: [version]
- Auditor: [name]

## Feature Inventory

### Aligned Features
| Feature | Notion ID | Repo Location | Status |
|---------|-----------|---------------|--------|
| ... | ... | ... | ✓ |

### Scope Creep (Repo Only)
| Feature | Repo Location | Recommendation |
|---------|---------------|----------------|
| ... | ... | Backport / Remove |

### Backlog (Notion Only)
| Feature | Notion ID | Priority |
|---------|-----------|----------|
| ... | ... | P1/P2/P3 |

## Remediation Plan
1. ...
2. ...

## Sign-off
- Auditor: [signature]
- Date: [date]
```

### Template B: Specification Drift Audit

```markdown
# Specification Drift Audit - [SPEC-ID]

## Documents Compared
- Notion: [URL/Name]
- Repository: [file path]

## Alignment Summary
| Section | Status | Notes |
|---------|--------|-------|
| Overview | ✓ | Aligned |
| Mechanics | ⚠️ | Minor drift |
| Behaviors | ✗ | Major divergence |

## Drift Details

### Section: [Name]
- **Notion States**: "..."
- **Repo States**: "..."
- **Drift Type**: Addition / Contradiction / Omission
- **Severity**: Critical / High / Medium / Low
- **Recommendation**: ...

## Remediation Checklist
- [ ] Update repo spec
- [ ] Update code if needed
- [ ] Update Notion if needed
- [ ] Document decision

## Audit Metadata
- Auditor: [name]
- Date: [date]
- Notion Last Modified: [date]
- Repo Last Modified: [date]
```

### Template C: Content Hash Comparison

```markdown
# Content Hash Audit - [DATE]

## Files Audited
| File | Notion Hash | Repo Hash | Match |
|------|-------------|-----------|-------|
| the_roots.json | abc123 | abc123 | ✓ |
| npc_elara.json | def456 | def789 | ✗ |

## Mismatches Requiring Review
| File | Drift Summary | Recommendation |
|------|---------------|----------------|
| npc_elara.json | 3 dialogue nodes differ | Sync from Notion |

## Actions Taken
1. ...
```

---

## Tooling Recommendations

### Recommended Tools

| Tool | Purpose | Integration |
|------|---------|-------------|
| **Notion API** | Extract structured data | Weekly export script |
| **diff/meld** | Side-by-side comparison | Manual audit |
| **jq** | JSON content extraction | Hash comparison |
| **grep/ripgrep** | Constant extraction | Code audit |
| **Git blame** | Change attribution | Drift investigation |

### Automation Opportunities

1. **Notion Export Script**: Nightly export of key databases to repo
2. **Content Hash Generator**: Auto-generate hashes for comparison
3. **Constant Extractor**: Pull all game constants into auditable format
4. **Drift Alert**: Notify when repo changes without Notion update

### Suggested Workflow Integration

```bash
# Pre-commit hook concept
#!/bin/bash
# Warn if changing spec without Notion reference
if git diff --cached --name-only | grep -q "docs/specs/"; then
    echo "⚠️  Spec modified - ensure Notion source is updated"
fi
```

---

## Quick Start Checklist

### First Audit (Manual)

1. [ ] Export Notion feature backlog to CSV
2. [ ] Export Notion specifications to Markdown
3. [ ] Run feature inventory comparison
4. [ ] Document all scope creep findings
5. [ ] Create remediation plan
6. [ ] Execute remediations
7. [ ] Establish ongoing sync cadence

### Ongoing Maintenance

1. [ ] Weekly: Check for Notion updates
2. [ ] Before commits: Reference Notion source
3. [ ] Monthly: Run full audit cycle
4. [ ] Quarterly: Review and refine process

---

## Appendix: Repository Audit Assets

### Existing Audit Infrastructure

| Document | Purpose | Location |
|----------|---------|----------|
| SPEC-AUDIT-MATRIX.md | Spec coverage tracking | `/docs/SPEC-AUDIT-MATRIX.md` |
| SPEC-AUDIT-REVIEW.md | Peer review validation | `/docs/SPEC-AUDIT-REVIEW.md` |
| SPEC-AUDIT-001.md | Monte Carlo audit framework | `/docs/specs/tools/SPEC-AUDIT-001.md` |

### Content Locations to Audit

| Content Type | Location | File Count |
|--------------|----------|------------|
| Specifications | `/docs/specs/**/*.md` | 42+ |
| Changelogs | `/docs/changelogs/` | 100+ |
| Implementation Plans | `/docs/plans/` | 100+ |
| Lore Documents | `/docs/lore/` | 200+ |
| Game Data (JSON) | `/data/` | 38 |
| Service Implementations | `/RuneAndRust.Engine/Services/` | 54 |

---

*Document generated by The Architect - Notion Synchronization Audit Framework*
*Last Updated: 2025-12-29*
