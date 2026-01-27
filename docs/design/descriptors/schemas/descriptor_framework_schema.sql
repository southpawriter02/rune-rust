-- =====================================================
-- v0.38.0: Descriptor Library Framework Schema
-- =====================================================
-- Version: v0.38.0
-- Author: Rune & Rust Development Team
-- Date: 2025-11-17
-- Prerequisites: v0.29-v0.32 (Biome Implementations)
-- =====================================================
-- Document ID: RR-SPEC-v0.38.0-DATABASE
-- Parent Specification: v0.38 Descriptor Library & Content Database
-- Status: Implementation
-- Timeline: 12-15 hours (Parent Framework)
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- TIER 1: BASE DESCRIPTOR TEMPLATES
-- =====================================================
-- Defines biome-agnostic archetypes (Pillar_Base, Corridor_Base, etc.)

CREATE TABLE IF NOT EXISTS Descriptor_Base_Templates (
    template_id INTEGER PRIMARY KEY AUTOINCREMENT,
    template_name TEXT NOT NULL UNIQUE,  -- "Pillar_Base", "Corridor_Base", "Chasm_Base"
    category TEXT NOT NULL CHECK(category IN ('Room', 'Feature', 'Object', 'Atmospheric', 'Loot', 'Entity', 'Psychological', 'Combat', 'Interaction')),
    archetype TEXT NOT NULL,  -- "Cover", "Hazard", "Container", "Ambient", "ResourceNode"

    -- Mechanical properties (JSON)
    base_mechanics TEXT,  -- {"hp": 50, "soak": 10, "destructible": true, "cover_quality": "Full"}

    -- Description templates (use {Modifier}, {Modifier_Adj}, {Modifier_Detail} placeholders)
    name_template TEXT NOT NULL,  -- "{Modifier} Support Pillar"
    description_template TEXT NOT NULL,  -- "A {Modifier_Adj} pillar that {Modifier_Detail}. It provides full cover."

    -- Metadata
    tags TEXT,  -- JSON array: ["Structure", "Cover", "Destructible"]
    notes TEXT,  -- Design notes
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_descriptor_base_category ON Descriptor_Base_Templates(category);
CREATE INDEX IF NOT EXISTS idx_descriptor_base_archetype ON Descriptor_Base_Templates(archetype);

-- =====================================================
-- TIER 2: THEMATIC MODIFIERS
-- =====================================================
-- Defines biome-specific variations (Scorched, Frozen, Rusted, Crystalline)

CREATE TABLE IF NOT EXISTS Descriptor_Thematic_Modifiers (
    modifier_id INTEGER PRIMARY KEY AUTOINCREMENT,
    modifier_name TEXT NOT NULL UNIQUE,  -- "Scorched", "Frozen", "Rusted", "Crystalline"
    primary_biome TEXT NOT NULL CHECK(primary_biome IN ('The_Roots', 'Muspelheim', 'Niflheim', 'Alfheim', 'Jotunheim')),

    -- Modifier descriptive properties
    adjective TEXT NOT NULL,  -- "scorched", "ice-covered", "corroded", "crystalline"
    detail_fragment TEXT NOT NULL,  -- "radiates intense heat", "drips with meltwater", "shows signs of decay"

    -- Mechanical modifiers (JSON)
    stat_modifiers TEXT,  -- {"fire_resistance": -50, "ice_vulnerability": 100}
    status_effects TEXT,  -- JSON array: [{"type": "Burning", "duration": 2}]

    -- Visual/atmospheric properties
    color_palette TEXT,  -- "red-orange-black"
    ambient_sounds TEXT,  -- JSON array: ["hissing steam", "crackling flames"]
    particle_effects TEXT,  -- JSON array: ["heat_shimmer", "smoke"]

    -- Metadata
    notes TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_modifier_biome ON Descriptor_Thematic_Modifiers(primary_biome);

-- =====================================================
-- TIER 3: COMPOSITE DESCRIPTORS
-- =====================================================
-- Generated combinations of Base Templates + Thematic Modifiers
-- Example: Pillar_Base + Scorched = "Scorched Support Pillar"

CREATE TABLE IF NOT EXISTS Descriptor_Composites (
    composite_id INTEGER PRIMARY KEY AUTOINCREMENT,
    base_template_id INTEGER NOT NULL,
    modifier_id INTEGER,  -- NULL for unmodified base templates

    -- Generated properties (computed from base + modifier)
    final_name TEXT NOT NULL,  -- "Scorched Support Pillar"
    final_description TEXT NOT NULL,  -- Full description with modifiers applied
    final_mechanics TEXT,  -- Merged base + modifier mechanics (JSON)

    -- Spawn rules
    biome_restrictions TEXT,  -- JSON array: ["Muspelheim", "The_Roots"]
    spawn_weight REAL DEFAULT 1.0,  -- Probability weight for selection
    spawn_rules TEXT,  -- JSON: {"min_room_size": "Medium", "requires_tag": "Industrial", "max_per_room": 3}

    -- Metadata
    is_active INTEGER DEFAULT 1,  -- Can be disabled without deletion
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (base_template_id) REFERENCES Descriptor_Base_Templates(template_id) ON DELETE CASCADE,
    FOREIGN KEY (modifier_id) REFERENCES Descriptor_Thematic_Modifiers(modifier_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_composite_base_template ON Descriptor_Composites(base_template_id);
CREATE INDEX IF NOT EXISTS idx_composite_modifier ON Descriptor_Composites(modifier_id);
CREATE INDEX IF NOT EXISTS idx_composite_active ON Descriptor_Composites(is_active);

-- =====================================================
-- INTEGRATION: Biome_Elements Extension
-- =====================================================
-- Add columns to existing Biome_Elements table to reference composite descriptors
-- This enables backward compatibility while transitioning to the descriptor system

-- Note: Biome_Elements table should already exist from v0.10-v0.12
-- We're adding columns to link to the new descriptor system

-- First, check if the columns don't already exist (for re-running script)
-- SQLite doesn't support ALTER TABLE IF NOT EXISTS, so we'll handle this gracefully

-- Add composite_descriptor_id column (links to Descriptor_Composites)
-- This column is NULL for legacy elements that haven't been migrated yet
-- Migration will populate this column for compatible elements

PRAGMA foreign_keys = OFF;

-- Since SQLite doesn't support IF NOT EXISTS for ALTER TABLE,
-- we'll create a migration helper table to track schema changes
CREATE TABLE IF NOT EXISTS Schema_Migrations (
    migration_id INTEGER PRIMARY KEY AUTOINCREMENT,
    migration_name TEXT NOT NULL UNIQUE,
    applied_at TEXT DEFAULT CURRENT_TIMESTAMP,
    description TEXT
);

-- Check and add composite_descriptor_id column to Biome_Elements
-- We'll use a version check approach
INSERT OR IGNORE INTO Schema_Migrations (migration_name, description)
VALUES ('v0.38.0_add_composite_descriptor_id', 'Add composite_descriptor_id to Biome_Elements');

-- Only add column if migration hasn't been applied
-- (Check by attempting to query the migration table)

PRAGMA foreign_keys = ON;

-- Note: The actual ALTER TABLE will be done conditionally in the application
-- or through a migration tool. For now, we document the schema change:

-- ALTER TABLE Biome_Elements ADD COLUMN composite_descriptor_id INTEGER;
-- ALTER TABLE Biome_Elements ADD COLUMN uses_composite BOOLEAN DEFAULT 0;
--
-- FOREIGN KEY (composite_descriptor_id) REFERENCES Descriptor_Composites(composite_id) ON DELETE SET NULL

-- =====================================================
-- HELPER VIEWS
-- =====================================================

-- View: Complete descriptor information (base + modifier + composite)
CREATE VIEW IF NOT EXISTS View_Descriptor_Details AS
SELECT
    dc.composite_id,
    dc.final_name,
    dc.final_description,
    dc.final_mechanics,
    dc.biome_restrictions,
    dc.spawn_weight,
    dc.spawn_rules,
    dc.is_active,

    -- Base template info
    bt.template_id,
    bt.template_name,
    bt.category,
    bt.archetype,
    bt.base_mechanics,
    bt.tags,

    -- Modifier info (nullable)
    tm.modifier_id,
    tm.modifier_name,
    tm.primary_biome,
    tm.adjective,
    tm.detail_fragment,
    tm.stat_modifiers,
    tm.status_effects,
    tm.color_palette,
    tm.ambient_sounds,
    tm.particle_effects

FROM Descriptor_Composites dc
INNER JOIN Descriptor_Base_Templates bt ON dc.base_template_id = bt.template_id
LEFT JOIN Descriptor_Thematic_Modifiers tm ON dc.modifier_id = tm.modifier_id;

-- View: Descriptors by category
CREATE VIEW IF NOT EXISTS View_Descriptors_By_Category AS
SELECT
    category,
    COUNT(DISTINCT composite_id) as composite_count,
    COUNT(DISTINCT base_template_id) as base_template_count
FROM View_Descriptor_Details
WHERE is_active = 1
GROUP BY category;

-- View: Descriptors by biome
CREATE VIEW IF NOT EXISTS View_Descriptors_By_Biome AS
SELECT
    tm.primary_biome,
    dc.category,
    COUNT(dc.composite_id) as descriptor_count
FROM Descriptor_Composites dc
INNER JOIN Descriptor_Base_Templates bt ON dc.base_template_id = bt.template_id
LEFT JOIN Descriptor_Thematic_Modifiers tm ON dc.modifier_id = tm.modifier_id
WHERE dc.is_active = 1
GROUP BY tm.primary_biome, dc.category;

-- =====================================================
-- SEED DATA: Example Base Templates
-- =====================================================
-- A few examples to validate the system

-- Example 1: Pillar_Base (Cover feature)
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags
) VALUES (
    'Pillar_Base', 'Feature', 'Cover',
    '{"hp": 50, "soak": 8, "cover_quality": "Full", "destructible": true, "blocks_los": false}',
    '{Modifier} Support Pillar',
    'A {Modifier_Adj} pillar that {Modifier_Detail}. It provides full cover and can withstand significant damage.',
    '["Structure", "Cover", "Destructible", "Industrial"]'
);

-- Example 2: Corridor_Base (Room template)
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags
) VALUES (
    'Corridor_Base', 'Room', 'Passage',
    '{"min_connections": 2, "max_connections": 2, "size": "Small", "linear": true}',
    '{Modifier} Corridor',
    'A {Modifier_Adj} passageway that {Modifier_Detail}. The narrow confines create a sense of {Modifier} oppression.',
    '["Corridor", "Passage", "Linear", "Confined"]'
);

-- Example 3: Container_Base (Interactive object)
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags
) VALUES (
    'Container_Base', 'Object', 'Container',
    '{"destructible": true, "hp": 20, "soak": 5, "requires_interaction": true, "loot_table_id": null}',
    '{Modifier} Storage Container',
    'A {Modifier_Adj} container that {Modifier_Detail}. It may contain valuable salvage.',
    '["Container", "Lootable", "Destructible", "Interactive"]'
);

-- =====================================================
-- SEED DATA: Example Thematic Modifiers
-- =====================================================

-- Modifier 1: Scorched (Muspelheim)
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name, primary_biome, adjective, detail_fragment,
    stat_modifiers, status_effects, color_palette, ambient_sounds
) VALUES (
    'Scorched', 'Muspelheim', 'scorched',
    'radiates intense heat and shows signs of fire damage',
    '{"fire_resistance": -50, "ice_vulnerability": 100, "heat_aura_damage": 2}',
    '[{"type": "Burning", "duration": 2, "chance": 0.3}]',
    'red-orange-black',
    '["crackling flames", "hissing steam", "metal groaning"]'
);

