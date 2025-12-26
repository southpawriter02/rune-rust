# Descriptor Content Guide

This document indexes all descriptor content in the game, totaling 1,000+ unique text fragments that create dynamic, procedurally-generated descriptions.

---

## 1. Overview

The descriptor system provides rich, varied text for all aspects of the game:

| Category | Files | Est. Records |
|----------|-------|--------------|
| **Room/Environment** | 5 | ~200 |
| **Atmospheric/Sensory** | 2 | ~155 |
| **Combat** | 3 | ~150 |
| **Skills/Actions** | 2 | ~100 |
| **NPCs/Dialogue** | 1 | ~80 |
| **Status Effects** | 1 | ~100 |
| **Examination** | 1 | ~120 |
| **Galdr/Magic** | 3 | ~100 |
| **Trauma** | 4 | ~100 |
| **Total** | 22 | ~1,100+ |

---

## 2. Data Files Reference

### Room & Environment Descriptors

| File | Purpose | Location |
|------|---------|----------|
| `v0.38.1_descriptor_fragments_content.sql` | Core room fragments (spatial, architectural, detail) | `data/descriptors/` |
| `v0.38.4_atmospheric_descriptors.sql` | Sensory details (smell, sound, light, temp) | `data/descriptors/` |
| `v0.38.13_ambient_environmental_descriptors_data.sql` | Ambient conditions and biome atmosphere | `data/descriptors/` |

### Combat Descriptors

| File | Purpose | Location |
|------|---------|----------|
| `v0.38.6_player_action_descriptors.sql` | Attack/defend action text | `data/descriptors/` |
| `v0.38.12_combat_mechanics_descriptors_data.sql` | Hit/miss/critical descriptions | `data/descriptors/` |

### Magic/Galdr Descriptors

| File | Purpose | Location |
|------|---------|----------|
| `v0.38.7_galdr_action_descriptors.sql` | Spell casting descriptions | `data/descriptors/` |
| `v0.38.7_galdr_outcome_descriptors.sql` | Success/failure text | `data/descriptors/` |
| `v0.38.7_galdr_miscast_descriptors.sql` | Wild magic surge text | `data/descriptors/` |

### Character Descriptors

| File | Purpose | Location |
|------|---------|----------|
| `v0.38.8_status_effect_descriptors_data.sql` | Condition/debuff descriptions | `data/descriptors/` |
| `v0.38.10_skill_usage_descriptors_data.sql` | Skill check flavor text | `data/descriptors/` |
| `v0.38.11_npc_descriptors_barks_data.sql` | NPC dialogue snippets | `data/descriptors/` |

### Examination Descriptors

| File | Purpose | Location |
|------|---------|----------|
| `v0.38.9_examination_perception_descriptors_data.sql` | Examine/search results | `data/descriptors/` |

### Trauma Descriptors

| File | Purpose | Location |
|------|---------|----------|
| `v0.38.14_trauma_type_descriptors_content.sql` | Trauma type text | `data/descriptors/` |
| `v0.38.14_trauma_trigger_descriptors_content.sql` | Trigger descriptions | `data/descriptors/` |
| `v0.38.14_breaking_point_descriptors_content.sql` | Breaking point text | `data/descriptors/` |
| `v0.38.14_recovery_descriptors_content.sql` | Recovery flavor text | `data/descriptors/` |

---

## 3. Schema Files Reference

All schemas are in `data/schemas/`:

| File | Purpose |
|------|---------|
| `v0.38.0_descriptor_framework_schema.sql` | Core framework tables |
| `v0.38.4_atmospheric_descriptor_schema.sql` | Sensory table structure |
| `v0.38.8_status_effect_descriptors_schema.sql` | Status effect tables |
| `v0.38.9_examination_perception_descriptors_schema.sql` | Examine tables |
| `v0.38.10_skill_usage_descriptors_schema.sql` | Skill text tables |
| `v0.38.11_npc_descriptors_barks_schema.sql` | NPC dialogue tables |
| `v0.38.12_combat_mechanics_descriptors_schema.sql` | Combat text tables |
| `v0.38.13_ambient_environmental_descriptors_schema.sql` | Environment tables |
| `v0.38.14_trauma_descriptor_library_schema.sql` | Trauma system tables |

