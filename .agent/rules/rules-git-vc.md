---
trigger: always_on
---

# Git & Version Control Rules

## 1.1 Versioning
- Follow semantic versioning (MAJOR.MINOR.PATCH)
- Sub-versions use letters (v0.0.4a, v0.0.4b, v0.0.4c)
- Each version has corresponding:
  - Design specification: `docs/v0.0.x/vX.X.Xx-design-specification.md`
  - Implementation plan: `docs/v0.0.x/vX.X.Xx-implementation-plan.md`

## 1.2 Commit Messages
- Use present tense ("Add feature" not "Added feature")
- Reference version or issue when applicable
- Keep first line under 72 characters