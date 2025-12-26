-- ═══════════════════════════════════════════════════════════════
-- v0.34.1: Database Schema & Companion Definitions
-- Rune & Rust: NPC Companion System
-- ═══════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════
-- TABLE CREATION
-- ═══════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Companions (
    companion_id INTEGER PRIMARY KEY,
    companion_name TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,

    archetype TEXT NOT NULL CHECK(archetype IN ('Warrior', 'Adept', 'Mystic')),
    faction_affiliation TEXT,
    background_summary TEXT NOT NULL,
    personality_traits TEXT NOT NULL,

    recruitment_location TEXT NOT NULL,
    required_faction TEXT,
    required_reputation_tier TEXT,
    required_reputation_value INTEGER,
    recruitment_quest_id INTEGER,

    base_might INTEGER NOT NULL DEFAULT 10,
    base_finesse INTEGER NOT NULL DEFAULT 10,
    base_sturdiness INTEGER NOT NULL DEFAULT 10,
    base_wits INTEGER NOT NULL DEFAULT 10,
    base_will INTEGER NOT NULL DEFAULT 10,

    base_max_hp INTEGER NOT NULL DEFAULT 30,
    base_defense INTEGER NOT NULL DEFAULT 10,
    base_soak INTEGER NOT NULL DEFAULT 0,

    resource_type TEXT NOT NULL CHECK(resource_type IN ('Stamina', 'Aether Pool')),
    base_max_resource INTEGER NOT NULL DEFAULT 100,

    combat_role TEXT NOT NULL,
    default_stance TEXT NOT NULL DEFAULT 'aggressive' CHECK(default_stance IN ('aggressive', 'defensive', 'passive')),

    starting_abilities TEXT NOT NULL,

    personal_quest_id INTEGER,
    personal_quest_title TEXT,

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (recruitment_quest_id) REFERENCES Quests(quest_id),
    FOREIGN KEY (personal_quest_id) REFERENCES Quests(quest_id)
);

CREATE INDEX IF NOT EXISTS idx_companions_faction ON Companions(required_faction);
CREATE INDEX IF NOT EXISTS idx_companions_location ON Companions(recruitment_location);

CREATE TABLE IF NOT EXISTS Characters_Companions (
    character_companion_id INTEGER PRIMARY KEY,
    character_id INTEGER NOT NULL,
    companion_id INTEGER NOT NULL,

    is_recruited BOOLEAN NOT NULL DEFAULT 0,
    recruited_at TIMESTAMP,
    is_in_party BOOLEAN NOT NULL DEFAULT 0,

    current_hp INTEGER NOT NULL,
    current_resource INTEGER NOT NULL,
    is_incapacitated BOOLEAN NOT NULL DEFAULT 0,

    current_stance TEXT NOT NULL DEFAULT 'aggressive' CHECK(current_stance IN ('aggressive', 'defensive', 'passive')),

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (character_id) REFERENCES Characters(character_id),
    FOREIGN KEY (companion_id) REFERENCES Companions(companion_id),
    UNIQUE(character_id, companion_id)
);

CREATE INDEX IF NOT EXISTS idx_char_companions_character ON Characters_Companions(character_id);
CREATE INDEX IF NOT EXISTS idx_char_companions_party ON Characters_Companions(character_id, is_in_party);

CREATE TABLE IF NOT EXISTS Companion_Progression (
    progression_id INTEGER PRIMARY KEY,
    character_id INTEGER NOT NULL,
    companion_id INTEGER NOT NULL,

    current_level INTEGER NOT NULL DEFAULT 1,
    current_legend INTEGER NOT NULL DEFAULT 0,
    legend_to_next_level INTEGER NOT NULL DEFAULT 100,

    override_might INTEGER,
    override_finesse INTEGER,
    override_sturdiness INTEGER,
    override_wits INTEGER,
    override_will INTEGER,

    override_max_hp INTEGER,
    override_defense INTEGER,
    override_soak INTEGER,

    equipped_weapon_id INTEGER,
    equipped_armor_id INTEGER,
    equipped_accessory_id INTEGER,

    unlocked_abilities TEXT NOT NULL DEFAULT '[]',

    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (character_id) REFERENCES Characters(character_id),
    FOREIGN KEY (companion_id) REFERENCES Companions(companion_id),
    FOREIGN KEY (equipped_weapon_id) REFERENCES Equipment(equipment_id),
    FOREIGN KEY (equipped_armor_id) REFERENCES Equipment(equipment_id),
    FOREIGN KEY (equipped_accessory_id) REFERENCES Equipment(equipment_id),
    UNIQUE(character_id, companion_id)
);

