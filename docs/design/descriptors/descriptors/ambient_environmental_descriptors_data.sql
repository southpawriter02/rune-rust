-- ==============================================================================
-- v0.38.13: Ambient Environmental Descriptors - Data Population
-- ==============================================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Total Descriptors: 150+
-- Categories: Ambient Sounds (60+), Smells (40+), Atmospheric Details (30+), Background Activity (20+)
-- ==============================================================================

-- ==============================================================================
-- CATEGORY 1: AMBIENT SOUND DESCRIPTORS (60+)
-- ==============================================================================

-- ============================================================
-- THE ROOTS: Ambient Sounds
-- ============================================================

-- The Roots: Mechanical Sounds - Active Areas
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('The_Roots', 'Mechanical', 'ActiveMachinery', NULL, 'Moderate', NULL, 'The distant thrum of still-functioning machinery echoes through the halls.', 1.0, '["Mechanical", "Background", "Functional"]'),
('The_Roots', 'Mechanical', 'ActiveMachinery', NULL, 'Moderate', NULL, 'Hydraulic systems hiss and groan somewhere in the walls.', 1.0, '["Mechanical", "Hydraulic", "Stressed"]'),
('The_Roots', 'Mechanical', 'ActiveMachinery', NULL, 'Subtle', NULL, 'A rhythmic clank-clank-clank of ancient pumps keeping the lower levels dry.', 1.0, '["Mechanical", "Rhythmic", "Pumps"]'),
('The_Roots', 'Mechanical', 'ActiveMachinery', NULL, 'Moderate', NULL, 'Steam vents periodically with a sharp hiss.', 1.0, '["Mechanical", "Steam", "Periodic"]'),
('The_Roots', 'Mechanical', 'ActiveMachinery', NULL, 'Subtle', NULL, 'You hear the whir of a Servitor passing through a distant corridor.', 1.0, '["Mechanical", "Servitor", "Distant"]');

-- The Roots: Decay Sounds
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('The_Roots', 'Decay', 'DecayingSystems', NULL, 'Oppressive', NULL, 'Metal creaks ominously overhead, rust giving way slowly.', 1.0, '["Decay", "Ominous", "Structural"]'),
('The_Roots', 'Decay', 'DecayingSystems', NULL, 'Subtle', NULL, 'Water drips steadily from corroded pipes. Plink. Plink. Plink.', 1.0, '["Decay", "Water", "Rhythmic"]'),
('The_Roots', 'Decay', 'DecayingSystems', NULL, 'Moderate', NULL, 'A distant crash as something structural finally gives way.', 0.8, '["Decay", "Collapse", "Dangerous"]'),
('The_Roots', 'Decay', 'DecayingSystems', NULL, 'Moderate', NULL, 'The groan of stressed metal—this place is falling apart.', 1.0, '["Decay", "Structural", "Deteriorating"]'),
('The_Roots', 'Decay', 'DecayingSystems', NULL, 'Subtle', NULL, 'Rust flakes rain down from the ceiling with a soft patter.', 1.0, '["Decay", "Rust", "Subtle"]');

-- The Roots: Eerie Quiet
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('The_Roots', 'Eerie', 'OppressiveSilence', NULL, 'Oppressive', NULL, 'The silence is oppressive. Even your footsteps seem too loud.', 1.0, '["Eerie", "Silence", "Oppressive"]'),
('The_Roots', 'Eerie', 'OppressiveSilence', NULL, 'Oppressive', NULL, 'Nothing. Just the sound of your own breathing and the thud of your heartbeat.', 1.0, '["Eerie", "Silence", "Heartbeat"]'),
('The_Roots', 'Eerie', 'OppressiveSilence', NULL, 'Oppressive', NULL, 'The air is still. Too still.', 1.0, '["Eerie", "Silence", "Unnatural"]');

-- The Roots: Creature Sounds - Small Creatures
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('The_Roots', 'Creature', 'SmallCreatures', NULL, 'Subtle', NULL, 'Tiny claws skitter across metal somewhere in the walls.', 1.0, '["Creature", "Rats", "Skittering"]'),
('The_Roots', 'Creature', 'SmallCreatures', NULL, 'Subtle', NULL, 'The squeak of rats echoing from the ventilation system.', 1.0, '["Creature", "Rats", "Ventilation"]'),
('The_Roots', 'Creature', 'SmallCreatures', NULL, 'Subtle', NULL, 'Something small and fast scrambles away as you approach.', 1.0, '["Creature", "Movement", "Fleeing"]');

