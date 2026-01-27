-- =====================================================
-- v0.38.18: Psychological Descriptor Schema
-- =====================================================
-- Version: v0.38.18
-- Author: Rune & Rust Development Team
-- Date: 2025-12-14
-- Prerequisites: v0.38.0 Descriptor Framework Schema
-- =====================================================
-- Extends the descriptor framework with psychological state tables
-- for stress, trauma, corruption, and biome-specific pressure.
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- STRESS DESCRIPTORS
-- =====================================================
-- Descriptors for stress accumulation at different thresholds

CREATE TABLE IF NOT EXISTS Psychological_Stress_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Stress threshold: 0-25, 26-50, 51-75, 76-99, 100
    threshold TEXT NOT NULL CHECK(threshold IN ('Minimal', 'Mounting', 'Critical', 'Breaking', 'Broken')),
    
    -- Type of manifestation: Physical, Mental, Behavioral
    manifestation TEXT NOT NULL CHECK(manifestation IN ('Physical', 'Mental', 'Behavioral')),
    
    -- The descriptor text
    descriptor_text TEXT NOT NULL,
    
    -- Selection properties
    weight REAL DEFAULT 1.0,
    intensity TEXT CHECK(intensity IN ('Subtle', 'Moderate', 'Oppressive')),
    tags TEXT,  -- JSON array
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_stress_descriptors_threshold ON Psychological_Stress_Descriptors(threshold);
CREATE INDEX IF NOT EXISTS idx_stress_descriptors_manifestation ON Psychological_Stress_Descriptors(manifestation);

-- =====================================================
-- TRAUMA DESCRIPTORS
-- =====================================================
-- Descriptors for active trauma manifestations

CREATE TABLE IF NOT EXISTS Psychological_Trauma_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Trauma type
    trauma_type TEXT NOT NULL CHECK(trauma_type IN ('Flashback', 'Panic', 'Dissociation', 'Hypervigilance', 'Avoidance')),
    
    -- Intensity level
    intensity TEXT NOT NULL CHECK(intensity IN ('Mild', 'Moderate', 'Severe')),
    
    -- The descriptor text
    descriptor_text TEXT NOT NULL,
    
    -- Trigger context (optional)
    trigger_context TEXT,  -- 'Combat', 'Darkness', 'Fire', etc.
    
    -- Selection properties
    weight REAL DEFAULT 1.0,
    tags TEXT,  -- JSON array
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trauma_descriptors_type ON Psychological_Trauma_Descriptors(trauma_type);
CREATE INDEX IF NOT EXISTS idx_trauma_descriptors_intensity ON Psychological_Trauma_Descriptors(intensity);

-- =====================================================
-- CORRUPTION DESCRIPTORS
-- =====================================================
-- Descriptors for corruption progression

CREATE TABLE IF NOT EXISTS Psychological_Corruption_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Corruption threshold: 0-10, 11-30, 31-50, 51-75, 76-99, 100
    threshold TEXT NOT NULL CHECK(threshold IN ('Clean', 'Touched', 'Tainted', 'Corrupted', 'Lost', 'Forlorn')),
    
    -- Type of manifestation: Physical, Mental, Social
    manifestation TEXT NOT NULL CHECK(manifestation IN ('Physical', 'Mental', 'Social', 'Urgent', 'Final')),
    
    -- The descriptor text
    descriptor_text TEXT NOT NULL,
    
    -- Selection properties
    weight REAL DEFAULT 1.0,
    tags TEXT,  -- JSON array
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_corruption_descriptors_threshold ON Psychological_Corruption_Descriptors(threshold);
CREATE INDEX IF NOT EXISTS idx_corruption_descriptors_manifestation ON Psychological_Corruption_Descriptors(manifestation);

-- =====================================================
-- BIOME PSYCHOLOGICAL PRESSURE
-- =====================================================
-- Biome-specific psychological effects

