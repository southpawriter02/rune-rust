# Capture Templates

> **For content authors:** See the full [Capture Template Authoring Guide](../../docs/guides/capture-template-authoring.md)

This directory contains externalized Data Capture templates for the Rune & Rust knowledge system.

## Directory Structure

```
capture-templates/
├── schema/
│   └── capture-template.schema.json    # JSON Schema definition
├── categories/
│   ├── rusted-servitor.json            # Servitor/automaton templates
│   ├── generic-container.json          # Container search templates
│   ├── blighted-creature.json          # Blight-corrupted creature templates
│   ├── industrial-site.json            # Industrial location templates
│   ├── ancient-ruin.json               # Ancient ruin templates
│   └── field-guide-triggers.json       # Gameplay event triggers
└── README.md                           # This file
```

## Quick Reference

### CaptureType Values

| Type | Description | Voice Style |
|------|-------------|-------------|
| `TextFragment` | Written records, notes, torn pages | Scholarly, formal, historical |
| `EchoRecording` | Audio logs, transcribed recordings | Personal, urgent, often corrupted |
| `VisualRecord` | Schematics, diagrams, images | Descriptive, technical, observational |
| `Specimen` | Biological samples, tissue analysis | Analytical, clinical, scientific |
| `OralHistory` | Dialogue fragments, rumors, traditions | Conversational, dialectal, personal |
| `RunicTrace` | Magical/technological trace analysis | Mystical, precise, layered |

### Domain 4 Compliance (CRITICAL)

All `fragmentContent` must follow Domain 4 constraints. POST-Glitch societies are archaeologists, not engineers.

| Forbidden | Example | Correction |
|-----------|---------|------------|
| Exact percentages | "95% casualties" | "nearly all perished" |
| Decimal precision | "18.5C temperature" | "uncomfortably warm" |
| Metric measurements | "800 meters away" | "beyond bowshot" |
| Population counts | "10,000 residents" | "a thriving settlement" |
| Technical specifications | "H2S 6-12 ppm" | "air thick with sulfurous stench" |

## Adding New Templates

### 1. Choose a Category File

Templates are organized by encounter context:
- **rusted-servitor.json** - Mechanical/automaton encounters
- **generic-container.json** - Container/chest searches
- **blighted-creature.json** - Corrupted creature encounters
- **industrial-site.json** - Factory/forge locations
- **ancient-ruin.json** - Pre-Glitch ruins and tombs
- **field-guide-triggers.json** - Gameplay mechanic discoveries

### 2. Template Structure

```json
{
  "id": "unique-template-id",
  "type": "TextFragment",
  "fragmentContent": "The lore text content. Must be 20-1000 characters and Domain 4 compliant.",
  "source": "Where the capture was found",
  "matchKeywords": ["keyword1", "keyword2"],
  "quality": 15,
  "tags": ["optional", "metadata"]
}
```

### 3. Required Fields

| Field | Description |
|-------|-------------|
| `id` | Unique kebab-case identifier (e.g., `servitor-fungal-infection`) |
| `type` | One of the six CaptureType values above |
| `fragmentContent` | The actual lore text (20-1000 chars) |
| `source` | Discovery context shown in UI (5-100 chars) |

### 4. Optional Fields

| Field | Default | Description |
|-------|---------|-------------|
| `matchKeywords` | Category default | Keywords for Codex matching |
| `quality` | 15 | Legend reward value (1-100) |
| `tags` | [] | Metadata tags for filtering |
| `domain4Notes` | null | Author notes (not in-game) |

## Validation

Run the validation script before committing:

```powershell
pwsh scripts/validate-templates.ps1
```

Expected output for valid templates:
```
=== Capture Template Validator ===
Loaded schema: data/capture-templates/schema/capture-template.schema.json

Validating: rusted-servitor.json
  OK 4 templates found
...

=== Summary ===
Total template files: 6
Total templates: 19

OK All templates valid!
```

## AAM-VOICE Layer 2 Quick Reference

Data Captures use medieval-flavored vocabulary:

| Don't Write | Write Instead |
|-------------|---------------|
| "Emergency bypass terminal" | "The humming altar" |
| "Power core" | "The heart-stone" |
| "Database" | "The memory-well" |
| "Robot" | "Automaton" or "iron-walker" |
| "Computer" | "Oracle-box" or "rune-mirror" |

## Related Documentation

- [Capture Template Authoring Guide](../../docs/guides/capture-template-authoring.md) - Full authoring guide for content creators
- [SPEC-CODEX-001](../../docs/specs/knowledge/SPEC-CODEX-001.md) - Codex system architecture
- [Sample Codex Entries](../../docs/design/09-data/sample-codex-entries.md) - Lore content examples
- [CaptureType.cs](../../RuneAndRust.Core/Enums/CaptureType.cs) - Enum source of truth