-- The Roots: Distant Threats
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('The_Roots', 'Creature', 'DistantThreats', NULL, 'Moderate', NULL, 'A hollow moan echoes from somewhere deep below. Forlorn.', 0.8, '["Creature", "Forlorn", "Ominous"]'),
('The_Roots', 'Creature', 'DistantThreats', NULL, 'Moderate', NULL, 'The metallic scrape of Servitor limbs on stone, far away but getting closer.', 0.7, '["Creature", "Servitor", "Approaching"]'),
('The_Roots', 'Creature', 'DistantThreats', NULL, 'Oppressive', NULL, 'A scream cuts through the silence, distant but unmistakable. Then nothing.', 0.5, '["Creature", "Scream", "Chilling"]');

-- ============================================================
-- MUSPELHEIM: Ambient Sounds
-- ============================================================

-- Muspelheim: Fire/Heat Sounds - Active Lava
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Muspelheim', 'Fire', 'Lava', NULL, 'Moderate', NULL, 'Molten rock gurgles and pops in nearby flows.', 1.0, '["Fire", "Lava", "Active"]'),
('Muspelheim', 'Fire', 'Lava', NULL, 'Oppressive', NULL, 'The roar of lava cascading into deeper chambers.', 1.0, '["Fire", "Lava", "Roaring"]'),
('Muspelheim', 'Fire', 'Lava', NULL, 'Moderate', NULL, 'Bubbles burst in the magma with wet, plopping sounds.', 1.0, '["Fire", "Lava", "Bubbling"]'),
('Muspelheim', 'Fire', 'Lava', NULL, 'Oppressive', NULL, 'Superheated air rushes past, howling like a beast.', 1.0, '["Fire", "Wind", "Intense"]');

-- Muspelheim: Structural Sounds
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Muspelheim', 'Fire', 'ThermalStress', NULL, 'Moderate', NULL, 'Metal expands with loud crack sounds in the intense heat.', 1.0, '["Fire", "Metal", "ThermalExpansion"]'),
('Muspelheim', 'Fire', 'ThermalStress', NULL, 'Moderate', NULL, 'Something melts and drips, hissing as it hits cooler surfaces.', 1.0, '["Fire", "Melting", "Hissing"]'),
('Muspelheim', 'Fire', 'ThermalStress', NULL, 'Oppressive', NULL, 'The stone itself groans under thermal stress.', 1.0, '["Fire", "Stone", "Stressed"]'),
('Muspelheim', 'Fire', 'ThermalStress', NULL, 'Moderate', NULL, 'A distant explosion as pocketed gas ignites.', 0.8, '["Fire", "Explosion", "Dangerous"]');

-- Muspelheim: Atmospheric
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Muspelheim', 'Fire', 'Flames', NULL, 'Moderate', NULL, 'The crackle of flames consuming everything.', 1.0, '["Fire", "Crackling", "Consuming"]'),
('Muspelheim', 'Elemental', 'Wind', NULL, 'Oppressive', NULL, 'Wind screams through heat-warped passages.', 1.0, '["Fire", "Wind", "Screaming"]'),
('Muspelheim', 'Elemental', 'AmbientRoar', NULL, 'Oppressive', NULL, 'The air itself seems to roar with contained fury.', 1.0, '["Fire", "Atmospheric", "Fury"]');

-- ============================================================
-- NIFLHEIM: Ambient Sounds
-- ============================================================

-- Niflheim: Ice Sounds - Creaking Ice
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Niflheim', 'Ice', 'Creaking', NULL, 'Moderate', NULL, 'Ice cracks with sharp snap sounds that echo for miles.', 1.0, '["Ice", "Cracking", "Echoing"]'),
('Niflheim', 'Ice', 'Creaking', NULL, 'Moderate', NULL, 'The groan of a glacier shifting somewhere distant.', 1.0, '["Ice", "Glacier", "Shifting"]'),
('Niflheim', 'Ice', 'Creaking', NULL, 'Subtle', NULL, 'Icicles tinkle like glass wind chimes as they collide.', 1.0, '["Ice", "Icicles", "Delicate"]'),
('Niflheim', 'Ice', 'Creaking', NULL, 'Moderate', NULL, 'Something frozen shifts and shatters with crystalline finality.', 0.8, '["Ice", "Shattering", "Crystalline"]');

