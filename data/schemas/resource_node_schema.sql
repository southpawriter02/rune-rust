-- ============================================================
-- v0.38.5: Loot & Resource Templates - Schema
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Procedural resource node generation with biome distribution
-- ============================================================

-- Resource Nodes Table
-- Stores resource node instances in rooms
CREATE TABLE IF NOT EXISTS Resource_Nodes (
    node_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,

    -- Descriptor reference
    composite_descriptor_id INTEGER,

    -- Node identity
    node_name TEXT NOT NULL,
    description TEXT NOT NULL,
    node_type TEXT NOT NULL,  -- 'MineralVein', 'SalvageWreckage', 'OrganicHarvest', 'AethericAnomaly'

    -- Extraction mechanics
    extraction_type TEXT NOT NULL,  -- 'Mining', 'Salvaging', 'Harvesting', 'Siphoning'
    extraction_dc INTEGER,
    extraction_time INTEGER,  -- Turns
    requires_tool BOOLEAN DEFAULT 0,
    required_tool TEXT,  -- 'Mining_Tool', 'Salvage_Kit', 'Aether_Siphon', etc.

    -- Yield
    yield_min INTEGER,
    yield_max INTEGER,
    resource_type TEXT,  -- 'Iron_Ore', 'Star_Metal', 'Luminous_Fungus', etc.
    rarity_tier TEXT,  -- 'Common', 'Uncommon', 'Rare', 'Legendary'

    -- State
    depleted BOOLEAN DEFAULT 0,
    uses_remaining INTEGER,
    max_uses INTEGER,

    -- Hazards
    hazardous BOOLEAN DEFAULT 0,
    hazard_description TEXT,
    trap_chance REAL DEFAULT 0,

    -- Special properties
    hidden BOOLEAN DEFAULT 0,
    detection_dc INTEGER,
    unstable BOOLEAN DEFAULT 0,
    requires_galdr BOOLEAN DEFAULT 0,

    -- Biome restriction
    biome_restriction TEXT,

    -- Tags
    tags TEXT,  -- JSON array

    FOREIGN KEY (composite_descriptor_id) REFERENCES Descriptor_Composites(composite_id),
    CHECK (node_type IN ('MineralVein', 'SalvageWreckage', 'OrganicHarvest', 'AethericAnomaly')),
    CHECK (extraction_type IN ('Mining', 'Salvaging', 'Harvesting', 'Siphoning', 'Search')),
    CHECK (rarity_tier IN ('Common', 'Uncommon', 'Rare', 'Legendary'))
);

-- Index for room queries
CREATE INDEX IF NOT EXISTS idx_resource_nodes_room
ON Resource_Nodes(room_id);

-- Index for biome queries
CREATE INDEX IF NOT EXISTS idx_resource_nodes_biome
ON Resource_Nodes(biome_restriction);

-- Biome Resource Profiles Table
-- Defines resource distribution for each biome
CREATE TABLE IF NOT EXISTS Biome_Resource_Profiles (
    profile_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_name TEXT NOT NULL UNIQUE,

    -- Distribution tables (JSON arrays)
    common_resources TEXT NOT NULL,  -- [{"template": "Ore_Vein_Base", "resource": "Iron", "weight": 0.4}]
    uncommon_resources TEXT NOT NULL,
    rare_resources TEXT NOT NULL,
    legendary_resources TEXT,

    -- Spawn density rules
    spawn_density_small INTEGER DEFAULT 0,  -- Small rooms
    spawn_density_medium INTEGER DEFAULT 2, -- Medium rooms
    spawn_density_large INTEGER DEFAULT 3,  -- Large rooms

    -- Special rules
    unique_resources TEXT,  -- JSON array of biome-unique resources
    notes TEXT
);

-- ============================================================
-- Notes:
-- - Resource_Nodes: Instances placed in rooms during generation
-- - Biome_Resource_Profiles: Defines resource availability per biome
-- - Extraction mechanics determine time cost and skill requirements
-- - Rarity tiers affect spawn probability: 70% common, 25% uncommon, 5% rare
-- - Hidden nodes require detection checks before extraction
-- - Hazardous/unstable nodes have extraction risks
-- ============================================================
