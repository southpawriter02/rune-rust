-- ==============================================================================
-- v0.38.9: Perception & Examination Descriptors - Database Schema
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Comprehensive examination and perception descriptor library
-- Timeline: 10-12 hours
-- Philosophy: Reward player curiosity with layered environmental storytelling
-- ==============================================================================

-- ==============================================================================
-- TABLE 1: Examination_Descriptors
-- ==============================================================================
-- Purpose: Object examination with layered detail levels
-- Pattern: Follows v0.38.6 Combat and v0.38.7 Galdr descriptor patterns
-- Templates: Use {Variable} placeholders for dynamic content
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Examination_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    object_category TEXT NOT NULL,       -- Door, Machinery, Decorative, Structural,
                                         -- Container, Furniture, Environmental

    object_type TEXT,                    -- Specific type: LockedDoor, BlastDoor,
                                         -- ServitorCorpse, AncientConsole, WallInscription,
                                         -- Skeleton, SupportPillar, ControlPanel, etc.

    -- Detail Level (Layered Examination System)
    detail_level TEXT NOT NULL,          -- Cursory (no check required)
                                         -- Detailed (WITS DC 12)
                                         -- Expert (WITS DC 18)

    -- Context
    biome_name TEXT,                     -- The_Roots, Muspelheim, Niflheim, Alfheim,
                                         -- Jotunheim, NULL (any biome)

    object_state TEXT,                   -- Intact, Damaged, Destroyed, Locked, Sealed,
                                         -- Operational, Dormant, NULL (any state)

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Examination narrative with {Variables}

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Lore", "Technical", "Historical"]

    -- Constraints
    CONSTRAINT valid_object_category CHECK (object_category IN (
        'Door', 'Machinery', 'Decorative', 'Structural',
        'Container', 'Furniture', 'Environmental', 'Corpse'
    )),
    CONSTRAINT valid_detail_level CHECK (detail_level IN (
        'Cursory', 'Detailed', 'Expert'
    ))
);

-- ==============================================================================
-- TABLE 2: Perception_Check_Descriptors
-- ==============================================================================
-- Purpose: Hidden element detection (traps, secrets, caches)
-- Usage: Triggered on successful Perception/WITS checks
-- Integration: Rewards high WITS characters with world knowledge
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Perception_Check_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    detection_type TEXT NOT NULL,        -- HiddenTrap, SecretDoor, HiddenCache,
                                         -- AmbushPoint, WeakStructure, RunicInscription

    success_level TEXT NOT NULL,         -- Success (basic DC)
                                         -- ExpertSuccess (high DC)

    -- Difficulty Context
    difficulty_class INTEGER,            -- DC for this detection (12, 15, 18, 20, 22, etc.)

    -- Biome Context
    biome_name TEXT,                     -- Biome-specific detection cues
                                         -- NULL (any biome)

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Detection success narrative

    -- Additional Information (for Expert success)
    expert_insight TEXT,                 -- Extra lore/context for expert-level success
                                         -- NULL for basic success

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_detection_type CHECK (detection_type IN (
        'HiddenTrap', 'SecretDoor', 'HiddenCache', 'AmbushPoint',
        'WeakStructure', 'RunicInscription', 'RecentActivity'
    )),
    CONSTRAINT valid_success_level CHECK (success_level IN (
        'Success', 'ExpertSuccess'
    ))
);

-- ==============================================================================
-- TABLE 3: Flora_Descriptors
-- ==============================================================================
-- Purpose: Plants, fungi, mosses, crystalline growths
-- Usage: Adds living world feeling, biome atmosphere
-- Integration: Examine flora, alchemy components, world building
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Flora_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    flora_name TEXT NOT NULL,            -- Display name (e.g., "Luminous Shelf Fungus")
    flora_type TEXT NOT NULL,            -- Fungus, Moss, Lichen, Plant, CrystallineGrowth,
                                         -- Vine, Flower

    -- Detail Level
    detail_level TEXT NOT NULL,          -- Cursory, Detailed, Expert

    -- Biome Classification
    biome_name TEXT NOT NULL,            -- Biome where this flora appears

    -- Properties
    is_harvestable INTEGER DEFAULT 0,    -- Can be harvested for alchemy
    is_dangerous INTEGER DEFAULT 0,      -- Harmful to touch/proximity
    alchemy_use TEXT,                    -- Alchemical applications (NULL if not harvestable)

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Flora description with {Variables}

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_flora_type CHECK (flora_type IN (
        'Fungus', 'Moss', 'Lichen', 'Plant', 'CrystallineGrowth',
        'Vine', 'Flower', 'Algae', 'Spore'
    )),
    CONSTRAINT valid_detail_level CHECK (detail_level IN (
        'Cursory', 'Detailed', 'Expert'
    ))
);

