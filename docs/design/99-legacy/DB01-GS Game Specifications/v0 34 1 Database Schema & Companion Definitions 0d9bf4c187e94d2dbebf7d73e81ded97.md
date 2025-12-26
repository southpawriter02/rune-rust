# v0.34.1: Database Schema & Companion Definitions

Type: Technical
Description: 4 new tables (Companions, Characters_Companions, Companion_Progression, Companion_Quests), 6 recruitable NPC definitions with stats/abilities/faction requirements, SQL seeding scripts. 8-12 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.33.1 (Faction database), v0.3 (Equipment System), v0.2 (Progression System)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.34: NPC Companion System (v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.34.1-COMPANION-DATABASE

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-12 hours

**Prerequisites:** v0.33.1 (Faction database), v0.3 (Equipment System), v0.2 (Progression System)

**Parent Specification:** v0.34 NPC Companion System[[1]](v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)

---

## I. Executive Summary

### The Deliverable

This specification defines the **complete database architecture** for the NPC Companion System, including:

- **Companions table** — 6-8 recruitable NPC definitions with stats, abilities, and faction requirements
- **Characters_Companions table** — Per-character party membership and companion state
- **Companion_Progression table** — Level, equipment slots, ability unlocks
- **Companion_Quests table** — Personal questlines for each companion
- **Full SQL seeding scripts** — All 6-8 companions with complete data

This phase establishes the data foundation that v0.34.2 (AI), v0.34.3 (Recruitment), and v0.34.4 (Services) will build upon.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.34.1)

**Database Schema:**

- 4 new tables (Companions, Characters_Companions, Companion_Progression, Companion_Quests)
- Complete column definitions with types and constraints
- Foreign key relationships
- Indexes for performance

**6-8 Companion Definitions:**

- Full stat blocks (MIGHT, FINESSE, STURDINESS, WITS, WILL)
- Starting HP, Defense, Soak values
- Base archetype (Warrior/Adept/Mystic)
- Faction requirements
- Starting ability loadouts
- Personal quest hooks

**SQL Seeding Scripts:**

- INSERT statements for all companions
- Starting abilities seeded into Abilities table
- Personal quests seeded into Quests table
- Faction requirement data

**Documentation:**

- Column purpose explanations
- Data model relationships
- Query examples

### ❌ Explicitly Out of Scope

- Service implementation (defer to v0.34.4)
- AI behavior logic (defer to v0.34.2)
- Recruitment UI (defer to v0.34.3)
- Companion leveling formulas (defer to v0.34.3)
- Migration scripts (new tables, no migration needed)
- Companion dialogue content (defer to narrative pass)
- Advanced Corruption/Loyalty tracking (simplified for v0.34)

---

## III. Database Schema

### Table 1: Companions

**Purpose:** Master definition table for all recruitable companion NPCs.

```sql
CREATE TABLE Companions (
    companion_id INTEGER PRIMARY KEY,
    companion_name TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL, -- "Kára Ironbreaker" (with diacritics)
    
    -- Identity
    archetype TEXT NOT NULL CHECK(archetype IN ('Warrior', 'Adept', 'Mystic')),
    faction_affiliation TEXT, -- 'Iron-Bane', 'Jotun-Reader', 'Rust-Clan', 'God-Sleeper', 'Independent'
    background_summary TEXT NOT NULL,
    personality_traits TEXT NOT NULL,
    
    -- Recruitment Requirements
    recruitment_location TEXT NOT NULL, -- Biome/area where found
    required_faction TEXT, -- Faction name (NULL if no requirement)
    required_reputation_tier TEXT, -- 'Neutral', 'Friendly', 'Honored', NULL
    required_reputation_value INTEGER, -- Numeric threshold (0, 25, 50, NULL)
    recruitment_quest_id INTEGER, -- Quest that unlocks recruitment
    
    -- Base Stats (Level 1)
    base_might INTEGER NOT NULL DEFAULT 10,
    base_finesse INTEGER NOT NULL DEFAULT 10,
    base_sturdiness INTEGER NOT NULL DEFAULT 10,
    base_wits INTEGER NOT NULL DEFAULT 10,
    base_will INTEGER NOT NULL DEFAULT 10,
    
    base_max_hp INTEGER NOT NULL DEFAULT 30,
    base_defense INTEGER NOT NULL DEFAULT 10,
    base_soak INTEGER NOT NULL DEFAULT 0,
    
    -- Resource System
    resource_type TEXT NOT NULL CHECK(resource_type IN ('Stamina', 'Aether Pool')),
    base_max_resource INTEGER NOT NULL DEFAULT 100,
    
    -- Combat Role
    combat_role TEXT NOT NULL, -- 'Tank', 'DPS', 'Support', 'Utility'
    default_stance TEXT NOT NULL DEFAULT 'aggressive' CHECK(default_stance IN ('aggressive', 'defensive', 'passive')),
    
    -- Starting Abilities (JSON array of ability_ids)
    starting_abilities TEXT NOT NULL, -- '[1001, 1002, 1003]'
    
    -- Personal Quest
    personal_quest_id INTEGER,
    personal_quest_title TEXT,
    
    -- Metadata
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (recruitment_quest_id) REFERENCES Quests(quest_id),
    FOREIGN KEY (personal_quest_id) REFERENCES Quests(quest_id)
);

CREATE INDEX idx_companions_faction ON Companions(required_faction);
CREATE INDEX idx_companions_location ON Companions(recruitment_location);
```

