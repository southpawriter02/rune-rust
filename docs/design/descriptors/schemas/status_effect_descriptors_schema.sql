-- ==============================================================================
-- v0.38.8: Status Effects & Condition Descriptors - Database Schema
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Comprehensive status effect and condition descriptor library
-- Timeline: 8-10 hours
-- Philosophy: Every ailment tells a story of how the world harms you
-- ==============================================================================

-- ==============================================================================
-- TABLE 1: Status_Effect_Descriptors
-- ==============================================================================
-- Purpose: Core status effect flavor text for application, ticks, and expiry
-- Pattern: Follows v0.38.6 Combat and v0.38.7 Galdr descriptor patterns
-- Templates: Use {Variable} placeholders for dynamic content
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Status_Effect_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    effect_type TEXT NOT NULL,           -- Burning, Bleeding, Poisoned, Stunned, Slowed,
                                         -- Weakened, Blinded, Confused, Corroding, Freezing,
                                         -- Haste, Strengthened, Protected, Regenerating,
                                         -- BlightCorruption, Cursed

    application_context TEXT NOT NULL,   -- OnApply (how affliction is applied)
                                         -- OnTick (ongoing effect each turn)
                                         -- OnExpire (recovery/end of effect)
                                         -- OnRemove (active removal/cleanse)

    -- Severity Levels
    severity TEXT,                       -- Minor (1-2 damage/turn or weak effects)
                                         -- Moderate (3-5 damage/turn or medium effects)
                                         -- Severe (6+ damage/turn or strong effects)
                                         -- NULL (for non-severity-based effects)

    -- Source Context
    source_type TEXT,                    -- EnemyAttack, Environmental, GaldrBackfire,
                                         -- WeaponCoated, BeastBite, ToxicHaze,
                                         -- Lightning, ConcussiveForce, AlfheimTear,
                                         -- ForlornTouch, Natural, Antidote, Bandaged,
                                         -- NULL (generic)

    source_detail TEXT,                  -- Specific source details (e.g., "Fire", "Slashing",
                                         -- "Piercing", "Acid", "Venom", etc.)

    -- Target Information
    target_type TEXT,                    -- Player, Enemy, Ally, NULL (all)

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Status effect narrative with {Variables}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Verbose", "Concise", "Dramatic"]

    -- Constraints
    CONSTRAINT valid_effect_type CHECK (effect_type IN (
        'Burning', 'Bleeding', 'Poisoned', 'Stunned', 'Slowed',
        'Weakened', 'Blinded', 'Confused', 'Corroding', 'Freezing',
        'Haste', 'Strengthened', 'Protected', 'Regenerating',
        'BlightCorruption', 'Cursed'
    )),
    CONSTRAINT valid_context CHECK (application_context IN (
        'OnApply', 'OnTick', 'OnExpire', 'OnRemove'
    )),
    CONSTRAINT valid_severity CHECK (severity IS NULL OR severity IN (
        'Minor', 'Moderate', 'Severe', 'Catastrophic'
    ))
);

