-- ==============================================================================
-- v0.38.7: Ability & Galdr Flavor Text - Galdr Action Descriptors
-- ==============================================================================
-- Purpose: Galdr casting sequences, invocations, chanting patterns
-- Coverage: 50+ descriptors across all rune schools and success levels
-- Pattern: Invocation → Rune Manifestation → Discharge → Aftermath
-- ==============================================================================

-- ==============================================================================
-- FEHU (Fire) - FlameBolt
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
-- Minor Success (1-2 successes)
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'MinorSuccess', 'You chant the Fehu rune, its fiery syllables harsh on your tongue. Flames sputter weakly from your palm, barely reaching the {Target}.', 1.0, '["Verbose", "Descriptive"]'),
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'MinorSuccess', 'You intone Fehu''s name—a small gout of fire leaps forth, more smoke than flame.', 1.0, '["Concise"]'),
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'MinorSuccess', 'Your Galdr falters. The Fehu rune flickers in the air, and weak flames dance toward the {Target}.', 1.0, '["Dramatic"]'),

-- Solid Success (3-4 successes)
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'SolidSuccess', 'You sing the Fehu rune, its ancient will igniting the air. A bolt of flame streaks toward the {Target}, trailing embers.', 1.0, '["Verbose", "Descriptive"]'),
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'SolidSuccess', '"Fehu! Fehu! Fehu!" The chant builds—fire erupts from your outstretched hand in a roaring lance!', 1.0, '["Dramatic"]'),
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'SolidSuccess', 'You invoke Fehu with steady voice. The rune blazes crimson, and fire answers your call!', 1.0, '["Concise"]'),
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'SolidSuccess', 'Your Galdr resonates with primal heat. Fehu manifests in burning light, and flame leaps toward the {Target}!', 1.0, '["Mystical"]'),

-- Exceptional Success (5+ successes)
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'ExceptionalSuccess', 'You invoke Fehu with perfect resonance—the rune blazes in the air before you! A devastating torrent of flame engulfs the {Target}!', 1.0, '["Dramatic"]'),
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'ExceptionalSuccess', 'Your Galdr reaches a crescendo. Fehu manifests as a burning sigil, and reality itself ignites! The {Target} is consumed by primal fire!', 1.0, '["Epic", "Verbose"]'),
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'ExceptionalSuccess', 'You channel Fehu''s full fury! The rune erupts in searing light—fire incarnate roars toward the {Target}!', 1.0, '["Concise", "Powerful"]');

-- ==============================================================================
-- FEHU (Fire) - InfernoWard
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Activation', 'Fehu', 'InfernoWard', 'SolidSuccess', 'You trace Fehu in the air, chanting its protective aspects. Fire coalesces into a whirling barrier around you.', 1.0, '["Defensive"]'),
('GaldrCasting', 'Activation', 'Fehu', 'InfernoWard', 'SolidSuccess', 'Your Galdr weaves Fehu into a shield—flames dance across your skin without burning.', 1.0, '["Mystical"]'),
('GaldrCasting', 'Activation', 'Fehu', 'InfernoWard', 'ExceptionalSuccess', 'You invoke Fehu''s warding strength! A corona of protective flame erupts around you, pulsing with defensive fury!', 1.0, '["Epic"]'),
('GaldrCasting', 'EffectTrigger', 'Fehu', 'InfernoWard', NULL, 'The Inferno Ward flares as the {Enemy} strikes, flames licking outward to punish the assault!', 1.0, '["Active", "Reactive"]'),
('GaldrCasting', 'EffectTrigger', 'Fehu', 'InfernoWard', NULL, 'Your fiery barrier pulses with each attack, the {Enemy} recoiling from the heat!', 1.0, '["Dramatic"]');

-- ==============================================================================
-- THURISAZ (Ice/Thorns) - FrostLance
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
-- Minor Success
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'MinorSuccess', 'You chant Thurisaz, the giant''s rune. Ice crystals form sluggishly, creating a weak shard that wobbles toward the {Target}.', 1.0, '["Verbose"]'),
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'MinorSuccess', 'Your Galdr summons Thurisaz. Frost crackles weakly in the air, barely forming a spear.', 1.0, '["Concise"]'),

-- Solid Success
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'SolidSuccess', 'You chant Thurisaz, the giant''s rune. Ice crystallizes from nothing, forming a deadly spear that hurtles toward the {Target}!', 1.0, '["Descriptive"]'),
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'SolidSuccess', 'Your Galdr echoes with Thurisaz''s harsh consonants—frost erupts, flash-freezing the very air into a piercing lance!', 1.0, '["Dramatic"]'),
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'SolidSuccess', 'You invoke the thorn-rune! Ice answers, jagged and cruel, streaking toward the {Target}!', 1.0, '["Concise", "Brutal"]'),

