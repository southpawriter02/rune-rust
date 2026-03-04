# Legacy Entity Manifest

## Metadata
* **Source:** `src/Infrastructure/RuneAndRust.Infrastructure/Persistence/Seeders/EntityTableSeeder.cs`
* **Status:** LEGACY (To Be Migrated)
* **Last Updated:** 2026-01-30

## Overview
This document contains the legacy entity definitions extracted from the codebase. These entities must be audited and migrated to V5.0 standards (d10 mechanics, Aetheric Manipulation patterns, Post-Glitch vocabulary).

## Entity Table

| ID | Name | Faction | Biome | Cost | HP | Atk | Def | Spd | Tags | Legacy Violations |
|----|------|---------|-------|------|----|-----|-----|-----|------|-------------------|
| `skeleton_minion` | Skeleton Minion | Undead Legion | Citadel | 3 | 15 | 5 | 2 | 5 | Undead, Brittle, Melee | Undead -> Undying? |
| `ghost_light` | Ghost Light | Undead Legion | Citadel | 2 | 8 | 3 | 0 | 8 | Undead, Flying, Glowing | Undead -> Undying? |
| `skeleton_warrior` | Skeleton Warrior | Undead Legion | Citadel | 10 | 30 | 10 | 5 | 6 | Undead, Armed, Melee | Undead -> Undying? |
| `skeleton_archer` | Skeleton Archer | Undead Legion | Citadel | 12 | 20 | 12 | 3 | 8 | Undead, Armed, Ranged | Undead -> Undying? |
| `wight` | Barrow Wight | Undead Legion | Citadel | 25 | 50 | 12 | 10 | 10 | Undead, Heavy, Fearsome | Undead -> Undying? |
| `death_knight` | Death Knight | Undead Legion | Citadel | 60 | 80 | 18 | 15 | 12 | Undead, Heavy, Melee, Commander | Undead -> Undying? |
| `lich_lord` | Lich Lord | Undead Legion | Citadel | 150 | 120 | 25 | 12 | 18 | Undead, Magical, Ranged, Commander | **Magical** (Dom 3), Undead -> Undying? |
| `rust_mite` | Rust Mite | Rust Swarm | The Roots | 2 | 10 | 4 | 1 | 4 | Mechanical, Swarm, Corrosive | Mechanical -> Iron-Walker? |
| `spore_cloud` | Spore Cloud | Rust Swarm | The Roots | 3 | 12 | 3 | 0 | 6 | Organic, Flying, Toxic | |
| `scavenger_drone` | Scavenger Drone | Rust Swarm | The Roots | 15 | 35 | 12 | 6 | 8 | Mechanical, Salvager, Melee | Mechanical -> Iron-Walker? |
| `fungal_stalker` | Fungal Stalker | Rust Swarm | The Roots | 18 | 40 | 14 | 5 | 12 | Organic, Stealthy, Melee | |
| `spitter_node` | Spitter Node | Rust Swarm | The Roots | 20 | 30 | 15 | 4 | 6 | Organic, Stationary, Ranged, Toxic | |
| `heavy_loader` | Heavy Loader Bot | Rust Swarm | The Roots | 55 | 90 | 20 | 18 | 6 | Mechanical, Heavy, Melee, Armored | **Bot** (Forbidden Term), Mechanical -> Iron-Walker? |
| `root_mother` | The Root Mother | Rust Swarm | The Roots | 140 | 150 | 22 | 15 | 14 | Organic, Massive, Spawner, Commander | |
| `fire_imp` | Fire Imp | Fire Cult | Muspelheim | 4 | 12 | 6 | 2 | 10 | Fire, Small, Ranged | Imp -> Entropy-Spawn? |
| `cinder` | Living Cinder | Fire Cult | Muspelheim | 3 | 8 | 5 | 0 | 8 | Fire, Flying, Explosive | |
| `flame_cultist` | Flame Cultist | Fire Cult | Muspelheim | 12 | 25 | 12 | 4 | 10 | Human, Fire, Ranged | |
| `magma_elemental` | Magma Elemental | Fire Cult | Muspelheim | 30 | 60 | 15 | 12 | 6 | Elemental, Fire, Heavy, Melee | |
| `ash_wraith` | Ash Wraith | Fire Cult | Muspelheim | 22 | 40 | 16 | 6 | 12 | Undead, Fire, Phasing, Melee | Undead -> Undying? |
| `flame_warden` | Flame Warden | Fire Cult | Muspelheim | 65 | 85 | 22 | 14 | 14 | Human, Fire, Heavy, Melee, Commander | |
| `ember_king` | The Ember King | Fire Cult | Muspelheim | 160 | 140 | 28 | 18 | 16 | Elemental, Fire, Massive, Ranged, Commander | |
| `ice_sprite` | Ice Sprite | Frost Horrors | Niflheim | 3 | 10 | 4 | 1 | 12 | Cold, Small, Flying, Freezing | |
| `frost_worm` | Frost Worm | Frost Horrors | Niflheim | 4 | 15 | 5 | 3 | 4 | Cold, Burrowing, Melee | |
| `frozen_corpse` | Frozen Corpse | Frost Horrors | Niflheim | 14 | 35 | 10 | 8 | 5 | Undead, Cold, Slow, Melee | Undead -> Undying? |
| `ice_wraith` | Ice Wraith | Frost Horrors | Niflheim | 20 | 30 | 14 | 4 | 14 | Undead, Cold, Phasing, Melee, Freezing | Undead -> Undying? |
| `frost_archer` | Frost Archer | Frost Horrors | Niflheim | 16 | 25 | 13 | 5 | 10 | Human, Cold, Ranged, Freezing | |
| `glacial_behemoth` | Glacial Behemoth | Frost Horrors | Niflheim | 70 | 100 | 18 | 20 | 6 | Elemental, Cold, Massive, Melee, Armored | |
| `frost_queen` | The Frost Queen | Frost Horrors | Niflheim | 155 | 130 | 24 | 16 | 18 | Undead, Cold, Magical, Ranged, Commander | **Magical** (Dom 3), Undead -> Undying? |
| `stone_sprite` | Stone Sprite | Awakened Titans | Jotunheim | 4 | 18 | 5 | 5 | 6 | Elemental, Stone, Small | |
| `rune_wisp` | Rune Wisp | Awakened Titans | Jotunheim | 5 | 12 | 7 | 0 | 14 | Magical, Flying, Ranged | **Magical** (Dom 3) |
| `giantkin` | Giantkin Warrior | Awakened Titans | Jotunheim | 25 | 55 | 16 | 10 | 8 | Giant, Heavy, Melee | Giant -> Jotun? |
| `runeguard` | Rune Guardian | Awakened Titans | Jotunheim | 30 | 70 | 12 | 15 | 10 | Construct, Magical, Armored, Melee | **Magical** (Dom 3) |
| `stormcaller` | Storm Caller | Awakened Titans | Jotunheim | 28 | 40 | 18 | 6 | 14 | Giant, Magical, Ranged, Lightning | **Magical** (Dom 3), Giant -> Jotun? |
| `jotun_champion` | Jotun Champion | Awakened Titans | Jotunheim | 80 | 120 | 24 | 18 | 10 | Giant, Heavy, Melee, Commander | Giant -> Jotun? |
| `titan_awakened` | Awakened Titan | Awakened Titans | Jotunheim | 200 | 200 | 35 | 25 | 12 | Giant, Massive, Ancient, Commander | Giant -> Jotun? |

## Migration Priorities
1. Remove all instances of **Magical** tag. Replace with specific Aetheric Manipulation descriptions in the new spec.
2. Rename "Undead" to "Undying" or "Husk" per Canonical Species rules.
3. Rename "Robot" (or implied bots) to "Iron-Walker" or "Servitor".
4. Rename "Giant" to "Jotun" (partially done in naming, but tags should reflect it).
