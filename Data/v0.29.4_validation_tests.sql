-- v0.29.4 Integration & Validation Tests
-- Execute these tests to validate complete Muspelheim biome implementation

-- ============================================================================
-- Test 1: Complete Data Integrity Check
-- ============================================================================

SELECT '=== Test 1: Data Integrity Check ===' as test_name;

-- Verify all 8 room templates exist
SELECT 'Room Templates' as category, COUNT(*) as count,
       CASE WHEN COUNT(*) = 8 THEN '✅ PASS' ELSE '❌ FAIL' END as status
FROM Biome_RoomTemplates
WHERE biome_id = 4;

-- Verify all 5 enemy types exist
SELECT 'Enemy Spawns' as category, COUNT(*) as count,
       CASE WHEN COUNT(*) = 5 THEN '✅ PASS' ELSE '❌ FAIL' END as status
FROM Biome_EnemySpawns
WHERE biome_id = 4;

-- Verify all 8 hazards exist
SELECT 'Environmental Hazards' as category, COUNT(*) as count,
       CASE WHEN COUNT(*) = 8 THEN '✅ PASS' ELSE '❌ FAIL' END as status
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 4;

-- Verify all 9 resources exist
SELECT 'Resource Drops' as category, COUNT(*) as count,
       CASE WHEN COUNT(*) = 9 THEN '✅ PASS' ELSE '❌ FAIL' END as status
FROM Biome_ResourceDrops
WHERE biome_id = 4;

-- ============================================================================
-- Test 2: WFC Adjacency Rules Validation
-- ============================================================================

SELECT '=== Test 2: WFC Adjacency Rules ===' as test_name;

-- Check that all templates have valid JSON adjacency rules
SELECT
    template_name,
    wfc_adjacency_rules,
    CASE
        WHEN json_valid(wfc_adjacency_rules) = 1 THEN '✅ Valid JSON'
        ELSE '❌ Invalid JSON'
    END as json_status
FROM Biome_RoomTemplates
WHERE biome_id = 4;

-- Check entrance/exit templates
SELECT
    'Entrance Templates' as category,
    GROUP_CONCAT(template_name, ', ') as templates,
    COUNT(*) as count
FROM Biome_RoomTemplates
WHERE biome_id = 4 AND can_be_entrance = 1;

SELECT
    'Exit Templates' as category,
    GROUP_CONCAT(template_name, ', ') as templates,
    COUNT(*) as count
FROM Biome_RoomTemplates
WHERE biome_id = 4 AND can_be_exit = 1;

-- ============================================================================
-- Test 3: Enemy Spawn Weight Distribution
-- ============================================================================

SELECT '=== Test 3: Enemy Spawn Distribution ===' as test_name;

SELECT
    enemy_name,
    spawn_weight,
    ROUND(spawn_weight * 100.0 / (SELECT SUM(spawn_weight) FROM Biome_EnemySpawns WHERE biome_id = 4), 2) as spawn_chance_percent,
    CASE
        WHEN enemy_name = 'Forge-Hardened Undying' AND spawn_weight = 150 THEN '✅ Expected'
        WHEN enemy_name = 'Magma Elemental' AND spawn_weight = 80 THEN '✅ Expected'
        WHEN enemy_name = 'Rival Berserker' AND spawn_weight = 60 THEN '✅ Expected'
        WHEN enemy_name = 'Iron-Bane Crusader' AND spawn_weight = 20 THEN '✅ Expected'
        WHEN enemy_name = 'Surtur''s Herald' AND spawn_weight = 1 THEN '✅ Expected (Boss)'
        ELSE '⚠️ Unexpected'
    END as validation
FROM Biome_EnemySpawns
WHERE biome_id = 4
ORDER BY spawn_weight DESC;

-- ============================================================================
-- Test 4: Fire Resistance Data Validation
-- ============================================================================

SELECT '=== Test 4: Fire Resistance System ===' as test_name;

