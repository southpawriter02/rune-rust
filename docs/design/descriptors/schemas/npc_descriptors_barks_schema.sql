-- ==============================================================================
-- v0.38.11: NPC Descriptors & Dialogue Barks - Database Schema
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Comprehensive NPC description and ambient dialogue library
-- Timeline: 10-12 hours
-- Philosophy: Every NPC contributes to world-building through appearance and voice
-- ==============================================================================

-- ==============================================================================
-- TABLE 1: NPC_Physical_Descriptors
-- ==============================================================================
-- Purpose: Physical appearance descriptors for NPCs by archetype and subtype
-- Pattern: Follows v0.38.10 Skill Check descriptor patterns
-- Templates: Use {Variable} placeholders for dynamic content
-- ==============================================================================

CREATE TABLE IF NOT EXISTS NPC_Physical_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    npc_archetype TEXT NOT NULL,         -- Dvergr, Seidkona, Bandit, Raider, Merchant, Guard, Citizen, Forlorn

    npc_subtype TEXT NOT NULL,           -- Archetype-specific subtypes:
                                         -- Dvergr: Tinkerer, Runecaster, Merchant
                                         -- Seidkona: WanderingSeidkona, YoungAcolyte, Seidmadr
                                         -- Bandit: Scout, Leader, DesperateOutcast
                                         -- Raider: Veteran, Brute, Scavenger
                                         -- Merchant: Prosperous, Struggling, Shrewd
                                         -- Guard: Veteran, Rookie, Captain
                                         -- Citizen: Laborer, Artisan, Elder
                                         -- Forlorn: Fresh, Deteriorated, Ancient

    descriptor_type TEXT NOT NULL,       -- FullBody, Face, Clothing, Equipment, Bearing, Distinguishing

    -- Contextual Modifiers (NULL = generic, applies to all)
    condition TEXT,                      -- Healthy, Wounded, Exhausted, Affluent, Impoverished, BattleReady, NULL
    biome_context TEXT,                  -- Muspelheim, Niflheim, Alfheim, The_Roots, NULL (any)
    age_category TEXT,                   -- Young, MiddleAged, Elderly, Ageless, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Physical description with {Variables}
                                         -- Available: {NPCName}, {NPCArchetype}, {NPCSubtype}, {Height},
                                         --            {Build}, {HairColor}, {EyeColor}, {Weapon}, {Armor},
                                         --            {Tool}, {Accessory}, {Wounds}, {Scars}, {Weathering},
                                         --            {Biome}, {Weather}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Intimidating", "Memorable", "Distinctive", "Professional"]

    -- Constraints
    CONSTRAINT valid_npc_archetype CHECK (npc_archetype IN (
        'Dvergr', 'Seidkona', 'Bandit', 'Raider', 'Merchant', 'Guard', 'Citizen', 'Forlorn'
    )),
    CONSTRAINT valid_descriptor_type CHECK (descriptor_type IN (
        'FullBody', 'Face', 'Clothing', 'Equipment', 'Bearing', 'Distinguishing'
    )),
    CONSTRAINT valid_condition CHECK (condition IS NULL OR condition IN (
        'Healthy', 'Wounded', 'Exhausted', 'Affluent', 'Impoverished', 'BattleReady'
    )),
    CONSTRAINT valid_age_category CHECK (age_category IS NULL OR age_category IN (
        'Young', 'MiddleAged', 'Elderly', 'Ageless'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_npc_physical_lookup
ON NPC_Physical_Descriptors(npc_archetype, npc_subtype, descriptor_type, condition);

-- ==============================================================================
-- TABLE 2: NPC_Ambient_Bark_Descriptors
-- ==============================================================================
-- Purpose: Ambient dialogue barks for NPCs by archetype, activity, and context
-- Usage: Describes what NPCs say while idle, working, or observing
-- Integration: Fired during room population, NPC idle behavior, and player proximity
-- ==============================================================================

CREATE TABLE IF NOT EXISTS NPC_Ambient_Bark_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    npc_archetype TEXT NOT NULL,         -- Dvergr, Seidkona, Bandit, Raider, Merchant, Guard, Citizen, Forlorn

    npc_subtype TEXT NOT NULL,           -- Same subtypes as NPC_Physical_Descriptors

    bark_type TEXT NOT NULL,             -- Bark categories:
                                         -- AtWork, IdleConversation, Observation, Warning, Celebration,
                                         -- Concern, Suspicion, Encouragement, Complaint, Teaching,
                                         -- Threat, Insult, Wounded, Fleeing, BattleCry, Greeting

    -- Contextual Modifiers (NULL = generic, applies to all)
    activity_context TEXT,               -- Working, Idle, Trading, Guarding, Crafting, Traveling,
                                         -- Fighting, Resting, Searching, Performing_Ritual, NULL

    disposition_context TEXT,            -- Hostile, Unfriendly, Neutral, Friendly, Allied, NULL

    biome_context TEXT,                  -- Muspelheim, Niflheim, Alfheim, The_Roots, NULL (any)

    trigger_condition TEXT,              -- PlayerNearby, PlayerAbsent, AllyPresent, EnemyNear,
                                         -- DangerDetected, ResourceFound, MechanismRepaired,
                                         -- RitualComplete, NULL

    -- Template Text
    dialogue_text TEXT NOT NULL,         -- Dialogue bark with {Variables}
                                         -- Available: {NPCName}, {NPCArchetype}, {NPCSubtype}, {PlayerName},
                                         --            {PlayerClass}, {PlayerFaction}, {Biome}, {LocationName},
                                         --            {TimeOfDay}, {Activity}, {Disposition}, {Condition},
                                         --            {Tool}, {Resource}, {ItemName}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Dvergr_Technical", "Threatening", "Mystical", "Cultural_Reference"]

    -- Constraints
    CONSTRAINT valid_npc_archetype_bark CHECK (npc_archetype IN (
        'Dvergr', 'Seidkona', 'Bandit', 'Raider', 'Merchant', 'Guard', 'Citizen', 'Forlorn'
    )),
    CONSTRAINT valid_bark_type CHECK (bark_type IN (
        'AtWork', 'IdleConversation', 'Observation', 'Warning', 'Celebration',
        'Concern', 'Suspicion', 'Encouragement', 'Complaint', 'Teaching',
        'Threat', 'Insult', 'Wounded', 'Fleeing', 'BattleCry', 'Greeting'
    )),
    CONSTRAINT valid_activity_context CHECK (activity_context IS NULL OR activity_context IN (
        'Working', 'Idle', 'Trading', 'Guarding', 'Crafting', 'Traveling',
        'Fighting', 'Resting', 'Searching', 'Performing_Ritual'
    )),
    CONSTRAINT valid_disposition_context CHECK (disposition_context IS NULL OR disposition_context IN (
        'Hostile', 'Unfriendly', 'Neutral', 'Friendly', 'Allied'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_npc_bark_lookup
ON NPC_Ambient_Bark_Descriptors(npc_archetype, npc_subtype, bark_type, activity_context, disposition_context);

-- ==============================================================================
-- TABLE 3: NPC_Reaction_Descriptors
-- ==============================================================================
-- Purpose: Emotional reaction descriptors for NPCs responding to events
-- Usage: Describes how NPCs react to player actions and environmental changes
-- Integration: Triggered by combat events, player actions, discoveries, dangers
-- ==============================================================================

CREATE TABLE IF NOT EXISTS NPC_Reaction_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    npc_archetype TEXT NOT NULL,         -- Dvergr, Seidkona, Bandit, Raider, Merchant, Guard, Citizen, Forlorn

    npc_subtype TEXT NOT NULL,           -- Same subtypes as NPC_Physical_Descriptors

    reaction_type TEXT NOT NULL,         -- Emotional reaction:
                                         -- Surprised, Angry, Fearful, Relieved, Suspicious, Joyful,
                                         -- Pained, Confused, Impressed, Disgusted, Grateful, Betrayed,
                                         -- Proud, Curious, Resigned

    -- Trigger Classification
    trigger_event TEXT NOT NULL,         -- Event that triggered reaction:
                                         -- PlayerApproaches, PlayerAttacks, PlayerHelps, PlayerGifts, PlayerSteals,
                                         -- AllyKilled, EnemyKilled, TakingDamage, VictoryAchieved,
                                         -- TreasureFound, SecretRevealed, MechanismRepaired, AncientKnowledgeFound,
                                         -- TrapTriggered, BlightEncounter, StructuralCollapse, AmbushDetected,
                                         -- QuestCompleted, BetrayalDetected, GiftReceived, TheftDetected,
                                         -- MagicWitnessed, RuneActivated, ProphecyFulfilled

    -- Contextual Modifiers (NULL = generic, applies to all)
    intensity TEXT,                      -- Mild, Moderate, Strong, Extreme, NULL

    prior_disposition TEXT,              -- Hostile, Unfriendly, Neutral, Friendly, Allied, NULL
                                         -- Affects how reaction is expressed

    action_tendency TEXT,                -- Approach, Flee, Attack, Assist, Ignore, Report, Investigate, Guard, NULL
                                         -- Likely follow-up behavior

    biome_context TEXT,                  -- Muspelheim, Niflheim, Alfheim, The_Roots, NULL (any)

    -- Template Text
    reaction_text TEXT NOT NULL,         -- Reaction dialogue/description with {Variables}
                                         -- Available: {NPCName}, {NPCArchetype}, {NPCSubtype}, {PlayerName},
                                         --            {PlayerClass}, {PlayerAction}, {TriggerEvent}, {ItemName},
                                         --            {AllyName}, {ReactionType}, {Intensity}, {Disposition},
                                         --            {Biome}, {Location}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Combat", "Social", "Achievement", "Betrayal"]

    -- Constraints
    CONSTRAINT valid_npc_archetype_reaction CHECK (npc_archetype IN (
        'Dvergr', 'Seidkona', 'Bandit', 'Raider', 'Merchant', 'Guard', 'Citizen', 'Forlorn'
    )),
    CONSTRAINT valid_reaction_type CHECK (reaction_type IN (
        'Surprised', 'Angry', 'Fearful', 'Relieved', 'Suspicious', 'Joyful',
        'Pained', 'Confused', 'Impressed', 'Disgusted', 'Grateful', 'Betrayed',
        'Proud', 'Curious', 'Resigned'
    )),
    CONSTRAINT valid_intensity CHECK (intensity IS NULL OR intensity IN (
        'Mild', 'Moderate', 'Strong', 'Extreme'
    )),
    CONSTRAINT valid_prior_disposition CHECK (prior_disposition IS NULL OR prior_disposition IN (
        'Hostile', 'Unfriendly', 'Neutral', 'Friendly', 'Allied'
    )),
    CONSTRAINT valid_action_tendency CHECK (action_tendency IS NULL OR action_tendency IN (
        'Approach', 'Flee', 'Attack', 'Assist', 'Ignore', 'Report', 'Investigate', 'Guard'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_npc_reaction_lookup
ON NPC_Reaction_Descriptors(npc_archetype, npc_subtype, reaction_type, trigger_event, prior_disposition);

-- ==============================================================================
-- SCHEMA COMPLETE
-- ==============================================================================
-- Next Steps:
-- 1. Load this schema into the database
-- 2. Populate with v0.38.11_npc_descriptors_barks_data.sql
-- 3. Implement DescriptorRepository_NPCFlavorExtensions.cs
-- 4. Implement NPCFlavorTextService.cs
-- 5. Integrate with TalkCommand, RoomPopulationService, CombatEngine
-- ==============================================================================
