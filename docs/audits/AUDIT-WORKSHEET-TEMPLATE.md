# Scope Creep Audit Worksheet

> **Audit Date:** [YYYY-MM-DD]
> **Auditor:** [Name]
> **Scope:** [Full / Partial - specify domains]
> **Notion Export Date:** [YYYY-MM-DD]

---

## Pre-Audit Checklist

- [ ] Notion content exported to local/shared folder
- [ ] Repository version noted: `v______`
- [ ] Audit dimensions identified

---

## Section 1: Feature Inventory Comparison

### 1.1 Notion Feature List (from Backlog/Roadmap)

| # | Notion Feature | Notion ID/URL | Priority |
|---|----------------|---------------|----------|
| 1 | | | |
| 2 | | | |
| 3 | | | |
| 4 | | | |
| 5 | | | |

### 1.2 Repository Implemented Features (from Changelogs)

| # | Repo Feature | Version Introduced | Spec Reference |
|---|--------------|-------------------|----------------|
| 1 | | | |
| 2 | | | |
| 3 | | | |
| 4 | | | |
| 5 | | | |

### 1.3 Alignment Analysis

| Feature | In Notion | In Repo | Status | Action Required |
|---------|:---------:|:-------:|--------|-----------------|
| | ✓ | ✓ | ALIGNED | None |
| | ✗ | ✓ | **SCOPE CREEP** | Backport or Remove |
| | ✓ | ✗ | BACKLOG | Expected |
| | ✗ | ✗ | N/A | - |

**Scope Creep Count:** ___
**Alignment Rate:** ___%

---

## Section 2: Specification Drift Analysis

### 2.1 Specification Mapping

| Repo Spec | Notion Design Doc | Last Sync Date | Status |
|-----------|-------------------|----------------|--------|
| SPEC-DICE-001 | | | |
| SPEC-COMBAT-001 | | | |
| SPEC-CHAR-001 | | | |
| SPEC-INV-001 | | | |
| SPEC-ABILITY-001 | | | |
| | | | |
| | | | |

### 2.2 Drift Findings

| Spec ID | Drift Type | Severity | Description | Recommendation |
|---------|------------|----------|-------------|----------------|
| | Addition | | | |
| | Contradiction | | | |
| | Omission | | | |
| | Elaboration | | | |

**Drift Categories:**
- **Addition**: Repo adds content not in Notion
- **Contradiction**: Repo states opposite of Notion
- **Omission**: Repo missing Notion requirement
- **Elaboration**: Repo expands on Notion (acceptable if consistent)

---

## Section 3: Content Comparison

### 3.1 Lore Content

| Lore Domain | Notion Source | Repo Path | Hash Match | Notes |
|-------------|---------------|-----------|:----------:|-------|
| Cosmology | | `/docs/lore/00-cosmology/` | ✓/✗ | |
| Timeline | | `/docs/lore/01-time/` | ✓/✗ | |
| Magic | | `/docs/lore/02-magic-science/` | ✓/✗ | |
| Technology | | `/docs/lore/03-technology/` | ✓/✗ | |
| Factions | | `/docs/lore/04-factions/` | ✓/✗ | |
| Species | | `/docs/lore/05-species/` | ✓/✗ | |
| Entities | | `/docs/lore/06-entities/` | ✓/✗ | |

### 3.2 Game Data Files

| Data File | Notion Source | Match | Discrepancies |
|-----------|---------------|:-----:|---------------|
| `/data/biomes/*.json` | | ✓/✗ | |
| `/data/templates/*.json` | | ✓/✗ | |
| `/data/dialogues/*.json` | | ✓/✗ | |
| `/data/locales/*.json` | | ✓/✗ | |

### 3.3 Balance Constants

| Constant | Notion Value | Repo Value | Match | Location |
|----------|--------------|------------|:-----:|----------|
| Base Repair DC | | 8 | ✓/✗ | `BodgingService.cs` |
| Regen Percent | | 0.10f | ✓/✗ | `CreatureTraitService.cs` |
| | | | | |
| | | | | |

---

## Section 4: Domain 4 Compliance Check

For all POST-Glitch content, verify no precision measurements exist:

### 4.1 Forbidden Terms Scan

| Document | Forbidden Terms Found | Line References |
|----------|----------------------|-----------------|
| | Hz, dB, %, °C, meters | |
| | API, Bug, Glitch | |
| | | |

### 4.2 Voice Discipline Check

| Content Type | AAM-VOICE Compliant | Issues |
|--------------|:-------------------:|--------|
| Bestiary entries | ✓/✗ | |
| Item descriptions | ✓/✗ | |
| Room templates | ✓/✗ | |
| Dialogue | ✓/✗ | |

---

## Section 5: Remediation Plan

### 5.1 Critical (Must Fix)

| # | Issue | Source of Truth | Action | Owner | Due |
|---|-------|-----------------|--------|-------|-----|
| 1 | | Notion / Repo | | | |
| 2 | | | | | |

### 5.2 High Priority

| # | Issue | Action | Owner | Due |
|---|-------|--------|-------|-----|
| 1 | | | | |
| 2 | | | | |

### 5.3 Medium/Low Priority

| # | Issue | Action | Owner | Due |
|---|-------|--------|-------|-----|
| 1 | | | | |
| 2 | | | | |

---

## Section 6: Process Improvements

### 6.1 What Caused the Drift?

- [ ] No established sync process
- [ ] Emergency fixes bypassed Notion
- [ ] Design evolved during implementation
- [ ] Multiple contributors without coordination
- [ ] Notion not updated after implementation
- [ ] Other: ________________________________

### 6.2 Preventive Measures

| Measure | Implement By | Owner |
|---------|--------------|-------|
| Pre-commit Notion reference check | | |
| Weekly sync meeting | | |
| Changelog → Notion backflow | | |
| | | |

---

## Audit Summary

| Metric | Value |
|--------|-------|
| Features Audited | |
| Specifications Audited | |
| Content Files Audited | |
| Scope Creep Instances Found | |
| Drift Instances Found | |
| Critical Remediations Required | |
| Overall Alignment Score | __% |

### Verdict

- [ ] **ALIGNED** - Minimal drift, no critical issues
- [ ] **MINOR DRIFT** - Some discrepancies, easily correctable
- [ ] **SIGNIFICANT DRIFT** - Multiple issues, remediation sprint needed
- [ ] **CRITICAL MISALIGNMENT** - Major scope creep, design review required

---

## Sign-off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Auditor | | | |
| Reviewer | | | |
| Owner | | | |

---

## Next Steps

1. [ ] Complete remediations by [date]
2. [ ] Schedule follow-up audit for [date]
3. [ ] Implement preventive measures
4. [ ] Update NOTION-SYNC-AUDIT-STRATEGY.md if process changes

---

*Template version 1.0.0 - Based on NOTION-SYNC-AUDIT-STRATEGY.md*
