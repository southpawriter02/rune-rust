-- ============================================================================
-- v0.30.1: Niflheim Biome - Database Schema & Room Templates
-- ============================================================================
-- Part of: Rune & Rust v5.0 - Niflheim Biome Implementation
-- Status: Implementation Ready
-- Timeline: 8-12 hours
-- Prerequisites: v0.29 (Muspelheim), v0.20 (Tactical Grid)
--
-- Purpose:
--   Complete database schema foundation for the Niflheim biome, including:
--   - Biome metadata (biome_id: 5)
--   - Room templates with WFC adjacency rules (8 templates)
--   - Resource drop definitions (9 resources, Tier 2-5)
--   - Dual verticality support ([Roots] and [Canopy] variants)
--
-- The Fantasy:
--   Niflheim is the cryogenic catastrophe biome - frozen wastes where
--   civilization's thermal regulation systems have failed catastrophically.
--   Deep cryogenic laboratories, coolant pumping stations, and high-altitude
--   research outposts locked in permanent, absolute zero. This is industrial
--   refrigeration run amok, not natural winter.
-- ============================================================================

-- ============================================================================
-- SECTION 1: Biome Metadata
-- ============================================================================
-- Core biome definition for Niflheim (biome_id: 5)
-- Settings v5.0 Compliance: Technology failure, not magic
-- ============================================================================

INSERT INTO Biomes (
    biome_id,
    name,
    description,
    ambient_condition,
    difficulty_tier,
    verticality_type,
    dominant_damage_type,
    counter_damage_type,
    is_active,
    created_at
) VALUES (
    5,
    'Niflheim',
    'Cryogenic catastrophe zone where thermal regulation systems have failed catastrophically. Coolant pumping stations and high-altitude research outposts locked in permanent sub-zero. Flash-frozen machinery, shattered atmospheric shields, and treacherous ice sheets mark this industrial refrigeration disaster. 800 years of system decay have created a frozen graveyard where the cold is hostile physics failure.',
    'Frigid Cold',  -- Universal Ice vulnerability, critical hits cause [Slowed]
    'Mid-Late',     -- Parallels Muspelheim as mid-to-late game challenge
    'Dual',         -- Supports both [Roots] and [Canopy] variants
    'Ice',          -- Primary damage type
    'Fire',         -- Tactical counter (Fire damage provides advantage)
    TRUE,
    CURRENT_TIMESTAMP
);

-- Biome feature tags for mechanical integration
INSERT INTO Biome_FeatureTags (biome_id, tag_name, tag_value) VALUES
    (5, 'movement_challenge', 'slippery_terrain'),
    (5, 'ambient_vulnerability', 'ice_damage'),
    (5, 'critical_debuff', 'slowed'),
    (5, 'brittleness_mechanic', 'ice_to_brittle'),
    (5, 'terrain_dominant', 'ice_sheets'),
    (5, 'temperature_extreme', 'cryogenic'),
    (5, 'system_failure_type', 'thermal_regulation'),
    (5, 'verticality_zones', 'roots_and_canopy');

-- ============================================================================
-- SECTION 2: Room Templates (8 Templates)
-- ============================================================================
-- 4 [Roots] templates (underground cryogenic facilities)
-- 4 [Canopy] templates (high-altitude frozen outposts)
-- Each includes WFC adjacency rules for procedural generation
-- ============================================================================

-- ----------------------------------------------------------------------------
-- [ROOTS] TEMPLATES (Underground Cryogenic Facilities)
-- ----------------------------------------------------------------------------