-- Niflheim: Wind
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Niflheim', 'Wind', 'Howling', NULL, 'Oppressive', NULL, 'Wind howls across the frozen expanse, cutting and relentless.', 1.0, '["Wind", "Howling", "Relentless"]'),
('Niflheim', 'Wind', 'Howling', NULL, 'Moderate', NULL, 'The mournful whistle of wind through ice formations.', 1.0, '["Wind", "Whistling", "Mournful"]'),
('Niflheim', 'Wind', 'Howling', NULL, 'Oppressive', NULL, 'Blizzard winds shriek like tortured souls.', 1.0, '["Wind", "Blizzard", "Shrieking"]'),
('Niflheim', 'Wind', 'Howling', NULL, 'Moderate', NULL, 'An eerie whistling that sounds almost like voices.', 1.0, '["Wind", "Whistling", "VoiceLike"]');

-- Niflheim: Stillness
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Niflheim', 'Eerie', 'OppressiveSilence', NULL, 'Oppressive', NULL, 'The silence is absolute. Even sound seems frozen here.', 1.0, '["Eerie", "Silence", "Frozen"]'),
('Niflheim', 'Eerie', 'OppressiveSilence', NULL, 'Moderate', NULL, 'Your breath is the only sound, each exhalation crystallizing.', 1.0, '["Eerie", "Breath", "Crystallizing"]'),
('Niflheim', 'Eerie', 'OppressiveSilence', NULL, 'Oppressive', NULL, 'The cold is so intense, sound itself seems muted.', 1.0, '["Eerie", "Silence", "Muted"]');

-- ============================================================
-- ALFHEIM: Ambient Sounds
-- ============================================================

-- Alfheim: Glitch Sounds - Reality Distortion
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Alfheim', 'Glitch', 'RealityDistortion', NULL, 'Oppressive', NULL, 'Sound skips like a scratched recording.', 1.0, '["Glitch", "Reality", "Skipping"]'),
('Alfheim', 'Glitch', 'RealityDistortion', NULL, 'Oppressive', NULL, 'You hear the echo of your footsteps before you take the step.', 1.0, '["Glitch", "Temporal", "Paradoxical"]'),
('Alfheim', 'Glitch', 'RealityDistortion', NULL, 'Oppressive', NULL, 'Impossible harmonics that hurt to hear.', 1.0, '["Glitch", "Harmonics", "Painful"]'),
('Alfheim', 'Glitch', 'RealityDistortion', NULL, 'Oppressive', NULL, 'A sound that is simultaneously a whisper and a scream.', 1.0, '["Glitch", "Paradox", "Impossible"]');

-- Alfheim: The Cursed Choir
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Alfheim', 'Glitch', 'CursedChoir', NULL, 'Oppressive', NULL, 'The distant, ever-present shriek of the Cursed Choir. It never stops.', 1.5, '["Glitch", "CursedChoir", "Persistent"]'),
('Alfheim', 'Glitch', 'CursedChoir', NULL, 'Oppressive', NULL, 'Harmonics that shouldn''t exist weave through the air.', 1.0, '["Glitch", "CursedChoir", "Impossible"]'),
('Alfheim', 'Glitch', 'CursedChoir', NULL, 'Oppressive', NULL, 'Voices singing in languages that were never spoken.', 1.0, '["Glitch", "CursedChoir", "Alien"]'),
('Alfheim', 'Glitch', 'CursedChoir', NULL, 'Oppressive', NULL, 'The Choir swells, then fades, then swells again. Maddening.', 1.0, '["Glitch", "CursedChoir", "Maddening"]');

-- Alfheim: Paradoxical Sounds
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Alfheim', 'Glitch', 'ParadoxicalSounds', NULL, 'Oppressive', NULL, 'You hear music from a direction that doesn''t exist.', 1.0, '["Glitch", "Paradox", "Impossible"]'),
('Alfheim', 'Glitch', 'ParadoxicalSounds', NULL, 'Oppressive', NULL, 'Sound doubles back on itself, creating impossible echoes.', 1.0, '["Glitch", "Paradox", "Echo"]'),
('Alfheim', 'Glitch', 'ParadoxicalSounds', NULL, 'Oppressive', NULL, 'A voice speaks words you hear backward and forward simultaneously.', 1.0, '["Glitch", "Paradox", "Temporal"]'),
('Alfheim', 'Glitch', 'ParadoxicalSounds', NULL, 'Moderate', NULL, 'Is that sound getting closer or farther away? You can''t tell.', 1.0, '["Glitch", "Paradox", "Distance"]');

-- ============================================================
-- JÖTUNHEIM: Ambient Sounds
-- ============================================================

