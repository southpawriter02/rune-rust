-- ==============================================================================
-- v0.38.7: Ability & Galdr Flavor Text - Miscast Descriptors
-- ==============================================================================
-- Purpose: Paradox, Blight corruption, and magical failure narratives
-- Coverage: Miscast types, severity levels, corruption sources
-- Philosophy: Runic Blight corrupts Galdr, causing paradoxical effects
-- ==============================================================================

-- ==============================================================================
-- BLIGHT CORRUPTION - FEHU (Fire becomes wrong)
-- ==============================================================================

INSERT INTO Galdr_Miscast_Descriptors (miscast_type, severity, rune_school, ability_name, corruption_source, descriptor_text, mechanical_effect, weight, tags)
VALUES
('BlightCorruption', 'Minor', 'Fehu', 'FlameBolt', 'RunicBlight', 'Your chant falters—the Blight warps Fehu''s meaning! Fire burns backward, cold as ice, wrong!', '{"damage": 3, "status": "Disoriented", "duration": 1}', 1.0, '["Corrupted", "Fire"]'),
('BlightCorruption', 'Moderate', 'Fehu', 'FlameBolt', 'RunicBlight', 'Fehu inverts! Fire erupts—but it''s cold flame, paradoxical and painful! You stagger as reality rebels!', '{"damage": 6, "status": "Corrupted", "duration": 2, "target": "Self"}', 1.0, '["Paradoxical", "Dangerous"]'),
('BlightCorruption', 'Severe', 'Fehu', NULL, 'RunicBlight', 'The Blight seizes your Galdr! Fehu becomes something else—fire that devours light, burns cold, screams with voices! You cry out!', '{"damage": 10, "status": "Blight_Touched", "duration": 3, "target": "Self"}', 1.0, '["Horrifying", "Severe"]');

-- ==============================================================================
-- PARADOX - Reality Rebels
-- ==============================================================================

INSERT INTO Galdr_Miscast_Descriptors (miscast_type, severity, rune_school, ability_name, descriptor_text, mechanical_effect, weight, tags)
VALUES
('Paradox', 'Minor', NULL, NULL, 'The rune you summon isn''t quite right—it flickers between states. The spell works, but reality protests.', '{"damage": 0, "random_effect": "minor_glitch"}', 1.0, '["Reality-Bending", "Subtle"]'),
('Paradox', 'Moderate', NULL, NULL, 'The rune you summon isn''t {Rune}—it''s something else, something that shouldn''t exist. Paradoxical {Element} erupts unpredictably!', '{"damage": 5, "random_target": true}', 1.0, '["Unpredictable"]'),
('Paradox', 'Severe', 'Thurisaz', 'FrostLance', 'RunicBlight', 'The Blight corrupts your chant! Thurisaz inverts—the frost burns, thorns grow from ice, nothing makes sense! Your mind reels!', '{"damage": 8, "status": "Confused", "duration": 2, "target": "Self"}', 1.0, '["Mind-Breaking", "Severe"]'),
('Paradox', 'Catastrophic', NULL, NULL, 'Reality tears! The rune exists and doesn''t exist—past and future collapse into now! You scream as impossible knowledge floods your mind!', '{"damage": 15, "status": "Broken", "duration": 3, "target": "Self", "permanent_corruption": 1}', 0.5, '["Catastrophic", "Horrifying"]');

-- ==============================================================================
-- BACKLASH - Magic Recoils
-- ==============================================================================

INSERT INTO Galdr_Miscast_Descriptors (miscast_type, severity, rune_school, ability_name, descriptor_text, mechanical_effect, weight, tags)
VALUES
('Backlash', 'Minor', NULL, NULL, 'The spell fizzles, the rune-light fading harmlessly. You feel drained.', '{"ap_loss": 5}', 1.0, '["Harmless", "Draining"]'),
('Backlash', 'Moderate', 'Fehu', 'FlameBolt', 'AethericOverload', 'The magic recoils! Fire lashes back at you! You stumble, singed and disoriented!', '{"damage": 5, "status": "Burning", "duration": 2, "target": "Self"}', 1.0, '["Painful"]'),
('Backlash', 'Moderate', 'Ansuz', 'LightningBolt', 'AethericOverload', 'Lightning arcs back through you! Your muscles spasm as the invisible fire courses through your body!', '{"damage": 6, "status": "Stunned", "duration": 1, "target": "Self"}', 1.0, '["Shocking"]'),
('Backlash', 'Severe', 'Naudiz', 'DrainLife', 'RunicBlight', 'Naudiz reverses! Instead of draining the enemy, it drains YOU! Shadow tendrils tear at your life force!', '{"damage": 10, "healing_to_enemy": 10, "target": "Self"}', 1.0, '["Reversed", "Dangerous"]'),
('Backlash', 'Catastrophic', NULL, NULL, 'The Aether itself rebels! Magic explodes outward in all directions—friend and foe alike caught in the blast!', '{"damage": 12, "target": "Area", "affects": "All"}', 0.5, '["Devastating", "Indiscriminate"]');