-- Exceptional Success
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'ExceptionalSuccess', 'You sing Thurisaz with terrible clarity. The rune appears as frozen fire—the lance that forms is less ice than winter''s wrath made solid!', 1.0, '["Epic", "Poetic"]'),
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'ExceptionalSuccess', 'Thurisaz roars in your voice! Reality freezes—a massive ice spear erupts forth, crackling with supernatural cold!', 1.0, '["Powerful"]');

-- ==============================================================================
-- ANSUZ (Wind/Lightning) - LightningBolt
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
-- Minor Success
('GaldrCasting', 'Invocation', 'Ansuz', 'LightningBolt', 'MinorSuccess', 'You invoke Ansuz, the breath of gods. Faint electricity crackles, barely touching the {Target}.', 1.0, '["Concise"]'),
('GaldrCasting', 'Invocation', 'Ansuz', 'LightningBolt', 'MinorSuccess', 'Your Galdr calls upon Ansuz—weak sparks leap from your fingertips, more light than lightning.', 1.0, '["Descriptive"]'),

-- Solid Success
('GaldrCasting', 'Invocation', 'Ansuz', 'LightningBolt', 'SolidSuccess', 'You invoke Ansuz, the breath of gods. Lightning arcs from your fingertips with a thunderous crack!', 1.0, '["Dramatic"]'),
('GaldrCasting', 'Invocation', 'Ansuz', 'LightningBolt', 'SolidSuccess', 'Your Galdr calls upon Ansuz—electricity crackles through the air, coalescing into a bolt that strikes the {Target}!', 1.0, '["Descriptive"]'),
('GaldrCasting', 'Invocation', 'Ansuz', 'LightningBolt', 'SolidSuccess', 'You shout Ansuz! The air splits—lightning answers, brilliant and deadly!', 1.0, '["Concise"]'),

-- Exceptional Success
('GaldrCasting', 'Invocation', 'Ansuz', 'LightningBolt', 'ExceptionalSuccess', 'You shout Ansuz with divine authority! The rune blazes white-hot—lightning chains between targets, reality itself shocked by the strike!', 1.0, '["Epic"]'),
('GaldrCasting', 'Invocation', 'Ansuz', 'LightningBolt', 'ExceptionalSuccess', 'Your Galdr becomes thunder itself! Ansuz manifests as pure storm-fury, arcing through the {Target} and beyond!', 1.0, '["Powerful", "Dramatic"]');

-- ==============================================================================
-- BERKANAN (Healing) - HealingChant
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
-- Minor Success
('GaldrCasting', 'Invocation', 'Berkanan', 'HealingChant', 'MinorSuccess', 'You sing Berkanan''s gentle syllables. Faint warmth flows through you—minor wounds begin to close.', 1.0, '["Gentle"]'),
('GaldrCasting', 'Invocation', 'Berkanan', 'HealingChant', 'MinorSuccess', 'Your Galdr whispers of growth. Berkanan flickers weakly, soothing pain but healing little.', 1.0, '["Subtle"]'),

-- Solid Success
('GaldrCasting', 'Invocation', 'Berkanan', 'HealingChant', 'SolidSuccess', 'You sing Berkanan''s gentle syllables, the growth-rune. Warmth flows through you as flesh begins to knit.', 1.0, '["Descriptive"]'),
('GaldrCasting', 'Invocation', 'Berkanan', 'HealingChant', 'SolidSuccess', 'Your Galdr resonates with Berkanan''s nurturing light. Green light bathes your wounds—skin closes, bones realign.', 1.0, '["Mystical"]'),
('GaldrCasting', 'Invocation', 'Berkanan', 'HealingChant', 'SolidSuccess', 'You chant Berkanan with reverence. Life itself answers—healing light floods through you!', 1.0, '["Reverent"]'),

-- Exceptional Success
('GaldrCasting', 'Invocation', 'Berkanan', 'HealingChant', 'ExceptionalSuccess', 'You invoke Berkanan with perfect pitch. The rune manifests as a flowering tree—life itself pours into you, mending even grievous injuries!', 1.0, '["Epic", "Beautiful"]'),
('GaldrCasting', 'Invocation', 'Berkanan', 'HealingChant', 'ExceptionalSuccess', 'Your Galdr becomes a song of renewal! Berkanan blazes with verdant light—wounds vanish, vitality restored!', 1.0, '["Powerful"]');