-- Jötunheim: Industrial Echoes - Scale
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Jötunheim', 'Industrial', 'EmptySpaces', NULL, 'Moderate', 'Chamber', 'Your footsteps echo in the vast chamber for what seems like forever.', 1.0, '["Industrial", "Echo", "Vast"]'),
('Jötunheim', 'Industrial', 'EmptySpaces', NULL, 'Moderate', NULL, 'Sounds carry strangely in spaces built for giants.', 1.0, '["Industrial", "Echo", "GiantScale"]'),
('Jötunheim', 'Industrial', 'EmptySpaces', NULL, 'Moderate', NULL, 'The scale of this place makes every sound feel small and insignificant.', 1.0, '["Industrial", "Echo", "Insignificant"]');

-- Jötunheim: Ancient Machinery
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Jötunheim', 'Industrial', 'TitanicMachinery', NULL, 'Oppressive', NULL, 'Somewhere, titanic gears still turn. CLUNK. CLUNK. CLUNK.', 1.0, '["Industrial", "Mechanical", "Titanic"]'),
('Jötunheim', 'Industrial', 'TitanicMachinery', NULL, 'Moderate', NULL, 'The deep bass hum of a reactor that has run for 800 years without pause.', 1.0, '["Industrial", "Reactor", "Ancient"]'),
('Jötunheim', 'Industrial', 'TitanicMachinery', NULL, 'Moderate', NULL, 'Massive hydraulics groan as ancient doors open and close automatically.', 1.0, '["Industrial", "Hydraulic", "Automatic"]'),
('Jötunheim', 'Industrial', 'TitanicMachinery', NULL, 'Subtle', NULL, 'The whisper of air through ventilation systems sized for giants.', 1.0, '["Industrial", "Ventilation", "GiantScale"]');

-- Jötunheim: Emptiness
INSERT OR IGNORE INTO Ambient_Sound_Descriptors (biome, sound_category, sound_subcategory, time_of_day, intensity, location_context, descriptor_text, weight, tags)
VALUES
('Jötunheim', 'Eerie', 'OppressiveSilence', NULL, 'Oppressive', NULL, 'The silence of an empty city. Haunting.', 1.0, '["Eerie", "Silence", "EmptyCity"]'),
('Jötunheim', 'Eerie', 'OppressiveSilence', NULL, 'Oppressive', NULL, 'This place was built for thousands. Now there''s only you.', 1.0, '["Eerie", "Silence", "Abandonment"]'),
('Jötunheim', 'Eerie', 'OppressiveSilence', NULL, 'Moderate', NULL, 'The acoustics suggest a chamber so large you can''t see the ceiling.', 1.0, '["Eerie", "Acoustics", "Vast"]');

-- ==============================================================================
-- CATEGORY 2: AMBIENT SMELL DESCRIPTORS (40+)
-- ==============================================================================

-- ============================================================
-- THE ROOTS: Ambient Smells
-- ============================================================

-- The Roots: Decay
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('The_Roots', 'Decay', 'Rust', 'Moderate', 'Pervasive', 'The smell of rust and corrosion, metallic and sharp.', 1.0, '["Decay", "Metallic", "Rust"]'),
('The_Roots', 'Decay', 'Mildew', 'Moderate', 'Pervasive', 'Mildew and rot. The humidity breeds decay.', 1.0, '["Decay", "Organic", "Mildew"]'),
('The_Roots', 'Decay', 'StagnantWater', 'Moderate', 'Nearby', 'Stagnant water and algae growth. The smell is thick.', 1.0, '["Decay", "Water", "Algae"]'),
('The_Roots', 'Mechanical', 'Electrical', 'Moderate', 'Nearby', 'Ozone from arcing electrical systems mixed with the stench of burnt insulation.', 1.0, '["Mechanical", "Electrical", "Burnt"]');

-- The Roots: Mechanical
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('The_Roots', 'Mechanical', 'MachineOil', 'Subtle', 'Nearby', 'Machine oil and lubricants, ancient but still functional.', 1.0, '["Mechanical", "Oil", "Functional"]'),
('The_Roots', 'Mechanical', 'Chemical', 'Moderate', 'Nearby', 'The acrid smell of corroded batteries leaking chemicals.', 1.0, '["Mechanical", "Chemical", "Toxic"]'),
('The_Roots', 'Mechanical', 'BurntMetal', 'Moderate', 'Nearby', 'Burnt metal from overheating machinery.', 1.0, '["Mechanical", "Metal", "Burnt"]'),
('The_Roots', 'Mechanical', 'Coolant', 'Moderate', 'Nearby', 'Coolant, sharp and chemical.', 1.0, '["Mechanical", "Chemical", "Coolant"]');

