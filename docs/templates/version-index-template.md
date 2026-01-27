# Version Index Template

Use this template for creating new version index files (e.g., `v0.0.X-index.md`).

---

## v0.0.X - [Version Theme/Name]

**Focus:** [One-sentence description of the version's primary goal]

**Prerequisites:** [List prerequisite versions, e.g., "v0.0.5 Complete"]

### Related Documents

- [v0.0.X Scope Breakdown](v0.0.X-scope-breakdown.md) *(if version is complex enough to warrant sub-phases)*
- [v0.0.Xa Design Specification](v0.0.Xa-design-specification.md) *(for each sub-phase)*
- [v0.0.Xa Implementation Plan](v0.0.Xa-implementation-plan.md) *(optional, for complex implementations)*

### Features

#### Core Features
- [ ] **[Feature Name]** - [Brief description of what it does]
- [ ] **[Feature Name]** - [Brief description, include command if applicable: `command`]
- [ ] **[Feature Name]** - [Brief description]

#### Secondary Features *(optional section)*
- [ ] **[Feature Name]** - [Brief description]
- [ ] **[Feature Name]** - [Brief description]

#### [Feature Category Name] *(optional, for grouping related features)*
- [ ] **[Feature Name]** - [Brief description]
  - [Sub-feature or detail]
  - [Sub-feature or detail]
- [ ] **[Feature Name]** - [Brief description]

### Data Model Updates

- [ ] Create `[EntityName]` entity with [brief description of properties]
- [ ] Create `[ValueObjectName]` value object for [purpose]
- [ ] Add `[PropertyName]` to `[ExistingEntity]`
- [ ] Add `[EnumName]` enum ([value1], [value2], ...)
- [ ] Create `[ServiceName]` for [purpose]

### Commands *(if applicable)*

| Command | Description |
|---------|-------------|
| `command` | What it does |
| `command <arg>` | What it does with argument |

### Configuration *(if applicable)*

- [ ] Add `[config-file].json` for [purpose]
- [ ] Update `[existing-config].json` with [new properties]

---

## Usage Notes

1. **Simple versions** (1-2 features): May not need sub-phases or scope breakdown
2. **Medium versions** (3-5 features): Consider scope breakdown, 2-3 sub-phases
3. **Complex versions** (6+ features): Require scope breakdown, multiple sub-phases

## Naming Conventions

- Version index: `v0.0.X-index.md`
- Scope breakdown: `v0.0.X-scope-breakdown.md`
- Design specifications: `v0.0.Xa-design-specification.md`, `v0.0.Xb-design-specification.md`
- Implementation plans: `v0.0.Xa-implementation-plan.md`
