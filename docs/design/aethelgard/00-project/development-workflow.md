---
id: WORKFLOW-DEV-001
title: "Integrated Development Workflow"
version: 1.2
status: versioned
last-updated: 2025-12-26
---

# Integrated Development Workflow

> *"Order from chaos. Not through rigid control, but through clear channels of flow."*

This document defines the standard operating procedure for moving features from abstract ideas to shipped code in the **Rune & Rust** project. It explicitly integrates our tool stack: **Notion**, **NotebookLM**, **GitHub**, **GitLens**, and **AI Assistants**.

---

## 1. Tool Stack & Roles

Each tool has a specific "job to be done" to avoid roadmap fragmentation.

| Tool | Role | The "Source of Truth" For... |
|------|------|------------------------------|
| **Notion** | **The Creative Workshop** | *Drafting lore, narrative bible, storing visual references/mood boards, unstructured design ideas.* |
| **NotebookLM** | **The Strategist / Librarian** | *Context query, lore consistency checks, brainstorming based on existing docs, high-level planning.* |
| **GitHub** | **The Project Manager** | *Task status, "Definition of Done", Issue tracking, code review, project roadmap.* |
| **GitLens** | **The Historian** | *Line-by-line code history, blame, visual branch management in VS Code.* |
| **Claude** | **The Architect / Lead Dev** | *Refactoring, complex implementation, writing detailed specs, "Deep Work".* |
| **ChatGPT** | **The Mechanic / Rubber Duck** | *Quick scripts, explaining errors, regex, verifying simple logic, "Fast Tasks".* |
| **Gemini** | **The Analyst** | *Repo-wide analysis (via IDE), connecting extensive context.* |

---

## 2. NotebookLM Strategy: The "Trinity" Approach

To maximize AI context quality, **do not** dump everything into one notebook. Separation prevents "context pollution" (e.g., getting flavor text when you asked for an API signature).

### Notebook A: "The Oracle" (Lore & Narrative)
*   **Purpose**: World-building, tone, flavor text, factions, geography.
*   **Sources**: `docs/01-core/bible/`, `docs/02-entities/`, `docs/07-environment/`.
*   **Use When**: Writing quest text, naming items, checking timeline consistency.
*   **Goal**: "Does this sound like Rune & Rust?"

### Notebook B: "The Mechanic" (Game Systems)
*   **Purpose**: Rules, math, balance diagrams, system interactions.
*   **Sources**: `docs/01-core/mechanics/`, `docs/03-character/`, `docs/04-systems/`.
*   **Use When**: Designing a new ability, balancing damage curves, creating items.
*   **Goal**: "Is this mechanically balanced?"

### Notebook C: "The Engineer" (Tech & Implementation)
*   **Purpose**: Code patterns, architecture, implementation plans, changelogs.
*   **Sources**: `docs/00-project/`, `docs/plans/`, `docs/architecture/`, `docs/changelogs/`.
*   **Use When**: Writing code, refactoring, planning a migration, writing Unit Tests.
*   **Goal**: "How do I implement this safely?"

> **Pro Tip**: When moving a feature from **Idea -> Code**, you will "pass the torch" between notebooks. You start in **The Oracle** to define the *what*, move to **The Mechanic** to define the *rules*, and finish in **The Engineer** to define the *how*.

---

## 3. The Feature Pipeline (Lifecycle)

We treat development as a pipeline with distinct gates.

```mermaid
flowchart TD
    Idea[1. Ideation (Notion)] -->|Export/Sync| Research[2. Consistency (NotebookLM)]
    Research -->|Refinement| Spec[3. Specification (Markdown)]
    Spec -->|Architect| Plan[4. Implementation Plan]
    Plan -->|GitHub| Track[5. Tracking & Issues]
    Track -->|Claude/Gemini| Code[6. Implementation]
    Code -->|Manual/Auto| QA[7. Verification]
    QA -->|Git| Merge[8. Archive]
```

### Stage 1: Ideation & Narrative (Notion)
*Goal: Capture the creative spark without technical constraints.*
* **Use Case**: Inventing a new faction, a unique weapon, or a plot point.
* **Action**:
    1.  Create a page in **Notion** (e.g., "Drafts/The Ash-Walkers").
    2.  Dump ideas, images, and flavor text.
    3.  Iterate freely.
* **Result**: A rich, unstructured creative draft.

