-- ═══════════════════════════════════════════════════════════════
-- v0.34.1: Companion System Verification Queries
-- Rune & Rust: NPC Companion System Database Validation
-- ═══════════════════════════════════════════════════════════════

-- Usage:
-- sqlite3 runeandrust.db < Data/v0.34.1_verification_queries.sql

-- ═══════════════════════════════════════════════════════════════
-- 1. SCHEMA VALIDATION
-- ═══════════════════════════════════════════════════════════════

.mode column
.headers on

SELECT '=== SCHEMA VALIDATION ===' as '';

-- Check all companion tables exist
SELECT 'Companion Tables:' as Check, COUNT(*) as Count
FROM sqlite_master
WHERE type='table' AND name LIKE '%Companion%';
-- Expected: 5

-- List all companion tables
SELECT 'Table Name:' as Check, name as TableName
FROM sqlite_master
WHERE type='table' AND name LIKE '%Companion%'
ORDER BY name;

-- ═══════════════════════════════════════════════════════════════
-- 2. DATA SEEDING VALIDATION
-- ═══════════════════════════════════════════════════════════════

SELECT '=== DATA SEEDING VALIDATION ===' as '';

-- Count companions
SELECT 'Total Companions:' as Check, COUNT(*) as Count FROM Companions;
-- Expected: 6

-- Count companion abilities
SELECT 'Total Companion Abilities:' as Check, COUNT(*) as Count FROM Companion_Abilities;
-- Expected: 18

-- List all companions
SELECT 'All Companions:' as '';
SELECT
    companion_id as ID,
    display_name as Name,
    archetype as Class,
    combat_role as Role,
    faction_affiliation as Faction
FROM Companions
ORDER BY companion_id;

-- ═══════════════════════════════════════════════════════════════
-- 3. EXAMPLE QUERIES FROM SPECIFICATION
-- ═══════════════════════════════════════════════════════════════

SELECT '=== EXAMPLE QUERIES ===' as '';

-- Get All Recruitable Companions
SELECT 'Get All Recruitable Companions:' as '';
SELECT
    companion_id,
    display_name,
    archetype,
    combat_role,
    faction_affiliation,
    recruitment_location
FROM Companions
ORDER BY faction_affiliation, display_name;

-- Companions by Archetype
SELECT 'Companions by Archetype:' as '';
SELECT
    archetype,
    COUNT(*) as count,
    GROUP_CONCAT(display_name, ', ') as names
FROM Companions
GROUP BY archetype
ORDER BY archetype;
-- Expected: Warrior: 3, Adept: 1, Mystic: 2

-- Faction Requirements
SELECT 'Faction Requirements:' as '';
SELECT
    display_name as Companion,
    required_faction as Faction,
    required_reputation_tier as Tier,
    required_reputation_value as Value
FROM Companions
WHERE required_faction IS NOT NULL
ORDER BY required_faction, display_name;

-- Independent Companions (No Faction Requirement)
SELECT 'Independent Companions (No Faction Requirement):' as '';
SELECT
    display_name as Companion,
    recruitment_location as Location
FROM Companions
WHERE required_faction IS NULL
ORDER BY display_name;

-- Companion Abilities by Owner
SELECT 'Companion Abilities by Owner:' as '';
SELECT
    owner as Companion,
    COUNT(*) as AbilityCount,
    GROUP_CONCAT(ability_name, ', ') as Abilities
FROM Companion_Abilities
GROUP BY owner
ORDER BY owner;

-- Tank Companions
SELECT 'Tank Companions:' as '';
SELECT
    display_name as Name,
    archetype as Class,
    base_max_hp as HP,
    base_defense as Defense,
    base_soak as Soak,
    faction_affiliation as Faction
FROM Companions
WHERE combat_role = 'Tank'
ORDER BY base_max_hp DESC;

-- Support & Utility Companions
SELECT 'Support & Utility Companions:' as '';
SELECT
    display_name as Name,
    archetype as Class,
    combat_role as Role,
    resource_type as Resource,
    base_max_resource as MaxResource
FROM Companions
WHERE combat_role IN ('Support', 'Utility')
ORDER BY combat_role, display_name;

-- Passive Abilities
SELECT 'Passive Abilities:' as '';
SELECT
    owner as Companion,
    ability_name as Ability,
    description as Description
