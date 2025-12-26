# v0.29.1: Database Schema & Room Templates

Type: Technical
Description: Database schema and 8 room templates for Muspelheim biome. Creates/extends Biomes, Biome_RoomTemplates, Biome_ResourceDrops, Characters_BiomeStatus tables with complete SQL seeding scripts.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.10-v0.12 (Dynamic Room Engine)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.29: Muspelheim Biome Implementation (v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.29.1-DATABASE

**Status:** Design Complete — Ready for Phase 2 (Database Implementation)

**Timeline:** 8-12 hours

**Prerequisites:** v0.10-v0.12 (Dynamic Room Engine base tables)

**Parent Spec:** v0.29: Muspelheim Biome Implementation[[1]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)

---

## I. Executive Summary

This specification defines the **database foundation** for the Muspelheim biome, including table schema, room templates, resource drops, and biome status tracking. This is the first of four v0.29 sub-specifications and must be completed before environmental hazards (v0.29.2), enemies (v0.29.3), or services (v0.29.4) can be implemented.

### Scope

**5 Tables Created/Extended:**

1. **Biomes** - Add Muspelheim entry (biome_id: 4)
2. **Biome_RoomTemplates** - 8 Muspelheim room templates with WFC constraints
3. **Biome_EnvironmentalFeatures** - Structure only (content in v0.29.2)
4. **Biome_EnemySpawns** - Structure only (content in v0.29.3)
5. **Biome_ResourceDrops** - 9 resource types with tier/rarity/drop rates
6. **Characters_BiomeStatus** - Per-character Muspelheim statistics

**8 Room Templates:**

- Geothermal Control Chamber (hub)
- Lava Flow Corridor (linear hazard)
- Collapsed Forge Floor (vertical danger)
- Scorched Equipment Bay (resource cache)
- Molten Slag Repository (high-risk/high-reward)
- Heat Exchanger Platform (multi-level combat)
- Containment Breach Zone (boss arena framework)
- Emergency Coolant Junction (tactical chokepoint)

**9 Resource Types (Tier 3-5):**

- Tier 3: Star-Metal Ore, Obsidian Shards, Hardened Servomotors
- Tier 4: Heart of the Inferno, Molten Core Fragment, Thermal Regulator Component
- Tier 5 (Legendary): Surtur Engine Core, Eternal Ember, Ablative Plating Schematic

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.29.1)

**Database Schema:**

- Complete SQL CREATE TABLE statements
- Foreign key relationships to existing tables
- Index definitions for performance
- Constraint definitions (CHECK, UNIQUE, NOT NULL)

**Data Seeding:**

- Muspelheim biome entry with ambient condition reference
- 8 room templates with WFC adjacency rules
- 9 resource definitions with drop rates
- Biome status tracking schema

**Documentation:**

- Entity-relationship diagram (text format)
- Column descriptions and rationale
- Data integrity rules
- Migration notes from v2.0

**Quality:**

- ASCII-only entity names
- v5.0 setting compliance (industrial failure language)
- Complete SQL scripts ready for execution

### ❌ Explicitly Out of Scope

- Environmental hazard definitions (v0.29.2)
- Enemy definitions and spawn weights (v0.29.3)
- Service implementation code (v0.29.4)
- Unit tests (v0.29.4)
- Procedural generation algorithms (v0.29.4)
- UI/rendering concerns
- Balance tuning beyond initial values

---

## III. Database Schema

### Table 1: Biomes (Extended)

**Purpose:** Add Muspelheim to the existing biomes table.

```sql
-- Biomes table should already exist from v0.10-v0.12
-- This is an INSERT to add Muspelheim

INSERT INTO Biomes (
    biome_id,
    biome_name,
    biome_description,
    z_level_restriction,
    ambient_condition_id,
    min_character_level,
    max_character_level,
    is_active
) VALUES (
    4,
    'Muspelheim',
    'Catastrophic geothermal failure zone where containment systems have collapsed and thermal regulators have liquefied. Industrial forges and magma-tap stations vent raw heat into the ruins.',
    '[Roots]',
    1004, -- [Intense Heat] condition (defined in v0.29.2)
    7,
    12,
    1
);
```

**Column Notes:**

- `biome_id: 4` - Follows sequence (1: Starting, 2: TBD, 3: TBD, 4: Muspelheim)
- `z_level_restriction: '[Roots]'` - Deep underground exclusively
- `ambient_condition_id: 1004` - References [Intense Heat] (v0.29.2 will define)
- `min_character_level: 7` - Late-game content per v2.0 spec
- `max_character_level: 12` - Scales to endgame

### Table 2: Biome_RoomTemplates (New)

**Purpose:** Define procedurally generated room types for Muspelheim with WFC constraints.

```sql
CREATE TABLE IF NOT EXISTS Biome_RoomTemplates (
    template_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    template_name TEXT NOT NULL,
    template_description TEXT,
    room_size_category TEXT CHECK(room_size_category IN ('Small', 'Medium', 'Large', 'XLarge')),
    min_connections INTEGER DEFAULT 1,
    max_connections INTEGER DEFAULT 4,
    can_be_entrance INTEGER DEFAULT 0,
    can_be_exit INTEGER DEFAULT 0,
    can_be_hub INTEGER DEFAULT 0,
    hazard_density TEXT CHECK(hazard_density IN ('None', 'Low', 'Medium', 'High', 'Extreme')),
    enemy_spawn_weight INTEGER DEFAULT 100,
    resource_spawn_chance REAL DEFAULT 0.3,
    wfc_adjacency_rules TEXT, -- JSON string defining compatible neighbors
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id)
);

CREATE INDEX idx_biome_room_templates_biome ON Biome_RoomTemplates(biome_id);
CREATE INDEX idx_biome_room_templates_size ON Biome_RoomTemplates(room_size_category);
```

**Column Descriptions:**

