-- ============================================================
-- v0.38.4: Atmospheric Descriptor Data
-- 150+ Sensory Descriptors across 5 Categories
-- ============================================================

-- ============================================================
-- CATEGORY 1: LIGHTING (40+ descriptors)
-- ============================================================

-- Generic Lighting: Dim
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Lighting', 'Subtle', 'The light here is dim, barely enough to see by.', NULL, '["Dim", "Generic"]'),
('Lighting', 'Moderate', 'Shadows crowd the edges of your vision.', NULL, '["Dim", "Shadow"]'),
('Lighting', 'Moderate', 'What little illumination exists seems to be fading.', NULL, '["Dim", "Failing"]');

-- Generic Lighting: Flickering
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Lighting', 'Moderate', 'The runic light panels flicker erratically, casting unstable shadows.', NULL, '["Flickering", "Runic"]'),
('Lighting', 'Moderate', 'Light pulses in irregular intervals, never quite steady.', NULL, '["Flickering", "Unstable"]'),
('Lighting', 'Oppressive', 'The illumination stutters like a failing circuit, plunging into darkness between pulses.', NULL, '["Flickering", "Extreme"]');

-- Generic Lighting: Harsh
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Lighting', 'Moderate', 'Harsh, unfiltered light glares from exposed fixtures.', NULL, '["Harsh", "Industrial"]'),
('Lighting', 'Oppressive', 'The brightness is clinical and unforgiving, revealing every crack and stain.', NULL, '["Harsh", "Clinical"]'),
('Lighting', 'Moderate', 'Light reflects harshly off metallic surfaces, creating painful glare.', NULL, '["Harsh", "Reflective"]');

-- Generic Lighting: Darkness
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Lighting', 'Oppressive', 'True darkness reigns here, swallowing all light.', NULL, '["Darkness", "Extreme"]'),
('Lighting', 'Oppressive', 'The shadows are absolute and impenetrable.', NULL, '["Darkness", "Absolute"]'),
('Lighting', 'Oppressive', 'Not even runic light penetrates this gloom.', NULL, '["Darkness", "Unnatural"]');

-- Biome-Specific Lighting: Muspelheim (Warm/Fire)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Lighting', 'Moderate', 'Everything is bathed in red-orange firelight.', 'Muspelheim', '["Warm", "Fire"]'),
('Lighting', 'Moderate', 'The light has a warm, almost molten quality.', 'Muspelheim', '["Warm", "Molten"]'),
('Lighting', 'Oppressive', 'Firelight flares and dims with the rhythm of flowing lava.', 'Muspelheim', '["Fire", "Lava"]'),
('Lighting', 'Subtle', 'A faint red glow emanates from heated rock.', 'Muspelheim', '["Warm", "Subtle"]');

-- Biome-Specific Lighting: Niflheim (Cold/Ice)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Lighting', 'Moderate', 'Blue-white light gives everything a frozen appearance.', 'Niflheim', '["Cold", "Ice"]'),
('Lighting', 'Moderate', 'The illumination is pale and lifeless.', 'Niflheim', '["Cold", "Pale"]'),
('Lighting', 'Subtle', 'Wan light filters through layers of frost.', 'Niflheim', '["Cold", "Frost"]'),
('Lighting', 'Oppressive', 'The light itself seems frozen, trapped in crystalline ice.', 'Niflheim', '["Cold", "Crystalline"]');

-- Biome-Specific Lighting: The Roots (Sickly/Corrupt)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Lighting', 'Moderate', 'The light has a sickly, greenish tinge.', 'The_Roots', '["Sickly", "Green"]'),
('Lighting', 'Moderate', 'Illumination is the color of corroded copper.', 'The_Roots', '["Sickly", "Corroded"]'),
('Lighting', 'Moderate', 'Sickly light flickers from failing panels.', 'The_Roots', '["Sickly", "Failing"]'),
('Lighting', 'Subtle', 'A faint greenish glow suggests bioluminescent mold.', 'The_Roots', '["Sickly", "Bioluminescent"]');

