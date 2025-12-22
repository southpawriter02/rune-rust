-- =====================================================
-- v0.38.17: Entity Descriptor Schema
-- =====================================================
-- Version: v0.38.17
-- Author: Rune & Rust Development Team
-- Date: 2025-12-14
-- Prerequisites: v0.38.0 Descriptor Framework Schema
-- =====================================================
-- Extends the descriptor framework with entity-specific tables
-- for races, factions, specializations, and magic descriptors.
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- RACE DESCRIPTORS
-- =====================================================
-- Descriptors for playable and NPC races

CREATE TABLE IF NOT EXISTS Entity_Race_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    race_id TEXT NOT NULL,  -- 'Dvergr', 'Dokkalfar', 'Ljosalfar', 'Jotun_Forged', 'Human'
    
    -- Category: build, skin, eyes, hair, movement, speech, mannerism
    category TEXT NOT NULL CHECK(category IN ('Build', 'Skin', 'Eyes', 'Hair', 'Movement', 'Speech', 'Mannerism', 'Aura')),
    
    -- The descriptor text
    descriptor_text TEXT NOT NULL,
    
    -- Selection properties
    weight REAL DEFAULT 1.0,
    tags TEXT,  -- JSON array: ["Industrial", "Forge-Born"]
    
    -- Thematic modifier affinity (optional)
    modifier_affinity TEXT,  -- 'Forge-Born', 'Stone-Touched', etc.
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_race_descriptors_race ON Entity_Race_Descriptors(race_id);
CREATE INDEX IF NOT EXISTS idx_race_descriptors_category ON Entity_Race_Descriptors(category);

-- =====================================================
-- RACE THEMATIC MODIFIERS
-- =====================================================
-- Modifiers that can be applied to race descriptors