CREATE INDEX IF NOT EXISTS idx_companion_prog_character ON Companion_Progression(character_id);

CREATE TABLE IF NOT EXISTS Companion_Quests (
    companion_quest_id INTEGER PRIMARY KEY,
    character_id INTEGER NOT NULL,
    companion_id INTEGER NOT NULL,
    quest_id INTEGER NOT NULL,

    is_unlocked BOOLEAN NOT NULL DEFAULT 0,
    is_started BOOLEAN NOT NULL DEFAULT 0,
    is_completed BOOLEAN NOT NULL DEFAULT 0,

    loyalty_reward INTEGER DEFAULT 0,

    unlocked_at TIMESTAMP,
    started_at TIMESTAMP,
    completed_at TIMESTAMP,

    FOREIGN KEY (character_id) REFERENCES Characters(character_id),
    FOREIGN KEY (companion_id) REFERENCES Companions(companion_id),
    FOREIGN KEY (quest_id) REFERENCES Quests(quest_id),
    UNIQUE(character_id, companion_id, quest_id)
);

CREATE INDEX IF NOT EXISTS idx_companion_quests_char ON Companion_Quests(character_id);
CREATE INDEX IF NOT EXISTS idx_companion_quests_companion ON Companion_Quests(companion_id);

-- ═══════════════════════════════════════════════════════════════
-- COMPANION DEFINITIONS
-- ═══════════════════════════════════════════════════════════════

-- Companion 1: Kára Ironbreaker (Iron-Bane Tank)
INSERT INTO Companions VALUES (
    34001, 'Kara_Ironbreaker', 'Kára Ironbreaker',
    'Warrior', 'Iron-Bane',
    'Former security protocol enforcer turned Undying hunter. Lost her entire squad to a Draugr Juggernaut ambush. Methodical, disciplined, carries survivor''s guilt. Sees every Undying destroyed as atonement.',
    'Stoic, duty-driven, distrusts magic',
    'Trunk (Iron-Bane Enclave)', 'Iron-Bane', 'Friendly', 25, NULL,
    14, 10, 15, 9, 8,
    45, 12, 3,
    'Stamina', 120,
    'Tank', 'defensive',
    '[34101, 34102, 34103]',
    NULL, 'The Last Protocol',
    CURRENT_TIMESTAMP
);

-- Companion 2: Finnr the Rust-Sage (Jötun-Reader Support)
INSERT INTO Companions VALUES (
    34002, 'Finnr_Rust_Sage', 'Finnr the Rust-Sage',
    'Mystic', 'Jotun-Reader',
    'Scholar obsessed with Pre-Glitch knowledge. Believes understanding the Glitch is the only path forward. Socially awkward, brilliant, dangerously curious about forbidden data.',
    'Curious, verbose, oblivious to danger',
    'Alfheim Archives', 'Jotun-Reader', 'Friendly', 25, NULL,
    8, 10, 9, 15, 14,
    28, 10, 1,
    'Aether Pool', 150,
    'Support', 'defensive',
    '[34201, 34202, 34203]',
    NULL, 'The Forlorn Archive',
    CURRENT_TIMESTAMP
);

-- Companion 3: Bjorn Scrap-Hand (Rust-Clan Utility)
INSERT INTO Companions VALUES (
    34003, 'Bjorn_Scrap_Hand', 'Bjorn Scrap-Hand',
    'Adept', 'Rust-Clan',
    'Pragmatic scavenger who survived by being useful. Can fix anything, build weapons from scrap, knows where to find resources. No ideology—just survival.',
    'Practical, cynical, surprisingly loyal',
    'Midgard Trade Outpost', 'Rust-Clan', 'Neutral', 0, NULL,
    11, 12, 12, 14, 9,
    35, 11, 2,
    'Stamina', 110,
    'Utility', 'aggressive',
    '[34301, 34302, 34303]',
    NULL, 'The Old Workshop',
    CURRENT_TIMESTAMP
);