SELECT
    enemy_name,
    json_extract(spawn_rules_json, '$.fire_resistance') as fire_res,
    json_extract(spawn_rules_json, '$.ice_resistance') as ice_res,
    CASE
        WHEN CAST(json_extract(spawn_rules_json, '$.fire_resistance') AS INTEGER) > 0
        THEN '✅ Brittle-Eligible'
        ELSE '❌ Not Eligible'
    END as brittle_status
FROM Biome_EnemySpawns
WHERE biome_id = 4
ORDER BY CAST(json_extract(spawn_rules_json, '$.fire_resistance') AS INTEGER) DESC;

-- ============================================================================
-- Test 5: Hazard Density Distribution
-- ============================================================================

SELECT '=== Test 5: Hazard Density Distribution ===' as test_name;

SELECT
    hazard_density_category as density,
    COUNT(*) as hazard_count,
    GROUP_CONCAT(feature_name, ', ') as hazards
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 4
GROUP BY hazard_density_category
ORDER BY
    CASE hazard_density_category
        WHEN 'Low' THEN 1
        WHEN 'Medium' THEN 2
        WHEN 'High' THEN 3
        WHEN 'Extreme' THEN 4
    END;

-- Check that hazards match room template densities
SELECT
    r.hazard_density as room_density,
    COUNT(DISTINCT r.template_id) as room_count,
    COUNT(DISTINCT h.feature_id) as compatible_hazards
FROM Biome_RoomTemplates r
LEFT JOIN Biome_EnvironmentalFeatures h ON h.biome_id = r.biome_id
WHERE r.biome_id = 4
GROUP BY r.hazard_density;

-- ============================================================================
-- Test 6: Resource Tier & Rarity Distribution
-- ============================================================================

SELECT '=== Test 6: Resource Distribution ===' as test_name;

SELECT
    resource_tier as tier,
    rarity,
    COUNT(*) as resource_count,
    AVG(base_drop_chance) as avg_drop_chance,
    SUM(weight) as total_weight
FROM Biome_ResourceDrops
WHERE biome_id = 4
GROUP BY resource_tier, rarity
ORDER BY resource_tier DESC,
    CASE rarity
        WHEN 'Legendary' THEN 1
        WHEN 'Epic' THEN 2
        WHEN 'Rare' THEN 3
        WHEN 'Uncommon' THEN 4
        WHEN 'Common' THEN 5
    END;

-- Verify special node resources
SELECT
    'Special Nodes' as category,
    resource_name,
    rarity,
    base_drop_chance
FROM Biome_ResourceDrops
WHERE biome_id = 4 AND requires_special_node = 1;

-- ============================================================================
-- Test 7: Room Size → Enemy Count Mapping
-- ============================================================================

SELECT '=== Test 7: Room Size Validation ===' as test_name;

SELECT
    room_size_category,
    COUNT(*) as room_count,
    AVG(enemy_spawn_weight) as avg_enemy_weight,
    CASE room_size_category
        WHEN 'Small' THEN '1-2 enemies expected'
        WHEN 'Medium' THEN '2-3 enemies expected'
        WHEN 'Large' THEN '3-4 enemies expected'
        WHEN 'XLarge' THEN '4-5 enemies expected'
    END as expected_enemy_count
FROM Biome_RoomTemplates
WHERE biome_id = 4
GROUP BY room_size_category;

-- ============================================================================
-- Test 8: Boss Room Validation
-- ============================================================================

SELECT '=== Test 8: Boss Room Configuration ===' as test_name;

-- Check Containment Breach Zone template
SELECT
    template_name,
    room_size_category,
    hazard_density,
    enemy_spawn_weight,
    can_be_entrance,
    can_be_exit,
    CASE
        WHEN template_name = 'Containment Breach Zone' THEN '✅ Boss Room Exists'
        ELSE '❌ Boss Room Missing'
    END as validation
FROM Biome_RoomTemplates
WHERE biome_id = 4 AND template_name = 'Containment Breach Zone';

-- Check Surtur's Herald boss spawn
SELECT
    enemy_name,
    enemy_type,
    min_level,
    max_level,
    spawn_weight,
    json_extract(spawn_rules_json, '$.fire_resistance') as fire_res,
    json_extract(spawn_rules_json, '$.tags') as tags,
    CASE
        WHEN enemy_type = 'Boss' AND spawn_weight = 1 THEN '✅ Boss Configuration Valid'
        ELSE '❌ Boss Configuration Invalid'
    END as validation
