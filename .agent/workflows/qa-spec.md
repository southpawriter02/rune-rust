---
description: How to QA a spec to ensure it meets the golden standard
---

# QA Workflow: Spec Conformance Check

This workflow ensures a spec meets the golden standard for its category.

## Step 1: Identify Spec Category

Determine which category the spec belongs to:

| Category | Location Pattern | Golden Standard |
|----------|------------------|-----------------|
| **Specialization** | `03-character/specializations/{name}/overview.md` | `berserkr/overview.md` |
| **Ability** | `03-character/specializations/{spec}/abilities/{ability}.md` | `corridor-maker.md` |
| **Status Effect** | `04-systems/status-effects/{effect}.md` | `bleeding.md` |
| **Core System** | `01-core/{system}.md` | (use system.md template) |
| **Resource** | `01-core/resources/{resource}.md` | (use resource.md template) |
| **Crafting Trade** | `04-systems/crafting/{trade}.md` | (use craft.md template) |

## Step 2: Open Reference Materials

Open both the spec being reviewed and its reference:

```
# For Specialization
docs/03-character/specializations/berserkr/overview.md

# For Ability
docs/03-character/specializations/ruin-stalker/abilities/corridor-maker.md

# For Status Effect
docs/04-systems/status-effects/bleeding.md

# Template (if no golden standard exists)
docs/.templates/{category}.md
```

## Step 3: Check Required Sections

### All Specs Must Have:

| Section | Check | Notes |
|---------|-------|-------|
| YAML frontmatter | [ ] | id, title, version, status, last-updated |
| Opening flavor quote | [ ] | Immediately after title |
| Identity/Quick Reference table | [ ] | Key properties in table format |
| Mechanical Effects | [ ] | How it works mechanically |
| Balance Data | [ ] | Power curves, effectiveness ratings |
| Voice Guidance | [ ] | Reference to flavor-text templates |
| Implementation Status | [ ] | Checklist of dev tasks |
| Changelog | [ ] | Version history at end |

### Specialization-Specific:

| Section | Check |
|---------|-------|
| Core philosophy/tagline | [ ] |
| Ability tree with tier/rank structure | [ ] |
| Resource economy | [ ] |
| Related documentation links | [ ] |

### Ability-Specific:

| Section | Check |
|---------|-------|
| Quick reference table (tier, cost, range) | [ ] |
| Rank progression | [ ] |
| Workflow diagram (Mermaid) | [ ] |
| Synergies section | [ ] |
| Tactical applications | [ ] |
| Technical Implementation (C# interface) | [ ] |

### Status Effect-Specific:

| Section | Check |
|---------|-------|
| Application/removal triggers | [ ] |
| Stacking rules | [ ] |
| Duration mechanics | [ ] |
| Synergies AND Conflicts | [ ] |
| Counter-play options | [ ] |

## Step 4: Check Content Quality

| Check | Flag If |
|-------|---------|
| **Dice notation** | Resolution not using d10, or damage using wrong tier |
| **Status effects** | Not wrapped in brackets: `Bleeding` should be `[Bleeding]` |
| **Links** | Absolute paths instead of relative |
| **Tables** | Misaligned columns or missing headers |
| **Code blocks** | Missing language tag (csharp, sql, etc.) |
| **Mermaid diagrams** | Syntax errors or missing for complex flows |
| **Formulas** | Not in code blocks |

## Step 5: Verify Formatting

| Element | Expected |
|---------|----------|
| **Section numbering** | `## 1. Overview`, `### 1.1 Details` |
| **Bold** | Key terms, column headers, important values |
| **Italics** | Flavor text quotes |
| **Horizontal rules** | `---` between major sections |
| **GitHub alerts** | Used sparingly (NOTE, WARNING, CAUTION) |

## Step 6: Calculate Conformance Score

```
Score = (Present Sections / Required Sections) × 100
```

| Score | Rating | Action |
|-------|--------|--------|
| 90-100% | ✅ Excellent | Ready for approval |
| 75-89% | 🟡 Good | Minor additions needed |
| 50-74% | 🟠 Needs Work | Schedule remediation |
| Below 50% | 🔴 Critical | Prioritize immediately |

## Step 7: Document Issues

For each issue found, document:

```markdown
### [spec-name.md]

**Score:** X% (Y/Z sections)
**Category:** [Specialization|Ability|Status Effect|etc.]
**Golden Standard:** [reference file]

**Missing Sections:**
- [ ] Section name (where it should go)
- [ ] Section name

**Content Issues:**
- Line X: Issue description
- Line Y: Issue description

**Formatting Issues:**
- Line X: Issue description

**Priority:** [Critical|High|Medium|Low]
```

## Step 8: Compare Line Count

Golden standards have representative line counts:

| Category | Golden Standard | Lines |
|----------|-----------------|-------|
| Specialization | berserkr/overview.md | ~320 |
| Ability | corridor-maker.md | ~226 |
| Status Effect | bleeding.md | ~284 |

If the spec is significantly shorter, it likely lacks depth.

## QA Checklist Summary

```markdown
## QA Checklist for: [spec-name.md]

**Category:** _______________
**Golden Standard Used:** _______________

### Structure
- [ ] YAML frontmatter complete
- [ ] Opening flavor quote present
- [ ] Identity table present
- [ ] All category-specific sections present
- [ ] Balance Data section present
- [ ] Voice Guidance reference present
- [ ] Implementation Status checklist present
- [ ] Changelog present

### Content Quality
- [ ] Dice notation correct (d10 resolution, tiered damage)
- [ ] Status effects in brackets [Effect]
- [ ] All links are relative paths
- [ ] Tables properly formatted
- [ ] Code blocks have language tags
- [ ] Mermaid diagrams render correctly

### Formatting
- [ ] Numbered section headers
- [ ] Horizontal rules between sections
- [ ] Bold for key terms
- [ ] Italics for flavor text

**Conformance Score:** ___/___
**Priority:** _______________
**Approved:** [ ] Yes  [ ] No — needs revision
```
