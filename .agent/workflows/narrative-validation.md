# Narrative Validation Workflow

Use this workflow when validating narrative or descriptive content against the 9-domain setting fundamentals.

## Quick Reference

The modular validation checks are located in `.validation/checks/`:

| Domain | Focus | Severity |
|--------|-------|----------|
| 1. Cosmology | Nine Realms, deck architecture | P2-HIGH |
| 2. Timeline | PG notation, chronology | P2-HIGH |
| 3. Magic | Aetheric terminology | P1-CRITICAL |
| 4. Technology | Medieval baseline | P1-CRITICAL |
| 5. Species | Fauna/flora codex | P3-MEDIUM |
| 6. Entities | Characters, factions | P2-HIGH |
| 7. Reality | Paradox, CPS, Blight | P1-CRITICAL |
| 8. Geography | Realm biomes | P2-HIGH |
| 9. Counter-Rune | Eihwaz restrictions | P1-CRITICAL |

## Workflow Steps

1. **Identify content type** to determine applicable domains
2. **Run validation script** or manually review against domain checks
3. **Document violations** with quoted examples
4. **Apply remediation** per domain-specific guidance
5. **Re-validate** to confirm fixes

## Running Validation

```bash
# Single file, all domains
.validation/validate.sh <file>

# Single file, specific domain
.validation/validate.sh --domain=magic <file>

# Batch directory
.validation/validate.sh --batch <directory>
```

## Domain Selection by Content Type

| Content | Domains to Check |
|---------|------------------|
| Room templates | 1, 4, 5, 7, 8 |
| Dialogue trees | 2, 3, 6, 9 |
| Biome definitions | 1, 4, 5, 7, 8 |
| Flavor text | 3, 4, 7 |
| Ability specs | 3, 4, 7, 9 |
| Status effects | 3, 7 |
| World lore | All domains |

## Integration with QA Workflow

This workflow complements the spec structure validation in `qa-spec.md`:

1. **First:** Validate spec structure (frontmatter, sections)
2. **Then:** Validate narrative content (this workflow)

Both must pass for content to be considered conformant.
