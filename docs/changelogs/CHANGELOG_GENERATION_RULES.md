# Agent Instructions: Generating High-Fidelity Changelogs

**Goal:** Generate "Deep-Dive" technical changelogs that serve as both a record of release and a technical specification of the implemented state. The output must match the level of detail found in `v0.0.2.md` (single-part) or `v0.3.10.md` (multi-part).

---

## 1. General Formatting Rules

### File Naming & Location
- **Filename:** `vX.Y.Z.md` (consolidated version file)
- **Location:** `docs/changelogs/v0.X.x/` folder (grouped by minor version)

### Title Header Formats

**Single-Part Release:**
```markdown
# Changelog: vX.Y.Z - <Thematic Title>

**Release Date:** YYYY-MM-DD
**Total Tests:** N (M new tests added)
```

**Multi-Part Release:**
```markdown
# Changelog: vX.Y.Z - <Thematic Title>

**Versions:** vX.Y.Za through vX.Y.Zc
**Release Dates:** YYYY-MM-DD (all parts) or YYYY-MM-DD through YYYY-MM-DD
```

### General Formatting
- **Separators:** Use horizontal rules `---` between major sections
- **Language:** Technical, precise, past tense for implemented features
- **Headers:** Use `##` for major sections, `###` for subsections

---

## 2. Table of Contents (Required)

**Placement:** Immediately after the metadata block, before the first `---` separator.

### Single-Part Format
Flat list of all `##` level sections:

```markdown
## Table of Contents

- [Summary](#summary)
- [New Files Created](#new-files-created)
- [Files Modified](#files-modified)
- [Code Implementation Details](#code-implementation-details)
- [Logging Matrix](#logging-matrix)
- [Test Coverage](#test-coverage)
- [DI Registration](#di-registration)
- [Verification Results](#verification-results)
- [Directory Structure After vX.Y.Z](#directory-structure-after-vxyz)
- [Running Tests](#running-tests)
- [Next Steps](#next-steps)
```

### Multi-Part Format
Hierarchical structure with Parts as top-level entries and subsections nested:

```markdown
## Table of Contents

- [Overview](#overview)
- [Part A: The Subtitle](#part-a-the-subtitle)
  - [Summary](#summary)
  - [New Files Created](#new-files-created)
  - [Files Modified](#files-modified)
  - [Code Implementation Details](#code-implementation-details)
  - [Logging Matrix](#logging-matrix)
  - [Test Coverage](#test-coverage)
- [Part B: The Other Subtitle](#part-b-the-other-subtitle)
  - [Summary](#summary-1)
  - [New Files Created](#new-files-created-1)
  - ...
- [Part C: The Final Subtitle](#part-c-the-final-subtitle)
  - [Summary](#summary-2)
  - ...
- [Related Documentation](#related-documentation)
- [Credits](#credits)
```

### Anchor Link Rules (GitHub-Flavored Markdown)
- Convert header text to lowercase
- Replace spaces with hyphens (`-`)
- Remove punctuation (except hyphens)
- Remove emoji characters
- For duplicate headers: append `-1`, `-2`, etc.

**Examples:**
- `## Part A: The Preferences (Settings Engine)` → `#part-a-the-preferences-settings-engine`
- `### Summary` (first occurrence) → `#summary`
- `### Summary` (second occurrence) → `#summary-1`
- `## 🎯 Executive Summary` → `#-executive-summary`

---

## 3. Multi-Part Release Structure

### When to Use
Use multi-part structure when a release spans multiple sub-versions (a, b, c, etc.) that were developed and released as a coordinated trilogy or sequence.

### Structure

```markdown
## Overview

Brief paragraph summarizing all Parts and their combined impact.
List key achievements across all Parts.

---

## Part A: The Subtitle

[Standard sections repeated here]

---

## Part B: The Subtitle

[Standard sections repeated here]

---

## Part C: The Subtitle

[Standard sections repeated here]
```

### Part Header Format
- Use `## Part X: <Subtitle>` where X is A, B, C, etc.
- Subtitles should be thematic (e.g., "The Foundation", "The Logic", "The Interface")

### Section Repetition
Each Part contains its own complete set of standard sections:
- Summary
- New Files Created
- Files Modified
- Code Implementation Details
- Logging Matrix
- Test Coverage
- (Optional: DI Registration, Verification, etc.)

---

## 4. Required Sections (In Order)

### A. Summary / Overview
- **Single-part:** Single paragraph summarizing the release's architectural focus
- **Multi-part:** Overview section summarizing all Parts, followed by individual Part summaries
- Mention specific layers touched (e.g., Core, Engine, Terminal, Test)
- Mention key patterns introduced (e.g., Dependency Injection, Factory Pattern)

### B. New Files Created
- **Grouping:** Group by Project/Layer (e.g., `### Core Layer`, `### Engine Layer`)
- **Format:** Markdown Table
- **Columns:** `File` (relative path), `Purpose`

```markdown
| File | Purpose |
|------|---------|
| `RuneAndRust.Core/Enums/MyEnum.cs` | Defines X |
```

### C. Files Modified
- **Format:** Markdown Table
- **Columns:** `File`, `Change`

```markdown
| File | Change |
|------|--------|
| `Program.cs` | Registered `IMyService` |
```

### D. Code Implementation Details
- **Content:** This is the "meat" of the changelog. Do not just list files; explain *logic*.
- **For Enums:** Provide the full code snippet if concise, or values list with comments.
- **For Services/Classes:**
  - List key methods/contract signatures
  - Bulleted list of *behaviors* (e.g., "Clamps value between 1 and 10")
  - Document specific constants or thresholds (e.g., "Success threshold: 8+")

