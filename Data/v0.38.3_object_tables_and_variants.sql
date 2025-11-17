-- =====================================================
-- v0.38.3: Interactive Object Repository - Tables & Function Variants
-- =====================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Define Interactive_Objects table, Object_Interactions log,
--          and function variants for all object templates
-- =====================================================

-- =====================================================
-- INTERACTIVE OBJECTS TABLE
-- =====================================================

CREATE TABLE IF NOT EXISTS Interactive_Objects (
    object_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,

    -- Descriptor reference
    composite_descriptor_id INTEGER,
    base_template_name TEXT NOT NULL,
    modifier_name TEXT,

    -- Object identity
    object_name TEXT NOT NULL,
    description TEXT NOT NULL,
    object_type TEXT NOT NULL,  -- 'Mechanism', 'Container', 'Investigatable', 'Barrier'

    -- Interaction mechanics
    interaction_type TEXT NOT NULL,  -- 'Pull', 'Search', 'Read', 'Hack', 'Open'
    requires_check INTEGER DEFAULT 0,
    check_type TEXT,  -- 'WITS', 'MIGHT', 'Lockpicking', 'Hacking'
    check_dc INTEGER,

    -- State management
    current_state TEXT,  -- 'Up'/'Down', 'Open'/'Closed', 'Locked'/'Unlocked'
    interacted INTEGER DEFAULT 0,
    reversible INTEGER DEFAULT 0,

    -- Loot reference (for containers)
    loot_table_id INTEGER,
    loot_tier TEXT,  -- 'Common', 'Uncommon', 'Rare'

    -- Consequence
    consequence_type TEXT,  -- 'Unlock', 'Trigger', 'Spawn', 'Reveal', 'Loot'
    consequence_data TEXT,  -- JSON: {"target": "nearby_door", "action": "open"}

    -- Trap properties (for pressure plates, trapped chests)
    is_trap INTEGER DEFAULT 0,
    trap_damage TEXT,
    trap_detected INTEGER DEFAULT 0,
    trap_disarmed INTEGER DEFAULT 0,

    -- Timestamps
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP

    -- FOREIGN KEY (room_id) REFERENCES Rooms(room_id),
    -- FOREIGN KEY (composite_descriptor_id) REFERENCES Descriptor_Composites(composite_id)
);

-- =====================================================
-- OBJECT INTERACTIONS LOG
-- =====================================================

CREATE TABLE IF NOT EXISTS Object_Interactions (
    interaction_id INTEGER PRIMARY KEY AUTOINCREMENT,
    object_id INTEGER NOT NULL,
    character_id INTEGER NOT NULL,
    interaction_timestamp TEXT DEFAULT CURRENT_TIMESTAMP,

    -- Interaction attempt
    interaction_type TEXT NOT NULL,

    -- Check result
    check_required INTEGER,
    check_type TEXT,
    check_dc INTEGER,
    check_roll TEXT,  -- JSON array of dice: [5, 6, 3, 7, 8]
    check_successes INTEGER,
    check_success INTEGER,

    -- Consequence
    consequence_executed TEXT,
    consequence_description TEXT,

    -- Result
    interaction_success INTEGER,
    failure_reason TEXT,

    FOREIGN KEY (object_id) REFERENCES Interactive_Objects(object_id)
    -- FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);

-- Create index for faster lookups
CREATE INDEX IF NOT EXISTS idx_objects_room ON Interactive_Objects(room_id);
CREATE INDEX IF NOT EXISTS idx_interactions_object ON Object_Interactions(object_id);
CREATE INDEX IF NOT EXISTS idx_interactions_character ON Object_Interactions(character_id);

-- =====================================================
-- FUNCTION VARIANTS TABLE
-- =====================================================

