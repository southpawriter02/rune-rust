-- =====================================================
-- v0.38.14: Trauma Descriptor Library Schema
-- =====================================================
-- Version: v0.38.14
-- Author: Rune & Rust Development Team
-- Date: 2025-11-23
-- Prerequisites: v0.15 (Trauma Economy System), v0.38.0 (Descriptor Framework)
-- =====================================================
-- Document ID: RR-SPEC-v0.38.14-DATABASE
-- Parent Specification: v0.38 Descriptor Library & Content Database
-- Status: Implementation
-- Timeline: 10-12 hours (Trauma Descriptor Library)
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- TRAUMA DESCRIPTORS TABLE
-- =====================================================
-- Stores narrative descriptors for trauma acquisition, manifestation, triggers, and recovery

CREATE TABLE IF NOT EXISTS Trauma_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    descriptor_name TEXT NOT NULL UNIQUE,  -- "Flashbacks_Acquisition_Combat"

    -- Trauma linkage
    trauma_id TEXT NOT NULL,  -- Links to Trauma.TraumaId (e.g., "paranoia", "flashbacks")
    trauma_name TEXT NOT NULL,  -- Display name (e.g., "[PARANOIA]", "[FLASHBACKS]")

    -- Descriptor type and context
    descriptor_type TEXT NOT NULL CHECK(descriptor_type IN (
        'Acquisition',      -- Moment of trauma acquisition (Breaking Point)
        'Manifestation',    -- Ongoing effects during gameplay
        'Trigger',          -- Contextual triggering
        'BreakingPoint',    -- Approaching/reaching breaking point
        'Suppression',      -- Cognitive Stabilizer usage
        'Recovery'          -- Saga Quest removal
    )),
    context_tag TEXT,  -- "Combat", "Environmental", "Social", "Isolation", "Blight", etc.

    -- Narrative content
    descriptor_text TEXT NOT NULL,  -- The actual flavor text
    progression_level INTEGER CHECK(progression_level IN (1, 2, 3)),  -- Trauma progression level

    -- Mechanical context (JSON)
    mechanical_context TEXT,  -- {"stress_threshold": 75, "trigger_condition": "low_health"}

    -- Display/selection rules
    spawn_weight REAL DEFAULT 1.0,  -- Probability weight for random selection
    display_conditions TEXT,  -- JSON: {"min_stress": 50, "has_trauma": "paranoia"}

    -- Metadata
    tags TEXT,  -- JSON array: ["Intrusive", "Combat", "Visceral"]
    notes TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trauma_descriptors_trauma_id ON Trauma_Descriptors(trauma_id);
CREATE INDEX IF NOT EXISTS idx_trauma_descriptors_type ON Trauma_Descriptors(descriptor_type);
CREATE INDEX IF NOT EXISTS idx_trauma_descriptors_context ON Trauma_Descriptors(context_tag);
CREATE INDEX IF NOT EXISTS idx_trauma_descriptors_progression ON Trauma_Descriptors(progression_level);

-- =====================================================
-- BREAKING POINT DESCRIPTORS TABLE
-- =====================================================
-- Specialized table for stress threshold warnings and breaking point moments

CREATE TABLE IF NOT EXISTS Breaking_Point_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    descriptor_name TEXT NOT NULL UNIQUE,  -- "Warning_Mental_Fracture_01"

    -- Stress thresholds
    stress_threshold_min INTEGER NOT NULL,  -- 75 for "approaching", 100 for "breaking"
    stress_threshold_max INTEGER NOT NULL,  -- 99 for "approaching", 100 for "breaking"

    -- Descriptor phase
    phase TEXT NOT NULL CHECK(phase IN (
        'Warning',          -- Warning signs (75-99% stress)
        'Breaking',         -- Breaking point moment (100% stress)
        'SystemMessage',    -- System prompt for resolve check
        'ResolveSuccess',   -- Successful resolve check
        'ResolveFailure',   -- Failed resolve check
        'TraumaReveal'      -- Specific trauma reveal message
    )),

    -- Narrative content
    descriptor_text TEXT NOT NULL,

    -- Display rules
    spawn_weight REAL DEFAULT 1.0,
    tags TEXT,  -- JSON array: ["Visceral", "Psychological", "Horror"]

    -- Metadata
    notes TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_breaking_point_phase ON Breaking_Point_Descriptors(phase);
CREATE INDEX IF NOT EXISTS idx_breaking_point_stress ON Breaking_Point_Descriptors(stress_threshold_min, stress_threshold_max);

-- =====================================================
-- TRAUMA TRIGGER CONDITIONS TABLE
-- =====================================================
-- Defines specific conditions that trigger trauma manifestations