-- ==============================================================================
-- TABLE 2: Status_Effect_Source_Modifiers
-- ==============================================================================
-- Purpose: Source-specific variations (beast bite vs. toxic haze)
-- Usage: Additional flavor based on how the status effect was applied
-- Integration: Layered with base descriptors for richer narratives
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Status_Effect_Source_Modifiers (
    modifier_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Source Classification
    effect_type TEXT NOT NULL,           -- Which status effect this modifies
    source_type TEXT NOT NULL,           -- Specific source (BeastBite, ToxicHaze, etc.)

    -- Source Details
    enemy_archetype TEXT,                -- Servitor, Forlorn, Corrupted_Dvergr,
                                         -- Blight_Touched_Beast, Aether_Wraith, NULL

    weapon_type TEXT,                    -- TwoHanded, OneHanded, Bow, Crossbow, NULL
    environmental_hazard TEXT,           -- Lava, BrokenPipe, CollapsingCeiling,
                                         -- JaggedMetal, BrokenGlass, NULL

    -- Biome Context
    biome_name TEXT,                     -- The_Roots, Muspelheim, Niflheim, Alfheim,
                                         -- Jotunheim, NULL (any)

    -- Modifier Text
    modifier_prefix TEXT,                -- Text to prepend to base descriptor
    modifier_suffix TEXT,                -- Text to append to base descriptor
    replacement_text TEXT,               -- Complete replacement descriptor (optional)

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_source_effect_type CHECK (effect_type IN (
        'Burning', 'Bleeding', 'Poisoned', 'Stunned', 'Slowed',
        'Weakened', 'Blinded', 'Confused', 'Corroding', 'Freezing',
        'Haste', 'Strengthened', 'Protected', 'Regenerating',
        'BlightCorruption', 'Cursed'
    ))
);

-- ==============================================================================
-- TABLE 3: Status_Effect_Severity_Profiles
-- ==============================================================================
-- Purpose: Define severity thresholds and characteristics per effect type
-- Usage: Maps damage ranges to severity levels (e.g., 1-2 = Minor, 3-5 = Moderate)
-- Integration: Used to select appropriate descriptors based on effect strength
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Status_Effect_Severity_Profiles (
    profile_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Effect Classification
    effect_type TEXT NOT NULL,           -- Status effect type
    severity TEXT NOT NULL,              -- Severity level

    -- Thresholds (for DoT effects)
    damage_per_turn_min INTEGER,        -- Minimum damage for this severity
    damage_per_turn_max INTEGER,        -- Maximum damage for this severity

    -- Stack Thresholds (for stackable effects like Bleeding, BlightCorruption)
    stack_count_min INTEGER,             -- Minimum stacks for this severity
    stack_count_max INTEGER,             -- Maximum stacks for this severity

    -- Duration Thresholds (for timed effects)
    duration_min INTEGER,                -- Minimum duration (turns)
    duration_max INTEGER,                -- Maximum duration (turns)

    -- Visual/Narrative Cues
    intensity_description TEXT,          -- Narrative guidance ("Manageable pain" vs "Agony")
    urgency_level TEXT,                  -- Low, Medium, High, Critical

    -- Metadata
    is_active INTEGER DEFAULT 1,

    -- Constraints
    CONSTRAINT valid_severity_profile_effect CHECK (effect_type IN (
        'Burning', 'Bleeding', 'Poisoned', 'Stunned', 'Slowed',
        'Weakened', 'Blinded', 'Confused', 'Corroding', 'Freezing',
        'Haste', 'Strengthened', 'Protected', 'Regenerating',
        'BlightCorruption', 'Cursed'
    )),
    CONSTRAINT valid_severity_level CHECK (severity IN (
        'Minor', 'Moderate', 'Severe', 'Catastrophic'
    )),
    CONSTRAINT valid_urgency CHECK (urgency_level IN (
        'Low', 'Medium', 'High', 'Critical'
    )),
    -- Ensure unique profiles per effect/severity combination
    CONSTRAINT unique_effect_severity UNIQUE (effect_type, severity)
);

-- ==============================================================================
-- TABLE 4: Status_Effect_Interaction_Descriptors
-- ==============================================================================
-- Purpose: Flavor text for status effect interactions (e.g., Burning + Freezing)
-- Usage: When multiple status effects interact, combine, or cancel each other
-- Integration: Triggered by AdvancedStatusEffectService interaction system
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Status_Effect_Interaction_Descriptors (
    interaction_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Interaction Classification
    effect_type_1 TEXT NOT NULL,         -- First status effect
    effect_type_2 TEXT NOT NULL,         -- Second status effect

    interaction_type TEXT NOT NULL,      -- Suppress (cancels out)
                                         -- Amplify (enhances effect)
                                         -- Transform (creates new effect)
                                         -- Synergy (bonus effect)
                                         -- Neutralize (both removed)

    -- Result
    result_effect TEXT,                  -- New effect if transformation occurs
                                         -- NULL if suppression/neutralization

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Interaction narrative with {Variables}

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_interaction_type CHECK (interaction_type IN (
        'Suppress', 'Amplify', 'Transform', 'Synergy', 'Neutralize'
    ))
);