-- ==============================================================================
-- TIWAZ (Protection) - RuneWard
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Activation', 'Tiwaz', 'RuneWard', 'SolidSuccess', 'You invoke Tiwaz, the justice-rune. Golden light coalesces into protective wards around you.', 1.0, '["Defensive"]'),
('GaldrCasting', 'Activation', 'Tiwaz', 'RuneWard', 'SolidSuccess', 'Your Galdr weaves Tiwaz into reality. The air shimmers as divine barriers materialize!', 1.0, '["Mystical"]'),
('GaldrCasting', 'Activation', 'Tiwaz', 'RuneWard', 'ExceptionalSuccess', 'You chant Tiwaz with unwavering conviction! The rune blazes in golden brilliance—unbreakable wards surround you!', 1.0, '["Epic"]'),
('GaldrCasting', 'EffectTrigger', 'Tiwaz', 'RuneWard', NULL, 'The Rune Ward flares golden as the {Enemy}''s attack strikes—and shatters against divine protection!', 1.0, '["Reactive"]'),
('GaldrCasting', 'EffectTrigger', 'Tiwaz', 'RuneWard', NULL, 'Tiwaz''s wards pulse with righteous fury, repelling the {Enemy}''s assault!', 1.0, '["Dramatic"]');

-- ==============================================================================
-- HAGALAZ (Destructive Ice) - Hailstorm
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Invocation', 'Hagalaz', 'Hailstorm', 'MinorSuccess', 'You chant Hagalaz, the hail-rune. Small ice shards form, pelting the {Target} weakly.', 1.0, '["Concise"]'),
('GaldrCasting', 'Invocation', 'Hagalaz', 'Hailstorm', 'SolidSuccess', 'Your Galdr summons Hagalaz''s fury! Ice shards erupt in a deadly storm, battering the {Target}!', 1.0, '["Dramatic"]'),
('GaldrCasting', 'Invocation', 'Hagalaz', 'Hailstorm', 'ExceptionalSuccess', 'You invoke Hagalaz with destructive might! The rune manifests as a blizzard of razor ice—chaos incarnate!', 1.0, '["Epic", "Violent"]');

-- ==============================================================================
-- NAUDIZ (Draining/Weakening) - DrainLife
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Invocation', 'Naudiz', 'DrainLife', 'MinorSuccess', 'You whisper Naudiz, the need-rune. Faint tendrils of shadow reach toward the {Target}, draining weakly.', 1.0, '["Dark", "Subtle"]'),
('GaldrCasting', 'Invocation', 'Naudiz', 'DrainLife', 'SolidSuccess', 'Your Galdr invokes Naudiz, the rune of need. Shadow tendrils erupt, siphoning life from the {Target}!', 1.0, '["Dark", "Dramatic"]'),
('GaldrCasting', 'Invocation', 'Naudiz', 'DrainLife', 'ExceptionalSuccess', 'You chant Naudiz with desperate hunger! The rune manifests as writhing shadow—life force floods from the {Target} into you!', 1.0, '["Dark", "Powerful"]');

-- ==============================================================================
-- ISA (Stasis/Freezing) - FrozenTime
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Invocation', 'Isa', 'FrozenTime', 'SolidSuccess', 'You intone Isa, the ice-rune. Time itself seems to slow as frost spreads across the {Target}!', 1.0, '["Mystical"]'),
('GaldrCasting', 'Invocation', 'Isa', 'FrozenTime', 'ExceptionalSuccess', 'Your Galdr bends reality around Isa! The rune blazes cold—the {Target} freezes mid-motion, trapped in crystalline stasis!', 1.0, '["Epic", "Reality-Bending"]');

-- ==============================================================================
-- JERA (Time/Growth) - Accelerate
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Activation', 'Jera', 'Accelerate', 'SolidSuccess', 'You chant Jera, the year-rune. Time flows faster around you—your movements blur with unnatural speed!', 1.0, '["Mystical", "Utility"]'),
('GaldrCasting', 'Activation', 'Jera', 'Accelerate', 'ExceptionalSuccess', 'Your Galdr warps Jera''s temporal essence! The rune manifests as spiraling light—time bends to your will!', 1.0, '["Epic"]');

