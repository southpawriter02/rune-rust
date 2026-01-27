-- =====================================================
-- v0.38.2: Environmental Feature Catalog - Hazard Modifiers
-- =====================================================
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Define hazard-specific thematic modifiers
--          for environmental feature composition
-- New Modifiers: Lava_Filled, Geothermal, Void
-- Reuse: Scorched, Frozen, Rusted, Crystalline, Monolithic (from v0.38.1)
-- =====================================================

-- =====================================================
-- HAZARD-SPECIFIC MODIFIERS
-- =====================================================

-- Modifier 1: Lava_Filled (Muspelheim-specific)
-- Primary Application: Chasm_Base
-- Effect: Adds massive fire damage to base fall damage
-- Visual: Flowing molten rock, red-orange glow
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name,
    primary_biome,
    adjective,
    detail_fragment,
    stat_modifiers,
    status_effects,
    color_palette,
    ambient_sounds,
    particle_effects,
    notes
) VALUES (
    'Lava_Filled',
    'Muspelheim',
    'lava-filled',
    'flows with molten rock that glows red-orange',
    '{
      "fall_damage_bonus": "8d6",
      "damage_type": "Fire",
      "ambient_heat_range": 2,
      "ambient_heat_damage": "1d4"
    }',
    '[["Burning", 2]]',
    '["red", "orange", "black", "yellow"]',
    '["low rumble of flowing lava", "crackling heat", "hissing rock"]',
    '["ember_drift", "heat_shimmer", "lava_glow"]',
    'Composite: Chasm_Base + Lava_Filled = Lava River (6d6 Physical + 8d6 Fire fall damage, ambient heat 1d4 within 2 tiles)'
);

-- Modifier 2: Geothermal (The Roots-specific)
-- Primary Application: Steam_Vent_Base
-- Effect: Superheated steam from industrial failures
-- Visual: Hissing pipes, condensation, rust stains
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name,
    primary_biome,
    adjective,
    detail_fragment,
    stat_modifiers,
    status_effects,
    color_palette,
    ambient_sounds,
    particle_effects,
    notes
) VALUES (
    'Geothermal',
    'The_Roots',
    'geothermal',
    'vents superheated steam from fractured pipes',
    '{}',
    '[]',
    '["gray", "white", "orange", "brown"]',
    '["hissing steam", "groaning pipes", "dripping condensation"]',
    '["steam_jet", "condensation_drip"]',
    'Composite: Steam_Vent_Base + Geothermal = Geothermal Steam Vent (2d6 Fire, periodic activation)'
);

-- Modifier 3: Void (Alfheim-specific)
-- Primary Application: Chasm_Base, Reality-warping features
-- Effect: Reality tears, psychic damage, spatial instability
-- Visual: Flickering void, non-Euclidean geometry, Aether corruption
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name,
    primary_biome,
    adjective,
    detail_fragment,
    stat_modifiers,
    status_effects,
    color_palette,
    ambient_sounds,
    particle_effects,
    notes
) VALUES (
    'Void',
    'Alfheim',
    'reality-torn',
    'flickers with unstable void energy that defies comprehension',
    '{
      "fall_damage": "6d6",
      "damage_type": "Psychic",
      "proximity_stress": 2,
      "proximity_range": 3,
      "unstable": true
    }',
    '[["Stressed", 1]]',
    '["purple", "black", "silver", "flickering"]',
    '["reality tear hum", "whispered nothingness", "spatial distortion"]',
    '["void_flicker", "reality_tear", "aether_corruption"]',
    'Composite: Chasm_Base + Void = Reality Tear (6d6 Psychic fall damage, +2 Psychic Stress per turn within 3 tiles, may shift position)'
);

-- Modifier 4: Corrupted (Cross-biome)
-- Primary Application: Toxic_Haze_Base, Radiation_Zone_Base
-- Effect: Aether corruption, enhanced poison effects
-- Visual: Sickly green-purple glow, unstable particles
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name,
    primary_biome,
    adjective,
    detail_fragment,
    stat_modifiers,
    status_effects,
    color_palette,
    ambient_sounds,
    particle_effects,
    notes
) VALUES (
    'Corrupted',
    'Alfheim',
    'corrupted',
    'pulses with sickly Aether corruption',
    '{
      "damage_multiplier": 1.5,
      "status_effect_chance_bonus": 0.25
    }',
    '[["Poisoned", 1], ["Stressed", 1]]',
    '["green", "purple", "black"]',
    '["crackling corruption", "whispered madness"]',
    '["corruption_motes", "unstable_aether"]',
    'Increases poison damage by 50%, enhances status effect application chance by 25%'
);

