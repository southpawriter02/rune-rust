# Archetypes & Specializations System — Specification Review Report

**Review Date**: 2025-12-11
**Reviewer**: Claude Code (Spec Review Agent)
**Scope**: Comprehension, Completeness, and Validation Testing
**Status**: **CRITICAL ISSUES IDENTIFIED**

---

## Executive Summary

The Archetypes and Specializations specification suite contains well-structured documentation with strong conceptual design. However, **critical formula inconsistencies** exist between documents that would cause implementation failures. Additionally, **incomplete coverage** of specializations and **validation test gaps** require attention before implementation.

| Category | Status | Issues |
|----------|--------|--------|
| **Comprehension** | PASS with notes | Clear structure, minor terminology conflicts |
| **Completeness** | PARTIAL | 1 of 17+ specializations fully documented |
| **Validation Testing** | FAIL | Formulas conflict; tests would fail |

---

## CRITICAL: Formula Inconsistencies

### CRIT-001: Stamina Formula Conflict

**Severity**: CRITICAL
**Impact**: Character creation will produce different values depending on which spec is followed

| Document | Formula | Warrior Result |
|----------|---------|----------------|
| `archetypes-and-specializations.md:79` | `20 + (MIGHT + FINESSE) × 5` | 20 + (4+3)×5 = **55** |
| `resources.md:77` | `50 + (STURDINESS × 5) + (FINESSE × 2)` | 50 + (4×5) + (3×2) = **76** |
| `archetypes.md:136` | `20 + (MIGHT + FINESSE) × 5` | 20 + (4+3)×5 = **55** |

**Recommendation**: Standardize to `20 + (MIGHT + FINESSE) × 5` as used in primary specs. Update `resources.md:77`.

---

### CRIT-002: Aether Pool Formula Conflict

**Severity**: CRITICAL
**Impact**: Mystic AP calculation will be incorrect

| Document | Formula | Mystic Result |
|----------|---------|---------------|
| `archetypes-and-specializations.md:80` | `20 + (WILL + WITS) × 5` | 20 + (4+4)×5 = **60** (70 with Attunement) |
| `resources.md:78` | `50 + (WILL × 10)` | 50 + (4×10) = **90** |
| `mystic.md:94` | `20 + (WILL + WITS) × 5 + 10` | 20 + 40 + 10 = **70** |

**Recommendation**: Standardize to `20 + (WILL + WITS) × 5` as primary formula, with Aetheric Attunement providing +10. Update `resources.md:78`.

---

### CRIT-003: Archetype ID Mismatch

**Severity**: CRITICAL
**Impact**: Database foreign keys will fail; specialization lookups will be incorrect

| Document | Warrior | Adept | Skirmisher | Mystic |
|----------|---------|-------|------------|--------|
| `archetypes-and-specializations.md:253` | 1 | 2 | **4** | **5** |
| `archetypes.md` (DB Schema:242-246) | 1 | 2 | **3** | **4** |
| Individual archetype specs | 1 | 2 | **3** | **4** |

**Recommendation**: Standardize to 1=Warrior, 2=Adept, 3=Skirmisher, 4=Mystic. Fix `archetypes-and-specializations.md:253`.

---

### CRIT-004: Attribute Base Value Conflict

**Severity**: HIGH
**Impact**: Point-buy system will calculate incorrectly

| Document | Base Value | Point Pool |
|----------|------------|------------|
| `attributes.md:87` | All start at **5** | 15 points |
| `character-creation.md:179` | All start at **1** | 15 points |

**Recommendation**: Reconcile base values. If base is 5, the 15-point pool produces attributes 5-10. If base is 1, the pool produces 1-6. Current archetype distributions (2-4 range) suggest base 1 is intended for display, but internal values are 5+.

---

## HIGH: Structural Inconsistencies

### HIGH-001: Tier 1 PP Cost Conflict

**Severity**: HIGH
**Impact**: Progression economy will be miscalculated

