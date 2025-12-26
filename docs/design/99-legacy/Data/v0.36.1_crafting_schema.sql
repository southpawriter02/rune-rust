-- ═══════════════════════════════════════════════════════════════════
-- v0.36.1: Advanced Crafting System - Database Schema & Recipe Definitions
-- ═══════════════════════════════════════════════════════════════════
-- Version: v0.36.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-17
-- Prerequisites: v0.3 (Equipment & Loot)
-- ═══════════════════════════════════════════════════════════════════
-- Document ID: RR-SPEC-v0.36.1-CRAFTING-DATABASE
-- Parent Specification: v0.36: Advanced Crafting System
-- Status: Implementation Ready
-- Timeline: 7-10 hours
-- ═══════════════════════════════════════════════════════════════════

BEGIN TRANSACTION;

-- ═══════════════════════════════════════════════════════════════════
-- PREREQUISITE TABLES: Items & Character_Inventory
-- (Create if they don't exist for forward compatibility)
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Worlds (
    world_id INTEGER PRIMARY KEY,
    world_name TEXT NOT NULL,
    world_description TEXT
);

CREATE TABLE IF NOT EXISTS Sectors (
    sector_id INTEGER PRIMARY KEY,
    sector_name TEXT NOT NULL,
    world_id INTEGER,
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id)
);

-- Insert default world if not exists
INSERT OR IGNORE INTO Worlds (world_id, world_name, world_description)
VALUES (1, 'Midgard', 'The Trunk - Central hub of Yggdrasil');

-- Insert base sectors if not exist
INSERT OR IGNORE INTO Sectors (sector_id, sector_name, world_id)
VALUES
(1, 'Midgard Central', 1),
(2, 'Muspelheim', 1),
(3, 'Niflheim', 1),
(4, 'Alfheim', 1),
(5, 'Jotunheim', 1);

