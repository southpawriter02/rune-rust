-- ==============================================================================
-- v0.38.10: Skill Usage Flavor Text - Database Schema
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Comprehensive skill check descriptor library for non-combat actions
-- Timeline: 12-15 hours
-- Philosophy: Every skill check tells a story of expertise, struggle, and consequence
-- ==============================================================================

-- ==============================================================================
-- TABLE 1: Skill_Check_Descriptors
-- ==============================================================================
-- Purpose: Core skill check flavor text for attempts, successes, and failures
-- Pattern: Follows v0.38.8 Status Effect descriptor patterns
-- Templates: Use {Variable} placeholders for dynamic content
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Skill_Check_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    skill_type TEXT NOT NULL,            -- SystemBypass, Acrobatics, WastelandSurvival, Rhetoric

    action_type TEXT NOT NULL,           -- Specific action within skill:
                                         -- SystemBypass: Lockpicking, TerminalHacking, TrapDisarm
                                         -- Acrobatics: Climbing, Leaping, Stealth
                                         -- WastelandSurvival: Tracking, Foraging, Navigation
                                         -- Rhetoric: Persuasion, Deception, Intimidation

    check_phase TEXT NOT NULL,           -- Attempt (setup/context)
                                         -- Success (general success)
                                         -- Failure (general failure)
                                         -- CriticalSuccess (exceptional success)

    -- Success/Failure Degree
    result_degree TEXT,                  -- Minimal (1-2 margin)
                                         -- Solid (3-5 margin)
                                         -- Critical (6+ margin)
                                         -- NULL (for Attempt phase)

    -- Environmental Context
    environmental_context TEXT,          -- Context-specific modifiers:
                                         -- Lockpicking: SimpleLock, ComplexLock, CorrodedLock, DamagedLock
                                         -- Climbing: CorrodedStructure, DangerousHeight, GlitchedTerrain
                                         -- Tracking: FreshTracks, OldTracks, UnusualTracks
                                         -- Foraging: RichArea, DangerousArea, ContaminatedArea
                                         -- Navigation: NormalTravel, StormHazard, GlitchedSpace
                                         -- Stealth: ShadowyCover, NoisyEnvironment, OpenGround
                                         -- Rhetoric: ReasonableRequest, DifficultRequest, SimpleDeception, ComplexDeception
                                         -- NULL (generic)

    biome_context TEXT,                  -- Muspelheim, Niflheim, Alfheim, The_Roots, NULL (any)

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Skill check narrative with {Variables}
                                         -- Available: {Player}, {Character}, {SkillType}, {ActionType},
                                         --            {DC}, {Roll}, {Margin}, {LockType}, {Terminal},
                                         --            {Gap}, {Height}, {Target}, {Biome}, {Condition},
                                         --            {Terrain}, {Duration}, {Tool}, {ToolQuality}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Verbose", "Concise", "Dramatic", "Technical"]

    -- Constraints
    CONSTRAINT valid_skill_type CHECK (skill_type IN (
        'SystemBypass', 'Acrobatics', 'WastelandSurvival', 'Rhetoric'
    )),
    CONSTRAINT valid_action_type CHECK (action_type IN (
        'Lockpicking', 'TerminalHacking', 'TrapDisarm',
        'Climbing', 'Leaping', 'Stealth',
        'Tracking', 'Foraging', 'Navigation',
        'Persuasion', 'Deception', 'Intimidation'
    )),
    CONSTRAINT valid_check_phase CHECK (check_phase IN (
        'Attempt', 'Success', 'Failure', 'CriticalSuccess'
    )),
    CONSTRAINT valid_result_degree CHECK (result_degree IS NULL OR result_degree IN (
        'Minimal', 'Solid', 'Critical'
    ))
);