CREATE TABLE IF NOT EXISTS Trauma_Trigger_Conditions (
    trigger_id INTEGER PRIMARY KEY AUTOINCREMENT,
    trigger_name TEXT NOT NULL UNIQUE,  -- "Combat_Low_Health", "Environmental_Darkness"

    -- Trigger definition
    trigger_category TEXT NOT NULL,  -- "Environmental", "Combat", "Social"
    trigger_condition TEXT NOT NULL,  -- "low_health", "darkness", "betrayal", "similar_enemy"

    -- Linked trauma types (which traumas respond to this trigger)
    applicable_trauma_ids TEXT NOT NULL,  -- JSON array: ["flashbacks", "battle_tremors"]

    -- Mechanical thresholds (JSON)
    thresholds TEXT,  -- {"health_percent": 25, "light_level": "Dim"}

    -- Narrative fragments
    trigger_description TEXT NOT NULL,  -- "When health drops below 25%"
    stress_impact TEXT,  -- "+5 Psychic Stress" or "Disadvantage on next action"

    -- Metadata
    tags TEXT,
    notes TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trigger_conditions_category ON Trauma_Trigger_Conditions(trigger_category);

-- =====================================================
-- RECOVERY DESCRIPTORS TABLE
-- =====================================================
-- Describes trauma suppression (Cognitive Stabilizer) and removal (Saga Quests)

CREATE TABLE IF NOT EXISTS Recovery_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    descriptor_name TEXT NOT NULL UNIQUE,  -- "Stabilizer_Application_Relief"

    -- Recovery type
    recovery_type TEXT NOT NULL CHECK(recovery_type IN (
        'Stabilizer_Application',  -- Using Cognitive Stabilizer
        'Stabilizer_Duration',     -- While stabilizer is active
        'Stabilizer_Wearing_Off',  -- When effect ends
        'Quest_Beginning',         -- Starting trauma removal quest
        'Quest_During',            -- During quest progression
        'Quest_Completion',        -- Successful trauma removal
        'Healing_Moment'           -- Emotional release
    )),

    -- Trauma specificity (null = applies to all traumas)
    trauma_id TEXT,  -- Optional: specific trauma this recovery applies to

    -- Narrative content
    descriptor_text TEXT NOT NULL,

    -- Display rules
    spawn_weight REAL DEFAULT 1.0,
    tags TEXT,

    -- Metadata
    notes TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_recovery_type ON Recovery_Descriptors(recovery_type);
CREATE INDEX IF NOT EXISTS idx_recovery_trauma_id ON Recovery_Descriptors(trauma_id);

-- =====================================================
-- HELPER VIEWS
-- =====================================================

-- View: Trauma Descriptors by Type
CREATE VIEW IF NOT EXISTS View_Trauma_Descriptors_By_Type AS
SELECT
    descriptor_type,
    COUNT(*) as descriptor_count,
    COUNT(DISTINCT trauma_id) as unique_trauma_count
FROM Trauma_Descriptors
GROUP BY descriptor_type;

-- View: Trauma Descriptors by Context
CREATE VIEW IF NOT EXISTS View_Trauma_Descriptors_By_Context AS
SELECT
    context_tag,
    descriptor_type,
    COUNT(*) as descriptor_count
FROM Trauma_Descriptors
WHERE context_tag IS NOT NULL
GROUP BY context_tag, descriptor_type;

-- View: Complete Trauma Descriptor Details
CREATE VIEW IF NOT EXISTS View_Trauma_Descriptor_Details AS
SELECT
    td.descriptor_id,
    td.descriptor_name,
    td.trauma_id,
    td.trauma_name,
    td.descriptor_type,
    td.context_tag,
    td.descriptor_text,
    td.progression_level,
    td.spawn_weight,
    td.tags
FROM Trauma_Descriptors td
ORDER BY td.trauma_id, td.descriptor_type, td.context_tag;

-- View: Breaking Point Descriptors by Phase
CREATE VIEW IF NOT EXISTS View_Breaking_Point_By_Phase AS
SELECT
    phase,
    COUNT(*) as descriptor_count,
    AVG(stress_threshold_min) as avg_min_threshold,
    AVG(stress_threshold_max) as avg_max_threshold
FROM Breaking_Point_Descriptors
GROUP BY phase;

-- View: Trauma Trigger Coverage
CREATE VIEW IF NOT EXISTS View_Trauma_Trigger_Coverage AS
SELECT
    ttc.trigger_category,
    ttc.trigger_condition,
    ttc.trigger_name,
    COUNT(*) as trigger_count
FROM Trauma_Trigger_Conditions ttc
GROUP BY ttc.trigger_category, ttc.trigger_condition;

-- View: Recovery Descriptor Coverage
CREATE VIEW IF NOT EXISTS View_Recovery_Descriptor_Coverage AS
SELECT
    recovery_type,
    COUNT(*) as descriptor_count,
    COUNT(DISTINCT trauma_id) as trauma_specific_count
FROM Recovery_Descriptors
GROUP BY recovery_type;

-- =====================================================
-- MIGRATION TRACKING
-- =====================================================

INSERT OR IGNORE INTO Schema_Migrations (migration_name, description)
VALUES ('v0.38.14_trauma_descriptor_library_schema', 'Add trauma descriptor library tables for psychological trauma system');

COMMIT;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Test 1: Trauma Descriptors Table Created
-- SELECT COUNT(*) FROM Trauma_Descriptors;
-- Expected: 0 (will be populated by content scripts)

-- Test 2: Breaking Point Descriptors Table Created
-- SELECT COUNT(*) FROM Breaking_Point_Descriptors;
-- Expected: 0 (will be populated by content scripts)

-- Test 3: Trauma Trigger Conditions Table Created
-- SELECT COUNT(*) FROM Trauma_Trigger_Conditions;
-- Expected: 0 (will be populated by content scripts)

-- Test 4: Recovery Descriptors Table Created
-- SELECT COUNT(*) FROM Recovery_Descriptors;
-- Expected: 0 (will be populated by content scripts)

-- Test 5: Views Created
-- SELECT * FROM View_Trauma_Descriptors_By_Type;
-- Expected: Empty result set (no data yet)

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] Trauma_Descriptors table created
-- [ ] Breaking_Point_Descriptors table created
-- [ ] Trauma_Trigger_Conditions table created
-- [ ] Recovery_Descriptors table created
-- [ ] All indexes created
-- [ ] Helper views created (6 views)
-- [ ] Schema migration tracked
-- [ ] SQL script executes without errors
-- =====================================================

-- END v0.38.14 TRAUMA DESCRIPTOR LIBRARY SCHEMA
