-- ==============================================================================
-- v0.38.16: Narrative Flavor Pack - Data Population
-- ==============================================================================
-- Purpose: Add immersive flavor text for Runic Blight, Potions, and Locations.
-- Themes: Cargo Cult, Horror, Reverence for the Machine.
-- ==============================================================================

-- ============================================================
-- SECTION 1: RUNIC BLIGHT (Radiation)
-- ============================================================
-- Replacing "Radiation" with "Invisible Fire", "Sickness-Light", "Blight-Heat".

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Geiger Counter / Blight Detector
('Tool', 'BlightDetector', 'Cursory', NULL, 'Active', 'A small box that clicks like a dying insect.', 1.0, '["Basic", "Tool"]'),
('Tool', 'BlightDetector', 'Detailed', NULL, 'Active', 'The spirit-box chatters frantically. The invisible fire is strong here. Do not linger.', 1.0, '["Lore", "Warning"]'),
('Tool', 'BlightDetector', 'Expert', NULL, 'Active', 'Pre-Glitch Hazard Meter. It detects the sickness-light that boils the blood. The needle is buried in the red zone.', 1.0, '["Technical", "Warning"]'),

-- Blighted Zone Description
('Environment', 'BlightZone', 'Cursory', NULL, 'Hazardous', 'The air tastes of metal and blood.', 1.0, '["Basic", "Atmosphere"]'),
('Environment', 'BlightZone', 'Detailed', NULL, 'Hazardous', 'Your skin prickles. The invisible fire burns cold here. Shadows seem to detach from the walls.', 1.0, '["Lore", "Horror"]'),
('Environment', 'BlightZone', 'Expert', NULL, 'Hazardous', 'High-Intensity Blight Field. Prolonged exposure will warp the flesh-pattern. The very stones are sick with it.', 1.0, '["Technical", "Warning"]');

-- ============================================================
-- SECTION 2: ALCHEMICAL MENDING (Health Potions)
-- ============================================================
-- Replacing "Health Potion" with "Alchemical Mending", "Blood-Binder", "Life-Draught".

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Small Health Potion
('Consumable', 'SmallMending', 'Cursory', NULL, 'Sealed', 'A vial of red liquid, warm to the touch.', 1.0, '["Basic", "Consumable"]'),
('Consumable', 'SmallMending', 'Detailed', NULL, 'Sealed', 'Blood-Binder draught. It smells of copper and bitter herbs. It knits flesh, but leaves scars.', 1.0, '["Lore", "Consumable"]'),
('Consumable', 'SmallMending', 'Expert', NULL, 'Sealed', 'Pre-Glitch Flesh-Mender. Stops the blood-flow and knits the living weave. Tastes like burning iron.', 1.0, '["Technical", "Consumable"]'),

-- Large Health Potion
('Consumable', 'LargeMending', 'Cursory', NULL, 'Sealed', 'A heavy flask glowing with deep crimson light.', 1.0, '["Basic", "Consumable"]'),
('Consumable', 'LargeMending', 'Detailed', NULL, 'Sealed', 'A Life-Draught of the Old Makers. Even through the glass, you feel its pulse. It promises to deny death its due.', 1.0, '["Lore", "Consumable"]'),
('Consumable', 'LargeMending', 'Expert', NULL, 'Sealed', 'Makers'' Vitality Draught. Capable of restarting the inner-works and fusing broken bone in heartbeats. The shock to the system is severe.', 1.0, '["Technical", "Consumable"]');

-- ============================================================
-- SECTION 3: LOCATION ATMOSPHERE (Ozone & Rot)
-- ============================================================

INSERT INTO Examination_Descriptors (object_category, object_type, detail_level, biome_name, object_state, descriptor_text, weight, tags)
VALUES
-- Industrial Ruins
('Environment', 'Ruins', 'Cursory', NULL, 'Decayed', 'The smell of storm-scorch and ancient rot hangs heavy in the air.', 1.0, '["Basic", "Atmosphere"]'),
('Environment', 'Ruins', 'Detailed', NULL, 'Decayed', 'This place smells of lightning-scorch and wet iron. It is the scent of a machine that died screaming.', 1.0, '["Lore", "Atmosphere"]'),

-- Fungal Caverns
('Environment', 'Caverns', 'Cursory', NULL, 'Damp', 'The air is thick with spores and the sweet stench of decay.', 1.0, '["Basic", "Atmosphere"]'),
('Environment', 'Caverns', 'Detailed', NULL, 'Damp', 'A cloying smell fills your nose—rotting meat and blooming fungus. The silence is broken only by the drip of unseen water.', 1.0, '["Lore", "Atmosphere"]'),

-- Power Plant / Generator Room
('Environment', 'Generator', 'Cursory', NULL, 'Active', 'The air hums. You can taste the lightning on your tongue.', 1.0, '["Basic", "Atmosphere"]'),
('Environment', 'Generator', 'Detailed', NULL, 'Active', 'The smell of storm-scorch is overpowering here—sharp and metallic, like a storm trapped in a bottle. The hairs on your arms stand up.', 1.0, '["Lore", "Atmosphere"]');