-- Companion 4: Valdis the Forlorn-Touched (Independent Glass Cannon)
INSERT INTO Companions VALUES (
    34004, 'Valdis_Forlorn_Touched', 'Valdis the Forlorn-Touched',
    'Mystic', 'Independent',
    'Seidkona who communes with Forlorn too deeply. Hears voices, sees ghosts, balances on edge of Breaking. Powerful but unstable—high-risk, high-reward companion.',
    'Haunted, prophetic, unpredictable',
    'Niflheim Frozen Ruins', NULL, NULL, NULL, NULL,
    7, 9, 7, 12, 16,
    24, 9, 0,
    'Aether Pool', 180,
    'DPS', 'aggressive',
    '[34401, 34402, 34403]',
    NULL, 'Breaking the Voices',
    CURRENT_TIMESTAMP
);

-- Companion 5: Runa Shield-Sister (Independent Tank)
INSERT INTO Companions VALUES (
    34005, 'Runa_Shield_Sister', 'Runa Shield-Sister',
    'Warrior', 'Independent',
    'Mercenary guard who protects those weaker than herself. No faction loyalties—only a personal code. Straightforward, protective, suspicious of authority.',
    'Honorable, protective, independent',
    'Jotunheim Assembly Yards', NULL, NULL, NULL, NULL,
    13, 11, 16, 10, 9,
    50, 13, 4,
    'Stamina', 130,
    'Tank', 'defensive',
    '[34501, 34502, 34503]',
    NULL, 'The Broken Oath',
    CURRENT_TIMESTAMP
);

-- Companion 6: Einar the God-Touched (God-Sleeper DPS)
INSERT INTO Companions VALUES (
    34006, 'Einar_God_Touched', 'Einar the God-Touched',
    'Warrior', 'God-Sleeper',
    'Cargo cultist who believes Jötun-Forged are sleeping gods. Zealous, powerful near Jötun corpses, sees your journey as divine will. Dangerous fanaticism mixed with genuine combat skill.',
    'Zealous, charismatic, sees omens everywhere',
    'Jotunheim Temple (Einherjar Torso-Cave)', 'God-Sleeper', 'Friendly', 25, NULL,
    16, 10, 13, 8, 11,
    42, 11, 2,
    'Stamina', 140,
    'DPS', 'aggressive',
    '[34601, 34602, 34603]',
    NULL, 'Awaken the Sleeper',
    CURRENT_TIMESTAMP
);

-- ═══════════════════════════════════════════════════════════════
-- COMPANION ABILITIES SEEDING
-- ═══════════════════════════════════════════════════════════════

-- Kára Ironbreaker Abilities
INSERT INTO Abilities VALUES (
    34101, 'Shield Bash', 'Kara_Ironbreaker',
    'Bash target with shield, dealing 1d8 + MIGHT damage and stunning for 1 turn.',
    'Stamina', 5, 1, 1, 'single_target',
    'melee', 'Physical', NULL, 'Stun: 1 turn',
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34102, 'Taunt', 'Kara_Ironbreaker',
    'Force target enemy to attack Kára for 2 turns.',
    'Stamina', 10, 1, 2, 'single_target',
    'ranged', NULL, NULL, 'Taunt: 2 turns',
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34103, 'Purification Strike', 'Kara_Ironbreaker',
    'Powerful strike dealing 2d6 + MIGHT damage. +2d6 bonus damage vs. Undying.',
    'Stamina', 15, 2, 1, 'single_target',
    'melee', 'Physical', '+2d6 vs. Undying', NULL,
    'companion_ability', CURRENT_TIMESTAMP
);

-- Finnr Rust-Sage Abilities
INSERT INTO Abilities VALUES (
    34201, 'Aetheric Bolt', 'Finnr_Rust_Sage',
    'Fire bolt of Aetheric energy dealing 2d6 + WILL damage at range.',
    'Aether Pool', 20, 2, 6, 'single_target',
    'ranged', 'Magic', NULL, NULL,
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34202, 'Data Analysis', 'Finnr_Rust_Sage',
    'Analyze target enemy, revealing weaknesses. Allies gain +2 to hit and +1d6 damage vs. that enemy for 3 turns.',
    'Aether Pool', 30, 3, 6, 'single_target',
    'ranged', NULL, NULL, 'Weakness Revealed: 3 turns',
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34203, 'Runic Shield', 'Finnr_Rust_Sage',
    'Grant ally +3 Defense for 3 turns.',
    'Aether Pool', 25, 3, 6, 'single_target',
    'ranged', NULL, NULL, 'Defense +3: 3 turns',
    'companion_ability', CURRENT_TIMESTAMP
);