| Document | Tier 1 Cost |
|----------|-------------|
| `archetypes-and-specializations.md:109` | **0 PP** (granted free with unlock) |
| `_template.md:92` | **3 PP each** |
| `atgeir-wielder/atgeir-wielder-overview.md:103-105` | **3 PP each** |

**Recommendation**: Clarify whether Tier 1 abilities cost 0 PP (granted automatically with 3 PP unlock) or 3 PP each (total 9 PP for Tier 1). The main spec implies free grant, but template implies purchase.

---

### HIGH-002: Path Type Terminology Mismatch

**Severity**: MEDIUM
**Impact**: Documentation inconsistency

| Template Term | Actual Usage |
|---------------|--------------|
| `Coherent` | Used in specializations |
| `Divergent` | **Never used** — replaced by `Heretical` |
| `Hybrid` | Not observed in current specs |

**Recommendation**: Update `_template.md` to use `Coherent / Heretical / Hybrid`.

---

### HIGH-003: Derived Stats Table Conflicts

**Severity**: MEDIUM
**Impact**: HP calculation inconsistencies

| Document | Max HP Formula |
|----------|----------------|
| `resources.md:76` | `50 + (STURDINESS × 10)` |
| `attributes.md:179` | `50 + (STURDINESS × 10) + Gear − Corruption%` |
| `warrior.md:92` | `50 + (STURDINESS × 10) + 10% Vigor` |