-- Modifier 2: Frozen (Niflheim)
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name, primary_biome, adjective, detail_fragment,
    stat_modifiers, status_effects, color_palette, ambient_sounds
) VALUES (
    'Frozen', 'Niflheim', 'ice-covered',
    'drips with meltwater and is coated in thick frost',
    '{"ice_resistance": -50, "fire_vulnerability": 100, "slippery": true}',
    '[{"type": "Chilled", "duration": 2, "chance": 0.3}]',
    'white-blue-grey',
    '["cracking ice", "howling wind", "dripping water"]'
);

-- Modifier 3: Rusted (The Roots)
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name, primary_biome, adjective, detail_fragment,
    stat_modifiers, status_effects, color_palette, ambient_sounds
) VALUES (
    'Rusted', 'The_Roots', 'corroded',
    'shows extensive decay and structural weakness',
    '{"hp_multiplier": 0.7, "soak_multiplier": 0.8, "brittle": true}',
    '[{"type": "Tetanus", "duration": 3, "chance": 0.2}]',
    'rust-brown-grey',
    '["creaking metal", "dripping water", "settling debris"]'
);

-- Modifier 4: Crystalline (Alfheim)
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name, primary_biome, adjective, detail_fragment,
    stat_modifiers, status_effects, color_palette, ambient_sounds
) VALUES (
    'Crystalline', 'Alfheim', 'crystalline',
    'glows with inner light and refracts colors beautifully',
    '{"hp_multiplier": 1.2, "soak_multiplier": 0.6, "reflective": true, "light_source": true}',
    '[{"type": "Dazzled", "duration": 1, "chance": 0.15}]',
    'rainbow-white-gold',
    '["chiming tones", "resonant hum", "tinkling crystals"]'
);