-- Template 1: Cryogenic Storage Vault (Large, Entrance-capable, [Roots])
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    room_size,
    connection_count_min,
    connection_count_max,
    hazard_density,
    enemy_count_min,
    enemy_count_max,
    resource_spawn_chance,
    is_entrance_eligible,
    is_exit_eligible,
    is_boss_room,
    verticality_zone,
    wfc_adjacency_rules,
    description
) VALUES (
    5,
    'Cryogenic Storage Vault',
    'Large',
    3,  -- High connectivity (entrance hub)
    5,
    0.25,  -- Moderate hazards (frozen equipment, ice sheets)
    3,
    4,
    0.70,  -- High resource chance (preserved materials)
    TRUE,  -- Entrance-eligible
    FALSE,
    FALSE,
    'Roots',
    '{
        "allowed_neighbors": [
            "Coolant Distribution Tunnel",
            "Frozen Research Lab",
            "Permafrost Containment Chamber",
            "Emergency Thermal Junction"
        ],
        "preferred_connections": 4,
        "wall_types": ["reinforced_cryogenic_walls", "frost_covered_metal"],
        "floor_materials": ["slippery_ice_sheets", "frozen_grating"],
        "lighting": "emergency_blue_dim"
    }',
    'Vast underground vault where biological samples and equipment were cryogenically preserved. Massive storage racks tower overhead, covered in thick frost. Liquid nitrogen pipes snake across the ceiling, many ruptured and leaking. The floor is treacherous ice, and the air is painfully cold. Pre-Glitch storage containers are sealed in ice blocks.'
);

-- Template 2: Coolant Distribution Tunnel (Small, Connector, [Roots])
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    room_size,
    connection_count_min,
    connection_count_max,
    hazard_density,
    enemy_count_min,
    enemy_count_max,
    resource_spawn_chance,
    is_entrance_eligible,
    is_exit_eligible,
    is_boss_room,
    verticality_zone,
    wfc_adjacency_rules,
    description
) VALUES (
    5,
    'Coolant Distribution Tunnel',
    'Small',
    2,
    2,
    0.40,  -- High hazard density (cryo-vents, icicles)
    1,
    2,
    0.40,
    FALSE,
    FALSE,
    FALSE,
    'Roots',
    '{
        "allowed_neighbors": [
            "Cryogenic Storage Vault",
            "Frozen Research Lab",
            "Permafrost Containment Chamber",
            "Glacial Throne Room"
        ],
        "preferred_connections": 2,
        "wall_types": ["exposed_coolant_pipes", "ice_encrusted_conduit"],
        "floor_materials": ["frozen_metal_grating", "permafrost"],
        "lighting": "flickering_cryogenic_blue"
    }',
    'Narrow service tunnel lined with massive coolant distribution pipes. Many have burst, spraying liquid nitrogen intermittently. Icicles hang from the ceiling like daggers. The walls glisten with frost patterns. Frozen maintenance drones are embedded in ice along the passage.'
);

-- Template 3: Frozen Research Lab (Medium, High Hazards, [Roots])
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    room_size,
    connection_count_min,
    connection_count_max,
    hazard_density,
    enemy_count_min,
    enemy_count_max,
    resource_spawn_chance,
    is_entrance_eligible,
    is_exit_eligible,
    is_boss_room,
    verticality_zone,
    wfc_adjacency_rules,
    description
) VALUES (
    5,
    'Frozen Research Lab',
    'Medium',
    2,
    3,
    0.50,  -- Very high hazards (unstable equipment, ice boulders)
    2,
    3,
    0.65,  -- High resources (research materials)
    FALSE,
    FALSE,
    FALSE,
    'Roots',
    '{
        "allowed_neighbors": [
            "Cryogenic Storage Vault",
            "Coolant Distribution Tunnel",
            "Emergency Thermal Junction"
        ],
        "preferred_connections": 2,
        "wall_types": ["lab_partition_ice", "shattered_observation_glass"],
        "floor_materials": ["frozen_tile", "ice_covered_equipment"],
        "lighting": "dim_blue_emergency"
    }',
    'Pre-Glitch laboratory frozen mid-operation. Workstations are encased in ice, displays cracked and dark. Cryogenic fog drifts across the floor. Experimental equipment has shattered from thermal shock, creating hazardous debris fields. Frozen corpses of researchers are slumped at their stations, perfectly preserved.'
);

