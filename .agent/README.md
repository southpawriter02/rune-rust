# Agent Configuration Directory

This directory contains rules and workflows for AI agents working on the Rune & Rust project. All content is organized hierarchically for easy navigation and maintenance.

## 📁 Directory Structure

```
.agent/
├── README.md (this file)
├── rules/              # Rules that govern agent behavior
│   ├── 01-technical/      # Technical standards (coding, stack, constraints)
│   ├── 02-documentation/  # Documentation standards and conventions
│   ├── 03-content/        # Content and narrative guidelines
│   ├── 04-game-mechanics/ # Core game mechanics rules
│   ├── 05-domains/        # Nine world-building domains (cosmology, time, magic, etc.)
│   ├── 06-audit/          # Quality audit standards and golden examples
│   └── 07-project/        # Current project phase and status
└── workflows/          # Step-by-step workflows for common tasks
    ├── development/       # Code implementation workflows
    ├── quality-assurance/ # Testing and validation workflows
    └── onboarding/        # Context and getting-started workflows
```

## 🎯 Quick Start

### For New Agents
1. Start with [workflows/onboarding/gaining-context.md](workflows/onboarding/gaining-context.md)
2. Review [rules/07-project/current-phase.md](rules/07-project/current-phase.md)
3. Familiarize yourself with [rules/01-technical/](rules/01-technical/)

### For Development Tasks
1. Check [workflows/development/feature-implementation.md](workflows/development/feature-implementation.md)
2. Follow [rules/01-technical/coding-standards.md](rules/01-technical/coding-standards.md)
3. Use [workflows/development/component-creation.md](workflows/development/component-creation.md) for UI work

### For Content Creation
1. Review [rules/03-content/narrative-voice.md](rules/03-content/narrative-voice.md)
2. Check relevant domain rules in [rules/05-domains/](rules/05-domains/)
3. Use [workflows/quality-assurance/narrative-validation.md](workflows/quality-assurance/narrative-validation.md)

## 📚 Documentation

### Rules (Always Active)
Rules define constraints and standards that must be followed at all times. See [rules/README.md](rules/README.md) for full catalog.

**Key Rule Categories:**
- **Technical**: Coding standards, tech stack, constraints
- **Documentation**: Templates, formatting, naming conventions
- **Content**: Narrative voice, setting context
- **Game Mechanics**: Dice system, core mechanics
- **Domains**: Nine lore domains (cosmology through allrune)
- **Audit**: Quality standards and golden examples

### Workflows (Context-Specific)
Workflows provide step-by-step procedures for specific tasks. See [workflows/README.md](workflows/README.md) for full catalog.

**Key Workflow Categories:**
- **Development**: Feature implementation, component creation, database migrations
- **Quality Assurance**: Spec validation, narrative validation
- **Onboarding**: Gaining context, scaffolding

## 🔍 Finding What You Need

### By Topic
- **Coding?** → [rules/01-technical/](rules/01-technical/)
- **Writing?** → [rules/03-content/](rules/03-content/)
- **Game Design?** → [rules/04-game-mechanics/](rules/04-game-mechanics/)
- **Lore?** → [rules/05-domains/](rules/05-domains/)
- **Quality?** → [rules/06-audit/](rules/06-audit/)

### By Task
- **Implementing a feature?** → [workflows/development/feature-implementation.md](workflows/development/feature-implementation.md)
- **Creating UI?** → [workflows/development/component-creation.md](workflows/development/component-creation.md)
- **Migrating database?** → [workflows/development/database-migration.md](workflows/development/database-migration.md)
- **Validating content?** → [workflows/quality-assurance/](workflows/quality-assurance/)
- **Getting started?** → [workflows/onboarding/](workflows/onboarding/)

## 🏛️ Agent Personas

The project uses specialized agent personas (defined in `/CLAUDE.md`):

- **🏗️ The Forge-Master**: Backend & Systems (C#, Services, DI)
- **🎨 The Rune-Scribe**: UI/UX (Avalonia, XAML, ViewModels)
- **📜 The Archivist**: Data & Lore (SQL, JSON, Game Content)
- **🛡️ The QA Sentinel**: Testing (xUnit, Validation)

## 📋 File Conventions

All rule and workflow files follow these conventions:
- YAML frontmatter with metadata
- Markdown format
- Clear section headers
- Examples where applicable
- Checklists for verification

## 🔄 Updating This Directory

When adding or modifying rules/workflows:
1. Place files in the appropriate category subdirectory
2. Update the relevant README.md file
3. Use descriptive, kebab-case filenames
4. Include YAML frontmatter with `trigger` or `description`
5. Follow existing file structure patterns

## 📖 Related Documentation

- `/CLAUDE.md` - Main agent identity and directives
- `/docs/` - Project documentation and specifications
- `/README.md` - Project overview and setup
