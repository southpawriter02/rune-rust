-- =====================================================
-- v0.33.1: Faction System Database Schema & Definitions
-- =====================================================
-- Version: v0.33.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.14 (Quest System), v0.8 (NPC System)
-- =====================================================
-- Document ID: RR-SPEC-v0.33.1-DATABASE
-- Parent Specification: v0.33 Faction System & Reputation
-- Status: Implementation Ready
-- Timeline: 8-12 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- TABLE CREATION
-- =====================================================

-- Table: Factions
-- Stores the 5 major faction definitions
CREATE TABLE IF NOT EXISTS Factions (
    faction_id INTEGER PRIMARY KEY,
    faction_name TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    philosophy TEXT,
    description TEXT,
    primary_location TEXT,
    allied_factions TEXT,
    enemy_factions TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_factions_name ON Factions(faction_name);

-- Table: Characters_FactionReputations
-- Tracks per-character reputation with each faction
-- Reputation scale: -100 (Hated) to +100 (Exalted)
CREATE TABLE IF NOT EXISTS Characters_FactionReputations (
    reputation_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    faction_id INTEGER NOT NULL,
    reputation_value INTEGER DEFAULT 0 CHECK(reputation_value BETWEEN -100 AND 100),
    reputation_tier TEXT CHECK(reputation_tier IN ('Hated', 'Hostile', 'Neutral', 'Friendly', 'Allied', 'Exalted')),
    last_modified TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
    FOREIGN KEY (faction_id) REFERENCES Factions(faction_id),
    UNIQUE(character_id, faction_id)
);

CREATE INDEX IF NOT EXISTS idx_char_faction_rep_character ON Characters_FactionReputations(character_id);
CREATE INDEX IF NOT EXISTS idx_char_faction_rep_faction ON Characters_FactionReputations(faction_id);
CREATE INDEX IF NOT EXISTS idx_char_faction_rep_tier ON Characters_FactionReputations(reputation_tier);

-- Table: Faction_Quests
-- Links quests to factions with reputation requirements
-- quest_id references quest IDs from JSON-based quest system
CREATE TABLE IF NOT EXISTS Faction_Quests (
    faction_quest_id INTEGER PRIMARY KEY AUTOINCREMENT,
    quest_id TEXT NOT NULL,
    faction_id INTEGER NOT NULL,
    required_reputation INTEGER DEFAULT 0,
    reputation_reward INTEGER DEFAULT 0,
    is_repeatable INTEGER DEFAULT 0,
    FOREIGN KEY (faction_id) REFERENCES Factions(faction_id)
);

CREATE INDEX IF NOT EXISTS idx_faction_quests_faction ON Faction_Quests(faction_id);
CREATE INDEX IF NOT EXISTS idx_faction_quests_quest_id ON Faction_Quests(quest_id);
CREATE INDEX IF NOT EXISTS idx_faction_quests_rep_req ON Faction_Quests(required_reputation);

-- Table: Faction_Rewards
-- Stores faction-exclusive rewards unlocked at reputation thresholds
CREATE TABLE IF NOT EXISTS Faction_Rewards (
    reward_id INTEGER PRIMARY KEY AUTOINCREMENT,
    faction_id INTEGER NOT NULL,
    reward_type TEXT CHECK(reward_type IN ('Equipment', 'Consumable', 'Service', 'Ability', 'Discount')),
    reward_name TEXT NOT NULL,
    reward_description TEXT,
    required_reputation INTEGER DEFAULT 0,
    reward_data TEXT,
    FOREIGN KEY (faction_id) REFERENCES Factions(faction_id)
);

CREATE INDEX IF NOT EXISTS idx_faction_rewards_faction ON Faction_Rewards(faction_id);
CREATE INDEX IF NOT EXISTS idx_faction_rewards_type ON Faction_Rewards(reward_type);
CREATE INDEX IF NOT EXISTS idx_faction_rewards_rep_req ON Faction_Rewards(required_reputation);

-- =====================================================
-- DATA SEEDING: FACTIONS
-- =====================================================

-- Faction 1: Iron-Banes (Anti-Undying Zealots)
-- faction_id: 1
-- Philosophy: Purification protocols against corrupted processes
-- Primary Location: Trunk/Roots patrols, Muspelheim operations
INSERT OR IGNORE INTO Factions (
    faction_id,
    faction_name,
    display_name,
    philosophy,
    description,
    primary_location,
    allied_factions,
    enemy_factions
) VALUES (
    1,
    'IronBanes',
    'Iron-Banes',
    'The Undying are corrupted processes that must be purged. Every autonomous construct following 800-year-old protocols is a threat to coherent reality. We follow purification protocols to restore system integrity.',
    'Anti-Undying specialists who hunt corrupted constructs and prevent Runic Blight spread. Not religious zealots, but methodical anti-corruption technicians following purification protocols developed after the Glitch.',
    'Trunk/Roots/Muspelheim',
    'RustClans',
    'GodSleeperCultists'
);

-- Faction 2: God-Sleeper Cultists (Jötun-Forged Worshippers)
-- faction_id: 2
-- Philosophy: Cargo cult believing Jötun-Forged are sleeping gods
-- Primary Location: Jötunheim
INSERT OR IGNORE INTO Factions (
    faction_id,
    faction_name,
    display_name,
    philosophy,
    description,
    primary_location,
    allied_factions,
    enemy_factions
) VALUES (
    2,
    'GodSleeperCultists',
    'God-Sleeper Cultists',
    'The Jötun-Forged are sleeping gods awaiting the signal to awaken. Their dormancy is sacred. We are the caretakers, the faithful, the ones who will be there when they rise. Do not harm the sleepers.',
    'Cargo cultists who interpret Jötun logic core broadcasts as divine messages. They protect dormant Jötun-Forged and establish temples in Jötunheim. Their faith is a misinterpretation of corrupted psychic broadcasts.',
    'Jotunheim',
    'Independents',
    'IronBanes'
);

-- Faction 3: Jötun-Readers (Pre-Glitch Scholars)
-- faction_id: 3
-- Philosophy: Data archaeologists seeking Pre-Glitch knowledge
-- Primary Location: Alfheim, Command Deck sites
INSERT OR IGNORE INTO Factions (
    faction_id,
    faction_name,
    display_name,
    philosophy,
    description,
    primary_location,
    allied_factions,
    enemy_factions
) VALUES (
    3,
    'JotunReaders',
    'Jötun-Readers',
    'Knowledge is the only path to understanding the Glitch. Every corrupted log, every fragmented database, every Jötun logic core—these are the keys to comprehension. We preserve, we study, we learn.',
    'Data archaeologists and system analysts dedicated to recovering Pre-Glitch knowledge. They study corrupted systems to understand the Great Silence and archive all recovered data. Knowledge is their highest value.',
    'Alfheim',
    'RustClans',
    ''
);

-- Faction 4: Rust-Clans (Midgard Survivors)
-- faction_id: 4
-- Philosophy: Pragmatic survival first, no ideology
-- Primary Location: Midgard
INSERT OR IGNORE INTO Factions (
    faction_id,
    faction_name,
    display_name,
    philosophy,
    description,
    primary_location,
    allied_factions,
    enemy_factions
) VALUES (
    4,
    'RustClans',
    'Rust-Clans',
    'Survival first. No ideology, no worship, no grand theories. We scavenge, we trade, we defend our territory. The world crashed—we''re still here. That''s what matters.',
    'Pragmatic Midgard survivors focused on resource acquisition and trade networks. They cooperate with both Iron-Banes and Jötun-Readers when beneficial, prioritizing practical survival over ideology.',
    'Midgard',
    'IronBanes,JotunReaders',
    ''
);

-- Faction 5: Independents (Unaffiliated)
-- faction_id: 5
-- Philosophy: Maintain neutrality, reject faction politics
-- Primary Location: Anywhere
INSERT OR IGNORE INTO Factions (
    faction_id,
    faction_name,
    display_name,
    philosophy,
    description,
    primary_location,
    allied_factions,
    enemy_factions
) VALUES (
    5,
    'Independents',
    'Independents',
    'Factions are chains. We walk our own path.',
    'Unaffiliated individuals who reject faction membership. They maintain neutrality in faction conflicts and value personal freedom over collective identity. Gaining reputation with Independents requires actively declining other faction offers.',
    'All',
    '',
    ''
);

COMMIT;

-- =====================================================
-- VERIFICATION QUERIES (Run these to verify seeding)
-- =====================================================

-- Test 1: Faction Count
-- SELECT COUNT(*) FROM Factions;
-- Expected: 5

-- Test 2: Faction Names
-- SELECT faction_id, faction_name, display_name FROM Factions ORDER BY faction_id;
-- Expected: 5 factions (Iron-Banes, God-Sleeper Cultists, Jötun-Readers, Rust-Clans, Independents)

-- Test 3: Table Existence
-- SELECT name FROM sqlite_master WHERE type='table' AND name LIKE '%Faction%' ORDER BY name;
-- Expected: Characters_FactionReputations, Faction_Quests, Faction_Rewards, Factions

-- Test 4: Index Existence
-- SELECT name FROM sqlite_master WHERE type='index' AND name LIKE '%faction%' ORDER BY name;
-- Expected: 9 indexes across 4 tables

-- Test 5: Foreign Key Validation
-- PRAGMA foreign_key_check;
-- Expected: No results (empty = all FKs valid)

-- Test 6: Reputation Constraint Test
-- INSERT INTO Characters_FactionReputations (character_id, faction_id, reputation_value, reputation_tier)
-- VALUES (1, 1, 150, 'Exalted');
-- Expected: CHECK constraint failed (reputation_value must be -100 to +100)

-- Test 7: Unique Constraint Test
-- INSERT INTO Characters_FactionReputations (character_id, faction_id, reputation_value, reputation_tier)
-- VALUES (1, 1, 50, 'Allied');
-- INSERT INTO Characters_FactionReputations (character_id, faction_id, reputation_value, reputation_tier)
-- VALUES (1, 1, 60, 'Allied');
-- Expected: Second insert fails with UNIQUE constraint violation

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] All 4 tables created (Factions, Characters_FactionReputations, Faction_Quests, Faction_Rewards)
-- [ ] 5 factions seeded with complete data
-- [ ] All indexes created for performance optimization
-- [ ] Foreign key constraints validated
-- [ ] Reputation value constraints enforced (-100 to +100)
-- [ ] Reputation tier constraints enforced (6 valid tiers)
-- [ ] Unique constraint on (character_id, faction_id) enforced
-- [ ] Migration script executes without errors
-- [ ] All faction philosophies comply with v5.0 voice (technology, not mythology)
-- [ ] Faction names are ASCII-compliant
-- =====================================================
