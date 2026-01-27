-- ==============================================================================
-- v0.38.7: Ability & Galdr Flavor Text - Database Schema
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Comprehensive ability and Galdr casting descriptor library
-- Timeline: 10-12 hours
-- Philosophy: Every magical action resonates with Norse metaphysics
-- ==============================================================================

-- ==============================================================================
-- TABLE 1: Galdr_Action_Descriptors
-- ==============================================================================
-- Purpose: Casting sequences, invocations, and chanting patterns
-- Pattern: Follows v0.38.6 Combat_Action_Descriptors structure
-- Templates: Use {Variable} placeholders for dynamic content
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Galdr_Action_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    category TEXT NOT NULL,              -- GaldrCasting, AbilityActivation, WeaponArt, TacticalAction
    action_type TEXT,                    -- Invocation, Chant, Manifestation, Discharge, Aftermath

    -- Galdr Specifics
    rune_school TEXT,                    -- Fehu, Thurisaz, Ansuz, Raido, Hagalaz, Naudiz, Isa, Jera,
                                         -- Tiwaz, Berkanan, Mannaz, Laguz, NULL (for non-Galdr)
    ability_name TEXT,                   -- FlameBolt, FrostLance, LightningBolt, HealingChant, etc.

    -- Success Level (for outcome-based descriptors)
    success_level TEXT,                  -- MinorSuccess (1-2), SolidSuccess (3-4),
                                         -- ExceptionalSuccess (5+), Miscast, NULL

    -- Ability Category (for non-Galdr abilities)
    ability_category TEXT,               -- WeaponArt, TacticalAbility, DefensiveStance, NULL
    weapon_type TEXT,                    -- TwoHanded, OneHanded, Bow, Crossbow, NULL

    -- Biome Context (optional)
    biome_name TEXT,                     -- The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim, NULL

    -- Template Text (with {Variable} placeholders)
    descriptor_text TEXT NOT NULL,       -- The actual flavor text template

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection (1.0 = default)
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Verbose", "Concise", "Dramatic"]

    -- Constraints
    CONSTRAINT valid_category CHECK (category IN (
        'GaldrCasting', 'AbilityActivation', 'WeaponArt',
        'TacticalAction', 'PassiveEffect'
    )),
    CONSTRAINT valid_action_type CHECK (action_type IS NULL OR action_type IN (
        'Invocation', 'Chant', 'RuneManifestation', 'Discharge',
        'Aftermath', 'EffectTrigger', 'Activation'
    )),
    CONSTRAINT valid_success_level CHECK (success_level IS NULL OR success_level IN (
        'MinorSuccess', 'SolidSuccess', 'ExceptionalSuccess', 'Miscast'
    ))
);

-- ==============================================================================
-- TABLE 2: Galdr_Manifestation_Descriptors
-- ==============================================================================
-- Purpose: Visual/sensory descriptions of magical effects
-- Usage: What the magic looks like, sounds like, feels like
-- Layering: Combines with action descriptors for full narrative
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Galdr_Manifestation_Descriptors (
    manifestation_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    rune_school TEXT NOT NULL,           -- Fehu, Thurisaz, Ansuz, Raido, etc.
    manifestation_type TEXT NOT NULL,    -- Visual, Auditory, Tactile, Supernatural

    -- Magic Type
    element TEXT,                        -- Fire, Ice, Lightning, Wind, Earth, Water,
                                         -- Healing, Shadow, Aether, NULL
    power_level TEXT,                    -- Weak, Moderate, Strong, Devastating

    -- Context
    biome_name TEXT,                     -- Biome-specific visual modifiers
    environmental_interaction TEXT,      -- How magic interacts with environment

    -- Description
    descriptor_text TEXT NOT NULL,       -- Manifestation flavor text

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,                           -- JSON array

    -- Constraints
    CONSTRAINT valid_manifestation_type CHECK (manifestation_type IN (
        'Visual', 'Auditory', 'Tactile', 'Supernatural', 'RunicGlyph'
    )),
    CONSTRAINT valid_power_level CHECK (power_level IN (
        'Weak', 'Moderate', 'Strong', 'Devastating', 'Catastrophic'
    ))
);

