-- v0.37.1: Core Navigation Commands - Database Schema
-- Document ID: RR-SPEC-v0.37.1-NAVIGATION-COMMANDS
-- Description: Schema for interactive objects, searchable containers, and navigation command support

-- ============================================
-- Interactive Objects (for investigate command)
-- ============================================
CREATE TABLE IF NOT EXISTS InteractiveObjects (
    object_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,
    object_name TEXT NOT NULL,
    description TEXT,
    detailed_description TEXT,

    -- Investigation properties
    is_investigatable BOOLEAN DEFAULT 0,
    investigation_dc INTEGER DEFAULT 2,
    success_description TEXT,
    failure_description TEXT,
    already_investigated BOOLEAN DEFAULT 0,

    -- Rewards (JSON or foreign key to rewards table)
    reward_items TEXT,  -- JSON: [{"item_id": 1, "quantity": 1}, ...]
    reward_currency INTEGER DEFAULT 0,
    reward_components TEXT,  -- JSON: {"ScrapMetal": 5, ...}

    -- Metadata
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (room_id) REFERENCES Rooms(room_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_interactive_objects_room
ON InteractiveObjects(room_id);

CREATE INDEX IF NOT EXISTS idx_interactive_objects_investigatable
ON InteractiveObjects(is_investigatable)
WHERE is_investigatable = 1;

-- ============================================
-- Searchable Containers (for search command)
-- ============================================
CREATE TABLE IF NOT EXISTS SearchableContainers (
    container_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,
    container_name TEXT NOT NULL,
    container_type TEXT DEFAULT 'chest',  -- chest, crate, barrel, corpse, etc.
    description TEXT,

    -- Search properties
    already_searched BOOLEAN DEFAULT 0,
    requires_key BOOLEAN DEFAULT 0,
    key_item_id INTEGER,  -- Foreign key to Items table

    -- Hidden compartments (requires investigation)
    has_hidden_compartment BOOLEAN DEFAULT 0,
    hidden_revealed BOOLEAN DEFAULT 0,
    hidden_investigation_dc INTEGER DEFAULT 3,

    -- Loot tier
    loot_tier INTEGER DEFAULT 0,  -- 0=trash, 1=common, 2=uncommon, 3=rare

    -- Metadata
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (room_id) REFERENCES Rooms(room_id) ON DELETE CASCADE,
    FOREIGN KEY (key_item_id) REFERENCES Items(item_id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_searchable_containers_room
ON SearchableContainers(room_id);

CREATE INDEX IF NOT EXISTS idx_searchable_containers_unsearched
ON SearchableContainers(already_searched)
WHERE already_searched = 0;

-- ============================================
-- Container Contents (what's inside containers)
-- ============================================
CREATE TABLE IF NOT EXISTS ContainerContents (
    content_id INTEGER PRIMARY KEY AUTOINCREMENT,
    container_id INTEGER NOT NULL,

    -- What type of content
    content_type TEXT NOT NULL,  -- 'equipment', 'component', 'currency', 'consumable'

    -- Equipment
    item_id INTEGER,  -- Foreign key to Items/Equipment table
    quantity INTEGER DEFAULT 1,

    -- Components
    component_type TEXT,  -- ComponentType enum value
    component_quantity INTEGER,

    -- Currency
    currency_amount INTEGER,

    -- Consumables
    consumable_id INTEGER,

    -- Hidden content flag
    is_hidden BOOLEAN DEFAULT 0,  -- Only revealed after investigation

    FOREIGN KEY (container_id) REFERENCES SearchableContainers(container_id) ON DELETE CASCADE,
    FOREIGN KEY (item_id) REFERENCES Items(item_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_container_contents_container
ON ContainerContents(container_id);

-- ============================================
-- Command History (for analytics and debugging)
-- ============================================
CREATE TABLE IF NOT EXISTS CommandHistory (
    history_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    command_text TEXT NOT NULL,
    command_type TEXT NOT NULL,  -- look, go, investigate, search, etc.
    target TEXT,  -- What was targeted (room direction, object name, etc.)

    -- Execution results
    executed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    success BOOLEAN NOT NULL,
    error_message TEXT,

    -- Context
    room_id INTEGER,
    turn_number INTEGER,

    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_command_history_character
ON CommandHistory(character_id, executed_at);

CREATE INDEX IF NOT EXISTS idx_command_history_command_type
ON CommandHistory(command_type);

-- ============================================
-- Command Aliases (extensible alias system)
-- ============================================
CREATE TABLE IF NOT EXISTS CommandAliases (
    alias_id INTEGER PRIMARY KEY AUTOINCREMENT,
    alias_text TEXT NOT NULL UNIQUE,
    canonical_command TEXT NOT NULL,

    -- Metadata
    is_active BOOLEAN DEFAULT 1,
    is_custom BOOLEAN DEFAULT 0,  -- Custom aliases defined by players
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    CHECK (length(alias_text) > 0 AND length(canonical_command) > 0)
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_aliases_text
ON CommandAliases(alias_text)
WHERE is_active = 1;

-- ============================================
-- Seed Data: Default Command Aliases
-- ============================================

-- Look aliases
INSERT OR IGNORE INTO CommandAliases (alias_text, canonical_command, is_custom) VALUES
('l', 'look', 0),
('examine', 'look', 0),
('x', 'look', 0);

-- Go/Movement aliases
INSERT OR IGNORE INTO CommandAliases (alias_text, canonical_command, is_custom) VALUES
('g', 'go', 0),
('move', 'go', 0),
('n', 'go north', 0),
('s', 'go south', 0),
('e', 'go east', 0),
('w', 'go west', 0),
('north', 'go north', 0),
('south', 'go south', 0),
('east', 'go east', 0),
('west', 'go west', 0);

-- Investigate aliases
INSERT OR IGNORE INTO CommandAliases (alias_text, canonical_command, is_custom) VALUES
('inv', 'investigate', 0),
('examine', 'investigate', 0);

-- Search aliases (already canonical, but adding common variants)
INSERT OR IGNORE INTO CommandAliases (alias_text, canonical_command, is_custom) VALUES
('loot', 'search', 0),
('check', 'search', 0);

-- ============================================
-- Verification Queries
-- ============================================

-- Check that aliases table was created and populated
-- SELECT COUNT(*) as alias_count FROM CommandAliases WHERE is_active = 1;

-- Check interactive objects structure
-- SELECT * FROM sqlite_master WHERE type='table' AND name='InteractiveObjects';

-- Check searchable containers structure
-- SELECT * FROM sqlite_master WHERE type='table' AND name='SearchableContainers';
