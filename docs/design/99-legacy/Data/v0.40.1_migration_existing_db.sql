-- ==============================================================================
-- v0.40.1: Migration Script for Existing Databases
-- ==============================================================================
-- Purpose: Add New Game+ columns to existing Characters table
-- Usage: Run this script on an existing runeandrust.db database
-- Date: 2025-11-24
-- ==============================================================================

-- ==============================================================================
-- SAFETY CHECK: Verify Characters table exists
-- ==============================================================================

SELECT CASE
    WHEN COUNT(*) > 0 THEN 'Characters table found - proceeding with migration'
    ELSE 'ERROR: Characters table not found!'
END AS migration_status
FROM sqlite_master
WHERE type='table' AND name='Characters';

-- ==============================================================================
-- STEP 1: Add New Game+ columns to Characters table
-- ==============================================================================

-- Add current_ng_plus_tier column (active NG+ tier: 0-5)
ALTER TABLE Characters ADD COLUMN current_ng_plus_tier INTEGER DEFAULT 0
    CHECK (current_ng_plus_tier >= 0 AND current_ng_plus_tier <= 5);

-- Add highest_ng_plus_tier column (highest tier ever completed: 0-5)
ALTER TABLE Characters ADD COLUMN highest_ng_plus_tier INTEGER DEFAULT 0
    CHECK (highest_ng_plus_tier >= 0 AND highest_ng_plus_tier <= 5);

-- Add has_completed_campaign column (boolean as INTEGER: 0=false, 1=true)
ALTER TABLE Characters ADD COLUMN has_completed_campaign INTEGER DEFAULT 0
    CHECK (has_completed_campaign IN (0, 1));

-- Add ng_plus_completions column (total number of NG+ runs completed)
ALTER TABLE Characters ADD COLUMN ng_plus_completions INTEGER DEFAULT 0
    CHECK (ng_plus_completions >= 0);

-- ==============================================================================
-- STEP 2: Create indexes for performance
-- ==============================================================================

CREATE INDEX IF NOT EXISTS idx_characters_ng_tier ON Characters(current_ng_plus_tier);
CREATE INDEX IF NOT EXISTS idx_characters_campaign_complete ON Characters(has_completed_campaign);
CREATE INDEX IF NOT EXISTS idx_characters_highest_tier ON Characters(highest_ng_plus_tier);

-- ==============================================================================
-- STEP 3: Verify column additions
-- ==============================================================================

-- Show updated Characters table schema
SELECT '=== Characters Table Schema (Updated) ===' AS status;
PRAGMA table_info(Characters);

-- Show columns we just added
SELECT '=== New Game+ Columns Added ===' AS status;
SELECT
    name AS column_name,
    type AS data_type,
    dflt_value AS default_value,
    pk AS is_primary_key
FROM pragma_table_info('Characters')
WHERE name LIKE '%ng_plus%' OR name LIKE '%campaign%';

-- ==============================================================================
-- STEP 4: Mark existing high-level characters as campaign complete
-- ==============================================================================

-- OPTIONAL: Automatically mark characters at milestone 10+ as having completed campaign
-- Comment out this section if you don't want automatic campaign completion

-- UPDATE Characters
-- SET has_completed_campaign = 1
-- WHERE current_milestone >= 10;

-- SELECT 'Marked ' || changes() || ' characters as campaign complete (milestone >= 10)' AS status;

-- ==============================================================================
-- STEP 5: Migration complete verification
-- ==============================================================================

SELECT '=== v0.40.1 Migration Complete ===' AS status;
SELECT 'Characters with NG+ columns: ' || COUNT(*) AS verification
FROM Characters;

SELECT 'Characters marked as campaign complete: ' || COUNT(*) AS verification
FROM Characters
WHERE has_completed_campaign = 1;

-- ==============================================================================
-- END OF MIGRATION SCRIPT
-- ==============================================================================
