---
id: PLAN-GH-SETUP-001
title: "GitHub Project Setup Implementation Plan"
version: 1.0
status: draft
last-updated: 2025-12-26
reference: docs/00-project/github-project-setup.md
---

# GitHub Project Setup Implementation Plan

> *"Configuration without ritual is just settings. Configuration with ritual is workflow."*

This document provides a step-by-step task list for implementing the GitHub Project configuration defined in `github-project-setup.md`. Each section contains explicit instructions, requirements, and definitions of done.

---

## Phase 1: Project Creation

### Task 1.1: Create the GitHub Project
**Instructions**:
1.  Navigate to the GitHub Repository: `southpawriter02/rune-rust`.
2.  Go to **Projects** tab > **New project**.
3.  Select **Board** template (or start blank).
4.  Name it: `Rune & Rust Master Board`.
5.  Set Visibility: **Private** (recommended for solo dev).

**Requirements**:
- [ ] Project name is exactly `Rune & Rust Master Board`.
- [ ] Project is linked to the `rune-rust` repository.

**Definition of Done**:
- [ ] Project URL is accessible: `https://github.com/users/southpawriter02/projects/N`.

---

## Phase 2: Custom Fields

### Task 2.1: Create "Status" Field
**Instructions**:
1.  In Project Settings > **Fields** > **New Field**.
2.  **Name**: `Status`.
3.  **Type**: `Single Select`.
4.  **Options** (create in order):
    | Option Name | Color (Suggested) |
    |-------------|-------------------|
    | Backlog | Gray |
    | Spec Review | Blue |
    | Ready | Green |
    | In Progress | Yellow |
    | QA / Verify | Purple |
    | Done | Dark Green |
5.  Set `Status` as the **default grouping field** for Board views.

**Definition of Done**:
- [ ] All 6 status options exist.
- [ ] Default status for new items is `Backlog`.

---

### Task 2.2: Create "Priority" Field
**Instructions**:
1.  **New Field** > Name: `Priority` > Type: `Single Select`.
2.  **Options**:
    | Option Name | Color |
    |-------------|-------|
    | P0 (Critical) | Red |
    | P1 (High) | Orange |
    | P2 (Normal) | Blue |
    | P3 (Someday) | Gray |

### Priority Definitions

| Priority | Definition | When to Use | Examples |
|----------|------------|-------------|----------|
| **P0 (Critical)** | Work that **blocks all other progress** or represents a **live defect**. Must be addressed immediately—drop everything else. | Broken builds, data loss bugs, security vulnerabilities, showstopper crashes. | "Save system corrupts data on load", "Build fails on main branch", "Crash on startup". |
| **P1 (High)** | Important work that should be completed **this sprint**. Represents core functionality or significant user-facing improvements. | Key features for the current milestone, bugs affecting core gameplay, architectural prerequisites. | "Implement combat turn order", "Fix stamina not regenerating", "Add persistence layer". |
| **P2 (Normal)** | Standard priority work. Should be completed **within 2-3 sprints**. The bulk of planned feature work. | Enhancements, non-blocking bugs, documentation, polish items. | "Add new status effect: Bleeding", "Improve error messages", "Write spec for Runeforging". |
| **P3 (Someday)** | Nice-to-have ideas with **no committed timeline**. May never be implemented. Used for parking interesting ideas. | Wishlist features, speculative improvements, low-impact polish. | "Add particle effects to abilities", "Support controller input", "Investigate performance optimization". |

**Definition of Done**:
- [ ] All 4 priority options exist with correct colors.

---

### Task 2.3: Create "Size" Field
**Instructions**:
1.  **New Field** > Name: `Size` > Type: `Single Select`.
2.  **Options**:
    | Option Name | Meaning |
    |-------------|---------|
    | XS | Chore (<1h) |
    | S | Small (1-2h) |
    | M | Medium (1 day) |
    | L | Large (2-3 days) |
    | XL | Extra Large (1 week) |

### Size Definitions

