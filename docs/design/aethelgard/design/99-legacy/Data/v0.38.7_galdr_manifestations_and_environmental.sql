-- ==============================================================================
-- v0.38.7: Galdr Manifestation & Environmental Reaction Descriptors
-- ==============================================================================
-- Purpose: Visual/sensory manifestations + biome environmental reactions
-- ==============================================================================

-- ==============================================================================
-- GALDR MANIFESTATION DESCRIPTORS - Visual Effects
-- ==============================================================================

INSERT INTO Galdr_Manifestation_Descriptors (rune_school, manifestation_type, element, power_level, descriptor_text, weight, tags)
VALUES
-- FEHU (Fire) - Visual
('Fehu', 'Visual', 'Fire', 'Moderate', 'The Fehu rune blazes crimson in the air, heat shimmering around it like a mirage.', 1.0, '["Fire", "Beautiful"]'),
('Fehu', 'Visual', 'Fire', 'Strong', 'Burning sigils hang in the air—the Fehu rune incarnate, wreathed in flame!', 1.0, '["Fire", "Dramatic"]'),
('Fehu', 'Visual', 'Fire', 'Devastating', 'Reality ignites! Fehu manifests as a pillar of fire, ancient and terrible!', 1.0, '["Fire", "Epic"]'),

-- FEHU (Fire) - Auditory
('Fehu', 'Auditory', 'Fire', 'Moderate', 'The roar of flames drowns out all other sound—crackling, consuming, alive.', 1.0, '["Fire", "Intense"]'),
('Fehu', 'Auditory', 'Fire', 'Strong', 'Fire roars like a forge of the gods, deafening in its fury!', 1.0, '["Fire", "Deafening"]'),

-- FEHU (Fire) - Tactile
('Fehu', 'Tactile', 'Fire', 'Moderate', 'Heat washes over everything in waves, the air itself burning.', 1.0, '["Fire", "Oppressive"]'),
('Fehu', 'Tactile', 'Fire', 'Strong', 'Searing heat radiates outward—exposed skin burns just from proximity!', 1.0, '["Fire", "Painful"]'),

-- THURISAZ (Ice) - Visual
('Thurisaz', 'Visual', 'Ice', 'Moderate', 'The Thurisaz rune appears in frost-white light, ice crystals forming around it.', 1.0, '["Ice", "Cold"]'),
('Thurisaz', 'Visual', 'Ice', 'Strong', 'Jagged ice formations erupt from nothing, crystalline and deadly!', 1.0, '["Ice", "Sharp"]'),
('Thurisaz', 'Visual', 'Ice', 'Devastating', 'Winter incarnate! Thurisaz manifests as a glacier of frozen fury!', 1.0, '["Ice", "Epic"]'),

-- THURISAZ (Ice) - Auditory
('Thurisaz', 'Auditory', 'Ice', 'Moderate', 'The sharp crack of ice forming fills the air—crystalline and clear.', 1.0, '["Ice", "Sharp"]'),
('Thurisaz', 'Auditory', 'Ice', 'Strong', 'Ice groans and shatters, the sound of winter''s wrath unleashed!', 1.0, '["Ice", "Violent"]'),

-- ANSUZ (Lightning) - Visual
('Ansuz', 'Visual', 'Lightning', 'Moderate', 'Electric blue light arcs between points, the Ansuz rune crackling with potential.', 1.0, '["Lightning", "Bright"]'),
('Ansuz', 'Visual', 'Lightning', 'Strong', 'Lightning chains across the space in dazzling brilliance—white-hot and blinding!', 1.0, '["Lightning", "Dazzling"]'),
('Ansuz', 'Visual', 'Lightning', 'Devastating', 'The rune becomes pure electricity—reality shocked into submission!', 1.0, '["Lightning", "Epic"]'),

-- ANSUZ (Lightning) - Auditory
('Ansuz', 'Auditory', 'Lightning', 'Moderate', 'Thunder cracks, echoing off surfaces with sharp reports.', 1.0, '["Lightning", "Loud"]'),
('Ansuz', 'Auditory', 'Lightning', 'Strong', 'Deafening thunder! The sound alone is a weapon!', 1.0, '["Lightning", "Deafening"]'),

-- BERKANAN (Healing) - Visual
('Berkanan', 'Visual', 'Healing', 'Moderate', 'Soft green light emanates from the Berkanan rune, gentle and nurturing.', 1.0, '["Healing", "Gentle"]'),
('Berkanan', 'Visual', 'Healing', 'Strong', 'The rune manifests as a flowering tree of light—life incarnate!', 1.0, '["Healing", "Beautiful"]'),

-- BERKANAN (Healing) - Tactile
('Berkanan', 'Tactile', 'Healing', 'Moderate', 'Warmth flows like honey, soothing and restorative.', 1.0, '["Healing", "Soothing"]'),
('Berkanan', 'Tactile', 'Healing', 'Strong', 'Life itself pours into you—every cell renewed, every wound mended!', 1.0, '["Healing", "Euphoric"]'),

