# Specification & Code Development Rules

Parent item: Specification Writing Guide (Specification%20Writing%20Guide%202ba55eb312da8032b14de6752422e93e.md)

**Purpose**: This document establishes mandatory rules and best practices for developing specifications and implementing code for the Rune & Rust project.

**Last Updated**: 2025-11-19
**Target Audience**: All developers (human and AI) working on specifications and implementation

---

## Table of Contents

1. [Core Principles](Specification%20&%20Code%20Development%20Rules%202ba55eb312da80fa94b5c44a9d9da5ab.md)
2. [Specification Development Rules](Specification%20&%20Code%20Development%20Rules%202ba55eb312da80fa94b5c44a9d9da5ab.md)
3. [Code Development Rules](Specification%20&%20Code%20Development%20Rules%202ba55eb312da80fa94b5c44a9d9da5ab.md)
4. [Setting Compliance Rules](Specification%20&%20Code%20Development%20Rules%202ba55eb312da80fa94b5c44a9d9da5ab.md)
5. [Quality Standards](Specification%20&%20Code%20Development%20Rules%202ba55eb312da80fa94b5c44a9d9da5ab.md)
6. [Collaboration Rules](Specification%20&%20Code%20Development%20Rules%202ba55eb312da80fa94b5c44a9d9da5ab.md)
7. [Enforcement & Review](Specification%20&%20Code%20Development%20Rules%202ba55eb312da80fa94b5c44a9d9da5ab.md)

---

## Core Principles

### Principle 1: Design Before Implementation

**Rule**: NEVER write code without a specification or clear design intent.

**Rationale**: Code without design leads to:

- Inconsistent player experiences
- Technical debt
- Difficult-to-maintain systems
- Misaligned features

**Exception**: Prototyping is allowed for exploration, but must be discarded or formalized before merging.

### Principle 2: Setting Compliance is Non-Negotiable

**Rule**: ALL game content must comply with Aethelgard setting lore (see `SETTING_COMPLIANCE.md`).

**Rationale**: Rune & Rust has a specific, detailed setting that differentiates it from generic fantasy. Breaking setting consistency destroys immersion.

**Enforcement**: All specifications and code must pass setting compliance checks before approval.

### Principle 3: One Source of Truth

**Rule**: Specifications are the authoritative source for WHAT and WHY. Code is the source for HOW.

**Rationale**: When specifications and code conflict, specifications win (unless specification is outdated and needs updating).

**Process**: If code diverges from spec, either:

1. Update code to match spec, OR
2. Update spec to match new design intent (with approval)

### Principle 4: Testability & Verifiability

**Rule**: Every functional requirement must be testable. Every mechanic must have worked examples.

**Rationale**: If you can't test it, you can't verify it works correctly.

**Implementation**: All FR acceptance criteria must be checkboxes that can be tested.

### Principle 5: Iterative Refinement

**Rule**: Specifications and code are living documents. Update them as understanding deepens.

**Rationale**: First drafts are never perfect. Embrace incremental improvement.

**Process**: Use version control and change logs to track evolution.

---

## Specification Development Rules

### SPEC-RULE-001: Use the Template

**Mandatory**: All specifications MUST use `TEMPLATE.md` as the starting point.

**Rationale**: Consistency ensures all specs contain required sections and are easy to navigate.

**Verification**: ✅ Specification contains all template sections (or marks as "N/A" with justification)

**Violation Handling**: Specification returned to draft status until template compliance achieved.

---

### SPEC-RULE-002: Complete All Checklists

**Mandatory**: All checklists in the template MUST be completed before marking spec as "Review" status.

**Checklists**:

- Functional Requirements Completeness
- System Mechanics Completeness
- Integration Points Completeness
- Document Completeness Checklist

**Rationale**: Checklists ensure thoroughness and catch gaps.

**Verification**: ✅ All checkboxes in checklists are checked or explicitly marked N/A

