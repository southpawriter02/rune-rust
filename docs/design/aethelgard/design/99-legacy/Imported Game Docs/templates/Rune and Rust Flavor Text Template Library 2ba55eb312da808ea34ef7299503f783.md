# Rune and Rust: Flavor Text Template Library

Sub-item: Puzzles Template (Puzzles%20Template%202ba55eb312da803b9578eaf3b4caad1b.md), NPC Flavor Text Template (NPC%20Flavor%20Text%20Template%202ba55eb312da8022a691c276b6a12b71.md), Magic (Galdr) Flavor Text Template (Magic%20(Galdr)%20Flavor%20Text%20Template%202ba55eb312da8004b170cf82099d369f.md), Interactive Elements Template (Interactive%20Elements%20Template%202ba55eb312da80348a6ac8b0a2b03b6d.md), Hazards & Environmental Features Template (Hazards%20&%20Environmental%20Features%20Template%202ba55eb312da80cf8034c145223d5439.md), Combat Flavor Text Template (Combat%20Flavor%20Text%20Template%202b955eb312da80e2aea3ea99de4e0425.md), Biome Descriptor Template (Biome%20Descriptor%20Template%202b955eb312da805c84e4e04b6a43d813.md), Trauma & Psychological Flavor Text Template (Trauma%20&%20Psychological%20Flavor%20Text%20Template%202ba55eb312da805aba21cb79d0b72939.md), Status Effects Flavor Text Template (Status%20Effects%20Flavor%20Text%20Template%202ba55eb312da80bd83e9db2044ca2e3c.md), Specialization & Abilities Flavor Text Template (Specialization%20&%20Abilities%20Flavor%20Text%20Template%202ba55eb312da8046ac3ef86104522951.md), Skill Checks & Examination Flavor Text Template (Skill%20Checks%20&%20Examination%20Flavor%20Text%20Template%202ba55eb312da80dc8c00c744e033f09b.md), Sensory & Atmospheric Descriptor Template (Sensory%20&%20Atmospheric%20Descriptor%20Template%202ba55eb312da80bdbe4de81d5440c613.md)

This directory contains comprehensive templates for generating consistent, immersive flavor text across all setting elements in Rune and Rust. These templates serve as guides for creating new content that maintains the game's atmospheric tone and narrative voice.

---

## Quick Navigation

