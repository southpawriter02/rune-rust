# Data Assets

This directory contains game data assets used by the Dynamic Room Engine.

## Structure

```
data/
├── templates/          ← Room templates (20 JSON files)
├── biomes/             ← Biome definitions (JSON)
├── dialogues/          ← NPC dialogue trees (8 JSON files)
├── schemas/            ← Database schemas (SQL)
└── descriptors/        ← Descriptor content (SQL)
```

---

## Dialogues

NPC dialogue trees in JSON format.

| File | NPC | Nodes |
|------|-----|-------|
| `sigrun_dialogues.json` | Sigrun (Midgard Combine) | 8 |
| `astrid_dialogues.json` | Astrid | 6 |
| `bjorn_dialogues.json` | Bjorn | 5 |
| `eydis_dialogues.json` | Eydis | 5 |
| `gunnar_dialogues.json` | Gunnar | 5 |
| `kjartan_dialogues.json` | Kjartan | 6 |
| `rolf_dialogues.json` | Rolf | 7 |
| `thorvald_dialogues.json` | Thorvald | 5 |

**Schema:** See [dialogue-system.md](../docs/02-entities/dialogue-system.md)

---

## Templates

Room template JSON files organized by archetype.

| Archetype | Count | Purpose |
|-----------|-------|---------|
| corridors | 6 | Linear connectors |
| chambers | 5 | Combat/exploration |
| entry_halls | 3 | Sector entry |
| junctions | 2 | Branching points |
| boss_arenas | 2 | Boss encounters |
| secret_rooms | 2 | Hidden rewards |

**Schema:** See [core.md](../docs/07-environment/room-engine/core.md)

---

## Biomes

Biome definition files with:
- Available templates
- Descriptor categories
- Element pools (enemies, hazards, loot)
- Spawn rules

**Schema:** See [biomes/overview.md](../docs/07-environment/biomes/overview.md)

---

## Schemas

SQL DDL files defining database tables:

| Version | Purpose |
|---------|---------|
| v0.38.0 | Descriptor framework |
| v0.38.1 | Room description library |
| v0.38.x | Extended descriptor schemas |
| v0.39.1 | 3D spatial layout |
| v0.39.2 | Biome transitions |
| v0.39.3 | Content density |

---

## Descriptors

SQL data files with 1000+ text descriptors:

| Category | File |
|----------|------|
| Room fragments | v0.38.1_descriptor_fragments_content.sql |
| Atmospheric | v0.38.4_atmospheric_descriptors.sql |
| Combat | v0.38.12_combat_mechanics_descriptors_data.sql |
| Trauma | v0.38.14_trauma_*_content.sql |

**Schema:** See [descriptors.md](../docs/07-environment/room-engine/descriptors.md)
