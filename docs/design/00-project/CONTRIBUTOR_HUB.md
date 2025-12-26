# Contributor Hub

> **Start Here** — The central dispatch for all Rune & Rust contributors (Human & AI).

---

## 1. Quick Navigation

| Resource | Purpose |
|----------|---------|
| [Documentation Standards](./DOCUMENTATION_STANDARDS.md) | **The Law**. Formatting, gold standards, and rules. |
| [Templates Library](../.templates/README.md) | Source files for creating new specs. |
| [Glossary](./GLOSSARY.md) | Terminology definitions. |
| [Overview](./OVERVIEW.md) | High-level game vision and pillars. |
| [Architecture](./ARCHITECTURE.md) | Technical stack and code organization. |

---

## 2. Setting Context (Required Reading)

**Rune & Rust** is a **post-apocalyptic, Nordic-themed RPG** set in **Aethelgard**—800 years after the Ginnungagap Glitch destroyed all understanding of technology.

### Thematic Pillars

| Pillar | Expression |
|--------|------------|
| **Cargo Cult Technology** | Characters interact with tech they cannot comprehend |
| **Survival Horror** | Resources are scarce, death is permanent-ish, corruption accumulates |
| **Nordic Fatalism** | Doom is coming; glory is in how you face it |
| **Body Horror** | Corruption physically transforms characters |
| **Found Family** | Party bonds matter; isolation is death |

### Core Concepts

- **Lost Knowledge**: Inhabitants have lost all understanding of WHY technology works—only that certain patterns produce results
- **Bodge Culture**: Technology is maintained through trial-and-error "bodging," not engineering
- **Runic Mysticism**: Elder Futhark runes produce real effects via Aetheric energy, but principles are forgotten
- **Automata**: Ancient machines (Dvergr constructs, Jötun engines) still roam the ruins

> **Reference:** [`.agent/rules/setting-context.md`](../../.agent/rules/setting-context.md)

---

## 3. Narrative Voice Guidelines

All content must maintain the **Post-Glitch voice**—sensory, gritty, and medieval-flavored.

### Vocabulary Replacement (MANDATORY)

| Modern Concept | ❌ FORBIDDEN | ✅ USE INSTEAD |
|----------------|-------------|----------------|
| Computing | terminal, computer, program | spirit-slate, oracle-box, inscribe |
| Energy | electricity, voltage, power core | lightning, spark, heart-stone |
| Materials | polymer, plastic, synthetic | resin, false-bone, old-shell |
| Radiation | radiation, radioactive | sickness-light, blight-heat |
| Biology | cellular, DNA, genetic | living weave, blood-record, flesh-pattern |
| Repair | repair, fix, optimize | mend, heal, appease, attune |
| Precision | 10%, 500 meters, 20°C | a tithe, a long walk, blood-warm |

### Magic System Terms (CRITICAL)

| ❌ FORBIDDEN | ✅ USE INSTEAD |
|-------------|----------------|
| magic, magical | Aetheric, runic |
| spell, spellcasting | Galdr, runic invocation, inscription |
| mana | Aether, Aetheric reserves |
| wizard, sorcerer | Runasmidr, Galdr-singer |
| enchanted | inscribed, rune-marked |
| arcane | Aetheric, Pre-Glitch |

> **Key Rule:** All Aetheric effects MUST proceed from a **runic focal point** (inscription, Galdr, or rune-marked object). NO spontaneous magic.

> **References:**
> - [`.agent/rules/narrative-voice.md`](../../.agent/rules/narrative-voice.md)
> - [`.agent/rules/magic-system.md`](../../.agent/rules/magic-system.md)

---

## 4. AI Personas (Agent Modes)

When working with AI assistants, specify which persona/mode to use:

| Persona | Role | When to Use |
|---------|------|-------------|
| **Narrator** | Creative Writer / Lore Expert | Flavor text, voice guidance, descriptions |
| **Validator** | QA / Compliance Officer | Checking domain violations, spec conformance |
| **Developer** | Software Engineer / Architect | C# implementation, tests, database design |
| **Worldbuilder** | System Designer / Content Creator | New specs, mechanics, balance data |

### Persona Files

- [`docs/ai-workflows/personas/narrator.md`](../ai-workflows/personas/narrator.md) — Cargo Cult voice, vocabulary rules
- [`docs/ai-workflows/personas/validator.md`](../ai-workflows/personas/validator.md) — Domain checks, reporting format
- [`docs/ai-workflows/personas/developer.md`](../ai-workflows/personas/developer.md) — C#/.NET standards, architecture
- [`docs/ai-workflows/personas/worldbuilder.md`](../ai-workflows/personas/worldbuilder.md) — Golden standards, spec structure