-- TIWAZ (Protection) - Visual
('Tiwaz', 'Visual', NULL, 'Moderate', 'Golden light coalesces into runic barriers, shimmering with divine authority.', 1.0, '["Protection", "Holy"]'),
('Tiwaz', 'Visual', NULL, 'Strong', 'The Tiwaz rune blazes like a sun—wards manifesting in brilliant gold!', 1.0, '["Protection", "Radiant"]'),

-- NAUDIZ (Shadow/Draining) - Visual
('Naudiz', 'Visual', 'Shadow', 'Moderate', 'Shadow tendrils writhe from the Naudiz rune, hungry and grasping.', 1.0, '["Dark", "Sinister"]'),
('Naudiz', 'Visual', 'Shadow', 'Strong', 'Darkness incarnate—the rune becomes void, devouring light itself!', 1.0, '["Dark", "Horrifying"]'),

-- ISA (Ice/Stasis) - Supernatural
('Isa', 'Supernatural', 'Ice', 'Strong', 'Time itself seems to slow around the Isa rune—reality freezing into stillness.', 1.0, '["Ice", "Time", "Unnatural"]'),
('Isa', 'Supernatural', 'Ice', 'Devastating', 'The world stops! Everything crystallizes in perfect frozen moment!', 1.0, '["Ice", "Epic"]');

-- ==============================================================================
-- GALDR MANIFESTATION DESCRIPTORS - Runic Glyphs
-- ==============================================================================

INSERT INTO Galdr_Manifestation_Descriptors (rune_school, manifestation_type, element, descriptor_text, weight, tags)
VALUES
('Fehu', 'RunicGlyph', 'Fire', 'The Fehu rune (ᚠ) burns in the air, lines of crimson fire tracing ancient geometry.', 1.0, '["Rune", "Fire"]'),
('Thurisaz', 'RunicGlyph', 'Ice', 'The Thurisaz rune (ᚦ) materializes in frost—angular and sharp, like the thorn it represents.', 1.0, '["Rune", "Ice"]'),
('Ansuz', 'RunicGlyph', 'Lightning', 'The Ansuz rune (ᚨ) crackles into existence—electric blue lines of divine breath.', 1.0, '["Rune", "Lightning"]'),
('Berkanan', 'RunicGlyph', 'Healing', 'The Berkanan rune (ᛒ) glows with verdant light, a symbol of growth and renewal.', 1.0, '["Rune", "Healing"]'),
('Tiwaz', 'RunicGlyph', NULL, 'The Tiwaz rune (ᛏ) appears in golden radiance—the spear of the sky god, justice made manifest.', 1.0, '["Rune", "Protection"]'),
('Isa', 'RunicGlyph', 'Ice', 'The Isa rune (ᛁ) freezes into existence—a single vertical line, cold and absolute.', 1.0, '["Rune", "Ice"]'),
('Laguz', 'RunicGlyph', 'Water', 'The Laguz rune (ᛚ) flows like water, shimmering aquamarine curves.', 1.0, '["Rune", "Water"]');

-- ==============================================================================
-- BIOME-SPECIFIC MANIFESTATIONS
-- ==============================================================================

INSERT INTO Galdr_Manifestation_Descriptors (rune_school, manifestation_type, element, biome_name, descriptor_text, weight, tags)
VALUES
-- The Roots
('Fehu', 'Visual', 'Fire', 'The_Roots', 'Steam erupts from corroded pipes as fire ignites the humid air—reflections dancing in stagnant pools!', 0.8, '["Biome", "Atmospheric"]'),
('Thurisaz', 'Visual', 'Ice', 'The_Roots', 'Frost spreads across rust-covered metal with satisfying pops and cracks!', 0.8, '["Biome", "Atmospheric"]'),

-- Muspelheim
('Fehu', 'Visual', 'Fire', 'Muspelheim', 'Your flames merge with the ambient inferno—barely distinguishable from the hellscape around you!', 0.8, '["Biome", "Amplified"]'),
('Thurisaz', 'Visual', 'Ice', 'Muspelheim', 'Ice and fire clash spectacularly—steam exploding in massive plumes!', 0.8, '["Biome", "Conflicting"]'),

-- Alfheim
(NULL, 'Supernatural', NULL, 'Alfheim', 'Reality flickers—the magic was/is/will be manifest! All states exist simultaneously!', 0.8, '["Biome", "Paradoxical"]'),
(NULL, 'Supernatural', NULL, 'Alfheim', 'The Cursed Choir harmonizes—magic becomes song, reality becomes music, existence becomes rhythm!', 0.8, '["Biome", "Horrifying"]');

-- ==============================================================================
-- GALDR ENVIRONMENTAL REACTIONS
-- ==============================================================================

