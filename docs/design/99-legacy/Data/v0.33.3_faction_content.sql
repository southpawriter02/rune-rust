-- =====================================================
-- v0.33.3: Faction Quests & Rewards Content
-- =====================================================
-- Version: v0.33.3
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.33.1 (Database schema), v0.33.2 (Reputation mechanics)
-- =====================================================
-- Document ID: RR-SPEC-v0.33.3-CONTENT
-- Parent Specification: v0.33 Faction System & Reputation
-- Status: Implementation Ready
-- Timeline: 7-11 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- FACTION_QUESTS: Link quests to factions
-- =====================================================
-- 25 faction quests total (5 per faction)
-- Quest JSON files are in Data/Quests/faction_*.json
-- =====================================================

-- Iron-Banes Quests (Faction ID: 1)
INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_ironbanes_training_protocols', 1, 0, 15, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_ironbanes_purge_rust', 1, 0, 20, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_ironbanes_corrupted_forge', 1, 25, 30, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_ironbanes_destroy_sleeper', 1, 50, 40, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_ironbanes_blight_containment', 1, 75, 50, 0);

-- God-Sleeper Cultists Quests (Faction ID: 2)
INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_godsleeper_temple_duties', 2, 0, 10, 1);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_godsleeper_offerings', 2, 0, 15, 1);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_godsleeper_defend_sacred', 2, 25, 30, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_godsleeper_awakening_ritual', 2, 50, 40, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_godsleeper_sacred_pilgrimage', 2, 75, 50, 0);

-- Jötun-Readers Quests (Faction ID: 3)
INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_jotunreaders_data_recovery', 3, 0, 15, 1);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_jotunreaders_knowledge_sharing', 3, 0, 20, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_jotunreaders_glitch_analysis', 3, 25, 25, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_jotunreaders_forbidden_archive', 3, 50, 35, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_jotunreaders_great_silence', 3, 75, 50, 0);

-- Rust-Clans Quests (Faction ID: 4)
INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_rustclans_defense_duty', 4, 0, 20, 1);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_rustclans_scavenge_run', 4, 0, 15, 1);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_rustclans_trade_route', 4, 25, 25, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_rustclans_cache_recovery', 4, 50, 30, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_rustclans_clan_sigil', 4, 75, 40, 0);

-- Independents Quests (Faction ID: 5)
INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_independents_own_path', 5, 0, 10, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_independents_solo_survival', 5, 0, 15, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_independents_decline_recruitment', 5, 25, 20, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_independents_neutral_mediator', 5, 50, 30, 0);

INSERT OR IGNORE INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward, is_repeatable)
VALUES ('faction_independents_lone_wolf', 5, 75, 50, 0);

-- =====================================================
-- FACTION_REWARDS: Exclusive rewards at reputation gates
-- =====================================================
-- 18 faction rewards (3+ per faction minimum)
-- =====================================================

