# Changelog: v[X.Y.Z] - [Thematic Title]

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                         CHANGELOG TEMPLATE                                    ║
║                            Version 1.0.0                                      ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  PURPOSE: Technical release documentation with complete traceability          ║
║  LAYER: Layer 4 (Ground Truth) - Full technical precision                    ║
║  AUDIENCE: Developers, QA, Project Managers                                   ║
║                                                                              ║
║  Generate after each release with complete file and test tracking.            ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->

---

```yaml
# ══════════════════════════════════════════════════════════════════════════════
# FRONTMATTER - ⚠️ REQUIRED
# ══════════════════════════════════════════════════════════════════════════════
version: v[X.Y.Z]
title: "[Thematic Title]"
release-date: YYYY-MM-DD
status: released                 # draft | pre-release | released
release-type: "[Type]"           # major | minor | patch | hotfix

# Metrics
total-files-changed: [N]
new-files: [N]
modified-files: [N]
deleted-files: [N]
total-tests: [N]
new-tests: [N]
test-coverage: "[X%]"

# References
milestone: "[Milestone Name]"
plan-document: "docs/plans/vX.Y.x/vX.Y.Z.md"
previous-version: "v[X.Y.Z-1]"

# Contributors
contributors:
  - "[Contributor 1]"
  - "[Contributor 2]"

tags:
  - [tag1]
  - [tag2]
```

---

## Table of Contents

