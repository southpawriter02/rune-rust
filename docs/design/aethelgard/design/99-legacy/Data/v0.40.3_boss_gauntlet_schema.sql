-- ═══════════════════════════════════════════════════════════════════════
-- v0.40.3: BOSS GAUNTLET MODE
-- Sequential boss encounter challenge with limited resources
-- ═══════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════
-- GAUNTLET SEQUENCES
-- Defines gauntlet configurations (boss order, difficulty tiers)
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Boss_Gauntlet_Sequences (
    sequence_id TEXT PRIMARY KEY,           -- e.g., "gauntlet_classic", "gauntlet_nightmare"
    sequence_name TEXT NOT NULL,            -- Display name
    description TEXT NOT NULL,              -- Lore description
    difficulty_tier TEXT NOT NULL,          -- "Moderate", "Hard", "Extreme", "Nightmare"
    boss_count INTEGER NOT NULL,            -- Number of bosses (8-10)
    boss_ids TEXT NOT NULL,                 -- JSON array of boss IDs in order

    -- Resource limits
    max_full_heals INTEGER DEFAULT 3,       -- Maximum full heals allowed
    max_revives INTEGER DEFAULT 1,          -- Maximum revives allowed

    -- Requirements
    required_ng_plus_tier INTEGER DEFAULT 0,-- Minimum NG+ tier required
    prerequisite_runs TEXT,                 -- JSON array of prerequisite gauntlet IDs

    -- Rewards
    completion_reward_id TEXT,              -- Legendary item reward
    title_reward TEXT,                      -- Prestige title granted

    -- Metadata
    active BOOLEAN DEFAULT 1,               -- Is this gauntlet currently available?
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ═══════════════════════════════════════════════════════════════════════
-- GAUNTLET RUNS
-- Individual gauntlet run attempts
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Boss_Gauntlet_Runs (
    run_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    sequence_id TEXT NOT NULL,

    -- Run state
    status TEXT NOT NULL,                   -- "in_progress", "victory", "defeat"
    current_boss_index INTEGER DEFAULT 0,   -- Current boss number (0-based)

    -- Resources
    full_heals_remaining INTEGER NOT NULL,  -- Heals left
    revives_remaining INTEGER NOT NULL,     -- Revives left

    -- Statistics
    total_time_seconds INTEGER,             -- Total run time
    total_damage_taken INTEGER DEFAULT 0,   -- Total damage across all bosses
    total_damage_dealt INTEGER DEFAULT 0,   -- Total damage dealt
    total_deaths INTEGER DEFAULT 0,         -- Total deaths (revives used)

    -- Character state snapshot (JSON)
    starting_character_state TEXT NOT NULL, -- Character stats at gauntlet start

    -- NG+ context
    ng_plus_tier INTEGER DEFAULT 0,         -- NG+ tier when run started

    -- Timestamps
    started_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    completed_at DATETIME,

    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    FOREIGN KEY (sequence_id) REFERENCES Boss_Gauntlet_Sequences(sequence_id)
);

CREATE INDEX IF NOT EXISTS idx_gauntlet_runs_character ON Boss_Gauntlet_Runs(character_id);
CREATE INDEX IF NOT EXISTS idx_gauntlet_runs_sequence ON Boss_Gauntlet_Runs(sequence_id);
CREATE INDEX IF NOT EXISTS idx_gauntlet_runs_status ON Boss_Gauntlet_Runs(status);

-- ═══════════════════════════════════════════════════════════════════════
-- BOSS ENCOUNTERS
-- Individual boss fights within gauntlet runs
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Boss_Gauntlet_Boss_Encounters (
    encounter_id INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    boss_index INTEGER NOT NULL,            -- Boss number in sequence (0-based)
    boss_id TEXT NOT NULL,                  -- Boss identifier from v0.23

    -- Outcome
    result TEXT NOT NULL,                   -- "victory", "defeat"

    -- Statistics
    completion_time_seconds INTEGER,        -- Time to complete this boss
    damage_taken INTEGER DEFAULT 0,
    damage_dealt INTEGER DEFAULT 0,
    deaths INTEGER DEFAULT 0,               -- Deaths during this fight

    -- Resource usage
    heals_used INTEGER DEFAULT 0,           -- Full heals used in this fight
    revive_used BOOLEAN DEFAULT 0,          -- Did we revive in this fight?

    -- Player state after fight (JSON)
    ending_character_state TEXT,            -- HP, corruption, etc. after fight

    -- Timestamp
    completed_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (run_id) REFERENCES Boss_Gauntlet_Runs(run_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_gauntlet_encounters_run ON Boss_Gauntlet_Boss_Encounters(run_id);
CREATE INDEX IF NOT EXISTS idx_gauntlet_encounters_boss ON Boss_Gauntlet_Boss_Encounters(boss_id);

-- ═══════════════════════════════════════════════════════════════════════
-- LEADERBOARDS
-- Track best performances across 4 categories
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Boss_Gauntlet_Leaderboard (
    entry_id INTEGER PRIMARY KEY AUTOINCREMENT,
    sequence_id TEXT NOT NULL,
    category TEXT NOT NULL,                 -- "fastest", "flawless", "no_heal", "ng_plus"

    -- Run reference
    run_id INTEGER NOT NULL,
    character_id INTEGER NOT NULL,
    character_name TEXT NOT NULL,

    -- Performance metrics
    total_time_seconds INTEGER NOT NULL,
    total_deaths INTEGER NOT NULL,
    heals_used INTEGER NOT NULL,
    ng_plus_tier INTEGER NOT NULL,

    -- Ranking
    rank INTEGER,                           -- Calculated rank in category

    -- Timestamp
    achieved_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (run_id) REFERENCES Boss_Gauntlet_Runs(run_id) ON DELETE CASCADE,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    FOREIGN KEY (sequence_id) REFERENCES Boss_Gauntlet_Sequences(sequence_id)
);

CREATE INDEX IF NOT EXISTS idx_gauntlet_leaderboard_sequence ON Boss_Gauntlet_Leaderboard(sequence_id);
CREATE INDEX IF NOT EXISTS idx_gauntlet_leaderboard_category ON Boss_Gauntlet_Leaderboard(category);
CREATE INDEX IF NOT EXISTS idx_gauntlet_leaderboard_rank ON Boss_Gauntlet_Leaderboard(rank);

-- ═══════════════════════════════════════════════════════════════════════
-- VIEWS
-- ═══════════════════════════════════════════════════════════════════════

-- Best times per sequence
CREATE VIEW IF NOT EXISTS vw_gauntlet_best_times AS
SELECT
    sequence_id,
    MIN(total_time_seconds) as best_time_seconds,
    COUNT(*) as total_completions,
    AVG(total_time_seconds) as avg_time_seconds
FROM Boss_Gauntlet_Runs
WHERE status = 'victory'
GROUP BY sequence_id;

-- Flawless runs (no deaths)
CREATE VIEW IF NOT EXISTS vw_gauntlet_flawless_runs AS
SELECT
    run_id,
    character_id,
    sequence_id,
    total_time_seconds,
    ng_plus_tier,
    completed_at
FROM Boss_Gauntlet_Runs
WHERE status = 'victory'
  AND total_deaths = 0
ORDER BY total_time_seconds ASC;

-- Character gauntlet statistics
CREATE VIEW IF NOT EXISTS vw_character_gauntlet_stats AS
SELECT
    character_id,
    COUNT(*) as total_runs,
    SUM(CASE WHEN status = 'victory' THEN 1 ELSE 0 END) as victories,
    SUM(CASE WHEN status = 'defeat' THEN 1 ELSE 0 END) as defeats,
    ROUND(CAST(SUM(CASE WHEN status = 'victory' THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100, 1) as win_rate,
    MIN(CASE WHEN status = 'victory' THEN total_time_seconds END) as best_time,
    AVG(CASE WHEN status = 'victory' THEN total_time_seconds END) as avg_completion_time,
    SUM(CASE WHEN total_deaths = 0 AND status = 'victory' THEN 1 ELSE 0 END) as flawless_runs
FROM Boss_Gauntlet_Runs
GROUP BY character_id;
