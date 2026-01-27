-- ============================================
-- v0.34.3: Companion Personal Quest Definitions
-- ============================================
-- Defines 6 personal questlines, one for each companion
-- These quests unlock when companions are recruited
-- Completion provides unique rewards and deepens companion relationships

-- ============================================
-- QUEST DEFINITIONS
-- ============================================

-- Quest IDs: 7001-7006 (companion quest range)

-- 1. Kara Ironbreaker: 'The Last Protocol'
-- Theme: Military duty vs. humanity, recovering her squad's final mission data
INSERT INTO Quests (quest_id, quest_name, display_name, quest_type, description, objective_summary, reward_legend, reward_items, required_level, is_faction_quest, status)
VALUES (
    7001,
    'the_last_protocol',
    'The Last Protocol',
    'companion_personal',
    'Kara''s former squad, the Iron Watch, vanished during a high-risk infiltration of a Sovereign enclave. She has located their last known transmission coordinates deep in the Wastes. The encrypted data packet contains their final mission status - and potentially evidence of what went wrong. Kara must face the ghosts of her past and recover the truth.',
    'Travel to Grid Sector 47-Delta. Locate the Iron Watch beacon. Decrypt the mission logs. Survive the Sovereign patrol ambush.',
    500,
    'iron_watch_insignia,encrypted_data_core',
    5,
    0,
    'available'
);

-- 2. Finnr: 'The Forlorn Archive'
-- Theme: Lost knowledge, preserving vs. using dangerous data
INSERT INTO Quests (quest_id, quest_name, display_name, quest_type, description, objective_summary, reward_legend, reward_items, required_level, is_faction_quest, status)
VALUES (
    7002,
    'the_forlorn_archive',
    'The Forlorn Archive',
    'companion_personal',
    'Finnr has spent decades searching for the Forlorn Archive - a pre-Collapse library said to contain uncorrupted AI research. Recent Whisperwind intelligence suggests it lies beneath the ruins of Old Uppsala. However, the Archive is guarded by a rogue AI construct that has killed previous expeditions. Finnr must decide whether to preserve the knowledge or destroy it to prevent its misuse.',
    'Locate the Forlorn Archive entrance. Navigate the AI security maze. Confront the Guardian Construct. Choose: Preserve or Purge the Archive.',
    450,
    'archival_access_key,pre_collapse_dataslate',
    4,
    0,
    'available'
);

-- 3. Bjorn Stormhand: 'The Old Workshop'
-- Theme: Legacy, family, and the burden of craftsmanship
INSERT INTO Quests (quest_id, quest_name, display_name, quest_type, description, objective_summary, reward_legend, reward_items, required_level, is_faction_quest, status)
VALUES (
    7003,
    'the_old_workshop',
    'The Old Workshop',
    'companion_personal',
    'Bjorn''s grandfather was a legendary weaponsmith in Jarnheim, known for forging "Stormbreaker" - a weapon said to channel lightning itself. After the Sovereign occupation, the workshop was sealed and booby-trapped. Bjorn seeks to reclaim his family''s legacy and recover the final masterwork, but the workshop is now haunted by automated defense systems and scavengers.',
    'Infiltrate the sealed Stormhand Workshop. Disarm the legacy traps. Recover the Stormbreaker schematics. Craft or reclaim the masterwork.',
    400,
    'stormbreaker_schematic,master_smithing_tools',
    3,
    0,
    'available'
);

-- 4. Valdis: 'Breaking the Voices'
-- Theme: Mental trauma, self-control, confronting inner demons
INSERT INTO Quests (quest_id, quest_name, display_name, quest_type, description, objective_summary, reward_legend, reward_items, required_level, is_faction_quest, status)
VALUES (
    7004,
    'breaking_the_voices',
    'Breaking the Voices',
    'companion_personal',
    'Valdis''s psychic trauma has intensified. The "voices" she hears are not random - they are fragmented echoes of a Sovereign psych-conditioning program called "Project Overwrite." A rogue Whisperwind neuromancer offers a dangerous procedure to sever the conditioning, but it requires confronting the original trauma source: the facility where she was experimented on. Valdis must face her past to reclaim her mind.',
    'Locate the Project Overwrite facility. Undergo the neuromantic ritual. Confront the trauma construct. Break the conditioning or embrace the power.',
    550,
    'psychic_dampener_implant,fragmented_memory_shard',
    6,
    0,
    'available'
);