### E. Logging Matrix (CRITICAL)
- **Purpose:** Explicit documentation of all structured log events introduced
- **Grouping:** Group by Service or formatting context
- **Format:** Markdown Table
- **Columns:** `Event`, `Level`, `Template`
  - *Event:* Human readable description (e.g., "Method entry")
  - *Level:* Trace, Debug, Info, Warning, Error
  - *Template:* The raw Serilog message template

### F. Test Coverage
- **Summary Block:** Code block showing Total, Passed, Failed, Duration
- **Complete Test Inventory:**
  - Group by Test Class (e.g., `#### DiceServiceTests (28 tests)`)
  - **Format:** Markdown Table with `Test Name`, `Description`
  - **Note:** List *every single test* added, not just a sample

### G. DI Registration
- Show the code block added to `Program.cs` or startup configuration

### H. Verification Results
- **Build:** Console output snippet showing "Build succeeded"
- **Tests:** Console output snippet showing "Passed! ..."

### I. Directory Structure After Release
- **Format:** Code block with directory tree
- **Annotations:** `[NEW]` for new files, `[MODIFIED]` for changed files
- Only show relevant directories (avoid `bin`/`obj`)

### J. Running Tests
- Provide `dotnet test` commands with relevant filters

### K. Next Steps
- Bulleted list of what comes in `vX.Y.Z+1`

---

## 5. Archival Process

### When to Archive
After consolidating individual changelogs (e.g., `v0.3.10a.md`, `v0.3.10b.md`, `v0.3.10c.md`) into a single version file (`v0.3.10.md`), move the originals to the archive.

### Archive Location
`docs/changelogs/archive/`

### Callout Format
Add a GitHub-style blockquote callout at the top of each archived file:

```markdown
> **Archived** - This changelog has been consolidated. See the complete version at [vX.Y.Z](../v0.X.x/vX.Y.Z.md).
```

### Retention Policy
All original files are preserved in the archive for reference and historical accuracy.

---

## 6. Style & Voice

- **Voice:** Use standard Professional Engineering English (not the "Jötun-Reader" voice)
- **Precision:** Do not say "Added some tests." Say "Added 12 tests covering boundary conditions and null checks."
- **Completeness:**
  - If a file was created, it **must** appear in the file list
  - If a log statement exists in the code, it **must** appear in the Logging Matrix

---

## 7. Example Templates

### Single-Part Template

```markdown
# Changelog: v0.0.X - The Feature Name

**Release Date:** 2025-MM-DD
**Total Tests:** N (M new tests added)

## Table of Contents

- [Summary](#summary)
- [New Files Created](#new-files-created)
- [Files Modified](#files-modified)
- [Code Implementation Details](#code-implementation-details)
- [Logging Matrix](#logging-matrix)
- [Test Coverage](#test-coverage)
- [DI Registration](#di-registration)
- [Verification Results](#verification-results)
- [Directory Structure After v0.0.X](#directory-structure-after-v00x)
- [Running Tests](#running-tests)
- [Next Steps](#next-steps)

---

## Summary

Brief architectural summary...

---

## New Files Created

### Layer Name

| File | Purpose |
|------|---------|
| `Path/To/File.cs` | Description |

---

## Code Implementation Details

### ServiceName

- Behavior details
- Constants and thresholds

...
```

### Multi-Part Template

```markdown
# Changelog: vX.Y.Z - The Trilogy Title

**Versions:** vX.Y.Za through vX.Y.Zc
**Release Dates:** 2025-MM-DD (all parts)

## Table of Contents

- [Overview](#overview)
- [Part A: The First Chapter](#part-a-the-first-chapter)
  - [Summary](#summary)
  - [New Files Created](#new-files-created)
  - [Files Modified](#files-modified)
  - [Code Implementation Details](#code-implementation-details)
  - [Test Coverage](#test-coverage)
- [Part B: The Second Chapter](#part-b-the-second-chapter)
  - [Summary](#summary-1)
  - [New Files Created](#new-files-created-1)
  - ...
- [Part C: The Final Chapter](#part-c-the-final-chapter)
  - [Summary](#summary-2)
  - ...
- [Credits](#credits)

---

## Overview

Version X.Y.Z represents a comprehensive three-part release that...

**Part A** establishes...
**Part B** implements...
**Part C** completes...

Combined test count: N tests (M new across all parts)

---

## Part A: The First Chapter

**Release Date:** 2025-MM-DD

### Summary

Part A introduces...

---

### New Files Created

#### Core Layer

| File | Purpose |
|------|---------|
| ... | ... |

---

### Code Implementation Details

...

---

## Part B: The Second Chapter

**Release Date:** 2025-MM-DD

### Summary

Part B builds upon Part A by...

[Continue with standard sections]

---

## Part C: The Final Chapter

**Release Date:** 2025-MM-DD

### Summary

Part C completes the trilogy by...

[Continue with standard sections]

---

## Credits

**Primary Developer:** The Architect (Claude)
**Test Coverage:** X%+ across all new services
```

---

## 8. Reference Examples

| Type | Reference File | Description |
|------|----------------|-------------|
| Single-Part | [v0.0.2.md](v0.0.x/v0.0.2.md) | Clean single-release changelog with all required sections |
| Multi-Part | [v0.3.10.md](v0.3.x/v0.3.10.md) | Three-part consolidated changelog with TOC and Part structure |
