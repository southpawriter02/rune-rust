-- ==============================================================================
-- Rune & Rust: Database Initialization Script
-- ==============================================================================
-- Purpose: Initialize the runeandrust.db database with all required schemas
-- Usage: Run this script to set up a fresh database
-- ==============================================================================

-- ==============================================================================
-- v0.38: Descriptor Framework & Content Database
-- ==============================================================================

-- Base descriptor framework
.read Data/v0.38.0_descriptor_framework_schema.sql

-- v0.38.13: Ambient Environmental Descriptors (NEWEST)
.read Data/v0.38.13_ambient_environmental_descriptors_schema.sql
.read Data/v0.38.13_ambient_environmental_descriptors_data.sql

-- v0.38.12: Combat Mechanics Descriptors
.read Data/v0.38.12_combat_mechanics_descriptors_schema.sql
.read Data/v0.38.12_combat_mechanics_descriptors_data.sql

-- v0.38.11: NPC Descriptors & Dialogue Barks
.read Data/v0.38.11_npc_descriptors_barks_schema.sql
.read Data/v0.38.11_npc_descriptors_barks_data.sql

-- v0.38.10: Skill Usage Descriptors
.read Data/v0.38.10_skill_usage_descriptors_schema.sql
.read Data/v0.38.10_skill_usage_descriptors_data.sql

-- v0.38.9: Examination & Perception Descriptors
.read Data/v0.38.9_examination_perception_descriptors_schema.sql
.read Data/v0.38.9_examination_perception_descriptors_data.sql

-- v0.38.8: Status Effect Descriptors
.read Data/v0.38.8_status_effect_descriptors_schema.sql
.read Data/v0.38.8_status_effect_descriptors_data.sql

-- v0.38.4: Atmospheric Descriptors (Static)
.read Data/v0.38.4_atmospheric_descriptor_schema.sql
.read Data/v0.38.4_atmospheric_descriptors.sql
.read Data/v0.38.4_biome_atmosphere_profiles.sql

-- v0.38.1-3: Room Descriptions, Features, Objects
.read Data/v0.38.1_room_description_library_schema.sql
.read Data/v0.38.1_descriptor_fragments_content.sql
.read Data/v0.38.1_room_function_variants.sql
.read Data/v0.38.2_environmental_feature_catalog_schema.sql
.read Data/v0.38.2_hazard_modifiers.sql
.read Data/v0.38.3_interactive_object_repository_schema.sql
.read Data/v0.38.3_object_tables_and_variants.sql

-- v0.38.5-7: Resources, Combat, Galdr
.read Data/v0.38.5_resource_node_schema.sql
.read Data/v0.38.5_resource_node_templates.sql
.read Data/v0.38.5_biome_resource_profiles.sql
.read Data/v0.38.6_combat_flavor_text_schema.sql
.read Data/v0.38.6_enemy_voice_profiles.sql
.read Data/v0.38.6_enemy_voice_profile_population.sql
.read Data/v0.38.6_environmental_combat_modifiers.sql
.read Data/v0.38.6_player_action_descriptors.sql
.read Data/v0.38.7_galdr_flavor_text_schema.sql
.read Data/v0.38.7_galdr_action_descriptors.sql
.read Data/v0.38.7_galdr_outcome_descriptors.sql
.read Data/v0.38.7_galdr_miscast_descriptors.sql
.read Data/v0.38.7_caster_voices_and_ability_flavors.sql
.read Data/v0.38.7_galdr_manifestations_and_environmental.sql

-- ==============================================================================
-- Other Systems (Add as needed)
-- ==============================================================================

-- v0.34: Companion System
-- .read Data/v0.34.1_companion_schema.sql
-- .read Data/v0.34.3_companion_quests.sql

-- v0.33: Faction System
-- .read Data/v0.33.1_faction_schema.sql
-- .read Data/v0.33.3_faction_content.sql

-- v0.29-32: Biome Systems
-- .read Data/v0.29.1_muspelheim_schema.sql
-- .read Data/v0.30.1_niflheim_schema.sql
-- .read Data/v0.31.1_alfheim_schema.sql
-- .read Data/v0.32.1_jotunheim_schema.sql

-- ==============================================================================
-- v0.39: Advanced Dynamic Room Engine
-- ==============================================================================

-- v0.39.1: 3D Vertical Layer System
.read Data/v0.39.1_spatial_layout_schema.sql

-- v0.39.2: Biome Transition & Blending
.read Data/v0.39.2_biome_transition_schema.sql

-- ==============================================================================
-- v0.40: Endgame Content & Replayability
-- ==============================================================================

-- v0.40.1: New Game+ System
.read Data/v0.40.1_new_game_plus_schema.sql

-- v0.40.2: Challenge Sectors
.read Data/v0.40.2_challenge_sectors_schema.sql
.read Data/v0.40.2_challenge_sectors_data.sql

-- v0.40.3: Boss Gauntlet Mode
.read Data/v0.40.3_boss_gauntlet_schema.sql
.read Data/v0.40.3_boss_gauntlet_data.sql

-- v0.40.4: Endless Mode & Leaderboards
.read Data/v0.40.4_endless_mode_schema.sql

-- ==============================================================================
-- Verification Queries
-- ==============================================================================

SELECT '=== Database Initialization Complete ===' AS status;
SELECT '--- Ambient Environmental Descriptors ---' AS category;
SELECT 'Ambient Sounds: ' || COUNT(*) AS count FROM Ambient_Sound_Descriptors;
SELECT 'Ambient Smells: ' || COUNT(*) AS count FROM Ambient_Smell_Descriptors;
SELECT 'Atmospheric Details: ' || COUNT(*) AS count FROM Ambient_Atmospheric_Detail_Descriptors;
SELECT 'Background Activities: ' || COUNT(*) AS count FROM Ambient_Background_Activity_Descriptors;

SELECT '--- v0.39: Advanced Room Engine ---' AS category;
SELECT 'Biome Adjacency Rules: ' || COUNT(*) AS count FROM Biome_Adjacency;
