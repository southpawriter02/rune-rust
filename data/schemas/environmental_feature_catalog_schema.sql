-- =====================================================
-- v0.38.2: Environmental Feature Catalog - Base Templates
-- =====================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Define 10+ base environmental feature templates
--          for Dynamic Room Engine (v0.11) population system
-- Categories: Static Terrain (Cover, Obstacles, Elevation)
--            Dynamic Hazards (Periodic, Proximity, Triggered)
-- =====================================================

-- =====================================================
-- STATIC TERRAIN TEMPLATES
-- =====================================================

-- Template 1: Pillar_Base (Heavy Cover)
-- Category: Feature | Archetype: Cover
-- Tags: Structure, Cover, Destructible
-- Use Case: Tactical positioning, line-of-sight blocking
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
    'Pillar_Base',
    'Feature',
    'Cover',
    '{
      "hp": 50,
      "soak": 8,
      "cover_quality": "Heavy",
      "cover_bonus": -4,
      "blocks_los": true,
      "destructible": true,
      "tiles_occupied": 1
    }',
    '{Modifier} Support Pillar',
    'A {Modifier_Adj} pillar {Modifier_Detail}. It provides heavy cover.',
    '["Structure", "Cover", "Destructible"]',
    'Heavy cover provides -4 dice to hit, blocks line of sight'
);

-- Template 2: Chasm_Base (Impassable Obstacle)
-- Category: Feature | Archetype: Obstacle
-- Tags: Impassable, Dangerous, Environmental
-- Use Case: Tactical dividers, creates multi-zone combat arenas
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
    'Chasm_Base',
    'Feature',
    'Obstacle',
    '{
      "impassable": true,
      "fall_damage": "6d6",
      "damage_type": "Physical",
      "requires_acrobatics": false,
      "tiles_width": 2,
      "tactical_divider": true
    }',
    '{Modifier} Chasm',
    'A {Modifier_Adj} chasm {Modifier_Detail}. Falling into it would be fatal.',
    '["Impassable", "Dangerous", "Environmental"]',
    'Modifiers add damage types: Lava_Filled (+8d6 Fire), Frozen (slippery edges), Void (Psychic damage)'
);

-- Template 3: Elevation_Base (Tactical Advantage)
-- Category: Feature | Archetype: Tactical
-- Tags: Elevation, Advantage, Tactical
-- Use Case: Ranged attack bonuses, strategic positioning
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
    'Elevation_Base',
    'Feature',
    'Tactical',
    '{
      "elevation_bonus": "+1d",
      "applies_to": "Ranged",
      "climb_cost": 3,
      "requires_check": false,
      "tiles_occupied": 4,
      "provides_cover": true
    }',
    '{Modifier} Platform',
    'A {Modifier_Adj} raised platform {Modifier_Detail}. It offers a tactical vantage point.',
    '["Elevation", "Advantage", "Tactical"]',
    'High ground grants +1d to ranged attacks, costs 3 movement to reach'
);

-- Template 4: Rubble_Pile_Base (Difficult Terrain)
-- Category: Feature | Archetype: Obstacle
-- Tags: Difficult_Terrain, Cover, Environmental
-- Use Case: Movement impediment, light cover
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
    'Rubble_Pile_Base',
    'Feature',
    'Obstacle',
    '{
      "movement_cost_modifier": 2,
      "cover_quality": "Light",
      "cover_bonus": -2,
      "blocks_los": false,
      "destructible": false,
      "tiles_occupied": 2
    }',
    '{Modifier} Rubble Pile',
    'A pile of {Modifier_Adj} rubble {Modifier_Detail}. Crossing it will slow movement.',
    '["Difficult_Terrain", "Cover", "Environmental"]',
    'Coherent Glitch: MUST spawn beneath Unstable_Ceiling hazards (environmental storytelling)'
);