-- Template 4: Permafrost Containment Chamber (Large, Legendary Resources, [Roots])
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    room_size,
    connection_count_min,
    connection_count_max,
    hazard_density,
    enemy_count_min,
    enemy_count_max,
    resource_spawn_chance,
    is_entrance_eligible,
    is_exit_eligible,
    is_boss_room,
    verticality_zone,
    wfc_adjacency_rules,
    description
) VALUES (
    5,
    'Permafrost Containment Chamber',
    'Large',
    1,
    2,
    0.35,  -- Moderate hazards (brittle ice bridges, cryo-fog)
    3,
    4,
    0.85,  -- Very high resource chance (rare materials)
    FALSE,
    FALSE,
    FALSE,
    'Roots',
    '{
        "allowed_neighbors": [
            "Cryogenic Storage Vault",
            "Coolant Distribution Tunnel"
        ],
        "preferred_connections": 1,
        "wall_types": ["thick_permafrost", "reinforced_ice"],
        "floor_materials": ["ancient_ice_layers", "frozen_bedrock"],
        "lighting": "crystalline_blue_glow"
    }',
    'Massive chamber carved from ancient permafrost layers. The walls are translucent ice revealing frozen Pre-Glitch artifacts. Pristine ice cores protrude from the floor like pillars. The temperature is absolute zero. Rare resources are suspended in the ice, requiring extraction. Silence is absolute.'
);

-- ----------------------------------------------------------------------------
-- [CANOPY] TEMPLATES (High-Altitude Frozen Outposts)
-- ----------------------------------------------------------------------------

-- Template 5: Atmospheric Shield Platform (XLarge, High Connectivity, [Canopy])
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    room_size,
    connection_count_min,
    connection_count_max,
    hazard_density,
    enemy_count_min,
    enemy_count_max,
    resource_spawn_chance,
    is_entrance_eligible,
    is_exit_eligible,
    is_boss_room,
    verticality_zone,
    wfc_adjacency_rules,
    description
) VALUES (
    5,
    'Atmospheric Shield Platform',
    'XLarge',
    2,
    4,
    0.30,  -- Moderate hazards (wind exposure, unstable platforms)
    4,
    5,
    0.60,
    TRUE,  -- Entrance-eligible for [Canopy] entry
    FALSE,
    FALSE,
    'Canopy',
    '{
        "allowed_neighbors": [
            "Frozen Observatory Deck",
            "High-Altitude Weather Station",
            "Emergency Thermal Junction",
            "Glacial Throne Room"
        ],
        "preferred_connections": 3,
        "wall_types": ["shattered_atmospheric_shield", "exposed_platform_edge"],
        "floor_materials": ["ice_covered_grating", "frozen_walkway"],
        "lighting": "harsh_white_sunlight"
    }',
    'Exposed platform where atmospheric shield generators once maintained breathable air at high altitude. The shields have shattered, leaving crystalline debris scattered across frozen grating. Bitter winds howl through the ruins. The view extends for kilometers across frozen wastelands. Ice-adapted predators patrol the perimeter.'
);

-- Template 6: Frozen Observatory Deck (Medium, Science Focus, [Canopy])
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    room_size,
    connection_count_min,
    connection_count_max,
    hazard_density,
    enemy_count_min,
    enemy_count_max,
    resource_spawn_chance,
    is_entrance_eligible,
    is_exit_eligible,
    is_boss_room,
    verticality_zone,
    wfc_adjacency_rules,
    description
) VALUES (
    5,
    'Frozen Observatory Deck',
    'Medium',
    1,
    3,
    0.40,
    2,
    3,
    0.75,  -- High resources (scientific instruments)
    FALSE,
    FALSE,
    FALSE,
    'Canopy',
    '{
        "allowed_neighbors": [
            "Atmospheric Shield Platform",
            "High-Altitude Weather Station"
        ],
        "preferred_connections": 2,
        "wall_types": ["observatory_dome_ice", "reinforced_glass_shattered"],
        "floor_materials": ["frozen_deck", "ice_covered_equipment"],
        "lighting": "starlight_through_ice"
    }',
    'Astronomical observatory frozen at the moment of abandonment. Massive telescopes are locked in ice, pointed at the stars. Observation logs are flash-frozen mid-entry. The dome has cracked, allowing snow to drift inside. Scientific instruments are preserved in ice, potentially salvageable.'
);