CREATE TABLE IF NOT EXISTS Object_Function_Variants (
    variant_id INTEGER PRIMARY KEY AUTOINCREMENT,
    base_template_name TEXT NOT NULL,
    function_name TEXT NOT NULL UNIQUE,
    function_description TEXT NOT NULL,

    -- Mechanic overrides (JSON)
    mechanic_overrides TEXT,  -- {"check_dc": 18, "loot_tier": "Rare"}

    -- Consequence definition (JSON)
    consequence_type TEXT,
    consequence_data TEXT,

    -- Biome affinity
    biome_affinity TEXT,  -- JSON array: ["Muspelheim", "Niflheim"]

    -- Tags
    tags TEXT,

    created_at TEXT DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- FUNCTION VARIANT DEFINITIONS
-- =====================================================

-- LEVER FUNCTION VARIANTS
INSERT OR IGNORE INTO Object_Function_Variants (
    base_template_name, function_name, function_description,
    consequence_type, consequence_data, tags
) VALUES
    ('Lever_Base', 'Door_Control', 'Controls a nearby door (open/close)',
     'Unlock', '{"target_type": "door", "action": "toggle", "range": "nearby"}',
     '["Puzzle", "Utility"]'),

    ('Lever_Base', 'Power_Diverter', 'Redirects power, deactivates hazards',
     'Trigger', '{"action": "disable_hazards", "room": "current"}',
     '["Utility", "Hazard"]'),

    ('Lever_Base', 'Bridge_Extender', 'Extends bridge across chasm',
     'Reveal', '{"action": "extend_bridge", "target": "chasm"}',
     '["Puzzle", "Navigation"]'),

    ('Lever_Base', 'Trap_Trigger', 'Activates trap (hostile)',
     'Spawn', '{"action": "activate_trap", "damage": "4d6", "type": "dart_trap"}',
     '["Trap", "Hostile"]'),

    ('Lever_Base', 'Secret_Revealer', 'Reveals hidden passage',
     'Reveal', '{"action": "reveal_passage", "type": "secret_door"}',
     '["Puzzle", "Secret"]');

-- CONSOLE FUNCTION VARIANTS
INSERT OR IGNORE INTO Object_Function_Variants (
    base_template_name, function_name, function_description,
    mechanic_overrides, consequence_type, consequence_data, tags
) VALUES
    ('Console_Base', 'Door_Override', 'Unlocks security doors',
     '{"check_dc": 15}',
     'Unlock', '{"target_type": "security_door", "action": "unlock", "room": "current"}',
     '["Security", "Utility"]'),

    ('Console_Base', 'Hazard_Control', 'Disables environmental hazards',
     '{"check_dc": 15}',
     'Trigger', '{"action": "disable_hazards", "room": "current"}',
     '["Utility", "Hazard"]'),

    ('Console_Base', 'Data_Recovery', 'Reveals quest information',
     '{"check_dc": 18}',
     'Reveal', '{"action": "reveal_data", "type": "quest_hook", "lore_value": "High"}',
     '["Quest", "Lore"]'),

    ('Console_Base', 'Enemy_Summon', 'Hostile: spawns Servitors',
     '{"check_dc": 12, "failure_consequence": "spawn_enemies"}',
     'Spawn', '{"action": "spawn_enemies", "enemy_type": "Servitor", "count": 3}',
     '["Hostile", "Combat"]'),

    ('Console_Base', 'Power_Routing', 'Restores power to area',
     '{"check_dc": 20}',
     'Trigger', '{"action": "restore_power", "room": "area"}',
     '["Utility", "Power"]');

-- PRESSURE PLATE FUNCTION VARIANTS
INSERT OR IGNORE INTO Object_Function_Variants (
    base_template_name, function_name, function_description,
    consequence_type, consequence_data, tags
) VALUES
    ('Pressure_Plate_Base', 'Dart_Trap', 'Fires poisoned darts (2d6 + Poison)',
     'Trap', '{"damage": "2d6", "damage_type": "Physical", "status_effect": ["Poisoned", 2]}',
     '["Trap", "Damage"]'),

    ('Pressure_Plate_Base', 'Alarm', 'Alerts enemies in adjacent rooms',
     'Spawn', '{"action": "alert_enemies", "range": "adjacent_rooms"}',
     '["Trap", "Alert"]'),

    ('Pressure_Plate_Base', 'Collapse', 'Triggers ceiling collapse',
     'Trap', '{"damage": "4d6", "damage_type": "Physical", "area": "room"}',
     '["Trap", "Damage", "Environmental"]'),

    ('Pressure_Plate_Base', 'Lock_Doors', 'Seals all exits',
     'Trigger', '{"action": "lock_doors", "room": "current"}',
     '["Trap", "Puzzle"]');

-- CHEST FUNCTION VARIANTS
INSERT OR IGNORE INTO Object_Function_Variants (
    base_template_name, function_name, function_description,
    mechanic_overrides, tags
) VALUES
    ('Chest_Base', 'Supply_Cache', 'Uncommon loot, low trap chance',
     '{"loot_tier": "Uncommon", "trap_chance": 0.1}',
     '["Loot", "Safe"]'),

    ('Chest_Base', 'Weapon_Locker', 'Rare loot, medium trap chance',
     '{"loot_tier": "Rare", "trap_chance": 0.3, "check_dc": 18}',
     '["Loot", "Valuable"]'),

    ('Chest_Base', 'Trapped_Decoy', 'Common loot, guaranteed trap',
     '{"loot_tier": "Common", "trap_chance": 1.0}',
     '["Loot", "Trap"]'),

    ('Chest_Base', 'Heirloom_Chest', 'Rare loot, complex lock',
     '{"loot_tier": "Rare", "check_dc": 22, "trap_chance": 0.0}',
     '["Loot", "Valuable", "Puzzle"]');

-- CORPSE ENTITY TYPE VARIANTS
INSERT OR IGNORE INTO Object_Function_Variants (
    base_template_name, function_name, function_description,
    mechanic_overrides, consequence_data, tags
) VALUES
    ('Corpse_Base', 'Scavenger', 'Killed by Servitors, common loot',
     '{"loot_tier": "Common"}',
     '{"narrative": "Puncture wounds suggest Servitor weapons.", "clue": "Scavenger map fragment"}',
     '["Narrative", "Common"]'),

    ('Corpse_Base', 'Jotun_Reader', 'Research notes, uncommon loot',
     '{"loot_tier": "Uncommon"}',
     '{"narrative": "Clutches research notes on Jötun runes.", "clue": "Runic translation fragment"}',
     '["Narrative", "Lore", "Uncommon"]'),

    ('Corpse_Base', 'Warrior', 'Combat gear, uncommon loot',
     '{"loot_tier": "Uncommon"}',
     '{"narrative": "Fell in combat, weapons nearby.", "clue": "Battle-worn journal"}',
     '["Narrative", "Combat", "Uncommon"]'),

    ('Corpse_Base', 'Dvergr_Tinkerer', 'Schematic fragment, rare loot',
     '{"loot_tier": "Rare"}',
     '{"narrative": "Dvergr tool belt and blueprints.", "clue": "Schematic: Advanced Repair Kit"}',
     '["Narrative", "Crafting", "Rare"]');

-- DATA SLATE CONTENT TYPE VARIANTS
INSERT OR IGNORE INTO Object_Function_Variants (
    base_template_name, function_name, function_description,
    consequence_data, tags
) VALUES
    ('Data_Slate_Base', 'Personal_Log', 'Daily log entry',
     '{"text_type": "log", "quest_hook": false, "lore_value": "Low"}',
     '["Lore", "Personal"]'),

    ('Data_Slate_Base', 'Warning_Message', 'Danger alert',
     '{"text_type": "warning", "quest_hook": false, "lore_value": "Medium"}',
     '["Lore", "Warning"]'),

    ('Data_Slate_Base', 'Research_Notes', 'Technical data',
     '{"text_type": "research", "quest_hook": true, "lore_value": "High"}',
     '["Lore", "Quest", "Technical"]'),

    ('Data_Slate_Base', 'Distress_Signal', 'Call for help',
     '{"text_type": "distress", "quest_hook": true, "lore_value": "Medium"}',
     '["Quest", "Narrative"]'),

    ('Data_Slate_Base', 'Corrupted_Fragment', 'Glitched text',
     '{"text_type": "corrupted", "quest_hook": false, "lore_value": "High", "corruption_level": "High"}',
     '["Lore", "Glitch", "Aether"]');

-- DOOR FUNCTION VARIANTS
INSERT OR IGNORE INTO Object_Function_Variants (
    base_template_name, function_name, function_description,
    mechanic_overrides, tags
) VALUES
    ('Door_Base', 'Standard', 'Standard unlocked door',
     '{"locked": false, "hp": 50}',
     '["Passage"]'),

    ('Door_Base', 'Security', 'Locked security door, requires keycard',
     '{"locked": true, "hp": 80, "requires": "Keycard"}',
     '["Passage", "Security"]'),

    ('Door_Base', 'Blast', 'Reinforced blast door, requires console override',
     '{"locked": true, "hp": 150, "soak": 20, "requires": "Console_Override"}',
     '["Passage", "Security", "Heavy"]'),

    ('Door_Base', 'Rusted_Stuck', 'Stuck from rust, requires MIGHT check',
     '{"locked": false, "requires_check": true, "check_type": "MIGHT", "check_dc": 15}',
     '["Passage", "Damaged"]');

-- =====================================================
-- COMPOSITE OBJECTS (Pre-generated Examples)
-- =====================================================

-- Example 1: Corroded Door Lever (Lever_Base + Rusted + Door_Control)
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id,
    modifier_id,
    final_name,
    final_description,
    final_mechanics,
    biome_restrictions,
    spawn_weight,
    is_active
) VALUES (
    (SELECT template_id FROM Descriptor_Base_Templates WHERE template_name = 'Lever_Base'),
    (SELECT modifier_id FROM Descriptor_Thematic_Modifiers WHERE modifier_name = 'Rusted'),
    'Corroded Door Lever',
    'A corroded lever shows centuries of oxidation and decay. It is currently in the Up position.',
    '{
      "interaction_type": "Pull",
      "requires_check": false,
      "consequence_type": "Unlock",
      "consequence_data": {"target_type": "door", "action": "toggle"}
    }',
    '["The_Roots"]',
    1.0,
    1
);

