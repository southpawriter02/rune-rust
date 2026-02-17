# Rune & Rust — Game Specification Index

> **A text-based fantasy dungeon crawler RPG**
> C# • PostgreSQL • Terminal + AvaloniaUI

---

## Quick Navigation

| Category | Description |
|----------|-------------|
| [00-project](./00-project/) | Vision, glossary, conventions, architecture |
| [01-core](./01-core/) | Game loop, persistence, events, time, RNG |
| [02-entities](./02-entities/) | Attributes, conditions, archetypes, specializations |
| [03-combat](./03-combat/) | Positioning, actions, damage, stances, keywords |
| [04-magic](./04-magic/) | Spell system, schools, casting mechanics |
| [05-items](./05-items/) | Equipment, consumables, properties |
| [06-crafting](./06-crafting/) | Recipes, stations, materials |
| [07-environment](./07-environment/) | Dungeons, movement, puzzles, generation |
| [08-ui](./08-ui/) | Terminal commands, Avalonia GUI, accessibility |
| [09-data](./09-data/) | Database schema, enums, constants |
| [10-testing](./10-testing/) | Unit tests, integration tests, scenarios |

---

## Specification Status

### Core Systems
| Spec ID | Name | Status |
|---------|------|--------|
| SPEC-CORE-DICE | [Dice Pool System](./01-core/dice-system.md) | 📝 Draft |
| SPEC-CORE-SAGA | [Saga System](./01-core/saga-system.md) | 📝 Draft |
| SPEC-CORE-TRAUMA | [Trauma Economy](./01-core/trauma-economy.md) | 📝 Draft |
| SPEC-CORE-CHARACTER-CREATION | [Character Creation](./01-core/character-creation.md) | 📝 Draft |
| SPEC-CORE-GAMELOOP | [Game Loop & State](./01-core/game-loop.md) | 📝 Draft |
| SPEC-CORE-PERSISTENCE | [Persistence](./01-core/persistence.md) | 📝 Draft |
| SPEC-CORE-EVENTS | [Event System](./01-core/events.md) | 📝 Draft |

### Attributes
| Spec ID | Name | Status |
|---------|------|--------|
| SPEC-CORE-ATTRIBUTES | [Attributes Overview](./01-core/attributes/attributes.md) | 📝 Draft |
| SPEC-CORE-ATTR-MIGHT | [MIGHT](./01-core/attributes/might.md) | 📝 Draft |
| SPEC-CORE-ATTR-FINESSE | [FINESSE](./01-core/attributes/finesse.md) | 📝 Draft |
| SPEC-CORE-ATTR-STURDINESS | [STURDINESS](./01-core/attributes/sturdiness.md) | 📝 Draft |
| SPEC-CORE-ATTR-WITS | [WITS](./01-core/attributes/wits.md) | 📝 Draft |
| SPEC-CORE-ATTR-WILL | [WILL](./01-core/attributes/will.md) | 📝 Draft |

### Resources
| Spec ID | Name | Status |
|---------|------|--------|
| SPEC-CORE-RESOURCES | [Resources Overview](./01-core/resources/resources.md) | 📝 Draft |
| SPEC-CORE-RES-HP | [Health Pool](./01-core/resources/hp.md) | 📝 Draft |
| SPEC-CORE-RES-STAMINA | [Stamina](./01-core/resources/stamina.md) | 📝 Draft |
| SPEC-CORE-RES-AETHER | [Aether Pool](./01-core/resources/aether.md) | 📝 Draft |
| SPEC-CORE-RES-STRESS | [Psychic Stress](./01-core/resources/stress.md) | 📝 Draft |
| SPEC-CORE-RES-RAGE | [Rage](./01-core/resources/rage.md) | 📝 Draft |
| SPEC-CORE-RES-MOMENTUM | [Momentum](./01-core/resources/momentum.md) | 📝 Draft |
| SPEC-CORE-RES-COHERENCE | [Coherence](./01-core/resources/coherence.md) | 📝 Draft |

### Archetypes
| Spec ID | Name | Status |
|---------|------|--------|
| SPEC-CORE-ARCHETYPES | [Archetypes Overview](./02-entities/archetypes/archetypes.md) | 📝 Draft |
| SPEC-ARCHETYPE-WARRIOR | [Warrior](./02-entities/archetypes/warrior.md) | 📝 Draft |
| SPEC-ARCHETYPE-SKIRMISHER | [Skirmisher](./02-entities/archetypes/skirmisher.md) | 📝 Draft |
| SPEC-ARCHETYPE-MYSTIC | [Mystic](./02-entities/archetypes/mystic.md) | 📝 Draft |
| SPEC-ARCHETYPE-ADEPT | [Adept](./02-entities/archetypes/adept.md) | 📝 Draft |

### Specializations
| Spec ID | Name | Status |
|---------|------|--------|
| SPEC-SPECIALIZATION-ATGEIR-WIELDER | [Atgeir-Wielder](./02-entities/specializations/atgeir-wielder.md) | ✅ Complete |

### Combat System
| Spec ID | Name | Status |
|---------|------|--------|
| SPEC-COMBAT-RESOLUTION | [Combat Resolution](./03-combat/combat-resolution.md) | 📝 Draft |
| SPEC-COMBAT-OUTCOMES | [Attack Outcomes](./03-combat/attack-outcomes.md) | 📝 Draft |
| SPEC-COMBAT-DEFENSE | [Defensive Reactions](./03-combat/defensive-reactions.md) | 📝 Draft |
| SPEC-COMBAT-STANCES | [Combat Stances](./03-combat/combat-stances.md) | 📝 Draft |
| SPEC-COMBAT-STATUS | [Status Effects](./03-combat/status-effects.md) | 📝 Draft |

## Document Format

All specifications follow a standardized format with:
- YAML frontmatter (ID, version, status, dependencies)
- Identity tables and design philosophy
- Detailed mechanics with exact formulas
- GUI specifications with color codes
- Implementation status and priority phases
- Testing requirements

See [CONVENTIONS.md](./00-project/CONVENTIONS.md) for full details.

---

## Getting Started

1. Review [OVERVIEW.md](./00-project/OVERVIEW.md) for game vision
2. Read [GLOSSARY.md](./00-project/GLOSSARY.md) for terminology
3. See [ARCHITECTURE.md](./00-project/ARCHITECTURE.md) for technical stack
4. Read [CONTRIBUTOR_HUB.md](./00-project/CONTRIBUTOR_HUB.md) to start contributing