-- Modifier 5: Industrial_Decay (The Roots-specific)
-- Primary Application: All static terrain
-- Effect: Weakened structures, rust, corrosion
-- Visual: Orange-brown rust, metal fatigue, water damage
INSERT OR IGNORE INTO Descriptor_Thematic_Modifiers (
    modifier_name,
    primary_biome,
    adjective,
    detail_fragment,
    stat_modifiers,
    status_effects,
    color_palette,
    ambient_sounds,
    particle_effects,
    notes
) VALUES (
    'Industrial_Decay',
    'The_Roots',
    'rusted and corroded',
    'shows extensive industrial decay and water damage',
    '{
      "hp_multiplier": 0.6,
      "soak_multiplier": 0.7
    }',
    '[]',
    '["orange", "brown", "gray", "black"]',
    '["creaking metal", "dripping water", "structural groans"]',
    '["rust_flakes", "water_drips"]',
    'Reduces HP by 40%, reduces Soak by 30% (structural weakness from decay)'
);

-- =====================================================
-- COMPOSITE FEATURE EXAMPLES
-- =====================================================
-- These are pre-generated composites for common biome features
-- The DescriptorService can also generate these on-the-fly

-- Composite 1: Lava River (Chasm_Base + Lava_Filled)
-- Muspelheim signature hazard
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id,
    modifier_id,
    final_name,
    final_description,
    final_mechanics,
    biome_restrictions,
    spawn_weight,
    spawn_rules,
    is_active
) VALUES (
    (SELECT template_id FROM Descriptor_Base_Templates WHERE template_name = 'Chasm_Base'),
    (SELECT modifier_id FROM Descriptor_Thematic_Modifiers WHERE modifier_name = 'Lava_Filled'),
    'Lava River',
    'A lava-filled chasm flows with molten rock that glows red-orange. Falling into it would be fatal.',
    '{
      "impassable": true,
      "fall_damage": "6d6 Physical + 8d6 Fire",
      "damage_type": "Physical+Fire",
      "tiles_width": 2,
      "tactical_divider": true,
      "ambient_heat_range": 2,
      "ambient_heat_damage": "1d4 Fire"
    }',
    '["Muspelheim"]',
    1.5,
    '{"biome_required": "Muspelheim", "archetype_preference": "BossArena"}',
    1
);

-- Composite 2: Geothermal Steam Vent (Steam_Vent_Base + Geothermal)
-- The Roots industrial hazard
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id,
    modifier_id,
    final_name,
    final_description,
    final_mechanics,
    biome_restrictions,
    spawn_weight,
    spawn_rules,
    is_active
) VALUES (
    (SELECT template_id FROM Descriptor_Base_Templates WHERE template_name = 'Steam_Vent_Base'),
    (SELECT modifier_id FROM Descriptor_Thematic_Modifiers WHERE modifier_name = 'Geothermal'),
    'Geothermal Steam Vent',
    'A fractured pipe vents geothermal steam from fractured pipes. It erupts every 3 turns.',
    '{
      "damage": "2d6",
      "damage_type": "Fire",
      "activation_frequency": 3,
      "activation_type": "Periodic",
      "area_pattern": "3x3",
      "status_effect": null,
      "warning_turn": true
    }',
    '["The_Roots"]',
    1.0,
    '{"biome_required": "The_Roots"}',
    1
);

