# Agent Instructions: Generating High-Fidelity Changelogs

**Goal:** Generate "Deep-Dive" technical changelogs that serve as both a record of release and a technical specification of the implemented state. The output must match the level of detail found in `v0.0.2.md`.

## 1. General Formatting Rules
*   **Filename:** `vX.Y.Z.md`
*   **Title Header:** `# Changelog: vX.Y.Z - <Thematic Title>`
*   **Release Date:** `**Release Date:** YYYY-MM-DD`
*   **Separators:** Use horizontal rules `---` between major sections.
*   **Language:** Technical, precise, past tense for implemented features.

## 2. Required Sections (In Order)

### A. Summary
*   A single paragraph summarizing the release's architectural focus.
*   Mention specific layers touched (e.g., Domain, Engine, Test).
*   Mention key patterns introduced (e.g., Dependency Injection, Unit of Work).

### B. New Files Created
*   **Grouping:** Group by Project/Layer (e.g., `### Core Layer`, `### Engine Layer`).
*   **Format:** Markdown Table.
*   **Columns:** `File` (relative path preferred), `Purpose`.
    ```markdown
    | File | Purpose |
    |------|---------|
    | `RuneAndRust.Core/Enums/MyEnum.cs` | Defines X |
    ```

### C. Files Modified
*   **Format:** Markdown Table.
*   **Columns:** `File`, `Change`.
    ```markdown
    | File | Change |
    |------|--------|
    | `Program.cs` | Registered `IMyService` |
    ```

### D. Code Implementation Details
*   **Content:** This is the "Meat" of the changelog. Do not just list files; explain *logic*.
*   **For Enums:** Provide the full code snippet if concise, or values list with comments.
*   **For Services/Classes:**
    *   List key methods/contract signatures.
    *   Bulleted list of *behaviors* (e.g., "Clamps value between 1 and 10", "Throws exception on negative input").
    *   Document specific constants or thresholds (e.g., "Success threshold: 8+").

### E. Logging Matrix (CRITICAL)
*   **Purpose:** explicit documentation of all structured log events introduced.
*   **Grouping:** Group by Service or formatting context.
*   **Format:** Markdown Table.
*   **Columns:** `Event`, `Level`, `Template`.
    *   *Event:* Human readable description (e.g., "Method entry").
    *   *Level:* Trace, Debug, Info, Warning, Error.
    *   *Template:* The raw Serilog message template (e.g., `"Rolling {PoolSize}d10 for {Context}"`).

### F. Test Coverage
*   **Summary Block:** Code block showing Total, Passed, Failed, Duration.
*   **Complete Test Inventory:**
    *   Group by Test Class (e.g., `#### DiceServiceTests (28 tests)`).
    *   **Format:** Markdown Table.
    *   **Columns:** `Test Name`, `Description`.
    *   *Test Name:* The exact method name (e.g., `Roll_ShouldReturns_X`).
    *   *Description:* One-line summary of what is being asserted.
    *   *Note:* You must list *every single test* added or significant to this release, not just a sample.

### G. DI Registration
*   Show the code block added to `Program.cs` or the startup configuration to register the new services.

### H. Verification Results
*   **Build:** Console output snippet showing "Build succeeded".
*   **Tests:** Console output snippet showing "Passed! ...".

### I. Directory Structure After Release
*   **Format:** Code block with a directory tree.
*   **Annotations:**
    *   `[NEW]` for files added this release.
    *   `[MODIFIED]` for files changed.
    *   Only show relevant directories (e.g., source code, tests, docs), avoid `bin`/`obj`.

### J. Running Tests
*   Provide standard `dotnet test` commands with relevant filters for the new components.

### K. Next Steps
*   Bulleted list of what comes in `vX.Y.Z+1`.

## 3. Style & Voice
*   **Voice:** The "Jötun-Reader" voice is *not* effective here; use standard Professional Engineering English.
*   **Precision:** Do not say "Added some tests." Say "Added 12 tests covering boundary conditions and null checks."
*   **Completeness:** If a file was created, it **must** appear in the file list. If a log statement exists in the code, it **must** appear in the Logging Matrix.

## 4. Example Template

```markdown
# Changelog: v0.0.X - The Feature Name

**Release Date:** 2025-MM-DD

## Summary
Brief architectural summary...

---

## New Files Created
### Layer Name
| File | Purpose |
|------|---------|
...

---

## Code Implementation Details
### ServiceName
- Details
```