---

## 5. Decision Tree: What are you building?

Use this tree to determine which Template and Path to use.

```mermaid
flowchart TD
    START[Start] --> TYPE{What are you creating?}

    %% Branch 1: Character Options
    TYPE --> |Character Option| CHAR{Type?}
    CHAR --> |Specialization| SPEC[Use **specialization.md**<br>Path: `docs/03-character/specializations/{name}/{name}-overview.md`]
    CHAR --> |Ability| ABIL[Use **ability.md**<br>Path: `docs/03-character/specializations/{spec}/abilities/{name}.md`]
    CHAR --> |Skill| SKILL[Use **skill.md**<br>Path: `docs/01-core/skills/{name}.md`]

    %% Branch 2: Systems
    TYPE --> |Game System| SYS{Type?}
    SYS --> |Core Logic| CORE[Use **system.md**<br>Path: `docs/01-core/{name}.md`]
    SYS --> |Resource/Currency| RES[Use **resource.md**<br>Path: `docs/01-core/resources/{name}.md`]
    SYS --> |Status Effect| STAT[Use **status-effect.md**<br>Path: `docs/04-systems/status-effects/{name}.md`]
    SYS --> |Crafting Trade| CRAFT[Use **craft.md**<br>Path: `docs/04-systems/crafting/{name}.md`]

    %% Branch 3: Entities/Enemies
    TYPE --> |Entity/Enemy| ENT{Type?}
    ENT --> |Enemy/Mob| MOB[Use **Workflow C** below<br>Path: `docs/02-entities/enemies/{category}/{name}.md`]
    ENT --> |NPC| NPC[Use entity template<br>Path: `docs/02-entities/npcs/{faction}/{name}.md`]
    ENT --> |Faction| FACTION[Path: `docs/02-entities/factions/{name}.md`]
```

---

## 6. Workflows

### Workflow A: Creating a New Specification