-- Biome-Specific Lighting: Alfheim (Prismatic/Reality-Warping)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Lighting', 'Oppressive', 'Light refracts into impossible colors.', 'Alfheim', '["Prismatic", "Impossible"]'),
('Lighting', 'Oppressive', 'The illumination shifts through the spectrum without pattern or reason.', 'Alfheim', '["Prismatic", "Chaotic"]'),
('Lighting', 'Oppressive', 'Light refracts impossibly through crystalline structures that shouldn''t exist.', 'Alfheim', '["Prismatic", "Crystal"]'),
('Lighting', 'Moderate', 'Colors bleed and merge in ways that hurt to perceive.', 'Alfheim', '["Prismatic", "Painful"]');

-- Biome-Specific Lighting: Jötunheim (Industrial/Dim)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Lighting', 'Moderate', 'Dim industrial lighting casts long shadows.', 'Jötunheim', '["Dim", "Industrial"]'),
('Lighting', 'Subtle', 'Emergency lighting provides minimal illumination.', 'Jötunheim', '["Dim", "Emergency"]'),
('Lighting', 'Moderate', 'Ancient light strips struggle against the darkness.', 'Jötunheim', '["Dim", "Failing"]');

-- ============================================================
-- CATEGORY 2: SOUND (40+ descriptors)
-- ============================================================

-- Generic Sound: Mechanical
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Moderate', 'Distant machinery groans and clanks rhythmically.', NULL, '["Mechanical", "Industrial"]'),
('Sound', 'Subtle', 'The whir of failing servos echoes through the space.', NULL, '["Mechanical", "Servos"]'),
('Sound', 'Moderate', 'Grinding gears struggle against centuries of corrosion.', NULL, '["Mechanical", "Grinding"]'),
('Sound', 'Oppressive', 'The cacophony of tortured machinery is overwhelming.', NULL, '["Mechanical", "Extreme"]');

-- Generic Sound: Water
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Subtle', 'Water drips steadily from unseen sources.', NULL, '["Water", "Dripping"]'),
('Sound', 'Moderate', 'The sound of flowing water echoes from below.', NULL, '["Water", "Flowing"]'),
('Sound', 'Subtle', 'Condensation falls in irregular patterns.', NULL, '["Water", "Condensation"]');

-- Generic Sound: Wind/Air
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Subtle', 'Air hisses through cracks in the walls.', NULL, '["Wind", "Hissing"]'),
('Sound', 'Moderate', 'A low moan of moving air fills the space.', NULL, '["Wind", "Moaning"]'),
('Sound', 'Moderate', 'Ventilation systems wheeze and rattle.', NULL, '["Wind", "Ventilation"]');

-- Generic Sound: Electrical
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Moderate', 'Power conduits arc and crackle intermittently.', NULL, '["Electrical", "Arcing"]'),
('Sound', 'Moderate', 'The buzz of unstable electricity permeates everything.', NULL, '["Electrical", "Buzzing"]'),
('Sound', 'Subtle', 'Static discharge pops and hisses.', NULL, '["Electrical", "Static"]');

-- Generic Sound: Silence
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Oppressive', 'The silence here is oppressive and unnatural.', NULL, '["Silence", "Unnatural"]'),
('Sound', 'Oppressive', 'Not even your footsteps seem to make sound.', NULL, '["Silence", "Absolute"]'),
('Sound', 'Oppressive', 'The absence of noise is almost deafening.', NULL, '["Silence", "Deafening"]');

-- Psychic/Metaphysical Sounds
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Subtle', 'Whispers at the edge of hearing suggest voices long silenced.', NULL, '["Psychic", "Whispers"]'),
('Sound', 'Moderate', 'You hear fragments of conversation that aren''t there.', NULL, '["Psychic", "Echoes"]'),
('Sound', 'Oppressive', 'The air carries echoes of ancient screams.', NULL, '["Psychic", "Screams"]'),
('Sound', 'Oppressive', 'A high-pitched shriek underlies all other sounds.', 'Alfheim', '["Psychic", "Cursed_Choir"]'),
('Sound', 'Oppressive', 'Reality itself seems to emit a painful frequency.', 'Alfheim', '["Psychic", "Reality_Break"]');

