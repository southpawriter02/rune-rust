---
description: A series of items to complete/check before finalizing a commit.
---

## Quick Reference Checklist

Before submitting any change:

- [ ] Adheres to the implementation plan, design specification, and scope-breakdown requirements
- [ ] Follows Clean Architecture layer boundaries
- [ ] Uses data-driven configuration where appropriate
- [ ] Includes XML documentation for public members
- [ ] Has unit tests with AAA pattern and FluentAssertions
- [ ] Uses structured logging with message templates
- [ ] Validates arguments at method entry
- [ ] Returns result objects for expected failures
- [ ] Uses correct naming conventions (PascalCase, camelCase, kebab-case)
- [ ] Configuration files use kebab-case IDs
- [ ] Entities have private EF Core constructors
- [ ] Functionality is fully documented/captured in a specification
- [ ] A changelog has been drafted that captures all work performed