-- ==============================================================================
-- FIZZLE - Spell Failure
-- ==============================================================================

INSERT INTO Galdr_Miscast_Descriptors (miscast_type, severity, rune_school, ability_name, descriptor_text, mechanical_effect, weight, tags)
VALUES
('Fizzle', 'Minor', NULL, NULL, 'The spell fizzles as the {Rune} resists your invocation. Nothing happens.', '{"damage": 0, "ap_cost_wasted": true}', 1.5, '["Harmless", "Common"]'),
('Fizzle', 'Moderate', NULL, NULL, 'Your Galdr stutters mid-chant. The {Rune} manifests weakly, then collapses. You feel embarrassed.', '{"damage": 0, "ap_cost_wasted": true, "status": "Embarrassed", "duration": 1}', 1.0, '["Failure"]'),
('Fizzle', 'Minor', 'Berkanan', 'HealingChant', NULL, 'Berkanan flickers but won''t manifest. The healing fails—your wounds remain.', '{"damage": 0, "ap_cost_wasted": true}', 1.0, '["Disappointing"]');

-- ==============================================================================
-- WILD MAGIC - Uncontrolled Effects
-- ==============================================================================

INSERT INTO Galdr_Miscast_Descriptors (miscast_type, severity, rune_school, descriptor_text, mechanical_effect, weight, tags)
VALUES
('WildMagic', 'Moderate', NULL, 'Your Galdr spirals out of control! The {Rune} manifests—but not as you intended! Magic erupts wildly!', '{"random_effect": true, "unpredictable": true}', 1.0, '["Chaotic"]'),
('WildMagic', 'Moderate', 'Fehu', 'Fire explodes in random directions! Flames lance toward friend and foe alike—you''ve lost control!', '{"damage": 6, "target": "Random", "count": 3}', 1.0, '["Dangerous", "Chaotic"]'),
('WildMagic', 'Severe', 'Jera', 'Time magic goes haywire! Everything speeds up, slows down, reverses—temporal chaos!', '{"random_time_effects": true, "affects": "All", "duration": 3}', 0.7, '["Reality-Warping", "Severe"]');

-- ==============================================================================
-- ALFHEIM DISTORTION - Cursed Choir Interference
-- ==============================================================================

INSERT INTO Galdr_Miscast_Descriptors (miscast_type, severity, rune_school, biome_name, corruption_source, descriptor_text, mechanical_effect, weight, tags)
VALUES
('AlfheimDistortion', 'Minor', NULL, 'Alfheim', 'AlfheimCursedChoir', 'The Cursed Choir harmonizes with your Galdr—or does it corrupt it? The spell works, but feels... wrong.', '{"spell_works": true, "corruption": 1}', 1.0, '["Unsettling", "Biome"]'),
('AlfheimDistortion', 'Moderate', NULL, 'Alfheim', 'AlfheimCursedChoir', 'Your rune flickers between states—was that {Rune} or is it {Rune} or will it be {Rune}? Reality rebels, but the spell resolves... somehow.', '{"spell_works": true, "status": "Disoriented", "duration": 2, "corruption": 2}', 1.0, '["Paradoxical", "Biome"]'),
('AlfheimDistortion', 'Moderate', NULL, 'Alfheim', 'AlfheimCursedChoir', 'The Choir sings your Galdr backwards! Time inverts—the spell happened before you cast it!', '{"temporal_paradox": true, "random_outcome": true}', 1.0, '["Mind-Breaking", "Biome"]'),
('AlfheimDistortion', 'Severe', NULL, 'Alfheim', 'AlfheimCursedChoir', 'Reality fractures! Your spell exists in all states simultaneously—hit, miss, critical, fizzle—all at once! Your mind strains to comprehend!', '{"all_outcomes": true, "status": "Broken", "duration": 2, "corruption": 3}', 0.5, '["Catastrophic", "Reality-Breaking"]'),
('AlfheimDistortion', 'Catastrophic', NULL, 'Alfheim', 'AlfheimCursedChoir', 'The Cursed Choir BECOMES your Galdr! They sing through you—your voice merges with theirs! The spell is perfect and terrifying!', '{"amplified": true, "damage": 20, "corruption": 5, "permanent_effect": "Choir_Touched"}', 0.3, '["Horrifying", "Corrupting", "Powerful"]');