**Violation Handling**: Specification blocked from approval until all checklists complete.

---

### SPEC-RULE-003: All Formulas Have Worked Examples

**Mandatory**: EVERY formula or mechanic MUST include at least one worked example with concrete values.

**Example Format**:

```
Formula:
  Result = BaseValue × Modifier

Example:
  BaseValue = 30
  Modifier = 1.25
  Result = 30 × 1.25 = 37.5 → 37 (floored)

```

**Rationale**: Worked examples:

- Clarify designer intent
- Catch errors in formulas
- Enable implementers to verify correctness
- Provide test cases

**Verification**: ✅ Every formula/mechanic has Example: section with numbers

**Violation Handling**: Specification returned to author for examples.

---

### SPEC-RULE-004: All Requirements Have Acceptance Criteria

**Mandatory**: Every Functional Requirement (FR-XXX) MUST have acceptance criteria in checkbox format.

**Minimum**: 3 acceptance criteria per FR (typical: 4-6)

**Format**:

```
**Acceptance Criteria**:
- [ ] Specific, measurable criterion 1
- [ ] Specific, measurable criterion 2
- [ ] Specific, measurable criterion 3

```

**Rationale**: Acceptance criteria define "done" and enable testing.

**Verification**: ✅ All FR sections have Acceptance Criteria subsection with checkboxes

**Violation Handling**: FR is incomplete until criteria added.

---

### SPEC-RULE-005: No Ambiguous Language

**Mandatory**: Specifications MUST use precise, unambiguous language.

**Avoid**:

- ❌ "should" (use "must" or "can")
- ❌ "usually" (specify exact conditions)
- ❌ "roughly" (provide exact values or ranges)
- ❌ "might" (specify conditions or remove)

**Use**:

- ✅ "must" (required)
- ✅ "can" (optional)
- ✅ "if [condition] then [behavior]" (conditional)
- ✅ Exact numbers or ranges (e.g., "10-15" not "around 12")

**Rationale**: Ambiguity leads to misimplementation and bugs.

**Verification**: ✅ Manual review for ambiguous language

**Violation Handling**: Author revises to clarify.

---

### SPEC-RULE-006: Setting Compliance Check Required

**Mandatory**: All specifications MUST pass setting compliance validation before approval.

**Process**:

1. Read `SETTING_COMPLIANCE.md` Quick Compliance Check
2. Identify applicable domains (Cosmology, Magic, Technology, etc.)
3. Validate spec against domain checklists
4. Document compliance in spec (add note: "Setting Compliance: Validated against Domain X, Y")

**Common Violations**:

- Using "mana" instead of "Aether"
- Traditional spellcasting without runic focal points
- Pre-Glitch magic users
- Creating new Data-Slates or programming Pre-Glitch systems
- Resolving the Counter-Rune paradox

**Verification**: ✅ Specification includes setting compliance validation section

**Violation Handling**: Specification blocked until compliance achieved.

---

### SPEC-RULE-007: Link to Related Documentation

**Mandatory**: All specifications MUST link to:

- Related specifications (SPEC-XXX-XXX format)
- Implementation documentation (Layer 1 docs)
- Code references (file:line format)

**Rationale**: Cross-referencing enables navigation and impact analysis.

**Verification**: ✅ "Related Documentation" section populated with links

**Violation Handling**: Specification quality reduced; author adds links.

---

### SPEC-RULE-008: Assign Unique Spec ID

**Mandatory**: All specifications MUST have a unique ID in format `SPEC-{DOMAIN}-{NUMBER}`.

**Domains**: COMBAT, PROGRESSION, ECONOMY, WORLD, NARRATIVE, FACTION, AI

**Number Assignment**:

1. Check `SPEC_BACKLOG.md` for existing specs in domain
2. Assign next sequential number (001, 002, etc.)
3. Update SPEC_BACKLOG.md with new spec entry

