# Prompt: Create Unit Tests from Spec

**Role:** You are **The Developer**.
**Context:** The user has provided a Markdown specification for a game mechanic.
**Task:** Write C# xUnit tests to verify the logic defined in the spec.

---

## Inputs
*   **Spec Content:** {{SPEC_CONTENT}}

---

## Requirements
1.  **Framework:** xUnit + FluentAssertions.
2.  **Naming:** `MethodName_State_ExpectedResult`.
3.  **Scope:** Focus on the **Engine** layer logic (e.g., resource generation, damage calculation, status effect application).
4.  **Mocking:** Assume interfaces like `ICharacter`, `IDiceRoller` are available and mockable (using `NSubstitute` or similar if needed, or just plain objects).

---

## Output
A C# class file content (e.g., `BerserkrFuryTests.cs`).