-- =====================================================
-- SEED DATA: Example Composite Descriptors
-- =====================================================

-- Composite 1: Scorched Support Pillar (Pillar_Base + Scorched)
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id, modifier_id,
    final_name, final_description, final_mechanics,
    biome_restrictions, spawn_weight, spawn_rules
)
SELECT
    bt.template_id,
    tm.modifier_id,
    'Scorched Support Pillar',
    'A scorched pillar that radiates intense heat and shows signs of fire damage. It provides full cover and can withstand significant damage.',
    '{"hp": 50, "soak": 8, "cover_quality": "Full", "destructible": true, "blocks_los": false, "fire_resistance": -50, "ice_vulnerability": 100, "heat_aura_damage": 2}',
    '["Muspelheim"]',
    1.0,
    '{"min_room_size": "Medium", "max_per_room": 3}'
FROM Descriptor_Base_Templates bt, Descriptor_Thematic_Modifiers tm
WHERE bt.template_name = 'Pillar_Base' AND tm.modifier_name = 'Scorched';

-- Composite 2: Ice-Covered Support Pillar (Pillar_Base + Frozen)
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id, modifier_id,
    final_name, final_description, final_mechanics,
    biome_restrictions, spawn_weight, spawn_rules
)
SELECT
    bt.template_id,
    tm.modifier_id,
    'Ice-Covered Support Pillar',
    'An ice-covered pillar that drips with meltwater and is coated in thick frost. It provides full cover and can withstand significant damage.',
    '{"hp": 50, "soak": 8, "cover_quality": "Full", "destructible": true, "blocks_los": false, "ice_resistance": -50, "fire_vulnerability": 100, "slippery": true}',
    '["Niflheim"]',
    1.0,
    '{"min_room_size": "Medium", "max_per_room": 3}'
