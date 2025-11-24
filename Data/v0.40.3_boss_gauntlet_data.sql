-- ═══════════════════════════════════════════════════════════════════════
-- v0.40.3: BOSS GAUNTLET MODE - SEQUENCE DATA
-- Defines 10 gauntlet sequences with varying difficulty and boss compositions
-- ═══════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════
-- MODERATE TIER GAUNTLETS (2.0-2.5x difficulty)
-- Entry-level gauntlets for learning mechanics
-- ═══════════════════════════════════════════════════════════════════════

-- Gauntlet 1: Apprentice's Trial
-- 8 bosses, mixed types, moderate difficulty
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_apprentice',
 'Apprentice''s Trial',
 'A proving ground for aspiring warriors. Face 8 diverse bosses to test your fundamental combat skills.',
 'Moderate',
 8,
 '["boss_corrupted_guardian", "boss_vault_keeper", "boss_shadow_weaver", "boss_flame_warden", "boss_frost_sentinel", "boss_void_harbinger", "boss_storm_caller", "boss_earth_shaper"]',
 3, 1, 0, NULL,
 'legendary_gauntlet_blade_tier1',
 'Apprentice Champion',
 1);

-- Gauntlet 2: Elemental Gauntlet
-- 8 bosses, elemental theme
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_elemental',
 'Elemental Gauntlet',
 'Master the elements. Eight elemental bosses test your adaptability across fire, frost, lightning, and earth.',
 'Moderate',
 8,
 '["boss_flame_warden", "boss_flame_titan", "boss_frost_sentinel", "boss_ice_tyrant", "boss_storm_caller", "boss_lightning_archon", "boss_earth_shaper", "boss_stone_colossus"]',
 3, 1, 0, NULL,
 'legendary_elemental_focus',
 'Elemental Master',
 1);

-- ═══════════════════════════════════════════════════════════════════════
-- HARD TIER GAUNTLETS (2.5-3.0x difficulty)
-- Challenging gauntlets requiring solid strategy
-- ═══════════════════════════════════════════════════════════════════════

-- Gauntlet 3: Corruption's Descent
-- 9 bosses, corruption/void theme
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_corruption',
 'Corruption''s Descent',
 'Descend into the heart of corruption. Nine increasingly corrupted bosses await, each more twisted than the last.',
 'Hard',
 9,
 '["boss_corrupted_guardian", "boss_shadow_weaver", "boss_void_harbinger", "boss_corruption_spreader", "boss_nightmare_herald", "boss_abyss_walker", "boss_void_tyrant", "boss_corruption_incarnate", "boss_oblivion_lord"]',
 3, 1, 0, '["gauntlet_apprentice"]',
 'legendary_void_breaker',
 'Corruption''s Bane',
 1);

-- Gauntlet 4: Titan's Challenge
-- 9 bosses, large/powerful enemies
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_titans',
 'Titan''s Challenge',
 'Face the mightiest of foes. Nine massive bosses with overwhelming power demand perfect execution.',
 'Hard',
 9,
 '["boss_flame_titan", "boss_ice_tyrant", "boss_stone_colossus", "boss_storm_giant", "boss_void_tyrant", "boss_chaos_titan", "boss_war_golem", "boss_ancient_dragon", "boss_primordial_beast"]',
 3, 1, 1, '["gauntlet_elemental"]',
 'legendary_titan_crusher',
 'Titan Slayer',
 1);

-- Gauntlet 5: Speed Run Gauntlet
-- 8 bosses, balanced for speedrunning
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_speedrun',
 'Speed Run Gauntlet',
 'Time is of the essence. Eight bosses balanced for quick, aggressive play. Leaderboard focused.',
 'Hard',
 8,
 '["boss_vault_keeper", "boss_shadow_weaver", "boss_flame_warden", "boss_frost_sentinel", "boss_storm_caller", "boss_void_harbinger", "boss_war_golem", "boss_chaos_sentinel"]',
 3, 1, 0, '["gauntlet_apprentice"]',
 'legendary_time_breaker',
 'Speed Demon',
 1);

-- ═══════════════════════════════════════════════════════════════════════
-- EXTREME TIER GAUNTLETS (3.0-3.5x difficulty)
-- Elite-level gauntlets for veterans
-- ═══════════════════════════════════════════════════════════════════════

-- Gauntlet 6: Nightmare Crucible
-- 10 bosses, extremely difficult
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_nightmare',
 'Nightmare Crucible',
 'The ultimate test of skill. Ten nightmare-tier bosses with brutal mechanics. Only the elite survive.',
 'Extreme',
 10,
 '["boss_nightmare_herald", "boss_abyss_walker", "boss_void_tyrant", "boss_corruption_incarnate", "boss_oblivion_lord", "boss_chaos_titan", "boss_ancient_dragon", "boss_primordial_beast", "boss_reality_render", "boss_end_of_all_things"]',
 3, 1, 2, '["gauntlet_corruption", "gauntlet_titans"]',
 'legendary_nightmare_ender',
 'Nightmare''s End',
 1);