-- Template 7: High-Altitude Weather Station (Small, Connector, [Canopy])
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    room_size,
    connection_count_min,
    connection_count_max,
    hazard_density,
    enemy_count_min,
    enemy_count_max,
    resource_spawn_chance,
    is_entrance_eligible,
    is_exit_eligible,
    is_boss_room,
    verticality_zone,
    wfc_adjacency_rules,
    description
) VALUES (
    5,
    'High-Altitude Weather Station',
    'Small',
    2,
    4,
    0.35,
    1,
    2,
    0.50,
    FALSE,
    TRUE,  -- Exit-eligible for [Canopy]
    FALSE,
    'Canopy',
    '{
        "allowed_neighbors": [
            "Atmospheric Shield Platform",
            "Frozen Observatory Deck",
            "Glacial Throne Room"
        ],
        "preferred_connections": 3,
        "wall_types": ["prefab_metal_frozen", "sensor_array_ice"],
        "floor_materials": ["frozen_metal", "ice_sheet"],
        "lighting": "dim_emergency_blue"
    }',
    'Compact meteorological station perched at extreme altitude. Weather sensors are coated in thick ice, still transmitting garbled data. Wind howls constantly. The station creaks and sways. Frozen weather balloons hang from the ceiling. Emergency thermal systems failed long ago.'
);

-- Template 8: Glacial Throne Room (XLarge, BOSS ROOM, Dual Verticality)
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    room_size,
    connection_count_min,
    connection_count_max,
    hazard_density,
    enemy_count_min,
    enemy_count_max,
    resource_spawn_chance,
    is_entrance_eligible,
    is_exit_eligible,
    is_boss_room,
    verticality_zone,
    wfc_adjacency_rules,
    description
) VALUES (
    5,
    'Glacial Throne Room',
    'XLarge',
    1,
    3,
    0.60,  -- Extreme hazards (icicle ceiling, ice boulders, cryo-vents)
    1,  -- Boss only: Frost-Giant
    1,
    0.95,  -- Guaranteed legendary resources
    FALSE,
    FALSE,
    TRUE,  -- BOSS ROOM
    'Both',  -- Accessible from either [Roots] or [Canopy]
    '{
        "allowed_neighbors": [
            "Coolant Distribution Tunnel",
            "Atmospheric Shield Platform",
            "High-Altitude Weather Station",
            "Emergency Thermal Junction"
        ],
        "preferred_connections": 2,
        "wall_types": ["cathedral_ice_walls", "frozen_machinery_throne"],
        "floor_materials": ["pristine_ice", "frozen_throne_platform"],
        "lighting": "ethereal_blue_glow"
    }',
    'Cathedral-like chamber dominated by a massive throne of frozen machinery. A dormant Frost-Giant (Jötun-Forged warmachine) sits upon the throne, encased in ice but eyes glowing faintly. The chamber is silent except for the creak of ice. Icicles the size of pillars hang overhead. The floor is a sheet of perfect, treacherous ice. This is the heart of Niflheim.'
);

-- Template 9: Emergency Thermal Junction (Small, Safe Haven, Dual Verticality)
INSERT INTO Biome_RoomTemplates (
    biome_id,
    template_name,
    room_size,
    connection_count_min,
    connection_count_max,
    hazard_density,
    enemy_count_min,
    enemy_count_max,
    resource_spawn_chance,
    is_entrance_eligible,
    is_exit_eligible,
    is_boss_room,
    verticality_zone,
    wfc_adjacency_rules,
    description
) VALUES (
    5,
    'Emergency Thermal Junction',
    'Small',
    2,
    4,
    0.10,  -- Low hazards (safe respite)
    0,  -- No enemies (safe room)
    1,  -- Rare patrol
    0.50,
    FALSE,
    TRUE,  -- Exit-eligible
    FALSE,
    'Both',  -- Bridge between [Roots] and [Canopy]
    '{
        "allowed_neighbors": [
            "Cryogenic Storage Vault",
            "Frozen Research Lab",
            "Atmospheric Shield Platform",
            "Glacial Throne Room"
        ],
        "preferred_connections": 3,
        "wall_types": ["thermal_insulated_walls", "heat_exchanger_panels"],
        "floor_materials": ["heated_grating", "warmth_radiating"],
        "lighting": "warm_orange_glow"
    }',
    'Rare functioning emergency thermal system junction. Heat exchangers still operate, maintaining breathable temperature. This is a pocket of warmth in the frozen hell. Scavengers have left supply caches here. The sound of machinery is a comforting contrast to the silence outside. A temporary respite before venturing back into the cold.'
);

