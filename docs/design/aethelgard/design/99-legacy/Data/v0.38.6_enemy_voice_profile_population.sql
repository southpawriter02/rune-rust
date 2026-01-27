-- ============================================================
-- v0.38.6: Enemy Voice Profile Descriptor Population
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Auto-populate enemy voice profile descriptor arrays
-- ============================================================
-- This script should be run AFTER inserting all combat descriptors
-- It queries descriptor IDs by archetype and updates voice profiles
-- ============================================================

-- Update SERVITOR voice profile
UPDATE Enemy_Voice_Profiles
SET
    attack_descriptors = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Servitor'
          AND category = 'EnemyAttack'
          AND (tags NOT LIKE '%Special%' OR tags IS NULL)
    ),
    reaction_damage = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Servitor'
          AND category = 'EnemyDefense'
          AND outcome_type = 'SolidHit'
    ),
    reaction_death = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Servitor'
          AND category = 'EnemyDefense'
          AND outcome_type = 'CriticalHit'
    ),
    special_attacks = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Servitor'
          AND category = 'EnemyAttack'
          AND tags LIKE '%Special%'
    )
WHERE enemy_archetype = 'Servitor';

-- Update FORLORN voice profile
UPDATE Enemy_Voice_Profiles
SET
    attack_descriptors = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Forlorn'
          AND category = 'EnemyAttack'
          AND (tags NOT LIKE '%Special%' OR tags IS NULL)
    ),
    reaction_damage = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Forlorn'
          AND category = 'EnemyDefense'
          AND outcome_type = 'SolidHit'
    ),
    reaction_death = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Forlorn'
          AND category = 'EnemyDefense'
          AND outcome_type = 'CriticalHit'
    ),
    special_attacks = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Forlorn'
          AND category = 'EnemyAttack'
          AND tags LIKE '%Special%'
    )
WHERE enemy_archetype = 'Forlorn';

-- Update CORRUPTED_DVERGR voice profile
UPDATE Enemy_Voice_Profiles
SET
    attack_descriptors = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Corrupted_Dvergr'
          AND category = 'EnemyAttack'
          AND (tags NOT LIKE '%Special%' OR tags IS NULL)
    ),
    reaction_damage = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Corrupted_Dvergr'
          AND category = 'EnemyDefense'
          AND outcome_type = 'SolidHit'
    ),
    reaction_death = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Corrupted_Dvergr'
          AND category = 'EnemyDefense'
          AND outcome_type = 'CriticalHit'
    ),
    special_attacks = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Corrupted_Dvergr'
          AND category = 'EnemyAttack'
          AND tags LIKE '%Special%'
    )
WHERE enemy_archetype = 'Corrupted_Dvergr';

-- Update BLIGHT_TOUCHED_BEAST voice profile
UPDATE Enemy_Voice_Profiles
SET
    attack_descriptors = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Blight_Touched_Beast'
          AND category = 'EnemyAttack'
          AND (tags NOT LIKE '%Special%' OR tags IS NULL)
    ),
    reaction_damage = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Blight_Touched_Beast'
          AND category = 'EnemyDefense'
          AND outcome_type = 'SolidHit'
    ),
    reaction_death = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Blight_Touched_Beast'
          AND category = 'EnemyDefense'
          AND outcome_type = 'CriticalHit'
    ),
    special_attacks = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Blight_Touched_Beast'
          AND category = 'EnemyAttack'
          AND tags LIKE '%Special%'
    )
WHERE enemy_archetype = 'Blight_Touched_Beast';

-- Update AETHER_WRAITH voice profile
UPDATE Enemy_Voice_Profiles
SET
    attack_descriptors = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Aether_Wraith'
          AND category = 'EnemyAttack'
          AND (tags NOT LIKE '%Special%' OR tags IS NULL)
    ),
    reaction_damage = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Aether_Wraith'
          AND category = 'EnemyDefense'
          AND outcome_type = 'SolidHit'
    ),
    reaction_death = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Aether_Wraith'
          AND category = 'EnemyDefense'
          AND outcome_type = 'CriticalHit'
    ),
    special_attacks = (
        SELECT json_group_array(descriptor_id)
        FROM Combat_Action_Descriptors
        WHERE enemy_archetype = 'Aether_Wraith'
          AND category = 'EnemyAttack'
          AND tags LIKE '%Special%'
    )
WHERE enemy_archetype = 'Aether_Wraith';

-- ============================================================
-- Verification: Check that all profiles have descriptors
-- ============================================================

SELECT
    enemy_archetype,
    voice_description,
    json_array_length(attack_descriptors) AS attack_count,
    json_array_length(reaction_damage) AS damage_count,
    json_array_length(reaction_death) AS death_count,
    json_array_length(special_attacks) AS special_count
FROM Enemy_Voice_Profiles
ORDER BY enemy_archetype;

-- ============================================================
-- Notes:
-- - This script must be run AFTER player_action_descriptors.sql
--   and enemy_voice_profiles.sql
-- - Uses SQLite's json_group_array() to automatically create arrays
-- - Verification query at the end shows descriptor counts per profile
-- ============================================================
