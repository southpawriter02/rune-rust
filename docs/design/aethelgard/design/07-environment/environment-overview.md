---
id: SPEC-ENVIRONMENT
title: "Environment Systems — Overview"
version: 1.0
status: draft
last-updated: 2025-12-07
---

# Environment Systems — Overview

This section documents the procedural environment generation systems including dungeon layout, room composition, biomes, and hazards.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    SECTOR GENERATION                         │
├─────────────────────────────────────────────────────────────┤
│  1. Graph Generation      │  DungeonGenerator               │
│  2. Spatial Layout        │  SpatialLayoutService           │
│  3. Biome Assignment      │  BiomeTransitionService         │
│  4. Content Density       │  ContentDensityService          │
│  5. Population            │  RoomPopulationService          │
│  6. Descriptor Generation │  RoomDescriptorService          │
└─────────────────────────────────────────────────────────────┘
```

---

## Documentation Structure

```
07-environment/
├── overview.md                 ← You are here
├── ambient-conditions.md       ← Room-wide environmental modifiers
├── dynamic-hazards.md          ← Interactive battlefield elements
├── puzzle-system.md            ← Multi-solution puzzles, TUI/GUI
├── navigation.md               ← Movement, travel, Z-axis
├── settlements.md              ← Towns, services, social sectors
├── room-engine/
│   ├── core.md                 ← Graph, templates, seeds
│   ├── spatial-layout.md       ← 3D coordinates, vertical
│   ├── descriptors.md          ← Three-tier text composition
│   ├── examination.md          ← Perception, object examination
│   └── population.md           ← Content density, spawning
└── biomes/
    ├── overview.md             ← Transitions, blending
    ├── the-roots.md
    ├── muspelheim.md
    ├── niflheim.md
    ├── alfheim.md
    └── jotunheim.md
```

---

## Key Concepts

### Procedural Generation Pipeline

1. **Graph Generation** — Wave Function Collapse creates abstract room nodes
2. **Spatial Layout** — Nodes placed in 3D coordinate space (X, Y, Z)
3. **Biome Assignment** — Biome blending for transition zones
4. **Content Density** — Global budgets determine enemy/hazard distribution
5. **Population** — Enemies, hazards, loot spawned per room budget
6. **Descriptors** — Three-tier composition generates unique text

### Seed Reproducibility

All generation uses **deterministic seeding**:
- Same seed → identical dungeon layout
- Enables save/load, replay, and debugging

### Coherent Glitch Generator

Environmental storytelling philosophy:
- 800 years of decay shapes every room
- Technological remnants mixed with corruption
- Consistent theming within biomes

---

## Related Specifications

| Spec | Purpose |
|------|---------|
| [Ambient Conditions](ambient-conditions.md) | Room-wide environmental modifiers (Psychic Resonance, etc.) |
| [Dynamic Hazards](dynamic-hazards.md) | Interactive battlefield elements (Steam Vents, etc.) |
| [Room Engine Core](room-engine/core.md) | Graph layout, templates |
| [Descriptors](room-engine/descriptors.md) | Three-tier text composition |
| [Examination](room-engine/examination.md) | Perception checks, object examination, flora/fauna |
| [Puzzle System](puzzle-system.md) | Multi-solution puzzles, TUI/GUI presentation |
| [Navigation](navigation.md) | Movement commands, travel, Z-axis |
| [Settlements](settlements.md) | Towns, services, social sectors |
| [Biomes Overview](biomes/biomes-overview.md) | Biome system |
| [Descriptor Library](../99-legacy/Data/) | SQL descriptor data |
