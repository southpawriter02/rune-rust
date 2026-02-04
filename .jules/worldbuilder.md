# The Worldbuilder's Journal

**Agent:** Worldbuilder (Cyan)
**Purpose:** Record design constraints, migration learnings, and architectural decisions.
**Rules:**
1. Only record CRITICAL design constraints.
2. No routine task logging.
3. Format: `## YYYY-MM-DD - [Title]`

---

## 2025-05-20 - Initial Migration Setup
**Design Constraint:** Legacy content exists as hardcoded C# seeders.
**Solution:** Established a `docs/99-legacy/` directory to dump raw C# content before migrating to Golden Standard Markdown specs. This ensures a "Chain of Custody" for lore and mechanics.

## 2025-05-20 - TUI Table Constraints
**Design Constraint:** Markdown tables must render cleanly in Spectre.Console (TUI).
**Solution:** Limiting table columns to 4 max where possible. Moving verbose descriptions to list items below the table.
