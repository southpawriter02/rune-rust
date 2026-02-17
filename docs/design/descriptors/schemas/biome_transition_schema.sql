-- ==============================================================================
-- v0.39.2: Biome Transition & Blending - Database Schema
-- ==============================================================================
-- Purpose: Enable multi-biome sectors with logical environmental transitions
-- Prerequisites: v0.10-v0.12 (Dynamic Room Engine), v0.29-v0.32 (Biomes),
--                v0.38 (Descriptor Library), v0.39.1 (3D Vertical System)
-- Timeline: Part of v0.39 Advanced Dynamic Room Engine
-- ==============================================================================

BEGIN TRANSACTION;

-- ==============================================================================
-- ALTER EXISTING ROOMS TABLE - Add Biome Blending Support
-- ==============================================================================

-- Add biome blending columns to existing Rooms table
ALTER TABLE Rooms ADD COLUMN primary_biome TEXT DEFAULT 'TheRoots';
ALTER TABLE Rooms ADD COLUMN secondary_biome TEXT DEFAULT NULL;
ALTER TABLE Rooms ADD COLUMN biome_blend_ratio REAL DEFAULT 0.0;

-- Create indexes for biome queries
CREATE INDEX IF NOT EXISTS idx_rooms_primary_biome ON Rooms(primary_biome);
CREATE INDEX IF NOT EXISTS idx_rooms_biomes ON Rooms(primary_biome, secondary_biome);

-- Constraints (enforced at application level due to SQLite ALTER TABLE limitations)
-- biome_blend_ratio should be between 0.0 and 1.0
-- 0.0 = 100% primary_biome, 1.0 = 100% secondary_biome
-- secondary_biome can be NULL for pure single-biome rooms

-- ==============================================================================
-- NEW: BIOME ADJACENCY RULES
-- ==============================================================================
-- Defines which biomes can be adjacent and transition requirements
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Biome_Adjacency (
    adjacency_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_a TEXT NOT NULL,                          -- First biome (e.g., "TheRoots")
    biome_b TEXT NOT NULL,                          -- Second biome (e.g., "Muspelheim")
    compatibility TEXT NOT NULL,                    -- Compatible, RequiresTransition, Incompatible
    min_transition_rooms INTEGER DEFAULT 0,         -- Minimum transition rooms required
    max_transition_rooms INTEGER DEFAULT 3,         -- Maximum transition rooms recommended
    transition_theme TEXT,                          -- Theme description (e.g., "Geothermal escalation")
    notes TEXT,                                     -- Design notes and rationale
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CHECK (compatibility IN ('Compatible', 'RequiresTransition', 'Incompatible')),
    CHECK (min_transition_rooms >= 0 AND min_transition_rooms <= max_transition_rooms),
    CHECK (max_transition_rooms >= 0 AND max_transition_rooms <= 6),
    UNIQUE(biome_a, biome_b)
);

CREATE INDEX IF NOT EXISTS idx_adjacency_biomes ON Biome_Adjacency(biome_a, biome_b);
CREATE INDEX IF NOT EXISTS idx_adjacency_compatibility ON Biome_Adjacency(compatibility);

-- ==============================================================================
-- NEW: ROOM ENVIRONMENTAL PROPERTIES
-- ==============================================================================
-- Stores environmental gradient values (temperature, Aetheric intensity, scale)
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Room_Environmental_Properties (
    property_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id TEXT NOT NULL,                          -- Room ID (string format from v0.10)
    property_name TEXT NOT NULL,                    -- Temperature, AethericIntensity, Scale, etc.
    property_value REAL NOT NULL,                   -- Numeric value (temperature in Celsius, intensity 0-1, scale factor)
    property_description TEXT,                      -- Human-readable description
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CHECK (property_name IN (
        'Temperature',
        'AethericIntensity',
        'ScaleFactor',
        'Humidity',
        'Pressure',
        'LightLevel',
        'RadiationLevel',
        'CorrosionRate'
    )),
    UNIQUE(room_id, property_name)
);

