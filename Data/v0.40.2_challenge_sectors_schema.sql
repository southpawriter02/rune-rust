-- ==============================================================================
-- v0.40.2: Challenge Sectors - Database Schema
-- ==============================================================================
-- Purpose: Implement handcrafted extreme difficulty challenges with unique modifiers
-- Author: v0.40.2 Specification
-- Date: 2025-11-24
-- Dependencies: v0.40.1 (New Game+ System), v0.10-v0.12 (Dynamic Room Engine)
-- ==============================================================================

-- ==============================================================================
-- SECTION 1: CHALLENGE MODIFIERS TABLE
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Challenge_Modifiers (
    modifier_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    category TEXT NOT NULL, -- Combat, Resource, Environmental, Psychological, Restriction
    description TEXT NOT NULL,
    difficulty_multiplier REAL NOT NULL,

    -- Application parameters (JSON)
    parameters TEXT, -- JSON object with modifier-specific parameters

    -- Implementation reference
    application_logic TEXT, -- Service method name or implementation class

    -- Metadata
    is_active INTEGER DEFAULT 1, -- Boolean: can this modifier be used?
    sort_order INTEGER DEFAULT 0,

    CHECK (category IN ('Combat', 'Resource', 'Environmental', 'Psychological', 'Restriction')),
    CHECK (difficulty_multiplier >= 0.0)
);

CREATE INDEX IF NOT EXISTS idx_modifiers_category ON Challenge_Modifiers(category);
CREATE INDEX IF NOT EXISTS idx_modifiers_active ON Challenge_Modifiers(is_active);

-- ==============================================================================
-- SECTION 2: CHALLENGE SECTORS TABLE
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Challenge_Sectors (
    sector_id TEXT PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    description TEXT NOT NULL,
    lore_text TEXT,

    -- Difficulty
    total_difficulty_multiplier REAL NOT NULL,

    -- Generation parameters
    biome_theme TEXT NOT NULL, -- Biome type for procedural generation
    enemy_pool TEXT, -- JSON array of enemy types
    room_count INTEGER DEFAULT 10,

    -- Rewards
    unique_reward_id TEXT, -- Legendary reward item ID
    unique_reward_name TEXT,
    unique_reward_description TEXT,

    -- Progression
    required_ng_plus_tier INTEGER DEFAULT 0, -- Minimum NG+ tier to attempt
    prerequisite_sectors TEXT, -- JSON array of sector_ids that must be completed first

    -- Metadata
    is_active INTEGER DEFAULT 1,
    sort_order INTEGER DEFAULT 0,
    design_notes TEXT, -- Designer notes for balance/testing

    CHECK (total_difficulty_multiplier >= 1.0),
    CHECK (room_count >= 1 AND room_count <= 20),
    CHECK (required_ng_plus_tier >= 0 AND required_ng_plus_tier <= 5)
);

CREATE INDEX IF NOT EXISTS idx_sectors_difficulty ON Challenge_Sectors(total_difficulty_multiplier);
CREATE INDEX IF NOT EXISTS idx_sectors_active ON Challenge_Sectors(is_active);
CREATE INDEX IF NOT EXISTS idx_sectors_ng_tier ON Challenge_Sectors(required_ng_plus_tier);

-- ==============================================================================
-- SECTION 3: CHALLENGE SECTOR MODIFIERS (Junction Table)
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Challenge_Sector_Modifiers (
    sector_id TEXT NOT NULL,
    modifier_id TEXT NOT NULL,
    application_order INTEGER DEFAULT 0, -- Order to apply modifiers (some may depend on others)

    PRIMARY KEY (sector_id, modifier_id),
    FOREIGN KEY (sector_id) REFERENCES Challenge_Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (modifier_id) REFERENCES Challenge_Modifiers(modifier_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_sector_modifiers_sector ON Challenge_Sector_Modifiers(sector_id);
CREATE INDEX IF NOT EXISTS idx_sector_modifiers_modifier ON Challenge_Sector_Modifiers(modifier_id);

-- ==============================================================================
-- SECTION 4: CHALLENGE COMPLETIONS TABLE
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Challenge_Completions (
    completion_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    sector_id TEXT NOT NULL,

    -- Completion metadata
    completed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    completion_time_seconds INTEGER, -- How long the run took
    deaths_during_run INTEGER DEFAULT 0,

    -- Run statistics
    damage_taken INTEGER DEFAULT 0,
    damage_dealt INTEGER DEFAULT 0,
    enemies_killed INTEGER DEFAULT 0,

    -- NG+ context
    ng_plus_tier INTEGER DEFAULT 0, -- Which NG+ tier was active

    -- First completion flag
    is_first_completion INTEGER DEFAULT 0, -- Boolean: was this the first time completing this sector?

    FOREIGN KEY (sector_id) REFERENCES Challenge_Sectors(sector_id) ON DELETE CASCADE,

    CHECK (completion_time_seconds >= 0),
    CHECK (deaths_during_run >= 0),
    CHECK (ng_plus_tier >= 0 AND ng_plus_tier <= 5)
);

CREATE INDEX IF NOT EXISTS idx_completions_character ON Challenge_Completions(character_id);
CREATE INDEX IF NOT EXISTS idx_completions_sector ON Challenge_Completions(sector_id);
CREATE INDEX IF NOT EXISTS idx_completions_timestamp ON Challenge_Completions(completed_at);
CREATE INDEX IF NOT EXISTS idx_completions_first ON Challenge_Completions(is_first_completion);

-- ==============================================================================
-- SECTION 5: CHALLENGE PROGRESS TRACKING TABLE
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Challenge_Progress (
    character_id INTEGER PRIMARY KEY,

    -- Overall progress
    total_sectors_completed INTEGER DEFAULT 0,
    total_attempts INTEGER DEFAULT 0,

    -- Fastest completions
    fastest_sector_id TEXT,
    fastest_completion_time INTEGER,

    -- Statistics
    total_challenge_time_seconds INTEGER DEFAULT 0,
    total_deaths_in_challenges INTEGER DEFAULT 0,

    -- Achievements
    all_sectors_completed INTEGER DEFAULT 0, -- Boolean
    perfect_run_count INTEGER DEFAULT 0, -- No deaths

    -- Last updated
    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,

    CHECK (total_sectors_completed >= 0),
    CHECK (total_attempts >= 0)
);

CREATE INDEX IF NOT EXISTS idx_progress_completed ON Challenge_Progress(total_sectors_completed);

-- ==============================================================================
-- SECTION 6: VERIFICATION QUERIES
-- ==============================================================================

-- Verify tables were created successfully
SELECT 'v0.40.2: Challenge Sectors Schema Created Successfully' AS status;

-- Show table counts
SELECT '=== Challenge System Tables ===' AS section;
SELECT 'Challenge_Modifiers: ' || COUNT(*) AS status FROM Challenge_Modifiers;
SELECT 'Challenge_Sectors: ' || COUNT(*) AS status FROM Challenge_Sectors;
SELECT 'Challenge_Sector_Modifiers: ' || COUNT(*) AS status FROM Challenge_Sector_Modifiers;
SELECT 'Challenge_Completions: ' || COUNT(*) AS status FROM Challenge_Completions;
SELECT 'Challenge_Progress: ' || COUNT(*) AS status FROM Challenge_Progress;

-- ==============================================================================
-- END OF SCHEMA
-- ==============================================================================