FROM Descriptor_Base_Templates bt, Descriptor_Thematic_Modifiers tm
WHERE bt.template_name = 'Pillar_Base' AND tm.modifier_name = 'Frozen';

-- Composite 3: Corroded Support Pillar (Pillar_Base + Rusted)
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id, modifier_id,
    final_name, final_description, final_mechanics,
    biome_restrictions, spawn_weight, spawn_rules
)
SELECT
    bt.template_id,
    tm.modifier_id,
    'Corroded Support Pillar',
    'A corroded pillar that shows extensive decay and structural weakness. It provides full cover but may collapse under heavy damage.',
    '{"hp": 35, "soak": 6, "cover_quality": "Full", "destructible": true, "blocks_los": false, "brittle": true}',
    '["The_Roots"]',
    1.2,
    '{"min_room_size": "Small", "max_per_room": 4}'
FROM Descriptor_Base_Templates bt, Descriptor_Thematic_Modifiers tm
WHERE bt.template_name = 'Pillar_Base' AND tm.modifier_name = 'Rusted';

COMMIT;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Test 1: Base Templates Created
-- SELECT COUNT(*) as base_template_count FROM Descriptor_Base_Templates;
-- Expected: 3 (Pillar_Base, Corridor_Base, Container_Base)

-- Test 2: Thematic Modifiers Created
-- SELECT COUNT(*) as modifier_count FROM Descriptor_Thematic_Modifiers;
-- Expected: 4 (Scorched, Frozen, Rusted, Crystalline)

-- Test 3: Composite Descriptors Created
-- SELECT COUNT(*) as composite_count FROM Descriptor_Composites;
-- Expected: 3 (Scorched Pillar, Frozen Pillar, Rusted Pillar)

-- Test 4: View Validation
-- SELECT * FROM View_Descriptor_Details;
-- Expected: All 3 composites with full details

-- Test 5: Category Breakdown
-- SELECT * FROM View_Descriptors_By_Category;
-- Expected: Feature: 3, Room: 0, Object: 0

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] Descriptor_Base_Templates table created
-- [ ] Descriptor_Thematic_Modifiers table created
-- [ ] Descriptor_Composites table created
-- [ ] Schema_Migrations tracking table created
-- [ ] Helper views created (View_Descriptor_Details, etc.)
-- [ ] 3 example base templates seeded
-- [ ] 4 example thematic modifiers seeded
-- [ ] 3 example composite descriptors seeded
-- [ ] All foreign key constraints working
-- [ ] All indexes created
-- [ ] SQL migration script executes without errors
-- =====================================================

-- END v0.38.0 DESCRIPTOR FRAMEWORK SCHEMA
