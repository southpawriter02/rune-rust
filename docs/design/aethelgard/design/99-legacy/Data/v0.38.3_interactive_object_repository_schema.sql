-- =====================================================
-- v0.38.3: Interactive Object Repository - Base Templates
-- =====================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Define 9+ base interactive object templates
--          for Dynamic Room Engine (v0.11) population system
-- Categories: Mechanisms (Lever, Console, Pressure Plate)
--            Containers (Crate, Chest, Locker)
--            Investigatable (Corpse, Data Slate)
--            Barriers (Door)
-- =====================================================

-- =====================================================
-- MECHANISM TEMPLATES
-- =====================================================

-- Template 1: Lever_Base (Simple Mechanism)
-- Category: Object | Type: Mechanism
-- Tags: Mechanism, Simple, Puzzle
-- Use Case: Door controls, power diverters, bridge extenders
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags,
    notes
) VALUES (
    'Lever_Base',
    'Object',
    'Mechanism',
    '{
      "interaction_type": "Pull",
      "requires_check": false,
      "states": ["Up", "Down"],
      "reversible": true,
      "consequence_type": "Trigger",
      "range": "Adjacent"
    }',
    '{Modifier} Lever',
    'A {Modifier_Adj} lever {Modifier_Detail}. It is currently in the {State} position.',
    '["Mechanism", "Simple", "Puzzle"]',
    'Simple pull mechanism with reversible states (Up/Down)'
);

-- Template 2: Console_Base (Hacking Mechanism)
-- Category: Object | Type: Mechanism
-- Tags: Mechanism, Hacking, Technical
-- Use Case: Door overrides, hazard control, data access
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags,
    notes
) VALUES (
    'Console_Base',
    'Object',
    'Mechanism',
    '{
      "interaction_type": "Hack",
      "requires_check": true,
      "check_type": "WITS",
      "check_dc": 15,
      "attempts_allowed": 3,
      "failure_consequence": "Lockout",
      "consequence_type": "Data_Access"
    }',
    '{Modifier} Control Console',
    'A {Modifier_Adj} control console {Modifier_Detail}. Its display {Display_State}.',
    '["Mechanism", "Hacking", "Technical"]',
    'Requires WITS check (DC 15), 3 attempts allowed, lockout on failure'
);

-- Template 3: Pressure_Plate_Base (Hidden Trap Mechanism)
-- Category: Object | Type: Mechanism
-- Tags: Mechanism, Trap, Hidden
-- Use Case: Traps, alarms, environmental hazards
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags,
    notes
) VALUES (
    'Pressure_Plate_Base',
    'Object',
    'Mechanism',
    '{
      "interaction_type": "Automatic",
      "trigger": "Weight",
      "detection_dc": 18,
      "disarm_dc": 20,
      "consequence_type": "Trap",
      "one_time": false
    }',
    'Hidden Pressure Plate',
    'A concealed pressure plate {Modifier_Detail}. [Detected only with WITS check DC 18]',
    '["Mechanism", "Trap", "Hidden"]',
    'Automatic weight trigger, WITS DC 18 to detect, DC 20 to disarm, reusable'
);

-- =====================================================
-- CONTAINER TEMPLATES
-- =====================================================

-- Template 4: Crate_Base (Common Container)
-- Category: Object | Type: Container
-- Tags: Container, Common, Searchable
-- Use Case: Common loot, environmental decoration
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags,
    notes
) VALUES (
    'Crate_Base',
    'Object',
    'Container',
    '{
      "interaction_type": "Search",
      "requires_check": false,
      "locked": false,
      "destructible": true,
      "hp": 20,
      "loot_tier": "Common",
      "search_time": 1
    }',
    '{Modifier} Crate',
    'A {Modifier_Adj} crate {Modifier_Detail}. It appears {Locked_State}.',
    '["Container", "Common", "Searchable"]',
    'Unlocked, destructible (HP: 20), common loot tier, 1 turn search time'
);

-- Template 5: Chest_Base (Valuable Container)
-- Category: Object | Type: Container
-- Tags: Container, Valuable, Lockable
-- Use Case: Uncommon/rare loot, possible traps
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags,
    notes
) VALUES (
    'Chest_Base',
    'Object',
    'Container',
    '{
      "interaction_type": "Open",
      "requires_check": true,
      "check_type": "Lockpicking",
      "check_dc": 15,
      "locked": true,
      "destructible": false,
      "loot_tier": "Uncommon",
      "trap_chance": 0.2
    }',
    '{Modifier} Chest',
    'A {Modifier_Adj} chest {Modifier_Detail}. It is {Locked_State}.',
    '["Container", "Valuable", "Lockable"]',
    'Locked, requires Lockpicking DC 15, uncommon loot tier, 20% trap chance'
);