1. [Release Summary](#1-release-summary)
2. [Breaking Changes](#2-breaking-changes)
3. [New Features](#3-new-features)
4. [Improvements](#4-improvements)
5. [Bug Fixes](#5-bug-fixes)
6. [New Files](#6-new-files)
7. [Modified Files](#7-modified-files)
8. [Deleted Files](#8-deleted-files)
9. [Implementation Details](#9-implementation-details)
10. [Logging Matrix](#10-logging-matrix)
11. [Test Coverage](#11-test-coverage)
12. [DI Registration](#12-di-registration)
13. [Database Changes](#13-database-changes)
14. [Configuration Changes](#14-configuration-changes)
15. [Known Issues](#15-known-issues)
16. [Upgrade Guide](#16-upgrade-guide)

---

## 1. Release Summary

> ⚠️ REQUIRED

### 1.1 Overview

| Attribute | Value |
|-----------|-------|
| **Version** | v[X.Y.Z] |
| **Release Date** | YYYY-MM-DD |
| **Type** | [Major / Minor / Patch / Hotfix] |
| **Milestone** | [Milestone Name] |
| **Plan** | [Link to plan document] |

### 1.2 Highlights

> 3-5 bullet points summarizing the most important changes

- **[Category]:** [Brief description of major change]
- **[Category]:** [Brief description of major change]
- **[Category]:** [Brief description of major change]

### 1.3 Metrics Dashboard

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         RELEASE METRICS                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  FILES                            TESTS                                     │
│  ─────                            ─────                                     │
│  Total Changed:    [N]            Total Tests:     [N]                      │
│  New:              [N]            New Tests:       [N]                      │
│  Modified:         [N]            Passing:         [N]                      │
│  Deleted:          [N]            Coverage:        [X%]                     │
│                                                                             │
│  LINES OF CODE                    COMMITS                                   │
│  ─────────────                    ───────                                   │
│  Added:            [N]            Total:           [N]                      │
│  Removed:          [N]            Features:        [N]                      │
│  Net Change:       [+/-N]         Fixes:           [N]                      │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Breaking Changes

> ⚠️ REQUIRED if any breaking changes exist

### 2.1 Breaking Changes Summary

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ⚠️  BREAKING CHANGE: [Change Name]                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  WHAT CHANGED:                                                              │
│  [Description of the breaking change]                                       │
│                                                                             │
│  AFFECTED COMPONENTS:                                                       │
│  ├── [Component 1]                                                          │
│  ├── [Component 2]                                                          │
│  └── [Component 3]                                                          │
│                                                                             │
│  MIGRATION REQUIRED: Yes                                                    │
│                                                                             │
│  BEFORE:                                                                    │
│  ```csharp                                                                  │
│  // Old usage                                                               │
│  ```                                                                        │
│                                                                             │
│  AFTER:                                                                     │
│  ```csharp                                                                  │
│  // New usage                                                               │
│  ```                                                                        │
│                                                                             │
│  MIGRATION STEPS:                                                           │
│  1. [Step 1]                                                                │
│  2. [Step 2]                                                                │
│  3. [Step 3]                                                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Breaking Changes Index

| Change | Type | Severity | Migration |
|--------|------|----------|-----------|
| [Change] | API / Schema / Config | High/Medium | [Link to section] |

---

## 3. New Features

> ⚠️ REQUIRED

### 3.1 Feature: [Feature Name]

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ✨ NEW FEATURE: [Feature Name]                                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  DESCRIPTION:                                                               │
│  [What this feature does and why it was added]                              │
│                                                                             │
│  SPECIFICATION: SPEC-[DOMAIN]-[NNN]                                         │
│  PLAN: v[X.Y.Z][letter]                                                     │
│                                                                             │
│  KEY COMPONENTS:                                                            │
│  ├── [File/Class]: [Purpose]                                                │
│  ├── [File/Class]: [Purpose]                                                │
│  └── [File/Class]: [Purpose]                                                │
│                                                                             │
│  USAGE EXAMPLE:                                                             │
│  ```csharp                                                                  │
│  // Example code                                                            │
│  ```                                                                        │
│                                                                             │
│  RELATED TESTS:                                                             │
│  ├── [TestClass]: [N] tests                                                 │
│  └── [TestClass]: [N] tests                                                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Features Index

| Feature | Specification | Components | Tests |
|---------|---------------|------------|-------|
| [Feature] | SPEC-XXX-NNN | [N] files | [N] tests |

---

## 4. Improvements

> 📋 RECOMMENDED

### 4.1 Improvement: [Improvement Name]

| Attribute | Description |
|-----------|-------------|
| **Category** | Performance / UX / Code Quality / Documentation |
| **Description** | [What was improved] |
| **Benefit** | [Why this matters] |
| **Files Affected** | [N] |

### 4.2 Improvements Index

| Improvement | Category | Impact |
|-------------|----------|--------|
| [Improvement] | [Category] | [High/Medium/Low] |

---

## 5. Bug Fixes

> 📋 RECOMMENDED

### 5.1 Fix: [Bug Description]

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  🐛 BUG FIX: [Bug Title]                                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ISSUE: [Issue ID or description]                                           │
│  SEVERITY: [Critical / High / Medium / Low]                                 │
│                                                                             │
│  SYMPTOM:                                                                   │
│  [What users observed]                                                      │
│                                                                             │
│  ROOT CAUSE:                                                                │
│  [Technical explanation of why this happened]                               │
│                                                                             │
│  FIX:                                                                       │
│  [What was changed to fix it]                                               │
│                                                                             │
│  FILES CHANGED:                                                             │
│  ├── [File 1]: [What changed]                                               │
│  └── [File 2]: [What changed]                                               │
│                                                                             │
│  REGRESSION TEST:                                                           │
│  [Test added to prevent recurrence]                                         │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Bug Fixes Index

| Bug | Severity | Fix | Test |
|-----|----------|-----|------|
| [Bug] | [Severity] | [Brief fix description] | [Test name] |

---

## 6. New Files

> ⚠️ REQUIRED

### 6.1 New Files by Category

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         NEW FILES CREATED                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SERVICES                                                                   │
│  ────────                                                                   │
│  ├── src/Services/I[Feature]Service.cs                                      │
│  │   └── [Brief purpose]                                                    │
│  ├── src/Services/[Feature]Service.cs                                       │
│  │   └── [Brief purpose]                                                    │
│                                                                             │
│  MODELS                                                                     │
│  ──────                                                                     │
│  ├── src/Models/[Model]Model.cs                                             │
│  │   └── [Brief purpose]                                                    │
│                                                                             │
│  TESTS                                                                      │
│  ─────                                                                      │
│  ├── tests/Services/[Feature]ServiceTests.cs                                │
│  │   └── [N] test methods                                                   │
│                                                                             │
│  DOCUMENTATION                                                              │
│  ─────────────                                                              │
│  ├── docs/specs/[domain]/SPEC-XXX-NNN.md                                    │
│  │   └── [Specification for feature]                                        │
│                                                                             │
│  DATABASE                                                                   │
│  ────────                                                                   │
│  ├── migrations/V[N]__[Description].sql                                     │
│  │   └── [What this migration does]                                         │
│                                                                             │
│  TOTAL NEW FILES: [N]                                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6.2 New Files Table

| File Path | Category | Purpose | Lines |
|-----------|----------|---------|-------|
| `[path]` | [Category] | [Purpose] | [N] |

---

## 7. Modified Files

> ⚠️ REQUIRED

### 7.1 Modified Files by Category

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         MODIFIED FILES                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SERVICES                                                                   │
│  ────────                                                                   │
│  ├── src/Services/ExistingService.cs                                        │
│  │   ├── Added: [method/property]                                           │
│  │   └── Modified: [method/property]                                        │
│                                                                             │
│  MODELS                                                                     │
│  ──────                                                                     │
│  ├── src/Models/ExistingModel.cs                                            │
│  │   └── Added: [property]                                                  │
│                                                                             │
│  CONFIGURATION                                                              │
│  ─────────────                                                              │
│  ├── Program.cs                                                             │
│  │   └── Added: DI registration for [services]                              │
│                                                                             │
│  TOTAL MODIFIED FILES: [N]                                                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Modified Files Table

| File Path | Changes | Lines Added | Lines Removed |
|-----------|---------|-------------|---------------|
| `[path]` | [Summary] | +[N] | -[N] |

---

## 8. Deleted Files

> 📎 OPTIONAL - Include if files were deleted

### 8.1 Deleted Files

| File Path | Reason | Replacement |
|-----------|--------|-------------|
| `[path]` | [Why deleted] | [What replaces it, if any] |

---

## 9. Implementation Details

> ⚠️ REQUIRED

### 9.1 Architecture Changes

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    ARCHITECTURE DIAGRAM                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  [ASCII diagram showing component relationships]                            │
│                                                                             │
│  ┌─────────────────┐         ┌─────────────────┐                           │
│  │  New Component  │ ──────► │ Existing System │                           │
│  │  [Name]         │         │ [Name]          │                           │
│  └─────────────────┘         └─────────────────┘                           │
│          │                                                                  │
│          ▼                                                                  │
│  ┌─────────────────┐                                                       │
│  │  Database       │                                                       │
│  │  [New tables]   │                                                       │
│  └─────────────────┘                                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Key Implementation Decisions

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| [Decision] | [Why this choice] | [What else was considered] |

### 9.3 Code Samples

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// KEY IMPLEMENTATION: [Description]
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// [Description of this code sample]
/// </summary>
public class [ClassName]
{
    // [Key code excerpt]
}
```

---

## 10. Logging Matrix

> ⚠️ REQUIRED

### 10.1 Logging Implementation

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         LOGGING MATRIX                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SERVICE: [ServiceName]                                                     │
│  ════════════════════════                                                   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Method                 │ Entry │ Exit │ Error │ Debug │ Metrics   │   │
│  ├────────────────────────┼───────┼──────┼───────┼───────┼───────────┤   │
│  │ MethodName1            │  ✓    │  ✓   │  ✓    │  ○    │    ✓      │   │
│  │ MethodName2            │  ✓    │  ✓   │  ✓    │  ✓    │    ○      │   │
│  │ MethodName3            │  ✓    │  ✓   │  ✓    │  ○    │    ✓      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  LEGEND:                                                                    │
│  ✓ = Implemented                                                            │
│  ○ = Not Required                                                           │
│  ✗ = Missing (needs remediation)                                            │
│                                                                             │
│  LOG LEVELS USED:                                                           │
│  ├── Information: Method entry/exit, state changes                          │
│  ├── Warning: Degraded conditions, fallback paths                           │
│  ├── Error: Exceptions, failures                                            │
│  └── Debug: Detailed diagnostic information                                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 10.2 Logging Compliance

| Service | Entry/Exit | Errors | Debug | Compliant |
|---------|------------|--------|-------|-----------|
| [Service] | ✓/✗ | ✓/✗ | ✓/○ | Yes/No |

---

## 11. Test Coverage

> ⚠️ REQUIRED

### 11.1 Test Summary

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         TEST COVERAGE REPORT                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  OVERALL COVERAGE: [XX%]                                                    │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Category          │ Tests │ Passing │ Failing │ Skipped │ Coverage │   │
│  ├───────────────────┼───────┼─────────┼─────────┼─────────┼──────────┤   │
│  │ Unit Tests        │  [N]  │   [N]   │   [N]   │   [N]   │   [X%]   │   │
│  │ Integration Tests │  [N]  │   [N]   │   [N]   │   [N]   │   [X%]   │   │
│  │ E2E Tests         │  [N]  │   [N]   │   [N]   │   [N]   │   [X%]   │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  TOTAL TESTS: [N]                                                           │
│  NEW TESTS THIS RELEASE: [N]                                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 11.2 Test Files

| Test File | Tests | Type | Coverage |
|-----------|-------|------|----------|
| `[TestFile].cs` | [N] | Unit/Integration | [X%] |

### 11.3 New Tests

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// NEW TESTS: [TestClass]
// ═══════════════════════════════════════════════════════════════════════════

// [List of new test method signatures]
[Fact] public async Task MethodName_Scenario_ExpectedResult()
[Fact] public async Task MethodName_Scenario_ExpectedResult()
[Theory] public async Task MethodName_WithData_ExpectedResult(params)
```

---

## 12. DI Registration

> ⚠️ REQUIRED

### 12.1 New Registrations

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// NEW DI REGISTRATIONS (Program.cs or App.axaml.cs)
// ═══════════════════════════════════════════════════════════════════════════

// Services
services.AddSingleton<I[Feature]Service, [Feature]Service>();
services.AddScoped<I[Feature]Repository, [Feature]Repository>();

// Configuration
services.Configure<[Feature]Settings>(configuration.GetSection("[Feature]Settings"));
```

### 12.2 DI Registration Table

| Interface | Implementation | Lifetime | Notes |
|-----------|----------------|----------|-------|
| `I[Feature]Service` | `[Feature]Service` | Singleton | [Notes] |

---

## 13. Database Changes

> 📎 OPTIONAL - Include if database changes exist

### 13.1 Migration Summary

| Migration | Description | Reversible |
|-----------|-------------|------------|
| V[N]__[Name].sql | [What it does] | Yes/No |

### 13.2 Schema Changes

```sql
-- ═══════════════════════════════════════════════════════════════════════════
-- NEW TABLES
-- ═══════════════════════════════════════════════════════════════════════════

CREATE TABLE [table_name] (
    id              UUID            PRIMARY KEY,
    -- [columns]
);

-- ═══════════════════════════════════════════════════════════════════════════
-- MODIFIED TABLES
-- ═══════════════════════════════════════════════════════════════════════════

ALTER TABLE [table_name]
    ADD COLUMN [column_name] [TYPE];
```

---

## 14. Configuration Changes

> 📎 OPTIONAL - Include if configuration changes exist

### 14.1 New Configuration

```json
{
  "[Feature]Settings": {
    "PropertyName": "value",
    "AnotherProperty": true
  }
}
```

### 14.2 Configuration Schema

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `[Feature]Settings:PropertyName` | string | "value" | [What it controls] |

---

## 15. Known Issues

> 📎 OPTIONAL

### 15.1 Known Issues

| Issue | Description | Workaround | Fix Planned |
|-------|-------------|------------|-------------|
| [Issue] | [Description] | [How to work around] | v[X.Y.Z] |

---

## 16. Upgrade Guide

> 📋 RECOMMENDED (especially for breaking changes)

### 16.1 Pre-Upgrade Checklist

- [ ] Backup database
- [ ] Review breaking changes
- [ ] Update configuration files
- [ ] [Other prerequisite]

### 16.2 Upgrade Steps

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         UPGRADE PROCEDURE                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. PREPARATION                                                             │
│     ├── [ ] Stop running services                                           │
│     ├── [ ] Backup database                                                 │
│     └── [ ] Document current configuration                                  │
│                                                                             │
│  2. CODE UPDATE                                                             │
│     ├── [ ] Pull latest code / update packages                              │
│     ├── [ ] Review and apply configuration changes                          │
│     └── [ ] Run migrations                                                  │
│                                                                             │
│  3. VALIDATION                                                              │
│     ├── [ ] Run test suite                                                  │
│     ├── [ ] Verify critical functionality                                   │
│     └── [ ] Check logs for errors                                           │
│                                                                             │
│  4. POST-UPGRADE                                                            │
│     ├── [ ] Monitor for issues                                              │
│     ├── [ ] Update documentation                                            │
│     └── [ ] Notify stakeholders                                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 16.3 Rollback Procedure

[Steps to rollback to previous version if needed]

---

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~[TestClassName]"
```

---

## Directory Structure

```
project/
├── src/
│   ├── Services/
│   │   ├── I[Feature]Service.cs       # [New]
│   │   └── [Feature]Service.cs        # [New]
│   ├── Models/
│   │   └── [Model].cs                 # [New]
│   └── Program.cs                     # [Modified]
├── tests/
│   └── Services/
│       └── [Feature]ServiceTests.cs   # [New]
├── docs/
│   └── specs/
│       └── SPEC-XXX-NNN.md            # [New]
└── migrations/
    └── V[N]__[Description].sql        # [New]
```

---

<!--
╔══════════════════════════════════════════════════════════════════════════════╗
║                              END OF TEMPLATE                                  ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  GENERATION CHECKLIST:                                                        ║
║  ────────────────────────────────────────────────────────────────────────────║
║  □ All new files documented                                                   ║
║  □ All modified files documented                                              ║
║  □ Breaking changes highlighted                                               ║
║  □ Test coverage reported                                                     ║
║  □ Logging matrix completed                                                   ║
║  □ DI registrations documented                                                ║
║  □ Database changes documented (if any)                                       ║
║  □ Upgrade guide provided (if breaking changes)                               ║
║  □ Metrics accurate                                                           ║
╚══════════════════════════════════════════════════════════════════════════════╝
-->
