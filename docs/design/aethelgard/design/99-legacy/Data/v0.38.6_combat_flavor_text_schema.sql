-- ============================================================
-- v0.38.6: Combat & Action Flavor Text - Schema
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Dynamic combat action descriptions for immersive gameplay
-- ============================================================

-- Combat Action Descriptors Table
-- Stores flavor text for player and enemy combat actions
CREATE TABLE IF NOT EXISTS Combat_Action_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    category TEXT NOT NULL,  -- 'PlayerMeleeAttack', 'PlayerRangedAttack', 'PlayerDefense',
                             -- 'PlayerMovement', 'EnemyAttack', 'EnemyDefense',
                             -- 'EnemyMovement', 'EnvironmentalReaction'
    weapon_type TEXT,  -- 'SwordOneHanded', 'SwordTwoHanded', 'Bow', 'Crossbow', etc. (NULL for enemy/env)
    enemy_archetype TEXT,  -- NULL for player actions, enemy archetype for enemy actions
    outcome_type TEXT,  -- 'Miss', 'Deflected', 'GlancingHit', 'SolidHit',
                        -- 'DevastatingHit', 'CriticalHit', 'Fumble' (NULL for neutral actions)
    descriptor_text TEXT NOT NULL,
    tags TEXT,  -- JSON array (e.g., '["OneHanded", "Critical", "Lethal"]')

    CHECK (category IN ('PlayerMeleeAttack', 'PlayerRangedAttack', 'PlayerDefense',
                        'PlayerMovement', 'EnemyAttack', 'EnemyDefense', 'EnemyMovement',
                        'EnvironmentalReaction')),
    CHECK (outcome_type IN ('Miss', 'Deflected', 'GlancingHit', 'SolidHit',
                            'DevastatingHit', 'CriticalHit', 'Fumble') OR outcome_type IS NULL)
);

-- Index for fast player action queries
CREATE INDEX IF NOT EXISTS idx_combat_player_actions
ON Combat_Action_Descriptors(category, weapon_type, outcome_type);

-- Index for fast enemy action queries
CREATE INDEX IF NOT EXISTS idx_combat_enemy_actions
ON Combat_Action_Descriptors(category, enemy_archetype);

-- Enemy Voice Profiles Table
-- Defines combat personality and descriptor sets for each enemy archetype
CREATE TABLE IF NOT EXISTS Enemy_Voice_Profiles (
    profile_id INTEGER PRIMARY KEY AUTOINCREMENT,
    enemy_archetype TEXT NOT NULL UNIQUE,
    voice_description TEXT NOT NULL,  -- "Mechanical, emotionless, relentless"
    setting_context TEXT NOT NULL,  -- Lore explanation

    -- Descriptor references (JSON arrays of descriptor_ids)
    attack_descriptors TEXT NOT NULL,  -- Regular attack flavor text IDs
    reaction_damage TEXT NOT NULL,  -- Reaction to taking damage IDs
    reaction_death TEXT NOT NULL,  -- Death/defeat descriptor IDs
    special_attacks TEXT  -- Special attack descriptor IDs (optional)
);

-- Environmental Combat Modifiers Table
-- Contextual combat atmosphere and hazard integration per biome
CREATE TABLE IF NOT EXISTS Environmental_Combat_Modifiers (
    modifier_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_name TEXT NOT NULL,
    modifier_type TEXT NOT NULL,  -- 'Reaction', 'HazardIntegration'
    descriptor_text TEXT NOT NULL,
    trigger_chance REAL DEFAULT 0.3,  -- Probability of modifier appearing (0.0-1.0)

    FOREIGN KEY (biome_name) REFERENCES Biomes(biome_name),
    CHECK (modifier_type IN ('Reaction', 'HazardIntegration')),
    CHECK (trigger_chance >= 0.0 AND trigger_chance <= 1.0)
);

-- Index for biome-specific combat modifiers
CREATE INDEX IF NOT EXISTS idx_environmental_combat_biome
ON Environmental_Combat_Modifiers(biome_name, modifier_type);

-- ============================================================
-- Notes:
-- - Combat_Action_Descriptors: 200+ entries
--   * 100+ player action descriptors (all weapon types × outcomes)
--   * 50+ enemy descriptors (5 archetypes × actions/reactions)
--   * 50+ environmental reactions
-- - Enemy_Voice_Profiles: 5 entries (Servitor, Forlorn, Corrupted_Dvergr,
--   Blight_Touched_Beast, Aether_Wraith)
-- - Environmental_Combat_Modifiers: 50+ entries (5 biomes × modifier types)
-- - Template variables: {Weapon}, {Enemy}, {Target_Location}, {Vital_Location},
--   {Damage_Type_Descriptor}, {Environment_Feature}, {DamageAmount}
-- ============================================================
