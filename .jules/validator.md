# The Validator's Journal

> **Purpose:** Track CRITICAL compliance learnings only. This is NOT a log—only add entries when you discover patterns, fixes, or constraints that affect future validation work.

---

## Entry Guidelines

Only add entries when you discover:
- A new persistent "scientific leak" pattern
- A voice fix that clarified a major ambiguity
- A rejected canon change with important constraints
- A gap in the 9-Domain validation logic

**Format:**
```markdown
## YYYY-MM-DD - [Descriptive Title]

**Violation:** [What you found]
**Learning:** [Why it persisted / root cause]
**Prevention:** [New rule or check added]
```

---

## Journal Entries

### 2024-12-24 - Journal Initialization

**Violation:** N/A (initialization entry)
**Learning:** The Validator agent requires a persistent learning repository to avoid rediscovering the same patterns across sessions.
**Prevention:** Journal created at `.jules/validator.md`. All CRITICAL learnings to be recorded here for cross-session continuity.

### 2024-12-24 - Flavor Text Domain 4 Violations

**Violation:** Found persistent use of "ozone" and "cellular regeneration" in `narrative_flavor_pack.sql`.
**Learning:** Writers often default to sci-fi tropes (ozone, radiation, cells) when describing post-apocalyptic settings, forgetting the "Ignorance as Aesthetic" rule (Domain 4).
**Prevention:**
1. Added check for "ozone" -> "storm-scorch" / "lightning-smell".
2. Added check for "cellular" -> "living weave" / "flesh-pattern".
3. Added check for "battery" -> "spark-vessel".

---

<!--
TEMPLATE FOR NEW ENTRIES:

## YYYY-MM-DD - [Title]

**Violation:** [What you found]
**Learning:** [Why it persisted]
**Prevention:** [New rule added]

-->