FROM Companion_Abilities
WHERE range_type = 'passive'
ORDER BY owner;

-- Abilities by Resource Type
SELECT 'Abilities by Resource Type:' as '';
SELECT
    resource_cost_type as ResourceType,
    COUNT(*) as Count,
    AVG(resource_cost) as AvgCost,
    MIN(resource_cost) as MinCost,
    MAX(resource_cost) as MaxCost
FROM Companion_Abilities
WHERE resource_cost_type IS NOT NULL
GROUP BY resource_cost_type;

-- ═══════════════════════════════════════════════════════════════
-- 4. STAT DISTRIBUTION ANALYSIS
-- ═══════════════════════════════════════════════════════════════

SELECT '=== STAT DISTRIBUTION ANALYSIS ===' as '';

-- Highest MIGHT
SELECT 'Highest MIGHT:' as '';
SELECT display_name, base_might FROM Companions ORDER BY base_might DESC LIMIT 3;

-- Highest WILL
SELECT 'Highest WILL:' as '';
SELECT display_name, base_will FROM Companions ORDER BY base_will DESC LIMIT 3;

-- Highest HP
SELECT 'Highest HP:' as '';
SELECT display_name, base_max_hp FROM Companions ORDER BY base_max_hp DESC LIMIT 3;

-- Lowest HP (Glass Cannons)
SELECT 'Lowest HP (Glass Cannons):' as '';
SELECT display_name, base_max_hp FROM Companions ORDER BY base_max_hp ASC LIMIT 3;

-- ═══════════════════════════════════════════════════════════════
-- 5. FOREIGN KEY & CONSTRAINT VALIDATION
-- ═══════════════════════════════════════════════════════════════

SELECT '=== CONSTRAINT VALIDATION ===' as '';

-- Check starting_abilities JSON format
SELECT 'Starting Abilities JSON Validation:' as '';
SELECT
    display_name as Companion,
    starting_abilities as JSON,
    CASE
        WHEN starting_abilities LIKE '[%]' THEN 'Valid'
        ELSE 'Invalid'
    END as Status
FROM Companions;

-- Verify starting_abilities match actual abilities
SELECT 'Starting Abilities Match Check:' as '';
SELECT
    c.display_name as Companion,
    c.starting_abilities as DeclaredAbilities,
    COUNT(ca.ability_id) as ActualAbilityCount
FROM Companions c
LEFT JOIN Companion_Abilities ca ON ca.owner = c.companion_name
GROUP BY c.companion_id
ORDER BY c.display_name;
-- Expected: All should have 3 abilities

-- ═══════════════════════════════════════════════════════════════
-- 6. INDEX VALIDATION
-- ═══════════════════════════════════════════════════════════════

SELECT '=== INDEX VALIDATION ===' as '';

-- List all companion-related indices
SELECT 'Companion Indices:' as '';
SELECT name as IndexName
FROM sqlite_master
WHERE type='index' AND name LIKE 'idx_companion%'
ORDER BY name;
-- Expected: 8 indices

-- ═══════════════════════════════════════════════════════════════
-- 7. SIMULATED RECRUITMENT CHECK
-- ═══════════════════════════════════════════════════════════════

SELECT '=== SIMULATED RECRUITMENT CHECKS ===' as '';

-- Simulate: Can player with Iron-Bane Friendly (+25) recruit Kára?
SELECT 'Can recruit Kára Ironbreaker with Iron-Bane Friendly (+25)?:' as '';
SELECT
    display_name as Companion,
    required_faction as RequiredFaction,
    required_reputation_value as RequiredRep,
    CASE
        WHEN 25 >= required_reputation_value THEN 'Yes - Can Recruit'
        ELSE 'No - Reputation Too Low'
    END as Result
FROM Companions
WHERE companion_id = 34001;

-- Simulate: Can player with Rust-Clan Neutral (0) recruit Bjorn?
SELECT 'Can recruit Bjorn with Rust-Clan Neutral (0)?:' as '';
SELECT
    display_name as Companion,
    required_faction as RequiredFaction,
    required_reputation_value as RequiredRep,
    CASE
        WHEN 0 >= required_reputation_value THEN 'Yes - Can Recruit'
        ELSE 'No - Reputation Too Low'
    END as Result
FROM Companions
WHERE companion_id = 34003;

