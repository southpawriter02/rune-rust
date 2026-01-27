-- ============================================================
-- v0.38.5: Resource Node Base Templates
-- 6+ base templates for procedural resource generation
-- ============================================================

-- ============================================================
-- TEMPLATE 1: Ore Vein Base (MineralVein)
-- ============================================================
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template,
    tags, notes
) VALUES (
    'Ore_Vein_Base', 'Resource', 'MineralVein',
    '{"extraction_type": "Mining", "extraction_dc": 12, "extraction_time": 2, "yield_min": 2, "yield_max": 4, "depletes": true, "uses": 3, "requires_tool": false}',
    '{Modifier} {Resource_Type} Vein',
    'A {Modifier_Adj} vein of {Resource_Type} {Modifier_Detail}. [Mining DC 12]',
    '["Mining", "Metal", "Industrial"]',
    'Common ore deposits. Yields 2-4 units over 3 extractions. DC 12 MIGHT check or Mining Tool.'
);

-- ============================================================
-- TEMPLATE 2: Salvage Wreckage Base (SalvageWreckage)
-- ============================================================
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template,
    tags, notes
) VALUES (
    'Salvage_Wreckage_Base', 'Resource', 'SalvageWreckage',
    '{"extraction_type": "Salvaging", "extraction_dc": 15, "extraction_time": 3, "yield_min": 1, "yield_max": 3, "depletes": true, "uses": 2, "trap_chance": 0.1, "requires_tool": false}',
    '{Modifier} {Wreckage_Type}',
    'The wreckage of {Article} {Modifier_Adj} {Wreckage_Type}. {Modifier_Detail}. [Salvage DC 15]',
    '["Salvage", "Mechanical", "Junk"]',
    'Salvageable mechanical wreckage. Yields 1-3 components over 2 extractions. 10% trap chance.'
);

-- ============================================================
-- TEMPLATE 3: Fungal Growth Base (OrganicHarvest)
-- ============================================================
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template,
    tags, notes
) VALUES (
    'Fungal_Growth_Base', 'Resource', 'OrganicHarvest',
    '{"extraction_type": "Harvesting", "extraction_dc": 10, "extraction_time": 1, "yield_min": 1, "yield_max": 2, "depletes": true, "uses": 2, "poisonous": false, "requires_tool": false}',
    '{Modifier} {Fungus_Type}',
    '{Article_Cap} {Modifier_Adj} {Fungus_Type} grows here. {Modifier_Detail}.',
    '["Harvest", "Organic", "Alchemy"]',
    'Harvestable fungal growth. Yields 1-2 units over 2 harvests. No tools required.'
);

-- ============================================================
-- TEMPLATE 4: Chemical Deposit Base (OrganicHarvest)
-- ============================================================
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template,
    tags, notes
) VALUES (
    'Chemical_Deposit_Base', 'Resource', 'OrganicHarvest',
    '{"extraction_type": "Harvesting", "extraction_dc": 14, "extraction_time": 2, "yield_min": 1, "yield_max": 1, "depletes": true, "uses": 1, "hazardous": true, "requires_tool": false}',
    '{Chemical_Type} Deposit',
    'A deposit of {Chemical_Type} {Modifier_Detail}. Handle with care. [Harvest DC 14]',
    '["Harvest", "Chemical", "Hazardous"]',
    'Hazardous chemical deposit. Single use, 1 unit yield. Extraction carries risk.'
);

-- ============================================================
-- TEMPLATE 5: Aetheric Anomaly Base (AethericAnomaly)
-- ============================================================
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template,
    tags, notes
) VALUES (
    'Aetheric_Anomaly_Base', 'Resource', 'AethericAnomaly',
    '{"extraction_type": "Siphoning", "extraction_dc": 18, "extraction_time": 2, "yield_min": 1, "yield_max": 1, "depletes": true, "uses": 1, "unstable": true, "requires_galdr": true, "requires_tool": true}',
    '{Anomaly_Type}',
    '{Article_Cap} {Anomaly_Type} pulses with unstable Aetheric energy. {Modifier_Detail}. [Siphon DC 18, Galdr required]',
    '["Siphon", "Aetheric", "Magical", "Unstable"]',
    'Aetheric resource node. Requires Galdr-caster or Aether Siphon. Unstable, single use.'
);

-- ============================================================
-- TEMPLATE 6: Ancient Cache Base (SalvageWreckage)
-- ============================================================
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template,
    tags, notes
) VALUES (
    'Ancient_Cache_Base', 'Resource', 'SalvageWreckage',
    '{"extraction_type": "Search", "extraction_dc": 16, "extraction_time": 2, "yield_quality": "Rare", "depletes": true, "uses": 1, "hidden": true, "trap_chance": 0.3, "requires_tool": false}',
    '{Culture} Supply Cache',
    'A hidden {Culture} supply cache {Modifier_Detail}. [Hidden, WITS DC 18 to detect]',
    '["Cache", "Hidden", "Valuable", "Trapped"]',
    'Hidden supply cache. Must be detected (WITS DC 18) before search. 30% trap chance. Rare quality loot.'
);

-- ============================================================
-- TEMPLATE 7: Crystal Formation Base (MineralVein)
-- ============================================================
INSERT OR IGNORE INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template,
    tags, notes
) VALUES (
    'Crystal_Formation_Base', 'Resource', 'MineralVein',
    '{"extraction_type": "Mining", "extraction_dc": 14, "extraction_time": 2, "yield_min": 1, "yield_max": 2, "depletes": true, "uses": 2, "fragile": true, "requires_tool": false}',
    '{Modifier} {Crystal_Type} Formation',
    '{Article_Cap} {Modifier_Adj} formation of {Crystal_Type} crystals. {Modifier_Detail}. [Mining DC 14, fragile]',
    '["Mining", "Crystal", "Fragile", "Valuable"]',
    'Crystalline resource node. Fragile - failed extraction may shatter crystals. Yields 1-2 units.'
);

-- ============================================================
-- Total: 7 base templates covering all 4 resource node types
-- MineralVein: Ore_Vein_Base, Crystal_Formation_Base
-- SalvageWreckage: Salvage_Wreckage_Base, Ancient_Cache_Base
-- OrganicHarvest: Fungal_Growth_Base, Chemical_Deposit_Base
-- AethericAnomaly: Aetheric_Anomaly_Base
-- ============================================================