-- ==============================================================================
-- TABLE 5: Status_Effect_Environmental_Context
-- ==============================================================================
-- Purpose: Biome-specific status effect variations
-- Usage: How status effects manifest differently in each realm
-- Example: Burning in Muspelheim vs. Burning in Niflheim
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Status_Effect_Environmental_Context (
    context_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    effect_type TEXT NOT NULL,           -- Status effect type
    biome_name TEXT NOT NULL,            -- The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim

    application_context TEXT,            -- OnApply, OnTick, OnExpire, NULL (all)

    -- Environmental Modifier
    environmental_descriptor TEXT NOT NULL, -- How the biome affects the status
                                            -- "The volcanic heat intensifies the flames!"
                                            -- "The eternal ice slows the bleeding..."

    -- Mechanical Modifiers (optional)
    duration_modifier REAL,              -- Multiplier for duration (e.g., 1.5 = 50% longer)
    damage_modifier REAL,                -- Multiplier for damage (e.g., 0.75 = 25% less)

    -- Metadata
    trigger_chance REAL DEFAULT 0.30,    -- 30% chance for flavor to appear
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_env_effect_type CHECK (effect_type IN (
        'Burning', 'Bleeding', 'Poisoned', 'Stunned', 'Slowed',
        'Weakened', 'Blinded', 'Confused', 'Corroding', 'Freezing',
        'Haste', 'Strengthened', 'Protected', 'Regenerating',
        'BlightCorruption', 'Cursed'
    )),
    CONSTRAINT valid_env_context CHECK (application_context IS NULL OR application_context IN (
        'OnApply', 'OnTick', 'OnExpire', 'OnRemove'
    )),
    CONSTRAINT valid_trigger_chance CHECK (trigger_chance >= 0.0 AND trigger_chance <= 1.0)
);

-- ==============================================================================
-- INDEXES FOR PERFORMANCE
-- ==============================================================================

-- Status_Effect_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_status_effect_type
    ON Status_Effect_Descriptors(effect_type);
CREATE INDEX IF NOT EXISTS idx_status_context
    ON Status_Effect_Descriptors(application_context);
CREATE INDEX IF NOT EXISTS idx_status_severity
    ON Status_Effect_Descriptors(severity);
CREATE INDEX IF NOT EXISTS idx_status_source
    ON Status_Effect_Descriptors(source_type);
CREATE INDEX IF NOT EXISTS idx_status_target
    ON Status_Effect_Descriptors(target_type);

-- Composite index for common queries
CREATE INDEX IF NOT EXISTS idx_status_effect_context_severity
    ON Status_Effect_Descriptors(effect_type, application_context, severity);

-- Status_Effect_Source_Modifiers indexes
CREATE INDEX IF NOT EXISTS idx_status_modifier_effect
    ON Status_Effect_Source_Modifiers(effect_type);
CREATE INDEX IF NOT EXISTS idx_status_modifier_source
    ON Status_Effect_Source_Modifiers(source_type);
CREATE INDEX IF NOT EXISTS idx_status_modifier_biome
    ON Status_Effect_Source_Modifiers(biome_name);

-- Status_Effect_Severity_Profiles indexes
CREATE INDEX IF NOT EXISTS idx_severity_profile_effect
    ON Status_Effect_Severity_Profiles(effect_type);
CREATE INDEX IF NOT EXISTS idx_severity_profile_level
    ON Status_Effect_Severity_Profiles(severity);