**Rationale**: Unique IDs enable precise cross-referencing.

**Verification**: ✅ Spec ID follows format and is unique

**Violation Handling**: ID collision resolved by author.

---

### SPEC-RULE-009: Update SPEC_BACKLOG.md

**Mandatory**: When creating a new spec, MUST add entry to `SPEC_BACKLOG.md`.

**Required Entry Fields**:

- Spec ID
- Status (Planned → In Progress → Completed)
- Priority (High/Medium/Low)
- Domain
- Proposed Scope (bullet points)
- Dependencies
- Why Needed
- Implementation Exists (Yes/No)

**Rationale**: Backlog is central registry for tracking specifications.

**Verification**: ✅ SPEC_BACKLOG.md updated with new spec

**Violation Handling**: Specification not discoverable until backlog updated.

---

### SPEC-RULE-010: No TBD or TODO Placeholders in Approved Specs

**Mandatory**: Specifications marked "Approved" or "Active" MUST NOT contain TBD/TODO placeholders.

**Draft Status**: TBD/TODO acceptable in "Draft" status
**Review Status**: TBD/TODO acceptable if explicitly called out in review notes
**Approved Status**: NO TBD/TODO allowed

**Rationale**: Approved specs are implementation-ready. Placeholders indicate incomplete design.

**Verification**: ✅ Search for "TBD", "TODO", "[PLACEHOLDER]" returns zero results

**Violation Handling**: Specification downgraded to "Draft" until placeholders resolved.

---

## Code Development Rules

### CODE-RULE-001: Follow Specification Requirements

**Mandatory**: All code MUST implement the functional requirements (FR-XXX) defined in specifications.

**Process**:

1. Read specification completely before coding
2. Implement each FR with traceability (code comments reference FR-XXX)
3. Verify acceptance criteria met
4. Add unit tests for each FR

**Rationale**: Code that doesn't match spec creates divergence and confusion.

**Verification**: ✅ Code review checks FR implementation completeness

**Violation Handling**: Code rejected in review; implement missing FRs.

---

### CODE-RULE-002: Reference Specification in Code Comments

**Mandatory**: All major classes/services MUST include header comment referencing specification.

**Format**:

```csharp
/// <summary>
/// [Brief description of class purpose]
///
/// Specification: SPEC-XXX-YYY ([Spec Name])
/// Related: SPEC-AAA-BBB (if applicable)
/// </summary>
public class MyService
{
    // ...
}

```

**Rationale**: Enables developers to find design rationale from code.

**Verification**: ✅ Major classes have spec reference in XML docs

**Violation Handling**: Code review requests spec reference addition.

---

### CODE-RULE-003: Use Canonical Terminology

**Mandatory**: All code, variables, and classes MUST use canonical Aethelgard terminology.

**Correct Terminology**:

- ✅ Aether (not mana, magic points, MP)
- ✅ Legend (not XP, experience)
- ✅ Weaving (not spellcasting, magic)
- ✅ Jötun (not AI, robots, machines)
- ✅ Runic Blight (not corruption as generic term)
- ✅ Saga (not quest log, journal)
- ✅ Glitch (not bug, error) - for in-universe context

**Rationale**: Code terminology reinforces setting consistency.

**Verification**: ✅ Code review checks for non-canonical terms

**Violation Handling**: Rename variables/classes to canonical terms.

---

### CODE-RULE-004: No Magic Numbers

**Mandatory**: All numeric values in formulas MUST be named constants or configuration values.

**Bad**:

```csharp
int damage = baseDamage * 1.25; // Magic number!

```

**Good**:

```csharp
const float CRIT_MULTIPLIER = 1.25f;
int damage = baseDamage * CRIT_MULTIPLIER;

```

**Rationale**: Named constants enable tuning without code changes and clarify intent.

**Verification**: ✅ Code review checks for magic numbers

**Violation Handling**: Extract magic numbers to constants.

