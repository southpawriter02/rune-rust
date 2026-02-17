# Migration Plan: Legacy to New Specification Structure

## 1. Executive Summary

This document outlines the plan to migrate 400+ legacy specification files into the new, consolidated documentation structure defined in `docs/00-project/CONVENTIONS.md`. The goal is to create a cohesive, navigable, and implementation-ready specification library.

**Key Strategy**: Consolidate scattered "atomic" specs (single abilities, single mechanics) into "system" specs (complete Specializations, complete Combat Engine).

---

## 2. Migration Phases

### Phase 1: Core Systems (High Priority)
**Goal**: Establish the foundation.

| Legacy File(s) | New Spec ID | New File Path | Notes |
|----------------|-------------|---------------|-------|
| `Attributes System` | `SPEC-CORE-ATTRIBUTES` | `docs/01-core/attributes.md` | |
| `Dice Pool System` | `SPEC-CORE-DICE` | `docs/01-core/dice-system.md` | |
| `Turn System` | `SPEC-CORE-TURN` | `docs/01-core/turn-system.md` | Merge into Game Loop if small |
| `Time System` | `SPEC-CORE-TIME` | `docs/01-core/time-system.md` | |
| `Resource Systems` | `SPEC-CORE-RESOURCES` | `docs/01-core/resources.md` | HP, Stamina, Mana, etc. |
| `Save Load System` | `SPEC-CORE-PERSISTENCE` | `docs/01-core/persistence.md` | *Existing* - Review & Update |
| `Game Loop` | `SPEC-CORE-GAMELOOP` | `docs/01-core/game-loop.md` | *Existing* - Review & Update |

### Phase 2: Combat Engine
**Goal**: Define the conflict resolution mechanics.

| Legacy File(s) | New Spec ID | New File Path | Notes |
|----------------|-------------|---------------|-------|
| `Combat System` | `SPEC-COMBAT-ENGINE` | `docs/03-combat/engine.md` | Core flow, phases |
| `Combat Resolution System` | `SPEC-COMBAT-RESOLUTION` | `docs/03-combat/resolution.md` | Attack rolls, defense |
| `Damage Calculation`, `Damage System` | `SPEC-COMBAT-DAMAGE` | `docs/03-combat/damage.md` | Consolidate formulas |
| `Status Effect System` (folder) | `SPEC-COMBAT-STATUS` | `docs/03-combat/status-effects.md` | Master list & mechanics |
| `Action System` | `SPEC-COMBAT-ACTIONS` | `docs/03-combat/actions.md` | Standard, Bonus, Move actions |
| `Positioning`, `Stance System` | `SPEC-COMBAT-POSITION` | `docs/03-combat/positioning.md` | Rows, movement, stances |

### Phase 3: Entities & Specializations (Massive Consolidation)
**Goal**: Define the player characters and their capabilities.
**Strategy**: Merge "Tier X Ability" files into their parent Specialization file.

| Legacy File(s) | New Spec ID | New File Path | Notes |
|----------------|-------------|---------------|-------|
| `Archetype & Specialization System` | `SPEC-ENTITY-CLASSES` | `docs/02-entities/class-system.md` | Base rules |
| `Atgeir-wielder` + associated abilities | `SPEC-SPEC-ATGEIR` | `docs/02-entities/specializations/atgeir-wielder.md` | *Complete* |
| `Alka-hestur` + associated abilities | `SPEC-SPEC-ALKAHESTUR` | `docs/02-entities/specializations/alka-hestur.md` | |
| `Berserkr` + associated abilities | `SPEC-SPEC-BERSERKR` | `docs/02-entities/specializations/berserkr.md` | |
| `Bone-Setter` + associated abilities | `SPEC-SPEC-BONESETTER` | `docs/02-entities/specializations/bone-setter.md` | |
| ... (Repeat for all 12+ specs) | ... | ... | |

### Phase 4: Items & Crafting
**Goal**: Economy and progression.

| Legacy File(s) | New Spec ID | New File Path | Notes |
|----------------|-------------|---------------|-------|
| `Equipment System`, `Weapon System` | `SPEC-ITEM-EQUIPMENT` | `docs/05-items/equipment.md` | |
| `Loot System` | `SPEC-ITEM-LOOT` | `docs/05-items/loot-tables.md` | |
| `Crafting System` | `SPEC-CRAFT-SYSTEM` | `docs/06-crafting/system.md` | |
| `Recipes` (folder/files) | `SPEC-CRAFT-RECIPES` | `docs/06-crafting/recipes.md` | |

### Phase 5: Environment & World
**Goal**: The setting and exploration.

| Legacy File(s) | New Spec ID | New File Path | Notes |
|----------------|-------------|---------------|-------|
| `Procedural Dungeon Generation` | `SPEC-ENV-GENERATION` | `docs/07-environment/generation.md` | Algorithms |
| `Dynamic Lighting System` | `SPEC-ENV-LIGHTING` | `docs/07-environment/lighting.md` | |
| `Encounter System` | `SPEC-ENV-ENCOUNTERS` | `docs/07-environment/encounters.md` | Spawning logic |

### Phase 6: UI & Technical
**Goal**: Implementation details.

| Legacy File(s) | New Spec ID | New File Path | Notes |
|----------------|-------------|---------------|-------|
| `Avalonia` specs (v0.43) | `SPEC-UI-GUI` | `docs/08-ui/gui-system.md` | |
| `Database Schema` | `SPEC-DATA-SCHEMA` | `docs/09-data/schema.md` | |

---

## 3. Implementation Strategy

### 3.1 The "Engine" Pattern
To ensure **flexibility** and **performance**, we will follow the architecture defined in `ARCHITECTURE.md`:
1.  **Stateless Services**: Game logic lives in `RuneAndRust.Engine` services (e.g., `CombatService`, `MovementService`).
2.  **Rich Domain Models**: Entities (Character, Monster) hold state but minimal logic.
3.  **Event-Driven**: Systems communicate via `GameEvent` (e.g., `AttackDeclared`, `DamageTaken`) to allow decoupled reactions (e.g., UI updates, Achievement triggers).

### 3.2 Handling Overlap
Legacy files often duplicate information (e.g., `Damage System` vs `Combat Resolution`).
*   **Rule**: The `03-combat` specs are the source of truth for *mechanics*.
*   **Rule**: The `02-entities` specs are the source of truth for *data* (stats, ability values).
*   **Action**: When migrating, cross-reference. If a conflict exists, prioritize the most recent "v0.XX" implementation spec if available, otherwise the most detailed mechanic spec.

### 3.3 Performance Optimization
*   **Data Structures**: Use `Dictionary<Guid, T>` for O(1) lookups of game objects.
*   **ECS-Lite**: For the combat engine, treat "Status Effects" and "Modifiers" as components that can be iterated over quickly.
*   **Database**: Use `jsonb` for flexible entity data (like varying component stats) to avoid rigid schema migrations for every new item property.

---

## 4. Next Steps

1.  **Approve Plan**: Confirm this structure.
2.  **Execute Phase 1**: Create Core System specs.
3.  **Execute Phase 2**: Create Combat Engine specs.
4.  **Execute Phase 3**: Begin the Great Specialization Migration (one by one).