-- ==============================================================================
-- TABLE 3: Galdr_Outcome_Descriptors
-- ==============================================================================
-- Purpose: Success/failure narratives for ability resolution
-- Usage: Describes the result of an ability activation
-- Integration: Works with combat outcomes and status effects
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Galdr_Outcome_Descriptors (
    outcome_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    ability_name TEXT NOT NULL,          -- Specific ability (FlameBolt, HealingChant, etc.)
    outcome_type TEXT NOT NULL,          -- Hit, Miss, CriticalHit, PartialEffect, FullEffect
    success_count INTEGER,               -- Number of successes rolled (1-2, 3-4, 5+)

    -- Target Information
    target_type TEXT,                    -- Enemy, Self, Ally, Area, Environment
    enemy_archetype TEXT,                -- Servitor, Forlorn, Corrupted_Dvergr, etc.

    -- Effect Category
    effect_category TEXT,                -- Damage, Healing, Buff, Debuff, Control, Utility

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Outcome narrative with {Variables}

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_outcome_type CHECK (outcome_type IN (
        'Hit', 'Miss', 'CriticalHit', 'PartialEffect', 'FullEffect',
        'Resisted', 'Immune', 'Amplified'
    )),
    CONSTRAINT valid_effect_category CHECK (effect_category IN (
        'Damage', 'Healing', 'Buff', 'Debuff', 'Control', 'Utility', 'Summon'
    ))
);

-- ==============================================================================
-- TABLE 4: Galdr_Miscast_Descriptors
-- ==============================================================================
-- Purpose: Paradox, Blight interference, and magical failure narratives
-- Lore: Runic Blight corrupts Galdr, causing paradoxical effects
-- Integration: Triggered on failed casting checks or critical failures
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Galdr_Miscast_Descriptors (
    miscast_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    miscast_type TEXT NOT NULL,          -- BlightCorruption, Paradox, Backlash, Fizzle,
                                         -- WildMagic, AlfheimDistortion
    severity TEXT NOT NULL,              -- Minor, Moderate, Severe, Catastrophic

    -- Rune School
    rune_school TEXT,                    -- Which rune was corrupted (NULL = any)
    ability_name TEXT,                   -- Specific ability (NULL = any)

    -- Corruption Source
    corruption_source TEXT,              -- RunicBlight, AlfheimCursedChoir,
                                         -- AethericOverload, NULL

    -- Biome Context
    biome_name TEXT,                     -- Biome-specific miscast effects

    -- Effect Description
    descriptor_text TEXT NOT NULL,       -- Miscast narrative
    mechanical_effect TEXT,              -- JSON: {"damage": 5, "status": "Corrupted", "duration": 2}

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_miscast_type CHECK (miscast_type IN (
        'BlightCorruption', 'Paradox', 'Backlash', 'Fizzle',
        'WildMagic', 'AlfheimDistortion', 'RunicInversion'
    )),
    CONSTRAINT valid_severity CHECK (severity IN (
        'Minor', 'Moderate', 'Severe', 'Catastrophic'
    ))
);

-- ==============================================================================
-- TABLE 5: Galdr_Caster_Voices
-- ==============================================================================
-- Purpose: Mystic personality profiles (like Enemy_Voice_Profiles)
-- Usage: Defines casting style, personality, and preferred descriptors
-- Integration: Player specializations (VardWarden, RustWitch) + NPC mages
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Galdr_Caster_Voices (
    voice_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Identity
    caster_archetype TEXT NOT NULL,      -- VardWarden, RustWitch, Völva, Seiðkona,
                                         -- CorruptedMage, BossMage, etc.
    voice_name TEXT NOT NULL,            -- Display name

    -- Personality
    voice_description TEXT NOT NULL,     -- "Reverent and methodical" / "Desperate and heretical"
    casting_style TEXT NOT NULL,         -- "Formal runic incantations" / "Broken whispered chants"

    -- Lore Context
    setting_context TEXT,                -- Background lore

    -- Descriptor Associations (JSON arrays of descriptor IDs)
    invocation_descriptors TEXT,         -- JSON: [12, 15, 18] (Galdr_Action_Descriptors)
    manifestation_descriptors TEXT,      -- JSON: [3, 7, 9] (Galdr_Manifestation_Descriptors)
    miscast_descriptors TEXT,            -- JSON: [2, 5] (Galdr_Miscast_Descriptors)

    -- Preferred Rune Schools
    preferred_schools TEXT,              -- JSON: ["Tiwaz", "Berkanan"] (for VardWarden)

    -- Metadata
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Unique constraint
    CONSTRAINT unique_caster_archetype UNIQUE (caster_archetype)
);