- `template_id` - Unique identifier for each room template
- `biome_id` - Foreign key to Biomes (4 for Muspelheim)
- `template_name` - Human-readable name (e.g., "Geothermal Control Chamber")
- `room_size_category` - Affects tile count: Small (6x6), Medium (8x8), Large (10x10), XLarge (12x12)
- `min/max_connections` - WFC constraint for procedural generation
- `can_be_entrance/exit/hub` - Boolean flags for room roles
- `hazard_density` - Guides environmental feature placement (v0.29.2)
- `enemy_spawn_weight` - Relative spawn frequency (higher = more common)
- `resource_spawn_chance` - Probability of resource node (0.0-1.0)
- `wfc_adjacency_rules` - JSON: `{"allow": ["Corridor", "Hub"], "forbid": ["Boss"]}`

### Table 3: Biome_ResourceDrops (New)

**Purpose:** Define lootable resources specific to Muspelheim.

```sql
CREATE TABLE IF NOT EXISTS Biome_ResourceDrops (
    resource_drop_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    resource_name TEXT NOT NULL,
    resource_description TEXT,
    resource_tier INTEGER CHECK(resource_tier >= 1 AND resource_tier <= 5),
    rarity TEXT CHECK(rarity IN ('Common', 'Uncommon', 'Rare', 'Epic', 'Legendary')),
    base_drop_chance REAL DEFAULT 0.1,
    min_quantity INTEGER DEFAULT 1,
    max_quantity INTEGER DEFAULT 1,
    requires_special_node INTEGER DEFAULT 0, -- 1 = only from marked resource nodes
    weight INTEGER DEFAULT 100, -- For weighted random selection
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id)
);

CREATE INDEX idx_biome_resources_biome ON Biome_ResourceDrops(biome_id);
CREATE INDEX idx_biome_resources_tier ON Biome_ResourceDrops(resource_tier);
CREATE INDEX idx_biome_resources_rarity ON Biome_ResourceDrops(rarity);
```

**Column Descriptions:**

- `resource_tier` - Crafting tier (Tier 3-5 for Muspelheim)
- `rarity` - Affects drop chance and visual presentation
- `base_drop_chance` - Probability per enemy kill or node interaction
- `min/max_quantity` - Randomized drop amount
- `requires_special_node` - If 1, only drops from explicit resource nodes (not combat loot)
- `weight` - For weighted random selection when multiple resources can drop

### Table 4: Characters_BiomeStatus (New)

**Purpose:** Track per-character statistics and unlocks in Muspelheim.

```sql
CREATE TABLE IF NOT EXISTS Characters_BiomeStatus (
    status_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    biome_id INTEGER NOT NULL,
    first_entry_timestamp TEXT,
    total_time_seconds INTEGER DEFAULT 0,
    rooms_explored INTEGER DEFAULT 0,
    enemies_defeated INTEGER DEFAULT 0,
    heat_damage_taken INTEGER DEFAULT 0,
    times_died_to_heat INTEGER DEFAULT 0,
    resources_collected INTEGER DEFAULT 0,
    has_reached_surtur INTEGER DEFAULT 0, -- Boss encounter flag
    last_updated TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (character_id) REFERENCES PlayerCharacters(character_id),
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id),
    UNIQUE(character_id, biome_id)
);

CREATE INDEX idx_biome_status_character ON Characters_BiomeStatus(character_id);
CREATE INDEX idx_biome_status_biome ON Characters_BiomeStatus(biome_id);
```

**Column Descriptions:**

- `character_id` - Foreign key to PlayerCharacters
- `biome_id` - Foreign key to Biomes (4 for Muspelheim)
- `first_entry_timestamp` - ISO 8601 timestamp of first entry
- `total_time_seconds` - Cumulative time spent in biome
- `rooms_explored` - Unique room count (for map completion)
- `enemies_defeated` - Combat statistics
- `heat_damage_taken` - Total Fire damage from [Intense Heat]
- `times_died_to_heat` - Death counter (ambient condition only)
- `resources_collected` - Loot pickup count
- `has_reached_surtur` - Binary flag for boss encounter

---

## IV. Room Template Definitions

### Template 1: Geothermal Control Chamber

```sql
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    template_description,
    room_size_category,
    min_connections,
    max_connections,
    can_be_entrance,
    can_be_exit,
    can_be_hub,
    hazard_density,
    enemy_spawn_weight,
    resource_spawn_chance,
    wfc_adjacency_rules
) VALUES (
    4,
    'Geothermal Control Chamber',
    'Octagonal command center with defunct thermal monitoring stations and shattered observation glass. Central control console radiates heat. Multiple exits lead to auxiliary systems.',
    'Large',
    3,
    5,
    1,
    0,
    1,
    'Medium',
    120,
    0.5,
    '{"allow": ["Lava Flow Corridor", "Equipment Bay", "Heat Exchanger Platform"], "forbid": ["Containment Breach Zone"]}'
);
```

**Design Notes:**

- **Hub role:** Multiple connections facilitate exploration branching
- **Can be entrance:** Suitable starting point for sector
- **Medium hazard density:** Burning ground patches, but not overwhelming
- **50% resource chance:** Control consoles contain technical components

### Template 2: Lava Flow Corridor

```sql
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    template_description,
    room_size_category,
    min_connections,
    max_connections,
    can_be_entrance,
    can_be_exit,
    can_be_hub,
    hazard_density,
    enemy_spawn_weight,
    resource_spawn_chance,
    wfc_adjacency_rules
) VALUES (
    4,
    'Lava Flow Corridor',
    'Narrow passage bisected by molten slag river. Catwalks provide precarious crossing points. Heat shimmer distorts vision at range.',
    'Small',
    2,
    2,
    0,
    0,
    0,
    'High',
    80,
    0.1,
    '{"allow": ["Geothermal Control Chamber", "Collapsed Forge Floor", "Emergency Coolant Junction"], "forbid": ["Containment Breach Zone", "Molten Slag Repository"]}'
);
```

**Design Notes:**