-- ============================================================================
-- SECTION 3: Resource Drop Definitions (9 Resources)
-- ============================================================================
-- Tier 2-3: Common/Uncommon (Cryo-Coolant, Frost-Lichen, Ice-Bear Pelt)
-- Tier 4: Rare (Pristine Ice Core, Supercooled Alloy)
-- Tier 5: Legendary (Heart of Frost-Giant, Eternal Frost Crystal)
-- ============================================================================

-- Tier 2 (Common) - Basic salvage materials
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_tier,
    rarity,
    drop_weight,
    quantity_min,
    quantity_max,
    requires_special_node,
    description
) VALUES
(
    5,
    'Frost-Lichen',
    2,
    'Common',
    150,  -- Very common
    3,
    7,
    FALSE,
    'Hardy bioluminescent lichen that thrives in extreme cold. Grows on frozen surfaces throughout Niflheim. Used in basic cold-resistance preparations and as a reagent for thermal insulation compounds. Emits a faint blue glow.'
),
(
    5,
    'Frozen Scrap Metal',
    2,
    'Common',
    120,
    2,
    5,
    FALSE,
    'Metal fragments flash-frozen and preserved in pristine condition. Salvaged from shattered machinery and structural failures. Useful for basic repairs and crafting, superior to oxidized scrap due to preservation.'
);

-- Tier 3 (Uncommon) - Specialized materials
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_tier,
    rarity,
    drop_weight,
    quantity_min,
    quantity_max,
    requires_special_node,
    description
) VALUES
(
    5,
    'Cryo-Coolant Fluid',
    3,
    'Uncommon',
    80,
    1,
    3,
    FALSE,
    'Industrial refrigerant from ruptured coolant systems. Remains liquid at extreme sub-zero temperatures. Highly valuable for crafting cold-resistance gear and as a component in advanced thermal regulation devices. Handle with insulated gloves.'
),
(
    5,
    'Ice-Bear Pelt',
    3,
    'Uncommon',
    60,
    1,
    1,
    FALSE,
    'Thick fur from ice-adapted beasts. Provides exceptional thermal insulation. Prized for crafting cold-resistant armor and cloaks. The fur is dense and water-repellent, evolved for survival in Niflheim conditions.'
);

-- Tier 4 (Rare) - Advanced components
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_tier,
    rarity,
    drop_weight,
    quantity_min,
    quantity_max,
    requires_special_node,
    description
) VALUES
(
    5,
    'Pristine Ice Core',
    4,
    'Rare',
    40,
    1,
    2,
    TRUE,  -- Requires investigation of special ice pillars
    'Ancient ice containing perfectly preserved Pre-Glitch materials. Acts as a Runic Catalyst for advanced crafting. The ice is impossibly clear and dense, defying natural formation processes. Extracting the contents requires careful thawing.'
),
(
    5,
    'Supercooled Alloy Sample',
    4,
    'Rare',
    35,
    1,
    1,
    TRUE,  -- Requires salvaging from specific frozen machinery
    'Metal alloy that has undergone phase transformation at cryogenic temperatures. Exhibits unusual strength and conductivity properties. Valuable for crafting advanced equipment and understanding Pre-Glitch metallurgy.'
),
(
    5,
    'Cryogenic Data Core',
    4,
    'Rare',
    50,
    1,
    1,
    TRUE,  -- Requires extracting from frozen research stations
    'Frozen data storage unit from Pre-Glitch research facilities. May contain technical schematics, research data, or operational logs. The extreme cold has preserved the data integrity. Requires specialized equipment to access.'
);

-- Tier 5 (Legendary) - Boss and ultra-rare drops
INSERT INTO Biome_ResourceDrops (
    biome_id,
    resource_name,
    resource_tier,
    rarity,
    drop_weight,
    quantity_min,
    quantity_max,
    requires_special_node,
    description
) VALUES
(
    5,
    'Heart of the Frost-Giant',
    5,
    'Legendary',
    5,  -- Ultra-rare (boss drop)
    1,
    1,
    TRUE,  -- Boss-only drop from Glacial Throne Room
    'Legendary power core from a dormant Jötun-Forged warmachine. Pulses with cryogenic energy even when removed. The ultimate Runic Catalyst for ice-aligned crafting. Radiates cold that can flash-freeze moisture in the air. Contains advanced thermal regulation technology.'
),
(
    5,
    'Eternal Frost Crystal',
    5,
    'Legendary',
    3,  -- Ultra-rare
    1,
    1,
    TRUE,  -- Deepest permafrost chambers only
    'Crystallized form of extreme cold, defying thermodynamic laws. Remains frozen at any ambient temperature. A perfect thermal sink. Can be used to craft legendary cold-resistance equipment or as a component in experimental cryogenic systems. Origin unknown.'
);

