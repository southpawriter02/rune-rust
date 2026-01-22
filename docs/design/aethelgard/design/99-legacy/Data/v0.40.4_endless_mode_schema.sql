-- ═══════════════════════════════════════════════════════════════════════
-- v0.40.4: ENDLESS MODE & LEADERBOARDS
-- Wave-based survival mode with infinite difficulty scaling
-- ═══════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════
-- ENDLESS MODE SEASONS
-- Seasonal leaderboard resets every 3 months
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Endless_Mode_Seasons (
    season_id TEXT PRIMARY KEY,             -- e.g., "season_1_2025_q4"
    name TEXT NOT NULL,                     -- Display name
    start_date DATETIME NOT NULL,           -- Season start
    end_date DATETIME NOT NULL,             -- Season end
    is_active BOOLEAN DEFAULT 0,            -- Currently active season?

    CHECK (end_date > start_date)
);

-- Seed initial season
INSERT INTO Endless_Mode_Seasons (season_id, name, start_date, end_date, is_active)
VALUES (
    'season_1_2025_q4',
    'Season 1: The First Gauntlet',
    '2025-10-01 00:00:00',
    '2025-12-31 23:59:59',
    1
);

-- ═══════════════════════════════════════════════════════════════════════
-- ENDLESS MODE RUNS
-- Individual endless mode run attempts
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Endless_Mode_Runs (
    run_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    seed TEXT NOT NULL,                     -- Run seed for reproducibility

    -- Run state
    start_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME,
    is_active BOOLEAN DEFAULT 1,            -- Is run currently active?
    highest_wave_reached INTEGER DEFAULT 1, -- Furthest wave reached
    current_wave INTEGER DEFAULT 1,         -- Current active wave

    -- Combat metrics
    total_enemies_killed INTEGER DEFAULT 0,
    total_bosses_killed INTEGER DEFAULT 0,
    total_damage_taken INTEGER DEFAULT 0,
    total_damage_dealt INTEGER DEFAULT 0,

    -- Performance
    total_time_seconds INTEGER DEFAULT 0,

    -- Scoring (calculated on run end)
    wave_score INTEGER DEFAULT 0,           -- WavesCompleted × 1000
    kill_score INTEGER DEFAULT 0,           -- EnemiesKilled × 50
    boss_score INTEGER DEFAULT 0,           -- BossesKilled × 500
    time_bonus INTEGER DEFAULT 0,           -- Max(0, 10000 - TotalTime)
    survival_bonus INTEGER DEFAULT 0,       -- Max(0, 5000 - DamageTaken)
    total_score INTEGER DEFAULT 0,          -- Sum of all scores

    -- Verification
    character_build_hash TEXT,              -- Build snapshot for anti-cheat
    is_verified BOOLEAN DEFAULT 0,          -- Verified by system

    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,

    CHECK (highest_wave_reached >= 1),
    CHECK (current_wave >= 1)
);

CREATE INDEX IF NOT EXISTS idx_endless_runs_character ON Endless_Mode_Runs(character_id);
CREATE INDEX IF NOT EXISTS idx_endless_runs_active ON Endless_Mode_Runs(is_active);
CREATE INDEX IF NOT EXISTS idx_endless_runs_seed ON Endless_Mode_Runs(seed);
CREATE INDEX IF NOT EXISTS idx_endless_runs_score ON Endless_Mode_Runs(total_score DESC);
CREATE INDEX IF NOT EXISTS idx_endless_runs_wave ON Endless_Mode_Runs(highest_wave_reached DESC);

-- ═══════════════════════════════════════════════════════════════════════
-- ENDLESS MODE WAVES
-- Individual wave records within runs
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Endless_Mode_Waves (
    wave_id INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    wave_number INTEGER NOT NULL,

    -- Wave composition
    enemy_count INTEGER NOT NULL,
    enemy_level INTEGER NOT NULL,
    difficulty_multiplier REAL NOT NULL,
    is_boss_wave BOOLEAN DEFAULT 0,

    -- Wave performance
    start_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME,
    wave_time_seconds INTEGER,
    enemies_killed INTEGER DEFAULT 0,
    damage_taken INTEGER DEFAULT 0,
    damage_dealt INTEGER DEFAULT 0,

    FOREIGN KEY (run_id) REFERENCES Endless_Mode_Runs(run_id) ON DELETE CASCADE,

    CHECK (wave_number >= 1),
    CHECK (enemy_count >= 1)
);