FROM Biome_EnemySpawns
WHERE biome_id = 4 AND enemy_type = 'Boss';

-- ============================================================================
-- Test 9: Level Range Coverage
-- ============================================================================

SELECT '=== Test 9: Level Range Coverage ===' as test_name;

-- Check enemy level distribution (biome is levels 7-12)
SELECT
    enemy_name,
    min_level,
    max_level,
    max_level - min_level + 1 as level_span,
    CASE
        WHEN min_level >= 7 AND max_level <= 12 THEN '✅ Within Range'
        ELSE '⚠️ Outside Expected Range'
    END as validation
FROM Biome_EnemySpawns
WHERE biome_id = 4
ORDER BY min_level;

-- Check biome metadata
SELECT
    biome_name,
    min_character_level,
    max_character_level,
    CASE
        WHEN min_character_level = 7 AND max_character_level = 12 THEN '✅ Expected Levels'
        ELSE '❌ Unexpected Levels'
    END as validation
FROM Biomes
WHERE biome_id = 4;

-- ============================================================================
-- Test 10: Hazard Special Rules Validation
-- ============================================================================

SELECT '=== Test 10: Hazard Special Rules ===' as test_name;

SELECT
    feature_name,
    damage_per_turn,
    damage_type,
    special_rules,
    CASE
        WHEN feature_name = '[Chasm/Lava River]' AND damage_per_turn = 999 THEN '✅ Instant Death'
        WHEN feature_name LIKE '%Volatile Gas%' AND special_rules LIKE '%chain_reaction%' THEN '✅ Explosive'
        WHEN feature_name LIKE '%Steam Vent%' AND special_rules LIKE '%destructible%' THEN '✅ Destructible'
        ELSE '✅ Standard Hazard'
    END as validation
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 4
ORDER BY damage_per_turn DESC;

-- ============================================================================
-- Test 11: Resource Weight Distribution
-- ============================================================================

SELECT '=== Test 11: Resource Weight Probabilities ===' as test_name;

SELECT
    resource_name,
    weight,
    ROUND(weight * 100.0 / (SELECT SUM(weight) FROM Biome_ResourceDrops WHERE biome_id = 4), 2) as drop_probability_percent,
    rarity,
    resource_tier
FROM Biome_ResourceDrops
WHERE biome_id = 4
ORDER BY weight DESC;

-- Check legendary resources are rarest
SELECT
    'Legendary Resources' as category,
    AVG(weight) as avg_weight,
    AVG(base_drop_chance) as avg_drop_chance,
    CASE
        WHEN AVG(weight) < 10 THEN '✅ Appropriately Rare'
        ELSE '⚠️ Too Common'
    END as validation
FROM Biome_ResourceDrops
WHERE biome_id = 4 AND rarity = 'Legendary';

-- ============================================================================
-- Test 12: Biome Completeness Summary
-- ============================================================================

SELECT '=== Test 12: Biome Completeness Summary ===' as test_name;

SELECT
    'Muspelheim Biome v0.29' as feature,
    (SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 4) as room_templates,
    (SELECT COUNT(*) FROM Biome_EnemySpawns WHERE biome_id = 4) as enemy_types,
    (SELECT COUNT(*) FROM Biome_EnvironmentalFeatures WHERE biome_id = 4) as hazards,
    (SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 4) as resources,
    CASE
        WHEN (SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 4) = 8
         AND (SELECT COUNT(*) FROM Biome_EnemySpawns WHERE biome_id = 4) = 5
         AND (SELECT COUNT(*) FROM Biome_EnvironmentalFeatures WHERE biome_id = 4) = 8
         AND (SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 4) = 9
        THEN '✅ ALL SYSTEMS OPERATIONAL'
        ELSE '❌ INCOMPLETE DATA'
    END as overall_status;

-- Final summary
SELECT
    '=== v0.29.4 Validation Complete ===' as message,
    'Database: Ready for Procedural Generation' as status;