-- ==============================================================================
-- TABLE 4: Fauna_Descriptors
-- ==============================================================================
-- Purpose: Ambient creatures, critters, non-combat animals
-- Usage: Living world atmosphere, environmental storytelling
-- Integration: Observation checks, world immersion
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Fauna_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    creature_name TEXT NOT NULL,         -- Display name (e.g., "Cave Rat", "Rust Beetle")
    creature_type TEXT NOT NULL,         -- Rodent, Insect, Bird, Reptile, AmbientCreature

    -- Observation Context
    observation_type TEXT NOT NULL,      -- Sighting, Sound, Traces, ExpertObservation

    -- Biome Context
    biome_name TEXT,                     -- Where this fauna appears
                                         -- NULL (multiple biomes)

    -- Behavior
    is_hostile INTEGER DEFAULT 0,        -- Non-hostile by default (ambient creatures)
    ecological_role TEXT,                -- Predator, Scavenger, Herbivore, Decomposer,
                                         -- BlightAdapted, NULL

    -- Expert Knowledge
    expert_insight TEXT,                 -- Additional context for expert observation
                                         -- NULL for basic sightings

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Creature observation narrative

    -- Metadata
    weight REAL DEFAULT 1.0,
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_creature_type CHECK (creature_type IN (
        'Rodent', 'Insect', 'Bird', 'Reptile', 'Amphibian',
        'AmbientCreature', 'BlightCreature', 'Construct'
    )),
    CONSTRAINT valid_observation_type CHECK (observation_type IN (
        'Sighting', 'Sound', 'Traces', 'ExpertObservation'
    ))
);

-- ==============================================================================
-- TABLE 5: Examination_Lore_Fragments
-- ==============================================================================
-- Purpose: Lore nuggets revealed through expert examination
-- Usage: Connects examination to world history, rewards thorough players
-- Integration: Expert-level examinations unlock lore entries
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Examination_Lore_Fragments (
    lore_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification
    lore_category TEXT NOT NULL,         -- HistoricalEvent, TechnicalKnowledge,
                                         -- CulturalArtifact, BlightOrigin,
                                         -- PreBlightSociety, EvacuationRecord

    -- Associated Examination
    related_object_type TEXT,            -- What object reveals this lore
    required_detail_level TEXT,          -- Cursory, Detailed, Expert

    -- Biome Context
    biome_name TEXT,                     -- Where this lore is found
                                         -- NULL (multiple locations)

    -- Lore Content
    lore_title TEXT NOT NULL,            -- Short title
    lore_text TEXT NOT NULL,             -- The actual lore content

    -- Metadata
    is_active INTEGER DEFAULT 1,
    tags TEXT,

    -- Constraints
    CONSTRAINT valid_lore_category CHECK (lore_category IN (
        'HistoricalEvent', 'TechnicalKnowledge', 'CulturalArtifact',
        'BlightOrigin', 'PreBlightSociety', 'EvacuationRecord', 'ReligiousText'
    )),
    CONSTRAINT valid_required_level CHECK (required_detail_level IN (
        'Cursory', 'Detailed', 'Expert'
    ))
);

-- ==============================================================================
-- INDEXES FOR PERFORMANCE
-- ==============================================================================

-- Examination_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_examination_category
    ON Examination_Descriptors(object_category);
CREATE INDEX IF NOT EXISTS idx_examination_type
    ON Examination_Descriptors(object_type);
CREATE INDEX IF NOT EXISTS idx_examination_detail_level
    ON Examination_Descriptors(detail_level);
CREATE INDEX IF NOT EXISTS idx_examination_biome
    ON Examination_Descriptors(biome_name);

-- Composite index for common queries
CREATE INDEX IF NOT EXISTS idx_examination_category_type_level
    ON Examination_Descriptors(object_category, object_type, detail_level);

-- Perception_Check_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_perception_detection_type
    ON Perception_Check_Descriptors(detection_type);
CREATE INDEX IF NOT EXISTS idx_perception_success_level
    ON Perception_Check_Descriptors(success_level);