CREATE INDEX IF NOT EXISTS idx_endless_waves_run ON Endless_Mode_Waves(run_id);
CREATE INDEX IF NOT EXISTS idx_endless_waves_number ON Endless_Mode_Waves(wave_number);

-- ═══════════════════════════════════════════════════════════════════════
-- ENDLESS MODE LEADERBOARDS
-- Global competitive rankings
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Endless_Mode_Leaderboards (
    entry_id INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    character_id INTEGER NOT NULL,
    player_name TEXT NOT NULL,

    -- Category
    category TEXT NOT NULL,                 -- 'highest_wave', 'highest_score'

    -- Performance
    highest_wave_reached INTEGER NOT NULL,
    total_score INTEGER NOT NULL,
    total_time_seconds INTEGER NOT NULL,

    -- Metadata
    character_level INTEGER,
    specialization_name TEXT,
    seed TEXT NOT NULL,
    character_build_hash TEXT,
    is_verified BOOLEAN DEFAULT 0,
    report_count INTEGER DEFAULT 0,         -- Community report count

    -- Seasonal
    season_id TEXT,
    submitted_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Ranking (calculated)
    rank INTEGER,

    FOREIGN KEY (run_id) REFERENCES Endless_Mode_Runs(run_id) ON DELETE CASCADE,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    FOREIGN KEY (season_id) REFERENCES Endless_Mode_Seasons(season_id),

    CHECK (category IN ('highest_wave', 'highest_score')),
    CHECK (highest_wave_reached >= 1)
);

CREATE INDEX IF NOT EXISTS idx_endless_leaderboard_category ON Endless_Mode_Leaderboards(category);
CREATE INDEX IF NOT EXISTS idx_endless_leaderboard_wave ON Endless_Mode_Leaderboards(highest_wave_reached DESC);
CREATE INDEX IF NOT EXISTS idx_endless_leaderboard_score ON Endless_Mode_Leaderboards(total_score DESC);
CREATE INDEX IF NOT EXISTS idx_endless_leaderboard_season ON Endless_Mode_Leaderboards(season_id);
CREATE INDEX IF NOT EXISTS idx_endless_leaderboard_rank ON Endless_Mode_Leaderboards(rank);

-- ═══════════════════════════════════════════════════════════════════════
-- VIEWS
-- ═══════════════════════════════════════════════════════════════════════

-- Active season view
CREATE VIEW IF NOT EXISTS vw_active_endless_season AS
SELECT season_id, name, start_date, end_date
FROM Endless_Mode_Seasons
WHERE is_active = 1
LIMIT 1;

-- Top performers by wave
CREATE VIEW IF NOT EXISTS vw_endless_top_waves AS
SELECT
    l.entry_id,
    l.player_name,
    l.highest_wave_reached,
    l.total_score,
    l.total_time_seconds,
    l.specialization_name,
    l.rank,
    l.season_id
FROM Endless_Mode_Leaderboards l
WHERE l.category = 'highest_wave'
  AND l.is_verified = 1
ORDER BY l.highest_wave_reached DESC, l.total_score DESC
LIMIT 100;

-- Top performers by score
CREATE VIEW IF NOT EXISTS vw_endless_top_scores AS
SELECT
    l.entry_id,
    l.player_name,
    l.total_score,
    l.highest_wave_reached,
    l.total_time_seconds,
    l.specialization_name,
    l.rank,
    l.season_id
FROM Endless_Mode_Leaderboards l
WHERE l.category = 'highest_score'
  AND l.is_verified = 1
ORDER BY l.total_score DESC, l.highest_wave_reached DESC
LIMIT 100;

-- Character endless statistics
CREATE VIEW IF NOT EXISTS vw_character_endless_stats AS
SELECT
    character_id,
    COUNT(*) as total_runs,
    MAX(highest_wave_reached) as best_wave,
    MAX(total_score) as best_score,
    AVG(highest_wave_reached) as avg_wave,
    AVG(total_score) as avg_score,
    SUM(total_enemies_killed) as lifetime_kills,
    SUM(total_bosses_killed) as lifetime_bosses
FROM Endless_Mode_Runs
WHERE is_active = 0
GROUP BY character_id;

-- Wave progression statistics
CREATE VIEW IF NOT EXISTS vw_endless_wave_stats AS
SELECT
    wave_number,
    COUNT(*) as times_reached,
    AVG(wave_time_seconds) as avg_time,
    AVG(damage_taken) as avg_damage_taken,
    AVG(enemies_killed) as avg_enemies_killed
FROM Endless_Mode_Waves
WHERE end_time IS NOT NULL
GROUP BY wave_number
ORDER BY wave_number;
