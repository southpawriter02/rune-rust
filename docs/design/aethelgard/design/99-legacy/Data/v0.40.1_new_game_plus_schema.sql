-- ==============================================================================
-- v0.40.1: New Game+ System - Database Schema
-- ==============================================================================
-- Purpose: Implement difficulty scaling and progression carryover for 5 NG+ tiers
-- Author: v0.40.1 Specification
-- Date: 2025-11-24
-- Dependencies: PlayerCharacter entity, Combat System, Trauma Economy
-- ==============================================================================

-- ==============================================================================
-- SECTION 1: ALTER EXISTING CHARACTERS TABLE
-- ==============================================================================
-- Note: SQLite doesn't support ALTER TABLE ADD COLUMN with AFTER clause
-- These columns will be added to the end of the table

-- Add New Game+ tier tracking columns
-- Idempotent: Check if columns exist before adding (SQLite pattern)

-- Add current_ng_plus_tier column (active NG+ tier: 0-5, 0 = first playthrough)
-- This uses a SQLite-safe pattern that checks before adding
PRAGMA table_info(Characters);
-- If column doesn't exist, add it (SQLite doesn't support IF NOT EXISTS for columns)

-- Workaround: Use a separate migration table to track schema versions
CREATE TABLE IF NOT EXISTS Schema_Migrations (
    migration_id TEXT PRIMARY KEY,
    applied_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    description TEXT
);

-- Check if v0.40.1 migration has been applied
INSERT OR IGNORE INTO Schema_Migrations (migration_id, description)
VALUES ('v0.40.1_ng_plus_columns', 'New Game+ tier tracking columns');

-- Only add columns if migration hasn't been applied before
-- Note: This is a simplified approach. In production, use a proper migration tool.
-- For now, we'll use CREATE TABLE IF NOT EXISTS pattern for new tables
-- and rely on the application layer to handle missing columns gracefully.

-- ==============================================================================
-- WORKAROUND: Schema Evolution Strategy
-- ==============================================================================
-- Since SQLite doesn't support conditional column addition easily,
-- we'll create new tables and then provide a migration script separately.
-- The application will handle missing columns by checking PRAGMA table_info.

-- For documentation purposes, here's what we need to add to Characters:
-- ALTER TABLE Characters ADD COLUMN current_ng_plus_tier INTEGER DEFAULT 0;
-- ALTER TABLE Characters ADD COLUMN highest_ng_plus_tier INTEGER DEFAULT 0;
-- ALTER TABLE Characters ADD COLUMN has_completed_campaign INTEGER DEFAULT 0; -- BOOLEAN as INTEGER
-- ALTER TABLE Characters ADD COLUMN ng_plus_completions INTEGER DEFAULT 0;

-- ==============================================================================
-- SECTION 2: NEW GAME+ CARRYOVER SNAPSHOTS TABLE
-- ==============================================================================

CREATE TABLE IF NOT EXISTS NG_Plus_Carryover (
    carryover_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    ng_plus_tier INTEGER NOT NULL,
    snapshot_timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Carryover data (stored as JSON for flexibility)
    character_data TEXT NOT NULL, -- Level, Legend, PP, attributes (JSON)
    specialization_data TEXT, -- Unlocked specs, abilities (JSON)
    equipment_data TEXT, -- Inventory snapshot (JSON)
    crafting_data TEXT, -- Materials, recipes (JSON)
    currency_data TEXT, -- Scrap and other currencies (JSON)

    -- Pre-reset data for comparison and debugging
    quest_state_snapshot TEXT, -- Quest IDs and progress (JSON, for reset verification)
    world_state_snapshot TEXT, -- Sector completion (JSON, for reset verification)

    -- Constraints
    CHECK (ng_plus_tier >= 1 AND ng_plus_tier <= 5)
);

-- Indexes for efficient queries
CREATE INDEX IF NOT EXISTS idx_ng_carryover_character ON NG_Plus_Carryover(character_id);
CREATE INDEX IF NOT EXISTS idx_ng_carryover_tier ON NG_Plus_Carryover(ng_plus_tier);
CREATE INDEX IF NOT EXISTS idx_ng_carryover_timestamp ON NG_Plus_Carryover(snapshot_timestamp);

-- ==============================================================================
-- SECTION 3: NEW GAME+ DIFFICULTY SCALING TABLE
-- ==============================================================================

CREATE TABLE IF NOT EXISTS NG_Plus_Scaling (
    tier INTEGER PRIMARY KEY,
    difficulty_multiplier REAL NOT NULL,
    enemy_level_increase INTEGER NOT NULL,
    boss_phase_threshold_reduction REAL NOT NULL,
    corruption_rate_multiplier REAL NOT NULL,
    legend_reward_multiplier REAL NOT NULL,
    description TEXT,

    -- Constraints
    CHECK (tier >= 1 AND tier <= 5),
    CHECK (difficulty_multiplier >= 1.0),
    CHECK (enemy_level_increase >= 0),
    CHECK (boss_phase_threshold_reduction >= 0.0 AND boss_phase_threshold_reduction <= 1.0),
    CHECK (corruption_rate_multiplier >= 1.0),
    CHECK (legend_reward_multiplier >= 1.0)
);