-- Template 6: Locker_Base (Personal Storage)
-- Category: Object | Type: Container
-- Tags: Container, Personal, Lockable
-- Use Case: Personal effects, common loot, narrative items
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags,
    notes
) VALUES (
    'Locker_Base',
    'Object',
    'Container',
    '{
      "interaction_type": "Open",
      "requires_check": true,
      "check_type": "Lockpicking",
      "check_dc": 12,
      "locked": true,
      "destructible": true,
      "hp": 30,
      "loot_tier": "Common",
      "personal_effects": true
    }',
    '{Modifier} Locker',
    'A {Modifier_Adj} personal locker {Modifier_Detail}. A faded nameplate reads ''{Random_Name}''.',
    '["Container", "Personal", "Lockable"]',
    'Locked, Lockpicking DC 12, destructible (HP: 30), personal effects (narrative)'
);

-- =====================================================
-- INVESTIGATABLE TEMPLATES
-- =====================================================

-- Template 7: Corpse_Base (Lootable Narrative Object)
-- Category: Object | Type: Investigatable
-- Tags: Investigatable, Loot, Narrative
-- Use Case: Environmental storytelling, loot, quest clues
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags,
    notes
) VALUES (
    'Corpse_Base',
    'Object',
    'Investigatable',
    '{
      "interaction_type": "Search",
      "requires_check": false,
      "loot_tier": "Random",
      "search_time": 1,
      "narrative_clue": true,
      "decay_state": "Recent"
    }',
    '{Entity_Type} Corpse',
    'The corpse of {Article} {Entity_Type} lies here. {Decay_Description}. {Cause_Of_Death_Clue}.',
    '["Investigatable", "Loot", "Narrative"]',
    'No check required, random loot tier, provides narrative clue about cause of death'
);

-- Template 8: Data_Slate_Base (Readable Object)
-- Category: Object | Type: Investigatable
-- Tags: Investigatable, Readable, Quest
-- Use Case: Lore delivery, quest hooks, environmental storytelling
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags,
    notes
) VALUES (
    'Data_Slate_Base',
    'Object',
    'Investigatable',
    '{
      "interaction_type": "Read",
      "requires_check": false,
      "text_length": "Short",
      "corruption_level": "None",
      "quest_hook": false,
      "lore_value": "Medium"
    }',
    '{Modifier} Data-Slate',
    'A {Modifier_Adj} Data-Slate {Modifier_Detail}. Its display {Display_State}.',
    '["Investigatable", "Readable", "Quest"]',
    'No check required, provides text content (lore, quest hooks, personal logs)'
);

-- =====================================================
-- BARRIER TEMPLATES
-- =====================================================

-- Template 9: Door_Base (Passage Barrier)
-- Category: Object | Type: Barrier
-- Tags: Barrier, Passage, Lockable
-- Use Case: Room connections, locked passages, destructible barriers
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name,
    category,
    archetype,
    base_mechanics,
    name_template,
    description_template,
    tags,
    notes
) VALUES (
    'Door_Base',
    'Object',
    'Barrier',
    '{
      "interaction_type": "Open",
      "requires_check": false,
      "locked": false,
      "destructible": true,
      "hp": 50,
      "soak": 10,
      "blocks_los": true
    }',
    '{Modifier} Door',
    'A {Modifier_Adj} door {Modifier_Detail}. It is currently {State}.',
    '["Barrier", "Passage", "Lockable"]',
    'Standard door (HP: 50, Soak: 10), can be locked, destructible, blocks line of sight'
);

-- =====================================================
-- SCHEMA VERSION TRACKING
-- =====================================================

INSERT OR IGNORE INTO Schema_Migrations (version, description, applied_at)
VALUES (
    'v0.38.3',
    'Interactive Object Repository: 9 base templates (3 mechanisms, 3 containers, 2 investigatable, 1 barrier)',
    CURRENT_TIMESTAMP
);

-- =====================================================
-- END v0.38.3 Base Templates
-- =====================================================
