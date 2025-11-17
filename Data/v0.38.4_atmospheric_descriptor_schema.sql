-- ============================================================
-- v0.38.4: Atmospheric Descriptor System - Schema
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Multi-sensory environmental storytelling
-- ============================================================

-- Atmospheric Descriptors Table
-- Stores individual sensory descriptors for lighting, sound, smell, temperature, psychic presence
CREATE TABLE IF NOT EXISTS Atmospheric_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    category TEXT NOT NULL,  -- 'Lighting', 'Sound', 'Smell', 'Temperature', 'PsychicPresence'
    intensity TEXT NOT NULL,  -- 'Subtle', 'Moderate', 'Oppressive'
    descriptor_text TEXT NOT NULL,
    biome_affinity TEXT,  -- NULL for generic, or specific biome (e.g., 'Muspelheim')
    tags TEXT,  -- JSON array of tags (e.g., '["Dim", "Generic"]')

    CHECK (category IN ('Lighting', 'Sound', 'Smell', 'Temperature', 'PsychicPresence')),
    CHECK (intensity IN ('Subtle', 'Moderate', 'Oppressive'))
);

-- Index for fast category/intensity queries
CREATE INDEX IF NOT EXISTS idx_atmospheric_category_intensity
ON Atmospheric_Descriptors(category, intensity);

-- Index for biome-specific queries
CREATE INDEX IF NOT EXISTS idx_atmospheric_biome
ON Atmospheric_Descriptors(biome_affinity);

-- Biome Atmosphere Profiles Table
-- Defines atmospheric composition for each biome
CREATE TABLE IF NOT EXISTS Biome_Atmosphere_Profiles (
    profile_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_name TEXT NOT NULL UNIQUE,
    lighting_descriptors TEXT NOT NULL,  -- JSON array of descriptor_ids
    sound_descriptors TEXT NOT NULL,
    smell_descriptors TEXT NOT NULL,
    temperature_descriptors TEXT NOT NULL,
    psychic_descriptors TEXT NOT NULL,
    composite_template TEXT NOT NULL,  -- Combined description template with placeholders

    -- Optional: Default intensity per biome
    default_intensity TEXT DEFAULT 'Moderate',

    CHECK (default_intensity IN ('Subtle', 'Moderate', 'Oppressive'))
);

-- ============================================================
-- Notes:
-- - Atmospheric_Descriptors: 150+ entries across 5 categories
-- - Biome_Atmosphere_Profiles: 5 entries (The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim)
-- - Tags enable flexible querying and filtering
-- - Biome affinity NULL = generic/universal descriptor
-- ============================================================