-- ==============================================================================
-- SECTION 4: SEED INITIAL SCALING VALUES
-- ==============================================================================

-- Insert NG+ tier scaling data
INSERT OR REPLACE INTO NG_Plus_Scaling
    (tier, difficulty_multiplier, enemy_level_increase, boss_phase_threshold_reduction,
     corruption_rate_multiplier, legend_reward_multiplier, description)
VALUES
    (1, 1.5, 2, 0.10, 1.25, 1.15, 'NG+1: First step into higher difficulty'),
    (2, 2.0, 4, 0.20, 1.50, 1.30, 'NG+2: Significantly enhanced enemies'),
    (3, 2.5, 6, 0.30, 1.75, 1.45, 'NG+3: Extreme challenge for optimized builds'),
    (4, 3.0, 8, 0.40, 2.00, 1.60, 'NG+4: Near-impossible without perfect execution'),
    (5, 3.5, 10, 0.50, 2.25, 1.75, 'NG+5: Maximum difficulty, ultimate mastery test');

-- ==============================================================================
-- SECTION 5: NEW GAME+ COMPLETION LOG TABLE
-- ==============================================================================

CREATE TABLE IF NOT EXISTS NG_Plus_Completions (
    completion_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    completed_tier INTEGER NOT NULL,
    completion_timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    total_playtime_seconds INTEGER,
    deaths_during_run INTEGER DEFAULT 0,
    bosses_defeated INTEGER DEFAULT 0,

    -- Constraints
    CHECK (completed_tier >= 1 AND completed_tier <= 5),
    CHECK (total_playtime_seconds >= 0),
    CHECK (deaths_during_run >= 0),
    CHECK (bosses_defeated >= 0)
);

-- Indexes for efficient queries
CREATE INDEX IF NOT EXISTS idx_ng_completions_character ON NG_Plus_Completions(character_id);
CREATE INDEX IF NOT EXISTS idx_ng_completions_tier ON NG_Plus_Completions(completed_tier);
CREATE INDEX IF NOT EXISTS idx_ng_completions_timestamp ON NG_Plus_Completions(completion_timestamp);

-- ==============================================================================
-- SECTION 6: VERIFICATION QUERIES
-- ==============================================================================

-- Verify tables were created successfully
SELECT 'v0.40.1: New Game+ Schema Created Successfully' AS status;

-- Show NG+ scaling tiers
SELECT '=== NG+ Difficulty Scaling Tiers ===' AS section;
SELECT
    tier AS 'NG+ Tier',
    difficulty_multiplier AS 'Enemy HP/Dmg',
    enemy_level_increase AS 'Level +',
    (boss_phase_threshold_reduction * 100) || '%' AS 'Phase Trigger',
    corruption_rate_multiplier AS 'Corruption Rate',
    legend_reward_multiplier AS 'Legend Bonus',
    description AS 'Description'
FROM NG_Plus_Scaling
ORDER BY tier;

-- Show table counts
SELECT '=== Table Status ===' AS section;
SELECT 'NG_Plus_Carryover entries: ' || COUNT(*) AS status FROM NG_Plus_Carryover;
SELECT 'NG_Plus_Scaling entries: ' || COUNT(*) AS status FROM NG_Plus_Scaling;
SELECT 'NG_Plus_Completions entries: ' || COUNT(*) AS status FROM NG_Plus_Completions;
SELECT 'Schema_Migrations entries: ' || COUNT(*) AS status FROM Schema_Migrations;

-- ==============================================================================
-- SECTION 7: MIGRATION SCRIPT FOR EXISTING CHARACTERS TABLE
-- ==============================================================================
-- This section provides the SQL needed to add columns to an existing Characters table
-- Run this separately if the Characters table already exists

-- NOTE: Execute this manually if Characters table already exists:
/*
-- Add NG+ columns to existing Characters table
ALTER TABLE Characters ADD COLUMN current_ng_plus_tier INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN highest_ng_plus_tier INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN has_completed_campaign INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN ng_plus_completions INTEGER DEFAULT 0;

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_characters_ng_tier ON Characters(current_ng_plus_tier);
CREATE INDEX IF NOT EXISTS idx_characters_campaign_complete ON Characters(has_completed_campaign);

-- Verify columns added
PRAGMA table_info(Characters);
*/

-- ==============================================================================
-- END OF SCHEMA
-- ==============================================================================