-- ==============================================================================
-- RUNIC INVERSION - Rune Meaning Corrupts
-- ==============================================================================

INSERT INTO Galdr_Miscast_Descriptors (miscast_type, severity, rune_school, descriptor_text, mechanical_effect, weight, tags)
VALUES
('RunicInversion', 'Moderate', 'Fehu', 'Fehu inverts! Instead of fire and wealth, you channel poverty and ice! The rune betrays you!', '{"element": "Ice", "inverted": true, "unexpected": true}', 1.0, '["Inverted"]'),
('RunicInversion', 'Moderate', 'Berkanan', 'Berkanan inverts! Instead of growth and healing, you channel decay and withering! Wounds worsen!', '{"damage": 8, "status": "Withering", "duration": 2, "target": "Self"}', 1.0, '["Reversed", "Dangerous"]'),
('RunicInversion', 'Moderate', 'Tiwaz', 'Tiwaz inverts! Instead of protection and justice, you channel chaos and vulnerability! Defenses crumble!', '{"remove_buffs": true, "status": "Vulnerable", "duration": 3, "target": "Self"}', 1.0, '["Catastrophic"]'),
('RunicInversion', 'Severe', 'Laguz', 'Laguz inverts! Instead of purification and water, you channel corruption and poison! Toxins flood your system!', '{"damage": 10, "status": "Poisoned", "duration": 3, "target": "Self"}', 1.0, '["Lethal", "Reversed"]');

-- ==============================================================================
-- BIOME-SPECIFIC MISCASTS
-- ==============================================================================

INSERT INTO Galdr_Miscast_Descriptors (miscast_type, severity, biome_name, corruption_source, descriptor_text, mechanical_effect, weight, tags)
VALUES
-- The Roots
('BlightCorruption', 'Moderate', 'The_Roots', 'RunicBlight', 'Rust-tainted Blight seeps into your Galdr! The rune corrodes, magic decaying mid-cast!', '{"damage": 5, "status": "Corroded", "duration": 2}', 0.8, '["Biome", "Rust"]'),

-- Muspelheim
('WildMagic', 'Moderate', 'Muspelheim', 'AethericOverload', 'The ambient inferno overwhelms your Galdr! Fire magic spirals out of control!', '{"damage": 8, "fire_explosion": true, "target": "Area"}', 0.8, '["Biome", "Fire"]'),

-- Niflheim
('BlightCorruption', 'Moderate', 'Niflheim', 'RunicBlight', 'Primal cold invades your Galdr! The rune freezes mid-formation, shattering into ice shards!', '{"damage": 6, "status": "Frostbitten", "duration": 2, "target": "Self"}', 0.8, '["Biome", "Ice"]'),

-- Jotunheim
('Paradox', 'Moderate', 'Jotunheim', NULL, 'Ancient giant-magic interferes! Your rune clashes with primordial power—reality groans!', '{"damage": 7, "random_effect": true}', 0.8, '["Biome", "Ancient"]');

-- ==============================================================================
-- MINOR COSMETIC MISCASTS (No Mechanical Effect)
-- ==============================================================================

INSERT INTO Galdr_Miscast_Descriptors (miscast_type, severity, rune_school, descriptor_text, mechanical_effect, weight, tags)
VALUES
('Fizzle', 'Minor', 'Fehu', 'Your flames sputter out, producing only a puff of smoke. Embarrassing.', '{"cosmetic": true}', 1.2, '["Harmless", "Cosmetic"]'),
('Fizzle', 'Minor', 'Ansuz', 'Your lightning fizzles into a weak static shock. You feel a tingle, nothing more.', '{"cosmetic": true}', 1.2, '["Harmless", "Cosmetic"]'),
('Fizzle', 'Minor', 'Thurisaz', 'Your frost lance becomes a snowflake. It drifts gently to the ground.', '{"cosmetic": true}', 1.2, '["Harmless", "Comical"]'),
('WildMagic', 'Minor', 'Berkanan', 'Healing magic misfires! Flowers sprout from your wounds instead of closing them. They''re pretty, at least.', '{"cosmetic": true, "flowers": true}', 1.0, '["Harmless", "Weird"]'),
('WildMagic', 'Minor', NULL, 'The {Rune} manifests as the wrong color. It works fine, but looks ridiculous.', '{"cosmetic": true, "color_swap": true}', 1.3, '["Harmless", "Comical"]');

-- ==============================================================================
-- Statistics Query
-- ==============================================================================
-- SELECT miscast_type, severity, COUNT(*) as count
-- FROM Galdr_Miscast_Descriptors
-- GROUP BY miscast_type, severity
-- ORDER BY miscast_type, severity;
