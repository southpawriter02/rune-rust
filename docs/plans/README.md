# Rune & Rust Implementation Plans

> **Last Updated:** 2025-12-22
> **Current Version:** v0.3.10c (Settings & Controls)

This directory contains all implementation plans for Rune & Rust, organized by version and milestone.

---

## Quick Navigation

| Milestone | Versions | Theme | Status |
|-----------|----------|-------|--------|
| [1. Infrastructure](#milestone-1-infrastructure) | v0.0.1 - v0.0.5 | Foundation, DI, Persistence | Complete |
| [2. Exploration](#milestone-2-exploration) | v0.1.0 - v0.1.3 | Character Creation, Interaction, Codex | Complete |
| [3. Combat](#milestone-3-combat) | v0.2.0 - v0.2.4 | Combat System, AI, Abilities | Complete |
| [4. Survivor](#milestone-4-survivor) | v0.3.0 - v0.3.3 | Trauma, Crafting, Rest, Hazards | Complete |
| [4.5 Interface Polish](#milestone-45-interface-polish) | v0.3.4 - v0.3.10 | TUI Enhancement, UX | In Progress |
| [5-13. Future](#milestones-5-13-future) | v0.4.0+ | Saga, Magic, Settlements, Endgame | Planned |

---

## Key Documents

| Document | Description |
|----------|-------------|
| [roadmap.md](roadmap.md) | Master status tracker with implementation checklist |
| [MILESTONES.md](MILESTONES.md) | Consolidated milestone definitions |

---

## Milestone 1: Infrastructure

*The Walking Skeleton - Core architecture and persistence*

| Version | Codename | Focus | Status |
|---------|----------|-------|--------|
| [v0.0.1](v0.0.1.md) | The Foundation | Solution scaffolding, DI, Serilog | Complete |
| [v0.0.2](v0.0.2.md) | The Domain | Dice mechanics, Attributes, Character entity | Complete |
| [v0.0.3](v0.0.3.md) | The Loop | Game state machine, Command Parser | Complete |
| [v0.0.4](v0.0.4.md) | Persistence | PostgreSQL/EF Core, Save/Load | Complete |
| [v0.0.5](v0.0.5.md) | Spatial Core | 3D coordinates, Room entity, Navigation | Complete |

---

## Milestone 2: Exploration

*The Explorer - World interaction and discovery*

| Version | Codename | Focus | Status |
|---------|----------|-------|--------|
| [v0.1.0](v0.1.0.md) | The Survivor | Character creation wizard | Complete |
| [v0.1.1](v0.1.1.md) | Interaction | Interactable objects, WITS examination | Complete |
| [v0.1.2](v0.1.2.md) | Survival | Inventory, Equipment, Loot tables | Complete |
| [v0.1.3](v0.1.3.md) | The Codex | Data Captures, Journal system | Complete |

---

## Milestone 3: Combat

*The Warrior - Tactical combat and enemy AI*

| Version | Codename | Focus | Status |
|---------|----------|-------|--------|
| [v0.2.0](v0.2.0.md) | Combat Core | Initiative, Turn order, Attack resolution | Complete |
| [v0.2.1](v0.2.1.md) | The Armory | Weapon damage, Armor soak, Status effects | Complete |
| [v0.2.2](v0.2.2.md) | The Bestiary | Enemy templates, AI behaviors, Elite traits | Complete |
| [v0.2.3](v0.2.3.md) | The Arsenal | Resource system, Ability engine, Archetype kits | Complete |
| [v0.2.4](v0.2.4.md) | The Adversary | Enemy abilities, Tactical AI, Telegraphs | Complete |

---

## Milestone 4: Survivor

*The Survivor - Trauma, crafting, and environmental systems*

| Version | Codename | Focus | Status |
|---------|----------|-------|--------|
| [v0.3.0](v0.3.0.md) | Trauma Economy | Psychic Stress, Corruption, Breaking Points | Complete |
| [v0.3.1](v0.3.1.md) | The Crafting Bench | Bodging, Alchemy, Runeforging, Field Medicine | Complete |
| [v0.3.2](v0.3.2.md) | Rest & Recovery | Camp mechanics, Ambush risk, Resource recovery | Complete |
| [v0.3.3](v0.3.3.md) | Dynamic Environment | Hazards, Ambient conditions, Ecosystem | Complete |

---

## Milestone 4.5: Interface Polish

*The Interface - TUI enhancement and user experience*

| Version | Codename | Focus | Status |
|---------|----------|-------|--------|
| [v0.3.4](v0.3.4.md) | The Gateway | Main menu, Character creation wizard, Intro | Complete |
| [v0.3.5](v0.3.5.md) | The HUD | Exploration UI, Minimap, Room descriptions | Complete |
| [v0.3.6](v0.3.6.md) | The Tactician | Combat grid, Initiative timeline, Intent icons | Complete |
| [v0.3.7](v0.3.7.md) | The Ledger | Inventory UI, Crafting bench, Journal tabs | Complete |
| v0.3.8 | Dynamic Engine | Template-based room generation | Complete |
| [v0.3.9](v0.3.9.md) | The Feedback | Screen FX, Accessibility themes, Help system | Complete |
| [v0.3.10](v0.3.10.md) | The Configurator | Settings engine, Options UI, Key rebinding | Complete |

---

## Milestones 5-13: Future

*See [MILESTONES.md](MILESTONES.md) for detailed future roadmap*

| Milestone | Theme | Key Features |
|-----------|-------|--------------|
| 5. Saga | Progression | Leveling, Specializations, Factions |
| 6. Weaver | Magic | Spells, Chants, Wild Magic, Runeforging |
| 7. Architect | Settlements | Safe Zones, Trade, Quest Chains |
| 8. Adversary | Advanced Combat | Traits, Squad AI, Bosses, Stealth |
| 9. World | Biomes | Muspelheim, Niflheim, Jotunheim, Alfheim |
| 10. Deep | Underworld | Svartalfheim, Helheim, Sunken Sectors |
| 11. Sky | Endgame | Asgard, Valhalla, Counter-Rune |
| 12. Launch | GUI | Avalonia GUI, Audio, Achievements |
| 13. Legacy | Replay | New Game+, Territory Control |

---

## Version Plan Structure

Each version plan follows this structure:
- **Overview**: Summary of goals and phases
- **Phase A/B/C**: Detailed implementation for each sub-version
  - Architecture & Data Flow
  - Logic Decision Trees
  - Code Implementation
  - Testing Requirements
  - Deliverable Checklist
  - Draft Changelog

---

## Additional Resources

| Document | Description |
|----------|-------------|
| [v0.1.2-progress.md](v0.1.2-progress.md) | Progress tracking for v0.1.2 |

---

*Generated by The Architect - Rune & Rust Development*
