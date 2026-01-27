-- =====================================================
-- v0.38.1: Room Description Library Schema
-- =====================================================
-- Version: v0.38.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-17
-- Prerequisites: v0.38.0 (Descriptor Framework)
-- =====================================================
-- Document ID: RR-SPEC-v0.38.1-DATABASE
-- Parent Specification: v0.38.1 Room Description Library
-- Status: Implementation
-- Timeline: 10-12 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- DESCRIPTOR FRAGMENTS TABLE
-- =====================================================
-- Stores reusable text fragments for room descriptions
-- Categories: SpatialDescriptor, ArchitecturalFeature, Detail, Atmospheric

CREATE TABLE IF NOT EXISTS Descriptor_Fragments (
    fragment_id INTEGER PRIMARY KEY AUTOINCREMENT,
    category TEXT NOT NULL CHECK(category IN ('SpatialDescriptor', 'ArchitecturalFeature', 'Detail', 'Atmospheric', 'Direction')),
    subcategory TEXT,  -- "Wall", "Ceiling", "Floor", "Decay", "Runes", etc.
    fragment_text TEXT NOT NULL,
    tags TEXT,  -- JSON array for filtering: ["Large", "Industrial", "Muspelheim"]
    weight REAL DEFAULT 1.0,  -- For weighted random selection
    is_active INTEGER DEFAULT 1,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_fragment_category ON Descriptor_Fragments(category);
CREATE INDEX IF NOT EXISTS idx_fragment_subcategory ON Descriptor_Fragments(subcategory);
CREATE INDEX IF NOT EXISTS idx_fragment_active ON Descriptor_Fragments(is_active);

-- =====================================================
-- ROOM FUNCTION VARIANTS TABLE
-- =====================================================
-- Stores functional descriptors for chambers (e.g., "Pumping Station", "Forge Hall")

CREATE TABLE IF NOT EXISTS Room_Function_Variants (
    function_id INTEGER PRIMARY KEY AUTOINCREMENT,
    function_name TEXT NOT NULL UNIQUE,
    function_detail TEXT NOT NULL,  -- Description of the function's purpose
    biome_affinity TEXT,  -- JSON array: ["Muspelheim", "The_Roots"]
    archetype TEXT CHECK(archetype IN ('Chamber', 'PowerStation', 'Laboratory', 'Storage')),
    tags TEXT,  -- JSON array
    created_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_function_archetype ON Room_Function_Variants(archetype);

-- =====================================================
-- BASE ROOM TEMPLATES (15+ templates)
-- =====================================================

-- Template 1: Entry Hall Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Entry_Hall_Base', 'Room', 'EntryHall',
    '{"size": "Medium", "min_exits": 1, "max_exits": 2, "spawn_budget_multiplier": 0.5, "ambient_danger_level": "Low"}',
    'The {Modifier} Entry Hall',
    'You enter {Article} {Modifier_Adj} entry hall. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Detail_2}. The air {Atmospheric_Detail}.',
    '["Starting", "Safe", "Orientation"]',
    'Safe starting room with low danger'
);

-- Template 2: Corridor Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Corridor_Base', 'Room', 'Corridor',
    '{"size": "Small", "min_exits": 2, "max_exits": 2, "spawn_budget_multiplier": 0.8, "ambient_danger_level": "Medium"}',
    'The {Modifier} Corridor',
    '{Article_Cap} {Modifier_Adj} corridor stretches {Direction_Descriptor}. {Architectural_Feature}. {Spatial_Descriptor}. {Detail_1}. {Modifier_Detail}.',
    '["Transit", "Linear", "Narrow"]',
    'Linear transit passage'
);

-- Template 3: Chamber Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Chamber_Base', 'Room', 'Chamber',
    '{"size": "Large", "min_exits": 1, "max_exits": 4, "spawn_budget_multiplier": 1.2, "ambient_danger_level": "High"}',
    'The {Modifier} {Function} Chamber',
    '{Article_Cap} {Modifier_Adj} {Function} dominates this space. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Detail_2}. {Function_Detail}. {Modifier_Detail}.',
    '["Large", "Combat", "Exploration"]',
    'Large room for combat and exploration'
);

-- Template 4: Junction Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Junction_Base', 'Room', 'Junction',
    '{"size": "Medium", "min_exits": 3, "max_exits": 4, "spawn_budget_multiplier": 1.0, "ambient_danger_level": "Medium"}',
    'The {Modifier} Junction',
    'Multiple passages converge at this {Modifier_Adj} junction. {Spatial_Descriptor}. {Architectural_Feature}. {Exit_Description}. {Detail_1}.',
    '["Branching", "Decision", "Navigation"]',
    'Branching point with 3+ exits'
);