-- 5. Runa Bloodsinger: 'The Broken Oath'
-- Theme: Redemption, violence, finding purpose beyond vengeance
INSERT INTO Quests (quest_id, quest_name, display_name, quest_type, description, objective_summary, reward_legend, reward_items, required_level, is_faction_quest, status)
VALUES (
    7005,
    'the_broken_oath',
    'The Broken Oath',
    'companion_personal',
    'Runa swore a blood oath to protect her village before it was destroyed by raiders. She tracked the raider leader, "Scar-Eye," for years before losing the trail. New intelligence suggests Scar-Eye now leads a mercenary company in the employ of the Sovereign. Runa must choose: fulfill her oath of vengeance and become what she hates, or break the oath and find a new purpose.',
    'Track Scar-Eye to the Sovereign outpost. Infiltrate the mercenary camp. Confront Scar-Eye. Choose: Kill or Spare.',
    500,
    'bloodstained_axe,village_heirloom_pendant',
    5,
    0,
    'available'
);

-- 6. Einar Flameheart: 'Awaken the Sleeper'
-- Theme: Faith, doubt, and the cost of divine power
INSERT INTO Quests (quest_id, quest_name, display_name, quest_type, description, objective_summary, reward_legend, reward_items, required_level, is_faction_quest, status)
VALUES (
    7006,
    'awaken_the_sleeper',
    'Awaken the Sleeper',
    'companion_personal',
    'Einar''s faith has been tested by the Collapse. He believes a divine entity - the "Sleeper" - rests beneath the ruins of Gamla Uppsala, waiting to be awakened. Einar has received visions guiding him to perform an ancient ritual. However, the Sleeper may not be what he believes. Awakening it could bring salvation - or catastrophe. Einar must confront whether his faith is divine or delusion.',
    'Gather the three ritual components. Travel to Gamla Uppsala. Perform the Awakening Ritual. Face the Sleeper.',
    600,
    'divine_flame_focus,relic_of_the_sleeper',
    7,
    0,
    'available'
);

-- ============================================
-- LINK QUESTS TO COMPANIONS
-- ============================================

-- Update Companions table to reference personal quests
UPDATE Companions SET personal_quest_id = 7001, personal_quest_title = 'The Last Protocol' WHERE companion_id = 1; -- Kara
UPDATE Companions SET personal_quest_id = 7002, personal_quest_title = 'The Forlorn Archive' WHERE companion_id = 2; -- Finnr
UPDATE Companions SET personal_quest_id = 7003, personal_quest_title = 'The Old Workshop' WHERE companion_id = 3; -- Bjorn
UPDATE Companions SET personal_quest_id = 7004, personal_quest_title = 'Breaking the Voices' WHERE companion_id = 4; -- Valdis
UPDATE Companions SET personal_quest_id = 7005, personal_quest_title = 'The Broken Oath' WHERE companion_id = 5; -- Runa
UPDATE Companions SET personal_quest_id = 7006, personal_quest_title = 'Awaken the Sleeper' WHERE companion_id = 6; -- Einar

-- ============================================
-- VERIFICATION QUERIES
-- ============================================

-- Verify all quests created
-- SELECT quest_id, quest_name, display_name, quest_type, required_level, reward_legend FROM Quests WHERE quest_id BETWEEN 7001 AND 7006;

-- Verify companion linkage
-- SELECT companion_id, display_name, personal_quest_id, personal_quest_title FROM Companions WHERE personal_quest_id IS NOT NULL;

-- ============================================
-- NOTES
-- ============================================
-- Quest IDs 7001-7006 reserved for companion personal quests
-- All quests start as 'available' and unlock when companion is recruited (handled by RecruitmentService)
-- Rewards include unique items and Legend (XP) appropriate for companion level ranges
-- Quest completion mechanics handled by existing QuestService integration
-- Each quest has moral choice element reflecting companion's character arc