-- Template 5: Crate_Stack_Base (Light Cover)
-- Category: Feature | Archetype: Cover
-- Tags: Cover, Destructible, Light
-- Use Case: Light tactical cover, destructible obstacles
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
    'Crate_Stack_Base',
    'Feature',
    'Cover',
    '{
      "hp": 25,
      "soak": 4,
      "cover_quality": "Light",
      "cover_bonus": -2,
      "blocks_los": false,
      "destructible": true,
      "tiles_occupied": 1
    }',
    '{Modifier} Crate Stack',
    'A stack of {Modifier_Adj} crates {Modifier_Detail}. They provide light cover.',
    '["Cover", "Destructible", "Light"]',
    'Light cover provides -2 dice to hit, does not block line of sight'
);

-- Template 6: Bulkhead_Base (Heavy Obstacle)
-- Category: Feature | Archetype: Obstacle
-- Tags: Impassable, Structure, Industrial
-- Use Case: Room dividers, chokepoints
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
    'Bulkhead_Base',
    'Feature',
    'Obstacle',
    '{
      "impassable": true,
      "hp": 100,
      "soak": 15,
      "destructible": true,
      "tiles_occupied": 1,
      "blocks_los": true
    }',
    '{Modifier} Bulkhead',
    'A {Modifier_Adj} bulkhead door {Modifier_Detail}. It completely blocks passage.',
    '["Impassable", "Structure", "Industrial"]',
    'Very high HP and soak, can be destroyed with sustained effort'
);

-- =====================================================
-- DYNAMIC HAZARD TEMPLATES
-- =====================================================

-- Template 7: Steam_Vent_Base (Periodic Hazard)
-- Category: Feature | Archetype: DynamicHazard
-- Tags: Hazard, Periodic, Area_Effect
-- Use Case: Timed area damage, predictable danger
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
    'Steam_Vent_Base',
    'Feature',
    'DynamicHazard',
    '{
      "damage": "2d6",
      "damage_type": "Fire",
      "activation_frequency": 3,
      "activation_type": "Periodic",
      "area_pattern": "3x3",
      "status_effect": null,
      "warning_turn": true
    }',
    '{Modifier} Steam Vent',
    'A fractured pipe vents {Modifier_Adj} steam periodically. It erupts every 3 turns.',
    '["Hazard", "Periodic", "Area_Effect"]',
    'Periodic hazards activate every N turns, warning turn allows tactical repositioning'
);

-- Template 8: Power_Conduit_Base (Proximity Hazard)
-- Category: Feature | Archetype: DynamicHazard
-- Tags: Hazard, Proximity, Electrical
-- Use Case: Area denial, movement restriction
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
    'Power_Conduit_Base',
    'Feature',
    'DynamicHazard',
    '{
      "damage": "3d6",
      "damage_type": "Lightning",
      "activation_range": 2,
      "activation_type": "Proximity",
      "status_effect": ["Stunned", 1],
      "enhanced_by": ["Flooded"]
    }',
    'Live Power Conduit',
    'An exposed power conduit arcs with {Modifier_Adj} electricity. Approach with caution.',
    '["Hazard", "Proximity", "Electrical"]',
    'Coherent Glitch: If room has [Flooded] ambient condition, damage increases to 6d6, range increases to 4'
);

-- Template 9: Unstable_Ceiling_Base (Triggered Hazard)
-- Category: Feature | Archetype: DynamicHazard
-- Tags: Hazard, Triggered, One_Time, Area_Effect
-- Use Case: One-time event, changes battlefield terrain
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
    'Unstable_Ceiling_Base',
    'Feature',
    'DynamicHazard',
    '{
      "damage": "4d6",
      "damage_type": "Physical",
      "triggers": ["Explosion", "Heavy_Attack", "Loud_Action"],
      "area_pattern": "All_Combatants",
      "one_time": true,
      "creates_terrain": "Rubble_Pile"
    }',
    'Unstable Ceiling',
    'The {Modifier_Adj} ceiling shows dangerous signs of instability. Loud actions may trigger a collapse.',
    '["Hazard", "Triggered", "One_Time", "Area_Effect"]',
    'Coherent Glitch: When activated, deals 4d6 to all combatants, creates Rubble_Pile terrain, hazard destroyed'
);