| Size | Time Estimate | Scope | Examples |
|------|---------------|-------|----------|
| **XS** | < 1 hour | A trivial change with no design required. Often a single-line fix or config tweak. | "Fix typo in tooltip", "Update README version", "Add missing log statement". |
| **S** | 1–2 hours | A contained change affecting a single function or file. Minimal testing required. | "Add validation to input field", "Implement simple utility function", "Write unit test for existing method". |
| **M** | 1 day | A meaningful feature or bug fix touching multiple files. Requires focused attention and testing. | "Add new status effect", "Implement save game UI", "Fix room transition logic". |
| **L** | 2–3 days | A significant feature requiring design, multiple components, and thorough testing. May need a Spec Review first. | "Implement combat turn system", "Add inventory persistence", "Refactor damage calculation engine". |
| **XL** | ~1 week | A major initiative that should probably be broken into smaller issues. Use sparingly—consider decomposition. | "Implement full crafting system", "Major architectural refactor", "New specialization with 5+ abilities". |

> [!TIP]
> If you estimate **XL**, ask yourself: "Can I break this into 2-3 smaller L or M tasks?" XL items are harder to track and more likely to stall.

**Definition of Done**:
- [ ] All 5 size options exist.

---

### Task 2.4: Create "Type" Field
**Instructions**:
1.  **New Field** > Name: `Type` > Type: `Single Select`.
2.  **Options**: `Feature`, `Bug`, `Refactor`, `Docs`, `Lore`.

### Type Definitions

| Type | Definition | When to Use | Examples |
|------|------------|-------------|----------|
| **Feature** | New functionality that adds capabilities to the game. Delivers user-facing value. | Implementing new systems, abilities, UI components, or game mechanics. | "Add Runeforging crafting system", "Implement Bleeding status effect", "Create inventory screen". |
| **Bug** | A defect where existing functionality does not work as intended. Something is *broken*. | Crashes, incorrect calculations, UI glitches, data corruption, unexpected behavior. | "Stamina doesn't regenerate after combat", "Save file corrupts on special characters", "Enemy AI freezes on turn 3". |
| **Refactor** | Internal code improvements that do not change external behavior. Reduces technical debt. | Restructuring code, improving performance, consolidating duplicated logic, migrating patterns. | "Extract damage calculation into service", "Migrate JSON saves to binary", "Consolidate entity interfaces". |
| **Docs** | Technical documentation, specs, or project management artifacts. | Writing specs, updating READMEs, creating implementation plans, changelog entries. | "Write Runeforging spec", "Update architecture diagram", "Add API documentation for SaveService". |
| **Lore** | World-building, narrative content, flavor text, or creative writing. Does not involve code. | Faction histories, item descriptions, dialogue, setting documentation. | "Write Souring Mires history", "Create Ash-Walker faction lore", "Draft Berserkr ability flavor text". |

**Definition of Done**:
- [ ] All 5 type options exist.

---

### Task 2.5: Create "Feature Group" Field
**Instructions**:
1.  **New Field** > Name: `Feature Group` > Type: `Text`.
2.  *(No predefined options—free-form text like "Combat", "Persistence")*.

**Definition of Done**:
- [ ] Field appears on Issue details panel.

---

### Task 2.6: Create "Target Ver" Iteration Field
**Instructions**:
1.  **New Field** > Name: `Target Ver` > Type: `Iteration`.
2.  Configure iteration length: **2 weeks**.
3.  Create initial iterations (e.g., `v0.4 Sprint 1`, `v0.4 Sprint 2`).

**Definition of Done**:
- [ ] At least 2 future iterations are defined.

---

## Phase 3: Views

### Task 3.1: Create "Roadmap" View
**Instructions**:
1.  **New View** > Name: `Roadmap` > Layout: **Timeline**.
2.  **Group By**: `Feature Group` (or `Type`).
3.  **Filter**: `Status` is not `Done`.
4.  *(Optional)* Configure date fields if using start/end dates.

