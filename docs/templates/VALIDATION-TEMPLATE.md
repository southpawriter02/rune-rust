# Domain [N]: [Domain Name] Validation Check

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                         VALIDATION TEMPLATE                                   ║
║                            Version 1.0.0                                      ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  PURPOSE: Domain compliance testing and validation rules                      ║
║  LAYER: Layer 4 (Ground Truth) - Definitive validation criteria              ║
║  AUDIENCE: QA, Content Validators, The Rune-Warden                           ║
║                                                                              ║
║  Use this template to define validation rules for each of the 9 Domains.      ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->

---

```yaml
# ══════════════════════════════════════════════════════════════════════════════
# FRONTMATTER - ⚠️ REQUIRED
# ══════════════════════════════════════════════════════════════════════════════
id: VAL-DOM-[NN]
title: "Domain [N]: [Domain Name] Validation"
version: 1.0
status: active                   # draft | active | deprecated
domain-id: DOM-[N]
severity-base: P[N]              # P1-Critical | P2-High | P3-Medium | P4-Low

# Scope
applies-to:
  - "[Content Type 1]"
  - "[Content Type 2]"
  - "[Content Type 3]"

# Metadata
last-updated: YYYY-MM-DD
author: "[Author Name]"
reviewers: []

# Cross-references
related-domains:
  - VAL-DOM-[NN]
related-specs:
  - SPEC-[DOM]-[NNN]

tags:
  - validation
  - domain-[n]
  - [tag]
```

---

## Table of Contents