**Issue**: Warrior passive (Warrior's Vigor) adds +10% HP, but this isn't reflected in the generic resource formula. Corruption penalty exists in attributes spec but not resources spec.

**Recommendation**: Create unified derived stats table with all modifiers clearly documented.

---

## MODERATE: Completeness Gaps

### MOD-001: Specialization Documentation Coverage

**Severity**: MODERATE
**Impact**: Implementation cannot proceed for undocumented specializations

| Archetype | Claimed | Fully Documented | Gap |
|-----------|---------|------------------|-----|
| **Warrior** | 6 | 1 (Atgeir-Wielder) | 5 missing |
| **Skirmisher** | 4 | 0 | 4 missing |
| **Mystic** | 2+ | 0 | 2+ missing |
| **Adept** | 5 | 0 | 5 missing |
| **Total** | 17+ | 1 | **16+ missing** |

**Recommendation**: Prioritize documentation of at least one specialization per archetype before alpha.

---

### MOD-002: Missing Referenced Specifications

The following specs are referenced but not found in the current structure:

| Missing Spec ID | Referenced By |
|-----------------|---------------|
| `SPEC-CORE-SAGA` | archetypes-and-specializations.md, archetypes.md |
| `SPEC-CORE-RES-MOMENTUM` | skirmisher.md |
| `SPEC-CORE-RES-AETHER` | mystic.md |
| `SPEC-CORE-TRAUMA` | mystic.md |
| `SPEC-CORE-ATTR-*` (individual) | All archetype specs |
| `SPEC-SPEC-*` (specializations) | All archetype specs |

**Recommendation**: Create stub files for referenced specs or update references to existing files.

---

### MOD-003: Resource System Documentation Gaps

| Resource | Claimed Location | Status |
|----------|------------------|--------|
| `rage.md` | resources/rage.md | **NOT FOUND** |
| `momentum.md` | resources/momentum.md | **NOT FOUND** |
| `coherence.md` | resources/coherence.md | **NOT FOUND** |

**Recommendation**: Create specialization resource specifications or remove from resources.md.

---

## LOW: Minor Issues

### LOW-001: Version Status Inconsistency

| Document | Status |
|----------|--------|
| `archetypes-and-specializations.md` | `proposed` |
| `warrior.md`, `skirmisher.md`, etc. | `draft` |
| `_template.md` | `approved` |
| `atgeir-wielder/atgeir-wielder-overview.md` | `approved` |

**Recommendation**: Standardize status labels. Suggest: `draft` → `proposed` → `approved` → `final`.

---

### LOW-002: Unchecked Test Cases

The main spec (`archetypes-and-specializations.md:265-268`) contains test cases marked incomplete:

```markdown
- [ ] **Creation Flow**: Verify first specialization is free.
```

Other tests are inconsistently marked (no checkboxes).

**Recommendation**: Use consistent checkbox format for all test cases.

---

### LOW-003: Warrior HP Calculation Note

The Warrior spec states 99 HP at creation:
- Base: 50 + (4×10) = 90
- Vigor (+10%): 90 × 1.10 = **99**

This is correctly calculated. However, the Vigor bonus compounds with Milestones (line 280), which may not be intended:
```
Milestone 5: (90 + 50) × 1.10 = 154
```

**Recommendation**: Clarify if Vigor applies to base only or base+milestone.

---

## Validation Testing Results

### Formula Validation Tests

| Test | Expected | Actual | Status |
|------|----------|--------|--------|
| Warrior Stamina | 55 | 55 or 76 (conflicting formulas) | **FAIL** |
| Mystic AP | 70 | 70 or 90 (conflicting formulas) | **FAIL** |
| Skirmisher HP | 80 | 80 | PASS |
| Adept Attribute Total | 14 | 14 | PASS |
| Warrior Attribute Total | 15 | 15 | PASS |

### Archetype ID Validation

| Test | Expected | Actual | Status |
|------|----------|--------|--------|
| Warrior ID | 1 | 1 | PASS |
| Adept ID | 2 | 2 | PASS |
| Skirmisher ID | 3 | 3 or 4 (conflicting) | **FAIL** |
| Mystic ID | 4 | 4 or 5 (conflicting) | **FAIL** |

### PP Economy Validation

| Milestone | Expected PP | Abilities Available | Status |
|-----------|-------------|---------------------|--------|
| 0 | 0 | 3 (Tier 1 free) or 0 (if Tier 1 costs PP) | **UNCLEAR** |
| 3 | 3 | Can unlock 2nd spec OR save | PASS |
| 12 | 12 | Tier 3 in primary OR Tier 2 in two | PASS |

---

## Recommendations Summary

### Priority 1: Critical Fixes (Before Implementation)

1. **Standardize Stamina formula** across all documents to `20 + (MIGHT + FINESSE) × 5`
2. **Standardize AP formula** across all documents to `20 + (WILL + WITS) × 5`
3. **Fix Archetype IDs** to consistent 1-2-3-4 mapping
4. **Clarify attribute base values** (1 or 5)
5. **Resolve Tier 1 PP cost** (free vs 3 PP each)

### Priority 2: High-Priority Improvements

1. Create unified derived stats reference table
2. Update template path types to match usage
3. Document at least one specialization per archetype

### Priority 3: Documentation Hygiene

1. Create stub files for missing spec references
2. Standardize version status labels
3. Complete test case checklists
4. Add specialization resource specifications

---

## Appendix: Files Reviewed

| File | Path | Status |
|------|------|--------|
| Main Archetype Spec | `docs/03-character/archetypes-and-specializations.md` | Reviewed |
| Character Creation | `docs/01-core/character-creation.md` | Reviewed |
| Archetypes Overview | `docs/02-entities/archetypes/archetypes.md` | Reviewed |
| Warrior Archetype | `docs/02-entities/archetypes/warrior.md` | Reviewed |
| Skirmisher Archetype | `docs/02-entities/archetypes/skirmisher.md` | Reviewed |
| Mystic Archetype | `docs/02-entities/archetypes/mystic.md` | Reviewed |
| Adept Archetype | `docs/02-entities/archetypes/adept.md` | Reviewed |
| Specialization Template | `docs/02-entities/specializations/_template.md` | Reviewed |
| Atgeir-Wielder Overview | `docs/02-entities/specializations/atgeir-wielder/atgeir-wielder-overview.md` | Reviewed |
| Skewer Ability | `docs/02-entities/specializations/atgeir-wielder/abilities/skewer.md` | Reviewed |
| Living Fortress Ability | `docs/02-entities/specializations/atgeir-wielder/abilities/living-fortress.md` | Reviewed |
| Resources Overview | `docs/01-core/resources/resources.md` | Reviewed |
| Attributes Overview | `docs/01-core/attributes/attributes.md` | Reviewed |

---

*End of Review Report*