CREATE TABLE IF NOT EXISTS Psychological_Biome_Pressure (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Which biome
    biome TEXT NOT NULL CHECK(biome IN ('The_Roots', 'Muspelheim', 'Niflheim', 'Svartalfheim', 'Vanaheim', 'Jotunheim')),
    
    -- Pressure type: Isolation, Heat, Cold, Darkness, Growth, Scale
    pressure_type TEXT NOT NULL,
    
    -- The descriptor text
    descriptor_text TEXT NOT NULL,
    
    -- Selection properties
    weight REAL DEFAULT 1.0,
    intensity TEXT CHECK(intensity IN ('Subtle', 'Moderate', 'Oppressive')),
    tags TEXT,  -- JSON array
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_biome_pressure_biome ON Psychological_Biome_Pressure(biome);
CREATE INDEX IF NOT EXISTS idx_biome_pressure_type ON Psychological_Biome_Pressure(pressure_type);

-- =====================================================
-- RECOVERY DESCRIPTORS
-- =====================================================
-- Descriptors for recovery and grounding moments

CREATE TABLE IF NOT EXISTS Psychological_Recovery_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Recovery type
    recovery_type TEXT NOT NULL CHECK(recovery_type IN ('Rest', 'Social', 'Grounding', 'Support', 'Milestone', 'Acceptance')),
    
    -- The descriptor text
    descriptor_text TEXT NOT NULL,
    
    -- Selection properties
    weight REAL DEFAULT 1.0,
    tags TEXT,  -- JSON array
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_recovery_descriptors_type ON Psychological_Recovery_Descriptors(recovery_type);

-- =====================================================
-- SEED DATA: Stress Descriptors
-- =====================================================

INSERT OR IGNORE INTO Psychological_Stress_Descriptors (threshold, manifestation, descriptor_text, weight, intensity, tags) VALUES
-- Minimal (0-25)
('Minimal', 'Physical', 'Your grip tightens on your weapon — unconscious, instinctive', 1.0, 'Subtle', '["Combat", "Tension"]'),
('Minimal', 'Physical', 'A slight headache pulses behind your eyes. Nothing serious. Yet', 1.0, 'Subtle', '["Early", "Warning"]'),
('Minimal', 'Mental', 'Your thoughts are clear, focused. You can handle this', 1.0, 'Subtle', '["Positive", "Confident"]'),
-- Mounting (26-50)
('Mounting', 'Physical', 'Your jaw aches from clenching. You did not notice you were doing it', 1.0, 'Moderate', '["Tension", "Unconscious"]'),
('Mounting', 'Physical', 'Sweat breaks out despite the cold. Your body knows something is wrong', 1.0, 'Moderate', '["Danger", "Autonomic"]'),
('Mounting', 'Mental', 'Shadows seem to move more than they should. You are seeing threats everywhere', 1.0, 'Moderate', '["Paranoia", "Visual"]'),
('Mounting', 'Mental', 'Every sound makes you flinch. You are wound too tight', 1.0, 'Moderate', '["Hypervigilance", "Sound"]'),
-- Critical (51-75)
('Critical', 'Physical', 'Your heart pounds in your ears, drowning out everything else', 1.0, 'Oppressive', '["Panic", "Heartbeat"]'),
('Critical', 'Physical', 'Hands shake visibly now. Fine motor control is compromised', 1.0, 'Oppressive', '["Trembling", "Control"]'),
('Critical', 'Mental', 'Thoughts fragment. Complete sentences are getting harder', 1.0, 'Oppressive', '["Fragmented", "Disoriented"]'),
('Critical', 'Behavioral', 'You snap at allies. The words leave before you can stop them', 1.0, 'Oppressive', '["Aggressive", "Social"]'),
-- Breaking (76-99)
('Breaking', 'Physical', 'Your whole body trembles. The shaking will not stop', 1.0, 'Oppressive', '["Trembling", "Uncontrollable"]'),
('Breaking', 'Physical', 'Vision tunnels. The world narrows to a point', 1.0, 'Oppressive', '["Tunnel Vision", "Panic"]'),
('Breaking', 'Mental', 'Thoughts scream, overlap, contradict. Coherence is failing', 1.0, 'Oppressive', '["Fragmented", "Overwhelmed"]'),
('Breaking', 'Behavioral', 'Fight or flight — the only options that make sense now', 1.0, 'Oppressive', '["Primal", "Survival"]'),
-- Broken (100)
('Broken', 'Mental', 'Something inside you SNAPS. A string pulled too tight, finally giving', 1.0, 'Oppressive', '["Break", "Collapse"]'),
('Broken', 'Mental', 'The world tilts. Reality refuses to hold its shape', 1.0, 'Oppressive', '["Break", "Dissociation"]'),
('Broken', 'Mental', 'Control is gone. Your body moves without your permission', 1.0, 'Oppressive', '["Break", "Loss of Control"]');

-- =====================================================
-- SEED DATA: Trauma Descriptors
-- =====================================================

INSERT OR IGNORE INTO Psychological_Trauma_Descriptors (trauma_type, intensity, descriptor_text, weight, tags) VALUES
-- Flashback
('Flashback', 'Mild', 'A memory surfaces, unwanted. You push it back down', 1.0, '["Memory", "Controlled"]'),
('Flashback', 'Moderate', 'The past bleeds into the present. You see both at once', 1.0, '["Memory", "Overlay"]'),
('Flashback', 'Severe', 'You are there again. Not here. THERE. Where it happened', 1.0, '["Memory", "Immersive"]'),
-- Panic
('Panic', 'Mild', 'Your heart skips. A lurch of fear, quickly controlled', 1.0, '["Fear", "Controlled"]'),
('Panic', 'Moderate', 'Panic claws at your chest. Breathing becomes a conscious effort', 1.0, '["Fear", "Physical"]'),
('Panic', 'Severe', 'You cannot breathe. You cannot think. You can only feel the terror', 1.0, '["Fear", "Overwhelming"]'),
-- Dissociation
('Dissociation', 'Mild', 'For a moment, you feel distant from your body. A step removed', 1.0, '["Detachment", "Mild"]'),
('Dissociation', 'Moderate', 'You are watching yourself from outside. A puppet without a script', 1.0, '["Detachment", "Depersonalization"]'),
('Dissociation', 'Severe', 'You are not sure who you are. The name feels like a word, not an identity', 1.0, '["Detachment", "Identity"]'),
-- Hypervigilance
('Hypervigilance', 'Mild', 'You are watching the shadows closer than you need to', 1.0, '["Alert", "Caution"]'),
('Hypervigilance', 'Moderate', 'You cannot relax. Your body refuses. There is danger everywhere', 1.0, '["Alert", "Unending"]'),
('Hypervigilance', 'Severe', 'Everyone is a threat. Everything is a trap. Trust is impossible', 1.0, '["Paranoid", "Isolated"]');

-- =====================================================
-- SEED DATA: Corruption Descriptors
-- =====================================================

INSERT OR IGNORE INTO Psychological_Corruption_Descriptors (threshold, manifestation, descriptor_text, weight, tags) VALUES
-- Touched (11-30)
('Touched', 'Physical', 'A paleness that does not wash away. A coldness that does not warm', 1.0, '["Subtle", "Physical"]'),
('Touched', 'Mental', 'Dreams that are not yours. Memories that never happened', 1.0, '["Dreams", "Foreign"]'),
-- Tainted (31-50)
('Tainted', 'Physical', 'Veins show through the skin, darker than they should be', 1.0, '["Visible", "Veins"]'),
('Tainted', 'Physical', 'Your shadow does not always match your movements', 1.0, '["Shadow", "Wrong"]'),
('Tainted', 'Mental', 'The whispers have words now. You are trying not to understand them', 1.0, '["Whispers", "Words"]'),
('Tainted', 'Social', 'People look at you differently. They sense what you are becoming', 1.0, '["Social", "Recognition"]'),
-- Corrupted (51-75)
('Corrupted', 'Physical', 'Your skin has changed texture. Not quite human anymore', 1.0, '["Transformation", "Skin"]'),
('Corrupted', 'Physical', 'One eye does not blink the same as the other', 1.0, '["Asymmetry", "Eyes"]'),
('Corrupted', 'Mental', 'The whispers have become a voice. It knows your name', 1.0, '["Voice", "Named"]'),
('Corrupted', 'Mental', 'The hunger is new. You do not want to think about what it wants', 1.0, '["Hunger", "Unknown"]'),
('Corrupted', 'Social', 'Allies keep their distance. They are right to', 1.0, '["Social", "Isolated"]'),
-- Lost (76-99)
('Lost', 'Physical', 'Looking in mirrors is inadvisable. What looks back is not entirely you', 1.0, '["Mirror", "Other"]'),
('Lost', 'Physical', 'Your body obeys, but reluctantly. It has its own agenda now', 1.0, '["Body", "Rebellion"]'),
('Lost', 'Mental', 'You are losing yourself. Watching it happen. Unable to stop it', 1.0, '["Self", "Fading"]'),
('Lost', 'Urgent', 'There is still time. Maybe. If you can find help. If anyone will help you', 1.0, '["Desperate", "Hope"]'),
-- Forlorn (100)
('Forlorn', 'Final', 'The change is complete. What was you is now... other', 1.0, '["Final", "Complete"]'),
('Forlorn', 'Final', 'You fall. What rises wearing your face is no longer you', 1.0, '["Final", "Transformation"]');

-- =====================================================
-- SEED DATA: Biome Pressure Descriptors
-- =====================================================

INSERT OR IGNORE INTO Psychological_Biome_Pressure (biome, pressure_type, descriptor_text, weight, intensity, tags) VALUES
-- The Roots
('The_Roots', 'Isolation', 'How long since you have seen another person? Days? Weeks?', 1.0, 'Moderate', '["Isolation", "Time"]'),
('The_Roots', 'Claustrophobia', 'The walls press in. They are not moving. You know that. They are not moving', 1.0, 'Oppressive', '["Space", "Walls"]'),
('The_Roots', 'Dread', 'Every corridor could be your last. Every room could be a tomb', 1.0, 'Moderate', '["Death", "Possibility"]'),
-- Muspelheim
('Muspelheim', 'Heat', 'Your thoughts move slowly, boiled sluggish by the heat', 1.0, 'Moderate', '["Heat", "Cognitive"]'),
('Muspelheim', 'Thirst', 'Water. Just water. You would kill for water', 1.0, 'Oppressive', '["Thirst", "Desperate"]'),
('Muspelheim', 'Fury', 'The heat breeds anger. Everything is an enemy', 1.0, 'Moderate', '["Anger", "Heat"]'),
-- Niflheim
('Niflheim', 'Cold', 'The cold takes emotion first. Then hope. Then everything else', 1.0, 'Oppressive', '["Cold", "Emotional"]'),
('Niflheim', 'Despair', 'What is the point? The cold has already decided the outcome', 1.0, 'Oppressive', '["Hopelessness", "Fatalism"]'),
('Niflheim', 'Stillness', 'Part of you wants to stop. To freeze. To become ice', 1.0, 'Oppressive', '["Stillness", "Death Wish"]'),
-- Svartalfheim
('Svartalfheim', 'Darkness', 'The dark has weight here. Presence. Intent', 1.0, 'Oppressive', '["Darkness", "Presence"]'),
('Svartalfheim', 'Paranoia', 'Something clicked. Close. Too close. Where?', 1.0, 'Moderate', '["Sound", "Paranoia"]'),
('Svartalfheim', 'Madness', 'You are talking to yourself. The silence is worse', 1.0, 'Moderate', '["Madness", "Isolation"]'),
-- Vanaheim
('Vanaheim', 'Consumption', 'The jungle wants to eat you. It is patient. It has time', 1.0, 'Moderate', '["Growth", "Predatory"]'),
('Vanaheim', 'Infection', 'Spores in your lungs. Pollen in your blood. Are you still you?', 1.0, 'Oppressive', '["Infection", "Identity"]'),
('Vanaheim', 'Alienation', 'This life does not want you here. Every plant is hostile', 1.0, 'Moderate', '["Nature", "Hostility"]');

-- =====================================================
-- SEED DATA: Recovery Descriptors
-- =====================================================

INSERT OR IGNORE INTO Psychological_Recovery_Descriptors (recovery_type, descriptor_text, weight, tags) VALUES
('Rest', 'Breathing slows. The shaking stops. You are still here', 1.0, '["Physical", "Calming"]'),
('Rest', 'Silence, but the good kind. The safe kind', 1.0, '["Safety", "Peace"]'),
('Social', 'Another voice. Human. Not threatening. You had forgotten the sound', 1.0, '["Human Contact", "Relief"]'),
('Social', 'They are watching your back. You can lower your guard, just a little', 1.0, '["Trust", "Safety"]'),
('Grounding', 'Count the exits. Name the objects. You are here. You are now. You are safe', 1.0, '["Technique", "Present"]'),
('Grounding', 'Breathe. Just breathe. The memory is then. This is now', 1.0, '["Technique", "Separation"]'),
('Support', 'Someone who understands. They have been there too', 1.0, '["Understanding", "Connection"]'),
('Support', 'You do not have to explain. They know. They survived it too', 1.0, '["Understanding", "Shared"]'),
('Acceptance', 'The scar is part of you now. It does not have to define you', 1.0, '["Growth", "Integration"]'),
('Milestone', 'You survived. Again. It does not feel like victory, but it is', 1.0, '["Survival", "Achievement"]');

COMMIT;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Test 1: Stress Descriptors by threshold
-- SELECT threshold, COUNT(*) FROM Psychological_Stress_Descriptors GROUP BY threshold;
-- Expected: Minimal ~3, Mounting ~4, Critical ~4, Breaking ~4, Broken ~3

-- Test 2: Trauma Descriptors by type
-- SELECT trauma_type, COUNT(*) FROM Psychological_Trauma_Descriptors GROUP BY trauma_type;
-- Expected: Each type ~3

-- Test 3: Corruption Descriptors by threshold
-- SELECT threshold, COUNT(*) FROM Psychological_Corruption_Descriptors GROUP BY threshold;
-- Expected: Touched ~2, Tainted ~4, Corrupted ~5, Lost ~4, Forlorn ~2

-- Test 4: Biome Pressure by biome
-- SELECT biome, COUNT(*) FROM Psychological_Biome_Pressure GROUP BY biome;
-- Expected: Each biome ~3

-- =====================================================
-- END v0.38.18 PSYCHOLOGICAL DESCRIPTOR SCHEMA
-- =====================================================