CREATE INDEX IF NOT EXISTS idx_env_properties_room ON Room_Environmental_Properties(room_id);
CREATE INDEX IF NOT EXISTS idx_env_properties_name ON Room_Environmental_Properties(property_name);

-- ==============================================================================
-- POPULATE DEFAULT BIOME ADJACENCY RULES
-- ==============================================================================

-- TheRoots adjacency rules
INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms, transition_theme, notes)
VALUES
    ('TheRoots', 'Muspelheim', 'RequiresTransition', 1, 2, 'Geothermal escalation', 'Heat gradually increases from broken cooling systems to volcanic heat'),
    ('TheRoots', 'Niflheim', 'RequiresTransition', 1, 2, 'Cooling failure', 'Temperature drops as cooling systems freeze over'),
    ('TheRoots', 'Jotunheim', 'Compatible', 0, 1, 'Industrial overlap', 'Both use massive industrial architecture'),
    ('TheRoots', 'Alfheim', 'Compatible', 0, 1, 'Aetheric seepage', 'Aetheric energy leaks from Alfheim containment'),
    ('TheRoots', 'NeutralZone', 'Compatible', 0, 1, 'Standard transition', 'Minimal environmental change');

-- Muspelheim adjacency rules
INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms, transition_theme, notes)
VALUES
    ('Muspelheim', 'TheRoots', 'RequiresTransition', 1, 2, 'Geothermal escalation', 'Heat gradually decreases into industrial zones'),
    ('Muspelheim', 'Niflheim', 'Incompatible', 0, 0, NULL, 'Fire and ice cannot directly coexist - requires neutral zone intermediary'),
    ('Muspelheim', 'Jotunheim', 'RequiresTransition', 2, 3, 'Heat dissipation', 'Volcanic heat gradually cools in giant-scale chambers'),
    ('Muspelheim', 'Alfheim', 'RequiresTransition', 1, 2, 'Aetheric fire fade', 'Volcanic heat meets Aetheric energy'),
    ('Muspelheim', 'NeutralZone', 'RequiresTransition', 2, 3, 'Heat dissipation', 'Extreme heat gradually becomes tolerable');

-- Niflheim adjacency rules
INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms, transition_theme, notes)
VALUES
    ('Niflheim', 'TheRoots', 'RequiresTransition', 1, 2, 'Cooling failure', 'Sub-zero temperatures warm to industrial ambient'),
    ('Niflheim', 'Muspelheim', 'Incompatible', 0, 0, NULL, 'Ice and fire cannot directly coexist - requires neutral zone intermediary'),
    ('Niflheim', 'Jotunheim', 'RequiresTransition', 1, 2, 'Frozen scale', 'Massive architecture becomes ice-encased'),
    ('Niflheim', 'Alfheim', 'RequiresTransition', 1, 2, 'Frozen Aetheric', 'Aetheric energy crystallizes in extreme cold'),
    ('Niflheim', 'NeutralZone', 'RequiresTransition', 2, 3, 'Warming trend', 'Extreme cold gradually becomes tolerable');

-- Jotunheim adjacency rules
INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms, transition_theme, notes)
VALUES
    ('Jotunheim', 'TheRoots', 'Compatible', 0, 1, 'Industrial overlap', 'Giant-scale transitions to human-scale infrastructure'),
    ('Jotunheim', 'Muspelheim', 'RequiresTransition', 2, 3, 'Heat dissipation', 'Massive chambers transition to volcanic heat'),
    ('Jotunheim', 'Niflheim', 'RequiresTransition', 1, 2, 'Frozen scale', 'Colossal architecture meets extreme cold'),
    ('Jotunheim', 'Alfheim', 'RequiresTransition', 1, 2, 'Scale distortion', 'Giant-scale meets Aetheric spatial warping'),
    ('Jotunheim', 'NeutralZone', 'RequiresTransition', 1, 2, 'Scale transition', 'Massive architecture becomes human-scale');