### Stage 2: Research & Synthesize (NotebookLM)
*Goal: Ensure the new idea fits the existing world (Lore Consistency).*
* **Use Case**: Checking if "Ash-Walkers" conflicts with "The Forlorn".
* **Action**:
    1.  Upload the Notion draft (PDF/Text) to **Notebook A (Oracle)**.
    2.  Query: *"Does this faction conflict with established lore?"*
    3.  Query **Notebook B (Mechanic)**: *"Suggest mechanics for Ash-Walker abilities."*
* **Result**: A "Feature Brief" that is lore-compatible and mechanically grounded.

### Stage 3: Specification & Design (The Architect)
*Goal: Turn the brief into a rigorous specification.*
* **Action**:
    1.  Use **Claude** to convert the Brief into a technical Spec (`docs/04-systems/...`).
    2.  **Human Review**: Ensure it aligns with "Golden Standards".
* **Result**: A committed Markdown Spec file.

### Stage 4: Implementation Plan (The Plan)
*Goal: Break the Spec down into atomic steps.*
* **Action**: Create `docs/plans/v0.X.Y/feature-name.md`.
* **Result**: An Implementation Plan Artifact with a checklist.

### Stage 5: Task Tracking (GitHub)
*Goal: Visible, trackable units of work.*
* **Reference**: See detailed guide at [GitHub Project Configuration & Workflow](github-project-setup.md).
* **Action**:
    1.  Create **GitHub Issues** from the Plan's checklist.
    2.  Link Issues to the Spec.
* **Result**: A populated Project Board.

---

## 4. Detailed Use Case Playbooks

### Playbook A: "The New Feature" (Top-Down)
*Scenario: "I want to add a 'Runeforging' system."*
1.  **Notion**: Draft the "Vibe".
2.  **Notebook A**: Check lore constraints.
3.  **Notebook B**: Check crafting mechanics constraints.
4.  **Claude**: Draft Spec.
5.  **GitHub**: Create Issues.
6.  **Code**: Implement.

### Playbook B: "The Bug Fix" (Bottom-Up)
*Scenario: "Collision glitch in the Souring Mires."*
1.  **GitLens**: "Toggle File Blame" on `PhysicsService`.
2.  **Gemini**: Analyze stack trace.
3.  **Notebook C (Engineer)**: *"Search implementation plans for recent physics changes."*
4.  **Code**: Fix.
5.  **GitHub**: Commit `fix`.

### Playbook C: "The Refactor" (Architectural)
*Scenario: "Switch from JSON to Binary saves."*
1.  **Notebook C**: Upload current `persistence.md`. Ask *"What systems depend on JSON?"*
2.  **Claude**: Propose transition plan.
3.  **Markdown**: Update `docs/plans/v0.X.Y/refactor-save.md`.
4.  **Code**: Execute.

---

## 5. AI Decision Matrix

| Task Type | Recommended AI | Why? |
|-----------|----------------|------|
| **"Make this sound cool"** | **Notebook A (Oracle)** | Best for voice/tone consistency. |
| **"Balance this item"** | **Notebook B (Mechanic)** | Best for system math/comparisons. |
| **"Plan this refactor"** | **Notebook C (Engineer)** | Best for architectural constraints. |
| **"Write complex code"** | **Claude 3.5 Sonnet** | Best reasoning/generated code. |
| **"Find usage"** | **Gemini (1.5 Pro)** | Massive context window for repo-wide search. |
| **"Quick Script"** | **ChatGPT (4o)** | Fast, standard tasks. |

---

## 6. Git & GitLens Workflow

**GitLens is your "Flight Recorder".**

1.  **Interactive Rebase**: Use GitLens to visually squash "WIP" commits before merging.
2.  **Blame & Heatmap**: Before touching a legacy file, use **Toggle File Blame** to see *why* code was written that way (and which AI wrote it).
3.  **Worktrees**: If you need to switch contexts (e.g., fix a bug while in the middle of a feature), use Git Worktrees to maintain separate states without stashing hell.

---

## 7. Checklists

### Planning Checklist
- [ ] Idea drafted in Notion?
- [ ] Checked **Notebook A** for Lore conflicts?
- [ ] Checked **Notebook B** for Mechanic conflicts?
- [ ] Spec created in `docs/`?

### Pre-Implementation Checklist
- [ ] Implementation Plan created?
- [ ] Checked **Notebook C** for architectural patterns?
- [ ] GitHub Issues created?

### Completion Checklist
- [ ] Build & Tests pass.
- [ ] Spec marked `[x] Implemented`.
- [ ] Changelog updated.
- [ ] GitLens: Commits cleaned?