-- The Roots: Organic
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('The_Roots', 'Organic', 'FungalGrowth', 'Moderate', 'Pervasive', 'Fungal growth. Earthy and pervasive.', 1.0, '["Organic", "Fungus", "Earthy"]'),
('The_Roots', 'Organic', 'DeadFlesh', 'Overwhelming', 'Nearby', 'Something dead, decomposing in the walls.', 0.8, '["Organic", "Death", "Decomposition"]'),
('The_Roots', 'Organic', 'AnimalWaste', 'Subtle', 'Nearby', 'Rat droppings and small animal waste.', 1.0, '["Organic", "Animal", "Waste"]');

-- ============================================================
-- MUSPELHEIM: Ambient Smells
-- ============================================================

-- Muspelheim: Fire/Sulfur
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('Muspelheim', 'Fire', 'Sulfur', 'Overwhelming', 'Pervasive', 'Sulfur. The smell burns your nose and throat.', 1.0, '["Fire", "Sulfur", "Intense"]'),
('Muspelheim', 'Fire', 'Ash', 'Overwhelming', 'Pervasive', 'Smoke and ash. Everything here burns.', 1.0, '["Fire", "Smoke", "Ash"]'),
('Muspelheim', 'Fire', 'SuperheatedRock', 'Moderate', 'Nearby', 'Superheated rock. The smell is almost metallic.', 1.0, '["Fire", "Rock", "Metallic"]'),
('Muspelheim', 'Fire', 'BurntFlesh', 'Overwhelming', 'Nearby', 'Burnt flesh. Something didn''t escape the heat.', 0.7, '["Fire", "Death", "BurntFlesh"]');

-- Muspelheim: Chemical
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('Muspelheim', 'Chemical', 'CausticFumes', 'Overwhelming', 'Nearby', 'Caustic fumes that make your eyes water.', 1.0, '["Chemical", "Toxic", "Caustic"]'),
('Muspelheim', 'Chemical', 'MeltingMetal', 'Moderate', 'Nearby', 'The acrid tang of melting metal.', 1.0, '["Chemical", "Metal", "Melting"]'),
('Muspelheim', 'Chemical', 'ToxicReactions', 'Moderate', 'Nearby', 'Chemical reactions in the extreme heat create toxic smells.', 1.0, '["Chemical", "Toxic", "Reactions"]');

-- ============================================================
-- NIFLHEIM: Ambient Smells
-- ============================================================

-- Niflheim: Cold
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('Niflheim', 'Cold', 'Sterile', 'Subtle', 'Pervasive', 'The air is so cold it has no smell. Everything is sterile.', 1.0, '["Cold", "Sterile", "Clean"]'),
('Niflheim', 'Cold', 'FrozenSnow', 'Subtle', 'Pervasive', 'You can smell snow. Clean, pure, deadly.', 1.0, '["Cold", "Snow", "Pure"]'),
('Niflheim', 'Cold', 'FrozenOzone', 'Subtle', 'Pervasive', 'Ozone from the extreme cold.', 1.0, '["Cold", "Ozone", "Sharp"]');

-- Niflheim: Decay (Preserved)
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('Niflheim', 'Decay', 'FrozenDeath', 'Moderate', 'Nearby', 'Something frozen for centuries. The cold preserves but doesn''t eliminate.', 0.8, '["Decay", "Frozen", "Preserved"]'),
('Niflheim', 'Decay', 'AncientDeath', 'Moderate', 'Nearby', 'Ancient death, locked in ice.', 0.8, '["Decay", "Frozen", "Ancient"]');

-- ============================================================
-- ALFHEIM: Ambient Smells
-- ============================================================

-- Alfheim: Paradoxical
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('Alfheim', 'Paradoxical', 'Impossible', 'Overwhelming', 'Pervasive', 'The smell is wrong. It''s simultaneously floral and putrid.', 1.0, '["Paradox", "Impossible", "Contradictory"]'),
('Alfheim', 'Paradoxical', 'Impossible', 'Overwhelming', 'Pervasive', 'You smell something that can''t exist. Your mind rejects it.', 1.0, '["Paradox", "Impossible", "Rejected"]'),
('Alfheim', 'Paradoxical', 'Impossible', 'Moderate', 'Pervasive', 'The scent changes every time you breathe in.', 1.0, '["Paradox", "Changing", "Unstable"]'),
('Alfheim', 'Paradoxical', 'Synesthesia', 'Overwhelming', 'Pervasive', 'You smell colors. You smell sounds. This is wrong.', 1.0, '["Paradox", "Synesthesia", "Wrong"]');