-- Iron-Banes Rewards (Faction ID: 1)
INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (1, 'Discount', 'Iron-Bane Merchant Discount', 'Friendly standing grants 10% discount at Iron-Bane merchants', 25, '{"discount_percent": 10}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (1, 'Consumable', 'Purification Sigil', 'Anti-Undying consumable that deals +2d6 damage to corrupted entities. Access to purchase at Iron-Bane merchants.', 50, '{"item_id": "purification_sigil", "damage_bonus": "2d6", "target": "Undying"}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (1, 'Equipment', 'Zealot''s Blade', 'Legendary anti-Undying weapon. +2d6 damage vs Undying, +1 MIGHT. Only available to Exalted Iron-Banes.', 75, '{"item_id": "zealots_blade", "damage_bonus": "2d6", "attribute_bonus": {"might": 1}}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (1, 'Ability', 'Purification Protocol Training', 'Learn advanced anti-corruption techniques. Grants +4 to rolls when fighting Undying.', 50, '{"ability_id": "purification_protocol", "bonus": 4, "condition": "vs_undying"}');

-- God-Sleeper Cultists Rewards (Faction ID: 2)
INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (2, 'Ability', 'Cultist''s Blessing', 'Gain +4 to rolls when near Jötun corpses or dormant constructs. The sleeping gods watch over you.', 25, '{"ability_id": "cultists_blessing", "bonus": 4, "condition": "near_jotun"}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (2, 'Equipment', 'Jötun-Touched Robe', 'Armor with psychic resistance. Reduces Psychic Stress gain by 50% when near Jötun logic cores.', 50, '{"item_id": "jotun_touched_robe", "psychic_resistance": 0.5}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (2, 'Ability', 'God-Sleeper''s Grimoire', 'Access to Jötun Attunement ability. Allows limited communion with dormant Jötun logic cores to gain knowledge.', 75, '{"ability_id": "jotun_attunement", "effect": "commune_with_logic_cores"}');

-- Jötun-Readers Rewards (Faction ID: 3)
INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (3, 'Service', 'Reader Archive Access', 'Access to Jötun-Reader archives. Unlock lore entries and historical data about Pre-Glitch era.', 25, '{"service_id": "archive_access", "lore_unlocks": true}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (3, 'Equipment', 'Scholar''s Focus', '+2 WITS equipment. Enhances analytical abilities and data interpretation.', 50, '{"item_id": "scholars_focus", "attribute_bonus": {"wits": 2}}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (3, 'Ability', 'Decryption Protocols', 'Ability to read corrupted logs and encrypted Pre-Glitch data. Essential for uncovering hidden knowledge.', 75, '{"ability_id": "decryption_protocols", "effect": "read_corrupted_data"}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (3, 'Discount', 'Scholar Discount', 'Allied standing grants 20% discount at Jötun-Reader knowledge merchants.', 50, '{"discount_percent": 20}');

-- Rust-Clans Rewards (Faction ID: 4)
INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (4, 'Discount', 'Rust-Clan Trade Discount', 'Friendly standing grants 15% discount at Rust-Clan merchants.', 25, '{"discount_percent": 15}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (4, 'Equipment', 'Scavenger''s Kit', 'Improvised crafting tools. Grants bonus to crafting and salvage rolls.', 50, '{"item_id": "scavengers_kit", "crafting_bonus": 2}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (4, 'Service', 'Clan Sigil - Hidden Cache Access', 'Access to hidden Rust-Clan supply caches scattered throughout Midgard. Contains rare salvage and resources.', 75, '{"service_id": "cache_access", "cache_locations": "midgard"}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (4, 'Ability', 'Pragmatic Survival', 'Rust-Clan training grants +2 to all survival-related rolls.', 50, '{"ability_id": "pragmatic_survival", "bonus": 2, "condition": "survival_rolls"}');

-- Independents Rewards (Faction ID: 5)
INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (5, 'Ability', 'Lone Wolf Trait', '+10% to all stats when adventuring solo without companions. Exalted independence mastery.', 100, '{"ability_id": "lone_wolf", "stat_bonus": 0.10, "condition": "solo"}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (5, 'Service', 'Independent Network', 'Access to hidden independent trader network. Special items not available through faction merchants.', 50, '{"service_id": "independent_network", "merchant_access": true}');

INSERT OR IGNORE INTO Faction_Rewards (faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data)
VALUES (5, 'Ability', 'Self-Reliance', 'Gain +4 to rolls when refusing faction assistance. Your independence is your strength.', 25, '{"ability_id": "self_reliance", "bonus": 4, "condition": "refuse_help"}');

COMMIT;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Test 1: Faction Quest Count
-- SELECT faction_id, COUNT(*) as quest_count
-- FROM Faction_Quests
-- GROUP BY faction_id
-- ORDER BY faction_id;
-- Expected: 5 quests per faction (5 factions = 25 total)

-- Test 2: Faction Reward Count
-- SELECT faction_id, COUNT(*) as reward_count
-- FROM Faction_Rewards
-- GROUP BY faction_id
-- ORDER BY faction_id;
-- Expected: 3-4 rewards per faction (18 total)

-- Test 3: Reputation-Gated Content
-- SELECT faction_id, required_reputation, COUNT(*) as item_count
-- FROM Faction_Quests
-- GROUP BY faction_id, required_reputation
-- ORDER BY faction_id, required_reputation;
-- Expected: Distribution across 0, 25, 50, 75 reputation tiers

-- Test 4: Repeatable Quests
-- SELECT COUNT(*) FROM Faction_Quests WHERE is_repeatable = 1;
-- Expected: 8 repeatable quests (basic earning quests)

-- Test 5: Reward Types
-- SELECT reward_type, COUNT(*) FROM Faction_Rewards GROUP BY reward_type;
-- Expected: Distribution across Equipment, Consumable, Service, Ability, Discount

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] 25 faction quests seeded (5 per faction)
-- [ ] 18 faction rewards seeded (3-4 per faction)
-- [ ] Reputation gates correctly assigned (0, 25, 50, 75, 100)
-- [ ] Repeatable quests flagged appropriately
-- [ ] Reward types properly categorized
-- [ ] All quest IDs match JSON file names
-- [ ] All faction IDs valid (1-5)
-- [ ] SQL migration script executes without errors
-- =====================================================
