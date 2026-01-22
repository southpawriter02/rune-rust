-- ==============================================================================
-- v0.39.1: 3D Vertical Layer System - Database Schema
-- ==============================================================================
-- Purpose: Extend Rooms table with 3D coordinates and create vertical connections
-- Prerequisites: Existing Rooms table from v0.10-v0.12
-- Timeline: Part of v0.39 Advanced Dynamic Room Engine
-- ==============================================================================

-- ==============================================================================
-- ALTER EXISTING ROOMS TABLE - Add 3D Spatial Coordinates
-- ==============================================================================

-- Add 3D coordinate columns to existing Rooms table
ALTER TABLE Rooms ADD COLUMN coord_x INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN coord_y INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN coord_z INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN vertical_layer TEXT DEFAULT 'GroundLevel';

-- Create indexes for spatial queries
CREATE INDEX IF NOT EXISTS idx_rooms_position ON Rooms(coord_x, coord_y, coord_z);
CREATE INDEX IF NOT EXISTS idx_rooms_layer ON Rooms(vertical_layer);
CREATE INDEX IF NOT EXISTS idx_rooms_z_level ON Rooms(coord_z);

-- ==============================================================================
-- NEW: VERTICAL CONNECTIONS TABLE
-- ==============================================================================
-- Stores connections between rooms at different Z levels (stairs, shafts, etc.)
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Vertical_Connections (
    connection_id INTEGER PRIMARY KEY AUTOINCREMENT,
    from_room_id TEXT NOT NULL,              -- Room ID (string format from v0.10)
    to_room_id TEXT NOT NULL,                -- Target room ID
    connection_type TEXT NOT NULL,           -- Stairs, Shaft, Elevator, Ladder, Collapsed
    traversal_dc INTEGER DEFAULT 0,          -- Skill check difficulty (0 = no check)
    is_blocked BOOLEAN DEFAULT 0,            -- Whether passage is currently blocked
    blockage_description TEXT,               -- Description of blockage
    levels_spanned INTEGER DEFAULT 1,        -- Number of Z levels this connection spans
    description TEXT,                        -- Flavor description of connection
    hazards TEXT,                            -- JSON array of hazards during traversal
    is_bidirectional BOOLEAN DEFAULT 1,      -- Can traverse both ways
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CHECK (connection_type IN ('Stairs', 'Shaft', 'Elevator', 'Ladder', 'Collapsed')),
    CHECK (traversal_dc >= 0 AND traversal_dc <= 25),
    CHECK (levels_spanned >= 1 AND levels_spanned <= 6),
    CHECK (is_blocked IN (0, 1)),
    CHECK (is_bidirectional IN (0, 1))
);

-- Indexes for vertical connection queries
CREATE INDEX IF NOT EXISTS idx_vertical_from ON Vertical_Connections(from_room_id);
CREATE INDEX IF NOT EXISTS idx_vertical_to ON Vertical_Connections(to_room_id);
CREATE INDEX IF NOT EXISTS idx_vertical_type ON Vertical_Connections(connection_type);
CREATE INDEX IF NOT EXISTS idx_vertical_blocked ON Vertical_Connections(is_blocked);

-- ==============================================================================
-- NEW: SPATIAL VALIDATION LOG TABLE
-- ==============================================================================
-- Tracks spatial validation issues for debugging and telemetry
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Spatial_Validation_Log (
    validation_id INTEGER PRIMARY KEY AUTOINCREMENT,
    sector_id INTEGER,                       -- Associated sector (if applicable)
    validation_type TEXT NOT NULL,           -- Overlap, Unreachable, MissingConnection, LayerBounds
    severity TEXT NOT NULL,                  -- Warning, Error, Critical
    room_id_1 TEXT,                          -- First affected room
    room_id_2 TEXT,                          -- Second affected room (for overlaps)
    position_x INTEGER,                      -- X coordinate of issue
    position_y INTEGER,                      -- Y coordinate of issue
    position_z INTEGER,                      -- Z coordinate of issue
    description TEXT NOT NULL,               -- Human-readable description
    validated_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CHECK (validation_type IN ('Overlap', 'Unreachable', 'MissingConnection', 'LayerBounds', 'InvalidFootprint')),
    CHECK (severity IN ('Warning', 'Error', 'Critical'))
);

-- Indexes for validation log queries
CREATE INDEX IF NOT EXISTS idx_validation_sector ON Spatial_Validation_Log(sector_id);
CREATE INDEX IF NOT EXISTS idx_validation_type ON Spatial_Validation_Log(validation_type);
CREATE INDEX IF NOT EXISTS idx_validation_severity ON Spatial_Validation_Log(severity);
CREATE INDEX IF NOT EXISTS idx_validation_timestamp ON Spatial_Validation_Log(validated_at);

-- ==============================================================================
-- EXAMPLE DATA - Reference Implementation
-- ==============================================================================

-- Example 1: Entry Hall at origin (GroundLevel)
-- INSERT INTO Rooms (room_id, name, coord_x, coord_y, coord_z, vertical_layer)
-- VALUES ('room_1', 'Collapsed Entry Hall', 0, 0, 0, 'GroundLevel');

