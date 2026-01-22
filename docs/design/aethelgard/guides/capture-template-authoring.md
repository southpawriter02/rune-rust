# Capture Template Authoring Guide

> **Audience:** Lore writers, content designers, world-builders
> **No programming required** - just JSON editing
> **Version:** v0.3.25d

---

## Essential Reading First

Before creating templates, review these documents:

1. **[Sample Codex Entries](../design/09-data/sample-codex-entries.md)** - Contains:
   - Complete multi-fragment entry examples (The Ginnungagap Glitch, The Forlorn, etc.)
   - Writer's Guide with voice-by-type guidelines
   - Placement difficulty progression
   - Content principles (progressive revelation, mystery preservation)

2. **[SPEC-CODEX-001](../specs/knowledge/SPEC-CODEX-001.md)** - Contains:
   - How fragments link to Codex Entries
   - Unlock thresholds (25%, 50%, 75%, 100%)
   - Entry categories (Bestiary, Factions, Technical, etc.)

---

## Quick Start

1. Open `data/capture-templates/categories/` in your editor
2. Find the category that matches your content (e.g., `ancient-ruin.json`)
3. Add a new template object to the `templates` array
4. Run validation: `pwsh scripts/validate-templates.ps1`
5. Submit a pull request

---

## Template Structure

Every template has these fields:

| Field | Required | Description | Example |
|-------|----------|-------------|---------|
| `id` | Yes | Unique kebab-case identifier | `"ruin-sealed-chamber"` |
| `type` | Yes | Type of capture (see list below) | `"TextFragment"` |
| `fragmentContent` | Yes | The lore text (20-1000 characters) | `"Wall inscriptions reveal..."` |
| `source` | Yes | How/where it was discovered | `"Wall inscription"` |
| `matchKeywords` | Optional | Keywords for Codex linking | `["ruin", "ancient"]` |
| `quality` | Optional | Legend reward value (default: 15) | `15` or `30` |
| `tags` | Optional | Metadata for filtering | `["aesir", "pre-glitch"]` |

---

## Capture Types

Choose the type that best fits how the information would be discovered:

| Type | When to Use | Example Context |
|------|-------------|-----------------|
| `TextFragment` | Written records, notes, torn pages | "A crumpled note with faded writing..." |
| `EchoRecording` | Audio logs, transcribed recordings | "A damaged echo-stone preserves a fragment..." |
| `VisualRecord` | Schematics, diagrams, murals, images | "Murals cover what remains of the walls..." |
| `Specimen` | Biological samples, tissue analysis | "Tissue samples reveal extensive mutation..." |
| `OralHistory` | Dialogue, rumors, oral traditions | "The veterans speak of it in hushed tones..." |
| `RunicTrace` | Magical/technological residue | "Faint Aetheric resonance detected..." |

---

## Categories

Templates are organized by discovery context:

| Category File | Use For |
|---------------|---------|
| `rusted-servitor.json` | Automata, machines, servitors |
| `blighted-creature.json` | Corrupted creatures, Blight-infected |
| `industrial-site.json` | Forges, factories, mechanisms |
| `ancient-ruin.json` | Ruins, inscriptions, temples |
| `generic-container.json` | Chests, crates, storage |
| `field-guide-triggers.json` | Tutorial/mechanic discovery |

---

## Voice by Capture Type (Summary)