-- Items Table: Core item definitions
CREATE TABLE IF NOT EXISTS Items (
    item_id INTEGER PRIMARY KEY,
    item_name TEXT NOT NULL,
    item_type TEXT NOT NULL CHECK(item_type IN ('Weapon', 'Armor', 'Consumable', 'Component', 'Quest', 'Misc')),
    quality_tier INTEGER DEFAULT 1 CHECK(quality_tier BETWEEN 1 AND 5),
    item_description TEXT,
    value_credits INTEGER DEFAULT 0,
    is_stackable BOOLEAN DEFAULT 0,
    max_stack_size INTEGER DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_items_type ON Items(item_type);
CREATE INDEX IF NOT EXISTS idx_items_quality ON Items(quality_tier);

-- Character_Inventory Table: Player item storage
CREATE TABLE IF NOT EXISTS Character_Inventory (
    inventory_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    item_id INTEGER NOT NULL,
    quantity INTEGER DEFAULT 1 CHECK(quantity > 0),
    quality_tier INTEGER DEFAULT 1 CHECK(quality_tier BETWEEN 1 AND 5),
    is_equipped BOOLEAN DEFAULT 0,
    acquired_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
    FOREIGN KEY (item_id) REFERENCES Items(item_id)
);

CREATE INDEX IF NOT EXISTS idx_inventory_character ON Character_Inventory(character_id);
CREATE INDEX IF NOT EXISTS idx_inventory_item ON Character_Inventory(item_id);
CREATE INDEX IF NOT EXISTS idx_inventory_equipped ON Character_Inventory(character_id, is_equipped);

-- ═══════════════════════════════════════════════════════════════════
-- CRAFTING SYSTEM TABLES (6 New Tables)
-- ═══════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════
-- Table 1: Crafting_Recipes
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Crafting_Recipes (
    recipe_id INTEGER PRIMARY KEY AUTOINCREMENT,
    recipe_name TEXT NOT NULL,
    recipe_tier TEXT NOT NULL CHECK(recipe_tier IN ('Basic', 'Advanced', 'Expert', 'Master')),
    crafted_item_type TEXT NOT NULL CHECK(crafted_item_type IN ('Weapon', 'Armor', 'Consumable', 'Inscription')),
    crafted_item_base_id INTEGER,
    required_station TEXT NOT NULL CHECK(required_station IN ('Forge', 'Workshop', 'Laboratory', 'Runic_Altar', 'Field_Station', 'Any')),
    crafting_time_minutes INTEGER DEFAULT 5 CHECK(crafting_time_minutes >= 0),
    quality_bonus INTEGER DEFAULT 0 CHECK(quality_bonus >= 0 AND quality_bonus <= 2),
    discovery_method TEXT CHECK(discovery_method IN ('Merchant', 'Quest', 'Loot', 'Ability', 'Default')),
    recipe_description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (crafted_item_base_id) REFERENCES Items(item_id)
);

CREATE INDEX IF NOT EXISTS idx_recipes_tier ON Crafting_Recipes(recipe_tier);
CREATE INDEX IF NOT EXISTS idx_recipes_item_type ON Crafting_Recipes(crafted_item_type);
CREATE INDEX IF NOT EXISTS idx_recipes_station ON Crafting_Recipes(required_station);

-- ═══════════════════════════════════════════════════════════════════
-- Table 2: Recipe_Components
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Recipe_Components (
    component_id INTEGER PRIMARY KEY AUTOINCREMENT,
    recipe_id INTEGER NOT NULL,
    component_item_id INTEGER NOT NULL,
    quantity_required INTEGER NOT NULL CHECK(quantity_required > 0),
    minimum_quality INTEGER DEFAULT 1 CHECK(minimum_quality BETWEEN 1 AND 5),
    is_consumed BOOLEAN DEFAULT 1,

    FOREIGN KEY (recipe_id) REFERENCES Crafting_Recipes(recipe_id) ON DELETE CASCADE,
    FOREIGN KEY (component_item_id) REFERENCES Items(item_id)
);

CREATE INDEX IF NOT EXISTS idx_recipe_components_recipe ON Recipe_Components(recipe_id);
CREATE INDEX IF NOT EXISTS idx_recipe_components_item ON Recipe_Components(component_item_id);

-- ═══════════════════════════════════════════════════════════════════
-- Table 3: Crafting_Stations
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Crafting_Stations (
    station_id INTEGER PRIMARY KEY AUTOINCREMENT,
    station_name TEXT NOT NULL,
    station_type TEXT NOT NULL CHECK(station_type IN ('Forge', 'Workshop', 'Laboratory', 'Runic_Altar', 'Field_Station')),
    max_quality_tier INTEGER NOT NULL CHECK(max_quality_tier BETWEEN 1 AND 5),
    location_world_id INTEGER NOT NULL DEFAULT 1,
    location_sector_id INTEGER,
    location_room_id TEXT,
    is_portable BOOLEAN DEFAULT 0,
    station_description TEXT,

    FOREIGN KEY (location_world_id) REFERENCES Worlds(world_id),
    FOREIGN KEY (location_sector_id) REFERENCES Sectors(sector_id)
);

CREATE INDEX IF NOT EXISTS idx_stations_type ON Crafting_Stations(station_type);
CREATE INDEX IF NOT EXISTS idx_stations_sector ON Crafting_Stations(location_sector_id);

-- ═══════════════════════════════════════════════════════════════════
-- Table 4: Character_Recipes
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Character_Recipes (
    character_recipe_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    recipe_id INTEGER NOT NULL,
    discovered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    times_crafted INTEGER DEFAULT 0,
    discovery_source TEXT,

    FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
    FOREIGN KEY (recipe_id) REFERENCES Crafting_Recipes(recipe_id) ON DELETE CASCADE,
    UNIQUE(character_id, recipe_id)
);

CREATE INDEX IF NOT EXISTS idx_character_recipes_char ON Character_Recipes(character_id);
CREATE INDEX IF NOT EXISTS idx_character_recipes_recipe ON Character_Recipes(recipe_id);

-- ═══════════════════════════════════════════════════════════════════
-- Table 5: Equipment_Modifications
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Equipment_Modifications (
    modification_id INTEGER PRIMARY KEY AUTOINCREMENT,
    equipment_item_id INTEGER NOT NULL,
    modification_type TEXT NOT NULL CHECK(modification_type IN ('Stat_Boost', 'Resistance', 'Special_Effect', 'Inscription', 'Quality_Upgrade')),
    modification_name TEXT NOT NULL,
    modification_value TEXT NOT NULL,
    is_permanent BOOLEAN DEFAULT 1,
    remaining_uses INTEGER,
    applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    applied_by_recipe_id INTEGER,

    FOREIGN KEY (equipment_item_id) REFERENCES Character_Inventory(inventory_id) ON DELETE CASCADE,
    FOREIGN KEY (applied_by_recipe_id) REFERENCES Crafting_Recipes(recipe_id)
);

CREATE INDEX IF NOT EXISTS idx_modifications_equipment ON Equipment_Modifications(equipment_item_id);
CREATE INDEX IF NOT EXISTS idx_modifications_type ON Equipment_Modifications(modification_type);

-- ═══════════════════════════════════════════════════════════════════
-- Table 6: Runic_Inscriptions
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Runic_Inscriptions (
    inscription_id INTEGER PRIMARY KEY AUTOINCREMENT,
    inscription_name TEXT NOT NULL,
    inscription_tier INTEGER NOT NULL CHECK(inscription_tier BETWEEN 1 AND 5),
    target_equipment_type TEXT NOT NULL CHECK(target_equipment_type IN ('Weapon', 'Armor', 'Both')),
    effect_type TEXT NOT NULL CHECK(effect_type IN ('Damage_Bonus', 'Elemental', 'Resistance', 'Status', 'Special')),
    effect_value TEXT NOT NULL,
    is_temporary BOOLEAN DEFAULT 0,
    uses_if_temporary INTEGER DEFAULT 10,
    component_requirements TEXT NOT NULL,
    crafting_cost_credits INTEGER DEFAULT 0,
    inscription_description TEXT,

    UNIQUE(inscription_name)
);

CREATE INDEX IF NOT EXISTS idx_inscriptions_tier ON Runic_Inscriptions(inscription_tier);
CREATE INDEX IF NOT EXISTS idx_inscriptions_type ON Runic_Inscriptions(effect_type);
CREATE INDEX IF NOT EXISTS idx_inscriptions_target ON Runic_Inscriptions(target_equipment_type);

-- ═══════════════════════════════════════════════════════════════════
-- SEED DATA: Component Items (50+ items)
-- ═══════════════════════════════════════════════════════════════════

-- ═════ WEAPON COMPONENTS ═════

-- Metal Ingots (Tier 1-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(5001, 'Scrap Iron Ingot', 'Component', 1, 'Salvaged iron suitable for basic weapon frames.', 10, 1, 50),
(5002, 'Steel Ingot', 'Component', 2, 'Refined steel for moderate quality weapons.', 25, 1, 50),
(5003, 'Titanium Alloy', 'Component', 3, 'Advanced titanium alloy for high-quality weapons.', 75, 1, 50),
(5004, 'Star-Metal Ingot', 'Component', 4, 'Pre-Glitch exotic metal with exceptional properties.', 200, 1, 50);

-- Power Cores (Tier 2-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(5010, 'Basic Energy Cell', 'Component', 2, 'Standard power source for energy weapons.', 30, 1, 20),
(5011, 'Plasma Core', 'Component', 3, 'High-output plasma generation core.', 100, 1, 20),
(5012, 'Fusion Cell', 'Component', 4, 'Miniaturized fusion reactor for advanced weapons.', 250, 1, 20);

-- Weapon Frames (Tier 2-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(5020, 'Standard Frame', 'Component', 2, 'Basic weapon structural framework.', 20, 1, 30),
(5021, 'Reinforced Frame', 'Component', 3, 'Reinforced structural frame with stabilizers.', 60, 1, 30),
(5022, 'Precision Frame', 'Component', 4, 'Precision-machined frame with integrated systems.', 150, 1, 30);

-- Grips/Handles (Tier 1-3)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(5030, 'Salvaged Grip', 'Component', 1, 'Worn grip from discarded weapon.', 5, 1, 50),
(5031, 'Synthetic Grip', 'Component', 2, 'Ergonomic synthetic material grip.', 15, 1, 50),
(5032, 'Neural Interface Grip', 'Component', 3, 'Bio-reactive grip with neural feedback.', 50, 1, 50);

-- ═════ ARMOR COMPONENTS ═════

-- Alloy Plates (Tier 1-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(6001, 'Scrap Plating', 'Component', 1, 'Salvaged armor plating, heavily worn.', 8, 1, 50),
(6002, 'Steel Plate', 'Component', 2, 'Standard steel armor plating.', 20, 1, 50),
(6003, 'Composite Plate', 'Component', 3, 'Layered composite armor plating.', 70, 1, 50),
(6004, 'Reactive Armor Plate', 'Component', 4, 'Advanced reactive armor with energy dispersion.', 180, 1, 50);

-- Mesh Weave (Tier 2-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(6010, 'Ballistic Mesh', 'Component', 2, 'Flexible ballistic fiber mesh.', 25, 1, 40),
(6011, 'Nano-Weave', 'Component', 3, 'Nano-fiber weave with self-repair properties.', 80, 1, 40),
(6012, 'Adaptive Mesh', 'Component', 4, 'Adaptive nano-weave that responds to impacts.', 200, 1, 40);

-- Insulation (Tier 1-3)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(6020, 'Thermal Padding', 'Component', 1, 'Basic thermal insulation padding.', 5, 1, 50),
(6021, 'Thermal Insulation', 'Component', 2, 'Advanced thermal regulation system.', 18, 1, 50),
(6022, 'Climate Control System', 'Component', 3, 'Active climate control insulation.', 55, 1, 50);

-- Servo Actuators (Tier 3-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(6030, 'Standard Servo', 'Component', 3, 'Powered actuator for powered armor systems.', 90, 1, 30),
(6031, 'Enhanced Servo', 'Component', 4, 'High-output servo with neural link.', 220, 1, 30);

-- ═════ CONSUMABLE COMPONENTS ═════

-- Chemical Compounds (Tier 1-3)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(7001, 'Basic Chemicals', 'Component', 1, 'Common chemical reagents.', 3, 1, 99),
(7002, 'Refined Compounds', 'Component', 2, 'Purified chemical compounds.', 12, 1, 99),
(7003, 'Exotic Reagents', 'Component', 3, 'Rare chemical reagents with unique properties.', 40, 1, 99);

-- Biological Samples (Tier 1-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(7010, 'Organic Matter', 'Component', 1, 'Generic biological tissue samples.', 4, 1, 99),
(7011, 'Adrenaline Extract', 'Component', 2, 'Extracted adrenaline compound.', 15, 1, 99),
(7012, 'Enzyme Catalyst', 'Component', 3, 'Biologically derived enzyme catalyst.', 50, 1, 99),
(7013, 'Stem Cell Culture', 'Component', 4, 'Pre-Glitch stem cell culture sample.', 150, 1, 99);

-- Electronic Parts (Tier 1-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(7020, 'Salvaged Circuitry', 'Component', 1, 'Recovered electronic components.', 5, 1, 99),
(7021, 'Integrated Circuits', 'Component', 2, 'Functional integrated circuit chips.', 18, 1, 99),
(7022, 'Nano-Processors', 'Component', 3, 'Advanced nano-scale processors.', 65, 1, 99),
(7023, 'Quantum Chips', 'Component', 4, 'Pre-Glitch quantum computing chips.', 180, 1, 99);

-- ═════ RUNIC COMPONENTS ═════

-- Aetheric Shards (Tier 3-5)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(9001, 'Minor Aetheric Shard', 'Component', 3, 'Crystallized fragment of Aetheric energy.', 100, 1, 20),
(9002, 'Aetheric Shard', 'Component', 4, 'Significant Aetheric crystal with contained energy.', 250, 1, 20),
(9003, 'Major Aetheric Shard', 'Component', 5, 'Massive Aetheric crystal, pulsing with power.', 600, 1, 20);

-- Glyphs (Tier 2-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(9010, 'Basic Glyph', 'Component', 2, 'Pre-inscribed runic glyph pattern.', 40, 1, 30),
(9011, 'Complex Glyph', 'Component', 3, 'Intricate multi-layered glyph.', 90, 1, 30),
(9012, 'Master Glyph', 'Component', 4, 'Flawless Pre-Glitch glyph inscription.', 220, 1, 30);

-- Binding Reagents (Tier 2-4)
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(9020, 'Stabilizing Compound', 'Component', 2, 'Chemical stabilizer for Aetheric energy.', 30, 1, 50),
(9021, 'Binding Catalyst', 'Component', 3, 'Catalyst enabling permanent runic bonds.', 75, 1, 50),
(9022, 'Quantum Binder', 'Component', 4, 'Quantum-level binding agent.', 190, 1, 50);

-- ═════ ADDITIONAL WEAPON COMPONENTS ═════

-- Weapon Barrels & Receivers
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(5040, 'Salvaged Barrel', 'Component', 1, 'Worn weapon barrel, functional.', 8, 1, 30),
(5041, 'Precision Barrel', 'Component', 3, 'High-quality rifled barrel.', 70, 1, 30),
(5042, 'Energy Conduit', 'Component', 3, 'Focuses and directs energy weapon output.', 85, 1, 30);

-- ═════ ADDITIONAL ARMOR COMPONENTS ═════

-- Armor Linings
INSERT OR IGNORE INTO Items (item_id, item_name, item_type, quality_tier, item_description, value_credits, is_stackable, max_stack_size)
VALUES
(6040, 'Basic Padding', 'Component', 1, 'Simple protective padding.', 6, 1, 50),
(6041, 'Shock-Absorbing Gel', 'Component', 2, 'Impact-dampening gel layer.', 22, 1, 50),
(6042, 'Kinetic Dampener', 'Component', 3, 'Advanced kinetic energy dissipation system.', 68, 1, 50);

-- ═══════════════════════════════════════════════════════════════════
-- SEED DATA: Crafting Recipes (100+ recipes)
-- ═══════════════════════════════════════════════════════════════════

-- ═════ WEAPON RECIPES (30 recipes) ═════

-- Basic Tier Weapons (10 recipes)
INSERT OR IGNORE INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, discovery_method, recipe_description)
VALUES
(1001, 'Improvised Blade', 'Basic', 'Weapon', 'Forge', 0, 'Default', 'Crude melee weapon crafted from scrap metal.'),
(1002, 'Salvaged Pistol', 'Basic', 'Weapon', 'Workshop', 0, 'Default', 'Basic energy pistol assembled from salvaged parts.'),
(1003, 'Makeshift Rifle', 'Basic', 'Weapon', 'Workshop', 0, 'Default', 'Improvised rifle with modest stopping power.'),
(1004, 'Scrap Axe', 'Basic', 'Weapon', 'Forge', 0, 'Default', 'Simple axe from salvaged metal.'),
(1005, 'Crude Spear', 'Basic', 'Weapon', 'Forge', 0, 'Default', 'Basic spear with sharpened metal tip.'),
(1006, 'Improvised Dagger', 'Basic', 'Weapon', 'Workshop', 0, 'Default', 'Quick-craft blade for close combat.'),
(1007, 'Basic Staff', 'Basic', 'Weapon', 'Workshop', 0, 'Default', 'Simple channeling staff.'),
(1008, 'Scrap Mace', 'Basic', 'Weapon', 'Forge', 0, 'Default', 'Heavy blunt weapon from scrap.'),
(1009, 'Simple Bow', 'Basic', 'Weapon', 'Workshop', 0, 'Default', 'Basic ranged weapon.'),
(1010, 'Improvised Hammer', 'Basic', 'Weapon', 'Forge', 0, 'Default', 'Heavy hammer from industrial parts.');

-- Basic Recipe Components
INSERT OR IGNORE INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
-- Improvised Blade (1001)
(1001, 5001, 2, 1), -- 2x Scrap Iron
(1001, 5030, 1, 1), -- 1x Salvaged Grip
-- Salvaged Pistol (1002)
(1002, 5001, 1, 1), -- 1x Scrap Iron
(1002, 5010, 1, 2), -- 1x Basic Energy Cell
(1002, 7020, 1, 1), -- 1x Salvaged Circuitry
-- Makeshift Rifle (1003)
(1003, 5002, 1, 2), -- 1x Steel Ingot
(1003, 5020, 1, 2), -- 1x Standard Frame
(1003, 5031, 1, 2), -- 1x Synthetic Grip
-- Scrap Axe (1004)
(1004, 5001, 1, 1), -- 1x Scrap Iron
(1004, 5030, 1, 1), -- 1x Salvaged Grip
-- Crude Spear (1005)
(1005, 5001, 1, 1), -- 1x Scrap Iron
(1005, 5040, 1, 1), -- 1x Salvaged Barrel (pole)
-- Improvised Dagger (1006)
(1006, 5001, 1, 1), -- 1x Scrap Iron
(1006, 6040, 1, 1), -- 1x Basic Padding (grip wrap)
-- Basic Staff (1007)
(1007, 5040, 1, 1), -- 1x Salvaged Barrel (shaft)
(1007, 7020, 1, 1), -- 1x Salvaged Circuitry
-- Scrap Mace (1008)
(1008, 5001, 2, 1), -- 2x Scrap Iron
(1008, 5030, 1, 1), -- 1x Salvaged Grip
-- Simple Bow (1009)
(1009, 5040, 1, 1), -- 1x Salvaged Barrel (frame)
(1009, 6040, 1, 1), -- 1x Basic Padding (string)
-- Improvised Hammer (1010)
(1010, 5001, 2, 1), -- 2x Scrap Iron
(1010, 5030, 1, 1); -- 1x Salvaged Grip

-- Advanced Tier Weapons (10 recipes)
INSERT OR IGNORE INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, discovery_method, recipe_description)
VALUES
(1020, 'Plasma Rifle', 'Advanced', 'Weapon', 'Workshop', 1, 'Merchant', 'Energy weapon firing superheated plasma bolts.'),
(1021, 'Vibro-Blade', 'Advanced', 'Weapon', 'Forge', 1, 'Merchant', 'Melee weapon with high-frequency vibration edge.'),
(1022, 'Arc Pistol', 'Advanced', 'Weapon', 'Workshop', 1, 'Quest', 'Energy pistol with electrical discharge capability.'),
(1023, 'Reinforced Greatsword', 'Advanced', 'Weapon', 'Forge', 1, 'Merchant', 'Heavy two-handed blade with reinforced structure.'),
(1024, 'Combat Spear', 'Advanced', 'Weapon', 'Forge', 1, 'Merchant', 'Military-grade spear with balanced design.'),
(1025, 'Energy Lance', 'Advanced', 'Weapon', 'Workshop', 1, 'Quest', 'Powered lance with energy blade.'),
(1026, 'Tactical Shotgun', 'Advanced', 'Weapon', 'Workshop', 1, 'Merchant', 'Close-range energy scatter weapon.'),
(1027, 'Compound Bow', 'Advanced', 'Weapon', 'Workshop', 1, 'Merchant', 'Advanced ranged weapon with improved draw.'),
(1028, 'Power Axe', 'Advanced', 'Weapon', 'Forge', 1, 'Quest', 'Powered axe with energy-enhanced edge.'),
(1029, 'Phase Dagger', 'Advanced', 'Weapon', 'Workshop', 1, 'Quest', 'Dagger that phases through armor.');

-- Advanced Recipe Components
INSERT OR IGNORE INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
-- Plasma Rifle (1020)
(1020, 5021, 1, 3), -- 1x Reinforced Frame
(1020, 5011, 1, 3), -- 1x Plasma Core
(1020, 5031, 1, 2), -- 1x Synthetic Grip
-- Vibro-Blade (1021)
(1021, 5003, 2, 3), -- 2x Titanium Alloy
(1021, 5032, 1, 3), -- 1x Neural Interface Grip
(1021, 7021, 1, 2), -- 1x Integrated Circuits
-- Arc Pistol (1022)
(1022, 5002, 1, 2), -- 1x Steel Ingot
(1022, 5010, 1, 2), -- 1x Basic Energy Cell
(1022, 7021, 2, 2), -- 2x Integrated Circuits
-- Reinforced Greatsword (1023)
(1023, 5003, 2, 3), -- 2x Titanium Alloy
(1023, 5021, 1, 3), -- 1x Reinforced Frame
(1023, 5032, 1, 3), -- 1x Neural Interface Grip
-- Combat Spear (1024)
(1024, 5003, 1, 3), -- 1x Titanium Alloy
(1024, 5021, 1, 3), -- 1x Reinforced Frame
(1024, 5031, 1, 2), -- 1x Synthetic Grip
-- Energy Lance (1025)
(1025, 5021, 1, 3), -- 1x Reinforced Frame
(1025, 5011, 1, 3), -- 1x Plasma Core
(1025, 7021, 1, 2), -- 1x Integrated Circuits
-- Tactical Shotgun (1026)
(1026, 5020, 1, 2), -- 1x Standard Frame
(1026, 5010, 2, 2), -- 2x Basic Energy Cell
(1026, 7021, 1, 2), -- 1x Integrated Circuits
-- Compound Bow (1027)
(1027, 5002, 1, 2), -- 1x Steel Ingot
(1027, 5020, 1, 2), -- 1x Standard Frame
(1027, 6041, 1, 2), -- 1x Shock-Absorbing Gel
-- Power Axe (1028)
(1028, 5003, 2, 3), -- 2x Titanium Alloy
(1028, 5011, 1, 3), -- 1x Plasma Core
(1028, 5032, 1, 3), -- 1x Neural Interface Grip
-- Phase Dagger (1029)
(1029, 5003, 1, 3), -- 1x Titanium Alloy
(1029, 5011, 1, 3), -- 1x Plasma Core
(1029, 7022, 1, 3); -- 1x Nano-Processors

-- Expert Tier Weapons (10 recipes)
INSERT OR IGNORE INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, discovery_method, recipe_description)
VALUES
(1040, 'Fusion Lance', 'Expert', 'Weapon', 'Workshop', 2, 'Quest', 'Devastating energy weapon powered by fusion reactor.'),
(1041, 'Monomolecular Sword', 'Expert', 'Weapon', 'Forge', 2, 'Loot', 'Blade with monomolecular edge, cuts through most materials.'),
(1042, 'Rail Cannon', 'Expert', 'Weapon', 'Workshop', 2, 'Loot', 'Electromagnetic accelerator firing projectiles at hypersonic velocities.'),
(1043, 'Plasma Greatsword', 'Expert', 'Weapon', 'Forge', 2, 'Quest', 'Massive blade wreathed in superheated plasma.'),
(1044, 'Quantum Rifle', 'Expert', 'Weapon', 'Workshop', 2, 'Loot', 'Rifle utilizing quantum uncertainty for phase-shifting projectiles.'),
(1045, 'Graviton Hammer', 'Expert', 'Weapon', 'Forge', 2, 'Quest', 'Hammer that manipulates gravitational fields.'),
(1046, 'Disruptor Pistol', 'Expert', 'Weapon', 'Workshop', 2, 'Loot', 'Molecular disruption sidearm.'),
(1047, 'Nano-Blade', 'Expert', 'Weapon', 'Workshop', 2, 'Quest', 'Blade composed of programmable nanomachines.'),
(1048, 'Cryo-Spear', 'Expert', 'Weapon', 'Workshop', 2, 'Loot', 'Spear that freezes targets on contact.'),
(1049, 'Arc-Bow', 'Expert', 'Weapon', 'Workshop', 2, 'Quest', 'Bow that fires electrified arrows.');

-- Expert Recipe Components
INSERT OR IGNORE INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
-- Fusion Lance (1040)
(1040, 5022, 1, 4), -- 1x Precision Frame
(1040, 5012, 1, 4), -- 1x Fusion Cell
(1040, 7022, 2, 3), -- 2x Nano-Processors
-- Monomolecular Sword (1041)
(1041, 5004, 1, 4), -- 1x Star-Metal Ingot
(1041, 5032, 1, 3), -- 1x Neural Interface Grip
(1041, 7023, 1, 4), -- 1x Quantum Chips
-- Rail Cannon (1042)
(1042, 5004, 2, 4), -- 2x Star-Metal Ingot
(1042, 5012, 1, 4), -- 1x Fusion Cell
(1042, 7023, 2, 4), -- 2x Quantum Chips
-- Plasma Greatsword (1043)
(1043, 5004, 2, 4), -- 2x Star-Metal Ingot
(1043, 5012, 1, 4), -- 1x Fusion Cell
(1043, 5032, 1, 3), -- 1x Neural Interface Grip
-- Quantum Rifle (1044)
(1044, 5022, 1, 4), -- 1x Precision Frame
(1044, 5012, 1, 4), -- 1x Fusion Cell
(1044, 7023, 2, 4), -- 2x Quantum Chips
-- Graviton Hammer (1045)
(1045, 5004, 3, 4), -- 3x Star-Metal Ingot
(1045, 5012, 1, 4), -- 1x Fusion Cell
(1045, 7023, 1, 4), -- 1x Quantum Chips
-- Disruptor Pistol (1046)
(1046, 5022, 1, 4), -- 1x Precision Frame
(1046, 5012, 1, 4), -- 1x Fusion Cell
(1046, 7022, 2, 3), -- 2x Nano-Processors
-- Nano-Blade (1047)
(1047, 5004, 1, 4), -- 1x Star-Metal Ingot
(1047, 7022, 3, 3), -- 3x Nano-Processors
(1047, 7023, 1, 4), -- 1x Quantum Chips
-- Cryo-Spear (1048)
(1048, 5022, 1, 4), -- 1x Precision Frame
(1048, 5012, 1, 4), -- 1x Fusion Cell
(1048, 7003, 2, 3), -- 2x Exotic Reagents
-- Arc-Bow (1049)
(1049, 5004, 1, 4), -- 1x Star-Metal Ingot
(1049, 5012, 1, 4), -- 1x Fusion Cell
(1049, 7023, 1, 4); -- 1x Quantum Chips

-- ═════ ARMOR RECIPES (30 recipes) ═════

-- Basic Tier Armor (10 recipes)
INSERT OR IGNORE INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, discovery_method, recipe_description)
VALUES
(2001, 'Salvaged Chest Plate', 'Basic', 'Armor', 'Forge', 0, 'Default', 'Basic chest protection from salvaged plating.'),
(2002, 'Scrap Helmet', 'Basic', 'Armor', 'Forge', 0, 'Default', 'Head protection from scrap metal.'),
(2003, 'Makeshift Gloves', 'Basic', 'Armor', 'Workshop', 0, 'Default', 'Hand protection with minimal flexibility.'),
(2004, 'Improvised Boots', 'Basic', 'Armor', 'Workshop', 0, 'Default', 'Basic footwear with metal reinforcement.'),
(2005, 'Scrap Leg Armor', 'Basic', 'Armor', 'Forge', 0, 'Default', 'Leg protection from salvaged plates.'),
(2006, 'Basic Pauldrons', 'Basic', 'Armor', 'Forge', 0, 'Default', 'Shoulder guards from scrap.'),
(2007, 'Padded Vest', 'Basic', 'Armor', 'Workshop', 0, 'Default', 'Light protective vest.'),
(2008, 'Salvaged Bracers', 'Basic', 'Armor', 'Forge', 0, 'Default', 'Forearm protection.'),
(2009, 'Makeshift Shield', 'Basic', 'Armor', 'Forge', 0, 'Default', 'Simple defensive shield.'),
(2010, 'Scrap Belt', 'Basic', 'Armor', 'Workshop', 0, 'Default', 'Utility belt with armor plating.');

-- Basic Armor Components
INSERT OR IGNORE INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
-- Salvaged Chest Plate (2001)
(2001, 6001, 3, 1), -- 3x Scrap Plating
(2001, 6020, 1, 1), -- 1x Thermal Padding
-- Scrap Helmet (2002)
(2002, 6001, 2, 1), -- 2x Scrap Plating
(2002, 6040, 1, 1), -- 1x Basic Padding
-- Makeshift Gloves (2003)
(2003, 6001, 1, 1), -- 1x Scrap Plating
(2003, 6010, 1, 2), -- 1x Ballistic Mesh
-- Improvised Boots (2004)
(2004, 6001, 2, 1), -- 2x Scrap Plating
(2004, 6040, 1, 1), -- 1x Basic Padding
-- Scrap Leg Armor (2005)
(2005, 6001, 2, 1), -- 2x Scrap Plating
(2005, 6020, 1, 1), -- 1x Thermal Padding
-- Basic Pauldrons (2006)
(2006, 6001, 2, 1), -- 2x Scrap Plating
-- Padded Vest (2007)
(2007, 6010, 1, 2), -- 1x Ballistic Mesh
(2007, 6040, 2, 1), -- 2x Basic Padding
-- Salvaged Bracers (2008)
(2008, 6001, 2, 1), -- 2x Scrap Plating
-- Makeshift Shield (2009)
(2009, 6001, 3, 1), -- 3x Scrap Plating
(2009, 5001, 1, 1), -- 1x Scrap Iron (handle)
-- Scrap Belt (2010)
(2010, 6001, 1, 1), -- 1x Scrap Plating
(2010, 6040, 1, 1); -- 1x Basic Padding

-- Advanced Tier Armor (10 recipes)
INSERT OR IGNORE INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, discovery_method, recipe_description)
VALUES
(2020, 'Reinforced Chest Plate', 'Advanced', 'Armor', 'Forge', 1, 'Merchant', 'Composite armor with layered protection.'),
(2021, 'Combat Helmet', 'Advanced', 'Armor', 'Forge', 1, 'Merchant', 'Full-coverage helmet with HUD integration.'),
(2022, 'Tactical Boots', 'Advanced', 'Armor', 'Workshop', 1, 'Quest', 'Armored boots with mobility enhancements.'),
(2023, 'Combat Gloves', 'Advanced', 'Armor', 'Workshop', 1, 'Merchant', 'Tactical gloves with grip enhancement.'),
(2024, 'Reinforced Leg Plates', 'Advanced', 'Armor', 'Forge', 1, 'Merchant', 'Heavy leg armor with joint protection.'),
(2025, 'Power Pauldrons', 'Advanced', 'Armor', 'Workshop', 1, 'Quest', 'Powered shoulder guards.'),
(2026, 'Tactical Vest', 'Advanced', 'Armor', 'Workshop', 1, 'Merchant', 'Multi-layered protective vest.'),
(2027, 'Combat Bracers', 'Advanced', 'Armor', 'Forge', 1, 'Merchant', 'Reinforced forearm guards.'),
(2028, 'Energy Shield', 'Advanced', 'Armor', 'Workshop', 1, 'Quest', 'Powered energy barrier shield.'),
(2029, 'Utility Harness', 'Advanced', 'Armor', 'Workshop', 1, 'Merchant', 'Advanced load-bearing harness.');

-- Advanced Armor Components
INSERT OR IGNORE INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
-- Reinforced Chest Plate (2020)
(2020, 6003, 2, 3), -- 2x Composite Plate
(2020, 6011, 1, 3), -- 1x Nano-Weave
(2020, 6021, 1, 2), -- 1x Thermal Insulation
-- Combat Helmet (2021)
(2021, 6003, 1, 3), -- 1x Composite Plate
(2021, 7021, 1, 2), -- 1x Integrated Circuits
(2021, 6041, 1, 2), -- 1x Shock-Absorbing Gel
-- Tactical Boots (2022)
(2022, 6003, 1, 3), -- 1x Composite Plate
(2022, 6011, 1, 3), -- 1x Nano-Weave
(2022, 6030, 1, 3), -- 1x Standard Servo
-- Combat Gloves (2023)
(2023, 6003, 1, 3), -- 1x Composite Plate
(2023, 6011, 1, 3), -- 1x Nano-Weave
-- Reinforced Leg Plates (2024)
(2024, 6003, 2, 3), -- 2x Composite Plate
(2024, 6011, 1, 3), -- 1x Nano-Weave
(2024, 6021, 1, 2), -- 1x Thermal Insulation
-- Power Pauldrons (2025)
(2025, 6003, 2, 3), -- 2x Composite Plate
(2025, 6030, 1, 3), -- 1x Standard Servo
(2025, 7021, 1, 2), -- 1x Integrated Circuits
-- Tactical Vest (2026)
(2026, 6011, 2, 3), -- 2x Nano-Weave
(2026, 6041, 2, 2), -- 2x Shock-Absorbing Gel
-- Combat Bracers (2027)
(2027, 6003, 2, 3), -- 2x Composite Plate
(2027, 6011, 1, 3), -- 1x Nano-Weave
-- Energy Shield (2028)
(2028, 6003, 2, 3), -- 2x Composite Plate
(2028, 5011, 1, 3), -- 1x Plasma Core
(2028, 7021, 2, 2), -- 2x Integrated Circuits
-- Utility Harness (2029)
(2029, 6011, 1, 3), -- 1x Nano-Weave
(2029, 7021, 1, 2), -- 1x Integrated Circuits
(2029, 6041, 1, 2); -- 1x Shock-Absorbing Gel

-- Expert Tier Armor (10 recipes)
INSERT OR IGNORE INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, discovery_method, recipe_description)
VALUES
(2040, 'Powered Exo-Armor', 'Expert', 'Armor', 'Workshop', 2, 'Quest', 'Full powered armor suit with integrated systems.'),
(2041, 'Adaptive Combat Suit', 'Expert', 'Armor', 'Workshop', 2, 'Loot', 'Nano-adaptive armor that responds to threats.'),
(2042, 'Assault Helmet', 'Expert', 'Armor', 'Forge', 2, 'Loot', 'Advanced helmet with neural interface and targeting.'),
(2043, 'Quantum Armor', 'Expert', 'Armor', 'Workshop', 2, 'Quest', 'Armor utilizing quantum uncertainty for phase-shifting.'),
(2044, 'Graviton Plating', 'Expert', 'Armor', 'Forge', 2, 'Loot', 'Armor that manipulates gravitational fields for protection.'),
(2045, 'Nano-Mesh Suit', 'Expert', 'Armor', 'Workshop', 2, 'Quest', 'Full-body suit of programmable nanomachines.'),
(2046, 'Cryo-Armor', 'Expert', 'Armor', 'Workshop', 2, 'Loot', 'Armor with integrated cryogenic systems.'),
(2047, 'Energy Barrier Suit', 'Expert', 'Armor', 'Workshop', 2, 'Quest', 'Armor with projected energy barriers.'),
(2048, 'Stealth Carapace', 'Expert', 'Armor', 'Workshop', 2, 'Loot', 'Armor with active camouflage systems.'),
(2049, 'Titan Frame', 'Expert', 'Armor', 'Forge', 2, 'Quest', 'Massive powered armor for maximum protection.');

-- Expert Armor Components
INSERT OR IGNORE INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
-- Powered Exo-Armor (2040)
(2040, 6004, 3, 4), -- 3x Reactive Armor Plate
(2040, 6012, 2, 4), -- 2x Adaptive Mesh
(2040, 6031, 2, 4), -- 2x Enhanced Servo
(2040, 6022, 1, 3), -- 1x Climate Control System
-- Adaptive Combat Suit (2041)
(2041, 6004, 2, 4), -- 2x Reactive Armor Plate
(2041, 6012, 2, 4), -- 2x Adaptive Mesh
(2041, 7023, 1, 4), -- 1x Quantum Chips
-- Assault Helmet (2042)
(2042, 6004, 1, 4), -- 1x Reactive Armor Plate
(2042, 7023, 2, 4), -- 2x Quantum Chips
(2042, 6042, 1, 3), -- 1x Kinetic Dampener
-- Quantum Armor (2043)
(2043, 6004, 3, 4), -- 3x Reactive Armor Plate
(2043, 7023, 3, 4), -- 3x Quantum Chips
(2043, 6012, 1, 4), -- 1x Adaptive Mesh
-- Graviton Plating (2044)
(2044, 6004, 3, 4), -- 3x Reactive Armor Plate
(2044, 5012, 1, 4), -- 1x Fusion Cell
(2044, 7023, 2, 4), -- 2x Quantum Chips
-- Nano-Mesh Suit (2045)
(2045, 6012, 3, 4), -- 3x Adaptive Mesh
(2045, 7022, 3, 3), -- 3x Nano-Processors
(2045, 7023, 1, 4), -- 1x Quantum Chips
-- Cryo-Armor (2046)
(2046, 6004, 2, 4), -- 2x Reactive Armor Plate
(2046, 6012, 1, 4), -- 1x Adaptive Mesh
(2046, 7003, 3, 3), -- 3x Exotic Reagents
(2046, 5012, 1, 4), -- 1x Fusion Cell
-- Energy Barrier Suit (2047)
(2047, 6004, 2, 4), -- 2x Reactive Armor Plate
(2047, 5012, 2, 4), -- 2x Fusion Cell
(2047, 7023, 2, 4), -- 2x Quantum Chips
-- Stealth Carapace (2048)
(2048, 6012, 2, 4), -- 2x Adaptive Mesh
(2048, 7023, 2, 4), -- 2x Quantum Chips
(2048, 7022, 2, 3), -- 2x Nano-Processors
-- Titan Frame (2049)
(2049, 6004, 4, 4), -- 4x Reactive Armor Plate
(2049, 6031, 3, 4), -- 3x Enhanced Servo
(2049, 5012, 2, 4); -- 2x Fusion Cell

-- ═════ CONSUMABLE RECIPES (30 recipes) ═════

-- Healing Consumables (10 recipes)
INSERT OR IGNORE INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, discovery_method, recipe_description)
VALUES
(3001, 'Basic Stim-Pack', 'Basic', 'Consumable', 'Laboratory', 0, 'Default', 'Restores 15 HP instantly.'),
(3002, 'Advanced Stim-Pack', 'Advanced', 'Consumable', 'Laboratory', 0, 'Merchant', 'Restores 30 HP instantly.'),
(3003, 'Combat Regeneration Serum', 'Expert', 'Consumable', 'Laboratory', 0, 'Quest', 'Restores 50 HP and grants regeneration for 3 turns.'),
(3004, 'Repair Kit', 'Basic', 'Consumable', 'Field_Station', 0, 'Default', 'Repairs 10 HP of equipment damage.'),
(3005, 'Medical Supplies', 'Basic', 'Consumable', 'Laboratory', 0, 'Default', 'Removes [Bleeding] status.'),
(3006, 'Antitoxin', 'Advanced', 'Consumable', 'Laboratory', 0, 'Merchant', 'Removes [Poisoned] status.'),
(3007, 'Trauma Kit', 'Advanced', 'Consumable', 'Laboratory', 0, 'Merchant', 'Restores 25 HP and stabilizes critical injuries.'),
(3008, 'Nano-Heal Injector', 'Expert', 'Consumable', 'Laboratory', 0, 'Quest', 'Restores 60 HP and grants immunity to status effects for 2 turns.'),
(3009, 'Emergency Stim', 'Basic', 'Consumable', 'Field_Station', 0, 'Default', 'Restores 20 HP, can be used while [Downed].'),
(3010, 'Resuscitation Serum', 'Expert', 'Consumable', 'Laboratory', 0, 'Loot', 'Revives [Downed] ally with 50% HP.');

-- Healing Consumable Components
INSERT OR IGNORE INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
-- Basic Stim-Pack (3001)
(3001, 7001, 2, 1), -- 2x Basic Chemicals
(3001, 7010, 1, 1), -- 1x Organic Matter
-- Advanced Stim-Pack (3002)
(3002, 7002, 2, 2), -- 2x Refined Compounds
(3002, 7011, 1, 2), -- 1x Adrenaline Extract
-- Combat Regeneration Serum (3003)
(3003, 7003, 2, 3), -- 2x Exotic Reagents
(3003, 7013, 1, 4), -- 1x Stem Cell Culture
-- Repair Kit (3004)
(3004, 5001, 1, 1), -- 1x Scrap Iron
(3004, 7020, 1, 1), -- 1x Salvaged Circuitry
-- Medical Supplies (3005)
(3005, 7001, 2, 1), -- 2x Basic Chemicals
(3005, 6040, 1, 1), -- 1x Basic Padding
-- Antitoxin (3006)
(3006, 7002, 2, 2), -- 2x Refined Compounds
(3006, 7012, 1, 3), -- 1x Enzyme Catalyst
-- Trauma Kit (3007)
(3007, 7002, 3, 2), -- 3x Refined Compounds
(3007, 7011, 1, 2), -- 1x Adrenaline Extract
(3007, 6041, 1, 2), -- 1x Shock-Absorbing Gel
-- Nano-Heal Injector (3008)
(3008, 7003, 2, 3), -- 2x Exotic Reagents
(3008, 7022, 1, 3), -- 1x Nano-Processors
(3008, 7013, 1, 4), -- 1x Stem Cell Culture
-- Emergency Stim (3009)
(3009, 7001, 2, 1), -- 2x Basic Chemicals
(3009, 7011, 1, 2), -- 1x Adrenaline Extract
-- Resuscitation Serum (3010)
(3010, 7003, 3, 3), -- 3x Exotic Reagents
(3010, 7013, 1, 4), -- 1x Stem Cell Culture
(3010, 7023, 1, 4); -- 1x Quantum Chips

-- Buff Consumables (10 recipes)
INSERT OR IGNORE INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, discovery_method, recipe_description)
VALUES
(3020, 'Combat Stimulant', 'Basic', 'Consumable', 'Laboratory', 0, 'Default', '+10% damage for 3 turns.'),
(3021, 'Focus Enhancer', 'Advanced', 'Consumable', 'Laboratory', 0, 'Merchant', '+15% accuracy for 5 turns.'),
(3022, 'Berserker Serum', 'Expert', 'Consumable', 'Laboratory', 0, 'Quest', '+30% damage, -10% defense for 4 turns.'),
(3023, 'Defensive Boost', 'Basic', 'Consumable', 'Laboratory', 0, 'Default', '+2 Defense for 3 turns.'),
(3024, 'Speed Enhancer', 'Advanced', 'Consumable', 'Laboratory', 0, 'Merchant', '+1 Action for 2 turns.'),
(3025, 'Energy Shield Booster', 'Advanced', 'Consumable', 'Laboratory', 0, 'Merchant', 'Grants 20 temporary HP for 4 turns.'),
(3026, 'Resistance Cocktail', 'Expert', 'Consumable', 'Laboratory', 0, 'Quest', '+25% resistance to all damage types for 3 turns.'),
(3027, 'Stamina Booster', 'Basic', 'Consumable', 'Field_Station', 0, 'Default', 'Restores 20 Stamina.'),
(3028, 'Overcharge Serum', 'Expert', 'Consumable', 'Laboratory', 0, 'Loot', 'Next ability deals double damage, costs no Stamina.'),
(3029, 'Titan Strength Elixir', 'Expert', 'Consumable', 'Laboratory', 0, 'Quest', '+4 MIGHT for 5 turns.');

-- Buff Consumable Components
INSERT OR IGNORE INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
-- Combat Stimulant (3020)
(3020, 7001, 2, 1), -- 2x Basic Chemicals
(3020, 7011, 1, 2), -- 1x Adrenaline Extract
-- Focus Enhancer (3021)
(3021, 7002, 2, 2), -- 2x Refined Compounds
(3021, 7022, 1, 3), -- 1x Nano-Processors
-- Berserker Serum (3022)
(3022, 7003, 2, 3), -- 2x Exotic Reagents
(3022, 7013, 1, 4), -- 1x Stem Cell Culture
(3022, 7011, 2, 2), -- 2x Adrenaline Extract
-- Defensive Boost (3023)
(3023, 7001, 2, 1), -- 2x Basic Chemicals
(3023, 6040, 1, 1), -- 1x Basic Padding
-- Speed Enhancer (3024)
(3024, 7002, 2, 2), -- 2x Refined Compounds
(3024, 7011, 2, 2), -- 2x Adrenaline Extract
-- Energy Shield Booster (3025)
(3025, 7002, 2, 2), -- 2x Refined Compounds
(3025, 7021, 1, 2), -- 1x Integrated Circuits
-- Resistance Cocktail (3026)
(3026, 7003, 3, 3), -- 3x Exotic Reagents
(3026, 7012, 1, 3), -- 1x Enzyme Catalyst
-- Stamina Booster (3027)
(3027, 7001, 2, 1), -- 2x Basic Chemicals
(3027, 7010, 1, 1), -- 1x Organic Matter
-- Overcharge Serum (3028)
(3028, 7003, 2, 3), -- 2x Exotic Reagents
(3028, 7023, 1, 4), -- 1x Quantum Chips
(3028, 5012, 1, 4), -- 1x Fusion Cell
-- Titan Strength Elixir (3029)
(3029, 7003, 3, 3), -- 3x Exotic Reagents
(3029, 7013, 1, 4), -- 1x Stem Cell Culture
(3029, 7011, 2, 2); -- 2x Adrenaline Extract

-- Utility Consumables (10 recipes)
INSERT OR IGNORE INTO Crafting_Recipes (recipe_id, recipe_name, recipe_tier, crafted_item_type, required_station, quality_bonus, discovery_method, recipe_description)
VALUES
(3040, 'EMP Grenade', 'Advanced', 'Consumable', 'Laboratory', 0, 'Merchant', 'Disables electronic systems in area.'),
(3041, 'Smoke Charge', 'Basic', 'Consumable', 'Laboratory', 0, 'Default', 'Creates smoke cloud, granting concealment.'),
(3042, 'Signal Flare', 'Basic', 'Consumable', 'Field_Station', 0, 'Default', 'Calls reinforcements or marks location.'),
(3043, 'Flashbang', 'Basic', 'Consumable', 'Laboratory', 0, 'Default', 'Stuns enemies in area for 1 turn.'),
(3044, 'Frag Grenade', 'Advanced', 'Consumable', 'Workshop', 0, 'Merchant', 'Deals 3d6 damage in area.'),
(3045, 'Plasma Grenade', 'Expert', 'Consumable', 'Laboratory', 0, 'Quest', 'Deals 4d8 fire damage in area.'),
(3046, 'Cryo Grenade', 'Advanced', 'Consumable', 'Laboratory', 0, 'Merchant', 'Freezes enemies in area for 2 turns.'),
(3047, 'Acid Vial', 'Advanced', 'Consumable', 'Laboratory', 0, 'Merchant', 'Deals damage over time, reduces armor.'),
(3048, 'Quantum Mine', 'Expert', 'Consumable', 'Laboratory', 0, 'Loot', 'Creates unstable quantum field trap.'),
(3049, 'Nanite Swarm', 'Expert', 'Consumable', 'Laboratory', 0, 'Quest', 'Deploys repair nanites that heal allies in area.');

-- Utility Consumable Components
INSERT OR IGNORE INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality)
VALUES
-- EMP Grenade (3040)
(3040, 7021, 2, 2), -- 2x Integrated Circuits
(3040, 7001, 1, 1), -- 1x Basic Chemicals
-- Smoke Charge (3041)
(3041, 7001, 3, 1), -- 3x Basic Chemicals
-- Signal Flare (3042)
(3042, 7001, 1, 1), -- 1x Basic Chemicals
(3042, 7020, 1, 1), -- 1x Salvaged Circuitry
-- Flashbang (3043)
(3043, 7001, 2, 1), -- 2x Basic Chemicals
(3043, 7020, 1, 1), -- 1x Salvaged Circuitry
-- Frag Grenade (3044)
(3044, 5002, 1, 2), -- 1x Steel Ingot
(3044, 7002, 2, 2), -- 2x Refined Compounds
-- Plasma Grenade (3045)
(3045, 5011, 1, 3), -- 1x Plasma Core
(3045, 7003, 2, 3), -- 2x Exotic Reagents
-- Cryo Grenade (3046)
(3046, 7002, 2, 2), -- 2x Refined Compounds
(3046, 7003, 1, 3), -- 1x Exotic Reagents
-- Acid Vial (3047)
(3047, 7002, 3, 2), -- 3x Refined Compounds
-- Quantum Mine (3048)
(3048, 7023, 1, 4), -- 1x Quantum Chips
(3048, 7003, 2, 3), -- 2x Exotic Reagents
-- Nanite Swarm (3049)
(3049, 7022, 2, 3), -- 2x Nano-Processors
(3049, 7013, 1, 4); -- 1x Stem Cell Culture

-- ═══════════════════════════════════════════════════════════════════
-- SEED DATA: Runic Inscriptions (20+ definitions)
-- ═══════════════════════════════════════════════════════════════════

INSERT OR IGNORE INTO Runic_Inscriptions (inscription_id, inscription_name, inscription_tier, target_equipment_type, effect_type, effect_value, is_temporary, uses_if_temporary, component_requirements, crafting_cost_credits, inscription_description)
VALUES
-- Weapon Inscriptions (Elemental)
(8001, 'Rune of Flame', 3, 'Weapon', 'Elemental', '{"element": "Fire", "bonus_damage": 5, "burn_chance": 0.15}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9020, "quantity": 2, "min_quality": 2}]', 150, 'Temporary rune adding fire damage and burn chance.'),
(8002, 'Rune of Frost', 3, 'Weapon', 'Elemental', '{"element": "Ice", "bonus_damage": 5, "slow_chance": 0.20}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9020, "quantity": 2, "min_quality": 2}]', 150, 'Temporary rune adding ice damage and slow chance.'),
(8003, 'Rune of Lightning', 3, 'Weapon', 'Elemental', '{"element": "Lightning", "bonus_damage": 5, "chain_chance": 0.10}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9020, "quantity": 2, "min_quality": 2}]', 150, 'Temporary rune adding lightning damage with chain chance.'),
(8004, 'Rune of Poison', 3, 'Weapon', 'Elemental', '{"element": "Poison", "bonus_damage": 3, "poison_chance": 0.25}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9020, "quantity": 2, "min_quality": 2}]', 150, 'Temporary rune adding poison damage and status chance.'),

-- Weapon Inscriptions (Damage Bonus)
(8005, 'Rune of Sharpness', 4, 'Weapon', 'Damage_Bonus', '{"stat": "damage", "value": 10}', 0, NULL, '[{"item_id": 9002, "quantity": 1, "min_quality": 4}, {"item_id": 9021, "quantity": 2, "min_quality": 3}]', 500, 'Permanent +10 damage inscription.'),
(8006, 'Rune of Power', 4, 'Weapon', 'Damage_Bonus', '{"stat": "damage", "value": 8, "critical_bonus": 5}', 0, NULL, '[{"item_id": 9002, "quantity": 1, "min_quality": 4}, {"item_id": 9021, "quantity": 2, "min_quality": 3}]', 500, 'Permanent +8 damage, +5 on crits.'),
(8007, 'Rune of Precision', 4, 'Weapon', 'Damage_Bonus', '{"stat": "accuracy", "value": 2}', 0, NULL, '[{"item_id": 9002, "quantity": 1, "min_quality": 4}, {"item_id": 9021, "quantity": 1, "min_quality": 3}]', 400, 'Permanent +2 accuracy.'),

-- Weapon Inscriptions (Status Effects)
(8008, 'Rune of Bleeding', 3, 'Weapon', 'Status', '{"status": "Bleeding", "application_chance": 0.25, "duration": 3}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9010, "quantity": 1, "min_quality": 2}]', 180, 'Temporary rune with 25% chance to apply Bleeding.'),
(8009, 'Rune of Weakness', 3, 'Weapon', 'Status', '{"status": "Weakened", "application_chance": 0.20, "duration": 2}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9010, "quantity": 1, "min_quality": 2}]', 180, 'Temporary rune with 20% chance to apply Weakened.'),
(8010, 'Rune of Stunning', 4, 'Weapon', 'Status', '{"status": "Stunned", "application_chance": 0.15, "duration": 1}', 1, 8, '[{"item_id": 9002, "quantity": 1, "min_quality": 4}, {"item_id": 9011, "quantity": 1, "min_quality": 3}]', 250, 'Temporary rune with 15% chance to stun.'),

-- Armor Inscriptions (Resistance)
(8011, 'Rune of Fire Resistance', 3, 'Armor', 'Resistance', '{"resistance_type": "Fire", "value": 15}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9020, "quantity": 2, "min_quality": 2}]', 150, 'Temporary +15% fire resistance.'),
(8012, 'Rune of Ice Resistance', 3, 'Armor', 'Resistance', '{"resistance_type": "Ice", "value": 15}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9020, "quantity": 2, "min_quality": 2}]', 150, 'Temporary +15% ice resistance.'),
(8013, 'Rune of Energy Resistance', 3, 'Armor', 'Resistance', '{"resistance_type": "Energy", "value": 15}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9020, "quantity": 2, "min_quality": 2}]', 150, 'Temporary +15% energy resistance.'),
(8014, 'Rune of Physical Resistance', 3, 'Armor', 'Resistance', '{"resistance_type": "Physical", "value": 10}', 1, 10, '[{"item_id": 9001, "quantity": 1, "min_quality": 3}, {"item_id": 9020, "quantity": 2, "min_quality": 2}]', 150, 'Temporary +10% physical resistance.'),

-- Armor Inscriptions (Defense Bonus)
(8015, 'Rune of Fortification', 4, 'Armor', 'Damage_Bonus', '{"stat": "mitigation", "value": 5}', 0, NULL, '[{"item_id": 9002, "quantity": 1, "min_quality": 4}, {"item_id": 9021, "quantity": 2, "min_quality": 3}]', 500, 'Permanent +5 mitigation inscription.'),
(8016, 'Rune of Warding', 4, 'Armor', 'Damage_Bonus', '{"stat": "defense", "value": 3}', 0, NULL, '[{"item_id": 9002, "quantity": 1, "min_quality": 4}, {"item_id": 9021, "quantity": 2, "min_quality": 3}]', 450, 'Permanent +3 defense.'),

-- Armor Inscriptions (Special Effects)
(8017, 'Rune of Regeneration', 4, 'Armor', 'Special', '{"effect": "regeneration", "hp_per_turn": 3}', 1, 10, '[{"item_id": 9002, "quantity": 1, "min_quality": 4}, {"item_id": 9011, "quantity": 1, "min_quality": 3}]', 250, 'Temporary rune granting 3 HP regeneration per turn.'),
(8018, 'Rune of Vitality', 4, 'Armor', 'Special', '{"effect": "max_hp_bonus", "value": 20}', 0, NULL, '[{"item_id": 9002, "quantity": 1, "min_quality": 4}, {"item_id": 9021, "quantity": 2, "min_quality": 3}]', 500, 'Permanent +20 max HP.'),
(8019, 'Rune of Reflection', 4, 'Armor', 'Special', '{"effect": "damage_reflection", "percentage": 10}', 1, 8, '[{"item_id": 9002, "quantity": 1, "min_quality": 4}, {"item_id": 9011, "quantity": 1, "min_quality": 3}]', 280, 'Temporary rune reflecting 10% of damage back.'),

-- Universal Inscriptions
(8020, 'Rune of the Void', 5, 'Both', 'Special', '{"effect": "void_touched", "damage_bonus": 15, "corruption_cost": 2}', 0, NULL, '[{"item_id": 9003, "quantity": 1, "min_quality": 5}, {"item_id": 9022, "quantity": 2, "min_quality": 4}]', 1000, 'Permanent inscription granting immense power at corruption cost.'),
(8021, 'Rune of Aetheric Mastery', 5, 'Both', 'Special', '{"effect": "aetheric_boost", "ability_cost_reduction": 2, "damage_increase": 10}', 0, NULL, '[{"item_id": 9003, "quantity": 1, "min_quality": 5}, {"item_id": 9022, "quantity": 2, "min_quality": 4}]', 1000, 'Permanent inscription enhancing Aetheric abilities.'),
(8022, 'Rune of the Titan', 5, 'Both', 'Special', '{"effect": "titan_strength", "might_bonus": 3, "hp_bonus": 30}', 0, NULL, '[{"item_id": 9003, "quantity": 1, "min_quality": 5}, {"item_id": 9022, "quantity": 2, "min_quality": 4}]', 1000, 'Permanent inscription of overwhelming might.');

-- ═══════════════════════════════════════════════════════════════════
-- SEED DATA: Crafting Stations (10+ stations)
-- ═══════════════════════════════════════════════════════════════════

INSERT OR IGNORE INTO Crafting_Stations (station_id, station_name, station_type, max_quality_tier, location_world_id, location_sector_id, location_room_id, is_portable, station_description)
VALUES
-- Trunk (Midgard) Stations
(1, 'Central Forge', 'Forge', 3, 1, 1, 'safe_zone_forge', 0, 'Primary forge in Midgard central hub.'),
(2, 'Engineering Workshop', 'Workshop', 4, 1, 1, 'safe_zone_workshop', 0, 'Advanced fabrication facility in Midgard.'),
(3, 'Research Laboratory', 'Laboratory', 4, 1, 1, 'safe_zone_lab', 0, 'Chemical and biological synthesis lab.'),

-- Muspelheim Stations
(10, 'Forgemaster''s Hall', 'Forge', 3, 1, 2, 'muspel_forge_01', 0, 'Ancient forge repurposed for weapon crafting.'),
(11, 'Heat Treatment Bay', 'Workshop', 4, 1, 2, 'muspel_workshop_01', 0, 'Uses geothermal heat for advanced metallurgy.'),

-- Niflheim Stations
(20, 'Cryogenic Fabricator', 'Laboratory', 4, 1, 3, 'nifl_lab_01', 0, 'Low-temperature synthesis facility.'),
(21, 'Ice Forge', 'Forge', 3, 1, 3, 'nifl_forge_01', 0, 'Forge utilizing extreme cold for metal treatment.'),

-- Alfheim Stations
(30, 'Aetheric Weaving Chamber', 'Runic_Altar', 5, 1, 4, 'alf_altar_01', 0, 'Rare altar for runic inscriptions.'),
(31, 'Photonic Workshop', 'Workshop', 4, 1, 4, 'alf_workshop_01', 0, 'Light-based fabrication systems.'),
(32, 'Quantum Laboratory', 'Laboratory', 4, 1, 4, 'alf_lab_01', 0, 'Advanced laboratory with quantum systems.'),

-- Jotunheim Stations
(40, 'Titan''s Anvil', 'Forge', 3, 1, 5, 'jot_forge_01', 0, 'Massive forge capable of large-scale projects.'),
(41, 'Assembly Bay', 'Workshop', 4, 1, 5, 'jot_workshop_01', 0, 'Industrial-scale fabrication facility.'),
(42, 'Jotun-Reader Archive Workshop', 'Laboratory', 4, 1, 5, 'jot_lab_01', 0, 'Laboratory with recovered Jotun-Reader technology.'),

-- Portable Station (Einbui)
(100, 'Field Crafting Station', 'Field_Station', 2, 1, NULL, NULL, 1, 'Portable crafting station usable anywhere (Einbui ability).');

COMMIT;
