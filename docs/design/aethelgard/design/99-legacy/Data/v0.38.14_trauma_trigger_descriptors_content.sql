-- =====================================================
-- v0.38.14: Trauma Trigger Descriptors Content
-- =====================================================
-- Populates Trauma_Descriptors with contextual trigger descriptions (40+)
-- AND Trauma_Trigger_Conditions with mechanical trigger definitions
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- ENVIRONMENTAL TRIGGERS
-- =====================================================

-- Returning to Trauma Site
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Trauma_Site_01', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Environmental', 'You''re back. Here. Where it happened. The memories flood back. [+5 Psychic Stress]', null, '{"trigger": "trauma_site_return", "stress": 5}', 1.0, '["Memory", "Site", "Stress"]'),
('Trigger_Trauma_Site_02', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Environmental', 'This is where you broke. Your body remembers even if you try to forget.', null, '{"trigger": "trauma_site_return", "stress": 5}', 0.8, '["Body_Memory", "Site", "Visceral"]'),
('Trigger_Similar_Situation_01', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Environmental', 'This is too familiar. Just like before. Your trauma activates. [Disadvantage on next action]', null, '{"trigger": "similar_situation", "effect": "disadvantage"}', 1.0, '["Pattern", "Familiar", "Triggered"]'),
('Trigger_Similar_Situation_02', 'battle_tremors', '[BATTLE TREMORS]', 'Trigger', 'Environmental', 'The parallels to your traumatic experience are undeniable. You''re reliving it.', null, '{"trigger": "similar_situation"}', 0.9, '["Parallel", "Reliving", "Triggered"]');

-- Darkness
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Darkness_01', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Environmental', 'The darkness hides threats. You know it does. Every shadow could be lethal. [+3 Psychic Stress]', null, '{"trigger": "darkness", "stress": 3}', 1.0, '["Darkness", "Shadows", "Paranoia"]'),
('Trigger_Darkness_02', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Environmental', 'You can''t see properly. That means they can''t be seen. The panic sets in.', null, '{"trigger": "darkness", "stress": 2}', 0.8, '["Visibility", "Panic", "Fear"]'),
('Trigger_Darkness_03', 'paradoxical_paranoia', '[PARADOXICAL PARANOIA]', 'Trigger', 'Environmental', 'In the dark, nothing is certain. Are you even still in the same room? Was there always a room?', null, '{"trigger": "darkness", "reality_doubt": true}', 0.7, '["Darkness", "Reality", "Uncertainty"]');

-- Confined Spaces
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Confined_01', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Environmental', 'The walls press in. No room to maneuver. You''re trapped. [+2 Psychic Stress]', null, '{"trigger": "confined_space", "stress": 2}', 1.0, '["Confined", "Claustrophobic", "Trapped"]'),
('Trigger_Confined_02', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Environmental', 'This small space reminds you of when you nearly died. The memory is vivid. Inescapable.', null, '{"trigger": "confined_space", "flashback_risk": 0.3}', 0.8, '["Small_Space", "Memory", "Inescapable"]');

-- Isolation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Isolation_01', 'trust_erosion', '[TRUST EROSION]', 'Trigger', 'Environmental', 'You''re alone. That''s safer, isn''t it? No one to betray you. But the loneliness cuts deep.', null, '{"trigger": "isolation", "stress": 2}', 1.0, '["Alone", "Loneliness", "Safer"]'),
('Trigger_Isolation_02', 'systemic_apathy', '[SYSTEMIC APATHY]', 'Trigger', 'Environmental', 'Alone in the ruins. A fitting metaphor. You and the corpse of civilization, both meaningless.', null, '{"trigger": "isolation", "stress": 3}', 0.8, '["Alone", "Nihilism", "Ruins"]'),
('Trigger_Isolation_03', 'existential_dread', '[EXISTENTIAL DREAD]', 'Trigger', 'Environmental', 'In the silence, the questions return. Why continue? Why exist? The void has no answers.', null, '{"trigger": "isolation", "stress": 4}', 0.7, '["Silence", "Questions", "Void"]');

-- Blight Presence
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Blight_01', 'paradoxical_paranoia', '[PARADOXICAL PARANOIA]', 'Trigger', 'Environmental', 'The Blight is near. You can feel it warping reality. Or are you warping it? You can''t tell.', null, '{"trigger": "blight_presence", "reality_distortion": true}', 1.0, '["Blight", "Reality", "Warping"]'),
('Trigger_Blight_02', 'auditory_hallucinations', '[AUDITORY HALLUCINATIONS]', 'Trigger', 'Environmental', 'The Choir grows louder as the Blight intensifies. They''re calling to you. They want you to join them.', null, '{"trigger": "blight_presence", "stress": 4}', 1.0, '["Choir", "Calling", "Blight"]'),
('Trigger_Blight_03', 'reality_dissociation', '[REALITY DISSOCIATION]', 'Trigger', 'Environmental', 'The Blight makes everything uncertain. What''s real? What''s corruption? The line dissolves.', null, '{"trigger": "blight_presence", "dissociation": true}', 0.9, '["Uncertain", "Corruption", "Dissolving"]');

-- =====================================================
-- COMBAT TRIGGERS
-- =====================================================

-- Similar Enemy Type
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Similar_Enemy_01', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Combat', 'A Forlorn. Just like the one that nearly killed you. Panic seizes you.', null, '{"trigger": "similar_enemy", "enemy_type": "Forlorn", "stress": 5}', 1.0, '["Enemy", "Forlorn", "Panic"]'),
('Trigger_Similar_Enemy_02', 'battle_tremors', '[BATTLE TREMORS]', 'Trigger', 'Combat', 'Your trauma triggers at the sight of the Servitor. Your hands shake. [Battle Tremors activate]', null, '{"trigger": "similar_enemy", "enemy_type": "Servitor"}', 1.0, '["Enemy", "Servitor", "Tremors"]'),
('Trigger_Similar_Enemy_03', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Combat', 'The Choir Echoes. The sound. That horrific sound. You''re back in Alfheim. Trapped in memory.', null, '{"trigger": "similar_enemy", "enemy_type": "Choir_Echo", "stunned": 1}', 1.0, '["Enemy", "Choir", "Memory"]'),
('Trigger_Similar_Enemy_04', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Combat', 'Another ambush predator. Just like before. Your hypervigilance screams warnings.', null, '{"trigger": "similar_enemy", "enemy_trait": "ambusher"}', 0.9, '["Ambush", "Warning", "Predator"]');

-- Low Health
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Low_Health_01', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Combat', 'You''re injured badly. Just like before. The fear is overwhelming. [Flashback triggers]', null, '{"trigger": "low_health", "health_threshold": 0.25, "stunned": 1}', 1.0, '["Injured", "Fear", "Near_Death"]'),
('Trigger_Low_Health_02', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Combat', 'Death is close. Too close. Your past trauma erupts. [Stunned 1 turn]', null, '{"trigger": "low_health", "health_threshold": 0.25, "stunned": 1}', 0.9, '["Near_Death", "Trauma", "Stunned"]'),
('Trigger_Low_Health_03', 'battle_tremors', '[BATTLE TREMORS]', 'Trigger', 'Combat', 'Your injuries trigger your nervous system. The tremors worsen. You can barely hold your weapon.', null, '{"trigger": "low_health", "health_threshold": 0.25, "tremor_intensify": true}', 0.8, '["Injured", "Tremors", "Weapon"]'),
('Trigger_Low_Health_04', 'existential_dread', '[EXISTENTIAL DREAD]', 'Trigger', 'Combat', 'Facing death again. Maybe this time you should just... let go. [+5 Psychic Stress]', null, '{"trigger": "low_health", "health_threshold": 0.25, "stress": 5}', 0.6, '["Death", "Giving_Up", "Stress"]');

-- Being Outnumbered
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Outnumbered_01', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Combat', 'Too many enemies. Too many angles. You can''t watch them all. The panic is paralyzing.', null, '{"trigger": "outnumbered", "enemy_count": 3, "stress": 3}', 1.0, '["Outnumbered", "Panic", "Overwhelmed"]'),
('Trigger_Outnumbered_02', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Combat', 'Surrounded. Just like that time. You barely survived then. Will you survive now?', null, '{"trigger": "outnumbered", "enemy_count": 4, "stress": 4}', 0.9, '["Surrounded", "Survival", "Memory"]'),
('Trigger_Outnumbered_03', 'systemic_apathy', '[SYSTEMIC APATHY]', 'Trigger', 'Combat', 'Outnumbered. Probably going to die. Does it even matter? Everything ends the same way.', null, '{"trigger": "outnumbered", "enemy_count": 3, "initiative_penalty": -2}', 0.7, '["Outnumbered", "Apathy", "Death"]');

-- Ambushed
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Ambush_01', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Combat', 'They came from nowhere. You should have seen them. You should have been ready. You weren''t. [+4 Psychic Stress]', null, '{"trigger": "ambushed", "stress": 4}', 1.0, '["Ambush", "Unready", "Failure"]'),
('Trigger_Ambush_02', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Combat', 'Ambushed again. The memory of the last ambush overwhelms the present. You freeze.', null, '{"trigger": "ambushed", "stunned": 1}', 0.9, '["Ambush", "Freeze", "Memory"]'),
('Trigger_Ambush_03', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Combat', 'You failed to detect the threat. Your vigilance wasn''t enough. It''s never enough.', null, '{"trigger": "ambushed", "stress": 3}', 0.8, '["Ambush", "Failed", "Never_Enough"]');

-- Ally Down
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Ally_Down_01', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Combat', 'Your ally falls. Just like before. You couldn''t save them then either.', null, '{"trigger": "ally_down", "stress": 4}', 1.0, '["Ally", "Failed", "Memory"]'),
('Trigger_Ally_Down_02', 'trust_erosion', '[TRUST EROSION]', 'Trigger', 'Combat', 'They fell. Of course they did. Everyone fails eventually. Everyone leaves.', null, '{"trigger": "ally_down", "stress": 3}', 0.8, '["Ally", "Abandonment", "Failure"]'),
('Trigger_Ally_Down_03', 'systemic_apathy', '[SYSTEMIC APATHY]', 'Trigger', 'Combat', 'Another person down. Another meaningless death in a meaningless world.', null, '{"trigger": "ally_down", "motivation_penalty": true}', 0.6, '["Ally", "Meaningless", "Death"]');

-- Critical Hit Received
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Critical_Hit_01', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Combat', 'The blow lands hard. Too hard. Like that strike that nearly killed you before. [Flashback risk]', null, '{"trigger": "critical_hit", "flashback_chance": 0.4}', 1.0, '["Critical", "Pain", "Memory"]'),
('Trigger_Critical_Hit_02', 'battle_tremors', '[BATTLE TREMORS]', 'Trigger', 'Combat', 'The impact rattles your entire nervous system. Your tremors worsen significantly.', null, '{"trigger": "critical_hit", "tremor_intensify": true}', 0.9, '["Critical", "Nervous_System", "Tremors"]');

-- =====================================================
-- SOCIAL TRIGGERS
-- =====================================================

-- Betrayal/Mistrust
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Betrayal_01', 'trust_erosion', '[TRUST EROSION]', 'Trigger', 'Social', 'They lie. Of course they lie. Everyone lies. [Trust Erosion activates]', null, '{"trigger": "betrayal", "trust_penalty": true}', 1.0, '["Lie", "Betrayal", "Expected"]'),
('Trigger_Betrayal_02', 'trust_erosion', '[TRUST EROSION]', 'Trigger', 'Social', 'Your trauma makes it impossible to believe them, even if they''re honest.', null, '{"trigger": "perceived_deception", "rhetoric_penalty": -2}', 0.9, '["Deception", "Distrust", "Impossible"]'),
('Trigger_Betrayal_03', 'trust_erosion', '[TRUST EROSION]', 'Trigger', 'Social', 'You knew it. You knew they''d betray you. Everyone does eventually.', null, '{"trigger": "betrayal", "stress": 5}', 0.8, '["Betrayal", "Inevitable", "Everyone"]');

-- Crowds
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Crowd_01', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Social', 'Too many people. Too much stimulation. Your trauma overwhelms you. [Anxiety attack]', null, '{"trigger": "crowd", "people_count": 5, "stress": 4}', 1.0, '["Crowd", "Overwhelmed", "Anxiety"]'),
('Trigger_Crowd_02', 'corrupted_social_script', '[CORRUPTED SOCIAL SCRIPT]', 'Trigger', 'Social', 'So many faces. So many incomprehensible expressions. You can''t process them all.', null, '{"trigger": "crowd", "people_count": 4, "social_penalty": true}', 0.9, '["Crowd", "Faces", "Incomprehensible"]'),
('Trigger_Crowd_03', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Social', 'Which one is the threat? They can''t all be safe. Someone here means harm.', null, '{"trigger": "crowd", "people_count": 4, "paranoia": true}', 0.8, '["Crowd", "Threat", "Paranoia"]');

-- Loud Noises
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Loud_Noise_01', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Environmental', 'The sudden noise makes you jump violently. Your heart races. Threat? Where?', null, '{"trigger": "loud_noise", "stress": 2}', 1.0, '["Noise", "Jump", "Startle"]'),
('Trigger_Loud_Noise_02', 'auditory_hallucinations', '[AUDITORY HALLUCINATIONS]', 'Trigger', 'Environmental', 'Was that real or in your head? The noise blends with the Choir. You can''t tell anymore.', null, '{"trigger": "loud_noise", "confusion": true}', 0.8, '["Noise", "Confusion", "Real"]'),
('Trigger_Loud_Noise_03', 'flashbacks', '[FLASHBACKS]', 'Trigger', 'Combat', 'The explosion. The sound triggers a memory. Suddenly you''re back in that moment of terror.', null, '{"trigger": "loud_noise", "flashback_chance": 0.3}', 0.7, '["Explosion", "Sound", "Terror"]');

-- Being Touched Unexpectedly
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trigger_Unexpected_Touch_01', 'trust_erosion', '[TRUST EROSION]', 'Trigger', 'Social', 'They touch your shoulder. You flinch violently, hand moving to your weapon.', null, '{"trigger": "unexpected_touch", "flinch": true}', 1.0, '["Touch", "Flinch", "Weapon"]'),
('Trigger_Unexpected_Touch_02', 'hypervigilance', '[HYPERVIGILANCE]', 'Trigger', 'Social', 'Someone approaches from behind. Your instincts scream danger. You spin, ready to fight.', null, '{"trigger": "unexpected_approach", "stress": 3}', 0.9, '["Approach", "Behind", "Danger"]');

-- =====================================================
-- MECHANICAL TRIGGER CONDITIONS
-- =====================================================
-- Define the conditions that activate these triggers

INSERT INTO Trauma_Trigger_Conditions (trigger_name, trigger_category, trigger_condition, applicable_trauma_ids, thresholds, trigger_description, stress_impact, tags) VALUES
-- Environmental
('Trauma_Site_Return', 'Environmental', 'location_match', '["flashbacks", "battle_tremors", "hypervigilance"]', '{"location_type": "trauma_origin"}', 'When returning to the location where trauma was acquired', '+5 Psychic Stress', '["Memory", "Location"]'),
('Darkness_High', 'Environmental', 'light_level', '["hypervigilance", "paradoxical_paranoia"]', '{"light_level": "Dim"}', 'When in dim or dark areas', '+2-3 Psychic Stress', '["Darkness", "Visibility"]'),
('Confined_Space', 'Environmental', 'room_size', '["hypervigilance", "flashbacks"]', '{"room_size": "Small"}', 'When in small, confined rooms', '+2 Psychic Stress', '["Confined", "Claustrophobic"]'),
('Isolation', 'Environmental', 'alone', '["trust_erosion", "systemic_apathy", "existential_dread"]', '{"ally_count": 0}', 'When separated from allies', '+2-4 Psychic Stress', '["Alone", "Isolated"]'),
('Blight_Presence', 'Environmental', 'blight_corruption', '["paradoxical_paranoia", "auditory_hallucinations", "reality_dissociation"]', '{"blight_intensity": "Medium"}', 'When near Blight manifestations', '+3-5 Psychic Stress', '["Blight", "Corruption"]'),

-- Combat
('Similar_Enemy', 'Combat', 'enemy_type_match', '["flashbacks", "battle_tremors", "hypervigilance"]', '{"enemy_type": "trauma_source"}', 'When encountering enemy type that caused trauma', '+5 Psychic Stress or Stunned', '["Enemy", "Memory"]'),
('Low_Health', 'Combat', 'health_percentage', '["flashbacks", "battle_tremors", "existential_dread"]', '{"health_percent": 25}', 'When health drops below 25%', 'Flashback trigger or +5 Stress', '["Near_Death", "Injured"]'),
('Outnumbered', 'Combat', 'enemy_count', '["hypervigilance", "flashbacks", "systemic_apathy"]', '{"enemy_count_ratio": 2}', 'When facing 2+ enemies per ally', '+3-4 Psychic Stress', '["Outnumbered", "Overwhelmed"]'),
('Ambushed', 'Combat', 'surprise_round', '["hypervigilance", "flashbacks"]', '{"surprise": true}', 'When ambushed by enemies', '+3-4 Psychic Stress or Stunned', '["Ambush", "Surprise"]'),
('Ally_Down', 'Combat', 'ally_unconscious', '["flashbacks", "trust_erosion", "systemic_apathy"]', '{"ally_downed": true}', 'When an ally is knocked unconscious', '+3-4 Psychic Stress', '["Ally", "Failure"]'),
('Critical_Hit', 'Combat', 'damage_critical', '["flashbacks", "battle_tremors"]', '{"critical_hit": true}', 'When receiving a critical hit', 'Flashback risk or tremor intensification', '["Critical", "Pain"]'),

-- Social
('Betrayal', 'Social', 'deception_detected', '["trust_erosion"]', '{"deception": true}', 'When detecting deception or betrayal', '+5 Psychic Stress, trust penalties', '["Betrayal", "Lie"]'),
('Crowd', 'Social', 'npc_count', '["hypervigilance", "corrupted_social_script"]', '{"npc_count": 4}', 'When in groups of 4+ people', '+4 Psychic Stress, social penalties', '["Crowd", "Overwhelmed"]'),
('Loud_Noise', 'Environmental', 'sound_intensity', '["hypervigilance", "auditory_hallucinations", "flashbacks"]', '{"volume": "Loud"}', 'When exposed to sudden loud noises', '+2 Psychic Stress or flashback', '["Noise", "Startle"]'),
('Unexpected_Touch', 'Social', 'physical_contact', '["trust_erosion", "hypervigilance"]', '{"contact": "unexpected"}', 'When touched unexpectedly', 'Flinch response, +3 Stress', '["Touch", "Contact"]');

COMMIT;

-- =====================================================
-- VERIFICATION
-- =====================================================
-- SELECT COUNT(*) FROM Trauma_Descriptors WHERE descriptor_type = 'Trigger';
-- Expected: 40+ trigger descriptors
--
-- SELECT COUNT(*) FROM Trauma_Trigger_Conditions;
-- Expected: 14 trigger condition definitions
-- =====================================================