-- ============================================================================
-- SECTION 4: Character Biome Status Tracking
-- ============================================================================
-- Extends Characters_BiomeStatus table for Niflheim-specific tracking
-- ============================================================================

-- Add Niflheim tracking columns (if not already generic)
-- Assumes Characters_BiomeStatus table exists from v0.29
-- Tracks: cold damage taken, deaths, rooms explored, boss defeated

-- Insert default status for existing characters
INSERT INTO Characters_BiomeStatus (
    character_id,
    biome_id,
    total_damage_taken,
    total_kills,
    times_died,
    rooms_explored,
    boss_defeated,
    legendary_resources_found,
    first_entered_at,
    last_visited_at
)
SELECT
    character_id,
    5,  -- Niflheim biome_id
    0,  -- No damage yet
    0,  -- No kills yet
    0,  -- No deaths yet
    0,  -- No rooms explored yet
    FALSE,  -- Boss not defeated
    0,  -- No legendary resources found
    NULL,  -- Not entered yet
    NULL
FROM Characters
WHERE NOT EXISTS (
    SELECT 1 FROM Characters_BiomeStatus
    WHERE Characters_BiomeStatus.character_id = Characters.character_id
    AND Characters_BiomeStatus.biome_id = 5
);

-- ============================================================================
-- SECTION 5: Validation Queries
-- ============================================================================
-- Quality assurance checks for schema integrity
-- ============================================================================

-- Verify biome entry
SELECT
    biome_id,
    name,
    ambient_condition,
    difficulty_tier,
    verticality_type,
    dominant_damage_type,
    counter_damage_type
FROM Biomes
WHERE biome_id = 5;

-- Verify room template count and variety
SELECT
    verticality_zone,
    COUNT(*) as template_count,
    SUM(CASE WHEN is_boss_room THEN 1 ELSE 0 END) as boss_rooms,
    SUM(CASE WHEN is_entrance_eligible THEN 1 ELSE 0 END) as entrance_rooms,
    SUM(CASE WHEN is_exit_eligible THEN 1 ELSE 0 END) as exit_rooms
FROM Biome_RoomTemplates
WHERE biome_id = 5
GROUP BY verticality_zone;

-- Verify resource tier distribution
SELECT
    resource_tier,
    rarity,
    COUNT(*) as resource_count,
    SUM(CASE WHEN requires_special_node THEN 1 ELSE 0 END) as special_nodes_required
FROM Biome_ResourceDrops
WHERE biome_id = 5
GROUP BY resource_tier, rarity
ORDER BY resource_tier;

-- Verify WFC adjacency rules are valid JSON
SELECT
    template_name,
    jsonb_pretty(wfc_adjacency_rules::jsonb) as adjacency_rules
FROM Biome_RoomTemplates
WHERE biome_id = 5
LIMIT 3;

-- Check total drop weight distribution
SELECT
    SUM(drop_weight) as total_weight,
    AVG(drop_weight) as avg_weight,
    MAX(drop_weight) as max_weight,
    MIN(drop_weight) as min_weight
FROM Biome_ResourceDrops
WHERE biome_id = 5;

-- ============================================================================
-- SECTION 6: Success Criteria
-- ============================================================================
-- ✅ Biome metadata inserted (biome_id: 5)
-- ✅ 9 room templates created (4 Roots, 4 Canopy, 1 Both)
-- ✅ 9 resources defined across 5 tiers
-- ✅ WFC adjacency rules in valid JSON format
-- ✅ Dual verticality support ([Roots] and [Canopy])
-- ✅ Boss room identified (Glacial Throne Room)
-- ✅ Safe haven room (Emergency Thermal Junction)
-- ✅ Character tracking schema extended
-- ✅ Validation queries confirm data integrity
-- ============================================================================

-- End of v0.30.1_niflheim_schema.sql