CREATE TABLE IF NOT EXISTS Entity_Race_Modifiers (
    modifier_id INTEGER PRIMARY KEY AUTOINCREMENT,
    modifier_name TEXT NOT NULL UNIQUE,  -- 'Forge-Born', 'Echo-Touched', 'Rust-Afflicted'
    race_id TEXT NOT NULL,  -- Which race this modifier applies to
    
    -- Modifier properties
    adjective TEXT NOT NULL,  -- 'ash-marked', 'resonant'
    detail_fragment TEXT NOT NULL,  -- 'bears the scars of the deep forges'
    
    -- Biome/origin affinity
    biome_affinity TEXT,  -- 'Muspelheim', 'Svartalfheim', etc.
    
    -- Tags for filtering
    tags TEXT,  -- JSON array
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_race_modifiers_race ON Entity_Race_Modifiers(race_id);

-- =====================================================
-- FACTION DESCRIPTORS
-- =====================================================
-- Descriptors for human factions and other groups

CREATE TABLE IF NOT EXISTS Entity_Faction_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    faction_id TEXT NOT NULL,  -- 'Hearth_Clans', 'Grove_Clans', 'Rust_Clans', 'Forsaken'
    
    -- Category
    category TEXT NOT NULL CHECK(category IN ('Build', 'Skin', 'Eyes', 'Clothing', 'Movement', 'Speech', 'Demeanor', 'Work')),
    
    -- The descriptor text
    descriptor_text TEXT NOT NULL,
    
    -- Selection properties
    weight REAL DEFAULT 1.0,
    tags TEXT,  -- JSON array
    
    -- Origin biome
    origin_biome TEXT,  -- 'Muspelheim', 'Vanaheim', etc.
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_faction_descriptors_faction ON Entity_Faction_Descriptors(faction_id);
CREATE INDEX IF NOT EXISTS idx_faction_descriptors_category ON Entity_Faction_Descriptors(category);

-- =====================================================
-- SPECIALIZATION DESCRIPTORS
-- =====================================================
-- Descriptors for character specializations

CREATE TABLE IF NOT EXISTS Entity_Specialization_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    specialization_id TEXT NOT NULL,  -- 'Berserkr', 'Skjaldmaer', 'Bone_Setter', etc.
    
    -- Category
    category TEXT NOT NULL CHECK(category IN ('Stance', 'Tells', 'Aura', 'Equipment', 'Mannerism')),
    
    -- The descriptor text
    descriptor_text TEXT NOT NULL,
    
    -- Selection properties
    weight REAL DEFAULT 1.0,
    tags TEXT,  -- JSON array
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_spec_descriptors_spec ON Entity_Specialization_Descriptors(specialization_id);
CREATE INDEX IF NOT EXISTS idx_spec_descriptors_category ON Entity_Specialization_Descriptors(category);

-- =====================================================
-- MAGIC DESCRIPTORS
-- =====================================================
-- Descriptors for magical manifestations

CREATE TABLE IF NOT EXISTS Magic_Descriptors (
    descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
    magic_type TEXT NOT NULL CHECK(magic_type IN ('Galdr', 'Runic', 'Blight', 'Wild', 'Blood')),
    
    -- Stage of magic: Casting, Building, Release, Failure, Decay
    stage TEXT NOT NULL CHECK(stage IN ('Casting', 'Building', 'Release', 'Failure', 'Decay', 'Corruption')),
    
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

CREATE INDEX IF NOT EXISTS idx_magic_descriptors_type ON Magic_Descriptors(magic_type);
CREATE INDEX IF NOT EXISTS idx_magic_descriptors_stage ON Magic_Descriptors(stage);

-- =====================================================
-- NPC COMPOSITE TABLE
-- =====================================================
-- Pre-generated NPC descriptions combining race + faction + specialization

CREATE TABLE IF NOT EXISTS Entity_NPC_Composites (
    composite_id INTEGER PRIMARY KEY AUTOINCREMENT,
    npc_id TEXT NOT NULL UNIQUE,  -- Links to NPC master record
    
    -- Component references
    race_id TEXT NOT NULL,
    faction_id TEXT,  -- NULL for non-faction NPCs
    specialization_id TEXT,  -- NULL for non-specialized NPCs
    
    -- Generated composite description
    appearance_description TEXT NOT NULL,  -- Full physical description
    behavioral_description TEXT,  -- Movement, speech patterns
    
    -- Modifiers applied
    race_modifier_id INTEGER,  -- FK to Entity_Race_Modifiers
    
    -- Metadata
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (race_modifier_id) REFERENCES Entity_Race_Modifiers(modifier_id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_npc_composites_race ON Entity_NPC_Composites(race_id);
CREATE INDEX IF NOT EXISTS idx_npc_composites_faction ON Entity_NPC_Composites(faction_id);

-- =====================================================
-- SEED DATA: Dvergr Race Descriptors
-- =====================================================

INSERT OR IGNORE INTO Entity_Race_Descriptors (race_id, category, descriptor_text, weight, tags) VALUES
-- Build
('Dvergr', 'Build', 'Broad-shouldered and compact, built like a mining rig given flesh', 1.0, '["Industrial", "Strong"]'),
('Dvergr', 'Build', 'Dense muscle packed on a frame designed for tunnel work', 1.0, '["Industrial", "Strong"]'),
('Dvergr', 'Build', 'Squat and powerful, with hands like excavator claws', 1.0, '["Industrial", "Intimidating"]'),
-- Skin
('Dvergr', 'Skin', 'Skin the color of worn granite, with a texture like polished stone', 1.0, '["Stone", "Mineral"]'),
('Dvergr', 'Skin', 'Complexion of iron ore, with mineral flecks catching what light exists', 1.0, '["Mineral", "Metallic"]'),
('Dvergr', 'Skin', 'Hide like tanned leather stretched over bedrock', 1.0, '["Tough", "Weathered"]'),
-- Eyes
('Dvergr', 'Eyes', 'Eyes like bore-holes, deep-set and glinting with forge-light', 1.0, '["Intense", "Industrial"]'),
('Dvergr', 'Eyes', 'Calculating eyes that assess structural integrity unconsciously', 1.0, '["Analytical", "Technical"]'),
('Dvergr', 'Eyes', 'Dark eyes that seem to absorb light like mine shafts', 1.0, '["Dark", "Deep"]'),
-- Movement
('Dvergr', 'Movement', 'Moves with the patient inevitability of a glacier', 1.0, '["Slow", "Deliberate"]'),
('Dvergr', 'Movement', 'Each step deliberate, testing ground weight capacity', 1.0, '["Cautious", "Technical"]'),
('Dvergr', 'Movement', 'Walks like they expect the floor to collapse', 1.0, '["Paranoid", "Careful"]'),
-- Speech
('Dvergr', 'Speech', 'Voice like gravel in a tumbler — words ground out slowly', 1.0, '["Deep", "Rough"]'),
('Dvergr', 'Speech', 'Speaks in precise, engineering-grade sentences', 1.0, '["Technical", "Precise"]'),
('Dvergr', 'Speech', 'Says nothing that is not load-bearing', 1.0, '["Economical", "Direct"]');

-- =====================================================
-- SEED DATA: Dökkálfar Race Descriptors
-- =====================================================

INSERT OR IGNORE INTO Entity_Race_Descriptors (race_id, category, descriptor_text, weight, tags) VALUES
-- Build
('Dokkalfar', 'Build', 'Elongated and angular, as if stretched by the darkness itself', 1.0, '["Thin", "Unsettling"]'),
('Dokkalfar', 'Build', 'Slender to the point of emaciation, every bone visible', 1.0, '["Thin", "Frail"]'),
('Dokkalfar', 'Build', 'Moves like a shadow given reluctant substance', 1.0, '["Ethereal", "Dark"]'),
-- Skin
('Dokkalfar', 'Skin', 'Skin like bleached bone, translucent enough to show violet veins', 1.0, '["Pale", "Translucent"]'),
('Dokkalfar', 'Skin', 'Pallor of something that has not seen sunlight in generations', 1.0, '["Pale", "Sunless"]'),
('Dokkalfar', 'Skin', 'Flesh the color of cave-fish, faintly luminescent', 1.0, '["Pale", "Bioluminescent"]'),
-- Eyes
('Dokkalfar', 'Eyes', 'Eyes like pools of ink, with no visible whites', 1.0, '["Dark", "Unsettling"]'),
('Dokkalfar', 'Eyes', 'Pupils that expand to consume the entire eye in darkness', 1.0, '["Adaptive", "Dark"]'),
('Dokkalfar', 'Eyes', 'Eyes that reflect light that is not there', 1.0, '["Mystical", "Unsettling"]'),
-- Movement
('Dokkalfar', 'Movement', 'Moves through darkness as if it were their native medium', 1.0, '["Stealthy", "Dark"]'),
('Dokkalfar', 'Movement', 'Glides rather than walks, feet barely touching ground', 1.0, '["Graceful", "Ethereal"]'),
('Dokkalfar', 'Movement', 'Steps around obstacles you cannot see', 1.0, '["Perceptive", "Dark"]');

-- =====================================================
-- SEED DATA: Human Faction Descriptors
-- =====================================================

-- Hearth-Clans (Muspelheim)
INSERT OR IGNORE INTO Entity_Faction_Descriptors (faction_id, category, descriptor_text, weight, origin_biome, tags) VALUES
('Hearth_Clans', 'Build', 'Compact and heat-hardened, skin like sun-cured leather', 1.0, 'Muspelheim', '["Heat-Resistant", "Strong"]'),
('Hearth_Clans', 'Build', 'Barrel-chested and sweating, even in cold environments', 1.0, 'Muspelheim', '["Heat-Adapted"]'),
('Hearth_Clans', 'Skin', 'Burn scars worn like badges of honor, mapping years of forge-work', 1.0, 'Muspelheim', '["Scarred", "Experienced"]'),
('Hearth_Clans', 'Clothing', 'Heat-resistant leathers and fire-touched metals', 1.0, 'Muspelheim', '["Protective", "Industrial"]'),
('Hearth_Clans', 'Speech', 'Speaks like a bellows — short, forceful bursts', 1.0, 'Muspelheim', '["Direct", "Forceful"]'),
('Hearth_Clans', 'Demeanor', 'Suspicious of outsiders, fiercely loyal to clan', 1.0, 'Muspelheim', '["Loyal", "Suspicious"]');

-- Rust-Clans (Jötunheim)
INSERT OR IGNORE INTO Entity_Faction_Descriptors (faction_id, category, descriptor_text, weight, origin_biome, tags) VALUES
('Rust_Clans', 'Build', 'Wiry and quick, built for squeezing through ruins', 1.0, 'Jotunheim', '["Agile", "Lean"]'),
('Rust_Clans', 'Build', 'Lean from irregular meals, muscled from hauling salvage', 1.0, 'Jotunheim', '["Lean", "Strong"]'),
('Rust_Clans', 'Skin', 'Rust permanently embedded under fingernails and in skin creases', 1.0, 'Jotunheim', '["Stained", "Weathered"]'),
('Rust_Clans', 'Clothing', 'Wears mismatched salvage turned into armor — no two pieces alike', 1.0, 'Jotunheim', '["Salvage", "Mismatched"]'),
('Rust_Clans', 'Movement', 'Tests every surface before trusting their weight to it', 1.0, 'Jotunheim', '["Cautious", "Experienced"]'),
('Rust_Clans', 'Speech', 'Speaks in Gutter-Cant — fast, clipped, transactional', 1.0, 'Jotunheim', '["Cant", "Direct"]');

-- Forsaken (Niflheim)
INSERT OR IGNORE INTO Entity_Faction_Descriptors (faction_id, category, descriptor_text, weight, origin_biome, tags) VALUES
('Forsaken', 'Build', 'Hollow-cheeked and haggard, every calorie shows', 1.0, 'Niflheim', '["Thin", "Starved"]'),
('Forsaken', 'Build', 'Hunched from cold and exhaustion, never fully straightening', 1.0, 'Niflheim', '["Hunched", "Broken"]'),
('Forsaken', 'Skin', 'Frostbite scars on extremities — fingers, ears, nose', 1.0, 'Niflheim', '["Frostbitten", "Scarred"]'),
('Forsaken', 'Eyes', 'Dead eyes that have stopped hoping for rescue', 1.0, 'Niflheim', '["Hopeless", "Dead"]'),
('Forsaken', 'Clothing', 'Patched and re-patched cold-weather gear, held together by desperation', 1.0, 'Niflheim', '["Patched", "Desperate"]'),
('Forsaken', 'Demeanor', 'Flinches from authority, obeys without question', 1.0, 'Niflheim', '["Broken", "Obedient"]');

-- =====================================================
-- SEED DATA: Specialization Descriptors
-- =====================================================

INSERT OR IGNORE INTO Entity_Specialization_Descriptors (specialization_id, category, descriptor_text, weight, tags) VALUES
-- Berserkr
('Berserkr', 'Stance', 'Stands with barely contained tension, muscles locked to prevent premature explosion', 1.0, '["Tense", "Dangerous"]'),
('Berserkr', 'Tells', 'Eyes that flicker between focus and something primal, feral', 1.0, '["Unstable", "Feral"]'),
('Berserkr', 'Tells', 'Scars that look self-inflicted — the marks of rage turned inward', 1.0, '["Scarred", "Self-Destructive"]'),
('Berserkr', 'Aura', 'The air around them feels charged, dangerous, unstable', 1.0, '["Dangerous", "Charged"]'),
('Berserkr', 'Mannerism', 'Jaw clenches and unclenches rhythmically, grinding teeth', 1.0, '["Tense", "Restless"]'),
-- Bone-Setter
('Bone_Setter', 'Stance', 'Hands held slightly forward, ready to catch, to mend, to steady', 1.0, '["Ready", "Careful"]'),
('Bone_Setter', 'Tells', 'Calluses from wrapping bandages, setting splints, applying pressure', 1.0, '["Healer", "Experienced"]'),
('Bone_Setter', 'Tells', 'Clothing spotted with old bloodstains that never fully wash out', 1.0, '["Bloodstained", "Experienced"]'),
('Bone_Setter', 'Aura', 'Radiates calm under pressure — the eye of any storm', 1.0, '["Calm", "Stable"]'),
('Bone_Setter', 'Equipment', 'Medical kit always within reach, tools organized by emergency priority', 1.0, '["Prepared", "Organized"]');

-- =====================================================
-- SEED DATA: Magic Descriptors
-- =====================================================

INSERT OR IGNORE INTO Magic_Descriptors (magic_type, stage, descriptor_text, weight, intensity, tags) VALUES
-- Galdr
('Galdr', 'Casting', 'Voice rises in frequencies that drill into the ear', 1.0, 'Moderate', '["Sound", "Painful"]'),
('Galdr', 'Casting', 'Words that are not quite words, syllables that hurt to hear', 1.0, 'Moderate', '["Sound", "Wrong"]'),
('Galdr', 'Building', 'The air thickens, charged with potential, waiting to discharge', 1.0, 'Moderate', '["Pressure", "Building"]'),
('Galdr', 'Release', 'Reality stutters, skips, then reconfigures around the effect', 1.0, 'Oppressive', '["Reality", "Distortion"]'),
('Galdr', 'Failure', 'The song cracks, discordant, and something wrong bleeds through', 1.0, 'Oppressive', '["Failure", "Wrong"]'),
-- Runic
('Runic', 'Casting', 'Runes flare with light that seems to come from behind reality', 1.0, 'Moderate', '["Light", "Mystical"]'),
('Runic', 'Release', 'The inscribed pattern projects its meaning into the world', 1.0, 'Moderate', '["Pattern", "Manifestation"]'),
('Runic', 'Decay', 'Light stutters, flickers, fades — the rune power exhausted', 1.0, 'Subtle', '["Fading", "Exhausted"]'),
('Runic', 'Corruption', 'Something wrong in the pattern — the rune writhes, becoming other', 1.0, 'Oppressive', '["Corruption", "Wrong"]'),
-- Blight
('Blight', 'Casting', 'A wrongness at the edge of perception, like a word you forgot', 1.0, 'Subtle', '["Subtle", "Wrong"]'),
('Blight', 'Building', 'Reality texture roughens — edges blur, colors shift', 1.0, 'Moderate', '["Reality", "Distortion"]'),
('Blight', 'Release', 'The world code itself fragments — geometry stops behaving', 1.0, 'Oppressive', '["Reality", "Breakdown"]');

COMMIT;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Test 1: Race Descriptors
-- SELECT race_id, COUNT(*) FROM Entity_Race_Descriptors GROUP BY race_id;
-- Expected: Dvergr ~15, Dokkalfar ~12

-- Test 2: Faction Descriptors
-- SELECT faction_id, COUNT(*) FROM Entity_Faction_Descriptors GROUP BY faction_id;
-- Expected: Hearth_Clans ~6, Rust_Clans ~6, Forsaken ~6

-- Test 3: Specialization Descriptors
-- SELECT specialization_id, COUNT(*) FROM Entity_Specialization_Descriptors GROUP BY specialization_id;
-- Expected: Berserkr ~5, Bone_Setter ~5

-- Test 4: Magic Descriptors
-- SELECT magic_type, COUNT(*) FROM Magic_Descriptors GROUP BY magic_type;
-- Expected: Galdr ~5, Runic ~4, Blight ~3

-- =====================================================
-- END v0.38.17 ENTITY DESCRIPTOR SCHEMA
-- =====================================================
