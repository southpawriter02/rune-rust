---
id: WORKFLOW-GH-001
title: "GitHub Project Configuration & Workflow"
version: 1.0
status: draft
last-updated: 2025-12-26
---

# GitHub Project Configuration & Workflow

> *"A roadmap is not a list of features. It is a statement of intent."*

This document details the configuration and operational rituals for using **GitHub Projects** to track the **Rune & Rust** development lifecycle. It translates the high-level workflow into concrete fields, views, and automations.

---

## 1. Project Structure

We utilize a **Single Project** ("Rune & Rust Master Board") to track all work across repositories.

### The "Item" Taxonomy
Everything in the Project must be an **Issue**.
*   **Pull Requests** are *linked* to Issues but should not clutter the primary board views unless specifically tracking review status.
*   **Drafts** are allowed for quick notes but must be converted to Issues before "Implementation" status.

---

## 2. Configuration: Custom Fields

Create these specific custom fields in the GitHub Project settings to support our workflow.

| Field Name | Type | Options / Configuration | Purpose |
|------------|------|-------------------------|---------|
| **Status** | `Single Select` | *(See Detailed Definitions below)* | Core workflow state. |
| **Priority** | `Single Select` | **P0 (Critical)**, **P1 (High)**, **P2 (Normal)**, **P3 (Someday)** | Triage sorting. |
| **Size** | `Single Select` | **XS** (Chore), **S** (1-2h), **M** (1d), **L** (2-3d), **XL** (1w) | Estimation & Velocity. |
| **Type** | `Single Select` | **Feature**, **Bug**, **Refactor**, **Docs**, **Lore** | Categorization. |
| **Feature Group** | `Text` | *(Examples: Combat, Inventory, Persistence)* | Grouping related tasks without rigorous Milestones. |
| **Target Ver** | `Iteration` | *(Linked to 2-week date ranges)* | Sprint planning. |

### Status Definitions

| Status | Definition | Entry Criteria | Exit Criteria |
|--------|------------|----------------|---------------|
| **Backlog** | Ideas, unrefined requests, and future "wishlist" items. The holding pen for work that is identified but not yet prioritized. | Item created. | Triage selects it for upcoming work. |
| **Spec Review** | Work that requires architectural design or lore research before coding can begin. Assigned to `Claude` or `NotebookLM`. | Selected from Backlog; complexity > S. | **Spec Artifact** created and committed to `docs/specs/`. |
| **Ready** | Work that is fully defined, unblocked, and ready for a developer to pick up immediately. | Spec approved; tasks clear; dependencies met. | Developer assigns self and moves to 'In Progress'. |
| **In Progress** | Active work. Code is being written, or lore is being drafted. | Developer starts work. | PR opened; content drafted. |
| **QA / Verify** | Work that is "code complete" but requires verification against the Spec/Requirements. Linked PRs are usually merged or in final review. | PR merged; feature available in build. | Verification steps passed; Changelog updated. |
| **Done** | Completed units of work. No further action required. | QA passed; all tasks checked off. | (Archived after 14 days). |

---

## 3. Configuration: Views

Configure these tabs in the Project UI.

### View 1: "Roadmap" (Timeline Layout)
*   **Layout**: Timeline.
*   **Group By**: `Feature Group` or `Type`.
*   **Filter**: `Status` is not **Done**.
*   **Date Fields**: Start/End date (optional) or `Target Ver`.
*   **Purpose**: Strategic view. "What major systems are coming next?"

### View 2: "The Board" (Kanban Layout)
*   **Layout**: Board.
*   **Group By**: `Status`.
*   **Filter**: `Target Ver` = `@current` (Current Iteration).
*   **Sort**: `Priority` (Desc).
*   **Purpose**: Daily execution. The "Active State".

### View 3: "Spec Review" (Table Layout)
*   **Layout**: Table.
*   **Filter**: `Status` == **Spec Review**.
*   **Purpose**: Focus area for `Claude` architecture tasks. Before moving to **Ready**, an item *must* have a linked Spec Artifact.

### View 4: "Backlog Triage" (Table Layout)
*   **Layout**: Table.
*   **Filter**: `Status` == **Backlog**.
*   **Sort**: `Priority` > `Type`.
*   **Purpose**: Weekly planning. Pull items from here into "The Board".

---

## 4. Issue Templates

Standardize input to ensure tools (and AI) have what they need.

### Template A: Feature Request
```markdown
## 💡 Context
*(Link to Notion Ideation page or NotebookLM Brief)*

## 📐 Requirements
- [ ] Requirement 1
- [ ] Requirement 2

## 🔗 Dependencies
- Relies on: #123
- Blocks: #456

## 📚 Documentation
- Spec: `docs/04-systems/...`
- Plan: `docs/plans/v0.X/...`
```

### Template B: Bug Report
```markdown
## 🐛 Description
*(What happened?)*

## 🔍 Context
- **Version**: v0.X.Y
- **Stack Trace**:
  ```
  (Paste here)
  ```

## 🛠 Reproduction
1. Go to...
2. Click...
```

---

## 5. Workflow Rituals

### Step 1: Ingestion (The "Inbox")
*   **Trigger**: A new tasks arises from Notion brainstorming or a bug discovery.
*   **Action**: Create Issue -> Add to Project -> Status: **Backlog**.
*   **AI Metadata**: Assign `Feature Group` and preliminary `Size`.

### Step 2: Refining (Backlog -> Ready)
*   **Trigger**: Weekly Planning or "Low Work" signal.
*   **Action**:
    1.  Select top **P0/P1** items from Backlog.
    2.  **Spec Check**: Does this need a design doc?
        *   *Yes*: Set Status to **Spec Review**. Assign to `Claude`.
        *   *No (Small fix)*: Move to **Ready**.
    3.  **Spec Review Loop**: Once Spec is committed (`docs/specs/...`), move to **Ready**.

### Step 3: Execution (Ready -> In Progress)
*   **Trigger**: Dev starts work.
*   **Action**:
    1.  Assign self.
    2.  Move to **In Progress**.
    3.  Create Branch (via GitLens): `feat/123-feature-name`.

### Step 4: Verification (In Progress -> QA)
*   **Trigger**: Code committed. PR opened.
*   **Action**:
    1.  Move to **QA / Verify**.
    2.  Execute "Verification Plan" from the Implementation Plan artifact.
    3.  (Optional) Ask `Gemini` or `ChatGPT` to review the PR diff for silly errors.

### Step 5: Completion (QA -> Done)
*   **Trigger**: PR Merged.
*   **Action**:
    1.  Close Issue (Auto-moves to **Done** via Workflow automation).
    2.  Update Changelog.

---

## 6. Automation Rules (Project Settings)

Reduce manual clicking.

1.  **Auto-Add**:
    *   *When*: Issue created in `southpawriter02/rune-rust`.
    *   *Action*: Add to Project "Rune & Rust Master Board".
2.  **Auto-Archive**:
    *   *When*: Item stays in **Done** for 14 days.
    *   *Action*: Archive.
3.  **PR Sync**:
    *   *When*: Linked PR is merged.
    *   *Action*: Move Issue to **QA / Verify** (or Done, depending on preference). *Recommendation: Move to QA so you remember to verify manually.*

---

## 7. Interaction with NotebookLM

*   **The Roadmap Update**:
    *   Every Friday, export the **Roadmap View** (or screenshot/text copy).
    *   Upload to **Notebook C (Engineer)**.
    *   *Why?* So when you ask "What are we working on?", the AI knows the current sprint context.