-- Example 2: Corridor to the north (same level)
-- INSERT INTO Rooms (room_id, name, coord_x, coord_y, coord_z, vertical_layer)
-- VALUES ('room_2', 'Rust-Choked Corridor', 0, 1, 0, 'GroundLevel');

-- Example 3: Chamber below entry hall (one level down)
-- INSERT INTO Rooms (room_id, name, coord_x, coord_y, coord_z, vertical_layer)
-- VALUES ('room_3', 'Geothermal Chamber', 0, 0, -1, 'UpperRoots');

-- Example 4: Stairs connecting entry hall to chamber (free traversal)
-- INSERT INTO Vertical_Connections (
--     from_room_id, to_room_id, connection_type,
--     traversal_dc, levels_spanned, description, is_bidirectional
-- )
-- VALUES (
--     'room_1', 'room_3', 'Stairs',
--     0, 1,
--     'Corroded metal stairs descend into the geothermal levels. The steps are slick with condensation.',
--     1
-- );

-- Example 5: Maintenance shaft (requires Athletics check)
-- INSERT INTO Vertical_Connections (
--     from_room_id, to_room_id, connection_type,
--     traversal_dc, levels_spanned, description, is_bidirectional
-- )
-- VALUES (
--     'room_3', 'room_5', 'Shaft',
--     12, 2,
--     'A maintenance shaft plunges deeper into the infrastructure. Rusted handholds line the walls.',
--     1
-- );

-- Example 6: Blocked shaft (impassable until cleared)
-- INSERT INTO Vertical_Connections (
--     from_room_id, to_room_id, connection_type,
--     traversal_dc, is_blocked, blockage_description, levels_spanned
-- )
-- VALUES (
--     'room_5', 'room_7', 'Shaft',
--     12, 1,
--     'The maintenance shaft is choked with debris from a ceiling collapse. Clearing it would require significant effort (MIGHT DC 15, 10 minutes).',
--     2
-- );

-- Example 7: Elevator (may be powered or broken)
-- INSERT INTO Vertical_Connections (
--     from_room_id, to_room_id, connection_type,
--     traversal_dc, levels_spanned, description
-- )
-- VALUES (
--     'room_2', 'room_8', 'Elevator',
--     0, 3,
--     'A cargo elevator with flickering control panels. The mechanism groans ominously but appears functional.',
--     1
-- );

-- ==============================================================================
-- VALIDATION QUERIES
-- ==============================================================================

-- Query 1: Find all rooms at a specific Z level
-- SELECT room_id, name, coord_x, coord_y, coord_z, vertical_layer
-- FROM Rooms
-- WHERE coord_z = -1
-- ORDER BY coord_x, coord_y;

-- Query 2: Find all vertical connections from a room
-- SELECT vc.*, r_to.name AS destination_name
-- FROM Vertical_Connections vc
-- JOIN Rooms r_to ON vc.to_room_id = r_to.room_id
-- WHERE vc.from_room_id = 'room_1'
-- AND vc.is_blocked = 0;

-- Query 3: Find all rooms reachable via stairs (no checks required)
-- SELECT DISTINCT r.*
-- FROM Rooms r
-- JOIN Vertical_Connections vc ON (r.room_id = vc.to_room_id OR r.room_id = vc.from_room_id)
-- WHERE vc.connection_type = 'Stairs'
-- AND vc.is_blocked = 0;

-- Query 4: Find potential room overlaps (same position)
-- SELECT r1.room_id AS room1, r2.room_id AS room2,
--        r1.coord_x, r1.coord_y, r1.coord_z
-- FROM Rooms r1
-- JOIN Rooms r2 ON r1.coord_x = r2.coord_x
--              AND r1.coord_y = r2.coord_y
--              AND r1.coord_z = r2.coord_z
--              AND r1.room_id < r2.room_id;

-- Query 5: Count rooms per vertical layer
-- SELECT vertical_layer, COUNT(*) AS room_count
-- FROM Rooms
-- WHERE coord_x IS NOT NULL
-- GROUP BY vertical_layer
-- ORDER BY coord_z;

-- Query 6: Find all blocked vertical connections
-- SELECT vc.*, r_from.name AS from_name, r_to.name AS to_name
-- FROM Vertical_Connections vc
-- JOIN Rooms r_from ON vc.from_room_id = r_from.room_id
-- JOIN Rooms r_to ON vc.to_room_id = r_to.room_id
-- WHERE vc.is_blocked = 1;

-- ==============================================================================
-- MIGRATION NOTES
-- ==============================================================================
-- For existing databases:
-- 1. Run this script to add new columns and tables
-- 2. All existing rooms will default to (0, 0, 0, 'GroundLevel')
-- 3. Regenerate dungeons to populate 3D coordinates via SpatialLayoutService
-- 4. Legacy rooms without coordinates will remain playable but won't have vertical traversal
-- ==============================================================================

SELECT 'v0.39.1 Spatial Layout Schema installed successfully' AS status;
