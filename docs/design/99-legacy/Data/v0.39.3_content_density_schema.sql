-- =====================================================
-- v0.39.3: Content Density & Population Budget Schema
-- =====================================================
-- Implements global population budgets preventing content over-saturation
-- Philosophy: Not every room needs combat - pacing through global density awareness
-- =====================================================

-- =====================================================
-- SECTOR POPULATION BUDGET
-- =====================================================

CREATE TABLE IF NOT EXISTS Sector_Population_Budget (
    budget_id INTEGER PRIMARY KEY AUTOINCREMENT,
    sector_id INTEGER NOT NULL,

    -- Global budgets
    total_enemy_budget INTEGER NOT NULL,
    total_hazard_budget INTEGER NOT NULL,
    total_loot_budget INTEGER NOT NULL,

    -- Actual spawned counts
    enemies_spawned INTEGER DEFAULT 0,
    hazards_spawned INTEGER DEFAULT 0,
    loot_spawned INTEGER DEFAULT 0,

    -- Metadata
    difficulty_tier TEXT,  -- Easy, Normal, Hard, Lethal
    biome_id TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CHECK (enemies_spawned <= total_enemy_budget),
    CHECK (hazards_spawned <= total_hazard_budget),
    CHECK (loot_spawned <= total_loot_budget),
    CHECK (difficulty_tier IN ('Easy', 'Normal', 'Hard', 'Lethal'))
);

CREATE INDEX IF NOT EXISTS idx_budget_sector ON Sector_Population_Budget(sector_id);
CREATE INDEX IF NOT EXISTS idx_budget_difficulty ON Sector_Population_Budget(difficulty_tier);
CREATE INDEX IF NOT EXISTS idx_budget_biome ON Sector_Population_Budget(biome_id);

-- =====================================================
-- THREAT HEATMAP (for debugging/analytics)
-- =====================================================

CREATE TABLE IF NOT EXISTS Threat_Heatmap (
    heatmap_id INTEGER PRIMARY KEY AUTOINCREMENT,
    sector_id INTEGER NOT NULL,
    room_id INTEGER NOT NULL,

    -- Threat counts
    total_threats INTEGER DEFAULT 0,
    enemy_count INTEGER DEFAULT 0,
    hazard_count INTEGER DEFAULT 0,

    -- Intensity classification
    threat_intensity TEXT,  -- None, Low, Medium, High, Extreme

    -- Position (for spatial heatmap visualization)
    coord_x INTEGER,
    coord_y INTEGER,
    coord_z INTEGER,

    -- Timestamp
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CHECK (threat_intensity IN ('None', 'Low', 'Medium', 'High', 'Extreme'))
);

CREATE INDEX IF NOT EXISTS idx_heatmap_sector ON Threat_Heatmap(sector_id);
CREATE INDEX IF NOT EXISTS idx_heatmap_room ON Threat_Heatmap(room_id);
CREATE INDEX IF NOT EXISTS idx_heatmap_intensity ON Threat_Heatmap(threat_intensity);
CREATE INDEX IF NOT EXISTS idx_heatmap_position ON Threat_Heatmap(coord_x, coord_y, coord_z);

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Query to check budget distribution
-- SELECT
--     b.sector_id,
--     b.difficulty_tier,
--     b.biome_id,
--     b.total_enemy_budget,
--     b.enemies_spawned,
--     b.total_enemy_budget - b.enemies_spawned AS remaining_enemy_budget,
--     b.total_hazard_budget,
--     b.hazards_spawned,
--     b.total_hazard_budget - b.hazards_spawned AS remaining_hazard_budget
-- FROM Sector_Population_Budget b
-- ORDER BY b.created_at DESC;

-- Query to analyze threat heatmap distribution
-- SELECT
--     threat_intensity,
--     COUNT(*) AS room_count,
--     ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Threat_Heatmap WHERE sector_id = 1), 2) AS percentage,
--     AVG(total_threats) AS avg_threats,
--     MIN(total_threats) AS min_threats,
--     MAX(total_threats) AS max_threats
-- FROM Threat_Heatmap
-- WHERE sector_id = 1
-- GROUP BY threat_intensity
-- ORDER BY
--     CASE threat_intensity
--         WHEN 'None' THEN 1
--         WHEN 'Low' THEN 2
--         WHEN 'Medium' THEN 3
--         WHEN 'High' THEN 4
--         WHEN 'Extreme' THEN 5
--     END;

-- Query to find over-populated sectors
-- SELECT
--     b.sector_id,
--     b.difficulty_tier,
--     b.biome_id,
--     b.enemies_spawned,
--     b.total_enemy_budget,
--     COUNT(h.room_id) AS total_rooms,
--     ROUND(CAST(b.enemies_spawned AS FLOAT) / COUNT(h.room_id), 2) AS avg_enemies_per_room
-- FROM Sector_Population_Budget b
-- LEFT JOIN Threat_Heatmap h ON b.sector_id = h.sector_id
-- GROUP BY b.sector_id
-- HAVING avg_enemies_per_room > 3.0  -- Target: 2.0-2.5 enemies per room
-- ORDER BY avg_enemies_per_room DESC;

-- =====================================================
-- SCHEMA COMPLETE
-- =====================================================