-- Biome-Specific Sound: Muspelheim (Fire/Lava)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Moderate', 'Lava rumbles distantly like approaching thunder.', 'Muspelheim', '["Fire", "Lava"]'),
('Sound', 'Moderate', 'Steam hisses from vents with violent force.', 'Muspelheim', '["Fire", "Steam"]'),
('Sound', 'Oppressive', 'Lava rumbles like distant thunder, and flames crackle constantly.', 'Muspelheim', '["Fire", "Overwhelming"]'),
('Sound', 'Subtle', 'The crackle of flames provides an almost comforting background.', 'Muspelheim', '["Fire", "Subtle"]');

-- Biome-Specific Sound: Niflheim (Ice/Wind)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Moderate', 'Ice creaks and groans under unknown stress.', 'Niflheim', '["Ice", "Creaking"]'),
('Sound', 'Oppressive', 'Wind howls with primal fury.', 'Niflheim', '["Ice", "Wind"]'),
('Sound', 'Subtle', 'The faint tinkle of ice crystals falling.', 'Niflheim', '["Ice", "Crystals"]'),
('Sound', 'Moderate', 'Frozen structures contract with sharp cracks.', 'Niflheim', '["Ice", "Cracking"]');

-- Biome-Specific Sound: The Roots (Water/Metal)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Moderate', 'Water drips steadily, and metal groans under stress.', 'The_Roots', '["Water", "Metal"]'),
('Sound', 'Subtle', 'The hiss of leaking steam punctuates the silence.', 'The_Roots', '["Steam", "Leaking"]'),
('Sound', 'Moderate', 'Corroded pipes rattle and clang.', 'The_Roots', '["Metal", "Corroded"]');

-- Biome-Specific Sound: Jötunheim (Industrial/Echoing)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Sound', 'Moderate', 'Machinery groans distantly in vast spaces.', 'Jötunheim', '["Mechanical", "Echoing"]'),
('Sound', 'Subtle', 'Your footsteps echo endlessly in the emptiness.', 'Jötunheim', '["Echoing", "Empty"]'),
('Sound', 'Moderate', 'The sound of titanic industry long silenced lingers.', 'Jötunheim', '["Industrial", "Ghost"]');

-- ============================================================
-- CATEGORY 3: SMELL (40+ descriptors)
-- ============================================================

-- Industrial Scents: Rust/Metal
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Moderate', 'The metallic tang of rust is overwhelming.', NULL, '["Metal", "Rust"]'),
('Smell', 'Moderate', 'The air smells of oxidized iron and decay.', NULL, '["Metal", "Oxidized"]'),
('Smell', 'Oppressive', 'Everything reeks of corroded metal.', NULL, '["Metal", "Corroded"]'),
('Smell', 'Subtle', 'A faint metallic scent hangs in the air.', NULL, '["Metal", "Subtle"]');

-- Industrial Scents: Oil/Chemical
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Moderate', 'The acrid smell of leaked lubricants hangs heavy.', NULL, '["Chemical", "Oil"]'),
('Smell', 'Oppressive', 'Chemical residue burns your nostrils.', NULL, '["Chemical", "Burning"]'),
('Smell', 'Moderate', 'The air is thick with petroleum byproducts.', NULL, '["Chemical", "Petroleum"]');

-- Industrial Scents: Ozone
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Subtle', 'The sharp scent of ozone marks electrical activity.', NULL, '["Ozone", "Electrical"]'),
('Smell', 'Moderate', 'The air smells like a lightning strike.', NULL, '["Ozone", "Lightning"]');

-- Organic Scents: Decay
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Oppressive', 'The sweet stench of rot pervades everything.', NULL, '["Organic", "Decay"]'),
('Smell', 'Moderate', 'Decomposition fills your nose.', NULL, '["Organic", "Decomposition"]'),
('Smell', 'Oppressive', 'The smell of death is inescapable.', NULL, '["Organic", "Death"]');

-- Organic Scents: Mold/Mildew
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Moderate', 'Damp, musty air suggests fungal growth.', NULL, '["Organic", "Mold"]'),
('Smell', 'Moderate', 'The scent of mildew is pervasive.', NULL, '["Organic", "Mildew"]'),
('Smell', 'Subtle', 'A faint mustiness indicates moisture.', NULL, '["Organic", "Musty"]');