-- Composite 3: Reality Tear (Chasm_Base + Void)
-- Alfheim Aether corruption hazard
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id,
    modifier_id,
    final_name,
    final_description,
    final_mechanics,
    biome_restrictions,
    spawn_weight,
    spawn_rules,
    is_active
) VALUES (
    (SELECT template_id FROM Descriptor_Base_Templates WHERE template_name = 'Chasm_Base'),
    (SELECT modifier_id FROM Descriptor_Thematic_Modifiers WHERE modifier_name = 'Void'),
    'Reality Tear',
    'A reality-torn chasm flickers with unstable void energy that defies comprehension. Falling into it would be fatal.',
    '{
      "impassable": true,
      "fall_damage": "6d6 Psychic",
      "damage_type": "Psychic",
      "tiles_width": 2,
      "tactical_divider": true,
      "proximity_stress": 2,
      "proximity_range": 3,
      "unstable": true,
      "may_shift": "Position may change between turns"
    }',
    '["Alfheim"]',
    1.2,
    '{"biome_required": "Alfheim", "archetype_preference": "Chamber"}',
    1
);

-- Composite 4: Scorched Support Pillar (Pillar_Base + Scorched)
-- Muspelheim cover
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id,
    modifier_id,
    final_name,
    final_description,
    final_mechanics,
    biome_restrictions,
    spawn_weight,
    spawn_rules,
    is_active
) VALUES (
    (SELECT template_id FROM Descriptor_Base_Templates WHERE template_name = 'Pillar_Base'),
    (SELECT modifier_id FROM Descriptor_Thematic_Modifiers WHERE modifier_name = 'Scorched'),
    'Scorched Support Pillar',
    'A scorched pillar radiates intense heat. It provides heavy cover.',
    '{
      "hp": 50,
      "soak": 8,
      "cover_quality": "Heavy",
      "cover_bonus": -4,
      "blocks_los": true,
      "destructible": true,
      "tiles_occupied": 1,
      "fire_resistance": -50
    }',
    '["Muspelheim"]',
    1.0,
    '{"biome_required": "Muspelheim"}',
    1
);

-- Composite 5: Frozen Chasm (Chasm_Base + Frozen)
-- Niflheim obstacle
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id,
    modifier_id,
    final_name,
    final_description,
    final_mechanics,
    biome_restrictions,
    spawn_weight,
    spawn_rules,
    is_active
) VALUES (
    (SELECT template_id FROM Descriptor_Base_Templates WHERE template_name = 'Chasm_Base'),
    (SELECT modifier_id FROM Descriptor_Thematic_Modifiers WHERE modifier_name = 'Frozen'),
    'Frozen Chasm',
    'An ice-covered chasm is encased in thick ice. Falling into it would be fatal.',
    '{
      "impassable": true,
      "fall_damage": "6d6 Physical + 2d6 Cold",
      "damage_type": "Physical+Cold",
      "tiles_width": 2,
      "tactical_divider": true,
      "slippery_edges": true,
      "edge_movement_penalty": 1
    }',
    '["Niflheim"]',
    1.0,
    '{"biome_required": "Niflheim"}',
    1
);

-- Composite 6: Corroded Rubble Pile (Rubble_Pile_Base + Industrial_Decay)
-- The Roots difficult terrain
INSERT OR IGNORE INTO Descriptor_Composites (
    base_template_id,
    modifier_id,
    final_name,
    final_description,
    final_mechanics,
    biome_restrictions,
    spawn_weight,
    spawn_rules,
    is_active
) VALUES (
    (SELECT template_id FROM Descriptor_Base_Templates WHERE template_name = 'Rubble_Pile_Base'),
    (SELECT modifier_id FROM Descriptor_Thematic_Modifiers WHERE modifier_name = 'Industrial_Decay'),
    'Corroded Rubble Pile',
    'A pile of rusted and corroded rubble shows extensive industrial decay and water damage. Crossing it will slow movement.',
    '{
      "movement_cost_modifier": 2,
      "cover_quality": "Light",
      "cover_bonus": -2,
      "blocks_los": false,
      "destructible": false,
      "tiles_occupied": 2
    }',
    '["The_Roots"]',
    1.0,
    '{"biome_required": "The_Roots"}',
    1
);

-- =====================================================
-- SCHEMA VERSION TRACKING
-- =====================================================

INSERT OR IGNORE INTO Schema_Migrations (version, description, applied_at)
VALUES (
    'v0.38.2b',
    'Hazard Modifiers: 5 new modifiers + 6 composite feature examples',
    CURRENT_TIMESTAMP
);

-- =====================================================
-- END v0.38.2 Hazard Modifiers
-- =====================================================