**Definition of Done**:
- [ ] View shows items grouped by Feature Group.
- [ ] Completed items are hidden.

---

### Task 3.2: Create "The Board" View
**Instructions**:
1.  **New View** > Name: `The Board` > Layout: **Board**.
2.  **Group By**: `Status`.
3.  **Filter**: `Target Ver` = `@current`.
4.  **Sort**: `Priority` (Descending).

**Definition of Done**:
- [ ] Columns are: Backlog, Spec Review, Ready, In Progress, QA / Verify, Done.
- [ ] Default view shows only current iteration items.

---

### Task 3.3: Create "Spec Review" View
**Instructions**:
1.  **New View** > Name: `Spec Review` > Layout: **Table**.
2.  **Filter**: `Status` == `Spec Review`.
3.  **Columns**: Title, Priority, Size, Assignee.

**Definition of Done**:
- [ ] Only Spec Review items are visible.

---

### Task 3.4: Create "Backlog Triage" View
**Instructions**:
1.  **New View** > Name: `Backlog Triage` > Layout: **Table**.
2.  **Filter**: `Status` == `Backlog`.
3.  **Sort**: `Priority` (Desc) > `Type`.

**Definition of Done**:
- [ ] Only Backlog items are visible.
- [ ] Sorted by Priority.

---

## Phase 4: Automation

### Task 4.1: Configure Auto-Add Rule
**Instructions**:
1.  Project Settings > **Workflows** > **Auto-add to project**.
2.  **Trigger**: When an Issue is **created** in `southpawriter02/rune-rust`.
3.  **Action**: Add to `Rune & Rust Master Board`.

**Definition of Done**:
- [ ] New Issues appear in Project automatically.

---

### Task 4.2: Configure Auto-Archive Rule
**Instructions**:
1.  Project Settings > **Workflows** > **Auto-archive items**.
2.  **Trigger**: Item is in `Done` for **14 days**.
3.  **Action**: Archive.

**Definition of Done**:
- [ ] Old Done items are auto-archived.

---

### Task 4.3: Configure PR Merge Rule (Optional)
**Instructions**:
1.  Project Settings > **Workflows** > **Item closed**.
2.  **Trigger**: Linked PR is **merged**.
3.  **Action**: Set `Status` to `QA / Verify`.

**Definition of Done**:
- [ ] Merged PRs move linked Issues to QA.

---

## Phase 5: Issue Templates

### Task 5.1: Create Feature Request Template
**Instructions**:
1.  In the repository, create `.github/ISSUE_TEMPLATE/feature_request.md`.
2.  Paste template from `github-project-setup.md` Section 4, Template A.

**Definition of Done**:
- [ ] "Feature Request" option appears when creating new Issue.

---

### Task 5.2: Create Bug Report Template
**Instructions**:
1.  Create `.github/ISSUE_TEMPLATE/bug_report.md`.
2.  Paste template from `github-project-setup.md` Section 4, Template B.

**Definition of Done**:
- [ ] "Bug Report" option appears when creating new Issue.

---

## Phase 6: Verification

### Task 6.1: End-to-End Test
**Instructions**:
1.  Create a **test Issue** titled `[TEST] Project Setup Verification`.
2.  Verify it auto-adds to the Project.
3.  Manually set all custom fields.
4.  Move it through each Status column.
5.  Close the Issue and verify it moves to Done.
6.  Wait (or manually archive) to verify archival.
7.  Delete the test Issue.

**Definition of Done**:
- [ ] All fields are editable.
- [ ] All views display the item correctly.
- [ ] Automations fire as expected.

---

## Summary Checklist

| Phase | Tasks | Status |
|-------|-------|--------|
| Phase 1: Project Creation | 1 | [ ] |
| Phase 2: Custom Fields | 6 | [ ] |
| Phase 3: Views | 4 | [ ] |
| Phase 4: Automation | 3 | [ ] |
| Phase 5: Issue Templates | 2 | [ ] |
| Phase 6: Verification | 1 | [ ] |
| **Total** | **17** | |