> **Full guide:** [sample-codex-entries.md Writer's Guide](../design/09-data/sample-codex-entries.md#writers-guide-creating-data-captures)

| Type | Voice Style | Example Opening |
|------|-------------|-----------------|
| **TextFragment** | Scholarly, formal, historical | *"The records indicate..."* |
| **EchoRecording** | Personal, urgent, often corrupted | *"[STATIC] This is..."* |
| **VisualRecord** | Descriptive, technical, observational | *"The schematic depicts..."* |
| **Specimen** | Analytical, clinical, scientific | *"Analysis reveals..."* |
| **OralHistory** | Conversational, dialectal, personal | *"My grandmother told me..."* |
| **RunicTrace** | Mystical, precise, layered | *"The residual pattern suggests..."* |

---

## How Fragments Become Codex Entries

> **Detailed in:** [SPEC-CODEX-001](../specs/knowledge/SPEC-CODEX-001.md)

Data Capture templates (this system) link to CodexEntries via `matchKeywords`:

```
+---------------------+       +---------------------+
|  CaptureTemplate    |       |    CodexEntry       |
+---------------------+       +---------------------+
| matchKeywords: [    |------>| Title: "The Forlorn"|
|   "forlorn",        |       | TotalFragments: 4   |
|   "einherjar"       |       | UnlockThresholds:   |
| ]                   |       |   25%: NAME_REVEALED|
+---------------------+       |   50%: BEHAVIOR     |
                              |   75%: WEAKNESS     |
                              |   100%: FULL_ENTRY  |
                              +---------------------+
```

**Your job:** Create individual fragments. The system links them to entries.

---

## Step-by-Step: Adding a Template

### 1. Open the Category File

```bash
# In VS Code, Cursor, or any text editor
code data/capture-templates/categories/ancient-ruin.json
```

### 2. Locate the Templates Array

```json
{
  "category": "ancient-ruin",
  "version": "1.0.0",
  "templates": [
    // Existing templates here...

    // ADD YOUR NEW TEMPLATE HERE
  ]
}
```

### 3. Add Your Template

```json
{
  "id": "ruin-sealed-door",
  "type": "TextFragment",
  "fragmentContent": "A warning carved deep into the stone: 'Beyond this threshold, the Sleepers remain. Do not disturb what was sealed by necessity, not choice.'",
  "source": "Door inscription",
  "matchKeywords": ["ruin", "ancient", "sleeper"],
  "quality": 15,
  "tags": ["mystery", "warning"]
}
```

### 4. Validate Your Work

```bash
pwsh scripts/validate-templates.ps1
```

Expected output:
```
=== Capture Template Validator ===
Validating: ancient-ruin.json
  [OK] 4 templates found

=== Summary ===
Total templates: 20
[OK] All templates valid!
```

### 5. Submit for Review

```bash
git add data/capture-templates/
git commit -m "feat(lore): add sealed-door ruin template"
git push origin feature/new-lore
# Open pull request
```

---

## Domain 4 Compliance Checklist

> **CRITICAL:** Post-Glitch inhabitants cannot perform precision measurements.
> Your lore text must NOT contain:

### Forbidden

| Category | Bad Example | Why |
|----------|-------------|-----|
| **Percentages** | "95% casualties" | Requires census/statistics |
| **Decimals** | "18.5C" | Requires thermometers |
| **Exact Distances** | "800 meters away" | Requires surveying tools |
| **Population Counts** | "10,000 residents" | Requires census records |
| **Gas Concentrations** | "H2S 6-12 ppm" | Requires analyzers |
| **Technical Specs** | "2.4 GHz frequency" | Modern terminology |

### Allowed Alternatives

| Instead Of | Write |
|------------|-------|
| "95% casualties" | "nearly all perished" |
| "18.5C" | "uncomfortably warm" / "hot enough to blister" |
| "800 meters" | "beyond bowshot" / "a hard day's walk" |
| "10,000 residents" | "a thriving settlement" / "thousands" |
| "6-12 ppm" | "air thick with sulfurous stench" |
| "2.4 GHz" | "a persistent hum that sets teeth on edge" |

### Medieval Measurement Reference

| Modern | Medieval Equivalent |
|--------|---------------------|
| 1-5 meters | "arm's reach," "a few paces" |
| 10-20 meters | "javelin-throw distance" |
| 50-100 meters | "bowshot range" |
| 1+ kilometers | "a hard morning's walk" |
| Seconds | "a few heartbeats" |
| Minutes | "while a candle burns a finger's width" |
| Hours | "from dawn to midday" |

---

## AAM-VOICE Layer 2 Reference

Data Captures use **Layer 2: Diagnostic/Observational** voice:

### Characteristics

- **Sensory-first:** Describe what characters SEE, SMELL, HEAR, FEEL
- **No explanations:** Characters don't know WHY things work
- **Medieval vocabulary:** No modern/technical terms
- **Pragmatic mysticism:** "The rune works" not "the algorithm executes"
- **Grit over glamour:** Everything is weathered, repaired, jury-rigged

### Voice Examples

**Technical (Avoid):**
> "The terminal's display flickered as corrupted data scrolled across the screen."

**Medieval-flavored (Preferred):**
> "The oracle-box shuddered, its glass face swimming with ghost-light and symbols no sage could parse."

### Translation Guide

| Don't Write | Write Instead |
|-------------|---------------|
| "Emergency bypass terminal" | "The humming altar" |
| "Power core" | "The heart-stone" |
| "Database" | "The memory-well" |
| "Algorithm" | "The pattern" |
| "Malfunction" | "Curse" or "blight" |
| "Synthetic" | "Dvergr-made" or "Old World craft" |
| "Robot" | "Automaton" or "iron-walker" |
| "Computer" | "Oracle-box" or "rune-mirror" |

---

## Example Template: Annotated Walkthrough

```json
{
  "id": "servitor-emergency-protocol",
  "type": "EchoRecording",
  "fragmentContent": "A damaged echo-stone crackles to life: '...Unit designate Gamma-Seven. Emergency protocols enacted. All personnel evacuate to designated muster points. This is not a drill. Repeat: the containment field has--' The voice dissolves into static.",
  "source": "Echo-stone recovery",
  "matchKeywords": ["servitor", "automaton", "aesir", "emergency"],
  "quality": 15,
  "tags": ["aesir", "pre-glitch", "emergency", "containment"]
}
```

**Field-by-field breakdown:**

| Field | Value | Notes |
|-------|-------|-------|
| `id` | `"servitor-emergency-protocol"` | Kebab-case, pattern: [category]-[subject]-[detail] |
| `type` | `"EchoRecording"` | Audio log discovery |
| `fragmentContent` | (lore text) | Sensory, medieval vocab, no precision measurements |
| `source` | `"Echo-stone recovery"` | Shown in Codex as context |
| `matchKeywords` | `["servitor", "automaton", "aesir", "emergency"]` | Links to Codex entries |
| `quality` | `15` | Standard (15) or Specialist (30) |
| `tags` | `["aesir", "pre-glitch", ...]` | Metadata for filtering |

---

## NotebookLM to JSON Workflow

If you've drafted lore in NotebookLM, follow this pipeline:

### 1. Export from NotebookLM

- Open your NotebookLM source
- Copy the lore text you want to convert
- Note the context (creature, location, discovery method)

### 2. Create Template Skeleton

```json
{
  "id": "",
  "type": "",
  "fragmentContent": "",
  "source": "",
  "matchKeywords": [],
  "quality": 15,
  "tags": []
}
```

### 3. Fill In Fields

1. **id:** Create from context: `"blight-fungal-network"`
2. **type:** Match discovery: examining creature = `"Specimen"`
3. **fragmentContent:** Paste and refine for Domain 4 compliance
4. **source:** Where found: `"Biological analysis"`
5. **matchKeywords:** Extract key nouns: `["blight", "fungal", "creature"]`
6. **tags:** Add metadata: `["horror", "mutation"]`

### 4. Domain 4 Pass

Read through `fragmentContent` and check for:
- [ ] No percentages or decimals
- [ ] No metric measurements
- [ ] No modern terminology
- [ ] Sensory descriptions only
- [ ] Medieval vocabulary

### 5. Validate and Commit

```bash
pwsh scripts/validate-templates.ps1
git add data/capture-templates/
git commit -m "feat(lore): add blight-fungal-network template"
```

---

## Troubleshooting

### Validation Error: "Missing 'id'"

```
Error: ancient-ruin.json: Template missing 'id'
```

**Fix:** Add a unique `id` field to your template:
```json
"id": "ruin-your-template-name"
```

### Validation Error: "Invalid type"

```
Warning: Template 'my-template' has invalid type 'TextFrag'
```

**Fix:** Use exact enum values (case-sensitive):
- `TextFragment`
- `EchoRecording`
- `VisualRecord`
- `Specimen`
- `OralHistory`
- `RunicTrace`

### Validation Warning: "Domain 4 violation"

```
Warning: Template 'ruin-measurements' may violate Domain 4 (precision measurements)
```

**Fix:** Review your `fragmentContent` for percentages, decimals, or metric units. Replace with qualitative descriptions.

### JSON Parse Error

```
Error: ancient-ruin.json: JSON parse error - Unexpected token
```

**Possible causes:**
- Missing comma between templates
- Missing closing brace `}`
- Unescaped quotes in text (use `\"` or `'`)
- Trailing comma after last template

**Tip:** Use a JSON validator like [jsonlint.com](https://jsonlint.com)

---

## Review Process

All new templates go through this process:

1. **Self-Review:** Run validation script
2. **Domain 4 Check:** Use checklist above
3. **Voice Check:** Match Layer 2 guidelines
4. **Pull Request:** Submit for team review
5. **Content Review:** Lore lead checks for consistency
6. **Merge:** Added to next build

---

## FAQ

**Q: Can I create a new category?**
A: Yes, but coordinate with the dev team first. They need to add keywords to `CategoryKeywords` in `DataCaptureService.cs`.

**Q: What if my template doesn't fit any category?**
A: Use `generic-container.json` as a catch-all, or request a new category.

**Q: How long should `fragmentContent` be?**
A: Between 20 and 1000 characters. Aim for 100-300 for readability.

**Q: Do keywords matter?**
A: Yes! They link captures to Codex entries. Use nouns that match entry titles.

**Q: Can I include dialogue?**
A: Yes! Use single quotes inside the JSON string:
```json
"fragmentContent": "The veteran warned: 'Never trust a Servitor that still moves.'"
```

---

## Related Documentation

These existing documents provide essential context for content authors:

| Document | Contains |
|----------|----------|
| **[sample-codex-entries.md](../design/09-data/sample-codex-entries.md)** | Complete entry examples, Writer's Guide, placement difficulty |
| **[SPEC-CODEX-001](../specs/knowledge/SPEC-CODEX-001.md)** | System architecture, unlock thresholds, categories |
| **[DOCUMENTATION_STANDARDS.md](../00-project/DOCUMENTATION_STANDARDS.md)** | Domain 4 measurement rules, voice guidelines |

---

## Need Help?

- **Discord:** #lore-development channel
- **Complete Examples:** `docs/design/09-data/sample-codex-entries.md`
- **Schema Reference:** `data/capture-templates/schema/capture-template.schema.json`