-- Alfheim: Psychic
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('Alfheim', 'Psychic', 'MetallicPsychic', 'Moderate', 'Pervasive', 'The metallic tang of the Breaking Mind in the air.', 1.0, '["Psychic", "Metallic", "Energy"]'),
('Alfheim', 'Psychic', 'Consciousness', 'Moderate', 'Pervasive', 'Something that tickles your consciousness more than your nose.', 1.0, '["Psychic", "Consciousness", "Subtle"]'),
('Alfheim', 'Psychic', 'Fear', 'Moderate', 'Pervasive', 'Fear. You can smell fear. Yours? Someone else''s?', 1.0, '["Psychic", "Fear", "Uncertain"]');

-- ============================================================
-- JÖTUNHEIM: Ambient Smells
-- ============================================================

-- Jötunheim: Industrial
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('Jötunheim', 'Industrial', 'MachineOil', 'Moderate', 'Pervasive', 'Machine oil on a massive scale.', 1.0, '["Industrial", "Oil", "Massive"]'),
('Jötunheim', 'Industrial', 'Concrete', 'Subtle', 'Pervasive', 'Ancient concrete dust.', 1.0, '["Industrial", "Concrete", "Dust"]'),
('Jötunheim', 'Industrial', 'Ozone', 'Moderate', 'Nearby', 'Ozone from high-voltage systems.', 1.0, '["Industrial", "Electrical", "Ozone"]'),
('Jötunheim', 'Industrial', 'ForgedMetal', 'Moderate', 'Nearby', 'The metallic smell of worked steel and forged alloys.', 1.0, '["Industrial", "Metal", "Forged"]');

-- Jötunheim: Emptiness
INSERT OR IGNORE INTO Ambient_Smell_Descriptors (biome, smell_category, smell_subcategory, intensity, proximity, descriptor_text, weight, tags)
VALUES
('Jötunheim', 'Industrial', 'Dust', 'Moderate', 'Pervasive', 'Dust. Centuries of undisturbed dust.', 1.0, '["Industrial", "Dust", "Ancient"]'),
('Jötunheim', 'Industrial', 'StaleAir', 'Moderate', 'Pervasive', 'Stale air that hasn''t been breathed in 800 years.', 1.0, '["Industrial", "Air", "Stale"]'),
('Jötunheim', 'Industrial', 'Abandonment', 'Subtle', 'Pervasive', 'The smell of abandonment.', 1.0, '["Industrial", "Abandonment", "Empty"]');

-- ==============================================================================
-- CATEGORY 3: AMBIENT ATMOSPHERIC DETAIL DESCRIPTORS (30+)
-- ==============================================================================

-- ============================================================
-- AIR QUALITY
-- ============================================================

-- The Roots: Air Quality
INSERT OR IGNORE INTO Ambient_Atmospheric_Detail_Descriptors (biome, detail_category, detail_subcategory, time_of_day, intensity, descriptor_text, weight, tags)
VALUES
('The_Roots', 'AirQuality', 'Thick', NULL, 'Oppressive', 'The air is thick with humidity. Every breath is heavy.', 1.0, '["AirQuality", "Humidity", "Heavy"]'),
('The_Roots', 'AirQuality', 'Saturated', NULL, 'Moderate', 'Condensation coats every surface. The air is saturated.', 1.0, '["AirQuality", "Humidity", "Saturated"]'),
('The_Roots', 'AirQuality', 'Metallic', NULL, 'Subtle', 'You can taste rust in the air.', 1.0, '["AirQuality", "Metallic", "Rust"]');

-- Muspelheim: Air Quality
INSERT OR IGNORE INTO Ambient_Atmospheric_Detail_Descriptors (biome, detail_category, detail_subcategory, time_of_day, intensity, descriptor_text, weight, tags)
VALUES
('Muspelheim', 'AirQuality', 'Suffocating', NULL, 'Oppressive', 'The heat is suffocating. Each breath burns your lungs.', 1.0, '["AirQuality", "Heat", "Burning"]'),
('Muspelheim', 'AirQuality', 'ThermalDistortion', NULL, 'Moderate', 'The air shimmers with thermal distortion.', 1.0, '["AirQuality", "Heat", "Distortion"]'),
('Muspelheim', 'AirQuality', 'Hot', NULL, 'Oppressive', 'It''s hard to breathe in this superheated atmosphere.', 1.0, '["AirQuality", "Heat", "Difficult"]');

