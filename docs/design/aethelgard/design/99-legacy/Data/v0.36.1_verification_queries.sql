-- ═══════════════════════════════════════════════════════════════════
-- v0.36.1: Crafting System - Verification Queries
-- ═══════════════════════════════════════════════════════════════════
-- Use these queries to verify the database migration was successful
-- ═══════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════
-- 1. VERIFY TABLES CREATED
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== VERIFY TABLES ===' AS check_name;

SELECT name, type
FROM sqlite_master
WHERE type='table'
AND (name LIKE '%Crafting%' OR name LIKE '%Recipe%' OR name LIKE '%Inscription%' OR name LIKE '%Modification%' OR name = 'Items' OR name = 'Character_Inventory')
ORDER BY name;

-- Expected: 10 tables (Items, Character_Inventory, Crafting_Recipes, Recipe_Components, Crafting_Stations, Character_Recipes, Equipment_Modifications, Runic_Inscriptions, Worlds, Sectors)

-- ═══════════════════════════════════════════════════════════════════
-- 2. VERIFY INDEXES CREATED
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== VERIFY INDEXES ===' AS check_name;

SELECT name, tbl_name
FROM sqlite_master
WHERE type='index'
AND (name LIKE '%recipe%' OR name LIKE '%station%' OR name LIKE '%inscription%' OR name LIKE '%modification%' OR name LIKE '%items%' OR name LIKE '%inventory%')
ORDER BY tbl_name, name;

-- Expected: 18+ indexes

-- ═══════════════════════════════════════════════════════════════════
-- 3. VERIFY COMPONENT ITEMS SEEDED
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== COMPONENT ITEMS BY TIER ===' AS check_name;

SELECT quality_tier, COUNT(*) as count
FROM Items
WHERE item_type = 'Component'
GROUP BY quality_tier
ORDER BY quality_tier;

-- Expected: Tier 1-5 components totaling 45+ items

SELECT '=== COMPONENT ITEMS BY CATEGORY ===' AS check_name;

SELECT
    CASE
        WHEN item_id BETWEEN 5000 AND 5999 THEN 'Weapon Components'
        WHEN item_id BETWEEN 6000 AND 6999 THEN 'Armor Components'
        WHEN item_id BETWEEN 7000 AND 7999 THEN 'Consumable Components'
        WHEN item_id BETWEEN 9000 AND 9999 THEN 'Runic Components'
        ELSE 'Other'
    END AS category,
    COUNT(*) as count
FROM Items
WHERE item_type = 'Component'
GROUP BY category
ORDER BY category;

-- Expected: 4 categories with components

-- ═══════════════════════════════════════════════════════════════════
-- 4. VERIFY CRAFTING RECIPES SEEDED
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== RECIPES BY TYPE AND TIER ===' AS check_name;

SELECT recipe_tier, crafted_item_type, COUNT(*) as count
FROM Crafting_Recipes
GROUP BY recipe_tier, crafted_item_type
ORDER BY crafted_item_type, recipe_tier;

-- Expected: 90 recipes (30 weapons, 30 armor, 30 consumables) across Basic, Advanced, Expert tiers

SELECT '=== RECIPES BY STATION ===' AS check_name;

SELECT required_station, COUNT(*) as count
FROM Crafting_Recipes
GROUP BY required_station
ORDER BY required_station;

-- Expected: Recipes distributed across Forge, Workshop, Laboratory, Field_Station

SELECT '=== RECIPES BY DISCOVERY METHOD ===' AS check_name;

SELECT discovery_method, COUNT(*) as count
FROM Crafting_Recipes
GROUP BY discovery_method
ORDER BY discovery_method;

-- Expected: Mix of Default, Merchant, Quest, Loot

-- ═══════════════════════════════════════════════════════════════════
-- 5. VERIFY RECIPE COMPONENTS
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== RECIPE COMPONENTS COUNT ===' AS check_name;

SELECT COUNT(*) as total_component_mappings
FROM Recipe_Components;

-- Expected: 250+ component mappings

SELECT '=== RECIPES WITH COMPONENT DETAILS (Sample) ===' AS check_name;

SELECT
    r.recipe_id,
    r.recipe_name,
    r.recipe_tier,
    COUNT(rc.component_id) as component_count
FROM Crafting_Recipes r
LEFT JOIN Recipe_Components rc ON r.recipe_id = rc.recipe_id
GROUP BY r.recipe_id, r.recipe_name, r.recipe_tier
HAVING component_count > 0
LIMIT 10;

-- Expected: All recipes have 1-4 components

-- ═══════════════════════════════════════════════════════════════════
-- 6. VERIFY RUNIC INSCRIPTIONS
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== INSCRIPTIONS BY TIER AND TYPE ===' AS check_name;

SELECT inscription_tier, target_equipment_type, COUNT(*) as count
FROM Runic_Inscriptions
GROUP BY inscription_tier, target_equipment_type
ORDER BY inscription_tier, target_equipment_type;

-- Expected: 22 inscriptions across tiers 3-5, for Weapon, Armor, and Both

SELECT '=== INSCRIPTIONS BY EFFECT TYPE ===' AS check_name;

SELECT effect_type, COUNT(*) as count
FROM Runic_Inscriptions
GROUP BY effect_type
ORDER BY effect_type;

-- Expected: Elemental, Damage_Bonus, Status, Resistance, Special

SELECT '=== TEMPORARY VS PERMANENT INSCRIPTIONS ===' AS check_name;

