---
trigger: always_on
---

## Directory Structure

### Top-Level Organization

```
docs/
├── .templates/               # Documentation templates (do not modify lightly)
│   ├── ability.md
│   ├── craft.md
│   ├── resource.md
│   ├── skill.md
│   ├── specialization.md
│   ├── status-effect.md
│   ├── system.md
│   ├── README.md
│   └── flavor-text/          # Voice/tone guidance (13 files)
│
├── 00-project/               # Meta-documentation, standards, planning
│   ├── DOCUMENTATION_STANDARDS.md  ← You are here
│   └── ...
│
├── 01-core/                  # Core game systems
│   ├── attributes/           # MIGHT, FINESSE, STURDINESS, WITS, WILL
│   ├── resources/            # HP, Stamina, Stress, Fury, AP
│   ├── skills/               # Skill definitions and overview
│   ├── character-creation.md
│   ├── death-resurrection.md
│   ├── dice-system.md
│   ├── game-loop.md
│   ├── persistence.md
│   ├── saga-system.md
│   └── trauma-economy.md
│
├── 02-entities/              # NPCs, enemies, archetypes
│   ├── archetypes/           # Enemy types (Forlorn, Draugr, etc.)
│   └── npcs/                 # Named characters
│
├── 03-character/             # Player character options
│   └── specializations/      # One folder per specialization
│       ├── berserkr/
│       │   ├── overview.md   # ⭐ Golden standard
│       │   └── abilities/    # Individual ability specs
│       ├── bone-setter/
│       ├── ruin-stalker/
│       │   └── abilities/
│       │       └── corridor-maker.md  # ⭐ Golden standard
│       └── runasmidr/
│
├── 03-combat/                # Combat system mechanics
│
├── 04-systems/               # Game subsystems
│   ├── crafting/             # Field Medicine, Runeforging, Alchemy, Bodging
│   └── status-effects/       # All status effect specs
│       └── bleeding.md       # ⭐ Golden standard
│
├── 04-magic/                 # Magic system details (if separated)
│
├── 05-items/                 # Equipment, consumables, materials
│
├── 06-crafting/              # Alternate crafting location (consolidate with 04-systems)
│
├── 07-environment/           # Biomes, room engine, locations
│
├── 08-ui/                    # UI/UX specifications
│
├── 09-data/                  # Data schemas, tables
│
├── 10-testing/               # Test specifications
│
└── 99-legacy/                # Legacy Notion exports (968 files, reference only)
```

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| **Folders** | `lowercase-kebab` | `bone-setter/`, `status-effects/` |
| **Files** | `lowercase-kebab.md` | `death-resurrection.md` |
| **Specialization folders** | ASCII version of name | `runasmidr/` (not Rúnasmiðr) |
| **Ability files** | Ability name in kebab-case | `corridor-maker.md` |
| **Overview files** | Always `overview.md` | `berserkr/overview.md` |

### File Placement Guide

| Content Type | Location |
|--------------|----------|
| New specialization | `03-character/specializations/{name}/overview.md` |
| New ability | `03-character/specializations/{spec}/abilities/{ability}.md` |
| New status effect | `04-systems/status-effects/{effect}.md` |
| New crafting trade | `04-systems/crafting/{trade}.md` |
| Core resource (HP, Stamina) | `01-core/resources/{resource}.md` |
| Core system | `01-core/{system}.md` |
| New enemy type | `02-entities/archetypes/{type}.md` |
| UI specification | `08-ui/{component}.md` |

### Legacy Folder

`99-legacy/` contains ~968 files exported from Notion. These are **reference only**:

- ❌ Do not edit legacy files
- ❌ Do not link to legacy files from active specs
- ✅ Use as source material for new specs
- ✅ Extract patterns and content, then create proper specs