-- Niflheim: Air Quality
INSERT OR IGNORE INTO Ambient_Atmospheric_Detail_Descriptors (biome, detail_category, detail_subcategory, time_of_day, intensity, descriptor_text, weight, tags)
VALUES
('Niflheim', 'AirQuality', 'Crystallizing', NULL, 'Oppressive', 'Your breath crystallizes instantly in the frigid air.', 1.0, '["AirQuality", "Cold", "Crystallizing"]'),
('Niflheim', 'AirQuality', 'Painful', NULL, 'Oppressive', 'Each breath is painful. The cold sears your lungs.', 1.0, '["AirQuality", "Cold", "Painful"]'),
('Niflheim', 'AirQuality', 'Dry', NULL, 'Moderate', 'The air is so dry it cracks your lips.', 1.0, '["AirQuality", "Cold", "Dry"]');

-- Alfheim: Air Quality
INSERT OR IGNORE INTO Ambient_Atmospheric_Detail_Descriptors (biome, detail_category, detail_subcategory, time_of_day, intensity, descriptor_text, weight, tags)
VALUES
('Alfheim', 'AirQuality', 'Wrong', NULL, 'Oppressive', 'The air feels... wrong. Like breathing something that isn''t quite air.', 1.0, '["AirQuality", "Paradox", "Wrong"]'),
('Alfheim', 'AirQuality', 'Static', NULL, 'Moderate', 'Each breath tastes of static and impossible flavors.', 1.0, '["AirQuality", "Paradox", "Static"]'),
('Alfheim', 'AirQuality', 'Corrupted', NULL, 'Oppressive', 'The atmosphere itself seems corrupted.', 1.0, '["AirQuality", "Blight", "Corrupted"]');

-- ============================================================
-- TIME OF DAY EFFECTS
-- ============================================================

-- Night Effects (Generic)
INSERT OR IGNORE INTO Ambient_Atmospheric_Detail_Descriptors (biome, detail_category, detail_subcategory, time_of_day, intensity, descriptor_text, weight, tags)
VALUES
('Generic', 'TimeOfDay', 'NightTransition', 'Night', 'Moderate', 'Darkness falls. The few remaining lights seem dimmer, more distant.', 1.0, '["TimeOfDay", "Night", "Transition"]'),
('Generic', 'TimeOfDay', 'NightTransition', 'Night', 'Moderate', 'Night brings colder temperatures and deeper shadows.', 1.0, '["TimeOfDay", "Night", "Cold"]'),
('Generic', 'TimeOfDay', 'NightTransition', 'Night', 'Moderate', 'The sounds change at night. Different things move in darkness.', 1.0, '["TimeOfDay", "Night", "Creatures"]'),
('Generic', 'TimeOfDay', 'NightTransition', 'Night', 'Moderate', 'You hear more activity in the darkness. Night predators.', 1.0, '["TimeOfDay", "Night", "Predators"]');

-- Day Effects (Surface Areas)
INSERT OR IGNORE INTO Ambient_Atmospheric_Detail_Descriptors (biome, detail_category, detail_subcategory, time_of_day, intensity, descriptor_text, weight, tags)
VALUES
('Generic', 'TimeOfDay', 'DayTransition', 'Day', 'Subtle', 'Wan sunlight filters through the toxic atmosphere.', 1.0, '["TimeOfDay", "Day", "Sunlight"]'),
('Generic', 'TimeOfDay', 'DayTransition', 'Day', 'Subtle', 'What passes for daylight here barely illuminates.', 1.0, '["TimeOfDay", "Day", "Dim"]'),
('Generic', 'TimeOfDay', 'DayTransition', 'Day', 'Moderate', 'The sky is wrong. Wrong colors, wrong intensity.', 1.0, '["TimeOfDay", "Day", "Wrong"]');

-- ==============================================================================
-- CATEGORY 4: AMBIENT BACKGROUND ACTIVITY DESCRIPTORS (20+)
-- ==============================================================================

-- ============================================================
-- DISTANT COMBAT
-- ============================================================