**Key Design Decisions:**

- **companion_name (ASCII)** vs. **display_name (Unicode):** Separates code identifiers from player-facing text
- **Base stats at Level 1:** Scaling formulas applied in CompanionProgressionService
- **JSON starting_abilities:** Flexible array storage, parsed by service layer
- **Faction requirements:** Some companions require reputation, others don't

---

### Table 2: Characters_Companions

**Purpose:** Tracks which companions are recruited by which player characters, and their current state.

```sql
CREATE TABLE Characters_Companions (
    character_companion_id INTEGER PRIMARY KEY,
    character_id INTEGER NOT NULL,
    companion_id INTEGER NOT NULL,
    
    -- Recruitment State
    is_recruited BOOLEAN NOT NULL DEFAULT 0,
    recruited_at TIMESTAMP,
    is_in_party BOOLEAN NOT NULL DEFAULT 0,
    
    -- Current State
    current_hp INTEGER NOT NULL,
    current_resource INTEGER NOT NULL,
    is_incapacitated BOOLEAN NOT NULL DEFAULT 0, -- System Crash state
    
    -- Current Stance
    current_stance TEXT NOT NULL DEFAULT 'aggressive' CHECK(current_stance IN ('aggressive', 'defensive', 'passive')),
    
    -- Metadata
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (character_id) REFERENCES Characters(character_id),
    FOREIGN KEY (companion_id) REFERENCES Companions(companion_id),
    UNIQUE(character_id, companion_id)
);

CREATE INDEX idx_char_companions_character ON Characters_Companions(character_id);
CREATE INDEX idx_char_companions_party ON Characters_Companions(character_id, is_in_party);
```

**Key Design Decisions:**

