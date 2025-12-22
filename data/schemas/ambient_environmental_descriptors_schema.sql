-- ==============================================================================
-- v0.38.13: Ambient Environmental Descriptors - Database Schema
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Comprehensive ambient sound, smell, and atmospheric detail library
-- Timeline: 8-10 hours
-- Philosophy: A living world that engages all senses
-- ==============================================================================

-- ==============================================================================
-- TABLE 1: Ambient_Sound_Descriptors
-- ==============================================================================
-- Purpose: Periodic ambient sounds that fire to make the world feel alive
-- Difference from v0.38.4: v0.38.4 = static room atmosphere, v0.38.13 = dynamic events
-- Usage: Fire periodically based on biome, time of day, and context
-- Integration: Used by AmbientEnvironmentService to generate periodic ambient events
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Ambient_Sound_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification (REQUIRED)
    biome TEXT NOT NULL,                 -- The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic

    sound_category TEXT NOT NULL,        -- Mechanical, Decay, Eerie, Creature, Fire, Ice,
                                         -- Wind, Glitch, Industrial, Elemental

    sound_subcategory TEXT NOT NULL,     -- ActiveMachinery, DecayingSystems, SmallCreatures,
                                         -- DistantThreats, Lava, Creaking, Howling, RealityDistortion,
                                         -- CursedChoir, TitanicMachinery, EmptySpaces

    -- Contextual Modifiers (NULL = generic, applies to all)
    time_of_day TEXT,                    -- Day, Night, NULL (any time)

    intensity TEXT,                      -- Subtle, Moderate, Oppressive, NULL (any intensity)

    location_context TEXT,               -- Corridor, Chamber, Exterior, Underground, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Ambient sound description
                                         -- Available: {Biome}, {LocationName}, {TimeOfDay},
                                         --            {DistanceDescriptor}, {IntensityModifier}
                                         -- Example: "The distant thrum of still-functioning machinery echoes through the halls."

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Mechanical", "Background", "Ominous"]

    -- Constraints
    CONSTRAINT valid_biome CHECK (biome IN (
        'The_Roots', 'Muspelheim', 'Niflheim', 'Alfheim', 'Jötunheim', 'Generic'
    )),
    CONSTRAINT valid_time_of_day CHECK (time_of_day IS NULL OR time_of_day IN (
        'Day', 'Night'
    )),
    CONSTRAINT valid_intensity CHECK (intensity IS NULL OR intensity IN (
        'Subtle', 'Moderate', 'Oppressive'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_ambient_sound_lookup
ON Ambient_Sound_Descriptors(biome, sound_category, time_of_day);

-- ==============================================================================
-- TABLE 2: Ambient_Smell_Descriptors
-- ==============================================================================
-- Purpose: Environmental smell descriptors that enhance immersion
-- Usage: Fire periodically or when entering new areas
-- Integration: Used by AmbientEnvironmentService to generate smell descriptions
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Ambient_Smell_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification (REQUIRED)
    biome TEXT NOT NULL,                 -- The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic

    smell_category TEXT NOT NULL,        -- Decay, Mechanical, Organic, Fire, Cold, Chemical,
                                         -- Paradoxical, Industrial, Psychic

    smell_subcategory TEXT NOT NULL,     -- Rust, Mildew, MachineOil, FungalGrowth, DeadFlesh,
                                         -- Sulfur, Brimstone, Ash, FrozenOzone, Sterile,
                                         -- Impossible, MetallicPsychic, Dust, Abandonment

    -- Contextual Modifiers (NULL = generic, applies to all)
    intensity TEXT,                      -- Subtle, Moderate, Overwhelming, NULL (any intensity)

    proximity TEXT,                      -- Immediate, Nearby, Distant, Pervasive, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Smell description
                                         -- Available: {Biome}, {LocationName}, {IntensityModifier},
                                         --            {SourceDescription}
                                         -- Example: "The smell of rust and corrosion, metallic and sharp."

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Unpleasant", "Industrial", "Organic"]

    -- Constraints
    CONSTRAINT valid_biome_smell CHECK (biome IN (
        'The_Roots', 'Muspelheim', 'Niflheim', 'Alfheim', 'Jötunheim', 'Generic'
    )),
    CONSTRAINT valid_intensity_smell CHECK (intensity IS NULL OR intensity IN (
        'Subtle', 'Moderate', 'Overwhelming'
    )),
    CONSTRAINT valid_proximity CHECK (proximity IS NULL OR proximity IN (
        'Immediate', 'Nearby', 'Distant', 'Pervasive'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_ambient_smell_lookup
ON Ambient_Smell_Descriptors(biome, smell_category, intensity);

-- ==============================================================================
-- TABLE 3: Ambient_Atmospheric_Detail_Descriptors
-- ==============================================================================
-- Purpose: Air quality, weather effects, and environmental conditions
-- Usage: Fire periodically to describe changing environmental conditions
-- Integration: Used by AmbientEnvironmentService for atmospheric changes
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Ambient_Atmospheric_Detail_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification (REQUIRED)
    biome TEXT NOT NULL,                 -- The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic

    detail_category TEXT NOT NULL,       -- AirQuality, Temperature, Humidity, Visibility,
                                         -- TimeOfDay, WeatherEffect

    detail_subcategory TEXT NOT NULL,    -- Thick, Breathable, Suffocating, Hot, Cold, Dry,
                                         -- Saturated, Clear, Obscured, DayTransition,
                                         -- NightTransition, StormEffect, CalmEffect

    -- Contextual Modifiers (NULL = generic, applies to all)
    time_of_day TEXT,                    -- Day, Night, Transition, NULL

    intensity TEXT,                      -- Subtle, Moderate, Oppressive, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Atmospheric detail description
                                         -- Available: {Biome}, {TimeOfDay}, {IntensityModifier},
                                         --            {TemperatureValue}, {VisibilityDistance}
                                         -- Example: "The air is thick with humidity. Every breath is heavy."

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Oppressive", "Physical", "Environmental"]

    -- Constraints
    CONSTRAINT valid_biome_atmo CHECK (biome IN (
        'The_Roots', 'Muspelheim', 'Niflheim', 'Alfheim', 'Jötunheim', 'Generic'
    )),
    CONSTRAINT valid_time_of_day_atmo CHECK (time_of_day IS NULL OR time_of_day IN (
        'Day', 'Night', 'Transition'
    )),
    CONSTRAINT valid_intensity_atmo CHECK (intensity IS NULL OR intensity IN (
        'Subtle', 'Moderate', 'Oppressive'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_ambient_atmospheric_lookup
ON Ambient_Atmospheric_Detail_Descriptors(biome, detail_category, time_of_day);

-- ==============================================================================
-- TABLE 4: Ambient_Background_Activity_Descriptors
-- ==============================================================================
-- Purpose: Background activity that suggests a larger world (distant combat, events, survivors)
-- Usage: Fire occasionally to create sense of living world beyond player's immediate location
-- Integration: Used by AmbientEnvironmentService for background world events
-- ==============================================================================

CREATE TABLE IF NOT EXISTS Ambient_Background_Activity_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Classification (REQUIRED)
    biome TEXT NOT NULL,                 -- The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic

    activity_category TEXT NOT NULL,     -- DistantCombat, EnvironmentalEvent, OtherSurvivors,
                                         -- CreatureActivity, StructuralEvent, RealityEvent

    activity_subcategory TEXT NOT NULL,  -- Weapons, Screams, Explosions, Collapse, Tremor,
                                         -- RealityTear, Voices, Caravan, Singing,
                                         -- Predators, Movement

    -- Contextual Modifiers (NULL = generic, applies to all)
    time_of_day TEXT,                    -- Day, Night, NULL

    distance TEXT,                       -- Near, Medium, Far, Uncertain, NULL

    threat_level TEXT,                   -- Safe, Concerning, Dangerous, NULL

    -- Template Text
    descriptor_text TEXT NOT NULL,       -- Background activity description
                                         -- Available: {Distance}, {Direction}, {ThreatLevel},
                                         --            {TimeOfDay}, {Biome}
                                         -- Example: "The clash of weapons echoes from somewhere distant."

    -- Metadata
    weight REAL DEFAULT 1.0,             -- Probability weight for selection
    is_active INTEGER DEFAULT 1,         -- Enable/disable descriptor
    tags TEXT,                           -- JSON array: ["Ominous", "Combat", "Distant"]

    -- Constraints
    CONSTRAINT valid_biome_bg CHECK (biome IN (
        'The_Roots', 'Muspelheim', 'Niflheim', 'Alfheim', 'Jötunheim', 'Generic'
    )),
    CONSTRAINT valid_time_of_day_bg CHECK (time_of_day IS NULL OR time_of_day IN (
        'Day', 'Night'
    )),
    CONSTRAINT valid_distance CHECK (distance IS NULL OR distance IN (
        'Near', 'Medium', 'Far', 'Uncertain'
    )),
    CONSTRAINT valid_threat_level CHECK (threat_level IS NULL OR threat_level IN (
        'Safe', 'Concerning', 'Dangerous'
    ))
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_ambient_background_lookup
ON Ambient_Background_Activity_Descriptors(biome, activity_category, distance);

-- ==============================================================================
-- SCHEMA COMPLETE
-- ==============================================================================
-- Next Steps:
-- 1. Load this schema into the database
-- 2. Populate with v0.38.13_ambient_environmental_descriptors_data.sql
-- 3. Implement C# descriptor classes in RuneAndRust.Core/AmbientFlavor/
-- 4. Implement DescriptorRepository_AmbientEnvironmentalExtensions.cs
-- 5. Implement AmbientEnvironmentService.cs
-- 6. Integrate with game loop for periodic ambient event generation
-- ==============================================================================