-- Gauntlet 7: Flawless Gauntlet
-- 8 bosses, designed for no-death runs
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_flawless',
 'Flawless Gauntlet',
 'Perfect execution required. Eight bosses designed to punish a single mistake. Can you achieve perfection?',
 'Extreme',
 8,
 '["boss_corrupted_guardian", "boss_shadow_weaver", "boss_flame_titan", "boss_ice_tyrant", "boss_void_tyrant", "boss_chaos_titan", "boss_ancient_dragon", "boss_oblivion_lord"]',
 3, 1, 1, '["gauntlet_corruption"]',
 'legendary_perfection_blade',
 'Flawless Victor',
 1);

-- Gauntlet 8: NG+ Elite Gauntlet
-- 9 bosses, NG+ focused
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_ng_plus_elite',
 'NG+ Elite Gauntlet',
 'NG+ scaling at its finest. Nine bosses with extreme NG+ modifiers. Requires NG+2 minimum.',
 'Extreme',
 9,
 '["boss_void_harbinger", "boss_corruption_spreader", "boss_nightmare_herald", "boss_void_tyrant", "boss_chaos_titan", "boss_war_golem", "boss_ancient_dragon", "boss_primordial_beast", "boss_reality_render"]',
 3, 1, 2, '["gauntlet_nightmare"]',
 'legendary_ng_plus_crown',
 'NG+ Elite',
 1);

-- ═══════════════════════════════════════════════════════════════════════
-- NIGHTMARE TIER GAUNTLETS (3.5x+ difficulty)
-- The absolute hardest content in the game
-- ═══════════════════════════════════════════════════════════════════════

-- Gauntlet 9: Endgame Crucible
-- 10 bosses, maximum difficulty
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_endgame',
 'Endgame Crucible',
 'The final challenge. Ten of the most punishing bosses in sequence. Only the absolute best can claim victory.',
 'Nightmare',
 10,
 '["boss_corruption_spreader", "boss_nightmare_herald", "boss_abyss_walker", "boss_void_tyrant", "boss_corruption_incarnate", "boss_oblivion_lord", "boss_chaos_titan", "boss_ancient_dragon", "boss_primordial_beast", "boss_reality_render"]',
 3, 1, 3, '["gauntlet_nightmare", "gauntlet_flawless", "gauntlet_ng_plus_elite"]',
 'legendary_ultimate_weapon',
 'Endgame Legend',
 1);

-- Gauntlet 10: The Impossible Gauntlet
-- 10 bosses, truly insane difficulty
INSERT INTO Boss_Gauntlet_Sequences
(sequence_id, sequence_name, description, difficulty_tier, boss_count, boss_ids,
 max_full_heals, max_revives, required_ng_plus_tier, prerequisite_runs,
 completion_reward_id, title_reward, active)
VALUES
('gauntlet_impossible',
 'The Impossible Gauntlet',
 'The stuff of legends. Ten endgame bosses at maximum scaling. Reserved for those who have conquered everything else.',
 'Nightmare',
 10,
 '["boss_void_tyrant", "boss_corruption_incarnate", "boss_oblivion_lord", "boss_chaos_titan", "boss_war_golem", "boss_ancient_dragon", "boss_primordial_beast", "boss_reality_render", "boss_end_of_all_things", "boss_final_god"]',
 3, 1, 5, '["gauntlet_endgame"]',
 'legendary_gods_bane',
 'The Impossible',
 1);

-- ═══════════════════════════════════════════════════════════════════════
-- NOTES
-- ═══════════════════════════════════════════════════════════════════════

-- Boss IDs reference the v0.23 Boss Encounters system
-- All gauntlets provide 3 full heals and 1 revive
-- Prerequisite chains ensure gradual difficulty progression
-- NG+ tier requirements gate hardest content
-- Unique legendary rewards incentivize completion
-- Prestige titles provide social recognition

-- Difficulty Tiers:
-- - Moderate: 2.0-2.5x (Entry level)
-- - Hard: 2.5-3.0x (Challenging)
-- - Extreme: 3.0-3.5x (Elite)
-- - Nightmare: 3.5x+ (Hardest)

-- Leaderboard Categories:
-- 1. Fastest: Quickest completion time
-- 2. Flawless: No deaths
-- 3. No Heal: No healing items used
-- 4. NG+: Highest NG+ tier completions