---

## 4. Descriptor Categories

### 4.1 Spatial Descriptors

Text for room size and shape:

- "This cramped passage barely allows movement"
- "A vast chamber stretches into the gloom"
- "The ceiling arches overhead, lost in shadow"

### 4.2 Architectural Descriptors

Building elements and materials:

- "Corroded metal plates form the walls"
- "Massive rivets hold together ancient girders"
- "Smooth, seamless walls suggest advanced fabrication"

### 4.3 Detail Descriptors

Immersive environmental details:

- "Rust streaks mark the surfaces like old blood"
- "Faint runes glow with dying light"
- "Something has recently disturbed the debris"

### 4.4 Atmospheric Descriptors (155+)

Sensory immersion by category:

| Type | Examples |
|------|----------|
| Smell | "The air reeks of machine oil and ozone" |
| Sound | "Distant clanking echoes through the corridors" |
| Light | "Flickering emergency lights cast dancing shadows" |
| Temperature | "A cold draft carries the chill of deep places" |

### 4.5 Combat Descriptors

Action flavor text:

- HIT: "Your blade finds its mark with a satisfying crunch"
- MISS: "Your strike whistles past, connecting with nothing"
- CRITICAL: "A devastating blow tears through defenses"
- BLOCK: "Your shield absorbs the impact with a thunderous clang"

### 4.6 Trauma Descriptors

Mental state text:

- TRIGGER: "The sight of the body brings back unwanted memories"
- BREAK: "Something inside you finally gives way"
- RECOVERY: "For the first time in days, you feel a moment of peace"

---

## 5. Integration with Room Engine

See [docs/07-environment/room-engine/descriptors.md](../07-environment/room-engine/descriptors.md) for:
- Three-tier composition model
- Fragment selection algorithms
- Composite generation

---

## 7. Phased Implementation Guide

### Phase 1: Database Migration
- [ ] **Schema**: Verify all listed schemas are applied in PostgreSQL.
- [ ] **Data**: Verify all listed data files are seeded.
- [ ] **Access**: Create repositories for each descriptor category (Room, Combat, Trauma).

### Phase 2: Selection Logic
- [ ] **Weighted Selection**: Implement random selection with weight support.
- [ ] **Context Filtering**: Filter descriptors by Biome, Danger Level, Light Level.
- [ ] **Templates**: Implement text template interpolation (Replacing `{Target}`, `{Weapon}`).

### Phase 3: Content Expansion
- [ ] **Audit**: Review database for empty categories or low-variety buckets.
- [ ] **Authoring**: Add 50 new entries for "The Roots" biome.

### Phase 4: Integration
- [ ] **Room Engine**: Connect `RoomRepository` to descriptor tables.
- [ ] **Combat**: Connect `CombatLogService` to descriptor tables.

---

## 8. Testing Requirements

### 8.1 Unit Tests
- [ ] **Selection**: GetDescriptor(Category) -> Returns valid string.
- [ ] **Filtering**: GetDescriptor(Biome="Ice") -> Returns only Ice descriptors.
- [ ] **Formatting**: Interpolate("Use {Weapon}", "Sword") -> "Use Sword".

### 8.2 Integration Tests
- [ ] **Database**: All foreign keys between Descriptors and Biomes are valid.
- [ ] **Load**: Application startup validates all descriptor templates.

### 8.3 Manual QA
- [ ] **Reading**: "Walk" through 10 generated rooms, checking for repeated or nonsensical phrases.

---

## 9. Logging Requirements

**Reference:** [logging.md](../../00-project/logging.md)

### 9.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Missing | Warn | "No descriptors found for category {Category} in biome {Biome}." | `Category`, `Biome` |
| Selection | Verbose | "Selected descriptor {Id}: {Text}" | `Id`, `Text` |

---

## 10. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial index |
| 1.1 | 2025-12-14 | Standardized with Phased Guide, Testing, and Logging |