-- Example 2: Crystalline Control Console (Console_Base + Crystalline + Hazard_Control)
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id,
    modifier_id,
    final_name,
    final_description,
    final_mechanics,
    biome_restrictions,
    spawn_weight,
    is_active
) VALUES (
    (SELECT template_id FROM Descriptor_Base_Templates WHERE template_name = 'Console_Base'),
    (SELECT modifier_id FROM Descriptor_Thematic_Modifiers WHERE modifier_name = 'Crystalline'),
    'Crystalline Control Console',
    'A crystalline control console pulses with unstable Aetheric energy. Its display flickers with incomprehensible symbols.',
    '{
      "interaction_type": "Hack",
      "requires_check": true,
      "check_type": "WITS",
      "check_dc": 15,
      "consequence_type": "Trigger",
      "consequence_data": {"action": "disable_hazards", "room": "current"}
    }',
    '["Alfheim"]',
    1.2,
    1
);

-- Example 3: Scorched Crate (Crate_Base + Scorched)
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id,
    modifier_id,
    final_name,
    final_description,
    final_mechanics,
    biome_restrictions,
    spawn_weight,
    is_active
) VALUES (
    (SELECT template_id FROM Descriptor_Base_Templates WHERE template_name = 'Crate_Base'),
    (SELECT modifier_id FROM Descriptor_Thematic_Modifiers WHERE modifier_name = 'Scorched'),
    'Fire-Damaged Crate',
    'A scorched crate radiates residual heat. It appears unlocked.',
    '{
      "interaction_type": "Search",
      "requires_check": false,
      "locked": false,
      "loot_tier": "Common"
    }',
    '["Muspelheim"]',
    1.0,
    1
);

-- =====================================================
-- SCHEMA VERSION TRACKING
-- =====================================================

INSERT OR IGNORE INTO Schema_Migrations (version, description, applied_at)
VALUES (
    'v0.38.3b',
    'Interactive Objects: Tables (Interactive_Objects, Object_Interactions, Object_Function_Variants) + 30+ function variants',
    CURRENT_TIMESTAMP
);

-- =====================================================
-- END v0.38.3 Tables & Variants
-- =====================================================