---

### CODE-RULE-005: All Public Methods Have XML Documentation

**Mandatory**: All public classes, methods, and properties MUST have XML documentation comments.

**Required Elements**:

- `<summary>`: What does this do?
- `<param>`: What does each parameter mean?
- `<returns>`: What does this return?
- `<exception>` (if applicable): What exceptions thrown?

**Rationale**: Self-documenting code aids maintenance and AI comprehension.

**Verification**: ✅ Compiler warnings for missing XML docs

**Violation Handling**: Add XML docs before merge.

---

### CODE-RULE-006: Follow SOLID Principles

**Mandatory**: All code MUST adhere to SOLID principles:

- **S**ingle Responsibility: One class, one purpose
- **O**pen/Closed: Open for extension, closed for modification
- **L**iskov Substitution: Subclasses must be substitutable for base classes
- **I**nterface Segregation: Many specific interfaces > one general interface
- **D**ependency Inversion: Depend on abstractions, not concretions

**Rationale**: SOLID principles ensure maintainable, testable code.

**Verification**: ✅ Code review evaluates SOLID compliance

**Violation Handling**: Refactor to adhere to SOLID.

---

### CODE-RULE-007: Separate Data from Logic

**Mandatory**: Data models (POCOs) MUST NOT contain business logic.

**Data Models** (RuneAndRust.Core):

- ✅ Properties with getters/setters
- ✅ Data validation (simple constraints)
- ❌ Business logic methods
- ❌ Dependencies on services

**Services** (RuneAndRust.Engine):

- ✅ Business logic
- ✅ Dependencies on other services
- ❌ Direct database access (use repositories)

**Rationale**: Separation enables testing, reusability, and clarity.

**Verification**: ✅ Core models are POCOs

**Violation Handling**: Move logic from models to services.

---

### CODE-RULE-008: No Hardcoded Strings for Data

**Mandatory**: Enemy names, ability names, item names, etc. MUST come from database/JSON, not hardcoded strings.

**Bad**:

```csharp
if (enemyType == "Draugr Warrior") { ... } // Hardcoded!

```

**Good**:

```csharp
if (enemy.EnemyID == EnemyDatabase.DraugrWarrior.ID) { ... }

```

**Rationale**: Data-driven design enables balance changes without recompilation.

**Verification**: ✅ Code review checks for hardcoded data strings

**Violation Handling**: Move strings to database/configuration.

---

### CODE-RULE-009: All Services Use Dependency Injection

**Mandatory**: All services MUST use constructor-based dependency injection.

**Format**:

```csharp
public class MyService
{
    private readonly IDependency _dependency;

    public MyService(IDependency dependency)
    {
        _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
    }
}

```

**Rationale**: DI enables:

- Unit testing with mocks
- Loose coupling
- Clear dependencies

**Verification**: ✅ Services use constructor injection

**Violation Handling**: Refactor to use DI.

---

### CODE-RULE-010: Write Unit Tests for All Services

**Mandatory**: All services MUST have corresponding unit test class with >80% code coverage.

**Required Tests**:

- Happy path (expected inputs/outputs)
- Edge cases (boundary conditions)
- Error conditions (invalid inputs, null checks)
- Integration points (mocked dependencies)

**Rationale**: Tests ensure correctness and prevent regressions.

**Verification**: ✅ Test coverage report shows >80% coverage

**Violation Handling**: Write missing tests before merge.

---

## Setting Compliance Rules

### SETTING-RULE-001: Read SETTING_COMPLIANCE.md Before Creating Content

**Mandatory**: MUST read `SETTING_COMPLIANCE.md` before:

- Writing specifications with game content
- Implementing abilities, enemies, items
- Creating narrative text
- Designing new systems

**Rationale**: Prevents setting violations and rework.

**Verification**: ✅ Author confirms compliance check performed

**Violation Handling**: Content revised to comply with setting.

---