SELECT
    CASE WHEN is_temporary = 1 THEN 'Temporary' ELSE 'Permanent' END as inscription_type,
    COUNT(*) as count
FROM Runic_Inscriptions
GROUP BY is_temporary;

-- Expected: Mix of temporary and permanent inscriptions

-- ═══════════════════════════════════════════════════════════════════
-- 7. VERIFY CRAFTING STATIONS
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== STATIONS BY TYPE ===' AS check_name;

SELECT station_type, COUNT(*) as count
FROM Crafting_Stations
GROUP BY station_type
ORDER BY station_type;

-- Expected: 14 stations across 5 types (Forge, Workshop, Laboratory, Runic_Altar, Field_Station)

SELECT '=== STATIONS BY SECTOR ===' AS check_name;

SELECT
    COALESCE(s.sector_name, 'Portable') as location,
    cs.station_name,
    cs.station_type,
    cs.max_quality_tier
FROM Crafting_Stations cs
LEFT JOIN Sectors s ON cs.location_sector_id = s.sector_id
ORDER BY cs.location_sector_id, cs.station_id;

-- Expected: Stations distributed across Midgard, Muspelheim, Niflheim, Alfheim, Jotunheim, plus 1 portable

-- ═══════════════════════════════════════════════════════════════════
-- 8. DATA INTEGRITY CHECKS
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== CHECK ORPHANED RECIPE COMPONENTS ===' AS check_name;

SELECT COUNT(*) as orphaned_components
FROM Recipe_Components rc
WHERE NOT EXISTS (
    SELECT 1 FROM Crafting_Recipes r WHERE r.recipe_id = rc.recipe_id
)
OR NOT EXISTS (
    SELECT 1 FROM Items i WHERE i.item_id = rc.component_item_id
);

-- Expected: 0 (no orphaned records)

SELECT '=== CHECK RECIPE QUALITY BONUS CONSTRAINTS ===' AS check_name;

SELECT COUNT(*) as invalid_quality_bonus
FROM Crafting_Recipes
WHERE quality_bonus < 0 OR quality_bonus > 2;

-- Expected: 0 (all bonuses between 0-2)

SELECT '=== CHECK COMPONENT QUALITY CONSTRAINTS ===' AS check_name;

SELECT COUNT(*) as invalid_quality
FROM Recipe_Components
WHERE minimum_quality < 1 OR minimum_quality > 5;

-- Expected: 0 (all quality tiers between 1-5)

SELECT '=== CHECK STATION QUALITY TIER CONSTRAINTS ===' AS check_name;

SELECT COUNT(*) as invalid_station_quality
FROM Crafting_Stations
WHERE max_quality_tier < 1 OR max_quality_tier > 5;

-- Expected: 0 (all quality tiers between 1-5)

-- ═══════════════════════════════════════════════════════════════════
-- 9. SAMPLE QUERIES FOR SERVICE LAYER
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== SAMPLE: Get All Basic Recipes ===' AS check_name;

SELECT recipe_id, recipe_name, crafted_item_type, required_station
FROM Crafting_Recipes
WHERE recipe_tier = 'Basic'
ORDER BY crafted_item_type, recipe_name
LIMIT 10;

SELECT '=== SAMPLE: Get Recipe with Components ===' AS check_name;

-- Example: Plasma Rifle recipe (ID 1020)
SELECT
    r.recipe_name,
    r.recipe_tier,
    r.required_station,
    i.item_name as component_name,
    rc.quantity_required,
    rc.minimum_quality
FROM Crafting_Recipes r
JOIN Recipe_Components rc ON r.recipe_id = rc.recipe_id
JOIN Items i ON rc.component_item_id = i.item_id
WHERE r.recipe_id = 1020
ORDER BY rc.component_id;

SELECT '=== SAMPLE: Get Stations in Alfheim ===' AS check_name;

SELECT station_name, station_type, max_quality_tier, location_room_id
FROM Crafting_Stations
WHERE location_sector_id = 4
ORDER BY station_type;

SELECT '=== SAMPLE: Get Weapon Inscriptions (Tier 4+) ===' AS check_name;

SELECT inscription_name, inscription_tier, effect_type, is_temporary, crafting_cost_credits
FROM Runic_Inscriptions
WHERE target_equipment_type IN ('Weapon', 'Both')
AND inscription_tier >= 4
ORDER BY inscription_tier DESC, inscription_name;

-- ═══════════════════════════════════════════════════════════════════
-- 10. SUMMARY REPORT
-- ═══════════════════════════════════════════════════════════════════

SELECT '=== MIGRATION SUMMARY ===' AS check_name;

SELECT 'Total Items (Components)' as metric, COUNT(*) as count FROM Items WHERE item_type = 'Component'
UNION ALL
SELECT 'Total Recipes', COUNT(*) FROM Crafting_Recipes
UNION ALL
SELECT 'Total Recipe Components', COUNT(*) FROM Recipe_Components
UNION ALL
SELECT 'Total Runic Inscriptions', COUNT(*) FROM Runic_Inscriptions
UNION ALL
SELECT 'Total Crafting Stations', COUNT(*) FROM Crafting_Stations;

-- Expected Summary:
-- Components: 45+
-- Recipes: 90
-- Recipe Components: 250+
-- Inscriptions: 22
-- Stations: 14

-- ═══════════════════════════════════════════════════════════════════
-- END OF VERIFICATION QUERIES
-- ═══════════════════════════════════════════════════════════════════
