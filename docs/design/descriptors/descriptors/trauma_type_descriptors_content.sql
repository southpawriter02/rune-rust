-- =====================================================
-- v0.38.14: Trauma Type Descriptors Content
-- =====================================================
-- Populates Trauma_Descriptors with acquisition and manifestation descriptions
-- Covers 10 trauma types with 30+ total descriptors
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- COMBAT TRAUMAS
-- =====================================================

-- =================
-- FLASHBACKS
-- =================

-- Acquisition: Breaking Point
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Flashbacks_Acquisition_01', 'flashbacks', '[FLASHBACKS]', 'Acquisition', 'Combat', 'Your mind shatters. Suddenly you''re back there—the moment when you thought you''d die. You see it again. Feel it again. The fear is fresh.', 1, '{"acquisition_source": "near_death"}', 1.0, '["Visceral", "Psychological", "Combat"]'),
('Flashbacks_Acquisition_02', 'flashbacks', '[FLASHBACKS]', 'Acquisition', 'Psychic', 'The psychic pressure breaks something inside you. Every near-death experience you''ve had floods back simultaneously. You''re drowning in memory.', 1, '{"acquisition_source": "psychic_overload"}', 1.0, '["Psychological", "Overwhelming", "Psychic"]'),
('Flashbacks_Acquisition_Complete', 'flashbacks', '[FLASHBACKS]', 'Acquisition', null, '[TRAUMA ACQUIRED: Flashbacks] Combat will never be the same. The past bleeds into the present.', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation: Ongoing Effects
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Flashbacks_Manifestation_01', 'flashbacks', '[FLASHBACKS]', 'Manifestation', 'Combat', 'Mid-combat, a flashback strikes. For a moment, you''re not here—you''re reliving past trauma. [Stunned 1 turn]', 1, '{"effect": "stunned", "duration": 1}', 1.0, '["Debilitating", "Combat", "Random"]'),
('Flashbacks_Manifestation_02', 'flashbacks', '[FLASHBACKS]', 'Manifestation', 'Combat', 'The smell of blood triggers you. Your vision swims with overlaid memories of previous battles.', 1, '{"effect": "disadvantage", "duration": 1}', 0.8, '["Sensory", "Combat", "Disorienting"]'),
('Flashbacks_Manifestation_03', 'flashbacks', '[FLASHBACKS]', 'Manifestation', 'Combat', 'You freeze, trapped in a memory of the last time you almost died here.', 1, '{"effect": "stunned", "duration": 1}', 0.7, '["Paralyzing", "Memory", "Combat"]'),
('Flashbacks_Manifestation_Progression_2', 'flashbacks', '[FLASHBACKS]', 'Manifestation', 'Combat', 'The flashback is more vivid now. You can''t tell what''s real. Was that blow from now or then? [Stunned 2 turns]', 2, '{"effect": "stunned", "duration": 2}', 1.0, '["Severe", "Disorienting", "Combat"]'),
('Flashbacks_Manifestation_Progression_3', 'flashbacks', '[FLASHBACKS]', 'Manifestation', 'Combat', 'Reality fractures. You''re in three battles at once—past, present, and nightmare. You can''t fight what isn''t there. [Stunned 3 turns]', 3, '{"effect": "stunned", "duration": 3}', 1.0, '["Critical", "Reality_Break", "Combat"]');

-- =================
-- BATTLE TREMORS
-- =================

-- Acquisition
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Battle_Tremors_Acquisition_01', 'battle_tremors', '[BATTLE TREMORS]', 'Acquisition', 'Combat', 'Your hands won''t stop shaking. The adrenaline crash is too much. You''ve pushed your body beyond its limits too many times.', 1, '{"acquisition_source": "repeated_combat_stress"}', 1.0, '["Physical", "Nervous_System", "Combat"]'),
('Battle_Tremors_Acquisition_Complete', 'battle_tremors', '[BATTLE TREMORS]', 'Acquisition', null, '[TRAUMA ACQUIRED: Battle Tremors] Your nervous system is damaged. Fine motor control is compromised.', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Battle_Tremors_Manifestation_01', 'battle_tremors', '[BATTLE TREMORS]', 'Manifestation', 'Combat', 'Your hands shake as you try to strike. The blow is less precise than intended. [-1 to attack rolls]', 1, '{"effect": "attack_penalty", "value": -1}', 1.0, '["Mechanical", "Attack", "Constant"]'),
('Battle_Tremors_Manifestation_02', 'battle_tremors', '[BATTLE TREMORS]', 'Manifestation', 'Combat', 'You fumble your weapon momentarily as tremors seize your hands.', 1, '{"effect": "fumble_risk"}', 0.6, '["Physical", "Unreliable", "Combat"]'),
('Battle_Tremors_Manifestation_03', 'battle_tremors', '[BATTLE TREMORS]', 'Manifestation', 'Combat', 'The shaking makes it hard to aim. Your shot goes wide.', 1, '{"effect": "attack_penalty", "value": -1}', 0.8, '["Precision", "Ranged", "Combat"]'),
('Battle_Tremors_Manifestation_Progression_2', 'battle_tremors', '[BATTLE TREMORS]', 'Manifestation', 'Combat', 'Your tremors worsen. You can barely hold your weapon steady. [-2 to attack rolls]', 2, '{"effect": "attack_penalty", "value": -2}', 1.0, '["Severe", "Attack", "Constant"]'),
('Battle_Tremors_Manifestation_Progression_3', 'battle_tremors', '[BATTLE TREMORS]', 'Manifestation', 'Combat', 'Your hands are useless. The tremors make precision impossible. Every strike is compromised. [-3 to attack rolls, critical fumble on 1-3]', 3, '{"effect": "attack_penalty", "value": -3, "fumble_range": "1-3"}', 1.0, '["Critical", "Attack", "Debilitating"]');

-- =================
-- HYPERVIGILANCE
-- =================

-- Acquisition
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Hypervigilance_Acquisition_01', 'hypervigilance', '[HYPERVIGILANCE]', 'Acquisition', 'Combat', 'You can never relax again. Every shadow might hide an enemy. Every sound could be a threat. Sleep becomes impossible.', 1, '{"acquisition_source": "constant_threat"}', 1.0, '["Paranoia", "Exhausting", "Sleep"]'),
('Hypervigilance_Acquisition_Complete', 'hypervigilance', '[HYPERVIGILANCE]', 'Acquisition', null, '[TRAUMA ACQUIRED: Hypervigilance] You''re always on edge, always ready. It''s exhausting.', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Hypervigilance_Manifestation_01', 'hypervigilance', '[HYPERVIGILANCE]', 'Manifestation', 'Environmental', 'You spin at a noise that wasn''t there. Your paranoia is exhausting.', 1, '{"effect": "false_positive"}', 0.8, '["Paranoid", "Exhausting", "Environmental"]'),
('Hypervigilance_Manifestation_02', 'hypervigilance', '[HYPERVIGILANCE]', 'Manifestation', 'Combat', 'You can''t focus. You''re watching every angle, anticipating threats that don''t exist.', 1, '{"effect": "will_penalty", "value": -1}', 1.0, '["Focus", "Combat", "Distracted"]'),
('Hypervigilance_Manifestation_03', 'hypervigilance', '[HYPERVIGILANCE]', 'Manifestation', 'Rest', 'Sleep is impossible. Every time you close your eyes, you''re certain something approaches. [Cannot rest properly]', 1, '{"effect": "rest_penalty", "effectiveness": 0.5}', 1.0, '["Sleep", "Rest", "Exhausting"]'),
('Hypervigilance_Manifestation_Progression_2', 'hypervigilance', '[HYPERVIGILANCE]', 'Manifestation', 'Environmental', 'Exhaustion weighs on you. You haven''t slept properly in days. Everything is a threat. [-2 to WILL checks]', 2, '{"effect": "will_penalty", "value": -2}', 1.0, '["Severe", "Exhaustion", "WILL"]'),
('Hypervigilance_Manifestation_Progression_3', 'hypervigilance', '[HYPERVIGILANCE]', 'Manifestation', 'Environmental', 'You''re on the edge of collapse. Days without real sleep. Your mind manufactures threats from nothing. [-3 to WILL, disadvantage on Perception]', 3, '{"effect": "will_penalty", "value": -3, "perception_disadvantage": true}', 1.0, '["Critical", "Collapse", "Exhaustion"]');

-- =====================================================
-- BLIGHT-RELATED TRAUMAS
-- =====================================================

-- =================
-- PARADOXICAL PARANOIA
-- =================

-- Acquisition
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Paradoxical_Paranoia_Acquisition_01', 'paradoxical_paranoia', '[PARADOXICAL PARANOIA]', 'Acquisition', 'Blight', 'The Blight''s corruption takes root in your mind. You start seeing patterns that shouldn''t exist. Connections that aren''t there. Or are they?', 1, '{"acquisition_source": "blight_corruption"}', 1.0, '["Blight", "Corruption", "Reality"]'),
('Paradoxical_Paranoia_Acquisition_Complete', 'paradoxical_paranoia', '[PARADOXICAL PARANOIA]', 'Acquisition', null, '[TRAUMA ACQUIRED: Paradoxical Paranoia] You can no longer trust your own perceptions.', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Paradoxical_Paranoia_Manifestation_01', 'paradoxical_paranoia', '[PARADOXICAL PARANOIA]', 'Manifestation', 'Environmental', 'Is that person following you, or have they always been ahead of you? Time feels wrong.', 1, '{"effect": "perception_disadvantage"}', 0.9, '["Temporal", "Disorienting", "Blight"]'),
('Paradoxical_Paranoia_Manifestation_02', 'paradoxical_paranoia', '[PARADOXICAL PARANOIA]', 'Manifestation', 'Environmental', 'You''re certain the corridor you just walked through doesn''t exist anymore. You''re not sure you exist anymore.', 1, '{"effect": "reality_doubt"}', 0.7, '["Existential", "Reality", "Blight"]'),
('Paradoxical_Paranoia_Manifestation_03', 'paradoxical_paranoia', '[PARADOXICAL PARANOIA]', 'Manifestation', 'Environmental', 'The walls are breathing. No—you''re breathing. No—the walls are breathing with you. This is wrong.', 1, '{"effect": "false_perception"}', 0.8, '["Sensory", "Disturbing", "Blight"]'),
('Paradoxical_Paranoia_Manifestation_Progression_2', 'paradoxical_paranoia', '[PARADOXICAL PARANOIA]', 'Manifestation', 'Combat', 'Which enemy is real? You can''t tell. They all look simultaneously present and absent. [Disadvantage on Perception, 20% false threat detection]', 2, '{"effect": "perception_disadvantage", "false_positive_chance": 0.2}', 1.0, '["Severe", "Combat", "Perception"]');

-- =================
-- AUDITORY HALLUCINATIONS
-- =================

-- Acquisition
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Auditory_Hallucinations_Acquisition_01', 'auditory_hallucinations', '[AUDITORY HALLUCINATIONS]', 'Acquisition', 'Blight', 'The Cursed Choir won''t leave your mind. Even when you''re far from Alfheim, you hear them singing. Always singing.', 1, '{"acquisition_source": "cursed_choir"}', 1.0, '["Blight", "Alfheim", "Choir"]'),
('Auditory_Hallucinations_Acquisition_Complete', 'auditory_hallucinations', '[AUDITORY HALLUCINATIONS]', 'Acquisition', null, '[TRAUMA ACQUIRED: Auditory Hallucinations] The Choir follows you now.', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Auditory_Hallucinations_Manifestation_01', 'auditory_hallucinations', '[AUDITORY HALLUCINATIONS]', 'Manifestation', 'Environmental', 'You hear voices that aren''t there. They whisper things you don''t want to hear.', 1, '{"effect": "concentration_penalty", "value": -2}', 0.9, '["Auditory", "Disturbing", "Whispers"]'),
('Auditory_Hallucinations_Manifestation_02', 'auditory_hallucinations', '[AUDITORY HALLUCINATIONS]', 'Manifestation', 'Environmental', 'The Choir swells in your mind. You shake your head violently, trying to clear it.', 1, '{"effect": "concentration_penalty", "value": -2}', 0.8, '["Choir", "Overwhelming", "Alfheim"]'),
('Auditory_Hallucinations_Manifestation_03', 'auditory_hallucinations', '[AUDITORY HALLUCINATIONS]', 'Manifestation', 'Environmental', 'Someone calls your name. You turn. No one''s there. There''s never anyone there.', 1, '{"effect": "false_positive"}', 0.7, '["Isolation", "Unnerving", "Name"]'),
('Auditory_Hallucinations_Manifestation_04', 'auditory_hallucinations', '[AUDITORY HALLUCINATIONS]', 'Manifestation', 'Combat', 'Kill them. They''re corrupted. You''re corrupted. Everything is corruption. The voice won''t stop.', 1, '{"effect": "hostile_whispers", "stress": 3}', 0.5, '["Hostile", "Corruption", "Violent"]'),
('Auditory_Hallucinations_Manifestation_Progression_2', 'auditory_hallucinations', '[AUDITORY HALLUCINATIONS]', 'Manifestation', 'Combat', 'The voices are screaming now. You can''t hear your allies over the Choir. [Risk of misidentifying threat locations, -3 to concentration]', 2, '{"effect": "concentration_penalty", "value": -3, "threat_confusion": true}', 1.0, '["Severe", "Disorienting", "Combat"]');

-- =================
-- REALITY DISSOCIATION
-- =================

-- Acquisition
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Reality_Dissociation_Acquisition_01', 'reality_dissociation', '[REALITY DISSOCIATION]', 'Acquisition', 'Blight', 'The barrier between real and unreal dissolves. You''re no longer sure which side you''re on.', 1, '{"acquisition_source": "reality_break"}', 1.0, '["Blight", "Reality", "Existential"]'),
('Reality_Dissociation_Acquisition_Complete', 'reality_dissociation', '[REALITY DISSOCIATION]', 'Acquisition', null, '[TRAUMA ACQUIRED: Reality Dissociation] Is this real? Are you?', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Reality_Dissociation_Manifestation_01', 'reality_dissociation', '[REALITY DISSOCIATION]', 'Manifestation', 'Environmental', 'For a moment, you''re not convinced you''re real. Maybe you''re someone else''s hallucination.', 1, '{"effect": "ap_loss", "value": 1, "chance": 0.2}', 0.8, '["Existential", "AP", "Reality"]'),
('Reality_Dissociation_Manifestation_02', 'reality_dissociation', '[REALITY DISSOCIATION]', 'Manifestation', 'Environmental', 'The room flickers. Is this the Blight, or is this you? You don''t know anymore.', 1, '{"effect": "reality_doubt"}', 0.7, '["Flickering", "Uncertain", "Blight"]'),
('Reality_Dissociation_Manifestation_03', 'reality_dissociation', '[REALITY DISSOCIATION]', 'Manifestation', 'Environmental', 'You look at your hands. Are these your hands? Were they always this shape?', 1, '{"effect": "dissociation"}', 0.6, '["Body", "Dissociation", "Identity"]'),
('Reality_Dissociation_Manifestation_Progression_2', 'reality_dissociation', '[REALITY DISSOCIATION]', 'Manifestation', 'Combat', 'Nothing feels real. Are you fighting? Are they fighting? Is any of this happening? [Random chance to lose 1 AP per turn]', 2, '{"effect": "ap_loss", "value": 1, "chance": 0.35}', 1.0, '["Severe", "AP", "Reality"]');

-- =====================================================
-- SOCIAL TRAUMAS
-- =====================================================

-- =================
-- CORRUPTED SOCIAL SCRIPT
-- =================

-- Acquisition
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Corrupted_Social_Script_Acquisition_01', 'corrupted_social_script', '[CORRUPTED SOCIAL SCRIPT]', 'Acquisition', 'Social', 'Your ability to interact normally is broken. The Blight has corrupted the part of you that understands people.', 1, '{"acquisition_source": "social_corruption"}', 1.0, '["Social", "Blight", "Communication"]'),
('Corrupted_Social_Script_Acquisition_Complete', 'corrupted_social_script', '[CORRUPTED SOCIAL SCRIPT]', 'Acquisition', null, '[TRAUMA ACQUIRED: Corrupted Social Script] People seem alien to you now.', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Corrupted_Social_Script_Manifestation_01', 'corrupted_social_script', '[CORRUPTED SOCIAL SCRIPT]', 'Manifestation', 'Social', 'You say something. Their expression tells you it was wrong. You don''t understand why.', 1, '{"effect": "rhetoric_penalty", "value": -3}', 1.0, '["Communication", "Awkward", "Social"]'),
('Corrupted_Social_Script_Manifestation_02', 'corrupted_social_script', '[CORRUPTED SOCIAL SCRIPT]', 'Manifestation', 'Social', 'They''re talking to you but the words don''t make sense. Not linguistically—conceptually.', 1, '{"effect": "comprehension_failure"}', 0.7, '["Understanding", "Alien", "Social"]'),
('Corrupted_Social_Script_Manifestation_03', 'corrupted_social_script', '[CORRUPTED SOCIAL SCRIPT]', 'Manifestation', 'Social', 'You try to smile. Judging by their reaction, you did it wrong.', 1, '{"effect": "npc_negative_reaction"}', 0.8, '["Expression", "Uncanny", "Social"]'),
('Corrupted_Social_Script_Manifestation_Progression_2', 'corrupted_social_script', '[CORRUPTED SOCIAL SCRIPT]', 'Manifestation', 'Social', 'Every interaction feels like speaking a foreign language. You''re fluent in words but not in meaning. [-5 to Rhetoric, NPCs actively uncomfortable]', 2, '{"effect": "rhetoric_penalty", "value": -5, "npc_discomfort": true}', 1.0, '["Severe", "Alienating", "Social"]');

-- =================
-- TRUST EROSION
-- =================

-- Acquisition
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trust_Erosion_Acquisition_01', 'trust_erosion', '[TRUST EROSION]', 'Acquisition', 'Social', 'You''ve been betrayed too many times. You can''t trust anyone anymore. Not even yourself.', 1, '{"acquisition_source": "betrayal"}', 1.0, '["Betrayal", "Social", "Trust"]'),
('Trust_Erosion_Acquisition_Complete', 'trust_erosion', '[TRUST EROSION]', 'Acquisition', null, '[TRAUMA ACQUIRED: Trust Erosion] Cooperation is almost impossible now.', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Trust_Erosion_Manifestation_01', 'trust_erosion', '[TRUST EROSION]', 'Manifestation', 'Social', 'They offer help. You refuse automatically. You can''t trust them.', 1, '{"effect": "cooperation_penalty"}', 0.9, '["Distrust", "Isolation", "Social"]'),
('Trust_Erosion_Manifestation_02', 'trust_erosion', '[TRUST EROSION]', 'Manifestation', 'Social', 'Are they lying? They''re probably lying. Everyone lies.', 1, '{"effect": "suspicion"}', 0.8, '["Paranoid", "Cynical", "Social"]'),
('Trust_Erosion_Manifestation_03', 'trust_erosion', '[TRUST EROSION]', 'Manifestation', 'Social', 'Your party member reaches for you. You flinch away violently.', 1, '{"effect": "physical_distrust"}', 0.6, '["Physical", "Flinch", "Allies"]'),
('Trust_Erosion_Manifestation_Progression_2', 'trust_erosion', '[TRUST EROSION]', 'Manifestation', 'Social', 'You trust no one. Not allies. Not yourself. Cooperation is nearly impossible. [Disadvantage on team coordination, cannot accept NPC help]', 2, '{"effect": "cooperation_disadvantage", "npc_help_blocked": true}', 1.0, '["Severe", "Isolation", "Cooperation"]');

-- =====================================================
-- EXISTENTIAL TRAUMAS
-- =====================================================

-- =================
-- SYSTEMIC APATHY
-- =================

-- Acquisition
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Systemic_Apathy_Acquisition_01', 'systemic_apathy', '[SYSTEMIC APATHY]', 'Acquisition', 'Existential', 'What''s the point? The world ended 800 years ago. Everything you do is meaningless. Why bother?', 1, '{"acquisition_source": "existential_crisis"}', 1.0, '["Existential", "Nihilism", "Apathy"]'),
('Systemic_Apathy_Acquisition_Complete', 'systemic_apathy', '[SYSTEMIC APATHY]', 'Acquisition', null, '[TRAUMA ACQUIRED: Systemic Apathy] You''ve stopped caring.', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Systemic_Apathy_Manifestation_01', 'systemic_apathy', '[SYSTEMIC APATHY]', 'Manifestation', 'Environmental', 'You move mechanically, going through the motions. Nothing matters.', 1, '{"effect": "initiative_penalty", "value": -2}', 0.9, '["Motivation", "Apathetic", "Initiative"]'),
('Systemic_Apathy_Manifestation_02', 'systemic_apathy', '[SYSTEMIC APATHY]', 'Manifestation', 'Combat', 'They''re in danger. You should care. You don''t.', 1, '{"effect": "motivation_loss"}', 0.7, '["Callous", "Detached", "Allies"]'),
('Systemic_Apathy_Manifestation_03', 'systemic_apathy', '[SYSTEMIC APATHY]', 'Manifestation', 'Environmental', 'Survival? Death? It''s all the same.', 1, '{"effect": "quest_motivation_penalty"}', 0.6, '["Nihilism", "Quests", "Meaningless"]'),
('Systemic_Apathy_Manifestation_Progression_2', 'systemic_apathy', '[SYSTEMIC APATHY]', 'Manifestation', 'Environmental', 'Why even try? Nothing you do changes anything. The world is dead. [-3 to initiative, reduced quest motivation]', 2, '{"effect": "initiative_penalty", "value": -3, "quest_penalty": true}', 1.0, '["Severe", "Depression", "Unmotivated"]');

-- =================
-- EXISTENTIAL DREAD
-- =================

-- Acquisition
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Existential_Dread_Acquisition_01', 'existential_dread', '[EXISTENTIAL DREAD]', 'Acquisition', 'Existential', 'The weight of existence crushes you. 800 years of horror. Why does anything continue? Why do you?', 1, '{"acquisition_source": "existential_horror"}', 1.0, '["Existential", "Horror", "Weight"]'),
('Existential_Dread_Acquisition_Complete', 'existential_dread', '[EXISTENTIAL DREAD]', 'Acquisition', null, '[TRAUMA ACQUIRED: Existential Dread] You''ve looked into the abyss.', 1, '{"trauma_acquired": true}', 1.0, '["System", "Permanent"]');

-- Manifestation
INSERT INTO Trauma_Descriptors (descriptor_name, trauma_id, trauma_name, descriptor_type, context_tag, descriptor_text, progression_level, mechanical_context, spawn_weight, tags) VALUES
('Existential_Dread_Manifestation_01', 'existential_dread', '[EXISTENTIAL DREAD]', 'Manifestation', 'Environmental', 'The meaninglessness of it all overwhelms you suddenly. [+3 Psychic Stress]', 1, '{"effect": "stress_gain", "value": 3}', 0.8, '["Stress", "Overwhelming", "Existential"]'),
('Existential_Dread_Manifestation_02', 'existential_dread', '[EXISTENTIAL DREAD]', 'Manifestation', 'Environmental', 'Why are you here? Why is anything? The questions consume you.', 1, '{"effect": "stress_gain", "value": 2}', 0.7, '["Questions", "Consuming", "Existential"]'),
('Existential_Dread_Manifestation_03', 'existential_dread', '[EXISTENTIAL DREAD]', 'Manifestation', 'Environmental', 'For a moment, you consider just... stopping. Giving up.', 1, '{"effect": "despair_vulnerability"}', 0.5, '["Despair", "Giving_Up", "Dark"]'),
('Existential_Dread_Manifestation_Progression_2', 'existential_dread', '[EXISTENTIAL DREAD]', 'Manifestation', 'Environmental', 'The weight is crushing. Every moment is agony. Why continue? [Periodic +5 Psychic Stress, vulnerability to despair effects]', 2, '{"effect": "stress_gain", "value": 5, "despair_vulnerable": true}', 1.0, '["Severe", "Crushing", "Despair"]');

COMMIT;

-- =====================================================
-- VERIFICATION
-- =====================================================
-- SELECT COUNT(*) FROM Trauma_Descriptors;
-- Expected: 55+ trauma type descriptors (acquisition + manifestation)
--
-- SELECT trauma_id, descriptor_type, COUNT(*)
-- FROM Trauma_Descriptors
-- GROUP BY trauma_id, descriptor_type;
-- Expected: Each trauma has both Acquisition and Manifestation descriptors
-- =====================================================