-- Biome-Specific Smell: Muspelheim (Brimstone/Sulfur)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Oppressive', 'Sulfur and superheated rock dominate.', 'Muspelheim', '["Brimstone", "Sulfur"]'),
('Smell', 'Oppressive', 'The air smells of volcanic fury.', 'Muspelheim', '["Volcanic", "Brimstone"]'),
('Smell', 'Oppressive', 'Sulfur and superheated rock dominate, burning your nostrils.', 'Muspelheim', '["Brimstone", "Overwhelming"]'),
('Smell', 'Moderate', 'The scent of brimstone fills the air.', 'Muspelheim', '["Brimstone", "Moderate"]'),
('Smell', 'Moderate', 'Superheated metal and ash create an acrid atmosphere.', 'Muspelheim', '["Metal", "Ash"]');

-- Biome-Specific Smell: Niflheim (Frozen/Sterile)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Moderate', 'The scent is crisp and painfully cold.', 'Niflheim', '["Cold", "Crisp"]'),
('Smell', 'Subtle', 'Frozen moisture gives the air a clean, sterile smell.', 'Niflheim', '["Cold', 'Sterile"]'),
('Smell', 'Moderate', 'The air smells of frozen ozone.', 'Niflheim', '["Cold", "Ozone"]');

-- Biome-Specific Smell: The Roots (Rust/Mildew)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Moderate', 'The air smells of rust and mildew.', 'The_Roots', '["Rust", "Mildew"]'),
('Smell', 'Moderate', 'Damp corrosion and fungal growth dominate.', 'The_Roots', '["Rust", "Fungal"]'),
('Smell', 'Oppressive', 'The stench of rust, mildew, and stagnant water is overpowering.', 'The_Roots', '["Rust", "Stagnant"]');

-- Biome-Specific Smell: Alfheim (Ozone/Burnt Reality)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Oppressive', 'The air smells of ozone and something burning that shouldn''t exist.', 'Alfheim', '["Ozone", "Reality_Burn"]'),
('Smell', 'Oppressive', 'An impossible scent—copper mixed with burnt sugar and despair.', 'Alfheim', '["Impossible", "Paradox"]'),
('Smell', 'Moderate', 'Ozone mingles with an indescribable wrongness.', 'Alfheim', '["Ozone", "Wrong"]');

-- Biome-Specific Smell: Jötunheim (Rust/Old Industry)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Smell', 'Moderate', 'The air smells of rust and old industry.', 'Jötunheim', '["Rust", "Industrial"]'),
('Smell', 'Subtle', 'Ancient lubricants and metal decay pervade.', 'Jötunheim', '["Industrial", "Decay"]'),
('Smell', 'Moderate', 'The scent of abandoned factories hangs heavy.', 'Jötunheim', '["Industrial", "Abandoned"]');

-- ============================================================
-- CATEGORY 4: TEMPERATURE (35+ descriptors)
-- ============================================================

-- Heat: Warm
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Subtle', 'The air is pleasantly warm.', NULL, '["Heat", "Warm"]'),
('Temperature', 'Subtle', 'Residual heat from machinery warms the space.', NULL, '["Heat", "Mechanical"]');

-- Heat: Hot
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Moderate', 'The temperature here is oppressively hot.', NULL, '["Heat", "Hot"]'),
('Temperature', 'Moderate', 'Heat radiates from every surface.', NULL, '["Heat", "Radiating"]'),
('Temperature', 'Moderate', 'Sweat forms immediately in this sauna-like environment.', NULL, '["Heat", "Humid"]');

-- Heat: Scorching
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Oppressive', 'The air burns your lungs with each breath.', NULL, '["Heat", "Scorching"]'),
('Temperature', 'Oppressive', 'Heat waves shimmer visibly in the air.', NULL, '["Heat", "Extreme"]'),
('Temperature', 'Oppressive', 'It feels like standing next to an open furnace.', NULL, '["Heat", "Furnace"]');