- **is_recruited vs. is_in_party:** Companions can be recruited but not actively in party (party size limit)
- **is_incapacitated:** Tracks System Crash state (can't fight until recovered)
- **UNIQUE constraint:** Each character can recruit each companion only once

---

### Table 3: Companion_Progression

**Purpose:** Tracks companion leveling, equipment, and ability unlocks per character.

```sql
CREATE TABLE Companion_Progression (
    progression_id INTEGER PRIMARY KEY,
    character_id INTEGER NOT NULL,
    companion_id INTEGER NOT NULL,
    
    -- Leveling
    current_level INTEGER NOT NULL DEFAULT 1,
    current_legend INTEGER NOT NULL DEFAULT 0, -- XP
    legend_to_next_level INTEGER NOT NULL DEFAULT 100,
    
    -- Stat Overrides (if different from base + level scaling)
    override_might INTEGER,
    override_finesse INTEGER,
    override_sturdiness INTEGER,
    override_wits INTEGER,
    override_will INTEGER,
    
    override_max_hp INTEGER,
    override_defense INTEGER,
    override_soak INTEGER,
    
    -- Equipment Slots (item_ids, NULL if empty)
    equipped_weapon_id INTEGER,
    equipped_armor_id INTEGER,
    equipped_accessory_id INTEGER,
    
    -- Unlocked Abilities (JSON array)
    unlocked_abilities TEXT NOT NULL DEFAULT '[]', -- Grows as companion levels
    
    -- Metadata
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (character_id) REFERENCES Characters(character_id),
    FOREIGN KEY (companion_id) REFERENCES Companions(companion_id),
    FOREIGN KEY (equipped_weapon_id) REFERENCES Equipment(equipment_id),
    FOREIGN KEY (equipped_armor_id) REFERENCES Equipment(equipment_id),
    FOREIGN KEY (equipped_accessory_id) REFERENCES Equipment(equipment_id),
    UNIQUE(character_id, companion_id)
);

CREATE INDEX idx_companion_prog_character ON Companion_Progression(character_id);
```

**Key Design Decisions:**

- **override_* columns:** Allow manual stat adjustments (default NULL, use base + scaling)
- **Equipment slots:** Reuse existing Equipment table
- **unlocked_abilities JSON:** Expands beyond starting_abilities as companion levels

---

### Table 4: Companion_Quests

**Purpose:** Links companions to their personal questlines and tracks completion state.

```sql
CREATE TABLE Companion_Quests (
    companion_quest_id INTEGER PRIMARY KEY,
    character_id INTEGER NOT NULL,
    companion_id INTEGER NOT NULL,
    quest_id INTEGER NOT NULL,
    
    -- State
    is_unlocked BOOLEAN NOT NULL DEFAULT 0, -- Becomes available when companion recruited
    is_started BOOLEAN NOT NULL DEFAULT 0,
    is_completed BOOLEAN NOT NULL DEFAULT 0,
    
    -- Relationship Impact (future expansion)
    loyalty_reward INTEGER DEFAULT 0, -- Future: increase companion loyalty
    
    -- Metadata
    unlocked_at TIMESTAMP,
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    
    FOREIGN KEY (character_id) REFERENCES Characters(character_id),
    FOREIGN KEY (companion_id) REFERENCES Companions(companion_id),
    FOREIGN KEY (quest_id) REFERENCES Quests(quest_id),
    UNIQUE(character_id, companion_id, quest_id)
);

CREATE INDEX idx_companion_quests_char ON Companion_Quests(character_id);
CREATE INDEX idx_companion_quests_companion ON Companion_Quests(companion_id);
```

**Key Design Decisions:**

- **is_unlocked:** Personal quests unlock when companion recruited
- **Reuses Quests table:** Personal quests are standard quests with companion_id reference
- **loyalty_reward:** Placeholder for future Loyalty system (v2.0+)

---

## IV. Companion Definitions

### Companion 1: Kára Ironbreaker

**ASCII Name:** `Kara_Ironbreaker`

**Display Name:** Kára Ironbreaker

**Stats:**

```sql
INSERT INTO Companions (
    companion_id, companion_name, display_name,
    archetype, faction_affiliation, background_summary, personality_traits,
    recruitment_location, required_faction, required_reputation_tier, required_reputation_value, recruitment_quest_id,
    base_might, base_finesse, base_sturdiness, base_wits, base_will,
    base_max_hp, base_defense, base_soak,
    resource_type, base_max_resource,
    combat_role, default_stance,
    starting_abilities,
    personal_quest_title
) VALUES (
    34001, 'Kara_Ironbreaker', 'Kára Ironbreaker',
    'Warrior', 'Iron-Bane',
    'Former security protocol enforcer turned Undying hunter. Lost her entire squad to a Draugr Juggernaut ambush. Methodical, disciplined, carries survivor''s guilt. Sees every Undying destroyed as atonement.',
    'Stoic, duty-driven, distrusts magic',
    'Trunk (Iron-Bane Enclave)', 'Iron-Bane', 'Friendly', 25, NULL,
    14, 10, 15, 9, 8, -- High MIGHT, STURDINESS
    45, 12, 3, -- Tank stats
    'Stamina', 120,
    'Tank', 'defensive',
    '[34101, 34102, 34103]', -- Shield Bash, Taunt, Purification Strike
    'The Last Protocol'
);
```

**Starting Abilities:**

- **34101: Shield Bash** — Bash with shield, stun 1 turn (5 Stamina)
- **34102: Taunt** — Force enemy to attack Kara (10 Stamina)
- **34103: Purification Strike** — +2d6 vs. Undying (15 Stamina)

**Personal Quest:** "The Last Protocol" (Quest ID: TBD in Quests table)

---

### Companion 2: Finnr the Rust-Sage

**ASCII Name:** `Finnr_Rust_Sage`

**Display Name:** Finnr the Rust-Sage

**Stats:**

```sql
INSERT INTO Companions (
    companion_id, companion_name, display_name,
    archetype, faction_affiliation, background_summary, personality_traits,
    recruitment_location, required_faction, required_reputation_tier, required_reputation_value, recruitment_quest_id,
    base_might, base_finesse, base_sturdiness, base_wits, base_will,
    base_max_hp, base_defense, base_soak,
    resource_type, base_max_resource,
    combat_role, default_stance,
    starting_abilities,
    personal_quest_title
) VALUES (
    34002, 'Finnr_Rust_Sage', 'Finnr the Rust-Sage',
    'Mystic', 'Jotun-Reader',
    'Scholar obsessed with Pre-Glitch knowledge. Believes understanding the Glitch is the only path forward. Socially awkward, brilliant, dangerously curious about forbidden data.',
    'Curious, verbose, oblivious to danger',
    'Alfheim Archives', 'Jotun-Reader', 'Friendly', 25, NULL,
    8, 10, 9, 15, 14, -- High WITS, WILL
    28, 10, 1, -- Low HP support
    'Aether Pool', 150,
    'Support', 'defensive',
    '[34201, 34202, 34203]', -- Aetheric Bolt, Data Analysis, Runic Shield
    'The Forlorn Archive'
);
```

**Starting Abilities:**

- **34201: Aetheric Bolt** — Ranged magic damage (20 Aether)
- **34202: Data Analysis** — Reveal enemy weaknesses (30 Aether, Out-of-combat)
- **34203: Runic Shield** — Grant ally +3 Defense for 3 turns (25 Aether)

**Personal Quest:** "The Forlorn Archive"

---

### Companion 3: Bjorn Scrap-Hand

**ASCII Name:** `Bjorn_Scrap_Hand`

**Display Name:** Bjorn Scrap-Hand

**Stats:**

```sql
INSERT INTO Companions (
    companion_id, companion_name, display_name,
    archetype, faction_affiliation, background_summary, personality_traits,
    recruitment_location, required_faction, required_reputation_tier, required_reputation_value, recruitment_quest_id,
    base_might, base_finesse, base_sturdiness, base_wits, base_will,
    base_max_hp, base_defense, base_soak,
    resource_type, base_max_resource,
    combat_role, default_stance,
    starting_abilities,
    personal_quest_title
) VALUES (
    34003, 'Bjorn_Scrap_Hand', 'Bjorn Scrap-Hand',
    'Adept', 'Rust-Clan',
    'Pragmatic scavenger who survived by being useful. Can fix anything, build weapons from scrap, knows where to find resources. No ideology—just survival.',
    'Practical, cynical, surprisingly loyal',
    'Midgard Trade Outpost', 'Rust-Clan', 'Neutral', 0, NULL,
    11, 12, 12, 14, 9, -- Balanced Adept
    35, 11, 2,
    'Stamina', 110,
    'Utility', 'aggressive',
    '[34301, 34302, 34303]', -- Improvised Repair, Scrap Grenade, Resourceful
    'The Old Workshop'
);
```

**Starting Abilities:**

- **34301: Improvised Repair** — Heal ally 2d8 HP (20 Stamina)
- **34302: Scrap Grenade** — AOE damage 2x2 area (25 Stamina)
- **34303: Resourceful (Passive)** — +10% loot quality when in party

**Personal Quest:** "The Old Workshop"

---

### Companion 4: Valdis the Forlorn-Touched

**ASCII Name:** `Valdis_Forlorn_Touched`

**Display Name:** Valdis the Forlorn-Touched

**Stats:**

```sql
INSERT INTO Companions (
    companion_id, companion_name, display_name,
    archetype, faction_affiliation, background_summary, personality_traits,
    recruitment_location, required_faction, required_reputation_tier, required_reputation_value, recruitment_quest_id,
    base_might, base_finesse, base_sturdiness, base_wits, base_will,
    base_max_hp, base_defense, base_soak,
    resource_type, base_max_resource,
    combat_role, default_stance,
    starting_abilities,
    personal_quest_title
) VALUES (
    34004, 'Valdis_Forlorn_Touched', 'Valdis the Forlorn-Touched',
    'Mystic', 'Independent',
    'Seidkona who communes with Forlorn too deeply. Hears voices, sees ghosts, balances on edge of Breaking. Powerful but unstable—high-risk, high-reward companion.',
    'Haunted, prophetic, unpredictable',
    'Niflheim Frozen Ruins', NULL, NULL, NULL, NULL, -- Found during exploration
    7, 9, 7, 12, 16, -- Extremely high WILL, glass cannon
    24, 9, 0, -- Lowest HP
    'Aether Pool', 180, -- High Aether pool
    'DPS', 'aggressive',
    '[34401, 34402, 34403]', -- Spirit Bolt, Forlorn Whisper, Fragile Mind
    'Breaking the Voices'
);
```

**Starting Abilities:**

- **34401: Spirit Bolt** — Heavy psychic damage (30 Aether)
- **34402: Forlorn Whisper** — Fear effect, enemy skips turn (40 Aether)
- **34403: Fragile Mind (Passive)** — +25% spell damage, -25% max HP

**Personal Quest:** "Breaking the Voices"

---

### Companion 5: Runa Shield-Sister

**ASCII Name:** `Runa_Shield_Sister`

**Display Name:** Runa Shield-Sister

**Stats:**

```sql
INSERT INTO Companions (
    companion_id, companion_name, display_name,
    archetype, faction_affiliation, background_summary, personality_traits,
    recruitment_location, required_faction, required_reputation_tier, required_reputation_value, recruitment_quest_id,
    base_might, base_finesse, base_sturdiness, base_wits, base_will,
    base_max_hp, base_defense, base_soak,
    resource_type, base_max_resource,
    combat_role, default_stance,
    starting_abilities,
    personal_quest_title
) VALUES (
    34005, 'Runa_Shield_Sister', 'Runa Shield-Sister',
    'Warrior', 'Independent',
    'Mercenary guard who protects those weaker than herself. No faction loyalties—only a personal code. Straightforward, protective, suspicious of authority.',
    'Honorable, protective, independent',
    'Jotunheim Assembly Yards', NULL, NULL, NULL, NULL, -- Dynamic event rescue
    13, 11, 16, 10, 9, -- Highest STURDINESS
    50, 13, 4, -- Best tank stats
    'Stamina', 130,
    'Tank', 'defensive',
    '[34501, 34502, 34503]', -- Defensive Stance, Interpose, Shield Wall
    'The Broken Oath'
);
```

**Starting Abilities:**

- **34501: Defensive Stance** — +5 Defense, -2 damage for 3 turns (15 Stamina)
- **34502: Interpose** — Take damage for ally this turn (10 Stamina)
- **34503: Shield Wall** — Grant all allies +2 Defense (25 Stamina)

**Personal Quest:** "The Broken Oath"

---

### Companion 6: Einar the God-Touched

**ASCII Name:** `Einar_God_Touched`

**Display Name:** Einar the God-Touched

**Stats:**

```sql
INSERT INTO Companions (
    companion_id, companion_name, display_name,
    archetype, faction_affiliation, background_summary, personality_traits,
    recruitment_location, required_faction, required_reputation_tier, required_reputation_value, recruitment_quest_id,
    base_might, base_finesse, base_sturdiness, base_wits, base_will,
    base_max_hp, base_defense, base_soak,
    resource_type, base_max_resource,
    combat_role, default_stance,
    starting_abilities,
    personal_quest_title
) VALUES (
    34006, 'Einar_God_Touched', 'Einar the God-Touched',
    'Warrior', 'God-Sleeper',
    'Cargo cultist who believes Jötun-Forged are sleeping gods. Zealous, powerful near Jötun corpses, sees your journey as divine will. Dangerous fanaticism mixed with genuine combat skill.',
    'Zealous, charismatic, sees omens everywhere',
    'Jotunheim Temple (Einherjar Torso-Cave)', 'God-Sleeper', 'Friendly', 25, NULL,
    16, 10, 13, 8, 11, -- Highest MIGHT
    42, 11, 2,
    'Stamina', 140,
    'DPS', 'aggressive',
    '[34601, 34602, 34603]', -- Berserker Rage, Jotun Attunement, Reckless Strike
    'Awaken the Sleeper'
);
```

**Starting Abilities:**

- **34601: Berserker Rage** — +4 MIGHT for 3 turns, take +2 damage (20 Stamina)
- **34602: Jötun Attunement (Passive)** — +4 all stats when near Jötun corpse
- **34603: Reckless Strike** — High damage, lowers own Defense (15 Stamina)

**Personal Quest:** "Awaken the Sleeper"

---

## V. Complete SQL Seeding Script

**File:** `Data/v0.34.1_companion_schema.sql`

```sql
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
```

---

## VI. Query Examples

### Get All Recruitable Companions

```sql
SELECT 
    companion_id,
    display_name,
    archetype,
    combat_role,
    faction_affiliation,
    recruitment_location
FROM Companions
ORDER BY faction_affiliation, display_name;
```

---

### Check if Player Can Recruit Companion

```sql
-- For Kára Ironbreaker (Iron-Bane, Friendly +25)
SELECT 
    c.display_name,
    c.required_faction,
    c.required_reputation_value,
    fr.reputation_value as current_reputation,
    CASE 
        WHEN fr.reputation_value >= c.required_reputation_value THEN 'Can Recruit'
        ELSE 'Reputation Too Low'
    END as recruitment_status
FROM Companions c
LEFT JOIN Faction_Reputations fr 
    ON fr.faction_name = c.required_faction
    AND fr.character_id = ?
WHERE c.companion_id = 34001;
```

---

### Get Player's Active Party

```sql
SELECT 
    c.display_name,
    c.combat_role,
    cc.current_hp,
    c.base_max_hp,
    cc.current_stance,
    [cc.is](http://cc.is)_incapacitated
FROM Characters_Companions cc
JOIN Companions c ON cc.companion_id = c.companion_id
WHERE cc.character_id = ?
    AND [cc.is](http://cc.is)_in_party = 1
ORDER BY c.display_name;
```

---

### Get Companion with Progression Data

```sql
SELECT 
    c.display_name,
    c.archetype,
    cp.current_level,
    cp.current_legend,
    c.base_might + (cp.current_level - 1) * 2 as effective_might,
    c.base_max_hp + (cp.current_level - 1) * 5 as effective_hp,
    cp.unlocked_abilities
FROM Companion_Progression cp
JOIN Companions c ON cp.companion_id = c.companion_id
WHERE cp.character_id = ?
    AND cp.companion_id = ?;
```

---

## VII. Testing Checklist

**Database Integrity:**

- [ ]  All 4 tables created successfully
- [ ]  Foreign key constraints enforced
- [ ]  Indexes created
- [ ]  CHECK constraints prevent invalid data

**Companion Seeding:**

- [ ]  All 6 companions inserted
- [ ]  Display names include diacritics (Kára, Jötun-Reader)
- [ ]  ASCII companion_name for code use
- [ ]  Starting abilities JSON arrays valid
- [ ]  Faction requirements accurate

**Abilities Seeding:**

- [ ]  18 abilities total (3 per companion)
- [ ]  Ability IDs match starting_abilities JSON
- [ ]  Resource costs reasonable
- [ ]  Damage values balanced

**Query Validation:**

- [ ]  Can retrieve all companions
- [ ]  Can check recruitment requirements
- [ ]  Can get active party
- [ ]  Can join with progression data

---

## VIII. Deployment Instructions

### Step 1: Execute Schema Script

```bash
sqlite3 Data/RuneAndRust.db < Data/v0.34.1_companion_schema.sql
```

### Step 2: Verify Table Creation

```sql
.tables
-- Should show: Companions, Characters_Companions, Companion_Progression, Companion_Quests

.schema Companions
```

### Step 3: Verify Companion Data

```sql
SELECT COUNT(*) FROM Companions;
-- Expected: 6

SELECT COUNT(*) FROM Abilities WHERE ability_category = 'companion_ability';
-- Expected: 18
```

### Step 4: Test Queries

```sql
-- Test companion retrieval
SELECT display_name, faction_affiliation FROM Companions;

-- Test ability lookup
SELECT a.ability_name, a.description 
FROM Companions c
JOIN Abilities a ON a.ability_id IN (
    SELECT value FROM json_each(c.starting_abilities)
)
WHERE c.companion_id = 34001;
```

---

## IX. Next Steps

With database foundation complete:

- **v0.34.2** implements CompanionAIService (uses this data)
- **v0.34.3** implements RecruitmentService (queries Companions table)
- **v0.34.4** implements CompanionService orchestration (manages Characters_Companions)

---

**Implementation-ready database architecture for NPC Companion System complete.**