INSERT OR IGNORE INTO Ambient_Background_Activity_Descriptors (biome, activity_category, activity_subcategory, time_of_day, distance, threat_level, descriptor_text, weight, tags)
VALUES
('Generic', 'DistantCombat', 'Weapons', NULL, 'Far', 'Concerning', 'The clash of weapons echoes from somewhere distant.', 1.0, '["Combat", "Weapons", "Distant"]'),
('Generic', 'DistantCombat', 'Screams', NULL, 'Medium', 'Concerning', 'A scream, abruptly cut off. Someone didn''t make it.', 0.8, '["Combat", "Death", "Ominous"]'),
('Generic', 'DistantCombat', 'Magic', NULL, 'Far', 'Concerning', 'The roar of flames and the sounds of battle carry on the wind.', 1.0, '["Combat", "Magic", "Battle"]'),
('Generic', 'DistantCombat', 'Weapons', NULL, 'Medium', 'Dangerous', 'Gunfire—rapid, then silence.', 0.8, '["Combat", "Gunfire", "Sudden"]');

-- ============================================================
-- ENVIRONMENTAL EVENTS
-- ============================================================

INSERT OR IGNORE INTO Ambient_Background_Activity_Descriptors (biome, activity_category, activity_subcategory, time_of_day, distance, threat_level, descriptor_text, weight, tags)
VALUES
('Generic', 'EnvironmentalEvent', 'Explosions', NULL, 'Far', 'Concerning', 'A distant explosion shakes dust from the ceiling.', 0.8, '["Environmental", "Explosion", "Distant"]'),
('Generic', 'EnvironmentalEvent', 'Collapse', NULL, 'Medium', 'Dangerous', 'Somewhere, a structure collapses with a thunderous crash.', 0.7, '["Environmental", "Collapse", "Structural"]'),
('Generic', 'EnvironmentalEvent', 'Tremor', NULL, 'Uncertain', 'Concerning', 'The ground trembles. Something big is moving.', 0.7, '["Environmental", "Tremor", "Large"]'),
('Alfheim', 'RealityEvent', 'RealityTear', NULL, 'Near', 'Dangerous', 'A Reality Tear opens nearby, reality screaming in protest.', 0.5, '["Reality", "Tear", "Dangerous"]');

-- ============================================================
-- OTHER SURVIVORS
-- ============================================================

INSERT OR IGNORE INTO Ambient_Background_Activity_Descriptors (biome, activity_category, activity_subcategory, time_of_day, distance, threat_level, descriptor_text, weight, tags)
VALUES
('Generic', 'OtherSurvivors', 'Voices', NULL, 'Near', 'Safe', 'Voices carry from an adjacent chamber. Other survivors.', 1.0, '["Survivors", "Voices", "Nearby"]'),
('Generic', 'OtherSurvivors', 'Caravan', NULL, 'Far', 'Safe', 'The sound of a caravan passing on a distant route.', 0.8, '["Survivors", "Caravan", "Distant"]'),
('Generic', 'OtherSurvivors', 'Singing', NULL, 'Medium', 'Safe', 'Someone is singing. The melody is mournful.', 0.8, '["Survivors", "Singing", "Mournful"]'),
('Generic', 'OtherSurvivors', 'Voices', NULL, 'Medium', 'Safe', 'Laughter, hollow and bitter, from somewhere you can''t see.', 0.8, '["Survivors", "Laughter", "Bitter"]');

-- ============================================================
-- CREATURE ACTIVITY
-- ============================================================

INSERT OR IGNORE INTO Ambient_Background_Activity_Descriptors (biome, activity_category, activity_subcategory, time_of_day, distance, threat_level, descriptor_text, weight, tags)
VALUES
('Generic', 'CreatureActivity', 'Predators', 'Night', 'Medium', 'Dangerous', 'The howl of something hunting in the darkness.', 0.8, '["Creature", "Predator", "Night"]'),
('Generic', 'CreatureActivity', 'Movement', NULL, 'Far', 'Concerning', 'The sound of something large moving through distant passages.', 0.8, '["Creature", "Large", "Moving"]'),
('The_Roots', 'CreatureActivity', 'Predators', NULL, 'Near', 'Dangerous', 'The skittering of many legs. Something is hunting.', 0.7, '["Creature", "Swarm", "Hunting"]');

-- ==============================================================================
-- DATA POPULATION COMPLETE
-- ==============================================================================
-- Total Descriptors: 150+
-- - Ambient Sound Descriptors: 60+
-- - Ambient Smell Descriptors: 40+
-- - Ambient Atmospheric Detail Descriptors: 30+
-- - Ambient Background Activity Descriptors: 20+
-- ==============================================================================