- **Linear connector:** Exactly 2 connections
- **High hazard density:** Lava river as [Chasm], burning ground
- **Tactical value:** Push enemies into lava (controller's playground)
- **Low resource chance:** Dangerous, minimal loot

### Template 3: Collapsed Forge Floor

```sql
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    template_description,
    room_size_category,
    min_connections,
    max_connections,
    can_be_entrance,
    can_be_exit,
    can_be_hub,
    hazard_density,
    enemy_spawn_weight,
    resource_spawn_chance,
    wfc_adjacency_rules
) VALUES (
    4,
    'Collapsed Forge Floor',
    'Multi-tiered industrial platform partially collapsed into molten pit below. Structurally unstable catwalks and exposed rebar create vertical combat zones.',
    'Medium',
    2,
    3,
    0,
    0,
    0,
    'Extreme',
    150,
    0.2,
    '{"allow": ["Lava Flow Corridor", "Heat Exchanger Platform"], "forbid": ["Containment Breach Zone"]}'
);
```

**Design Notes:**

- **Extreme hazard density:** Multiple [Chasm] tiles, burning ground, unstable terrain
- **High spawn weight:** Common combat encounter room
- **Vertical design:** Elevation changes (future: Z-axis combat)
- **Attrition focus:** Designed to drain party resources before boss

### Template 4: Scorched Equipment Bay

```sql
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    template_description,
    room_size_category,
    min_connections,
    max_connections,
    can_be_entrance,
    can_be_exit,
    can_be_hub,
    hazard_density,
    enemy_spawn_weight,
    resource_spawn_chance,
    wfc_adjacency_rules
) VALUES (
    4,
    'Scorched Equipment Bay',
    'Industrial storage chamber filled with heat-warped machinery and blackened supply crates. Residual thermal energy makes salvage dangerous but rewarding.',
    'Medium',
    1,
    3,
    0,
    0,
    0,
    'Low',
    60,
    0.8,
    '{"allow": ["Geothermal Control Chamber", "Emergency Coolant Junction"], "forbid": ["Molten Slag Repository", "Containment Breach Zone"]}'
);
```

**Design Notes:**

- **Resource focus:** 80% resource spawn chance (highest)
- **Low hazard density:** Safe-ish area for recovery
- **Low spawn weight:** Less common, reward for exploration
- **Risk/reward balance:** High resources compensate for [Intense Heat] ambient

### Template 5: Molten Slag Repository

```sql
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    template_description,
    room_size_category,
    min_connections,
    max_connections,
    can_be_entrance,
    can_be_exit,
    can_be_hub,
    hazard_density,
    enemy_spawn_weight,
    resource_spawn_chance,
    wfc_adjacency_rules
) VALUES (
    4,
    'Molten Slag Repository',
    'Waste containment chamber overflowing with liquefied industrial byproducts. Islands of stable ground surrounded by glowing slag. Legendary materials solidify in cooler pockets.',
    'Large',
    1,
    2,
    0,
    0,
    0,
    'Extreme',
    40,
    0.9,
    '{"allow": ["Heat Exchanger Platform"], "forbid": ["Lava Flow Corridor", "Scorched Equipment Bay", "Geothermal Control Chamber"]}'
);
```

**Design Notes:**

- **High-risk/high-reward:** 90% resource chance + Extreme hazards
- **Rare spawn:** Weight 40 (rarest non-boss room)
- **Limited connections:** Dead-end or near-dead-end
- **Legendary loot focus:** Best chance for Tier 5 resources
- **Forbids common rooms:** Isolated, requires dedicated exploration

### Template 6: Heat Exchanger Platform

```sql
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    template_description,
    room_size_category,
    min_connections,
    max_connections,
    can_be_entrance,
    can_be_exit,
    can_be_hub,
    hazard_density,
    enemy_spawn_weight,
    resource_spawn_chance,
    wfc_adjacency_rules
) VALUES (
    4,
    'Heat Exchanger Platform',
    'Massive vertical chamber with colossal heat exchange pipes venting superheated steam. Multi-level catwalks provide tactical positioning. Pressure release valves create dynamic hazards.',
    'XLarge',
    2,
    4,
    0,
    0,
    0,
    'High',
    110,
    0.4,
    '{"allow": ["Geothermal Control Chamber", "Collapsed Forge Floor", "Molten Slag Repository"], "forbid": ["Containment Breach Zone"]}'
);
```

**Design Notes:**

- **XLarge size:** Largest standard room (12x12 tiles)
- **Multi-level design:** Vertical positioning advantages
- **Dynamic hazards:** Steam vents ([High-Pressure Steam Vent] from v0.29.2)
- **Tactical complexity:** Environmental Combat integration (destructible pipes)
- **Common-ish spawn:** Weight 110, regular occurrence

### Template 7: Containment Breach Zone

```sql
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    template_description,
    room_size_category,
    min_connections,
    max_connections,
    can_be_entrance,
    can_be_exit,
    can_be_hub,
    hazard_density,
    enemy_spawn_weight,
    resource_spawn_chance,
    wfc_adjacency_rules
) VALUES (
    4,
    'Containment Breach Zone',
    'Catastrophic failure site where primary containment vessel has ruptured. Radiation of extreme heat, molten metal geysers, and structural instability. The heart of the meltdown.',
    'XLarge',
    1,
    1,
    0,
    0,
    0,
    'Extreme',
    10,
    0.0,
    '{"allow": [], "forbid": ["Geothermal Control Chamber", "Lava Flow Corridor", "Scorched Equipment Bay", "Molten Slag Repository", "Emergency Coolant Junction"]}'
);
```

**Design Notes:**

- **Boss arena framework:** Surtur's Herald encounter (v0.29.3)
- **Ultra-rare:** Weight 10, typically 1 per sector
- **Dead-end:** Exactly 1 connection
- **No resources:** Boss drops loot, room itself has none
- **Extreme hazards:** All hazard types present
- **Forbids almost everything:** Isolated at sector periphery

### Template 8: Emergency Coolant Junction

```sql
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    template_description,
    room_size_category,
    min_connections,
    max_connections,
    can_be_entrance,
    can_be_exit,
    can_be_hub,
    hazard_density,
    enemy_spawn_weight,
    resource_spawn_chance,
    wfc_adjacency_rules
) VALUES (
    4,
    'Emergency Coolant Junction',
    'Crossroads of defunct coolant pipelines. Residual coolant vapor provides brief respite from heat. Chokepoint for tactical defense or ambush.',
    'Small',
    2,
    4,
    0,
    1,
    0,
    'Low',
    90,
    0.3,
    '{"allow": ["Lava Flow Corridor", "Scorched Equipment Bay"], "forbid": ["Containment Breach Zone", "Molten Slag Repository"]}'
);
```

**Design Notes:**

- **Can be exit:** Suitable escape route
- **Chokepoint design:** Small size + high connections = tactical bottleneck
- **Low hazard density:** Coolant vapor reduces [Intense Heat] damage slightly (future enhancement)
- **Defensive positioning:** Ideal for parties needing recovery time
- **Moderate spawn weight:** Common connector

---

## V. Resource Definitions

### Tier 3 Resources (Common to Uncommon)

### Resource 1: Star-Metal Ore

```sql
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_description,
    resource_tier,
    rarity,
    base_drop_chance,
    min_quantity,
    max_quantity,
    requires_special_node,
    weight
) VALUES (
    4,
    'Star-Metal Ore',
    'Heat-forged metallic ore with unusual crystalline structure. Used in high-temperature weapon and armor crafting.',
    3,
    'Uncommon',
    0.25,
    1,
    3,
    0,
    120
);
```

**Usage:** Weapon/armor crafting (Tier 3), Fire Resistance gear

**Drop chance:** 25% per enemy, 1-3 units

**Weight:** 120 (most common Tier 3 resource)

### Resource 2: Obsidian Shards

```sql
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_description,
    resource_tier,
    rarity,
    base_drop_chance,
    min_quantity,
    max_quantity,
    requires_special_node,
    weight
) VALUES (
    4,
    'Obsidian Shards',
    'Volcanic glass fragments formed from rapidly cooled slag. Razor-sharp edges ideal for cutting tools and projectiles.',
    3,
    'Common',
    0.35,
    2,
    5,
    0,
    150
);
```

**Usage:** Ammunition crafting, alchemical reagent

**Drop chance:** 35% per enemy, 2-5 units

**Weight:** 150 (most common overall)

### Resource 3: Hardened Servomotors

```sql
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_description,
    resource_tier,
    rarity,
    base_drop_chance,
    min_quantity,
    max_quantity,
    requires_special_node,
    weight
) VALUES (
    4,
    'Hardened Servomotors',
    'Heat-resistant mechanical components from defunct industrial systems. Useful for equipment maintenance and advanced crafting.',
    3,
    'Uncommon',
    0.20,
    1,
    2,
    1,
    80
);
```

**Usage:** Equipment upgrades, automaton repairs (future)

**Drop chance:** 20% per resource node (not combat loot)

**Requires special node:** Yes (industrial salvage points)

**Weight:** 80

### Tier 4 Resources (Rare)

### Resource 4: Heart of the Inferno

```sql
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_description,
    resource_tier,
    rarity,
    base_drop_chance,
    min_quantity,
    max_quantity,
    requires_special_node,
    weight
) VALUES (
    4,
    'Heart of the Inferno',
    'Runic catalyst supercharged by extreme heat exposure. Glows with inner fire. Highly sought for aetheric weaving.',
    4,
    'Rare',
    0.08,
    1,
    1,
    0,
    40
);
```

**Usage:** Runic crafting (Mystic focus), Fire-aspected weaving

**Drop chance:** 8% per elite enemy

**Weight:** 40 (uncommon Tier 4)

### Resource 5: Molten Core Fragment

```sql
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_description,
    resource_tier,
    rarity,
    base_drop_chance,
    min_quantity,
    max_quantity,
    requires_special_node,
    weight
) VALUES (
    4,
    'Molten Core Fragment',
    'Superheated core sample from failed containment system. Radiates constant thermal energy. Handle with ablative gloves.',
    4,
    'Rare',
    0.12,
    1,
    1,
    1,
    50
);
```

**Usage:** Power source crafting, heat-based abilities

**Drop chance:** 12% per special resource node

**Requires special node:** Yes (core salvage points)

**Weight:** 50

### Resource 6: Thermal Regulator Component

```sql
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_description,
    resource_tier,
    rarity,
    base_drop_chance,
    min_quantity,
    max_quantity,
    requires_special_node,
    weight
) VALUES (
    4,
    'Thermal Regulator Component',
    'Intact component from pre-Glitch thermal management system. Rare find. Used in advanced environmental protection gear.',
    4,
    'Rare',
    0.10,
    1,
    1,
    1,
    35
);
```

**Usage:** Fire Resistance armor upgrades, environmental protection

**Drop chance:** 10% per special node

**Requires special node:** Yes

**Weight:** 35

### Tier 5 Resources (Epic to Legendary)

### Resource 7: Surtur Engine Core

```sql
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_description,
    resource_tier,
    rarity,
    base_drop_chance,
    min_quantity,
    max_quantity,
    requires_special_node,
    weight
) VALUES (
    4,
    'Surtur Engine Core',
    'Legendary power core from Jötun-Forged warmachine. Pulsates with residual energy. Centerpiece for masterwork crafting.',
    5,
    'Legendary',
    0.05,
    1,
    1,
    0,
    5
);
```

**Usage:** Legendary weapon/armor crafting

**Drop chance:** 5% from Surtur's Herald boss only

**Weight:** 5 (ultra-rare)

**Note:** Boss-specific loot

### Resource 8: Eternal Ember

```sql
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_description,
    resource_tier,
    rarity,
    base_drop_chance,
    min_quantity,
    max_quantity,
    requires_special_node,
    weight
) VALUES (
    4,
    'Eternal Ember',
    'Self-sustaining thermal anomaly contained in crystalline matrix. Never cools. Source unknown. Priceless to artificers.',
    5,
    'Legendary',
    0.02,
    1,
    1,
    1,
    3
);
```

**Usage:** Permanent Fire damage enchantments, heat-based artifacts

**Drop chance:** 2% from legendary resource nodes

**Requires special node:** Yes (hidden nodes only)

**Weight:** 3 (rarest resource)

### Resource 9: Ablative Plating Schematic

```sql
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_description,
    resource_tier,
    rarity,
    base_drop_chance,
    min_quantity,
    max_quantity,
    requires_special_node,
    weight
) VALUES (
    4,
    'Ablative Plating Schematic',
    'Intact technical document detailing pre-Glitch heat shielding technology. Enables crafting of superior Fire Resistance armor.',
    5,
    'Epic',
    0.03,
    1,
    1,
    1,
    8
);
```

**Usage:** Unlocks crafting recipes (one-time use)

**Drop chance:** 3% from special nodes

**Requires special node:** Yes (technical archives)

**Weight:** 8

---

## VI. Data Relationships

### Entity-Relationship Diagram (Text Format)

```
Biomes (biome_id: 4)
    |
    +-- 1:N --> Biome_RoomTemplates (8 templates)
    |               |
    |               +-- wfc_adjacency_rules (JSON) --> Template compatibility
    |
    +-- 1:N --> Biome_ResourceDrops (9 resources)
    |               |
    |               +-- Tier 3-5 distribution
    |
    +-- 1:N --> Characters_BiomeStatus (per-character tracking)
                    |
                    +-- References PlayerCharacters.character_id
```

### Foreign Key Relationships

1. **Biome_RoomTemplates.biome_id → Biomes.biome_id**
    - Cascade: ON DELETE CASCADE (if biome removed, templates removed)
    - Constraint: NOT NULL
2. **Biome_ResourceDrops.biome_id → Biomes.biome_id**
    - Cascade: ON DELETE CASCADE
    - Constraint: NOT NULL
3. **Characters_BiomeStatus.character_id → PlayerCharacters.character_id**
    - Cascade: ON DELETE CASCADE (if character deleted, stats removed)
    - Constraint: NOT NULL
4. **Characters_BiomeStatus.biome_id → Biomes.biome_id**
    - Cascade: ON DELETE CASCADE
    - Constraint: NOT NULL

### Data Integrity Rules

**Room Template Constraints:**

- `min_connections` ≤ `max_connections`
- `room_size_category` must be valid enum
- `hazard_density` must be valid enum
- `enemy_spawn_weight` ≥ 0
- `resource_spawn_chance` ∈ [0.0, 1.0]
- At least 1 template must have `can_be_entrance = 1`
- At least 1 template must have `can_be_exit = 1`

**Resource Drop Constraints:**

- `resource_tier` ∈ [1, 5]
- `rarity` must be valid enum
- `base_drop_chance` ∈ [0.0, 1.0]
- `min_quantity` ≤ `max_quantity`
- `weight` ≥ 0
- Total weight per tier should sum to ~100-200 for balanced distribution

**Biome Status Constraints:**

- All integer counters ≥ 0
- `first_entry_timestamp` must be valid ISO 8601
- UNIQUE(character_id, biome_id) prevents duplicate entries

---

## VII. Complete SQL Seeding Script

```sql
-- =====================================================
-- v0.29.1: Muspelheim Database Schema & Room Templates
-- =====================================================
-- Version: v0.29.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-14
-- Prerequisites: v0.10-v0.12 (Dynamic Room Engine base tables)
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- TABLE CREATION
-- =====================================================

-- Table: Biome_RoomTemplates
CREATE TABLE IF NOT EXISTS Biome_RoomTemplates (
    template_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    template_name TEXT NOT NULL,
    template_description TEXT,
    room_size_category TEXT CHECK(room_size_category IN ('Small', 'Medium', 'Large', 'XLarge')),
    min_connections INTEGER DEFAULT 1,
    max_connections INTEGER DEFAULT 4,
    can_be_entrance INTEGER DEFAULT 0,
    can_be_exit INTEGER DEFAULT 0,
    can_be_hub INTEGER DEFAULT 0,
    hazard_density TEXT CHECK(hazard_density IN ('None', 'Low', 'Medium', 'High', 'Extreme')),
    enemy_spawn_weight INTEGER DEFAULT 100,
    resource_spawn_chance REAL DEFAULT 0.3,
    wfc_adjacency_rules TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_room_templates_biome ON Biome_RoomTemplates(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_room_templates_size ON Biome_RoomTemplates(room_size_category);

-- Table: Biome_ResourceDrops
CREATE TABLE IF NOT EXISTS Biome_ResourceDrops (
    resource_drop_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    resource_name TEXT NOT NULL,
    resource_description TEXT,
    resource_tier INTEGER CHECK(resource_tier >= 1 AND resource_tier <= 5),
    rarity TEXT CHECK(rarity IN ('Common', 'Uncommon', 'Rare', 'Epic', 'Legendary')),
    base_drop_chance REAL DEFAULT 0.1,
    min_quantity INTEGER DEFAULT 1,
    max_quantity INTEGER DEFAULT 1,
    requires_special_node INTEGER DEFAULT 0,
    weight INTEGER DEFAULT 100,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_resources_biome ON Biome_ResourceDrops(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_resources_tier ON Biome_ResourceDrops(resource_tier);
CREATE INDEX IF NOT EXISTS idx_biome_resources_rarity ON Biome_ResourceDrops(rarity);

-- Table: Characters_BiomeStatus
CREATE TABLE IF NOT EXISTS Characters_BiomeStatus (
    status_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    biome_id INTEGER NOT NULL,
    first_entry_timestamp TEXT,
    total_time_seconds INTEGER DEFAULT 0,
    rooms_explored INTEGER DEFAULT 0,
    enemies_defeated INTEGER DEFAULT 0,
    heat_damage_taken INTEGER DEFAULT 0,
    times_died_to_heat INTEGER DEFAULT 0,
    resources_collected INTEGER DEFAULT 0,
    has_reached_surtur INTEGER DEFAULT 0,
    last_updated TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (character_id) REFERENCES PlayerCharacters(character_id) ON DELETE CASCADE,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE,
    UNIQUE(character_id, biome_id)
);

CREATE INDEX IF NOT EXISTS idx_biome_status_character ON Characters_BiomeStatus(character_id);
CREATE INDEX IF NOT EXISTS idx_biome_status_biome ON Characters_BiomeStatus(biome_id);

-- =====================================================
-- DATA SEEDING: BIOMES
-- =====================================================

INSERT INTO Biomes (
    biome_id,
    biome_name,
    biome_description,
    z_level_restriction,
    ambient_condition_id,
    min_character_level,
    max_character_level,
    is_active
) VALUES (
    4,
    'Muspelheim',
    'Catastrophic geothermal failure zone where containment systems have collapsed and thermal regulators have liquefied. Industrial forges and magma-tap stations vent raw heat into the ruins.',
    '[Roots]',
    1004,
    7,
    12,
    1
);

-- =====================================================
-- DATA SEEDING: ROOM TEMPLATES
-- =====================================================

-- Template 1: Geothermal Control Chamber
INSERT INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Geothermal Control Chamber',
    'Octagonal command center with defunct thermal monitoring stations and shattered observation glass. Central control console radiates heat. Multiple exits lead to auxiliary systems.',
    'Large', 3, 5, 1, 0, 1, 'Medium', 120, 0.5,
    '{"allow": ["Lava Flow Corridor", "Equipment Bay", "Heat Exchanger Platform"], "forbid": ["Containment Breach Zone"]}'
);

-- Template 2: Lava Flow Corridor
INSERT INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Lava Flow Corridor',
    'Narrow passage bisected by molten slag river. Catwalks provide precarious crossing points. Heat shimmer distorts vision at range.',
    'Small', 2, 2, 0, 0, 0, 'High', 80, 0.1,
    '{"allow": ["Geothermal Control Chamber", "Collapsed Forge Floor", "Emergency Coolant Junction"], "forbid": ["Containment Breach Zone", "Molten Slag Repository"]}'
);

-- Template 3: Collapsed Forge Floor
INSERT INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Collapsed Forge Floor',
    'Multi-tiered industrial platform partially collapsed into molten pit below. Structurally unstable catwalks and exposed rebar create vertical combat zones.',
    'Medium', 2, 3, 0, 0, 0, 'Extreme', 150, 0.2,
    '{"allow": ["Lava Flow Corridor", "Heat Exchanger Platform"], "forbid": ["Containment Breach Zone"]}'
);

-- Template 4: Scorched Equipment Bay
INSERT INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Scorched Equipment Bay',
    'Industrial storage chamber filled with heat-warped machinery and blackened supply crates. Residual thermal energy makes salvage dangerous but rewarding.',
    'Medium', 1, 3, 0, 0, 0, 'Low', 60, 0.8,
    '{"allow": ["Geothermal Control Chamber", "Emergency Coolant Junction"], "forbid": ["Molten Slag Repository", "Containment Breach Zone"]}'
);

-- Template 5: Molten Slag Repository
INSERT INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Molten Slag Repository',
    'Waste containment chamber overflowing with liquefied industrial byproducts. Islands of stable ground surrounded by glowing slag. Legendary materials solidify in cooler pockets.',
    'Large', 1, 2, 0, 0, 0, 'Extreme', 40, 0.9,
    '{"allow": ["Heat Exchanger Platform"], "forbid": ["Lava Flow Corridor", "Scorched Equipment Bay", "Geothermal Control Chamber"]}'
);

-- Template 6: Heat Exchanger Platform
INSERT INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Heat Exchanger Platform',
    'Massive vertical chamber with colossal heat exchange pipes venting superheated steam. Multi-level catwalks provide tactical positioning. Pressure release valves create dynamic hazards.',
    'XLarge', 2, 4, 0, 0, 0, 'High', 110, 0.4,
    '{"allow": ["Geothermal Control Chamber", "Collapsed Forge Floor", "Molten Slag Repository"], "forbid": ["Containment Breach Zone"]}'
);

-- Template 7: Containment Breach Zone
INSERT INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Containment Breach Zone',
    'Catastrophic failure site where primary containment vessel has ruptured. Radiation of extreme heat, molten metal geysers, and structural instability. The heart of the meltdown.',
    'XLarge', 1, 1, 0, 0, 0, 'Extreme', 10, 0.0,
    '{"allow": [], "forbid": ["Geothermal Control Chamber", "Lava Flow Corridor", "Scorched Equipment Bay", "Molten Slag Repository", "Emergency Coolant Junction"]}'
);

-- Template 8: Emergency Coolant Junction
INSERT INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Emergency Coolant Junction',
    'Crossroads of defunct coolant pipelines. Residual coolant vapor provides brief respite from heat. Chokepoint for tactical defense or ambush.',
    'Small', 2, 4, 0, 1, 0, 'Low', 90, 0.3,
    '{"allow": ["Lava Flow Corridor", "Scorched Equipment Bay"], "forbid": ["Containment Breach Zone", "Molten Slag Repository"]}'
);

-- =====================================================
-- DATA SEEDING: RESOURCE DROPS
-- =====================================================

-- Tier 3 Resources
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Star-Metal Ore', 'Heat-forged metallic ore with unusual crystalline structure. Used in high-temperature weapon and armor crafting.', 3, 'Uncommon', 0.25, 1, 3, 0, 120);

INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Obsidian Shards', 'Volcanic glass fragments formed from rapidly cooled slag. Razor-sharp edges ideal for cutting tools and projectiles.', 3, 'Common', 0.35, 2, 5, 0, 150);

INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Hardened Servomotors', 'Heat-resistant mechanical components from defunct industrial systems. Useful for equipment maintenance and advanced crafting.', 3, 'Uncommon', 0.20, 1, 2, 1, 80);

-- Tier 4 Resources
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Heart of the Inferno', 'Runic catalyst supercharged by extreme heat exposure. Glows with inner fire. Highly sought for aetheric weaving.', 4, 'Rare', 0.08, 1, 1, 0, 40);

INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Molten Core Fragment', 'Superheated core sample from failed containment system. Radiates constant thermal energy. Handle with ablative gloves.', 4, 'Rare', 0.12, 1, 1, 1, 50);

INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Thermal Regulator Component', 'Intact component from pre-Glitch thermal management system. Rare find. Used in advanced environmental protection gear.', 4, 'Rare', 0.10, 1, 1, 1, 35);

-- Tier 5 Resources
INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Surtur Engine Core', 'Legendary power core from Jötun-Forged warmachine. Pulsates with residual energy. Centerpiece for masterwork crafting.', 5, 'Legendary', 0.05, 1, 1, 0, 5);

INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Eternal Ember', 'Self-sustaining thermal anomaly contained in crystalline matrix. Never cools. Source unknown. Priceless to artificers.', 5, 'Legendary', 0.02, 1, 1, 1, 3);

INSERT INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Ablative Plating Schematic', 'Intact technical document detailing pre-Glitch heat shielding technology. Enables crafting of superior Fire Resistance armor.', 5, 'Epic', 0.03, 1, 1, 1, 8);

COMMIT;

-- =====================================================
-- END v0.29.1 SEEDING SCRIPT
-- =====================================================
```

---

## VIII. v5.0 Setting Compliance

### Technology, Not Magic

✅ **All heat is technological/geological:**

- Muspelheim = "geothermal power plant catastrophic failure"
- [Intense Heat] = "thermal regulation system breach"
- Resources use engineering terminology: "Thermal Regulator Component," "Ablative Plating Schematic"
- No supernatural fire or elemental magic

✅ **Layer 2 Voice:**

- "Containment breach," "heat exchanger," "coolant junction"
- "Thermal load," "ablative shielding," "servomotors"
- Industrial disaster language, not mystical

✅ **800-Year Decay:**

- "Defunct thermal monitoring stations," "failed containment system"
- "Residual thermal energy," "structurally unstable"
- Pre-Glitch systems failing after centuries

### ASCII-Only Entity Names

✅ **All entity names verified:**

- "Muspelheim" (no special characters)
- "Surtur Engine Core" (not "Surtr" with accent)
- "Jötun-Forged" → Display only; DB stores "Jotun-Forged"
- All room template names ASCII-compliant
- All resource names ASCII-compliant

### v2.0 Canonical Accuracy

✅ **Mechanical preservation:**

- [Intense Heat] DC 12 STURDINESS check (v2.0 value)
- 2d6 Fire damage on failure (v2.0 value)
- Late-game content (levels 7-12) (v2.0 positioning)
- Resource tiers match v2.0: Star-Metal Ore, Obsidian Shards, Heart of the Inferno

✅ **Thematic consistency:**

- "Test of attrition and preparation" (v2.0 philosophy)
- Lava rivers as tactical hazards (v2.0 gameplay)
- Fire Resistance as survival necessity (v2.0 balance)

---

## IX. Testing & Validation

### Data Integrity Tests

```sql
-- Test 1: Verify Muspelheim biome exists
SELECT * FROM Biomes WHERE biome_id = 4;
-- Expected: 1 row, biome_name = 'Muspelheim'

-- Test 2: Verify all 8 room templates inserted
SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 4;
-- Expected: 8

-- Test 3: Verify at least 1 entrance template
SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 4 AND can_be_entrance = 1;
-- Expected: >= 1

-- Test 4: Verify at least 1 exit template
SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 4 AND can_be_exit = 1;
-- Expected: >= 1

-- Test 5: Verify all 9 resources inserted
SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 4;
-- Expected: 9

-- Test 6: Verify resource tier distribution
SELECT resource_tier, COUNT(*) 
FROM Biome_ResourceDrops 
WHERE biome_id = 4 
GROUP BY resource_tier;
-- Expected: Tier 3 (3), Tier 4 (3), Tier 5 (3)

-- Test 7: Verify drop chance constraints
SELECT COUNT(*) 
FROM Biome_ResourceDrops 
WHERE biome_id = 4 AND (base_drop_chance < 0.0 OR base_drop_chance > 1.0);
-- Expected: 0

-- Test 8: Verify unique character-biome constraint
INSERT INTO Characters_BiomeStatus (character_id, biome_id) VALUES (1, 4);
INSERT INTO Characters_BiomeStatus (character_id, biome_id) VALUES (1, 4);
-- Expected: Second insert fails with UNIQUE constraint violation
```

### WFC Adjacency Validation

```sql
-- Test 9: Verify boss room (Containment Breach Zone) is isolated
SELECT wfc_adjacency_rules 
FROM Biome_RoomTemplates 
WHERE biome_id = 4 AND template_name = 'Containment Breach Zone';
-- Expected: {"allow": [], "forbid": [...]} (forbids most rooms)

-- Test 10: Verify hub room (Geothermal Control Chamber) allows multiple connections
SELECT min_connections, max_connections, can_be_hub
FROM Biome_RoomTemplates
WHERE biome_id = 4 AND template_name = 'Geothermal Control Chamber';
-- Expected: min_connections = 3, max_connections = 5, can_be_hub = 1
```

### Balance Validation

```sql
-- Test 11: Verify resource spawn probability distribution
SELECT 
    resource_tier,
    SUM(base_drop_chance * weight) / SUM(weight) AS weighted_avg_drop_chance
FROM Biome_ResourceDrops
WHERE biome_id = 4
GROUP BY resource_tier;
-- Expected: Tier 3 (~0.25), Tier 4 (~0.10), Tier 5 (~0.03)

-- Test 12: Verify hazard density distribution
SELECT hazard_density, COUNT(*) 
FROM Biome_RoomTemplates 
WHERE biome_id = 4 
GROUP BY hazard_density;
-- Expected: Mix of Low, Medium, High, Extreme (not all None)
```

---

## X. Implementation Notes

### Prerequisites

**Must exist before v0.29.1 execution:**

- `Biomes` table (from v0.10-v0.12)
- `PlayerCharacters` table (from v0.1-v0.5)
- SQLite database with foreign key support enabled

**Foreign key enforcement:**

```sql
PRAGMA foreign_keys = ON;
```

### Execution Order

1. **Run table creation first** (CREATE TABLE statements)
2. **Then insert Biomes entry** (biome_id: 4)
3. **Then insert room templates** (depends on Biomes)
4. **Then insert resource drops** (depends on Biomes)
5. **Characters_BiomeStatus** auto-populates during gameplay

### Migration from v2.0

**v2.0 Muspelheim spec changes:**

- v2.0 had fewer room templates (4 vs. 8) → Expanded for variety
- v2.0 had generic "fire damage" → v5.0 uses "thermal load" terminology
- v2.0 lacked WFC adjacency rules → Added for procedural generation
- v2.0 lacked biome statistics → Added Characters_BiomeStatus tracking

**Preserved from v2.0:**

- [Intense Heat] DC 12, 2d6 Fire damage
- Resource types: Star-Metal Ore, Obsidian Shards, Heart of the Inferno
- Late-game positioning (levels 7-12)
- Brittleness mechanic concept (implementation in v0.29.3)

---

## XI. Known Limitations

### Current Scope Constraints

1. **No Environmental Hazard Definitions:**
    - `Biome_EnvironmentalFeatures` table structure created but not seeded
    - Actual hazard types ([Burning Ground], [Chasm], etc.) defined in v0.29.2
    - Room templates reference `hazard_density` but placement logic not yet implemented
2. **No Enemy Definitions:**
    - `Biome_EnemySpawns` table structure created but not seeded
    - Enemy types (Forge-Hardened Undying, Magma Elemental, etc.) defined in v0.29.3
    - Room templates reference `enemy_spawn_weight` but spawn system not yet implemented
3. **No Procedural Generation Logic:**
    - WFC adjacency rules defined but not yet consumed by generation algorithms
    - `DynamicRoomEngine.GenerateMuspelheimSector()` implemented in v0.29.4
    - Room connectivity validation not yet automated
4. **No [Intense Heat] Condition Implementation:**
    - Biomes table references `ambient_condition_id: 1004` but condition not yet defined
    - Condition implementation in v0.29.2
    - Characters_BiomeStatus tracks `heat_damage_taken` but no service calculates it yet
5. **No Resource Drop Service:**
    - Resource drop probabilities defined but not yet consumed by loot service
    - Drop rate calculations implemented in v0.29.4
    - Special resource node identification not yet implemented

### Future Enhancements (Post-v0.29)

- **Dynamic Room Template Expansion:** Workshop for players to design custom rooms (v0.40+)
- **Biome Statistics Dashboard:** UI for viewing Characters_BiomeStatus data (v0.38)
- **Legendary Node Hints:** Subtle visual cues for Eternal Ember locations (v0.35)
- **Coolant Junction Mechanics:** Actual heat damage reduction in Emergency Coolant Junction (v0.33)
- **Multi-Biome Resources:** Resources usable across multiple biomes (v0.32)

---

## XII. Success Criteria

### Phase 2 Completion Checklist

- [ ]  All 3 new tables created successfully
- [ ]  Muspelheim biome entry inserted (biome_id: 4)
- [ ]  All 8 room templates inserted with valid WFC rules
- [ ]  All 9 resources inserted with correct tier/rarity/drop rates
- [ ]  Foreign key relationships validated
- [ ]  Data integrity tests pass (all 12 tests)
- [ ]  ASCII-only entity names confirmed
- [ ]  v5.0 setting compliance verified
- [ ]  SQL script executes without errors
- [ ]  Database size increase reasonable (<500 KB for v0.29.1 data)

### Ready for v0.29.2

✅ **v0.29.1 provides foundation for:**

- Environmental hazard seeding (uses `Biome_EnvironmentalFeatures` table)
- [Intense Heat] condition definition (references `biome_id: 4`)
- Hazard placement logic (consumes `hazard_density` from room templates)

---

## XIII. Related Documents

**Parent Specification:**

- v0.29: Muspelheim Biome Implementation[[1]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)

**Next Specifications:**

- v0.29.2: Environmental Hazards & Ambient Conditions (depends on v0.29.1)
- v0.29.3: Enemy Definitions & Spawn System (depends on v0.29.1)
- v0.29.4: Service Implementation & Testing (depends on v0.29.1-3)

**Canonical Sources:**

- v2.0: Feature Specification: The Muspelheim Biome[[2]](https://www.notion.so/Feature-Specification-The-Muspelheim-Biome-2a355eb312da80cdab65de771b57e414?pvs=21)
- v5.0: Aethelgard Setting Fundamentals[[3]](https://www.notion.so/META-Aethelgard-Setting-Fundamentals-Canonical-Ground-Rules-d9b4c6bed0b0434dae14e8a2767235c3?pvs=21)

**Prerequisites:**

- v0.10-v0.12: Dynamic Room Engine[[4]](https://www.notion.so/v0-10-Dynamic-Room-Engine-Core-f6b2626559d844d78fc65f1fe2c30798?pvs=21)

**Project Context:**

- Master Roadmap[[5]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)
- AI Session Handoff[[6]](https://www.notion.so/AI-Session-Handoff-Rune-Rust-Development-Status-e19fe6060d6d4e44ae7402d88e3cc6a3?pvs=21)
- MANDATORY Requirements[[7]](https://www.notion.so/MANDATORY-Specification-Requirements-Quality-Standards-a40022443c8b4abfb8cbd4882839447d?pvs=21)

---

**Phase 1 (Database Design): COMPLETE ✓**

**Phase 2 (Database Implementation): Ready to begin**