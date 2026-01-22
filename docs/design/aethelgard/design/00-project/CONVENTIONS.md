---
id: SPEC-PROJECT-CONVENTIONS
title: "Rune & Rust — Conventions"
version: 1.0
status: draft
last-updated: 2025-12-07
---

# Rune & Rust — Conventions

Standards for documentation, naming, and code across the project.

---

## 1. Specification Document Format

### 1.1 Required Frontmatter

```yaml
---
id: SPEC-[CATEGORY]-[NAME]
title: "[Full Title]"
version: X.Y
status: draft | review | approved | deprecated
last-updated: YYYY-MM-DD
related-files:
  - path: "Path/To/File.cs"
    status: Implemented | Planned | Referenced
---
```

### 1.2 Spec ID Format

| Category | Format | Example |
|----------|--------|---------|
| Project | `SPEC-PROJECT-*` | `SPEC-PROJECT-OVERVIEW` |
| Core | `SPEC-CORE-*` | `SPEC-CORE-GAMELOOP` |
| Entity | `SPEC-ENTITY-*` | `SPEC-ENTITY-ATTRIBUTES` |
| Specialization | `SPEC-SPECIALIZATION-*` | `SPEC-SPECIALIZATION-ATGEIR-WIELDER` |
| Combat | `SPEC-COMBAT-*` | `SPEC-COMBAT-DAMAGE` |
| Magic | `SPEC-MAGIC-*` | `SPEC-MAGIC-ELEMENTAL` |
| Item | `SPEC-ITEM-*` | `SPEC-ITEM-WEAPONS` |
| Craft | `SPEC-CRAFT-*` | `SPEC-CRAFT-RECIPES` |
| Environment | `SPEC-ENV-*` | `SPEC-ENV-DUNGEONS` |
| UI | `SPEC-UI-*` | `SPEC-UI-TERMINAL` |
| Data | `SPEC-DATA-*` | `SPEC-DATA-SCHEMA` |
| Test | `SPEC-TEST-*` | `SPEC-TEST-COMBAT` |

---

## 2. Document Structure

### 2.1 Required Sections

1. **Document Control** — Version history table
2. **Overview** — Identity table, design philosophy
3. **Core Mechanics** — Detailed specifications
4. **Implementation Status** — File paths and status
5. **Testing Requirements** — Test cases

### 2.2 Optional Sections

- Status Effects (for abilities)
- GUI Requirements (for visible elements)
- Implementation Priority (for complex specs)

---

## 3. Formula Notation

### 3.1 Dice Notation

| Notation | Meaning |
|----------|---------|
| `Xd Y` | Roll X dice with Y sides |
| `Roll(ATTR)` | Roll dice equal to attribute value |
| `Roll(ATTR + N)` | Roll dice equal to attribute plus bonus |

### 3.2 Pseudocode Style

```
If (Condition):
    Result = Action
    Target.AddStatus("StatusName", Duration: N)
```

---

## 4. Naming Conventions

### 4.1 Code (C#)

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `AtgeirWielder` |
| Methods | PascalCase | `CalculateDamage()` |
| Properties | PascalCase | `StaminaCost` |
| Private fields | _camelCase | `_currentHealth` |
| Enums | PascalCase | `DamageType.Physical` |
| Constants | UPPER_SNAKE | `MAX_RANK` |

### 4.2 Database (PostgreSQL)

| Element | Convention | Example |
|---------|------------|---------|
| Tables | snake_case, plural | `specializations` |
| Columns | snake_case | `display_name` |
| Primary keys | `id` | `id` |
| Foreign keys | `{table}_id` | `archetype_id` |

### 4.3 Files

| Type | Convention | Example |
|------|------------|---------|
| Specs | kebab-case.md | `atgeir-wielder.md` |
| C# files | PascalCase.cs | `AtgeirWielder.cs` |
| Tests | *Tests.cs | `AtgeirWielderTests.cs` |

---

## 5. GUI Specifications

### 5.1 Rank Colors

| Rank | Color | Hex |
|------|-------|-----|
| 1 | Bronze | `#CD7F32` |
| 2 | Silver | `#C0C0C0` |
| 3 | Gold | `#FFD700` |

### 5.2 Status Effect Colors

| Category | Color | Hex |
|----------|-------|-----|
| Debuff (minor) | Blue | `#4169E1` |
| Debuff (major) | Yellow | `#FFD700` |
| Damage over time | Red | `#DC143C` |
| Buff | Green | `#32CD32` |
| Stance | Gray | `#808080` |

### 5.3 ASCII Mockups

Use box-drawing characters for UI mockups:
```
┌─────────────────────────────────┐
│ [Element Name]                  │
│ Description or content          │
└─────────────────────────────────┘
```

---

## 6. Version Control

### 6.1 Spec Versioning

- **Major** (X.0): Breaking changes or major redesign
- **Minor** (X.Y): New content or significant additions
- **Status**: `draft` → `review` → `approved`

### 6.2 Change Log Format

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | YYYY-MM-DD | Initial specification |