-- Alfheim adjacency rules
INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms, transition_theme, notes)
VALUES
    ('Alfheim', 'TheRoots', 'Compatible', 0, 1, 'Aetheric seepage', 'Aetheric energy fades into mundane reality'),
    ('Alfheim', 'Muspelheim', 'RequiresTransition', 1, 2, 'Aetheric fire fade', 'Aetheric energy transitions to volcanic heat'),
    ('Alfheim', 'Niflheim', 'RequiresTransition', 1, 2, 'Frozen Aetheric', 'Aetheric energy becomes crystallized by cold'),
    ('Alfheim', 'Jotunheim', 'RequiresTransition', 1, 2, 'Scale distortion', 'Aetheric warping meets giant-scale'),
    ('Alfheim', 'NeutralZone', 'Compatible', 0, 1, 'Aetheric fade', 'Aetheric presence gradually diminishes');

-- NeutralZone adjacency rules (all biomes can connect to NeutralZone)
INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms, transition_theme, notes)
VALUES
    ('NeutralZone', 'TheRoots', 'Compatible', 0, 1, 'Standard transition', 'Neutral areas transition to industrial'),
    ('NeutralZone', 'Muspelheim', 'RequiresTransition', 2, 3, 'Heat dissipation', 'Moderate zones warm to volcanic heat'),
    ('NeutralZone', 'Niflheim', 'RequiresTransition', 2, 3, 'Warming trend', 'Moderate zones cool to sub-zero'),
    ('NeutralZone', 'Jotunheim', 'RequiresTransition', 1, 2, 'Scale transition', 'Human-scale transitions to giant-scale'),
    ('NeutralZone', 'Alfheim', 'Compatible', 0, 1, 'Aetheric fade', 'Mundane reality meets Aetheric energy');

-- ==============================================================================
-- VALIDATION QUERIES
-- ==============================================================================

-- Query 1: Find all transition rooms (rooms with blend ratio > 0)
-- SELECT room_id, name, primary_biome, secondary_biome, biome_blend_ratio
-- FROM Rooms
-- WHERE secondary_biome IS NOT NULL
-- AND biome_blend_ratio > 0
-- ORDER BY biome_blend_ratio;

-- Query 2: Find adjacency rule for two biomes
-- SELECT * FROM Biome_Adjacency
-- WHERE (biome_a = 'TheRoots' AND biome_b = 'Muspelheim')
--    OR (biome_a = 'Muspelheim' AND biome_b = 'TheRoots');

-- Query 3: Find all incompatible biome pairs
-- SELECT biome_a, biome_b, notes
-- FROM Biome_Adjacency
-- WHERE compatibility = 'Incompatible';

-- Query 4: Get environmental properties for a room
-- SELECT property_name, property_value, property_description
-- FROM Room_Environmental_Properties
-- WHERE room_id = 'room_123'
-- ORDER BY property_name;

-- Query 5: Find all rooms with specific temperature range
-- SELECT r.room_id, r.name, r.primary_biome, rep.property_value AS temperature
-- FROM Rooms r
-- JOIN Room_Environmental_Properties rep ON r.room_id = rep.room_id
-- WHERE rep.property_name = 'Temperature'
-- AND rep.property_value BETWEEN 0 AND 100
-- ORDER BY rep.property_value;

-- Query 6: Count transition rooms between specific biome pair
-- SELECT COUNT(*) AS transition_count,
--        AVG(biome_blend_ratio) AS avg_blend
-- FROM Rooms
-- WHERE primary_biome = 'TheRoots'
-- AND secondary_biome = 'Muspelheim';

-- ==============================================================================
-- MIGRATION NOTES
-- ==============================================================================
-- For existing databases:
-- 1. Run this script to add new columns and tables
-- 2. All existing rooms will default to primary_biome='TheRoots', secondary_biome=NULL, blend_ratio=0
-- 3. Regenerate dungeons to populate biome transitions via BiomeTransitionService
-- 4. Legacy rooms without biome blending will remain playable as single-biome rooms
-- ==============================================================================

COMMIT;

SELECT 'v0.39.2 Biome Transition Schema installed successfully' AS status;
SELECT 'Biome Adjacency Rules: ' || COUNT(*) AS count FROM Biome_Adjacency;
