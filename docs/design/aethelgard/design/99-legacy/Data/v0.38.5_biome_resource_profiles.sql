-- ============================================================
-- v0.38.5: Biome Resource Profiles
-- Defines resource distribution for each biome
-- ============================================================

-- Profile 1: The Roots
-- Industrial decay, fungal growth, basic salvage
INSERT OR IGNORE INTO Biome_Resource_Profiles (
    biome_name,
    common_resources,
    uncommon_resources,
    rare_resources,
    legendary_resources,
    spawn_density_small,
    spawn_density_medium,
    spawn_density_large,
    unique_resources,
    notes
) VALUES (
    'The_Roots',
    '[
        {"template": "Ore_Vein_Base", "resource": "Iron", "weight": 0.4},
        {"template": "Salvage_Wreckage_Base", "resource": "Servitor", "weight": 0.3},
        {"template": "Fungal_Growth_Base", "resource": "Luminous_Fungus", "weight": 0.2},
        {"template": "Chemical_Deposit_Base", "resource": "Volatile_Oil", "weight": 0.1}
    ]',
    '[
        {"template": "Ore_Vein_Base", "resource": "Star_Metal", "weight": 0.5},
        {"template": "Salvage_Wreckage_Base", "resource": "Power_Conduit", "weight": 0.3},
        {"template": "Chemical_Deposit_Base", "resource": "Caustic_Sludge", "weight": 0.2}
    ]',
    '[
        {"template": "Ancient_Cache_Base", "resource": "Dvergr_Cache", "weight": 0.6},
        {"template": "Aetheric_Anomaly_Base", "resource": "Runic_Eddy", "weight": 0.4}
    ]',
    NULL,
    0, 2, 3,
    '["Luminous_Fungus", "Runic_Eddy"]',
    'Industrial ruins with fungal growth. Medium spawn density. Common salvage opportunities.'
);

-- Profile 2: Muspelheim
-- Volcanic resources, fire-hardened metals, thermal hazards
INSERT OR IGNORE INTO Biome_Resource_Profiles (
    biome_name,
    common_resources,
    uncommon_resources,
    rare_resources,
    legendary_resources,
    spawn_density_small,
    spawn_density_medium,
    spawn_density_large,
    unique_resources,
    notes
) VALUES (
    'Muspelheim',
    '[
        {"template": "Ore_Vein_Base", "resource": "Obsidian", "weight": 0.5},
        {"template": "Fungal_Growth_Base", "resource": "Ember_Moss", "weight": 0.3},
        {"template": "Chemical_Deposit_Base", "resource": "Magma_Residue", "weight": 0.2}
    ]',
    '[
        {"template": "Ore_Vein_Base", "resource": "Star_Metal", "weight": 0.6},
        {"template": "Salvage_Wreckage_Base", "resource": "Forge_Equipment", "weight": 0.4}
    ]',
    '[
        {"template": "Ancient_Cache_Base", "resource": "Ancient_Forge_Cache", "weight": 0.7},
        {"template": "Crystal_Formation_Base", "resource": "Heart_of_Inferno", "weight": 0.3}
    ]',
    '[{"template": "Crystal_Formation_Base", "resource": "Heart_of_Inferno", "weight": 1.0}]',
    0, 1, 2,
    '["Obsidian", "Ember_Moss", "Magma_Residue", "Heart_of_Inferno"]',
    'Volcanic realm. Low spawn density due to environmental hazards. Fire-hardened resources.'
);

-- Profile 3: Niflheim
-- Frozen resources, preserved organics, cryo-tech
INSERT OR IGNORE INTO Biome_Resource_Profiles (
    biome_name,
    common_resources,
    uncommon_resources,
    rare_resources,
    legendary_resources,
    spawn_density_small,
    spawn_density_medium,
    spawn_density_large,
    unique_resources,
    notes
) VALUES (
    'Niflheim',
    '[
        {"template": "Fungal_Growth_Base", "resource": "Frost_Lichen", "weight": 0.4},
        {"template": "Chemical_Deposit_Base", "resource": "Cryogenic_Fluid", "weight": 0.3},
        {"template": "Crystal_Formation_Base", "resource": "Ice_Crystal", "weight": 0.3}
    ]',
    '[
        {"template": "Salvage_Wreckage_Base", "resource": "Cryo_System", "weight": 0.6},
        {"template": "Fungal_Growth_Base", "resource": "Frozen_Organics", "weight": 0.4}
    ]',
    '[
        {"template": "Crystal_Formation_Base", "resource": "Eternal_Ice_Shard", "weight": 0.6},
        {"template": "Ancient_Cache_Base", "resource": "Seidkona_Cache", "weight": 0.4}
    ]',
    '[{"template": "Crystal_Formation_Base", "resource": "Eternal_Ice_Shard", "weight": 1.0}]',
    0, 2, 3,
    '["Frost_Lichen", "Cryogenic_Fluid", "Eternal_Ice_Shard", "Frozen_Organics"]',
    'Frozen wasteland. Resources often encased in ice. Medium spawn density.'
);

