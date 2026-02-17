# v0.19.x Reference Materials

These documents are **shared data registries** consumed by the versioned biome implementations (v0.19.1–v0.19.6). They are **not** separate implementation phases.

## Reading Order

1. **[lore-reconciliation.md](lore-reconciliation.md)** — Start here. Master bridge between Aethelgard lore and implementation scope. References all other docs below.
2. **[enemy-translation.md](enemy-translation.md)** — Lore enemies → implementation-ready monster definitions with IDs, factions, stats, abilities.
3. **[hazard-registry.md](hazard-registry.md)** — Zone-specific hazards (contact, proximity, triggered, ambient) with full mechanic specs.
4. **[room-templates.md](room-templates.md)** — 124 room templates across all 9 realms with biome/zone assignments. Consumed incrementally per biome version.
5. **[descriptor-system.md](descriptor-system.md)** — Thematic modifiers, ambient sounds, description templates for procedural room generation. Integrated at **v0.19.6b**.

## How These Relate to Implementation Versions

| Version                        | What It Pulls From These Docs                                     |
| ------------------------------ | ----------------------------------------------------------------- |
| v0.19.0                        | Nothing (core infrastructure only)                                |
| v0.19.1 (Midgard)              | Midgard enemies, hazards, room templates from each registry       |
| v0.19.2 (Svartalfheim/Helheim) | Respective realm entries from each registry                       |
| v0.19.3 (Muspelheim/Niflheim)  | Respective realm entries from each registry                       |
| v0.19.4 (Jötunheim/Vanaheim)   | Respective realm entries from each registry                       |
| v0.19.5 (Alfheim/Asgard)       | Respective realm entries from each registry                       |
| v0.19.6 (Integration)          | Descriptor system integration (v0.19.6b), cross-biome transitions |