-- Template 10: Burning_Ground_Base (Persistent Hazard)
-- Category: Feature | Archetype: DynamicHazard
-- Tags: Hazard, Persistent, Fire
-- Use Case: Area denial, continuous damage zones
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
    'Burning_Ground_Base',
    'Feature',
    'DynamicHazard',
    '{
      "damage": "2d6",
      "damage_type": "Fire",
      "activation_timing": "End_Of_Turn",
      "tiles_affected": 4,
      "status_effect": ["Burning", 2],
      "spread_chance": 0.1
    }',
    '{Modifier} Burning Ground',
    'The ground here burns with {Modifier_Adj} flames. Standing in it deals fire damage.',
    '["Hazard", "Persistent", "Fire"]',
    'Deals damage at end of turn to anyone standing in affected tiles, may spread to adjacent tiles (10% chance)'
);

-- Template 11: Toxic_Haze_Base (Area Hazard)
-- Category: Feature | Archetype: DynamicHazard
-- Tags: Hazard, Persistent, Poison, Area_Effect
-- Use Case: Room-wide debuff, accuracy penalty
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
    'Toxic_Haze_Base',
    'Feature',
    'DynamicHazard',
    '{
      "damage": "1d4",
      "damage_type": "Poison",
      "activation_timing": "Start_Of_Turn",
      "area_pattern": "Room_Wide",
      "status_effect": ["Poisoned", 0.25],
      "accuracy_penalty": -1
    }',
    '{Modifier} Toxic Haze',
    'A {Modifier_Adj} haze fills the air. Breathing it causes damage and impairs accuracy.',
    '["Hazard", "Persistent", "Poison", "Area_Effect"]',
    'Room-wide effect, deals damage at start of turn, 25% chance to inflict [Poisoned], -1 accuracy penalty'
);

-- Template 12: Electrified_Floor_Base (Movement Hazard)
-- Category: Feature | Archetype: DynamicHazard
-- Tags: Hazard, Movement_Triggered, Electrical
-- Use Case: Movement punishment, tactical positioning
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
    'Electrified_Floor_Base',
    'Feature',
    'DynamicHazard',
    '{
      "damage": "3d6",
      "damage_type": "Lightning",
      "activation_type": "Movement",
      "tiles_affected": 6,
      "status_effect": ["Stunned", 0.2]
    }',
    'Electrified Floor',
    'The {Modifier_Adj} floor pulses with electrical current. Moving across it triggers shocks.',
    '["Hazard", "Movement_Triggered", "Electrical"]',
    'Activates when characters move across affected tiles, 20% chance to inflict [Stunned]'
);

-- Template 13: Radiation_Zone_Base (Persistent Hazard)
-- Category: Feature | Archetype: DynamicHazard
-- Tags: Hazard, Persistent, Radiation, Debuff
-- Use Case: Area denial, long-term debuff
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
    'Radiation_Zone_Base',
    'Feature',
    'DynamicHazard',
    '{
      "damage": "1d6",
      "damage_type": "Poison",
      "activation_timing": "End_Of_Turn",
      "tiles_affected": 9,
      "status_effect": ["Irradiated", 1],
      "stacks": true
    }',
    'Radiation Zone',
    'A {Modifier_Adj} zone pulses with dangerous radiation. Prolonged exposure is lethal.',
    '["Hazard", "Persistent", "Radiation", "Debuff"]',
    '[Irradiated] stacks with duration, increasing damage over time'
);

-- =====================================================
-- SCHEMA VERSION TRACKING
-- =====================================================

INSERT OR IGNORE INTO Schema_Migrations (version, description, applied_at)
VALUES (
    'v0.38.2',
    'Environmental Feature Catalog: 13 base templates (6 static terrain, 7 dynamic hazards)',
    CURRENT_TIMESTAMP
);

-- =====================================================
-- END v0.38.2 Schema
-- =====================================================