-- Bjorn Scrap-Hand Abilities
INSERT INTO Abilities VALUES (
    34301, 'Improvised Repair', 'Bjorn_Scrap_Hand',
    'Use scrap materials to heal ally for 2d8 HP.',
    'Stamina', 20, 2, 3, 'single_target',
    'ranged', 'Healing', NULL, NULL,
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34302, 'Scrap Grenade', 'Bjorn_Scrap_Hand',
    'Throw improvised explosive dealing 2d6 damage to all enemies in 2x2 area.',
    'Stamina', 25, 3, 5, 'area_2x2',
    'ranged', 'Physical', 'AOE', NULL,
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34303, 'Resourceful', 'Bjorn_Scrap_Hand',
    'Passive: When Bjorn is in party, increase loot quality by 10%.',
    NULL, 0, 0, 0, 'self',
    'passive', NULL, 'Loot +10%', NULL,
    'companion_ability', CURRENT_TIMESTAMP
);

-- Valdis Forlorn-Touched Abilities
INSERT INTO Abilities VALUES (
    34401, 'Spirit Bolt', 'Valdis_Forlorn_Touched',
    'Unleash psychic blast dealing 3d6 + WILL damage.',
    'Aether Pool', 30, 3, 6, 'single_target',
    'ranged', 'Psychic', NULL, NULL,
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34402, 'Forlorn Whisper', 'Valdis_Forlorn_Touched',
    'Channel terrifying Forlorn voices. Target is feared and skips their next turn.',
    'Aether Pool', 40, 4, 5, 'single_target',
    'ranged', 'Psychic', NULL, 'Fear: Skip 1 turn',
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34403, 'Fragile Mind', 'Valdis_Forlorn_Touched',
    'Passive: +25% spell damage, -25% max HP.',
    NULL, 0, 0, 0, 'self',
    'passive', NULL, 'Glass Cannon', NULL,
    'companion_ability', CURRENT_TIMESTAMP
);

-- Runa Shield-Sister Abilities
INSERT INTO Abilities VALUES (
    34501, 'Defensive Stance', 'Runa_Shield_Sister',
    'Adopt defensive posture. Gain +5 Defense but deal -2 damage for 3 turns.',
    'Stamina', 15, 3, 0, 'self',
    'self', NULL, NULL, 'Defense +5, Damage -2: 3 turns',
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34502, 'Interpose', 'Runa_Shield_Sister',
    'Step in front of ally. All damage to that ally this turn redirected to Runa.',
    'Stamina', 10, 1, 3, 'single_target',
    'melee', NULL, NULL, 'Redirect damage: 1 turn',
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34503, 'Shield Wall', 'Runa_Shield_Sister',
    'Raise shield to protect entire party. All allies gain +2 Defense for 2 turns.',
    'Stamina', 25, 2, 0, 'all_allies',
    'self', NULL, NULL, 'Party Defense +2: 2 turns',
    'companion_ability', CURRENT_TIMESTAMP
);

-- Einar God-Touched Abilities
INSERT INTO Abilities VALUES (
    34601, 'Berserker Rage', 'Einar_God_Touched',
    'Enter berserk state. Gain +4 MIGHT for 3 turns but take +2 damage from all sources.',
    'Stamina', 20, 3, 0, 'self',
    'self', NULL, NULL, 'MIGHT +4, Vulnerability +2: 3 turns',
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34602, 'Jotun Attunement', 'Einar_God_Touched',
    'Passive: When within 10 tiles of Jötun corpse, gain +4 to all attributes.',
    NULL, 0, 0, 0, 'self',
    'passive', NULL, 'Conditional +4 all stats', NULL,
    'companion_ability', CURRENT_TIMESTAMP
);

INSERT INTO Abilities VALUES (
    34603, 'Reckless Strike', 'Einar_God_Touched',
    'Powerful overhead strike dealing 3d6 + MIGHT damage. Lowers own Defense by -3 for 1 turn.',
    'Stamina', 15, 2, 1, 'single_target',
    'melee', 'Physical', NULL, 'Self Defense -3: 1 turn',
    'companion_ability', CURRENT_TIMESTAMP
);