-- ==============================================================================
-- TABLE 6: Galdr_Environmental_Reactions
-- ==============================================================================
-- Purpose: How biomes react to Galdr casting
-- Pattern: Similar to Environmental_Combat_Modifiers from v0.38.6
-- Usage: Additional flavor for casting in specific locations
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Galdr_Environmental_Reactions (
    reaction_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Location
    biome_name TEXT NOT NULL,            -- The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim

    -- Reaction Type
    reaction_type TEXT NOT NULL,         -- Resonance, Interference, Amplification, Distortion

    -- Magic Interaction
    rune_school TEXT,                    -- Which rune school (NULL = all)
    element TEXT,                        -- Which element (NULL = all)

    -- Trigger Conditions
    trigger_chance REAL DEFAULT 0.30,    -- 30% chance by default
    power_level_min TEXT,                -- NULL or Weak/Moderate/Strong/Devastating

    -- Reaction Description
    descriptor_text TEXT NOT NULL,       -- Environmental flavor text

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_reaction_type CHECK (reaction_type IN (
        'Resonance', 'Interference', 'Amplification', 'Distortion',
        'Harmony', 'Rejection'
    ))
);

-- ==============================================================================
-- TABLE 7: Ability_Flavor_Descriptors
-- ==============================================================================
-- Purpose: Non-Galdr ability flavor (weapon arts, tactical abilities)
-- Usage: Whirlwind Strike, Precision Strike, Sprint, Defensive Stance, etc.
-- Integration: Warrior/Adept abilities that aren't magical
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Ability_Flavor_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    ability_category TEXT NOT NULL,      -- WeaponArt, TacticalAbility, DefensiveAbility, PassiveAbility
    ability_name TEXT NOT NULL,          -- WhirlwindStrike, PrecisionStrike, Sprint, DefensiveStance

    -- Context
    weapon_type TEXT,                    -- TwoHanded, OneHanded, Bow, Crossbow, Unarmed, NULL
    specialization TEXT,                 -- SkarHordeAspirant, IronBane, AtgeirWielder, NULL

    -- Success Level
    success_level TEXT,                  -- MinorSuccess, SolidSuccess, ExceptionalSuccess, Failure

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Ability activation/resolution text with {Variables}

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_ability_category CHECK (ability_category IN (
        'WeaponArt', 'TacticalAbility', 'DefensiveAbility',
        'PassiveAbility', 'ResourceAbility'
    ))
);

-- ==============================================================================
-- INDEXES FOR PERFORMANCE
-- ==============================================================================

-- Galdr_Action_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_galdr_action_category
    ON Galdr_Action_Descriptors(category);
CREATE INDEX IF NOT EXISTS idx_galdr_action_rune_school
    ON Galdr_Action_Descriptors(rune_school);
CREATE INDEX IF NOT EXISTS idx_galdr_action_ability
    ON Galdr_Action_Descriptors(ability_name);
CREATE INDEX IF NOT EXISTS idx_galdr_action_success
    ON Galdr_Action_Descriptors(success_level);

-- Galdr_Manifestation_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_galdr_manifest_school
    ON Galdr_Manifestation_Descriptors(rune_school);
CREATE INDEX IF NOT EXISTS idx_galdr_manifest_element
    ON Galdr_Manifestation_Descriptors(element);
CREATE INDEX IF NOT EXISTS idx_galdr_manifest_power
    ON Galdr_Manifestation_Descriptors(power_level);