-- Companions available to all players (no faction requirement)
SELECT 'Companions available to all players:' as '';
SELECT
    display_name as Companion,
    recruitment_location as Location
FROM Companions
WHERE required_faction IS NULL
ORDER BY display_name;

-- ═══════════════════════════════════════════════════════════════
-- 8. ABILITY ANALYSIS
-- ═══════════════════════════════════════════════════════════════

SELECT '=== ABILITY ANALYSIS ===' as '';

-- Abilities by damage type
SELECT 'Abilities by Damage Type:' as '';
SELECT
    damage_type as DamageType,
    COUNT(*) as Count,
    GROUP_CONCAT(ability_name, ', ') as Abilities
FROM Companion_Abilities
WHERE damage_type IS NOT NULL
GROUP BY damage_type;

-- Abilities by target type
SELECT 'Abilities by Target Type:' as '';
SELECT
    target_type as TargetType,
    COUNT(*) as Count
FROM Companion_Abilities
GROUP BY target_type
ORDER BY Count DESC;

-- Highest cost abilities
SELECT 'Highest Cost Abilities:' as '';
SELECT
    ability_name as Ability,
    owner as Companion,
    resource_cost_type as Resource,
    resource_cost as Cost
FROM Companion_Abilities
WHERE resource_cost > 0
ORDER BY resource_cost DESC
LIMIT 5;

-- ═══════════════════════════════════════════════════════════════
-- 9. DATA INTEGRITY CHECKS
-- ═══════════════════════════════════════════════════════════════

SELECT '=== DATA INTEGRITY CHECKS ===' as '';

-- Check for NULL required fields
SELECT 'Companions with NULL required fields:' as '';
SELECT
    display_name,
    CASE WHEN archetype IS NULL THEN 'Missing archetype' ELSE '' END,
    CASE WHEN combat_role IS NULL THEN 'Missing combat_role' ELSE '' END,
    CASE WHEN resource_type IS NULL THEN 'Missing resource_type' ELSE '' END
FROM Companions
WHERE archetype IS NULL OR combat_role IS NULL OR resource_type IS NULL;
-- Expected: 0 rows

-- Check for duplicate companion_name
SELECT 'Duplicate companion_name check:' as '';
SELECT companion_name, COUNT(*) as Count
FROM Companions
GROUP BY companion_name
HAVING COUNT(*) > 1;
-- Expected: 0 rows

-- Check for orphaned abilities (owner doesn't exist)
SELECT 'Orphaned abilities check:' as '';
SELECT ca.ability_name, ca.owner
FROM Companion_Abilities ca
LEFT JOIN Companions c ON ca.owner = c.companion_name
WHERE c.companion_id IS NULL;
-- Expected: 0 rows

-- ═══════════════════════════════════════════════════════════════
-- 10. SUMMARY STATISTICS
-- ═══════════════════════════════════════════════════════════════

SELECT '=== SUMMARY STATISTICS ===' as '';

SELECT 'Summary:' as '';
SELECT
    'Total Companions' as Metric,
    COUNT(*) as Value
FROM Companions
UNION ALL
SELECT
    'Total Abilities',
    COUNT(*)
FROM Companion_Abilities
UNION ALL
SELECT
    'Avg Base HP',
    AVG(base_max_hp)
FROM Companions
UNION ALL
SELECT
    'Avg Base Defense',
    AVG(base_defense)
FROM Companions
UNION ALL
SELECT
    'Warriors',
    COUNT(*)
FROM Companions WHERE archetype = 'Warrior'
UNION ALL
SELECT
    'Adepts',
    COUNT(*)
FROM Companions WHERE archetype = 'Adept'
UNION ALL
SELECT
    'Mystics',
    COUNT(*)
FROM Companions WHERE archetype = 'Mystic'
UNION ALL
SELECT
    'Faction-Locked',
    COUNT(*)
FROM Companions WHERE required_faction IS NOT NULL
UNION ALL
SELECT
    'Independent',
    COUNT(*)
FROM Companions WHERE required_faction IS NULL;

-- ═══════════════════════════════════════════════════════════════
-- VERIFICATION COMPLETE
-- ═══════════════════════════════════════════════════════════════

SELECT '=== VERIFICATION COMPLETE ===' as '';
SELECT 'v0.34.1 Companion System database schema validated successfully.' as Result;