-- Status_Effect_Interaction_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_interaction_effect1
    ON Status_Effect_Interaction_Descriptors(effect_type_1);
CREATE INDEX IF NOT EXISTS idx_interaction_effect2
    ON Status_Effect_Interaction_Descriptors(effect_type_2);
CREATE INDEX IF NOT EXISTS idx_interaction_type
    ON Status_Effect_Interaction_Descriptors(interaction_type);

-- Status_Effect_Environmental_Context indexes
CREATE INDEX IF NOT EXISTS idx_env_context_effect
    ON Status_Effect_Environmental_Context(effect_type);
CREATE INDEX IF NOT EXISTS idx_env_context_biome
    ON Status_Effect_Environmental_Context(biome_name);

-- ==============================================================================
-- TEMPLATE VARIABLE REFERENCE
-- ==============================================================================
-- Available template variables for descriptor_text fields:
--
-- CHARACTER VARIABLES:
--   {Target}              - Target name (enemy, player, ally name)
--   {Enemy}               - Enemy name/archetype
--   {Player}              - Player name
--
-- EFFECT VARIABLES:
--   {EffectType}          - Status effect name (Burning, Poisoned, etc.)
--   {Damage}              - Damage amount per turn
--   {Duration}            - Effect duration in turns
--   {StackCount}          - Number of stacks (for stackable effects)
--   {Severity}            - Severity level (Minor, Moderate, Severe)
--
-- SOURCE VARIABLES:
--   {Source}              - Source of status effect (enemy name, hazard, etc.)
--   {SourceDetail}        - Specific detail (fire type, weapon, etc.)
--   {Weapon}              - Weapon name (if weapon-sourced)
--
-- LOCATION VARIABLES:
--   {Location}            - Body part affected (arm, leg, torso, etc.)
--   {Vital_Location}      - Vital area (heart, throat, core, etc.)
--   {Biome}               - Current biome name
--   {Environment_Feature} - Nearby feature (lava flow, pipe, ceiling, etc.)
--
-- MANIFESTATION VARIABLES:
--   {BlightEffect}        - Blight corruption manifestation
--   {ParadoxLevel}        - Paradox buildup level (Low, Moderate, High)
--   {CorruptionStacks}    - Blight corruption stack count
--
-- ==============================================================================
-- USAGE EXAMPLE
-- ==============================================================================
-- Query for Burning application descriptor (Moderate severity, Fire source):
--
-- SELECT descriptor_text
-- FROM Status_Effect_Descriptors
-- WHERE effect_type = 'Burning'
--   AND application_context = 'OnApply'
--   AND severity = 'Moderate'
--   AND source_type = 'EnemyAttack'
--   AND is_active = 1
-- ORDER BY RANDOM()
-- LIMIT 1;
--
-- Result: "The {Enemy}'s flames catch on your clothing—you're burning!"
--
-- After variable substitution:
-- "The Corrupted Servitor's flames catch on your clothing—you're burning!"
--
-- ==============================================================================
-- Query for Bleeding tick descriptor (Severe severity):
--
-- SELECT descriptor_text
-- FROM Status_Effect_Descriptors
-- WHERE effect_type = 'Bleeding'
--   AND application_context = 'OnTick'
--   AND severity = 'Severe'
--   AND is_active = 1
-- ORDER BY RANDOM()
-- LIMIT 1;
--
-- Result: "You're hemorrhaging—the world swims as blood loss takes its toll!"
--
-- ==============================================================================
-- Query for Poisoned expiration with antidote:
--
-- SELECT descriptor_text
-- FROM Status_Effect_Descriptors
-- WHERE effect_type = 'Poisoned'
--   AND application_context = 'OnRemove'
--   AND source_type = 'Antidote'
--   AND is_active = 1
-- ORDER BY RANDOM()
-- LIMIT 1;
--
-- Result: "The antidote burns its way down—immediately the poison's effects diminish!"
--
-- ==============================================================================
