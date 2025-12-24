# Agent Rules

Rules define constraints, standards, and guidelines that govern agent behavior. All rules in this directory are **always active** unless explicitly scoped.

## 📂 Rule Categories

### 01-technical/ - Technical Standards
Core technical requirements for code implementation.

| File | Purpose | Key Topics |
|------|---------|------------|
| `coding-standards.md` | C# coding conventions | Naming, async patterns, nullability |
| `technical-stack.md` | Technology stack definitions | .NET 8, Avalonia, ReactiveUI, PostgreSQL |
| `technical-constraints.md` | Technical limitations | System constraints and boundaries |

**When to reference:** Every code implementation task.

---

### 02-documentation/ - Documentation Standards
Standards for writing and organizing documentation.

| File | Purpose | Key Topics |
|------|---------|------------|
| `doc-templates.md` | Template guidelines | Available templates and usage |
| `formatting-conventions.md` | Format standards | Markdown, tables, code blocks |
| `naming-conventions.md` | Naming rules | File names, IDs, cross-references |
| `cross-references.md` | Linking standards | How to reference other docs |

**When to reference:** Creating or updating any documentation.

---

### 03-content/ - Content & Narrative Guidelines
Rules for in-game content, flavor text, and narrative voice.

| File | Purpose | Key Topics |
|------|---------|------------|
| `narrative-rules.md` | AAM compliance rules | Layer 2 voice, precision constraints |
| `narrative-voice.md` | Voice guidelines | Jötun-Reader perspective, tone |
| `setting-context.md` | World context | Lost knowledge, bodge culture, themes |

**When to reference:** Writing any player-facing content (tooltips, descriptions, dialogue).

---

### 04-game-mechanics/ - Core Game Mechanics
Fundamental game system rules.

| File | Purpose | Key Topics |
|------|---------|------------|
| `dice-system.md` | Dice mechanics | d10 resolution, tiered damage (d4-d10) |
| `key-game-mechanics.md` | Core systems | Corruption, Stress, Stamina, attributes |
| `magic-system.md` | Magic rules | Runes, FUTHARK protocol, Runic Blight |

**When to reference:** Implementing combat, character stats, or magic systems.

---

### 05-domains/ - World-Building Domains
Nine canonical domains that define the game world's lore and constraints.

| File | Domain | Key Topics |
|------|--------|------------|
| `domain-1-cosmology.md` | Cosmology | Nine Realms, Yggdrasil structure, The Deep |
| `domain-2-time.md` | Time & History | Timeline, eras, pre/post-Glitch |
| `domain-3-magic.md` | Magic System | Rune mechanics, Aetheric Field |
| `domain-4-technology.md` | Technology | **CRITICAL: Precision constraints** |
| `domain-5-species.md` | Species & Races | Playable races, NPCs, Forlorn |
| `domain-6-entities.md` | Entities | NPCs, factions, creatures |
| `domain-7-reality.md` | Reality Anomalies | Glitches, reality breaks, instabilities |
| `domain-8-geography.md` | Geography | Locations, regions, landmarks |
| `domain-9-allrune.md` | AllRune | The omnipresent AI consciousness |

**When to reference:** Any lore-related content creation or validation.

**⚠️ CRITICAL:** Domain 4 (Technology) contains the **precision measurement ban** for post-Glitch content. This is the #1 violated rule.

---

### 06-audit/ - Quality & Audit Standards
Standards for quality assurance and spec validation.

| File | Purpose | Key Topics |
|------|---------|------------|
| `audit-standards.md` | Audit methodology | How to audit specs, conformance scoring |
| `gold-standard-specs.md` | Reference examples | Golden standard spec examples |
| `directory-structure.md` | Repository organization | Where files belong |

**When to reference:** Auditing existing specs, creating new specs, or organizing files.

---

### 07-project/ - Project Management
Current project status and phase information.

| File | Purpose | Key Topics |
|------|---------|------------|
| `current-phase.md` | Active project phase | Current sprint goals, priorities |

**When to reference:** Understanding current priorities and active work.

---

## 🎯 Common Use Cases

### Starting a New Feature
1. Check `07-project/current-phase.md` for priorities
2. Review `01-technical/coding-standards.md`
3. Consult relevant game mechanic rules (`04-game-mechanics/`)

### Writing In-Game Text
1. **MUST READ:** `05-domains/domain-4-technology.md` (precision ban)
2. Review `03-content/narrative-voice.md`
3. Check relevant domain rules (`05-domains/`)
4. Validate with `06-audit/audit-standards.md`

### Creating Documentation
1. Review `02-documentation/doc-templates.md`
2. Follow `02-documentation/formatting-conventions.md`
3. Use `02-documentation/naming-conventions.md`

### Auditing Content
1. Use `06-audit/audit-standards.md` methodology
2. Compare to `06-audit/gold-standard-specs.md`
3. Check domain compliance (`05-domains/`)

## 🔴 Critical Rules (Never Violate)

### Domain 4: Technology Constraints
**Post-Glitch "Layer 2" content CANNOT contain precision measurements.**

❌ **Forbidden:**
- "95% chance"
- "4.2 meters"
- "35°C"
- "18 seconds"
- "API," "Bug," "Glitch"

✅ **Allowed:**
- "Almost certain"
- "A spear's throw"
- "Oppressively hot"
- "Several moments"
- "Anomaly," "Phenomenon"

### Coding Standards
- `<Nullable>enable</Nullable>` is **strict**
- Always use `async Task` (never `async void`)
- All services must use interface-based design

### Dice System
- Resolution rolls: **d10 only**
- Damage dice: Tiered hierarchy (d4 → d6 → d8 → d10)

## 📊 Rule Priority Levels

All rules are active, but violations have different severity:

| Priority | Severity | Examples |
|----------|----------|----------|
| 🔴 **Critical** | Breaks game/lore consistency | Domain 4 violations, dice system errors |
| 🟠 **High** | Degrades quality | Code standard violations, missing docs |
| 🟡 **Medium** | Reduces clarity | Formatting issues, naming inconsistencies |
| 🟢 **Low** | Stylistic preference | Minor formatting variations |

## 🔄 Updating Rules

When modifying rules:
1. Ensure changes don't conflict with other rules
2. Update this README if adding/removing files
3. Mark outdated rules clearly
4. Document rationale for major changes
5. Update `trigger` metadata in YAML frontmatter

## 📖 Related Resources

- Parent: [/.agent/README.md](../README.md)
- Workflows: [/.agent/workflows/README.md](../workflows/README.md)
- Main Agent Doc: [/CLAUDE.md](/CLAUDE.md)