-- ==============================================================================
-- TABLE 2: Skill_Fumble_Descriptors
-- ==============================================================================
-- Purpose: Fumble and catastrophic failure descriptors
-- Usage: Describes consequences of critical failures (fumbles)
-- Integration: Selected when skill check rolls are catastrophically low
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Skill_Fumble_Descriptors (
    fumble_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    skill_type TEXT NOT NULL,            -- SystemBypass, Acrobatics, WastelandSurvival, Rhetoric
    action_type TEXT NOT NULL,           -- Specific action that fumbled

    consequence_type TEXT NOT NULL,      -- Type of fumble consequence:
                                         -- ToolBreakage, AlarmTriggered, TrapActivated,
                                         -- InjuryTaken, ItemLost, DetectedByEnemy,
                                         -- ResourceWasted, StructuralCollapse,
                                         -- SocialConsequence, TimeWasted, Poisoned,
                                         -- ReputationLoss, ProgressLost,
                                         -- EnvironmentalHazard, BlightCorruption

    -- Severity
    severity TEXT DEFAULT 'Moderate',    -- Minor (annoying but recoverable)
                                         -- Moderate (setback with real consequences)
                                         -- Severe (major problem)
                                         -- Catastrophic (disaster)

    -- Mechanical Effects
    damage_formula TEXT,                 -- Damage dealt (e.g., "1d6", "2d6", "3d10", NULL)
    status_effect_applied TEXT,          -- Status effect applied (e.g., "Poisoned", "Stunned", NULL)
    next_attempt_dc_modifier INTEGER,    -- DC modifier for next attempt (e.g., +2, +5, NULL)
    time_penalty_minutes INTEGER,        -- Time penalty in minutes (e.g., 10, 60, NULL)
    prevents_retry INTEGER DEFAULT 0,    -- 1 if prevents further attempts, 0 otherwise

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Fumble consequence narrative with {Variables}
                                         -- Available: {Player}, {Character}, {Damage}, {StatusEffect},
                                         --            {DCModifier}, {TimeLost}, {Tool}, {ToolType},
                                         --            {Alarm}, {Trap}, {Structure}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Dangerous", "Comedic", "Tragic", "Equipment_Loss"]

    -- Constraints
    CONSTRAINT valid_fumble_skill_type CHECK (skill_type IN (
        'SystemBypass', 'Acrobatics', 'WastelandSurvival', 'Rhetoric'
    )),
    CONSTRAINT valid_fumble_consequence CHECK (consequence_type IN (
        'ToolBreakage', 'AlarmTriggered', 'TrapActivated',
        'InjuryTaken', 'ItemLost', 'DetectedByEnemy',
        'ResourceWasted', 'StructuralCollapse',
        'SocialConsequence', 'TimeWasted', 'Poisoned',
        'ReputationLoss', 'ProgressLost',
        'EnvironmentalHazard', 'BlightCorruption'
    )),
    CONSTRAINT valid_fumble_severity CHECK (severity IN (
        'Minor', 'Moderate', 'Severe', 'Catastrophic'
    ))
);

-- ==============================================================================
-- Indexes for Performance
-- ==============================================================================

CREATE INDEX IF NOT EXISTS idx_skill_check_lookup ON Skill_Check_Descriptors(
    skill_type, action_type, check_phase, result_degree
);

CREATE INDEX IF NOT EXISTS idx_skill_check_environmental ON Skill_Check_Descriptors(
    environmental_context, biome_context
);

CREATE INDEX IF NOT EXISTS idx_fumble_lookup ON Skill_Fumble_Descriptors(
    skill_type, action_type, consequence_type
);

-- ==============================================================================
-- Statistics View
-- ==============================================================================

CREATE VIEW IF NOT EXISTS vw_skill_usage_stats AS
SELECT
    'Skill Check Descriptors' AS category,
    skill_type,
    action_type,
    COUNT(*) AS descriptor_count
FROM Skill_Check_Descriptors
WHERE is_active = 1
GROUP BY skill_type, action_type

UNION ALL

SELECT
    'Fumble Descriptors' AS category,
    skill_type,
    action_type,
    COUNT(*) AS descriptor_count
FROM Skill_Fumble_Descriptors
WHERE is_active = 1
GROUP BY skill_type, action_type

ORDER BY category, skill_type, action_type;

-- ==============================================================================
-- End of Schema
-- ==============================================================================