-- Template 5: Boss Arena Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Boss_Arena_Base', 'Room', 'BossArena',
    '{"size": "XLarge", "min_exits": 1, "max_exits": 1, "spawn_budget_multiplier": 0.0, "ambient_danger_level": "Extreme", "boss_spawn_required": true}',
    'The {Modifier} Arena',
    '{Article_Cap} vast {Modifier_Adj} chamber opens before you. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Detail_2}. {Ominous_Detail}. This is clearly a place of significance.',
    '["Climactic", "Arena", "Boss"]',
    'Boss encounter arena'
);

-- Template 6: Secret Room Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Secret_Room_Base', 'Room', 'SecretRoom',
    '{"size": "Small", "min_exits": 1, "max_exits": 1, "spawn_budget_multiplier": 0.0, "ambient_danger_level": "Low", "loot_bonus_multiplier": 2.0}',
    'The Hidden {Modifier} Cache',
    'You discover {Article} hidden {Modifier_Adj} cache. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Loot_Hint}.',
    '["Hidden", "Rewards", "Secret"]',
    'Hidden room with bonus loot'
);

-- Template 7: Vertical Shaft Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Vertical_Shaft_Base', 'Room', 'VerticalShaft',
    '{"size": "Medium", "min_exits": 2, "max_exits": 2, "spawn_budget_multiplier": 0.6, "ambient_danger_level": "High", "vertical_transit": true}',
    'The {Modifier} Shaft',
    '{Article_Cap} {Modifier_Adj} shaft extends vertically {Direction_Descriptor}. {Architectural_Feature}. {Spatial_Descriptor}. {Detail_1}. {Traversal_Warning}.',
    '["Vertical", "Transit", "Canopy", "Roots"]',
    'Vertical transit between levels'
);

-- Template 8: Maintenance Hub Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Maintenance_Hub_Base', 'Room', 'MaintenanceHub',
    '{"size": "Medium", "min_exits": 2, "max_exits": 4, "spawn_budget_multiplier": 1.0, "ambient_danger_level": "Medium"}',
    'The {Modifier} Maintenance Hub',
    '{Article_Cap} {Modifier_Adj} maintenance hub serves as a junction for utility systems. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Industrial_Detail}.',
    '["Industrial", "Functional", "Utility"]',
    'Maintenance and utility hub'
);

-- Template 9: Storage Bay Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Storage_Bay_Base', 'Room', 'StorageBay',
    '{"size": "Large", "min_exits": 1, "max_exits": 2, "spawn_budget_multiplier": 0.8, "ambient_danger_level": "Low", "loot_spawn_bonus": 1.5}',
    'The {Modifier} Storage Bay',
    '{Article_Cap} {Modifier_Adj} storage bay is filled with {Storage_Contents}. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Salvage_Potential}.',
    '["Storage", "Salvage", "Resources"]',
    'Storage area with salvage potential'
);

-- Template 10: Observation Platform Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Observation_Platform_Base', 'Room', 'ObservationPlatform',
    '{"size": "Medium", "min_exits": 1, "max_exits": 2, "spawn_budget_multiplier": 0.7, "ambient_danger_level": "Low", "tactical_advantage": true}',
    'The {Modifier} Observation Platform',
    '{Article_Cap} {Modifier_Adj} observation platform provides {Vantage_Description}. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Visibility_Detail}.',
    '["Elevated", "Vantage", "Tactical"]',
    'Elevated vantage point'
);

-- Template 11: Power Station Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Power_Station_Base', 'Room', 'PowerStation',
    '{"size": "Large", "min_exits": 1, "max_exits": 3, "spawn_budget_multiplier": 1.1, "ambient_danger_level": "High", "electrical_hazard": true}',
    'The {Modifier} Power Station',
    '{Article_Cap} {Modifier_Adj} power station hums with {Energy_State}. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Electrical_Warning}. {Modifier_Detail}.',
    '["Industrial", "Energy", "Hazardous"]',
    'Power generation facility'
);

-- Template 12: Laboratory Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Laboratory_Base', 'Room', 'Laboratory',
    '{"size": "Medium", "min_exits": 1, "max_exits": 2, "spawn_budget_multiplier": 0.9, "ambient_danger_level": "Medium", "data_terminal_chance": 0.8}',
    'The {Modifier} Laboratory',
    '{Article_Cap} {Modifier_Adj} laboratory contains {Research_Equipment}. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Research_Focus}. {Modifier_Detail}.',
    '["Research", "Scientific", "Alfheim"]',
    'Research and experimentation facility'
);

-- Template 13: Barracks Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Barracks_Base', 'Room', 'Barracks',
    '{"size": "Medium", "min_exits": 1, "max_exits": 2, "spawn_budget_multiplier": 1.0, "ambient_danger_level": "Medium", "enemy_density_bonus": 1.2}',
    'The {Modifier} Barracks',
    '{Article_Cap} {Modifier_Adj} barracks once housed {Occupant_Description}. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Military_Detail}.',
    '["Military", "Security", "Fortified"]',
    'Military living quarters'
);