CREATE INDEX IF NOT EXISTS idx_perception_difficulty
    ON Perception_Check_Descriptors(difficulty_class);

-- Flora_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_flora_name
    ON Flora_Descriptors(flora_name);
CREATE INDEX IF NOT EXISTS idx_flora_biome
    ON Flora_Descriptors(biome_name);
CREATE INDEX IF NOT EXISTS idx_flora_type
    ON Flora_Descriptors(flora_type);
CREATE INDEX IF NOT EXISTS idx_flora_detail_level
    ON Flora_Descriptors(detail_level);

-- Fauna_Descriptors indexes
CREATE INDEX IF NOT EXISTS idx_fauna_name
    ON Fauna_Descriptors(creature_name);
CREATE INDEX IF NOT EXISTS idx_fauna_biome
    ON Fauna_Descriptors(biome_name);
CREATE INDEX IF NOT EXISTS idx_fauna_observation
    ON Fauna_Descriptors(observation_type);

-- Examination_Lore_Fragments indexes
CREATE INDEX IF NOT EXISTS idx_lore_category
    ON Examination_Lore_Fragments(lore_category);
CREATE INDEX IF NOT EXISTS idx_lore_object_type
    ON Examination_Lore_Fragments(related_object_type);
CREATE INDEX IF NOT EXISTS idx_lore_biome
    ON Examination_Lore_Fragments(biome_name);

-- ==============================================================================
-- TEMPLATE VARIABLE REFERENCE
-- ==============================================================================
-- Available template variables for descriptor_text fields:
--
-- OBJECT VARIABLES:
--   {Object}              - Object name/type
--   {ObjectState}         - Current state (locked, damaged, etc.)
--   {Material}            - Material composition
--   {Age}                 - Approximate age
--
-- LOCATION VARIABLES:
--   {Biome}               - Current biome name
--   {Location}            - Specific location within room
--   {Direction}           - Directional reference (north, above, below, etc.)
--
-- CHARACTER VARIABLES:
--   {Player}              - Player name
--   {WITS}                - Player WITS score (for skill checks)
--
-- LORE VARIABLES:
--   {Faction}             - Associated faction (Jötun, Dvergr, etc.)
--   {Era}                 - Historical era (Pre-Blight, Evacuation, etc.)
--   {Manufacturer}        - Creator/manufacturer
--
-- PERCEPTION VARIABLES:
--   {HiddenElement}       - What was discovered
--   {DetectionCue}        - How it was noticed (dust pattern, air current, etc.)
--
-- FLORA/FAUNA VARIABLES:
--   {Species}             - Scientific/common name
--   {Behavior}            - Creature behavior
--   {AlchemyUse}          - Alchemical application
--
-- ==============================================================================
-- USAGE EXAMPLE
-- ==============================================================================
-- Query for object examination (Detailed level, Door category):
--
-- SELECT descriptor_text
-- FROM Examination_Descriptors
-- WHERE object_category = 'Door'
--   AND object_type = 'LockedDoor'
--   AND detail_level = 'Detailed'
--   AND is_active = 1
-- ORDER BY RANDOM()
-- LIMIT 1;
--
-- Result: "A heavy iron door reinforced with Jötun metalwork. The lock mechanism
--          is complex—Jötun engineering, designed to resist forced entry..."
--
-- ==============================================================================
-- Query for perception check (Hidden Trap, DC 15):
--
-- SELECT descriptor_text, expert_insight
-- FROM Perception_Check_Descriptors
-- WHERE detection_type = 'HiddenTrap'
--   AND difficulty_class <= 18
--   AND is_active = 1
-- ORDER BY difficulty_class DESC
-- LIMIT 1;
--
-- Result: "Your trained eye catches a discrepancy—a pressure plate,
--          barely visible beneath the dust!"
--
-- ==============================================================================
-- Query for flora examination (Luminous Shelf Fungus, Expert level):
--
-- SELECT descriptor_text, alchemy_use
-- FROM Flora_Descriptors
-- WHERE flora_name = 'Luminous Shelf Fungus'
--   AND detail_level = 'Expert'
--   AND is_active = 1
-- LIMIT 1;
--
-- Result: "Luminous Shelf Fungus, Fungus lucidus. It thrives in high-humidity,
--          low-light environments... Properly prepared, it's a potent alchemical
--          reagent for light, vision, and consciousness-altering potions."
--
-- ==============================================================================