| Template | Purpose | Primary Use |
| --- | --- | --- |
| [Biome Descriptors](https://www.notion.so/biome-descriptors.md) | Environment descriptions by biome | Room generation, exploration |
| [Sensory & Atmospheric](https://www.notion.so/sensory-atmospheric.md) | Ambient sounds, smells, details | Atmosphere building |
| [Combat Flavor](https://www.notion.so/combat-flavor.md) | Combat actions and reactions | Battle narration |
| [Hazards & Environmental](https://www.notion.so/hazards-environmental.md) | Environmental dangers | Exploration, survival |
| [Interactive Elements](https://www.notion.so/interactive-elements.md) | Mechanisms, containers, barriers | Object interaction |
| [Puzzles](https://www.notion.so/puzzles.md) | Environmental challenges | Problem-solving gameplay |
| [Specialization Abilities](https://www.notion.so/specialization-abilities.md) | Class-specific powers | Character abilities |
| [Trauma & Psychological](https://www.notion.so/trauma-psychological.md) | Stress, trauma, madness | Horror atmosphere |
| [NPC Flavor](https://www.notion.so/npc-flavor.md) | Characters and dialogue | Social interaction |
| [Magic (Galdr)](https://www.notion.so/magic-galdr.md) | Runic spellcasting | Magic system |
| [Status Effects](https://www.notion.so/status-effects.md) | Buffs, debuffs, conditions | Combat mechanics |
| [Skill Checks & Examination](https://www.notion.so/skill-examination.md) | Skill use and investigation | Core gameplay |

---

## Setting Overview

### The World

Rune and Rust takes place in the decaying remnants of a once-great Dvergr (dwarf) civilization, 800 years after a catastrophic event known as **The Silence**. Players explore procedurally-generated dungeons across five distinct biomes, each with unique atmospheric qualities.

### Biomes at a Glance

| Biome | Theme | Emotional Core | Key Hazards |
| --- | --- | --- | --- |
| **The Roots** | Industrial decay | Oppressive dread | Toxic fumes, electrical, collapse |
| **Muspelheim** | Volcanic fury | Primal terror | Fire, extreme heat, lava |
| **Niflheim** | Eternal frost | Creeping despair | Cold, ice, isolation |
| **Alfheim** | Reality distortion | Madness | Paradox, Cursed Choir, glitches |
| **Jotunheim** | Giant-scale ruins | Awe, insignificance | Scale hazards, ancient traps |

### Core Themes

- **Decay and entropy** - everything is failing, rusting, collapsing
- **Isolation** - the world is empty, hostile, indifferent
- **Corruption** - machines and minds have been twisted
- **Survival** - resources are scarce, danger is constant
- **Mystery** - the truth of The Silence awaits discovery

---

## Template Structure

Each template follows a consistent structure:

### 1. Overview Section

- Purpose and scope of the template
- Quick reference tables
- Categorization systems

### 2. Template Definitions

- Standardized format for entries
- Required and optional fields
- Variable placeholders

### 3. Category-Specific Content

- Domain-specific templates
- Example entries
- Variation guidelines

### 4. Writing Guidelines

- Tone and voice guidance
- Word choice recommendations
- Common pitfalls to avoid

### 5. Quality Checklist

- Pre-submission verification points
- Consistency checks
- Technical requirements

---

## Common Template Elements

### Standard Field Structure

```markdown
CATEGORY: [Type]
Biome: [The_Roots | Muspelheim | Niflheim | Alfheim | Jotunheim | Universal]
Subcategory: [Specific type]
Intensity: [Subtle | Moderate | Oppressive]
Weight: [0.5-2.0]
Tags: ["Tag1", "Tag2", "Tag3"]

DESCRIPTOR TEXT:
[Content - 15-60 words depending on category]

```

### Intensity Levels

| Level | Meaning | Usage |
| --- | --- | --- |
| **Subtle** | Background, easily missed | Frequent, ambient |
| **Moderate** | Noticeable, contributes to mood | Standard usage |
| **Oppressive** | Demands attention, emotionally impactful | Key moments, dangers |

### Weighting System

| Weight | Frequency | Use For |
| --- | --- | --- |
| 0.5 | Rare | Special, memorable moments |
| 1.0 | Standard | Normal frequency |
| 1.5 | Common | Desired more often |
| 2.0 | Frequent | Core, essential content |

---

## Writing Principles

### Voice and Tone

**Do:**

- Use active voice
- Engage multiple senses
- Be specific, not generic
- Maintain biome consistency
- Create emotional resonance

**Avoid:**

- Passive voice
- Generic descriptions
- Modern slang/anachronisms
- Breaking immersion with game terms
- Excessive humor in serious contexts

### Mechanical Integration

When content has mechanical effects, use **[brackets]** for clarity:

```
"The flames engulf you! [Take 2d6 fire damage, Burning status applied]"

```

### Length Guidelines

| Content Type | Word Count |
| --- | --- |
| Brief descriptor | 15-25 words |
| Standard descriptor | 25-40 words |
| Elaborate descriptor | 40-60 words |
| Dialogue line | 10-30 words |
| Lore entry | 50-150 words |

---

## Integration with SQL Data

These templates are designed to generate content compatible with the existing SQL descriptor tables (v0.38+). When creating new entries:

1. **Match the schema** - ensure fields align with table columns
2. **Use valid enums** - biome names, categories must match exactly
3. **Include required fields** - don't omit mandatory columns
4. **Maintain tag consistency** - use established tag vocabulary

### Key SQL Tables

| Table | Template |
| --- | --- |
| `Ambient_Sound_Descriptors` | [Sensory & Atmospheric](https://www.notion.so/sensory-atmospheric.md) |
| `Ambient_Smell_Descriptors` | [Sensory & Atmospheric](https://www.notion.so/sensory-atmospheric.md) |
| `Combat_Defensive_Action_Descriptors` | [Combat Flavor](https://www.notion.so/combat-flavor.md) |
| `Combat_Stance_Descriptors` | [Combat Flavor](https://www.notion.so/combat-flavor.md) |
| `Environmental_Hazard_Descriptors` | [Hazards & Environmental](https://www.notion.so/hazards-environmental.md) |
| `Interactive_Object_Descriptors` | [Interactive Elements](https://www.notion.so/interactive-elements.md) |
| `NPC_Reaction_Descriptors` | [NPC Flavor](https://www.notion.so/npc-flavor.md) |
| `Galdr_Action_Descriptors` | [Magic (Galdr)](https://www.notion.so/magic-galdr.md) |
| `Status_Effect_Descriptors` | [Status Effects](https://www.notion.so/status-effects.md) |
| `Trauma_Descriptors` | [Trauma & Psychological](https://www.notion.so/trauma-psychological.md) |
| `Skill_Check_Descriptors` | [Skill Checks & Examination](https://www.notion.so/skill-examination.md) |

---

## Workflow Recommendations

### Creating New Content

1. **Identify category** - which template applies?
2. **Review existing content** - check SQL data for patterns
3. **Draft using template** - follow the structure
4. **Verify consistency** - biome, tone, mechanics
5. **Test integration** - ensure SQL compatibility
6. **Quality check** - use the template's checklist

### Expanding Coverage

Priority areas for new content:

- [ ]  More biome-specific variations
- [ ]  Additional enemy voice profiles
- [ ]  Expanded puzzle types
- [ ]  Trauma manifestation variety
- [ ]  NPC archetype expansion

---

## Version History

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | Nov 2025 | Initial template library creation |

---

## Contributing

When adding new templates or expanding existing ones:

1. Maintain the established format
2. Include comprehensive examples
3. Add to this README's navigation
4. Update the SQL integration section if new tables are involved
5. Follow the writing principles consistently

---

## Related Documentation

- `/docs/v0.38_descriptor_framework_integration.md` - Technical framework details
- `/Data/v0.38.*_*.sql` - Existing descriptor data
- `/RuneAndRust.Engine/Services/*FlavorTextService.cs` - Service implementations
- `/RuneAndRust.Core/Descriptors/` - Core descriptor models

---

## Support

For questions about these templates or the flavor text system:

- Review the relevant service implementation in `/RuneAndRust.Engine/`
- Check the SQL schema files in `/Data/`
- Consult existing content for patterns and precedent