INSERT INTO Galdr_Environmental_Reactions (biome_name, reaction_type, rune_school, element, trigger_chance, descriptor_text, weight, tags)
VALUES
-- ==== THE ROOTS ====
('The_Roots', 'Resonance', 'Fehu', 'Fire', 0.30, 'Steam erupts from rusty pipes as your Galdr ignites the humid air!', 1.0, '["Atmospheric"]'),
('The_Roots', 'Resonance', 'Fehu', 'Fire', 0.30, 'Your flames reflect in pools of stagnant water, casting eerie shadows across corroded walls.', 1.0, '["Eerie"]'),
('The_Roots', 'Resonance', 'Thurisaz', 'Ice', 0.30, 'Frost spreads across corroded metal, cracking with satisfying pops!', 1.0, '["Satisfying"]'),
('The_Roots', 'Resonance', 'Thurisaz', 'Ice', 0.30, 'Your ice magic crystallizes the humid air, creating beautiful but deadly formations.', 1.0, '["Beautiful"]'),
('The_Roots', 'Interference', 'Ansuz', 'Lightning', 0.20, 'Lightning grounds through rusty infrastructure—your spell earthed before reaching full power!', 1.0, '["Weakened"]'),

-- ==== MUSPELHEIM ====
('Muspelheim', 'Amplification', 'Fehu', 'Fire', 0.40, 'Fire answers fire—your Galdr draws upon the volcanic fury all around!', 1.0, '["Amplified"]'),
('Muspelheim', 'Amplification', 'Fehu', 'Fire', 0.40, 'The ambient inferno surges into your spell—flames erupt with doubled intensity!', 1.0, '["Powerful"]'),
('Muspelheim', 'Interference', 'Thurisaz', 'Ice', 0.50, 'The Inferno fights your ice! Steam explodes as cold meets heat!', 1.0, '["Conflicting"]'),
('Muspelheim', 'Interference', 'Thurisaz', 'Ice', 0.50, 'Your frost magic struggles against overwhelming heat—the spell manifests weakly!', 1.0, '["Weakened"]'),
('Muspelheim', 'Resonance', 'Thurisaz', 'Ice', 0.15, 'Against all logic, frost manifests in this hellish place—a testament to your power!', 1.0, '["Impressive"]'),

-- ==== NIFLHEIM ====
('Niflheim', 'Amplification', 'Thurisaz', 'Ice', 0.40, 'Primal cold resonates with your Galdr—ice magic amplified by the frozen realm itself!', 1.0, '["Amplified"]'),
('Niflheim', 'Amplification', 'Isa', 'Ice', 0.40, 'Niflheim''s eternal ice empowers your stasis magic—time freezes absolutely!', 1.0, '["Powerful"]'),
('Niflheim', 'Interference', 'Fehu', 'Fire', 0.50, 'Cold devours your flames—the spell struggles against primal winter!', 1.0, '["Weakened"]'),
('Niflheim', 'Resonance', 'Ansuz', 'Lightning', 0.30, 'Lightning arcs between ice formations, amplified by crystalline structures!', 1.0, '["Amplified"]'),

-- ==== ALFHEIM ====
('Alfheim', 'Distortion', NULL, NULL, 0.35, 'The Cursed Choir harmonizes with your Galdr—or does it corrupt it?', 1.0, '["Unsettling"]'),
('Alfheim', 'Distortion', NULL, NULL, 0.35, 'Your rune flickers between states—was/is/will be! Reality rebels against your magic!', 1.0, '["Paradoxical"]'),
('Alfheim', 'Distortion', NULL, NULL, 0.35, 'Reality itself protests—yet the spell works anyway, defying logic!', 1.0, '["Defiant"]'),
('Alfheim', 'Amplification', NULL, NULL, 0.20, 'The Cursed Choir SINGS your Galdr! Reality-bending harmonics amplify your spell impossibly!', 1.0, '["Powerful", "Horrifying"]'),

-- ==== JOTUNHEIM ====
('Jotunheim', 'Resonance', NULL, NULL, 0.30, 'Stone resonates with your chant—ancient runes glowing faintly in response!', 1.0, '["Ancient"]'),
('Jotunheim', 'Resonance', NULL, NULL, 0.30, 'The bones of the giants themselves remember magic! Your Galdr echoes through ancient halls!', 1.0, '["Echoing", "Ancient"]'),
('Jotunheim', 'Interference', 'Mannaz', NULL, 0.25, 'Giant-magic clashes with human-runes! Mannaz struggles against primordial power!', 1.0, '["Conflicting"]');

-- ==============================================================================
-- Statistics Queries
-- ==============================================================================
-- SELECT rune_school, manifestation_type, COUNT(*) as count
-- FROM Galdr_Manifestation_Descriptors
-- GROUP BY rune_school, manifestation_type;
--
-- SELECT biome_name, reaction_type, COUNT(*) as count
-- FROM Galdr_Environmental_Reactions
-- GROUP BY biome_name, reaction_type;