-- Profile 4: Alfheim
-- Crystalline paradox, Aetheric resources, unstable materials
INSERT OR IGNORE INTO Biome_Resource_Profiles (
    biome_name,
    common_resources,
    uncommon_resources,
    rare_resources,
    legendary_resources,
    spawn_density_small,
    spawn_density_medium,
    spawn_density_large,
    unique_resources,
    notes
) VALUES (
    'Alfheim',
    '[
        {"template": "Fungal_Growth_Base", "resource": "Paradox_Spore", "weight": 0.4},
        {"template": "Crystal_Formation_Base", "resource": "Crystalline_Formation", "weight": 0.3},
        {"template": "Aetheric_Anomaly_Base", "resource": "Runic_Eddy", "weight": 0.3}
    ]',
    '[
        {"template": "Ore_Vein_Base", "resource": "Aetheric_Crystal", "weight": 0.6},
        {"template": "Aetheric_Anomaly_Base", "resource": "Reality_Fracture", "weight": 0.4}
    ]',
    '[
        {"template": "Aetheric_Anomaly_Base", "resource": "All_Rune_Fragment", "weight": 0.6},
        {"template": "Ancient_Cache_Base", "resource": "Pre_Blight_Cache", "weight": 0.4}
    ]',
    '[{"template": "Aetheric_Anomaly_Base", "resource": "All_Rune_Fragment", "weight": 1.0}]',
    1, 3, 4,
    '["Paradox_Spore", "Aetheric_Crystal", "Reality_Fracture", "All_Rune_Fragment"]',
    'Blight epicenter. High spawn density but resources are unstable/dangerous. Aetheric-rich.'
);

-- Profile 5: Jötunheim
-- Ancient tech, titanic-scale resources, industrial salvage
INSERT OR IGNORE INTO Biome_Resource_Profiles (
    biome_name,
    common_resources,
    uncommon_resources,
    rare_resources,
    legendary_resources,
    spawn_density_small,
    spawn_density_medium,
    spawn_density_large,
    unique_resources,
    notes
) VALUES (
    'Jotunheim',
    '[
        {"template": "Salvage_Wreckage_Base", "resource": "Jotun_Tech", "weight": 0.5},
        {"template": "Ore_Vein_Base", "resource": "Ancient_Alloy", "weight": 0.3},
        {"template": "Chemical_Deposit_Base", "resource": "Industrial_Chemical", "weight": 0.2}
    ]',
    '[
        {"template": "Salvage_Wreckage_Base", "resource": "Jotun_Console", "weight": 0.7},
        {"template": "Salvage_Wreckage_Base", "resource": "Hardened_Servomotor", "weight": 0.3}
    ]',
    '[
        {"template": "Ancient_Cache_Base", "resource": "Titanic_Component_Cache", "weight": 0.6},
        {"template": "Salvage_Wreckage_Base", "resource": "Jotun_Data_Archive", "weight": 0.4}
    ]',
    '[{"template": "Salvage_Wreckage_Base", "resource": "Jotun_Data_Archive", "weight": 1.0}]',
    0, 2, 4,
    '["Jotun_Tech", "Jotun_Console", "Jotun_Data_Archive", "Titanic_Component_Cache"]',
    'Titanic industrial ruins. Medium-high spawn density. Advanced tech salvage.'
);

-- ============================================================
-- Notes on Distribution:
-- - Common: 70% spawn probability
-- - Uncommon: 25% spawn probability
-- - Rare: 5% spawn probability
-- - Legendary: <1% spawn probability (special conditions)
-- - Weights within each tier determine selection probability
-- - Spawn density is per room size (Small/Medium/Large)
-- - Unique resources are biome-exclusive
-- ============================================================
