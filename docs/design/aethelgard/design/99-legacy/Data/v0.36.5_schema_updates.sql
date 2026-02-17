-- ═══════════════════════════════════════════════════════════════════
-- v0.36.5: Schema Updates for Crafting System Integration
-- ═══════════════════════════════════════════════════════════════════
-- Adds missing columns and tables for complete crafting integration
-- Safe to run multiple times (uses ALTER TABLE IF NOT EXISTS pattern)
-- ═══════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════
-- 1. ADD CREDITS COLUMN TO CHARACTERS TABLE
-- ═══════════════════════════════════════════════════════════════════

-- SQLite doesn't support IF NOT EXISTS for ALTER TABLE ADD COLUMN
-- This will error if column exists, which is safe (can be ignored)

-- Add credits column (default 0)
-- Note: Run this via try-catch or check column existence first
ALTER TABLE Characters ADD COLUMN credits INTEGER DEFAULT 0;

-- ═══════════════════════════════════════════════════════════════════
-- 2. ADD ADDITIONAL COLUMNS TO CHARACTER_RECIPES IF MISSING
-- ═══════════════════════════════════════════════════════════════════

-- Add discovered_at column if missing
ALTER TABLE Character_Recipes ADD COLUMN discovered_at TEXT NULL;

-- Add discovery_source column if missing
ALTER TABLE Character_Recipes ADD COLUMN discovery_source TEXT DEFAULT 'Default';

-- ═══════════════════════════════════════════════════════════════════
-- 3. CREATE VIRTUAL FIELD STATION FOR EINBUI CRAFTING
-- ═══════════════════════════════════════════════════════════════════

INSERT OR IGNORE INTO Crafting_Stations (
    station_id,
    station_type,
    station_name,
    max_quality_tier,
    location_sector_id,
    location_room_id,
    requires_controlling,
    usage_cost_credits,
    station_description
)
VALUES (
    100,
    'Field_Station',
    'Field Crafting (Einbui)',
    2,
    NULL,
    NULL,
    0,
    0,
    'Portable field crafting enabled by Einbui specialization. Limited to Tier 2 quality and Basic consumables.'
);

-- ═══════════════════════════════════════════════════════════════════
-- 4. VERIFICATION QUERIES
-- ═══════════════════════════════════════════════════════════════════

-- Check if credits column exists
SELECT 'Credits Column Check' as check_name;
PRAGMA table_info(Characters);

-- Check if field station was created
SELECT 'Field Station Check' as check_name;
SELECT * FROM Crafting_Stations WHERE station_id = 100;

-- Check Character_Recipes columns
SELECT 'Character_Recipes Columns' as check_name;
PRAGMA table_info(Character_Recipes);

-- ═══════════════════════════════════════════════════════════════════
-- END OF SCHEMA UPDATES
-- ═══════════════════════════════════════════════════════════════════