-- Cold: Cool
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Subtle', 'The air is refreshingly cool.', NULL, '["Cold", "Cool"]'),
('Temperature', 'Subtle', 'A pleasant chill pervades the space.', NULL, '["Cold", "Chill"]');

-- Cold: Cold
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Moderate', 'The temperature is uncomfortably cold.', NULL, '["Cold", "Uncomfortable"]'),
('Temperature', 'Moderate', 'Your breath fogs in the frigid air.', NULL, '["Cold", "Frigid"]'),
('Temperature', 'Moderate', 'Frost forms on exposed metal.', NULL, '["Cold", "Frost"]');

-- Cold: Freezing
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Oppressive', 'Bone-deep cold numbs your extremities.', NULL, '["Cold", "Freezing"]'),
('Temperature', 'Oppressive', 'Ice crystals hang suspended in the air.', NULL, '["Cold", "Ice_Crystals"]'),
('Temperature', 'Oppressive', 'The cold is physically painful.', NULL, '["Cold", "Painful"]');

-- Humidity: Dry
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Subtle', 'The air is desert-dry.', NULL, '["Humidity", "Dry"]'),
('Temperature', 'Moderate', 'Moisture has been leached from everything.', NULL, '["Humidity", "Desiccated"]');

-- Humidity: Humid
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Moderate', 'Oppressive humidity makes breathing difficult.', NULL, '["Humidity", "Oppressive"]'),
('Temperature', 'Moderate', 'Condensation drips from every surface.', NULL, '["Humidity", "Condensation"]'),
('Temperature', 'Oppressive', 'The air is thick with moisture.', NULL, '["Humidity", "Thick"]');

-- Biome-Specific Temperature: Muspelheim (Scorching)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Oppressive', 'Scorching heat makes each breath painful.', 'Muspelheim', '["Heat", "Scorching"]'),
('Temperature', 'Moderate', 'The heat is intense but bearable in short bursts.', 'Muspelheim', '["Heat", "Intense"]'),
('Temperature', 'Subtle', 'Warmth radiates from the volcanic rock.', 'Muspelheim', '["Heat", "Radiant"]');

-- Biome-Specific Temperature: Niflheim (Freezing)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Oppressive', 'The cold here defies description—bone-deep and merciless.', 'Niflheim', '["Cold", "Extreme"]'),
('Temperature', 'Moderate', 'Frost coats everything, and your breath freezes.', 'Niflheim', '["Cold", "Frost"]'),
('Temperature', 'Subtle', 'A chill pervades, never quite unbearable but always present.', 'Niflheim', '["Cold", "Persistent"]');

-- Biome-Specific Temperature: The Roots (Cool/Humid)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Moderate', 'Cool humidity clings to everything.', 'The_Roots', '["Cool", "Humid"]'),
('Temperature', 'Subtle', 'The air is cool and damp.', 'The_Roots', '["Cool", "Damp"]');

-- Biome-Specific Temperature: Alfheim (Paradox/Nonsensical)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Oppressive', 'Temperature makes no sense—hot and cold war against each other.', 'Alfheim', '["Paradox", "Impossible"]'),
('Temperature', 'Moderate', 'The air is simultaneously burning and freezing.', 'Alfheim', '["Paradox", "Contradictory"]');

-- Biome-Specific Temperature: Jötunheim (Cool/Still)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('Temperature', 'Moderate', 'The temperature is cool and still.', 'Jötunheim', '["Cool", "Still"]'),
('Temperature', 'Subtle', 'Stale air suggests no ventilation.', 'Jötunheim', '["Stale", "Still"]');

-- ============================================================
-- CATEGORY 5: PSYCHIC PRESENCE (25+ descriptors)
-- ============================================================

-- Runic Blight: Low
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Subtle', 'A faint unease prickles at the edges of consciousness.', NULL, '["Blight", "Low"]'),
('PsychicPresence', 'Subtle', 'Something feels slightly wrong here.', NULL, '["Blight", "Low"]'),
('PsychicPresence', 'Subtle', 'The air carries a sense of wrongness.', NULL, '["Blight", "Low"]');