### SETTING-RULE-002: No Generic Fantasy Tropes

**Mandatory**: MUST NOT use generic fantasy terminology or mechanics unless aligned with Aethelgard lore.

**Violations**:

- ❌ "Mana" (use Aether)
- ❌ Traditional wizards/mages (use Weavers, require runic focal points)
- ❌ Elves, dwarves, orcs (setting-specific species only)
- ❌ Generic "magic system" (use Aether + Runic Focal Points + Weaving)
- ❌ Medieval Europe analogues (Aethelgard is post-apocalyptic with Pre-Glitch tech)

**Rationale**: Rune & Rust has unique setting identity that must be preserved.

**Verification**: ✅ Setting compliance review

**Violation Handling**: Replace generic terms with canonical equivalents.

---

### SETTING-RULE-003: Layer 2 Diagnostic Voice for Game Text

**Mandatory**: All player-facing narrative text MUST use Layer 2 Diagnostic Voice (see `SETTING_COMPLIANCE.md`).

**Characteristics**:

- Clinical precision with foreboding undertones
- Technical terminology from Pre-Glitch era
- Emotional detachment masking dread
- Glitch-influenced syntax distortions (use sparingly)

**Example**:

```
✅ Good: "Subject exhibits entropy-accelerated decay. Necrotic Blight signatures detected. Termination recommended."

❌ Bad: "The zombie looks scary and is rotting."

```

**Rationale**: Voice consistency creates atmospheric immersion.

**Verification**: ✅ Voice compliance review

**Violation Handling**: Rewrite text in Layer 2 Diagnostic Voice.

---

### SETTING-RULE-004: Respect Established Mysteries

**Mandatory**: MUST NOT resolve or explain established setting mysteries without explicit approval.

**Protected Mysteries**:

- Counter-Rune paradox (must remain unresolved)
- True nature of the Glitch
- Origin of the Nine Realms
- Purpose of the Jötun

**Rationale**: Mysteries create engagement. Premature resolution kills intrigue.

**Verification**: ✅ Design review flags mystery resolution

**Violation Handling**: Revise content to preserve mystery.

---

### SETTING-RULE-005: No Pre-Glitch Technology Creation

**Mandatory**: MUST NOT create new Pre-Glitch technology or reprogram Jötun systems.

**Allowed**:

- ✅ Scavenging Pre-Glitch tech
- ✅ Repairing existing tech (limited, degraded)
- ✅ Jötun-Readers analyzing systems (imperfect, risky)

**Forbidden**:

- ❌ Creating new Data-Slates
- ❌ Programming Jötun AIs
- ❌ Reverse-engineering Pre-Glitch fabricators
- ❌ Precise Jötun diagnostics (they're unreliable)

**Rationale**: Pre-Glitch tech is degraded and inscrutable. Perfect understanding breaks setting.

**Verification**: ✅ Tech compliance review

**Violation Handling**: Replace with scavenging/imperfect analysis.

---

## Quality Standards

### QUALITY-RULE-001: No Compiler Warnings

**Mandatory**: Code MUST compile with zero warnings.

**Exceptions**: Warnings explicitly suppressed with `#pragma warning disable` and justification comment.

**Rationale**: Warnings indicate potential issues and reduce code quality.

**Verification**: ✅ Build log shows 0 warnings

**Violation Handling**: Fix warnings before merge.

---

### QUALITY-RULE-002: Code Review Required

**Mandatory**: All code changes MUST be reviewed before merge.

**Review Checklist**:

- [ ]  Implements specification requirements
- [ ]  Follows code rules (CODE-RULE-001 through CODE-RULE-010)
- [ ]  Has unit tests with >80% coverage
- [ ]  XML documentation complete
- [ ]  No magic numbers
- [ ]  No compiler warnings
- [ ]  Setting compliance validated (if applicable)

**Rationale**: Code review catches errors and ensures consistency.

**Verification**: ✅ GitHub PR approved by reviewer

**Violation Handling**: Changes requested in review.

---

### QUALITY-RULE-003: Consistent Formatting

**Mandatory**: All code MUST follow consistent formatting (use project `.editorconfig`).

**Standards**:

- Indentation: 4 spaces (no tabs)
- Braces: Allman style (opening brace on new line)
- Line length: 120 characters max
- Naming: PascalCase (classes, methods), camelCase (parameters, locals)

**Rationale**: Consistent formatting aids readability.

**Verification**: ✅ Auto-formatter applied

**Violation Handling**: Run formatter before commit.

---

### QUALITY-RULE-004: Meaningful Commit Messages

**Mandatory**: Commit messages MUST be descriptive and follow format.

**Format**:

```
[type]: [brief description]

[Optional detailed explanation]

Refs: SPEC-XXX-YYY (if implementing specification)

```

**Types**: feat, fix, docs, refactor, test, chore

**Examples**:

```
feat: Implement damage calculation with armor mitigation

Added DamageCalculationService with support for physical/aetheric damage,
armor resistance, and critical hit multipliers.

Refs: SPEC-COMBAT-002

```

**Rationale**: Good commit messages enable project archaeology and understanding.

**Verification**: ✅ Commit message follows format

**Violation Handling**: Amend commit message.

---

### QUALITY-RULE-005: Test Before Commit

**Mandatory**: MUST run full test suite before committing.

**Process**:

1. Run `dotnet test` (all tests must pass)
2. Review test coverage report (>80% target)
3. Manual smoke test (if UI changes)
4. Commit only if all tests pass

**Rationale**: Broken tests indicate regressions.

**Verification**: ✅ CI pipeline runs tests on push

**Violation Handling**: Fix failing tests before merge.

---

## Collaboration Rules

### COLLAB-RULE-001: Communicate Design Intent

**Mandatory**: When creating specifications or implementing features, MUST document "why" not just "what".

**In Specifications**: Use "Rationale" subsections to explain design decisions
**In Code**: Use comments to explain non-obvious logic
**In Reviews**: Explain reasoning behind implementation choices

**Rationale**: Understanding intent enables better feedback and future maintenance.

**Verification**: ✅ Rationale sections populated in specs; code comments explain why

**Violation Handling**: Add rationale explanations.

---

### COLLAB-RULE-002: Ask Questions Early

**Mandatory**: If requirements are unclear or ambiguous, MUST ask for clarification before implementing.

**Process**:

1. Identify ambiguity
2. Ask user/owner for clarification
3. Document answer in specification or code comments
4. Proceed with implementation

**Rationale**: Assumptions lead to wrong implementations and rework.

**Verification**: ✅ Questions logged and answered

**Violation Handling**: Rework based on correct requirements.

---

### COLLAB-RULE-003: Update Documentation

**Mandatory**: When code changes, MUST update corresponding documentation.

**Documentation Types**:

- Specifications (if requirements change)
- Layer 1 system docs (if implementation significantly changes)
- XML code comments (if method signatures or behavior changes)
- README files (if setup/usage changes)

**Rationale**: Stale documentation is worse than no documentation.

**Verification**: ✅ Documentation review in PRs

**Violation Handling**: Update documentation before merge.

---

### COLLAB-RULE-004: Use Specs as Discussion Basis

**Mandatory**: When discussing features, MUST reference specifications.

**Format**: "According to SPEC-COMBAT-002, damage calculation should..."

**Rationale**: Specs are shared source of truth. Referencing them aligns discussions.

**Verification**: ✅ Discussions reference spec IDs

**Violation Handling**: Redirect discussion to specification.

---

### COLLAB-RULE-005: Propose Changes via Specs

**Mandatory**: When proposing significant changes, MUST update specification first.

**Process**:

1. Draft specification changes
2. Present to stakeholders
3. Get approval
4. Implement code changes
5. Update spec version history

**Rationale**: Design-first approach prevents wasted implementation effort.

**Verification**: ✅ Spec updated before code

**Violation Handling**: Spec updated retroactively; change reviewed.

---

## Enforcement & Review

### Review Checklist for Specifications

Before approving a specification, verify:

- [ ]  **SPEC-RULE-001**: Uses [TEMPLATE.md](http://template.md/) structure
- [ ]  **SPEC-RULE-002**: All checklists completed
- [ ]  **SPEC-RULE-003**: All formulas have worked examples
- [ ]  **SPEC-RULE-004**: All FRs have acceptance criteria
- [ ]  **SPEC-RULE-005**: No ambiguous language
- [ ]  **SPEC-RULE-006**: Setting compliance validated
- [ ]  **SPEC-RULE-007**: Related documentation linked
- [ ]  **SPEC-RULE-008**: Unique Spec ID assigned
- [ ]  **SPEC-RULE-009**: SPEC_BACKLOG.md updated
- [ ]  **SPEC-RULE-010**: No TBD/TODO placeholders (if Approved status)
- [ ]  **SETTING-RULE-001**: Setting compliance reviewed
- [ ]  **SETTING-RULE-002**: No generic fantasy tropes
- [ ]  **SETTING-RULE-003**: Layer 2 Diagnostic Voice used
- [ ]  **SETTING-RULE-004**: Mysteries preserved
- [ ]  **SETTING-RULE-005**: No Pre-Glitch tech creation

### Review Checklist for Code

Before approving code:

- [ ]  **CODE-RULE-001**: Implements specification FRs
- [ ]  **CODE-RULE-002**: Spec references in code comments
- [ ]  **CODE-RULE-003**: Canonical terminology used
- [ ]  **CODE-RULE-004**: No magic numbers
- [ ]  **CODE-RULE-005**: XML documentation complete
- [ ]  **CODE-RULE-006**: SOLID principles followed
- [ ]  **CODE-RULE-007**: Data separated from logic
- [ ]  **CODE-RULE-008**: No hardcoded data strings
- [ ]  **CODE-RULE-009**: Dependency injection used
- [ ]  **CODE-RULE-010**: Unit tests >80% coverage
- [ ]  **QUALITY-RULE-001**: No compiler warnings
- [ ]  **QUALITY-RULE-002**: Code review completed
- [ ]  **QUALITY-RULE-003**: Consistent formatting
- [ ]  **QUALITY-RULE-004**: Meaningful commit messages
- [ ]  **QUALITY-RULE-005**: All tests pass

### Rule Violation Severity

**Critical** (blocks merge):

- Setting compliance violations (SETTING-RULE-*)
- Missing specification for new features (SPEC-RULE-001)
- Failing tests (QUALITY-RULE-005)
- Compiler warnings (QUALITY-RULE-001)

**High** (requires fix before approval):

- Missing acceptance criteria (SPEC-RULE-004)
- No worked examples (SPEC-RULE-003)
- Missing unit tests (CODE-RULE-010)
- Magic numbers (CODE-RULE-004)

**Medium** (fix soon):

- Missing XML docs (CODE-RULE-005)
- Ambiguous language (SPEC-RULE-005)
- Missing spec references (CODE-RULE-002)

**Low** (nice to have):

- Formatting inconsistencies (QUALITY-RULE-003)
- Minor documentation gaps (COLLAB-RULE-003)

---

## Summary

These rules ensure:

- ✅ Consistent, high-quality specifications
- ✅ Maintainable, testable code
- ✅ Setting compliance and thematic consistency
- ✅ Effective collaboration between humans and AI
- ✅ Traceability from design to implementation

**Key Takeaway**: Design first, implement second, document always, test everything.

---

**Document Version**: 1.0
**Maintained By**: Specification governance framework
**Feedback**: Update this document as new rules or best practices emerge