-- ==============================================================================
-- MANNAZ (Enhancement) - Empower
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Activation', 'Mannaz', 'Empower', 'SolidSuccess', 'You invoke Mannaz, the human-rune. Violet fire flows through you—your capabilities surge!', 1.0, '["Enhancement"]'),
('GaldrCasting', 'Activation', 'Mannaz', 'Empower', 'ExceptionalSuccess', 'Your Galdr resonates with Mannaz perfectly! The rune blazes—human potential unlocked, strength flooding every fiber!', 1.0, '["Epic", "Empowering"]');

-- ==============================================================================
-- LAGUZ (Water/Purification) - CleansingWave
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Invocation', 'Laguz', 'CleansingWave', 'SolidSuccess', 'You sing Laguz, the water-rune. Purifying waves wash over you, cleansing corruption and poison.', 1.0, '["Purifying"]'),
('GaldrCasting', 'Invocation', 'Laguz', 'CleansingWave', 'ExceptionalSuccess', 'Your Galdr channels Laguz''s full purifying might! The rune manifests as cascading water—all corruption washes away!', 1.0, '["Epic", "Cleansing"]');

-- ==============================================================================
-- RAIDO (Movement/Speed) - Swift Journey
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Activation', 'Raido', 'SwiftJourney', 'SolidSuccess', 'You invoke Raido, the journey-rune. Silver light surrounds you—your speed doubles, feet barely touching ground!', 1.0, '["Utility", "Movement"]'),
('GaldrCasting', 'Activation', 'Raido', 'SwiftJourney', 'ExceptionalSuccess', 'Your Galdr bends space around Raido! The rune blazes silver—you move like wind itself!', 1.0, '["Epic"]');

-- ==============================================================================
-- BIOME-SPECIFIC GALDR (The Roots)
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, biome_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'The_Roots', 'SolidSuccess', 'You chant Fehu—steam erupts from rusty pipes as your flames ignite the humid air, fire reflecting in pools of stagnant water!', 0.8, '["Biome-Specific", "Atmospheric"]'),
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'The_Roots', 'SolidSuccess', 'Your Galdr summons Thurisaz! Frost spreads across corroded metal with satisfying pops, crystallizing the decay!', 0.8, '["Biome-Specific", "Atmospheric"]');

-- ==============================================================================
-- BIOME-SPECIFIC GALDR (Muspelheim)
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, biome_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Invocation', 'Fehu', 'FlameBolt', 'Muspelheim', 'SolidSuccess', 'Your flames seem pitiful compared to the ambient inferno—but fire answers fire! Your Galdr draws upon the volcanic fury all around!', 0.8, '["Biome-Specific", "Amplified"]'),
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'Muspelheim', 'SolidSuccess', 'The Inferno fights your ice! Steam explodes as Thurisaz manifests—cold meeting heat in spectacular conflict!', 0.8, '["Biome-Specific", "Conflicting"]'),
('GaldrCasting', 'Invocation', 'Thurisaz', 'FrostLance', 'Muspelheim', 'ExceptionalSuccess', 'Against all logic, frost manifests in this hellish place! Your Galdr proves stronger than the environment—a testament to your will!', 0.8, '["Biome-Specific", "Impressive"]');

-- ==============================================================================
-- BIOME-SPECIFIC GALDR (Alfheim - Reality Distortion)
-- ==============================================================================

INSERT INTO Galdr_Action_Descriptors (category, action_type, rune_school, ability_name, biome_name, success_level, descriptor_text, weight, tags)
VALUES
('GaldrCasting', 'Invocation', NULL, NULL, 'Alfheim', 'SolidSuccess', 'The Cursed Choir harmonizes with your Galdr—or does it corrupt it? Your rune flickers between states: was/is/will be!', 0.8, '["Biome-Specific", "Reality-Distortion", "Horrifying"]'),
('GaldrCasting', 'Invocation', NULL, NULL, 'Alfheim', 'SolidSuccess', 'Your rune flickers between states—was that {Rune} or is it {Rune} or will it be {Rune}? Reality rebels against your magic—yet the spell works anyway!', 0.8, '["Biome-Specific", "Paradoxical"]'),
('GaldrCasting', 'Invocation', NULL, NULL, 'Alfheim', 'ExceptionalSuccess', 'Reality itself rebels against your Galdr—but you prove stronger! The spell tears through Alfheim''s distortions with terrible clarity!', 0.8, '["Biome-Specific", "Triumphant"]');

-- ==============================================================================
-- Statistics Query
-- ==============================================================================
-- Verify descriptor counts:
-- SELECT rune_school, COUNT(*) as count
-- FROM Galdr_Action_Descriptors
-- GROUP BY rune_school
-- ORDER BY count DESC;