-- Runic Blight: Moderate
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Moderate', 'The Runic Blight''s presence is palpable.', NULL, '["Blight", "Moderate"]'),
('PsychicPresence', 'Moderate', 'Reality feels thin and unstable.', NULL, '["Blight", "Moderate"]'),
('PsychicPresence', 'Moderate', 'Paradox lurks at the corners of perception.', NULL, '["Blight", "Moderate"]'),
('PsychicPresence', 'Moderate', 'Your mind protests against subtle contradictions in the space.', NULL, '["Blight", "Moderate"]');

-- Runic Blight: High
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Oppressive', 'The Blight''s corruption is overwhelming.', NULL, '["Blight", "High"]'),
('PsychicPresence', 'Oppressive', 'Your mind rebels against contradictions in the space.', NULL, '["Blight", "High"]'),
('PsychicPresence', 'Oppressive', 'Coherence itself seems to fray at the edges.', NULL, '["Blight", "High"]'),
('PsychicPresence', 'Oppressive', 'Reality warps visibly under the weight of paradox.', NULL, '["Blight", "High"]');

-- Forlorn Presence: Echoes
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Subtle', 'The ghosts of the dead linger here.', NULL, '["Forlorn", "Echoes"]'),
('PsychicPresence', 'Subtle', 'You sense watching eyes that aren''t there.', NULL, '["Forlorn", "Echoes"]'),
('PsychicPresence', 'Moderate', 'Sorrow hangs heavy in the air.', NULL, '["Forlorn", "Sorrow"]');

-- Forlorn Presence: Active
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Oppressive', 'Forlorn presence is unmistakable—the dead are close.', NULL, '["Forlorn", "Active"]'),
('PsychicPresence', 'Oppressive', 'The dead are too close, their anguish palpable.', NULL, '["Forlorn", "Active"]'),
('PsychicPresence', 'Oppressive', 'Anguish from 800 years past bleeds through.', NULL, '["Forlorn", "Ancient"]');

-- Biome-Specific Psychic: Muspelheim (Diminished Blight)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Subtle', 'The Blight seems diminished here, burned away by primal fire.', 'Muspelheim', '["Blight", "Diminished"]'),
('PsychicPresence', 'Subtle', 'Fire purges corruption, leaving the mind clearer.', 'Muspelheim', '["Blight", "Purged"]');

-- Biome-Specific Psychic: Niflheim (Frozen Blight)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Moderate', 'The Blight''s presence is frozen but not dormant.', 'Niflheim', '["Blight", "Frozen"]'),
('PsychicPresence', 'Moderate', 'Corruption sleeps beneath the ice, waiting.', 'Niflheim', '["Blight", "Sleeping"]');

-- Biome-Specific Psychic: Alfheim (Extreme Blight)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Oppressive', 'The Blight''s presence is overwhelming; reality itself frays at the edges.', 'Alfheim', '["Blight", "Extreme"]'),
('PsychicPresence', 'Oppressive', 'This is the epicenter—paradox made manifest.', 'Alfheim', '["Blight", "Epicenter"]'),
('PsychicPresence', 'Oppressive', 'Coherence is a suggestion, not a rule.', 'Alfheim', '["Blight", "Chaos"]');

-- Biome-Specific Psychic: The Roots (Moderate Blight)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Moderate', 'A faint unease suggests the Blight''s presence.', 'The_Roots', '["Blight", "Moderate"]'),
('PsychicPresence', 'Subtle', 'The Blight lurks at the edges, never quite absent.', 'The_Roots', '["Blight", "Lurking"]');

-- Biome-Specific Psychic: Jötunheim (Industrial Ghosts)
INSERT OR IGNORE INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags)
VALUES
('PsychicPresence', 'Moderate', 'Echoes of titanic labor linger in the silence.', 'Jötunheim', '["Forlorn", "Industrial"]'),
('PsychicPresence', 'Subtle', 'The ghosts of industry haunt this place.', 'Jötunheim', '["Forlorn", "Ghost"]');

-- ============================================================
-- Total Descriptors: 155
-- Lighting: 33, Sound: 40, Smell: 38, Temperature: 29, PsychicPresence: 25
-- ============================================================