-- Template 14: Forge Chamber Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Forge_Chamber_Base', 'Room', 'ForgeCharnber',
    '{"size": "Large", "min_exits": 1, "max_exits": 2, "spawn_budget_multiplier": 1.1, "ambient_danger_level": "High", "fire_hazard": true}',
    'The {Modifier} Forge',
    '{Article_Cap} {Modifier_Adj} forge dominates this chamber. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Forge_Equipment}. {Heat_Warning}. {Modifier_Detail}.',
    '["Industrial", "Muspelheim", "Fire", "Crafting"]',
    'Forge and metalworking facility'
);

-- Template 15: Cryo Vault Base
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags, notes
) VALUES (
    'Cryo_Vault_Base', 'Room', 'CryoVault',
    '{"size": "Large", "min_exits": 1, "max_exits": 2, "spawn_budget_multiplier": 1.0, "ambient_danger_level": "Medium", "cold_hazard": true}',
    'The {Modifier} Cryo Vault',
    '{Article_Cap} {Modifier_Adj} cryo vault preserves {Cryo_Contents}. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. {Cryo_Status}. {Cold_Warning}. {Modifier_Detail}.',
    '["Cryogenic", "Niflheim", "Cold", "Preservation"]',
    'Cryogenic storage facility'
);

-- =====================================================
-- UPDATED THEMATIC MODIFIERS (Room-Specific Details)
-- =====================================================

-- Update Rusted modifier with room-specific details
UPDATE Descriptor_Thematic_Modifiers
SET
    detail_fragment = 'shows centuries of oxidation and decay',
    ambient_sounds = '["dripping water echoing", "groan of stressed metal", "hissing steam from fractured pipes"]',
    particle_effects = '["rust particles", "water drips", "steam wisps"]',
    notes = 'The Roots biome - advanced decay and corrosion'
WHERE modifier_name = 'Rusted';

-- Update Scorched modifier with room-specific details
UPDATE Descriptor_Thematic_Modifiers
SET
    detail_fragment = 'radiates intense heat, making the air shimmer',
    ambient_sounds = '["low rumble of flowing lava", "hissing steam vents", "crackle of flames"]',
    particle_effects = '["heat shimmer", "smoke", "ember sparks"]',
    notes = 'Muspelheim biome - extreme heat and fire damage'
WHERE modifier_name = 'Scorched';

-- Update Frozen modifier with room-specific details
UPDATE Descriptor_Thematic_Modifiers
SET
    detail_fragment = 'is encased in thick sheets of ancient ice',
    ambient_sounds = '["creak and crack of shifting ice", "howling wind through passages", "drip of melting frost"]',
    particle_effects = '["ice crystals", "frozen mist", "frost particles"]',
    notes = 'Niflheim biome - extreme cold and ice'
WHERE modifier_name = 'Frozen';

-- Update Crystalline modifier with room-specific details
UPDATE Descriptor_Thematic_Modifiers
SET
    detail_fragment = 'has crystallized into bizarre, impossible formations',
    ambient_sounds = '["high-pitched shriek of the Cursed Choir", "reality hums and vibrates", "discordant crystalline chimes"]',
    particle_effects = '["aetheric shimmer", "reality distortion", "crystalline gleam"]',
    notes = 'Alfheim biome - Aetheric corruption and reality distortion'
WHERE modifier_name = 'Crystalline';

-- Insert Monolithic modifier (Jötunheim)
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name, primary_biome, adjective, detail_fragment,
    stat_modifiers, status_effects, color_palette, ambient_sounds, particle_effects, notes
) VALUES (
    'Monolithic', 'Jotunheim', 'monolithic',
    'is constructed on a massive, inhuman scale',
    '{}',
    '[]',
    'gray-steel-dark',
    '["distant metallic clanging", "groan of enormous structures", "echoes of titanic footsteps"]',
    '["dust motes", "rust flakes", "industrial debris"]',
    'Jötunheim biome - massive scale industrial ruins'
);

COMMIT;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Test 1: Base Templates Created
-- SELECT COUNT(*) as template_count FROM Descriptor_Base_Templates WHERE category = 'Room';
-- Expected: 18 (3 from v0.38.0 + 15 from v0.38.1)

-- Test 2: Thematic Modifiers Updated
-- SELECT COUNT(*) as modifier_count FROM Descriptor_Thematic_Modifiers;
-- Expected: 5 (Rusted, Scorched, Frozen, Crystalline, Monolithic)

-- Test 3: Descriptor Fragments Table Created
-- SELECT COUNT(*) as fragment_count FROM Descriptor_Fragments;
-- Expected: 0 (will be populated in next script)

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [x] Descriptor_Fragments table created
-- [x] Room_Function_Variants table created
-- [x] 15+ room base templates inserted
-- [x] All 6 core archetypes covered
-- [x] 5 thematic modifiers updated with room details
-- [x] Schema validates without errors
-- =====================================================

-- END v0.38.1 ROOM DESCRIPTION LIBRARY SCHEMA