-- Galdr_Outcome_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_galdr_outcome_ability
    ON Galdr_Outcome_Descriptors(ability_name);
CREATE INDEX IF NOT EXISTS idx_galdr_outcome_type
    ON Galdr_Outcome_Descriptors(outcome_type);

-- Galdr_Miscast_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_galdr_miscast_type
    ON Galdr_Miscast_Descriptors(miscast_type);
CREATE INDEX IF NOT EXISTS idx_galdr_miscast_severity
    ON Galdr_Miscast_Descriptors(severity);
CREATE INDEX IF NOT EXISTS idx_galdr_miscast_school
    ON Galdr_Miscast_Descriptors(rune_school);

-- Galdr_Environmental_Reactions indexes
CREATE INDEX IF NOT EXISTS idx_galdr_env_biome
    ON Galdr_Environmental_Reactions(biome_name);
CREATE INDEX IF NOT EXISTS idx_galdr_env_school
    ON Galdr_Environmental_Reactions(rune_school);

-- Ability_Flavor_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_ability_flavor_category
    ON Ability_Flavor_Descriptors(ability_category);
CREATE INDEX IF NOT EXISTS idx_ability_flavor_ability
    ON Ability_Flavor_Descriptors(ability_name);

-- ==============================================================================
-- TEMPLATE VARIABLE REFERENCE
-- ==============================================================================
-- Available template variables for descriptor_text fields:
--
-- CHARACTER VARIABLES:
--   {Caster}              - Caster name ("Bjorn Ironwill" / "You")
--   {Target}              - Target name (enemy, ally, "yourself")
--   {Enemy}               - Enemy name
--   {Ally}                - Ally name
--
-- ABILITY VARIABLES:
--   {Ability}             - Ability display name
--   {Rune}                - Rune name (Fehu, Thurisaz, etc.)
--   {RuneSymbol}          - Rune symbol (ᚠ, ᚦ, etc.)
--   {Element}             - Element type (Fire, Ice, Lightning, etc.)
--
-- WEAPON VARIABLES:
--   {Weapon}              - Weapon name
--   {WeaponType}          - Weapon category (Sword, Axe, Bow, etc.)
--
-- OUTCOME VARIABLES:
--   {Damage}              - Damage amount
--   {SuccessCount}        - Number of successes rolled
--   {Healing}             - Healing amount
--   {Duration}            - Effect duration (turns)
--
-- LOCATION VARIABLES:
--   {Target_Location}     - Body part hit (torso, arm, leg, head, etc.)
--   {Vital_Location}      - Vital area (heart, core, neck, etc.)
--   {Biome}               - Current biome name
--   {Environment_Feature} - Nearby feature (pillar, wall, pool, etc.)
--
-- MANIFESTATION VARIABLES:
--   {RunicGlyph}          - Rune visual manifestation
--   {MagicColor}          - Magic visual color
--   {SoundEffect}         - Auditory effect
--   {TactileEffect}       - Tactile sensation
--
-- CORRUPTION VARIABLES:
--   {BlightEffect}        - Blight corruption description
--   {ParadoxManifestation}- Paradox visual
--   {CorruptionLevel}     - Severity (Minor, Moderate, Severe, Catastrophic)
--
-- ==============================================================================
-- USAGE EXAMPLE
-- ==============================================================================
-- SELECT descriptor_text
-- FROM Galdr_Action_Descriptors
-- WHERE category = 'GaldrCasting'
--   AND rune_school = 'Fehu'
--   AND ability_name = 'FlameBolt'
--   AND success_level = 'ExceptionalSuccess'
--   AND is_active = 1
-- ORDER BY RANDOM()
-- LIMIT 1;
--
-- Result: "You invoke Fehu with perfect resonance—the rune blazes in the air
--          before you! A devastating torrent of flame engulfs the {Target}!"
--
-- After variable substitution:
-- "You invoke Fehu with perfect resonance—the rune blazes in the air before you!
--  A devastating torrent of flame engulfs the Corrupted Servitor!"
-- ==============================================================================
