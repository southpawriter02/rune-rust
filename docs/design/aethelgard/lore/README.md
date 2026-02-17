# Lore Documentation

This directory contains all lore documents for the Rune & Rust project, organized by category.

---

## Directory Structure

| Directory | Description | Examples |
|-----------|-------------|----------|
| **meta/** | Canonical governance, setting fundamentals, writing standards | `setting-fundamentals.md` |
| **factions/** | Faction dossiers, cultural studies, creed documents | `midgard-combine.md` |
| **entities/** | Automata, AI systems, constructs, named NPCs | `chief-archivist.md` |
| **fauna/** | Wildlife, creatures, bestiary entries | `ash-crow.md`, `blight-hound.md` |
| **flora/** | Plants, fungi, vegetation | *(pending)* |
| **geography/** | Realm gazetteers, locations, regions | *(pending)* |
| **hazards/** | Environmental threats, atmospheric phenomena | `co2-pool.md` |
| **history/** | Ages, eras, historical events | `era-creeds.md` |

---

## Adding New Content

1. **Identify the category** that best fits your content
2. **Create the file** in the appropriate directory using kebab-case naming
3. **Apply Markdown formatting** consistent with existing entries
4. **Include YAML frontmatter** with id, title, classification, status, last-updated

---

## Formatting Standards

All lore documents should include:

- **YAML frontmatter** with document metadata
- **Tables** for structured data (unlock thresholds, fragment metadata)
- **Blockquotes** for in-world content fragments
- **Horizontal rules** between major sections
- **Data Captures** formatted with property tables and integration notes

See `fauna/ash-crow.md` as a gold standard example.
