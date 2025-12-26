-- ============================================================
-- v0.38.4: Biome Atmosphere Profiles
-- Defines atmospheric composition for each biome
-- ============================================================

-- Profile 1: The Roots
-- Atmosphere: Sickly greenish light, dripping water, rust/mildew, cool/humid, moderate Blight
INSERT OR IGNORE INTO Biome_Atmosphere_Profiles (
    biome_name,
    lighting_descriptors,
    sound_descriptors,
    smell_descriptors,
    temperature_descriptors,
    psychic_descriptors,
    composite_template,
    default_intensity
) VALUES (
    'The_Roots',
    '[21, 22, 23, 24]',  -- Sickly greenish, corroded, flickering, bioluminescent
    '[37, 38, 39, 13, 14]',  -- Water drips, metal groans, hissing steam, water, condensation
    '[58, 59, 60, 23, 24, 25]',  -- Rust/mildew, damp corrosion, fungal, mold, mildew, musty
    '[76, 77, 30]',  -- Cool/humid, cool/damp, condensation
    '[146, 147, 139]',  -- Moderate Blight, lurking, faint unease
    '{Lighting}. {Sound}. {Smell}. {Temperature}. {Psychic}.',
    'Moderate'
);

-- Profile 2: Muspelheim
-- Atmosphere: Red-orange firelight, lava rumbles, brimstone, scorching heat, diminished Blight
INSERT OR IGNORE INTO Biome_Atmosphere_Profiles (
    biome_name,
    lighting_descriptors,
    sound_descriptors,
    smell_descriptors,
    temperature_descriptors,
    psychic_descriptors,
    composite_template,
    default_intensity
) VALUES (
    'Muspelheim',
    '[14, 15, 16, 17]',  -- Red-orange firelight, warm/molten, lava flares, red glow
    '[28, 29, 30, 31]',  -- Lava rumbles, steam hisses, lava/flames, crackling flames
    '[44, 45, 46, 47, 48]',  -- Sulfur/brimstone, volcanic, burning nostrils, brimstone, ash
    '[68, 69, 70, 8, 9]',  -- Scorching, intense heat, radiant warmth, hot, sweat
    '[136, 137]',  -- Diminished Blight, fire purges
    '{Lighting}. {Sound}. {Smell}. {Temperature}. {Psychic}.',
    'Oppressive'
);

-- Profile 3: Niflheim
-- Atmosphere: Blue-white frozen light, ice creaking, frozen ozone, bone-deep cold, frozen Blight
INSERT OR IGNORE INTO Biome_Atmosphere_Profiles (
    biome_name,
    lighting_descriptors,
    sound_descriptors,
    smell_descriptors,
    temperature_descriptors,
    psychic_descriptors,
    composite_template,
    default_intensity
) VALUES (
    'Niflheim',
    '[18, 19, 20, 21]',  -- Blue-white frozen, pale/lifeless, frost, crystalline
    '[32, 33, 34, 35]',  -- Ice creaks, wind howls, ice crystals, frozen cracks
    '[49, 50, 51]',  -- Crisp/cold, frozen/sterile, frozen ozone
    '[73, 74, 75, 12, 13, 14]',  -- Bone-deep cold, frost/breath, persistent chill, cold, frigid, frost
    '[138, 139]',  -- Frozen Blight, corruption sleeps
    '{Lighting}. {Sound}. {Smell}. {Temperature}. {Psychic}.',
    'Oppressive'
);

-- Profile 4: Alfheim
-- Atmosphere: Prismatic impossible light, Cursed Choir shriek, burnt reality, paradox temp, extreme Blight
INSERT OR IGNORE INTO Biome_Atmosphere_Profiles (
    biome_name,
    lighting_descriptors,
    sound_descriptors,
    smell_descriptors,
    temperature_descriptors,
    psychic_descriptors,
    composite_template,
    default_intensity
) VALUES (
    'Alfheim',
    '[25, 26, 27, 28]',  -- Impossible colors, chaotic spectrum, crystal refraction, painful colors
    '[24, 25, 26, 27]',  -- Cursed Choir shriek, reality frequency, whispers, echoes
    '[52, 53, 54]',  -- Burnt reality, impossible scent, ozone/wrongness
    '[78, 79]',  -- Paradox temp, burning/freezing simultaneously
    '[140, 141, 142, 119, 120, 121]',  -- Extreme Blight, epicenter, chaos, high corruption, contradictions, fraying
    '{Lighting}. {Sound}. {Smell}. {Temperature}. {Psychic}.',
    'Oppressive'
);

-- Profile 5: Jötunheim
-- Atmosphere: Dim industrial, distant machinery, rust/old industry, cool/still, industrial ghosts
INSERT OR IGNORE INTO Biome_Atmosphere_Profiles (
    biome_name,
    lighting_descriptors,
    sound_descriptors,
    smell_descriptors,
    temperature_descriptors,
    psychic_descriptors,
    composite_template,
    default_intensity
) VALUES (
    'Jötunheim',
    '[29, 30, 31, 1, 2]',  -- Dim industrial, emergency lighting, failing strips, dim, shadows
    '[40, 41, 42, 7, 8, 9]',  -- Distant machinery, echoing footsteps, titanic industry, mechanical groans, servos, grinding
    '[55, 56, 57, 19, 20]',  -- Rust/industrial, ancient lubricants, abandoned factories, rust, oxidized
    '[80, 81, 10, 11]',  -- Cool/still, stale/still, cool, pleasant chill
    '[143, 144, 125, 126]',  -- Titanic labor echoes, industrial ghosts, ghosts linger, watching eyes
    '{Lighting}. {Sound}. {Smell}. {Temperature}. {Psychic}.',
    'Moderate'
);

-- ============================================================
-- Notes on Descriptor ID Mapping:
-- These IDs correspond to the descriptor_id values in Atmospheric_Descriptors
-- The arrays contain multiple options for variety in generation
-- The composite_template uses placeholders that get replaced during generation
-- ============================================================