1.  **Ideation**: Ensure the concept fits the [Project Overview](./OVERVIEW.md).
2.  **Selection**: Use the Decision Tree above to find your Template.
3.  **Drafting**:
    *   Copy the template to the target directory.
    *   Fill in YAML frontmatter (`status: draft`).
    *   **Crucial**: Consult [Golden Standards](./DOCUMENTATION_STANDARDS.md#golden-standard-specs) to see the expected depth.
    *   **Crucial**: Fill in the **Balance Data** section (estimates are fine for drafts).
4.  **Formatting Check**:
    *   Ensure all dice are d10 (Resolution) or Tiered d4-d10 (Damage).
    *   Verify all internal links are relative.
5.  **Narrative Validation**: Run Domain checks (see Section 7).
6.  **Review**: Run the [Self-Audit Checklist](#8-checklists) below.
7.  **Commit**: Push with `status: draft`.

---

### Workflow B: Remediation (Fixing Legacy Specs)

*Target: Files in `docs/99-legacy/` or incomplete drafts.*

1.  **Analyze**: Read the legacy file for core concepts.
2.  **Migrate**: Create a **fresh file** using the modern template (do not edit legacy in place).
3.  **Map**: Move content from legacy to the new structure.
    *   *Legacy "Power"* → *Mechanical Effects*
    *   *Legacy "Flavor"* → *Voice Guidance*
4.  **Fill Gaps**: Legacy files often miss **Balance Data** and **Implementation Status**. You must add these.
5.  **Validate**: Run 9-domain validation (Section 7).
6.  **Deprecate**: If a legacy file is fully replaced, update its status or move it to `docs/99-legacy/`.

---

### Workflow C: Entity/Enemy Creation (AI-Assisted)

*For generating enemies, mobs, or creatures from a concept description.*

```mermaid
flowchart TD
    INPUT[User provides description<br>e.g., "frost-touched wolf pack"] --> DESIGN

    subgraph DESIGN [1. Design Phase - Worldbuilder Persona]
        D1[Analyze concept against setting] --> D2[Determine species lineage<br>Domain 5 compliance]
        D2 --> D3[Design 2-4 mob variants<br>Basic, Elite, Alpha, etc.]
        D3 --> D4[Define stat blocks<br>d10 resolution, tiered damage]
    end

    DESIGN --> TEMPLATE

    subgraph TEMPLATE [2. Template Selection]
        T1[Use **enemy.md** template] --> T2{Set Category Field}
        T2 --> |Beast/Creature| BEAST[category: beast]
        T2 --> |Automaton| AUTO[category: automaton]
        T2 --> |Humanoid| HUMAN[category: humanoid]
        T2 --> |Blight-Mutant| BLIGHT[category: blight-creature]
    end

    TEMPLATE --> SPEC

    subgraph SPEC [3. Specification - Worldbuilder Persona]
        S1[Create spec with YAML frontmatter] --> S2[Fill mechanical sections<br>HP, damage, abilities]
        S2 --> S3[Add tactical notes<br>Synergies, counters]
    end

    SPEC --> NARRATIVE

    subgraph NARRATIVE [4. Narrative Layer - Narrator Persona]
        N1[Write Voice Guidance] --> N2[Add flavor text<br>Cargo Cult voice]
        N2 --> N3[Create encounter descriptions]
    end

    NARRATIVE --> VALIDATE

    subgraph VALIDATE [5. Validation - Validator Persona]
        V1[Run Domain 3: Magic terms] --> V2[Run Domain 4: Technology voice]
        V2 --> V3[Run Domain 5: Species lineage]
        V3 --> V4[Run Domain 7: Reality rules]
        V4 --> V5{All domains pass?}
        V5 --> |No| FIX[Fix violations] --> V1
        V5 --> |Yes| DONE[✓ Spec Complete]
    end
```

#### Entity Creation Checklist

- [ ] **Lineage defined**: What was this before the Glitch? (Domain 5)
- [ ] **Mutation vector**: Blight exposure? Gene-Storm? Bio-engineering remnant?
- [ ] **Abilities grounded**: All supernatural traits via Aetheric mechanics (Domain 3)
- [ ] **No forbidden terms**: magic, spell, cellular, radiation, etc. (Domain 3, 4)
- [ ] **Stat block complete**: HP, damage dice (tiered), abilities
- [ ] **Voice Guidance**: Flavor text uses Cargo Cult vocabulary
- [ ] **Tactical notes**: Synergies, counters, encounter recommendations

---

### Workflow D: Narrative Validation

*Run this after any content creation to ensure setting compliance.*

1.  **Identify content type** to determine applicable domains
2.  **Run validation** per domain (see Section 7)
3.  **Document violations** with line numbers and quoted text
4.  **Apply remediation** per domain-specific guidance
5.  **Re-validate** to confirm fixes

---

## 7. Validation: The 9-Domain System

All content must pass the **Modular Narrative Validation Framework**. Violations are graded by severity.

### Domain Quick Reference

| Domain | Focus | Severity | Key Checks |
|--------|-------|----------|------------|
| **1. Cosmology** | Nine Realms, deck architecture | P2-HIGH | No "upper/lower realm", use "deck/sector" |
| **2. Timeline** | PG notation, chronology | P2-HIGH | No AD/CE dates, use PG (Post-Glitch) |
| **3. Magic** | Aetheric terminology | **P1-CRITICAL** | No magic/spell/mana, require runic focal points |
| **4. Technology** | Medieval baseline | **P1-CRITICAL** | No precision measurements, cargo cult verbs |
| **5. Species** | Fauna/flora codex | P3-MEDIUM | Blight-mutation lineage, no generic fantasy |
| **6. Entities** | Characters, factions | P2-HIGH | Faction terms, no wizard/mage |
| **7. Reality** | Paradox, CPS, Blight | **P1-CRITICAL** | No "cured/cleansed", Blight is progressive |
| **8. Geography** | Realm biomes | P2-HIGH | No outdoor/sky references, use deck/corridor |
| **9. Counter-Rune** | Eihwaz restrictions | **P1-CRITICAL** | Apocrypha-level content only |

### Severity Levels

| Level | Meaning | Action |
|-------|---------|--------|
| **P1-CRITICAL** | Blocks canon consistency | **Must fix before merge** |
| **P2-HIGH** | Pervasive pattern violation | Fix in current sprint |
| **P3-MEDIUM** | Isolated violation | Queue for batch remediation |
| **P4-LOW** | Style preference | Optional cleanup |

### Domain Selection by Content Type

| Content Type | Check These Domains |
|--------------|---------------------|
| Room templates | 1, 4, 5, 7, 8 |
| Dialogue trees | 2, 3, 6, 9 |
| Biome definitions | 1, 4, 5, 7, 8 |
| Flavor text | 3, 4, 7 |
| Ability specs | 3, 4, 7, 9 |
| Status effects | 3, 7 |
| Enemy/entity specs | 3, 4, 5, 6, 7 |
| World lore | All domains |

### Running Validation

```bash
# Single file, all domains
.validation/validate.sh <file>

# Single file, specific domain
.validation/validate.sh --domain=magic <file>

# Batch directory
.validation/validate.sh --batch <directory>
```

> **Reference:** [`.validation/config.yaml`](../../.validation/config.yaml) for full domain definitions.

---

## 8. Checklists

### New Spec Creation Checklist

Copy this into your PR or mental buffer:

- [ ] **Location**: File is in the correct directory (see Decision Tree).
- [ ] **Naming**: File name is `kebab-case.md`.
- [ ] **Frontmatter**: YAML block is present with `id`, `title`, `status`.
- [ ] **Sections**: All "Required" sections from Template are present.
    - [ ] Overview Table
    - [ ] Mechanical Effects
    - [ ] Balance Data (Required!)
    - [ ] Voice Guidance (Required!)
    - [ ] Implementation Status
    - [ ] Changelog
- [ ] **Dice**: Resolution is d10; Damage is Tiered (d4-d10). No "2d6" for Greatswords!
- [ ] **Links**: All links work and are relative (`../file.md`, not `/docs/file.md`).

### Pre-Commit Validation Checklist

Before you say "Task Complete":

**Domain 3 (Magic) - P1-CRITICAL:**
- [ ] No "magic", "magical", "spell", "spellcasting"?
- [ ] No "mana", "wizard", "sorcerer", "arcane", "enchanted"?
- [ ] All Aetheric effects have runic focal points?

**Domain 4 (Technology) - P1-CRITICAL:**
- [ ] No precision measurements (percentages, exact distances, temperatures)?
- [ ] No "cellular", "polymer", "electricity", "radiation"?
- [ ] No "program", "calibrate", "optimize", "download"?

**Domain 7 (Reality) - P1-CRITICAL:**
- [ ] No "cured the paradox", "cleansed the blight", "healed from CPS"?
- [ ] Blight portrayed as progressive/irreversible?

**General:**
- [ ] Deleted all placeholders (e.g., `[Insert Description]`)?
- [ ] Compared work against the **Golden Standard** for that category?

---

## 9. Tools & Automation

*   **Validation**: Run `.validation/validate.sh` to check for domain violations.
*   **Templates**: Always pull fresh from `docs/.templates/` to ensure you have the latest version.
*   **AI Prompts**: Use prompts in `docs/ai-workflows/prompts/` for consistent generation.
*   **Automation Scripts**: See `docs/ai-workflows/automation/` for batch processing.

---

## 10. Golden Standards Reference

When creating content, compare against these fully-conformant examples:

| Category | Golden Standard | Path | Lines |
|----------|-----------------|------|-------|
| **Specialization** | Berserkr | `docs/03-character/specializations/berserkr/berserkr-overview.md` | ~320 |
| **Ability** | Corridor Maker | `docs/03-character/specializations/ruin-stalker/abilities/corridor-maker.md` | ~226 |
| **Status Effect** | Bleeding | `docs/04-systems/status-effects/bleeding.md` | ~284 |

If your spec is significantly shorter than the golden standard, it likely lacks depth.

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────────────────────┐
│  RUNE & RUST - QUICK REFERENCE                                  │
├─────────────────────────────────────────────────────────────────┤
│  SETTING: Post-apocalyptic Nordic, Cargo Cult tech              │
│  DICE: d10 resolution | d4-d10 tiered damage                    │
│  VOICE: Medieval-flavored, sensory, no modern terms             │
├─────────────────────────────────────────────────────────────────┤
│  FORBIDDEN TERMS (P1-CRITICAL):                                 │
│  • magic, spell, mana, wizard, arcane, enchanted                │
│  • cellular, polymer, electricity, radiation                    │
│  • cured, cleansed, healed (for Blight/CPS)                     │
│  • program, calibrate, optimize, download                       │
├─────────────────────────────────────────────────────────────────┤
│  USE INSTEAD:                                                   │
│  • Aetheric, runic, Galdr, inscription, rune-marked             │
│  • living weave, resin, spark, sickness-light                   │
│  • contained, managed, slowed (Blight is permanent)             │
│  • inscribe, attune, appease, commune                           │
├─────────────────────────────────────────────────────────────────┤
│  AI PERSONAS:                                                   │
│  • Narrator → Flavor text, voice guidance                       │
│  • Validator → Domain checks, QA                                │
│  • Developer → C# implementation                                │
│  • Worldbuilder → New specs, mechanics                          │
└─────────────────────────────────────────────────────────────────┘
```