1. [Domain Overview](#1-domain-overview)
2. [Canonical Ground Truth](#2-canonical-ground-truth)
3. [Key Constraints](#3-key-constraints)
4. [Terminology Reference](#4-terminology-reference)
5. [Validation Checklist](#5-validation-checklist)
6. [Common Violations](#6-common-violations)
7. [Green Flags](#7-green-flags)
8. [Decision Tree](#8-decision-tree)
9. [Remediation Strategies](#9-remediation-strategies)
10. [Validation Examples](#10-validation-examples)
11. [Automated Checks](#11-automated-checks)
12. [Changelog](#12-changelog)

---

## The 9 Domain Matrix Reference

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       THE 9 DOMAIN FRAMEWORK                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  DOM-1: COSMOLOGY        │ Creation, Aetheric source, world structure      │
│  DOM-2: TIMELINE         │ Eras, Glitch event, historical accuracy         │
│  DOM-3: MAGIC            │ Aetheric system, runes, manifestations          │
│  DOM-4: TECHNOLOGY       │ PRE/POST-Glitch constraints, precision rules    │
│  DOM-5: SPECIES          │ Sapient species, population, demographics       │
│  DOM-6: ENTITIES         │ Creatures, constructs, classifications          │
│  DOM-7: REALITY          │ Blight, corruption, environmental effects       │
│  DOM-8: GEOGRAPHY        │ Regions, settlements, territorial boundaries    │
│  DOM-9: COUNTER-RUNE     │ Opposition forces, faction conflicts            │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│  THIS DOCUMENT: Domain [N] - [Domain Name]                                  │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Domain Overview

> ⚠️ REQUIRED

### 1.1 Domain Identity

| Attribute | Value |
|-----------|-------|
| **Domain ID** | DOM-[N] |
| **Domain Name** | [Domain Name] |
| **Base Severity** | P[N] - [Critical/High/Medium/Low] |
| **Scope** | [What this domain governs] |

### 1.2 Domain Purpose

[2-3 sentences explaining what this domain protects and why it matters for world consistency]

### 1.3 Impact of Violations

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      VIOLATION IMPACT MATRIX                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SEVERITY        IMPACT                           RESPONSE                  │
│  ────────        ──────                           ────────                  │
│  P1-CRITICAL     Breaks core world logic          Block publication         │
│                  Contradicts established canon    Immediate remediation     │
│                                                                             │
│  P2-HIGH         Significant inconsistency        Require revision          │
│                  Confuses world rules             Before next review        │
│                                                                             │
│  P3-MEDIUM       Minor inconsistency              Flag for revision         │
│                  Style/voice deviation            Can publish with note     │
│                                                                             │
│  P4-LOW          Nitpick / preference             Optional revision         │
│                  Non-blocking issue               Document for future       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Canonical Ground Truth

> ⚠️ REQUIRED

### 2.1 Authoritative Sources

| Source | Type | Authority Level | Notes |
|--------|------|-----------------|-------|
| [Document] | [Spec/Design/Lore] | Primary | [What it defines] |
| [Document] | [Spec/Design/Lore] | Secondary | [What it defines] |

### 2.2 Core Truths

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      CANONICAL GROUND TRUTHS                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  TRUTH 1: [Statement of canonical fact]                                     │
│  ═══════════════════════════════════════                                    │
│  Source: [Where this is defined]                                            │
│  Immutable: [Yes/No - can this ever change?]                                │
│  Affects: [What content types this constrains]                              │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  TRUTH 2: [Statement of canonical fact]                                     │
│  ═══════════════════════════════════════                                    │
│  Source: [Where this is defined]                                            │
│  Immutable: [Yes/No]                                                        │
│  Affects: [What content types this constrains]                              │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  TRUTH 3: [Statement of canonical fact]                                     │
│  ═══════════════════════════════════════                                    │
│  Source: [Where this is defined]                                            │
│  Immutable: [Yes/No]                                                        │
│  Affects: [What content types this constrains]                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.3 Exceptions & Edge Cases

| Exception | Context | Handling |
|-----------|---------|----------|
| [Exception] | [When it applies] | [How to handle] |

---

## 3. Key Constraints

> ⚠️ REQUIRED

### 3.1 Hard Constraints (MUST)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      HARD CONSTRAINTS                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ❌ MUST NOT:                                                               │
│  ═══════════                                                                │
│                                                                             │
│  HC-1: [Forbidden action/content]                                           │
│        └── Rationale: [Why this is forbidden]                               │
│        └── Severity: P[N]                                                   │
│                                                                             │
│  HC-2: [Forbidden action/content]                                           │
│        └── Rationale: [Why this is forbidden]                               │
│        └── Severity: P[N]                                                   │
│                                                                             │
│  HC-3: [Forbidden action/content]                                           │
│        └── Rationale: [Why this is forbidden]                               │
│        └── Severity: P[N]                                                   │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  ✓ MUST:                                                                    │
│  ═══════                                                                    │
│                                                                             │
│  HC-4: [Required action/content]                                            │
│        └── Rationale: [Why this is required]                                │
│        └── Severity: P[N]                                                   │
│                                                                             │
│  HC-5: [Required action/content]                                            │
│        └── Rationale: [Why this is required]                                │
│        └── Severity: P[N]                                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Soft Constraints (SHOULD)

| Constraint | Priority | Rationale |
|------------|----------|-----------|
| [Constraint] | P[N] | [Why this is preferred] |
| [Constraint] | P[N] | [Why this is preferred] |

### 3.3 Layer-Specific Constraints

| Layer | Additional Constraints |
|-------|----------------------|
| Layer 1 (Mythic) | [Constraints for mythic content] |
| Layer 2 (Diagnostic) | [Constraints for diagnostic content] |
| Layer 3 (Technical) | [Constraints for technical content] |
| Layer 4 (Ground Truth) | [Constraints for ground truth content] |

---

## 4. Terminology Reference

> ⚠️ REQUIRED

### 4.1 Required Terms

| Context | Required Term | Forbidden Alternatives |
|---------|---------------|----------------------|
| [Context] | "[Correct term]" | "[Forbidden]", "[Forbidden]" |
| [Context] | "[Correct term]" | "[Forbidden]", "[Forbidden]" |

### 4.2 Forbidden Vocabulary

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      FORBIDDEN VOCABULARY                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  CATEGORY: [Category Name]                                                  │
│  ══════════════════════════                                                 │
│                                                                             │
│  FORBIDDEN          ACCEPTABLE ALTERNATIVES                                 │
│  ─────────          ──────────────────────                                  │
│  [word]             "[alt1]", "[alt2]", "[alt3]"                            │
│  [word]             "[alt1]", "[alt2]"                                      │
│  [word]             "[alt1]", "[alt2]", "[alt3]"                            │
│                                                                             │
│  CATEGORY: [Category Name]                                                  │
│  ══════════════════════════                                                 │
│                                                                             │
│  FORBIDDEN          ACCEPTABLE ALTERNATIVES                                 │
│  ─────────          ──────────────────────                                  │
│  [word]             "[alt1]", "[alt2]"                                      │
│  [word]             "[alt1]"                                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Context-Sensitive Terms

| Term | Layer 1 Usage | Layer 2 Usage | Layer 3 Usage |
|------|---------------|---------------|---------------|
| [Concept] | "[L1 term]" | "[L2 term]" | "[L3 term]" |

---

## 5. Validation Checklist

> ⚠️ REQUIRED

### 5.1 Pre-Publication Checklist

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      VALIDATION CHECKLIST                                    │
│                      Domain [N]: [Domain Name]                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  DOCUMENT: _________________________________ DATE: _______________          │
│  VALIDATOR: ________________________________                                │
│                                                                             │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                             │
│  HARD CONSTRAINT CHECKS:                                                    │
│                                                                             │
│  [ ] HC-1: [Check description]                                              │
│      └── Pass / Fail / N/A                                                  │
│                                                                             │
│  [ ] HC-2: [Check description]                                              │
│      └── Pass / Fail / N/A                                                  │
│                                                                             │
│  [ ] HC-3: [Check description]                                              │
│      └── Pass / Fail / N/A                                                  │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  TERMINOLOGY CHECKS:                                                        │
│                                                                             │
│  [ ] No forbidden vocabulary present                                        │
│  [ ] Required terminology used correctly                                    │
│  [ ] Layer-appropriate language                                             │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  CONSISTENCY CHECKS:                                                        │
│                                                                             │
│  [ ] Consistent with canonical sources                                      │
│  [ ] No contradictions with existing content                                │
│  [ ] Cross-references valid                                                 │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  RESULT:                                                                    │
│  [ ] PASS - All checks passed                                               │
│  [ ] CONDITIONAL PASS - Minor issues noted                                  │
│  [ ] FAIL - Critical issues found                                           │
│                                                                             │
│  NOTES:                                                                     │
│  _____________________________________________________________________     │
│  _____________________________________________________________________     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Quick Validation Matrix

| Check | Critical | High | Medium | Low |
|-------|----------|------|--------|-----|
| [Check 1] | [ ] | [ ] | [ ] | [ ] |
| [Check 2] | [ ] | [ ] | [ ] | [ ] |
| [Check 3] | [ ] | [ ] | [ ] | [ ] |

---

## 6. Common Violations

> ⚠️ REQUIRED

### 6.1 Violation Catalog

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  VIOLATION: V-[N]-001 - [Violation Name]                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SEVERITY: P[N]                                                             │
│  FREQUENCY: [Common / Occasional / Rare]                                    │
│                                                                             │
│  DESCRIPTION:                                                               │
│  [What the violation looks like]                                            │
│                                                                             │
│  EXAMPLE (BAD):                                                             │
│  "[Example of violating content]"                                           │
│                                                                             │
│  WHY IT'S WRONG:                                                            │
│  [Explanation of why this violates the domain]                              │
│                                                                             │
│  DETECTION:                                                                 │
│  ├── Automated: [Yes/No] - [How to detect]                                  │
│  └── Manual: [What to look for]                                             │
│                                                                             │
│  REMEDIATION:                                                               │
│  [How to fix this violation]                                                │
│                                                                             │
│  EXAMPLE (CORRECTED):                                                       │
│  "[Corrected version of the example]"                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6.2 Violation Frequency Table

| Violation ID | Name | Severity | Frequency | Auto-Detect |
|--------------|------|----------|-----------|-------------|
| V-[N]-001 | [Name] | P[N] | [Freq] | Yes/No |
| V-[N]-002 | [Name] | P[N] | [Freq] | Yes/No |

---

## 7. Green Flags

> 📋 RECOMMENDED

### 7.1 Indicators of Compliance

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      GREEN FLAGS                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ✓ GREEN FLAG 1: [Indicator of good compliance]                             │
│    └── [What this looks like in practice]                                   │
│                                                                             │
│  ✓ GREEN FLAG 2: [Indicator of good compliance]                             │
│    └── [What this looks like in practice]                                   │
│                                                                             │
│  ✓ GREEN FLAG 3: [Indicator of good compliance]                             │
│    └── [What this looks like in practice]                                   │
│                                                                             │
│  ✓ GREEN FLAG 4: [Indicator of good compliance]                             │
│    └── [What this looks like in practice]                                   │
│                                                                             │
│  ✓ GREEN FLAG 5: [Indicator of good compliance]                             │
│    └── [What this looks like in practice]                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Exemplary Content

| Document | Why It's Good | Link |
|----------|---------------|------|
| [Document] | [What makes it compliant] | [Link] |

---

## 8. Decision Tree

> ⚠️ REQUIRED

### 8.1 Validation Decision Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    VALIDATION DECISION TREE                                  │
│                    Domain [N]: [Domain Name]                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                    ┌─────────────────────────┐                              │
│                    │   Start Validation      │                              │
│                    │   [Content to check]    │                              │
│                    └───────────┬─────────────┘                              │
│                                │                                            │
│                                ▼                                            │
│                    ┌─────────────────────────┐                              │
│                    │   Q1: [First question   │                              │
│               ┌────│   to determine path]?   │────┐                         │
│               │    └─────────────────────────┘    │                         │
│              YES                                 NO                         │
│               │                                   │                         │
│               ▼                                   ▼                         │
│    ┌─────────────────────┐             ┌─────────────────────┐              │
│    │ Q2: [Follow-up]?    │             │ Q3: [Alternate]?    │              │
│    └──────────┬──────────┘             └──────────┬──────────┘              │
│               │                                   │                         │
│         ┌─────┴─────┐                       ┌─────┴─────┐                   │
│        YES         NO                      YES         NO                   │
│         │           │                       │           │                   │
│         ▼           ▼                       ▼           ▼                   │
│    ┌────────┐  ┌────────┐              ┌────────┐  ┌────────┐               │
│    │  PASS  │  │ FAIL   │              │  PASS  │  │ CHECK  │               │
│    │        │  │ P[N]   │              │        │  │ DOM-X  │               │
│    └────────┘  └────────┘              └────────┘  └────────┘               │
│                                                                             │
│  OUTCOMES:                                                                  │
│  ─────────                                                                  │
│  PASS: Content is compliant with Domain [N]                                 │
│  FAIL P1: Critical violation - block publication                            │
│  FAIL P2: High violation - require revision                                 │
│  FAIL P3: Medium violation - flag for revision                              │
│  CHECK DOM-X: Delegate to Domain X for assessment                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Edge Case Decisions

| Scenario | Decision | Rationale |
|----------|----------|-----------|
| [Edge case] | [Pass/Fail/Escalate] | [Why] |

---

## 9. Remediation Strategies

> ⚠️ REQUIRED

### 9.1 Remediation by Violation Type

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  REMEDIATION: [Violation Type]                                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  STEP 1: Identify                                                           │
│  [How to locate all instances of this violation]                            │
│                                                                             │
│  STEP 2: Analyze                                                            │
│  [How to understand the context and severity]                               │
│                                                                             │
│  STEP 3: Correct                                                            │
│  [Specific actions to fix the violation]                                    │
│                                                                             │
│  STEP 4: Verify                                                             │
│  [How to confirm the fix is complete]                                       │
│                                                                             │
│  STEP 5: Prevent                                                            │
│  [How to prevent recurrence]                                                │
│                                                                             │
│  ───────────────────────────────────────────────────────────────────────    │
│                                                                             │
│  BEFORE:                                                                    │
│  "[Example of violating content]"                                           │
│                                                                             │
│  AFTER:                                                                     │
│  "[Example of corrected content]"                                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Quick Fix Reference

| Violation | Quick Fix | Time Estimate |
|-----------|-----------|---------------|
| [Violation] | [Fix description] | [Effort] |

---

## 10. Validation Examples

> ⚠️ REQUIRED

### 10.1 Passing Examples

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE: PASS - [Description]                                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  CONTENT:                                                                   │
│  "[Example of compliant content]"                                           │
│                                                                             │
│  ANALYSIS:                                                                  │
│  ├── ✓ [Check 1]: [Why it passes]                                           │
│  ├── ✓ [Check 2]: [Why it passes]                                           │
│  └── ✓ [Check 3]: [Why it passes]                                           │
│                                                                             │
│  VERDICT: PASS                                                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 10.2 Failing Examples

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  EXAMPLE: FAIL - [Description]                                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  CONTENT:                                                                   │
│  "[Example of non-compliant content]"                                       │
│                                                                             │
│  ANALYSIS:                                                                  │
│  ├── ✗ [Check 1]: [Why it fails]                                            │
│  ├── ✓ [Check 2]: [Passes this check]                                       │
│  └── ✗ [Check 3]: [Why it fails]                                            │
│                                                                             │
│  VIOLATIONS FOUND:                                                          │
│  ├── V-[N]-001: [Specific violation]                                        │
│  └── V-[N]-003: [Specific violation]                                        │
│                                                                             │
│  VERDICT: FAIL (P2)                                                         │
│                                                                             │
│  REMEDIATION:                                                               │
│  "[Corrected version of the content]"                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 10.3 Edge Case Examples

| Example | Content | Decision | Rationale |
|---------|---------|----------|-----------|
| [Edge case] | "[Content]" | Pass/Fail | [Why] |

---

## 11. Automated Checks

> 📋 RECOMMENDED

### 11.1 Regex Patterns

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      AUTOMATED DETECTION PATTERNS                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  PATTERN: [Pattern Name]                                                    │
│  ══════════════════════                                                     │
│                                                                             │
│  Purpose: [What this pattern detects]                                       │
│  Severity: P[N]                                                             │
│                                                                             │
│  Regex: `[regex pattern]`                                                   │
│                                                                             │
│  Matches:                                                                   │
│  ├── "[example match 1]"                                                    │
│  ├── "[example match 2]"                                                    │
│  └── "[example match 3]"                                                    │
│                                                                             │
│  Does NOT Match:                                                            │
│  ├── "[valid content that should not match]"                                │
│  └── "[valid content that should not match]"                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 11.2 Validation Script

```bash
# ═══════════════════════════════════════════════════════════════════════════
# Domain [N] Validation Script
# ═══════════════════════════════════════════════════════════════════════════

#!/bin/bash

# Check for [violation type]
grep -rn "[pattern]" docs/ --include="*.md"

# Check for [another violation type]
grep -rn "[pattern]" docs/ --include="*.md"
```

### 11.3 Integration Points

| Tool | Check | Configuration |
|------|-------|---------------|
| [Tool] | [What it checks] | [How to configure] |

---

## 12. Changelog

> 🔒 LOCKED

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | YYYY-MM-DD | [Author] | Initial validation rules |

---

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                              END OF TEMPLATE                                  ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  VALIDATION DOCUMENT CHECKLIST:                                               ║
║  ────────────────────────────────────────────────────────────────────────────║
║  □ Canonical ground truths documented                                         ║
║  □ All hard constraints listed                                                ║
║  □ Terminology reference complete                                             ║
║  □ Validation checklist ready to use                                          ║
║  □ Common violations cataloged                                                ║
║  □ Decision tree tested                                                       ║
║  □ Remediation strategies provided                                            ║
║  □ Examples (pass/fail) included                                              ║
║  □ Automated checks documented                                                ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->
