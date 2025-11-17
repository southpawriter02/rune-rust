-- ============================================================
-- v0.38.6: Environmental Combat Modifiers
-- Parent: v0.38 Descriptor Library & Content Database
-- Purpose: Biome-specific combat atmosphere and hazard integration
-- ============================================================

-- ============================================================
-- THE ROOTS - Environmental Combat Modifiers
-- ============================================================

-- The Roots - Reactions
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('The_Roots', 'Reaction',
 'Your strike sends rust flakes cascading from the ceiling.',
 0.25),

('The_Roots', 'Reaction',
 'The clash of combat echoes through corroded halls.',
 0.30),

('The_Roots', 'Reaction',
 'Steam hisses from fractured pipes as you fight.',
 0.25),

('The_Roots', 'Reaction',
 'Your footwork splashes through puddles of stagnant water.',
 0.20),

('The_Roots', 'Reaction',
 'Ancient metal groans in sympathy with the violence.',
 0.20);

-- The Roots - Hazard Integration
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('The_Roots', 'HazardIntegration',
 'Your dodge sends you stumbling against a corroded pillar.',
 0.15),

('The_Roots', 'HazardIntegration',
 'The enemy''s miss crashes into a rusted support beam, which groans ominously.',
 0.20),

('The_Roots', 'HazardIntegration',
 'Sparks from the clash ignite a patch of leaked oil!',
 0.10),

('The_Roots', 'HazardIntegration',
 'Combat disturbs a nest of rust-rats that scatter in all directions.',
 0.15);

-- ============================================================
-- MUSPELHEIM - Environmental Combat Modifiers
-- ============================================================

-- Muspelheim - Reactions
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('Muspelheim', 'Reaction',
 'The heat distorts the air between you and your enemy.',
 0.30),

('Muspelheim', 'Reaction',
 'Molten droplets spatter from the ceiling as combat rages.',
 0.25),

('Muspelheim', 'Reaction',
 'The clash of weapons sends sparks into pools of magma.',
 0.25),

('Muspelheim', 'Reaction',
 'Flames roar higher as if feeding on the violence.',
 0.30),

('Muspelheim', 'Reaction',
 'Volcanic glass cracks underfoot as you maneuver.',
 0.20);

-- Muspelheim - Hazard Integration
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('Muspelheim', 'HazardIntegration',
 'Your enemy''s corpse tumbles into a lava flow and is consumed.',
 0.15),

('Muspelheim', 'HazardIntegration',
 'You''re forced back toward the blazing heat!',
 0.20),

('Muspelheim', 'HazardIntegration',
 'The enemy uses the flames as cover for its attack!',
 0.18),

('Muspelheim', 'HazardIntegration',
 'A geyser of flame erupts nearby, forcing you to adjust your stance!',
 0.15),

('Muspelheim', 'HazardIntegration',
 'The intense heat saps your strength as the fight continues.',
 0.25);

-- ============================================================
-- NIFLHEIM - Environmental Combat Modifiers
-- ============================================================

-- Niflheim - Reactions
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('Niflheim', 'Reaction',
 'Your breath fogs as you circle your opponent.',
 0.30),

('Niflheim', 'Reaction',
 'Ice cracks beneath your feet as you fight.',
 0.28),

('Niflheim', 'Reaction',
 'Frozen crystals shatter as attacks miss their mark.',
 0.25),

('Niflheim', 'Reaction',
 'The cold makes your movements stiff and uncertain.',
 0.22),

('Niflheim', 'Reaction',
 'Snow swirls around the combatants in a deadly dance.',
 0.25);

-- Niflheim - Hazard Integration
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('Niflheim', 'HazardIntegration',
 'Your dodge sends you sliding on ice!',
 0.25),

('Niflheim', 'HazardIntegration',
 'The enemy''s strike shatters an icicle, sending shards flying!',
 0.20),

('Niflheim', 'HazardIntegration',
 'You use a frozen pillar for cover!',
 0.18),

('Niflheim', 'HazardIntegration',
 'The freezing air makes your weapon hand numb.',
 0.20),

('Niflheim', 'HazardIntegration',
 'A sudden blizzard obscures your vision mid-combat!',
 0.15);

-- ============================================================
-- ALFHEIM - Environmental Combat Modifiers
-- ============================================================

-- Alfheim - Reactions
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('Alfheim', 'Reaction',
 'The Cursed Choir''s shriek intensifies as violence erupts.',
 0.35),

('Alfheim', 'Reaction',
 'Reality flickers with each exchange of blows.',
 0.32),

('Alfheim', 'Reaction',
 'Crystalline structures pulse in rhythm with the combat.',
 0.28),

('Alfheim', 'Reaction',
 'You''re not sure if you''re fighting or if the fight is fighting itself.',
 0.25),

('Alfheim', 'Reaction',
 'Paradox energy crackles around each strike.',
 0.30);

-- Alfheim - Hazard Integration
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('Alfheim', 'HazardIntegration',
 'A Reality Tear opens where your strike misses!',
 0.20),

('Alfheim', 'HazardIntegration',
 'The enemy phases through a crystalline formation!',
 0.22),

('Alfheim', 'HazardIntegration',
 'Your attack creates echoes—are those past attempts or future ones?',
 0.25),

('Alfheim', 'HazardIntegration',
 'The Cursed Choir''s wail disrupts your concentration!',
 0.28),

('Alfheim', 'HazardIntegration',
 'Space itself warps, making distance meaningless!',
 0.18);

-- ============================================================
-- JÖTUNHEIM - Environmental Combat Modifiers
-- ============================================================

-- Jötunheim - Reactions
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('Jötunheim', 'Reaction',
 'The vast chamber amplifies every sound of combat.',
 0.30),

('Jötunheim', 'Reaction',
 'Machinery groans in sympathy with the violence.',
 0.25),

('Jötunheim', 'Reaction',
 'Shadows stretch impossibly long in the industrial gloom.',
 0.28),

('Jötunheim', 'Reaction',
 'Your fight seems insignificant in this titanic space.',
 0.22),

('Jötunheim', 'Reaction',
 'Ancient gears rumble as if disturbed by the combat.',
 0.25);

-- Jötunheim - Hazard Integration
INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
VALUES
('Jötunheim', 'HazardIntegration',
 'A stray strike activates dormant machinery!',
 0.18),

('Jötunheim', 'HazardIntegration',
 'The enemy uses massive support pillars for cover!',
 0.22),

('Jötunheim', 'HazardIntegration',
 'You''re forced to fight around enormous, ancient equipment!',
 0.20),

('Jötunheim', 'HazardIntegration',
 'A massive construct looms over the battle, indifferent.',
 0.15),

('Jötunheim', 'HazardIntegration',
 'Runic machinery flares to life, complicating the fight!',
 0.20);

-- ============================================================
-- Notes:
-- - Total environmental modifiers: 50+ across 5 biomes
-- - Trigger chances balanced for atmosphere without overwhelming combat text
-- - Reactions: General atmospheric flavor (20-35% chance)
-- - Hazard Integration: Tactical elements (10-25% chance)
-- - Biomes have distinct combat personalities
-- ============================================================
