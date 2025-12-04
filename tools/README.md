# Rune & Rust Tools

This directory contains utility scripts for managing game content.

## Available Tools

### export_descriptors.py

Exports all descriptor data from SQL files to Notion-compatible formats.

**Features:**
- Parses SQL INSERT statements to extract descriptor data
- Exports to CSV (for Notion database imports)
- Exports to Markdown (for Notion pages)
- Exports to JSON (for programmatic use)
- Creates Notion database templates with property definitions
- Generates a master index for easy navigation

**Usage:**

```bash
# Export all formats to ./exports
python3 tools/export_descriptors.py

# Export only CSV files
python3 tools/export_descriptors.py --format csv

# Export only Markdown files
python3 tools/export_descriptors.py --format md

# Custom output directory
python3 tools/export_descriptors.py --output-dir ./my-exports
```

**Output Structure:**

```
exports/
├── csv/                    # CSV files for Notion database import
│   ├── descriptor_fragments.csv
│   ├── atmospheric_descriptors.csv
│   ├── status_effects.csv
│   └── ...
├── markdown/               # Markdown files for Notion pages
│   ├── descriptor_fragments.md
│   ├── descriptor_fragments_notion_template.md
│   └── ...
└── json/                   # JSON files for programmatic use
    ├── descriptor_fragments.json
    └── ...
```

**Supported Descriptor Types:**

| Type | SQL File | Description |
|------|----------|-------------|
| descriptor_fragments | v0.38.1 | Room description fragments |
| room_function_variants | v0.38.1 | Chamber function variants |
| atmospheric_descriptors | v0.38.4 | Lighting, sound, smell, temperature |
| thematic_modifiers | v0.38.0 | Biome thematic modifiers |
| status_effects | v0.38.8 | Buff/debuff descriptors |
| galdr_actions | v0.38.7 | Magic action descriptors |
| galdr_outcomes | v0.38.7 | Magic outcome descriptors |
| npc_barks | v0.38.11 | NPC dialogue snippets |
| trauma_types | v0.38.14 | Trauma type descriptors |
| trauma_triggers | v0.38.14 | Trauma trigger descriptors |
| breaking_points | v0.38.14 | Breaking point descriptors |
| recovery_descriptors | v0.38.14 | Recovery descriptors |
| examination_descriptors | v0.38.9 | Perception/examination text |
| skill_usage | v0.38.10 | Skill usage descriptors |
| combat_mechanics | v0.38.12 | Combat action descriptors |
| ambient_environmental | v0.38.13 | Ambient environment descriptors |

**Importing to Notion:**

1. **For Databases:**
   - Create a new Notion database
   - Reference the `*_notion_template.md` file for required properties
   - Import the corresponding CSV file
   - Map columns to database properties

2. **For Pages:**
   - Import the Markdown files directly into Notion
   - The files are formatted for Notion's Markdown parser

## Requirements

- Python 3.7+
- No external dependencies required

## Adding New Export Types

To add support for new descriptor tables:

1. Add the SQL filename to the `SQL_FILES` dictionary in `export_descriptors.py`
2. If the table follows the standard INSERT pattern, it will be auto-detected
3. For custom table structures, add a specific extraction function

## Related Documentation

- `docs/dynamic-room-engine-descriptor-overview.md` - How descriptors work
- `docs/archive/version-notes/v0.38_descriptor_framework_integration.md` - Framework guide
- `docs/templates/flavor-text/README.md` - Flavor text templates
