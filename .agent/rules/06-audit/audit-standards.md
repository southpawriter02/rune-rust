---
trigger: always_on
---

## Audit Standards

### Purpose

Audits ensure documentation quality and consistency. Every spec should be periodically reviewed against templates and golden standards to identify gaps.

### Audit Methodology

**Step 1: Identify spec category**
- Specialization, Ability, Status Effect, System, Resource, Craft, etc.

**Step 2: Load reference materials**
- Template: `/docs/.templates/{category}.md`
- Golden standard: See Golden Standards section

**Step 3: Check each required section**
- Mark present/absent
- Note quality issues (incomplete, outdated, wrong format)

**Step 4: Check content quality**
- Dice notation (d10 for resolution, tiered d4-d10 for damage)
- Link validity (no broken relative links)
- Table formatting (proper alignment)
- Mermaid diagram syntax

**Step 5: Calculate conformance score**
- Count present required sections ÷ total required sections

### Conformance Scoring

| Score | Rating | Action |
|-------|--------|--------|
| 90-100% | ✅ Excellent | No action needed |
| 75-89% | 🟡 Good | Minor additions |
| 50-74% | 🟠 Needs Work | Schedule remediation |
| Below 50% | 🔴 Critical | Prioritize immediately |

### Conformance Checklist (All Specs)

| Section | Required? | Check |
|---------|-----------|-------|
| YAML frontmatter (id, title, version, status, last-updated) | ✅ All | [ ] |
| Opening flavor quote | ✅ All | [ ] |
| Identity/Quick Reference table | ✅ All | [ ] |
| Mechanical effects | ✅ All | [ ] |
| Mermaid workflow diagram | ✅ Complex | [ ] |
| Balance Data section | ✅ All | [ ] |
| Voice Guidance reference | ✅ All | [ ] |
| Implementation Status checklist | ✅ All | [ ] |
| Changelog | ✅ All | [ ] |
| Technical Implementation (C#) | ✅ Complex | [ ] |

### Content Quality Checks

| Check | Flag If |
|-------|---------|
| **Dice notation** | Uses `d10` as universal die without context |
| **Status effect refs** | Not wrapped in brackets: `Bleeding` → `[Bleeding]` |
| **Links** | Broken or absolute paths instead of relative |
| **Tables** | Misaligned or missing headers |
| **Code blocks** | Missing language tag |
| **Mermaid** | Syntax errors or missing diagrams for complex flows |

### Issue Documentation Format

For each spec audited, document:

```markdown
### [spec-name.md]

**Score:** 75% (9/12 sections)
**Path:** `docs/path/to/spec.md`

**Missing Sections:**
- [ ] Balance Data (line ~150)
- [ ] Changelog (end of file)
- [ ] Voice Guidance reference

**Content Issues:**
- Line 45: Uses `6d10` without proficiency context
- Line 78: Broken link to `../status-effects/bleeding.md`

**Priority:** Medium
```

### Remediation Priority

| Priority | Criteria | Timeline |
|----------|----------|----------|
| 🔴 **Critical** | Score < 50%, or blocking development | Immediate |
| 🟠 **High** | Missing Balance Data or Implementation Status | This sprint |
| 🟡 **Medium** | Missing Changelog, Voice Guidance | Next sprint |
| 🟢 **Low** | Minor formatting, dice notation | Backlog |

### Batch Audit Process

When auditing multiple specs:

1. **Group by category** — All abilities together, all status effects together
2. **Sample first** — Audit 3-5 per category to identify patterns
3. **Document common gaps** — Issues appearing in most specs
4. **Prioritize universal fixes** — E.g., add Changelog to all specs at once
5. **Track progress** — Update audit report after each batch

### Audit Report Structure

```markdown
# Audit Report

## Summary
| Category | Specs Audited | Avg Score |
|----------|---------------|-----------|
| Abilities | 5 | 62% |

## Universal Gaps
- Balance Data: Missing in 100%
- Changelog: Missing in 100%

## Per-Spec Issues
### wild-swing.md
...
```